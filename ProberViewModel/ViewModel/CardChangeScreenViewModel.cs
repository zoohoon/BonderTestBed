using ProberErrorCode;
using ProberInterfaces;
using RelayCommandBase;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Runtime.CompilerServices;
using ProberInterfaces.SequenceRunner;
using LogModule;
using System.Linq;
using System.Windows;
using ProberInterfaces.State;

namespace CardChangeScreenVM
{
    public class CardChangeScreenViewModel : IMainScreenViewModel, ISetUpState
    {
        private readonly Guid _ViewModelGUID = new Guid("91D47E22-66F7-C358-736C-5BCF7C3E287E");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        #region << PropertyChanged >>
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]String info = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        public CardChangeScreenViewModel()
        {
        }

        private AsyncObservableCollection<ISequenceBehaviorGroupItem> _SequenceCollection;
        public AsyncObservableCollection<ISequenceBehaviorGroupItem> SequenceCollection
        {
            get { return _SequenceCollection; }
            set
            {
                if (value != _SequenceCollection)
                {
                    _SequenceCollection = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AsyncObservableCollection<IOPortDescripter<bool>> _IOPorts;
        public AsyncObservableCollection<IOPortDescripter<bool>> IOPorts
        {
            get { return _IOPorts; }
            set
            {
                if (value != _IOPorts)
                {
                    _IOPorts = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IStage3DModel _Stage3DModel;
        public IStage3DModel Stage3DModel
        {
            get { return _Stage3DModel; }
            set
            {
                if (value != _Stage3DModel)
                {
                    _Stage3DModel = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ISequenceRunner SequenceRunnerModule { get; private set; }
        public IViewModelManager ViewModelManager { get; private set; }

        private int mViewNUM;
        private bool mIsItDisplayed2RateMagnification;


        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (this.Initialized == false)
                {
                    this.SequenceRunnerModule = this.SequenceRunner();
                    this.ViewModelManager = this.ViewModelManager();
  
                    ObservableCollection<ISequenceBehaviorGroupItem> Seqgroup = this.CardChangeModule()?.GetCcGroupCollection();

                    if(Seqgroup != null)
                    {
                        this.SequenceCollection = new AsyncObservableCollection<ISequenceBehaviorGroupItem>(Seqgroup);

                        foreach (var v in this.SequenceCollection)
                        {
                            v.InitModule();
                        }

                        this.IOPorts = new AsyncObservableCollection<IOPortDescripter<bool>>(this.SequenceRunnerModule.GetInputPorts(this.SequenceCollection?.ToList()));

                        retval = EventCodeEnum.NONE;
                    }
                    else
                    {
                        LoggerManager.Debug("[CardChangeScreenViewModel], InitModule() : Can not make SequenceCollection.");
                    }

                    this.Stage3DModel = this.ViewModelManager.Stage3DModel;

                    LoggerManager.Debug("CardChangeScreenViewModel InitModule : {0}");

                    Initialized = true;
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

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            try
            {
                LoggerManager.Debug($"DeInitViewModel() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        #region <<Command>>

        #region Change ProbeCardCommand Command
        private AsyncCommand _ChangeProbeCardCommand;

        public ICommand ChangeProbeCardCommand
        {
            get
            {
                if (null == _ChangeProbeCardCommand)
                    _ChangeProbeCardCommand = new AsyncCommand(ChangeProbeCard);
                return _ChangeProbeCardCommand;
            }
        }

        public async Task ChangeProbeCard()
        {
            //this.ViewModelManager().Lock(this.GetHashCode(), "Wait", "Changing ProbeCard");
            

            try
            {
                SequenceRunnerModule.RunCardChange(true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                
            }
        }
        #endregion

        #region CCRetry Command
        private AsyncCommand _CCRetryCommand;

        public ICommand CCRetryCommand
        {
            get
            {
                if (null == _CCRetryCommand) _CCRetryCommand = new AsyncCommand(CCRetry);
                return _CCRetryCommand;
            }
        }

        private async Task CCRetry()
        {
            //this.ViewModelManager().Lock(this.GetHashCode(), "Wait", "CCRetry");
            

            try
            {
                SequenceRunnerModule.RunRetry();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                
            }
        }
        #endregion

        #region BehaviorReverse Command
        private AsyncCommand _BehaviorReverseCommand;

        public ICommand BehaviorReverseCommand
        {
            get
            {
                if (null == _BehaviorReverseCommand)
                    _BehaviorReverseCommand = new AsyncCommand(BehaviorReverse);
                return _BehaviorReverseCommand;
            }
        }

        private async Task BehaviorReverse()
        {
            //this.ViewModelManager().Lock(this.GetHashCode(), "Wait", "BehaviorReverse");
            

            try
            {
                SequenceRunnerModule.RunReverse();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                
            }
        }
        #endregion

        #region ManualRecoverty Button Command

        private AsyncCommand _AlternateRecoveryManualModeCommand;

        public ICommand AlternateRecoveryManualModeCommand
        {
            get
            {
                if (null == _AlternateRecoveryManualModeCommand)
                    _AlternateRecoveryManualModeCommand = new AsyncCommand(AlternateRecoveryManualMode);
                return _AlternateRecoveryManualModeCommand;
            }
        }

        private async Task AlternateRecoveryManualMode()
        {
            //this.ViewModelManager().Lock(this.GetHashCode(), "Wait", "AlternateRecoveryManualMode");
            

            try
            {
                SequenceRunnerModule.AlternateManualMode();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                
            }
        }
        #endregion

        #region DirectRun Button Command
        private AsyncCommand _ManualReverseCommand;

        public ICommand ManualReverseCommand
        {
            get
            {
                if (null == _ManualReverseCommand)
                    _ManualReverseCommand = new AsyncCommand(ManualReverse);
                return _ManualReverseCommand;
            }
        }

        private async Task ManualReverse()
        {
            //this.ViewModelManager().Lock(this.GetHashCode(), "Wait", "ManualReverse");
            

            try
            {
                SequenceRunnerModule.RunManualReverse();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                
            }
        }

        private AsyncCommand _ManualRetryCommand;

        public ICommand ManualRetryCommand
        {
            get
            {
                if (null == _ManualRetryCommand)
                    _ManualRetryCommand = new AsyncCommand(ManualRetry);
                return _ManualRetryCommand;
            }
        }

        private async Task ManualRetry()
        {
            //this.ViewModelManager().Lock(this.GetHashCode(), "Wait", "ManualRetry");
            

            try
            {
                SequenceRunnerModule.RunManualRetry();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                
            }
        }
        #endregion

        #region End Button Command
        private AsyncCommand _EndCommand;

        public ICommand EndCommand
        {
            get
            {
                if (null == _EndCommand) _EndCommand = new AsyncCommand(End);
                return _EndCommand;
            }
        }

        private async Task End()
        {
            //this.ViewModelManager().Lock(this.GetHashCode(), "Wait", "End");
            

            try
            {
                SequenceRunnerModule.End();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                
            }
        }

        private RelayCommand _X2ViewChangeCommand;
        public RelayCommand X2ViewChangeCommand
        {
            get
            {
                if (null == _X2ViewChangeCommand) _X2ViewChangeCommand = new RelayCommand(Viewx2Func);
                return _X2ViewChangeCommand;
            }
        }

        public void Viewx2Func() // 2x view
        {
            try
            {
                mIsItDisplayed2RateMagnification = !mIsItDisplayed2RateMagnification;
                ViewModelManager.Set3DCamPosition((CameraViewPoint)mViewNUM, mIsItDisplayed2RateMagnification);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private RelayCommand _CWViewChangeCommand;
        public ICommand CWViewChangeCommand
        {
            get
            {
                if (null == _CWViewChangeCommand) _CWViewChangeCommand = new RelayCommand(CWVIEWFunc);
                return _CWViewChangeCommand;
            }
        }

        public void CWVIEWFunc() //CW
        {
            try
            {
                mViewNUM = ((Enum.GetNames(typeof(CameraViewPoint)).Length) + (--mViewNUM)) % Enum.GetNames(typeof(CameraViewPoint)).Length;
                ViewModelManager.Set3DCamPosition((CameraViewPoint)Math.Abs(mViewNUM), mIsItDisplayed2RateMagnification);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _CenterViewChangeCommand;
        public ICommand CenterViewChangeCommand
        {
            get
            {
                if (null == _CenterViewChangeCommand) _CenterViewChangeCommand = new RelayCommand(CenterViewFunc);
                return _CenterViewChangeCommand;
            }
        }

        public void CenterViewFunc() //FRONT
        {
            try
            {
                mViewNUM = 0;
                mIsItDisplayed2RateMagnification = false;
                ViewModelManager.Set3DCamPosition((CameraViewPoint)mViewNUM, mIsItDisplayed2RateMagnification);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _CCWViewChangeCommand;
        public ICommand CCWViewChangeCommand
        {
            get
            {
                if (null == _CCWViewChangeCommand) _CCWViewChangeCommand = new RelayCommand(CCWViewFunc);
                return _CCWViewChangeCommand;
            }
        }
        public void CCWViewFunc() // CCW
        {
            try
            {
                mViewNUM = Math.Abs(++mViewNUM % Enum.GetNames(typeof(CameraViewPoint)).Length);
                ViewModelManager.Set3DCamPosition((CameraViewPoint)mViewNUM, mIsItDisplayed2RateMagnification);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        public Task<EventCodeEnum> InitViewModel()
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //this.SysState().SetSetUpState();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Stage3DModel = null;
                    Stage3DModel = this.ViewModelManager().Stage3DModel;
                });
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            try
            {
                //this.SysState().SetSetUpDoneState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        #endregion
    }

    public class ScrollIntoViewBehavior : Behavior<ListBox>
    {
        protected override void OnAttached()
        {
            try
            {
                base.OnAttached();
                AssociatedObject.SelectionChanged += ScrollIntoView;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        protected override void OnDetaching()
        {
            try
            {
                AssociatedObject.SelectionChanged -= ScrollIntoView;
                base.OnDetaching();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void ScrollIntoView(object o, SelectionChangedEventArgs e)
        {
            try
            {
                ListBox b = (ListBox)o;
                if (b == null)
                    return;
                if (b.SelectedItem == null)
                    return;

                ListBoxItem item = (ListBoxItem)((ListBox)o).ItemContainerGenerator
                                    .ContainerFromItem(((ListBox)o).SelectedItem);
                if (item != null) item.BringIntoView();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
