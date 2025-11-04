using SerializerUtil;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace ProberInterfaces
{
    public static class SystemManager
    {
        private static string filePath = @"C:\ProberSystem\SystemInfo";
        private static string fileName = "SystemMode.json";
        public static SystemModeEnum SysteMode { get; set; } = SystemModeEnum.Single;
        public static SystemTypeEnum SystemType { get; set; } = SystemTypeEnum.None;
        public static SystemExcuteModeEnum SysExcuteMode { get; set; } = SystemExcuteModeEnum.Prober;
        private static SystemMode SystemModeParam;

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void LoadParam()
        {
            string fullPath = filePath + "\\" + fileName;

            if (Directory.Exists(filePath) == false)
            {
                Directory.CreateDirectory(filePath);
            }

            bool IsSuccess = false;

            if (AppDomain.CurrentDomain.FriendlyName == "ProberSystem.exe")
            {
                SysExcuteMode = SystemExcuteModeEnum.Prober;
            }
            else if (AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")
            {
                SysExcuteMode = SystemExcuteModeEnum.Remote;
            }
            else if(AppDomain.CurrentDomain.FriendlyName == "BonderSystem.exe")
            {
                SysExcuteMode = SystemExcuteModeEnum.Prober;
            }

            if (!File.Exists(fullPath))
            {
                SystemModeParam = new SystemMode();

                IsSuccess = SerializeManager.Serialize(fullPath, SystemModeParam, serializerType: SerializerType.EXTENDEDXML);
            }

            IsSuccess = SerializeManager.Deserialize(fullPath, out var param, deserializerType: SerializerType.EXTENDEDXML);

            SystemModeParam = (SystemMode)param;
            SysteMode = SystemModeParam.SysteMode;
            SystemType = SystemModeParam.SystemType;

            Extensions_IParam.ProberRunMode = SystemModeParam.SystemRunMode;

            SerializeManager.Serialize(fullPath, SystemModeParam, serializerType: SerializerType.EXTENDEDXML);

            

            //if (!File.Exists(fullPath))
            //{
            //    SystemModeParam = new SystemMode();

            //    IsSuccess = SerializeManager.Serialize(fullPath, SystemModeParam, serializerType: SerializerType.EXTENDEDXML);
            //}

            //IsSuccess = SerializeManager.Deserialize(fullPath, out param, deserializerType: SerializerType.EXTENDEDXML);

            //SystemModeParam = (SystemMode)param;
            //SysteMode = SystemModeParam.SysteMode;

            //if (System.AppDomain.CurrentDomain.FriendlyName == "ProberSystem.exe")
            //    SysExcuteMode = SystemExcuteModeEnum.Prober;
            //else if (System.AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")
            //    SysExcuteMode = SystemExcuteModeEnum.Remote;
            //if (System.AppDomain.CurrentDomain.FriendlyName == "ProberSystem.exe")
            //    SysExcuteMode = SystemExcuteModeEnum.Prober;
            //else if (System.AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")
            //    SysExcuteMode = SystemExcuteModeEnum.Remote;
        }
    }

    [Serializable]
    public class SystemMode
    {
        private SystemModeEnum _SysteMode;
        public SystemModeEnum SysteMode
        {
            get { return _SysteMode; }
            set
            {
                if (value != _SysteMode)
                {
                    _SysteMode = value;
                }
            }
        }
        private SystemTypeEnum _SystemType;
        public SystemTypeEnum SystemType
        {
            get { return _SystemType; }
            set
            {
                if (value != _SystemType)
                {
                    _SystemType = value;
                }
            }
        }

        private RunMode _SystemRunMode;
        public RunMode SystemRunMode
        {
            get { return _SystemRunMode; }
            set
            {
                if (value != _SystemRunMode)
                {
                    _SystemRunMode = value;
                }
            }
        }

        public SystemMode()
        {
            SysteMode = SystemModeEnum.Single;
            SystemType = SystemTypeEnum.None;
            SystemRunMode = RunMode.DEFAULT;
        }
    }

    public enum SystemModeEnum
    {
        Single,
        Multiple,
    }

    public enum SystemTypeEnum
    {
        None,
        Opera,
        GOP,
        DRAX
    }

    public enum SystemExcuteModeEnum
    {
        Prober,
        Remote
    }

    public enum CellInitModeEnum
    {
        BeforeInit,
        NormalEnd,
        ErrorEnd
    }
}
