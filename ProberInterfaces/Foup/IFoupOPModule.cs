using LogModule;

using ProberErrorCode;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using Newtonsoft.Json;
using ProberInterfaces.Communication.RFID;
using ProberInterfaces.CassetteIDReader;
using ProberInterfaces.Communication.BarcodeReader;
using System.Threading;
using System.Threading.Tasks;

namespace ProberInterfaces.Foup
{
    [Serializable]
    public abstract class FoupBehavior : IFoupBehavior
    {
        public FoupBehavior()
        {

        }
        [XmlIgnore, JsonIgnore]
        public IFoupModule FoupModule { get; set; }
        [XmlIgnore, JsonIgnore]
        public IFoupIOStates FoupIOManager { get; set; }
        [XmlIgnore, JsonIgnore]
        public FoupBehaviorStateEnum State { get; set; }

        [XmlIgnore, JsonIgnore]
        public ObservableCollection<IOPortDescripter<bool>> Inputs { get; set; }

        [XmlIgnore, JsonIgnore]
        public ObservableCollection<IOPortDescripter<bool>> RequiredInputs { get; set; }

        [XmlIgnore, JsonIgnore]
        public ObservableCollection<IOPortDescripter<bool>> Outputs { get; set; }

        public abstract EventCodeEnum Run();

