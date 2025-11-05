using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Threading.Tasks;
using LogModule;
using System.Threading;
using System.Runtime.Serialization;
using MetroDialogInterfaces;
using Autofac;
using SecuritySystem;
using System.Windows;

namespace RelayCommandBase
{
    public static class Extensions_ICommand
    {
        private static Autofac.IContainer StageContainer;
        private static Autofac.IContainer LoaderContainer;

        public static void SetStageContainer(this ICommand command, Autofac.IContainer container)
        {
            StageContainer = container;
        }
        public static void SetLoaderContainer(this ICommand command, Autofac.IContainer container)
        {
            LoaderContainer = container;
        }

        private static IMetroDialogManager _MetroDialogManager;
        public static IMetroDialogManager MetroDialogManager(this ICommand module)
        {
            if (_MetroDialogManager == null)
                _MetroDialogManager = StageContainer?.Resolve<IMetroDialogManager>();
            return _MetroDialogManager;
        }

    }

    public interface IAsyncCommand : ICommand
    {
        Task ExecuteAsync(object parameter);
    }
    public interface IAsyncCommand<T> : ICommand
    {
        Task ExecuteAsync(T parameter);
        bool CanExecute(T parameter);
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class PreventLogging : Attribute
    {

    }
    [Serializable, DataContract]
    public class RelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        private Action methodToExecute;
        private Action tipScreenExecute;
        private Func<bool> canExecuteEvaluator;
        private ICommand cstPickCommand;

