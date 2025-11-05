using LogModule;
using Newtonsoft.Json;
using ProbeCardObject;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using ProberInterfaces.PinAlign.ProbeCardData;
using ProberInterfaces.State;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Serialization;
using VirtualStageConnector;

namespace PinPadMatchModule
{
    public class PinPadMatchModule : IProcessingModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        public bool Initialized { get; set; } = false;

        public SubModuleMovingStateBase MovingState { get; set; }
        
        private SubModuleStateBase _AlignModuleState;
        public SubModuleStateBase SubModuleState
        {
            get { return _AlignModuleState; }
            set
            {
                if (value != _AlignModuleState)
                {
                    _AlignModuleState = value;
                    RaisePropertyChanged();
                }
            }
        }
        //private IParam _DevParam;
        //[ParamIgnore]
        //public IParam DevParam
        //{
        //    get { return _DevParam; }
        //    set
        //    {
        //        if (value != _DevParam)
        //        {
        //            _DevParam = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }

        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }

        //private List<MachineCoordinate> Pads = new List<MachineCoordinate>();
        //private List<MachineCoordinate> Pins = new List<MachineCoordinate>();
        //private MachineCoordinate PinCenter = null;
        //private MachineCoordinate PadCenter = null;

        public StringBuilder LastResultStringBuilder = null;

        public EventCodeEnum ClearData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
            bool isDataNotMatched = false;
            int pincnt = 0;
            try
            {
                IList<DUTPadObject> pads = this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos;

                foreach (IDut dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                {
                    pincnt += dut.PinList.Count;
                }

                if(pincnt != pads.Count)
                {
                    isDataNotMatched = true;
                }
                else
                {
                    foreach (IDut dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                    {
                        foreach (IPinData pin in dut.PinList)
                        {
                            if (pads.FirstOrDefault(die =>  (pin.PinNum.Value == die.PadNumber.Value) && (pin.DutNumber.Value == die.DutNumber)) != null)
                            {
                                isDataNotMatched = false;                                
                            }
                            else
                            {
                                isDataNotMatched = true;
                                 break;
                            }
                        }
                    }
                }
                if(isDataNotMatched)
                {
                    retVal = EventCodeEnum.NODATA;
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                }
                //Gordon Test Code
                retVal = EventCodeEnum.NONE;
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
            }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }

        public void ClearState()
        {
            return;
        }

        public void DeInitModule()
        {
            return;
        }

        public EventCodeEnum DoClearData()
        {
            return EventCodeEnum.UNDEFINED;
        }

        public EventCodeEnum DoExecute()
        { 
            try
            {
            //Gordon Test Code
            //this.StageSupervisor().ProbeCardInfo.SetPinPadAlignState(AlignStateEnum.DONE);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return EventCodeEnum.NONE;//DoPinPadMatch();
        }

        public EventCodeEnum DoExitRecovery()
        {
            return EventCodeEnum.UNDEFINED;
        }

        public EventCodeEnum DoRecovery()
        {
            return EventCodeEnum.UNDEFINED;
        }

        public EventCodeEnum Execute()
        {
            return EventCodeEnum.UNDEFINED;
        }

        public EventCodeEnum ExitRecovery()
        {
            return EventCodeEnum.UNDEFINED;
        }

        public MovingStateEnum GetMovingState()
        {
            return MovingState.GetState();
        }

        public SubModuleStateEnum GetState()
        {
            return _AlignModuleState.GetState();
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {

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

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retval;
        }

        public bool IsExecute()
        {
            bool isExecute = false;
            try
            {
            if(this.StageSupervisor().ProbeCardInfo.GetAlignState() == AlignStateEnum.DONE)
            {
                isExecute = true;
            }
            else
            {
                isExecute = false;
            }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return isExecute;
        }

        public bool IsParameterChanged(bool issave = false)
        {
            return false;
        }

        //public EventCodeEnum LoadDevParameter()
        //{
        //    return EventCodeEnum.NONE;
        //}

        public EventCodeEnum ParamValidation()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum Recovery()
        {
            return EventCodeEnum.NONE;
        }

        //public EventCodeEnum SaveDevParameter()
        //{
        //    return EventCodeEnum.NONE;
        //}
        public EventCodeEnum DoPinPadMatch()
        {
            bool useBestFitted = false;
            EventCodeEnum eventCodeEnum = EventCodeEnum.UNDEFINED;
            try
            {
                useBestFitted = this.ProbingModule().GetEnablePTPAEnhancer();
                if (useBestFitted == true)
                {
                    LoggerManager.Debug($"PinPadMatchModule(): Use best fitted model.");
                    eventCodeEnum = DoPinPadMatchBestFitted();
                }
                else
                {
                    LoggerManager.Debug($"PinPadMatchModule(): Use averaging model.");
                    eventCodeEnum = DoPinPadMatchAvg();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"DoPinPadMatch(): Error occurred. Err = {err.Message}");
                eventCodeEnum = DoPinPadMatchAvg();
            }
            return eventCodeEnum;
        }
        public EventCodeEnum DoPinPadMatchAvg()
        {
            List<MachineCoordinate> DiffPadToPadCen0 = new List<MachineCoordinate>();
            List<MachineCoordinate> NewPadPos = new List<MachineCoordinate>();
            List<MachineCoordinate> DiffPinToPinCen0 = new List<MachineCoordinate>();
            List<MachineCoordinate> DiffPinToPinCen1 = new List<MachineCoordinate>();

            MachineCoordinate tampcoord = new MachineCoordinate();
            PinCoordinate tampcoord2 = new PinCoordinate();

            double PadAngle = 0;
            double PinAngle = 0;
            double Drad = 0;
            double PadLen = 0;
            double SumDiff = 0;
            double SumLen = 0;
            double DiffAngle = 0;
            double dX = 0;
            double dY = 0;
            double dxSum = 0;
            double dySum = 0;

            MachineCoordinate PinCenter = new MachineCoordinate();
            MachineCoordinate PadCenter = new MachineCoordinate();
            MachineCoordinate PadDist = new MachineCoordinate();
            MachineCoordinate PinDist = new MachineCoordinate();
            List<IPinData> CurPinList = new List<IPinData>();
            IPinData Cur_Pin;
            WaferCoordinate PadPos = new WaferCoordinate();
            double pinPosX = 0;
            double pinPosY = 0;
            double padPosX = 0;
            double padPosY = 0;
            //int idx;
            bool bError = false;

            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            var PinAlignParam = (this.PinAligner().PinAlignDevParam as PinAlignDevParameters);
            List<string> out_of_tolerence_list = new List<string>();

            try
            {
                if (this.StageSupervisor().ProbeCardInfo.GetPinCount() <= 0)
                {
                    this.NotifyManager().Notify(EventCodeEnum.PIN_NOT_ENOUGH);
                    return EventCodeEnum.PIN_NOT_ENOUGH;
                }

                this.WaferAligner().UpdatePadCen();

                PinCenter.X.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenX;
                PinCenter.Y.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenY;

                PadCenter.X.Value = this.StageSupervisor().WaferObject.GetSubsInfo().Pads.PadCenX;
                PadCenter.Y.Value = this.StageSupervisor().WaferObject.GetSubsInfo().Pads.PadCenY;

                SumDiff = 0;

                DiffPadToPadCen0.Clear();
                DiffPinToPinCen0.Clear();

                for (int i = 0; i <= this.StageSupervisor().ProbeCardInfo.GetPinCount() - 1; i++)
                {
                    Cur_Pin = this.StageSupervisor().ProbeCardInfo.GetPin(i);

                    if (Cur_Pin != null)
                    {
                        var tmpPad = this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos.FirstOrDefault(pad =>
                        (pad.PadNumber.Value == Cur_Pin.PinNum.Value) );

                        //var tmpPad = this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos.FirstOrDefault(pad =>
                        //((int)pad.DutMIndex.XIndex == Cur_Pin.DutMacIndex.Value.XIndex) &&
                        //((int)pad.DutMIndex.YIndex == Cur_Pin.DutMacIndex.Value.YIndex));


                        var tmpDut = this.StageSupervisor().GetParam_ProbeCard().GetDutFromPinNum(Cur_Pin.PinNum.Value);
                        IDut firstdut = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[0];

                        // TODO: FLIP 검토 할것
                        //if(this.VisionManager().DispHorFlip == DispFlipEnum.FLIP && this.VisionManager().DispVerFlip == DispFlipEnum.FLIP)
                        //{
                        //    firstdut = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.FirstOrDefault(item =>
                        //               item.MacIndex.XIndex == this.StageSupervisor().WaferObject.GetPhysInfo().MapCountX.Value - 1 - firstdut.MacIndex.XIndex
                        //               && item.MacIndex.YIndex == this.StageSupervisor().WaferObject.GetPhysInfo().MapCountY.Value - 1 - firstdut.MacIndex.YIndex);
                        //}

                        if (tmpPad != null && tmpDut != null)
                        {
                            pinPosX = Cur_Pin.AbsPos.X.Value;
                            pinPosY = Cur_Pin.AbsPos.Y.Value;
                            LoggerManager.Debug($"INDEX {i}: ({pinPosX}, {pinPosY})");
                            padPosX = ((tmpDut.MacIndex.XIndex - firstdut.MacIndex.XIndex)
                                        * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Width.Value) 
                                        + tmpPad.PadCenter.X.Value;
                            padPosY = ((tmpDut.MacIndex.YIndex - firstdut.MacIndex.YIndex)
                                        * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Height.Value) +
                                        tmpPad.PadCenter.Y.Value;
                            LoggerManager.Debug($"INDEX {i}: ({padPosX}, {padPosY})");
                            //padPosX = this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos[idx].MIndexLCWaferCoord.X.Value + this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos[idx].PadCenter.X.Value;
                            //padPosY = this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos[idx].MIndexLCWaferCoord.Y.Value + this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos[idx].PadCenter.Y.Value;

                            //LoggerManager.Debug($"{pinPosX} {pinPosY}");

                            PadDist = new MachineCoordinate();

                            PadDist.X.Value = padPosX - PadCenter.GetX();
                            PadDist.Y.Value = padPosY - PadCenter.GetY();

                            DiffPadToPadCen0.Add(PadDist);
                            
                            PinDist = new MachineCoordinate();

                            PinDist.X.Value = pinPosX - PinCenter.GetX();
                            PinDist.Y.Value = pinPosY - PinCenter.GetY();

                            DiffPinToPinCen0.Add(PinDist);
                            
                            PadAngle = Math.Atan2(PadDist.GetY(), PadDist.GetX());
                            PinAngle = Math.Atan2(PinDist.GetY(), PinDist.GetX());

                            Drad = PinAngle - PadAngle;
                            //PinAngleSum += PinAngle;
                            if (RadianToDegree(Drad) < -300)
                            {
                                Drad = Drad + 2 * Math.PI;
                            }
                            else if (RadianToDegree(Drad) > 300)
                            {
                                Drad = Drad - 2 * Math.PI;
                            }

                            PadLen = GetDistance2D(new MachineCoordinate(0, 0, 0), DiffPadToPadCen0[i]);

                            SumDiff = SumDiff + Drad * PadLen;
                            SumLen = SumLen + PadLen;
                        }
                        else
                        {
                            // 핀과 매칭되는 패드가 없음. 에러.
                            retval = EventCodeEnum.PIN_PAD_MATCH_FAIL;
                            return retval;
                        }
                    }
                    else
                    {
                        // 핀 번호가 이상함
                        retval = EventCodeEnum.PIN_INVAILD_STATUS;
                        return retval;
                    }
                }

                if (SumLen == 0)
                    DiffAngle = 0;
                else
                    DiffAngle = RadianToDegree(SumDiff / SumLen);

                if (DiffAngle > PinAlignParam.PinPadMatchParam.PinPadMatchToleranceAngle.Value)
                {
                    LoggerManager.Debug($"PTPA : Optimize angle is over tolerance. Current angle = {DiffAngle}, Tolerance = {PinAlignParam.PinPadMatchParam.PinPadMatchToleranceAngle.Value})");

                    retval = EventCodeEnum.PIN_OVER_ANGLE_TOLERANCE;
                    return retval;
                }
                else
                {
                    LoggerManager.PinLog($"Pin/Pad optimized angle = {DiffAngle}");
                }

                bError = false;

                LoggerManager.PinLog($"PTPA : Deviation = ({PinAlignParam.PinPadMatchParam.PinPadMatchToleranceX.Value}, {PinAlignParam.PinPadMatchParam.PinPadMatchToleranceY.Value})");

                for (int i = 0; i <= this.StageSupervisor().ProbeCardInfo.GetPinCount() - 1; i++)
                {
                    Cur_Pin = this.StageSupervisor().ProbeCardInfo.GetPin(i);
                    if (Cur_Pin != null)
                    {
                        var tmpPad = this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos.FirstOrDefault(pad =>
                        (pad.PadNumber.Value == Cur_Pin.PinNum.Value));

                        //var tmpPad = this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos.FirstOrDefault(pad => 
                        //((int)pad.DutMIndex.XIndex == Cur_Pin.DutMacIndex.Value.XIndex)&&
                        //((int)pad.DutMIndex.YIndex == Cur_Pin.DutMacIndex.Value.YIndex));

                        if (tmpPad != null)
                        {
                            padPosX = DiffPadToPadCen0[i].X.Value;
                            padPosY = DiffPadToPadCen0[i].Y.Value;

                            PadPos = new WaferCoordinate();
                            tampcoord = new MachineCoordinate();

                            PadPos.X.Value = padPosX;
                            PadPos.Y.Value = padPosY;
                            //LoggerManager.Debug($"TESTpin #{Cur_Pin.PinNum}:  {DiffPinToPinCen0[i].X.Value} - {tampcoord.X.Value} ");
                            GetRotCoord(ref tampcoord, PadPos, DiffAngle);       // 현재 패드를 매칭 각도만큼 돌려서 변경된 위치를 구한다.
                            //LoggerManager.Debug($"rotated offset = #{Cur_Pin}  {tampcoord.X.Value - PadPos.X.Value}, {tampcoord.Y.Value - PadPos.Y.Value}");

                            dX = Math.Abs(DiffPinToPinCen0[i].X.Value - tampcoord.X.Value);
                            dY = Math.Abs(DiffPinToPinCen0[i].Y.Value - tampcoord.Y.Value);

                            string description = string.Empty;

                            if (dX > PinAlignParam.PinPadMatchParam.PinPadMatchToleranceX.Value || dY > PinAlignParam.PinPadMatchParam.PinPadMatchToleranceY.Value)
                            {
                                description = $"PTPA : [F] pin #{Cur_Pin.PinNum} matching error = ({DiffPinToPinCen0[i].X.Value - tampcoord.X.Value}, " +
                                                    $"{DiffPinToPinCen0[i].Y.Value - tampcoord.Y.Value})";
                                //LoggerManager.Debug($"TESTpin #{Cur_Pin.PinNum}:  {DiffPinToPinCen0[i].X.Value} - {tampcoord.X.Value} ");
                                //LoggerManager.Debug($"TESTpin #{Cur_Pin.PinNum}:  {DiffPinToPinCen0[i].Y.Value} - {tampcoord.Y.Value} ");
                                LoggerManager.PinLog(description);

                                bError = true;
                            }
                            else
                            {
                                description = $"PTPA : [P] pin #{Cur_Pin.PinNum} matching result = ({DiffPinToPinCen0[i].X.Value - tampcoord.X.Value}, " +
                                                    $"{DiffPinToPinCen0[i].Y.Value - tampcoord.Y.Value})";
                                //LoggerManager.Debug($"TESTpin #{Cur_Pin.PinNum}:  {DiffPinToPinCen0[i].X.Value} - {tampcoord.X.Value} ");
                                //LoggerManager.Debug($"TESTpin #{Cur_Pin.PinNum}:  {DiffPinToPinCen0[i].Y.Value} - {tampcoord.Y.Value} ");
                                LoggerManager.PinLog(description);
                            }

                            out_of_tolerence_list.Add(description);

                            dxSum += dX;
                            dySum += dY;

                        }
                        else
                        {
                            retval = EventCodeEnum.PIN_PAD_MATCH_FAIL;
                            return retval;
                        }
                    }
                    else
                    {
                        retval = EventCodeEnum.PIN_INVAILD_STATUS;
                        return retval;
                    }
                }

                if (bError == true)
                {
                    LoggerManager.PinLog("PTPA : FAILED");

                    retval = EventCodeEnum.PTPA_OUT_OF_TOLERENCE;
                    return retval;
                }
                else
                {
                    this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DiffAngle = DiffAngle * 1;
                    this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DiffX = -dxSum / this.StageSupervisor().ProbeCardInfo.GetPinCount();
                    this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DiffY = -dySum / this.StageSupervisor().ProbeCardInfo.GetPinCount();

                    double calX = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DiffX;
                    double calY = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DiffY;
                    double calT = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DiffAngle;

                    VirtualStageConnector.VirtualStageConnector.Instance.SendTCPCommand(TCPCommand.CALIBRATED_ORIGIN, xumstep: calX, yumstep: calY, zumstep: 0, theta: calT);

                    LoggerManager.PinLog($"PTPA : PASSED  Optimize Angle = {this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DiffAngle},    Average difference = ({this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DiffX}, {this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DiffY})");

                    retval = EventCodeEnum.NONE;
                    return retval;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retval = EventCodeEnum.PIN_PAD_MATCH_FAIL;
                
                return retval;
            }            
            finally
            {
                // Make result string for fail occurred.

                // Probe-To-Pad Alignment (PTPA) : FAILED
                // Reason : PIN_OVER_ANGLE_TOLERANCE

                // Probe-To-Pad Alignment (PTPA) : FAILED
                // Reason : PTPA_OUT_OF_TOLERENCE
                // PTPA : Deviation = (1, 1)
                // PTPA : [F] pin #1 matching error = (0.0552463604494733, -3.75019028829001)
                // PTPA : [F] pin #2 matching error = (0.615908465909797, 2.51434286777248)
                // PTPA : [P] pin #3 matching result = (0.616407448648715, 0.618399490765114)
                // PTPA : [F] pin #4 matching error = (-1.2875622750089, 0.617447929750142)

                if (retval != EventCodeEnum.NONE)
                {
                    if(retval == EventCodeEnum.PIN_OVER_ANGLE_TOLERANCE)
                    {
                        LastResultStringBuilder = new StringBuilder();

                        LastResultStringBuilder.Append($"Probe-To-Pad Alignment (PTPA) : FAILED");
                        LastResultStringBuilder.Append(System.Environment.NewLine);

                        LastResultStringBuilder.Append($"Reason : {retval}");
                        LastResultStringBuilder.Append(System.Environment.NewLine);

                        LastResultStringBuilder.Append($"Optimize angle is over tolerance. Current angle = {DiffAngle}, Tolerance = {PinAlignParam.PinPadMatchParam.PinPadMatchToleranceAngle.Value})");
                        LastResultStringBuilder.Append(System.Environment.NewLine);

                        LastResultStringBuilder.Append($"Please check that parameters.");
                    }

                    if (retval == EventCodeEnum.PTPA_OUT_OF_TOLERENCE)
                    {
                        LastResultStringBuilder = new StringBuilder();

                        LastResultStringBuilder.Append($"Probe-To-Pad Alignment (PTPA) : FAILED");
                        LastResultStringBuilder.Append(System.Environment.NewLine);

                        LastResultStringBuilder.Append($"Reason : {retval}");
                        LastResultStringBuilder.Append(System.Environment.NewLine);

                        LastResultStringBuilder.Append($"Tolerance Information : X = {PinAlignParam.PinPadMatchParam.PinPadMatchToleranceX.Value}, Y = {PinAlignParam.PinPadMatchParam.PinPadMatchToleranceY.Value}");
                        LastResultStringBuilder.Append(System.Environment.NewLine);

                        foreach (var result in out_of_tolerence_list)
                        {
                           LastResultStringBuilder.Append($"{result}");
                           LastResultStringBuilder.Append(System.Environment.NewLine);
                        }

                        LastResultStringBuilder.Append(System.Environment.NewLine);
                        LastResultStringBuilder.Append($"Please check that parameters.");
                    }
                }
            }
        }

        public EventCodeEnum DoPinPadMatchBestFitted()
        {
            List<MachineCoordinate> DiffPadToPadCen0 = new List<MachineCoordinate>();
            List<MachineCoordinate> NewPadPos = new List<MachineCoordinate>();
            List<MachineCoordinate> DiffPinToPinCen0 = new List<MachineCoordinate>();
            List<MachineCoordinate> DiffPinToPinCen1 = new List<MachineCoordinate>();

            MachineCoordinate tampcoord = new MachineCoordinate();
            PinCoordinate tampcoord2 = new PinCoordinate();

            double PadAngle = 0;
            double PinAngle = 0;
            double Drad = 0;
            double PadLen = 0;
            double SumDiff = 0;
            double SumLen = 0;
            double DiffAngle = 0;
            double dX = 0;
            double dY = 0;
            double dxSum = 0;
            double dySum = 0;

            MachineCoordinate PinCenter = new MachineCoordinate();
            MachineCoordinate PadCenter = new MachineCoordinate();
            MachineCoordinate PadDist = new MachineCoordinate();
            MachineCoordinate PinDist = new MachineCoordinate();
            List<IPinData> CurPinList = new List<IPinData>();
            IPinData Cur_Pin;
            WaferCoordinate PadPos = new WaferCoordinate();
            double pinPosX = 0;
            double pinPosY = 0;
            double padPosX = 0;
            double padPosY = 0;
            int idx;
            bool bError = false;
            int dut_array = 0;
            int pin_array = 0;

            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            var PinAlignParam = (this.PinAligner().PinAlignDevParam as PinAlignDevParameters);
            List<string> out_of_tolerence_list = new List<string>();

            try
            {
                if (this.StageSupervisor().ProbeCardInfo.GetPinCount() <= 0)
                {
                    this.NotifyManager().Notify(EventCodeEnum.PIN_NOT_ENOUGH);
                    return EventCodeEnum.PIN_NOT_ENOUGH;
                }

                this.WaferAligner().UpdatePadCen();

                PinCenter.X.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenX;
                PinCenter.Y.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenY;

                PadCenter.X.Value = this.StageSupervisor().WaferObject.GetSubsInfo().Pads.PadCenX;
                PadCenter.Y.Value = this.StageSupervisor().WaferObject.GetSubsInfo().Pads.PadCenY;

                SumDiff = 0;

                DiffPadToPadCen0.Clear();
                DiffPinToPinCen0.Clear();
                List<int> pinIndexes = new List<int>();
                List<double> pinAndPadAngles = new List<double>();
                List<double> padDists = new List<double>();

                for (int i = 0; i <= this.StageSupervisor().ProbeCardInfo.GetPinCount() - 1; i++)
                {
                    Cur_Pin = this.StageSupervisor().ProbeCardInfo.GetPin(i);
                    pinIndexes.Add(i);
                    this.StageSupervisor().ProbeCardInfo.GetArrayIndex(i, out dut_array, out pin_array);

                    if (Cur_Pin != null)
                    {
                        idx = this.StageSupervisor().WaferObject.GetSubsInfo().Pads.GetPadArrayIndex(Cur_Pin.PinNum.Value - 1);

                        if (idx >= 0)
                        {
                            pinPosX = Cur_Pin.AbsPos.X.Value;
                            pinPosY = Cur_Pin.AbsPos.Y.Value;

                            padPosX = ((this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[dut_array].MacIndex.XIndex - this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[0].MacIndex.XIndex)
                                        * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Width.Value)
                                        + this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos[idx].PadCenter.X.Value;
                            padPosY = ((this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[dut_array].MacIndex.YIndex - this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[0].MacIndex.YIndex)
                                        * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Height.Value) +
                                        this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos[idx].PadCenter.Y.Value;

                            //padPosX = this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos[idx].MIndexLCWaferCoord.X.Value + this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos[idx].PadCenter.X.Value;
                            //padPosY = this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos[idx].MIndexLCWaferCoord.Y.Value + this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos[idx].PadCenter.Y.Value;

                            //LoggerManager.Debug($"{pinPosX} {pinPosY}");

                            PadDist = new MachineCoordinate();

                            PadDist.X.Value = padPosX - PadCenter.GetX();
                            PadDist.Y.Value = padPosY - PadCenter.GetY();

                            DiffPadToPadCen0.Add(PadDist);

                            PinDist = new MachineCoordinate();

                            PinDist.X.Value = pinPosX - PinCenter.GetX();
                            PinDist.Y.Value = pinPosY - PinCenter.GetY();

                            DiffPinToPinCen0.Add(PinDist);

                            PadAngle = Math.Atan2(PadDist.GetY(), PadDist.GetX());
                            PinAngle = Math.Atan2(PinDist.GetY(), PinDist.GetX());

                            Drad = PinAngle - PadAngle;
                            //PinAngleSum += PinAngle;
                            if (RadianToDegree(Drad) < -300)
                            {
                                Drad = Drad + 2 * Math.PI;
                            }
                            else if (RadianToDegree(Drad) > 300)
                            {
                                Drad = Drad - 2 * Math.PI;
                            }

                            PadLen = GetDistance2D(new MachineCoordinate(0, 0, 0), DiffPadToPadCen0[i]);

                            pinAndPadAngles.Add(Drad);
                            padDists.Add(PadLen);

                            SumDiff = SumDiff + Drad * PadLen;
                            SumLen = SumLen + PadLen;
                        }
                        else
                        {
                            // 핀과 매칭되는 패드가 없음. 에러.
                            retval = EventCodeEnum.PIN_PAD_MATCH_FAIL;
                            return retval;
                        }
                    }
                    else
                    {
                        // 핀 번호가 이상함
                        retval = EventCodeEnum.PIN_INVAILD_STATUS;
                        return retval;
                    }
                }

                if (SumLen == 0)
                    DiffAngle = 0;
                else
                    DiffAngle = RadianToDegree(SumDiff / SumLen);

                if (DiffAngle > PinAlignParam.PinPadMatchParam.PinPadMatchToleranceAngle.Value)
                {
                    LoggerManager.Debug($"PTPA : Optimize angle is over tolerance. Current angle = {DiffAngle}, Tolerance = {PinAlignParam.PinPadMatchParam.PinPadMatchToleranceAngle.Value})");

                    retval = EventCodeEnum.PIN_OVER_ANGLE_TOLERANCE;
                    return retval;
                }
                else
                {
                    LoggerManager.PinLog($"Pin/Pad optimized angle = {DiffAngle}");
                }

                List<List<int>> pinCombinations = new List<List<int>>();
                List<double> combDiffRad = new List<double>();
                List<double> combPadLen = new List<double>();
                List<double> combDiffAngle = new List<double>();
                Dictionary<int, MachineCoordinate> ptpaDiffs = new Dictionary<int, MachineCoordinate>();
                Dictionary<int, double> ptpaDist = new Dictionary<int, double>();

                int pinCombIndex = 0;
                StringBuilder stb = new StringBuilder();
                var pcs = GetKCombs<int>(pinIndexes, 3); // Get pin combinations from pin list
                foreach (var pc in pcs)
                {
                    pinCombinations.Add(pc.ToList());
                }
                foreach (var pc in pinCombinations)
                {

                    combDiffRad.Add(0.0);
                    combPadLen.Add(0.0);
                    combDiffAngle.Add(0.0);
                    if (pc.Count == 3 & pinAndPadAngles.Count > 3 & padDists.Count > 3)
                    {
                        stb.Clear();
                        stb.Append("Pin Combination #");
                        stb.Append(pinCombIndex);
                        stb.Append(" :[");
                        for (int i = 0; i < pc.Count; i++)
                        {
                            combDiffRad[pinCombIndex] = combDiffRad[pinCombIndex] + pinAndPadAngles[pc[i]] * padDists[pc[i]];
                            combPadLen[pinCombIndex] = combPadLen[pinCombIndex] + padDists[pc[i]];
                            stb.Append(pc[i]);
                            if (i < pc.Count - 1) stb.Append(",");
                        }
                        stb.Append("]");
                    }
                    if (combPadLen[pinCombIndex] == 0)
                        combDiffAngle[pinCombIndex] = 0;
                    else
                        combDiffAngle[pinCombIndex] = RadianToDegree(combDiffRad[pinCombIndex] / combPadLen[pinCombIndex]);
                    LoggerManager.Debug($"{stb.ToString()} Angle = {combDiffAngle[pinCombIndex]:0.000000}");

                    MachineCoordinate ptpaDiff = new MachineCoordinate();
                    var ptpaResult = CalcPTPAResults(DiffPadToPadCen0, DiffPinToPinCen0, combDiffAngle[pinCombIndex], out ptpaDiff);
                    if(ptpaResult == EventCodeEnum.NONE)
                    {
                        ptpaDiffs.Add(pinCombIndex, ptpaDiff);
                        var dist = Math.Sqrt(ptpaDiff.X.Value * ptpaDiff.X.Value + ptpaDiff.Y.Value * ptpaDiff.Y.Value);
                        ptpaDist.Add(pinCombIndex, dist);
                    }
                    pinCombIndex++;
                }
                var items = from pair in ptpaDist
                            orderby pair.Value ascending
                            select pair;
                var bestFittedIndex = items.FirstOrDefault().Key;
                double maxThreshold = 1.25;
                double minThreshold = 0.75;
                if (Math.Abs(combDiffAngle[bestFittedIndex]) > Math.Abs(DiffAngle * minThreshold) &
                    Math.Abs(combDiffAngle[bestFittedIndex]) < Math.Abs(DiffAngle * maxThreshold))
                {
                    LoggerManager.Debug($"DoPinPadMatchBestFitted(): Best fitted at {combDiffAngle[bestFittedIndex]:0.000000}. Avg. Angle was {DiffAngle:0.000000}");
                    DiffAngle = combDiffAngle[bestFittedIndex];
                }
                else
                {   // Out of tolerence. Restore to average angle.
                    LoggerManager.Debug($"DoPinPadMatchBestFitted(): Best fitted at {combDiffAngle[bestFittedIndex]:0.000000} and out of Tol., Use Avg. angle({DiffAngle:0.000000})");
                }
                bError = false;
                LoggerManager.PinLog($"PTPA : Deviation = ({PinAlignParam.PinPadMatchParam.PinPadMatchToleranceX.Value}, {PinAlignParam.PinPadMatchParam.PinPadMatchToleranceY.Value})");
                for (int i = 0; i <= this.StageSupervisor().ProbeCardInfo.GetPinCount() - 1; i++)
                {
                    
                    Cur_Pin = this.StageSupervisor().ProbeCardInfo.GetPin(i);
                    if (Cur_Pin != null)
                    {
                        idx = this.StageSupervisor().WaferObject.GetSubsInfo().Pads.GetPadArrayIndex(Cur_Pin.PinNum.Value - 1);
                        if (idx >= 0)
                        {
                            padPosX = DiffPadToPadCen0[i].X.Value;
                            padPosY = DiffPadToPadCen0[i].Y.Value;

                            PadPos = new WaferCoordinate();
                            tampcoord = new MachineCoordinate();

                            PadPos.X.Value = padPosX;
                            PadPos.Y.Value = padPosY;

                            GetRotCoord(ref tampcoord, PadPos, DiffAngle);       // 현재 패드를 매칭 각도만큼 돌려서 변경된 위치를 구한다.
                            //LoggerManager.Debug($"rotated offset = #{Cur_Pin}  {tampcoord.X.Value - PadPos.X.Value}, {tampcoord.Y.Value - PadPos.Y.Value}");

                            NewPadPos.Add(new MachineCoordinate(tampcoord));

                            dX = Math.Abs(DiffPinToPinCen0[i].X.Value - tampcoord.X.Value);
                            dY = Math.Abs(DiffPinToPinCen0[i].Y.Value - tampcoord.Y.Value);

                            string description = string.Empty;

                            if (dX > PinAlignParam.PinPadMatchParam.PinPadMatchToleranceX.Value || dY > PinAlignParam.PinPadMatchParam.PinPadMatchToleranceY.Value)
                            {
                                description = $"PTPA : [F] pin #{Cur_Pin.PinNum} matching error = ({DiffPinToPinCen0[i].X.Value - tampcoord.X.Value}, " +
                                                    $"{DiffPinToPinCen0[i].Y.Value - tampcoord.Y.Value})";

                                LoggerManager.PinLog(description);

                                bError = true;
                            }
                            else
                            {
                                description = $"PTPA : [P] pin #{Cur_Pin.PinNum} matching result = ({DiffPinToPinCen0[i].X.Value - tampcoord.X.Value}, " +
                                                    $"{DiffPinToPinCen0[i].Y.Value - tampcoord.Y.Value})";

                                LoggerManager.PinLog(description);
                            }

                            out_of_tolerence_list.Add(description);

                            dxSum += dX;
                            dySum += dY;

                        }
                        else
                        {
                            retval = EventCodeEnum.PIN_PAD_MATCH_FAIL;
                            return retval;
                        }
                    }
                    else
                    {
                        retval = EventCodeEnum.PIN_INVAILD_STATUS;
                        return retval;
                    }
                }

                if (bError == true)
                {
                    LoggerManager.PinLog("PTPA : FAILED");

                    retval = EventCodeEnum.PTPA_OUT_OF_TOLERENCE;
                    return retval;
                }
                else
                {
                    this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DiffAngle = DiffAngle * 1;
                    this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DiffX = -dxSum / this.StageSupervisor().ProbeCardInfo.GetPinCount();
                    this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DiffY = -dySum / this.StageSupervisor().ProbeCardInfo.GetPinCount();

                    LoggerManager.PinLog($"PTPA : PASSED  Optimize Angle = {this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DiffAngle},    Average difference = ({this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DiffX}, {this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DiffY})");

                    retval = EventCodeEnum.NONE;
                    return retval;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retval = EventCodeEnum.PIN_PAD_MATCH_FAIL;

                return retval;
            }
            finally
            {
                // Make result string for fail occurred.

                // Probe-To-Pad Alignment (PTPA) : FAILED
                // Reason : PIN_OVER_ANGLE_TOLERANCE

                // Probe-To-Pad Alignment (PTPA) : FAILED
                // Reason : PTPA_OUT_OF_TOLERENCE
                // PTPA : Deviation = (1, 1)
                // PTPA : [F] pin #1 matching error = (0.0552463604494733, -3.75019028829001)
                // PTPA : [F] pin #2 matching error = (0.615908465909797, 2.51434286777248)
                // PTPA : [P] pin #3 matching result = (0.616407448648715, 0.618399490765114)
                // PTPA : [F] pin #4 matching error = (-1.2875622750089, 0.617447929750142)

                if (retval != EventCodeEnum.NONE)
                {
                    if (retval == EventCodeEnum.PIN_OVER_ANGLE_TOLERANCE)
                    {
                        LastResultStringBuilder = new StringBuilder();

                        LastResultStringBuilder.Append($"Probe-To-Pad Alignment (PTPA) : FAILED");
                        LastResultStringBuilder.Append(System.Environment.NewLine);

                        LastResultStringBuilder.Append($"Reason : {retval}");
                        LastResultStringBuilder.Append(System.Environment.NewLine);

                        LastResultStringBuilder.Append($"Optimize angle is over tolerance. Current angle = {DiffAngle}, Tolerance = {PinAlignParam.PinPadMatchParam.PinPadMatchToleranceAngle.Value})");
                        LastResultStringBuilder.Append(System.Environment.NewLine);

                        LastResultStringBuilder.Append($"Please check that parameters.");
                    }

                    if (retval == EventCodeEnum.PTPA_OUT_OF_TOLERENCE)
                    {
                        LastResultStringBuilder = new StringBuilder();

                        LastResultStringBuilder.Append($"Probe-To-Pad Alignment (PTPA) : FAILED");
                        LastResultStringBuilder.Append(System.Environment.NewLine);

                        LastResultStringBuilder.Append($"Reason : {retval}");
                        LastResultStringBuilder.Append(System.Environment.NewLine);

                        LastResultStringBuilder.Append($"Tolerance Information : X = {PinAlignParam.PinPadMatchParam.PinPadMatchToleranceX.Value}, Y = {PinAlignParam.PinPadMatchParam.PinPadMatchToleranceY.Value}");
                        LastResultStringBuilder.Append(System.Environment.NewLine);

                        foreach (var result in out_of_tolerence_list)
                        {
                            LastResultStringBuilder.Append($"{result}");
                            LastResultStringBuilder.Append(System.Environment.NewLine);
                        }

                        LastResultStringBuilder.Append(System.Environment.NewLine);
                        LastResultStringBuilder.Append($"Please check that parameters.");
                    }
                }
            }
        }
        static IEnumerable<IEnumerable<T>> GetPermutationsWithRept<T>(IEnumerable<T> list, int length)
        {
            if (length == 1) return list.Select(t => new T[] { t });
            return GetPermutationsWithRept(list, length - 1)
                .SelectMany(t => list,
                    (t1, t2) => t1.Concat(new T[] { t2 }));
        }
        static IEnumerable<IEnumerable<T>> GetKCombs<T>(IEnumerable<T> list, int length) where T : IComparable
        {
            if (length == 1) return list.Select(t => new T[] { t });
            return GetKCombs(list, length - 1)
                .SelectMany(t => list.Where(o => o.CompareTo(t.Last()) > 0),
                    (t1, t2) => t1.Concat(new T[] { t2 }));
        }
        private EventCodeEnum CalcPTPAResults(
            List<MachineCoordinate> DiffPadToPadCen0, 
            List<MachineCoordinate> DiffPinToPinCen0, 
            double compAngle,
            out MachineCoordinate ptpadiff)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            List<MachineCoordinate> NewPadPos = new List<MachineCoordinate>();
            List<string> out_of_tolerence_list = new List<string>();
            ptpadiff = new MachineCoordinate();
            double dxSum = 0;
            double dySum = 0;
            //bool bError;
            try
            {
                var PinAlignParam = (this.PinAligner().PinAlignDevParam as PinAlignDevParameters);
                for (int i = 0; i <= this.StageSupervisor().ProbeCardInfo.GetPinCount() - 1; i++)
                {
                    var cur_Pin = this.StageSupervisor().ProbeCardInfo.GetPin(i);
                    if (cur_Pin != null)
                    {
                        var idx = this.StageSupervisor().WaferObject.GetSubsInfo().Pads.GetPadArrayIndex(cur_Pin.PinNum.Value - 1);
                        if (idx >= 0)
                        {
                            var padPosX = DiffPadToPadCen0[i].X.Value;
                            var padPosY = DiffPadToPadCen0[i].Y.Value;
                            var PadPos = new WaferCoordinate();
                            var tampcoord = new MachineCoordinate();
                            PadPos.X.Value = padPosX;
                            PadPos.Y.Value = padPosY;
                            GetRotCoord(ref tampcoord, PadPos, compAngle);       // 현재 패드를 매칭 각도만큼 돌려서 변경된 위치를 구한다.
                                                                                 //LoggerManager.Debug($"rotated offset = #{Cur_Pin}  {tampcoord.X.Value - PadPos.X.Value}, {tampcoord.Y.Value - PadPos.Y.Value}");
                            NewPadPos.Add(new MachineCoordinate(tampcoord));
                            var dX = Math.Abs(DiffPinToPinCen0[i].X.Value - tampcoord.X.Value);
                            var dY = Math.Abs(DiffPinToPinCen0[i].Y.Value - tampcoord.Y.Value);
                            string description = string.Empty;
                            if (dX > PinAlignParam.PinPadMatchParam.PinPadMatchToleranceX.Value || dY > PinAlignParam.PinPadMatchParam.PinPadMatchToleranceY.Value)
                            {
                                description = $"PTPA : [F] pin #{cur_Pin.PinNum} matching error = ({DiffPinToPinCen0[i].X.Value - tampcoord.X.Value}, " +
                                                    $"{DiffPinToPinCen0[i].Y.Value - tampcoord.Y.Value})";
                                LoggerManager.PinLog(description);
                                //bError = true;
                            }
                            else
                            {
                                description = $"PTPA : [P] pin #{cur_Pin.PinNum} matching result = ({DiffPinToPinCen0[i].X.Value - tampcoord.X.Value}, " +
                                                    $"{DiffPinToPinCen0[i].Y.Value - tampcoord.Y.Value})";
                                LoggerManager.PinLog(description);
                            }
                            out_of_tolerence_list.Add(description);
                            dxSum += dX;
                            dySum += dY;
                        }
                        else
                        {
                            retval = EventCodeEnum.PIN_PAD_MATCH_FAIL;
                            return retval;
                        }
                    }
                    else
                    {
                        retval = EventCodeEnum.PIN_INVAILD_STATUS;
                        return retval;
                    }
                }
                ptpadiff.X.Value = dxSum;
                ptpadiff.Y.Value = dySum;
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"CalcPTPAResults(): Error occurred. Err = {err.Message}");
            }
            return retval;
        }

        //private EventCodeEnum GetPinPadData()
        //{
        //    LoggerManager.Debug($"[{DateTime.Now.ToLocalTime()}] [PinPadMatchModule] GetPinPadData() : GetPinPadData Start");
        //    EventCodeEnum ret = EventCodeEnum.NONE;
        //    //PinData temppin = new PinData();
        //    WaferCoordinate CurDieRefCorner = null;
        //    WaferCoordinate PadCenterABS = null;

        //    int pincnt = 0;


        //    //PinCoordinate CurDutRefCorner = null;
        //    //double RefCornerOffsetX = 0;
        //    //double RefCornerOffsetY = 0;

        //    //int NumOfPin = 0;

        //    try
        //    {
        //        List<DUTPadObject> pads = new List<DUTPadObject>();
        //        pads = (List<DUTPadObject>)this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos;

        //        foreach (IDut dut in this.StageSupervisor().ProbeCardInfo.DutList)
        //        {
        //            pincnt += dut.PinList.Count;
        //        }

        //        if (Pads.Count > 0)
        //        {
        //            Pads.Clear();
        //        }
        //        if (Pins.Count > 0)
        //        {
        //            Pins.Clear();
        //        }

        //        if (pincnt != pads.Count)
        //        {
        //            LoggerManager.Debug($"[{DateTime.Now.ToLocalTime()}] [PinPadMatchModule] GetPinPadData() : Pin count and Pad count is Not Matched");
        //            LoggerManager.Debug($"[{DateTime.Now.ToLocalTime()}] [PinPadMatchModule] GetPinPadData() : Please, Chekc Pin and Pad DataWWW");
        //            ret = EventCodeEnum.PTPA_PAD_NOT_ENOUGH;
        //        }
        //        else
        //        {
        //            foreach (IDut dut in this.StageSupervisor().ProbeCardInfo.DutList)
        //            {
        //                foreach (IPinData pin in dut.PinList)
        //                {
        //                    foreach (DUTPadObject DP in pads)
        //                    {
        //                        if (pin.PinNum.Equals(DP.PadNumber.Value.ToString()))
        //                        {
        //                            CurDieRefCorner = this.WaferAligner().MachineIndexConvertToProbingCoord((int)DP.MachineIndex.XIndex, (int)DP.MachineIndex.YIndex);
        //                            PadCenterABS = new WaferCoordinate(CurDieRefCorner.GetX() + DP.PadCenter.GetX(), CurDieRefCorner.GetY() + DP.PadCenter.GetY(), CurDieRefCorner.GetZ() + DP.PadCenter.GetZ());
        //                            Pads.Add(new MachineCoordinate(this.CoordinateManager().WaferHighChuckConvert.ConvertBack(new WaferCoordinate(PadCenterABS))));
        //                            Pins.Add(new MachineCoordinate(this.CoordinateManager().PinHighPinConvert.ConvertBack(pin.AbsPos)));
        //                            break;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        ret = EventCodeEnum.PTPA_FAIL;
        //        //LoggerManager.Error($err + "GetPinPadData() : Error occured.");
        //        LoggerManager.Exception(err);

        //    }
        //    LoggerManager.Debug($"[{DateTime.Now.ToLocalTime()}] [PinPadMatchModule] GetPinPadData() : GetPinPadData Done");
        //    return ret;
        //}

        //private void CalcCOG(ref MachineCoordinate CenterPos, List<MachineCoordinate> Possitions)
        //{
        //    MachineCoordinate Sum = new MachineCoordinate();
        //    try
        //    {
        //        foreach (MachineCoordinate pos in Possitions)
        //        {
        //            Sum.X.Value += pos.GetX();
        //            Sum.Y.Value += pos.GetY();
        //        }
        //        CenterPos = new MachineCoordinate((Sum.GetX() / Possitions.Count), (Sum.GetY() / Possitions.Count));
        //    }
        //    catch (Exception err)
        //    {
        //        //LoggerManager.Error($err + "CalcCOG() : Error occured.");
        //        LoggerManager.Exception(err);
        //    }
        //}
        private double GetDistance2D(MachineCoordinate FirstPin, MachineCoordinate SecondPin)
        {
            double Distance = -1;
            try
            {

            try
            {
                Distance = Math.Sqrt(Math.Pow(FirstPin.GetX() - SecondPin.GetX(), 2) + Math.Pow(FirstPin.GetY() - SecondPin.GetY(), 2));
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "GetRotCoord() : Error occured.");
                LoggerManager.Exception(err);
            }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return Distance;
        }
        public double GetDegree(PinCoordinate pivot, PinCoordinate pointOld, PinCoordinate pointNew)
        {
            //==> degree = atan((y2 - cy) / (x2-cx)) - atan((y1 - cy)/(x1-cx)) : 세점사이의 각도 구함
            double originDegree = 0;
            double updateDegree = 0;
            
            try
            {
                originDegree = Math.Atan2(
                 pointOld.Y.Value - pivot.Y.Value,
                 pointOld.X.Value - pivot.X.Value)
                 * 180 / Math.PI;

                updateDegree = Math.Atan2(
                     pointNew.Y.Value - pivot.Y.Value,
                     pointNew.X.Value - pivot.X.Value)
                     * 180 / Math.PI;
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "GetRotCoord() : Error occured.");
                LoggerManager.Exception(err);
            }
            //==> 프로버 카드가 틀어진 θ 각
            return updateDegree - originDegree;
        }
        private double DegreeToRadian(double angle)
        {
            double Radian = 0;
            try
            {
                // 각도 => 라디안 변환 
                Radian = Math.PI * angle / 180.0;
                //Radian = angle * 180 / Math.PI; // * 57.2957795130823;
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "GetRotCoord() : Error occured.");
                LoggerManager.Exception(err);
            }
            return Radian;
        }
        private double RadianToDegree(double angle)
        {
            // 라디안 => 각도 변환
            double degree = 0;
            try
            {
                degree = angle / Math.PI * 180; // 1.74532925199433E-02;
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "GetRotCoord() : Error occured.");
                LoggerManager.Exception(err);
            }
            return degree;
        }
        private void GetRotCoord(ref MachineCoordinate NewPos, WaferCoordinate OriPos, double angle)
        {
            try
            {
            double newx = 0.0;
            double newy = 0.0;
            double th = DegreeToRadian(angle);

            try
            {
                newx = OriPos.X.Value;
                newy = OriPos.Y.Value;

                NewPos = new MachineCoordinate((newx * Math.Cos(th) - newy * Math.Sin(th)), (newx * Math.Sin(th) + newy * Math.Cos(th)));
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "GetRotCoord() : Error occured.");
                LoggerManager.Exception(err);
            }


            //NewPos.X.Value = newx * Math.Cos(th) - newy * Math.Sin(th);
            //NewPos.Y.Value = newx * Math.Sin(th) + newy * Math.Cos(th);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }        
    }
}
