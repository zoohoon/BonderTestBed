using ProberErrorCode;
using ProberInterfaces.Device;
using SecsGemServiceInterface;
using System;
using System.Collections.Generic;
using XGEMWrapper;

namespace ProberInterfaces.Loader
{
    public interface ITrasnferObjectSet
    {
        void UnSelectAll();
        //EventCodeEnum AssignWaferType(EnumWaferType type, string sourcename, SubstrateSizeEnum size);
        void RemoveAssignedWaferType(IPolishWaferSourceInformation pwinfo);
        void UpdateAssignedWaferTypeColor(AsyncObservableCollection<IPolishWaferSourceInformation> polishWaferSources);
        EventCodeEnum AssignWaferType(EnumWaferType type, IPolishWaferSourceInformation pwinfo);
        List<IWaferSupplyMappingInfo> GetSelectedModulesList();
        List<IWaferSupplyMappingInfo> GetDefineNameModulesList(string PWDefineName);
        List<IWaferSupplyMappingInfo> GetAssignedMappingInfo(EnumWaferType type, string sourcename);

        EventCodeEnum UpdateInfo(IPolishWaferSourceInformation pwinfo);
    }

    public interface IWaferSupplyMappingInfo
    {

    }


    public interface ITransferObjectDeviceInfo
    {

    }

    public interface IDeviceManagerParameter
    {

    }



    public interface IDeviceManager : IHasSysParameterizable, IModule
    {
        IParam DeviceManagerParamerer_IParam { get; set; }
        IParam PolishWaferInfoParam_IParam { get; set; }
        ITrasnferObjectSet TransferObjectInfos { get; set; }
        //IDeviceManagerParameter GetDeviceManagerParameter();
        string GetLoaderDevicePath();
        (OCRDevParameter param, EventCodeEnum retVal) GetOCRDevParameter(string devicename);
        byte[] Compress(string devicename, int stageindex = -1);
        string GetFullPath(Object obj, int stagenum);
        EventCodeEnum SetRecipeToStage(DownloadStageRecipeActReqData data);
        EventCodeEnum SetDevice(int stageindex, string devicename, string lotid, string lotCstHashCode, bool loaddev = true, int foupnumber = -1, bool showprogress = true, bool waitload = true);
        void SetPMIDevice(int foupnumber, string devicename);
        void SetPMIDeviceUsingCellParam(int foupnumber, int cellnumber);
        EventCodeEnum SetParameterForDevice(NeedChangeParameterInDevice data);
        object GetLockObject();

        #region Detach Device
        string DetachDeviceFolderName { get; }
        bool GetDetachDeviceFlag();
        void SetDetachDeviceFlag(bool flag);

        AsyncObservableCollection<IPolishWaferSourceInformation> GetPolishWaferSources();
        List<ModuleID> GetPolishSourceModules();
        IPolishWaferSourceInformation GetPolishWaferInformation(ModuleID moduleID);
        #endregion

        void UpdateFixedTrayCanUseBuffer();
        EventCodeEnum RecipeValidation(string recipeName, SubstrateSizeEnum value);
    }
}
