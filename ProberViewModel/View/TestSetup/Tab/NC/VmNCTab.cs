using System;

namespace TestSetupDialog.Tab.NC
{
    using NeedleCleanerModuleParameter;
    using ProberInterfaces;
    using RelayCommandBase;
    using SubstrateObjects;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    public class VmNCTab : INotifyPropertyChanged, IFactoryModule, IDisposable
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private bool _NCFlagHoldCleaningUpState;
        public bool NCFlagHoldCleaningUpState
        {
            get { return _NCFlagHoldCleaningUpState; }
            set
            {
                if (value != _NCFlagHoldCleaningUpState)
                {
                    _NCFlagHoldCleaningUpState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _NCEngrOverdrive;
        public double NCEngrOverdrive
        {
            get { return _NCEngrOverdrive; }
            set
            {
                if (value != _NCEngrOverdrive)
                {
                    _NCEngrOverdrive = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _NCEngrCleaningSpeed;
        public double NCEngrCleaningSpeed
        {
            get { return _NCEngrCleaningSpeed; }
            set
            {
                if (value != _NCEngrCleaningSpeed)
                {
                    _NCEngrCleaningSpeed = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _NCEngrCleaningAccel;
        public double NCEngrCleaningAccel
        {
            get { return _NCEngrCleaningAccel; }
            set
            {
                if (value != _NCEngrCleaningAccel)
                {
                    _NCEngrCleaningAccel = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _NCCleaningOverdriveOffset;
        public double NCCleaningOverdriveOffset
        {
            get { return _NCCleaningOverdriveOffset; }
            set
            {
                if (value != _NCCleaningOverdriveOffset)
                {
                    _NCCleaningOverdriveOffset = value;
                    RaisePropertyChanged();
                }
            }
        }


        private NeedleCleanSystemParameter _SystemParam;
        public NeedleCleanSystemParameter SystemParam
        {
            get { return (NeedleCleanSystemParameter)this.StageSupervisor().NCObject.NCSysParam_IParam; }
            set
            {
                if (value != _SystemParam)
                {
                    _SystemParam = value;
                    RaisePropertyChanged();
                }
            }
        }
        public NeedleCleanObject NC
        {
            get { return this.StageSupervisor().NCObject as NeedleCleanObject; }
        }
        public VmNCTab()
        {
            SystemParam = this.StageSupervisor().NCObject.NCSysParam_IParam as NeedleCleanSystemParameter;
            if(NC.NCSheetVMDefs.Count !=0)
            {
                NCFlagHoldCleaningUpState = NC.NCSheetVMDefs[0].FlagHoldCleaningUpState;
                NCEngrOverdrive = NC.NCSheetVMDefs[0].EngrOverdrive;

                NC.NCSheetVMDefs[0].EngrCleaningSpeed = 30000;
                NC.NCSheetVMDefs[0].EngrCleaningAccel = 300000;

                NCEngrCleaningSpeed = NC.NCSheetVMDefs[0].EngrCleaningSpeed;
                NCEngrCleaningAccel = NC.NCSheetVMDefs[0].EngrCleaningAccel;
            }
            NCCleaningOverdriveOffset = SystemParam.CleaningOverdriveOffset.Value;

        }

        #region ==> ExitCommand
        private RelayCommand _ApplyCommand;
        public ICommand ApplyCommand
        {
            get
            {
                if (null == _ApplyCommand) _ApplyCommand = new RelayCommand(ApplyCommandFunc);
                return _ApplyCommand;
            }
        }
        private void ApplyCommandFunc()
        {
            SystemParam.CleaningOverdriveOffset.Value = NCCleaningOverdriveOffset;
            this.NeedleCleaner().SaveSysParameter();
            foreach (var ncshhetvmedf in NC.NCSheetVMDefs)
            {
                ncshhetvmedf.FlagHoldCleaningUpState = NCFlagHoldCleaningUpState;
                ncshhetvmedf.EngrOverdrive = NCEngrOverdrive;
                ncshhetvmedf.EngrCleaningSpeed = NCEngrCleaningSpeed;
                ncshhetvmedf.EngrCleaningAccel = NCEngrCleaningAccel;
            }

        }
        #endregion

        public void Dispose()
        {

        }
    }
}
