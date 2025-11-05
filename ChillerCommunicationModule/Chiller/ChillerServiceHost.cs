using System;

namespace ControlModules
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Enum;
    using ProberInterfaces.Temperature;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Threading.Tasks;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ChillerServiceHost : IChillerService
    {
        public IChillerManager Manager { get; set; }

        private ServiceHost CommanderServiceHost = null;
        private object lockobj;

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                lockobj = new object();
                try
                {
                    ServiceMetadataBehavior serviceMetadataBehavior = null;
                    ServiceDebugBehavior debugBehavior = null;
                    string localURI = $"net.tcp://localhost:{ServicePort.ChillerServicePort}/POS/chillerpipe";
                    Task task = new Task(() =>
                    {
                        var netTcpBinding = new NetTcpBinding()
                        {
                            MaxBufferPoolSize = 2147483647,
                            MaxBufferSize = 2147483647,
                            MaxReceivedMessageSize = 2147483647,
                            SendTimeout = new TimeSpan(0, 5, 0),
                            ReceiveTimeout = TimeSpan.MaxValue,
                            OpenTimeout = new TimeSpan(0, 10, 0),
                            CloseTimeout = new TimeSpan(0, 10, 0),
                            ReliableSession = new OptionalReliableSession() { InactivityTimeout = TimeSpan.FromMinutes(1), Enabled = true }
                        };

                        netTcpBinding.Security.Mode = SecurityMode.None;
                        CommanderServiceHost = new ServiceHost(this);
                        CommanderServiceHost.AddServiceEndpoint(typeof(IChillerService), netTcpBinding, localURI);

                        debugBehavior = CommanderServiceHost.Description.Behaviors.Find<ServiceDebugBehavior>();
                        if (debugBehavior != null)
                        {
                            debugBehavior.IncludeExceptionDetailInFaults = true;
                        }

                        serviceMetadataBehavior = CommanderServiceHost.Description.Behaviors.Find<ServiceMetadataBehavior>();
                        if (serviceMetadataBehavior == null)
                            serviceMetadataBehavior = new ServiceMetadataBehavior();

                        serviceMetadataBehavior.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                        CommanderServiceHost.Description.Behaviors.Add(serviceMetadataBehavior);

                        CommanderServiceHost.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName,
                            MetadataExchangeBindings.CreateMexTcpBinding(),
                            $"{localURI}/mex"
                            );

                        CommanderServiceHost.Open();
                    });
                    task.Start();
                    task.Wait();
                    retVal = EventCodeEnum.NONE;
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        public EventCodeEnum Connect(string address, int port, int stageindex = -1)
        {
            //if (Manager.GetChillerCommModule(stageindex).CommunicationState == EnumCommunicationState.CONNECTING)
            //    return EventCodeEnum.NONE;
            //Comm 에서 Chiller 가 Connect 되었는지 확인
            return EventCodeEnum.NONE;
            //return Manager?.Connect(address,port) ?? EventCodeEnum.UNDEFINED;
        }

        public EventCodeEnum Disconnect(int stageindex = -1)
        {
            return EventCodeEnum.NONE;
        }

        public EnumCommunicationState GetCommState(int stageindex = -1)
        {
            return Manager?.GetCommState(stageindex) ?? EnumCommunicationState.UNAVAILABLE;
        }

        public byte[] GetChillerParam(int stageindex = -1)
        {
            return Manager?.GetChillerParam(stageindex);
        }
        public int GetCurrentPower(int stageindex = -1)
        {
            return Manager?.GetCurrentPower(stageindex) ?? -1;
        }

        public int GetErrorReport(int stageindex = -1)
        {
            return Manager?.GetChillerModule(stageindex)?.ChillerInfo?.ErrorReport ?? -1;
        }

        public double GetExtMoveVal(int stageindex = -1)
        {
            return Manager?.GetExtMoveVal(stageindex) ?? -1;
        }

        public double GetInternalTempValue(int stageindex = -1)
        {
            return Manager?.GetChillerModule(stageindex)?.ChillerInfo?.ChillerInternalTemp ?? -1;
        }

        public double GetLowerAlramInternalLimit(int stageindex = -1)
        {
            return Manager?.GetLowerAlramInternalLimit(stageindex) ?? -1;
        }

        public double GetLowerAlramProcessLimit(int stageindex = -1)
        {
            return Manager?.GetLowerAlramProcessLimit(stageindex) ?? -1;
        }

        public double GetMaxSetTemp(int stageindex = -1)
        {
            return Manager?.GetMaxSetTemp(stageindex) ?? -1;
        }

        public double GetMinSetTemp(int stageindex = -1)
        {
            return Manager?.GetMinSetTemp(stageindex) ?? -1;
        }

        public double GetProcessTempVal(int stageindex = -1)
        {
            return Manager?.GetProcessTempVal(stageindex) ?? -1;
        }

        public (bool, bool) GetProcTempActValSetMode(int stageindex = -1)
        {
            return Manager?.GetProcTempActValSetMode(stageindex) ?? (false, false);
        }

        public int GetPumpPressureVal(int stageindex = -1)
        {
            return Manager?.GetPumpPressureVal(stageindex) ?? -1;
        }

        public int GetPumpSpeed(int stageindex = -1)
        {
            return Manager?.GetPumpSpeed(stageindex) ?? -1;
        }

        public double GetReturnTempVal(int stageindex = -1)
        {
            return Manager?.GetReturnTempVal(stageindex) ?? -1;
        }

        public int GetSerialNumber(int stageindex = -1)
        {
            return Manager?.GetSerialNumber(stageindex) ?? -1;
        }

        public int GetSerialNumHigh(int stageindex = -1)
        {
            return Manager?.GetSerialNumHigh(stageindex) ?? -1;
        }

        public int GetSerialNumLow(int stageindex = -1)
        {
            return Manager?.GetSerialNumLow(stageindex) ?? -1;
        }

        public int GetSetTempPumpSpeed(int stageindex = -1)
        {
            return Manager?.GetSetTempPumpSpeed(stageindex) ?? -1;
        }

        public double GetSetTempValue(int stageindex = -1)
        {
            return Manager?.GetChillerModule(stageindex)?.ChillerInfo?.SetTemp ?? -1;
        }

        public int GetStatusOfThermostat(int stageindex = -1)
        {
            return Manager?.GetStatusOfThermostat(stageindex) ?? -1;
        }

        public double GetUpperAlramInternalLimit(int stageindex = -1)
        {
            return Manager?.GetUpperAlramInternalLimit(stageindex) ?? -1;
        }

        public double GetUpperAlramProcessLimit(int stageindex = -1)
        {
            return Manager?.GetUpperAlramProcessLimit(stageindex) ?? -1;
        }

        public int GetWarningMessage(int stageindex = -1)
        {
            int warningCode = 0;
            try
            {
                var chillerModule = Manager?.GetChillerModule(stageindex);
                var warningReport = chillerModule?.ChillerInfo?.WarningReport;

                if (warningReport != null && warningReport.Count != 0)
                {
                    warningCode = warningReport.Last();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                warningCode = 0;
            }
            return warningCode;
        }

        public bool IsAutoPID(int stageindex = -1)
        {
            return Manager?.IsAutoPID(stageindex) ?? false;
        }

        public bool IsCirculationActive(int stageindex = -1)
        {
            return Manager?.IsCirculationActive(stageindex) ?? false;
        }

        public (bool, bool) IsOperatingLock(int stageindex = -1)
        {
            return Manager?.IsOperatingLock(stageindex) ?? (false, false);
        }

        public bool IsServiceAvailable()
        {
            return Manager?.IsServiceAvailable() ?? false;
        }

        public bool IsTempControlActive(int stageindex = -1)
        {
            return Manager?.GetChillerModule(stageindex)?.ChillerInfo?.CoolantActivate ?? false;
        }

        public bool IsTempControlProcessMode(int stageindex = -1)
        {
            return Manager?.IsTempControlProcessMode(stageindex) ?? false;
        }

        public void SetSetTempPumpSpeed(int iValue, int stageindex = -1)
        {
            Manager?.SetSetTempPumpSpeed(iValue, stageindex);
        }
        public EventCodeEnum CheckCanUseChiller(double sendVal, int stageindex = -1, bool offvalve = false, bool forcedSetValue = false)
        {
            return Manager?.CheckCanUseChiller(sendVal, stageindex, offvalve) ?? EventCodeEnum.CHILLER_CHECK_CAN_USE_CHILLER_ERROR;
        }
        public EventCodeEnum SetTargetTemp(double sendVal, TempValueType sendTempValueType, int stageindex = -1, bool forcedSetValue = false)
        {
            lock (lockobj)
            {
                return Manager?.SetTargetTemp(sendVal, sendTempValueType, stageindex) ?? EventCodeEnum.CHILLER_SET_TARGET_TEMP_ERROR;
            }
        }

        public void SetTempActiveMode(bool bValue, int stageindex = -1)
        {
            Manager?.SetTempActiveMode(bValue, stageindex);
        }

        public void InitService()
        {
            //minskim// 추후 chiller proxy callback을 사용해야 할 경우 이 위치에서 callback list를 등록 해야함
            return;
        }

        public void SetCircuationActive(bool bValue, int stageindex = -1, int chillerindex = -1)
        {
            return;
        }

        public bool IsCirculationActive(int stageindex = -1, int chillerindex = -1)
        {
            return false;
        }

        public bool GetChillerAbortActiveState(int stageindex = -1)
        {
            return Manager?.GetChillerAbortActiveState(stageindex) ?? true;
        }

        public void Set_OperaionLockValue(int chillerindex = -1, bool lockValue = false)
        {
            return;
        }
    }
}
