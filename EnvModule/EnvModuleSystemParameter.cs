namespace EnvModule
{
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Serialization;

    [Serializable]
    public class EnvModuleSystemParameter : ISystemParameterizable, IParamNode, IParam
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
        public string FilePath { get; } = "";
        [field: NonSerialized]
        [ParamIgnore, XmlIgnore]
        public string FileName { get; } = "EnvSystemParameter.json";
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

        private List<IEnvConditionChecker> _EnvConditionCheckerList
             = new List<IEnvConditionChecker>();
        public List<IEnvConditionChecker> EnvConditionCheckerList
        {
            get { return _EnvConditionCheckerList; }
            set
            {
                if (value != _EnvConditionCheckerList)
                {
                    _EnvConditionCheckerList = value;
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
                if(EnvConditionCheckerList == null)
                {
                    EnvConditionCheckerList = new List<IEnvConditionChecker>();
                }

                EnvConditionCheckerList.Add(new TemperatureChecker());
                //EnvConditionCheckerList.Add(new DewPointChecker());
                EnvConditionCheckerList.Add(new ChillerChecker());
                //EnvConditionCheckerList.Add(new TopPurgeChecker());
                //EnvConditionCheckerList.Add(new DryAirChecker());

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
    
    public interface IEnvConditionChecker : IFactoryModule
    {
        Element<string> CheckerClassName { get; set; }
        long ErrorOccurredTimeoutSec { get; set; }
        Nullable<DateTime> ErrorOccurredTime { get; set; }
        bool NotifyEnable { get; set; }
        EventCodeEnum Init();
        EventCodeEnum Checking(out string errorMsg);
    }
}
