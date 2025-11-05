using ProberInterfaces;
using ProberInterfaces.AirCooling;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using SequenceService;
using ProberInterfaces.Command;
using ProberErrorCode;

using ProberInterfaces.State;
using ProberInterfaces.Wizard;
using LogModule;
using System.Runtime.CompilerServices;

namespace AirCoolingMoodule
{
    public class AirCoolingModule : SequenceServiceBase, IAirCoolingModule, INotifyPropertyChanged, IHasDevParameterizable, IHasSysParameterizable
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected override void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        private CommandInformation _CommandInfo;
        public CommandInformation CommandInfo
        {
            get { return _CommandInfo; }
            set { _CommandInfo = value; }
        }
        
        private ReasonOfError _ReasonOfError = new ReasonOfError(ModuleEnum.AirCooling);
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

        private AirCoolingDeviceFile _AirCoolingDevFile;

        public AirCoolingDeviceFile AirCoolingDevFile
        {
            get { return _AirCoolingDevFile; }
            set { _AirCoolingDevFile = value; }
        }
        private AirCoolingSystemFile _AirCoolingSysFile;

        public AirCoolingSystemFile AirCoolingSysFile
        {
            get { return _AirCoolingSysFile; }
            set { _AirCoolingSysFile = value; }
        }
        private AirCoolingStateBase _AirCoolingState;
        public AirCoolingStateBase AirCoolingState
        {
            get { return _AirCoolingState; }
        }

        public IInnerState InnerState
        {
            get { return _AirCoolingState; }
            set
            {
                if (value != _AirCoolingState)
                {
                    _AirCoolingState = value as AirCoolingStateBase;
                }
            }
        }

        public IInnerState PreInnerState { get; set; }



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
        AutoResetEvent areUpdateCheckTempIntv = new AutoResetEvent(false);
        AutoResetEvent areUpdatActionIntv = new AutoResetEvent(false);
        System.Threading.Timer _monitoringTimer;

        #endregion


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

