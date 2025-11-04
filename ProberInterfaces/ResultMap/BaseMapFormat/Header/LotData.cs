using ProberInterfaces.ResultMap;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProberInterfaces
{
    [Serializable]
    [ProtoBuf.ProtoContract]
    public class LotData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]String info = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        private string mLotName = string.Empty;
        [ProtoBuf.ProtoMember(1)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.LOTID)]
        public string LotName
        {
            get { return mLotName; }
            set
            {
                if (mLotName != value)
                {
                    mLotName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int mChuckTemp;
        [ProtoBuf.ProtoMember(2)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.CHUCKTEMP)]
        public int ChuckTemp
        {
            get { return mChuckTemp; }
            set
            {
                if (mChuckTemp != value)
                {
                    mChuckTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private byte mOCRtype;
        [ProtoBuf.ProtoMember(3)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.OCRTYPE)]
        public byte OCRtype
        {
            get { return mOCRtype; }
            set
            {
                if (mOCRtype != value)
                {
                    mOCRtype = value;
                    RaisePropertyChanged();
                }
            }
        }

        public LotData()
        {
            if(this.LotName == null)
            {
                LotName = string.Empty;
            }
        }
    }
}
