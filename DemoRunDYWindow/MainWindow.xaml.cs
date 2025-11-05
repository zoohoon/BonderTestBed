namespace DemoRunDYWindow
{
    using Autofac;
    using LoaderMaster;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Collections.ObjectModel;
    using ProberInterfaces;
    using ProberInterfaces.Foup;
    using ProberInterfaces.Loader;
    using System.IO;
    using LogModule;
    using LoaderBase.Communication;
    using LoaderCommunicationModule;
    using RelayCommandBase;
    using System.Collections;
    using System.Globalization;
    using SecsGemServiceInterface;
    using LoaderBase;

    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Property

        public Autofac.IContainer Container;
        private LoaderSupervisor Master { get; set; }
        private LoaderCommunicationManager LoaderCommManager { get; set; }
        private IFoupOpModule FoupOPModule => Container.Resolve<IFoupOpModule>();
        private ObservableCollection<LotInfo> _FoupLotInfos ;
        public ObservableCollection<LotInfo> FoupLotInfos
        {
            get { return _FoupLotInfos; }
            set
            {
                if (value != _FoupLotInfos)
                {
                    _FoupLotInfos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private LotInfo _SelectedFoup;
        public LotInfo SelectedFoup
        {
            get { return _SelectedFoup; }
            set
            {
                if (value != _SelectedFoup)
                {
                    _SelectedFoup = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ObservableCollection<CellInfo> _CellInfos;
        public ObservableCollection<CellInfo> CellInfos
        {
            get { return _CellInfos; }
            set
            {
                if (value != _CellInfos)
                {
                    _CellInfos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<CellInfo> _SelectedCells;
        public List<CellInfo> SelectedCells
        {
            get { return _SelectedCells; }
            set
            {
                if (value != _SelectedCells)
                {
                    _SelectedCells = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ObservableCollection<string> _DeviceNames;
        public ObservableCollection<string> DeviceNames
        {
            get { return _DeviceNames; }
            set
            {
                if (value != _DeviceNames)
                {
                    _DeviceNames = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _SelectedDevice;
        public string SelectedDevice
        {
            get { return _SelectedDevice; }
            set
            {
                if (value != _SelectedDevice)
                {
                    _SelectedDevice = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<int> _FoupIndexs;
        public ObservableCollection<int> FoupIndexs
        {
            get { return _FoupIndexs; }
            set
            {
                if (value != _FoupIndexs)
                {
                    _FoupIndexs = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _WaitDoneSlotNum;
        public int WaitDoneSlotNum
        {
            get { return _WaitDoneSlotNum; }
            set
            {
                if (value != _WaitDoneSlotNum)
                {
                    _WaitDoneSlotNum = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<int> _SlotNums;
        public ObservableCollection<int> SlotNums
        {
            get { return _SlotNums; }
            set
            {
                if (value != _SlotNums)
                {
                    _SlotNums = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsWaitWfConfirm;
        public bool IsWaitWfConfirm
        {
            get { return _IsWaitWfConfirm; }
            set
            {
                if (value != _IsWaitWfConfirm)
                {
                    _IsWaitWfConfirm = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsEmulGemMode;
        public bool IsEmulGemMode
        {
            get { return _IsEmulGemMode; }
            set
            {
                if (value != _IsEmulGemMode)
                {
                    _IsEmulGemMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsFoupShiftMode;
        public bool IsFoupShiftMode
        {
            get { return _IsFoupShiftMode; }
            set
            {
                if (value != _IsFoupShiftMode)
                {
                    _IsFoupShiftMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }
        public MainWindow(LoaderSupervisor master) : this()
        {
            Master = master;
            DataContext = this;
            InitViewModel();
        }

        public void InitViewModel()
        {
            try 
            {
                Container = Master.cont;
                LoaderCommManager = (LoaderCommunicationManager)Container.Resolve<ILoaderCommunicationManager>();
                int foupcount = FoupOPModule.FoupControllers.Count();
                int cellcount = LoaderSupervisor.StageCount;

                IsEmulGemMode = this.GEMModule().IsExternalLotMode;
                if (this.Master.LotSysParam.FoupShiftMode.Value == FoupShiftModeEnum.SHIFT)
                {
                    IsFoupShiftMode =  true;
                }
                else
                {
                    IsFoupShiftMode = false;
                }
                IsWaitWfConfirm = Master.GetIsWaitForWaferIdConfirm();

               FoupLotInfos = new ObservableCollection<LotInfo>();
                FoupIndexs = new ObservableCollection<int>();

                

                for (int index = 0; index < foupcount; index++)
                {
                    var temp = new ObservableCollection<string>();
                    for (int i = 1; i <= 25; i++)
                    {
                        temp.Add("CE2GT171SEB" + string.Format("{0:00}", i + 25 * index));
                    }

                    FoupLotInfos.Add(new LotInfo(index + 1, cellcount, temp, Master));
                    //FoupLotInfos[index].Master = Master;
                    FoupIndexs.Add(index + 1);
                }

                SelectedFoup = FoupLotInfos.First();

                SlotNums = new ObservableCollection<int>();
                for (int index = 1; index <= 25; index++)
                {
                    SlotNums.Add(index);
                }

                CellInfos = new ObservableCollection<CellInfo>();
                for (int index = 0; index < cellcount; index++)
                {
                    CellInfos.Add(new CellInfo(index + 1));
                }

                InitDevices();
                InitCellEnable();
            }
            catch (Exception)
            {
            }
        }

        private void InitDevices()
        {
            try
            {
                IDeviceManager devicemanager = Container.Resolve<IDeviceManager>();

                if (devicemanager != null)
                {
                    var devpath = devicemanager.GetLoaderDevicePath();
                    if (Directory.Exists(devpath))
                    {
                        string[] dirs = Directory.GetDirectories(devpath);
                        DeviceNames = new ObservableCollection<string>();
                        foreach (var dir in dirs)
                        {
                            DeviceNames.Add(new DirectoryInfo(dir).Name);
                        }
                    }
                    else
                        Directory.CreateDirectory(devpath);
                }

                var cells = LoaderCommManager.Cells;
                if(cells != null)
                {
                    foreach (var cell in cells)
                    {
                        //Cell 이 연결이 되어있으면
                        if(cell.StageInfo.IsConnected)
                        {
                            var cellInfo = CellInfos.SingleOrDefault(info => info.CellIndex == cell.StageInfo.Index);
                            if(cellInfo != null)
                            {
                                cellInfo.CurDeviceName = cell.StageInfo.DeviceName;
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

        private void InitCellEnable()
        {
            try
            {
                var cells = LoaderCommManager.Cells;
                if (cells != null)
                {
                    foreach (var cell in cells)
                    {
                        //Cell 이 연결이 되어있으면
                        if (cell.StageInfo.IsConnected)
                        {
                            foreach (var foup in FoupLotInfos)
                            {
                                foreach (var slot in foup.SlotInfos)
                                {
                                    var cellinfo = slot.SlotCellInfos.SingleOrDefault(info => info.CellIndex == cell.StageInfo.Index);
                                    if (cellinfo != null)
                                    {
                                        cellinfo.EnableCell = true;
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
        }

        #region Command 


        private RelayCommand _SetDeviceCommand;
        public ICommand SetDeviceCommand
        {
            get
            {
                if (null == _SetDeviceCommand) _SetDeviceCommand = new RelayCommand(SetDeviceCommandFunc);
                return _SetDeviceCommand;
            }
        }

        public void SetDeviceCommandFunc()
        {
            try
            {
                foreach (var cell in SelectedCells)
                {
                    cell.SetDeviceName = SelectedDevice;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private RelayCommand _ClearDeviceCommand;
        public ICommand ClearDeviceCommand
        {
            get
            {
                if (null == _ClearDeviceCommand) _ClearDeviceCommand = new RelayCommand(ClearDeviceCommandFunc);
                return _ClearDeviceCommand;
            }
        }

        public void ClearDeviceCommandFunc()
        {
            try
            {
                foreach (var cell in CellInfos)
                {
                    cell.SetDeviceName = "";
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private AsyncCommand<int> _LotStartCommand;
        public ICommand LotStartCommand 
        {
            get
            {
                if (null == _LotStartCommand) _LotStartCommand = new AsyncCommand<int>(LotStartCommandFunc);
                return _LotStartCommand;
            }
        }
        private IFoupOpModule Foup => Container.Resolve<IFoupOpModule>();
        private bool isWaitDownloadRecipe = true;
        public async Task LotStartCommandFunc(int foupnumber)
        {
            try
            {
                if (Master.ActiveLotInfos[foupnumber - 1].State != LotStateEnum.Running)
                {
                    foreach(var cell in CellInfos)
                    {
                        if(cell.IsForecedDone)
                        {
                            var client = Master.GetClient(cell.CellIndex);
                            if (client != null)
                            {
                                client.SetForcedDone(EnumModuleForcedState.ForcedDone);
                            }
                        }
                    }
                    isWaitDownloadRecipe = true;
                    var modules = Master.Loader.ModuleManager;
                    //this.LoaderModule.GetLoaderCommands().CassetteLoad(Cassette);



                    List<int> slotList = new List<int>();
                    var foupInfo = FoupLotInfos.SingleOrDefault(foupinfo => foupinfo.FoupIndex == foupnumber);

                    Task task = null;
                    var Cassette = Master.Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, foupnumber);
                    var CassetteModule = Master.Loader.GetLoaderInfo().StateMap.CassetteModules[foupnumber - 1];
                    if (Master.GEMModule().GemSysParam.ReceiveMessageType.Equals("SemicsGemReceiverSEKX"))
                    {
                        //Master.SetFoupShiftMode(1);
                        //this.GEMModule().GetPIVContainer().FoupShiftMode.Value = 1;//원래는 host에서 OnGEMReqChangeECV로 받음.                                                                                                          
                    }
                    else
                    {
                        Master.ActiveLotInfos[Cassette.ID.Index - 1].FoupLoad();
                        //  Foup.FoupControllers[Cassette.ID.Index - 1].Execute(new FoupLoadCommand());
                        Master.Loader.ScanCount = 25;

                        Cassette.SetNoReadScanState();
                    }


                    string carrierId = "PCF5155";
                    #region Active Lot


                    ActiveProcessActReqData activeData = new ActiveProcessActReqData();
                    activeData.UseStageNumbers_str = "000000000000";
                    activeData.FoupNumber = foupnumber;
                    activeData.LotID = foupInfo.LotID;
                    activeData.UseStageNumbers = new List<int>();
                    foreach (var stage in CellInfos)
                    {
                        var selectecell = SelectedCells.Find(cell => cell.CellIndex == stage.CellIndex);
                        if(selectecell != null)
                        {
                            activeData.UseStageNumbers.Add(stage.CellIndex);
                            activeData.UseStageNumbers_str = (string)activeData.UseStageNumbers_str.Insert(stage.CellIndex - 1, "1");
                            activeData.UseStageNumbers_str = (string)activeData.UseStageNumbers_str.Remove(stage.CellIndex, 1);
                        }
                    }

                    if (Master.GEMModule().GemSysParam.ReceiveMessageType.Equals("SemicsGemReceiverSEKX") == false)
                    {                        
                        Master.ActiveProcess(activeData);

                        DockFoupActReqData dockFoupActReqData = new DockFoupActReqData();
                        dockFoupActReqData.ActionType = EnumRemoteCommand.DOCK_FOUP;
                        dockFoupActReqData.FoupNumber = foupnumber;
                        task = new Task(() =>
                        {
                            this.GEMModule().GemCommManager.OnRemoteCommandAction(dockFoupActReqData);
                        });
                        task.Start();
                        await task;
                    }
                    else
                    {                      
                        
                        //if(Cassette.CarrierId.Value == null)
                        //{
                            CassetteModule.FoupID = carrierId;
                            this.GEMModule().GetPIVContainer().SetFoupInfo(foupnumber, carrierid: carrierId);
                            this.GEMModule().GetPIVContainer().UpdateFoupInfo(foupnumber);
                        //}

                        ProceedWithCarrierReqData proceedWithCarrierReqData = new ProceedWithCarrierReqData();
                        proceedWithCarrierReqData.DataID = 0;
                        proceedWithCarrierReqData.CattrData = new string[1];
                        proceedWithCarrierReqData.CattrData[0] = activeData.LotID;
                        proceedWithCarrierReqData.PTN = activeData.FoupNumber;
                        proceedWithCarrierReqData.LotID = activeData.LotID;
                        proceedWithCarrierReqData.CarrierID = CassetteModule.FoupID;
                        proceedWithCarrierReqData.ActionType = EnumCarrierAction.PROCEEDWITHCARRIER;
                        
                        

                        ProceedWithSlotReqData proceedWithSlot = new ProceedWithSlotReqData();
                        proceedWithSlot.DataID = 1;
                        proceedWithSlot.PTN = activeData.FoupNumber;
                        proceedWithSlot.CarrierID = CassetteModule.FoupID;
                        proceedWithSlot.ActionType = EnumCarrierAction.PROCEEDWITHSLOT;
                        proceedWithSlot.SlotMap = new string[25];
                        proceedWithSlot.OcrMap = new string[25];

                       
                        for (int index = 0; index < 25; index++)
                        {
                            proceedWithSlot.OcrMap[index] = foupInfo.SlotInfos[25 - 1 - index].Predefined_WaferId;
                        }


                        task = new Task(() =>
                        {
                            this.GEMModule().GemCommManager.OnCarrierActMsgRecive(proceedWithCarrierReqData);
                            this.GEMModule().GemCommManager.OnCarrierActMsgRecive(proceedWithSlot);
                        });
                        task.Start();
                        await task;
                    }                    
                    #endregion

                    #region DownLoad Receipte
                    DownloadStageRecipeActReqData DownloadReqData = new DownloadStageRecipeActReqData();
                    Master.GEMModule().IsExternalLotMode = true;
                    IsEmulGemMode = this.GEMModule().IsExternalLotMode;

                    if (Master.GEMModule().GemSysParam.ReceiveMessageType.Equals("SemicsGemReceiverSEKX"))
                    {
                        DownloadReqData.ActionType = EnumRemoteCommand.DLRECIPE;
                    }
                    else
                    {
                        DownloadReqData.ActionType = EnumRemoteCommand.DOWNLOAD_STAGE_RECIPE;
                    }
                    
                    DownloadReqData.LotID = activeData.LotID;
                    DownloadReqData.FoupNumber = activeData.FoupNumber;
                    foreach (var cell in activeData.UseStageNumbers)
                    {
                        var dcell = CellInfos.SingleOrDefault(info => info.CellIndex == cell);
                        if(dcell != null)
                        {
                            if(dcell.SetDeviceName != null)
                            {
                                if(dcell.SetDeviceName != "")
                                {
                                    DownloadReqData.RecipeDic.Add(dcell.CellIndex, dcell.SetDeviceName);
                                }
                            }
                        }
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (DownloadReqData.RecipeDic.Count > 0)
                        {
                            DownloadReqData.UseFTP = false;
                            this.GEMModule().ResetStageDownloadRecipe();
                            var ret = Master.DeviceManager().SetRecipeToStage(DownloadReqData);// #Hynix_Merge: Hynix 코드에는 false 있고 RC_Integrated에는 없음.
                            Master.SetRecipeToDevice(DownloadReqData);
                        }
                    });

                    task = new Task(() =>
                    {
                        if (DownloadReqData.RecipeDic.Count > 0)
                        {
                            while (isWaitDownloadRecipe == true)
                            {
                                if (this.GEMModule().GetComplateDownloadRecipe(activeData.UseStageNumbers))
                                {
                                    isWaitDownloadRecipe = false;
                                }
                            }
                        }
                    });
                    task.Start();
                    await task;

                    if (DownloadReqData.RecipeDic.Count != 0 & activeData.UseStageNumbers.Count != 0)
                    {
                        string recipe = "";
                        DownloadReqData.RecipeDic.TryGetValue(activeData.UseStageNumbers[0], out recipe);
                    }
                    #endregion

                    if (Master.GEMModule().GemSysParam.ReceiveMessageType.Equals("SemicsGemReceiverSEKX"))
                    {
                       

                        int[] tempSlotMap = new int[25];
                        foreach (var slotinfo in foupInfo.SlotInfos)
                        {
                            SlotCellInfo scinfo = new SlotCellInfo(slotinfo.SlotIndex);                            
                            scinfo.CellIndexs = new List<int>();

                            foreach (var scif in slotinfo.SlotCellInfos)
                            {
                                if (scif.EnableSlot == true)
                                {
                                    tempSlotMap[scinfo.SlotIndex - 1] = 1;
                                }
                            }

                        }

                        SelectStageSlotActReqData StageSlotReqData = new SelectStageSlotActReqData();
                        StageSlotReqData.PTN = activeData.FoupNumber;
                        StageSlotReqData.CarrierId = CassetteModule.FoupID;
                        StageSlotReqData.CellMap = activeData.UseStageNumbers_str;
                        StageSlotReqData.SlotMap = string.Join("", tempSlotMap); 
                        StageSlotReqData.ActionType = EnumRemoteCommand.STAGE_SLOT;
                        this.GEMModule().GemCommManager.OnRemoteCommandAction(StageSlotReqData);
                    }
                    else
                    {                        

                        SelectSlotsStagesActReqData selectSlotsActReqData = new SelectSlotsStagesActReqData();
                        selectSlotsActReqData.LotID = foupInfo.LotID;
                    selectSlotsActReqData.ActionType = EnumRemoteCommand.SELECT_SLOTS_STAGE;
                        selectSlotsActReqData.FoupNumber = foupnumber;

                        foreach (var slotinfo in foupInfo.SlotInfos)
                        {
                        SlotCellInfo scinfo = new SlotCellInfo(slotinfo.SlotIndex);
                            scinfo.SlotIndex = slotinfo.SlotIndex;
                            scinfo.CellIndexs = new List<int>();

                            foreach (var scif in slotinfo.SlotCellInfos)
                            {
                            if(scif.EnableSlot == true)
                                {
                                    scinfo.CellIndexs.Add(scif.CellIndex);
                                }
                            }
                            selectSlotsActReqData.SlotStageNumbers.Add(scinfo);
                        }

                        selectSlotsActReqData.ActionType = EnumRemoteCommand.SELECT_SLOTS_STAGES;
                        task = new Task(() =>
                        {
                            this.GEMModule().GemCommManager.OnRemoteCommandAction(selectSlotsActReqData);
                        });
                        task.Start();
                        await task;
                        
                    }
                        
                   
                    
                   

                    StartLotActReqData startLotActReqData = new StartLotActReqData();
                    startLotActReqData.LotID = foupInfo.LotID;
                    startLotActReqData.FoupNumber = foupnumber;
                    if (Master.GEMModule().GemSysParam.ReceiveMessageType.Equals("SemicsGemReceiverSEKX"))
                    {
                        startLotActReqData.ActionType = EnumRemoteCommand.PSTART;
                    }
                    else
                    {
                        startLotActReqData.ActionType = EnumRemoteCommand.START_LOT;// lotid 안넣어주는것만 다르지만...
                    }
                    task = new Task(() =>
                    {
                        //this.GEMModule().GemCommManager.OnRemoteCommandAction(dockFoupActReqData);
                        //this.GEMModule().GemCommManager.OnRemoteCommandAction(selectSlotsActReqData);
                        this.GEMModule().GemCommManager.OnRemoteCommandAction(startLotActReqData);
                    });
                    task.Start();
                    await task;

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<int> _CellResumeCommand;
        public ICommand CellResumeCommand
        {
            get
            {
                if (null == _CellResumeCommand) _CellResumeCommand = new AsyncCommand<int>(CellResumeCommandFunc);
                return _CellResumeCommand;
            }
        }
        
        
        public Task CellResumeCommandFunc(int cellindex)
        {
            try
            {
                var cell = CellInfos.FirstOrDefault(c => c.CellIndex == cellindex);
                if(cell != null)
                {
                    var client = Master.GetClient(cell.CellIndex);
                    if (client != null)
                    {
                        client?.CellProbingResume();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }



        private AsyncCommand<bool> _SetEmulGemModeCommand;
        public ICommand SetEmulGemModeCommand
        {
            get
            {
                if (null == _SetEmulGemModeCommand) _SetEmulGemModeCommand = new AsyncCommand<bool>(SetEmulGemModeCommandFunc);
                return _SetEmulGemModeCommand;
            }
        }

        public Task SetEmulGemModeCommandFunc(bool ischecked)
        {
            try
            {
                this.GEMModule().IsExternalLotMode = ischecked;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);                
            }
            return Task.CompletedTask;
        }

        private AsyncCommand<bool> _SetFoupShiftModeCommand;
        public ICommand SetFoupShiftModeCommand
        {
            get
            {
                if (null == _SetFoupShiftModeCommand) _SetFoupShiftModeCommand = new AsyncCommand<bool>(SetFoupShiftModeCommandFunc);
                return _SetFoupShiftModeCommand;
            }
        }

        public Task SetFoupShiftModeCommandFunc(bool ischecked)
        {
            try
            {

                if (IsFoupShiftMode)
                {
                    Master.SetFoupShiftMode(1);
                    this.GEMModule().GetPIVContainer().FoupShiftMode.Value = 1;//원래는 host에서 OnGEMReqChangeECV로 받음.                                                                                                          

                }
                else
                {
                    Master.SetFoupShiftMode(0);
                    this.GEMModule().GetPIVContainer().FoupShiftMode.Value = 0;//원래는 host에서 OnGEMReqChangeECV로 받음.                                                                                                          
                }
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }


        private AsyncCommand<int> _LotCancelCommand;
        public ICommand LotCancelCommand
        {
            get
            {
                if (null == _LotCancelCommand) _LotCancelCommand = new AsyncCommand<int>(LotCancelCommandFunc);
                return _LotCancelCommand;
            }
        }

        public Task LotCancelCommandFunc(int foupnumber)
        {
            try
            {
                CarrierActReqData reqData = new CarrierActReqData();
                reqData.PTN = foupnumber;
                reqData.ActionType = EnumCarrierAction.CANCELCARRIER;

                this.GEMModule().GemCommManager.OnCarrierActMsgRecive(reqData);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }
        private AsyncCommand<int> _DockCommand;
        public ICommand DockCommand
        {
            get
            {
                if (null == _DockCommand) _DockCommand = new AsyncCommand<int>(DockCommandFunc);
                return _DockCommand;
            }
        }

        public Task DockCommandFunc(int foupnumber)
        {
            try
            {
                DockFoupActReqData actReqData = new DockFoupActReqData();
                actReqData.FoupNumber = foupnumber;
                actReqData.ActionType = EnumRemoteCommand.DOCK_FOUP;

                this.GEMModule().GemCommManager.OnRemoteCommandAction(actReqData);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }
        private AsyncCommand<int> _UndockCommand;
        public ICommand UndockCommand
        {
            get
            {
                if (null == _UndockCommand) _UndockCommand = new AsyncCommand<int>(UndockCommandFunc);
                return _UndockCommand;
            }
        }

        public Task UndockCommandFunc(int foupnumber)
        {
            try
            {
                if (Master.GEMModule().GemSysParam.ReceiveMessageType.Equals("SemicsGemReceiverSEKX"))// TODO: 일단은...
                {
                    CarrierIdentityData actReqData = new CarrierIdentityData();
                    actReqData.PTN = foupnumber;
                    actReqData.ActionType = EnumRemoteCommand.UNDOCK;
                    this.GEMModule().GemCommManager.OnRemoteCommandAction(actReqData);
                }
                else
                {
                    DockFoupActReqData actReqData = new DockFoupActReqData();
                    actReqData.FoupNumber = foupnumber;
                    if (Master.GEMModule().GemSysParam.ReceiveMessageType.Equals("SemicsGemReceiverSEKR"))// TODO: 일단은...
                    {
                        actReqData.ActionType = EnumRemoteCommand.UNDOCK_FOUP;
                    }
                    else
                    {
                        actReqData.ActionType = EnumRemoteCommand.UNDOCK;
                    }

                    this.GEMModule().GemCommManager.OnRemoteCommandAction(actReqData);
                }                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        private AsyncCommand<int> _CellEndTestCommand;
        public ICommand CellEndTestCommand
        {
            get
            {
                if (null == _CellEndTestCommand) _CellEndTestCommand = new AsyncCommand<int>(CellEndTestCommandFunc);
                return _CellEndTestCommand;
            }
        }

        public Task CellEndTestCommandFunc(int cellnumber)
        {
            try
            {
                var selectedcell = CellInfos.SingleOrDefault(cell => cell.CellIndex == cellnumber);
                if (selectedcell != null)
                {
                    EndTestReqLPDate reqdata = new EndTestReqLPDate();
                    reqdata.StageNumber = cellnumber;
                    reqdata.ActionType = EnumRemoteCommand.END_TEST_LP;
                    reqdata.FoupNumber = selectedcell.WaferUnloadFoupNumber;
                    this.GEMModule().GemCommManager.OnRemoteCommandAction(reqdata);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        private AsyncCommand<int> _CellErrorEndCommand;
        public ICommand CellErrorEndCommand
        {
            get
            {
                if (null == _CellErrorEndCommand) _CellErrorEndCommand = new AsyncCommand<int>(CellErrorEndCommandFunc);
                return _CellErrorEndCommand;
            }
        }

        public Task CellErrorEndCommandFunc(int cellnumber)
        {
            try
            {
                var selectedcell = CellInfos.SingleOrDefault(cell => cell.CellIndex == cellnumber);
                if (selectedcell != null)
                {
                    ErrorEndLPData reqdata = new ErrorEndLPData();
                    reqdata.StageNumber = cellnumber;
                    reqdata.ActionType = EnumRemoteCommand.ERROR_END_LP;
                    reqdata.FoupNumber = selectedcell.WaferUnloadFoupNumber;
                    this.GEMModule().GemCommManager.OnRemoteCommandAction(reqdata);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }


        private RelayCommand<int> _FoupDynamicModeEnableCommand;
        public ICommand FoupDynamicModeEnableCommand
        {
            get
            {
                if (null == _FoupDynamicModeEnableCommand) _FoupDynamicModeEnableCommand = new RelayCommand<int>(FoupDynamicModeEnableCommandFunc);
                return _FoupDynamicModeEnableCommand;
            }
        }

        public void FoupDynamicModeEnableCommandFunc(int foupnumber)
        {
            try
            {
                var foupInfo = FoupLotInfos.SingleOrDefault(foupinfo => foupinfo.FoupIndex == foupnumber);
                if(foupInfo != null)
                {
                    foupInfo.DYModeEnable = true;
                    foupInfo.DYModeDisable = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        private RelayCommand<int> _FoupDynamicModeDisableCommand;
        public ICommand FoupDynamicModeDisableCommand
        {
            get
            {
                if (null == _FoupDynamicModeDisableCommand) _FoupDynamicModeDisableCommand = new RelayCommand<int>(FoupDynamicModeDisableCommandFunc);
                return _FoupDynamicModeDisableCommand;
            }
        }

        public void FoupDynamicModeDisableCommandFunc(int foupnumber)
        {
            try
            {
                var foupInfo = FoupLotInfos.SingleOrDefault(foupinfo => foupinfo.FoupIndex == foupnumber);
                if (foupInfo != null)
                {
                    foupInfo.DYModeEnable = false;
                    foupInfo.DYModeDisable = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private RelayCommand<int> _FoupSetLoadModeCommand;
        public ICommand FoupSetLoadModeCommand
        {
            get
            {
                if (null == _FoupSetLoadModeCommand) _FoupSetLoadModeCommand = new RelayCommand<int>(FoupSetLoadModeCommandFunc);
                return _FoupSetLoadModeCommand;
            }
        }

        public void FoupSetLoadModeCommandFunc(int foupnumber)
        {
            try
            {
                ChangeLoadPortModeActReqData reqData = new ChangeLoadPortModeActReqData();
                reqData.FoupNumber = foupnumber;
                reqData.ActionType = EnumRemoteCommand.CHANGE_LP_MODE_STATE;
                reqData.FoupModeState = 1;
                this.GEMModule().GemCommManager.OnRemoteCommandAction(reqData);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private RelayCommand<int> _FoupSetLoadUnloadModeCommand;
        public ICommand FoupSetLoadUnloadModeCommand
        {
            get
            {
                if (null == _FoupSetLoadUnloadModeCommand) _FoupSetLoadUnloadModeCommand = new RelayCommand<int>(FoupSetLoadUnloadModeCommandFunc);
                return _FoupSetLoadUnloadModeCommand;
            }
        }

        public void FoupSetLoadUnloadModeCommandFunc(int foupnumber)
        {
            try
            {
                ChangeLoadPortModeActReqData reqData = new ChangeLoadPortModeActReqData();
                reqData.FoupNumber = foupnumber;
                reqData.ActionType = EnumRemoteCommand.CHANGE_LP_MODE_STATE;
                reqData.FoupModeState = 2;
                this.GEMModule().GemCommManager.OnRemoteCommandAction(reqData);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        private AsyncCommand<object> _SelectedItemChangedCommand;
        public ICommand SelectedItemChangedCommand
        {
            get
            {
                if (null == _SelectedItemChangedCommand) _SelectedItemChangedCommand = new AsyncCommand<object>(SelectedItemChangedCommandFunc);
                return _SelectedItemChangedCommand;
            }
        }
        private Task SelectedItemChangedCommandFunc(object obj)
        {
            try
            {
                IList items = obj as IList;
                List<CellInfo> selectedcellimtes = items.Cast<CellInfo>().ToList();
                SelectedCells = selectedcellimtes;
                if (items != null)
                {
                    int selectedcount = items.Count;

                    //ChangedSelectedItemsCount = selectedcount;

                    foreach (var cell in CellInfos)
                    {
                        var selcell = selectedcellimtes.Find(item => item.CellIndex == cell.CellIndex);
                        if(selcell != null)
                        {
                            cell.IsSelected = true;
                        }
                        else
                        {
                            cell.IsSelected = false;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        //EnableWaitForWaferIdConfrimCommand
        private RelayCommand _EnableWaferIdWaitCommand;
        public ICommand EnableWaferIdWaitCommand
        {
            get
            {
                if (null == _EnableWaferIdWaitCommand) _EnableWaferIdWaitCommand = new RelayCommand(EnableWaferIdWaitCommandFunc);
                return _EnableWaferIdWaitCommand;
            }
        }

        private void EnableWaferIdWaitCommandFunc()
        {
            try
            {
                if (IsWaitWfConfirm)
                {
                    Master.SetIsWaitForWaferIdConfirm(true);
                }
                else
                {
                    Master.SetIsWaitForWaferIdConfirm(false);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _WaferIdWaitDoneCommand;
        public ICommand WaferIdWaitDoneCommand
        {
            get
            {
                if (null == _WaferIdWaitDoneCommand) _WaferIdWaitDoneCommand = new RelayCommand(WaferIdWaitDoneCommandFunc);
                return _WaferIdWaitDoneCommand;
            }
        }

        private void WaferIdWaitDoneCommandFunc()
        {
            try
            {
                int foupNum = 0;
                int SlotNum = 0;

                if (wfcf_foup1.IsChecked == true)
                {
                    foupNum = 1; 
                }
                else if(wfcf_foup2.IsChecked == true)
                {
                    foupNum = 2;
                }

                if(foupNum != 0)
                {
                    SlotNum = WaitDoneSlotNum;
                }

                if(SlotNum != 0)
                {
                    WaferConfirmActReqData reqData = new WaferConfirmActReqData();
                    reqData.LotID = Master.ActiveLotInfos[foupNum - 1].LotID;
                    reqData.PTN = foupNum;
                    reqData.SlotNum = SlotNum;
                    reqData.ActionType = EnumRemoteCommand.WFIDCONFPROC;
                    this.GEMModule().GemCommManager.OnRemoteCommandAction(reqData);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion


    }

    #region Converter 

    public class CellSelectedToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Brush brush = Brushes.Gray;
            try
            {
                bool isSelected = (bool)value;
                if(isSelected == true)
                {
                    brush = Brushes.OrangeRed;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    #endregion
}
