using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using FoupModules.FoupModuleState;
using Autofac;
using ProberInterfaces;
using ProberInterfaces.Foup;

using System.Runtime.CompilerServices;
using ProberErrorCode;
using CylinderManagerModule;

using CylType;
using FoupProcedureManagerProject;
using LogModule;
using System.Xml.Serialization;
using Newtonsoft.Json;
using RFID;
using LoaderBase;
using System.Threading;
using ProberInterfaces.RFID;
using NotifyEventModule;
using ProberInterfaces.Event;
using ProberInterfaces.Enum;
using ProberInterfaces.CassetteIDReader;
using CassetteIDReader;
using ProberInterfaces.Communication.BarcodeReader;
using FoupModules.DockingPlate;
using FoupModules.DockingPort;
using FoupModules.FoupOpener;
using FoupModules.FoupCover;
using FoupModules.FoupTilt;
using FoupModules.DockingPort40;
using System.Collections.ObjectModel;
using MetroDialogInterfaces;
using ProberInterfaces.ControlClass.ViewModel.Foup;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using FoupModules.DockingPortDoor;
using ProberInterfaces.Template;
using System.Reflection;

namespace FoupModules
{
    public class EmulFoupModule : IFoupModule, INotifyPropertyChanged, ITemplateModule, ITemplate, ITemplateStateModule
    {
        private object syncObj = new object();

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;
#pragma warning disable 0067
        public event FoupPresenceStateChangeEvent PresenceStateChangedEvent;
#pragma warning restore 0067
        public event FoupCarrierIdChangedEvent FoupCarrierIdChangedEvent;
        public event FoupClampStateChangeEvent FoupClampStateChangedEvent;
        public Autofac.IContainer Container => FoupGlobal.Container;

