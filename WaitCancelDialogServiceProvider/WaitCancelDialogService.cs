using System;

namespace WaitCancelDialogServiceProvider
{
    using MahApps.Metro.Controls.Dialogs;
    using System.Windows;
    using System.ComponentModel;
    using System.Windows.Input;
    using System.Threading;
    using ProberErrorCode;
    using ProberInterfaces;
    using LogModule;
    using System.Runtime.CompilerServices;
    using MetroDialogInterfaces;
    using System.Diagnostics;

    public class WaitCancelDialogService : INotifyPropertyChanged, IModule
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

        public Autofac.IContainer Container { get; set; }

        private WaitCancelDialog _waitCancelDialog;

        public WaitCancelDialog WCDialog
        {
            get { return _waitCancelDialog; }
            set { _waitCancelDialog = value; }
        }

        public CancellationTokenSource cancellationToken;

        private bool _IsOpenWaitCancelDialog;

        public bool IsOpenWaitCancelDialog
        {
            get { return _IsOpenWaitCancelDialog; }
            set { _IsOpenWaitCancelDialog = value; }
        }

        private string _Message;
        public string Message
        {
            get { return _Message; }
            set
            {
                if (value != _Message)
                {
                    _Message = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _IsUseCancelButton;
        public Visibility IsUseCancelButton
        {
            get { return _IsUseCancelButton; }
            set
            {
                if (value != _IsUseCancelButton)
                {
                    _IsUseCancelButton = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _HashCode;
        public string HashCode
        {
            get { return _HashCode; }
            set
            {
                if (value != _HashCode)
                {
                    _HashCode = value;
                    RaisePropertyChanged();
                }
            }
        }

        // Cancel Button에 표시할 Content 문자열
        private string _cancelButtonContent;
        public string cancelButtonContent
        {
            get { return _cancelButtonContent; }
            set
            {
                if (value != _cancelButtonContent)
                {
                    _cancelButtonContent = value;
                    RaisePropertyChanged();
                }
            }
        }

        // Cancel Button Enable Flag
        private bool _isEnableCancelButton = true;
        public bool isEnableCancelButton
        {
            get { return _isEnableCancelButton; }
            set
            {
                if (value != _isEnableCancelButton)
                {
                    _isEnableCancelButton = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ICommand _cmdNegativeButtonClick;
        public ICommand cmdNegativeButtonClick
        {
            get { return _cmdNegativeButtonClick; }
            set
            {
                if (value != _cmdNegativeButtonClick)
                {
                    _cmdNegativeButtonClick = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Timer _timer;

        private Stopwatch _waitTimer = new Stopwatch();
        public Stopwatch WaitTimer
        {
            get { return _waitTimer; }
            set
            {
                if (value != _waitTimer)
                {
                    _waitTimer = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string WaitTimerElapsed
        {
            get { return WaitTimer.Elapsed.ToString(@"hh\:mm\:ss"); }
        }
        public void SetContainer(Autofac.IContainer container)
        {
            Container = container;
        }
        public CustomDialog GetDialog()
        {
            CustomDialog retval = null;

            try
            {
                retval = WCDialog;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public void StartTimer(bool reset = false)
        {
            try
            {
                if (reset)
                {
                    ResetTimer();
                }

                if (_timer == null)
                {
                    _timer = new Timer(TimerCallback, null, 0, 1000);
                }

                WaitTimer.Reset();
                WaitTimer.Start();
                _timer.Change(0, 1000);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void ResetTimer(bool start = false)
        {
            try
            {
                if (_timer != null)
                {
                    // Stop the timer
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);

                    WaitTimer.Reset();
                }

                if(start)
                {
                    StartTimer();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void TimerCallback(object state)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    RaisePropertyChanged("WaitTimerElapsed");
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetMessageData(string message)
        {
            try
            {
                this.Message = message;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetData(string message, string hashcode, CancellationTokenSource canceltokensource = null, string cancelButtonText = "", bool restrattimer = false)
        {
            try
            {
                this.Message = message;
                this.cancellationToken = canceltokensource;
                this.HashCode = hashcode;
                this.cancelButtonContent = cancelButtonText;

                if (this.cancellationToken == null)
                {
                    IsUseCancelButton = Visibility.Hidden;
                }
                else
                {
                    IsUseCancelButton = Visibility.Visible;
                    isEnableCancelButton = true; // Cancel 버튼 사용 시 버튼 Enable 상태로 초기화
                }

                if (this.cancelButtonContent != "Cancel")
                {
                    IsUseCancelButton = Visibility.Visible;
                    isEnableCancelButton = true;
                }

                if(this.cancelButtonContent == string.Empty)
                {
                    this.cancelButtonContent = "Cancel";
                }

                StartTimer(restrattimer);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        public void ClearData(bool starttimer = false)
        {
            try
            {
                this.Message = "Wait";
                this.cancellationToken = null;
                this.HashCode = null;
                this.cancelButtonContent = "Cancel";

                ResetTimer(starttimer);

                IsUseCancelButton = Visibility.Hidden;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ShowCancelButton(CancellationTokenSource canceltokensource = null, string cancelButtonText = "")
        {
            try
            {
                this.cancellationToken = canceltokensource;
                this.cancelButtonContent = cancelButtonText;

                if (this.cancellationToken == null)
                {
                    IsUseCancelButton = Visibility.Hidden;
                }
                else
                {
                    IsUseCancelButton = Visibility.Visible;
                    isEnableCancelButton = true; // Cancel 버튼 사용 시 버튼 Enable 상태로 초기화
                }

                if (this.cancelButtonContent != "Cancel")
                {
                    IsUseCancelButton = Visibility.Visible;
                    isEnableCancelButton = true;
                }

                if (this.cancelButtonContent == string.Empty)
                {
                    this.cancelButtonContent = "Cancel";
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void HiddenCancelButton()
        {
            try
            {
                IsUseCancelButton = Visibility.Hidden;
                isEnableCancelButton = false; 
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ShowCancelButton()
        {
            try
            {
                IsUseCancelButton = Visibility.Visible;
                isEnableCancelButton = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        WCDialog = new WaitCancelDialog();
                        WCDialog.DataContext = this;
                        WCDialog.DialogSettings.AnimateShow = false;
                        WCDialog.DialogSettings.AnimateHide = false;

                        WCDialog.DialogSettings.OwnerCanCloseWithDialog = true;
                    });

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
                retval = EventCodeEnum.UNDEFINED;

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
    }
}
