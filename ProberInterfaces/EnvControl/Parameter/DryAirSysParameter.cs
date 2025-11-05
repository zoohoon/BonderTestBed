using System;
using System.Collections.Generic;

namespace ProberInterfaces.EnvControl.Parameter
{
    using LogModule;
    using ProberErrorCode;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    public interface IDryAirSysParameter
    {
        Element<EnumDryAirModuleMode> DryAirModuleMode { get; set; }
        Element<double> ActivatableHighTemp { get; set; }
    }

    [DataContract]
    public class DryAirSysParameter : INotifyPropertyChanged, IParamNode, IDryAirSysParameter
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region .. IParam Property
        public string Genealogy { get; set; }
        public object Owner { get; set; }
        public List<object> Nodes { get; set; }
        #endregion

        private Element<EnumDryAirModuleMode> _DryAirModuleMode
             = new Element<EnumDryAirModuleMode>();
        [DataMember]
        public Element<EnumDryAirModuleMode> DryAirModuleMode
        {
            get { return _DryAirModuleMode; }
            set
            {
                if (value != _DryAirModuleMode)
                {
                    _DryAirModuleMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _ActivatableHighTemp
            = new Element<double>() { Value = 40};
        //Dry Air 동작 온도
        [DataMember]
        public Element<double> ActivatableHighTemp
        {
            get { return _ActivatableHighTemp; }
            set
            {
                if (value != _ActivatableHighTemp)
                {
                    _ActivatableHighTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        public DryAirSysParameter()
        {

        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                InitParam();
                retVal = EventCodeEnum.NONE;
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
                InitParam();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void InitParam()
        {
            if (System.AppDomain.CurrentDomain.FriendlyName == "ProberSystem.exe")
            {
                DryAirModuleMode.Value = EnumDryAirModuleMode.REMOTE;
            }
            else if (System.AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")
            {
                DryAirModuleMode.Value = EnumDryAirModuleMode.HUBER;
            }
        }
    }
}
