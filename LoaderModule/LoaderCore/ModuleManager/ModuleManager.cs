using System;
using System.Collections.Generic;
using System.Linq;

using Autofac;
using LoaderBase;
using ProberInterfaces;
using LoaderParameters;
using ProberErrorCode;
using LogModule;

namespace LoaderCore
{
    public class ModuleManager : IModuleManager
    {
        private List<IAttachedModule> _Modules = new List<IAttachedModule>();

        public InitPriorityEnum InitPriority => InitPriorityEnum.LEVEL2;

        public IContainer Container { get; set; }

        public ILoaderModule Loader => Container.Resolve<ILoaderModule>();

        public void DeInitModule()
        {
            try
            {
                foreach (var module in _Modules)
                {
                    module.DeInitModule();
                }
                _Modules.Clear();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum InitModule(IContainer container)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.Container = container;

                DeInitModule();

                retVal = CreateAttachedModules();

                if (retVal == EventCodeEnum.NONE)
                {
                    if (Loader.ServiceCallback != null)
                    {
                        Loader.UpdateDeviceParam(Loader.ServiceCallback.GetLoaderDeviceParameter());
                    }
                    else
                    {
                        Loader.LoadDevParameter();
                    }
                    retVal = UpdateDeviceParameters();
                }
                //if (retVal == EventCodeEnum.NONE)
                //    retVal = InitAttachModules();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        private EventCodeEnum CreateAttachedModules()
        {
            var systemParam = Loader.SystemParameter;

            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //remove all slots
                int moduleIndex;

                moduleIndex = 1;
                foreach (var moduleDef in systemParam.ScanCameraModules)
                {
                    ScanCameraModule module = new ScanCameraModule();
                    module.SetContainer(Container);
                    module.SetDefinition(moduleDef, moduleIndex++);

                    _Modules.Add(module);
                }


                moduleIndex = 1;
                foreach (var moduleDef in systemParam.ScanSensorModules)
                {
                    ScanSensorModule module = new ScanSensorModule();
                    module.SetContainer(Container);
                    module.SetDefinition(moduleDef, moduleIndex++);

                    _Modules.Add(module);
                }

                moduleIndex = 1;
                int globalSlotIdx = 1;
                Loader.PrevSlotInfo.Clear();
                foreach (var moduleDef in systemParam.CassetteModules)
                {
                    CassetteModule module = new CassetteModule();
                    module.SetContainer(Container);
                    module.SetDefinition(moduleDef, moduleIndex++);

                    int localSlotIdx = 1;
                    foreach (var slotDef in moduleDef.SlotModules)
                    {
                        SlotModule slot = new SlotModule();
                        slot.SetContainer(Container);
                        slot.SetDefinition(slotDef, globalSlotIdx++);
                        slot.SetCassette(module, localSlotIdx++);
                        _Modules.Add(slot);

                        SlotModule prevSlot = new SlotModule();
                        prevSlot.SetContainer(Container);
                        prevSlot.SetDefinition(slotDef, globalSlotIdx);
                        prevSlot.SetCassette(module, localSlotIdx);


                        Loader.PrevSlotInfo.Add(prevSlot);

                    }

                    _Modules.Add(module);
                }

                moduleIndex = 1;
                foreach (var moduleDef in systemParam.ARMModules)
                {
                    ARMModule module = new ARMModule();
                    module.SetContainer(Container);
                    module.SetDefinition(moduleDef, moduleIndex++);

                    _Modules.Add(module);
                }


                moduleIndex = 1;
                foreach (var moduleDef in systemParam.CardARMModules)
                {
                    CardARMModule module = new CardARMModule();
                    module.SetContainer(Container);
                    module.SetDefinition(moduleDef, moduleIndex++);

                    _Modules.Add(module);
                }

                moduleIndex = 1;
                foreach (var moduleDef in systemParam.PreAlignModules)
                {
                    PreAlignModule module = new PreAlignModule();
                    module.SetContainer(Container);
                    module.SetDefinition(moduleDef, moduleIndex++);

                    _Modules.Add(module);
                }

                moduleIndex = 1;
                foreach (var moduleDef in systemParam.CognexOCRModules)
                {
                    CognexOCRModule module = new CognexOCRModule();
                    module.SetContainer(Container);
                    module.SetDefinition(moduleDef, moduleIndex++);

                    _Modules.Add(module);
                }

                moduleIndex = 1;
                foreach (var moduleDef in systemParam.SemicsOCRModules)
                {
                    SemicsOCRModule module = new SemicsOCRModule();
                    module.SetContainer(Container);
                    module.SetDefinition(moduleDef, moduleIndex++);

                    _Modules.Add(module);
                }

                moduleIndex = 1;
                foreach (var moduleDef in systemParam.ChuckModules)
                {
                    //=> CHUCK
                    ChuckModule module = new ChuckModule();
                    module.SetContainer(Container);
                    module.SetDefinition(moduleDef, moduleIndex++);

                    _Modules.Add(module);
                }

                moduleIndex = 1;
                foreach (var moduleDef in systemParam.FixedTrayModules)
                {
                    FixedTrayModule module = new FixedTrayModule();
                    module.SetContainer(Container);
                    module.SetDefinition(moduleDef, moduleIndex++);

                    _Modules.Add(module);
                }

                moduleIndex = 1;
                foreach (var moduleDef in systemParam.InspectionTrayModules)
                {
                    InspectionTrayModule module = new InspectionTrayModule();
                    module.SetContainer(Container);
                    module.SetDefinition(moduleDef, moduleIndex++);

                    _Modules.Add(module);
                }
                #region // Buffer module
                moduleIndex = 1;
                foreach (var moduleDef in systemParam.BufferModules)
                {
                    BufferModule module = new BufferModule();
                    module.SetContainer(Container);
                    module.SetDefinition(moduleDef, moduleIndex++);

                    _Modules.Add(module);
                }
                #endregion
                #region // CardBufferTray
                moduleIndex = 1;
                foreach (var moduleDef in systemParam.CardBufferTrayModules)
                {
                    CardBufferTrayModule module = new CardBufferTrayModule();
                    module.SetContainer(Container);
                    module.SetDefinition(moduleDef, moduleIndex++);

                    _Modules.Add(module);
                }

                moduleIndex = 1;
                foreach (var moduleDef in systemParam.CardBufferModules)
                {
                    CardBufferModule module = new CardBufferModule();
                    module.SetContainer(Container);
                    module.SetDefinition(moduleDef, moduleIndex++);

                    _Modules.Add(module);
                }
                #endregion
                #region // CC
                moduleIndex = 1;
                foreach (var moduleDef in systemParam.CCModules)
                {
                    CCModule module = new CCModule();
                    module.SetContainer(Container);
                    module.SetDefinition(moduleDef, moduleIndex++);

                    _Modules.Add(module);
                }
                #endregion

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum InitAttachModules(bool IsRefresh = false)
        {
            int errcount = 0;
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                foreach (var module in _Modules)
                {
                    if (!(IsRefresh && module is CassetteModule))
                    {
                        retVal = module.InitModule();

                        if (retVal != EventCodeEnum.NONE)
                        {
                            errcount++;
                            LoggerManager.Debug($"ModuleManager.InitAttachModules() Fail Moudule = {module}");
                        }
                    }
                }
                retVal = errcount > 0 ? EventCodeEnum.LOADER_SYSTEM_ERROR : EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum ResetWaferLocation()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //=> Set illegal scan state
                var cassetteModules = FindModules<ICassetteModule>();
                foreach (var cassetteModule in cassetteModules)
                {
                    if (cassetteModule.ScanState == CassetteScanStateEnum.READ)
                        cassetteModule.SetIllegalScanState();
                }

                //=> Set wafer location (with clear backup info) 
                var holderModules = FindModules<IWaferOwnable>();
                foreach (var holderModule in holderModules)
                {
                    if (holderModule is ISlotModule)
                        continue;

                    holderModule.Holder.SetUndefined();
                    holderModule.RecoveryWaferStatus();
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum UpdateDefinitionParameters()
        {
            var systemParam = Loader.SystemParameter;

            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            //remove all slots
            int moduleIndex;

            try
            {
                moduleIndex = 1;
                foreach (var moduleDef in systemParam.ScanCameraModules)
                {
                    var module = FindModule<IScanCameraModule>(moduleDef.GetModuleType(), moduleIndex);
                    module.SetDefinition(moduleDef, moduleIndex++);
                }

                moduleIndex = 1;
                foreach (var moduleDef in systemParam.ScanSensorModules)
                {
                    var module = FindModule<IScanSensorModule>(moduleDef.GetModuleType(), moduleIndex);
                    module.SetDefinition(moduleDef, moduleIndex++);
                }

                moduleIndex = 1;
                int globalSlotIdx = 1;
                foreach (var moduleDef in systemParam.CassetteModules)
                {
                    var module = FindModule<ICassetteModule>(moduleDef.GetModuleType(), moduleIndex);
                    module.SetDefinition(moduleDef, moduleIndex++);

                    foreach (var slotDef in moduleDef.SlotModules)
                    {
                        var slot = FindModule<ISlotModule>(slotDef.GetModuleType(), globalSlotIdx);
                        slot.SetDefinition(slotDef, globalSlotIdx++);
                    }
                }

                moduleIndex = 1;
                foreach (var moduleDef in systemParam.ARMModules)
                {
                    var module = FindModule<IARMModule>(moduleDef.GetModuleType(), moduleIndex);
                    module.SetDefinition(moduleDef, moduleIndex++);
                }

                moduleIndex = 1;
                foreach (var moduleDef in systemParam.PreAlignModules)
                {
                    var module = FindModule<IPreAlignModule>(moduleDef.GetModuleType(), moduleIndex);
                    module.SetDefinition(moduleDef, moduleIndex++);
                }

                moduleIndex = 1;
                foreach (var moduleDef in systemParam.CognexOCRModules)
                {
                    var module = FindModule<ICognexOCRModule>(moduleDef.GetModuleType(), moduleIndex);
                    module.SetDefinition(moduleDef, moduleIndex++);
                }

                moduleIndex = 1;
                foreach (var moduleDef in systemParam.SemicsOCRModules)
                {
                    var module = FindModule<ISemicsOCRModule>(moduleDef.GetModuleType(), moduleIndex);
                    module.SetDefinition(moduleDef, moduleIndex++);
                }

                moduleIndex = 1;
                foreach (var moduleDef in systemParam.ChuckModules)
                {
                    var module = FindModule<IChuckModule>(moduleDef.GetModuleType(), moduleIndex);
                    module.SetDefinition(moduleDef, moduleIndex++);
                }

                moduleIndex = 1;
                foreach (var moduleDef in systemParam.FixedTrayModules)
                {
                    var module = FindModule<IFixedTrayModule>(moduleDef.GetModuleType(), moduleIndex);
                    module.SetDefinition(moduleDef, moduleIndex++);
                }

                moduleIndex = 1;
                foreach (var moduleDef in systemParam.InspectionTrayModules)
                {
                    var module = FindModule<IInspectionTrayModule>(moduleDef.GetModuleType(), moduleIndex);
                    module.SetDefinition(moduleDef, moduleIndex++);
                }



                moduleIndex = 1;
                foreach (var moduleDef in systemParam.CardARMModules)
                {
                    var module = FindModule<ICardARMModule>(moduleDef.GetModuleType(), moduleIndex);
                    module.SetDefinition(moduleDef, moduleIndex++);
                }

                moduleIndex = 1;
                foreach (var moduleDef in systemParam.CardBufferModules)
                {
                    var module = FindModule<ICardBufferModule>(moduleDef.GetModuleType(), moduleIndex);
                    module.SetDefinition(moduleDef, moduleIndex++);
                }

                moduleIndex = 1;
                foreach (var moduleDef in systemParam.CardBufferTrayModules)
                {
                    var module = FindModule<ICardBufferTrayModule>(moduleDef.GetModuleType(), moduleIndex);
                    module.SetDefinition(moduleDef, moduleIndex++);
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum SetCstDevice(int cstIndex, string deviceName, double loadingAngle, double unloadingAngle, SubstrateSizeEnum size, WaferNotchTypeEnum notchType,OCRDevParameter ocrDev, EnumWaferType type=EnumWaferType.STANDARD)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                CassetteDevice cassetteDev = new CassetteDevice();
                cassetteDev.AllocateDeviceInfo.DeviceName.Value = deviceName;
                cassetteDev.AllocateDeviceInfo.NotchAngle.Value = loadingAngle;
                cassetteDev.AllocateDeviceInfo.SlotNotchAngle.Value = unloadingAngle;
                cassetteDev.LoadingNotchAngle.Value = loadingAngle;
                cassetteDev.AllocateDeviceInfo.WaferType.Value = type;
                cassetteDev.AllocateDeviceInfo.Type.Value = SubstrateTypeEnum.Wafer;
                cassetteDev.AllocateDeviceInfo.Size.Value = size;
                cassetteDev.AllocateDeviceInfo.OCRType.Value = ProberInterfaces.Enum.OCRTypeEnum.COGNEX;
                cassetteDev.AllocateDeviceInfo.OCRMode.Value = ProberInterfaces.Enum.OCRModeEnum.READ;
                cassetteDev.AllocateDeviceInfo.OCRAngle.Value = ocrDev.OCRAngle;
                cassetteDev.AllocateDeviceInfo.OCRDevParam.lotIntegrity.LotIntegrityEnable = ocrDev.lotIntegrity.LotIntegrityEnable;
                cassetteDev.AllocateDeviceInfo.OCRDevParam.lotIntegrity.LotnameDigit = ocrDev.lotIntegrity.LotnameDigit;
                cassetteDev.AllocateDeviceInfo.OCRDevParam.lotIntegrity.Lotnamelength = ocrDev.lotIntegrity.Lotnamelength;
                cassetteDev.AllocateDeviceInfo.OCRDevParam = ocrDev;
                cassetteDev.AllocateDeviceInfo.NotchType = notchType;
               var module = FindModule<ICassetteModule>(cassetteDev.GetModuleType(), cstIndex);

                module.SetDevice(cassetteDev);

                this.Loader.Foups[cstIndex - 1].NotchAngle = module.Device.AllocateDeviceInfo.NotchAngle.Value;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            retVal = EventCodeEnum.NONE;

            return retVal;
        }
        public EventCodeEnum UpdateDeviceParameters()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                var deviceParam = Loader.DeviceParameter;

                int moduleIndex;

                moduleIndex = 1;
                foreach (var dev in deviceParam.ScanCameraModules)
                {
                    var module = FindModule<IScanCameraModule>(dev.GetModuleType(), moduleIndex);
                    module.SetDevice(dev);

                    moduleIndex++;
                }

                moduleIndex = 1;
                foreach (var dev in deviceParam.ScanSensorModules)
                {
                    var module = FindModule<IScanSensorModule>(dev.GetModuleType(), moduleIndex);
                    if (module == null)
                        continue;
                    module.SetDevice(dev);

                    moduleIndex++;
                }

                //=> Cassette
                moduleIndex = 1;
                int globalSlotIdx = 1;
                foreach (var dev in deviceParam.CassetteModules)
                {
                    var module = FindModule<ICassetteModule>(dev.GetModuleType(), moduleIndex);
                    module.SetDevice(dev);

                    foreach (var slotDev in dev.SlotModules)
                    {
                        var slot = FindModule<ISlotModule>(slotDev.GetModuleType(), globalSlotIdx);
                        if (slot == null)
                            continue;

                        slot.SetDevice(slotDev);

                        globalSlotIdx++;
                    }

                    moduleIndex++;
                }

                moduleIndex = 1;
                foreach (var dev in deviceParam.ARMModules)
                {
                    var module = FindModule<IARMModule>(dev.GetModuleType(), moduleIndex);
                    if (module == null)
                        continue;
                    module.SetDevice(dev);

                    moduleIndex++;
                }

                moduleIndex = 1;
                foreach (var dev in deviceParam.PreAlignModules)
                {
                    var module = FindModule<IPreAlignModule>(dev.GetModuleType(), moduleIndex);
                    if (module == null)
                        continue;
                    module.SetDevice(dev);

                    moduleIndex++;
                }

                //=> OCRModules
                moduleIndex = 1;
                foreach (var dev in deviceParam.CognexOCRModules)
                {
                    var module = FindModule<ICognexOCRModule>(dev.GetModuleType(), moduleIndex);
                    if (module == null)
                        continue;
                    module.SetDevice(dev);

                    moduleIndex++;
                }

                moduleIndex = 1;
                foreach (var dev in deviceParam.SemicsOCRModules)
                {
                    var module = FindModule<ISemicsOCRModule>(dev.GetModuleType(), moduleIndex);
                    if (module == null)
                        continue;
                    module.SetDevice(dev);

                    moduleIndex++;
                }

                //=> ChuckModules
                moduleIndex = 1;
                foreach (var dev in deviceParam.ChuckModules)
                {
                    var module = FindModule<IChuckModule>(dev.GetModuleType(), moduleIndex);
                    if (module == null)
                        continue;
                    module.SetDevice(dev);

                    moduleIndex++;
                }

                //=> FixedTrayModules
                moduleIndex = 1;
                foreach (var dev in deviceParam.FixedTrayModules)
                {
                    var module = FindModule<IFixedTrayModule>(dev.GetModuleType(), moduleIndex);
                    if (module == null)
                        continue;
                    module.SetDevice(dev);

                    moduleIndex++;
                }

                //=> InspectionTrayModules
                moduleIndex = 1;
                foreach (var dev in deviceParam.InspectionTrayModules)
                {
                    var module = FindModule<IInspectionTrayModule>(dev.GetModuleType(), moduleIndex);
                    if (module == null)
                        continue;
                    module.SetDevice(dev);

                    moduleIndex++;
                }


                moduleIndex = 1;
                foreach (var dev in deviceParam.CardBufferModules)
                {
                    var module = FindModule<ICardBufferModule>(dev.GetModuleType(), moduleIndex);
                    if (module == null)
                        continue;
                    module.SetDevice(dev);

                    moduleIndex++;
                }

                moduleIndex = 1;
                foreach (var dev in deviceParam.CardArmModules)
                {
                    var module = FindModule<ICardARMModule>(dev.GetModuleType(), moduleIndex);
                    if (module == null)
                        continue;
                    module.SetDevice(dev);

                    moduleIndex++;
                }

                moduleIndex = 1;
                foreach (var dev in deviceParam.CardBufferTrayModules)
                {
                    var module = FindModule<ICardBufferTrayModule>(dev.GetModuleType(), moduleIndex);
                    if (module == null)
                        continue;
                    module.SetDevice(dev);

                    moduleIndex++;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public IAttachedModule FindModule(ModuleID id)
        {
            IAttachedModule retModule = null;

            try
            {
                retModule = _Modules.Where(item => item.ID == id).FirstOrDefault();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retModule;
        }

        public IAttachedModule FindModule(ModuleTypeEnum moduleType, int index)
        {
            IAttachedModule retModule = null;

            try
            {
                retModule = _Modules
                .Where(
                item =>
                item.ID.ModuleType == moduleType &&
                item.ID.Index == index
                ).FirstOrDefault();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retModule;
        }

        public T FindModule<T>(ModuleTypeEnum moduleType, int index) where T : class, IAttachedModule
        {
            T retVal = null;

            try
            {
                retVal = FindModules<T>()
                .Where(
                item =>
                item.ID.ModuleType == moduleType &&
                item.ID.Index == index
                ).FirstOrDefault();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public T FindModule<T>(ModuleID id) where T : class, IAttachedModule
        {
            T retVal = null;

            try
            {
                retVal = FindModules<T>()
                .Where(item => item.ID == id)
                .FirstOrDefault();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public T FindModule<T>(string label) where T : class, IAttachedModule
        {
            T retVal = null;

            try
            {
                retVal = FindModules<T>()
                .Where(item => item.ID.Label == label)
                .FirstOrDefault();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public List<T> FindModules<T>() where T : class, IAttachedModule
        {
            List<T> retVal = null;

            try
            {
                retVal = _Modules.OfType<T>().ToList();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public List<ISlotModule> FindSlots(ICassetteModule cassette)
        {
            List<ISlotModule> retVal = null;

            try
            {
                retVal = _Modules
                .OfType<ISlotModule>()
                .Where(item => item.Cassette == cassette)
                .OrderByDescending(item => item.ID.Index)
                .ToList();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public IARMModule FindUsableARM(ARMUseTypeEnum useType, params IARMModule[] excludeARMs)
        {
            IARMModule found = null;

            try
            {
                foreach (var ARM in _Modules.OfType<IARMModule>())
                {
                    if (ARM.Definition.UseType.Value.HasFlag(useType) &&
                        excludeARMs.Contains(ARM) == false &&
                        ARM.Holder.Status == EnumSubsStatus.NOT_EXIST)
                    {
                        found = ARM;
                        break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return found;
        }

        public T FindUsableModule<T>()
           where T : class, IWaferOwnable
        {
            T retVal = null;

            try
            {
                retVal = _Modules.OfType<T>()
                .Where(item => item.Holder.Status == EnumSubsStatus.NOT_EXIST)
                .FirstOrDefault();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public IPreAlignable FindUsablePreAlignable(TransferObject transferObject)
        {
            IPreAlignable retVal = null;
            try
            {
                retVal = _Modules.OfType<IPreAlignable>()
                .Where(item =>
                item.CanPreAlignable(transferObject) &&
                item.Holder.Status == EnumSubsStatus.NOT_EXIST)
                .FirstOrDefault();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public T FindUsableModule<T>(ModuleID id)
          where T : class, IWaferOwnable
        {
            var module = FindModule(id) as T;

            try
            {
                if (module != null && module.Holder.Status == EnumSubsStatus.NOT_EXIST)
                    return module;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return null;
        }

        #region => Find Substrate Object Methods
        public List<TransferObject> GetTransferObjectAll()
        {
            List<TransferObject> retList = null;

            try
            {
                retList = FindModules<IWaferOwnable>()
                .Where(item => item.Holder.Status == EnumSubsStatus.EXIST)
                .Select(item => item.Holder.TransferObject)
                .ToList();
                var cardList= FindModules<ICardOwnable>()
                .Where(item => item.Holder.Status == EnumSubsStatus.EXIST|| item.Holder.Status == EnumSubsStatus.CARRIER)
                .Select(item => item.Holder.TransferObject)
                .ToList();
                foreach (var transferObj in cardList)
                {
                    retList.Add(transferObj);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retList;
        }

        public List<TransferObject> GetUnknownTransferObjectAll()
        {
            List<TransferObject> retList = null;

            try
            {
                retList = FindModules<IWaferOwnable>()
                .Where(item => item.Holder.Status == EnumSubsStatus.UNKNOWN)
                .Select(item => item.Holder.TransferObject)
                .ToList();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retList;
        }


        public List<TransferObject> GetCardObjectAll()
        {
            List<TransferObject> retList = null;

            try
            {

                retList = FindModules<ICardOwnable>()
                .Where(item => item.Holder.Status == EnumSubsStatus.EXIST)
                .Select(item => item.Holder.TransferObject)
                .ToList();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retList;
        }

        public TransferObject FindTransferObject(string GUID)
        {
            TransferObject retObj = null;

            try
            {
                retObj = GetTransferObjectAll().Where(item => item.ID.Value == GUID).FirstOrDefault();
            }
            catch (Exception)
            {

            }
            return retObj;
        }
        public TransferObject FindUnknownTransferObject(string GUID)
        {
            TransferObject retObj = null;

            try
            {
                retObj = GetUnknownTransferObjectAll().Where(item => item.ID.Value == GUID).FirstOrDefault();
            }
            catch (Exception)
            {

            }
            return retObj;
        }

        #endregion
    }
}
