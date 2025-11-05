using SerializerUtil;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace ProberInterfaces
{
    public static class SystemModuleCount
    {
        private static string filePath = @"C:\ProberSystem\SystemInfo";
        private static string fileName = "ModuleCount.json";
        public static ModuleCount ModuleCnt;

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
                ModuleCnt = new ModuleCount();

                IsSuccess = SerializeManager.Serialize(fullPath, ModuleCnt, serializerType: SerializerType.EXTENDEDXML);
            }

            object param = null;

            IsSuccess = SerializeManager.Deserialize(fullPath, out param, deserializerType: SerializerType.EXTENDEDXML);

            ModuleCnt = (ModuleCount)param;
        }
    }

    [Serializable]
    public class ModuleCount
    {
        private int _StageCount;
        public int StageCount
        {
            get { return _StageCount; }
            set
            {
                if (value != _StageCount)
                {
                    _StageCount = value;
                }
            }
        }
       
        private int _FoupCount;
        public int FoupCount
        {
            get { return _FoupCount; }
            set
            {
                if (value != _FoupCount)
                {
                    _FoupCount = value;
                }
            }
        }
        private int _SlotCount;
        public int SlotCount
        {
            get { return _SlotCount; }
            set
            {
                if (value != _SlotCount)
                {
                    _SlotCount = value;
                }
            }
        }
        private int _BufferCount;
        public int BufferCount
        {
            get { return _BufferCount; }
            set
            {
                if (value != _BufferCount)
                {
                    _BufferCount = value;
                }
            }
        }
        private int _PACount;
        public int PACount
        {
            get { return _PACount; }
            set
            {
                if (value != _PACount)
                {
                    _PACount = value;
                }
            }
        }
        private int _ArmCount;
        public int ArmCount
        {
            get { return _ArmCount; }
            set
            {
                if (value != _ArmCount)
                {
                    _ArmCount = value;
                }
            }
        }
        private int _CardArmCount;
        public int CardArmCount
        {
            get { return _CardArmCount; }
            set
            {
                if (value != _CardArmCount)
                {
                    _CardArmCount = value;
                }
            }
        }
        private int _INSPCount;
        public int INSPCount
        {
            get { return _INSPCount; }
            set
            {
                if (value != _INSPCount)
                {
                    _INSPCount = value;
                }
            }
        }
        private int _FixedTrayCount;
        public int FixedTrayCount
        {
            get { return _FixedTrayCount; }
            set
            {
                if (value != _FixedTrayCount)
                {
                    _FixedTrayCount = value;
                }
            }
        }
        private int _CardTrayCount;
        public int CardTrayCount
        {
            get { return _CardTrayCount; }
            set
            {
                if (value != _CardTrayCount)
                {
                    _CardTrayCount = value;
                }
            }
        }
        private int _CardBufferCount;
        public int CardBufferCount
        {
            get { return _CardBufferCount; }
            set
            {
                if (value != _CardBufferCount)
                {
                    _CardBufferCount = value;
                }
            }
        }
        public ModuleCount()
        {
            StageCount = 12;
            BufferCount = 5;
            INSPCount = 3;
            FoupCount = 3;
            PACount = 3;
            ArmCount = 2;
            CardArmCount = 1;
            CardBufferCount = 4;
            CardTrayCount = 9;
            FixedTrayCount = 9;
            INSPCount = 3;
            SlotCount = 25;
        }
    }
}
