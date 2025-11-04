using System;
using System.Linq;

using Autofac;
using LoaderBase;
using ProberInterfaces;
using LoaderParameters;
using ProberErrorCode;
using LogModule;
using ProberInterfaces.Enum;
using LoaderParameters.Data;

namespace LoaderCore
{
    internal class CognexOCRModule : AttachedModuleBase, ICognexOCRModule
    {
        public override bool Initialized { get; set; } = false;
        public override ModuleTypeEnum ModuleType => ModuleTypeEnum.COGNEXOCR;

        public OCRTypeEnum OCRType => OCRTypeEnum.COGNEX;

        public OCRDefinitionBase OCRDefinitionBase => Definition;

        public CognexOCRDefinition Definition { get; set; }
        
        public CognexOCRDevice Device { get; set; }
        public ReservationInfo ReservationInfo { get; set; }

        public EventCodeEnum SetDefinition(CognexOCRDefinition definition, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Definition = definition;
                ID = ModuleID.Create(ModuleType, index, "");

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            
            return retVal;
        }

        public EventCodeEnum SetDevice(CognexOCRDevice device)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Device = device;

                if (string.IsNullOrEmpty(device.Label.Value) == false)
                    this.ID = ModuleID.Create(ID.ModuleType, ID.Index, device.Label.Value);

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            
            return retVal;
        }

        public override EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    Initialized = false;

                    ReservationInfo = new ReservationInfo();

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            
            return retval;
        }

        public override void DeInitModule()
        {

        }
        
        public bool CanOCR(TransferObject subObj)
        {
            bool retval = false;

            try
            {
                bool OCRTypeEquals =
                OCRType == subObj.OCRType.Value &&
                Definition.OCRDirection.Value == subObj.OCRDirection.Value;

                bool hasParam = Definition.AccessParams
                    .Count(
                    item =>
                    item.SubstrateType.Value == subObj.Type.Value &&
                    item.SubstrateSize.Value == subObj.Size.Value) > 0;

                retval = OCRTypeEquals && hasParam;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public IPreAlignModule GetDependecyPA()
        {
            IPreAlignModule PA = null;

            try
            {
                PA = Loader.ModuleManager.FindModule(ModuleTypeEnum.PA, this.ID.Index) as IPreAlignModule;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return PA;
        }

        public OCROffset GetOCROffset()
        {
            OCROffset offset = new OCROffset();

            try
            {
                offset.OffsetU = Device.OffsetU.Value;
                offset.OffsetW = Device.OffsetW.Value;
                offset.OffsetV = Device.OffsetV.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            
            return offset;
        }

        public OCRAccessParam GetAccessParam(SubstrateTypeEnum type, SubstrateSizeEnum size)
        {
            OCRAccessParam system = null;

            try
            {
                system = Definition.AccessParams
               .Where(
               item =>
               item.SubstrateType.Value == type &&
               item.SubstrateSize.Value == size
               ).FirstOrDefault();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return system;
        }
        public SubchuckMotionParam GetSubchuckMotionParam(SubstrateSizeEnum size)
        {
            SubchuckMotionParam param = null;

            try
            {
                param = Definition.SubchuckMotionParams.Where(
                    item =>
                    item.SubstrateSize.Value == size
                    ).FirstOrDefault();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return param;
        }
    }
}
