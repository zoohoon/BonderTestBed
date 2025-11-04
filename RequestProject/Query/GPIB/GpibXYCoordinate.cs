using ProberInterfaces;
using System;
using System.Text;
using LogModule;

namespace RequestCore.QueryPack.GPIB
{
    public interface IXYCoordinate : IFactoryModule
    {
        string MakeXYCoordinate();
    }

    public class TskXYCoordinate : IXYCoordinate
    {
        public string MakeXYCoordinate()
        {
            string retVal = null;
            try
            {
                string formatStr = null;
                string sXfmt = "0";
                string sYfmt = "0";
                StringBuilder dataSb = new StringBuilder();

                IProbingModule ProbingModule = this.ProbingModule();
                ICoordinateManager CoordinateManager = this.CoordinateManager();
                IStageSupervisor StageSupervisor = this.StageSupervisor();
                MachineIndex probingMI = null;
                UserIndex probingUI = null;
                probingMI = ProbingModule.ProbingLastMIndex;
                probingUI = CoordinateManager.MachineIndexConvertToUserIndex(probingMI);

                long XIndex = probingUI.XIndex;
                long YIndex = probingUI.YIndex;

                if (XIndex < -99) { XIndex = -99; }
                if (YIndex < -99) { YIndex = -99; }

                IGPIB GpibModule = this.GPIB();

                IGPIBSysParam GpibSysParam = this.GPIB().GPIBSysParam_IParam;

                if (GpibSysParam == null)
                {
                    // ERROR
                }

                formatStr = "{0:000;-00;}";

                sXfmt = string.Format(formatStr, XIndex);
                sYfmt = string.Format(formatStr, YIndex);

                dataSb.Append('Y');
                dataSb.Append(sYfmt);
                dataSb.Append('X');
                dataSb.Append(sXfmt);

                retVal = dataSb.ToString();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }




    public class TelXYCoordinate : IXYCoordinate
    {
        public string MakeXYCoordinate()
        {
            string retVal = null;
            try
            {
                string formatStr = null;
                string sXfmt = null;
                string sYfmt = null;
                int expMode = 0;
                StringBuilder dataSb = new StringBuilder();

                IProbingModule ProbingModule = this.ProbingModule();
                ICoordinateManager CoordinateManager = this.CoordinateManager();
                IStageSupervisor StageSupervisor = this.StageSupervisor();
                MachineIndex probingMI = null;
                UserIndex probingUI = null;
                probingMI = ProbingModule.ProbingLastMIndex;
                probingUI = CoordinateManager.MachineIndexConvertToUserIndex(probingMI);

                long XIndex = probingUI.XIndex;
                long YIndex = probingUI.YIndex;

                IGPIB GpibModule = this.GPIB();

                IGPIBSysParam GpibSysParam = this.GPIB().GPIBSysParam_IParam;

                if (GpibSysParam == null)
                {
                    // ERROR
                }

                expMode = this.GPIB().GPIBSysParam_IParam.AddSignForLargeA.Value;

                if (expMode == 1)
                {
                    formatStr = "{0:+000;-000;}";
                }
                else if (expMode == 2)
                {
                    formatStr = "{0:000;-000;}";
                }
                else
                {
                    formatStr = "{0:000;-00;}";
                }

                sXfmt = string.Format(formatStr, XIndex);
                sYfmt = string.Format(formatStr, YIndex);

                dataSb.Append(sXfmt);
                dataSb.Append(sYfmt);
                retVal = dataSb.ToString();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

}
