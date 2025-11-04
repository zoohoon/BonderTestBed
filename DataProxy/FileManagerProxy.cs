using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Proxies;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace RemoteServiceProxy
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class FileManagerProxy : ClientBase<IFileManager>, IFileManagerProxy
    {
        public FileManagerProxy(string ip, int port)
            : base(
            new ServiceEndpoint(ContractDescription.GetContract(typeof(IFileManager)),
            new NetTcpBinding()
            {
                ReceiveTimeout = TimeSpan.MaxValue,
                MaxBufferPoolSize = 2147483647,
                MaxReceivedMessageSize = 2147483647,
                Security = new NetTcpSecurity() { Mode = SecurityMode.None },
                ReliableSession = new OptionalReliableSession() { InactivityTimeout = TimeSpan.FromMinutes(1), Enabled = true }
            },
            new EndpointAddress($"net.tcp://{ip}:{port}/POS/{ServiceAddress.FileManagerService}")))
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
                            LoggerManager.Error($"FileManagerProxy IsServiceAvailable timeout error");
                        }
                        finally
                        {
                            (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"FileManager Service service error.");
                        retVal = false;
                    }
                }
            }
            catch (Exception)
            {
                LoggerManager.Error($"FileManager Service service error.");
                retVal = false;
            }

            return retVal;
        }


        public void InitService()
        {
            Channel.IsServiceAvailable();
        }

        public void DeInitService()
        {
            //Dispose
        }

        public void Dispose()
        {
            lock (chnLockObj)
            {

            }
        }

        public bool IsOpened()
        {
            bool retVal = false;

            if (State == CommunicationState.Opened | State == CommunicationState.Created)
                retVal = true;
            return retVal;
        }

        public byte[] GetDeviceByFileName(string filename)
        {
            byte[] retval = null;

            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        retval = Channel.GetDeviceByFileName(filename);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retval;
            }
        }
        public bool SetDeviceByFileName(byte[] device, string devicename)
        {
            bool retval = false;

            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        retval = Channel.SetDeviceByFileName(device, devicename);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retval;
            }
        }

        public ObservableCollection<string> GetDeviceNamelist()
        {
            ObservableCollection<string> devicenamelist = null;

            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        devicenamelist = Channel.GetDeviceNamelist();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return devicenamelist;
            }
        }

        public ObservableCollection<SimpleDeviceInfo> GetDevicelistTest()
        {
            ObservableCollection<SimpleDeviceInfo> retval = null;

            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                    {
                        retval = Channel.GetDevicelistTest();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public ObservableCollection<DeviceInfo> GetDevicelist()
        {
            ObservableCollection<DeviceInfo> retval = null;

            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                    {
                        retval = Channel.GetDevicelist();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public byte[] GetDevicebytelist()
        {
            byte[] retval = null;

            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        retval = Channel.GetDevicebytelist();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retval;
            }
        }
        public byte[] GetFileManagerParam()
        {
            byte[] retval = null;

            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        retval = Channel.GetFileManagerParam();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retval;
            }
        }

        public string GetSystemParamFullPath(string parameterPath, string parameterName)
        {
            string retval = null;

            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        retval = Channel.GetSystemParamFullPath(parameterPath, parameterName);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retval;
            }
        }

        public string GetDeviceParamFullPath(string parameterPath = "", string parameterName = "", bool isContainDeviceName = true)
        {
            string retval = null;

            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        retval = Channel.GetDeviceParamFullPath(parameterPath, parameterName, isContainDeviceName);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retval;
            }
        }

        public string GetDeviceName()
        {
            string retval = null;

            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        retval = Channel.GetDeviceName();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retval;
            }
        }

        public EventCodeEnum ChangeDevice(string DevName)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        retval = Channel.ChangeDevice(DevName);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retval;
            }
        }

        public EventCodeEnum DeleteDevice(string DevName)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        retval = Channel.DeleteDevice(DevName);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retval;
            }
        }

        public byte[] GetCompressedFile(DateTime startdate, DateTime enddate, List<EnumLoggerType> logtypes, List<EnumProberModule> imagetypes, 
            bool includeGEM = false, bool includeClip = false, bool includeLoadedDevice = false, bool inlcudeSystemparam = false, bool inlcudebackupinfo = false, bool inlcudeSysteminfo = false)
        {
            byte[] retval = null;

            lock (chnLockObj)
            {
                if (IsOpened())
                {
                    try
                    {
                        var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                        try
                        {
                            (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 10, 0);
                            retval = Channel.GetCompressedFile(startdate, enddate, logtypes, imagetypes, includeGEM, includeClip, includeLoadedDevice, inlcudeSystemparam, inlcudebackupinfo, inlcudeSysteminfo);
                        }
                        catch (Exception)
                        {
                            LoggerManager.Error($"FileManagerProxy GetCompressedFile timeout error");
                        }
                        finally
                        {
                            (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                return retval;
            }
        }
    }
}
