using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.CardChange;
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

namespace TestHeadDockScreenVM
{
    public class TestHeadDockScreenViewModel : IMainScreenViewModel
    {
        private readonly Guid _ViewModelGUID = new Guid("F1A99DB1-571B-4871-92ED-4849E77C1755");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        #region << PropertyChanged >>
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]String info = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        public TestHeadDockScreenViewModel()
        {
        }

        private bool _IsUndokingMode = true;
        public bool IsUndokingMode
        {
            get { return _IsUndokingMode; }
            set
            {
                if (value != _IsUndokingMode)
                {
                    _IsUndokingMode = value;
                    RaisePropertyChanged(nameof(IsUndokingMode));
                    SequenceCollection = new AsyncObservableCollection<ISequenceBehaviorGroupItem>(this.CardChangeModule().GetTHDockGroupCollection(THDockType.TH_UNDOCK));
                }
            }
        }

        private bool _IsDokingMode;
        public bool IsDokingMode
        {
            get { return _IsDokingMode; }
            set
            {
                if (value != _IsDokingMode)
                {
                    _IsDokingMode = value;
                    RaisePropertyChanged(nameof(IsDokingMode));
                    SequenceCollection = new AsyncObservableCollection<ISequenceBehaviorGroupItem>(this.CardChangeModule().GetTHDockGroupCollection(THDockType.TH_DOCK));
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

                    ObservableCollection<ISequenceBehaviorGroupItem> SeqDockgroup = this.CardChangeModule()?.GetTHDockGroupCollection(THDockType.TH_DOCK);
                    ObservableCollection<ISequenceBehaviorGroupItem> SeqUndockgroup = this.CardChangeModule()?.GetTHDockGroupCollection(THDockType.TH_UNDOCK);

                    //IList<ISequenceBehaviorGroupItem> thDockGroupCollection = this.CardChangeModule().GetTHDockGroupCollection(THDockType.TH_DOCK);
                    //IList<ISequenceBehaviorGroupItem> thUndockGroupCollection = this.CardChangeModule().GetTHDockGroupCollection(THDockType.TH_UNDOCK);

                    if(SeqDockgroup != null && SeqUndockgroup != null)
                    {
                        this.SequenceCollection = new AsyncObservableCollection<ISequenceBehaviorGroupItem>(SeqDockgroup);

                        foreach (var v in SeqDockgroup)
                        {
                            v.InitModule();
                        }

                        this.SequenceCollection = new AsyncObservableCollection<ISequenceBehaviorGroupItem>(SeqDockgroup);

                        foreach (var v in SeqUndockgroup)
                        {
                            v.InitModule();
                        }

                        var dockIOList = this.SequenceRunnerModule.GetInputPorts(SeqDockgroup.ToList());
                        var undockIOList = this.SequenceRunnerModule.GetInputPorts(SeqUndockgroup.ToList());

                        var ioGroup = dockIOList;

                        foreach (var io in undockIOList)
                        {
                            ioGroup.Add(io);
                        }

                        this.IOPorts = new AsyncObservableCollection<IOPortDescripter<bool>>(ioGroup);
                    }

                    this.Stage3DModel = this.ViewModelManager.Stage3DModel;
                    // 
                    LoggerManager.Debug("TestHeadDockScreenViewModel InitModule : {0}");

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

        #region <<Command>>

        #region Tester Docking Command
        private AsyncCommand _TesterHeadUnlockCommand;

        public ICommand TesterHeadUnlockCommand
        {
            get
            {
                if (null == _TesterHeadUnlockCommand)
                    _TesterHeadUnlockCommand = new AsyncCommand(TesterHeadUnlock);
                return _TesterHeadUnlockCommand;
            }
        }

        private async Task TesterHeadUnlock()
        {
            //this.ViewModelManager().Lock(this.GetHashCode(), "Wait", "TesterHeadUnlock");
            try
            {
                

                if (IsUndokingMode == true)
                {
                    SequenceRunnerModule.RunTestHeadDockUndock(THDockType.TH_UNDOCK, true);
                }
                else if (IsUndokingMode == false)
                {
                    SequenceRunnerModule.RunTestHeadDockUndock(THDockType.TH_DOCK, true);
                }
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

        #region Retry Command
        private AsyncCommand _RetryCommand;

        public ICommand RetryCommand
        {
            get
            {
                if (null == _RetryCommand) _RetryCommand = new AsyncCommand(Retry);
                return _RetryCommand;
            }
        }

        private async Task Retry()
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
