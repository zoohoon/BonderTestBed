using ProberInterfaces.ResultMap;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProberInterfaces
{
    [Serializable]
    [ProtoBuf.ProtoContract]
    public class TimeData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]String info = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        private string mLotStartTime = string.Empty;
        [ProtoBuf.ProtoMember(1)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.LOTSTARTTIME)]
        public string LotStartTime
        {
            get { return mLotStartTime; }
            set
            {
                if (mLotStartTime != value)
                {
                    mLotStartTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string mLotEndTime = string.Empty;
        [ProtoBuf.ProtoMember(2)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.LOTENDTIME)]
        public string LotEndTime
        {
            get { return mLotEndTime; }
            set
            {
                if (mLotEndTime != value)
                {
                    mLotEndTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string mWaferLoadingTime = string.Empty;
        [ProtoBuf.ProtoMember(3)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.WAFERLOADINGTIME)]
        public string WaferLoadingTime
        {
            get { return mWaferLoadingTime; }
            set
            {
                if (mWaferLoadingTime != value)
                {
                    mWaferLoadingTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string mWaferUnloadingTime = string.Empty;
        [ProtoBuf.ProtoMember(4)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.WAFERUNLOADINGTIME)]
        public string WaferUnloadingTime
        {
            get { return mWaferUnloadingTime; }
            set
            {
                if (mWaferUnloadingTime != value)
                {
                    mWaferUnloadingTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string mProbingStartTime = string.Empty;
        [ProtoBuf.ProtoMember(5)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.PROBINGSTARTTIME)]
        public string ProbingStartTime
        {
            get { return mProbingStartTime; }
            set
            {
                if (mProbingStartTime != value)
                {
                    mProbingStartTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string mProbingFinishTime = string.Empty;
        [ProtoBuf.ProtoMember(6)]
        [ProberMapPropertyAttribute(EnumProberMapProperty.PROBINGENDTIME)]
        public string ProbingFinishTime
        {
            get { return mProbingFinishTime; }
            set
            {
                if (mProbingFinishTime != value)
                {
                    mProbingFinishTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        public TimeData()
        {
            if(this.LotStartTime == null)
            {
                this.LotStartTime = string.Empty;
            }

            if (this.LotEndTime == null)
            {
                this.LotEndTime = string.Empty;
            }

            if (this.WaferLoadingTime == null)
            {
                this.WaferLoadingTime = string.Empty;
            }

            if (this.WaferUnloadingTime == null)
            {
                this.WaferUnloadingTime = string.Empty;
            }

            if (this.ProbingStartTime == null)
            {
                this.ProbingStartTime = string.Empty;
            }

            if (this.ProbingFinishTime == null)
            {
                this.ProbingFinishTime = string.Empty;
            }
        }
    }
}
