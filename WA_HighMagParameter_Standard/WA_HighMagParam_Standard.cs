using System;

namespace WA_HighMagParameter_Standard
{
    using Focusing;
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;

    using ProberInterfaces.AlignEX;
    using ProberInterfaces.Param;
    using ProberInterfaces.Vision;
    using ProberInterfaces.WaferAlignEX;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;

    [Serializable]
    public class WA_HighMagParam_Standard : AlginParamBase, INotifyPropertyChanged
    {
        public override string FilePath { get; } = "WaferAlignParam\\Standard\\";

        public override string FileName { get; } = "WA_HighMagParam_Standard.json";
        public string PatternbasePath { get; } = "\\WaferAlignParam\\AlignPattern\\HighMag";
        public string PatternName { get; } = "\\WaferAlign_HighPattern";

        private FocusParameter _PresetFocusParam;
        public FocusParameter PresetFocusParam
        {
            get { return _PresetFocusParam; }
            set
            {
                if (value != _PresetFocusParam)
                {
                    _PresetFocusParam = value;
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

        private Element<ObservableCollection<WAStandardPTInfomation>> _Patterns
             = new Element<ObservableCollection<WAStandardPTInfomation>>();
        public Element<ObservableCollection<WAStandardPTInfomation>> Patterns
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

        private ObservableCollection<WAHeightPositionParam> _HeightPosParams
  = new ObservableCollection<WAHeightPositionParam>();
        public ObservableCollection<WAHeightPositionParam> HeightPosParams
        {
            get { return _HeightPosParams; }
            set
            {
                if (value != _HeightPosParams)
                {
                    _HeightPosParams = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<HeightPointEnum> _HeightProfilingPointType
             = new Element<HeightPointEnum>();
        public Element<HeightPointEnum> HeightProfilingPointType
        {
            get { return _HeightProfilingPointType; }
            set
            {
                if (value != _HeightProfilingPointType)
                {
                    _HeightProfilingPointType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<High_ProcessingPointEnum> _High_ProcessingPoint
             = new Element<High_ProcessingPointEnum>(High_ProcessingPointEnum.HIGH_7PT);
        public Element<High_ProcessingPointEnum> High_ProcessingPoint
        {
            get { return _High_ProcessingPoint; }
            set
            {
                if (value != _High_ProcessingPoint)
                {
                    _High_ProcessingPoint = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _AcceptAlignMimumLength;
        /// <summary>
        /// PostAlignMiniumLength
        /// </summary>
        public double AcceptAlignMimumLength
        {
            get { return _AcceptAlignMimumLength; }
            set
            {
                if (value != _AcceptAlignMimumLength)
                {
                    _AcceptAlignMimumLength = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _AcceptAlignOptimumLength;
        public double AcceptAlignOptimumLength
        {
            get { return _AcceptAlignOptimumLength; }
            set
            {
                if (value != _AcceptAlignOptimumLength)
                {
                    _AcceptAlignOptimumLength = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _AcceptAlignMaximumLength;
        public double AcceptAlignMaximumLength
        {
            get { return _AcceptAlignMaximumLength; }
            set
            {
                if (value != _AcceptAlignMaximumLength)
                {
                    _AcceptAlignMaximumLength = value;
                    RaisePropertyChanged();
                }
            }
        }


        private double _AlignMinimumLength;
        public double AlignMinimumLength
        {
            get { return _AlignMinimumLength; }
            set
            {
                if (value != _AlignMinimumLength)
                {
                    _AlignMinimumLength = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _AlignOptimumLength;
        public double AlignOptimumLength
        {
            get { return _AlignOptimumLength; }
            set
            {
                if (value != _AlignOptimumLength)
                {
                    _AlignOptimumLength = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _AlignMaximumLength;
        public double AlignMaximumLength
        {
            get { return _AlignMaximumLength; }
            set
            {
                if (value != _AlignMaximumLength)
                {
                    _AlignMaximumLength = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _DeadZone;
        public double DeadZone
        {
            get { return _DeadZone; }
            set
            {
                if (value != _DeadZone)
                {
                    _DeadZone = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _WaferPlanarityLimit
             = new Element<double>() { Value = 250 };
        public Element<double> WaferPlanarityLimit
        {
            get { return _WaferPlanarityLimit; }
            set
            {
                if (value != _WaferPlanarityLimit)
                {
                    _WaferPlanarityLimit = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<double> _LimitValueX
     = new Element<double>();
        /// <summary>
        /// Verify Align의 X Tolerance 
        /// </summary>
        public Element<double> LimitValueX
        {
            get { return _LimitValueX; }
            set
            {
                if (value != _LimitValueX)
                {
                    _LimitValueX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _LimitValueY
            = new Element<double>();
        /// <summary>
        /// Verify Align의 Y Tolerance 
        /// </summary>
        public Element<double> LimitValueY
        {
            get { return _LimitValueY; }
            set
            {
                if (value != _LimitValueY)
                {
                    _LimitValueY = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// 해당 변수의 단위는 '(각도)        /// </summary>
        /// Lowlimit 0.00005
        /// Upperlimit 3um 기준 * 10 = 30um 오차 허용 범위
        private Element<double> _VerifyLimitAngle
            = new Element<double>() { Value = 0.003438, LowerLimit = 0.00005, UpperLimit = 0.01146};
        public Element<double> VerifyLimitAngle
        {
            get { return _VerifyLimitAngle; }
            set
            {
                if (value != _VerifyLimitAngle)
                {
                    _VerifyLimitAngle = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PMParameter _DefaultPMParam
             = new PMParameter();
        public PMParameter DefaultPMParam
        {
            get { return _DefaultPMParam; }
            set
            {
                if (value != _DefaultPMParam)
                {
                    _DefaultPMParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private WA_HighStandard_JumpIndex_ManualParam _JumpIndexManualInputParam
            = new WA_HighStandard_JumpIndex_ManualParam();
        public WA_HighStandard_JumpIndex_ManualParam JumpIndexManualInputParam
        {
            get { return _JumpIndexManualInputParam; }
            set
            {
                if (value != _JumpIndexManualInputParam)
                {
                    _JumpIndexManualInputParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private WA_HighStandard_FocusingROIWithPatternSizeParam _FocusingROIWithPatternSize
            = new WA_HighStandard_FocusingROIWithPatternSizeParam();
        public WA_HighStandard_FocusingROIWithPatternSizeParam FocusingROIWithPatternSize
        {
            get { return _FocusingROIWithPatternSize; }
            set
            {
                if (value != _FocusingROIWithPatternSize)
                {
                    _FocusingROIWithPatternSize = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _EnablePostJumpindex = true;

        public bool EnablePostJumpindex
        {
            get { return _EnablePostJumpindex; }
            set { _EnablePostJumpindex = value; }
        }


        private ModuleDllInfo _FocusingModuleDllInfo;

        public ModuleDllInfo FocusingModuleDllInfo
        {
            get { return _FocusingModuleDllInfo; }
            set { _FocusingModuleDllInfo = value; }
        }

        private ModuleDllInfo _PresetFocusingModuleDllInfo;

        public ModuleDllInfo PresetFocusingModuleDllInfo
        {
            get { return _PresetFocusingModuleDllInfo; }
            set { _PresetFocusingModuleDllInfo = value; }
        }

        public WA_HighMagParam_Standard()
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

                if (this.FocusManager() != null)
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
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = SetDefaultParam();
                SetLotDefaultParam();
                retVal = EventCodeEnum.NONE;
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
                Version = typeof(WA_HighMagParam_Standard).Assembly.GetName().Version;
                CamType = EnumProberCam.WAFER_HIGH_CAM;
                Acquistion = "HighMagAlign";
                //Acquistion = new WaferAlignAcqStep(new HighMagAlign());
                //AlignLength - 12Inch 기준.

                AcceptAlignMimumLength = 40000; // 4cm
                AcceptAlignOptimumLength = 120000; // 12cm
                AcceptAlignMaximumLength = 160000; // 16cm

                AlignMinimumLength = 200000; // 20cm
                AlignOptimumLength = 240000; // 24cm
                AlignMaximumLength = 280000; // 28cm

                DeadZone = 10000;//1cm

                FocusingModuleDllInfo = FocusingDLLInfo.GetNomalFocusingDllInfo();

                if (FocusParam == null)
                {
                    FocusParam = new NormalFocusParameter();
                }

                FocusParam.FocusingCam.Value = EnumProberCam.WAFER_HIGH_CAM;
                FocusParam.DepthOfField.Value = 1;
                FocusParam.OutFocusLimit.Value = 40;
                FocusParam.FocusRange.Value = 300;
                FocusParam.FocusingAxis.Value = EnumAxisConstants.Z;

                HeightProfilingPointType.Value = HeightPointEnum.POINT5;
                Patterns.Value = new ObservableCollection<WAStandardPTInfomation>();

                string ptpath
                     = this.FileManager().FileManagerParam.DeviceParamRootDirectory +
                        "\\" + this.FileManager().FileManagerParam.DeviceName + PatternbasePath + @"\";

                if (Directory.Exists(Path.GetDirectoryName(ptpath)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(ptpath));
                }

                LimitValueX.Value = 200;
                LimitValueY.Value = 200;
                VerifyLimitAngle.Value = 0.003438;

                DefaultPMParam.PMAcceptance.Value = 95;
                DefaultPMParam.PMCertainty.Value = 100;

                FocusingROIWithPatternSize.RetryFocusingROIMargin_X.Value = 40;
                FocusingROIWithPatternSize.RetryFocusingROIMargin_Y.Value = 40;
                High_ProcessingPoint.Value = High_ProcessingPointEnum.HIGH_7PT;

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw err;
            }

            return retVal;
        }

        private void SetLotDefaultParam()
        {
            try
            {

                WAStandardPTInfomation ptinfo
                     = new WAStandardPTInfomation();
                ptinfo.CamType.Value = EnumProberCam.WAFER_HIGH_CAM;
                //ptinfo.FocusingModel = new NormalFocusing();
                ptinfo.X.Value = 100;
                ptinfo.Y.Value = 100;
                ptinfo.Z.Value = 7;
                ptinfo.PMParameter = new ProberInterfaces.Vision.PMParameter();

                ptinfo.PMParameter.ModelFilePath.Value = this.FileManager().FileManagerParam.DeviceParamRootDirectory +
                "\\" + this.FileManager().FileManagerParam.DeviceName + PatternbasePath + PatternName + "0";
                ptinfo.PMParameter.PatternFileExtension.Value = ".mmo";
                ptinfo.MIndex = new MachineIndex(20, 20);
                ptinfo.JumpIndexs = new ObservableCollection<StandardJumpIndexParam>();
                ptinfo.PostJumpIndex = new ObservableCollection<StandardJumpIndexParam>();
                ptinfo.PostJumpIndex.Add(new StandardJumpIndexParam(0, 0, true));
                ptinfo.PostJumpIndex.Add(new StandardJumpIndexParam(-9, 0, false));
                ptinfo.PostJumpIndex.Add(new StandardJumpIndexParam(9, 0, false));
                ptinfo.JumpIndexs.Add(new StandardJumpIndexParam(13, 0, true));
                ptinfo.JumpIndexs.Add(new StandardJumpIndexParam(-13, 0, true));
                ptinfo.JumpIndexs.Add(new StandardJumpIndexParam(0, 13, true));
                ptinfo.JumpIndexs.Add(new StandardJumpIndexParam(0, -13, true));
                ptinfo.LightParams = new ObservableCollection<LightValueParam>();
                ptinfo.ProcDirection.Value = EnumWAProcDirection.BIDIRECTIONAL;

                ptinfo.WaferCenter = new WaferCoordinate(100, 100, 0);

                ptinfo.AcceptFocusing.Value = false;
                Patterns.Value.Add(ptinfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void CopyTo(WA_HighMagParam_Standard target)
        {
            try
            {
                foreach (var pattern in Patterns.Value)
                {
                    target.Patterns.Value.Add(new WAStandardPTInfomation());
                    pattern.CopyTo(target.Patterns.Value[target.Patterns.Value.Count]);
                }

                DefaultPMParam.CopyTo(target.DefaultPMParam);
                JumpIndexManualInputParam.CopyTo(target.JumpIndexManualInputParam);
                target.HeightProfilingPointType.Value = this.HeightProfilingPointType.Value;
                target.LimitValueX.Value = this.LimitValueX.Value;
                target.LimitValueY.Value = this.LimitValueY.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

    [Serializable]
    public class WA_HighStandard_JumpIndex_ManualParam : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private long _Left1Index;
        public long Left1Index
        {
            get { return _Left1Index; }
            set
            {
                if (value != _Left1Index)
                {
                    _Left1Index = value;
                    NotifyPropertyChanged("Left1Index");
                }
            }
        }


        private long _Left2Index;
        public long Left2Index
        {
            get { return _Left2Index; }
            set
            {
                if (value != _Left2Index)
                {
                    _Left2Index = value;
                    NotifyPropertyChanged("Left2Index");
                }
            }
        }


        private long _Right1Index;
        public long Right1Index
        {
            get { return _Right1Index; }
            set
            {
                if (value != _Right1Index)
                {
                    _Right1Index = value;
                    NotifyPropertyChanged("Right1Index");
                }
            }
        }



        private long _Right2Index;
        public long Right2Index
        {
            get { return _Right2Index; }
            set
            {
                if (value != _Right2Index)
                {
                    _Right2Index = value;
                    NotifyPropertyChanged("Right2Index");
                }
            }
        }

        private long _UpperIndex;
        public long UpperIndex
        {
            get { return _UpperIndex; }
            set
            {
                if (value != _UpperIndex)
                {
                    _UpperIndex = value;
                    NotifyPropertyChanged("UpperIndex");
                }
            }
        }


        private long _BottomIndex;
        public long BottomIndex
        {
            get { return _BottomIndex; }
            set
            {
                if (value != _BottomIndex)
                {
                    _BottomIndex = value;
                    NotifyPropertyChanged("BottomIndex");
                }
            }
        }



        public WA_HighStandard_JumpIndex_ManualParam()
        {

        }

        public void CopyTo(WA_HighStandard_JumpIndex_ManualParam target)
        {
            try
            {
                target.Left1Index = this.Left1Index;
                target.Left2Index = this.Left2Index;
                target.Right1Index = this.Right1Index;
                target.Right2Index = this.Right2Index;
                target.UpperIndex = this.UpperIndex;
                target.BottomIndex = this.BottomIndex;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


    }

    [Serializable]
    public class WA_HighStandard_FocusingROIWithPatternSizeParam : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private Element<double> _RetryFocusingROIMargin_X = new Element<double>() { Value = 40 };
        public Element<double> RetryFocusingROIMargin_X
        {
            get { return _RetryFocusingROIMargin_X; }
            set
            {
                if (value != _RetryFocusingROIMargin_X)
                {
                    _RetryFocusingROIMargin_X = value;
                    NotifyPropertyChanged("RetryFocusingROIMargin_X");
                }
            }
        }

        private Element<double> _RetryFocusingROIMargin_Y = new Element<double>() { Value = 40 };
        public Element<double> RetryFocusingROIMargin_Y
        {
            get { return _RetryFocusingROIMargin_Y; }
            set
            {
                if (value != _RetryFocusingROIMargin_Y)
                {
                    _RetryFocusingROIMargin_Y = value;
                    NotifyPropertyChanged("RetryFocusingROIMargin_Y");
                }
            }
        }

        public WA_HighStandard_FocusingROIWithPatternSizeParam()
        {

        }

        public void CopyTo(WA_HighStandard_FocusingROIWithPatternSizeParam target)
        {
            try
            {
                target.RetryFocusingROIMargin_X.Value = this.RetryFocusingROIMargin_X.Value;
                target.RetryFocusingROIMargin_Y.Value = this.RetryFocusingROIMargin_Y.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
