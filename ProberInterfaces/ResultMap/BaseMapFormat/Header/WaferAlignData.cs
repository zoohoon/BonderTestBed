using ProberInterfaces.ResultMap;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProberInterfaces
{

    [Serializable]
    [ProtoBuf.ProtoContract]
    public class WaferAlignData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]String info = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        //private double mEdgeDiffX;
        //[ProtoBuf.ProtoMember(1)]
        //public double EdgeDiffX
        //{
        //    get { return mEdgeDiffX; }
        //    set
        //    {
        //        if (mEdgeDiffX != value)
        //        {
        //            mEdgeDiffX = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private double mEdgeDiffY;
        //[ProtoBuf.ProtoMember(2)]
        //public double EdgeDiffY
        //{
        //    get { return mEdgeDiffY; }
        //    set
        //    {
        //        if (mEdgeDiffY != value)
        //        {
        //            mEdgeDiffY = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private byte mAlignAxis;
        //[ProtoBuf.ProtoMember(3)]
        //public byte AlignAxis
        //{
        //    get { return mAlignAxis; }
        //    set
        //    {
        //        if (mAlignAxis != value)
        //        {
        //            mAlignAxis = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private double mAlignedIndexSizeX;
        [ProtoBuf.ProtoMember(1)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.ALIGNEDINDEXSIZEX)]
        public double AlignedIndexSizeX
        {
            get { return mAlignedIndexSizeX; }
            set
            {
                if (mAlignedIndexSizeX != value)
                {
                    mAlignedIndexSizeX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double mAlignedIndexSizeY;
        [ProtoBuf.ProtoMember(2)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.ALIGNEDINDEXSIZEY)]
        public double AlignedIndexSizeY
        {
            get { return mAlignedIndexSizeY; }
            set
            {
                if (mAlignedIndexSizeY != value)
                {
                    mAlignedIndexSizeY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double mDieSpaceingX;
        [ProtoBuf.ProtoMember(3)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.STREETSIZEX)]
        public double DieStreetSizeX
        {
            get { return mDieSpaceingX; }
            set
            {
                if (mDieSpaceingX != value)
                {
                    mDieSpaceingX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double mDieSpaceingY;
        [ProtoBuf.ProtoMember(4)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.STREETSIZEY)]
        public double DieStreetSizeY
        {
            get { return mDieSpaceingY; }
            set
            {
                if (mDieSpaceingY != value)
                {
                    mDieSpaceingY = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private byte mIndexAlignType;
        //[ProtoBuf.ProtoMember(8)]
        //public byte IndexAlignType
        //{
        //    get { return mIndexAlignType; }
        //    set
        //    {
        //        if (mIndexAlignType != value)
        //        {
        //            mIndexAlignType = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
    }
}
