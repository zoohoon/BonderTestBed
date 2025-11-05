using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LogModule;
using Newtonsoft.Json;
using System.Xml.Serialization;

namespace StageMoveParameter
{

    [Serializable]
    public class StageMoveParam : ISystemParameterizable, INotifyPropertyChanged, IParamNode
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
                LoggerManager.Debug($"[StageMoveParam] [Method = Init] [Error = {err}]");
                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }
        public void SetElementMetaData()
        {

        }
        

        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public string FilePath { get; } = "";

        public string FileName { get; } = "StageMoveParam.json";

        private Element<string> _OPUSVClassName = new Element<string>();
        public Element<string> OPUSVClassName
        {
            get { return _OPUSVClassName; }
            set
            {
                if (value != _OPUSVClassName)
                {
                    _OPUSVClassName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _OPUSVMoveDLLName = new Element<string>();
        public Element<string> OPUSVMoveDLLName
        {
            get { return _OPUSVMoveDLLName; }
            set
            {
                if (value != _OPUSVMoveDLLName)
                {
                    _OPUSVMoveDLLName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _UseOPUSVMove = new Element<bool>();
        public Element<bool> UseOPUSVMove
        {
            get { return _UseOPUSVMove; }
            set
            {
                if (value != _UseOPUSVMove)
                {
                    _UseOPUSVMove = value;
                    RaisePropertyChanged();
                }
            }
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

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                SetDefaultParam_OPUSV();
                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw new Exception($"Error during Setting Default Param From {this.GetType().Name}. {err.Message}");
            }

            return RetVal;
        }

        private void SetDefaultParam_OPUSV()
        {
            try
            {
                OPUSVClassName.Value = "OPUSVStageMove";
                OPUSVMoveDLLName.Value = "OpusVStageMove.dll";

                UseOPUSVMove.Value = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                SetDefaultParam_OPUSV();
                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw new Exception($"Error during Setting Default Param From {this.GetType().Name}. {err.Message}");
            }
            return RetVal;
        }
    }
}
