using System;
using System.Threading.Tasks;

namespace CardIDManualDialog
{
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

    public class VmCardIDMainPage : IMainScreenViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        #region ==> CardIDString
        private String _CardIDString;
        public String CardIDString
        {
            get { return _CardIDString; }
            set
            {
                if (value != _CardIDString)
                {
                    _CardIDString = value;
                    if (_CardIDString == String.Empty)
                        CardIDInputBoxBrush = Brushes.LightGray;

                    CurTextLength = CardIDString.Length;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> CardIDInputBoxBrush
        private Brush _CardIDInputBoxBrush;
        public Brush CardIDInputBoxBrush
        {
            get { return _CardIDInputBoxBrush; }
            set
            {
                if (value != _CardIDInputBoxBrush)
                {
                    _CardIDInputBoxBrush = value;
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
            String upperCardIDString = CardIDString.ToUpper();

            if (CardIDString == null || CardIDString == "")
            {
                MessageBox.Show($"ProbeCard ID is Empty","",MessageBoxButton.OK);
            }
            else if(CardIDString.Length < 2)
            {
                MessageBox.Show($"Please enter at least 2 characters of Card ID","", MessageBoxButton.OK);
            }
            else
            {
                if (MessageBox.Show($"ProbeCard ID: {CardIDString} \nIs Correct? ", "Probe Card ID Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if (cardObject != null)
                    {
                        cardObject.ProbeCardID.Value = CardIDString;
                    }
                    _Win.Close();
                }
                else
                {

                }
            }

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

            if (MessageBox.Show($"Do you really want to Exit? ", "Exit", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (cardObject != null)
                {
                    cardObject.ProbeCardID.Value = string.Empty;
                }
                _Win.Close();
            }

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
        private TransferObject cardObject = null;
        public Autofac.IContainer _Container;
        private ICognexProcessManager _CognexProcessManager = null;

        readonly Guid _ViewModelGUID = new Guid("4a47078e-51f2-4f75-8bcc-e9af26bbb2a2");
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

            CardIDString = String.Empty;
            CardIDInputBoxBrush = Brushes.LightGray;

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
            CardIDString = String.Empty;
            CardIDInputBoxBrush = Brushes.LightGray;

            Initialized = true;

            MaxTextLength = 15;
            cardObject = transobj;
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
