using System;

namespace ProberInterfaces.Param
{
    using System.ComponentModel;
    using System.Collections.Generic;
    using ProberInterfaces.Enum;
    using System.Xml.Serialization;
    using System.Runtime.Serialization;
    using ProberInterfaces.Vision;

    using System.Runtime.CompilerServices;
    using Newtonsoft.Json;
    using LogModule;

    [Serializable, DataContract]
    public class CatCoordinates : INotifyPropertyChanged, IParamNode
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

        public CatCoordinates()
        {
            try
            {
                //this.X.Value = 0;
                //this.Y.Value = 0;
                //this.Z.Value = -5000;
                //this.T.Value = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public CatCoordinates(double x, double y, double z)
        {
            try
            {
                X = new Element<double>();
                Y = new Element<double>();
                Z = new Element<double>();
                T = new Element<double>();
                this.X.Value = x;
                this.Y.Value = y;
                this.Z.Value = z;
                this.T.Value = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public CatCoordinates(double x, double y)
        {
            try
            {
                X = new Element<double>();
                Y = new Element<double>();
                Z = new Element<double>();
                T = new Element<double>();
                this.X.Value = x;
                this.Y.Value = y;
                this.Z.Value = 0;
                this.T.Value = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public CatCoordinates(double x, double y, double z, double t)
        {
            try
            {
                X = new Element<double>();
                Y = new Element<double>();
                Z = new Element<double>();
                T = new Element<double>();
                this.X.Value = x;
                this.Y.Value = y;
                this.Z.Value = z;
                this.T.Value = t;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void CopyTo(CatCoordinates target)
        {
            try
            {
                X.CopyTo(target.X);
                Y.CopyTo(target.Y);
                Z.CopyTo(target.Z);
                T.CopyTo(target.T);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public double GetX()
        {
            if (X == null) X = new Element<double>();
            return X.Value;
        }
        public double GetY()
        {
            if (Y == null) Y = new Element<double>();
            return Y.Value;
        }
        public double GetZ()
        {
            if (Z == null) Z = new Element<double>();
            return Z.Value;
        }
        public double GetT()
        {
            if (T == null) T = new Element<double>();
            return T.Value;
        }
        public double GetPZ()
        {
            if (PZ == null) PZ = new Element<double>();
            return PZ.Value;
        }


        private Element<double> _X = new Element<double>();
        [DataMember]
        public Element<double> X
        {
            get { return _X; }
            set
            {
                if (value != _X)
                {
                    _X = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _Y = new Element<double>();
        [DataMember]
        public Element<double> Y
        {
            get { return _Y; }
            set
            {
                if (value != _Y)
                {
                    _Y = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _Z = new Element<double>();
        [DataMember]
        public Element<double> Z
        {
            get { return _Z; }
            set
            {
                if (value != _Z)
                {
                    _Z = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _T = new Element<double>();
        [DataMember]
        public Element<double> T
        {
            get { return _T; }
            set
            {
                if (value != _T)
                {
                    _T = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<double> _PZ = new Element<double>();
        [DataMember]
        public Element<double> PZ
        {
            get { return _PZ; }
            set
            {
                if (value != _PZ)
                {
                    _PZ = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _Enable = new bool();
        [DataMember]
        public bool Enable
        {
            get { return _Enable; }
            set
            {
                if (value != _Enable)
                {
                    _Enable = value;
                    RaisePropertyChanged();
                }
            }
        }

        public void SetCoordinates(double x, double y, double z, double t, double pz = 0.0)
        {
            this.X.Value = x;
            this.Y.Value = y;
            this.Z.Value = z;
            this.T.Value = t;
            this.PZ.Value = pz;
        }

        public static CatCoordinates operator +(CatCoordinates b, CatCoordinates c)
        {
            CatCoordinates result = new CatCoordinates();
            result.X.Value = b.X.Value + c.X.Value;
            result.Y.Value = b.Y.Value + c.Y.Value;
            result.Z.Value = b.Z.Value + c.Z.Value;
            return result;
        }
        public static CatCoordinates operator -(CatCoordinates b, CatCoordinates c)
        {
            CatCoordinates result = new CatCoordinates();
            result.X.Value = b.X.Value - c.X.Value;
            result.Y.Value = b.Y.Value - c.Y.Value;
            result.Z.Value = b.Z.Value - c.Z.Value;
            return result;
        }

    }
    [Serializable]
    public class WaferCoordinate : CatCoordinates, INotifyPropertyChanged, IParamNode
    {
        //public event PropertyChangedEventHandler PropertyChanged;1

        //private void NotifyPropertyChanged(String info)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        //}


        public WaferCoordinate(double x, double y)
        {
            try
            {
                this.X.Value = x;
                this.Y.Value = y;
                this.Z.Value = 0;
                this.T.Value = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public WaferCoordinate(double x, double y, double z)
        {
            try
            {
                this.X.Value = x;
                this.Y.Value = y;
                this.Z.Value = z;
                this.T.Value = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public WaferCoordinate(double x, double y, double z, double t)
        {
            try
            {
                this.X.Value = x;
                this.Y.Value = y;
                this.Z.Value = z;
                this.T.Value = t;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public WaferCoordinate(WaferCoordinate coordinate)
        {
            try
            {
                this.X.Value = coordinate.X.Value;
                this.Y.Value = coordinate.Y.Value;
                this.Z.Value = coordinate.Z.Value;
                this.T.Value = coordinate.T.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public WaferCoordinate(CatCoordinates coordinate)
        {
            try
            {
                this.X.Value = coordinate.X.Value;
                this.Y.Value = coordinate.Y.Value;
                this.Z.Value = coordinate.Z.Value;
                this.T.Value = coordinate.T.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public WaferCoordinate()
        {

        }

        public int CompareTo(object obj)
        {
            try
            {
                WaferCoordinate compareTarget;
                if (obj is WaferCoordinate)
                {
                    compareTarget = (WaferCoordinate)obj;

                    if (X.Value > compareTarget.X.Value)
                    {
                        return 1;
                    }
                    else if (X.Value < compareTarget.X.Value)
                    {
                        return -1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                return 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public static WaferCoordinate operator +(WaferCoordinate b, WaferCoordinate c)
        {
            WaferCoordinate result = new WaferCoordinate();
            result.X.Value = b.X.Value + c.X.Value;
            result.Y.Value = b.Y.Value + c.Y.Value;
            result.Z.Value = b.Z.Value + c.Z.Value;
            return result;
        }
        public static WaferCoordinate operator -(WaferCoordinate b, WaferCoordinate c)
        {
            WaferCoordinate result = new WaferCoordinate();
            result.X.Value = b.X.Value - c.X.Value;
            result.Y.Value = b.Y.Value - c.Y.Value;
            result.Z.Value = b.Z.Value - c.Z.Value;
            return result;
        }
    }

    [Serializable,DataContract]
    public class MachineCoordinate : CatCoordinates, IParamNode

    {
        public MachineCoordinate(double x, double y)
        {
            try
            {
                X = new Element<double>();
                Y = new Element<double>();
                Z = new Element<double>();
                T = new Element<double>();
                this.X.Value = x;
                this.Y.Value = y;
                this.Z.Value = 0;
                this.T.Value = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public MachineCoordinate(double x, double y, double z)
        {
            try
            {
                X = new Element<double>();
                Y = new Element<double>();
                Z = new Element<double>();
                T = new Element<double>();
                this.X.Value = x;
                this.Y.Value = y;
                this.Z.Value = z;
                this.T.Value = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public MachineCoordinate(double x, double y, double z, double pz, double t = 0.0)
        {
            try
            {
                X = new Element<double>();
                Y = new Element<double>();
                Z = new Element<double>();
                T = new Element<double>();
                PZ = new Element<double>();
                this.X.Value = x;
                this.Y.Value = y;
                this.Z.Value = z;
                this.T.Value = t;
                this.PZ.Value = pz;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public MachineCoordinate(double x, double y, double z, double t)
        {
            try
            {
                X = new Element<double>();
                Y = new Element<double>();
                Z = new Element<double>();
                T = new Element<double>();
                this.X.Value = x;
                this.Y.Value = y;
                this.Z.Value = z;
                this.T.Value = t;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public MachineCoordinate(MachineCoordinate coordinate)
        {
            try
            {
                X = new Element<double>();
                Y = new Element<double>();
                Z = new Element<double>();
                T = new Element<double>();
                this.X.Value = coordinate.X.Value;
                this.Y.Value = coordinate.Y.Value;
                this.Z.Value = coordinate.Z.Value;
                this.T.Value = coordinate.T.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public MachineCoordinate()
        {

        }
    }


    [Serializable]
    public class PinCoordinate : CatCoordinates, INotifyPropertyChanged, ICloneable, IParamNode
    {
        [NonSerialized]
        private bool _IsRefDutIn;
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public bool IsRefDutIn
        {
            get { return _IsRefDutIn; }
            set
            {
                if (value != _IsRefDutIn)
                {
                    _IsRefDutIn = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private bool _IsPadIn;
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public bool IsPadIn
        {
            get { return _IsPadIn; }
            set
            {
                if (value != _IsPadIn)
                {
                    _IsPadIn = value;
                    RaisePropertyChanged();
                }
            }
        }

        public PinCoordinate RotatePinPos(double degree)
        {
            double radian = Math.PI * degree / 180.0;
            double cosq = Math.Cos(radian);
            double sinq = Math.Sin(radian);
            double sx = X.Value;
            double sy = Y.Value;
            double rx = (sx * cosq - sy * sinq); // 결과 좌표 x
            double ry = (sx * sinq + sy * cosq); // 결과 좌표 y
            return new PinCoordinate(rx, ry, Z.Value);
        }

        public object Clone()
        {
            return new PinCoordinate { X = this.X, Y = this.Y, Z = this.Z, T = this.T };
        }

        public PinCoordinate(double x, double y)
        {
            try
            {
                X = new Element<double>();
                Y = new Element<double>();
                Z = new Element<double>();
                T = new Element<double>();
                this.X.Value = x;
                this.Y.Value = y;
                this.Z.Value = 0;
                this.T.Value = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public PinCoordinate(double x, double y, double z)
        {
            try
            {
                X = new Element<double>();
                Y = new Element<double>();
                Z = new Element<double>();
                T = new Element<double>();
                this.X.Value = x;
                this.Y.Value = y;
                this.Z.Value = z;
                this.T.Value = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public PinCoordinate(double x, double y, double z, double t)
        {
            try
            {
                X = new Element<double>();
                Y = new Element<double>();
                Z = new Element<double>();
                T = new Element<double>();
                this.X.Value = x;
                this.Y.Value = y;
                this.Z.Value = z;
                this.T.Value = t;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public PinCoordinate(PinCoordinate coordinate)
        {
            try
            {
                X = new Element<double>();
                Y = new Element<double>();
                Z = new Element<double>();
                T = new Element<double>();
                this.X.Value = coordinate.X.Value;
                this.Y.Value = coordinate.Y.Value;
                this.Z.Value = coordinate.Z.Value;
                this.T.Value = coordinate.T.Value;
                this.Enable = coordinate.Enable;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public PinCoordinate(CatCoordinates coordinate)
        {
            try
            {
                X = new Element<double>();
                Y = new Element<double>();
                Z = new Element<double>();
                T = new Element<double>();
                this.X.Value = coordinate.X.Value;
                this.Y.Value = coordinate.Y.Value;
                this.Z.Value = coordinate.Z.Value;
                this.T.Value = coordinate.T.Value;
                this.Enable = coordinate.Enable;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public PinCoordinate()
        {
            try
            {
                this.X.Value = 0;
                this.Y.Value = 0;
                this.Z.Value = 0;
                this.T.Value = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public static PinCoordinate operator +(PinCoordinate b, PinCoordinate c)
        {
            PinCoordinate result = new PinCoordinate();
            result.X.Value = b.X.Value + c.X.Value;
            result.Y.Value = b.Y.Value + c.Y.Value;
            result.Z.Value = b.Z.Value + c.Z.Value;
            return result;
        }
        public static PinCoordinate operator -(PinCoordinate b, PinCoordinate c)
        {
            PinCoordinate result = new PinCoordinate();
            result.X.Value = b.X.Value - c.X.Value;
            result.Y.Value = b.Y.Value - c.Y.Value;
            result.Z.Value = b.Z.Value - c.Z.Value;
            return result;
        }
    }

    [Serializable]
    public class NCCoordinate : CatCoordinates, IParamNode
    {
        public NCCoordinate(double x, double y)
        {
            try
            {
                X = new Element<double>();
                Y = new Element<double>();
                Z = new Element<double>();
                T = new Element<double>();
                this.X.Value = x;
                this.Y.Value = y;
                this.Z.Value = 0;
                this.T.Value = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public NCCoordinate(double x, double y, double z)
        {
            try
            {
                X = new Element<double>();
                Y = new Element<double>();
                Z = new Element<double>();
                T = new Element<double>();
                this.X.Value = x;
                this.Y.Value = y;
                this.Z.Value = z;
                this.T.Value = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public NCCoordinate(double x, double y, double z, double t)
        {
            try
            {
                X = new Element<double>();
                Y = new Element<double>();
                Z = new Element<double>();
                T = new Element<double>();
                this.X.Value = x;
                this.Y.Value = y;
                this.Z.Value = z;
                this.T.Value = t;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public NCCoordinate(MachineCoordinate coordinate)
        {
            try
            {
                X = new Element<double>();
                Y = new Element<double>();
                Z = new Element<double>();
                T = new Element<double>();
                this.X.Value = coordinate.X.Value;
                this.Y.Value = coordinate.Y.Value;
                this.Z.Value = coordinate.Z.Value;
                this.T.Value = coordinate.T.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public NCCoordinate()
        {
            try
            {
                this.X.Value = 0;
                this.Y.Value = 0;
                this.Z.Value = 0;
                this.T.Value = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public static implicit operator NCCoordinate(int v)
        {
            throw new NotImplementedException();
        }
    }

    [Serializable]
    public class TiltCoord : CatCoordinates, IParamNode
    {
        public TiltCoord(double x, double y)
        {
            try
            {
                this.X.Value = x;
                this.Y.Value = y;
                this.Z.Value = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public TiltCoord(double x, double y, double z)
        {
            try
            {
                this.X.Value = x;
                this.Y.Value = y;
                this.Z.Value = z;
                this.T.Value = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public TiltCoord()
        {

        }
    }


    //나중에 옮겨야됨
    //public enum EnumPinLowPatternID
    //{
    //    FIRSTPATTERN = 1,
    //    SECONDPATTERN = 2,
    //    THIRDPATTERN = 3,
    //    FOURTHPATTERN = 4,
    //    UNDEFINED = 0       
    //}

    [Serializable]
    public class PinAlignPatternInfo : CatCoordinates, INotifyPropertyChanged, IParamNode
    {

        private string _PatternPath;
        public string PatternPath
        {
            get { return _PatternPath; }
            set
            {
                if (value != _PatternPath)
                {
                    _PatternPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PMParameter _PMParameter = new PMParameter();
        public PMParameter PMParameter
        {
            get { return _PMParameter; }
            set
            {
                if (value != _PMParameter)
                {
                    _PMParameter = value;
                    RaisePropertyChanged();
                }
            }
        }
        protected PinAlignPatternInfo(SerializationInfo info, StreamingContext context)
        {

        }
        public PinAlignPatternInfo(double x, double y)
        {
            try
            {
                X = new Element<double>();
                Y = new Element<double>();
                Z = new Element<double>();
                T = new Element<double>();
                this.X.Value = x;
                this.Y.Value = y;
                this.Z.Value = 0;
                this.T.Value = 0;
                PMParameter = new PMParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public PinAlignPatternInfo(double x, double y, PMParameter pmparam)
        {
            try
            {
                X = new Element<double>();
                Y = new Element<double>();
                Z = new Element<double>();
                T = new Element<double>();
                this.X.Value = x;
                this.Y.Value = y;
                this.Z.Value = 0;
                this.T.Value = 0;
                PMParameter = pmparam;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public PinAlignPatternInfo(string path, double x, double y)
        {
            try
            {
                X = new Element<double>();
                Y = new Element<double>();
                Z = new Element<double>();
                T = new Element<double>();
                PatternPath = path;
                this.X.Value = x;
                this.Y.Value = y;
                this.Z.Value = 0;
                this.T.Value = 0;
                PMParameter = new PMParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public PinAlignPatternInfo(double x, double y, double z, double t)
        {
            try
            {
                X = new Element<double>();
                Y = new Element<double>();
                Z = new Element<double>();
                T = new Element<double>();
                this.X.Value = x;
                this.Y.Value = y;
                this.Z.Value = z;
                this.T.Value = t;
                PMParameter = new PMParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public PinAlignPatternInfo(PinAlignPatternInfo coordinate)
        {
            try
            {
                X = new Element<double>();
                Y = new Element<double>();
                Z = new Element<double>();
                T = new Element<double>();
                this.X.Value = coordinate.X.Value;
                this.Y.Value = coordinate.Y.Value;
                this.Z.Value = coordinate.Z.Value;
                this.T.Value = coordinate.T.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public PinAlignPatternInfo()
        {
        }
    }
    [Serializable]
    public abstract class PLAPatternInfomation : PatternInfomation, INotifyPropertyChanged, IParamNode
    {

        //private EnumPinLowPatternID _PatternID;
        //public EnumPinLowPatternID PatternID
        //{
        //    get { return _PatternID; }
        //    set
        //    {
        //        if (value != _PatternID)
        //        {
        //            _PatternID = value;
        //            NotifyPropertyChanged("PatternID");
        //        }
        //    }
        //}

        private UserIndex _UserIndex;
        [DataMember]
        public UserIndex UserIndex
        {
            get { return _UserIndex; }
            set
            {
                if (value != _UserIndex)
                {
                    _UserIndex = value;
                    RaisePropertyChanged();
                }
            }
        }


        public PLAPatternInfomation()
        {

        }

    }
    [Serializable]
    public class PinLowAlignPatternInfo : PLAPatternInfomation, INotifyPropertyChanged, IParamNode
    {

        private string _FileName;
        [DataMember]
        public string FileName
        {
            get { return _FileName; }
            set
            {
                if (value != _FileName)
                {
                    _FileName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _GrayLevel;
        [DataMember]
        public new int GrayLevel
        {
            get { return _GrayLevel; }
            set
            {
                if (value != _GrayLevel)
                {
                    _GrayLevel = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<PinLowAlignPatternOrderEnum> _PatternOrder = new Element<PinLowAlignPatternOrderEnum>();
        [DataMember]
        public Element<PinLowAlignPatternOrderEnum> PatternOrder
        {
            get { return _PatternOrder; }
            set
            {
                if (value != _PatternOrder)
                {
                    _PatternOrder = value;
                    RaisePropertyChanged();
                }
            }
        }

        
        public PinLowAlignPatternInfo(double x, double y)
        {
            try
            {
                X = new Element<double>();
                Y = new Element<double>();
                Z = new Element<double>();
                T = new Element<double>();
                this.X.Value = x;
                this.Y.Value = y;
                this.Z.Value = 0;
                this.T.Value = 0;
                this.PatternState.Value = PatternStateEnum.READY;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public PinLowAlignPatternInfo(double x, double y, PMParameter pmparam)
        {
            try
            {
                X = new Element<double>();
                Y = new Element<double>();
                Z = new Element<double>();
                T = new Element<double>();
                this.X.Value = x;
                this.Y.Value = y;
                this.Z.Value = 0;
                this.T.Value = 0;
                this.PatternState.Value = PatternStateEnum.READY;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public PinLowAlignPatternInfo(string path, double x, double y)
        {
            try
            {
                X = new Element<double>();
                Y = new Element<double>();
                Z = new Element<double>();
                T = new Element<double>();
                FileName = path;
                this.X.Value = x;
                this.Y.Value = y;
                this.Z.Value = 0;
                this.T.Value = 0;
                this.PatternState.Value = PatternStateEnum.READY;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public PinLowAlignPatternInfo(double x, double y, double z, double t)
        {
            try
            {
                X = new Element<double>();
                Y = new Element<double>();
                Z = new Element<double>();
                T = new Element<double>();
                this.X.Value = x;
                this.Y.Value = y;
                this.Z.Value = z;
                this.T.Value = t;
                this.PatternState.Value = PatternStateEnum.READY;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public PinLowAlignPatternInfo(PinLowAlignPatternInfo coordinate)
        {
            try
            {
                X = new Element<double>();
                Y = new Element<double>();
                Z = new Element<double>();
                T = new Element<double>();
                this.X.Value = coordinate.X.Value;
                this.Y.Value = coordinate.Y.Value;
                this.Z.Value = coordinate.Z.Value;
                this.T.Value = coordinate.T.Value;
                this.CamType = coordinate.CamType;
                coordinate.PMParameter.CopyTo(this.PMParameter);
                //this.PMParameter.CopyTo(coordinate.PMParameter);
                this.PatternState.Value = PatternStateEnum.READY;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public PinLowAlignPatternInfo()
        {
            try
            {
                X = new Element<double>();
                Y = new Element<double>();
                Z = new Element<double>();
                T = new Element<double>();
                this.PatternState.Value = PatternStateEnum.NOTREG;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
    [Serializable()]
    public class PadCoordinate : CatCoordinates, IParamNode
    {
        public PadCoordinate()
        {

        }
        public PadCoordinate(double x, double y)
        {
            try
            {
                X = new Element<double>();
                Y = new Element<double>();
                Z = new Element<double>();
                T = new Element<double>();
                X.Value = x;
                Y.Value = y;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private bool _IsMatched;

        public bool IsMatched
        {
            get { return _IsMatched; }
            set { _IsMatched = value; }
        }
    }
}
