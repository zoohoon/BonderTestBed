using System;
using System.Collections.Generic;

namespace ProberInterfaces
{
    public interface IDeviceUpDownManager : IFactoryModule, IModule, IHasSysParameterizable
    {
        void ChangeUploadDirectory();
        void ChangeDownloadDirectory();
        List<String> GetLocalDeviceNameList();
        EnumDeviceUpDownErr GetServerDeviceNameList(out List<String> fileList);
        bool CheckDeviceExistInLocal(String deviceName);
        EnumDeviceUpDownErr CheckDeviceExistInServer(String deviceName, out bool isExists);
        void DeleteDeviceInLocal(String deviceName);
        EnumDeviceUpDownErr DownloadDevice(String deviceName, Func<long, long, bool> dataTransferEvent);
        EnumDeviceUpDownErr UploadDevice(String deviceName, Func<long, long, bool> dataTransferEvent);
        bool CheckConnection();
    }
    public enum EnumDeviceUpDownErr
    {
        NONE,
        TRANSFER_ERROR,
        ZIP_ERROR,
        DEVICENOTEXIST_ERROR,
        NETWORK_ERROR,
    }
}
