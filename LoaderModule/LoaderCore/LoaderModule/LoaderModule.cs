using Autofac;
using LoaderBase;
using LoaderParameters;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Foup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;


namespace LoaderCore
{
    using GPLoaderRouter;
    using LoaderBase.AttachModules.ModuleInterfaces;
    using LoaderBase.FactoryModules.ViewModelModule;
    using LoaderMapView;
    using LoaderParameters.Data;
    using LoaderServiceBase;
    using LoaderStates;
    using LogModule;
    using MetroDialogInterfaces;
    using NotifyEventModule;
    using ProberInterfaces.Event;
    using ProberInterfaces.Loader;
    using ProberInterfaces.PreAligner;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Text;
    using System.Windows;
    using WinAPIWrapper;

    ////using ProberInterfaces.ThreadSync;

    public class LoaderModule : ILoaderModule, IHasSysParameterizable, IFactoryModule, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public PropertyChangedEventHandler propertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged
        {
            add { this.propertyChanged += value; }
            remove { this.propertyChanged -= value; }
        }
        #endregion

        public LoaderStateBase StateObj { get; set; }
        public LoaderStateBase PreStateObj { get; set; }

        //private LockKey syncObj = new LockKey("Loader Module");
        private static object syncObj = new object();
        private static object broadCastObj = new object();

        //public Autofac.IContainer Container { get; set; }
        public Autofac.IContainer Container => this.GetLoaderContainer();
        public ILoaderViewModelManager LoaderViewModelManager => Container.Resolve<ILoaderViewModelManager>();

        public ILoaderService LoaderService { get; set; }
        public ILoaderMapConvert LoaderMapConvert { get; set; }
        public ILoaderMapConvert TopBar { get; set; }
        public ILoaderSupervisor LoaderMaster { get; set; }
        private LoaderServiceTypeEnum _ServiceType;
        public LoaderServiceTypeEnum ServiceType
        {
            get { return _ServiceType; }
            set { _ServiceType = value; }
        }
        private string _ResonOfError;
        public string ResonOfError
        {
            get { return _ResonOfError; }
            set { _ResonOfError = value; }
        }
        private string _ErrorDetails;
        public string ErrorDetails
        {
            get { return _ErrorDetails; }
            set { _ErrorDetails = value; }
        }
        private string _RecoveryBehavior;
        public string RecoveryBehavior
        {
            get { return _RecoveryBehavior; }
            set { _RecoveryBehavior = value; }
        }

        private int _RecoveryCellIdx;
        public int RecoveryCellIdx
        {
            get { return _RecoveryCellIdx; }
            set { _RecoveryCellIdx = value; }
        }

        private bool _IsInitialized;
        public bool IsInitialized
        {
            get { return _IsInitialized; }
            set { _IsInitialized = value; }
        }

        private string _RootParamPath;
        public string RootParamPath
        {
            get { return _RootParamPath; }
            set { _RootParamPath = value; }
        }

        private bool _PauseFlag;
        public bool PauseFlag
        {
            get { return _PauseFlag; }
            set { _PauseFlag = value; }
        }

        private bool _ResumeFlag;
        public bool ResumeFlag
        {
            get { return _ResumeFlag; }
            set { _ResumeFlag = value; }
        }

        private bool _LoaderRobotRunning;
        public bool LoaderRobotRunning
        {
            get { return _LoaderRobotRunning; }
            set { _LoaderRobotRunning = value; }
        }

        private bool _LoaderRobotAbortFlag;
        public bool LoaderRobotAbortFlag
        {
            get { return _LoaderRobotAbortFlag; }
            set { _LoaderRobotAbortFlag = value; }
        }

        private LoaderSystemParameter _SystemParameter;
        public LoaderSystemParameter SystemParameter
        {
            get { return _SystemParameter; }
            set { _SystemParameter = value; }
        }
        private LoaderOCRConfigParam _OCRConfig;
        public LoaderOCRConfigParam OCRConfig
        {
            get { return _OCRConfig; }
            set { _OCRConfig = value; }
        }


        private LoaderDeviceParameter _DeviceParameter;
        public LoaderDeviceParameter DeviceParameter
        {
            get { return _DeviceParameter; }
            set { _DeviceParameter = value; }
        }

        private List<ISlotModule> _PrevSlotInfo = new List<ISlotModule>();
        public List<ISlotModule> PrevSlotInfo
        {
            get { return _PrevSlotInfo; }
            set { _PrevSlotInfo = value; }
        }
        private bool _CenteringTestFlag = false;

        public bool CenteringTestFlag
        {
            get { return _CenteringTestFlag; }
            set { _CenteringTestFlag = value; }
        }

        private LoaderTestOption _LoaderOption;
        public LoaderTestOption LoaderOption
        {
            get { return _LoaderOption; }
            set { _LoaderOption = value; }
        }


        private LoaderProcModuleInfo _ProcModuleInfo;
        public LoaderProcModuleInfo ProcModuleInfo
        {
            get { return _ProcModuleInfo; }
            set { _ProcModuleInfo = value; }
        }
        private IDeviceManager _DeviceManager;

        private int _ChuckNumber;
        public int ChuckNumber
        {
            get { return _ChuckNumber; }
            set { _ChuckNumber = value; }
        }

        private int _ScanCount;
        public int ScanCount
        {
            get { return _ScanCount; }
            set { _ScanCount = value; }
        }
        private int _EmulScanCount;
        public int EmulScanCount
        {
            get { return _EmulScanCount; }
            set { _EmulScanCount = value; }
        }

        private bool[] _ScanFlag = new bool[3];
        public bool[] ScanFlag
        {
            get { return _ScanFlag; }
            set { _ScanFlag = value; }
        }

        private ObservableCollection<FoupFlag> _FoupScanFlag = new ObservableCollection<FoupFlag>();
        public ObservableCollection<FoupFlag> FoupScanFlag
        {
            get { return _FoupScanFlag; }
            set { _FoupScanFlag = value; }
        }
        public Autofac.IContainer StageContainer { get; private set; }


