using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighStandard
{
    using ProberInterfaces;
    using ProberInterfaces.Align;
    using ProberInterfaces.AlignEX;
    using ProberInterfaces.Enum;
    using System.ComponentModel;
    using Autofac;
    using NLog;
    using System.Windows.Input;
    using RelayCommandBase;
    using ProberInterfaces.WaferAlign;

    public enum WAHighSetupFunction
    {
        UNDIFINE = -1,
        ROISIZECHANGEDPT,
        REGPATTERN,
        DELETEPATTERN,
        MODIFYHEGIHTPRO,
        MOVEFORPATTERN,
        SAVE,
        UPDATEINFO,
        PREVSTEP
    }

    public class HighStandardSetupModule : URectAlignSetupModuleBase, INotifyPropertyChanged
    {
        private static Logger Log = NLog.LogManager.GetCurrentClassLogger();
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public override int InitPriority { get; set; }

        private IWaferAligner WaferAligner;
        private IStageSupervisor StageSupervisor;
        private ICamera cam;

        private WAHighSetupFunction ModifyCondition;
        private int _ChangeWidthValue;
        private int _ChangeHeightValue;

        private AlignModuleSetupStateBase _SetupState;
        public override AlignModuleSetupStateBase SetupState
        {
            get { return _SetupState; }
            set
            {
                if (value != _SetupState)
                {
                    _SetupState = value;
                    NotifyPropertyChanged("SetupState");
                }
            }
        }
        public override ErrorCodeEnum InitModule(Autofac.IContainer container)
        {
            ErrorCodeEnum RetVal = ErrorCodeEnum.UNDEFINED;
            try
            {
            SetupState = new HighSetupIdleState(this);

            WaferAligner = container.Resolve<IWaferAligner>();
            StageSupervisor = container.Resolve<IStageSupervisor>();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return RetVal;
        }
        public override ErrorCodeEnum LoadingModule()
        {
            ErrorCodeEnum retVal = ErrorCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }
        public override ErrorCodeEnum Run()
        {
            ErrorCodeEnum RetVal = ErrorCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return RetVal;
        }

        public override ErrorCodeEnum ReSet()
        {
            ErrorCodeEnum RetVal = ErrorCodeEnum.UNDEFINED;
            try
            {

            SetupState.Reset();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return RetVal;
        }

        private RelayCommand<object> _OpenFlyout;
        public ICommand OpenFlyout
        {
            get
            {
                if (null == _OpenFlyout) _OpenFlyout = new RelayCommand<object>(FuncOpenFlyout);
                return _OpenFlyout;
            }
        }

        public void FuncOpenFlyout(object noparam)
        {
            //FlyoutIsOpen = true;
        }

        private RelayCommand<object> _ExtendPatternHeightCommand;
        public ICommand ExtendPatternHeightCommand
        {
            get
            {
                if (null == _ExtendPatternHeightCommand) _ExtendPatternHeightCommand = new RelayCommand<object>(ExtendPatternHeight);
                return _ExtendPatternHeightCommand;
            }
        }
        private void ExtendPatternHeight(object parameter)
        {
            PTSizeYPluse();
        }

        private RelayCommand<object> _ExtendPatternWidthCommand;
        public ICommand ExtendPatternWidthCommand
        {
            get
            {
                if (null == _ExtendPatternWidthCommand) _ExtendPatternWidthCommand = new RelayCommand<object>(ExtendPatternWidth);
                return _ExtendPatternWidthCommand;
            }
        }

        private void ExtendPatternWidth(object parameter)
        {
            PTSizeXPluse();
        }

        private RelayCommand<object> _ReducePatternWidthCommand;
        public ICommand ReducePatternWidthCommand
        {
            get
            {
                if (null == _ReducePatternWidthCommand) _ReducePatternWidthCommand = new RelayCommand<object>(ReducePatternWidth);
                return _ReducePatternWidthCommand;
            }
        }
        private void ReducePatternWidth(object parameter)
        {
            PTSizeXMinus();
        }

        private RelayCommand<object> _ReducePatternHeightCommand;
        public ICommand ReducePatternHeightCommand
        {
            get
            {
                if (null == _ReducePatternHeightCommand) _ReducePatternHeightCommand = new RelayCommand<object>(ReducePatternHeight);
                return _ReducePatternHeightCommand;
            }
        }
        private void ReducePatternHeight(object parameter)
        {
            PTSizeYMinus();
        }

        private RelayCommand<object> _PrevStepCommand;
        public ICommand PrevStepCommand
        {
            get
            {
                if (null == _PrevStepCommand) _PrevStepCommand = new RelayCommand<object>(PrevStep);
                return _PrevStepCommand;
            }
        }
        private void PrevStep(object parameter)
        {
            try
            {
            ModifyCondition = WAHighSetupFunction.PREVSTEP;
            SetupState.Modify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        private RelayCommand<object> _AddRemovePatternCommand;
        public ICommand AddRemovePatternCommand
        {
            get
            {
                if (null == _AddRemovePatternCommand) _AddRemovePatternCommand = new RelayCommand<object>(AddHighPattern);
                return _AddRemovePatternCommand;
            }
        }
        private void AddHighPattern(object parameter)
        {
            try
            {
            ModifyCondition = WAHighSetupFunction.REGPATTERN;
            SetupState.Modify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        private AsyncCommand _NextStepCommand;
        public ICommand NextStepCommand
        {
            get
            {
                if (null == _NextStepCommand) _NextStepCommand = new AsyncCommand(StepNext);
                return _NextStepCommand;
            }
        }

        private async Task<ErrorCodeEnum> StepNext()
        {
            ErrorCodeEnum retval = ErrorCodeEnum.NONE;
            try
            {
            Task<ErrorCodeEnum> stateTask;
            stateTask = Task.Run(() => SetupState.Run());
            await stateTask;
            retval = stateTask.Result;

            if (retval == ErrorCodeEnum.NONE)
            {
      //          WaferAligner.SaveWaferAveThick();
      //          HiddenPatternRect();
      //          WaferAligner.WaferAlignSetupProcedure.Setup =
      //WaferAligner.WaferAlignSetupProcedure.SetupList
      //[++WaferAligner.WaferAlignSetupProcedure.CurSetupIndex];
      //          WaferAligner.WaferAlignSetupProcedure.Setup.InitSetup();
      //          SaveDataProcedureFile();
            }

            //retval = Resetting();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retval;
        }

        private void PTSizeXPluse()
        {
            try
            {
            ModifyCondition = WAHighSetupFunction.ROISIZECHANGEDPT;
            _ChangeWidthValue = 12;
            _ChangeHeightValue = 0;
            SetupState.Modify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        private void PTSizeXMinus()
        {
            try
            {
            ModifyCondition = WAHighSetupFunction.ROISIZECHANGEDPT;
            _ChangeWidthValue = -12;
            _ChangeHeightValue = 0;
            SetupState.Modify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        private void PTSizeYPluse()
        {
            try
            {
            ModifyCondition = WAHighSetupFunction.ROISIZECHANGEDPT;
            _ChangeWidthValue = 0;
            _ChangeHeightValue = 12;
            SetupState.Modify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        private void PTSizeYMinus()
        {
            try
            {
            ModifyCondition = WAHighSetupFunction.ROISIZECHANGEDPT;
            _ChangeWidthValue = 0;
            _ChangeHeightValue = -12;
            SetupState.Modify();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
    }

    public class HighSetupNoDataState : AlignModuleSetupNoDataState
    {
        public HighSetupNoDataState(IAlignSetupModule module) : base(module)
        {
        }

        public override ErrorCodeEnum Modify()
        {
            throw new NotImplementedException();
        }

        public override ErrorCodeEnum Reset()
        {
            throw new NotImplementedException();
        }

        public override ErrorCodeEnum Run()
        {
            throw new NotImplementedException();
        }
    }

    public class HighSetupIdleState : AlignModuleSetupIdleState
    {
        public HighSetupIdleState(IAlignSetupModule module) : base(module)
        {
        }

        public override ErrorCodeEnum Modify()
        {
            ErrorCodeEnum ret = ErrorCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return ret;
        }

        public override ErrorCodeEnum Reset()
        {
            ErrorCodeEnum RetVal = ErrorCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return RetVal;
        }

        public override ErrorCodeEnum Run()
        {
            ErrorCodeEnum ret = ErrorCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return ret;
        }

    }
    public class HighSetupModifyState : AlignModuleSetupModifyState
    {
        public HighSetupModifyState(IAlignSetupModule module) : base(module)
        {
        }

        public override ErrorCodeEnum Modify()
        {
            throw new NotImplementedException();
        }

        public override ErrorCodeEnum Reset()
        {
            throw new NotImplementedException();
        }

        public override ErrorCodeEnum Run()
        {
            throw new NotImplementedException();
        }
    }
    public class HighSetupDoneState : AlignModuleSetupDoneState
    {
        public HighSetupDoneState(IAlignSetupModule module) : base(module)
        {
        }

        public override ErrorCodeEnum Modify()
        {
            ErrorCodeEnum ret = ErrorCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return ret;
        }

        public override ErrorCodeEnum Reset()
        {
            throw new NotImplementedException();
        }

        public override ErrorCodeEnum Run()
        {
            ErrorCodeEnum ret = ErrorCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return ret;
        }
    }
    public class HighSetupErrorState : AlignModuleSetupErrorState
    {
        public HighSetupErrorState(IAlignSetupModule module) : base(module)
        {
        }

        public override ErrorCodeEnum Modify()
        {
            ErrorCodeEnum ret = ErrorCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return ret;
        }

        public override ErrorCodeEnum Reset()
        {
            throw new NotImplementedException();
        }

        public override ErrorCodeEnum Run()
        {
            ErrorCodeEnum ret = ErrorCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return ret;
        }
    }
}
