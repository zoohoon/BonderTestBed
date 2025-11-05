using ProberInterfaces.Vision;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Runtime.CompilerServices;
using System.Windows;
using ProberInterfaces.PMI;
using LogModule;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace ProberInterfaces.Param
{
    [Serializable, DataContract]
    [XmlInclude(typeof(PadObject))]
    public class PadObject : INotifyPropertyChanged, IParamNode
    {
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public PadObject()
        {
            try
            {
                //PadCenter = new PadCoordinate(0, 0);
                //PadColor = EnumPadColorType.UNDEFINED;
                //PadShape = EnumPadShapeType.UNDEFINED;
                //PadSizeX = 0;
                //PadSizeY = 0;
                //Index = 0;
                PadGuid.Value = Guid.NewGuid();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public PadObject(double padsizex, double padsizey)
        {
            try
            {
                PadSizeX.Value = padsizex;
                PadSizeY.Value = padsizey;
                PadColor.Value = EnumPadColorType.UNDEFINED;
                PadShape.Value = EnumPadShapeType.UNDEFINED;
                Index.Value = 0;
                PadGuid.Value = Guid.NewGuid();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public PadObject(double padsizex, double padsizey, PadCoordinate padcenter)
        {
            try
            {
                PadColor.Value = EnumPadColorType.UNDEFINED;
                PadShape.Value = EnumPadShapeType.UNDEFINED;
                PadSizeX.Value = 0;
                PadSizeY.Value = 0;
                Index.Value = 0;
                PadGuid.Value = Guid.NewGuid();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void CopyTo(PadObject target)
        {
            try
            {
                target.PadColor = PadColor;
                target.PadShape = PadShape;

                if (target.PadCenter == null) target.PadCenter = new WaferCoordinate();
                PadCenter.CopyTo(target.PadCenter);

                target.PadSizeX = PadSizeX;
                target.PadSizeY = PadSizeY;
                target.Index = Index;

                if (target.BlobParam == null) target.BlobParam = new BlobParameter();
                BlobParam.CopyTo(target.BlobParam);

                if (target.MachineIndex == null) target.MachineIndex = new MachineIndex();
                MachineIndex.CopyTo(target.MachineIndex);

                target.PadGuid = PadGuid;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void DefaultSetting()
        {
            try
            {

                BlobParam.BlobMinRadius.Value = 3;
                BlobParam.BlobThreshHold.Value = 120;
                BlobParam.MinBlobArea.Value = 50;
                PadColor.Value = EnumPadColorType.UNDEFINED;
                PadShape.Value = EnumPadShapeType.UNDEFINED;
                PadSizeX.Value = 0;
                PadSizeY.Value = 0;
                Index.Value = 0;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private Element<EnumPadColorType> _PadColor = new Element<EnumPadColorType>();
        [DataMember]
        public Element<EnumPadColorType> PadColor
        {
            get { return _PadColor; }
            set
            {
                if (value != _PadColor)
                {
                    _PadColor = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<EnumPadShapeType> _PadShape = new Element<EnumPadShapeType>();
        [DataMember]
        public Element<EnumPadShapeType> PadShape
        {
            get { return _PadShape; }
            set
            {
                if (value != _PadShape)
                {
                    _PadShape = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private PadCoordinate _PadCenter = new PadCoordinate();
        //public PadCoordinate PadCenter
        //{
        //    get { return _PadCenter; }
        //    set
        //    {
        //        if (value != _PadCenter)
        //        {
        //            _PadCenter = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        /// <summary>
        ///  등록시 속한 Die 의 LeftCorenr 로 부터 PadCenter까지 상대거리
        /// </summary>
        private WaferCoordinate _PadCenter = new WaferCoordinate();
        [DataMember]
        public WaferCoordinate PadCenter
        {
            get { return _PadCenter; }
            set
            {
                if (value != _PadCenter)
                {
                    _PadCenter = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _PadSizeX = new Element<double>();
        [DataMember]
        public Element<double> PadSizeX
        {
            get { return _PadSizeX; }
            set
            {
                if (value != _PadSizeX)
                {
                    _PadSizeX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _PadSizeY = new Element<double>();
        [DataMember]
        public Element<double> PadSizeY
        {
            get { return _PadSizeY; }
            set
            {
                if (value != _PadSizeY)
                {
                    _PadSizeY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _Index = new Element<int>();
        [DataMember]
        public Element<int> Index
        {
            get { return _Index; }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                    RaisePropertyChanged();
                }
            }
        }

        private BlobParameter _BlobParam = new BlobParameter();
        [DataMember]
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

        private MachineIndex _MachineIndex = new MachineIndex();
        [DataMember]
        public MachineIndex MachineIndex
        {
            get { return _MachineIndex; }
            set
            {
                if (value != _MachineIndex)
                {
                    _MachineIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<Guid> _PadGuid = new Element<Guid>();
        [DataMember]
        public Element<Guid> PadGuid
        {
            get { return _PadGuid; }
            set
            {
                if (value != _PadGuid)
                {
                    _PadGuid = value;
                    RaisePropertyChanged();
                }
            }
        }

        [XmlIgnore, JsonIgnore]
        public virtual string Genealogy { get; set; }
        [XmlIgnore, JsonIgnore]
        public virtual object Owner { get; set; }
        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }

        [XmlIgnore, JsonIgnore]
        public ImageBuffer MaskingBuf { get; set; }
    }

    [Serializable, DataContract]
    [XmlInclude(typeof(DUTPadObject))]
    public class DUTPadObject : PadObject, INotifyPropertyChanged, IParamNode
    {
        [field: NonSerialized, JsonIgnore]
        public new event PropertyChangedEventHandler PropertyChanged;
        protected new void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public DUTPadObject()
        {
            PadGuid.Value = Guid.NewGuid();
        }

        private MachineIndex _DutMIndex;
        [DataMember]
        public MachineIndex DutMIndex
        {
            get { return _DutMIndex; }
            set
            {
                if (value != _DutMIndex)
                {
                    _DutMIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _DutNumber;
        [DataMember]
        public long DutNumber
        {
            get { return _DutNumber; }
            set
            {
                if (value != _DutNumber)
                {
                    _DutNumber = value;
                    RaisePropertyChanged();
                }
            }
        }



        private WaferCoordinate _MIndexLCWaferCoord;
        [DataMember]
        public WaferCoordinate MIndexLCWaferCoord
        {
            get { return _MIndexLCWaferCoord; }
            set
            {
                if (value != _MIndexLCWaferCoord)
                {
                    _MIndexLCWaferCoord = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _PadNumber = new Element<int>();
        [DataMember]
        public Element<int> PadNumber
        {
            get { return _PadNumber; }
            set
            {
                if (value != _PadNumber)
                {
                    _PadNumber = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// 1번Dut의 LeftCorenr 로 부터 PadCenter까지 상대거리
        /// </summary>
        private WaferCoordinate _PadCenterRef = new WaferCoordinate();
        [DataMember]
        public WaferCoordinate PadCenterRef
        {
            get { return _PadCenterRef; }
            set
            {
                if (value != _PadCenterRef)
                {
                    _PadCenterRef = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    [Serializable, DataContract]
    [XmlInclude(typeof(PMIPadObject))]
    public class PMIPadObject : PadObject, INotifyPropertyChanged, IParamNode
    {
        [field: NonSerialized, JsonIgnore]
        public new event PropertyChangedEventHandler PropertyChanged;
        protected new void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        //[XmlIgnore, JsonIgnore]
        //public virtual string Genealogy { get; set; }
        //[XmlIgnore, JsonIgnore]
        //public virtual object Owner { get; set; }
        //[XmlIgnore, JsonIgnore]
        //public List<object> Nodes { get; set; }

        private PadTemplate _PadInfos;
        [DataMember]
        public PadTemplate PadInfos
        {
            get { return _PadInfos; }
            set
            {
                if (value != _PadInfos)
                {
                    _PadInfos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _PMIPadTemplateIndex = new Element<int>();
        [DataMember]
        public Element<int> PMIPadTemplateIndex
        {
            get { return _PMIPadTemplateIndex; }
            set
            {
                if (value != _PMIPadTemplateIndex)
                {
                    _PMIPadTemplateIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private int _SelectedPMIPadResultIndex;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public int SelectedPMIPadResultIndex
        {
            get { return _SelectedPMIPadResultIndex; }
            set
            {
                if (value != _SelectedPMIPadResultIndex)
                {
                    _SelectedPMIPadResultIndex = value;
                    RaisePropertyChanged();
                }

                if ((PMIResults.Count > 0) && (_SelectedPMIPadResultIndex >= 0))
                {
                    SelectedPMIPadResult = PMIResults[_SelectedPMIPadResultIndex];
                }
            }
        }

        [NonSerialized]
        private PMIPadResult _SelectedPMIPadResult;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public PMIPadResult SelectedPMIPadResult
        {
            get { return _SelectedPMIPadResult; }
            set
            {
                if (value != _SelectedPMIPadResult)
                {
                    _SelectedPMIPadResult = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private List<PMIPadResult> _PMIResults = new List<PMIPadResult>();
        [XmlIgnore, JsonIgnore]
        public List<PMIPadResult> PMIResults
        {
            get { return _PMIResults; }
            set
            {
                if (value != _PMIResults)
                {
                    _PMIResults = value;
                    RaisePropertyChanged();
                }
            }
        }


        [NonSerialized]
        private Rect _BoundingBox = new Rect();
        [XmlIgnore, JsonIgnore]
        public Rect BoundingBox
        {
            get { return _BoundingBox; }
            set
            {
                if (value != _BoundingBox)
                {
                    _BoundingBox = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private double _PadArea;
        [XmlIgnore, JsonIgnore]
        public double PadArea
        {
            get { return _PadArea; }
            set
            {
                if (value != _PadArea)
                {
                    _PadArea = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private Element<int> _PadRealIndex = new Element<int>();
        //[DataMember]
        //public Element<int> PadRealIndex
        //{
        //    get { return _PadRealIndex; }
        //    set
        //    {
        //        if (value != _PadRealIndex)
        //        {
        //            _PadRealIndex = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        public PMIPadObject()
        {
            try
            {
                PadGuid.Value = Guid.NewGuid();
                PMIResults = new List<PMIPadResult>();
                _BoundingBox = new Rect();
                PadInfos = new PadTemplate();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public PMIPadObject(PadTemplate template)
        {
            try
            {
                PadGuid.Value = Guid.NewGuid();
                PMIResults = new List<PMIPadResult>();
                _BoundingBox = new Rect();
                PadInfos = new PadTemplate(template);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void CopyTo(PMIPadObject target)
        {
            try
            {
                // Base
                target.PadColor = PadColor;
                target.PadShape = PadShape;

                if (target.PadCenter == null) target.PadCenter = new WaferCoordinate();
                PadCenter.CopyTo(target.PadCenter);

                target.PadSizeX = PadSizeX;
                target.PadSizeY = PadSizeY;
                target.Index = Index;

                if (target.BlobParam == null) target.BlobParam = new BlobParameter();
                BlobParam.CopyTo(target.BlobParam);

                // PMI
                //target.PadPathData = PadPathData;
                //target.PadAngle = PadAngle;
                target.PMIPadTemplateIndex = PMIPadTemplateIndex;

                // CopyTo
                //target.PMIResults = PMIResults;

                if (target.PMIResults == null) target.PMIResults = new List<PMIPadResult>();

                //target.PMIResults.Clear();

                foreach (var r in PMIResults)
                {
                    PMIPadResult padresult = new PMIPadResult();

                    r.CopyTo(padresult);

                    target.PMIResults.Add(padresult);
                }

                target.BoundingBox = BoundingBox;

                target.PadInfos = PadInfos;

                //target.PadRealIndex.Value = this.PadRealIndex.Value;

                target.MachineIndex = new MachineIndex(MachineIndex.XIndex, MachineIndex.YIndex);

                //LoggerManager.Debug($"Index = {PadRealIndex}, HashCode = {PMIResults.GetHashCode()}");
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PMIPadObject] [SetPMIPadData()] : {err}");
                LoggerManager.Exception(err);
            }
        }

        //public void DefaultSetting()
        //{
        //    try
        //    {
        //        BlobParam.BlobMinRadius.Value = 3;
        //        BlobParam.BlobThreshHold.Value = 120;
        //        BlobParam.MinBlobArea.Value = 50;
        //        PadColor.Value = EnumPadColorType.UNDEFINED;
        //        PadShape.Value = EnumPadShapeType.UNDEFINED;
        //        PadSizeX.Value = 30;
        //        PadSizeY.Value = 30;
        //        Index.Value = 0;
        //        PadPathData.Value = "";
        //        //PadAngle.Value = 0;
        //        PMIPadTemplateIndex.Value = 0;
        //        PMIResults = new List<PMIPadResult>();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Debug($"[PMIPadObject] [DefaultSetting()] : {err}");
        //        LoggerManager.Exception(err);
        //    }
        //}


    }

    [Serializable, DataContract]
    [XmlInclude(typeof(PadGroup))]
    public class PadGroup : INotifyPropertyChanged, IParamNode
    {
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        [XmlIgnore, JsonIgnore]
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
        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }
        [XmlIgnore, JsonIgnore]
        public string FilePath { get; } = "";
        public PadGroup()
        {
            try
            {
                //ObservableCollection<PadParameter> padparams = new ObservableCollection<PadParameter>();

                //PadData = padparams;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// 1번Dut의 LeftCorner 로 부터 Pad들의 Center 까지의 상대거리 X
        /// </summary>
        private double _PadCenX;
        [DataMember]
        public double PadCenX
        {
            get { return _PadCenX; }
            set
            {
                if (value != _PadCenX)
                {
                    _PadCenX = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// 1번Dut의 LeftCorner 로 부터 Pad들의 Center 까지의 상대거리 Y
        /// </summary>
        private double _PadCenY;
        [DataMember]
        public double PadCenY
        {
            get { return _PadCenY; }
            set
            {
                if (value != _PadCenY)
                {
                    _PadCenY = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private PadObject _RefPad = new PadObject();
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public PadObject RefPad
        {
            get { return _RefPad; }
            set
            {
                if (value != _RefPad)
                {
                    _RefPad = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private int _Flag;
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public int Flag
        {
            get { return _Flag; }
            set
            {
                if (value != _Flag)
                {
                    _Flag = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IList<DUTPadObject> _DutPadInfos = new List<DUTPadObject>();
        [SharePropPath, DataMember]
        public IList<DUTPadObject> DutPadInfos
        {
            get { return _DutPadInfos; }
            set
            {
                if (value != _DutPadInfos)
                {
                    _DutPadInfos = value;
                    RaisePropertyChanged();
                }
            }
        }
        /// <summary>
        /// PadNumber != DutNumber, PinNum과 대응되는값임 주의!
        /// </summary>
        /// <param name="pinNum"></param>
        /// <returns></returns>
        public int GetPadArrayIndex(int pinNum)
        {
            // 넘겨 받은 핀 번호랑 맞는 패드가 있는 배열의 인덱스를 리턴한다.
            int idx = -1;
            try
            {
                
                idx = DutPadInfos.ToList().FindIndex(pad => pad.PadNumber.Value == (pinNum + 1));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return idx;
        }

        //[NonSerialized]
        //private int _SelectedPMIPadIndex;
        //[ParamIgnore]
        //[XmlIgnore, JsonIgnore]
        //public int SelectedPMIPadIndex
        //{
        //    get { return _SelectedPMIPadIndex; }
        //    set
        //    {
        //        if (value != _SelectedPMIPadIndex)
        //        {
        //            _SelectedPMIPadIndex = value;
        //            RaisePropertyChanged();
        //        }

        //        if ((PMIPadInfos.Count > 0) && (_SelectedPMIPadIndex >= 0))
        //        {
        //            SelectedPMIPad = PMIPadInfos[_SelectedPMIPadIndex];
        //        }
        //    }
        //}

        //[NonSerialized]
        //private PMIPadObject _SelectedPMIPad;
        //[ParamIgnore]
        //[XmlIgnore, JsonIgnore]
        //public PMIPadObject SelectedPMIPad
        //{
        //    get { return _SelectedPMIPad; }
        //    set
        //    {
        //        if (value != _SelectedPMIPad)
        //        {
        //            _SelectedPMIPad = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private IList<PMIPadObject> _PMIPadInfos = new List<PMIPadObject>();
        [SharePropPath, DataMember]
        public IList<PMIPadObject> PMIPadInfos
        {
            get { return _PMIPadInfos; }
            set
            {
                if (value != _PMIPadInfos)
                {
                    _PMIPadInfos = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private PadObject _SetPadObject = new PadObject();
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public PadObject SetPadObject
        {
            get { return _SetPadObject; }
            set
            {
                if (value != _SetPadObject)
                {
                    _SetPadObject = value;
                    RaisePropertyChanged();
                }
            }
        }


        public void CopyTo(PadGroup target)
        {
            try
            {
                System.Diagnostics.Stopwatch sw = new Stopwatch();

                //sw.Reset();         // 초기화
                //sw.Start();

                if (target.RefPad == null) target.RefPad = new PadObject();
                RefPad.CopyTo(target.RefPad);

                //sw.Stop();          // 종료
                //Console.WriteLine("수행 시간 : {0}", sw.ElapsedMilliseconds / 1000.0F);

                target.Flag = Flag;
                target.SetPadObject.PadSizeX = SetPadObject.PadSizeX;
                target.SetPadObject.PadSizeY = SetPadObject.PadSizeY;

                //sw.Reset();         // 초기화
                //sw.Start();

                target.DutPadInfos.Clear();
                List<DUTPadObject> dutpads = DutPadInfos.ToList();
                ConcurrentBag<DUTPadObject> dutpadsBag = new ConcurrentBag<DUTPadObject>();

                //바로 target.DutPadInfos에 Add를 하면 병렬처리 때문에 Thread 충돌이 일어난다.
                //(List는 Thread에 안전하지 못하다.)
                //때문에 Thread에 안전한 ConcurrentBag에 마구자비로 넣고,
                //Parallel이 끝난 시점에 List에 넣어주도록 수정하였다.
                Parallel.For(0, dutpads.Count, i =>
                {
                    try
                    {
                        DUTPadObject pad = new DUTPadObject();
                        dutpads[i].CopyTo(pad);
                        dutpadsBag.Add(pad);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Debug("PadGroup CopyTo occure exception");
                        LoggerManager.Exception(err);
                    }

                });

                if (target.DutPadInfos is List<DUTPadObject>)
                {
                    (target.DutPadInfos as List<DUTPadObject>).AddRange(dutpadsBag);
                }
                else
                {
                    var dutPadInfoList = target.DutPadInfos.ToList();
                    dutPadInfoList.AddRange(dutpadsBag);
                    target.DutPadInfos = dutPadInfoList;
                }


                //sw.Stop();          // 종료
                //Console.WriteLine("수행 시간 : {0}", sw.ElapsedMilliseconds / 1000.0F);

                //target.PMIPadInfos.Clear();
                //Parallel.For(0, PMIPadInfos.Count, i =>
                //{
                //    PMIPadObject pad = new PMIPadObject();
                //    PMIPadInfos[i].CopyTo(pad);
                //    target.PMIPadInfos.Add(pad);
                //});

                //sw.Reset();         // 초기화
                //sw.Start();

                //target.PMIPadInfos = PMIPadInfos.AsParallel()
                //                            .Select(item =>
                //                            {
                //                                var ord = new PMIPadObject();
                //                                item.CopyTo(ord);
                //                                return ord;
                //                            }).ToList();

                //sw.Stop();          // 종료
                //Console.WriteLine("수행 시간 : {0}", sw.ElapsedMilliseconds / 1000.0F);

                //sw.Reset();         // 초기화
                //sw.Start();

                target.PMIPadInfos.Clear();

                foreach (var padinfo in PMIPadInfos)
                {
                    PMIPadObject pad = new PMIPadObject();
                    padinfo.CopyTo(pad);
                    target.PMIPadInfos.Add(pad);
                }

                //Parallel.For(0, PMIPadInfos.Count, i =>
                //{
                //    PMIPadObject pad = new PMIPadObject();
                //    PMIPadInfos[i].CopyTo(pad);
                //    target.PMIPadInfos.Add(pad);
                //});

                //sw.Stop();          // 종료
                //Console.WriteLine("수행 시간 : {0}", sw.ElapsedMilliseconds / 1000.0F);

                //target.SelectedPMIPadIndex = SelectedPMIPadIndex;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    [Serializable]
    [XmlInclude(typeof(PMIPadGroup))]
    public class PMIPadGroup : PadGroup
    {
        [field: NonSerialized, JsonIgnore]


        private IList<PMIPadObject> _PMIPads = new List<PMIPadObject>();
        public IList<PMIPadObject> PMIPads
        {
            get { return _PMIPads; }
            set
            {
                if (value != _PMIPads)
                {
                    _PMIPads = value;
                    RaisePropertyChanged();
                }
            }
        }

        public PMIPadGroup()
        {

        }
    }

    [Serializable]
    public class PMIResultes : INotifyPropertyChanged
    {
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public PMIResultes()
        {
            try
            {
                _TotalFailPadCount = 0;
                _TotalPassPadCount = 0;
                _TotalPMIPadCount = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        [NonSerialized]
        private long _TotalPMIPadCount;
        [XmlIgnore, JsonIgnore]
        public long TotalPMIPadCount
        {
            get { return _TotalPMIPadCount; }
            set { _TotalPMIPadCount = value; }
        }

        [NonSerialized]
        private long _TotalPassPadCount;
        [XmlIgnore, JsonIgnore]
        public long TotalPassPadCount
        {
            get { return _TotalPassPadCount; }
            set { _TotalPassPadCount = value; }
        }

        [NonSerialized]
        private long _TotalFailPadCount;
        [XmlIgnore, JsonIgnore]
        public long TotalFailPadCount
        {
            get { return _TotalFailPadCount; }
            set { _TotalFailPadCount = value; }
        }

        [NonSerialized]
        private ObservableCollection<PMIPadResult> _EachPadResultes;
        [XmlIgnore, JsonIgnore]
        public ObservableCollection<PMIPadResult> EachPadResultes
        {
            get { return _EachPadResultes; }
            set { _EachPadResultes = value; }
        }
    }

    //[Serializable]
    public class PMIPadResult : INotifyPropertyChanged
    {
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        [NonSerialized]
        private ObservableCollection<PadStatusCodeEnum> _PadStatus;
        [XmlIgnore, JsonIgnore]
        public ObservableCollection<PadStatusCodeEnum> PadStatus
        {
            get { return _PadStatus; }
            set
            {
                if (value != _PadStatus)
                {
                    _PadStatus = value;
                    RaisePropertyChanged();
                }
            }
        }


        [NonSerialized]
        private byte[] _Buffer;
        [XmlIgnore, JsonIgnore]
        public byte[] Buffer
        {
            get { return _Buffer; }
            set { _Buffer = value; }
        }

        [NonSerialized]
        private Rect _PadPosition;
        [XmlIgnore, JsonIgnore]
        public Rect PadPosition
        {
            get { return _PadPosition; }
            set
            {
                if (value != _PadPosition)
                {
                    _PadPosition = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private double _PadOffsetX;
        [XmlIgnore, JsonIgnore]
        public double PadOffsetX
        {
            get { return _PadOffsetX; }
            set
            {
                if (value != _PadOffsetX)
                {
                    _PadOffsetX = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private double _PadOffsetY;
        [XmlIgnore, JsonIgnore]
        public double PadOffsetY
        {
            get { return _PadOffsetY; }
            set
            {
                if (value != _PadOffsetY)
                {
                    _PadOffsetY = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private bool _IsSuccessDetectedPad;
        [XmlIgnore, JsonIgnore]
        public bool IsSuccessDetectedPad
        {
            get { return _IsSuccessDetectedPad; }
            set
            {
                if (value != _IsSuccessDetectedPad)
                {
                    _IsSuccessDetectedPad = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private int _SelectedPMIMarkIndex;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public int SelectedPMIMarkIndex
        {
            get { return _SelectedPMIMarkIndex; }
            set
            {
                if (value != _SelectedPMIMarkIndex)
                {
                    _SelectedPMIMarkIndex = value;
                    RaisePropertyChanged();
                }

                if ((MarkResults.Count > 0) && (_SelectedPMIMarkIndex >= 0))
                {
                    SelectedPMIMark = MarkResults[_SelectedPMIMarkIndex];
                }
            }
        }

        [NonSerialized]
        private PMIMarkResult _SelectedPMIMark;
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public PMIMarkResult SelectedPMIMark
        {
            get { return _SelectedPMIMark; }
            set
            {
                if (value != _SelectedPMIMark)
                {
                    _SelectedPMIMark = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private ObservableCollection<PMIMarkResult> _MarkResults;
        [XmlIgnore, JsonIgnore]
        public ObservableCollection<PMIMarkResult> MarkResults
        {
            get { return _MarkResults; }
            set { _MarkResults = value; }
        }

        [NonSerialized]
        private WaferCoordinate _UsedWaferPosition;
        [XmlIgnore, JsonIgnore]
        public WaferCoordinate UsedWaferPosition
        {
            get { return _UsedWaferPosition; }
            set { _UsedWaferPosition = value; }
        }

        public PMIPadResult()
        {
            MarkResults = new ObservableCollection<PMIMarkResult>();
        }

        public PMIPadResult(
            long DetectedMarkCount,
            long FailMarkCount,
            long PassMarkCount,
            Rect PadPosition,
            ObservableCollection<PMIMarkResult> MarkResults,
            ObservableCollection<PadStatusCodeEnum> status)
        {
            try
            {
                //_SizeX = SizeX;
                //_SizeY = SizeY;
                //_DetectedMarkCount = DetectedMarkCount;
                //_FailMarkCount = FailMarkCount;
                //_PassMarkCount = PassMarkCount;
                _PadPosition = PadPosition;
                _MarkResults = MarkResults;
                _PadStatus = status;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void ClearResult()
        {
            try
            {
                _PadStatus = new ObservableCollection<PadStatusCodeEnum>();
                _PadPosition = new Rect();
                _IsSuccessDetectedPad = false;
                //_DetectedMarkCount = 0;
                //_FailMarkCount = 0;
                //_PassMarkCount = 0;
                _MarkResults = new ObservableCollection<PMIMarkResult>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void CopyTo(PMIPadResult target)
        {
            try
            {
                if (target.PadStatus == null)
                {
                    target.PadStatus = new ObservableCollection<PadStatusCodeEnum>();
                }

                target.PadStatus.Clear();

                if(PadStatus != null && PadStatus.Count > 0)
                {
                    foreach (var ps in PadStatus)
                    {
                        target.PadStatus.Add(ps);
                    }
                }

                target.Buffer = Buffer;
                target.PadPosition = PadPosition;
                target.IsSuccessDetectedPad = IsSuccessDetectedPad;
                target.SelectedPMIMarkIndex = SelectedPMIMarkIndex;
                target.SelectedPMIMark = SelectedPMIMark;
                target.PadOffsetX = PadOffsetX;
                target.PadOffsetY = PadOffsetY;

                if (target.MarkResults == null) target.MarkResults = new ObservableCollection<PMIMarkResult>();

                target.MarkResults.Clear();

                if(MarkResults != null && MarkResults.Count > 0)
                {
                    foreach (var r in MarkResults)
                    {
                        PMIMarkResult markresult = new PMIMarkResult();

                        r.CopyTo(markresult);

                        target.MarkResults.Add(markresult);
                    }
                }

                target.UsedWaferPosition = UsedWaferPosition;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    [Serializable, DataContract]
    public class MarkProximity : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private double _Top;
        [DataMember]
        public double Top
        {
            get { return _Top; }
            set
            {
                if (value != _Top)
                {
                    _Top = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _Bottom;
        [DataMember]
        public double Bottom
        {
            get { return _Bottom; }
            set
            {
                if (value != _Bottom)
                {
                    _Bottom = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _Right;
        [DataMember]
        public double Right
        {
            get { return _Right; }
            set
            {
                if (value != _Right)
                {
                    _Right = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _Left;
        [DataMember]
        public double Left
        {
            get { return _Left; }
            set
            {
                if (value != _Left)
                {
                    _Left = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    [Serializable]
    public class PMIMarkResult
    {
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        [NonSerialized]
        private int _Index;
        [XmlIgnore, JsonIgnore]
        public int Index
        {
            get { return _Index; }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private bool _IsPass;
        [XmlIgnore, JsonIgnore]
        public bool IsPass
        {
            get { return _IsPass; }
            set
            {
                if (value != _IsPass)
                {
                    _IsPass = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private ObservableCollection<MarkStatusCodeEnum> _Status;
        [XmlIgnore, JsonIgnore]
        public ObservableCollection<MarkStatusCodeEnum> Status
        {
            get { return _Status; }
            set
            {
                if (value != _Status)
                {
                    _Status = value;
                    RaisePropertyChanged();
                }
            }
        }


        [NonSerialized]
        private double _Width;
        [XmlIgnore, JsonIgnore]
        public double Width
        {
            get { return _Width; }
            set
            {
                if (value != _Width)
                {
                    _Width = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private double _Height;
        [XmlIgnore, JsonIgnore]
        public double Height
        {
            get { return _Height; }
            set
            {
                if (value != _Height)
                {
                    _Height = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private Rect _MarkPosPixel;
        [XmlIgnore, JsonIgnore]
        public Rect MarkPosPixel
        {
            get { return _MarkPosPixel; }
            set
            {
                if (value != _MarkPosPixel)
                {
                    _MarkPosPixel = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private Rect _MarkPosUmFromLT;
        [XmlIgnore, JsonIgnore]
        public Rect MarkPosUmFromLT
        {
            get { return _MarkPosUmFromLT; }
            set
            {
                if (value != _MarkPosUmFromLT)
                {
                    _MarkPosUmFromLT = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        ///패드의 Center로부터 마크의 Center까지의 거리 X (Positive : Right, Negative : Left) 
        /// 패드의 Center로부터 마크의 Center까지의 거리 Y (Positive : Top, Negative : Bottom)
        /// </summary>
        [NonSerialized]
        private Point _ScrubCenter;
        [XmlIgnore, JsonIgnore]
        public Point ScrubCenter
        {
            get { return _ScrubCenter; }
            set
            {
                if (value != _ScrubCenter)
                {
                    _ScrubCenter = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private double _ScrubAreaPx;
        /// <summary>
        /// 검출된 마크의 영역 Px기준
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public double ScrubAreaPx
        {
            get { return _ScrubAreaPx; }
            set
            {
                if (value != _ScrubAreaPx)
                {
                    _ScrubAreaPx = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private double _ScrubArea;
        /// <summary>
        /// 검출된 마크의 영역 convex hull
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public double ScrubArea
        {
            get { return _ScrubArea; }
            set
            {
                if (value != _ScrubArea)
                {
                    _ScrubArea = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private double _ScrubAreaPercent;
        
        /// <summary>
        /// 패드 면적대비 마크의 면적 비율
        /// </summary>
        /// [NonSerialized]

        [XmlIgnore, JsonIgnore]
        public double ScrubAreaPercent
        {
            get { return _ScrubAreaPercent; }
            set
            {
                if (value != _ScrubAreaPercent)
                {
                    _ScrubAreaPercent = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Unit : um
        /// </summary>
        //[NonSerialized]
        //private Rect _DistanceFromPadEdge;
        //[XmlIgnore, JsonIgnore]
        //public Rect DistanceFromPadEdge
        //{
        //    get { return _DistanceFromPadEdge; }
        //    set
        //    {
        //        if (value != _DistanceFromPadEdge)
        //        {
        //            _DistanceFromPadEdge = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        /// <summary>
        /// Unit : um
        /// </summary>
        [NonSerialized]
        private MarkProximity _MarkProximity;
        [XmlIgnore, JsonIgnore]
        public MarkProximity MarkProximity
        {
            get { return _MarkProximity; }
            set
            {
                if (value != _MarkProximity)
                {
                    _MarkProximity = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private double _PercentageForPadSize;
        [XmlIgnore, JsonIgnore]
        public double PercentageForPadSize
        {
            get { return _PercentageForPadSize; }
            set
            {
                if (value != _PercentageForPadSize)
                {
                    _PercentageForPadSize = value;
                    RaisePropertyChanged();
                }
            }
        }

        public PMIMarkResult()
        {
            try
            {
                _Status = new ObservableCollection<MarkStatusCodeEnum>();
                _Width = 0;
                _Height = 0;
                _MarkPosPixel = new Rect();
                _MarkProximity = new MarkProximity();
                _PercentageForPadSize = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public PMIMarkResult(
            double Width,
            double Height,
            double PercentageForPadSize,
            Rect MarkPosition,
            ObservableCollection<MarkStatusCodeEnum> Status)
        {
            try
            {
                _Width = Width;
                _Height = Height;
                _PercentageForPadSize = PercentageForPadSize;
                _MarkPosPixel = MarkPosition;
                _Status = Status;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void CopyTo(PMIMarkResult target)
        {
            try
            {
                target.IsPass = IsPass;

                if (target.Status == null)
                {
                    target.Status = new ObservableCollection<MarkStatusCodeEnum>();
                }

                target.Status.Clear();

                foreach (var s in Status)
                {
                    target.Status.Add(s);
                }

                target.Width = Width;
                target.Height = Height;
                target.MarkPosPixel = MarkPosPixel;
                target.MarkPosUmFromLT = MarkPosUmFromLT;
                target.ScrubCenter = ScrubCenter;
                target.ScrubArea = ScrubArea;
                target.ScrubAreaPercent = ScrubAreaPercent;
                target.ScrubAreaPx = ScrubAreaPx;

                if (target.MarkProximity == null)
                {
                    target.MarkProximity = new MarkProximity();
                }

                target.MarkProximity.Top = MarkProximity.Top;
                target.MarkProximity.Bottom = MarkProximity.Bottom;
                target.MarkProximity.Right = MarkProximity.Right;
                target.MarkProximity.Left = MarkProximity.Left;

                target.PercentageForPadSize = PercentageForPadSize;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}


