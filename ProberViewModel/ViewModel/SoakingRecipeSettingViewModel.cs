using Autofac;
using LoaderBase.Communication;
using LoaderStatusSoakingSettingVM;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using RecipeEditorControl.RecipeEditorParamEdit;
using RelayCommandBase;
using SoakingParameters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SoakingRecipeSettingViewModel
{
    public class SoakingType : INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        
        private SoakingParamBase _SoakingInstance;
        public SoakingParamBase SoakingInstance
        {
            get { return _SoakingInstance; }
            set
            {
                if (value != _SoakingInstance)
                {
                    _SoakingInstance = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _EventSoakingName;
        public string EventSoakingName
        {
            get { return _EventSoakingName; }
            set
            {
                if (value != _EventSoakingName)
                {
                    _EventSoakingName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Enable;
        public bool Enable
        {
            get { return _Enable; }
            set
            {
                if (value != _Enable)
                {
                    _Enable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _SoakingTime;
        public int SoakingTime
        {
            get { return _SoakingTime; }
            set
            {
                if (value != _SoakingTime)
                {
                    _SoakingTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ZClearance;
        public double ZClearance
        {
            get { return _ZClearance; }
            set
            {
                if (value != _ZClearance)
                {
                    _ZClearance = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PinCoordinate _Pintolerance;
        public PinCoordinate Pintolerance
        {
            get { return _Pintolerance; }
            set
            {
                if (value != _Pintolerance)
                {
                    _Pintolerance = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region ==> EnableClickCommand
        private RelayCommand<Object> _EnableClickCommand;
        public ICommand EnableClickCommand
        {
            get
            {
                if (null == _EnableClickCommand) _EnableClickCommand = new RelayCommand<Object>(NCEnableClickCommandFunc);
                return _EnableClickCommand;
            }
        }
        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;


            LoggerManager.Debug("____________Save Soaking Devparameter____________");

            retVal = this.SoakingModule().SaveDevParameter();

            return retVal;
        }
        private void NCEnableClickCommandFunc(Object param)
        {
            SaveDevParameter();
        }

        //#region ==> TextBoxClickCommand
        //private RelayCommand<Object> _TextBoxClickCommand;
        //public ICommand TextBoxClickCommand
        //{
        //    get
        //    {
        //        if (null == _TextBoxClickCommand) _TextBoxClickCommand = new RelayCommand<Object>(TextBoxClickCommandFunc);
        //        return _TextBoxClickCommand;
        //    }
        //}
        //private void TextBoxClickCommandFunc(Object param)
        //{
        //    try
        //    {
        //        if (SoakingInstance.EventSoakingType.Value == EnumSoakingType.PREHAT_SOAK1 ||
        //            SoakingInstance.EventSoakingType.Value == EnumSoakingType.PREHAT_SOAK2)
        //        {
        //            System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
        //            tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 100);
        //            if (tb.Name == "ZClearance")
        //            {
        //                var value = Convert.ToDouble(tb.Text);
        //                if (value > -200)
        //                {
        //                    LoggerManager.Debug($"Can't Input ZClearance Value {SoakingInstance.ZClearance.Value}");
        //                    SoakingInstance.ZClearance.Value = -1000;
        //                    tb.Text = Convert.ToString(-1000);
        //                }
        //                else
        //                {
        //                    tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
        //                    SaveDevParameter();
        //                }
        //            }
        //            tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
        //            SaveDevParameter();
        //        }

        //        else
        //        {
        //            System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
        //            tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 100);
        //            tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
        //            SaveDevParameter();
        //        }
        //    }

        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);

        //    }

        //}
        //#endregion

        #endregion

    }
    public class SoakingRecipeSettingViewModel : IMainScreenViewModel, ISoakingRecipeSettingVM, IParamScrollingViewModel, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

        //private ObservableCollection<SoakingType> _SoakingTypeList
        //    = new ObservableCollection<SoakingType>();
        //public ObservableCollection<SoakingType> SoakingTypeList
        //{
        //    get { return _SoakingTypeList; }
        //    set
        //    {
        //        if (value != _SoakingTypeList)
        //        {
        //            _SoakingTypeList = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        public SoakingRecipeSettingViewModel()
        {
            StatusSoakingSettingViewGuid = new Guid("DA7462FF-9C5B-4E8A-B8C3-DAD82523A9F6");
        }
        private ILoaderCommunicationManager loaderCommunicationManager = null;
        private ObservableCollection<SoakingParamBase> _SoakingTypeList = new ObservableCollection<SoakingParamBase>();
        public ObservableCollection<SoakingParamBase> SoakingTypeList
        {
            get { return _SoakingTypeList; }
            set
            {
                if (value != _SoakingTypeList)
                {
                    _SoakingTypeList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private readonly Guid _ViewModelGUID = new Guid("48221362-A0EA-4BD1-81D5-E77895E939BF");
        public Guid ScreenGUID
        {
            get { return _ViewModelGUID; }
        }
        private Guid StatusSoakingSettingViewGuid;

        private const int _EventSoakingDevCatID = 10072001;
        private const int _AutoSoakingDevCatID = 10072002;
        private const int _StatusSoakingDevCatID = 10072003;
        private const int _CommonSoakingDevCatID = 10072004;
        public SoakingDeviceFile SoakingDevParam { get; set; }

        private RecipeEditorParamEditViewModel _RecipeEditorParamEdit;
        public RecipeEditorParamEditViewModel RecipeEditorParamEdit
        {
            get { return _RecipeEditorParamEdit; }
            set
            {
                if (value != _RecipeEditorParamEdit)
                {
                    _RecipeEditorParamEdit = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<string> _EventSoakingTypelist = new ObservableCollection<string>();
        public ObservableCollection<string> EventSoakingTypelist
        {
            get { return _EventSoakingTypelist; }
            set
            {
                if (value != _EventSoakingTypelist)
                {
                    _EventSoakingTypelist = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _SelectedItem;
        public string SelectedItem
        {
            get { return _SelectedItem; }
            set
            {
                if (value != _SelectedItem)
                {
                    _SelectedItem = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _TabControlSelectedIndex = 0;
        public int TabControlSelectedIndex
        {
            get { return _TabControlSelectedIndex; }
            set
            {
                if (value != _TabControlSelectedIndex)
                {
                    _TabControlSelectedIndex = value;

                    ChangeRecipeEditorItems();

                    RaisePropertyChanged();
                }
            }
        }
        private StatusSoakingDeviceFile _StatusSoakingDeviceFileObj;
        public StatusSoakingDeviceFile StatusSoakingDeviceFileObj
        {
            get { return _StatusSoakingDeviceFileObj; }
            set
            {
                if (value != _StatusSoakingDeviceFileObj)
                {
                    _StatusSoakingDeviceFileObj = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _ShowStatusSoaking = false;
        public bool ShowStatusSoaking
        {
            get { return _ShowStatusSoaking; }
            set
            {
                // Lot이 Idle 상태가 아니면 Status <-> State Soaking 상태를 변경할 수 없다.
                if (!IsSelectedStageIdleState())
                {
                    return;
                }

                if (value != _ShowStatusSoaking)
                {
                    _ShowStatusSoaking = value;

                    ChangeRecipeEditorItems();
                    SetTraceLastSoakingStateInfo();

                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsToggleBtnEnabled = true;
        public bool IsToggleBtnEnabled
        {
            get { return _IsToggleBtnEnabled; }
            set
            {
                if (value != _IsToggleBtnEnabled)
                {
                    _IsToggleBtnEnabled = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _ComboBoxVisibility = Visibility.Visible;
        public Visibility ComboBoxVisibility
        {
            get { return _ComboBoxVisibility; }
            set
            {
                if (value != _ComboBoxVisibility)
                {
                    _ComboBoxVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Visibility _StatusSoakVisibility = Visibility.Collapsed;
        public Visibility StatusSoakVisibility
        {
            get { return _StatusSoakVisibility; }
            set
            {
                if (value != _StatusSoakVisibility)
                {
                    _StatusSoakVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Visibility _EventNAutoSoakVisibility = Visibility.Visible;
        public Visibility EventNAutoSoakVisibility
        {
            get { return _EventNAutoSoakVisibility; }
            set
            {
                if (value != _EventNAutoSoakVisibility)
                {
                    _EventNAutoSoakVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }
        private List<String> _EnumExceptTypeName = new List<string>();
        public List<String> EnumExceptTypeName
        {
            get
            {
                return _EnumExceptTypeName;
            }
            set { _EnumExceptTypeName = value; }
        }

        private String _EnumTypeName;
        public String EnumTypeName
        {
            get
            {
                _EnumTypeName = "SoakingParamBase";
                return _EnumTypeName;
            }
            set { _EnumTypeName = value; }
        }

        private List<IElement> _DeviceParamElementList;
        private int _Index;

        private Dictionary<string, string> _EventSoakingPropertyPathMap = new Dictionary<string, string>();
        public bool Initialized { get; set; } = false;
        private bool bNextViewIsStatusSoakingSettingView = false;
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {

                    Initialized = true;
                    //SoakingDevParam = (SoakingDeviceFile)this.SoakingModule().SoakingDeviceFile_IParam;

                    //if (SoakingDevParam != null)
                    //{
                    //    SoakingTypeList = SoakingDevParam.EventSoakingParams;
                    //}

                    //SoakingTypeList = new ObservableCollection<SoakingParamBase>();
                    //var preheat1 = new SoakingType();
                    //var preheat2 = new SoakingType();
                    //var firstwafer = new SoakingType();
                    //var everywafer = new SoakingType();
                    //var lotresume = new SoakingType();
                    //var polishwafer = new SoakingType();

                    //preheat1.SoakingInstance = SoakingDevParam.PreHeatSoaking1;
                    //preheat2.SoakingInstance = SoakingDevParam.PreHeatSoaking2;
                    //firstwafer.SoakingInstance = SoakingDevParam.FirstWaferSoaking;
                    //everywafer.SoakingInstance = SoakingDevParam.EveryWaferSoaking;
                    //lotresume.SoakingInstance = SoakingDevParam.LotResumeSoaking;
                    //polishwafer.SoakingInstance = SoakingDevParam.PolishWaferSoaking;

                    //SoakingTypeList.Add(preheat1);
                    //SoakingTypeList.Add(preheat2);
                    //SoakingTypeList.Add(firstwafer);
                    //SoakingTypeList.Add(everywafer);
                    //SoakingTypeList.Add(lotresume);
                    //SoakingTypeList.Add(polishwafer);

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
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (this.SoakingModule().GetShowStatusSoakingSettingPageToggleValue() && !bNextViewIsStatusSoakingSettingView)
                {
                    (this.ViewModelManager().GetViewModelFromViewGuid(StatusSoakingSettingViewGuid) as StatusSoakingSettingViewModel)?.CheckAvailableStatusSoakingEnable();
                }
                bNextViewIsStatusSoakingSettingView = false;

                retval = this.SoakingModule().SaveDevParameter();
                bool CurrentStatusSoakingUsingFlag = this.SoakingModule().GetCurrentStatusSoakingUsingFlag();
                bool LastSettedStatusSoakingFlag = this.SoakingModule().GetBeforeStatusSoakingUsingFlag();
                if (this.SoakingModule().GetShowStatusSoakingSettingPageToggleValue())
                {
                    if (false == LastSettedStatusSoakingFlag && CurrentStatusSoakingUsingFlag)  //fase에서 true로 옵션을 변경한 경우 Status soaking prepare로 시작한다.
                        this.SoakingModule().ForceChange_PrepareStatus();
                }
                this.SoakingModule().SetBeforeStatusSoakingUsingFlag(CurrentStatusSoakingUsingFlag);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }
        public void DeInitModule()
        {

        }
        public Task<EventCodeEnum> InitViewModel()
        {
            if(EnumExceptTypeName == null)
            {
                EnumExceptTypeName = new List<string>();
            }

            EnumExceptTypeName.Clear();
            EnumExceptTypeName.Add("UNDEFINED");
            EnumExceptTypeName.Add("AUTO_SOAK");

            if (Enum.GetNames(typeof(EnumSoakingType)).Length > 0)
            {
                bool isExcept = false;

                for (int i = 0; i < Enum.GetNames(typeof(EnumSoakingType)).Length; i++)
                {
                    string curTypeEnumString = Enum.GetName(typeof(EnumSoakingType), i);

                    // EnumExceptTypeName 리스트에 존재하는 경우, 제외.
                    
                    if(EnumExceptTypeName.FirstOrDefault(x => x == curTypeEnumString) != null)
                    {
                        isExcept = true;
                    }
                    else
                    {
                        isExcept = false;
                    }

                    if (isExcept == false)
                    {
                        EventSoakingTypelist.Add(curTypeEnumString);
                    }
                }

                EventSoakingTypelist.Add("All");
            }

            RecipeEditorParamEdit = new RecipeEditorParamEditViewModel();

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        private void PopulatePageContents()
        {
            try
            {
                if (_EventSoakingPropertyPathMap.Count <= 0)
                {
                    if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober)
                    {
                        _DeviceParamElementList = this.ParamManager().GetDevElementList();
                    }
                    else
                    {
                        _DeviceParamElementList = this.GetLoaderContainer().Resolve<ILoaderParamManager>().GetDevElementList();
                    }

                    List<IElement> stageAxisLabelElemList = _DeviceParamElementList.
                        Where(item => item.PropertyPath.Contains("SoakingDeviceFile")).ToList();

                    List<IElement> data = new List<IElement>();

                    for (int i = 0; i < EventSoakingTypelist.Count; i++)
                    {
                        //filter = _DeviceParamElementList.Where(item => item.PropertyPath.Contains(enumType)).AsEnumerable();
                        data = stageAxisLabelElemList.Where(item => item.PropertyPath.Contains(i.ToString())).ToList();

                        var type = data.Where(x => x.ElementName == "Soaking Type").FirstOrDefault();

                        if (type != null)
                        {
                            string[] wordsSplit = type.PropertyPath.Split('.');

                            int endIdx = wordsSplit[2].LastIndexOf("[");
                            string key = wordsSplit[2].Substring(0, endIdx);

                            var Enumindex = EventSoakingTypelist.IndexOf(type.GetValue().ToString());

                            _EventSoakingPropertyPathMap.Add(EventSoakingTypelist[Enumindex], key);
                        }
                        else
                        {
                            LoggerManager.Error("Unknown.");
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"MotorsViewModel.PopulatePageContents(): Error occurred. Err = {err.Message}");
            }
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            TabControlSelectedIndex = 0;

            SelectedItem = EventSoakingTypelist[EventSoakingTypelist.Count() - 1].ToString();

            PopulatePageContents();

            int curid;

            if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober)
            {
                _DeviceParamElementList = this.ParamManager().GetDevElementList();
            }
            else
            {
                _DeviceParamElementList = this.GetLoaderContainer().Resolve<ILoaderParamManager>().GetDevElementList();
            }

            _ShowStatusSoaking = this.SoakingModule().GetShowStatusSoakingSettingPageToggleValue();
            ChangeRecipeEditorItems();
            RaisePropertyChanged("ShowStatusSoaking");

            if (ShowStatusSoaking == true)
            {
                curid = _StatusSoakingDevCatID;
                RecipeEditorParamEdit = new RecipeEditorParamEditViewModel();
                RecipeEditorParamEdit.HardCategoryFiltering(curid);
            }
            else if (TabControlSelectedIndex == 0)
            {
                curid = _EventSoakingDevCatID;
                SelectedItemAndCategoryFiltering(EnumTypeName, SelectedItem, curid);
            }
            else if(TabControlSelectedIndex == 1)
            {
                curid = _AutoSoakingDevCatID;
                RecipeEditorParamEdit = new RecipeEditorParamEditViewModel();
                RecipeEditorParamEdit.HardCategoryFiltering(curid);
            }
            else if (TabControlSelectedIndex == 2)
            {
                curid = _CommonSoakingDevCatID;
                RecipeEditorParamEdit = new RecipeEditorParamEditViewModel();
                RecipeEditorParamEdit.HardCategoryFiltering(curid);
            }

            //Status soaking과 기존 구 Event Soaking의 사용여부에 대한 옵션설정은 Maintenance mode면서 Lot Idle일때만 가능하다.(Toggle btton enable/disable처리)
            if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Remote)//SystemExcuteModeEnum.Remote
            {
                if (null == loaderCommunicationManager)
                {
                    this.loaderCommunicationManager = this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
                }
                if (this.loaderCommunicationManager.Cells[loaderCommunicationManager.SelectedStage.Index - 1].StageMode == GPCellModeEnum.MAINTENANCE
                     && IsSelectedStageIdleState())
                {
                    IsToggleBtnEnabled = true;
                }
                else
                    IsToggleBtnEnabled = false;
            }
            else//SystemExcuteModeEnum.Prober
            {
                if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE)
                {
                    IsToggleBtnEnabled = true;
                }
                else
                {
                    IsToggleBtnEnabled = false;
                }
            }

            //Status soaking과 기존 구 Event Soaking의 사용여부에 대한 옵션설정은 Maintenance mode면서 Lot Idle일때만 가능하다.(Toggle btton enable/disable처리)
            if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Remote)//SystemExcuteModeEnum.Remote
            {
                if (null == loaderCommunicationManager)
                {
                    this.loaderCommunicationManager = this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
                }
                if (this.loaderCommunicationManager.Cells[loaderCommunicationManager.SelectedStage.Index - 1].StageMode == GPCellModeEnum.MAINTENANCE
                     && IsSelectedStageIdleState())
                {
                    IsToggleBtnEnabled = true;
                }
                else
                    IsToggleBtnEnabled = false;
            }
            else//SystemExcuteModeEnum.Prober
            {
                if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE)
                {
                    IsToggleBtnEnabled = true;
                }
                else
                {
                    IsToggleBtnEnabled = false;
                }
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);

            //return await Task.Run<EventCodeEnum>(() => 
            //{
            //    TabControlSelectedIndex = 0;

            //    SelectedItem = EventSoakingTypelist[EventSoakingTypelist.Count() - 1].ToString();

            //    PopulatePageContents();

            //    int curid;

            //    _DeviceParamElementList = this.ParamManager().GetDevElementList();

            //    if (TabControlSelectedIndex == 0)
            //    {
            //        curid = _EventSoakingDevCatID;
            //        SelectedItemAndCategoryFiltering(EnumTypeName, SelectedItem, curid);
            //    }
            //    else
            //    {
            //        curid = _AutoSoakingDevCatID;
            //        RecipeEditorParamEdit = new RecipeEditorParamEditViewModel();
            //        RecipeEditorParamEdit.HardCategoryFiltering(curid);
            //    }
            //    return EventCodeEnum.NONE;
            //});
        }

        private async Task ChangeRecipeEditorItems()
        {
            int curid;

            //await this.WaitCancelDialogService().ShowDialog("Updating...");
            await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Updating...");

            try
            {
                if(ShowStatusSoaking == true)
                {
                    ComboBoxVisibility = Visibility.Collapsed;
                    EventNAutoSoakVisibility = Visibility.Collapsed;
                    StatusSoakVisibility = Visibility.Visible;
                    curid = _StatusSoakingDevCatID;
                    RecipeEditorParamEdit = new RecipeEditorParamEditViewModel();

                    RecipeEditorParamEdit.HardCategoryFiltering(curid);
                }
                else
                {
                    StatusSoakVisibility = Visibility.Collapsed;
                    if (TabControlSelectedIndex == 0)
                    {
                        curid = _EventSoakingDevCatID;
                        ComboBoxVisibility = Visibility.Visible;
                        EventNAutoSoakVisibility = Visibility.Visible;

                        SelectedItemAndCategoryFiltering(EnumTypeName, SelectedItem, curid);
                    }
                    else if (TabControlSelectedIndex == 1)
                    {
                        ComboBoxVisibility = Visibility.Hidden;
                        EventNAutoSoakVisibility = Visibility.Visible;

                        curid = _AutoSoakingDevCatID;

                        RecipeEditorParamEdit = new RecipeEditorParamEditViewModel();

                        List<string> exceptlist = new List<string>();
                        exceptlist.Add("Soaking Time");
                        exceptlist.Add("Soaking Type");

                        RecipeEditorParamEdit.HardCategoryFiltering(curid, exceptlist);
                    }
                    else
                    {
                        ComboBoxVisibility = Visibility.Hidden;
                        EventNAutoSoakVisibility = Visibility.Visible;
                        curid = _CommonSoakingDevCatID;
                        RecipeEditorParamEdit = new RecipeEditorParamEditViewModel();

                        RecipeEditorParamEdit.HardCategoryFiltering(curid);
                    }
                }
                
            }
            catch (Exception err)
            {
                LoggerManager.Error($"ChangeRecipeEditorItems(): Error occurred. Err = {err.Message}");
            }
            finally
            {
                //await this.WaitCancelDialogService().CloseDialog();
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        private async Task SetTraceLastSoakingStateInfo()
        {
            await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Updating...");

            try
            {
                this.SoakingModule().TraceLastSoakingStateInfo(ShowStatusSoaking);                
            }
            catch (Exception err)
            {
                LoggerManager.SoakingErrLog($"{err.Message}");
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        private void SelectedItemAndCategoryFiltering(string enumType, string selectItem, int categoryID)
        {
            try
            {
                string path = "";
                string lastSplit = "";
                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    RecipeEditorParamEdit = new RecipeEditorParamEditViewModel();
                    //RecipeEditorParamEdit.HardCategoryFiltering(categoryID);
                    RecipeEditorParamEdit.HardCategoryFiltering(categoryID,"Soaking Type");

                    List<IElement> data = new List<IElement>();
                    IEnumerable<IElement> filter;

                    enumType = "SoakingDeviceFile";
                    for (int i = 0; i < EventSoakingTypelist.Count; i++)
                    {
                        filter = _DeviceParamElementList.Where(item => item.PropertyPath.Contains(enumType)).AsEnumerable();
                        data = filter.Where(item => item.PropertyPath.Contains(i.ToString())).ToList();

                        var type = data.Where(x => x.ElementName == "Soaking Type").FirstOrDefault();

                        if (type != null)
                        {
                            var tmp = EventSoakingTypelist.IndexOf(type.GetValue().ToString());
                            RecipeEditorParamEdit.AddDataNameInfo(EventSoakingTypelist[tmp], data);
                        }
                        else
                        {
                            LoggerManager.Error("Unknown.");
                        }
                    }

                    if (selectItem == "All")
                        return;

                    string filteringkeyword = string.Empty;
                    bool tryRet = _EventSoakingPropertyPathMap.TryGetValue(selectItem, out filteringkeyword);

                    if(tryRet == true)
                    {
                        filter = _DeviceParamElementList.Where(item => item.PropertyPath.Contains(filteringkeyword)).AsEnumerable();
                        data = filter.Where(item => item.PropertyPath.Contains(_Index.ToString())).ToList();


                        List<string> exceptlist = new List<string>();
                        exceptlist.Add("Soaking Type");
                        exceptlist.Add("EMPTY");

                        RecipeEditorParamEdit.HardElementFiltering(filter.ToList(), exceptlist);
                    }
                    else
                    {
                        LoggerManager.Debug($"Please check filteringkeyword. {selectItem.ToString()}");
                    }
                }));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
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
        public EventCodeEnum UpProc()
        {
            RecipeEditorParamEdit.PrevPageCommandFunc();
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum DownProc()
        {
            RecipeEditorParamEdit.NextPageCommandFunc();
            return EventCodeEnum.NONE;
        }

        private RelayCommand _DropDownClosedCommand;
        public ICommand DropDownClosedCommand
        {
            get
            {
                if (null == _DropDownClosedCommand) _DropDownClosedCommand = new RelayCommand(DropDownClosedCommandFunc);
                return _DropDownClosedCommand;
            }
        }

        private void DropDownClosedCommandFunc()
        {
            try
            {
                if (SelectedItem != null)
                {
                    _Index = EventSoakingTypelist.IndexOf(SelectedItem);

                    int curid;
                    if (TabControlSelectedIndex == 0)
                    {
                        curid = _EventSoakingDevCatID;
                    }
                    else if (TabControlSelectedIndex == 1)
                    {
                        curid = _AutoSoakingDevCatID;
                    }
                    else if (TabControlSelectedIndex == 2)
                    {
                        curid = _CommonSoakingDevCatID;
                    }
                    else
                    {
                        curid = _StatusSoakingDevCatID;
                    }

                    SelectedItemAndCategoryFiltering(EnumTypeName, SelectedItem, curid);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    
        private AsyncCommand _StatusSoakingSetupCommand;
        public ICommand StatusSoakingSetupCommand
        {
            get
            {
                if (null == _StatusSoakingSetupCommand) _StatusSoakingSetupCommand = new AsyncCommand(FuncStatusSoakingSetup);
                return _StatusSoakingSetupCommand;
            }
        }
        private async Task FuncStatusSoakingSetup()
        {
            try
            {
                bNextViewIsStatusSoakingSettingView = true;
                await this.ViewModelManager().ViewTransitionAsync(StatusSoakingSettingViewGuid);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private AsyncCommand _StatusSoakingToggleOnCommand;
        public ICommand StatusSoakingToggleOnCommand
        {
            get
            {
                if (null == _StatusSoakingToggleOnCommand) _StatusSoakingToggleOnCommand = new AsyncCommand(StatusSoakingToggleOnCommandFunc);
                return _StatusSoakingToggleOnCommand;
            }
        }
        private async Task StatusSoakingToggleOnCommandFunc()
        {
            try
            {
                this.SoakingModule().SetShowStatusSoakingSettingPageToggleValue(ShowStatusSoaking);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private AsyncCommand _StatusSoakingToggleOffCommand;
        public ICommand StatusSoakingToggleOffCommand
        {
            get
            {
                if (null == _StatusSoakingToggleOffCommand) _StatusSoakingToggleOffCommand = new AsyncCommand(StatusSoakingToggleOffCommandFunc);
                return _StatusSoakingToggleOffCommand;
            }
        }
        private async Task StatusSoakingToggleOffCommandFunc()
        {
            try
            {
                this.SoakingModule().SetShowStatusSoakingSettingPageToggleValue(ShowStatusSoaking);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public SoakingRecipeDataDescription GetSaokingRecipeContactInfo()
        {
            SoakingRecipeDataDescription des = new SoakingRecipeDataDescription();

            try
            {
                //des.ProbingModuleOverDriveStartPosition = this.ProbingModule().GetOverDriveStartPosition();
                //des.ProbingModuleFirstContactHeight = this.ProbingModule().FirstContactHeight;
                //des.ProbingModuleAllContactHeight = this.ProbingModule().AllContactHeight;
                //des.ManualContactModuleOverDrive = this.ManualContactModule().OverDrive;
                //des.ManualCotactModuleIsZUpState = this.ManualContactModule().IsZUpState;
                //des.WantToMoveZInterval = WantToMoveZInterval;
                //des.CanUsingManualContactControl = CanUsingManualContactControl;
                //des.IsVisiblePanel = IsVisiblePanel;
                //des.ManualContactModuleMachinePosition = this.ManualContactModule().MachinePosition;
                //des.ManualContactModuleXYIndex = this.ManualContactModule().MXYIndex;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return des;
        }

        public SoakingRecipeDataDescription GetSoakingRecipeDataDescription()
        {
            return null;
        }

        /// <summary>
        /// Selected Stage가 Idle 상태인지 확인하는 함수
        /// Selected된 Stage가 없으면 false로 return.
        /// </summary>
        /// <returns></returns>
        private bool IsSelectedStageIdleState()
        {
            if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Remote)//Loader
            {
                if (null == loaderCommunicationManager)
                {
                    loaderCommunicationManager = this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
                }

                if (loaderCommunicationManager == null)
                {
                    return false;
                }

                if (loaderCommunicationManager.SelectedStage == null)
                {
                    return false;
                }
                if (loaderCommunicationManager.SelectedStage.StageInfo.LotData.LotState != "IDLE")
                {
                    return false;
                }
            }
            else//SystemExcuteModeEnum.Prober
            {
                if(this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.IDLE)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
