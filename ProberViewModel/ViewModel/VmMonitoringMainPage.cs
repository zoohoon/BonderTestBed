using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonitoringMainPageViewModel
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using LogModule;
    using MonitoringModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using RelayCommandBase;

    public class VmMonitoringMainPage : IMainScreenViewModel
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

        #region ==> UpCommand
        private AsyncCommand _UpCommand;
        public ICommand UpCommand
        {
            get
            {
                if (null == _UpCommand) _UpCommand = new AsyncCommand(UpCommandFunc);
                return _UpCommand;
            }
        }
        private async Task UpCommandFunc()
        {
            try
            {
                //this.StageSupervisor().StageModuleState.StageVMove(_YAxis, _MotionVMoveDistance, EnumTrjType.Normal);

                await Task.Run(() =>
                {
                    this.MotionManager().StageMove(0, _YAxis.Param.NegSWLimit.Value, _ZAxis.Param.HomeOffset.Value, ovrd: 0.1);
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> LeftCommand
        private RelayCommand _LeftCommand;
        public ICommand LeftCommand
        {
            get
            {
                if (null == _LeftCommand) _LeftCommand = new RelayCommand(LeftCommandFunc);
                return _LeftCommand;
            }
        }
        private void LeftCommandFunc()
        {
            //this.StageSupervisor().StageModuleState.StageVMove(_XAxis, _MotionVMoveDistance * -1, EnumTrjType.Normal);
        }
        #endregion

        #region ==> StopCommand
        private AsyncCommand _StopCommand;
        public ICommand StopCommand
        {
            get
            {
                if (null == _StopCommand) _StopCommand = new AsyncCommand(StopCommandFunc);
                return _StopCommand;
            }
        }
        private async Task StopCommandFunc()
        {
            try
            {
                //this.StageSupervisor().StageModuleState.StageMoveStop(_XAxis);
                //this.StageSupervisor().StageModuleState.StageMoveStop(_YAxis);
                //this.StageSupervisor().StageModuleState.StageMoveStop(_ZAxis);

                await Task.Run(() =>
                {
                    this.MotionManager().StageMove(0, 0, _ZAxis.Param.HomeOffset.Value);
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> RightCommand
        private RelayCommand _RightCommand;
        public ICommand RightCommand
        {
            get
            {
                if (null == _RightCommand) _RightCommand = new RelayCommand(RightCommandFunc);
                return _RightCommand;
            }
        }
        private void RightCommandFunc()
        {
            //this.StageSupervisor().StageModuleState.StageVMove(_XAxis, _MotionVMoveDistance, EnumTrjType.Normal);
        }
        #endregion

        #region ==> DownCommand
        private AsyncCommand _DownCommand;
        public ICommand DownCommand
        {
            get
            {
                if (null == _DownCommand) _DownCommand = new AsyncCommand(DownCommandFunc);
                return _DownCommand;
            }
        }
        private async Task DownCommandFunc()
        {
            //this.StageSupervisor().StageModuleState.StageVMove(_YAxis, _MotionVMoveDistance * -1, EnumTrjType.Normal);

            try
            {
                await Task.Run(() =>
                {
                    this.MotionManager().StageMove(0, _YAxis.Param.PosSWLimit.Value, _ZAxis.Param.HomeOffset.Value, ovrd: 0.1);
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> LockCommand
        private RelayCommand _LockCommand;
        public ICommand LockCommand
        {
            get
            {
                if (null == _LockCommand) _LockCommand = new RelayCommand(LockCommandFunc);
                return _LockCommand;
            }
        }
        private void LockCommandFunc()
        {
            try
            {
                List<EnumAxisConstants> axisList = new List<EnumAxisConstants>();
                axisList.Add(EnumAxisConstants.X);
                axisList.Add(EnumAxisConstants.Y);
                axisList.Add(EnumAxisConstants.Z);
                //this.MonitoringManager().SetLock(axisList);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> EmergencyCommand
        private RelayCommand _EmergencyCommand;
        public ICommand EmergencyCommand
        {
            get
            {
                if (null == _EmergencyCommand) _EmergencyCommand = new RelayCommand(EmergencyCommandFunc);
                return _EmergencyCommand;
            }
        }
        private void EmergencyCommandFunc()
        {
            try
            {
                List<EnumAxisConstants> axisList = new List<EnumAxisConstants>();
                axisList.Add(EnumAxisConstants.X);
                axisList.Add(EnumAxisConstants.Y);
                axisList.Add(EnumAxisConstants.Z);
                //this.MonitoringManager().SetEmergency(axisList);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> ResumeCommand
        private RelayCommand _ResumeCommand;
        public ICommand ResumeCommand
        {
            get
            {
                if (null == _ResumeCommand) _ResumeCommand = new RelayCommand(ResumeCommandFunc);
                return _ResumeCommand;
            }
        }
        private void ResumeCommandFunc()
        {
            try
            {
                List<EnumAxisConstants> axisList = new List<EnumAxisConstants>();
                axisList.Add(EnumAxisConstants.X);
                axisList.Add(EnumAxisConstants.Y);
                axisList.Add(EnumAxisConstants.Z);
                //this.MonitoringManager().SetUnlock(axisList);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        //private int _MotionVMoveDistance = 10;
        private ProbeAxisObject _XAxis;
        private ProbeAxisObject _YAxis;
        private ProbeAxisObject _ZAxis;

        private ObservableCollection<HWPartInfo> _HWCheckerInfoList = new ObservableCollection<HWPartInfo>();
        public ObservableCollection<HWPartInfo> HWCheckerInfoList
        {
            get { return _HWCheckerInfoList; }
            set { _HWCheckerInfoList = value; }
        }


        readonly Guid _ViewModelGUID = new Guid("5d9d5473-d933-4151-9d1b-93e001e0d589");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
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

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    if(this.MonitoringManager() != null)
                    {
                        List<HWPartChecker> hwPartCheckList = this.MonitoringManager().GetHWPartCheckList() as List<HWPartChecker>;
                        MonitoringSystemParameter monitoringParam = this.MonitoringManager().MonitoringSystemParam_IParam as MonitoringSystemParameter;

                        foreach (HWPartChecker hwPartCheck in hwPartCheckList)
                        {
                            _HWCheckerInfoList.Add(
                                new HWPartInfo(
                                    hwPartCheck.GetType().Name,
                                    monitoringParam.HWAxisCheckList[hwPartCheck.GetType().Name],
                                    hwPartCheck));
                        }
                    }
                    
                    if(this.MotionManager() != null)
                    {
                        _XAxis = this.MotionManager().GetAxis(EnumAxisConstants.X);
                        _YAxis = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                        _ZAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                    }
                    
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
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
    }
    public class HWPartInfo : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ==> HWPartName
        private String _HWPartName;
        public String HWPartName
        {
            get { return _HWPartName; }
            set
            {
                if (value != _HWPartName)
                {
                    _HWPartName = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> AxisDesc
        private String _AxisDesc;
        public String AxisDesc
        {
            get { return _AxisDesc; }
            set
            {
                if (value != _AxisDesc)
                {
                    _AxisDesc = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion


        public HWPartChecker PartChecker { get; set; }

        public HWPartInfo(String hwPartName, List<EnumAxisConstants> axisList, HWPartChecker partChecker)
        {
            try
            {
                HWPartName = hwPartName;

                _AxisDesc = String.Empty;
                foreach (EnumAxisConstants axisName in axisList)
                    _AxisDesc = _AxisDesc + " | " + axisName.ToString();

                PartChecker = partChecker;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

    }
}
