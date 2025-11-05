using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProberInterfaces
{
    using ProberErrorCode;
    using System.ServiceModel;
    using System.Threading;
    using MetroDialogInterfaces;
    using System.Windows.Input;

    [ServiceContract(CallbackContract = typeof(IDelegateEventHostCallback), SessionMode = SessionMode.Required)]
    public interface IDelegateEventHost
    {
        //event SoakgDialogShowEventHandler SoakDialogShowEvent;
        //event SoakDialogCloseEventHandler SoakDialogCloseEvent;

        event MessageDialogShowEventHandler MessageDialogShow;
        event MetroDialogShowEventHandler MetroDialogShowEvent;
        event MetroDialogCloseEventHandler MetroDialogCloseEvent;

        //event MetroDialogShowChuckIndexEventHandler MetroDialogShowChuckIndexEvent;
        //event MetroDialogCloseChuckIndexEventHandler MetroDialogCloseChuckIndexEvent;

        //event ProgressDialogShowEventHandler ProgressDialogShow;
        //event ProgressDialogCloseEventHandler ProgressDialogClose;
        //event ProgressDialogCloseAllEventHandler ProgressDialogCloseAll;

        event SingleInputDialogShowEventHandler SingleInputDialogShow;
        event SingleInputGetInputDataEventHandler SingleInputDialogGetInputData;


        [OperationContract]
        void InitService(int chuckId);
        [OperationContract]
        bool IsServiceAvailable();

        [OperationContract]
        Task<EnumMessageDialogResult> ShowMessageDialog(string Title, string Message, EnumMessageStyle enummessagesytel, string AffirmativeButtonText = "OK", string NegativeButtonText = "Cancel", string firstAuxiliaryButtonText = null, string secondAuxiliaryButtonText = null);
        [OperationContract]
        Task ShowMetroDialog(string viewAssemblyName, string viewClassName, string viewModelAssemblyName, string viewModelClassName, List<byte[]> data = null, bool waitunload = true);
        [OperationContract]
        Task CloseMetroDialog(string classname = null);
        //[OperationContract]
        //Task ShowMetroDialogChuckIndex(string viewAssemblyName, string viewClassName, string viewModelAssemblyName, string viewModelClassName, int chuckindex=-1,List<byte[]> data = null, bool waitunload = true);
        //[OperationContract]
        //Task CloseMetroDialogChuckIndex(string classname, int chuckinex = -1);

        //[OperationContract]
        //Task<EventCodeEnum> ShowProgressDialog(
        //  string title, string message, object callerassembly = null,
        //    CancellationTokenSource cancelts = null,bool issetprogress = false, bool visibilitycancel = true);
        //[OperationContract]
        //Task<EventCodeEnum> CloseProgressDialog(object callerassembly = null);

        [OperationContract]
        Task ShowWaitCancelDialog(string hashcode, string message, CancellationTokenSource canceltokensource = null, string caller = "", string cancelButtonText = "Cancel");
        [OperationContract]
        Task CloseWaitCancelDialog(string hashcode);
        [OperationContract]
        Task SetDataWaitCancelDialog(string message, string hashcoe, CancellationTokenSource canceltokensource = null, string cancelButtonText = "Cancel", bool restarttimer = false);
        [OperationContract]
        Task ClearDataWaitCancelDialog(bool restarttimer = false);
        //[OperationContract]
        //Task CloseAllProgressDialog();
        [OperationContract]
        Task<EnumMessageDialogResult> ShowSingleInputDialog(string Label = "Input", string posBtnLabel = "OK", string negBtnLabel = "Cancel", string subLabel = "");
        [OperationContract]
        string GetInputDataSingleInput();
        
        //[OperationContract]
        //Task<bool> ShowSoakDialog(string Title, string Message);
        //[OperationContract]
        //Task CloseSoakDialog();

        [OperationContract]
        EventCodeEnum Disconnect(int index = -1);
        IDelegateEventHostCallback GetDialogHostClient(int index = -1);

    }
    public interface IDialogServiceProxy
    {
        Task ShowWaitCancelDialog(string hashcode, string message, CancellationTokenSource canceltokensource = null, string caller = "", string cancelButtonText = "Cancel");
        Task CloseWaitCancelDialog(string hashcode);
        Task SetDataWaitCancelDialog(string message, string hashcoe, CancellationTokenSource canceltokensource = null, string cancelButtonText = "Cancel", bool restarttimer = false);
        Task ClearDataWaitCancelDialog(bool restarttimer = false);
        bool IsServiceAvailable();
    }
    public interface IDelegateEventHostCallback
    {
        [OperationContract(IsOneWay = true)]
        void DisConnect();

        //[OperationContract(IsOneWay = true)]
        //void ProgressDialogCancel();

        [OperationContract(IsOneWay = true)]
        void WaitCancelDialogCancelEvent();
    }

}
