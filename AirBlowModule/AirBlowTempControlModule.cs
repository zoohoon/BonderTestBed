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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NotifyEventModule;
using ProberInterfaces.Event;
using System.Collections.Generic;

namespace AirBlowModule
{
    public class AirBlowTempControlModule : IAirBlowTempControlModule, INotifyPropertyChanged, IHasDevParameterizable, IHasSysParameterizable
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
        private AirBlowTempControlStateBase _AirBlowTempControlState;

        public AirBlowTempControlStateBase AirBlowTempControlState
        {
            get { return _AirBlowTempControlState; }
        }

        public IInnerState InnerState
        {
            get { return _AirBlowTempControlState; }
            set
            {
                if (value != _AirBlowTempControlState)
                {
                    _AirBlowTempControlState = value as AirBlowTempControlStateBase;
                }
            }
        }

        public IInnerState PreInnerState { get; set; }

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
        private ReasonOfError _ReasonOfError = new ReasonOfError(ModuleEnum.AirBlowTempControl);
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


        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    CommandRecvSlot = new CommandSlot();
                    RunTokenSet = new CommandTokenSet();

                    //var v = TempManager.TempControl.NotifySVChanged.GetInvocationList(); //test

                    if (ABDeviceFile.AirBlowTempControlEnable.Value == true)
                    {
                        this.EventManager().RegisterEvent(typeof(AlarmSVChangedEvent).FullName, "ProbeEventSubscibers", EventFired);
                        _AirBlowTempControlState = new AirBlowTempControlIdleState(this);
                        ModuleState = new ModuleIdleState(this);
                    }
                    else
                    {
                        _AirBlowTempControlState = new AirBlowTempControlIdleState(this);
                        ModuleState = new ModuleIdleState(this);
                    }

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
        public ModuleStateEnum End() // Abort 시킬때 해야하는 행동
        {
            try
            {
                InnerState.End();
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
            InnerState.Abort();
            ModuleState.StateTransition(InnerState.GetModuleState());
            return InnerState.GetModuleState();
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
            ModuleState = state;
        }

        public EventCodeEnum InnerStateTransition(IInnerState state)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                PreInnerState = _AirBlowTempControlState;
                InnerState = state;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
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


        #region ParameterLoad&Save

        public EventCodeEnum SaveParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = SaveDevParameter();
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[AirBlowTemp] SaveParameter(): Serialize Error");
                    retVal = EventCodeEnum.PARAM_ERROR;
                }

