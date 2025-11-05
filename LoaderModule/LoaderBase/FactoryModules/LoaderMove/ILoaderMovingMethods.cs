using System;
using Autofac;
using LoaderBase;
using LoaderParameters;
using ProberErrorCode;
using ProberInterfaces;

namespace LoaderCore
{
    public interface ILoaderMovingMethods
    {
        IContainer Container { get; set; }
        ILoaderModule Loader { get; }
        IUExtensionObject UExtension { get; set; }

        EventCodeEnum ChuckDownMove(IARMModule ARM, IChuckModule Chuck);
        EventCodeEnum ChuckUpMove(IARMModule ARM, IChuckModule Chuck);
        EventCodeEnum FixedTrayDownMove(IARMModule ARM, IFixedTrayModule FixedTray);
        EventCodeEnum FixedTrayUpMove(IARMModule ARM, IFixedTrayModule FixedTray);
        EventCodeEnum FoupCoverDown(ICassetteModule Cassette);
        EventCodeEnum FoupCoverUp(ICassetteModule Cassette);
        EventCodeEnum Init(IContainer container, UExtensionBase extension);
        EventCodeEnum InspectionTrayDownMove(IARMModule ARM, IInspectionTrayModule InspectionTray);
        EventCodeEnum InspectionTrayUpMove(IARMModule ARM, IInspectionTrayModule InspectionTray);
        EventCodeEnum MotionInit();
        EventCodeEnum OCRMoveFromPreAlignUp(IARMModule ARM, IOCRReadable OCR, IPreAlignModule PA);
        EventCodeEnum PreAlignDownMove(IARMModule ARM, IPreAlignModule PA);
        EventCodeEnum PreAlignUpMove(IARMModule ARM, IPreAlignModule PA, LoaderMovingTypeEnum movingType);
        EventCodeEnum PreAlignUpMoveFromOCR(IARMModule ARM, IPreAlignModule PA, IOCRReadable OCR);
        EventCodeEnum PreChuckDownMove(IARMModule ARM, IChuckModule Chuck);
        EventCodeEnum PreChuckUpMove(IARMModule ARM, IChuckModule Chuck);
        EventCodeEnum RelMove(EnumAxisConstants axis, double value, LoaderMovingTypeEnum movingType);
        EventCodeEnum RelMove(EnumAxisConstants axis, double value, LoaderMovingTypeEnum movingType, Func<bool> stopFunc, bool resumeVal);
        EventCodeEnum RetractAll(LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL);
        EventCodeEnum RetractARMs(LoaderMovingTypeEnum movingType);
        EventCodeEnum RetractScanSensorAll();
        EventCodeEnum ScanCameraSlot1PosMove(IScanCameraModule ScanCamera, ICassetteModule Cassette);
        EventCodeEnum ScanSensorDownMove(IScanSensorModule ScanSensor, ICassetteModule Cassette);
        EventCodeEnum ScanSensorOut(IScanSensorModule ScanSensor);
        EventCodeEnum ScanSensorStartPosMove(IScanSensorModule ScanSensor, ICassetteModule Cassette);
        EventCodeEnum ScanSensorUpMove(IScanSensorModule ScanSensor, ICassetteModule Cassette);
        EventCodeEnum SlotDownMove(IARMModule ARM, ISlotModule Slot);
        EventCodeEnum SlotUpMove(IARMModule ARM, ISlotModule Slot);

        EventCodeEnum SafePosW();

        #region SetupMethod

        EventCodeEnum SetupToCstSlot1Method(IScanSensorModule ScanSensor, ICassetteModule Cassette, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool skipuaxis,int slot,int index);
        EventCodeEnum SetupToPAMoveMethod(IARMModule ARM, IPreAlignModule PA, LoaderMovingTypeEnum movingType, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool skipuaxis, int slot, int index);
        EventCodeEnum SetupToOCRMoveMethod(IARMModule ARM, IPreAlignModule PA, IOCRReadable OCR, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool skipuaxis, int slot, int index);
        EventCodeEnum SetupToChuckMoveMethod(IARMModule ARM, IChuckModule Chuck, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool skipuaxis, int slot, int index);
        EventCodeEnum SetupToFixedTrayMoveMethod(IARMModule ARM, IFixedTrayModule FixedTray, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool skipuaxis, int slot, int index);
        EventCodeEnum SetupToInspectionTrayMoveMethod(IARMModule ARM, IInspectionTrayModule InspectionTray, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool skipuaxis, int slot, int index);
        EventCodeEnum SetupToCstMoveMethod(IARMModule ARM, ISlotModule Slot,SubstrateTypeEnum subtype ,SubstrateSizeEnum subsize, bool skipuaxis, int slot, int index);

        #endregion

    }
}