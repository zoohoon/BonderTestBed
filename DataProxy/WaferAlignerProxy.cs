using System;
using System.Threading.Tasks;

namespace RemoteServiceProxy
{
    using LogModule;
    using ProberInterfaces;

    using System.ServiceModel;
    using System.ServiceModel.Description;
    using ProberInterfaces.Param;
    using System.Windows;
    using ProberErrorCode;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class WaferAlignerProxy : ClientBase<IWaferAligner>, IWaferAlignerProxy
    {
        public WaferAlignerProxy(string ip, int port)
            : base(
            new ServiceEndpoint(ContractDescription.GetContract(typeof(IWaferAligner)),
            new NetTcpBinding()
            {
                ReceiveTimeout = TimeSpan.MaxValue,
                SendTimeout = new TimeSpan(0, 10, 0),
                OpenTimeout = new TimeSpan(0, 10, 0),
                CloseTimeout = new TimeSpan(0,10,0),
                MaxBufferPoolSize = 524288,
                MaxReceivedMessageSize = 2147483646,
                //ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas()
                //{
                //    MaxDepth = 64,
                //    MaxStringContentLength = 2147483647,
                //    MaxArrayLength = 2147483647,
                //    MaxBytesPerRead = 4096,
                //    MaxNameTableCharCount = 16384
                //},
                Security = new NetTcpSecurity() { Mode = SecurityMode.None },
                ReliableSession = new OptionalReliableSession() { InactivityTimeout = TimeSpan.FromMinutes(1), Enabled = true }
            },
            new EndpointAddress($"net.tcp://{ip}:{port}/POS/{ServiceAddress.WAService}")))
        {
            lock (chnLockObj)
            {
                LoggerManager.Debug($"End point address: {this.Endpoint.Address.Uri.AbsoluteUri}");
            }
        }
        private object chnLockObj = new object();

        public bool IsOpened()
        {
            if (State == CommunicationState.Opened | State == CommunicationState.Created)
                return true;
            else
                return false;
        }
        public WaferCoordinate MachineIndexConvertToDieLeftCorner(long xindex, long yindex)
        {
            lock(chnLockObj)
            {
                if(IsOpened())
                {
                    return Channel.MachineIndexConvertToDieLeftCorner(xindex, yindex);
                }
                else
                {
                    return new WaferCoordinate(0, 0);
                }
            }
        }

        public WaferCoordinate MachineIndexConvertToDieLeftCorenr_NonCalcZ(long xindex, long yindex)
        {
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    return Channel.MachineIndexConvertToDieLeftCorner_NonCalcZ(xindex, yindex);
                }
                else
                {
                    return new WaferCoordinate(0, 0);
                }
            }
        }

        public WaferCoordinate MachineIndexConvertToDieCenter(long xindex, long yindex)
        {
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    return Channel.MachineIndexConvertToDieCenter(xindex, yindex);
                }
                else
                {
                    return new WaferCoordinate(0, 0);
                }
            }
        }

        public WaferCoordinate MachineIndexConvertToProbingCoord(long xindex, long yindex)
        {
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    return Channel.MachineIndexConvertToProbingCoord(xindex, yindex);
                }
                else
                {
                    return new WaferCoordinate(0, 0);
                }
            }
        }

        public void InitService()
        {
        }
        public void DeInitService()
        {
            //Dispose
        }
        public Point GetLeftCornerPositionForWAFCoord(WaferCoordinate position)
        {
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    return Channel.GetLeftCornerPositionForWAFCoord(position);
                }
                else
                {
                    return new Point(0, 0);
                }
            }
        }

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
                            LoggerManager.Error($"WaferAlignerProxy IsServiceAvailable timeout error");
                        }
                        finally
                        {
                            (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"WaferAlign Service service error.");
                        retVal = false;
                    }
                }
            }
            catch (Exception)
            {
                LoggerManager.Error($"WaferAlign Service service error.");
                retVal = false;
            }

            return retVal;
        }

        public EventCodeEnum ClearState()
        {
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    return Channel.ClearState();
                }
                else
                    return EventCodeEnum.UNDEFINED;
            }
        }
        public void SetSetupState()
        {
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    Channel.SetSetupState();
                }
            }
        }
        public void SetIsNewSetup(bool flag)
        {
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                    {
                        Channel.SetIsNewSetup(flag);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetIsModifySetup(bool flag)
        {
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                    {
                        Channel.SetIsModifySetup(flag);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task<bool> CheckPossibleSetup(bool isrecovery = false)
        {
            bool retval = false;

            //lock (chnLockObj)
            //{
                if (IsOpened())
                {
                    retval = await Channel.CheckPossibleSetup(isrecovery);
                }
            //}

            return retval;
        }

        public EventCodeEnum EdgeCheck(ref WaferCoordinate centeroffset, ref double maximum_value_X, ref double maximum_value_Y)
        {
            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    return Channel.EdgeCheck(ref centeroffset, ref maximum_value_X, ref maximum_value_Y);
                }
                else
                {
                    return EventCodeEnum.UNDEFINED;
                }
            }
        }
        public ModuleStateEnum GetModuleState()
        {
            ModuleStateEnum moduleState = ModuleStateEnum.UNDEFINED;
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                    {
                        moduleState = Channel.GetModuleState();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return moduleState;
        }

        public ModuleStateEnum GetPreModuleState()
        {
            ModuleStateEnum moduleState = ModuleStateEnum.UNDEFINED;
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                    {
                        moduleState = Channel.GetPreModuleState();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return moduleState;
        }

        public bool GetIsRecovery()
        {
            bool retVal = false;
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                    {
                        retVal = Channel.GetIsModify();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public (double, double) GetVerifyCenterLimitXYValue()
        {
            (double, double) retVal = (0, 0);

            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                    {
                        retVal = Channel.GetVerifyCenterLimitXYValue();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public void SetVerifyCenterLimitXYValue(double xLimit, double yLimit)
        {
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                    {
                        Channel.SetVerifyCenterLimitXYValue(xLimit, yLimit);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

    }
}
