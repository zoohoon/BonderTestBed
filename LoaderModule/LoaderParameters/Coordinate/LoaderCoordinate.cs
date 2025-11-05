using LogModule;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Runtime.Serialization;

using System.Runtime.CompilerServices;
using ProberInterfaces;
using Newtonsoft.Json;

namespace LoaderParameters
{
    /// <summary>
    /// LoaderCoordinate 를 정의합니다.
    /// </summary>
    [Serializable]
    [DataContract]
    public class LoaderCoordinate : INotifyPropertyChanged, ICloneable, IParamNode
    {
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

        /// <summary>
        /// 속성값이 변경되면 발생합니다.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 지정된 속성이 변경되었음을 발생시킵니다.
        /// </summary>
        /// <param name="propertyName">속성 이름</param>
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private Element<double> _A = new Element<double>(); //LZ
        /// <summary>
        /// A 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> A { get { return _A; } set { _A = value; RaisePropertyChanged(); } }

        private Element<double> _U = new Element<double>(); //LU
        /// <summary>
        /// U 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> U { get { return _U; } set { _U = value; RaisePropertyChanged(); } }

        private Element<double> _W = new Element<double>(); //LW
        /// <summary>
        /// W 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> W { get { return _W; } set { _W = value; RaisePropertyChanged(); } }

        private Element<UExtensionMoveParam> _E = new Element<UExtensionMoveParam>();
        /// <summary>
        /// E 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<UExtensionMoveParam> E { get { return _E; } set { _E = value; RaisePropertyChanged(); } }

        private Element<double> _SC = new Element<double>();

        [DataMember]
        public Element<double> SC { get { return _SC; } set { _SC = value; RaisePropertyChanged(); } }

        private Element<double> _BT = new Element<double>();
        /// <summary>
        /// BT 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> BT { get { return _BT; } set { _BT = value; RaisePropertyChanged(); } }


        private Element<double> _LX = new Element<double>();
        /// <summary>
        /// LX 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> LX { get { return _LX; } set { _LX = value; RaisePropertyChanged(); } }
        /*  Loader Ovrd 체크
        private Element<double> _Ovrd_A = new Element<double>();
        /// <summary>
        /// A Ovrd값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> Ovrd_A { get { return _Ovrd_A; } set { _Ovrd_A = value; RaisePropertyChanged(); } }

        private Element<double> _Ovrd_U = new Element<double>();
        /// <summary>
        /// U Ovrd값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> Ovrd_U { get { return _Ovrd_U; } set { _Ovrd_U = value; RaisePropertyChanged(); } }

        private Element<double> _Ovrd_W = new Element<double>();
        /// <summary>
        /// W Ovrd값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> Ovrd_W { get { return _Ovrd_W; } set { _Ovrd_W = value; RaisePropertyChanged(); } }

        private Element<double> _Ovrd_E = new Element<double>();
        /// <summary>
        /// E Ovrd값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> Ovrd_E { get { return _Ovrd_E; } set { _Ovrd_E = value; RaisePropertyChanged(); } }

        private Element<double> _Ovrd_SC = new Element<double>();
        / <summary>
        / SC Ovrd값을 가져오거나 설정합니다.
        / </summary>
        [DataMember]
        public Element<double> Ovrd_SC { get { return _Ovrd_SC; } set { _Ovrd_SC = value; RaisePropertyChanged(); } }*/

        public LoaderCoordinate()
        {
            try
            {
            _E.Value = new UExtensionNoneMoveParam();
            //_Ovrd_A.Value = 1;
            //_Ovrd_U.Value = 1;
            //_Ovrd_W.Value = 1;
            //_Ovrd_E.Value = 1;
            //_Ovrd_SC.Value = 1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public virtual object Clone()
        {
            try
            {
            var cloneObj = MemberwiseClone() as LoaderCoordinate;
            cloneObj.E.Value = E.Value.Clone<UExtensionMoveParam>();
            return cloneObj;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        public virtual object DeepClone()
        {
            try
            {
                var cloneObj = (LoaderCoordinate)this.MemberwiseClone();
                
                var obj = new Element<double>();
                this.A.CopyTo(obj);
                cloneObj.A = obj;

                obj = new Element<double>();
                this.U.CopyTo(obj);
                cloneObj.U = obj;

                obj = new Element<double>();
                this.W.CopyTo(obj);
                cloneObj.W = obj;

                cloneObj.E = new Element<UExtensionMoveParam> { Value = this.E.Value.Clone<UExtensionMoveParam>() };

                obj = new Element<double>();
                this.SC.CopyTo(obj);
                cloneObj.SC = obj;

                obj = new Element<double>();
                this.BT.CopyTo(obj);
                cloneObj.BT = obj;

                obj = new Element<double>();
                this.LX.CopyTo(obj);
                cloneObj.LX = obj;

                return cloneObj;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

    }

    /// <summary>
    /// UExtensionMoveParam 를 정의합니다.
    /// </summary>
    [Serializable]
    [DataContract]
    public abstract class UExtensionMoveParam : INotifyPropertyChanged, ICloneable
    {
        /// <summary>
        /// 속성값이 변경되면 발생합니다.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 지정된 속성이 변경되었음을 발생시킵니다.
        /// </summary>
        /// <param name="propertyName">속성 이름</param>
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public abstract object Clone();
    }

    /// <summary>
    /// UExtensionNoneMoveParam 를 정의합니다.
    /// </summary>
    [Serializable]
    [DataContract]
    public class UExtensionNoneMoveParam : UExtensionMoveParam
    {
        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public override object Clone()
        {
            return MemberwiseClone();
        }
    }

    /// <summary>
    /// UExtensionCylinderMoveParam 를 정의합니다.
    /// </summary>
    [Serializable]
    [DataContract]
    public class UExtensionCylinderMoveParam : UExtensionMoveParam, IParamNode
    {
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


        private Element<bool> _Port = new Element<bool>();
        /// <summary>
        /// Value 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<bool> Port { get { return _Port; } set { _Port = value; RaisePropertyChanged(); } }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public override object Clone()
        {
            return MemberwiseClone();
        }
    }

    /// <summary>
    /// UExtensionMotorMoveParam 를 정의합니다.
    /// </summary>
    [Serializable]
    [DataContract]
    public class UExtensionMotorMoveParam : UExtensionMoveParam, IParamNode
    {
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

        private Element<double> _Value = new Element<double>();
        /// <summary>
        /// Value 값을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<double> Value { get { return _Value; } set { _Value = value; RaisePropertyChanged(); } }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public override object Clone()
        {
            return MemberwiseClone();
        }
    }
}
