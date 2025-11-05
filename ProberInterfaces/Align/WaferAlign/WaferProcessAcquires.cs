
using ProberInterfaces.WaferAlignEX;
using System;

namespace ProberInterfaces.WaferAlign
{
    // Wafer center
    // Low mag. align, Theta
    // High mag. align, Theta
    // Die size X, Theta
    // Die size Y, Squareness
    // Height profile
    // Lower left corner
    // Coordinate origin
    // N-Die alignment
    [Serializable]
    public class WaferCenter : WaferAlignProcessAcq
    {
        public override WaferAlignProcAcqEnum GetAcqType()
        {
            return WaferAlignProcAcqEnum.WAFER_CENTER;
        }
    }
    [Serializable]
    public class LowMagAlign : WaferAlignProcessAcq
    {
        public override WaferAlignProcAcqEnum GetAcqType()
        {
            return WaferAlignProcAcqEnum.LOW_MAG_ALIGN;
        }
    }
    [Serializable]
    public class HighMagAlign : WaferAlignProcessAcq
    {
        public override WaferAlignProcAcqEnum GetAcqType()
        {
            return WaferAlignProcAcqEnum.HI_MAG_ALIGN;
        }
    }
    [Serializable]
    public class DieSizeX : WaferAlignProcessAcq
    {
        public override WaferAlignProcAcqEnum GetAcqType()
        {
            return WaferAlignProcAcqEnum.DIE_SIZE_X;
        }
    }
    [Serializable]
    public class DieSizeY : WaferAlignProcessAcq
    {
        public override WaferAlignProcAcqEnum GetAcqType()
        {
            return WaferAlignProcAcqEnum.DIE_SIZE_Y;
        }
    }
    [Serializable]
    public class WaferAngle : WaferAlignProcessAcq
    {
        public override WaferAlignProcAcqEnum GetAcqType()
        {
            return WaferAlignProcAcqEnum.ANGLE;
        }
    }
    [Serializable]
    public class HDWaferAngle : WaferAlignProcessAcq
    {
        public override WaferAlignProcAcqEnum GetAcqType()
        {
            return WaferAlignProcAcqEnum.HD_ANGLE;
        }
    }
    [Serializable]
    public class WaferSquareness : WaferAlignProcessAcq
    {
        public override WaferAlignProcAcqEnum GetAcqType()
        {
            return WaferAlignProcAcqEnum.SQUARENESS;
        }
    }
    [Serializable]
    public class HeightProfile : WaferAlignProcessAcq
    {
        public override WaferAlignProcAcqEnum GetAcqType()
        {
            return WaferAlignProcAcqEnum.HEIGHT_PROFILE;
        }
    }
    [Serializable]
    public class LowerLeftCorner : WaferAlignProcessAcq
    {
        public override WaferAlignProcAcqEnum GetAcqType()
        {
            return WaferAlignProcAcqEnum.LOWER_LEFT_EDGE;
        }
    }
    [Serializable]
    public class CoordinateOrigin : WaferAlignProcessAcq
    {
        public override WaferAlignProcAcqEnum GetAcqType()
        {
            return WaferAlignProcAcqEnum.COORD_ORIGIN;
        }
    }
    [Serializable]
    public class SingulationOffset : WaferAlignProcessAcq
    {
        public override WaferAlignProcAcqEnum GetAcqType()
        {
            return WaferAlignProcAcqEnum.SINGULATION_OFFSET;
        }
    }
    [Serializable]
    public class RegistPad : WaferAlignProcessAcq
    {
        public override WaferAlignProcAcqEnum GetAcqType()
        {
            return WaferAlignProcAcqEnum.SINGULATION_OFFSET;
        }
    }
}
