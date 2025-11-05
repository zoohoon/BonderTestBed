using System;
using System.Collections.Generic;

namespace TemplateParameters
{
    using ProberErrorCode;
    using ProberInterfaces;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Reflection;
    using System.Xml.Serialization;
    using LogModule;
    using Newtonsoft.Json;

    public class TemplateDescriptor : INotifyPropertyChanged, IParam, ISystemParameterizable
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }
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
        public void SetElementMetaData()
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private ObservableCollection<TemplateParameter> _TemplateModules
            = new ObservableCollection<TemplateParameter>();
        public ObservableCollection<TemplateParameter> TemplateModules
        {
            get { return _TemplateModules; }
            set
            {
                if (value != _TemplateModules)
                {
                    _TemplateModules = value;
                    NotifyPropertyChanged("TemplateModules");
                }
            }
        }

        public string FilePath { get; } = "Template";

        public string FileName { get; } = "TempateParameter.json";

        public TemplateDescriptor()
        {

        }
        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Assembly myAssembly = Assembly.GetExecutingAssembly();

            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "SetDefaultParam() : Error occurred.");
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public string GetFilePath()
        {
            return FilePath;
        }

        public string GetFileName()
        {
            return FileName;
        }


        #region //.. WaferAlignDefaultSetting



        #endregion

    }

   
}
