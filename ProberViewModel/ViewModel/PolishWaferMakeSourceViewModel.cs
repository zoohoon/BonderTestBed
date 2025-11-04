using Autofac;
using LogModule;
using MetroDialogInterfaces;
using PolishWaferParameters;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.ControlClass.ViewModel.Polish_Wafer;
using ProberInterfaces.Loader;
using RelayCommandBase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using VirtualKeyboardControl;

namespace PolishWaferMakeSourceVM
{

    public class PWColorPreset
    {
        public List<string> ColorNamelist;
        //public List<Brush> Brushlist;

        public KnownColor[] allColors;

        public PWColorPreset()
        {
            System.Array colorsArray = Enum.GetValues(typeof(KnownColor));
            allColors = new KnownColor[colorsArray.Length];

            Array.Copy(colorsArray, allColors, colorsArray.Length);

            //Brushlist = new List<Brush>();

            //Brushlist.Add(new SolidColorBrush(Color.FromRgb(255, 0, 0)));
            //Brushlist.Add(new SolidColorBrush(Color.FromRgb(0, 255, 0)));
            //Brushlist.Add(new SolidColorBrush(Color.FromRgb(0, 0, 255)));
            //Brushlist.Add(new SolidColorBrush(Color.FromRgb(255, 255, 0)));

            ColorNamelist = new List<string>();

            AddColor("Red");
            AddColor("Green");
            AddColor("Blue");
            AddColor("Chocolate");
            AddColor("Silver");
            AddColor("DarkOrchid");
            AddColor("DodgerBlue");
            AddColor("HotPink");
            AddColor("LemonChiffon");
        }

        private void AddColor(string inputname)
        {
            var check = allColors.FirstOrDefault(x => x.ToString() == inputname);

            string ColorName = string.Empty;

            if (check != null)
            {
                if (ColorName != inputname)
                {
                    ColorNamelist.Add(inputname);
                }
            }
        }
    }

    public class PolishWaferMakeSourceViewModel : IMainScreenViewModel, IPolishWaferMakeSourceVM
    {
        private readonly Guid _ViewModelGUID = new Guid("fbc1cb9c-9aaa-404b-b578-bccbdbe67c47");
        public Guid ScreenGUID
        {
            get { return _ViewModelGUID; }
        }

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        private int MaximumSourceCount = 0;

        //private PolishWaferParameter _PolishWaferparameters;
        //public PolishWaferParameter PolishWaferparameters
        //{
        //    get { return _PolishWaferparameters; }
        //    set
        //    {
        //        if (value != _PolishWaferparameters)
        //        {
        //            _PolishWaferparameters = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private PWColorPreset _ColorPreset = new PWColorPreset();
        public PWColorPreset ColorPreset
        {
            get { return _ColorPreset; }
            set
            {
                if (value != _ColorPreset)
                {
                    _ColorPreset = value;
                    RaisePropertyChanged();
                }
            }
        }


        private IPolishWaferSourceInformation _SelectedSourceInfo;
        public IPolishWaferSourceInformation SelectedSourceInfo
        {
            get { return _SelectedSourceInfo; }
            set
            {
                if (value != _SelectedSourceInfo)
                {
                    _SelectedSourceInfo = value;
                    RaisePropertyChanged();

                    //if (value != null)
                    //{
                    //    Task.Run(() => GetParamFromDevice(value));
                    //}
                }
            }
        }

