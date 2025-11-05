using LogModule;
using ProberInterfaces;
using ProberInterfaces.Proxies;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace MultiLauncherProxy
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class MultiExecuterProxy : DuplexClientBase<IMultiExecuter>, IMultiExecuterProxy
    {
        public int Port { get; set; }
        public string IP { get; set; }
        object LockObj = new object();
        public MultiExecuterProxy(int port, InstanceContext callback, string ip = null) :
        base(callback, new ServiceEndpoint(
            ContractDescription.GetContract(typeof(IMultiExecuter)),
            new NetTcpBinding()
            {
                OpenTimeout = new TimeSpan(0, 5, 30),
                SendTimeout = new TimeSpan(0, 0, 5, 30),
                ReceiveTimeout = TimeSpan.MaxValue,
                MaxBufferPoolSize = 524288,
                MaxReceivedMessageSize = 2147483646,
                Security = new NetTcpSecurity() { Mode = SecurityMode.None },
                ReliableSession = new OptionalReliableSession() { InactivityTimeout = new TimeSpan(0,1,0), Enabled = false }
            }, new EndpointAddress($"net.tcp://{ip}:{port}/POS/MultiExecuterService")))

        {
            Port = port;
            IP = ip;

            LoggerManager.Debug($"End point address: {this.Endpoint.Address.Uri.AbsoluteUri}");
        }

        private bool _Init = false;

        public bool Init
        {
            get { return _Init; }
            set { _Init = value; }
        }

        public void StartCell(int cellNum)
        {
            if (IsOpened())
            {
                lock(LockObj)
                {
                    Channel.StartCell(cellNum);
                }
            }
            else
            {
                bool ret = InitService();
                if (ret)
                    Channel.StartCell(cellNum);
            }
        }

        public void StartCellStageList(List<int> stageindex)
        {
            if (IsOpened())
            {
                lock (LockObj)
                {
                    Channel.StartCellStageList(stageindex);
                }
            }
            else
            {
                bool ret = InitService();
                if (ret)
                    Channel.StartCellStageList(stageindex);
            }
        }

        public void ExitCell(int cellNum)
        {
            if (IsOpened())
            {
                lock (LockObj)
                {
                    Channel.ExitCell(cellNum);
                }
            }
        }

        public bool GetCellConnectedInfo(int cellNum)
        {
            if (IsOpened())
            {
                lock (LockObj)
                {
                    return Channel.GetCellConnectedInfo(cellNum);
                }
            }
            else
                return false;
        }

        public bool GetCellAccessible(int cellNum)
        {
            try
            {
                if (IsOpened())
                {
                    lock(LockObj)
                    {
                        return Channel.GetCellAccessible(cellNum);
                    }
                }
                else
                    return false;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                return false;
            }
        }

        //public EnumCheck ConnectCheck()
        //{

        //    try
        //    {
        //        return Channel.ConnectCheck();
        //    }
        //    catch (Exception err)
        //    {
        //        if (Init)
        //        {
        //            LoggerManager.Error(err.ToString());
        //        }
        //    }


        //}
        public bool InitService()
        {
            bool retVal = false;

            try
            {
                Channel.InitService();
                Init = true;
                retVal = true;
            }
            catch (Exception err)
            {
                if (Init)
                {
                    LoggerManager.Error(err.ToString());
                }
            }

            return retVal;
        }
        public bool GetDiskInfo()
        {
            bool retVal = false;

            try
            {
                Channel.GetDiskInfo();
                Init = true;
                retVal = true;
            }
            catch (Exception err)
            {
                if (Init)
                {
                    LoggerManager.Error(err.ToString());
                }
            }

            return retVal;
        }

        public bool IsOpened()
        {
            bool retVal = false;
            if (State == CommunicationState.Opened || State == CommunicationState.Created)
                retVal = true;

            return retVal;
        }

        public void DeInitService()
        {
            try
            {
                Close();

                if (IsOpened())
                {
                    Abort();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                Abort();
            }
        }

        private ProxyManager _proxyManager;
        public ProxyManager ProxyManager
        {
            get { return _proxyManager; }
            set
            {
                if (value != _proxyManager)
                {
                    _proxyManager = value;
                }
            }
        }
        public void DisConnectLauncher()
        {
            try
            {
                ProxyManager?.Disconnect();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            
        }
        public EnumCellProcessState GetCellState(int cellNum)
        {
            EnumCellProcessState retVal = EnumCellProcessState.UNKNOWN;
            try
            {
                if (IsOpened())
                    retVal = Channel.GetCellState(cellNum);
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.ToString());
            }

            return retVal;
        }






    }
}
