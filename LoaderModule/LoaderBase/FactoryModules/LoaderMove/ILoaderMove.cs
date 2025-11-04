
using ProberErrorCode;
using ProberInterfaces;

namespace LoaderBase
{
    /// <summary>
    /// LoaderMove를 정의합니다.
    /// </summary>
    public interface ILoaderMove : ILoaderFactoryModule
    {
        /// <summary>
        /// Move State를 가져옵니다.
        /// </summary>
        LoaderMoveStateEnum State { get; }

        /// <summary>
        /// FoupAccessMode 를 가져옵니다.
        /// </summary>
        /// <param name="cassetteNumber">카세트 번호</param>
        /// <returns>FoupAccessMode</returns>
        FoupAccessModeEnum GetFoupAccessMode(int cassetteNumber);

        /// <summary>
        /// Motion을 초기화합니다.
        /// </summary>
        /// <returns></returns>
        EventCodeEnum MotionInit();

        /// <summary>
        /// JogAbsMove
        /// </summary>
        /// <param name="axis">axis</param>
        /// <param name="val">val</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum JogAbsMove(EnumAxisConstants axis, double val);

        /// <summary>
        /// JogRelMove
        /// </summary>
        /// <param name="axis">axis</param>
        /// <param name="val">val</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum JogRelMove(EnumAxisConstants axis, double val);

        /// <summary>
        /// 모든 모션을 안전위치로 이동합니다.
        /// </summary>
        /// <param name="movingType">movingType</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum RetractAll(LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL);



        /// <summary>
        /// W축을 안전위치로 이동합니다.
        /// </summary>
        /// <param name="movingType">movingType</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SafePosW();

        /// <summary>
        /// 카메라 스캔 시작 위치로 이동합니다. (Slot1 Position)
        /// </summary>
        /// <param name="CamScan"></param>
        /// <param name="Cassette"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum ScanCameraStartPosMove(IScanCameraModule CamScan, ICassetteModule Cassette);

        /// <summary>
        /// 카메라 스캔상태에서 RelMove를 수행합니다.
        /// </summary>
        /// <param name="axis">axis</param>
        /// <param name="value">value</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum ScanCameraRelMove(EnumAxisConstants axis, double value);

        /// <summary>
        /// 센서 스캔 시작 위치로 이동합니다.
        /// </summary>
        /// <param name="ScanSensor"></param>
        /// <param name="Cassette"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum ScanSensorStartPosMove(IScanSensorModule ScanSensor, ICassetteModule Cassette);

        /// <summary>
        /// 센서 스캔 모듈을 Extend 합니다.
        /// </summary>
        /// <param name="ScanSensor"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum ExtendScanSensor(IScanSensorModule ScanSensor);

        /// <summary>
        /// 센서 스캔 모듈이 Up 위치로 이동합니다.
        /// </summary>
        /// <param name="ScanSensor"></param>
        /// <param name="Cassette"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum ScanSensorUpMove(IScanSensorModule ScanSensor, ICassetteModule Cassette);

        /// <summary>
        /// 센서 스캔 모듈이 Home위치로 이동하고 Retract 합니다.
        /// </summary>
        /// <param name="ScanSensor"></param>
        /// <param name="Cassette"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum ScanSensorDownMove(IScanSensorModule ScanSensor, ICassetteModule Cassette);

        /// <summary>
        /// Slot Up 위치로 이동합니다.
        /// </summary>
        /// <param name="ARM"></param>
        /// <param name="Slot"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SlotUpMove(IARMModule ARM, ISlotModule Slot);

        /// <summary>
        /// Slot Down 위치로 이동합니다.
        /// </summary>
        /// <param name="ARM"></param>
        /// <param name="Slot"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SlotDownMove(IARMModule ARM, ISlotModule Slot);

        /// <summary>
        /// OCR 위치에서 PreAlign Up 위치로 이동합니다.
        /// </summary>
        /// <param name="ARM"></param>
        /// <param name="PA"></param>
        /// <param name="OCR"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum PreAlignUpMoveFromOCR(IARMModule ARM, IPreAlignModule PA, IOCRReadable OCR);

        /// <summary>
        /// PreAlign Up 위치로 이동합니다.
        /// </summary>
        /// <param name="ARM"></param>
        /// <param name="PA"></param>
        /// <param name="movingType"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum PreAlignUpMove(IARMModule ARM, IPreAlignModule PA, LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL);

        /// <summary>
        /// Prealign Up 위치로 이동합니다.
        /// </summary>
        /// <param name="ARM"></param>
        /// <param name="PA"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum PreAlignDownMove(IARMModule ARM, IPreAlignModule PA);

        /// <summary>
        /// PreAlign 상태에서 RelMove합니다. (U axis)
        /// </summary>
        /// <param name="ARM"></param>
        /// <param name="val"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum PreAlignRelMove(IARMModule ARM, double val);

