using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.AirBlow;
using ProberInterfaces.Command;
using ProberInterfaces.State;
using ProberInterfaces.Wizard;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace AirBlowModule
{
    public class AirBlowChuckCleaningModule : IAirBlowChuckCleaningModule, INotifyPropertyChanged, IHasDevParameterizable, IHasSysParameterizable
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

        #region Properties
        private ModuleStateBase _ModuleState;
        public ModuleStateBase ModuleState
        {
            get { return _ModuleState; }
            set
            {
                if (value != _ModuleState)
                {
                    _ModuleState = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ChuckAirBlowCleaningStateBase _AirBlowState;

        public ChuckAirBlowCleaningStateBase AirBlowState
        {
            get { return _AirBlowState; }
        }

        public IInnerState InnerState
        {
            get { return _AirBlowState; }
            set
            {
                if (value != _AirBlowState)
                {
                    _AirBlowState = value as ChuckAirBlowCleaningStateBase;
                }
            }
        }

        public IInnerState PreInnerState { get; set; }

        private EnumSubsStatus _PrevWaferStatus;

        public EnumSubsStatus PrevWaferStatus
        {
            get { return _PrevWaferStatus; }
            set { _PrevWaferStatus = value; }
        }

        public ISequenceEngineManager SequenceEngineManager
        {
            get
            {
                return this.SequenceEngineManager();
            }
        }

        private ObservableCollection<TransitionInfo> _TransitionInfo = new ObservableCollection<TransitionInfo>();
        public ObservableCollection<TransitionInfo> TransitionInfo
        {
            get { return _TransitionInfo; }
            set
            {
                if (value != _TransitionInfo)
                {
                    _TransitionInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AirBlowDeviceFile _ABDeviceFile;
        public AirBlowDeviceFile ABDeviceFile
        {
            get { return _ABDeviceFile; }
            set
            {
                if (value != _ABDeviceFile)
                {
                    _ABDeviceFile = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AirBlowSystemFile _ABSysFile;
        public AirBlowSystemFile ABSysFile
        {
            get { return _ABSysFile; }
            set
            {
                if (value != _ABSysFile)
                {
                    _ABSysFile = value;
                    RaisePropertyChanged();
                }
            }
        }


        private CommandSlot _CommandRecvSlot = new CommandSlot();
        public CommandSlot CommandRecvSlot
        {
            get { return _CommandRecvSlot; }
            set { _CommandRecvSlot = value; }
        }

        private CommandSlot _CommandProcSlot = new CommandSlot();
        public CommandSlot CommandRecvProcSlot
        {
            get { return _CommandProcSlot; }
            set { _CommandProcSlot = value; }
        }

        private CommandSlot _CommandRecvDoneSlot = new CommandSlot();
        public CommandSlot CommandRecvDoneSlot
        {
            get { return _CommandRecvDoneSlot; }
            set { _CommandRecvDoneSlot = value; }
        }
        private CommandSlot _CommandSendSlot = new CommandSlot();
        public CommandSlot CommandSendSlot
        {
            get { return _CommandSendSlot; }
            set { _CommandSendSlot = value; }
        }

        private CommandTokenSet _RunTokenSet;

        public CommandTokenSet RunTokenSet
        {
            get { return _RunTokenSet; }
            set { _RunTokenSet = value; }
        }

        #endregion

        private CommandInformation _CommandInfo;
        public CommandInformation CommandInfo
        {
            get { return _CommandInfo; }
            set { _CommandInfo = value; }
        }
        
        private ReasonOfError _ReasonOfError = new ReasonOfError(ModuleEnum.AirBlowChuckCleaning);
        public ReasonOfError ReasonOfError
        {
            get { return _ReasonOfError; }
            set
            {
                if (value != _ReasonOfError)
                {
                    _ReasonOfError = value;
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
        //private IParam _SysParam;
        //[ParamIgnore]
        //public IParam SysParam
        //{
        //    get { return _SysParam; }
        //    set
        //    {
        //        if (value != _SysParam)
        //        {
        //            _SysParam = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        private SubStepModules _SubModules;
        public ISubStepModules SubModules
        {
            get { return _SubModules; }
            set
            {
                if (value != _SubModules)
                {
                    _SubModules = (SubStepModules)value;
                    RaisePropertyChanged();
                }
            }
        }

        private EnumModuleForcedState _ForcedDone = EnumModuleForcedState.Normal;
        public EnumModuleForcedState ForcedDone
        {
            get { return _ForcedDone; }
            set { _ForcedDone = value; }
        }

        public bool IsBusy()
        {
            bool retVal = false;
            try
            {
                foreach (var subModule in SubModules.SubModules)
                {
                    if (subModule.GetMovingState() == MovingStateEnum.MOVING)
                    {
                        retVal = true;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public ModuleStateEnum End()
        {
            InnerState.End();
            ModuleState.StateTransition(InnerState.GetModuleState());
            return InnerState.GetModuleState();
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    CommandRecvSlot = new CommandSlot();
                    RunTokenSet = new CommandTokenSet();

                    PrevWaferStatus = EnumSubsStatus.NOT_EXIST;
                    
                    _AirBlowState = new AirBlowChuckCleaningIdleState(this);
                    ModuleState = new ModuleUndefinedState(this);
                    ModuleState.StateTransition(InnerState.GetModuleState());

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
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum ClearState()  //Data 초기화 함=> Done에서 IDLE 상태로 넘어감
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = InnerState.ClearState();
                ModuleState.StateTransition(InnerState.GetModuleState());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public ModuleStateEnum Pause()  //Pause가 호출했을때 해야하는 행동
        {
            try
            {
                InnerState.Pause();
                ModuleState.StateTransition(InnerState.GetModuleState());
                return InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public ModuleStateEnum Resume() // Pause가 풀렸을때 해야하는 행동
        {
            try
            {
                InnerState.Resume();
                ModuleState.StateTransition(InnerState.GetModuleState());
                return InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public ModuleStateEnum Abort()
        {
            try
            {
                InnerState.Abort();
                ModuleState.StateTransition(InnerState.GetModuleState());
                return InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public ModuleStateEnum Execute() // Don`t Touch
        {
            ModuleStateEnum stat = ModuleStateEnum.ERROR;
            try
            {
                EventCodeEnum retVal = InnerState.Execute();
                ModuleState.StateTransition(InnerState.GetModuleState());
                RunTokenSet.Update();
                stat = InnerState.GetModuleState();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return stat;
        }

        public void StateTransition(ModuleStateBase state)
        {
            try
            {
                ModuleState = state;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public EventCodeEnum InnerStateTransition(IInnerState state)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                PreInnerState = _AirBlowState;
                InnerState = state;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        #region ParameterLoad&Save

        public EventCodeEnum SaveParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = SaveDevParameter();
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[AirBlowCleaning] SaveParameter(): Serialize Error");
                    retVal = EventCodeEnum.PARAM_ERROR;
                }

                retVal = SaveSysParameter();
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[AirBlowCleaning] SaveParameter(): Serialize Error");
                    retVal = EventCodeEnum.PARAM_ERROR;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                IParam tmpParam = null;
                tmpParam = new AirBlowDeviceFile();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(AirBlowDeviceFile), this.GetType().Name);

                if (RetVal == EventCodeEnum.NONE)
                {
                    ABDeviceFile = tmpParam as AirBlowDeviceFile;
                    ABDeviceFile.Owner = this;
                }

                //DevParam = new IParamEmpty();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                IParam tmpParam = null;
                tmpParam = new AirBlowSystemFile();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(AirBlowSystemFile), this.GetType().Name);

                if (RetVal == EventCodeEnum.NONE)
                {
                    ABSysFile = tmpParam as AirBlowSystemFile;
                    ABSysFile.Owner = this;
                }

                //SysParam = new IParamEmpty();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = this.SaveParameter(ABDeviceFile, this.GetType().Name);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }



        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = this.SaveParameter(ABSysFile, this.GetType().Name);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;

        }

        public EventCodeEnum LoadAirBlowDeviceFile()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                ABDeviceFile = new AirBlowDeviceFile();
                string fullPath = this.FileManager().GetDeviceParamFullPath(ABDeviceFile.FilePath, this.GetType().Name + ABDeviceFile.FileName);
                try
                {
                    IParam param = null;
                    RetVal = this.LoadParameter(ref param, typeof(AirBlowDeviceFile), fixFullPath: fullPath);
                    if (RetVal == EventCodeEnum.NONE)
                    {
                        ABDeviceFile = param as AirBlowDeviceFile;
                    }
                }
                catch (Exception err)
                {
                    RetVal = EventCodeEnum.PARAM_ERROR;
                    //LoggerManager.Error($"[AirBlowCleaning] LoadDevParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                    LoggerManager.Exception(err);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public EventCodeEnum SaveABDeviceFile()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                string fullPath = this.FileManager().GetDeviceParamFullPath(ABDeviceFile.FilePath, this.GetType().Name + ABDeviceFile.FileName);

                try
                {
                    RetVal = this.SaveParameter(ABDeviceFile, fixFullPath: fullPath);
                }
                catch (Exception err)
                {
                    RetVal = EventCodeEnum.UNDEFINED;
                    //LoggerManager.Error($"[AirBlowCleaningModule] SaveABDeviceFile(): Serialize Error. Err = {0}", err.Message));
                    LoggerManager.Exception(err);

                    throw;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public EventCodeEnum LoadAirBlowSysFile()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                ABSysFile = new AirBlowSystemFile();
                string FullPath = this.FileManager().GetSystemParamFullPath(ABSysFile.FilePath, this.GetType().Name + ABSysFile.FileName);
                try
                {
                    IParam tmpParam = null;
                    RetVal = this.LoadParameter(ref tmpParam, typeof(AirBlowSystemFile), null, FullPath);
                    if (RetVal == EventCodeEnum.NONE)
                    {
                        ABSysFile = tmpParam as AirBlowSystemFile;
                    }
                }
                catch (Exception err)
                {
                    RetVal = EventCodeEnum.PARAM_ERROR;
                    //LoggerManager.Error($"[AirBlowCleaning] LoadSysParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                    LoggerManager.Exception(err);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        #endregion


        #region ActionFunc
        //Todo Chuck Cleaning
        public EventCodeEnum ChuckCleaning()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                double cleaningpos = ABSysFile.StartPos.Y.Value - ABSysFile.CleaningDistance.Value;

                //Chuck이 AirBlow StartPostion으로 이동한다. 
                ret = this.StageSupervisor().StageModuleState.AirBlowMove(ABSysFile.StartPos.X.Value, ABSysFile.StartPos.Y.Value, ABSysFile.StartPos.Z.Value);
                if (ret == EventCodeEnum.NONE)
                {
                    //바람 쏜다 
                    //다시 풀어야함
                    //ret = StageSuperVisor.StageModuleState.AirBlowAirOnOff(true);
                    if (ret == EventCodeEnum.NONE)
                    {
                        //척이 Y축으로 반복적으로 움직인다(세팅에 의해 시간 or 클리닝 카운트 )
                        if (ABSysFile.AirBlowCleaningType.Value == EnumAirBlowCleaningType.CLEANINGCOUNT)
                        {
                            for (int i = 0; i < ABDeviceFile.CleaningCount.Value; i++)
                            {
                                ret = this.StageSupervisor().StageModuleState.AirBlowMove(EnumAxisConstants.Y, cleaningpos, ABSysFile.AirBlowSpeed.Value, ABSysFile.AirBlowAcc.Value);
                                if (ret != EventCodeEnum.NONE)
                                {
                                    ret = EventCodeEnum.MOTION_MOVING_ERROR;
                                    LoggerManager.Error($"AirBlow ChuckCleaning() Moving Error");
                                    break;
                                }
                                ret = this.StageSupervisor().StageModuleState.AirBlowMove(EnumAxisConstants.Y, ABSysFile.StartPos.Y.Value, ABSysFile.AirBlowSpeed.Value, ABSysFile.AirBlowAcc.Value);
                                if (ret != EventCodeEnum.NONE)
                                {
                                    ret = EventCodeEnum.MOTION_MOVING_ERROR;
                                    LoggerManager.Error($"AirBlow ChuckCleaning() Moving Error");
                                    break;
                                }
                            }
                        }
                        else if (ABSysFile.AirBlowCleaningType.Value == EnumAirBlowCleaningType.TIME)
                        {
                            Stopwatch stw = new Stopwatch();
                            stw.Start();
                            bool runflag = true;
                            while (runflag)
                            {
                                if (stw.Elapsed.Seconds > ABDeviceFile.CleaningTime.Value)
                                {
                                    ret = EventCodeEnum.NONE;
                                    LoggerManager.Debug($"AirBlow ChuckCleaning() Moving");
                                    runflag = false;
                                }
                                else
                                {
                                    ret = this.StageSupervisor().StageModuleState.AirBlowMove(EnumAxisConstants.Y, cleaningpos, ABSysFile.AirBlowSpeed.Value, ABSysFile.AirBlowAcc.Value);
                                    if (ret != EventCodeEnum.NONE)
                                    {
                                        ret = EventCodeEnum.MOTION_MOVING_ERROR;
                                        LoggerManager.Error($"AirBlow ChuckCleaning() Moving Error");
                                        runflag = false;
                                    }
                                    ret = this.StageSupervisor().StageModuleState.AirBlowMove(EnumAxisConstants.Y, ABSysFile.StartPos.Y.Value, ABSysFile.AirBlowSpeed.Value, ABSysFile.AirBlowAcc.Value);
                                    if (ret != EventCodeEnum.NONE)
                                    {
                                        ret = EventCodeEnum.MOTION_MOVING_ERROR;
                                        LoggerManager.Error($"AirBlow ChuckCleaning() Moving Error");

                                        runflag = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            ret = EventCodeEnum.PARAM_ERROR;
                            LoggerManager.Error($"AirBlow ChuckCleaning() Param Error");
                        }

                        //바람 멈춤
                        //다시 풀어야함 
                        //ret = StageSuperVisor.StageModuleState.AirBlowAirOnOff(false);
                        if (ret != EventCodeEnum.NONE)
                        {
                            ret = EventCodeEnum.IO_DEV_CONN_ERROR;
                            LoggerManager.Error($"AirBlow ChuckCleaning() IO Error");
                        }
                    }
                    else
                    {
                        ret = EventCodeEnum.IO_DEV_CONN_ERROR;
                        LoggerManager.Error($"AirBlow ChuckCleaning() IO Error");
                    }

                }
                else
                {
                    ret = EventCodeEnum.MOTION_MOVING_ERROR;
                    LoggerManager.Error($"AirBlow ChuckCleaning() Moving Error");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                throw new Exception(string.Format("Class: {0} Function: {1} ReturnValue: {2} HashCode: {3} ExceptionMessage: {4} ", this, MethodBase.GetCurrentMethod(), ret.ToString(), err.GetHashCode(), err.Message));
            }

            return ret;
        }


        public bool CanExecute(IProbeCommandToken token)
        {
            bool isInjected = false;
            try
            {

                isInjected = _AirBlowState.CanExecute(token);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isInjected;
        }

        //Todo Chuck TempControl 


        #endregion

        public bool IsLotReady(out string msg) //Lot 시작시 조건 체크
        {
            msg = "";
            return true;
        }
     
        public EventCodeEnum ParamValidation()
        {
            throw new NotImplementedException();
        }

        public bool IsParameterChanged(bool issave = false)
        {
            throw new NotImplementedException();
        }
        public EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public string GetModuleMessage()
        {
            string retval = string.Empty;

            try
            {
                EnumChuckAirBlowCleaningState state = (InnerState as ChuckAirBlowCleaningStateBase).GetState();

                retval = state.ToString();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
}
