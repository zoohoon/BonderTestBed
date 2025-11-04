using System;
using System.Linq;
using System.Threading.Tasks;

namespace LoaderTestMainPageViewModel
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
    using System.Windows.Media;
    //using ProberSimulator;
    //using ViewModelModule;
    using VirtualKeyboardControl;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using System.Windows.Controls;
    using System.Windows.Shapes;
    using LoaderParameters.Data;
    using LoaderController;
    using MetroDialogInterfaces;

    public class VmLoaderTestMainPage : IMainScreenViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        readonly Guid _ViewModelGUID = new Guid("0f6717f9-adad-485e-9768-5265fe5fbff3");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        public bool Initialized { get; set; } = false;
        public ICommandManager CommandManager { get; set; }

        public ILoaderControllerExtension LoaderController { get; set; }
        public int TotalCassetteCount => LoaderController.LoaderInfo.StateMap.CassetteModules.Length;
        private TranslateRecipe _CurSelectedRecipe;
        private ModuleDataInfoBase _SrcHolderDataInfo;
        private ModuleDataInfoBase _DstHolderDataInfo;
        private String _TestTargetSub;

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
                if (_CurSelectedRecipe == null)
                    return;

                CassetteModuleInfo cassette = LoaderController.LoaderInfo.StateMap.CassetteModules.FirstOrDefault();
                if (cassette.ScanState != CassetteScanStateEnum.READ)
                    return;

                //this.ViewModelManager().Lock(this.GetHashCode(), "WaferTransfer", "wait");
                

                //=> find source
                TransferObject targetSub = _CurSelectedRecipe.SrcHolderDataInfo.GetSelectedSubstrate();
                //=> find dest pos
                ModuleID destPos = _CurSelectedRecipe.DstHolderDataInfo.GetSelectedModuleID();

                //=> Req to loader
                if (targetSub != null && destPos != ModuleID.UNDEFINED)
                {
                    if (this.LoaderController().ModuleState.GetState() == ModuleStateEnum.IDLE)
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
                        System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            CleanScreen();
                            UpdateScreen();
                        }));

                        
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #region ==> ScanCSTCommand
        private RelayCommand _ScanCSTCommand;
        public ICommand ScanCSTCommand
        {
            get
            {
                if (null == _ScanCSTCommand) _ScanCSTCommand = new RelayCommand(ScanCSTCommandFunc);
                return _ScanCSTCommand;
            }
        }
        private async void ScanCSTCommandFunc()
        {
            try
            {
                CleanScreen();
                DoScanCST();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private void DoScanCST()
        {
            try
            {
                CassetteModuleInfo cassette = LoaderController.LoaderInfo.StateMap.CassetteModules.FirstOrDefault();

                var foupController = this.FoupOpModule().GetFoupController(cassette.ID.Index);
                if (foupController.FoupModuleInfo.State == ProberInterfaces.Foup.FoupStateEnum.UNLOAD)
                {
                    EventCodeEnum retVal = foupController.Execute(new FoupLoadCommand() { });
                }

                if (foupController.FoupModuleInfo.State != ProberInterfaces.Foup.FoupStateEnum.LOAD)
                {
                    string caption = "ERROR";
                    string message = $"Foup state invalid. state={foupController.FoupModuleInfo.State}";

                    //var dlgRel = await this.MetroDialogManager().ShowMessageDialog(caption, message, EnumMessageStyle.Affirmative);

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
                throw;
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

        #region ==> TransferCmdAddCommand
        private RelayCommand _TransferCmdAddCommand;
        public ICommand TransferCmdAddCommand
        {
            get
            {
                if (null == _TransferCmdAddCommand) _TransferCmdAddCommand = new RelayCommand(TransferCmdAddCommandFunc);
                return _TransferCmdAddCommand;
            }
        }
        private void TransferCmdAddCommandFunc()
        {
            try
            {
                if (_SrcHolderDataInfo == null)
                    return;

                if (_DstHolderDataInfo == null)
                    return;

                //=> find source
                if (String.IsNullOrEmpty(_TestTargetSub))
                {
                    TransferObject targetSub = _SrcHolderDataInfo.GetSelectedSubstrate();
                    _TestTargetSub = targetSub.ID.Value;
                }

                String dstModuleName = _DstHolderDataInfo.ModuleInfo.ID.Label;

                TransferCmdList?.Add(new TransferCommand(LoaderController, _DstHolderDataInfo, $"==> {dstModuleName}"));
                SelectedTransferCmd = TransferCmdList.LastOrDefault();

                //==> Clear UI
                ArrowLineList = new ObservableCollection<Line>();
                if (_DstHolderDataInfo != null && _DstHolderDataInfo.UserControl != null)
                {
                    _DstHolderDataInfo.UserControl.Background = Brushes.Transparent;
                    _DstHolderDataInfo = null;
                }
                _CurSelectedRecipe = null;

                UpdateScreen();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #region ==> TransferCmdDelCommand
        private RelayCommand _TransferCmdDelCommand;
        public ICommand TransferCmdDelCommand
        {
            get
            {
                if (null == _TransferCmdDelCommand) _TransferCmdDelCommand = new RelayCommand(TransferCmdDelCommandFunc);
                return _TransferCmdDelCommand;
            }
        }
        private void TransferCmdDelCommandFunc()
        {
            try
            {
                TransferCmdList?.Remove(SelectedTransferCmd);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #region ==> TransferCmdClearCommand
        private RelayCommand _TransferCmdClearCommand;
        public ICommand TransferCmdClearCommand
        {
            get
            {
                if (null == _TransferCmdClearCommand) _TransferCmdClearCommand = new RelayCommand(TransferCmdClearCommandFunc);
                return _TransferCmdClearCommand;
            }
        }
        private void TransferCmdClearCommandFunc()
        {
            try
            {
                TransferCmdList.Clear();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #region ==> TransferCmdRunTestCommand
        private AsyncCommand _TransferCmdRunTestCommand;
        public ICommand TransferCmdRunTestCommand
        {
            get
            {
                if (null == _TransferCmdRunTestCommand) _TransferCmdRunTestCommand = new AsyncCommand(TransferCmdRunTestCommandFunc);
                return _TransferCmdRunTestCommand;
            }
        }
        private async Task TransferCmdRunTestCommandFunc()
        {
            try
            {
                if (TransferCmdList.Count < 1)
                    return;

                LoaderTestOption option = new LoaderTestOption();
                option.OptionFlag = true;
                option.ScanFailOption = ScanFailOptionEnum.FORCE_DONE;
                LoaderController.LoaderService.SetLoaderTestOption(option);
                ScanCount = 0;
                //==> 현재 Cassette
                CassetteModuleInfo cassette = LoaderController.LoaderInfo.StateMap.CassetteModules.FirstOrDefault();
                if (cassette.ScanState != CassetteScanStateEnum.READ)
                    return;

                //==> Wafer가 2개 이상이면 동작 않함.
                int waferCount = 0;
                waferCount += LoaderController.LoaderInfo.StateMap.ARMModules.Where(item => item.WaferStatus == EnumSubsStatus.EXIST).Count();
                waferCount += LoaderController.LoaderInfo.StateMap.PreAlignModules.Where(item => item.WaferStatus == EnumSubsStatus.EXIST).Count();
                waferCount += LoaderController.LoaderInfo.StateMap.FixedTrayModules.Where(item => item.WaferStatus == EnumSubsStatus.EXIST).Count();
                waferCount += LoaderController.LoaderInfo.StateMap.InspectionTrayModules.Where(item => item.WaferStatus == EnumSubsStatus.EXIST).Count();
                waferCount += LoaderController.LoaderInfo.StateMap.ChuckModules.Where(item => item.WaferStatus == EnumSubsStatus.EXIST).Count();
                waferCount += cassette.SlotModules.Where(item => item.WaferStatus == EnumSubsStatus.EXIST).Count();
                if (waferCount > 1)
                    return;

                int waferExistSlotIndex = 0;
                for (int i = 0; i < cassette.SlotModules.Length; i++)
                {
                    if (cassette.SlotModules[i].WaferStatus == EnumSubsStatus.EXIST)
                    {
                        waferExistSlotIndex = i;
                        break;
                    }
                }
                if (waferExistSlotIndex != 24)
                    return;

                option.ScanSlotNum = 1;
                Task.Run((Action)(() =>
                {
                    do
                    {
                        IStateModule stateModule = LoaderController as IStateModule;
                        ModuleStateEnum moduleState = ModuleStateEnum.UNDEFINED;
                        foreach (TransferCommand transferCmd in TransferCmdList)
                        {
                            if (transferCmd.ModuleType == ModuleTypeEnum.SCANCAM)
                            {
                                if (ScanCount > 1)
                                {
                                    (this.LoaderController as LoaderController).FoupTiltIgoreFlag = true;
                                }
                                ScanCount++;
                                DoScanCST();
                                while (true)
                                {
                                    System.Threading.Thread.Sleep(1000);
                                    moduleState = stateModule.ModuleState.State;
                                    if (moduleState == ModuleStateEnum.SUSPENDED ||
                                        moduleState == ModuleStateEnum.IDLE ||
                                        moduleState == ModuleStateEnum.PAUSED ||
                                        moduleState == ModuleStateEnum.ERROR)
                                        break;
                                }

                                if (moduleState == ModuleStateEnum.SUSPENDED || moduleState == ModuleStateEnum.ERROR)
                                {
                                    SequenceRepeat = false;
                                    TestRepeat = false;
                                    break;
                                }

                                continue;
                            }

                            SelectedTransferCmd = transferCmd;

                            //(this).ViewModelManager().Lock((int)this.GetHashCode(), (string)"WaferTransfer", (string)"wait");
                            

                            ModuleID destPos = transferCmd.GetdModuleID();
                            LoaderMapEditor editor = LoaderController.GetLoaderMapEditor();
                            editor.EditorState.SetTransfer(_TestTargetSub, destPos);

                            LoaderMapCommandParameter cmdParam = new LoaderMapCommandParameter();
                            cmdParam.Editor = editor;

                            bool isInjected = CommandManager.SetCommand<ILoaderMapCommand>(this, cmdParam);

                            while (true)
                            {
                                System.Threading.Thread.Sleep(1000);
                                moduleState = stateModule.ModuleState.State;
                                if (moduleState == ModuleStateEnum.IDLE ||
                                    moduleState == ModuleStateEnum.PAUSED ||
                                    moduleState == ModuleStateEnum.ERROR)
                                    break;
                            }
                            if (moduleState == ModuleStateEnum.ERROR || moduleState == ModuleStateEnum.UNDEFINED)
                            {
                                SequenceRepeat = false;
                                TestRepeat = false;
                                break;
                            }

                            

                            if (SequenceRepeat && transferCmd.ModuleType == ModuleTypeEnum.CST)
                            {
                                option.ScanSlotNum = transferCmd.SlotNum;

                                int slotNum = transferCmd.SlotNum;
                                slotNum = slotNum % (transferCmd.SlotCount);
                                slotNum++;
                                transferCmd.SlotNum = slotNum;
                                transferCmd.CmdDesc = $"==> {destPos.Label} : {slotNum}";

                            }
                        }
                    } while (TestRepeat);

                    System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        CleanScreen();
                        UpdateScreen();
                    }));
                }));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }
        #endregion

        #region ==> ScanCmdRunTestCommand
        private RelayCommand _ScanCmdRunTestCommand;
        public ICommand ScanCmdRunTestCommand
        {
            get
            {
                if (null == _ScanCmdRunTestCommand) _ScanCmdRunTestCommand = new RelayCommand(ScanCmdRunTestCommandFunc);
                return _ScanCmdRunTestCommand;
            }
        }



        private int ScanCount = 0;
        private int Cnt = 0;
        private void ScanCmdRunTestCommandFunc()
        {
            try
            {
                //(this.LoaderController as LoaderController).FoupTiltIgoreFlag = false;
                ScanCount = 0;
                Cnt = 0;
                bool flag = false;
                Task.Run(() =>
                {
                    do
                    {
                        IStateModule stateModule = LoaderController as IStateModule;
                        ModuleStateEnum moduleState = ModuleStateEnum.UNDEFINED;
                        Cnt++;
                        if (Cnt > 20)
                        {
                            break;
                        }
                        //if (ScanCount > 100)
                        //{
                        //    if (flag)
                        //    {
                        //        (this.LoaderController as LoaderController).FoupTiltIgoreFlag = true;
                        //        flag = false;
                        //    }
                        //    else
                        //    {
                        //        (this.LoaderController as LoaderController).FoupTiltIgoreFlag = false;
                        //        flag = true;
                        //    }
                        //    ScanCount = 0;
                        //}
                        ScanCount++;
                        DoScanCST();
                        while (true)
                        {
                            System.Threading.Thread.Sleep(1000);
                            moduleState = stateModule.ModuleState.State;
                            if (moduleState == ModuleStateEnum.SUSPENDED ||
                                moduleState == ModuleStateEnum.IDLE ||
                                moduleState == ModuleStateEnum.PAUSED ||
                                moduleState == ModuleStateEnum.ERROR)
                                break;
                        }

                        //if (moduleState == ModuleStateEnum.SUSPENDED || moduleState == ModuleStateEnum.ERROR)
                        //{
                        //    TestRepeat = false;
                        //    break;
                        //}

                        continue;
                    } while (TestRepeat);
                    (this.LoaderController as LoaderController).FoupTiltIgoreFlag = false;
                    System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        CleanScreen();
                        UpdateScreen();
                    }));
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #region ==> AddScanJobCommand
        private RelayCommand _AddScanJobCommand;
        public ICommand AddScanJobCommand
        {
            get
            {
                if (null == _AddScanJobCommand) _AddScanJobCommand = new RelayCommand(AddScanJobCommandFunc);
                return _AddScanJobCommand;
            }
        }
        private void AddScanJobCommandFunc()
        {
            try
            {
                CassetteModuleInfo cassette = LoaderController.LoaderInfo.StateMap.CassetteModules.FirstOrDefault();
                IFoupController foupController = this.FoupOpModule().GetFoupController(cassette.ID.Index);
                if (foupController.FoupModuleInfo.State != ProberInterfaces.Foup.FoupStateEnum.LOAD)
                    return;

                if (cassette != null)
                {
                    if (this.LoaderController().ModuleState.GetState() == ModuleStateEnum.IDLE)
                    {
                        TransferCommand cmd = new TransferCommand();
                        cmd.ModuleType = ModuleTypeEnum.SCANCAM;
                        cmd.CmdDesc = "==> SCAN";
                        TransferCmdList?.Add(cmd);
                        SelectedTransferCmd = TransferCmdList.LastOrDefault();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #region ==> TestRepeat
        private bool _TestRepeat;
        public bool TestRepeat
        {
            get { return _TestRepeat; }
            set
            {
                if (value != _TestRepeat)
                {
                    _TestRepeat = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> SequenceRepeat
        private bool _SequenceRepeat;
        public bool SequenceRepeat
        {
            get { return _SequenceRepeat; }
            set
            {
                if (value != _SequenceRepeat)
                {
                    if (value)
                        TestRepeat = true;
                    _SequenceRepeat = value;
                    RaisePropertyChanged();
                }
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

                ModuleInfoBase moduleInfo = paramArr[0] as ModuleInfoBase;
                if (moduleInfo == null)
                    return;

                UserControl userControl = paramArr[1] as UserControl;
                if (userControl == null)
                    return;

                UserControl parentUserControl = paramArr[2] as UserControl;
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
                    _CurSelectedRecipe = new TranslateRecipe(_SrcHolderDataInfo, _DstHolderDataInfo);

                UpdateScreen();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private bool CassetteStatusCheck(bool isSource, CassetteDataInfo cassetteDataInfo)
        {
            try
            {
                int min = 1;
                int max = cassetteDataInfo.CassetteInfo.SlotModules.Length;

                //int slotNum = VirtualKeyboard.Show(cassetteDataInfo.SlotNum, min, max);
                int slotNum = VirtualKeyboard.Show(1, min, max);
                if (slotNum == -1)
                    return false;

                var slotModule = cassetteDataInfo.CassetteInfo.SlotModules.Where(item => item.ID.Index == slotNum).FirstOrDefault();
                if (slotModule == null)
                    return false;

                if (isSource)
                {
                    if (slotModule.WaferStatus == EnumSubsStatus.NOT_EXIST)
                        return false;

                    ////==> Source and destination is same
                    //if (_DstHolderDataInfo != null )//&& cassetteDataInfo.ModuleInfo.ID == _DstHolderDataInfo.ModuleInfo.ID)
                    //    return;

                    //==> Remove prev icon highlight
                    if (_SrcHolderDataInfo != null)
                        _SrcHolderDataInfo.UserControl.Background = Brushes.Transparent;

                    cassetteDataInfo.SlotNum = slotNum;
                    _SrcHolderDataInfo = cassetteDataInfo;
                    _SrcHolderDataInfo.UserControl.Background = Brushes.SkyBlue;

                }
                else
                {
                    //==> Remove prev icon highlight
                    if (_DstHolderDataInfo != null)
                        _DstHolderDataInfo.UserControl.Background = Brushes.Transparent;

                    cassetteDataInfo.SlotNum = slotNum;
                    _DstHolderDataInfo = cassetteDataInfo;
                    _DstHolderDataInfo.UserControl.Background = Brushes.SkyBlue;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return true;
        }
        private bool HolderStatusCheck(bool isSource, HolderDataInfo holderDataInfo)
        {
            try
            {
                if (isSource)
                {
                    if (holderDataInfo.HolderInfo.WaferStatus != EnumSubsStatus.EXIST)
                        return false;

                    //==> Source and destination is same
                    if (_DstHolderDataInfo != null && holderDataInfo.ModuleInfo.ID == _DstHolderDataInfo.ModuleInfo.ID)
                        return false;

                    //==> Remove prev icon highlight
                    if (_SrcHolderDataInfo != null)
                        _SrcHolderDataInfo.UserControl.Background = Brushes.Transparent;

                    _SrcHolderDataInfo = holderDataInfo;
                    _SrcHolderDataInfo.UserControl.Background = Brushes.SkyBlue;
                }
                else
                {
                    //==> Remove prev icon highlight
                    if (_DstHolderDataInfo != null)
                        _DstHolderDataInfo.UserControl.Background = Brushes.Transparent;

                    _DstHolderDataInfo = holderDataInfo;
                    _DstHolderDataInfo.UserControl.Background = Brushes.SkyBlue;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return true;
        }
        #region ==> OLD
        //private void HandleIconClickCommandFunc(object param)
        //{
        //    var handleIcon = param as HandleIconBase;
        //    if (handleIcon == null)
        //        return;

        //    //=> source
        //    if (handleIcon.IsSource)
        //    {
        //        if (handleIcon is CassetteIcon)
        //        {
        //            CassetteIcon cstIcon = handleIcon as CassetteIcon;
        //            //1. TODO : slot num 선택할수 있게처리
        //            //Slot number input
        //            cstIcon.SlotNum = VirtualKeyboard.Show(cstIcon.SlotNum, KB_TYPE.DECIMAL, 1, 2);

        //            //Check input value
        //            if (Convert.ToInt32(cstIcon.SlotNum) < 1)
        //            {
        //                cstIcon.SlotNum = "1";
        //            }
        //            else if (Convert.ToInt32(cstIcon.SlotNum) > 25)
        //            {
        //                cstIcon.SlotNum = "25";
        //            }
        //        }

        //        if (handleIcon.GetSelectedSubstrate() != null)
        //        {
        //            //Highlight
        //            if (SrcHandleIconVM != null)
        //            {
        //                SrcHandleIconVM.UnhighlightIconColor();
        //            }

        //            handleIcon.HighlightIconColor();
        //            SrcHandleIconVM = handleIcon;

        //            //Draw Arrow if Dst exist
        //            if (SrcHandleIconVM != null && DstHandleIconVM != null)
        //            {
        //                DrawArrowLine();
        //            }
        //        }
        //        else
        //        {
        //            //TODO : 소스는 웨이퍼가 존재해야한다.
        //            if (SrcHandleIconVM != null)
        //            {
        //                SrcHandleIconVM.UnhighlightIconColor();
        //                SrcHandleIconVM = null;
        //            }
        //        }

        //    }
        //    else  //=> destination
        //    {
        //        if (handleIcon is CassetteIcon)
        //        {
        //            CassetteIcon cstIcon = handleIcon as CassetteIcon;
        //            //1. TODO : slot num 선택할수 있게처리

        //            // slot number input
        //            cstIcon.SlotNum = VirtualKeyboard.Show(cstIcon.SlotNum, KB_TYPE.DECIMAL, 1, 2);

        //            // check input value
        //            if (Convert.ToInt32(cstIcon.SlotNum) < 1)
        //            {
        //                cstIcon.SlotNum = "1";
        //            }
        //            else if (Convert.ToInt32(cstIcon.SlotNum) > 25)
        //            {
        //                cstIcon.SlotNum = "25";
        //            }
        //        }

        //        if (handleIcon.GetSelectedSubstrate() == null)
        //        {
        //            //Highlight
        //            if (DstHandleIconVM != null)
        //            {
        //                DstHandleIconVM.UnhighlightIconColor();
        //            }

        //            handleIcon.HighlightIconColor();
        //            DstHandleIconVM = handleIcon;

        //            //Draw Arrow if Dst exist
        //            if (SrcHandleIconVM != null && DstHandleIconVM != null)
        //            {
        //                DrawArrowLine();
        //            }
        //        }
        //        else
        //        {
        //            //TODO : 목적지는 웨이퍼가 없어야한다.
        //            if (DstHandleIconVM != null)
        //            {
        //                DstHandleIconVM.UnhighlightIconColor();
        //                DstHandleIconVM = null;
        //            }

        //        }
        //    }

        //}
        #endregion
        #endregion

        #region ==> ThreeLegHeight
        private double _ThreeLegHeight;
        public double ThreeLegHeight
        {
            get { return _ThreeLegHeight; }
            set
            {
                if (value != _ThreeLegHeight)
                {
                    _ThreeLegHeight = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> TransferCmdList
        private ObservableCollection<TransferCommand> _TransferCmdList;
        public ObservableCollection<TransferCommand> TransferCmdList
        {
            get { return _TransferCmdList; }
            set
            {
                if (value != _TransferCmdList)
                {
                    _TransferCmdList = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> SelectedTransferCmd
        private TransferCommand _SelectedTransferCmd;
        public TransferCommand SelectedTransferCmd
        {
            get { return _SelectedTransferCmd; }
            set
            {
                if (value != _SelectedTransferCmd)
                {
                    _SelectedTransferCmd = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> ArrowLineList
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

        private void UpdateScreen()
        {
            try
            {
                //==> Clear Arrow Line
                ArrowLineList = new ObservableCollection<Line>();
                //ArrowLineList.Clear();

                if (_CurSelectedRecipe != null)
                    DrawLineToScreen(_CurSelectedRecipe, Brushes.Yellow);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
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
                _CurSelectedRecipe = null;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private void DrawLineToScreen(TranslateRecipe translateCmd, Brush color)
        {
            try
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
                DrawStraight(srcIconPortPoint, betweenPoint1, color);
                DrawStraight(betweenPoint1, betweenPoint2, color);
                DrawStraight(betweenPoint2, endIconPortPoint, color);
                DrawArrowHeader(betweenPoint2, endIconPortPoint, color);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
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
                throw;
            }
        }
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

                Line arrowLine1 = MakeLine(endPt.X + ax, endPt.Y + ay, endPt.X, endPt.Y, color);
                Line arrowLine2 = MakeLine(endPt.X - ay, endPt.Y + ax, endPt.X, endPt.Y, color);
                ArrowLineList.Add(arrowLine1);
                ArrowLineList.Add(arrowLine2);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
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
                throw;
            }

            return line;
        }
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    TransferCmdList = new ObservableCollection<TransferCommand>();
                    LoaderController = this.LoaderController() as ILoaderControllerExtension;
                    CommandManager = this.CommandManager();

                    ArrowLineList = new ObservableCollection<Line>();
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

        }
        public Task<EventCodeEnum> InitViewModel()
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            LoaderTestOption option = new LoaderTestOption();
            LoaderController.LoaderService.SetLoaderTestOption(option);
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public EventCodeEnum InitPage(object parameter = null)
        {
            return EventCodeEnum.NONE;
        }
        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
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
    public abstract class ModuleDataInfoBase
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
                throw;
            }
        }
        public Point GetIconPortPoint()
        {
            //==> 여기서 익셉션 난다면...
            //==> 주로 UserControl이 Visual이 아니라서 익셉션이 나는것이 아니다.
            //==> 이미 지워진 UserControl을 사용해서 문제가 되는 것이다, Icon은 Loader의 Holder 객체들과 바인딩 되어있는데 
            //==> Holder 객체가 새로 채워질때마 새로운 UI가 추가되서 문제가 발생 하는 것이다.

            Point portPoint = new Point(0, 0);
            try
            {

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

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return portPoint;
        }
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
                throw;
            }
            return iconCenter;
        }
        public abstract TransferObject GetSelectedSubstrate();
        public abstract ModuleID GetSelectedModuleID();
    }
    public class HolderDataInfo : ModuleDataInfoBase
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
            if (HolderInfo.WaferStatus == ProberInterfaces.EnumSubsStatus.EXIST)
                return HolderInfo.Substrate;
            else
                return null;
        }
        public override ModuleID GetSelectedModuleID()
        {
            ModuleID id;
            try
            {
                id = HolderInfo.ID;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return id;
        }
    }
    public class CassetteDataInfo : ModuleDataInfoBase
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
                if (CassetteInfo.ScanState == CassetteScanStateEnum.READ)
                {

                    var selSlot = CassetteInfo.SlotModules.Where(item => item.ID.Index == SlotNum).FirstOrDefault();
                    if (selSlot.WaferStatus == ProberInterfaces.EnumSubsStatus.EXIST)
                    {
                        selSub = selSlot.Substrate;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return selSub;
        }
        public override ModuleID GetSelectedModuleID()
        {
            HolderModuleInfo selSlot = null;
            if (CassetteInfo.ScanState == CassetteScanStateEnum.READ)
            {
                selSlot = CassetteInfo.SlotModules.Where(item => item.ID.Index == SlotNum).FirstOrDefault();
            }
            ModuleID id;
            try
            {
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
                throw;
            }
            return id;
        }
    }
    public class TransferCommand : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ==> CmdDesc
        private String _CmdDesc;
        public String CmdDesc
        {
            get { return _CmdDesc; }
            set
            {
                if (value != _CmdDesc)
                {
                    _CmdDesc = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        public int SlotNum { get; set; }
        public int SlotCount { get; set; }
        public ModuleTypeEnum ModuleType { get; set; }
        private ILoaderControllerExtension _LoaderController;
        private int _ModuleIndex;

        public TransferCommand()
        {

        }
        public TransferCommand(ILoaderControllerExtension loaderController, ModuleDataInfoBase moduleData, String cmdDesc)
        {
            try
            {
                _LoaderController = loaderController;
                CmdDesc = cmdDesc;

                HolderDataInfo holderData = moduleData as HolderDataInfo;
                CassetteDataInfo cassetteData = moduleData as CassetteDataInfo;
                if (holderData != null)
                    ModuleType = holderData.HolderInfo.ID.ModuleType;

                else if (cassetteData != null)
                {
                    SlotNum = cassetteData.SlotNum;
                    SlotCount = cassetteData.CassetteInfo.SlotModules.Length;
                    CmdDesc = $"{CmdDesc} : {SlotNum}";
                    ModuleType = ModuleTypeEnum.CST;

                }

                GetIndex(moduleData.ModuleInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void GetIndex(ModuleInfoBase moduleInfo)
        {
            try
            {
                ModuleInfoBase[] moduleInfoBases = GetModuleInfoBases();
                int index = 0;
                for (int i = 0; i < moduleInfoBases.Length; i++)
                {
                    if (moduleInfoBases[i] == moduleInfo)
                    {
                        _ModuleIndex = i;
                        break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private ModuleInfoBase[] GetModuleInfoBases()
        {
            ModuleInfoBase[] moduleInfoBases = null;
            try
            {
                switch (ModuleType)
                {
                    case ModuleTypeEnum.CST:
                        moduleInfoBases = _LoaderController.LoaderInfo.StateMap.CassetteModules;
                        break;
                    case ModuleTypeEnum.ARM:
                        moduleInfoBases = _LoaderController.LoaderInfo.StateMap.ARMModules;
                        break;
                    case ModuleTypeEnum.PA:
                        moduleInfoBases = _LoaderController.LoaderInfo.StateMap.PreAlignModules;
                        break;
                    case ModuleTypeEnum.FIXEDTRAY:
                        moduleInfoBases = _LoaderController.LoaderInfo.StateMap.FixedTrayModules;
                        break;
                    case ModuleTypeEnum.INSPECTIONTRAY:
                        moduleInfoBases = _LoaderController.LoaderInfo.StateMap.InspectionTrayModules;
                        break;
                    case ModuleTypeEnum.CHUCK:
                        moduleInfoBases = _LoaderController.LoaderInfo.StateMap.ChuckModules;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return moduleInfoBases;
        }
        public ModuleID GetdModuleID()
        {
            ModuleID id = ModuleID.UNDEFINED;
            try
            {

                ModuleInfoBase[] moduleInfoBases = GetModuleInfoBases();

                if (moduleInfoBases == null)
                    return id;

                do
                {
                    if (ModuleType is ModuleTypeEnum.CST)
                    {
                        CassetteModuleInfo cassette = moduleInfoBases[_ModuleIndex] as CassetteModuleInfo;
                        if (cassette == null)
                            break;

                        HolderModuleInfo slot = null;
                        if (cassette.ScanState == CassetteScanStateEnum.READ)
                            slot = cassette.SlotModules.Where(item => item.ID.Index == SlotNum).FirstOrDefault();

                        if (slot == null)
                            id = ModuleID.UNDEFINED;
                        else
                            id = slot.ID;
                        break;
                    }
                    else
                    {
                        HolderModuleInfo holder = moduleInfoBases[_ModuleIndex] as HolderModuleInfo;
                        if (holder == null)
                            break;

                        id = holder.ID;
                        break;
                    }
                } while (false);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return id;
        }
    }
}
