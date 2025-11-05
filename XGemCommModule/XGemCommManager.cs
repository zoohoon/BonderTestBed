using GemExecutionProcessor;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.GEM;
using SecsGemServiceInterface;
using SecsGemServiceProxy;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Threading.Tasks;
using XGemCommandProcessor;
using XGEMWrapper;

namespace XGemCommModule
{
    public class XGemCommManager : IGEMCommManager, INotifyPropertyChanged

    {
        public int STAGE_ID_SECTION { get; private set; } = 1000000;

        private IGemProcessorCore GemProcessor = null;

        public bool Initialized { get; set; } = false;

        public event PropertyChangedEventHandler PropertyChanged;
        private Process serviceProcess = null;

        private SecsCommInform _SecsCommInformData;
        public SecsCommInform SecsCommInformData
        {
            get { return _SecsCommInformData; }
            set
            {
                if (_SecsCommInformData != value)
                {
                    _SecsCommInformData = value;
                    RaisePropertyChanged();
                }
            }
        }

        private void RaisePropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public void SetGEMDisconnectCallBack(GEMDisconnectDelegate callback)
        {
            GemProcessor?.SetGEMDisconnectCallBack(callback);
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            IParamManager paramManager = this.ParamManager();

            try
            {
                if (Initialized == false)
                {
                    SecsCommInformData = new SecsCommInform();

                    if (this.GEMModule().GemSysParam.GemProcessrorType.Value == GemProcessorType.CELL
                        || this.GEMModule().GemSysParam.GemProcessrorType.Value == GemProcessorType.SINGLE)
                    {
                        GemProcessor = new XGemExecutor();
                    }
                    else if (this.GEMModule().GemSysParam.GemProcessrorType.Value == GemProcessorType.COMMANDER)
                    {
                        GemProcessor = new XGemCommander();
                    }

                    GemProcessor?.Proc_SetCommManager(this);                    

                    retval = GemProcessor?.InitModule() ?? EventCodeEnum.UNDEFINED;
                    Initialized = (retval == EventCodeEnum.NONE);
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");
                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                retval = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }

            return retval;
        }


        public EventCodeEnum InitConnectService()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = GemProcessor?.ConnectService() ?? EventCodeEnum.UNDEFINED;

                if (retVal == EventCodeEnum.NONE)
                {
                    this.GEMModule().RegisteEvent_OnLoadElementInfo();
                    this.GEMModule().GetPIVContainer().InitData();
                    //GemProcessor.InitGemData();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum DeInitConnectService()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = GemProcessor?.DisConnectService() ?? EventCodeEnum.UNDEFINED;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum InitGemData()
        {
            if (GemProcessor != null)
                return GemProcessor.InitGemData();
            else
                return EventCodeEnum.GEM_NOT_EXIST_PROCESSER;
        }

        public EventCodeEnum StartWcfGemService()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            IGEMModule gem = this.GEMModule();

            if (System.IO.File.Exists(gem.GemSysParam.GEMServiceHostWCFPath.Value))
            {
                try
                {
                    serviceProcess = StartWcfGemServiceProcess(gem.GemSysParam.GEMServiceHostWCFPath.Value,
                        gem.GemSysParam.GEMServiceHostAPIType);

                    if (serviceProcess != null)
                    {
                        LoggerManager.Debug("[SECS/GEM] Start Wcf Gem Serivce Process is success");
                        retVal = ConnectToGemService();
                    }
                }
                catch (TimeoutException te)
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] Fail(Timeout) WCF Service");
                    throw new TimeoutException(te.Message);
                }
                catch (Exception err)
                {
                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                    throw new Exception($"[{this.GetType().Name}] Error when Open the GEM Service(WCF).");
                }
            }
            else
            {
                throw new Exception("Not Exist the GEM WCF Service.");
            }

            return retVal;
        }

