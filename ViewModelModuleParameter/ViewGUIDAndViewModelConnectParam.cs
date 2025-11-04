using System;
using System.Collections.Generic;

namespace ViewModelModuleParameter
{
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;

    [Serializable]
    public class ViewGUIDAndViewModelConnectParam : IParam, ISystemParameterizable
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }
        [NonSerialized]
        private Object _Owner;
        [JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }

        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[ViewGUIDAndViewModelConnectParam] [Method = Init] [Error = {err}]");
                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }
        public void SetElementMetaData()
        {

        }
        public string FilePath { get; } = "";

        public string FileName { get; } = "ProberViewGUIDConnect.json";


        private List<ViewGUIDConnectInfo> _ConnectionInfos = new List<ViewGUIDConnectInfo>();
        public List<ViewGUIDConnectInfo> ConnectionInfos
        {
            get { return _ConnectionInfos; }
            set
            {
                if (value != _ConnectionInfos)
                {
                    _ConnectionInfos = value;
                }
            }
        }

        private string _ParamLabel;
        public string Genealogy
        {
            get { return _ParamLabel; }
            set { _ParamLabel = value; }
        }

        private ViewGUIDConnectInfo MakeViewConnectInfo(string viewmodelInterface, Guid viewguid)
        {
            ViewGUIDConnectInfo tmp = new ViewGUIDConnectInfo();
            try
            {

                tmp.ViewModelInterface = viewmodelInterface;
                tmp.ViewGUID = viewguid;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return tmp;
        }
        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            // String and ViewGUID
            //_ConnectionInfos.Add(MakeViewConnectInfo("ILotView", new Guid("6223DFD5-EFAA-4B49-AB70-D8A5F03FA65D")));
            ConnectionInfos.Add(MakeViewConnectInfo("IPnpSetup", new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0")));
            ConnectionInfos.Add(MakeViewConnectInfo("IPnpManager", new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0")));
            return EventCodeEnum.NONE;
        }
    }
}
