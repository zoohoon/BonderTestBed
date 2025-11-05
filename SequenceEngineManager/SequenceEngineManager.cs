using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace SequenceEngine
{
    using Autofac;
    using LoaderBase;
    using LogModule;
    using ProberErrorCode;
    using SecsGemServiceInterface;
    using System.Runtime.CompilerServices;

    public class SequenceEngineManager : ISequenceEngineManager
    {
        public bool Initialized { get; set; } = false;

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        public readonly object StateLockObject = new object();

        private int _JobServiceCount;

        public int JobServiceCount
        {
            get
            {
                return _JobServiceCount;
            }
            set
            {
                if (_JobServiceCount != value)
                {
                    _JobServiceCount = value;
                    RaisePropertyChanged();
                }
            }
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool isMovingState()
        {
            bool Retval = false;
            try
            {
                Retval = this.LotOPModule().RunList.All(
                    item =>
                    item.ModuleState.GetState() != ModuleStateEnum.RUNNING
                    );
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Retval;
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool GetRunState(bool CheckStage = true, bool condiserStatusSoaking = false, bool isWaferTransfer = false)
        {
            bool Retval = false;
            try
            {

                if (CheckStage)
                {
                    Retval = (this.StageSupervisor().GetStageInitState() == CellInitModeEnum.NormalEnd) ? true : false;
                }
                else
                {
                    Retval = true;
                }

                if (Retval == true)
                {
                    Retval = this.LotOPModule().RunList.All(
                        item =>
                        item.ModuleState.GetState() != ModuleStateEnum.RUNNING &&
                        item.ModuleState.GetState() != ModuleStateEnum.PENDING &&
                        item.ModuleState.GetState() != ModuleStateEnum.ERROR
                        //&&item.ModuleState.GetState() != ModuleStateEnum.RECOVERY
                        );

                    if (condiserStatusSoaking)
                    {
                        if (this.SoakingModule().StatusSoakingParamIF.Get_ShowStatusSoakingSettingPageToggleValue())
                        {
                            if (Retval)  //lot start를 하기전에 polish wafer가 Loader를 통해 이송중이라면 PW를 받고 Start가 될 수 있도록 한다.
                            {
                                if (this.LotOPModule().TransferReservationAboutPolishWafer)
                                {
                                    LoggerManager.SoakingLog($"Polish wafer(for soaking) is transferring. it can not 'Lot start'");
                                    Retval = false;
                                }
                            }
                            else
                            {
                                bool isEnableStatusSoaking = false;
                                this.SoakingModule().StatusSoakingParamIF.IsEnableStausSoaking(ref isEnableStatusSoaking);
                                if (isEnableStatusSoaking)
                                {
                                    var checkModuleList = this.LotOPModule().RunList.Where(x =>
                                    (x.ModuleState.GetState() == ModuleStateEnum.RUNNING ||
                                      x.ModuleState.GetState() == ModuleStateEnum.PENDING ||
                                      x.ModuleState.GetState() == ModuleStateEnum.ERROR)).ToList();

                                    if (1 == checkModuleList.Count)
                                    {
                                        var get_module = checkModuleList.FirstOrDefault();
                                        if (null != get_module)
                                        {
                                            if (get_module is ISoakingModule)
                                            {
                                                Retval = true;
                                                LoggerManager.SoakingLog($"'Lot Run' Start while Idle soaking.");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (Retval == true && isWaferTransfer == false && this.StageSupervisor().Get_TCW_Mode() == TCW_Mode.ON)
                    {
                        Retval = false;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Retval;
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool GetRunStateChangeMode()
        {
            bool Retval = false;
            try
            {


                Retval = this.LotOPModule().RunList.All(
                    item =>
                    item.ModuleState.GetState() != ModuleStateEnum.RUNNING &&
                    item.ModuleState.GetState() != ModuleStateEnum.PENDING
                    //&&item.ModuleState.GetState() != ModuleStateEnum.RECOVERY
                    );
                //if (Retval)
                //{
                //    Retval= this.LoaderOPModule().RunList.All(
                //               item =>
                //        item.ModuleState.GetState() != ModuleStateEnum.RUNNING &&
                //        item.ModuleState.GetState() != ModuleStateEnum.PENDING &&
                //        item.ModuleState.GetState() != ModuleStateEnum.ERROR &&
                //        item.ModuleState.GetState() != ModuleStateEnum.RECOVERY
                //        );
                //}

                //Command Slot 카운트 체크
                //if (Retval == true)
                //{
                //    Retval = this.LotOPModule().RunList.All(item => item.CommandReceiveSlot.Token is NoCommand);
                //}
                // Check Loader

                //if (Retval == true)
                //{
                //    Retval = !(LoaderController.ModuleState.GetState() == ModuleStateEnum.RUNNING);

                //}

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Retval;
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool GetEndReadyState()
        {
            bool Retval = false;
            try
            {



                Retval = this.LotOPModule().RunList.All(
                    item =>
                    item.ModuleState.GetState() != ModuleStateEnum.RUNNING &&
                    item.ModuleState.GetState() != ModuleStateEnum.PENDING &&
                    item.ModuleState.GetState() != ModuleStateEnum.ERROR &&
                    item.ModuleState.GetState() != ModuleStateEnum.SUSPENDED
                    );



            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Retval;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool IsDoPolish()
        {
            bool Retval = false;
            try
            {

                if (this.PolishWaferModule().ModuleState.State == ModuleStateEnum.SUSPENDED || this.PolishWaferModule().ModuleState.State == ModuleStateEnum.RUNNING)
                {
                    Retval = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Retval;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool GetRunState(IStateModule excludeModule)
        {
            bool Retval = true;
            try
            {


                if (Retval == true)
                {
                    Retval = this.LotOPModule().RunList.All(
                        item =>
                        item != excludeModule && (
                        item.ModuleState.GetState() != ModuleStateEnum.RUNNING &&
                        item.ModuleState.GetState() != ModuleStateEnum.PENDING &&
                        item.ModuleState.GetState() != ModuleStateEnum.ERROR)
                        //&&item.ModuleState.GetState() != ModuleStateEnum.RECOVERY
                        );
                    //if (Retval)
                    //{
                    //    Retval= this.LoaderOPModule().RunList.All(
                    //               item =>
                    //        item.ModuleState.GetState() != ModuleStateEnum.RUNNING &&
                    //        item.ModuleState.GetState() != ModuleStateEnum.PENDING &&
                    //        item.ModuleState.GetState() != ModuleStateEnum.ERROR &&
                    //        item.ModuleState.GetState() != ModuleStateEnum.RECOVERY
                    //        );
                    //}
                }
                //Command Slot 카운트 체크
                //if (Retval == true)
                //{
                //    Retval = this.LotOPModule().RunList.All(item => item.CommandReceiveSlot.Token is NoCommand);
                //}
                // Check Loader

                //if (Retval == true)
                //{
                //    Retval = !(LoaderController.ModuleState.GetState() == ModuleStateEnum.RUNNING);

                //}

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Retval;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool GetMovingState()
        {
            bool Retval = false;
            try
            {


                Retval = this.LotOPModule().RunList.All(
                    item =>
                    item.ModuleState.GetState() != ModuleStateEnum.RUNNING
                    );

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Retval;
        }

        private int _EventServiceCount;

        public int EventServiceCount
        {
            get
            {
                return _EventServiceCount;
            }
            set
            {
                if (_EventServiceCount != value)
                {
                    _EventServiceCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool IsSequenceRunning = false;

        private List<ISequenceEngineService> _Services = new List<ISequenceEngineService>();
        public List<ISequenceEngineService> Services
        {
            get { return _Services; }
            set
            {
                if (value != _Services)
                {
                    _Services = value;
                    RaisePropertyChanged();
                }
            }
        }

        public SequenceEngineManager()
        {

        }


        public void AddSpecificSequence(ISequenceEngineService module, string threadname)
        {
            try
            {
                ISequenceEngineService service = module;
                service.ThreadName = "threadname";
                Services.Add(service);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private EventCodeEnum AddSequencesForStage()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                ISequenceEngineService service = null;

                service = this.LotOPModule() as ISequenceEngineService;
                if (service != null)
                {
                    service.ThreadName = "LotOPModule";
                    Services.Add(service);
                }

                service = this.LoaderOPModule() as ISequenceEngineService;
                if (service != null)
                {
                    service.ThreadName = "LoaderOPModule";
                    Services.Add(service);
                }

                service = this.EventExecutor() as ISequenceEngineService;
                if (service != null)
                {
                    service.ThreadName = "EventExecutor";
                    Services.Add(service);

                    RunSequence(service, "EventExecutor");
                }

                service = this.FoupOpModule() as ISequenceEngineService;
                if (service != null)
                {
                    service.ThreadName = "FoupOpModule";
                    Services.Add(service);
                }

                service = this.TempController() as ISequenceEngineService;
                if (service != null)
                {
                    service.ThreadName = "TempController";
                    Services.Add(service);
                }

                service = this.LampManager() as ISequenceEngineService;
                if (service != null)
                {
                    service.ThreadName = "LampManager";
                    Services.Add(service);
                }

                service = this.SequenceRunner() as ISequenceEngineService;
                if (service != null)
                {
                    service.ThreadName = "SequenceRunner";
                    Services.Add(service);
                }

                service = this.GPIB() as ISequenceEngineService;
                if (service != null)
                {
                    if (this.GPIB().GetGPIBEnable() == EnumGpibEnable.ENABLE)
                    {
                        service.ThreadName = "GPIB";
                        Services.Add(service);
                    }
                }

                service = this.TCPIPModule() as ISequenceEngineService;
                if (service != null)
                {
                    if (this.TCPIPModule().GetTCPIPOnOff() == EnumTCPIPEnable.ENABLE)
                    {
                        service.ThreadName = "TCPIP";
                        Services.Add(service);
                    }
                }
                service = this.CardChangeModule() as ISequenceEngineService;
                if (service != null)
                {
                    service.ThreadName = "CardChangeModule";
                    Services.Add(service);
                }

                service = this.SignalTowerManager() as ISequenceEngineService;
                if (service != null)
                {
                    service.ThreadName = "SignalTowerManager";
                    Services.Add(service);
                }



                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        private EventCodeEnum AddSequencesForLoader()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                ISequenceEngineService service = null;

                service = this.EventExecutor() as ISequenceEngineService;
                if (service != null)
                {
                    service.ThreadName = "EventExecutor";
                    Services.Add(service);

                    RunSequence(service, "EventExecutor");
                }

                // TODO :
                service = this.FoupOpModule() as ISequenceEngineService;
                if (service != null)
                {
                    service.ThreadName = "FoupOpModule";
                    Services.Add(service);
                }

                // Maestro는 조건에 따라, Thread 생성을 하지 않는 로직은 존재해서는 안된다.
                // 위 컨셉에 맞춰, 아래의 로직이 수정되었음. 

                //var IsCardChangeActionAllowed = this.GEMModule().CanExecuteRemoteAction(EnumRemoteCommand.START_CARD_CHANGE.ToString());

                //if(IsCardChangeActionAllowed)
                //{
                service = this.GetLoaderContainer()?.Resolve<ICardChangeSupervisor>() as ISequenceEngineService;
                if (service != null)
                {
                    service.ThreadName = "CardChangeSupervisor";
                    Services.Add(service);
                }
                //}

                service = this.SignalTowerManager() as ISequenceEngineService;
                if (service != null)
                {
                    service.ThreadName = "SignalTowerManager";
                    Services.Add(service);
                }

                //var IsWaferChangeActionAllowed = this.GEMModule().CanExecuteRemoteAction(EnumRemoteCommand.WAFER_CHANGE.ToString());

                //if(IsWaferChangeActionAllowed)
                //{
                service = this.GetLoaderContainer()?.Resolve<IWaferChangeSupervisor>() as ISequenceEngineService;
                if (service != null)
                {
                    service.ThreadName = "WaferChangeSupervisor";
                    Services.Add(service);
                }
                //}

                service = this.EnvMonitoringManager() as ISequenceEngineService;
                if (service != null)
                {
                    service.ThreadName = "EnvMonitoringManager";
                    Services.Add(service);
                }

                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        // 특정 Thread 종료 함수.

        public EventCodeEnum StopSequence(ISequenceEngineService service)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                var ret = Services.Find(x => x == service);

                if (ret != null)
                {
                    Services.Remove(service);
                }

                service.StopSequencer();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return RetVal;
        }

        // 특정 Thread 실행 함수
        public EventCodeEnum RunSequence(ISequenceEngineService service, string threadname)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                // List를 통한 Sequence 관리를 위해, 리스트에 존재하지 않는 경우 추가

                var ret = Services.Find(x => x == service);

                if (ret == null)
                {
                    service.ThreadName = threadname;
                    Services.Add(service);
                }

                service.RunSequencer();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return RetVal;
        }

        public EventCodeEnum RunSequences()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (IsSequenceRunning == false)
                {
                    foreach (ISequenceEngineService item in Services)
                    {
                        item.RunSequencer();
                    }

                    IsSequenceRunning = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    if (AppDomain.CurrentDomain.FriendlyName == "ProberSystem.exe")
                    {
                        retval = AddSequencesForStage();
                    }
                    else if (AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")
                    {
                        retval = AddSequencesForLoader();
                    }
                    else if(AppDomain.CurrentDomain.FriendlyName == "BonderSystem.exe")
                    {
                        // 250908 LJH 조건식 추가
                        retval = AddSequencesForStage();
                    }
                    else
                    {
                        LoggerManager.Error($"[SequenceEngineManager] InitModule() : Unknown");
                    }

                    //Services = new List<ISequenceEngineService>();
                    //retval = AddSequencesForStage();
                    //RetVal = RunSequences(); //Run은 ProberSystem MainWindow Init에서 함.

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
                //LoggerManager.Error($err, "InitModule Error occurred.");
                LoggerManager.Exception(err);
                throw;
            }

            return retval;
        }

        public void DeInitModule()
        {
            try
            {
                foreach (var thread in Services)
                {
                    thread.StopSequencer();
                }

                IsSequenceRunning = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public object GetLockObject()
        {
            return StateLockObject;
        }
    }
}