        /// <summary>
        /// PreAlign 상태에서 RelMove합니다. (V axis)
        /// </summary>
        /// <param name="PA"></param>
        /// <param name="val"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum PreAlignRelMove(IPreAlignModule PA, double val);

        /// <summary>
        /// PreAlignModule이 Zero position으로 이동합니다. (V axis)
        /// </summary>
        /// <param name="PA"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum PreAlignZeroMove(IPreAlignModule PA);

        /// <summary>
        /// Wafer의 Notch 중앙으로 이동합니다 (V axis)
        /// </summary>
        /// <param name="PA"></param>
        /// <param name="input"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum FindNotchMove(IPreAlignModule PA, EnumMotorDedicatedIn input);

        /// <summary>
        /// PreAlign Up 상태에서 OCR Pos로 이동합니다.
        /// </summary>
        /// <param name="UseARM"></param>
        /// <param name="OCR"></param>
        /// <param name="PA"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum OCRMoveFromPreAlignUp(IARMModule UseARM, IOCRReadable OCR, IPreAlignModule PA);

        /// <summary>
        /// InspectionTray  Up 위치로 이동합니다.
        /// </summary>
        /// <param name="ARM"></param>
        /// <param name="InspectionTray"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum InspectionTrayUpMove(IARMModule ARM, IInspectionTrayModule InspectionTray);

        /// <summary>
        /// InspectionTray Down 위치로 이동합니다.
        /// </summary>
        /// <param name="ARM"></param>
        /// <param name="InspectionTray"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum InspectionTrayDownMove(IARMModule ARM, IInspectionTrayModule InspectionTray);

        /// <summary>
        /// FixedTray Up 위치로 이동합니다.
        /// </summary>
        /// <param name="ARM"></param>
        /// <param name="FixedTray"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum FixedTrayUpMove(IARMModule ARM, IFixedTrayModule FixedTray);

        /// <summary>
        /// FixedTray Down 위치로 이동합니다.
        /// </summary>
        /// <param name="ARM"></param>
        /// <param name="FixedTray"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum FixedTrayDownMove(IARMModule ARM, IFixedTrayModule FixedTray);

        /// <summary>
        /// PreChuck 위치로 이동합니다. 
        /// </summary>
        /// <param name="ARM"></param>
        /// <param name="Chuck"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum PreChuckUpMove(IARMModule ARM, IChuckModule Chuck);

        /// <summary>
        /// PreChuck 위치로 이동합니다.
        /// </summary>
        /// <param name="ARM"></param>
        /// <param name="Chuck"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum PreChuckDownMove(IARMModule ARM, IChuckModule Chuck);

        /// <summary>
        /// Chuck Up 위치로 이동합니다.
        /// </summary>
        /// <param name="ARM"></param>
        /// <param name="Chuck"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum ChuckUpMove(IARMModule ARM, IChuckModule Chuck);

        /// <summary>
        /// Chuck Down 위치로 이동합니다.
        /// </summary>
        /// <param name="ARM"></param>
        /// <param name="Chuck"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum ChuckDownMove(IARMModule ARM, IChuckModule Chuck);

        /// <summary>
        /// 대상의 Up 위치로 이동합니다. (A axis)
        /// </summary>
        /// <param name="ARM"></param>
        /// <param name="Target"></param>
        /// <param name="movingType"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum PickUp(IARMModule ARM, IWaferOwnable Target, 
            LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.ACCESS);

