using LoaderMapView;
using LoaderParameters.Data;
using LogModule;
using System;
using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ProberInterfaces;
using LoaderBase;
using ProberErrorCode;
using MetroDialogInterfaces;

namespace ProberViewModel
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        public MainWindow()
        {
            InitializeComponent();
        }

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        private TransferObject _WaferObj;
        public TransferObject WaferObj
        {
            get { return _WaferObj; }
            set
            {
                if (value != _WaferObj)
                {
                    _WaferObj = value;
                    RaisePropertyChanged();
                }
            }
        }
        void ToolWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                
                // Code to remove close box from window
                var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private TransferObject _CardObj;
        public TransferObject CardObj
        {
            get { return _CardObj; }
            set
            {
                if (value != _CardObj)
                {
                    _CardObj = value;
                    RaisePropertyChanged();
                }
            }
        }
        private String _OriginModule;
        public String OriginModule
        {
            get { return _OriginModule; }
            set
            {
                if (value != _OriginModule)
                {
                    _OriginModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        private String _FoupID;
        public String FoupID
        {
            get { return _FoupID; }
            set
            {
                if (value != _FoupID)
                {
                    _FoupID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private String _MissingStr;
        public String MissingStr
        {
            get { return _MissingStr; }
            set
            {
                if (value != _MissingStr)
                {
                    _MissingStr = value;
                    RaisePropertyChanged();
                }
            }
        }
        ILoaderSupervisor LoaderMaster = null;
        ModuleTypeEnum ModuleType = ModuleTypeEnum.UNDEFINED;
        int ModuleNum = 0;

        public MainWindow(ModuleTypeEnum moduleType, object item, ILoaderSupervisor loaderMaster) : this()
        {
            try
            {
                
                DataContext = this;
                LoaderMaster = loaderMaster;
                WaferInfoGrid.Visibility = Visibility.Collapsed;
                PolishWaferInfoGrid.Visibility = Visibility.Collapsed;
                CardInfoGrid.Visibility = Visibility.Collapsed;
                MissingBtnGrid.Visibility = Visibility.Collapsed;
                VacuumBtnGrid.Visibility = Visibility.Collapsed;
                //MissingInfoGrid.Visibility = Visibility.Collapsed;
                //   UndefinedMenu.Visibility = Visibility.Collapsed;
                //      UndefineRow.Height=GridLength.Auto;

                // OkbtnRow.Height = GridLength.Auto;
                TransferObj = item;
                ModuleType = moduleType;
                bool isWaferUnknown = false;
                if (moduleType == ModuleTypeEnum.SLOT)
                {
                    var SlotObj = item as SlotObject;
                    WaferObj = SlotObj.WaferObj;
                    ModuleNum = (SlotObj.FoupNumber * 25) + SlotObj.Index;
                    if (SlotObj.WaferStatus==ProberInterfaces.EnumSubsStatus.UNKNOWN)
                    {
                        MissingBtnGrid.Visibility = Visibility.Visible;
                        isWaferUnknown = true;
                    }
                    else
                    {
                        MissingBtnGrid.Visibility = Visibility.Collapsed;
                        WaferInfoGrid.Visibility = Visibility.Visible;
                    }
             //       UndefinedMenu.Visibility = Visibility.Collapsed;

                 
                }
                else if (moduleType == ModuleTypeEnum.ARM)
                {
                    var ArmObj = item as ArmObject;
                    WaferObj = ArmObj.WaferObj;
                    ModuleNum = ArmObj.Index;
                    if (ArmObj.WaferStatus == ProberInterfaces.EnumSubsStatus.UNKNOWN)
                    {
                        MissingBtnGrid.Visibility = Visibility.Visible;
                   //     MissingInfoGrid.Visibility = Visibility.Visible;
                        isWaferUnknown = true;
                        //      UndefinedMenu.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        MissingBtnGrid.Visibility = Visibility.Collapsed;
                        WaferInfoGrid.Visibility = Visibility.Visible;

                    }
                }
                else if (moduleType == ModuleTypeEnum.PA)
                {
                    var PAObj = item as PAObject;
                    WaferObj = PAObj.WaferObj;
                    ModuleNum = PAObj.Index;

                    if (LoaderMaster.Loader.PAManager.PAModules[ModuleNum - 1].State.PAAlignAbort == true
                         || PAObj.WaferStatus == ProberInterfaces.EnumSubsStatus.UNKNOWN
                         || PAObj.WaferStatus == ProberInterfaces.EnumSubsStatus.UNDEFINED
                         || PAObj.WaferObj.Size.Value == SubstrateSizeEnum.UNDEFINED
                         || PAObj.PAStatus == ProberInterfaces.PreAligner.EnumPAStatus.Error)
                    {
                        MissingBtnGrid.Visibility = Visibility.Visible;
                        WaferInfoGrid.Visibility = Visibility.Visible;
                        VacuumBtnGrid.Visibility = Visibility.Visible;
                        if (LoaderMaster.Loader.PAManager.PAModules[ModuleNum - 1].State.PAAlignAbort != true) 
                        {
                            ExistBtn.Visibility = Visibility.Visible;
                        }
                        //NotExistBtn.Visibility = Visibility.Collapsed;
                        //  MissingInfoGrid.Visibility = Visibility.Visible;
                        isWaferUnknown = true;
                        //      UndefinedMenu.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        MissingBtnGrid.Visibility = Visibility.Collapsed;
                        WaferInfoGrid.Visibility = Visibility.Visible;

                    }
                }
                else if (moduleType == ModuleTypeEnum.BUFFER)
                {
                    var BufferObj = item as BufferObject;
                    WaferObj = BufferObj.WaferObj;
                    ModuleNum = BufferObj.Index;
                    if (BufferObj.WaferStatus == ProberInterfaces.EnumSubsStatus.UNKNOWN)
                    {
                        MissingBtnGrid.Visibility = Visibility.Visible;
                      //  MissingInfoGrid.Visibility = Visibility.Visible;
                        isWaferUnknown = true;
                        //      UndefinedMenu.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        MissingBtnGrid.Visibility = Visibility.Collapsed;
                        WaferInfoGrid.Visibility = Visibility.Visible;

                    }
                }
                else if (moduleType == ModuleTypeEnum.CHUCK)
                {
                    var Chuck = item as StageObject;
                    ModuleNum = Chuck.Index;
                
                    if (Chuck.WaferStatus == ProberInterfaces.EnumSubsStatus.UNKNOWN)
                    {
                        MissingBtnGrid.Visibility = Visibility.Visible;
                        WaferObj = LoaderMaster.GetTransferObjectToSlotInfo(Chuck.Index);

                        // MissingInfoGrid.Visibility = Visibility.Visible;
                        WaferInfoGrid.Visibility = Visibility.Visible;
                        isWaferUnknown = true;
                        //      UndefinedMenu.Visibility = Visibility.Visible;
                    }
                    else if(Chuck.CardStatus == ProberInterfaces.EnumSubsStatus.UNKNOWN)
                    {
                        MissingBtnGrid.Visibility = Visibility.Visible;
                       // MissingInfoGrid.Visibility = Visibility.Visible;
                        isWaferUnknown = false;
                    }
                    else if (Chuck.IsWaferOnHandler == true)
                    {
                        MissingBtnGrid.Visibility = Visibility.Visible;
                        WaferObj = LoaderMaster.GetTransferObjectToSlotInfo(Chuck.Index);

                        // MissingInfoGrid.Visibility = Visibility.Visible;
                        WaferInfoGrid.Visibility = Visibility.Visible;
                        isWaferUnknown = true;
                    }
                    else
                    {
                        MissingBtnGrid.Visibility = Visibility.Collapsed;
                        if (Chuck.WaferStatus == ProberInterfaces.EnumSubsStatus.EXIST)
                        {
                            WaferObj = Chuck.WaferObj;
                            WaferInfoGrid.Visibility = Visibility.Visible;
                        }
                        if (Chuck.CardStatus == ProberInterfaces.EnumSubsStatus.EXIST)
                        {
                            CardObj = Chuck.CardObj;
                            CardInfoGrid.Visibility = Visibility.Visible;
                        }
                    }

                }
                else if (moduleType == ModuleTypeEnum.FIXEDTRAY)
                {
                    if (item is FixedTrayInfoObject)
                    {
                        var FixedTrayObj = item as FixedTrayInfoObject;
                        WaferObj = FixedTrayObj.WaferObj;
                        ModuleNum = FixedTrayObj.Index;
                        if (FixedTrayObj.WaferStatus == ProberInterfaces.EnumSubsStatus.UNKNOWN)
                        {
                            MissingBtnGrid.Visibility = Visibility.Visible;
                            isWaferUnknown = true;
                        }
                        else
                        {
                            MissingBtnGrid.Visibility = Visibility.Collapsed;
                            WaferInfoGrid.Visibility = Visibility.Visible;

                        }
                    }
                    else if (item is FixedTrayObject)
                    {
                        var FixedTrayObj = item as FixedTrayObject;
                        WaferObj = FixedTrayObj.DeviceInfo;
                        ModuleNum = FixedTrayObj.WaferSupplyInfo.ID.Index;
                        MissingBtnGrid.Visibility = Visibility.Collapsed;
                        WaferInfoGrid.Visibility = Visibility.Collapsed;
                        PolishWaferInfoGrid.Visibility = Visibility.Visible;
                    }

                }
                else if (moduleType == ModuleTypeEnum.INSPECTIONTRAY)
                {
                    if (item is InspectionTrayInfoObject)
                    {
                        var INSPTrayObj = item as InspectionTrayInfoObject;
                        WaferObj = INSPTrayObj.WaferObj;
                        ModuleNum = INSPTrayObj.Index;
                        // INSP 는 사용자 접근 가능 모듈
                        MissingBtnGrid.Visibility = Visibility.Collapsed;
                        WaferInfoGrid.Visibility = Visibility.Visible;
                    }
                    else if (item is InspectionTrayObject)
                    {
                        var INSPTrayObj = item as InspectionTrayObject;
                        WaferObj = INSPTrayObj.DeviceInfo;
                        ModuleNum = INSPTrayObj.WaferSupplyInfo.ID.Index;
                        MissingBtnGrid.Visibility = Visibility.Collapsed;
                        WaferInfoGrid.Visibility = Visibility.Collapsed;
                        PolishWaferInfoGrid.Visibility = Visibility.Visible;
                    }
                }
                else if (moduleType == ModuleTypeEnum.CARDTRAY)
                {
                    var CardTrayObj = item as CardTrayObject;
                    CardObj = CardTrayObj.CardObj;
                    ModuleNum = CardTrayObj.Index;

                    if (CardTrayObj.WaferStatus == EnumSubsStatus.UNKNOWN)
                    {
                        MissingBtnGrid.Visibility = Visibility.Visible;
                        //MissingInfoGrid.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        MissingBtnGrid.Visibility = Visibility.Collapsed;
                        WaferInfoGrid.Visibility = Visibility.Visible;
                        CardInfoGrid.Visibility = Visibility.Visible;
                    }
                }
                else if (moduleType == ModuleTypeEnum.CARDBUFFER)
                {
                    var CardBufferObj = item as CardBufferObject;
                    CardObj = CardBufferObj.CardObj;
                    ModuleNum = CardBufferObj.Index;
                    if (CardBufferObj.WaferStatus == EnumSubsStatus.UNKNOWN)
                    {
                        MissingBtnGrid.Visibility = Visibility.Visible;
                       // MissingInfoGrid.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        MissingBtnGrid.Visibility = Visibility.Collapsed;
                        WaferInfoGrid.Visibility = Visibility.Visible;
                        CardInfoGrid.Visibility = Visibility.Visible;
                    }
                }
                else if (moduleType == ModuleTypeEnum.CARDARM)
                {
                    var CardArmObj = item as CardArmObject;
                    CardObj = CardArmObj.CardObj;
                    ModuleNum = CardArmObj.Index;
                    if (CardArmObj.WaferStatus == EnumSubsStatus.UNKNOWN)
                    {
                        MissingBtnGrid.Visibility = Visibility.Visible;
                       // MissingInfoGrid.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        MissingBtnGrid.Visibility = Visibility.Collapsed;
                        WaferInfoGrid.Visibility = Visibility.Visible;
                        CardInfoGrid.Visibility = Visibility.Visible;
                    }
                }

                if (WaferObj != null)
                {
                    int slotIdx = WaferObj.OriginHolder.Index % 25;
                    if(slotIdx==0)
                    {
                        slotIdx = 25;
                    }
                    int foupNum = ((WaferObj.OriginHolder.Index-1) / 25) + 1;
                    OriginModule = WaferObj.OriginHolder.ModuleType + " " + slotIdx;
                    if (WaferObj.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                    {
                        FoupID = "FOUP " + foupNum;
                    }
                }
                
                if (CardInfoGrid.Visibility==Visibility.Collapsed&& WaferInfoGrid.Visibility == Visibility.Visible)
                {
                    CardColum.Width = GridLength.Auto;
                    Width = 600;
                }
                else if(CardInfoGrid.Visibility == Visibility.Visible && WaferInfoGrid.Visibility == Visibility.Collapsed)
                {
                    WaferColum.Width = GridLength.Auto;
                    Width = 600;
                }

                if (PolishWaferInfoGrid.Visibility == Visibility.Collapsed)
                {
                    PolishWaferColum.Width = GridLength.Auto;
                    //Width = 600;
                }
                else if (PolishWaferInfoGrid.Visibility == Visibility.Visible)
                {
                    WaferColum.Width = GridLength.Auto;
                    CardColum.Width = GridLength.Auto;
                    Width = 600;
                }
                //if (MissingInfoGrid.Visibility == Visibility.Visible)
                //{
                //    string transferStr = isWaferUnknown?"Wafer":"Card";
                //    MissingStr = $"{transferStr} status is Unknown to {moduleType}{ModuleNum}.\nPlease check the current status \nand then select it.";
                //    Height = 450;
                //}
                //if(MissingBtnGrid.Visibility==Visibility.Visible)
                //{
                    
                //    Height = 800;
                //}
                ResizeMode = ResizeMode.NoResize;
                WindowStartupLocation = WindowStartupLocation.Manual;
                Loaded += ToolWindow_Loaded;
                //ResizeMode = ResizeMode.NoResize;
                Topmost = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private object _TransferObj;
        public object TransferObj
        {
            get { return _TransferObj; }
            set
            {
                if (value != _TransferObj)
                {
                    _TransferObj = value;
                    RaisePropertyChanged();
                }
            }
        }



        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void ExistBtn_Click(object sender, RoutedEventArgs e)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            string errorMessage = "";
            if (LoaderMaster!=null)
            {
                (retVal, errorMessage) = await LoaderMaster.RecoveryUnknownStatus(ModuleType, ModuleNum, EnumSubsStatus.EXIST);
                if(retVal == EventCodeEnum.NONE)
                {
                     LoaderMaster.MetroDialogManager().ShowMessageDialog(
                                "Unknown Recovery Success",
                                "Current Wafer Status: EXIST", EnumMessageStyle.Affirmative);
                }
                else
                {
                    LoaderMaster.MetroDialogManager().ShowMessageDialog(
                                "Unknown Recovery Error",
                                $"Please check the Wafer Status again.\n{errorMessage}", EnumMessageStyle.Affirmative);
                }
            }
            this.Close();
        }

        private async void NotExistBtn_Click(object sender, RoutedEventArgs e)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            string errorMessage = "";
            if (LoaderMaster != null)
            {
                (retVal, errorMessage) = await LoaderMaster.RecoveryUnknownStatus(ModuleType, ModuleNum, EnumSubsStatus.NOT_EXIST);
                if (retVal == EventCodeEnum.NONE)
                {
                    LoaderMaster.MetroDialogManager().ShowMessageDialog(
                                   "Unknown Recovery Success",
                                   "Current Wafer Status: NOT EXIST", EnumMessageStyle.Affirmative);

                }
                else
                {
                    LoaderMaster.MetroDialogManager().ShowMessageDialog(
                                "Unknown Recovery Error",
                                $"Please check the Wafer Status again.\n{errorMessage}", EnumMessageStyle.Affirmative);
                }
            }
           
            this.Close();
        }

        private async void VacOffBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                bool isexstwaferonpa = false;
                retVal = LoaderMaster.Loader.PAManager.PAModules[ModuleNum - 1].IsSubstrateExist(out isexstwaferonpa);
                if (retVal == EventCodeEnum.NONE)
                {
                    if (isexstwaferonpa == true)
                    {
                        retVal = LoaderMaster.Loader.PAManager.PAModules[ModuleNum - 1].ReleaseSubstrate();
                        if (retVal == EventCodeEnum.NONE)
                        {
                            EnumMessageDialogResult result;
                            result = await LoaderMaster.MetroDialogManager().ShowMessageDialog( "Wafer exists in Pa", "Remove the wafer on the pa manually and click the ok button.", EnumMessageStyle.Affirmative);
                            if (result == EnumMessageDialogResult.AFFIRMATIVE)
                            {
                                retVal = LoaderMaster.Loader.PAManager.PAModules[ModuleNum - 1].IsSubstrateExist(out isexstwaferonpa);
                                if (retVal == EventCodeEnum.NONE)
                                {
                                    if (isexstwaferonpa == true)
                                    {
                                        LoaderMaster.MetroDialogManager().ShowMessageDialog( "Wafer exists in Pa", "Wafer still exists in PA.", EnumMessageStyle.Affirmative);
                                    }
                                    else
                                    {
                                        retVal = LoaderMaster.SkipUnknownWaferLocation(ModuleType, ModuleNum);
                                        if (retVal == EventCodeEnum.NONE)
                                        {
                                            LoaderMaster.MetroDialogManager().ShowMessageDialog("PA Release Success", "Successfully recovered PA.", EnumMessageStyle.Affirmative);
                                            LoaderMaster.Loader.PAManager.PAModules[ModuleNum - 1].State.PAAlignAbort = false;
                                        }
                                    }
                                }
                                else
                                {
                                    LoaderMaster.MetroDialogManager().ShowMessageDialog( "PA unavailable", "Please check the PA status.", EnumMessageStyle.Affirmative);
                                }
                            }
                        }
                        else
                        {
                            LoaderMaster.MetroDialogManager().ShowMessageDialog( "PA unavailable", "Please check the PA status.", EnumMessageStyle.Affirmative);
                        }
                    }
                    else
                    {
                        retVal = LoaderMaster.SkipUnknownWaferLocation(ModuleType, ModuleNum);
                        if (retVal == EventCodeEnum.NONE)
                        {
                            LoaderMaster.MetroDialogManager().ShowMessageDialog("PA Release Success", "Successfully recovered PA.", EnumMessageStyle.Affirmative);
                            LoaderMaster.Loader.PAManager.PAModules[ModuleNum - 1].State.PAAlignAbort = false;
                        }
                    }
                }
                else
                {
                    LoaderMaster.MetroDialogManager().ShowMessageDialog( "PA unavailable", "Please check the PA status.", EnumMessageStyle.Affirmative);
                }

                this.Close();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally 
            {
                LoaderMaster.Loader.BroadcastLoaderInfo();
            }
        }
    }
}
