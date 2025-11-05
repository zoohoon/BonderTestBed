using System;

namespace WA_BoundaryParameter_Standard
{
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.AlignEX;
    using System.ComponentModel;

    [Serializable]
    public class WA_BoundaryParam_Standard : AlginParamBase, INotifyPropertyChanged, IParamNode
    {
        public override string FilePath { get; } = "WaferAlignParam\\Standard\\";

        public override string FileName { get; } = "WA_BoundaryParam_Standard.json";

        public WA_BoundaryParam_Standard()
        {

        }
        public override EventCodeEnum InitParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception)
            {
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
                Version = typeof(WA_BoundaryParam_Standard).Assembly.GetName().Version;
                CamType = EnumProberCam.WAFER_HIGH_CAM;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw err;
            }

            return retVal;
        }

    }
}