        private bool _GP_ManualOCREnable;
        public bool GP_ManualOCREnable
        {
            get
            {
                return _GP_ManualOCREnable;
            }
            set
            {
                if (value != _GP_ManualOCREnable)
                {
                    _GP_ManualOCREnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<int> _AlreadyAssignedStages = new List<int>();
        public List<int> AlreadyAssignedStages
        {
            get { return _AlreadyAssignedStages; }
            set
            {
                if (value != _AlreadyAssignedStages)
                {
                    _AlreadyAssignedStages = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _AlreadyAssignedCassette;
        public int AlreadyAssignedCassette
        {
            get { return _AlreadyAssignedCassette; }
            set
            {
                if (value != _AlreadyAssignedCassette)
                {
                    _AlreadyAssignedCassette = value;
                    RaisePropertyChanged();
                }
            }
        }



        public DateTime statecheckbasetime = DateTime.Now;

        public bool ExceedRunningStateDuration()
        {
            bool bret = false;
            try
            {
                string loaderState = StateObj.GetType().Name;
                List<string> checkStates = new List<string>() { "PROCESSING", "SCHEDULING", "SUSPENDED" };
                int index = checkStates.FindIndex(x => x == loaderState);
                CheckManualinputDialog();
                if (index >= 0)
                {
                    if ((DateTime.Now.Ticks - statecheckbasetime.Ticks) > (TimeSpan.TicksPerMinute * LoaderMaster.GetExecutionTimeoutError()))
                    {
                        statecheckbasetime = DateTime.Now;
                        LoggerManager.Debug($"abnormal loader state (occurred {LoaderMaster.GetExecutionTimeoutError()} minute hang)");
                        bret = true;
                    }
                }
                else
                {
                    statecheckbasetime = DateTime.Now;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return bret;
        }

        private void CheckManualinputDialog()
        {
            try
            {
                string[] windowTitles = { "WaferIDManualInput", "CognexManualInput", "CardIDManualInput" };
                foreach (string title in windowTitles)
                {
                    bool isOpenManualinputDialog = Win32APIWrapper.CheckWindowExists(title);

                    if (isOpenManualinputDialog)
                    {
                        statecheckbasetime = DateTime.Now;
                        break;
                    }
                    else
                    {
                        //Dialog 없음
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private ModuleStateEnum _ModuleState;
        public ModuleStateEnum ModuleState
        {
            get
            {
                return StateObj.ModuleState;
            }
            set
            {
                if (value != _ModuleState)
                {
                    statecheckbasetime = DateTime.Now;
                    _ModuleState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private SubstrateSizeEnum _DeviceSize;
        public SubstrateSizeEnum DeviceSize
        {
            get
            {
                return _DeviceSize;
            }
            set
            {
                if (value != _DeviceSize)
                {
                    _DeviceSize = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<FoupObject> _Foups = new ObservableCollection<FoupObject>();
        public ObservableCollection<FoupObject> Foups
        {
            get { return _Foups; }
            set
            {
                if (value != _Foups)
                {
                    _Foups = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<LoaderJobViewData> _LoaderJobViewList = new ObservableCollection<LoaderJobViewData>();
        public ObservableCollection<LoaderJobViewData> LoaderJobViewList
        {
            get { return _LoaderJobViewList; }
            set
            {
                if (value != _LoaderJobViewList)
                {
                    _LoaderJobViewList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<LoaderJobViewList> _LoaderJobCollection = new ObservableCollection<LoaderJobViewList>();
        public ObservableCollection<LoaderJobViewList> LoaderJobCollection
        {
            get { return _LoaderJobCollection; }
            set
            {
                if (value != _LoaderJobCollection)
                {
                    _LoaderJobCollection = value;
                    RaisePropertyChanged();
                }
            }
        }
        public EventCodeEnum LoaderJobSorting()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                Dictionary<string, LoaderJobViewList> loaderJobDic = new Dictionary<string, LoaderJobViewList>();

                LoaderJobCollection.Clear();
                if (LoaderJobViewList.Count > 0)
                {
                    foreach (var job in LoaderJobViewList)
                    {
                        if (loaderJobDic.ContainsKey(job.OriginHolder.ToString()))
                        {
                            loaderJobDic[job.OriginHolder.ToString()].LoaderJobList.Add(job);
                        }
                        else
                        {
                            LoaderJobViewList jobList = new LoaderJobViewList();
                            jobList.LoaderJobList.Add(job);
                            jobList.Origin = job.OriginHolder;
                            loaderJobDic.Add(job.OriginHolder.ToString(), jobList);
                        }
                    }
                    foreach (var loaderJobList in loaderJobDic.Values)
                    {
                        loaderJobList.LoaderJobList.Add(new LoaderJobViewData(loaderJobList.LoaderJobList[loaderJobList.LoaderJobList.Count - 1].DstHolder, loaderJobList.LoaderJobList[loaderJobList.LoaderJobList.Count - 1].DstHolder, loaderJobList.LoaderJobList[loaderJobList.LoaderJobList.Count - 1].OriginHolder));
                        LoaderJobCollection.Add(loaderJobList);
                    }


                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum LoaderJobViewerDone()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                Func<ModuleID, ModuleID, bool> isSameHolder = (ModuleID source, ModuleID target) =>
                {
                    bool ret = false;
                    if (source.ModuleType == ModuleTypeEnum.COGNEXOCR ||
                       source.ModuleType == ModuleTypeEnum.PA ||
                       target.ModuleType == ModuleTypeEnum.COGNEXOCR ||
                       target.ModuleType == ModuleTypeEnum.PA)
                    {
                        if (source.Index == target.Index &&
                           (source.ModuleType == ModuleTypeEnum.PA || source.ModuleType == ModuleTypeEnum.COGNEXOCR) &&
                           (target.ModuleType == ModuleTypeEnum.PA || target.ModuleType == ModuleTypeEnum.COGNEXOCR))
                        {
                            ret = true;
                        }
                    }
                    else
                    {
                        ret = source == target;
                    }

                    return ret;
                };

                if (LoaderJobViewList.Count > 0)
                {
                    var lockobj = new object();
                    lock (lockobj)
                    {
                        var loaderJobs = LoaderJobViewList.Where(i => isSameHolder(i.CurrentHolder, ProcModuleInfo.Source) &&
                                                                  isSameHolder(i.DstHolder, ProcModuleInfo.Destnation) &&
                                                                 isSameHolder(ProcModuleInfo.Origin, ProcModuleInfo.Origin) &&
                                                                 i.JobDone == false);//.FirstOrDefault();
                        if (loaderJobs != null)
                        {
                            foreach (var loaderJob in loaderJobs)
                            {
                                int jobindex = LoaderJobViewList.IndexOf(loaderJob);

                                loaderJob.JobDone = true;
                                loaderJob.isRunning = false;

                                LoggerManager.Debug($"LoaderJobViewerDone(): " +
                                    $"Job({jobindex}) Done Info: Source({ProcModuleInfo.Source.ModuleType}.{ProcModuleInfo.Source.Index}), " +
                                    $"Target({ProcModuleInfo.Destnation.ModuleType}.{ProcModuleInfo.Destnation.Index})");
                            }

                        }
                        else
                        {
                            LoggerManager.Debug($"LoaderJobViewerDone(): loaderJob is null.");
                            LoggerManager.Debug($"LoaderJobViewerDone(): " +
                                $"CurProcModule Info: Source({ProcModuleInfo.Source.ModuleType}.{ProcModuleInfo.Source.Index}), " +
                                $"Target({ProcModuleInfo.Destnation.ModuleType}.{ProcModuleInfo.Destnation.Index})");
                            foreach (var job in LoaderJobViewList)
                            {
                                LoggerManager.Debug($"LoaderJobViewerDone(): " +
                               $"LoaderJobViewList Info: Source({job.CurrentHolder.ModuleType}.{job.CurrentHolder.Index}), " +
                               $"Target({job.DstHolder.ModuleType}.{job.DstHolder.Index}) : {job.JobDone}");
                            }
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

        public void FoupInfoUpdate()
        {
            foreach (var foup in Foups)
            {

                foreach (var slot in foup.Slots)
                {
                    slot.WaferStatus = EnumSubsStatus.NOT_EXIST;
                    if (slot.WaferObj != null)
                    {
                        slot.WaferStatus = EnumSubsStatus.EXIST;
                    }
                }
            }
        }
        public ILoaderServiceCallback ServiceCallback { get; set; }


        #region => External Modules
        public IMotionManagerProxy MotionManager => Container.Resolve<IMotionManagerProxy>();

        public IIOManagerProxy IOManager => Container.Resolve<IIOManagerProxy>();

        public IVisionManagerProxy VisionManager => Container.Resolve<IVisionManagerProxy>();

        public ILightProxy Light => Container.Resolve<ILightProxy>();

        public IPAManager PAManager => Container.Resolve<IPAManager>();

        private IFoupOpModule _Foup;

        public IFoupOpModule Foup
        {
            get
            {
                if (_Foup == null)
                {
                    _Foup = Container.Resolve<IFoupOpModule>();
                }
                return _Foup;
            }
        }

        #endregion


        public IDeviceManager DeviceManager { get; set; }
        public INotifyManager NotifyManager { get; set; }
        public IEnvControlManager EnvControlManager { get; set; }
        public ICardChangeSupervisor CardChangeSupervisor { get; set; }

        #region => Management Modules
        public ILoaderMove Move => Container.Resolve<ILoaderMove>();

        public IModuleManager ModuleManager => Container.Resolve<IModuleManager>();

        public IWaferTransferRemoteService WaferTransferRemoteService => Container.Resolve<IWaferTransferRemoteService>();
        public ICardTransferRemoteService CardTransferRemoteService => Container.Resolve<ICardTransferRemoteService>();
        public IOCRRemoteService OCRRemoteService => Container.Resolve<IOCRRemoteService>();

        public ILoaderSequencer Sequencer => Container.Resolve<ILoaderSequencer>();
        public ILoaderMapSlicer MapSlicer => Container.Resolve<ILoaderMapSlicer>();

        public Queue<IWaferOwnable> UnknownModule = new Queue<IWaferOwnable>();
        #endregion

        #region => Loader Work Thread
        private Thread myRunningThread;

        private bool isStopThreadReq = false;

        private void StartWorkerThread()
        {
            try
            {
                StopThread();

                myRunningThread = new Thread(new ThreadStart(DoWork));
                myRunningThread.Name = this.GetType().Name;
                myRunningThread.Start();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void StopThread()
        {
            try
            {
                isStopThreadReq = true;

                if (myRunningThread != null && myRunningThread.IsAlive)
                {
                    myRunningThread.Join();
                }

                isStopThreadReq = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void DoWork()
        {
            try
            {
                while (isStopThreadReq == false)
                {
                    var hash = this.GetHashCode();
                    Execute();
                    //if (PauseFlag)
                    //{
                    //    StateObj.Pause();
                    //}
                    //if (ResumeFlag)
                    //{
                    //    StateObj.Resume();
                    //}
                    //_Delays.DelayFor(2);
                    Thread.Sleep(2);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region => Init Methods
        public void SetLoaderContainer(Autofac.IContainer loaderContainer)
        {
            try
            {
                //this.Container = loaderContainer;
                this.AssignLoaderContainer(loaderContainer);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetStageContainer(Autofac.IContainer stageContainer)
        {
            try
            {
                this.StageContainer = stageContainer;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        private List<ILoaderFactoryModule> GetFactoryModules()
        {
            List<ILoaderFactoryModule> factoryModules = null;

            try
            {
                var type = this.GetType();
                var propInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                factoryModules = new List<ILoaderFactoryModule>();

                var interfaceType = typeof(ILoaderFactoryModule);

                foreach (var pi in propInfos)
                {
                    if (interfaceType.IsAssignableFrom(pi.PropertyType))
                    {
                        var value = pi.GetValue(this, null);

                        factoryModules.Add(value as ILoaderFactoryModule);
                    }
                }

                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    EnvControlManager = Container.Resolve<IEnvControlManager>();
                    DeviceManager = Container.Resolve<IDeviceManager>();
                    NotifyManager = Container.Resolve<INotifyManager>();
                    CardChangeSupervisor = Container.Resolve<ICardChangeSupervisor>();
                }

                factoryModules = factoryModules.OrderByDescending(x => x?.InitPriority).ToList();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return factoryModules;
        }
        public EventCodeEnum RecoveryBackupData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (File.Exists(LOADER_BACKUP_FILE))
                {

                    IParam param = null;
                    var loadResult = this.LoadParameter(ref param, typeof(LoaderInfo), null, LOADER_BACKUP_FILE);
                    var backupInfo = param as LoaderInfo;

                    if (backupInfo != null)
                    {
                        if (backupInfo.StateMap != null)
                        {
                            foreach (var holderInfo in backupInfo.StateMap.GetHolderModuleAll())
                            {
                                var holderModule = this.ModuleManager.FindModule<IWaferOwnable>(holderInfo.ID);

                                if (holderModule != null)
                                {
                                    holderModule.Holder.RecoveryBackupData(holderInfo);
                                }

                            }

                            //for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                            //{
                            //    var cstModule = this.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, i + 1);
                            //    cstModule.SetCarrierId(backupInfo.StateMap.CassetteModules[i].FoupID);
                            //}

                            retVal = EventCodeEnum.NONE;
                        }
                        else
                        {
                            retVal = EventCodeEnum.UNDEFINED;
                        }
                    }
                    else
                    {
                        retVal = EventCodeEnum.UNDEFINED;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public async Task<EventCodeEnum> CheckCanUseBufferExistPolish(bool useMsg)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var loaderInfo = GetLoaderInfo();
                string fiexdTrayIndexString = "";
                DeviceManagerParameter DMParam = this.DeviceManager()?.DeviceManagerParamerer_IParam as DeviceManagerParameter;
                List<string> PolishInfosInFixedTray = new List<string>();

                foreach (var item in DMParam.DeviceMappingInfos)
                {
                    IAttachedModule module = ModuleManager.FindModule(item.WaferSupplyInfo.ID);

                    if (module != null)
                    {
                        if ((module.ModuleType == ModuleTypeEnum.FIXEDTRAY))
                        {
                            if (item.DeviceInfo.PolishWaferInfo != null)
                            {
                                PolishInfosInFixedTray.Add(item.DeviceInfo.PolishWaferInfo.DefineName?.Value);
                            }
                            else
                            {
                                PolishInfosInFixedTray.Add("");
                            }
                        }
                    }
                }

                var fixedTray = ModuleManager.FindModules<IFixedTrayModule>();
                for (int i = 0; i < this.SystemParameter.FixedTrayModules.Count; i++)
                {
                    if (this.SystemParameter.FixedTrayModules[i].CanUseBuffer.Value == true
                        && PolishInfosInFixedTray[i] != "")
                    {
                        this.SystemParameter.FixedTrayModules[i].CanUseBuffer.Value = false;
                        fiexdTrayIndexString += i + 1 + ", ";
                    }
                }

                if (useMsg == true)
                {
                    if (fiexdTrayIndexString != "")
                    {
                        int index = fiexdTrayIndexString.LastIndexOf(',');
                        string msg = fiexdTrayIndexString.Remove(index, 1);
                        await this.MetroDialogManager().ShowMessageDialog("Warning", $"PolishWafer are already assigned to this tray {msg} \nCannot set 'can use buffer'.", EnumMessageStyle.Affirmative);
                    }
                }

                retVal = EventCodeEnum.NONE;
                SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum GetModulesSupportingCassetteType(CassetteTypeEnum cassetteType)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var size = GetDefaultWaferSize();
                foreach (var Module in SystemParameter.ChuckModules)
                {
                    var exist = Module.AccessParams.Any(dev => dev.SubstrateSize.Value == size && dev.CassetteType.Value == cassetteType);
                    if (exist)
                    {
                        retVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        return retVal = EventCodeEnum.PARAM_ERROR;
                    }
                }
                foreach (var Module in SystemParameter.PreAlignModules)
                {
                    var exist = Module.AccessParams.Any(dev => dev.SubstrateSize.Value == size && dev.CassetteType.Value == cassetteType);
                    if (exist)
                    {
                        retVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        return retVal = EventCodeEnum.PARAM_ERROR;
                    }
                }
                foreach (var Module in SystemParameter.CassetteModules)
                {
                    var exist = Module.Slot1AccessParams.Any(dev => dev.SubstrateSize.Value == size && dev.CassetteType.Value == cassetteType);
                    if (exist)
                    {
                        retVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        return retVal = EventCodeEnum.PARAM_ERROR;
                    }
                }
                foreach (var Module in SystemParameter.BufferModules)
                {
                    var exist = Module.AccessParams.Any(dev => dev.SubstrateSize.Value == size && dev.CassetteType.Value == cassetteType);
                    if (exist)
                    {
                        retVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        return retVal = EventCodeEnum.PARAM_ERROR;
                    }
                }
                foreach (var Module in SystemParameter.InspectionTrayModules)
                {
                    var exist = Module.AccessParams.Any(dev => dev.SubstrateSize.Value == size && dev.CassetteType.Value == cassetteType);
                    if (exist)
                    {
                        retVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        return retVal = EventCodeEnum.PARAM_ERROR;
                    }
                }
                foreach (var Module in SystemParameter.ScanSensorModules)
                {
                    var exist = Module.ScanParams.Any(dev => dev.SubstrateSize.Value == size && dev.CassetteType.Value == cassetteType);
                    if (exist)
                    {
                        retVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        return retVal = EventCodeEnum.PARAM_ERROR;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally 
            {
                if (retVal == EventCodeEnum.PARAM_ERROR)
                {
                    LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(). The specified Cassette type{cassetteType} is not supported.");
                }
            }
            return retVal;
        }

        public EventCodeEnum SetModuleEnable()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                var buffers = ModuleManager.FindModules<IBufferModule>();

                for (int i = 0; i < this.SystemParameter.BufferModules.Count; i++)
                {
                    buffers[i].Enable = this.SystemParameter.BufferModules[i].Enable.Value;
                }
                var PAs = ModuleManager.FindModules<IPreAlignModule>();

                for (int i = 0; i < this.SystemParameter.PreAlignModules.Count; i++)
                {
                    PAs[i].Enable = this.SystemParameter.PreAlignModules[i].Enable.Value;
                }
                var cardbuffer = ModuleManager.FindModules<ICardBufferModule>();
                for (int i = 0; i < this.SystemParameter.CardBufferModules.Count; i++)
                {
                    cardbuffer[i].Enable = this.SystemParameter.CardBufferModules[i].Enable.Value;
                }
                var fixedTray = ModuleManager.FindModules<IFixedTrayModule>();
                for (int i = 0; i < this.SystemParameter.FixedTrayModules.Count; i++)
                {
                    fixedTray[i].CanUseBuffer = this.SystemParameter.FixedTrayModules[i].CanUseBuffer.Value;
                }


                var foups = ModuleManager.FindModules<ICassetteModule>();
                for (int i = 0; i < this.SystemParameter.CassetteModules.Count; i++)
                {
                    foups[i].Enable = this.SystemParameter.CassetteModules[i].Enable.Value;
                    if (this.Foup != null && Foup.FoupControllers != null)
                    {
                        if (Foup.FoupControllers.Count > i)
                        {
                            Foup.FoupControllers[i].FoupModuleInfo.Enable = this.SystemParameter.CassetteModules[i].Enable.Value;
                        }
                    }
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public EventCodeEnum Initialize(LoaderServiceTypeEnum serviceType, string rootParamPath)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            //using (Locker locker = new Locker(syncObj))
            //{
            lock (syncObj)
            {
                //if (locker.AcquiredLock == false)
                //{
                //    System.Diagnostics.Debugger.Break();
                //    return retVal;
                //}

                try
                {
                    if (IsInitialized == false)
                    {
                        //Deinitialize();
                        if (SystemManager.SysteMode == SystemModeEnum.Multiple && serviceType != LoaderServiceTypeEnum.DynamicLinking)
                        {
                            LoaderMaster = Container.Resolve<ILoaderSupervisor>();
                        }

                        StateObj = new INIT(this);
                        ProcModuleInfo = new LoaderProcModuleInfo();
                        this.ServiceType = serviceType;
                        this.RootParamPath = rootParamPath;

                        retVal = LoadSysParameter();
                        //retVal = LoadDevParameter();
                        LoaderOption = new LoaderTestOption();


                        //init factory modules
                        foreach (var module in GetFactoryModules())
                        {
                            if (module != null)
                            {
                                retVal = module.InitModule(Container);

                                if (retVal != EventCodeEnum.NONE)
                                    break;
                            }
                        }

                        //retVal = Foup.InitModule();
                        if (retVal != EventCodeEnum.NONE)
                        {
                            LoggerManager.Error($"Foup Controller error. Return = {retVal}");
                        }

                        //Recovery Backup data
                        RecoveryBackupData();
                        BroadcastLoaderInfo();

                        StartWorkerThread();
                        if (SystemManager.SysteMode == SystemModeEnum.Multiple && serviceType != LoaderServiceTypeEnum.DynamicLinking)
                        {
                            for (int i = 0; i < SystemModuleCount.ModuleCnt.PACount; i++)
                            {
                                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                                {
                                    PAManager.PAModules[i].SetPAStatus(EnumPAStatus.Idle);
                                    continue;
                                }
                                retVal = PAManager.PAModules[i].ModuleReset();
                                if (retVal == EventCodeEnum.NONE)
                                {
                                    bool IsExist = false;
                                    retVal = PAManager.PAModules[i].IsSubstrateExist(out IsExist);
                                    if (retVal == EventCodeEnum.NONE)
                                    {
                                        //IsSubstrateExist 함수 안에서 HoldSubstrate / ReleaseSubstrate 호출 함
                                        LoggerManager.Debug($"[LOADER] Initialize() : Is Sub strate Wafer Exist {IsExist}");
                                    }
                                    else
                                    {
                                        // 명령어 수행 실패 했을 경우
                                        PAManager.PAModules[i].ReleaseSubstrate();
                                    }
                                }
                                else
                                {
                                    LoggerManager.Error($"[LOADER] Initialize() : Pa Module Reset Error");
                                }
                                PAManager.PAModules[i].UpdateState();
                            }
                        }

                        _DeviceManager = Container.Resolve<IDeviceManager>();

                        //_DeviceManager.InitModule();
                        if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                        {
                            for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                            {
                                Foups.Add(new FoupObject(i));
                                Foups[i].AllocatedCellInfo = "";
                                Foups[i].Index = i + 1;

                                var module = ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, i + 1);

                                //TODO: V19 왜 null이었는지는 한번 더 점검할것.
                                if (module?.Device?.AllocateDeviceInfo != null && module?.Device.AllocateDeviceInfo.NotchAngle != null)
                                {
                                    Foups[i].NotchAngle = module.Device.AllocateDeviceInfo.NotchAngle.Value;
                                }

                                Foups[i].Slots = new ObservableCollection<SlotObject>();

                                for (int j = 0; j < 25; j++)
                                {
                                    Foups[i].Slots.Add(new SlotObject(25 - j));
                                    Foups[i].Slots[j].FoupNumber = Foups[i].Index;
                                }

                                Foups[i].LotSettings = new ObservableCollection<LoaderBase.Communication.IStagelotSetting>();

                                for (int j = 0; j < SystemModuleCount.ModuleCnt.StageCount; j++)
                                {
                                    StageObject_forLot setting = new StageObject_forLot(GetUseLotProcessingVerify());
                                    setting.Index = j + 1;
                                    setting.FoupNumber = Foups[i].Index;

                                    Foups[i].LotSettings.Add(setting);

                                }

                                FoupScanFlag.Add(new FoupFlag());
                                FoupScanFlag[i].ScanFlag = false;
                                FoupScanFlag[i].FoupName = $"Foup{i + 1}";

                                this.DeviceSize = this.GetLoaderCommands().GetDeviceSize(i);

                            }
                        }

                        IsInitialized = true;

                        LoadLoaderOCRConfig();
#pragma warning disable 4014
                        //brett// loader init 중 호출되고 있으므로 await를 하지 않는다.(해당 함수 내부에서 에러시 message box를 await하는 구문이 있음, init시점에서는 해당 messagebox를 닫을때까지 대기하지 않기 위함 
                        CheckCanUseBufferExistPolish(false);
#pragma warning restore 4014
                        //SetModuleEnable();
                        GetLoaderInfo();

                        var attachedModules = ModuleManager.FindModules<IWaferOwnable>();
                        foreach (var module in attachedModules)
                        {
                            SetAttachedModuleVID(module.ID, module.Holder);
                            module.HolderStatusChanged += SetAttachedModuleVID;
                        }
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }

            return retVal;
        }
        public EventCodeEnum SetAttachedModuleVID(ModuleID moduleID, IWaferHolder holder)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (moduleID.ModuleType == ModuleTypeEnum.FIXEDTRAY)
                {
                    ret = this.LoaderMaster.GEMModule().SetFixedTrayVID(moduleID, holder);

                    if (ret != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"SetAttachedModuleVID Return = {ret}");
                    }
                }
                else
                {
                    // TODO : 다른 타입을 고려 해야 하는 경우

                    ret = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public EventCodeEnum Deinitialize()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            //using (Locker locker = new Locker(syncObj))
            //{
            lock (syncObj)
            {
                //if (locker.AcquiredLock == false)
                //{
                //    System.Diagnostics.Debugger.Break();
                //    return retVal;
                //}

                try
                {
                    StopThread();

                    var modules = GetFactoryModules();
                    modules.Reverse();

                    foreach (var module in modules)
                    {
                        try
                        {
                            if (module != null)
                            {
                                module.DeInitModule();
                            }
                        }
                        catch (Exception moderr)
                        {
                            LoggerManager.Debug($"Module Deinit. Error occurred. Err = {moderr.Message}");
                        }

                    }

                    retVal = EventCodeEnum.NONE;
                }
                catch (Exception err)
                {
                    retVal = EventCodeEnum.UNDEFINED;
                    //LoggerManager.Error($ex.Message);
                    LoggerManager.Exception(err);

                }
            }
            return retVal;
        }

        public EventCodeEnum Connect(ILoaderServiceCallback callback)
        {
            //using (Locker locker = new Locker(syncObj))
            //{
            lock (syncObj)
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                //if (locker.AcquiredLock == false)
                //{
                //    System.Diagnostics.Debugger.Break();
                //    return retVal;
                //}
                try
                {
                    // ToDo: Surpport nullable on ServiceCallBack for off-line loader.
                    this.ServiceCallback = callback;

                    retVal = EventCodeEnum.NONE;
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                return retVal;
            }
        }

        public EventCodeEnum Disconnect()
        {
            //using (Locker locker = new Locker(syncObj))
            //{
            lock (syncObj)
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                //if (locker.AcquiredLock == false)
                //{
                //    System.Diagnostics.Debugger.Break();
                //    return retVal;
                //}

                try
                {
                    this.ServiceCallback = null;

                    retVal = EventCodeEnum.NONE;
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                return retVal;
            }
        }

        #endregion

        #region => Parameters

        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                tmpParam = new LoaderDeviceParameter();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(LoaderDeviceParameter));

                if (RetVal == EventCodeEnum.NONE)
                {
                    DeviceParameter = tmpParam as LoaderDeviceParameter;
                    //if (LoaderService != null)
                    //{
                    //    LoaderService.UpdateLoaderSystem();
                    //}
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public EventCodeEnum SaveDevParameter() // Don`t Touch
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = this.SaveParameter(DeviceParameter);
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
                tmpParam = new LoaderSystemParameter();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(LoaderSystemParameter));

                if (RetVal == EventCodeEnum.NONE)
                {
                    SystemParameter = tmpParam as LoaderSystemParameter;
                    if (SystemParameter.Card_ID_Position == null)
                    {
                        SystemParameter.Card_ID_Position = new LoaderCoordinate();
                    }
                    if (SystemParameter.CardTrayIndexOffset == null)
                    {
                        SystemParameter.CardTrayIndexOffset = new Element<int>();
                    }
                    if (SystemParameter.CardTrayIndexOffset.Value == 0)
                    {
                        if (SystemManager.SystemType == SystemTypeEnum.Opera)
                        {
                            SystemParameter.CardTrayIndexOffset.Value = 4;
                        }
                        else
                        {
                            SystemParameter.CardTrayIndexOffset.Value = SystemParameter.CardBufferModules.Count;
                        }
                    }
                    if (SystemParameter.CardBufferModules.Where(i => i.Enable.Value == true).Count() == 0)
                    {
                        foreach (var cardBuffer in SystemParameter.CardBufferModules)
                        {
                            cardBuffer.Enable.Value = true;
                        }
                        SaveSysParameter();
                    }
                    SystemParameter.CardTrayIndexOffset.SetOriginValue();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public EventCodeEnum LoadLoaderOCRConfig()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                tmpParam = new LoaderOCRConfigParam();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(LoaderOCRConfigParam));

                if (RetVal == EventCodeEnum.NONE)
                {
                    OCRConfig = tmpParam as LoaderOCRConfigParam;
                }


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }
        public EventCodeEnum SaveLoaderOCRConfig()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = this.SaveParameter(OCRConfig);
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
                retVal = this.SaveParameter(SystemParameter);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        #endregion


        public void AddUnknownModule(IWaferOwnable module)
        {
            UnknownModule.Enqueue(module);
        }
        public void ResetUnknownModule()
        {
            try
            {
                IWaferOwnable Module = null;
                while (true)
                {
                    if (UnknownModule.Count >= 1)
                    {
                        Module = UnknownModule.Dequeue();
                        Module.Holder.SetUnload();
                    }
                    else
                    {
                        break;
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public void SetPause()
        {
            try
            {
                PauseFlag = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetResume()
        {
            try
            {
                ResumeFlag = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public LoaderInfo GetLoaderInfo()
        {
            LoaderInfo loaderInfo = new LoaderInfo();

            try
            {
                loaderInfo.ModuleInfo = GetLoaderModuleInfo();
                loaderInfo.StateMap = GetCurrStateMap();
                loaderInfo.DynamicMode = 0;
                loaderInfo.FoupShiftMode = 0;

                if (SystemManager.SysteMode != SystemModeEnum.Single)
                {
                    loaderInfo.StateMap.ActiveFoupList = LoaderMaster.ActiveLotInfos.FindAll(item => item.State == LotStateEnum.Running ||
                                                                                                 item.State == LotStateEnum.End ||
                                                                                                 item.State == LotStateEnum.Cancel)
                                                                                .Select(item => item.FoupNumber)
                                                                                .ToList();
                    loaderInfo.StateMap.ActiveFoupList.AddRange(LoaderMaster.Prev_ActiveLotInfos.Select(item => item.FoupNumber));
                    if (LoaderMaster != null)
                    {
                        loaderInfo.DynamicMode = (int)LoaderMaster.DynamicMode;
                        loaderInfo.FoupShiftMode = (int)LoaderMaster.GetFoupShiftMode();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return loaderInfo;

            LoaderModuleInfo GetLoaderModuleInfo()
            {
                LoaderModuleInfo retVal = null;

                try
                {
                    retVal = new LoaderModuleInfo();

                    retVal.ModuleState = this.ModuleState;
                    retVal.ReasonOfSuspended = Sequencer.GetSuspendedInfo();
                    retVal.ChuckNumber = -1;
                    if (retVal.ReasonOfSuspended == ReasonOfSuspendedEnum.LOAD || retVal.ReasonOfSuspended == ReasonOfSuspendedEnum.UNLOAD
                        || retVal.ReasonOfSuspended == ReasonOfSuspendedEnum.CARD_LOAD || retVal.ReasonOfSuspended == ReasonOfSuspendedEnum.CARD_UNLOAD)
                    {
                        retVal.ChuckNumber = this.ChuckNumber;
                    }
                    //retVal = new LoaderModuleInfo()
                    //{
                    //    ModuleState = this.ModuleState,
                    //    ReasonOfSuspended = Sequencer.GetSuspendedInfo(),
                    //};
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }

                return retVal;
            }

            LoaderMap GetCurrStateMap()
            {
                LoaderMap map = new LoaderMap();

                try
                {
                    map.CassetteModules = GetCassetteInfos();
                    map.ARMModules = GetHolderInfo<IARMModule>();
                    map.PreAlignModules = GetHolderInfo<IPreAlignModule>();
                    map.FixedTrayModules = GetFixedTrayInfo();
                    map.BufferModules = GetHolderInfo<IBufferModule>();
                    map.InspectionTrayModules = GetHolderInfo<IInspectionTrayModule>();
                    map.ChuckModules = GetHolderInfo<IChuckModule>();
                    map.SemicsOCRModules = GetOCRModules<ISemicsOCRModule>();
                    map.CognexOCRModules = GetOCRModules<ICognexOCRModule>();
                    map.CardBufferModules = GetBufferModuleInfo<ICardBufferModule>();
                    map.CardTrayModules = GetBufferModuleInfo<ICardBufferTrayModule>();
                    map.CCModules = GetBufferModuleInfo<ICCModule>();
                    map.CardArmModule = GetBufferModuleInfo<ICardARMModule>();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                return map;

                CassetteModuleInfo[] GetCassetteInfos()
                {
                    var cassettes = ModuleManager.FindModules<ICassetteModule>();
                    List<CassetteModuleInfo> cstInfoList = new List<CassetteModuleInfo>();

                    try
                    {
                        foreach (var cassette in cassettes)
                        {
                            try
                            {
                                CassetteModuleInfo info = new CassetteModuleInfo();
                                info.ID = cassette.ID;
                                info.ScanState = cassette.ScanState;
                                info.FoupState = cassette.FoupState;
                                info.FoupCoverState = cassette.FoupCoverState;
                                info.CST_HashCode = cassette.HashCode;
                                info.FoupID = cassette.FoupID;
                                info.SlotModules = GetSlotModules(cassette);
                                info.LotMode = cassette.LotMode;
                                info.Enable = cassette.Enable;

                                cstInfoList.Add(info);
                            }
                            catch (Exception err)
                            {
                                throw new Exception(err.Message + $"// cassette : {cassette}");
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                    return cstInfoList.ToArray();
                }

                FixedTrayModuleInfo[] GetFixedTrayInfo()
                {
                    var fixedTrays = ModuleManager.FindModules<IFixedTrayModule>();
                    List<FixedTrayModuleInfo> fixedInfoList = new List<FixedTrayModuleInfo>();
                    foreach (var module in fixedTrays)
                    {
                        try
                        {
                            FixedTrayModuleInfo info = new FixedTrayModuleInfo();
                            info.ID = module.ID;
                            info.WaferStatus = module.Holder.Status;
                            info.Substrate = module.Holder.GetTransferObjectClone();
                            info.ReservationInfo = module.ReservationInfo;
                            info.Enable = module.Enable;
                            info.CanUseBuffer = module.CanUseBuffer;
                            fixedInfoList.Add(info);
                        }
                        catch (Exception err)
                        {

                            throw new Exception(err.Message + $"// module : {module}");
                        }

                    }
                    return fixedInfoList.ToArray();

                }

                HolderModuleInfo[] GetSlotModules(ICassetteModule cassette)
                {
                    List<HolderModuleInfo> list = new List<HolderModuleInfo>();
                    try
                    {
                        foreach (var module in ModuleManager.FindSlots(cassette))
                        {
                            try
                            {
                                HolderModuleInfo info = new HolderModuleInfo();
                                info.ID = module.ID;
                                info.WaferStatus = module.Holder.Status;
                                info.Substrate = module.Holder.GetTransferObjectClone();
                                if (module.Holder.CurrentWaferInfo != null && info.ID == module.Holder.CurrentWaferInfo.OriginHolder)
                                {
                                    if (ServiceCallback != null)
                                    {
                                        ServiceCallback.WaferHolderChanged(module.LocalSlotNumber, module.Holder.CurrentWaferInfo.CurrHolder.ModuleType.ToString());
                                    }
                                    //    info.ModuleType = module.Holder.CurrentWaferInfo.CurrHolder.ModuleType;
                                }
                                else
                                {
                                    if (ServiceCallback != null)
                                    {
                                        ServiceCallback.WaferHolderChanged(module.LocalSlotNumber, ModuleTypeEnum.UNDEFINED.ToString());
                                    }
                                    //    info.ModuleType = ModuleTypeEnum.UNDEFINED;
                                }
                                list.Add(info);
                            }
                            catch (Exception err)
                            {
                                throw new Exception(err.Message + $"// module : {module}");
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                    return list.ToArray();
                }
                CardModuleInfo[] GetBufferModuleInfo<T>() where T : class, ICardOwnable
                {
                    List<CardModuleInfo> list = new List<CardModuleInfo>();
                    try
                    {
                        foreach (var module in ModuleManager.FindModules<T>())
                        {
                            try
                            {
                                CardModuleInfo info = new CardModuleInfo();
                                info.ID = module.ID;
                                info.WaferStatus = module.Holder.Status;
                                info.Substrate = module.Holder.GetTransferObjectClone();
                                info.WaferType = EnumWaferType.CARD;
                                info.ModuleType = module.ModuleType;
                                info.Enable = module.Enable;
                                //if (module.Holder.CurrentWaferInfo != null)
                                //{
                                //    info.ModuleType = module.Holder.CurrentWaferInfo.CurrHolder.ModuleType;
                                //}
                                list.Add(info);
                            }
                            catch (Exception err)
                            {
                                throw new Exception(err.Message + $"// module : {module}");
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                    return list.ToArray();
                }

                HolderModuleInfo[] GetHolderInfo<T>() where T : class, IWaferOwnable
                {
                    List<HolderModuleInfo> list = new List<HolderModuleInfo>();
                    try
                    {
                        foreach (var module in ModuleManager.FindModules<T>())
                        {
                            try
                            {
                                HolderModuleInfo info = new HolderModuleInfo();
                                info.ID = module.ID;
                                info.WaferStatus = module.Holder.Status;
                                info.Substrate = module.Holder.GetTransferObjectClone();
                                info.ReservationInfo = module.ReservationInfo;
                                info.Enable = module.Enable;
                                //if (module.Holder.CurrentWaferInfo != null)
                                //{
                                //    info.ModuleType = module.Holder.CurrentWaferInfo.CurrHolder.ModuleType;
                                //}
                                list.Add(info);
                            }
                            catch (Exception err)
                            {
                                throw new Exception(err.Message + $"// module : {module}");
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                    return list.ToArray();
                }

                OCRModuleInfo[] GetOCRModules<T>() where T : class, IOCRReadable
                {
                    List<OCRModuleInfo> list = new List<OCRModuleInfo>();
                    try
                    {
                        foreach (var module in ModuleManager.FindModules<T>())
                        {
                            try
                            {
                                OCRModuleInfo info = new OCRModuleInfo();
                                info.ID = module.ID;
                                info.ReservationInfo = module.ReservationInfo;

                                list.Add(info);
                            }
                            catch (Exception err)
                            {
                                throw new Exception(err.Message + $"// module : {module}");
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                    return list.ToArray();
                }
            }
        }


        #region => Work Methods
        [MethodImpl(MethodImplOptions.Synchronized)]
        public ResponseResult SetRequest(LoaderMap dstMap)
        {
            //using (Locker locker = new Locker(syncObj))
            //{
            lock (syncObj)
            {
                ResponseResult retVal = null;
                //if (locker.AcquiredLock == false)
                //{
                //    System.Diagnostics.Debugger.Break();
                //    return retVal;
                //}

                try
                {
                    retVal = StateObj.SetRequest(dstMap);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                return retVal;
            }
        }

        public void Execute()
        {
            //using (Locker locker = new Locker(syncObj))
            //{
            lock (syncObj)
            {
                //if (locker.AcquiredLock == false)
                //{
                //    System.Diagnostics.Debugger.Break();
                //    return;
                //}
                try
                {
                    StateObj.Execute();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
        }

        public EventCodeEnum AwakeProcessModule()
        {
            //using (Locker locker = new Locker(syncObj))
            //{
            lock (syncObj)
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                //if (locker.AcquiredLock == false)
                //{
                //    System.Diagnostics.Debugger.Break();
                //    return retVal;
                //}

                try
                {
                    retVal = StateObj.AwakeProcessModule();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                return retVal;
            }
        }

        public EventCodeEnum AbortProcessModule()
        {
            //using (Locker locker = new Locker(syncObj))
            //{
            lock (syncObj)
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                //if (locker.AcquiredLock == false)
                //{
                //    System.Diagnostics.Debugger.Break();
                //    return retVal;
                //}

                try
                {
                    retVal = StateObj.AbortProcessModule();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                return retVal;
            }
        }

        public EventCodeEnum AbortRequest()
        {
            //using (Locker locker = new Locker(syncObj))
            //{
            lock (syncObj)
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                //if (locker.AcquiredLock == false)
                //{
                //    System.Diagnostics.Debugger.Break();
                //    return retVal;
                //}

                try
                {
                    retVal = StateObj.AbortRequest();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                return retVal;
            }
        }

        public EventCodeEnum ClearRequestData()
        {
            //using (Locker locker = new Locker(syncObj))
            //{
            lock (syncObj)
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                //if (locker.AcquiredLock == false)
                //{
                //    System.Diagnostics.Debugger.Break();
                //    return retVal;
                //}

                try
                {
                    retVal = StateObj.ClearRequestData();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                return retVal;
            }
        }
        public EventCodeEnum Pause()
        {
            //using (Locker locker = new Locker(syncObj))
            //{
            lock (syncObj)
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                //if (locker.AcquiredLock == false)
                //{
                //    System.Diagnostics.Debugger.Break();
                //    return retVal;
                //}
                try
                {
                    retVal = StateObj.Pause();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                return retVal;
            }
        }
        public EventCodeEnum Resume()
        {
            //using (Locker locker = new Locker(syncObj))
            //{
            lock (syncObj)
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                //if (locker.AcquiredLock == false)
                //{
                //    System.Diagnostics.Debugger.Break();
                //    return retVal;
                //}
                try
                {
                    retVal = StateObj.Resume();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                return retVal;
            }
        }

        public EventCodeEnum SetEMGSTOP()
        {
            //using (Locker locker = new Locker(syncObj))
            //{
            lock (syncObj)
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                try
                {
                    retVal = StateObj.SetEMGSTOP();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                return retVal;
            }
        }
        public void SelfRecovery()
        {
            //using (Locker locker = new Locker(syncObj))
            //{
            //    if (locker.AcquiredLock == false)
            //    {
            //        System.Diagnostics.Debugger.Break();
            //        return;
            //    }
            lock (syncObj)
            {
                try
                {
                    StateObj.SelfRecovery();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
        }

        #endregion

        #region Recovery Methods

        public EventCodeEnum RECOVERY_MotionInit()
        {
            //using (Locker locker = new Locker(syncObj))
            //{
            lock (syncObj)
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                //if (locker.AcquiredLock == false)
                //{
                //    System.Diagnostics.Debugger.Break();
                //    return retVal;
                //}

                try
                {
                    retVal = StateObj.MotionInit();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                return retVal;
            }
        }
        public EventCodeEnum RECOVERY_ResetWaferLocation()
        {
            //using (Locker locker = new Locker(syncObj))
            //{
            lock (syncObj)
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                //if (locker.AcquiredLock == false)
                //{
                //    System.Diagnostics.Debugger.Break();
                //    return retVal;
                //}

                try
                {
                    retVal = StateObj.ResetWaferLocation();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                return retVal;
            }
        }
        #endregion

        #region => Motion Methods
        public EventCodeEnum SystemInit()
        {
            //using (Locker locker = new Locker(syncObj))
            //{
            lock (syncObj)
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                //if (locker.AcquiredLock == false)
                //{
                //    System.Diagnostics.Debugger.Break();
                //    return retVal;
                //}

                try
                {
                    retVal = StateObj.SystemInit();

                    // 250911 LJH LoaderMaster 싱글일 경우 미사용? 예외처리 추가
                    if(null == LoaderMaster)
                    {
                        return retVal;
                    }

                    if (retVal != EventCodeEnum.NONE)
                    {
                        LoaderMaster.LoaderSystemInitFailure = true;
                    }
                    else
                    {
                        LoaderMaster.LoaderSystemInitFailure = false;
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                return retVal;
            }
        }

        public EventCodeEnum MOTION_JogRelMove(EnumAxisConstants axis, double value)
        {
            //using (Locker locker = new Locker(syncObj))
            //{
            lock (syncObj)
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                //if (locker.AcquiredLock == false)
                //{
                //    System.Diagnostics.Debugger.Break();
                //    return retVal;
                //}

                try
                {
                    retVal = StateObj.JogRelMove(axis, value);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                return retVal;
            }
        }

        public EventCodeEnum MOTION_JogAbsMove(EnumAxisConstants axis, double value)
        {
            //using (Locker locker = new Locker(syncObj))
            //{
            lock (syncObj)
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                //if (locker.AcquiredLock == false)
                //{
                //    System.Diagnostics.Debugger.Break();
                //    return retVal;
                //}

                try
                {
                    retVal = StateObj.JogAbsMove(axis, value);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                return retVal;
            }
        }
        #endregion

        #region => Setting Param Methods
        public EventCodeEnum UpdateSystemParam(LoaderSystemParameter systemParam)
        {
            //using (Locker locker = new Locker(syncObj))
            //{
            lock (syncObj)
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                //if (locker.AcquiredLock == false)
                //{
                //    System.Diagnostics.Debugger.Break();
                //    return retVal;
                //}

                try
                {
                    retVal = StateObj.UpdateSystem(systemParam);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                return retVal;
            }
        }

        public EventCodeEnum SaveSystemParam(LoaderSystemParameter systemParam = null)
        {
            //using (Locker locker = new Locker(syncObj))
            //{
            lock (syncObj)
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                //if (locker.AcquiredLock == false)
                //{
                //    System.Diagnostics.Debugger.Break();
                //    return retVal;
                //}

                try
                {
                    retVal = StateObj.SaveSystem(systemParam);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                return retVal;
            }
        }

        public EventCodeEnum UpdateDeviceParam(LoaderDeviceParameter deviceParam)
        {
            //using (Locker locker = new Locker(syncObj))
            //{
            lock (syncObj)
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                //if (locker.AcquiredLock == false)
                //{
                //    System.Diagnostics.Debugger.Break();
                //    return retVal;
                //}

                try
                {
                    retVal = StateObj.UpdateDevice(deviceParam);
                    if (retVal == EventCodeEnum.NONE)
                    {
                        if (deviceParam.CassetteModules[0].AllocateDeviceInfo.OCRType.Value == ProberInterfaces.Enum.OCRTypeEnum.COGNEX)
                        {
                            Container.Resolve<ICognexProcessManager>().LoadConfig();
                        }
                    }

                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                return retVal;
            }
        }

        //public EventCodeEnum SaveDeviceParam(LoaderDeviceParameter deviceParam = null)
        //{
        //    using (Locker locker = new Locker(syncObj))
        //    {
        //        EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //        if (locker.AcquiredLock == false)
        //        {
        //            System.Diagnostics.Debugger.Break();
        //            return retVal;
        //        }

        //        try
        //        {
        //            retVal = StateObj.SaveDevice(deviceParam);
        //        }
        //        catch (Exception err)
        //        {
        //            LoggerManager.Exception(err);
        //        }
        //        return retVal;
        //    }
        //}
        public EventCodeEnum RetractAll()
        {
            //using (Locker locker = new Locker(syncObj))
            //{
            lock (syncObj)
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                //if (locker.AcquiredLock == false)
                //{
                //    System.Diagnostics.Debugger.Break();
                //    return retVal;
                //}

                try
                {
                    retVal = StateObj.RetractAll();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                return retVal;
            }
        }
        public EventCodeEnum MoveToModuleForSetup(ModuleTypeEnum module, bool skipuaxis, int slot, int index)
        {
            //using (Locker locker = new Locker(syncObj))
            //{
            lock (syncObj)
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                //if (locker.AcquiredLock == false)
                //{
                //    System.Diagnostics.Debugger.Break();
                //    return retVal;
                //}

                try
                {
                    retVal = StateObj.MoveToModuleForSetup(module, skipuaxis, slot, index);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                return retVal;
            }
        }
        #endregion

        public void FOUP_RaiseFoupStateChanged(FoupModuleInfo foupInfo)
        {
            try
            {
                var cassettes = ModuleManager.FindModules<ICassetteModule>();

                int chagedScanStateCount = 0;

                var cassette = cassettes.Where(item => item.ID.Index == foupInfo.FoupNumber).FirstOrDefault();

                if (cassette != null)
                {
                    lock (cassette.GetLockObj())
                    {
                        if (cassette.ScanState == CassetteScanStateEnum.READ || cassette.ScanState == CassetteScanStateEnum.RESERVED)
                        {
                            if (foupInfo.State == ProberInterfaces.Foup.FoupStateEnum.LOAD)
                            {
                                //No State Transition.
                            }
                            else if (foupInfo.State == ProberInterfaces.Foup.FoupStateEnum.UNLOAD)
                            {
                                cassette.SetNoReadScanState();
                                chagedScanStateCount++;
                            }
                            else
                            {
                                cassette.SetIllegalScanState();
                                chagedScanStateCount++;
                            }
                        }
                        else
                        {
                            cassette.SetNoReadScanState();
                        }

                        cassette.SetFoupState(foupInfo.State);
                        cassette.SetFoupCoverState(foupInfo.FoupCoverState);
                        BroadcastLoaderInfo();

                        if (chagedScanStateCount > 0)
                        {
                            BroadcastLoaderInfo();
                        }
                    }

                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($ex);
                LoggerManager.Exception(err);

            }
        }

        public void FOUP_RaiseWaferOutDetected(int cassetteNumber)
        {
            try
            {
                var foupAccesMode = Move.GetFoupAccessMode(cassetteNumber);
                if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    if (foupAccesMode == FoupAccessModeEnum.ACCESSED)
                    {
                        return;
                    }
                }

                if (StateObj.ModuleState == ModuleStateEnum.RUNNING)
                {
                    return;
                }

                //NONE, UNKNOWN 상태일때 스캔상태를 취소한다.
                var CST = ModuleManager
                    .FindModules<ICassetteModule>()
                    .Where(item => item.ID.Index == cassetteNumber)
                    .FirstOrDefault();
                if (CST != null)
                {
                    lock (CST.GetLockObj())
                    {
                        if (CST.ScanState == CassetteScanStateEnum.READ)
                        {
                            CST.SetIllegalScanState();

                            BroadcastLoaderInfo();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool SetNoReadScanState(int cassetteNumber)
        {
            bool retval = false;

            try
            {
                var currCst = ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, cassetteNumber);

                if (currCst != null)
                {
                    lock (currCst.GetLockObj())
                    {
                        currCst.SetNoReadScanState();
                    }
                }
                else
                {
                    LoggerManager.Error($"[LoaderModule], SetNoReadScanState() : currCst is null.");
                }

                retval = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }


        public bool IsFoupAccessed(int cassetteNumber)
        {
            bool isFoupAccessState = false;

            try
            {
                var foupAccesMode = Move.GetFoupAccessMode(cassetteNumber);
                if (foupAccesMode == FoupAccessModeEnum.NO_ACCESSED)
                    isFoupAccessState = false;
                else
                    isFoupAccessState = true; //UNKNWON상태도 ACCESS상태로 인식한다.
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return isFoupAccessState;
        }

        private string LOADER_BACKUP_FILE = @"C:\Logs\Backup\loaderInfo.Json";
        public object lockObj = new object();
        public void LoaderMapUpdate()
        {
            try
            {
                lock (lockObj)
                {
                    var loaderInfo = GetLoaderInfo();
                    if (LoaderMaster != null)
                    {
                        if (LoaderMapConvert != null)
                        {
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                LoaderMapConvert.LoaderMapConvert(loaderInfo.StateMap);
                            }));
                            LoaderMapConvert.UpdateChanged();
                        }
                        if (TopBar != null)
                        {
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                TopBar.LoaderMapConvert(loaderInfo.StateMap);
                            }));
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        [MethodImpl(MethodImplOptions.PreserveSig)]
        public void BroadcastLoaderInfo(bool updatebackupdata = false)
        {

            try
            {
                lock (broadCastObj)
                {
                    DeviceManagerParameter DMParam = this.DeviceManager()?.DeviceManagerParamerer_IParam as DeviceManagerParameter;

                    if (DMParam != null)
                    {
                        var polishWaferCompatibleHolders = DMParam.DeviceMappingInfos.Where(x => x.WaferSupplyInfo.ModuleType == ModuleTypeEnum.FIXEDTRAY || x.WaferSupplyInfo.ModuleType == ModuleTypeEnum.INSPECTIONTRAY);

                        // Polish Wafer로 설정되어 있는 Holder의 경우, Polish Wafer 정보를 업데이트 해줘야 한다.
                        foreach (var item in polishWaferCompatibleHolders)
                        {
                            IAttachedModule module = ModuleManager.FindModule(item.WaferSupplyInfo.ID);

                            if (module is IWaferOwnable ownable)
                            {
                                TransferObject tmp = (module as IWaferOwnable).Holder.TransferObject;

                                // 웨이퍼 존재
                                if (tmp != null)
                                {
                                    // Holder가 PW용
                                    if (item.DeviceInfo.WaferType.Value == EnumWaferType.POLISH)
                                    {
                                        if (item.DeviceInfo.PolishWaferInfo != null)
                                        {
                                            if (tmp.WaferType.Value != EnumWaferType.POLISH)
                                            {
                                                if (tmp.PolishWaferInfo == null)
                                                {
                                                    tmp.PolishWaferInfo = new PolishWaferInformation();
                                                }

                                                tmp.PolishWaferInfo.Copy(item.DeviceInfo.PolishWaferInfo);
                                                tmp.Size.Value = item.DeviceInfo.PolishWaferInfo.Size.Value;
                                                tmp.NotchType = item.DeviceInfo.PolishWaferInfo.NotchType.Value;
                                                tmp.WaferType.Value = EnumWaferType.POLISH;
                                            }
                                            else
                                            {
                                                // 이미 Polish Wafer 값을 갖고 있더라도, 다른 데이터가 존재하면 변경해줘야 한다.
                                                // 이름이 다른 경우, 모든 데이터 초기화 
                                                // 이름이 같지만, 그 외 기억되어야 하는 값이 아닌 데이터가 변경되었을 경우, 그 외의 데이터들 초기화
                                                tmp.Size.Value = item.DeviceInfo.PolishWaferInfo.Size.Value;
                                                tmp.NotchType = item.DeviceInfo.PolishWaferInfo.NotchType.Value;
                                                if (tmp.PolishWaferInfo == null)
                                                {
                                                    tmp.PolishWaferInfo = new PolishWaferInformation();
                                                }

                                                if (tmp.PolishWaferInfo.DefineName.Value != item.DeviceInfo.PolishWaferInfo.DefineName.Value)
                                                {
                                                    tmp.PolishWaferInfo.Copy(item.DeviceInfo.PolishWaferInfo);
                                                }
                                                else
                                                {
                                                    // 이름이 같은 경우, TouchCount와 CurrentAngle는 DeviceManagerParameter에 기록되어 있는 초기값으로 변경하면 안된다.
                                                    if (item.DeviceInfo.PolishWaferInfo.OCRConfigParam != null)
                                                    {
                                                        if (tmp.PolishWaferInfo.OCRConfigParam == null)
                                                        {
                                                            tmp.PolishWaferInfo.OCRConfigParam = new OCRDevParameter();
                                                        }
                                                        tmp.PolishWaferInfo.OCRConfigParam.Copy(item.DeviceInfo.PolishWaferInfo.OCRConfigParam);
                                                    }

                                                    if (tmp.PolishWaferInfo.MaxLimitCount.Value != item.DeviceInfo.PolishWaferInfo.MaxLimitCount.Value)
                                                    {
                                                        tmp.PolishWaferInfo.MaxLimitCount.Value = item.DeviceInfo.PolishWaferInfo.MaxLimitCount.Value;
                                                    }

                                                    if (tmp.PolishWaferInfo.Size.Value != item.DeviceInfo.PolishWaferInfo.Size.Value)
                                                    {
                                                        tmp.PolishWaferInfo.Size.Value = item.DeviceInfo.PolishWaferInfo.Size.Value;
                                                    }

                                                    if (tmp.PolishWaferInfo.NotchType.Value != item.DeviceInfo.PolishWaferInfo.NotchType.Value)
                                                    {
                                                        tmp.NotchType = item.DeviceInfo.PolishWaferInfo.NotchType.Value;
                                                    }

                                                    if (tmp.PolishWaferInfo.Margin.Value != item.DeviceInfo.PolishWaferInfo.Margin.Value)
                                                    {
                                                        tmp.PolishWaferInfo.Margin.Value = item.DeviceInfo.PolishWaferInfo.Margin.Value;
                                                    }

                                                    if (tmp.PolishWaferInfo.Thickness.Value != item.DeviceInfo.PolishWaferInfo.Thickness.Value)
                                                    {
                                                        tmp.PolishWaferInfo.Thickness.Value = item.DeviceInfo.PolishWaferInfo.Thickness.Value;
                                                    }

                                                    if (tmp.PolishWaferInfo.NotchAngle.Value != item.DeviceInfo.PolishWaferInfo.NotchAngle.Value)
                                                    {
                                                        tmp.PolishWaferInfo.NotchAngle.Value = item.DeviceInfo.PolishWaferInfo.NotchAngle.Value;
                                                    }

                                                    if (tmp.PolishWaferInfo.RotateAngle.Value != item.DeviceInfo.PolishWaferInfo.RotateAngle.Value)
                                                    {
                                                        tmp.PolishWaferInfo.RotateAngle.Value = item.DeviceInfo.PolishWaferInfo.RotateAngle.Value;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LoggerManager.Debug("Unknown.");
                                        }
                                    }
                                    else
                                    {
                                        // Holder가 PW용이 아닐 때, 기본 타입으로 초기화.
                                        if (tmp.WaferType.Value == EnumWaferType.POLISH)
                                        {
                                            tmp.WaferType.Value = EnumWaferType.STANDARD;
                                            tmp.OCR.Value = string.Empty;

                                            tmp.PolishWaferInfo = null;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    var loaderInfo = GetLoaderInfo();
                    //=> Save backup file

                    // TODO: TEST 
                    Extensions_IParam.SaveParameter(null, loaderInfo, null, LOADER_BACKUP_FILE, isNotSave_ChangeLog: true);

                    if (updatebackupdata == true)
                    {
                        RecoveryBackupData();
                    }


                    //=> Broadcast service callback
                    if (LoaderMaster != null)
                    {
                        if (LoaderMapConvert != null)
                        {
                            //System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            //{
                            if ((LoaderViewModelManager.CurrentVM == LoaderMapConvert as IMainScreenViewModel) || LoaderMapConvert is IForcedLoaderMapConvert)
                            {
                                LoaderMapConvert.LoaderMapConvert(loaderInfo.StateMap);
                                LoaderMapConvert.UpdateChanged();
                                FoupInfoUpdate();
                            }

                            //})); // #Hynix_Merge: UI Lock 걸려서 주석 했던것 같음. 뭐가 맞는가?
                        }
                        if (TopBar != null)
                        {
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                TopBar.LoaderMapConvert(loaderInfo.StateMap);
                            }));
                        }

                        foreach (var client in LoaderMaster.ClientList)
                        {
                            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                            if (LoaderMaster.IsAliveClient(client.Value))
                            {
                                LoaderMaster.OnLoaderInfoChangedClient(client.Value, loaderInfo);
                                retVal = EventCodeEnum.NONE;
                            }
                            else
                            {
                                LoggerManager.Debug($"[LOADER] OnLoaderChanged() IsAliveClient Error{client.Key}");
                            }
                            if (retVal != EventCodeEnum.NONE)
                            {
                                //Log.Warn("[LOADER] OnLoaderChanged() : Notify Failed.");
                                LoggerManager.Debug($"[LOADER] OnLoaderChanged() : Notify Failed.");
                            }
                        }
                    }
                    if (ServiceCallback != null)
                    {
                        /*EventCodeEnum retVal =*/
                        ServiceCallback.OnLoaderInfoChanged(loaderInfo);
                        EventCodeEnum retVal = EventCodeEnum.NONE;

                        if (retVal != EventCodeEnum.NONE)
                        {
                            //Log.Warn("[LOADER] OnLoaderChanged() : Notify Failed.");
                            LoggerManager.Debug($"[LOADER] OnLoaderChanged() : Notify Failed.");
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task<EventCodeEnum> DoScanJob(int cassetteIdx)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            List<ILoaderJob> jobList = new List<ILoaderJob>();

            try
            {
                LoggerManager.Debug($"LoaderModule.DoScanJob(): FoupModule({cassetteIdx}): thread lock start.");
                Foup.FoupControllers[cassetteIdx - 1].Service.FoupModule.FplockSlim?.Wait();
                LoggerManager.Debug($"LoaderModule.DoScanJob(): FoupModule({cassetteIdx}): thread lock running.");
                var dstCstInfo = GetLoaderInfo().StateMap.CassetteModules.Where(i => i.ID.Index == cassetteIdx).FirstOrDefault();
                ScanCount = 25;
                var currCst = ModuleManager.FindModule<ICassetteModule>(dstCstInfo.ID);

                IGPLoader gpLoader = Container.Resolve<IGPLoader>();
                string foupstateStr = "";
                if (Foup.FoupControllers[cassetteIdx - 1].ValidationCassetteAvailable(out foupstateStr) == EventCodeEnum.NONE)
                {
                    if (LoaderMaster.ValidationFoupLoadedState(cassetteIdx, ref foupstateStr) == EventCodeEnum.NONE)
                    {
                        currCst.SetReadingScanState();
                        LoggerManager.Debug($"[LOADER] UpdateCassetteScanState() : Foup{cassetteIdx} Set reading scan state.");

                        ScanCassetteJob sch = new ScanCassetteJob();
                        sch.Init(this, currCst);
                        var jobResult = sch.DoSchedule();


                        ILoaderProcessModule scanModule = null;
                        if (SystemManager.SysteMode == SystemModeEnum.Single)
                        {
                            scanModule = new SensorScanCassette();
                        }
                        else if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                        {
                            scanModule = new GP_SensorScanCassette();
                        }
                        else
                        {
                            return retVal;
                        }
                        scanModule.Init(Container, jobResult.NextProc);
                        retVal = EventCodeEnum.NONE;
                        //await Task.Factory.StartNew(() =>
                        //{
                        //    while (true)
                        //    {
                        //        scanModule.Execute();
                        //        if (scanModule.State == LoaderProcStateEnum.DONE)
                        //        {
                        //            break;
                        //        }
                        //        Thread.Sleep(1);
                        //    }
                        //});
                        Task task = new Task(() =>
                        {
                            while (true)
                            {
                                scanModule.Execute();
                                if (scanModule.State == LoaderProcStateEnum.DONE || scanModule.State == LoaderProcStateEnum.SYSTEM_ERROR)
                                {
                                    break;
                                }
                                Thread.Sleep(1);
                            }

                        });
                        task.Start();
                        await task;

                        //MakeData And Send Gem Event
                        if (retVal == EventCodeEnum.NONE)
                        {
                            while (true)
                            {
                                if (currCst.ScanState == CassetteScanStateEnum.ILLEGAL || currCst.ScanState == CassetteScanStateEnum.READ)
                                {
                                    Foup.FoupControllers[cassetteIdx - 1]?.Service?.FoupModule.Cover.CheckState();
                                    if (currCst.ScanState == CassetteScanStateEnum.READ)
                                    {
                                        if (this.GEMModule().GemSysParam.ReceiveMessageType.Equals("SemicsGemReceiverSEKR") == false)
                                        {
                                            StringBuilder scanstr = new StringBuilder();
                                            var slots = GetLoaderInfo().StateMap.CassetteModules[currCst.ID.Index - 1].SlotModules.ToList();
                                            if (slots != null)
                                            {
                                                slots.Sort((slotTarget1, slotTarget2) => slotTarget1.ID.Index.CompareTo(slotTarget2.ID.Index));
                                                //foreach (var slot in this.LoaderModule.GetLoaderInfo().StateMap.CassetteModules[reqData.FoupNumber - 1].SlotModules)
                                                foreach (var slot in slots)
                                                {
                                                    if (slot.WaferStatus == EnumSubsStatus.EXIST)
                                                    {
                                                        scanstr.Append('3');
                                                    }
                                                    else
                                                    {
                                                        scanstr.Append('1');
                                                    }
                                                }

                                            }

                                            PIVInfo pivinfo = new PIVInfo(foupnumber: cassetteIdx, listofslot: scanstr.ToString());

                                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                            this.EventManager().RaisingEvent(typeof(ScanFoupDoneEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                            semaphore.Wait();
                                        }
                                    }
                                    break;
                                }
                                Thread.Sleep(10);
                            }
                        }
                    }
                    else
                    {
#pragma warning disable 4014
                        //brett// messagebox는 출력해 놓고 이후 작업(event 발생 등)을 실행하기 위해 await 하지 않음
                        this.MetroDialogManager().ShowMessageDialog("Foup Scan Error", $"Foup{cassetteIdx}, {foupstateStr}" +
                            $"\nRequired is Foup State: LOAD, Cassette State : LOADED, Port In : True" +
                            $"\nPlease reload this foup.", EnumMessageStyle.Affirmative);
#pragma warning restore 4014
                        retVal = EventCodeEnum.FOUP_ERROR;

                        PIVInfo pivinfo = new PIVInfo(foupnumber: cassetteIdx, listofslot: "");

                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(ScanFoupFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();

                        LoggerManager.Debug($"[LoaderModule] DoScanJob() can not execute.");
                    }
                }
                else
                {
#pragma warning disable 4014
                    //ann// messagebox는 출력해 놓고 이후 작업(event 발생 등)을 실행하기 위해 await 하지 않음
                    this.MetroDialogManager().ShowMessageDialog("Foup Scan Error", $"Foup{cassetteIdx}, {foupstateStr}" +
                        $"\nNo CST was detected for the specified CST type." +
                        $"\nPlease reload this foup.", EnumMessageStyle.Affirmative);
                    retVal = EventCodeEnum.FOUP_ERROR;
#pragma warning restore 4014

                    PIVInfo pivinfo = new PIVInfo(foupnumber: cassetteIdx, listofslot: "");

                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(ScanFoupFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    semaphore.Wait();

                    LoggerManager.Debug($"[LoaderModule] DoScanJob() can not execute. {foupstateStr}");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                Foup.FoupControllers[cassetteIdx - 1].Service.FoupModule.FplockSlim?.Release();
                LoggerManager.Debug($"LoaderModule.DoScanJob(): FoupModule({cassetteIdx})(): thread lock end.");
            }
            return retVal;

        }
        public EventCodeEnum ModuleInit(bool IsRefresh = false)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (ModuleState != ModuleStateEnum.RUNNING)
                {
                    Sequencer.Clear();
                    retVal = ModuleManager.InitAttachModules(IsRefresh);
                    BroadcastLoaderInfo();
                }
                else
                {
                    retVal = EventCodeEnum.MOTION_MOVING_ERROR;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public void SetTestCenteringFlag(bool centeringtestflag)
        {
            CenteringTestFlag = centeringtestflag;
        }

        public void SetLoaderMapConvert(ILoaderMapConvert mapConvert)
        {
            LoaderMapConvert = mapConvert;
        }
        public void SetTopBar(ILoaderMapConvert mapConvert)
        {
            TopBar = mapConvert;
        }
        public ILoaderMapConvert HandlingConvert { get; set; }
        public void SetLoaderMapConvertHandling(ILoaderMapConvert mapConvert)
        {
            HandlingConvert = mapConvert;
        }
        public void SetHandlingConvert()
        {
            LoaderMapConvert = HandlingConvert;
        }
        public bool IsFoupUnload(int foupindex)// 로더가 움직일수 있는지 확인. 
        {
            bool retVal = true;
            try
            {
                var remainJob = LoaderJobViewList.Count(x => x.JobDone == false);



                for (int i = 0; i < LoaderJobViewList.Count(); i++)
                {
                    if (LoaderJobViewList[i].CurrentHolder.ModuleType == ModuleTypeEnum.SLOT)
                    {
                        ICassetteModule cstModule = ModuleManager.FindModule(ModuleTypeEnum.CST, foupindex) as ICassetteModule;
                        var slotCount = ModuleManager.FindSlots(cstModule).Count();
                        int slotNum = LoaderJobViewList[i].CurrentHolder.Index % slotCount;
                        int offset = 0;
                        if (slotNum == 0)
                        {
                            slotNum = slotCount;
                            offset = -1;
                        }
                        int foupNum = ((LoaderJobViewList[i].CurrentHolder.Index + offset) / slotCount) + 1;

                        if (foupNum == foupindex && remainJob != 0)
                        {
                            retVal = false;
                            break;
                        }
                    }
                    if (LoaderJobViewList[i].DstHolder.ModuleType == ModuleTypeEnum.SLOT)
                    {
                        ICassetteModule cstModule = ModuleManager.FindModule(ModuleTypeEnum.CST, foupindex) as ICassetteModule;
                        var slotCount = ModuleManager.FindSlots(cstModule).Count();
                        int slotNum = LoaderJobViewList[i].DstHolder.Index % slotCount;
                        int offset = 0;
                        if (slotNum == 0)
                        {
                            slotNum = slotCount;
                            offset = -1;
                        }
                        int foupNum = ((LoaderJobViewList[i].DstHolder.Index + offset) / slotCount) + 1;

                        if (foupNum == foupindex && remainJob != 0)
                        {
                            retVal = false;
                            break;
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
        public string SlotToFoupConvert(ModuleID ID)
        {
            string retVal = null;
            try
            {
                if (ID != null)
                {
                    if (ID.ModuleType == ModuleTypeEnum.SLOT)
                    {
                        ICassetteModule cstModule = ModuleManager.FindModule(ModuleTypeEnum.CST, 1) as ICassetteModule;
                        var slotCount = ModuleManager.FindSlots(cstModule).Count();
                        int slotNum = ID.Index % slotCount;
                        int offset = 0;
                        if (slotNum == 0)
                        {
                            slotNum = slotCount;
                            offset = -1;
                        }
                        int foupNum = ((ID.Index + offset) / slotCount) + 1;

                        retVal = $"[ FOUP{foupNum} , SLOT{slotNum} ]";
                    }
                    else
                    {
                        retVal = ID.ToString();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public SubstrateSizeEnum GetDefaultWaferSize()
        {
            SubstrateSizeEnum retval = SubstrateSizeEnum.INCH12;

            try
            {
                retval = this.SystemParameter.DefaultWaferSize.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public bool GetUseLotProcessingVerify()
        {
            bool retval = false;

            try
            {
                retval = this.SystemParameter.UseLotProcessingVerify.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public long GetBlockingJobtimeforOpenedDoor()
        {
            long retval = 0;

            try
            {
                retval = this.SystemParameter.LoaderJobTimeoutforOpenedDoor.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum SetDeviceSize(SubstrateSizeEnum size, int Foupindex)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;

            try
            {
                this.DeviceSize = size;
                if (size == SubstrateSizeEnum.UNDEFINED)
                {
                    this.DeviceSize = GetDefaultWaferSize();
                    LoggerManager.Debug($"SetDeviceSize(). Wafer size is set to default wafer size {this.DeviceSize}");
                }

                if (SystemManager.SysteMode != SystemModeEnum.Single)
                {
                    errorCode = LoaderService.UpdateLoaderSystem(Foupindex);
                }
                else
                {
                    errorCode = errorCode = EventCodeEnum.NONE;
                }
                LoggerManager.Debug($"SetDeviceSize() Size:{size} Return Value={errorCode}");
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"SetDeviceSize Error. Err = {err.Message}");
            }

            return errorCode;
        }

        public CassetteTypeEnum GetCassetteType(int foupindex)
        {
            CassetteTypeEnum retval = CassetteTypeEnum.UNDEFINED;

            try
            {
                if (this.Foup != null && Foup.FoupControllers != null)
                {
                    if (Foup.FoupControllers.Count >= foupindex)
                    {
                        retval = Foup.FoupControllers[foupindex - 1].GetCassetteType();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public EventCodeEnum ValidateCassetteTypesConsistency(out string combinedLog)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            combinedLog = "";
            try
            {
                for (int i = 1; i <= SystemModuleCount.ModuleCnt.FoupCount; i++)
                {
                    CassetteTypeEnum currentCassetteType = currentCassetteType = GetCassetteType(i);

                    if (currentCassetteType != CassetteTypeEnum.FOUP_25)
                    {
                        retval = EventCodeEnum.CASSETTE_TYPE_MISMATCH;
                    }
                    combinedLog += $"Foup Index: {i}, Cassette Type: {currentCassetteType}\n";
                }

                if (retval == EventCodeEnum.UNDEFINED)
                {
                    retval = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        

        public EventCodeEnum SetCassetteDeviceSize(SubstrateSizeEnum size, int Foupindex)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;

            try
            {
                if (SystemManager.SysteMode != SystemModeEnum.Single)
                {
                    errorCode = LoaderService.UpdateCassetteSystem(size, Foupindex);
                }
                else
                {
                    errorCode = errorCode = EventCodeEnum.NONE;
                }
                LoggerManager.Debug($"SetCassetteDeviceSize() Size:{size} Return Value={errorCode}");
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"SetDeviceSize Error. Err = {err.Message}");
            }

            return errorCode;
        }
        private IGPLoader _GPLoader;
        public IGPLoader GPLoader
        {

            get { return _GPLoader; }
            set
            {
                if (value != _GPLoader)
                {
                    _GPLoader = value;
                    RaisePropertyChanged();
                }
            }
        }
        /// <summary>
        /// Scan 후에 cover 닫은 Cassette 판별하여 close 하는 함수 ( manual or lot) 
        /// </summary>
        /// <param name="cassetteModule"></param>
        /// // 
        // 1. 우선순위로 판단
        // 1-1. 우선순위 1 만 열어둔다.
        // 1-2. 우선순위 1은 아니지만, 우선순위 1 foup 에 있는 wafer 들이 다 unprocessed 이면, 열어둔다.
        // 2. 셀 할당으로 판단
        // 2-1. 나의 lot 에 할당된 모든 셀이 어느 lot 에도 할당되지 않았다면 열어둔다.
        // 2-2. 나의 lot 에 할당된 모든 셀이 이미 다른 lot 에 할당 되었으면  닫는다.         
        public async Task CloseFoupCoverFunc(ICassetteModule cassetteModule, bool IsManual = false)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                bool isCloseCover = true;       // default value = close   

                if (IsManual)
                {
                    if (cassetteModule.FoupState == FoupStateEnum.LOAD && cassetteModule.ScanState == CassetteScanStateEnum.READ)
                    {
                        await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");
                        Task task = new Task(() =>
                        {
                            retVal = this.FoupOpModule().GetFoupController(cassetteModule.ID.Index).Execute(new FoupCoverDownCommand());
                        });
                        task.Start();
                        await task;
                        if (retVal == EventCodeEnum.NONE)
                        {
                            cassetteModule.FoupCoverState = FoupCoverStateEnum.CLOSE;
                            LoggerManager.Debug("FoupCoverState: CLOSE / Manual Success" + "/" + cassetteModule.ID.Index.ToString());
                        }
                        else
                        {
                            LoggerManager.Debug("FoupCoverState: CLOSE / Manual Fail " + "/" + retVal.ToString());
                        }
                    }
                    else
                    {
                        LoggerManager.Debug("FoupCoverState can not change. Please check foup status. / Manual " + "/" + cassetteModule.ID.Index.ToString());
                    }
                }
                else
                {
                    foreach (var activeLotInfo in LoaderMaster.ActiveLotInfos)
                    {
                        if (activeLotInfo.AssignState == LotAssignStateEnum.ASSIGNED && activeLotInfo.State == LotStateEnum.Idle) // activeprocess 함수에서 호출하면 우선순위를 알수없고 idle 상태이다.
                        {
                            var Cassette = ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, activeLotInfo.FoupNumber);

                            bool isEqual(List<int> curassignedList, List<int> alreadyAssignedList)
                            {
                                if (curassignedList.Count() != alreadyAssignedList.Count())
                                {
                                    return false;
                                }
                                foreach (var n in curassignedList)
                                {
                                    if (!alreadyAssignedList.Contains(n))
                                    {
                                        return false;
                                    }
                                }
                                return true;
                            }

                            // 나에게 할당 된 Stage 모두 이미 다른 lot 에 할당되어있는지 확인.                                                        
                            if (isEqual(activeLotInfo.AssignedUsingStageIdxList, AlreadyAssignedStages))
                            {
                                // 나에게 할당 된 모든 cell 이 이미 다른 lot 에 할당되어있으면 close
                                isCloseCover = true;
                                LoggerManager.Debug("FoupCoverState: isCloseCover: true" + "/" + Cassette.ID.Index.ToString());

                                var runningCst = LoaderMaster.ActiveLotInfos.FindAll(item => item.AssignState == LotAssignStateEnum.ASSIGNED &&
                                                                                     item.State == LotStateEnum.Running);

                                if (runningCst != null)
                                {
                                    if (runningCst.Count() == 1)
                                    {
                                        // running 중인 cassette 1개 / 돌고있는 wafer 가 있다. close 한다.
                                        isCloseCover = true;
                                        LoggerManager.Debug($"FoupCoverState: isCloseCover: true" + "/" + Cassette.ID.Index.ToString());
                                        var isLastWafer = runningCst.Find(a => a.NotDoneSlotList.Count() == 1);
                                        if (isLastWafer != null)
                                        {
                                            // last wafer가 돌고 있다. // foup cover open 
                                            isCloseCover = false;
                                            LoggerManager.Debug("FoupCoverState: isCloseCover: false" + "/" + Cassette.ID.Index.ToString());
                                        }
                                    }
                                    else
                                    {
                                        // already running 중인 cassette N개
                                        isCloseCover = true;
                                        LoggerManager.Debug("FoupCoverState: isCloseCover: false" + "/" + Cassette.ID.Index.ToString());
                                    }

                                }
                            }
                            else
                            {
                                // 나에게 할당 된 모든 cell 이 이미 다른 lot 어디에도 없다면 open                                
                                isCloseCover = false;
                                AlreadyAssignedStages = activeLotInfo.AssignedUsingStageIdxList;
                                LoggerManager.Debug("FoupCoverState: isCloseCover: false" + "/" + Cassette.ID.Index.ToString());
                            }

                            if (isCloseCover)
                            {
                                if (Cassette.FoupState == FoupStateEnum.LOAD && Cassette.ScanState == CassetteScanStateEnum.READ)
                                {
                                    Task task = new Task(() =>
                                    {
                                        retVal = this.FoupOpModule().GetFoupController(Cassette.ID.Index).Execute(new FoupCoverDownCommand());
                                    });
                                    task.Start();
                                    await task;
                                    if (retVal == EventCodeEnum.NONE)
                                    {
                                        Cassette.FoupCoverState = FoupCoverStateEnum.CLOSE;
                                        LoggerManager.Debug("FoupCoverState: CLOSE / Online Success" + "/" + Cassette.ID.Index.ToString());
                                    }
                                    else
                                    {
                                        LoggerManager.Debug("FoupCoverState: CLOSE / Online Fail " + "/" + retVal.ToString());
                                    }
                                }
                                else
                                {
                                    LoggerManager.Debug("FoupCoverState can not change. Please check foup status. / Online " + "/" + Cassette.ID.Index.ToString());
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }
        public void ClearAlreadyAssignedStages()
        {
            try
            {
                AlreadyAssignedStages.Clear();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public EventCodeEnum SetTransferWaferSize(TransferObject transferSource, EnumSubsStatus sourceWaferStatus)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            SubstrateSizeEnum waferSize = SubstrateSizeEnum.UNDEFINED;
            int foupNum = -1;
            try
            {
                if (transferSource != null)
                {
                    if (sourceWaferStatus == EnumSubsStatus.EXIST)
                    {
                        if (transferSource.WaferType.Value == EnumWaferType.POLISH)
                        {
                            errorCode = EventCodeEnum.NONE;
                            waferSize = transferSource.PolishWaferInfo.Size.Value;
                            LoggerManager.Debug($"SetTransferWaferSize(): Polsih Wafer Size = {waferSize}");
                        }
                        else if (transferSource.WaferType.Value == EnumWaferType.STANDARD || transferSource.WaferType.Value == EnumWaferType.TCW)
                        {
                            errorCode = EventCodeEnum.NONE;
                            waferSize = transferSource.Size.Value;
                            LoggerManager.Debug($"SetTransferWaferSize(): {transferSource.CurrHolder.ModuleType} Wafer Size = {waferSize}");
                        }
                        else
                        {
                            errorCode = EventCodeEnum.WAFER_SIZE_ERROR;
                            waferSize = transferSource.Size.Value;
                            LoggerManager.Debug($"SetTransferWaferSize(): {transferSource.WaferType.Value} Wafer Size = {waferSize} ");
                        }

                        if (transferSource.PreAlignState == PreAlignStateEnum.SKIP)
                        {
                            var LoaderSize = SystemParameter.GetMaxWaferSizeHandledByLoader();
                            if (LoaderSize != SubstrateSizeEnum.UNDEFINED)
                            {
                                errorCode = EventCodeEnum.NONE;
                                waferSize = LoaderSize;
                                LoggerManager.Debug($"SetTransferWaferSize(): Max WaferSize Handled By Loader: Wafer Size = {waferSize}");
                            }

                            for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                            {
                                CassetteTypeEnum CassetteType = GetCassetteType(i + 1);
                                if (CassetteType != CassetteTypeEnum.FOUP_25)
                                {
                                    foupNum = i + 1;
                                    LoggerManager.Debug($"SetTransferWaferSize(): Max Wafer Offset Handled By Loader: CassetteType = {CassetteType}");
                                    break;
                                }
                            }
                        }


                        if (transferSource.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                        {
                            int offset = 0;
                            if (transferSource.OriginHolder.Index % 25 == 0)
                            {
                                offset = -1;
                            }
                            foupNum = ((transferSource.OriginHolder.Index + offset) / 25) + 1;
                        }

                    }
                    else if (sourceWaferStatus == EnumSubsStatus.UNDEFINED || sourceWaferStatus == EnumSubsStatus.UNKNOWN)
                    {
                        errorCode = EventCodeEnum.WAFER_SIZE_ERROR;
                    }
                    else
                    {
                        errorCode = EventCodeEnum.WAFER_SIZE_ERROR;
                    }

                    if (errorCode == EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"SetTransferWaferSize() Before Wafer Size: { this.DeviceSize}, After Wafer Size: {waferSize}");
                        errorCode = SetDeviceSize(waferSize, foupNum);
                    }
                }
                else
                {
                    LoggerManager.Debug($"SetTransferWaferSize(): transferSource obj is null");
                    errorCode = EventCodeEnum.PARAM_INSUFFICIENT;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"SetTransferWaferSize Error. Err = {err.Message}");
            }

            return errorCode;
        }

        public void SetTransferAbort()
        {
            try
            {
                if (LoaderRobotRunning)
                {
                    LoaderRobotAbortFlag = true;
                }
                else
                {
                    LoaderRobotAbortFlag = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool GetTransferAbort()
        {
            bool retVal = false;
            try
            {
                retVal = LoaderRobotAbortFlag;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }//end of class
}
