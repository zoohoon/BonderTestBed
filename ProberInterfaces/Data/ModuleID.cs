using LogModule;
using System;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProberInterfaces
{
    /// <summary>
    /// Attached Module의 ID를 정의합니다.
    /// </summary>
    [Serializable]
    [DataContract]
    public struct ModuleID : INotifyPropertyChanged
    {
        /// <summary>
        /// 속성값이 변경되면 발생합니다.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 지정된 속성이 변경되었음을 발생시킵니다.
        /// </summary>
        /// <param name="propertyName">속성 이름</param>
        private void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private ModuleTypeEnum _ModuleType;
        /// <summary>
        /// ModuleType 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]

        public ModuleTypeEnum ModuleType
        {
            get { return _ModuleType; }
            set { _ModuleType = value; RaisePropertyChanged(); }
        }

        private int _Index;
        /// <summary>
        /// Index 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public int Index
        {
            get { return _Index; }
            set { _Index = value; RaisePropertyChanged(); }
        }

        private string _Label;
        /// <summary>
        /// Label 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public string Label { get { return _Label; } set { _Label = value; RaisePropertyChanged(); } }
        
        /// <summary>
        /// 인스턴스를 생성합니다.
        /// </summary>
        /// <param name="type">모듈 타입</param>
        /// <param name="index">인덱스</param>
        /// <param name="label">라벨</param>
        public ModuleID(ModuleTypeEnum type, int index, string label) : this()
        {
            try
            {
            this.ModuleType = type;
            this.Index = index;
            this.Label = label;
            if(string.IsNullOrEmpty(label))
            {
                this.Label = string.Format("{0}{1}", ModuleType, Index);
            }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        /// <summary>
        /// 오브젝트를 나타내는 문자열을 반환합니다.
        /// </summary>
        /// <returns>오브젝트를 나타내는 문자열</returns>
        public override string ToString()
        {
            return Label;
        }

        /// <summary>
        /// Equals를 재정의합니다.
        /// </summary>
        /// <param name="obj">비교대상 오브젝트</param>
        /// <returns>같은지 여부</returns>
        public override bool Equals(object obj)
        {
            if (obj is ModuleID == false)
                return false;

            return this == (ModuleID)obj;
        }

        /// <summary>
        /// GetHashCode를 재정의합니다.
        /// </summary>
        /// <returns>hash code</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// 인스턴스를 생성합니다.
        /// </summary>
        /// <param name="type">모듈 타입</param>
        /// <param name="number">인덱스</param>
        /// <param name="label">라벨</param>
        /// <returns></returns>
        public static ModuleID Create(ModuleTypeEnum type, int number, string label = "")
        {
            return new ModuleID(type, number, label);
        }

        /// <summary>
        /// == 오퍼레이터를 재정의합니다.
        /// </summary>
        /// <param name="my">my</param>
        /// <param name="other">other</param>
        /// <returns>같은지 여부</returns>
        public static bool operator ==(ModuleID my, ModuleID other)
        {
            return my.ModuleType == other.ModuleType && my.Index == other.Index;
        }

        /// <summary>
        /// != 오퍼레이터를 재정의합니다.
        /// </summary>
        /// <param name="my">my</param>
        /// <param name="other">other</param>
        /// <returns>같지 않은 지 여부</returns>
        public static bool operator !=(ModuleID my, ModuleID other)
        {
            return !(my == other);
        }

        /// <summary>
        /// UNDEFINED ID를 가져옵니다.
        /// </summary>
        public static readonly ModuleID UNDEFINED = new ModuleID(ModuleTypeEnum.UNDEFINED, 0, "UNDEFINED");
    }

    /// <summary>
    /// Attached Module의 Type을 정의합니다.
    /// </summary>
    [DataContract]
    [Serializable]
    public enum ModuleTypeEnum
    {
        /// <summary>
        /// INVALID
        /// </summary>
        [EnumMember]
        INVALID = -1,
        /// <summary>
        /// UNDEFINED
        /// </summary>
        [EnumMember]
        UNDEFINED,
        /// <summary>
        /// CST
        /// </summary>
        [EnumMember]
        CST,
        /// <summary>
        /// SCANSENSOR
        /// </summary>
        [EnumMember]
        SCANSENSOR,
        /// <summary>
        /// SCANCAM
        /// </summary>
        [EnumMember]
        SCANCAM,
        //=> substrate ownable : hold & position

        /// <summary>
        /// SLOT
        /// </summary>
        [EnumMember]
        SLOT,
        /// <summary>
        /// ARM
        /// </summary>
        [EnumMember]
        ARM,
        /// <summary>
        /// PA
        /// </summary>
        [EnumMember]
        PA,
        /// <summary>
        /// FIXEDTRAY
        /// </summary>
        [EnumMember]
        FIXEDTRAY,
        /// <summary>
        /// INSPECTIONTRAY
        /// </summary>
        [EnumMember]
        INSPECTIONTRAY,
        /// <summary>
        /// CHUCK
        /// </summary>
        [EnumMember]
        CHUCK,
        /// <summary>
        /// SEMICSOCR
        /// </summary>
        [EnumMember]
        SEMICSOCR,
        /// <summary>
        /// COGNEXOCR
        /// </summary>
        [EnumMember]
        COGNEXOCR,
        //-------------

        //Framed ------
        //[EnumMember]
        //GRIPPER,
        //[EnumMember]
        //FRAMEDPA,

        //-------------
        /// <summary>
        /// Wafer buffer
        /// </summary>
        [EnumMember]
        BUFFER,
        /// <summary>
        /// Card Handling Arm
        /// </summary>
        [EnumMember]
        CARDARM,
        /// <summary>
        /// Card buffer
        /// </summary>
        [EnumMember]
        CARDBUFFER,
        /// <summary>
        /// Card changer on stage
        /// </summary>
        [EnumMember]
        CC,
        [EnumMember]
        CARDTRAY,
    }
}
