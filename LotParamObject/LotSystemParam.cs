namespace LotParamObject
{
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    [Serializable]
    public class LotSystemParam : INotifyPropertyChanged, ISystemParameterizable
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region <remarks> ISystemParameterizable Property </remarks>
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public string FilePath { get; } = "LOT";

        public string FileName { get; } = "LotSysParameter.json";
        public string Genealogy { get; set; }

        [NonSerialized]
        private Object _Owner;
        [JsonIgnore, ParamIgnore]
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

        [JsonIgnore]
        public List<object> Nodes { get; set; }
        #endregion

        #region <remarks> Property </remarks>

        private Element<int> _LotPauseTimeoutAlarm
             = new Element<int>();
        /// <summary>
        /// LOT 가 Pause 된 후에 설정 Timeout 시간이 초과되었는데도 Resume 이 안되고 계속 Pause 상태가 유지될 시에 Alarm 을 발생
        /// Unit : Sec
        /// </summary>
        public Element<int> LotPauseTimeoutAlarm
        {
            get { return _LotPauseTimeoutAlarm; }
            set
            {
                if (value != _LotPauseTimeoutAlarm)
                {
                    _LotPauseTimeoutAlarm = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region <remarks> ISystemParameterizable Method </remarks>
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
                retVal = EventCodeEnum.NONE;
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

        }
        #endregion

    }
}
