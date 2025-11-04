using System;
using System.Collections.Generic;

namespace LoaderCore.RemoteService
{
    using ProberErrorCode;
    using ProberInterfaces;
    using LogModule;
    using Newtonsoft.Json;
    using System.ComponentModel;
    public class RemoteIOMappings : INotifyPropertyChanged, ISystemParameterizable, IParamNode
    {
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
        [JsonIgnore]
        public bool IsParamChanged { get; set; }
        public string Genealogy { get; set; }
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
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public EventCodeEnum SetEmulParam()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum SetDefaultParam()
        {
            throw new NotImplementedException();
        }

        public void SetElementMetaData()
        {
        }

        public string FilePath { get; } = "IO";

        public string FileName { get; } = "GPLRemoteIOMapping.Json";


    }


}
