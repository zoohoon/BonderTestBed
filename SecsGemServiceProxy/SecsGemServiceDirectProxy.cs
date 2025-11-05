using ProberInterfaces;
using SecsGemServiceInterface;
using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using XGEMWrapper;

namespace SecsGemServiceProxy
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class SecsGemServiceDirectProxy : ClientBase<ISecsGemService> , IFactoryModule, ISecsGemServiceProxy
    {
        private string _IP;
        public string IP
        {
            get { return _IP; }
            set { _IP = value; }
        }

        private int _Port;
        public int Port
        {
            get { return _Port; }
            set { _Port = value; }
        }

        public object lockObj = new object();
        public SecsGemServiceDirectProxy(InstanceContext callback, ServiceEndpoint endpoint) :
            base(callback, endpoint)
        {
            if (endpoint.Binding is NetTcpBinding)
            {
                IP = endpoint.Address.Uri.Host;
                Port = endpoint.Address.Uri.Port;
            }
        }

        public CommunicationState GetState()
        {
            return State;
        }

        public bool IsOpened()
        {
            return (State == CommunicationState.Opened || State == CommunicationState.Created);
        }

        public long Init_SECSGEM(string Config)
        {
            return Channel.Init_SECSGEM(Config);
        }
        public long Close_SECSGEM(int proberId = 0)
        {
            if (!IsOpened())
                return -1;
            return Channel.Close_SECSGEM();
        }

        public long Start()
        {
            if (!IsOpened())
                return -1;
            return Channel.Start();
        }

        public long Stop()
        {
            if (!IsOpened())
                return -1;
            return Channel.Stop();
        }

        public void ServerConnect(int proberId)
        {
            if (!IsOpened())
                return;
            Channel.ServerConnect(proberId);
        }

        public long MakeObject(ref long pnObjectID)
        {
            if (!IsOpened())
                return -1;
            return Channel.MakeObject(ref pnObjectID);
        }

        public long SetListItem(long nObjectID, long nItemCount)
        {
            if (!IsOpened())
                return -1;
            return Channel.SetListItem(nObjectID, nItemCount);
        }

        public long SetBinaryItem(long nObjectID, byte nValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.SetBinaryItem(nObjectID, nValue);
        }

        public long SetBoolItem(long nObjectID, bool nValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.SetBoolItem(nObjectID, nValue);
        }

        public long SetBoolItems(long nObjectID, bool[] pnValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.SetBoolItems(nObjectID, pnValue);
        }

        public long SetUint1Item(long nObjectID, byte nValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.SetUint1Item(nObjectID, nValue);
        }

        public long SetUint1Items(long nObjectID, byte[] pnValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.SetUint1Items(nObjectID, pnValue);
        }

        public long SetUint2Item(long nObjectID, ushort nValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.SetUint2Item(nObjectID, nValue);
        }

        public long SetUint2Items(long nObjectID, ushort[] pnValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.SetUint2Items(nObjectID, pnValue);
        }

        public long SetUint4Item(long nObjectID, uint nValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.SetUint4Item(nObjectID, nValue);
        }

        public long SetUint4Items(long nObjectID, uint[] pnValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.SetUint4Items(nObjectID, pnValue);
        }

        public long SetUint8Item(long nObjectID, ulong nValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.SetUint8Item(nObjectID, nValue);
        }

        public long SetUint8Items(long nObjectID, ulong[] pnValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.SetUint8Items(nObjectID, pnValue);
        }

        public long SetInt1Item(long nObjectID, sbyte nValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.SetInt1Item(nObjectID, nValue);
        }

        public long SetInt1Items(long nObjectID, sbyte[] pnValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.SetInt1Items(nObjectID, pnValue);
        }

        public long SetInt2Item(long nObjectID, short nValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.SetInt2Item(nObjectID, nValue);
        }

        public long SetInt2Items(long nObjectID, short[] pnValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.SetInt2Items(nObjectID, pnValue);
        }

        public long SetInt4Item(long nObjectID, int nValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.SetInt4Item(nObjectID, nValue);
        }

        public long SetInt4Items(long nObjectID, int[] pnValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.SetInt4Items(nObjectID, pnValue);
        }

        public long SetInt8Item(long nObjectID, long nValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.SetInt8Item(nObjectID, nValue);
        }

        public long SetInt8Items(long nObjectID, long[] pnValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.SetInt8Items(nObjectID, pnValue);
        }

        public long SetFloat4Item(long nObjectID, float nValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.SetFloat4Item(nObjectID, nValue);
        }

        public long SetFloat4Items(long nObjectID, float[] pnValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.SetFloat4Items(nObjectID, pnValue);
        }

        public long SetFloat8Item(long nObjectID, double nValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.SetFloat8Item(nObjectID, nValue);
        }

        public long SetFloat8Items(long nObjectID, double[] pnValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.SetFloat8Items(nObjectID, pnValue);
        }

        public long SetStringItem(long nObjectID, string pszValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.SetStringItem(nObjectID, pszValue);
        }

        public long GetListItem(long nObjectID, ref long pnItemCount)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetListItem(nObjectID, ref pnItemCount);
        }

        public long GetBinaryItem(long nObjectID, ref byte[] pnValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetBinaryItem(nObjectID, ref pnValue);
        }

        public long GetBoolItem(long nObjectID, ref bool[] pnValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetBoolItem(nObjectID, ref pnValue);
        }

        public long GetUint1Item(long nObjectID, ref byte[] pnValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetUint1Item(nObjectID, ref pnValue);
        }

        public long GetUint2Item(long nObjectID, ref ushort[] pnValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetUint2Item(nObjectID, ref pnValue);
        }

        public long GetUint4Item(long nObjectID, ref uint[] pnValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetUint4Item(nObjectID, ref pnValue);
        }

        public long GetUint8Item(long nObjectID, ref ulong[] pnValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetUint8Item(nObjectID, ref pnValue);
        }

        public long GetInt1Item(long nObjectID, ref sbyte[] pnValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetInt1Item(nObjectID, ref pnValue);
        }

        public long GetInt2Item(long nObjectID, ref short[] pnValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetInt2Item(nObjectID, ref pnValue);
        }

        public long GetInt4Item(long nObjectID, ref int[] pnValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetInt4Item(nObjectID, ref pnValue);
        }

        public long GetInt8Item(long nObjectID, ref long[] pnValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetInt8Item(nObjectID, ref pnValue);
        }

        public long GetFloat4Item(long nObjectID, ref float[] pnValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetFloat4Item(nObjectID, ref pnValue);
        }

        public long GetFloat8Item(long nObjectID, ref double[] pnValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetFloat8Item(nObjectID, ref pnValue);
        }

        public long GetStringItem(long nObjectID, ref string psValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetStringItem(nObjectID, ref psValue);
        }

        public long CloseObject(long nObjectID)
        {
            if (!IsOpened())
                return -1;
            return Channel.CloseObject(nObjectID);
        }

        public long SendSECSMessage(long nObjectID, long nStream, long nFunction, long nSysbyte)
        {
            if (!IsOpened())
                return -1;
            return Channel.SendSECSMessage(nObjectID, nStream, nFunction, nSysbyte);
        }

        public long GEMReqOffline()
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMReqOffline();
        }

        public long GEMReqLocal()
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMReqLocal();
        }

        public long GEMReqRemote()
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMReqRemote();
        }

        public long GEMSetEstablish(long bState)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMSetEstablish(bState);
        }

        public long GEMSetParam(string sParamName, string sParamValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMSetParam(sParamName, sParamValue);
        }

        public long GEMGetParam(string sParamName, ref string psParamValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMGetParam(sParamName, ref psParamValue);
        }

        public long GEMEQInitialized(long nInitType)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMEQInitialized(nInitType);
        }

        public long GEMReqGetDateTime()
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMReqGetDateTime();
        }

        public long GEMRspGetDateTime(long nMsgId, string sSystemTime)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMRspGetDateTime(nMsgId, sSystemTime);
        }

        public long GEMRspDateTime(long nMsgId, long nResult)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMRspDateTime(nMsgId, nResult);
        }

        public long GEMSetAlarm(long nID, long nState, int stageNum = 0)
        {
            if (!IsOpened())
                return -1;
            //return Channel.GEMRspDateTime(nID, nState);
            return Channel.GEMSetAlarm(nID, nState, stageNum);
        }

        public long ClearAlarmOnly(int cellIndex = 0)
        {
            if (!IsOpened())
                return -1;
            //return Channel.GEMRspDateTime(nID, nState);
            return Channel.ClearAlarmOnly(cellIndex);
        }

        public long GEMRspRemoteCommand(long nMsgId, string sCmd, long nHCAck, long nCount, long[] pnResult)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMRspRemoteCommand(nMsgId, sCmd, nHCAck, nCount, pnResult);
        }

        public long GEMRspRemoteCommand2(long nMsgId, string sCmd, long nHCAck, long nCount, string[] psCpName, long[] pnCpAck)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMRspRemoteCommand2(nMsgId, sCmd, nHCAck, nCount, psCpName, pnCpAck);
        }

        public long GEMRspChangeECV(long nMsgId, long nResult)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMRspChangeECV(nMsgId, nResult);
        }

        public long GEMSetECVChanged(long nCount, long[] pnEcIds, string[] psEcVals, int stageNum = -1)
        {
            lock (lockObj)
            {
                if (!IsOpened())
                    return -1;
                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                    return Channel.GEMSetECVChanged(nCount, pnEcIds, psEcVals, this.LoaderController().GetChuckIndex());
                else
                    return Channel.GEMSetECVChanged(nCount, pnEcIds, psEcVals);
            }
        }

        public long GEMReqAllECInfo()
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMReqAllECInfo();
        }

        public long GEMSetPPChanged(long nMode, string sPpid, long nLength, string pbBody)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMSetPPChanged(nMode, sPpid, nLength, pbBody);
        }

        public long GEMSetPPFmtChanged(long nMode, string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMSetPPFmtChanged(nMode, sPpid, sMdln, sSoftRev, nCount, psCCode, pnParamCount, psParamNames);
        }

        public long GEMReqPPLoadInquire(string sPpid, long nLength)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMReqPPLoadInquire(sPpid, nLength);
        }

        public long GEMRspPPLoadInquire(long nMsgId, string sPpid, long nResult)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMRspPPLoadInquire(nMsgId, sPpid, nResult);
        }

        public long GEMReqPPSend(string sPpid, string pbBody)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMReqPPSend(sPpid, pbBody);
        }

        public long GEMRspPPSend(long nMsgId, string sPpid, long nResult)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMRspPPSend(nMsgId, sPpid, nResult);
        }

        public long GEMReqPP(string sPpid)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMReqPP(sPpid);
        }

        public long GEMRspPP(long nMsgId, string sPpid, string pbBody)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMRspPP(nMsgId, sPpid, pbBody);
        }

        public long GEMRspPPDelete(long nMsgId, long nCount, string[] psPpids, long nResult)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMRspPPDelete(nMsgId, nCount, psPpids, nResult);
        }

        public long GEMRspPPList(long nMsgId, long nCount, string[] psPpids)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMRspPPList(nMsgId, nCount, psPpids);
        }

        public long GEMReqPPFmtSend(string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMReqPPFmtSend(sPpid, sMdln, sSoftRev, nCount, psCCode, pnParamCount, psParamNames);
        }

        public long GEMReqPPFmtSend2(string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames, string[] psParamValues)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMReqPPFmtSend2(sPpid, sMdln, sSoftRev, nCount, psCCode, pnParamCount, psParamNames, psParamValues);
        }

        public long GEMRspPPFmtSend(long nMsgId, string sPpid, long nResult)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMRspPPFmtSend(nMsgId, sPpid, nResult);
        }

        public long GEMReqPPFmt(string sPpid)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMReqPPFmt(sPpid);
        }

        public long GEMRspPPFmt(long nMsgId, string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMRspPPFmt(nMsgId, sPpid, sMdln, sSoftRev, nCount, psCCode, pnParamCount, psParamNames);
        }

        public long GEMRspPPFmt2(long nMsgId, string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames, string[] psParamValues)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMRspPPFmt2(nMsgId, sPpid, sMdln, sSoftRev, nCount, psCCode, pnParamCount, psParamNames, psParamValues);
        }

        public long GEMReqPPFmtVerification(string sPpid, long nCount, long[] pnAck, string[] psSeqNumber, string[] psError)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMReqPPFmtVerification(sPpid, nCount, pnAck, psSeqNumber, psError);
        }

        public long GEMSetTerminalMessage(long nTid, string sMsg)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMSetTerminalMessage(nTid, sMsg);
        }

        public long GEMSetVariable(long nCount, long[] pnVid, string[] psValue, int stageNum = -1, bool immediatelyUpdate = false)
        {
            lock(lockObj)
            {
                if (!IsOpened())
                    return -1;
                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                    return Channel.GEMSetVariable(nCount, pnVid, psValue, this.LoaderController().GetChuckIndex(), immediatelyUpdate);
                else
                    return Channel.GEMSetVariable(nCount, pnVid, psValue);
            }
        }

        public long GEMGetVariable(long nCount, ref long[] pnVid, ref string[] psValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMGetVariable(nCount, ref pnVid, ref psValue);
        }

        public long GEMSetVariables(long nObjectID, long nVid, int stageNum = -1, bool immediatelyUpdate = false)
        {
            lock (lockObj)
            {
                if (!IsOpened())
                    return -1;
                //System.Threading.Thread.Sleep(1);
                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                    return Channel.GEMSetVariables(nObjectID, nVid, this.LoaderController().GetChuckIndex(), immediatelyUpdate);
                else
                    return Channel.GEMSetVariables(nObjectID, nVid);
            }
        }

        public long GEMSetVarName(long nCount, string[] psVidName, string[] psValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMSetVarName(nCount, psVidName, psValue);
        }

        public long GEMGetVarName(long nCount, ref string[] psVidName, ref string[] psValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMGetVarName(nCount, ref psVidName, ref psValue);
        }

        public long GEMSetEvent(long nEventID, int stageNum = -1)
        {
            lock(lockObj)
            {
                if (!IsOpened())
                    return -1;
                return Channel.GEMSetEvent(nEventID, stageNum);
            }
        }

        public long GEMSetSpecificMessage(long nObjectID, string sMsgName)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMSetSpecificMessage(nObjectID, sMsgName);
        }

        public long GEMGetSpecificMessage(long nSObjectID, ref long pnRObjectID, string sMsgName)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMGetSpecificMessage(nSObjectID, ref pnRObjectID, sMsgName);
        }

        public long GetAllStringItem(long nObjectID, ref string psValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetAllStringItem(nObjectID, ref psValue);
        }

        public long SetAllStringItem(long nObjectID, string sValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.SetAllStringItem(nObjectID, sValue);
        }

        public long GEMReqPPSendEx(string sPpid, string sRecipePath)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMReqPPSendEx(sPpid, sRecipePath);
        }

        public long GEMRspPPSendEx(long nMsgId, string sPpid, string sRecipePath, long nResult)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMRspPPSendEx(nMsgId, sPpid, sRecipePath, nResult);
        }

        public long GEMReqPPEx(string sPpid, string sRecipePath)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMReqPPEx(sPpid, sRecipePath);
        }

        public long GEMRspPPEx(long nMsgId, string sPpid, string sRecipePath)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMRspPPEx(nMsgId, sPpid, sRecipePath);
        }

        public long GEMSetVariableEx(long nObjectID, long nVid)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMSetVariableEx(nObjectID, nVid);
        }

        public long GEMReqLoopback(long nCount, long[] pnAbs)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMReqLoopback(nCount, pnAbs);
        }

        public long GEMSetEventEnable(long nCount, long[] pnCEIDs, long nEnable)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMSetEventEnable(nCount, pnCEIDs, nEnable);
        }

        public long GEMSetAlarmEnable(long nCount, long[] pnALIDs, long nEnable)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMSetAlarmEnable(nCount, pnALIDs, nEnable);
        }

        public long GEMGetEventEnable(long nCount, long[] pnCEIDs, ref long[] pnEnable)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMGetEventEnable(nCount, pnCEIDs, ref pnEnable);
        }

        public long GEMGetAlarmEnable(long nCount, long[] pnALIDs, ref long[] pnEnable)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMGetAlarmEnable(nCount, pnALIDs, ref pnEnable);
        }

        public long GEMGetAlarmInfo(long nCount, long[] pnALIDs, ref long[] pnALCDs, ref string[] psALTXs)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMGetAlarmInfo(nCount, pnALIDs, ref pnALCDs, ref psALTXs);
        }

        public long GEMGetSVInfo(long nCount, long[] pnSVIDs, ref string[] psMins, ref string[] psMaxs)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMGetSVInfo(nCount, pnSVIDs, ref psMins, ref psMaxs);
        }

        public long GEMGetECVInfo(long nCount, long[] pnECIDs, ref string[] psNames, ref string[] psDefs, ref string[] psMins, ref string[] psMaxs, ref string[] psUnits)
        {

            if (!IsOpened())
                return -1;
            return Channel.GEMGetECVInfo(nCount, pnECIDs, ref psNames, ref psDefs, ref psMins, ref psMaxs, ref psUnits);
        }

        public long GEMRspOffline(long nMsgId, long nAck)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMRspOffline(nMsgId, nAck);
        }

        public long GEMRspOnline(long nMsgId, long nAck)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMRspOnline(nMsgId, nAck);
        }

        public long GEMReqHostOffline()
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMReqHostOffline();
        }

        public long GEMReqStartPolling(string sName, long nScanTime)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMReqStartPolling(sName, nScanTime);
        }

        public long GEMReqStopPolling(string sName)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMReqStopPolling(sName);
        }

        public long GEMEnableLog(long bEnabled)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMEnableLog(bEnabled);
        }

        public long GEMSetLogOption(string sDriectory, string sPrefix, string sExtension, long nKeepDay, long bMakeDailyLog, long bMakeSubDirectory)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMSetLogOption(sDriectory, sPrefix, sExtension, nKeepDay, bMakeDailyLog, bMakeSubDirectory);
        }

        public long LoadMessageRecevieModule(string dllpath, string receivername)
        {
            return -1;
        }

        public bool Are_You_There()
        {
            try
            {
                if (!IsOpened())
                    return false;
                return Channel.IsOpened();
            }
            catch (Exception)
            {
                return false;
            }
        }
        //Add
        public long GetCurrentItemInfo(long nObjectID, ref long pnItemType, ref long pnItemCount)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetCurrentItemInfo(nObjectID, ref pnItemType, ref pnItemCount);
        }
        public long GetAttrData(ref long pnObjectID, long nMsgID, string sAttrName)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetAttrData(ref pnObjectID, nMsgID, sAttrName);
        }
        public long GEMSetEventEx(long nEventID, long nCount, long[] pnVID, string[] psValue)//, List<Dictionary<long, (int objtype, object value)>> ExecutorDataDic = null)
        {
            lock (lockObj)
            {
                if (!IsOpened())
                    return -1;
                return Channel.GEMSetEventEx(nEventID, nCount, pnVID, psValue);
            }
        }
        public bool GetActive()
        {
            if (!IsOpened())
                return false;
            return Channel.GetActive();
        }
        public long GEMRsqOffline(long nMsgId, long nAck)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMRsqOffline(nMsgId, nAck);
        }

        #region GEM300Pro 
        public long CJDelAllJobInfo()
        {
            if (!IsOpened())
                return -1;
            return Channel.CJDelAllJobInfo();
        }

        public long CJDelJobInfo(string sCJobID)
        {
            if (!IsOpened())
                return -1;
            return Channel.CJDelJobInfo(sCJobID);
        }

        public long CJGetAllJobInfo(ref long pnObjID, ref long pnCount)
        {
            if (!IsOpened())
                return -1;
            return Channel.CJGetAllJobInfo(ref pnObjID, ref pnCount);
        }

        public long CJGetHOQJob()
        {
            if (!IsOpened())
                return -1;
            return Channel.CJGetHOQJob();
        }

        public long CJReqCommand(string sCJobID, long nCommand, string sCPName, string sCPVal)
        {
            if (!IsOpened())
                return -1;
            return Channel.CJReqCommand(sCJobID, nCommand, sCPName, sCPVal);
        }

        public long CJReqCreate(string sCJobID, long nStartMethod, long nCountPRJob, string[] psPRJobID)
        {
            if (!IsOpened())
                return -1;
            return Channel.CJReqCreate(sCJobID, nStartMethod, nCountPRJob, psPRJobID);
        }

        public long CJReqGetAllJobID()
        {
            if (!IsOpened())
                return -1;
            return Channel.CJReqGetAllJobID();
        }

        public long CJReqGetJob(string sCJobID)
        {
            if (!IsOpened())
                return -1;
            return Channel.CJReqGetJob(sCJobID);
        }

        public long CJReqHOQJob(string sCJobID)
        {
            if (!IsOpened())
                return -1;
            return Channel.CJReqHOQJob(sCJobID);
        }

        public long CJReqSelect(string sCJobID)
        {
            if (!IsOpened())
                return -1;
            return Channel.CJReqSelect(sCJobID);
        }

        public long CJRspCommand(long nMsgId, string sCJobID, long nCommand, long nResult, long nErrCode, string sErrText)
        {
            if (!IsOpened())
                return -1;
            return Channel.CJRspCommand(nMsgId, sCJobID, nCommand, nResult, nErrCode, sErrText);
        }

        public long CJRspVerify(long nMsgId, string sCJobID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (!IsOpened())
                return -1;
            return Channel.CJRspVerify(nMsgId, sCJobID, nResult, nErrCount, pnErrCode, psErrText);
        }

        public long CJSetJobInfo(string sCJobID, long nState, long nStartMethod, long nCountPRJob, string[] psPRJobID)
        {
            if (!IsOpened())
                return -1;
            return Channel.CJSetJobInfo(sCJobID, nState, nStartMethod, nCountPRJob, psPRJobID);
        }

        public long CloseGEMObject(long nMsgID)
        {
            if (!IsOpened())
                return -1;
            return Channel.CloseGEMObject(nMsgID);
        }

        public long CMSDelAllCarrierInfo()
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSDelAllCarrierInfo();
        }

        public long CMSDelCarrierInfo(string sCarrierID)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSDelCarrierInfo(sCarrierID);
        }

        public long CMSGetAllCarrierInfo(ref long pnMsgId, ref long pnCount)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSGetAllCarrierInfo(ref pnMsgId, ref pnCount);
        }

        public long CMSReqBind(string sLocID, string sCarrierID, string sSlotMap)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSReqBind(sLocID, sCarrierID, sSlotMap);
        }

        public long CMSReqCancelBind(string sLocID, string sCarrierID)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSReqCancelBind(sLocID, sCarrierID);
        }

        public long CMSReqCancelCarrier(string sLocID, string sCarrierID)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSReqCancelCarrier(sLocID, sCarrierID);
        }

        public long CMSReqCarrierIn(string sLocID, string sCarrierID)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSReqCarrierIn(sLocID, sCarrierID);
        }

        public long CMSReqCarrierOut(string sLocID, string sCarrierID)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSReqCarrierOut(sLocID, sCarrierID);
        }

        public long CMSReqCarrierReCreate(string sLocID, string sCarrierID)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSReqCarrierReCreate(sLocID, sCarrierID);
        }

        public long CMSReqChangeAccess(long nMode, string sLocID)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSReqChangeAccess(nMode, sLocID);
        }

        public long CMSReqChangeServiceStatus(string sLocID, long nState)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSReqChangeServiceStatus(sLocID, nState);
        }

        public long CMSReqProceedCarrier(string sLocID, string sCarrierID, string sSlotMap, long nCount, string[] psLotID, string[] psSubstrateID, string sUsage)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSReqProceedCarrier(sLocID, sCarrierID, sSlotMap, nCount, psLotID, psSubstrateID, sUsage);
        }

        public long CMSRspCancelCarrier(long nMsgId, string sLocID, string sCarrierID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSRspCancelCarrier(nMsgId, sLocID, sCarrierID, nResult, nErrCount, pnErrCode, psErrText);
        }

        public long CMSRspCancelCarrierAtPort(long nMsgId, string sLocID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSRspCancelCarrierAtPort(nMsgId, sLocID, nResult, nErrCount, pnErrCode, psErrText);
        }

        public long CMSRspCancelCarrierOut(long nMsgId, string sCarrierID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSRspCancelCarrierOut(nMsgId, sCarrierID, nResult, nErrCount, pnErrCode, psErrText);
        }

        public long CMSRspCarrierIn(long nMsgId, string sLocID, string sCarrierID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSRspCarrierIn(nMsgId, sLocID, sCarrierID, nResult, nErrCount, pnErrCode, psErrText);
        }

        public long CMSRspCarrierOut(long nMsgId, string sLocID, string sCarrierID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSRspCarrierOut(nMsgId, sLocID, sCarrierID, nResult, nErrCount, pnErrCode, psErrText);
        }

        public long CMSRspCarrierRelease(long nMsgId, string sLocID, string sCarrierID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSRspCarrierRelease(nMsgId, sLocID, sCarrierID, nResult, nErrCount, pnErrCode, psErrText);
        }

        public long CMSRspCarrierTagReadData(long nMsgId, string sLocID, string sCarrierID, string sData, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSRspCarrierTagReadData(nMsgId, sLocID, sCarrierID, sData, nResult, nErrCount, pnErrCode, psErrText);
        }

        public long CMSRspCarrierTagWriteData(long nMsgId, string sLocID, string sCarrierID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSRspCarrierTagWriteData(nMsgId, sLocID, sCarrierID, nResult, nErrCount, pnErrCode, psErrText);
        }

        public long CMSRspChangeAccess(long nMsgId, long nMode, long nResult, long nErrCount, string[] psLocID, long[] pnErrCode, string[] psErrText)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSRspChangeAccess(nMsgId, nMode, nResult, nErrCount, psLocID, pnErrCode, psErrText);
        }

        public long CMSRspChangeServiceStatus(long nMsgId, string sLocID, long nState, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSRspChangeServiceStatus(nMsgId, sLocID, nState, nResult, nErrCount, pnErrCode, psErrText);
        }

        public long CMSSetBufferCapacityChanged(string sPartID, string sPartType, long nAPPCapacity, long nPCapacity, long nUnPCapacity)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSSetBufferCapacityChanged(sPartID, sPartType, nAPPCapacity, nPCapacity, nUnPCapacity);
        }

        public long CMSSetCarrierAccessing(string sLocID, long nState, string sCarrierID)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSSetCarrierAccessing(sLocID, nState, sCarrierID);
        }

        public long CMSSetCarrierID(string sLocID, string sCarrierID, long nResult)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSSetCarrierID(sLocID, sCarrierID, nResult);
        }

        public long CMSSetCarrierIDStatus(string sCarrierID, long nState)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSSetCarrierIDStatus(sCarrierID, nState);
        }

        public long CMSSetCarrierInfo(string sCarrierID, string sLocID, long nIdStatus, long nSlotMapStatus, long nAccessingStatus, string sSlotMap, long nContentsMapCount, string[] psLotID, string[] psSubstrateID, string sUsage)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSSetCarrierInfo(sCarrierID, sLocID, nIdStatus, nSlotMapStatus, nAccessingStatus, sSlotMap, nContentsMapCount, psLotID, psSubstrateID, sUsage);
        }

        public long CMSSetCarrierLocationInfo(string sLocID, string sCarrierID)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSSetCarrierLocationInfo(sLocID, sCarrierID);
        }

        public long CMSSetCarrierMovement(string sLocID, string sCarrierID)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSSetCarrierMovement(sLocID, sCarrierID);
        }

        public long CMSSetCarrierOnOff(string sLocID, long nState)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSSetCarrierOnOff(sLocID, nState);
        }

        public long CMSSetCarrierOutStart(string sLocID, string sCarrierID)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSSetCarrierOutStart(sLocID, sCarrierID);
        }

        public long CMSSetLPInfo(string sLocID, long nTransferState, long nAccessMode, long nReservationState, long nAssociationState, string sCarrierID)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSSetLPInfo(sLocID, nTransferState, nAccessMode, nReservationState, nAssociationState, sCarrierID);
        }

        public long CMSSetMaterialArrived(string sMaterialID)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSSetMaterialArrived(sMaterialID);
        }

        public long CMSSetPIOSignalState(string sLocID, long nSignal, long nState)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSSetPIOSignalState(sLocID, nSignal, nState);
        }

        public long CMSSetPresenceSensor(string sLocID, long nState)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSSetPresenceSensor(sLocID, nState);
        }

        public long CMSSetReadyToLoad(string sLocID)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSSetReadyToLoad(sLocID);
        }

        public long CMSSetReadyToUnload(string sLocID)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSSetReadyToUnload(sLocID);
        }

        public long CMSSetSlotMap(string sLocID, string sSlotMap, string sCarrierID, long nResult)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSSetSlotMap(sLocID, sSlotMap, sCarrierID, nResult);
        }

        public long CMSSetSlotMapStatus(string sCarrierID, long nState)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSSetSlotMapStatus(sCarrierID, nState);
        }

        public long CMSSetSubstrateCount(string sCarrierID, long nSubstCount)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSSetSubstrateCount(sCarrierID, nSubstCount);
        }

        public long CMSSetTransferReady(string sLocID, long nState)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSSetTransferReady(sLocID, nState);
        }

        public long CMSSetUsage(string sCarrierID, string sUsage)
        {
            if (!IsOpened())
                return -1;
            return Channel.CMSSetUsage(sCarrierID, sUsage);
        }

        public long GEMGetVariables(ref long pnObjectID, long nVid)
        {
            if (!IsOpened())
                return -1;
            return Channel.GEMGetVariables(ref pnObjectID, nVid);
        }

        public long GetCarrierAccessingStatus(long nMsgId, long nIndex, ref long pnState)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetCarrierAccessingStatus(nMsgId, nIndex, ref pnState);
        }

        public long GetCarrierClose(long nMsgId)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetCarrierClose(nMsgId);
        }

        public long GetCarrierContentsMap(long nMsgId, long nIndex, long nCount, ref string[] psLotID, ref string[] psSubstrateID)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetCarrierContentsMap(nMsgId, nIndex, nCount, ref psLotID, ref psSubstrateID);
        }

        public long GetCarrierContentsMapCount(long nMsgId, long nIndex, ref long pnCount)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetCarrierContentsMapCount(nMsgId, nIndex, ref pnCount);
        }

        public long GetCarrierID(long nMsgId, long nIndex, ref string psCarrierID)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetCarrierID(nMsgId, nIndex, ref psCarrierID);
        }

        public long GetCarrierIDStatus(long nMsgId, long nIndex, ref long pnState)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetCarrierIDStatus(nMsgId, nIndex, ref pnState);
        }

        public long GetCarrierLocID(long nMsgId, long nIndex, ref string psLocID)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetCarrierLocID(nMsgId, nIndex, ref psLocID);
        }

        public long GetCarrierSlotMap(long nMsgId, long nIndex, ref string psSlotMap)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetCarrierSlotMap(nMsgId, nIndex, ref psSlotMap);
        }

        public long GetCarrierSlotMapStatus(long nMsgId, long nIndex, ref long pnState)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetCarrierSlotMapStatus(nMsgId, nIndex, ref pnState);
        }

        public long GetCarrierUsage(long nMsgId, long nIndex, ref string psUsage)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetCarrierUsage(nMsgId, nIndex, ref psUsage);
        }

        public long GetCtrlJobClose(long nObjID)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetCtrlJobClose(nObjID);
        }

        public long GetCtrlJobID(long nObjID, long nIndex, ref string psCtrlJobID)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetCtrlJobID(nObjID, nIndex, ref psCtrlJobID);
        }

        public long GetCtrlJobPRJobCount(long nObjID, long nIndex, ref long pnCount)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetCtrlJobPRJobCount(nObjID, nIndex, ref pnCount);
        }

        public long GetCtrlJobPRJobIDs(long nObjID, long nIndex, long nCount, ref string[] psPRJobIDs)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetCtrlJobPRJobIDs(nObjID, nIndex, nCount, ref psPRJobIDs);
        }

        public long GetCtrlJobStartMethod(long nObjID, long nIndex, ref long pnAutoStart)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetCtrlJobStartMethod(nObjID, nIndex, ref pnAutoStart);
        }

        public long GetCtrlJobState(long nObjID, long nIndex, ref long pnState)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetCtrlJobState(nObjID, nIndex, ref pnState);
        }

        public string GetIP()
        {
            if (!IsOpened())
                return "";
            return Channel.GetIP();
        }

        public long GetPort()
        {
            if (!IsOpened())
                return -1;
            return Channel.GetPort();
        }

        public long GetPRJobAutoStart(long nObjID, long nIndex, ref long pnAutoStart)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetPRJobAutoStart(nObjID, nIndex, ref pnAutoStart);
        }

        public long GetPRJobCarrier(long nObjID, long nIndex, long nCount, ref string[] psCarrierID, ref string[] psSlotInfo)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetPRJobCarrier(nObjID, nIndex, nCount, ref psCarrierID, ref psSlotInfo);
        }

        public long GetPRJobCarrierCount(long nObjID, long nIndex, ref long pnCount)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetPRJobCarrierCount(nObjID, nIndex, ref pnCount);
        }

        public long GetPRJobClose(long nObjID)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetPRJobClose(nObjID);
        }

        public long GetPRJobID(long nObjID, long nIndex, ref string psPJobID)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetPRJobID(nObjID, nIndex, ref psPJobID);
        }

        public long GetPRJobMtrlFormat(long nObjID, long nIndex, ref long pnMtrlFormat)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetPRJobMtrlFormat(nObjID, nIndex, ref pnMtrlFormat);
        }

        public long GetPRJobMtrlOrder(long nObjID, long nIndex, ref long pnMtrlOrder)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetPRJobMtrlOrder(nObjID, nIndex, ref pnMtrlOrder);
        }

        public long GetPRJobRcpID(long nObjID, long nIndex, ref string psRcpID)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetPRJobRcpID(nObjID, nIndex, ref psRcpID);
        }

        public long GetPRJobRcpParam(long nObjID, long nIndex, long nCount, ref string[] psRcpParName, ref string[] psRcpParValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetPRJobRcpParam(nObjID, nIndex, nCount, ref psRcpParName, ref psRcpParValue);
        }

        public long GetPRJobRcpParamEx(long nObjID, long nIndex, long nCount, ref string[] psRcpParName, ref long[] pnRcpParValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetPRJobRcpParamEx(nObjID, nIndex, nCount, ref psRcpParName, ref pnRcpParValue);
        }

        public long GetPRJobRcpParamCount(long nObjID, long nIndex, ref long pnCount)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetPRJobRcpParamCount(nObjID, nIndex, ref pnCount);
        }

        public long GetPRJobState(long nObjID, long nIndex, ref long pnState)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetPRJobState(nObjID, nIndex, ref pnState);
        }

        public long GetSubstrateClose(long nObjID)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetSubstrateClose(nObjID);
        }

        public long GetSubstrateID(long nObjID, long nIndex, ref string psSubstrateID)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetSubstrateID(nObjID, nIndex, ref psSubstrateID);
        }

        public long GetSubstrateLocID(long nObjID, long nIndex, ref string psSubstLocID)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetSubstrateLocID(nObjID, nIndex, ref psSubstLocID);
        }

        public long GetSubstrateState(long nObjID, long nIndex, ref long pnTransportState, ref long pnProcessingState, ref long pnReadingState)
        {
            if (!IsOpened())
                return -1;
            return Channel.GetSubstrateState(nObjID, nIndex, ref pnTransportState, ref pnProcessingState, ref pnReadingState);
        }

        public long Initialize(string sCfg)
        {
            if (!IsOpened())
                return -1;
            return Channel.Initialize(sCfg);
        }

        public long OpenGEMObject(ref long pnMsgID, string sObjType, string sObjID)
        {
            if (!IsOpened())
                return -1;
            return Channel.OpenGEMObject(ref pnMsgID, sObjType, sObjID);
        }

        public long PJDelAllJobInfo()
        {
            if (!IsOpened())
                return -1;
            return Channel.PJDelAllJobInfo();
        }

        public long PJDelJobInfo(string sPJobID)
        {
            if (!IsOpened())
                return -1;
            return Channel.PJDelJobInfo(sPJobID);
        }

        public long PJGetAllJobInfo(ref long pnObjID, ref long pnPJobCount)
        {
            if (!IsOpened())
                return -1;
            return Channel.PJGetAllJobInfo(ref pnObjID, ref pnPJobCount);
        }

        public long PJReqCommand(long nCommand, string sPJobID)
        {
            if (!IsOpened())
                return -1;
            return Channel.PJReqCommand(nCommand, sPJobID);
        }

        public long PJReqCreate(string sPJobID, long nMtrlFormat, long nAutoStart, long nMtrlOrder, long nMtrlCount, string[] psMtrlID, string[] psSlotInfo, long nRcpMethod, string sRcpID, long nRcpParCount, string[] psRcpParName, string[] psRcpParValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.PJReqCreate(sPJobID, nMtrlFormat, nAutoStart, nMtrlOrder, nMtrlCount, psMtrlID, psSlotInfo, nRcpMethod, sRcpID, nRcpParCount, psRcpParName, psRcpParValue);
        }

        public long PJReqCreateEx(string sPJobID, long nMtrlFormat, long nAutoStart, long nMtrlOrder, long nMtrlCount, string[] psMtrlID, string[] psSlotInfo, long nRcpMethod, string sRcpID, long nRcpParCount, string[] psRcpParName, long[] pnRcpParValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.PJReqCreateEx(sPJobID, nMtrlFormat, nAutoStart, nMtrlOrder, nMtrlCount, psMtrlID, psSlotInfo, nRcpMethod, sRcpID, nRcpParCount, psRcpParName, pnRcpParValue);
        }

        public long PJReqGetAllJobID()
        {
            if (!IsOpened())
                return -1;
            return Channel.PJReqGetAllJobID();
        }

        public long PJReqGetJob(string sPJobID)
        {
            if (!IsOpened())
                return -1;
            return Channel.PJReqGetJob(sPJobID);
        }

        public long PJRspCommand(long nMsgID, long nCommand, string sPJobID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (!IsOpened())
                return -1;
            return Channel.PJRspCommand(nMsgID, nCommand, sPJobID, nResult, nErrCount, pnErrCode, psErrText);
        }

        public long PJRspSetMtrlOrder(long nMsgID, long nResult)
        {
            if (!IsOpened())
                return -1;
            return Channel.PJRspSetMtrlOrder(nMsgID, nResult);
        }

        public long PJRspSetRcpVariable(long nMsgID, string sPJobID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (!IsOpened())
                return -1;
            return Channel.PJRspSetRcpVariable(nMsgID, sPJobID, nResult, nErrCount, pnErrCode, psErrText);
        }

        public long PJRspSetStartMethod(long nMsgID, long nPJobCount, string[] psPJobID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (!IsOpened())
                return -1;
            return Channel.PJRspSetStartMethod(nMsgID, nPJobCount, psPJobID, nResult, nErrCount, pnErrCode, psErrText);
        }

        public long PJRspVerify(long nMsgID, long nPJobCount, string[] psPJobID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (!IsOpened())
                return -1;
            return Channel.PJRspVerify(nMsgID, nPJobCount, psPJobID, nResult, nErrCount, pnErrCode, psErrText);
        }

        public long PJSetJobInfo(string sPJobID, long nMtrlFormat, long nAutoStart, long nMtrlOrder, long nMtrlCount, string[] psMtrlID, string[] psSlotInfo, long nRcpMethod, string sRcpID, long nRcpParCount, string[] psRcpParName, string[] psRcpParVal)
        {
            if (!IsOpened())
                return -1;
            return Channel.PJSetJobInfo(sPJobID, nMtrlFormat, nAutoStart, nMtrlOrder, nMtrlCount, psMtrlID, psSlotInfo, nRcpMethod, sRcpID, nRcpParCount, psRcpParName, psRcpParVal);
        }

        public long PJSetJobInfoEx(string sPJobID, long nMtrlFormat, long nAutoStart, long nMtrlOrder, long nMtrlCount, string[] psMtrlID, string[] psSlotInfo, long nRcpMethod, string sRcpID, long nRcpParCount, string[] psRcpParName, long[] pnRcpParVal)
        {
            if (!IsOpened())
                return -1;
            return Channel.PJSetJobInfoEx(sPJobID, nMtrlFormat, nAutoStart, nMtrlOrder, nMtrlCount, psMtrlID, psSlotInfo, nRcpMethod, sRcpID, nRcpParCount, psRcpParName, pnRcpParVal);
        }

        public long PJSetState(string sPJobID, long nState)
        {
            if (!IsOpened())
                return -1;
            return Channel.PJSetState(sPJobID, nState);
        }

        public long PJSettingUpCompt(string sPJobID)
        {
            if (!IsOpened())
                return -1;
            return Channel.PJSettingUpCompt(sPJobID);
        }

        public long PJSettingUpStart(string sPJobID)
        {
            if (!IsOpened())
                return -1;
            return Channel.PJSettingUpStart(sPJobID);
        }

        public long RangeCheck(long[] pnIds)
        {
            if (!IsOpened())
                return -1;
            return Channel.RangeCheck(pnIds);
        }

        public bool ReadXGemInfoFromRegistry()
        {
            if (!IsOpened())
                return false;
            return Channel.ReadXGemInfoFromRegistry();
        }

        public int RunProcess(string sName, string sCfgPath, string sPassword)
        {
            if (!IsOpened())
                return -1;
            return Channel.RunProcess(sName, sCfgPath, sPassword);
        }

        public long SendUserMessage(long nObjectID, string sCommand, long nTransID)
        {
            if (!IsOpened())
                return -1;
            return Channel.SendUserMessage(nObjectID, sCommand, nTransID);
        }

        public void SetActive(bool bNewValue)
        {
            if (!IsOpened())
                return;
            Channel.SetActive(bNewValue);
        }

        public long SetBinaryItems(long nObjectID, byte[] pnValue)
        {
            if (!IsOpened())
                return -1;
            return Channel.SetBinaryItems(nObjectID, pnValue);
        }

        public void SetIP(string sNewValue)
        {
            if (!IsOpened())
                return;
            Channel.SetIP(sNewValue);
        }

        public void SetPort(long nNewValue)
        {
            if (!IsOpened())
                return;
            Channel.SetPort(nNewValue);
        }

        public long STSDelAllSubstrateInfo()
        {
            if (!IsOpened())
                return -1;
            return Channel.STSDelAllSubstrateInfo();
        }

        public long STSDelSubstrateInfo(string sSubstrateID)
        {
            if (!IsOpened())
                return -1;
            return Channel.STSDelSubstrateInfo(sSubstrateID);
        }

        public long STSGetAllSubstrateInfo(ref long pnObjID, ref long pnCount)
        {
            if (!IsOpened())
                return -1;
            return Channel.STSGetAllSubstrateInfo(ref pnObjID, ref pnCount);
        }

        public long STSReqCancelSubstrate(string sSubstLocID, string sSubstrateID)
        {
            if (!IsOpened())
                return -1;
            return Channel.STSReqCancelSubstrate(sSubstLocID, sSubstrateID);
        }

        public long STSReqCreateSubstrate(string sSubstLocID, string sSubstrateID)
        {
            if (!IsOpened())
                return -1;
            return Channel.STSReqCreateSubstrate(sSubstLocID, sSubstrateID);
        }

        public long STSReqDeleteSubstrate(string sSubstLocID, string sSubstrateID)
        {
            if (!IsOpened())
                return -1;
            return Channel.STSReqDeleteSubstrate(sSubstLocID, sSubstrateID);
        }

        public long STSReqProceedSubstrate(string sSubstLocID, string sSubstrateID, string sReadSubstID)
        {
            if (!IsOpened())
                return -1;
            return Channel.STSReqProceedSubstrate(sSubstLocID, sSubstrateID, sReadSubstID);
        }

        public long STSRspCancelSubstrate(long nMsgID, string sSubstLocID, string sSubstrateID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (!IsOpened())
                return -1;
            return Channel.STSRspCancelSubstrate(nMsgID, sSubstLocID, sSubstrateID, nResult, nErrCount, pnErrCode, psErrText);
        }

        public long STSRspCreateSubstrate(long nMsgID, string sSubstLocID, string sSubstrateID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (!IsOpened())
                return -1;
            return Channel.STSRspCreateSubstrate(nMsgID, sSubstLocID, sSubstrateID, nResult, nErrCount, pnErrCode, psErrText);
        }

        public long STSRspDeleteSubstrate(long nMsgID, string sSubstLocID, string sSubstrateID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (!IsOpened())
                return -1;
            return Channel.STSRspDeleteSubstrate(nMsgID, sSubstLocID, sSubstrateID, nResult, nErrCount, pnErrCode, psErrText);
        }

        public long STSRspUpdateSubstrate(long nMsgID, string sSubstLocID, string sSubstrateID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (!IsOpened())
                return -1;
            return Channel.STSRspUpdateSubstrate(nMsgID, sSubstLocID, sSubstrateID, nResult, nErrCount, pnErrCode, psErrText);
        }

        public long STSSetBatchLocationInfo(string sBatchLocID, string sSubstrateID)
        {
            if (!IsOpened())
                return -1;
            return Channel.STSSetBatchLocationInfo(sBatchLocID, sSubstrateID);
        }

        public long STSSetBatchProcessing(long nCount, string[] psSubstLocID, string[] psSubstrateID, long nState)
        {
            if (!IsOpened())
                return -1;
            return Channel.STSSetBatchProcessing(nCount, psSubstLocID, psSubstrateID, nState);
        }

        public long STSSetBatchTransport(long nCount, string[] psSubstLocID, string[] psSubstrateID, long nState)
        {
            if (!IsOpened())
                return -1;
            return Channel.STSSetBatchTransport(nCount, psSubstLocID, psSubstrateID, nState);
        }

        public long STSSetMaterialArrived(string sMaterialID)
        {
            if (!IsOpened())
                return -1;
            return Channel.STSSetMaterialArrived(sMaterialID);
        }

        public long STSSetProcessing(string sSubstLocID, string sSubstrateID, long nState)
        {
            if (!IsOpened())
                return -1;
            return Channel.STSSetProcessing(sSubstLocID, sSubstrateID, nState);
        }

        public long STSSetSubstLocationInfo(string sSubstLocID, string sSubstrateID)
        {
            if (!IsOpened())
                return -1;
            return Channel.STSSetSubstLocationInfo(sSubstLocID, sSubstrateID);
        }

        public long STSSetSubstrateID(string sSubstLocID, string sSubstrateID, string sSubstReadID, long nResult)
        {
            if (!IsOpened())
                return -1;
            return Channel.STSSetSubstrateID(sSubstLocID, sSubstrateID, sSubstReadID, nResult);
        }

        public long STSSetSubstrateInfo(string sSubstLocID, string sSubstrateID, long nTransportState, long nProcessingState, long nReadingState)
        {
            if (!IsOpened())
                return -1;
            return Channel.STSSetSubstrateInfo(sSubstLocID, sSubstrateID, nTransportState, nProcessingState, nReadingState);
        }

        public long STSSetTransport(string sSubstLocID, string sSubstrateID, long nState)
        {
            if (!IsOpened())
                return -1;
            return Channel.STSSetTransport(sSubstLocID, sSubstrateID, nState);
        }
        #endregion
    }

}

