namespace LoaderRecoveryControl
{
    using Autofac;
    using LoaderBase;
    using LoaderBase.Communication;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using RelayCommandBase;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;

    public class LoaderRecoveryMaster : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private List<RecoveryBehavior> _RecoveryBehaviorList = new List<RecoveryBehavior>();
        public List<RecoveryBehavior> RecoveryBehaviorList
        {
            get { return _RecoveryBehaviorList; }
            set
            {
                if (value != _RecoveryBehaviorList)
                {
                    _RecoveryBehaviorList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private RecoveryBehavior _RecoveryBehavior;
        public RecoveryBehavior RecoveryBehavior
        {
            get { return _RecoveryBehavior; }
            set
            {
                if (value != _RecoveryBehavior)
                {
                    _RecoveryBehavior = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _CurrStepIdx;
        public int CurrStepIdx
        {
            get { return _CurrStepIdx; }
            set
            {
                if (value != _CurrStepIdx)
                {
                    _CurrStepIdx = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _isRecoveryBeh;
        public bool isRecoveryBeh
        {
            get { return _isRecoveryBeh; }
            set
            {
                if (value != _isRecoveryBeh)
                {
                    _isRecoveryBeh = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _RecoveryFinished;
        public bool RecoveryFinished
        {
            get { return _RecoveryFinished; }
            set
            {
                if (value != _RecoveryFinished)
                {
                    _RecoveryFinished = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _ExeButtonEnable;
        public bool ExeButtonEnable
        {
            get { return _ExeButtonEnable; }
            set
            {
                if (value != _ExeButtonEnable)
                {
                    _ExeButtonEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        public LoaderRecoveryMaster()
        {
            if(RecoveryBehaviorList.Count == 0)
            {
                RecoveryBehaviorList.Add(new CardHandOffRecovery());
                RecoveryBehaviorList.Add(new LoaderSystemInitRecovery());
            }
        }
        public EventCodeEnum SetRecoveryBehavior(string beh, int cellIdx)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                RecoveryFinished = false;
                ExeButtonEnable = true;
                CurrStepIdx = 0;
                RecoveryBehavior = RecoveryBehaviorList.FirstOrDefault(x => x.Name == beh);
                if (RecoveryBehavior != null)
                {
                    isRecoveryBeh = true;
                    foreach (var step in RecoveryBehavior.RecoveryStepList)
                    {
                        step.ExecuteDone = false;
                    }
                    RecoveryBehavior.SetRemoteMediumProxyForCellRecovery(cellIdx);
                }
                else
                {
                    isRecoveryBeh = false;
                    RecoveryFinished = true;
                    ExeButtonEnable = false;
                    LoggerManager.Debug($"SetRecoveryBehavior RecoveryBehavior is null");
                }
                

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private AsyncCommand _ExecuteCommand;
        public ICommand ExecuteCommand
        {
            get
            {
                if (null == _ExecuteCommand) _ExecuteCommand = new AsyncCommand(Execute);
                return _ExecuteCommand;
            }
        }

        public async Task<EventCodeEnum> Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = await RecoveryBehavior.RecoveryStepList[CurrStepIdx].ExecuteFunc.Invoke();

                if (retVal == EventCodeEnum.NONE)
                {
                    RecoveryBehavior.RecoveryStepList[CurrStepIdx].ExecuteDone = true;
                    CurrStepIdx++;

                    if (CurrStepIdx < RecoveryBehavior.RecoveryStepList.Count)
                    {
                        //RecoveryBehavior.RecoveryStepList[CurrStepIdx].ExecuteDone = true;
                    }
                    else if (CurrStepIdx == RecoveryBehavior.RecoveryStepList.Count)
                    {
                        RecoveryFinished = true;
                        ExeButtonEnable = false;
                    }
                }
                else
                {
                    string message = $"Recovery Execute Fail : Type: {RecoveryBehavior.Name}, Step Description: {RecoveryBehavior.RecoveryStepList[CurrStepIdx].Description}, retVal: {retVal}";
                    string caption = "Warning";
                    MessageBoxButton buttons = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Question;
                    if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.OK)
                    {

                    }
                    LoggerManager.Debug($"Recovery Execute Fail : Type: {RecoveryBehavior.Name}, Step Description: {RecoveryBehavior.RecoveryStepList[CurrStepIdx].Description}, retVal: {retVal}");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public abstract class RecoveryBehavior : IFactoryModule
    {
        public abstract string Name { get; set; }
        public abstract List<RecoveryStep> RecoveryStepList { get; set; }
        public abstract void SetRemoteMediumProxyForCellRecovery(int cellIdx);
    }

    public class RecoveryStep : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public PropertyChangedEventHandler propertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged
        {
            add { this.propertyChanged += value; }
            remove { this.propertyChanged -= value; }
        }
        #endregion

        public RecoveryStep(string desc, int idx, Func<Task<EventCodeEnum>> executeFunc = null, Func<EventCodeEnum> checkFunc = null)
        {
            Description = desc;
            StepIdx = idx;
            ExecuteFunc = executeFunc;
            CheckFunc = checkFunc;
        }

        private Func<Task<EventCodeEnum>> _ExecuteFunc;
        public Func<Task<EventCodeEnum>> ExecuteFunc
        {
            get { return _ExecuteFunc; }
            set
            {
                if (value != _ExecuteFunc)
                {
                    _ExecuteFunc = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Func<EventCodeEnum> _CheckFunc;
        public Func<EventCodeEnum> CheckFunc
        {
            get { return _CheckFunc; }
            set
            {
                if (value != _CheckFunc)
                {
                    _CheckFunc = value;
                    RaisePropertyChanged();
                }
            }
        }
        
        private string _Description;
        public string Description
        {
            get { return _Description; }
            set
            {
                if (value != _Description)
                {
                    _Description = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _StepIdx;
        public int StepIdx
        {
            get { return _StepIdx; }
            set
            {
                if (value != _StepIdx)
                {
                    _StepIdx = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _ExecuteDone;
        public bool ExecuteDone
        {
            get { return _ExecuteDone; }
            set
            {
                if (value != _ExecuteDone)
                {
                    _ExecuteDone = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum Check()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (CheckFunc != null)
                {
                    retVal = CheckFunc.Invoke();
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class CardHandOffRecovery : RecoveryBehavior
    {
        private ILoaderCommunicationManager _LoaderCommunicationManager => this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
        private ILoaderModule LoaderModule => this.GetLoaderContainer().Resolve<ILoaderModule>();


        public CardHandOffRecovery()
        {
            Name = this.GetType().Name;
            MakeRecoveryStep();
        }

        private IRemoteMediumProxy _RemoteMediumProxy;
        public IRemoteMediumProxy RemoteMediumProxy
        {
            get { return _RemoteMediumProxy; }
            set
            {
                if (value != _RemoteMediumProxy)
                {
                    _RemoteMediumProxy = value;
                }
            }
        }

        private string _Name;
        public override string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                }
            }
        }

        private List<RecoveryStep> _RecoveryStepList = new List<RecoveryStep>();
        public override List<RecoveryStep> RecoveryStepList
        {
            get { return _RecoveryStepList; }
            set
            {
                if (value != _RecoveryStepList)
                {
                    _RecoveryStepList = value;
                }
            }
        }

        public override void SetRemoteMediumProxyForCellRecovery(int cellIdx)
        {
            try
            {
                RemoteMediumProxy = _LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>(cellIdx);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void MakeRecoveryStep()
        {
            try
            {
                #region Foreced Drop Pod
                string desc1 = "1. Foreced Drop Pod";

                var funcTask1 = new Func<Task<EventCodeEnum>>(async () =>
                {
                    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                    try
                    {
                        retVal = await RemoteMediumProxy.GPCC_OP_ForcedDropPodCommand();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }

                    return retVal;
                });

                RecoveryStep step1 = new RecoveryStep(desc1, 0, executeFunc: funcTask1);
                RecoveryStepList.Add(step1);
                #endregion

                #region Retract Card Arm
                string desc2 = "2. Retract Card Arm";

                var funcTask2 = new Func<Task<EventCodeEnum>>(() =>
                {
                    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                    try
                    {
                        var loader = LoaderModule;
                        double dist = 0;

                        var actPos = loader.MotionManager.GetAxis(EnumAxisConstants.LCC).Status.Position.Actual;
                        if (actPos > 0)
                        {
                            LoggerManager.Debug($"LCC actPos value: {actPos}");
                            dist -= actPos;
                            retVal = loader.GetLoaderCommands().JogMove(loader.MotionManager.GetAxis(EnumAxisConstants.LCC), dist);
                        }
                        else if (actPos == 0)
                        {
                            LoggerManager.Debug($"LCC actPos is aleady zero, value: {actPos}");
                            retVal = EventCodeEnum.NONE;
                        }
                        else
                        {
                            LoggerManager.Debug($"LCC actPos is negative, value: {actPos}");
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }

                    return Task.FromResult(retVal);
                });

                RecoveryStep step2 = new RecoveryStep(desc2, 1, executeFunc: funcTask2);
                RecoveryStepList.Add(step2);
                #endregion
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
    public class LoaderSystemInitRecovery : RecoveryBehavior
    {
        private ILoaderCommunicationManager _LoaderCommunicationManager => this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
        private ILoaderModule LoaderModule => this.GetLoaderContainer().Resolve<ILoaderModule>();


        public LoaderSystemInitRecovery()
        {
            Name = this.GetType().Name;
            MakeRecoveryStep();
        }

        private string _Name;
        public override string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                }
            }
        }

        private List<RecoveryStep> _RecoveryStepList = new List<RecoveryStep>();
        public override List<RecoveryStep> RecoveryStepList
        {
            get { return _RecoveryStepList; }
            set
            {
                if (value != _RecoveryStepList)
                {
                    _RecoveryStepList = value;
                }
            }
        }

        public void MakeRecoveryStep()
        {
            try
            {
                #region System Init
                string desc1 = "1. System Initialize";

                //경고 확인 필요.
                var funcTask1 = new Func<Task<EventCodeEnum>>(() =>
                {
                    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                    try
                    {
                        retVal = LoaderModule.SystemInit();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }

                    return Task.FromResult(retVal);
                });

                RecoveryStep step1 = new RecoveryStep(desc1, 0, executeFunc: funcTask1);
                RecoveryStepList.Add(step1);
                #endregion
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void SetRemoteMediumProxyForCellRecovery(int cellIdx)
        {
            throw new NotImplementedException();
        }
    }
}