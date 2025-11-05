using System;
using System.Runtime.Serialization;

namespace ProberInterfaces.PolishWafer
{
    [Serializable, DataContract]
    public class PolishWaferIndexModel
    {
        private int _IntervalIndex;
        [DataMember]
        public int IntervalIndex
        {
            get { return _IntervalIndex; }
            set
            {
                if (value != _IntervalIndex)
                {
                    _IntervalIndex = value;
                }
            }
        }

        private int _CleaningIndex;
        [DataMember]
        public int CleaningIndex
        {
            get { return _CleaningIndex; }
            set
            {
                if (value != _CleaningIndex)
                {
                    _CleaningIndex = value;
                }
            }
        }

    }

    [Serializable, DataContract]
    public class PolishWaferDefineModel
    {
        [DataMember]
        public string DefineName;
        [DataMember]
        public int IntervalIndex;
        [DataMember]
        public int CleaningIndex;
    }

    [Serializable, DataContract]
    public class PolishWaferRecipeSettingModel2
    {
        [DataMember]
        public int IntervalParamIndex;
        [DataMember]
        public object TextBox;
    }
}
