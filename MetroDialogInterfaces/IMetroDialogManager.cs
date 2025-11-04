using System.Collections.Generic;
using System.Threading.Tasks;

namespace MetroDialogInterfaces
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    public delegate Task MetroDialogShowEventHandler(
        string viewAssemblyName, string viewClassName, string viewModelAssemblyName, string viewModelClassName, List<byte[]> data = null, bool waitunload = true);
    public delegate Task MetroDialogShowChuckIndexEventHandler(
        string viewAssemblyName, string viewClassName, string viewModelAssemblyName, string viewModelClassName, int chuckindex = -1, List<byte[]> data = null, bool waitunload = false);

    public delegate Task MetroDialogCloseEventHandler(string classname = null);
    //public delegate Task MetroDialogCloseChuckIndexEventHandler(string classname, int chuckinex = -1);
    public delegate Task MetroDialogCloseChuckIndexEventHandler(string classname = null, int chuckinex = -1);

    public delegate Task<EnumMessageDialogResult> MessageDialogShowEventHandler(string Title, string Message, EnumMessageStyle enummessagesytel, string AffirmativeButtonText = "OK", string NegativeButtonText = "Cancel", string firstAuxiliaryButtonText = null, string secondAuxiliaryButtonText = null);

    public interface IMetroDialogManager
    { 
        // EVENT
        event MetroDialogShowEventHandler MetroDialoShow;
        event MetroDialogCloseEventHandler MetroDialogClose;

        event MessageDialogShowEventHandler MessageDialogShow;

        event SingleInputDialogShowEventHandler SingleInputDialogShow;
        event SingleInputGetInputDataEventHandler SingleInputGetInputData;

        //event MetroDialogShowChuckIndexEventHandler MetroDialogShowChuckIndex;
        //event MetroDialogCloseChuckIndexEventHandler MetroDialogCloseChuckIndex;

        Window GetMetroWindow(bool doNotCreate = false);

        // SHOW
        Task<EnumMessageDialogResult> ShowMessageDialogTarget(Window window, string Title, string Message, EnumMessageStyle enummessagesytel, string AffirmativeButtonText = "OK", string NegativeButtonText = "Cancel", string firstAuxiliaryButtonText = null, string secondAuxiliaryButtonText = null);
        Task<EnumMessageDialogResult> ShowMessageDialog(string Title, string Message, EnumMessageStyle enummessagesytel, string AffirmativeButtonText = "OK", string NegativeButtonText = "Cancel", string firstAuxiliaryButtonText = null, string secondAuxiliaryButtonText = null);
        Task<EnumMessageDialogResult> ShowSingleInputDialog(string Label = "Input", string posBtnLabel = "OK", string negBtnLabel = "Cancel", string subLabel = "");

        Task ShowWaitCancelDialogTarget(Window window, string hashcode, string message, CancellationTokenSource canceltokensource = null, string caller = "");
        Task ShowWaitCancelDialog(string hashcode, string message, CancellationTokenSource canceltokensource = null, string caller = "", bool localonly = false, string cancelButtonText = "Cancel", Action<object> action = null, object actionObject = null, bool keepDialogWhenCancelButtonClick = false);

        Task<EnumMessageDialogResult> ShowPasswordInputDialog(string Label = "Input", string posBtnLabel = "OK", string negBtnLabel = "Cancel");

        // Advanced View
        Task ShowAdvancedDialog(ContentControl window, List<byte[]> data = null, bool waitunload = true);

        Task ShowWindow(ContentControl window, bool waitunload = true);

        Task ShowWindow(string viewAssemblyName, string viewClassName, string viewModelAssemblyName, string viewModelClassName, List<byte[]> data = null, bool waitunload = true);

        // CLOSE

        //Task CloseWaitCancelDialaog(string hashcode);

        Task CloseWaitCancelDialaog(string hashcode, bool localonly = false);
        //Task CloseWaitCancelDialaog(string hashcode);

        Task CloseWaitCancelDialaogTarget(Window window, string hashcode);
        Task CloseWindow(ContentControl window, string hashcode = null);
        Task CloseWindow(string classname);
        Task CloseWindow();

        // ETC

        void SetMetroWindowLoaded(bool flag);

        string GetSingleInputData();
        string GetPasswordInputData();

        //Task WaitAsyncSemaphore();
        //void ReleaseSemaphore();

        int DialogCount { get; }
        int GetWaitingThreads();

        bool IsShowingWaitCancelDialog();
        bool IsShowExistAdvance(string viewclassname, string viewmodelclassname);
        void SetAdvanceDialogToPnpStep();

        bool MetroWindowLoaded { get; }

        Task SetDataWaitCancelDialog(string message, string hashcoe, CancellationTokenSource canceltokensource = null, string cancelButtonText = "Cancel", bool restarttimer = false);

        Task ClearDataWaitCancelDialog(bool restarttimer = false);
        void SetMessageToWaitCancelDialog(string message);
        void HiddenCancelButtonOfWaitCancelDiaglog([CallerMemberName] string callFuncNm = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int sourceLineNumber = 0);
        void ReshowCancelButtonOfWaitCancelDiaglog([CallerMemberName] string callFuncNm = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int sourceLineNumber = 0);
        
        void ShowCancelButtonOfWaitCancelDiaglog(CancellationTokenSource canceltokensource, string cancelButtonText, Action<object> cancelAction, object cancelActionObject, bool keepDialogWhenCancelButtonClick, [CallerMemberName] string callFuncNm = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int sourceLineNumber = 0);
    }
}
