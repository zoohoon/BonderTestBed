using LogModule;
using System;
using System.Collections.Generic;

namespace ProberInterfaces.AlignEX
{
    using Newtonsoft.Json;
    using ProberErrorCode;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;

    [Serializable]
    public abstract class AlginParamBase : IParam, INotifyPropertyChanged , IDeviceParameterizable, IParamNode
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public virtual EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = InitParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retval = EventCodeEnum.PARAM_ERROR;
            }
            return retval;
        }
        public void SetElementMetaData()
        {

        }

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
     
        //Version
        private Version _Version;
        public Version Version
        {
            get { return _Version; }
            set
            {
                if (value != _Version)
                {
                    _Version = value;
                    RaisePropertyChanged();
                }
            }
        }


        private string _Acquistion;
        public string Acquistion
        {
            get { return _Acquistion; }
            set
            {
                if (value != _Acquistion)
                {
                    _Acquistion = value;
                    RaisePropertyChanged();
                }
            }
        }


        private EnumProberCam _CamType
             = new EnumProberCam();
        public EnumProberCam CamType
        {
            get { return _CamType; }
            set
            {
                if (value != _CamType)
                {
                    _CamType = value;
                    RaisePropertyChanged();
                }
            }
        }


        public abstract string FilePath { get; }

        public abstract string FileName { get; }

        public abstract EventCodeEnum SetDefaultParam();

        public abstract EventCodeEnum SetEmulParam();

        public abstract EventCodeEnum InitParam();
    }


}
