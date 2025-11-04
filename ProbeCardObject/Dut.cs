using LogModule;
using Newtonsoft.Json;
using ProberInterfaces;
using ProberInterfaces.Param;
using ProberInterfaces.PinAlign.ProbeCardData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Windows;
using System.Xml.Serialization;

namespace ProbeCardObject
{
    [Serializable]
    public class Dut : INotifyPropertyChanged, IDut, IParamNode
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        [XmlIgnore, JsonIgnore]
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

        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public List<object> Nodes { get; set; }

        [DataMember]
        public int DutNumber { get; set; }

        [DataMember]
        public bool DutEnable { get; set; }

        [DataMember]
        public MachineIndex MacIndex { get; set; } = new MachineIndex();

        [DataMember]
        public UserIndex UserIndex { get; set; } = new UserIndex();

        [DataMember]
        public PinCoordinate RefCorner { get; set; } = new PinCoordinate();

        [DataMember]
        public List<IPinData> PinList { get; set; } = new List<IPinData>();

        #region //==================> 삭제 예정
        /// <summary>
        /// JsonIgnore 삭제 => Serialize 안되서 WCF 통신시 정보 못넘어감.
        /// </summary>
        private double _DutSizeLeft = new double();
        [DataMember]
        public double DutSizeLeft
        {
            get { return _DutSizeLeft; }
            set
            {
                if (value != _DutSizeLeft)
                {
                    _DutSizeLeft = value;
                    RaisePropertyChanged("");
                }
            }
        }

        private double _DutSizeTop = new double();
        [DataMember]
        public double DutSizeTop
        {
            get { return _DutSizeTop; }
            set
            {
                if (value != _DutSizeTop)
                {
                    _DutSizeTop = value;
                    RaisePropertyChanged("");
                }
            }
        }

        private double _DutSizeWidth = new double();
        [DataMember]
        public double DutSizeWidth
        {
            get { return _DutSizeWidth; }
            set
            {
                if (value != _DutSizeWidth)
                {
                    _DutSizeWidth = value;
                    RaisePropertyChanged("");
                }
            }
        }

        private double _DutSizeHeight = new double();
        [DataMember]
        public double DutSizeHeight
        {
            get { return _DutSizeHeight; }
            set
            {
                if (value != _DutSizeHeight)
                {
                    _DutSizeHeight = value;
                    RaisePropertyChanged("");
                }
            }
        }
        private Visibility _DutVisibility = new Visibility();
        [JsonIgnore]
        public Visibility DutVisibility
        {
            get { return _DutVisibility; }
            set
            {
                if (value != _DutVisibility)
                {
                    _DutVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        public Dut()
        {
            try
            {
                _DutSizeHeight = 100;
                _DutSizeWidth = 100;
                _DutSizeLeft = 30;
                _DutSizeTop = 30;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public Dut(MachineIndex mIndex)
        {
            try
            {
                MacIndex = mIndex;
                UserIndex = new UserIndex();
                //PinList = new List<PinData>();
                PinList = new List<IPinData>();
                RefCorner = new PinCoordinate();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public Dut(Dut dut)
        {
            try
            {
                DutNumber = dut.DutNumber;
                DutEnable = dut.DutEnable;

                UserIndex = new UserIndex(dut.UserIndex.XIndex, dut.UserIndex.YIndex);
                MacIndex = new MachineIndex(dut.MacIndex.XIndex, dut.MacIndex.YIndex);
                RefCorner = new PinCoordinate(dut.RefCorner);

                PinList = new List<IPinData>();

                foreach (var pinData in dut.PinList)
                {
                    PinList.Add(new PinData(pinData));
                }


                //PinList = new List<PinData>();
                //foreach (var pinData in dut.PinList)
                //    PinList.Add(new PinData(pinData));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void Copy(ref Dut target)
        {
            try
            {
                target.DutNumber = this.DutNumber;
                target.DutEnable = this.DutEnable;
                target.MacIndex = this.MacIndex;
                target.UserIndex = this.UserIndex;
                target.RefCorner = this.RefCorner;

                target.PinList = new List<IPinData>();

                foreach (var pin in this.PinList)
                {
                    target.PinList.Add(new PinData(pin));
                }

                target.DutSizeLeft = this.DutSizeLeft;
                target.DutSizeTop = this.DutSizeTop;
                target.DutSizeWidth = this.DutSizeWidth;
                target.DutSizeHeight = this.DutSizeHeight;
                target.DutVisibility = this.DutVisibility;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}

