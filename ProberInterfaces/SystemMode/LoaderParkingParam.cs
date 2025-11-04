using SerializerUtil;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace ProberInterfaces
{
    public static class LoaderParkingParam
    {
        private static string filePath = @"C:\ProberSystem\SystemInfo";
        private static string fileName = "LoaderParking.json";
        public static ParkingParam parkingParam;

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void LoadParam()
        {
            string fullPath = filePath + "\\" + fileName;
            bool IsSuccess = false;

            if (Directory.Exists(filePath) == false)
            {
                Directory.CreateDirectory(filePath);
            }

            if (!File.Exists(fullPath))
            {
                parkingParam = new ParkingParam();

                IsSuccess = SerializeManager.Serialize(fullPath, parkingParam, serializerType: SerializerType.EXTENDEDXML);
            }

            object param = null;

            IsSuccess = SerializeManager.Deserialize(fullPath, out param, deserializerType: SerializerType.EXTENDEDXML);

            parkingParam = (ParkingParam)param;
        }
    }

    [Serializable]
    public class ParkingParam
    {
        private int _LeftDownStageIndex;
        public int LeftDownStageIndex
        {
            get { return _LeftDownStageIndex; }
            set
            {
                if (value != _LeftDownStageIndex)
                {
                    _LeftDownStageIndex = value;
                }
            }
        }

        private int _RightDownStageIndex;
        public int RightDownStageIndex
        {
            get { return _RightDownStageIndex; }
            set
            {
                if (value != _RightDownStageIndex)
                {
                    _RightDownStageIndex = value;
                }
            }
        }

        public ParkingParam()
        {
           if(SystemManager.SystemType==SystemTypeEnum.Opera)
            {
                LeftDownStageIndex = 9;
                RightDownStageIndex = 12;
            }
            else if(SystemManager.SystemType == SystemTypeEnum.DRAX)
            {
                LeftDownStageIndex = 7;
                RightDownStageIndex = 12;
            }
            else if(SystemManager.SystemType == SystemTypeEnum.GOP)
            {
                LeftDownStageIndex = 1;
                RightDownStageIndex = SystemModuleCount.ModuleCnt.StageCount;
            }
            else
            {

            }
        }
    }
}
