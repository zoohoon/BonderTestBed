using Newtonsoft.Json;
using ProberErrorCode;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace LogModule
{
    public class AlarmLogData : INotifyPropertyChanged    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private int _OccurEquipment;
        public int OccurEquipment
        {
            get { return _OccurEquipment; }
            set
            {
                if (value != _OccurEquipment)
                {
                    _OccurEquipment = value;
                    RaisePropertyChanged();
                }
            }
        }


        private EventCodeEnum _ErrorCode;
        [DataMember]
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

        private string _ErrorMessage;
        [DataMember]
        public string ErrorMessage
        {
            get { return _ErrorMessage; }
            set
            {
                if (value != _ErrorMessage)
                {
                    _ErrorMessage = value;
                    RaisePropertyChanged();
                }
            }
        }

        private DateTime _ErrorOccurTime;
        [DataMember]
        public DateTime ErrorOccurTime
        {
            get { return _ErrorOccurTime; }
            set
            {
                if (value != _ErrorOccurTime)
                {
                    _ErrorOccurTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsChecked;
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

        private EnumProberModule _ModuleType;
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
