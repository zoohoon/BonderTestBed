using System;

namespace RemoteServiceProxy
{
    using LogModule;
    using ProberInterfaces;
    using ProberInterfaces.Param;
    using ProberInterfaces.Proxies;

    using System.ServiceModel;
    using System.ServiceModel.Description;
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]

    public class CoordinateManagerProxy : ClientBase<ICoordinateManager>, ICoordinateManagerProxy
    {
        private object chnLockObj = new object();

        public CoordinateManagerProxy(string ip, int port)
            : base(
            new ServiceEndpoint(ContractDescription.GetContract(typeof(ICoordinateManager)),
            new NetTcpBinding()
            {
                ReceiveTimeout = TimeSpan.MaxValue,
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
            new EndpointAddress($"net.tcp://{ip}:{port}/POS/{ServiceAddress.CoordinateManagerService}")))
        {
            lock (chnLockObj)
            {
                LoggerManager.Debug($"End point address: {this.Endpoint.Address.Uri.AbsoluteUri}");
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
                            LoggerManager.Error($"CoordinateManagerProxy IsServiceAvailable timeout error");
                        }
                        finally
                        {
                            (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"Coordinate Service service error.");
                        retVal = false;
                    }
                }
            }
            catch (Exception)
            {
                LoggerManager.Error($"Coordinate Service service error.");
                retVal = false;
            }

            return retVal;
        }

        public MachineIndex GetCurMachineIndex(WaferCoordinate Pos)
        {
            lock (chnLockObj)
            {
                return Channel.GetCurMachineIndex(Pos);
            }
        }

        public UserIndex GetCurUserIndex(CatCoordinates Pos)
        {
            lock (chnLockObj)
            {
                return Channel.GetCurUserIndex(Pos);
            }
        }

        public void InitService()
        {
            lock (chnLockObj)
            {
                Channel.InitService();
                IsServiceAvailable();
            }
            LoggerManager.Debug($"CoordinateManagerProxy(): Service initilized.");
        }

        public void DeInitService()
        {
            //Dispose
        }

        //public int InitHostServie()
        //{
        //    lock (chnLockObj)
        //    {
        //        if (IsOpened())
        //            return Channel.InitHostService();
        //        else
        //            return -1;
        //    }
        //}

        public bool IsOpened()
        {
            if (State == CommunicationState.Opened | State == CommunicationState.Created)
                return true;
            else
                return false;
        }

        public UserIndex MachineIndexConvertToUserIndex(MachineIndex MI)
        {
            lock (chnLockObj)
            {
                return Channel.MachineIndexConvertToUserIndex(MI);
            }
        }

        public CatCoordinates PmResultConverToUserCoord(PMResult pmresult)
        {
            lock (chnLockObj)
            {
                return Channel.PmResultConverToUserCoord(pmresult);
            }
        }

        public MachineCoordinate RelPosToAbsPos(MachineCoordinate RelPos)
        {
            lock (chnLockObj)
            {
                return Channel.RelPosToAbsPos(RelPos);
            }
        }

        public void StageCoordConvertToChuckCoord()
        {
            lock (chnLockObj)
            {
                Channel.StageCoordConvertToChuckCoord();
            }
        }

        public CatCoordinates StageCoordConvertToUserCoord(EnumProberCam camtype)
        {
            lock (chnLockObj)
            {
                return Channel.StageCoordConvertToUserCoord(camtype);
            }
        }

        public void StopStageCoordConvertToChuckCoord()
        {
            lock (chnLockObj)
            {
                Channel.StopStageCoordConvertToChuckCoord();
            }
        }

        public MachineIndex UserIndexConvertToMachineIndex(UserIndex UI)
        {
            try
            {
                lock (chnLockObj)
                {
                    return Channel.UserIndexConvertToMachineIndex(UI);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
            }
            return new MachineIndex();
        }



        public UserIndex WMIndexConvertWUIndex(long mindexX, long mindexY)
        {
            UserIndex retval = new UserIndex();

            try
            {
                lock (chnLockObj)
                {
                    var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                    try
                    {
                        (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 0, 5);
                        retval = Channel.WMIndexConvertWUIndex(mindexX, mindexY);
                    }
                    catch (Exception)
                    {
                        LoggerManager.Error($"CoordinateManagerProxy WMIndexConvertWUIndex timeout error");
                    }
                    finally
                    {
                        (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public MachineIndex WUIndexConvertWMIndex(long uindexX, long uindexY)
        {
            try
            {
                lock (chnLockObj)
                {
                    return Channel.WUIndexConvertWMIndex(uindexX, uindexY);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
            }
            return new MachineIndex();
        }
    }
}
