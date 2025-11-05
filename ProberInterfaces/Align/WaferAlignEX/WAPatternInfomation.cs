using LogModule;
using System.Collections.Generic;

namespace ProberInterfaces.WaferAlignEX
{
    using System.ComponentModel;
    using ProberInterfaces.Param;
    using System;
    using System.Collections.ObjectModel;
    using ProberErrorCode;

    using System.Runtime.CompilerServices;
    using Newtonsoft.Json;
    using System.Runtime.Serialization;

    public enum EnumVerDirection
    {
        UPPERBOTTOM,
        BOTTOMUPPER
    }
    public enum EnumHorDirection
    {
        LEFTRIGHT,
        RIGHTLEFT
    }

    [Serializable]
    public abstract class WAPatternInfomation : PatternInfomation, INotifyPropertyChanged, IParamNode
    {

        private Element<EnumWAProcDirection> _ProcDirection
             = new Element<EnumWAProcDirection>();
        [DataMember]
        public Element<EnumWAProcDirection> ProcDirection
        {
            get { return _ProcDirection; }
            set
            {
                if (value != _ProcDirection)
                {
                    _ProcDirection = value;
                    RaisePropertyChanged();
                }
            }
        }

        private MachineIndex _MIndex;
        [DataMember]
        public MachineIndex MIndex
        {
            get { return _MIndex; }
            set
            {
                if (value != _MIndex)
                {
                    _MIndex = value;
                    RaisePropertyChanged();
                }
            }
        }


