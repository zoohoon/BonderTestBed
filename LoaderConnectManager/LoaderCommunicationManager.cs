using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoaderCommunicationModule
{

    using Autofac;
    using CallbackServices;
    using CommunicationModule;
    using IOStateProxy;
    using IOStateServiceClient;
    using LoaderBase;
    using LoaderBase.Communication;
    using LoaderBase.FactoryModules.ServiceClient;
    using LoaderBase.FactoryModules.ViewModelModule;
    using LoaderMapView;
    using LoaderParameters;
    using LoaderParameters.Data;
    using LoaderRemoteMediatorModule;
    using LoaderServiceBase;
    using LogModule;
    using MetroDialogInterfaces;
    using MotionManagerProxy;
    using MultiLauncherProxy;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Loader;
    using ProberInterfaces.NeedleClean;
    using ProberInterfaces.PinAlign;
    using ProberInterfaces.PMI;
    using ProberInterfaces.PnpSetup;
    using ProberInterfaces.PolishWafer;
    using ProberInterfaces.Proxies;
    using ProberInterfaces.Retest;
    using ProberInterfaces.Temperature;
    using ProberInterfaces.Utility;
    using RelayCommandBase;
    using RemoteServiceProxy;
    using SerializerUtil;
    using ServiceInterfaces;
    using ServiceProxy;
    using SubstrateObjects;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.Threading;
    using System.Windows.Data;
    using System.Windows.Input;
    using CellInfo = LoaderMapView.CellInfo;

    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class LoaderCommunicationManager : INotifyPropertyChanged, ILoaderCommunicationManager, IFactoryModule, IDisposable, ILoaderFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //..Property
        private Autofac.IContainer _Container => this.GetLoaderContainer();

        public bool Initialized { get; set; }

        private IPnpManager PnpManager => _Container.Resolve<IPnpManager>();

        private ClientFactory _ClientFactory;

        public ClientFactory ClientFactory
        {
            get { return _ClientFactory; }
            set { _ClientFactory = value; }
        }

        //minskim// User Action으로 인한 Cell 선택 변경 시 Device 정보 획득이 완료될때까지 wait 하기 위한 AutoResetEvent임 
        private static AutoResetEvent AutoResetSelectStage = new AutoResetEvent(true);

        public IParam LoaderCommunicationParam => _LoaderCommParam;

        private LoaderCommunicationParameter _LoaderCommParam
             = new LoaderCommunicationParameter();

        public LoaderCommunicationParameter LoaderCommParam
        {
            get { return _LoaderCommParam; }
            set { _LoaderCommParam = value; }
        }


        private IOPortProxy _IOPortProxy;
        public IOPortProxy IOPortProxy
        {
            get { return _IOPortProxy; }
            set { _IOPortProxy = value; }
        }

        private MotionAxisProxy _MotionAxisProxy;

        public MotionAxisProxy MotionAxisProxy
        {
            get { return _MotionAxisProxy; }
            set { _MotionAxisProxy = value; }
        }

        private ImageDispHost _DispHostService;
        public ImageDispHost DispHostService
        {
            get { return _DispHostService; }
            set
            {
                if (value != _DispHostService)
                {
                    _DispHostService = value;
                    RaisePropertyChanged();
                }
            }
        }

        private DelegateEventHost _DelegateEventService;
        public DelegateEventHost DelegateEventService
        {
            get { return _DelegateEventService; }
            set
            {
                if (value != _DelegateEventService)
                {
                    _DelegateEventService = value;
                    RaisePropertyChanged();
                }
            }
        }

        private LoaderDataGatewayHost _DataGatewayService;

        public LoaderDataGatewayHost DataGatewayService
        {
            get { return _DataGatewayService; }
            set { _DataGatewayService = value; }
        }

        private ServiceHost _GPLoaderServiceHost;

        public ServiceHost GPLoaderServiceHost
        {
            get { return _GPLoaderServiceHost; }
            set { _GPLoaderServiceHost = value; }
        }
        private ServiceHost _LoaderServiceHost;

        public ServiceHost LoaderServiceHost
        {
            get { return _LoaderServiceHost; }
            set { _LoaderServiceHost = value; }
        }

        private string _DispUriString;

        public string DispUriString
        {
            get { return _DispUriString; }
            set { _DispUriString = value; }
        }

        private string _EventEelegateUriString;

        public string EventEelegateUriString
        {
            get { return _EventEelegateUriString; }
            set { _EventEelegateUriString = value; }
        }

        private string _DataGatewayUriString;

        public string DataGatewayUriString
        {
            get { return _DataGatewayUriString; }
            set { _DataGatewayUriString = value; }
        }



        private CameraModule.Camera _Camera;
        public CameraModule.Camera Camera
        {
            get { return _Camera; }
            set
            {
                if (value != _Camera)
                {
                    _Camera = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<IStageObject> _Cells = new ObservableCollection<IStageObject>();
        public ObservableCollection<IStageObject> Cells
        {
            get { return _Cells; }
            set
            {
                if (value != _Cells)
                {
                    _Cells = value;
                    RaisePropertyChanged();
                }
            }
        }



        public ILoaderCommunicationManagerCallback ServiceCallBack { get; private set; }
        //private ObservableCollection<ILotSetting_Object> _LotSettings = new ObservableCollection<ILotSetting_Object>();
        //public ObservableCollection<ILotSetting_Object> LotSettings
        //{
        //    get { return _LotSettings; }
        //    set
        //    {
        //        if (value != _LotSettings)
        //            _LotSettings = value;
        //        RaisePropertyChanged();
        //    }
        //}

        //private ObservableCollection<IStagelotSetting> _Cells_ForLot = new ObservableCollection<IStagelotSetting>();
        //public ObservableCollection<IStagelotSetting> Cells_ForLot
        //{
        //    get { return _Cells_ForLot; }
        //    set
        //    {
        //        if (value != _Cells_ForLot)
        //            _Cells_ForLot = value;
        //            RaisePropertyChanged();
        //    }
        //}

        public ILoaderSupervisor LoaderMaster => _Container.Resolve<ILoaderSupervisor>();

        public event ChangeSelectedStageHandler ChangeSelectedStageEvent;

        private StageObject _SelectedStage;
        public IStageObject SelectedStage
        {
            get { return _SelectedStage; }
            set
            {
                if (value != _SelectedStage)
                {
                    //minskim// 이전 Stage 변경 action이 완료될때 까지 대기 하도록 함,UpdateSelectedStage 내부 Task 수행중임에도 Set이 호출되는 상황이 있어 이를 방지하기 위함
                    //minskim// 대기 event reset은 UpdateSelectedStage가 내부 Task 종료 전에 return 되므로 UpdateSelectedStage의 finally 위치 에서 해야함(추후 수정시 Deadlock 조건이 발생하지 않도록 주의 필요)
                    AutoResetSelectStage.WaitOne();

                    // TODO : Reset ParamManager property

                    //if (_SelectedStage != null && _SelectedStage.StageInfo.IsConnected == true)
                    //{
                    //    this.GetParamManagerClient()?.SetChangedDeviceParam(true);
                    //    this.GetParamManagerClient()?.SetChangedSystemParam(true);
                    //}

                    _SelectedStage = (StageObject)value;

                    if (_SelectedStage == null)
                    {
                        SelectedCellInfo = null;
                        SelectedStageIndex = -1;

                    }
                    else
                    {
                        SelectedCellInfo = _SelectedStage.StageInfo as CellInfo;
                        SelectedStageIndex = _SelectedStage.Index;
                    }

                    var task = Task.Run(() => UpdateSelectedStage());
                    task.Wait();

                    RaisePropertyChanged();
                    if (ChangeSelectedStageEvent != null)
                        ChangeSelectedStageEvent();
                }
            }
        }

        private int _SelectedStageIndex;

        public int SelectedStageIndex
        {
            get { return _SelectedStageIndex; }
            set { _SelectedStageIndex = value; RaisePropertyChanged(); }
        }

        private CellInfo _SelectedCellInfo;

        public CellInfo SelectedCellInfo
        {
            get { return _SelectedCellInfo; }
            set { _SelectedCellInfo = value; }
        }

        private int _LastLoadStageIndex = -1;
        public int LastLoadStageIndex
        {
            get { return _LastLoadStageIndex; }
            set
            {
                if (value != _LastLoadStageIndex)
                {
                    _LastLoadStageIndex = value;
                }
            }
        }

        public IStageObject GetStageObject(int index = -1)
        {
            IStageObject stageObject = null;
            try
            {
                if (index == -1)
                {
                    if (SelectedStage != null)
                        index = SelectedStage.Index;
                }
                if (index != -1)
                {
                    stageObject = Cells[index];
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return stageObject;
        }

        public IWaferObject GetWaferObject(int index = -1)
        {
            IWaferObject retVal = null;
            try
            {
                var stageobj = GetStageObject(index);
                if (stageobj != null)
                {

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private async Task GetWaferObject(IStageObject stage)
        {
            try
            {
                //if (stage.StageMode == GPCellModeEnum.ONLINE & stage.StreamingMode == StreamingModeEnum.STREAMING_OFF)
                //{
                //    LoggerManager.Debug($"Stage#[{stage.Index}] GetWaferObject Reject : StageMode is ONLINE & StreamingMode is STREAMING_OFF");
                //    return;
                //}

                //StageSupervisorProxy stageproxy = (StageSupervisorProxy)GetStageSupervisorClient(stage.Index);
                StageSupervisorProxy stageproxy = GetProxy<IStageSupervisorProxy>(stage.Index) as StageSupervisorProxy;

                if (stageproxy == null)
                    return;

                IWaferObject waferobj = stageproxy?.GetWaferObject();

                if (waferobj == null)
                    return;

                waferobj.Init();
                waferobj.WaferDevObjectRef.Init();

                if (waferobj != null)
                {
                    waferobj.AlignState = stageproxy.GetAlignState(AlignTypeEnum.Wafer);
                }

                SubstrateInfoNonSerialized substratenonserial = null;

                substratenonserial = stageproxy?.GetSubstrateInfoNonSerialized();

                ISubstrateInfo subsinfo = waferobj.GetSubsInfo();

                // TODO : Check Count
                //subsinfo.Pads.DutPadInfos.Count

                //int tmpcount = subsinfo.Pads.DutPadInfos.Count;

                //for (int i = 1; i <= tmpcount / 2; i++)
                //{
                //    int removeindex = tmpcount - i;

                //    subsinfo.Pads.DutPadInfos.RemoveAt(removeindex);
                //}

                if (substratenonserial != null)
                {
                    subsinfo.ActualDeviceSize = substratenonserial.ActualDeviceSize;
                    subsinfo.ActualDieSize = substratenonserial.ActualDieSize;
                    subsinfo.WaferCenter = substratenonserial.WaferCenter;
                    subsinfo.WaferObjectChangedToggle = substratenonserial.WaferObjectChangedToggle;
                    subsinfo.DIEs = await stageproxy.GetConcreteDIEs();
                    subsinfo.DutCenX = substratenonserial.DutCenX;
                    subsinfo.DutCenY = substratenonserial.DutCenY;
                }

                this.StageSupervisor().WaferObject = waferobj;
                waferobj = null;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void GetProbeCardObject(IStageObject stage)
        {
            try
            {
                if (stage != null)
                {
                    //if (stage.StageMode == GPCellModeEnum.ONLINE & stage.StreamingMode == StreamingModeEnum.STREAMING_OFF)
                    //{
                    //    LoggerManager.Debug($"Stage#[{stage.Index}] GetProbeCardObject Reject : StageMode is ONLINE & StreamingMode is STREAMING_OFF");
                    //    return;
                    //}

                    //StageSupervisorProxy stageproxy = (StageSupervisorProxy)GetStageSupervisorClient(stage.Index);
                    StageSupervisorProxy stageproxy = GetProxy<IStageSupervisorProxy>(stage.Index) as StageSupervisorProxy;

                    IProbeCard probeCardObject = null;

                    if (stageproxy != null)
                    {
                        probeCardObject = stageproxy.GetProbeCardConcreteObject();
                    }

                    if (probeCardObject != null)
                    {
                        probeCardObject.AlignState = stageproxy.GetAlignState(AlignTypeEnum.Pin);
                    }

                    this.StageSupervisor().ProbeCardInfo = probeCardObject;
                }
                else
                {
                    LoggerManager.Debug($"GetProbeCardObject(): stage is null");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void GetMarkObject(IStageObject stage)
        {
            try
            {
                if (stage != null)
                {
                    //if (stage.StageMode == GPCellModeEnum.ONLINE & stage.StreamingMode == StreamingModeEnum.STREAMING_OFF)
                    //{
                    //    LoggerManager.Debug($"Stage#[{stage.Index}] GetMarkObject Reject : StageMode is ONLINE & StreamingMode is STREAMING_OFF");
                    //    return;
                    //}

                    //StageSupervisorProxy stageproxy = (StageSupervisorProxy)GetStageSupervisorClient(stage.Index);
                    StageSupervisorProxy stageproxy = GetProxy<IStageSupervisorProxy>(stage.Index) as StageSupervisorProxy;

                    IMarkObject markObject = null;

                    if (stageproxy != null)
                    {
                        markObject = stageproxy.GetMarkConcreteObject();
                    }


                    if (markObject != null)
                    {
                        markObject.AlignState = stageproxy.GetAlignState(AlignTypeEnum.Mark);
                    }

                    this.StageSupervisor().MarkObject = markObject;
                }
                else
                {
                    LoggerManager.Debug($"GetMarkObject(): stage is null");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private ObservableCollection<LauncherObject> _MultiLaunchers = new ObservableCollection<LauncherObject>();
        public ObservableCollection<LauncherObject> MultiLaunchers
        {
            get { return _MultiLaunchers; }
            set
            {
                if (value != _MultiLaunchers)
                {
                    _MultiLaunchers = value;
                    RaisePropertyChanged();
                }
            }
        }

        private LauncherObject _SelectedLauncher;
        public ILauncherObject SelectedLauncher
        {
            get { return _SelectedLauncher; }
            set
            {
                if (value != _SelectedLauncher)
                {
                    _SelectedLauncher = (LauncherObject)value;
                    RaisePropertyChanged();
                }
            }
        }

        private ILoaderSupervisor LoaderSupervisor => _Container.Resolve<ILoaderSupervisor>();
        ILoaderModule LoaderModule => _Container.Resolve<ILoaderModule>();
        private ILoaderViewModelManager LoaderViewModelManager => _Container.Resolve<ILoaderViewModelManager>();
        //private Guid _LoaderMainVMGuid { get; set; }

        public EventLogManager eventLogManager = LoggerManager.EventLogMg;

        #endregion

        #region //..Callback Services
        IOMappingsCallbackService iOMappingsCallbackService { get; set; }
        MotionManagerCallbackService motionManagerCallbackService { get; set; }
        StageSupervisorCallbackService stageSupervisorCallbackService { get; set; }
        ViewModelDataManagerCallbackService viewModelDataManagerCallbackService { get; set; }
        MultiExecuterCallbackService multiExecuterCallbackService { get; set; }
        #endregion

        #region //..Init & Parameter

        public LoaderCommunicationManager()
        {

        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            eventLogManager.CellLogAdd += AddStageAlram;
            eventLogManager.CellLogRemove += RemoveStageAlram;
            try
            {
                // REMOVED
                //_LoaderMainVMGuid = new Guid("3491BBA7-8AF7-EE23-58DA-E028066E22A1");

                ClientFactory = ClientFactory.GetFactory();
                System.Net.ServicePointManager.DefaultConnectionLimit = 200;
                iOMappingsCallbackService = new IOMappingsCallbackService();
                motionManagerCallbackService = new MotionManagerCallbackService();
                stageSupervisorCallbackService = new StageSupervisorCallbackService();
                viewModelDataManagerCallbackService = new ViewModelDataManagerCallbackService();
                multiExecuterCallbackService = new MultiExecuterCallbackService();

                LoadParameter();

                int stagecnt = 0;

                if (LoaderSupervisor == null)
                    stagecnt = 12;
                else
                    stagecnt = LoaderSupervisor.ClientList.Count;

                stagecnt = SystemModuleCount.ModuleCnt.StageCount;

                for (int i = 1; i <= stagecnt; i++)
                {
                    StageObject tmpstageobj = new StageObject(i);

                    ICollectionView ErrorCodeAlaramsView = CollectionViewSource.GetDefaultView(tmpstageobj.StageInfo.ErrorCodeAlarams);
                    ErrorCodeAlaramsView.SortDescriptions.Add(new SortDescription(nameof(AlarmLogData.ErrorOccurTime), ListSortDirection.Descending));

                    tmpstageobj.StageInfo.ErrorCodeAlarams.CollectionChanged += (s, e) =>
                    {
                        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                        {
                            ErrorCodeAlaramsView.Refresh();
                        }
                    };

                    Cells.Add(tmpstageobj);
                }

                SetLauncherList();

                InitMultiExecuterService();

                ConnectLaunchers();

                //Application.Current.MainWindow.Closing += new CancelEventHandler(MainWindow_Closing);
                InitLoaderService();
                InitDispHostService();
                InitDelegateEventService();
                InitDataGatewayService();

                retVal = EventCodeEnum.NONE;


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        //public int GetLauncherNum(string ip)
        //{
        //    LauncherNum = 0;

        //    try
        //    {
        //        foreach (var launcher in LoaderCommParam.LauncherParams)
        //        {
        //            if (launcher.IP == ip)
        //            {
        //                LauncherNum = (launcher.LauncherIndex);
        //            }
        //        }

        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        LoggerManager.Debug($"[{this.GetType().Name}] GetLauncherNum() : LauncherNum = {LauncherNum}");
        //        LauncherNum = 0;
        //    }

        //    return LauncherNum;
        //}

        Dictionary<int, bool> Loader_Connect = new Dictionary<int, bool>();
        Dictionary<int, List<string>> Launcher_List = new Dictionary<int, List<string>>();
        public void SetLauncherList()
        {
            try
            {
                foreach (var launcher in LoaderCommParam.LauncherParams)
                {
                    if (launcher.LauncherIndex > 0)
                    {
                        Launcher_List.Add(launcher.LauncherIndex - 1, new List<string>());
                        Loader_Connect.Add(launcher.LauncherIndex - 1, false);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }



        public async Task DiskAlarm(int lunchernum, string pc_name, string drivename)
        {
            try
            {
                //LauncherNum = GetLauncherNum(ip);
                EnumMessageDialogResult ret;

                if (lunchernum != 0)
                {
                    var isExist = Launcher_List[lunchernum-1].FirstOrDefault(x => x == drivename);

                    if (isExist == null)
                    {
                        Launcher_List[lunchernum - 1].Add(drivename);

                        try
                        {
                            if(LoaderMaster.GetGPLoader() != null)
                            {
                                LoaderMaster.GetGPLoader()?.LoaderBuzzer(true);
                            }
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                            LoggerManager.Debug($"[{ this.GetType().Name}] DiskAlarm() : GetGPLoader.LaoderBuzzer(true) exception, if GetGPLoader() != null -> LoaderBuzzer no execute");
                        }

                        ret = await this.MetroDialogManager().ShowMessageDialog("Disk Full Message", $"Please Check the information below  \n" + "Launcher Index :" + lunchernum + "\n" + "PC Name :" + pc_name + "\n" + "Drive Name :" + drivename, EnumMessageStyle.Affirmative);

                        if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                        {
                            Launcher_List[lunchernum - 1].Remove(drivename);
                            try
                            {
                                if (LoaderMaster.GetGPLoader() != null)
                                {
                                    LoaderMaster.GetGPLoader()?.LoaderBuzzer(false);
                                }
                            }
                            catch (Exception err)
                            {
                                LoggerManager.Exception(err);
                                LoggerManager.Debug($"[{ this.GetType().Name}] DiskAlarm() : GetGPLoader.LaoderBuzzer(false) exception, if GetGPLoader() != null -> LoaderBuzzer no execute");
                            }
                            
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"[{this.GetType().Name}] DiskAlarm() : Launcher_List[{lunchernum - 1}] duplication");
                    }
                }

            }
            catch (Exception err)
            {
                Launcher_List.Clear();
                LoggerManager.Exception(err);
                LoggerManager.Debug($"[{this.GetType().Name}] DiskAlarm() : Execption");
            }

        }

        public void GetDiskInfo(int index)
        {
            try
            {
                var launcher = GetLuncherProxyFormCellIndex(index);
                if (launcher != null)
                {
                    if (launcher != null)
                    {
                        if (launcher.IsOpened())
                        {
                            launcher.GetDiskInfo();
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"GetDiskInfo() : Launcher Index[{index}] Socket DisConnected");
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.Debug($"[{this.GetType().Name}] GetDiskInfo() Exception");
            }

        }




        public InitPriorityEnum InitPriority { get; set; }
        public EventCodeEnum InitModule(Autofac.IContainer container)
        {
            return EventCodeEnum.NONE;
        }

        private bool dispose = false;
        public void DeInitModule()
        {
            try
            {
                //MultiLaunchers 의 ServiceCallback LoaderExit
                foreach (var launcher in MultiLaunchers) // 활성 client DisConnect ServiceCallback
                {
                    ILoaderCommunicationManagerCallback LauncherServiceCallback = null;
                    if (launcher.ServiceCallback != null)
                    {
                        LauncherServiceCallback = launcher.ServiceCallback;
                        LauncherServiceCallback.LoaderExit();
                    }
                }

                foreach (var launcher in LoaderCommParam.LauncherParams)
                {
                    Loader_Connect[launcher.LauncherIndex - 1] = false;
                }

                foreach (var luncher in MultiLaunchers)
                {
                    luncher.MultiExecuterProxy.DeInitService();
                    luncher.ServiceCallback = null;
                }

                dispose = true;
                foreach (var cell in Cells)
                {
                    DisConnectStage(cell.Index);
                }

                try
                {
                    GPLoaderServiceHost.Close();
                    LoaderServiceHost.Close();
                }
                catch (Exception err)
                {
                    Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                    GPLoaderServiceHost.Abort();
                    LoaderServiceHost.Abort();
                }

                DispHostService.DeInitService();
                DelegateEventService.DeInitModule();
                this.EnvControlManager().DisConnect();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum LoadParameter()

        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                string OpParameterPath = Path.Combine(this.FileManager().GetRootParamPath(), @"Parameters\Loader\LoaderServiceParameter.json");

                if (File.Exists(OpParameterPath) == false)
                {
                    LoaderCommParam = CreateDefaultParamm();
                    LoaderCommParam.SetDefaultParam();
                    Extensions_IParam.SaveParameter(null, LoaderCommParam, null, OpParameterPath);

                }

                else
                {


                    retVal = EventCodeEnum.NONE;
                    IParam tmpParam = null;
                    retVal = this.LoadParameter(ref tmpParam, typeof(LoaderCommunicationParameter), null, OpParameterPath);

                    if (retVal != EventCodeEnum.NONE)
                    {
                        throw new Exception("[LoaderController] Faile LoadLoaderControllerParameter.");

                    }
                    else
                    {
                        LoaderCommParam = tmpParam as LoaderCommunicationParameter;

                    }
                }

                LoaderCommunicationParameter CreateDefaultParamm()
                {
                    return new LoaderCommunicationParameter();
                }


                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.PARAM_ERROR;
            }


            return retVal;
        }

        public EventCodeEnum SaveParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                string OpParameterPath = Path.Combine(this.FileManager().GetRootParamPath(), @"Parameters\Loader\LoaderServiceParameter.json");
                this.SaveParameter(LoaderCommParam, ".json", OpParameterPath, Extensions_IParam.FileType.JSON);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion

        #region //..InitService
        public ILoaderServiceCallback LoaderCallback { get; set; }
        public IGPLoaderService GPLoaderService { get; set; }
        public ILoaderService LoaderService => _Container.Resolve<ILoaderService>();

        #region //.. GPLoaderService
        public void InitLoaderService()
        {
            string localURI = $"net.tcp://{LoaderCommParam.LIP}";
            //string loaderCoreDllName = "LoaderCore.dll";

            string loaderCoreDllName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LoaderCore.dll");
            var loaderCoreAssembly = Assembly.LoadFrom(loaderCoreDllName);

            var gpServices = ReflectionEx.GetAssignableInstances<IGPLoaderService>(loaderCoreAssembly);
            GPLoaderService = gpServices.FirstOrDefault(s => s.GetServiceType() == LoaderServiceTypeEnum.WCF);
            GPLoaderService.SetContainer(_Container);
            try
            {
                Task task = new Task(() =>
                {
                    var netTcpBinding = new NetTcpBinding()
                    {
                        Security = new NetTcpSecurity() { Mode = SecurityMode.None },
                        ReceiveTimeout = TimeSpan.MaxValue,
                        SendTimeout = new TimeSpan(0, 10, 0),
                        OpenTimeout = new TimeSpan(0, 10, 0),
                        CloseTimeout = new TimeSpan(0, 10, 0),
                        MaxBufferPoolSize = 524288,
                        MaxReceivedMessageSize = 2147483647,
                        ReliableSession = new OptionalReliableSession() { InactivityTimeout = TimeSpan.FromMinutes(1), Enabled = true }
                    };


                    var serviceHost = new ServiceHost(GPLoaderService,
                        new Uri[] { new Uri($"{localURI}:{ServicePort.LoaderServicePort}/POS") });
                    serviceHost.AddServiceEndpoint(typeof(IGPLoaderService), netTcpBinding, "GPLService");

                    serviceHost.Open();
                    serviceHost.Opened += ServiceHost_Opened;
                    serviceHost.Faulted += ServiceHost_Faulted;
                    serviceHost.Closed += ServiceHost_Closed;
                    GPLoaderServiceHost = serviceHost;
                    LoggerManager.Debug("Service started. Available in following endpoints");
                    foreach (var serviceEndpoint in serviceHost.Description.Endpoints)
                    {
                        LoggerManager.Debug($"LoaderHost Create End Point : {serviceEndpoint.Address}");
                    }
                });
                task.Start();
                task.Wait();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void ServiceHost_Opened(object sender, EventArgs e)
        {
            InitLoaderHostService();
        }
        private void ServiceHost_Faulted(object sender, EventArgs e)
        {
            try
            {
                if (!dispose)
                {
                    LoggerManager.Debug("GPLService Host channel Faulted. Try Reopen");
                    (GPLoaderServiceHost as ICommunicationObject).Open();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void ServiceHost_Closed(object sender, EventArgs e)
        {
            try
            {
                LoggerManager.Debug($"GPLService Host_Closed(): Service Channel Closed.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public int InitLoaderHostService()
        {
            LoaderCallback = OperationContext.Current.GetCallbackChannel<ILoaderServiceCallback>();

            LoaderService.SetCallBack(LoaderCallback);
            LoggerManager.Debug($"InitHostService(): Hash = {LoaderCallback.GetHashCode()}");
            return LoaderCallback.GetHashCode();
        }

        //MultiExecuter & Loader Connect
        public void InitMultiExecuterService()
        {
            string localURI = $"net.tcp://{LoaderCommParam.LIP}";

            var ServiceHostOverTCP = new ServiceHost(this,
                new Uri[] { new Uri($"{localURI}:{ServicePort.MultiLauncherControlServicePort}/POS") });
            try
            {
                var tcpBinding = new NetTcpBinding()
                {
                    Security = new NetTcpSecurity() { Mode = SecurityMode.None },
                    ReceiveTimeout = TimeSpan.MaxValue,
                    SendTimeout = new TimeSpan(0, 10, 0),
                    OpenTimeout = new TimeSpan(0, 10, 0),

                    ReliableSession = new OptionalReliableSession() { InactivityTimeout = TimeSpan.FromMinutes(1), Enabled = false }
                };

                ServiceHostOverTCP.AddServiceEndpoint(
                    typeof(ILoaderCommunicationManager),
                    tcpBinding,
                    "LoaderSystemService");
                ServiceHostOverTCP.Open();
                ServiceHostOverTCP.Faulted += Host_Faulted;
                ServiceHostOverTCP.Closed += Host_Closed;
                LoaderServiceHost = ServiceHostOverTCP;

                Debug.Print("Service started. Available in following endpoints");
                foreach (var serviceEndpoint in ServiceHostOverTCP.Description.Endpoints)
                {
                    LoggerManager.Debug($"LoaderHost Create End Point : {serviceEndpoint.Address}");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void Host_Faulted(object sender, EventArgs e)
        {
            try
            {
                if (!dispose)
                {
                    LoggerManager.Debug($"LoaderCommunicationManager LoaderSystemService Host_Faulted(): Service Channel Faulted. Try Reopen");
                    (LoaderServiceHost as ICommunicationObject).Open();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void Host_Closed(object sender, EventArgs e)
        {
            try
            {
                LoggerManager.Debug($"LoaderCommunicationManager LoaderSystemService Host_Closed(): Service Channel Closed.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void Channel_Faulted(object sender, EventArgs e)
        {
            foreach (var lunch in MultiLaunchers)
            {
                if (lunch.MultiExecuterProxy.State == CommunicationState.Faulted || lunch.MultiExecuterProxy.State == CommunicationState.Closed)
                {
                    lunch.IsConnected = false;
                    Loader_Connect[lunch.Index - 1] = false;
                }
            }
        }
        private void LauncherService_Faulted(object sender, EventArgs e)
        {
            try
            {
                foreach (var lunch in MultiLaunchers)
                {
                    if (lunch.MultiExecuterProxy.State == CommunicationState.Faulted || lunch.MultiExecuterProxy.State == CommunicationState.Closed)
                    {
                        lunch.IsConnected = false;
                        Loader_Connect[lunch.Index] = false;

                        //ServiceCallback to Launcher
                        lunch.ServiceCallback = null;
                        lunch.MultiExecuterProxy.DeInitService();

                        LoggerManager.Debug($"LauncherIndex.{lunch.Index + 1}, LauncherService Client Channel faulted.");
                    }
                }

                if (dispose)
                {
                    (LoaderServiceHost as ICommunicationObject).Close();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private List<ILoaderCommunicationManagerCallback> clients = new List<ILoaderCommunicationManagerCallback>();
        public ILoaderCommunicationManagerCallback LoaderServiceCallBack { get; private set; }
        public Task LoaderInitService(int cellnum)
        {
            //multiexecuter가 실행 후 로더 연결시 호출되는 함수
            try
            {
                int lunchernum = FindLuncherIndex(cellnum);
                OperationContext.Current.Channel.Faulted += Channel_Faulted;

                ServiceCallBack = OperationContext.Current.GetCallbackChannel<ILoaderCommunicationManagerCallback>();
                if (ServiceCallBack != null)
                {
                    MultiLaunchers[lunchernum - 1].ServiceCallback = ServiceCallBack;
                }
                clients.Add(ServiceCallBack);
                LoggerManager.Debug($"InitHostService(): Hash = {ServiceCallBack.GetHashCode()}");

                var launcher = LoaderCommParam.LauncherParams.ToList<LauncherParameter>().Find(param => param.LauncherIndex == (lunchernum));

                //multiexecuter client proxy 생성 후 접속
                if (Loader_Connect[launcher.LauncherIndex - 1] == false) //ConnectLaunchers()
                {
                    if (launcher != null)
                    {
                        var launcherporxy = new MultiExecuterProxy(launcher.Port, multiExecuterCallbackService.GetInstanceContext(), launcher.IP);

                        bool isConnected = false;
                        if (CommunicationManager.CheckAvailabilityCommunication(launcher.IP, launcher.Port)) //MultiExecuter ON + Reconnect
                        {
                            launcherporxy.InitService();
                            launcherporxy.GetDiskInfo();
                            (launcherporxy as ICommunicationObject).Faulted += LauncherService_Faulted;
                            isConnected = true;
                            Loader_Connect[launcher.LauncherIndex - 1] = true;
                        }

                        MultiLaunchers[launcher.LauncherIndex - 1].MultiExecuterProxy = launcherporxy;
                        MultiLaunchers[launcher.LauncherIndex - 1].IsConnected = isConnected;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.Debug($"[{this.GetType().Name}] LoaderInitService() Exception");

            }
            return Task.CompletedTask;
        }

        public bool IsServiceAvailable()
        {
            return true;
        }

        public void ConnectLoaderClient(int index = -1)
        {
            if (index == -1)
                index = (int)_SelectedStage?.Index;
            if (index != -1)
            {
                var stage = GetProxy<IStageSupervisorProxy>(index);
                stage?.InitLoaderClient();
            }
        }
        #endregion

        #region //.. DispServiceHost
        public IImageDispHost GetDispHostService()
        {
            if (DispHostService == null)
                InitDispHostService();
            return DispHostService;
        }

        public void InitDispHostService()
        {
            try
            {
                if (DispHostService == null)
                {
                    DispHostService = new ImageDispHost();
                    DispUriString = DispHostService.InitService(
                        LoaderCommParam.DispProxyPort,
                        LoaderCommParam.LIP);
                }


                (GetDispHostService() as ImageDispHost).ImageUpdate
               += LoaderViewModelManager.DispHostService_ImageUpdate;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region //.. DialogServiceHost
        public IDelegateEventHost GetDelegateEventService()
        {
            return DelegateEventService;
        }

        private void InitDelegateEventService()
        {
            try
            {
                if (DelegateEventService == null)
                {
                    DelegateEventService = new DelegateEventHost();
                    EventEelegateUriString = DelegateEventService.InitService(
                        ServicePort.DialogProxyPort,
                        LoaderCommParam.LIP);
                }

                if(GetDelegateEventService()!= null)
                {
                    //GetDelegateEventService().MetroDialogShowChuckIndexEvent += _Container.Resolve<IMetroDialogManager>().ShowWindowChuckIndex;
                    //GetDelegateEventService().MetroDialogCloseChuckIndexEvent += _Container.Resolve<IMetroDialogManager>().CloseWindowChuckIndex;

                    GetDelegateEventService().MessageDialogShow += _Container.Resolve<IMetroDialogManager>().ShowMessageDialog;
                    GetDelegateEventService().MetroDialogShowEvent += _Container.Resolve<IMetroDialogManager>().ShowWindow;
                    GetDelegateEventService().MetroDialogCloseEvent += _Container.Resolve<IMetroDialogManager>().CloseWindow;

                    GetDelegateEventService().SingleInputDialogShow += _Container.Resolve<IMetroDialogManager>().ShowSingleInputDialog;
                    GetDelegateEventService().SingleInputDialogGetInputData += _Container.Resolve<IMetroDialogManager>().GetSingleInputData;

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region //.. DataGatewayServiceHost
        public ILoaderDataGateway GetDataGatewayService()
        {
            return DataGatewayService;
        }

        public void InitDataGatewayService()
        {
            try
            {
                if (DataGatewayService == null)
                {
                    DataGatewayService = new LoaderDataGatewayHost();
                    DataGatewayUriString = DataGatewayService.InitService(
                        ServicePort.DataGatewayProxyPort,
                        LoaderCommParam.LIP);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        public EventCodeEnum ConnectLaunchers()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            bool isConnected = false;

            try
            {
                if (LoaderCommParam != null)
                {
                    var context = new InstanceContext(this);

                    //Launcher
                    foreach (var launcher in LoaderCommParam.LauncherParams)
                    {
                        isConnected = false;

                        var launcherporxy = new MultiExecuterProxy(launcher.Port, multiExecuterCallbackService.GetInstanceContext(), launcher.IP);
                        // if( CheckAvailabilityAsync(launcher.IP, launcher.Port).Result)

                        if (CommunicationManager.CheckAvailabilityCommunication(launcher.IP, launcher.Port))
                        {
                            Loader_Connect[launcher.LauncherIndex - 1] = true;
                            launcherporxy.InitService();
                            launcherporxy.GetDiskInfo();
                            (launcherporxy as System.ServiceModel.ICommunicationObject).Faulted += LauncherService_Faulted;
                            isConnected = true;
                        }

                        MultiLaunchers.Add(new LauncherObject(launcher.LauncherIndex - 1, launcherporxy));
                        MultiLaunchers[launcher.LauncherIndex - 1].IsConnected = isConnected;

                        foreach (var param in launcher.StageParams)
                        {
                            MultiLaunchers[launcher.LauncherIndex - 1].CellIndexs.Add(param.Index);
                            var cell = Cells.ToList<IStageObject>().Find(stg => stg.Index == param.Index);
                            if (cell != null)
                            {
                                cell.LauncherIndex = launcher.LauncherIndex;
                                cell.StageInfo.EnableAutoConnect = param.EnableAutoConnect;
                            }
                        }

                    }
                }

                bStopUpdateThread = false;
                UpdateThread = new Thread(new ThreadStart(UpdateProc));
                UpdateThread.Name = this.GetType().Name;
                UpdateThread.Start();



            }
            catch (Exception err)
            {
                LoggerManager.Exception(err); 
                LoggerManager.Debug($"[{this.GetType().Name}] ConnectLaunchers() Exception");
            }
            return retVal;
        }

        public EventCodeEnum ConnectLauncher(int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var launcher = LoaderCommParam.LauncherParams.ToList<LauncherParameter>().Find(param => param.LauncherIndex == (index + 1));
                if (launcher != null)
                {
                    multiExecuterCallbackService = new MultiExecuterCallbackService();
                    var context = multiExecuterCallbackService.GetInstanceContext();
                    var launcherporxy = new MultiExecuterProxy(launcher.Port, context, launcher.IP);

                    if (CommunicationManager.CheckAvailabilityCommunication(launcher.IP, launcher.Port))
                    {
                        //if (MultiLaunchers[index].MultiExecuterProxy != null)
                        //  MultiLaunchers[index].MultiExecuterProxy.De
                        //  ();

                        //var launcherporxy = new MultiExecuterProxy(launcher.Port, context, launcher.IP);
                        MultiLaunchers[launcher.LauncherIndex - 1].IsConnected = true;
                        launcherporxy.InitService();
                        Loader_Connect[launcher.LauncherIndex - 1] = true;
                        if (launcherporxy.IsOpened())
                        {
                            MultiLaunchers[index].MultiExecuterProxy = launcherporxy;
                            retVal = EventCodeEnum.NONE;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.Debug($"[{this.GetType().Name}] ConnectLaunchers([{index}]) Exception");
            }
            return retVal;
        }


        public void DisConnectLauncher(int lunchernum)
        {
            try
            {
                if (LoaderCommParam != null)
                {
                    MultiLaunchers[lunchernum - 1].IsConnected = false;
                    Loader_Connect[lunchernum - 1] = false;

                    //ServiceCallback to Launcher
                    MultiLaunchers[lunchernum - 1].ServiceCallback = null;
                    //proxy abort , close
                    MultiLaunchers[lunchernum - 1].MultiExecuterProxy.DeInitService();

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public int GetLuncherProxyIndexFormCellIndex(int cellindex)
        {
            int returnindex = -1;
            bool iscontain = false;
            LauncherObject lunchobj = null;
            try
            {
                //var luncher = MultiLaunchers.Single(lunchers => lunchers.CellIndexs.Find(index => index == cellindex) != null);
                foreach (var lunch in MultiLaunchers)
                {
                    iscontain = lunch.CellIndexs.Contains(cellindex);
                    if (iscontain)
                    {
                        lunchobj = lunch;
                    }
                }
                if (lunchobj != null)
                    returnindex = lunchobj.Index;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return returnindex;
        }

        public IMultiExecuterProxy GetLuncherProxyFormCellIndex(int cellindex)
        {
            IMultiExecuterProxy proxy = null;
            bool iscontain = false;
            LauncherObject lunchobj = null;
            try
            {
                //var luncher = MultiLaunchers.Single(lunchers => lunchers.CellIndexs.Find(index => index == cellindex) != null);
                foreach (var lunch in MultiLaunchers)
                {
                    iscontain = lunch.CellIndexs.Contains(cellindex);
                    if (iscontain)
                    {
                        lunchobj = lunch;
                        break;
                    }
                }
                if (lunchobj != null)
                    proxy = lunchobj.MultiExecuterProxy;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return proxy;
        }

        public IMultiExecuterProxy GetLuncherProxyFormCellIndex(List<int> indexlist)
        {
            IMultiExecuterProxy proxy = null;
            bool iscontain = false;
            LauncherObject lunchobj = null;
            try
            {
                //var luncher = MultiLaunchers.Single(lunchers => lunchers.CellIndexs.Find(index => index == cellindex) != null);
                foreach (var lunch in MultiLaunchers)
                {
                    iscontain = lunch.CellIndexs.Contains(indexlist[0]);
                    if (iscontain)
                    {
                        lunchobj = lunch;
                    }
                }
                if (lunchobj != null)
                    proxy = lunchobj.MultiExecuterProxy;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return proxy;
        }

        public void StartProberSystem(List<int> indexlist)
        {
            try
            {
                foreach (var launcher in MultiLaunchers)
                {
                    var intersectlist = launcher.CellIndexs.Intersect(indexlist);
                    if (intersectlist.Count() > 0)
                    {
                        var prox = GetLuncherProxyFormCellIndex(intersectlist.FirstOrDefault());
                        if (prox != null)
                        {
                            if (prox.IsOpened())
                            {
                                List<int> tmp = new List<int>();
                                foreach (var item in intersectlist)
                                {
                                    tmp.Add(item);
                                }
                                prox.StartCellStageList(tmp);
                            }
                            else
                            {

                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ExitProberSystem(int cellindex = -1)
        {
            try
            {
                if (cellindex == -1)
                    cellindex = _SelectedStage.StageInfo.Index;

                var luncher = GetLuncherProxyFormCellIndex(cellindex);
                if (luncher.IsOpened())
                {
                    if (luncher != null)
                        luncher.ExitCell(cellindex);
                }
                else
                {
                    var lunchidx = GetLuncherProxyIndexFormCellIndex(cellindex);
                    var retval = ConnectLauncher(lunchidx);
                    luncher = GetLuncherProxyFormCellIndex(cellindex);

                    if (luncher.IsOpened())
                    {
                        luncher.ExitCell(cellindex);
                    }
                }


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public ObservableCollection<ILauncherObject> GetMultiLaunchers()
        {
            ObservableCollection<ILauncherObject> launcherObjects = new ObservableCollection<ILauncherObject>();
            try
            {
                foreach (var launcher in MultiLaunchers)
                {
                    launcherObjects.Add(launcher);
                }


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return launcherObjects;
        }

        /// <summary>
        /// 인자로 들어온 cell에 status soaking 사용중이라면 soaking module을 idle이나 running으로 될 수 있도록 정리해준다.
        /// </summary>
        /// <param name="Cell_Idx"> 확인될 Cell idx</param>
        /// <returns>true: soaking module이 정리됨, false: 정리안됨</returns>
        public bool Can_I_ChangeMaintenanceModeInStatusSoaking(int Cell_Idx)
        {
            try
            {
                var cell = LoaderMaster.GetClient(Cell_Idx);
                if(null != cell)
                {
                    bool statussoak_toglgle = cell.GetShowStatusSoakingSettingPageToggleValue();
                    if (false == statussoak_toglgle) //status soaking 미사용
                        return true;

                    cell.Check_N_ClearStatusSoaking(); // soaking을 정리하도록 처리
                    int waitingMaxTime = 10;
                    int waitCnt = 0;
                    while (true) // soaking module 상태가 maintenance mode로 갈 수 있는 상태인지 체크
                    {
                        Thread.Sleep(1000);
                        LoaderMaster.StageSoakingMode();
                        if (ModuleStateEnum.RUNNING != LoaderMaster.StageSetSoakingState)
                        {
                            LoggerManager.SoakingLog($"Soaking Module Clear to change maintenance.(ModuleState:{LoaderMaster.StageSetSoakingState}, cell idx:{Cell_Idx})");
                            return true;
                        }

                        waitCnt++;
                        if (waitCnt >= waitingMaxTime)
                        {
                            LoggerManager.SoakingErrLog($"Timeout - soaking module state is not changed.(SoakingModule, cell idx:{Cell_Idx}))");
                            return false;
                        }
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.SoakingErrLog($"Exception occure.(msg:{err.Message})");
            }

            return false;
        }

        public async Task<bool> ConnectStage(int index, bool skipMsgDialog = false)
        {
            bool retVal = false;
            IStageSupervisorProxy stage = null;

            IStageObject obj = null;
            try
            {
                //int launcherindex = -1;

                //if ((launcherindex = GetLauncherIndxex(index)) != -1)
                //{
                //    var launcherproxy = MultiLaunchers[launcherindex].MultiExecuterProxy as MultiExecuterProxy;
                //    if (launcherproxy.IsOpened())
                //    {
                //        if (!launcherproxy.GetCellAccessible(index))
                //        {
                //            retVal = false;
                //            return retVal;
                //        }
                //    }
                //}
                obj = GetStage(index);
                var stageinfo = GetStageCommParam(index);

                if (stageinfo != null && obj != null)
                {
                    obj.StageInfo.IsConnectChecking = true;

                    if (!obj.StageInfo.EnableAutoConnect)
                        SelectedStage = obj as StageObject;

                    string devname = string.Empty;

                    stage = InitProxy<IStageSupervisorProxy>(index);

                    //minskim// stage의 연결상태 확인 로직을 추가함 
                    if (stage == null || (stage as ServiceProxy.StageSupervisorProxy).State != CommunicationState.Opened)
                    {
                        if(!skipMsgDialog)
                        {
                            await this.MetroDialogManager().ShowMessageDialog("Error Message", "Could not connect Please check the communication again.", EnumMessageStyle.Affirmative);
                        }
                        return retVal;
                    }

                    //minskim// cell이 init중인 경우 연결하지않는다.
                    if(stage.GetStageInitState() == CellInitModeEnum.BeforeInit)
                    {
                        if (!skipMsgDialog)
                        {
                            await this.MetroDialogManager().ShowMessageDialog("Error Message", "The cell is in system init mode, so try later.", EnumMessageStyle.Affirmative);
                        }
                        return retVal;
                    }

                    stage?.InitLoaderClient();
                    stage?.SetDynamicMode(LoaderMaster.DynamicMode);
                    var gemConnState = stage?.InitGemConnectService();
                    if (gemConnState != EventCodeEnum.NONE)
                    {
                        if (!skipMsgDialog)
                        {
                            await this.MetroDialogManager().ShowMessageDialog("Error Message", "[SECS/GEM] Could not connect Please check the communication again.", EnumMessageStyle.Affirmative);
                        }
                        return retVal;
                    }
                    stage?.BindEventEelegateService(EventEelegateUriString);
                    stage?.BindDispService(DispUriString); //connect 할때 channel은 연결해 놓도록 함, 이후 flag로 조절하도록 함
                    stage?.BindDataGatewayService(DataGatewayUriString);
                    //InitMotionProxy(index);
                    //InitRemoteMediumProxy(index);

                    if (obj != null)
                    {
                        if (!obj.StageInfo.EnableAutoConnect)
                        {
                            SelectedCellInfo = (obj.StageInfo as CellInfo);
                        }

                        //연결시 얻어오는 Data

                        devname = stage?.GetDeviceName();

                        //===================
                        obj.StageInfo.DeviceName = devname;
                        obj.StageInfo.IsConnected = true;


                        //InitProxy<IMotionAxisProxy>(index);
                        InitProxy<IRemoteMediumProxy>(index);

                        (obj.DispHorFlip, obj.DispVerFlip) = stage.GetDisplayFlipInfo();
                        (obj.ReverseManualMoveX, obj.ReverseManualMoveY) = stage.GetReverseMoveInfo();

                        // 초기 연결 시
                        var stagemodeinfo = stage.GetStageMode();
                        bool isForcedDone = stage.IsForcedDoneMode();
                        if (isForcedDone)
                        {
                            obj.ForcedDone = EnumModuleForcedState.ForcedDone;
                        }
                        else
                        {
                            obj.ForcedDone = EnumModuleForcedState.Normal;
                        }

                        await SetStageMode(stagemodeinfo.Item1, stagemodeinfo.Item2, false, obj.Index, true, true);// => Loader 에서 set 하는게 아니라 스테이지로부터 정보를 얻어와야한다.                        
                        LoaderMaster.InitStageWaferObject(index);

                        stage.LoaderConnected();
                        Cells[index - 1].MonitoringBehaviorList = LoaderMaster.GetMonitoringBehaviorFromClient(index);
                    }
                    obj.Reconnecting = false;
                    retVal = true;
                }
            }
            catch (Exception err)
            {
                if (!skipMsgDialog)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Error Message", "Could not connect Please check the communication again.", EnumMessageStyle.Affirmative);
                }

                LoggerManager.Exception(err);
            }
            finally
            {
                if(obj != null)
                {
                    obj.StageInfo.IsConnectChecking = false;
                }
                if (retVal == false)
                {
                    AbortStage(index);
                }
            }
            return retVal;
        }

        object stageLockObject = new object();
        public IStageObject GetStage(int index = -1)
        {
            IStageObject stage = null;

            try
            {
                lock (stageLockObject)
                {
                    if (index == -1 & _SelectedStage != null)
                        index = _SelectedStage.Index;
                    if (index != -1)
                    {
                        stage = Cells.ToList<IStageObject>().Find(cell => cell.Index == index);
                    }
                    else
                    {
                        // LoggerManager.Error($"Selected Index is {index}. The index value is wrong. Maybe selected cell's information is not exist.");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return stage;
        }

        public int GetLauncherIndxex(int index)
        {
            int retVal = -1;
            try
            {
                foreach (var launcher in MultiLaunchers)
                {
                    foreach (var cell in launcher.CellIndexs)
                    {
                        if (cell == index)
                        {
                            retVal = launcher.Index;
                            break;
                        }
                    }
                }
                //var launcherparam = MultiLaunchers.ToList<LauncherObject>().Find(launcher => launcher.CellIndexs.Find(stageindex => stageindex == index) != null);
                //if (launcherparam != null)
                //    retVal = launcherparam.Index;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        AutoResetEvent areUpdateEvent = new AutoResetEvent(false);
        bool bStopUpdateThread;
        Thread UpdateThread;


        public void ProbingTimeUpdate()
        {
            try
            {
                for (int i = 0; i < Cells.Count(); i++)
                {
                    if (Cells[i].IsProbing)
                    {
                        string time = (DateTime.Now.Subtract(Cells[i].ProbingTime)).ToString();

                        Cells[i].ProbingTimeStr = " (" + time.Substring(0, time.IndexOf('.')) + ")";
                    }
                    else
                    {
                        Cells[i].ProbingTimeStr = "";
                    }

                    if ((Cells[i] as StageObject).ManualZUPState == ManualZUPStateEnum.Z_UP)
                    {
                        string time = (DateTime.Now.Subtract((Cells[i] as StageObject).ManualProbingTime)).ToString();

                        (Cells[i] as StageObject).ManualProbingTimeStr = " (" + time.Substring(0, time.IndexOf('.')) + ")";
                    }
                    else
                    {
                        (Cells[i] as StageObject).ManualProbingTimeStr = "";
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
        }
        private void UpdateProc()
        {
            try
            {
                while (bStopUpdateThread == false)
                {
                    try
                    {
                        ProbingTimeUpdate();
                        System.Threading.Thread.Sleep(500);
                    }
                    catch (Exception err)
                    {
                        System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
        }

        public Tuple<string, int> GetStageIPPortInfo(int index)
        {
            Tuple<string, int> ret = null;
            try
            {
                var param = GetStageCommParam(index);
                if (param != null)
                {
                    ret = new Tuple<string, int>(param.IP, param.Port);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
        public Tuple<string, int> GetLuncherIPPortInfo(int index)
        {
            Tuple<string, int> ret = null;
            try
            {
                var param = GetLuncherParam(index);
                if (param != null)
                {
                    ret = new Tuple<string, int>(param.IP, param.Port);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }


        private int[] PauseCount = new int[12];
        //private ILoaderMainVM LoaderMainVM { get; set; }
        private void UpdateStageState()
        {
            for (int i = 0; i < Cells.Count; i++)
            {
                if (LoaderSupervisor.StageStates.Count > 0)
                {
                    if (LoaderSupervisor.StageStates[i] == ModuleStateEnum.PAUSED || LoaderSupervisor.StageStates[i] == ModuleStateEnum.PAUSING)
                    {
                        if (Cells[i].StageState == ModuleStateEnum.PAUSING)
                        {
                            if (PauseCount[i] > 7)
                            {
                                Cells[i].StageState = ModuleStateEnum.PAUSED;
                                PauseCount[i] = 0;
                            }
                            PauseCount[i]++;
                        }
                        else if (Cells[i].StageState == ModuleStateEnum.PAUSED)
                        {
                            if (PauseCount[i] > 7)
                            {
                                Cells[i].StageState = ModuleStateEnum.PAUSING;
                                PauseCount[i] = 0;
                            }
                            PauseCount[i]++;

                        }
                        else
                        {
                            Cells[i].StageState = LoaderSupervisor.StageStates[i];
                        }
                    }
                    else
                    {
                        Cells[i].StageState = LoaderSupervisor.StageStates[i];
                    }
                }

                if (Cells[i].StageInfo.IsReservePause == true)
                {
                    Cells[i].StageInfo.IsReservePause = LoaderSupervisor.GetClient(Cells[i].Index)?.GetReservePause() ?? false;
                }
                Thread.Sleep(20);
            }
            //if (LoaderMainVM != null)
            //{
            //    LoaderMainVM.UpdateStageState();
            //}
        }
        //public void SetLoaderMainView(ILoaderMainVM mainVM)
        //{
        //    LoaderMainVM = mainVM;
        //}

        private StageCommiuncationParameter GetStageCommParam(int index = -1)
        {
            StageCommiuncationParameter parameter = null;
            try
            {
                if (index == -1 & _SelectedStage != null)
                {
                    index = _SelectedStage.StageInfo.Index;
                }

                //index 1 ~ 12               
                if (index != -1)
                {
                    foreach (var launcher in LoaderCommParam.LauncherParams)
                    {
                        parameter = launcher.StageParams.ToList<StageCommiuncationParameter>().Find(param => param.Index == index);
                        if (parameter != null)
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }

            return parameter;
        }

        private LauncherParameter GetLuncherParam(int index = -1)
        {
            LauncherParameter luncherparam = null;
            try
            {
                luncherparam = LoaderCommParam.LauncherParams.ToList<LauncherParameter>().Find(param => param.LauncherIndex == (index + 1));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return luncherparam;
        }

        public int FindLuncherIndex(int cellnum)
        {
            int luncherindex = -1;
            try
            {
                int launchercount = LoaderCommParam.LauncherParams.Count;
                for (int i = 0; i <= launchercount; i++)
                {
                    var existcellnum = LoaderCommParam.LauncherParams[i].StageParams.ToList<StageCommiuncationParameter>().Find(param => param.Index == cellnum);
                    if (existcellnum != null)
                    {
                        luncherindex = i + 1;
                        break;
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                LoggerManager.Exception(err);
            }
            return luncherindex;
        }

        public string GetStageDeviceName(int index)
        {
            try
            {
                if (Cells.Where(cell => cell.StageInfo.Index == index).Count() != 0)
                {
                    var stageproxy = GetProxy<IStageSupervisorProxy>(index);

                    if (stageproxy != null)
                    {
                        return stageproxy.GetDeviceName();
                    }
                }
                else
                    return null;
            }
            catch (Exception err)
            {
                throw err;
            }
            return null;

        }

        public void ChangeStreamingMode(IStageObject obj, bool bOn)
        {
            try
            {
                if(obj != null)
                {
                    GetProxy<IStageSupervisorProxy>(obj.Index)?.SetAcceptUpdateDisp(bOn);
                    if (bOn)
                    {
                        LoaderMaster.SetStreamingMode(obj.Index, StreamingModeEnum.STREAMING_ON);
                        obj.StreamingMode = StreamingModeEnum.STREAMING_ON;
                    }
                    else
                    {
                        LoaderMaster.SetStreamingMode(obj.Index, StreamingModeEnum.STREAMING_OFF);
                        obj.StreamingMode = StreamingModeEnum.STREAMING_OFF;
                    }
                }                         
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool DisConnectStage(int index = -1)
        {
            bool retVal = false;

            if (index == -1)
            {
                if (_SelectedStage != null)
                    index = _SelectedStage.Index;
                else
                    return false;
            }

            if (Cells == null || Cells.Count == 0)
                return false;

            var stage = Cells.Single(cellinfo => cellinfo.Index == index);

            try
            {
                if (stage != null)
                {
                    stage.StageInfo.IsConnectChecking = true;
                    CellInfo cellinfo = stage.StageInfo as CellInfo;
                    var stageinfo = GetStageCommParam(index);

                    //brett// cell이 강제 종료된 경우(gem 연결이 끊어진 경우) Proxy를 얻어오려 할 경우 hang 발생. 이런 경우는 abort 처리 하도록 한다.
                    bool bForceAbort = !this.GEMModule().GemCommManager.GetRemoteConnectState(index);
                    var client = LoaderMaster.GetClient(index, bForceAbort);
                    if (!bForceAbort && client != null && CommunicationManager.CheckAvailabilityCommunication(stageinfo.IP, stageinfo.Port + PortOffsets.StageSupervisorServicePortOffset))
                    {
                        if (stage.StageInfo.LotData != null && stage.StageInfo.LotData.LotState != null)
                        {
                            if (stage.StageInfo.LotData.LotState.Equals(ModuleStateEnum.RUNNING.ToString()))
                            {
                                this.MetroDialogManager().ShowMessageDialog("Warning Message",
                                    "Can not disconnect when the LOT is running.", EnumMessageStyle.Affirmative);
                                return false;
                            }
                        }

                        LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Try Disconnect cell({index})");

                        ChangeStreamingMode(stage, false);

                        GetDispHostService()?.Disconnect(index);
                        GetDelegateEventService()?.Disconnect(index);
                        GetDataGatewayService()?.Disconnect(index);

                        GPLoaderService?.Disconnect(index);
                        this.EnvControlManager()?.DisConnect(index);
                        // GPLoaderService.Disconnect() 호출 이후 Gem 연결을 끊어 주어야 함
                        GetProxy<IStageSupervisorProxy>(index)?.DeInitGemConnectService();
                        cellinfo.DisConnectProxies();
                    }
                    else
                    {
                        LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Already Disconnected cell({index})");

                        var hostclient = GetDispHostService().GetDispHostClient(index);
                        if (hostclient != null)
                        {
                            (hostclient as ICommunicationObject).Abort();
                        }

                        var eventclient = GetDelegateEventService().GetDialogHostClient(index);
                        if (eventclient != null)
                        {
                            (eventclient as ICommunicationObject).Abort();
                        }

                        var datatgatewayclient = GetDataGatewayService().GetDataGatewayHostClient(index);
                        if (datatgatewayclient != null)
                        {
                            (datatgatewayclient as ICommunicationObject).Abort();
                            LoggerManager.Debug($"DataGateway Host Callback Channel Abort. cell index = {index}");
                        }


                        cellinfo.AbortProxies();

                        LoaderMaster.RemoveClientAtList(index);
                    }

                    stage.StageInfo.Clear();
                    stage.StageInfo.SetTitles.Clear();
                    stage.StageInfo.LastTitle = string.Empty;

                    stage.StageState = ModuleStateEnum.UNDEFINED;
                    stage.StageMode = GPCellModeEnum.DISCONNECT;
                    stage.StageInfo.LotData = null;
                    //stage.WaferStatus = EnumSubsStatus.UNDEFINED;
                    //stage.CardStatus = EnumSubsStatus.UNDEFINED;

                    if (stage is StageObject stageObject)
                    {
                        stageObject.ChillingTimeProgressBar_Visibility = System.Windows.Visibility.Hidden;
                    }

                    stage.ForcedDone = EnumModuleForcedState.Normal;
                    SelectedCellInfo = null;

                    if (this.LoaderSupervisor.StageStates.Count > 0)
                    {
                        this.LoaderSupervisor.StageStates[stage.Index - 1] = ModuleStateEnum.UNDEFINED;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (stage != null)
                {
                    stage.Reconnecting = false;
                    stage.StageInfo.IsConnectChecking = false;
                }
            }
            return retVal;
        }
        public void Dispose()
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private bool IsStageWorking = false;
        private bool IsLoaderWorking = false;

        public void SetStageWorkingFlag(bool flag)
        {
            try
            {
                IsStageWorking = flag;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetLoaderWorkingFlag(bool flag)
        {
            try
            {
                IsLoaderWorking = flag;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public async Task WaitStageJob()
        {
            await semaphoreSlim.WaitAsync();

            try
            {
                IsStageWorking = true;

                while (IsStageWorking == true)
                {
                    //_delays.DelayFor(1);
                    System.Threading.Thread.Sleep(30);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public ObservableCollection<IStageObject> GetStages()
        {
            return Cells;
        }



        #region // Get Proxy Sample
        /// <summary>
        /// CellMode에 따라 Proxy 를 접근할 수 있는지 없는지 확인하는 함수.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index"></param>
        /// <returns></returns>
        //private bool CheckGetProxy<T>(int index)
        //{
        //    bool retVal = false;
        //    try
        //    {
        //        var stage = GetStage(index);
        //        if (stage != null)
        //        {
        //            if (stage.StageInfo.IsConnected)
        //            {
        //                if (stage.StageMode == GPCellModeEnum.ONLINE)
        //                {
        //                    if ((typeof(T) == typeof(IRemoteMediumProxy)) | (typeof(T) == typeof(IStageSupervisorProxy)) | (typeof(T) == typeof(ITempControllerProxy))
        //                        | typeof(T) == typeof(IFileManagerProxy) | typeof(T) == typeof(ISoakingModuleProxy) | (typeof(T) == typeof(IParamManagerProxy)))
        //                    {
        //                        retVal = true;
        //                    }
        //                }
        //                else if (stage.StageMode == GPCellModeEnum.MAINTENANCE | stage.StageMode == GPCellModeEnum.OFFLINE)
        //                {
        //                    retVal = true;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    return retVal;
        //}

        //object waClientLock = new object();
        //public IWaferAlignerProxy GetWaferAlignerClient(int index = -1)
        //{
        //    IProberProxy client = null;
        //    IStageObject stageobj = null;

        //    try
        //    {
        //        if (index == -1 & SelectedStage != null)
        //        {
        //            index = _SelectedStage.StageInfo.Index;
        //        }

        //        if (index != -1 & CheckGetProxy<IWaferAlignerProxy>(index))
        //        {
        //            IProberProxy proxy = null;
        //            stageobj = GetStage(index);
        //            stageobj?.StageInfo.Proxies.TryGetValue(typeof(IWaferAlignerProxy), out client);
        //            if (client != null)
        //            {
        //                if ((client as System.ServiceModel.ICommunicationObject).State == CommunicationState.Opened)
        //                {
        //                    //if(client.IsServiceAvailable() == false)
        //                    //{
        //                    //    (client as ICommunicationObject).Abort();
        //                    //    InitProxy();
        //                    //}
        //                    return (IWaferAlignerProxy)client;
        //                }
        //                else
        //                {
        //                    (client as System.ServiceModel.ICommunicationObject).Abort();
        //                    InitProxy();
        //                }
        //            }
        //            else
        //            {
        //                InitProxy();
        //            }
        //        }

        //        void InitProxy()
        //        {
        //            client = InitWaferAlignerProxy(index);
        //            RegisteProxyToStage(typeof(IWaferAlignerProxy), client, index);
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw err;
        //    }
        //    return (IWaferAlignerProxy)client;
        //}
        #endregion

        #region //..CallBack
        private ObservableCollection<IOPortDescripter<bool>> _Inputs = new ObservableCollection<IOPortDescripter<bool>>();
        public ObservableCollection<IOPortDescripter<bool>> Inputs
        {
            get { return _Inputs; }
            set
            {
                if (value != _Inputs)
                {
                    _Inputs = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<IOPortDescripter<bool>> _Outputs = new ObservableCollection<IOPortDescripter<bool>>();
        public ObservableCollection<IOPortDescripter<bool>> Outputs
        {
            get { return _Outputs; }
            set
            {
                if (value != _Outputs)
                {
                    _Outputs = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<ProbeAxisObject> _Axes = new ObservableCollection<ProbeAxisObject>();
        public ObservableCollection<ProbeAxisObject> Axes
        {
            get { return _Axes; }
            set
            {
                if (value != _Axes)
                {
                    _Axes = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion
        #region //.. Motion Update
        private double _MotionAxesUpdateTime = 3500;

        public double MotionAxesUpdateTime
        {
            get { return _MotionAxesUpdateTime; }
            set { _MotionAxesUpdateTime = value; }
        }

        public void SetAxesStateUpdateTime(double time)
        {
            MotionAxesUpdateTime = time;
        }

        public ProbeAxisObject GetAxis(EnumAxisConstants axis)
        {
            try
            {
                return Axes.ToList<ProbeAxisObject>().Find(ax => ax.AxisType.Value == axis);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return null;
        }
        #endregion

        public void SetWaferMapCamera(EnumProberCam cam)
        {
            try
            {
                var stageproxy = GetProxy<IStageSupervisorProxy>();

                if (stageproxy != null)
                {
                    stageproxy.SetWaferMapCam(cam);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            //_SelectedStage.StageInfo.StageProxy.SetWaferMapCam(cam);
        }

        public IWaferObject SetWaferObjectState(IStageObject stage, AlignStateEnum state)
        {
            IWaferObject retVal = null;
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion

        #region //..Command

        private WaferObject _WaferObject;
        public WaferObject WaferObject
        {
            get { return _WaferObject; }
            set
            {
                if (value != _WaferObject)
                {
                    _WaferObject = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string str_stage = "STAGE";
        private string _CurListView = "STAGE";
        public string CurListView
        {
            get { return _CurListView; }
            set
            {
                if (value != _CurListView)
                {
                    _CurListView = value;
                    RaisePropertyChanged();
                }
            }
        }



        private AsyncCommand<object> _ConnectCommand;
        public ICommand ConnectCommand
        {
            get
            {
                if (null == _ConnectCommand) _ConnectCommand = new AsyncCommand<object>(ConnectToProxy);
                return _ConnectCommand;
            }
        }


        
        private async Task ConnectToProxy(object obj)
        {
            Stopwatch stw = new Stopwatch();

            try
            {
                stw.Reset();
                stw.Start();

                if (CurListView.Equals(str_stage))
                {
                    int cellindex = Convert.ToInt32((_SelectedStage.Name.Split('#'))[1]);
                    var connectVal = await ConnectStage(cellindex);

                    if (connectVal)
                    {
                        //(_SelectedStage.StageInfo as CellInfo).StageProxy = GetStageSupervisorClient(cellindex);
                        System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            try
                            {
                                stw.Stop();
                                if (_SelectedStage != null)
                                {
                                    _SelectedStage.StageInfo.IsConnected = true;
                                    _SelectedStage.StageInfo.IsExcuteLot = true;
                                }
                            }
                            catch (Exception err)
                            {
                                LoggerManager.Exception(err);
                            }
                        }));
                    }

                }
                else
                {
                    if (SelectedLauncher != null)
                        ConnectLauncher(SelectedLauncher.Index);
                }

            }
            catch (Exception err)
            {

                LoggerManager.Error($"ConnectToProxy(): Error occurred. Err = {err.Message}");

            }
            finally
            {

            }

        }
        #endregion

        public async Task<EventCodeEnum> DeviceReload(IStageObject stage, bool forced = false)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (stage != null)
                {
                    bool canLoad = false;

                    if (forced == true)
                    {
                        canLoad = true;
                    }
                    else
                    {
                        // 순서 주의 : WaferObject -> ProbeCard
                        // ProbeCard DutSizeX와 DutSizeY의 Getter에서 WaferObject 데이터를 사용하기 때문
                        if (stage.StageMode == GPCellModeEnum.ONLINE & stage.StreamingMode == StreamingModeEnum.STREAMING_OFF)
                        {
                            canLoad = false;

                            LoggerManager.Debug($"Stage#[{stage.Index}] DeviceReload reject : StageMode is ONLINE & StreamingMode is STREAMING_OFF");
                        }
                        else
                        {
                            canLoad = true;
                        }
                    }

                    if (canLoad)
                    {
                        await GetWaferObject(stage);
                        GetProbeCardObject(stage);
                        GetMarkObject(stage);
                    }
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        //public async Task<EventCodeEnum> DeviceReload()
        //{
        //    EventCodeEnum retval = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        // 순서 주의 : WaferObject -> ProbeCard
        //        // ProbeCard DutSizeX와 DutSizeY의 Getter에서 WaferObject 데이터를 사용하기 때문
        //        if(_SelectedStage != null)
        //        {
        //            await GetWaferObject(_SelectedStage);
        //            GetProbeCardObject(_SelectedStage);
        //            GetMarkObject(_SelectedStage);
        //        }

        //        retval = EventCodeEnum.NONE;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        public async Task UpdateSelectedStage(IStageObject stageObj = null, bool bSkipCheck = false)
        {
            try
            {
                if (stageObj != null && SelectedStage != null)
                {
                    if (SelectedStage.Index != stageObj.Index)
                    {
                        return;
                    }
                }
                // 바뀐 셀이 연결되어 있는 경우, GEM 연결이 끊어져 있으면 Cell이 Down된 것으로 판단한다.
                if (SelectedStage != null && SelectedStage.StageInfo.IsConnected == true 
                    && this.GEMModule().GemCommManager.GetRemoteConnectState(SelectedStage.Index))
                {
                    bool NeedLoadMotionInfo = false;
                    bool NeedLoadParam = false;

                    // selected cell의 parameter를 다시 load 해야 한다는 flag를 set 한다.
                    GetProxy<IParamManagerProxy>()?.SetChangedDeviceParam(true);
                    GetProxy<IParamManagerProxy>()?.SetChangedSystemParam(true);

                    if (!bSkipCheck)
                    {
                        // 이전 선택된 셀들에 대해 일괄 정리를 수행 한다.
                        var updateDispCells = Cells.Where(cell => cell.StreamingMode == StreamingModeEnum.STREAMING_ON);
                        // STREAMING 모드인 cell들을 OFF로 변경 시키기 위함
                        foreach (var cell in updateDispCells)
                        {
                            try
                            {
                                var onstage = GetProxy<IStageSupervisorProxy>(cell.Index);
                                if (onstage != null && SelectedStage.Index != cell.Index)
                                {
                                    ChangeStreamingMode(cell, false);
                                }
                            }
                            catch (Exception err)
                            {
                                LoggerManager.Exception(err);
                            }
                        }
                    }

                    if (SelectedStage.StageMode == GPCellModeEnum.ONLINE)
                    {
                        if(!bSkipCheck)
                        {
                            if (SelectedStage.StreamingMode == StreamingModeEnum.STREAMING_ON)
                            {
                                // 선택한 cell을 streaming on mode로 변경한다.
                                ChangeStreamingMode(SelectedStage, true);
                            }
                            else
                            {
                                // 선택한 cell을 streaming off mode로 변경한다.
                                ChangeStreamingMode(SelectedStage, false);
                            }
                        }                        
                    }
                    else if (_SelectedStage.StageMode == GPCellModeEnum.MAINTENANCE)
                    {
                        NeedLoadMotionInfo = true;

                        if(!bSkipCheck)
                        {
                            // 선택한 cell을 streaming on mode로 변경한다.
                            ChangeStreamingMode(SelectedStage, true);
                        }                        

                        NeedLoadParam = true;
                    }
                    else //OFFLINE
                    {
                        if (!bSkipCheck)
                        {
                            // 선택한 cell을 streaming on mode로 변경한다.
                            ChangeStreamingMode(SelectedStage, false);
                        }
                    }

                    if (NeedLoadMotionInfo == true)
                    {
                        if (SelectedCellInfo == null)
                        {
                            IStageObject stageobj = GetStage();

                            if (stageobj != null)
                            {
                                SelectedCellInfo = stageobj.StageInfo as CellInfo;
                            }
                        }

                        if (SelectedCellInfo != null)
                        {
                            var motionAxisProxy = GetProxy<IMotionAxisProxy>();
                            if (motionAxisProxy != null)
                            {
                                this.MotionManager().StageAxes.ProbeAxisProviders = motionAxisProxy.Axes;

                                (this.MotionManager() as IMotionManagerServiceClient).MotionAxisProxy = motionAxisProxy;
                                (this.MotionManager() as IMotionManagerServiceClient)?.ThreadResume();
                            }
                            else
                            {
                                (this.MotionManager() as IMotionManagerServiceClient)?.ThreadPuase();
                            }
                        }
                    }
                    else
                    {
                        (this.MotionManager() as IMotionManagerServiceClient)?.ThreadPuase();
                    }

                    if (NeedLoadParam == true)
                    {
                        // TODO : For Speed up, use lastloadstageindex.

                        //await Task.Run(async() =>
                        //{

                        //GetWaferObject(_SelectedStage);
                        //GetProbeCardObject(_SelectedStage);
                        //GetMarkObject(_SelectedStage);

                        await DeviceReload(SelectedStage);

                        //});

                        LastLoadStageIndex = _SelectedStage.Index;
                        this.ProbingModule().ClearUnderDutDevs();
                        /////Stage WaferObject MapIndex 와의 동기를 맞추기 위해 
                        /////(Loader 측 MapView에서 Map 클릭시 Stage 에 Index를 전달하기 위해)
                        //if (this.StageSupervisor().WaferObject.ChangeMapIndexDelegate != null)
                        //    this.StageSupervisor().WaferObject.ChangeMapIndexDelegate -= UpdateMapIndex;

                        //this.StageSupervisor().WaferObject.ChangeMapIndexDelegate += UpdateMapIndex;
                    }

                    // TODO Position is right?
                    //var vm = this.ViewModelManager().GetViewModelFromGuid(_LoaderMainVMGuid);

                    //if (vm != null)
                    //{
                    //    (vm as ILoaderMainVM).SoakingType = "No Soak";
                    //    (vm as ILoaderMainVM).SoakRemainTIme = 0.ToString();

                    //}
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //minskim// 모든 동작(Task) 완료 후 Set하여 대기가 풀리도록 함
                AutoResetSelectStage.Set();
            }
        }
        public Task UpdateMapIndex(object newVal)
        {
            try
            {
                var mapindex = this.StageSupervisor().WaferObject?.GetCurrentMIndex();
                this.StageSupervisor().WaferIndexUpdated(mapindex.XIndex, mapindex.YIndex);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        //Pass_ToCheckSetStageMode : 해당 함수에서 Cell에 SetStageMode 가 실패한 패스하고 다음 스텝을 진행할 것인지 여부
        //이유:일반적으로 mode change시 cell에 mode 변경이 성공되어야만 loader도 mode를 바꾸는데 이와 다르게 loader와 cell간 연결하면 cell의 상태를 따라가게끔 되어 있기 때문에 해당 부분의 성공여부와 상관없이 loader는 cell의 mode를 따라가야 함)
        public async Task<bool> SetStageMode(GPCellModeEnum stagemode, StreamingModeEnum streamingmode, bool showdialogflag = true, int stageindex = -1, bool updateToStage = true, bool Pass_ToCheckSetStageMode = false)
        {
            bool retVal = true;
            try
            {
                //if (showdialogflag)
                //    await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                IStageObject stageObj = null;
                if (stageindex == -1)
                {
                    if (SelectedStage != null)
                    {
                        stageObj = SelectedStage;
                        stageindex = SelectedStage.StageInfo.Index;
                    }
                    else
                    {
                        await this.MetroDialogManager().ShowMessageDialog("Error Message", "Not exist selected cell information. Please select the cell you want.", EnumMessageStyle.Affirmative);                        
                        return false;
                    }
                }
                else
                {
                    stageObj = Cells.ToList<IStageObject>().Find(cell => cell.Index == stageindex);
                }

                if (stageObj == null)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Error Message", "Not exist selected cell information. Please select the cell you want.", EnumMessageStyle.Affirmative);                    
                    return false;
                }
                
                stageObj.IsStageModeChanged = true;

                if (updateToStage == true)
                {
                    if(EventCodeEnum.NONE != LoaderMaster.SetStageMode(stageindex, stagemode))
                    {
                        LoggerManager.Debug($"Failed to change to '{stagemode.ToString()}' (stageindex:{stageindex}),Pass_ToCheckSetStageMode:{Pass_ToCheckSetStageMode.ToString()}");
                        if(false == Pass_ToCheckSetStageMode)
                            return false;
                    }
                }

                var updateDispCells = Cells.Where(cell => cell.StreamingMode == StreamingModeEnum.STREAMING_ON);
                // STREAMING 모드인 cell들을 OFF로 변경 시키기 위함
                foreach (var cell in updateDispCells)
                {
                    try
                    {
                        var onstage = GetProxy<IStageSupervisorProxy>(cell.Index);
                        if (onstage != null && stageindex != cell.Index)
                        {
                            ChangeStreamingMode(cell, false);
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                if (stagemode == GPCellModeEnum.ONLINE)
                {
                    if (stageObj.StageMode != stagemode)
                    {
                        DisconnectProxyOnline(stageindex);
                    }

                    if (streamingmode == StreamingModeEnum.STREAMING_ON)
                    {
                        // 선택한 cell을 streaming on mode로 변경한다.
                        ChangeStreamingMode(stageObj, true);
                    }
                    else
                    {
                        // 선택한 cell을 streaming off mode로 변경한다.
                        ChangeStreamingMode(stageObj, false);
                    }
                }
                else if (stagemode == GPCellModeEnum.MAINTENANCE)
                {
                    if (stageObj.StageMode != stagemode)
                    {
                        // 선택한 cell이 maintenance 모드인 경우 update display flag를 true로 한다.
                        ChangeStreamingMode(stageObj, true);

                        this.ParamManager().GetDevElementList();
                        this.ParamManager().GetSysElementList();
                    }
                }
                else if (stagemode == GPCellModeEnum.OFFLINE)
                {
                    if (stageObj.StageMode != stagemode)
                    {
                        ChangeStreamingMode(stageObj, false);
                    }
                }
                else
                {
                    LoggerManager.Error($"Unknown");
                    retVal = false;
                }

                //stageObj.StageMode = stagemode;
                var stagemodeinfo = GetProxy<IStageSupervisorProxy>(stageindex)?.GetStageMode();
                if (stagemodeinfo != null)
                {
                    stageObj.StageMode = stagemodeinfo.Value.Item1;
                    LoggerManager.Debug($"SetStageMode() - Get Stage Mode : {stagemodeinfo.Value.Item1}");
                }

                await UpdateSelectedStage(stageObj, true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = false;
            }
            finally
            {
                //if (showdialogflag)
                //    await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }

            return retVal;
        }


        /// <summary>
        /// 선택해제된 cell이나 Online 모드일때 사용하지 않는 프록시들 연결 끊기.
        /// </summary>
        public void DisconnectProxyOnline(int stageindex)
        {
            try
            {
                var stage = Cells.FirstOrDefault(cell => cell.Index == stageindex);

                if (stage != null && stage.StageInfo != null && stage.StageInfo.ProxyManager != null)
                {
                    bool IsClose = false;

                    for (int i = stage.StageInfo.ProxyManager.ProxyCache.Count - 1; i >= 0; i--)
                    {
                        var item = stage.StageInfo.ProxyManager.ProxyCache.ElementAt(i);
                        var itemKey = item.Key;
                        var itemValue = item.Value;

                        if ((itemKey.Name.Equals(typeof(IRemoteMediumProxy).Name) == true) ||
                                (itemKey.Name.Equals(typeof(IStageSupervisorProxy).Name) == true) ||
                                (itemKey.Name.Equals(typeof(ITempControllerProxy).Name) == true) ||
                                (itemKey.Name.Equals(typeof(IFileManagerProxy).Name) == true) ||
                                (itemKey.Name.Equals(typeof(ISoakingModuleProxy).Name) == true) ||
                                (itemKey.Name.Equals(typeof(IParamManagerProxy).Name) == true)
                        )
                        {
                            IsClose = false;
                        }
                        else
                        {
                            IsClose = true;
                        }

                        if (IsClose == true)
                        {
                            stage.StageInfo.ProxyManager.Close(itemKey, itemValue.Proxy);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetTitleMessage(int Cellindex, string message, string foreground = "", string background = "")
        {
            try
            {
                int MaximumCount = 100;

                IStageObject stage = Cells[Cellindex - 1];

                //if (stage.StageInfo != null && stage.StageInfo.LotData != null)
                if (stage.StageInfo != null)
                {
                    string msg = message.Replace('_', ' ');

                    msg = msg.ToUpper();

                    if (stage.StageInfo.SetTitles != null)
                    {
                        if (stage.StageInfo.SetTitles.Count >= MaximumCount)
                        {
                            stage.StageInfo.SetTitles.Clear();
                        }

                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            stage.StageInfo.SetTitles.Add(msg);
                            stage.StageInfo.LastTitle = stage.StageInfo.SetTitles.Last();
                        });
                    }
                }
                else
                {
                    LoggerManager.Error("Data is wrong. StageInfo or LotData.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetDeviceName(int Cellindex, string deviceName)
        {
            try
            {
                IStageObject stage = Cells[Cellindex - 1];

                //if (stage.StageInfo != null && stage.StageInfo.LotData != null)
                if (stage.StageInfo != null)
                {


                    if (stage.StageInfo.DeviceName != null)
                    {

                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            stage.StageInfo.DeviceName = deviceName;
                            stage.StageInfo.LotData.DeviceName = deviceName;
                        });
                    }
                }
                else
                {
                    LoggerManager.Error("Data is wrong. StageInfo or LotData.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetDeviceLoadResult(int cellno, bool result)
        {
            try
            {
                try
                {
                    IStageObject stage = Cells[cellno - 1];

                    if (stage.StageInfo != null)
                    {


                        if (stage.StageInfo.DeviceName != null)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                stage.StageInfo.DeviceLoadResult = result;
                            });
                        }
                    }
                    else
                    {
                        LoggerManager.Error("Data is wrong. StageInfo or LotData.");
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
            }
        }

        public byte[] GetBytesFoupObjects()
        {
            byte[] retval = null;

            try
            {
                WrappedCasseteInfos tmp = new WrappedCasseteInfos();

                tmp.Foups = LoaderMaster.Loader.Foups;

                //foreach (var item in LoaderMaster.Loader.Foups)
                //{
                //    SimplifiedFoupObject tmp = new SimplifiedFoupObject();

                //    tmp.Index = item.Index;
                //    tmp.State = item.State;
                //    tmp.LotState = item.LotState;

                //    tmplist.Add(tmp);
                //}

                retval = SerializeManager.SerializeToByte(tmp, typeof(WrappedCasseteInfos));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum SetRecoveryMode(int cellIdx, bool isRecovery)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                var stage = Cells.Single(cellinfo => cellinfo.Index == cellIdx);
                stage.IsRecoveryMode = isRecovery;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void AddStageAlram(AlarmLogData alarm)
        {
            var stage = GetStage(alarm.OccurEquipment);
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (stage != null)
                {

                    stage.StageInfo.ErrorCodeAlarams.Add(alarm);
                    stage.StageInfo.AlarmMessageNotNotifiedCount++;
                }

            });
        }

        public void RemoveStageAlram(AlarmLogData alarm)
        {
            var stage = GetStage(alarm.OccurEquipment);
            if (stage != null)
            {

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    for (int i = stage.StageInfo.ErrorCodeAlarams.Count - 1; i >= 0; i--)
                    {
                        if (stage.StageInfo.ErrorCodeAlarams[i].ErrorOccurTime == alarm.ErrorOccurTime
                    && stage.StageInfo.ErrorCodeAlarams[i].ErrorCode == alarm.ErrorCode
                    && stage.StageInfo.ErrorCodeAlarams[i].ErrorMessage == alarm.ErrorMessage
                    && stage.StageInfo.ErrorCodeAlarams[i].OccurEquipment == alarm.OccurEquipment)
                        {
                            stage.StageInfo.ErrorCodeAlarams.RemoveAt(i);
                            stage.StageInfo.AlarmMessageNotNotifiedCount--;
                        }

                    }

                });
            }
        }

        public void UpdateStageAlarmCount()
        {
            try
            {

                foreach (var item in Cells)
                {
                    item.StageInfo.AlarmMessageNotNotifiedCount = item.StageInfo.ErrorCodeAlarams.Where(alram => alram.IsChecked == false).Count();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetCellModeChanging(int Cell_Idx)
        {
            try
            {
                var stage = GetStage(Cell_Idx);
                stage.StageInfo.IsConnectChecking = true;
                LoaderMaster.SetCellModeChanging(Cell_Idx);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void ResetCellModeChanging(int Cell_Idx)
        {
            try
            {
                var stage = GetStage(Cell_Idx);
                stage.StageInfo.IsConnectChecking = false;
                LoaderMaster.ResetCellModeChanging(Cell_Idx);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region Proxy<T>

        public T GetProxy<T>(int index = -1) where T : IProberProxy
        {
            T retval = default(T);

            try
            {
                var stageobj = GetStage(index);

                if (stageobj != null)
                {
                    if (stageobj.StageInfo.IsConnected == true)
                    {
                        if (CheckAccessibleProxy<T>(stageobj.StageMode) == true)
                        {
                            var pm = stageobj.StageInfo.ProxyManager;

                            // 등록이 되어 있지 않은 경우
                            if (pm.IsRegist<T>() == false)
                            {
                                InitProxy<T>(index);
                            }

                            retval = pm.GetProxy<T>();

                            if (retval != null)
                            {
                                if (retval is ICommunicationObject communicationObject)
                                {
                                    if (communicationObject.State != CommunicationState.Opened)
                                    {
                                        //communicationObject.Abort();

                                        InitProxy<T>(index);

                                        retval = pm.GetProxy<T>();
                                    }
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

            return retval;
        }

        /// CellMode에 따라 Proxy 를 접근할 수 있는지 없는지 확인하는 함수.
        private bool CheckAccessibleProxy<T>(GPCellModeEnum mode)
        {
            bool retVal = false;

            try
            {
                //private bool CheckGetProxy<T>(int index)
                //{
                //    bool retVal = false;
                //    try
                //    {
                //        var stage = GetStage(index);
                //        if (stage != null)
                //        {
                //            if (stage.StageInfo.IsConnected)
                //            {
                //                if (stage.StageMode == GPCellModeEnum.ONLINE)
                //                {
                //                    if ((typeof(T) == typeof(IRemoteMediumProxy)) | (typeof(T) == typeof(IStageSupervisorProxy)) | (typeof(T) == typeof(ITempControllerProxy))
                //                        | typeof(T) == typeof(IFileManagerProxy) | typeof(T) == typeof(ISoakingModuleProxy) | (typeof(T) == typeof(IParamManagerProxy)))
                //                    {
                //                        retVal = true;
                //                    }
                //                }
                //                else if (stage.StageMode == GPCellModeEnum.MAINTENANCE | stage.StageMode == GPCellModeEnum.OFFLINE)
                //                {
                //                    retVal = true;
                //                }
                //            }
                //        }
                //    }
                //    catch (Exception err)
                //    {
                //        LoggerManager.Exception(err);
                //    }
                //    return retVal;
                //}

                if (mode == GPCellModeEnum.ONLINE)
                {
                    if (typeof(T) == typeof(IRemoteMediumProxy) ||
                         typeof(T) == typeof(IStageSupervisorProxy) ||
                         typeof(T) == typeof(ITempControllerProxy) ||
                         typeof(T) == typeof(IFileManagerProxy) ||
                         typeof(T) == typeof(ISoakingModuleProxy) ||
                         typeof(T) == typeof(IParamManagerProxy) ||
                         typeof(T) == typeof(IPMIModuleProxy) ||
                         typeof(T) == typeof(IIOPortProxy)
                        )
                    {
                        retVal = true;
                    }
                }
                else if (mode == GPCellModeEnum.MAINTENANCE | mode == GPCellModeEnum.OFFLINE)
                {
                    retVal = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        private MethodInfo _initProxyMethod;
        public MethodInfo InitProxyMethod
        {
            get
            {
                if (_initProxyMethod == null)
                {
                    _initProxyMethod = typeof(LoaderCommunicationManager).GetMethod(nameof(LoaderCommunicationManager.InitProxy));
                }

                return _initProxyMethod;
            }
        }

        //minskim// library 이중 참조 문제를 회피 하기 위해 ProxyManager에서 동적 호출하기 위한 등록한 callback method
        MethodInfo _disconnectStageMethod;

        public MethodInfo DisconnectStageMethod
        {
            get
            {
                if (_disconnectStageMethod == null)
                {
                    _disconnectStageMethod = typeof(LoaderCommunicationManager).GetMethod(nameof(LoaderCommunicationManager.DisConnectStageProxy));
                }
                return _disconnectStageMethod;
            }
        }

        //minskim// proxy fault 발생시 호출되는 function
        public T DisConnectStageProxy<T>(int index = -1) where T : IProberProxy
        {
            T client = default(T);

            try
            {
                var stage = Cells.Single(cellinfo => cellinfo.Index == index);
                //disconnectStage 호출 전에 flag를 먼저 false로 변경해야함(다른 thread에서 해당 flag 참조 하여 getproxy를 지속 호출하기 때문임)
                stage.StageInfo.IsConnected = false;
                DisConnectStage(index);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return client;
        }

        public bool AbortStage(int index, bool delAllProxies = true)
        {
            bool retVal = false;
            try
            {
                var stage = Cells.Single(cellinfo => cellinfo.Index == index);
                if (stage != null)
                {
                    CellInfo cellinfo = stage.StageInfo as CellInfo;

                    //display callback channel abort
                    var hostclient = GetDispHostService().GetDispHostClient(index);
                    if (hostclient != null)
                    {
                        (hostclient as ICommunicationObject).Abort();
                        //(GetDispHostService() as ImageDispHost).Clients.Remove(index);
                        LoggerManager.Debug($"Image Host Callback Channel Abort. cell index = {index}");
                    }
                    //dialog callback channel abort
                    var eventclient = GetDelegateEventService().GetDialogHostClient(index);
                    if (eventclient != null)
                    {
                        (eventclient as ICommunicationObject).Abort();
                        //(GetDelegateEventService() as DelegateEventHost).Clients.Remove(index);
                        LoggerManager.Debug($"Dialog Host Callback Channel Abort. cell index = {index}");
                    }

                    //datatgateway callback channel abort
                    var datatgatewayclient = GetDataGatewayService().GetDataGatewayHostClient(index);
                    if (datatgatewayclient != null)
                    {
                        (datatgatewayclient as ICommunicationObject).Abort();
                        //(GetDelegateEventService() as DelegateEventHost).Clients.Remove(index);
                        LoggerManager.Debug($"DataGateway Host Callback Channel Abort. cell index = {index}");

                    }
                    cellinfo.AbortProxies();

                    if (delAllProxies)
                    {
                        var envClient = this.EnvControlManager().GetEnvControlClient(index);
                        if (envClient != null)
                        {
                            (eventclient as ICommunicationObject).Abort();

                            LoggerManager.Debug($"ENV Host Callback Channel Abort. cell index = {index}");
                        }

                        // stage host에 연결한 loader proxy channel 들 abort
                        string chuckkey = $"CHUCK{index}";
                        ILoaderServiceCallback loaderclient = null;
                        LoaderMaster.ClientList.TryGetValue(chuckkey, out loaderclient);
                        if (loaderclient != null)
                        {
                            (loaderclient as ICommunicationObject).Abort();
                            LoggerManager.Debug($"GPLService Host Callback Channel Abort. cell index = {index}");
                        }
                    }
                    retVal = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public bool IsAliveStageSupervisor(int nIndex)
        {
            bool bAlive = false;
            try
            {
                LoggerManager.Debug($"IsAliveStageSupervisor(Cell={nIndex}) Check Start");
                var stageinfo = GetStageCommParam(nIndex);
                StageSupervisorProxy stagesupervisor = new StageSupervisorProxy(stageinfo.Port + 100, stageinfo.IP, true);
                stagesupervisor.CheckService();
                if (stagesupervisor.State == CommunicationState.Opened)
                {
                    if (stagesupervisor.GetStageInitState() != CellInitModeEnum.BeforeInit)
                    {
                        bAlive = true;
                    }
                    else
                    {
                        LoggerManager.Debug($"IsAliveStageSupervisor(Cell={nIndex}) System initialization in progress.");
                    }
                    stagesupervisor.Close();
                }
                else
                {
                    stagesupervisor.Abort();
                }
            }
            catch(Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                LoggerManager.Debug($"IsAliveStageSupervisor(Cell={nIndex}) not yet executed.");
            }                
            finally
            {
                LoggerManager.Debug($"IsAliveStageSupervisor(Cell={nIndex}) Check End");
            }


            return bAlive;
        }

        public T InitProxy<T>(int index = -1) where T : IProberProxy
        {
            T client = default(T);
            //bool retval = false;

            try
            {
                var stageinfo = GetStageCommParam(index);

                if (stageinfo != null)
                {
                    var stageobj = GetStage(index);

                    if (stageobj.StageInfo.ProxyManager == null)
                    {
                        stageobj.StageInfo.ProxyManager = new ProxyManager(InitProxyMethod, DisconnectStageMethod, stageobj.Index, this);
                    }

                    var proxyset = CreateProxy<T>(stageinfo.Port, stageinfo.IP, stageobj.StageInfo.ProxyManager);

                    if (proxyset != null)
                    {
                        stageobj.StageInfo.ProxyManager.RegistProxy<T>(proxyset);

                        (proxyset.Proxy as IProberProxy).InitService();

                        client = (T)proxyset.Proxy;
                        //retval = true;
                    }
                    else
                    {
                        LoggerManager.Error($"[LoaderCommunicationManager], InitProxy() : proxy is null.");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return client;
            //return retval;
        }

        private ProxySet CreateProxy<T>(int StagePort, string StageIP, ProxyManager proxymanager)
        {
            object retval = null;
            ProxySet ret = null;
            string serviceName = string.Empty;
            string hostname = string.Empty;

            int port = 0;
            int portoffset = 0;

            try
            {
                serviceName = typeof(T).Name;

                if (serviceName[0] == char.Parse("I"))
                {
                    serviceName = serviceName.Remove(0, 1);

                    LoggerManager.Debug($"[LoaderCommunicationManager], CreateProxy(), ServiceName = {serviceName}");
                }

                bool Existoffset = PortOffsets.GetPortOffset<T>(out portoffset);

                if (Existoffset == true)
                {
                    port = StagePort + portoffset;
                }
                else
                {
                    throw new ArgumentException();
                }

                switch (serviceName)
                {
                    case nameof(StageSupervisorProxy):
                    {
                        retval = new StageSupervisorProxy(port, StageIP);
                        hostname = nameof(IStageSupervisor);
                        break;
                    }
                    case nameof(StageMoveProxy):
                    {
                        retval = new StageMoveProxy(port, StageIP);
                        hostname = nameof(IStageMove);
                        break;
                    }
                    case nameof(IOPortProxy):
                    {
                        retval = new IOPortProxy(StageIP, port, GetInstanceContext<T>());
                        hostname = nameof(IIOMappingsParameter);
                        break;
                    }
                    case nameof(MotionAxisProxy):
                    {
                        retval = new MotionAxisProxy(StageIP, port, GetInstanceContext<T>());
                        hostname = nameof(IMotionManager);
                        break;
                    }
                    case nameof(RemoteMediumProxy):
                    {
                        retval = new RemoteMediumProxy(port, GetInstanceContext<T>(), StageIP);
                        hostname = nameof(ILoaderRemoteMediator);
                        break;
                    }
                    case nameof(TempControllerProxy):
                    {
                        retval = new TempControllerProxy(StageIP, port);
                        hostname = nameof(ITempController);
                        break;
                    }
                    case nameof(PolishWaferModuleProxy):
                    {
                        retval = new PolishWaferModuleProxy(StageIP, port);
                        hostname = nameof(IPolishWaferModule);
                        break;
                    }
                    case nameof(PinAlignerProxy):
                    {
                        retval = new PinAlignerProxy(StageIP, port);
                        hostname = nameof(IPinAligner);
                        break;
                    }
                    case nameof(FileManagerProxy):
                    {
                        retval = new FileManagerProxy(StageIP, port);
                        hostname = nameof(IFileManager);
                        break;
                    }
                    case nameof(LotOPModuleProxy):
                    {
                        retval = new LotOPModuleProxy(StageIP, port);
                        hostname = nameof(ILotOPModule);
                        break;
                    }
                    case nameof(SoakingModuleProxy):
                    {
                        retval = new SoakingModuleProxy(StageIP, port);
                        hostname = nameof(ISoakingModule);
                        break;
                    }
                    case nameof(PMIModuleProxy):
                    {
                        retval = new PMIModuleProxy(StageIP, port);
                        hostname = nameof(IPMIModule);
                        break;
                    }
                    case nameof(ParamManagerProxy):
                    {
                        retval = new ParamManagerProxy(StageIP, port);
                        hostname = nameof(IParamManager);
                        break;
                    }
                    case nameof(CoordinateManagerProxy):
                    {
                        retval = new CoordinateManagerProxy(StageIP, port);
                        hostname = nameof(ICoordinateManager);
                        break;
                    }
                    case nameof(WaferAlignerProxy):
                    {
                        retval = new WaferAlignerProxy(StageIP, port);
                        hostname = nameof(IWaferAligner);
                        break;
                    }
                    case nameof(RetestModuleProxy):
                    {
                        retval = new RetestModuleProxy(StageIP, port);
                        hostname = nameof(IRetestModule);
                        break;
                    }

                    default:
                        break;
                }

                if (retval != null)
                {
                    // Event

                    ICommunicationObject comobj = retval as ICommunicationObject;

                    proxymanager.RegistEvent(comobj);

                    //dynamic t = retval;
                    //t.Endpoint.EndpointBehaviors.Add(new MyBehavior());

                    ret = new ProxySet(retval, hostname);
                }
                else
                {
                    LoggerManager.Error($"[LoaderCommunicationManager], CreateProxy() : service name = {serviceName}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
            //return retval;
        }

        private InstanceContext GetInstanceContext<T>()
        {
            InstanceContext retval = null;

            var serviceName = typeof(T).Name;

            if (serviceName[0] == char.Parse("I"))
            {
                serviceName = serviceName.Remove(0, 1);
            }

            switch (serviceName)
            {
                case nameof(MotionAxisProxy):
                    {
                        retval = motionManagerCallbackService.GetInstanceContext();
                        break;
                    }
                case nameof(IOPortProxy):
                    {
                        retval = iOMappingsCallbackService.GetInstanceContext();
                        break;
                    }
                case nameof(RemoteMediumProxy):
                    {
                        retval = viewModelDataManagerCallbackService.GetInstanceContext();
                        break;
                    }

                default:
                    break;
            }

            return retval;
        }

        #endregion

        #region // DriveInfo
        public void SetDiskInfo(int lunchernum, string pc_name, string drivename, string usagespace, string availablespace, string totalspace, string percent) // Set Drive Info
        {
            try
            {
                var Launcher = MultiLaunchers.Where(item => item.Index == lunchernum-1).FirstOrDefault();
                if (Launcher != null)
                {
                    var get_diskinfo = Launcher.LauncherDiskObjectCollection.Where(item => item.DriveName == drivename).FirstOrDefault();
                    if (get_diskinfo != null)
                    {
                        get_diskinfo.UsageSpace = usagespace;
                        get_diskinfo.AvailableSpace = availablespace;
                        get_diskinfo.TotalSpace = totalspace;
                        get_diskinfo.Percent = percent;
                    }
                    else
                    {
                        LauncherDiskObject drive = new LauncherDiskObject(drivename, usagespace, availablespace, totalspace, percent);
                        Launcher.LauncherDiskObjectCollection.Add(drive);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.Debug($"[{ this.GetType().Name}]SetDiskInfo() Exception");
            }

        }


        #endregion
    }
}