        public void StateTransition(EventCodeEnum errorcode)
        {
            try
            {
                if (errorcode == EventCodeEnum.NONE)
                {
                    State = FoupBehaviorStateEnum.Done;
                }
                else
                {
                    //State = FoupBehaviorStateEnum.IDLE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public abstract EventCodeEnum InitBehavior();
        public abstract EventCodeEnum CheckIOState();
        public abstract EventCodeEnum SetRequiredIO();
        public int CompareOutput(string outputAlias, bool io)
        {
            int result = -1;
            try
            {
                foreach (var output in Outputs)
                {
                    if (output.Alias.Value == outputAlias)
                    {
                        result = output.MonitorForIO(io);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return result;
        }
    }

    public interface IFoupBehavior : IFactoryModule
    {
        IFoupIOStates FoupIOManager { get; set; }
        IFoupModule FoupModule { get; set; }
        EventCodeEnum Run();
    }

    public interface IFoupProcedure
    {
        string Caption { get; set; }
        FoupBehavior Behavior { get; set; }

    }
    public interface IFoupProcedureStateMap
    {
    }

    public interface IFoupProcedureStateMaps : IEnumerable<IFoupProcedureStateMap>
    {

    }
    public interface IFoupProcedureManager : IFoupProcRunManager
    {
        IFoupProcedureStateMaps SelectedProcedureStateMaps { get; set; }
        void SettingProcedure(List<IFoupProcedure> foupProcedures
                                    , List<string> LoadOrderList
                                    , List<string> UnloadOrderList
                                    , IFoupModule FoupModule
                                    , IFoupIOStates FoupIOManager);
        int SetSequenceTab(IFoupProcedure curprocedure);
        IFoupProcedure GetSelectedProcedureStateMapNode();
        int GetSelectedProcedureIndex();
        IFoupProcedureStateMaps GetProcedures(FoupStateEnum foupstate);
        EventCodeEnum InitSelectedProcedureStateMapNode(FoupStateEnum setfoupstate);
        EventCodeEnum SetPrevSelectedProcedureStateMapNode();
        //EventCodeEnum SetPrevSelectedProcedureStateMapNode();
        void SetCoverDownProcedure(FoupCoverStateEnum coverstate);
        void SetDockingPlateProcedure(DockingPlateStateEnum dockingplatestate);
        EventCodeEnum SequencesRefresh();
        //IFoupProcedureStateMaps LoadProcedure { get; set; }
        //IFoupProcedureStateMaps UnloadProcedure { get; set; }
    }
    public interface IFoupProcRunManager
    {
        EventCodeEnum FastBackwardRun();
        EventCodeEnum FastForwardRun();
        void InitProcedures();
        FoupStateEnum FindFoupState();
        EventCodeEnum LoadRun();
        EventCodeEnum NextRun();
        EventCodeEnum ContinueRun();
        EventCodeEnum PreviousRun();
        EventCodeEnum ReverseRun();
        EventCodeEnum UnloadRun();
    }
    public interface IFoupSubView
    {

    }
    public interface IFoup3DModel
    {

    }
    public interface ICSTControlCommands
    {
        #region // Foup Commands
        EventCodeEnum LockCassette(int index);
        EventCodeEnum UnLockCassette(int index);
        EventCodeEnum DockingPortIn(int index);
        EventCodeEnum DockingPortOut(int index);
        EventCodeEnum CoverOpen(int index);
        EventCodeEnum CoverClose(int index);
        EventCodeEnum CoverLock(int index);
        EventCodeEnum CoverUnLock(int index);
        EventCodeEnum FOUPReset(int index);
        void RaiseFoupModuleStateChanged(FoupModuleInfo moduleInfo);
        #endregion
    }
    public interface IFoupOpModule : IFactoryModule, IModule, IHasSysParameterizable, IHasDevParameterizable
    {
        IParam FoupManagerDevParam_IParam { get; set; }
        IParam FoupManagerSysParam_IParam { get; set; }
        ObservableCollection<IFoupController> FoupControllers { get; }

        FoupIOMappings GetFoupIOMap(int cassetteNumber);
        IFoupController GetFoupController(int cassetteNumber);
        EventCodeEnum FoupInitState();
        EventCodeEnum InitProcedures();
        void SetFoupModeStatus(int foupindex, FoupModeStatusEnum statusEnum);
        void TempSetFoupModeStatus(int foupindex, bool entry);
        IRFIDAdapter RFIDAdapter { get; }
    }

    public delegate void FoupPresenceStateChangeEvent(int foupIndex, bool presenceState, bool presenceStateChangedDone);
    public delegate void FoupCarrierIdChangedEvent(int foupIndex, string carrierId);
    public delegate void FoupClampStateChangeEvent(int foupIndex, bool clampState);
    public interface IFoupModule : IFactoryModule, IParamNode, IHasSysParameterizable, IModule
    {
        event FoupPresenceStateChangeEvent PresenceStateChangedEvent;
        event FoupCarrierIdChangedEvent FoupCarrierIdChangedEvent;
        event FoupClampStateChangeEvent FoupClampStateChangedEvent;
        int FoupNumber { get; }
        int FoupIndex { get; }
        bool IsLock { get; set; }
        IGPLoader GPLoader { get; }

        ICSTControlCommands GPCommand { get; }

        IFoupIOStates IOManager { get; }

        IFoupDockingPlate DockingPlate { get; }

        IFoupDockingPort DockingPort { get; }

        IFoupDockingPort40 DockingPort40 { get; }

        IFoupDoor Door { get; }

        IFoupTilt Tilt { get; }

        string ErrorDetails { get; set; }

        IFoupCover Cover { get; }

        IFoupCassetteOpener CassetteOpener { get; }

        IFoupPermission Permission { get; }

        ICylinderManager CylinderManager { get; set; }

        IFoupProcedureManager ProcManager { get; set; }

        SemaphoreSlim FplockSlim { get; set; }
        ObservableCollection<IOPortDescripter<bool>> Inputs { get; set; }

        ObservableCollection<IOPortDescripter<bool>> Outputs { get; set; }

        void FoupPermissionStateTransition(IFoupPermission state);

        void FoupModuleStateTransition(FoupModuleStateBase FoupModuleState);

        FoupModuleStateBase ModuleState { get; }

        void SetCallback(IFoupServiceCallback callback);

        EventCodeEnum InitModule(int foupNumber, FoupSystemParam systemParam, FoupDeviceParam deviceParam, CassetteConfigurationParameter CassetteConfigurationParam);

        EventCodeEnum Deinit();

        EventCodeEnum ChangeDevice(FoupDeviceParam deviceParam);
        EventCodeEnum CassetteTypeAvailable(CassetteTypeEnum cassetteType);
        CassetteTypeEnum GetCassetteType();
        EventCodeEnum ValidationCassetteAvailable(CassetteTypeEnum cassetteType , out string msg);
        FoupModuleInfo GetFoupModuleInfo();

        void SetLoaderState(ModuleStateEnum state);

        void SetLotState(ModuleStateEnum state);
        void ChangeState(FoupStateEnum state);

        EventCodeEnum Connect();

        EventCodeEnum Disconnect();

        EventCodeEnum InitState();

        EventCodeEnum ErrorClear();
        
        EventCodeEnum Load();
        
        EventCodeEnum Unload();

        EventCodeEnum FosB_Load();

        EventCodeEnum FosB_Unload();

        EventCodeEnum CoverUp();

        EventCodeEnum CoverDown();

        EventCodeEnum DockingPlateLock();

        EventCodeEnum DockingPlateUnlock();

        EventCodeEnum DockingPortIn();

        EventCodeEnum DockingPortOut();

        EventCodeEnum DockingPort40In();

        EventCodeEnum DockingPort40Out();

        EventCodeEnum CassetteOpenerLock();

        EventCodeEnum CassetteOpenerUnlock();
        EventCodeEnum FoupTiltUp();
        EventCodeEnum FoupTiltDown();

        EventCodeEnum MonitorForWaferOutSensor(bool value);

        //FoupProcedureManager FoupProcedureManager { get; set; }
        //IFoupProcedureManager ProcManager { get; set; }

        FoupSystemParam SystemParam { get; set; }

        FoupDeviceParam DeviceParam { get; set; }
        CassetteConfigurationParameter CassetteConfigurationParam { get; set; }
        void BroadcastFoupStateAsync();

        EventCodeEnum UpdateFoupState(bool forced_event = false);
        EventCodeEnum UpdateFosBFoupState();
        EventCodeEnum InitProcedures();
        ICassetteIDReaderModule CassetteIDReaderModule { get; }
        IBarcodeReaderModule BarcodeReaderModule { get; }
        IRFIDModule RFIDModule { get; }
        string Read_CassetteID();
        void ChangeFoupServiceStatus(GEMFoupStateEnum state, bool forcewrite = false, bool forced_event = false);
        void OccurFoupClampStateChangedEvent(int foupIndex, bool clampState);

        void RisingRFIDEvent(bool success, string casssetteId);
        EventCodeEnum FoupModuleReset();
        //List<string> GetLoadSequence();
        EventCodeEnum Continue();
        EventCodeEnum Refresh();
        EventCodeEnum PrevRun();
        EventCodeEnum InitSelectedProcedureStateMapNode(FoupStateEnum setfoupstate);
        EventCodeEnum CheckCoverDown();
        EventCodeEnum CheckDockingPlate();
        void ShowIOErrorMessage(List<string> errorIO);
        void ShowSequenceErrorMessage(string procedurestate, List<string> safetieslist);
        Task ShowFoupErrorDialogMessage();
        EventCodeEnum ValidationAvailableFoupState(int foupNumber);

        void SetFoupOptionInfomation(FoupOptionInfomation foupOptionInfo);
    }
    public interface IFoupSubModule
    {
    }

    public interface IFoupCover
    {
        FoupCoverStateEnum EnumState { get; set; }

        ObservableCollection<IOPortDescripter<bool>> Inputs { get; set; }

        ObservableCollection<IOPortDescripter<bool>> Outputs { get; set; }
        FoupCoverStateEnum GetState();

        EventCodeEnum Close();

        EventCodeEnum Open();

        EventCodeEnum StateInit();
       EventCodeEnum CheckState();
    }

    public interface IFoupDockingPlate
    {
        DockingPlateStateEnum EnumState { get; set; }
        ObservableCollection<IOPortDescripter<bool>> Inputs { get; set; }

        ObservableCollection<IOPortDescripter<bool>> Outputs { get; set; }

        EventCodeEnum Lock();
        EventCodeEnum Unlock();
        EventCodeEnum RecoveryUnlock();
        DockingPlateStateEnum GetState();
        EventCodeEnum StateInit();
        EventCodeEnum CheckState();
    }
    public interface IFoupCassetteOpener
    {
        FoupCassetteOpenerStateEnum EnumState { get; }
        ObservableCollection<IOPortDescripter<bool>> Inputs { get; set; }

        ObservableCollection<IOPortDescripter<bool>> Outputs { get; set; }
        EventCodeEnum Lock();
        EventCodeEnum Unlock();
        FoupCassetteOpenerStateEnum GetState();
        EventCodeEnum StateInit();
        EventCodeEnum CheckState();
    }

    public interface IFoupDockingPort
    {
        DockingPortStateEnum EnumState { get; set; }
        ObservableCollection<IOPortDescripter<bool>> Inputs { get; set; }

        ObservableCollection<IOPortDescripter<bool>> Outputs { get; set; }
        EventCodeEnum In();
        EventCodeEnum Out();
        DockingPortStateEnum GetState();
        EventCodeEnum StateInit();
        EventCodeEnum CheckState();
    }

    public interface IFoupDockingPort40
    {
        DockingPort40StateEnum EnumState { get; }
        EventCodeEnum In();
        EventCodeEnum Out();
        DockingPort40StateEnum GetState();
        EventCodeEnum StateInit();
    }

    public interface IFoupDoor
    {
        DockingPortDoorStateEnum EnumState { get; set; }
        EventCodeEnum Open();
        EventCodeEnum Close();
        DockingPortDoorStateEnum GetState();
        EventCodeEnum StateInit();
    }
    public interface IFoupTilt
    {
        TiltStateEnum EnumState { get; set; }
        EventCodeEnum Down();
        EventCodeEnum Up();
        TiltStateEnum GetState();
        EventCodeEnum StateInit();
    }

    public interface IFoupIOStates : IFactoryModule, IParamNode, IHasSysParameterizable, IModule
    {
        FoupIOMappings IOMap { get; }

        IIOService IOServ { get; set; }

        new EventCodeEnum InitModule();

        int DeInitIOStates();

        int WriteBit(IOPortDescripter<bool> portDesc, bool value);

        int ReadBit(IOPortDescripter<bool> portDesc, out bool value);

        int MonitorForIO(IOPortDescripter<bool> io, bool level, long maintainTime = 0, long timeout = 0);

        int WaitForIO(IOPortDescripter<bool> io, bool level, long timeout = 0);

        IOPortDescripter<bool> GetIOPortDescripter(string ioName);
    }

  

    public interface IFoupPermission
    {
        FoupPermissionStateEnum GetState();

        EventCodeEnum SetBusy();

        EventCodeEnum SetAuto();

        EventCodeEnum SetEveryOne();

    }

    public abstract class FoupModuleStateBase : IFactoryModule
    {
        protected IFoupModule Module { get; set; }

        public FoupModuleStateBase(IFoupModule module)
        {
            Module = module;
        }

        public abstract FoupStateEnum State { get; }

        public abstract EventCodeEnum Load();

        public abstract EventCodeEnum UnLoad();
        public abstract EventCodeEnum Continue();
        public abstract EventCodeEnum PrevRun();

    }
}
