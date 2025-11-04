using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Temperature.EnvMonitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EnvMonitoring
{
    public class AfterErrorBehaviorParam : ISystemParameterizable
    {
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
            }
            return retval;
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                SetDefaultParam();
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                AfterErrorBehaviors = new List<IAfterErrorBehavior>();
                // 순서 정하기
                AfterErrorBehaviors.Add(new ZDown(true));
                AfterErrorBehaviors.Add(new SaveResultMap(true));
                AfterErrorBehaviors.Add(new SystemError(true)); 
          
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public void SetElementMetaData()
        {

        }

        public string FilePath { get; } = "Temperature";

        public string FileName { get; } = "AfterErrorBehaviors.json";

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
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
        public List<object> Nodes { get; set; }
        private List<IAfterErrorBehavior> _AfterErrorBehaviors;
        public List<IAfterErrorBehavior> AfterErrorBehaviors
        {
            get { return _AfterErrorBehaviors; }
            set
            {
                if (value != _AfterErrorBehaviors)
                {
                    _AfterErrorBehaviors = value;
                }
            }
        }
    }
}
