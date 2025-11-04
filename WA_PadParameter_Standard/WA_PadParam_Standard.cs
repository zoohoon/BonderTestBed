using System;
using LogModule;

namespace WA_PadParameter_Standard
{
    using Focusing;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.AlignEX;
    using ProberInterfaces.Param;
    using ProberInterfaces.Vision;
    using System.ComponentModel;

    [Serializable]
    public class WA_PadParam_Standard : AlginParamBase, INotifyPropertyChanged ,IParam, IParamNode
    {
        public override string FilePath { get; } = "WaferAlignParam\\Standard\\";

        public override string FileName { get; } = "WA_PadParam_Standard.json";

        private BlobParameter _BlobParam
             = new BlobParameter();
        public BlobParameter BlobParam
        {
            get { return _BlobParam; }
            set
            {
                if (value != _BlobParam)
                {
                    _BlobParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _RefpadRecoveryLimit
             = new Element<double>();
        public Element<double> RefpadRecoveryLimit
        {
            get { return _RefpadRecoveryLimit; }
            set
            {
                if (value != _RefpadRecoveryLimit)
                {
                    _RefpadRecoveryLimit = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ModuleDllInfo _FocusingModuleDllInfo;

        public ModuleDllInfo FocusingModuleDllInfo
        {
            get { return _FocusingModuleDllInfo; }
            set { _FocusingModuleDllInfo = value; }
        }

        private FocusParameter _FocusParam;
        public FocusParameter FocusParam
        {
            get { return _FocusParam; }
            set
            {
                if (value != _FocusParam)
                {
                    _FocusParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        public WA_PadParam_Standard()
        {

        }
        public override EventCodeEnum InitParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (FocusParam == null)
                {
                    FocusParam = new NormalFocusParameter();
                }

                if(this.FocusManager() != null)
                {
                    retval = this.FocusManager().ValidationFocusParam(FocusParam);

                    if (retval != EventCodeEnum.NONE)
                    {
                        this.FocusManager().MakeDefalutFocusParam(EnumProberCam.WAFER_HIGH_CAM, EnumAxisConstants.Z, FocusParam);
                    }
                }

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
                Version = typeof(WA_PadParam_Standard).Assembly.GetName().Version;
                CamType = EnumProberCam.WAFER_HIGH_CAM;


                BlobParam.BlobMinRadius.Value = 3;
                BlobParam.BlobThreshHold.Value = 120;
                BlobParam.MinBlobArea.Value = 50;

                RefpadRecoveryLimit.Value = 5;
                FocusingModuleDllInfo = FocusingDLLInfo.GetNomalFocusingDllInfo();

                retVal = EventCodeEnum.NONE;
                //Pads.PadInfos.Clear();
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }
    }
}
