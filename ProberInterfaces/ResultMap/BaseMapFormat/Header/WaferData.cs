using ProberInterfaces.ResultMap;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProberInterfaces
{
    [Serializable]
    [ProtoBuf.ProtoContract]
    public class WaferData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]String info = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        private string mWaferID;
        [ProtoBuf.ProtoMember(1)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.WAFERID)]
        public string WaferID
        {
            get { return mWaferID; }
            set
            {
                if (mWaferID != value)
                {
                    mWaferID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private byte mWaferSlotNum;
        [ProberMapPropertyAttribute(EnumProberMapProperty.SLOTNO)]
        [ProtoBuf.ProtoMember(2)]
        public byte WaferSlotNum
        {
            get { return mWaferSlotNum; }
            set
            {
                if (mWaferSlotNum != value)
                {
                    mWaferSlotNum = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private byte mWaferInsertedSlotNum;
        //[ProtoBuf.ProtoMember(3)]
        //public byte WaferInsertedSlotNum
        //{
        //    get { return mWaferInsertedSlotNum; }
        //    set
        //    {
        //        if (mWaferInsertedSlotNum != value)
        //        {
        //            mWaferInsertedSlotNum = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private double mWaferSize;
        [ProtoBuf.ProtoMember(3)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.WAFERSIZE)]
        public double WaferSize
        {
            get { return mWaferSize; }
            set
            {
                if (mWaferSize != value)
                {
                    mWaferSize = value;
                    RaisePropertyChanged();
                }
            }
        }

        private byte mFlatNotchType;
        [ProtoBuf.ProtoMember(4)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.NOTCHTYPE)]
        public byte FlatNotchType
        {
            get { return mFlatNotchType; }
            set
            {
                if (mFlatNotchType != value)
                {
                    mFlatNotchType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int mFlatNotchDir;
        [ProtoBuf.ProtoMember(5)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.NOTCHDIR)]
        public int FlatNotchDir
        {
            get { return mFlatNotchDir; }
            set
            {
                if (mFlatNotchDir != value)
                {
                    mFlatNotchDir = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int mFrmdFlatNotchDir;
        [ProtoBuf.ProtoMember(6)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.FRAMEDNOTCHDIR)]
        public int FrmdFlatNotchDir
        {
            get { return mFrmdFlatNotchDir; }
            set
            {
                if (mFrmdFlatNotchDir != value)
                {
                    mFrmdFlatNotchDir = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
