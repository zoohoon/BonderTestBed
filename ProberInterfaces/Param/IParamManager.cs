using System;
using System.Collections.Generic;

namespace ProberInterfaces
{
    using ProberErrorCode;
    using System.Collections.Concurrent;
    using System.Collections.ObjectModel;
    using System.ServiceModel;

    [ServiceContract]
    public interface IParamManager : IFactoryModule, IModule, IHasDevParameterizable, IHasSysParameterizable
    {
        ConcurrentDictionary<String, ConcurrentDictionary<String, IElement>> DevDBElementDictionary { get; set; }
        ConcurrentDictionary<String, ConcurrentDictionary<String, IElement>> SysDBElementDictionary { get; set; }
        ConcurrentDictionary<String, ConcurrentDictionary<String, IElement>> CommonDBElementDictionary { get; set; }
        [OperationContract]
        IElement GetElement(int elementID);
        IElement GetElement(string propertyPath);
        IElement GetAssociateElement(string associateID);
        [OperationContract]
        long GetElementIDFormVID(long vid);
        [OperationContract]
        string GetPropertyPathFromVID(long vid);
        //[OperationContract]
        /// <summary>
        /// ParamManager의 DevDBElementDictionary로 부터 모든 Element를 List형태로 제공해줍니다.
        /// </summary>
        /// <returns> ParamManager가 가지고 있는 모든 DevElement </returns>
        List<IElement> GetDevElementList();
        [OperationContract]
        byte[] GetDevElementsBulk();
        [OperationContract]
        byte[] GetDevParamElementsBulk(string paramName);
        //[OperationContract]
        /// <summary>
        /// ParamManager의 SysDBElementDictionary로 부터 모든 Element를 List형태로 제공해줍니다.
        /// </summary>
        /// <returns> ParamManager가 가지고 있는 모든 SysElement </returns>
        List<IElement> GetSysElementList();
        [OperationContract]
        byte[] GetSysElementsBulk();
        [OperationContract]
        /// <summary>
        /// ParamManager의 CommonDBElementDictionary로 부터 모든 Element를 List형태로 제공해줍니다.
        /// </summary>
        /// <returns> ParamManager가 가지고 있는 모든 CommonElement </returns>
        List<IElement> GetCommonElementList();

        event EventHandler OnLoadElementInfoFromDB;
        [OperationContract]
        bool IsServiceAvailable();

        [OperationContract]
        void InitService();
        [OperationContract(IsOneWay = true)]
        void SaveElement(IElement elem, bool isNeedValidation = false);//, string source_classname = null);
        [OperationContract(IsOneWay = true)]
        void SetElement(string propPath, Object setval);
        [OperationContract(IsOneWay = true)]
        void SaveElementPack(byte[] pack, bool isNeedValidation = false);//, string source_classname = null);
        void SaveElement(string categoryID);
        [OperationContract]
        void SaveCategory(string categoryid);
        [OperationContract]
        void SyncDBTableByCSV();
        void LoadElementInfoFromDB();
        [OperationContract]
        void LoadElementInfosFromDB();
        [OperationContract]
        void LoadElementInfoFromDB(ParamType paramType, IElement elem, String dbPropertyPath);
        [OperationContract]
        void LoadSysElementInfoFromDB();
        [OperationContract]
        void LoadDevElementInfoFromDB();
        [OperationContract]
        void LoadComElementInfoFromDB(bool isOccureLoadeEvent = false);
        [OperationContract]
        void RegistElementToDB();
        [OperationContract]
        void ExportDBtoCSV();
        [OperationContract]
        bool IsAvailable();

        [OperationContract]
        void SetChangedDeviceParam(bool flag);
        [OperationContract]
        void SetChangedSystemParam(bool flag);
        [OperationContract]

        bool GetChangedDeviceParam();
        [OperationContract]

        bool GetChangedSystemParam();
        [OperationContract]
        EventCodeEnum VerifyLotVIDsCheckBeforeLot();
        [OperationContract]
        ObservableCollection<int> GetVerifyLotVIDs();
        [OperationContract]
        void SetVerifyLotVIDs(ObservableCollection<int> vids);
        [OperationContract]
        void UpdateVerifyParam();
        [OperationContract]
        void UpdateUpperLimit(string propertyPath, string setValue);
        [OperationContract]
        void UpdateLowerLimit(string propertyPath, string setValue);
        [OperationContract]
        bool GetVerifyParameterBeforeStartLotEnable();
        [OperationContract]
        List<VerifyParamInfo> GetVerifyParamInfo();

        [OperationContract]
        void SetVerifyParameterBeforeStartLotEnable(bool flag);
        [OperationContract]
        void SetVerifyParamInfo(List<VerifyParamInfo> infos);
        string GetElementPath(int elementID);
        [OperationContract]
        List<IElement> GetElementList(List<IElement> emlList, ref byte[] bulkElem);

        (bool needValidation, IModule source) ClassNameConverter(string source_classname);
        [OperationContract]
        EventCodeEnum CheckOriginSetValueAvailable(string propertypath, object val);
        [OperationContract]
        EventCodeEnum CheckSetValueAvailable(string propertypath, object val);
        [OperationContract]
        EventCodeEnum SetOriginValue(string propertypath, Object val, bool isNeedValidation = false, bool isEqualsValue = true, object valueChangedParam = null);
        [OperationContract]
        EventCodeEnum SetValue(string propertypath, Object val, bool isNeedValidation = false);
        [OperationContract]
        EventCodeEnum ApplyAltParamToElement();
        [OperationContract]
        bool CheckExistAltParamInParameterObject(object paramFileObj, ref string msg);
    }

    public interface ILoaderParamManager : IParamManager
    {

    }

}
