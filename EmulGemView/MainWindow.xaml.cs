using Autofac;
using LoaderBase;
using LoaderMaster;
using LoaderParameters;
using LogModule;
using NotifyEventModule;
using ProberInterfaces;
using ProberInterfaces.Event;
using ProberInterfaces.Foup;
using ProberInterfaces.Loader;
using SecsGemServiceInterface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using MetroDialogInterfaces;

namespace EmulGemView
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            ModuleTypeArrayArray = Enum.GetValues(typeof(ModuleEnum));
        }

        private LoaderSupervisor Master;
        public Autofac.IContainer Container;
        private IFoupOpModule Foup => Container.Resolve<IFoupOpModule>();
        private IGEMModule GemModule => Container.Resolve<IGEMModule>();

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

        private ObservableCollection<string> _DeviceNames
         = new ObservableCollection<string>();
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


        private DownloadStageRecipeActReqData DownloadReqData = new DownloadStageRecipeActReqData() { UseFTP = false };
        private bool isWaitDownloadRecipe = true;
        public MainWindow(LoaderSupervisor master) : this()
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
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
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
        
        //private bool isFirstDemoRun = false;
        public async Task LotStart(int FoupNumber)
        {

            try
            {
                if(FoupNumber > SystemModuleCount.ModuleCnt.FoupCount)
                {
                    MessageBox.Show($"This Foup is not available.", "LOT Start Fail", MessageBoxButton.OK);
                    return;
                }

                if (Master.ActiveLotInfos[FoupNumber - 1].State != LotStateEnum.Running)
                {
                    LoggerManager.Debug($"DemoRunView Click LotStart Foup #{FoupNumber}");
                    isWaitDownloadRecipe = true;
                    var modules = Master.Loader.ModuleManager;

                    var Cassette = Master.Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, FoupNumber);
                    Master.ActiveLotInfos[Cassette.ID.Index - 1].FoupLoad();
                    Master.Loader.ScanCount = 25;

                    Cassette.SetNoReadScanState();

                    List<int> slotList = new List<int>();
                    ActiveProcessActReqData activeData = new ActiveProcessActReqData();
                    activeData.UseStageNumbers_str = "000000000000";
                    activeData.FoupNumber = FoupNumber;
                    string LotID = null;
                    string slotMapList = "";
                    string cellMapList = "";
                    if (FoupNumber == 1)
                    {
                        LotID = tb_LOT1ID.Text;
                    }
                    else if (FoupNumber == 2)
                    {
                        LotID = tb_LOT2ID.Text;
                    }
                    else if (FoupNumber == 3)
                    {
                        LotID = tb_LOT3ID.Text;
                    }
                    else if (FoupNumber == 4)
                    {
                        LotID = tb_LOT4ID.Text;
                    }
                    activeData.LotID = LotID;
                    activeData.UseStageNumbers = new List<int>();

                    Cells.ToList().ForEach(x =>
                    {
                        var idx = x.IsChecked ? 1 : 0;

                        if (idx > 0)
                            activeData.UseStageNumbers.Add(x.Index);
                        cellMapList += idx.ToString();
                    });

                    if(activeData.UseStageNumbers.Count == 0)
                    {
                        MessageBox.Show($"There is no stage selected.", "LOT Start Fail", MessageBoxButton.OK);
                        return;
                    }

                    if (CB_SLOT1.IsChecked.Value)
                    {
                        slotList.Add(1);
                        slotMapList = "1";
                    }
                    else
                    {
                        slotMapList = "0";
                    }
                    if (CB_SLOT2.IsChecked.Value)
                    {
                        slotList.Add(2);
                        slotMapList += "1";
                    }
                    else
                    {
                        slotMapList += "0";
                    }
                    if (CB_SLOT3.IsChecked.Value)
                    {
                        slotList.Add(3);
                        slotMapList += "1";
                    }
                    else
                    {
                        slotMapList += "0";
                    }
                    if (CB_SLOT4.IsChecked.Value)
                    {
                        slotList.Add(4);
                        slotMapList += "1";
                    }
                    else
                    {
                        slotMapList += "0";
                    }
                    if (CB_SLOT5.IsChecked.Value)
                    {
                        slotList.Add(5);
                        slotMapList += "1";
                    }
                    else
                    {
                        slotMapList += "0";
                    }
                    if (CB_SLOT6.IsChecked.Value)
                    {
                        slotList.Add(6);
                        slotMapList += "1";
                    }
                    else
                    {
                        slotMapList += "0";
                    }
                    if (CB_SLOT7.IsChecked.Value)
                    {
                        slotList.Add(7);
                        slotMapList += "1";
                    }
                    else
                    {
                        slotMapList += "0";
                    }
                    if (CB_SLOT8.IsChecked.Value)
                    {
                        slotList.Add(8);
                        slotMapList += "1";
                    }
                    else
                    {
                        slotMapList += "0";
                    }
                    if (CB_SLOT9.IsChecked.Value)
                    {
                        slotList.Add(9);
                        slotMapList += "1";
                    }
                    else
                    {
                        slotMapList += "0";
                    }
                    if (CB_SLOT10.IsChecked.Value)
                    {
                        slotList.Add(10);
                        slotMapList += "1";
                    }
                    else
                    {
                        slotMapList += "0";
                    }
                    if (CB_SLOT11.IsChecked.Value)
                    {
                        slotList.Add(11);
                        slotMapList += "1";
                    }
                    else
                    {
                        slotMapList += "0";
                    }
                    if (CB_SLOT12.IsChecked.Value)
                    {
                        slotList.Add(12);
                        slotMapList += "1";
                    }
                    else
                    {
                        slotMapList += "0";
                    }
                    if (CB_SLOT13.IsChecked.Value)
                    {
                        slotList.Add(13);
                        slotMapList += "1";
                    }
                    else
                    {
                        slotMapList += "0";
                    }
                    if (CB_SLOT14.IsChecked.Value)
                    {
                        slotList.Add(14);
                        slotMapList += "1";
                    }
                    else
                    {
                        slotMapList += "0";
                    }
                    if (CB_SLOT15.IsChecked.Value)
                    {
                        slotList.Add(15);
                        slotMapList += "1";
                    }
                    else
                    {
                        slotMapList += "0";
                    }
                    if (CB_SLOT16.IsChecked.Value)
                    {
                        slotList.Add(16);
                        slotMapList += "1";
                    }
                    else
                    {
                        slotMapList += "0";
                    }
                    if (CB_SLOT17.IsChecked.Value)
                    {
                        slotList.Add(17);
                        slotMapList += "1";
                    }
                    else
                    {
                        slotMapList += "0";
                    }
                    if (CB_SLOT18.IsChecked.Value)
                    {
                        slotList.Add(18);
                        slotMapList += "1";
                    }
                    else
                    {
                        slotMapList += "0";
                    }
                    if (CB_SLOT19.IsChecked.Value)
                    {
                        slotList.Add(19);
                        slotMapList += "1";
                    }
                    else
                    {
                        slotMapList += "0";
                    }
                    if (CB_SLOT20.IsChecked.Value)
                    {
                        slotList.Add(20);
                        slotMapList += "1";
                    }
                    else
                    {
                        slotMapList += "0";
                    }
                    if (CB_SLOT21.IsChecked.Value)
                    {
                        slotList.Add(21);
                        slotMapList += "1";
                    }
                    else
                    {
                        slotMapList += "0";
                    }
                    if (CB_SLOT22.IsChecked.Value)
                    {
                        slotList.Add(22);
                        slotMapList += "1";
                    }
                    else
                    {
                        slotMapList += "0";
                    }
                    if (CB_SLOT23.IsChecked.Value)
                    {
                        slotList.Add(23);
                        slotMapList += "1";
                    }
                    else
                    {
                        slotMapList += "0";
                    }
                    if (CB_SLOT24.IsChecked.Value)
                    {
                        slotList.Add(24);
                        slotMapList += "1";
                    }
                    else
                    {
                        slotMapList += "0";
                    }
                    if (CB_SLOT25.IsChecked.Value)
                    {
                        slotList.Add(25);
                        slotMapList += "1";
                    }
                    else
                    {
                        slotMapList += "0";
                    }

                    
                    if (Master.GEMModule().IsExternalLotMode == false)
                    {
                        //isFirstDemoRun = true;
                    }
                    Master.GEMModule().IsExternalLotMode = true;

                    //DownloadReqData.ActionType = EnumRemoteCommand.DOWNLOAD_STAGE_RECIPE;
                    DownloadReqData.LotID = activeData.LotID;
                    DownloadReqData.FoupNumber = activeData.FoupNumber;

                    if (FoupNumber == 1)
                    {
                        if (rb_nonePMI1.IsChecked == true)
                        {
                            Master.ActiveLotInfos[FoupNumber - 1].PMIEveryInterval = 0;
                            Master.ActiveLotInfos[FoupNumber - 1].DoPMICount = 0;
                        }
                        else if (rb_everyPMI1.IsChecked == true)
                        {
                            int count = 0;
                            if (Int32.TryParse(tb_PMI1COUNT.Text, out count))
                            {
                                Master.ActiveLotInfos[FoupNumber - 1].PMIEveryInterval = count;
                            }
                        }
                        else if (rb_totalPMI1.IsChecked == true)
                        {
                            int count = 0;
                            if (Int32.TryParse(tb_PMI1COUNT.Text, out count))
                            {
                                Master.ActiveLotInfos[FoupNumber - 1].DoPMICount = count;
                            }
                        }
                    }
                    else if (FoupNumber == 2)
                    {
                        if (rb_nonePMI2.IsChecked == true)
                        {
                            Master.ActiveLotInfos[FoupNumber - 1].PMIEveryInterval = 0;
                            Master.ActiveLotInfos[FoupNumber - 1].DoPMICount = 0;
                        }
                        else if (rb_everyPMI2.IsChecked == true)
                        {
                            int count = 0;
                            if (Int32.TryParse(tb_PMI2COUNT.Text, out count))
                            {
                                Master.ActiveLotInfos[FoupNumber - 1].PMIEveryInterval = count;
                            }
                        }
                        else if (rb_totalPMI2.IsChecked == true)
                        {
                            int count = 0;
                            if (Int32.TryParse(tb_PMI2COUNT.Text, out count))
                            {
                                Master.ActiveLotInfos[FoupNumber - 1].DoPMICount = count;
                            }
                        }
                    }
                    else if (FoupNumber == 3)
                    {
                        if (rb_nonePMI3.IsChecked == true)
                        {
                            Master.ActiveLotInfos[FoupNumber - 1].PMIEveryInterval = 0;
                            Master.ActiveLotInfos[FoupNumber - 1].DoPMICount = 0;
                        }
                        else if (rb_everyPMI3.IsChecked == true)
                        {
                            int count = 0;
                            if (Int32.TryParse(tb_PMI3COUNT.Text, out count))
                            {
                                Master.ActiveLotInfos[FoupNumber - 1].PMIEveryInterval = count;
                            }
                        }
                        else if (rb_totalPMI3.IsChecked == true)
                        {
                            int count = 0;
                            if (Int32.TryParse(tb_PMI3COUNT.Text, out count))
                            {
                                Master.ActiveLotInfos[FoupNumber - 1].DoPMICount = count;
                            }
                        }
                    }
                    else if (FoupNumber == 4)
                    {
                        if (rb_nonePMI4.IsChecked == true)
                        {
                            Master.ActiveLotInfos[FoupNumber - 1].PMIEveryInterval = 0;
                            Master.ActiveLotInfos[FoupNumber - 1].DoPMICount = 0;
                        }
                        else if (rb_everyPMI4.IsChecked == true)
                        {
                            int count = 0;
                            if (Int32.TryParse(tb_PMI4COUNT.Text, out count))
                            {
                                Master.ActiveLotInfos[FoupNumber - 1].PMIEveryInterval = count;
                            }
                        }
                        else if (rb_totalPMI4.IsChecked == true)
                        {
                            int count = 0;
                            if (Int32.TryParse(tb_PMI4COUNT.Text, out count))
                            {
                                Master.ActiveLotInfos[FoupNumber - 1].DoPMICount = count;
                            }
                        }
                    }

                    if (GemModule.GemSysParam.ReceiveMessageType.Equals("SemicsGemReceiverSEKR"))
                    {
                        //Master.ActiveProcess(activeData);
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
                        Application.Current.Dispatcher.Invoke(() =>
                            {
                                //if (DownloadReqData.Count > 0)
                                if (DownloadReqData.RecipeDic.Count > 0)
                                {
                                    GemModule.ResetStageDownloadRecipe();
                                    var ret = Master.DeviceManager().SetRecipeToStage(DownloadReqData);
                                    Master.SetRecipeToDevice(DownloadReqData);
                                }
                            });

                        //if (DownloadReqData.Count > 0)
                        task = new Task(() =>
                        {
                            //if (DownloadReqData.Count > 0)
                            if (DownloadReqData.RecipeDic.Count > 0)
                            {
                                while (isWaitDownloadRecipe == true)
                                {
                                    if (GemModule.GetComplateDownloadRecipe(activeData.UseStageNumbers))
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
                        else
                        {
                        }

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
                        proceedWithCarrierReqData.CarrierID = tb_probecard_id.Text + activeData.FoupNumber.ToString();
                        proceedWithCarrierReqData.ActionType = EnumCarrierAction.PROCEEDWITHCARRIER;

                        ProceedWithCarrierReqData proceedWithCarrierReqData2 = new ProceedWithCarrierReqData();
                        proceedWithCarrierReqData2.DataID = 1;
                        proceedWithCarrierReqData2.PTN = activeData.FoupNumber;
                        proceedWithCarrierReqData2.CarrierID = tb_probecard_id.Text + activeData.FoupNumber.ToString();
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
                        proceedWithCellSlotActReqData.CarrierID = tb_probecard_id.Text + activeData.FoupNumber.ToString();
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
                    else if (GemModule.GemSysParam.ReceiveMessageType.Equals("SemicsGemReceiverSEKX"))
                    {
                        //EquipmentReqData EquipmentReqData = new EquipmentReqData();
                        //EquipmentReqData.ECID = new uint[1] { 3800 };
                        //EquipmentReqData.ECV = "1";




                        ProceedWithCarrierReqData proceedWithCarrierReqData = new ProceedWithCarrierReqData();
                        proceedWithCarrierReqData.DataID = 0;
                        proceedWithCarrierReqData.CattrData = new string[1];
                        proceedWithCarrierReqData.CattrData[0] = activeData.LotID;
                        proceedWithCarrierReqData.LotID = activeData.LotID;
                        proceedWithCarrierReqData.PTN = activeData.FoupNumber;
                        proceedWithCarrierReqData.CarrierID = tb_probecard_id.Text + activeData.FoupNumber.ToString();
                        proceedWithCarrierReqData.ActionType = EnumCarrierAction.PROCEEDWITHCARRIER;
                        

                        ProceedWithSlotReqData proceedWithSlot = new ProceedWithSlotReqData();
                        proceedWithSlot.DataID = 1;
                        proceedWithSlot.PTN = activeData.FoupNumber;
                        proceedWithSlot.CarrierID = tb_probecard_id.Text + activeData.FoupNumber.ToString();
                        proceedWithSlot.ActionType = EnumCarrierAction.PROCEEDWITHSLOT;
                        proceedWithSlot.OcrMap = new string[25];


                        int startidx = (1 * 25) * (activeData.FoupNumber - 1) + 1;
                        int endidx = (1 * 25) * (activeData.FoupNumber);
                        for (int index = startidx; index <= endidx; index++)
                        {
                            proceedWithSlot.OcrMap[index - (25 * (activeData.FoupNumber - 1)) - 1] = "CE2GT171SEB" + string.Format("{0:00}", index);
                        }


                        SelectStageSlotActReqData selectStageSlotReqData = new SelectStageSlotActReqData();
                        selectStageSlotReqData.PTN = activeData.FoupNumber;
                        selectStageSlotReqData.CarrierId = tb_probecard_id.Text + activeData.FoupNumber.ToString();
                        selectStageSlotReqData.CellMap = cellMapList;
                        selectStageSlotReqData.SlotMap = slotMapList;
                        selectStageSlotReqData.ActionType = EnumRemoteCommand.STAGE_SLOT;



                        //DownloadStageRecipeActReqData downloadrecipe = new DownloadStageRecipeActReqData();
                        //var cellmaparr = cellMapList.ToArray();
                        //for (int i = 0; i < cellMapList.Length; i++)
                        //{
                        //    if (cellmaparr[i] == 1)
                        //    {
                        //        downloadrecipe.RecipeDic.Add(cellmaparr[i], SelectedDevice);
                        //    }
                        //}
                        //downloadrecipe.ActionType = EnumRemoteCommand.DLRECIPE;

                        StartLotActReqData startLotActReqData = new StartLotActReqData();
                        startLotActReqData.FoupNumber = activeData.FoupNumber;
                        //startLotActReqData.OCRReadFalg = 1;
                        startLotActReqData.LotID = activeData.LotID;
                        if (CB_TCWCheck.IsChecked.Value)
                        {
                            startLotActReqData.ActionType = EnumRemoteCommand.TC_START;
                        }
                        else
                        {
                            startLotActReqData.ActionType = EnumRemoteCommand.PSTART;
                        }
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
                           
                            //GemModule.GemCommManager.ECVChangeMsgReceive(EquipmentReqData);                        
                            GemModule.GemCommManager.OnCarrierActMsgRecive(proceedWithSlot);
                            GemModule.GetPIVContainer().SetCarrierId(proceedWithSlot.PTN, proceedWithSlot.CarrierID);
                            Thread.Sleep(1000);
                            GemModule.GemCommManager.OnRemoteCommandAction(selectStageSlotReqData);
                            Thread.Sleep(1000);
                            //if (downloadrecipe.RecipeDic.Count > 0)
                            //{
                            //    GemModule.GemCommManager.OnRemoteCommandAction(downloadrecipe);
                            //}
                            //Thread.Sleep(1000);
                            GemModule.GemCommManager.OnRemoteCommandAction(startLotActReqData);
                        });
                        task.Start();
                        await task;


                    }
                }
            }
            catch
            {

            }
        }

        public async Task MultiLotStart(List<int> foupNums)
        {

            try
            {
                foreach (var foupNum in foupNums)
                {
                    if (Master.ActiveLotInfos[foupNum - 1].State != LotStateEnum.Running)
                    {
                        isWaitDownloadRecipe = true;
                        var modules = Master.Loader.ModuleManager;
                        //this.LoaderModule.GetLoaderCommands().CassetteLoad(Cassette);

                        var Cassette = Master.Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, foupNum);
                        Foup.FoupControllers[Cassette.ID.Index - 1].Execute(new FoupLoadCommand());
                        Master.Loader.ScanCount = 25;

                        Cassette.SetNoReadScanState();

                        List<int> slotList = new List<int>();
                        ActiveProcessActReqData activeData = new ActiveProcessActReqData();
                        activeData.UseStageNumbers_str = "000000000000";
                        activeData.FoupNumber = foupNum;
                        string LotID = null;
                        string slotMapList = "";
                        string cellMapList = "";
                        if (foupNum == 1)
                        {
                            LotID = tb_LOT1ID.Text;
                        }
                        else if (foupNum == 2)
                        {
                            LotID = tb_LOT2ID.Text;
                        }
                        else if (foupNum == 3)
                        {
                            LotID = tb_LOT3ID.Text;
                        }
                        else if (foupNum == 4)
                        {
                            LotID = tb_LOT4ID.Text;
                        }
                        activeData.LotID = LotID;
                        activeData.UseStageNumbers = new List<int>();

                        Cells.ToList().ForEach(x =>
                        {
                            var idx = x.IsChecked ? 1 : 0;

                            if (idx > 0)
                                activeData.UseStageNumbers.Add(x.Index);
                            cellMapList += idx.ToString();
                        });

                        if (activeData.UseStageNumbers.Count == 0)
                        {
                            MessageBox.Show($"There is no stage selected in foup({foupNum}).", "LOT Start Fail", MessageBoxButton.OK);
                            continue;
                        }

                        if (CB_SLOT1.IsChecked.Value)
                        {
                            slotList.Add(1);
                            slotMapList = "1";
                        }
                        else
                        {
                            slotMapList = "0";
                        }
                        if (CB_SLOT2.IsChecked.Value)
                        {
                            slotList.Add(2);
                            slotMapList += "1";
                        }
                        else
                        {
                            slotMapList += "0";
                        }
                        if (CB_SLOT3.IsChecked.Value)
                        {
                            slotList.Add(3);
                            slotMapList += "1";
                        }
                        else
                        {
                            slotMapList += "0";
                        }
                        if (CB_SLOT4.IsChecked.Value)
                        {
                            slotList.Add(4);
                            slotMapList += "1";
                        }
                        else
                        {
                            slotMapList += "0";
                        }
                        if (CB_SLOT5.IsChecked.Value)
                        {
                            slotList.Add(5);
                            slotMapList += "1";
                        }
                        else
                        {
                            slotMapList += "0";
                        }
                        if (CB_SLOT6.IsChecked.Value)
                        {
                            slotList.Add(6);
                            slotMapList += "1";
                        }
                        else
                        {
                            slotMapList += "0";
                        }
                        if (CB_SLOT7.IsChecked.Value)
                        {
                            slotList.Add(7);
                            slotMapList += "1";
                        }
                        else
                        {
                            slotMapList += "0";
                        }
                        if (CB_SLOT8.IsChecked.Value)
                        {
                            slotList.Add(8);
                            slotMapList += "1";
                        }
                        else
                        {
                            slotMapList += "0";
                        }
                        if (CB_SLOT9.IsChecked.Value)
                        {
                            slotList.Add(9);
                            slotMapList += "1";
                        }
                        else
                        {
                            slotMapList += "0";
                        }
                        if (CB_SLOT10.IsChecked.Value)
                        {
                            slotList.Add(10);
                            slotMapList += "1";
                        }
                        else
                        {
                            slotMapList += "0";
                        }
                        if (CB_SLOT11.IsChecked.Value)
                        {
                            slotList.Add(11);
                            slotMapList += "1";
                        }
                        else
                        {
                            slotMapList += "0";
                        }
                        if (CB_SLOT12.IsChecked.Value)
                        {
                            slotList.Add(12);
                            slotMapList += "1";
                        }
                        else
                        {
                            slotMapList += "0";
                        }
                        if (CB_SLOT13.IsChecked.Value)
                        {
                            slotList.Add(13);
                            slotMapList += "1";
                        }
                        else
                        {
                            slotMapList += "0";
                        }
                        if (CB_SLOT14.IsChecked.Value)
                        {
                            slotList.Add(14);
                            slotMapList += "1";
                        }
                        else
                        {
                            slotMapList += "0";
                        }
                        if (CB_SLOT15.IsChecked.Value)
                        {
                            slotList.Add(15);
                            slotMapList += "1";
                        }
                        else
                        {
                            slotMapList += "0";
                        }
                        if (CB_SLOT16.IsChecked.Value)
                        {
                            slotList.Add(16);
                            slotMapList += "1";
                        }
                        else
                        {
                            slotMapList += "0";
                        }
                        if (CB_SLOT17.IsChecked.Value)
                        {
                            slotList.Add(17);
                            slotMapList += "1";
                        }
                        else
                        {
                            slotMapList += "0";
                        }
                        if (CB_SLOT18.IsChecked.Value)
                        {
                            slotList.Add(18);
                            slotMapList += "1";
                        }
                        else
                        {
                            slotMapList += "0";
                        }
                        if (CB_SLOT19.IsChecked.Value)
                        {
                            slotList.Add(19);
                            slotMapList += "1";
                        }
                        else
                        {
                            slotMapList += "0";
                        }
                        if (CB_SLOT20.IsChecked.Value)
                        {
                            slotList.Add(20);
                            slotMapList += "1";
                        }
                        else
                        {
                            slotMapList += "0";
                        }
                        if (CB_SLOT21.IsChecked.Value)
                        {
                            slotList.Add(21);
                            slotMapList += "1";
                        }
                        else
                        {
                            slotMapList += "0";
                        }
                        if (CB_SLOT22.IsChecked.Value)
                        {
                            slotList.Add(22);
                            slotMapList += "1";
                        }
                        else
                        {
                            slotMapList += "0";
                        }
                        if (CB_SLOT23.IsChecked.Value)
                        {
                            slotList.Add(23);
                            slotMapList += "1";
                        }
                        else
                        {
                            slotMapList += "0";
                        }
                        if (CB_SLOT24.IsChecked.Value)
                        {
                            slotList.Add(24);
                            slotMapList += "1";
                        }
                        else
                        {
                            slotMapList += "0";
                        }
                        if (CB_SLOT25.IsChecked.Value)
                        {
                            slotList.Add(25);
                            slotMapList += "1";
                        }
                        else
                        {
                            slotMapList += "0";
                        }

                        

                        //DownloadReqData.ActionType = EnumRemoteCommand.DOWNLOAD_STAGE_RECIPE;
                        DownloadReqData.LotID = activeData.LotID;
                        DownloadReqData.FoupNumber = activeData.FoupNumber;

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            //if (DownloadReqData.Count > 0)
                            if (DownloadReqData.RecipeDic.Count > 0)
                            {
                                GemModule.ResetStageDownloadRecipe();
                                var ret = Master.DeviceManager().SetRecipeToStage(DownloadReqData);
                                Master.SetRecipeToDevice(DownloadReqData);
                            }
                        });

                        Task task = new Task(() =>
                        {
                            //if (DownloadReqData.Count > 0)
                            if (DownloadReqData.RecipeDic.Count > 0)
                            {
                                while (isWaitDownloadRecipe == true)
                                {
                                    if (GemModule.GetComplateDownloadRecipe(activeData.UseStageNumbers))
                                    {
                                        isWaitDownloadRecipe = false;
                                    }
                                }
                            }
                        });
                        task.Start();
                        await task;

                        //Master.DownloadRecipeToStage(activeData, SelectedDevice);
                        if (DownloadReqData.RecipeDic.Count != 0 & activeData.UseStageNumbers.Count != 0)
                        {
                            string recipe = "";
                            DownloadReqData.RecipeDic.TryGetValue(activeData.UseStageNumbers[0], out recipe);
                            //Master.SetDeviceName(FoupNumber, recipe);
                        }
                        else
                        {
                            //Master.SetDeviceName(FoupNumber, LotID);
                        }

                        if (foupNum == 1)
                        {
                            if (rb_nonePMI1.IsChecked == true)
                            {
                                Master.ActiveLotInfos[foupNum - 1].PMIEveryInterval = 0;
                                Master.ActiveLotInfos[foupNum - 1].DoPMICount = 0;
                            }
                            else if (rb_everyPMI1.IsChecked == true)
                            {
                                int count = 0;
                                if (Int32.TryParse(tb_PMI1COUNT.Text, out count))
                                {
                                    Master.ActiveLotInfos[foupNum - 1].PMIEveryInterval = count;
                                }
                            }
                            else if (rb_totalPMI1.IsChecked == true)
                            {
                                int count = 0;
                                if (Int32.TryParse(tb_PMI1COUNT.Text, out count))
                                {
                                    Master.ActiveLotInfos[foupNum - 1].DoPMICount = count;
                                }
                            }
                        }
                        else if (foupNum == 2)
                        {
                            if (rb_nonePMI2.IsChecked == true)
                            {
                                Master.ActiveLotInfos[foupNum - 1].PMIEveryInterval = 0;
                                Master.ActiveLotInfos[foupNum - 1].DoPMICount = 0;
                            }
                            else if (rb_everyPMI2.IsChecked == true)
                            {
                                int count = 0;
                                if (Int32.TryParse(tb_PMI2COUNT.Text, out count))
                                {
                                    Master.ActiveLotInfos[foupNum - 1].PMIEveryInterval = count;
                                }
                            }
                            else if (rb_totalPMI2.IsChecked == true)
                            {
                                int count = 0;
                                if (Int32.TryParse(tb_PMI2COUNT.Text, out count))
                                {
                                    Master.ActiveLotInfos[foupNum - 1].DoPMICount = count;
                                }
                            }
                        }
                        else if (foupNum == 3)
                        {
                            if (rb_nonePMI3.IsChecked == true)
                            {
                                Master.ActiveLotInfos[foupNum - 1].PMIEveryInterval = 0;
                                Master.ActiveLotInfos[foupNum - 1].DoPMICount = 0;
                            }
                            else if (rb_everyPMI3.IsChecked == true)
                            {
                                int count = 0;
                                if (Int32.TryParse(tb_PMI3COUNT.Text, out count))
                                {
                                    Master.ActiveLotInfos[foupNum - 1].PMIEveryInterval = count;
                                }
                            }
                            else if (rb_totalPMI3.IsChecked == true)
                            {
                                int count = 0;
                                if (Int32.TryParse(tb_PMI3COUNT.Text, out count))
                                {
                                    Master.ActiveLotInfos[foupNum - 1].DoPMICount = count;
                                }
                            }
                        }
                        else if (foupNum == 4)
                        {
                            if (rb_nonePMI4.IsChecked == true)
                            {
                                Master.ActiveLotInfos[foupNum - 1].PMIEveryInterval = 0;
                                Master.ActiveLotInfos[foupNum - 1].DoPMICount = 0;
                            }
                            else if (rb_everyPMI4.IsChecked == true)
                            {
                                int count = 0;
                                if (Int32.TryParse(tb_PMI4COUNT.Text, out count))
                                {
                                    Master.ActiveLotInfos[foupNum - 1].PMIEveryInterval = count;
                                }
                            }
                            else if (rb_totalPMI4.IsChecked == true)
                            {
                                int count = 0;
                                if (Int32.TryParse(tb_PMI4COUNT.Text, out count))
                                {
                                    Master.ActiveLotInfos[foupNum - 1].DoPMICount = count;
                                }
                            }
                        }

                        if (GemModule.GemSysParam.ReceiveMessageType.Equals("SemicsGemReceiverSEKR"))
                        {
                            //Master.ActiveProcess(activeData);
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
                            dockFoupActReqData.FoupNumber = foupNum;

                            SelectSlotsActReqData selectSlotsActReqData = new SelectSlotsActReqData();
                            selectSlotsActReqData.LotID = LotID;
                            selectSlotsActReqData.ActionType = EnumRemoteCommand.SELECT_SLOTS;
                            selectSlotsActReqData.FoupNumber = foupNum;
                            selectSlotsActReqData.UseSlotNumbers = slotList;

                            StartLotActReqData startLotActReqData = new StartLotActReqData();
                            startLotActReqData.LotID = LotID;
                            startLotActReqData.FoupNumber = foupNum;
                            startLotActReqData.ActionType = EnumRemoteCommand.START_LOT;

                            task = new Task(() =>
                            {
                                GemModule.GemCommManager.OnRemoteCommandAction(activelotreqdata);
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
                            proceedWithCarrierReqData.CarrierID = tb_probecard_id.Text;
                            proceedWithCarrierReqData.ActionType = EnumCarrierAction.PROCEEDWITHCARRIER;

                            ProceedWithCarrierReqData proceedWithCarrierReqData2 = new ProceedWithCarrierReqData();
                            proceedWithCarrierReqData2.DataID = 1;
                            proceedWithCarrierReqData2.PTN = activeData.FoupNumber;
                            proceedWithCarrierReqData2.CarrierID = tb_probecard_id.Text;
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
                            proceedWithCellSlotActReqData.CarrierID = tb_probecard_id.Text;
                            proceedWithCellSlotActReqData.ActionType = EnumCarrierAction.PROCESSEDWITHCELLSLOT;


                            StartLotActReqData startLotActReqData = new StartLotActReqData();
                            startLotActReqData.FoupNumber = activeData.FoupNumber;
                            //startLotActReqData.OCRReadFalg = 0;
                            startLotActReqData.LotID = activeData.LotID;
                            startLotActReqData.ActionType = EnumRemoteCommand.PSTART;
                            task = new Task(() =>
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

                        }
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private async void Btn_LOT1Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show($"Do you Want LOT 1 Start? ", "LOT1 Start Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if (CB_CST1.IsChecked.Value || CB_CST2.IsChecked.Value || CB_CST3.IsChecked.Value || CB_CST4.IsChecked.Value)
                    {
                        List<int> foupNums = new List<int>();
                        if (CB_CST1.IsChecked.Value)
                        {
                            foupNums.Add(1);
                        }
                        if (CB_CST2.IsChecked.Value)
                        {
                            foupNums.Add(2);
                        }
                        if (CB_CST3.IsChecked.Value)
                        {
                            foupNums.Add(3);
                        }
                        if (CB_CST4.IsChecked.Value)
                        {
                            foupNums.Add(4);
                        }
                        await MultiLotStart(foupNums);
                    }
                    else
                    {
                        await LotStart(1);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private async void Btn_LOT2Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show($"Do you Want LOT 2 Start? ", "LOT2 Start Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    await LotStart(2);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async void Btn_LOT3Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show($"Do you Want LOT 3 Start? ", "LOT3 Start Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    await LotStart(3);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async void Btn_LOT4Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show($"Do you Want LOT 4 Start? ", "LOT4 Start Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    await LotStart(4);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void Btn_LOT1Cancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Master.CarrierCancel(1);
                LoggerManager.Debug($"DemoRunView Click CanCelCarrier Foup #1");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void Btn_LOT2Cancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Master.CarrierCancel(2);
                LoggerManager.Debug($"DemoRunView Click CanCelCarrier Foup #2");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void Btn_LOT3Cancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Master.CarrierCancel(3);
                LoggerManager.Debug($"DemoRunView Click CanCelCarrier Foup #3");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void Btn_LOT4Cancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Master.CarrierCancel(4);
                LoggerManager.Debug($"DemoRunView Click CanCelCarrier Foup #4");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void InitDevices()
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
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitDevices();
        }

        private void Loadport1_placed_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GemModule.GetPIVContainer().SetFoupInfo(1, null, 0, null, null);
                if (tb_probecard_id.Text != "")
                {
                    var carrierid = tb_probecard_id.Text + "1";
                    Master.FoupCarrierIdChanged(1, carrierid);
                    GemModule.GetPIVContainer().SetCarrierId(1, carrierid);
                    GemModule.GetPIVContainer().UpdateFoupInfo(2);

                    PIVInfo pIV = new PIVInfo(foupnumber: 1, carrierid: carrierid);
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    GemModule.EventManager().RaisingEvent(typeof(CassetteIDReadDoneEvent).FullName, new ProbeEventArgs(this, semaphore, pIV));
                    semaphore.Wait();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void Loadport2_placed_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GemModule.GetPIVContainer().SetFoupInfo(2, null, 0, null, null);
                if (tb_probecard_id.Text != "")
                {
                    var carrierid = tb_probecard_id.Text + "2";
                    Master.FoupCarrierIdChanged(2, carrierid);
                    GemModule.GetPIVContainer().SetCarrierId(2, carrierid);
                    GemModule.GetPIVContainer().UpdateFoupInfo(2);

                    PIVInfo pIV = new PIVInfo(foupnumber: 2, carrierid: carrierid);
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    GemModule.EventManager().RaisingEvent(typeof(CassetteIDReadDoneEvent).FullName, new ProbeEventArgs(this, semaphore, pIV));
                    semaphore.Wait();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void Loadport3_placed_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GemModule.GetPIVContainer().SetFoupInfo(3, null, 0, null, null);
                if (tb_probecard_id.Text != "")
                {
                    var carrierid = tb_probecard_id.Text + "3";
                    Master.FoupCarrierIdChanged(3, carrierid);
                    GemModule.GetPIVContainer().SetCarrierId(3, tb_probecard_id.Text);
                    GemModule.GetPIVContainer().UpdateFoupInfo(3);

                    PIVInfo pivinfo = new PIVInfo(foupnumber: 3, carrierid: carrierid);
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    GemModule.EventManager().RaisingEvent(typeof(CassetteIDReadDoneEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    semaphore.Wait();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private void Loadport4_placed_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GemModule.GetPIVContainer().SetFoupInfo(4, null, 0, null, null);
                if (tb_probecard_id.Text != "")
                {
                    var carrierid = tb_probecard_id.Text + "4";
                    Master.FoupCarrierIdChanged(4, carrierid);
                    GemModule.GetPIVContainer().SetCarrierId(4, tb_probecard_id.Text);
                    GemModule.GetPIVContainer().UpdateFoupInfo(4);

                    PIVInfo pivinfo = new PIVInfo(foupnumber: 4, carrierid: carrierid);
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    GemModule.EventManager().RaisingEvent(typeof(CassetteIDReadDoneEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    semaphore.Wait();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void CB_SlotAll_Checked(object sender, RoutedEventArgs e)
        {
            CB_SLOT1.IsChecked = true;
            CB_SLOT2.IsChecked = true;
            CB_SLOT3.IsChecked = true;
            CB_SLOT4.IsChecked = true;
            CB_SLOT5.IsChecked = true;
            CB_SLOT6.IsChecked = true;
            CB_SLOT7.IsChecked = true;
            CB_SLOT8.IsChecked = true;
            CB_SLOT9.IsChecked = true;
            CB_SLOT10.IsChecked = true;
            CB_SLOT11.IsChecked = true;
            CB_SLOT12.IsChecked = true;
            CB_SLOT13.IsChecked = true;
            CB_SLOT14.IsChecked = true;
            CB_SLOT15.IsChecked = true;
            CB_SLOT16.IsChecked = true;
            CB_SLOT17.IsChecked = true;
            CB_SLOT18.IsChecked = true;
            CB_SLOT19.IsChecked = true;
            CB_SLOT20.IsChecked = true;
            CB_SLOT21.IsChecked = true;
            CB_SLOT22.IsChecked = true;
            CB_SLOT23.IsChecked = true;
            CB_SLOT24.IsChecked = true;
            CB_SLOT25.IsChecked = true;
        }

        private void CB_SlotAll_Unchecked(object sender, RoutedEventArgs e)
        {
            CB_SLOT1.IsChecked = false;
            CB_SLOT2.IsChecked = false;
            CB_SLOT3.IsChecked = false;
            CB_SLOT4.IsChecked = false;
            CB_SLOT5.IsChecked = false;
            CB_SLOT6.IsChecked = false;
            CB_SLOT7.IsChecked = false;
            CB_SLOT8.IsChecked = false;
            CB_SLOT9.IsChecked = false;
            CB_SLOT10.IsChecked = false;
            CB_SLOT11.IsChecked = false;
            CB_SLOT12.IsChecked = false;
            CB_SLOT13.IsChecked = false;
            CB_SLOT14.IsChecked = false;
            CB_SLOT15.IsChecked = false;
            CB_SLOT16.IsChecked = false;
            CB_SLOT17.IsChecked = false;
            CB_SLOT18.IsChecked = false;
            CB_SLOT19.IsChecked = false;
            CB_SLOT20.IsChecked = false;
            CB_SLOT21.IsChecked = false;
            CB_SLOT22.IsChecked = false;
            CB_SLOT23.IsChecked = false;
            CB_SLOT24.IsChecked = false;
            CB_SLOT25.IsChecked = false;

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

        private Array _ForceStateArray = Enum.GetValues(typeof(EnumModuleForcedState));
        public Array ForceStateArray
        {
            get { return _ForceStateArray; }
            set
            {
                if (value != _ForceStateArray)
                {
                    _ForceStateArray = value;
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

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            //if (CB_Cell1.IsChecked.Value)
            //{
            //    var client = Master.GetClient(1);
            //    client?.SetStilProbingZUp((bool)ProbingZUPToggle.IsChecked);
            //}
            //if (CB_Cell2.IsChecked.Value)
            //{
            //    var client = Master.GetClient(2);
            //    client?.SetStilProbingZUp((bool)ProbingZUPToggle.IsChecked);
            //}
            //if (CB_Cell3.IsChecked.Value)
            //{
            //    var client = Master.GetClient(3);
            //    client?.SetStilProbingZUp((bool)ProbingZUPToggle.IsChecked);
            //}
            //if (CB_Cell4.IsChecked.Value)
            //{
            //    var client = Master.GetClient(4);
            //    client?.SetStilProbingZUp((bool)ProbingZUPToggle.IsChecked);
            //}
            //if (CB_Cell5.IsChecked.Value)
            //{
            //    var client = Master.GetClient(5);
            //    client?.SetStilProbingZUp((bool)ProbingZUPToggle.IsChecked);
            //}
            //if (CB_Cell6.IsChecked.Value)
            //{
            //    var client = Master.GetClient(6);
            //    client?.SetStilProbingZUp((bool)ProbingZUPToggle.IsChecked);
            //}
            //if (CB_Cell7.IsChecked.Value)
            //{
            //    var client = Master.GetClient(7);
            //    client?.SetStilProbingZUp((bool)ProbingZUPToggle.IsChecked);
            //}
            //if (CB_Cell8.IsChecked.Value)
            //{
            //    var client = Master.GetClient(8);
            //    client?.SetStilProbingZUp((bool)ProbingZUPToggle.IsChecked);
            //}
            //if (CB_Cell9.IsChecked.Value)
            //{
            //    var client = Master.GetClient(9);
            //    client?.SetStilProbingZUp((bool)ProbingZUPToggle.IsChecked);
            //}
            //if (CB_Cell10.IsChecked.Value)
            //{
            //    var client = Master.GetClient(10);
            //    client?.SetStilProbingZUp((bool)ProbingZUPToggle.IsChecked);
            //}
            //if (CB_Cell11.IsChecked.Value)
            //{
            //    var client = Master.GetClient(11);
            //    client?.SetStilProbingZUp((bool)ProbingZUPToggle.IsChecked);
            //}
            //if (CB_Cell12.IsChecked.Value)
            //{
            //    var client = Master.GetClient(12);
            //    client?.SetStilProbingZUp((bool)ProbingZUPToggle.IsChecked);
            //}
        }

        private void ProbingZUPToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            //if (CB_Cell1.IsChecked.Value)
            //{
            //    var client = Master.GetClient(1);
            //    client?.SetStilProbingZUp((bool)ProbingZUPToggle.IsChecked);
            //}
            //if (CB_Cell2.IsChecked.Value)
            //{
            //    var client = Master.GetClient(2);
            //    client?.SetStilProbingZUp((bool)ProbingZUPToggle.IsChecked);
            //}
            //if (CB_Cell3.IsChecked.Value)
            //{
            //    var client = Master.GetClient(3);
            //    client?.SetStilProbingZUp((bool)ProbingZUPToggle.IsChecked);
            //}
            //if (CB_Cell4.IsChecked.Value)
            //{
            //    var client = Master.GetClient(4);
            //    client?.SetStilProbingZUp((bool)ProbingZUPToggle.IsChecked);
            //}
            //if (CB_Cell5.IsChecked.Value)
            //{
            //    var client = Master.GetClient(5);
            //    client?.SetStilProbingZUp((bool)ProbingZUPToggle.IsChecked);
            //}
            //if (CB_Cell6.IsChecked.Value)
            //{
            //    var client = Master.GetClient(6);
            //    client?.SetStilProbingZUp((bool)ProbingZUPToggle.IsChecked);
            //}
            //if (CB_Cell7.IsChecked.Value)
            //{
            //    var client = Master.GetClient(7);
            //    client?.SetStilProbingZUp((bool)ProbingZUPToggle.IsChecked);
            //}
            //if (CB_Cell8.IsChecked.Value)
            //{
            //    var client = Master.GetClient(8);
            //    client?.SetStilProbingZUp((bool)ProbingZUPToggle.IsChecked);
            //}
            //if (CB_Cell9.IsChecked.Value)
            //{
            //    var client = Master.GetClient(9);
            //    client?.SetStilProbingZUp((bool)ProbingZUPToggle.IsChecked);
            //}
            //if (CB_Cell10.IsChecked.Value)
            //{
            //    var client = Master.GetClient(10);
            //    client?.SetStilProbingZUp((bool)ProbingZUPToggle.IsChecked);
            //}
            //if (CB_Cell11.IsChecked.Value)
            //{
            //    var client = Master.GetClient(11);
            //    client?.SetStilProbingZUp((bool)ProbingZUPToggle.IsChecked);
            //}
            //if (CB_Cell12.IsChecked.Value)
            //{
            //    var client = Master.GetClient(12);
            //    client?.SetStilProbingZUp((bool)ProbingZUPToggle.IsChecked);
            //}
        }

        private void Btn_Set_recipe_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Cells.ToList().ForEach(x =>
                {
                    if (x.IsChecked)
                    {
                        if (DownloadReqData.RecipeDic.ContainsKey(x.Index))
                        {
                            DownloadReqData.RecipeDic.Remove(x.Index);
                        }
                        DownloadReqData.RecipeDic.Add(x.Index, SelectedDevice);
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void btn_UseFTP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DownloadReqData.UseFTP = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void btn_DontUseFTP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DownloadReqData.UseFTP = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void Btn_download_recipe_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GemModule.GemSysParam.ReceiveMessageType.Equals("SemicsGemReceiverSEKX"))
                {
                    DownloadReqData.ActionType = EnumRemoteCommand.DLRECIPE;
                }
                else
                {
                    DownloadReqData.ActionType = EnumRemoteCommand.DOWNLOAD_STAGE_RECIPE;
                }

                if (tb_LOT1ID.Text != "")
                {
                    DownloadReqData.LotID = tb_LOT1ID.Text;
                    DownloadReqData.FoupNumber = 1;
                }
                if (tb_LOT2ID.Text != "")
                {
                    DownloadReqData.LotID = tb_LOT2ID.Text;
                    DownloadReqData.FoupNumber = 2;
                }
                if (tb_LOT3ID.Text != "")
                {
                    DownloadReqData.LotID = tb_LOT3ID.Text;
                    DownloadReqData.FoupNumber = 3;
                }
                if (tb_LOT4ID.Text != "")
                {
                    DownloadReqData.LotID = tb_LOT4ID.Text;
                    DownloadReqData.FoupNumber = 4;
                }


                Application.Current.Dispatcher.Invoke(() =>
                {
                    var ret = Master.DeviceManager().SetRecipeToStage(DownloadReqData);
                    Master.SetRecipeToDevice(DownloadReqData);
                    GemModule.ResetStageDownloadRecipe();

                });

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private void Btn_reset_checked_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Cells.ToList().ForEach(x =>
                {
                    x.IsChecked = false;
                });

                DownloadReqData.RecipeDic.Clear();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void Btn_IsExternalLotMode_Emul_Click(object sender, RoutedEventArgs e)
        {
            //mModule.IsExternalLotMode = true;
            GemModule.IsExternalLotMode = false;
            //GemModule.AutoZupDownByGemEventEnable = false;
        }

        private void Btn_loadingcheckMode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Cells.ToList().ForEach(x =>
                {
                    if (x.IsChecked)
                    {
                        var client = Master.GetClient(x.Index);
                        client?.SetLotLoadingPosCheckMode(true);
                    }
                    x.IsChecked = false;
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void Btn_loadingcheckfree_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Cells.ToList().ForEach(x =>
                {
                    if (x.IsChecked)
                    {
                        var client = Master.GetClient(x.Index);
                        client?.SetLotLoadingPosCheckMode(false);
                    }
                    x.IsChecked = false;
                });


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void btn_cellLotEnd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Cells.ToList().ForEach(x =>
                {
                    if (x.IsChecked)
                    {
                        var client = Master.GetClient(x.Index);
                        client?.LotOPEnd();
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void btn_clear_recipe_Click(object sender, RoutedEventArgs e)
        {
            DownloadReqData.RecipeDic.Clear();
        }

        private void btn_ErrorEnd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Cells.ToList().ForEach(x =>
                {
                    if (x.IsChecked)
                    {
                        var client = Master.GetClient(x.Index);
                        client?.ReserveErrorEnd();
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void btn_StageUseDownLoadReciep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Cells.ToList().ForEach(x =>
                {
                    if (x.IsChecked)
                    {
                        var client = Master.GetClient(x.Index);
                        client?.SetRecipeDownloadEnable(true);
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void btn_StageNotUseDownLoadReciep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Cells.ToList().ForEach(x =>
                {
                    if (x.IsChecked)
                    {
                        var client = Master.GetClient(x.Index);
                        client?.SetRecipeDownloadEnable(false);
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void btn_abortDownloadRecipe_Click(object sender, RoutedEventArgs e)
        {
            isWaitDownloadRecipe = false;
        }

        private void btn_cellend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorEndData errorEndData = new ErrorEndData();
                errorEndData.ActionType = EnumRemoteCommand.ERROR_END;

                Cells.ToList().ForEach(x =>
                {
                    if (x.IsChecked)
                    {
                        errorEndData.StageNumber = x.Index;
                    }
                });

                GemModule.GemCommManager.OnRemoteCommandAction(errorEndData);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void Btn_LOT1Suspend_Click(object sender, RoutedEventArgs e)
        {
            this.Master.LotSuspend(1);
            //
        }

        private void Btn_LOT2Suspend_Click(object sender, RoutedEventArgs e)
        {
            this.Master.LotSuspend(2);
        }

        private void Btn_LOT3Suspend_Click(object sender, RoutedEventArgs e)
        {
            this.Master.LotSuspend(3);
        }
        private void Btn_LOT4Suspend_Click(object sender, RoutedEventArgs e)
        {
            this.Master.LotSuspend(4);
        }

        private void SetDevice_Click(object sender, RoutedEventArgs e)
        {
            if (CB_CST1.IsChecked.Value)
            {
            }
            if (CB_CST2.IsChecked.Value)
            {
            }
            if (CB_CST3.IsChecked.Value)
            {
            }
            if (CB_CST4.IsChecked.Value)
            {
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

        private void LOT4_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.Master.ActiveLotInfos[3].IsActiveFromHost
                    = !this.Master.ActiveLotInfos[3].IsActiveFromHost;
                LoggerManager.Debug($"Foup 4's IsActiveFormHost change to { this.Master.ActiveLotInfos[2].IsActiveFromHost.ToString()}");
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

        private void loadport1_removed_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PIVInfo pivinfo = new PIVInfo(foupnumber: 1);

                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                GemModule.EventManager().RaisingEvent(typeof(CarrierRemovedEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                semaphore.Wait();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void loadport2_removed_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PIVInfo pivinfo = new PIVInfo(foupnumber: 2);

                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                GemModule.EventManager().RaisingEvent(typeof(CarrierRemovedEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                semaphore.Wait();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void loadport3_removed_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PIVInfo pivinfo = new PIVInfo(foupnumber: 3);

                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                GemModule.EventManager().RaisingEvent(typeof(CarrierRemovedEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                semaphore.Wait();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void loadport4_removed_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PIVInfo pivinfo = new PIVInfo(foupnumber: 4);

                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                GemModule.EventManager().RaisingEvent(typeof(CarrierRemovedEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                semaphore.Wait();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void TC_EndBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StageActReqData tcEndData = new StageActReqData();
                tcEndData.ActionType = EnumRemoteCommand.TC_END;

                Cells.ToList().ForEach(x =>
                {
                    if (x.IsChecked)
                    {
                        tcEndData.StageNumber = x.Index;
                    }
                });

                GemModule.GemCommManager.OnRemoteCommandAction(tcEndData);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class CellGridItem : INotifyPropertyChanged
    {
        /// <summary>
        /// 1부터 시작
        /// </summary>
        private int _Index;
        public int Index
        {
            get { return _Index; }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Name = "Cell";
        public string Name
        {
            get { return $"Cell{Index}"; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsChecked = false;
        public bool IsChecked
        {
            get { return _IsChecked; }
            set
            {
                if (value != _IsChecked)
                {
                    _IsChecked = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _ForcedDoneLabelVisibility = Visibility.Hidden;
        public Visibility ForcedDoneLabelVisibility
        {
            get { return _ForcedDoneLabelVisibility; }
            set
            {
                if (value != _ForcedDoneLabelVisibility)
                {
                    _ForcedDoneLabelVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<ModuleEnum> _ForcedDoneModule = new List<ModuleEnum>();
        public List<ModuleEnum> ForcedDoneModule
        {
            get { return _ForcedDoneModule; }
            set
            {
                if (value != _ForcedDoneModule)
                {
                    _ForcedDoneModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        public CellGridItem(int index)
        {
            Index = index;
        }

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
