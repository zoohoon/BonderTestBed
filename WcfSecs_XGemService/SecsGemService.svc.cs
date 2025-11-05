using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Windows.Forms;
using GEM_XGem;
using System.Threading;
using System.Timers;
using System.Diagnostics;
using SecsGemServiceInterface;

namespace WcfSecs_XGemService
{
    //[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext =false)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class SecsGemService : ISecsGemService, IDisposable
    {

        private XGemNet m_XGem = null;
        private ISecsGemServiceCallbackBase callback = null;
        //private static XGemNet m_XGem = null;
        //private static ISecsGemCallbackServiceCallback callback;

        private Object LockObj = new object();
        //private Queue<Action> callbackQueue;
        private Thread ServiceThread = null;

        private SecsGemStateEnum SECSGemState;
        private SecsCommStateEnum SECSCommState;
        private SecsControlStateEnum SECSControlState;
        private bool threadRun;

        private System.Timers.Timer timer;
        private object callbackLocker = new object();

        public SecsGemService()
        {
            if (m_XGem == null)
            {
                GemServiceInitSetting();
            }


            timer = new System.Timers.Timer();
            timer.Interval = 1000;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            timer.Start();

            //threadRun = true;
            //ThreadStart ts = new ThreadStart(LoopThread);
            //ServiceThread = new Thread(ts);
            //ServiceThread.Start();

            ////callback.CallBack_ConnectSuccess();
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

        private void GemServiceInitSetting()
        {
            m_XGem = new XGemNet();
            m_XGem.OnGEMCommStateChanged        += new OnGEMCommStateChanged(XGem_OnGEMCommStateChange);
            m_XGem.OnXGEMStateEvent             += new OnXGEMStateEvent(XGEM_OnXGEMStateEvent);
            m_XGem.OnSECSMessageReceived        += new OnSECSMessageReceived(XGEM_OnSECSMessageReceive);
            m_XGem.OnGEMControlStateChanged     += new OnGEMControlStateChanged(XGEM_OnGEMControlStateChange);
            m_XGem.OnGEMRspGetDateTime          += new OnGEMRspGetDateTime(XGEM_OnGEMRspGetDateTime);
            m_XGem.OnGEMReqDateTime             += new OnGEMReqDateTime(XGEM_OnGEMReqDateTime);

            m_XGem.OnGEMTerminalMessage         += new OnGEMTerminalMessage(XGEM_OnGEMTerminalMessage);
            m_XGem.OnGEMTerminalMultiMessage    += new OnGEMTerminalMultiMessage(XGEM_OnGEMTerminalMultiMessage);

            m_XGem.OnGEMReqRemoteCommand        += new OnGEMReqRemoteCommand(XGEM_OnGEMReqRemoteCommand);
            m_XGem.OnGEMReqPPList               += new OnGEMReqPPList(XGEM_OnGEMReqPPList);

            ////////////////////////////////////////////////////
            m_XGem.OnGEMECVChanged              += new OnGEMECVChanged(XGEM_OnGEMECVChanged);
            m_XGem.OnGEMReqChangeECV            += new OnGEMReqChangeECV(XGEM_OnGEMReqChangeECV);
            m_XGem.OnGEMReqGetDateTime          += new OnGEMReqGetDateTime(XGEM_OnGEMReqGetDateTime);
            m_XGem.OnGEMErrorEvent              += new OnGEMErrorEvent(XGEM_OnGEMErrorEvent);
            m_XGem.OnGEMReqPPLoadInquire        += new OnGEMReqPPLoadInquire(XGEM_OnGEMReqPPLoadInquire);
            m_XGem.OnGEMReqPPEx                 += new OnGEMReqPPEx(XGEM_OnGEMReqPPEx);
            m_XGem.OnGEMReqPPDelete             += new OnGEMReqPPDelete(XGEM_OnGEMReqPPDelete);
            m_XGem.OnGEMRspAllECInfo            += new OnGEMRspAllECInfo(XGEM_OnGEMRspAllECInfo);

            m_XGem.OnGEMReqPPSendEx             += new OnGEMReqPPSendEx(m_XGem_OnGEMReqPPSendEx);

            SECSGemState = SecsGemStateEnum.UNKNOWN;
            SECSCommState = SecsCommStateEnum.UNKNOWN;
            SECSControlState = SecsControlStateEnum.UNKNOWN;
        }

        private void XGEM_OnGEMReqPPEx(long nMsgId, string sPpid, string sRecipePath)
        {
            lock (callbackLocker)
            {
                callback.OnGEMReqPPEx(nMsgId, sPpid, sRecipePath);

                timer.Stop();
                timer.Start();
            }
        }

        private void m_XGem_OnGEMReqPPSendEx(long nMsgId, string sPpid, string sRecipePath)
        {
            lock (callbackLocker)
            {
                callback.OnGEMReqPPSendEx(nMsgId, sPpid, sRecipePath);

                timer.Stop();
                timer.Start();
            }
        }

        private void XGEM_OnGEMRspAllECInfo(long lCount, long[] plVid, string[] psName, string[] psValue, string[] psDefault, string[] psMin, string[] psMax, string[] psUnit)
        {
            lock (callbackLocker)
            {
                callback.OnGEMRspAllECInfo(lCount, plVid, psName, psValue, psDefault, psMin, psMax, psUnit);

                timer.Stop();
                timer.Start();
            }
        }

        private void XGEM_OnGEMReqPPDelete(long nMsgId, long nCount, string[] psPpid)
        {
            lock (callbackLocker)
            {
                callback.OnGEMReqPPDelete(nMsgId, nCount, psPpid);

                timer.Stop();
                timer.Start();
            }
        }

        private void XGEM_OnGEMReqPPLoadInquire(long nMsgId, string sPpid, long nLength)
        {
            lock (callbackLocker)
            {
                this.m_XGem.GEMRspPPLoadInquire(nMsgId, sPpid, 0);

                timer.Stop();
                timer.Start();
            }
        }

        private void XGEM_OnGEMErrorEvent(string sErrorName, long nErrorCode)
        {
            lock (callbackLocker)
            {
                callback.OnGEMErrorEvent(sErrorName, nErrorCode);

                timer.Stop();
                timer.Start();
            }
        }

        private void XGEM_OnGEMReqGetDateTime(long nMsgId)
        {
            lock (callbackLocker)
            {
                string systemTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                long ret = this.m_XGem.GEMRspGetDateTime(nMsgId, systemTime);

                timer.Stop();
                timer.Start();
            }
        }

        private void XGEM_OnGEMReqChangeECV(long nMsgId, long nCount, long[] pnEcids, string[] psVals)
        {
            lock (callbackLocker)
            {
                callback.OnGEMReqChangeECV(nMsgId, nCount, pnEcids, psVals);

                timer.Stop();
                timer.Start();
            }
        }

        private void XGEM_OnGEMECVChanged(long nCount, long[] pnEcids, string[] psVals)
        {
            lock (callbackLocker)
            {
                callback.OnGEMECVChanged(nCount, pnEcids, psVals);

                timer.Stop();
                timer.Start();
            }
        }

        private void XGEM_OnGEMReqPPList(long nMsgId)
        {
            lock (callbackLocker)
            {
                callback.OnGEMReqPPList(nMsgId);
                timer.Stop();
                timer.Start();
            }
        }

        private void XGEM_OnGEMReqRemoteCommand(long nMsgId, string sRcmd, long nCount, string[] psNames, string[] psVals)
        {
            lock (callbackLocker)
            {
                EnumRemoteCommand Rcmd = EnumRemoteCommand.UNDEFINE;

                bool parseResult = Enum.TryParse(sRcmd, out Rcmd);

                if(parseResult != false)
                {
                    callback.OnGEMReqRemoteCommand(nMsgId, Rcmd, nCount, psNames, psVals);
                }

                timer.Stop();
                timer.Start();
            }
        }

        private void XGEM_OnGEMReqDateTime(long nMsgId, string sSystemTime)
        {
            lock (callbackLocker)
            {
                callback.OnGEMReqDateTime(nMsgId, sSystemTime);
                timer.Stop();
                timer.Start();
            }

            //m_XGem.GEMRspDateTime(nMsgId, 0);
        }

        private void XGEM_OnGEMTerminalMultiMessage(long nTid, long nCount, string[] psMsg)
        {
            lock (callbackLocker)
            {
                callback.OnGEMTerminalMultiMessage(nTid, nCount, psMsg);
                timer.Stop();
                timer.Start();
            }
        }

        private void XGEM_OnGEMTerminalMessage(long nTid, string sMsg)
        {
            lock (callbackLocker)
            {
                callback.OnGEMTerminalMessage(nTid, sMsg);
                timer.Stop();
                timer.Start();
            }
        }

        private void XGEM_OnGEMRspGetDateTime(string sSystemTime)
        {
            lock (callbackLocker)
            {
                callback.OnGEMRspGetDateTime(sSystemTime);
                timer.Stop();
                timer.Start();
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

        public void ServerConnect()
        {
            callback = OperationContext.Current.GetCallbackChannel<ISecsGemServiceCallbackBase>();
            //callback.Are_You_There();
        }

        //public bool ServerConnect()
        //{
        //    callback = OperationContext.Current.GetCallbackChannel<ISecsGemServiceCallback>();
        //    return callback.CallBack_ConnectSuccess();
        //    //단순히 객체를 만들기 위한 함수.
        //}


        public void Dispose()
        {
            if (m_XGem != null)
            {
                //callback.
                //callback.CallBack_Close();
                threadRun = false;

                m_XGem.OnGEMCommStateChanged -= XGem_OnGEMCommStateChange;
                m_XGem.OnXGEMStateEvent -= XGEM_OnXGEMStateEvent;
                m_XGem.OnSECSMessageReceived -= XGEM_OnSECSMessageReceive;
                m_XGem.OnGEMControlStateChanged -= XGEM_OnGEMControlStateChange;

                m_XGem.OnGEMRspGetDateTime -= XGEM_OnGEMRspGetDateTime;
                m_XGem.OnGEMReqDateTime -= XGEM_OnGEMReqDateTime;

                m_XGem.OnGEMTerminalMessage -= XGEM_OnGEMTerminalMessage;
                m_XGem.OnGEMTerminalMultiMessage -= XGEM_OnGEMTerminalMultiMessage;

                m_XGem.OnGEMReqRemoteCommand -= XGEM_OnGEMReqRemoteCommand;
                m_XGem.OnGEMReqPPList -= XGEM_OnGEMReqPPList;

                m_XGem.Stop();
                m_XGem.Close();
            }
        }

        public long Init_SECSGEM(string Config)
        {
            long initVal = 0;

            initVal = m_XGem.Initialize(Config);

            if (initVal == -10001)
            {
                Dispose();
                GemServiceInitSetting();
                initVal = m_XGem.Initialize(Config);
            }

            return initVal;
        }

        public long Close_SECSGEM()
        {
            long initVal = 0;

            initVal = m_XGem.Close();

            return initVal;
        }

        public long Start()
        {
            long initRet = 0;

            initRet = m_XGem.Start();

            return initRet;
        }

        public long Stop()
        {
            long initRet = 0;

            initRet = m_XGem.Stop();

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

            if (Enum.IsDefined(typeof(SecsControlStateEnum), (int)nState))
                Enum.TryParse(nState.ToString(), out SECSControlState);
            else
                SECSControlState = SecsControlStateEnum.UNKNOWN;

            //callback = OperationContext.Current.GetCallbackChannel<ISecsGemServiceCallback>();
            lock (callbackLocker)
            {
                callback.OnGEMControlStateChange(SECSControlState);
            }
            //callbackQueue.Enqueue(()=>callback.OnGEMControlStateChange(SECSControlState.ToString()));
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
        private void XGEM_OnSECSMessageReceive(long nMsgId, long nStream, long nFunction, long nSysbyte)
        {
            //lock(callbackLocker)
            //{
            //    callback.OnSECSMessageReceive(nMsgId, nStream, nFunction, nSysbyte);
            //}


            if (nStream == 3 && nFunction == 17)
            {
                long pnMsgId = 0;
                byte CAACK = 0;

                long listCount1 = 0;
                long listCount2 = 0;
                long listCount3 = 0;
                long listCount4 = 0;

                uint[] USER_UINT4 = new uint[1];
                string CARRIERACTION = null;
                string CARRIERID = null;
                byte[] PTN = new byte[1];

                string CATTRID = null;
                string CATTRDATA = null;
                string CATTRDATA2 = null;

                CarrierActReqData carrierActReqData = null;

                int total_cnt = 0;

                //Stopwatch sw = new Stopwatch();
                //sw.Start();

                m_XGem.GetList(nMsgId, ref listCount1);
                m_XGem.GetU4(nMsgId, ref USER_UINT4);
                m_XGem.GetAscii(nMsgId, ref CARRIERACTION);
                m_XGem.GetAscii(nMsgId, ref CARRIERID);
                m_XGem.GetU1(nMsgId, ref PTN);
                m_XGem.GetList(nMsgId, ref listCount2);

                if (CARRIERACTION.ToUpper() == EnumCarrierAction.PROCEEDWITHCARRIER.ToString())
                {
                    PROCEEDWITHCARRIER_ReqData Carrier = new PROCEEDWITHCARRIER_ReqData();
                    Carrier.ActionType = EnumCarrierAction.PROCEEDWITHCARRIER;
                    Carrier.SYSTEMBYTE = nSysbyte;
                    Carrier.DATAID = USER_UINT4[0];
                    Carrier.CARRIERACTION = CARRIERACTION;
                    Carrier.CARRIERID = CARRIERID;
                    Carrier.PTN = PTN[0];
                    Carrier.COUNT = listCount2;
                    Carrier.CATTRID = new string[listCount2];
                    Carrier.CATTRDATA = new string[listCount2];
                    Carrier.SLOTMAP = new string[listCount2 * 25];

                    for (int i = 0; i < listCount2; i++)
                    {
                        m_XGem.GetList(nMsgId, ref listCount3);

                        if (listCount3 == 2)
                        {
                            m_XGem.GetAscii(nMsgId, ref CATTRID);
                            Carrier.CATTRID[i] = CATTRID;
                            m_XGem.GetAscii(nMsgId, ref CATTRDATA);
                            Carrier.CATTRDATA[i] = CATTRDATA;
                        }
                        else if (listCount3 == 3)
                        {
                            m_XGem.GetAscii(nMsgId, ref CATTRID);
                            Carrier.CATTRID[i] = CATTRID;
                            m_XGem.GetList(nMsgId, ref listCount4);

                            for (int s = 0; s < 25; s++)
                            {
                                string slotmap = null;
                                m_XGem.GetAscii(nMsgId, ref slotmap);
                                Carrier.SLOTMAP[total_cnt++] = slotmap;
                            }

                            string temp = null;
                            string cattrdata = null;
                            m_XGem.GetList(nMsgId, ref listCount4);
                            m_XGem.GetAscii(nMsgId, ref temp);
                            m_XGem.GetAscii(nMsgId, ref cattrdata);
                            Carrier.CATTRDATA[i] = cattrdata;
                        }
                    }

                    carrierActReqData = Carrier;
                }
                else if (CARRIERACTION.ToUpper() == EnumCarrierAction.PROCEEDWITHSLOT.ToString())
                {
                    PROCEEDWITHSLOT_ReqData proceedWithSlot = new PROCEEDWITHSLOT_ReqData();
                    proceedWithSlot.ActionType = EnumCarrierAction.PROCEEDWITHSLOT;

                    proceedWithSlot.CARRIERID = CARRIERID;
                    proceedWithSlot.PTN = PTN[0];
                    proceedWithSlot.COUNT = listCount2;
                    proceedWithSlot.SLOTMAP = new string[25];
                    proceedWithSlot.OCRMAP = new string[25];

                    for (int i = 0; i < listCount2; i++)
                    {
                        m_XGem.GetList(nMsgId, ref listCount3);
                        m_XGem.GetAscii(nMsgId, ref CATTRID);
                        m_XGem.GetList(nMsgId, ref listCount4);

                        for (int s = 0; s < 25; s++)
                        {
                            string slotmap = null;
                            m_XGem.GetAscii(nMsgId, ref slotmap);
                            proceedWithSlot.SLOTMAP[s] = slotmap;
                        }

                        string temp = null;
                        string cattrdata = null;
                        m_XGem.GetList(nMsgId, ref listCount3);
                        m_XGem.GetAscii(nMsgId, ref temp);
                        m_XGem.GetAscii(nMsgId, ref cattrdata);
                        proceedWithSlot.USAGE = cattrdata;

                        long item_cnt = 0;
                        string cattrid = null;
                        long slotmap_cnt = 0;
                        m_XGem.GetList(nMsgId, ref item_cnt);
                        m_XGem.GetAscii(nMsgId, ref cattrid);
                        m_XGem.GetList(nMsgId, ref slotmap_cnt);

                        for (int s = 0; s < 25; s++)
                        {
                            string ocrmap = null;
                            m_XGem.GetAscii(nMsgId, ref ocrmap);
                            proceedWithSlot.OCRMAP[s] = ocrmap;
                        }
                    }
                    //sw.Stop();
                    //Trace.WriteLine("aa : " + sw.ElapsedMilliseconds);

                    carrierActReqData = proceedWithSlot;
                }
                else if (CARRIERACTION.ToUpper() == EnumCarrierAction.CANCELCARRIER.ToString())
                {
                    CANCELCARRIER_ReqData cancelcarrier = new CANCELCARRIER_ReqData();
                    cancelcarrier.ActionType = EnumCarrierAction.CANCELCARRIER;
                    carrierActReqData = cancelcarrier;
                }
                else
                {
                }

                //sw.Stop();
                //MessageBox.Show(sw.ElapsedMilliseconds.ToString());
                m_XGem.CloseObject(nMsgId);
                if(carrierActReqData != null)
                {
                    carrierActReqData.Stream = 3;
                    carrierActReqData.Function = 17;
                    carrierActReqData.Sysbyte = nSysbyte;
                    callback.OnCarrierActMsgRecive(carrierActReqData);
                }
            }
            else if (nStream == 3 && nFunction == 27)
            {
                long pnMsgId = 0;
                byte CAACK = 0;

                long listCount1 = 0;
                long listCount2 = 0;

                byte[] ACCESSMODE = new byte[1];
                byte[] PTN = null;

                //if (this.mGemStateParam.ControlState != "REMOTE")
                //{
                //    CAACK = 2;
                //}

                m_XGem.GetList(nMsgId, ref listCount1);
                m_XGem.GetU1(nMsgId, ref ACCESSMODE);
                m_XGem.GetList(nMsgId, ref listCount2);
                PTN = new byte[listCount2];
                m_XGem.GetU1(nMsgId, ref PTN);

                /////////////////////////////////////////////////////////////////////////////////
                if ((CAACK != 1) && (CAACK != 2) && (CAACK != 6))
                {

                }

                m_XGem.MakeObject(ref pnMsgId);

                m_XGem.SetList(pnMsgId, 2);
                m_XGem.SetU1(pnMsgId, CAACK);//
                m_XGem.SetList(pnMsgId, 0);

                m_XGem.SendSECSMessage(pnMsgId, nStream, nFunction + 1, nSysbyte);
            }


            //lock (callbackLocker)
            //{
            //    callback.OnSECSMessageReceive(nMsgId, nStream, nFunction, nSysbyte);
            //    timer.Stop();
            //    timer.Start();
            //
        }

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

            if (Enum.IsDefined(typeof(SecsGemStateEnum), (int)nState))
                Enum.TryParse(nState.ToString(), out SECSGemState);
            else
                SECSGemState = SecsGemStateEnum.UNKNOWN;

            string sLog = String.Format("[XGEM ==> EQ] OnXGEMStateEvent:{0}", SECSGemState.ToString());

            if (SECSGemState == SecsGemStateEnum.EXECUTE)
            {
                GEMReqAllECInfo();
            }
            else
            {
                if(SECSCommState == SecsCommStateEnum.COMMUNICATING)
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
                    callback.OnGEMStateEvent(SECSGemState);
                }
            }
            catch
            {
                callback = null;
            }
            //callbackQueue.Enqueue(() => callback.OnGEMStateEvent(SECSGemState.ToString()));
        }

        private void XGem_OnGEMCommStateChange(long nState)
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

            //callbackQueue.Enqueue(() => callback.OnGEMCommStateChange(SECSCommState.ToString()));
            //callback = OperationContext.Current.GetCallbackChannel<ISecsGemServiceCallback>();

            try
            {
                lock (callbackLocker)
                {
                    callback.OnGEMCommStateChange(SECSCommState);
                }
            }
            catch
            {
                Dispose();
            }
        }

        public long MakeObject(ref long pnMsgId)
        {
            return m_XGem.MakeObject(ref pnMsgId);
        }

        public long SetListItem(long nMsgId, long nItemCount)
        {
            return m_XGem.SetList(nMsgId, nItemCount);
        }

        public long SetBinaryItem(long nMsgId, byte nValue)
        {
            return m_XGem.SetBinary(nMsgId, nValue);
        }

        public long SetBoolItem(long nMsgId, bool nValue)
        {
            return m_XGem.SetBool(nMsgId, nValue);
        }

        public long SetBoolItems(long nMsgId, bool[] pnValue)
        {
            return m_XGem.SetBool(nMsgId, pnValue);
        }

        public long SetUint1Item(long nMsgId, byte nValue)
        {
            return m_XGem.SetU1(nMsgId, nValue);
        }

        public long SetUint1Items(long nMsgId, byte[] pnValue)
        {
            return m_XGem.SetU1(nMsgId, pnValue);
        }

        public long SetUint2Item(long nMsgId, ushort nValue)
        {
            return m_XGem.SetU2(nMsgId, nValue);
        }

        public long SetUint2Items(long nMsgId, ushort[] pnValue)
        {
            return m_XGem.SetU2(nMsgId, pnValue);
        }

        public long SetUint4Item(long nMsgId, uint nValue)
        {
            return m_XGem.SetU4(nMsgId, nValue);
        }

        public long SetUint4Items(long nMsgId, uint[] pnValue)
        {
            return m_XGem.SetU4(nMsgId, pnValue);
        }

        public long SetUint8Item(long nMsgId, uint nValue)
        {
            return m_XGem.SetU8(nMsgId, nValue);
        }

        public long SetUint8Items(long nMsgId, uint[] pnValue)
        {
            return m_XGem.SetU8(nMsgId, pnValue);
        }

        public long SetInt1Item(long nMsgId, sbyte nValue)
        {
            return m_XGem.SetI1(nMsgId, nValue);
        }

        public long SetInt1Items(long nMsgId, sbyte[] pnValue)
        {
            return m_XGem.SetI1(nMsgId, pnValue);
        }

        public long SetInt2Item(long nMsgId, short nValue)
        {
            return m_XGem.SetI2(nMsgId, nValue);
        }

        public long SetInt2Items(long nMsgId, short[] pnValue)
        {
            return m_XGem.SetI2(nMsgId, pnValue);
        }

        public long SetInt4Item(long nMsgId, int nValue)
        {
            return m_XGem.SetI4(nMsgId, nValue);
        }

        public long SetInt4Items(long nMsgId, int[] pnValue)
        {
            return m_XGem.SetI4(nMsgId, pnValue);
        }

        public long SetInt8Items(long nMsgId, int[] pnValue)
        {
            return m_XGem.SetI8(nMsgId, pnValue);
        }

        public long SetInt8Item(long nMsgId, int nValue)
        {
            return m_XGem.SetI8(nMsgId, nValue);
        }

        public long SetFloat4Item(long nMsgId, float nValue)
        {
            return m_XGem.SetF4(nMsgId, nValue);
        }

        public long SetFloat4Items(long nMsgId, float[] pnValue)
        {
            return m_XGem.SetF4(nMsgId, pnValue);

        }

        public long SetFloat8Item(long nMsgId, double nValue)
        {
            return m_XGem.SetF8(nMsgId, nValue);

        }

        public long SetFloat8Items(long nMsgId, double[] pnValue)
        {
            return m_XGem.SetF8(nMsgId, pnValue);
        }

        public long SetStringItem(long nMsgId, string pszValue)
        {
            return m_XGem.SetAscii(nMsgId, pszValue);
        }

        public long GetListItem(long nMsgId, ref long pnItemCount)
        {
            return m_XGem.GetList(nMsgId, ref pnItemCount);
        }

        public long GetBinaryItem(long nMsgId, ref byte[] pnValue)
        {
            return m_XGem.GetBinary(nMsgId, ref pnValue);
        }

        public long GetBoolItem(long nMsgId, ref bool[] pnValue)
        {
            return m_XGem.GetBool(nMsgId, ref pnValue);
        }

        public long GetUint1Item(long nMsgId, ref byte[] pnValue)
        {
            return m_XGem.GetU1(nMsgId, ref pnValue);
        }

        public long GetUint2Item(long nMsgId, ref ushort[] pnValue)
        {
            return m_XGem.GetU2(nMsgId, ref pnValue);
        }

        public long GetUint4Item(long nMsgId, ref uint[] pnValue)
        {
            return m_XGem.GetU4(nMsgId, ref pnValue);
        }

        public long GetUint8Item(long nMsgId, ref uint[] pnValue)
        {
            return m_XGem.GetU8(nMsgId, ref pnValue);
        }

        public long GetInt1Item(long nMsgId, ref sbyte[] pnValue)
        {
            return m_XGem.GetI1(nMsgId, ref pnValue);
        }

        public long GetInt2Item(long nMsgId, ref short[] pnValue)
        {
            return m_XGem.GetI2(nMsgId, ref pnValue);
        }

        public long GetInt4Item(long nMsgId, ref int[] pnValue)
        {
            return m_XGem.GetI4(nMsgId, ref pnValue);
        }

        public long GetInt8Item(long nMsgId, ref int[] pnValue)
        {
            return m_XGem.GetI8(nMsgId, ref pnValue);
        }

        public long GetFloat4Item(long nMsgId, ref float[] pnValue)
        {
            return m_XGem.GetF4(nMsgId, ref pnValue);
        }

        public long GetFloat8Item(long nMsgId, ref double[] pnValue)
        {
            return m_XGem.GetF8(nMsgId, ref pnValue);
        }

        public long GetStringItem(long nMsgId, ref string psValue)
        {
            return m_XGem.GetAscii(nMsgId, ref psValue);
        }

        public long CloseObject(long nMsgId)
        {
            return m_XGem.CloseObject(nMsgId);
        }

        public long SendSECSMessage(long nMsgId, long nStream, long nFunction, long nSysbyte)
        {
            return m_XGem.SendSECSMessage(nMsgId, nStream, nFunction, nSysbyte);
        }

        public long GEMReqOffline()
        {
            return m_XGem.GEMReqOffline();
        }

        public long GEMReqLocal()
        {
            return m_XGem.GEMReqLocal();
        }

        public long GEMReqRemote()
        {
            return m_XGem.GEMReqRemote();
        }

        public long GEMSetEstablish(long bState)
        {
            return m_XGem.GEMSetEstablish(bState);
        }

        public long GEMSetParam(string sParamName, string sParamValue)
        {
            return m_XGem.GEMSetParam(sParamName, sParamValue);
        }

        public long GEMGetParam(string sParamName, ref string psParamValue)
        {
            long retVal = -1;
            string[] splitParamName = null;
            StringBuilder setValue = new StringBuilder();
            string getData = string.Empty;
            string tmpData = string.Empty;

            splitParamName = sParamName.Split(',');

            if(splitParamName != null)
            {
                foreach(var paramName in splitParamName)
                {
                    if(!string.IsNullOrEmpty(paramName))
                    {
                        retVal = m_XGem.GEMGetParam(paramName, ref getData);

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

            
            return retVal;
        }

        public long GEMReqGetDateTime()
        {
            return m_XGem.GEMReqGetDateTime();
        }

        public long GEMRspGetDateTime(long nMsgId, string sSystemTime)
        {
            return m_XGem.GEMRspGetDateTime(nMsgId, sSystemTime);
        }

        public long GEMRspDateTime(long nMsgId, long nResult)
        {
            return m_XGem.GEMRspDateTime(nMsgId, nResult);

        }

        public long GEMSetAlarm(long nID, long nState)
        {
            return m_XGem.GEMSetAlarm(nID, nState);
        }

        public long GEMRspRemoteCommand(long nMsgId, string sCmd, long nHCAck, long nCount, long[] pnResult)
        {
            return m_XGem.GEMRspRemoteCommand(nMsgId, sCmd, nHCAck, nCount, pnResult);
        }

        public long GEMRspRemoteCommand2(long nMsgId, string sCmd, long nHCAck, long nCount, string[] psCpName, long[] pnCpAck)
        {
            return m_XGem.GEMRspRemoteCommand2(nMsgId, sCmd, nHCAck, nCount, psCpName, pnCpAck);
        }

        public long GEMRspChangeECV(long nMsgId, long nResult)
        {
            return m_XGem.GEMRspChangeECV(nMsgId, nResult);
        }

        public long GEMSetECVChanged(long nCount, long[] pnEcIds, string[] psEcVals)
        {
            return m_XGem.GEMSetECVChanged(nCount, pnEcIds, psEcVals);
        }

        public long GEMEnableLog(long bEnabled)
        {
            return m_XGem.GEMEnableLog(bEnabled);
        }

        public long GEMSetLogOption(string sDriectory, string sPrefix, string sExtension, long nKeepDay, long bMakeDailyLog, long bMakeSubDirectory)
        {
            return m_XGem.GEMSetLogOption(sDriectory, sPrefix, sExtension, nKeepDay, bMakeDailyLog, bMakeSubDirectory);
        }

        public long GEMReqAllECInfo()
        {
            return m_XGem.GEMReqAllECInfo();
        }

        public long GEMSetPPChanged(long nMode, string sPpid, long nLength, string pbBody)
        {
            return m_XGem.GEMSetPPChanged(nMode, sPpid, nLength, pbBody);
        }

        public long GEMSetPPFmtChanged(long nMode, string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames)
        {
            return m_XGem.GEMSetPPFmtChanged(nMode, sPpid, sMdln, sSoftRev, nCount, psCCode, pnParamCount, psParamNames);
        }

        public long GEMReqPPLoadInquire(string sPpid, long nLength)
        {
            return m_XGem.GEMReqPPLoadInquire(sPpid, nLength);
        }

        public long GEMRspPPLoadInquire(long nMsgId, string sPpid, long nResult)
        {
            return m_XGem.GEMRspPPLoadInquire(nMsgId, sPpid, nResult);

        }

        public long GEMReqPPSend(string sPpid, string pbBody)
        {
            return m_XGem.GEMReqPPSend(sPpid, pbBody);

        }

        public long GEMRspPPSend(long nMsgId, string sPpid, long nResult)
        {
            return m_XGem.GEMRspPPSend(nMsgId, sPpid, nResult);

        }

        public long GEMReqPP(string sPpid)

        {
            return m_XGem.GEMReqPP(sPpid);

        }

        public long GEMRspPP(long nMsgId, string sPpid, string pbBody)
        {
            return m_XGem.GEMRspPP(nMsgId, sPpid, pbBody);
        }

        public long GEMRspPPDelete(long nMsgId, long nCount, string[] psPpids, long nResult)
        {
            return m_XGem.GEMRspPPDelete(nMsgId, nCount, psPpids, nResult);
        }

        public long GEMRspPPList(long nMsgId, long nCount, string[] psPpids)
        {
            return m_XGem.GEMRspPPList(nMsgId, nCount, psPpids);
        }

        public long GEMReqPPFmtSend(string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames)
        {
            return m_XGem.GEMReqPPFmtSend(sPpid, sMdln, sSoftRev, nCount, psCCode, pnParamCount, psParamNames);
        }

        public long GEMReqPPFmtSend2(string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames, string[] psParamValues)
        {
            return m_XGem.GEMReqPPFmtSend2(sPpid, sMdln, sSoftRev, nCount, psCCode, pnParamCount, psParamNames, psParamValues);
        }

        public long GEMRspPPFmtSend(long nMsgId, string sPpid, long nResult)
        {
            return m_XGem.GEMRspPPFmtSend(nMsgId, sPpid, nResult);
        }

        public long GEMReqPPFmt(string sPpid)
        {
            return m_XGem.GEMReqPPFmt(sPpid);
        }

        public long GEMRspPPFmt(long nMsgId, string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames)
        {
            return m_XGem.GEMRspPPFmt(nMsgId, sPpid, sMdln, sSoftRev, nCount, psCCode, pnParamCount, psParamNames);
        }

        public long GEMRspPPFmt2(long nMsgId, string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames, string[] psParamValues)
        {
            return m_XGem.GEMRspPPFmt2(nMsgId, sPpid, sMdln, sSoftRev, nCount, psCCode, pnParamCount, psParamNames, psParamValues);

        }

        public long GEMReqPPFmtVerification(string sPpid, long nCount, long[] pnAck, string[] psSeqNumber, string[] psError)
        {
            return m_XGem.GEMReqPPFmtVerification(sPpid, nCount, pnAck, psSeqNumber, psError);

        }

        public long GEMSetTerminalMessage(long nTid, string sMsg)
        {
            return m_XGem.GEMSetTerminalMessage(nTid, sMsg);

        }


        public long GEMGetVariable(long nCount, ref long[] pnVid, ref string[] psValue)
        {
            return m_XGem.GEMGetVariable(nCount, ref pnVid, ref psValue);
        }

        public long GEMSetVariables(long nMsgId, long nVid)
        {
            return m_XGem.GEMSetVariables(nMsgId, nVid);
        }

        public long GEMSetVarName(long nCount, string[] psVidName, string[] psValue)
        {
            return m_XGem.GEMSetVarName(nCount, psVidName, psValue);
        }

        public long GEMGetVarName(long nCount, string[] psVidName, string[] psValue)
        {
            return m_XGem.GEMSetVarName(nCount, psVidName, psValue);
        }

        public long GEMSetEvent(long nEventID)
        {
            return m_XGem.GEMSetEvent(nEventID);
        }

        public long GEMSetSpecificMessage(long nMsgId, string sMsgName)
        {
            return m_XGem.GEMSetSpecificMessage(nMsgId, sMsgName);
        }

        public long GEMGetSpecificMessage(long nSObjectID, ref long pnRObjectID, string sMsgName)
        {
            return m_XGem.GEMGetSpecificMessage(nSObjectID, ref pnRObjectID, sMsgName);
        }

        public long GetAllStringItem(long nMsgId, ref string psValue)
        {
            return m_XGem.GetAllStringItem(nMsgId, ref psValue);
        }

        public long SetAllStringItem(long nMsgId, string sValue)
        {
            return m_XGem.SetAllStringItem(nMsgId, sValue);
        }

        public long GEMReqPPSendEx(string sPpid, string sRecipePath)
        {
            return m_XGem.GEMReqPPSendEx(sPpid, sRecipePath);
        }

        public long GEMRspPPSendEx(long nMsgId, string sPpid, string sRecipePath, long nResult)
        {
            return m_XGem.GEMRspPPSendEx(nMsgId, sPpid, sRecipePath, nResult);
        }

        public long GEMReqPPEx(string sPpid, string sRecipePath)
        {
            return m_XGem.GEMReqPPEx(sPpid, sRecipePath);
        }

        public long GEMRspPPEx(long nMsgId, string sPpid, string sRecipePath)
        {

            return m_XGem.GEMRspPPEx(nMsgId, sPpid, sRecipePath);
        }

        public long GEMSetVariableEx(long nMsgId, long nVid)
        {
            return m_XGem.GEMSetVariableEx(nMsgId, nVid);
        }

        public long GEMReqLoopback(long nCount, long[] pnAbs)
        {
            return m_XGem.GEMReqLoopback(nCount, pnAbs);
        }

        public long GEMSetEventEnable(long nCount, long[] pnCEIDs, long nEnable)
        {
            return m_XGem.GEMSetEventEnable(nCount, pnCEIDs, nEnable);
        }

        public long GEMSetAlarmEnable(long nCount, long[] pnALIDs, long nEnable)
        {
            return m_XGem.GEMSetAlarmEnable(nCount, pnALIDs, nEnable);

        }

        public long GEMGetEventEnable(long nCount, long[] pnCEIDs, ref long[] pnEnable)
        {
            return m_XGem.GEMGetEventEnable(nCount, pnCEIDs, ref pnEnable);

        }

        public long GEMGetAlarmEnable(long nCount, long[] pnALIDs, ref long[] pnEnable)
        {
            return m_XGem.GEMGetAlarmEnable(nCount, pnALIDs, ref pnEnable);

        }

        public long GEMGetAlarmInfo(long nCount, long[] pnALIDs, ref long[] pnALCDs, ref string[] psALTXs)
        {
            return m_XGem.GEMGetAlarmInfo(nCount, pnALIDs, ref pnALCDs, ref psALTXs);

        }

        public long GEMGetSVInfo(long nCount, long[] pnSVIDs, ref string[] psMins, ref string[] psMaxs)
        {
            return m_XGem.GEMGetSVInfo(nCount, pnSVIDs, ref psMins, ref psMaxs);

        }

        public long GEMGetECVInfo(long nCount, long[] pnECIDs, ref string[] psNames, ref string[] psDefs, ref string[] psMins, ref string[] psMaxs, ref string[] psUnits)
        {
            return m_XGem.GEMGetECVInfo(nCount, pnECIDs, ref psNames, ref psDefs, ref psMins, ref psMaxs, ref psUnits);
        }

        public long GEMRspOffline(long nMsgId, long nAck)
        {
            return m_XGem.GEMRsqOffline(nMsgId, nAck);
        }

        public long GEMRspOnline(long nMsgId, long nAck)
        {
            return m_XGem.GEMRspOnline(nMsgId, nAck);
        }

        public long GEMReqHostOffline()
        {
            return m_XGem.GEMReqHostOffline();
        }

        public long GEMReqStartPolling(string sName, long nScanTime)
        {
            return m_XGem.GEMReqStartPolling(sName, nScanTime);
        }

        public long GEMReqStopPolling(string sName)
        {
            return m_XGem.GEMReqStopPolling(sName);
        }

        public long GEMEQInitialized(long nInitType)
        {
            return m_XGem.GEMEQInitialized(nInitType);
        }

        public long GEMSetVariable(long nCount, long[] pnVid, string[] psValue)
        {
            return m_XGem.GEMSetVariable(nCount, pnVid, psValue);
        }

        #endregion
    }
}
