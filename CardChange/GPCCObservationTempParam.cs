using System;
using System.Collections.Generic;

namespace CardChange
{
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.CardChange;
    using ProberInterfaces.Param;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;

    public class GPCCObservationTempParam : INotifyPropertyChanged, IParamNode, ISystemParameterizable, IGPCCObservationTempParam
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

        public string FilePath { get; } = "CardChange";
        public string FileName { get; } = "GPCCObservationParam_Temp.json";

        #region ==> Inheritance Property
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
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
        public List<object> Nodes { get; set; }
        #endregion

        public List<PinCoordinate> ObservationMarkPosList { get; set; }
        public List<PinCoordinate> RegisteredMarkPosList { get; set; }
        public double ObservationPatternWidth { get; set; }
        public double ObservationPatternHeight { get; set; }
        public ushort ObservationCOAXIAL { get; set; }
        public ushort ObservationOBLIQUE { get; set; }

        public EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }

        public EventCodeEnum SetDefaultParam()
        {
            ObservationMarkPosList = new List<PinCoordinate>();
            ObservationMarkPosList.Add(new PinCoordinate(115000, 115000, -45665));//==> Z 25867
            ObservationMarkPosList.Add(new PinCoordinate(115000, -115000, -45665));
            ObservationMarkPosList.Add(new PinCoordinate(-115000, 115000, -45665));
            ObservationMarkPosList.Add(new PinCoordinate(-115000, -115000, -45665));

            RegisteredMarkPosList = new List<PinCoordinate>();
            foreach (PinCoordinate pinPos in ObservationMarkPosList)
            {
                RegisteredMarkPosList.Add(new PinCoordinate(pinPos.X.Value, pinPos.Y.Value, pinPos.Z.Value));
            }

            ObservationPatternWidth = 230;
            ObservationPatternHeight = 230;
            ObservationCOAXIAL = 100;
            ObservationOBLIQUE = 100;
            return EventCodeEnum.NONE;
        }

        public void SetElementMetaData()
        {
        }

    }
}
