using LogModule;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using PMIModuleParameter;
using PMIProcesser;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.DialogControl;
using ProberInterfaces.Enum;
using ProberInterfaces.PMI;
using ProberInterfaces.Vision;
using RelayCommandBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using VirtualKeyboardControl;

namespace PMIProcesser
{
    public class ManualPMISetupControlService : INotifyPropertyChanged, IFactoryModule
    {
        #region == > PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private ManualPMISetupControl DialogControl;
        private ManualPMIProcessModule ManualPMIProcessModule;
        //private PMIModuleDevParam ModuleParam;

        private IStateModule _PMIModule;
        public IStateModule PMIModule
        {
            get { return _PMIModule; }
            set
            {
                if (value != _PMIModule)
                {
                    _PMIModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ManualPMISetupControlService(ManualPMIProcessModule ManualProcessModule)
        {
            try
            {
            DialogControl = new ManualPMISetupControl();
            DialogControl.DataContext = this;

            PMIModule = this.PMIModule();

            ManualPMIProcessModule = ManualProcessModule;
            //ModuleParam = ManualPMIProcessModule.PMIDevParam;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }


        #region ==> Command
        private AsyncCommand _CmdExitClick;
        public ICommand CmdExitClick
        {
            get
            {
                if (null == _CmdExitClick) _CmdExitClick
                        = new AsyncCommand(ExitDialogCommand);
                return _CmdExitClick;
            }
        }

        private async Task ExitDialogCommand()
        {
            try
            {
                // Close current pop-up Window
                await HiddenDialogControl();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public async Task ShowDialogControl()
        {
            try
            {
                await this.MetroDialogManager().ShowWindow(DialogControl);
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[ManualPMISetupControlService] [ShowDialogControl()] : {err}");
            }
        }

        private async Task HiddenDialogControl()
        {
            try
            {
                await this.MetroDialogManager().CloseWindow(DialogControl);
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[ManualPMISetupControlService] [HiddenDialogControl()] : {err}");
            }
        }

        #endregion
    }
}