        /// <summary>
        /// 대상의 Down 위치로 이동합니다. (A axis)
        /// </summary>
        /// <param name="ARM"></param>
        /// <param name="Target"></param>
        /// <param name="movingType"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum PlaceDown(IARMModule ARM, IWaferOwnable Target, 
            LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.ACCESS);

        /// <summary>
        /// Slot1 Down 위치로 이동합니다. (A axis)
        /// </summary>
        /// <param name="ScanSensor"></param>
        /// <param name="Cassete"></param>
        /// <param name="SubstrateType"></param>
        /// <param name="SubstrateSize"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetupToCstSlot1(IScanSensorModule ScanSensor, ICassetteModule Cassette, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize,bool uaxisskip,int slotnum,int index=1);

        /// <summary>
        ///  PreAlign Down 위치로 이동합니다. (A axis)
        /// </summary>
        /// <param name="Arm"></param>
        /// <param name="PA"></param>
        /// <param name="MovingType"></param>
        /// <param name="SubstrateType"></param>
        /// <param name="SubstrateSize"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetupToPAMove(IARMModule ARM, IPreAlignModule PA, LoaderMovingTypeEnum movingType, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool uaxisskip, int slotnum, int index = 1);
        /// <summary>
        ///  OCRSetup 위치로 이동합니다. (A axis)
        /// </summary>
        /// <param name="Arm"></param>
        /// <param name="PA"></param>
        /// <param name="OCR"></param>
        /// <param name="SubstrateType"></param>
        /// <param name="SubstrateSize"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetupToOCRMove(IARMModule ARM, IPreAlignModule PA, IOCRReadable OCR, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool uaxisskip, int slotnum, int index = 1);

        /// <summary>
        ///  ChuckSetup 위치로 이동합니다. (A axis)
        /// </summary>
        /// <param name="Arm"></param>
        /// <param name="Chuck"></param>
        /// <param name="SubstrateType"></param>
        /// <param name="SubstrateSize"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetupToChuckMove(IARMModule ARM, IChuckModule Chuck, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool uaxisskip, int slotnum, int index = 1);

        /// <summary>
        ///  ChuckSetup 위치로 이동합니다. (A axis)
        /// </summary>
        /// <param name="Arm"></param>
        /// <param name="FixedTray"></param>
        /// <param name="SubstrateType"></param>
        /// <param name="SubstrateSize"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetupToFixedTrayMove(IARMModule ARM, IFixedTrayModule FixedTray, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool uaxisskip, int slotnum, int index = 1);

        /// <summary>
        ///  ChuckSetup 위치로 이동합니다. (A axis)
        /// </summary>
        /// <param name="Arm"></param>
        /// <param name="InspectionTray"></param>
        /// <param name="SubstrateType"></param>
        /// <param name="SubstrateSize"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetupToInspectionTrayMove(IARMModule ARM, IInspectionTrayModule InspectionTray, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool uaxisskip, int slotnum, int index = 1);

        /// <summary>
        ///  ChuckSetup 위치로 이동합니다. (A axis)
        /// </summary>
        /// <param name="ScanSensor"></param>
        /// <param name="Cassette"></param>
        /// <param name="SubstrateType"></param>
        /// <param name="SubstrateSize"></param>
        /// <param name="CstNum"></param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetupToCstMove(IARMModule ARM, ISlotModule Slot, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool uaxisskip, int slotnum, int index = 1);
        /// 
    }//end of class

    /// <summary>
    /// LoaderMoveState 를 정의합니다.
    /// </summary>
    public enum LoaderMoveStateEnum
    {
        /// <summary>
        /// UNDEFINED
        /// </summary>
        UNDEFINED,
        /// <summary>
        /// ERROR
        /// </summary>
        ERROR,
        /// <summary>
        /// Jog
        /// </summary>
        Jog,
        /// <summary>
        /// Home
        /// </summary>
        Home,
        /// <summary>
        /// Retracted
        /// </summary>
        Retracted,
        /// <summary>
        /// ScanCameraHome
        /// </summary>
        ScanCameraHome,
        /// <summary>
        /// ScanSensorHome
        /// </summary>
        ScanSensorHome,
        /// <summary>
        /// ScanSensorExtended
        /// </summary>
        ScanSensorExtended,
        /// <summary>
        /// SlotUp
        /// </summary>
        SlotUp,
        /// <summary>
        /// SlotDown
        /// </summary>
        SlotDown,
        /// <summary>
        /// PreAlignUp
        /// </summary>
        PreAlignUp,
        /// <summary>
        /// PreAlignDown
        /// </summary>
        PreAlignDown,
        /// <summary>
        /// OCR
        /// </summary>
        OCR,
        /// <summary>
        /// InspectionTrayUp
        /// </summary>
        InspectionTrayUp,
        /// <summary>
        /// InspectionTrayDown
        /// </summary>
        InspectionTrayDown,
        /// <summary>
        /// FixedTrayUp
        /// </summary>
        FixedTrayUp,
        /// <summary>
        /// FixedTrayDown
        /// </summary>
        FixedTrayDown,
        /// <summary>
        /// PreChuck
        /// </summary>
        PreChuck,
        /// <summary>
        /// ChuckUp
        /// </summary>
        ChuckUp,
        /// <summary>
        /// ChuckDown
        /// </summary>
        ChuckDown,
        /// <summary>
        /// LoaderSetup
        /// </summary>
        LoaderSetup,
    }

    /// <summary>
    /// LoaderMovingType을 정의합니다.
    /// </summary>
    public enum LoaderMovingTypeEnum
    {
        /// <summary>
        /// NORMAL
        /// </summary>
        NORMAL,
        /// <summary>
        /// RECOVERY
        /// </summary>
        RECOVERY,
        /// <summary>
        /// ACCESS
        /// </summary>
        ACCESS,
    }

    /// <summary>
    /// FoupAccessMode를 정의합니다.
    /// </summary>
    public enum FoupAccessModeEnum
    {
        /// <summary>
        /// UNKNOWN
        /// </summary>
        UNKNOWN,
        /// <summary>
        /// NO_ACCESSED
        /// </summary>
        NO_ACCESSED,
        /// <summary>
        /// ACCESSED
        /// </summary>
        ACCESSED,
    }
}
