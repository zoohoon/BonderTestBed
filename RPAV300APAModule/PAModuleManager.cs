using LogModule;
using Newtonsoft.Json;
using ProberInterfaces.PreAligner;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using ProberErrorCode;
using System.Runtime.CompilerServices;
using ProberInterfaces;
using System.Xml.Serialization;

namespace PAModule
{

    public class PAModuleManager: INotifyPropertyChanged, IPAManager
    {
        #region //..PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        public void DeInitModule()
        {
            foreach (var pa in PAModules)
            {
                pa.DeInitModule();
            }
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                PAModules = new List<IPreAligner>();
                foreach (var commInfo in PADesc.CommInfos)
                {
                    PAVPAModule pa = new PAVPAModule();
                    pa.SetParam(commInfo);
                    pa.InitModule();
                    PAModules.Add(pa);
                }
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"PAModuleManager.InitModule(): Error occurred. Err = {err.Message}");
            }
            return ret;
        }

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            IParam tmpParam = null;
            ret = this.LoadParameter(ref tmpParam, typeof(PADescriptor));
            if (ret == EventCodeEnum.NONE)
            {
                PADesc = (PADescriptor)tmpParam;
            }
            return ret;
        }
        PADescriptor PADesc;
        private EventCodeEnum LoadPAParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            PADesc = new PADescriptor();
            try
            {
                IParam tmpParam = null;
                RetVal = this.LoadParameter(ref tmpParam, typeof(PADescriptor));
                if (RetVal == EventCodeEnum.NONE)
                {
                    PADesc = tmpParam as PADescriptor;
                }
                //PADesc.CommInfos
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($String.Format("[ECATIOProvider] LoadSysParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);

            }

            return RetVal;
        }
        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            return ret;
        }


        private List<IPreAligner> _PAModules;
        public List<IPreAligner> PAModules
        {
            get { return _PAModules; }
            set
            {
                if (value != _PAModules)
                {
                    _PAModules = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool Initialized { get; set; }
    }

    [Serializable]
    public class PADescriptor : INotifyPropertyChanged, ISystemParameterizable, IParamNode
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }


        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);


                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public void SetElementMetaData()
        {

        }
        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            SerCommInfo commInfo;
            CommInfos = new List<SerCommInfo>();

            commInfo = new SerCommInfo();
            commInfo.SetDefaultParam();
            commInfo.PortName.Value = "COM1";
            CommInfos.Add(commInfo);

            commInfo = new SerCommInfo();
            commInfo.SetDefaultParam();
            commInfo.PortName.Value = "COM2";
            CommInfos.Add(commInfo);

            commInfo = new SerCommInfo();
            commInfo.SetDefaultParam();
            commInfo.PortName.Value = "COM3";
            commInfo.IsAttached.Value = false;
            CommInfos.Add(commInfo);
            return EventCodeEnum.NONE;
        }
        private List<SerCommInfo> _CommInfos;
        public List<SerCommInfo> CommInfos
        {
            get { return _CommInfos; }
            set
            {
                if (value != _CommInfos)
                {
                    _CommInfos = value;
                    RaisePropertyChanged();
                }
            }
        }

        [ParamIgnore]
        public string FilePath { get; } = "PreAligner";
        [ParamIgnore]
        public string FileName { get; } = "PAManagerParam.Json";
        [ParamIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
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

    }
}
