namespace ProberInterfaces.Param
{
    using LogModule;
    using ProberErrorCode;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;

    [Serializable]
    public class AlternateSysParameter : IParam, ISystemParameterizable, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region <remarks> ISystemParameterizable Property <remarks>
        [field: NonSerialized]
        [ParamIgnore, XmlIgnore]
        public string FilePath { get; } = "AltParam";
        [field: NonSerialized]
        [ParamIgnore, XmlIgnore]
        public string FileName { get; } = "AltSysParam.json";
        [field: NonSerialized]
        [ParamIgnore, XmlIgnore]
        public bool IsParamChanged { get; set; }
        [field: NonSerialized]
        [ParamIgnore, XmlIgnore]
        public string Genealogy { get; set; }
        [field: NonSerialized]
        [ParamIgnore, XmlIgnore]
        public object Owner { get; set; }
        [field: NonSerialized]
        [ParamIgnore, XmlIgnore]
        public List<object> Nodes { get; set; }
        #endregion

        #region <remarks> Property <remarks>
        private List<AltParamProperty> _AltParamPropertyPathList
             = new List<AltParamProperty>();
        public List<AltParamProperty> AltParamPropertyPathList
        {
            get { return _AltParamPropertyPathList; }
            set
            {
                if (value != _AltParamPropertyPathList)
                {
                    _AltParamPropertyPathList = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion
        public EventCodeEnum Init()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = SetDefaultParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (AltParamPropertyPathList == null)
                {
                    AltParamPropertyPathList = new List<AltParamProperty>();
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void SetElementMetaData()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    [Serializable]
    public class AltParamProperty : IParamNode, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region <remarks> IParamNode Property <remarks>
        [field: NonSerialized]
        [ParamIgnore, XmlIgnore]
        public string Genealogy { get; set; }
        [field: NonSerialized]
        [ParamIgnore, XmlIgnore]
        public object Owner { get; set; }
        [field: NonSerialized]
        [ParamIgnore, XmlIgnore]
        public List<object> Nodes { get; set; }
        #endregion

        private string _PropertyPath;
        public string PropertyPath
        {
            get { return _PropertyPath; }
            set
            {
                if (value != _PropertyPath)
                {
                    _PropertyPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private object _Value;
        public object Value
        {
            get { return _Value; }
            set
            {
                if (value != _Value)
                {
                    _Value = value;
                    RaisePropertyChanged();
                }
            }
        }

        public AltParamProperty(string propertyPath, object value)
        {
            PropertyPath = propertyPath;
            Value = value;
        }
    }
}