        private Process Gemprocess = null;
        private Process StartWcfGemServiceProcess(string servicePath, GemAPIType type)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            Process tmpProcess = null;
            Process[] findProcess = null;
            long difCount = 0;
            long difTime = 0;
            string xGemProcessName = "SecsGemServiceHostApp";
            string gemProcessName = "";
            try
            {
                gemProcessName = xGemProcessName;
                psi.FileName = servicePath;
                psi.Arguments = type.ToString();

                tmpProcess = new Process();
                tmpProcess.StartInfo = psi;
                tmpProcess.Start();
                System.Threading.Thread.Sleep(1000);
                findProcess = Process.GetProcessesByName(gemProcessName);

                difCount = TimeSpan.TicksPerSecond; //1000 * 10000;
                difTime = DateTime.Now.Ticks;

                LoggerManager.Debug($"[{DateTime.Now.ToLocalTime()}] : GEM : START WCF Service");
                while (findProcess.Length != 1)
                {
                    if (difCount < DateTime.Now.Ticks - difTime)
                    {
                        break;
                        throw new TimeoutException("GEM WCF Service Open Fail.");
                    }
                    findProcess = Process.GetProcessesByName(gemProcessName);
                    //System.Threading.Thread.Sleep(1);
                    System.Threading.Thread.Sleep(100);
                }

                //System.Threading.Thread.Sleep(1000);
                System.Threading.Thread.Sleep(1000);
                LoggerManager.Debug($"[{this.GetType().Name}] Suceess WCF Service");
                Gemprocess = tmpProcess;
            }
            catch (Exception err)
            {
                tmpProcess = null;
                throw err;
            }

