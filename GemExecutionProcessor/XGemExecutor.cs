using CommunicationModule;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Enum;
using ProberInterfaces.GEM;
using SecsGemServiceInterface;
using SecsGemServiceProxy;
using SoakingParameters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Windows;
using XGEMWrapper;

namespace GemExecutionProcessor
{
    //[CallbackBehavior(UseSynchronizationContext = false)]
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class XGemExecutor : IGemProcessorCore, INotifyPropertyChanged, IFactoryModule,
                               ISecsGemServiceCallback, IDisposable
    {
        #region <remarks> PropertyChanged                           </remarks>
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

        #region <remarks> GEM Module Property & Field               </remarks>
        private object _lockObj = new object();

        public object lockObj
        {
            get { return _lockObj; }
            set { _lockObj = value; }
        }
        public bool Initialized { get; set; } = false;

        private IGEMCommManager GemCommManager = null;
        private bool IsDisposed = false;

        private object proxylockobj = new object();
        #region ==> Gem Service Proxy & Callback
        // Gem Service와 직접적으로 연결하는 Proxy입니다.
        private SecsGemServiceDirectProxy _SecsGemServiceProxy;
        private SecsGemServiceDirectProxy SecsGemServiceProxy
        {
            get { return (SecsGemServiceDirectProxy)GetSecsGemServiceProxyBase(); }
            set
            {
                _SecsGemServiceProxy = value;
            }
        }
        public ClientBase<ISecsGemService> GetSecsGemServiceProxyBase()
        {
            SecsGemServiceDirectProxy proxy = null;
            try
            {
                if (_SecsGemServiceProxy != null)
                {
                    if (_SecsGemServiceProxy.State == CommunicationState.Opened || _SecsGemServiceProxy.State == CommunicationState.Created)
                        proxy = _SecsGemServiceProxy;
                }
                /* cell 시작이후 무조건 loader로 gem 연결 시도 하는 부분을 제거함, stage connect/disconnect와 동기를 맞춤
                else
                {
                    ConnectToCommander();
                }
                */
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return proxy;
        }
        public ISecsGemService GetSecsGemServiceProxy() => SecsGemServiceProxy;
        private ISecsGemService SecsGemClientCallback = null;
        #endregion

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

        public void GEMDisconnectCallBack(long lStageID)
        {
            return;
        }

        public void SetGEMDisconnectCallBack(GEMDisconnectDelegate callback)
        {
            return;
        }

        public void Proc_SetCommManager(IGEMCommManager gemManager)
        {
            GemCommManager = gemManager;
        }

        public bool HasSecsGemServiceProxy() => SecsGemClientCallback != null;
        //public ClientBase<ISecsGemService> GetSecsGemServiceProxy() => SecsGemServiceProxy;
        public void SetSecsGemServiceProxy(ClientBase<ISecsGemService> proxy)
        {
            SecsGemServiceProxy = proxy as SecsGemServiceDirectProxy;
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
                if(SecsGemClientCallback != null)
                {
                    retVal = SecsGemClientCallback.IsOpened();
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

        public bool GetConnectState(int index = 0)
        {
            bool retVal = false;
            try
            {
                retVal = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void ECVChangeMsgReceive(EquipmentReqData msgData)
        {
            if (msgData == null)
                return;
        }
        public void RemoteActMsgReceive(RemoteActReqData msgData)
        {
            if (msgData == null)
                return;

            byte HCACK = 0x04;//TODO: 이부분 S2F15 ACK 날리는 포맷다른데 일단 코드 처리해놓고 나중에 수정할 것.

            LoggerManager.Debug($"[GEM COMMANDER] OnRemoteCommandAction : {msgData.ActionType}");
            if (msgData.Stream == 2 && msgData.Function == 9)
            {
                S2F49SendAck(msgData.ObjectID, msgData.Stream, msgData.Function, msgData.Sysbyte, HCACK, msgData.Count);
            }

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
                //if (msgData == null)
                //    return;

                //byte CACK = 0;
                //string CommandFullName = "";

                //#region <remarks> CACK Define </remarks>
                //// <summary>
                //// CACK 정의
                //// </summary>
                //// <remarks>
                ////      0 = Acknowledge, command has been performed.
                ////      1 = Invalid command
                ////      2 = Can not perform now
                ////      3 = Invalid data or argument
                ////      4 = Acknowledge, request will be performed with completion signaled later by an event.
                ////      5 = Rejected. Invalid state.
                ////      6 = Command performed with errors.
                ////      7-63 Reserved.
                //// </remarks>
                //#endregion

                //if (msgData.ActionType == EnumCarrierAction.PROCEEDWITHCARRIER)
                //{
                //    S3F17SendAck(msgData.ObjectID, msgData.Stream, msgData.Function, msgData.Sysbyte, CACK, msgData.Count);
                //}
                //else if (msgData.ActionType == EnumCarrierAction.PROCEEDWITHSLOT)
                //{
                //    var varifyData = msgData as ProceedWithSlotReqData;
                //    if (true)
                //    {
                //        CACK = 0;
                //    }
                //    else
                //    {

                //    }
                //    S3F17SendAck(msgData.ObjectID, msgData.Stream, msgData.Function, msgData.Sysbyte, CACK, msgData.Count);                

                //    if (CACK == 0 || CACK == 4)
                //    {
                //        this.EventManager().RaisingEvent(typeof(SlotMapVarifyDoneEvent).FullName);
                //    }
                //}
                //else if (msgData.ActionType == EnumCarrierAction.CANCELCARRIER)
                //{

                //    S3F17SendAck(msgData.ObjectID, msgData.Stream, msgData.Function, msgData.Sysbyte, CACK, msgData.Count);
                //}
                //else
                //{

                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region  <remarks> OnRemoteCommandAction(EnumRemoteCommand command, RemoteActReqData msgData) </remarks>

        public void OnRemoteCommandAction(RemoteActReqData msgData)
        {
            try
            {
                LoggerManager.Debug($"[GEM EXECUTOR] OnRemoteCommandAction : {msgData.ActionType}");

                #region comment
                //switch (msgData.ActionType)
                //{
                //    case EnumRemoteCommand.UNDEFINE:
                //        break;
                //    case EnumRemoteCommand.SET_PARAMETERS:
                //        {
                //            var reqData = msgData as SetParameterActReqData;
                //            foreach (var parameter in reqData.ParameterDic)
                //            {
                //                LoggerManager.Debug($"[GEM EXECUTOR] OnRemoteCommandAction - SET_PARAMETERS: {parameter.Key}");
                //                switch (parameter.Key)
                //                {
                //                    case "SET_TEMPERATURE"://"HOTTEMPERATURE_HOTTEMPERATURE":
                //                        //EX) 90 도면 090 으로 들어옴.
                //                        double temp = Convert.ToDouble(parameter.Value) * 10;
                //                        this.TempController().SetSV(temp);
                //                        break;
                //                    case "PREHEATING_TIME"://"REPRE5":
                //                        //var soakparam = (this.SoakingModule().SoakingDeviceFile_IParam as SoakingDeviceFile).EventSoakingParams.
                //                        //    Single(param => param.EventSoakingType.Value == EnumSoakingType.LOTSTART_SOAK);
                //                        //if (soakparam != null)
                //                        //{
                //                        //    soakparam.Enable.Value = true;
                //                        //    soakparam.SoakingTimeInSeconds.Value = Convert.ToInt32(parameter.Value) * 60;
                //                        //}
                //                        break;
                //                    case "STOP_AT_FIRSTDIE"://"START0":
                //                        {
                //                            this.LotOPModule().LotInfo.StopBeforeProbeFlag = true;
                //                        }
                //                        break;
                //                    default:
                //                        break;
                //                }
                //            }
                //        }
                //        break;
                //    case EnumRemoteCommand.Z_UP:
                //        {
                //            var reqData = msgData as ZUpActReqData;
                //            bool cmdResult = false;
                //            if (this.ProbingModule().ModuleState.State == ModuleStateEnum.RUNNING ||
                //                this.ProbingModule().ModuleState.State == ModuleStateEnum.SUSPENDED)
                //            {
                //                cmdResult = this.CommandManager().SetCommand<IZUPRequest>(this);

                //                LoggerManager.Debug($"[GEM EXECUTOR] OnRemoteCommandAction - Z_UP: SetCommand<IZUPRequest> {cmdResult}");
                //            }
                //            else
                //            {
                //                LoggerManager.Debug($"[GEM EXECUTOR] OnRemoteCommandAction - Z_UP: Don't send SetCommand because probingmodule state is {this.ProbingModule().ModuleState.State}");
                //            }
                //        }
                //        break;
                //    case EnumRemoteCommand.END_TEST:
                //        {
                //            var reqData = msgData as EndTestReqDate;
                //            LoggerManager.Debug($"[GEM EXECUTOR] OnRemoteCommandAction - END_TEST: PMIExecFlag {reqData.PMIExecFlag} 0:RECIPE PMI, 1: SKIP PMI, 2: EXCUTE PMI");

                //            if (this.ProbingModule().ModuleState.GetState() != ModuleStateEnum.PAUSED)
                //            {
                //                PMIRemoteOperationEnum remotevalue = PMIRemoteOperationEnum.UNDEFIEND;

                //                switch (reqData.PMIExecFlag)
                //                {
                //                    //RECIPE PMI
                //                    case 0:
                //                        remotevalue = PMIRemoteOperationEnum.ITSELF;
                //                        break;
                //                    // SKIP PMI
                //                    case 1:
                //                        remotevalue = PMIRemoteOperationEnum.SKIP;
                //                        break;
                //                    // EXCUTE PMI
                //                    case 2:
                //                        remotevalue = PMIRemoteOperationEnum.FORCEDEXECUTE;
                //                        break;
                //                    default:
                //                        remotevalue = PMIRemoteOperationEnum.UNDEFIEND;
                //                        break;
                //                }

                //                this.PMIModule().SetRemoteOperation(remotevalue);

                //                this.CommandManager().SetCommand<IMoveToNextDie>(this);
                //                //this.CommandManager().SetCommand<IUnloadWafer>(this);
                //            }

                //        }
                //        break;
                //    case EnumRemoteCommand.ERROR_END:
                //        {
                //            this.LoaderController().IsCancel = true;
                //            var lockobj = this.StageSupervisor().GetStagePIV().GetPIVDataLockObject();
                //            lock (lockobj)
                //            {
                //                //this.StageSupervisor().GetStagePIV().BackupStageLotInfo(this.StageSupervisor().GetStagePIV().LoadFoupNumber.Value, lotid: this.StageSupervisor().GetStagePIV().LotID.Value);
                //                //this.StageSupervisor().GetStagePIV().LotID.Value = " ";
                //                //this.StageSupervisor().GetStagePIV().ResetWaferID("");
                //                //this.StageSupervisor().GetStagePIV().SetStageLotInfo(this.StageSupervisor().GetStagePIV().LoadFoupNumber.Value, lotid: this.StageSupervisor().GetStagePIV().LotID.Value);
                //                this.StageSupervisor().GetStagePIV().XCoordinate.Value = -9999;
                //                this.StageSupervisor().GetStagePIV().YCoordinate.Value = -9999;
                //                this.StageSupervisor().GetStagePIV().SetStageState(GEMStageStateEnum.UNLOADING);
                //                this.GEMModule().SetEvent(this.GEMModule().GetEventNumberFormEventName(typeof(WaferTestingAborted).FullName));
                //                if (this.StageSupervisor().WaferObject.GetStatus() == EnumSubsStatus.EXIST)
                //                {
                //                    this.StageSupervisor().WaferObject.SetSkipped();
                //                }
                //            }
                //        }
                //        break;
                //    case EnumRemoteCommand.START_STAGE:
                //        {
                //            //Data 살리기.
                //            var reqData = msgData as StartStage;
                //            if (reqData != null)
                //            {
                //                var lockobj = this.StageSupervisor().GetStagePIV().GetPIVDataLockObject();
                //                lock (lockobj)
                //                {
                //                    //this.StageSupervisor().GetStagePIV().SetBackupStageLotInfo(reqData.FoupNumber, reqData.LotID);
                //                }
                //            }
                //            this.CommandManager().SetCommand<ILotOpResume>(this);
                //        }
                //        break;
                //    default:
                //        break;
                //}
                #endregion

                this.GEMModule().ExcuteRemoteCommandAction(msgData, this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void OnDefineReportRecive(SecsGemDefineReport report)
        {
            return;
        }

        #endregion

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

                retVal = SecsGemServiceProxy?.SendSECSMessage(pnObjectID, nStream, nFunction + 1, nSysbyte) ?? -1;
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
            return 0;
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
                else if (value.GetType() == typeof(int)||
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
                else if (value.GetType() == typeof(double)||
                         value.GetType() == typeof(float)||
                         value.GetType() == typeof(Double)
                         )
                {
                    SecsGemServiceProxy.SetFloat8Item(objectid, (double)value);                   
                }
                else if (value.GetType() == typeof(bool)||
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
                SecsGemServiceProxy?.SetUint1Item(pnObjectID, CAACK);
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

                    SecsGemServiceProxy?.GEMSetVariable(1, GemCommManager.GetOrgDataID(1200), new string[] { "SOFTREV" });
                    SecsGemServiceProxy?.GEMSetVariable(1, GemCommManager.GetOrgDataID(1201), new string[] { version });

                    GemCommManager.CommEnable();

                    DoUpdateGemVariables();
                    GEM_Init_SVID();
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
                if (CommState == SecsCommStateEnum.COMM_DISABLED)
                {
                    this.GEMModule().CommunicationState = EnumCommunicationState.DISCONNECT;
                }
                else
                {
                    this.GEMModule().CommunicationState = EnumCommunicationState.CONNECTED;
                }
                this.GemCommManager.SecsCommInformData.CommunicationState = (SecsEnum_CommunicationState)CommState;

                getInfo = GetCommInfo(getCommDataName);
                SetCommInfo(getInfo, getCommDataName);

                long nCount = 1;
                long[] naId = GemCommManager.GetOrgDataID(1001); // Control State SV 값 조회
                string[] saVal = new string[1];

                long ret = this.SecsGemServiceProxy?.GEMGetVariable(nCount, ref naId, ref saVal) ?? -1;
                if (ret == 0 && saVal != null && 0 < saVal.Length)
                {
                    int iControlState = -1;
                    bool parseResult = false;
                    parseResult = int.TryParse(saVal[0], out iControlState);

                    if (parseResult != false && ((((int)SecsEnum_ControlState.UNKNOWN) < iControlState) && (iControlState <= ((int)SecsEnum_ControlState.ONLINE_REMOTE))))
                    {
                        this.GemCommManager.SecsCommInformData.ControlState = (SecsEnum_ControlState)iControlState;
                    }
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

                MessageBox.Show(TerminalData.ToString());
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
                MessageBox.Show("Single\n" + sMsg);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        #endregion

        #region <remarks> void OnGEMReqDateTime(long nMsgId, string sSystemTime) </remarks>
        /// <summary>
        ///     Termial Message가 왔을 때 발생하는 Event를 처리하는 함수.
        /// </summary>
        /// <remarks>
        ///     GEM -> Main.
        ///     First Create : 2019-05-16, Semics R&D1, Jake Kim.
        /// </remarks>
        /// <param name="nTid">      Terminal ID          </param>
        /// <param name="sMsg">     메세지          </param>
        public void OnGEMReqDateTime(long nMsgId, string sSystemTime)
        {
            try
            {
                SystemTime updatedTime = new SystemTime();
                updatedTime.Year = Convert.ToUInt16(sSystemTime.Substring(0, 4));
                updatedTime.Month = Convert.ToUInt16(sSystemTime.Substring(4, 2));
                updatedTime.Day = Convert.ToUInt16(sSystemTime.Substring(6, 2));
                updatedTime.Hour = Convert.ToUInt16(sSystemTime.Substring(8, 2));
                updatedTime.Minute = Convert.ToUInt16(sSystemTime.Substring(10, 2));
                updatedTime.Second = Convert.ToUInt16(sSystemTime.Substring(12, 2));
                updatedTime.Millisecond = Convert.ToUInt16(sSystemTime.Substring(14, 2));

                SetLocalTime(ref updatedTime);
                LoggerManager.Debug($"[GEM] Update System Time : {updatedTime}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            //throw new NotImplementedException();
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

                //rspremotecommand는 GEMRspRemoteCommand2 함수만 사용한다.(그냥 ..Command는 사용안한다.)
                SecsGemServiceProxy?.GEMRspRemoteCommand2(nMsgId, Rcmd.ToString(), retHCAck, CPName?.Length ?? 0, CPName, CPVal); //5번째 6번째는 각각의
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
                SecsGemServiceProxy?.GEMRspPPList(nMsgId, ppList.Length, ppList.ToArray());
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
        }
        #endregion

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

        #endregion

        #region <remarks> GEM Module Methods & Command              </remarks>

        #region <remarks> Initialize Module </remarks>

        public bool InitSecsGem(String ConfigPath = @"C:\ProberSystem\Parameters\SystemParam\GEM\GemConfig.cfg")
        {
            bool retVal = false;

            try
            {
                if (System.IO.File.Exists(ConfigPath))
                {
                    retVal = SecsGemServiceProxy?.Initialize(ConfigPath) == 0 ? true : false;
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

        public long Start()
        {
            long retVal = -1;
            try
            {
                retVal = this.SecsGemServiceProxy?.Start() ?? -1;
            }
            catch(Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                retVal = -1;
            }

            return retVal;
        }

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

        public void Proc_SetParamInfoToGem()
        {
            try
            {
                SecsGemServiceProxy?.GEMSetParam(GemGlobalData.IP, this.GemCommManager.SecsCommInformData.HSMSIP);
                SecsGemServiceProxy?.GEMSetParam(GemGlobalData.Port, this.GemCommManager.SecsCommInformData.HSMSPort);
                SecsGemServiceProxy?.GEMSetParam(GemGlobalData.Active, this.GemCommManager.SecsCommInformData.HSMSPassive == SecsEnum_Passive.ACTIVE ? "true" : "false");

                SecsGemServiceProxy?.GEMSetParam(GemGlobalData.DeviceID, this.GemCommManager.SecsCommInformData.HSMSDeviceID);
                SecsGemServiceProxy?.GEMSetParam(GemGlobalData.LinkTestInterval, this.GemCommManager.SecsCommInformData.HSMSLinkTestInterval);
                SecsGemServiceProxy?.GEMSetParam(GemGlobalData.RetryLimit, this.GemCommManager.SecsCommInformData.HSMSRetryLimit);

                SecsGemServiceProxy?.GEMSetParam(GemGlobalData.T3, this.GemCommManager.SecsCommInformData.T3);
                SecsGemServiceProxy?.GEMSetParam(GemGlobalData.T5, this.GemCommManager.SecsCommInformData.T5);
                SecsGemServiceProxy?.GEMSetParam(GemGlobalData.T6, this.GemCommManager.SecsCommInformData.T6);
                SecsGemServiceProxy?.GEMSetParam(GemGlobalData.T7, this.GemCommManager.SecsCommInformData.T7);
                SecsGemServiceProxy?.GEMSetParam(GemGlobalData.T8, this.GemCommManager.SecsCommInformData.T8);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void DoUpdateGemVariables()
        {
            //SecsGemClient.GEMSetVariable(1, new long[1] { SVID_TEMP_REF_VALUE - 1025 },       new string[] { "value" });
            //SecsGemClient.GEMSetVariable(1, new long[1] { SVID_BARCODE_LOTID  - 1102 },       new string[] { "value" });
            //SecsGemClient.GEMSetVariable(1, new long[1] { SVID_PROBE_CARDID   - 1105 },       new string[] { "value" });
            //SecsGemClient.GEMSetVariable(1, new long[1] { SVID_CASSETTE_MAP1  - 1150 },       new string[] { "value" });
            //SecsGemClient.GEMSetVariable(1, new long[1] { SVID_STATION        - 1203 },       new string[] { "value" });

            //SecsGemClient.GEMSetVariable(1, new long[1] { DVID_CHUCK_SLOTNO           - 1508 },       new string[] { "value" });
            //SecsGemClient.GEMSetVariable(1, new long[1] { DVID_EVENT_WAFER_FLATANGLE  - 1522 },       new string[] { "value" });
            //SecsGemClient.GEMSetVariable(1, new long[1] { DVID_DEVICE_NAME            - 30098 },      new string[] { "value" });
            //SecsGemClient.GEMSetVariable(1, new long[1] { DVID_OVERDRIVE              - 30198 },      new string[] { "value" });
            //SecsGemClient.GEMSetVariable(1, new long[1] { DVID_GROSS_DIE_CNT          - 30848 },      new string[] { "value" });
        }

        public void GEM_Init_SVID()
        {
            //SecsGemClient.GEMSetVariable(1, new long[1] { SVID_PROBE_CARDID   - 1105 },       new string[] { "value" });
            //SecsGemClient.GEMSetVariable(1, new long[1] { SVID_PROBER_ID      - 1203 },       new string[] { "value" });
            //SecsGemClient.GEMSetVariable(1, new long[1] { SVID_STATION        - 1203 },       new string[] { "value" });
        }

        public long Proc_SetEstablish(long bState)
        {
            long retVal = -1;
            try
            {
                retVal = SecsGemServiceProxy?.GEMSetEstablish(bState) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public long Proc_ReqOffline()
        {
            long retVal = -1;
            try
            {
                retVal = SecsGemServiceProxy?.GEMReqOffline() ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public long Proc_ReqLocal()
        {
            long retVal = -1;
            try
            {
                retVal = SecsGemServiceProxy?.GEMReqLocal() ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public long Proc_ReqRemote()
        {
            long retVal = -1;
            try
            {
                retVal = SecsGemServiceProxy?.GEMReqRemote() ?? -1;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public long Proc_TimeRequest()
        {
            long retVal = -1;
            try
            {
                retVal = SecsGemServiceProxy?.GEMReqGetDateTime() ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public long Proc_SendTerminal(string sendStr)
        {
            long retVal = -1;
            try
            {
                retVal = SecsGemServiceProxy?.GEMSetTerminalMessage(0, sendStr) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public long Proc_SetVariable(int vidLength, long[] convertDataID, string[] values, bool immediatelyUpdate = false)
        {
            long retVal = -1;
            try
            {
                lock (proxylockobj)
                {
                    retVal = SecsGemServiceProxy?.GEMSetVariable(vidLength, convertDataID, values, -1, immediatelyUpdate) ?? -1;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public long Proc_SetVariables(long nObjectID, long nVid, bool immediatelyUpdate = false)
        {
            long retVal = -1;
            try
            {
                lock (proxylockobj)
                {
                    retVal = SecsGemServiceProxy?.GEMSetVariables(nObjectID, nVid) ?? -1;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public long Proc_SetECVChanged(int vidLength, long[] convertDataID, string[] values)
        {
            return SecsGemServiceProxy?.GEMSetECVChanged(vidLength, convertDataID, values) ?? -1;
        }

        public long Proc_SetEvent(long eventNum)//, int stgnum = -1, List<Dictionary<long, (int objtype, object value)>> ExecutorDataDic = null)
        {
            try
            {
                int stageNum = -1;
                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    stageNum = this.LoaderController().GetChuckIndex();
                    this.GEMModule().GetPIVContainer().CurTemperature.Value = this.TempController().TempInfo.CurTemp.Value;
                    ///현재 Load 된 Foup의 Lot ID 로 셋팅.
                    //var loadfoupandlotiddic = this.StageSupervisor().GetStagePIV().GetLoadFoupAndLotID();
                    //if (loadfoupandlotiddic.ContainsKey
                    //    (this.StageSupervisor().GetStagePIV().LoadFoupNumber.Value))
                    //{
                    //    string lotid = "";
                    //    loadfoupandlotiddic.TryGetValue(this.StageSupervisor().GetStagePIV().LoadFoupNumber.Value, out lotid);
                    //    this.StageSupervisor().GetStagePIV().LotID.Value = lotid;
                    //}
                }
                long? retVal = -1;

                if(eventNum != -1)
                {
                    lock (proxylockobj)
                    {
                        retVal = SecsGemServiceProxy?.GEMSetEvent(eventNum, stageNum) ?? -1;
                    }
                }
                return retVal == null ? -1 : (long)retVal;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return -1;
            }
        }
        public long Proc_SetAlarm(long nID, long nState, int cellIndex = 0)
        {
            try
            {
                long? retVal = -1;
                retVal = SecsGemServiceProxy?.GEMSetAlarm(nID, nState, cellIndex);//xgemcommanderhost에 있는 Proc_SetAlarm을 불러줌.
                return retVal == null ? -1 : (long)retVal;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public EventCodeEnum Proc_ClearAlarmOnly(int cellIndex = 0)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                var retval = SecsGemServiceProxy?.ClearAlarmOnly(cellIndex);//xgemcommanderhost에 있는 Proc_SetAlarm을 불러줌.
                    
                if(retval >= 0)
                {
                    ret = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        #endregion

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    IsDisposed = false;
                    Initialized = true;
                    retval = EventCodeEnum.NONE;
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
                if (this.GEMModule().GemSysParam.GemProcessrorType.Value == GemProcessorType.SINGLE)
                {
                    retVal = GemCommManager?.StartWcfGemService() ?? EventCodeEnum.UNDEFINED;
                }
                else if (this.GEMModule().GemSysParam.GemProcessrorType.Value == GemProcessorType.CELL)
                {
                    try
                    {
                        retVal = ConnectToCommander();
                        this.GEMModule().GetPIVContainer().SetDeviceName(this.FileManager().GetDeviceName());
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
            if (this.GEMModule().GemSysParam.GemProcessrorType.Value == GemProcessorType.SINGLE)
            {
                GemCommManager?.ProcessClose();
            }
            else if (this.GEMModule().GemSysParam.GemProcessrorType.Value == GemProcessorType.CELL)
            {
                try
                {
                    // [STM_CATANIA] Disconnect 했을 때, 상태값을 받고 싶어하기 때문에 Enum을 추가해서 처리함.
                    this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.DISCONNECTED);
                    // 데이터 연결 해제를 먼저하고, Close/Abort를 진행할 수 있도록 순서를 변경함.
                    this.GEMModule().DeleteNotifyEventToElement();

                    if (_SecsGemServiceProxy != null)
                    {
                        lock (proxylockobj)
                        {
                            try
                            {
                                if (_SecsGemServiceProxy.State == CommunicationState.Opened)
                                {
                                    _SecsGemServiceProxy.Close();
                                }
                                else if (_SecsGemServiceProxy.State == CommunicationState.Faulted)
                                {
                                    _SecsGemServiceProxy.Abort();
                                }
                            }
                            catch (Exception err)
                            {
                                LoggerManager.Exception(err);
                            }

                            _SecsGemServiceProxy = null;
                            
                        }
                    }

                    if(SecsGemClientCallback != null)
                    {
                        var obj = SecsGemClientCallback as ICommunicationObject;
                        var objstate = obj.State;
                        if(objstate != CommunicationState.Faulted && objstate != CommunicationState.Closed)
                        {
                            SecsGemClientCallback.Close_SECSGEM(this.LoaderController().GetChuckIndex());
                        }
                        SecsGemClientCallback = null;
                    }
                    retVal = EventCodeEnum.NONE;
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
            return retVal;
        }
        public EventCodeEnum InitGemData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //Init Stage State & PreHeat State & Probe Card ID
                IStagePIV stagePIV = this.GEMModule().GetPIVContainer() as IStagePIV;
                if (stagePIV != null)
                {

                    stagePIV.StageNumber.Value = this.LoaderController().GetChuckIndex();
                    // [STM_CATANIA] State 값을 받고 싶어하기 때문에 Enum을 추가해서 처리함.
                    this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.LOTOP_IDLE);
                    stagePIV.SetStageState((GEMStageStateEnum)stagePIV.StageState.Value);

                    stagePIV.SetProberCardID(this.CardChangeModule().GetProbeCardID());
                    // stagePIV.SetProberCardID(this.GetParam_ProbeCard().ProbeCardDevObjectRef.ProbeCardID.Value);
                    stagePIV.RecipeID.Value = this.StageSupervisor().GetDeviceName();

                    this.GEMModule().GetPIVContainer().SetDeviceName(
                       stagePIV.RecipeID.Value);
                    this.GEMModule().GetPIVContainer().SetSVTemp(
                        this.TempController().TempInfo.TargetTemp.Value);

                    var soakparam = (this.SoakingModule().SoakingDeviceFile_IParam as SoakingDeviceFile).EventSoakingParams.
                        Single(param => param.EventSoakingType.Value == EnumSoakingType.LOTSTART_SOAK);
                    if (soakparam != null)
                    {
                        if (soakparam.Enable.Value)
                            stagePIV.SetPreHeatState(GEMPreHeatStateEnum.PRE_HEATING);
                        else
                            stagePIV.SetPreHeatState(GEMPreHeatStateEnum.NOT_PRE_HEATING);
                    }
                }
                else
                {
                    LoggerManager.Debug("InitGemData() : PIVContainer is null");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void DeInitModule()
        {
            
        }

        public void Dispose()
        {
            try
            {
                if (this.IsDisposed == false)
                {
                    this.IsDisposed = true;
                    this.Initialized = false;

                    DisConnectService();
                    this.CloseGemServiceHost();  //1. Host Close
                    this.GemCommManager.ProcessClose();       //2. Process Close
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void CloseGemServiceHost()
        {
            try
            {
                SecsGemServiceProxy?.Close_SECSGEM();
                SecsGemServiceProxy = null;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// Commander와 연결합니다.
        /// </summary>
        /// <returns></returns>
        private EventCodeEnum ConnectToCommander()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {

                if (CommunicationManager.CheckAvailabilityCommunication(this.LoaderController().GetLoaderIP(), 7513))
                {
                    // Loader와 연결이 끊어졌는지 확인하고, 끊겼을 경우 DisConnect 후에 Connect 로직을 탈 수 있도록 처리
                    if (_SecsGemServiceProxy != null)
                    {
                        try
                        {
                            _SecsGemServiceProxy.Are_You_There();
                        }
                        catch (Exception err)
                        {
                            System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                            LoggerManager.Debug($"[XGemExcutor] Failed to connect to the XGemCommanderHost. ( isOpen )");
                            DisConnectService();
                        }
                    }

                    // true : State - created, opened ... | false : State - closed, faulted.
                    var statecheck = (_SecsGemServiceProxy as System.ServiceModel.ICommunicationObject)?.State == CommunicationState.Closed
                                | (_SecsGemServiceProxy as System.ServiceModel.ICommunicationObject)?.State == CommunicationState.Faulted;

                    if (_SecsGemServiceProxy == null | statecheck)
                    {
                        var context = new InstanceContext(this);
                        Binding binding = new NetTcpBinding()
                        {
                            Security = new NetTcpSecurity() { Mode = SecurityMode.None },
                            MaxBufferPoolSize = 2147483647,
                            MaxBufferSize = 2147483647,
                            MaxReceivedMessageSize = 2147483647,
                            SendTimeout = TimeSpan.MaxValue,
                            ReceiveTimeout = TimeSpan.MaxValue
                        };
                        EndpointAddress endpointAddress = new EndpointAddress($"net.tcp://{this.LoaderController().GetLoaderIP()}:7513/secsgempipe");
                        var contract = ContractDescription.GetContract(typeof(ISecsGemService));
                        var serviceEndpoint = new ServiceEndpoint(contract, binding, endpointAddress);
                        _SecsGemServiceProxy = new SecsGemServiceDirectProxy(context, serviceEndpoint);
                        _SecsGemServiceProxy?.Initialize(null);
                        var duplex = new DuplexChannelFactory<ISecsGemService>(context, binding, endpointAddress);
                        SecsGemClientCallback = duplex.CreateChannel(context);

                        try
                        {
                            SecsGemClientCallback.ServerConnect(this.LoaderController().GetChuckIndex());
                            LoggerManager.Debug($"[SECS/GEM][{this.GetType().Name}] ConnectSuccess() - GEM Init");

                            (_SecsGemServiceProxy as ICommunicationObject).Faulted += GemServiceFaultedEventHandler;
                            (_SecsGemServiceProxy as ICommunicationObject).Closed += GemServiceClosedEventHandler;
                            retval = EventCodeEnum.NONE;
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Debug($"[SECS/GEM][{this.GetType().Name}] ConnectFailed() - Failed GEM Init (Exception : {err}");
                            _SecsGemServiceProxy = null;
                        }
                    }
                    else
                    {
                        //연결된 상태
                        retval = EventCodeEnum.NONE;
                    }
                }
                
            }
            catch (Exception err)
            {
                _SecsGemServiceProxy = null;
                throw err;
            }

            return retval;
        }

        private void GemServiceFaultedEventHandler(object sender, EventArgs e)
        {
            try
            {
                // DisConnect to SecsGemServiceHost.
                // 알람, 메세지 추가
                if (this.GEMModule().GemSysParam.GemProcessrorType.Value == GemProcessorType.CELL)
                {
                    if (_SecsGemServiceProxy != null)
                    {
                        if (_SecsGemServiceProxy.State == CommunicationState.Closed)
                            return; // already closed.
                    }
                }

                DisConnectService();

                // cell을 재연결 하거나 다른 곳에서 재연결을 하는 트리거가 있기 때문에 여기서 할 필요가 없어서 주석처리 함.
                /*
                LoggerManager.Debug("[SECS/GEM] Gem connect Error - Faulted ( from Loader )");
                this.MetroDialogManager().ShowMessageDialog(
                    "Gem connect Error ( from Loader )",
                    "The SECS/GEM connection with the loader has been disconnected.", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                //this.GemCommManager.InitConnectService();
                */
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void GemServiceClosedEventHandler(object sender, EventArgs e)
        {
            try
            {
                LoggerManager.Debug("[SECS/GEM] Gem connect Error - Closed ( from Loader )");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public List<long> GetVidsFormCEID(long ceid)
        {
            return null;
        }

        public void GEMRspRemoteCommand(long nMsgId, string sCmd, long nHCAck, long nCount, long[] pnResult)
        {
            try
            {
                LoggerManager.Debug($"[GEM EXECUTOR] GEMRspRemoteCommand : {sCmd}");
                SecsGemServiceProxy.GEMRspRemoteCommand(nMsgId, sCmd, nHCAck, nCount, pnResult);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
