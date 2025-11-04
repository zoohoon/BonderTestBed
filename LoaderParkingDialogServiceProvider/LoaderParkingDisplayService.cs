using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace LoaderParkingDialogServiceProvider
{
    using Autofac;
    using LoaderBase;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.DialogControl;
    using RelayCommandBase;
    using System.Windows;

    public class LoaderParkingDisplayService : INotifyPropertyChanged, ILoaderParkingDisplayDialogService, IFactoryModule
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        public InitPriorityEnum InitPriority => InitPriorityEnum.LEVEL3;
        public bool Initialized { get; set; } = false;
        private LoaderParkingDisplayDialog LoaderParkingDialog;
        private Autofac.IContainer _Container => this.GetLoaderContainer();
        public ILoaderSupervisor LoaderMaster => _Container.Resolve<ILoaderSupervisor>();

        private string _LoaderParkingMessage;
        public string LoaderParkingMessage
        {
            get { return _LoaderParkingMessage; }
            set
            {
                if (value != _LoaderParkingMessage)
                {
                    _LoaderParkingMessage = value;
                    RaisePropertyChanged();
                }
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
                        LoaderParkingDialog = new LoaderParkingDisplayDialog();
                        LoaderParkingDialog.DataContext = this;
                    });
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");
                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }

                Initialized = true;

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public EventCodeEnum InitModule(Autofac.IContainer container)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    LoaderParkingDialog = new LoaderParkingDisplayDialog();
                    LoaderParkingDialog.DataContext = this;
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
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task<bool> ShowDialog(string message)
        {
            bool retVal = true;
            try
            {
               
                LoaderParkingMessage = message;
                await this.MetroDialogManager().ShowWindow(LoaderParkingDialog);
            }
            catch (Exception err)
            {
                LoggerManager.Error(err + "ShowDialog() : Error occured.");
            }

            return retVal;
        }
        public async Task CloseDialog()
        {
            try
            {
                //다이어 로그 닫을때 해야하는 동작
                this.LoaderMaster.Loader.GetLoaderCommands().UnlockRobot();
                await this.MetroDialogManager().CloseWindow(LoaderParkingDialog);
            }
            catch (Exception err)
            {
                LoggerManager.Error(err + "CloseDialog() : Error occured.");
            }
        }

        private AsyncCommand _OKCommand;
        public IAsyncCommand OKCommand
        {
            get
            {
                if (null == _OKCommand) _OKCommand
                        = new AsyncCommand(OKCommandFunc);
                return _OKCommand;
            }
        }

        private async Task OKCommandFunc()
        {
            try
            {

                await CloseDialog();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