        public RelayCommand(Action methodToExecute, Func<bool> canExecuteEvaluator, Action tipscreen)
        {
            try
            {
                this.methodToExecute = methodToExecute;
                this.canExecuteEvaluator = canExecuteEvaluator;
                this.tipScreenExecute = tipscreen;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public RelayCommand(Action methodToExecute, Func<bool> canExecuteEvaluator)
        {
            try
            {
                this.methodToExecute = methodToExecute;
                this.canExecuteEvaluator = canExecuteEvaluator;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public RelayCommand(Action methodToExecute)
            : this(methodToExecute, null)
        {
        }

        public RelayCommand(ICommand cstPickCommand)
        {
            this.cstPickCommand = cstPickCommand;
        }

        public bool CanExecute(object parameter)
        {
            try
            {
                if (this.canExecuteEvaluator == null)
                {
                    return true;
                }
                else
                {
                    bool result = this.canExecuteEvaluator.Invoke();
                    if (tipScreenExecute != null)
                    {
                        return true;
                    }
                    else
                    {
                        return result;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void Execute(object parameter)
        {
            try
            {
                bool loggingEnabled = !Attribute.IsDefined(methodToExecute?.Method, typeof(PreventLogging));
                if (loggingEnabled == true)
                {
                    LoggerManager.Debug($"Execute [{methodToExecute.Method.Name}] relay command.");
                }

                if (this.canExecuteEvaluator != null)
                {
                    if (this.canExecuteEvaluator.Invoke() == true)
                    {
                        this.methodToExecute.Invoke();

                        if (loggingEnabled == true)
                        {
                            LoggerManager.Debug($"Execute [{methodToExecute.Method.Name}] relay command DONE.");
                        }
                    }
                    else
                    {
                        if (tipScreenExecute != null)
                        {
                            this.tipScreenExecute.Invoke();
                            if (loggingEnabled == true)
                            {
                                LoggerManager.Debug($"Tip screen execute [{methodToExecute.Method.Name}] relay command DONE.");
                            }
                        }
                    }
                }
                else
                {
                    this.methodToExecute.Invoke();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
    [Serializable, DataContract]
    public class RelayCommand<T> : ICommand
    {
        #region Fields

        private readonly Action<T> _execute = null;
        private readonly Predicate<T> _canExecute = null;
        private ICommand cmdSelectTemplateShapeClick;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new command that can always execute.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        public RelayCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Creates a new command with conditional execution.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            try
            {
                if (execute == null)
                    throw new ArgumentNullException("execute");

                _execute = execute;
                _canExecute = canExecute;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public RelayCommand(ICommand cmdSelectTemplateShapeClick)
        {
            this.cmdSelectTemplateShapeClick = cmdSelectTemplateShapeClick;
        }

        #endregion

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecute != null)
                    CommandManager.RequerySuggested += value;
            }
            remove
            {
                if (_canExecute != null)
                    CommandManager.RequerySuggested -= value;
            }
        }

        public void Execute(object parameter)
        {
            try
            {
                bool loggingEnabled = !Attribute.IsDefined(_execute.Method, typeof(PreventLogging));

                if (loggingEnabled == true)
                {
                    LoggerManager.Debug($"Execute [{_execute.Method.Name}] relay command.");
                }

                if (_execute != null)
                {
                    _execute((T)parameter);

                    if (loggingEnabled == true)
                    {
                        LoggerManager.Debug($"Execute [{_execute.Method.Name}] relay command DONE.");
                    }
                }
                else
                {
                    if (loggingEnabled == true)
                    {
                        LoggerManager.Debug("Not Exist Command");
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion
    }

    [Serializable, DataContract]
    public class AsyncCommand : IAsyncCommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;

        public bool ShowWaitDialog = true;
        public string ShowWaitMessage = "Wait";

        private Func<Task> _CheckJobFinishedExecute;

        [DataMember]
        private bool _isExecuting;

        private CancellationTokenSourcePack _CancelTokenPack;
        public CancellationTokenSourcePack CancelTokenPack
        {
            get { return _CancelTokenPack; }
            set
            {
                if (value != _CancelTokenPack)
                {
                    _CancelTokenPack = value;
                }
            }
        }

        private string _CancelButtonText;

        public string CancelButtonText
        {
            get { return _CancelButtonText; }
            set { _CancelButtonText = value; }
        }

        private Action<object> _CancelAction;

        public Action<object> CancelAction
        {
            get { return _CancelAction; }
            set { _CancelAction = value; }
        }
        private object _CancelActionObject;

        public object CancelActionObject
        {
            get { return _CancelActionObject; }
            set { _CancelActionObject = value; }
        }

        private bool _KeepDialogWhenCancelButtonClick;

        public bool KeepDialogWhenCancelButtonClick
        {
            get { return _KeepDialogWhenCancelButtonClick; }
            set { _KeepDialogWhenCancelButtonClick = value; }
        }


        private string _HashCode;
        public string HashCode
        {
            get { return _HashCode; }
            set
            {
                if (value != _HashCode)
                {
                    _HashCode = value;
                }
            }
        }


        public AsyncCommand(Func<Task> execute) : this(execute, () => true)
        {
        }

        public AsyncCommand(Func<Task> execute, Func<bool> canExecute)
        {
            try
            {
                _execute = execute;
                _canExecute = canExecute;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public AsyncCommand(Func<Task> execute, bool showWaitCancel = true, string showWaitMessage = "Wait") : this(execute, () => true)
        {
            ShowWaitDialog = showWaitCancel;
            ShowWaitMessage = showWaitMessage;
        }

        public void SetJobTask(Func<Task> task)
        {
            _CheckJobFinishedExecute = task;
        }

        public bool CanExecute(object parameter)
        {
            return !(_isExecuting && (_canExecute?.Invoke() ?? true));
        }

        public event EventHandler CanExecuteChanged;

        public async void Execute(object parameter)
        {
            await ExecuteAsync(parameter);

        }

        public void SetCancelTokenPack(CancellationTokenSourcePack TokenPack, string cancelButtonText = "", Action<object> cancelAction = null, object cancelActionObject = null, bool keepDialogWhenCancelButtonClick = false)
        {
            CancelTokenPack = TokenPack;
            CancelButtonText = cancelButtonText;
            CancelAction = cancelAction;
            CancelActionObject = cancelActionObject;
            KeepDialogWhenCancelButtonClick = keepDialogWhenCancelButtonClick;
        }

        private async Task ShowWaitCancelDialog()
        {
            try
            {
                CancellationTokenSource token = null;

                // Show하기전에, Cancel Token 확인
                // TokenSource를 Set해놓은 커맨드의 경우, Cancel이 가능해야 한다.
                if (CancelTokenPack != null && CancelTokenPack.TokenSource != null)
                {
                    //IsUseCancelButton = Visibility.Visible;
                    if (this.CancelTokenPack.ReNew)
                    {
                        this.CancelTokenPack.TokenSource = new CancellationTokenSource();
                        this.CancelTokenPack.ReNew = false;
                    }

                    token = this.CancelTokenPack.TokenSource;
                }

                HashCode = SecurityUtil.GetHashCode_SHA256(DateTime.Now.Ticks + this.GetType().FullName);

                if (this.MetroDialogManager() != null & ShowWaitDialog)
                {

                    if(CancelTokenPack != null && (String.IsNullOrEmpty(CancelButtonText) == false || CancelAction != null || CancelActionObject != null))
                    {
                        await this.MetroDialogManager()?.ShowWaitCancelDialog(HashCode, ShowWaitMessage, token, cancelButtonText: CancelButtonText, action: CancelAction, actionObject: CancelActionObject, keepDialogWhenCancelButtonClick: KeepDialogWhenCancelButtonClick);
                    }
                    else
                    {
                        await this.MetroDialogManager()?.ShowWaitCancelDialog(HashCode, ShowWaitMessage, token);

                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async Task CloseWaitCancelDialog()
        {
            try
            {
                if (this.MetroDialogManager() != null && ShowWaitDialog)
                {
                    await this.MetroDialogManager()?.CloseWaitCancelDialaog(HashCode);
                }

                HashCode = string.Empty;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task ExecuteAsync(object parameter)
        {
            bool loggingEnabled = true;
            Task waitTask = null;
            string dialogHash = this.GetHashCode().ToString();
            try
            {
                loggingEnabled = !Attribute.IsDefined(_execute.Method, typeof(PreventLogging));
                if (loggingEnabled == true)
                {
                    LoggerManager.Debug($"Execute [{_execute.Method.Name}] async command.");
                }
                if (CanExecute(null))
                {
                    _isExecuting = true;

                    OnCanExecuteChanged();

                    await ShowWaitCancelDialog().ConfigureAwait(false);
                    if (_CheckJobFinishedExecute != null)
                    {
                        //waitTask = Task.Run( () => _CheckJobFinishedExecute());
                        waitTask = new Task(() => _CheckJobFinishedExecute());
                        //waitTask.ConfigureAwait(false);

                    }

                    //Task task = Task.Run(() => _execute());
                    //Task task = new Task(() => _execute());
                    ////task.ConfigureAwait(false);
                    //task.Start();
                    //await task;

                    //minskim// ProberSystem 종료시 CloseWaitCancelDialog() 호출 없이 _execute함수 내에서 프로그램이 종료되게 된다.
                    //이 경우 LoaderSystem이 무한 대기 상태에 빠지게 되므로 종료 전에 CloseWaitCancelDialog를 호출 하도록 함
                    //if (AppDomain.CurrentDomain.FriendlyName.Equals("ProberSystem.exe") && _execute.Method.Name.Equals("ShutDownSystem"))
                    //{
                    //    await CloseWaitCancelDialog();
                    //}

                    await _execute();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                _isExecuting = false;
                if (loggingEnabled == true)
                {
                    LoggerManager.Debug($"Execute [{_execute.Method.Name}] async command DONE.");
                }
                OnCanExecuteChanged();

                if (waitTask != null)
                {
                    waitTask.Start();
                    await waitTask;
                }

                await CloseWaitCancelDialog();
            }
        }

        protected virtual void OnCanExecuteChanged()
        {
            try
            {
                if (Application.Current.Dispatcher.CheckAccess())
                {
                    if (CanExecuteChanged != null) CanExecuteChanged(this, new EventArgs());
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (CanExecuteChanged != null) CanExecuteChanged(this, new EventArgs());
                    });
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
            }

        }
    }
    [Serializable, DataContract]
    public class AsyncCommand<T> : IAsyncCommand
    {
        //private readonly Func<Task> _execute;
        private readonly Func<T, Task> _execute;
        //private readonly Func<bool> _canExecute;
        private readonly Predicate<T> _canExecute = null;

        public bool ShowWaitDialog = true;
        public string ShowWaitMessage = "Wait";

        private Func<Task> _CheckJobFinishedExecute;

        private bool _isExecuting;
        private Func<Task> jogStepPosMethod;

        private CancellationTokenSourcePack _CancelTokenPack = new CancellationTokenSourcePack();
        public CancellationTokenSourcePack CancelTokenPack
        {
            get { return _CancelTokenPack; }
            set
            {
                if (value != _CancelTokenPack)
                {
                    _CancelTokenPack = value;
                }
            }
        }

        private string _HashCode;
        public string HashCode
        {
            get { return _HashCode; }
            set
            {
                if (value != _HashCode)
                {
                    _HashCode = value;
                }
            }
        }

        public void SetJobTask(Func<Task> task)
        {
            _CheckJobFinishedExecute = task;
        }

        public AsyncCommand(Func<T, Task> execute) : this(execute, null)
        {
        }
        public AsyncCommand(Func<T, Task> execute, bool showWaitCancel = true, string showWaitMessage = "Wait") : this(execute, null)
        {
            ShowWaitDialog = showWaitCancel;
            ShowWaitMessage = showWaitMessage;
        }

        //public AsyncCommand(Action<T> execute, Func<bool> canExecute)
        public AsyncCommand(Func<T, Task> execute, Predicate<T> canExecute)
        {
            try
            {
                _execute = execute;
                _canExecute = canExecute;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public AsyncCommand(Func<Task> jogStepPosMethod)
        {
            this.jogStepPosMethod = jogStepPosMethod;
        }

        //public bool CanExecute(object parameter)
        //{
        //    return !(_isExecuting && _canExecute());
        //}

        private async Task ShowWaitCancelDialog()
        {
            // Show하기전에, Cancel Token 확인
            // TokenSource를 Set해놓은 커맨드의 경우, Cancel이 가능해야 한다.
            if (this.CancelTokenPack.TokenSource != null)
            {
                //IsUseCancelButton = Visibility.Visible;
                if (this.CancelTokenPack.ReNew)
                {
                    this.CancelTokenPack.TokenSource = new CancellationTokenSource();
                    this.CancelTokenPack.ReNew = false;
                }
            }

            HashCode = SecurityUtil.GetHashCode_SHA256(DateTime.Now.Ticks + this.GetType().FullName);

            if (this.MetroDialogManager() != null)
            {
                await this.MetroDialogManager()?.ShowWaitCancelDialog(HashCode, ShowWaitMessage);
            }
        }

        private async Task CloseWaitCancelDialog()
        {
            try
            {
                if (this.MetroDialogManager() != null && ShowWaitDialog)
                {
                    await this.MetroDialogManager()?.CloseWaitCancelDialaog(HashCode);
                }

                HashCode = string.Empty;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool CanExecute(object parameter)
        {
            return (!_isExecuting) && (_canExecute == null ? true : _canExecute((T)parameter));
        }

        public event EventHandler CanExecuteChanged;


        public async void Execute(object parameter)
        {
            await ExecuteAsync(parameter);
        }

        protected virtual void OnCanExecuteChanged()
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                if (CanExecuteChanged != null) CanExecuteChanged(this, new EventArgs());
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (CanExecuteChanged != null) CanExecuteChanged(this, new EventArgs());
                });
            }
        }

        public async Task ExecuteAsync(object parameter)
        {
            bool loggingEnabled = true;
            Task waitTask = null;

            //_isExecuting = true;
            //OnCanExecuteChanged();

            try
            {
                loggingEnabled = !Attribute.IsDefined(_execute.Method, typeof(PreventLogging));

                // TODO : CanExecute
                if (CanExecute(null))
                {
                    if (loggingEnabled == true)
                    {
                        LoggerManager.Debug($"Execute [{_execute.Method.Name}] async generic command.");
                    }

                    _isExecuting = true;
                    OnCanExecuteChanged();

                    if (ShowWaitDialog == true)
                    {
                        await ShowWaitCancelDialog().ConfigureAwait(false);
                    }

                    if (_CheckJobFinishedExecute != null)
                    {
                        //waitTask = Task.Run(() => _CheckJobFinishedExecute());
                        waitTask = new Task(() => _CheckJobFinishedExecute());
                    }
                    //Task task = new Task(() => _execute((T)parameter));
                    //task.Start();
                    //await task;
                    await _execute((T)parameter);
                }
                else
                {
                    if (loggingEnabled == true)
                    {
                        LoggerManager.Debug($"Can not Execute [{_execute.Method.Name}] async generic command. _isExecuting = {_isExecuting}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
            }
            finally
            {
                _isExecuting = false;
                if (loggingEnabled == true)
                {
                    LoggerManager.Debug($"Execute [{_execute.Method.Name}] async generic DONE.");
                }

                OnCanExecuteChanged();

                if (waitTask != null)
                {
                    waitTask.Start();
                    await waitTask;
                }

                await CloseWaitCancelDialog();
            }
        }
    }

    public static class AsyncHelpers
    {
        /// <summary>
        /// Execute's an async Task<T> method which has a void return value synchronously
        /// </summary>
        /// <param name="task">Task<T> method to execute</param>
        public static void RunSync(Func<Task> task)
        {
            var oldContext = SynchronizationContext.Current;
            var synch = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synch);
            synch.Post(async _ =>
            {
                try
                {
                    await task();
                }
                catch (Exception e)
                {
                    synch.InnerException = e;
                    throw e;
                }
                finally
                {
                    synch.EndMessageLoop();
                }
            }, null);
            synch.BeginMessageLoop();

            SynchronizationContext.SetSynchronizationContext(oldContext);
        }

        /// <summary>
        /// Execute's an async Task<T> method which has a T return type synchronously
        /// </summary>
        /// <typeparam name="T">Return Type</typeparam>
        /// <param name="task">Task<T> method to execute</param>
        /// <returns></returns>
        public static T RunSync<T>(Func<Task<T>> task)
        {
            var oldContext = SynchronizationContext.Current;
            var synch = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synch);
            T ret = default(T);
            synch.Post(async _ =>
            {
                try
                {
                    ret = await task();
                }
                catch (Exception e)
                {
                    synch.InnerException = e;
                    throw;
                }
                finally
                {
                    synch.EndMessageLoop();
                }
            }, null);
            synch.BeginMessageLoop();
            SynchronizationContext.SetSynchronizationContext(oldContext);
            return ret;
        }

        private class ExclusiveSynchronizationContext : SynchronizationContext
        {
            private bool done;
            public Exception InnerException { get; set; }
            readonly AutoResetEvent workItemsWaiting = new AutoResetEvent(false);
            readonly Queue<Tuple<SendOrPostCallback, object>> items =
                new Queue<Tuple<SendOrPostCallback, object>>();

            public override void Send(SendOrPostCallback d, object state)
            {
                throw new NotSupportedException("We cannot send to our same thread");
            }

            public override void Post(SendOrPostCallback d, object state)
            {
                lock (items)
                {
                    items.Enqueue(Tuple.Create(d, state));
                }
                workItemsWaiting.Set();
            }

            public void EndMessageLoop()
            {
                Post(_ => done = true, null);
            }

            public void BeginMessageLoop()
            {
                while (!done)
                {
                    Tuple<SendOrPostCallback, object> task = null;
                    lock (items)
                    {
                        if (items.Count > 0)
                        {
                            task = items.Dequeue();
                        }
                    }
                    if (task != null)
                    {
                        task.Item1(task.Item2);
                        if (InnerException != null) // the method threw an exeption
                        {
                            throw new AggregateException("AsyncHelpers.Run method threw an exception.", InnerException);
                        }
                    }
                    else
                    {
                        workItemsWaiting.WaitOne();
                    }
                }
            }

            public override SynchronizationContext CreateCopy()
            {
                return this;
            }
        }
    }
}