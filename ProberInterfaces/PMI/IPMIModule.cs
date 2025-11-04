using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces.Enum;
using ProberInterfaces.Param;
using ProberInterfaces.PnpSetup;
using SharpDXRender;
using SharpDXRender.RenderObjectPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using WinSize = System.Windows.Size;

namespace ProberInterfaces.PMI
{
    public class DoPMIData
    {
        public DoPMIData()
        {
            try
            {
                PMITrigger = PMIRemoteTriggerEnum.UNDIFINED;
                RemoteOperation = PMIRemoteOperationEnum.UNDEFIEND;

                if (ProcessedPMIMIndex == null)
                {
                    ProcessedPMIMIndex = new AsyncObservableCollection<MachineIndex>();
                }

                if (ReservedPMIMIndex == null)
                {
                    ReservedPMIMIndex = new AsyncObservableCollection<MachineIndex>();
                }
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncObservableCollection<MachineIndex> _ProcessedPMIMIndex;
        public AsyncObservableCollection<MachineIndex> ProcessedPMIMIndex
        {
            get { return _ProcessedPMIMIndex; }
            set
            {
                if (value != _ProcessedPMIMIndex)
                {
                    _ProcessedPMIMIndex = value;
                }
            }
        }

        private AsyncObservableCollection<MachineIndex> _ReservedPMIMIndex;
        public AsyncObservableCollection<MachineIndex> ReservedPMIMIndex
        {
            get { return _ReservedPMIMIndex; }
            set
            {
                if (value != _ReservedPMIMIndex)
                {
                    _ReservedPMIMIndex = value;
                }
            }
        }

        private int _WaferMapIndex;
        public int WaferMapIndex
        {
            get { return _WaferMapIndex; }
            set
            {
                if (value != _WaferMapIndex)
                {
                    _WaferMapIndex = value;
                }
            }
        }


        private bool _IsTurnFocusing;
        public bool IsTurnFocusing
        {
            get { return _IsTurnFocusing; }
            set
            {
                if (value != _IsTurnFocusing)
                {
                    _IsTurnFocusing = value;
                }
            }
        }

        private bool _IsTurnAutoLight;
        public bool IsTurnAutoLight
        {
            get { return _IsTurnAutoLight; }
            set
            {
                if (value != _IsTurnAutoLight)
                {
                    _IsTurnAutoLight = value;
                }
            }
        }

        // 0 ~ 65,535
        private ushort _RememberAutoLightValue;
        public ushort RememberAutoLightValue
        {
            get { return _RememberAutoLightValue; }
            set
            {
                if (value != _RememberAutoLightValue)
                {
                    _RememberAutoLightValue = value;
                }
            }
        }

        private double _RememberFocusingZValue;
        public double RememberFocusingZValue
        {
            get { return _RememberFocusingZValue; }
            set
            {
                if (value != _RememberFocusingZValue)
                {
                    _RememberFocusingZValue = value;
                }
            }
        }

        private PMIWORKMODE _WorkMode;
        public PMIWORKMODE WorkMode
        {
            get { return _WorkMode; }
            set
            {
                if (value != _WorkMode)
                {
                    _WorkMode = value;
                }
            }
        }

        private PMIRemoteTriggerEnum _PMITrigger;
        public PMIRemoteTriggerEnum PMITrigger
        {
            get { return _PMITrigger; }
            set
            {
                if (value != _PMITrigger)
                {
                    _PMITrigger = value;
                }
            }
        }

        private PMIRemoteOperationEnum _RemoteOperation;
        public PMIRemoteOperationEnum RemoteOperation
        {
            get { return _RemoteOperation; }
            set
            {
                if (value != _RemoteOperation)
                {
                    _RemoteOperation = value;
                }
            }
        }

        private PMIDieResult _LastPMIDieResult = new PMIDieResult();
        public PMIDieResult LastPMIDieResult
        {
            get { return _LastPMIDieResult; }
            set
            {
                if (value != _LastPMIDieResult)
                {
                    _LastPMIDieResult = value;
                }
            }
        }

        private int _AllPassPadCount = 0;
        public int AllPassPadCount
        {
            get { return _AllPassPadCount; }
            set
            {
                if (value != _AllPassPadCount)
                {
                    _AllPassPadCount = value;
                }
            }
        }

        private int _AllFailPadCount = 0;
        public int AllFailPadCount
        {
            get { return _AllFailPadCount; }
            set
            {
                if (value != _AllFailPadCount)
                {
                    _AllFailPadCount = value;
                }
            }
        }

        private EventCodeEnum _result;
        public EventCodeEnum Result
        {
            get { return _result; }
            set
            {
                if (value != _result)
                {
                    _result = value;
                }
            }
        }

        private bool _IsTurnOnMarkAlign = false;
        public bool IsTurnOnMarkAlign
        {
            get { return _IsTurnOnMarkAlign; }
            set
            {
                if (value != _IsTurnOnMarkAlign)
                {
                    _IsTurnOnMarkAlign = value;
                }
            }
        }

        /// <summary>
        /// 초기값을 -1로 설정하자.
        /// </summary>
        private int _RememberLastRemaingPMIDies;
        public int RememberLastRemaingPMIDies
        {
            get { return _RememberLastRemaingPMIDies; }
            set
            {
                if (value != _RememberLastRemaingPMIDies)
                {
                    _RememberLastRemaingPMIDies = value;
                }
            }
        }

    }

    [DataContract, Serializable]
    public class PMIDieResult : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private MachineIndex _MI;
        public MachineIndex MI
        {
            get { return _MI; }
            set
            {
                if (value != _MI)
                {
                    _MI = value;
                    RaisePropertyChanged();
                }
            }
        }

        private UserIndex _UI;
        public UserIndex UI
        {
            get { return _UI; }
            set
            {
                if (value != _UI)
                {
                    _UI = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _PassPadCount = 0;
        public int PassPadCount
        {
            get { return _PassPadCount; }
            set
            {
                if (value != _PassPadCount)
                {
                    _PassPadCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _FailPadCount = 0;
        public int FailPadCount
        {
            get { return _FailPadCount; }
            set
            {
                if (value != _FailPadCount)
                {
                    _FailPadCount = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    [DataContract, Serializable]
    public class PMIDrawingGroup : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public PMIDrawingGroup()
        {
            try
            {
                _Die = false;
                _Template = false;
                _RegisterdPad = false;
                _RegisterdPadIndex = false;
                _JudgingWindow = false;
                _MarkMin = false;
                _MarkMax = false;
                _DetectedMark = false;

                // Default value is true, True means to draw an overlay
                _IsOverlay = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private PadTemplate _PadTemplate = null;
        [DataMember]
        public PadTemplate PadTemplate
        {
            get { return _PadTemplate; }
            set
            {
                if (value != _PadTemplate)
                {
                    _PadTemplate = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PadTableTemplate _PadTableTemplate;
        [DataMember]
        public PadTableTemplate PadTableTemplate
        {
            get { return _PadTableTemplate; }
            set
            {
                if (value != _PadTableTemplate)
                {
                    _PadTableTemplate = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _SelectedPMIPadIndex;
        [DataMember]
        public int SelectedPMIPadIndex
        {
            get { return _SelectedPMIPadIndex; }
            set
            {
                if (value != _SelectedPMIPadIndex)
                {
                    _SelectedPMIPadIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PMI_SETUP_MODE _SetupMode = PMI_SETUP_MODE.UNDEFINED;
        [DataMember]
        public PMI_SETUP_MODE SetupMode
        {
            get { return _SetupMode; }
            set
            {
                if (value != _SetupMode)
                {
                    _SetupMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsOverlay;
        [DataMember]
        public bool IsOverlay
        {
            get { return _IsOverlay; }
            set
            {
                if (value != _IsOverlay)
                {
                    _IsOverlay = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Die;
        [DataMember]
        public bool Die
        {
            get { return _Die; }
            set
            {
                if (value != _Die)
                {
                    _Die = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Template;
        [DataMember]
        public bool Template
        {
            get { return _Template; }
            set
            {
                if (value != _Template)
                {
                    _Template = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _RegisterdPad;
        [DataMember]
        public bool RegisterdPad
        {
            get { return _RegisterdPad; }
            set
            {
                if (value != _RegisterdPad)
                {
                    _RegisterdPad = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _RegisterdPadIndex;
        [DataMember]
        public bool RegisterdPadIndex
        {
            get { return _RegisterdPadIndex; }
            set
            {
                if (value != _RegisterdPadIndex)
                {
                    _RegisterdPadIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _JudgingWindow;
        [DataMember]
        public bool JudgingWindow
        {
            get { return _JudgingWindow; }
            set
            {
                if (value != _JudgingWindow)
                {
                    _JudgingWindow = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _MarkMin;
        [DataMember]
        public bool MarkMin
        {
            get { return _MarkMin; }
            set
            {
                if (value != _MarkMin)
                {
                    _MarkMin = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _MarkMax;
        [DataMember]
        public bool MarkMax
        {
            get { return _MarkMax; }
            set
            {
                if (value != _MarkMax)
                {
                    _MarkMax = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _DetectedMark;
        [DataMember]
        public bool DetectedMark
        {
            get { return _DetectedMark; }
            set
            {
                if (value != _DetectedMark)
                {
                    _DetectedMark = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    [Serializable, DataContract]
    public class PadTemplate : INotifyPropertyChanged, IParamNode, IFactoryModule
    {
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        [XmlIgnore, JsonIgnore]
        public virtual string Genealogy { get; set; }
        [XmlIgnore, JsonIgnore]
        public virtual object Owner { get; set; }
        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }

        //[ParamIgnore]
        //public string Genealogy { get; set; }

        //[NonSerialized]
        //private Object _Owner;

        //[XmlIgnore, JsonIgnore, ParamIgnore]
        //public Object Owner
        //{
        //    get { return _Owner; }
        //    set
        //    {
        //        if (_Owner != value)
        //        {
        //            _Owner = value;
        //        }
        //    }
        //}
        //[ParamIgnore]
        //public List<object> Nodes { get; set; }

        private Element<int> _Color = new Element<int>();
        [DataMember]
        public Element<int> Color
        {
            get { return _Color; }
            set
            {
                if (value != _Color)
                {
                    _Color = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _ShapeName = new Element<string>();
        [DataMember]
        public Element<string> ShapeName
        {
            get { return _ShapeName; }
            set
            {
                if (value != _ShapeName)
                {
                    _ShapeName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _PathData = new Element<string>();
        [DataMember]
        public Element<string> PathData
        {
            get { return _PathData; }
            set
            {
                if (value != _PathData)
                {
                    _PathData = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _SizeX = new Element<double>();
        [DataMember]
        public Element<double> SizeX
        {
            get { return _SizeX; }
            set
            {
                if (value != _SizeX)
                {
                    _SizeX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _SizeY = new Element<double>();
        [DataMember]
        public Element<double> SizeY
        {
            get { return _SizeY; }
            set
            {
                if (value != _SizeY)
                {
                    _SizeY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _Angle = new Element<double>();
        [DataMember]
        public Element<double> Angle
        {
            get { return _Angle; }
            set
            {
                if (value != _Angle)
                {
                    _Angle = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _Area = new Element<double>();
        [DataMember]
        public Element<double> Area
        {
            get { return _Area; }
            set
            {
                if (value != _Area)
                {
                    _Area = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _JudgingWindowSizeX = new Element<double>();
        [DataMember]
        public Element<double> JudgingWindowSizeX
        {
            get { return _JudgingWindowSizeX; }
            set
            {
                if (value != _JudgingWindowSizeX)
                {
                    _JudgingWindowSizeX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _JudgingWindowSizeY = new Element<double>();
        [DataMember]
        public Element<double> JudgingWindowSizeY
        {
            get { return _JudgingWindowSizeY; }
            set
            {
                if (value != _JudgingWindowSizeY)
                {
                    _JudgingWindowSizeY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _MarkWindowMinSizeX = new Element<double>();
        [DataMember]
        public Element<double> MarkWindowMinSizeX
        {
            get { return _MarkWindowMinSizeX; }
            set
            {
                if (value != _MarkWindowMinSizeX)
                {
                    _MarkWindowMinSizeX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _MarkWindowMinSizeY = new Element<double>();
        [DataMember]
        public Element<double> MarkWindowMinSizeY
        {
            get { return _MarkWindowMinSizeY; }
            set
            {
                if (value != _MarkWindowMinSizeY)
                {
                    _MarkWindowMinSizeY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _MarkWindowMaxSizeX = new Element<double>();
        [DataMember]
        public Element<double> MarkWindowMaxSizeX
        {
            get { return _MarkWindowMaxSizeX; }
            set
            {
                if (value != _MarkWindowMaxSizeX)
                {
                    _MarkWindowMaxSizeX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _MarkWindowMaxSizeY = new Element<double>();
        [DataMember]
        public Element<double> MarkWindowMaxSizeY
        {
            get { return _MarkWindowMaxSizeY; }
            set
            {
                if (value != _MarkWindowMaxSizeY)
                {
                    _MarkWindowMaxSizeY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _MarkWindowMinPercent = new Element<double>();
        [DataMember]
        public Element<double> MarkWindowMinPercent
        {
            get { return _MarkWindowMinPercent; }
            set
            {
                if (value != _MarkWindowMinPercent)
                {
                    _MarkWindowMinPercent = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _MarkWindowMaxPercent = new Element<double>();
        [DataMember]
        public Element<double> MarkWindowMaxPercent
        {
            get { return _MarkWindowMaxPercent; }
            set
            {
                if (value != _MarkWindowMaxPercent)
                {
                    _MarkWindowMaxPercent = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _TemplateOffset = new Element<double>();
        [DataMember]
        public Element<double> TemplateOffset
        {
            get { return _TemplateOffset; }
            set
            {
                if (value != _TemplateOffset)
                {
                    _TemplateOffset = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _TemplateCornerRadius = new Element<double>();
        [DataMember]
        public Element<double> TemplateCornerRadius
        {
            get { return _TemplateCornerRadius; }
            set
            {
                if (value != _TemplateCornerRadius)
                {
                    _TemplateCornerRadius = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<PAD_SHAPE> _Shape = new Element<PAD_SHAPE>();
        [DataMember]
        public Element<PAD_SHAPE> Shape
        {
            get { return _Shape; }
            set
            {
                if (value != _Shape)
                {
                    _Shape = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<PAD_EDGE_OFFSET_MODE> _EdgeOffsetMode = new Element<PAD_EDGE_OFFSET_MODE>();
        [DataMember]
        public Element<PAD_EDGE_OFFSET_MODE> EdgeOffsetMode
        {
            get { return _EdgeOffsetMode; }
            set
            {
                if (value != _EdgeOffsetMode)
                {
                    _EdgeOffsetMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<PAD_CORNERRADIUS_MODE> _CornerRadiusMode = new Element<PAD_CORNERRADIUS_MODE>();
        [DataMember]
        public Element<PAD_CORNERRADIUS_MODE> CornerRadiusMode
        {
            get { return _CornerRadiusMode; }
            set
            {
                if (value != _CornerRadiusMode)
                {
                    _CornerRadiusMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<PAD_JUDGING_WINDOW_MODE> _JudgingWindowMode = new Element<PAD_JUDGING_WINDOW_MODE>();
        [DataMember]
        public Element<PAD_JUDGING_WINDOW_MODE> JudgingWindowMode
        {
            get { return _JudgingWindowMode; }
            set
            {
                if (value != _JudgingWindowMode)
                {
                    _JudgingWindowMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        public double UpdateArea()
        {
            double retval = 0;

            try
            {
                // Make Geometry, and used GetArea() Method.

                Geometry geo = Geometry.Parse(this.PathData.Value);
                Geometry transfomedGeo;

                TransformGroup tg = new TransformGroup();

                ScaleTransform t1 = new ScaleTransform();
                TranslateTransform t2 = new TranslateTransform();
                RotateTransform t3 = new RotateTransform();

                tg.Children.Clear();

                double scaleX, scaleY;

                scaleX = (SizeX.Value / geo.Bounds.Width);
                scaleY = (SizeY.Value / geo.Bounds.Height);

                t1.ScaleX = scaleX;
                t1.ScaleY = scaleY;

                t3.Angle = this.Angle.Value;

                tg.Children.Add(t1);
                tg.Children.Add(t3);

                transfomedGeo = geo.Clone();
                transfomedGeo.Transform = tg;

                this.Area.Value = transfomedGeo.GetArea();

                if (this.Area.Value > 0)
                {
                    this.MarkWindowMinPercent.Value = (this.MarkWindowMinSizeX.Value * this.MarkWindowMinSizeY.Value) * 100.0 / this.Area.Value;
                    this.MarkWindowMaxPercent.Value = (this.MarkWindowMaxSizeX.Value * this.MarkWindowMaxSizeY.Value) * 100.0 / this.Area.Value;
                }
                else
                {
                    this.MarkWindowMinPercent.Value = 0;
                    this.MarkWindowMaxPercent.Value = 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private void Copy(PadTemplate source)
        {
            this.Color.Value = source.Color.Value;
            this.ShapeName.Value = source.ShapeName.Value;

            this.PathData.Value = source.PathData.Value;
            this.SizeX.Value = source.SizeX.Value;
            this.SizeY.Value = source.SizeY.Value;
            this.Angle.Value = source.Angle.Value;

            this.Area.Value = source.Area.Value;

            this.JudgingWindowSizeX.Value = source.JudgingWindowSizeX.Value;
            this.JudgingWindowSizeY.Value = source.JudgingWindowSizeY.Value;
            this.MarkWindowMinSizeX.Value = source.MarkWindowMinSizeX.Value;
            this.MarkWindowMinSizeY.Value = source.MarkWindowMinSizeY.Value;
            this.MarkWindowMaxSizeX.Value = source.MarkWindowMaxSizeX.Value;
            this.MarkWindowMaxSizeY.Value = source.MarkWindowMaxSizeY.Value;
            this.MarkWindowMinPercent.Value = source.MarkWindowMinPercent.Value;
            this.MarkWindowMaxPercent.Value = source.MarkWindowMaxPercent.Value;
            this.TemplateOffset.Value = source.TemplateOffset.Value;
            this.Shape.Value = source.Shape.Value;
            this.EdgeOffsetMode.Value = source.EdgeOffsetMode.Value;
            this.CornerRadiusMode.Value = source.CornerRadiusMode.Value;
            this.JudgingWindowMode.Value = source.JudgingWindowMode.Value;
        }

        public PadTemplate(PadTemplate template)
        {
            try
            {
                Copy(template);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public PadTemplate()
        {
            try
            {
                //_Color.Value = 0;
                //_ShapeName.Value = "Rectangle";
                //_Shape.Value = PAD_SHAPE.RECTANGLE;
                //_PathData.Value = "M0,0 L0,1 1,1 1,0 z";
                //_JudgingWindowMode.Value = PAD_JUDGING_WINDOW_MODE.TWOWAY;
                //_EdgeOffsetMode.Value = PAD_EDGE_OFFSET_MODE.DISABLE;

                //_SizeX.Value = 50;
                //_SizeY.Value = 50;
                //_Angle.Value = 0;
                //_PadArea.Value = 2500;

                //_TemplateOffset.Value = 0;

                //_JudgingWindowSizeX.Value = 1;
                //_JudgingWindowSizeY.Value = 1;

                //_MarkWindowMinSizeX.Value = 3;
                //_MarkWindowMinSizeY.Value = 3;

                //_MarkWindowMaxSizeX.Value = 10;
                //_MarkWindowMaxSizeY.Value = 10;

                //_MarkWindowMinPercent.Value = _MarkWindowMinSizeX.Value * _MarkWindowMinSizeY.Value;
                //_MarkWindowMaxPercent.Value = _MarkWindowMaxSizeX.Value * _MarkWindowMaxSizeY.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public PadTemplate(string Path)
        {
            try
            {
                _Color.Value = 0;
                _ShapeName.Value = "RECTANGLE";
                _Shape.Value = PAD_SHAPE.RECTANGLE;
                _PathData.Value = Path;
                _SizeX.Value = 50;
                _SizeY.Value = 50;
                _Angle.Value = 0;
                _Area.Value = 2500;
                _TemplateOffset.Value = 0;
                _JudgingWindowSizeX.Value = 1;
                _JudgingWindowSizeY.Value = 1;
                _MarkWindowMinSizeX.Value = 3;
                _MarkWindowMinSizeY.Value = 3;
                _MarkWindowMaxSizeX.Value = 10;
                _MarkWindowMaxSizeY.Value = 10;
                _MarkWindowMinPercent.Value = _MarkWindowMinSizeX.Value * _MarkWindowMinSizeY.Value;
                _MarkWindowMaxPercent.Value = _MarkWindowMaxSizeX.Value * _MarkWindowMaxSizeY.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public PadTemplate(PAD_SHAPE Shape, string Name, PAD_COLOR color, PAD_JUDGING_WINDOW_MODE JudgingWindowMode, PAD_EDGE_OFFSET_MODE EdgeOffsetMode, PAD_CORNERRADIUS_MODE CornerRadiusMode,  string Path, double EdgeOffset = 0, double CornerRadius = 0)
        {
            try
            {
                _Color.Value = (int)color;
                _Shape.Value = Shape;
                _ShapeName.Value = Name;

                _JudgingWindowMode.Value = JudgingWindowMode;
                _EdgeOffsetMode.Value = EdgeOffsetMode;
                _CornerRadiusMode.Value = CornerRadiusMode;
                
                _PathData.Value = Path;
                _Shape.Value = Shape;

                _SizeX.Value = 50;
                _SizeY.Value = 50;

                _Angle.Value = 0;

                _Area.Value = 2500;

                _TemplateOffset.Value = EdgeOffset;
                _TemplateCornerRadius.Value = CornerRadius;

                _JudgingWindowSizeX.Value = 1;
                _JudgingWindowSizeY.Value = 1;
                _MarkWindowMinSizeX.Value = 3;
                _MarkWindowMinSizeY.Value = 3;
                _MarkWindowMaxSizeX.Value = 10;
                _MarkWindowMaxSizeY.Value = 10;
                _MarkWindowMinPercent.Value = _MarkWindowMinSizeX.Value * _MarkWindowMinSizeY.Value;
                _MarkWindowMaxPercent.Value = _MarkWindowMaxSizeX.Value * _MarkWindowMaxSizeY.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

    }

    [Serializable]
    public class PadGroupTemplate : INotifyPropertyChanged, IParamNode
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ==> ParamNode
        [ParamIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }
        [ParamIgnore]
        public List<object> Nodes { get; set; }
        #endregion

        public PadGroupTemplate()
        {
            Groups = new List<PMIGroupData>();
        }

        private Element<int> _UsedTableIndex = new Element<int>();
        public Element<int> UsedTableIndex
        {
            get { return _UsedTableIndex; }
            set
            {
                if (value != _UsedTableIndex)
                {
                    _UsedTableIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _GroupingDone = new Element<bool>();
        public Element<bool> GroupingDone
        {
            get { return _GroupingDone; }
            set
            {
                if (value != _GroupingDone)
                {
                    _GroupingDone = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<PMIGroupData> _Groups;
        public List<PMIGroupData> Groups
        {
            get { return _Groups; }
            set
            {
                if (value != _Groups)
                {
                    _Groups = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    [Serializable, DataContract]
    public class DieMapTemplate : INotifyPropertyChanged, IParamNode
    {
        #region //..PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public const int BIT_SIZE_INT = 32;

        #region ==> ParamNode
        [ParamIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }
        [ParamIgnore]
        public List<object> Nodes { get; set; }
        #endregion

        public DieMapTemplate(int width, int height)
        {
            try
            {
                MapWidth = width;
                MapHeight = height;

                InitMap();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private int _MapWidth;
        [DataMember]
        public int MapWidth
        {
            get { return _MapWidth; }
            set
            {
                if (value != _MapWidth)
                {
                    _MapWidth = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _MapHeight;
        [DataMember]
        public int MapHeight
        {
            get { return _MapHeight; }
            set
            {
                if (value != _MapHeight)
                {
                    _MapHeight = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<long[]> _PMIMap = new Element<long[]>();
        [DataMember]
        public Element<long[]> PMIMap
        {
            get { return _PMIMap; }
            set
            {
                if (value != _PMIMap)
                {
                    _PMIMap = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<long[]> _PMITableMap = new Element<long[]>();
        [DataMember]
        public Element<long[]> PMITableMap
        {
            get { return _PMITableMap; }
            set
            {
                if (value != _PMITableMap)
                {
                    _PMITableMap = value;
                    RaisePropertyChanged();
                }
            }
        }

        public void ClearEnable()
        {
            try
            {
                Array.Clear(PMIMap.Value, 0, PMIMap.Value.GetLength(0));

                //PMIMap.Value = Enumerable.Repeat<uint>(0, PMIMap.Value.GetLength(0)).ToArray<uint>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ClearTable()
        {
            try
            {
                Array.Clear(PMITableMap.Value, 0, PMITableMap.Value.GetLength(0));

                //PMITableMap.Value = Enumerable.Repeat<uint>(0, PMITableMap.Value.GetLength(0)).ToArray<uint>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetTable(int X, int Y, int tableNum)
        {
            try
            {
                if ((MapWidth <= 0) || (MapHeight <= 0))
                {
                    return;
                }

                BitArray b = new BitArray(new int[] { tableNum + 1 });
                int[] bits = b.Cast<bool>().Select(bit => bit ? 1 : 0).ToArray();

                // bits[3] bits[2] bits[1] bits[0]

                int Index = (Y * MapWidth) + X;

                int mapindex = Index / 8;

                int StartbitNum = (Index % 8) * 4;

                uint val = 0;

                for (int i = 0; i < 4; i++)
                {
                    uint tmp = (uint)(0x01 << (StartbitNum + i));

                    if ((PMITableMap.Value[mapindex] & tmp) == tmp)
                    {
                        PMITableMap.Value[mapindex] = PMITableMap.Value[mapindex] - tmp;
                    }

                    val = val + (uint)(bits[i] << (StartbitNum + i));
                }

                PMITableMap.Value[mapindex] = (uint)(PMITableMap.Value[mapindex] | val);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public int GetTable(int X, int Y)
        {
            int retval = 0;
            try
            {

                int Index = (Y * MapWidth) + X;

                int mapindex = Index / 8;

                int StartbitNum = (Index % 8) * 4;

                BitArray b = new BitArray(new int[] { (int)PMITableMap.Value[mapindex] });
                int[] bits = b.Cast<bool>().Select(bit => bit ? 1 : 0).ToArray();

                for (int i = 0; i < 4; i++)
                {
                    if (bits[StartbitNum + i] == 0x01)
                    {
                        retval = retval + (0x01 << i);
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void SetEnable(int X, int Y, bool SetValue)
        {
            int Index = (Y * MapWidth) + X;

            int range = MapWidth * MapHeight;

            if ((MapWidth <= 0) || (MapHeight <= 0))
            {
                return;
            }

            if (Index > range)
            {
                try
                {
                    // ERROR
                    throw new ArgumentException($"Index{Index} is out of range{range}.");
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }

            int mapindex = Index / BIT_SIZE_INT;

            int bitNum = Index % BIT_SIZE_INT;

            uint bitValue;

            bitValue = (uint)(0x01 << bitNum);

            if (SetValue == true)
            {
                PMIMap.Value[mapindex] = (uint)(PMIMap.Value[mapindex] | bitValue);
            }
            else
            {
                if ((PMIMap.Value[mapindex] & bitValue) == bitValue)
                {
                    PMIMap.Value[mapindex] = (uint)(PMIMap.Value[mapindex] - bitValue);
                }
            }
        }

        public uint GetEnable(int X, int Y)
        {
            uint retval = 0x00;
            try
            {
                if (X >= 0 && Y >= 00)
                {
                    int Index = (Y * MapWidth) + X;

                    int range = MapWidth * MapHeight;

                    if ((MapWidth <= 0) || (MapHeight <= 0))
                    {
                    }
                    else
                    {
                        if (Index > range)
                        {
                            // ERROR
                            throw new ArgumentException($"Index{Index} is out of range{range}.");
                        }

                        int mapindex = Index / BIT_SIZE_INT;

                        int bitNum = Index % BIT_SIZE_INT;

                        uint bitValue;

                        bitValue = (uint)(0x01 << bitNum);

                        if ((PMIMap.Value[mapindex] & bitValue) == bitValue)
                        {
                            retval = 0x01;
                        }
                        else
                        {
                            retval = 0x00;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public List<MachineIndex> GetAllEnables()
        {
            var retVal = new List<MachineIndex>();

            for (var i = 0; i < PMIMap.Value.Count(); i++)
            {
                var mapValue = PMIMap.Value[i];
                if (mapValue > 0)
                {
                    var binaryNum = Convert.ToString(mapValue, 2);

                    var searchIdx = -1;
                    do
                    {
                        searchIdx = binaryNum.IndexOf('1');

                        if (searchIdx > -1)
                        {
                            var bitValIdx = (binaryNum.Length - 1) - searchIdx;

                            binaryNum = binaryNum.Substring(0, searchIdx) +
                                        "0" +
                                        binaryNum.Substring(searchIdx + 1);

                            long index = (BIT_SIZE_INT * i) + bitValIdx;

                            #region 범위 체크
                            if ((MapWidth <= 0) || (MapHeight <= 0))
                            {
                                continue;
                            }
                            if (index > MapWidth * MapHeight)
                            {
                                continue;
                            }
                            #endregion

                            var x = index % MapWidth;
                            var y = index / MapWidth;
                            
                            retVal.Add(new MachineIndex(x, y));
                        }
                    } while (searchIdx > -1);
                }
            }

            // Sorting
            retVal.Sort((m1, m2) =>
            {
                if ((m1.XIndex == m2.XIndex) && (m1.YIndex == m2.YIndex))
                {
                    return 0;
                }
                if ((m1.XIndex > m2.XIndex))// || (m1.YIndex < m2.YIndex))
                {
                    return 1;
                }
                if(m1.XIndex < m2.XIndex)
                {
                    return -1;
                }
                if(m1.XIndex == m2.XIndex)
                {
                    if ((m1.YIndex > m2.YIndex))// || (m1.YIndex < m2.YIndex))
                    {
                        return 1;
                    }
                    if (m1.YIndex < m2.YIndex)
                    {
                        return -1;
                    }
                }
                return 0;
            });

            return retVal;
        }

        public EventCodeEnum InitMap()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {

                try
                {
                    if ((MapWidth > 0) && (MapHeight > 0))
                    {
                        long MapCount = (MapWidth * MapHeight) / BIT_SIZE_INT + 1;

                        PMIMap.Value = new long[MapCount];
                        PMITableMap.Value = new long[MapCount * 4];

                        retval = EventCodeEnum.NONE;
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Debug($"[DieMapTemplate] [InitMap()]: {err}");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    [Serializable]
    public class PadTableTemplate : INotifyPropertyChanged, IParamNode
    {
        #region //..PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ==> ParamNode
        [ParamIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }
        [ParamIgnore]
        public List<object> Nodes { get; set; }
        #endregion

        public void Clear()
        {
            try
            {
                this.PadEnable.Clear();
                this.GroupingDone.Value = false;
                this.Groups.Clear();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public PadTableTemplate()
        {
            try
            {
                _PadEnable = new List<Element<bool>>();
                _GroupingDone.Value = false;
                _Groups = new List<PMIGroupData>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private List<Element<bool>> _PadEnable;
        public List<Element<bool>> PadEnable
        {
            get { return _PadEnable; }
            set
            {
                if (value != _PadEnable)
                {
                    _PadEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _GroupingDone = new Element<bool>();
        public Element<bool> GroupingDone
        {
            get { return _GroupingDone; }
            set
            {
                if (value != _GroupingDone)
                {
                    _GroupingDone = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<PMIGroupData> _Groups;
        public List<PMIGroupData> Groups
        {
            get { return _Groups; }
            set
            {
                if (value != _Groups)
                {
                    _Groups = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    [Serializable]
    public class PMIGroupData : INotifyPropertyChanged
    {
        #region //..PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public PMIGroupData()
        {
            try
            {
                _SeqNum.Value = 0;
                //_PadRegCount = 0;
                _GroupPosition.Value = new Rect();
                _PadDataInGroup = new List<PadDataInGroup>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private Element<int> _SeqNum = new Element<int>();
        public Element<int> SeqNum
        {
            get { return _SeqNum; }
            set
            {
                if (value != _SeqNum)
                {
                    _SeqNum = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<Rect> _GroupPosition = new Element<Rect>();
        public Element<Rect> GroupPosition
        {
            get { return _GroupPosition; }
            set
            {
                if (value != _GroupPosition)
                {
                    _GroupPosition = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<PadDataInGroup> _PadDataInGroup = new List<PadDataInGroup>();
        public List<PadDataInGroup> PadDataInGroup
        {
            get { return _PadDataInGroup; }
            set
            {
                if (value != _PadDataInGroup)
                {
                    _PadDataInGroup = value;
                    RaisePropertyChanged();
                }
            }
        }

    }

    [Serializable]
    public class PadDataInGroup : INotifyPropertyChanged
    {
        #region //..PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        public PadDataInGroup()
        {
            try
            {
                _PadCenPosInGroup = new Element<Point>();
                _PadRealIndex.Value = 0;
                _PadWidth.Value = 0.0;
                _PadHeight.Value = 0.0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private Element<Point> _PadCenPosInGroup = new Element<Point>();
        public Element<Point> PadCenPosInGroup
        {
            get { return _PadCenPosInGroup; }
            set
            {
                if (value != _PadCenPosInGroup)
                {
                    _PadCenPosInGroup = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _PadRealIndex = new Element<int>();
        public Element<int> PadRealIndex
        {
            get { return _PadRealIndex; }
            set
            {
                if (value != _PadRealIndex)
                {
                    _PadRealIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _PadWidth = new Element<double>();
        public Element<double> PadWidth
        {
            get { return _PadWidth; }
            set
            {
                if (value != _PadWidth)
                {
                    _PadWidth = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _PadHeight = new Element<double>();
        public Element<double> PadHeight
        {
            get { return _PadHeight; }
            set
            {
                if (value != _PadHeight)
                {
                    _PadHeight = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    [Serializable]
    public class WaferTemplate : INotifyPropertyChanged, IParamNode
    {
        [ParamIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }
        [ParamIgnore]
        public List<object> Nodes { get; set; }

        #region //..PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public WaferTemplate()
        {
            try
            {
                _PMIEnable.Value = false;
                SelectedMapIndex.Value = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private Element<bool> _PMIEnable = new Element<bool>();
        public Element<bool> PMIEnable
        {
            get { return _PMIEnable; }
            set
            {
                if (value != _PMIEnable)
                {
                    _PMIEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _SelectedMapIndex = new Element<int>();
        public Element<int> SelectedMapIndex
        {
            get { return _SelectedMapIndex; }
            set
            {
                if (value != _SelectedMapIndex)
                {
                    _SelectedMapIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    #region InterFaces
    public interface IPMIInfo : IParamNode
    {
        //PMI_SETUP_MODE SetupMode { get; set; }
        //PMI_RENDER_TABLE_MODE RenderTableMode { get; set; }
        //int SelectedPadTemplateIndex { get; set; }
        int SelectedNormalPMIMapTemplateIndex { get; set; }
        //int SelectedSamplePMIMapTemplateIndex { get; set; }
        //int SelectedPadTableTemplateIndex { get; set; }
        //int SelectedWaferTemplateIndex { get; set; }

        //PadTemplate SelectedPadTemplate { get; set; }
        //DieMapTemplate SelectedNormalPMIMapTemplate { get; set; }
        //DieMapTemplate SelectedSamplePMIMapTemplate { get; set; }
        //PadTableTemplate SelectedPadTableTemplate { get; set; }
        //WaferTemplate SelectedWaferTemplate { get; set; }
        //Path SelectedPadTemplatePath { get; set; }

        ObservableCollection<PadTemplate> PadTemplateInfo { get; set; }
        ObservableCollection<DieMapTemplate> NormalPMIMapTemplateInfo { get; set; }
        //ObservableCollection<DieMapTemplate> SamplePMIMapTemplateInfo { get; set; }
        ObservableCollection<PadTableTemplate> PadTableTemplateInfo { get; set; }

        ObservableCollection<WaferTemplate> WaferTemplateInfo { get; set; }
        //ObservableCollection<PadGroupTemplate> PadGroupTemplateInfo { get; set; }

        PadTemplate GetPadTemplate(int index);
        DieMapTemplate GetNormalPMIMapTemplate(int index);
        //DieMapTemplate GetSamplePMIMapTemplate(int index);
        PadTableTemplate GetPadTableTemplate(int index);
        WaferTemplate GetWaferTemplate(int index);

        EventCodeEnum UpdatePadTableTemplateInfo();
        //void PMIInfoUpdatedToLoader();

        //PadGroupTemplate GetPadGroupTemplate(int index);

        //bool PadGroupingDone { get; set; }

        //EventCodeEnum SetSelectedPadTableTemplateIndex(int index);
        //EventCodeEnum SetSelectedNormalMapTemplateIndex(int index);
        //EventCodeEnum SetSelectedSampleMapTemplateIndex(int index);
        //EventCodeEnum SetSelectedPadTemplateIndex(int index);
        //int GetSelectedPadTemplateIndex();
        //int GetSelectedPadTableIndex();
        //int GetSelectedNormalPMIMapIndex();

        //int GetSelectedSamplePMIMapIndex();
        //ObservableCollection<PadTemplate> GetPadTemplateInfo();
        //ObservableCollection<PadTableTemplate> GetPadTableTemplateInfo();
        //ObservableCollection<DieMapTemplate> GetNormalPMIMapTemplateInfo();
        //ObservableCollection<DieMapTemplate> GetSamplePMIMapTemplateInfo();
        //ObservableCollection<WaferTemplate> GetWaferTemplateInfo();
        //ObservableCollection<PadGroupTemplate> GetPadGroupTemplateInfo();
    }

    public interface IPMIRenderLayer
    {
        void UpdateTemplate(AsyncObservableCollection<RenderObject> objects);
        void UpdateJudgingWindow(AsyncObservableCollection<RenderObject> objects);
        void UpdateMarkMinSize(AsyncObservableCollection<RenderObject> objects);
        void UpdateMarkMaxSize(AsyncObservableCollection<RenderObject> objects);
        void UpdateRegisterdPad(AsyncObservableCollection<RenderObject> objects);
        void UpdateRegisterdPadIndex(AsyncObservableCollection<RenderObject> objects);
        void UpdateDetectedMarks(AsyncObservableCollection<RenderObject> objects);
        void UpdateProximityLine(AsyncObservableCollection<RenderObject> objects);

        void ClearAllRenderContainer();
    }

    public interface IHasPMIDrawingGroup
    {
        PMIDrawingGroup DrawingGroup { get; set; }
    }

    public interface IPMIModuleLogger : IModule
    {
        string GetPMIFileFullPath();
        EventCodeEnum RecordPMIResultPerDie(MachineIndex MI);
    }

    public interface IPMIMarkInformationAnalyzer : IModule
    {
        List<MarkStatusCodeEnum> GetMarkStatusCode();
    }

    public interface IPMIModuleSubRutine : ISubRoutine, IFactoryModule, IModule
    {
        void ClearRenderObjects();
        RenderLayer InitPMIRenderLayer(WinSize size, float r, float g, float b, float a);
        WinSize GetLayerSize();
        Point GetRenderLayerRatio();
        bool CheckFocusingInterval(int DutNo);
        void SetSubModule(object SubModule);
        object GetSubModule();
        void UpdateLabel();
        void UpdateCurrentPadIndex();
        void UpdateRenderLayer();
        void MovedDelegate(ImageBuffer Img);
        EventCodeEnum FindPad(PadTemplate padtemplate);
        EventCodeEnum DoPMI(DoPMIData Info);
        EventCodeEnum PadGroupingMethod(int TableNumber);
        EventCodeEnum MakeGroupSequence(int TemplateIndex);
        EventCodeEnum MoveToPad(ICamera CurCam, MachineIndex Mi, int padIndex);
        EventCodeEnum MoveToMark(ICamera CurCam, MachineIndex Mi, int padIndex, int markIndex);
        bool CheckPMIPadExist();
        bool CheckCurWaferPMIEnable();
        bool CheckCurWaferInterval();
        bool CheckCurTouchdownCount();
        IPMIInfo PMIInfo { get; }
        EventCodeEnum EnterMovePadPosition(ref PMIPadObject MovedPadInfo);
    }

    [ServiceContract]
    public interface IPMIModule : IStateModule, IPnpSetupScreen, IHasDevParameterizable, IHasSysParameterizable, ITemplateStateModule, ITemplateExtension
    {
        EventCodeEnum ResetPMIData();
        EventCodeEnum InitPMIResult();
        bool IsTurnOnPMIInLotRun();
        bool IsLastProbingSeqProceessd();
        [OperationContract]
        bool GetPMIEnableParam();
        void ClearRenderObjects();
        //EventCodeEnum UpdateDisplayedDevices(ICamera Curcam);
        EventCodeEnum UpdateDisplayedDevices(ICamera Curcam, bool InitPads = true);
        IParam PMIModuleDevParam_IParam { get; set; }
        IParam PMIModuleSysParam_IParam { get; set; }
        RenderLayer InitPMIRenderLayer(WinSize size, float r, float g, float b, float a);
        WinSize GetLayerSize();
        //IPMIModuleDevParam PMIDevParam { get; set; }
        //PadTemplate GetCurrentTempalte();

        //void ChangeTemplateSizeCommand(JOG_DIRECTION direction);
        //void ChangeTemplateOffsetCommand(SETUP_DIRECTION direction);
        //void ChangeTemplateAngleCommand();
        //void ChangeJudgingWindowSizeCommand(JOG_DIRECTION direction);
        //void ChangeMarkSizeCommand(MARK_SIZE curMode, JOG_DIRECTION direction);
        //void ChangePadPositionCommand(SELECTION_MODE mode, JOG_DIRECTION direction);
        //void ChangeTemplateColorCommand(PAD_COLOR padColor);
        //void AddTemplateCommand(PAD_SHAPE shape, string name, PAD_COLOR color, double offset = 0.1);
        //void DeleteTemplateCommand();
        //void ChangeTemplateIndexCommand(SETUP_DIRECTION direction);

        void SetSubModule(object SubModule);
        void UpdateCurrentPadIndex();
        void UpdateRenderLayer();

        //void ChangedPadTemplate(PadTemplate template);

        Point GetRenderLayerRatio();
        //RenderLayer GetPMIRenderLayer();
        //int? GetPMITemplateIndex();
        EventCodeEnum MovedDelegate(ImageBuffer Img);
        //EventCodeEnum FindPad();
        EventCodeEnum FindPad(PadTemplate padtemplate);
        EventCodeEnum DoPMI();
        //EventCodeEnum PadGroupingMethod(int curPMIPadTableNum, ref List<PMIGroupData> PMIPadGroup);
        EventCodeEnum UpdateGroupingInformation();
        EventCodeEnum PadGroupingMethod(int curPMIPadTableNum);
        EventCodeEnum MakeGroupSequence(int TableIndex);
        EventCodeEnum MoveToPad(ICamera CurCam, MachineIndex Mi, int padIndex);
        EventCodeEnum MoveToMark(ICamera CurCam, MachineIndex Mi, int padIndex, int markIndex);

        //bool CheckPMIDieExist();
        bool CheckCurWaferPMIEnable();
        bool CheckFocusingInterval(int DutNo);
        IPMIModuleLogger PMILogger { get; set; }
        DoPMIData DoPMIInfo { get; set; }
        //void PadIndexMoveCommand(object direction);
        //void TableIndexMoveCommand(object direction);
        //[OperationContract]
        //EventCodeEnum DoPMIProcessing();

        [OperationContract]
        IParam GetPMIDevIParam();
        [OperationContract]
        byte[] GetPMIDevParam();
        [OperationContract]
        IParam GetPMISysIParam();

        EventCodeEnum SetPMITrigger(PMIRemoteTriggerEnum trigger);

        EventCodeEnum SetRemoteOperation(PMIRemoteOperationEnum remotevalue);

        void PMIInfoUpdatedToLoader();

        [OperationContract]
        void AddPadTemplate(PadTemplate template);
        [OperationContract]
        bool IsServiceAvailable();

        EventCodeEnum EnterMovePadPosition(ref PMIPadObject MovedPadInfo);

        IFocusing GetFocuisngModule();

        List<MachineIndex> GetRemainingPMIDies();

        /// <summary>
        /// PMI 성공 여부 반환.
        /// </summary>
        /// <returns></returns>
        bool GetPMIResult();

        [OperationContract]
        PMITriggerComponent GetTriggerComponent();
    }
    #endregion

    #region Enums

    public enum PMIWORKMODE
    {
        MANUAL = 0,
        AUTO
    }

    [DataContract]
    public enum PadStatusResultEnum
    {
        [EnumMember]
        ALL = 0,
        [EnumMember]
        FAIL,
        [EnumMember]
        PASS,
    }

    public enum PadStatusCodeEnum
    {
        PASS = 0,
        FAIL,
        NO_PROBE_MARK,
        TOO_MANY_PROBE_MARK,
        NEED_REFERENCE_IMAGE, ///PMI 검사 Vision 옵션을 Reference로 설정하였는데 패드 이미지가 없는 경우
    }

    public enum MarkStatusCodeEnum
    {
        PASS = 0,
        TOO_CLOSE_TO_EDGE,
        MARK_AREA_TOO_SMALL,
        MARK_AREA_TOO_BIG,
        MARK_SIZE_TOO_SMALL,
        MARK_SIZE_TOO_BIG
    }
    public enum DELEGATEONOFF
    {
        ON = 0,
        OFF
    }

    public enum PMIManualMode
    {
        DIE = 0,
        DUT
    }

    public enum PMIStateEnum
    {
        UNDEFINED = 0,
        IDLE,
        DONE,
        ERROR,
        RUNNING,
        RECOVERY,
        SUSPENDED,
        ABORT,
        PAUSED
    }

    public enum PMIImageCombineMode
    {
        BINARY = 0,
        ORIGINAL
    }

    //public enum OP_MODE
    //{
    //    Disable = 0,
    //    Normal,
    //    Sample
    //}
    public enum OP_MODE
    {
        Disable = 0,
        Enable
    }

    public enum PMI_PAUSE_METHOD
    {
        UNDEFINED = 0,
        NOTUSE,
        IMMEDIATELY,
        AFTER_ALL_DIE_INSPECTION
    }

    public enum OP_MODE_ON_RETEST
    {
        BeforeRetestOnly = 0,
        AfterRetestOnly,
        EnableBoth
    }

    public enum MARK_COMPARE_MODE
    {
        Area = 0,
        Size,
        AreaAndSize
    }

    public enum MARK_COMPARE_UNIT
    {
        Individual = 0,
        Sum,
        IndividualAndSum
    }

    public enum MARK_AREA_CALCULATE_MODE
    {        
        Square = 0
        , Convex // 홈 채우기
        //,Concave ==내부 채우기
        ,None //채우기 없음
    }
 
    public enum PAD_MARK_DIFFERENTIATION_MODE
    {
        /// <summary>
        /// 최초, 사용자가 설정하지 않음, 기존 옵션 마이그레이션 되지 않음을 표시               
        /// </summary>
        None = 0,
        /// <summary>
        /// 트라이앵글로 산출된 중간값으로 이진화
        /// </summary>
        SimpleThreshold,
        /// <summary>
        /// 패드의 노이즈 밝기를 계산하여 이진화
        /// </summary>
        PadNoiseAwareThreshold,
        /// <summary>
        /// 이전에 저장해둔 패드 이미지를 이용하기
        /// </summary>
        ReferenceImage
    }

    [DataContract]
    public enum LOGGING_MODE
    {
        [EnumMember]
        All = 0,
        [EnumMember]
        OnlyFail,
        [EnumMember]
        OnlyPass
    }

    public enum GROUPING_METHOD
    {
        Single = 0,
        Multi
    }

    public enum PAD_EDGE_OFFSET_MODE
    {
        DISABLE = 0,
        ENABLE
    }

    public enum PAD_CORNERRADIUS_MODE
    {
        DISABLE = 0,
        ENABLE
    }

    public enum PAD_JUDGING_WINDOW_MODE
    {
        ONEWAY,
        TWOWAY,
    }

    [DataContract]
    public enum PAD_SHAPE
    {
        [EnumMember]
        RECTANGLE = 0,
        [EnumMember]
        CIRCLE = 1,
        [EnumMember]
        DIAMOND = 2,
        [EnumMember]
        OVAL = 3,
        [EnumMember]
        OCTAGON = 4,
        [EnumMember]
        HALF_OCTAGON = 5,
        [EnumMember]
        ROUNDED_RECTANGLE = 6,
        [EnumMember]
        CUSTOM = 99
    }

    [DataContract]
    public enum PMI_SETUP_MODE
    {
        [EnumMember]
        UNDEFINED = 0,
        [EnumMember]
        NONE,
        [EnumMember]
        PAD,
        [EnumMember]
        JUDGINGWINDOW,
        [EnumMember]
        MARK,
        [EnumMember]
        MARKMIN,
        [EnumMember]
        MARKMAX,
        [EnumMember]
        TABLE
    }

    //public enum PMI_RENDER_TABLE_MODE
    //{
    //    DISABLE = 0,
    //    ENABLE
    //}

    public enum SELECT_DIRECTION
    {
        PREV = 0,
        NEXT
    }

    public enum SETUP_DIRECTION
    {
        PREV = 0,
        NEXT
    }

    public enum MAP_SELECT_MODE
    {
        DIE = 0,
        DUT
    }

    public enum SELECTION_MODE
    {
        SINGLE = 0,
        ALL
    }

    public enum JOG_DIRECTION
    {
        UP = 0,
        DOWN,
        LEFT,
        RIGHT
    }

    public enum PAD_COLOR
    {
        WHITE = 0,
        BLACK
    }

    public enum MARK_SIZE
    {
        MIN = 0,
        MAX
    }

    public enum PAD_USING
    {
        DISABLE = 0,
        ENABLE
    }

    public enum EnumMessageDialogTitle
    {
        UNKNOWN = 0,
        FAILED,
        SUCCESS,
        NOTIFY
    }

    public enum PadResultMoveMode
    {
        ALL = 0,
        PASS,
        FAIL
    }
    public class PMIGeometry
    {
        public Point Scale { get; set; }
        public Point Offset { get; set; }
        public Point Center { get; set; }
        public double Angle { get; set; }
        private Point RenderLayerRatio { get; set; }
        private Point RenderLayerSize { get; set; }
        public ScaleTransform ScaleT { get; set; }
        public TranslateTransform OffsetT { get; set; }
        public RotateTransform CenterT { get; set; }

        public Point ObjectSize { get; set; }
        public Geometry Geo { get; set; }

        //public CatCoordinates CurrentPos { get; set; }
        //public WaferCoordinate PadCenterPos { get; set; }
        public Geometry TransfomedGeo { get; set; }

        //public ScaleTransform CreateScaleTransform(Point Scale)
        //{
        //    return new ScaleTransform(Scale.X, Scale.Y);
        //}

        //public TranslateTransform CreateTranslateTransform(Point Offset)
        //{
        //    return new TranslateTransform(Offset.X, Offset.Y);
        //}

        //public RotateTransform CreateRotateTransform(double Angle, Point Center)
        //{
        //    return new RotateTransform(Angle, Center.X, Center.Y);
        //}

        public PMIGeometry(Point objectsize, double angle, Geometry geo, Point renderlayerratio, Point renderlayersize)
        {
            try
            {
                this.ObjectSize = objectsize;
                this.Angle = angle;
                this.Geo = geo;

                this.RenderLayerRatio = renderlayerratio;
                this.RenderLayerSize = renderlayersize;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void CalcScalePoint()
        {
            try
            {
                Scale = new Point(ObjectSize.X / Geo.Bounds.Width * RenderLayerRatio.X,
                                  ObjectSize.Y / Geo.Bounds.Height * RenderLayerRatio.Y);

                ScaleT = new ScaleTransform(Scale.X, Scale.Y);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void CalcOffsetPoint(double M = 0, double N = 0, double offsetx = 0, double offsety = 0)
        {
            try
            {
                Offset = new Point(-(Geo.Bounds.Left * Scale.X) + M - (Geo.Bounds.Width * Scale.X / 2),
                                  -(Geo.Bounds.Top * Scale.Y) + N - (Geo.Bounds.Height * Scale.Y / 2));

                OffsetT = new TranslateTransform(Offset.X + offsetx, Offset.Y + offsety);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void CalcCenterPoint()
        {
            try
            {
                Center = new Point((Offset.X + (Geo.Bounds.Width * Scale.X / 2)),
                                   (Offset.Y + (Geo.Bounds.Height * Scale.Y / 2)));

                CenterT = new RotateTransform(Angle, Center.X, Center.Y);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void GetTransformedGeometry(ScaleTransform scaleT = null, TranslateTransform translateT = null, RotateTransform rotateT = null)
        {
            try
            {
                TransformGroup group = new TransformGroup();

                if (scaleT != null)
                {
                    group.Children.Add(scaleT);
                }

                if (translateT != null)
                {
                    group.Children.Add(translateT);
                }

                if (rotateT != null)
                {
                    group.Children.Add(rotateT);
                }

                TransfomedGeo = Geo.Clone();
                TransfomedGeo.Transform = group;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    [Serializable]
    public class PMITriggerComponent : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private Element<int> _EveryWaferInterval = new Element<int>();
        public Element<int> EveryWaferInterval
        {
            get { return _EveryWaferInterval; }
            set
            {
                if (value != _EveryWaferInterval)
                {
                    _EveryWaferInterval = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _TotalNumberOfWafersToPerform = new Element<int>();
        public Element<int> TotalNumberOfWafersToPerform
        {
            get { return _TotalNumberOfWafersToPerform; }
            set
            {
                if (value != _TotalNumberOfWafersToPerform)
                {
                    _TotalNumberOfWafersToPerform = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _TouchdownCountInterval = new Element<int>();
        public Element<int> TouchdownCountInterval
        {
            get { return _TouchdownCountInterval; }
            set
            {
                if (value != _TouchdownCountInterval)
                {
                    _TouchdownCountInterval = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// 자세히 설명 적을 것
        /// </summary>
        private Element<bool> _ExecuteAfterWaferProcessed = new Element<bool>();
        public Element<bool> ExecuteAfterWaferProcessed
        {
            get { return _ExecuteAfterWaferProcessed; }
            set
            {
                if (value != _ExecuteAfterWaferProcessed)
                {
                    _ExecuteAfterWaferProcessed = value;
                    RaisePropertyChanged();
                }
            }
        }

        public PMITriggerComponent()
        {
            if (EveryWaferInterval != null)
            {
                EveryWaferInterval.Value = 0;
            }

            if (TotalNumberOfWafersToPerform != null)
            {
                TotalNumberOfWafersToPerform.Value = 0;
            }

            if (TouchdownCountInterval != null)
            {
                TouchdownCountInterval.Value = 0;
            }
        }
    }

    #endregion
}
