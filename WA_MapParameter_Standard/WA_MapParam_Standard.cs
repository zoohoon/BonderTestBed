using System;
using LogModule;

namespace WA_MapParameter_Standard
{
    using Focusing;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.AlignEX;
    using ProberInterfaces.Param;
    using ProberInterfaces.Vision;
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    [Serializable]
    public class WA_MapParam_Standard : AlginParamBase, INotifyPropertyChanged, IParamNode
    {
        public override string FilePath { get; } = "WaferAlignParam\\Standard\\";

        public override string FileName { get; } = "WA_MapParam_Standard.json";
        public string PatternPath { get; } = "WaferAlignParam\\AlignPattern\\WaferMarginPT\\";


        private ObservableCollection<PatternInfomation> _Patterns;
        public ObservableCollection<PatternInfomation> Patterns
        {
            get { return _Patterns; }
            set
            {
                if (value != _Patterns)
                {
                    _Patterns = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private Element<double> _PatternROIMargin
        //     = new Element<double>();
        //public Element<double> PatternROIMargin
        //{
        //    get { return _PatternROIMargin; }
        //    set
        //    {
        //        if (value != _PatternROIMargin)
        //        {
        //            _PatternROIMargin = value;
        //            NotifyPropertyChanged("PatternROIMargin");
        //        }
        //    }
        //}

        private PMParameter _PMParam;
        public PMParameter PMParam
        {
            get { return _PMParam; }
            set
            {
                if (value != _PMParam)
                {
                    _PMParam = value;
                    RaisePropertyChanged();
                }
            }
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

        private ModuleDllInfo _FocusingModuleDllInfo;

        public ModuleDllInfo FocusingModuleDllInfo
        {
            get { return _FocusingModuleDllInfo; }
            set { _FocusingModuleDllInfo = value; }
        }


        public WA_MapParam_Standard()
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
                Version = typeof(WA_MapParam_Standard).Assembly.GetName().Version;
                CamType = EnumProberCam.WAFER_LOW_CAM;
                //FocusParam = new WaferFocusParameter();
                Patterns = new ObservableCollection<PatternInfomation>();
                PMParam = new PMParameter(65, 80);
                //PatternROIMargin.Value = 10;
                FocusingModuleDllInfo = FocusingDLLInfo.GetNomalFocusingDllInfo();

                if(FocusParam == null)
                {
                    FocusParam = new NormalFocusParameter();
                }

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
