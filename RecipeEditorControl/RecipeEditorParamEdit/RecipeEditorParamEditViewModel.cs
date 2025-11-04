using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecipeEditorControl.RecipeEditorParamEdit
{
    using AccountModule;
    using Autofac;
    using LoaderBase.Communication;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Enum;
    using ProberInterfaces.Network;
    using ProberInterfaces.ParamUtil;
    using ProberInterfaces.Proxies;
    using ProberInterfaces.Utility;
    using RecipeEditorControl.RecipeEditorUC;
    using RelayCommandBase;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using VirtualKeyboardControl;

    public class RecipeEditorParamEditViewModel : INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ==> ParamRecordVMList
        private ObservableCollection<ParamRecordViewModel> _ParamRecordVMList;
        public ObservableCollection<ParamRecordViewModel> ParamRecordVMList
        {
            get { return _ParamRecordVMList; }
            set
            {
                if (value != _ParamRecordVMList)
                {
                    _ParamRecordVMList = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        private const double _defaultRecordMinHeight = 65;

        private double _itemControlMinHeight;
        public double ItemControlMinHeight
        {
            get { return _itemControlMinHeight; }
            set
            {
                if (value != _itemControlMinHeight)
                {
                    _itemControlMinHeight = value;
                    RaisePropertyChanged();
                }
            }
        }

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

        #region ==> CurPageIndex
        private int _CurPageIndex;
        public int CurPageIndex
        {
            get { return _CurPageIndex; }
            set
            {
                if (value != _CurPageIndex)
                {
                    _CurPageIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> ParamSearchBoxText
        private String _ParamSearchBoxText;
        public String ParamSearchBoxText
        {
            get { return _ParamSearchBoxText; }
            set
            {
                if (value != _ParamSearchBoxText)
                {
                    _ParamSearchBoxText = value;
                    RaisePropertyChanged();

                    if (string.IsNullOrEmpty(value) == true)
                    {
                        IsSearchDataClearButtonVisible = false;
                    }
                    else
                    {
                        IsSearchDataClearButtonVisible = true;
                    }
                }
            }
        }
        #endregion

        #region ==> PrevPageCommand
        private RelayCommand _PrevPageCommand;
        public ICommand PrevPageCommand
        {
            get
            {
                if (null == _PrevPageCommand)
                    _PrevPageCommand = new RelayCommand(PrevPageCommandFunc);
                return _PrevPageCommand;
            }
        }
        public void PrevPageCommandFunc()
        {
            try
            {
                CurPageIndex--;
                if (CurPageIndex < 1)
                {
                    CurPageIndex = 1;
                    return;
                }

                UpdatePage();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> NextPageCommand
        private RelayCommand _NextPageCommand;
        public ICommand NextPageCommand
        {
            get
            {
                if (null == _NextPageCommand)
                    _NextPageCommand = new RelayCommand(NextPageCommandFunc);
                return _NextPageCommand;
            }
        }
        public void NextPageCommandFunc()
        {
            CurPageIndex++;
            try
            {
                if (CurPageIndex > TotalPageCount)
                {
                    CurPageIndex = TotalPageCount;
                    return;
                }

                UpdatePage();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> ParamSearchBoxClickCommand
        private RelayCommand _ParamSearchBoxClickCommand;
        public ICommand ParamSearchBoxClickCommand
        {
            get
            {
                if (null == _ParamSearchBoxClickCommand)
                    _ParamSearchBoxClickCommand = new RelayCommand(ParamSearchBoxClickCommandFunc);
                return _ParamSearchBoxClickCommand;
            }
        }
        private void ParamSearchBoxClickCommandFunc()
        {
            try
            {
                String filterKeyword = VirtualKeyboard.Show(ParamSearchBoxText, KB_TYPE.DECIMAL | KB_TYPE.ALPHABET);

                if (filterKeyword == null)  // || filterKeyword == string.Empty)
                {
                    filterKeyword = string.Empty;
                }

                KeywordFiltering(filterKeyword);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> RefreshCommand
        private AsyncCommand _RefreshCommand;
        public ICommand RefreshCommand
        {
            get
            {
                if (null == _RefreshCommand)
                {
                    _RefreshCommand = new AsyncCommand(RefreshCommandAsyncFunc);
                }

                return _RefreshCommand;
            }
        }

        private Task RefreshCommandAsyncFunc()
        {
            InitParameter();
            return Task.CompletedTask;
        }
        #endregion

        #region ==> CleanSheetSetupCommand
        private RelayCommand<CUI.Button> _CleanSheetSetupCommand;
        public ICommand CleanSheetSetupCommand
        {
            get
            {
                if (null == _CleanSheetSetupCommand)
                    _CleanSheetSetupCommand = new RelayCommand<CUI.Button>(CleanSheetSetupCommandFunc);
                return _CleanSheetSetupCommand;
            }
        }

        private void CleanSheetSetupCommandFunc(CUI.Button cuiparam)
        {
            try
            {
                Guid viewguid = new Guid();
                List<Guid> pnpsteps = new List<Guid>();
                this.PnPManager().GetCuiBtnParam(this.NeedleCleaner(), cuiparam.GUID, out viewguid, out pnpsteps);
                if (pnpsteps.Count != 0)
                {
                    this.PnPManager().SetNavListToGUIDs(this.NeedleCleaner(), pnpsteps);
                    this.ViewModelManager().ViewTransitionAsync(viewguid);
                }

                //Guid ViewGUID = CUIService.GetTargetViewGUID(cuiparam.GUID);
                //this.ViewModelManager().ViewTransitionUsingVM(ViewGUID, this.NeedleCleaner());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> CleanTextBoxClickCommand
        private RelayCommand _CleanTextBoxClickCommand;
        public ICommand CleanTextBoxClickCommand
        {
            get
            {
                if (null == _CleanTextBoxClickCommand)
                    _CleanTextBoxClickCommand = new RelayCommand(CleanTextBoxClickCommandFunc);
                return _CleanTextBoxClickCommand;
            }
        }
        private void CleanTextBoxClickCommandFunc()
        {
            try
            {
                if (ParamSearchBoxText != null)
                {
                    ParamSearchBoxText = null;
                    KeywordFiltering(ParamSearchBoxText);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private bool _IsSearchDataClearButtonVisible = false;
        public bool IsSearchDataClearButtonVisible
        {
            get { return _IsSearchDataClearButtonVisible; }
            set
            {
                if (value != _IsSearchDataClearButtonVisible)
                {
                    _IsSearchDataClearButtonVisible = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        private Visibility _NavPageButtonVisible;
        public Visibility NavPageButtonVisible
        {
            get { return _NavPageButtonVisible; }
            set
            {
                if (value != _NavPageButtonVisible)
                {
                    _NavPageButtonVisible = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<ParamRecordViewModel> _RecordData;
        private List<ParamRecordViewModel> _FilteredRecordData;

        private List<ParamRecordViewModel> _BeforeFilteredRecordData;

        private int _RecordCountPerPage = 10;
        private List<IElement> _DeviceParamElementList;
        private List<IElement> _SystemParamElementList;

        public RecipeEditorParamEditViewModel()
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var windowHeight = Application.Current.MainWindow.Height;

                if (windowHeight > 1024)
                {
                    _RecordCountPerPage = (int)Math.Round(windowHeight / 128) + 1;
                    NavPageButtonVisible = Visibility.Visible;
                    _RecordCountPerPage = 10;
                }
                else
                {
                    NavPageButtonVisible = Visibility.Hidden;
                }

                ItemControlMinHeight = _defaultRecordMinHeight * _RecordCountPerPage;
            });

            InitParameter();
        }

        public void InitParameter()
        {
            try
            {
                //==> Set Record View Model
                ParamRecordVMList = new ObservableCollection<ParamRecordViewModel>();

                for (int i = 0; i < _RecordCountPerPage; i++)
                {
                    _ParamRecordVMList.Add(new ParamRecordViewModel());
                }

                _RecordData = new List<ParamRecordViewModel>();
                _FilteredRecordData = new List<ParamRecordViewModel>();
                
                if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober)
                {
                    if (this.ParamManager() != null)
                    {
                        _DeviceParamElementList = this.ParamManager().GetDevElementList();
                        _SystemParamElementList = this.ParamManager().GetSysElementList();
                    }
                }
                else
                {
                    _DeviceParamElementList = this.GetLoaderContainer().Resolve<ILoaderParamManager>().GetDevElementList();
                    _SystemParamElementList = this.GetLoaderContainer().Resolve<ILoaderParamManager>().GetSysElementList();
                }

                SetupParameterRecord(_DeviceParamElementList, Brushes.White);
                SetupParameterRecord(_SystemParamElementList, Brushes.White);

                KeywordFiltering(String.Empty);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async Task<EventCodeEnum> SetValueRejectAction(IElement element, Object newVal)
        {
            EventCodeEnum isValid = EventCodeEnum.UNDEFINED;
            string details = null;
            Task<EventCodeEnum> task = new Task<EventCodeEnum>(() =>
            {
                EventCodeEnum checkResult = EventCodeEnum.UNDEFINED;
                try
                {
                    if (AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")
                    {
                        var loadercomm = this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
                        checkResult = loadercomm.GetProxy<IParamManagerProxy>().CheckSetValueAvailable(element.PropertyPath, newVal);//, this.GetType().FullName);

                    }
                    else
                    {
                        checkResult = element.CheckSetValueAvailable(element.PropertyPath, newVal, out details);
                    }

                    return checkResult;
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                return checkResult;

            });
            task.Start();// UI 단이므로 다른 스레드에서 처리
            isValid = await task;

            if (isValid != EventCodeEnum.NONE)
            {
                await this.ParamManager().MetroDialogManager().ShowMessageDialog("Param Validation Failed", $"Cannot be set due to {task.Result} error code. \n details:{details}", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
            }
            return isValid;
        }


        private Task VkNumericCmd(Object param)
        {
            RecipeEditorCmdButtonViewModel paramRecord = param as RecipeEditorCmdButtonViewModel;
            if (paramRecord == null)
                return Task.CompletedTask;

            if (paramRecord.Elem.WriteMaskingLevel < AccountManager.CurrentUserInfo.UserLevel)
                return Task.CompletedTask;

            String strValue = "";
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                strValue = VirtualKeyboard.Show(paramRecord.Caption, KB_TYPE.DECIMAL);
            }));

            if (paramRecord.Elem.Unit != null && paramRecord.Elem.Unit != "EMPTY" && paramRecord.Elem.Unit != "")
            {
                strValue = strValue.Replace($"{paramRecord.Elem.Unit}", "");
                strValue = strValue.Trim();
            }

            int intValue = 0;
            if (int.TryParse(strValue, out intValue) == false)
                return Task.CompletedTask;

            //EventCodeEnum isValid = SetValueRejectAction(paramRecord.Elem, intValue).Result; //TODO: 언젠가.. 모든 Element의 Validation이 가능하면 살릴 수 있는 코드.
            //if (isValid == EventCodeEnum.NONE)
            {
                paramRecord.SetElemValueBuffer(intValue);
                if (paramRecord.FlushValueBuffer())
                    SaveParameter(paramRecord.Elem);
            }
            return Task.CompletedTask;
        }

        private Task VkStrCmd(Object param)
        {
            RecipeEditorCmdButtonViewModel paramRecord = param as RecipeEditorCmdButtonViewModel;
            if (paramRecord == null)
                return Task.CompletedTask;

            if (paramRecord.Elem.WriteMaskingLevel < AccountManager.CurrentUserInfo.UserLevel)
                return Task.CompletedTask;

            String strValue = "";
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                strValue = VirtualKeyboard.Show(paramRecord.Caption, minCharLen: 0, maxCharLen: 100);
            }));

            //EventCodeEnum isValid = SetValueRejectAction(paramRecord.Elem, strValue).Result; //TODO: 언젠가.. 모든 Element의 Validation이 가능하면 살릴 수 있는 코드.
            //if (isValid == EventCodeEnum.NONE)
            {
                paramRecord.SetElemValueBuffer(strValue);
                if (paramRecord.FlushValueBuffer())
                    SaveParameter(paramRecord.Elem);
            }
            return Task.CompletedTask;
        }

        private Task VkIpAddressCmd(Object param)
        {
            RecipeEditorCmdButtonViewModel paramRecord = param as RecipeEditorCmdButtonViewModel;
            if (paramRecord == null)
                return Task.CompletedTask;

            if (paramRecord.Elem.WriteMaskingLevel < AccountManager.CurrentUserInfo.UserLevel)
                return Task.CompletedTask;

            IPAddressVer4 ipAddress = (IPAddressVer4)paramRecord.Elem.GetValue();


            String strValue = "";
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                strValue = VirtualKeyboard.Show(ipAddress.IP);
            }));

            IPAddressVer4 newIpAddress = IPAddressVer4.GetData(strValue);

            paramRecord.SetElemValueBuffer(newIpAddress);
            if (paramRecord.FlushValueBuffer())
                SaveParameter(paramRecord.Elem);

            return Task.CompletedTask;
        }

        private Task VkFloatCmd(Object param)
        {
            RecipeEditorCmdButtonViewModel paramRecord = param as RecipeEditorCmdButtonViewModel;
            if (paramRecord == null)
                return Task.CompletedTask;

            if (paramRecord.Elem.WriteMaskingLevel < AccountManager.CurrentUserInfo.UserLevel)
                return Task.CompletedTask;

            String strValue = "";
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                strValue = VirtualKeyboard.Show(paramRecord.Caption, KB_TYPE.DECIMAL | KB_TYPE.FLOAT);
            }));

            if (paramRecord.Elem.Unit != null && paramRecord.Elem.Unit != "EMPTY" && paramRecord.Elem.Unit != "")
            {
                strValue = strValue.Replace($"{paramRecord.Elem.Unit}", "");
                strValue = strValue.Trim();
            }

            object value = null;
            if (paramRecord.Elem.ValueType == typeof(float))
            {
                float floatvalue = 0;
                if (float.TryParse(strValue, out floatvalue) == false)
                    return Task.CompletedTask;
                value = floatvalue;
            }
            if (paramRecord.Elem.ValueType == typeof(double))
            {
                double doublevalue = 0;
                if (double.TryParse(strValue, out doublevalue) == false)
                    return Task.CompletedTask;
                value = doublevalue;
            }

            //EventCodeEnum isValid = SetValueRejectAction(paramRecord.Elem, value).Result; //TODO: 언젠가.. 모든 Element의 Validation이 가능하면 살릴 수 있는 코드.
            //if (isValid == EventCodeEnum.NONE)
            {
                paramRecord.SetElemValueBuffer(value);
                if (paramRecord.FlushValueBuffer())
                    SaveParameter(paramRecord.Elem);
            }
            return Task.CompletedTask;
        }

        public void SetupParameterRecord(List<IElement> paramElemList, Brush brush)
        {
            try
            {
                #region ==> Command Type : Button을 눌렀을 떄 호출되는 Command
                ICommand vkStrCmd = new AsyncCommand<Object>(VkStrCmd);
                ICommand vkNumericCmd = new AsyncCommand<Object>(VkNumericCmd);
                ICommand vkFloatCmd = new AsyncCommand<Object>(VkFloatCmd);
                ICommand vkIpAdressCmd = new AsyncCommand<Object>(VkIpAddressCmd);
                #endregion

                //==> TODO : SetupParameterRecord는 System에서 한번만 수행하는 것이 아니기에 이 위치에서 정렬 하는 것은 옳지 않다.
                //==> Param Manager에서 Element들을 미리 정렬해야 한다.
                //==> Element ID 순으로 정렬
                if (paramElemList != null)
                {
                    paramElemList.Sort(delegate (IElement elem1, IElement elem2)
                    {
                        if (elem1.ElementID > elem2.ElementID)
                            return 1;
                        else if (elem1.ElementID < elem2.ElementID)
                            return -1;
                        return 0;
                    });
                }

                if (paramElemList != null)
                {
                    foreach (IElement paramElem in paramElemList)
                    {
                        //==> TODO : 모든 Element는 Element ID 가 할당 되어 있어야 한다.

                        if (paramElem.GetValue() == null)
                            continue;

                        if (paramElem.ValueType == null)
                        {
                            if (paramElem.ValueTypeDesc == null)
                            {
                                continue;
                            }
                            else
                            {
                                try
                                {
                                    paramElem.ValueType = Type.GetType(paramElem.ValueTypeDesc);
                                }
                                catch (Exception err)
                                {
                                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                                    LoggerManager.Debug($"Error occurred while retreive type. Type Desc. = {paramElem.ValueTypeDesc}");
                                    continue;
                                }
                            }
                        }

                        if (paramElem.ReadMaskingLevel < AccountManager.CurrentUserInfo.UserLevel)
                            continue;

                        Type elemType = paramElem.ValueType;

                        if (elemType != null)
                        {
                            if (elemType == typeof(String))
                            {
                                //==> 사용자 정의 Type
                                AddEditorParameterRecord(paramElem, vkStrCmd, brush);
                            }
                            else if (elemType.IsEnum)
                            {
                                String[] enumNameArray = Enum.GetNames(elemType);

                                // TODO : Enum 중 사용자에게 보여주고 싶지 않은 것에 대한 처리?
                                String[] FilteredEnumNameArray;
                                List<String> templist = new List<string>();

                                foreach (var enumname in enumNameArray)
                                {
                                    Enum enumobj = Enum.Parse(elemType, enumname) as Enum;

                                    var attribute = EnumExtensions.GetAttribute<EnumIgnore>(enumobj);

                                    // EnumIgnore을 설정해놓지 않은 항목만 추가
                                    if (attribute == null)
                                    {
                                        templist.Add(enumname);
                                    }
                                }

                                FilteredEnumNameArray = templist.ToArray();

                                AddPopupListParameterRecord(paramElem, FilteredEnumNameArray, brush);
                                //AddPopupListParameterRecord(paramElem, enumNameArray, brush);
                            }
                            else if (paramElem.IsNumericType())
                            {
                                AddEditorParameterRecord(paramElem, vkNumericCmd, brush);
                            }
                            else if (paramElem.IsFloatingType())
                            {
                                AddEditorParameterRecord(paramElem, vkFloatCmd, brush);
                            }
                            else if (paramElem.IsBooleanType())
                            {
                                String[] boolTypeArray = new String[] { "True", "False" };
                                AddPopupListParameterRecord(paramElem, boolTypeArray, brush);
                            }
                            else if (paramElem.IsIPAddressType())
                            {
                                AddEditorParameterRecord(paramElem, vkIpAdressCmd, brush);
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
                LoggerManager.Exception(err, $"SetupParameterRecord(): Error occurred");
            }
        }

        private void AddRecordData(ParamRecordViewModel recordViewModel)
        {
            try
            {
                _RecordData.Add(recordViewModel);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        private void AddEditorParameterRecord(IElement elem, ICommand showEditorCommand, Brush brush)
        {
            try
            {
                ParamRecordViewModel newParamRecrod = new ParamRecordViewModel();
                newParamRecrod.ElementID = elem.ElementID;
                newParamRecrod.CategoryID = elem.CategoryID;
                newParamRecrod.CategoryIDList = elem.CategoryIDList ?? new List<int>();
                newParamRecrod.Name = elem.ElementName;
                newParamRecrod.Description = elem.Description;
                newParamRecrod.MinMax = $"{elem.LowerLimit}/{elem.UpperLimit}";
                newParamRecrod.RecordVisibility = Visibility.Visible;
                newParamRecrod.RecordColor = brush;
                newParamRecrod.Elem = elem;
                newParamRecrod.EditorButton = new RecipeEditorCmdButtonViewModel(elem, showEditorCommand);

                AddRecordData(newParamRecrod);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void AddPopupListParameterRecord(IElement elem, String[] popupItemMenuLabel, Brush brush)
        {
            try
            {
                //==> Parsing Check
                ParamRecordViewModel newParamRecrod = new ParamRecordViewModel();

                newParamRecrod.ElementID = elem.ElementID;
                newParamRecrod.Name = elem.ElementName;
                newParamRecrod.CategoryID = elem.CategoryID;
                newParamRecrod.CategoryIDList = elem.CategoryIDList ?? new List<int>();
                newParamRecrod.Description = elem.Description;
                newParamRecrod.MinMax = "-";
                newParamRecrod.RecordVisibility = Visibility.Visible;
                newParamRecrod.RecordColor = brush;

                if (elem.ValueType == typeof(double))
                {
                    string tempstr = (Math.Truncate((double)elem.GetValue() * 100) / 100).ToString();
                    elem.SetValue(Convert.ToDouble(tempstr));
                    newParamRecrod.Elem = elem;
                }
                else
                {
                    newParamRecrod.Elem = elem;
                }

                newParamRecrod.EditorButton = new RecipeEditorCtxMenuButtonViewModel(elem, popupItemMenuLabel, SaveParameter);

                AddRecordData(newParamRecrod);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void UpdatePage()
        {
            try
            {
                TotalPageCount = GetTotalPageCount();

                Application.Current.Dispatcher.Invoke((Action)(() =>
                {

                    int startRecordIndex = (CurPageIndex - 1) * _RecordCountPerPage;
                    int endRecordIndex = startRecordIndex + _RecordCountPerPage;

                    if (endRecordIndex > _FilteredRecordData.Count)
                    {
                        endRecordIndex = _FilteredRecordData.Count;
                    }

                    var _FilteredParamRecordVMList = new ObservableCollection<ParamRecordViewModel>();  // Binding 업데이트

                    for (int i = 0; i < _RecordCountPerPage; i++)
                    {
                        _FilteredParamRecordVMList.Add(new ParamRecordViewModel());
                    }

                    for (int i = 0; i < _RecordCountPerPage; i++)
                    {
                        if (startRecordIndex < endRecordIndex)
                        {
                            _FilteredParamRecordVMList[i] = _FilteredRecordData[startRecordIndex];
                            _FilteredParamRecordVMList[i].RecordVisibility = Visibility.Visible;
                        }
                        else
                        {
                            _FilteredParamRecordVMList[i] = new ParamRecordViewModel();
                            _FilteredParamRecordVMList[i].RecordVisibility = Visibility.Collapsed;
                        }
                        startRecordIndex++;
                    }

                    ParamRecordVMList = _FilteredParamRecordVMList;
                    ItemControlMinHeight = _defaultRecordMinHeight * 2;
                }));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private int GetTotalPageCount()
        {
            int pageCount = -1;

            if (_FilteredRecordData != null && _FilteredRecordData.Count > 0)
            {
                pageCount = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(_FilteredRecordData.Count) / _RecordCountPerPage));
            }

            if (pageCount < 1)
            {
                pageCount = 1;
            }

            return pageCount;
        }

        //public void KeywordFiltering(String filterKeyword)
        //{
        //    try
        //    {
        //        ParamSearchBoxText = filterKeyword;

        //        if (String.IsNullOrEmpty(ParamSearchBoxText))
        //            _FilteredRecordData = _RecordData;
        //        else
        //        {
        //            List<ParamRecordViewModel> temp = new List<ParamRecordViewModel>();
        //            foreach (ParamRecordViewModel paramRecord in _RecordData)
        //            {
        //                //==> Element ID Search
        //                //==> Category ID Search
        //                int id = 0;
        //                if (int.TryParse(ParamSearchBoxText, out id))
        //                {
        //                    if (paramRecord.ElementID == id)
        //                    {
        //                        temp.Add(paramRecord);
        //                        continue;
        //                    }
        //                    if (paramRecord.CategoryIDList != null && paramRecord.CategoryIDList.Contains(id))
        //                    {
        //                        temp.Add(paramRecord);
        //                        continue;
        //                    }
        //                }

        //                //==> Parameter Name Search ...
        //                //==> Parameter Description Search 
        //                if (paramRecord.Name == null)
        //                    continue;

        //                if (paramRecord.Description == null)
        //                    continue;

        //                if (paramRecord.Name.ToLower().Contains(ParamSearchBoxText.ToLower()) ||
        //                    paramRecord.Description.ToLower().Contains(ParamSearchBoxText.ToLower()))
        //                {
        //                    temp.Add(paramRecord);
        //                    continue;
        //                }
        //            }
        //            _FilteredRecordData = temp;
        //        }

        //        CurPageIndex = 1;//==> 현재 page 갱신
        //        UpdatePage();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        public void KeywordFiltering(String filterKeyword)
        {
            try
            {
                ParamSearchBoxText = filterKeyword;

                if (String.IsNullOrEmpty(ParamSearchBoxText))
                {
                    //_FilteredRecordData = _RecordData;

                    if (_BeforeFilteredRecordData != null)
                    {
                        _FilteredRecordData = _BeforeFilteredRecordData;
                    }
                }
                else
                {
                    List<ParamRecordViewModel> temp = new List<ParamRecordViewModel>();
                    List<ParamRecordViewModel> search_Item_source = null;

                    if (_BeforeFilteredRecordData != null)
                    {
                        search_Item_source = _BeforeFilteredRecordData;
                    }
                    else
                    {
                        search_Item_source = _RecordData;
                    }

                    foreach (ParamRecordViewModel paramRecord in search_Item_source)
                    {
                        //==> Element ID Search
                        //==> Category ID Search
                        int id = 0;

                        if (int.TryParse(ParamSearchBoxText, out id))
                        {
                            if (paramRecord.ElementID == id)
                            {
                                temp.Add(paramRecord);
                                continue;
                            }

                            if (paramRecord.CategoryIDList != null && paramRecord.CategoryIDList.Contains(id))
                            {
                                temp.Add(paramRecord);
                                continue;
                            }
                        }

                        //==> Parameter Name Search ...
                        //==> Parameter Description Search 
                        if (paramRecord.Name == null)
                        {
                            continue;
                        }

                        if (paramRecord.Description == null)
                        {
                            continue;
                        }

                        if (paramRecord.Name.ToLower().Contains(ParamSearchBoxText.ToLower()) ||
                            paramRecord.Description.ToLower().Contains(ParamSearchBoxText.ToLower()))
                        {
                            temp.Add(paramRecord);
                            continue;
                        }
                    }

                    _FilteredRecordData = temp;
                }

                CurPageIndex = 1;//==> 현재 page 갱신
                UpdatePage();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ElementIDFiltering(List<int> elementIdList)
        {
            try
            {
                _FilteredRecordData = _RecordData.Where(item => elementIdList.Contains(item.ElementID)).ToList();
                _BeforeFilteredRecordData = new List<ParamRecordViewModel>(_FilteredRecordData);

                CurPageIndex = 1;//==> 현재 page 갱신
                UpdatePage();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void HardElementIDFiltering(List<int> elementIdList)
        {
            try
            {
                _RecordData = _RecordData.Where(item => elementIdList.Contains(item.ElementID)).ToList();
                _FilteredRecordData = _RecordData;
                _BeforeFilteredRecordData = new List<ParamRecordViewModel>(_FilteredRecordData);

                CurPageIndex = 1;//==> 현재 page 갱신
                UpdatePage();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void HardElementFiltering(List<IElement> elementList, List<string> ExceptElementName = null)
        {
            try
            {
                if (_RecordData != null)
                {
                    _RecordData = _RecordData.Where(item => elementList.Contains(item.Elem)).ToList();

                    if (ExceptElementName != null)
                    {
                        if (_RecordData != null && _RecordData.Count > 0)
                        {
                            foreach (var item in ExceptElementName)
                            {
                                var foundRecord = _RecordData.FirstOrDefault(x => x.Name.Contains(item) == true);

                                if (foundRecord != null)
                                {
                                    _RecordData.Remove(foundRecord);
                                }
                            }
                        }
                    }

                    _FilteredRecordData = _RecordData;
                    _BeforeFilteredRecordData = new List<ParamRecordViewModel>(_FilteredRecordData);

                    CurPageIndex = 1;//==> 현재 page 갱신
                    UpdatePage();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void CategoryFiltering(String categoryID)
        {
            try
            {
                int intCategoryID;

                if (int.TryParse(categoryID, out intCategoryID) == false)
                    return;

                CategoryFiltering(intCategoryID);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void CategoryFiltering(int categoryID)
        {
            try
            {
                _FilteredRecordData = _RecordData.Where(item => (item != null) && (item.CategoryIDList.Contains(categoryID))).ToList();
                _BeforeFilteredRecordData = new List<ParamRecordViewModel>(_FilteredRecordData);

                CurPageIndex = 1;//==> 현재 page 갱신
                UpdatePage();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void HardCategoryFiltering(int categoryID, List<string> ExceptElementName = null)
        {
            try
            {
                if (_RecordData != null)
                {
                    var catedRecords = _RecordData.Where(item => item.CategoryIDList.Contains(categoryID)).ToList();

                    if (ExceptElementName != null)
                    {
                        if (catedRecords != null && catedRecords.Count > 0)
                        {
                            foreach (var item in ExceptElementName)
                            {
                                var foundRecord = catedRecords.FirstOrDefault(x => x.Name == item);

                                if (foundRecord != null)
                                {
                                    catedRecords.Remove(foundRecord);
                                }
                            }
                        }
                    }

                    _FilteredRecordData = catedRecords;
                    _BeforeFilteredRecordData = new List<ParamRecordViewModel>(_FilteredRecordData);
                    _BeforeFilteredRecordData = new List<ParamRecordViewModel>(_FilteredRecordData);

                    CurPageIndex = 1;//==> 현재 page 갱신
                    UpdatePage();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void HardCategoryFiltering(int categoryID, string filter)
        {
            try
            {
                if (_RecordData != null)
                {
                    var catedRecords = _RecordData.Where(item => item.CategoryIDList.Contains(categoryID)).ToList();
                    for (int i = 0; i < catedRecords?.Count; i++)
                    {
                        if (catedRecords[i].Name == filter)
                        {
                            catedRecords.RemoveAt(i);
                        }
                    }

                    _FilteredRecordData = catedRecords;
                    _BeforeFilteredRecordData = new List<ParamRecordViewModel>(_FilteredRecordData);

                    CurPageIndex = 1;//==> 현재 page 갱신
                    UpdatePage();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void HardCategoryFiltering(List<int> categoryIDList)
        {
            try
            {
                var temp = new List<ParamRecordViewModel>();
                foreach (int categoryID in categoryIDList)
                {
                    var cognexElemList = _RecordData.Where(item => item.CategoryIDList.Contains(categoryID));
                    temp.AddRange(cognexElemList);
                }

                _RecordData = temp;

                _FilteredRecordData = _RecordData;

                CurPageIndex = 1;//==> 현재 page 갱신
                UpdatePage();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void AddDataNameInfo(String info, List<IElement> elementList)
        {
            try
            {
                if (_RecordData != null)
                {
                    List<ParamRecordViewModel> recordDataList = _RecordData.Where(item => elementList.Contains(item.Elem)).ToList();
                    foreach (ParamRecordViewModel record in recordDataList)
                    {
                        String space = String.Empty;
                        if (record.Name != null)
                        {
                            for (int i = 0; i < (record.Name.Length - info.Length) / 2; i++)
                                space += ' ';

                            record.Name = $"{record.Name}\n{space}({info})";
                        }
                        else
                        {
                            record.Name = "Name Error.";
                        }
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SaveParameter(IElement elem)
        {

            if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober)
            {
                this.ParamManager().SaveElement(elem, isNeedValidation: true);//, this.GetType().FullName);// ClassName 전달할 것.
            }
            else
            {
                this.GetLoaderContainer().Resolve<ILoaderParamManager>().SaveElement(elem);// isNeedValidation: true);//, this.GetType().FullName);
            }            

        }

    }
}
