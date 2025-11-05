using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TempDisplayDialogServiceProvider
{
    using MahApps.Metro.Controls;
    using MahApps.Metro.Controls.Dialogs;
    using System.Windows;
    using System.ComponentModel;
    using System.Windows.Input;
    using ProberInterfaces.DialogControl;
    using System.Runtime.CompilerServices;
    using RelayCommandBase;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces.Temperature;
    using ProberInterfaces;
    using ProberInterfaces.Command;
    using ProberInterfaces.Command.Internal;

    public class TempDisplayDialogService : INotifyPropertyChanged, ITempDisplayDialogService
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName]string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public bool Initialized { get; set; } = false;

        private TempDisplayDialog TempDisplayDialog;

        public ITempController TempController { get; private set; }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

                try
                {
                    if (Initialized == false)
                    {
                        TempDisplayDialog = new TempDisplayDialog();
                        TempDisplayDialog.DataContext = this;

                        TempController = this.TempController();

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
                    LoggerManager.Error(err + "InitModule() : Error occured");
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

        private bool IsNomalEnd = false;
        private bool _IsShowing = false;
        public bool IsShowing
        {
            get { return _IsShowing; }
            set
            {
                _IsShowing = value;
            }
        }

        private bool IsDialogOpenPossibleFlag = false;
        public void TurnOnPossibleFlag()
        {
            IsDialogOpenPossibleFlag = true;
        }

        public async Task<bool> ShowDialog()
        {
            try
            {
                if (!IsShowing)
                {
                    IsNomalEnd = true;

                    if (IsDialogOpenPossibleFlag)
                    {
                        IsShowing = true;
                        await this.MetroDialogManager().ShowWindow(TempDisplayDialog);
                    }
                    IsDialogOpenPossibleFlag = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err + "ShowDialog() : Error occured.");
            }

            return IsNomalEnd;
        }

        private AsyncCommand _cmdCancelButtonClick;
        public ICommand cmdCancelButtonClick
        {
            get
            {
                if (null == _cmdCancelButtonClick) _cmdCancelButtonClick
                        = new AsyncCommand(CancelButtonClick);
                return _cmdCancelButtonClick;
            }
        }

        private async Task CancelButtonClick()
        {
            try
            {
                IsNomalEnd = false;
                await CloseDialog();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public async Task CloseDialog()
        {
            try
            {
                 System.Threading.Thread.Sleep(1000);
                IsDialogOpenPossibleFlag = false;
                //if (IsShowing)
                {
                    ICommandManager CommandManager = TempController.CommandManager();
                    CommandManager.SetCommand<IReturnToDefaltSetTemp>(TempController);
                    await this.MetroDialogManager().CloseWindow(TempDisplayDialog);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err + "CloseDialog() : Error occured.");
            }
            IsShowing = false;
        }
    }
}
