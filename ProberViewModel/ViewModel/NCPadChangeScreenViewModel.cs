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
using System.Windows;
using System.Linq;
using ProberInterfaces.State;

namespace NCPadChangeScreenVM
{
    public class NCPadChangeScreenViewModel : IMainScreenViewModel, ISetUpState
    {
        private readonly Guid _ViewModelGUID = new Guid("7E637DA9-B58B-4157-BB16-3E6C6CFF186F");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        #region << PropertyChanged >>
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]String info = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        public NCPadChangeScreenViewModel()
        {
        }
        private bool _IsItDisplayed2RateMagnification;
        public bool IsItDisplayed2RateMagnification
        {
            get { return _IsItDisplayed2RateMagnification; }
            set
            {
                if (value != _IsItDisplayed2RateMagnification)
                {
                    _IsItDisplayed2RateMagnification = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _ViewNUM;
        public int ViewNUM
        {
            get { return _ViewNUM; }
            set
            {
                if (value != _ViewNUM)
                {
                    _ViewNUM = value;
                    RaisePropertyChanged();
                }
            }
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
                if (Initialized == false)
                {
                    this.SequenceRunnerModule = this.SequenceRunner();
                    this.ViewModelManager = this.ViewModelManager();

                    ObservableCollection<ISequenceBehaviorGroupItem> Seqgroup = this.NeedleCleaner()?.GetNCPadChangeGroupCollection();

                    if(Seqgroup != null)
                    {
                        this.SequenceCollection = new AsyncObservableCollection<ISequenceBehaviorGroupItem>();
                        
                        foreach (var v in this.SequenceCollection)
                        {
                            v.InitModule();
                        }
                     
                        this.IOPorts = new AsyncObservableCollection<IOPortDescripter<bool>>(this.SequenceRunnerModule.GetInputPorts(this.SequenceCollection?.ToList()));

                        retval = EventCodeEnum.NONE;
                    }
                    else
                    {
                        LoggerManager.Debug("[NCPadChangeScreenViewModel], InitModule() : Can not make SequenceCollection.");

                    }

                    this.Stage3DModel = this.ViewModelManager.Stage3DModel;

                    SequenceRunnerModule = this.SequenceRunner();
                    //SequenceCollection = this.CardChangeModule().GetCcGroupCollection();

                    LoggerManager.Debug("NCPadChangeScreenViewModel InitModule : {0}");

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

        #region <<Command>>

        #region NC Pad Change Command
        private AsyncCommand _ChangeNCPadCommand;

        public ICommand ChangeNCPadCommand
        {
            get
            {
                if (null == _ChangeNCPadCommand) _ChangeNCPadCommand = new AsyncCommand(ChangeNCPad);
                return _ChangeNCPadCommand;
            }
        }

        private async Task ChangeNCPad()
        {
            //this.ViewModelManager().Lock(this.GetHashCode(), "Wait", "Changing NC Pad");
            try
            {
                

                SequenceRunnerModule.RunNcPadChage(true);
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
            //this.ViewModelManager().Lock(this.GetHashCode(), "Wait", "Retry");

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

        public void Viewx2() // 2x view
        {
            try
            {
                IsItDisplayed2RateMagnification = !IsItDisplayed2RateMagnification;
                ViewModelManager.Set3DCamPosition((CameraViewPoint)ViewNUM, IsItDisplayed2RateMagnification);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void CWVIEW() //CW
        {
            try
            {
                ViewNUM = ((Enum.GetNames(typeof(CameraViewPoint)).Length) + (--ViewNUM)) % Enum.GetNames(typeof(CameraViewPoint)).Length;
                ViewModelManager.Set3DCamPosition((CameraViewPoint)ViewNUM, IsItDisplayed2RateMagnification);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void CenterView() //FRONT
        {
            try
            {
                ViewNUM = 0;
                IsItDisplayed2RateMagnification = false;
                ViewModelManager.Set3DCamPosition((CameraViewPoint)ViewNUM, IsItDisplayed2RateMagnification);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void CCWView() // CCW
        {
            try
            {
                ViewNUM = Math.Abs(++ViewNUM % Enum.GetNames(typeof(CameraViewPoint)).Length);
                ViewModelManager.Set3DCamPosition((CameraViewPoint)ViewNUM, IsItDisplayed2RateMagnification);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
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
                ViewModelManager.Set3DCamPosition((CameraViewPoint)mViewNUM, mIsItDisplayed2RateMagnification);
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
                CenterView();

                ViewNUM = 0;

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
            }
        }
    }
}
