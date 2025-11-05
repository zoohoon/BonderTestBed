using Autofac;
using DeviceUpDownControl;
using LoaderBase;
using LoaderBase.Communication;
using LoaderBase.LoaderLog;
using LoaderParameters;
using LoaderParameters.Data;
using LogModule;
using MetroDialogInterfaces;
using PMIModuleParameter;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Device;
using ProberInterfaces.Loader;
using RetestObject;
using SecsGemServiceInterface;
using SubstrateObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using XGEMWrapper;

namespace LoaderCore.DeviceManager
{
    public class GPDeviceManager : IDeviceManager, IFactoryModule, ILoaderFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //..Properties
        public bool Initialized { get; set; } = false;
        private Autofac.IContainer _Container { get; set; }
        public ILoaderModule Loader { get; set; }
        public string BasePath { get; set; }

        private ITrasnferObjectSet _TransferObjectInfos;
        public ITrasnferObjectSet TransferObjectInfos
        {
            get { return _TransferObjectInfos; }
            set
            {
                if (value != _TransferObjectInfos)
                {
                    _TransferObjectInfos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private TrasnferObjectSet _TransferObjectInfosConcrete;
        public TrasnferObjectSet TransferObjectInfosConcrete
        {
            get { return _TransferObjectInfosConcrete; }
            set
            {
                if (value != _TransferObjectInfosConcrete)
                {
                    _TransferObjectInfosConcrete = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private TrasnferObjectSet _TransferObjectInfos = new TrasnferObjectSet();
        //public TrasnferObjectSet TransferObjectInfos
        //{
        //    get { return _TransferObjectInfos; }
        //    set
        //    {
        //        if (value != _TransferObjectInfos)
        //        {
        //            _TransferObjectInfos = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private IParam _DeviceManagerParamerer_IParam;
        public IParam DeviceManagerParamerer_IParam
        {
            get { return _DeviceManagerParamerer_IParam; }
            set
            {
                if (value != _DeviceManagerParamerer_IParam)
                {
                    _DeviceManagerParamerer_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IParam _PolishWaferInfoParam_IParam;
        public IParam PolishWaferInfoParam_IParam
        {
            get { return _PolishWaferInfoParam_IParam; }
            set
            {
                if (value != _PolishWaferInfoParam_IParam)
                {
                    _PolishWaferInfoParam_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private DeviceManagerParameter _DeviceManagerParam = new DeviceManagerParameter();
        public DeviceManagerParameter DeviceManagerParam
        {
            get { return _DeviceManagerParam; }
            set
            {
                if (value != _DeviceManagerParam)
                {
                    _DeviceManagerParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PolishWaferInfoParameter _PolishWaferInfoParam;

        public PolishWaferInfoParameter PolishWaferInfoParam
        {
            get { return _PolishWaferInfoParam; }
            set 
            {
                if (value != _PolishWaferInfoParam)
                {
                    _PolishWaferInfoParam = value;
                    RaisePropertyChanged();
                }
            }
        }


        public string DetachDeviceFolderName { get; } = "CellDevices";

        //private ObservableCollection<PolishWaferInformation> _PolisWaferInfos = new ObservableCollection<PolishWaferInformation>();
        //public ObservableCollection<PolishWaferInformation> PolisWaferInfos
        //{
        //    get { return _PolisWaferInfos; }
        //    set
        //    {
        //        if (value != _PolisWaferInfos)
        //        {
        //            _PolisWaferInfos = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        public InitPriorityEnum InitPriority { get; set; }
        object device_lockobj = new object();


        #endregion

        #region //..Creator & Init
        public GPDeviceManager(ILoaderModule loader)
        {
            try
            {
                this.Loader = loader;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public GPDeviceManager()
        {

        }

        //public EventCodeEnum GetPolisWaferInfos()
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        // TODO: Test Code
        //        PolisWaferInfos.Add(new PolishWaferInformation("Undefined1"));
        //        PolisWaferInfos.Add(new PolishWaferInformation("Undefined2"));
        //        PolisWaferInfos.Add(new PolishWaferInformation("Undefined3"));

        //        retVal = EventCodeEnum.NONE;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    return retVal;
        //}

        public EventCodeEnum CreateTransferObject()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (TransferObjectInfosConcrete == null)
                {
                    TransferObjectInfosConcrete = new TrasnferObjectSet();
                }

                if (TransferObjectInfosConcrete != null)
                {
                    // WaferSupplyInfo.ModuleType
                    // WaferSupplyInfo.ID
                    // DeviceInfo

                    // Slot
                    var Slotobjects = DeviceManagerParam.DeviceMappingInfos.Where(x => x.WaferSupplyInfo.ModuleType == ModuleTypeEnum.SLOT);

                    int CassetteSlotNum = 25;

                    int foupcount = Slotobjects.Count() / CassetteSlotNum;

                    for (int i = 0; i < foupcount; i++)
                    {
                        // Input the Foup's index. Start Index is 1.

                        TransferObjectInfosConcrete.Foups.Add(new FoupObject(i + 1));
                    }

                    foreach (var item in Slotobjects.Select((value, i) => new { i, value }))
                    {
                        var obj = item.value;
                        var index = item.i;

                        // 0 ~ 24 => 1
                        // 25 ~ 49 => 2
                        // 50 ~ 74=> 3

                        var foupnumber = (index / CassetteSlotNum);

                        TransferObjectInfosConcrete.Foups[foupnumber].Slots.Add(new SlotObject(obj));
                    }

                    // FixedTray 
                    var fixedtrayobjects = DeviceManagerParam.DeviceMappingInfos.Where(x => x.WaferSupplyInfo.ModuleType == ModuleTypeEnum.FIXEDTRAY);

                    foreach (var obj in fixedtrayobjects)
                    {
                        TransferObjectInfosConcrete.FixedTrays.Add(new FixedTrayObject(obj));
                    }

                    // InspectionTray
                    var Inspectionrayobjects = DeviceManagerParam.DeviceMappingInfos.Where(x => x.WaferSupplyInfo.ModuleType == ModuleTypeEnum.INSPECTIONTRAY);

                    foreach (var obj in Inspectionrayobjects)
                    {
                        TransferObjectInfosConcrete.InspectionTrays.Add(new InspectionTrayObject(obj));
                    }

                    TransferObjectInfos = TransferObjectInfosConcrete;

                    retVal = EventCodeEnum.NONE;
                }

                //ITransferObjectInfos = TransferObjectInfos;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    retVal = LoadSysParameter();

                    retVal = CreateTransferObject();

                    //retVal = GetPolisWaferInfos();

                    _Container = this.GetLoaderContainer();

                    retVal = EventCodeEnum.NONE;

                    Initialized = true;
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
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion


        #region //..DevParameter
        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;               
                tmpParam = new DeviceManagerParameter();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                retVal = this.LoadParameter(ref tmpParam, typeof(DeviceManagerParameter));
                if (retVal == EventCodeEnum.NONE)
                {
                    DeviceManagerParam = tmpParam as DeviceManagerParameter;
                    DeviceManagerParamerer_IParam = DeviceManagerParam;
                }

                tmpParam = new PolishWaferInfoParameter();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                retVal = this.LoadParameter(ref tmpParam, typeof(PolishWaferInfoParameter));

                if (retVal == EventCodeEnum.NONE)
                {
                    PolishWaferInfoParam = tmpParam as PolishWaferInfoParameter;
                    PolishWaferInfoParam_IParam = PolishWaferInfoParam;
                }

                if(PolishWaferInfoParam != null)
                {
                    if (PolishWaferInfoParam.PolishWaferTypeParams.Count() == 0 && 
                        DeviceManagerParam.PolishWaferSourceParameters.Count() != 0) // 처음 폴리시 파라미터가 별도로 생성될때만 동작하도록 의도함.
                    {
                        foreach (var item in DeviceManagerParam.PolishWaferSourceParameters)
                        {
                            LoggerManager.Debug($"PolishWaferInfoParam.PolishWaferTypeParams.Add(DefineName: {item.DefineName.Value})");
                            PolishWaferInformation polishinfo = new PolishWaferInformation();

                            if (item.OCRConfigParam == null)
                            {
                                item.OCRConfigParam = new OCRDevParameter();
                                item.OCRConfigParam.SetHynixDefaultParam();
                            }

                            polishinfo.Copy(item);

                            PolishWaferInfoParam.PolishWaferTypeParams.Add(polishinfo);
                        }

                        this.SaveParameter(PolishWaferInfoParam);
                    }
                }

                // Synchronize using LoaderSystem.Json

                if (_Container == null)
                {
                    _Container = this.GetLoaderContainer();
                }

                if (Loader == null)
                {
                    Loader = _Container?.Resolve<ILoaderModule>();
                }

                if (Loader != null)
                {

                    int CassetteModuleCount = Loader.SystemParameter.CassetteModules.Count;
                    int SlotModuleCount = 0;
                    int FixedTrayCount = Loader.SystemParameter.FixedTrayModules.Count;
                    int InspectionTrayCount = Loader.SystemParameter.InspectionTrayModules.Count;

                    foreach (var Cassette in Loader.SystemParameter.CassetteModules)
                    {
                        SlotModuleCount += Cassette.SlotModules.Count;
                    }

                    var SlotModules = DeviceManagerParam.DeviceMappingInfos.Where(x => x.WaferSupplyInfo.ModuleType == ModuleTypeEnum.SLOT);
                    var FixedTrayModules = DeviceManagerParam.DeviceMappingInfos.Where(x => x.WaferSupplyInfo.ModuleType == ModuleTypeEnum.FIXEDTRAY);
                    var InspectionTrayModules = DeviceManagerParam.DeviceMappingInfos.Where(x => x.WaferSupplyInfo.ModuleType == ModuleTypeEnum.INSPECTIONTRAY);

                    if (SlotModules.Count() != SlotModuleCount)
                    {
                        var itemsToRemove = SlotModules.ToList();

                        foreach (var itemToRemove in itemsToRemove)
                        {
                            DeviceManagerParam.DeviceMappingInfos.Remove(itemToRemove);
                        }

                        //SLOT
                        for (int i = 1; i <= SlotModuleCount; i++)
                        {
                            WaferSupplyMappingInfo mappingInfo = new WaferSupplyMappingInfo();
                            mappingInfo.DeviceInfo = new TransferObject();
                            mappingInfo.WaferSupplyInfo = new WaferSupplyModuleInfo(ModuleTypeEnum.SLOT, i);
                            DeviceManagerParam.DeviceMappingInfos.Add(mappingInfo);
                        }
                    }

                    if (FixedTrayModules.Count() != FixedTrayCount)
                    {
                        var itemsToRemove = FixedTrayModules.ToList();

                        foreach (var itemToRemove in itemsToRemove)
                        {
                            DeviceManagerParam.DeviceMappingInfos.Remove(itemToRemove);
                        }

                        for (int i = 1; i <= FixedTrayCount; i++)
                        {
                            WaferSupplyMappingInfo mappingInfo = new WaferSupplyMappingInfo();
                            mappingInfo.DeviceInfo = new TransferObject();
                            mappingInfo.WaferSupplyInfo = new WaferSupplyModuleInfo(ModuleTypeEnum.FIXEDTRAY, i);
                            DeviceManagerParam.DeviceMappingInfos.Add(mappingInfo);
                        }
                    }

                    if (InspectionTrayModules.Count() != InspectionTrayCount)
                    {
                        var itemsToRemove = InspectionTrayModules.ToList();

                        foreach (var itemToRemove in itemsToRemove)
                        {
                            DeviceManagerParam.DeviceMappingInfos.Remove(itemToRemove);
                        }

                        for (int i = 1; i <= InspectionTrayCount; i++)
                        {
                            WaferSupplyMappingInfo mappingInfo = new WaferSupplyMappingInfo();
                            mappingInfo.DeviceInfo = new TransferObject();
                            mappingInfo.WaferSupplyInfo = new WaferSupplyModuleInfo(ModuleTypeEnum.INSPECTIONTRAY, i);
                            DeviceManagerParam.DeviceMappingInfos.Add(mappingInfo);
                        }
                    }

                    SyncOCRConfigurationsFromPolishWaferSources();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }

        public void SyncOCRConfigurationsFromPolishWaferSources()
        {
            try
            {
                AsyncObservableCollection<IPolishWaferSourceInformation> PolishWaferTypeParams = PolishWaferInfoParam.PolishWaferTypeParams;

                if (PolishWaferTypeParams != null && PolishWaferTypeParams.Count >= 0)
                {
                    foreach (var item in DeviceManagerParam.DeviceMappingInfos)
                    {
                        if ((item.WaferSupplyInfo.ModuleType == ModuleTypeEnum.FIXEDTRAY || item.WaferSupplyInfo.ModuleType == ModuleTypeEnum.INSPECTIONTRAY) &&
                            item.DeviceInfo.PolishWaferInfo != null)
                        {
                            var pwsource = PolishWaferTypeParams.FirstOrDefault(pw => pw.DefineName.Value == item.DeviceInfo.PolishWaferInfo.DefineName.Value);
                            if (pwsource != null && pwsource.OCRConfigParam != null)
                            {
                                if (item.DeviceInfo.PolishWaferInfo.OCRConfigParam == null)
                                {
                                    item.DeviceInfo.PolishWaferInfo.OCRConfigParam = new OCRDevParameter();
                                }

                                item.DeviceInfo.PolishWaferInfo.OCRConfigParam.Copy(pwsource.OCRConfigParam);
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                DeviceManagerParam.PolishWaferSourceParameters.Clear();

                foreach (var item in PolishWaferInfoParam.PolishWaferTypeParams)
                {
                    LoggerManager.Debug($"DeviceManagerParam.PolishWaferSourceParameters.Add(DefineName: {item.DefineName.Value})");
                    PolishWaferInformation polishinfo = new PolishWaferInformation();

                    // 데이터가 없는 경우, Default 값을 넣어줌
                    if (item.OCRConfigParam == null)
                    {
                        item.OCRConfigParam = new OCRDevParameter();
                        item.OCRConfigParam.SetHynixDefaultParam();
                    }

                    polishinfo.Copy(item);

                    DeviceManagerParam.PolishWaferSourceParameters.Add(polishinfo);
                }

                // Module별로 가지고 있는 PolishWaferInfo Config 데이터도 같이 동기를 맞춘다.
                SyncOCRConfigurationsFromPolishWaferSources();

                retVal = this.SaveParameter(PolishWaferInfoParam);
                retVal = this.SaveParameter(DeviceManagerParam);

                // (1) Broadcast

                Loader.BroadcastLoaderInfo();

                // (2) Recovery 

                foreach (var item in DeviceManagerParam.DeviceMappingInfos)
                {
                    IAttachedModule module = this.Loader.ModuleManager.FindModule(item.WaferSupplyInfo.ID);

                    if (module != null)
                    {
                        if ((module.ModuleType == ModuleTypeEnum.FIXEDTRAY) || (module.ModuleType == ModuleTypeEnum.INSPECTIONTRAY))
                        {
                            IWaferOwnable ownable = null;

                            ownable = module as IWaferOwnable;

                            ownable.RecoveryWaferStatus();
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"DeviceManager, FindModule is null value. Please check the parameters. ID : {item.WaferSupplyInfo.ID}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public void InitBasePath()
        {
            try
            {
                BasePath = this.FileManager().GetRootParamPath();

                //string[] CommandLineArgs = Environment.GetCommandLineArgs();

                //foreach (var v in CommandLineArgs)
                //{
                //    if (v.ToLower().Contains("[path]"))
                //    {
                //        string[] splitString = v.Split(new string[] { "[path]", "[Path]", "[PATH]" }, StringSplitOptions.RemoveEmptyEntries);

                //        if (0 < splitString.Length)
                //        {
                //            if (!string.IsNullOrEmpty(splitString[0]))
                //                BasePath = splitString[0];
                //        }
                //    }
                //}

            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public string GetLoaderDevicePath()
        {
            string str = null;
            try
            {
                if (BasePath == null) InitBasePath();
                return Path.Combine(BasePath, "Parameters", "Devices");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return str;

        }

        //public string GetLoaderParamPath()
        //{
        //    if (BasePath == null) InitBasePath();
        //    return Path.Combine(BasePath, "Parameters", "Loader");
        //}

        #endregion


        #region //..Method
        //public TransferObjectDeviceInfo GetDeviceInfo(IWaferSupplyModule waferSupplyModule)
        //{
        //    TransferObjectDeviceInfo deviceInfo = null;
        //    WaferSupplyMappingInfo mappingInfo = null;
        //    try
        //    {
        //        if (waferSupplyModule != null)
        //        {
        //            mappingInfo = DeviceManagerParam.DeviceMappingInfos.FirstOrDefault(i => i.WaferSupplyInfo.ModuleType == waferSupplyModule.ModuleType &&
        //            i.WaferSupplyInfo.ID == waferSupplyModule.ID);

        //            if (mappingInfo != null)
        //            {
        //                deviceInfo = mappingInfo.DeviceInfo;
        //            }
        //        }

        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return deviceInfo;
        //}

        public AsyncObservableCollection<IPolishWaferSourceInformation> GetPolishWaferSources()
        {
            AsyncObservableCollection<IPolishWaferSourceInformation> retval = null;

            try
            {
                retval = PolishWaferInfoParam.PolishWaferTypeParams;
                //retval = DeviceManagerParam.PolishWaferSourceParameters;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public TransferObject GetDeviceInfo(IWaferSupplyModule waferSupplyModule)
        {
            TransferObject deviceInfo = null;
            WaferSupplyMappingInfo mappingInfo = null;

            try
            {
                if (waferSupplyModule != null)
                {
                    mappingInfo = DeviceManagerParam.DeviceMappingInfos.FirstOrDefault(i => i.WaferSupplyInfo.ModuleType == waferSupplyModule.ModuleType && i.WaferSupplyInfo.ID == waferSupplyModule.ID);

                    if (mappingInfo != null)
                    {
                        deviceInfo = mappingInfo.DeviceInfo;
                        if (deviceInfo.WaferType.Value == EnumWaferType.POLISH)
                        {
                            deviceInfo.Type.Value = SubstrateTypeEnum.Wafer;
                            deviceInfo.WaferSubstrateType.Value = WaferSubstrateTypeEnum.Normal;

                            var polishwaferinfo = GetPolishWaferSources();
                            if (polishwaferinfo != null && polishwaferinfo.Count > 0) //이상함.
                            {
                                if (deviceInfo.PolishWaferInfo != null)
                                {
                                    // PolishWaferInfomation 내부에 OCRConfigParam 프로퍼티가 나중에 추가되어서 최신으로 넣어주기 위한 코드 
                                    deviceInfo.PolishWaferInfo = (polishwaferinfo.Where(pw => pw.DefineName.Value == deviceInfo.PolishWaferInfo.DefineName.Value).FirstOrDefault()) as PolishWaferInformation ?? null;

                                    var waferInfoToCopy = polishwaferinfo
                                        .Where(pw => pw.DefineName.Value == deviceInfo.PolishWaferInfo.DefineName.Value)
                                        .FirstOrDefault() as PolishWaferInformation;

                                    if (waferInfoToCopy != null)
                                    {
                                        deviceInfo.PolishWaferInfo.Copy(waferInfoToCopy);
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return deviceInfo;
        }

        public List<ModuleID> GetPolishSourceModules()
        {
            List<ModuleID> modules = new List<ModuleID>();
            try
            {
                //var allpwtypes = GetPolishWaferSources();

                var mappoings = DeviceManagerParam.DeviceMappingInfos.Where(w => w.DeviceInfo.PolishWaferInfo != null).ToList();
                foreach (var item in mappoings)
                {
                    modules.Add(item.WaferSupplyInfo.ID);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return modules;

        }

        public IPolishWaferSourceInformation GetPolishWaferInformation(ModuleID moduleID)
        {
            IPolishWaferSourceInformation retVal = null;
            try
            {
                var allpwtypes = GetPolishWaferSources();
                var mappoing = DeviceManagerParam.DeviceMappingInfos.Where(w => w.WaferSupplyInfo.ID == moduleID).FirstOrDefault();
                if(mappoing != null)
                {
                    retVal = allpwtypes.Where(w => w.DefineName.Value == mappoing.DeviceInfo?.PolishWaferInfo?.DefineName.Value).FirstOrDefault() as IPolishWaferSourceInformation;
                }
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public byte[] Compress(string devicename, int stageindex = -1)
        {
            byte[] arr = null;

            try
            {
                string fullpath = GetLoaderDevicePath() + "\\" + devicename;

                if (GetDetachDeviceFlag() & stageindex != -1)
                {
                    fullpath = GetLoaderDevicePath() + $"\\{DetachDeviceFolderName}"
                   + "\\c" + $"{stageindex.ToString().PadLeft(2, '0')}";
                }

                string zippath = fullpath + ".zip";

                DirectoryInfo directory = new DirectoryInfo(fullpath);

                if (!directory.Exists)
                    return null;

                string extractPath = directory.FullName;

                if (!extractPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                    extractPath += Path.DirectorySeparatorChar;

                if (!File.Exists(zippath))
                    ZipFile.CreateFromDirectory(fullpath, zippath);

                arr = File.ReadAllBytes(zippath);

                //File.WriteAllBytes(@"C:\ProberSystem\LoaderSystem\EMUL\Parameters\Devices\Test.zip", arr);

                File.Delete(zippath);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return arr;
        }

        public string GetFullPath(Object obj, int stagenum)
        {
            string fullpath = null;
            try
            {
                var commmanager = _Container.Resolve<ILoaderCommunicationManager>();
                string devname = commmanager.GetStageDeviceName(stagenum);
                var devpath = GetLoaderDevicePath() + "\\" + devname;
                if (obj is IParam)
                {
                    IParam tmpParam = obj as IParam;
                    tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                    fullpath = devpath + "\\" + tmpParam.FileName;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return fullpath;
        }

        public (OCRDevParameter param, EventCodeEnum retVal) GetOCRDevParameter(string devicename)
        {
            OCRDevParameter ocrParam = new OCRDevParameter();
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (string.IsNullOrEmpty(devicename))
                {
                    devicename = "DEFAULTDEVNAME";
                }

                var loaderbasepath = GetLoaderDevicePath();
                var loadpath = loaderbasepath + "\\" + devicename + "\\" + ocrParam.FilePath + "\\" + ocrParam.FileName;

                IParam ocrTempParam = new OCRDevParameter();
                ocrTempParam.Genealogy = this.GetType().Name + "." + ocrTempParam.GetType().Name + ".";
                retVal = this.LoadParameter(ref ocrTempParam, typeof(OCRDevParameter), null, loadpath);

                if (retVal == EventCodeEnum.NONE)
                {
                    ocrParam = ocrTempParam as OCRDevParameter;
                }
                else
                {
                    LoggerManager.Debug($"GetOCRDevParameter(): Load OCRDevParam Failed, devicename:{devicename}");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return (ocrParam, retVal);
        }

        public EventCodeEnum SetParameterForDevice(NeedChangeParameterInDevice data)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                lock(device_lockobj)
                {
                    ILoaderSupervisor loaderSupervisor = _Container.Resolve<ILoaderSupervisor>();

                    if (!(data.FoupNumber <= 0 | data.FoupNumber > loaderSupervisor.ActiveLotInfos.Count))
                    {
                        ActiveLotInfo activeLotInfo = loaderSupervisor.ActiveLotInfos[data.FoupNumber - 1];
                        foreach (var stageIdx in activeLotInfo.UsingStageIdxList)
                        {
                            if(activeLotInfo.CellDeviceInfoDic.ContainsKey(stageIdx))
                            {
                                string assignDevName = "";
                                activeLotInfo.CellDeviceInfoDic.TryGetValue(stageIdx, out assignDevName);
                                
                                if(assignDevName.Equals(data.DeviceName))
                                {
                                    if (activeLotInfo.CellSetParamOfDeviceDic.ContainsKey(stageIdx))
                                    {
                                        activeLotInfo.CellSetParamOfDeviceDic.Remove(stageIdx);
                                        LoggerManager.Debug($"GPDeviceManager - SetParameterForDevice() check CellSetParamOfDeviceDic data : already exist Stage#{stageIdx} data so delete.");
                                    }

                                    activeLotInfo.CellSetParamOfDeviceDic.Add(stageIdx, data);
                                    LoggerManager.Debug($"GPDeviceManager - SetParameterForDevice() add to CellSetParamOfDeviceDic : FoupNumber : {data.FoupNumber}, LotID : {data.LOTID}, Stage : {stageIdx}, Device : {data.DeviceName}");

                                    ILoaderCommunicationManager loaderCommunicationManager = _Container.Resolve<ILoaderCommunicationManager>();
                                    var stage = loaderCommunicationManager.GetProxy<IStageSupervisorProxy>(stageIdx);
                                    if(stage != null)
                                    {
                                        stage.SetNeedChangeParaemterInDeviceInfo(data);
                                    }
                                    LoggerManager.Debug($"SetParameterForDevice() - stage number {stageIdx} : ");
                                }
                            }
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
        public EventCodeEnum RecipeValidation(string Devicename, SubstrateSizeEnum SubstrateSize)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var loaderbasepath = GetLoaderDevicePath();
                string loadpath = string.Empty;

                var WaferDevObject = new WaferDevObject();
                loadpath = loaderbasepath + "\\" + Devicename + "\\" + WaferDevObject.FileName;

                IParam tmpParam = new WaferDevObject();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                retVal = this.LoadParameter(ref tmpParam, typeof(WaferDevObject), null, loadpath);
                if (retVal == EventCodeEnum.NONE)
                {
                    WaferDevObject = tmpParam as WaferDevObject;
                    EnumWaferSize size = EnumWaferSize.UNDEFINED;

                    if (SubstrateSize == SubstrateSizeEnum.INCH6)
                        size = EnumWaferSize.INCH6;
                    else if (SubstrateSize == SubstrateSizeEnum.INCH8)
                        size = EnumWaferSize.INCH8;
                    else if (SubstrateSize == SubstrateSizeEnum.INCH12)
                        size = EnumWaferSize.INCH12;
                    else
                        size = EnumWaferSize.UNDEFINED;

                    LoggerManager.Debug($"GPDeviceManager - RecipeValidation() Devicename's Wafer Szie: {WaferDevObject.PhysInfo.WaferSizeEnum},CST Wafer Szie: {size}");

                    if (WaferDevObject.PhysInfo.WaferSizeEnum == size)
                    {
                        retVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        retVal = EventCodeEnum.WAFER_SIZE_ERROR;
                        LoggerManager.Debug($"GPDeviceManager - RecipeValidation() Error occurred while loading WaferDevObject parameters. Err = {retVal}");
                    }
                }
                else
                {
                    retVal = EventCodeEnum.PARAM_ERROR;
                    LoggerManager.Debug($"GPDeviceManager - RecipeValidation() Error occurred while loading WaferDevObject parameters. Err = {retVal}");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }



        public EventCodeEnum SetRecipeToStage(DownloadStageRecipeActReqData data)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var commmanager = _Container.Resolve<ILoaderCommunicationManager>();

                List<string> recipes = new List<string>();
                ILoaderLogManagerModule loaderLogModule = _Container.Resolve<ILoaderLogManagerModule>();
                ILoaderLogSplitManager loaderLogSplitmanager = _Container.Resolve<ILoaderLogSplitManager>();
                string rootpath = loaderLogModule.LoaderLogParam.DeviceDownLoadPath.Value;
                DeviceExplorerViewModel NetworkDeviceExplorer = new DeviceExplorerViewModel("pack://application:,,,/ImageResourcePack;component/Images/FolderIcon.png");
                NetworkDeviceExplorer.ChangeDeviceCountPerPage(16);

                // <!-- LOT 와 연관된 Device Download 라면 ActiveLotInfo 에 데이터를 넣어준다 -->
                if (data.FoupNumber != 0) //Foup 정보가 있다는 것은 LOT(Foup)과 연관된 Device 라는 의미로 본다.
                {
                    ILoaderSupervisor loaderSupervisor = _Container.Resolve<ILoaderSupervisor>();

                    var activeLotInfo = loaderSupervisor.ActiveLotInfos[data.FoupNumber - 1];
                    if (activeLotInfo != null)
                    {
                        if (activeLotInfo.CellDeviceInfoDic == null)
                            activeLotInfo.CellDeviceInfoDic = new Dictionary<int, string>();
                        foreach (var rdic in data.RecipeDic)
                        {
                            if (activeLotInfo.CellDeviceInfoDic.ContainsKey(rdic.Key))
                            {
                                activeLotInfo.CellDeviceInfoDic.Remove(rdic.Key);
                                LoggerManager.Debug($"GPDeviceManager - SetRecipeToStage() check CellDeviceInfoDic data : already exist Stage#{rdic.Key} data so delete.");
                            }

                            activeLotInfo.CellDeviceInfoDic.Add(rdic.Key, rdic.Value);
                            LoggerManager.Debug($"GPDeviceManager - SetRecipeToStage() add to CellDeviceInfoDic : FoupNumber : {data.FoupNumber}, LotID : {data.LotID}, Stage : {rdic.Key}, Device : {rdic.Value}");
                        }
                    }
                }

                foreach (var rdic in data.RecipeDic)
                {
                    //같은 이름의 device 를 다운받은 적이 없다면
                    if (recipes.Find(recipe => recipe == rdic.Value) == null)
                    {
                        //if (useftp == true)
                        if (data.UseFTP)
                        {
                            retVal = loaderLogSplitmanager.ConnectCheck(rootpath, loaderLogModule.LoaderLogParam.UserName.Value,loaderLogModule.LoaderLogParam.Password.Value);

                            if (retVal == EventCodeEnum.NONE)
                            {
                                string downloadDevName = rdic.Value;
                                if ((downloadDevName != string.Empty) && downloadDevName != null)
                                {
                                    string destpath = GetLoaderDevicePath() + "\\" + downloadDevName;
                                    try
                                    {
                                        string downpath = null;
                                        if (loaderLogModule.LoaderLogParam.DeviceDownLoadPath.Value[loaderLogModule.LoaderLogParam.DeviceDownLoadPath.Value.Length - 1] == '/')
                                        {
                                            downpath = loaderLogModule.LoaderLogParam.DeviceDownLoadPath.Value + downloadDevName;
                                        }
                                        else
                                        {
                                            downpath = loaderLogModule.LoaderLogParam.DeviceDownLoadPath.Value + '/' + downloadDevName;
                                        }
                                        if ((retVal = loaderLogSplitmanager.LoaderDeviceDownloadFromServer(downpath, destpath, loaderLogModule.LoaderLogParam.UserName.Value,
                                        loaderLogModule.LoaderLogParam.Password.Value)) != EventCodeEnum.NONE)
                                        {
                                            // FTP Server 로 부터 recipe 를 다운받지 못했을 경우
                                            LoggerManager.Debug($"Download recipe form server failed.");
                                            break;
                                        }
                                        else
                                        {
                                            // OriginalDeviceZipName
                                            // FTP Server 로 부터 recipe 를 다운 받은 경우 ( LOT 와 연관된 Device 인 경우 - Foup number, LOT ID  데이터가 있으면) Device 를 알집파일로 보관한다.
                                            if (data.FoupNumber != 0 && data.LotID != null)
                                            {
                                                ILoaderSupervisor loaderSupervisor = _Container.Resolve<ILoaderSupervisor>();
                                                var activeLotInfo = loaderSupervisor.ActiveLotInfos[data.FoupNumber - 1];

                                                string zippath = destpath + $"_{data.LotID}_Foup{data.FoupNumber}_{activeLotInfo.CST_HashCode}.zip";
                                                if (File.Exists(zippath))
                                                    File.Delete(zippath);

                                                ZipFile.CreateFromDirectory(destpath, zippath);

                                                activeLotInfo.OriginalDeviceZipName.Add(downloadDevName, zippath);
                                                LoggerManager.Debug($"GPDeviceManager - SetRecipeToStage() add to OriginalDeviceZipName : FoupNumber : {data.FoupNumber}, LotID : {data.LotID}, devZipPath : {zippath}");
                                            }
                                        }
                                    }
                                    catch (System.IO.FileNotFoundException e)
                                    {
                                        LoggerManager.Exception(e);
                                    }
                                    catch (Exception err)
                                    {
                                        LoggerManager.Exception(err);
                                    }
                                }
                                recipes.Add(rdic.Value);
                            }
                            else
                            {
                                //FTP connect error.
                                recipes.Add(rdic.Value); 
                                if (retVal == EventCodeEnum.LOGUPLOAD_LOGIN_OR_PASSWORD_INCORRECT)
                                {
                                    LoggerManager.Debug("Login or password incorrect! error code : LOGUPLOAD_LOGIN_OR_PASSWORD_INCORRECT");
                                    return retVal = EventCodeEnum.LOGUPLOAD_LOGIN_OR_PASSWORD_INCORRECT;
                                }
                                else
                                {
                                    LoggerManager.Debug("Could not connect to server. error code : LOGUPLOAD_CONNECT_FAIL");
                                    return retVal = EventCodeEnum.LOGUPLOAD_CONNECT_FAIL;
                                }

                            }
                        }
                        else
                        {
                            retVal = EventCodeEnum.NONE;
                        }
                    }
                }

                if (retVal == EventCodeEnum.NONE) // FTP 연결 & 다운로드 성공시에만 동작.
                {
                    //ILoaderSupervisor loaderSupervisor = _Container.Resolve<ILoaderSupervisor>();

                    foreach (var dic in data.RecipeDic)
                    {
                        if(data.FoupNumber > 0)
                        {

                            this.GEMModule().GetPIVContainer().FoupNumber.Value = data.FoupNumber;
                            this.GEMModule().GetPIVContainer().SetLotID(data.LotID);
                            //v22_merge// Gem 확인 필요
                            this.GEMModule().GetPIVContainer().SetLoaderLotIds(data.FoupNumber, data.LotID);

                            ILoaderSupervisor loaderSupervisor = _Container.Resolve<ILoaderSupervisor>();
                            var activeLotInfo = loaderSupervisor?.ActiveLotInfos[data.FoupNumber - 1];
                            string cstHashCode = "";
                            if (activeLotInfo != null)
                            {
                                cstHashCode = activeLotInfo.CST_HashCode;
                            }

                            retVal = SetDevice(dic.Key, dic.Value, data.LotID, cstHashCode, true, data.FoupNumber, false, false);

                        }
                        else
                        {
                            retVal = SetDevice(dic.Key, dic.Value, "", "", true, data.FoupNumber, false, false);
                        }

                        LoggerManager.Debug($"[GPDeviceManager] SetRecipeToStage - state number : {dic.Key}, recipe id : {dic.Value}, lot id : {data.LotID}, foup number {data.FoupNumber}, SetDevice() result = {retVal}");
                    }


                    var loaderbasepath = GetLoaderDevicePath();
                    string loadpath = string.Empty;

                    //카세트 할당
                    if (data.FoupNumber > 0)
                    {
                        WaferObject wafer = new WaferObject();
                        wafer.WaferDevObject = new WaferDevObject();
                        loadpath = loaderbasepath + "\\" + data.RecipeDic.Values.ElementAt(0) + "\\" + wafer.WaferDevObject.FileName;

                        IParam tmpParam = new WaferDevObject();
                        tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                        retVal = this.LoadParameter(ref tmpParam, typeof(WaferDevObject), null, loadpath);

                        if (retVal == EventCodeEnum.NONE)
                        {
                            wafer.WaferDevObject = tmpParam as WaferDevObject;
                        }

                        //var ocrPath = GetLoaderDevicePath();
                        OCRDevParameter ocrParam = new OCRDevParameter();
                        var getparam = GetOCRDevParameter(data.RecipeDic.Values.ElementAt(0));
                        if (getparam.retVal == EventCodeEnum.NONE)
                        {
                            ocrParam = getparam.param as OCRDevParameter;
                        }


                        WaferNotchTypeEnum notchType = (WaferNotchTypeEnum)Enum.Parse(typeof(WaferNotchTypeEnum), wafer.GetPhysInfo().NotchType.Value.ToString(), true);

                        //여기서 참조하고 있던 DeviceName은 참조하고 있지 않은 값이었음.WaferMapFile의 DeviceName
                        //this.Loader.ModuleManager.SetCstDevice(data.FoupNumber, wafer.GetSubsInfo().DeviceName.Value, wafer.GetPhysInfo().NotchAngle.Value, wafer.GetPhysInfo().UnLoadingAngle.Value, (SubstrateSizeEnum)wafer.GetPhysInfo().WaferSizeEnum, notchType, ocrParam);                        
                        this.Loader.ModuleManager.SetCstDevice(data.FoupNumber, data.RecipeDic.Values.ElementAt(0), wafer.GetPhysInfo().NotchAngle.Value, wafer.GetPhysInfo().UnLoadingAngle.Value, (SubstrateSizeEnum)wafer.GetPhysInfo().WaferSizeEnum, notchType, ocrParam);

                        PMIModuleDevParam pmiDevice = new PMIModuleDevParam();

                        loadpath = loaderbasepath + "\\" + data.RecipeDic.Values.ElementAt(0) + "\\" + pmiDevice.FilePath + "\\" + pmiDevice.FileName;
                        retVal = this.LoadParameter(ref tmpParam, typeof(PMIModuleDevParam), null, loadpath);

                        if (retVal == EventCodeEnum.NONE)
                        {
                            pmiDevice = tmpParam as PMIModuleDevParam;

                            //if (data.FoupNumber <= 0)
                            //{
                            //    data.FoupNumber = 1;
                            //}

                            if (pmiDevice != null && pmiDevice.TriggerComponent != null)
                            {
                                this.Loader.LoaderMaster.ActiveLotInfos[data.FoupNumber - 1].DoPMICount = pmiDevice.TriggerComponent.TotalNumberOfWafersToPerform.Value;
                                this.Loader.LoaderMaster.ActiveLotInfos[data.FoupNumber - 1].PMIEveryInterval = pmiDevice.TriggerComponent.EveryWaferInterval.Value;
                            }
                        }

                        // Retest

                        RetestDeviceParam retestDevice = new RetestDeviceParam();

                        loadpath = loaderbasepath + "\\" + data.RecipeDic.Values.ElementAt(0) + "\\" + retestDevice.FilePath + "\\" + retestDevice.FileName;
                        retVal = this.LoadParameter(ref tmpParam, typeof(RetestDeviceParam), null, loadpath);

                        if (retVal == EventCodeEnum.NONE)
                        {
                            retestDevice = tmpParam as RetestDeviceParam;

                            if (data.FoupNumber != 0)
                            {
                                ILoaderSupervisor loaderSupervisor = _Container.Resolve<ILoaderSupervisor>();

                                var activeLotInfo = loaderSupervisor.ActiveLotInfos[data.FoupNumber - 1];
                                var currentfoup = Loader.Foups.FirstOrDefault(x => x.Index == data.FoupNumber);

                                if (activeLotInfo != null && currentfoup != null)
                                {
                                    // CassetteModule에 LotMode 할당
                                    var cassette = Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, data.FoupNumber);

                                    if (cassette != null)
                                    {
                                        if (retestDevice.ForcedLotMode.Value == ForcedLotModeEnum.ForcedCP1)
                                        {
                                            cassette.LotMode = LotModeEnum.CP1;
                                        }
                                        else if (retestDevice.ForcedLotMode.Value == ForcedLotModeEnum.ForcedMPP)
                                        {
                                            cassette.LotMode = LotModeEnum.MPP;
                                        }
                                    }
                                    else
                                    {
                                        LoggerManager.Debug($"[{this.GetType().Name}], SetRecipeToStage() : cassette is null. FoupNumber = {data.FoupNumber}");
                                    }

                                    // 할당 된 Stage 중
                                    foreach (var idx in activeLotInfo.AssignedUsingStageIdxList)
                                    {
                                        var cell = commmanager.Cells.FirstOrDefault(x => x.Index == idx);
                                        var setting = currentfoup.LotSettings.FirstOrDefault(x => x.Index == idx);

                                        if (cell != null && setting != null)
                                        {
                                            if (retestDevice.ForcedLotMode.Value == ForcedLotModeEnum.ForcedCP1)
                                            {
                                                setting.LotMode = LotModeEnum.CP1;
                                            }
                                            else if (retestDevice.ForcedLotMode.Value == ForcedLotModeEnum.ForcedMPP)
                                            {
                                                setting.LotMode = LotModeEnum.MPP;
                                            }

                                            if (setting.LotMode != LotModeEnum.UNDEFINED)
                                            {
                                                var StageIndex = cell.Index;
                                                var StageState = cell.StageState;

                                                if (StageState == ModuleStateEnum.IDLE)
                                                {
                                                    IStageSupervisorProxy proxy = commmanager.GetProxy<IStageSupervisorProxy>(idx);

                                                    if (proxy != null)
                                                    {
                                                        // ForcedLotMode의 값이 UNDEFINED인 경우, Cell에게 값을 할당해줘야 하는 로직이 필요하다.
                                                        proxy.ChangeLotMode(setting.LotMode);

                                                        LoggerManager.Debug($"[{this.GetType().Name}], SetRecipeToStage() : Stage Index = {StageIndex}, State = {StageState}, CP Mode is assigned ({setting.LotMode}).");
                                                    }
                                                    else
                                                    {
                                                        LoggerManager.Debug($"[{this.GetType().Name}], SetRecipeToStage() : Stage Index = {StageIndex}, State = {StageState}, CP Mode cannot be assigned. (proxy is null)");
                                                    }
                                                }
                                                else
                                                {

                                                    LoggerManager.Debug($"[{this.GetType().Name}], SetRecipeToStage() : Stage Index = {StageIndex}, State = {StageState}, CP Mode cannot be assigned.");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                LoggerManager.Error($"[{this.GetType().Name}], SetRecipeToStage() : data.FoupNumber is wrong.");
                            }
                        }
                    }

                }
                else
                {
                    //TODO: serialize된 device값을 null로 보내서 셀에서 Fail 로 만들기 위함.
                    foreach (var dic in data.RecipeDic)
                    {
                        //ILoaderSupervisor loaderSupervisor = _Container.Resolve<ILoaderSupervisor>();

                        this.GEMModule().GetPIVContainer().FoupNumber.Value = data.FoupNumber;
                        this.GEMModule().GetPIVContainer().SetLotID(data.LotID);
                        this.GEMModule().GetPIVContainer().SetLoaderLotIds(data.FoupNumber, data.LotID);

                        ILoaderSupervisor loaderSupervisor = _Container.Resolve<ILoaderSupervisor>();
                        string cstHashCode = "";
                        if (data.FoupNumber > 0)
                        {
                            var activeLotInfo = loaderSupervisor?.ActiveLotInfos[data.FoupNumber - 1];                           

                            if (activeLotInfo != null)
                            {
                                cstHashCode = activeLotInfo.CST_HashCode;
                            }
                        }

                        

                        SetDevice(dic.Key, null, data.LotID, cstHashCode, true, data.FoupNumber, false, false);
                        LoggerManager.Debug($"[GPDeviceManager] SetRecipeToStage - state number : {dic.Key}, recipe id : {dic.Value}, lot id : {data.LotID}, foup number {data.FoupNumber} Device Null");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void ClearDeviceZipFiles()
        {
            try
            {
                // destpath 경로에서 zip 파일들 삭제 
                string destpath = GetLoaderDevicePath() + "\\";
                foreach (string files in Directory.GetFiles(destpath))
                {
                    var fileAttributes = File.GetAttributes(files);
                    if (fileAttributes == FileAttributes.Compressed)
                    {
                        File.Delete(files);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum SetDevice(int stageindex, string devicename, string lotid, string lotCstHashCode, bool loaddev = true, int foupnumber = -1, bool showprogress = true, bool waitload = true)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                var commmanager = _Container.Resolve<ILoaderCommunicationManager>();

                if (devicename != null)
                {
                    var device = Compress(devicename, stageindex);

                    var stage = commmanager.GetProxy<IStageSupervisorProxy>(stageindex);

                    if (stage != null)
                    {
                        stage.SetDevice(device, devicename, lotid, lotCstHashCode, loaddev, foupnumber, showprogress);

                        retVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        LoggerManager.Error("No Connection #Stage ");
                    }
                }
                else
                {
                    var stage = commmanager.GetProxy<IStageSupervisorProxy>(stageindex);
                    
                    if (stage != null)
                    {
                        stage.SetDevice(null, devicename, lotid, lotCstHashCode, loaddev, foupnumber, showprogress);

                        retVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        LoggerManager.Error("No Connection #Stage ");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public void SetPMIDevice(int foupnumber, string devicename)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                var loaderbasepath = GetLoaderDevicePath();
                string loadpath = string.Empty;
                IParam tmpParam = null;
                PMIModuleDevParam pmiDevice = new PMIModuleDevParam();

                loadpath = loaderbasepath + "\\" + devicename + "\\" + pmiDevice.FilePath + "\\" + pmiDevice.FileName;
                retVal = this.LoadParameter(ref tmpParam, typeof(PMIModuleDevParam), null, loadpath);

                if (retVal == EventCodeEnum.NONE)
                {
                    pmiDevice = tmpParam as PMIModuleDevParam;

                    if (foupnumber <= 0)
                    {
                        foupnumber = 1;
                    }

                    if (pmiDevice != null && pmiDevice.TriggerComponent != null)
                    {
                        this.Loader.LoaderMaster.ActiveLotInfos[foupnumber - 1].DoPMICount = pmiDevice.TriggerComponent.TotalNumberOfWafersToPerform.Value;
                        this.Loader.LoaderMaster.ActiveLotInfos[foupnumber - 1].PMIEveryInterval = pmiDevice.TriggerComponent.EveryWaferInterval.Value;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetPMIDeviceUsingCellParam(int foupnumber, int cellnumber)
        {

            try
            {
                var commmanager = _Container.Resolve<ILoaderCommunicationManager>();

                bool? isEnable = commmanager.GetProxy<IPMIModuleProxy>(cellnumber)?.GetPMIEnableParam();

                if (foupnumber <= 0)
                {
                    foupnumber = 1;
                }

                if (isEnable == true)
                {
                    var param = commmanager.GetProxy<IPMIModuleProxy>(cellnumber)?.GetTriggerComponent();

                    if (param != null)
                    {
                        this.Loader.LoaderMaster.ActiveLotInfos[foupnumber - 1].DoPMICount = param.TotalNumberOfWafersToPerform.Value;
                        this.Loader.LoaderMaster.ActiveLotInfos[foupnumber - 1].PMIEveryInterval = param.EveryWaferInterval.Value;
                    }
                    else
                    {
                        this.Loader.LoaderMaster.ActiveLotInfos[foupnumber - 1].DoPMICount = 0;
                        this.Loader.LoaderMaster.ActiveLotInfos[foupnumber - 1].PMIEveryInterval = 0;

                        LoggerManager.Error($"[{this.GetType().Name}], SetPMIDeviceUsingCellParam() : param is null.");
                    }
                }
                else
                {
                    this.Loader.LoaderMaster.ActiveLotInfos[foupnumber - 1].DoPMICount = 0;
                    this.Loader.LoaderMaster.ActiveLotInfos[foupnumber - 1].PMIEveryInterval = 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool GetDetachDeviceFlag()
        {
            try
            {
                //return DeviceManagerParam?.DetachDevice ?? false;
                return false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }
        public void SetDetachDeviceFlag(bool flag)
        {
            try
            {
                DeviceManagerParam.DetachDevice = flag;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum InitModule(Autofac.IContainer container)
        {
            return EventCodeEnum.NONE;
        }

        public void UpdateFixedTrayCanUseBuffer()
        {
            try
            {
                string fiexdTrayIndexString = "";

                foreach (var item in (TransferObjectInfos as TrasnferObjectSet).FixedTrays)
                {
                    IAttachedModule module = Loader.ModuleManager.FindModule(item.WaferSupplyInfo.ID);
                    if(module.ModuleType == ModuleTypeEnum.FIXEDTRAY)
                    {
                        item.CanUseBuffer = (module as IFixedTrayModule).CanUseBuffer;
                    }

                    if(item.CanUseBuffer == true && 
                        item.DeviceInfo.PolishWaferInfo?.DefineName.Value != null &&
                        item.DeviceInfo.PolishWaferInfo?.DefineName.Value != "")
                    {
                        fiexdTrayIndexString += item.Name + ", ";
                    }
                }
                if(fiexdTrayIndexString != "")
                {
                    int index = fiexdTrayIndexString.LastIndexOf(',');
                    string msg = fiexdTrayIndexString.Remove(index, 1);
                    this.MetroDialogManager().ShowMessageDialog("Warning", $"{msg} is for Buffer. \nPlease reassign the object another location.", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public object GetLockObject()
        {
            return device_lockobj;
        }
        //public DeviceManagerParameter GetDeviceManagerParameter()
        //{
        //    return DeviceManagerParam;
        //}

        #endregion

    }
}
