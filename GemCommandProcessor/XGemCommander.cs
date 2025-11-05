
using Autofac;
using LoaderBase;
using LoaderBase.Communication;
using LoaderServiceBase;
using LogModule;
using NotifyEventModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Enum;
using ProberInterfaces.GEM;
using ProberInterfaces.Proxies;
using SecsGemServiceInterface;
using SecsGemServiceProxy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Text;
using System.Xml;
using XGEMWrapper;

namespace XGemCommandProcessor
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class XGemCommander : IGemProcessorCore, INotifyPropertyChanged, IFactoryModule,
                                 ISecsGemServiceCallback, IDisposable
    {

        #region <remarks> PropertyChanged                           </remarks>
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

        #region <remarks> GEM Module Property & Field               </remarks>
        private object _lockObj = new object();

        public object lockObj
        {
            get { return _lockObj; }
            set { _lockObj = value; }
        }

        object backupInfoLock = new object();

        public bool Initialized { get; set; } = false;

        private IGEMCommManager GemCommManager = null; //GemCommander를 소유하고있는 Manager 객체입니다.
        private object WcfCommLockObj = new object();
        private bool IsDisposed = false;

        public GemAlarmBackupInfo GemAlarmBackupInfo { get; set; } = new GemAlarmBackupInfo();

        #region ==> Gem Service Proxy & Callback
        // Gem Service와 직접적으로 연결하는 Proxy입니다.
        private SecsGemServiceDirectProxy _SecsGemServiceProxy;
        private SecsGemServiceDirectProxy SecsGemServiceProxy
        {
            get { return _SecsGemServiceProxy; }
            set
            {
                _SecsGemServiceProxy = value;
            }
        }
        private ISecsGemService SecsGemClientCallback = null;
        #endregion

        public ISecsGemServiceHost CommandHostService = null;

        private GEMCEIDDictionaryParam GEMCEIDDictionaryParam { get; set; }
        //brett// Cell과의 GEM 연결이 끊어진 경우 Loader에 알려주기 위한 Callback Function 대리자
        public GEMDisconnectDelegate disconnectDelegate = null;

        #endregion

        #region <remarks> Import Dll </remarks>
        [StructLayout(LayoutKind.Sequential)]
        public struct SystemTime
        {
            public ushort Year;
            public ushort Month;
            public ushort DayOfWeek;
            public ushort Day;
            public ushort Hour;
            public ushort Minute;
            public ushort Second;
            public ushort Millisecond;
        };

        [DllImport("kernel32.dll")]
        public static extern bool SetLocalTime(ref SystemTime time);
        #endregion

        public void Proc_SetCommManager(IGEMCommManager gemManager)
        {
            GemCommManager = gemManager;
        }

        public bool HasSecsGemServiceProxy()
        {
            bool retVal = false;
            try
            {
                if (SecsGemClientCallback != null)
                {
                    ICommunicationObject commobj = SecsGemClientCallback as ICommunicationObject;
                    if (commobj != null)
                    {
                        if (commobj.State != CommunicationState.Faulted
                            || commobj.State != CommunicationState.Closed)
                        {
                            retVal = true;
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
        public ClientBase<ISecsGemService> GetSecsGemServiceProxyBase() => SecsGemServiceProxy;
        public ISecsGemService GetSecsGemServiceProxy() => SecsGemServiceProxy;
        public void SetSecsGemServiceProxy(ClientBase<ISecsGemService> proxy)
        {
            try
            {
                SecsGemServiceProxy = proxy as SecsGemServiceDirectProxy;
                if (SecsGemServiceProxy != null)
                {
                    SecsGemServiceProxy.InnerChannel.Faulted += GemServiceFaultedEventHandler;
                    SecsGemServiceProxy.InnerChannel.Closed += GemServiceFaultedEventHandler;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private void GemServiceFaultedEventHandler(object sender, EventArgs e)
        {
            try
            {
                if (this.GEMModule().GEMHostManualAction == GEMHOST_MANUALACTION.CONNECT || this.GEMModule().GEMHostManualAction == GEMHOST_MANUALACTION.UNDEFIND)
                {
                    DisConnectService();
                }

                if (this.GEMModule().GEMHostManualAction != GEMHOST_MANUALACTION.RECONNECT)
                {
                    this.GEMModule().GemCommManager.SecsCommInformData.Enable = SecsEnum_Enable.DISABLE;
                    LoggerManager.Debug("[SECS/GEM] Gem connect Error ( from Host )");
                    this.MetroDialogManager().ShowMessageDialog(
                        "Gem connect Error ( from Host )",
                        "The SECS/GEM connection with the host has been disconnected.", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetSecsGemServiceCallback(ISecsGemService callbackObj)
        {
            this.SecsGemClientCallback = callbackObj;
        }


        public bool GetRemoteConnectState(int stageIndex = -1)
        {
            bool retVal = false;
            try
            {
                if (CommandHostService != null)
                {
                    retVal = CommandHostService.CheckCommConnectivity(stageIndex);
                }
                else
                {
                    retVal = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        #region <remarks> WCF Callback Methods                      </remarks>
        /// <summary>
        ///     WCF 콜백 함수들.
        /// </summary>
        /// <remarks>
        ///     First Create : 2017-12-27, Semics R&D1, Jake Kim.
        /// </remarks>

        public bool CallBack_ConnectSuccess()
        {
            return true;
        }

        public bool Are_You_There()
        {
            return true;
        }

        public void CallBack_Close()
        {
            try
            {
                CloseGemServiceHost();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        ILoaderCommunicationManager _LoaderCommunicationManager { get; set; }
        public bool GetConnectState(int index = 0)
        {
            bool retVal = false;
            try
            {
                _LoaderCommunicationManager = this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
                var stage = _LoaderCommunicationManager.GetStage(index);
                if (stage != null)
                {

                    if (stage.StageInfo.IsConnected)
                    {
                        retVal = true;
                    }
                    else
                    {
                        retVal = false;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void RemoteActMsgReceive(RemoteActReqData msgData)
        {
            if (msgData == null)
                return;

            byte HCACK = 0x04;

            LoggerManager.Debug($"[GEM COMMANDER] OnRemoteCommandAction : {msgData.ActionType}");
            if (msgData.Stream == 2 && msgData.Function == 9)
            {
                S2F49SendAck(msgData.ObjectID, msgData.Stream, msgData.Function, msgData.Sysbyte, HCACK, msgData.Count);
            }

        }

        public void ECVChangeMsgReceive(EquipmentReqData msgData)
        {
            if (msgData == null)
                return;

            //byte HCACK = 0x04;

            LoggerManager.Debug($"[GEM COMMANDER] ECVChangeMsgReceive");

            long[] temp = new long[msgData.ECID.Count()];
            for (int i = 0; i < msgData.ECID.Count(); i++)
            {
                temp[i] = msgData.ECID[i];
            }

            //ProberInterfaces.GEM.EnumVidType type = ProberInterfaces.GEM.EnumVidType.ECID;
            ////this.GEMModule().GemCommManager.SetVariable(this.GEMModule().GemCommManager.GetOrgDataID(temp), new string[] { msgData.ECV.ToString() }, type);
            ////setResult = GemProcessor.Proc_SetECVChanged(vidLength, convertDataID, values);
            //UpdateToEcv(msgData);
            //OnGEMReqChangeECV(msgData.ObjectID, msgData.Count, temp, new string[1] { msgData.ECV });

        }

        


        #region <remarks> void OnCarrierActMsgRecive(CarrierActReqData msgData) </remarks>
        /// <summary>
        ///     GEM으로부터 메세지 수신. Carrier Command 처리.
        /// </summary>
        /// <remarks>
        ///     GEM -> Main.
        ///     S3F17
        ///     First Create : 2018-04-27, Semics R&D1, Jake Kim.
        /// </remarks>
        /// <param name="msgData">      메세지 정보          </param>
        public void OnCarrierActMsgRecive(CarrierActReqData msgData)
        {
            try
            {
                if (msgData == null)
                    return;

                byte CACK = 0;

                #region <remarks> CACK Define </remarks>
                // <summary>
                // CACK 정의
                // </summary>
                // <remarks>
                //      0 = Acknowledge, command has been performed.
                //      1 = Invalid command
                //      2 = Can not perform now
                //      3 = Invalid data or argument
                //      4 = Acknowledge, request will be performed with completion signaled later by an event.
                //      5 = Rejected. Invalid state.
                //      6 = Command performed with errors.
                //      7-63 Reserved.
                // </remarks>
                #endregion

                LoggerManager.Debug($"[GEM COMMANDER] OnRemoteCommandAction : {msgData.ActionType}");


                if (msgData.ActionType == EnumCarrierAction.PROCEEDWITHCARRIER)
                {
                    if (this.GEMModule().GemSysParam.ReceiveMessageType.Equals("SemicsGemReceiverSEKS"))
                    {
                        #region [STM_CATANIA] PTN, LOTID Error Check
                        (ushort ack, string msgtxt) ptnInfoAckData = (0, string.Empty);
                        (ushort ack, string msgtxt) lotInfoAckData = (0, string.Empty);

                        if (msgData is ProceedWithCarrierReqData proceedWithCarrierReqData)
                        {
                            CACK = 3;
                            ptnInfoAckData = CheckFoupNumberError(proceedWithCarrierReqData.PTN);

                            // PTN(=foupNumber)가 Error면 다음 정보들 체크하는게 의가 없기 때문에 정상일 경우만 다음 Error Check 진행.
                            if (ptnInfoAckData.ack == 0)
                            {
                                lotInfoAckData = CheckLotIdError(msgData.ActionType, proceedWithCarrierReqData.LotID, proceedWithCarrierReqData.PTN);
                                if (lotInfoAckData.ack == 0)
                                {
                                    CACK = 0;
                                }
                            }
                        }
                        else
                        {
                            CACK = 5;
                        }
                        #endregion

                        #region [STM_CATANIA] Send S3F17 Ack
                        if (SecsGemServiceProxy != null)
                        {
                            long pnObjectID = msgData.ObjectID;
                            SecsGemServiceProxy.MakeObject(ref pnObjectID);

                            SecsGemServiceProxy.SetListItem(pnObjectID, 2);
                            SecsGemServiceProxy.SetUint1Item(pnObjectID, CACK);
                            SecsGemServiceProxy.SetListItem(pnObjectID, msgData.Count);

                            // Lot Info
                            SecsGemServiceProxy.SetListItem(pnObjectID, 2);
                            SecsGemServiceProxy.SetUint2Item(pnObjectID, lotInfoAckData.ack);
                            SecsGemServiceProxy.SetStringItem(pnObjectID, lotInfoAckData.msgtxt);

                            SecsGemServiceProxy.SendSECSMessage(pnObjectID, msgData.Stream, msgData.Function + 1, msgData.Sysbyte);
                        }                    
                        #endregion
                    }
                    //else
                    //{
                    //    S3F17SendAck(msgData.ObjectID, msgData.Stream, msgData.Function, msgData.Sysbyte, CACK, msgData.Count);
                    //}
                }
                else if (msgData.ActionType == EnumCarrierAction.PROCESSEDWITHCELLSLOT)
                {
                    if (this.GEMModule().GemSysParam.ReceiveMessageType.Equals("SemicsGemReceiverSEKS"))
                    {
                        #region [STM_CATANIA] PTN, CELLINFO, SLOTINFO, LOTID Error Check
                        (ushort ack, string msgtxt) ptnInfoAckData = (0, string.Empty);
                        (ushort ack, string msgtxt) cellInfoAckData = (0, string.Empty);
                        (ushort ack, string msgtxt) slotInfoAckData = (0, string.Empty);
                        (ushort ack, string msgtxt) lotInfoAckData = (0, string.Empty);

                        if (msgData is ProceedWithCellSlotActReqData proceedWithCellSlotActReqData)
                        {
                            #region sub data code def
                            //  0 - ok
                            //  1 - unknown object
                            //  2 - unknown class
                            //  3 - unknown object instance
                            //  4 - unknown attribute type
                            //  5 - read-only attribute
                            //  6 - unknown class
                            //  7 - invalid attribute value
                            //  8 - syntax error
                            //  9 - verification error
                            //  10 - validation error
                            #endregion

                            CACK = 0x03;
                            ptnInfoAckData = CheckFoupNumberError(proceedWithCellSlotActReqData.PTN);

                            // PTN(=foupNumber)가 Error면 다음 정보들 체크하는게 의가 없기 때문에 정상일 경우만 다음 Error Check 진행.
                            if (ptnInfoAckData.ack == 0)
                            {
                                cellInfoAckData = CheckCellInfoError(proceedWithCellSlotActReqData.CellMap, proceedWithCellSlotActReqData.PTN);
                                slotInfoAckData = CheckSlotInfoError(proceedWithCellSlotActReqData.SlotMap, proceedWithCellSlotActReqData.PTN);
                                lotInfoAckData = CheckLotIdError(msgData.ActionType, proceedWithCellSlotActReqData.LOTID, proceedWithCellSlotActReqData.PTN);

                                if (cellInfoAckData.ack == 0 &&
                                    slotInfoAckData.ack == 0 &&
                                    lotInfoAckData.ack == 0)
                                {
                                    CACK = 0x00;
                                }
                            }
                        }
                        else
                        {
                            CACK = 5;
                        }
                        #endregion

                        #region [STM_CATANIA] Send S3F17 Ack
                        if (SecsGemServiceProxy != null)
                        {
                            long pnObjectID = msgData.ObjectID;
                            SecsGemServiceProxy.MakeObject(ref pnObjectID);

                            SecsGemServiceProxy.SetListItem(pnObjectID, 2);
                            SecsGemServiceProxy.SetUint1Item(pnObjectID, CACK);
                            SecsGemServiceProxy.SetListItem(pnObjectID, msgData.Count);

                            // Lot Info
                            SecsGemServiceProxy.SetListItem(pnObjectID, 2);
                            SecsGemServiceProxy.SetUint2Item(pnObjectID, lotInfoAckData.ack);
                            SecsGemServiceProxy.SetStringItem(pnObjectID, lotInfoAckData.msgtxt);

                            // Cell Info
                            SecsGemServiceProxy.SetListItem(pnObjectID, 2);
                            SecsGemServiceProxy.SetUint2Item(pnObjectID, cellInfoAckData.ack);
                            SecsGemServiceProxy.SetStringItem(pnObjectID, cellInfoAckData.msgtxt);

                            // Slot Info
                            SecsGemServiceProxy.SetListItem(pnObjectID, 2);
                            SecsGemServiceProxy.SetUint2Item(pnObjectID, slotInfoAckData.ack);
                            SecsGemServiceProxy.SetStringItem(pnObjectID, slotInfoAckData.msgtxt);

                            SecsGemServiceProxy.SendSECSMessage(pnObjectID, msgData.Stream, msgData.Function + 1, msgData.Sysbyte);
                        }
                        #endregion
                    }
                    //else
                    //{
                    //    S3F17SendAck(msgData.ObjectID, msgData.Stream, msgData.Function, msgData.Sysbyte, CACK, msgData.Count);
                    //}
                }
                else if (msgData.ActionType == EnumCarrierAction.PROCEEDWITHSLOT)
                {
                    //v22_merge//
                    if (!this.GEMModule().GemSysParam.ReceiveMessageType.Equals("SemicsGemReceiverSEKX"))
                    {
                        if (true)
                        {
                            CACK = 0;
                        }
                        else
                        {

                        }
                        S3F17SendAck(msgData.ObjectID, msgData.Stream, msgData.Function, msgData.Sysbyte, CACK, msgData.Count);

                        if (CACK == 0 || CACK == 4)
                        {
                            this.EventManager().RaisingEvent(typeof(SlotMapVarifyDoneEvent).FullName);
                        }
                    }
                }
                else if (msgData.ActionType == EnumCarrierAction.CANCELCARRIER | msgData.ActionType == EnumCarrierAction.RELEASECARRIER)
                {
                    if (this.GEMModule().GemSysParam.ReceiveMessageType.Equals("SemicsGemReceiverSEKS"))
                    {
                        #region [STM_CATANIA] PTN Error Check
                        (ushort ack, string msgtxt) ptnInfoAckData = (0, string.Empty);
                        (ushort ack, string msgtxt) lotInfoAckData = (0, string.Empty);
                        if (msgData is CarrieActReqData cancelCarrierReqData)
                        {
                            CACK = 3;
                            ptnInfoAckData = CheckFoupNumberError(cancelCarrierReqData.PTN);
                            // PTN(=foupNumber)가 Error면 다음 정보들 체크하는게 의가 없기 때문에 정상일 경우만 다음 Error Check 진행.
                            if (ptnInfoAckData.ack == 0)
                            {
                                lotInfoAckData = CheckLotIdError(msgData.ActionType, cancelCarrierReqData.LOTID, cancelCarrierReqData.PTN);
                                if (lotInfoAckData.ack == 0)
                                {
                                    CACK = 0;
                                }
                            }
                        }
                        else
                        {
                            CACK = 5;
                        }
                        #endregion

                        #region [STM_CATANIA] Send S3F17 Ack
                        if (SecsGemServiceProxy != null)
                        {
                            long pnObjectID = msgData.ObjectID;
                            SecsGemServiceProxy.MakeObject(ref pnObjectID);

                            SecsGemServiceProxy.SetListItem(pnObjectID, 2);
                            SecsGemServiceProxy.SetUint1Item(pnObjectID, CACK);
                            SecsGemServiceProxy.SetListItem(pnObjectID, msgData.Count);

                            // Lot Info
                            SecsGemServiceProxy.SetListItem(pnObjectID, 2);
                            SecsGemServiceProxy.SetUint2Item(pnObjectID, lotInfoAckData.ack);
                            SecsGemServiceProxy.SetStringItem(pnObjectID, lotInfoAckData.msgtxt);

                            SecsGemServiceProxy.SendSECSMessage(pnObjectID, msgData.Stream, msgData.Function + 1, msgData.Sysbyte);
                        }
                        #endregion
                    }
                    else 
                    {
                        if (!this.GEMModule().GemSysParam.ReceiveMessageType.Equals("SemicsGemReceiverSEKX"))
                        {
                            CACK = 0x04;
                            S3F17SendAck(msgData.ObjectID, msgData.Stream, msgData.Function, msgData.Sysbyte, CACK, msgData.Count);
                        }
                    }
                }

                if (CACK == 0x00)
                {
                    this.GEMModule().ExcuteCarrierCommandAction(msgData, this);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region  <remarks> OnRemoteCommandAction(EnumRemoteCommand command, RemoteActReqData msgData) </remarks>
        object device_lockobj = new object();


        public void OnRemoteCommandAction(RemoteActReqData msgData)
        {
            if (msgData == null)
                return;

            try
            {
                LoggerManager.Debug($"[GEM COMMANDER] OnRemoteCommandAction : {msgData.ActionType}");
                this.GEMModule().ExcuteRemoteCommandAction(msgData, this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        #endregion
        public void OnDefineReportRecive(SecsGemDefineReport report)
        {
            try
            {
                if (CommandHostService != null)
                    CommandHostService.SetSecsGemDefineReport(report);

                // Debug Log
                LoggerManager.Debug("Gem DefineReport");
                foreach (var ceid in report.CEIDs)
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
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        

        #region <remarks> void OnSECSMessageReceive(long nObjectID, long nStream, long nFunction, long nSysbyte) </remarks>
        /// <summary>
        ///     GEM으로부터 메세지 수신. Remote Msg & Carrier Command 처리.
        /// </summary>
        /// <remarks>
        ///     GEM -> Main.
        ///     S2F21, S3F17, S3F27
        ///     First Create : 2018-04-26, Semics R&D1, Jake Kim.
        /// </remarks>
        /// <param name="nObjectID">    ID              </param>
        /// <param name="nStream">      Stream Number   </param>
        /// <param name="nFunction">    Function Number </param>
        /// <param name="nSysbyte">     Messege Number  </param>
        public void OnSECSMessageReceive(long nObjectID, long nStream, long nFunction, long nSysbyte)
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region <remarks> long SendAck(long pnObjectID, long nStream, long nFunction, long nSysbyte, byte CAACK, long nCount) </remarks>
        /// <summary>
        ///     대답을 하는 함수.
        /// </summary>
        /// <remarks>
        ///     GEM -> Main.
        /// </remarks>
        /// <param name="pnObjectID">   ID                          </param>
        /// <param name="nStream">      Stream Number               </param>
        /// <param name="nFunction">    Function Number             </param>
        /// <param name="nSysbyte">     Messege Number              </param>
        /// <param name="CAACK">        Carrier Action Acknowledge  </param>
        /// <param name="nCount">       Error List Size             </param>
        /// <returns>Message Send 결과</returns>
        public long SendAck(long pnObjectID, long nStream, long nFunction, long nSysbyte, byte CAACK, long nCount)
        {
            long retVal = 0;
            try
            {
                SecsGemServiceProxy?.MakeObject(ref pnObjectID);

                SecsGemServiceProxy?.SetListItem(pnObjectID, 2);
                //SecsGemServiceProxy?.SetUint1Item(pnObjectID, CAACK);
                SecsGemServiceProxy?.SetBinaryItem(pnObjectID, CAACK);
                SecsGemServiceProxy?.SetListItem(pnObjectID, nCount);

                for (int i = 0; i < nCount; i++)
                {
                    SecsGemServiceProxy?.SetListItem(pnObjectID, 2);
                    SecsGemServiceProxy?.SetUint2Item(pnObjectID, 0);
                    SecsGemServiceProxy?.SetStringItem(pnObjectID, "");
                }

                retVal = SecsGemServiceProxy?.SendSECSMessage(pnObjectID, nStream, nFunction + 1, nSysbyte) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion
       

        #region <remarks> long S2F49SendAck(long pnObjectID, long nStream, long nFunction, long nSysbyte, byte HAACK, long nCount) </remarks>
        /// <summary>
        /// S2F49의 대답을 하는 함수.
        /// </summary>        
        /// <param name="pnObjectID">   ID                          </param>
        /// <param name="nStream">      Stream Number               </param>
        /// <param name="nFunction">    Function Number             </param>
        /// <param name="nSysbyte">     Messege Number              </param>
        /// <param name="HAACK">        Remote Action Acknowledge  </param>
        /// <param name="nCount">       Error List Size             </param>
        /// <returns></returns>
        private long S2F49SendAck(long pnObjectID, long nStream, long nFunction, long nSysbyte, byte HAACK, long nCount)
        {
            //long pnObjectID = 0;
            //byte HAACK = 0x04;

            //MakeObject(ref pnObjectID);
            //SetListItem(pnObjectID, 2);
            //SetBinaryItem(pnObjectID, HAACK);
            //SetListItem(pnObjectID, 0);
            //SendSECSMessage(pnObjectID, nStream, nFunction + 1, nSysbyte);


            long retVal = 0;
            try
            {
                SecsGemServiceProxy.MakeObject(ref pnObjectID);
                SecsGemServiceProxy.SetListItem(pnObjectID, 2);
                SecsGemServiceProxy.SetUint1Item(pnObjectID, HAACK);
                SecsGemServiceProxy.SetListItem(pnObjectID, 0);//TODO: nCount를 0으로 줘서 일단 다 정상작동한다고 알려준다...CPNAME랑 CEPACK에 무슨 값을 넣어야될지 몰라성..

                //SecsGemServiceProxy.SetListItem(pnObjectID, nCount);
                //for (int i = 0; i < nCount; i++)
                //{
                //    SecsGemServiceProxy.SetListItem(pnObjectID, 2);
                //    SecsGemServiceProxy.SetUint2Item(pnObjectID, 0);// CPNAME
                //    SecsGemServiceProxy.SetStringItem(pnObjectID, "");// CEPACK
                //}

                retVal = SecsGemServiceProxy.SendSECSMessage(pnObjectID, nStream, nFunction + 1, nSysbyte);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion

        #region <remarks> long S3F17SendAck(long pnObjectID, long nStream, long nFunction, long nSysbyte, byte CAACK, long nCount) </remarks>
        /// <summary>
        ///     S3F17의 대답을 하는 함수.
        /// </summary>
        /// <remarks>
        ///     GEM -> Main.
        ///     S3F18
        ///     First Create : 2018-04-26, Semics R&D1, Jake Kim.
        /// </remarks>
        /// <param name="pnObjectID">   ID                          </param>
        /// <param name="nStream">      Stream Number               </param>
        /// <param name="nFunction">    Function Number             </param>
        /// <param name="nSysbyte">     Messege Number              </param>
        /// <param name="CAACK">        Carrier Action Acknowledge  </param>
        /// <param name="nCount">       Error List Size             </param>
        /// <returns>Message Send 결과</returns>
        public long S3F17SendAck(long pnObjectID, long nStream, long nFunction, long nSysbyte, byte CAACK, long nCount)
        {
            long retVal = 0;
            try
            {
                if (SecsGemServiceProxy != null)
                {
                    SecsGemServiceProxy?.MakeObject(ref pnObjectID);

                    SecsGemServiceProxy?.SetListItem(pnObjectID, 2);
                    SecsGemServiceProxy?.SetUint1Item(pnObjectID, CAACK);
                    SecsGemServiceProxy?.SetListItem(pnObjectID, nCount);

                    for (int i = 0; i < nCount; i++)
                    {
                        SecsGemServiceProxy?.SetListItem(pnObjectID, 2);
                        SecsGemServiceProxy?.SetUint2Item(pnObjectID, 0);
                        SecsGemServiceProxy?.SetStringItem(pnObjectID, "");
                    }
                    

                    retVal = SecsGemServiceProxy.SendSECSMessage(pnObjectID, nStream, nFunction + 1, nSysbyte);
                }
                else
                {
                    // TODO : 

                    LoggerManager.Error($"[XGemCommander], S3F17SendAck() : SecsGemServiceProxy is null.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion

        #region <remarks> long S3F17SendAckWithMsgData(long pnObjectID, long nStream, long nFunction, long nSysbyte, byte CAACK, long nCount) </remarks>
        /// <summary>
        ///     S3F17의 대답을 하는 함수.
        /// </summary>
        /// <remarks>
        ///     GEM -> Main.
        ///     S3F18
        /// </remarks>
        /// <param name="pnObjectID">   ID                          </param>
        /// <param name="nStream">      Stream Number               </param>
        /// <param name="nFunction">    Function Number             </param>
        /// <param name="nSysbyte">     Messege Number              </param>
        /// <param name="CAACK">        Carrier Action Acknowledge  </param>
        /// <param name="nCount">       Error List Size             </param>
        /// <returns>Message Send 결과</returns>
        /// S3F17SendAck(msgData.ObjectID, msgData.Stream, msgData.Function, msgData.Sysbyte, CACK, msgData.Count);
        /// S3F17SendAckWithReqData(msgData, CAACK)
        private long S3F17SendAckWithReqData(long pnObjectID, long nStream, long nFunction, long nSysbyte, byte CAACK, long nCount)
        {
            long retVal = 0;
            try
            {
                if (SecsGemServiceProxy != null)
                {
                    SecsGemServiceProxy.MakeObject(ref pnObjectID);

                    SecsGemServiceProxy.SetListItem(pnObjectID, 2);
                    SecsGemServiceProxy.SetUint1Item(pnObjectID, CAACK);
                    SecsGemServiceProxy.SetListItem(pnObjectID, nCount);

                    for (int i = 0; i < nCount; i++)
                    {
                        SecsGemServiceProxy.SetListItem(pnObjectID, 2);
                        SecsGemServiceProxy.SetUint2Item(pnObjectID, 0);
                    }
                    SecsGemServiceProxy.SetStringItem(pnObjectID, "");

                    retVal = SecsGemServiceProxy?.SendSECSMessage(pnObjectID, nStream, nFunction + 1, nSysbyte) ?? -1; ;
                }
                else
                {
                    // TODO : 

                    LoggerManager.Error($"[XGemCommander], S3F17SendAck() : SecsGemServiceProxy is null.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion

        #region <remarks> long S3F17SendAck(long pnObjectID, long nStream, long nFunction, long nSysbyte, byte CAACK, long nCount) </remarks>
        /// <summary>
        ///     S3F17의 대답을 하는 함수.
        /// </summary>
        /// <remarks>
        ///     GEM -> Main.
        ///     S3F18
        ///     First Create : 2018-04-26, Semics R&D1, Jake Kim.
        /// </remarks>
        /// <param name="pnObjectID">   ID                          </param>
        /// <param name="nStream">      Stream Number               </param>
        /// <param name="nFunction">    Function Number             </param>
        /// <param name="nSysbyte">     Messege Number              </param>
        /// <param name="CAACK">        Carrier Action Acknowledge  </param>
        /// <param name="nCount">       Error List Size             </param>
        /// <returns>Message Send 결과</returns>
        private long S3F17SendErrorAck(long pnObjectID, long nStream, long nFunction, long nSysbyte, byte CAACK, long nCount)
        {
            long retVal = 0;
            try
            {
                if (SecsGemServiceProxy != null)
                {
                    SecsGemServiceProxy.MakeObject(ref pnObjectID);

                    SecsGemServiceProxy.SetListItem(pnObjectID, 2);
                    SecsGemServiceProxy.SetUint1Item(pnObjectID, CAACK);
                    SecsGemServiceProxy.SetListItem(pnObjectID, nCount);

                    for (int i = 0; i < nCount; i++)
                    {
                        SecsGemServiceProxy.SetListItem(pnObjectID, 2);
                        SecsGemServiceProxy.SetUint2Item(pnObjectID, 0);
                    }
                    SecsGemServiceProxy.SetStringItem(pnObjectID, "");

                    retVal = SecsGemServiceProxy.SendSECSMessage(pnObjectID, nStream, nFunction + 1, nSysbyte);
                }
                else
                {
                    // TODO : 

                    LoggerManager.Error($"[XGemCommander], S3F17SendAck() : SecsGemServiceProxy is null.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion

        #region <remarks> long S6F12SendAck(long pnObjectID, long nStream, long nFunction, long nSysbyte, byte CAACK, long nCount) </remarks>
        /// <summary>
        ///     S2F11의 대답을 하는 함수.
        /// </summary>
        /// <param name="pnObjectID">   ID                          </param>
        /// <param name="nStream">      Stream Number               </param>
        /// <param name="nFunction">    Function Number             </param>
        /// <param name="nSysbyte">     Messege Number              </param>
        /// <param name="CAACK">        Carrier Action Acknowledge  </param>
        /// <param name="nCount">       Error List Size             </param>
        /// <returns>Message Send 결과</returns>
        private long S6F12SendAck(long pnObjectID, long nStream, long nFunction, long nSysbyte, byte CAACK, long nCount)
        {
            long retVal = 0;
            try
            {
                SecsGemServiceProxy.MakeObject(ref pnObjectID);

                SecsGemServiceProxy.SetListItem(pnObjectID, 2);
                SecsGemServiceProxy.SetUint1Item(pnObjectID, CAACK);
                SecsGemServiceProxy.SetListItem(pnObjectID, nCount);

                for (int i = 0; i < nCount; i++)
                {
                    SecsGemServiceProxy.SetListItem(pnObjectID, 2);
                    SecsGemServiceProxy.SetUint2Item(pnObjectID, 0);
                    SecsGemServiceProxy.SetStringItem(pnObjectID, "");
                }

                retVal = SecsGemServiceProxy.SendSECSMessage(pnObjectID, nStream, nFunction + 1, nSysbyte);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion

        #region <remarks> long S3F27SendAck(long pnObjectID, long nStream, long nFunction, long nSysbyte, byte CAACK, long nCount) </remarks>
        public long S3F27SendAck(long pnObjectID, long nStream, long nFunction, long nSysbyte, byte CAACK, long nCount, List<CarrierChangeAccessModeResult> result)
        {
            long retVal = -1;
            try
            {
                SecsGemServiceProxy.MakeObject(ref pnObjectID);

                SecsGemServiceProxy.SetListItem(pnObjectID, 2);
                SecsGemServiceProxy.SetUint1Item(pnObjectID, CAACK);
                SecsGemServiceProxy.SetListItem(pnObjectID, result.Count);

                for (int i = 0; i < result.Count; i++)
                {
                    SecsGemServiceProxy.SetListItem(pnObjectID, 3);
                    SecsGemServiceProxy.SetUint1Item(pnObjectID, Convert.ToByte(result[i].FoupNumber));
                    SecsGemServiceProxy.SetUint2Item(pnObjectID, Convert.ToUInt16(result[i].ErrorCode));
                    SecsGemServiceProxy.SetStringItem(pnObjectID, result[i].ErrText);
                }

                retVal = SecsGemServiceProxy.SendSECSMessage(pnObjectID, nStream, nFunction + 1, nSysbyte);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion

        #region long MakeListObject(List<object> value)
        public long MakeListObject(object value)
        {
            long retVal = -1;
            try
            {
                SecsGemServiceProxy.MakeObject(ref retVal);
                MakeList(retVal, value);
                //SecsGemServiceProxy.CloseObject(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public object MakeList(long objectID, object values)
        {
            object ret = null;
            try
            {                

                if (values is IList)
                {
                    IList tolist = values as IList;
                    SecsGemServiceProxy.SetListItem(objectID, tolist.Count);

                    for (int i = 0; i < tolist.Count; i++)
                    {                        
                        if (tolist[i] is IList)
                        {
                            MakeList(objectID, (object)tolist[i]);
                        }
                        else
                        {
                            SetItemFromType(objectID, tolist[i]);
                        }
                    }
                }
                else
                {
                    SetItemFromType(objectID, values);
                }


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }

        public void SetItemFromType(long objectid, object value)
        {
            try
            {
                if (value.GetType() == typeof(byte) ||
                    value.GetType() == typeof(Byte))
                {
                    SecsGemServiceProxy.SetInt1Item(objectid, (sbyte)value);
                }
                else if (value.GetType() == typeof(int) ||
                    value.GetType() == typeof(Int16) ||
                    value.GetType() == typeof(Int32) ||
                    value.GetType() == typeof(Int64))
                {
                    SecsGemServiceProxy.SetInt8Item(objectid, (int)value);
                }
                else if (value.GetType() == typeof(UInt16) ||
                        value.GetType() == typeof(UInt32) ||
                        value.GetType() == typeof(UInt64) ||
                        value.GetType() == typeof(uint) ||
                        value.GetType() == typeof(ulong) ||
                        value.GetType() == typeof(ushort))
                {
                    SecsGemServiceProxy.SetUint8Item(objectid, (uint)value);
                }
                else if (value.GetType() == typeof(double) ||
                         value.GetType() == typeof(float) ||
                         value.GetType() == typeof(Double)
                         )
                {
                    SecsGemServiceProxy.SetFloat8Item(objectid, (double)value);
                }
                else if (value.GetType() == typeof(bool) ||
                         value.GetType() == typeof(Boolean)
                    )
                {
                    SecsGemServiceProxy.SetBoolItem(objectid, (bool)value);
                }
                else
                {
                    SecsGemServiceProxy.SetStringItem(objectid, (string)value);//ascii
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        #endregion



        #region <remarks> void OnGEMControlStateChange(SecsControlStateEnum ControlState) </remarks>
        /// <summary>
        ///     GEM의 Control State가 변경되었을 때의 Event 처리 Function.
        /// </summary>
        /// <remarks>
        ///     GEM -> Main.
        ///     First Create : 2019-05-16, Semics R&D1, Jake Kim.
        /// </remarks>
        /// <param name="ControlState">      Control State          </param>
        public void OnGEMControlStateChange(SecsControlStateEnum ControlState)
        {
            try
            {
                this.GemCommManager.SecsCommInformData.ControlState = (SecsEnum_ControlState)((int)ControlState);
                LoggerManager.Debug($"[{this.GetType()}] GEM Control State : {ControlState.ToString()}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        #endregion

        #region <remarks> void OnGEMStateEvent(SecsGemStateEnum GemState) </remarks>
        /// <summary>
        ///     GEM의 상태가 변경되었을 때의 Event 처리 Function.
        /// </summary>
        /// <remarks>
        ///     GEM -> Main.
        ///     First Create : 2019-05-16, Semics R&D1, Jake Kim.
        /// </remarks>
        /// <param name="GemState">      GemState          </param>
        public void OnGEMStateEvent(SecsGemStateEnum GemState)
        {
            try
            {
                if (GemState == SecsGemStateEnum.EXECUTE)
                {
                    FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetEntryAssembly().Location);
                    string version = fvi.ProductVersion;

                    SecsGemServiceProxy.GEMSetVariable(1, GemCommManager.GetOrgDataID(1200), new string[] { version });
                    SecsGemServiceProxy.GEMSetVariable(1, GemCommManager.GetOrgDataID(1201), new string[] { "OPERA" });

                    //GemCommManager.CommEnable();
                }
                else
                {
                    if (this.GemCommManager.SecsCommInformData.CommunicationState == SecsEnum_CommunicationState.COMMUNICATING)
                    {
                        this.GemCommManager.SecsCommInformData.CommunicationState = SecsEnum_CommunicationState.COMM_DISABLED;
                    }

                    if (this.GemCommManager.SecsCommInformData.CommunicationState == SecsEnum_CommunicationState.COMMUNICATING ||
                        this.GemCommManager.SecsCommInformData.ControlState == SecsEnum_ControlState.ONLINE_LOCAL)
                    {
                        this.GemCommManager.SecsCommInformData.ControlState = SecsEnum_ControlState.EQ_OFFLINE;
                    }
                }

                LoggerManager.Debug($"[{this.GetType()}] GEM State : {GemState}");
                this.GemCommManager.SecsCommInformData.GemState = GemState;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region <remarks> void OnGEMCommStateChange(SecsCommStateEnum CommState) </remarks>
        /// <summary>
        ///     GEM의 Communication State가 변경되었을 때의 Event 처리 Function.
        /// </summary>
        /// <remarks>
        ///     GEM -> Main.
        ///     First Create : 2019-05-16, Semics R&D1, Jake Kim.
        /// </remarks>
        /// <param name="CommState">      Communication State          </param>
        public void OnGEMCommStateChange(SecsCommStateEnum CommState)
        {
            string getInfo = string.Empty;
            string[] getCommDataName = new string[]
            {   GemGlobalData.IP,       GemGlobalData.Port,             GemGlobalData.Active,
                GemGlobalData.DeviceID, GemGlobalData.LinkTestInterval, GemGlobalData.RetryLimit,
                GemGlobalData.T3,       GemGlobalData.T5,
                GemGlobalData.T6,       GemGlobalData.T7,               GemGlobalData.T8};

            try
            {
                if (CommState == SecsCommStateEnum.COMM_DISABLED ||
                    CommState == SecsCommStateEnum.WAIT_DELAY ||
                    CommState == SecsCommStateEnum.WAIT_CRA)
                {
                    this.GEMModule().CommunicationState = EnumCommunicationState.DISCONNECT;
                    // WAIT_DELAY -> WAIT_CRA 로 변경되었을경우 Server 와의 연결이 끊긴것으로 보고 알람 발생해야함
                    // 또는 CommunicationState 가 CONNECTED 였다가, DISCONNECT 로변경될 경우 알람 발생 해야함.
                }
                else
                {
                    this.GEMModule().CommunicationState = EnumCommunicationState.CONNECTED;
                }
                this.GemCommManager.SecsCommInformData.CommunicationState = (SecsEnum_CommunicationState)CommState;

                getInfo = GetCommInfo(getCommDataName);
                SetCommInfo(getInfo, getCommDataName);

                long nCount = 3;
                long[] naId = new long[3];
                string[] saVal = new string[3];

                naId[0] = 3511; // OnlineSub State 값 조회
                naId[1] = 3514; // InitControl State 값 조회
                naId[2] = 3509; // OfflineSub State 값 조회
                long ret = this.SecsGemServiceProxy.GEMGetVariable(nCount, ref naId, ref saVal);
                if (ret == 0 && saVal != null && 0 < saVal.Length)
                {
                    //int iControlState = -1;
                    int onlinesubStateVal = -1;
                    int initControlStateVal = -1;
                    int offlinesubStateVal = -1;
                    bool onlineParseResult = false;
                    bool initParseResult = false;
                    bool offlineParseResult = false;

                    onlineParseResult = int.TryParse(saVal[0], out onlinesubStateVal);
                    initParseResult = int.TryParse(saVal[1], out initControlStateVal);
                    offlineParseResult = int.TryParse(saVal[2], out offlinesubStateVal);

                    if (initParseResult)
                    {
                        switch (initControlStateVal)
                        {
                            case 1: //OFFLINE
                                if (offlineParseResult)
                                {
                                    switch (offlinesubStateVal)
                                    {
                                        case 1:
                                            this.GemCommManager.SecsCommInformData.ControlState = SecsEnum_ControlState.EQ_OFFLINE;
                                            break;
                                        case 2:
                                            this.GemCommManager.SecsCommInformData.ControlState = SecsEnum_ControlState.ATTEMPT_ONLINE;
                                            break;
                                        case 3:
                                            this.GemCommManager.SecsCommInformData.ControlState = SecsEnum_ControlState.HOST_OFFLINE;
                                            break;
                                    }
                                }
                                break;
                            case 2: //ONLINE
                                if (onlineParseResult)
                                {
                                    switch (onlinesubStateVal)
                                    {
                                        case 1:
                                            this.GemCommManager.SecsCommInformData.ControlState = SecsEnum_ControlState.ONLINE_LOCAL;
                                            break;
                                        case 2:
                                            this.GemCommManager.SecsCommInformData.ControlState = SecsEnum_ControlState.ONLINE_REMOTE;
                                            break;
                                    }
                                }
                                break;
                        }
                    }

                    //if (parseResult != false && ((((int)SecsEnum_ControlState.UNKNOWN) < iControlState) && (iControlState <= ((int)SecsEnum_ControlState.ONLINE_REMOTE))))
                    //{
                    //    this.GemCommManager.SecsCommInformData.ControlState = (SecsEnum_ControlState)iControlState;
                    //}
                }

                LoggerManager.Debug($"[{this.GetType()}] GEM Communication State : {CommState}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region <remarks> void OnGEMRspGetDateTime(string sSystemTime) </remarks>
        /// <summary>
        ///     Host로부터 시간을 가져올 때 일어나는 Event에 대한 함수.
        /// </summary>
        /// <remarks>
        ///     GEM -> Main.
        ///     First Create : 2019-05-16, Semics R&D1, Jake Kim.
        /// </remarks>
        /// <param name="sSystemTime">      시간          </param>
        public void OnGEMRspGetDateTime(string sSystemTime)
        {
            try
            {
                this.GemCommManager.SecsCommInformData.RequestTimeStr = sSystemTime;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region <remarks> void OnGEMTerminalMultiMessage(long nTid, long nCount, string[] psMsg) </remarks>
        /// <summary>
        ///     Multi Termial Message가 왔을 때 발생하는 Event를 처리하는 함수.
        /// </summary>
        /// <remarks>
        ///     GEM -> Main.
        ///     First Create : 2019-05-16, Semics R&D1, Jake Kim.
        /// </remarks>
        /// <param name="nTid">      Terminal ID          </param>
        /// <param name="nCount">    메세지 갯수          </param>
        /// <param name="psMsg">     메세지          </param>
        public void OnGEMTerminalMultiMessage(long nTid, long nCount, string[] psMsg)
        {
            try
            {
                StringBuilder TerminalData = new StringBuilder();

                TerminalData.Append("Multi\n");

                for (int i = 0; i < nCount; i++)
                {
                    TerminalData.Append(psMsg[i]);
                    TerminalData.Append("\n");
                }

                System.Windows.Forms.MessageBox.Show(TerminalData.ToString());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region <remarks> void OnGEMTerminalMessage(long nTid, string sMsg) </remarks>
        /// <summary>
        ///     Termial Message가 왔을 때 발생하는 Event를 처리하는 함수.
        /// </summary>
        /// <remarks>
        ///     GEM -> Main.
        ///     First Create : 2019-05-16, Semics R&D1, Jake Kim.
        /// </remarks>
        /// <param name="nTid">      Terminal ID          </param>
        /// <param name="sMsg">     메세지          </param>
        public void OnGEMTerminalMessage(long nTid, string sMsg)
        {
            try
            {
                System.Windows.Forms.MessageBox.Show("Single\n" + sMsg);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        #endregion

        #region <remarks> void OnGEMReqDateTime(long nMsgId, string sSystemTime) </remarks>

        /// <summary>
        ///     Termial Message가 왔을 때 발생하는 Event를 처리하는 함수. (F2, S31)
        /// </summary>
        /// <remarks>
        ///     GEM -> Main.
        ///     First Create : 2019-05-16, Semics R&D1, Jake Kim.
        /// </remarks>
        /// <param name="nTid">      Terminal ID          </param>
        /// <param name="sMsg">     메세지          </param>
        public void OnGEMReqDateTime(long nMsgId, string sSystemTime)
        {
            long TIACK = 0;
            ///0 - ok
            ///1 - not done
            try
            {
                //A:16 YYYYMMDDHHMMSScc



                SystemTime updatedTime = new SystemTime();
                updatedTime.Year = Convert.ToUInt16(sSystemTime.Substring(0, 4));
                updatedTime.Month = Convert.ToUInt16(sSystemTime.Substring(4, 2));
                updatedTime.Day = Convert.ToUInt16(sSystemTime.Substring(6, 2));
                updatedTime.Hour = Convert.ToUInt16(sSystemTime.Substring(8, 2));
                updatedTime.Minute = Convert.ToUInt16(sSystemTime.Substring(10, 2));
                updatedTime.Second = Convert.ToUInt16(sSystemTime.Substring(12, 2));
                updatedTime.Millisecond = Convert.ToUInt16(sSystemTime.Substring(14, 2));
                // Call the unmanaged function that sets the new date and time instantly
                SetLocalTime(ref updatedTime);
                LoggerManager.Debug($"[GEM] Update System Time : {updatedTime}");

                CommandHostService.GEMRspGetDateTime(nMsgId, sSystemTime);
                SecsGemServiceProxy.GEMRspDateTime(nMsgId, TIACK);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                TIACK = 1;
                SecsGemServiceProxy.GEMRspDateTime(nMsgId, TIACK);
            }

            //
        }
        #endregion

        #region <remarks> void OnGEMReqRemoteCommand(long nMsgId, EnumRemoteCommand Rcmd, long nCount, string[] psNames, string[] psVals) </remarks>
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        ///     GEM -> Main.
        ///     First Create : 2019-05-20, Semics R&D1, Jake Kim.
        /// </remarks>
        /// <param name="nMsgId">             </param>
        /// <param name="Rcmd">               </param>
        /// <param name="nCount">             </param>
        /// <param name="psNames">            </param>
        /// <param name="psVals">             </param>
        public void OnGEMReqRemoteCommand(long nMsgId, EnumRemoteCommand Rcmd, long nCount, string[] psNames, string[] psVals)
        {
            try
            {
                List<string> psNameList = null;
                List<string> psValList = null;

                if (0 < nCount && psNames != null && psVals != null)
                {
                    psNameList = new List<string>(psNames);
                    psValList = new List<string>(psVals);
                }

                String[] CPName = null;
                long[] CPVal = null;
                long retHCAck = 0;


                if (Rcmd == EnumRemoteCommand.ABORT)
                {

                }
                else if (Rcmd == EnumRemoteCommand.CC_START)
                {

                }
                else if (Rcmd == EnumRemoteCommand.DLRECIPE)
                {

                }
                else if (Rcmd == EnumRemoteCommand.JOB_CANCEL)
                {

                }
                else if (Rcmd == EnumRemoteCommand.JOB_CREATE)
                {

                }
                else if (Rcmd == EnumRemoteCommand.ONLINE_LOCAL)
                {

                }
                else if (Rcmd == EnumRemoteCommand.ONLINE_REMOTE)
                {

                }
                else if (Rcmd == EnumRemoteCommand.ONLINEPP_SELECT)
                {

                }
                else if (Rcmd == EnumRemoteCommand.PAUSE)
                {
                }
                else if (Rcmd == EnumRemoteCommand.PSTART)
                {
                }
                else if (Rcmd == EnumRemoteCommand.PW_REQUEST)
                {

                }
                else if (Rcmd == EnumRemoteCommand.RESTART)
                {

                }
                else if (Rcmd == EnumRemoteCommand.RESUME)
                {
                }
                else if (Rcmd == EnumRemoteCommand.SCAN_CASSETTE)
                {

                }
                else if (Rcmd == EnumRemoteCommand.SIGNAL_TOWER)
                {

                }
                else if (Rcmd == EnumRemoteCommand.START)
                {

                }
                else if (Rcmd == EnumRemoteCommand.STOP)
                {

                }
                else if (Rcmd == EnumRemoteCommand.UNDOCK)
                {

                }
                else if (Rcmd == EnumRemoteCommand.WFCLN)
                {

                }
                else if (Rcmd == EnumRemoteCommand.WFIDCONFPROC)
                {

                }
                else if (Rcmd == EnumRemoteCommand.ZIF_REQUEST)
                {

                }
                else if (Rcmd == EnumRemoteCommand.DOWNLOAD_STAGE_RECIPE)
                {

                }
                LoggerManager.Debug($"[GEM COMMANDER] OnRemoteCommandAction2 : {Rcmd}");
                //rspremotecommand는 GEMRspRemoteCommand2 함수만 사용한다.(그냥 ..Command는 사용안한다.)
                SecsGemServiceProxy.GEMRspRemoteCommand2(nMsgId, Rcmd.ToString(), retHCAck, CPName?.Length ?? 0, CPName, CPVal); //5번째 6번째는 각각의
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region <remarks> void OnGEMReqPPList(long nMsgId) </remarks>
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        ///     GEM -> Main.
        ///     First Create : 2019-05-20, Semics R&D1, Jake Kim.
        /// </remarks>
        /// <param name="nMsgId">             </param>
        public void OnGEMReqPPList(long nMsgId)
        {
            try
            {
                ///////////////////////test code
                //C:\Program Files (x86)\Linkgenesis\XGem v3.x\SE\Manual\
                string dirPath = this.FileManager().GetDeviceRootPath();
                string[] ppList = null;

                if (System.IO.Directory.Exists(dirPath))
                {
                    System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(dirPath);
                    ppList = di.GetDirectories().Select(directory => directory.Name)?.ToArray();
                }
                SecsGemServiceProxy.GEMRspPPList(nMsgId, ppList.Length, ppList.ToArray());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region <remarks> void OnGEMECVChanged(long nCount, long[] pnEcids, string[] psVals) </remarks>
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        ///     GEM -> Main.
        ///     First Create : 2019-05-20, Semics R&D1, Jake Kim.
        /// </remarks>
        /// <param name="nCount">             </param>
        /// <param name="pnEcids">             </param>
        /// <param name="psVals">             </param>
        public void OnGEMECVChanged(long nCount, long[] pnEcids, string[] psVals)
        {
        }
        #endregion

        #region <remarks> void OnGEMECVChangedOnGEMReqChangeECV(long nMsgId, long nCount, long[] pnEcids, string[] psVals) </remarks>
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        ///     GEM -> Main.
        ///     First Create : 2019-05-20, Semics R&D1, Jake Kim.
        /// </remarks>
        /// <param name="nMsgId">             </param>
        /// <param name="nCount">             </param>
        /// <param name="pnEcids">             </param>
        /// <param name="psVals">             </param>
        public void OnGEMReqChangeECV(long nMsgId, long nCount, long[] pnEcids, string[] psVals)
        {
            string errorlog = "";
            string getlog = "";
            //EventCodeEnum retVal = EventCodeEnum.UNDEFINED;            
            try
            {
                ILoaderCommunicationManager loadercomm = this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
                ILoaderSupervisor loadermaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                LoggerManager.Debug($"[GEM COMMANDER] OnGEMReqChangeECV pnEcids.Count:{pnEcids.Length}");
            

                //1.모든 ECID에 대해서 Validate를 시행함.
                //하나라도 NONE이 아니면 Reject로 Response함.
                var retVal = ValidateAllECID(pnEcids, psVals, out getlog);
                if(retVal != EventCodeEnum.NONE)
                {
                    errorlog = getlog;                    
                }
                else
                {
                    //2. 하나하나 Pre-Behavior(Validate(By Source, By Parameter) ) 상위item의 Post-Behavior에의해 상태가 바뀌었을 수도있음.
                    //           -> Value Update
                    //           -> Post-Behavior(By Source, Update to other module, DVID, State Clear) 를 실행함. 

                    for (int i = 0; i < pnEcids.Count(); i++)
                    {
                        try
                        {
                            errorlog = "";//clear

                            var ecid = pnEcids[i];
                            var setvalue = psVals[i];

                            LoggerManager.Debug($"[GEM COMMANDER] OnGEMReqChangeECV : Try Update ({i+1}/{nCount}) ECID:{pnEcids[i]}, ECV:{psVals[i]}");
                            var gemvidInfo = this.GEMModule().FindVidInfo(this.GEMModule().DicECID.DicProberGemID.Value, ecid);//로더쪽 파라미터 


                            VidUpdateTypeEnum vidOwner = VidUpdateTypeEnum.BOTH;
                            int stgNum = -1;
                            (vidOwner, stgNum) = (CommandHostService as XGemCommanderHost).GetVidOwner(ecid);

                            if (vidOwner != VidUpdateTypeEnum.CELL && vidOwner != VidUpdateTypeEnum.COMMANDER)
                            {
                                retVal = EventCodeEnum.PARAM_INSUFFICIENT;// 파라미터를 찾지 못한것으로 판단.
                                break;
                            }
                            else if (vidOwner == VidUpdateTypeEnum.CELL && (stgNum < 0 || stgNum > SystemModuleCount.ModuleCnt.StageCount))
                            {
                                retVal = EventCodeEnum.PARAM_SET_INVALID_OUT_OF_RANGE;// 파라미터를 찾지 못한것으로 판단.
                                break;
                            }
                            else
                            {

                                if (vidOwner == VidUpdateTypeEnum.CELL)
                                {
                                    ILoaderServiceCallback cell = loadermaster.GetClient(stgNum);
                                    IParamManagerProxy client = loadercomm.GetProxy<IParamManagerProxy>(stgNum);// 연결상태는 ValidateAllECID()에서 확인했음.
                                    if (cell == null || client == null)
                                    {
                                        retVal = EventCodeEnum.STAGE_DISCONNECTED;
                                        errorlog = $"Stage{stgNum} is not connected.";
                                        break;
                                    }
                                    else
                                    {
                                        if (cell.GetStageInfo().CellMode != GPCellModeEnum.ONLINE)
                                        {
                                            retVal = EventCodeEnum.INVALID_ACCESS;
                                            errorlog = $"Stage{stgNum} is not online. cur:{cell.GetStageInfo().CellMode}";
                                            break;
                                        }
                                        else
                                        {
                                            var findkey = client.GetPropertyPathFromVID(ecid);
                                            if (findkey == "" || findkey == null)
                                            {
                                                retVal = EventCodeEnum.PARAM_INSUFFICIENT;
                                                errorlog = $"validate failed. target stgnum:{stgNum}, vid:{ecid}, propertypath:{findkey}.";
                                                break;
                                            }
                                            else
                                            {
                                                retVal = client.SetOriginValue(findkey, setvalue, isNeedValidation: true);
                                                if (retVal != EventCodeEnum.NONE)
                                                {
                                                    break;
                                                }
                                            }                                            
                                        }
                                    }
                                }
                                else//COMMANDER
                                {                                    
                                    retVal = this.ParamManager().SetOriginValue(gemvidInfo.key, setvalue, isNeedValidation: true);//, source_classname: this.GEMModule().GetType().FullName);// 타겟이 로더 또는 전역인 경우                                
                                    if (retVal != EventCodeEnum.NONE)
                                    {
                                        break;
                                    }
                                }

                            }

                            if (retVal != EventCodeEnum.NONE)
                            {
                                errorlog += errorlog;
                                LoggerManager.Debug($"[GEM COMMANDER] OnGEMReqChangeECV : Update Fail, Out Loop ({i}/{nCount}) ECID:{pnEcids[i]}, ECV:{psVals[i]}");
                                break;// 다음 ecv setvalue를 진행하면 안된다.
                            }

                        }
                        catch (Exception err)
                        {
                            retVal = EventCodeEnum.PARAM_SET_INVALID_OUT_OF_RANGE;
                            LoggerManager.Exception(err);
                            break;
                        }
                    }
                }

                if (retVal != EventCodeEnum.NONE)
                {                    
                    LoggerManager.Debug($"[GEM COMMANDER] OnGEMReqChangeECV : result:{retVal} details: {errorlog}");                    
                }

                long ack = -1;
                EnumGEM_EAC eac = ConvertResultToEAC(retVal, out ack);
                SecsGemServiceProxy.GEMRspChangeECV(nMsgId, ack);  // S2F16
              
                //{
                //    SecsGemServiceProxy.GEMSetECVChanged(nCount, pnEcids, psVals);  // CEID 2010, all Ecv changed Success <- SetVariable을하면 Gem Dongle에서 나감.
                //}
               
                LoggerManager.Debug($"[GEM COMMANDER] OnGEMReqChangeECV : End retVal:{retVal}, Replied {ack}.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        #endregion

        private EnumGEM_EAC ConvertResultToEAC(EventCodeEnum eventCodeEnum, out long ack)
        {
            EnumGEM_EAC ret = EnumGEM_EAC.UNDEFINE;
            long retack = -1;
            try
            {
                if (eventCodeEnum == EventCodeEnum.NONE)
                {
                    ret = EnumGEM_EAC.OK;
                    retack = 0x00;
                }
                else if (eventCodeEnum == EventCodeEnum.PARAM_INSUFFICIENT ||
                   eventCodeEnum == EventCodeEnum.PARAM_ERROR)
                {
                    ret = EnumGEM_EAC.ONE_OR_MORE_CONSTANT_IS_NOT_EXIST;
                    retack = 0x01;
                }
                else if (eventCodeEnum == EventCodeEnum.STAGE_DISCONNECTED ||
                    eventCodeEnum == EventCodeEnum.INVALID_ACCESS)
                {
                    ret = EnumGEM_EAC.BUSY;
                    retack = 0x02;
                }               
                else if (eventCodeEnum == EventCodeEnum.PARAM_SET_INVALID_OUT_OF_RANGE)
                {
                    ret = EnumGEM_EAC.ONE_OR_MORE_OUT_OF_RANGE;
                    retack = 0x03;
                }
                else// default
                {
                    ret = EnumGEM_EAC.ONE_OR_MORE_OUT_OF_RANGE;
                    retack = 0x03;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                ack = retack;
            }

            return ret;
        }

       

        private EventCodeEnum ValidateAllECID(long[] pnEcids, string[] psVals, out string errorlog)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            string errormsg = "";
            try
            {
                ILoaderCommunicationManager loadercomm = this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();

                long errorEcid = -1;
                int errorIndex = -1;

                for (int i = 0; i < pnEcids.Count(); i++)//ECV를 여러개 설정할 수 있음.
                {
                    var ecid = pnEcids[i];
                    var setvalue = psVals[i];
                    errorEcid = ecid;
                    errorIndex = i + 1;

                    VidUpdateTypeEnum vidOwner = VidUpdateTypeEnum.BOTH;
                    int stgNum = -1;
                    (vidOwner, stgNum) = (CommandHostService as XGemCommanderHost).GetVidOwner(ecid);

                    if (vidOwner != VidUpdateTypeEnum.CELL && vidOwner != VidUpdateTypeEnum.COMMANDER)
                    {
                        errormsg = $"[{errorIndex}/{pnEcids.Count()}] ecid({ecid}) vidOwner is not invalid.";
                        retVal = EventCodeEnum.PARAM_INSUFFICIENT;// 파라미터를 찾지 못한것으로 판단.
                        break;
                    }
                    else if (vidOwner == VidUpdateTypeEnum.CELL && (stgNum < 0 || stgNum > SystemModuleCount.ModuleCnt.StageCount))
                    {
                        errormsg = $"[{errorIndex}/{pnEcids.Count()}] stageNum is invalid. stageNum:{stgNum}";
                        retVal = EventCodeEnum.PARAM_SET_INVALID_OUT_OF_RANGE;// 파라미터를 찾지 못한것으로 판단.
                        break;
                    }
                    else
                    {
                        var gemvidInfo = this.GEMModule().FindVidInfo(this.GEMModule().DicECID.DicProberGemID.Value, ecid);
                        if (vidOwner == VidUpdateTypeEnum.CELL)
                        {
                            var client = loadercomm.GetProxy<IParamManagerProxy>(index: stgNum);
                            if (client != null)
                            {
                                //gemvidInfo//로더쪽 파라미터 //v22_merge TODO: 이부분 GetVidOwner()에서 VIDInfomation으로 대체 하도록 변경
                                //retVal = client.CheckSetValueAvailable(gemvidInfo.key, setvalue, out errormsg, this.GEMModule().GetType().FullName);
                                var findkey = client.GetPropertyPathFromVID(ecid);
                                if (findkey == "" || findkey == null)
                                {
                                    retVal = EventCodeEnum.PARAM_INSUFFICIENT;
                                    errormsg = $"[{errorIndex}/{pnEcids.Count()}] ecid({ecid}) validate failed. target stgnum:{stgNum}, vid:{ecid}, propertypath:{findkey}.";
                                    break;
                                }
                                else
                                {
                                    retVal = client.CheckOriginSetValueAvailable(findkey, setvalue);//, this.GEMModule().GetType().FullName);
                                    if (retVal != EventCodeEnum.NONE)
                                    {
                                        retVal = EventCodeEnum.INVALID_ACCESS;
                                        errormsg = $"[{errorIndex}/{pnEcids.Count()}] ecid({ecid}) validate failed. target stgnum:{stgNum}, vid:{ecid}, setvalue:{setvalue}.";
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                retVal = EventCodeEnum.STAGE_DISCONNECTED;
                                errormsg = $"[{errorIndex}/{pnEcids.Count()}] ecid({ecid}) target stg is disconnected. stgnum:{stgNum}.";
                                break;
                            }
                        }
                        else//COMMANDER
                        {
                            IElement element = this.ParamManager().GetElement(gemvidInfo.key);
                            if (element != null)
                            {
                                retVal =  this.ParamManager().CheckOriginSetValueAvailable(element.PropertyPath, setvalue);//, source_classname: this.GEMModule().GetType().FullName);// 타겟이 로더 또는 전역인 경우                                
                                if(retVal != EventCodeEnum.NONE)
                                {
                                    errormsg = $"[{errorIndex}/{pnEcids.Count()}] ecid({ecid}) check origin validation is failed. origin:{element.OriginPropertyPath}";
                                    break;
                                }
                            }
                            else
                            {
                                retVal = EventCodeEnum.PARAM_INSUFFICIENT;
                                errormsg = $"[{errorIndex}/{pnEcids.Count()}] ecid({ecid}) ECID Gem info cannot find in  parameter. vid:{ecid}.";
                                break;
                            }
                           
                        }
                    }


                }//ecids for loop

                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                errorlog = errormsg;
            }
            return retVal;

        }


        #region <remarks> void OnGEMErrorEvent(string sErrorName, long nErrorCode) </remarks>
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        ///     GEM -> Main.
        ///     First Create : 2019-05-20, Semics R&D1, Jake Kim.
        /// </remarks>
        /// <param name="sErrorName">             </param>
        /// <param name="nErrorCode">             </param>
        public void OnGEMErrorEvent(string sErrorName, long nErrorCode)
        {
        }
        #endregion

        #region <remarks> void OnGEMReqPPDelete(long nMsgId, long nCount, string[] psPpid) </remarks>
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        ///     GEM -> Main.
        ///     First Create : 2019-05-20, Semics R&D1, Jake Kim.
        /// </remarks>
        /// <param name="nMsgId">             </param>
        /// <param name="nCount">             </param>
        /// <param name="psPpid">             </param>
        public void OnGEMReqPPDelete(long nMsgId, long nCount, string[] psPpid)
        {
        }
        #endregion

        #region <remarks> void OnGEMRspAllECInfo(long lCount, long[] plVid, string[] psName, string[] psValue, string[] psDefault, string[] psMin, string[] psMax, string[] psUnit) </remarks>
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        ///     GEM -> Main.
        ///     First Create : 2019-05-20, Semics R&D1, Jake Kim.
        /// </remarks>
        /// <param name="lCount">             </param>
        /// <param name="plVid">             </param>
        /// <param name="psName">             </param>
        /// <param name="psValue">             </param>
        /// <param name="psDefault">             </param>
        /// <param name="psMin">             </param>
        /// <param name="psMax">             </param>
        /// <param name="psUnit">             </param>
        public void OnGEMRspAllECInfo(long lCount, long[] plVid, string[] psName, string[] psValue, string[] psDefault, string[] psMin, string[] psMax, string[] psUnit)
        {
            for (int i = 0; i < lCount; i++)
            {
                if (string.Compare(psName[i], "DeviceID", true) == 0)
                {
                    this.GemCommManager.SecsCommInformData.HSMSDeviceID = psValue[i];
                }
                else if (string.Compare(psName[i], "IPAddress", true) == 0)
                {
                    this.GemCommManager.SecsCommInformData.HSMSIP = psValue[i];
                }
                else if (string.Compare(psName[i], "PortNumber", true) == 0)
                {
                    this.GemCommManager.SecsCommInformData.HSMSPort = psValue[i];
                }
                else if (string.Compare(psName[i], "ActiveMode", true) == 0)
                {
                    if (psValue[i] != null)
                    {
                        if (psValue[i].Equals("0", StringComparison.CurrentCultureIgnoreCase))
                        {
                            this.GemCommManager.SecsCommInformData.HSMSPassive = SecsEnum_Passive.PASSIVE;
                        }
                        else
                        {
                            this.GemCommManager.SecsCommInformData.HSMSPassive = SecsEnum_Passive.ACTIVE;
                        }
                    }
                }
                else if (string.Compare(psName[i], "LinkTestInterval", true) == 0)
                {
                    this.GemCommManager.SecsCommInformData.HSMSLinkTestInterval = psValue[i];
                }
                else if (string.Compare(psName[i], "RetryLimit", true) == 0)
                {
                    this.GemCommManager.SecsCommInformData.HSMSRetryLimit = psValue[i];
                }
                else if (string.Compare(psName[i], "T3Timeout", true) == 0)
                {
                    this.GemCommManager.SecsCommInformData.T3 = psValue[i];
                }
                else if (string.Compare(psName[i], "T5Timeout", true) == 0)
                {
                    this.GemCommManager.SecsCommInformData.T5 = psValue[i];
                }
                else if (string.Compare(psName[i], "T6Timeout", true) == 0)
                {
                    this.GemCommManager.SecsCommInformData.T6 = psValue[i];
                }
                else if (string.Compare(psName[i], "T7Timeout", true) == 0)
                {
                    this.GemCommManager.SecsCommInformData.T7 = psValue[i];
                }
                else if (string.Compare(psName[i], "T8Timeout", true) == 0)
                {
                    this.GemCommManager.SecsCommInformData.T8 = psValue[i];
                }
                else if (string.Compare(psName[i], "InitControlState", true) == 0) //3514 //Offline, Online
                {
                    int? tmpInt = StringToInt(psValue[i]);
                    if (tmpInt != null)
                    {
                        this.GemCommManager.SecsCommInformData.InitControlState = (SecsEnum_ON_OFFLINEState)tmpInt;
                    }
                }
                else if (string.Compare(psName[i], "OffLineSubState", true) == 0)
                {
                    int? tmpInt = StringToInt(psValue[i]);
                    if (tmpInt != null)
                    {
                        this.GemCommManager.SecsCommInformData.OffLineSubState = (SecsEnum_OfflineSubState)tmpInt;
                    }
                }
                else if (string.Compare(psName[i], "OnLineSubState", true) == 0) //1 => Local, 2 => Remote
                {
                    int? tmpInt = StringToInt(psValue[i]);
                    if (tmpInt != null)
                    {
                        this.GemCommManager.SecsCommInformData.OnLineSubState = (SecsEnum_OnlineSubState)tmpInt;
                    }
                }
                else if (string.Compare(psName[i], "Equipment_Initiated_Connected", true) == 0) // 101 //Host = 0, EQ = 1 //
                {
                    int? tmpInt = StringToInt(psValue[i]);
                    if (tmpInt != null)
                    {
                        this.GemCommManager.SecsCommInformData.Equipment_Initiated_Connected = (SecsEnum_EstablishSource)tmpInt;
                    }
                }
                else if (string.Compare(psName[i], "DefaultCommState", true) == 0) //130 //Disable = 0, Able = 1 //
                {
                    int? tmpInt = StringToInt(psValue[i]);
                    if (tmpInt != null)
                    {
                        this.GemCommManager.SecsCommInformData.DefaultCommState = (SecsEnum_Enable)(tmpInt);
                    }
                }
            }
        }
        #endregion

        #region [STM_CATANIA] SendAck - Error Check Functions
        (ushort ack, string msgtxt) CheckFoupNumberError(int ptn)
        {
            (ushort ack, string msgtxt) ret = (0, string.Empty);
            var foupcnt = SystemModuleCount.ModuleCnt.FoupCount;
            var ptnVal = ptn;
            if (ptnVal < 1 || ptnVal > foupcnt)
            {
                ret.ack = 10;
                ret.msgtxt = "PTNERROR";
            }
            else
            {
                var lotinfos = this.GetLoaderContainer().Resolve<LoaderBase.ILoaderSupervisor>().ActiveLotInfos;
                lotinfos[ptn - 1].RCMDErrorCheckDic.TryGetValue("PTN", out string prevPtn);

                if (!ptnVal.ToString().Equals(prevPtn))
                {
                    ret.ack = 10;
                    ret.msgtxt = "PTNERROR";
                }
            }

            return ret;
        }

        (ushort ack, string msgtxt) CheckLotIdError(EnumCarrierAction carrierActType, string lotidVal, int ptnVal)
        {
            (ushort ack, string msgtxt) ret = (0, string.Empty);
            var lotid = string.Empty;
            var lotinfos = this.GetLoaderContainer().Resolve<LoaderBase.ILoaderSupervisor>().ActiveLotInfos;

            if (carrierActType.Equals(EnumCarrierAction.PROCEEDWITHCARRIER) 
                || carrierActType.Equals(EnumCarrierAction.PROCESSEDWITHCELLSLOT)
                || carrierActType.Equals(EnumCarrierAction.CANCELCARRIER))
            {
                lotinfos[ptnVal - 1].RCMDErrorCheckDic.TryGetValue("LOTID", out string prevLotid);
                lotid = prevLotid;
            }
            else
            {
                if (string.IsNullOrEmpty(lotid))
                {
                    ret.ack = 10; // validation error
                    ret.msgtxt = $"LOTIDEMPTY";
                }
            }

            if (!string.IsNullOrEmpty(lotid) && !lotid.Equals(lotidVal))
            {
                ret.ack = 10; // validation error
                ret.msgtxt = $"LOTIDERROR_{lotid}";
            }

            return ret;
        }

        (ushort ack, string msgtxt) CheckCellInfoError(string cellmap, int ptnVal)
        {
            (ushort ack, string msgtxt) ret = (0, string.Empty);

            var cellcnt = SystemModuleCount.ModuleCnt.StageCount;            
            if (cellmap.Length == cellcnt)
            {
                for (int i = 0; i < cellmap.Length; i++)
                {
                    if (!(cellmap[i] == '1'|| cellmap[i] == '0'))
                    {
                        ret.ack = 10; // validation error
                        ret.msgtxt = "CELLINFOERROR";
                        break;
                    }
                    if (cellmap[i] == '1')
                    {
                        if (!GetConnectState(Convert.ToInt32(i + 1)))
                        {
                            ret.ack = 10; // validation error
                            ret.msgtxt = "CELLNOTCONNECTED";
                            break;

                        }
                    }
                }
            }
            else
            {
                ret.ack = 10; // validation error
                ret.msgtxt = "CELLINFOERROR";
            }

            if (ret.ack == 0)
            {
                //[STM_CATANIA] 받은 Cell Info가 정상이라면 이전 gem 커맨드(STM시나리오상으로는 ONLINE_PPSELECT)에 받은 Cell 정보와 일치하는지 확인한다. 
                var lotinfos = this.GetLoaderContainer().Resolve<LoaderBase.ILoaderSupervisor>().ActiveLotInfos;
                lotinfos[ptnVal - 1].RCMDErrorCheckDic.TryGetValue("CELLINFO", out string prevCellMap);

                if (!prevCellMap.Equals(cellmap))
                {
                    ret.ack = 10; // validation error
                    ret.msgtxt = "CELLINFOERROR";
                }
            }

            return ret;
        }

        (ushort ack, string msgtxt) CheckSlotInfoError(string slotmap, int ptnVal)
        {
            (ushort ack, string msgtxt) ret = (0, string.Empty);

            var foups = this.GetLoaderContainer().Resolve<LoaderBase.ILoaderModule>().Foups;
            var slotcnt = SystemModuleCount.ModuleCnt.SlotCount;
            var allowedSlotChars = new List<char>() { '0', '1', 't', 'T', 'f', 'F' };
            if (slotmap.Length == slotcnt)
            {
                for (int i = 0; i < slotmap.Length; i++)
                {
                    var slotChar = slotmap[i];

                    if (!allowedSlotChars.Contains(slotChar))
                    {
                        ret.ack = 10; // validation error
                        ret.msgtxt = "SLOTINFOERROR";
                        break;
                    }
                    if (slotChar == '1' || slotChar == 't' || slotChar == 'T')
                    {
                        if ((int)foups[ptnVal - 1].Slots[slotcnt - 1 - i].WaferState != 1) // 1 : UNPROCESSED
                        {
                            ret.ack = 10; // validation error
                            ret.msgtxt = "SLOTINFOERROR";
                            break;
                        }
                    }
                }
            }
            else
            {
                ret.ack = 10; // validation error
                ret.msgtxt = "SLOTINFOERROR";
            }

            return ret;
        }
        #endregion

        private int? StringToInt(string str)
        {
            bool parseResult = false;
            int? retInt = null;
            int tmpInt = 0;
            parseResult = int.TryParse(str, out tmpInt);

            if (parseResult == true)
            {
                retInt = (int)tmpInt;
            }
            else
            {
                retInt = null;
            }

            return retInt;
        }

        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);

        #region <remarks> void OnGEMReqPPSendEx(long nMsgId, string sPpid, string sRecipePath) </remarks>
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        ///     GEM -> Main.
        ///     First Create : 2019-05-20, Semics R&D1, Jake Kim.
        /// </remarks>
        /// <param name="nMsgId">             </param>
        /// <param name="sPpid">             </param>
        /// <param name="sRecipePath">             </param>
        public void OnGEMReqPPSendEx(long nMsgId, string sPpid, string sRecipePath)
        {
            try
            {
                var nResult = 0;

                if (string.IsNullOrEmpty(sPpid))
                {
                    //PPID Not found
                    nResult = 4;
                }
                else
                {
                    // 1.Project Path를 가져온다.(PPBODY 정보는 ProjectPath + \XWork\Recipe 폴더에 sPpid name으로 저장되기 때문)
                    var configFile = @"C:\ProberSystem\Utility\GEM\Config\GEM_PROCESS.cfg";
                    if (!File.Exists(configFile))
                    {
                        // other error
                        nResult = 7;
                    }

                    StringBuilder projectPath = new StringBuilder(64);
                    GetPrivateProfileString("XGEM", "ProjectPath", "", projectPath, 64, configFile);

                    // 2.파일이 제대로 저장되었는지 확인한다.
                    var xtrfPath = Path.GetDirectoryName(projectPath.ToString()) + @"\XWork\Recipe";
                    var Ppid = sPpid.Split('/');
                    var xtrfFileName = Ppid.Last().Trim();
                    var xtrfFile = Path.Combine(xtrfPath, xtrfFileName);

                    if (!File.Exists(xtrfFile))
                    {
                        // other error (파일 생성이 안됨)
                        nResult = 7;
                    }

                    // syntax error checking
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.Load(xtrfFile);
                }

                //S7F3Ack => S7F4
                SecsGemServiceProxy.GEMRspPPSendEx(nMsgId, sPpid, sRecipePath, nResult);
            }
            catch (Exception err)
            {
                // 오류로 빠지는 경우 other error(nResult = 7)로 응답한다.
                SecsGemServiceProxy.GEMRspPPSendEx(nMsgId, sPpid, sRecipePath, 7);
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region <remarks> void OnGEMReqPPEx(long nMsgId, string sPpid, string sRecipePath) </remarks>
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        ///     GEM -> Main.
        ///     First Create : 2019-05-20, Semics R&D1, Jake Kim.
        /// </remarks>
        /// <param name="nMsgId">             </param>
        /// <param name="sPpid">             </param>
        /// <param name="sRecipePath">             </param>
        public void OnGEMReqPPEx(long nMsgId, string sPpid, string sRecipePath)
        {
        }
        #endregion


        #region <remarks> GEM Module Methods & Command              </remarks>

        #region <remarks> Initialize Module </remarks>
        public bool
            InitSecsGem(String ConfigPath = @"C:\ProberSystem\Parameters\SystemParam\GEM\GemConfig.cfg")
        {
            bool retVal = false;

            try
            {
                if (System.IO.File.Exists(ConfigPath))
                {
                    retVal = SecsGemServiceProxy.Init_SECSGEM(ConfigPath) == 0 ? true : false;
                }
                else
                {
                    retVal = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        #endregion

        #region <remarks> Close the Gem Service Host (WCF) </remarks>
        public void CloseGemServiceHost()
        {
            try
            {
                SecsGemServiceProxy?.Close_SECSGEM();
                SecsGemServiceProxy = null;
                LoggerManager.Debug("[SECS/GEM] Close SecsGem Service Host");

                SecsGemClientCallback = null;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        public string GetCommInfo(params string[] setCommParamName)
        {
            string retVal = string.Empty;
            StringBuilder sbCommInfo = new StringBuilder();
            long? getRetVal = -1;

            try
            {
                if (setCommParamName != null)
                {
                    for (int i = 0; i < setCommParamName.Length; i++)
                    {
                        sbCommInfo.Append(setCommParamName[i]);

                        if (i != (setCommParamName.Length - 1))
                            sbCommInfo.Append(",");
                    }

                    getRetVal = SecsGemServiceProxy?.GEMGetParam(sbCommInfo.ToString(), ref retVal);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public void SetCommInfo(string commDataValue, params string[] getCommDataName)
        {
            string[] splitCommDataVal = null;

            try
            {
                splitCommDataVal = commDataValue.Split(',');
                if ((commDataValue != null && getCommDataName != null) &&
                    (splitCommDataVal.Length == getCommDataName.Length))
                {
                    for (int i = 0; i < getCommDataName.Length; i++)
                    {
                        string tmpStr = splitCommDataVal[i];

                        if (!tmpStr.Contains("ErrCode:") && (tmpStr != null))
                        {
                            if (getCommDataName[i] == GemGlobalData.IP)
                            {
                                this.GemCommManager.SecsCommInformData.HSMSIP = tmpStr;
                            }
                            else if (getCommDataName[i] == GemGlobalData.Port)
                            {
                                this.GemCommManager.SecsCommInformData.HSMSPort = tmpStr;
                            }
                            else if (getCommDataName[i] == GemGlobalData.Active)
                            {
                                this.GemCommManager.SecsCommInformData.HSMSPassive = (tmpStr?.Equals("true", StringComparison.CurrentCultureIgnoreCase) ?? false)
                                                                ? SecsEnum_Passive.ACTIVE : SecsEnum_Passive.PASSIVE;
                            }
                            else if (getCommDataName[i] == GemGlobalData.DeviceID)
                            {
                                this.GemCommManager.SecsCommInformData.HSMSDeviceID = tmpStr;
                            }
                            else if (getCommDataName[i] == GemGlobalData.LinkTestInterval)
                            {
                                this.GemCommManager.SecsCommInformData.HSMSLinkTestInterval = tmpStr;
                            }
                            else if (getCommDataName[i] == GemGlobalData.RetryLimit)
                            {
                                this.GemCommManager.SecsCommInformData.HSMSRetryLimit = tmpStr;
                            }
                            else if (getCommDataName[i] == GemGlobalData.T3)
                            {
                                this.GemCommManager.SecsCommInformData.T3 = tmpStr;
                            }
                            else if (getCommDataName[i] == GemGlobalData.T5)
                            {
                                this.GemCommManager.SecsCommInformData.T5 = tmpStr;
                            }
                            else if (getCommDataName[i] == GemGlobalData.T6)
                            {
                                this.GemCommManager.SecsCommInformData.T6 = tmpStr;
                            }
                            else if (getCommDataName[i] == GemGlobalData.T7)
                            {
                                this.GemCommManager.SecsCommInformData.T7 = tmpStr;
                            }
                            else if (getCommDataName[i] == GemGlobalData.T8)
                            {
                                this.GemCommManager.SecsCommInformData.T8 = tmpStr;
                            }
                        }
                        else
                        {
                            //여기서 ErrCode는 GEM 동글에서 데이터를 가져올때의 반환값이다.
                            LoggerManager.Debug($"[{this.GetType().Name}] SetCommInfo() : Setting wrong data to {getCommDataName[i]}. ErrCode : {tmpStr}");
                        }
                    }
                }
                else
                {
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ParamSettingApply()
        {
            try
            {
                Proc_SetParamInfoToGem();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public long CommEnable()
        {
            long retVal = -1;
            try
            {

                retVal = SecsGemServiceProxy?.GEMSetEstablish((long)SecsEnum_Enable.ENABLE) ?? -1;

                if (retVal == 0)
                {
                    //success
                    this.GemCommManager.SecsCommInformData.Enable = SecsEnum_Enable.ENABLE;
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

        public void Dispose()
        {
            try
            {
                if (this.IsDisposed == false)
                {
                    this.IsDisposed = true;
                    this.Initialized = false;

                    this.CloseGemServiceHost();  //1. Host Close
                    this.GemCommManager.ProcessClose();       //2. Process Close
                    this.SecsGemClientCallback = null;

                    this.GemCommManager.SecsCommInformData.CommunicationState = SecsEnum_CommunicationState.UNDEFIND;
                    this.GemCommManager.SecsCommInformData.ControlState = SecsEnum_ControlState.UNDEFIND;
                    this.GemCommManager.SecsCommInformData.DefaultCommState = SecsEnum_Enable.UNKNOWN;
                    this.GemCommManager.SecsCommInformData.InitControlState = SecsEnum_ON_OFFLINEState.OFFLINE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        ///////////////////////////////////////Function

        public void DeInitModule()
        {
            Dispose();
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    IsDisposed = false;
                    retval = ConnectService();

                    lock (backupInfoLock)
                    {
                        LoadGemAlarmBackupFile();

                        if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                        {
                            if (GemAlarmBackupInfo.GemAlarmStateInfos.Count() != SystemModuleCount.ModuleCnt.StageCount + 1)
                            {
                                GemAlarmBackupInfo.Initialize();
                                SaveGemAlarmBackupFile();
                            }
                            else
                            {
                                // use loaded param.
                            }
                        }
                        else
                        {
                            if (GemAlarmBackupInfo.GemAlarmStateInfos.Count() == 0)
                            {
                                GemAlarmBackupInfo.Initialize();
                                SaveGemAlarmBackupFile();
                            }
                            else
                            {
                                // use loaded param.
                            }
                        }
                    }
                    
                    Initialized = true;
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

        public EventCodeEnum ConnectService()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = StartGemService(); // GEM Service에 연결합니다.

                if (CommandHostService == null)
                {
                    CommandHostService = new XGemCommanderHost() { Commander = this };
                }
                if (CommandHostService != null)
                {
                    if (CommandHostService.StartCommanderService() == true) // Loader -> Cell 쪽 Service Host를 Open합니다.
                    {
                        retVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        retVal = EventCodeEnum.UNDEFINED;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum DisConnectService()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //CommandHostService.CloseCommanderService();                
                this.CloseGemServiceHost();  //1. Host Close
                this.GemCommManager.ProcessClose();       //2. Process Close
                this.GEMModule().CommunicationState = EnumCommunicationState.DISCONNECT;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void GEMDisconnectCallBack(long lStageID)
        {
            if (disconnectDelegate != null)
            {
                disconnectDelegate(lStageID);
            }
        }

        public void SetGEMDisconnectCallBack(GEMDisconnectDelegate callback)
        {
            disconnectDelegate = callback;
        }

        public EventCodeEnum InitGemData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //Init Foup State & Card Tray Probe Card ID
                //ILoaderPIV loaderPIV = LoaderMaster?.GetLoaderPIV();
                //this.GEMModule().GetPIVContainer().SetFoupState(1, GEMFoupStateEnum.ONLINE);
                //this.GEMModule().GetPIVContainer().SetFoupState(2, GEMFoupStateEnum.ONLINE);
                //this.GEMModule().GetPIVContainer().SetFoupState(3, GEMFoupStateEnum.ONLINE);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        //#Hynix_Merge: 검토 필요, Hynix에 원래 이코드가 있었는데...
        //private void LoadCEIDParam()
        //{
        //    try
        //    {
        //        IParam tmpParam = null;
        //        tmpParam = new GEMCEIDDictionaryParam();
        //        LoggerManager.Debug($"[{this.GetType().Name}] Start Load to DVID Param.");
        //        var RetVal = this.LoadParameter(ref tmpParam, typeof(GEMCEIDDictionaryParam), "DVID");

        //        if (RetVal == EventCodeEnum.NONE)
        //        {
        //            LoggerManager.Debug($"[{this.GetType().Name}] Finish loading DVID Param.");
        //            GEMCEIDDictionaryParam = tmpParam as GEMCEIDDictionaryParam;
        //        }
        //        else
        //        {
        //            LoggerManager.Debug($"[{this.GetType().Name}] Fail loading DVID Param.");
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}
        //==================

        private EventCodeEnum StartGemService()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                retval = GemCommManager?.StartWcfGemService() ?? EventCodeEnum.UNDEFINED;
            }
            catch (Exception err)
            {
                throw err;
            }

            return retval;
        }

        public long Proc_SetEstablish(long bState)
        {
            return SecsGemServiceProxy?.GEMSetEstablish(bState) ?? -1;
        }

        public long Proc_ReqOffline()
        {
            return SecsGemServiceProxy?.GEMReqOffline() ?? -1;
        }

        public long Proc_ReqLocal()
        {
            return SecsGemServiceProxy?.GEMReqLocal() ?? -1;
        }

        public long Proc_ReqRemote()
        {
            return SecsGemServiceProxy?.GEMReqRemote() ?? -1;
        }

        public long Start()
        {
            return SecsGemServiceProxy?.Start() ?? -1;
        }

        public long Proc_SetVariable(int vidLength, long[] convertDataID, string[] values, bool immediatelyUpdate = false)
        {
            lock (lockObj)
            {
                return SecsGemServiceProxy?.GEMSetVariable(vidLength, convertDataID, values) ?? -1;
            }

        }

        public long Proc_SetVariables(long nObjectID, long nVid, bool immediatelyUpdate = false)
        {
            lock (lockObj)
            {
                return SecsGemServiceProxy?.GEMSetVariables(nObjectID, nVid) ?? -1;
            }

        }

        public long Proc_SetECVChanged(int vidLength, long[] convertDataID, string[] values)
        {
            lock (lockObj)
            {
                return SecsGemServiceProxy?.GEMSetECVChanged(vidLength, convertDataID, values) ?? -1;
            }
        }
      

        public long Proc_SetEvent(long eventNum)
        {
            try
            {
                lock (lockObj)
                {
                    long? retVal = SecsGemServiceProxy?.GEMSetEvent(eventNum);
                    return retVal == null ? -1 : (long)retVal;

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public long Proc_SetAlarm(long nID, long nState, int cellindex = 0)
        {            
            long retVal = -1;
            try
            {
                retVal = (GetSecsGemServiceProxy() as SecsGemServiceDirectProxy)?.GEMSetAlarm(nID, nState) ?? -1;
                ModifyGemAlarmState(nID, nState, cellindex);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }


        public EventCodeEnum Proc_ClearAlarmOnly(int cellIndex = 0)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                var target = GemAlarmBackupInfo.GemAlarmStateInfos.Find(info => info.CellIndex == cellIndex);
                if(target != null)
                {
                    var alids = new List<long>(target.ALIDs);
                    if (alids != null)
                    {
                        foreach (var alid in alids)
                        {
                            Proc_SetAlarm(alid, (long)ProberInterfaces.GEM.GemAlarmState.CLEAR, cellIndex);
                        }
                        ret = EventCodeEnum.NONE;
                    }
                }
              
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }


        public void Proc_SetParamInfoToGem()
        {
            try
            {
                SecsGemServiceProxy.GEMSetParam(GemGlobalData.IP, this.GemCommManager.SecsCommInformData.HSMSIP);
                SecsGemServiceProxy.GEMSetParam(GemGlobalData.Port, this.GemCommManager.SecsCommInformData.HSMSPort);
                SecsGemServiceProxy.GEMSetParam(GemGlobalData.Active, this.GemCommManager.SecsCommInformData.HSMSPassive == SecsEnum_Passive.ACTIVE ? "true" : "false");

                SecsGemServiceProxy.GEMSetParam(GemGlobalData.DeviceID, this.GemCommManager.SecsCommInformData.HSMSDeviceID);
                SecsGemServiceProxy.GEMSetParam(GemGlobalData.LinkTestInterval, this.GemCommManager.SecsCommInformData.HSMSLinkTestInterval);
                SecsGemServiceProxy.GEMSetParam(GemGlobalData.RetryLimit, this.GemCommManager.SecsCommInformData.HSMSRetryLimit);

                SecsGemServiceProxy.GEMSetParam(GemGlobalData.T3, this.GemCommManager.SecsCommInformData.T3);
                SecsGemServiceProxy.GEMSetParam(GemGlobalData.T5, this.GemCommManager.SecsCommInformData.T5);
                SecsGemServiceProxy.GEMSetParam(GemGlobalData.T6, this.GemCommManager.SecsCommInformData.T6);
                SecsGemServiceProxy.GEMSetParam(GemGlobalData.T7, this.GemCommManager.SecsCommInformData.T7);
                SecsGemServiceProxy.GEMSetParam(GemGlobalData.T8, this.GemCommManager.SecsCommInformData.T8);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public long Proc_TimeRequest()
        {
            long funcRetVal = -1;
            try
            {
                funcRetVal = SecsGemServiceProxy?.GEMReqGetDateTime() ?? -1;

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

        public long Proc_SendTerminal(string sendStr)
        {
            long funcRetVal = -1;
            try
            {
                funcRetVal = SecsGemServiceProxy?.GEMSetTerminalMessage(0, sendStr) ?? -1;

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

      

        #region //..CEID 
        public List<long> GetVidsFormCEID(long ceid)
        {
            List<long> vids = null;
            try
            {
                if(GEMCEIDDictionaryParam.CEIDDic.Count != 0)
                {
                    GEMCEIDDictionaryParam.CEIDDic.TryGetValue(ceid, out vids);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return vids;
        }
        #endregion

        public void GEMRspRemoteCommand(long nMsgId, string sCmd, long nHCAck, long nCount, long[] pnResult)
        {
            try
            {
                LoggerManager.Debug($"[GEM COMMANDER] GEMRspRemoteCommand : {sCmd}");
                SecsGemServiceProxy.GEMRspRemoteCommand(nMsgId, sCmd, nHCAck, nCount, pnResult);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        
        #endregion


        #region => Load & Save GemAlarmBackup File
        public EventCodeEnum LoadGemAlarmBackupFile()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Remote)
                {
                    IParam param = null;
                    string FullPath = System.IO.Path.Combine(@"C:\Logs\Backup\GemAlarmInfo.Json");


                    retVal = this.LoadParameter(ref param, typeof(GemAlarmBackupInfo), null, FullPath);
                    if (retVal == EventCodeEnum.NONE)
                    {
                        GemAlarmBackupInfo = (GemAlarmBackupInfo)param;
                        GemAlarmBackupInfo.FileFullPath = FullPath;
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SaveGemAlarmBackupFile()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (GemAlarmBackupInfo != null)
                {
                    Extensions_IParam.SaveParameter(null, GemAlarmBackupInfo, null, GemAlarmBackupInfo.FileFullPath, isNotSave_ChangeLog: true);
                }
                else
                {
                    LoggerManager.Debug("[GEM] SaveGemAlarmBackupFile() error occured. GemAlarmBackupInfo object is null.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        /// <summary>
        /// SetAlarm 이 불릴때마다 GemAlarmBackupInfo에 추가한다.  (로더에서 사용)
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum ModifyGemAlarmState(long alid, long nState, int cellindex = 0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                lock (backupInfoLock)
                {
                    var isExist = GemAlarmBackupInfo.GemAlarmStateInfos.Where(w => w.CellIndex == cellindex).FirstOrDefault();
                    if (isExist != null)
                    {
                        if (nState == (long)GemAlarmState.CLEAR)
                        {
                            if (isExist.ALIDs.Contains(alid) == true)
                            {
                                isExist.ALIDs.Remove(alid);

                                LoggerManager.Debug($"[GemAlarmBackupInfo] Remove ALID({alid}, {isExist.CellIndex}).");

                                retVal = EventCodeEnum.NONE;
                            }
                            else
                            {
                                retVal = EventCodeEnum.INVALID_PARAMETER_FIND;

                                LoggerManager.Debug($"[GemAlarmBackupInfo] ModifyGemAlarmState(state:{nState}): cannot find owner {alid}, {cellindex}, Need to Initialize.");
                            }

                        }
                        else if (nState == (long)GemAlarmState.SET)
                        {
                            if (isExist.ALIDs.Contains(alid) == false)
                            {
                                isExist.ALIDs.Add(alid);

                                LoggerManager.Debug($"[GemAlarmBackupInfo] Add, ALID = {alid}, CellIndex = {isExist.CellIndex}", isInfo:true);

                                retVal = EventCodeEnum.NONE;
                            }
                            else
                            {
                                retVal = EventCodeEnum.DUPLICATE_INVOCATION;

                                LoggerManager.Debug($"[GemAlarmBackupInfo] ModifyGemAlarmState(state:{nState}): skip add backup ALID,  ALID({alid}, {cellindex}) already exist.");
                            }

                        }
                        else
                        {
                            //nothing
                        }

                        SaveGemAlarmBackupFile();
                    }
                    else
                    {
                        retVal = EventCodeEnum.INVALID_PARAMETER_FIND;

                        LoggerManager.Debug($"[GemAlarmBackupInfo] ModifyGemAlarmState(state:{nState}): cannot find owner {alid}, {cellindex}, Need to Initialize.");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }



        /// <summary>
        /// ALID Backup 정보는 Owner만 가지고 있음. 로더, 셀 각각이 주체가 되어 Add/Remove 해줘야하기 때문이다.
        /// </summary>
        /// <param name="alid"></param>
        /// <returns></returns>
        public GemAlarmStateInfo FindALIDOwner(long alid)
        {
            GemAlarmStateInfo retVal = null;
            try
            {
                lock (backupInfoLock)
                {
                    retVal = GemAlarmBackupInfo.GemAlarmStateInfos.Where(w => w.ALIDs.Contains(alid)).FirstOrDefault();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

      
        #endregion
    }
}
