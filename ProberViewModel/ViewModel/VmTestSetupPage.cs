using System;
using System.Threading.Tasks;

namespace TestSetupDialog
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using RelayCommandBase;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Input;
    using TestSetupDialog.Tab.Pin;
    using TestSetupDialog.Tab.NC;
    using TestSetupDialog.Tab.GPCC;

    public class VmTestSetupPage : IMainScreenViewModel, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
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
            try
            {
                PinTabViewModel.Dispose();
                GPCCViewModel.Dispose();
                _Win.Close();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> TopBarMouseDownCommand
        private RelayCommand _TopBarMouseDownCommand;
        public ICommand TopBarMouseDownCommand
        {
            get
            {
                if (null == _TopBarMouseDownCommand) _TopBarMouseDownCommand = new RelayCommand(MouseDownCommandFunc);
                return _TopBarMouseDownCommand;
            }
        }

        private void MouseDownCommandFunc()
        {
            _Win.DragMove();
        }
        #endregion

        #region ==> PinTabViewModel
        private VmPinTab _PinTabViewModel;
        public VmPinTab PinTabViewModel
        {
            get { return _PinTabViewModel; }
            set { _PinTabViewModel = value; }
        }
        #endregion

        #region ==> NCTabViewModel
        private VmNCTab _NCTabViewModel;
        public VmNCTab NCTabViewModel
        {
            get { return _NCTabViewModel; }
            set { _NCTabViewModel = value; }
        }
        #endregion

        #region ==> GPCCViewModel
        private VmGPCardChangeSetting _GPCCViewModel;
        public VmGPCardChangeSetting GPCCViewModel
        {
            get { return _GPCCViewModel; }
            set { _GPCCViewModel = value; }
        }
        #endregion

        #region ==> GPObservaSetting
        private VmGPCardChangeObservation _GPObservaSetting;
        public VmGPCardChangeObservation GPObservaSetting
        {
            get { return _GPObservaSetting; }
            set { _GPObservaSetting = value; }
        }
        #endregion

        private Window _Win;

        public void SetWindow(Window win)
        {
            _Win = win;
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    retval = EventCodeEnum.NONE;
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

        }
        public Task<EventCodeEnum> InitViewModel()
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                _PinTabViewModel = new VmPinTab();
                _NCTabViewModel = new VmNCTab();
                _GPCCViewModel = new VmGPCardChangeSetting();
                _GPObservaSetting = new VmGPCardChangeObservation();

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"DeInitViewModel() in {GetType().Name}");

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }

        readonly Guid _ViewModelGUID = new Guid("AECF25CC-42BA-4D30-B94C-50EA23B3934E");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        public bool Initialized { get; set; } = false;
        private int _SelectedIndex;
        public int SelectedIndex
        {
            get { return _SelectedIndex; }
            set
            {
                if (value != _SelectedIndex)
                {
                    _SelectedIndex = value;
                    RaisePropertyChanged();
                    if (SelectedIndex == 2 || SelectedIndex == 3)
                    {
                        if (this.StageSupervisor().StageModuleState.GetState() != StageStateEnum.CARDCHANGE)
                        {
                            LoggerManager.Debug($"[CardChange IO Controller UI] StageMoveState: {this.StageSupervisor().StageModuleState.GetState()} , Execute Z Cleared ");
                            this.StageSupervisor().StageModuleState.ZCLEARED();
                        }
                        else
                        {
                            LoggerManager.Debug($"[CardChange IO Controller UI] StageMoveState: {this.StageSupervisor().StageModuleState.GetState()} , Not Execute Z Cleared ");
                        }
                    }
                }
            }
        }
    }
}
