using LogModule;
using ProberInterfaces;
using RelayCommandBase;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using UcDisplayPort;

namespace ComponentVerificationDialog
{
    class ComponentVerificationDialogViewModel : INotifyPropertyChanged, IFactoryModule
    {
        enum StageCamEnum
        {
            WAFER_HIGH_CAM,
            WAFER_LOW_CAM,
            PIN_HIGH_CAM,
            PIN_LOW_CAM,
        }

        public ComponentVerificationDialogViewModel()
        {
            model = new ComponentVerificationDialogModel();

            DisplayPort = new DisplayPort() { GUID = new Guid("EB926257-B367-4ACC-83D9-D3B629757BE0") };

            Array stagecamvalues = Enum.GetValues(typeof(StageCamEnum));
            foreach (var cam in this.VisionManager().GetCameras())
            {
                for (int index = 0; index < stagecamvalues.Length; index++)
                {
                    if (((StageCamEnum)stagecamvalues.GetValue(index)).ToString() == cam.GetChannelType().ToString())
                    {
                        this.VisionManager().SetDisplayChannel(cam, DisplayPort);
                        this.VisionManager().StartGrab(cam.GetChannelType(), this);

                        break;
                    }
                }
            }

            #region ==> Binding DisplayPort
            ((UcDisplayPort.DisplayPort)DisplayPort).DataContext = this;

            Binding bindX = new Binding
            {
                Path = new System.Windows.PropertyPath("StageSupervisor.MoveTargetPosX"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.MoveToX, bindX);

            Binding bindY = new Binding
            {
                Path = new System.Windows.PropertyPath("StageSupervisor.MoveTargetPosY"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.MoveToY, bindY);


            Binding bindCamera = new Binding
            {
                Path = new System.Windows.PropertyPath("Cam"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.AssignedCamearaProperty, bindCamera);
            #endregion
        }

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        #region ==> Property
        private ComponentVerificationDialogModel model = null;
        public ComponentVerificationDialogModel Model
        {
            get { return model; }
            set
            {
                if (model != value)
                {
                    model = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IDisplayPort _DisplayPort;
        public IDisplayPort DisplayPort
        {
            get { return _DisplayPort; }
            set
            {
                if (value != _DisplayPort)
                {
                    _DisplayPort = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ICamera _RefCam;
        public ICamera RefCam
        {
            get { return _RefCam; }
            set
            {
                if (_RefCam != value)
                {
                    if (_RefCam != null)
                    {
                        this.VisionManager()?.StopGrab(_RefCam.GetChannelType());
                    }

                    _RefCam = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> Command 
        private RelayCommand _AddButtonCommand;
        public ICommand AddButtonCommand
        {
            get
            {
                if (null == _AddButtonCommand)
                {
                    _AddButtonCommand = new RelayCommand(AddCommand);
                }
                return _AddButtonCommand;
            }
        }
        private void AddCommand()
        {
            try
            {
                Model.AddCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _DelButtonCommand;
        public ICommand DelButtonCommand
        {
            get
            {
                if (null == _DelButtonCommand)
                {
                    _DelButtonCommand = new RelayCommand(DeleteCommand);
                }
                
                return _DelButtonCommand;
            }
        }
        private void DeleteCommand()
        {
            try
            {
                Model.DeleteCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _ScenarioUpCommand;
        public ICommand ScenarioUpCommand
        {
            get
            {
                if (null == _ScenarioUpCommand)
                {
                    _ScenarioUpCommand = new RelayCommand(ScenarioUp);
                }

                return _ScenarioUpCommand;
            }
        }
        private void ScenarioUp()
        {
            try
            {
                Model.ScenarioUp();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _ScenarioDownCommand;
        public ICommand ScenarioDownCommand
        {
            get
            {
                if (null == _ScenarioDownCommand)
                {
                    _ScenarioDownCommand = new RelayCommand(ScenarioDown);
                }

                return _ScenarioDownCommand;
            }
        }
        private void ScenarioDown()
        {
            try
            {
                Model.ScenarioDown();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _ScenarioRunCommand;
        public ICommand ScenarioRunCommand
        {
            get
            {
                if (null == _ScenarioRunCommand) _ScenarioRunCommand = new AsyncCommand(ScenarioRun);
                return _ScenarioRunCommand;
            }
        }
        private async Task ScenarioRun()
        {
            try
            {
                await Task.Run(() => Model.ScenarioRun());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _ScenarioStopCommand;
        public ICommand ScenarioStopCommand
        {
            get
            {
                if (null == _ScenarioStopCommand) _ScenarioStopCommand = new AsyncCommand(ScenarioStop);
                return _ScenarioStopCommand;
            }
        }
        private async Task ScenarioStop()
        {
            try
            {
                await Task.Run(() => Model.ScenarioStop());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _ParamItemSellectionChangedCommand;
        public ICommand ParamItemSellectionChangedCommand
        {
            get
            {
                if (null == _ParamItemSellectionChangedCommand)
                {
                    _ParamItemSellectionChangedCommand = new RelayCommand<object>(ParamItemSellectionChanged);
                }

                return _ParamItemSellectionChangedCommand;
            }
        }

        private void ParamItemSellectionChanged(object parameter)
        {
            try
            {
                var ParamName = parameter as string;
                Model.ParamItemSellectionChanged(ParamName);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _ApplyFilterCommand;
        public ICommand ApplyFilterCommand
        {
            get
            {
                if (null == _ApplyFilterCommand)
                {
                    _ApplyFilterCommand = new RelayCommand(ApplyFilter);
                }

                return _ApplyFilterCommand;
            }
        }

        private void ApplyFilter()
        {
            try
            {
                Model.ApplyLogFilter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _LogExportCommand;
        public ICommand LogExportCommand
        {
            get
            {
                if (null == _LogExportCommand)
                {
                    _LogExportCommand = new RelayCommand(LogExport);
                }

                return _LogExportCommand;
            }
        }

        private void LogExport()
        {
            try
            {
                Model.ExportFilteredLog();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _ScenarioExportCommand;
        public ICommand ScenarioExportCommand
        {
            get
            {
                if (null == _ScenarioExportCommand) _ScenarioExportCommand = new AsyncCommand(ScenarioExport);
                return _ScenarioExportCommand;
            }
        }
        private async Task ScenarioExport()
        {
            try
            {
                await Task.Run(() => Model.ScenarioExport());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _ScenarioImportCommand;
        public ICommand ScenarioImportCommand
        {
            get
            {
                if (null == _ScenarioImportCommand) _ScenarioImportCommand = new AsyncCommand(ScenarioImport);
                return _ScenarioImportCommand;
            }
        }
        private async Task ScenarioImport()
        {
            try
            {
                await Task.Run(() => Model.ScenarioImport());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _ScenarioClearCommand;
        public ICommand ScenarioClearCommand
        {
            get
            {
                if (null == _ScenarioClearCommand) _ScenarioClearCommand = new RelayCommand(ScenarioClear);
                return _ScenarioClearCommand;
            }
        }
        private void ScenarioClear()
        {
            try
            {
                Model.ScenarioClear();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _LogClearCommand;
        public ICommand LogClearCommand
        {
            get
            {
                if (null == _LogClearCommand) _LogClearCommand = new RelayCommand(LogClear);
                return _LogClearCommand;
            }
        }
        private void LogClear()
        {
            try
            {
                Model.LogClear();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        /// <summary>
        /// Component Verification Dialog를 Open/Close 할 때 상태를 Update 하기 위한 함수
        /// </summary>
        /// <param name="IsDialogOpen"></param>
        public void UpdateDialogOpenCloseState(bool IsDialogOpen)
        {
            try
            {
                if (IsDialogOpen)
                {
                    // Component Verification Dialog가 Open될 때 ProberSystem 화면 잠금 및 Setup State로 변경
                    this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");
                    this.SysState().SetSetUpState();
                    LoggerManager.Debug("[Component Verification] Dialog is opened. ProberSystem is locked.");
                }
                else
                {
                    // Component Verification Dialog가 Close될 때 ProberSystem 화면 잠금 해제 및 Idle State로 변경
                    this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
                    this.SysState().SetIdleState();
                    LoggerManager.Debug("[Component Verification] Dialog is closed. ProberSystem is unlocked.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
