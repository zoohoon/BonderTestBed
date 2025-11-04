using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SignalTowerModule
{
    public interface ISignalTowerEventParam
    {
        string Guid { get; set; }
        List<SignalDefineInformation> SignalDefineInformations { get; set; }
        string SignalDescription { get; set; }
        int CellIdx { get; set; }
        int FoupIdx { get; set; }
        int Priority { get; set; }
        string EventName { get; set; }
    }

    [Serializable]
    public class SignalTowerEventParam : ISignalTowerEventParam
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion 
        public SignalTowerEventParam()
        {
            CellIdx = 0;
            FoupIdx = 0;
        }
        public SignalTowerEventParam(string guid, 
                                     int cellidx, 
                                     int foupidx, 
                                     List<SignalDefineInformation> signalDefineInformation, 
                                     string signalDescription,
                                     int priority,
                                     bool needRemoveevent,
                                     string eventName
                                     )
        {
            Guid = guid;
            CellIdx = cellidx;
            FoupIdx = foupidx;
            SignalDefineInformations = signalDefineInformation;
            SignalDescription = signalDescription;
            Priority = priority;
            NeedRemoveEvent = needRemoveevent;
            EventName = eventName;
        }
        private string _Guid;
        [XmlElement]
        public string Guid
        {
            get { return _Guid; }
            set { _Guid = value; }            
        }

        private List<SignalDefineInformation> _SignalDefineInformations = new List<SignalDefineInformation>();
        [XmlElement]
        public List<SignalDefineInformation> SignalDefineInformations
        {
            get { return _SignalDefineInformations; }
            set
            {
                if (_SignalDefineInformations != value)
                {
                    _SignalDefineInformations = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _SignalDescription;
        [XmlElement]
        public string SignalDescription
        {
            get { return _SignalDescription; }
            set { _SignalDescription = value; }
        }

        private int _CellIdx;
        [XmlIgnore, JsonIgnore]
        public int CellIdx
        {
            get { return _CellIdx; }
            set { _CellIdx = value; }
        }

        private int _FoupIdx;
        [XmlIgnore, JsonIgnore]
        public int FoupIdx
        {
            get { return _FoupIdx; }
            set { _FoupIdx = value; }
        }
        private int _Priority = 0;
        [XmlElement]
        public int Priority
        {
            get { return _Priority; }
            set { _Priority = value; }
        }

        private bool _NeedRemoveEvent = false;
        [XmlIgnore, JsonIgnore]
        public bool NeedRemoveEvent
        {
            get { return _NeedRemoveEvent; }
            set { _NeedRemoveEvent = value; }
        }

        private string _EventName;
        [XmlIgnore, JsonIgnore]
        public string EventName
        {
            get { return _EventName; }
            set { _EventName = value; }
        }

        private bool _ProcessedEvent = false;
        [XmlIgnore, JsonIgnore]
        public bool ProcessedEvent
        {
            get { return _ProcessedEvent; }
            set { _ProcessedEvent = value; }
        }
    }
}
