namespace EmulGemView
{
    using Autofac;
    using LoaderBase;
    using LoaderMaster;
    using LoaderParameters;
    using LoaderParameters.Data;
    using LogModule;
    using ProberInterfaces;
    using ProberInterfaces.Foup;
    using RelayCommandBase;
    using SecsGemServiceInterface;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;

    /// <summary>
    /// DryRunTmpWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DryRunTmpWindow : Window, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public DryRunTmpWindow()
        {
            InitializeComponent();

            ModuleTypeArrayArray = Enum.GetValues(typeof(ModuleEnum));
        }

        public DryRunTmpWindow(LoaderSupervisor master) : this()
        {
            try
            {
                Master = master;
                Container = Master.cont;
                DataContext = this;

                // cell 추가 
                for (int i = 1; i < 13; i++)
                {
                    Cells.Add(new CellGridItem(i));
                }
                UpdateCellForcedDoneState();
                InitViewModel();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }


        private void InitViewModel()
        {
            try
            {
                Container = Master.cont;
                int foupcount = SystemModuleCount.ModuleCnt.FoupCount;
                int cellcount = SystemModuleCount.ModuleCnt.StageCount;

                FoupLotInfos = new ObservableCollection<LotInfo>();
                for (int index = 0; index < foupcount; index++)
                {
                    FoupLotInfos.Add(new LotInfo(index + 1, cellcount, Master));
                }

                SelectedFoup = FoupLotInfos.First();

                Cells = new ObservableCollection<CellGridItem>();
                for (int index = 0; index < cellcount; index++)
                {
                    Cells.Add(new CellGridItem(index + 1));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #region <!-- Property -->

        private LoaderSupervisor Master;
        public Autofac.IContainer Container;
        private IFoupOpModule Foup => Container.Resolve<IFoupOpModule>();
        private IGEMModule GemModule => Container.Resolve<IGEMModule>();

        private ObservableCollection<LotInfo> _FoupLotInfos;
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
                    if (_SelectedFoup != null)
                    {
                        _SelectedFoup.IsSelected = false;
                    }
                    _SelectedFoup = value;
                    _SelectedFoup.IsSelected = true;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<CellGridItem> _Cells = new ObservableCollection<CellGridItem>();
        public ObservableCollection<CellGridItem> Cells
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

        private List<CellGridItem> _SelectedCells;
        public List<CellGridItem> SelectedCells
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

        private Array _ModuleTypeArrayArray;
        public Array ModuleTypeArrayArray
        {
            get { return _ModuleTypeArrayArray; }
            set
            {
                if (value != _ModuleTypeArrayArray)
                {
                    _ModuleTypeArrayArray = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ModuleEnum _ModuleType;
        public ModuleEnum ModuleType
        {
            get { return _ModuleType; }
            set
            {
                if (value != _ModuleType)
                {
                    _ModuleType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Array _ForceStateArray = Enum.GetValues(typeof(EnumModuleForcedState));
        public Array ForceStateArray
        {
            get { return ForceStateArray1; }
            set
            {
                if (value != ForceStateArray1)
                {
                    ForceStateArray1 = value;
                    RaisePropertyChanged();
                }
            }
        }


        private EnumModuleForcedState _ForcedState;
        public EnumModuleForcedState ForcedState
        {
            get { return _ForcedState; }
            set
            {
                if (value != _ForcedState)
                {
                    _ForcedState = value;
                    RaisePropertyChanged();
                }
            }
        }

        public Array ForceStateArray1 { get => _ForceStateArray; set => _ForceStateArray = value; }

        #endregion


        private RelayCommand<object> _SelectedItemChangedCommand;
        public ICommand SelectedItemChangedCommand
        {
            get
            {
                if (null == _SelectedItemChangedCommand) _SelectedItemChangedCommand = new RelayCommand<object>(SelectedItemChangedCommandFunc);
                return _SelectedItemChangedCommand;
            }
        }
        private void SelectedItemChangedCommandFunc(object obj)
        {
            try
            {
                IList items = obj as IList;
                List<CellGridItem> selectedcellimtes = items.Cast<CellGridItem>().ToList();
                SelectedCells = selectedcellimtes;
                if (items != null)
                {
                    int selectedcount = items.Count;
                    foreach (var cell in Cells)
                    {
                        var selcell = selectedcellimtes.Find(item => item.Index == cell.Index);
                        if (selcell != null)
                        {
                            cell.IsChecked = true;
                        }
                        else
                        {
                            cell.IsChecked = false;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }



        private RelayCommand<object> _SlotSelectedItemChangedCommand;
        public ICommand SlotSelectedItemChangedCommand
        {
            get
            {
                if (null == _SlotSelectedItemChangedCommand) _SlotSelectedItemChangedCommand = new RelayCommand<object>(SlotSelectedItemChangedCommandFunc);
                return _SlotSelectedItemChangedCommand;
            }
        }
        private void SlotSelectedItemChangedCommandFunc(object obj)
        {
            try
            {
                IList items = obj as IList;
                List<SlotInfo> selectedslotimtes = items.Cast<SlotInfo>().ToList();
                if (items != null)
                {
                    int selectedcount = items.Count;

                    if(SelectedFoup != null)
                    {
                        if(SelectedFoup.SlotInfos != null)
                        {
                            foreach (var slotinfo in SelectedFoup.SlotInfos)
                            {
                                var selslot = selectedslotimtes.Find(item => item.SlotIndex == slotinfo.SlotIndex);
                                if(selslot != null)
                                {
                                    slotinfo.IsSelected = true;
                                }
                                else
                                {
                                    slotinfo.IsSelected = false;
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



        private AsyncCommand<object> _LotStartCommand;
        public ICommand LotStartCommand
        {
            get
            {
                if (null == _LotStartCommand) _LotStartCommand = new AsyncCommand<object>(LotStartCommandFunc, false);
                return _LotStartCommand;
            }
        }
        private async Task LotStartCommandFunc(object obj)
        {
            try
            {
                int foupnumber = (int)obj;

                if(foupnumber != SelectedFoup.FoupIndex)
                {
                    
                    SelectedFoup = FoupLotInfos.SingleOrDefault(info => info.FoupIndex == foupnumber);
                }

                if (MessageBox.Show($"Do you Want LOT#{foupnumber} Start? ", "LOT1 Start Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    await LotStart(foupnumber);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private void UpdateCellForcedDoneState()
        {
            Cells.ToList().ForEach(x =>
            {
                var client = Master.GetClient(x.Index);
                if (client != null)
                {
                    if (client.GetForcedDoneModules().ToList().Count > 0)
                    {
                        Console.WriteLine(client.GetForcedDoneModules().ToList().Count.ToString());
                        x.ForcedDoneLabelVisibility = Visibility.Visible;
                    }
                    else
                    {
                        x.ForcedDoneLabelVisibility = Visibility.Hidden;
                    }
                }
            });
        }


        public async Task LotStart(int FoupNumber)
        {
            try
            {
                if(SelectedFoup == null)
                {
                    LoggerManager.Debug("SelectedFoup is null");
                    return;
                }

                if(SelectedFoup.LotID == null)
                {
                    LoggerManager.Debug("SelectedFoup LOTID is null");
                    return;
                }
                
                if (Master.ActiveLotInfos[FoupNumber - 1].State != LotStateEnum.Running)
                {
                    LoggerManager.Debug($"DemoRunView Click LotStart Foup #{FoupNumber}");

                    var modules = Master.Loader.ModuleManager;

                    var Cassette = Master.Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, FoupNumber);
                    Master.ActiveLotInfos[Cassette.ID.Index - 1].FoupLoad();
                    Master.Loader.ScanCount = 25;

                    Cassette.SetNoReadScanState();

                    List<int> slotList = new List<int>();
                    ActiveProcessActReqData activeData = new ActiveProcessActReqData();
                    activeData.UseStageNumbers_str = "000000000000";
                    activeData.FoupNumber = FoupNumber;
                    string LotID = SelectedFoup.LotID;
                    string slotMapList = "";
                    string cellMapList = "";

                    activeData.LotID = LotID;
                    activeData.UseStageNumbers = new List<int>();

                    Cells.ToList().ForEach(x =>
                    {
                        var idx = x.IsChecked ? 1 : 0;

                        if (idx > 0)
                            activeData.UseStageNumbers.Add(x.Index);
                        cellMapList += idx.ToString();
                    });



                    if (SelectedFoup.SlotInfos != null)
                    {
                        for (int index = SelectedFoup.SlotInfos.Count; index > 0; index--)
                        {
                            if (SelectedFoup.SlotInfos[index - 1].IsSelected)
                            {
                                slotList.Add(SelectedFoup.SlotInfos[index - 1].SlotIndex);
                                slotMapList += "1";
                            }
                            else
                            {
                                if (GemModule.GemSysParam.ReceiveMessageType.Equals("SemicsGemReceiverSEKR"))
                                {
                                    slotMapList += "0";
                                }
                                else
                                {
                                    slotMapList += "3";
                                }
                            }
                        }
                    }


                    Master.GEMModule().IsExternalLotMode = true;

                    if (GemModule.GemSysParam.ReceiveMessageType.Equals("SemicsGemReceiverSEKR"))
                    {
                        ActiveProcessActReqData activelotreqdata = new ActiveProcessActReqData();
                        activelotreqdata.FoupNumber = activeData.FoupNumber;
                        activelotreqdata.LotID = activeData.LotID;
                        activelotreqdata.UseStageNumbers = activeData.UseStageNumbers;
                        foreach (var item in activeData.UseStageNumbers)
                        {
                            activelotreqdata.UseStageNumbers_str = activeData.UseStageNumbers_str.Insert(item - 1, "1");
                        }
                        activelotreqdata.ActionType = EnumRemoteCommand.ACTIVATE_PROCESS;

                        DockFoupActReqData dockFoupActReqData = new DockFoupActReqData();
                        dockFoupActReqData.ActionType = EnumRemoteCommand.DOCK_FOUP;
                        dockFoupActReqData.FoupNumber = FoupNumber;

                        SelectSlotsActReqData selectSlotsActReqData = new SelectSlotsActReqData();
                        selectSlotsActReqData.LotID = LotID;
                        selectSlotsActReqData.ActionType = EnumRemoteCommand.SELECT_SLOTS;
                        selectSlotsActReqData.FoupNumber = FoupNumber;
                        selectSlotsActReqData.UseSlotNumbers = slotList;

                        StartLotActReqData startLotActReqData = new StartLotActReqData();
                        startLotActReqData.LotID = LotID;
                        startLotActReqData.FoupNumber = FoupNumber;
                        startLotActReqData.ActionType = EnumRemoteCommand.START_LOT;

                        Task task = new Task(() =>
                        {
                            GemModule.GemCommManager.OnRemoteCommandAction(activelotreqdata);

                        });
                        task.Start();
                        await task;

                        task = new Task(() =>
                        {
                            GemModule.GemCommManager.OnRemoteCommandAction(dockFoupActReqData);
                            GemModule.GemCommManager.OnRemoteCommandAction(selectSlotsActReqData);
                            GemModule.GemCommManager.OnRemoteCommandAction(startLotActReqData);

                        });
                        task.Start();
                        await task;

                    }
                    else if (GemModule.GemSysParam.ReceiveMessageType.Equals("SemicsGemReceiverSEKT") ||
                            GemModule.GemSysParam.ReceiveMessageType.Equals("SemicsGemReceiverSEKS"))
                    {
                        ProceedWithCarrierReqData proceedWithCarrierReqData = new ProceedWithCarrierReqData();
                        proceedWithCarrierReqData.DataID = 0;
                        proceedWithCarrierReqData.CattrData = new string[1];
                        proceedWithCarrierReqData.CattrData[0] = activeData.LotID;
                        proceedWithCarrierReqData.PTN = activeData.FoupNumber;
                        proceedWithCarrierReqData.ActionType = EnumCarrierAction.PROCEEDWITHCARRIER;

                        ProceedWithCarrierReqData proceedWithCarrierReqData2 = new ProceedWithCarrierReqData();
                        proceedWithCarrierReqData2.DataID = 1;
                        proceedWithCarrierReqData2.PTN = activeData.FoupNumber;
                        proceedWithCarrierReqData2.ActionType = EnumCarrierAction.PROCEEDWITHCARRIER;
                        proceedWithCarrierReqData2.SlotMap = new string[25];
                        for (int index = 0; index < 25; index++)
                        {
                            proceedWithCarrierReqData2.SlotMap[index] = "CE2GT171SEB" + string.Format("{0:00}", (index + 1) * activeData.FoupNumber);
                        }

                        ProceedWithCellSlotActReqData proceedWithCellSlotActReqData = new ProceedWithCellSlotActReqData();
                        proceedWithCellSlotActReqData.PTN = activeData.FoupNumber;
                        proceedWithCellSlotActReqData.LOTID = activeData.LotID;
                        proceedWithCellSlotActReqData.CellMap = activeData.UseStageNumbers.ToString();
                        proceedWithCellSlotActReqData.SlotMap = slotMapList;
                        proceedWithCellSlotActReqData.CellMap = cellMapList;
                        proceedWithCellSlotActReqData.ActionType = EnumCarrierAction.PROCESSEDWITHCELLSLOT;


                        StartLotActReqData startLotActReqData = new StartLotActReqData();
                        startLotActReqData.FoupNumber = activeData.FoupNumber;
                        //startLotActReqData.OCRReadFalg = 0;
                        startLotActReqData.LotID = activeData.LotID;
                        startLotActReqData.ActionType = EnumRemoteCommand.PSTART;
                        Task task = new Task(() =>
                        {
                            var cassette = Master.Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, activeData.FoupNumber);

                            GemModule.GemCommManager.OnCarrierActMsgRecive(proceedWithCarrierReqData);
                            while (true)
                            {
                                if (cassette.ScanState == CassetteScanStateEnum.ILLEGAL || cassette.ScanState == CassetteScanStateEnum.READ)
                                {
                                    if (cassette.ScanState == CassetteScanStateEnum.READ)
                                    {
                                        break;
                                    }
                                }
                            }
                            Thread.Sleep(1000);
                            while (true)
                            {
                                var sctmodule = Master.Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, activeData.FoupNumber);
                                if (sctmodule.ScanState == CassetteScanStateEnum.ILLEGAL || sctmodule.ScanState == CassetteScanStateEnum.READ)
                                {
                                    if (sctmodule.ScanState == CassetteScanStateEnum.READ)
                                    {
                                        break;
                                    }
                                }
                            }
                            GemModule.GemCommManager.OnCarrierActMsgRecive(proceedWithCarrierReqData2);
                            Thread.Sleep(1000);
                            GemModule.GemCommManager.OnCarrierActMsgRecive(proceedWithCellSlotActReqData);
                            Thread.Sleep(1000);
                            GemModule.GemCommManager.OnRemoteCommandAction(startLotActReqData);
                        });
                        task.Start();
                        await task;
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void CB_SlotAll_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if(SelectedFoup != null)
                {
                    if(SelectedFoup.SlotInfos != null)
                    {
                        foreach (var slotinfo in SelectedFoup.SlotInfos)
                        {
                            slotinfo.IsSelected = true;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void CB_SlotAll_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedFoup != null)
                {
                    if (SelectedFoup.SlotInfos != null)
                    {
                        foreach (var slotinfo in SelectedFoup.SlotInfos)
                        {
                            slotinfo.IsSelected = false;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void CB_StageAll_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                Cells.ToList().ForEach(x =>
                {
                    x.IsChecked = true;
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void CB_StageAll_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                Cells.ToList().ForEach(x =>
                {
                    x.IsChecked = false;
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private void Stage_forcedone_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Cells.ToList().ForEach(x =>
                {
                    if (x.IsChecked)
                    {
                        var client = Master.GetClient(x.Index);
                        client?.SetForcedDone(ForcedState);
                    }

                });
                UpdateCellForcedDoneState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void Stage_forcedoneSelected_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Cells.ToList().ForEach(x =>
                {
                    if (x.IsChecked)
                    {
                        var client = Master.GetClient(x.Index);
                        client?.SetForcedDoneSpecificModule(ForcedState, ModuleType);
                    }
                });
                UpdateCellForcedDoneState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void LOT1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.Master.ActiveLotInfos[0].IsActiveFromHost
                     = !this.Master.ActiveLotInfos[0].IsActiveFromHost;
                LoggerManager.Debug($"Foup 1's IsActiveFormHost change to { this.Master.ActiveLotInfos[0].IsActiveFromHost.ToString()}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void LOT2_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.Master.ActiveLotInfos[1].IsActiveFromHost
                     = !this.Master.ActiveLotInfos[1].IsActiveFromHost;
                LoggerManager.Debug($"Foup 2's IsActiveFormHost change to { this.Master.ActiveLotInfos[1].IsActiveFromHost.ToString()}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void LOT3_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.Master.ActiveLotInfos[2].IsActiveFromHost
                    = !this.Master.ActiveLotInfos[2].IsActiveFromHost;
                LoggerManager.Debug($"Foup 3's IsActiveFormHost change to { this.Master.ActiveLotInfos[2].IsActiveFromHost.ToString()}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    #region Infomation Class

    public class LotInfo : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private int _FoupIndex;
        public int FoupIndex
        {
            get { return _FoupIndex; }
            set
            {
                if (value != _FoupIndex)
                {
                    _FoupIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _LotID;
        public string LotID
        {
            get { return _LotID; }
            set
            {
                if (value != _LotID)
                {
                    _LotID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _DYModeEnable = true;
        public bool DYModeEnable
        {
            get { return _DYModeEnable; }
            set
            {
                if (value != _DYModeEnable)
                {
                    _DYModeEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        public ILoaderSupervisor Master { get; set; }
        private bool _DYModeDisable;
        public bool DYModeDisable
        {
            get { return _DYModeDisable; }
            set
            {
                if (value != _DYModeDisable)
                {
                    _DYModeDisable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private DynamicFoupStateEnum _DynamicFoupState;
        public DynamicFoupStateEnum DynamicFoupState
        {
            get { return _DynamicFoupState; }
            set
            {
                if (value != _DynamicFoupState)
                {
                    _DynamicFoupState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<SlotInfo> _SlotInfos;
        public ObservableCollection<SlotInfo> SlotInfos
        {
            get { return _SlotInfos; }
            set
            {
                if (value != _SlotInfos)
                {
                    _SlotInfos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsSelected;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (value != _IsSelected)
                {
                    _IsSelected = value;
                    RaisePropertyChanged();
                }
            }
        }


        private RelayCommand _LoadUnloadCommand;
        public ICommand LoadUnloadCommand
        {
            get
            {
                if (null == _LoadUnloadCommand) _LoadUnloadCommand = new RelayCommand(LoadUnloadCommandFunc);
                return _LoadUnloadCommand;
            }
        }

        public void LoadUnloadCommandFunc()
        {
            DynamicFoupState = DynamicFoupStateEnum.LOAD_AND_UNLOAD;
            Master.ActiveLotInfos[FoupIndex - 1].DynamicFoupState = DynamicFoupState;
            Master.Loader.Foups[FoupIndex - 1].DynamicFoupState = DynamicFoupState;
        }

        private RelayCommand _OnlyUnloadCommand;
        public ICommand OnlyUnloadCommand
        {
            get
            {
                if (null == _OnlyUnloadCommand) _OnlyUnloadCommand = new RelayCommand(OnlyUnloadCommandFunc);
                return _OnlyUnloadCommand;
            }
        }

        public void OnlyUnloadCommandFunc()
        {
            DynamicFoupState = DynamicFoupStateEnum.UNLOAD;
            Master.ActiveLotInfos[FoupIndex - 1].DynamicFoupState = DynamicFoupState;
            Master.Loader.Foups[FoupIndex - 1].DynamicFoupState = DynamicFoupState;
        }





        public LotInfo(int foupidx, int stageCount, ILoaderSupervisor loaderMaster)
        {
            try
            {
                FoupIndex = foupidx;
                SlotInfos = new ObservableCollection<SlotInfo>();
                Master = loaderMaster;
                for (int index = 25; index > 0; index--)
                {
                    SlotInfos.Add(new SlotInfo(index, stageCount));
                }
            }
            catch (Exception)
            {

            }
        }
    }

    public class SlotInfo : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private int _SlotIndex;
        public int SlotIndex
        {
            get { return _SlotIndex; }
            set
            {
                if (value != _SlotIndex)
                {
                    _SlotIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsSelected;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (value != _IsSelected)
                {
                    _IsSelected = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ObservableCollection<SlotCellInfomation> _SlotCellInfos;
        public ObservableCollection<SlotCellInfomation> SlotCellInfos
        {
            get { return _SlotCellInfos; }
            set
            {
                if (value != _SlotCellInfos)
                {
                    _SlotCellInfos = value;
                    RaisePropertyChanged();
                }
            }
        }

        public SlotInfo(int slotindex, int stageCount)
        {
            try
            {
                SlotIndex = slotindex;
                SlotCellInfos = new ObservableCollection<SlotCellInfomation>();
                int stageMaxCount = 12;

                for (int index = 0; index < stageMaxCount; index++)
                {
                    SlotCellInfos.Add(new SlotCellInfomation(index + 1));
                }
            }
            catch (Exception)
            {

            }
        }
    }

    public class SlotCellInfomation : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private int _CellIndex;
        public int CellIndex
        {
            get { return _CellIndex; }
            set
            {
                if (value != _CellIndex)
                {
                    _CellIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _EnableCell = false;
        public bool EnableCell
        {
            get { return _EnableCell; }
            set
            {
                if (value != _EnableCell)
                {
                    _EnableCell = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _EnableSlot;
        public bool EnableSlot
        {
            get { return _EnableSlot; }
            set
            {
                if (value != _EnableSlot)
                {
                    _EnableSlot = value;
                    RaisePropertyChanged();
                }
            }
        }

        public SlotCellInfomation(int cellNumber)
        {
            CellIndex = cellNumber;
        }


    }

    public class CellInfo : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private int _CellIndex;
        public int CellIndex
        {
            get { return _CellIndex; }
            set
            {
                if (value != _CellIndex)
                {
                    _CellIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _CurDeviceName;
        public string CurDeviceName
        {
            get { return _CurDeviceName; }
            set
            {
                if (value != _CurDeviceName)
                {
                    _CurDeviceName = value;
                    RaisePropertyChanged();
                }
            }
        }


        private string _SetDeviceName;
        public string SetDeviceName
        {
            get { return _SetDeviceName; }
            set
            {
                if (value != _SetDeviceName)
                {
                    _SetDeviceName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _WaferUnloadFoupNumber;
        public int WaferUnloadFoupNumber
        {
            get { return _WaferUnloadFoupNumber; }
            set
            {
                if (value != _WaferUnloadFoupNumber)
                {
                    _WaferUnloadFoupNumber = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsSelected;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (value != _IsSelected)
                {
                    _IsSelected = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _IsForecedDone;
        public bool IsForecedDone
        {
            get { return _IsForecedDone; }
            set
            {
                if (value != _IsForecedDone)
                {
                    _IsForecedDone = value;
                    RaisePropertyChanged();
                }
            }
        }

        public CellInfo(int cellidx)
        {
            CellIndex = cellidx;
        }
    }

    #endregion

    #region Converter 

    public class CellSelectedToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Brush brush = Brushes.Gray;
            try
            {
                bool isSelected = (bool)value;
                if (isSelected == true)
                {
                    brush = Brushes.MediumPurple;
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
