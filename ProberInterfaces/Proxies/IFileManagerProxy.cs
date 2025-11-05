using LogModule;
using ProberErrorCode;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ProberInterfaces.Proxies
{
    public interface IFileManagerProxy : IProberProxy
    {
        new void InitService();

        byte[] GetFileManagerParam();

        ObservableCollection<DeviceInfo> GetDevicelist();
        ObservableCollection<SimpleDeviceInfo> GetDevicelistTest();

        ObservableCollection<string> GetDeviceNamelist();

        byte[] GetDeviceByFileName(string filename);

        bool SetDeviceByFileName(byte[] device, string devicename);

        byte[] GetDevicebytelist();

        string GetSystemParamFullPath(string parameterPath, string parameterName);
        string GetDeviceParamFullPath(string parameterPath = "", string parameterName = "", bool isContainDeviceName = true);

        string GetDeviceName();

        EventCodeEnum ChangeDevice(string DevName);

        EventCodeEnum DeleteDevice(string DevName);

        byte[] GetCompressedFile(DateTime startdate, DateTime enddate, List<EnumLoggerType> logtypes, List<EnumProberModule> imagetypes, bool includeGEM = false, bool includeClip = false, bool includeLoadedDevice = false, bool inlcudeSystemparam = false, bool inlcudebackupinfo = false, bool inlcudeSysteminfo = false);
    }
}
