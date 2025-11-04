using LogModule;
using MahApps.Metro.Controls.Dialogs;
using ProbeCardObject;
using ProberInterfaces;
using RelayCommandBase;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using WaferSelectSetup;

namespace PinAlignIntervalSettingViewProject.UC
{
    public class WaferIntervalSettingUIViewModel : WaferSelectSetupBase, IFactoryModule
    {
        private Guid _ViewModelGUID = new Guid("F641A3FC-10C7-FA06-3416-3D4079D36C1D");

        public Guid ViewModelGUID { get { return _ViewModelGUID; } }

        private WaferIntervalSettingUI DialogControl;

        private ObservableCollection<ButtonDescriptor> WaferSlots = new ObservableCollection<ButtonDescriptor>();

        private PinAlignDevParameters PinAlignParam => (this.PinAligner()?.PinAlignDevParam as PinAlignDevParameters);

        public WaferIntervalSettingUIViewModel()
        {
            try
            {
                for (int i = 0; i < 25; i++)
                {
                    WaferSlots.Add(new ButtonDescriptor());
                }

                WaferSelectBtn = new ObservableCollection<ButtonDescriptor>(WaferSlots);

                for (int i = 0; i < 25; i++)
                {
                    WaferSelectBtn[i].Command = new RelayCommand<Object>(SelectWaferIndexCommand);
                    WaferSelectBtn[i].CommandParameter = i;

                    if (PinAlignParam != null && PinAlignParam.PinAlignInterval != null)
                        WaferSelectBtn[i].isChecked = PinAlignParam.PinAlignInterval.WaferInterval[i].Value;
                }

                SelectAllBtn.Command = new RelayCommand<Object>(SetAllWaferUsingCommand);
                SelectAllBtn.CommandParameter = true;

                ClearAllBtn.Command = new RelayCommand<Object>(SetAllWaferUsingCommand);
                ClearAllBtn.CommandParameter = false;

                DialogControl = new WaferIntervalSettingUI();
                DialogControl.DataContext = this;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SelectWaferIndexCommand(Object index)
        {
            try
            {
                PinAlignParam.PinAlignInterval.WaferInterval[(int)index].Value = !(bool)PinAlignParam.PinAlignInterval.WaferInterval[(int)index].GetValue();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[WaferPMIMapSetupControlService] [SelectWaferIndexCommand()] : {err}");
            }
        }

        public void SetAllWaferUsingCommand(Object Using)
        {
            try
            {
                bool flag = (bool)Using;

                for (int i = 0; i < 25; i++)
                {
                    PinAlignParam.PinAlignInterval.WaferInterval[i].SetValue(flag);
                    WaferSelectBtn[i].isChecked = flag;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[WaferPMIMapSetupControlService] [SetAllWaferUsingCommand()] : {err}");
            }
        }


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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task ExitDialogCommand()
        {
            try
            {
                // Close current pop-up Window
                this.PinAligner().SavePinAlignDevParam();
                await HiddenDialogControl();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task ShowDialogControl()
        {
            try
            {
                await this.MetroDialogManager().ShowWindow(DialogControl);
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[WaferPMIMapSetupControlService] [ShowDialogControl()] : {err}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task HiddenDialogControl()
        {
            try
            {
                await this.MetroDialogManager().CloseWindow(DialogControl);
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[WaferPMIMapSetupControlService] [HiddenDialogControl()] : {err}");
            }
        }
        private RelayCommand<CUI.Button> _WaferIntervalSetupCommand;
        public ICommand WaferIntervalSetupCommand
        {
            get
            {
                if (null == _WaferIntervalSetupCommand) _WaferIntervalSetupCommand = new RelayCommand<CUI.Button>(FuncWaferIntervalSetup);
                return _WaferIntervalSetupCommand;
            }
        }

        private void FuncWaferIntervalSetup(CUI.Button cuiparam)
        {
            DialogControl.ShowModalDialogExternally();
        }
    }
}
