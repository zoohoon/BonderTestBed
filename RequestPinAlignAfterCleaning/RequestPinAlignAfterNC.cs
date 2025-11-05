using System;
using System.Collections.Generic;
using System.ComponentModel;
using ProberErrorCode;
using LogModule;
using NeedleCleanerModuleParameter;
using ProberInterfaces.State;
using ProberInterfaces;
using SubstrateObjects;
using ProberInterfaces.NeedleClean;
using ProberInterfaces.Command.Internal;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace RequestPinAlignAfterCleaning
{
    public class RequestPinAlignAfterNC : IProcessingModule, INotifyPropertyChanged, ITemplateModule
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

        #region IParamNode
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
        [ParamIgnore]
        public List<object> Nodes { get; set; }
        #endregion
        private IStateModule _NeedleCleanModule;
        public IStateModule NeedleCleanModule
        {
            get { return _NeedleCleanModule; }
            set
            {
                if (value != _NeedleCleanModule)
                {
                    _NeedleCleanModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        public NeedleCleanObject NC { get { return this.StageSupervisor().NCObject as NeedleCleanObject; } }

        private NeedleCleanDeviceParameter _NeedleCleanerParam;
        public NeedleCleanDeviceParameter NeedleCleanerParam
        {
            get
            {
                //if (_NeedleCleanerParam == null)
                //{
                //    var module = NeedleCleanModule as IHasDevParameterizable;
                //    var ncmodule = this.NeedleCleanModule();
                //    _NeedleCleanerParam = module.DevParam as NeedleCleanDeviceParameter;
                //}
                return _NeedleCleanerParam;
            }
            set
            {
                if (value != _NeedleCleanerParam)
                {
                    _NeedleCleanerParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        public SubModuleMovingStateBase MovingState { get; set; }

        public RequestPinAlignAfterNC()
        {

        }

        public RequestPinAlignAfterNC(IStateModule Module)
        {
            _NeedleCleanModule = Module;
        }

        #region ISubModule
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

        public EventCodeEnum StartJob()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            return RetVal;
        }

        public EventCodeEnum Execute()       // 외부에서(ex,lot) Needle clean module 의 
        {
            return SubModuleState.Execute();
        }

        public SubModuleStateEnum GetState()
        {
            return SubModuleState.GetState();
        }

        public INeedleCleanDeviceObject NeedleCleanDeviceObject { get; set; }
        //private IMotionManager Motion;

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    MovingState = new SubModuleStopState(this);
                    SubModuleState = new SubModuleIdleState(this);

                    var stage = this.StageSupervisor();
                    var ncModule = this.NeedleCleaner();
                    NeedleCleanModule = ncModule;
                    //NeedleCleanerParam = (NeedleCleanDeviceParameter)ncModule.DevParam;
                    NeedleCleanerParam = (NeedleCleanDeviceParameter)ncModule.NeedleCleanDeviceParameter_IParam;

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

        public EventCodeEnum ClearData()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. RequestPinAlignAfterNC - ClearData() : Error occured.");
                throw err;
            }
            return retVal;
        }
        #endregion

        #region IHasDevParameterizable
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

        public EventCodeEnum LoadDevParameter()
        {


            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            //IParam tmpParam = null;
            //tmpParam = new WA_EdgeParam_Standard();
            //tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
            //RetVal = Extensions_IParam.LoadParameter(ref tmpParam, typeof(WA_EdgeParam_Standard));

            //if (RetVal == EventCodeEnum.NONE)
            //{
            //    Param = tmpParam as WA_EdgeParam_Standard;
            //    EdgeStandardParam = tmpParam as WA_EdgeParam_Standard;
            //    //TempEdgeStandardParam;
            //}


            return RetVal;
        }

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            //try
            //{
            //    if (Param != null)
            //    {
            //        Param = EdgeStandardParam;
            //        RetVal = Extensions_IParam.SaveParameter(Param);
            //        RetVal = Extensions_IParam.DeleteTempFile(EdgeStandardParam, typeof(WA_EdgeParam_Standard));
            //    }
            //}
            //catch (Exception err)
            //{
            //    LoggerManager.Debug($"{err.ToString()}. EdgeStndard - SaveDevParameter() : Error occured.");
            //}

            return RetVal;
        }
        #endregion

        public void DeInitModule()
        {
        }

        private bool IsReadyToCleaning()
        {
            try
            {
                if (NC.NCSysParam.TouchSensorRegistered.Value != true || NC.NCSysParam.TouchSensorBaseRegistered.Value != true ||
                    NC.NCSysParam.TouchSensorPadBaseRegistered.Value != true || NC.NCSysParam.TouchSensorOffsetRegistered.Value != true)
                {
                    return false;
                }
                else
                {
                    return true;
                }

            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. NeedleCleanProcessor - IsReadyToCleaning() : Error occured.");
                return false;
            }
        }

        public EventCodeEnum DoExecute()         //실제 동작하는 logic
        {
            EventCodeEnum RetVal = EventCodeEnum.NONE;

            try
            {
                if (IsReadyToCleaning() == false)
                {
                    return EventCodeEnum.NEEDLE_CLEANING_NOT_READY;
                }

                if (NeedleCleanModule.LotOPModule().InnerState.GetModuleState() == ModuleStateEnum.RUNNING)
                {
                    if (SubModuleState.GetState() != SubModuleStateEnum.SUSPEND)
                    {
                        if (this.StageSupervisor().NCObject.NeedleCleaningProcessed == false)
                        {
                            SubModuleState = new SubModuleDoneState(this);
                            RetVal = EventCodeEnum.NONE;
                            return RetVal;
                        }
                        //this.StageSupervisor().NCObject.NeedleCleaningProcessed = false;
                        if (NeedleCleanerParam.PinAlignAfterCleaning.Value == true)
                        {
                            if (this.StageSupervisor().NCObject.PinAlignAfterCleaningProcessed == false)
                            {
                                if (this.CommandManager().SetCommand<IDOPINALIGN>(this.NeedleCleaner()) == true)
                                {
                                    // 핀 얼라인 동작 요청
                                    LoggerManager.Debug("Request pin alignment after needle cleaning...");
                                    RetVal = EventCodeEnum.SUB_SUSPEND;
                                    SubModuleState = new SubModuleSuspendState(this);
                                }
                                else
                                {
                                    // 핀 얼라인 Busy 상태? (니들 클리닝이 이미 RUN 상태인데 핀이 외부 명령으로 인해 바쁘다? 말도 안되는 상황)
                                    //this.NeedleCleanModule.ReasonOfError.Reason = "Pin Alignment module is busy";

                                    RetVal = EventCodeEnum.NEEDLE_CLEANING_PIN_ALIGNMENT_COMMAND_REJECTED;

                                    this.NeedleCleanModule.ReasonOfError.AddEventCodeInfo(RetVal, "Pin Alignment module is busy", this.GetType().Name);

                                    SubModuleState = new SubModuleErrorState(this);
                                }
                            }
                            else
                            {
                                // 이미 동작 완료된 상태
                                if (this.StageSupervisor().ProbeCardInfo.GetAlignState() == AlignStateEnum.DONE)
                                {
                                    SubModuleState = new SubModuleDoneState(this);
                                    RetVal = EventCodeEnum.NONE;
                                }
                                else
                                {
                                    // Pin align is not done yet. Return error.
                                    //this.NeedleCleanModule.ReasonOfError.Reason = "Pin Alignment is not done yet";
                                    RetVal = EventCodeEnum.NEEDLE_CLEANING_PIN_ALIGNMENT_NOT_DONE;

                                    this.NeedleCleanModule.ReasonOfError.AddEventCodeInfo(RetVal, "Pin Alignment is not done yet", this.GetType().Name);

                                    SubModuleState = new SubModuleErrorState(this);
                                }
                            }
                        }
                        else
                        {
                            // 옵션 꺼져 있음
                            if (this.StageSupervisor().ProbeCardInfo.GetAlignState() == AlignStateEnum.DONE)
                            {
                                SubModuleState = new SubModuleDoneState(this);
                                RetVal = EventCodeEnum.NONE;
                            }
                            else
                            {
                                // Pin align is not done yet. Return error.
                                RetVal = EventCodeEnum.NEEDLE_CLEANING_PIN_ALIGNMENT_NOT_DONE;
                                //this.NeedleCleanModule.ReasonOfError.Reason = "Pin Alignment is not done yet";
                                this.NeedleCleanModule.ReasonOfError.AddEventCodeInfo(RetVal, "Pin Alignment is not done yet", this.GetType().Name);

                                SubModuleState = new SubModuleErrorState(this);
                            }
                        }
                    }
                    else
                    {
                        // 옵션이 켜져 있어서 핀 얼라인에게 커맨드를 날렸고, 커맨드가 완료되어 온 상황
                        if (this.StageSupervisor().NCObject.PinAlignAfterCleaningProcessed == false)
                        {
                            // 이미 SUSPEND 상태라는 이야기는 커맨드가 나가서 핀 얼라인이 돌고 다시 돌아온 상태
                            if (this.StageSupervisor().ProbeCardInfo.GetAlignState() == AlignStateEnum.DONE)
                            {
                                this.StageSupervisor().NCObject.PinAlignAfterCleaningProcessed = true;
                                SubModuleState = new SubModuleDoneState(this);
                                RetVal = EventCodeEnum.NONE;
                            }
                            else
                            {
                                // Pin align is not done yet. Return error.
                                RetVal = EventCodeEnum.NEEDLE_CLEANING_PIN_ALIGNMENT_NOT_DONE;
                                //this.NeedleCleanModule.ReasonOfError.Reason = "Pin Alignment is not done yet";
                                this.NeedleCleanModule.ReasonOfError.AddEventCodeInfo(RetVal, "Pin Alignment is not done yet", this.GetType().Name);

                                SubModuleState = new SubModuleErrorState(this);
                            }
                        }
                        else
                        {
                            // 이상한 상태. 난 이미 끝났는데 내가 SUSPEND 상태일 수 없음. 플래그가 제대로 초기화 되지 않았음. 
                            LoggerManager.Debug($"RequestPinAlignAfterCleaning - Done flag does not initialized correctly");
                            SubModuleState = new SubModuleDoneState(this);
                            RetVal = EventCodeEnum.NONE;
                        }
                    }
                }
                else
                {
                    // 사용자에 의한 메뉴얼 동작 시                    
                    if (this.StageSupervisor().ProbeCardInfo.GetAlignState() == AlignStateEnum.DONE || Extensions_IParam.ProberRunMode == RunMode.EMUL)
                    {
                        SubModuleState = new SubModuleDoneState(this);
                        RetVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        // Pin align is not done yet. Return error.
                        RetVal = EventCodeEnum.NEEDLE_CLEANING_PIN_ALIGNMENT_NOT_DONE;
                        SubModuleState = new SubModuleErrorState(this);
                    }
                }
            }

            catch (Exception err)
            {
                this.StageSupervisor().NCObject.NeedleCleaningProcessed = false;
                LoggerManager.Debug($"{err.ToString()}. RequestPinAlignAfterCleaning - DoExecute() : Error occured.");
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                throw err;
            }

            this.StageSupervisor().NCObject.NeedleCleaningProcessed = false;
            return RetVal;

        }

        public EventCodeEnum DoClearData()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. RequestPinAlignAfterCleaning - DoClearData() : Error occured.");
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                throw err;
            }
            return retVal;
        }
        public void ClearState()
        {
            SubModuleState = new SubModuleIdleState(this);
        }
        public EventCodeEnum DoRecovery()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum DoExitRecovery()
        {
            throw new NotImplementedException();
        }

        public bool IsExecute()
        {

            try
            {
                if (NC.NCSysParam.CleanUnitAttached.Value == true)
                {
                    for (int i = 0; i <= NC.NCSysParam.MaxCleanPadNum.Value - 1; i++)
                    {
                        if (this.NeedleCleaner().IsTimeToCleaning(i) == true)
                        {
                            return true;
                        }
                    }
                    return false;
                }
                return false;
            }

            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. CleanPadFocusing - IsExecute() : Error occured.");
                throw err;
            }
        }

        public EventCodeEnum Recovery()
        {
            return SubModuleState.Recovery();
        }

        public MovingStateEnum GetMovingState()
        {
            return MovingState.GetState();
        }

        public EventCodeEnum ExitRecovery()
        {
            return SubModuleState.ExitRecovery();
        }

        public EventCodeEnum ParamValidation()
        {
            throw new NotImplementedException();
        }

        public bool IsParameterChanged(bool issave = false)
        {
            return false;
        }
    }
}
