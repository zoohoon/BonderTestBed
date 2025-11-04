using System;
using System.Collections.Generic;

namespace RemoteServiceProxy
{
    using LogModule;
    using ProberInterfaces;
    using ProberInterfaces.Temperature;

    using System.ServiceModel;
    using System.ServiceModel.Description;
    using ProberErrorCode;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class TempControllerProxy : ClientBase<ITempController>, ITempControllerProxy
    {
        public TempControllerProxy(string ip, int port)
            : base(
            new ServiceEndpoint(ContractDescription.GetContract(typeof(ITempController)),
            new NetTcpBinding()
            {
                ReceiveTimeout = TimeSpan.MaxValue,
                MaxBufferPoolSize = 524288,
                MaxReceivedMessageSize = 2147483646,
                ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas()
                {
                    MaxDepth = 64,
                    MaxStringContentLength = 2147483647,
                    MaxArrayLength = 2147483647,
                    MaxBytesPerRead = 4096,
                    MaxNameTableCharCount = 16384
                },
                Security = new NetTcpSecurity() { Mode = SecurityMode.None },
                ReliableSession = new OptionalReliableSession() { InactivityTimeout = TimeSpan.FromMinutes(1), Enabled = true }
            },
            new EndpointAddress($"net.tcp://{ip}:{port}/POS/{ServiceAddress.TempControlService}")))
        {
            lock (chnLockObj)
            {
                LoggerManager.Debug($"End point address: {this.Endpoint.Address.Uri.AbsoluteUri}");
            }
        }
        private object chnLockObj = new object();

        public bool IsServiceAvailable()
        {
            bool retVal = false;
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                    {
                        var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                        try
                        {
                            (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 0, 15);
                            retVal = Channel.IsServiceAvailable();
                        }
                        catch (Exception)
                        {
                            LoggerManager.Error($"TempControllerProxy IsServiceAvailable timeout error");
                        }
                        finally
                        {
                            (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"TempController Service service error.");
                        retVal = false;
                    }
                }
            }
            catch (Exception)
            {
                LoggerManager.Error($"TempController Service service error.");
                retVal = false;
            }

            return retVal;
        }
        public void AddHeaterOffset(double reftemp, double measuredtemp)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.AddHeaterOffset(reftemp, measuredtemp);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ClearHeaterOffset()
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.ClearHeaterOffset();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void Dispose()
        {
            lock (chnLockObj)
            {

            }
        }

