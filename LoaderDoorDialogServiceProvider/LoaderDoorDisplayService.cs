using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace LoaderDoorDialogServiceProvider
{
    using Autofac;
    using LoaderBase;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.DialogControl;
    using RelayCommandBase;
    using System.Windows;

    public class LoaderDoorDisplayService : INotifyPropertyChanged, ILoaderDoorDisplayDialogService, IFactoryModule
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName]string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        public InitPriorityEnum InitPriority => InitPriorityEnum.LEVEL3;
        public bool Initialized { get; set; } = false;
        private LoaderDoorDisplayDialog LoaderDoorDialog;
        private Autofac.IContainer _Container => this.GetLoaderContainer();
        public ILoaderSupervisor LoaderMaster => _Container.Resolve<ILoaderSupervisor>();

        private string _LoaderDoorMessage;
        public string LoaderDoorMessage
        {
            get { return _LoaderDoorMessage; }
            set
            {
                if (value != _LoaderDoorMessage)
                {
                    _LoaderDoorMessage = value;
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
                        LoaderDoorDialog = new LoaderDoorDisplayDialog();
                        LoaderDoorDialog.DataContext = this;
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
                    LoaderDoorDialog = new LoaderDoorDisplayDialog();
                    LoaderDoorDialog.DataContext = this;
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
                LoaderDoorMessage = message;
                await this.MetroDialogManager().ShowWindow(LoaderDoorDialog);
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
                bool left = false;
                bool right = false;
                this.GetGPLoader().LoaderBuzzer(false);
                var ret = LoaderMaster.GetLoaderDoorStatus(out left, out right);

                if (ret == EventCodeEnum.NONE &&
                    left == false &&
                    right == false)
                {
                    await this.MetroDialogManager().CloseWindow(LoaderDoorDialog);
                }
                else if(left)
                {
                    LoaderDoorMessage = "Loader Left Door Opened.";
                }
                else if(right)
                {
                    LoaderDoorMessage = "Loader Right Door Opened.";
                }
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
