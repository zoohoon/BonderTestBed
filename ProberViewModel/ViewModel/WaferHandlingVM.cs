using System;
using System.Linq;
using System.Threading.Tasks;

namespace WaferHandlingViewModel
{
    using LoaderControllerBase;
    using LoaderParameters;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Command;
    using ProberInterfaces.Command.Internal;
    using ProberInterfaces.Foup;
    using RelayCommandBase;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using System.Windows.Media.Media3D;
    using VirtualKeyboardControl;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using MetroDialogInterfaces;
    using ProberInterfaces.State;

    public class WaferHandlingVM : IMainScreenViewModel, ISetUpState
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        readonly Guid _ViewModelGUID = new Guid("030B1CFA-A617-404A-9C44-031754F15F7E");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        public bool Initialized { get; set; } = false;
        public int CassetteWaferHandleIconIdx { get; set; }

        public ILoaderControllerExtension LoaderController { get; set; }
        public ICommandManager CommandManager { get; set; }

        public IViewModelManager ViewModelManager { get; set; }
        public int ViewNUM { get; set; }
        public bool ViewNUMx2 { get; set; }
        private ModuleDataInfoBase _SrcHolderDataInfo;
        private ModuleDataInfoBase _DstHolderDataInfo;

        TranslateRecipe _CurSelectedRecipe;
        private TranslateRecipe CurSelectedRecipe
        {
            get
            {
                return _CurSelectedRecipe;
            }
            set
            {
                _CurSelectedRecipe = value;

                if (_CurSelectedRecipe == null)
                {
                    TransferButtonEnable = false;
                }
                else
                {
                    TransferButtonEnable = true;
                }
            }
        }

