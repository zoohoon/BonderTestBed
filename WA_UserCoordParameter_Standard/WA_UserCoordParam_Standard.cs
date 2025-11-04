using System;
using LogModule;

namespace WA_UserCoordParameter_Standard
{
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.AlignEX;
    using System.ComponentModel;

    [Serializable]
    public class WA_UserCoordParam_Standard : AlginParamBase, INotifyPropertyChanged , IParamNode
    {
        public override string FilePath { get; } = "WaferAlignParam\\Standard\\";

        public override string FileName { get; } = "WA_UserCoordParam_Standard.json";


        public WA_UserCoordParam_Standard()
        {

        }

        public override EventCodeEnum InitParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                retval = EventCodeEnum.PARAM_ERROR;
            }
            return retval;
        }
        public override EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public override EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Version = typeof(WA_UserCoordParam_Standard).Assembly.GetName().Version;
                CamType = EnumProberCam.WAFER_LOW_CAM;
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }
    }
}
