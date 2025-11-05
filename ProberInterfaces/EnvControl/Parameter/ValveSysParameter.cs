

namespace ProberInterfaces.EnvControl.Parameter
{
    using LogModule;
    using ProberErrorCode;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    public interface IValveSysParameter
    {
        Element<EnumValveModuleType> ValveModuleType { get; set; }
        List<ValveMappingParameter> ValveMappingParam { get; }
        List<ValveMappingParameter> DryAirMappingParam { get; }
    }

    [DataContract]
    public class ValveSysParameter : INotifyPropertyChanged , IValveSysParameter
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

        private Element<EnumValveModuleType> _ValveModuleType
             = new Element<EnumValveModuleType>();
        [DataMember]
        public Element<EnumValveModuleType> ValveModuleType
        {
            get { return _ValveModuleType; }
            set
            {
                if (value != _ValveModuleType)
                {
                    _ValveModuleType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<ValveMappingParameter> _ValveMappingParam
            = new List<ValveMappingParameter>();
        /// <summary>
        /// coolant의 valve Mapping 정보. 
        /// </summary>
        [DataMember]
        public List<ValveMappingParameter> ValveMappingParam
        {
            get { return _ValveMappingParam; }
            set
            {
                if (value != _ValveMappingParam)
                {
                    _ValveMappingParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _ValveWaitTimeout_msec = new Element<int>() { Value = 4500 };// Default : SettlingTime * 3
        /// <summary>
        /// SetValve를 할때 원하는 Valve 상태가 될때까지 기다리는 시간
        /// </summary>
        [DataMember]
        public Element<int> ValveWaitTimeout_msec
        {
            get { return _ValveWaitTimeout_msec; }
            set
            {
                if (value != _ValveWaitTimeout_msec)
                {
                    _ValveWaitTimeout_msec = value;
                    RaisePropertyChanged();
                }
            }
        }

       

        private Element<int> _ValveSettlingTime_msec = new Element<int>() { Value = 1500 };// Default : 실제 동작 시간 * 1.5
        /// <summary>
        /// SetValve를 한 후 Valve 동작이 완료될 때 까지 기다리는 시간
        /// </summary>
        [DataMember]
        public Element<int> ValveSettlingTime_msec
        {
            get { return _ValveSettlingTime_msec; }
            set
            {
                if (value != _ValveSettlingTime_msec)
                {
                    _ValveSettlingTime_msec = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<ValveMappingParameter> _DryAirMappingParam
            = new List<ValveMappingParameter>();
        [DataMember]
        public List<ValveMappingParameter> DryAirMappingParam
        {
            get { return _DryAirMappingParam; }
            set
            {
                if (value != _DryAirMappingParam)
                {
                    _DryAirMappingParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ValveModuleType.Value = EnumValveModuleType.MODBUS;
                if(ValveMappingParam != null)
                {
                    ValveMappingParam = new List<ValveMappingParameter>();
                }
                ValveMappingParam.Add(new ValveMappingParameter(1, 3));
                ValveMappingParam.Add(new ValveMappingParameter(2, 2));
                ValveMappingParam.Add(new ValveMappingParameter(3, 1));
                ValveMappingParam.Add(new ValveMappingParameter(4, 0));
                ValveMappingParam.Add(new ValveMappingParameter(5, 3));
                ValveMappingParam.Add(new ValveMappingParameter(6, 2));
                ValveMappingParam.Add(new ValveMappingParameter(7, 1));
                ValveMappingParam.Add(new ValveMappingParameter(8, 0));
                ValveMappingParam.Add(new ValveMappingParameter(9, 0));
                ValveMappingParam.Add(new ValveMappingParameter(10, 1));
                ValveMappingParam.Add(new ValveMappingParameter(11, 2));
                ValveMappingParam.Add(new ValveMappingParameter(12, 3));

                SetGPDryAirMapping();

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
                SetDefaultParam();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void SetGPDryAirMapping()
        {
            try
            {
                if (DryAirMappingParam != null)
                {
                    DryAirMappingParam = new List<ValveMappingParameter>();
                }
                if (SystemManager.SystemType == SystemTypeEnum.Opera)
                {
                    /// Chiller #1
                    DryAirMappingParam.Add(new ValveMappingParameter(9, 1));
                    DryAirMappingParam.Add(new ValveMappingParameter(10, 2));
                    DryAirMappingParam.Add(new ValveMappingParameter(11, 3));
                    DryAirMappingParam.Add(new ValveMappingParameter(12, 4));
                    /// Chiller #2
                    DryAirMappingParam.Add(new ValveMappingParameter(5, 5));
                    DryAirMappingParam.Add(new ValveMappingParameter(6, 6));
                    DryAirMappingParam.Add(new ValveMappingParameter(7, 7));
                    DryAirMappingParam.Add(new ValveMappingParameter(8, 8));
                    /// Chiller #3
                    DryAirMappingParam.Add(new ValveMappingParameter(1, 9));
                    DryAirMappingParam.Add(new ValveMappingParameter(2, 10));
                    DryAirMappingParam.Add(new ValveMappingParameter(3, 11));
                    DryAirMappingParam.Add(new ValveMappingParameter(4, 12));
                }
                else if (SystemManager.SystemType == SystemTypeEnum.DRAX)
                {
                    /// Chiller #1
                    DryAirMappingParam.Add(new ValveMappingParameter(1, 1));
                    DryAirMappingParam.Add(new ValveMappingParameter(2, 2));
                    DryAirMappingParam.Add(new ValveMappingParameter(7, 3));
                    DryAirMappingParam.Add(new ValveMappingParameter(8, 4));
                    /// Chiller #2
                    DryAirMappingParam.Add(new ValveMappingParameter(3, 5));
                    DryAirMappingParam.Add(new ValveMappingParameter(4, 6));
                    DryAirMappingParam.Add(new ValveMappingParameter(5, 9));
                    DryAirMappingParam.Add(new ValveMappingParameter(6, 10));
                    /// Chiller #3
                    DryAirMappingParam.Add(new ValveMappingParameter(9, 7));
                    DryAirMappingParam.Add(new ValveMappingParameter(10, 8));
                    DryAirMappingParam.Add(new ValveMappingParameter(11, 11));
                    DryAirMappingParam.Add(new ValveMappingParameter(12, 12));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    [DataContract]
    public class ValveMappingParameter : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private int _StageIndex;
        [DataMember]
        public int StageIndex
        {
            get { return _StageIndex; }
            set
            {
                if (value != _StageIndex)
                {
                    _StageIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _ValveIndex;
        [DataMember]
        public int ValveIndex
        {
            get { return _ValveIndex; }
            set
            {
                if (value != _ValveIndex)
                {
                    _ValveIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ValveMappingParameter(int stageidx, int valveidx)
        {
            this.StageIndex = stageidx;
            this.ValveIndex = valveidx;
        }
    }

    public enum ColdValveControlEnum
    {
        UNDEFINE,
        REMOTE, // Chiller 등 다른 하드웨어 내부
        LOCAL //PLC
    }
}
