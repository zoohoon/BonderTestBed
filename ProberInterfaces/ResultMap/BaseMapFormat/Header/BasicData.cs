using ProberInterfaces.ResultMap;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProberInterfaces
{
    [Serializable]
    [ProtoBuf.ProtoContract]
    public class BasicData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]String info = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        private byte mMapfileVersion;
        [ProtoBuf.ProtoMember(1)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.MAPFILEVERSION)]
        public byte MapfileVersion
        {
            get { return mMapfileVersion; }
            set
            {
                if (mMapfileVersion != value)
                {
                    mMapfileVersion = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string mDeviceName = string.Empty;
        [ProtoBuf.ProtoMember(2)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.DEVICENAME)]
        public string DeviceName
        {
            get { return mDeviceName; }
            set
            {
                if (mDeviceName != value)
                {
                    mDeviceName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string mProberID = string.Empty;
        [ProtoBuf.ProtoMember(3)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.PROBERID)]
        public string ProberID
        {
            get { return mProberID; }
            set
            {
                if (mProberID != value)
                {
                    mProberID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string mOPName = string.Empty;
        [ProtoBuf.ProtoMember(4)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.OPERATORNAME)]
        public string OPName
        {
            get { return mOPName; }
            set
            {
                if (mOPName != value)
                {
                    mOPName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string mCardName = string.Empty;
        [ProtoBuf.ProtoMember(5)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.CARDNAME)]
        public string CardName
        {
            get { return mCardName; }
            set
            {
                if (mCardName != value)
                {
                    mCardName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int mCardSite;
        [ProtoBuf.ProtoMember(6)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.CARDSITE)]
        public int CardSite
        {
            get { return mCardSite; }
            set
            {
                if (mCardSite != value)
                {
                    mCardSite = value;
                    RaisePropertyChanged();
                }
            }
        }

        public BasicData()
        {
            if(this.DeviceName == null)
            {
                this.DeviceName = string.Empty;
            }

            if (this.ProberID == null)
            {
                this.ProberID = string.Empty;
            }

            if (this.OPName == null)
            {
                this.OPName = string.Empty;
            }

            if (this.CardName == null)
            {
                this.CardName = string.Empty;
            }
        }
    }
}
