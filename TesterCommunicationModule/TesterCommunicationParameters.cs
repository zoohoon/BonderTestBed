using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using LogModule;
using Newtonsoft.Json;
using ProberInterfaces.Communication.Tester;

namespace TesterCommunicationModule
{

    [Serializable]
    public class TesterCommunicationSysParam : ISystemParameterizable, INotifyPropertyChanged, ITesterComSysParam, IParam, IParamNode
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }

        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (AppDomain.CurrentDomain.FriendlyName == "ProberEmulator.exe")
                {
                    EnumTesterComType.Value = ProberInterfaces.Communication.Tester.EnumTesterComType.TCPIP;
                }

                LoggerManager.Debug($"[TesterCommunicationSysParam], Init() : EnumTesterComType = {EnumTesterComType.Value}");

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
            this.EnumTesterComType.ElementName = "Tester communication method";
            this.EnumTesterComType.ElementAdmin = "Alvin";
            this.EnumTesterComType.CategoryID = "30005";
            this.EnumTesterComType.Description = "Set the communication method with the tester";
        }

        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        [ParamIgnore]
        public string FilePath { get; } = "TesterCommunication";

        [ParamIgnore]
        public string FileName { get; } = "TesterCommunicationSysParam.Json";
        public string Genealogy { get; set; } = "TesterCommunicationSysParam";
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

        public TesterCommunicationSysParam()
        {
        }

        private Element<EnumTesterComType> _EnumTesterComType = new Element<EnumTesterComType>();
        public Element<EnumTesterComType> EnumTesterComType
        {
            get { return _EnumTesterComType; }
            set
            {
                if (value != _EnumTesterComType)
                {
                    _EnumTesterComType = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw new Exception("Error during Setting Default Param From GpibSysParam.");
            }

            return RetVal;
        }

        public EventCodeEnum SetDefaultParam()
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
    }
}
