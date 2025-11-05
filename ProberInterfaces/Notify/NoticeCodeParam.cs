using System;

namespace ProberInterfaces
{
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces.Lamp;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    [DataContract]
    public class EventCodeParam : INotifyPropertyChanged
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

        private EventCodeEnum _EventCode;
        [DataMember]
        public EventCodeEnum EventCode
        {
            get { return _EventCode; }
            set
            {
                if (value != _EventCode)
                {
                    _EventCode = value;
                    RaisePropertyChanged();
                }
            }
        }


        private EnumProberErrorKind _ProberErrorKind;
        [DataMember]
        public EnumProberErrorKind ProberErrorKind
        {
            get { return _ProberErrorKind; }
            set
            {
                if (value != _ProberErrorKind)
                {
                    _ProberErrorKind = value;
                    RaisePropertyChanged();
                }
            }
        }


        private string _Title;
        /// <summary>
        /// IsEnableNotifyMessageDialog == true 일때 출력할 Message Dialog Title
        /// </summary>
        [DataMember]
        public string Title
        {
            get { return _Title; }
            set
            {
                if (value != _Title)
                {
                    _Title = value;
                    RaisePropertyChanged();
                }
            }
        }


        private string _Message;
        /// <summary>
        /// IsEnableNotifyMessageDialog == true 일때 출력할 Message Content
        /// </summary>
        [DataMember]
        public string Message
        {
            get { return _Message; }
            set
            {
                if (value != _Message)
                {
                    _Message = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _GemAlaramNumber = -1;
        /// <summary>
        /// Gem Alarm Number
        /// </summary>
        [DataMember]
        public long GemAlaramNumber
        {
            get { return _GemAlaramNumber; }
            set
            {
                if (value != _GemAlaramNumber)
                {
                    _GemAlaramNumber = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _EnableNotifyMessageDialog = false;
        /// <summary>
        /// Message Dialog 를 띄울지 말지 여부 ( true : 띄움, false : 안띄움 )
        /// </summary>
        [DataMember]
        public bool EnableNotifyMessageDialog
        {
            get { return _EnableNotifyMessageDialog; }
            set
            {
                if (value != _EnableNotifyMessageDialog)
                {
                    _EnableNotifyMessageDialog = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _MessageOccurTitle = "";
        /// <summary>
        /// Message 앞에 Index 와 연결할 타이틀 ex ) [LoadPort #1], [Pre Aligner #1]
        /// </summary>
        [DataMember]
        public string MessageOccurTitle
        {
            get { return _MessageOccurTitle; }
            set
            {
                if (value != _MessageOccurTitle)
                {
                    _MessageOccurTitle = value;
                    RaisePropertyChanged();
                }
            }
        }


        #region Eventlog

        private bool _EnableNotifyEventlog = true;
        [DataMember]
        public bool EnableNotifyEventlog
        {
            get { return _EnableNotifyEventlog; }
            set
            {
                if (value != _EnableNotifyEventlog)
                {
                    _EnableNotifyEventlog = value;
                    RaisePropertyChanged();
                }
            }
        }


        #endregion

        #region Prolog

        private bool _EnableNotifyProlog = true;
        [DataMember]
        public bool EnableNotifyProlog
        {
            get { return _EnableNotifyProlog; }
            set
            {
                if (value != _EnableNotifyProlog)
                {
                    _EnableNotifyProlog = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PrologType _ProLogType = PrologType.UNDEFINED;
        [DataMember]
        public PrologType ProLogType
        {
            get { return _ProLogType; }
            set
            {
                if (value != _ProLogType)
                {
                    _ProLogType = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion



        #region Buzzer, Lamp

        private RequestCombination _LampParam;
        public RequestCombination LampParam
        {
            get { return _LampParam; }
            set
            {
                if (value != _LampParam)
                {
                    _LampParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsUseBuzzer = false;
        public bool IsUseBuzzer
        {
            get { return _IsUseBuzzer; }
            set
            {
                if (value != _IsUseBuzzer)
                {
                    _IsUseBuzzer = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _BuzzerTimerSec = 0.0;
        /// <summary>
        /// 0.0 이면 사용자가 끌때까지 울림.
        /// </summary>
        public double BuzzerTimerSec
        {
            get { return _BuzzerTimerSec; }
            set
            {
                if (value != _BuzzerTimerSec)
                {
                    _BuzzerTimerSec = value;
                    RaisePropertyChanged();
                }
            }
        }



        #endregion

        private DateTime _OccurTime;
        [DataMember, JsonIgnore]
        public DateTime OccurTime
        {
            get { return _OccurTime; }
            set
            {
                if (value != _OccurTime)
                {
                    _OccurTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private bool _IsChecked = false;
        [JsonIgnore]
        public bool IsChecked
        {
            get { return _IsChecked; }
            set
            {
                if (value != _IsChecked)
                {
                    _IsChecked = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _Index = 0;
        [DataMember, JsonIgnore]
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

        private EnumProberModule _ModuleType;
        [DataMember, JsonIgnore]
        public EnumProberModule ModuleType
        {
            get { return _ModuleType; }
            set
            {
                if (value != _ModuleType)
                {
                    _ModuleType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _ImageDatasHashCode = string.Empty;
        [DataMember, JsonIgnore]
        public string ImageDatasHashCode
        {
            get { return _ImageDatasHashCode; }
            set
            {
                if (value != _ImageDatasHashCode)
                {
                    _ImageDatasHashCode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _ModuleStartTime = string.Empty;
        [DataMember, JsonIgnore]
        public string ModuleStartTime
        {
            get { return _ModuleStartTime; }
            set
            {
                if (value != _ModuleStartTime)
                {
                    _ModuleStartTime = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
