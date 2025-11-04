using System;
using System.Collections.Generic;

namespace RequestPinAlignAfterPolishWaferCleaning
{
    using ProberInterfaces;
    using ProberErrorCode;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using ProberInterfaces.State;
    using LogModule;
    using Newtonsoft.Json;
    using ProberInterfaces.Command.Internal;
    using ProberInterfaces.PolishWafer;
    using ProberInterfaces.Command;

    public class RequestPinAlignAfterPWCleaning : IProcessingModule, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //..IProciessingModule Property
        public string Genealogy { get; set; }
        public object Owner { get; set; }
        public List<object> Nodes { get; set; }
        public bool Initialized { get; set; }

        public SubModuleMovingStateBase MovingState { get; set; }

        private SubModuleStateBase _SubModuleState;
        public SubModuleStateBase SubModuleState
        {
            get { return _SubModuleState; }
            set
            {
                if (value != _SubModuleState)
                {
                    _SubModuleState = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region //..Init & DeInit Method
        public RequestPinAlignAfterPWCleaning()
        {

        }
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                SubModuleState = new SubModuleIdleState(this);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void DeInitModule()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region //..IProcessingModule Method (Don't Touch)
        public void ClearState()
        {
            SubModuleState = new SubModuleIdleState(this);
        }
        public EventCodeEnum ClearData()
        {
            return SubModuleState.ClearData();
        }
        public EventCodeEnum Recovery()
        {
            return SubModuleState.Recovery();
        }
        public EventCodeEnum ExitRecovery()
        {
            return SubModuleState.ExitRecovery();
        }
        public EventCodeEnum Execute()
        {
            return SubModuleState.Execute();
        }
        public MovingStateEnum GetMovingState()
        {
            return MovingState.GetState();
        }
        public SubModuleStateEnum GetState()
        {
            return SubModuleState.GetState();
        }

        #endregion

        #region //..IProcessing Method

        public EventCodeEnum ParamValidation()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        public bool IsExecute()
        {
            bool retVal = false;
            try
            {
                retVal = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        public bool IsParameterChanged(bool issave = false)
        {
            bool retVal = false;
            try
            {
                retVal = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        public EventCodeEnum DoClearData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        public EventCodeEnum DoExecute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            IPolishWaferModule pwmoule = this.PolishWaferModule();
            
            IPolishWaferCleaningParameter cleaningparam = null;
            PolishWafertCleaningInfo cleaninginfo = null;

            if (pwmoule.IsManualTriggered == false)
            {
                cleaningparam = pwmoule.GetCurrentCleaningParam();
                cleaninginfo = pwmoule.ProcessingInfo.GetCurrentCleaningInfo();
            }
            else
            {
                cleaningparam = pwmoule.ManualCleaningParam;
                cleaninginfo = pwmoule.ManualCleaningInfo;
            }

            if (cleaningparam == null)
            {
                LoggerManager.Error($"Unknwon Error.");
            }

            try
            {
                // 클리닝이 이루어진 후, 동작이 이루어져야 하기 때문에, 클리닝이 끝났는지 확인.
                if (cleaninginfo.PolishWaferCleaningProcessed == false)
                {
                    SubModuleState = new SubModuleDoneState(this);
                    retVal = EventCodeEnum.NONE;
                    return retVal;
                }

                if (cleaningparam.PinAlignAfterCleaning.Value == true)
                {
                    if ((this.PolishWaferModule().CommandSendSlot?.Token?.GetState() == CommandStateEnum.DONE || cleaninginfo.PinAlignAfterCleaningProcessed == true) &&
                        this.StageSupervisor().ProbeCardInfo.GetAlignState() == AlignStateEnum.DONE)
                    {
                        this.PolishWaferModule().CommandSendSlot.ClearToken();
                        cleaninginfo.PinAlignAfterCleaningProcessed = true;
                        SubModuleState = new SubModuleDoneState(this);
                        return retVal = EventCodeEnum.NONE;
                    }

                    if (cleaninginfo.PinAlignAfterCleaningProcessed == false)
                    {
                        // 클리닝을 진행한 후, 핀 얼라인을 다시 하기 위해, 핀 얼라인 데이터를 초기화하자.
                        //this.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);

                        if (this.CommandManager().SetCommand<IDOPINALIGN>(this.PolishWaferModule()) == true)
                        {
                            this.PinAligner().PinAlignSource = PINALIGNSOURCE.POLISH_WAFER;

                            // 핀 얼라인 동작 요청
                            LoggerManager.Debug("Request pin alignment after polish wafer cleaning...");
                            retVal = EventCodeEnum.SUB_SUSPEND;
                            SubModuleState = new SubModuleSuspendState(this);
                        }
                        else
                        {
                            // 핀 얼라인 Busy 상태? (니들 클리닝이 이미 RUN 상태인데 핀이 외부 명령으로 인해 바쁘다? 말도 안되는 상황)
                            retVal = EventCodeEnum.POLISHWAFER_CLEANING_PIN_ALIGNMENT_COMMAND_REJECTED;

                            this.PolishWaferModule().ReasonOfError.AddEventCodeInfo(retVal, "Pin Alignment module is busy", this.GetType().Name);
                            //this.PolishWaferModule().ReasonOfError.Reason = "Pin Alignment module is busy";

                            SubModuleState = new SubModuleErrorState(this);
                        }
                    }
                }
                else
                {
                    ////옵션 꺼져 있음
                    this.PolishWaferModule().CommandSendSlot.ClearToken();
                    cleaninginfo.PinAlignAfterCleaningProcessed = true;
                    SubModuleState = new SubModuleDoneState(this);
                    retVal = EventCodeEnum.NONE;
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                cleaninginfo.PinAlignAfterCleaningProcessed = false;
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Exception(err);
                throw err;
            }

            return retVal;
        }
        public EventCodeEnum DoExitRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        public EventCodeEnum DoRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
        #endregion
    }
}
