using System;
using LogModule;

namespace WA_IndexAlignParameter_Edge
{
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.AlignEX;
    using ProberInterfaces.Param;
    using ProberInterfaces.WaferAlignEX.Enum;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Xml.Serialization;

    [Serializable]
    public class WA_IndexAlignParam_Edge : AlginParamBase, INotifyPropertyChanged, IParamNode
    {

        [XmlIgnore, JsonIgnore]
        public override string FilePath { get; } = "WaferAlignParam\\Standard\\";
        [XmlIgnore, JsonIgnore]
        public override string FileName { get; } = "WA_IndexAlignParam_Edge.json";


        private Element<ObservableCollection<EdgeIndexAlignParam>> _EdgeParams
             = new Element<ObservableCollection<EdgeIndexAlignParam>>();
        public Element<ObservableCollection<EdgeIndexAlignParam>> EdgeParams
        {
            get { return _EdgeParams; }
            set
            {
                if (value != _EdgeParams)
                {
                    _EdgeParams = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EnumWASubModuleEnable _AlignEnable;
        public EnumWASubModuleEnable AlignEnable
        {
            get { return _AlignEnable; }
            set
            {
                if (value != _AlignEnable)
                {
                    _AlignEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _AlignThreshold = new Element<int>() { Value = 20 };
        public Element<int> AlignThreshold
        {
            get { return _AlignThreshold; }
            set
            {
                if (value != _AlignThreshold)
                {
                    _AlignThreshold = value;
                    RaisePropertyChanged();
                }
            }
        }

        // 40 마이크론 정도 오차가 있을 수 있다는 가정으로 탄생
        // 디폴트 값 : Wafer Low Ratio = 5.32, 40/5.32 = 7.5187, 반올림하여 8로 사용
        // 정확한 위치가 계산되지 않는 이슈가 있어 디폴트 값을 64로 키워놓음.
        private Element<int> _AllowableRange = new Element<int>() { Value = 64 };
        public Element<int> AllowableRange
        {
            get { return _AllowableRange; }
            set
            {
                if (value != _AllowableRange)
                {
                    _AllowableRange = value;
                    RaisePropertyChanged();
                }
            }
        }

        [XmlIgnore, JsonIgnore]
        public Element<int> _PositionToleranceWCtoTargetdie = new Element<int>();
        public Element<int> PositionToleranceWCtoTargetdie
        {
            get { return _PositionToleranceWCtoTargetdie; }
            set
            {
                _PositionToleranceWCtoTargetdie = value;
                RaisePropertyChanged("PositionToleranceWCtoTargetdie");
            }
        }

        public Element<long> _CenMregist_X = new Element<long>();
        public Element<long> CenMregist_X
        {
            get { return _CenMregist_X; }
            set
            {
                _CenMregist_X = value;
                RaisePropertyChanged("CenMregist_X");
            }
        }

        public Element<long> _CenMregist_YY = new Element<long>();
        public Element<long> CenMregist_Y
        {
            get { return _CenMregist_YY; }
            set
            {
                _CenMregist_YY = value;
                RaisePropertyChanged("CenMregist_Y");
            }
        }

        public Element<double> _WaferCentertoTargetX = new Element<double>();
        public Element<double> WaferCentertoTargetX
        {
            get { return _WaferCentertoTargetX; }
            set
            {
                _WaferCentertoTargetX = value;
                RaisePropertyChanged("WaferCentertoTargetX");
            }
        }
        public Element<double> _WaferCentertoTargetY = new Element<double>();
        public Element<double> WaferCentertoTargetY
        {
            get { return _WaferCentertoTargetY; }
            set
            {
                _WaferCentertoTargetY = value;
                RaisePropertyChanged("WaferCentertoTargetY");
            }
        }


        private ObservableCollection<LightValueParam> _LightParams;
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


        public WA_IndexAlignParam_Edge()
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
        public WA_IndexAlignParam_Edge(WA_IndexAlignParam_Edge param)
        {
            try
            {
                AlignEnable = param.AlignEnable;
                AllowableRange = param.AllowableRange;
                AlignThreshold = param.AlignThreshold;                
                PositionToleranceWCtoTargetdie = param.PositionToleranceWCtoTargetdie;
                WaferCentertoTargetX = param.WaferCentertoTargetX;
                WaferCentertoTargetY = param.WaferCentertoTargetY;
                CenMregist_X = param.CenMregist_X;
                CenMregist_Y = param.CenMregist_Y;

                if (param.LightParams != null)
                {
                    foreach (var lparam in param.LightParams)
                    {
                        this.LightParams.Add(new LightValueParam(lparam.Type.Value, lparam.Value.Value));
                    }
                }

                if (param.EdgeParams.Value != null)
                {
                    foreach (var eparam in param.EdgeParams.Value)
                    {
                        this.EdgeParams.Value.Add(new EdgeIndexAlignParam(eparam.Position, eparam.Direction));
                    }
                }

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }


        public override EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Version = typeof(WA_IndexAlignParam_Edge).Assembly.GetName().Version;
                CamType = EnumProberCam.WAFER_LOW_CAM;

                EdgeParams.Value = new ObservableCollection<EdgeIndexAlignParam>();
                AllowableRange.Value = 64;
                AlignThreshold.Value = 20;
                AlignEnable = EnumWASubModuleEnable.DISABLE;
                PositionToleranceWCtoTargetdie = new Element<int>();
                WaferCentertoTargetX = new Element<double>();
                WaferCentertoTargetY = new Element<double>();
                gIntEdgeDetectProcToleranceRad = new Element<double>();
                gIntEdgeDetectProcToleranceRad.Value = 1000;
                gIntEdgeDetectProcToleranceRad.LowerLimit = 0;
                gIntEdgeDetectProcToleranceRad.UpperLimit = 2000;

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }


        public override EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = SetDefaultParam();
                SetLotEmulParam();
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }


        public void SetLotEmulParam()
        {
            try
            {

                double wSize = this.StageSupervisor().WaferObject.GetPhysInfo().WaferSize_um.Value;
                double edgepos = 0.0;
                edgepos = ((wSize / 2) / Math.Sqrt(2));

                EdgeParams.Value.Clear();
                EdgeParams.Value.Add(new EdgeIndexAlignParam(new WaferCoordinate(edgepos, edgepos), EnumIndexAlignDirection.RIGHTUPPER));
                EdgeParams.Value.Add(new EdgeIndexAlignParam(new WaferCoordinate(-edgepos, edgepos), EnumIndexAlignDirection.LEFTUPPER));
                EdgeParams.Value.Add(new EdgeIndexAlignParam(new WaferCoordinate(-edgepos, -edgepos), EnumIndexAlignDirection.LEFTLOWER));
                EdgeParams.Value.Add(new EdgeIndexAlignParam(new WaferCoordinate(edgepos, -edgepos), EnumIndexAlignDirection.RIGHTLOWER));

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

    }

    [Serializable]
    public class EdgeIndexAlignParam : INotifyPropertyChanged
    {
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private WaferCoordinate _Position;
        public WaferCoordinate Position
        {
            get { return _Position; }
            set
            {
                if (value != _Position)
                {
                    _Position = value;
                    NotifyPropertyChanged("Position");
                }
            }
        }

        private EnumIndexAlignDirection _Direction;
        public EnumIndexAlignDirection Direction
        {
            get { return _Direction; }
            set
            {
                if (value != _Direction)
                {
                    _Direction = value;
                    NotifyPropertyChanged("Direction");
                }
            }
        }

        public EdgeIndexAlignParam()
        {

        }
        public EdgeIndexAlignParam(WaferCoordinate pos, EnumIndexAlignDirection direc)
        {
            try
            {
                Position = pos;
                Direction = direc;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

    }
}
