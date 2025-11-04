using System;
using LogModule;

namespace WA_EdgeParameter_Standard
{
    using ProberInterfaces.AlignEX;
    using ProberInterfaces.Param;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using ProberInterfaces;
    using ProberErrorCode;
    using ProberInterfaces.WaferAlignEX.Enum;
    using Focusing;

    [Serializable]
    public class WA_EdgeParam_Standard : AlginParamBase, INotifyPropertyChanged, IParamNode
    {

        public override string FilePath { get; } = "WaferAlignParam\\Standard\\";
        public override string FileName { get; } = "WA_EdgeParam_Standard.json";

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


        private Element<EnumWASubModuleEnable> _EdgeMovement;
        public Element<EnumWASubModuleEnable> EdgeMovement
        {
            get { return _EdgeMovement; }
            set
            {
                if (value != _EdgeMovement)
                {
                    _EdgeMovement = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ushort _DefaultLightValue;
        public ushort DefaultLightValue
        {
            get { return _DefaultLightValue; }
            set
            {
                if (value != _DefaultLightValue)
                {
                    _DefaultLightValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<LightValueParam> _LightParams;
        [SharePropPath]
        public ObservableCollection<LightValueParam> LightParams
        {
            get { return _LightParams; }
            set
            {
                if (value != _LightParams)
                {
                    _LightParams = value;
                    RaisePropertyChanged();
                }
            }
        }




        private Element<double> _gIntEdgeDetectProcTolerance;
        public Element<double> gIntEdgeDetectProcTolerance
        {
            get { return _gIntEdgeDetectProcTolerance; }
            set
            {
                if (value != _gIntEdgeDetectProcTolerance)
                {
                    _gIntEdgeDetectProcTolerance = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _gIntEdgeDetectProcToleranceRad;
        public Element<double> gIntEdgeDetectProcToleranceRad
        {
            get { return _gIntEdgeDetectProcToleranceRad; }
            set
            {
                if (value != _gIntEdgeDetectProcToleranceRad)
                {
                    _gIntEdgeDetectProcToleranceRad = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _gIntEdgeDetectProcToleranceLoadingPos;
        public Element<double> gIntEdgeDetectProcToleranceLoadingPos
        {
            get { return _gIntEdgeDetectProcToleranceLoadingPos; }
            set
            {
                if (value != _gIntEdgeDetectProcToleranceLoadingPos)
                {
                    _gIntEdgeDetectProcToleranceLoadingPos = value;
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


        public WA_EdgeParam_Standard()
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
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = SetDefaultParam();
                gIntEdgeDetectProcTolerance.SetNodeSetupStateSetuped();
                gIntEdgeDetectProcToleranceRad.SetNodeSetupStateSetuped();
                gIntEdgeDetectProcToleranceLoadingPos.SetNodeSetupStateSetuped();
            }
            catch (Exception err)
            {
                throw new Exception($"Error during Setting Default Param From {this.GetType().Name}. {err.Message}");
            }
            return retVal;
        }

        public override EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //Version = 1000;
                Version = typeof(WA_EdgeParam_Standard).Assembly.GetName().Version;
                DefaultLightValue = 200;
                CamType = EnumProberCam.WAFER_LOW_CAM;
                LightParams = new ObservableCollection<LightValueParam>();
                EdgeMovement = new Element<EnumWASubModuleEnable>();
                EdgeMovement.Value = EnumWASubModuleEnable.THETA_RETRY;
                Acquistion = "WaferCenter";

                gIntEdgeDetectProcTolerance = new Element<double>();
                gIntEdgeDetectProcTolerance.Value = 300;
                gIntEdgeDetectProcTolerance.LowerLimit = 300;

                gIntEdgeDetectProcToleranceRad = new Element<double>();
                gIntEdgeDetectProcToleranceRad.Value = 1000;
                gIntEdgeDetectProcToleranceRad.LowerLimit = 0;
                gIntEdgeDetectProcToleranceRad.UpperLimit = 2000;

                gIntEdgeDetectProcToleranceLoadingPos = new Element<double>();
                gIntEdgeDetectProcToleranceLoadingPos.Value = 2000;

                if(FocusingModuleDllInfo == null)
                {
                    FocusingModuleDllInfo = FocusingDLLInfo.GetNomalFocusingDllInfo();
                }

                if(FocusParam == null)
                {
                    FocusParam = new NormalFocusParameter();
                }

                FocusParam.FocusingCam.Value = EnumProberCam.WAFER_LOW_CAM;

                //SetLotEmulParam();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw new Exception($"Error during Setting Default Param From {this.GetType().Name}. {err.Message}");
            }

            return retVal;
        }

        public void SetLotEmulParam()
        {
            try
            {
                gIntEdgeDetectProcTolerance.SetNodeSetupStateSetuped();
                gIntEdgeDetectProcToleranceRad.SetNodeSetupStateSetuped();
                gIntEdgeDetectProcToleranceLoadingPos.SetNodeSetupStateSetuped();
            }
            catch (Exception err)
            {
                throw new Exception($"Error during SetLotEmulParam {this.GetType().Name}. {err.Message}");
            }
        }

        public override EventCodeEnum Init()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (FocusingModuleDllInfo == null)
                {
                    FocusingModuleDllInfo = FocusingDLLInfo.GetNomalFocusingDllInfo();


                    // ann todo -> OutFocusLimit ¡÷ºÆ
                    FocusParam.FocusingCam.Value = EnumProberCam.WAFER_HIGH_CAM;
                    FocusParam.DepthOfField.Value = 1;
                    //FocusParam.OutFocusLimit.Value = 40;
                    FocusParam.FocusRange.Value = 300;
                    FocusParam.FocusingAxis.Value = EnumAxisConstants.Z;

                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            return retVal;
        }

    }
}
