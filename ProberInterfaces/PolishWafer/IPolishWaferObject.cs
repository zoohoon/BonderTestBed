using ProberInterfaces.Param;

namespace ProberInterfaces.PolishWafer
{

    public class PolishWaferCleaningJobParam
    {
        private WaferCoordinate _CleaningPosition;

        public WaferCoordinate CleaningPosition
        {
            get { return _CleaningPosition; }
            set { _CleaningPosition = value; }
        }

        private bool _CleaningDoneFlag;

        public bool CleaningDoneFlag
        {
            get { return _CleaningDoneFlag; }
            set { _CleaningDoneFlag = value; }
        }

        public PolishWaferCleaningJobParam(WaferCoordinate position)
        {
            CleaningPosition = position;
        }
    }

    #region //..Enum
    public enum EnumCleaningWaferState
    {
        UNDEFIEND = -1,
        READY,
        LOAD,
        PROCESSING,
        PROCESSED
    }
    public enum EnumCleaningWaferPhysicalType
    {
        UNDEFIEND = -1,
        ABRAISIVE,
        GEL
    }

    public enum EnumPolishWaferCleaningMediaSource
    {
        UNDEFIEND = -1,
        FOUP = UNDEFIEND + 1,
        BUFFER_TRAY,
        INSPECTION_TRAY
    }

    public enum EnumPolishWaferFocusingMode
    {
        UNDEFIEND = -1,
        POINT1 = 1,
        POINT5 = 5
    }

    public enum EnumPolishWaferFocusingType
    {
        UNDEFIEND = -1,
        CAMERA,
        TOUCHSENSOR
    }

    public enum EnumCleaningTriggerMode
    {
        UNDEFIEND = -1,
        LOT_START = UNDEFIEND + 1,
        LOT_END,
        WAFER_INTERVAL,
        TOUCHDOWN_COUNT
    }

    public enum EnumCleaningType
    {
        UNDEFIEND = -1,
        ABRAISIVE,
        GEL,
        //ABRAISIVE + GEL
        COMBO
    }
    public enum CleaningDirection
    {
        UNDEFIEND = -1,
        Right,
        Left,
        Up,
        Down,
        Right_Up,
        Right_Down,
        Left_Up,
        Left_Down
    }

    public enum EnumCleaningContactSeqMode
    {
        UNDEFIEND = -1,
        ContactLength,
        PositionShift
    }

    public enum EnumCleaningMode
    {
        UNDEFIEND = -1,
        UP_DOWN,
        One_Direction,
        Octagonal,
        Square
    }

    #endregion
}
