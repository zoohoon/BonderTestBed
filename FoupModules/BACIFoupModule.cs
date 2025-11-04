using NLog;
using ParamHelper;
using ProberInterfaces.Enum;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using FoupModules.DockingPlate;
using FoupModules.DockingPort;
using FoupModules.DockingPort40;
using FoupModules.DockingPortDoor;
using FoupModules.FoupCover;
using FoupModules.FoupOpener;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoupModules.FoupModuleState;
using Autofac;
using ProberInterfaces;
using ProberInterfaces.Param;
using ProberInterfaces.Foup;
using FoupModules.Interfaces;

using System.Runtime.CompilerServices;
using ProberErrorCode;
using ProberInterfaces.Event;
using NotifyEventModule;
using CylinderManagerModule;
using System.Reflection;
using CylType;
using FoupProcedureManagerProject;
using LogModule;
using FoupModules.FoupTilt;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace FoupModules
{
    public class BACIFoupIOGlobalVar
    {
        public static long IO_CHECK_MAINTAIN_TIME = 100;
        public static long IO_CHECK_TIME_OUT = 500;
    }
    public class BACIFoupModule : IFoupModule, INotifyPropertyChanged
    {
        private object syncObj = new object();
        protected Logger Log = NLog.LogManager.GetCurrentClassLogger();

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        public Autofac.IContainer Container => FoupGlobal.Container;

        private int _FoupNumber;
        public int FoupNumber
        {
            get { return _FoupNumber; }
            set
            {
                if (value != _FoupNumber)
                {
                    _FoupNumber = value;
                    RaisePropertyChanged();
                }
            }
        }
        private IFoupIOStates _FoupIOManager;
        public IFoupIOStates IOManager
        {

            get { return _FoupIOManager; }
            set
            {
                if (value != _FoupIOManager)
                {
                    _FoupIOManager = value;
                    RaisePropertyChanged();
                }
            }
        }

        private FoupSystemParam _SystemParam;
        public FoupSystemParam SystemParam
        {
            get { return _SystemParam; }
            set
            {
                if (value != _SystemParam)
                {
                    _SystemParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private FoupDeviceParam _DeviceParam;
        public FoupDeviceParam DeviceParam
        {
            get { return _DeviceParam; }
            set
            {
                if (value != _DeviceParam)
                {
                    _DeviceParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IFoupPermission _Permission;
        public IFoupPermission Permission
        {
            get { return _Permission; }
            set
            {
                if (value != _Permission)
                {
                    _Permission = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IFoupDockingPlate _DockingPlate;
        public IFoupDockingPlate DockingPlate
        {
            get { return _DockingPlate; }
            set
            {
                if (value != _DockingPlate)
                {
                    _DockingPlate = value;
                    RaisePropertyChanged();
                }
            }
        }


        private IFoupDockingPort _DockingPort;
        public IFoupDockingPort DockingPort
        {
            get { return _DockingPort; }
            set
            {
                if (value != _DockingPort)
                {
                    _DockingPort = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IFoupDockingPort40 _DockingPort40;
        public IFoupDockingPort40 DockingPort40
        {
            get { return _DockingPort40; }
            set
            {
                if (value != _DockingPort40)
                {
                    _DockingPort40 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IFoupDoor _Door;
        public IFoupDoor Door
        {
            get { return _Door; }
            set
            {
                if (value != _Door)
                {
                    _Door = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IFoupCover _Cover;
        public IFoupCover Cover
        {
            get { return _Cover; }
            set
            {
                if (value != _Cover)
                {
                    _Cover = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IFoupCassetteOpener _CassetteOpener;
        public IFoupCassetteOpener CassetteOpener
        {
            get { return _CassetteOpener; }
            set
            {
                if (value != _CassetteOpener)
                {
                    _CassetteOpener = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IFoupTilt _Tilt;
        public IFoupTilt Tilt
        {
            get { return _Tilt; }
            set
            {
                if (value != _Tilt)
                {
                    _Tilt = value;
                    RaisePropertyChanged();
                }
            }
        }
        private FoupModuleStateBase _ModuleState;
        public FoupModuleStateBase ModuleState
        {
            get { return _ModuleState; }
            set
            {
                if (value != _ModuleState)
                {
                    _ModuleState = value;
                    RaisePropertyChanged();
                }
            }
        }
        private FoupLoadUnloadParam FoupLoadUnloadParam;

        private FoupProcedureManager _FoupProcedureManager = new FoupProcedureManager();
        public FoupProcedureManager FoupProcedureManager
        {
            get { return _FoupProcedureManager; }
            set
            {
                if (value != _FoupProcedureManager)
                {
                    _FoupProcedureManager = value;
                    RaisePropertyChanged();
                }
            }
        }

        private FoupModuleInfo _foupInfo = new FoupModuleInfo();
        public FoupModuleInfo foupInfo
        {
            get { return _foupInfo; }
            set
            {
                if (value != _foupInfo)
                {
                    _foupInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ModuleStateEnum LoaderState { get; set; }

        public ModuleStateEnum LotState { get; set; }

        public IFileManager FileManager => this.FileManager();

        public IFoupServiceCallback Callback { get; set; }
        public ICylinderManager CylinderManager { get; set; }
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }
        public List<object> Nodes { get; set; }
        public IFoupProcedureManager ProcManager { get; set; }

        //private IParam _SysParam;
        //[ParamIgnore]
        //public IParam SysParam
        //{
        //    get { return _SysParam; }
        //    set
        //    {
        //        if (value != _SysParam)
        //        {
        //            _SysParam = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        public bool Initialized { get; set; } = false;

        public ErrorCodeEnum Deinit()
        {
            ErrorCodeEnum retVal;
            try
            {

                _FoupIOManager.DeInitIOStates();
                retVal = ErrorCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public void ChangeState(FoupStateEnum state)
        {
            try
            {
                switch (state)
                {
                    case FoupStateEnum.ERROR:
                        ModuleState = new FoupErrorState(this);
                        break;
                    case FoupStateEnum.ILLEGAL:
                        ModuleState = new FoupilegalState(this);
                        break;
                    case FoupStateEnum.LOAD:
                        ModuleState = new FoupLoadState(this);
                        break;
                    case FoupStateEnum.UNLOAD:
                        ModuleState = new FoupUnLoadState(this);
                        break;
                    default:
                        ModuleState = new FoupilegalState(this);
                        break;
                }

                BroadcastFoupStateAsync();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void BroadcastFoupStateAsync()
        {
            try
            {
                var moduleInfo = GetFoupModuleInfo();

                //** 동기로 호출하도록 변경
                if (Callback != null)
                {
                    Callback.RaiseFoupModuleStateChanged(moduleInfo);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public FoupModuleInfo GetFoupModuleInfo()
        {
            //FoupModuleInfo foupInfo = new FoupModuleInfo();
            try
            {

                foupInfo.FoupNumber = FoupNumber;
                foupInfo.DockingPlateState = DockingPlateStateEnum.LOCK;
                foupInfo.OpenerState = FoupCassetteOpenerStateEnum.LOCK;
                foupInfo.DockingPortState = DockingPortStateEnum.IN;
                foupInfo.DockingPort40State = DockingPort40StateEnum.OUT;
                foupInfo.DockingPortDoorState = DockingPortDoorStateEnum.CLOSE;
                foupInfo.FoupCoverState = FoupCoverStateEnum.UP;

                foupInfo.State = ModuleState.State;
                //foupInfo.FoupVacSensorStateEnum 

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return foupInfo;
        }
        public void SetCallback(IFoupServiceCallback callback)
        {
            this.Callback = callback;
        }
        //public IFoupProcedureManager ProcManager { get; set; }
        //IFoupProcedureManager IFoupModule.FoupProcedureManager { get; set; }

        private CylinderManager cylinderManager;


        public ErrorCodeEnum LoadSysParameter()
        {
            ErrorCodeEnum RetVal = ErrorCodeEnum.UNDEFINED;
            try
            {

                RetVal = LoadCylinderManagerParam();

                RetVal = LoadFoupLoadUnloadParameter();
                RetVal = ErrorCodeEnum.NONE;
                ProcManager = new FoupProcedureManager();
                ProcManager.SettingProcedure(FoupLoadUnloadParam.FoupProcedureList,
                                                        FoupLoadUnloadParam.LoadOrder,
                                                        FoupLoadUnloadParam.UnloadOrder,
                                                        this,
                                                        IOManager
                                                        );

                CylinderManager = cylinderManager;
                ProcManager = ProcManager;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        private Type[] GetFoupLoadNeedTypes()
        {
            List<Type> FoupLoadNeedAllType = new List<Type>();
            try
            {
                FoupLoadNeedAllType.Add(typeof(FoupBehavior));
                FoupLoadNeedAllType.Add(typeof(FoupSafeties));

                var foupBehaviorClass = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                        from type in assembly.GetTypes()
                                        where type.IsSubclassOf(typeof(FoupBehavior))
                                        select type;

                var foupSafetyClass = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                      from type in assembly.GetTypes()
                                      where type.IsSubclassOf(typeof(FoupSafety))
                                      select type;

                foreach (var behavior in foupBehaviorClass)
                {
                    FoupLoadNeedAllType.Add(behavior);
                }

                foreach (var safety in foupSafetyClass)
                {
                    FoupLoadNeedAllType.Add(safety);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug("Error occurred.");
                LoggerManager.Exception(err);

            }

            return FoupLoadNeedAllType.ToArray();
        }

        private ErrorCodeEnum LoadFoupLoadUnloadParameter()
        {
            ErrorCodeEnum RetVal = ErrorCodeEnum.UNDEFINED;
            try
            {

                object deserializedObj;

                string FullPath;

                FoupLoadUnloadParam = new FoupLoadUnloadParam();

                // 1. Wafer Size : 6Inch, 8Inch, 12Inch
                // 2. Wafer Type : Normal, Framed
                // 3. Load Type : Flat, Top, 6&8 Loader

                // (1) 5호기꺼
                // Wafer Size = 12Inch
                // Wafer Type = Normal
                // Load Type = Top

                string FilePath = FoupLoadUnloadParam.FilePath;
                string FileName = FoupLoadUnloadParam.FileName;

                FullPath = this.FileManager().GetSystemParamFullPath(FilePath, FileName);

                Type[] FoupLoadNeedTypes = null;
                FoupLoadNeedTypes = GetFoupLoadNeedTypes();

                try
                {
                    if (Directory.Exists(Path.GetDirectoryName(FullPath)) == false)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(FullPath));
                    }

                    IParam tmpParam = null;
                    tmpParam = new FoupLoadUnloadParam();
                    tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                    RetVal = this.LoadParameter(ref tmpParam, typeof(FoupLoadUnloadParam));

                    if (RetVal == ErrorCodeEnum.NONE)
                    {
                        FoupLoadUnloadParam = tmpParam as FoupLoadUnloadParam;
                    }
                    //if (File.Exists(FullPath) == false)
                    //{
                    //    FoupLoadUnloadParam.SetDefaultParam();

                    //    RetVal = ParamServices.Serialize(FullPath, FoupLoadUnloadParam, FoupLoadNeedTypes);

                    //    if (RetVal == ErrorCodeEnum.PARAM_ERROR)
                    //    {
                    //        Log.Error(String.Format("[FoupModule] FoupLoadUnloadParam(): Serialize Error"));
                    //        return RetVal;
                    //    }
                    //}

                    //deserializedObj = ParamServices.Deserialize(FullPath, typeof(FoupLoadUnloadParam), FoupLoadNeedTypes);

                    //if (deserializedObj == null)
                    //{
                    //    RetVal = ErrorCodeEnum.PARAM_ERROR;

                    //    Log.Error(String.Format("[FoupModule] FoupLoadUnloadParam(): DeSerialize Error"));
                    //    return RetVal;
                    //}

                    //FoupLoadUnloadParam = (FoupLoadUnloadParam)deserializedObj;

                    RetVal = ErrorCodeEnum.NONE;
                }
                catch (Exception err)
                {
                    RetVal = ErrorCodeEnum.PARAM_ERROR;
                    Log.Error(String.Format("[FoupModule] FoupLoadUnloadParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                    throw;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        private ErrorCodeEnum LoadCylinderManagerParam()
        {
            ErrorCodeEnum RetVal = ErrorCodeEnum.UNDEFINED;
            try
            {
                BindingFlags _BindFlags = BindingFlags.Public | BindingFlags.Instance;
                CylinderParams Params = new CylinderParams();
                List<IOPortDescripter<bool>> ioPorDescripterList = null;
                string FullPath = string.Empty;
                #region CylinderManager...

                FullPath = this.FileManager().GetSystemParamFullPath(Params.FilePath, "FoupCylinderIOParameter.json");
                ioPorDescripterList = new List<IOPortDescripter<bool>>();
                cylinderManager = new CylinderManager();
                cylinderManager.Cylinders = new FoupCylinderType();

                if (Directory.Exists(Path.GetDirectoryName(FullPath)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(FullPath));
                }

                if (File.Exists(FullPath) == false)
                {
                    Params = SetDefaultFoupCylinderParam();
                    cylinderManager.LoadParameter(FullPath, ioPorDescripterList, Params);
                }
                else
                {
                    cylinderManager.LoadParameter(FullPath, ioPorDescripterList);
                }
                #endregion

                CylinderManager = cylinderManager;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        private CylinderMappingParameter MakeCylinderParameter(string cylindername,
                                                        string ex_out_key,
                                                        string re_out_key,
                                                        List<string> ex_in_key_list,
                                                        List<string> re_in_key_list)
        {
            CylinderMappingParameter ret = new CylinderMappingParameter();
            try
            {

                ret.CylinderName = cylindername;
                ret.Extend_Output_Key = ex_out_key;
                ret.Retract_OutPut_Key = re_out_key;

                if (ret.Extend_Input_key_list == null)
                {
                    ret.Extend_Input_key_list = new List<string>();
                }

                ret.Extend_Input_key_list = ex_in_key_list;

                if (ret.Retract_Input_key_list == null)
                {
                    ret.Retract_Input_key_list = new List<string>();
                }

                ret.Retract_Input_key_list = re_in_key_list;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }
        private CylinderParams SetDefaultFoupCylinderParam()
        {
            CylinderParams Params = new CylinderParams();
            try
            {

                List<string> Extand_In = new List<string>();
                List<string> Retract_In = new List<string>();

                // FoupDockingPlate6

                Extand_In.Add(IOManager.IOMap.Inputs.DI_C6IN_PLACEMENT.Key.Value);
                Retract_In.Add(IOManager.IOMap.Inputs.DI_C6IN_C8IN_NPLACEMENT.Key.Value);

                Params.CylinderMappingParameterList.Add
                    (
                        MakeCylinderParameter
                            (
                                "FoupDockingPlate6",
                                IOManager.IOMap.Outputs.DO_C6IN_C8IN_LOCK_AIR.Key.Value,
                                IOManager.IOMap.Outputs.DO_NULL.Key.Value,
                                Extand_In.ToList(),
                                Retract_In.ToList()
                            )
                    );


                Extand_In.Clear();
                Retract_In.Clear();

                // FoupDockingPlate8
                Extand_In.Add(IOManager.IOMap.Inputs.DI_C8IN_PLACEMENT.Key.Value);
                Retract_In.Add(IOManager.IOMap.Inputs.DI_C6IN_C8IN_NPLACEMENT.Key.Value);

                Params.CylinderMappingParameterList.Add
                    (
                        MakeCylinderParameter
                        (
                            "FoupDockingPlate8",
                            IOManager.IOMap.Outputs.DO_C6IN_C8IN_LOCK_AIR.Key.Value,
                            IOManager.IOMap.Outputs.DO_NULL.Key.Value,
                            Extand_In.ToList(),
                            Retract_In.ToList()
                        )
                    );
                Extand_In.Clear();
                Retract_In.Clear();

                // FoupDockingPlate12
                Extand_In.Add(IOManager.IOMap.Inputs.DI_C12IN_PLACEMENT.Key.Value);
                Retract_In.Add(IOManager.IOMap.Inputs.DI_C12IN_NPLACEMENT.Key.Value);

                Params.CylinderMappingParameterList.Add(
                    MakeCylinderParameter(
                        "FoupDockingPlate12",
                        IOManager.IOMap.Outputs.DO_NULL.Key.Value,
                        IOManager.IOMap.Outputs.DO_NULL.Key.Value,
                        Extand_In.ToList(),
                        Retract_In.ToList()
                        )
                    );
                Extand_In.Clear();
                Retract_In.Clear();

                // FoupDockingPort
                Extand_In.Add(IOManager.IOMap.Inputs.DI_CP_IN.Key.Value);
                Retract_In.Add(IOManager.IOMap.Inputs.DI_CP_OUT.Key.Value);

                Params.CylinderMappingParameterList.Add(
                    MakeCylinderParameter(
                        "FoupDockingPort",
                        IOManager.IOMap.Outputs.DO_NULL.Key.Value,
                        IOManager.IOMap.Outputs.DO_NULL.Key.Value,
                        Extand_In.ToList(),
                        Retract_In.ToList())
                    );
                Extand_In.Clear();
                Retract_In.Clear();

                // FoupDockingPort40
                Extand_In.Add(IOManager.IOMap.Inputs.DI_CP_40_IN.Key.Value);
                Retract_In.Add(IOManager.IOMap.Inputs.DI_CP_40_OUT.Key.Value);

                Params.CylinderMappingParameterList.Add(
                    MakeCylinderParameter(
                        "FoupDockingPort40",
                        IOManager.IOMap.Outputs.DO_NULL.Key.Value,
                        IOManager.IOMap.Outputs.DO_NULL.Key.Value,
                        Extand_In.ToList(),
                        Retract_In.ToList())
                    );
                Extand_In.Clear();
                Retract_In.Clear();

                // FoupRotator
                Extand_In.Add(IOManager.IOMap.Inputs.DI_FO_CLOSE.Key.Value);
                Retract_In.Add(IOManager.IOMap.Inputs.DI_FO_OPEN.Key.Value);

                Params.CylinderMappingParameterList.Add(
                    MakeCylinderParameter(
                        "FoupRotator",
                        IOManager.IOMap.Outputs.DO_NULL.Key.Value,
                        IOManager.IOMap.Outputs.DO_NULL.Key.Value,
                        Extand_In.ToList(),
                        Retract_In.ToList())
                    );
                Extand_In.Clear();
                Retract_In.Clear();

                // FoupCover
                Extand_In.Add(IOManager.IOMap.Inputs.DI_FO_UP.Key.Value);
                Retract_In.Add(IOManager.IOMap.Inputs.DI_FO_DOWN.Key.Value);

                Params.CylinderMappingParameterList.Add(
                    MakeCylinderParameter(
                        "FoupCover",
                        IOManager.IOMap.Outputs.DO_NULL.Key.Value,
                        IOManager.IOMap.Outputs.DO_NULL.Key.Value,
                        Extand_In.ToList(),
                        Retract_In.ToList())
                    );
                Extand_In.Clear();
                Retract_In.Clear();

                // FoupCassetteTiltting
                //Extand_In.Add(IOManager.IOMap.Inputs.DI_CSTT_UP.Key.Value);
                Retract_In.Add(IOManager.IOMap.Inputs.DI_CSTT_DOWN.Key.Value);

                Params.CylinderMappingParameterList.Add(
                    MakeCylinderParameter(
                        "FoupCassetteTilting",
                        IOManager.IOMap.Outputs.DO_CSTT_AIR.Key.Value,
                        IOManager.IOMap.Outputs.DO_NULL.Key.Value,
                        Extand_In.ToList(),
                        Retract_In.ToList())
                    );
                Extand_In.Clear();
                Retract_In.Clear();



            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Params;
        }

        public ErrorCodeEnum ChangeDevice(FoupDeviceParam param)
        {
            ErrorCodeEnum retVal;
            try
            {

                this.DeviceParam = param;

                retVal = InitState();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public ErrorCodeEnum InitState()
        {
            ErrorCodeEnum retVal;
            Permission = new FoupPermissionEveryOneState(this);


            return ErrorCodeEnum.NONE;
        }
        public ErrorCodeEnum ErrorClear()
        {
            ErrorCodeEnum retVal;
            try
            {

                retVal = ErrorCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public ErrorCodeEnum SetLoaderBusy(bool isEnable)
        {
            ErrorCodeEnum retVal;
            try
            {

                if (isEnable)
                {
                    //퍼미션 상태 변경.
                    //UpdateFoupLampState();
                    retVal = ErrorCodeEnum.NONE;
                }
                else
                {
                    ErrorClear();
                    //UpdateFoupState();
                    retVal = ErrorCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public void FoupModuleStateTransition(FoupModuleStateBase FoupModuleState)
        {
            this.ModuleState = FoupModuleState;
        }

        public ErrorCodeEnum InitModule(int foupNumber, FoupSystemParam systemParam, FoupDeviceParam deviceParam)
        {
            ErrorCodeEnum retVal;

            try
            {
                this.FoupNumber = foupNumber;
                this.SystemParam = systemParam;
                this.DeviceParam = deviceParam;

                IOManager = new FoupIOStates();
                IOManager.LoadSysParameter();
                IOManager.InitModule();

                retVal = LoadSysParameter();
                ProcManager = new EmulFoupProcManager();
                ModuleState = new FoupLoadState(this);

                //emul lamp status
                foupInfo.PresenceLamp = true;
                foupInfo.PlacementLamp = false;
                foupInfo.LoadLamp = true;
                foupInfo.UnloadLamp = false;
                foupInfo.AutoLamp = true;
                foupInfo.BusyLamp = true;
                foupInfo.AlarmLamp = false;

                foupInfo = GetFoupModuleInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Debug("Error occurred.");
                LoggerManager.Exception(err);
            }


            return ErrorCodeEnum.NONE;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SetLotStateInfo(ModuleStateEnum state)
        {
            try
            {
                LotState = state;
                BroadcastFoupStateAsync();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public ErrorCodeEnum Connect()
        {
            Log.Debug($"Foup emulator is online.");
            return ErrorCodeEnum.NONE;
        }

        public ErrorCodeEnum Disconnect()
        {
            return ErrorCodeEnum.NONE;
        }
        private bool UpdatePermissionState()
        {
            bool isFoupStateUpdate = false;
            try
            {
                //=> Update FoupPermission
                if (LoaderState == ModuleStateEnum.RUNNING)
                {
                    if (Permission.GetState() != FoupPermissionStateEnum.BUSY)
                    {
                        isFoupStateUpdate = true;
                        Permission.SetBusy();
                    }
                }
                else if (LotState != ModuleStateEnum.IDLE)
                {
                    if (Permission.GetState() != FoupPermissionStateEnum.AUTO)
                    {
                        isFoupStateUpdate = true;
                        Permission.SetAuto();
                    }
                }
                else
                {
                    if (Permission.GetState() != FoupPermissionStateEnum.EVERY_ONE)
                    {
                        isFoupStateUpdate = true;
                        Permission.SetEveryOne();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isFoupStateUpdate;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SetLoaderState(ModuleStateEnum state)
        {
            try
            {
                LoaderState = state;

                bool isUpdate = UpdatePermissionState();

                if (isUpdate)
                {
                    BroadcastFoupStateAsync();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public ErrorCodeEnum Load()
        {
            ErrorCodeEnum retVal;
            try
            {

                if (Callback.IsFoupUsingByLoader())
                {
                    retVal = ErrorCodeEnum.UNDEFINED;
                }
                else
                {
                    retVal = ModuleState.Load();
                    BroadcastFoupStateAsync();
                    //UpdateFoupState();
                    ModuleState = new FoupLoadState(this);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public ErrorCodeEnum Unload()
        {
            ErrorCodeEnum retVal;
            try
            {
                retVal = ModuleState.UnLoad();
                BroadcastFoupStateAsync();
                //UpdateFoupState();
                ModuleState = new FoupUnLoadState(this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public ErrorCodeEnum CoverUp()
        {
            return ErrorCodeEnum.NONE;
        }

        public ErrorCodeEnum CoverDown()
        {
            return ErrorCodeEnum.NONE;
        }

        public ErrorCodeEnum DockingPlateLock()
        {
            ErrorCodeEnum retVal;
            try
            {
                if (DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH6)
                {
                    retVal = DockingPlate.Lock();
                    //GetFoupModuleInfo();
                    BroadcastFoupStateAsync();
                }
                else if (DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH8)
                {
                    retVal = DockingPlate.Lock();
                    //GetFoupModuleInfo();
                    BroadcastFoupStateAsync();
                }
                else if (DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH12)
                {
                    retVal = ErrorCodeEnum.NONE;
                }
                else
                {
                    retVal = ErrorCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public ErrorCodeEnum DockingPlateUnlock()
        {
            ErrorCodeEnum retVal;
            try
            {
                if (DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH6)
                {
                    retVal = DockingPlate.Unlock();
                    //GetFoupModuleInfo();
                    BroadcastFoupStateAsync();
                }
                else if (DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH8)
                {
                    retVal = DockingPlate.Unlock();
                    //GetFoupModuleInfo();
                    BroadcastFoupStateAsync();
                }
                else if (DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH12)
                {
                    retVal = ErrorCodeEnum.NONE;
                }
                else
                {
                    retVal = ErrorCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public ErrorCodeEnum DockingPortIn()
        {
            return ErrorCodeEnum.NONE;
        }

        public ErrorCodeEnum DockingPortOut()
        {
            return ErrorCodeEnum.NONE;
        }

        public ErrorCodeEnum DockingPort40In()
        {
            return ErrorCodeEnum.NONE;
        }

        public ErrorCodeEnum DockingPort40Out()
        {
            return ErrorCodeEnum.NONE;
        }

        public ErrorCodeEnum CassetteOpenerLock()
        {
            return ErrorCodeEnum.NONE;
        }

        public ErrorCodeEnum CassetteOpenerUnlock()
        {
            return ErrorCodeEnum.NONE;
        }

        public ErrorCodeEnum MonitorForWaferOutSensor(bool value)
        {
            return ErrorCodeEnum.NONE;
        }

        public void SetLotState(ModuleStateEnum state)
        {
            this.SetLotState(state);
        }

        public ErrorCodeEnum SaveSysParameter()
        {
            ErrorCodeEnum retVal = ErrorCodeEnum.UNDEFINED;
            try
            {
                CylinderParams Params = new CylinderParams();
                string FullPath = string.Empty;

                FullPath = this.FileManager().GetSystemParamFullPath(Params.FilePath, "FoupCylinderIOParameter.json");
                retVal = cylinderManager.SaveParameter(FullPath);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public ErrorCodeEnum FoupTiltUp()
        {
            ErrorCodeEnum retVal;
            try
            {
                if (DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH6)
                {
                    retVal = Tilt.Up();
                    //GetFoupModuleInfo();
                    BroadcastFoupStateAsync();
                }
                else if (DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH8)
                {
                    retVal = Tilt.Up();
                    //GetFoupModuleInfo();
                    BroadcastFoupStateAsync();
                }
                else if (DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH12)
                {
                    retVal = ErrorCodeEnum.NONE;
                }
                else
                {
                    retVal = ErrorCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;

        }
        public ErrorCodeEnum FoupTiltDown()
        {
            ErrorCodeEnum retVal;
            try
            {
                if (DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH6)
                {
                    retVal = Tilt.Down();
                    //GetFoupModuleInfo();
                    BroadcastFoupStateAsync();
                }
                else if (DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH8)
                {
                    retVal = Tilt.Down();
                    //GetFoupModuleInfo();
                    BroadcastFoupStateAsync();
                }
                else if (DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH12)
                {
                    retVal = ErrorCodeEnum.NONE;
                }
                else
                {
                    retVal = ErrorCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;

        }

        public void FoupPermissionStateTransition(IFoupPermission state)
        {
            Permission = state;
        }

        public void DeInitModule()
        {
        }

        public ErrorCodeEnum InitModule()
        {
            ErrorCodeEnum retval = ErrorCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    Initialized = true;

                    retval = ErrorCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.DebugError($"DUPLICATE_INVOCATION");

                    retval = ErrorCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }


    public class BACIFoupProcManager : IFoupProcedureManager, IFoupProcRunManager, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        private IFoupProcedureStateMaps _SelectedProcedureStateMaps;
        public IFoupProcedureStateMaps SelectedProcedureStateMaps
        {
            get { return _SelectedProcedureStateMaps; }
            set
            {
                _SelectedProcedureStateMaps = value;
                RaisePropertyChanged();
            }
        }
        public ErrorCodeEnum FastBackwardRun()
        {
            return ErrorCodeEnum.NONE;
        }

        public ErrorCodeEnum FastForwardRun()
        {
            return ErrorCodeEnum.NONE;
        }

        public void InitProcedures()
        {

        }

        public ErrorCodeEnum LoadRun()
        {
            return ErrorCodeEnum.NONE;
        }

        public ErrorCodeEnum NextRun()
        {
            return ErrorCodeEnum.NONE;
        }

        public ErrorCodeEnum PreviousRun()
        {
            return ErrorCodeEnum.NONE;
        }

        public ErrorCodeEnum ReverseRun()
        {
            return ErrorCodeEnum.NONE;
        }

        public ErrorCodeEnum UnloadRun()
        {
            return ErrorCodeEnum.NONE;
        }

        public void SettingProcedure(List<IFoupProcedure> foupProcedures, List<string> LoadOrderList, List<string> UnloadOrderList, IFoupModule FoupModule, IFoupIOStates FoupIOManager)
        {

        }
        #endregion

        public ErrorCodeEnum LoadSysParameter()
        {
            ErrorCodeEnum retval = ErrorCodeEnum.UNDEFINED;

            try
            {
                retval = ErrorCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }


    }
}