        public string GetModuleMessage()
        {
            string retval = string.Empty;

            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
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
        private void AreUpdateSet(object obj)
        {
            try
            {
            areUpdateCheckTempIntv.Set();
            areUpdatActionIntv.Set();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
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

                    if (AirCoolingSysFile.AirCoolingOnProber.Value == true)
                    {
                        _AirCoolingState = new AirCoolingIdleState(this);
                        ModuleState = new ModuleIdleState(this);
                    }
                    else
                    {
                        _AirCoolingState = new AirCoolingDoneState(this);
                        ModuleState = new ModuleDoneState(this);
                    }

                    _monitoringTimer = new Timer(new TimerCallback(AreUpdateSet), null, 100, 100);

                    this.ThreadName = this.GetType().Name;

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

                _monitoringTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _monitoringTimer.Dispose();
                _monitoringTimer = null;

                this.StopSequencer();
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
            retVal=InnerState.ClearState();
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
            catch(Exception err)
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
            catch(Exception err)
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
            catch(Exception err)
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
            catch(Exception err)
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
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

            IParam tmpParam = null;
            tmpParam = new AirCoolingDeviceFile();
            tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
            RetVal = this.LoadParameter(ref tmpParam, typeof(AirCoolingDeviceFile));

            if (RetVal == EventCodeEnum.NONE)
            {
                AirCoolingDevFile = tmpParam as AirCoolingDeviceFile;
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
            try
            {
                return SaveACDeviceFile();
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

            IParam tmpParam = null;
            tmpParam = new AirCoolingSystemFile();
            tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
            RetVal = this.LoadParameter(ref tmpParam, typeof(AirCoolingSystemFile));

            if (RetVal == EventCodeEnum.NONE)
            {
                AirCoolingSysFile = tmpParam as AirCoolingSystemFile;
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
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = this.SaveParameter(AirCoolingSysFile);
                if (RetVal != EventCodeEnum.NONE)
                {
                    throw new Exception($"[{this.GetType().Name} - SaveSysParameter] Faile SaveParameter");
                }
            }
            catch (Exception err)
            {

                RetVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($String.Format("[AirCooling] LoadDevParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);

            }
            return RetVal;
        }
      
        public EventCodeEnum LoadAirCoolingDeviceFile()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
            AirCoolingDevFile = new AirCoolingDeviceFile();

            string fullPath = this.FileManager().GetDeviceParamFullPath(AirCoolingDevFile.FilePath, AirCoolingDevFile.FileName);

            try
            {
                IParam tmpParam = null;
                RetVal = this.LoadParameter(ref tmpParam, typeof(AirCoolingDeviceFile), null, fullPath);
                if (RetVal == EventCodeEnum.NONE)
                {
                    AirCoolingDevFile = tmpParam as AirCoolingDeviceFile;
                }
            }
            catch (Exception err)
            {

                RetVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($String.Format("[AirCooling] LoadDevParam(): Error occurred while loading parameters. Err = {0}", err.Message));
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
        public EventCodeEnum SaveACDeviceFile()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

            string fullPath = this.FileManager().GetDeviceParamFullPath(AirCoolingDevFile.FilePath, AirCoolingDevFile.FileName);

            try
            {
                RetVal = Extensions_IParam.SaveParameter(null, AirCoolingDevFile, null, fullPath);
                if (RetVal != EventCodeEnum.NONE)
                {
                    throw new Exception($"[{this.GetType().Name} - SaveACDeviceFile] Faile SaveParameter");
                }
            }
            catch (Exception err)
            {

                RetVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($String.Format("[AirCooling] LoadDevParam(): Error occurred while loading parameters. Err = {0}", err.Message));
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

        public EventCodeEnum LoadAirCoolingSysFile()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

            AirCoolingSysFile = new AirCoolingSystemFile();

            string FullPath = this.FileManager().GetSystemParamFullPath(AirCoolingSysFile.FilePath, AirCoolingSysFile.FileName);

            try
            {
                IParam tmpParam = null;
                RetVal = this.LoadParameter(ref tmpParam, typeof(AirCoolingSystemFile), null, FullPath);
                if (RetVal == EventCodeEnum.NONE)
                {
                    AirCoolingSysFile = tmpParam as AirCoolingSystemFile;
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($String.Format("[AirCooling] LoadSysParam(): Error occurred while loading parameters. Err = {0}", err.Message));
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
        public EventCodeEnum AirCoolingFunc()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
            bool checkTemp = true;
            bool chuckairon;
            bool tempcontrolrun = true;
            Stopwatch activatestopwatch = new Stopwatch();
            Stopwatch deactivatestopwatch = new Stopwatch();

            Task.Run(() =>
            {
                while (checkTemp)
                {
                    if (this.TempController().TempInfo.CurTemp.Value <= AirCoolingDevFile.TargetTemp.Value)
                    {
                        var io = this.IOManager().IOServ.WriteBit(
                this.IOManager().IO.Outputs.DOCHUCKCOOLAIRON, false);
                        if (io == IORet.NO_ERR)
                        {
                            chuckairon = false;
                            checkTemp = false;
                            tempcontrolrun = false;
                            LoggerManager.Debug($"ChuckAirOff ");
                        }
                        else
                        {
                            chuckairon = false;
                            checkTemp = false;
                            tempcontrolrun = false;
                            LoggerManager.Error($"ChuckAirOff Error");
                        }
                        areUpdateCheckTempIntv.WaitOne();
                    }

                }
            });

            var ioret = this.IOManager().IOServ.WriteBit(
                this.IOManager().IO.Outputs.DOCHUCKCOOLAIRON, true);
            if (ioret == IORet.NO_ERR)
            {
                chuckairon = true;
                activatestopwatch.Start();
                    while (tempcontrolrun)
                    {
                        if (AirCoolingDevFile.AirActivatingTime.Value == 0 && AirCoolingDevFile.AirDeActivatingTime.Value == 0)
                        {
                            if (chuckairon == true)
                            {
                                // Infinite Turn on 
                            }
                            else
                            {
                                IORet ioretVal = this.IOManager().IOServ.WriteBit(
                                                    this.IOManager().IO.Outputs.DOCHUCKCOOLAIRON, true);
                                if (ioretVal == IORet.NO_ERR)
                                {
                                    LoggerManager.Debug($"ChuckAirOn ");
                                    chuckairon = true;
                                }
                            }
                        }
                        else
                        {
                            if (activatestopwatch.Elapsed.Seconds >= AirCoolingDevFile.AirActivatingTime.Value)
                            {
                                activatestopwatch.Stop();
                                activatestopwatch.Reset();
                                IORet ioretVal = this.IOManager().IOServ.WriteBit(
                    this.IOManager().IO.Outputs.DOCHUCKCOOLAIRON, false);
                                if (ioretVal == IORet.NO_ERR)
                                {
                                    deactivatestopwatch.Start();
                                }
                                else
                                {
                                    chuckairon = false;
                                    checkTemp = false;
                                    tempcontrolrun = false;
                                    LoggerManager.Error($"ChuckAirOff Error");
                                }
                            }
                            if (deactivatestopwatch.Elapsed.Seconds >= AirCoolingDevFile.AirDeActivatingTime.Value)
                            {
                                deactivatestopwatch.Stop();
                                deactivatestopwatch.Reset();
                                IORet ioretVal = this.IOManager().IOServ.WriteBit(
                    this.IOManager().IO.Outputs.DOCHUCKCOOLAIRON, true);
                                if (ioretVal == IORet.NO_ERR)
                                {
                                    activatestopwatch.Start();
                                }
                                else
                                {
                                    chuckairon = false;
                                    checkTemp = false;
                                    tempcontrolrun = false;
                                    LoggerManager.Error($"ChuckAirOn Error");
                                }
                            }
                        }
                        areUpdatActionIntv.WaitOne();
                    }
                    if (tempcontrolrun == false)
                    {
                        IORet ioretVal = this.IOManager().IOServ.WriteBit(
                                            this.IOManager().IO.Outputs.DOCHUCKCOOLAIRON, false);
                        LoggerManager.Debug($"ChuckAirOff");
                    }

            }
            else
            {
                LoggerManager.Error($"ChuckCoolAirOn Error");
                ret = EventCodeEnum.IO_DEV_CONN_ERROR;
            }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return ret;
        }

        public override ModuleStateEnum SequenceRun()
        {
            ModuleStateEnum RetVal = ModuleStateEnum.UNDEFINED;
            try
            {

            RetVal = InnerState.GetModuleState();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return RetVal;
        }

     
        public bool CanExecute(IProbeCommandToken token)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum InnerStateTransition(IInnerState state)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

            PreInnerState = _AirCoolingState;
            InnerState = state;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
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
    }
}
