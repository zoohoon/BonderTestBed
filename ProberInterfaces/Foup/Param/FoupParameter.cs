using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ProberErrorCode;

using LogModule;
using Newtonsoft.Json;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace ProberInterfaces.Foup
{
    public enum FoupTypeEnum
    {
        UNDEFINED,
        TOP,
        FLAT,
        CST8PORT_FLAT
    }
    /// <summary>
    /// FOUP (Front-Opening Unified Pod): 300mm 웨이퍼를 보관하는 데 사용되는 표준 카세트입니다. FOUP는 웨이퍼의 오염을 방지하기 위해 밀폐된 구조를 가지고 있으며, 앞면이 열려 로봇이 웨이퍼를 자동으로 꺼낼 수 있습니다.
    /// FOSB(Front-Opening Shipping Box) : 웨이퍼를 안전하게 운반하기 위한 상자 형태의 카세트로, 주로 300mm 웨이퍼를 장거리 운송할 때 사용됩니다.FOUP와 달리 FOSB는 공정이 아닌 운송에 특화되어 있습니다.
    /// SMIF (Standard Mechanical Interface) Pod: 200mm 웨이퍼용으로 개발된 카세트입니다.SMIF 시스템은 공기 중의 오염 물질로부터 웨이퍼를 보호하면서도 자동화 장비와의 인터페이스를 제공합니다.
    /// Open Cassette: 초기 반도체 제조에서 널리 사용되던 카세트로, 150mm와 200mm 웨이퍼를 보관할 수 있습니다.Open Cassette는 밀폐되지 않은 구조로, 클린룸 내에서 웨이퍼를 다루는 데 사용됩니다.
    /// FOUP -> 13,25 가 있을 예정
    /// Open Cassette 는 6,8inch 모두를 의미
    /// </summary>
    public enum CassetteTypeEnum
    {
        UNDEFINED = -1,
        FOUP_25,
        FOUP_13,
        //FOSB,
        //SMIF,
        //OPENCASSETTE
    }

    [Serializable]
    public class FoupManagerSystemParameter : INotifyPropertyChanged, ISystemParameterizable
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

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
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
        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    FoupModules = new ObservableCollection<FoupSystemParam>();
                    FoupSystemParam sysParam = new FoupSystemParam();
                    sysParam.FoupType.Value = FoupTypeEnum.CST8PORT_FLAT;
                    FoupModules.Add(sysParam);
                }
                else if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    FoupModules = new ObservableCollection<FoupSystemParam>();
                    FoupSystemParam sysParam = new FoupSystemParam();
                    sysParam.FoupType.Value = FoupTypeEnum.UNDEFINED;
                    sysParam.CassetteType.Value = CassetteTypeEnum.FOUP_25;
                    FoupModules.Add(sysParam);
                    sysParam = new FoupSystemParam();
                    sysParam.FoupType.Value = FoupTypeEnum.UNDEFINED;
                    sysParam.CassetteType.Value = CassetteTypeEnum.FOUP_25;
                    FoupModules.Add(sysParam);
                    sysParam = new FoupSystemParam();
                    sysParam.FoupType.Value = FoupTypeEnum.UNDEFINED;
                    sysParam.CassetteType.Value = CassetteTypeEnum.FOUP_25;
                    FoupModules.Add(sysParam);                    
                    
                }
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

        }
        private ObservableCollection<FoupSystemParam> _FoupModules
            = new ObservableCollection<FoupSystemParam>();
        public ObservableCollection<FoupSystemParam> FoupModules
        {
            get { return _FoupModules; }
            set { _FoupModules = value; NotifyPropertyChanged(); }
        }

        public string FilePath => "FOUP";

        public string FileName => "FoupManagerSysParam.json";
    }
    [Serializable]
    public class FoupManagerDeviceParameter : INotifyPropertyChanged, IDeviceParameterizable
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

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {

                try
                {
                    retval = EventCodeEnum.NONE;
                }
                catch (Exception err)
                {
                    LoggerManager.Debug($"[FoupManagerDeviceParameter] [Method = Init] [Error = {err}]");
                    retval = EventCodeEnum.PARAM_ERROR;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }
        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }

        public EventCodeEnum SetDefaultGPParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
               
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }

        
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    FoupModules = new ObservableCollection<FoupDeviceParam>();
                    FoupDeviceParam devParam = new FoupDeviceParam();
                    devParam.SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                    devParam.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    FoupModules.Add(devParam);
                }else if(SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    FoupModules = new ObservableCollection<FoupDeviceParam>();
                    FoupDeviceParam devParam = new FoupDeviceParam();
                    devParam.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                    devParam.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    FoupModules.Add(devParam);
                    devParam = new FoupDeviceParam();
                    devParam.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                    devParam.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    FoupModules.Add(devParam);
                    devParam = new FoupDeviceParam();
                    
                    devParam.SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                    devParam.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                    FoupModules.Add(devParam);
                }
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }
        public void SetElementMetaData()
        {

        }
        private ObservableCollection<FoupDeviceParam> _FoupModules;
        public ObservableCollection<FoupDeviceParam> FoupModules
        {
            get { return _FoupModules; }
            set { _FoupModules = value; NotifyPropertyChanged(); }
        }

        public string FilePath => "FOUP";

        public string FileName => "FoupManagerDevParam.json";
    }
    [Serializable]
    public class FoupSystemParam : INotifyPropertyChanged, IParamNode
    {
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private Element<FoupServiceTypeEnum> _ServiceType = new Element<FoupServiceTypeEnum>();
        public Element<FoupServiceTypeEnum> ServiceType
        {
            get { return _ServiceType; }
            set
            {
                if (value != _ServiceType)
                {
                    _ServiceType = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private Element<FoupTypeEnum> _FoupType = new Element<FoupTypeEnum>();
        public Element<FoupTypeEnum> FoupType
        {
            get { return _FoupType; }
            set
            {
                if (value != _FoupType)
                {
                    _FoupType = value;
                    NotifyPropertyChanged();
                }
            }
        }
      

        private Element<FoupModeStatusEnum> _FoupModeStatus = new Element<FoupModeStatusEnum>();
        public Element<FoupModeStatusEnum> FoupModeStatus
        {
            get { return _FoupModeStatus; }
            set
            {
                if (value != _FoupModeStatus)
                {
                    _FoupModeStatus = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private Element<CassetteTypeEnum> _CassetteType = new Element<CassetteTypeEnum>();
        public Element<CassetteTypeEnum> CassetteType
        {
            get { return _CassetteType; }
            set
            {
                if (value != _CassetteType)
                {
                    _CassetteType = value;
                    NotifyPropertyChanged();
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

        public List<object> Nodes { get; set; }
    }
    [Serializable]
    public class FoupDeviceParam : INotifyPropertyChanged, IParamNode
    {
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private Element<SubstrateTypeEnum> _SubstrateType = new Element<SubstrateTypeEnum>();
        public Element<SubstrateTypeEnum> SubstrateType
        {
            get { return _SubstrateType; }
            set { _SubstrateType = value; NotifyPropertyChanged(); }
        }

        private Element<SubstrateSizeEnum> _SubstrateSize = new Element<SubstrateSizeEnum>();
        public Element<SubstrateSizeEnum> SubstrateSize
        {
            get { return _SubstrateSize; }
            set { _SubstrateSize = value; NotifyPropertyChanged(); }
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

    }

    [Serializable]
    public class CassetteConfigurationParameter : INotifyPropertyChanged, ISystemParameterizable
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

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
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
        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                CassetteModules = new ObservableCollection<CassetteConfigurationParam>();
                CassetteConfigurationParam cassetteConfigurationParam = new CassetteConfigurationParam();
                cassetteConfigurationParam.Enable.Value = true;
                cassetteConfigurationParam.CassetteType.Value = CassetteTypeEnum.FOUP_25;
                cassetteConfigurationParam.CheckCondition.Add("DI_CST12_PRESs", true);
                cassetteConfigurationParam.CheckCondition.Add("DI_CST12_PRES2s", true);
                
                CassetteModules.Add(cassetteConfigurationParam);

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

        }
        private ObservableCollection<CassetteConfigurationParam> _CassetteModules
            = new ObservableCollection<CassetteConfigurationParam>();
        public ObservableCollection<CassetteConfigurationParam> CassetteModules
        {
            get { return _CassetteModules; }
            set { _CassetteModules = value; NotifyPropertyChanged(); }
        }

        public string FilePath => "FOUP";

        public string FileName => "CassetteTypeConfigurationParam.json";
    }

    [Serializable]
    public class CassetteConfigurationParam : INotifyPropertyChanged, IParamNode
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

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
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

        private Element<CassetteTypeEnum> _CassetteType = new Element<CassetteTypeEnum>();
        public Element<CassetteTypeEnum> CassetteType
        {
            get { return _CassetteType; }
            set
            {
                if (value != _CassetteType)
                {
                    _CassetteType = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private Element<bool> _Enable = new Element<bool>();
        public Element<bool> Enable
        {
            get { return _Enable; }
            set
            {
                if (value != _Enable)
                {
                    _Enable = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private Dictionary<string, bool> _CheckCondition = new Dictionary<string, bool>();
        public Dictionary<string, bool> CheckCondition
        {
            get { return _CheckCondition; }
            set
            {
                if (value != _CheckCondition)
                {
                    _CheckCondition = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }      
}