        private ITrasnferObjectSet _TrasnferObjectSet;
        public ITrasnferObjectSet TrasnferObjectSet
        {
            get { return _TrasnferObjectSet; }
            set
            {
                if (value != _TrasnferObjectSet)
                {
                    _TrasnferObjectSet = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private PolishWaferParameter _PolishWaferparameters;
        //public PolishWaferParameter PolishWaferparameters
        //{
        //    get { return _PolishWaferparameters; }
        //    set
        //    {
        //        if (value != _PolishWaferparameters)
        //        {
        //            _PolishWaferparameters = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private ObservableCollection<PolishWaferIntervalParameter> _PolishWaferIntervalParameters;
        public ObservableCollection<PolishWaferIntervalParameter> PolishWaferIntervalParameters
        {
            get { return _PolishWaferIntervalParameters; }
            set
            {
                if (value != _PolishWaferIntervalParameters)
                {
                    _PolishWaferIntervalParameters = value;
                    RaisePropertyChanged();
                }
            }
        }


        private AsyncObservableCollection<IPolishWaferSourceInformation> _PolishWaferSourceInformations;
        public AsyncObservableCollection<IPolishWaferSourceInformation> PolishWaferSourceInformations
        {
            get { return _PolishWaferSourceInformations; }
            set
            {
                if (value != _PolishWaferSourceInformations)
                {
                    _PolishWaferSourceInformations = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IDeviceManager devicemanager;


        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    InitParam();

                    MaximumSourceCount = ColorPreset.ColorNamelist.Count;

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

        public Task<EventCodeEnum> InitViewModel()
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        private void InitParam()
        {
            try
            {
                if (this.DeviceManager() != null)
                {
                    PolishWaferSourceInformations = this.DeviceManager().GetPolishWaferSources();
                }

                if (this.PolishWaferModule() != null)
                {
                    var pwparam = this.PolishWaferModule().GetPolishWaferIParam();

                    if (pwparam != null)
                    {
                        PolishWaferIntervalParameters = (pwparam as PolishWaferParameter).PolishWaferIntervalParameters;

                        SelectedSourceInfo = null;

                        if (SystemManager.SysteMode == SystemModeEnum.Single)
                        {
                            Autofac.IContainer loadercontainer = this.LoaderController().GetLoaderContainer();

                            devicemanager = loadercontainer.Resolve<IDeviceManager>();

                            TrasnferObjectSet = devicemanager.TransferObjectInfos;
                        }
                        else
                        {
                            if (System.AppDomain.CurrentDomain.FriendlyName == "ProberSystem.exe")
                            {
                                if ((this.LoaderController().GetconnectFlag() == true) &&
                                    (this.StageCommunicationManager().IsEnableDialogProxy() == true))
                                {

                                }
                                else
                                {
                                    TrasnferObjectSet = null;
                                }
                            }
                            else
                            {
                                TrasnferObjectSet = this.DeviceManager().TransferObjectInfos;
                            }
                        }

                        if(PolishWaferSourceInformations != null)
                        {
                            for (int i = 0; i < PolishWaferSourceInformations.Count; i++)
                            {
                                if (ColorPreset.ColorNamelist.Count > i)
                                {
                                    PolishWaferSourceInformations[i].IdentificationColorBrush.Value = ColorPreset.ColorNamelist[i];
                                }
                                else
                                {
                                    LoggerManager.Debug($"TODO");
                                }
                            }
                        }
                        //}
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            try
            {
                InitParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            try
            {
                this.PolishWaferModule().SaveDevParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
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

        private AsyncCommand _AddSourceCommand;
        public IAsyncCommand AddSourceCommand
        {
            get
            {
                if (null == _AddSourceCommand)
                    _AddSourceCommand = new AsyncCommand(AddSourceCommandFunc);
                return _AddSourceCommand;
            }
        }

        private async Task AddSourceCommandFunc()
        {
            try
            {

                if (PolishWaferSourceInformations.Count >= MaximumSourceCount)
                {
                    await this.MetroDialogManager().ShowMessageDialog("[Information]", "Can not add more data.", EnumMessageStyle.Affirmative);
                }
                else

                {

                    EnumMessageDialogResult result = EnumMessageDialogResult.NEGATIVE;

                    result = await this.MetroDialogManager().ShowSingleInputDialog("Name : ", "Add");

                    if (result == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        string devName = string.Empty;

                        Task task = new Task(() =>
                        {
                            devName = this.MetroDialogManager().GetSingleInputData();
                        });
                        task.Start();
                        await task;

                        //await Task.Run(() =>
                        //{
                        //    devName = this.MetroDialogManager().GetSingleInputData();
                        //});

                        if (!string.IsNullOrEmpty(devName))
                        {
                            if (PolishWaferSourceInformations == null)
                            {
                                PolishWaferSourceInformations = new AsyncObservableCollection<IPolishWaferSourceInformation>();
                            }

                            IPolishWaferSourceInformation alreadyHasDevDir = null;
                            bool isProcessChangeDevice = false;

                            if (PolishWaferSourceInformations.Count > 0)
                            {
                                alreadyHasDevDir = PolishWaferSourceInformations.Where(x => x.DefineName.Value == devName).FirstOrDefault();
                            }

                            if (alreadyHasDevDir != null)
                            {
                                isProcessChangeDevice = false;

                                // TODO: 중복된 이름으로 Add 할 수 없다는 내용을 사용자에게 전달해야 함.
                            }
                            else
                            {
                                isProcessChangeDevice = true;
                            }

                            if (isProcessChangeDevice)
                            {
                                System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                                {
                                    PolishWaferInformation tmpinfo = new PolishWaferInformation();
                                    tmpinfo.DefineName.Value = devName;
                                    tmpinfo.IdentificationColorBrush.Value = ColorPreset.ColorNamelist[PolishWaferSourceInformations.Count];
                                    tmpinfo.Thickness.Value = 1000;
                                    tmpinfo.Size.Value = SubstrateSizeEnum.INCH12;
                                    tmpinfo.NotchType.Value = WaferNotchTypeEnum.NOTCH;

                                    PolishWaferSourceInformations.Add(tmpinfo);

                                    if (PolishWaferSourceInformations.Count == 1)
                                    {
                                        SelectedSourceInfo = PolishWaferSourceInformations[0];
                                    }
                                }));

                                this.PolishWaferModule().SaveDevParameter();
                                //await SaveAllDeviceUsingName(devName);
                                //await ChangeDeviceFuncUsingName(devName);
                                //GetDevList();
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

        private AsyncCommand _RemoveSourceCommand;
        public IAsyncCommand RemoveSourceCommand
        {
            get
            {
                if (null == _RemoveSourceCommand)
                    _RemoveSourceCommand = new AsyncCommand(RemoveSourceCommandFunc);
                return _RemoveSourceCommand;
            }
        }

        private async Task RemoveSourceCommandFunc()
        {
            try
            {
                if (SelectedSourceInfo != null)
                {
                    EnumMessageDialogResult result = EnumMessageDialogResult.NEGATIVE;


                    result = await this.MetroDialogManager().ShowMessageDialog("[Information]", "Are you sure you want to remove the data?", EnumMessageStyle.AffirmativeAndNegative);

                    if (result == EnumMessageDialogResult.AFFIRMATIVE)  // OK
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            // SelectedSourceInf가 다른 객체인 경우가 발생 됨.
                            //PolishWaferInformation tmp = PolishWaferSourceInformations.FirstOrDefault(x => x.DefineName.Value == SelectedSourceInfo.DefineName.Value);

                            //if(tmp != null)
                            //{
                            // 삭제하려고 하는 디바이스 이름과 동일한 파라미터를 갖고 있는 경우 같이 삭제해줘야 한다.
                            // 인터벌 파라미터 안에 있는 클리닝 파라미터 전체 검사 진행.

                            if (PolishWaferIntervalParameters.Count > 0)
                            {
                                foreach (var intervalparam in PolishWaferIntervalParameters.ToList())
                                {
                                    foreach (var cleaningparam in intervalparam.CleaningParameters.ToList())
                                    {
                                        if (cleaningparam.WaferDefineType.Value == SelectedSourceInfo.DefineName.Value)
                                        {
                                            // 삭제
                                            intervalparam.CleaningParameters.Remove(cleaningparam);
                                        }
                                    }
                                }
                            }

                            PolishWaferSourceInformations.Remove(SelectedSourceInfo);
                            SelectedSourceInfo = null;

                            for (int i = 0; i < PolishWaferSourceInformations.Count; i++)
                            {
                                if (ColorPreset.ColorNamelist.Count > i)
                                {
                                    PolishWaferSourceInformations[i].IdentificationColorBrush.Value = ColorPreset.ColorNamelist[i];
                                }
                                else
                                {
                                    LoggerManager.Debug($"TODO");
                                }
                            }

                            //}
                        }));

                        this.PolishWaferModule().SaveDevParameter();
                    }
                    else if (result == EnumMessageDialogResult.NEGATIVE) // CANCEL
                    {

                    }
                }
                else
                {

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _ChangedSizeCommand;
        public ICommand ChangedSizeCommand
        {
            get
            {
                if (null == _ChangedSizeCommand) _ChangedSizeCommand = new RelayCommand<Object>(ChangedSizeCommandFunc);
                return _ChangedSizeCommand;
            }
        }
        private RelayCommand<object> _ChangedNotchTypeCommand;
        public ICommand ChangedNotchTypeCommand
        {
            get
            {
                if (null == _ChangedNotchTypeCommand) _ChangedNotchTypeCommand = new RelayCommand<object>(ChangedSizeCommandFunc);
                return _ChangedNotchTypeCommand;
            }
        }

        private void ChangedSizeCommandFunc(object obj)
        {
            try
            {
                //this.PolishWaferModule().PolishWaferParameter = this.PolishWaferparameters;
                //this.PolishWaferModule().SaveDevParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _DecimalTextBoxClickCommand;
        public ICommand DecimalTextBoxClickCommand
        {
            get
            {
                if (null == _DecimalTextBoxClickCommand) _DecimalTextBoxClickCommand = new RelayCommand<Object>(FuncDecimalTextBoxClickCommand);
                return _DecimalTextBoxClickCommand;
            }
        }

        private void FuncDecimalTextBoxClickCommand(object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private RelayCommand<Object> _FloatTextBoxClickCommand;
        public ICommand FloatTextBoxClickCommand
        {
            get
            {
                if (null == _FloatTextBoxClickCommand) _FloatTextBoxClickCommand = new RelayCommand<Object>(FloatTextBoxClickCommandFunc);
                return _FloatTextBoxClickCommand;
            }
        }

        private void FloatTextBoxClickCommandFunc(object param)
        {
            try
            {
                if (param is System.Windows.Controls.TextBox)
                {
                    System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                    tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.FLOAT | KB_TYPE.DECIMAL);
                    tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _AssignSourceCommand;
        public IAsyncCommand AssignSourceCommand
        {
            get
            {
                if (null == _AssignSourceCommand) _AssignSourceCommand = new AsyncCommand(AssignSourceCommandFunc);
                return _AssignSourceCommand;
            }
        }

        private async Task AssignSourceCommandFunc()
        {
            try
            {
                // TODO: Loader쪽에 파라미터 넣어놔야 함.
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<object> _SelectedObjectCommand;
        public IAsyncCommand SelectedObjectCommand
        {
            get
            {
                if (null == _SelectedObjectCommand) _SelectedObjectCommand = new AsyncCommand<object>(SelectedObjectCommandFunc);
                return _SelectedObjectCommand;
            }
        }

        private async Task SelectedObjectCommandFunc(object param)
        {
            try
            {
                IList<object> paramlist = (IList<object>)param;

                if (paramlist.Count > 0)
                {
                    if (paramlist[0] is string)
                    {
                        if (((string)paramlist[0]).Equals("SlotListView"))
                        {
                            //SlotObject;
                            ListView lv = (ListView)paramlist[1];

                            // TODO:
                            //switch ((lv.DataContext as FoupObject).Index)
                            //{
                            //    case 1:
                            //        SelectedCasstte1Slots = lv.SelectedItems.Cast<SlotObject>().ToList();
                            //        break;
                            //    case 2:
                            //        SelectedCasstte2Slots = lv.SelectedItems.Cast<SlotObject>().ToList();
                            //        break;
                            //    case 3:
                            //        SelectedCasstte3Slots = lv.SelectedItems.Cast<SlotObject>().ToList();
                            //        break;
                            //}
                        }
                        else if (((string)paramlist[0]).Equals("FoupListView"))
                        {
                            //FoupObject
                            ListView lv = (ListView)paramlist[1];
                            //if ((_PreSelectedFoupItemsCount != -1 & _PreSelectedFoupItemsCount != lv.SelectedItems.Count))
                            {
                                // TODO:
                                //foreach (var foup in Foups)
                                //{
                                //    if (foup.IsSelected != foup._PreIsSelected)
                                //    {
                                //        foreach (var slot in foup.Slots)
                                //        {
                                //            slot.IsSelected = foup.IsSelected;
                                //        }

                                //    }
                                //}
                            }
                            //_PreSelectedFoupItemsCount = lv.SelectedItems.Count;
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        // StageObject
                        //SelectedCells = paramlist.Cast<StageObject>().ToList();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _AssignCommand;
        public IAsyncCommand AssignCommand
        {
            get
            {
                if (null == _AssignCommand) _AssignCommand = new AsyncCommand(AssignCommandFunc);
                return _AssignCommand;
            }
        }

        private async Task AssignCommandFunc()
        {
            try
            {
                if (SelectedSourceInfo != null)
                {
                    //TrasnferObjectSet.AssignWaferType(EnumWaferType.POLISH, SelectedSourceInfo.DefineName.Value, SelectedSourceInfo.Size.Value);
                    TrasnferObjectSet.AssignWaferType(EnumWaferType.POLISH, SelectedSourceInfo);

                    TrasnferObjectSet.UnSelectAll();

                    this.DeviceManager().SaveSysParameter();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private AsyncCommand _RemoveCommand;
        public IAsyncCommand RemoveCommand
        {
            get
            {
                if (null == _RemoveCommand) _RemoveCommand = new AsyncCommand(RemoveCommandFunc);
                return _RemoveCommand;
            }
        }

        private async Task RemoveCommandFunc()
        {
            try
            {
                if (TrasnferObjectSet != null)
                {
                    //TrasnferObjectSet.AssignWaferType(EnumWaferType.UNDEFINED, string.Empty, SubstrateSizeEnum.UNDEFINED);
                    TrasnferObjectSet.AssignWaferType(EnumWaferType.UNDEFINED, SelectedSourceInfo);

                    TrasnferObjectSet.UnSelectAll();

                    this.DeviceManager().SaveSysParameter();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void UpdateCleaningParameters(string sourcename)
        {
            try
            {
                if (PolishWaferIntervalParameters.Count > 0)
                {
                    foreach (var intervalparam in PolishWaferIntervalParameters.ToList())
                    {
                        foreach (var cleaningparam in intervalparam.CleaningParameters.ToList())
                        {
                            if (cleaningparam.WaferDefineType.Value == sourcename)
                            {
                                intervalparam.CleaningParameters.Remove(cleaningparam);
                            }
                        }
                    }

                    this.PolishWaferModule().SaveDevParameter();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        //public void SetPolishWaferIParam(byte[] param)
        //{
        //    try
        //    {
        //        object target = null;

        //        var result = SerializeManager.DeserializeFromByte(param, out target, typeof(PolishWaferParameter));

        //        if (target != null)
        //        {
        //            PolishWaferparameters = target as PolishWaferParameter;

        //            this.PolishWaferModule().PolishWaferParameter = PolishWaferparameters;
        //            this.PolishWaferModule().SaveDevParameter();
        //        }
        //        else
        //        {
        //            LoggerManager.Error($"SetPolishWaferIParam function is faild.");
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        //public void SetSelectedObjectCommand(byte[] info)
        //{
        //    try
        //    {
        //        object target = null;

        //        if (info != null)
        //        {
        //            var result = SerializeManager.DeserializeFromByte(info, out target, typeof(PolishWaferInformation));

        //            if (target != null)
        //            {
        //                SelectedSourceInfo = target as PolishWaferInformation;
        //            }
        //            else
        //            {
        //                LoggerManager.Error($"SetSelectedObjectCommand function is faild.");
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        #region Remote commands

        public async Task AddSourceRemoteCommand()
        {
            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");


                if (PolishWaferSourceInformations.Count >= MaximumSourceCount)
                {
                    await this.MetroDialogManager().ShowMessageDialog("[Information]", "Can not add more data.", EnumMessageStyle.Affirmative);
                }
                else

                {

                    EnumMessageDialogResult result = EnumMessageDialogResult.NEGATIVE;

                    result = await this.MetroDialogManager().ShowSingleInputDialog("Name : ", "Add");

                    if (result == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        string devName = string.Empty;

                        Task task = new Task(() =>
                        {
                            devName = this.MetroDialogManager().GetSingleInputData();
                        });
                        task.Start();
                        await task;

                        //await Task.Run(() =>
                        //{
                        //    devName = this.MetroDialogManager().GetSingleInputData();
                        //});

                        if (!string.IsNullOrEmpty(devName))
                        {
                            if (PolishWaferSourceInformations == null)
                            {
                                PolishWaferSourceInformations = new AsyncObservableCollection<IPolishWaferSourceInformation>();
                            }

                            IPolishWaferSourceInformation alreadyHasDevDir = null;
                            bool isProcessChangeDevice = false;

                            if (PolishWaferSourceInformations.Count > 0)
                            {
                                alreadyHasDevDir = PolishWaferSourceInformations.Where(x => x.DefineName.Value == devName).FirstOrDefault();
                            }

                            if (alreadyHasDevDir != null)
                            {
                                isProcessChangeDevice = false;

                                // TODO: 중복된 이름으로 Add 할 수 없다는 내용을 사용자에게 전달해야 함.
                            }
                            else
                            {
                                isProcessChangeDevice = true;
                            }

                            if (isProcessChangeDevice)
                            {
                                System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                                {
                                    PolishWaferInformation tmpinfo = new PolishWaferInformation();
                                    tmpinfo.DefineName.Value = devName;
                                    tmpinfo.IdentificationColorBrush.Value = ColorPreset.ColorNamelist[PolishWaferSourceInformations.Count];
                                    tmpinfo.Thickness.Value = 1000;
                                    tmpinfo.Size.Value = SubstrateSizeEnum.INCH12;
                                    tmpinfo.NotchType.Value = WaferNotchTypeEnum.NOTCH;

                                    PolishWaferSourceInformations.Add(tmpinfo);

                                    if (PolishWaferSourceInformations.Count == 1)
                                    {
                                        SelectedSourceInfo = PolishWaferSourceInformations[0];
                                    }
                                }));

                                this.PolishWaferModule().SaveDevParameter();
                                //await SaveAllDeviceUsingName(devName);
                                //await ChangeDeviceFuncUsingName(devName);
                                //GetDevList();
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

        public async Task RemoveSourceRemoteCommand()
        {
            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                if (SelectedSourceInfo != null)
                {
                    EnumMessageDialogResult result = EnumMessageDialogResult.NEGATIVE;


                    result = await this.MetroDialogManager().ShowMessageDialog("[Information]", "Are you sure you want to remove the data?", EnumMessageStyle.AffirmativeAndNegative);

                    if (result == EnumMessageDialogResult.AFFIRMATIVE)  // OK
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            // SelectedSourceInf가 다른 객체인 경우가 발생 됨.
                            //PolishWaferInformation tmp = PolishWaferSourceInformations.FirstOrDefault(x => x.DefineName.Value == SelectedSourceInfo.DefineName.Value);

                            //if(tmp != null)
                            //{
                            // 삭제하려고 하는 디바이스 이름과 동일한 파라미터를 갖고 있는 경우 같이 삭제해줘야 한다.
                            // 인터벌 파라미터 안에 있는 클리닝 파라미터 전체 검사 진행.

                            if (PolishWaferIntervalParameters.Count > 0)
                            {
                                foreach (var intervalparam in PolishWaferIntervalParameters.ToList())
                                {
                                    foreach (var cleaningparam in intervalparam.CleaningParameters.ToList())
                                    {
                                        if (cleaningparam.WaferDefineType.Value == SelectedSourceInfo.DefineName.Value)
                                        {
                                            // 삭제
                                            intervalparam.CleaningParameters.Remove(cleaningparam);
                                        }
                                    }
                                }
                            }

                            PolishWaferSourceInformations.Remove(SelectedSourceInfo);
                            SelectedSourceInfo = null;

                            for (int i = 0; i < PolishWaferSourceInformations.Count; i++)
                            {
                                if (ColorPreset.ColorNamelist.Count > i)
                                {
                                    PolishWaferSourceInformations[i].IdentificationColorBrush.Value = ColorPreset.ColorNamelist[i];
                                }
                                else
                                {
                                    LoggerManager.Debug($"TODO");
                                }
                            }


                            //}
                        }));

                        this.PolishWaferModule().SaveDevParameter();
                    }
                    else if (result == EnumMessageDialogResult.NEGATIVE) // CANCEL
                    {

                    }
                }
                else
                {

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

        public async Task AssignRemoteCommand()
        {
            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                if (SelectedSourceInfo != null)
                {
                    //TrasnferObjectSet.AssignWaferType(EnumWaferType.POLISH, SelectedSourceInfo.DefineName.Value, SelectedSourceInfo.Size.Value);
                    TrasnferObjectSet.AssignWaferType(EnumWaferType.POLISH, SelectedSourceInfo);

                    TrasnferObjectSet.UnSelectAll();

                    this.DeviceManager().SaveSysParameter();
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

        public async Task RemoveRemoteCommand()
        {
            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                if (TrasnferObjectSet != null)
                {
                    //TrasnferObjectSet.AssignWaferType(EnumWaferType.UNDEFINED, string.Empty, SubstrateSizeEnum.UNDEFINED);
                    TrasnferObjectSet.AssignWaferType(EnumWaferType.UNDEFINED, SelectedSourceInfo);

                    TrasnferObjectSet.UnSelectAll();

                    this.DeviceManager().SaveSysParameter();
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

        public async Task SelectedObjectRemoteCommand(object param)
        {
            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                IList<object> paramlist = (IList<object>)param;

                if (paramlist.Count > 0)
                {
                    if (paramlist[0] is string)
                    {
                        if (((string)paramlist[0]).Equals("SlotListView"))
                        {
                            //SlotObject;
                            ListView lv = (ListView)paramlist[1];

                            // TODO:
                            //switch ((lv.DataContext as FoupObject).Index)
                            //{
                            //    case 1:
                            //        SelectedCasstte1Slots = lv.SelectedItems.Cast<SlotObject>().ToList();
                            //        break;
                            //    case 2:
                            //        SelectedCasstte2Slots = lv.SelectedItems.Cast<SlotObject>().ToList();
                            //        break;
                            //    case 3:
                            //        SelectedCasstte3Slots = lv.SelectedItems.Cast<SlotObject>().ToList();
                            //        break;
                            //}
                        }
                        else if (((string)paramlist[0]).Equals("FoupListView"))
                        {
                            //FoupObject
                            ListView lv = (ListView)paramlist[1];
                            //if ((_PreSelectedFoupItemsCount != -1 & _PreSelectedFoupItemsCount != lv.SelectedItems.Count))
                            {
                                // TODO:
                                //foreach (var foup in Foups)
                                //{
                                //    if (foup.IsSelected != foup._PreIsSelected)
                                //    {
                                //        foreach (var slot in foup.Slots)
                                //        {
                                //            slot.IsSelected = foup.IsSelected;
                                //        }

                                //    }
                                //}
                            }
                            //_PreSelectedFoupItemsCount = lv.SelectedItems.Count;
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        // StageObject
                        //SelectedCells = paramlist.Cast<StageObject>().ToList();
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

        #endregion

    }
}