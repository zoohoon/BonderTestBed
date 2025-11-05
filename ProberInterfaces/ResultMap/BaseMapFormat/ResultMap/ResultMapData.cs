using System;
using System.Collections.Generic;

namespace ProberInterfaces
{
    [Serializable]
    [ProtoBuf.ProtoContract]
    public class ResultMapData
    {
        public ResultMapData() { }
        //public ResultMapData(int SizeX, int SizeY)
        //{
        //    try
        //    {
        //        for (int i = 0; i < SizeX; i++)
        //        {
        //            for (int j = 0; j < SizeY; j++)
        //            {
        //                mStatusMap.Add(0);
        //                mBINMap.Add(0);
        //                mMapCoordX.Add(0);
        //                mMapCoordY.Add(0);
        //                mDUTMap.Add(0);
        //                mTestCntMap.Add(0);
        //                mInked.Add(false);
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}
        [ProtoBuf.ProtoMember(1)]
        public List<byte> mStatusMap = new List<byte>();

        [ProtoBuf.ProtoMember(2)]
        public List<byte> mRePrbMap = new List<byte>();

        [ProtoBuf.ProtoMember(3)]
        public List<long> mBINMap = new List<long>();

        [ProtoBuf.ProtoMember(4)]
        public List<int> mMapCoordX = new List<int>();

        [ProtoBuf.ProtoMember(5)]
        public List<int> mMapCoordY = new List<int>();

        [ProtoBuf.ProtoMember(6)]
        public List<int> mDUTMap = new List<int>();

        [ProtoBuf.ProtoMember(7)]
        public List<byte> mTestCntMap = new List<byte>();

        [ProtoBuf.ProtoMember(8)]
        public List<bool> mInked = new List<bool>();

        [ProtoBuf.ProtoMember(9)]
        public List<double> mXWaferCenterDistance = new List<double>();

        [ProtoBuf.ProtoMember(10)]
        public List<double> mYWaferCenterDistance = new List<double>();

        [ProtoBuf.ProtoMember(11)]
        public List<double> mOverdrive = new List<double>();

        [ProtoBuf.ProtoMember(11)]
        public List<DateTime> mTestStartTime = new List<DateTime>();

        [ProtoBuf.ProtoMember(12)]
        public List<bool> mFailMarkInspection = new List<bool>();

        [ProtoBuf.ProtoMember(13)]
        public List<bool> mNeedleMarkInspection = new List<bool>();

        [ProtoBuf.ProtoMember(14)]
        public List<bool> mNeedleCleaning = new List<bool>();

        [ProtoBuf.ProtoMember(15)]
        public List<bool> mNeedleAlign = new List<bool>();
    }
}