        private SemaphoreSlim _FplockSlim = new SemaphoreSlim(1, 1);
        public SemaphoreSlim FplockSlim
        {
            get { return _FplockSlim; }
            set { _FplockSlim = value; }
        }

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
        private int _FoupIndex;
        public int FoupIndex
        {
            get { return _FoupIndex; }
            private set
            {
                if (value != _FoupIndex)
                {
                    _FoupIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsLock;
        public bool IsLock
        {
            get { return _IsLock; }
            set
            {
                if (value != _IsLock)
                {
                    _IsLock = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Foup6_8Presence1;
        public bool Foup6_8Presence1
        {
            get { return _Foup6_8Presence1; }
            set { _Foup6_8Presence1 = value; }
        }
        private bool _Foup6_8Presence2;

        public bool Foup6_8Presence2
        {
            get { return _Foup6_8Presence2; }
            set { _Foup6_8Presence2 = value; }
        }
        private bool _Foup6_8Presence3;

        public bool Foup6_8Presence3
        {
            get { return _Foup6_8Presence3; }
            set { _Foup6_8Presence3 = value; }
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

        private CassetteConfigurationParameter _CassetteConfigurationParam;
        public CassetteConfigurationParameter CassetteConfigurationParam
        {
            get { return _CassetteConfigurationParam; }
            set
            {
                if (value != _CassetteConfigurationParam)
                {
                    _CassetteConfigurationParam = value;
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

        private string _ErrorDetails = "";

        public string ErrorDetails
        {
            get { return _ErrorDetails; }
            set { _ErrorDetails = value; }
        }


        private TiltStateEnum _CurTiltState;
        public TiltStateEnum CurTiltState
        {
            get { return _CurTiltState; }
            set
            {
                if (value != _CurTiltState)
                {
                    _CurTiltState = value;
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


        private FoupOptionInfomation _FoupOptionInfo = new FoupOptionInfomation();
        public FoupOptionInfomation FoupOptionInfo
        {
            get { return _FoupOptionInfo; }
            set
            {
                if (value != _FoupOptionInfo)
                {
                    _FoupOptionInfo = value;
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
        private ObservableCollection<IOPortDescripter<bool>> _Inputs = new ObservableCollection<IOPortDescripter<bool>>();
        public ObservableCollection<IOPortDescripter<bool>> Inputs
        {
            get { return _Inputs; }
            set
            {
                if (value != _Inputs)
                {
                    _Inputs = value;
                }
            }
        }
        private ObservableCollection<IOPortDescripter<bool>> _Outputs = new ObservableCollection<IOPortDescripter<bool>>();
        public ObservableCollection<IOPortDescripter<bool>> Outputs
        {
            get { return _Outputs; }
            set
            {
                if (value != _Outputs)
                {
                    _Outputs = value;
                }
            }
        }
        public List<object> Nodes { get; set; }
        public IFoupProcedureManager ProcManager { get; set; }

        public IGPLoader GPLoader => this.GetGPLoader();

        public ICSTControlCommands GPCommand => (ICSTControlCommands)GPLoader;


        private ICassetteIDReaderModule _CassetteIDReaderModule;
        public ICassetteIDReaderModule CassetteIDReaderModule
        {
            get { return _CassetteIDReaderModule; }
            set
            {
                if (value != _CassetteIDReaderModule)
                {
                    _CassetteIDReaderModule = value;
                    RaisePropertyChanged();
                }
            }
        }
        private IBarcodeReaderModule _BarcodeReaderModule;
        public IBarcodeReaderModule BarcodeReaderModule
        {
            get { return _BarcodeReaderModule; }
            set
            {
                if (value != _BarcodeReaderModule)
                {
                    _BarcodeReaderModule = value;
                    RaisePropertyChanged();
                }
            }
        }
        private IRFIDModule _RFIDModule;
        public IRFIDModule RFIDModule
        {
            get { return _RFIDModule; }
            set
            {
                if (value != _RFIDModule)
                {
                    _RFIDModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum Deinit()
        {
            EventCodeEnum retVal;
            try
            {

                _FoupIOManager.DeInitIOStates();
                retVal = EventCodeEnum.NONE;

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
                    //case FoupStateEnum.ILLEGAL:
                    //    ModuleState = new FoupilegalState(this);
                    //    break;
                    case FoupStateEnum.LOAD:
                        ModuleState = new FoupLoadState(this);
                        break;
                    case FoupStateEnum.UNLOAD:
                        ModuleState = new FoupUnLoadState(this);
                        break;
                    default:
                        ModuleState = new FoupErrorState(this);
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
                if (SystemParam.FoupType.Value == FoupTypeEnum.TOP)
                {

                    foupInfo.FoupNumber = FoupNumber;
                    foupInfo.DockingPlateState = DockingPlate.GetState();
                    foupInfo.OpenerState = CassetteOpener.GetState();
                    foupInfo.DockingPortState = DockingPort.GetState();
                    foupInfo.DockingPort40State = DockingPort40.GetState();
                    //foupInfo.DockingPortDoorState = Door.GetState();
                    foupInfo.FoupCoverState = Cover.EnumState;
                    // UpdateFoupState();
                    foupInfo.State = ModuleState.State;

                    foupInfo.TiltState = Tilt.GetState();

                    //foupInfo.FoupVacSensorStateEnum 
                }
                else if (SystemParam.FoupType.Value == FoupTypeEnum.FLAT)
                {
                    foupInfo.FoupNumber = FoupNumber;
                    foupInfo.DockingPlateState = DockingPlate.GetState();
                    foupInfo.OpenerState = CassetteOpener.GetState();
                    foupInfo.DockingPortState = DockingPort.GetState();
                    foupInfo.DockingPort40State = DockingPort40.GetState();
                    foupInfo.DockingPortDoorState = Door.GetState();
                    foupInfo.FoupCoverState = Cover.EnumState;
                    // UpdateFoupState();
                    foupInfo.State = ModuleState.State;

                    foupInfo.TiltState = Tilt.GetState();

                    //foupInfo.FoupVacSensorStateEnum 
                }
                else if (SystemParam.FoupType.Value == FoupTypeEnum.CST8PORT_FLAT)
                {
                    foupInfo.FoupNumber = FoupNumber;
                    foupInfo.DockingPlateState = DockingPlate.GetState();
                    // UpdateFoupState();
                    foupInfo.State = ModuleState.State;
                    foupInfo.FoupCoverState = FoupCoverStateEnum.OPEN;
                    //foupInfo.FoupVacSensorStateEnum 
                    foupInfo.TiltState = Tilt.GetState();
                }
                else if (SystemParam.FoupType.Value == FoupTypeEnum.UNDEFINED)
                {
                    foupInfo.FoupNumber = FoupNumber;
                    foupInfo.DockingPlateState = DockingPlate.GetState();
                    if (CassetteOpener == null)
                    {
                        GPFoupOpener gfo = new GPFoupOpener();
                        gfo.Module = this;
                        gfo.StateInit();
                        CassetteOpener = gfo;
                    }
                    foupInfo.OpenerState = CassetteOpener.GetState();
                    foupInfo.DockingPortState = DockingPort.GetState();
                    foupInfo.DockingPort40State = DockingPort40.GetState();

                    if (Door == null)
                    {
                        DockingPortDoorNomal dpdn = new DockingPortDoorNomal(this);
                        dpdn.StateInit();
                        Door = dpdn;
                    }
                    foupInfo.DockingPortDoorState = Door.GetState();

                    // Cover.StateInit();
                    foupInfo.FoupCoverState = Cover.GetState();

                    foupInfo.State = ModuleState.State;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return foupInfo;
        }

        private EventCodeEnum CheckModuleState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                _DockingPlate.CheckState();
                _DockingPort.CheckState();
                _Cover.CheckState();
                CassetteOpener.CheckState();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public void SetCallback(IFoupServiceCallback callback)
        {
            this.Callback = callback;
        }
        //public IFoupProcedureManager ProcManager { get; set; }
        //IFoupProcedureManager IFoupModule.FoupProcedureManager { get; set; }

        private CylinderManager cylinderManager;

        public EmulFoupModule()
        {
        }

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                RetVal = LoadTemplate();
                RetVal = this.TemplateManager().InitTemplate(this);

                RetVal = LoadCylinderManagerParam();

                RetVal = LoadFoupLoadUnloadParameter();
                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public void UpdateFoupSequenceCaption()
        {
            try
            {
                if (System.AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")
                {
                    ILoaderModule loadermodule = this.GetLoaderContainer().Resolve<ILoaderModule>();

                    if (loadermodule.GetDefaultWaferSize() == SubstrateSizeEnum.INCH6 || loadermodule.GetDefaultWaferSize() == SubstrateSizeEnum.INCH8)
                    {
                        var item = (FoupProcedure)FoupLoadUnloadParam.FoupProcedureList.Find(i => i.Behavior.GetType().Name == "FoupCassetteOpener_Unlock");
                        item.Caption = "DOCKED VALIDATION";
                        var item2 = (FoupProcedure)FoupLoadUnloadParam.FoupProcedureList.Find(i => i.Behavior.GetType().Name == "FoupCassetteOpener_Lock");
                        item2.Caption = "COVER CLOSED VALIDATION";
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
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

        private EventCodeEnum LoadFoupLoadUnloadParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

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

                    if (RetVal == EventCodeEnum.NONE)
                    {
                        FoupLoadUnloadParam = tmpParam as FoupLoadUnloadParam;
                    }

                    RetVal = EventCodeEnum.NONE;
                }
                catch (Exception err)
                {
                    RetVal = EventCodeEnum.PARAM_ERROR;
                    LoggerManager.Error(String.Format("[FoupModule] FoupLoadUnloadParam(): Error occurred while loading parameters. Err = {0}", err.Message));
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

        private EventCodeEnum LoadCylinderManagerParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                #region CylinderManager...

                cylinderManager = new CylinderManager();
                cylinderManager.Cylinders = new FoupCylinderType();

                string FullPath = this.FileManager().GetSystemParamFullPath("Cylinder", "FoupCylinderIOParameter.Json");

                CylinderParams Params;
                List<IOPortDescripter<bool>> ioPortDescripterList = new List<IOPortDescripter<bool>>();

                if (Directory.Exists(Path.GetDirectoryName(FullPath)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(FullPath));
                }

                BindingFlags _BindFlags = BindingFlags.Public | BindingFlags.Instance;

                // Inputs
                foreach (PropertyInfo propInfo in IOManager.IOMap.Inputs.GetType().GetProperties(_BindFlags))
                {
                    Object paramValue = propInfo.GetValue(IOManager.IOMap.Inputs);
                    if (paramValue == null)
                        continue;

                    if (ReflectionUtil.IsObjectAssignable(paramValue.GetType(), typeof(IOPortDescripter<bool>)))
                    {
                        ioPortDescripterList.Add((IOPortDescripter<bool>)paramValue);
                    }
                }

                // Outputs
                foreach (PropertyInfo propInfo in IOManager.IOMap.Outputs.GetType().GetProperties(_BindFlags))
                {
                    Object paramValue = propInfo.GetValue(IOManager.IOMap.Outputs);
                    if (paramValue == null)
                        continue;

                    if (ReflectionUtil.IsObjectAssignable(paramValue.GetType(), typeof(IOPortDescripter<bool>)))
                    {
                        ioPortDescripterList.Add((IOPortDescripter<bool>)paramValue);
                    }
                }


                if (File.Exists(FullPath) == false)
                {
                    Params = SetDefaultFoupCylinderParam();
                    cylinderManager.LoadParameter(FullPath, ioPortDescripterList, Params);
                }
                else
                {
                    cylinderManager.LoadParameter(FullPath, ioPortDescripterList);
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
                        IOManager.IOMap.Outputs.DO_C12IN_LOCK_AIR.Key.Value,
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
                        IOManager.IOMap.Outputs.DO_CP_CYL_IN_AIR.Key.Value,
                        IOManager.IOMap.Outputs.DO_CP_CYL_OUT_AIR.Key.Value,
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
                        IOManager.IOMap.Outputs.DO_CP_40_CYL_IN_AIR.Key.Value,
                        IOManager.IOMap.Outputs.DO_CP_40_CYL_OUT_AIR.Key.Value,
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
                        IOManager.IOMap.Outputs.DO_FO_ROTATOR_CLOSE_AIR.Key.Value,
                        IOManager.IOMap.Outputs.DO_FO_ROTATOR_OPEN_AIR.Key.Value,
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
                        IOManager.IOMap.Outputs.DO_FO_UP_AIR.Key.Value,
                        IOManager.IOMap.Outputs.DO_FO_DOWN_AIR.Key.Value,
                        Extand_In.ToList(),
                        Retract_In.ToList())
                    );
                Extand_In.Clear();
                Retract_In.Clear();

                // FoupCassetteTiltting
                Extand_In.Add(IOManager.IOMap.Inputs.DI_CSTT_UP.Key.Value);
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

        public EventCodeEnum ChangeDevice(FoupDeviceParam param)
        {
            EventCodeEnum retVal;
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

        public EventCodeEnum CassetteTypeAvailable(CassetteTypeEnum cassetteType)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                var ret = CassetteConfigurationParam.CassetteModules.Any(c => c.CassetteType.Value == cassetteType && c.Enable.Value == true);
                if (ret)
                {
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }

        public CassetteTypeEnum GetCassetteType()
        {
            CassetteTypeEnum cassetteTypeEnum = CassetteTypeEnum.UNDEFINED;
            try
            {
                cassetteTypeEnum = SystemParam.CassetteType.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return cassetteTypeEnum;
        }
        public EventCodeEnum ValidationCassetteAvailable(CassetteTypeEnum cassetteType, out string msg)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            msg = "";
            try
            {
                retVal = CassetteTypeAvailable(cassetteType);
                if (retVal == EventCodeEnum.NONE)
                {
                    var CassetteConfiguration = CassetteConfigurationParam.CassetteModules.Where(c => c.CassetteType.Value == cassetteType && c.Enable.Value == true).FirstOrDefault();

                    if (CassetteConfiguration != null)
                    {
                        if (CassetteConfiguration.CheckCondition != null &&
                            CassetteConfiguration.CheckCondition.Count > 0)
                        {
                            List<string> trueKeys = CassetteConfiguration.CheckCondition.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
                            List<bool> isOn = new List<bool>();
                            if (trueKeys != null && trueKeys.Count > 0)
                            {
                                foreach (var key in trueKeys)
                                {
                                    string sensor = key + $".{FoupIndex}";
                                    if (this.IOManager.GetIOPortDescripter(sensor) == null)
                                    {
                                        msg = $"Attempted to detect a sensor({key}) that does not exist in the parameter.";
                                        return EventCodeEnum.UNKNOWN_EXCEPTION;
                                    }
                                    else
                                    {
                                        isOn.Add(this.IOManager.GetIOPortDescripter(sensor).Value);
                                    }
                                }
                                var combinedLog = string.Join(",\n", trueKeys.Zip(isOn, (key, state) => $"IO: {key}, Value: {state}"));
                                if (isOn.Where(kvp => kvp == false).ToList() != null && isOn.Where(kvp => kvp == false).ToList().Count() > 0)
                                {
                                    // Error 라는 뜻
                                    retVal = EventCodeEnum.SENSOR_DISCONNECTED;
                                    msg = $"Some of the sensors that should be detected are not sensing the input.\n{combinedLog}";
                                }
                                else
                                {
                                    retVal = EventCodeEnum.NONE;
                                    //성공 정상
                                }
                            }
                            else
                            {
                                retVal = EventCodeEnum.NONE;
                                LoggerManager.Debug($"ValidationCassetteAvailable() Foup#{FoupNumber}. None of the configured sensors are in use.");
                                //Error 는 아니지만 로그는 남겨줘라
                            }
                        }
                        else
                        {
                            retVal = EventCodeEnum.PARAM_ERROR;
                            msg = $"Feature is enabled, but no valid conditions are set.";
                            // 기능 사용이 ON 이지만 조건이 없음 
                        }
                    }
                    else
                    {
                        retVal = EventCodeEnum.PARAM_ERROR;
                        msg = "Feature is not enabled.";
                        // 기능 사용이 OFF 로 되어 있음
                    }
                }
                else
                {
                    retVal = EventCodeEnum.PARAM_ERROR;
                    msg = "Feature usage is not supported. This means the parameter settings are not configured.";
                }
                LoggerManager.Debug($"ValidationCassetteAvailable() Foup#{FoupNumber}. {msg}. {retVal}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                msg = "An unknown error has occurred.";
                LoggerManager.Debug($"ValidationCassetteAvailable() Foup#{FoupNumber}. An unknown error has occurred.");
            }

            return retVal;
        }

        public EventCodeEnum InitState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Permission = new FoupPermissionEveryOneState(this);

                if (Template != null && SystemManager.SysteMode == SystemModeEnum.Single)    // 템플레이트 로딩 상태 확인
                {
                    DockingPlate = (IFoupDockingPlate)Template.EntryModules.FirstOrDefault(t => t is IFoupDockingPlate);

                    if (DockingPlate == null)
                    {
                        LoggerManager.Debug($"FOUP: DockingPlate Sub-module is not available.");
                    }
                    else
                    {
                        if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                        {
                            DockingPlate.EnumState = DockingPlateStateEnum.LOCK;
                        }

                        ((FoupDockingPlateBase)DockingPlate).Module = this;
                        retVal = DockingPlate.StateInit();



                        foreach (var input in DockingPlate.Inputs)
                        {
                            Inputs.Add(input);
                        }
                        foreach (var output in DockingPlate.Outputs)
                        {
                            Outputs.Add(output);
                        }

                        if (retVal != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"FOUP: DockingPlate StateInit is failed.");
                        }
                    }

                    DockingPort = (IFoupDockingPort)Template.EntryModules.FirstOrDefault(t => t is IFoupDockingPort);
                    if (DockingPort == null)
                    {
                        LoggerManager.Debug($"FOUP: DockingPort Sub-module is not available.");
                    }
                    else
                    {
                        ((FoupDockingPortBase)DockingPort).Module = this;
                        retVal = DockingPort.StateInit();

                        if (retVal != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"FOUP: DockingPort StateInit is failed.");
                        }
                        foreach (var input in DockingPort.Inputs)
                        {
                            Inputs.Add(input);
                        }
                        foreach (var output in DockingPort.Outputs)
                        {
                            Outputs.Add(output);
                        }
                    }

                    DockingPort40 = (IFoupDockingPort40)Template.EntryModules.FirstOrDefault(t => t is IFoupDockingPort40);
                    if (DockingPort40 == null)
                    {
                        LoggerManager.Debug($"FOUP: DockingPort40 Sub-module is not available.");
                    }
                    else
                    {
                        ((DockingPort40Base)DockingPort40).Module = this;
                        retVal = DockingPort40.StateInit();

                        if (retVal != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"FOUP: DockingPort40 StateInit is failed.");
                        }

                    }

                    Cover = (IFoupCover)Template.EntryModules.FirstOrDefault(t => t is IFoupCover);
                    if (Cover == null)
                    {
                        LoggerManager.Debug($"FOUP: Cover Sub-module is not available.");
                    }
                    else
                    {
                        ((FoupCoverBase)Cover).Module = this;
                        retVal = Cover.StateInit();

                        if (retVal != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"FOUP: Cover StateInit is failed.");
                        }
                        foreach (var input in Cover.Inputs)
                        {
                            Inputs.Add(input);
                        }
                        foreach (var output in Cover.Outputs)
                        {
                            Outputs.Add(output);
                        }
                    }

                    CassetteOpener = (IFoupCassetteOpener)Template.EntryModules.FirstOrDefault(t => t is IFoupCassetteOpener);
                    if (CassetteOpener == null)
                    {
                        LoggerManager.Debug($"FOUP: FoupCassetteOpener Sub-module is not available.");
                    }
                    else
                    {
                        ((FoupOpenerBase)CassetteOpener).Module = this;
                        retVal = CassetteOpener.StateInit();

                        if (retVal != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"FOUP: CassetteOpener StateInit is failed.");
                        }
                        foreach (var input in CassetteOpener.Inputs)
                        {
                            Inputs.Add(input);
                        }
                        foreach (var output in CassetteOpener.Outputs)
                        {
                            Outputs.Add(output);
                        }
                    }

                    Door = (IFoupDoor)Template.EntryModules.FirstOrDefault(t => t is IFoupDoor);
                    if (Door == null)
                    {
                        LoggerManager.Debug($"FOUP: Door Sub-module is not available. Apply default sub-module.");
                        Door = new DockingPortDoorNomal(this);

                        if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                        {
                            Door.EnumState = DockingPortDoorStateEnum.OPEN;
                        }

                        retVal = Door.StateInit();

                        if (retVal != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"FOUP: CassetteOpener StateInit is failed.");
                        }
                    }
                    else
                    {
                        ((DockingPortDoorBase)Door).Module = this;
                        retVal = Door.StateInit();

                        if (retVal != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"FOUP: CassetteOpener StateInit is failed.");
                        }
                    }

                    Tilt = (IFoupTilt)Template.EntryModules.FirstOrDefault(t => t is IFoupTilt);
                    if (Tilt == null)
                    {
                        LoggerManager.Debug($"FOUP: Tilt Sub-module is not available.");
                    }
                    else
                    {
                        if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                        {
                            Tilt.EnumState = TiltStateEnum.DOWN;
                        }

                        ((FoupTiltBase)Tilt).Module = this;
                        retVal = Tilt.StateInit();

                        if (retVal != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"FOUP: Tilt StateInit is failed.");
                        }
                    }

                    retVal = UpdateFoupState();
                }
                else if (SystemManager.SysteMode == SystemModeEnum.Multiple) 
                {
                    retVal = EventCodeEnum.NONE;
                }
                BroadcastFoupStateAsync();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
        public EventCodeEnum ErrorClear()
        {
            EventCodeEnum retVal;

            try
            {
                _DockingPlate.StateInit();
                _DockingPort.StateInit();
                _DockingPort40.StateInit();
                _Door.StateInit();
                _Cover.StateInit();
                _CassetteOpener.StateInit();
                _Tilt.StateInit();
                UpdateFoupState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            retVal = EventCodeEnum.NONE;

            return retVal;
        }
        public EventCodeEnum SetLoaderBusy(bool isEnable)
        {
            EventCodeEnum retVal;

            try
            {
                if (isEnable)
                {
                    //퍼미션 상태 변경.
                    UpdateFoupLampState();
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    ErrorClear();
                    UpdateFoupState();
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        #region => Lamp State
        public bool[] LampState = new bool[8];
        /// <summary>
        /// ******LAMP STATE******
        ///LampState[0] - PRESENCE [Cassette가 DockingPort에 올라온 상태]
        ///LampState[1] - PLACEMENT [Cassette가 올라와서 Lock채워진 상태]
        ///LampState[2] - UNLOAD [Docking Port가 Move Out 상태]
        ///LampState[3] - LOAD [Docking Port가 Move In 상태]
        ///LampState[4] - AUTO [Lot Run 상태]
        ///LampState[5] - RESERVE [ROT Oprn 상태 -> CoverDetect and Rot Close중에 점멸]
        ///LampState[6] - BUSY [Loader가 작동 중인 상태 -> 점등 중에 Docking Port 작동 불가]
        ///LampState[7] - ALARM [작동에 문제가 생긴 상태 -> 점등 중에 Docking Port 작동 불가] 
        /// </summary>
        #endregion

        public void UpdateFoupLampState()
        {
            EventCodeEnum retVal;

            // TODO : 데드락 발생 경우가 있어, 임시로 동작 안하게끔 처리 해놓음
            return;
#pragma warning disable 0162

            try
            {
                if (SystemParam.FoupType.Value == FoupTypeEnum.TOP)
                {
                    if (DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH6)
                    {
                        retVal = UpdateFoupState();
                    }
                    else if (DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH8)
                    {
                        //retVal = UpdateFoupState();
                    }
                    else if (DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH12)
                    {
                        for (int i = 0; i < LampState.Length; i++)
                        {
                            LampState[i] = false;
                        }

                        //=> update lamp state from module info
                        bool value;
                        bool value2;
                        bool value3;
                        //bool value4;
                        if (_ModuleState.State == FoupStateEnum.LOAD)
                        {
                            LampState[2] = false;
                            LampState[3] = true;
                            LampState[7] = false; //alram
                        }
                        else if (_ModuleState.State == FoupStateEnum.UNLOAD)
                        {
                            LampState[2] = true;
                            LampState[3] = false;
                            LampState[7] = false; //alram
                        }
                        else // error, illegal
                        {
                            LampState[2] = false;
                            LampState[3] = false;
                            LampState[7] = true; //alram
                        }
                        IOManager.ReadBit(IOManager.IOMap.Inputs.DI_C12IN_PRESENCE1, out value);
                        IOManager.ReadBit(IOManager.IOMap.Inputs.DI_C12IN_PRESENCE2, out value2);
                        IOManager.ReadBit(IOManager.IOMap.Inputs.DI_C12IN_NPLACEMENT, out value3); // true면 Unlock상태

                        if (value == false)
                        {
                            LampState[0] = false;
                            LampState[1] = false;
                        }
                        else if (value == true)
                        {
                            if (value3 == false)
                            {
                                LampState[0] = true;
                                LampState[1] = false;
                            }
                            else
                            {
                                LampState[0] = true;
                                LampState[1] = true;
                            }
                        }

                        if (_Permission.GetState() == FoupPermissionStateEnum.AUTO)
                        {
                            LampState[4] = true; //auto
                            LampState[6] = false; //busy
                        }
                        else if (_Permission.GetState() == FoupPermissionStateEnum.BUSY)
                        {
                            LampState[4] = false; //auto
                            LampState[6] = true; //busy
                        }
                        else
                        {
                            LampState[4] = false; //auto
                            LampState[6] = false; //busy
                        }
                        LampState[5] = false; //reserve

                        // System LAMP Value
                        foupInfo.PresenceLamp = LampState[0];
                        foupInfo.PlacementLamp = LampState[1];
                        foupInfo.LoadLamp = LampState[3];
                        foupInfo.UnloadLamp = LampState[2];
                        foupInfo.AutoLamp = LampState[4];
                        foupInfo.BusyLamp = LampState[6];
                        foupInfo.AlarmLamp = LampState[7];


                        //=> Wirte bits
                        for (int i = 0; i < 8; i++)
                        {
                            IOManager.WriteBit(IOManager.IOMap.Outputs.DO_SERIAL, !LampState[i]);
                            IOManager.WriteBit(IOManager.IOMap.Outputs.DO_CLK, true);
                            IOManager.WriteBit(IOManager.IOMap.Outputs.DO_CLK, false);
                        }

                        IOManager.WriteBit(IOManager.IOMap.Outputs.DO_LATCH, true);
                        IOManager.WriteBit(IOManager.IOMap.Outputs.DO_LATCH, false);
                        //retVal = UpdateFoupState();
                    }
                    else
                    {

                        retVal = EventCodeEnum.FOUP_PARAM_ERROR;
                    }
                }
                else if (SystemParam.FoupType.Value == FoupTypeEnum.FLAT)
                {
                    if (this.SystemParam == null || this.DeviceParam == null)
                    {
                        ModuleState = new FoupErrorState(this);

                        retVal = EventCodeEnum.NONE;
                    }
                    else if (DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH6)
                    {


                        retVal = EventCodeEnum.NONE;
                    }
                    else if (DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH8)
                    {
                        for (int i = 0; i < LampState.Length; i++)
                        {
                            LampState[i] = false;
                        }

                        //=> update lamp state from module info
                        bool value;
                        bool value2;
                        bool value3;
                        //bool value4;
                        if (_ModuleState.State == FoupStateEnum.LOAD)
                        {
                            LampState[2] = false;
                            LampState[3] = true;
                            LampState[7] = false; //alram
                        }
                        else if (_ModuleState.State == FoupStateEnum.UNLOAD)
                        {
                            LampState[2] = true;
                            LampState[3] = false;
                            LampState[7] = false; //alram
                        }
                        else // error, illegal
                        {
                            LampState[2] = false;
                            LampState[3] = false;
                            LampState[7] = true; //alram
                        }
                        IOManager.ReadBit(IOManager.IOMap.Inputs.DI_C12IN_PRESENCE1, out value);
                        IOManager.ReadBit(IOManager.IOMap.Inputs.DI_C12IN_PRESENCE2, out value2);
                        IOManager.ReadBit(IOManager.IOMap.Inputs.DI_C12IN_NPLACEMENT, out value3); // true면 Unlock상태

                        if (value == false)
                        {
                            LampState[0] = false;
                            LampState[1] = false;
                        }
                        else if (value == true)
                        {
                            if (value3 == false)
                            {
                                LampState[0] = true;
                                LampState[1] = false;
                            }
                            else
                            {
                                LampState[0] = true;
                                LampState[1] = true;
                            }
                        }

                        if (_Permission.GetState() == FoupPermissionStateEnum.AUTO)
                        {
                            LampState[4] = true; //auto
                            LampState[6] = false; //busy
                        }
                        else if (_Permission.GetState() == FoupPermissionStateEnum.BUSY)
                        {
                            LampState[4] = false; //auto
                            LampState[6] = true; //busy
                        }
                        else
                        {
                            LampState[4] = false; //auto
                            LampState[6] = false; //busy
                        }
                        LampState[5] = false; //reserve

                        // System LAMP Value
                        foupInfo.PresenceLamp = LampState[0];
                        foupInfo.PlacementLamp = LampState[1];
                        foupInfo.LoadLamp = LampState[3];
                        foupInfo.UnloadLamp = LampState[2];
                        foupInfo.AutoLamp = LampState[4];
                        foupInfo.BusyLamp = LampState[6];
                        foupInfo.AlarmLamp = LampState[7];


                        //=> Wirte bits
                        for (int i = 0; i < 8; i++)
                        {
                            IOManager.WriteBit(IOManager.IOMap.Outputs.DO_SERIAL, !LampState[i]);
                            IOManager.WriteBit(IOManager.IOMap.Outputs.DO_CLK, true);
                            IOManager.WriteBit(IOManager.IOMap.Outputs.DO_CLK, false);
                        }

                        IOManager.WriteBit(IOManager.IOMap.Outputs.DO_LATCH, true);
                        IOManager.WriteBit(IOManager.IOMap.Outputs.DO_LATCH, false);
                        //retVal = UpdateFoupState();
                    }
                    else if (DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH12)
                    {
                        for (int i = 0; i < LampState.Length; i++)
                        {
                            LampState[i] = false;
                        }

                        //=> update lamp state from module info
                        bool value;
                        bool value2;
                        bool value3;
                        //bool value4;
                        if (_ModuleState.State == FoupStateEnum.LOAD)
                        {
                            LampState[2] = false;
                            LampState[3] = true;
                            LampState[7] = false; //alram
                        }
                        else if (_ModuleState.State == FoupStateEnum.UNLOAD)
                        {
                            LampState[2] = true;
                            LampState[3] = false;
                            LampState[7] = false; //alram
                        }
                        else // error, illegal
                        {
                            LampState[2] = false;
                            LampState[3] = false;
                            LampState[7] = true; //alram
                        }
                        IOManager.ReadBit(IOManager.IOMap.Inputs.DI_C12IN_PRESENCE1, out value);
                        IOManager.ReadBit(IOManager.IOMap.Inputs.DI_C12IN_PRESENCE2, out value2);
                        IOManager.ReadBit(IOManager.IOMap.Inputs.DI_C12IN_NPLACEMENT, out value3); // true면 Unlock상태

                        if (value == false)
                        {
                            LampState[0] = false;
                            LampState[1] = false;
                        }
                        else if (value == true)
                        {
                            if (value3 == false)
                            {
                                LampState[0] = true;
                                LampState[1] = false;
                            }
                            else
                            {
                                LampState[0] = true;
                                LampState[1] = true;
                            }
                        }

                        if (_Permission.GetState() == FoupPermissionStateEnum.AUTO)
                        {
                            LampState[4] = true; //auto
                            LampState[6] = false; //busy
                        }
                        else if (_Permission.GetState() == FoupPermissionStateEnum.BUSY)
                        {
                            LampState[4] = false; //auto
                            LampState[6] = true; //busy
                        }
                        else
                        {
                            LampState[4] = false; //auto
                            LampState[6] = false; //busy
                        }
                        LampState[5] = false; //reserve

                        // System LAMP Value
                        foupInfo.PresenceLamp = LampState[0];
                        foupInfo.PlacementLamp = LampState[1];
                        foupInfo.LoadLamp = LampState[3];
                        foupInfo.UnloadLamp = LampState[2];
                        foupInfo.AutoLamp = LampState[4];
                        foupInfo.BusyLamp = LampState[6];
                        foupInfo.AlarmLamp = LampState[7];


                        //=> Wirte bits
                        for (int i = 0; i < 8; i++)
                        {
                            IOManager.WriteBit(IOManager.IOMap.Outputs.DO_SERIAL, !LampState[i]);
                            IOManager.WriteBit(IOManager.IOMap.Outputs.DO_CLK, true);
                            IOManager.WriteBit(IOManager.IOMap.Outputs.DO_CLK, false);
                        }

                        IOManager.WriteBit(IOManager.IOMap.Outputs.DO_LATCH, true);
                        IOManager.WriteBit(IOManager.IOMap.Outputs.DO_LATCH, false);
                        //retVal = UpdateFoupState();
                    }
                    else
                    {

                        retVal = EventCodeEnum.FOUP_PARAM_ERROR;
                    }
                }
                else if (SystemParam.FoupType.Value == FoupTypeEnum.CST8PORT_FLAT)
                {
                    if (DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH6)
                    {
                        for (int i = 0; i < LampState.Length; i++)
                        {
                            LampState[i] = false;
                        }

                        //=> update lamp state from module info
                        bool value;
                        bool value2;
                        bool value3;
                        //bool value4;
                        bool value5;
                        if (_ModuleState.State == FoupStateEnum.LOAD)
                        {
                            LampState[2] = false;
                            LampState[3] = true;
                            LampState[7] = false; //alram
                        }
                        else if (_ModuleState.State == FoupStateEnum.UNLOAD)
                        {
                            LampState[2] = true;
                            LampState[3] = false;
                            LampState[7] = false; //alram
                        }
                        else // error, illegal
                        {
                            LampState[2] = false;
                            LampState[3] = false;
                            LampState[7] = true; //alram
                        }

                        // TODO : 6인치 Presense1 쓰는 코드 삭제 되어야 함.
                        IOManager.ReadBit(IOManager.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE1, out value);
                        IOManager.ReadBit(IOManager.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE2, out value2);
                        IOManager.ReadBit(IOManager.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE3, out value5);
                        IOManager.ReadBit(IOManager.IOMap.Inputs.DI_C6IN_C8IN_NPLACEMENT, out value3); // true면 Unlock상태

                        if (value == false)
                        {
                            LampState[0] = false;
                            LampState[1] = false;
                        }
                        else if (value == true)
                        {
                            if (value3 == false)
                            {
                                LampState[0] = true;
                                LampState[1] = true;
                            }
                            else
                            {
                                LampState[0] = true;
                                LampState[1] = false;
                            }
                        }

                        if (_Permission.GetState() == FoupPermissionStateEnum.AUTO)
                        {
                            LampState[4] = true; //auto
                            LampState[6] = false; //busy
                        }
                        else if (_Permission.GetState() == FoupPermissionStateEnum.BUSY)
                        {
                            LampState[4] = false; //auto
                            LampState[6] = true; //busy
                        }
                        else
                        {
                            LampState[4] = false; //auto
                            LampState[6] = false; //busy
                        }
                        LampState[5] = false; //reserve

                        // System LAMP Value
                        foupInfo.PresenceLamp = LampState[0];
                        foupInfo.PlacementLamp = LampState[1];
                        foupInfo.LoadLamp = LampState[3];
                        foupInfo.UnloadLamp = LampState[2];
                        foupInfo.AutoLamp = LampState[4];
                        foupInfo.BusyLamp = LampState[6];
                        foupInfo.AlarmLamp = LampState[7];
                        retVal = EventCodeEnum.NONE;
                    }
                    else if (DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH8)
                    {
                        for (int i = 0; i < LampState.Length; i++)
                        {
                            LampState[i] = false;
                        }

                        //=> update lamp state from module info
                        //bool value;
                        bool value2;
                        bool value3;
                        //bool value4;
                        bool value5;
                        if (_ModuleState.State == FoupStateEnum.LOAD)
                        {
                            LampState[2] = false;
                            LampState[3] = true;
                            LampState[7] = false; //alram
                        }
                        else if (_ModuleState.State == FoupStateEnum.UNLOAD)
                        {
                            LampState[2] = true;
                            LampState[3] = false;
                            LampState[7] = false; //alram
                        }
                        else // error, illegal
                        {
                            LampState[2] = false;
                            LampState[3] = false;
                            LampState[7] = true; //alram
                        }
                        //IOManager.ReadBit(IOManager.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE1, out value);
                        IOManager.ReadBit(IOManager.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE2, out value2);
                        IOManager.ReadBit(IOManager.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE3, out value5);
                        IOManager.ReadBit(IOManager.IOMap.Inputs.DI_C6IN_C8IN_NPLACEMENT, out value3); // true면 Unlock상태

                        if (value2 == false && value5 == false)
                        {
                            LampState[0] = false;
                            LampState[1] = false;
                        }
                        else if (value2 == true && value5 == true)
                        {
                            if (value3 == false)
                            {
                                LampState[0] = true;
                                LampState[1] = true;
                            }
                            else
                            {
                                LampState[0] = true;
                                LampState[1] = false;
                            }
                        }

                        if (_Permission.GetState() == FoupPermissionStateEnum.AUTO)
                        {
                            LampState[4] = true; //auto
                            LampState[6] = false; //busy
                        }
                        else if (_Permission.GetState() == FoupPermissionStateEnum.BUSY)
                        {
                            LampState[4] = false; //auto
                            LampState[6] = true; //busy
                        }
                        else
                        {
                            LampState[4] = false; //auto
                            LampState[6] = false; //busy
                        }
                        LampState[5] = false; //reserve

                        // System LAMP Value
                        foupInfo.PresenceLamp = LampState[0];
                        foupInfo.PlacementLamp = LampState[1];
                        foupInfo.LoadLamp = LampState[3];
                        foupInfo.UnloadLamp = LampState[2];
                        foupInfo.AutoLamp = LampState[4];
                        foupInfo.BusyLamp = LampState[6];
                        foupInfo.AlarmLamp = LampState[7];
                        retVal = EventCodeEnum.NONE;


                    }
                    else if (DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH12)
                    {
                        retVal = EventCodeEnum.FOUP_PARAM_ERROR;
                    }
                    else
                    {
                        retVal = EventCodeEnum.FOUP_PARAM_ERROR;

                    }
                }
                else
                {
                    if (GPLoader != null)
                    {
                        // EMUL에서 지원 안됨.
                        //GPLoader.CheckIOValue(IOManager.IOMap.Outputs.DO_CST_IND_ALARMs[FoupIndex]);
                        //GPLoader.CheckIOValue(IOManager.IOMap.Outputs.DO_CST_IND_BUSYs[FoupIndex]);
                        //GPLoader.CheckIOValue(IOManager.IOMap.Outputs.DO_CST_IND_RESERVEDs[FoupIndex]);
                        //GPLoader.CheckIOValue(IOManager.IOMap.Outputs.DO_CST_IND_AUTOs[FoupIndex]);
                        //GPLoader.CheckIOValue(IOManager.IOMap.Outputs.DO_CST_IND_LOADs[FoupIndex]);
                        //GPLoader.CheckIOValue(IOManager.IOMap.Outputs.DO_CST_IND_UNLOADs[FoupIndex]);
                        //GPLoader.CheckIOValue(IOManager.IOMap.Outputs.DO_CST_IND_PLACEMENTs[FoupIndex]);
                        //GPLoader.CheckIOValue(IOManager.IOMap.Outputs.DO_CST_IND_PRESENCEs[FoupIndex]);

                        foupInfo.PresenceLamp = IOManager.IOMap.Outputs.DO_CST_IND_PRESENCEs[FoupIndex].Value;
                        foupInfo.PlacementLamp = IOManager.IOMap.Outputs.DO_CST_IND_PLACEMENTs[FoupIndex].Value;
                        foupInfo.ReserveLamp = IOManager.IOMap.Outputs.DO_CST_IND_RESERVEDs[FoupIndex].Value;
                        foupInfo.LoadLamp = IOManager.IOMap.Outputs.DO_CST_IND_LOADs[FoupIndex].Value;
                        foupInfo.UnloadLamp = IOManager.IOMap.Outputs.DO_CST_IND_UNLOADs[FoupIndex].Value;
                        foupInfo.AutoLamp = IOManager.IOMap.Outputs.DO_CST_IND_AUTOs[FoupIndex].Value;
                        foupInfo.BusyLamp = IOManager.IOMap.Outputs.DO_CST_IND_BUSYs[FoupIndex].Value;
                        foupInfo.AlarmLamp = IOManager.IOMap.Outputs.DO_CST_IND_ALARMs[FoupIndex].Value;
                    }

                    retVal = EventCodeEnum.NONE;

                }
                //=> init state to false
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

#pragma warning restore 0162
        }

        public void FoupModuleStateTransition(FoupModuleStateBase FoupModuleState)
        {
            this.ModuleState = FoupModuleState;
        }

        public EventCodeEnum InitModule(int foupNumber, FoupSystemParam systemParam, FoupDeviceParam deviceParam, CassetteConfigurationParameter CassetteConfigurationParam)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {

                try
                {
                    if (Initialized == false)
                    {
                        this.FoupNumber = foupNumber;
                        FoupIndex = FoupNumber - 1;

                        this.SystemParam = systemParam;
                        this.DeviceParam = deviceParam;
                        this.CassetteConfigurationParam = CassetteConfigurationParam;

                        IOManager = this.GetFoupIO();

                        if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                        {
                            DockingPlate = new GP_Emul_FOUPDockingPlate(this);
                            DockingPlate.StateInit();

                            foreach (var input in DockingPlate.Inputs)
                            {
                                Inputs.Add(input);
                            }

                            foreach (var output in DockingPlate.Outputs)
                            {
                                Outputs.Add(output);
                            }

                            DockingPort = new GP_Emul_FoupDockingPort(this);
                            DockingPort.StateInit();

                            foreach (var input in DockingPort.Inputs)
                            {
                                Inputs.Add(input);
                            }

                            foreach (var output in DockingPort.Outputs)
                            {
                                Outputs.Add(output);
                            }

                            Cover = new GP_Emul_FoupCover12Inch(this);
                            Cover.StateInit();

                            foreach (var input in Cover.Inputs)
                            {
                                Inputs.Add(input);
                            }

                            foreach (var output in Cover.Outputs)
                            {
                                Outputs.Add(output);
                            }

                            CassetteOpener = new GP_Emul_FoupOpener(this);
                            CassetteOpener.StateInit();

                            foreach (var input in CassetteOpener.Inputs)
                            {
                                Inputs.Add(input);
                            }

                            foreach (var output in CassetteOpener.Outputs)
                            {
                                Outputs.Add(output);
                            }

                            DockingPort40 = new GP_Emul_DPExt(this);
                            DockingPort40.StateInit();

                            Tilt = new GP_Emul_FoupTilt(this);
                            Tilt.StateInit();
                        }

                        retval = LoadSysParameter();
                        if (retval != EventCodeEnum.NONE)
                        {
                            LoggerManager.Error($"LoadSysParameter() Failed");
                        }
                       
                        ModuleState = new FoupErrorState(this);

                        retval = InitState(); //TODO : ADD UNDEFINED STATE 

                        UpdateFoupSequenceCaption();
                        ProcManager = new EmulFoupProcManager(this);
                        ProcManager.SettingProcedure(FoupLoadUnloadParam.FoupProcedureList,
                                                                FoupLoadUnloadParam.LoadOrder,
                                                                FoupLoadUnloadParam.UnloadOrder,
                                                                this,
                                                                IOManager
                                                                );

                        CylinderManager = cylinderManager;
                        ProcManager = ProcManager;

                        //emul lamp status
                        foupInfo.PresenceLamp = true;
                        foupInfo.PlacementLamp = false;
                        foupInfo.LoadLamp = true;
                        foupInfo.UnloadLamp = false;
                        foupInfo.AutoLamp = true;
                        foupInfo.BusyLamp = true;
                        foupInfo.AlarmLamp = false;

                        foupInfo = GetFoupModuleInfo();

                        foupInfo.PropertyChanged += this.FoupOpModule().GetFoupController(foupNumber).OnChangedFoupInfoFunc;

                        if (SystemManager.SysteMode == SystemModeEnum.Single)
                        {
                            IOManager.IOMap.Inputs.DI_WAFER_OUT.PropertyChanged += DI_WAFER_OUT_PropertyChanged;
                            IOManager.IOMap.Inputs.DI_LOAD_SWITCH.PropertyChanged += DI_LOAD_SWITCH_PropertyChanged;
                            IOManager.IOMap.Inputs.DI_UNLOAD_SWITCH.PropertyChanged += DI_UNLOAD_SWITCH_PropertyChanged;
                            IOManager.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE2.PropertyChanged += DI_C6_C8IN_PRESENCE2_PropertyChanged;
                            IOManager.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE3.PropertyChanged += DI_C6_C8IN_PRESENCE3_PropertyChanged;
                        }

                        Foup6_8Presence1 = IOManager.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE1.Value;
                        Foup6_8Presence2 = IOManager.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE2.Value;
                        Foup6_8Presence3 = IOManager.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE3.Value;

                        if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                        {
                            if (IOManager.IOMap.Inputs.DI_WAFER_OUTs.Count > 0)
                            {
                                IOManager.IOMap.Inputs.DI_WAFER_OUTs[FoupIndex].PropertyChanged += DI_WAFER_OUT_PropertyChanged;
                            }
                            else
                            {
                                LoggerManager.Error($"IO data(DI_WAFER_OUTs) is wrong.");
                            }
                        }

                        if (IOManager.IOMap.Inputs.DI_CST12_PRESs.Count > 0)
                        {
                            IOManager.IOMap.Inputs.DI_CST12_PRESs[FoupIndex].PropertyChanged += DI_CST12IN_PRESENCE_PropertyChanged;
                        }

                        if (IOManager.IOMap.Inputs.DI_CST12_PRES2s.Count > 0)
                        {
                            IOManager.IOMap.Inputs.DI_CST12_PRES2s[FoupIndex].PropertyChanged += DI_CST12IN_PRESENCE_PropertyChanged;
                        }

                        if (IOManager.IOMap.Inputs.DI_CST_Exists != null)
                        {
                            if (IOManager.IOMap.Inputs.DI_CST_Exists.Count > 0)
                            {
                                IOManager.IOMap.Inputs.DI_CST_Exists[FoupIndex].PropertyChanged += DI_CST12IN_PRESENCE_PropertyChanged;
                            }
                        }

                        UpdateFoupState();

                        // 최초 기동 시, FoupLoadState에 맞춰 IO 값을 변경
                        ForceFoupIO();

                        CassetteIDReaderModule = new CassetteIDReaderModule(FoupIndex);
                        retval = CassetteIDReaderModule.LoadSysParameter();

                        if (retval != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"CassetteIDReaderModule LoadSysParameter() Failed");
                        }
                        retval = CassetteIDReaderModule.InitModule();

                        if (CassetteIDReaderModule.CSTIDReader is IRFIDModule)
                        {
                            RFIDModule = CassetteIDReaderModule.CSTIDReader as IRFIDModule;
                        }
                        else if (CassetteIDReaderModule.CSTIDReader is IBarcodeReaderModule)
                        {
                            BarcodeReaderModule = CassetteIDReaderModule.CSTIDReader as IBarcodeReaderModule;
                        }

                        Initialized = true;
                        retval = EventCodeEnum.NONE;

                        var loaderMaster = this.GetLoaderContainer()?.Resolve<ILoaderSupervisor>();

                        if (loaderMaster != null)
                        {
                            if (loaderMaster.ActiveLotInfos.Count > 0 && FoupIndex > 0)
                            {
                                IOManager.IOMap.Inputs.DI_UNLOAD_SWITCH.PropertyChanged += loaderMaster.ActiveLotInfos[FoupIndex].DI_LOAD_SWITCH_PropertyChanged;
                            }
                        }



                    }
                    else
                    {
                        LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                        retval = EventCodeEnum.DUPLICATE_INVOCATION;
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Debug("Error occurred.");
                    LoggerManager.Exception(err);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }

        private void ForceFoupIO()
        {

            try
            {
                var presence1 = this.GetFoupIO().IOMap.Inputs.DI_CST12_PRESs[FoupIndex];
                var presence2 = this.GetFoupIO().IOMap.Inputs.DI_CST12_PRES2s[FoupIndex];
                var cstExist = this.GetFoupIO().IOMap.Inputs.DI_CST_Exists[FoupIndex];

                if (this.ModuleState.State == FoupStateEnum.LOAD)
                {
                    presence1.ForcedIO.IsForced = true;
                    presence1.ForcedIO.ForecedValue = true;

                    presence2.ForcedIO.IsForced = true;
                    presence2.ForcedIO.ForecedValue = true;

                    cstExist.ForcedIO.IsForced = true;
                    cstExist.ForcedIO.ForecedValue = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
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

        public EventCodeEnum Connect()
        {
            LoggerManager.Debug($"Foup emulator is online.");
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum Disconnect()
        {
            return EventCodeEnum.NONE;
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

        private void DI_WAFER_OUT_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName == "Value")
                {
                    var val = IOManager.IOMap.Inputs.DI_WAFER_OUT.Value;

                    if (SystemParam.FoupType.Value == FoupTypeEnum.UNDEFINED)
                    {
                        val = IOManager.IOMap.Inputs.DI_WAFER_OUTs[FoupIndex].Value;
                    }

                    //TODO : 
                    if (val == true)
                    {
                        var info = GetFoupModuleInfo();

                        if (info.FoupCoverState == FoupCoverStateEnum.OPEN)
                        {
                            Callback.RaiseWaferOutDetected();
                        }

                        foupInfo.WaferOutSensor = FoupWaferOutSensorStateEnum.Detected;
                    }
                    else
                    {
                        foupInfo.WaferOutSensor = FoupWaferOutSensorStateEnum.NotDetected;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void DI_C12IN_PRESENCE_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName == "Value")
                {
                    var val = IOManager.IOMap.Inputs.DI_C12IN_PRESENCE1.Value;
                    var val2 = IOManager.IOMap.Inputs.DI_C12IN_PRESENCE2.Value;

                    if (SystemParam.FoupType.Value == FoupTypeEnum.UNDEFINED)
                    {
                        val = IOManager.IOMap.Inputs.DI_CST12_PRESs[FoupIndex].Value;
                        val2 = IOManager.IOMap.Inputs.DI_CST12_PRES2s[FoupIndex].Value;
                    }

                    if (val && val2 == true)
                    {
                        UpdateFoupLampState();

                        this.EventManager().RaisingEvent(typeof(FoupDockEvent).FullName);
                    }
                    else if (val == false)
                    {
                        UpdateFoupLampState();

                        this.EventManager().RaisingEvent(typeof(CarrierRemovedEvent).FullName);
                        this.EventManager().RaisingEvent(typeof(FoupReadyToLoadEvent).FullName);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void DI_C6_C8IN_PRESENCE1_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName == "Value")
                {
                    var val = IOManager.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE1.Value;

                    if (SystemParam.FoupType.Value == FoupTypeEnum.UNDEFINED)
                    {
                        val = IOManager.IOMap.Inputs.DI_CST08_PRESs[FoupIndex].Value;
                    }

                    if (val == true)
                    {
                        Foup6_8Presence1 = true;
                    }
                    else if (val == false)
                    {
                        Foup6_8Presence1 = false;
                    }

                    FoupPresenceStateChange();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void DI_C6_C8IN_PRESENCE2_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName == "Value")
                {
                    var val = IOManager.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE2.Value;

                    if (SystemParam.FoupType.Value == FoupTypeEnum.UNDEFINED)
                    {
                        val = IOManager.IOMap.Inputs.DI_CST08_PRES_2s[FoupIndex].Value;
                    }

                    if (val == true)
                    {
                        Foup6_8Presence2 = true;
                    }
                    else if (val == false)
                    {
                        Foup6_8Presence2 = false;
                    }

                    FoupPresenceStateChange();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void DI_C6_C8IN_PRESENCE3_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName == "Value")
                {
                    var val = IOManager.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE3.Value;

                    if (val == true)
                    {
                        Foup6_8Presence3 = true;
                    }
                    else if (val == false)
                    {
                        Foup6_8Presence3 = false;
                    }

                    FoupPresenceStateChange();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void DI_CST12IN_PRESENCE_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName == "Value")
                {
                    FoupPresenceStateChange();

                    if (PresenceStateChangedEvent != null)
                    {
                        PresenceStateChangedEvent(FoupIndex + 1, false, false);
                    }

                    UpdateFoupLampState();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void DI_CST_LOCK12s_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName == "Value")
                {
                    if (FoupClampStateChangedEvent != null)
                    {
                        FoupClampStateChangedEvent(FoupIndex + 1, IOManager.IOMap.Inputs.DI_CST_LOCK12s[FoupIndex].Value);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void DI_UNLOAD_SWITCH_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                lock (lockObj)
                {
                    if (Permission.GetState() == FoupPermissionStateEnum.EVERY_ONE)
                    {
                        if (e.PropertyName == "Value")
                        {
                            var val = IOManager.IOMap.Inputs.DI_UNLOAD_SWITCH.Value;

                            if (SystemParam.FoupType.Value == FoupTypeEnum.UNDEFINED)
                            {
                                val = IOManager.IOMap.Inputs.DI_FOUP_UNLOAD_BUTTONs[FoupIndex].Value;
                            }

                            if (val == true)
                            {
                                if (this.ModuleState.State != FoupStateEnum.UNLOAD)
                                {
                                    if (GPLoader != null && !this.GPLoader.IsLoaderBusy)
                                    {
                                        // E84 가 연결되어있고 Auto Mode 인 경우에는 Manual Load/Unload 를 동작하지 못하게 한다.
                                        var e84controller = this.E84Module().GetE84Controller(FoupNumber, E84OPModuleTypeEnum.FOUP);

                                        if (e84controller != null)
                                        {
                                            if (e84controller.CommModule.Connection == E84ComStatus.CONNECTED)
                                            {
                                                if (e84controller.CommModule.RunMode == E84Mode.AUTO)
                                                {
                                                    this.MetroDialogManager().ShowMessageDialog("Error Message", "Manual Load/Unload cannot be operated when E84 is in Auto Mode. \nOperate after changing to Manual Mode.", EnumMessageStyle.Affirmative);

                                                    return;
                                                }
                                                else
                                                {
                                                    LoggerManager.Debug($"Unload() Foup#{FoupNumber}. e84controller runmode is {e84controller.CommModule.RunMode}.");
                                                }
                                            }
                                            else
                                            {
                                                LoggerManager.Debug($"Unload() Foup#{FoupNumber}. e84controller connection state is {e84controller.CommModule.Connection}");
                                            }
                                        }
                                        else
                                        {
                                            LoggerManager.Debug($"Unload() Foup#{FoupNumber}. e84controller is null.");
                                        }

                                        Unload();
                                        UpdateFoupLampState();
                                    }
                                    else if (GPLoader != null && this.GPLoader.IsLoaderBusy)
                                    {
                                        this.MetroDialogManager().ShowMessageDialog("Loader Busy", "It cannot be Unload.", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                                    }
                                    else
                                    {
                                        // E84 가 연결되어있고 Auto Mode 인 경우에는 Manual Load/Unload 를 동작하지 못하게 한다.
                                        var e84controller = this.E84Module().GetE84Controller(FoupNumber, E84OPModuleTypeEnum.FOUP);

                                        if (e84controller != null)
                                        {
                                            if (e84controller.CommModule.Connection == E84ComStatus.CONNECTED)
                                            {
                                                if (e84controller.CommModule.RunMode == E84Mode.AUTO)
                                                {
                                                    this.MetroDialogManager().ShowMessageDialog("Error Message", "Manual Load/Unload cannot be operated when E84 is in Auto Mode. \nOperate after changing to Manual Mode.", EnumMessageStyle.Affirmative);

                                                    return;
                                                }
                                                else
                                                {
                                                    LoggerManager.Debug($"Unload() Foup#{FoupNumber}. e84controller runmode is {e84controller.CommModule.RunMode}.");
                                                }
                                            }
                                            else
                                            {
                                                LoggerManager.Debug($"Unload() Foup#{FoupNumber}. e84controller connection state is {e84controller.CommModule.Connection}");
                                            }
                                        }
                                        else
                                        {
                                            LoggerManager.Debug($"Unload() Foup#{FoupNumber}. e84controller is null.");
                                        }

                                        Unload();
                                        UpdateFoupLampState();
                                    }
                                }
                            }
                            else if (val == false)
                            {
                                UpdateFoupLampState();
                            }

                            LoggerManager.Debug($"Foup#{FoupIndex} DI_FOUP_UNLOAD_BUTTONs value is change to {val}");
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void DI_C12IN_NPLACEMENT_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (Permission.GetState() == FoupPermissionStateEnum.EVERY_ONE)
                {
                    if (e.PropertyName == "Value")
                    {
                        var val = IOManager.IOMap.Inputs.DI_C12IN_NPLACEMENT.Value;

                        if (SystemParam.FoupType.Value == FoupTypeEnum.UNDEFINED)
                        {
                            val = IOManager.IOMap.Inputs.DI_CST_UNLOCK12s[FoupIndex].Value;
                        }

                        if (val == true)
                        {
                            UpdateFoupLampState();
                        }
                        else if (val == false)
                        {
                            UpdateFoupLampState();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private object lockObj = new object();
        private void DI_LOAD_SWITCH_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                lock (lockObj)
                {
                    if (Permission.GetState() == FoupPermissionStateEnum.EVERY_ONE)
                    {
                        if (e.PropertyName == "Value")
                        {
                            var val = IOManager.IOMap.Inputs.DI_LOAD_SWITCH.Value;

                            if (SystemParam.FoupType.Value == FoupTypeEnum.UNDEFINED)
                            {
                                val = IOManager.IOMap.Inputs.DI_FOUP_LOAD_BUTTONs[FoupIndex].Value;
                            }
                            if (val == true)
                            {
                                if (this.ModuleState.State != FoupStateEnum.LOAD)
                                {
                                    if (GPLoader != null && !this.GPLoader.IsLoaderBusy)
                                    {
                                        // E84 가 연결되어있고 Auto Mode 인 경우에는 Manual Load/Unload 를 동작하지 못하게 한다.
                                        var e84controller = this.E84Module().GetE84Controller(FoupNumber, E84OPModuleTypeEnum.FOUP);
                                        if (e84controller != null)
                                        {
                                            if (e84controller.CommModule.Connection == E84ComStatus.CONNECTED)
                                            {
                                                if (e84controller.CommModule.RunMode == E84Mode.AUTO)
                                                {
                                                    this.MetroDialogManager().ShowMessageDialog("Error Message", "Manual Load/Unload cannot be operated when E84 is in Auto Mode. \nOperate after changing to Manual Mode.", EnumMessageStyle.Affirmative);

                                                    return;
                                                }
                                                else
                                                {
                                                    LoggerManager.Debug($"Load() Foup#{FoupNumber}. e84controller runmode is {e84controller.CommModule.RunMode}.");
                                                }
                                            }
                                            else
                                            {
                                                LoggerManager.Debug($"Load() Foup#{FoupNumber}. e84controller connection state is {e84controller.CommModule.Connection}");
                                            }
                                        }
                                        else
                                        {
                                            LoggerManager.Debug($"Load() Foup#{FoupNumber}. e84controller is null.");
                                        }

                                        if (ValidationCassetteAvailable(GetCassetteType(), out string msg) == EventCodeEnum.NONE)
                                        {
                                            Load();
                                            UpdateFoupState();
                                        }
                                        else
                                        {
                                            this.MetroDialogManager().ShowMessageDialog("Foup Load Error", $"Foup#{FoupNumber} {msg}." +
                                                $"\nNo CST was detected for the specified CST type." +
                                                $"\nPlease reload this foup.", EnumMessageStyle.Affirmative);
                                        }
                                    }
                                    else if (GPLoader != null && this.GPLoader.IsLoaderBusy)
                                    {
                                        this.MetroDialogManager().ShowMessageDialog("Loader Busy", "It cannot be Load.", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                                    }
                                    else
                                    {
                                        // E84 가 연결되어있고 Auto Mode 인 경우에는 Manual Load/Unload 를 동작하지 못하게 한다.
                                        var e84controller = this.E84Module().GetE84Controller(FoupNumber, E84OPModuleTypeEnum.FOUP);

                                        if (e84controller != null)
                                        {
                                            if (e84controller.CommModule.Connection == E84ComStatus.CONNECTED)
                                            {
                                                if (e84controller.CommModule.RunMode == E84Mode.AUTO)
                                                {
                                                    this.MetroDialogManager().ShowMessageDialog("Error Message", "Manual Load/Unload cannot be operated when E84 is in Auto Mode. \nOperate after changing to Manual Mode.", EnumMessageStyle.Affirmative);

                                                    return;
                                                }
                                                else
                                                {
                                                    LoggerManager.Debug($"Load() Foup#{FoupNumber}. e84controller runmode is {e84controller.CommModule.RunMode}.");
                                                }
                                            }
                                            else
                                            {
                                                LoggerManager.Debug($"Load() Foup#{FoupNumber}. e84controller connection state is {e84controller.CommModule.Connection}");
                                            }
                                        }
                                        else
                                        {
                                            LoggerManager.Debug($"Load() Foup#{FoupNumber}. e84controller is null.");
                                        }

                                        if (ValidationCassetteAvailable(GetCassetteType(), out string msg) == EventCodeEnum.NONE)
                                        {
                                            Load();
                                            UpdateFoupState();
                                        }
                                        else
                                        {
                                            this.MetroDialogManager().ShowMessageDialog("Foup Load Error", $"Foup#{FoupNumber} {msg}." +
                                                $"\nNo CST was detected for the specified CST type." +
                                                $"\nPlease reload this foup.", EnumMessageStyle.Affirmative);
                                        }
                                    }
                                }
                            }
                            else if (val == false)
                            {
                                UpdateFoupLampState();
                            }

                            LoggerManager.Debug($"Foup#{FoupIndex} DI_FOUP_LOAD_BUTTONs value is change to {val}");
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum Load()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"FoupModule.Load(): thread lock start.");
                FplockSlim.Wait();
                LoggerManager.Debug($"FoupModule.Load(): thread lock running.");
                LoggerManager.Debug($"FoupInfo's PresenceState: {foupInfo.FoupPRESENCEState}", isInfo: true);
                LoggerManager.Debug($"FoupInfo's FoupCoverState: {foupInfo.FoupCoverState}", isInfo: true);
                LoggerManager.Debug($"FoupInfo's DockingPlateState: {foupInfo.DockingPlateState}", isInfo: true);
                LoggerManager.Debug($"FoupInfo's DockingPortState: {foupInfo.DockingPortState}", isInfo: true);
                LoggerManager.Debug($"FoupInfo's DockingPortDoorState: {foupInfo.DockingPortDoorState}", isInfo: true);
                LoggerManager.Debug($"FoupInfo's OpenerState: {foupInfo.OpenerState}", isInfo: true);
                LoggerManager.Debug($"FoupInfo's TiltState: {foupInfo.TiltState}", isInfo: true);
                LoggerManager.Debug($"FoupInfo's WaferOutSensor: {foupInfo.WaferOutSensor}", isInfo: true);

                if (SystemManager.SysteMode == SystemModeEnum.Multiple || this.MonitoringManager().IsSystemError == false)
                {
                    if (foupInfo.FoupPRESENCEState == FoupPRESENCEStateEnum.CST_ATTACH)
                    {
                        if (Callback.IsFoupUsingByLoader())
                        {
                            retVal = EventCodeEnum.UNDEFINED;
                        }
                        else
                        {
                            retVal = ModuleState.Load();
                            UpdateFoupState();
                            BroadcastFoupStateAsync();
                        }
                    }
                    else
                    {
                        retVal = EventCodeEnum.UNDEFINED;
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            finally
            {
                FplockSlim?.Release();
                LoggerManager.Debug($"FoupModule.Load(): thread lock end.");
            }

            return retVal;
        }

        public EventCodeEnum Unload()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                LoggerManager.Debug($"FoupModule.Unload(): thread lock start.");
                FplockSlim?.Wait();
                LoggerManager.Debug($"FoupModule.Unload(): thread lock running.");
                LoggerManager.Debug($"FoupInfo's PresenceState: {foupInfo.FoupPRESENCEState}, Time:{DateTime.Now}", isInfo: true);
                LoggerManager.Debug($"FoupInfo's FoupCoverState: {foupInfo.FoupCoverState}, Time:{DateTime.Now}", isInfo: true);
                LoggerManager.Debug($"FoupInfo's DockingPlateState: {foupInfo.DockingPlateState}, Time:{DateTime.Now}", isInfo: true);
                LoggerManager.Debug($"FoupInfo's DockingPortState: {foupInfo.DockingPortState}, Time:{DateTime.Now}", isInfo: true);
                LoggerManager.Debug($"FoupInfo's DockingPortDoorState: {foupInfo.DockingPortDoorState}, Time:{DateTime.Now}", isInfo: true);
                LoggerManager.Debug($"FoupInfo's OpenerState: {foupInfo.OpenerState}, Time:{DateTime.Now}", isInfo: true);
                LoggerManager.Debug($"FoupInfo's TiltState: {foupInfo.TiltState}, Time:{DateTime.Now}", isInfo: true);
                LoggerManager.Debug($"FoupInfo's WaferOutSensor: {foupInfo.WaferOutSensor}, Time:{DateTime.Now}", isInfo: true);


                if (SystemManager.SysteMode == SystemModeEnum.Multiple || this.MonitoringManager().IsSystemError == false)
                {
                    retVal = ModuleState.UnLoad();
                }

                UpdateFoupState();
                BroadcastFoupStateAsync();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            finally
            {
                FplockSlim?.Release();
                LoggerManager.Debug($"FoupModule.Unload(): thread lock end.");
            }

            return retVal;
        }

        public EventCodeEnum Continue()
        {
            EventCodeEnum retVal;
            try
            {
                LoggerManager.Debug($"FoupModule.Continue(): thread lock start.");
                FplockSlim?.Wait();
                LoggerManager.Debug($"FoupModule.Continue(): thread lock running.");

                retVal = ModuleState.Continue();
                BroadcastFoupStateAsync();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            finally
            {
                FplockSlim?.Release();
                LoggerManager.Debug($"FoupModule.Load(): thread lock end.");
            }
            return retVal;
        }
        public EventCodeEnum PrevRun()
        {
            EventCodeEnum retVal;
            try
            {
                LoggerManager.Debug($"FoupModule.PrevRun(): thread lock start.");
                FplockSlim?.Wait();
                LoggerManager.Debug($"FoupModule.PrevRun(): thread lock running.");

                retVal = ModuleState.PrevRun();
                BroadcastFoupStateAsync();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            finally
            {
                FplockSlim?.Release();
                LoggerManager.Debug($"FoupModule.Load(): thread lock end.");
            }
            return retVal;
        }
        public EventCodeEnum FosB_Load()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"FosB_Load START", isInfo: true);
                LoggerManager.Debug($"FoupInfo's PresenceState: {foupInfo.FoupPRESENCEState}", isInfo: true);
                LoggerManager.Debug($"FoupInfo's FoupCoverState: {foupInfo.FoupCoverState}", isInfo: true);
                LoggerManager.Debug($"FoupInfo's DockingPlateState: {foupInfo.DockingPlateState}", isInfo: true);
                LoggerManager.Debug($"FoupInfo's DockingPortState: {foupInfo.DockingPortState}", isInfo: true);
                LoggerManager.Debug($"FoupInfo's DockingPortDoorState: {foupInfo.DockingPortDoorState}", isInfo: true);
                LoggerManager.Debug($"FoupInfo's WaferOutSensor: {foupInfo.WaferOutSensor}", isInfo: true);

                if (SystemManager.SysteMode == SystemModeEnum.Multiple || this.MonitoringManager().IsSystemError == false)
                {
                    if (foupInfo.FoupPRESENCEState == FoupPRESENCEStateEnum.CST_ATTACH)
                    {
                        if (Callback.IsFoupUsingByLoader())
                        {
                            retVal = EventCodeEnum.UNDEFINED;
                        }
                        else
                        {
                            LoggerManager.Debug($"FosB_Load 1.DockingPlateLock Start");
                            retVal = this.DockingPlateLock();
                            if (retVal == EventCodeEnum.NONE)
                            {
                                LoggerManager.Debug($"FosB_Load 2.DockingPortIn Start");
                                retVal = this.DockingPortIn();
                                if (retVal == EventCodeEnum.NONE)
                                {
                                    LoggerManager.Debug($"FosB_Load Success");
                                    ModuleState = new FoupLoadState(this);
                                    BroadcastFoupStateAsync();
                                }
                            }
                        }
                    }
                    else
                    {
                        retVal = EventCodeEnum.UNDEFINED;
                    }


                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }

        public EventCodeEnum FosB_Unload()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                LoggerManager.Debug($"FosB_Unload START", isInfo: true);
                LoggerManager.Debug($"FoupInfo's PresenceState: {foupInfo.FoupPRESENCEState}, Time:{DateTime.Now}", isInfo: true);
                LoggerManager.Debug($"FoupInfo's FoupCoverState: {foupInfo.FoupCoverState}, Time:{DateTime.Now}", isInfo: true);
                LoggerManager.Debug($"FoupInfo's DockingPlateState: {foupInfo.DockingPlateState}, Time:{DateTime.Now}", isInfo: true);
                LoggerManager.Debug($"FoupInfo's DockingPortState: {foupInfo.DockingPortState}, Time:{DateTime.Now}", isInfo: true);
                LoggerManager.Debug($"FoupInfo's DockingPortDoorState: {foupInfo.DockingPortDoorState}, Time:{DateTime.Now}", isInfo: true);
                LoggerManager.Debug($"FoupInfo's WaferOutSensor: {foupInfo.WaferOutSensor}, Time:{DateTime.Now}", isInfo: true);

                if (SystemManager.SysteMode == SystemModeEnum.Multiple || this.MonitoringManager().IsSystemError == false)
                {
                    LoggerManager.Debug($"FosB_Unload 1.CoverClose Start");
                    retVal = this.Cover.Close();
                    if (retVal == EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"FosB_Unload 2.DockingPort40 Out Start");
                        retVal = this.DockingPort40Out();
                        if (retVal == EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"FosB_Unload 3.DockingPortOut Start");
                            retVal = this.DockingPortOut();
                            if (retVal == EventCodeEnum.NONE)
                            {
                                LoggerManager.Debug($"FosB_Unload 4.DockingPlateUnlock Start");
                                retVal = this.DockingPlateUnlock();
                                if (retVal == EventCodeEnum.NONE)
                                {
                                    LoggerManager.Debug($"FosB_Unload Success");
                                    ModuleState = new FoupUnLoadState(this);
                                    BroadcastFoupStateAsync();
                                }
                            }
                        }
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
        public EventCodeEnum CoverUp()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum CoverDown()
        {
            return EventCodeEnum.NONE;
        }

        public void OccurFoupClampStateChangedEvent(int foupIndex, bool clampState)
        {
            try
            {
                if (FoupClampStateChangedEvent != null)
                {
                    FoupClampStateChangedEvent(FoupIndex + 1, clampState);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum DockingPlateLock()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = DockingPlate.Lock();

                PIVInfo pivinfo = new PIVInfo(foupnumber: FoupIndex + 1);

                if (retVal != EventCodeEnum.NONE)
                {
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(ClampLockFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    semaphore.Wait();

                    RisingRFIDEvent(false, " ");
                }
                else
                {
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(ClampLockEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    semaphore.Wait();

                    OccurFoupClampStateChangedEvent(FoupIndex + 1, true);//E84Controller의 Clamp 상태를 동기

                    Read_CassetteID();
                }

                BroadcastFoupStateAsync();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }


        public EventCodeEnum DockingPlateUnlock()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = DockingPlate.Unlock();

                if (retVal != EventCodeEnum.NONE)
                {
                    PIVInfo pivinfo = new PIVInfo(foupnumber: FoupIndex + 1);
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(ClampUnlockFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    semaphore.Wait();
                }
                else
                {
                    ChangeFoupServiceStatus(GEMFoupStateEnum.READY_TO_UNLOAD);

                    PIVInfo pivinfo = new PIVInfo(foupnumber: FoupIndex + 1);
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(ClampUnlockEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    semaphore.Wait();

                    OccurFoupClampStateChangedEvent(FoupIndex + 1, false);
                }

                BroadcastFoupStateAsync();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum DockingPlateRecoveryUnlock()
        {
            EventCodeEnum retVal;

            try
            {
                retVal = DockingPlate.RecoveryUnlock();

                UpdateFoupState();
                BroadcastFoupStateAsync();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }

        public EventCodeEnum DockingPortIn()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (SystemParam.FoupType.Value == FoupTypeEnum.TOP)
                {
                    retVal = DockingPort.In();
                }
                else if (SystemParam.FoupType.Value == FoupTypeEnum.FLAT)
                {
                    retVal = DockingPort.In();
                }
                else if (SystemParam.FoupType.Value == FoupTypeEnum.UNDEFINED)
                {
                    retVal = DockingPort.In();
                }
                else if (SystemParam.FoupType.Value == FoupTypeEnum.CST8PORT_FLAT)
                {
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
                }

                BroadcastFoupStateAsync();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum DockingPortOut()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (SystemParam.FoupType.Value == FoupTypeEnum.TOP)
                {
                    retVal = DockingPort.Out();
                }
                else if (SystemParam.FoupType.Value == FoupTypeEnum.FLAT)
                {
                    retVal = DockingPort.Out();
                }
                else if (SystemParam.FoupType.Value == FoupTypeEnum.UNDEFINED)
                {
                    retVal = DockingPort.Out();
                }
                else if (SystemParam.FoupType.Value == FoupTypeEnum.CST8PORT_FLAT)
                {
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
                }

                BroadcastFoupStateAsync();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum DockingPort40In()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (SystemParam.FoupType.Value == FoupTypeEnum.TOP)
                {
                    retVal = DockingPort40.In();
                }
                else if (SystemParam.FoupType.Value == FoupTypeEnum.FLAT)
                {
                    retVal = DockingPort40.In();
                }
                else if (SystemParam.FoupType.Value == FoupTypeEnum.CST8PORT_FLAT)
                {
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
                }

                BroadcastFoupStateAsync();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum DockingPort40Out()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (SystemParam.FoupType.Value == FoupTypeEnum.TOP)
                {
                    retVal = DockingPort40.Out();
                }
                else if (SystemParam.FoupType.Value == FoupTypeEnum.FLAT)
                {
                    retVal = DockingPort40.Out();
                }
                else if (SystemParam.FoupType.Value == FoupTypeEnum.UNDEFINED)
                {
                    retVal = DockingPort40.Out();
                }
                else if (SystemParam.FoupType.Value == FoupTypeEnum.CST8PORT_FLAT)
                {
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
                }

                BroadcastFoupStateAsync();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum CassetteOpenerLock()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (SystemParam.FoupType.Value == FoupTypeEnum.TOP)
                {
                    retVal = CassetteOpener.Lock();
                }
                else if (SystemParam.FoupType.Value == FoupTypeEnum.FLAT)
                {
                    retVal = CassetteOpener.Lock();
                }
                else if (SystemParam.FoupType.Value == FoupTypeEnum.CST8PORT_FLAT)
                {
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
                }

                BroadcastFoupStateAsync();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum CassetteOpenerUnlock()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (SystemParam.FoupType.Value == FoupTypeEnum.TOP)
                {
                    retVal = CassetteOpener.Unlock();
                }
                else if (SystemParam.FoupType.Value == FoupTypeEnum.FLAT)
                {
                    retVal = CassetteOpener.Unlock();
                }
                else if (SystemParam.FoupType.Value == FoupTypeEnum.CST8PORT_FLAT)
                {
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
                }

                BroadcastFoupStateAsync();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum FoupTiltUp()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (SystemParam.FoupType.Value == FoupTypeEnum.TOP)
                {
                    retVal = Tilt.Up();
                    retVal = EventCodeEnum.NONE;
                }
                else if (SystemParam.FoupType.Value == FoupTypeEnum.FLAT)
                {
                    retVal = Tilt.Up();
                }
                else if (SystemParam.FoupType.Value == FoupTypeEnum.CST8PORT_FLAT)
                {
                    retVal = Tilt.Up();
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
                }

                BroadcastFoupStateAsync();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum FoupTiltDown()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (SystemParam.FoupType.Value == FoupTypeEnum.TOP)
                {
                    retVal = Tilt.Down();
                }
                else if (SystemParam.FoupType.Value == FoupTypeEnum.FLAT)
                {
                    retVal = Tilt.Down();
                }
                else if (SystemParam.FoupType.Value == FoupTypeEnum.CST8PORT_FLAT)
                {
                    retVal = Tilt.Down();
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
                }

                BroadcastFoupStateAsync();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum MonitorForWaferOutSensor(bool value)
        {
            return EventCodeEnum.NONE;
        }

        public void SetLotState(ModuleStateEnum state)
        {
            this.SetLotState(state);
        }

        public EventCodeEnum SaveSysParameter()
        {
            throw new NotImplementedException();
        }

        public void FoupPermissionStateTransition(IFoupPermission state)
        {
            Permission = state;
        }

        public void FoupPresenceStateChange()  // 
        {
            try
            {
                if ((this.GPLoader.GetCSTState(FoupIndex) == EnumCSTState.LOADING
                    || this.GPLoader.GetCSTState(FoupIndex) == EnumCSTState.UNLOADING))
                {
                    LoggerManager.Debug($"FoupPresenceStateChange() Ignore Presence state change. State is {this.GPLoader.GetCSTState(FoupIndex)}");
                    return;
                }

                if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    bool PRESENCE6_8_2 = false;
                    bool PRESENCE6_8_3 = false;

                    bool C6IN_PLACEMENT = false;
                    bool C8IN_PLACEMENT = false;
                    bool C6_8IN_NPLACEMENT = false;

                    this.IOManager.IOServ.ReadBit(this.IOManager.IOMap.Inputs.DI_C6IN_PLACEMENT, out C6IN_PLACEMENT);
                    this.IOManager.IOServ.ReadBit(this.IOManager.IOMap.Inputs.DI_C8IN_PLACEMENT, out C8IN_PLACEMENT);
                    this.IOManager.IOServ.ReadBit(this.IOManager.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE2, out PRESENCE6_8_2);
                    this.IOManager.IOServ.ReadBit(this.IOManager.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE3, out PRESENCE6_8_3);

                    bool matched = false;

                    this.IOManager.IOServ.ReadBit(this.IOManager.IOMap.Inputs.DI_C6IN_C8IN_NPLACEMENT, out C6_8IN_NPLACEMENT);

                    if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                    {
                        if (IOManager.IOMap.Inputs.DI_CST12_PRESs[FoupIndex].Value &&
                        IOManager.IOMap.Inputs.DI_CST12_PRES2s[FoupIndex].Value == true)
                        {
                            foupInfo.Foup12IN_PRESENCEState = Foup12IN_PRESENCEStateEnum.CST_ATTACH;
                            if (DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH12)
                            {
                                foupInfo.FoupPRESENCEState = FoupPRESENCEStateEnum.CST_ATTACH;
                            }
                            else
                            {
                                foupInfo.FoupPRESENCEState = FoupPRESENCEStateEnum.CST_NOT_MATCHED;
                                if (foupInfo.State == FoupStateEnum.LOAD)
                                {
                                    SetLEDStatus(FoupPRESENCEStateEnum.CST_NOT_MATCHED, FoupPRESENCEStateEnum.CST_DETACH);
                                }
                                else if (foupInfo.State == FoupStateEnum.UNLOAD)
                                {
                                    SetLEDStatus(FoupPRESENCEStateEnum.CST_DETACH, FoupPRESENCEStateEnum.CST_NOT_MATCHED);
                                }
                                else
                                {
                                    SetLEDStatus(FoupPRESENCEStateEnum.CST_DETACH, FoupPRESENCEStateEnum.CST_DETACH);
                                }
                            }
                        }
                    }

                    if (PRESENCE6_8_2 == true && PRESENCE6_8_3 == true)  //6_8 Presence sensor 1,2 인식
                    {
                        foupInfo.FoupPRESENCEState = FoupPRESENCEStateEnum.CST_ATTACH;
                        // INCH 확인
                        if (C8IN_PLACEMENT == true)
                        {
                            foupInfo.Foup6IN_PRESENCEState = Foup6IN_PRESENCEStateEnum.CST_DETACH;
                            foupInfo.Foup8IN_PRESENCEState = Foup8IN_PRESENCEStateEnum.CST_ATTACH;
                            foupInfo.Foup12IN_PRESENCEState = Foup12IN_PRESENCEStateEnum.CST_DETACH;

                            if (DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH8)
                            {
                                matched = true;
                            }
                            else
                            {
                                matched = false;
                            }
                        }
                        else if (C6IN_PLACEMENT == true)
                        {
                            foupInfo.Foup6IN_PRESENCEState = Foup6IN_PRESENCEStateEnum.CST_ATTACH;
                            foupInfo.Foup8IN_PRESENCEState = Foup8IN_PRESENCEStateEnum.CST_DETACH;
                            foupInfo.Foup12IN_PRESENCEState = Foup12IN_PRESENCEStateEnum.CST_DETACH;

                            if (DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH6)
                            {
                                matched = true;
                            }
                            else
                            {
                                matched = false;
                            }
                        }
                        else
                        {
                            // NPlacement, 그 외 => Inch 확인 불가

                            foupInfo.Foup6IN_PRESENCEState = Foup6IN_PRESENCEStateEnum.CST_DETACH;
                            foupInfo.Foup8IN_PRESENCEState = Foup8IN_PRESENCEStateEnum.CST_DETACH;
                            foupInfo.Foup12IN_PRESENCEState = Foup12IN_PRESENCEStateEnum.CST_DETACH;

                            matched = false;
                        }
                    }
                    else //6_8 Presence sensor Sensor Not Atach
                    {
                        if (foupInfo.Foup8IN_PRESENCEState == Foup8IN_PRESENCEStateEnum.CST_ATTACH |
                            foupInfo.Foup6IN_PRESENCEState == Foup6IN_PRESENCEStateEnum.CST_ATTACH)
                        {
                            foupInfo.FoupPRESENCEState = FoupPRESENCEStateEnum.CST_DETACH;

                            if (foupInfo.State != FoupStateEnum.UNLOAD)
                            {
                                if (foupInfo.DockingPlateState != DockingPlateStateEnum.UNLOCK)
                                {
                                    DockingPlateRecoveryUnlock();
                                }
                            }
                            else
                            {
                                foupInfo.Foup8IN_PRESENCEState = Foup8IN_PRESENCEStateEnum.CST_DETACH;
                                foupInfo.Foup6IN_PRESENCEState = Foup6IN_PRESENCEStateEnum.CST_DETACH;
                            }
                        }

                        if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                        {
                            if (foupInfo.FoupPRESENCEState == FoupPRESENCEStateEnum.CST_ATTACH)
                            {
                                if (IOManager.IOMap.Inputs.DI_CST12_PRESs[FoupIndex].Value &&
                                IOManager.IOMap.Inputs.DI_CST12_PRES2s[FoupIndex].Value == false)
                                {
                                    foupInfo.FoupPRESENCEState = FoupPRESENCEStateEnum.CST_DETACH;
                                    if (foupInfo.State != FoupStateEnum.UNLOAD)
                                    {
                                        if (foupInfo.DockingPlateState != DockingPlateStateEnum.UNLOCK)
                                        {
                                            DockingPlateUnlock();
                                        }
                                    }
                                    else
                                    {
                                    }
                                }
                            }
                        }
                    }

                    // TODO : V19
                    if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                    {
                        matched = true;
                    }

                    // 매칭 확인
                    if (matched == true)
                    {
                        foupInfo.FoupPRESENCEState = FoupPRESENCEStateEnum.CST_ATTACH;
                    }
                    else
                    {
                        if (foupInfo.State == FoupStateEnum.LOAD)
                        {
                            SetLEDStatus(FoupPRESENCEStateEnum.CST_NOT_MATCHED, FoupPRESENCEStateEnum.CST_DETACH);
                        }
                        else if (foupInfo.State == FoupStateEnum.UNLOAD)
                        {
                            SetLEDStatus(FoupPRESENCEStateEnum.CST_DETACH, FoupPRESENCEStateEnum.CST_NOT_MATCHED);
                        }
                        else
                        {
                            SetLEDStatus(FoupPRESENCEStateEnum.CST_DETACH, FoupPRESENCEStateEnum.CST_DETACH);
                        }
                    }
                }
                else if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    bool PRESENCE6_8_1 = Foup6_8Presence2;
                    bool PRESENCE6_8_2 = Foup6_8Presence3;
                    bool PRESENCE6_8_3 = Foup6_8Presence1;

                    var presence1 = IOManager.IOMap.Inputs.DI_CST12_PRESs[FoupIndex];
                    var presence2 = IOManager.IOMap.Inputs.DI_CST12_PRES2s[FoupIndex];

                    if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                    {
                        // cassette type  보고 type 에 설정되어 있는 io값 확인하기. selly
                        if (IOManager.IOMap.Inputs.DI_CST12_PRESs[FoupIndex].Value &&
                        IOManager.IOMap.Inputs.DI_CST12_PRES2s[FoupIndex].Value == true)
                        {
                            LoggerManager.Debug($"[FOUP{FoupIndex + 1} PRESENCE] on");
                            int isPresence1 = 1; int isPresence2 = 1; int isExist = 1;
                            if (IOManager.IOMap.Inputs.DI_CST_Exists != null)
                            {
                                if (IOManager.IOMap.Inputs.DI_CST_Exists.Count > FoupIndex)
                                {
                                    var exist = IOManager.IOMap.Inputs.DI_CST_Exists[FoupIndex];
                                    isPresence1 = IOManager.MonitorForIO(presence1, true, presence1.MaintainTime.Value, presence1.TimeOut.Value);
                                    isPresence2 = IOManager.MonitorForIO(presence2, true, presence2.MaintainTime.Value, presence2.TimeOut.Value);
                                    isExist = IOManager.MonitorForIO(exist, true, exist.MaintainTime.Value, exist.TimeOut.Value);
                                    LoggerManager.Debug($"[FOUP{FoupIndex + 1}] DI_CST_Exists state is " + $"{IOManager.IOMap.Inputs.DI_CST_Exists[FoupIndex].Value}");
                                    LoggerManager.Debug($"[FOUP{FoupIndex + 1}] (DI_CST12_PRESs.{FoupIndex}, DI_CST12_PRES2s.{FoupIndex}, DI_CST_Exists.{FoupIndex}) state is {IOManager.IOMap.Inputs.DI_CST_Exists[FoupIndex].Value}) ");
                                }
                            }

                            //int val = IOManager.IOMap.Inputs.DI_CST_Exists[FoupIndex].Value ? 1 : 0;
                            if (isPresence1 == 1 && isPresence2 == 1 && isExist == 1)
                            {
                                //bool cstExist = false;
                                //IOManager.ReadBit(IOManager.IOMap.Inputs.DI_CST_Exists[FoupIndex], out cstExist);

                                foupInfo.Foup12IN_PRESENCEState = Foup12IN_PRESENCEStateEnum.CST_ATTACH;
                                if (DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH12)
                                {
                                    //var state = IOManager.MonitorForIO(IOManager.IOMap.Inputs.DI_CST12_PRESs[FoupIndex], true, 500, 1000);
                                    //var state = IOManager.MonitorForIO(IOManager.IOMap.Inputs.DI_CST12_PRESs[FoupIndex], true, 500, 1000);
                                    //if (state == 1)  // State maintained for specific duration
                                    //{
                                    foupInfo.FoupPRESENCEState = FoupPRESENCEStateEnum.CST_ATTACH;
                                    LoggerManager.Debug($"#{FoupIndex + 1} Foup PRESENCEState Change to CST_ATTACH");

                                    if (PresenceStateChangedEvent == null || ValidationRFIDCommState(false) == false)
                                    {
                                        // TODO : 데드락 발생 경우가 있어, 임시로 동작 안하게끔 처리 해놓음
                                        //PIVInfo pivinfo = new PIVInfo(foupnumber: FoupIndex + 1);
                                        //SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                        //this.EventManager().RaisingEvent(typeof(CarrierPlacedEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                        //semaphore.Wait();

                                    }
                                    else
                                    {
                                        PresenceStateChangedEvent(FoupIndex + 1, true, true);
                                    }
                                }
                                else
                                {
                                    foupInfo.FoupPRESENCEState = FoupPRESENCEStateEnum.CST_NOT_MATCHED;
                                    if (foupInfo.State == FoupStateEnum.LOAD)
                                    {
                                        SetLEDStatus(FoupPRESENCEStateEnum.CST_NOT_MATCHED, FoupPRESENCEStateEnum.CST_DETACH);
                                    }
                                    else if (foupInfo.State == FoupStateEnum.UNLOAD)
                                    {
                                        SetLEDStatus(FoupPRESENCEStateEnum.CST_DETACH, FoupPRESENCEStateEnum.CST_NOT_MATCHED);
                                    }
                                    else
                                    {
                                        SetLEDStatus(FoupPRESENCEStateEnum.CST_DETACH, FoupPRESENCEStateEnum.CST_DETACH);
                                    }
                                }
                            }

                        }

                        else if (IOManager.IOMap.Inputs.DI_CST12_PRESs[FoupIndex].Value == false &&
                        IOManager.IOMap.Inputs.DI_CST12_PRES2s[FoupIndex].Value == false)
                        {
                            LoggerManager.Debug($"[FOUP{FoupIndex + 1} PRESENCE] off");
                            int isPresence1 = 1; int isPresence2 = 1; int isExist = 1;
                            if (IOManager.IOMap.Inputs.DI_CST_Exists != null)
                            {
                                if (IOManager.IOMap.Inputs.DI_CST_Exists.Count > FoupIndex)
                                {
                                    var exist = IOManager.IOMap.Inputs.DI_CST_Exists[FoupIndex];
                                    if (IOManager.IOMap.Inputs.DI_CST_Exists[FoupIndex].IOOveride.Value == EnumIOOverride.NONE)
                                    {
                                        isPresence1 = IOManager.MonitorForIO(presence1, false, presence1.MaintainTime.Value, presence1.TimeOut.Value);
                                        isPresence2 = IOManager.MonitorForIO(presence2, false, presence2.MaintainTime.Value, presence2.TimeOut.Value);
                                        isExist = IOManager.MonitorForIO(exist, false, exist.MaintainTime.Value, exist.TimeOut.Value);
                                        LoggerManager.Debug($"[FOUP{FoupIndex + 1}] DI_CST_Exists state is " + $"{IOManager.IOMap.Inputs.DI_CST_Exists[FoupIndex].Value}");
                                        LoggerManager.Debug($"[FOUP{FoupIndex + 1}] (DI_CST12_PRESs.{FoupIndex}, DI_CST12_PRES2s.{FoupIndex}, DI_CST_Exists.{FoupIndex}) state is " +
                                                                                                                                                $"({IOManager.IOMap.Inputs.DI_CST12_PRESs[FoupIndex].Value}, " +
                                                                                                                                                $"{ IOManager.IOMap.Inputs.DI_CST12_PRES2s[FoupIndex].Value}, " +
                                                                                                                                                $"{ IOManager.IOMap.Inputs.DI_CST_Exists[FoupIndex].Value}) ");

                                    }
                                }
                            }

                            if (isPresence1 == 1 && isPresence2 == 1 && isExist == 1)
                            {
                                foupInfo.Foup12IN_PRESENCEState = Foup12IN_PRESENCEStateEnum.CST_DETACH;

                                if (DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH12)
                                {
                                    if (foupInfo.FoupPRESENCEState != FoupPRESENCEStateEnum.CST_DETACH)
                                    {
                                        foupInfo.FoupPRESENCEState = FoupPRESENCEStateEnum.CST_DETACH;

                                        LoggerManager.Debug($"#{FoupIndex + 1} Foup PRESENCEState Change to CST_DETACH");

                                        PIVInfo pivinfo = new PIVInfo(foupnumber: FoupIndex + 1);
                                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                        this.EventManager().RaisingEvent(typeof(CarrierRemovedEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                        semaphore.Wait();

                                        if (PresenceStateChangedEvent != null)
                                        {
                                            PresenceStateChangedEvent(FoupIndex + 1, false, true);
                                        }

                                        ChangeFoupServiceStatus(GEMFoupStateEnum.TRANSFER_READY, true);
                                        ChangeFoupServiceStatus(GEMFoupStateEnum.READY_TO_LOAD);

                                        ClearCassetteID();
                                    }
                                }
                            }

                        }
                    }

                    if (foupInfo.FoupPRESENCEState == FoupPRESENCEStateEnum.CST_ATTACH)
                    {
                        if (IOManager.IOMap.Inputs.DI_CST12_PRESs[FoupIndex].Value &&
                        IOManager.IOMap.Inputs.DI_CST12_PRES2s[FoupIndex].Value == false)
                        {

                            var state = IOManager.MonitorForIO(IOManager.IOMap.Inputs.DI_CST12_PRESs[FoupIndex], false, 500, 1000);

                            if (state == 1) // In cassetted attatched state, detect false value on presense, release foup. 
                            {
                                foupInfo.FoupPRESENCEState = FoupPRESENCEStateEnum.CST_DETACH;

                                LoggerManager.Debug($"#{FoupIndex + 1} Foup PRESENCEState Change to CST_DETACH");

                                if (foupInfo.State != FoupStateEnum.UNLOAD)
                                {
                                    if (foupInfo.DockingPlateState != DockingPlateStateEnum.UNLOCK)
                                    {
                                        DockingPlateUnlock();
                                    }
                                }
                                else
                                {
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
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

        public EventCodeEnum InitModule()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                ret = LoadTemplate();
                ret = this.TemplateManager().InitTemplate(this);
            }
            catch (Exception err)
            {
                LoggerManager.Error($"InitModule(): Error occurred. Err = {err.Message}");

            }
            return ret;
        }

        public EventCodeEnum UpdateFoupState(bool forced_event = false)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                #region INCH6_8
                if (SystemParam.FoupType.Value == FoupTypeEnum.CST8PORT_FLAT)
                {
                    if (_DockingPlate.GetState() != DockingPlateStateEnum.ERROR)
                    {
                        if (_DockingPlate.GetState() == DockingPlateStateEnum.LOCK)
                        {
                            ModuleState = new FoupLoadState(this);
                            retVal = EventCodeEnum.NONE;
                        }
                        else if (_DockingPlate.GetState() == DockingPlateStateEnum.UNLOCK)
                        {
                            ModuleState = new FoupUnLoadState(this);
                            retVal = EventCodeEnum.NONE;
                        }
                        else
                        {
                            ModuleState = new FoupErrorState(this);
                            retVal = EventCodeEnum.NONE;
                        }
                    }
                    else
                    {
                        ModuleState = new FoupErrorState(this);
                        retVal = EventCodeEnum.NONE;
                    }
                }
                #endregion

                #region TOPLOADER
                else if (SystemParam.FoupType.Value == FoupTypeEnum.TOP)
                {
                    if (_DockingPlate.GetState() != DockingPlateStateEnum.ERROR &&
                        _DockingPort.GetState() != DockingPortStateEnum.ERROR &&
                        _DockingPort40.GetState() != DockingPort40StateEnum.ERROR &&
                        _CassetteOpener.GetState() != FoupCassetteOpenerStateEnum.ERROR)
                    {
                        if (_DockingPlate.GetState() == DockingPlateStateEnum.LOCK &&
                            _DockingPort.GetState() == DockingPortStateEnum.IN &&
                            _DockingPort40.GetState() == DockingPort40StateEnum.OUT &&
                            _Cover.EnumState == FoupCoverStateEnum.OPEN &&
                            _CassetteOpener.GetState() == FoupCassetteOpenerStateEnum.UNLOCK)
                        {
                            ModuleState = new FoupLoadState(this);
                            retVal = EventCodeEnum.NONE;
                        }
                        else if (_DockingPlate.GetState() == DockingPlateStateEnum.UNLOCK &&
                            _DockingPort.GetState() == DockingPortStateEnum.OUT &&
                            _DockingPort40.GetState() == DockingPort40StateEnum.IN &&
                            _Cover.EnumState == FoupCoverStateEnum.CLOSE &&
                            _CassetteOpener.GetState() == FoupCassetteOpenerStateEnum.LOCK)
                        {
                            ModuleState = new FoupUnLoadState(this);
                            retVal = EventCodeEnum.NONE;
                        }
                        else
                        {
                            ModuleState = new FoupErrorState(this);
                            retVal = EventCodeEnum.NONE;
                        }
                    }
                    else
                    {
                        ModuleState = new FoupErrorState(this);
                        retVal = EventCodeEnum.NONE;
                    }
                }
                else if (SystemParam.FoupType.Value == FoupTypeEnum.FLAT)
                {
                    if (_DockingPlate.GetState() != DockingPlateStateEnum.ERROR &&
                        _DockingPort.GetState() != DockingPortStateEnum.ERROR &&
                        _DockingPort40.GetState() != DockingPort40StateEnum.ERROR &&
                        _CassetteOpener.GetState() != FoupCassetteOpenerStateEnum.ERROR)
                    {
                        if (this.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH6 ||
                            this.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH8)
                        {
                            if (_DockingPlate.GetState() == DockingPlateStateEnum.LOCK &&
                           _DockingPort.GetState() == DockingPortStateEnum.IN &&
                           _DockingPort40.GetState() == DockingPort40StateEnum.OUT &&
                           _Cover.EnumState == FoupCoverStateEnum.OPEN &&
                           _CassetteOpener.GetState() == FoupCassetteOpenerStateEnum.LOCK)
                            {
                                ModuleState = new FoupLoadState(this);
                                retVal = EventCodeEnum.NONE;
                            }
                            else if (_DockingPlate.GetState() == DockingPlateStateEnum.UNLOCK &&
                                _DockingPort.GetState() == DockingPortStateEnum.OUT &&
                                _DockingPort40.GetState() == DockingPort40StateEnum.OUT &&
                                _Cover.EnumState == FoupCoverStateEnum.OPEN &&
                                _CassetteOpener.GetState() == FoupCassetteOpenerStateEnum.LOCK)
                            {
                                ModuleState = new FoupUnLoadState(this);
                                retVal = EventCodeEnum.NONE;
                            }
                            else
                            {
                                ModuleState = new FoupErrorState(this);
                                retVal = EventCodeEnum.NONE;
                            }
                        }
                        else if (this.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH12)
                        {
                            if (_DockingPlate.GetState() == DockingPlateStateEnum.LOCK &&
                           _DockingPort.GetState() == DockingPortStateEnum.IN &&
                           _DockingPort40.GetState() == DockingPort40StateEnum.OUT &&
                           _Cover.EnumState == FoupCoverStateEnum.OPEN &&
                           _CassetteOpener.GetState() == FoupCassetteOpenerStateEnum.UNLOCK)
                            {
                                ModuleState = new FoupLoadState(this);
                                retVal = EventCodeEnum.NONE;
                            }
                            else if (_DockingPlate.GetState() == DockingPlateStateEnum.UNLOCK &&
                                _DockingPort.GetState() == DockingPortStateEnum.OUT &&
                                _DockingPort40.GetState() == DockingPort40StateEnum.OUT &&
                                _Cover.EnumState == FoupCoverStateEnum.OPEN &&
                                _CassetteOpener.GetState() == FoupCassetteOpenerStateEnum.LOCK)
                            {
                                ModuleState = new FoupUnLoadState(this);
                                retVal = EventCodeEnum.NONE;
                            }
                            else
                            {
                                ModuleState = new FoupErrorState(this);
                                retVal = EventCodeEnum.NONE;
                            }
                        }

                    }
                    else
                    {
                        LoggerManager.Debug($"Docking plate state = {_DockingPlate.GetState()}");
                        LoggerManager.Debug($"Docking port state = {_DockingPort.GetState()}");
                        LoggerManager.Debug($"Docking port extender(DockingPort40) state = {_DockingPort40.GetState()}");
                        LoggerManager.Debug($"Cassette opener state = {_CassetteOpener.GetState()}");
                        LoggerManager.Debug($"Cover state = {_Cover.EnumState}");
                        LoggerManager.Debug($"Door state = {_Door.GetState()}");
                        LoggerManager.Debug($"Tilt state = {_Tilt.GetState()}");

                        ModuleState = new FoupErrorState(this);
                        retVal = EventCodeEnum.NONE;
                    }
                }
                else if (SystemParam.FoupType.Value == FoupTypeEnum.UNDEFINED)
                {
                    LoggerManager.Debug($"Docking plate state = {_DockingPlate.GetState()}");
                    LoggerManager.Debug($"Docking port state = {_DockingPort.GetState()}");
                    LoggerManager.Debug($"Docking port extender(DockingPort40) state = {_DockingPort40.GetState()}");
                    LoggerManager.Debug($"Cassette opener state = {_CassetteOpener.GetState()}");
                    LoggerManager.Debug($"Cover state = {_Cover.EnumState}");
                    LoggerManager.Debug($"Door state = {_Door.GetState()}");
                    LoggerManager.Debug($"Tilt state = {_Tilt.GetState()}");

                    bool ischeckCassetteopener = true;
                    var size = this.GPLoader.GetDeviceSize(this.FoupIndex);
                    if (size == SubstrateSizeEnum.INCH6 || size == SubstrateSizeEnum.INCH8)
                    {
                        LoggerManager.Debug($"UpdateFoupState (): Wafer size = {size}");
                        ischeckCassetteopener = false;
                    }

                    if (ischeckCassetteopener)
                    {
                        if (_DockingPlate.GetState() == DockingPlateStateEnum.LOCK &&
                                     _DockingPort.GetState() == DockingPortStateEnum.IN &&
                                     _CassetteOpener.GetState() == FoupCassetteOpenerStateEnum.UNLOCK)
                        {
                            ModuleState = new FoupLoadState(this);
                            ChangeFoupServiceStatus(GEMFoupStateEnum.TRANSFER_BLOCKED);
                        }
                        else if (_DockingPlate.GetState() == DockingPlateStateEnum.UNLOCK &&
                                    _DockingPort.GetState() == DockingPortStateEnum.OUT &&
                                    _CassetteOpener.GetState() == FoupCassetteOpenerStateEnum.LOCK &&
                                    _Cover.EnumState == FoupCoverStateEnum.CLOSE)
                        {
                            ModuleState = new FoupUnLoadState(this);
                            //ChangeFoupServiceStatus(GEMFoupStateEnum.TRANSFER_READY);
                            if (foupInfo.FoupPRESENCEState == FoupPRESENCEStateEnum.CST_ATTACH)
                            {
                                ChangeFoupServiceStatus(GEMFoupStateEnum.READY_TO_UNLOAD);
                                if (Extensions_IParam.LoadProgramFlag == true)
                                {
                                    this.E84Module().SetSignal(FoupIndex + 1, E84OPModuleTypeEnum.FOUP, E84SignalTypeEnum.HO_AVBL, true);
                                }
                            }
                            else
                            {
                                //ChangeFoupServiceStatus(GEMFoupStateEnum.READY_TO_LOAD);
                                if (Extensions_IParam.LoadProgramFlag == true)
                                {
                                    this.E84Module().SetSignal(FoupIndex + 1, E84OPModuleTypeEnum.FOUP, E84SignalTypeEnum.HO_AVBL, true);
                                }
                            }
                        }
                        else if (_DockingPlate.GetState() == DockingPlateStateEnum.UNLOCK &&
                            _DockingPort.GetState() == DockingPortStateEnum.IN &&
                            _CassetteOpener.GetState() == FoupCassetteOpenerStateEnum.UNLOCK)
                        {
                            if (foupInfo.FoupPRESENCEState == FoupPRESENCEStateEnum.CST_DETACH)
                            {
                                ModuleState = new FoupEMPTY_CASSETTEState(this);
                                ChangeFoupServiceStatus(GEMFoupStateEnum.TRANSFER_READY);
                                ChangeFoupServiceStatus(GEMFoupStateEnum.READY_TO_LOAD);
                            }

                        }
                        else
                        {
                            if (_DockingPlate.GetState() == DockingPlateStateEnum.LOCK &&
                                    _CassetteOpener.GetState() == FoupCassetteOpenerStateEnum.LOCK &&
                                    _Cover.EnumState == FoupCoverStateEnum.CLOSE)
                            {
                                if (ModuleState.State == FoupStateEnum.UNLOAD || ModuleState.State == FoupStateEnum.LOADING)
                                {
                                    ModuleState = new FoupLoadingState(this);
                                }
                                else
                                {
                                    ModuleState = new FoupUnLoadingState(this);
                                }

                            }
                            else if (ModuleState.State == FoupStateEnum.LOADING || ModuleState.State == FoupStateEnum.UNLOADING)
                            {

                            }
                            else
                            {
                                ModuleState = new FoupErrorState(this);
                                ChangeFoupServiceStatus(GEMFoupStateEnum.TRANSFER_BLOCKED);
                            }

                        }

                    }
                    else
                    {
                        if (_DockingPlate.GetState() == DockingPlateStateEnum.LOCK &&
                           _DockingPort.GetState() == DockingPortStateEnum.IN)
                        {
                            ModuleState = new FoupLoadState(this);
                            ChangeFoupServiceStatus(GEMFoupStateEnum.TRANSFER_BLOCKED);
                        }
                        else if (_DockingPlate.GetState() == DockingPlateStateEnum.UNLOCK &&
                            _DockingPort.GetState() == DockingPortStateEnum.OUT)
                        {
                            ModuleState = new FoupUnLoadState(this);
                            //ChangeFoupServiceStatus(GEMFoupStateEnum.TRANSFER_READY);
                            if (foupInfo.FoupPRESENCEState == FoupPRESENCEStateEnum.CST_ATTACH)
                            {
                                ChangeFoupServiceStatus(GEMFoupStateEnum.READY_TO_UNLOAD);
                                if (Extensions_IParam.LoadProgramFlag == true)
                                {
                                    this.E84Module().SetSignal(FoupIndex + 1, E84OPModuleTypeEnum.FOUP, E84SignalTypeEnum.HO_AVBL, true);
                                }
                            }
                            else
                            {
                                //ChangeFoupServiceStatus(GEMFoupStateEnum.READY_TO_LOAD);
                                if (Extensions_IParam.LoadProgramFlag == true)
                                {
                                    this.E84Module().SetSignal(FoupIndex + 1, E84OPModuleTypeEnum.FOUP, E84SignalTypeEnum.HO_AVBL, true);
                                }
                            }
                        }
                        else if (_DockingPlate.GetState() == DockingPlateStateEnum.UNLOCK &&
                            _DockingPort.GetState() == DockingPortStateEnum.IN)

                        {
                            if (foupInfo.FoupPRESENCEState == FoupPRESENCEStateEnum.CST_DETACH)
                            {
                                ModuleState = new FoupEMPTY_CASSETTEState(this);
                                ChangeFoupServiceStatus(GEMFoupStateEnum.TRANSFER_READY);
                                ChangeFoupServiceStatus(GEMFoupStateEnum.READY_TO_LOAD);
                            }

                        }
                        else
                        {
                            if (ModuleState.State == FoupStateEnum.UNLOADING || ModuleState.State == FoupStateEnum.LOADING)
                            {

                            }
                            else
                            {
                                ModuleState = new FoupErrorState(this);
                                ChangeFoupServiceStatus(GEMFoupStateEnum.TRANSFER_BLOCKED);
                            }
                        }
                    }
                    if (GPCommand != null)
                    {
                        var moduleInfo = GetFoupModuleInfo();
                        GPCommand.RaiseFoupModuleStateChanged(moduleInfo);

                        // EMUL에서 지원 안됨.
                        //foreach (var output in DockingPlate.Outputs)
                        //{
                        //    GPLoader.CheckIOValue(output);
                        //}
                        //foreach (var output in DockingPort.Outputs)
                        //{
                        //    GPLoader.CheckIOValue(output);
                        //}
                        //foreach (var output in Cover.Outputs)
                        //{
                        //    GPLoader.CheckIOValue(output);
                        //}
                        //foreach (var output in CassetteOpener.Outputs)
                        //{
                        //    GPLoader.CheckIOValue(output);
                        //}
                    }

                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Debug($"Unknown");
                    this.E84Module().SetSignal(FoupIndex + 1, E84OPModuleTypeEnum.FOUP, E84SignalTypeEnum.HO_AVBL, false);
                    ModuleState = new FoupErrorState(this);
                    retVal = EventCodeEnum.NONE;
                }
                #endregion

                UpdateFoupLampState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;

        }


        public EventCodeEnum UpdateFosBFoupState()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            if (GPCommand != null)
            {
                var moduleInfo = GetFoupModuleInfo();
                GPCommand.RaiseFoupModuleStateChanged(moduleInfo);
            }
            return retVal;
        }
        public EventCodeEnum InitProcedures()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            return retVal;
        }
#pragma warning disable 0649
        private List<IIOBase> _IoDeviceList;
#pragma warning restore 0649

        private bool _BlinkRun;
        private Task _BlinkRunTask;
        private ConcurrentDictionary<IOPortDescripter<bool>, Object> _BlinkPortDescBag = new ConcurrentDictionary<IOPortDescripter<bool>, Object>();
        public void SetLEDStatus(FoupPRESENCEStateEnum LOADLEDStatus = FoupPRESENCEStateEnum.CST_DETACH, FoupPRESENCEStateEnum UNLOADLEDStatus = FoupPRESENCEStateEnum.CST_DETACH)
        {
            try
            {
                bool LoadLEDOnFlag = true;
                if (this.IOManager.IOMap.Outputs.DO_LOAD_LAMP.Reverse.Value)
                    LoadLEDOnFlag = false;
                //==>> Load LED
                switch (LOADLEDStatus)
                {
                    case FoupPRESENCEStateEnum.CST_ATTACH:
                        Blink_LED(this.IOManager.IOMap.Outputs.DO_LOAD_LAMP, false); // ==>> LOAD LED BLINK OFF
                        SetLamp(this.IOManager.IOMap.Outputs.DO_LOAD_LAMP, LoadLEDOnFlag); // ==>>LOAD LED ON
                        break;

                    case FoupPRESENCEStateEnum.CST_DETACH:
                        Blink_LED(this.IOManager.IOMap.Outputs.DO_LOAD_LAMP, false); // ==>> LOAD LED BLINK OFF
                        SetLamp(this.IOManager.IOMap.Outputs.DO_LOAD_LAMP, !LoadLEDOnFlag); // ==>>LOAD LED OFF
                        break;
                    case FoupPRESENCEStateEnum.CST_NOT_MATCHED:
                        Blink_LED(this.IOManager.IOMap.Outputs.DO_LOAD_LAMP, true); // ==>> LOAD LED BLINK ON

                        break;
                }
                /*
                bool UnloadLEDOnFlag = true;
                if (this.IOManager.IOMap.Outputs.DO_UNLOAD_LAMP.Reverse.Value)
                    UnloadLEDOnFlag = false;
                */
                //==>> UnLoad LED
                switch (UNLOADLEDStatus)
                {
                    case FoupPRESENCEStateEnum.CST_ATTACH:
                        Blink_LED(this.IOManager.IOMap.Outputs.DO_UNLOAD_LAMP, false); // ==>> LOAD LED BLINK OFF
                        SetLamp(this.IOManager.IOMap.Outputs.DO_UNLOAD_LAMP, LoadLEDOnFlag); // ==>>LOAD LED ON
                        break;

                    case FoupPRESENCEStateEnum.CST_DETACH:
                        Blink_LED(this.IOManager.IOMap.Outputs.DO_UNLOAD_LAMP, false); // ==>> LOAD LED BLINK OFF
                        SetLamp(this.IOManager.IOMap.Outputs.DO_UNLOAD_LAMP, !LoadLEDOnFlag); // ==>>LOAD LED OFF
                        break;
                    case FoupPRESENCEStateEnum.CST_NOT_MATCHED:
                        Blink_LED(this.IOManager.IOMap.Outputs.DO_UNLOAD_LAMP, true); // ==>> LOAD LED BLINK ON

                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void SetLamp(IOPortDescripter<bool> portDesc, bool value)
        {
            try
            {
                if (_IoDeviceList != null)
                {
                    //==> Channel Descripter Index
                    int channelDescIdx = portDesc.ChannelIndex.Value;//=> 

                    //==> Channel Descripter Info
                    int devIdx = IOManager.IOServ.Outputs[channelDescIdx].DevIndex;//==> Device Number
                    int hwChannel = IOManager.IOServ.Outputs[channelDescIdx].ChannelIndex;//==> Hardware Channel
                    int port = portDesc.PortIndex.Value;

                    if (_IoDeviceList.Count > 0)
                        _IoDeviceList[devIdx].WriteBit(hwChannel, port, value);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void Blink_LED(IOPortDescripter<bool> portDesc, bool on)
        {
            try
            {

                if (on)
                {
                    if (_BlinkPortDescBag.Keys.Contains(portDesc) == false)
                        _BlinkPortDescBag.TryAdd(portDesc, null);
                    if (_BlinkRun == false)
                        RunBlink();//==> Run Port Blink
                }
                else
                {
                    Object removePortDesc;
                    if (_BlinkPortDescBag.Keys.Contains(portDesc))
                        _BlinkPortDescBag.TryRemove(portDesc, out removePortDesc);
                    if (_BlinkPortDescBag.Count == 0)
                        StopBlink();//==> Stop Port Blink
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void RunBlink()
        {
            try
            {

                if (_BlinkRun)
                    return;//==> 이미 실행중이어서 더 실행 시킬 필요 없음.

                StopBlink();//==> 안전 장치

                _BlinkRun = true;


                _BlinkRunTask = Task.Run(() =>
                {
                    const int blinkInterval = 500;
                    bool value = false;

                    do
                    {
                        foreach (var portDesc in _BlinkPortDescBag.Keys)
                            SetLamp(portDesc, value);

                        //Thread.Sleep(blinkInterval);
                        //delays.DelayFor(blinkInterval);
                        Thread.Sleep(blinkInterval);
                        value = !value;
                    } while (_BlinkRun);
                });

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void StopBlink()
        {
            try
            {
                _BlinkRun = false;
                if (_BlinkRunTask != null)
                    _BlinkRunTask.Wait();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }


        public void RisingRFIDEvent(bool success, string casssetteId)
        {
            try
            {
                PIVInfo pivinfo = new PIVInfo(foupnumber: FoupIndex + 1, carrierid: casssetteId);
                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                if (success)
                {
                    this.EventManager().RaisingEvent(typeof(CassetteIDReadDoneEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                }
                else
                {
                    this.EventManager().RaisingEvent(typeof(CassetteIDReadFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                }
                semaphore.Wait();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public string Read_CassetteID()
        {
            string retVal = "";
            try
            {
                var cassetteIDReader = (CassetteIDReaderModule as CassetteIDReaderModule);
                if (cassetteIDReader != null)
                {
                    if (cassetteIDReader.CSTIDReader != null)
                    {
                        //RFID Adapter(Multple) 따로 먼저 체크.
                        var rfidModule = (cassetteIDReader.CSTIDReader as RFIDModule);
                        if (rfidModule != null)
                        {
                            if (rfidModule.RFIDSysParam.RFIDProtocolType == EnumRFIDProtocolType.MULTIPLE)
                            {
                                var rfidAdapter = rfidModule.GetRFIDAdapter();

                                if (rfidAdapter == null || rfidAdapter.CommModule == null)
                                {
                                    LoggerManager.Debug($"[RFID Adapter] RFID #{FoupIndex + 1} Read Fail, Adpater is null.");
                                    PIVInfo pivinfo = new PIVInfo(foupnumber: FoupIndex + 1, carrierid: " ");
                                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                    this.EventManager().RaisingEvent(typeof(CassetteIDReadFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                    semaphore.Wait();
                                    return retVal;
                                }
                                if (rfidAdapter.CommModule.GetCommState() == EnumCommunicationState.DISCONNECT
                                    || rfidAdapter.CommModule.GetCommState() == EnumCommunicationState.UNAVAILABLE)
                                {
                                    LoggerManager.Debug($"[RFID Adapter] RFID #{FoupIndex + 1} Read Fail, Adapter CommModule is disconnected.");
                                    PIVInfo pivinfo = new PIVInfo(foupnumber: FoupIndex + 1, carrierid: " ");
                                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                    this.EventManager().RaisingEvent(typeof(CassetteIDReadFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                    semaphore.Wait();
                                    return retVal;
                                }

                                string rfidAdapterId = cassetteIDReader.CSTIDReader.ReadCassetteID();
                                if (string.IsNullOrEmpty(rfidAdapterId))
                                {
                                    //Fail read Cassette ID

                                    LoggerManager.Debug($"[RFID Adapter] Module #{FoupIndex + 1} Read Fail, Id is null.");
                                    PIVInfo pivinfo = new PIVInfo(foupnumber: FoupIndex + 1, carrierid: " ");
                                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                    this.EventManager().RaisingEvent(typeof(CassetteIDReadFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                    semaphore.Wait();

                                    if (FoupCarrierIdChangedEvent != null)
                                    {
                                        FoupCarrierIdChangedEvent(FoupIndex + 1, "");
                                    }

                                    retVal = rfidAdapterId;
                                }
                                else
                                {
                                    //Sucess read RFID
                                    LoggerManager.Debug($"[RFID Adapter] Module #{FoupIndex + 1} Read Success ID: {rfidAdapterId}");
                                    PIVInfo pivinfo = new PIVInfo(foupnumber: FoupIndex + 1, carrierid: rfidAdapterId);
                                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                    this.EventManager().RaisingEvent(typeof(CassetteIDReadDoneEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                    semaphore.Wait();

                                    if (FoupCarrierIdChangedEvent != null)
                                    {
                                        FoupCarrierIdChangedEvent(FoupIndex + 1, rfidAdapterId);
                                    }

                                    retVal = rfidAdapterId;
                                }
                                return retVal;
                            }
                        }

                        //cassetteIDReader는 RFID Single, Barcode 둘 중 하나임.
                        if (cassetteIDReader.CSTIDReader.CommModule == null)
                        {
                            LoggerManager.Debug($"[CassetteIDReaderModule] Module #{FoupIndex + 1} Read Fail, CommModule is null.");
                            PIVInfo pivinfo = new PIVInfo(foupnumber: FoupIndex + 1, carrierid: " ");
                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                            this.EventManager().RaisingEvent(typeof(CassetteIDReadFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                            semaphore.Wait();
                            return retVal;
                        }
                        if (cassetteIDReader.CSTIDReader.CommModule.GetCommState() == EnumCommunicationState.DISCONNECT
                            || cassetteIDReader.CSTIDReader.CommModule.GetCommState() == EnumCommunicationState.UNAVAILABLE)
                        {
                            LoggerManager.Debug($"[CassetteIDReaderModule] Module #{FoupIndex + 1} Read Fail, CommModule is disconncted.");
                            PIVInfo pivinfo = new PIVInfo(foupnumber: FoupIndex + 1, carrierid: "");
                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                            this.EventManager().RaisingEvent(typeof(CassetteIDReadFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                            semaphore.Wait();
                            return retVal;
                        }

                        string id = cassetteIDReader.CSTIDReader.ReadCassetteID();
                        if (string.IsNullOrEmpty(id))
                        {
                            //Fail read Cassette ID

                            LoggerManager.Debug($"[CassetteIDReaderModule] Module #{FoupIndex + 1} Read Fail, Id is null.");
                            PIVInfo pivinfo = new PIVInfo(foupnumber: FoupIndex + 1, carrierid: " ");
                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                            this.EventManager().RaisingEvent(typeof(CassetteIDReadFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                            semaphore.Wait();

                            if (FoupCarrierIdChangedEvent != null)
                            {
                                FoupCarrierIdChangedEvent(FoupIndex + 1, "");
                            }

                            retVal = id;
                        }
                        else
                        {
                            //Sucess read RFID
                            LoggerManager.Debug($"[CassetteIDReaderModule] Module #{FoupIndex + 1} Read Success ID: {id}");
                            PIVInfo pivinfo = new PIVInfo(foupnumber: FoupIndex + 1, carrierid: id);
                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                            this.EventManager().RaisingEvent(typeof(CassetteIDReadDoneEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                            semaphore.Wait();

                            if (FoupCarrierIdChangedEvent != null)
                            {
                                FoupCarrierIdChangedEvent(FoupIndex + 1, id);
                            }

                            retVal = id;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void ClearCassetteID()
        {
            try
            {
                this.GEMModule().GetPIVContainer().SetCarrierId(FoupIndex + 1, "");
                if (FoupCarrierIdChangedEvent != null)
                {
                    FoupCarrierIdChangedEvent(FoupIndex + 1, "");
                }
                if (RFIDModule != null)
                {
                    RFIDModule.ClearRFID();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ChangeFoupServiceStatus(GEMFoupStateEnum state, bool forcewrite = false, bool forced_event = false)
        {
            try
            {
                var curFoupState = this.GEMModule().GetPIVContainer().GetFoupState(FoupIndex + 1);
                if (forced_event)
                {
                    LoggerManager.Debug($"ChangeFoupServiceStatus(): Forced Rising Event. CurFoupState:{curFoupState}");
                }
                if (curFoupState != state || forced_event == true)
                {
                    if (state == GEMFoupStateEnum.IN_SERVICE)
                    {
                        PIVInfo pivinfo = new PIVInfo(foupnumber: FoupIndex + 1);
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(FoupInServiceEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();

                        this.GEMModule().GetPIVContainer().SetFoupState(FoupIndex + 1, state);
                    }
                    else if (state == GEMFoupStateEnum.OUT_OF_SERVICE)
                    {
                        PIVInfo pivinfo = new PIVInfo(foupnumber: FoupIndex + 1);
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(FoupOutOfServiceEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();

                        this.GEMModule().GetPIVContainer().SetFoupState(FoupIndex + 1, state);
                    }
                    else if (state == GEMFoupStateEnum.READY_TO_LOAD)
                    {
                        PIVInfo pivinfo = new PIVInfo(foupnumber: FoupIndex + 1);
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(FoupReadyToLoadEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();

                        this.GEMModule().GetPIVContainer().SetFoupState(FoupIndex + 1, state);
                    }
                    else if (state == GEMFoupStateEnum.READY_TO_UNLOAD)
                    {
                        PIVInfo pivinfo = new PIVInfo(foupnumber: FoupIndex + 1);
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(FoupReadyToUnloadEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();

                        this.GEMModule().GetPIVContainer().SetFoupState(FoupIndex + 1, state);
                    }
                    else if (state == GEMFoupStateEnum.TRANSFER_READY)
                    {
                        if ((curFoupState != GEMFoupStateEnum.READY_TO_LOAD &&
                            curFoupState != GEMFoupStateEnum.READY_TO_UNLOAD) || (forcewrite == true))
                        {
                            PIVInfo pivinfo = new PIVInfo(foupnumber: FoupIndex + 1);
                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                            this.EventManager().RaisingEvent(typeof(FoupTransferReady).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                            semaphore.Wait();

                            this.GEMModule().GetPIVContainer().SetFoupState(FoupIndex + 1, state);
                        }
                    }
                    else if (state == GEMFoupStateEnum.TRANSFER_BLOCKED)
                    {
                        PIVInfo pivinfo = new PIVInfo(foupnumber: FoupIndex + 1);
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(FoupTransferBlockedEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();

                        this.GEMModule().GetPIVContainer().SetFoupState(FoupIndex + 1, state);
                    }
                }


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum InitSelectedProcedureStateMapNode(FoupStateEnum setfoupstate)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                retval = ProcManager.InitSelectedProcedureStateMapNode(setfoupstate);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public EventCodeEnum Refresh()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"FoupModule.Refresh(): thread lock start.");
                FplockSlim?.Wait();
                LoggerManager.RecoveryLog($"FoupInfo's PresenceState: {foupInfo.FoupPRESENCEState},");
                LoggerManager.RecoveryLog($"FoupInfo's FoupCoverState: {foupInfo.FoupCoverState},");
                LoggerManager.RecoveryLog($"FoupInfo's DockingPlateState: {foupInfo.DockingPlateState},");
                LoggerManager.RecoveryLog($"FoupInfo's DockingPortState: {foupInfo.DockingPortState},");
                LoggerManager.RecoveryLog($"FoupInfo's DockingPortDoorState: {foupInfo.DockingPortDoorState},");
                LoggerManager.RecoveryLog($"FoupInfo's OpenerState: {foupInfo.OpenerState},");
                LoggerManager.RecoveryLog($"FoupInfo's TiltState: {foupInfo.TiltState},");
                LoggerManager.RecoveryLog($"FoupInfo's WaferOutSensor: {foupInfo.WaferOutSensor},");

                if (SystemManager.SysteMode == SystemModeEnum.Multiple || this.MonitoringManager().IsSystemError == false)
                {
                    if (foupInfo.FoupPRESENCEState == FoupPRESENCEStateEnum.CST_ATTACH)
                    {
                        if (Callback.IsFoupUsingByLoader())
                        {
                            retVal = EventCodeEnum.UNDEFINED;
                        }
                        else
                        {
                            retVal = ProcManager.SequencesRefresh();
                            CheckModuleState();
                            if (GPCommand != null)
                            {
                                GPCommand.FOUPReset(this.FoupIndex);
                            }
                            UpdateFoupState();
                            BroadcastFoupStateAsync();
                        }
                    }
                    else
                    {
                        LoggerManager.RecoveryLog($"Foup PresenceState: {foupInfo.FoupPRESENCEState}, FoupPReSENCEState is required {FoupPRESENCEStateEnum.CST_ATTACH}", true);
                        retVal = EventCodeEnum.FOUP_ERROR;
                    }
                }
                else
                {
                    retVal = EventCodeEnum.SYSTEM_ERROR;
                    LoggerManager.RecoveryLog($"{retVal}", true);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            finally
            {
                FplockSlim?.Release();
                LoggerManager.Debug($"FoupModule.Refresh(): thread lock end.");
            }

            return retVal;
        }

        public void ShowIOErrorMessage(List<string> errorIO)
        {
            try
            {
                string joinedErrors = string.Join(Environment.NewLine, errorIO);
                this.MetroDialogManager().ShowMessageDialog("Check IO Status", $"Input/Output Check List :\n{joinedErrors}", EnumMessageStyle.Affirmative, "OK");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ShowSequenceErrorMessage(string procedurestate, List<string> safetieslist)
        {
            try
            {
                string joinedSafties = string.Join(Environment.NewLine, safetieslist);
                this.MetroDialogManager().ShowMessageDialog($"Sequence Control Error", $"It is recommended to activate the refresh button.\nReason : {procedurestate}\nCheck List :\n{joinedSafties}"
                    , EnumMessageStyle.Affirmative, "OK");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task ShowFoupErrorDialogMessage()
        {
            try
            {
                string msg = "";
                if (ProcManager.GetSelectedProcedureStateMapNode() != null)
                {
                    msg = $"{ProcManager.GetSelectedProcedureStateMapNode().Caption}";
                }

                ILoaderModule loaderModule = this.GetLoaderContainer().Resolve<ILoaderModule>();


                EnumMessageDialogResult ret = await this.MetroDialogManager().ShowMessageDialog($"Foup#{FoupIndex + 1} Error", $"{msg} is Error.\n " +
                    $"Details : {this.ErrorDetails}", EnumMessageStyle.AffirmativeAndNegative, "Recovery");

                if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    ILoaderSupervisor loaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                    if (loaderMaster.ModuleState.State == ModuleStateEnum.IDLE || loaderMaster.ModuleState.State == ModuleStateEnum.PAUSED)
                    {
                        await System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            var viewmodel = this.ViewModelManager().GetViewModelFromGuid(new Guid("0DA29549-83DA-40A2-9A6A-1D058BDA3D88")); // viewmodel
                            if (viewmodel is IGPFoupRecoveryControlVM foupRecoveryViewModel)
                            {
                                // SelectedFoup 값을 변경합니다.
                                //foupRecoveryViewModel.InitViewModel();
                                foupRecoveryViewModel.SetSelectedFoup(FoupIndex + 1);
                            }

                            this.ViewModelManager().ViewTransitionAsync(new Guid("F7BE0142-1ED7-4483-A257-AC512454F6F2")); // FoupRecoveryViewGuid
                        }));
                    }
                    else
                    {
                        await this.MetroDialogManager().ShowMessageDialog($"Loader State {LoaderState}", "You can enter the page only when the loader status is IDLE or PASUED.", EnumMessageStyle.Affirmative, "OK");
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public EventCodeEnum CheckDockingPlate()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                ProcManager.SetDockingPlateProcedure(_DockingPlate.GetState());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }
        public EventCodeEnum CheckCoverDown()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {

                ProcManager.SetCoverDownProcedure(_Cover.EnumState);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public EventCodeEnum FoupModuleReset()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public bool ValidationRFIDCommState(bool checkConnectState)
        {
            bool retVal = true;
            try
            {
                var cassetteIDReader = CassetteIDReaderModule as CassetteIDReaderModule;
                if (cassetteIDReader != null)
                {
                    if (cassetteIDReader.CSTIDReader != null)
                    {
                        var rfidModule = cassetteIDReader.CSTIDReader as RFIDModule;
                        if (rfidModule != null)
                        {
                            if (rfidModule.RFIDSysParam.RFIDProtocolType == EnumRFIDProtocolType.MULTIPLE)
                            {
                                var rfidAdapter = rfidModule.GetRFIDAdapter();

                                if (rfidAdapter == null || rfidAdapter.CommModule == null)
                                {
                                    retVal = false;
                                }
                                if (checkConnectState && retVal)
                                {
                                    if (rfidAdapter.CommModule.GetCommState() == EnumCommunicationState.DISCONNECT
                                    || rfidAdapter.CommModule.GetCommState() == EnumCommunicationState.UNAVAILABLE)
                                    {
                                        retVal = false;
                                    }
                                }

                                return retVal;
                            }
                        }

                        if (cassetteIDReader.CSTIDReader.CommModule == null)
                        {
                            retVal = false;
                        }
                        if (checkConnectState && retVal)
                        {
                            if (cassetteIDReader.CSTIDReader.CommModule.GetCommState() == EnumCommunicationState.DISCONNECT
                            || cassetteIDReader.CSTIDReader.CommModule.GetCommState() == EnumCommunicationState.UNAVAILABLE)
                            {
                                retVal = false;
                            }
                        }
                    }
                }
                else
                {
                    retVal = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum ValidationAvailableFoupState(int foupNumber)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void SetFoupOptionInfomation(FoupOptionInfomation foupOptionInfo)
        {
            try
            {

                if (foupOptionInfo != null)
                {
                    if (FoupOptionInfo == null)
                    {
                        FoupOptionInfo = new FoupOptionInfomation();
                    }
                    string str = "";
                    bool originvalue = FoupOptionInfo.IsCassetteDetectEventAfterRFID;
                    if (FoupOptionInfo.IsCassetteDetectEventAfterRFID != foupOptionInfo.IsCassetteDetectEventAfterRFID)
                    {
                        FoupOptionInfo.IsCassetteDetectEventAfterRFID = foupOptionInfo.IsCassetteDetectEventAfterRFID;
                        str += $"[value changed] IsCassetteDetectEventAfterRFID {originvalue} to {FoupOptionInfo.IsCassetteDetectEventAfterRFID}";
                    }
                    else
                    {
                        str += $"[value set] IsCassetteDetectEventAfterRFID is {originvalue}";
                    }

                    LoggerManager.Debug($"[FoupModule] FOUP#{FoupNumber} SetFoupOptionInfomation(). {str}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region // Template methods
        public EventCodeEnum ParamValidation()
        {
            return EventCodeEnum.NONE;
        }
        public bool IsParameterChanged(bool issave = false)
        {
            return false;
        }
        private TemplateStateCollection _Template;
        public TemplateStateCollection Template
        {
            get { return _Template; }
            set
            {
                if (value != _Template)
                {
                    _Template = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ITemplateFileParam _TemplateParameter;
        public ITemplateFileParam TemplateParameter
        {
            get { return _TemplateParameter; }
            set
            {
                if (value != _TemplateParameter)
                {
                    _TemplateParameter = value;
                    RaisePropertyChanged();
                }
            }
        }
        public ITemplateParam LoadTemplateParam { get; set; }
        public ISubRoutine SubRoutine { get; set; }

        ProberInterfaces.Foup.ICSTControlCommands IFoupModule.GPCommand => (ICSTControlCommands)this.GetGPLoader();

        private EventCodeEnum LoadTemplate()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                IParam tmpParam = null;
                tmpParam = new FoupModuleTemplateParam();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                retVal = this.LoadParameter(ref tmpParam, typeof(FoupModuleTemplateParam));

                TemplateParameter = (FoupModuleTemplateParam)tmpParam;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        #endregion
        
    }


    public class EmulFoupProcManager : IFoupProcedureManager, IFoupProcRunManager, INotifyPropertyChanged
    {
        public EmulFoupProcManager()
        {

        }
        public EmulFoupProcManager(IFoupModule module)
        {
            Module = module;
        }
        IFoupModule Module;

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public FoupProcedureStateMaps LoadProcedureStateMaps { get; set; } = new FoupProcedureStateMaps();
        public FoupProcedureStateMaps UnloadProcedureStateMaps { get; set; } = new FoupProcedureStateMaps();

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
        public int GetSelectedProcedureIndex()
        {
            int index = 0;

            try
            {
                if (SelectedProcedureStateMapNode != null)
                {
                    for (var node = SelectedProcedureStateMapNode.List.First; node != null; node = node.Next, index++)
                    {
                        if (SelectedProcedureStateMapNode.Value.Equals(node.Value))
                        {
                            return index;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return index;
        }
        public LinkedListNode<FoupProcedureStateMap> SelectedProcedureStateMapNode { get; set; } = null;
        //private FoupProcedureStateMaps SelectedProcedureStateMapNode { get; set; } = null;

        public void InitProcedures()
        {
            try
            {
                foreach (var loadProcedureStateMap in LoadProcedureStateMaps)
                {
                    loadProcedureStateMap.ProcedureState = new FoupProcedureIdle();
                }

                foreach (var unloadProcedureStateMap in UnloadProcedureStateMaps)
                {
                    unloadProcedureStateMap.ProcedureState = new FoupProcedureIdle();
                }

                SelectedProcedureStateMapNode = null;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public EventCodeEnum ReverseRun()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                retVal = SelectedProcedureStateMapNode.Value.Procedure.ReverseProcedure.PreSafetiesRun();

                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = SelectedProcedureStateMapNode.Value.Procedure.ReverseProcedure.BehaviorRun();
                }

                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = SelectedProcedureStateMapNode.Value.Procedure.ReverseProcedure.PostSafetiesRun();
                }
                //if (retVal == EventCodeEnum.NONE)
                //{
                //    SelectedProcedureStateMapNode = SelectedProcedureStateMapNode.Previous;
                //}
                //else
                //{
                //    // ???
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public EventCodeEnum PreviousRun()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                if (SelectedProcedureStateMapNode != null && SelectedProcedureStateMapNode.Previous != null)
                {
                    if (SelectedProcedureStateMapNode.Previous.Value.Procedure.Caption == "FOUP COVER DOWN")
                    {
                        return EventCodeEnum.FOUP_SEQUENCE_NULL;
                    }

                    SelectedProcedureStateMapNode = SelectedProcedureStateMapNode.Previous;
                    LoggerManager.RecoveryLog($"Pre Foup Sequence : {SelectedProcedureStateMapNode.Value.Procedure.Caption}, Clear Sequence state");

                    retVal = SelectedProcedureStateMapNode.Value.PreviousRun();
                    if (retVal == EventCodeEnum.NONE)
                    {
                        SelectedProcedureStateMapNode.Value.Procedure.Behavior.FoupModule.BroadcastFoupStateAsync();
                        return retVal;
                    }
                    else
                    {
                        List<string> presaftieserrorlist = new List<string>();
                        List<string> postsaftieserrorlist = new List<string>();

                        foreach (var safety in SelectedProcedureStateMapNode.Value.Procedure.PreSafeties_Recovery)
                        {
                            if (safety.State != FoupSafetyStateEnum.DONE)
                            {
                                int index = safety.ToString().IndexOf("I_");

                                if (index != -1)
                                {
                                    string parsedContent = safety.ToString().Substring(index + 2);
                                    presaftieserrorlist.Add(parsedContent);
                                }

                            }
                        }

                        foreach (var safety in SelectedProcedureStateMapNode.Value.Procedure.PostSafeties_Recovery)
                        {
                            if (safety.State != FoupSafetyStateEnum.DONE)
                            {
                                int index = safety.ToString().IndexOf("I_");

                                if (index != -1)
                                {
                                    string parsedContent = safety.ToString().Substring(index + 2);
                                    postsaftieserrorlist.Add(parsedContent);
                                }

                            }
                        }

                        if (presaftieserrorlist.Count > 0)
                        {
                            SelectedProcedureStateMapNode.Value.Procedure.Behavior.FoupModule.ShowSequenceErrorMessage("PreSafties Error", presaftieserrorlist);
                        }

                        if (postsaftieserrorlist.Count > 0)
                        {
                            SelectedProcedureStateMapNode.Value.Procedure.Behavior.FoupModule.ShowSequenceErrorMessage("PostSafties Error", postsaftieserrorlist);
                        }
                        retVal = EventCodeEnum.FOUP_ERROR;
                    }

                }
                else
                {
                    retVal = EventCodeEnum.FOUP_SEQUENCE_NULL;
                    LoggerManager.RecoveryLog($"Reason : {retVal}", true);

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }

        public EventCodeEnum LoadRun()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                InitProcedures();
                if (SelectedProcedureStateMapNode == null)
                {
                    SelectedProcedureStateMaps = LoadProcedureStateMaps;

                    SelectedProcedureStateMapNode = (SelectedProcedureStateMaps as FoupProcedureStateMaps).First;
                }

                retVal = ProceduresRun();
                //Module.DockingPlate.Lock();
                //Module.DockingPort.In();
                //Module.CassetteOpener.Unlock();
                //Module.Cover.Open();
                //return EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum UnloadRun()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                InitProcedures();
                if (SelectedProcedureStateMapNode == null)
                {
                    SelectedProcedureStateMaps = UnloadProcedureStateMaps;
                    SelectedProcedureStateMapNode = (SelectedProcedureStateMaps as FoupProcedureStateMaps).First;
                }

                retVal = ProceduresRun();
                //Module.Cover.Close();
                //Module.CassetteOpener.Lock();
                //Module.DockingPort.Out();
                //Module.DockingPlate.Unlock();

                //return EventCodeEnum.NONE;
                //var task = AsyncProceduresRun();

                //retVal = task.Result;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
        private EventCodeEnum ProceduresRun()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                bool EndFlag = false;

                while (SelectedProcedureStateMapNode != null)
                {
                    retVal = SelectedProcedureStateMapNode.Value.Run();

                    SelectedProcedureStateMapNode.Value.Procedure.Behavior.FoupModule.BroadcastFoupStateAsync();

                    if (retVal != EventCodeEnum.NONE)
                    {
                        EndFlag = false;
                        break;
                    }
                    else
                    {
                        EndFlag = true;
                        SelectedProcedureStateMapNode = SelectedProcedureStateMapNode.Next;

                    }
                }

                if (EndFlag == true)
                {
                    SelectedProcedureStateMapNode = null;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
        public EventCodeEnum NextRun()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                if (SelectedProcedureStateMapNode != null)
                {
                    retVal = SelectedProcedureStateMapNode.Value.Run();

                    if (retVal == EventCodeEnum.NONE)
                    {
                        SelectedProcedureStateMapNode = SelectedProcedureStateMapNode.Next;
                        SelectedProcedureStateMapNode.Value.Procedure.Behavior.FoupModule.BroadcastFoupStateAsync();

                    }
                    else
                    {
                        // ???
                    }
                }
                else
                {
                    // ???
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
        public EventCodeEnum ContinueRun()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                if (SelectedProcedureStateMapNode != null)
                {
                    retVal = SelectedProcedureStateMapNode.Value.RecoveryRun();

                    if (retVal == EventCodeEnum.NONE)
                    {
                        SelectedProcedureStateMapNode.Value.Procedure.Behavior.FoupModule.BroadcastFoupStateAsync();
                        SelectedProcedureStateMapNode = SelectedProcedureStateMapNode.Next;

                        if (SelectedProcedureStateMapNode == null)
                        {
                            retVal = EventCodeEnum.FOUP_SEQUENCE_END;
                        }
                    }
                    else
                    {
                        retVal = EventCodeEnum.FOUP_ERROR; //동작 error
                    }
                }
                else
                {
                    retVal = EventCodeEnum.FOUP_SEQUENCE_NULL;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
        public EventCodeEnum FastForwardRun()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                retVal = ProceduresRun();

                //retVal = ProceduresRun(ref isEndProcedure);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
        public EventCodeEnum FastBackwardRun()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                retVal = PreviousRun();

                //retVal = ProceduresRun(ref isEndProcedure);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public void SettingProcedure(List<IFoupProcedure> foupProcedures
                                    , List<string> LoadOrderList
                                    , List<string> UnloadOrderList
                                    , IFoupModule FoupModule
                                    , IFoupIOStates FoupIOManager)
        {
            try
            {
                this.LoadProcedureStateMaps.Clear();
                this.UnloadProcedureStateMaps.Clear();

                SetFoupModule(foupProcedures, FoupModule);
                SetFoupIOManager(foupProcedures, FoupIOManager);

                SetReverse(foupProcedures);

                SetLoadProcedureStateMaps(LoadProcedureStateMaps, foupProcedures, LoadOrderList);
                SetLoadProcedureStateMaps(UnloadProcedureStateMaps, foupProcedures, UnloadOrderList);

                foreach (var procedure in foupProcedures)
                {
                    procedure.Behavior.InitBehavior();
                }
                InitProcedures();

                //Procedures = SelectedProcedureStateMapNode;

                //LoadProcedure = LoadProcedureStateMaps;
                //UnloadProcedure = UnloadProcedureStateMaps;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void SetFoupModule(List<IFoupProcedure> procs, IFoupModule FoupModule)
        {
            try
            {
                foreach (var Procedure in procs)
                {
                    FoupProcedure proc = Procedure as FoupProcedure;

                    proc.Behavior.FoupModule = FoupModule;

                    foreach (var Item in proc.PreSafeties_Recovery)
                    {
                        Item.FoupModule = FoupModule;
                    }
                    foreach (var Item in proc.PostSafeties_Recovery)
                    {
                        Item.FoupModule = FoupModule;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void SetFoupIOManager(List<IFoupProcedure> procs, IFoupIOStates FoupIOManager)
        {
            try
            {
                foreach (var Procedure in procs)
                {
                    FoupProcedure proc = Procedure as FoupProcedure;

                    proc.Behavior.FoupIOManager = FoupIOManager;

                    foreach (var Item in proc.PreSafeties_Recovery)
                    {
                        Item.FoupIOManager = FoupIOManager;
                    }
                    foreach (var Item in proc.PostSafeties_Recovery)
                    {
                        Item.FoupIOManager = FoupIOManager;
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void SetReverse(List<IFoupProcedure> procs)
        {
            try
            {
                List<FoupProcedure> foupProcs = new List<FoupProcedure>();

                foreach (var item in procs)
                {
                    foupProcs.Add(item as FoupProcedure);
                }


                foreach (var procedure in foupProcs)
                {
                    FoupProcedure findProcedure =
                        foupProcs.Find(i => i.Behavior.GetType().Name == procedure.ReverseProcedureName);
                    if (findProcedure != null)
                    {
                        procedure.ReverseProcedure = findProcedure;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void SetLoadProcedureStateMaps(FoupProcedureStateMaps targetStateMaps
                                            , List<IFoupProcedure> foupProcedures
                                            , List<string> orderlist)
        {
            try
            {
                foreach (var procedure in orderlist)
                {
                    FoupProcedure findProcedure = (FoupProcedure)foupProcedures.Find(i => i.Behavior.GetType().Name == procedure);
                    if (findProcedure != null)
                    {
                        targetStateMaps.AddLast(new FoupProcedureStateMap() { Procedure = findProcedure });
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        //public IFoupProcedureStateMaps CurProcecdureStateMapNode()
        //{
        //    return SelectedProcedureStateMaps;
        //}

        public IFoupProcedure GetSelectedProcedureStateMapNode()
        {
            IFoupProcedure procedurestatemapnode = null;

            try
            {
                if (SelectedProcedureStateMapNode != null)
                {
                    procedurestatemapnode = SelectedProcedureStateMapNode.Value.Procedure;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return procedurestatemapnode;
        }

        public IFoupProcedureStateMaps GetProcedures(FoupStateEnum foupstate)
        {
            IFoupProcedureStateMaps procedures = null;

            try
            {
                if (foupstate == FoupStateEnum.LOAD)
                {
                    procedures = (IFoupProcedureStateMaps)this.LoadProcedureStateMaps;
                }
                else if (foupstate == FoupStateEnum.UNLOAD)
                {
                    procedures = (IFoupProcedureStateMaps)this.UnloadProcedureStateMaps;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return procedures;
        }

        public EventCodeEnum InitSelectedProcedureStateMapNode(FoupStateEnum setfoupstate)
        {
            EventCodeEnum retval = EventCodeEnum.FOUP_SEQUENCE_NULL;

            try
            {
                if (setfoupstate == FoupStateEnum.LOAD)
                {
                    foreach (var loadProcedureStateMap in LoadProcedureStateMaps)
                    {
                        loadProcedureStateMap.ProcedureState = new FoupProcedureIdle();
                    }

                    SelectedProcedureStateMaps = LoadProcedureStateMaps;
                }
                else if (setfoupstate == FoupStateEnum.UNLOAD)
                {

                    foreach (var unloadProcedureStateMap in UnloadProcedureStateMaps)
                    {
                        unloadProcedureStateMap.ProcedureState = new FoupProcedureIdle();
                    }

                    SelectedProcedureStateMaps = UnloadProcedureStateMaps;
                }

                SelectedProcedureStateMapNode = (SelectedProcedureStateMaps as FoupProcedureStateMaps).First;
                if (SelectedProcedureStateMapNode != null)
                {
                    retval = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retval;
        }
        public EventCodeEnum SetPrevSelectedProcedureStateMapNode()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (SelectedProcedureStateMapNode.Previous != null)
                {
                    SelectedProcedureStateMapNode = SelectedProcedureStateMapNode.Previous;
                    SelectedProcedureStateMapNode.Value.ProcedureState = new FoupProcedureIdle(); //idle state로 변경
                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    retval = EventCodeEnum.FOUP_SEQUENCE_NULL;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public int SetSequenceTab(IFoupProcedure curprocedure)
        {
            int tabindex = -1;

            try
            {
                foreach (var obj in LoadProcedureStateMaps) //load tab enabled, unload tab disabled
                {
                    if (obj.Procedure.Equals(curprocedure))
                    {
                        return 0;
                    }
                }

                foreach (var obj in UnloadProcedureStateMaps) //unload tab enabled, load tab disabled
                {
                    if (obj.Procedure.Equals(curprocedure))
                    {
                        return 1;
                    }
                }


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return tabindex;
        }

        public EventCodeEnum SequencesRefresh()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            bool errorflag = false;
            try
            {
                SelectedProcedureStateMapNode = (SelectedProcedureStateMaps as FoupProcedureStateMaps).First;

                foreach (var procedure in SelectedProcedureStateMaps as FoupProcedureStateMaps)
                {
                    if (procedure.ProcedureStateEnum == FoupProcedureStateEnum.DONE)
                    {
                        procedure.ProcedureState = new FoupProcedureIdle();

                        if (!errorflag)
                        {
                            EventCodeEnum ioValid = procedure.Procedure.Behavior.CheckIOState();

                            if (ioValid == EventCodeEnum.NONE)
                            {
                                procedure.ProcedureState = new FoupProcedureDone();
                                LoggerManager.RecoveryLog($"Foup Procedure : {SelectedProcedureStateMapNode.Value.Procedure.Caption}, IO Validation : DONE");
                                SelectedProcedureStateMapNode = SelectedProcedureStateMapNode.Next;
                            }
                            else if (ioValid == EventCodeEnum.UNDEFINED)
                            {
                                //error
                                LoggerManager.RecoveryLog($"Foup Procedure : {SelectedProcedureStateMapNode.Value.Procedure.Caption}, IO Validation : IDLE");
                                procedure.ProcedureState = new FoupProcedureIdle();
                            }
                            else if (ioValid == EventCodeEnum.IO_PORT_ERROR)
                            {
                                LoggerManager.RecoveryLog($"Foup Procedure : {SelectedProcedureStateMapNode.Value.Procedure.Caption}, IO Validation : ERROR", true);
                                procedure.ProcedureState = new FoupProcedureBehaviorError();
                                errorflag = true;
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public FoupStateEnum FindFoupState()
        {
            FoupStateEnum state = FoupStateEnum.UNDEFIND;

            try
            {
                foreach (var load in LoadProcedureStateMaps)
                {
                    if (SelectedProcedureStateMapNode.Value.Procedure == load.Procedure)
                    {
                        return FoupStateEnum.LOAD;
                    }
                }

                foreach (var unload in UnloadProcedureStateMaps)
                {
                    if (SelectedProcedureStateMapNode.Value.Procedure == unload.Procedure)
                    {
                        return FoupStateEnum.UNLOAD;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return state;
        }
        public void SetDockingPlateProcedure(DockingPlateStateEnum dockingplatestate)
        {
            try
            {
                if (dockingplatestate == DockingPlateStateEnum.LOCK)
                {
                    (SelectedProcedureStateMaps as FoupProcedureStateMaps).First.Value.ProcedureState = new FoupProcedureDone();
                    if (SelectedProcedureStateMapNode.Value.Procedure == (SelectedProcedureStateMaps as FoupProcedureStateMaps).First.Value.Procedure)
                    {
                        SelectedProcedureStateMapNode = SelectedProcedureStateMapNode.Next;
                    }
                }
                else
                {
                    (SelectedProcedureStateMaps as FoupProcedureStateMaps).First.Value.ProcedureState = new FoupProcedureIdle();
                    SelectedProcedureStateMapNode = (SelectedProcedureStateMaps as FoupProcedureStateMaps).First;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetCoverDownProcedure(FoupCoverStateEnum coverstate)
        {
            try
            {
                if (coverstate == FoupCoverStateEnum.CLOSE)
                {
                    (SelectedProcedureStateMaps as FoupProcedureStateMaps).First.Value.ProcedureState = new FoupProcedureDone();
                    if (SelectedProcedureStateMapNode.Value.Procedure == (SelectedProcedureStateMaps as FoupProcedureStateMaps).First.Value.Procedure)
                    {
                        SelectedProcedureStateMapNode = SelectedProcedureStateMapNode.Next;
                    }
                }
                else
                {
                    (SelectedProcedureStateMaps as FoupProcedureStateMaps).First.Value.ProcedureState = new FoupProcedureIdle();
                    SelectedProcedureStateMapNode = (SelectedProcedureStateMaps as FoupProcedureStateMaps).First;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }        
    }

}
