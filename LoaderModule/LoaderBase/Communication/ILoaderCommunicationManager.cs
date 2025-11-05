using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LoaderBase.Communication
{
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Proxies;
    using ServiceInterfaces;
    using System.Collections.ObjectModel;
    using System.ServiceModel;

    public delegate void PWVMSetParamDelegate();
    public delegate void ChangeSelectedStageHandler();

    [ServiceContract]
    public interface ILoaderCommunicationManagerCallback
    {
        [OperationContract]
        void LoaderExit();
    

    }

    [ServiceContract(CallbackContract = typeof(ILoaderCommunicationManagerCallback))]
    public interface ILoaderCommunicationManager : IModule
    {

        event ChangeSelectedStageHandler ChangeSelectedStageEvent;
        int SelectedStageIndex { get; set; }
        ILauncherObject SelectedLauncher { get; set; }
        IStageObject SelectedStage { get; set; }
        IParam LoaderCommunicationParam { get; }
        EventCodeEnum LoadParameter();
        EventCodeEnum SaveParameter();
        ObservableCollection<ILauncherObject> GetMultiLaunchers();
        IImageDispHost GetDispHostService();
        IDelegateEventHost GetDelegateEventService();
        bool AbortStage(int index, bool delAllProxies = true);
        bool IsAliveStageSupervisor(int nCellIndex);
        Task<bool> ConnectStage(int index, bool skipMsgDialog = false);
        EventCodeEnum ConnectLauncher(int index);
        EventCodeEnum ConnectLaunchers();

        void StartProberSystem(List<int> indexlist);

        void ExitProberSystem(int cellindex = -1);
        string GetStageDeviceName(int index);

        ProbeAxisObject GetAxis(EnumAxisConstants axis);
        void SetAxesStateUpdateTime(double time);
        void SetWaferMapCamera(EnumProberCam cam);

        //IParam PolishWaferParam { get; set; }
        //PWVMSetParamDelegate PWVMSetParamDelegate { get; set; }
        IStageObject GetStage(int index = -1);
        ObservableCollection<IStageObject> GetStages();
       

        //bool CheckAvailabilityComm(string ip, int port);
        Tuple<string, int> GetStageIPPortInfo(int index);
        Tuple<string, int> GetLuncherIPPortInfo(int index);
        bool DisConnectStage(int index = -1);
        void ConnectLoaderClient(int index = -1);
        ObservableCollection<IStageObject> Cells { get; }

        //ObservableCollection<IStagelotSetting> Cells_ForLot { get; }  

        //T GetClientProxy<T>(int index = -1);

        //void UpdateSelectedStage(bool ConnectedTrigger = false);
        //Task GetWaferObject(IStageObject stage);
        //void GetProbeCardObject(IStageObject stage);
        //void GetMarkObject(IStageObject stage);
        Task<bool> SetStageMode(GPCellModeEnum stagemode, StreamingModeEnum streamingmode, bool showdialogflag = true, int stageindex = -1, bool updateToStage = true, bool Pass_ToCheckSetStageMode = false);
        void DisconnectProxyOnline(int stageindex);

        T GetProxy<T>(int index = -1) where T : IProberProxy;
        T InitProxy<T>(int index = -1) where T : IProberProxy;

        //ISoakingModuleProxy GetSoakingModuleClient(int inex = -1);
        //IRemoteMediumProxy GetRemoteMediumClient(int index = -1);  // #Hynix_Merge: 삭제하고 GetClient로 바꾸는 방향으로 진행할 것.
        //IStageSupervisorProxy GetStageSupervisorClient(int index = -1);
        //IStageMoveProxy GetStageMoveClient(int index = -1);
        //ITempControllerProxy GetTempControllerClient(int index = -1);
        //IPolishWaferModuleProxy GetPolishWaferModuleClient(int index = -1);

        //IRetestModuleProxy GetRetestModuleClient(int index = -1);

        //IPinAlignerProxy GetPinAlignerModuleClient(int index = -1);

        //IFileManagerProxy GetFileManagerClient(int index = -1);

        //ILotOPModuleProxy GetLotOPModuleClient(int index = -1);
        //IPMIModuleProxy GetPMIModuleClient(int index = -1);
        //ICoordinateManagerProxy GetCoordManagerClient(int index = -1);
        //IParamManagerProxy GetParamManagerClient(int index = -1);
        //IWaferAlignerProxy GetWaferAlignerClient(int index = -1);
        //IMotionAxisProxy GetMotionAxisClient(int index = -1);

        Task WaitStageJob();
        void SetStageWorkingFlag(bool flag);
        void SetLoaderWorkingFlag(bool flag);

        void UpdateStageAlarmCount();

        void SetTitleMessage(int Cellindex, string message, string foreground = "", string background = "");

        void SetDeviceName(int Cellindex, string deviceName);
        void SetDeviceLoadResult(int cellno, bool result);

        Task<EventCodeEnum> DeviceReload(IStageObject stage, bool forced = false);
        Task UpdateMapIndex(object newVal);

        byte[] GetBytesFoupObjects();

        EventCodeEnum SetRecoveryMode(int cellIdx, bool isRecovery);

        bool Can_I_ChangeMaintenanceModeInStatusSoaking(int Cell_Idx);


        Task DiskAlarm(int lunchernum, string pc_name, string drivename);

        void DisConnectLauncher(int lunchernum);

        //void SetDiskInfo(int index);

        [OperationContract]
        void GetDiskInfo(int index);

        [OperationContract]
        Task LoaderInitService(int cellnum);

        [OperationContract]
        void SetDiskInfo(int lunchernum, string pc_name, string drivename, string usagespace, string availablespace, string totalspace, string percent);

        [OperationContract]
        int FindLuncherIndex(int cellnum);

        void SetCellModeChanging(int Cell_Idx);
        void ResetCellModeChanging(int Cell_Idx);
    }

}
