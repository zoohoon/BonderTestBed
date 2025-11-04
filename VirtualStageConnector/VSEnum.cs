using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualStageConnector
{
    public enum ImageType
    {
        SNAP,
        MARK,
        PA,
        PW,
        WAFER_EDGE
    }

    public enum TCPCommand
    {
        GRAB_SNAP = 1000,
        GRAB_LIVEON = 1001,
        GRAB_LIVEOFF = 1002,
        GRAB_MARK = 1003,
        GRAB_PA = 1004,
        //GRAB_PW = 1005,
        GRAB_WAFER_EDGE = 1006,

        MOVEXY = 2000,
        MOVEXY_ORGIN = 2001,
        ROTATE = 2002,
        ROTATE_ORIGIN = 2003,
        MOVEXY_PINORIGIN = 2004,
        CALIBRATED_ORIGIN = 2005,   // x, y, z, theta offset
        VIRTUAL_PROBING_ON = 2006,
        VIRTUAL_PROBING_OFF = 2007,
        WAFER_HIGHMAG = 3000,
        WAFER_LOWMAG = 3001,
        PIN_HIGHMAG = 3002,
        PIN_LOWMAG = 3003,
        MOVEZ = 4000,
        MOVEZ_ORIGIN = 4001,
        MOVEZ_SETHOME = 4002,
        MOVEXY_ABS = 5000,
        MOVEZ_ABS = 5001,
        MOVET_ABS = 5002,
        MOVEPZ_ABS = 5003,

        LIGHT_WAFER_HIGH_COAXIAL = 6000,
        LIGHT_WAFER_HIGH_OBLIQUE = 6002,

        LIGHT_PIN_HIGH_COAXIAL = 6001,
        LIGHT_PIN_HIGH_AUX = 6003,

        LIGHT_WAFER_LOW_COAXIAL = 6004,
        LIGHT_WAFER_LOW_OBLIQUE = 6005,

        LIGHT_PIN_LOW_COAXIAL = 6006,
        LIGHT_PIN_LOW_OBLIQUE = 6007,

        MOVEPZ = 7000,
        MOVEPZ_ORIGIN = 7001,
        MOVEPZ_SETPZHOME = 7002,

        SET_FOCUSING_START_POS = 8000,
        SET_WAFER_TYPE = 9000,
        SET_OVERDRIVE = 10000,

        SET_PROBING_OFFSET = 11000,
    }

    [Serializable]
    public class ProbingOffset
    {
        public double TwistZ { get; set; }
        public double SqrForProbe { get; set; }
        public double DeflectX { get; set; }
        public double DeflectY { get; set; }
        public double InclineZ { get; set; }
        public double PMShiftX { get; set; }
        public double PMShiftY { get; set; }
        public double PMShiftZ { get; set; }
        public double PMShiftT { get; set; }
        public double PMTempShiftX { get; set; }
        public double PMTempShiftY { get; set; }
        public double PMTempShiftT { get; set; }

        // 기본 생성자
        public ProbingOffset() { }

        public ProbingOffset(double twistZ, double sqrForProbe, 
            double deflectX, double deflectY, 
            double inclineZ,
            double pmShiftX, double pmShiftY, double pmShiftZ, double pmShiftT,
            double pmTempShiftX, double pmTempShiftY, double pmTempShiftT)
        {
            TwistZ = twistZ;
            SqrForProbe = sqrForProbe;
            DeflectX = deflectX;
            DeflectY = deflectY;
            InclineZ = inclineZ;
            PMShiftX = pmShiftX;
            PMShiftY = pmShiftY;
            PMShiftZ = pmShiftZ;
            PMShiftT = pmShiftT;
            PMTempShiftX = pmTempShiftX;
            PMTempShiftY = pmTempShiftY;
            PMTempShiftT = pmTempShiftT;
        }
    }
}
