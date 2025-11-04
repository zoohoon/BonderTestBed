using ProberInterfaces.ResultMap;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProberInterfaces
{
    [Serializable]
    [ProtoBuf.ProtoContract]
    public class CassetteData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]String info = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        private byte mCassetteNO;
        [ProtoBuf.ProtoMember(1)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.CASSETTENO)]
        public byte CassetteNO
        {
            get { return mCassetteNO; }
            set
            {
                if (mCassetteNO != value)
                {
                    mCassetteNO = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string mCassetteID = string.Empty;
        [ProtoBuf.ProtoMember(2)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.CASSETTEID)]
        public string CassetteID
        {
            get { return mCassetteID; }
            set
            {
                if (mCassetteID != value)
                {
                    mCassetteID = value;
                    RaisePropertyChanged();
                }
            }
        }

        public CassetteData()
        {
            if(this.CassetteID == null)
            {
                this.CassetteID = string.Empty;
            }
        }
    }
}
