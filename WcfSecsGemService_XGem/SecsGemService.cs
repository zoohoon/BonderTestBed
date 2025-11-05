using GEM_XGem;
using GEM_XGem300Pro;
using SecsGemServiceInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Timers;
using XGEMWrapper;

namespace WcfSecsGemService_XGem
{
    //[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class SecsGemService : ISecsGemService, IDisposable
    {
        //private XGemNet m_XGem = null;
        private ISecsGemServiceCallback callback = null;
        //private static XGemNet m_XGem = null;
        //private static ISecsGemCallbackServiceCallback callback;

        private Object LockObj = new object();
        //private Queue<Action> callbackQueue;
        //private Thread ServiceThread = null;

        private SecsGemStateEnum SECSGemState;
        private SecsCommStateEnum SECSCommState;
        private SecsControlStateEnum SECSControlState;
        //private bool threadRun;

        private System.Timers.Timer timer;
        private object callbackLocker = new object();

        ISecsGemMessageReceiver MessageReceiver { get; set; }
        private XGEM Gem { get; set; }

        List<RPTID> rPTIDs = new List<RPTID>();
        List<CEID> cEIDs = new List<CEID>();
        public SecsGemService(string StartupGemType)
        {
            if (Gem == null)
            {
                if (StartupGemType.Equals("XGEM"))
                {
                    XGemServiceInitSetting();
                }
                else if(StartupGemType.Equals("XGEM300"))
                {
                    XGem300ProServiceInitSetting();
                }
                else
                {
                    //NONE.
                }              
            }

            timer = new System.Timers.Timer();
            timer.Interval = 1000;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            timer.Start();

            // <remarks> Direct Excute Code 
            //long ret = 0;
            //ret = Init_SECSGEM("C:\\ProberSystem\\Utility\\GEM\\Config\\GEM_PROCESS.cfg");
            //ret = Start();
            //GEMSetVariable(1, new long[1] { 1200 }, new string[1] { "SOFTREV" });
            //GEMSetVariable(1, new long[1] { 1201 }, new string[1] { "1.0.1.14" });
            //ret = GEMEQInitialized(1);
            //ret = GEMReqAllECInfo();
            // </remarks> 


            //threadRun = true;
            //ThreadStart ts = new ThreadStart(LoopThread);
            //ServiceThread = new Thread(ts);
            //ServiceThread.Start();

            ////callback?.CallBack_ConnectSuccess();
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (callbackLocker)
            {
                try
                {
                    bool? r = callback?.Are_You_There();
                }
                catch
                {
                    callback = null;
                }
            }
        }

        private void XGemServiceInitSetting()
        {
            Gem = null;
            var m_XGem = new XGemNet();
            Gem = new XGEM(m_XGem);
            Gem.OnGEMCommStateChanged += new XGEMWrapper.OnGEMCommStateChanged(XGEM_OnGEMCommStateChange);
            Gem.OnXGEMStateEvent += new XGEMWrapper.OnXGEMStateEvent(XGEM_OnXGEMStateEvent);
            Gem.OnSECSMessageReceived += new XGEMWrapper.OnSECSMessageReceived(XGEM_OnSECSMessageReceive);
            Gem.OnGEMControlStateChanged += new XGEMWrapper.OnGEMControlStateChanged(XGEM_OnGEMControlStateChange);
            Gem.OnGEMRspGetDateTime += new XGEMWrapper.OnGEMRspGetDateTime(XGEM_OnGEMRspGetDateTime);
            Gem.OnGEMReqDateTime += new XGEMWrapper.OnGEMReqDateTime(XGEM_OnGEMReqDateTime);

            Gem.OnGEMTerminalMessage += new XGEMWrapper.OnGEMTerminalMessage(XGEM_OnGEMTerminalMessage);
            Gem.OnGEMTerminalMultiMessage += new XGEMWrapper.OnGEMTerminalMultiMessage(XGEM_OnGEMTerminalMultiMessage);

            Gem.OnGEMReqRemoteCommand += new XGEMWrapper.OnGEMReqRemoteCommand(XGEM_OnGEMReqRemoteCommand);
            Gem.OnGEMReqPPList += new XGEMWrapper.OnGEMReqPPList(XGEM_OnGEMReqPPList);

            ////////////////////////////////////////////////////
            Gem.OnGEMECVChanged += new XGEMWrapper.OnGEMECVChanged(XGEM_OnGEMECVChanged);
            Gem.OnGEMReqChangeECV += new XGEMWrapper.OnGEMReqChangeECV(XGEM_OnGEMReqChangeECV);
            Gem.OnGEMReqGetDateTime += new XGEMWrapper.OnGEMReqGetDateTime(XGEM_OnGEMReqGetDateTime);
            Gem.OnGEMErrorEvent += new XGEMWrapper.OnGEMErrorEvent(XGEM_OnGEMErrorEvent);
            Gem.OnGEMReqPPLoadInquire += new XGEMWrapper.OnGEMReqPPLoadInquire(XGEM_OnGEMReqPPLoadInquire);
            Gem.OnGEMReqPPEx += new XGEMWrapper.OnGEMReqPPEx(XGEM_OnGEMReqPPEx);
            Gem.OnGEMReqPPDelete += new XGEMWrapper.OnGEMReqPPDelete(XGEM_OnGEMReqPPDelete);
            Gem.OnGEMRspAllECInfo += new XGEMWrapper.OnGEMRspAllECInfo(XGEM_OnGEMRspAllECInfo);

            Gem.OnGEMReqPPSendEx += new XGEMWrapper.OnGEMReqPPSendEx(XGEM_OnGEMReqPPSendEx);

            SECSGemState = SecsGemStateEnum.UNKNOWN;
            SECSCommState = SecsCommStateEnum.UNKNOWN;
            SECSControlState = SecsControlStateEnum.UNKNOWN;
        }

        private void XGem300ProServiceInitSetting()
        {
            try
            {
                var m_XGem = new XGem300ProNet();
                Gem = new XGEM(m_XGem); 
                Gem.OnGEMCommStateChanged += new XGEMWrapper.OnGEMCommStateChanged(XGEM_OnGEMCommStateChange);
                Gem.OnXGEMStateEvent += new XGEMWrapper.OnXGEMStateEvent(XGEM_OnXGEMStateEvent);
                Gem.OnSECSMessageReceived += new XGEMWrapper.OnSECSMessageReceived(XGEM_OnSECSMessageReceive);
                Gem.OnGEMControlStateChanged += new XGEMWrapper.OnGEMControlStateChanged(XGEM_OnGEMControlStateChange);
                Gem.OnGEMRspGetDateTime += new XGEMWrapper.OnGEMRspGetDateTime(XGEM_OnGEMRspGetDateTime);
                Gem.OnGEMReqDateTime += new XGEMWrapper.OnGEMReqDateTime(XGEM_OnGEMReqDateTime);

                Gem.OnGEMTerminalMessage += new XGEMWrapper.OnGEMTerminalMessage(XGEM_OnGEMTerminalMessage);
                Gem.OnGEMTerminalMultiMessage += new XGEMWrapper.OnGEMTerminalMultiMessage(XGEM_OnGEMTerminalMultiMessage);
                Gem.OnGEMReqRemoteCommand += new XGEMWrapper.OnGEMReqRemoteCommand(XGEM_OnGEMReqRemoteCommand);
                Gem.OnGEMReqPPList += new XGEMWrapper.OnGEMReqPPList(XGEM_OnGEMReqPPList);

                ////////////////////////////////////////////////////
                Gem.OnGEMECVChanged += new XGEMWrapper.OnGEMECVChanged(XGEM_OnGEMECVChanged);
                Gem.OnGEMReqChangeECV += new XGEMWrapper.OnGEMReqChangeECV(XGEM_OnGEMReqChangeECV);
                Gem.OnGEMReqGetDateTime += new XGEMWrapper.OnGEMReqGetDateTime(XGEM_OnGEMReqGetDateTime);
                Gem.OnGEMErrorEvent += new XGEMWrapper.OnGEMErrorEvent(XGEM_OnGEMErrorEvent);
                Gem.OnGEMReqPPLoadInquire += new XGEMWrapper.OnGEMReqPPLoadInquire(XGEM_OnGEMReqPPLoadInquire);
                Gem.OnGEMReqPPEx += new XGEMWrapper.OnGEMReqPPEx(XGEM_OnGEMReqPPEx);
                Gem.OnGEMReqPPDelete += new XGEMWrapper.OnGEMReqPPDelete(XGEM_OnGEMReqPPDelete);
                Gem.OnGEMRspAllECInfo += new XGEMWrapper.OnGEMRspAllECInfo(XGEM_OnGEMRspAllECInfo);

                Gem.OnGEMReqPPSendEx += new XGEMWrapper.OnGEMReqPPSendEx(XGEM_OnGEMReqPPSendEx);
            }
            catch (Exception)
            {
                //catch
            }
        }

        private void XGEM_OnGEMReqPPEx(long nMsgId, string sPpid, string sRecipePath)
        {
            try
            {
                lock (callbackLocker)
                {
                    callback?.OnGEMReqPPEx(nMsgId, sPpid, sRecipePath);

                    timer.Stop();
                    timer.Start();
                }
            }
            catch (Exception err)
            {
                WriteLog(err.Message);
            }
        }

        private void XGEM_OnGEMReqPPSendEx(long nMsgId, string sPpid, string sRecipePath)
        {
            try
            {
                lock (callbackLocker)
                {
                    callback?.OnGEMReqPPSendEx(nMsgId, sPpid, sRecipePath);

                    timer.Stop();
                    timer.Start();
                }
            }
            catch (Exception err)
            {
                WriteLog(err.Message);
            }
        }

        private void XGEM_OnGEMRspAllECInfo(long lCount, long[] plVid, string[] psName, string[] psValue, string[] psDefault, string[] psMin, string[] psMax, string[] psUnit)
        {
            try
            {
                lock (callbackLocker)
                {
                    callback?.OnGEMRspAllECInfo(lCount, plVid, psName, psValue, psDefault, psMin, psMax, psUnit);

                    timer.Stop();
                    timer.Start();
                }
            }
            catch (Exception err)
            {
                WriteLog(err.Message);
            }
        }

        private void XGEM_OnGEMReqPPDelete(long nMsgId, long nCount, string[] psPpid)
        {
            try
            {
                lock (callbackLocker)
                {
                    callback?.OnGEMReqPPDelete(nMsgId, nCount, psPpid);

                    timer.Stop();
                    timer.Start();
                }
            }
            catch (Exception err)
            {
                WriteLog(err.Message);
            }
        }

        private void XGEM_OnGEMReqPPLoadInquire(long nMsgId, string sPpid, long nLength)
        {
            try
            {
                lock (callbackLocker)
                {
                    this.Gem.GEMRspPPLoadInquire(nMsgId, sPpid, 0);

                    timer.Stop();
                    timer.Start();
                }
            }
            catch(Exception err)
            {
                WriteLog(err.Message);
            }
        }

        private void XGEM_OnGEMErrorEvent(string sErrorName, long nErrorCode)
        {
            try
            {
                lock (callbackLocker)
                {
                    callback?.OnGEMErrorEvent(sErrorName, nErrorCode);

                    timer.Stop();
                    timer.Start();
                }
            }
            catch (Exception err)
            {
                WriteLog(err.Message);
            }
        }

        private void XGEM_OnGEMReqGetDateTime(long nMsgId)
        {
            try
            {
                lock (callbackLocker)
                {
                    string systemTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                    long ret = this.Gem.GEMRspGetDateTime(nMsgId, systemTime);

                    timer.Stop();
                    timer.Start();
                }
            }
            catch (Exception err)
            {
                WriteLog(err.Message);
            }
        }

        private void XGEM_OnGEMReqChangeECV(long nMsgId, long nCount, long[] pnEcids, string[] psVals)
        {
            try
            {
                lock (callbackLocker)
                {
                    callback?.OnGEMReqChangeECV(nMsgId, nCount, pnEcids, psVals);

                    timer.Stop();
                    timer.Start();
                }
            }
            catch (Exception err)
            {
                WriteLog(err.Message);
            }
        }

        private void XGEM_OnGEMECVChanged(long nCount, long[] pnEcids, string[] psVals)
        {
            try
            {
                lock (callbackLocker)
                {
                    callback?.OnGEMECVChanged(nCount, pnEcids, psVals);

                    timer.Stop();
                    timer.Start();
                }
            }
            catch (Exception err)
            {
                WriteLog(err.Message);
            }
        }

        private void XGEM_OnGEMReqPPList(long nMsgId)
        {
            try
            {
                lock (callbackLocker)
                {
                    callback?.OnGEMReqPPList(nMsgId);
                    timer.Stop();
                    timer.Start();
                }
            }
            catch (Exception err)
            {
                WriteLog(err.Message);
            }
        }

        private void XGEM_OnGEMReqRemoteCommand(long nMsgId, string sRcmd, long nCount, string[] psNames, string[] psVals)
        {
            try
            {
                lock (callbackLocker)
                {
                    EnumRemoteCommand Rcmd = EnumRemoteCommand.UNDEFINE;

                    bool parseResult = Enum.TryParse(sRcmd, out Rcmd);

                if (parseResult != false)
                {
                    callback?.OnGEMReqRemoteCommand(nMsgId, Rcmd, nCount, psNames, psVals);
                }

                    timer.Stop();
                    timer.Start();
                }
            }
            catch (Exception err)
            {
                WriteLog(err.Message);
            }
        }

        private void XGEM_OnGEMReqDateTime(long nMsgId, string sSystemTime)
        {
            try
            {
                lock (callbackLocker)
                {
                    callback?.OnGEMReqDateTime(nMsgId, sSystemTime);
                    timer.Stop();
                    timer.Start();
                }
            }
            catch (Exception err)
            {
                WriteLog(err.Message);
            }
            //m_XGem.GEMRspDateTime(nMsgId, 0);
        }

        private void XGEM_OnGEMTerminalMultiMessage(long nTid, long nCount, string[] psMsg)
        {
            try
            {
                lock (callbackLocker)
                {
                    callback?.OnGEMTerminalMultiMessage(nTid, nCount, psMsg);
                    timer.Stop();
                    timer.Start();
                }
            }
            catch (Exception err)
            {
                WriteLog(err.Message);
            }
        }

        private void XGEM_OnGEMTerminalMessage(long nTid, string sMsg)
        {
            try
            {
                lock (callbackLocker)
                {
                    callback?.OnGEMTerminalMessage(nTid, sMsg);
                    timer.Stop();
                    timer.Start();
                }
            }
            catch (Exception err)
            {
                WriteLog(err.Message);
            }
        }

        private void XGEM_OnGEMRspGetDateTime(string sSystemTime)
        {
            try
            {
                lock (callbackLocker)
                {
                    callback?.OnGEMRspGetDateTime(sSystemTime);
                    timer.Stop();
                    timer.Start();
                }
            }
            catch (Exception err)
            {
                WriteLog(err.Message);
            }
        }

        //private void XGEM_OnGEMECVChanged(long nCount, long[] pnEcids, string[] psVals)
        //{

        //}

        //public void LoopThread()
        //{
        //    while(threadRun)
        //    {
        //        lock(LockObj)
        //        {
        //            if (0 < callbackQueue.Count)
        //            {
        //                var func = callbackQueue.Dequeue();
        //                func.Invoke();
        //            }
        //        }
        //        Thread.Sleep(1);
        //    }
        //}

        public void ServerConnect(int proberId = 0)
        {
            try
            {
                callback = OperationContext.Current.GetCallbackChannel<ISecsGemServiceCallback>();
            }
            catch (Exception err)
            {
                WriteLog(err.Message);
            }
            //callback.Are_You_There();
        }

        //public bool ServerConnect()
        //{
        //    callback = OperationContext.Current.GetCallbackChannel<ISecsGemServiceCallback>();
        //    return callback?.CallBack_ConnectSuccess();
        //    //단순히 객체를 만들기 위한 함수.
        //}


        public void Dispose()
        {
            if (Gem != null)
            {
                //callback?.
                //callback?.CallBack_Close();
                //threadRun = false;

                Gem.OnGEMCommStateChanged -= XGEM_OnGEMCommStateChange;
                Gem.OnXGEMStateEvent -= XGEM_OnXGEMStateEvent;
                Gem.OnSECSMessageReceived -= XGEM_OnSECSMessageReceive;
                Gem.OnGEMControlStateChanged -= XGEM_OnGEMControlStateChange;

                Gem.OnGEMRspGetDateTime -= XGEM_OnGEMRspGetDateTime;
                Gem.OnGEMReqDateTime -= XGEM_OnGEMReqDateTime;

                Gem.OnGEMTerminalMessage -= XGEM_OnGEMTerminalMessage;
                Gem.OnGEMTerminalMultiMessage -= XGEM_OnGEMTerminalMultiMessage;

                Gem.OnGEMReqRemoteCommand -= XGEM_OnGEMReqRemoteCommand;
                Gem.OnGEMReqPPList -= XGEM_OnGEMReqPPList;
               
                Gem.Stop();
                Gem.Close();
                Gem.Dispose();
            }
        }

        public long Init_SECSGEM(string Config)
        {
            long initVal = 0;
            try
            {
                initVal = Gem.Initialize(Config);
                if (initVal == -10001)
                {
                    Dispose();
                    if (Gem.XGemMode.Equals(XGEM.MODE.XGEM))
                    {
                        XGemServiceInitSetting();
                    }
                    else if (Gem.XGemMode.Equals(XGEM.MODE.XGEM300Pro))
                    {
                        XGem300ProServiceInitSetting();
                    }
                    else
                    {
                        //NONE.
                    }                  
                    initVal = Gem.Initialize(Config);
                }
            }
            catch (Exception err)
            {
                WriteLog(err.Message);
            }
            return initVal;
        }

        public long LoadMessageRecevieModule(string dllpath, string receivername)
        {
            long retVal = -1;
            try
            {
                string LoadDLLPath = dllpath;
                if (File.Exists(LoadDLLPath) == true)
                {
                    var assembly = Assembly.LoadFrom(LoadDLLPath);
                    if (!assembly.IsDynamic)
                    {
                        foreach (Type type in assembly.GetExportedTypes())
                        {
                            if (type.Name == receivername)
                            {
                                MessageReceiver = (ISecsGemMessageReceiver)Activator.CreateInstance(type);
                                MessageReceiver.SetXGem(Gem);
                                break;
                            }
                        }
                    }
                    if (MessageReceiver != null)
                    {
                        MessageReceiver.callback = this.callback;
                        retVal = 1;
                    }
                    else
                    {
                        retVal = -1;
                    }
                }
                else
                {
                    //Error
                    retVal = -1;
                }


            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }

        public bool IsOpened()
        {
            bool retVal = false;
            try
            {
                if (Gem != null)
                    retVal = true;
            }
            catch (Exception err)
            {
                WriteLog(err.Message);
            }
            return retVal;
        }
        
        public long Close_SECSGEM(int proberId)
        {
            long initVal = 0;
            try
            {
                initVal = Gem.Close();
            }
            catch
            {
            }
            return initVal;
        }

        public long Start()
        {
            long initRet = 0;
            try
            {
                initRet = Gem.Start();
            }
            catch (Exception err)
            {
                WriteLog(err.Message);
            }
            return initRet;
        }

        public long Stop()
        {
            long initRet = 0;
            try
            {
                initRet = Gem.Stop();
            }
            catch (Exception err)
            {
                WriteLog(err.Message);
            }
            return initRet;
        }

        #region XGemEventHandler 멤버

        private void XGEM_OnGEMControlStateChange(long nState)
        {
            #region ControlState Number 설명
            // -1 : None
            //  1 : Equipment Off-Line
            //  2 : Attempt On-Line
            //  3 : Host Off-Line
            //  4 : Local
            //  5 : Remote
            #endregion
            try
            {
                if (Enum.IsDefined(typeof(SecsControlStateEnum), (int)nState))
                    Enum.TryParse(nState.ToString(), out SECSControlState);
                else
                    SECSControlState = SecsControlStateEnum.UNKNOWN;


                //string line = $"{DateTime.Now}[SecsControlStateEnum : {nState}]";
                //string path = @"C:\Logs\XGEM\SecsControlState.txt";
                //if (!Directory.Exists(Path.GetDirectoryName(path)))
                //{
                //    Directory.CreateDirectory(Path.GetDirectoryName(path));
                //}

                //using (StreamWriter sw = File.AppendText(path))
                //{
                //    sw.WriteLine(line);
                //}

                //callback = OperationContext.Current.GetCallbackChannel<ISecsGemServiceCallback>();
                lock (callbackLocker)
                {
                    callback.OnGEMControlStateChange(SECSControlState);
                }
                //callbackQueue.Enqueue(()=>callback.OnGEMControlStateChange(SECSControlState.ToString()));
            }
            catch (Exception err)
            {
                WriteLog(err.Message);
            }
        }

        /// <summary>
        ///     GEM으로부터 메세지 수신. Remote Msg & Carrier Command 처리.
        /// </summary>
        /// <remarks>
        ///     GEM -> Main.
        ///     S2F21, S2F41, S2F46, S3F17, S3F27
        ///     First Create    :   2017-12-28, Semics R&D1, Jake Kim.
        ///     Modify 01       :   2018-04-26, Semics R&D1, Jake Kim. 
        ///                             클라이언트에 단순히 들어온 메세지만 콜백하는 방법에서 데이터를 전부 받아 콜백하는 방법으로 바꿈.
        ///                         
        /// </remarks>
        /// <param name="nMsgId">    ID              </param>
        /// <param name="nStream">      Stream Number   </param>
        /// <param name="nFunction">    Function Number </param>
        /// <param name="nSysbyte">     Messege Number  </param>
        /// 
        private void XGEM_OnSECSMessageReceive(long nMsgId, long nStream, long nFunction, long nSysbyte)
        {
            try
            {


                if (nStream == 2 && nFunction == 33)
                {
                    XGEM_OnSECSMessageReceive_S2F33(nMsgId, nStream, nFunction, nSysbyte);
                }
                else if (nStream == 2 && nFunction == 35)
                {
                    XGEM_OnSECSMessageReceive_S2F35(nMsgId, nStream, nFunction, nSysbyte);
                }
                else if (nStream == 2 && nFunction == 37)
                {
                    XGEM_OnSECSMessageReceive_S2F37(nMsgId, nStream, nFunction, nSysbyte);
                }
                else if (nStream == 3 && nFunction == 27)
                {
                    XGEM_OnSECSMessageReceive_S3F27(nMsgId, nStream, nFunction, nSysbyte);
                }


                if (MessageReceiver != null)
                {
                    MessageReceiver?.XGEM_OnSECSMessageReceive(nMsgId, nStream, nFunction, nSysbyte);
                }
            }
            catch (Exception)
            {
            }
        }

        #region <remarks> Define Report (S2F33, S2F35) </remarks>

        private void XGEM_OnSECSMessageReceive_S2F33(long nMsgId, long nStream, long nFunction, long nSysbyte)
        {
            try
            {
               
                long list = 0;
                long rptidList = 0;
                uint[] USER_UINT4 = new uint[1];
                uint[] VID = new uint[1];
                uint[] RPTID = new uint[1];
                List<long> rptid = new List<long>();

                Gem.GetList(nMsgId, ref list);
                Gem.GetU4(nMsgId, ref USER_UINT4);
                Gem.GetList(nMsgId, ref list);
                var listcount = list;

                if (listcount > 0)
                {
                    for (int index = 0; index < listcount; index++)
                    {
                        Gem.GetList(nMsgId, ref rptidList);  //RPTID & VID LIST

                        RPTID rptidInfo = new RPTID();
                        Gem.GetU4(nMsgId, ref RPTID); // RPTID
                        rptidInfo.Rptid = RPTID[0];
                        Gem.GetList(nMsgId, ref rptidList); //VID LIST
                        var rptidlistcount = rptidList;

                        for (int jndex = 0; jndex < rptidlistcount; jndex++)
                        {
                            Gem.GetU4(nMsgId, ref VID); // VID
                            rptidInfo.VIDs.Add(VID[0]);
                            if (rPTIDs.Find(rptids => rptids.Rptid == rptidInfo.Rptid) == null)
                                rPTIDs.Add(rptidInfo);

                            //if (DefineReprt.RPTIDs.Find(rptids => rptids.Rptid == rptidInfo.Rptid) == null)
                            //    DefineReprt.RPTIDs.Add(rptidInfo);
                        }

                    }
                }
                else
                {
                    rPTIDs.Clear();
                }

                
            }
            catch (Exception err)
            {
                WriteLog(err.Message);
            }
        }
        private void XGEM_OnSECSMessageReceive_S2F35(long nMsgId, long nStream, long nFunction, long nSysbyte)
        {
            try
            {
               
                long list = 0;
                long rptidList = 0;
                uint[] USER_UINT4 = new uint[1];
                uint[] CEID = new uint[1];
                uint[] RPTID = new uint[1];
                List<long> rptid = new List<long>();

                Gem.GetList(nMsgId, ref list);
                Gem.GetU4(nMsgId, ref USER_UINT4);
                Gem.GetList(nMsgId, ref list);
                var listcount = list;

                if (listcount > 0)
                {
                    for (int index = 0; index < listcount; index++)
                    {
                        Gem.GetList(nMsgId, ref rptidList); // CEID & RPTID LIST

                        CEID ceidInfo = new CEID();
                        Gem.GetU4(nMsgId, ref CEID); // CEID
                        ceidInfo.Ceid = CEID[0];
                        Gem.GetList(nMsgId, ref rptidList); //PRTID LIST
                        var vidlistcount = rptidList;
                        for (int jndex = 0; jndex < vidlistcount; jndex++)
                        {
                            Gem.GetU4(nMsgId, ref RPTID); // RPTID
                            ceidInfo.RPTIDs.Add(RPTID[0]);
                            if (cEIDs.Find(ceids => ceids.Ceid == ceidInfo.Ceid) == null)
                                cEIDs.Add(ceidInfo);
                            //if (DefineReprt.CEIDs.Find(ceids => ceids.Ceid == ceidInfo.Ceid) == null)
                            //    DefineReprt.CEIDs.Add(ceidInfo);
                        }

                    }

                }
                else
                {
                    cEIDs.Clear();
                }
                
            }
            catch (Exception err)
            {
                WriteLog(err.Message);
            }
        }

        private void XGEM_OnSECSMessageReceive_S2F37(long nMsgId, long nStream, long nFunction, long nSysbyte)
        {
            try
            {
                long list = 0;
                bool[] USER_UINT4 = new bool[1];

                Gem.GetList(nMsgId, ref list);
                Gem.GetBool(nMsgId, ref USER_UINT4);
                Gem.GetList(nMsgId, ref list);
                var listcount = list;
                if (listcount > 0)
                {
                    SecsGemDefineReport defineReprt = new SecsGemDefineReport();
                    defineReprt.RPTIDs = rPTIDs;
                    defineReprt.CEIDs = cEIDs;
                    callback.OnDefineReportRecive(defineReprt);
                }
                else
                {
                    SecsGemDefineReport defineReprt = new SecsGemDefineReport();
                    callback.OnDefineReportRecive(defineReprt);
                }
            }
            catch (Exception err)
            {
                WriteLog(err.Message);
            }
        }

        #endregion

        #region <remarks> S2F49 </remarks>

        private void XGEM_OnSECSMessageReceive_S2F49(long nMsgId, long nStream, long nFunction, long nSysbyte)
        {
            try
            {
                
                RemoteActReqData remoteActReqData = null;
                long listCount1 = 0;

                uint[] USER_UINT4 = new uint[1];
                string REMOTE_COMMAND_ACTION = null;
                string REMOTE_COMMAND_ID = null;

                Gem.GetList(nMsgId, ref listCount1);
                Gem.GetU4(nMsgId, ref USER_UINT4);
                Gem.GetAscii(nMsgId, ref REMOTE_COMMAND_ID);
                Gem.GetAscii(nMsgId, ref REMOTE_COMMAND_ACTION);

                if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.PSTART.ToString())
                {
                    var actreqdata = new PStartActReqData();

                    actreqdata.Stream = nStream;
                    actreqdata.Function = nFunction;
                    actreqdata.Sysbyte = nSysbyte;

                    long dflist1 = 0;
                    string CPNAME = null;
                    string lotId = null;
                    uint[] ocrRead_UINT1 = new uint[1];
                    Gem.GetList(nMsgId, ref dflist1);

                    Gem.GetList(nMsgId, ref dflist1);// [L] LOT ID 
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] LOT ID 
                    Gem.GetAscii(nMsgId, ref lotId);// [A] LOT ID 
                    actreqdata.LotID = lotId;

                    Gem.GetList(nMsgId, ref dflist1);// [L] OCR READ 
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] OCR READ
                    Gem.GetU4(nMsgId, ref ocrRead_UINT1);// [U] OCR READ
                    actreqdata.OCRReadFalg = (int)ocrRead_UINT1[0];

                    remoteActReqData = actreqdata;
                    remoteActReqData.ActionType = EnumRemoteCommand.PSTART;
                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.ZUP.ToString()
                    || REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.TESTEND.ToString()
                    || REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.WAFERUNLOAD.ToString())
                {
                    long pnObjectID = 0;
                    byte HAACK = 0x04;
                    MakeObject(ref pnObjectID);
                    SetListItem(pnObjectID, 2);
                    SetBinaryItem(pnObjectID, HAACK);
                    SetListItem(pnObjectID, 0);
                    SendSECSMessage(pnObjectID, nStream, nFunction + 1, nSysbyte);

                    var actreqdata = new StageActReqData();
                    actreqdata.ObjectID = pnObjectID;
                    actreqdata.Stream = nStream;
                    actreqdata.Function = nFunction;
                    actreqdata.Sysbyte = nSysbyte;

                    long dataListCount = 0;
                    string CPNAME = null;
                    string stagenumber = null;
                    Gem.GetList(nMsgId, ref dataListCount);

                    Gem.GetList(nMsgId, ref dataListCount);// [L] StageNumber
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] StageNumber
                    Gem.GetAscii(nMsgId, ref stagenumber);// [A] StageNumber

                    actreqdata.StageNumber = Convert.ToInt32(stagenumber); //Data [U] StageNumber

                    remoteActReqData = actreqdata;

                    if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.ZUP.ToString())
                        remoteActReqData.ActionType = EnumRemoteCommand.ZUP;
                    if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.TESTEND.ToString())
                        remoteActReqData.ActionType = EnumRemoteCommand.TESTEND;
                    if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.WAFERUNLOAD.ToString())
                        remoteActReqData.ActionType = EnumRemoteCommand.WAFERUNLOAD;
                }

                if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.ACTIVATE_PROCESS.ToString())
                {
                    long pnObjectID = 0;
                    byte HAACK = 0x04;
                    MakeObject(ref pnObjectID);
                    SetListItem(pnObjectID, 2);
                    SetBinaryItem(pnObjectID, HAACK);
                    SetListItem(pnObjectID, 0);
                    SendSECSMessage(pnObjectID, nStream, nFunction + 1, nSysbyte);

                    var actreqdata = new ActiveProcessActReqData();
                    long dflist = 0;
                    uint[] user_foupnumber_UINT4 = new uint[1];
                    string msg = null;

                    Gem.GetList(nMsgId, ref dflist);

                    Gem.GetList(nMsgId, ref dflist); // [L] FoupNumber List
                    Gem.GetAscii(nMsgId, ref msg); //[A] Title FoupNumber 
                    Gem.GetU4(nMsgId, ref user_foupnumber_UINT4); // [U] FoupNumber
                    actreqdata.FoupNumber = (int)user_foupnumber_UINT4[0]; // Data : [U] FoupNumber

                    Gem.GetList(nMsgId, ref dflist); // [L] Lot ID List
                    Gem.GetAscii(nMsgId, ref msg); //[A] Title LOT ID  
                    Gem.GetAscii(nMsgId, ref msg); //[A] Lot ID 
                    actreqdata.LotID = msg; // Data : [A] Lot ID 

                    Gem.GetList(nMsgId, ref dflist); // [L] ListOfStagesToUse List
                    Gem.GetAscii(nMsgId, ref msg); //[A] Title ListOfStagesToUse
                    Gem.GetAscii(nMsgId, ref msg); //[A]  ListOfStagesToUse
                    actreqdata.UseStageNumbers_str = msg;
                    var array = msg.ToArray();
                    for (int index = 0; index < array.Length; index++)
                    {
                        if (array[index] == '1')
                            actreqdata.UseStageNumbers.Add(index + 1);// Data : [A] ListOfStagesToUse
                    }



                    remoteActReqData = actreqdata;
                    remoteActReqData.ActionType = EnumRemoteCommand.ACTIVATE_PROCESS;
                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.DOWNLOAD_STAGE_RECIPE.ToString())
                {
                    long pnObjectID = 0;
                    byte HAACK = 0x00;
                    MakeObject(ref pnObjectID);
                    SetListItem(pnObjectID, 2);
                    SetBinaryItem(pnObjectID, HAACK);
                    SetListItem(pnObjectID, 0);
                    SendSECSMessage(pnObjectID, nStream, nFunction + 1, nSysbyte);

                    var actreqdata = new DownloadStageRecipeActReqData();

                    long dflist = 0;
                    uint[] user_foupnumber_UINT4 = new uint[1];
                    uint[] user_stagenumber_UINT4 = new uint[1];
                    string msg = null;

                    Gem.GetList(nMsgId, ref dflist);

                    Gem.GetList(nMsgId, ref dflist); // [L] FoupNumber List
                    Gem.GetAscii(nMsgId, ref msg); //[A] Title FoupNumber 
                    Gem.GetU4(nMsgId, ref user_foupnumber_UINT4); // [U] FoupNumber
                    actreqdata.FoupNumber = (int)user_foupnumber_UINT4[0]; // Data : [U] FoupNumber

                    Gem.GetList(nMsgId, ref dflist); // [L] Lot ID List
                    Gem.GetAscii(nMsgId, ref msg); //[A] Title LOT ID  
                    Gem.GetAscii(nMsgId, ref msg); //[A] Lot ID 
                    actreqdata.LotID = msg; // Data : [A] Lot ID 


                    Gem.GetList(nMsgId, ref dflist); // [L] Set Recipe List
                    var count = dflist;
                    for (int index = 0; index < count; index++)
                    {
                        Gem.GetList(nMsgId, ref dflist);

                        Gem.GetList(nMsgId, ref dflist); // [L] StageNumber
                        Gem.GetAscii(nMsgId, ref msg); //[A] StageNumber
                        Gem.GetU4(nMsgId, ref user_stagenumber_UINT4); // [U]StageNumber

                        Gem.GetList(nMsgId, ref dflist); // [L] Recipe ID
                        Gem.GetAscii(nMsgId, ref msg); //[A] Title Recipe ID
                        Gem.GetAscii(nMsgId, ref msg); //[A] Recipe ID

                        actreqdata.RecipeDic.Add((int)user_stagenumber_UINT4[0], msg); //Data : [U] StageNumber , [A] Recipe Id
                    }

                    remoteActReqData = actreqdata;
                    //remoteActReqData.ActionType = EnumRemoteCommand.DOWNLOAD_STAGE_RECIPE;
                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.SET_PARAMETERS.ToString())
                {
                    long pnObjectID = 0;
                    byte HAACK = 0x00;
                    MakeObject(ref pnObjectID);
                    SetListItem(pnObjectID, 2);
                    SetBinaryItem(pnObjectID, HAACK);
                    SetListItem(pnObjectID, 0);
                    SendSECSMessage(pnObjectID, nStream, nFunction + 1, nSysbyte);

                    var actreqdata = new SetParameterActReqData();

                    long dflist = 0;
                    string msg = null;
                    string parameterinfo = null;
                    string parametervalue = null;
                    uint[] user_foupnumber_UINT4 = new uint[1];
                    Gem.GetList(nMsgId, ref dflist);
                    Gem.GetList(nMsgId, ref dflist);// [L] FoupNumber
                    Gem.GetAscii(nMsgId, ref msg);// [A] FoupNumber
                    Gem.GetU4(nMsgId, ref user_foupnumber_UINT4);// [U] FoupNumber
                    actreqdata.FoupNumber = (int)user_foupnumber_UINT4[0]; //Data [U] Foup Number

                    Gem.GetList(nMsgId, ref dflist); // [L] Lot ID List
                    Gem.GetAscii(nMsgId, ref msg); //[A] Title LOT ID  
                    Gem.GetAscii(nMsgId, ref msg); //[A] Lot ID 
                    actreqdata.LotID = msg; // Data : [A] Lot ID 

                    Gem.GetList(nMsgId, ref dflist); // [L] Recipe ID
                    Gem.GetAscii(nMsgId, ref msg); //[A] Title Recipe ID
                    Gem.GetAscii(nMsgId, ref msg); //[A] Recipe ID

                    Gem.GetList(nMsgId, ref dflist); // [L] ListOfDeviceParams
                    Gem.GetAscii(nMsgId, ref msg); //[A] ListOfDeviceParams

                    Gem.GetList(nMsgId, ref dflist);
                    long listcount = dflist;
                    for (int index = 0; index < listcount; index++)
                    {
                        Gem.GetList(nMsgId, ref dflist);

                        Gem.GetAscii(nMsgId, ref parameterinfo); //[A] Parameter Info : Stage Number_D/S/E_ParameterName
                        Gem.GetAscii(nMsgId, ref parametervalue); //[A] Parameter Value
                        actreqdata.ParameterDic.Add(parameterinfo, parametervalue);
                    }

                    Gem.GetList(nMsgId, ref dflist); // [L] ListOfOperParams
                    Gem.GetAscii(nMsgId, ref msg); //[A] ListOfOperParams

                    Gem.GetList(nMsgId, ref dflist);
                    listcount = dflist;

                    for (int index = 0; index < listcount; index++)
                    {
                        Gem.GetList(nMsgId, ref dflist);

                        Gem.GetAscii(nMsgId, ref parameterinfo); //[A] Parameter Info : Stage Number_D/S/E_ParameterName
                        Gem.GetAscii(nMsgId, ref parametervalue); //[A] Parameter Value
                        actreqdata.ParameterDic.Add(parameterinfo, parametervalue);
                    }
                    remoteActReqData = actreqdata;
                    remoteActReqData.ActionType = EnumRemoteCommand.SET_PARAMETERS;
                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.DOCK_FOUP.ToString())
                {
                    long pnObjectID = 0;
                    byte HAACK = 0x04;
                    MakeObject(ref pnObjectID);
                    SetListItem(pnObjectID, 2);
                    SetBinaryItem(pnObjectID, HAACK);
                    SetListItem(pnObjectID, 0);
                    SendSECSMessage(pnObjectID, nStream, nFunction + 1, nSysbyte);

                    var actreqdata = new DockFoupActReqData();

                    long dflist1 = 0;
                    string lpnumber = null;
                    uint[] user_foupnumber_UINT4 = new uint[1];
                    Gem.GetList(nMsgId, ref dflist1);

                    Gem.GetList(nMsgId, ref dflist1);// [L] FoupNumber
                    Gem.GetAscii(nMsgId, ref lpnumber);// [A] FoupNumber
                    Gem.GetU4(nMsgId, ref user_foupnumber_UINT4);// [U] FoupNumber

                    actreqdata.FoupNumber = (int)user_foupnumber_UINT4[0]; //Data [U] Foup Number

                    remoteActReqData = actreqdata;
                    //remoteActReqData.ActionType = EnumRemoteCommand.DOCK_FOUP;
                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.SELECT_SLOTS.ToString())
                {

                    long pnObjectID = 0;
                    byte HAACK = 0x04;
                    MakeObject(ref pnObjectID);
                    SetListItem(pnObjectID, 2);
                    SetBinaryItem(pnObjectID, HAACK);
                    SetListItem(pnObjectID, 0);
                    SendSECSMessage(pnObjectID, nStream, nFunction + 1, nSysbyte);

                    var actreqdata = new SelectSlotsActReqData();

                    long dflist = 0;
                    uint[] user_foupnumber_UINT4 = new uint[1];
                    string msg = null;

                    Gem.GetList(nMsgId, ref dflist);

                    Gem.GetList(nMsgId, ref dflist); // [L] FoupNumber List
                    Gem.GetAscii(nMsgId, ref msg); //[A] Title FoupNumber 
                    Gem.GetU4(nMsgId, ref user_foupnumber_UINT4); // [U] FoupNumber
                    actreqdata.FoupNumber = (int)user_foupnumber_UINT4[0]; // Data : [U] FoupNumber

                    Gem.GetList(nMsgId, ref dflist); // [L] Lot ID List
                    Gem.GetAscii(nMsgId, ref msg); //[A] Title LOT ID  
                    Gem.GetAscii(nMsgId, ref msg); //[A] Lot ID 
                    actreqdata.LotID = msg; // Data : [A] Lot ID 

                    Gem.GetList(nMsgId, ref dflist); // [L] ListOfSlotToUse List
                    Gem.GetAscii(nMsgId, ref msg); //[A] Title ListOfSlotToUse
                    Gem.GetAscii(nMsgId, ref msg); //[A]  ListOfSlotToUse
                    actreqdata.UseSlotNumbers_str = msg;
                    var array = msg.ToArray();

                    for (int index = 0; index < array.Length; index++)
                    {
                        if (array[index] == '1')
                            actreqdata.UseSlotNumbers.Add(index + 1);// Data : [A] ListOfStagesToUse
                    }

                    remoteActReqData = actreqdata;
                    //remoteActReqData.ActionType = EnumRemoteCommand.SELECT_SLOTS;
                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.START_LOT.ToString())
                {

                    long pnObjectID = 0;
                    byte HAACK = 0x04;
                    MakeObject(ref pnObjectID);
                    SetListItem(pnObjectID, 2);
                    SetBinaryItem(pnObjectID, HAACK);
                    SetListItem(pnObjectID, 0);
                    SendSECSMessage(pnObjectID, nStream, nFunction + 1, nSysbyte);

                    var actreqdata = new StartLotActReqData();

                    long dflist = 0;
                    uint[] user_foupnumber_UINT4 = new uint[1];
                    string msg = null;

                    Gem.GetList(nMsgId, ref dflist);

                    Gem.GetList(nMsgId, ref dflist); // [L] FoupNumber List
                    Gem.GetAscii(nMsgId, ref msg); //[A] Title FoupNumber 
                    Gem.GetU4(nMsgId, ref user_foupnumber_UINT4); // [U] FoupNumber
                    actreqdata.FoupNumber = (int)user_foupnumber_UINT4[0]; // Data : [U] FoupNumber

                    Gem.GetList(nMsgId, ref dflist); // [L] Lot ID List
                    Gem.GetAscii(nMsgId, ref msg); //[A] Title LOT ID  
                    Gem.GetAscii(nMsgId, ref msg); //[A] Lot ID 
                    actreqdata.LotID = msg; // Data : [A] Lot ID 

                    remoteActReqData = actreqdata;
                    //remoteActReqData.ActionType = EnumRemoteCommand.START_LOT;
                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.Z_UP.ToString())
                {

                    long pnObjectID = 0;
                    byte HAACK = 0x04;
                    MakeObject(ref pnObjectID);
                    SetListItem(pnObjectID, 2);
                    SetBinaryItem(pnObjectID, HAACK);
                    SetListItem(pnObjectID, 0);
                    SendSECSMessage(pnObjectID, nStream, nFunction + 1, nSysbyte);

                    var actreqdata = new ZUpActReqData();

                    long dflist = 0;
                    uint[] user_stagenumber_UINT4 = new uint[1];
                    string msg = null;

                    Gem.GetList(nMsgId, ref dflist);

                    Gem.GetList(nMsgId, ref dflist); // [L] Stage Number List
                    Gem.GetAscii(nMsgId, ref msg); //[A] Title StageNumber 
                    Gem.GetU4(nMsgId, ref user_stagenumber_UINT4); // [U] StageNumber
                    actreqdata.StageNumber = (int)user_stagenumber_UINT4[0]; // Data : [U] StageNumber



                    remoteActReqData = actreqdata;
                    remoteActReqData.ActionType = EnumRemoteCommand.Z_UP;
                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.END_TEST.ToString())
                {
                    long pnObjectID = 0;
                    byte HAACK = 0x04;
                    MakeObject(ref pnObjectID);
                    SetListItem(pnObjectID, 2);
                    SetBinaryItem(pnObjectID, HAACK);
                    SetListItem(pnObjectID, 0);
                    SendSECSMessage(pnObjectID, nStream, nFunction + 1, nSysbyte);

                    var actreqdata = new EndTestReqDate();

                    long dflist = 0;
                    uint[] user_stagenumber_UINT4 = new uint[1];
                    uint[] user_pmiexecflag_UINT4 = new uint[1];
                    string msg = null;

                    Gem.GetList(nMsgId, ref dflist);

                    Gem.GetList(nMsgId, ref dflist); // [L] Stage Number List
                    Gem.GetAscii(nMsgId, ref msg); //[A] Title StageNumber 
                    Gem.GetU4(nMsgId, ref user_stagenumber_UINT4); // [U] StageNumber
                    actreqdata.StageNumber = (int)user_stagenumber_UINT4[0];

                    Gem.GetList(nMsgId, ref dflist); // [L] PMI Exce Flag List
                    Gem.GetAscii(nMsgId, ref msg); //[A] Title StageNumber 
                    Gem.GetU4(nMsgId, ref user_pmiexecflag_UINT4); // [U] PMI Exce Flag
                    actreqdata.PMIExecFlag = (int)user_pmiexecflag_UINT4[0];


                    remoteActReqData = actreqdata;
                    remoteActReqData.ActionType = EnumRemoteCommand.END_TEST;

                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.CANCEL_CARRIER.ToString()
                    | REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.CARRIER_SUSPEND.ToString())
                {
                    long pnObjectID = 0;
                    byte HAACK = 0x04;
                    MakeObject(ref pnObjectID);
                    SetListItem(pnObjectID, 2);
                    SetBinaryItem(pnObjectID, HAACK);
                    SetListItem(pnObjectID, 0);
                    SendSECSMessage(pnObjectID, nStream, nFunction + 1, nSysbyte);

                    var actreqdata = new CarrierCancleData();

                    long dflist1 = 0;
                    string lpnumber = null;
                    uint[] user_foupnumber_UINT4 = new uint[1];
                    Gem.GetList(nMsgId, ref dflist1);

                    Gem.GetList(nMsgId, ref dflist1);// [L] FoupNumber
                    Gem.GetAscii(nMsgId, ref lpnumber);// [A] FoupNumber
                    Gem.GetU4(nMsgId, ref user_foupnumber_UINT4);// [U] FoupNumber

                    actreqdata.FoupNumber = (int)user_foupnumber_UINT4[0]; //Data [U] Foup Number

                    remoteActReqData = actreqdata;
                    if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.CANCEL_CARRIER.ToString())
                        remoteActReqData.ActionType = EnumRemoteCommand.CANCEL_CARRIER;
                    else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.CARRIER_SUSPEND.ToString())
                        remoteActReqData.ActionType = EnumRemoteCommand.CARRIER_SUSPEND;
                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.ERROR_END.ToString())
                {
                    long pnObjectID = 0;
                    byte HAACK = 0x04;
                    MakeObject(ref pnObjectID);
                    SetListItem(pnObjectID, 2);
                    SetBinaryItem(pnObjectID, HAACK);
                    SetListItem(pnObjectID, 0);
                    SendSECSMessage(pnObjectID, nStream, nFunction + 1, nSysbyte);

                    var actreqdata = new ErrorEndData();

                    long dflist1 = 0;
                    string lpnumber = null;
                    uint[] user_stagenumber_UINT4 = new uint[1];
                    Gem.GetList(nMsgId, ref dflist1);

                    Gem.GetList(nMsgId, ref dflist1);// [L] StageNumber
                    Gem.GetAscii(nMsgId, ref lpnumber);// [A] StageNumber
                    Gem.GetU4(nMsgId, ref user_stagenumber_UINT4);// [U] StageNumber

                    actreqdata.StageNumber = (int)user_stagenumber_UINT4[0]; //Data [U] StageNumber

                    remoteActReqData = actreqdata;
                    remoteActReqData.ActionType = EnumRemoteCommand.ERROR_END;
                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.START_STAGE.ToString())
                {
                    long pnObjectID = 0;
                    byte HAACK = 0x04;
                    MakeObject(ref pnObjectID);
                    SetListItem(pnObjectID, 2);
                    SetBinaryItem(pnObjectID, HAACK);
                    SetListItem(pnObjectID, 0);
                    SendSECSMessage(pnObjectID, nStream, nFunction + 1, nSysbyte);

                    var actreqdata = new StartStage();

                    long dflist1 = 0;
                    string lpnumber = null;
                    string stagenumber = null;
                    string lotid = null;
                    uint[] user_stagenumber_UINT4 = new uint[1];
                    uint[] user_foupnumber_UINT4 = new uint[1];
                    Gem.GetList(nMsgId, ref dflist1);

                    Gem.GetList(nMsgId, ref dflist1);// [L] FoupNumber
                    Gem.GetAscii(nMsgId, ref lpnumber);// [A] FoupNumber
                    Gem.GetU4(nMsgId, ref user_foupnumber_UINT4);// [U] FoupNumber
                    actreqdata.FoupNumber = (int)user_foupnumber_UINT4[0]; //Data [U] FoupNumber

                    Gem.GetList(nMsgId, ref dflist1); // [L] Lot ID List
                    Gem.GetAscii(nMsgId, ref lotid); //[A] Title LOT ID  
                    Gem.GetAscii(nMsgId, ref lotid); //[A] Lot ID 
                    actreqdata.LotID = lotid; // Data : [A] Lot ID 


                    Gem.GetList(nMsgId, ref dflist1);// [L] StageNumber
                    Gem.GetAscii(nMsgId, ref stagenumber);// [A] StageNumber
                    Gem.GetU4(nMsgId, ref user_stagenumber_UINT4);// [U] StageNumber
                    actreqdata.StageNumber = (int)user_stagenumber_UINT4[0]; //Data [U] StageNumber

                    remoteActReqData = actreqdata;
                    remoteActReqData.ActionType = EnumRemoteCommand.START_STAGE;
                }
                else if(REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.WAFERID_LIST.ToString())
                {
                    long pnObjectID = 0;
                    byte HAACK = 0x04;
                    MakeObject(ref pnObjectID);
                    SetListItem(pnObjectID, 2);
                    SetBinaryItem(pnObjectID, HAACK);
                    SetListItem(pnObjectID, 0);
                    SendSECSMessage(pnObjectID, nStream, nFunction + 1, nSysbyte);

                    var actreqdata = new AssignWaferIDMap();

                    long dflist = 0;
                    uint[] user_foupnumber_UINT4 = new uint[1];
                    string msg = null;
                    string waferid = null;

                    Gem.GetList(nMsgId, ref dflist);

                    Gem.GetList(nMsgId, ref dflist); // [L] FoupNumber List
                    Gem.GetAscii(nMsgId, ref msg); //[A] Title FoupNumber 
                    Gem.GetU4(nMsgId, ref user_foupnumber_UINT4); // [U] FoupNumber
                    actreqdata.FoupNumber = (int)user_foupnumber_UINT4[0]; // Data : [U] FoupNumber

                    Gem.GetList(nMsgId, ref dflist); // [L] Lot ID List
                    Gem.GetAscii(nMsgId, ref msg); //[A] Title LOT ID  
                    Gem.GetAscii(nMsgId, ref msg); //[A] Lot ID 
                    actreqdata.LotID = msg; // Data : [A] Lot ID 

                    Gem.GetList(nMsgId, ref dflist); // [L] Wafer id List
                    Gem.GetAscii(nMsgId, ref msg); //[A] Title Wafer id List
                    Gem.GetList(nMsgId, ref dflist); //[A]  Wafer id List data

                    var count = dflist;
                    for (int index = 0; index < count; index++)
                    {
                        Gem.GetAscii(nMsgId, ref waferid); //[A] Wafer ID
                        if (actreqdata.WaferIDs == null)
                        {
                            actreqdata.WaferIDs = new List<string>();
                        }
                        actreqdata.WaferIDs.Add(waferid);
                    }

                    remoteActReqData = actreqdata;
                    remoteActReqData.ActionType = EnumRemoteCommand.WAFERID_LIST;
                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.WAFERID_LIST.ToString())
                {
                    long pnObjectID = 0;
                    byte HAACK = 0x04;
                    MakeObject(ref pnObjectID);
                    SetListItem(pnObjectID, 2);
                    SetBinaryItem(pnObjectID, HAACK);
                    SetListItem(pnObjectID, 0);
                    SendSECSMessage(pnObjectID, nStream, nFunction + 1, nSysbyte);

                    var actreqdata = new AssignWaferIDMap();

                    long dflist = 0;
                    uint[] user_foupnumber_UINT4 = new uint[1];
                    string msg = null;
                    string waferid = null;

                    Gem.GetList(nMsgId, ref dflist);

                    Gem.GetList(nMsgId, ref dflist); // [L] FoupNumber List
                    Gem.GetAscii(nMsgId, ref msg); //[A] Title FoupNumber 
                    Gem.GetU4(nMsgId, ref user_foupnumber_UINT4); // [U] FoupNumber
                    actreqdata.FoupNumber = (int)user_foupnumber_UINT4[0]; // Data : [U] FoupNumber

                    Gem.GetList(nMsgId, ref dflist); // [L] Lot ID List
                    Gem.GetAscii(nMsgId, ref msg); //[A] Title LOT ID  
                    Gem.GetAscii(nMsgId, ref msg); //[A] Lot ID 
                    actreqdata.LotID = msg; // Data : [A] Lot ID 

                    Gem.GetList(nMsgId, ref dflist); // [L] Wafer id List
                    Gem.GetAscii(nMsgId, ref msg); //[A] Title Wafer id List
                    Gem.GetList(nMsgId, ref dflist); //[A]  Wafer id List data

                    var count = dflist;
                    for (int index = 0; index < count; index++)
                    {
                        Gem.GetAscii(nMsgId, ref waferid); //[A] Wafer ID
                        if (actreqdata.WaferIDs == null)
                        {
                            actreqdata.WaferIDs = new List<string>();
                        }
                        actreqdata.WaferIDs.Add(waferid);
                    }

                    remoteActReqData = actreqdata;
                    remoteActReqData.ActionType = EnumRemoteCommand.WAFERID_LIST;
                }
                callback.OnRemoteCommandAction(remoteActReqData);
            }
            catch (Exception err)
            {
                WriteLog(err.Message);
            }
        }

        #endregion

        #region <remarks> S3F17 </remarks>
        private void XGEM_OnSECSMessageReceive_S3F17(long nMsgId, long nStream, long nFunction, long nSysbyte)
        {
            try
            {
                //long pnMsgId = 0;
                //byte CAACK = 0;

                long mainListCount = 0;
                long subListCount = 0;
                long dataListCount = 0;

                uint[] USER_UINT4 = new uint[1];
                string CARRIERACTION = null;
                string CARRIERID = null;
                byte[] PTN = new byte[1];

                string CATTRID = null;
                string CATTRDATA = null;
                //string CATTRDATA2 = null;

                CarrierActReqData carrierActReqData = null;

                int total_cnt = 0;

                //Stopwatch sw = new Stopwatch();
                //sw.Start();

                Gem.GetList(nMsgId, ref mainListCount);
                Gem.GetU4(nMsgId, ref USER_UINT4);
                Gem.GetAscii(nMsgId, ref CARRIERACTION);
                Gem.GetAscii(nMsgId, ref CARRIERID);
                Gem.GetU1(nMsgId, ref PTN);
                Gem.GetList(nMsgId, ref subListCount);

                if (CARRIERACTION.ToUpper() == EnumCarrierAction.PROCEEDWITHCARRIER.ToString())
                {
                    var actCarrierData = new ProceedWithCarrierReqData();
                    actCarrierData.ActionType = EnumCarrierAction.PROCEEDWITHCARRIER;
                    actCarrierData.Sysbyte = nSysbyte;
                    actCarrierData.DataID = USER_UINT4[0];
                    actCarrierData.CarrierAction = CARRIERACTION;
                    actCarrierData.CarrierID = CARRIERID;
                    actCarrierData.PTN = PTN[0];
                    actCarrierData.Count = subListCount;
                    actCarrierData.CattrID = new string[subListCount];
                    actCarrierData.CattrData = new string[subListCount];
                    actCarrierData.SlotMap = new string[subListCount * 25];


                    for (int i = 0; i < subListCount; i++)
                    {
                        Gem.GetList(nMsgId, ref dataListCount);

                        if (actCarrierData.DataID == 0)
                        {
                            //LOT ID
                            Gem.GetAscii(nMsgId, ref CATTRID);
                            actCarrierData.CattrID[i] = CATTRID;
                            Gem.GetAscii(nMsgId, ref CATTRDATA);
                            actCarrierData.CattrData[i] = CATTRDATA;
                        }
                        else if (actCarrierData.DataID == 1)
                        {
                            long waferIdListCount = 0;
                            //WAFER ID
                            Gem.GetAscii(nMsgId, ref CATTRID);
                            actCarrierData.CattrID[i] = CATTRID;
                            Gem.GetList(nMsgId, ref waferIdListCount);

                            for (int s = 0; s < 25; s++)
                            {
                                string slotmap = null;
                                Gem.GetAscii(nMsgId, ref slotmap);
                                actCarrierData.SlotMap[total_cnt++] = slotmap;
                            }
                        }
                        //else if (listCount3 == 3)
                        //{
                        //    m_XGem.GetAscii(nMsgId, ref CATTRID);
                        //    Carrier.CattrID[i] = CATTRID;
                        //    m_XGem.GetList(nMsgId, ref listCount4);

                        //    for (int s = 0; s < 25; s++)
                        //    {
                        //        string slotmap = null;
                        //        m_XGem.GetAscii(nMsgId, ref slotmap);
                        //        Carrier.SlotMap[total_cnt++] = slotmap;
                        //    }

                        //    string temp = null;
                        //    string cattrdata = null;
                        //    m_XGem.GetList(nMsgId, ref listCount4);
                        //    m_XGem.GetAscii(nMsgId, ref temp);
                        //    m_XGem.GetAscii(nMsgId, ref cattrdata);
                        //    Carrier.CattrData[i] = cattrdata;
                        //}
                    }

                    carrierActReqData = actCarrierData;
                }
                else if (CARRIERACTION.ToUpper() == EnumCarrierAction.PROCEEDWITHSLOT.ToString())
                {
                    var actCarrierData = new ProceedWithSlotReqData();
                    actCarrierData.ActionType = EnumCarrierAction.PROCEEDWITHSLOT;

                    actCarrierData.CarrierID = CARRIERID;
                    actCarrierData.PTN = PTN[0];
                    actCarrierData.Count = subListCount;
                    actCarrierData.SlotMap = new string[25];
                    actCarrierData.OcrMap = new string[25];
                    long slotListCount = 0;
                    for (int i = 0; i < subListCount; i++)
                    {
                        Gem.GetList(nMsgId, ref dataListCount);
                        Gem.GetAscii(nMsgId, ref CATTRID);
                        Gem.GetList(nMsgId, ref slotListCount);

                        for (int s = 0; s < 25; s++)
                        {
                            string slotmap = null;
                            Gem.GetAscii(nMsgId, ref slotmap);
                            actCarrierData.SlotMap[s] = slotmap;
                        }

                        string temp = null;
                        string cattrdata = null;
                        Gem.GetList(nMsgId, ref dataListCount);
                        Gem.GetAscii(nMsgId, ref temp);
                        Gem.GetAscii(nMsgId, ref cattrdata);
                        actCarrierData.Usage = cattrdata;

                        long item_cnt = 0;
                        string cattrid = null;
                        long slotmap_cnt = 0;
                        Gem.GetList(nMsgId, ref item_cnt);
                        Gem.GetAscii(nMsgId, ref cattrid);
                        Gem.GetList(nMsgId, ref slotmap_cnt);

                        for (int s = 0; s < 25; s++)
                        {
                            string ocrmap = null;
                            Gem.GetAscii(nMsgId, ref ocrmap);
                            actCarrierData.OcrMap[s] = ocrmap;
                        }
                    }

                    carrierActReqData = actCarrierData;
                }
                else if (CARRIERACTION.ToUpper() == EnumCarrierAction.RELEASECARRIER.ToString()
                    || CARRIERACTION.ToUpper() == EnumCarrierAction.CANCELCARRIER.ToString())
                {
                    var actCarrierData = new CarrieActReqData();

                    if (CARRIERACTION.ToUpper() == EnumCarrierAction.RELEASECARRIER.ToString())
                    {
                        actCarrierData.ActionType = EnumCarrierAction.RELEASECARRIER;
                    }
                    else if (CARRIERACTION.ToUpper() == EnumCarrierAction.CANCELCARRIER.ToString())
                    {
                        actCarrierData.ActionType = EnumCarrierAction.CANCELCARRIER;
                    }

                    actCarrierData.Sysbyte = nSysbyte;
                    actCarrierData.DataID = USER_UINT4[0];
                    actCarrierData.CarrierAction = CARRIERACTION;
                    actCarrierData.CarrierID = CARRIERID;
                    actCarrierData.PTN = PTN[0];
                    actCarrierData.Count = subListCount;
                    for (int i = 0; i < subListCount; i++)
                    {
                        Gem.GetList(nMsgId, ref dataListCount);

                        //CARRIER DATA
                        Gem.GetAscii(nMsgId, ref CATTRID);
                        Gem.GetAscii(nMsgId, ref CATTRDATA);
                        actCarrierData.CarrierData = CATTRDATA;
                    }
                    carrierActReqData = actCarrierData;
                }
                else if (CARRIERACTION.ToUpper() == EnumCarrierAction.PROCESSEDWITHCELLSLOT.ToString())
                {
                    var actCarrierData = new ProceedWithCellSlotActReqData();
                    actCarrierData.ActionType = EnumCarrierAction.PROCESSEDWITHCELLSLOT;

                    actCarrierData.CarrierID = CARRIERID;
                    actCarrierData.PTN = PTN[0];
                    actCarrierData.Count = subListCount;

                    string lotid = null;
                    string slotInfo = null;

                    Gem.GetList(nMsgId, ref dataListCount); // [L] LOT ID 
                    Gem.GetAscii(nMsgId, ref lotid);        // [CPNAME] LOT ID
                    if (lotid.Equals("LOTID"))
                    {
                        Gem.GetAscii(nMsgId, ref lotid);    // [CPVAL][A] LOT ID 
                        actCarrierData.LOTID = lotid;
                    }

                    Gem.GetList(nMsgId, ref dataListCount); // [L] SLOT INFO
                    Gem.GetAscii(nMsgId, ref slotInfo);     // [CPNAME] SLOT INFO
                    if (slotInfo.Equals("SLOTINFO"))
                    {
                        Gem.GetAscii(nMsgId, ref slotInfo); // [CPVAL][A] SLOT INFO
                        actCarrierData.SlotMap = slotInfo;
                    }

                    carrierActReqData = actCarrierData;
                }

                Gem.CloseObject(nMsgId);
                if (carrierActReqData != null)
                {
                    carrierActReqData.Stream = 3;
                    carrierActReqData.Function = 17;
                    carrierActReqData.Sysbyte = nSysbyte;
                    callback?.OnCarrierActMsgRecive(carrierActReqData);
                }
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region <remarks> S3F27 </remarks>
        private void XGEM_OnSECSMessageReceive_S3F27(long nMsgId, long nStream, long nFunction, long nSysbyte)
        {
            try
            {
                //long pnMsgId = 0;
                //byte CAACK = 0;

                long listCount1 = 0;
                long listCount2 = 0;

                byte[] ACCESSMODE = new byte[1];
                byte[] PTN = null;
                List<int> ptns = new List<int>();
                Gem.GetList(nMsgId, ref listCount1);
                Gem.GetU1(nMsgId, ref ACCESSMODE);
                Gem.GetList(nMsgId, ref listCount2);
                PTN = new byte[listCount2];
                if (listCount2 != 0)
                {
                    for (int count = 0; count < listCount2; count++)
                    {
                        Gem.GetU1(nMsgId, ref PTN);
                        ptns.Add(PTN[0]);
                    }
                }

                CarrierAccesModeReqData reqData = new CarrierAccesModeReqData();
                reqData.ObjectID = nMsgId;
                reqData.Stream = nStream;
                reqData.Function = nFunction;
                reqData.Sysbyte = nSysbyte;
                reqData.ActionType = EnumCarrierAction.CHANGEACCESSMODE;
                reqData.AccessMode = ACCESSMODE[0];
                reqData.LoadPortList = ptns;

                callback.OnCarrierActMsgRecive(reqData);
                /////////////////////////////////////////////////////////////////////////////////
                //if ((CAACK != 1) && (CAACK != 2) && (CAACK != 6))
                //{

                //}

                //m_XGem.MakeObject(ref pnMsgId);

                //m_XGem.SetList(pnMsgId, 2);
                //m_XGem.SetU1(pnMsgId, CAACK);//
                //m_XGem.SetList(pnMsgId, 0);

                //m_XGem.SendSECSMessage(pnMsgId, nStream, nFunction + 1, nSysbyte);
            }
            catch (Exception)
            {

            }
        }
        #endregion
        //#region <remarks> long S3F17SendAck(long pnMsgId, long nStream, long nFunction, long nSysbyte, byte CAACK, long nCount) </remarks>
        ///// <summary>
        /////     S3F17의 대답을 하는 함수.
        ///// </summary>
        ///// <remarks>
        /////     GEM -> Main.
        /////     S3F18
        /////     First Create : 2018-04-26, Semics R&D1, Jake Kim.
        ///// </remarks>
        ///// <param name="pnMsgId">   ID                          </param>
        ///// <param name="nStream">      Stream Number               </param>
        ///// <param name="nFunction">    Function Number             </param>
        ///// <param name="nSysbyte">     Messege Number              </param>
        ///// <param name="CAACK">        Carrier Action Acknowledge  </param>
        ///// <param name="nCount">       Error List Size             </param>
        ///// <returns>Message Send 결과</returns>
        //private long S3F17SendAck(long pnMsgId, long nStream, long nFunction, long nSysbyte, byte CAACK, long nCount)
        //{
        //    long retVal = 0;

        //    SecsGemClient.MakeObject(ref pnMsgId);

        //    SecsGemClient.SetListItem(pnMsgId, 2);
        //    SecsGemClient.SetUint1Item(pnMsgId, CAACK);
        //    SecsGemClient.SetListItem(pnMsgId, nCount);

        //    for (int i = 0; i < nCount; i++)
        //    {
        //        SecsGemClient.SetListItem(pnMsgId, 2);
        //        SecsGemClient.SetUint2Item(pnMsgId, 0);
        //        SecsGemClient.SetStringItem(pnMsgId, "");
        //    }

        //    retVal = SecsGemClient.SendSECSMessage(pnMsgId, nStream, nFunction + 1, nSysbyte);
        //    return retVal;
        //}
        //#endregion




        private void XGEM_OnXGEMStateEvent(long nState)
        {
            #region State Number 설명
            //-1: Unknown    - Unknown
            // 0: Init       - XGem Initialize가 필요한 상태입니다.
            // 1: Idle       – XGem Initialize가 완료된 상태입니다.
            // 2: Setup      – XGem Start가 된 상태입니다.
            // 3: Ready      – Xservice Manager와 연결된 상태입니다.
            // 4: Execute    – XGem Process와 연결된 상태이며 이 상태에서만 메시지를 주고 받을 수 있다.
            #endregion
            try
            {

            }
            catch
            {
            }
            if (Enum.IsDefined(typeof(SecsGemStateEnum), (int)nState))
                Enum.TryParse(nState.ToString(), out SECSGemState);
            else
                SECSGemState = SecsGemStateEnum.UNKNOWN;

            string sLog = String.Format("[XGEM ==> EQ] OnXGEMStateEvent:{0}", SECSGemState.ToString());

            if (SECSGemState == SecsGemStateEnum.EXECUTE)
            {
                GEMReqAllECInfo();

                // <remarks> Direct Excute Code 
                //var ret = SetEstablish(1);
                // </remarks> 
            }
            else
            {
                if (SECSCommState == SecsCommStateEnum.COMMUNICATING)
                {
                    this.SECSCommState = SecsCommStateEnum.COMM_DISABLED;
                }

                if (SECSCommState == SecsCommStateEnum.COMMUNICATING || SECSControlState == SecsControlStateEnum.ONLINE_LOCAL)
                {
                    this.SECSControlState = SecsControlStateEnum.EQ_OFFLINE;
                }
            }

            //callback = OperationContext.Current.GetCallbackChannel<ISecsGemServiceCallback>();

            try
            {
                lock (callbackLocker)
                {
                    callback?.OnGEMStateEvent(SECSGemState);
                }
            }
            catch
            {
                callback = null;
            }
            //callbackQueue.Enqueue(() => callback?.OnGEMStateEvent(SECSGemState.ToString()));
        }

        private void XGEM_OnGEMCommStateChange(long nState)
        {
            #region Gem Communication Number 설명
            //-1 : None
            // 1 : Comm Disabled
            // 2 : WaitCRFromHost
            // 3 : WaitDelay
            // 4 : WaitCRA 
            // 5 : Communicating
            #endregion
            if (Enum.IsDefined(typeof(SecsCommStateEnum), (int)nState))
                Enum.TryParse(nState.ToString(), out SECSCommState);
            else
                SECSCommState = SecsCommStateEnum.UNKNOWN;

            //callbackQueue.Enqueue(() => callback?.OnGEMCommStateChange(SECSCommState.ToString()));
            //callback = OperationContext.Current.GetCallbackChannel<ISecsGemServiceCallback>();

            try
            {
                string line = $"{DateTime.Now}[SecsControlStateEnum : {nState}]";
                WriteLog(line);


                lock (callbackLocker)
                {
                    callback?.OnGEMCommStateChange(SECSCommState);
                }
            }
            catch
            {
                Dispose();
            }
        }

        public long MakeObject(ref long pnMsgId)
        {
            return Gem?.MakeObject(ref pnMsgId) ?? -1;
        }

        public long SetListItem(long nMsgId, long nItemCount)
        {
            return Gem?.SetList(nMsgId, nItemCount) ?? -1;
        }

        public long SetBinaryItem(long nMsgId, byte nValue)
        {
            return Gem?.SetBinary(nMsgId, nValue) ?? -1;
        }

        public long SetBoolItem(long nMsgId, bool nValue)
        {
            return Gem?.SetBool(nMsgId, nValue) ?? -1;
        }

        public long SetBoolItems(long nMsgId, bool[] pnValue)
        {
            return Gem?.SetBoolItems(nMsgId, pnValue) ?? -1;
        }

        public long SetUint1Item(long nMsgId, byte nValue)
        {
            return Gem?.SetU1(nMsgId, nValue) ?? -1;
        }

        public long SetUint1Items(long nMsgId, byte[] pnValue)
        {
            return Gem?.SetU1(nMsgId, pnValue) ?? -1;
        }

        public long SetUint2Item(long nMsgId, ushort nValue)
        {
            return Gem?.SetU2(nMsgId, nValue) ?? -1;
        }

        public long SetUint2Items(long nMsgId, ushort[] pnValue)
        {
            return Gem?.SetU2(nMsgId, pnValue) ?? -1;
        }

        public long SetUint4Item(long nMsgId, uint nValue)
        {
            return Gem?.SetU4(nMsgId, nValue) ?? -1;
        }

        public long SetUint4Items(long nMsgId, uint[] pnValue)
        {
            return Gem?.SetU4(nMsgId, pnValue) ?? -1;
        }

        public long SetUint8Item(long nMsgId, ulong nValue)
        {
            return Gem?.SetUint8Item(nMsgId, nValue) ?? -1;
        }

        public long SetUint8Items(long nMsgId, ulong[] pnValue)
        {
            return Gem?.SetUint8Items(nMsgId, pnValue) ?? -1;
        }

        public long SetInt1Item(long nMsgId, sbyte nValue)
        {
            return Gem?.SetI1(nMsgId, nValue) ?? -1;
        }

        public long SetInt1Items(long nMsgId, sbyte[] pnValue)
        {
            return Gem?.SetI1(nMsgId, pnValue) ?? -1;
        }

        public long SetInt2Item(long nMsgId, short nValue)
        {
            return Gem?.SetI2(nMsgId, nValue) ?? -1;
        }

        public long SetInt2Items(long nMsgId, short[] pnValue)
        {
            return Gem?.SetI2(nMsgId, pnValue) ?? -1;
        }

        public long SetInt4Item(long nMsgId, int nValue)
        {
            return Gem?.SetI4(nMsgId, nValue) ?? -1;
        }

        public long SetInt4Items(long nMsgId, int[] pnValue)
        {
            return Gem?.SetI4(nMsgId, pnValue) ?? -1;
        }

        public long SetInt8Items(long nMsgId, long[] pnValue)
        {
            return Gem?.SetInt8Items(nMsgId, pnValue) ?? -1;
        }

        public long SetInt8Item(long nMsgId, long nValue)
        {
            return Gem?.SetInt8Item(nMsgId, nValue) ?? -1;
        }

        public long SetFloat4Item(long nMsgId, float nValue)
        {
            return Gem?.SetF4(nMsgId, nValue) ?? -1;
        }

        public long SetFloat4Items(long nMsgId, float[] pnValue)
        {
            return Gem?.SetF4(nMsgId, pnValue) ?? -1;

        }

        public long SetFloat8Item(long nMsgId, double nValue)
        {
            return Gem?.SetF8(nMsgId, nValue) ?? -1;

        }

        public long SetFloat8Items(long nMsgId, double[] pnValue)
        {
            return Gem?.SetF8(nMsgId, pnValue) ?? -1;
        }

        public long SetStringItem(long nMsgId, string pszValue)
        {
            return Gem?.SetAscii(nMsgId, pszValue) ?? -1;
        }

        public long GetListItem(long nMsgId, ref long pnItemCount)
        {
            return Gem?.GetList(nMsgId, ref pnItemCount) ?? -1;
        }

        public long GetBinaryItem(long nMsgId, ref byte[] pnValue)
        {
            return Gem?.GetBinary(nMsgId, ref pnValue) ?? -1;
        }

        public long GetBoolItem(long nMsgId, ref bool[] pnValue)
        {
            return Gem?.GetBool(nMsgId, ref pnValue) ?? -1;
        }

        public long GetUint1Item(long nMsgId, ref byte[] pnValue)
        {
            return Gem?.GetU1(nMsgId, ref pnValue) ?? -1;
        }

        public long GetUint2Item(long nMsgId, ref ushort[] pnValue)
        {
            return Gem?.GetU2(nMsgId, ref pnValue) ?? -1;
        }

        public long GetUint4Item(long nMsgId, ref uint[] pnValue)
        {
            return Gem?.GetU4(nMsgId, ref pnValue) ?? -1;
        }

        public long GetUint8Item(long nMsgId, ref ulong[] pnValue)
        {
            return Gem?.GetUint8Item(nMsgId, ref pnValue) ?? -1;
        }

        public long GetInt1Item(long nMsgId, ref sbyte[] pnValue)
        {
            return Gem?.GetInt1Item(nMsgId, ref pnValue) ?? -1;
        }

        public long GetInt2Item(long nMsgId, ref short[] pnValue)
        {
            return Gem?.GetI2(nMsgId, ref pnValue) ?? -1;
        }

        public long GetInt4Item(long nMsgId, ref int[] pnValue)
        {
            return Gem?.GetI4(nMsgId, ref pnValue) ?? -1;
        }

        public long GetInt8Item(long nMsgId, ref long[] pnValue)
        {
            return Gem?.GetInt8Item(nMsgId, ref pnValue) ?? -1;
        }

        public long GetFloat4Item(long nMsgId, ref float[] pnValue)
        {
            return Gem?.GetF4(nMsgId, ref pnValue) ?? -1;
        }

        public long GetFloat8Item(long nMsgId, ref double[] pnValue)
        {
            return Gem?.GetF8(nMsgId, ref pnValue) ?? -1;
        }

        public long GetStringItem(long nMsgId, ref string psValue)
        {
            return Gem?.GetAscii(nMsgId, ref psValue) ?? -1;
        }

        public long CloseObject(long nMsgId)
        {
            return Gem?.CloseObject(nMsgId) ?? -1;
        }

        public long SendSECSMessage(long nMsgId, long nStream, long nFunction, long nSysbyte)
        {
            return Gem?.SendSECSMessage(nMsgId, nStream, nFunction, nSysbyte) ?? -1;
        }

        public long GEMReqOffline()
        {
            return Gem?.GEMReqOffline() ?? -1;
        }

        public long GEMReqLocal()
        {
            return Gem?.GEMReqLocal() ?? -1;
        }

        public long GEMReqRemote()
        {
            return Gem?.GEMReqRemote() ?? -1;
        }

        public long GEMSetEstablish(long bState)
        {
            return Gem?.GEMSetEstablish(bState) ?? -1;
        }

        public long GEMSetParam(string sParamName, string sParamValue)
        {
            return Gem?.GEMSetParam(sParamName, sParamValue) ?? -1;
        }

        public long GEMGetParam(string sParamName, ref string psParamValue)
        {
            long retVal = -1;
            try
            {
                string[] splitParamName = null;
                StringBuilder setValue = new StringBuilder();
                string getData = string.Empty;
                string tmpData = string.Empty;

                splitParamName = sParamName.Split(',');

                if (splitParamName != null)
                {
                    foreach (var paramName in splitParamName)
                    {
                        if (!string.IsNullOrEmpty(paramName))
                        {
                            retVal = Gem?.GEMGetParam(paramName, ref getData) ?? -1;

                            if (retVal == 0)
                            {
                                tmpData = getData;
                            }
                            else
                            {
                                tmpData = $"Unknown(ErrCode:{retVal})";
                            }

                            if (setValue.Length != 0)
                            {
                                setValue.Append(',');
                            }
                            setValue.Append(tmpData);
                        }
                    }

                    psParamValue = setValue.ToString();
                }
                else
                {
                    retVal = 0;
                }
            }
            catch (Exception err)
            {
                WriteLog(err.Message);
            }
            return retVal;
        }

        public long GEMReqGetDateTime()
        {
            return Gem?.GEMReqGetDateTime() ?? -1;
        }

        public long GEMRspGetDateTime(long nMsgId, string sSystemTime)
        {
            return Gem?.GEMRspGetDateTime(nMsgId, sSystemTime) ?? -1;
        }

        public long GEMRspDateTime(long nMsgId, long nResult)
        {
            return Gem?.GEMRspDateTime(nMsgId, nResult) ?? -1;

        }

        public long GEMSetAlarm(long nID, long nState, int cellIndex = 0)
        {
            return Gem?.GEMSetAlarm(nID, nState) ?? -1;
        }

        public long ClearAlarmOnly(int stageNum = 0)
        {
            //do nothing
            return 0;
        }


        public long GEMRspRemoteCommand(long nMsgId, string sCmd, long nHCAck, long nCount, long[] pnResult)
        {
            return Gem?.GEMRspRemoteCommand(nMsgId, sCmd, nHCAck, nCount, pnResult) ?? -1;
        }

        public long GEMRspRemoteCommand2(long nMsgId, string sCmd, long nHCAck, long nCount, string[] psCpName, long[] pnCpAck)
        {
            return Gem?.GEMRspRemoteCommand2(nMsgId, sCmd, nHCAck, nCount, psCpName, pnCpAck) ?? -1;
        }

        public long GEMRspChangeECV(long nMsgId, long nResult)
        {
            return Gem?.GEMRspChangeECV(nMsgId, nResult) ?? -1;
        }

        public long GEMSetECVChanged(long nCount, long[] pnEcIds, string[] psEcVals, int stageNum = -1)
        {
            return Gem?.GEMSetECVChanged(nCount, pnEcIds, psEcVals) ?? -1;
        }

        public long GEMEnableLog(long bEnabled)
        {
            return Gem?.GEMEnableLog(bEnabled) ?? -1;
        }

        public long GEMSetLogOption(string sDriectory, string sPrefix, string sExtension, long nKeepDay, long bMakeDailyLog, long bMakeSubDirectory)
        {
            return Gem?.GEMSetLogOption(sDriectory, sPrefix, sExtension, nKeepDay, bMakeDailyLog, bMakeSubDirectory) ?? -1;
        }

        public long GEMReqAllECInfo()
        {
            return Gem?.GEMReqAllECInfo() ?? -1;
        }

        public long GEMSetPPChanged(long nMode, string sPpid, long nLength, string pbBody)
        {
            return Gem?.GEMSetPPChanged(nMode, sPpid, nLength, pbBody) ?? -1;
        }

        public long GEMSetPPFmtChanged(long nMode, string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames)
        {
            return Gem?.GEMSetPPFmtChanged(nMode, sPpid, sMdln, sSoftRev, nCount, psCCode, pnParamCount, psParamNames) ?? -1;
        }

        public long GEMReqPPLoadInquire(string sPpid, long nLength)
        {
            return Gem?.GEMReqPPLoadInquire(sPpid, nLength) ?? -1;
        }

        public long GEMRspPPLoadInquire(long nMsgId, string sPpid, long nResult)
        {
            return Gem?.GEMRspPPLoadInquire(nMsgId, sPpid, nResult) ?? -1;

        }

        public long GEMReqPPSend(string sPpid, string pbBody)
        {
            return Gem?.GEMReqPPSend(sPpid, pbBody) ?? -1;

        }

        public long GEMRspPPSend(long nMsgId, string sPpid, long nResult)
        {
            return Gem?.GEMRspPPSend(nMsgId, sPpid, nResult) ?? -1;

        }

        public long GEMReqPP(string sPpid)

        {
            return Gem?.GEMReqPP(sPpid) ?? -1;

        }

        public long GEMRspPP(long nMsgId, string sPpid, string pbBody)
        {
            return Gem?.GEMRspPP(nMsgId, sPpid, pbBody) ?? -1;
        }

        public long GEMRspPPDelete(long nMsgId, long nCount, string[] psPpids, long nResult)
        {
            return Gem?.GEMRspPPDelete(nMsgId, nCount, psPpids, nResult) ?? -1;
        }

        public long GEMRspPPList(long nMsgId, long nCount, string[] psPpids)
        {
            return Gem?.GEMRspPPList(nMsgId, nCount, psPpids) ?? -1;
        }

        public long GEMReqPPFmtSend(string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames)
        {
            return Gem?.GEMReqPPFmtSend(sPpid, sMdln, sSoftRev, nCount, psCCode, pnParamCount, psParamNames) ?? -1;
        }

        public long GEMReqPPFmtSend2(string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames, string[] psParamValues)
        {
            return Gem?.GEMReqPPFmtSend2(sPpid, sMdln, sSoftRev, nCount, psCCode, pnParamCount, psParamNames, psParamValues) ?? -1;
        }

        public long GEMRspPPFmtSend(long nMsgId, string sPpid, long nResult)
        {
            return Gem?.GEMRspPPFmtSend(nMsgId, sPpid, nResult) ?? -1;
        }

        public long GEMReqPPFmt(string sPpid)
        {
            return Gem?.GEMReqPPFmt(sPpid) ?? -1;
        }

        public long GEMRspPPFmt(long nMsgId, string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames)
        {
            return Gem?.GEMRspPPFmt(nMsgId, sPpid, sMdln, sSoftRev, nCount, psCCode, pnParamCount, psParamNames) ?? -1;
        }

        public long GEMRspPPFmt2(long nMsgId, string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames, string[] psParamValues)
        {
            return Gem?.GEMRspPPFmt2(nMsgId, sPpid, sMdln, sSoftRev, nCount, psCCode, pnParamCount, psParamNames, psParamValues) ?? -1;

        }

        public long GEMReqPPFmtVerification(string sPpid, long nCount, long[] pnAck, string[] psSeqNumber, string[] psError)
        {
            return Gem?.GEMReqPPFmtVerification(sPpid, nCount, pnAck, psSeqNumber, psError) ?? -1;

        }

        public long GEMSetTerminalMessage(long nTid, string sMsg)
        {
            return Gem?.GEMSetTerminalMessage(nTid, sMsg) ?? -1;

        }


        public long GEMGetVariable(long nCount, ref long[] pnVid, ref string[] psValue)
        {
            return Gem?.GEMGetVariable(nCount, ref pnVid, ref psValue) ?? -1;
        }

        public long GEMSetVariables(long nObjectID, long nVid, int stageNum = -1, bool immediatelyUpdate = false)
        {
            return Gem?.GEMSetVariables(nObjectID, nVid) ?? -1;
        }

        public long GEMSetVarName(long nCount, string[] psVidName, string[] psValue)
        {
            return Gem?.GEMSetVarName(nCount, psVidName, psValue) ?? -1;
        }

        public long GEMGetVarName(long nCount, ref string[] psVidName, ref string[] psValue)
        {
            return Gem?.GEMGetVarName(nCount, ref psVidName, ref psValue) ?? -1;
        }

        public long GEMSetEvent(long nEventID, int stageNum = -1)
        {
            try
            {
                lock (Gem)
                {
                    return Gem?.GEMSetEvent(nEventID) ?? -1;
                }
            }
            catch (Exception err)
            {
                WriteLog(err.Message);
                return -1;
            }

        }

        public long GEMSetSpecificMessage(long nMsgId, string sMsgName)
        {
            return Gem?.GEMSetSpecificMessage(nMsgId, sMsgName) ?? -1;
        }

        public long GEMGetSpecificMessage(long nSObjectID, ref long pnRObjectID, string sMsgName)
        {
            return Gem?.GEMGetSpecificMessage(nSObjectID, ref pnRObjectID, sMsgName) ?? -1;
        }

        public long GetAllStringItem(long nMsgId, ref string psValue)
        {
            return Gem?.GetAllStringItem(nMsgId, ref psValue) ?? -1;
        }

        public long SetAllStringItem(long nMsgId, string sValue)
        {
            return Gem?.SetAllStringItem(nMsgId, sValue) ?? -1;
        }

        public long GEMReqPPSendEx(string sPpid, string sRecipePath)
        {
            return Gem?.GEMReqPPSendEx(sPpid, sRecipePath) ?? -1;
        }

        public long GEMRspPPSendEx(long nMsgId, string sPpid, string sRecipePath, long nResult)
        {
            return Gem?.GEMRspPPSendEx(nMsgId, sPpid, sRecipePath, nResult) ?? -1;
        }

        public long GEMReqPPEx(string sPpid, string sRecipePath)
        {
            return Gem?.GEMReqPPEx(sPpid, sRecipePath) ?? -1;
        }

        public long GEMRspPPEx(long nMsgId, string sPpid, string sRecipePath)
        {

            return Gem?.GEMRspPPEx(nMsgId, sPpid, sRecipePath) ?? -1;
        }

        public long GEMSetVariableEx(long nMsgId, long nVid)
        {
            return Gem?.GEMSetVariableEx(nMsgId, nVid) ?? -1;
        }

        public long GEMReqLoopback(long nCount, long[] pnAbs)
        {
            return Gem?.GEMReqLoopback(nCount, pnAbs) ?? -1;
        }

        public long GEMSetEventEnable(long nCount, long[] pnCEIDs, long nEnable)
        {
            return Gem?.GEMSetEventEnable(nCount, pnCEIDs, nEnable) ?? -1;
        }

        public long GEMSetAlarmEnable(long nCount, long[] pnALIDs, long nEnable)
        {
            return Gem?.GEMSetAlarmEnable(nCount, pnALIDs, nEnable) ?? -1;

        }

        public long GEMGetEventEnable(long nCount, long[] pnCEIDs, ref long[] pnEnable)
        {
            return Gem?.GEMGetEventEnable(nCount, pnCEIDs, ref pnEnable) ?? -1;

        }

        public long GEMGetAlarmEnable(long nCount, long[] pnALIDs, ref long[] pnEnable)
        {
            return Gem?.GEMGetAlarmEnable(nCount, pnALIDs, ref pnEnable) ?? -1;

        }

        public long GEMGetAlarmInfo(long nCount, long[] pnALIDs, ref long[] pnALCDs, ref string[] psALTXs)
        {
            return Gem?.GEMGetAlarmInfo(nCount, pnALIDs, ref pnALCDs, ref psALTXs) ?? -1;

        }

        public long GEMGetSVInfo(long nCount, long[] pnSVIDs, ref string[] psMins, ref string[] psMaxs)
        {
            return Gem?.GEMGetSVInfo(nCount, pnSVIDs, ref psMins, ref psMaxs) ?? -1;

        }

        public long GEMGetECVInfo(long nCount, long[] pnECIDs, ref string[] psNames, ref string[] psDefs, ref string[] psMins, ref string[] psMaxs, ref string[] psUnits)
        {
            return Gem?.GEMGetECVInfo(nCount, pnECIDs, ref psNames, ref psDefs, ref psMins, ref psMaxs, ref psUnits) ?? -1;
        }

        public long GEMRsqOffline(long nMsgId, long nAck)
        {
            return Gem?.GEMRsqOffline(nMsgId, nAck) ?? -1;
        }

        public long GEMRspOnline(long nMsgId, long nAck)
        {
            return Gem?.GEMRspOnline(nMsgId, nAck) ?? -1;
        }

        public long GEMReqHostOffline()
        {
            return Gem?.GEMReqHostOffline() ?? -1;
        }

        public long GEMReqStartPolling(string sName, long nScanTime)
        {
            return Gem?.GEMReqStartPolling(sName, nScanTime) ?? -1;
        }

        public long GEMReqStopPolling(string sName)
        {
            return Gem?.GEMReqStopPolling(sName) ?? -1;
        }

        public long GEMEQInitialized(long nInitType)
        {
            return Gem?.GEMEQInitialized(nInitType) ?? -1;
        }

        public long GEMSetVariable(long nCount, long[] pnVid, string[] psValue, int stageNum = -1, bool immediatelyUpdate = false)
        {
            return Gem?.GEMSetVariable(nCount, pnVid, psValue) ?? -1;
        }

        //Add
        public long SetBinaryItems(long nObjectID, byte[] pnValue)
        {
            return Gem.SetBinaryItems(nObjectID, pnValue);
        }
        public long GetCurrentItemInfo(long nObjectID, ref long pnItemType, ref long pnItemCount)
        {
            return Gem.GetCurrentItemInfo(nObjectID, ref pnItemType, ref pnItemCount);
        }
        public long GEMRspRemoteCommand(long nMsgId, string sCmd, long nHCAck, long nCount, string[] psCpName, long[] pnCpAck)
        {
            return Gem?.GEMRspRemoteCommand(nMsgId, sCmd, nHCAck, nCount, psCpName, pnCpAck) ?? -1;
        }
        public long GEMSetPPChanged(long nMode, string sPpid, long nSize, byte[] baBody)
        {
            return Gem.GEMSetPPChanged(nMode, sPpid, nSize, baBody);
        }
        public long GEMReqPPSend(string sPpid, byte[] baBody)
        {
            return Gem.GEMReqPPSend(sPpid, baBody);
        }
        public long GEMRspPP(long nMsgId, string sPpid, byte[] baBody)
        {
            return Gem.GEMRspPP(nMsgId, sPpid, baBody);
        }
        public bool GetActive()
        {
            return Gem.GetActive();
        }
        public string GetIP()
        {
            return Gem.GetIP();
        }
        public long GetPort()
        {
            return Gem.GetPort();
        }
        public void SetIP(string sNewValue)
        {
            Gem.SetIP(sNewValue);
        }
        public void SetPort(long nNewValue)
        {
            Gem.SetPort(nNewValue);
        }
        public void SetActive(bool bNewValue)
        {
            Gem.SetActive(bNewValue);
        }
        public long Initialize(string sCfg)
        {
            return Gem.Initialize(sCfg);
        }
        #endregion

        #region GEM300Pro
        /// <summary>
        ///     존재하는 모든 Carrier Object 에 대해 XGem 으로 삭제 요청합니다.
        /// </summary>
        public long CMSDelAllCarrierInfo()
        {
            return Gem.CMSDelAllCarrierInfo();
            //return m_XGem.CMSDelAllCarrierInfo();
        }

        /// <summary>
        ///     존재하는 CarrierID 정보를 XGem 으로 삭제 요청합니다.
        /// </summary>
        /// <remarks>
        /// E -> H : CMSDelAllCarrierInfo() => H -> E : XGEM_OnCMSCarrierDeleted()
        /// </remarks>
        public long CMSDelCarrierInfo(string sCarrierID)
        {
            return Gem.CMSDelCarrierInfo(sCarrierID);
        }

        /// <summary>
        ///     XGem 에 등록되어 있는 모든 Carrier 의 정보를 획득합니다.
        /// </summary>
        /// <remarks>
        /// E -> H : CMSDelAllCarrierInfo() => 
        /// H -> E : XGEM_OnGetCarrierID(), XGEM_OnGetCarrierID(), XGEM_OnGetCarrierIdStatus(), XGEM_OnGetCarrierSlotMapStatus(),
        ///          XGEM_OnGetCarrierAccessingStatus(), XGEM_OnGetCarrierContentsMapCount(), XGEM_OnGetCarrierContentsMap(), XGEM_OnGetCarrierUsage()
        /// </remarks>
        /// <param name="pnMsgId"> 전체 Carrier ObjectID 를 받을 변수 </param>
        /// <param name="pnCount"> Carrier 개수를 받을 변수 </param>
        public long CMSGetAllCarrierInfo(ref long pnMsgId, ref long pnCount)
        {
            return Gem.CMSGetAllCarrierInfo(ref pnMsgId, ref pnCount);
        }
        public long CMSReqBind(string sLocID, string sCarrierID, string sSlotMap)
        {
            return Gem.CMSReqBind(sLocID, sCarrierID, sSlotMap);
        }
        public long CMSReqCancelBind(string sLocID, string sCarrierID)
        {
            return Gem.CMSReqCancelBind(sLocID, sCarrierID);
        }
        public long CMSReqCancelCarrier(string sLocID, string sCarrierID)
        {
            return Gem.CMSReqCancelCarrier(sLocID, sCarrierID);
        }
        public long CMSReqCarrierIn(string sLocID, string sCarrierID)
        {
            return Gem.CMSReqCarrierIn(sLocID, sCarrierID);
        }
        public long CMSReqCarrierOut(string sLocID, string sCarrierID)
        {
            return Gem.CMSReqCarrierOut(sLocID, sCarrierID);
        }
        public long CMSReqCarrierReCreate(string sLocID, string sCarrierID)
        {
            return Gem.CMSReqCarrierReCreate(sLocID, sCarrierID);
        }
        public long CMSReqChangeAccess(long nMode, string sLocID)
        {
            return Gem.CMSReqChangeAccess(nMode, sLocID);
        }
        public long CMSReqChangeServiceStatus(string sLocID, long nState)
        {
            return Gem.CMSReqChangeServiceStatus(sLocID, nState);
        }
        public long CMSReqProceedCarrier(string sLocID, string sCarrierID, string sSlotMap, long nCount, string[] psLotID, string[] psSubstrateID, string sUsage)
        {
            return Gem.CMSReqProceedCarrier(sLocID, sCarrierID, sSlotMap, nCount, psLotID, psSubstrateID, sUsage);
        }
        public long CMSRspCancelCarrier(long nMsgId, string sLocID, string sCarrierID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return Gem.CMSRspCancelCarrier(nMsgId, sLocID, sCarrierID, nResult, nErrCount, pnErrCode, psErrText);
        }
        public long CMSRspCancelCarrierAtPort(long nMsgId, string sLocID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return Gem.CMSRspCancelCarrierAtPort(nMsgId, sLocID, nResult, nErrCount, pnErrCode, psErrText);
        }
        public long CMSRspCancelCarrierOut(long nMsgId, string sCarrierID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return Gem.CMSRspCancelCarrierOut(nMsgId, sCarrierID, nResult, nErrCount, pnErrCode, psErrText);
        }
        public long CMSRspCarrierIn(long nMsgId, string sLocID, string sCarrierID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return Gem.CMSRspCarrierIn(nMsgId, sLocID, sCarrierID, nResult, nErrCount, pnErrCode, psErrText);
        }
        public long CMSRspCarrierOut(long nMsgId, string sLocID, string sCarrierID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return Gem.CMSRspCarrierOut(nMsgId, sLocID, sCarrierID, nResult, nErrCount, pnErrCode, psErrText);
        }
        public long CMSRspCarrierRelease(long nMsgId, string sLocID, string sCarrierID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return Gem.CMSRspCarrierRelease(nMsgId, sLocID, sCarrierID, nResult, nErrCount, pnErrCode, psErrText);
        }
        public long CMSRspCarrierTagReadData(long nMsgId, string sLocID, string sCarrierID, string sData, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return Gem.CMSRspCarrierTagReadData(nMsgId, sLocID, sCarrierID, sData, nResult, nErrCount, pnErrCode, psErrText);
        }
        public long CMSRspCarrierTagWriteData(long nMsgId, string sLocID, string sCarrierID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return Gem.CMSRspCarrierTagWriteData(nMsgId, sLocID, sCarrierID, nResult, nErrCount, pnErrCode, psErrText);
        }
        public long CMSRspChangeAccess(long nMsgId, long nMode, long nResult, long nErrCount, string[] psLocID, long[] pnErrCode, string[] psErrText)
        {
            return Gem.CMSRspChangeAccess(nMsgId, nMode, nResult, nErrCount, psLocID, pnErrCode, psErrText);
        }
        public long CMSRspChangeServiceStatus(long nMsgId, string sLocID, long nState, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return Gem.CMSRspChangeServiceStatus(nMsgId, sLocID, nState, nResult, nErrCount, pnErrCode, psErrText);
        }
        public long CMSSetBufferCapacityChanged(string sPartID, string sPartType, long nAPPCapacity, long nPCapacity, long nUnPCapacity)
        {
            return Gem.CMSSetBufferCapacityChanged(sPartID, sPartType, nAPPCapacity, nPCapacity, nUnPCapacity);
        }
        public long CMSSetCarrierAccessing(string sLocID, long nState, string sCarrierID)
        {
            return Gem.CMSSetCarrierAccessing(sLocID, nState, sCarrierID);
        }
        public long CMSSetCarrierID(string sLocID, string sCarrierID, long nResult)
        {
            return Gem.CMSSetCarrierID(sLocID, sCarrierID, nResult);
        }
        public long CMSSetCarrierIDStatus(string sCarrierID, long nState)
        {
            return Gem.CMSSetCarrierIDStatus(sCarrierID, nState);
        }
        public long CMSSetCarrierInfo(string sCarrierID, string sLocID, long nIdStatus, long nSlotMapStatus, long nAccessingStatus, string sSlotMap, long nContentsMapCount, string[] psLotID, string[] psSubstrateID, string sUsage)
        {
            return Gem.CMSSetCarrierInfo(sCarrierID, sLocID, nIdStatus, nSlotMapStatus, nAccessingStatus, sSlotMap, nContentsMapCount, psLotID, psSubstrateID, sUsage);
        }
        public long CMSSetCarrierLocationInfo(string sLocID, string sCarrierID)
        {
            return Gem.CMSSetCarrierLocationInfo(sLocID, sCarrierID);
        }
        public long CMSSetCarrierMovement(string sLocID, string sCarrierID)
        {
            return Gem.CMSSetCarrierMovement(sLocID, sCarrierID);
        }
        public long CMSSetCarrierOnOff(string sLocID, long nState)
        {
            return Gem.CMSSetCarrierOnOff(sLocID, nState);
        }
        public long CMSSetCarrierOutStart(string sLocID, string sCarrierID)
        {
            return Gem.CMSSetCarrierOutStart(sLocID, sCarrierID);
        }
        public long CMSSetLPInfo(string sLocID, long nTransferState, long nAccessMode, long nReservationState, long nAssociationState, string sCarrierID)
        {
            return Gem.CMSSetLPInfo(sLocID, nTransferState, nAccessMode, nReservationState, nAssociationState, sCarrierID);
        }
        public long CMSSetMaterialArrived(string sMaterialID)
        {
            return Gem.CMSSetMaterialArrived(sMaterialID);
        }
        public long CMSSetPIOSignalState(string sLocID, long nSignal, long nState)
        {
            return Gem.CMSSetPIOSignalState(sLocID, nSignal, nState);
        }
        public long CMSSetPresenceSensor(string sLocID, long nState)
        {
            return Gem.CMSSetPresenceSensor(sLocID, nState);
        }
        public long CMSSetReadyToLoad(string sLocID)
        {
            return Gem.CMSSetReadyToLoad(sLocID);
        }
        public long CMSSetReadyToUnload(string sLocID)
        {
            return Gem.CMSSetReadyToUnload(sLocID);
        }
        public long CMSSetSlotMap(string sLocID, string sSlotMap, string sCarrierID, long nResult)
        {
            return Gem.CMSSetSlotMap(sLocID, sSlotMap, sCarrierID, nResult);
        }
        public long CMSSetSlotMapStatus(string sCarrierID, long nState)
        {
            return Gem.CMSSetSlotMapStatus(sCarrierID, nState);
        }
        public long CMSSetSubstrateCount(string sCarrierID, long nSubstCount)
        {
            return Gem.CMSSetSubstrateCount(sCarrierID, nSubstCount);
        }
        public long CMSSetTransferReady(string sLocID, long nState)
        {
            return Gem.CMSSetTransferReady(sLocID, nState);
        }
        public long CMSSetUsage(string sCarrierID, string sUsage)
        {
            return Gem.CMSSetUsage(sCarrierID, sUsage);
        }
        public long PJDelAllJobInfo()
        {
            return Gem.PJDelAllJobInfo();
        }
        public long PJDelJobInfo(string sPJobID)
        {
            return Gem.PJDelJobInfo(sPJobID);
        }
        public long PJGetAllJobInfo(ref long pnObjID, ref long pnPJobCount)
        {
            return Gem.PJGetAllJobInfo(ref pnObjID, ref pnPJobCount);
        }
        public long PJReqCommand(long nCommand, string sPJobID)
        {
            return Gem.PJReqCommand(nCommand, sPJobID);
        }
        public long PJReqCreate(string sPJobID, long nMtrlFormat, long nAutoStart, long nMtrlOrder, long nMtrlCount, string[] psMtrlID, string[] psSlotInfo, long nRcpMethod, string sRcpID, long nRcpParCount, string[] psRcpParName, string[] psRcpParValue)
        {
            return Gem.PJReqCreate(sPJobID, nMtrlFormat, nAutoStart, nMtrlOrder, nMtrlCount, psMtrlID, psSlotInfo, nRcpMethod, sRcpID, nRcpParCount, psRcpParName, psRcpParValue);
        }
        public long PJReqCreateEx(string sPJobID, long nMtrlFormat, long nAutoStart, long nMtrlOrder, long nMtrlCount, string[] psMtrlID, string[] psSlotInfo, long nRcpMethod, string sRcpID, long nRcpParCount, string[] psRcpParName, long[] pnRcpParValue)
        {
            return Gem.PJReqCreateEx(sPJobID, nMtrlFormat, nAutoStart, nMtrlOrder, nMtrlCount, psMtrlID, psSlotInfo, nRcpMethod, sRcpID, nRcpParCount, psRcpParName, pnRcpParValue);
        }
        public long PJReqGetAllJobID()
        {
            return Gem.PJReqGetAllJobID();
        }
        public long PJReqGetJob(string sPJobID)
        {
            return Gem.PJReqGetJob(sPJobID);
        }
        public long PJRspCommand(long nMsgID, long nCommand, string sPJobID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return Gem.PJRspCommand(nMsgID, nCommand, sPJobID, nResult, nErrCount, pnErrCode, psErrText);
        }
        public long PJRspSetMtrlOrder(long nMsgID, long nResult)
        {
            return Gem.PJRspSetMtrlOrder(nMsgID, nResult);
        }
        public long PJRspSetRcpVariable(long nMsgID, string sPJobID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return Gem.PJRspSetRcpVariable(nMsgID, sPJobID, nResult, nErrCount, pnErrCode, psErrText);
        }
        public long PJRspSetStartMethod(long nMsgID, long nPJobCount, string[] psPJobID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return Gem.PJRspSetStartMethod(nMsgID, nPJobCount, psPJobID, nResult, nErrCount, pnErrCode, psErrText);
        }

        /// <summary>
        ///     Host 에서 S16F11 혹은 S16F15 로 Process Job Create Service 를 전송 받았을 때 
        ///     EQ 로 Process Job의 생성 요청을 할 때 PJReqVerify 이벤트 가 발생하는데, PJReqVerify 이벤트 에 대한 응답으로 사용된다.
        /// </summary>
        /// <remarks>
        ///     E -> H
        ///     S16F12(PRJobCreateEnh Acknowledge), S16F16(PRJobMultiCreate Acknowledge)
        ///     First Create    :   2021-01-11, Semics R&D1, Leina Han.
        ///                             ///..내용 
        /// </remarks>                        
        /// <param name="nMsgID"> XGem Process 와 통신 시 사용되는Message Id 입니다.PJReqVerify 이벤트의 nMsgID 를 사용합니다. </param>
        /// <param name="nPJobCount"> Process Job Count </param>
        /// <param name="psPJobID"> Process Job id list 의 배열 변수 </param>
        /// <param name="nResult"> Process Job 생성 요청의 결과 0: SUCCESS , 1: FAILparam>
        /// <param name="nErrCount"> nResult 가 failed 일 때 발생되는 error count </param>
        /// <param name="pnErrCode"> Error Code list 의 배열 변수 </param>
        /// <param name="psErrText"> Error Code list 의 배열 변수 </param>
        public long PJRspVerify(long nMsgID, long nPJobCount, string[] psPJobID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return Gem.PJRspVerify(nMsgID, nPJobCount, psPJobID, nResult, nErrCount, pnErrCode, psErrText);
        }
        public long PJSetJobInfo(string sPJobID, long nMtrlFormat, long nAutoStart, long nMtrlOrder, long nMtrlCount, string[] psMtrlID, string[] psSlotInfo, long nRcpMethod, string sRcpID, long nRcpParCount, string[] psRcpParName, string[] psRcpParVal)
        {
            return Gem.PJSetJobInfo(sPJobID, nMtrlFormat, nAutoStart, nMtrlOrder, nMtrlCount, psMtrlID, psSlotInfo, nRcpMethod, sRcpID, nRcpParCount, psRcpParName, psRcpParVal);
        }
        public long PJSetJobInfoEx(string sPJobID, long nMtrlFormat, long nAutoStart, long nMtrlOrder, long nMtrlCount, string[] psMtrlID, string[] psSlotInfo, long nRcpMethod, string sRcpID, long nRcpParCount, string[] psRcpParName, long[] pnRcpParVal)
        {
            return Gem.PJSetJobInfoEx(sPJobID, nMtrlFormat, nAutoStart, nMtrlOrder, nMtrlCount, psMtrlID, psSlotInfo, nRcpMethod, sRcpID, nRcpParCount, psRcpParName, pnRcpParVal);
        }
        public long PJSetState(string sPJobID, long nState)
        {
            return Gem.PJSetState(sPJobID, nState);
        }
        public long PJSettingUpCompt(string sPJobID)
        {
            return Gem.PJSettingUpCompt(sPJobID);
        }
        public long PJSettingUpStart(string sPJobID)
        {
            return Gem.PJSettingUpStart(sPJobID);
        }
        public long RangeCheck(long[] pnIds)
        {
            return Gem.RangeCheck(pnIds);
        }
        public bool ReadXGemInfoFromRegistry()
        {
            return Gem.ReadXGemInfoFromRegistry();
        }
        public int RunProcess(string sName, string sCfgPath, string sPassword)
        {
            return Gem.RunProcess(sName, sCfgPath, sPassword);
        }
        /// <summary>
        ///     XGem 에 존재하는 모든 ControlJob 정보를 삭제합니다.
        /// </summary>
        public long CJDelAllJobInfo()
        {
            return Gem.CJDelAllJobInfo();
        }

        /// <summary>
        ///     지정 ControlJob 을 삭제하고자 할 때 사용합니다.
        /// </summary>
        /// <param name="sCJobID"></param>
        public long CJDelJobInfo(string sCJobID)
        {
            return Gem.CJDelJobInfo(sCJobID);
        }

        /// <summary>
        ///     XGem 에서 등록되어 있는 ControlJob 의 모든 정보를 가져올 때 사용합니다.
        /// </summary>
        /// <param name="pnObjID"> 전체 ControlJob ObjectID 를 받을 변수 </param>
        /// <param name="pnCount"> ControlJob 개수를 받을 변수 </param>
        public long CJGetAllJobInfo(ref long pnObjID, ref long pnCount)
        {
            long nReturn = 0;
            string sLog = "";
            long nObjID = 0;
            long nCJobCount = 0;

            string sCJobID = "";
            long nState = 0;
            long nAutoStart = 0;
            long nPRJobCount = 0;

            nReturn = Gem.CJGetAllJobInfo(ref nObjID, ref nCJobCount);
            if (nReturn == 0)
            {
                AddMessage("[EQ ==> XGEM] Send CJGetAllJobInfo successfully");

                for (int i = 0; i <= nCJobCount - 1; i++)
                {
                    nReturn = GetCtrlJobID(nObjID, i, ref sCJobID);
                    if (nReturn == 0)
                    {
                        sLog = String.Format("Send GetCtrlJobID successfully CJobID={0}", sCJobID);
                        this.AddMessage(sLog);
                    }
                    else
                    {
                        sLog = String.Format("Fail to GetCtrlJobID ({0})", nReturn);
                        this.AddMessage(sLog);
                    }

                    nReturn = GetCtrlJobState(nObjID, i, ref nState);
                    if (nReturn == 0)
                    {
                        sLog = String.Format("Send GetCtrlJobState successfully State={0}", nState);
                        this.AddMessage(sLog);
                    }
                    else
                    {
                        sLog = String.Format("Fail to GetCtrlJobState ({0})", nReturn);
                        this.AddMessage(sLog);
                    }

                    nReturn = GetCtrlJobStartMethod(nObjID, i, ref nAutoStart);
                    if (nReturn == 0)
                    {
                        sLog = String.Format("Send GetCtrlJobStartMethod successfully AutoStart={0}",
                        nAutoStart);
                        this.AddMessage(sLog);
                    }
                    else
                    {
                        sLog = String.Format("Fail to GetCtrlJobStartMethod ({0})", nReturn);
                        this.AddMessage(sLog);
                    }

                    nReturn = GetCtrlJobPRJobCount(nObjID, i, ref nPRJobCount);
                    if (nReturn == 0)
                    {
                        sLog = String.Format("Send GetCtrlJobPRJobCount successfully PRJobCount ={0}", nPRJobCount);
                        this.AddMessage(sLog);
                    }
                    else
                    {
                        sLog = String.Format("Fail to GetCtrlJobPRJobCount ({0})", nReturn);
                        this.AddMessage(sLog);
                    }

                    string[] saPRJobID = new string[nPRJobCount];
                    nReturn = GetCtrlJobPRJobIDs(nObjID, i, nPRJobCount, ref saPRJobID);
                    if (nReturn == 0)
                    {
                        sLog = String.Format("Send GetCtrlJobPRJobIDs successfully PRJobCount ={0}", nPRJobCount);
                        this.AddMessage(sLog);
                        for (int j = 0; j <= nPRJobCount - 1; j++)
                        {
                            sLog = String.Format("PRJobID={0}", saPRJobID[j]);
                            this.AddMessage(sLog);
                        }
                    }
                    else
                    {
                        sLog = String.Format("Fail to GetCtrlJobPRJobIDs ({0})", nReturn);
                        this.AddMessage(sLog);
                    }

                }
                GetCtrlJobClose(nObjID);
            }
            else
            {
                sLog = String.Format("[EQ ==> XGEM] Fail to CJGetHOQJob ({0})", nReturn);
                this.AddMessage(sLog);
            }
            return nReturn;

        }

        /// <summary>
        ///     XGem 에서 Head of Queue 상태인 ControlJob ID 를 요청합니다.
        /// </summary>
        /// <returns></returns>
        public long CJGetHOQJob()
        {
            return Gem.CJGetHOQJob();
        }

        /// <summary>
        ///     EQ 에서 생성된 Control Job 에 대해 Command 를 요청할 경우 사용됩니다.
        /// </summary>
        /// <param name="sCJobID"> Control Job id </param>
        /// <param name="nCommand">
        ///             1: START
        ///             2: PAUSE
        ///             3: RESUME
        ///             4: CANCEL
        ///             5: DESELECT
        ///             6: STOP
        ///             7: ABORT
        ///             8: Head of Queue
        /// </param>
        /// <param name="sCPName">Command parameters
        ///             “SAVEJOBS”
        ///             “REMOVEJOBS”
        /// </param>
        /// <param name="sCPVal"> 
        /// 0 = SAVEJOBS. This command does not destroy the
        /// Process Jobs specified by this Control Job.
        /// 1 = REMOVEJOBS.This command destroys all
        /// Process Jobs specified by this Control Job.
        /// </param>
        /// <returns></returns>
        public long CJReqCommand(string sCJobID, long nCommand, string sCPName, string sCPVal)
        {
            return Gem.CJReqCommand(sCJobID, nCommand, sCPName, sCPVal);
        }

        /// <summary>
        ///     EQ 에서 ControlJob 의 생성을 XGem 으로 요청할 때 사용합니다.
        /// </summary>
        /// <param name="sCJobID"> Control Job id </param>
        /// <param name="nStartMethod"> 0: UserStart, 1: AutoStart </param>
        /// <param name="nCountPRJob"> Control Job 에 연결할 Process Job id(s) </param>
        /// <param name="psPRJobID"> Process Job list 를 전송할 배열 변수 </param>
        /// <returns></returns>
        public long CJReqCreate(string sCJobID, long nStartMethod, long nCountPRJob, string[] psPRJobID)
        {
            return Gem.CJReqCreate(sCJobID, nStartMethod, nCountPRJob, psPRJobID);
        }

        /// <summary>
        ///     EQ 에서 XGem 에 등록되어 있는 모든 ControlJobID 를 확인하고자 할 때 사용합니다.
        /// </summary>
        public long CJReqGetAllJobID()
        {
            return Gem.CJReqGetAllJobID();
        }

        /// <summary>
        ///     EQ 에서 Control Job 의 정보를 요구할 때 사용합니다
        /// </summary>
        /// <param name="sCJobID"> Control Job id </param>
        public long CJReqGetJob(string sCJobID)
        {
            return Gem.CJReqGetJob(sCJobID);
        }

        /// <summary>
        ///     요청한 ControlJob 을 Head of Queue 상태로의 전환을 요청합니다.
        /// </summary>
        public long CJReqHOQJob(string sCJobID)
        {
            return Gem.CJReqHOQJob(sCJobID);
        }

        /// <summary>
        ///     EQ 에서 진행할 ControlJob 을 선택하기 위해 사용합니다.
        /// </summary>
        public long CJReqSelect(string sCJobID)
        {
            return Gem.CJReqSelect(sCJobID);
        }

        /// <summary>
        ///     S16F28 (H->E) Control Job Command Acknowledge
        /// </summary>
        /// <param name="nMsgId"> Message ID </param>
        /// <param name="sCJobID"> Control Job ID </param>
        /// <param name="nCommand"> ACKA 와 동일한 값을 가집니다. 0(FALSE): Failed , 1(TRUE) : Successful </param>
        /// <param name="nResult"> nResult 가 failed 일 때 발생되는 error count </param>
        /// <param name="nErrCode"> Error 가 발생했을 때 Error Code 배열 변수 </param>
        /// <param name="sErrText"> Error 가 발생했을 때 Error Text 배열 변수 </param>
        /// <returns></returns>
        public long CJRspCommand(long nMsgId, string sCJobID, long nCommand, long nResult, long nErrCode, string sErrText)
        {
            return Gem.CJRspCommand(nMsgId, sCJobID, nCommand, nResult, nErrCode, sErrText);
        }

        /// <summary>
        ///     S14F10 (HE) Create Object Acknowledge (COA)
        /// </summary>
        /// <param name="nMsgId"> XGem Process 와 통신 시 사용되는 Message Id 입니다. CJReqVerify 이벤트의 nMsgID 를 사용합니다. </param>
        /// <param name="sCJobID"> Control job </param>
        /// <param name="nResult"> OBJACK 와 동일한 값을 가집니다. 0 : SUCCESS, 1 : FAIL, >1 Reserved </param>
        /// <param name="nErrCount"> nResult 가 failed 일 때 발생되는 error count </param>
        /// <param name="pnErrCode"> Error 가 발생했을 때 Error Code 배열 변수 </param>
        /// <param name="psErrText"> Error 가 발생했을 때 Error Text 배열 변수 </param>
        /// <returns></returns>
        public long CJRspVerify(long nMsgId, string sCJobID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return Gem.CJRspVerify(nMsgId, sCJobID, nResult, nErrCount, pnErrCode, psErrText);
        }

        /// <summary>
        ///     이미 생성된 Control Job 의 attributes 를 설정할 때 사용합니다.
        ///     만약 XGem 에 Control Job 이 존재하지않으면 설정한 정보로 Control Job 을 생성합니다.
        /// </summary>
        /// <param name="sCJobID"> 설정할 Control Job id </param>
        /// <param name="nState"> Control Job state </param>
        /// <param name="nStartMethod"> Control Job StartMethod </param>
        /// <param name="nCountPRJob"> Control Job 에 등록할 Process Job 의 개수 </param>
        /// <param name="psPRJobID"> Control Job 에 등록할 Process Job id list 의 배열 변수 </param>
        /// <returns></returns>
        public long CJSetJobInfo(string sCJobID, long nState, long nStartMethod, long nCountPRJob, string[] psPRJobID)
        {
            return Gem.CJSetJobInfo(sCJobID, nState, nStartMethod, nCountPRJob, psPRJobID);
        }
        public long STSDelAllSubstrateInfo()
        {
            return Gem.STSDelAllSubstrateInfo();
        }
        public long STSDelSubstrateInfo(string sSubstrateID)
        {
            return Gem.STSDelSubstrateInfo(sSubstrateID);
        }
        public long STSGetAllSubstrateInfo(ref long pnObjID, ref long pnCount)
        {
            return Gem.STSGetAllSubstrateInfo(ref pnObjID, ref pnCount);
        }
        public long STSReqCancelSubstrate(string sSubstLocID, string sSubstrateID)
        {
            return Gem.STSReqCancelSubstrate(sSubstLocID, sSubstrateID);
        }
        public long STSReqCreateSubstrate(string sSubstLocID, string sSubstrateID)
        {
            return Gem.STSReqCreateSubstrate(sSubstLocID, sSubstrateID);
        }
        public long STSReqDeleteSubstrate(string sSubstLocID, string sSubstrateID)
        {
            return Gem.STSReqDeleteSubstrate(sSubstLocID, sSubstrateID);
        }
        public long STSReqProceedSubstrate(string sSubstLocID, string sSubstrateID, string sReadSubstID)
        {
            return Gem.STSReqProceedSubstrate(sSubstLocID, sSubstrateID, sReadSubstID);
        }
        public long STSRspCancelSubstrate(long nMsgID, string sSubstLocID, string sSubstrateID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return Gem.STSRspCancelSubstrate(nMsgID, sSubstLocID, sSubstrateID, nResult, nErrCount, pnErrCode, psErrText);
        }
        public long STSRspCreateSubstrate(long nMsgID, string sSubstLocID, string sSubstrateID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return Gem.STSRspCreateSubstrate(nMsgID, sSubstLocID, sSubstrateID, nResult, nErrCount, pnErrCode, psErrText);
        }
        public long STSRspDeleteSubstrate(long nMsgID, string sSubstLocID, string sSubstrateID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return Gem.STSRspDeleteSubstrate(nMsgID, sSubstLocID, sSubstrateID, nResult, nErrCount, pnErrCode, psErrText);
        }
        public long STSRspUpdateSubstrate(long nMsgID, string sSubstLocID, string sSubstrateID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            return Gem.STSRspUpdateSubstrate(nMsgID, sSubstLocID, sSubstrateID, nResult, nErrCount, pnErrCode, psErrText);
        }
        public long STSSetBatchLocationInfo(string sBatchLocID, string sSubstrateID)
        {
            return Gem.STSSetBatchLocationInfo(sBatchLocID, sSubstrateID);
        }
        public long STSSetBatchProcessing(long nCount, string[] psSubstLocID, string[] psSubstrateID, long nState)
        {
            return Gem.STSSetBatchProcessing(nCount, psSubstLocID, psSubstrateID, nState);
        }
        public long STSSetBatchTransport(long nCount, string[] psSubstLocID, string[] psSubstrateID, long nState)
        {
            return Gem.STSSetBatchTransport(nCount, psSubstLocID, psSubstrateID, nState);
        }
        public long STSSetMaterialArrived(string sMaterialID)
        {
            return Gem.STSSetMaterialArrived(sMaterialID);
        }
        public long STSSetProcessing(string sSubstLocID, string sSubstrateID, long nState)
        {
            return Gem.STSSetProcessing(sSubstLocID, sSubstrateID, nState);
        }
        public long STSSetSubstLocationInfo(string sSubstLocID, string sSubstrateID)
        {
            return Gem.STSSetSubstLocationInfo(sSubstLocID, sSubstrateID);
        }
        public long STSSetSubstrateID(string sSubstLocID, string sSubstrateID, string sSubstReadID, long nResult)
        {
            return Gem.STSSetSubstrateID(sSubstLocID, sSubstrateID, sSubstReadID, nResult);
        }
        public long STSSetSubstrateInfo(string sSubstLocID, string sSubstrateID, long nTransportState, long nProcessingState, long nReadingState)
        {
            return Gem.STSSetSubstrateInfo(sSubstLocID, sSubstrateID, nTransportState, nProcessingState, nReadingState);
        }
        public long STSSetTransport(string sSubstLocID, string sSubstrateID, long nState)
        {
            return Gem.STSSetTransport(sSubstLocID, sSubstrateID, nState);
        }
        public long GetCarrierAccessingStatus(long nMsgId, long nIndex, ref long pnState)
        {
            return Gem.GetCarrierAccessingStatus(nMsgId, nIndex, ref pnState);
        }
        public long GetCarrierClose(long nMsgId)
        {
            return Gem.GetCarrierClose(nMsgId);
        }
        public long GetCarrierContentsMap(long nMsgId, long nIndex, long nCount, ref string[] psLotID, ref string[] psSubstrateID)
        {
            return Gem.GetCarrierContentsMap(nMsgId, nIndex, nCount, ref psLotID, ref psSubstrateID);
        }
        public long GetCarrierContentsMapCount(long nMsgId, long nIndex, ref long pnCount)
        {
            return Gem.GetCarrierContentsMapCount(nMsgId, nIndex, ref pnCount);
        }
        public long GetCarrierID(long nMsgId, long nIndex, ref string psCarrierID)
        {
            return Gem.GetCarrierID(nMsgId, nIndex, ref psCarrierID);
        }
        public long GetCarrierIDStatus(long nMsgId, long nIndex, ref long pnState)
        {
            return Gem.GetCarrierIDStatus(nMsgId, nIndex, ref pnState);
        }
        public long GetCarrierLocID(long nMsgId, long nIndex, ref string psLocID)
        {
            return Gem.GetCarrierLocID(nMsgId, nIndex, ref psLocID);
        }
        public long GetCarrierSlotMap(long nMsgId, long nIndex, ref string psSlotMap)
        {
            return Gem.GetCarrierSlotMap(nMsgId, nIndex, ref psSlotMap);
        }
        public long GetCarrierSlotMapStatus(long nMsgId, long nIndex, ref long pnState)
        {
            return Gem.GetCarrierSlotMapStatus(nMsgId, nIndex, ref pnState);
        }
        public long GetCarrierUsage(long nMsgId, long nIndex, ref string psUsage)
        {
            return Gem.GetCarrierUsage(nMsgId, nIndex, ref psUsage);
        }
        public long GetCtrlJobClose(long nObjID)
        {
            return Gem.GetCtrlJobClose(nObjID);
        }
        public long GetCtrlJobID(long nObjID, long nIndex, ref string psCtrlJobID)
        {
            return Gem.GetCtrlJobID(nObjID, nIndex, ref psCtrlJobID);
        }
        public long GetCtrlJobPRJobCount(long nObjID, long nIndex, ref long pnCount)
        {
            return Gem.GetCtrlJobPRJobCount(nObjID, nIndex, ref pnCount);
        }
        public long GetCtrlJobPRJobIDs(long nObjID, long nIndex, long nCount, ref string[] psPRJobIDs)
        {
            return Gem.GetCtrlJobPRJobIDs(nObjID, nIndex, nCount, ref psPRJobIDs);
        }
        public long GetCtrlJobStartMethod(long nObjID, long nIndex, ref long pnAutoStart)
        {
            return Gem.GetCtrlJobStartMethod(nObjID, nIndex, ref pnAutoStart);
        }
        public long GetCtrlJobState(long nObjID, long nIndex, ref long pnState)
        {
            return Gem.GetCtrlJobState(nObjID, nIndex, ref pnState);
        }
        public long GetPRJobAutoStart(long nObjID, long nIndex, ref long pnAutoStart)
        {
            return Gem.GetPRJobAutoStart(nObjID, nIndex, ref pnAutoStart);
        }
        public long GetPRJobCarrier(long nObjID, long nIndex, long nCount, ref string[] psCarrierID, ref string[] psSlotInfo)
        {
            return Gem.GetPRJobCarrier(nObjID, nIndex, nCount, ref psCarrierID, ref psSlotInfo);
        }
        public long GetPRJobCarrierCount(long nObjID, long nIndex, ref long pnCount)
        {
            return Gem.GetPRJobCarrierCount(nObjID, nIndex, ref pnCount);
        }
        public long GetPRJobClose(long nObjID)
        {
            return Gem.GetPRJobClose(nObjID);
        }
        public long GetPRJobID(long nObjID, long nIndex, ref string psPJobID)
        {
            return Gem.GetPRJobID(nObjID, nIndex, ref psPJobID);
        }
        public long GetPRJobMtrlFormat(long nObjID, long nIndex, ref long pnMtrlFormat)
        {
            return Gem.GetPRJobMtrlFormat(nObjID, nIndex, ref pnMtrlFormat);
        }
        public long GetPRJobMtrlOrder(long nObjID, long nIndex, ref long pnMtrlOrder)
        {
            return Gem.GetPRJobMtrlOrder(nObjID, nIndex, ref pnMtrlOrder);
        }
        public long GetPRJobRcpID(long nObjID, long nIndex, ref string psRcpID)
        {
            return Gem.GetPRJobRcpID(nObjID, nIndex, ref psRcpID);
        }
        public long GetPRJobRcpParam(long nObjID, long nIndex, long nCount, ref string[] psRcpParName, ref string[] psRcpParValue)
        {
            return Gem.GetPRJobRcpParam(nObjID, nIndex, nCount, ref psRcpParName, ref psRcpParValue);
        }
        public long GetPRJobRcpParamEx(long nObjID, long nIndex, long nCount, ref string[] psRcpParName, ref long[] pnRcpParValue)
        {
            return Gem.GetPRJobRcpParamEx(nObjID, nIndex, nCount, ref psRcpParName, ref pnRcpParValue);
        }
        public long GetPRJobRcpParamCount(long nObjID, long nIndex, ref long pnCount)
        {
            return Gem.GetPRJobRcpParamCount(nObjID, nIndex, ref pnCount);
        }
        public long GetPRJobState(long nObjID, long nIndex, ref long pnState)
        {
            return Gem.GetPRJobState(nObjID, nIndex, ref pnState);
        }
        public long GetSubstrateClose(long nObjID)
        {
            return Gem.GetSubstrateClose(nObjID);
        }
        public long GetSubstrateID(long nObjID, long nIndex, ref string psSubstrateID)
        {
            return Gem.GetSubstrateID(nObjID, nIndex, ref psSubstrateID);
        }
        public long GetSubstrateLocID(long nObjID, long nIndex, ref string psSubstLocID)
        {
            return Gem.GetSubstrateLocID(nObjID, nIndex, ref psSubstLocID);
        }
        public long GetSubstrateState(long nObjID, long nIndex, ref long pnTransportState, ref long pnProcessingState, ref long pnReadingState)
        {
            return Gem.GetSubstrateState(nObjID, nIndex, ref pnTransportState, ref pnProcessingState, ref pnReadingState);
        }
        private void AddMessage(string log)
        {

        }
        public long GEMGetVariables(ref long pnObjectID, long nVid)
        {
            return Gem.GEMGetVariables(ref pnObjectID, nVid);
        }
        public long GEMSetEventEx(long nEventID, long nCount, long[] pnVID, string[] psValue)
        {
            return Gem.GEMSetEventEx(nEventID, nCount, pnVID, psValue);
        }
        public long GEMRspOffline(long nMsgId, long nAck)
        {
            return Gem.GEMRspOffline(nMsgId, nAck);
        }
        public long SendUserMessage(long nObjectID, string sCommand, long nTransID)
        {
            return Gem.SendUserMessage(nObjectID, sCommand, nTransID);
        }
        public long OpenGEMObject(ref long pnMsgID, string sObjType, string sObjID)
        {
            return Gem.OpenGEMObject(ref pnMsgID, sObjType, sObjID);
        }
        public long CloseGEMObject(long nMsgID)
        {
            return Gem.CloseGEMObject(nMsgID);
        }
        public long GetAttrData(ref long pnObjectID, long nMsgID, string sAttrName)
        {
            return Gem.GetAttrData(ref pnObjectID, nMsgID, sAttrName);
        }
        #endregion
        #region <remarks> Write Log </remarks>
        string path = @"C:\Logs\XGEM\SecsControlState.txt";
        private void WriteLog(string line)
        {
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(path)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                }

                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(line);
                }
            }
            catch 
            {
            }
        }

        #endregion
    }
}
