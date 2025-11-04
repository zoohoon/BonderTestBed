using ProberInterfaces.ResultMap;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProberInterfaces
{
    [System.SerializableAttribute()]
    public enum AxisDirectionEnum
    {

        /// <remarks/>
        UpRight = 1,

        /// <remarks/>
        DownRight = 2,

        /// <remarks/>
        UpLeft = 3,

        /// <remarks/>
        DownLeft = 4,
    }

    [Serializable]
    [ProtoBuf.ProtoContract]
    public class MapConfigData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] String info = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        private int mMapSizeX;
        /// <summary>
        /// Map 열의 갯수를 뜻합니다.
        /// </summary>
        [ProtoBuf.ProtoMember(1)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.MAPSIZEX)]
        public int MapSizeX
        {
            get { return mMapSizeX; }
            set
            {
                if (mMapSizeX != value)
                {
                    mMapSizeX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int mMapSizeY;
        /// <summary>
        /// Map 행의 갯수를 뜻합니다.
        /// </summary>
        [ProtoBuf.ProtoMember(2)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.MAPSIZEY)]
        public int MapSizeY
        {
            get { return mMapSizeY; }
            set
            {
                if (mMapSizeY != value)
                {
                    mMapSizeY = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private byte mMapDir;

        ///// <summary>
        ///// LU : 1, RU : 2, RD : 3, LD : 4
        ///// </summary>
        /////
        /////         /       
        /////     1   /   2   
        /////         /       
        /////  ///////////////
        /////         /
        /////     4   /   3
        /////         /
        /////         
        //[ProtoBuf.ProtoMember(3)]
        //[ProberMapPropertyAttribute(EnumProberMapProperty.MAPDIR)]
        //public byte MapDir
        //{
        //    get { return mMapDir; }
        //    set
        //    {
        //        if (mMapDir != value)
        //        {
        //            mMapDir = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private AxisDirectionEnum mAxisDirection;
        [ProtoBuf.ProtoMember(3)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.AXISDIR)]
        public AxisDirectionEnum AxisDirection
        {
            get { return mAxisDirection; }
            set
            {
                if (mAxisDirection != value)
                {
                    mAxisDirection = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int mFirstDieCoordX;
        [ProtoBuf.ProtoMember(4)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.FIRSTDIEX)]
        public int FirstDieCoordX
        {
            get { return mFirstDieCoordX; }
            set
            {
                if (mFirstDieCoordX != value)
                {
                    mFirstDieCoordX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int mFirstDieCoordY;
        [ProtoBuf.ProtoMember(5)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.FIRSTDIEY)]
        public int FirstDieCoordY
        {
            get { return mFirstDieCoordY; }
            set
            {
                if (mFirstDieCoordY != value)
                {
                    mFirstDieCoordY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int mRefDieCoordX;
        [ProtoBuf.ProtoMember(6)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.REFDIEX)]
        public int RefDieCoordX
        {
            get { return mRefDieCoordX; }
            set
            {
                if (mRefDieCoordX != value)
                {
                    mRefDieCoordX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int mRefDieCoordY;
        [ProtoBuf.ProtoMember(7)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.REFDIEY)]
        public int RefDieCoordY
        {
            get { return mRefDieCoordY; }
            set
            {
                if (mRefDieCoordY != value)
                {
                    mRefDieCoordY = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private byte mRefDieSet;
        //[ProtoBuf.ProtoMember(8)]
        //public byte RefDieSet
        //{
        //    get { return mRefDieSet; }
        //    set
        //    {
        //        if (mRefDieSet != value)
        //        {
        //            mRefDieSet = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private byte mProbingDirection;
        //[ProtoBuf.ProtoMember(9)]
        //public byte ProbingDirection
        //{
        //    get { return mProbingDirection; }
        //    set
        //    {
        //        if (mProbingDirection != value)
        //        {
        //            mProbingDirection = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private int mCenterDieCoordX;
        [ProtoBuf.ProtoMember(8)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.CENTERDIEX)]
        public int CenterDieCoordX
        {
            get { return mCenterDieCoordX; }
            set
            {
                if (mCenterDieCoordX != value)
                {
                    mCenterDieCoordX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int mCenterDieCoordY;
        [ProtoBuf.ProtoMember(9)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.CENTERDIEY)]
        public int CenterDieCoordY
        {
            get { return mCenterDieCoordY; }
            set
            {
                if (mCenterDieCoordY != value)
                {
                    mCenterDieCoordY = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private double mCenterOffsetX;
        //[ProtoBuf.ProtoMember(12)]
        //public double CenterOffsetX
        //{
        //    get { return mCenterOffsetX; }
        //    set
        //    {
        //        if (mCenterOffsetX != value)
        //        {
        //            mCenterOffsetX = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private double mCenterOffsetY;
        //[ProtoBuf.ProtoMember(13)]
        //public double CenterOffsetY
        //{
        //    get { return mCenterOffsetY; }
        //    set
        //    {
        //        if (mCenterOffsetY != value)
        //        {
        //            mCenterOffsetX = value;
        //            RaisePropertyChanged();
        //        }

        //    }
        //}

        private double _WaferCenterToRefDieCenterDistanceX;
        [ProtoBuf.ProtoMember(10)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.DISTANCEWAFERCENTERTOREFDIECENTERX)]
        public double WaferCenterToRefDieCenterDistanceX
        {
            get { return _WaferCenterToRefDieCenterDistanceX; }
            set
            {
                if (_WaferCenterToRefDieCenterDistanceX != value)
                {
                    _WaferCenterToRefDieCenterDistanceX = value;
                    RaisePropertyChanged();
                }

            }
        }

        private double _WaferCenterToRefDieCenterDistanceY;
        [ProtoBuf.ProtoMember(11)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.DISTANCEWAFERCENTERTOREFDIECENTERY)]
        public double WaferCenterToRefDieCenterDistanceY
        {
            get { return _WaferCenterToRefDieCenterDistanceY; }
            set
            {
                if (_WaferCenterToRefDieCenterDistanceY != value)
                {
                    _WaferCenterToRefDieCenterDistanceY = value;
                    RaisePropertyChanged();
                }

            }
        }

        public MapConfigData()
        {

        }
    }
}
