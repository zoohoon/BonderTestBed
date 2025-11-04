namespace ProberInterfaces.Param
{
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces.Data;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;

    public class StageMoveLockParameter : INotifyPropertyChanged, ISystemParameterizable, IParamNode, IStageMoveLockParameter
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
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

        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }

        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public string FilePath { get; } = "";

        [ParamIgnore]
        [XmlIgnore, JsonIgnore]
        public string FileName { get; } = "StageMoveLockParameter.json";

        private Element<bool> _LockAfterInit
            = new Element<bool>();
        public Element<bool> LockAfterInit
        {
            get { return _LockAfterInit; }
            set
            {
                if (value != _LockAfterInit)
                {
                    _LockAfterInit = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _DoorInterLockEnable
            = new Element<bool>();
        public Element<bool> DoorInterLockEnable
        {
            get { return _DoorInterLockEnable; }
            set
            {
                if (value != _DoorInterLockEnable)
                {
                    _DoorInterLockEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }


        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                DoorInterLockEnable.Value = false;
                LockAfterInit.Value = false;

                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public void SetElementMetaData()
        {
            LockAfterInit.CategoryID = "10015";
            LockAfterInit.ElementName = "LockAfterInit Enable";
            LockAfterInit.Description = "StageLock after Initialize";
            LockAfterInit.ReadMaskingLevel = 0;
            LockAfterInit.WriteMaskingLevel = 0;

            DoorInterLockEnable.CategoryID = "10015";
            DoorInterLockEnable.ElementName = "BackSide Door Interlock Enable";
            DoorInterLockEnable.Description = "BackSide Door Open Interlock";
            DoorInterLockEnable.ReadMaskingLevel = 0;
            DoorInterLockEnable.WriteMaskingLevel = 0;
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
    }
}
