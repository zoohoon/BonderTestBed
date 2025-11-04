namespace EnvModule
{
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Enum;
    using ProberInterfaces.Temperature.Chiller;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [Serializable]
    public class TemperatureChecker : IEnvConditionChecker
    {
        private Element<string> _CheckerClassName = new Element<string>() { Value = "TemperatureChecker" };
        public Element<string> CheckerClassName
        {
            get { return _CheckerClassName; }
            set { _CheckerClassName = value; }
        }

        private long _DefaultErrorTimeOut { get; } = 60000;

        private long _ErrorOccurredTimeoutSec;
        public long ErrorOccurredTimeoutSec
        {
            get { return _ErrorOccurredTimeoutSec; }
            set { _ErrorOccurredTimeoutSec = value; }
        }

        private bool _NotifyEnable = true;

        public bool NotifyEnable
        {
            get { return _NotifyEnable; }
            set { _NotifyEnable = value; }
        }

        private Nullable<DateTime> _ErrorOccurredTime;
        [JsonIgnore]
        public Nullable<DateTime> ErrorOccurredTime
        {
            get { return _ErrorOccurredTime; }
            set { _ErrorOccurredTime = value; }
        }

        public EventCodeEnum Init()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var tempLimitRunTime = this.TempController().GetLimitRunTimeSeconds();

                if(ErrorOccurredTimeoutSec < tempLimitRunTime)
                {
                    ErrorOccurredTimeoutSec = tempLimitRunTime;
                }
                if(ErrorOccurredTimeoutSec == 0)
                {
                    ErrorOccurredTimeoutSec = _DefaultErrorTimeOut;
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum Checking(out string errorMsg)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            errorMsg = "";

            try
            {
                if(this.TempController().ModuleState.GetState() != ModuleStateEnum.ERROR)
                {
                    if (this.TempController().TempInfo.TargetTemp.Value == this.TempController().TempInfo.SetTemp.Value)
                    {
                        if (this.TempController().ModuleState.GetState() == ModuleStateEnum.DONE || this.TempController().GetTempControllerState() == EnumTemperatureState.Monitoring)
                        {
                            if (this.TempController().IsCurTempWithinSetTempRange() == false)
                            {
                                retVal = EventCodeEnum.ENV_TEMPARATURE_OUT_OF_RANGE;
                            }
                            else
                            {
                                retVal = EventCodeEnum.NONE;
                            }
                        }
                        else
                        {
                            retVal = EventCodeEnum.ENV_TEMPERATURE_WAIT_DONE;
                        }
                    }
                    else
                    {
                        retVal = EventCodeEnum.ENV_TEMPERAUTRE_NOT_MATCHED;
                    }
                }
                else
                {
                    retVal = EventCodeEnum.ENV_TEMPERATURE_STATE_ERROR;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class ChillerChecker : IEnvConditionChecker
    {
        private Element<string> _CheckerClassName = new Element<string>() { Value = "ChillerChecker" };
        public Element<string> CheckerClassName
        {
            get { return _CheckerClassName; }
            set { _CheckerClassName = value; }
        }

        private long _DefaultErrorTimeOut { get; } = 5;

        private long _ErrorOccurredTimeoutSec;
        public long ErrorOccurredTimeoutSec
        {
            get { return _ErrorOccurredTimeoutSec; }
            set { _ErrorOccurredTimeoutSec = value; }
        }

        private bool _NotifyEnable = true;

        public bool NotifyEnable
        {
            get { return _NotifyEnable; }
            set { _NotifyEnable = value; }
        }

        private Nullable<DateTime> _ErrorOccurredTime;
        [JsonIgnore]
        public Nullable<DateTime> ErrorOccurredTime
        {
            get { return _ErrorOccurredTime; }
            set { _ErrorOccurredTime = value; }
        }

        public EventCodeEnum Init()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if(ErrorOccurredTimeoutSec == 0)
                {
                    ErrorOccurredTimeoutSec = _DefaultErrorTimeOut;
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum Checking(out string errorMsg)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            errorMsg = "";

            try
            {
                // TODO : 칠러를 사용하고 TempController의 ModuleState의 값이 DONE이라는 것과 Monitoring이라는 것의 의미는?
                if (this.TempController().IsUsingChillerState() &&
                    (this.TempController().ModuleState.GetState() == ModuleStateEnum.DONE || this.TempController().GetTempControllerState() == EnumTemperatureState.Monitoring))
                {
                    IChillerModule chillerModule = this.EnvControlManager().GetChillerModule();

                    if (chillerModule.GetCommState() == EnumCommunicationState.CONNECTED)
                    {
                        // TODO : CoolantActivate의 값이 false라는 것의 의미는?
                        if (chillerModule.ChillerInfo?.CoolantActivate == false)
                        {
                            retVal = EventCodeEnum.CHILLER_ACTIVATE_ERROR;
                        }
                        else
                        {
                            // TODO : 이 시점에서 GetValveState(EnumValveType.IN)의 값이 false라는 것의 의미는?

                            if (this.EnvControlManager().GetValveState(EnumValveType.IN) == false)
                            {
                                retVal = EventCodeEnum.ENV_VALVE_STATE_ERROR;
                            }
                        }
                    }
                    else
                    {
                        //retVal = EventCodeEnum.CHILLER_NOT_CONNECTED; => TempController.SequenceRun() 에서 확인 하고 있음.
                    }
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    //[Serializable]
    //public class DewPointChecker : IEnvConditionChecker
    //{
    //    private Element<string> _CheckerClassName = new Element<string>() { Value = "DewPointChecker" };
    //    public Element<string> CheckerClassName
    //    {
    //        get { return _CheckerClassName; }
    //        set { _CheckerClassName = value; }
    //    }

    //    private long _DefaultErrorTimeOut { get; } = 60000;

    //    private long _ErrorOccurredTimeoutSec;
    //    public long ErrorOccurredTimeoutSec
    //    {
    //        get { return _ErrorOccurredTimeoutSec; }
    //        set { _ErrorOccurredTimeoutSec = value; }
    //    }

    //    private bool _NotifyEnable = false;

    //    public bool NotifyEnable
    //    {
    //        get { return _NotifyEnable; }
    //        set { _NotifyEnable = value; }
    //    }

    //    private Nullable<DateTime> _ErrorOccurredTime;
    //    [JsonIgnore]
    //    public Nullable<DateTime> ErrorOccurredTime
    //    {
    //        get { return _ErrorOccurredTime; }
    //        set { _ErrorOccurredTime = value; }
    //    }

    //    public EventCodeEnum Init()
    //    {
    //        EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
    //        try
    //        {
    //            var dpWaitTimeout = this.EnvControlManager().GetDewPointModule()?.WaitTimeout??0;
    //            if (ErrorOccurredTimeoutSec < dpWaitTimeout)
    //            {
    //                ErrorOccurredTimeoutSec = dpWaitTimeout;
    //            }
    //            if (ErrorOccurredTimeoutSec == 0)
    //            {
    //                ErrorOccurredTimeoutSec = _DefaultErrorTimeOut;
    //            }
    //            retVal = EventCodeEnum.NONE;
               
    //            retVal = EventCodeEnum.NONE;
    //        }
    //        catch (Exception err)
    //        {
    //            LoggerManager.Exception(err);
    //        }
    //        return retVal;
    //    }

    //    public EventCodeEnum Checking(out string errorMsg)
    //    {
    //        EventCodeEnum retVal = EventCodeEnum.NONE;
    //        errorMsg = "";
    //        try
    //        {
    //            if(this.TempController().IsUsingChillerState())
    //            {
    //                var chillerModule = this.EnvControlManager().GetChillerModule();
    //                if(chillerModule != null)
    //                {
    //                    if(chillerModule.GetCommState() == EnumCommunicationState.CONNECTED)
    //                    {
    //                        if (chillerModule.ChillerInfo != null && chillerModule.ChillerInfo.ChillerInternalTemp < 0)
    //                        {
    //                            if (this.TempController().GetCurDewPointValue() > chillerModule.ChillerInfo.ChillerInternalTemp)
    //                            {
    //                                if(this.EnvControlManager().GetValveState(EnumValveType.IN))
    //                                {
    //                                    retVal = EventCodeEnum.DEW_POINT_HIGH_ERR;
    //                                }
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //        catch (Exception err)
    //        {
    //            LoggerManager.Exception(err);
    //        }
    //        return retVal;
    //    }
    //}

    //public class TopPurgeChecker : IEnvConditionChecker
    //{
    //    private Element<string> _CheckerClassName = new Element<string>() { Value = "TopPurgeChecker" };
    //    public Element<string> CheckerClassName
    //    {
    //        get { return _CheckerClassName; }
    //        set { _CheckerClassName = value; }
    //    }

    //    private long _DefaultErrorTimeOut { get; }

    //    private long _ErrorOccurredTimeoutSec;
    //    public long ErrorOccurredTimeoutSec
    //    {
    //        get { return _ErrorOccurredTimeoutSec; }
    //        set { _ErrorOccurredTimeoutSec = value; }
    //    }

    //    private bool _NotifyEnable = true;

    //    public bool NotifyEnable
    //    {
    //        get { return _NotifyEnable; }
    //        set { _NotifyEnable = value; }
    //    }

    //    private Nullable<DateTime> _ErrorOccurredTime;
    //    [JsonIgnore]
    //    public Nullable<DateTime> ErrorOccurredTime
    //    {
    //        get { return _ErrorOccurredTime; }
    //        set { _ErrorOccurredTime = value; }
    //    }

    //    public EventCodeEnum Init()
    //    {
    //        EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
    //        try
    //        {
    //            retVal = EventCodeEnum.NONE;
    //        }
    //        catch (Exception err)
    //        {
    //            LoggerManager.Exception(err);
    //        }
    //        return retVal;
    //    }

    //    public EventCodeEnum Checking(out string errorMsg)
    //    {
    //        EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
    //        errorMsg = "";
    //        try
    //        {
    //            retVal = EventCodeEnum.NONE;
    //        }
    //        catch (Exception err)
    //        {
    //            LoggerManager.Exception(err);
    //        }
    //        return retVal;
    //    }
    //}

    //public class DryAirChecker : IEnvConditionChecker
    //{
    //    private Element<string> _CheckerClassName = new Element<string>() { Value = "DryAirChecker" };
    //    public Element<string> CheckerClassName
    //    {
    //        get { return _CheckerClassName; }
    //        set { _CheckerClassName = value; }
    //    }

    //    private long _DefaultErrorTimeOut { get; }

    //    private long _ErrorOccurredTimeoutSec;
    //    public long ErrorOccurredTimeoutSec
    //    {
    //        get { return _ErrorOccurredTimeoutSec; }
    //        set { _ErrorOccurredTimeoutSec = value; }
    //    }

    //    private bool _NotifyEnable = true;

    //    public bool NotifyEnable
    //    {
    //        get { return _NotifyEnable; }
    //        set { _NotifyEnable = value; }
    //    }

    //    private Nullable<DateTime> _ErrorOccurredTime;
    //    [JsonIgnore]
    //    public Nullable<DateTime> ErrorOccurredTime
    //    {
    //        get { return _ErrorOccurredTime; }
    //        set { _ErrorOccurredTime = value; }
    //    }

    //    public EventCodeEnum Init()
    //    {
    //        EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
    //        try
    //        {
    //            retVal = EventCodeEnum.NONE;
    //        }
    //        catch (Exception err)
    //        {
    //            LoggerManager.Exception(err);
    //        }
    //        return retVal;
    //    }

    //    public EventCodeEnum Checking(out string errorMsg)
    //    {
    //        EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
    //        errorMsg = "";
    //        try
    //        {

    //        }
    //        catch (Exception err)
    //        {
    //            LoggerManager.Exception(err);
    //        }
    //        return retVal;
    //    }
    //}

}