        #region ==> TransferButtonEnable
        private bool _TransferButtonEnable;
        public bool TransferButtonEnable
        {
            get { return _TransferButtonEnable; }
            set
            {
                if (value != _TransferButtonEnable)
                {
                    _TransferButtonEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> Stage3DModel
        private IStage3DModel _Stage3DModel;
        public IStage3DModel Stage3DModel
        {
            get { return _Stage3DModel; }
            set
            {
                if (value != _Stage3DModel)
                {
                    _Stage3DModel = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> WaferTransferCommand
        private AsyncCommand _WaferTransferCommand;
        public ICommand WaferTransferCommand
        {
            get
            {
                if (null == _WaferTransferCommand) _WaferTransferCommand = new AsyncCommand(WaferTransferCommandFunc);
                return _WaferTransferCommand;
            }
        }

        private async Task WaferTransferCommandFunc()
        {
            try
            {
                //    
                bool isPreScanRead = false;
                if (CurSelectedRecipe == null)
                    return;

                CassetteModuleInfo cassette = LoaderController.LoaderInfo.StateMap.CassetteModules.FirstOrDefault();
                //if (cassette.ScanState != CassetteScanStateEnum.READ)
                //    return;
                //  this.ViewModelManager().Lock(this.GetHashCode(), "WaferTransfer", "wait");
                //LoaderController.LoaderInfo.StateMap.HolderModulesWithoutSlot
                if (LoaderController.LoaderInfo.StateMap.CassetteModules[0].ScanState != CassetteScanStateEnum.READ)
                {
                    isPreScanRead = true;

                    await DoScanCST();

                    //while (LoaderController.LoaderInfo.StateMap.CassetteModules[0].ScanState != CassetteScanStateEnum.READ && this.LoaderController().ModuleState.GetState() != ModuleStateEnum.IDLE)
                    //{
                    //    _delays.DelayFor(10);
                    //}

                    //if(LoaderController.LoaderInfo.StateMap.CassetteModules[0].ScanState == CassetteScanStateEnum.ILLEGAL)
                    //{
                    //    return;
                    //}
                    //WaferTransferCommandFunc();
                }
                //=> find source
                TransferObject targetSub = CurSelectedRecipe.SrcHolderDataInfo.GetSelectedSubstrate();
                //=> find dest pos
                ModuleID destPos = CurSelectedRecipe.DstHolderDataInfo.GetSelectedModuleID();

                if (isPreScanRead && targetSub != null && destPos != null)
                {

                    var targetSlot = LoaderController.LoaderInfo.StateMap.CassetteModules[0].SlotModules.Where(slotID => slotID.ID == targetSub.CurrPos).FirstOrDefault();
                    var destSlot = LoaderController.LoaderInfo.StateMap.CassetteModules[0].SlotModules.Where(slotID => slotID.ID == destPos).FirstOrDefault();
                    if (targetSlot != null)
                    {
                        if (targetSlot.WaferStatus != EnumSubsStatus.EXIST)
                        {
                            targetSub = null;
                            destPos.ModuleType = ModuleTypeEnum.UNDEFINED;
                            string caption = "WARNING";
                            string message = "There is No Wafer in  SLOT" + targetSlot.ID.Index;

                            var dlgRel = await this.MetroDialogManager().ShowMessageDialog(caption, message, EnumMessageStyle.Affirmative);
                        }
                        else
                        {
                            targetSub = targetSlot.Substrate;
                        }
                    }
                    if (destSlot != null && destSlot.WaferStatus == EnumSubsStatus.EXIST)
                    {

                        targetSub = null;
                        destPos.ModuleType = ModuleTypeEnum.UNDEFINED;
                        string caption = "WARNING";
                        string message = "There is Wafer in SLOT" + destSlot.ID.Index;

                        var dlgRel = await this.MetroDialogManager().ShowMessageDialog(caption, message, EnumMessageStyle.Affirmative);
                    }
                    await System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        CleanScreen();
                        UpdateScreen();
                    }));
                }
                //=> Req to loader

                if (targetSub != null && destPos != ModuleID.UNDEFINED)
                {
                    if ((targetSub.CurrHolder.ModuleType == destPos.ModuleType) && destPos.ModuleType == ModuleTypeEnum.ARM)
                    {
                        string caption = "WARNING";
                        string message = "This is a Non-Movable Location.";

                        var dlgRel = await this.MetroDialogManager().ShowMessageDialog(caption, message, EnumMessageStyle.Affirmative);

                        await System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            CleanScreen();
                            UpdateScreen();
                        }));
                    }
                    else if (this.LoaderController().ModuleState.GetState() == ModuleStateEnum.IDLE)
                    {
                        if (targetSub.Type.Value == SubstrateTypeEnum.UNDEFINED ||
                            targetSub.Size.Value == SubstrateSizeEnum.UNDEFINED)
                        {
                            if (destPos.ModuleType == ModuleTypeEnum.SLOT ||
                                destPos.ModuleType == ModuleTypeEnum.FIXEDTRAY ||
                                destPos.ModuleType == ModuleTypeEnum.INSPECTIONTRAY)
                            {
                                string caption = "WARNING";
                                string message = "Wafer type and size are undefined. Do you want to transfer the wafer using the desinitation information?";

                                var dlgRel = await this.MetroDialogManager().ShowMessageDialog(caption, message, EnumMessageStyle.AffirmativeAndNegative);

                                if (dlgRel == EnumMessageDialogResult.AFFIRMATIVE)
                                {

                                }
                                else
                                {
                                    return;
                                }

                            }
                            else
                            {
                                string caption = "WARNING";
                                string message = "Wafer type and size are undefined. Select the source module as the destination. ";

                                var dlgRel = await this.MetroDialogManager().ShowMessageDialog(caption, message, EnumMessageStyle.Affirmative);

                                return;

                            }

                        }

                        LoaderMapEditor editor = LoaderController.GetLoaderMapEditor();
                        editor.EditorState.SetTransfer(targetSub.ID.Value, destPos);

                        LoaderMapCommandParameter cmdParam = new LoaderMapCommandParameter();
                        cmdParam.Editor = editor;
                        
                        bool isInjected = CommandManager.SetCommand<ILoaderMapCommand>(this, cmdParam);

                        if (isInjected)
                        {
                            await Task.Run(() =>
                            {
                                this.LoaderController().WaitForCommandDone();
                            });
                        }

                        await System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            CleanScreen();
                            UpdateScreen();
                        }));

                        //this.ViewModelManager().UnLock(this.GetHashCode());
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        private AsyncCommand _TestScanCSTCommand;
        public ICommand TestScanCSTCommand
        {
            get
            {
                if (null == _TestScanCSTCommand) _TestScanCSTCommand = new AsyncCommand(TestScanCSTCommandFunc);
                return _TestScanCSTCommand;
            }
        }

        private async Task TestScanCSTCommandFunc()
        {
            try
            {
                while (true)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        //
                        await System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            CleanScreen();
                        }));
                        await DoScanCST();
                    }
                    await this.StageSupervisor().SystemInit();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region ==> ScanCSTCommand
        private AsyncCommand _ScanCSTCommand;
        public ICommand ScanCSTCommand
        {
            get
            {
                if (null == _ScanCSTCommand) _ScanCSTCommand = new AsyncCommand(ScanCSTCommandFunc);
                return _ScanCSTCommand;
            }
        }
        private async Task DoScanCST()
        {
            IORet IOretVal = IORet.ERROR;
            bool waferoutValue = false;
            try
            {
                IOretVal = (IORet)this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIWAFERSENSOR, true, 500,1000);
                if(IOretVal != IORet.NO_ERR)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Check Cassette", "Wafer out sensor is detected.", EnumMessageStyle.Affirmative);

                    return;
                }
                CassetteModuleInfo cassette = LoaderController.LoaderInfo.StateMap.CassetteModules.FirstOrDefault();

                var foupController = this.FoupOpModule().GetFoupController(cassette.ID.Index);
                
                if (foupController.FoupModuleInfo.State == FoupStateEnum.UNLOAD)
                {
                    EventCodeEnum retVal = foupController.Execute(new FoupLoadCommand() { });
                }

                //foupController.FoupModuleInfo.State = FoupStateEnum.LOAD;

                if (foupController.FoupModuleInfo.State != FoupStateEnum.LOAD)
                {
                    string caption = "ERROR";
                    string message = $"Cassette not loaded properly." +"\n" +
                        "Please check if the cassette is loaded normally.";

                    var dlgRel = await this.MetroDialogManager().ShowMessageDialog(caption, message, EnumMessageStyle.Affirmative);

                    //if (dlgRel == EnumMessageDialogResult.FirstAuxiliary)
                    //{
                    //    await this.ViewModelManager().ViewTransitionAsync(new Guid("e89d213f-abed-4962-b410-71ae4f0cdf53"));
                    //}
                    return;
                }

                if (cassette != null)
                {
                    if (this.LoaderController().ModuleState.GetState() == ModuleStateEnum.IDLE)
                    {
                        var editor = LoaderController.GetLoaderMapEditor();
                        editor.EditorState.SetScanCassette(cassette.ID);

                        LoaderMapCommandParameter cmdParam = new LoaderMapCommandParameter();
                        cmdParam.Editor = editor;
                        bool isInjected = CommandManager.SetCommand<ILoaderMapCommand>(this, cmdParam);

                        if (isInjected)
                        {
                            await Task.Run(() =>
                            {
                                this.LoaderController().WaitForCommandDone();

                                // 정상 종료 : 스캔 성공
                                if ((LoaderController.LoaderInfo.StateMap.CassetteModules[0].ScanState == CassetteScanStateEnum.READ) &&
                                     (this.LoaderController().ModuleState.GetState() == ModuleStateEnum.IDLE))
                                {
                                    return;
                                }
                                else
                                {
                                    // TODO : 실패 메시지 다이얼로그 띄어줘야 함.
                                    this.MetroDialogManager().ShowMessageDialog("ERROR", "Scan cassette is failed.", EnumMessageStyle.Affirmative);
                                    return;
                                }
                            });
                        }
                        //if (isInjected)
                        //{
                        //    await ProcessDialog.ShowDialog("LOADER", $"Scan Cassette : target={cassette.ID}");

                        //    await Task.Run(() =>
                        //    {
                        //        EventCodeEnum retVal;
                        //        retVal = LoaderController.WaitForCommandDone();
                        //    });

                        //    await ProcessDialog.CloseDialg();
                        //}
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private async Task ScanCSTCommandFunc()
        {
            try
            {
                //
                await System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    CleanScreen();
                }));
                await DoScanCST();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> LoadCSTCommand
        private AsyncCommand _LoadCSTCommand;
        public ICommand LoadCSTCommand
        {
            get
            {
                if (null == _LoadCSTCommand) _LoadCSTCommand = new AsyncCommand(LoadCSTCommandFunc);
                return _LoadCSTCommand;
            }
        }
        private async Task LoadCSTCommandFunc()
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    CleanScreen();
                    //this.FoupOpModule().FoupControllers[0].FoupModuleInfo.
                }));

                Task task = new Task(() =>
                {
                    this.FoupOpModule().GetFoupController(1).Execute(new FoupLoadCommand());
                });
                task.Start();
                await task;

                //Task.Run(() => this.FoupOpModule().GetFoupController(1).Execute(new FoupLoadCommand()));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> UnloadCSTCommand
        private AsyncCommand _UnloadCSTCommand;
        public ICommand UnloadCSTCommand
        {
            get
            {
                if (null == _UnloadCSTCommand) _UnloadCSTCommand = new AsyncCommand(UnloadCSTCommandFunc);
                return _UnloadCSTCommand;
            }
        }
        private async Task UnloadCSTCommandFunc()
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    CleanScreen();
                }));

                Task task = new Task(() =>
                {
                    this.FoupOpModule().GetFoupController(1).Execute(new FoupUnloadCommand());
                });
                task.Start();
                await task;

                //Task.Run(() => this.FoupOpModule().GetFoupController(1).Execute(new FoupUnloadCommand()));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> TotalPageCount
        private int _TotalPageCount;
        public int TotalPageCount
        {
            get { return _TotalPageCount; }
            set
            {
                if (value != _TotalPageCount)
                {
                    _TotalPageCount = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> PrevCassetteCommand
        private RelayCommand _PrevCassetteCommand;
        public ICommand PrevCassetteCommand
        {
            get
            {
                if (null == _PrevCassetteCommand) _PrevCassetteCommand = new RelayCommand(PrevCassetteCommandFunc);
                return _PrevCassetteCommand;
            }
        }
        private void PrevCassetteCommandFunc()
        {
            try
            {
                if (CurCassetteIndex <= 1)
                    return;

                CurCassetteIndex--;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> NextCassetteCommand
        private RelayCommand _NextCassetteCommand;
        public ICommand NextCassetteCommand
        {
            get
            {
                if (null == _NextCassetteCommand) _NextCassetteCommand = new RelayCommand(NextCassetteCommandFunc);
                return _NextCassetteCommand;
            }
        }
        private void NextCassetteCommandFunc()
        {
            try
            {
                if (CurCassetteIndex >= TotalCassetteCount)
                    return;

                CurCassetteIndex++;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> HandleIconClickCommand
        private RelayCommand<object> _HandleIconClickCommand;
        public ICommand HandleIconClickCommand
        {
            get
            {
                if (null == _HandleIconClickCommand) _HandleIconClickCommand = new RelayCommand<object>(HandleIconClickCommandFunc);
                return _HandleIconClickCommand;
            }
        }

        private void HandleIconClickCommandFunc(object param)
        {
            try
            {
                //==> Parameter Parsing and Check
                Object[] paramArr = param as Object[];
                if (paramArr == null)
                    return;

                if (paramArr.Length < 4)
                    return;

                ModuleInfoBase moduleInfo = paramArr[0] as ModuleInfoBase;//==> Button에 Binding 된 DataContext
                if (moduleInfo == null)
                    return;

                UserControl userControl = paramArr[1] as UserControl;//==> Button에 대한 UserControl
                if (userControl == null)
                    return;

                UserControl parentUserControl = paramArr[2] as UserControl;//==> 화면에 대한 UserControl(Visual type), 직선 같은 UI를 그리기 위해서 필요
                if (parentUserControl == null)
                    return;
                if (parentUserControl is Visual == false)
                    return;

                if (paramArr[3] is bool == false)
                    return;

                bool isSource = (bool)paramArr[3];

                //==> Create ModuleDataInfo
                bool holderStatus = false;
                if (moduleInfo is CassetteModuleInfo)
                {
                    CassetteDataInfo cassetteDataInfo = new CassetteDataInfo((CassetteModuleInfo)moduleInfo, userControl, parentUserControl, isSource);
                    holderStatus = CassetteStatusCheck(isSource, cassetteDataInfo);
                }
                else if (moduleInfo is HolderModuleInfo)
                {
                    HolderDataInfo holderDataInfo = new HolderDataInfo((HolderModuleInfo)moduleInfo, userControl, parentUserControl, isSource);
                    holderStatus = HolderStatusCheck(isSource, holderDataInfo);
                }

                if (holderStatus == false)
                    return;

                if (_SrcHolderDataInfo != null && _DstHolderDataInfo != null)
                    CurSelectedRecipe = new TranslateRecipe(_SrcHolderDataInfo, _DstHolderDataInfo);

                UpdateScreen();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private bool CassetteStatusCheck(bool isSource, CassetteDataInfo cassetteDataInfo)
        {
            try
            {
                int min = 1;
                int max = cassetteDataInfo.CassetteInfo.SlotModules.Length;

                //==> Casset를 선택 하였을 때 기본으로 1번 Slot을 선택 하도록 한다.
                int slotNum = VirtualKeyboard.Show(1, min, max);
                if (slotNum == -1)
                    return false;

                //==> Slot Number에 해당하는 Slot Module을 찾음.
                var slotModule = cassetteDataInfo.CassetteInfo.SlotModules.Where(item => item.ID.Index == slotNum).FirstOrDefault();
                if (slotModule == null)
                    return false;

                if (slotModule.WaferStatus == EnumSubsStatus.UNDEFINED ||
                    slotModule.WaferStatus == EnumSubsStatus.UNKNOWN)
                    return false;

                    if (isSource)
                {
                    if (slotModule.WaferStatus == EnumSubsStatus.NOT_EXIST)
                        return false;

                    ////==> Source and destination is same
                    //if (_DstHolderDataInfo != null )//&& cassetteDataInfo.ModuleInfo.ID == _DstHolderDataInfo.ModuleInfo.ID)
                    //    return;

                    //==> 이전에 선택한 Cassette Source Icon Highlight 지움
                    if (_SrcHolderDataInfo != null)
                        _SrcHolderDataInfo.UserControl.Background = Brushes.Transparent;

                    //==> 새로운 Cassette Source Icon 으로 지정
                    cassetteDataInfo.SlotNum = slotNum;
                    _SrcHolderDataInfo = cassetteDataInfo;
                    _SrcHolderDataInfo.UserControl.Background = Brushes.SkyBlue;
                }
                else
                {
                    if (slotModule.WaferStatus == EnumSubsStatus.EXIST)
                        return false;

                    //==> Source and destination is same
                    //if (_SrcHolderDataInfo != null )//&& cassetteDataInfo.ModuleInfo.ID == _SrcHolderDataInfo.ModuleInfo.ID)
                    //    return;

                    //==> 이전에 선택한 Cassette Destination Icon Highlight 지움
                    if (_DstHolderDataInfo != null)
                        _DstHolderDataInfo.UserControl.Background = Brushes.Transparent;

                    //==> 새로운 Cassette Destination Icon 으로 지정
                    cassetteDataInfo.SlotNum = slotNum;
                    _DstHolderDataInfo = cassetteDataInfo;
                    _DstHolderDataInfo.UserControl.Background = Brushes.SkyBlue;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
           
            return true;
        }


        private bool HolderStatusCheck(bool isSource, HolderDataInfo holderDataInfo)
        {
            try
            {
                if (isSource)
                {
                    //==> 이미 선택 되어 있는 Holder Source Icon 이다.
                    if (holderDataInfo.HolderInfo.WaferStatus != EnumSubsStatus.EXIST)
                        return false;

                    //==> Source와 Destination 버튼이 같다.
                    if (_DstHolderDataInfo != null && holderDataInfo.ModuleInfo.ID == _DstHolderDataInfo.ModuleInfo.ID)
                        return false;

                    //==> 이전에 선택한 Holder Source Icon 의 Highlight 지움
                    if (_SrcHolderDataInfo != null)
                        _SrcHolderDataInfo.UserControl.Background = Brushes.Transparent;

                    //==> 새로운 Holder Source Icon 으로 지정 한다.
                    _SrcHolderDataInfo = holderDataInfo;
                    _SrcHolderDataInfo.UserControl.Background = Brushes.SkyBlue;
                }
                else
                {
                    //==> 이미 선택 되어 있는 Holder Destination Icon 이다.
                    if (holderDataInfo.HolderInfo.WaferStatus == EnumSubsStatus.EXIST)
                        return false;

                    //==> Source와 Destination 버튼이 같다.
                    if (_SrcHolderDataInfo != null && holderDataInfo.ModuleInfo.ID == _SrcHolderDataInfo.ModuleInfo.ID)
                        return false;

                    //==> 이전에 선택한 Holder Destination Icon 의 Highlight 지움
                    if (_DstHolderDataInfo != null)
                        _DstHolderDataInfo.UserControl.Background = Brushes.Transparent;

                    //==> 새로운 Holder Destination Icon 으로 지정 한다.
                    _DstHolderDataInfo = holderDataInfo;
                    _DstHolderDataInfo.UserControl.Background = Brushes.SkyBlue;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return true;
        }

        #endregion

        #region ==> TotalCassetteCount
        public int TotalCassetteCount => LoaderController.LoaderInfo.StateMap.CassetteModules.Length;
        #endregion

        #region ==> CurCassetteIndex
        private int _CurCassetteIndex;
        /// <summary>
        /// 1 ~ N (non zero)
        /// </summary>
        public int CurCassetteIndex
        {
            get { return _CurCassetteIndex; }
            set
            {
                if (value != _CurCassetteIndex)
                {
                    _CurCassetteIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> ArrowLineList : 화면에 표시되는 직선
        private ObservableCollection<Line> _ArrowLineList;
        public ObservableCollection<Line> ArrowLineList
        {
            get { return _ArrowLineList; }
            set
            {
                if (value != _ArrowLineList)
                {
                    _ArrowLineList = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> X2ViewChangeCommand
        private RelayCommand _X2ViewChangeCommand;
        public RelayCommand X2ViewChangeCommand
        {
            get
            {
                if (null == _X2ViewChangeCommand) _X2ViewChangeCommand = new RelayCommand(X2ViewChangeCommandFunc);
                return _X2ViewChangeCommand;
            }
        }
        public void X2ViewChangeCommandFunc() // 2x view
        {
            try
            {
                ViewNUMx2 = !ViewNUMx2;
                ViewerCont();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> CWViewChangeCommand
        private RelayCommand _CWViewChangeCommand;
        public ICommand CWViewChangeCommand
        {
            get
            {
                if (null == _CWViewChangeCommand) _CWViewChangeCommand = new RelayCommand(CWViewChangeCommandFunc);
                return _CWViewChangeCommand;
            }
        }
        public void CWViewChangeCommandFunc() //ClockWise
        {
            try
            {
                ViewNUM = ViewNUM - 1;

                if (ViewNUM < 0)
                {
                    ViewNUM = 7;
                }

                ViewerCont();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> CenterViewChangeCommand
        private RelayCommand _CenterViewChangeCommand;
        public ICommand CenterViewChangeCommand
        {
            get
            {
                if (null == _CenterViewChangeCommand) _CenterViewChangeCommand = new RelayCommand(CenterViewChangeCommandFunc);
                return _CenterViewChangeCommand;
            }
        }
        public void CenterViewChangeCommandFunc() //FRONT
        {
            try
            {
                ViewNUM = 0;
                ViewNUMx2 = false;
                ViewerCont();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> CCWViewChangeCommand
        private RelayCommand _CCWViewChangeCommand;
        public ICommand CCWViewChangeCommand
        {
            get
            {
                if (null == _CCWViewChangeCommand) _CCWViewChangeCommand = new RelayCommand(CCWViewChangeCommandFunc);
                return _CCWViewChangeCommand;
            }
        }
        public void CCWViewChangeCommandFunc() // CounterClockWise
        {
            try
            {
                ViewNUM = ViewNUM + 1;
                if (ViewNUM > 7)
                {
                    ViewNUM = 0;
                }
                ViewerCont();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        public void ViewerCont()
        {
            try
            {
                if (ViewNUMx2 == false)
                {
                    if (ViewNUM == 0) // FRONT VIEW
                    {
                        ViewModelManager.CamPosition = new Point3D(121.4, 1487.7, 1830.4);
                        ViewModelManager.CamLookDirection = new Vector3D(0, -0.7, -0.8);
                        ViewModelManager.CamUpDirection = new Vector3D(0, 0.8, -0.6);
                    }
                    else if (ViewNUM == 1) // FOUP
                    {
                        ViewModelManager.CamPosition = new Point3D(1419.1, 1487.7, 1298.2);
                        ViewModelManager.CamLookDirection = new Vector3D(-0.5, -0.6, -0.6);
                        ViewModelManager.CamUpDirection = new Vector3D(-0.4, 0.8, -0.4);
                    }
                    else if (ViewNUM == 2) // LOADER
                    {
                        ViewModelManager.CamPosition = new Point3D(1960.4, 1487.7, 4.2);
                        ViewModelManager.CamLookDirection = new Vector3D(-0.8, -0.6, 0);
                        ViewModelManager.CamUpDirection = new Vector3D(-0.6, 0.8, 0);
                    }
                    else if (ViewNUM == 3) // LOADER BACK
                    {
                        ViewModelManager.CamPosition = new Point3D(1535.8, 1487.7, -1175.2);
                        ViewModelManager.CamLookDirection = new Vector3D(-0.6, -0.6, 0.5);
                        ViewModelManager.CamUpDirection = new Vector3D(-0.5, 0.8, 0.4);
                    }
                    else if (ViewNUM == 4) // BACK
                    {
                        ViewModelManager.CamPosition = new Point3D(134.3, 1487.7, -1834.7);
                        ViewModelManager.CamLookDirection = new Vector3D(0, -0.6, 0.8);
                        ViewModelManager.CamUpDirection = new Vector3D(0, 0.8, 0.6);
                    }
                    else if (ViewNUM == 5) // STAGE BACK
                    {
                        ViewModelManager.CamPosition = new Point3D(-1271.8, 1487.7, -1185);
                        ViewModelManager.CamLookDirection = new Vector3D(0.6, -0.6, 0.5);
                        ViewModelManager.CamUpDirection = new Vector3D(0.5, 0.8, 0.4);
                    }
                    else if (ViewNUM == 6) // STAGE 
                    {
                        ViewModelManager.CamPosition = new Point3D(-1704.7, 1487.7, -8.6);
                        ViewModelManager.CamLookDirection = new Vector3D(0.8, -0.6, 0);
                        ViewModelManager.CamUpDirection = new Vector3D(0.6, 0.8, 0);
                    }
                    else if (ViewNUM == 7) // STAGE 
                    {
                        ViewModelManager.CamPosition = new Point3D(-1055, 1487.7, 1397.5);
                        ViewModelManager.CamLookDirection = new Vector3D(0.5, -0.6, -0.6);
                        ViewModelManager.CamUpDirection = new Vector3D(0.4, 0.8, -0.5);
                    }
                    else
                    {

                    }
                }
                else if (ViewNUMx2 == true)
                {
                    if (ViewNUM == 0) // FRONT VIEW x2
                    {
                        ViewModelManager.CamPosition = new Point3D(123, 1148, 1412);
                        ViewModelManager.CamLookDirection = new Vector3D(0.0, -0.6, -0.8);
                        ViewModelManager.CamUpDirection = new Vector3D(0.0, 0.8, -0.6);
                    }
                    else if (ViewNUM == 1) // FOUP x2
                    {
                        ViewModelManager.CamPosition = new Point3D(1208, 1148, 911);
                        ViewModelManager.CamLookDirection = new Vector3D(-0.6, -0.6, -0.5);
                        ViewModelManager.CamUpDirection = new Vector3D(-0.5, 0.8, -0.4);
                    }
                    else if (ViewNUM == 2) // LOADER x2
                    {
                        ViewModelManager.CamPosition = new Point3D(1542, 1148, 3);
                        ViewModelManager.CamLookDirection = new Vector3D(-0.8, -0.6, 0.0);
                        ViewModelManager.CamUpDirection = new Vector3D(-0.6, 0.8, 0.0);
                    }
                    else if (ViewNUM == 3) // LOADER BACK x2
                    {
                        ViewModelManager.CamPosition = new Point3D(1041, 1148, -1083);
                        ViewModelManager.CamLookDirection = new Vector3D(-0.5, -0.6, 0.6);
                        ViewModelManager.CamUpDirection = new Vector3D(-0.4, 0.8, 0.5);
                    }
                    else if (ViewNUM == 4) // BACK x2
                    {
                        ViewModelManager.CamPosition = new Point3D(133, 1148, -1417);
                        ViewModelManager.CamLookDirection = new Vector3D(0.0, -0.6, 0.8);
                        ViewModelManager.CamUpDirection = new Vector3D(0.0, 0.8, 0.6);
                    }
                    else if (ViewNUM == 5) // STAGE BACK x2
                    {
                        ViewModelManager.CamPosition = new Point3D(-1095, 1148, -714);
                        ViewModelManager.CamLookDirection = new Vector3D(0.7, -0.6, 0.4);
                        ViewModelManager.CamUpDirection = new Vector3D(0.5, 0.8, 0.3);
                    }
                    else if (ViewNUM == 6) // STAGE x2
                    {
                        ViewModelManager.CamPosition = new Point3D(-1287, 1148, -7);
                        ViewModelManager.CamLookDirection = new Vector3D(0.8, -0.6, 0.0);
                        ViewModelManager.CamUpDirection = new Vector3D(0.6, 0.8, 0.0);
                    }
                    else if (ViewNUM == 7) // STAGE  x2
                    {
                        ViewModelManager.CamPosition = new Point3D(-876, 1148, 994);
                        ViewModelManager.CamLookDirection = new Vector3D(0.6, -0.6, -0.5);
                        ViewModelManager.CamUpDirection = new Vector3D(0.4, 0.8, -0.4);
                    }
                    else
                    {

                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void UpdateScreen()
        {
            try
            {
                //==> Clear Arrow Line
                ArrowLineList = new ObservableCollection<Line>();

                if (CurSelectedRecipe != null)
                    DrawLineToScreen(CurSelectedRecipe, Brushes.Yellow);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        //==> 화면 UI 정리
        private void CleanScreen()
        {
            try
            {
                ArrowLineList = new ObservableCollection<Line>();
                if (_SrcHolderDataInfo != null && _SrcHolderDataInfo.UserControl != null)
                {
                    _SrcHolderDataInfo.UserControl.Background = Brushes.Transparent;
                    _SrcHolderDataInfo = null;
                }
                if (_DstHolderDataInfo != null && _DstHolderDataInfo.UserControl != null)
                {
                    _DstHolderDataInfo.UserControl.Background = Brushes.Transparent;
                    _DstHolderDataInfo = null;
                }
                CurSelectedRecipe = null;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void DrawLineToScreen(TranslateRecipe translateCmd, Brush color)
        {
            /*
             * [ICON] (1)---(2)
             *               |
             *               |
             *               |
             *              (3)-->(4) [ICON]
             * 
             * (1) : srcIconPortPoint
             * (2) : betweenPoint1 
             * (3) : betweenPoint2
             * (4) : endIconPortPoint
             */

            try
            {
                if (translateCmd.SrcHolderDataInfo == null)
                    return;
                if (translateCmd.DstHolderDataInfo == null)
                    return;

                Point srcIconPortPoint = translateCmd.SrcHolderDataInfo.GetIconPortPoint();
                Point endIconPortPoint = translateCmd.DstHolderDataInfo.GetIconPortPoint();

                double betweenMiddleX = 0;
                betweenMiddleX = srcIconPortPoint.X + ((endIconPortPoint.X - srcIconPortPoint.X) / 2);

                Point betweenPoint1 = new Point(betweenMiddleX, srcIconPortPoint.Y);
                Point betweenPoint2 = new Point(betweenMiddleX, endIconPortPoint.Y);

                //==> Draw Arrow Line
                DrawStraight(srcIconPortPoint, betweenPoint1, color);//==> (1) - (2) 까지 직선
                DrawStraight(betweenPoint1, betweenPoint2, color);//==> (2) - (3) 까지 직선
                DrawStraight(betweenPoint2, endIconPortPoint, color);//==> (3) - (4) 까지 직선
                DrawArrowHeader(betweenPoint2, endIconPortPoint, color);//==> (3) - (4) 까지의 직선에 화살 머리
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #region ==> DrawLineToScreen [Vertical orientation]
        //private void DrawLineToScreen(TranslateRecipe translateCmd, Brush color)
        //{
        //    /*
        //     * [ICON] (1)
        //     *         |
        //     *         |
        //     *        (2)---(3)
        //     *               |
        //     *               V
        //     *              (4) [ICON]
        //     * 
        //     * (1) : srcIconPortPoint
        //     * (2) : betweenPoint1 
        //     * (3) : betweenPoint2
        //     * (4) : endIconPortPoint
        //     */

        //    if (translateCmd.SrcHolderDataInfo == null)
        //        return;
        //    if (translateCmd.DstHolderDataInfo == null)
        //        return;

        //    Point srcIconPortPoint = translateCmd.SrcHolderDataInfo.GetIconPortPoint();
        //    Point endIconPortPoint = translateCmd.DstHolderDataInfo.GetIconPortPoint();

        //    double betweenMiddleYPoint = 0;
        //    betweenMiddleYPoint = srcIconPortPoint.Y + ((endIconPortPoint.Y - srcIconPortPoint.Y) / 2);

        //    Point betweenPoint1 = new Point(srcIconPortPoint.X, betweenMiddleYPoint);
        //    Point betweenPoint2 = new Point(endIconPortPoint.X, betweenMiddleYPoint);

        //    //==> Draw Arrow Line
        //    DrawStraight(srcIconPortPoint, betweenPoint1, color);
        //    DrawStraight(betweenPoint1, betweenPoint2, color);
        //    DrawStraight(betweenPoint2, endIconPortPoint, color);
        //    DrawArrowHeader(betweenPoint2, endIconPortPoint, color);
        //}
        #endregion

        //==> 단순 직선을 그린다.
        private void DrawStraight(Point startPt, Point endPt, Brush color)
        {
            try
            {
                Line line = MakeLine(startPt.X, startPt.Y, endPt.X, endPt.Y, color);
                ArrowLineList.Add(line);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        //==> 화살표 직선에서 화살 머리를 그린다.
        private void DrawArrowHeader(Point startPt, Point endPt, Brush color)
        {
            try
            {
                double vx = endPt.X - startPt.X;
                double vy = endPt.Y - startPt.Y;
                double dist = Math.Sqrt(Math.Pow(vx, 2) + Math.Pow(vy, 2));
                double length = 10;
                vx /= dist;
                vy /= dist;

                double ax = length * (-vy - vx);
                double ay = length * (vx - vy);
                
                //==> 화살표 머리 부분
                Line arrowLine1 = MakeLine(endPt.X + ax, endPt.Y + ay, endPt.X, endPt.Y, color);
                Line arrowLine2 = MakeLine(endPt.X - ay, endPt.Y + ax, endPt.X, endPt.Y, color);
                ArrowLineList.Add(arrowLine1);
                ArrowLineList.Add(arrowLine2);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        //==> 직선을 만든다.
        private Line MakeLine(double x1, double y1, double x2, double y2, Brush color)
        {
            Line line = new Line();

            try
            {
                line.Stroke = color;
                line.X1 = x1;
                line.Y1 = y1;
                line.X2 = x2;
                line.Y2 = y2;
                line.StrokeThickness = 3;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return line;
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"DeInitViewModel() in {GetType().Name}");

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    this.CurCassetteIndex = 1;

                    LoaderController = this.LoaderController() as ILoaderControllerExtension;
                    CommandManager = this.CommandManager();

                    ViewModelManager = this.ViewModelManager();
                    CenterViewChangeCommandFunc();
                    ViewNUM = 0;
                    ViewNUMx2 = false;
                    ArrowLineList = new ObservableCollection<Line>();

                    Stage3DModel = this.ViewModelManager().Stage3DModel;

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
        public EventCodeEnum InitPage(object parameter = null)
        {
            return EventCodeEnum.NONE;
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                //this.SysState().SetSetUpState();
                CenterViewChangeCommandFunc();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Stage3DModel = null;
                    Stage3DModel = this.ViewModelManager().Stage3DModel;
                });

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            try
            {
                //this.SysState().SetSetUpDoneState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
    }
    class TranslateRecipe
    {
        public ModuleDataInfoBase SrcHolderDataInfo { get; set; }
        public ModuleDataInfoBase DstHolderDataInfo { get; set; }
        public TranslateRecipe(ModuleDataInfoBase srcHolderDataInfo, ModuleDataInfoBase dstHolderDataInfo)
        {
            try
            {
                SrcHolderDataInfo = srcHolderDataInfo;
                DstHolderDataInfo = dstHolderDataInfo;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

    //==> Icon에 바인딩 되는 DataContext
    abstract class ModuleDataInfoBase
    {
        public abstract ModuleInfoBase ModuleInfo { get; }
        public bool IsSource { get; set; }
        public UserControl UserControl { get; set; }
        public UserControl ParentUserControl { get; set; }
        public ModuleDataInfoBase(UserControl userControl, UserControl parentUserControl, bool isSource)
        {
            try
            {
                UserControl = userControl;
                ParentUserControl = parentUserControl;
                IsSource = isSource;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public Point GetIconPortPoint()
        {
            //==> [여기서 익셉션이 난다면...]
            //==> 주로 UserControl이 Visual이 아니라서 익셉션이 나는것이 아니다.
            //==> 이미 지워진 UserControl을 사용해서 문제가 되는 것이다, Icon은 Loader의 Holder 객체들과 바인딩 되어있는데 
            //==> Holder 객체가 새로 채워질때마 새롭게 UserControl이 추가되고 코드에서는 이미 지워진 UserControl을 참조해서 문제가 발생 하는 것이다.

            Point portPoint = new Point(0, 0);

            try
            {
                Point iconLeftTop = UserControl.TransformToAncestor(ParentUserControl).Transform(new Point(0, 0));
                portPoint = iconLeftTop;

                if (IsSource)
                {
                    portPoint.X += UserControl.ActualWidth;
                    portPoint.Y += UserControl.ActualHeight / 2;
                }
                else
                {
                    portPoint.Y += UserControl.ActualHeight / 2;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return portPoint;
        }
        //public Point GetIconPortPoint()
        //{
        //    Point iconLeftTop = UserControl.TransformToAncestor(ParentUserControl).Transform(new Point(0, 0));

        //    Point portPoint = iconLeftTop;

        //    if (IsSource)
        //    {
        //        portPoint.X += UserControl.ActualWidth / 2;
        //        portPoint.Y += UserControl.ActualHeight;
        //    }
        //    else
        //    {
        //        portPoint.X += UserControl.ActualWidth / 2;
        //    }


        //    return portPoint;
        //}
        public Point GetIconCenterPoint()
        {
            Point iconLeftTop = UserControl.TransformToAncestor(ParentUserControl).Transform(new Point(0, 0));
            Point iconCenter = iconLeftTop;

            try
            {
                iconCenter.X += UserControl.ActualWidth / 2;
                iconCenter.Y += UserControl.ActualHeight / 2;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return iconCenter;
        }
        public abstract TransferObject GetSelectedSubstrate();
        public abstract ModuleID GetSelectedModuleID();
    }
    //==> Holder Icon에 바인딩 되는 DataContext
    class HolderDataInfo : ModuleDataInfoBase
    {
        public override ModuleInfoBase ModuleInfo => HolderInfo;
        public HolderModuleInfo HolderInfo { get; set; }

        public HolderDataInfo(HolderModuleInfo holderInfo, UserControl userControl, UserControl parentUserControl, bool isSource)
            : base(userControl, parentUserControl, isSource)
        {
            HolderInfo = holderInfo;
        }

        public override TransferObject GetSelectedSubstrate()
        {
            TransferObject retval = null;

            try
            {
                if (HolderInfo.WaferStatus == EnumSubsStatus.EXIST)
                {
                    retval = HolderInfo.Substrate;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override ModuleID GetSelectedModuleID()
        {
            ModuleID id = ModuleID.UNDEFINED;

            try
            {
                id = HolderInfo.ID;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return id;
        }
    }
    //==> Cassette Icon에 바인딩 되는 DataContext
    class CassetteDataInfo : ModuleDataInfoBase
    {
        private int _SlotNum;
        public int SlotNum
        {
            get
            {
                return _SlotNum;
            }
            set
            {
                PropertyInfo slotNumProp = UserControl.GetType().GetProperty("SlotNum", BindingFlags.Public | BindingFlags.Instance);
                slotNumProp.SetValue(UserControl, value.ToString());
                _SlotNum = value;
            }
        }
        public override ModuleInfoBase ModuleInfo => CassetteInfo;
        public CassetteModuleInfo CassetteInfo { get; set; }

        public CassetteDataInfo(CassetteModuleInfo cassetteInfo, UserControl userControl, UserControl parentUserControl, bool isSource)
            : base(userControl, parentUserControl, isSource)
        {
            CassetteInfo = cassetteInfo;
        }

        public override TransferObject GetSelectedSubstrate()
        {
            TransferObject selSub = null;

            try
            {
                var selSlot = CassetteInfo.SlotModules.Where(item => item.ID.Index == SlotNum).FirstOrDefault();

                if (selSlot.WaferStatus == EnumSubsStatus.EXIST)
                {
                    selSub = selSlot.Substrate;
                }
                else
                {
                    selSub = new TransferObject();
                    selSub.CurrPos = selSlot.ID;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return selSub;
        }

        public override ModuleID GetSelectedModuleID()
        {
            HolderModuleInfo selSlot = null;

            ModuleID id = ModuleID.UNDEFINED;

            try
            {
                selSlot = CassetteInfo.SlotModules.Where(item => item.ID.Index == SlotNum).FirstOrDefault();

                if (selSlot == null)
                {
                    //TODO : error
                    id = ModuleID.UNDEFINED;
                }
                else
                {
                    id = selSlot.ID;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return id;
        }
    }
}
