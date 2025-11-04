using LogModule;
using System;
using System.Runtime.Serialization;

namespace ProberInterfaces.Enum
{
    //[DataContractAttribute]
    //public enum EnumJogDirection
    //{
    //    [EnumMember]
    //    Center,
    //    [EnumMember]
    //    Up,
    //    [EnumMember]
    //    RightUp,
    //    [EnumMember]
    //    Right,
    //    [EnumMember]
    //    RightDown,
    //    [EnumMember]
    //    Down,
    //    [EnumMember]
    //    LeftDown,
    //    [EnumMember]
    //    Left,
    //    [EnumMember]
    //    LeftUp,
    //    [EnumMember]
    //    ZBigUp,
    //    [EnumMember]
    //    ZBigDown,
    //    [EnumMember]
    //    ZSmallUp,
    //    [EnumMember]
    //    ZSmallDown
    //}

    public enum JogMode
    {
        Normal,
        DiagonalAll,                // 모든 대각선 조그 버튼 활성화
        DiagonalRightUpLeftDown,   // RightUp & LeftDown 조그 버튼 활성화
        DiagonalLeftUpRightDown    // LeftUp & RightDown 조그 버튼 활성화
    }

    public enum EnumJogDirection
    {
        Center,
        Up,
        RightUp,
        Right,
        RightDown,
        Down,
        LeftDown,
        Left,
        LeftUp,
        ZBigUp,
        ZBigDown,
        ZSmallUp,
        ZSmallDown
    }
    [Serializable, DataContract]
    public class JogParam
    {
        [DataMember]
        public EnumProberCam CurCameraType { get; set; }
        [DataMember]
        public EnumJogDirection Direction { get; set; }
        [DataMember]
        public double Distance { get; set; }
        public JogParam(EnumProberCam curCamType, EnumJogDirection direction, double distance)
        {
            try
            {
                CurCameraType = curCamType;
                Direction = direction;
                Distance = distance;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public JogParam(EnumProberCam curCamType, EnumJogDirection direction)
        {
            try
            {
                CurCameraType = curCamType;
                Direction = direction;
                Distance = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public JogParam(JogParam jogParam)
        {
            CurCameraType = jogParam.CurCameraType;
            Direction = jogParam.Direction;
            Distance = jogParam.Distance;
        }
        public bool IsEqual(JogParam obj)
        {
            return
                CurCameraType == obj.CurCameraType &&
                Direction == obj.Direction &&
                Distance == obj.Distance;
        }
    }
}
