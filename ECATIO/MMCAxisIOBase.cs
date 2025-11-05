using LogModule;
using ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET;
using ProberInterfaces;
using System;

namespace ECATIO
{

    public class MMCAxisIOBase : IECATIO
    {
        private MMCSingleAxis AxisIO;

        private ushort _AxisReference;
        public ushort AxisReference
        {
            get { return _AxisReference; }
            set { _AxisReference = value; }
        }

        int DIGITAL_OUTPUT = 16;
        public MMCAxisIOBase()
        {

        }
        public MMCAxisIOBase(string name, int c_hndl)
        {
            try
            {
                AxisIO = new MMCSingleAxis(name, c_hndl);
                _AxisReference = AxisIO.AxisReference;
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
                int totalIndex = DIGITAL_OUTPUT + index;
                uint state = AxisIO.GetDigOutputs32bit(totalIndex);
                if (((state >> totalIndex) >> index & 0x01) == 0x01)
                {
                    varUnion._byte = 1;
                }
                else
                {
                    varUnion._byte = 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void WritePIVar(ushort index, PI_VAR_UNION varUnion, VAR_TYPE varType)
        {
            uint nReadDigitalOutput = 0;
            int totalIndex = DIGITAL_OUTPUT + index;
            try
            {

                nReadDigitalOutput = AxisIO.GetDigOutputs32bit(0);

                if (((nReadDigitalOutput >> totalIndex) & 0x01) == 0x00)
                    nReadDigitalOutput += (uint)(0X01 << totalIndex);
                else
                    nReadDigitalOutput -= (uint)(0X01 << totalIndex);

                AxisIO.SetDigOutputs32Bit(0, nReadDigitalOutput);

                uint state = AxisIO.GetDigOutputs32bit(totalIndex);
                byte bitState = 0;
                if (((state >> totalIndex) >> index & 0x01) == 0x01)
                {
                    bitState = 1;
                }
                else
                {
                    bitState = 0;
                }
                if (varUnion.s_byte != bitState)
                {
                    // Exception
                }
                //nReadDigitalOutput = varUnion._byte;

                //if (((nReadDigitalOutput >> ConstantsList.DIGITAL_OUTPUT_1) & 0x01) == 0x00)
                //    nReadDigitalOutput += (0X01 << ConstantsList.DIGITAL_OUTPUT_1);
                //else
                //    nReadDigitalOutput -= (0X01 << ConstantsList.DIGITAL_OUTPUT_1);

                //AxisIO.SetDigOutputs32Bit(index, nReadDigitalOutput);
            }
            catch (MMCException ex)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, ex);
            }
        }
    }
}
