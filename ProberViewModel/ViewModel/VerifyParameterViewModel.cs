namespace ProberViewModel.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using RelayCommandBase;
    using Autofac;
    using VirtualKeyboardControl;

    public class VerifyParameterViewModel : IMainScreenViewModel, INotifyPropertyChanged, IFactoryModule
    {
        #region // ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        #region <remarks> Property </remarks>
        readonly Guid _ViewModelGUID = new Guid("692b8605-9906-4ff9-8441-1b1c0c52c44d");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        public bool Initialized { get; set; } = false;

        private double _MinValue;
        public double MinValue
        {
            get { return _MinValue; }
            set
            {
                if (value != _MinValue)
                {
                    _MinValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _MaxValue;
        public double MaxValue
        {
            get { return _MaxValue; }
            set
            {
                if (value != _MaxValue)
                {
                    _MaxValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _CurrnetValue;
        public double CurrnetValue
        {
            get { return _CurrnetValue; }
            set
            {
                if (value != _CurrnetValue)
                {
                    _CurrnetValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<ElementInfo> _ElementList
             = new ObservableCollection<ElementInfo>();
        public ObservableCollection<ElementInfo> ElementList
        {
            get { return _ElementList; }
            set
            {
                if (value != _ElementList)
                {
                    _ElementList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<VerifyParamInfo> _ListViewInfos
            = new ObservableCollection<VerifyParamInfo>();
        public ObservableCollection<VerifyParamInfo> ListViewInfos
        {
            get { return _ListViewInfos; }
            set
            {
                if (value != _ListViewInfos)
                {
                    _ListViewInfos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _SelectedIndex = -1;
        public int SelectedIndex
        {
            get { return _SelectedIndex; }
            set
            {
                if (value != _SelectedIndex)
                {
                    BackupValue();
                    _SelectedIndex = value;
                    RaisePropertyChanged();
                    UpdateValue();
                }
            }
        }

        private bool _IsEnable;
        public bool IsEnable
        {
            get { return _IsEnable; }
            set
            {
                if (value != _IsEnable)
                {
                    _IsEnable = value;
                    RaisePropertyChanged();
                }
            }
        }


        private IParamManager ParamManager { get; set; }


        #endregion

        #region <remarks> Command & Command Method </remarks>
        private AsyncCommand _SaveParamCommand;
        public ICommand SaveParamCommand
        {
            get
            {
                if (null == _SaveParamCommand) _SaveParamCommand = new AsyncCommand(SaveParamCommandFunc);
                return _SaveParamCommand;
            }
        }

        private async Task SaveParamCommandFunc()
        {
            try
            {
                ParamManager.SetVerifyParameterBeforeStartLotEnable(IsEnable);

                List<VerifyParamInfo> infos = new List<VerifyParamInfo>();

                if (ListViewInfos != null)
                {
                    foreach (var info in ListViewInfos)
                    {
                        infos.Add(new VerifyParamInfo()
                        {
                            PropertyName = info.PropertyName,
                            Description = info.Description,
                            MaxValve = info.MaxValve,
                            MinValue = info.MinValue,
                            EnumProperty = info.EnumProperty
                        });

                    }
                }
                ParamManager.SetVerifyParamInfo(infos);
                var retVal = ParamManager.SaveDevParameter();
                if(retVal == EventCodeEnum.NONE)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Information Message", "Success Save Parameter", 
                        MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("Information Message", "Fail Save Parameter",
                       MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private AsyncCommand _ExcuteCommand;
        public ICommand ExcuteCommand
        {
            get
            {
                if (null == _ExcuteCommand) _ExcuteCommand = new AsyncCommand(ExcuteCommandFunc);
                return _ExcuteCommand;
            }
        }

        private async Task ExcuteCommandFunc()
        {
            try
            {
                var retVal = ParamManager.VerifyLotVIDsCheckBeforeLot();
                if (retVal == EventCodeEnum.NONE)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Information Message", "Success Verify Parameter",
                        MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("Information Message", "Fail Verify Parameter",
                       MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void UpdateValue()
        {
            try
            {
                if (SelectedIndex != -1)
                {
                    if (ListViewInfos.Count != 0 && ListViewInfos.Count >= SelectedIndex)
                    {
                        MaxValue = ListViewInfos[SelectedIndex].MaxValve;
                        MinValue = ListViewInfos[SelectedIndex].MinValue;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private void BackupValue()
        {
            try
            {
                if(SelectedIndex != -1)
                {
                    if (ListViewInfos.Count != 0 && ListViewInfos.Count >= SelectedIndex)
                    {
                        ListViewInfos[SelectedIndex].MaxValve = MaxValue;
                        ListViewInfos[SelectedIndex].MinValue = MinValue;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _TextBoxClickCommand;
        public ICommand TextBoxClickCommand
        {
            get
            {
                if (null == _TextBoxClickCommand) _TextBoxClickCommand = new RelayCommand<Object>(TextBoxClickCommandFunc);
                return _TextBoxClickCommand;
            }
        }


        private void TextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL | KB_TYPE.FLOAT);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();

                BackupValue();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        #endregion

        #region <remarks> IMainScreenViewModel Method </remarks>

        public Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober)
                {
                    ParamManager = this.ParamManager();
                }
                else
                {
                    ParamManager = this.GetLoaderContainer().Resolve<ILoaderParamManager>();
                }


                ListViewInfos = new ObservableCollection<VerifyParamInfo>();
                IsEnable = ParamManager.GetVerifyParameterBeforeStartLotEnable();
                var infos = ParamManager.GetVerifyParamInfo();
                if (infos != null)
                {
                    foreach (var info in infos)
                    {
                        ListViewInfos.Add(new VerifyParamInfo()
                        {
                            PropertyName = info.PropertyName,
                            Description = info.Description,
                            MaxValve = info.MaxValve,
                            MinValue = info.MinValue,
                            EnumProperty = info.EnumProperty
                        });
                    }
                }
                //SetDevElementList();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public void DeInitModule()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
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

    }

    public class VIDValueInfo : INotifyPropertyChanged
    {
        #region // ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        private long _VID;
        public long VID
        {
            get { return _VID; }
            set
            {
                if (value != _VID)
                {
                    _VID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _Value;
        public double Value
        {
            get { return _Value; }
            set
            {
                if (value != _Value)
                {
                    _Value = value;
                    RaisePropertyChanged();
                }
            }
        }

    }

    public class ElementInfo : INotifyPropertyChanged
    {
        #region // ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        private int _ElementID;
        public int ElementID
        {
            get { return _ElementID; }
            set
            {
                if (value != _ElementID)
                {
                    _ElementID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Path;
        public string Path
        {
            get { return _Path; }
            set
            {
                if (value != _Path)
                {
                    _Path = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _VID;
        public int VID
        {
            get { return _VID; }
            set
            {
                if (value != _VID)
                {
                    _VID = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ElementInfo(int elementID, string path, int vid)
        {
            ElementID = elementID;
            Path = path;
            VID = vid;
        }
    }


    //public class VerifyParameterViewModel : IMainScreenViewModel, INotifyPropertyChanged, IFactoryModule
    //{
    //    #region // ==> PropertyChanged
    //    public event PropertyChangedEventHandler PropertyChanged;
    //    private void RaisePropertyChanged([CallerMemberName] string propName = null)
    //        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    //    #endregion

    //    #region <remarks> Property </remarks>
    //    readonly Guid _ViewModelGUID = new Guid("692b8605-9906-4ff9-8441-1b1c0c52c44d");
    //    public Guid ScreenGUID { get { return _ViewModelGUID; } }

    //    public bool Initialized { get; set; } = false;

    //    private double _MinValue;
    //    public double MinValue
    //    {
    //        get { return _MinValue; }
    //        set
    //        {
    //            if (value != _MinValue)
    //            {
    //                _MinValue = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //    private double _MaxValue;
    //    public double MaxValue
    //    {
    //        get { return _MaxValue; }
    //        set
    //        {
    //            if (value != _MaxValue)
    //            {
    //                _MaxValue = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //    private double _CurrnetValue;
    //    public double CurrnetValue
    //    {
    //        get { return _CurrnetValue; }
    //        set
    //        {
    //            if (value != _CurrnetValue)
    //            {
    //                _CurrnetValue = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //    private string _SearchText;
    //    public string SearchText
    //    {
    //        get { return _SearchText; }
    //        set
    //        {
    //            if (value != _SearchText)
    //            {
    //                _SearchText = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }


    //    private ObservableCollection<ElementInfo> _ElementList
    //         = new ObservableCollection<ElementInfo>();
    //    public ObservableCollection<ElementInfo> ElementList
    //    {
    //        get { return _ElementList; }
    //        set
    //        {
    //            if (value != _ElementList)
    //            {
    //                _ElementList = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //    private IParamManager ParamManager { get; set; }


    //    #endregion

    //    #region <remarks> Command & Command Method </remarks>
    //    private RelayCommand _ImportDeviceElementsCommand;
    //    public ICommand ImportDeviceElementsCommand
    //    {
    //        get
    //        {
    //            if (null == _ImportDeviceElementsCommand) _ImportDeviceElementsCommand = new RelayCommand(ImportDeviceElementsCommandFunc);
    //            return _ImportDeviceElementsCommand;
    //        }
    //    }

    //    private void ImportDeviceElementsCommandFunc()
    //    {
    //        try
    //        {
    //        }
    //        catch (Exception err)
    //        {
    //            LoggerManager.Exception(err);
    //        }
    //    }


    //    private RelayCommand _ImportSystemElementsCommand;
    //    public ICommand ImportSystemElementsCommand
    //    {
    //        get
    //        {
    //            if (null == _ImportSystemElementsCommand) _ImportSystemElementsCommand = new RelayCommand(ImportSystemElementsCommandFunc);
    //            return _ImportSystemElementsCommand;
    //        }
    //    }

    //    private void ImportSystemElementsCommandFunc()
    //    {
    //        try
    //        {
    //        }
    //        catch (Exception err)
    //        {
    //            LoggerManager.Exception(err);
    //        }
    //    }


    //    private void SetDevElementList()
    //    {
    //        try
    //        {
    //            var devElementList = this.ParamManager().GetDevElementList();
    //            ElementList.Clear();
    //            foreach (var devElement in devElementList)
    //            {
    //                ElementList.Add(new ElementInfo(devElement.ElementID, devElement.PropertyPath, devElement.VID));
    //            }
    //        }
    //        catch (Exception err)
    //        {
    //            LoggerManager.Exception(err);
    //        }
    //    }

    //    #endregion

    //    #region <remarks> IMainScreenViewModel Method </remarks>

    //    public Task<EventCodeEnum> InitViewModel()
    //    {
    //        EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
    //        try
    //        {

    //        }
    //        catch (Exception err)
    //        {
    //            LoggerManager.Exception(err);
    //        }
    //        return Task.FromResult<EventCodeEnum>(retVal);
    //    }

    //    public Task<EventCodeEnum> PageSwitched(object parameter = null)
    //    {
    //        EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
    //        try
    //        {
    //            ParamManager = this.ParamManager();
    //            SetDevElementList();
    //        }
    //        catch (Exception err)
    //        {
    //            LoggerManager.Exception(err);
    //        }
    //        return Task.FromResult<EventCodeEnum>(retVal);
    //    }

    //    public Task<EventCodeEnum> Cleanup(object parameter = null)
    //    {
    //        EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
    //        try
    //        {

    //        }
    //        catch (Exception err)
    //        {
    //            LoggerManager.Exception(err);
    //        }
    //        return Task.FromResult<EventCodeEnum>(retVal);
    //    }

    //    public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
    //    {
    //        EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
    //        try
    //        {

    //        }
    //        catch (Exception err)
    //        {
    //            LoggerManager.Exception(err);
    //        }
    //        return Task.FromResult<EventCodeEnum>(retVal);
    //    }

    //    public void DeInitModule()
    //    {
    //        try
    //        {

    //        }
    //        catch (Exception err)
    //        {
    //            LoggerManager.Exception(err);
    //        }
    //    }

    //    public EventCodeEnum InitModule()
    //    {
    //        EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
    //        try
    //        {

    //        }
    //        catch (Exception err)
    //        {
    //            LoggerManager.Exception(err);
    //        }
    //        return retVal;
    //    }

    //    #endregion

    //}


}
