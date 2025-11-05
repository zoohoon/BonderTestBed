using System;
using System.Threading.Tasks;

namespace ProbingSequenceSubViewModel
{
    using CUIServices;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using RelayCommandBase;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    public class ProbingSequenceSubViewModel : IMainScreenViewModel
    {
        private readonly Guid _ViewModelGUID = new Guid("e4cfa765-19c2-4f08-bcc1-2ec80a2c90dd");
        public Guid ScreenGUID
        {
            get { return _ViewModelGUID; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public bool Initialized { get; set; } = false;
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
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

        //public EventCodeEnum RollBackParameter()
        //{
        //    return EventCodeEnum.NONE;
        //}

        //public bool HasParameterToSave()
        //{
        //    return true;
        //}

        #region //..Command 
        private RelayCommand<CUI.Button> _ProbingSequenceViewCommand;
        public ICommand ProbingSequenceViewCommand
        {
            get
            {
                if (null == _ProbingSequenceViewCommand) _ProbingSequenceViewCommand = new RelayCommand<CUI.Button>(FuncProbingSequenceViewCommand);
                return _ProbingSequenceViewCommand;
            }
        }

        private void FuncProbingSequenceViewCommand(CUI.Button cuiparam)
        {
            try
            {

                Guid ViewGUID = CUIService.GetTargetViewGUID(cuiparam.GUID);

                this.ViewModelManager().ViewTransitionAsync(ViewGUID);

                //if (System.AppDomain.CurrentDomain.FriendlyName == "ProberSystem.exe")
                //{
                //    this.ViewModelManager().ViewTransitionAsync(ViewGUID);

                //}
                //else if (System.AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")
                //{
                //    this.ViewModelManager().ViewTransitionAsync(ViewGUID);
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

    }
}
