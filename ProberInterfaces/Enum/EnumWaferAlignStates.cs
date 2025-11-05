namespace ProberInterfaces.Enum
{

    public class EnumWaferAlignStates
    {
        private EnumEdgeStates _EdgeStatus;

        public EnumEdgeStates EdgeStatus
        {
            get { return _EdgeStatus; }
            set { _EdgeStatus = value; }
        }

        private EnumLowMagStates _LowMagStatus;

        public EnumLowMagStates LowMagStatus
        {
            get { return _LowMagStatus; }
            set { _LowMagStatus = value; }
        }

        private EnumHighMagStates _HighMagStatus;

        public EnumHighMagStates HighMagStatus
        {
            get { return _HighMagStatus; }
            set { _HighMagStatus = value; }
        }
    }
    public enum EnumWaferAlignFunctions
    {
        INVALID = -1,
        UNDEFINED = 0,
        EDGEEDCTION = UNDEFINED + 1,
        LOWMAG,
        HIGHMAG,
        HEIGHTPROFILING
    }
    public enum EnumEdgeStates
    {
        INVALID = -1,
        UNDEFINED = 0,
        EDGE_RUN,
        EDGE_DONE,
        EDGE_FAIL,
        EDGE_ABORT,
        EDGE_PAUSE
    }

    public enum EnumLowMagStates
    {
        INVALID = -1,
        UNDEFINED = 0,
        LOWMAG_RUN,
        LOWMAG_DONE,
        LOWMAG_FAIL,
        LOWMAG_ABORT,
        LOWMAG_PAUSE
    }
    public enum EnumHighMagStates
    {
        INVALID = -1,
        UNDEFINED = 0,
        HIGHMAG_RUN,
        HIGHMAG_DONE,
        HIGHMAG_FAIL,
        HIGHMAG_ABORT,
        HIGHMAG_PAUSE
    }
    public enum EnumHeightProfilingStates
    {
        INVALID = -1,
        UNDEFINED = 0,
        HIGHMAG_RUN,
        HIGHMAG_DONE,
        HIGHMAG_FAIL,
        HIGHMAG_ABORT,
        HIGHMAG_PAUSE
    }

    public enum EnumWaferAlignProcessingStates
    {
        INVALID = -1,
        UNDEFINED = 0,
        BLOB_FAIL,
        BLOB_DONE,
        PATTERNMATCHING_FAIL,
        PATTERNMATCHING_DONE,
        FOCUSING_FAIL,
        FOCUSING_DONE
    }
}

