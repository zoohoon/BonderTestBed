using System;
using System.Threading.Tasks;

namespace WaferIDManualDialog
{
    using Autofac;
    using LoaderBase;
    using LoaderBase.AttachModules.ModuleInterfaces;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using RelayCommandBase;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Input;

    public class VmWaferIDMainPage : IMainScreenViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        #region ==> WaferIDString
        private String _WaferIDString;
        public String WaferIDString
        {
            get { return _WaferIDString; }
            set
            {
                if (value != _WaferIDString)
                {
                    _WaferIDString = value;
                    if (_WaferIDString == String.Empty)
                        WaferIDInputBoxBrush = Brushes.LightGray;

                    CurTextLength = WaferIDString.Length;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        private string _TargetInfo;

        public string TargetInfo
        {
            get { return _TargetInfo; }
            set { _TargetInfo = value; }
        }

        private string _CurrModule;

        public string CurrModule
        {
            get { return _CurrModule; }
            set { _CurrModule = value; }
        }

        #region ==> WaferIDInputBoxBrush
        private Brush _WaferIDInputBoxBrush;
        public Brush WaferIDInputBoxBrush
        {
            get { return _WaferIDInputBoxBrush; }
            set
            {
                if (value != _WaferIDInputBoxBrush)
                {
                    _WaferIDInputBoxBrush = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> EnterKeyCommand
        private RelayCommand _EnterKeyCommand;
        public ICommand EnterKeyCommand
        {
            get
            {
                if (null == _EnterKeyCommand) _EnterKeyCommand = new RelayCommand(EnterKeyCommandFunc);
                return _EnterKeyCommand;
            }
        }
        private void EnterKeyCommandFunc()
        {
            //bool cheksumResult = false;
            String upperWaferIDString = WaferIDString.ToUpper();

            if (WaferIDString == null || WaferIDString == "")
            {
                MessageBox.Show($"ProbeWafer ID is Empty", "", MessageBoxButton.OK);
            }
            else
            {
                if (MessageBox.Show($"ProbeWafer ID: {WaferIDString} \nIs Correct? ", "Probe Wafer ID Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if (WaferObject != null)
                    {
                        WaferObject.OCR.Value = WaferIDString;
                    }
                    _Win.Close();
                }
                else
                {

                }
            }
            //var retVal = (this).MessageDialogService().ShowDialog("Probe Wafer ID Confirm", $"Is Probe Wafer ID: {WaferIDString} Correct? ", EnumMessageStyle.AffirmativeAndNegative).Result;

            //if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
            //{
            //    if(WaferObject!=null)
            //    {
            //        WaferObject.ProbeWaferID.Value= WaferIDString;
            //    }
            //    _Win.Close();
            //}
            //else
            //{

            //}
        }
        #endregion

        #region ==> ExitCommand
        private RelayCommand _ExitCommand;
        public ICommand ExitCommand
        {
            get
            {
                if (null == _ExitCommand) _ExitCommand = new RelayCommand(ExitCommandFunc);
                return _ExitCommand;
            }
        }

        private void ExitCommandFunc()
        {
            //if (this.LoaderController().OFR_WaferIDFail() == EventCodeEnum.UNDEFINED)
            //    this.MessageDialogService().ShowDialog("WaferID", "[ERROR] change WaferID module status to fail", EnumMessageStyle.Affirmative);
            if (MessageBox.Show($"Do you really want to Exit? ", "Exit", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _Win.Close();
                //throw new Exception();
            }
            //    var retVal = (this).MessageDialogService().ShowDialog("Exit", $"Do you really want to Exit?", EnumMessageStyle.AffirmativeAndNegative).Result;

            //if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
            //{
            //    _Win.Close();
            //}
            //else
            //{

            //}
        }
        #endregion

        #region ==> ConnectDisplayCommand
        private RelayCommand _ConnectDisplayCommand;
        public ICommand ConnectDisplayCommand
        {
            get
            {
                if (null == _ConnectDisplayCommand) _ConnectDisplayCommand = new RelayCommand(ConnectDisplayCommandFunc);
                return _ConnectDisplayCommand;
            }
        }
        private void ConnectDisplayCommandFunc()
        {
            if (_CognexProcessManager.ConnectDisplay(_CognexIP).Result == false)
                return;

        }
        #endregion










        private Object lockObject = new Object();





        #region ==> CurTextLength
        private int _CurTextLength;
        public int CurTextLength
        {
            get { return _CurTextLength; }
            set
            {
                if (value != _CurTextLength)
                {
                    _CurTextLength = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> MaxTextLength
        private int _MaxTextLength;
        public int MaxTextLength
        {
            get { return _MaxTextLength; }
            set
            {
                if (value != _MaxTextLength)
                {
                    _MaxTextLength = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        private String _CognexIP = string.Empty;
        //private CognexConfig _ManualConfig = null;
        private TransferObject WaferObject = null;
        public Autofac.IContainer _Container;
        private ICognexProcessManager _CognexProcessManager = null;

        readonly Guid _ViewModelGUID = new Guid("8394ca3a-5b6e-4110-b71f-c84672649a94");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        private Window _Win;

        public void SetWindow(Window win)
        {
            _Win = win;
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            if (Initialized)
            {
                return EventCodeEnum.DUPLICATE_INVOCATION;
            }

            WaferIDString = String.Empty;
            WaferIDInputBoxBrush = Brushes.LightGray;

            Initialized = true;

            MaxTextLength = 15;

            retval = EventCodeEnum.NONE;

            return retval;
        }

        public EventCodeEnum InitModule(TransferObject transobj)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            if (Initialized)
            {
                return EventCodeEnum.DUPLICATE_INVOCATION;
            }
            WaferIDString = String.Empty;
            WaferIDInputBoxBrush = Brushes.LightGray;

            Initialized = true;

            MaxTextLength = 15;
            WaferObject = transobj;


            var loader = _Container.Resolve<ILoaderModule>();
            
            if(loader != null)
            {
                TargetInfo = $"  Origin:{loader.SlotToFoupConvert(transobj.OriginHolder)}";
            }
    

            retval = EventCodeEnum.NONE;

            return retval;
        }
        //private int HostIndex = 0;

        public Task<EventCodeEnum> InitViewModel()
        {
            try
            {
                Task<EventCodeEnum> t = new Task<EventCodeEnum>(() => { return EventCodeEnum.NONE; });


                return t;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            try
            {
                Task<EventCodeEnum> t = new Task<EventCodeEnum>(() =>
                {
                    return EventCodeEnum.NONE;
                });

                return t;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            try
            {
                return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            try
            {
                Task<EventCodeEnum> t = new Task<EventCodeEnum>(() =>
                {
                    return EventCodeEnum.NONE;
                });

                return t;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void DeInitModule()
        {
        }

    }
}
