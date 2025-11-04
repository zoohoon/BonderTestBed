using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Proxies;
using System;
using System.ServiceModel;
using System.ServiceModel.Description;


namespace RemoteServiceProxy
{

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class SoakingModuleProxy : ClientBase<ISoakingModule>, ISoakingModuleProxy
    {
        public SoakingModuleProxy(string ip, int port)
            : base(
            new ServiceEndpoint(ContractDescription.GetContract(typeof(ISoakingModule)),
            new NetTcpBinding()
            {
                ReceiveTimeout = TimeSpan.MaxValue,
                MaxBufferPoolSize = 524288,
                MaxReceivedMessageSize = 50000000,
                Security = new NetTcpSecurity() { Mode = SecurityMode.None },
                ReliableSession = new OptionalReliableSession() { InactivityTimeout = TimeSpan.FromMinutes(1), Enabled = true }
            },
            new EndpointAddress($"net.tcp://{ip}:{port}/POS/{ServiceAddress.SoakingModuleService}")))
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
                            LoggerManager.Error($"SoakingModuleProxy IsServiceAvailable timeout error");
                        }
                        finally
                        {
                            (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"Soaking Service service error.");
                        retVal = false;
                    }
                }
            }
            catch (Exception)
            {
                LoggerManager.Error($"Soaking Service service error.");
                retVal = false;
            }

            return retVal;
        }
        public void InitService()
        {
            IsServiceAvailable();
        }
        public void DeInitService()
        {
            //Dispose
        }
        public bool IsOpened()
        {
            bool retVal = false;

            if (State == CommunicationState.Opened | State == CommunicationState.Created)
                retVal = true;
            return retVal;
        }
        private bool _SoakingCancelFlag;

        public bool SoakingCancelFlag
        {
            get { return _SoakingCancelFlag; }
            set
            {
                lock (chnLockObj)
                {
                    if (value != _SoakingCancelFlag)
                    {
                        _SoakingCancelFlag = value;
                    }

                }
            }
        }

        public bool IsUsePolishWafer()
        {
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    return Channel.IsUsePolishWafer();
                }
            }

            return false;
        }

        public int ChuckAwayToleranceLimitDef => 200;

        public int GetChuckAwayToleranceLimitX()
        {
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    return Channel.GetChuckAwayToleranceLimitX();
                }
            }

            return ChuckAwayToleranceLimitDef;
        }

        public int GetChuckAwayToleranceLimitY()
        {
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    return Channel.GetChuckAwayToleranceLimitY();
                }
            }

            return ChuckAwayToleranceLimitDef;
        }

        public int GetChuckAwayToleranceLimitZ()
        {
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    return Channel.GetChuckAwayToleranceLimitZ();
                }
            }

            return ChuckAwayToleranceLimitDef;
        }

        public byte[] GetStatusSoakingConfigParam()
        {
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    return Channel.GetStatusSoakingConfigParam();
                }
            }

            return null;
        }

        public bool SetStatusSoakingConfigParam(byte[] param, bool save_to_file = true)
        {
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    return Channel.SetStatusSoakingConfigParam(param, save_to_file);
                }
            }

            return false;
        }

        public SoakingStateEnum GetStatusSoakingState()
        {
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    return Channel.GetStatusSoakingState();
                }
            }

            return SoakingStateEnum.UNDEFINED;
        }
        public bool GetShowStatusSoakingSettingPageToggleValue()
        {
            bool retVal = false;
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    retVal = Channel.GetShowStatusSoakingSettingPageToggleValue();
                }
            }

            return retVal;
        }
        public void Check_N_ClearStatusSoaking()
        {
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    Channel.Check_N_ClearStatusSoaking();
                }
            }            
        }
        public void SetShowStatusSoakingSettingPageToggleValue(bool ToggleValue)
        {
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    Channel.SetShowStatusSoakingSettingPageToggleValue(ToggleValue);
                }
            }
        }

        public int GetStatusSoakingTime()
        {
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    return Channel.GetStatusSoakingTime();
                }
            }

            return -1;
        }
        public void SetCancleFlag(bool value,int chukindex)
        {
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    Channel.SetCancleFlag(value, chukindex);
                }
            }
        }

        public string GetSoakingTitle()
        {
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    return Channel.GetSoakingTitle();
                }
            }
            return null;
        }

        public string GetSoakingMessage()
        {
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                   return Channel.GetSoakingMessage();
                }
            }
            return null;
        }

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                    {
                        retval = Channel.SaveDevParameter();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                    {
                        retval = Channel.SaveSysParameter();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum ClearState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                    {
                        retval = Channel.ClearState();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum StartManualSoakingProc()
        {
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    return Channel.StartManualSoakingProc();
                }
            }

            return EventCodeEnum.UNDEFINED;
        }

        public EventCodeEnum StopManualSoakingProc()
        {
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    return Channel.StopManualSoakingProc();
                }
            }

            return EventCodeEnum.UNDEFINED;
        }

        public void TraceLastSoakingStateInfo(bool bStart)
        {
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    Channel.TraceLastSoakingStateInfo(bStart);
                }
            }
        }

        public (EventCodeEnum, DateTime/*Soaking start*/, SoakingStateEnum/*Soaking Status*/, SoakingStateEnum/*soakingSubState*/, ModuleStateEnum) GetCurrentSoakingInfo()
        {
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    return Channel.GetCurrentSoakingInfo();
                }
            }

            return (EventCodeEnum.NONE, default, SoakingStateEnum.UNDEFINED, SoakingStateEnum.UNDEFINED, ModuleStateEnum.UNDEFINED);
        }

        public bool GetCurrentStatusSoakingUsingFlag()
        {
            bool retVal = false;
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    retVal = Channel.GetCurrentStatusSoakingUsingFlag();
                }
            }

            return retVal;
        }

        public bool GetBeforeStatusSoakingUsingFlag()
        {
            bool retVal = false;
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    retVal = Channel.GetBeforeStatusSoakingUsingFlag();
                }
            }

            return retVal;            
        }

        public void SetBeforeStatusSoakingUsingFlag(bool UseStatusSoakingFlag)
        {            
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    Channel.SetBeforeStatusSoakingUsingFlag(UseStatusSoakingFlag);
                }
            }
        }

        public void ForceChange_PrepareStatus()
        {
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    Channel.ForceChange_PrepareStatus();
                }
            }
        }

        public bool IsEnablePolishWaferSoakingOnCurState()
        {
            bool retVal = false;
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    retVal = Channel.IsEnablePolishWaferSoakingOnCurState();
                }
            }

            return retVal;
        }

        public bool IsCurTempWithinSetSoakingTempRange()
        {
            bool retVal = false;
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    retVal = Channel.IsCurTempWithinSetSoakingTempRange();
                }
            }

            return retVal;
        }
        public void Set_PrepareStatusSoak_after_DeviceChange(bool PreheatSoak_after_DeviceChange)
        {
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    Channel.Set_PrepareStatusSoak_after_DeviceChange(PreheatSoak_after_DeviceChange);
                }
            }
        }
        public bool Get_PrepareStatusSoak_after_DeviceChange()
        {
            bool retVal = false;
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    retVal = Channel.Get_PrepareStatusSoak_after_DeviceChange();
                }
            }
            return retVal;
        }
    }
}
