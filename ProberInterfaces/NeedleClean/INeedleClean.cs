using ProberErrorCode;
using ProberInterfaces.PnpSetup;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProberInterfaces.Param;
using System.Collections.ObjectModel;
using ProberInterfaces.SequenceRunner;

namespace ProberInterfaces.NeedleClean
{
    public enum NeedleCleanStateEnum
    {
        UNDEFINED = 0,
        IDLE,
        DONE,
        ERROR,
        RUNNING,
        READY,
        PAUSED,
        ABORT,
        SUSPENDED,
        RECOVERY
    }

    public enum NC_CleaningType
    {
        SINGLEDIR = 0,
        USER_DEFINE = 1
    }

    public enum NC_CleaningDirection
    {
        HOLD = 0,
        TOP = 1,
        TOP_RIGHT = 2,
        RIGHT = 3,
        BOTTOM_RIGHT = 4,
        BOTTOM = 5,
        BOTTOM_LEFT = 6,
        LEFT = 7,
        TOP_LEFT = 8
    }

    public enum NC_SCRUB_Direction
    {
        NO_SCRUB = 0,
        TOP = 1,
        TOP_RIGHT = 2,
        RIGHT = 3,
        BOTTOM_RIGHT = 4,
        BOTTOM = 5,
        BOTTOM_LEFT = 6,
        LEFT = 7,
        TOP_LEFT = 8,
        SQUARE = 9,
        OCTAGONAL = 10
    }

    public interface INeedleCleanHeightProfiling
    {
        IParam NeedleCleanHeightProfilingParameter { get; set; }
        bool GetEnable();
        double GetPZErrorComp(double x, double y, double z);
    }


    public interface INeeldleCleanerSubRoutineStandard : ISubRoutine, IFactoryModule, IModule
    {
        bool IsNCSensorON();
        bool IsCleanPadUP();
        bool IsCleanPadDown();
        EventCodeEnum WaitForCleanPadUp();
        EventCodeEnum WaitForCleanPadDown();
        EventCodeEnum CleanPadUP(bool bWait);
        EventCodeEnum CleanPadDown(bool bWait);
        double GetMeasuredNcPadHeight(int ncNum, double posX, double posY);
        NCCoordinate ReadNcCurPosForWaferCam(int ncNum);
        NCCoordinate ReadNcCurPosForPin(int ncNum);
        NCCoordinate ReadNcCurPosForSensor(int ncNum);
        bool IsTimeToCleaning(int ncNum);

    }

    public interface INCSysParam : IParam
    {

    }

    public interface INCSheetProcParam
    {
        Element<long> ContactLimit { get; }
        Element<long> ContactCount { get; }
        Element<long> CycleLimit { get; }
        Element<long> CycleCount { get; }
        Element<NCCoordinate> LastCleaningPos { get; }
        Element<int> LastStartingPos { get; }
    }

    public interface INeedleCleanDeviceObject
    {
        List<Element<NCCoordinate>> UserDefinedSeq { get; }
        Element<NC_CleaningDirection> CleaningDirection { get; }
        Element<NC_CleaningType> CleaningType { get; }
        Element<long> CleaningDistance { get; }
        Element<int> CleaningCount { get; }
        Element<double> Overdrive { get; }
        Element<double> Clearance { get; }
        Element<double> ScrubLength { get; }
        Element<NC_SCRUB_Direction> ScrubDirection { get; }
    }

    public interface INeedleCleanObject : IHasSysParameterizable, IParamNode, IModuleParameter
    {
        ISubRoutine SubRoutine { get; set; }
        IParam NCSysParam_IParam { get; set; }
        Element<PinCoordinate> SensorPos { get; }
        Element<PinCoordinate> SensorFocusedPos { get; }
        Element<PinCoordinate> SensorBasePos { get; }
        Element<NCCoordinate> SensingPadBasePos { get; }
        bool PinAlignBeforeCleaningProcessed { get; set; }
        bool PinAlignAfterCleaningProcessed { get; set; }
        bool NeedleCleaningProcessed { get; set; }
        double NeedleCleanPadHeight { get; }
        double NeedleCleanPadWidth { get; }
        void InitCleanPadRender();
        void InitNCSequenceRender();
        byte[] GetNCObjectByteArray();
        void NCSheetVMDefsUpdated();
    }

    public interface INeedleCleanViewModel
    {

    }
    public delegate EventCodeEnum CleanUnitFocusing5pt(int ncNum);

    public interface INeedleCleanModule : IStateModule, IPnpSetupScreen,
        IHasDevParameterizable, IHasSysParameterizable, ITemplateStateModule
    {
        IParam NeedleCleanDeviceParameter_IParam { get; set; }
        INeedleCleanHeightProfiling NCHeightProfilingModule { get; set; }
        ObservableCollection<ISequenceBehaviorGroupItem> GetNCPadChangeGroupCollection();
        CleanUnitFocusing5pt DelFocusing { get; set; }
        bool IsNCSensorON();
        bool IsCleanPadUP();
        bool IsCleanPadDown();
        EventCodeEnum WaitForCleanPadUp();
        EventCodeEnum WaitForCleanPadDown();
        EventCodeEnum CleanPadUP(bool bWait);
        EventCodeEnum CleanPadDown(bool bWait);
        ProbeAxisObject NCAxis { get; }
        double GetMeasuredNcPadHeight(int ncNum, double posX, double posY);
        NCCoordinate ReadNcCurPosForWaferCam(int ncNum);
        NCCoordinate ReadNcCurPosForPin(int ncNum);
        NCCoordinate ReadNcCurPosForSensor(int ncNum);
        bool IsTimeToCleaning(int ncNum);
        Task<EventCodeEnum> Focusing5pt(int ncNum);
        EventCodeEnum DoNeedleCleaningProcess();
    }
}
