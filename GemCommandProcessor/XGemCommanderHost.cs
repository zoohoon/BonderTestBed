using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.GEM;
using SecsGemServiceInterface;
using SecsGemServiceProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Timers;
using System.Threading.Tasks;
using XGEMWrapper;

namespace XGemCommandProcessor
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class XGemCommanderHost : ISecsGemServiceHost, IFactoryModule
    {
        public IGemProcessorCore Commander { get; set; }
        private int StageIndex { get; set; }

        private object callbackLockObj { get; set; }

        private Timer timer;
        #region ==> 2. Commander Service Host & Callback Collection
        // Cell과 통신하는 Host입니다.
        private ServiceHost CommanderServiceHost = null;
        // Cell Callback 객체입니다.
        private Dictionary<long, ISecsGemServiceCallback> DicCommanderServiceCallback
            = new Dictionary<long, ISecsGemServiceCallback>();
        #endregion

        #region //..Ceid Parameter
        private string SecsGemDefineReportFilePath { get; } = @"C:\Logs\Backup\SecsGemDefineReport.Json";
        private SecsGemDefineReport _XGemDefineReport;
        public SecsGemDefineReport XGemDefineReport
        {
            get { return _XGemDefineReport; }
            set
            {
                if (value != _XGemDefineReport)
                {
                    _XGemDefineReport = value;
                }
            }
        }
        public EventCodeEnum LoadSecsGemDefineReport()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                object tmpParam = null;
                if (XGemDefineReport == null)
                {
                    XGemDefineReport = new SecsGemDefineReport();
                }
                Extensions_IParam.LoadDataFromJson(ref tmpParam, typeof(SecsGemDefineReport), SecsGemDefineReportFilePath);
                if(tmpParam != null)
                {
                    XGemDefineReport = (SecsGemDefineReport)tmpParam;

                    LoggerManager.Debug("Gem DefineReport");
                    foreach (var ceid in XGemDefineReport.CEIDs)
                    {
                        LoggerManager.Debug($"-------------------------");
                        LoggerManager.Debug($"CEID : {ceid.Ceid}");
                        foreach (var rptid in ceid.RPTIDs)
                        {
                            LoggerManager.Debug($"RPTID : {rptid}");
                        }
                        LoggerManager.Debug($"-------------------------");
                    }
                }

                LoggerManager.Debug("XGemCommanderHost LoadSecsGemDefineReport().");
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SaveSecsGemDefineReport()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if(XGemDefineReport != null)
                {
                    Extensions_IParam.SaveDataToJson(XGemDefineReport, SecsGemDefineReportFilePath);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        #endregion

        #region <!-- Stage Data Buffer -->
        /// <summary>
        /// long : vid , object : data value
        /// </summary>
        private List<Dictionary<long, VIDInfomation>> _ExecutorDataDic
             = new List<Dictionary<long, VIDInfomation>>();

        public List<Dictionary<long, VIDInfomation>> ExecutorDataDic
        {
            get { return _ExecutorDataDic; }
            set { _ExecutorDataDic = value; }
        }

        private List<Dictionary<long, VIDInfomation>> _ExecutorECVDataDic
             = new List<Dictionary<long, VIDInfomation>>();

        public List<Dictionary<long, VIDInfomation>> ExecutorECVDataDic
        {
            get { return _ExecutorECVDataDic; }
            set { _ExecutorECVDataDic = value; }
        }


        #endregion

        public XGemCommanderHost()
        {
            try
            {
                LoadSecsGemDefineReport();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool StartCommanderService()
        {
            bool retVal = false;

            try
            {
                callbackLockObj = new object();
                if (OpenGemCommanderHost("localhost", 7513) == EventCodeEnum.NONE)
                    retVal = true;

                timer = new Timer();
                timer.Interval = 1000;
                timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
                timer.Start();

                InitClientData();

                if(ExecutorDataDic.Count != SystemModuleCount.ModuleCnt.StageCount)
                {
                    //ExecutorDataDic.Clear();
                    //ExecutorECVDataDic.Clear();
                    for (int index = 0; index < SystemModuleCount.ModuleCnt.StageCount; index++)
                    {
                        ExecutorDataDic.Add(new Dictionary<long, VIDInfomation>());
                        ExecutorECVDataDic.Add(new Dictionary<long, VIDInfomation>());
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        private void InitClientData()
        {
            try
            {
                lock (Commander.lockObj)
                {
                    if (ExecutorDataDic != null)
                    {
                        for (int index = 0; index < ExecutorDataDic.Count; index++)
                        {
                            var dicData = ExecutorDataDic[index];
                            if (dicData.Count != 0)
                            {
                                var keys = dicData.Keys.ToList<long>();
                                long[] pnVid = new long[0];
                                string[] psValue = new string[0];

                                foreach (var vidInfo in dicData)
                                {
                                    if (vidInfo.Value.VidType == EnumVidType.SVID || vidInfo.Value.VidType == EnumVidType.ECID)
                                    {
                                        Array.Resize(ref pnVid, pnVid.Length + 1);
                                        Array.Resize(ref psValue, psValue.Length + 1);

                                        pnVid[pnVid.Length - 1] = vidInfo.Key;
                                        psValue[psValue.Length - 1] = vidInfo.Value.Value.ToString();
                                        LoggerManager.Debug($"InitClientData() Stage : {index + 1}, VID : {vidInfo.Key}, Value : {vidInfo.Value.Value.ToString()}");

                                        if (pnVid.Length > 0)
                                        {
                                            long retVal = (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMSetVariable(pnVid.Length, pnVid, psValue) ?? -1;
                                            LoggerManager.Debug($"InitClientData() Stage : {index + 1} update result is : {retVal}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum CloseCommanderService()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if(CommanderServiceHost.State != CommunicationState.Faulted && CommanderServiceHost.State != CommunicationState.Closed)
                {
                    CommanderServiceHost.Close();
                    LoggerManager.Debug("[SECS/GEM] CommandServiceHost is closed");
                }
            }
            catch (Exception err)
            {
                CommanderServiceHost.Abort();
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void SetSecsGemDefineReport(SecsGemDefineReport gemDefineReport)
        {
            try
            {
                XGemDefineReport = gemDefineReport;
                SaveSecsGemDefineReport();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public List<long> GetVidsFormCeid(long ceid)
        {
            List<long> vids = new List<long>();
            try
            {
                var ceids = XGemDefineReport.CEIDs.Find(param => param.Ceid == ceid);
                if (ceids != null)
                {
                    foreach (var rptid in ceids.RPTIDs)
                    {
                        var rptids = XGemDefineReport.RPTIDs.Find(param => param.Rptid == rptid);
                        if (rptids != null)
                        {
                            foreach (var vid in rptids.VIDs)
                            {
                                vids.Add(vid);
                            }
                        }
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return vids;
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                timer.Enabled = false;
                List<long> removeCollection = null;
                var callbacks = DicCommanderServiceCallback?.ToArray();
                if (callbacks != null)
                {
                    foreach (var callback in callbacks)
                    {
                        try
                        {
                            lock (callbackLockObj)
                            {
                                callback.Value.Are_You_There();
                            }
                        }
                        catch (Exception err)
                        {
                            System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                            LoggerManager.Debug($"Cell #{callback.Key} Gem communication fail.");

                            int retryCount = 3;
                            bool successFalg = false;
                            for (int count = 0; count < retryCount; count++)
                            {
                                try
                                {
                                    System.Threading.Thread.Sleep(1);
                                    successFalg = callback.Value.Are_You_There();
                                    if (successFalg)
                                    {
                                        break;
                                    }
                                }
                                catch (Exception errr)
                                {
                                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, errr);
                                    successFalg = false;
                                    LoggerManager.Debug($"Cell #{callback.Key} Gem communication retry fail. Retry count : {count + 1}");

                                }
                            }

                            if (successFalg == false)
                            {
                                if (removeCollection == null)
                                {
                                    removeCollection = new List<long>();
                                }
                                if (!removeCollection.Contains(callback.Key))
                                {
                                    removeCollection.Add(callback.Key);
                                }
                            }
                        }
                    }

                    if (removeCollection != null)
                    {
                        foreach (var callback in removeCollection)
                        {
                            if (DicCommanderServiceCallback.ContainsKey(callback))
                            {
                                DicCommanderServiceCallback.Remove(callback);
                                Commander.GEMDisconnectCallBack(callback);
                                this.MetroDialogManager().ShowMessageDialog("Error Message",
                                       $"CELL#{callback} Gem communication is disconnected. Please check the communication connection status.", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                timer.Enabled = true;
            }
        }

        private EventCodeEnum OpenGemCommanderHost(string ip, int port)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            ServiceMetadataBehavior serviceMetadataBehavior = null;
            ServiceDebugBehavior debugBehavior = null;
            string localURI = $"net.tcp://{ip}:{port}/secsgempipe";

            try
            {
                if (CommanderServiceHost != null)
                {
                    if (CommanderServiceHost.State == CommunicationState.Opened)
                    {
                        return EventCodeEnum.NONE;
                    }
                }

                Task task = new Task(() =>
                {
                    var netTcpBinding = new NetTcpBinding()
                    {
                        MaxBufferPoolSize = 2147483647,
                        MaxBufferSize = 2147483647,
                        MaxReceivedMessageSize = 2147483647,
                        SendTimeout = TimeSpan.MaxValue,
                        ReceiveTimeout = TimeSpan.MaxValue
                    };

                    netTcpBinding.Security.Mode = SecurityMode.None;
                    CommanderServiceHost = new ServiceHost(this);
                    CommanderServiceHost.AddServiceEndpoint(typeof(ISecsGemService), netTcpBinding, localURI);

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

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw err;
            }

            return retval;
        }


        public bool IsOpened()
        {
            return true;
        }

        #region <remarks> Request To GEM Dongle </remarks>

        public long Init_SECSGEM(string Config)
        {
            return 0;
        }

        public long Close_SECSGEM(int proberId = 0)
        {
            bool isExistKey = DicCommanderServiceCallback?.ContainsKey(proberId) ?? false;
            var isSuccessRemoveObj = isExistKey ? DicCommanderServiceCallback.Remove(proberId) : false;
            return isSuccessRemoveObj ? 0 : -1;
        }
        public long LoadMessageRecevieModule(string dllpath, string receivername)
        {
            return 0;
        }
        public long Start()
        {
            return 0;
        }

        public long Stop()
        {
            return 0;
        }

        public void ServerConnect(int proberId = 0)
        {
            try
            {
                var commanderServiceCallbackObj = OperationContext.Current.GetCallbackChannel<ISecsGemServiceCallback>();

                if (commanderServiceCallbackObj != null)
                {
                    if (DicCommanderServiceCallback.ContainsKey(proberId))
                        DicCommanderServiceCallback.Remove(proberId);
                    DicCommanderServiceCallback.Add(proberId, commanderServiceCallbackObj);

                    (commanderServiceCallbackObj as ICommunicationObject).Faulted += CallbackFaultedEventHandler;
                    (commanderServiceCallbackObj as ICommunicationObject).Closed += CallbackFaultedEventHandler;
                }
                StageIndex = proberId;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void CallbackFaultedEventHandler(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public long GEMReqOffline()
        {
            long retVal = -1;
            try
            {
                //retVal = (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMReqOffline();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public long GEMReqLocal()
        {
            long retVal = -1;
            try
            {
                //retVal = (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMReqLocal();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public long GEMReqRemote()
        {
            long retVal = -1;
            try
            {
                //retVal = (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMReqRemote();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public long GEMSetEstablish(long bState)
        {
            long retVal = -1;
            try
            {
                //retVal = (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMSetEstablish(bState);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public long MakeObject(ref long pnObjectID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.MakeObject(ref pnObjectID) ?? -1;
        }

        public long SetListItem(long nObjectID, long nItemCount)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.SetListItem(nObjectID, nItemCount) ?? -1;
        }

        public long SetBinaryItem(long nObjectID, byte nValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.SetBinaryItem(nObjectID, nValue) ?? -1;
        }

        public long SetBoolItem(long nObjectID, bool nValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.SetBoolItem(nObjectID, nValue) ?? -1;
        }

        public long SetBoolItems(long nObjectID, bool[] pnValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.SetBoolItems(nObjectID, pnValue) ?? -1;
        }

        public long SetUint1Item(long nObjectID, byte nValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.SetUint1Item(nObjectID, nValue) ?? -1;
        }

        public long SetUint1Items(long nObjectID, byte[] pnValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.SetUint1Items(nObjectID, pnValue) ?? -1;
        }

        public long SetUint2Item(long nObjectID, ushort nValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.SetUint2Item(nObjectID, nValue) ?? -1;
        }

        public long SetUint2Items(long nObjectID, ushort[] pnValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.SetUint2Items(nObjectID, pnValue) ?? -1;
        }

        public long SetUint4Item(long nObjectID, uint nValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.SetUint4Item(nObjectID, nValue) ?? -1;
        }

        public long SetUint4Items(long nObjectID, uint[] pnValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.SetUint4Items(nObjectID, pnValue) ?? -1;
        }

        public long SetUint8Item(long nObjectID, ulong nValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.SetUint8Item(nObjectID, nValue) ?? -1;
        }

        public long SetUint8Items(long nObjectID, ulong[] pnValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.SetUint8Items(nObjectID, pnValue) ?? -1;
        }

        public long SetInt1Item(long nObjectID, sbyte nValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.SetInt1Item(nObjectID, nValue) ?? -1;
        }

        public long SetInt1Items(long nObjectID, sbyte[] pnValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.SetInt1Items(nObjectID, pnValue) ?? -1;
        }

        public long SetInt2Item(long nObjectID, short nValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.SetInt2Item(nObjectID, nValue) ?? -1;
        }

        public long SetInt2Items(long nObjectID, short[] pnValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.SetInt2Items(nObjectID, pnValue) ?? -1;
        }

        public long SetInt4Item(long nObjectID, int nValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.SetInt4Item(nObjectID, nValue) ?? -1;
        }

        public long SetInt4Items(long nObjectID, int[] pnValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.SetInt4Items(nObjectID, pnValue) ?? -1;
        }

        public long SetInt8Item(long nObjectID, long nValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.SetInt8Item(nObjectID, nValue) ?? -1;
        }

        public long SetInt8Items(long nObjectID, long[] pnValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.SetInt8Items(nObjectID, pnValue) ?? -1;
        }

        public long SetFloat4Item(long nObjectID, float nValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.SetFloat4Item(nObjectID, nValue) ?? -1;
        }

        public long SetFloat4Items(long nObjectID, float[] pnValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.SetFloat4Items(nObjectID, pnValue) ?? -1;
        }

        public long SetFloat8Item(long nObjectID, double nValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.SetFloat8Item(nObjectID, nValue) ?? -1;
        }

        public long SetFloat8Items(long nObjectID, double[] pnValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.SetFloat8Items(nObjectID, pnValue) ?? -1;
        }

        public long SetStringItem(long nObjectID, string pszValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.SetStringItem(nObjectID, pszValue) ?? -1;
        }

        public long GetListItem(long nObjectID, ref long pnItemCount)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetListItem(nObjectID, ref pnItemCount) ?? -1;
        }

        public long GetBinaryItem(long nObjectID, ref byte[] pnValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetBinaryItem(nObjectID, ref pnValue) ?? -1;
        }

        public long GetBoolItem(long nObjectID, ref bool[] pnValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetBoolItem(nObjectID, ref pnValue) ?? -1;
        }

        public long GetUint1Item(long nObjectID, ref byte[] pnValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetUint1Item(nObjectID, ref pnValue) ?? -1;
        }

        public long GetUint2Item(long nObjectID, ref ushort[] pnValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetUint2Item(nObjectID, ref pnValue) ?? -1;
        }

        public long GetUint4Item(long nObjectID, ref uint[] pnValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetUint4Item(nObjectID, ref pnValue) ?? -1;
        }

        public long GetUint8Item(long nObjectID, ref ulong[] pnValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetUint8Item(nObjectID, ref pnValue) ?? -1;
        }

        public long GetInt1Item(long nObjectID, ref sbyte[] pnValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetInt1Item(nObjectID, ref pnValue) ?? -1;
        }

        public long GetInt2Item(long nObjectID, ref short[] pnValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetInt2Item(nObjectID, ref pnValue) ?? -1;
        }

        public long GetInt4Item(long nObjectID, ref int[] pnValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetInt4Item(nObjectID, ref pnValue) ?? -1;
        }

        public long GetInt8Item(long nObjectID, ref long[] pnValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetInt8Item(nObjectID, ref pnValue) ?? -1;
        }

        public long GetFloat4Item(long nObjectID, ref float[] pnValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetFloat4Item(nObjectID, ref pnValue) ?? -1;
        }

        public long GetFloat8Item(long nObjectID, ref double[] pnValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetFloat8Item(nObjectID, ref pnValue) ?? -1;
        }

        public long GetStringItem(long nObjectID, ref string psValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetStringItem(nObjectID, ref psValue) ?? -1;
        }


        public long SendSECSMessage(long nObjectID, long nStream, long nFunction, long nSysbyte)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.SendSECSMessage(nObjectID, nStream, nFunction, nSysbyte) ?? -1;
        }

        public long GEMSetParam(string sParamName, string sParamValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMSetParam(sParamName, sParamValue) ?? -1;
        }

        public long GEMGetParam(string sParamName, ref string psParamValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMGetParam(sParamName, ref psParamValue) ?? -1;
        }

        public long GEMEQInitialized(long nInitType)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMEQInitialized(nInitType) ?? -1;
        }

        public long GEMReqGetDateTime()
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMReqGetDateTime() ?? -1;
        }

        public long GEMRspGetDateTime(long nMsgId, string sSystemTime)
        {
            //return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMRspGetDateTime(nMsgId, sSystemTime) ?? -1;
            long retVal = 1;
            try
            {
                var callbacks = DicCommanderServiceCallback?.ToArray();
                if (callbacks != null)
                {
                    foreach (var callback in callbacks)
                    {
                        callback.Value.OnGEMReqDateTime(nMsgId, sSystemTime);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        public long GEMRspDateTime(long nMsgId, long nResult)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMRspDateTime(nMsgId, nResult) ?? -1;
        }

    

        public long GEMSetAlarm(long nID, long nState, int cellindex = 0)
        {
            //직접 호출 이었는데 Commander를 통해서 호출하도록 변경.
            //return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMSetAlarm(nID, nState) ?? -1;
            return Commander.Proc_SetAlarm(nID, nState, cellindex);
        }

        public long ClearAlarmOnly(int cellIndex = 0)
        {
            long retVal = -1;
           
            EventCodeEnum ret = Commander.Proc_ClearAlarmOnly(cellIndex);
            if (ret == EventCodeEnum.NONE)
            {
                retVal = 0;
            }
            else
            {
                //retVal = ret;
            }
            return retVal;
        }

        public long GEMRspRemoteCommand(long nMsgId, string sCmd, long nHCAck, long nCount, long[] pnResult)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMRspRemoteCommand(nMsgId, sCmd, nHCAck, nCount, pnResult) ?? -1;
        }

        public long GEMRspRemoteCommand2(long nMsgId, string sCmd, long nHCAck, long nCount, string[] psCpName, long[] pnCpAck)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMRspRemoteCommand2(nMsgId, sCmd, nHCAck, nCount, psCpName, pnCpAck) ?? -1;
        }

        public long GEMRspChangeECV(long nMsgId, long nResult)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMRspChangeECV(nMsgId, nResult) ?? -1;
        }

        public long GEMSetECVChanged(long nCount, long[] pnEcIds, string[] psEcVals, int stageNum = -1)
        {
            try
            {
                for (int index = 0; index < pnEcIds.Length; index++)
                {
                    //object ret = null;
                    if (ExecutorDataDic.Count >= stageNum - 1)
                    {
                        if (!ExecutorDataDic[stageNum - 1].ContainsKey(pnEcIds[index]))
                        {
                            //Dictionary 에 없는 경우
                            ExecutorDataDic[stageNum - 1].Add(pnEcIds[index], new VIDInfomation(psEcVals[index], EnumVidType.ECID, EnumVidObjectType.NONE));
                        }
                        else
                        {
                            ExecutorDataDic[stageNum - 1][pnEcIds[index]].Value = psEcVals[index];
                        }
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            //return retVal;

            //지금은 svid 성격의 값들만 가지고 있어서 바로 업데이트 해도 문제되지 않지만, 혹시나  dvid 성격의 값들이 있을수 있음....
            //TODO: 만약에 그런것들이 필요하다면 dic에만 넣어두고 나중에 이벤트시에만 업데이트 하도록 변경해야할것.
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMSetECVChanged(nCount, pnEcIds, psEcVals) ?? -1;
        }

        public long GEMReqAllECInfo()
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMReqAllECInfo() ?? -1;
        }

        public long GEMSetPPChanged(long nMode, string sPpid, long nLength, string pbBody)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMSetPPChanged(nMode, sPpid, nLength, pbBody) ?? -1;
        }

        public long GEMSetPPFmtChanged(long nMode, string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMSetPPFmtChanged(nMode, sPpid, sMdln, sSoftRev, nCount, psCCode, pnParamCount, psParamNames) ?? -1;
        }

        public long GEMReqPPLoadInquire(string sPpid, long nLength)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMReqPPLoadInquire(sPpid, nLength) ?? -1;
        }

        public long GEMRspPPLoadInquire(long nMsgId, string sPpid, long nResult)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMRspPPLoadInquire(nMsgId, sPpid, nResult) ?? -1;
        }

        public long GEMReqPPSend(string sPpid, string pbBody)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMReqPPSend(sPpid, pbBody) ?? -1;
        }

        public long GEMRspPPSend(long nMsgId, string sPpid, long nResult)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMRspPPSend(nMsgId, sPpid, nResult) ?? -1;
        }

        public long GEMReqPP(string sPpid)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMReqPP(sPpid) ?? -1;
        }

        public long GEMRspPP(long nMsgId, string sPpid, string pbBody)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMRspPP(nMsgId, sPpid, pbBody) ?? -1;
        }

        public long GEMRspPPDelete(long nMsgId, long nCount, string[] psPpids, long nResult)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMRspPPDelete(nMsgId, nCount, psPpids, nResult) ?? -1;
        }

        public long GEMRspPPList(long nMsgId, long nCount, string[] psPpids)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMRspPPList(nMsgId, nCount, psPpids) ?? -1;
        }

        public long GEMReqPPFmtSend(string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMReqPPFmtSend(sPpid, sMdln, sSoftRev, nCount, psCCode, pnParamCount, psParamNames) ?? -1;
        }

        public long GEMReqPPFmtSend2(string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames, string[] psParamValues)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMReqPPFmtSend2(sPpid, sMdln, sSoftRev, nCount, psCCode, pnParamCount, psParamNames, psParamValues) ?? -1;
        }

        public long GEMRspPPFmtSend(long nMsgId, string sPpid, long nResult)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMRspPPFmtSend(nMsgId, sPpid, nResult) ?? -1;
        }

        public long GEMReqPPFmt(string sPpid)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMReqPPFmt(sPpid) ?? -1;
        }

        public long GEMRspPPFmt(long nMsgId, string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMRspPPFmt(nMsgId, sPpid, sMdln, sSoftRev, nCount, psCCode, pnParamCount, psParamNames) ?? -1;
        }

        public long GEMRspPPFmt2(long nMsgId, string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames, string[] psParamValues)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMRspPPFmt2(nMsgId, sPpid, sMdln, sSoftRev, nCount, psCCode, pnParamCount, psParamNames, psParamValues) ?? -1;
        }

        public long GEMReqPPFmtVerification(string sPpid, long nCount, long[] pnAck, string[] psSeqNumber, string[] psError)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMReqPPFmtVerification(sPpid, nCount, pnAck, psSeqNumber, psError) ?? -1;
        }

        public long GEMSetTerminalMessage(long nTid, string sMsg)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMSetTerminalMessage(nTid, sMsg) ?? -1;
        }

        public long GEMSetVariable(long nCount, long[] pnVid, string[] psValue, int stageNum = -1, bool immediatelyUpdate = false)
        {
            //v22_merge// 코드 검토 필요, EnumVidObjectType.NONE 이게 맞나?
            long retVal = -1;
            try
            {
                lock (Commander.lockObj)
                {
                    EnumVidType vidType = EnumVidType.NONE;
                    if(immediatelyUpdate)
                    {
                        vidType = EnumVidType.SVID;
                    }
                    else
                    {
                        vidType = EnumVidType.DVID;
                    }
                    for (int index = 0; index < pnVid.Length; index++)
                    {
                        if (ExecutorDataDic.Count >= stageNum - 1)
                        {
                            if (!ExecutorDataDic[stageNum - 1].ContainsKey(pnVid[index]))
                            {
                                //Dictionary 에 없는 경우
                                ExecutorDataDic[stageNum - 1].Add(pnVid[index], new VIDInfomation( psValue[index], vidType, EnumVidObjectType.NONE));
                                retVal = 0;
                            }
                            else
                            {
                                //Dictionary 의 Value 만 변경.
                                ExecutorDataDic[stageNum - 1][pnVid[index]].Value = psValue[index];
                                retVal = 0;
                            }
                        }

                    }
                    if (immediatelyUpdate)
                    {
                        return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMSetVariable(nCount, pnVid, psValue) ?? -1;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public long GEMGetVariable(long nCount, ref long[] pnVid, ref string[] psValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMGetVariable(nCount, ref pnVid, ref psValue) ?? -1;
        }

        public (VidUpdateTypeEnum vidOwner, int stgNum) GetVidOwner(long ecid)
        {
            (VidUpdateTypeEnum owner, int stgNum) ret = (VidUpdateTypeEnum.BOTH, -1);
            try
            {
                var gemvidInfo = this.GEMModule().FindVidInfo(this.GEMModule().DicECID.DicProberGemID.Value, ecid);//로더쪽 파라미터 


                //셀쪽에 파라미터가 있을 수도 있으니까 뒤져 봐야함.
                for (int stageNum = 1; stageNum <= SystemModuleCount.ModuleCnt.StageCount; stageNum++)
                {
                    try
                    {
                        if (ExecutorDataDic[stageNum - 1].ContainsKey(ecid))
                        {
                            ret.stgNum = stageNum;
                            break;
                        }

                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }

                }

                if (ret.stgNum > 0)
                {
                    ret.owner = VidUpdateTypeEnum.CELL;//셀에 파라미터가 있다고 판단.
                }
                else
                {
                    if (gemvidInfo.val != null)
                    {
                        ret.owner = gemvidInfo.val.ProcessorType;
                    }
                    else
                    {
                        //invalid한 상태.
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }

        public long GEMSetVariables(long nObjectID, long nVid, int stageNum = -1, bool immediatelyUpdate = false)
        {
            //v22_merge// 코드 검토 필요 EnumVidObjectType.OBJECT 이게 맞나?
            long retVal = -1;
            try
            {
                lock (Commander.lockObj)
                {
                    EnumVidType vidType = EnumVidType.NONE;
                    if (immediatelyUpdate)
                    {
                        vidType = EnumVidType.SVID;
                    }
                    else
                    {
                        vidType = EnumVidType.DVID;
                    }

                    if (ExecutorDataDic.Count >= stageNum - 1)
                    {
                        if (!ExecutorDataDic[stageNum - 1].ContainsKey(nVid))
                        {
                            //Dictionary 에 없는 경우
                            ExecutorDataDic[stageNum - 1].Add(nVid, new VIDInfomation(nObjectID, vidType, EnumVidObjectType.OBJECT));
                            retVal = 0;
                        }
                        else
                        {
                            //Dictionary 의 Value 만 변경.
                            ExecutorDataDic[stageNum - 1][nVid].Value = nObjectID;
                            retVal = 0;
                        }
                    }


                    if (immediatelyUpdate)
                    {
                        return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMSetVariables(nObjectID, nVid) ?? -1;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public long GEMSetVarName(long nCount, string[] psVidName, string[] psValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMSetVarName(nCount, psVidName, psValue) ?? -1;
        }

        public long GEMGetVarName(long nCount, ref string[] psVidName, ref string[] psValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMGetVarName(nCount, ref psVidName, ref psValue) ?? -1;
        }

        public long GEMSetEvent(long nEventID, int stageNum = -1)
        {
            //v22_merge// VIDInfomation 사용으로 인해 검토 필요
            long retVal = -1;
            try
            {
                LoggerManager.Debug($"[GEMHOST] EventID : {nEventID}, StageNum : {stageNum}");
                //lock (this.GEMModule().GemCommManager.GetLockObj())
                lock (this.GEMModule().GemCommManager.GetProcessorLockObj())
                {
                    ///CEID 
                    if (stageNum != -1)
                    {
                        LoggerManager.Debug($"[GEMHOST] START EventID : {nEventID}, StageNum : {stageNum}");
                        var vids = GetVidsFormCeid(nEventID);
                        foreach (var vid in vids)
                        {
                            EnumVidType type = EnumVidType.NONE;
                            if (vid == -1)
                                return vid;

                            VIDInfomation vidInfo = null;
                            object value = null;
                            bool isExist = true;
                            isExist = ExecutorDataDic[stageNum - 1].ContainsKey(vid);
                            if (isExist == true)
                            {
                                ExecutorDataDic[stageNum - 1].TryGetValue(vid, out vidInfo);
                                if (vidInfo == null)
                                {
                                    value = "";
                                }
                                else
                                {
                                    value = vidInfo.Value;

                                    if (vidInfo.VidObjectType == EnumVidObjectType.NONE)
                                    {
                                        this.GEMModule().GemCommManager.SetVariable(new long[] { vid }, new string[] { (string)value }, type);
                                    }
                                    else if (vidInfo.VidObjectType == EnumVidObjectType.OBJECT)
                                    {
                                        if (value is long)
                                        {
                                            this.GEMModule().GemCommManager.SetVariables((long)value, vid, type);
                                        }
                                        else
                                        {
                                            LoggerManager.Debug($"SetVariables parameter type is invalid. vid:{vid}, value type:{value.GetType()}");
                                        }
                                    }
                                }
                                //LoggerManager.Debug($"[XGemCommanderHost] SetVariable(): [{nEventID}] ({vid}/{full_path}) =  {value}");
                            }
                            else
                            {
                                LoggerManager.Debug($"[XGemCommanderHost] not eixist data when set event. eventNum : {nEventID}, vid : {vid}");
                                //LoggerManager.Debug($"[XGemCommanderHost] SetVariable(): [{nEventID}] ({vid}/{full_path}) =  N/A");
                            }
                        }
                    }
                    //Delay(300);
                    retVal = Commander.Proc_SetEvent(nEventID);
                    LoggerManager.Debug($"[GEMHOST] END EventID : {nEventID}, StageNum : {stageNum}");// 여기는 로더만 탐.

                    if (retVal < 0)
                    {
                        LoggerManager.Debug($"[GEMHOST] Set Event Error EventID : {nEventID}, StageNum : {stageNum}");
                    }
                    if (this.GEMModule().IsExternalLotMode == true)
                    {
                        if (this.GEMModule().GemSysParam.ReceiveMessageType.Equals("SemicsGemReceiverSEKR"))
                        {
                            if (nEventID == 113 || nEventID == 114)
                            {
                                var reqdata = new ZUpActReqData();
                                reqdata.ActionType = EnumRemoteCommand.Z_UP;
                                reqdata.StageNumber = stageNum;
                                this.GEMModule().GemCommManager.OnRemoteCommandAction(reqdata);
                            }
                            else if (nEventID == 103 || nEventID == 104)
                            {
                                //Thread.Sleep(2000);
                                var reqdata = new EndTestReqDate();
                                reqdata.ActionType = EnumRemoteCommand.END_TEST;
                                reqdata.StageNumber = stageNum;
                                reqdata.PMIExecFlag = 0;
                                this.GEMModule().GemCommManager.OnRemoteCommandAction(reqdata);
                            }
                            else if (nEventID == 301 | nEventID == 302)
                            {
                                long downloadRecipeResultVID = 1037;
                                object value = null;
                                VIDInfomation vidInfo = null;

                                if (ExecutorDataDic[stageNum - 1].ContainsKey(downloadRecipeResultVID) == true)
                                {
                                    ExecutorDataDic[stageNum - 1].TryGetValue(downloadRecipeResultVID, out vidInfo);

                                    if (vidInfo != null)
                                    {
                                        value = vidInfo.Value;
                                        if (Convert.ToInt32(value) == 0)
                                        {
                                            value = false;
                                        }
                                        else if (Convert.ToInt32(value) >= 1)
                                        {
                                            value = true;
                                        }

                                        this.GEMModule().SetStageDownloadRecipeResult(stageNum, (bool)value);
                                    }
                                }
                                //}
                            }
                        }
                        else if (this.GEMModule().GemSysParam.ReceiveMessageType.Equals("SemicsGemReceiverSEKT") ||
                                this.GEMModule().GemSysParam.ReceiveMessageType.Equals("SemicsGemReceiverSEKS"))
                        {
                            if (nEventID == 3114)
                            {
                                var reqdata = new StageActReqData();
                                reqdata.ActionType = EnumRemoteCommand.ZUP;
                                reqdata.StageNumber = stageNum;
                                this.GEMModule().GemCommManager.OnRemoteCommandAction(reqdata);
                            }
                            else if (nEventID == 3113)
                            {
                                var reqdata = new StageActReqData();
                                reqdata.ActionType = EnumRemoteCommand.TESTEND;
                                reqdata.StageNumber = stageNum;
                                this.GEMModule().GemCommManager.OnRemoteCommandAction(reqdata);
                            }
                            else if (nEventID == 3104)
                            {
                                var reqdata = new StageActReqData();
                                reqdata.ActionType = EnumRemoteCommand.WAFERUNLOAD;
                                reqdata.StageNumber = stageNum;
                                this.GEMModule().GemCommManager.OnRemoteCommandAction(reqdata);
                            }
                            else if (nEventID == 301 | nEventID == 302)
                            {
                                long downloadRecipeResultVID = 1037;
                                object value = null;
                                VIDInfomation vidInfo = null;
                                if (ExecutorDataDic[stageNum - 1].ContainsKey(downloadRecipeResultVID) == true)
                                {
                                    ExecutorDataDic[stageNum - 1].TryGetValue(downloadRecipeResultVID, out vidInfo);

                                    if (vidInfo != null)
                                    {
                                        value = vidInfo.Value;
                                        if (Convert.ToInt32(value) == 0)
                                        {
                                            value = false;
                                        }
                                        else if (Convert.ToInt32(value) >= 1)
                                        {
                                            value = true;
                                        }

                                        this.GEMModule().SetStageDownloadRecipeResult(stageNum, (bool)value);
                                    }
                                }
                            }
                        }
                        else if (this.GEMModule().GemSysParam.ReceiveMessageType.Equals("SemicsGemReceiverSEKX"))
                        {
                          

                            if (nEventID == 301 | nEventID == 302)
                            {
                                long downloadRecipeResultVID = 1037;
                                object value = null;
                                VIDInfomation vidInfo = null;

                                if (ExecutorDataDic[stageNum - 1].ContainsKey(downloadRecipeResultVID) == true)
                                {
                                    ExecutorDataDic[stageNum - 1].TryGetValue(downloadRecipeResultVID, out vidInfo);

                                    if (vidInfo != null)
                                    {
                                        value = vidInfo.Value;
                                        if (Convert.ToInt32(value) == 0)
                                        {
                                            value = false;
                                        }
                                        else if (Convert.ToInt32(value) >= 1)
                                        {
                                            value = true;
                                        }

                                        this.GEMModule().SetStageDownloadRecipeResult(stageNum, (bool)value);
                                    }
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public long GEMSetSpecificMessage(long nObjectID, string sMsgName)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMSetSpecificMessage(nObjectID, sMsgName) ?? -1;
        }

        public long GEMGetSpecificMessage(long nSObjectID, ref long pnRObjectID, string sMsgName)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMGetSpecificMessage(nSObjectID, ref pnRObjectID, sMsgName) ?? -1;
        }

        public long GetAllStringItem(long nObjectID, ref string psValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetAllStringItem(nObjectID, ref psValue) ?? -1;
        }

        public long SetAllStringItem(long nObjectID, string sValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.SetAllStringItem(nObjectID, sValue) ?? -1;
        }

        public long GEMReqPPSendEx(string sPpid, string sRecipePath)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMReqPPSendEx(sPpid, sRecipePath) ?? -1;
        }

        public long GEMRspPPSendEx(long nMsgId, string sPpid, string sRecipePath, long nResult)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMRspPPSendEx(nMsgId, sPpid, sRecipePath, nResult) ?? -1;
        }

        public long GEMReqPPEx(string sPpid, string sRecipePath)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMReqPPEx(sPpid, sRecipePath) ?? -1;
        }

        public long GEMRspPPEx(long nMsgId, string sPpid, string sRecipePath)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMRspPPEx(nMsgId, sPpid, sRecipePath) ?? -1;
        }

        public long GEMSetVariableEx(long nObjectID, long nVid)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMSetVariableEx(nObjectID, nVid) ?? -1;
        }

        public long GEMReqLoopback(long nCount, long[] pnAbs)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMReqLoopback(nCount, pnAbs) ?? -1;
        }

        public long GEMSetEventEnable(long nCount, long[] pnCEIDs, long nEnable)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMSetEventEnable(nCount, pnCEIDs, nEnable) ?? -1;
        }

        public long GEMSetAlarmEnable(long nCount, long[] pnALIDs, long nEnable)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMSetAlarmEnable(nCount, pnALIDs, nEnable) ?? -1;
        }

        public long GEMGetEventEnable(long nCount, long[] pnCEIDs, ref long[] pnEnable)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMGetEventEnable(nCount, pnCEIDs, ref pnEnable) ?? -1;
        }

        public long GEMGetAlarmEnable(long nCount, long[] pnALIDs, ref long[] pnEnable)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMGetAlarmEnable(nCount, pnALIDs, ref pnEnable) ?? -1;
        }

        public long GEMGetAlarmInfo(long nCount, long[] pnALIDs, ref long[] pnALCDs, ref string[] psALTXs)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMGetAlarmInfo(nCount, pnALIDs, ref pnALCDs, ref psALTXs) ?? -1;
        }

        public long GEMGetSVInfo(long nCount, long[] pnSVIDs, ref string[] psMins, ref string[] psMaxs)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMGetSVInfo(nCount, pnSVIDs, ref psMins, ref psMaxs) ?? -1;
        }

        public long GEMGetECVInfo(long nCount, long[] pnECIDs, ref string[] psNames, ref string[] psDefs, ref string[] psMins, ref string[] psMaxs, ref string[] psUnits)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMGetECVInfo(nCount, pnECIDs, ref psNames, ref psDefs, ref psMins, ref psMaxs, ref psUnits) ?? -1;
        }

        public long GEMRsqOffline(long nMsgId, long nAck)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMRsqOffline(nMsgId, nAck) ?? -1;
        }

        public long GEMRspOnline(long nMsgId, long nAck)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMRspOnline(nMsgId, nAck) ?? -1;
        }

        public long GEMReqHostOffline()
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMReqHostOffline() ?? -1;
        }

        public long GEMReqStartPolling(string sName, long nScanTime)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMReqStartPolling(sName, nScanTime) ?? -1;
        }

        public long GEMReqStopPolling(string sName)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMReqStopPolling(sName) ?? -1;
        }

        public long GEMEnableLog(long bEnabled)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMEnableLog(bEnabled) ?? -1;
        }

        public long GEMSetLogOption(string sDriectory, string sPrefix, string sExtension, long nKeepDay, long bMakeDailyLog, long bMakeSubDirectory)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMSetLogOption(sDriectory, sPrefix, sExtension, nKeepDay, bMakeDailyLog, bMakeSubDirectory) ?? -1;
        }

        public long CloseObject(long nObjectID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CloseObject(nObjectID) ?? -1;
        }

        //Add
        public long GetCurrentItemInfo(long nObjectID, ref long pnItemType, ref long pnItemCount)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetCurrentItemInfo(nObjectID, ref pnItemType, ref pnItemCount) ?? -1;
        }

        #endregion

        #region <remarks> Request To GEM300Pro Dongle </remarks>
        public long CJDelAllJobInfo()
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CJDelAllJobInfo() ?? -1;
        }

        public long CJDelJobInfo(string sCJobID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CJDelJobInfo(sCJobID) ?? -1;
        }

        public long CJGetAllJobInfo(ref long pnObjID, ref long pnCount)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CJGetAllJobInfo(ref pnObjID, ref pnCount) ?? -1;
        }

        public long CJGetHOQJob()
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CJGetHOQJob() ?? -1;
        }

        public long CJReqCommand(string sCJobID, long nCommand, string sCPName, string sCPVal)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CJReqCommand(sCJobID, nCommand, sCPName, sCPVal) ?? -1;
        }

        public long CJReqCreate(string sCJobID, long nStartMethod, long nCountPRJob, string[] psPRJobID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CJReqCreate(sCJobID, nStartMethod, nCountPRJob, psPRJobID) ?? -1;
        }

        public long CJReqGetAllJobID()
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CJReqGetAllJobID() ?? -1;
        }

        public long CJReqGetJob(string sCJobID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CJReqGetJob(sCJobID) ?? -1;
        }

        public long CJReqHOQJob(string sCJobID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CJReqHOQJob(sCJobID) ?? -1;
        }

        public long CJReqSelect(string sCJobID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CJReqSelect(sCJobID) ?? -1;
        }

        public long CJRspCommand(long nMsgId, string sCJobID, long nCommand, long nResult, long nErrCode, string sErrText)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CJRspCommand(nMsgId, sCJobID, nCommand, nResult, nErrCode, sErrText) ?? -1;
        }

        public long CJRspVerify(long nMsgId, string sCJobID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CJRspVerify(nMsgId, sCJobID, nResult, nErrCount, pnErrCode, psErrText) ?? -1;
        }

        public long CJSetJobInfo(string sCJobID, long nState, long nStartMethod, long nCountPRJob, string[] psPRJobID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CJSetJobInfo(sCJobID, nState, nStartMethod, nCountPRJob, psPRJobID) ?? -1;
        }

        public long CloseGEMObject(long nMsgID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CloseGEMObject(nMsgID) ?? -1;
        }

        public long CMSDelAllCarrierInfo()
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSDelAllCarrierInfo() ?? -1;
        }

        public long CMSDelCarrierInfo(string sCarrierID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSDelCarrierInfo(sCarrierID) ?? -1;
        }

        public long CMSGetAllCarrierInfo(ref long pnMsgId, ref long pnCount)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSGetAllCarrierInfo(ref pnMsgId, ref pnCount) ?? -1;
        }

        public long CMSReqBind(string sLocID, string sCarrierID, string sSlotMap)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSReqBind(sLocID, sCarrierID, sSlotMap) ?? -1;
        }

        public long CMSReqCancelBind(string sLocID, string sCarrierID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSReqCancelBind(sLocID, sCarrierID) ?? -1;
        }

        public long CMSReqCancelCarrier(string sLocID, string sCarrierID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSReqCancelCarrier(sLocID, sCarrierID) ?? -1;
        }

        public long CMSReqCarrierIn(string sLocID, string sCarrierID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSReqCarrierIn(sLocID, sCarrierID) ?? -1;
        }

        public long CMSReqCarrierOut(string sLocID, string sCarrierID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSReqCarrierOut(sLocID, sCarrierID) ?? -1;
        }

        public long CMSReqCarrierReCreate(string sLocID, string sCarrierID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSReqCarrierReCreate(sLocID, sCarrierID) ?? -1;
        }

        public long CMSReqChangeAccess(long nMode, string sLocID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSReqChangeAccess(nMode, sLocID) ?? -1;
        }

        public long CMSReqChangeServiceStatus(string sLocID, long nState)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSReqChangeServiceStatus(sLocID, nState) ?? -1;
        }

        public long CMSReqProceedCarrier(string sLocID, string sCarrierID, string sSlotMap, long nCount, string[] psLotID, string[] psSubstrateID, string sUsage)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSReqProceedCarrier(sLocID, sCarrierID, sSlotMap, nCount, psLotID, psSubstrateID, sUsage) ?? -1;
        }

        public long CMSRspCancelCarrier(long nMsgId, string sLocID, string sCarrierID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSRspCancelCarrier(nMsgId, sLocID, sCarrierID, nResult, nErrCount, pnErrCode, psErrText) ?? -1;
        }

        public long CMSRspCancelCarrierAtPort(long nMsgId, string sLocID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSRspCancelCarrierAtPort(nMsgId, sLocID, nResult, nErrCount, pnErrCode, psErrText) ?? -1;
        }

        public long CMSRspCancelCarrierOut(long nMsgId, string sCarrierID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSRspCancelCarrierOut(nMsgId, sCarrierID, nResult, nErrCount, pnErrCode, psErrText) ?? -1;
        }

        public long CMSRspCarrierIn(long nMsgId, string sLocID, string sCarrierID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSRspCarrierIn(nMsgId, sLocID, sCarrierID, nResult, nErrCount, pnErrCode, psErrText) ?? -1;
        }

        public long CMSRspCarrierOut(long nMsgId, string sLocID, string sCarrierID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSRspCarrierOut(nMsgId, sLocID, sCarrierID, nResult, nErrCount, pnErrCode, psErrText) ?? -1;
        }

        public long CMSRspCarrierRelease(long nMsgId, string sLocID, string sCarrierID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSRspCarrierRelease(nMsgId, sLocID, sCarrierID, nResult, nErrCount, pnErrCode, psErrText) ?? -1;
        }

        public long CMSRspCarrierTagReadData(long nMsgId, string sLocID, string sCarrierID, string sData, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSRspCarrierTagReadData(nMsgId, sLocID, sCarrierID, sData, nResult, nErrCount, pnErrCode, psErrText) ?? -1;
        }

        public long CMSRspCarrierTagWriteData(long nMsgId, string sLocID, string sCarrierID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSRspCarrierTagWriteData(nMsgId, sLocID, sCarrierID, nResult, nErrCount, pnErrCode, psErrText) ?? -1;
        }

        public long CMSRspChangeAccess(long nMsgId, long nMode, long nResult, long nErrCount, string[] psLocID, long[] pnErrCode, string[] psErrText)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSRspChangeAccess(nMsgId, nMode, nResult, nErrCount, psLocID, pnErrCode, psErrText) ?? -1;
        }

        public long CMSRspChangeServiceStatus(long nMsgId, string sLocID, long nState, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSRspChangeServiceStatus(nMsgId, sLocID, nState, nResult, nErrCount, pnErrCode, psErrText) ?? -1;
        }

        public long CMSSetBufferCapacityChanged(string sPartID, string sPartType, long nAPPCapacity, long nPCapacity, long nUnPCapacity)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSSetBufferCapacityChanged(sPartID, sPartType, nAPPCapacity, nPCapacity, nUnPCapacity) ?? -1;
        }

        public long CMSSetCarrierAccessing(string sLocID, long nState, string sCarrierID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSSetCarrierAccessing(sLocID, nState, sCarrierID) ?? -1;
        }

        public long CMSSetCarrierID(string sLocID, string sCarrierID, long nResult)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSSetCarrierID(sLocID, sCarrierID, nResult) ?? -1;
        }

        public long CMSSetCarrierIDStatus(string sCarrierID, long nState)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSSetCarrierIDStatus(sCarrierID, nState) ?? -1;
        }

        public long CMSSetCarrierInfo(string sCarrierID, string sLocID, long nIdStatus, long nSlotMapStatus, long nAccessingStatus, string sSlotMap, long nContentsMapCount, string[] psLotID, string[] psSubstrateID, string sUsage)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSSetCarrierInfo(sCarrierID, sLocID, nIdStatus, nSlotMapStatus, nAccessingStatus, sSlotMap, nContentsMapCount, psLotID, psSubstrateID, sUsage) ?? -1;
        }

        public long CMSSetCarrierLocationInfo(string sLocID, string sCarrierID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSSetCarrierLocationInfo(sLocID, sCarrierID) ?? -1;
        }

        public long CMSSetCarrierMovement(string sLocID, string sCarrierID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSSetCarrierMovement(sLocID, sCarrierID) ?? -1;
        }

        public long CMSSetCarrierOnOff(string sLocID, long nState)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSSetCarrierOnOff(sLocID, nState) ?? -1;
        }

        public long CMSSetCarrierOutStart(string sLocID, string sCarrierID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSSetCarrierOutStart(sLocID, sCarrierID) ?? -1;
        }

        public long CMSSetLPInfo(string sLocID, long nTransferState, long nAccessMode, long nReservationState, long nAssociationState, string sCarrierID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSSetLPInfo(sLocID, nTransferState, nAccessMode, nReservationState, nAssociationState, sCarrierID) ?? -1;
        }

        public long CMSSetMaterialArrived(string sMaterialID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSSetMaterialArrived(sMaterialID) ?? -1;
        }

        public long CMSSetPIOSignalState(string sLocID, long nSignal, long nState)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSSetPIOSignalState(sLocID, nSignal, nState) ?? -1;
        }

        public long CMSSetPresenceSensor(string sLocID, long nState)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSSetPresenceSensor(sLocID, nState) ?? -1;
        }

        public long CMSSetReadyToLoad(string sLocID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSSetReadyToLoad(sLocID) ?? -1;
        }

        public long CMSSetReadyToUnload(string sLocID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSSetReadyToUnload(sLocID) ?? -1;
        }

        public long CMSSetSlotMap(string sLocID, string sSlotMap, string sCarrierID, long nResult)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSSetSlotMap(sLocID, sSlotMap, sCarrierID, nResult) ?? -1;
        }

        public long CMSSetSlotMapStatus(string sCarrierID, long nState)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSSetSlotMapStatus(sCarrierID, nState) ?? -1;
        }

        public long CMSSetSubstrateCount(string sCarrierID, long nSubstCount)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSSetSubstrateCount(sCarrierID, nSubstCount) ?? -1;
        }

        public long CMSSetTransferReady(string sLocID, long nState)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSSetTransferReady(sLocID, nState) ?? -1;
        }

        public long CMSSetUsage(string sCarrierID, string sUsage)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.CMSSetUsage(sCarrierID, sUsage) ?? -1;
        }

        public long GEMGetVariables(ref long pnObjectID, long nVid)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMGetVariables(ref pnObjectID, nVid) ?? -1;
        }

        //public long GEMReqPPSend(string sPpid, byte[] baBody)
        //{
        //    return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMReqPPSend(sPpid, baBody) ?? -1;
        //}

        public long GEMRspOffline(long nMsgId, long nAck)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMRspOffline(nMsgId, nAck) ?? -1;
        }

        //public long GEMRspPP(long nMsgId, string sPpid, byte[] baBody)
        //{
        //    return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMRspPP(nMsgId, sPpid, baBody) ?? -1;
        //}

        public long GEMSetEventEx(long nEventID, long nCount, long[] pnVID, string[] psValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMSetEventEx(nEventID, nCount, pnVID, psValue) ?? -1;
        }

        public bool GetActive()
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetActive() ?? false;
        }

        public long GetAttrData(ref long pnObjectID, long nMsgID, string sAttrName)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetAttrData(ref pnObjectID, nMsgID, sAttrName) ?? -1;
        }

        public string GetIP()
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetIP() ?? "";
        }

        public long GetPort()
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetPort() ?? -1;
        }

        public void SetIP(string sNewValue)
        {
            (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.SetIP(sNewValue);
        }

        public void SetPort(long nNewValue)
        {
            (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.SetPort(nNewValue);
        }

        public long OpenGEMObject(ref long pnMsgID, string sObjType, string sObjID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.OpenGEMObject(ref pnMsgID, sObjType, sObjID) ?? -1;
        }

        public long STSDelAllSubstrateInfo()
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.STSDelAllSubstrateInfo() ?? -1;
        }

        public long STSDelSubstrateInfo(string sSubstrateID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.STSDelSubstrateInfo(sSubstrateID) ?? -1;
        }

        public long STSGetAllSubstrateInfo(ref long pnObjID, ref long pnCount)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.STSGetAllSubstrateInfo(ref pnObjID, ref pnCount) ?? -1;
        }

        public long STSReqCancelSubstrate(string sSubstLocID, string sSubstrateID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.STSReqCancelSubstrate(sSubstLocID, sSubstrateID) ?? -1;
        }

        public long STSReqCreateSubstrate(string sSubstLocID, string sSubstrateID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.STSReqCreateSubstrate(sSubstLocID, sSubstrateID) ?? -1;
        }

        public long STSReqDeleteSubstrate(string sSubstLocID, string sSubstrateID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.STSReqDeleteSubstrate(sSubstLocID, sSubstrateID) ?? -1;
        }

        public long STSReqProceedSubstrate(string sSubstLocID, string sSubstrateID, string sReadSubstID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.STSReqProceedSubstrate(sSubstLocID, sSubstrateID, sReadSubstID) ?? -1;
        }

        public long STSRspCancelSubstrate(long nMsgID, string sSubstLocID, string sSubstrateID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.STSRspCancelSubstrate(nMsgID, sSubstLocID, sSubstrateID, nResult, nErrCount, pnErrCode, psErrText) ?? -1;
        }

        public long STSRspCreateSubstrate(long nMsgID, string sSubstLocID, string sSubstrateID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.STSRspCreateSubstrate(nMsgID, sSubstLocID, sSubstrateID, nResult, nErrCount, pnErrCode, psErrText) ?? -1;
        }

        public long STSRspDeleteSubstrate(long nMsgID, string sSubstLocID, string sSubstrateID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.STSRspDeleteSubstrate(nMsgID, sSubstLocID, sSubstrateID, nResult, nErrCount, pnErrCode, psErrText) ?? -1;
        }

        public long STSRspUpdateSubstrate(long nMsgID, string sSubstLocID, string sSubstrateID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.STSRspUpdateSubstrate(nMsgID, sSubstLocID, sSubstrateID, nResult, nErrCount, pnErrCode, psErrText) ?? -1;
        }

        public long STSSetBatchLocationInfo(string sBatchLocID, string sSubstrateID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.STSSetBatchLocationInfo(sBatchLocID, sSubstrateID) ?? -1;
        }

        public long STSSetBatchProcessing(long nCount, string[] psSubstLocID, string[] psSubstrateID, long nState)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.STSSetBatchProcessing(nCount, psSubstLocID, psSubstrateID, nState) ?? -1;
        }

        public long STSSetBatchTransport(long nCount, string[] psSubstLocID, string[] psSubstrateID, long nState)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.STSSetBatchTransport(nCount, psSubstLocID, psSubstrateID, nState) ?? -1;
        }

        public long STSSetMaterialArrived(string sMaterialID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.STSSetMaterialArrived(sMaterialID) ?? -1;
        }

        public long STSSetProcessing(string sSubstLocID, string sSubstrateID, long nState)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.STSSetProcessing(sSubstLocID, sSubstrateID, nState) ?? -1;
        }

        public long STSSetSubstLocationInfo(string sSubstLocID, string sSubstrateID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.STSSetSubstLocationInfo(sSubstLocID, sSubstrateID) ?? -1;
        }

        public long STSSetSubstrateID(string sSubstLocID, string sSubstrateID, string sSubstReadID, long nResult)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.STSSetSubstrateID(sSubstLocID, sSubstrateID, sSubstReadID, nResult) ?? -1;
        }

        public long STSSetSubstrateInfo(string sSubstLocID, string sSubstrateID, long nTransportState, long nProcessingState, long nReadingState)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.STSSetSubstrateInfo(sSubstLocID, sSubstrateID, nTransportState, nProcessingState, nReadingState) ?? -1;
        }

        public long STSSetTransport(string sSubstLocID, string sSubstrateID, long nState)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.STSSetTransport(sSubstLocID, sSubstrateID, nState) ?? -1;
        }

        public long GetCarrierAccessingStatus(long nMsgId, long nIndex, ref long pnState)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetCarrierAccessingStatus(nMsgId, nIndex, ref pnState) ?? -1;
        }

        public long GetCarrierClose(long nMsgId)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetCarrierClose(nMsgId) ?? -1;
        }

        public long GetCarrierContentsMap(long nMsgId, long nIndex, long nCount, ref string[] psLotID, ref string[] psSubstrateID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetCarrierContentsMap(nMsgId, nIndex, nCount, ref psLotID, ref psSubstrateID) ?? -1;
        }

        public long GetCarrierContentsMapCount(long nMsgId, long nIndex, ref long pnCount)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetCarrierContentsMapCount(nMsgId, nIndex, ref pnCount) ?? -1;
        }

        public long GetCarrierID(long nMsgId, long nIndex, ref string psCarrierID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetCarrierID(nMsgId, nIndex, ref psCarrierID) ?? -1;
        }

        public long GetCarrierIDStatus(long nMsgId, long nIndex, ref long pnState)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetCarrierIDStatus(nMsgId, nIndex, ref pnState) ?? -1;
        }

        public long GetCarrierLocID(long nMsgId, long nIndex, ref string psLocID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetCarrierLocID(nMsgId, nIndex, ref psLocID) ?? -1;
        }

        public long GetCarrierSlotMap(long nMsgId, long nIndex, ref string psSlotMap)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetCarrierSlotMap(nMsgId, nIndex, ref psSlotMap) ?? -1;
        }

        public long GetCarrierSlotMapStatus(long nMsgId, long nIndex, ref long pnState)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetCarrierSlotMapStatus(nMsgId, nIndex, ref pnState) ?? -1;
        }

        public long GetCarrierUsage(long nMsgId, long nIndex, ref string psUsage)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetCarrierUsage(nMsgId, nIndex, ref psUsage) ?? -1;
        }

        public long GetCtrlJobClose(long nObjID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetCtrlJobClose(nObjID) ?? -1;
        }

        public long GetCtrlJobID(long nObjID, long nIndex, ref string psCtrlJobID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetCtrlJobID(nObjID, nIndex, ref psCtrlJobID) ?? -1;
        }

        public long GetCtrlJobPRJobCount(long nObjID, long nIndex, ref long pnCount)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetCtrlJobPRJobCount(nObjID, nIndex, ref pnCount) ?? -1;
        }

        public long GetCtrlJobPRJobIDs(long nObjID, long nIndex, long nCount, ref string[] psPRJobIDs)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetCtrlJobPRJobIDs(nObjID, nIndex, nCount, ref psPRJobIDs) ?? -1;
        }

        public long GetCtrlJobStartMethod(long nObjID, long nIndex, ref long pnAutoStart)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetCtrlJobStartMethod(nObjID, nIndex, ref pnAutoStart) ?? -1;
        }

        public long GetCtrlJobState(long nObjID, long nIndex, ref long pnState)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetCtrlJobState(nObjID, nIndex, ref pnState) ?? -1;
        }

        public long GetPRJobAutoStart(long nObjID, long nIndex, ref long pnAutoStart)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetPRJobAutoStart(nObjID, nIndex, ref pnAutoStart) ?? -1;
        }

        public long GetPRJobCarrier(long nObjID, long nIndex, long nCount, ref string[] psCarrierID, ref string[] psSlotInfo)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetPRJobCarrier(nObjID, nIndex, nCount, ref psCarrierID, ref psSlotInfo) ?? -1;
        }

        public long GetPRJobCarrierCount(long nObjID, long nIndex, ref long pnCount)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetPRJobCarrierCount(nObjID, nIndex, ref pnCount) ?? -1;
        }

        public long GetPRJobClose(long nObjID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetPRJobClose(nObjID) ?? -1;
        }

        public long GetPRJobID(long nObjID, long nIndex, ref string psPJobID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetPRJobID(nObjID, nIndex, ref psPJobID) ?? -1;
        }

        public long GetPRJobMtrlFormat(long nObjID, long nIndex, ref long pnMtrlFormat)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetPRJobMtrlFormat(nObjID, nIndex, ref pnMtrlFormat) ?? -1;
        }

        public long GetPRJobMtrlOrder(long nObjID, long nIndex, ref long pnMtrlOrder)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetPRJobMtrlOrder(nObjID, nIndex, ref pnMtrlOrder) ?? -1;
        }

        public long GetPRJobRcpID(long nObjID, long nIndex, ref string psRcpID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetPRJobRcpID(nObjID, nIndex, ref psRcpID) ?? -1;
        }

        public long GetPRJobRcpParam(long nObjID, long nIndex, long nCount, ref string[] psRcpParName, ref string[] psRcpParValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetPRJobRcpParam(nObjID, nIndex, nCount, ref psRcpParName, ref psRcpParValue) ?? -1;
        }

        public long GetPRJobRcpParamEx(long nObjID, long nIndex, long nCount, ref string[] psRcpParName, ref long[] pnRcpParValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetPRJobRcpParamEx(nObjID, nIndex, nCount, ref psRcpParName, ref pnRcpParValue) ?? -1;
        }

        public long GetPRJobRcpParamCount(long nObjID, long nIndex, ref long pnCount)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetPRJobRcpParamCount(nObjID, nIndex, ref pnCount) ?? -1;
        }

        public long GetPRJobState(long nObjID, long nIndex, ref long pnState)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetPRJobState(nObjID, nIndex, ref pnState) ?? -1;
        }

        public long GetSubstrateClose(long nObjID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetSubstrateClose(nObjID) ?? -1;
        }

        public long GetSubstrateID(long nObjID, long nIndex, ref string psSubstrateID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetSubstrateID(nObjID, nIndex, ref psSubstrateID) ?? -1;
        }

        public long GetSubstrateLocID(long nObjID, long nIndex, ref string psSubstLocID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetSubstrateLocID(nObjID, nIndex, ref psSubstLocID) ?? -1;
        }

        public long GetSubstrateState(long nObjID, long nIndex, ref long pnTransportState, ref long pnProcessingState, ref long pnReadingState)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GetSubstrateState(nObjID, nIndex, ref pnTransportState, ref pnProcessingState, ref pnReadingState) ?? -1;
        }

        public long PJDelAllJobInfo()
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.PJDelAllJobInfo() ?? -1;
        }

        public long PJDelJobInfo(string sPJobID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.PJDelJobInfo(sPJobID) ?? -1;
        }

        public long PJGetAllJobInfo(ref long pnObjID, ref long pnPJobCount)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.PJGetAllJobInfo(ref pnObjID, ref pnPJobCount) ?? -1;
        }

        public long PJReqCommand(long nCommand, string sPJobID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.PJReqCommand(nCommand, sPJobID) ?? -1;
        }

        public long PJReqCreate(string sPJobID, long nMtrlFormat, long nAutoStart, long nMtrlOrder, long nMtrlCount, string[] psMtrlID, string[] psSlotInfo, long nRcpMethod, string sRcpID, long nRcpParCount, string[] psRcpParName, string[] psRcpParValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.PJReqCreate(sPJobID, nMtrlFormat, nAutoStart, nMtrlOrder, nMtrlCount, psMtrlID, psSlotInfo, nRcpMethod, sRcpID, nRcpParCount, psRcpParName, psRcpParValue) ?? -1;
        }

        public long PJReqCreateEx(string sPJobID, long nMtrlFormat, long nAutoStart, long nMtrlOrder, long nMtrlCount, string[] psMtrlID, string[] psSlotInfo, long nRcpMethod, string sRcpID, long nRcpParCount, string[] psRcpParName, long[] pnRcpParValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.PJReqCreateEx(sPJobID, nMtrlFormat, nAutoStart, nMtrlOrder, nMtrlCount, psMtrlID, psSlotInfo, nRcpMethod, sRcpID, nRcpParCount, psRcpParName, pnRcpParValue) ?? -1;
        }

        public long PJReqGetAllJobID()
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.PJReqGetAllJobID() ?? -1;
        }

        public long PJReqGetJob(string sPJobID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.PJReqGetJob(sPJobID) ?? -1;
        }

        public long PJRspCommand(long nMsgID, long nCommand, string sPJobID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.PJRspCommand(nMsgID, nCommand, sPJobID, nResult, nErrCount, pnErrCode, psErrText) ?? -1;
        }

        public long PJRspSetMtrlOrder(long nMsgID, long nResult)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.PJRspSetMtrlOrder(nMsgID, nResult) ?? -1;
        }

        public long PJRspSetRcpVariable(long nMsgID, string sPJobID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.PJRspSetRcpVariable(nMsgID, sPJobID, nResult, nErrCount, pnErrCode, psErrText) ?? -1;
        }

        public long PJRspSetStartMethod(long nMsgID, long nPJobCount, string[] psPJobID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.PJRspSetStartMethod(nMsgID, nPJobCount, psPJobID, nResult, nErrCount, pnErrCode, psErrText) ?? -1;
        }

        public long PJRspVerify(long nMsgID, long nPJobCount, string[] psPJobID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.PJRspVerify(nMsgID, nPJobCount, psPJobID, nResult, nErrCount, pnErrCode, psErrText) ?? -1;
        }

        public long PJSetJobInfo(string sPJobID, long nMtrlFormat, long nAutoStart, long nMtrlOrder, long nMtrlCount, string[] psMtrlID, string[] psSlotInfo, long nRcpMethod, string sRcpID, long nRcpParCount, string[] psRcpParName, string[] psRcpParVal)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.PJSetJobInfo(sPJobID, nMtrlFormat, nAutoStart, nMtrlOrder, nMtrlCount, psMtrlID, psSlotInfo, nRcpMethod, sRcpID, nRcpParCount, psRcpParName, psRcpParVal) ?? -1;
        }

        public long PJSetJobInfoEx(string sPJobID, long nMtrlFormat, long nAutoStart, long nMtrlOrder, long nMtrlCount, string[] psMtrlID, string[] psSlotInfo, long nRcpMethod, string sRcpID, long nRcpParCount, string[] psRcpParName, long[] pnRcpParVal)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.PJSetJobInfoEx(sPJobID, nMtrlFormat, nAutoStart, nMtrlOrder, nMtrlCount, psMtrlID, psSlotInfo, nRcpMethod, sRcpID, nRcpParCount, psRcpParName, pnRcpParVal) ?? -1;
        }

        public long PJSetState(string sPJobID, long nState)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.PJSetState(sPJobID, nState) ?? -1;
        }

        public long PJSettingUpCompt(string sPJobID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.PJSettingUpCompt(sPJobID) ?? -1;
        }

        public long PJSettingUpStart(string sPJobID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.PJSettingUpStart(sPJobID) ?? -1;
        }

        public long RangeCheck(long[] pnIds)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.RangeCheck(pnIds) ?? -1;
        }

        public bool ReadXGemInfoFromRegistry()
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.ReadXGemInfoFromRegistry() ?? false;
        }

        public int RunProcess(string sName, string sCfgPath, string sPassword)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.RunProcess(sName, sCfgPath, sPassword) ?? -1;
        }

        public void SetActive(bool bNewValue)
        {
            (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.SetActive(bNewValue);
        }

        public long SetBinaryItems(long nObjectID, byte[] pnValue)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.SetBinaryItems(nObjectID, pnValue) ?? -1;
        }

        public long SendUserMessage(long nObjectID, string sCommand, long nTransID)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.SendUserMessage(nObjectID, sCommand, nTransID) ?? -1;
        }

        public long Initialize(string sCfg)
        {
            return (Commander.GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.Initialize(sCfg) ?? -1;
        }
        #endregion

        #region <!-- CallBack -->
        public void OnRemoteCommandAction(int index, RemoteActReqData reqdata)
        {
            try
            {
                ISecsGemServiceCallback callback = null;
                DicCommanderServiceCallback.TryGetValue(index, out callback);
                lock (callbackLockObj)
                {
                    callback?.OnRemoteCommandAction(reqdata);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        private static DateTime Delay(int MS)
        {
            try
            {
                DateTime ThisMoment = DateTime.Now;
                TimeSpan duration = new TimeSpan(0, 0, 0, 0, MS);
                DateTime AfterWards = ThisMoment.Add(duration);

                while (AfterWards >= ThisMoment)
                {
                    System.Windows.Forms.Application.DoEvents();
                    ThisMoment = DateTime.Now;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return DateTime.Now;
        }


        public bool CheckCommConnectivity(int stageIndex = -1)
        {
            bool retVal = false;
            try
            {
                ISecsGemServiceCallback client = null;
                DicCommanderServiceCallback.TryGetValue(stageIndex, out client);
                if (client != null)
                {
                    ICommunicationObject commObj = client as ICommunicationObject;
                    CommunicationState state = commObj.State;
                    if (state == CommunicationState.Opened)
                        retVal = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class VIDInfomation
    {
        private object _Value;

        public object Value
        {
            get { return _Value; }
            set { _Value = value; }
        }

        private EnumVidType _VidType;

        public EnumVidType VidType
        {
            get { return _VidType; }
            set { _VidType = value; }
        }

        private EnumVidObjectType _VidObjectType;

        public EnumVidObjectType VidObjectType
        {
            get { return _VidObjectType; }
            set { _VidObjectType = value; }
        }

        public VIDInfomation(object value, EnumVidType vidType, EnumVidObjectType vidobjectType)
        {
            Value = value;
            VidType = vidType;
            VidObjectType = vidobjectType;
        }
    }
}
