using ProberInterfaces.Param;
using ProberInterfaces.PnpSetup;
using ProberErrorCode;

namespace ProberInterfaces.MarkAlign
{
    public interface IMarkAligner : ITemplateStateModule, IHasSysParameterizable, IStateModule, IPnpSetupScreen
    {
        IParam MarkAlignParam_IParam { get; set; }
        IFocusing MarkFocusModel { get; }
        bool UpdatePadCen { get; set; }

        double DiffMarkPosX { get; set; }

        double DiffMarkPosY { get; set; }
        bool ForceWaferCamCylinderExtended { get; set; }

        // Mark Align 수행 시 개별 Process를 수행하기 위한 변수 추가
        EnumMarkAlignProcMode MarkAlignProcMode { get; set; }

        EventCodeEnum DoMarkAlign(bool Force = false, bool updatePadCen = false, EnumMarkAlignProcMode Mode = EnumMarkAlignProcMode.None);

        MachineCoordinate MarkCumulativeChangeValue { get; set; }
        bool IsOnDebugMode { get; set; }
        bool IsSaveImageCompVerify { get; set; }
        string CompVerifyImagePathBase { get; set; }
        //Task<FocusingRet> FocusingAsync(EnumProberCam cam, Rect roi, CancellationToken token);
        bool GetMarkCompensationEnable();

        bool GetPinCompensationEnable();

        void SetPinCompensationEnable(bool enable);
        double GetMarkDiffTolerance_X();
        double GetMarkDiffTolerance_Y();
        int GetDelaywaferCamCylinderExtendedBeforeFocusing();
        void SetDelaywaferCamCylinderExtendedBeforeFocusing(int delaytime);
        void SetMarkDiffTolerance_X(double tolerancex);
        void SetMarkDiffTolerance_Y(double tolerancey);
        void SetTriggerMarkVerificationAfterWaferAlign(bool markverification);
        bool GetTriggerMarkVerificationAfterWaferAlign();

        IMarkAlignControlItems MarkAlignControlItems { get; }
        (double, double) GetMarkDiffToleranceOfWA();
        void SetMarkDiffToleranceOfWA(double toleranceX, double toleranceY);
        bool GetExecuteRetryAlignment();
    }


    public enum MarkAlignStateEnum
    {
        UNDEFINED = 0,
        INIT = 1,
        IDLE = 2,
        RUNNING = 3,
        SUSPENDED = 4,
        DONE = 5,
        ERROR = 6,
        FocusingFail,
        PatternFail,
        PAUSED,
        ABORT
    }

    public enum EnumMarkAlignProcMode
    {
        None,
        OnlyMoveToMark,
        OnlyFocusing,
        OnlyPatternMatching
    }

    public interface IMarkAlignControlItems
    {
        bool MARK_ALIGN_MOVE_ERROR { get; set; }
        bool MARK_ALIGN_FOCUSING_FAILED { get; set; }
        bool MARK_Pattern_Failure { get; set; }
        bool MARK_ALGIN_PATTERN_MATCH_FAILED { get; set; }
        bool MARK_ALIGN_SHIFT { get; set; }
    }
}
