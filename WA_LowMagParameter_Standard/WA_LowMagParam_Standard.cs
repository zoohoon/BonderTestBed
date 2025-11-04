using System;
using LogModule;

namespace WA_LowMagParameter_Standard
{
    using Focusing;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.AlignEX;
    using ProberInterfaces.Param;
    using ProberInterfaces.Vision;
    using ProberInterfaces.WaferAlignEX;
    using ProberInterfaces.WaferAlignEX.Enum;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Xml.Serialization;
    using WAProcBaseParam;

    [Serializable]
    public class WA_LowMagParam_Standard : AlginParamBase, INotifyPropertyChanged, IParamNode
    {
        [XmlIgnore, JsonIgnore]
        public override string FilePath { get; } = "WaferAlignParam\\Standard\\";
        [XmlIgnore, JsonIgnore]
        public override string FileName { get; } = "WA_LowMagParam_Standard.json";
        public string PatternbasePath { get; } = "\\WaferAlignParam\\AlignPattern\\LowMag";
        public string PatternName { get; } = "\\WaferAlign_LowPattern";

        private Element<EnumWASubModuleEnable> _LowMagMovement
             = new Element<EnumWASubModuleEnable>();
        public Element<EnumWASubModuleEnable> LowMagMovement
        {
            get { return _LowMagMovement; }
            set
            {
                if (value != _LowMagMovement)
                {
                    _LowMagMovement = value;
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

        private JumpIndexStandardParam _JumpIndexManualInputParam
             = new JumpIndexStandardParam();
        public JumpIndexStandardParam JumpIndexManualInputParam
        {
            get { return _JumpIndexManualInputParam; }
            set
            {
                if (value != _JumpIndexManualInputParam)
                {
                    _JumpIndexManualInputParam = value;
                    

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

        private Element<Low_ProcessingPointEnum> _Low_ProcessingPoint
             = new Element<Low_ProcessingPointEnum>(Low_ProcessingPointEnum.LOW_3PT);
        public Element<Low_ProcessingPointEnum> Low_ProcessingPoint
        {
            get { return _Low_ProcessingPoint; }
            set
            {
                if (value != _Low_ProcessingPoint)
                {
                    _Low_ProcessingPoint = value;
                    RaisePropertyChanged();
                }
            }
        }

        public override EventCodeEnum InitParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if(FocusParam == null)
                {
                    FocusParam = new NormalFocusParameter();
                }

                if(this.FocusManager() != null)
                {
                    retval = this.FocusManager().ValidationFocusParam(FocusParam);

                    if (retval != EventCodeEnum.NONE)
                    {
                        this.FocusManager().MakeDefalutFocusParam(EnumProberCam.WAFER_LOW_CAM, EnumAxisConstants.Z, FocusParam);
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

        public WA_LowMagParam_Standard()
        {

        }
        public override EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                SetDefaultParam();
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
                Version = typeof(WA_LowMagParam_Standard).Assembly.GetName().Version;
                CamType = EnumProberCam.WAFER_LOW_CAM;
                //Acquistion = new WaferAlignAcqStep(new LowMagAlign());
                Acquistion = "LowMagAlign";


                //AlignLength- 12Inch 기준.
                AlignMinimumLength = 40000; // 4cm
                AlignOptimumLength = 120000; // 12cm
                AlignMaximumLength = 160000; // 16cm

                DeadZone = 10000; //1cm
                FocusingModuleDllInfo = FocusingDLLInfo.GetNomalFocusingDllInfo();

                if(FocusParam == null)
                {
                    FocusParam = new NormalFocusParameter();
                }

                FocusParam.FocusingCam.Value = EnumProberCam.WAFER_LOW_CAM;
                FocusParam.FocusingAxis.Value = EnumAxisConstants.Z;

                Patterns.Value = new ObservableCollection<WAStandardPTInfomation>();
                string ptpath
                     = this.FileManager().FileManagerParam.DeviceParamRootDirectory +
                        "\\" + this.FileManager().FileManagerParam.DeviceName + PatternbasePath + @"\";

                if (Directory.Exists(Path.GetDirectoryName(ptpath)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(ptpath));
                }

                DefaultPMParam.PMAcceptance.Value = 90;
                DefaultPMParam.PMCertainty.Value = 100;
                Low_ProcessingPoint.Value = Low_ProcessingPointEnum.LOW_3PT;

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
                ptinfo.CamType.Value = EnumProberCam.WAFER_LOW_CAM;
                ptinfo.X.Value = 100;
                ptinfo.Y.Value = 100;
                ptinfo.Z.Value = 700;
                ptinfo.PMParameter = new ProberInterfaces.Vision.PMParameter();
                ptinfo.PMParameter.ModelFilePath.Value = this.FileManager().FileManagerParam.DeviceParamRootDirectory +
                            "\\" + this.FileManager().FileManagerParam.DeviceName + PatternbasePath + PatternName + "0";
                ptinfo.PMParameter.PatternFileExtension.Value = ".mmo";
                ptinfo.MIndex = new MachineIndex(20, 20);
                ptinfo.JumpIndexs = new ObservableCollection<StandardJumpIndexParam>();
                ptinfo.JumpIndexs.Add(new StandardJumpIndexParam(0, 0, false));
                ptinfo.JumpIndexs.Add(new StandardJumpIndexParam(-9, 0, false));
                ptinfo.JumpIndexs.Add(new StandardJumpIndexParam(9, 0, false));
                ptinfo.LightParams = new ObservableCollection<LightValueParam>();
                ptinfo.ProcDirection.Value = EnumWAProcDirection.HORIZONTAL;

                Patterns.Value.Add(ptinfo);

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

        public void CopyTo(WA_LowMagParam_Standard target)
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
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }
}
