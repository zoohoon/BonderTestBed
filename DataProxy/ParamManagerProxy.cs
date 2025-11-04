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
    public class ParamManagerProxy : ClientBase<IParamManager>, IParamManagerProxy
    {
        public ParamManagerProxy(string ip, int port) : base(
            new ServiceEndpoint(ContractDescription.GetContract(typeof(IParamManager)),
            new NetTcpBinding()
            {
                ReceiveTimeout = TimeSpan.MaxValue,
                MaxBufferPoolSize = 0,
                MaxReceivedMessageSize = 2147483646,
                //ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas()
                //{
                //    MaxDepth = 128,
                //    MaxStringContentLength = 2147483647,
                //    MaxArrayLength = 2147483647,
                //    MaxBytesPerRead = 524288,
                //    MaxNameTableCharCount = 16384
                //},
                Security = new NetTcpSecurity() { Mode = SecurityMode.None },
                ReliableSession = new OptionalReliableSession() { InactivityTimeout = TimeSpan.FromMinutes(2), Enabled = true }
            },
        new EndpointAddress($"net.tcp://{ip}:{port}/POS/{ServiceAddress.ParamManagerService}")))
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
                            LoggerManager.Error($"ParamManagerProxy IsServiceAvailable timeout error");
                        }
                        finally
                        {
                            (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"ParamManager Service service error.");
                        retVal = false;
                    }
                }
            }
            catch (Exception)
            {
                LoggerManager.Error($"ParamManager Service service error.");
                retVal = false;
            }

            return retVal;
        }
        public void InitService()
        {
            Channel.InitService();
        }

        public void DeInitService()
        {
            //Dispose
        }
        public IElement GetElement(int elementID)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        var element = Channel.GetElement(elementID);
                        return element;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return null;
        }

        public List<IElement> GetElementList(List<IElement> emlList, ref byte[] bulkElem)
        {
            List<IElement> retVal = null;
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retVal = Channel.GetElementList(emlList, ref bulkElem);

                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public string GetPropertyPathFromVID(long vid)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        return Channel.GetPropertyPathFromVID(vid);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return null;
        }

        public long GetElementIDFormVID(long vid)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        return Channel.GetElementIDFormVID(vid);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return -1;
        }

        public IElement GetAssociateElement(string associateID)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        var element = Channel.GetAssociateElement(associateID);
                        return element;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return null;
        }

        public List<IElement> GetDevElementList()
        {
            List<IElement> retval = null;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                        try
                        {
                            (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 5, 0);
                            retval = Channel.GetDevElementList();
                        }
                        catch (Exception)
                        {
                            LoggerManager.Error($"ParamManagerProxy GetDevElementList timeout error.");
                        }
                        finally
                        {
                            (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                        }

                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public List<IElement> GetSysElementList()
        {
            List<IElement> retval = null;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                        try
                        {
                            (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 5, 0);
                            retval = Channel.GetSysElementList();
                        }
                        catch (Exception)
                        {
                            LoggerManager.Error($"ParamManagerProxy GetSysElementList timeout error.");
                        }
                        finally
                        {
                            (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public List<IElement> GetCommonElementList()
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        var element = Channel.GetCommonElementList();
                        return element;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return null;
        }

        /// <summary>
        /// 사용되고 있지 않은 코드
        /// </summary>
        /// <param name="elem"></param>
        /// <param name="isNeedValidation"></param>
        /// <param name="source_classname"></param>
        public void SaveElement(IElement elem, bool isNeedValidation = false)//, string source_classname = null)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.SaveElement(elem, isNeedValidation);//, source_classname);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetElement(string propPath, Object setval)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.SetElement(propPath, setval);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SaveCategory(string categoryID)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.SaveCategory(categoryID);
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SyncDBTableByCSV()
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.SyncDBTableByCSV();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void LoadElementInfoFromDB()
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.LoadElementInfoFromDB();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void LoadElementInfoFromDB(ParamType paramType, IElement elem, string dbPropertyPath)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.LoadElementInfoFromDB(paramType, elem, dbPropertyPath);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void LoadSysElementInfoFromDB()
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.LoadSysElementInfoFromDB();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void LoadDevElementInfoFromDB()
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.LoadDevElementInfoFromDB();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void LoadComElementInfoFromDB()
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.LoadComElementInfoFromDB();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void RegistElementToDB()
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.RegistElementToDB();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ExportDBtoCSV()
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.ExportDBtoCSV();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void DeInitModule()
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.DeInitModule();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public byte[] GetDevParamElementsBulk(string paramName)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        return Channel.GetDevParamElementsBulk(paramName);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return null;
        }

        public byte[] GetDevElementsBulk()
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        return Channel.GetDevElementsBulk();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return null;
        }

        public byte[] GetSysElementsBulk()
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        return Channel.GetSysElementsBulk();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return null;
        }

        public EventCodeEnum CheckOriginSetValueAvailable(string propertypath, object val)//,  string source_classname = null)
        {
            //string erromsg = "";
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retVal = Channel.CheckOriginSetValueAvailable(propertypath, val);//, source_classname);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //errorlog = erromsg;
            }
            return retVal;
        }

        public EventCodeEnum CheckSetValueAvailable(string propertypath, object val)
        {
            //string erromsg = "";
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retVal = Channel.CheckSetValueAvailable(propertypath, val);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //errorlog = erromsg;
            }
            return retVal;
        }


        public EventCodeEnum SetOriginValue(string propertypath, Object val, bool isNeedValidation = false, bool isEqualsValue = true, object valueChangedParam = null)
        {            
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retVal = Channel.SetOriginValue(propertypath, val, isNeedValidation, isEqualsValue, valueChangedParam);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //errorlog = erromsg;
            }
            return retVal;
        }

        public EventCodeEnum SetValue(string propertypath, Object val, bool isNeedValidation = false)
        {            
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retVal = Channel.SetValue(propertypath, val, isNeedValidation);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //errorlog = erromsg;
            }
            return retVal;
        }

        public void SaveElementPack(byte[] pack, bool isNeedValidation = false)//, string source_classname = null)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.SaveElementPack(pack, isNeedValidation);//, source_classname);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool IsAvailable()
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        return Channel.IsAvailable();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return false;
        }

        public void SetChangedDeviceParam(bool flag)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.SetChangedDeviceParam(flag);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetChangedSystemParam(bool flag)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.SetChangedSystemParam(flag);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool GetChangedDeviceParam()
        {
            bool retval = false;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retval = Channel.GetChangedDeviceParam();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool IsOpened()
        {
            try
            {
                if (State == CommunicationState.Opened | State == CommunicationState.Created)
                    return true;
                else
                    return false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }
        public bool GetChangedSystemParam()
        {
            bool retval = false;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retval = Channel.GetChangedSystemParam();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum VerifyLotVIDsCheckBeforeLot()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retval = Channel.VerifyLotVIDsCheckBeforeLot();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public ObservableCollection<int> GetVerifyLotVIDs()
        {
            ObservableCollection<int> retVal = new ObservableCollection<int>();
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retVal = Channel.GetVerifyLotVIDs();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void SetVerifyLotVIDs(ObservableCollection<int> vids)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.SetVerifyLotVIDs(vids);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public void UpdateLowerLimit(string propertyPath, string setValue)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.UpdateLowerLimit(propertyPath, setValue);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public void UpdateUpperLimit(string propertyPath, string setValue)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.UpdateUpperLimit(propertyPath, setValue);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void UpdateVerifyParam()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public bool GetVerifyParameterBeforeStartLotEnable()
        {
            bool retVal = false;
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retVal = Channel.GetVerifyParameterBeforeStartLotEnable();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public List<VerifyParamInfo> GetVerifyParamInfo()
        {
            List<VerifyParamInfo> retVal = null;
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        retVal = Channel.GetVerifyParamInfo();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void SetVerifyParameterBeforeStartLotEnable(bool flag)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.SetVerifyParameterBeforeStartLotEnable(flag);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetVerifyParamInfo(List<VerifyParamInfo> infos)
        {
            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
                    {
                        Channel.SetVerifyParamInfo(infos);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (IsOpened())
                {
                    lock (chnLockObj)
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
    }
}
