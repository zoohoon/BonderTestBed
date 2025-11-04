using System;
using System.Collections.Generic;
using System.Linq;

namespace LoaderServiceClientModules.ParamManager
{
    using Autofac;
    using LogModule;

    using ProberErrorCode;
    using ProberInterfaces;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using LoaderBase.Communication;
    using SerializerUtil;
    using ProberInterfaces.Proxies;
    using System.Collections.Concurrent;

    public class ParamManagerServiceClient : ILoaderParamManager, INotifyPropertyChanged, IFactoryModule
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] String info = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        private ILoaderCommunicationManager _LoaderCommunicationManager;
        private ILoaderCommunicationManager LoaderCommunicationManager
        {
            get
            {
                if (_LoaderCommunicationManager == null)
                {
                    _LoaderCommunicationManager = this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
                }
                return _LoaderCommunicationManager;
            }
        }

        private int _DevUpdateStageIndex = -1;
        private int _SysUpdateStageIndex = -1;

        private ConcurrentDictionary<String, ConcurrentDictionary<String, IElement>> _DevDBElementDictionary = new ConcurrentDictionary<String, ConcurrentDictionary<String, IElement>>(StringComparer.OrdinalIgnoreCase);
        public ConcurrentDictionary<String, ConcurrentDictionary<String, IElement>> DevDBElementDictionary
        {
            get { return _DevDBElementDictionary; }
            set { _DevDBElementDictionary = value; }
        }
        private ConcurrentDictionary<String, ConcurrentDictionary<String, IElement>> _SysDBElementDictionary = new ConcurrentDictionary<string, ConcurrentDictionary<String, IElement>>(StringComparer.OrdinalIgnoreCase);
        public ConcurrentDictionary<String, ConcurrentDictionary<String, IElement>> SysDBElementDictionary
        {
            get { return _SysDBElementDictionary; }
            set { _SysDBElementDictionary = value; }
        }
        private ConcurrentDictionary<String, ConcurrentDictionary<String, IElement>> _CommonDBElementDictionary = new ConcurrentDictionary<string, ConcurrentDictionary<String, IElement>>(StringComparer.OrdinalIgnoreCase);
        public ConcurrentDictionary<String, ConcurrentDictionary<String, IElement>> CommonDBElementDictionary
        {
            get { return _CommonDBElementDictionary; }
            set { _CommonDBElementDictionary = value; }
        }

        private List<IElement> _SysElements = new List<IElement>();
        public List<IElement> SysElements
        {
            get { return _SysElements; }
            set
            {
                if (value != _SysElements)
                {
                    _SysElements = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<IElement> _DevElements = new List<IElement>();
        public List<IElement> DevElements
        {
            get { return _DevElements; }
            set
            {
                if (value != _DevElements)
                {
                    _DevElements = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool Initialized
        {
            get
            {
                if (LoaderCommunicationManager.SelectedStage != null)
                {
                    if (LoaderCommunicationManager.SelectedStage.StageInfo.IsConnected == true)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            set { }
        }
#pragma warning disable 0067
        public event EventHandler OnLoadElementInfoFromDB;
#pragma warning restore 0067
        public void DeInitModule()
        {
            LoggerManager.Debug($"ParamManagerServiceClient(): Deinit module.");
        }

        public void ExportDBtoCSV()
        {
            if (LoaderCommunicationManager.SelectedStage == null)
            {

            }
            else
            {
                LoaderCommunicationManager.GetProxy<IParamManagerProxy>().ExportDBtoCSV();
            }
        }
        public IElement GetAssociateElement(string associateID)
        {
            if (LoaderCommunicationManager.SelectedStage == null)
            {
                return null;
            }
            else
            {
                return LoaderCommunicationManager.GetProxy<IParamManagerProxy>().GetAssociateElement(associateID);
            }
        }
        public List<IElement> GetCommonElementList()
        {
            if (LoaderCommunicationManager.SelectedStage == null)
            {
                return null;
            }
            else
            {
                return LoaderCommunicationManager.GetProxy<IParamManagerProxy>().GetCommonElementList();
            }

        }
        public IElement GetElement(int elementID)
        {
            if (LoaderCommunicationManager.SelectedStage == null)
            {
                return new Element<string>() { Value = "NotDefined" };
            }
            else
            {

                return LoaderCommunicationManager.GetProxy<IParamManagerProxy>().GetElement(elementID);
            }
        }
        public string GetPropertyPathFromVID(long vid)
        {
            return LoaderCommunicationManager.GetProxy<IParamManagerProxy>().GetPropertyPathFromVID(vid);
        }
        public long GetElementIDFormVID(long vid)
        {
            if (LoaderCommunicationManager.SelectedStage == null)
            {
                return -1;
            }
            else
            {

                return LoaderCommunicationManager.GetProxy<IParamManagerProxy>().GetElementIDFormVID(vid);
            }
        }
        public IElement GetElement(string propertyPath)
        {
            return null;
        }
        public string GetElementPath(int elementID)
        {
            return null;
        }
        public List<IElement> GetElementList(List<IElement> emlList, ref byte[] bulkElem)
        {
            object target;
            var dataPack = bulkElem;

            var result = SerializeManager.DeserializeFromByte(dataPack, out target, typeof(ElementPacks), new Type[] { typeof(ElementPack) });

            ElementPacks packs = (ElementPacks)target;

            // Change emlList to a dictionary
            Dictionary<(int, string), IElement> emlDict = new Dictionary<(int, string), IElement>();

            foreach (var element in packs.Elements)
            {
                var key = (element.ElementID, element.CategoryID);

                if (emlDict.TryGetValue(key, out IElement existingElement))
                {
                    existingElement.SetValue(element.GetValue());
                }
                else
                {
                    emlDict.Add(key, (IElement)element);
                }
            }

            // Clear the original emlList and add the updated values from the dictionary
            emlList.Clear();
            foreach (var item in emlDict)
            {
                emlList.Add(item.Value);
            }

            return emlList;
        }

        private object lockSysElement = new object();
        public List<IElement> GetSysElementList()
        {
            List<IElement> retval;

            if (LoaderCommunicationManager.SelectedStage == null)
            {
                retval = new List<IElement>();
            }
            else
            {
                try
                {
                    lock (lockSysElement)
                    {
                        bool IsChanged = false;

                        IsChanged = LoaderCommunicationManager.GetProxy<IParamManagerProxy>().GetChangedSystemParam();

                        if (IsChanged == true | (LoaderCommunicationManager.SelectedStageIndex != _SysUpdateStageIndex))
                        {
                            LoggerManager.Debug($"[{this.GetType().Name}], GetSysElementList() Start");

                            var bulkElem = GetSysElementsBulk();

                            retval = GetElementList(SysElements, ref bulkElem);

                            LoaderCommunicationManager.GetProxy<IParamManagerProxy>().SetChangedSystemParam(false);
                            _SysUpdateStageIndex = LoaderCommunicationManager.SelectedStageIndex;

                            LoggerManager.Debug($"[{this.GetType().Name}], GetSysElementList() End. Cell Index = {LoaderCommunicationManager.SelectedStage.Index}");
                        }
                        else
                        {
                            retval = SysElements;
                        }
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"GetSysElementList(): Error occurred. Err = {err.Message}");
                    retval = null;
                }
            }

            return retval;
        }
        public List<IElement> GetDevElementList()
        {
            List<IElement> retval = null;

            if (LoaderCommunicationManager.SelectedStage == null)
            {
                retval = new List<IElement>();
            }
            else
            {
                try
                {
                    bool IsChanged = false;

                    IsChanged = LoaderCommunicationManager.GetProxy<IParamManagerProxy>().GetChangedDeviceParam();

                    if (IsChanged == true | (LoaderCommunicationManager.SelectedStageIndex != _DevUpdateStageIndex))
                    {
                        LoggerManager.Debug($"[{this.GetType().Name}], GetDevElementList() Start");

                        var bulkElem = GetDevElementsBulk();

                        retval = GetElementList(DevElements, ref bulkElem);

                        LoaderCommunicationManager.GetProxy<IParamManagerProxy>().SetChangedDeviceParam(false);
                        _DevUpdateStageIndex = LoaderCommunicationManager.SelectedStageIndex;
                        
                        LoggerManager.Debug($"[{this.GetType().Name}], GetDevElementList() End. Cell Index = {LoaderCommunicationManager.SelectedStage.Index}");
                    }
                    else
                    {
                        retval = DevElements;
                        var temp = DevElements.Where(x => x.Unit != null).ToList();
                    }

                }
                catch (Exception err)
                {
                    LoggerManager.Error($"GetDevElementList(): Error occurred. Err = {err.Message}");
                    retval = null;
                }
            }

            return retval;
        }
        public EventCodeEnum InitModule()
        {
            return EventCodeEnum.NONE;
        }

        public void LoadComElementInfoFromDB(bool isOccureLoadeEvent = false)
        {
            if (LoaderCommunicationManager.SelectedStage == null)
            {

            }
            else
            {
                LoaderCommunicationManager.GetProxy<IParamManagerProxy>().LoadComElementInfoFromDB();
            }

        }

        public void LoadDevElementInfoFromDB()
        {
            if (LoaderCommunicationManager.SelectedStage == null)
            {

            }
            else
            {
                LoaderCommunicationManager.GetProxy<IParamManagerProxy>().LoadDevElementInfoFromDB();
            }

        }

        public void LoadElementInfoFromDB()
        {
            if (LoaderCommunicationManager.SelectedStage == null)
            {

            }
            else
            {
                LoaderCommunicationManager.GetProxy<IParamManagerProxy>().LoadElementInfoFromDB();
            }

        }

        public void LoadElementInfoFromDB(ParamType paramType, IElement elem, string dbPropertyPath)
        {
            if (LoaderCommunicationManager.SelectedStage == null)
            {

            }
            else
            {
                LoaderCommunicationManager.GetProxy<IParamManagerProxy>().LoadElementInfoFromDB(paramType, elem, dbPropertyPath);
            }

        }

        public void LoadSysElementInfoFromDB()
        {
            if (LoaderCommunicationManager.SelectedStage == null)
            {

            }
            else
            {
                LoaderCommunicationManager.GetProxy<IParamManagerProxy>().LoadSysElementInfoFromDB();
            }

        }

        public void RegistElementToDB()
        {
            if (LoaderCommunicationManager.SelectedStage == null)
            {

            }
            else
            {
                LoaderCommunicationManager.GetProxy<IParamManagerProxy>().RegistElementToDB();
            }

        }

        public void SaveElement(IElement elem, bool isNeedValidation = false)//, string source_classname = null)
        {
            //Editor에서 save시 호출되는 곳
            ElementPacks packs = new ElementPacks();
            packs.Elements.Add((ElementPack)elem);

            var bulkElements = SerializeManager.SerializeToByte(packs, typeof(ElementPacks));
            SaveElementPack(bulkElements, isNeedValidation);//, source_classname);
        }

        public void SaveElement(string categoryID)
        {
            if (LoaderCommunicationManager.SelectedStage == null)
            {

            }
            else
            {
                LoaderCommunicationManager.GetProxy<IParamManagerProxy>().SaveCategory(categoryID);
            }

        }

        public void SetElement(string propPath, Object setval)
        {
            if (LoaderCommunicationManager.SelectedStage == null)
            {

            }
            else
            {
                LoaderCommunicationManager.GetProxy<IParamManagerProxy>().SetElement(propPath, setval);
            }

        }

        public void SyncDBTableByCSV()
        {
            if (LoaderCommunicationManager.SelectedStage == null)
            {

            }
            else
            {
                LoaderCommunicationManager.GetProxy<IParamManagerProxy>().SyncDBTableByCSV();
            }

        }

        public void SaveCategory(string categoryid)
        {
            SaveElement(categoryid);
        }

        public void LoadElementInfosFromDB()
        {
            LoadElementInfoFromDB();
        }

        public bool IsServiceAvailable()
        {
            return true;
        }
        public void InitService()
        {
            LoaderCommunicationManager.GetProxy<IParamManagerProxy>().InitService();
        }

        public byte[] GetDevParamElementsBulk(string paramName)
        {
            if (LoaderCommunicationManager.SelectedStage == null)
            {
                return null;
            }
            else
            {
                return LoaderCommunicationManager.GetProxy<IParamManagerProxy>().GetDevParamElementsBulk(paramName);
            }
        }

        public byte[] GetDevElementsBulk()
        {
            if (LoaderCommunicationManager.SelectedStage == null)
            {
                return null;
            }
            else
            {
                return LoaderCommunicationManager.GetProxy<IParamManagerProxy>().GetDevElementsBulk();
            }
        }

        public byte[] GetSysElementsBulk()
        {
            if (LoaderCommunicationManager.SelectedStage == null)
            {
                return null;
            }
            else
            {

                return LoaderCommunicationManager.GetProxy<IParamManagerProxy>().GetSysElementsBulk();
            }
        }


        public EventCodeEnum CheckOriginSetValueAvailable(string propertypath, object val)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (LoaderCommunicationManager.SelectedStage != null)
            {
                retVal = LoaderCommunicationManager.GetProxy<IParamManagerProxy>().CheckOriginSetValueAvailable(propertypath, val);             
            }
            return retVal;
        }

        public EventCodeEnum CheckSetValueAvailable(string propertypath, object val)//, string source_classname = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (LoaderCommunicationManager.SelectedStage != null)
            {
                retVal = LoaderCommunicationManager.GetProxy<IParamManagerProxy>().CheckSetValueAvailable(propertypath, val);//,  source_classname);
            }
            return retVal;
        }

        public EventCodeEnum SetOriginValue(string propertypath, Object val, bool isNeedValidation = false, bool isEqualsValue = true, object valueChangedParam = null)//, string source_classname = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            //string errormsg = "";
            if (LoaderCommunicationManager.SelectedStage != null)
            {
                retVal = LoaderCommunicationManager.GetProxy<IParamManagerProxy>().SetOriginValue(propertypath, val, isNeedValidation, isEqualsValue, valueChangedParam);//, source_classname);

            }
            //errorlog = errormsg;
            return retVal;
        }


        public EventCodeEnum SetValue(string propertypath, Object val, bool isNeedValidation = false)//, string source_classname = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (LoaderCommunicationManager.SelectedStage != null)
            {
                retVal = LoaderCommunicationManager.GetProxy<IParamManagerProxy>().SetValue(propertypath, val, isNeedValidation);//, source_classname);

            }
            return retVal;
        }

        public void SaveElementPack(byte[] bytepack, bool isNeedValidation = false)//, string source_classname = null)
        {
            if (LoaderCommunicationManager.SelectedStage != null)
            {
                LoaderCommunicationManager.GetProxy<IParamManagerProxy>().SaveElementPack(bytepack, isNeedValidation);//, source_classname);
            }
        }

        public bool IsAvailable()
        {
            if (LoaderCommunicationManager.SelectedStage == null)
            {
                return false;
            }
            else
            {
                return LoaderCommunicationManager.GetProxy<IParamManagerProxy>().IsAvailable();
            }
        }

        public void SetChangedDeviceParam(bool flag)
        {
            try
            {
                if (LoaderCommunicationManager.SelectedStage != null)
                {
                    LoaderCommunicationManager.GetProxy<IParamManagerProxy>().SetChangedDeviceParam(flag);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetChangedSystemParam(bool flag)
        {
            try
            {
                if (LoaderCommunicationManager.SelectedStage != null)
                {
                    LoaderCommunicationManager.GetProxy<IParamManagerProxy>().SetChangedSystemParam(flag);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool GetChangedDeviceParam()
        {
            bool retval = false;

            try
            {
                if (LoaderCommunicationManager.SelectedStage != null)
                {
                    retval = LoaderCommunicationManager.GetProxy<IParamManagerProxy>().GetChangedDeviceParam();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool GetChangedSystemParam()
        {
            bool retval = false;

            try
            {
                if (LoaderCommunicationManager.SelectedStage != null)
                {
                    retval = LoaderCommunicationManager.GetProxy<IParamManagerProxy>().GetChangedSystemParam();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public ParamDevParameter ParamDevParam;
        public EventCodeEnum VerifyLotVIDsCheckBeforeLot()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (LoaderCommunicationManager.SelectedStage != null)
                {
                    retval = LoaderCommunicationManager.GetProxy<IParamManagerProxy>().VerifyLotVIDsCheckBeforeLot();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public ObservableCollection<int> GetVerifyLotVIDs()
        {
            ObservableCollection<int> retVal = new ObservableCollection<int>();
            try
            {
                if (LoaderCommunicationManager.SelectedStage != null)
                {
                    retVal = LoaderCommunicationManager.GetProxy<IParamManagerProxy>().GetVerifyLotVIDs();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void SetVerifyLotVIDs(ObservableCollection<int> vids)
        {
            try
            {
                if (LoaderCommunicationManager.SelectedStage != null)
                {
                    LoaderCommunicationManager.GetProxy<IParamManagerProxy>().SetVerifyLotVIDs(vids);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void UpdateVerifyParam()
        {
            try
            {
                if (ParamDevParam == null)
                {
                    ParamDevParam = new ParamDevParameter();
                }
                ParamDevParam.VerifyParameterBeforeStartLotEnable = GetVerifyParameterBeforeStartLotEnable();
                ParamDevParam.VerifyParamInfos = GetVerifyParamInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public bool GetVerifyParameterBeforeStartLotEnable()
        {
            bool retVal = false;
            try
            {
                if (LoaderCommunicationManager.SelectedStage != null)
                {
                    retVal = LoaderCommunicationManager.GetProxy<IParamManagerProxy>().GetVerifyParameterBeforeStartLotEnable();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public List<VerifyParamInfo> GetVerifyParamInfo()
        {
            List<VerifyParamInfo> retVal = null;
            try
            {
                if (LoaderCommunicationManager.SelectedStage != null)
                {
                    retVal = LoaderCommunicationManager.GetProxy<IParamManagerProxy>().GetVerifyParamInfo();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void SetVerifyParameterBeforeStartLotEnable(bool flag)
        {
            try
            {
                if (LoaderCommunicationManager.SelectedStage != null)
                {
                    LoaderCommunicationManager.GetProxy<IParamManagerProxy>().SetVerifyParameterBeforeStartLotEnable(flag);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetVerifyParamInfo(List<VerifyParamInfo> infos)
        {
            try
            {
                if (LoaderCommunicationManager.SelectedStage != null)
                {
                    LoaderCommunicationManager.GetProxy<IParamManagerProxy>().SetVerifyParamInfo(infos);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum ApplyAltParamToElement()
        {
            return EventCodeEnum.NONE;
        }
        public bool CheckExistAltParamInParameterObject(object paramFileObj, ref string msg)
        {
            msg = "";
            return false;
        }

        #region <remarks> IHasDevParameterizable Methods<remarks>

        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (LoaderCommunicationManager.SelectedStage != null)
                {
                    retVal = LoaderCommunicationManager.GetProxy<IParamManagerProxy>().SaveDevParameter();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public void UpdateLowerLimit(string propertyPath, string setValue)
        {

        }

        public void UpdateUpperLimit(string propertyPath, string setValue)
        {

        }

        public (bool needValidation, IModule source) ClassNameConverter(string source_classname)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region <remarks> IHasSysParameterizable Methods <remarks>
        public EventCodeEnum LoadSysParameter()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SaveSysParameter()
        {
            return EventCodeEnum.NONE;
        }
        #endregion
    }
}
