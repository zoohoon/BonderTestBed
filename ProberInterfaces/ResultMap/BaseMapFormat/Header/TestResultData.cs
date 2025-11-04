using ProberInterfaces.ResultMap;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProberInterfaces
{
    [Serializable]
    [ProtoBuf.ProtoContract]
    public class TestResultData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]String info = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        //private byte mProbingMode;
        //[ProtoBuf.ProtoMember(1)]
        //public byte ProbingMode
        //{
        //    get { return mProbingMode; }
        //    set
        //    {
        //        if (mProbingMode != value)
        //        {
        //            mProbingMode = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private byte mProbingStartPos;
        //[ProtoBuf.ProtoMember(2)]
        //public byte ProbingStartPos
        //{
        //    get { return mProbingStartPos; }
        //    set
        //    {
        //        if (mProbingStartPos != value)
        //        {
        //            mProbingStartPos = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private byte mProbingDirection;
        //[ProtoBuf.ProtoMember(3)]
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

        //private int mTestDieInformAddress;
        //[ProtoBuf.ProtoMember(4)]
        //public int TestDieInformAddress
        //{
        //    get { return mTestDieInformAddress; }
        //    set
        //    {
        //        if (mTestDieInformAddress != value)
        //        {
        //            mTestDieInformAddress = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private long mTotalProbingSequence;
        [ProtoBuf.ProtoMember(1)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.PROBINGSEQCOUNT)]
        public long TotalProbingSequence
        {
            get { return mTotalProbingSequence; }
            set
            {
                if (mTotalProbingSequence != value)
                {
                    mTotalProbingSequence = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private long mLastProbingSequence;
        //[ProtoBuf.ProtoMember(6)]
        //public long LastProbingSequence
        //{
        //    get { return mLastProbingSequence; }
        //    set
        //    {
        //        if (mLastProbingSequence != value)
        //        {
        //            mLastProbingSequence = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private byte mTestCnt;
        //[ProtoBuf.ProtoMember(7)]
        //public byte TestCnt
        //{
        //    get { return mTestCnt; }
        //    set
        //    {
        //        if (mTestCnt != value)
        //        {
        //            mTestCnt = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private byte mCPCnt;
        [ProtoBuf.ProtoMember(2)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.CPCOUNT)]
        public byte CPCnt
        {
            get { return mCPCnt; }
            set
            {
                if (mCPCnt != value)
                {
                    mCPCnt = value;
                    RaisePropertyChanged();
                }
            }
        }

        private byte mRetestCnt;
        [ProtoBuf.ProtoMember(3)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.RETESTCOUNT)]
        public byte RetestCnt
        {
            get { return mRetestCnt; }
            set
            {
                if (mRetestCnt != value)
                {
                    mRetestCnt = value;
                    RaisePropertyChanged();
                }
            }
        }

        private byte mTestEndInformation;
        [ProtoBuf.ProtoMember(4)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.PROBINGENDREASON)]
        public byte TestEndInformation
        {
            get { return mTestEndInformation; }
            set
            {
                if (mTestEndInformation != value)
                {
                    mTestEndInformation = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long mTotalTestDieCnt;
        /// <summary>
        /// Lot안에 모든 Test Die Count를 뜻합니다.
        /// </summary>
        [ProtoBuf.ProtoMember(5)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.TESTDIECOUNTINLOT)]
        public long TotalTestDieCnt
        {
            get { return mTotalTestDieCnt; }
            set
            {
                if (mTotalTestDieCnt != value)
                {
                    mTotalTestDieCnt = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long mTotalPassDieCnt;
        /// <summary>
        /// Lot안에 모든 Pass Die Count를 뜻합니다.
        /// </summary>
        [ProtoBuf.ProtoMember(6)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.GOODDEVICESINLOT)]
        public long TotalPassDieCnt
        {
            get { return mTotalPassDieCnt; }
            set
            {
                if (mTotalPassDieCnt != value)
                {
                    mTotalPassDieCnt = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long mTotalFailDieCnt;
        /// <summary>
        /// Lot안에 모든 Faile Die Count를 뜻합니다.
        /// </summary>
        [ProtoBuf.ProtoMember(7)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.BADDEVICESINLOT)]
        public long TotalFailDieCnt
        {
            get { return mTotalFailDieCnt; }
            set
            {
                if (mTotalFailDieCnt != value)
                {
                    mTotalFailDieCnt = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long mTestDieCnt;
        /// <summary>
        /// 한 Wafer 안에 모든 Test Die Count를 뜻합니다.
        /// </summary>
        [ProtoBuf.ProtoMember(8)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.TESTDIECOUNTINWAFER)]
        public long TestDieCnt
        {
            get { return mTestDieCnt; }
            set
            {
                if (mTestDieCnt != value)
                {
                    mTestDieCnt = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long mPassDieCnt;
        /// <summary>
        /// 한 Wafer 안에 모든 Pass Die Count를 뜻합니다.
        /// </summary>
        [ProtoBuf.ProtoMember(9)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.GOODDEVICESINWAFER)]
        public long PassDieCnt
        {
            get { return mPassDieCnt; }
            set
            {
                if (mPassDieCnt != value)
                {
                    mPassDieCnt = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long mFailDieCnt;
        /// <summary>
        /// 한 Wafer 안에 모든 Fail Die Count를 뜻합니다.
        /// </summary>
        [ProtoBuf.ProtoMember(10)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.BADDEVICESINWAFER)]
        public long FailDieCnt
        {
            get { return mFailDieCnt; }
            set
            {
                if (mFailDieCnt != value)
                {
                    mFailDieCnt = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double mYield;
        [ProtoBuf.ProtoMember(11)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.YIELD)]
        public double Yield
        {
            get { return mYield; }
            set
            {
                if (mYield != value)
                {
                    mYield = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double mRetestYield;
        [ProtoBuf.ProtoMember(12)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.RETESTYIELD)]
        public double RetestYield
        {
            get { return mRetestYield; }
            set
            {
                if (mRetestYield != value)
                {
                    mRetestYield = value;
                    RaisePropertyChanged();
                }
            }

        }

        private long mTouchDownCnt;
        [ProtoBuf.ProtoMember(13)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.TOUCHDOWNCOUNT)]
        public long TouchDownCnt
        {
            get { return mTouchDownCnt; }
            set
            {
                if (mTouchDownCnt != value)
                {
                    mTouchDownCnt = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private long mProbeCardContactCnt;
        //[ProtoBuf.ProtoMember(20)]
        //public long ProbeCardContactCnt
        //{
        //    get { return mProbeCardContactCnt; }
        //    set
        //    {
        //        if (mProbeCardContactCnt != value)
        //        {
        //            mProbeCardContactCnt = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
    }
}
