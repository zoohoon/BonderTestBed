using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MetroDialogModule
{
    using Autofac;
    using DllImporter;
    using LoaderController.GPController;
    using LogModule;
    using MahApps.Metro.Controls;
    using MahApps.Metro.Controls.Dialogs;
    using MetroDialogInterfaces;
    using PasswordInputDialogServiceProvider;
    using ProberErrorCode;
    using ProberInterfaces;
    using RelayCommandBase;
    using SingleInputDialogServiceProvider;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using WaitCancelDialogServiceProvider;
    using ProberInterfaces.DialogControl;
    using System.IO;

    // �پ��� MetroWindow���� �������ִ� �پ��� �Լ��� ���� ����ϰ� �ϸ�, ������ �ϱ� ���� ����� ��.

    public class MetroDialogManager : IMetroDialogManager, INotifyPropertyChanged, IModule, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ServiceProvider

        public WaitCancelDialogService WaitCancelDialogServiceProvider;
        public SingleInputDialogService SingleInputDialogServiceProvider;
        public PasswordInputDialogService PasswordInputDialogServiceProvider;

        public WaitCancelDialogService TargetWaitCancelDialogServiceProvider;
        #endregion
        public bool Initialized { get; set; }
        private MetroWindow MetroWindow;
        private MetroWindow TargetWindow;
        public event MetroDialogShowEventHandler MetroDialoShow;
        public event MetroDialogCloseEventHandler MetroDialogClose;


        public event MessageDialogShowEventHandler MessageDialogShow;

        public event SingleInputDialogShowEventHandler SingleInputDialogShow;
        public event SingleInputGetInputDataEventHandler SingleInputGetInputData;

        private IPnpAdvanceSetupViewModel _ViewModel = null;
        ILoaderDoorDisplayDialogService LoaderDoorDialog;
        ILoaderParkingDisplayDialogService LoaderParkingDialog;
        static SemaphoreSlim dialogSemaphore = new SemaphoreSlim(1, 1);
        static SemaphoreSlim dialogCloseSemaphore = new SemaphoreSlim(1, 1);
        static SemaphoreSlim waitDialogSemaphore = new SemaphoreSlim(1, 1);
        static SemaphoreSlim waitDialogListSemaphore = new SemaphoreSlim(1, 1);
        static SemaphoreSlim metroSemaphore = new SemaphoreSlim(1, 1);
        private AsyncObservableCollection<BaseMetroDialogPack> _OpenDialogs = new AsyncObservableCollection<BaseMetroDialogPack>();
        public AsyncObservableCollection<BaseMetroDialogPack> OpenDialogs
        {
            get { return _OpenDialogs; }
            set { _OpenDialogs = value; }
        }

        private AsyncObservableCollection<BaseMetroDialogPack> _WaitDialogs = new AsyncObservableCollection<BaseMetroDialogPack>();
        public AsyncObservableCollection<BaseMetroDialogPack> WaitDialogs
        {
            get { return _WaitDialogs; }
            set { _WaitDialogs = value; }
        }


        private int _DialogCount = 0;
        public int DialogCount
        {
            get { return _DialogCount; }
            set
            {
                if (value != _DialogCount)
                {
                    _DialogCount = value;
                }
            }
        }

        private bool _MetroWindowLoaded = false;
        public bool MetroWindowLoaded
        {
            get { return _MetroWindowLoaded; }
            set
            {
                if (value != _MetroWindowLoaded)
                {
                    _MetroWindowLoaded = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool AlreadyVisibleFlag = false;

        // Wait Dialog의 Cancel 버튼 클릭 시 Dialog를 닫지 않고 유지하는 옵션
        private bool _keepDialogWhenCancelButtonClick = false;
        public bool keepDialogWhenCancelButtonClick
        {
            get { return _keepDialogWhenCancelButtonClick; }
            set
            {
                if (value != _keepDialogWhenCancelButtonClick)
                {
                    _keepDialogWhenCancelButtonClick = value;
                }
            }
        }

        private static int _waitingThreads = 0;
        public static int waitingThreads
        {
            get { return _waitingThreads; }
            private set
            {
                if (value != _waitingThreads)
                {
                    _waitingThreads = value;
                }
            }
        }

        private BaseMetroDialog _LastOpendDailog;
        public BaseMetroDialog LastOpendDailog
        {
            get { return _LastOpendDailog; }
            set
            {
                if (value != _LastOpendDailog)
                {
                    _LastOpendDailog = value;
                    RaisePropertyChanged();
                }
            }
        }

        // Cancel Button 을 Click 했을 때 수행할 Action
        private Action<object> cancelButtonClickAction = null;

        // Cancel Button Action 을 수행할 때 파라미터로 넘겨줄 object
        private object cancelActionObject = null;

        object waitDialogLockObject = new object();

        public void SetMetroWindowLoaded(bool flag)
        {
            try
            {
                this.MetroWindowLoaded = flag;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        public Window GetMetroWindow(bool doNotCreate = false)
        {
            try
            {
                if (doNotCreate == false)
                {
                    if (MetroWindow == null)
                    {
                        Application.Current.Dispatcher.Invoke(() => MetroWindow = (Application.Current.MainWindow as MahApps.Metro.Controls.MetroWindow));
                    }
                }

                return MetroWindow;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return null;
        }
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Initialized == false)
                {
                    WaitCancelDialogServiceProvider = new WaitCancelDialogService();
                    (WaitCancelDialogServiceProvider as IModule).InitModule();
                    WaitCancelDialogServiceProvider.cmdNegativeButtonClick = WaitCancelDialogCancelCommand;

                    TargetWaitCancelDialogServiceProvider = new WaitCancelDialogService();
                    (TargetWaitCancelDialogServiceProvider as IModule).InitModule();
                    TargetWaitCancelDialogServiceProvider.cmdNegativeButtonClick = WaitCancelDialogCancelCommand;

                    SingleInputDialogServiceProvider = new SingleInputDialogService();
                    (SingleInputDialogServiceProvider as IModule).InitModule();

                    PasswordInputDialogServiceProvider = new PasswordInputDialogService();
                    (PasswordInputDialogServiceProvider as IModule).InitModule();

                    if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Remote)
                    {
                        LoaderDoorDialog = this.GetLoaderContainer().Resolve<ILoaderDoorDisplayDialogService>();
                        LoaderParkingDialog = this.GetLoaderContainer().Resolve<ILoaderParkingDisplayDialogService>();
                    }
                    Application.Current.Dispatcher.Invoke
                        (
                            () => MetroWindow = (Application.Current.MainWindow as MahApps.Metro.Controls.MetroWindow)
                        );

                    Initialized = true;

                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retVal = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void DeInitModule()
        {
            return;
        }
        public bool IsShowingWaitCancelDialog()
        {
            bool retval = false;

            try
            {
                string DialogName = "WaitCancelDialog";

                BaseMetroDialogPack pack = OpenDialogs.Where(x => x.dialog.GetType().Name == DialogName).FirstOrDefault();

                if (pack != null)
                {
                    if (pack.dialog.IsVisible == true)
                    {
                        retval = true;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public async Task SetDataWaitCancelDialog(string message, string hashcoe, CancellationTokenSource canceltokensource = null, string cancelButtonText = "Cancel", bool restarttimer = false)
        {
            try
            {
                if (this.StageCommunicationManager()?.IsEnableDialogProxy() == true)
                {
                    await this.StageCommunicationManager().GetMessageEventHost().SetDataWaitCancelDialog(message, hashcoe, canceltokensource, cancelButtonText, restarttimer);
                    return;
                }

                if (WaitCancelDialogServiceProvider != null)
                {
                    WaitCancelDialogServiceProvider.SetData(message, hashcoe, canceltokensource, cancelButtonText, restarttimer);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task ClearDataWaitCancelDialog(bool restarttimer = false)
        {
            try
            {
                if (this.StageCommunicationManager()?.IsEnableDialogProxy() == true)
                {
                    await this.StageCommunicationManager().GetMessageEventHost().ClearDataWaitCancelDialog(restarttimer);
                    return;
                }

                if (WaitCancelDialogServiceProvider != null)
                {
                    WaitCancelDialogServiceProvider.ClearData(restarttimer);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetMessageToWaitCancelDialog(string message)
        {
            try
            {
                if (WaitCancelDialogServiceProvider != null)
                {
                    WaitCancelDialogServiceProvider.SetMessageData(message);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task ShowWaitCancelDialog(string hashcode, string message, CancellationTokenSource canceltokensource = null, string caller = "", bool localonly = false, string cancelButtonText = "Cancel", Action<object> cancelAction = null, object cancelActionObject = null, bool keepDialogWhenCancelButtonClick = false)
        {
            try
            {
                // 이미 창이 켜져있고 Cancel 버튼 클릭 시 Wait Dialog를 바로 끄지 않는 옵션을 사용하는 경우 외부에서 Wait Dialog를 Close 할 것이므로 다른 Wait Dialog 창을 키려고 할 때 키지 않도록 한다.
                if (IsShowingWaitCancelDialog() && this.keepDialogWhenCancelButtonClick)
                {
                    return;
                }

                if (this.StageCommunicationManager()?.IsEnableDialogProxy() == true && localonly == false)
                {
                    caller = SystemManager.SysExcuteMode.ToString();

                    try
                    {
                        await this.StageCommunicationManager().GetMessageEventHost().ShowWaitCancelDialog(hashcode, message, canceltokensource, caller, cancelButtonText);
                    }
                    catch (Exception err)
                    {
                        System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                        await this.StageCommunicationManager().GetMessageEventHost().ShowWaitCancelDialog(hashcode, message, canceltokensource, caller, cancelButtonText);
                    }

                    return;
                }

                CustomDialog dialog = WaitCancelDialogServiceProvider.GetDialog();
                cancelButtonClickAction = cancelAction;
                this.cancelActionObject = cancelActionObject;
                this.keepDialogWhenCancelButtonClick = keepDialogWhenCancelButtonClick; // Custom 버튼 클릭 시 Dialog가 Close 되는 옵션 설정.

                if (dialog != null)
                {
                    try
                    {
                        await dialogSemaphore.WaitAsync(10000);
                        await this.ShowWaitWindow(dialog, message, hashcode, canceltokensource, cancelButtonText);
                        WaitCancelDialogServiceProvider.IsOpenWaitCancelDialog = true;
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                    finally
                    {
                        dialogSemaphore.Release();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task CloseWaitCancelDialaog(string hashcode, bool localonly = false)
        {
            try
            {
                // 이미 창이 켜져있고 Cancel 버튼 클릭 시 Wait Dialog를 유지하는 옵션을 사용하는 경우 Close 할 때 해당 옵션을 다시 false로 돌려놓는다.
                if (AlreadyVisibleFlag && keepDialogWhenCancelButtonClick)
                {
                    keepDialogWhenCancelButtonClick = false;
                }

                if (this.StageCommunicationManager()?.IsEnableDialogProxy() == true && localonly == false)
                {
                    await this.StageCommunicationManager().GetMessageEventHost().CloseWaitCancelDialog(hashcode);
                    return;
                }

                CustomDialog dialog = WaitCancelDialogServiceProvider.GetDialog();
                await this.CloseWaitWindow(dialog, hashcode, localonly);
                WaitCancelDialogServiceProvider.IsOpenWaitCancelDialog = false;
                LoggerManager.Debug($"CloseWaitCancelDialaog() : Hash = {hashcode} Dialog closed");

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task ShowWaitCancelDialogTarget(Window window, string hashcode, string message, CancellationTokenSource canceltokensource = null, string caller = "")
        {

            try
            {
                Application.Current.Dispatcher.Invoke
                   (
                       () => TargetWindow = (window as MahApps.Metro.Controls.MetroWindow)
                   );

                CustomDialog dialog = TargetWaitCancelDialogServiceProvider.GetDialog();

                if (dialog != null)
                {
                    try
                    {
                        await dialogSemaphore.WaitAsync(1000);
                        await this.ShowWaitWindowTarget(dialog, message, hashcode, canceltokensource);
                        TargetWaitCancelDialogServiceProvider.IsOpenWaitCancelDialog = true;
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                    finally
                    {
                        dialogSemaphore.Release();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task CloseWaitCancelDialaogTarget(Window window, string hashcode)
        {
            try
            {
                Application.Current.Dispatcher.Invoke
                 (
                     () => TargetWindow = (window as MahApps.Metro.Controls.MetroWindow)
                 );

                // Stage에서 Loader로 
                if (this.StageCommunicationManager()?.IsEnableDialogProxy() == true)
                {
                    await this.StageCommunicationManager().GetMessageEventHost().CloseWaitCancelDialog(hashcode);
                    return;
                }

                CustomDialog dialog = TargetWaitCancelDialogServiceProvider.GetDialog();
                await this.CloseWaitWindowTarget(dialog, hashcode);
                TargetWaitCancelDialogServiceProvider.IsOpenWaitCancelDialog = false;
                LoggerManager.Debug($"CloseWaitCancelDialaog(): Hash = {hashcode} Dialog closed");

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ShowCancelButtonOfWaitCancelDiaglog(CancellationTokenSource canceltokensource , string cancelButtonText, Action<object> cancelAction, object cancelActionObject , bool keepDialogWhenCancelButtonClick , [CallerMemberName] string callFuncNm = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int sourceLineNumber = 0)
        {
            try
            {
                cancelButtonClickAction = cancelAction;
                this.cancelActionObject = cancelActionObject;
                this.keepDialogWhenCancelButtonClick = keepDialogWhenCancelButtonClick; // Custom 버튼 클릭 시 Dialog가 Close 되는 옵션 설정.
                WaitCancelDialogServiceProvider.ShowCancelButton(canceltokensource, cancelButtonText);
                string fileName = Path.GetFileName(filePath);
                LoggerManager.Debug($"MetroDialogManager ShowCancelButtonOfWaitCancelDiaglog().[{fileName}({sourceLineNumber}) '{callFuncNm}']");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void HiddenCancelButtonOfWaitCancelDiaglog([CallerMemberName] string callFuncNm = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int sourceLineNumber = 0)
        {
            try
            {
                WaitCancelDialogServiceProvider.HiddenCancelButton();
                string fileName = Path.GetFileName(filePath);
                LoggerManager.Debug($"MetroDialogManager HiddenCancelButtonOfWaitCancelDiaglog().[{fileName}({sourceLineNumber}) '{callFuncNm}']");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ReshowCancelButtonOfWaitCancelDiaglog([CallerMemberName] string callFuncNm = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int sourceLineNumber = 0)
        {
            try
            {
                WaitCancelDialogServiceProvider.ShowCancelButton();
                string fileName = Path.GetFileName(filePath);
                LoggerManager.Debug($"MetroDialogManager ReshowCancelButtonOfWaitCancelDiaglog().[{fileName}({sourceLineNumber}) '{callFuncNm}']");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        


        private AsyncCommand _WaitCancelDialogCancelCommand;
        public ICommand WaitCancelDialogCancelCommand
        {
            get
            {
                if (null == _WaitCancelDialogCancelCommand) _WaitCancelDialogCancelCommand = new AsyncCommand(WaitCancelDialogCancelCommandFunc, false);
                return _WaitCancelDialogCancelCommand;
            }
        }

        private async Task WaitCancelDialogCancelCommandFunc()
        {
            try
            {
                EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog(
                    "Are you sure you want to cancel?",
                    "Press the Cancel button to cancel the OK button to exit.", EnumMessageStyle.AffirmativeAndNegative);

                if (result == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    CancellationTokenSource token = WaitCancelDialogServiceProvider.cancellationToken;

                    if (token != null)
                    {
                        try
                        {
                            if (token.IsCancellationRequested != true)
                            {
                                token.Cancel();
                            }
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Error(err + "CancellationTokenSource token clear Error occured.");
                        }
                    }

                    // Cancel Button Click 시 등록된 Action을 수행하고 초기화
                    if (null != cancelButtonClickAction)
                    {
                        cancelButtonClickAction(cancelActionObject);
                        cancelButtonClickAction = null;
                    }
                    this.StageSupervisor().SetWaitCancelDialogHashCode(WaitCancelDialogServiceProvider.HashCode);
                    // Cancel Button Click 시 Dialog를 유지하는 옵션을 사용하는 경우 창을 닫지 않고 버튼만 비활성화 시킨다.
                    if (keepDialogWhenCancelButtonClick)
                    {
                        WaitCancelDialogServiceProvider.isEnableCancelButton = false;
                    }
                    else
                    {
                        CustomDialog waitdialog = WaitCancelDialogServiceProvider.GetDialog();
                        await this.MetroDialogManager().CloseWindow(waitdialog);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }

        private RelayCommand _cmdAffirmativeButtonClick;
        public ICommand cmdAffirmativeButtonClick
        {
            get
            {
                if (null == _cmdAffirmativeButtonClick) _cmdAffirmativeButtonClick = new RelayCommand(AffirmativeButtonClick);
                return _cmdAffirmativeButtonClick;
            }
        }

        private void AffirmativeButtonClick()
        {
            try
            {
                CustomDialog dialog = SingleInputDialogServiceProvider.GetDialog();
                SingleInputDialogServiceProvider.SetDialogResult(EnumMessageDialogResult.AFFIRMATIVE);

                this.CloseWindow(dialog).ConfigureAwait(false);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _cmdNegativeButtonClick;
        public ICommand cmdNegativeButtonClick
        {
            get
            {
                if (null == _cmdNegativeButtonClick) _cmdNegativeButtonClick = new RelayCommand(NegativeButtonClick);
                return _cmdNegativeButtonClick;
            }
        }

        private void NegativeButtonClick()
        {
            try
            {
                CustomDialog dialog = SingleInputDialogServiceProvider.GetDialog();
                SingleInputDialogServiceProvider.SetDialogResult(EnumMessageDialogResult.NEGATIVE);

                this.CloseWindow(dialog).ConfigureAwait(false);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _cmdPasswordInputAffirmativeButtonClick;
        public ICommand cmdPasswordInputAffirmativeButtonClick
        {
            get
            {
                if (null == _cmdPasswordInputAffirmativeButtonClick) _cmdPasswordInputAffirmativeButtonClick = new RelayCommand(PasswordInputAffirmativeButtonClick);
                return _cmdPasswordInputAffirmativeButtonClick;
            }
        }

        private void PasswordInputAffirmativeButtonClick()
        {
            try
            {
                CustomDialog dialog = PasswordInputDialogServiceProvider.GetDialog();

                PasswordInputDialogServiceProvider.SetDialogResult(EnumMessageDialogResult.AFFIRMATIVE);

                this.CloseWindow(dialog).ConfigureAwait(false);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _cmdPasswordInputNegativeButtonClick;
        public ICommand cmdPasswordInputNegativeButtonClick
        {
            get
            {
                if (null == _cmdPasswordInputNegativeButtonClick) _cmdPasswordInputNegativeButtonClick = new RelayCommand(PasswordInputNegativeButtonClick);
                return _cmdPasswordInputNegativeButtonClick;
            }
        }
        private void PasswordInputNegativeButtonClick()
        {
            try
            {
                CustomDialog dialog = PasswordInputDialogServiceProvider.GetDialog();
                PasswordInputDialogServiceProvider.SetDialogResult(EnumMessageDialogResult.NEGATIVE);

                this.CloseWindow(dialog).ConfigureAwait(false);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task<EnumMessageDialogResult> ShowSingleInputDialog(string Label = "Input", string posBtnLabel = "OK", string negBtnLabel = "Cancel", string subLabel = "")
        {
            EnumMessageDialogResult retval = EnumMessageDialogResult.NEGATIVE;

            try
            {
                // Stage���� Loader�� 
                if (SingleInputDialogShow != null && (this.StageCommunicationManager()?.IsEnableDialogProxy() == true))
                {
                    retval = await SingleInputDialogShow(Label, posBtnLabel, negBtnLabel, subLabel);
                    return retval;
                }

                CustomDialog dialog = SingleInputDialogServiceProvider.GetDialog();
                SingleInputDialogServiceProvider.SetData(Label, posBtnLabel, negBtnLabel, cmdAffirmativeButtonClick, cmdNegativeButtonClick, subLabel);

                if (dialog != null)
                {
                    await this.ShowMetroWindow(dialog, true);
                    retval = SingleInputDialogServiceProvider.GetDialogResult();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public string GetSingleInputData()
        {
            string retval = string.Empty;

            try
            {
                // Stage���� Loader��
                if (SingleInputGetInputData != null && this.StageCommunicationManager()?.IsEnableDialogProxy() == true)
                {
                    retval = SingleInputGetInputData();
                    return retval;
                }

                retval = SingleInputDialogServiceProvider.GetInputData();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public async Task<EnumMessageDialogResult> ShowMessageDialog(string Title, string Message, EnumMessageStyle enummessagesytel, string AffirmativeButtonText = "OK", string NegativeButtonText = "Cancel", string firstAuxiliaryButtonText = null, string secondAuxiliaryButtonText = null)
        {
            EnumMessageDialogResult retVal = EnumMessageDialogResult.UNDEFIND;

            try
            {
                if (MessageDialogShow != null && (this.StageCommunicationManager()?.IsEnableDialogProxy() == true))
                {
                    var gp_LC = this.LoaderController() as GP_LoaderController;

                    if (gp_LC != null)
                    {
                        string cellstr = gp_LC.ChuckID.ToString();
                        cellstr = cellstr.Replace("CHUCK", "CELL");

                        retVal = await MessageDialogShow($"{Title} in [{cellstr}]", Message, enummessagesytel, AffirmativeButtonText, NegativeButtonText, firstAuxiliaryButtonText, secondAuxiliaryButtonText);

                        return retVal;

                    }
                }

                MessageDialogResult dialogRet = MessageDialogResult.Negative;
                MessageDialogStyle msgStyle = MessageDialogStyle.Affirmative;

                MetroDialogSettings dlgSettings = null;
                switch (enummessagesytel)
                {
                    case EnumMessageStyle.Affirmative:
                        dlgSettings = new MetroDialogSettings()
                        {
                            ColorScheme = MetroDialogColorScheme.Inverted,
                            DefaultButtonFocus = MessageDialogResult.Affirmative,
                            DialogMessageFontSize = 22,
                            AffirmativeButtonText = AffirmativeButtonText,
                            MaximumBodyHeight = 196,
                            AnimateShow = false,
                            AnimateHide = false
                        };
                        msgStyle = MessageDialogStyle.Affirmative;
                        break;
                    case EnumMessageStyle.AffirmativeAndNegative:
                        dlgSettings = new MetroDialogSettings()
                        {
                            ColorScheme = MetroDialogColorScheme.Inverted,
                            DefaultButtonFocus = MessageDialogResult.Affirmative,
                            DialogMessageFontSize = 24,
                            AffirmativeButtonText = AffirmativeButtonText,
                            NegativeButtonText = NegativeButtonText,
                            AnimateShow = false,
                            AnimateHide = false
                        };
                        msgStyle = MessageDialogStyle.AffirmativeAndNegative;
                        break;
                    case EnumMessageStyle.AffirmativeAndNegativeAndDoubleAuxiliary:
                        dlgSettings = new MetroDialogSettings()
                        {
                            ColorScheme = MetroDialogColorScheme.Inverted,
                            DefaultButtonFocus = MessageDialogResult.Affirmative,
                            DialogMessageFontSize = 24,
                            AffirmativeButtonText = AffirmativeButtonText,
                            NegativeButtonText = NegativeButtonText,
                            FirstAuxiliaryButtonText = firstAuxiliaryButtonText,
                            AnimateShow = false,
                            AnimateHide = false
                        };
                        msgStyle = MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary;

                        break;
                    case EnumMessageStyle.AffirmativeAndNegativeAndSingleAuxiliary:
                        dlgSettings = new MetroDialogSettings()
                        {
                            ColorScheme = MetroDialogColorScheme.Inverted,
                            DefaultButtonFocus = MessageDialogResult.Affirmative,
                            DialogMessageFontSize = 24,
                            AffirmativeButtonText = AffirmativeButtonText,
                            NegativeButtonText = NegativeButtonText,
                            FirstAuxiliaryButtonText = firstAuxiliaryButtonText,
                            SecondAuxiliaryButtonText = secondAuxiliaryButtonText,
                            AnimateShow = false,
                            AnimateHide = false
                        };
                        msgStyle = MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary;
                        break;
                }

                ///���α׷��� �� �߱����� MetroWindow �� ����ϸ� Expcetion �� �߻��ؼ� ���α׷��� �� �㶧���� ����ϱ�����.
                if (MetroWindow?.ActualHeight == 0)
                {
                    await Task.Run(() =>
                    {
                        while (true)
                        {
                            if (MetroWindow.ActualHeight != 0)
                            {
                                break;
                            }
                            Thread.Sleep(1);
                        }
                    });
                }

                LoggerManager.Debug($"MessageDialog Show" + $" Title: {Title}" + $" Message: {Message}");

                if(MetroWindow != null)
                {
                    if (MetroWindow.Dispatcher.CheckAccess())
                    {
                        //deprecated => 여기로 들어온다면 Main UI에서 hang과 같은 현상이 일어남. 호출을 하는 부분에서 ConfigureAwait(false)를 써서 작업자 쓰레드에서 처리할 수 있도록 해야함.
                        dialogRet = await MetroWindow.ShowMessageAsync(Title, Message, msgStyle, dlgSettings);
                        //MetroWindow.ResetStoredFocus();
                    }
                    else
                    {
                        dialogRet = await Application.Current.Dispatcher.Invoke<Task<MessageDialogResult>>(() =>
                        {
                            var showTask = MetroWindow.ShowMessageAsync(Title, Message, msgStyle, dlgSettings);

                            return showTask;
                        });
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MetroWindow.ResetStoredFocus();
                        });
                    }
                }
                
                LoggerManager.Debug($"MessageDialog Close");

                switch (dialogRet)
                {
                    case MessageDialogResult.Affirmative:
                        retVal = EnumMessageDialogResult.AFFIRMATIVE;
                        break;
                    case MessageDialogResult.Negative:
                        retVal = EnumMessageDialogResult.NEGATIVE;
                        break;
                    case MessageDialogResult.FirstAuxiliary:
                        retVal = EnumMessageDialogResult.FirstAuxiliary;
                        break;
                    case MessageDialogResult.SecondAuxiliary:
                        retVal = EnumMessageDialogResult.SecondAuxiliary;
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. {this.GetType().Name} - ShowDialog()");
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public async Task<EnumMessageDialogResult> ShowMessageDialogTarget(Window window, string Title, string Message, EnumMessageStyle enummessagesytel, string AffirmativeButtonText = "OK", string NegativeButtonText = "Cancel", string firstAuxiliaryButtonText = null, string secondAuxiliaryButtonText = null)
        {
            EnumMessageDialogResult retVal = EnumMessageDialogResult.UNDEFIND;

            try
            {
                if (MessageDialogShow != null && (this.StageCommunicationManager()?.IsEnableDialogProxy() == true))
                {
                    var gp_LC = this.LoaderController() as GP_LoaderController;
                    if (gp_LC != null)
                    {

                        string cellstr = gp_LC.ChuckID.ToString();
                        cellstr = cellstr.Replace("CHUCK", "CELL");

                        retVal = await MessageDialogShow($"{Title} in [{cellstr}]", Message, enummessagesytel);

                        return retVal;

                    }
                }

                Application.Current.Dispatcher.Invoke
                          (
                              () => TargetWindow = (window as MahApps.Metro.Controls.MetroWindow)
                          );
                MessageDialogResult dialogRet = MessageDialogResult.Negative;
                MessageDialogStyle msgStyle = MessageDialogStyle.Affirmative;

                MetroDialogSettings dlgSettings = null;
                switch (enummessagesytel)
                {
                    case EnumMessageStyle.Affirmative:
                        dlgSettings = new MetroDialogSettings()
                        {
                            ColorScheme = MetroDialogColorScheme.Inverted,
                            DefaultButtonFocus = MessageDialogResult.Affirmative,
                            DialogMessageFontSize = 22,
                            AffirmativeButtonText = "OK",
                            MaximumBodyHeight = 196,
                            AnimateShow = false,
                            AnimateHide = false
                        };
                        msgStyle = MessageDialogStyle.Affirmative;
                        break;
                    case EnumMessageStyle.AffirmativeAndNegative:
                        dlgSettings = new MetroDialogSettings()
                        {
                            ColorScheme = MetroDialogColorScheme.Inverted,
                            DefaultButtonFocus = MessageDialogResult.Affirmative,
                            DialogMessageFontSize = 24,
                            AffirmativeButtonText = "OK",
                            NegativeButtonText = "Cancel",
                            AnimateShow = false,
                            AnimateHide = false
                        };
                        msgStyle = MessageDialogStyle.AffirmativeAndNegative;
                        break;
                    case EnumMessageStyle.AffirmativeAndNegativeAndDoubleAuxiliary:
                        dlgSettings = new MetroDialogSettings()
                        {
                            ColorScheme = MetroDialogColorScheme.Inverted,
                            DefaultButtonFocus = MessageDialogResult.Affirmative,
                            DialogMessageFontSize = 24,
                            AffirmativeButtonText = AffirmativeButtonText,
                            NegativeButtonText = NegativeButtonText,
                            FirstAuxiliaryButtonText = firstAuxiliaryButtonText,
                            AnimateShow = false,
                            AnimateHide = false
                        };
                        msgStyle = MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary;

                        break;
                    case EnumMessageStyle.AffirmativeAndNegativeAndSingleAuxiliary:
                        dlgSettings = new MetroDialogSettings()
                        {
                            ColorScheme = MetroDialogColorScheme.Inverted,
                            DefaultButtonFocus = MessageDialogResult.Affirmative,
                            DialogMessageFontSize = 24,
                            AffirmativeButtonText = AffirmativeButtonText,
                            NegativeButtonText = NegativeButtonText,
                            FirstAuxiliaryButtonText = firstAuxiliaryButtonText,
                            SecondAuxiliaryButtonText = secondAuxiliaryButtonText,
                            AnimateShow = false,
                            AnimateHide = false
                        };
                        msgStyle = MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary;
                        break;
                }

                ///���α׷��� �� �߱����� MetroWindow �� ����ϸ� Expcetion �� �߻��ؼ� ���α׷��� �� �㶧���� ����ϱ�����.
                if (TargetWindow?.ActualHeight == 0)
                {
                    await Task.Run(() =>
                    {
                        while (true)
                        {
                            if (TargetWindow.ActualHeight != 0)
                            {
                                break;
                            }
                            Thread.Sleep(1);
                        }
                    });
                }

                dialogRet = await Application.Current.Dispatcher.Invoke<Task<MessageDialogResult>>(() =>
                {
                    Task<MessageDialogResult> dialogResult = null;
                    try
                    {
                        LoggerManager.Debug($"MessageDialog Show" + $" Title: {Title}" + $" Message: {Message}");
                        dialogResult = TargetWindow.ShowMessageAsync(Title, Message, msgStyle, dlgSettings);
                        TargetWindow.ResetStoredFocus();
                        LoggerManager.Debug($"MessageDialog Close");
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                    return dialogResult;
                });

                switch (dialogRet)
                {
                    case MessageDialogResult.Affirmative:
                        retVal = EnumMessageDialogResult.AFFIRMATIVE;
                        break;
                    case MessageDialogResult.Negative:
                        retVal = EnumMessageDialogResult.NEGATIVE;
                        break;
                    case MessageDialogResult.FirstAuxiliary:
                        retVal = EnumMessageDialogResult.FirstAuxiliary;
                        break;
                    case MessageDialogResult.SecondAuxiliary:
                        retVal = EnumMessageDialogResult.SecondAuxiliary;
                        break;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. {this.GetType().Name} - ShowDialog()");
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public async Task<EnumMessageDialogResult> ShowPasswordInputDialog(string Label = "Input", string posBtnLabel = "OK", string negBtnLabel = "Cancel")
        {
            EnumMessageDialogResult retval = EnumMessageDialogResult.NEGATIVE;

            try
            {
                // TODO : Password...
                // Stage���� Loader�� 

                CustomDialog dialog = PasswordInputDialogServiceProvider.GetDialog();
                PasswordInputDialogServiceProvider.SetData(Label, posBtnLabel, negBtnLabel, cmdPasswordInputAffirmativeButtonClick, cmdPasswordInputNegativeButtonClick);

                if (dialog != null)
                {
                    await this.ShowMetroWindow(dialog, true);
                    retval = PasswordInputDialogServiceProvider.GetDialogResult();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public string GetPasswordInputData()
        {
            string retval = string.Empty;

            try
            {
                retval = PasswordInputDialogServiceProvider.GetInputData();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public async Task ShowWindow(ContentControl window, bool waitunload = true)
        {
            try
            {
                await ShowMetroWindow(window, waitunload);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task ShowAdvancedDialog(ContentControl window, List<byte[]> data = null, bool waitunload = true)
        {
            try
            {
                if (window.IsVisible == false)
                {
                    if (MetroDialoShow != null)
                    {
                        var viewclassname = window.ToString().Split('.');

                        string[] viewmodelclassname = null;

                        IPnpAdvanceSetupViewModel vm = null;

                        MetroWindow.Dispatcher.Invoke(() =>
                        {
                            if (window.DataContext != null)
                            {
                                vm = window.DataContext as IPnpAdvanceSetupViewModel;
                            }
                        });

                        if (vm != null)
                        {
                            viewmodelclassname = vm.ToString().Split('.');

                            string viewAssemblyName = $"{System.Reflection.Assembly.GetAssembly(window.GetType()).GetName().Name}.dll";
                            string viewClassName = viewclassname[viewclassname.Length - 1];
                            string viewModelAssemblyName = $"{System.Reflection.Assembly.GetAssembly(vm.GetType()).GetName().Name}.dll";
                            string viewModelClassName = viewmodelclassname[viewmodelclassname.Length - 1];

                            if (vm is IElemMinMaxAdvanceSetupViewModel)
                            {
                                // return 하지 않고 아래쪽에 있는 ShowMetroWindow (Local 에서 Dialog 띄움.)
                            }
                            else
                            {
                                await MetroDialoShow(viewAssemblyName, viewClassName, viewModelAssemblyName, viewModelClassName, data, waitunload);
                                return;
                            }


                        }
                        else
                        {
                            return;
                        }
                    }

                    if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober)
                    {
                        MetroWindow.Dispatcher.Invoke(() =>
                        {
                            if (window.DataContext != null)
                            {
                                _ViewModel = window.DataContext as IPnpAdvanceSetupViewModel;
                            }
                        });
                    }

                    await ShowMetroWindow(window, waitunload);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public bool IsShowExistAdvance(string viewclassname, string viewmodelclassname)
        {
            bool retVal = false;
            try
            {
                if (OpenDialogs.Count != 0)
                {
                    foreach (var dialog in OpenDialogs)
                    {
                        bool isAdvanceViewModel = false;
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            try
                            {
                                if (dialog.dialog is IPnpAdvanceSetupView & dialog.dialog.DataContext is IPnpAdvanceSetupViewModel)
                                {
                                    string viewname = dialog.dialog.ToString().Split('.')?.Last();
                                    string viewmodelname = dialog.dialog.DataContext.ToString().Split('.')?.Last();
                                    if (viewname.Equals(viewclassname) & viewmodelname.Equals(viewmodelclassname))
                                    {
                                        isAdvanceViewModel = true;
                                    }
                                }
                            }
                            catch (Exception err)
                            {
                                LoggerManager.Exception(err);
                            }

                        });
                        retVal = isAdvanceViewModel;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void SetAdvanceDialogToPnpStep()
        {
            try
            {
                if (OpenDialogs.Count != 0)
                {
                    foreach (var dialog in OpenDialogs)
                    {
                        if (dialog.dialog is IPnpAdvanceSetupView)
                        {
                            (this.PnPManager().SeletedStep as ProberInterfaces.PnpSetup.IPnpSetup).SetAdvanceSetupView((IPnpAdvanceSetupView)dialog.dialog);
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (dialog.dialog.DataContext is IPnpAdvanceSetupViewModel)
                                {
                                    (this.PnPManager().SeletedStep as ProberInterfaces.PnpSetup.IPnpSetup).SetAdvanceSetupViewModel((IPnpAdvanceSetupViewModel)dialog.dialog.DataContext);
                                }
                            });
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task ShowWindow(string viewAssemblyName, string viewClassName, string viewModelAssemblyName, string viewModelClassName, List<byte[]> data, bool waitunload = true)
        {
            try
            {
                BaseMetroDialog metroDialog = null;
                IPnpAdvanceSetupViewModel viewmodel = null;

                DllImporter DLLImporter = new DllImporter();

                MetroWindow.Dispatcher.Invoke(() =>
                {
                    Tuple<bool, Assembly> viewmodule = DLLImporter.LoadDLL(new ModuleDllInfo(viewAssemblyName, viewClassName));

                    if (viewmodule != null && viewmodule.Item1)
                    {
                        var view = DLLImporter.Assignable<BaseMetroDialog>(viewmodule.Item2);
                        foreach (var module in view)
                        {
                            string name = module.GetType().Name;
                            if (viewClassName.Equals(name))
                            {
                                metroDialog = module;
                                break;
                            }
                        }
                    }

                    if (viewModelAssemblyName != null & viewModelClassName != null)
                    {
                        Tuple<bool, Assembly> viewmodelmodule = DLLImporter.LoadDLL(new ModuleDllInfo(viewModelAssemblyName, viewModelClassName));

                        if (viewmodelmodule != null && viewmodelmodule.Item1)
                        {
                            var view = DLLImporter.Assignable<IPnpAdvanceSetupViewModel>(viewmodelmodule.Item2);

                            foreach (var module in view)
                            {
                                string name = module.GetType().Name;
                                if (viewModelClassName.Equals(name))
                                {
                                    viewmodel = module;
                                    if (viewmodel is IModule)
                                    {
                                        (viewmodel as IModule).InitModule();
                                    }
                                    break;
                                }
                            }
                        }
                    }

                    if (viewmodel != null)
                    {
                        if (viewmodel is IPnpAdvanceSetupViewModel)
                        {
                            _ViewModel = (IPnpAdvanceSetupViewModel)viewmodel;
                            _ViewModel.SetParameters(data);

                            this.PnPManager().SetAdvancedViewModel(_ViewModel);
                        }

                        metroDialog.DataContext = (IPnpAdvanceSetupViewModel)viewmodel;
                    }
                });

                await ShowMetroWindow(metroDialog, waitunload, metroDialog.GetType().Name);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task ShowMetroWindow(ContentControl window, bool waitunload = true, string hashcode = null, string caller = "")
        {
            BaseMetroDialog dialog = window as BaseMetroDialog;

            if (dialog != null)
            {
                try
                {
                    if (MetroWindowLoaded == true)
                    {
                        BaseMetroDialogPack tmp = OpenDialogs.Where(x => x.dialog.GetType().Name == dialog.GetType().Name).FirstOrDefault();

                        if (tmp == null)
                        {
                            if (dialog.IsVisible == true)
                            {
                                AlreadyVisibleFlag = true;

                                return;
                            }

                            try
                            {
                                if (dialog.IsVisible == false)
                                {
                                    var dialogs = OpenDialogs.Where(x => x.dialog.GetType().Name == dialog.GetType().Name);
                                    bool dialogExist = false;
                                    foreach (var item in dialogs)
                                    {
                                        if (item.dialog.IsVisible == true)
                                        {
                                            dialogExist = true;
                                            break;
                                        }
                                    }
                                    if (dialogExist == false)
                                    {
                                        CancellationTokenSource tokenSource = new CancellationTokenSource(1000);
                                        CancellationToken token = tokenSource.Token;
                                        LastOpendDailog = dialog;
                                        var showTask = Application.Current.Dispatcher.Invoke<Task>(() =>
                                        {
                                            return MetroWindow.ShowMetroDialogAsync(dialog, new MetroDialogSettings() { CancellationToken = token, AnimateShow = false, AnimateHide = false });

                                        });

                                        LoggerManager.Debug($"ShowMetroDialogAsync(): Task ID = {showTask.Id}");
                                        if (await Task.WhenAny(showTask, Task.Delay(1500, token)).ConfigureAwait(false) == showTask)
                                        {
                                            LoggerManager.Debug($"ShowMetroDialogAsync(Hash = {hashcode}): Task(ID = {showTask.Id}) DONE.");
                                        }
                                        else
                                        {
                                            LoggerManager.Debug($"ShowMetroDialogAsync(Hash = {hashcode}): Task(ID = {showTask.Id}) timeout occurred.");
                                        }
                                    }
                                    BaseMetroDialogPack pack = new BaseMetroDialogPack();
                                    pack.dialog = dialog;
                                    pack.HashCode = hashcode;
                                    OpenDialogs.Add(pack);
                                }
                            }
                            catch (Exception err)
                            {
                                LoggerManager.Exception(err, $"MetroWindow.ShowMetroDialogAsync(Hash = {hashcode})");
                            }

                            if (waitunload)
                            {
                                await dialog.WaitUntilUnloadedAsync();
                            }
                        }
                        else
                        {

                        }
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
        }
        public async Task CloseWindow(string classname)
        {
            try
            {
                BaseMetroDialogPack pack = null;


                if (classname != null || classname != string.Empty)
                {
                    pack = OpenDialogs.Where(x => x.dialog.GetType().Name == classname).FirstOrDefault();

                    if (pack != null)
                    {
                        await CloseWindow(pack.dialog);
                    }
                }
                else
                {
                    BaseMetroDialog dialog = null;

                    dialog = OpenDialogs.Last()?.dialog;

                    if (dialog != null)
                    {
                        await CloseWindow(dialog);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task ShowWaitWindow(ContentControl window, string message, string hashcode = null, CancellationTokenSource canceltokensource = null, string cancelButtonText = "Cancel")
        {
            BaseMetroDialog dialog = window as BaseMetroDialog;
            if (dialog != null)
            {
                //await MetroWindow.Dispatcher.Invoke(
                //(
                //    async () =>
                //    {
                await waitDialogSemaphore.WaitAsync(5000);
                try
                {
                    if (MetroWindowLoaded == true)
                    {
                        BaseMetroDialogPack tmp = OpenDialogs.Where(
                            x => x.dialog.GetType().Name == dialog.GetType().Name).FirstOrDefault();
                        if (tmp == null)
                        {

                            try
                            {
                                if (dialog.IsVisible == false)
                                {
                                    var ctSource = new CancellationTokenSource();
                                    ctSource.CancelAfter(2000);
                                    CancellationToken token = ctSource.Token;

                                    var showTask = Application.Current.Dispatcher.Invoke<Task>(() =>
                                        {
                                            WaitCancelDialogServiceProvider?.SetData(message, hashcode, canceltokensource, cancelButtonText);
                                            return MetroWindow.ShowMetroDialogAsync(dialog, new MetroDialogSettings() { CancellationToken = token, AnimateShow = false, AnimateHide = false });
                                        });

                                    if (await Task.WhenAny(showTask, Task.Delay(1500, token)) == showTask)
                                    {
                                        BaseMetroDialogPack pack = new BaseMetroDialogPack();
                                        pack.dialog = dialog;
                                        pack.HashCode = hashcode;
                                        OpenDialogs.Add(pack);
                                        AddWaitDialog(pack);
                                        LoggerManager.Debug($"ShowWaitWindow() : Hash = {hashcode} Dialog opened");
                                    }
                                    else
                                    {
                                        BaseMetroDialogPack pack = new BaseMetroDialogPack();
                                        pack.dialog = dialog;
                                        pack.HashCode = hashcode;
                                        OpenDialogs.Add(pack);
                                        AddWaitDialog(pack);
                                        showTask.Wait(1000);
                                        LoggerManager.Debug($"ShowWaitWindow() : Hash = {hashcode}, timeout occurred. Task state = {showTask.Status}");
                                    }
                                }
                                else
                                {
                                    LoggerManager.Debug($"ShowWaitWindow() : Hash = {hashcode}, Dialog already visible.");
                                }
                            }
                            catch (Exception err)
                            {
                                LoggerManager.Exception(err, $"ShowWaitWindow() : Hash = {hashcode}");
                            }
                        }
                        else
                        {
                            BaseMetroDialogPack pack = new BaseMetroDialogPack();
                            pack.dialog = null;
                            pack.HashCode = hashcode;
                            AddWaitDialog(pack);
                            LoggerManager.Debug($"ShowWaitWindow(): Hash = {tmp.HashCode}, Dialog already exist.");
                        }
                    }
                    else
                    {
                        BaseMetroDialogPack pack = new BaseMetroDialogPack();
                        pack.HashCode = hashcode;
                        AddWaitDialog(pack);
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                finally
                {
                    waitDialogSemaphore.Release();
                }
                //}));
            }
        }
        public async Task CloseWaitWindow(ContentControl window, string hashcode = null, bool localonly = false)
        {
            try
            {
                if (MetroDialogClose != null && localonly == false)
                {
                    // Cell이라면 로더쪽으로 Close Trigger 호출 시킴. (local only true라면 호출 불필요 함.)
                    // 현재 AutoSoaking 쪽에서만 localonly true로 사용하고 있음.
                    var classname = window.ToString().Split('.');
                    await MetroDialogClose(classname[classname.Length - 1]);
                }

                BaseMetroDialog dialog = window as BaseMetroDialog;

                if (dialog != null)
                {
                    await waitDialogSemaphore.WaitAsync(10000);
                    try
                    {
                        bool IsOpened = false;
                        BaseMetroDialogPack pack = OpenDialogs.Where(x => x.dialog.GetType().Name == dialog.GetType().Name).FirstOrDefault();
                        bool forced = false;
                        if (hashcode == string.Empty || hashcode == null)
                        {
                            IsOpened = true;
                            forced = true;
                        }
                        else
                        {
                            if (pack != null)
                            {
                                if (pack.HashCode == hashcode)
                                {
                                    IsOpened = true;
                                }
                                else
                                {
                                    LoggerManager.Debug($"CloseWaitWindow(): Dialog's hash = {pack.HashCode}, Close target hash = {hashcode}.");
                                    IsOpened = true;
                                }
                                if (pack.HashCode == null)
                                {
                                    IsOpened = true;
                                }
                            }
                        }

                        if (AlreadyVisibleFlag == true)
                        {
                            IsOpened = true;
                            forced = true;

                            AlreadyVisibleFlag = false;
                        }
                        if (IsOpened == true)
                        {
                            var packs = OpenDialogs.Where(x => x.dialog.GetType().Name == dialog.GetType().Name);
                            if (forced == true)
                            {

                                try
                                {
                                    if (dialog.IsVisible == true)
                                    {
                                        if (packs.Count() < 2)
                                        {
                                            await Application.Current.Dispatcher.Invoke<Task>(() =>
                                            {
                                                WaitCancelDialogServiceProvider?.ClearData();
                                                return MetroWindow.HideMetroDialogAsync(dialog, new MetroDialogSettings() { AnimateShow = false, AnimateHide = false });
                                            });
                                        }
                                    }

                                    WaitDialogs.Clear();

                                    if (pack != null)
                                    {
                                        OpenDialogs.Remove(pack);
                                    }

                                }
                                catch (Exception err)
                                {
                                    LoggerManager.Exception(err, $"MetroWindow.HideMetroDialogAsync()");
                                }
                                finally
                                {
                                }
                            }
                            else
                            {
                                try
                                {
                                    if (hashcode == null)
                                    {
                                        WaitDialogs.Clear();
                                    }
                                    RemoveWaitDialog(hashcode);

                                    if (pack.dialog.IsVisible == true & WaitDialogs.Count == 0)
                                    {
                                        await Application.Current.Dispatcher.Invoke<Task>(() =>
                                        {
                                            WaitCancelDialogServiceProvider?.ClearData();
                                            return MetroWindow.HideMetroDialogAsync(pack.dialog, new MetroDialogSettings() { AnimateShow = false, AnimateHide = false });
                                        });

                                        OpenDialogs.Remove(pack);
                                    }
                                    else
                                    {
                                        if (pack.dialog.IsVisible == false)
                                        {
                                            if (WaitDialogs.Count == 0)
                                            {
                                                LoggerManager.Debug($"CloseWaitWindow(): WaitDialogs list empty but OpenDialogs still exist and is in hide state. Hash = {pack.HashCode}");
                                            }
                                        }
                                    }
                                }
                                catch (Exception err)
                                {
                                    LoggerManager.Exception(err, $"MetroWindow.HideMetroDialogAsync()");
                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                RemoveWaitDialog(hashcode);
                            }
                            catch (Exception err)
                            {
                                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                    finally
                    {
                        waitDialogSemaphore.Release();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task ShowWaitWindowTarget(ContentControl window, string message, string hashcode = null, CancellationTokenSource canceltokensource = null, string cancelButtonText = "Cancel")
        {
            BaseMetroDialog dialog = window as BaseMetroDialog;

            if (dialog != null)
            {
                await waitDialogSemaphore.WaitAsync(5000);
                try
                {
                    try
                    {
                        if (dialog.IsVisible == false)
                        {
                            var ctSource = new CancellationTokenSource();
                            ctSource.CancelAfter(2000);
                            CancellationToken token = ctSource.Token;

                            var showTask = Application.Current.Dispatcher.Invoke<Task>(() =>
                            {
                                TargetWaitCancelDialogServiceProvider?.SetData(message, hashcode, canceltokensource);
                                return TargetWindow.ShowMetroDialogAsync(dialog, new MetroDialogSettings() { CancellationToken = token, AnimateShow = false, AnimateHide = false });
                            });

                            LoggerManager.Debug($"ShowMetroDialogAsync(): Task ID = {showTask.Id}, Hash = {hashcode}");

                        }
                        else
                        {
                            LoggerManager.Debug($"ShowMetroDialogAsync(Hash = {hashcode}): Dialog already visible.");
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err, $"MetroWindow.ShowMetroDialogAsync(Hash = {hashcode})");
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                finally
                {
                    waitDialogSemaphore.Release();
                }
            }
        }
        public async Task CloseWaitWindowTarget(ContentControl window, string hashcode = null)
        {
            await waitDialogSemaphore.WaitAsync(1000);
            try
            {

                BaseMetroDialog dialog = window as BaseMetroDialog;

                await Application.Current.Dispatcher.Invoke<Task>(() =>
                {
                    TargetWaitCancelDialogServiceProvider?.ClearData();
                    return TargetWindow.HideMetroDialogAsync(dialog, new MetroDialogSettings() { AnimateShow = false, AnimateHide = false });
                });

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err, $"MetroWindow.HideMetroDialogAsync()");
            }
        }
        private bool AddWaitDialog(BaseMetroDialogPack pack)
        {
            bool retVal = false;
            try
            {
                lock (waitDialogLockObject)
                {
                    var dialog = WaitDialogs.Where(d => d.HashCode == pack.HashCode).FirstOrDefault();

                    if (dialog == null)
                    {
                        WaitDialogs.Add(pack);
                        retVal = true;
                    }
                    else
                    {
                        LoggerManager.Debug($"AddWaitDialog(): Dialog already exist. Hash = {pack.HashCode}");
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            finally
            {

            }
            return retVal;
        }
        private bool RemoveWaitDialog(string hash)
        {
            bool retVal = false;
            try
            {
                lock (waitDialogLockObject)
                {
                    var matchedDialog = WaitDialogs.Where(x => x.HashCode == hash).FirstOrDefault();
                    if (matchedDialog != null)
                    {
                        WaitDialogs.Remove(matchedDialog);
                        retVal = true;
                    }
                    else
                    {
                        LoggerManager.Debug($"RemoveWaitDialog(): Such dialog does not exist on the list. Hash = {hash}");
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            finally
            {

            }
            return retVal;
        }
        public async Task CloseWindow(ContentControl window, string hashcode = null)
        {
            try
            {
                if (MetroDialogClose != null)
                {
                    // TODO : �δ��ʿ��� Show�� �߻��� ���̾�α׿� ���ؼ��� �ݱ� ������ �̷������ �Ѵ�.
                    var classname = window.ToString().Split('.');
                    await MetroDialogClose(classname[classname.Length - 1]);
                }

                BaseMetroDialog dialog = window as BaseMetroDialog;

                if (dialog != null)
                {
                    try
                    {
                        bool IsOpened = false;

                        // TODO : HashCode�� ���� Dialog�� ã�Ƽ� ������.
                        // HashCode�� �������� �ʴ´ٸ�, ���� ȣ��� ������ ������� Dialog�� �ƴϴ�.
                        // ������ Ÿ���� ���̾�α׸� �ߺ����� ȣ���� �� ���� ������ (Show x 2)

                        BaseMetroDialogPack pack = OpenDialogs.Where(x => x.dialog.GetType().Name == dialog.GetType().Name).FirstOrDefault();

                        bool forced = false;

                        if (hashcode == string.Empty || hashcode == null)
                        {
                            IsOpened = true;
                            forced = true;
                        }
                        else
                        {
                            if (pack != null)
                            {
                                if (pack.HashCode == hashcode)
                                {
                                    IsOpened = true;
                                }

                                if (pack.HashCode == null)
                                {
                                    IsOpened = true;
                                }
                            }
                        }

                        if (AlreadyVisibleFlag == true)
                        {
                            IsOpened = true;
                            forced = true;

                            AlreadyVisibleFlag = false;
                        }

                        if (IsOpened == true)
                        {

                            var packs = OpenDialogs.Where(x => x.dialog.GetType().Name == dialog.GetType().Name);

                            if (forced == true)
                            {
                                try
                                {
                                    if (dialog.IsVisible == true)
                                    {
                                        if (packs.Count() < 2)
                                        {
                                            var task = Application.Current.Dispatcher.Invoke<Task>(() =>
                                            {
                                                return MetroWindow.HideMetroDialogAsync(dialog, new MetroDialogSettings() { AnimateShow = false, AnimateHide = false });
                                            });
                                            await task;
                                        }
                                    }
                                    if (pack != null)
                                    {
                                        OpenDialogs.Remove(pack);
                                    }
                                }
                                catch (Exception err)
                                {
                                    LoggerManager.Exception(err, $"MetroWindow.HideMetroDialogAsync()");
                                }
                            }
                            else
                            {
                                try
                                {
                                    if (pack.dialog.IsVisible == true)
                                    {
                                        if (packs.Count() < 2)
                                        {
                                            var task = Application.Current.Dispatcher.Invoke<Task>(() =>
                                            {
                                                return MetroWindow.HideMetroDialogAsync(pack.dialog);
                                            });
                                            await task;

                                            OpenDialogs.Remove(pack);
                                        }
                                        else
                                        {
                                            if (packs.Count(x => x.HashCode == pack.HashCode) > 1)
                                            {
                                                LoggerManager.Debug($"CloseWindow(): Duplicated dialog detected. Hash = {hashcode}.");
                                                var task = Application.Current.Dispatcher.Invoke<Task>(() =>
                                                {
                                                    return MetroWindow.HideMetroDialogAsync(pack.dialog);
                                                });
                                                await task;
                                                OpenDialogs.Remove(pack);
                                            }
                                            else
                                            {
                                                LoggerManager.Debug($"CloseWindow(): Hooked dialog count = {packs.Count()}. Remove hash {pack.HashCode} dialog.");
                                                OpenDialogs.Remove(pack);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        OpenDialogs.Remove(pack);
                                    }
                                }
                                catch (Exception err)
                                {
                                    LoggerManager.Exception(err, $"MetroWindow.HideMetroDialogAsync()");
                                }
                            }
                        }
                        else
                        {
                            if (pack != null)
                            {
                                var packs = OpenDialogs.Where(x => x.dialog.GetType().Name == dialog.GetType().Name);

                                try
                                {
                                    if (packs.Count() == 1)
                                    {
                                        var task = Application.Current.Dispatcher.Invoke<Task>(() =>
                                        {
                                            return MetroWindow.HideMetroDialogAsync(packs.FirstOrDefault().dialog, new MetroDialogSettings() { AnimateShow = false, AnimateHide = false });
                                        });
                                        await task;
                                    }
                                    OpenDialogs.Remove(pack);
                                }
                                catch (Exception err)
                                {
                                    LoggerManager.Exception(err, $"MetroWindow.HideMetroDialogAsync()");
                                }
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public int GetWaitingThreads()
        {
            return waitingThreads;
        }
        public async Task CloseWindow()
        {
            try
            {
                if (LastOpendDailog != null)
                {
                    await Application.Current.Dispatcher.Invoke<Task>(() =>
                    {
                        return MetroWindow.HideMetroDialogAsync(LastOpendDailog, new MetroDialogSettings() { AnimateShow = false, AnimateHide = false });
                    });
                    OpenDialogs.Clear();
                    LastOpendDailog = null;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}