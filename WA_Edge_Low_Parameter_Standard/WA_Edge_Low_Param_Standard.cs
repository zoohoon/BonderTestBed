using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WA_Edge_Low_Parameter_Standard
{
    using ProberInterfaces.AlignEX;
    using ProberInterfaces.Param;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using ProberInterfaces;
    using System.Xml.Serialization;
    using ProberErrorCode;
    using ProberInterfaces.Vision;
    using ProberInterfaces.WaferAlignEX.ModuleInterface;
    using ParameterUtil;
    using ProberInterfaces.WaferAlignEX.Enum;
    using ProberInterfaces.WaferAlignEX;

    [Serializable()]
    public class WA_Edge_Low_Param_Standard : AlginParamBase, INotifyPropertyChanged, IParamNode
    {
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public override string FilePath { get; } = "WaferAlignParam\\Standard\\";
        public override string FileName { get; } = "WA_Edge_Low_Param_Standard.bin";
        public string PatternbasePath { get; } = "\\WaferAlignParam\\AlignPattern";
        public string PatternName { get; } = "\\WaferAlign_LowPattern";

        #region //..Edge

        //private EnumEdgeMovement _EdgeMovement;
        //public EnumEdgeMovement EdgeMovement
        //{
        //    get { return _EdgeMovement; }
        //    set
        //    {
        //        if (value != _EdgeMovement)
        //        {
        //            _EdgeMovement = value;
        //            NotifyPropertyChanged("EdgeMovement");
        //        }
        //    }
        //}

        private ObservableCollection<LightValueParam> _LightParams;
        public ObservableCollection<LightValueParam> LightParams
        {
            get { return _LightParams; }
            set
            {
                if (value != _LightParams)
                {
                    _LightParams = value;
                    NotifyPropertyChanged("LightParams");
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
                    NotifyPropertyChanged("gIntEdgeDetectProcTolerance");
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
                    NotifyPropertyChanged("gIntEdgeDetectProcToleranceRad");
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
                    NotifyPropertyChanged("gIntEdgeDetectProcToleranceLoadingPos");
                }
            }
        }
        #endregion

        #region //..Low
        private ObservableCollection<WAStandardPTInfomation> _Patterns
    = new ObservableCollection<WAStandardPTInfomation>();
        public ObservableCollection<WAStandardPTInfomation> Patterns
        {
            get { return _Patterns; }
            set
            {
                if (value != _Patterns)
                {
                    _Patterns = value;
                    NotifyPropertyChanged("Patterns");
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
                    NotifyPropertyChanged("AlignMinimumLength");
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
                    NotifyPropertyChanged("AlignOptimumLength");
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
                    NotifyPropertyChanged("AlignMaximumLength");
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
                    NotifyPropertyChanged("DeadZone");
                }
            }
        }


        private ModuleDllInfo _FocusingModuleDllInfo;

        public ModuleDllInfo FocusingModuleDllInfo
        {
            get { return _FocusingModuleDllInfo; }
            set { _FocusingModuleDllInfo = value; }
        }

        #endregion


        public WA_Edge_Low_Param_Standard()
        {

        }

        public override ErrorCodeEnum SetEmulParam()
        {
            ErrorCodeEnum retVal = ErrorCodeEnum.UNDEFINED;
            try
            {
            retVal = SetDefaultParam();
            Patterns = new ObservableCollection<WAStandardPTInfomation>();
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

            Patterns.Add(ptinfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }
        public override ErrorCodeEnum SetDefaultParam()
        {
            ErrorCodeEnum retVal = ErrorCodeEnum.UNDEFINED;
            try
            {
                CamType = EnumProberCam.WAFER_LOW_CAM;
                LightParams = new ObservableCollection<LightValueParam>();
                //EdgeMovement = EnumEdgeMovement.THETA_RETRY;

                gIntEdgeDetectProcTolerance = new Element<double>();
                gIntEdgeDetectProcTolerance.Value = 300;
                gIntEdgeDetectProcTolerance.LowerLimit = 300;

                gIntEdgeDetectProcToleranceRad = new Element<double>();
                gIntEdgeDetectProcToleranceRad.Value = 1000;
                gIntEdgeDetectProcToleranceRad.LowerLimit = 0;
                gIntEdgeDetectProcToleranceRad.UpperLimit = 2000;

                gIntEdgeDetectProcToleranceLoadingPos = new Element<double>();
                gIntEdgeDetectProcToleranceLoadingPos.Value = 2000;

                //AlignLength- 12Inch 기준.
                AlignMinimumLength = 40000; // 4cm
                AlignOptimumLength = 120000; // 12cm
                AlignMaximumLength = 160000; // 16cm

                DeadZone = 10000; //1cm
                FocusingModuleDllInfo = FocusingDLLInfo.GetNomalFocusingDllInfo();
                ParamFullPath = this.FileManager().GetDeviceParamFullPath(FilePath, FileName);
            }
            catch (Exception err)
            {
                throw err;
            }

            return retVal;
        }

    }
}