        public Dictionary<double, double> GetHeaterOffset()
        {
            Dictionary<double, double> retval = null;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retval = Channel.GetHeaterOffsets();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public int GetHeaterOffsetCount()
        {
            int retval = 0;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retval = Channel.GetHeaterOffsetCount();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public double GetTemperatureOffset(double value)
        {
            double retval = 0.0;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retval = Channel.GetTemperatureOffset(value);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void InitService()
        {
            IsServiceAvailable();
        }
        public void DeInitService()
        {
            //Dispose
        }

        public void SetSV(TemperatureChangeSource source, double changeSetTemp, bool willYouSaveSetValue = true, bool forcedSetValue = false)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.SetSV(source, changeSetTemp, willYouSaveSetValue, forcedSetValue);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetSVOnlyTC(double setTemp)
        {
            return;
        }

        public void SetAmbientTemp()
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.SetAmbientTemp();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetTemperature()
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.SetTemperature();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public double GetTemperature()
        {
            double retval = 0.0;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retval = Channel.GetTemperature();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }



        public void SetLoggingInterval(long seconds)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.SetLoggingInterval(seconds);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public EnumTemperatureState GetTempControllerState()
        {
            EnumTemperatureState retval = EnumTemperatureState.IDLE;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retval = Channel.GetTempControllerState();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public double GetDeviaitionValue()
        {
            double retval = 0;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retval = Channel.GetDeviaitionValue();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public double GetEmergencyAbortTempTolereance()
        {
            double retval = 0;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retval = Channel.GetEmergencyAbortTempTolereance();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public TempPauseTypeEnum GetTempPauseType()
        {
            TempPauseTypeEnum retval = TempPauseTypeEnum.NONE;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retval = Channel.GetTempPauseType();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void SetDeviaitionValue(double deviation, bool emergencyparam)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.SetDeviaitionValue(deviation, emergencyparam);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetTempPauseType(TempPauseTypeEnum pausetype)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.SetTempPauseType(pausetype);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public bool CheckSetDeviationParamLimit(double deviation, bool emergencyparam)
        {
            bool retval = false;
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retval = Channel.CheckSetDeviationParamLimit(deviation, emergencyparam);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }
        public double GetCurDewPointValue()
        {
            double retval = 0;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retval = Channel.GetCurDewPointValue();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public double GetMV()
        {
            double retval = 0;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retval = Channel.GetMV();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public double GetDewPointTolerance()
        {
            double retval = 0;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retval = Channel.GetDewPointTolerance();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public double GetSetTemp()
        {
            double retval = 0;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retval = Channel.GetSetTemp();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool GetHeaterOutPutState()
        {
            bool retval = false;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retval = Channel.GetHeaterOutPutState();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public void SaveOffsetParameter()
        {
            try
            {
                lock (chnLockObj)
                {
                    Channel.SaveOffsetParameter();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetTempMonitorInfo(TempMonitoringInfo info)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.SetTempMonitorInfo(info);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public TempMonitoringInfo GetTempMonitorInfo()
        {
            TempMonitoringInfo retInfo = null;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retInfo = Channel.GetTempMonitorInfo();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retInfo;
        }

        public byte[] GetParamByte()
        {
            byte[] param = null;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        param = Channel.GetParamByte();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return param;
        }
        public EventCodeEnum SetParamByte(byte[] devparam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retVal = Channel.SetParamByte(devparam);
                    }
                }
                else
                {
                    retVal = EventCodeEnum.PROXY_STATE_NOT_OPEN_ERROR;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void SetEndTempEmergencyErrorCommand()
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.SetEndTempEmergencyErrorCommand();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private bool IsOpened()
        {
            bool retVal = false;

            try
            {
                if (State == CommunicationState.Opened | State == CommunicationState.Created)
                {
                    retVal = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public bool GetIsOccurTimeOutError()
        {
            bool retVal = false;

            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                    {
                        retVal = Channel.GetIsOccurTimeOutError();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = false;
            }

            return retVal;
        }
        public void ClearTimeOutError()
        {
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                    {
                        Channel.ClearTimeOutError();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public double GetMonitoringMVTimeInSec()
        {
            double retVal = 0.0;

            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                    {
                        retVal = Channel.GetMonitoringMVTimeInSec();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = 0.0;
            }

            return retVal;
        }
        public void SetMonitoringMVTimeInSec(double value)
        {
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                    {
                        Channel.SetMonitoringMVTimeInSec(value);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public double GetCCActivatableTemp()
        {
            double retVal = 30.0;

            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                    {
                        retVal = Channel.GetCCActivatableTemp();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = 30.0;
            }

            return retVal;
        }

        public bool IsCurTempWithinSetTempRange()
        {
            bool retVal = false;
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                    {
                        retVal = Channel.IsCurTempWithinSetTempRange();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = false;
            }

            return retVal;
        }


        public bool IsCurTempUpperThanSetTemp(double setTemp, double margin)
        {
            bool retVal = false;
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                    {
                        retVal = Channel.IsCurTempUpperThanSetTemp(setTemp, margin);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = false;
            }

            return retVal;
        }

        public double GetDevSetTemp()
        {
            double retVal = -1;

            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                    {
                        retVal = Channel.GetDevSetTemp();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = -1;
            }

            return retVal;
        }

        public EventCodeEnum SetDevSetTemp(double setTemp)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                    {
                        retVal = Channel.SetDevSetTemp(setTemp);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public bool GetApplySVChangesBasedOnDeviceValue()
        {
            bool retVal = false;
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                    {
                        retVal = Channel.GetApplySVChangesBasedOnDeviceValue();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void SetActivatedState(bool forced = false)
        {
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                    {
                        Channel.SetActivatedState(forced);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public TempEventInfo GetPreviousTempInfoInHistory()
        {
            TempEventInfo retVal = null;
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                    {
                        retVal = Channel.GetPreviousTempInfoInHistory();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public TempEventInfo GetPreviousSourceTempInfoInHistory()
        {
            TempEventInfo retVal = null;
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                    {
                        retVal = Channel.GetPreviousSourceTempInfoInHistory();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public TempEventInfo GetCurrentTempInfoInHistory()
        {
            TempEventInfo retVal = null;
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                    {
                        retVal = Channel.GetCurrentTempInfoInHistory();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

    }
}
