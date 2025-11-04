namespace RepeatedTransferDialog
{
    using Autofac;
    using LoaderBase.FactoryModules.ViewModelModule;
    using ProberInterfaces;
    using ProberInterfaces.ControlClass.ViewModel;
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows;
    using RelayCommandBase;
    using System.Windows.Input;
    using LogModule;
    using System.Threading;
    using ProberInterfaces.Enum;

    public class RepeatedTransferVM : IFactoryModule, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private static MainWindow wd = null;
        public bool Show()
        {
            bool isCheck = false;
            try
            {
                if (LoaderViewModelManager.CurrentVM is ILoaderHandlingViewModel)
                {
                    if (wd != null)
                    {
                        wd.Show();
                        return isCheck;
                    }
                    String retValue = String.Empty;
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        wd = new MainWindow(this);
                        wd.Show();
                    });
                    //TimeSpan time = (LoaderViewModelManager.CurrentVM as ILoaderHandlingViewModel).GetRTElapsedTimeTotal();
                    //if(time.TotalSeconds != 0)
                    //{
                    //    ControlEnable = false;
                    //}
                }
                else
                {
                    LoggerManager.Debug($"[RepeatedTransferVM] CardLoadingTestCommandFunc() CurrentVM is not LoaderHandlingViewModel");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isCheck;
        }

        private AsyncCommand _RepeatAbortCommand;
        public ICommand RepeatAbortCommand
        {
            get
            {
                if (null == _RepeatAbortCommand) _RepeatAbortCommand = new AsyncCommand(RepeatAbortCommandFunc);
                return _RepeatAbortCommand;
            }
        }

        public async Task RepeatAbortCommandFunc()
        {
            if (LoaderViewModelManager.CurrentVM is ILoaderHandlingViewModel)
            {
                await (LoaderViewModelManager.CurrentVM as ILoaderHandlingViewModel).CancelTansferFunc();
                TransferDone = (LoaderViewModelManager.CurrentVM as ILoaderHandlingViewModel).GetTransferDoneState();
                while (!TransferDone)
                {
                    TransferDone = (LoaderViewModelManager.CurrentVM as ILoaderHandlingViewModel).GetTransferDoneState();
                    Thread.Sleep(100);
                }
                ControlEnable = true;
            }
            else
            {
                LoggerManager.Debug($"[RepeatedTransferVM] RepeatAbortCommandFunc() CurrentVM is not LoaderHandlingViewModel");
            }
        }

        private AsyncCommand _RepeatTransferCommand;
        public ICommand RepeatTransferCommand
        {
            get
            {
                if (null == _RepeatTransferCommand) _RepeatTransferCommand = new AsyncCommand(RepeatTransferCommandFunc, false);
                return _RepeatTransferCommand;
            }
        }

        public async Task RepeatTransferCommandFunc()
        {
            try
            {
                object sourceObj = (LoaderViewModelManager.CurrentVM as ILoaderHandlingViewModel).getSourceObject();
                object targeteObj = (LoaderViewModelManager.CurrentVM as ILoaderHandlingViewModel).getTargetObject();

                if (RpeatedTransferMode == EnumRepeatedTransferMode.OneCellMode)
                {
                    if(sourceObj == null || targeteObj == null)
                    {
                        MessageBox.Show("Please select the Source and Target", "SourceObj or TargetObj is null", MessageBoxButton.OK);
                        LoggerManager.Debug($"[RepeatedTransferVM] RepeatTransferCommandFunc() SourceObj or TargetObj is null({RpeatedTransferMode})");
                        return;
                    }
                }
                else if (RpeatedTransferMode == EnumRepeatedTransferMode.MultipleCellMode)
                {
                    if (sourceObj == null)
                    {
                        MessageBox.Show("Please select the Source", "SourceObj or TargetObj is null", MessageBoxButton.OK);
                        LoggerManager.Debug($"[RepeatedTransferVM] RepeatTransferCommandFunc() SourceObj or TargetObj is null({RpeatedTransferMode})");
                        return;
                    }
                }
                
                if (LoaderViewModelManager.CurrentVM is ILoaderHandlingViewModel)
                {
                    await (LoaderViewModelManager.CurrentVM as ILoaderHandlingViewModel).RepeatTransfer(CardID, PinAlignInterval, RepeatCount, SkipDocking, RpeatedTransferMode, RepeatDelayTime);
                    await GetCurrentCount();
                }
                else
                {
                    LoggerManager.Debug($"[RepeatedTransferVM] RepeatTransferCommandFunc() CurrentVM is not LoaderHandlingViewModel");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async Task GetCurrentCount()
        {
            try
            {
                Task task = new Task(() =>
                {
                    if (LoaderViewModelManager.CurrentVM is ILoaderHandlingViewModel)
                    {
                        TransferDone = (LoaderViewModelManager.CurrentVM as ILoaderHandlingViewModel).GetTransferDoneState();
                    }
                    while (!TransferDone)
                    {
                        ControlEnable = false;
                        CurrentCount = (LoaderViewModelManager.CurrentVM as ILoaderHandlingViewModel).GetTransferCurrentCount();
                        TransferDone = (LoaderViewModelManager.CurrentVM as ILoaderHandlingViewModel).GetTransferDoneState();
                        RTElapsedTimeLasted = (LoaderViewModelManager.CurrentVM as ILoaderHandlingViewModel).GetRTElapsedTimeLasted();
                        RTElapsedTimeTotal = (LoaderViewModelManager.CurrentVM as ILoaderHandlingViewModel).GetRTElapsedTimeTotal();
                        Thread.Sleep(10);
                    }
                    ControlEnable = true;
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public ILoaderViewModelManager LoaderViewModelManager => this.GetLoaderContainer().Resolve<ILoaderViewModelManager>();

        private EnumRepeatedTransferMode _RpeatedTransferMode = EnumRepeatedTransferMode.OneCellMode;
        public EnumRepeatedTransferMode RpeatedTransferMode
        {

            get { return _RpeatedTransferMode; }
            set
            {
                if (value != _RpeatedTransferMode)
                {
                    _RpeatedTransferMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _TransferDone;
        public bool TransferDone
        {

            get { return _TransferDone; }
            set
            {
                if (value != _TransferDone)
                {
                    _TransferDone = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _ControlEnable = true;
        public bool ControlEnable
        {

            get { return _ControlEnable; }
            set
            {
                if (value != _ControlEnable)
                {
                    _ControlEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _RepeatCount;
        public int RepeatCount
        {

            get { return _RepeatCount; }
            set
            {
                if (value != _RepeatCount)
                {
                    _RepeatCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _PinAlignInterval;
        public int PinAlignInterval
        {

            get { return _PinAlignInterval; }
            set
            {
                if (value != _PinAlignInterval)
                {
                    _PinAlignInterval = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _CurrentCount;
        public int CurrentCount
        {

            get { return _CurrentCount; }
            set
            {
                if (value != _CurrentCount)
                {
                    _CurrentCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _RepeatDelayTime = 1000;
        public int RepeatDelayTime
        {

            get { return _RepeatDelayTime; }
            set
            {
                if (value != _RepeatDelayTime)
                {
                    _RepeatDelayTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _CardID;
        public string CardID
        {

            get { return _CardID; }
            set
            {
                if (value != _CardID)
                {
                    _CardID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _SkipDocking = false;
        public bool SkipDocking
        {
            get { return _SkipDocking; }
            set
            {
                if (value != _SkipDocking)
                {
                    _SkipDocking = value;
                    RaisePropertyChanged();
                }
            }
        }

        private TimeSpan _RTElapsedTimeTotal;
        public TimeSpan RTElapsedTimeTotal
        {

            get { return _RTElapsedTimeTotal; }
            set
            {
                if (value != _RTElapsedTimeTotal)
                {
                    _RTElapsedTimeTotal = value;
                    RaisePropertyChanged();
                }
            }
        }

        private TimeSpan _RTElapsedTimeLasted;
        public TimeSpan RTElapsedTimeLasted
        {

            get { return _RTElapsedTimeLasted; }
            set
            {
                if (value != _RTElapsedTimeLasted)
                {
                    _RTElapsedTimeLasted = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}