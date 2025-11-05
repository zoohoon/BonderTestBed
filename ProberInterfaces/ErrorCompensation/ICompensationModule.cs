using LogModule;
using System;
using System.Collections.ObjectModel;
using ProberErrorCode;

namespace ProberInterfaces
{


    public enum FlgCOMP_MODE
    {
        COMP_MODE_INVALID = -1,

        COMP_MODE_X_LIN = 0,
        COMP_MODE_X_ANG = 1,
        COMP_MODE_X_STR = 2,
        COMP_MODE_Y_LIN = 3,
        COMP_MODE_Y_ANG = 4,
        COMP_MODE_Y_STR = 5,
        COMP_MODE_Y_ZA = 6,
        COMP_MODE_Y_ZH = 7,

        COMP_MODE_LAST = COMP_MODE_Y_STR + 1,
    }

    public interface ICompensationModule: IFactoryModule, IModule, IHasSysParameterizable
    {
        bool Enable1D { get; set; }
        bool Enable2D { get; set; }
        //EventCodeEnum LoadErrorTable(string Filepath);
        bool EnableLinearComp { get; set; }
        bool EnableStraightnessComp { get; set; }
        bool EnableAngularComp { get; set; }
        double m_sqr_angle { set; get; }
        int GetErrorComp(ObservableCollection<ProbeAxisObject> ProbeAxes);
        Object GetErrorTable();
        CompensationValue GetErrorComp(CompensationPos Pos);

        EventCodeEnum ForcedLoadFirstErrorTable();

    }

    public class CompensationValue
    {
        public double XValue;
        public double YValue;
        public double ZValue;
        public double CValue;
        public double RValue;
        public double TTValue;
        public double PZValue;

        public void Clear()
        {
            try
            {
            this.XValue = 0;
            this.YValue = 0;
            this.ZValue = 0;
            this.CValue = 0;
            this.RValue = 0;
            this.TTValue = 0;
            this.PZValue = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

    }

    public class CompensationPos
    {

        public double XPos;
        public double YPos;
        public double ZPos;
        public double CPos;
        public double RPos;
        public double TTPos;
        public double PZPos;
        public double CTPos;
        public double CCMPos;
        public double CCSPos;
        public double CCGPos;
    }

}
