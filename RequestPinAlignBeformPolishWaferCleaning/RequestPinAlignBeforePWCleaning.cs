using System;
using System.Collections.Generic;

namespace RequestPinAlignBeforePolishWaferCleaning
{
    using ProberInterfaces;
    using ProberErrorCode;
    using System.ComponentModel;
    using Newtonsoft.Json;
    using System.Runtime.CompilerServices;
    using ProberInterfaces.State;
    using LogModule;
    using ProberInterfaces.Command.Internal;
    using ProberInterfaces.PolishWafer;
    using ProberInterfaces.Command;

    public class RequestPinAlignBeforePWCleaning : IProcessingModule, INotifyPropertyChanged
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
        public RequestPinAlignBeforePWCleaning()
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

            if (cleaninginfo == null)
            {
                LoggerManager.Error($"Unknwon Error.");
            }

            try
            {
                LoggerManager.Debug($"PinAlignBeforePoliWaferCleaning - PinAlign Before Cleaning : {cleaningparam.PinAlignBeforeCleaning.Value} PinAlign Before Cleaning Processed :{cleaninginfo.PinAlignBeforeCleaningProcessed}");
                if (cleaninginfo.PinAlignBeforeCleaningProcessed == true ||
                    cleaningparam.PinAlignBeforeCleaning.Value == false) 
                {
                    if (cleaninginfo.PolishWaferCleaningProcessed == false) 
                    {
                        this.PolishWaferModule().CommandSendSlot.ClearToken();
                    }
                    SubModuleState = new SubModuleDoneState(this);
                    return retVal = EventCodeEnum.NONE;
                }
                
                if (cleaningparam.PinAlignBeforeCleaning.Value == true)
                {
                    //SubModuleState.GetState() != SubModuleStateEnum.SUSPEND 으로 확인 시 AlignStateEnum.DONE이여도 pin 개수 다르게 align 했울 수 있다
                    //== PINALIGNSOURCE.POLISH_WAFER 로 진행한 align 이 Done 이여야한다. 

                    // TODO : Check, Lot Run과 Manual 동작의 로직이 구분되어야 하는가?
                    if (this.PolishWaferModule().CommandSendSlot?.Token?.GetState() != CommandStateEnum.DONE
                        || this.PolishWaferModule().CommandRecvDoneSlot?.Token is IDoManualPolishWaferCleaning)
                    {
                        if (this.PolishWaferModule().CommandRecvDoneSlot?.Token is IDoManualPolishWaferCleaning)
                        {
                            this.PolishWaferModule().CommandRecvDoneSlot.ClearToken();
                        }

                        bool commandResult = false;

                        // Cell Paused 상태에서는 IDoManualPinAlign 커맨드만 받게 되어 있음.
                        if (this.PinAligner().ModuleState.GetState() == ModuleStateEnum.PAUSED)
                        {
                            commandResult = this.CommandManager().SetCommand<IDoManualPinAlign>(this.PolishWaferModule());
                        }
                        else
                        {
                            commandResult = this.CommandManager().SetCommand<IDOPINALIGN>(this.PolishWaferModule());
                        }

                        if (commandResult)
                        {
                            this.PinAligner().PinAlignSource = PINALIGNSOURCE.POLISH_WAFER;

                            //핀 얼라인 동작 요청
                            LoggerManager.Debug("Request pin alignment before polish wafer cleaning...");
                            retVal = EventCodeEnum.SUB_SUSPEND;
                            SubModuleState = new SubModuleSuspendState(this);
                        }
                        else
                        {
                            LoggerManager.Debug("Request pin alignment before polish wafer cleaning Pin Alignment module is busy");
                            // 핀 얼라인 Busy 상태? (니들 클리닝이 이미 RUN 상태인데 핀이 외부 명령으로 인해 바쁘다? 말도 안되는 상황)
                            retVal = EventCodeEnum.POLISHWAFER_CLEANING_PIN_ALIGNMENT_COMMAND_REJECTED;
                            this.PolishWaferModule().ReasonOfError.AddEventCodeInfo(retVal, "Pin Alignment module is busy", this.GetType().Name);
                            this.PolishWaferModule().CommandSendSlot.ClearToken();

                            SubModuleState = new SubModuleErrorState(this);
                        }
                    }
                    else
                    {
                        // 이미 동작 완료된 상태
                        if (this.PolishWaferModule().CommandSendSlot?.Token?.GetState() == CommandStateEnum.DONE &&
                        this.StageSupervisor().ProbeCardInfo.GetAlignState() == AlignStateEnum.DONE)
                        {
                            this.PolishWaferModule().CommandSendSlot.ClearToken();
                            cleaninginfo.PinAlignBeforeCleaningProcessed = true;
                            SubModuleState = new SubModuleDoneState(this);
                            retVal = EventCodeEnum.NONE;
                        }
                        else
                        {
                            // Pin align is not done yet. Return error.
                            retVal = EventCodeEnum.POLISHWAFER_CLEANING_PIN_ALIGNMENT_NOT_DONE;
                            this.PolishWaferModule().ReasonOfError.AddEventCodeInfo(retVal, "Pin Alignment is not done yet", this.GetType().Name);
                            SubModuleState = new SubModuleErrorState(this);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                //(this.PolishWaferModule().PolishWaferParameter as IPolishWaferParameter).PolishWaferCleaningProcessed = false;
                cleaninginfo.PolishWaferCleaningProcessed = false;

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
