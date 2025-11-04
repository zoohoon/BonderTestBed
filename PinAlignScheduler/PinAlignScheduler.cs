using LogModule;
using Newtonsoft.Json;
using ProbeCardObject;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Command.Internal;
using ProberInterfaces.NeedleClean;
using ProberInterfaces.PolishWafer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace PinAlignScheduler
{
    public class PinAlignScheduler : ISchedulingModule, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        private IStateModule _PinAlignModule;
        public IStateModule PinAlignModule
        {
            get { return _PinAlignModule; }
            set
            {
                if (value != _PinAlignModule)
                {
                    _PinAlignModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        public SubModuleStateBase SubModuleState { get; set; }

        public PinAlignScheduler()
        {
            _PinAlignModule = this.PinAligner();
        }
        public PinAlignScheduler(IStateModule Module)
        {
            _PinAlignModule = Module;
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    Initialized = true;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void DeInitModule()
        {
        }

        public bool IsExecute()
        {
            bool waferInterval = false;
            bool dieInterval = false;
            bool timeInterval = false;
            bool PinAlignDone = true;

            DateTime CurTime = DateTime.Now;
            bool retVal = false;

            try
            {
                int ProcessedCount = 0;

                PinAlignDevParameters PinAlignParam = this.PinAligner().PinAlignDevParam as PinAlignDevParameters;

                if (this.LotOPModule().InnerState.GetModuleState() == ModuleStateEnum.RUNNING && this.StageSupervisor().Get_TCW_Mode() == TCW_Mode.OFF)
                {
                    if (this.LotOPModule().ModuleStopFlag || this.PinAligner().WaferTransferRunning)
                    {
                        retVal = false;
                        return retVal;
                    }

                    // 다른 모듈로부터 요청
                    if (PinAlignModule.CommandRecvSlot.IsRequested<IDOPINALIGN>())
                    {
                        if (PinAlignModule.CommandRecvSlot.Token.Sender is INeedleCleanModule)
                        {
                            LoggerManager.Debug($"Pin Align Source (Before) : {this.PinAligner().PinAlignSource}");
                            this.PinAligner().PinAlignSource = PINALIGNSOURCE.NEEDLE_CLEANING;
                            LoggerManager.Debug($"Pin Align Source (After) : {this.PinAligner().PinAlignSource}");
                        }
                        else if (PinAlignModule.CommandRecvSlot.Token.Sender is IPolishWaferModule)
                        {
                            LoggerManager.Debug($"Pin Align Source (Before) : {this.PinAligner().PinAlignSource}");
                            this.PinAligner().PinAlignSource = PINALIGNSOURCE.POLISH_WAFER;
                            LoggerManager.Debug($"Pin Align Source (After) : {this.PinAligner().PinAlignSource}");
                        }
                        else
                        {
                            LoggerManager.Debug($"Unknown request to pin align : {PinAlignModule.CommandRecvSlot.Token.Sender.ToString()}");
                            this.PinAligner().PinAlignSource = PINALIGNSOURCE.WAFER_INTERVAL;
                        }
                    }

                    if (this.SequenceEngineManager().GetRunState())
                    {
                        if (this.StageSupervisor().ProbeCardInfo.GetAlignState() != AlignStateEnum.DONE)
                        {
                            // 얼라인이 되어 있지 않다면 인터벌과 상관없이 얼라인을 수행한다. 기본적으로 웨이퍼 인터벌 파라미터를 따른다.(Status soaking중 sample pin 제외)
                            if (false == PinAlignModule.CommandRecvSlot.IsRequested<IDOSamplePinAlignForSoaking>() &&
                                false == PinAlignModule.CommandRecvSlot.IsRequested<IDOPinAlignAfterSoaking>())
                            {
                                this.PinAligner().PinAlignSource = PINALIGNSOURCE.WAFER_INTERVAL;
                            }

                            PinAlignDone = false;
                        }

                        // this.LotOPModule().LotInfo.ProcessedWaferCount() : 현재 랏드에서 처리된 웨이퍼
                        // this.LotOPModule().LotInfo.UnProcessedWaferCount() : 현재 랏드에서 남은 웨이퍼

                        if (PinAlignParam.PinAlignInterval.UseWaferInterval.Value == WAFERINTERVALMODE.ALWAYS)
                        {
                            if (PinAlignParam.PinAlignInterval.MarkedWaferCountVal == 0)
                            {
                                PinAlignParam.PinAlignInterval.MarkedWaferCountVal = (long)this.LotOPModule().SystemInfo.WaferCount;
                            }

                            // 마지막으로 진행되었던 카운트와 현재 웨이퍼 카운트가 다르면 핀 얼라인 수행
                            if (PinAlignParam.PinAlignInterval.FlagAlignProcessedForThisWafer == false)
                            {
                                waferInterval = true;
                                this.PinAligner().PinAlignSource = PINALIGNSOURCE.WAFER_INTERVAL;
                            }
                        }
                        else if (PinAlignParam.PinAlignInterval.UseWaferInterval.Value == WAFERINTERVALMODE.STATICMODE)
                        {
                            ProcessedCount = this.LotOPModule().LotInfo.ProcessedWaferCount();

                            if (ProcessedCount >= 0 && ProcessedCount <= 24)
                            {
                                if (PinAlignParam.PinAlignInterval.WaferInterval[ProcessedCount].Value == true)
                                {
                                    if (PinAlignParam.PinAlignInterval.FlagAlignProcessedForThisWafer == false)
                                    {
                                        waferInterval = true;
                                        this.PinAligner().PinAlignSource = PINALIGNSOURCE.WAFER_INTERVAL;
                                    }
                                }
                            }
                        }
                        else if (PinAlignParam.PinAlignInterval.UseWaferInterval.Value == WAFERINTERVALMODE.INTERVALMODE && PinAlignParam.PinAlignInterval.WaferIntervalCount.Value > 0)
                        {
                            if (PinAlignParam.PinAlignInterval.MarkedWaferCountVal == 0) PinAlignParam.PinAlignInterval.MarkedWaferCountVal = (long)this.LotOPModule().SystemInfo.WaferCount;

                            // 마지막으로 진행된 웨이퍼 카운트에서 부터 현재까지 몇 장이나 더 진행되었나 확인
                            if (PinAlignParam.PinAlignInterval.WaferIntervalCount.Value <= ((long)this.LotOPModule().SystemInfo.WaferCount - PinAlignParam.PinAlignInterval.MarkedWaferCountVal))
                            {
                                waferInterval = true;
                                this.PinAligner().PinAlignSource = PINALIGNSOURCE.WAFER_INTERVAL;
                            }
                        }

                        if (PinAlignParam.PinAlignInterval.UseTimeInterval.Value == TIMEINTERVALMODE.ENABLE && PinAlignParam.PinAlignInterval.TimeInterval.Value > 0)
                        {
                            TimeSpan GapTime = CurTime - this.PinAligner().LastAlignDoneTime;

                            if (GapTime.TotalMinutes >= PinAlignParam.PinAlignInterval.TimeInterval.Value)
                            {
                                timeInterval = true;
                                this.PinAligner().PinAlignSource = PINALIGNSOURCE.TIME_INTERVAL;
                                //this.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
                                //this.StageSupervisor().ProbeCardInfo.SetPinPadAlignState(AlignStateEnum.IDLE);
                            }
                        }

                        if (PinAlignParam.PinAlignInterval.UseDieInterval.Value == DIEINTERVALMODE.ENABLE && PinAlignParam.PinAlignInterval.DieInterval.Value > 0)
                        {
                            if (PinAlignParam.PinAlignInterval.DieInterval.Value <= ((long)this.LotOPModule().SystemInfo.DieCount - PinAlignParam.PinAlignInterval.MarkedDieCountVal))
                            {
                                dieInterval = true;
                                this.PinAligner().PinAlignSource = PINALIGNSOURCE.DIE_INTERVAL;
                            }
                        }
                    }

                    retVal = (waferInterval || timeInterval || dieInterval) || (!PinAlignDone);

                    // 현재 코드 상, 카드 체인지가 되어, 이벤트가 발생되면, 디바이스 체인지 플래그가 같이 켜짐.
                    // 따라서, 이곳에서 구분해서 소스를 변경하려면, 비교 순서가 아래와 같이, DeviceChange를 먼저 봐야 됨.
                    if (retVal == true)
                    {
                        if (PinAlignParam.PinAlignInterval.FlagAlignProcessedAfterDeviceChange == false)
                        {
                            this.PinAligner().PinAlignSource = PINALIGNSOURCE.DEVICE_CHANGE;
                        }

                        if (PinAlignParam.PinAlignInterval.FlagAlignProcessedAfterCardChange == false)
                        {
                            this.PinAligner().PinAlignSource = PINALIGNSOURCE.CARD_CHANGE;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum ParamValidation()
        {
            return EventCodeEnum.NONE;
        }

        public bool IsParameterChanged(bool issave = false)
        {
            return false;
        }
    }
}