                retVal = SaveSysParameter();
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[AirBlowTemp] SaveParameter(): Serialize Error");
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
                    IParam tmpParam = null;
                    RetVal = this.LoadParameter(ref tmpParam, typeof(AirBlowDeviceFile), null, fullPath);
                    if (RetVal == EventCodeEnum.NONE)
                    {
                        ABDeviceFile = tmpParam as AirBlowDeviceFile;
                    }
                }
                catch (Exception err)
                {
                    RetVal = EventCodeEnum.PARAM_ERROR;
                    //LoggerManager.Error($"[AirBlowTempControl] LoadDevParam(): Error occurred while loading parameters. Err = {0}", err.Message));
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
                    RetVal = Extensions_IParam.SaveParameter(null, ABDeviceFile, null, fullPath);
                    if (RetVal != EventCodeEnum.NONE)
                    {
                        throw new Exception("[AirBlowTempControlModule - SaveABDeviceFile]Faile SaveParameter");
                    }
                }
                catch (Exception err)
                {
                    RetVal = EventCodeEnum.UNDEFINED;
                    //LoggerManager.Error($"[AirBlowTempControlModule] SaveABDeviceFile(): Serialize Error. Err = {0}", err.Message));
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
                    //LoggerManager.Error($"[AirBlowTempControl] LoadSysParam(): Error occurred while loading parameters. Err = {0}", err.Message));
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

        //Todo Chuck TempControl 
        //public EventCodeEnum ChuckTempControl()
        //{
        //    EventCodeEnum ret = EventCodeEnum.UNDEFINED;
        //    double cleaningpos = ABSysFile.StartPos.Y.Value + ABSysFile.CleaningDistance;
        //    double prevTemp = TempManager.TempControl.GetCurTemp;
        //    bool runflag = true;
        //    bool injury = false;
        //    Stopwatch stw = new Stopwatch();
        //    Stopwatch injurystw = new Stopwatch();
        //    //Chuck이 AirBlow StartPostion으로 이동한다. 
        //    ret = StageSuperVisor.StageModuleState.AirBlowMove(ABSysFile.StartPos.X.Value, ABSysFile.StartPos.Y.Value, ABSysFile.StartPos.Z.Value);

        //    if (ret == EventCodeEnum.NONE)
        //    {
        //        //바람 쏜다 
        //        ret = StageSuperVisor.StageModuleState.AirBlowAirOnOff(true);
        //        if (ret == EventCodeEnum.NONE)
        //        {
        //            //온도가 될때까지 무빙하면서 온도가 안떨어진다면 파라미터 세팅에 의해 멈춘다 
        //            stw.Start();
        //            while (runflag)
        //            {
        //                if (TempManager.TempControl.GetCurTemp <= ABDeviceFile.TargetTemp)
        //                {
        //                    ret = EventCodeEnum.NONE;
        //                    LoggerManager.Debug($String.Format("AirBlow ChuckTempControl()"));
        //                    runflag = false;
        //                }

        //                if (stw.Elapsed.Seconds >= ABDeviceFile.CheckTempTimeofSeconds)
        //                {
        //                    injury = true;
        //                    injurystw.Start();
        //                    stw.Reset();
        //                    stw.Stop();
        //                    prevTemp = TempManager.TempControl.GetCurTemp;
        //                }
        //                if (injury)
        //                {
        //                    if (TempManager.TempControl.GetCurTemp <= ABDeviceFile.TargetTemp)
        //                    {
        //                        ret = EventCodeEnum.NONE;
        //                        LoggerManager.Debug($String.Format("AirBlow ChuckTempControl()"));
        //                        runflag = false;
        //                    }
        //                    else
        //                    {
        //                        if (injurystw.Elapsed.Seconds >= ABDeviceFile.CheckTempInjuryTimeOfSeconds)
        //                        {
        //                            double offset = Math.Abs(TempManager.TempControl.GetCurTemp - (prevTemp));

        //                            if (offset <= ABDeviceFile.TempOffsetLimit)
        //                            {
        //                                runflag = false;
        //                                ret = EventCodeEnum.UNDEFINED;
        //                                LoggerManager.Error($"AirBlow ChuckTempControl() Temperature could not archived "));
        //                            }
        //                            else
        //                            {
        //                                prevTemp = TempManager.TempControl.GetCurTemp;
        //                                injurystw.Reset();
        //                                injurystw.Start();
        //                            }
        //                        }
        //                    }
        //                }

        //                if (runflag == true)
        //                {
        //                    ret = StageSuperVisor.StageModuleState.AirBlowMove(EnumAxisConstants.Y, cleaningpos, ABSysFile.AirBlowSpeed, ABSysFile.AirBlowAcc);
        //                    if (ret != EventCodeEnum.NONE)
        //                    {
        //                        ret = EventCodeEnum.MOTION_MOVING_ERROR;
        //                        LoggerManager.Error($"AirBlow ChuckTempControl() Moving Error"));
        //                        runflag = false;
        //                    }
        //                    ret = StageSuperVisor.StageModuleState.AirBlowMove(EnumAxisConstants.Y, ABSysFile.StartPos.Y.Value, ABSysFile.AirBlowSpeed, ABSysFile.AirBlowAcc);
        //                    if (ret != EventCodeEnum.NONE)
        //                    {
        //                        ret = EventCodeEnum.MOTION_MOVING_ERROR;
        //                        LoggerManager.Error($"AirBlow ChuckTempControl() Moving Error"));
        //                        runflag = false;
        //                    }

        //                }

        //            }

        //            //바람 멈춤
        //            ret = StageSuperVisor.StageModuleState.AirBlowAirOnOff(false);
        //            if (ret != EventCodeEnum.NONE)
        //            {
        //                ret = EventCodeEnum.IO_DEV_CONN_ERROR;
        //                LoggerManager.Error($"AirBlow ChuckTempControl() IO Error"));
        //            }
        //        }
        //        else
        //        {
        //            ret = EventCodeEnum.IO_DEV_CONN_ERROR;
        //            LoggerManager.Error($"AirBlow ChuckTempControl() IO Error"));
        //        }
        //    }
        //    else
        //    {
        //        ret = EventCodeEnum.MOTION_MOVING_ERROR;
        //        LoggerManager.Error($"AirBlow ChuckTempControl() Moving Error"));
        //    }
        //    return ret;
        //}
        public EventCodeEnum ChuckTempControl()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                double cleaningpos = ABSysFile.StartPos.Y.Value + ABSysFile.CleaningDistance.Value;
                double prevTemp = this.TempController().TempInfo.CurTemp.Value;
                bool runflag = true;
                bool injury = false;
                bool checkTemp = true;
                Stopwatch stw = new Stopwatch();
                Stopwatch injurystw = new Stopwatch();
                //Chuck이 AirBlow StartPostion으로 이동한다. 
                Task.Run(() =>
                {
                    while (checkTemp)
                    {
                        if (this.TempController().TempInfo.CurTemp.Value <= ABDeviceFile.TargetTemp.Value)
                        {
                            ret = this.StageSupervisor().StageModuleState.AirBlowAirOnOff(false);
                            if (ret == EventCodeEnum.NONE)
                            {
                                checkTemp = false;
                                runflag = false;
                                LoggerManager.Debug($"AirBlowIOOff.");
                            }
                            else
                            {
                                checkTemp = false;
                                runflag = false;
                                ret = EventCodeEnum.IO_DEV_CONN_ERROR;
                                LoggerManager.Error($"AirBlow ChuckTempControl() IO Error");
                            }
                        }
                    }
                });
                if (checkTemp == true)
                {
                    ret = this.StageSupervisor().StageModuleState.AirBlowMove(ABSysFile.StartPos.X.Value, ABSysFile.StartPos.Y.Value, ABSysFile.StartPos.Z.Value);
                    if (ret == EventCodeEnum.NONE)
                    {
                        //바람 쏜다 
                        ret = this.StageSupervisor().StageModuleState.AirBlowAirOnOff(true);
                        if (ret == EventCodeEnum.NONE)
                        {
                            //온도가 될때까지 무빙하면서 온도가 안떨어진다면 파라미터 세팅에 의해 멈춘다 
                            stw.Start();
                            while (runflag)
                            {
                                if (stw.Elapsed.Seconds >= ABDeviceFile.CheckTempTimeofSeconds.Value)
                                {
                                    injury = true;
                                    injurystw.Start();
                                    stw.Reset();
                                    stw.Stop();
                                    prevTemp = this.TempController().TempInfo.CurTemp.Value;
                                }
                                if (injury)
                                {
                                    if (this.TempController().TempInfo.CurTemp.Value <= ABDeviceFile.TargetTemp.Value)
                                    {
                                        ret = EventCodeEnum.NONE;
                                        LoggerManager.Debug($"AirBlow ChuckTempControl()");
                                        runflag = false;
                                    }
                                    else
                                    {
                                        if (injurystw.Elapsed.Seconds >= ABDeviceFile.CheckTempInjuryTimeOfSeconds.Value)
                                        {
                                            double offset = Math.Abs(this.TempController().TempInfo.CurTemp.Value - (prevTemp));

                                            if (offset <= ABDeviceFile.TempOffsetLimit.Value)
                                            {
                                                runflag = false;
                                                ret = EventCodeEnum.UNDEFINED;
                                                LoggerManager.Error($"AirBlow ChuckTempControl() Temperature could not archived ");
                                            }
                                            else
                                            {
                                                prevTemp = this.TempController().TempInfo.CurTemp.Value;
                                                injurystw.Reset();
                                                injurystw.Start();
                                            }
                                        }
                                    }
                                }

                                if (runflag == true)
                                {
                                    ret = this.StageSupervisor().StageModuleState.AirBlowMove(EnumAxisConstants.Y, cleaningpos, ABSysFile.AirBlowSpeed.Value, ABSysFile.AirBlowAcc.Value);
                                    if (ret != EventCodeEnum.NONE)
                                    {
                                        ret = EventCodeEnum.MOTION_MOVING_ERROR;
                                        LoggerManager.Error($"AirBlow ChuckTempControl() Moving Error");
                                        runflag = false;
                                    }
                                    ret = this.StageSupervisor().StageModuleState.AirBlowMove(EnumAxisConstants.Y, ABSysFile.StartPos.Y.Value, ABSysFile.AirBlowSpeed.Value, ABSysFile.AirBlowAcc.Value);
                                    if (ret != EventCodeEnum.NONE)
                                    {
                                        ret = EventCodeEnum.MOTION_MOVING_ERROR;
                                        LoggerManager.Error($"AirBlow ChuckTempControl() Moving Error");
                                        runflag = false;
                                    }

                                }
                            }

                            //바람 멈춤
                            ret = this.StageSupervisor().StageModuleState.AirBlowAirOnOff(false);
                            if (ret != EventCodeEnum.NONE)
                            {
                                ret = EventCodeEnum.IO_DEV_CONN_ERROR;
                                LoggerManager.Error($"AirBlow ChuckTempControl() IO Error");
                            }
                        }
                        else
                        {
                            ret = EventCodeEnum.IO_DEV_CONN_ERROR;
                            LoggerManager.Error($"AirBlow ChuckTempControl() IO Error");
                        }
                    }
                    else
                    {
                        ret = EventCodeEnum.MOTION_MOVING_ERROR;
                        LoggerManager.Error($"AirBlow ChuckTempControl() Moving Error");
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }
        #endregion

        public void SettingTempCallback(double prevSV, double nextSV, double nowTemp)
        {
            try
            {
                if (ABDeviceFile.AirBlowTempControlEnable.Value)
                {
                    if (nextSV < nowTemp && nextSV < prevSV)
                    {
                        InnerStateTransition(new AirBlowTempControlRunningState(this));
                        LoggerManager.Debug($"{GetType().Name}.StateTransition() : STATE={AirBlowTempControlState.GetState()}");

                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void EventFired(object sender, ProbeEventArgs e)
        {
            if (sender is AlarmSVChangedEvent)
            {
                if (e.Parameter != null)
                {
                    double prevSV = 0;
                    double value = 0;
                    double PV = 0;
                    if (e.Parameter is List<double>)
                    {
                        var a = e.Parameter as List<double>;
                        prevSV = a[0];
                        value = a[1];
                        PV = a[2];
                    }
                    SettingTempCallback(prevSV, value, PV);
                }
            }
        }

            public EventCodeEnum ClearState()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            return RetVal;
        }

        public bool CanExecute(IProbeCommandToken token)
        {
            throw new NotImplementedException();
        }

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

        public string GetModuleMessage()
        {
            throw new NotImplementedException();
        }
    }
}
