using System.Collections.Generic;

namespace ProberInterfaces.Proxies
{
    using ProberErrorCode;
    using System;
    using System.Collections.ObjectModel;

    public interface IParamManagerProxy : IFactoryModule, IProberProxy
    {
        new void InitService();
        void ExportDBtoCSV();

        bool IsAvailable();
        IElement GetAssociateElement(string associateID);
        List<IElement> GetCommonElementList();
        List<IElement> GetDevElementList();
        IElement GetElement(int elementID);
        long GetElementIDFormVID(long vid);
        string GetPropertyPathFromVID(long vid);
        List<IElement> GetSysElementList();
        void LoadComElementInfoFromDB();
        void LoadDevElementInfoFromDB();
        void LoadElementInfoFromDB();
        void LoadElementInfoFromDB(ParamType paramType, IElement elem, string dbPropertyPath);
        void LoadSysElementInfoFromDB();
        void RegistElementToDB();
        void SaveCategory(string categoryID);
        void SaveElement(IElement elem, bool isNeedValidation = false);//, string source_classname = null);
        
        void SetElement(string propPath, Object setval);
        void SyncDBTableByCSV();
        byte[] GetDevParamElementsBulk(string paramName);
        byte[] GetDevElementsBulk();
        byte[] GetSysElementsBulk();
        List<IElement> GetElementList(List<IElement> emlList, ref byte[] bulkElem);
        void SaveElementPack(byte[] bytepack, bool isNeedValidation = false);//, string source_classname = null);

        EventCodeEnum CheckOriginSetValueAvailable(string propertypath, object val);
        EventCodeEnum CheckSetValueAvailable(string propertypath, object val);

        EventCodeEnum SetOriginValue(string propertypath, Object val, bool isNeedValidation = false, bool isEqualsValue = true, object valueChangedParam = null);//, string source_classname = null);
        EventCodeEnum SetValue(string propertypath, Object val, bool isNeedValidation = false);

        void SetChangedDeviceParam(bool flag);
        void SetChangedSystemParam(bool flag);

        bool GetChangedDeviceParam();
        bool GetChangedSystemParam();
        EventCodeEnum VerifyLotVIDsCheckBeforeLot();
        ObservableCollection<int> GetVerifyLotVIDs();
        void SetVerifyLotVIDs(ObservableCollection<int> vids);
        void UpdateVerifyParam();
        void UpdateLowerLimit(string propertyPath, string setValue);
        void UpdateUpperLimit(string propertyPath, string setValue);
        bool GetVerifyParameterBeforeStartLotEnable();
        List<VerifyParamInfo> GetVerifyParamInfo();
        void SetVerifyParameterBeforeStartLotEnable(bool flag);
        void SetVerifyParamInfo(List<VerifyParamInfo> infos);
        EventCodeEnum SaveDevParameter();
    }
}