        public WAPatternInfomation()
        {

        }
        public WAPatternInfomation(Element<EnumWAProcDirection> procDirection, MachineIndex mIndex)
        {
            try
            {
                ProcDirection = procDirection;
                MIndex = mIndex;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }



    [Serializable]
    public class WAStandardPTInfomation : WAPatternInfomation, INotifyPropertyChanged, IParamNode
    {
        public new List<object> Nodes { get; set; }

        [NonSerialized]
        private EventCodeEnum _ErrorCode;
        //[XmlIgnore, JsonIgnore]
        [ParamIgnore]
        [JsonIgnore]
        public EventCodeEnum ErrorCode
        {
            get { return _ErrorCode; }
            set
            {
                if (value != _ErrorCode)
                {
                    _ErrorCode = value;
                    RaisePropertyChanged();
                }
            }
        }

        //[NonSerialized]
        //private IFocusing _FocusingModel;
        ////[XmlIgnore, JsonIgnore]
        //[ParamIgnore]
        //[JsonIgnore]
        //public IFocusing FocusingModel
        //{
        //    get { return _FocusingModel; }
        //    set
        //    {
        //        if (value != _FocusingModel)
        //        {
        //            _FocusingModel = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}


        //[NonSerialized]
        //private IFocusParameter _FocusParam;
        //[ParamIgnore]
        //[JsonIgnore]
        //public IFocusParameter FocusParam
        //{
        //    get { return _FocusParam; }
        //    set
        //    {
        //        if (value != _FocusParam)
        //        {
        //            _FocusParam = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}


        private Element<EnumWAProcDirection> _PostProcDirection
             = new Element<EnumWAProcDirection>();
        [DataMember]
        public Element<EnumWAProcDirection> PostProcDirection
        {
            get { return _PostProcDirection; }
            set
            {
                if (value != _PostProcDirection)
                {
                    _PostProcDirection = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<EnumVerDirection> _PostVerDirection
     = new Element<EnumVerDirection>();
        [DataMember]
        public Element<EnumVerDirection> PostVerDirection
        {
            get { return _PostVerDirection; }
            set { _PostVerDirection = value; }
        }
        private Element<EnumHorDirection> _PostHorDirection
             = new Element<EnumHorDirection>();
        [DataMember]
        public Element<EnumHorDirection> PostHorDirection
        {
            get { return _PostHorDirection; }
            set { _PostHorDirection = value; }
        }


        private bool _EnablePostJumpindex = true;
        [DataMember]
        public bool EnablePostJumpindex
        {
            get { return _EnablePostJumpindex; }
            set
            {
                if (value != _EnablePostJumpindex)
                {
                    _EnablePostJumpindex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<StandardJumpIndexParam> _PostJumpIndex
             = new ObservableCollection<StandardJumpIndexParam>();
        [DataMember]
        public ObservableCollection<StandardJumpIndexParam> PostJumpIndex
        {
            get { return _PostJumpIndex; }
            set
            {
                if (value != _PostJumpIndex)
                {
                    _PostJumpIndex = value;
                    RaisePropertyChanged();
                }
            }
        }



        private ObservableCollection<StandardJumpIndexParam> _JumpIndexs;
        [DataMember]
        public ObservableCollection<StandardJumpIndexParam> JumpIndexs
        {
            get { return _JumpIndexs; }
            set
            {
                if (value != _JumpIndexs)
                {
                    _JumpIndexs = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _AcceptFocusing = new Element<bool>();
        [DataMember]
        public Element<bool> AcceptFocusing
        {
            get { return _AcceptFocusing; }
            set
            {
                if (value != _AcceptFocusing)
                {
                    _AcceptFocusing = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _AccptPreSetFocusing = new Element<bool>();
        [DataMember]
        public Element<bool> AccptPreSetFocusing
        {
            get { return _AccptPreSetFocusing; }
            set
            {
                if (value != _AccptPreSetFocusing)
                {
                    _AccptPreSetFocusing = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<EnumVerDirection> _VerDirection
             = new Element<EnumVerDirection>();
        [DataMember]
        public Element<EnumVerDirection> VerDirection
        {
            get { return _VerDirection; }
            set { _VerDirection = value; }
        }
        private Element<EnumHorDirection> _HorDirection
             = new Element<EnumHorDirection>();
        [DataMember]
        public Element<EnumHorDirection> HorDirection
        {
            get { return _HorDirection; }
            set { _HorDirection = value; }
        }

        /// <summary>
        /// 저장될 당시의 기준 WaferCenter
        /// </summary>
        private WaferCoordinate _WaferCenter;
        [DataMember]
        public WaferCoordinate WaferCenter
        {
            get { return _WaferCenter; }
            set { _WaferCenter = value; }
        }

        /// <summary>
        /// ThetaAlign후 계산된 WaferCenter
        /// </summary>
        [NonSerialized]
        private WaferCoordinate _ProcWaferCenter
             = new WaferCoordinate();
        [ParamIgnore]
        [JsonIgnore]
        public WaferCoordinate ProcWaferCenter
        {
            get { return _ProcWaferCenter; }
            set { _ProcWaferCenter = value; }
        }



        public new string Genealogy { get; set; }

        public WAStandardPTInfomation()
        {

        }
        public WAStandardPTInfomation(string path)
        {
            this.PMParameter.ModelFilePath.Value = path;
        }
        public WAStandardPTInfomation(WAStandardPTInfomation info)
        {
            try
            {
                this.ErrorCode = info.ErrorCode;
                this.JumpIndexs = info.JumpIndexs;
                this.LightParams = info.LightParams;
                this.PMParameter.ModelFilePath = info.PMParameter.ModelFilePath;
                this.PatternState = info.PatternState;
                this.PMParameter = info.PMParameter;
                this.ProcDirection = info.ProcDirection;
                this.MIndex = info.MIndex;
                this.X = info.X;
                this.Y = info.Y;
                this.Z = info.Z;
                this.T = info.T;
                this.HorDirection = HorDirection;
                this.VerDirection = VerDirection;
                this.Imagebuffer = info.Imagebuffer;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void CopyTo(WAStandardPTInfomation target)
        {
            try
            {
                target.ErrorCode = this.ErrorCode;
                //target.FocusingModel = this.FocusingModel;
                target.JumpIndexs = new ObservableCollection<StandardJumpIndexParam>();
                for (int index = 0; index < this.JumpIndexs.Count; index++)
                {
                    target.JumpIndexs.Add(new StandardJumpIndexParam(this.JumpIndexs[index]));
                }

                target.PostJumpIndex = new ObservableCollection<StandardJumpIndexParam>();
                for (int index = 0; index < this.PostJumpIndex.Count; index++)
                {
                    target.PostJumpIndex.Add(new StandardJumpIndexParam(this.PostJumpIndex[index]));
                }

                target.AcceptFocusing.Value = this.AcceptFocusing.Value;
                target.VerDirection.Value = this.VerDirection.Value;
                target.HorDirection.Value = this.HorDirection.Value;
                if (this.WaferCenter != null)
                {
                    target.WaferCenter = new WaferCoordinate();
                    this.WaferCenter.CopyTo(target.WaferCenter);
                }

                if (this.ProcWaferCenter != null)
                {
                    target.ProcWaferCenter = new WaferCoordinate();
                    this.ProcWaferCenter.CopyTo(target.ProcWaferCenter);
                }

                target.CamType.Value = this.CamType.Value;
                target.X.Value = this.GetX();
                target.Y.Value = this.GetY();
                target.Z.Value = this.GetZ();

                target.LightParams = new ObservableCollection<LightValueParam>();
                foreach (var lightparam in this.LightParams)
                {
                    target.LightParams.Add(lightparam);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }



    [Serializable()]
    public class VMStandardPTParam : PatternInfomation
    {

        [NonSerialized]
        private IFocusing _FocusingModel;
        //[XmlIgnore, JsonIgnore]
        [ParamIgnore]
        [JsonIgnore]
        public IFocusing FocusingModel
        {
            get { return _FocusingModel; }
            set
            {
                if (value != _FocusingModel)
                {
                    _FocusingModel = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<StandardJumpIndexs> _JumpIndexs
             = new ObservableCollection<StandardJumpIndexs>();
        public ObservableCollection<StandardJumpIndexs> JumpIndexs
        {
            get { return _JumpIndexs; }
            set
            {
                if (value != _JumpIndexs)
                {
                    _JumpIndexs = value;
                    RaisePropertyChanged();
                }
            }
        }

        public VMStandardPTParam()
        {

        }

        public VMStandardPTParam(StandardJumpIndexs pt)
        {

        }
    }

    [Serializable()]
    public class StandardJumpIndexs
    {
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<StandardJumpIndexParam> _Indexs
             = new ObservableCollection<StandardJumpIndexParam>();
        public ObservableCollection<StandardJumpIndexParam> Indexs
        {
            get { return _Indexs; }
            set
            {
                if (value != _Indexs)
                {
                    _Indexs = value;
                    RaisePropertyChanged();
                }
            }
        }

        public StandardJumpIndexs()
        {

        }

        public StandardJumpIndexs(long xindex, long yindex)
        {
            Indexs.Add(new StandardJumpIndexParam(xindex, yindex));
        }

        public StandardJumpIndexs(List<StandardJumpIndexParam> param)
        {
            try
            {
                foreach (var p in param)
                {
                    Indexs.Add(p);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

    }



    [Serializable()]
    public class StandardJumpIndexParam : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private IndexCoord _Index = new MachineIndex();
        [DataMember]
        public IndexCoord Index
        {
            get { return _Index; }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                    NotifyPropertyChanged("Index");
                }
            }
        }

        [NonSerialized]
        private IndexCoord _RetryIndex;
        [ParamIgnore, JsonIgnore]
        public IndexCoord RetryIndex
        {
            get { return _RetryIndex; }
            set
            {
                if (value != _RetryIndex)
                {
                    _RetryIndex = value;
                    NotifyPropertyChanged("RetryIndex");
                }
            }
        }

        private Element<bool> _AcceptFocusing
            = new Element<bool>();
        public Element<bool> AcceptFocusing
        {
            get { return _AcceptFocusing; }
            set
            {
                if (value != _AcceptFocusing)
                {
                    _AcceptFocusing = value;
                    NotifyPropertyChanged("AcceptFocusing");
                }
            }
        }

        private Element<bool> _AcceptProcessing
            = new Element<bool>(true);
        [ParamIgnore, JsonIgnore]
        public Element<bool> AcceptProcessing
        {
            get { return _AcceptProcessing; }
            set
            {
                if (value != _AcceptProcessing)
                {
                    _AcceptProcessing = value;
                    NotifyPropertyChanged("AcceptProcessing");
                }
            }
        }

        [NonSerialized]
        private double _HeightProfilingVal = -1;
        [ParamIgnore, JsonIgnore]
        public double HeightProfilingVal
        {
            get { return _HeightProfilingVal; }
            set
            {
                if (value != _HeightProfilingVal)
                {
                    _HeightProfilingVal = value;
                    NotifyPropertyChanged("HeightProfilingVal");
                }
            }
        }

        //[NonSerialized]
        //private FocusParameter _FocusParam;
        //[ParamIgnore]
        //public FocusParameter FocusParam
        //{
        //    get { return _FocusParam; }
        //    set
        //    {
        //        if (value != _FocusParam)
        //        {
        //            _FocusParam = value;
        //            NotifyPropertyChanged("FocusParam");
        //        }
        //    }
        //}

        //[NonSerialized]
        //private FocusingBase _FocusingModel;
        //[ParamIgnore]
        //public FocusingBase FocusingModel
        //{
        //    get { return _FocusingModel; }
        //    set
        //    {
        //        if (value != _FocusingModel)
        //        {
        //            _FocusingModel = value;
        //            NotifyPropertyChanged("FocusingModel");
        //        }
        //    }
        //}

        public StandardJumpIndexParam()
        {

        }
        public StandardJumpIndexParam(MachineIndex index)
        {
            try
            {
                this.Index = index;
                this.AcceptFocusing.Value = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public StandardJumpIndexParam(long xindex, long yindex)
        {
            this.Index = new MachineIndex(xindex, yindex);
        }

        public StandardJumpIndexParam(IndexCoord index, bool acceptFocusing)
        {
            try
            {
                this.Index = index;
                this.AcceptFocusing.Value = acceptFocusing;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public StandardJumpIndexParam(long xindex, long yindex, bool acceptFocusing)
        {
            try
            {
                this.Index = new MachineIndex(xindex, yindex);
                this.AcceptFocusing.Value = acceptFocusing;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public StandardJumpIndexParam(StandardJumpIndexParam param)
        {
            try
            {
                this.Index = new MachineIndex(param.Index.XIndex, param.Index.YIndex);
                this.AcceptFocusing.Value = param.AcceptFocusing.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

    }


}