            return tmpProcess;
        }


        private ISecsGemService secsGemService { get; set; }
        /// <summary>
        /// Gem Service와 연결합니다.
        /// </summary>
        /// <returns></returns>
        private EventCodeEnum ConnectToGemService()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            bool resultInit = false;
            try
            {
                if (!GemProcessor.HasSecsGemServiceProxy())
                {
                    var context = new InstanceContext(GemProcessor);
                    Binding binding = new NetNamedPipeBinding()
                    {
                        MaxBufferPoolSize = 2147483647,
                        MaxBufferSize = 2147483647,
                        MaxReceivedMessageSize = 2147483647,
                        SendTimeout = TimeSpan.MaxValue,
                        ReceiveTimeout = TimeSpan.MaxValue
                    };
                    EndpointAddress endpointAddress = new EndpointAddress($"net.pipe://localhost/secsgempipe");
                    var contract = ContractDescription.GetContract(typeof(ISecsGemService));
                    var serviceEndpoint = new ServiceEndpoint(contract, binding, endpointAddress);
                    var secsGemServiceProxy = new SecsGemServiceDirectProxy(context, serviceEndpoint);
                    var duplex = new DuplexChannelFactory<ISecsGemService>(context, binding, endpointAddress);
                    var callbackObj = duplex.CreateChannel(context);

                    GemProcessor.SetSecsGemServiceProxy(secsGemServiceProxy);
                    GemProcessor.SetSecsGemServiceCallback(callbackObj);

                    try
                    {
                        callbackObj.ServerConnect();
                        secsGemService = callbackObj;
                        if (GemProcessor.InitSecsGem(this.GEMModule().GemSysParam.ConfigPath.Value))
                        {
                            if (GemProcessor.Start() == 0)
                            {
                                //CommEnable(); //Establish
                                //if (이게 Gem 동글에 붙으면!)
                                this.SecsCommInformData.GemProcessVersion = Get_GEMProcessVersion(); //XGem Version
                                resultInit = true;

                                SetSecsMessageReceiver();

                                while(true)
                                {
                                    if(SecsCommInformData.GemState == SecsGemStateEnum.EXECUTE)
                                    {
                                        break;
                                    }
                                    System.Threading.Thread.Sleep(1);
                                }
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"[{this.GetType().Name}] ConnectSuccess() - Failed GEM Init");
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Debug($"[{this.GetType().Name}] ConnectSuccess() - Failed GEM Init (Exception : {err}");
                        GemProcessor.SetSecsGemServiceProxy(null);
                    }
                }
                else
                {
                    LoggerManager.Debug("[SECS/GEM] Gem Service Proxy is already opened. Connect to Gem Service is failed");
                }

                if (resultInit == true)
                {
                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    throw new Exception($"[{this.GetType().Name}] InitGemService() Error");
                }
            }
            catch (Exception err)
            {
                throw err;
            }

            return retval;
        }

        public EventCodeEnum SetSecsMessageReceiver()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                long ret = secsGemService.LoadMessageRecevieModule(this.GEMModule().GemSysParam.GemMessageReceiveModulePath, this.GEMModule().GemSysParam.ReceiveMessageType);
                if (ret == -1)
                {
                    retVal = EventCodeEnum.NONE;
                    LoggerManager.Debug("[SECS/GEM] Load Message Receive Module Fail");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public ISecsGemService GetSecsGemServiceModule()
        {
            return secsGemService;
        }

        public EventCodeEnum SetRemoteActionRecipe(Dictionary<string, IGemActBehavior> recipeparam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //LoadDll
                //GemProcessor.SetRemoteActionRecipe(recipeparam);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        //public EventCodeEnum End

        #region <remarks> Set GEM Process Version </remarks>

        /* -----------------------------------------------------------
         * XGem Version
         ----------------------------------------------------------- */
        private string Get_GEMProcessVersion()
        {
            string version_info = "0.0.0.0";

            try
            {
                FileInfo file_64 = new FileInfo(@"C:\Program Files (x86)\Linkgenesis\XGem v3.x\SE\Bin\XGem.exe");
                if (file_64.Exists)
                {
                    var versionInfo = FileVersionInfo.GetVersionInfo(@"C:\Program Files (x86)\Linkgenesis\XGem v3.x\SE\Bin\XGem.exe");
                    version_info = versionInfo.ProductVersion;

                    LoggerManager.Debug($"[{this.GetType().Name}] GEM ProductVersion Version : {version_info}");
                }
                else
                {
                    FileInfo file_32 = new FileInfo(@"C:\Program Files\Linkgenesis\XGem v3.x\SE\Bin\XGem.exe");

                    if (file_32.Exists)
                    {
                        var versionInfo = FileVersionInfo.GetVersionInfo(@"C:\Program Files\Linkgenesis\XGem v3.x\SE\Bin\XGem.exe");
                        version_info = versionInfo.ProductVersion;
                        LoggerManager.Debug($"[{this.GetType().Name}] GEM ProductVersion Version : {version_info}");
                    }
                    else
                    {
                        //this._loggerCommon.WriteLog4Net(logType.Error, string.Format("XGem not install...."));
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            finally
            {
            }

            return version_info;
        }
        #endregion

        public long CommEnable()
        {
            long retVal = -1;
            try
            {
                retVal = GemProcessor.Proc_SetEstablish((long)SecsEnum_Enable.ENABLE);

                if (retVal == 0)
                {
                    //success
                    this.SecsCommInformData.Enable = SecsEnum_Enable.ENABLE;
                    LoggerManager.Debug($"[{this.GetType().Name}] CommEnable(), Code : {retVal}");
                }
                else
                {
                    //fail
                    //SecsCommInformData.Enable = SecsEnum_Enable.UNKNOWN;
                    LoggerManager.Debug($"[{this.GetType().Name}] Fail CommEnable(), Code : {retVal}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public long CommDisable()
        {
            long retVal = -1;
            try
            {
                retVal = GemProcessor.Proc_SetEstablish((long)SecsEnum_Enable.DISABLE);

                if (retVal == 0)
                {
                    //success
                    this.SecsCommInformData.Enable = SecsEnum_Enable.DISABLE;
                    LoggerManager.Debug($"[{this.GetType().Name}] CommDisable()");
                }
                else
                {
                    //fail
                    //SecsCommInformData.Enable = SecsEnum_Enable.UNKNOWN;
                    LoggerManager.Debug($"[{this.GetType().Name}] Fail CommDisable(), Code : {retVal}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public long ReqOffLine()
        {
            long retVal = -1;
            try
            {
                retVal = GemProcessor.Proc_ReqOffline();

                if (retVal == 0)
                {
                    //success
                    this.SecsCommInformData.ControlState = SecsEnum_ControlState.EQ_OFFLINE;
                    LoggerManager.Debug($"[{this.GetType().Name}] ReqOffLine(), Code : {retVal}");
                }
                else
                {
                    //fail
                    //SecsCommInformData.ControlState = SecsEnum_ControlState.UNKNOWN;
                    LoggerManager.Debug($"[{this.GetType().Name}] Fail ReqOffLine(), Code : {retVal}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public long ReqLocal()
        {
            long retVal = -1;
            try
            {

                retVal = GemProcessor.Proc_ReqLocal();

                if (retVal == 0)
                {
                    //success
                    this.SecsCommInformData.ControlState = SecsEnum_ControlState.ONLINE_LOCAL;
                    LoggerManager.Debug($"[{this.GetType().Name}] ReqLocal()");
                }
                else
                {
                    //fail
                    //SecsCommInformData.ControlState = SecsEnum_ControlState.UNKNOWN;
                    LoggerManager.Debug($"[{this.GetType().Name}] Fail ReqLocal(), Code : {retVal}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public long ReqRemote()
        {
            long retVal = -1;
            try
            {
                retVal = GemProcessor.Proc_ReqRemote();

                if (retVal == 0)
                {
                    //success
                    this.SecsCommInformData.ControlState = SecsEnum_ControlState.ONLINE_REMOTE;
                    LoggerManager.Debug($"[{this.GetType().Name}] ReqRemote()");
                    this.GEMModule().GetPIVContainer().ChangededControlStateEvent();
                }
                else
                {
                    //fail
                    //SecsCommInformData.ControlState = SecsEnum_ControlState.UNKNOWN;
                    LoggerManager.Debug($"[{this.GetType().Name}] Fail ReqRemote(), Code : {retVal}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public long SetInitControlState_Offline()
        {
            long retVal = -1;
            try
            {
                //3514 => 1(offline), 2(online)
                //3514에서 2일경우 3511의 값이 유효함.
                long[] vids = new long[] { 3514 };
                string[] vidValue = new string[] { ((long)SecsEnum_ON_OFFLINEState.OFFLINE).ToString() };

                ////////////////////////////////////////

                retVal = GemProcessor.Proc_SetECVChanged(vids.Length, vids, vidValue);

                if (retVal == 0)
                {
                    this.SecsCommInformData.InitControlState = SecsEnum_ON_OFFLINEState.OFFLINE;

                    LoggerManager.Debug($"[{this.GetType().Name}] SetInitControlState_Offline()");
                }
                else
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] Fail SetInitControlState_Offline(), Code : {retVal}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public long SetInitControlState_Local()
        {
            long retVal = -1;
            try
            {
                long[] vids = new long[] { 3514, 3511 };
                string[] vidValue = new string[] { ((long)SecsEnum_ON_OFFLINEState.ONLINE).ToString(), ((long)SecsEnum_OnlineSubState.LOCAL).ToString() };

                retVal = GemProcessor.Proc_SetECVChanged(vids.Length, vids, vidValue);

                if (retVal == 0)
                {
                    this.SecsCommInformData.InitControlState = SecsEnum_ON_OFFLINEState.ONLINE;
                    this.SecsCommInformData.OnLineSubState = SecsEnum_OnlineSubState.LOCAL;
                    LoggerManager.Debug($"[{this.GetType().Name}] SetInitControlState_Local()");
                }
                else
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] Fail SetInitControlState_Local(), Code : {retVal}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public long SetInitControlState_Remote()
        {
            long retVal = -1;
            try
            {
                long[] vids = new long[] { 3514, 3511 };
                string[] vidValue = new string[] { ((long)SecsEnum_ON_OFFLINEState.ONLINE).ToString(), ((long)SecsEnum_OnlineSubState.REMOTE).ToString() };

                retVal = GemProcessor.Proc_SetECVChanged(vids.Length, vids, vidValue);

                if (retVal == 0)
                {
                    this.SecsCommInformData.InitControlState = SecsEnum_ON_OFFLINEState.ONLINE;
                    this.SecsCommInformData.OnLineSubState = SecsEnum_OnlineSubState.REMOTE;
                    LoggerManager.Debug($"[{this.GetType().Name}] SetInitControlState_Remote()");
                }
                else
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] Fail SetInitControlState_Remote(), Code : {retVal}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public long SetInitProberEstablish()
        {
            long retVal = -1;
            try
            {
                //101  => 0(false), 1(true)
                long[] vids = new long[] { 101 };
                string[] vidValue = new string[] { ((long)SecsEnum_EstablishSource.PROBER).ToString() };

                retVal = GemProcessor.Proc_SetECVChanged(vids.Length, vids, vidValue);

                if (retVal == 0)
                {
                    this.SecsCommInformData.EstablishSource = SecsEnum_EstablishSource.PROBER;
                    LoggerManager.Debug($"[{this.GetType().Name}] SetInitProberEstablish()");
                }
                else
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] Fail SetInitProberEstablish(), Code : {retVal}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public long SetInitHostEstablish()
        {
            long retVal = -1;
            try
            {
                long[] vids = new long[] { 101 };
                string[] vidValue = new string[] { ((long)SecsEnum_EstablishSource.HOST).ToString() };

                retVal = GemProcessor.Proc_SetECVChanged(vids.Length, vids, vidValue);

                if (retVal == 0)
                {
                    this.SecsCommInformData.EstablishSource = SecsEnum_EstablishSource.HOST;
                    LoggerManager.Debug($"[{this.GetType().Name}] SetInitHostEstablish()");
                }
                else
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] Fail SetInitHostEstablish(), Code : {retVal}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public long SetInitCommunicationState_Enable()
        {
            long retVal = -1;
            try
            {
                //130  => 0(disable), 1(enable)
                long[] vids = new long[] { 130 };
                string[] vidValue = new string[] { ((long)SecsEnum_Enable.ENABLE).ToString() };

                retVal = GemProcessor.Proc_SetECVChanged(vids.Length, vids, vidValue);

                if (retVal == 0)
                {
                    this.SecsCommInformData.DefaultCommState = SecsEnum_Enable.ENABLE;
                    LoggerManager.Debug($"[{this.GetType().Name}] SetInitCommunicationState_Enable()");
                }
                else
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] Fail SetInitCommunicationState_Enable(), Code : {retVal}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public long SetInitCommunicationState_Disable()
        {
            long retVal = -1;
            try
            {
                long[] vids = new long[] { 130 };
                string[] vidValue = new string[] { ((long)SecsEnum_Enable.DISABLE).ToString() };

                retVal = GemProcessor.Proc_SetECVChanged(vids.Length, vids, vidValue);

                if (retVal == 0)
                {
                    this.SecsCommInformData.DefaultCommState = SecsEnum_Enable.DISABLE;
                    LoggerManager.Debug($"[{this.GetType().Name}] SetInitCommunicationState_Disable()");
                }
                else
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] Fail SetInitCommunicationState_Disable(), Code : {retVal}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public long TimeRequest()
        {
            long funcRetVal = -1;
            try
            {
                funcRetVal = GemProcessor.Proc_TimeRequest();

                if (funcRetVal == 0)
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] Request Time");
                }
                else
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] Fail Request Time, Code : {funcRetVal}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return funcRetVal;
        }

        public long SendTerminal(string sendStr)
        {
            long funcRetVal = -1;
            try
            {
                funcRetVal = GemProcessor.Proc_SendTerminal(sendStr);

                if (funcRetVal == 0)
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] SendTerminal() : {sendStr}");
                }
                else
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] Fail SendTerminal() : {sendStr}, Code : {funcRetVal}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return funcRetVal;
        }
        
        public long SetEvent(long eventNum)
        {
            try
            {
                return GemProcessor?.Proc_SetEvent(eventNum) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return -1;
            }
        }
        public long SetAlarm(long nID, long nState, int cellIndex = 0)
        {
            try
            {
                return GemProcessor?.Proc_SetAlarm(nID, nState, cellIndex) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return -1;
            }
        }


        public EventCodeEnum ClearAlarmOnly(int cellIndex = 0)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                ret = GemProcessor?.Proc_ClearAlarmOnly(cellIndex) ?? ret;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        //public long SetEvent(INotifyEvent notifyevent)
        //{
        //    return -1;
        //}
        public void CommunicationParamApply()
        {
            try
            {
                GemProcessor?.Proc_SetParamInfoToGem();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void DeInitModule()
        {
        }

        public void Dispose()
        {
            try
            {
                Gemprocess?.Dispose();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public long GEMSetVariable(int vidLength, long[] convertDataID, string[] values)
        {
            long retVal = -1;
            try
            {
                retVal = GemProcessor?.Proc_SetVariable(vidLength, convertDataID, values) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = -1;
            }
            return retVal;
        }

        public long GEMSetECVChanged(int vidLength, long[] convertDataID, string[] values)
        {
            long retVal = -1;
            try
            {
                retVal = GemProcessor?.Proc_SetECVChanged(vidLength, convertDataID, values) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = -1;
            }
            return retVal;
        }

        /// <summary>
        /// GEM 동글에 변경된 값을 Set합니다.
        /// </summary>
        /// <param name="vids"></param>
        /// <param name="values"></param>
        /// <param name="vidType"></param>
        public void SetVariable(long[] vids, string[] values, EnumVidType vidType, bool immediatelyUpdate = false)
        {
            long setResult = -1;
            int vidLength = -1;
            int valueLength = -1;
            long[] convertDataID = GetOrgDataID(vids);

            try
            {
                vidLength = convertDataID?.Length ?? 0;
                valueLength = values?.Length ?? 0;

                if (vidLength != 0 && vidLength == valueLength)
                {
                    var conbindLogData = from id in convertDataID
                                         from val in values
                                         select "[" + id + ", " + val + "],";

                    if (vidType == EnumVidType.SVID ||
                        vidType == EnumVidType.DVID ||
                        vidType == EnumVidType.ECID ||
                        vidType == EnumVidType.NONE)
                    {
                        setResult = GemProcessor.Proc_SetVariable(vidLength, convertDataID, values, immediatelyUpdate);
                    }
                    else if (vidType == EnumVidType.ECID)
                    {
                        // Element.SetValue로 이사감 
                        //setResult = GemProcessor.Proc_SetECVChanged(vidLength, convertDataID, values);// AfterValueChanged에서 해주는 일.
                    }

                    if (setResult == 0)
                    {
                        //LoggerManager.Debug($"[{this.GetType()}] Success SetVariable({vidType}) : {conbindLogData}");
                    }
                    else
                    {                       
                        //test code//
                        //LoggerManager.Debug($"[{this.GetType()}] Fail SetVariable({vidType}) : {conbindLogData.First()}");
                    }
                }
                else
                {
                    LoggerManager.Debug($"[{this.GetType()}] Fail SetVariable({vidType}). Wrong Vid, Value Length.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[{this.GetType()}] Exception on SetVariable({vidType}). [{string.Join(",", vids)}], [{string.Join(",", values)}]");
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// GEM 동글에 변경된 값을 Set합니다.
        /// </summary>
        /// <param name="vids"></param>
        /// <param name="values"></param>
        /// <param name="vidType"></param>
        public void SetVariables(long nObjectID, long nVid, EnumVidType vidType, bool immediatelyUpdate = false)
        {
            long setResult = -1;
            int vidLength = -1;            
            long[] convertDataID = GetOrgDataID(nVid);

            try
            {
                vidLength = convertDataID?.Length ?? 0;
                //valueLength = nObjectID?.Length ?? 0;

                if (vidLength != 0)//&& vidLength == valueLength)
                {
                    var conbindLogData = "[" + convertDataID + ", " + nObjectID + "]";

                    if (vidType == EnumVidType.SVID ||
                        vidType == EnumVidType.DVID ||
                        vidType == EnumVidType.NONE)
                    {                        
                        setResult = GemProcessor.Proc_SetVariables(nObjectID, nVid, immediatelyUpdate);
                        //LoggerManager.Debug($"Proc_SetVariables :{nVid}");
                    }
                    else if (vidType == EnumVidType.ECID)// ECID는 List 타입이 없음.
                    {
                        //setResult = GemProcessor.Proc_SetECVChangeds(nObjectID, nVid, immediatelyUpdate);
                    }

                    if (setResult == 0)
                    {
                        //LoggerManager.Debug($"[{this.GetType()}] Success SetVariable({vidType}) : {conbindLogData}");
                    }
                    else
                    {
                        LoggerManager.Debug($"[{this.GetType()}] Fail SetVariables({vidType}) : {conbindLogData.First()}");
                    }
                }
                else
                {
                    LoggerManager.Debug($"[{this.GetType()}] Fail SetVariables({vidType}). Wrong Vid, Value Length.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[{this.GetType()}] Exception on SetVariables({vidType}) nVid[{nVid}], [{nObjectID}]");
                LoggerManager.Exception(err);
            }
        }

        public long[] GetOrgDataID(params long[] gemIDArray)
        {
            long[] returnDataArray = new long[] { -1 };

            if (gemIDArray != null)
            {
                //int absIndex = this.StageSupervisor()?.StageAbsoluteIndex ?? 0;
                int absIndex = 0;
                returnDataArray = new long[gemIDArray.Length];

                for (int i = 0; i < gemIDArray.Length; i++)
                {
                    returnDataArray[i] = gemIDArray[i] + (absIndex * this.STAGE_ID_SECTION);
                }
            }

            return returnDataArray;
        }

        public void ProcessClose()
        {
            try
            {
                Process[] findProcess = null;
                this.serviceProcess?.Close();

                findProcess = Process.GetProcessesByName("SecsGemServiceHostApp");

                if (0 < findProcess.Length)
                {
                    foreach (var process in findProcess)
                    {
                        process.Kill();
                        Task.Delay(1000).Wait();
                        LoggerManager.Debug("[SECS/GEM] SecsGemServiceHostApp Program is Exit");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool GetRemoteConnectState(int stageIndex = -1)
        {
            bool retVal = false;
            try
            {
                retVal = GemProcessor.GetRemoteConnectState(stageIndex);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void OnRemoteCommandAction(RemoteActReqData msgData)
        {
            GemProcessor.OnRemoteCommandAction(msgData);
        }

        public void OnCarrierActMsgRecive(CarrierActReqData msgData)
        {
            try
            {
                if(GemProcessor != null)
                {
                    if (GemProcessor is ISecsGemServiceCallback)
                    {
                        (GemProcessor as ISecsGemServiceCallback)?.OnCarrierActMsgRecive(msgData);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ECVChangeMsgReceive(EquipmentReqData msgData)
        {
            try
            {
                if (GemProcessor != null)
                {
                    if (GemProcessor is ISecsGemServiceCallback)
                    {
                        (GemProcessor as ISecsGemServiceCallback)?.ECVChangeMsgReceive(msgData);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ECVChangeMsgReceive(long nMsgId, long nCount, long[] pnEcids, string[] psVals)
        {
            try
            {
                if (GemProcessor != null)
                {
                    if (GemProcessor is ISecsGemServiceCallback)
                    {
                        (GemProcessor as ISecsGemServiceCallback)?.OnGEMReqChangeECV(nMsgId, nCount, pnEcids, psVals);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public long SendAck(long pnObjectID, long nStream, long nFunction, long nSysbyte, byte CAACK, long nCount)
        {
            long retVal = -1;
            try
            {
                retVal = GemProcessor?.SendAck(pnObjectID, nStream, nFunction, nSysbyte, CAACK, nCount) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        public long S3F17SendAck(long pnObjectID, long nStream, long nFunction, long nSysbyte, byte CAACK, long nCount)
        {
            long retVal = -1;
            try
            {
                retVal = GemProcessor?.S3F17SendAck(pnObjectID, nStream, nFunction, nSysbyte, CAACK, nCount) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }


        public long S3F27SendAck(long pnObjectID, long nStream, long nFunction, long nSysbyte, byte CAACK, long nCount, List<CarrierChangeAccessModeResult> result)
        {
            long retVal = -1;
            try
            {
                retVal = GemProcessor?.S3F27SendAck(pnObjectID, nStream, nFunction, nSysbyte, CAACK, nCount, result) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public long MakeListObject(object value)
        {
            long ret = -1;
            try
            {
                ret = GemProcessor?.MakeListObject(value) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public bool GetConnectState(int index = 0)
        {
            bool retVal = false;
            try
            {
                retVal = GemProcessor.GetConnectState(index);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public object GetProcessorLockObj()
        {
            object lockObj = null;
            try
            {
                if(GemProcessor != null)
                {
                    lockObj = GemProcessor.lockObj;
                    if(lockObj == null)
                    {
                        lockObj = new object();
                        LoggerManager.Debug("XGemCommManager - GetProcessorLockObj() : Processor LockObj is null.");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return lockObj;
        }

        public object lockObj = new object();
        public object GetLockObj()
        {
            try
            {
                if (lockObj == null)
                {
                    lockObj = new object();
                    LoggerManager.Debug("XGemCommManager - GetLockObj() :  LockObj is null.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return lockObj;
        }
    }
}
