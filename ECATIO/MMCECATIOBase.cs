using LogModule;
using ProberInterfaces;
using System;
using ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET;

namespace ECATIO
{
    public class MMCECATIOBase : IECATIO
    {
        private ushort _AxisReference;
        public ushort AxisReference
        {
            get { return _AxisReference; }
            set { _AxisReference = value; }
        }
        private MMCECATIO ECATIO;
        public MMCECATIOBase()
        {

        }
        public MMCECATIOBase(string name, int c_hndl)
        {
            try
            {
                ECATIO = new MMCECATIO(name, c_hndl);
                _AxisReference = ECATIO.AxisReference;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void ReadPIVar(ushort index, PIVarDirection direction, VAR_TYPE varType, ref PI_VAR_UNION varUnion)
        {
            try
            {
                ECATIO.ReadPIVar(index, direction, varType, ref varUnion);
                byte a = varUnion._byte;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void WritePIVar(ushort index, PI_VAR_UNION varUnion, VAR_TYPE varType)
        {
            try
            {
                ECATIO.WritePIVar(index, varUnion, varType);
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
