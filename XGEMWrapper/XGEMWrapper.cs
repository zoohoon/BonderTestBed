using GEM_XGem;
using GEM_XGem300Pro;
using SecsGemServiceInterface;
using System.ServiceModel;
using System;
using System.Collections.Generic;
using System.Text;


namespace XGEMWrapper
{
    public interface ISecsGemServiceProxy : ISecsGemService
    {
        CommunicationState GetState();
    }
    [ServiceContract(CallbackContract = typeof(ISecsGemServiceCallback))]
    public interface ISecsGemService
    {
        [OperationContract]
        long Init_SECSGEM(String Config);
        [OperationContract]
        long Close_SECSGEM(int proberId = 0);
        [OperationContract]
        long Start();
        [OperationContract]
        long Stop();
        [OperationContract]
        void ServerConnect(int proberId = 0);
        [OperationContract]
        bool IsOpened();
        [OperationContract]
        long LoadMessageRecevieModule(string dllpath, string receivername);

        #region XGem/XGemPro Default Function
        [OperationContract]
        long MakeObject(ref long nObjectID);
        [OperationContract]
        long SetListItem(long nObjectID, long nItemCount);
        [OperationContract]
        long SetBinaryItem(long nObjectID, byte nValue);
        [OperationContract]
        long SetBinaryItems(long nObjectID, byte[] pnValue);
        [OperationContract]
        long SetBoolItem(long nObjectID, bool nValue);
        [OperationContract]
        long SetBoolItems(long nObjectID, bool[] pnValue);
        [OperationContract]
        long SetUint1Item(long nObjectID, byte nValue);
        [OperationContract]
        long SetUint1Items(long nObjectID, byte[] pnValue);
        [OperationContract]
        long SetUint2Item(long nObjectID, ushort nValue);
        [OperationContract]
        long SetUint2Items(long nObjectID, ushort[] pnValue);
        [OperationContract]
        long SetUint4Item(long nObjectID, uint nValue);
        [OperationContract]
        long SetUint4Items(long nObjectID, uint[] pnValue);
        [OperationContract]
        long SetUint8Item(long nObjectID, ulong nValue);
        [OperationContract]
        long SetUint8Items(long nObjectID, ulong[] pnValue);
        [OperationContract]
        long SetInt1Item(long nObjectID, sbyte nValue);
        [OperationContract]
        long SetInt1Items(long nObjectID, sbyte[] pnValue);
        [OperationContract]
        long SetInt2Item(long nObjectID, short nValue);
        [OperationContract]
        long SetInt2Items(long nObjectID, short[] pnValue);
        [OperationContract]
        long SetInt4Item(long nObjectID, int nValue);
        [OperationContract]
        long SetInt4Items(long nObjectID, int[] pnValue);
        [OperationContract]
        long SetInt8Item(long nObjectID, long nValue);
        [OperationContract]
        long SetInt8Items(long nObjectID, long[] pnValue);
        [OperationContract]
        long SetFloat4Item(long nObjectID, float nValue);
        [OperationContract]
        long SetFloat4Items(long nObjectID, float[] pnValue);
        [OperationContract]
        long SetFloat8Item(long nObjectID, double nValue);
        [OperationContract]
        long SetFloat8Items(long nObjectID, double[] pnValue);
        [OperationContract]
        long SetStringItem(long nObjectID, string pszValue);
        [OperationContract]
        long GetListItem(long nObjectID, ref long pnItemCount);
        [OperationContract]
        long GetBinaryItem(long nObjectID, ref byte[] pnValue);
        [OperationContract]
        long GetBoolItem(long nObjectID, ref bool[] pnValue);
        [OperationContract]
        long GetUint1Item(long nObjectID, ref byte[] pnValue);
        [OperationContract]
        long GetUint2Item(long nObjectID, ref ushort[] pnValue);
        [OperationContract]
        long GetUint4Item(long nObjectID, ref uint[] pnValue);
        [OperationContract]
        long GetUint8Item(long nObjectID, ref ulong[] pnValue);
        [OperationContract]
        long GetInt1Item(long nObjectID, ref sbyte[] pnValue);
        [OperationContract]
        long GetInt2Item(long nObjectID, ref short[] pnValue);
        [OperationContract]
        long GetInt4Item(long nObjectID, ref int[] pnValue);
        [OperationContract]
        long GetInt8Item(long nObjectID, ref long[] pnValue);
        [OperationContract]
        long GetFloat4Item(long nObjectID, ref float[] pnValue);
        [OperationContract]
        long GetFloat8Item(long nObjectID, ref double[] pnValue);
        [OperationContract]
        long GetStringItem(long nObjectID, ref string psValue);
        [OperationContract]
        long CloseObject(long nObjectID);
        [OperationContract]
        long SendSECSMessage(long nObjectID, long nStream, long nFunction, long nSysbyte);
        [OperationContract]
        long GEMReqOffline();
        [OperationContract]
        long GEMReqLocal();
        [OperationContract]
        long GEMReqRemote();
        [OperationContract]
        long GEMSetEstablish(long bState);
        [OperationContract]
        long GEMSetParam(string sPName, string sPValue);
        [OperationContract]
        long GEMGetParam(string sPName, ref string sPValue);
        [OperationContract]
        long GEMEQInitialized(long nInitType);
        [OperationContract]
        long GEMReqGetDateTime();
        [OperationContract]
        long GEMRspGetDateTime(long nMsgId, string sSystemTime);
        [OperationContract]
        long GEMRspDateTime(long nMsgId, long nResult);
        [OperationContract]
        long GEMSetAlarm(long nID, long nState, int stageNum = 0);
        [OperationContract]
        long GEMRspRemoteCommand(long nMsgId, string sCmd, long nHCAck, long nCount, long[] pnResult);
        [OperationContract]
        long GEMRspRemoteCommand2(long nMsgId, string sCmd, long nHCAck, long nCount, string[] psCpName, long[] pnCpAck);
        [OperationContract]
        long GEMRspChangeECV(long nMsgId, long nResult);
        [OperationContract]
        long GEMSetECVChanged(long nCount, long[] pnEcIds, string[] psEcVals, int stageNum = -1);
        [OperationContract]
        long GEMReqAllECInfo();
        [OperationContract]
        long GEMSetPPChanged(long nMode, string sPpid, long nSize, string sBody);
        [OperationContract]
        long GEMSetPPFmtChanged(long nMode, string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames);
        [OperationContract]
        long GEMReqPPLoadInquire(string sPpid, long nLength);
        [OperationContract]
        long GEMRspPPLoadInquire(long nMsgId, string sPpid, long nResult);
        [OperationContract]
        long GEMReqPPSend(string sPpid, string sBody);
        [OperationContract]
        long GEMRspPPSend(long nMsgId, string sPpid, long nResult);
        [OperationContract]
        long GEMReqPP(string sPpid);
        [OperationContract]
        long GEMRspPP(long nMsgId, string sPpid, string sBody);
        [OperationContract]
        long GEMRspPPDelete(long nMsgId, long nCount, string[] psPpids, long nResult);
        [OperationContract]
        long GEMRspPPList(long nMsgId, long nCount, string[] psPpids);
        [OperationContract]
        long GEMReqPPFmtSend(string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames);
        [OperationContract]
        long GEMReqPPFmtSend2(string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames, string[] psParamValues);
        [OperationContract]
        long GEMRspPPFmtSend(long nMsgId, string sPpid, long nResult);
        [OperationContract]
        long GEMReqPPFmt(string sPpid);
        [OperationContract]
        long GEMRspPPFmt(long nMsgId, string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames);
        [OperationContract]
        long GEMRspPPFmt2(long nMsgId, string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames, string[] psParamValues);
        [OperationContract]
        long GEMReqPPFmtVerification(string sPpid, long nCount, long[] pnAck, string[] psSeqNumber, string[] psError);
        [OperationContract]
        long GEMSetTerminalMessage(long nTid, string sMsg);
        [OperationContract]
        long GEMSetVariable(long nCount, long[] pnVid, string[] psValue, int stageNum = -1, bool immediatelyUpdate = false);
        [OperationContract]
        long GEMGetVariable(long nCount, ref long[] pnVID, ref string[] psValue);
        [OperationContract]
        long GEMSetVariables(long nObjectID, long nVid, int stageNum = -1, bool immediatelyUpdate = false);
        [OperationContract]
        long GEMSetVarName(long nCount, string[] psVidName, string[] psValue);
        [OperationContract]
        long GEMGetVarName(long nCount, ref string[] psVIDName, ref string[] psValue);
        [OperationContract]
        long GEMSetEvent(long nEventID, int stageNum = -1);
        [OperationContract]
        long GEMSetSpecificMessage(long nObjectID, string sMsgName);
        [OperationContract]
        long GEMGetSpecificMessage(long nSObjectID, ref long pnRObjectID, string sMsgName);
        [OperationContract]
        long GetAllStringItem(long nObjectID, ref string psValue);
        [OperationContract]
        long SetAllStringItem(long nObjectID, string sValue);
        [OperationContract]
        long GEMReqPPSendEx(string sPpid, string sRecipePath);
        [OperationContract]
        long GEMRspPPSendEx(long nMsgId, string sPpid, string sRecipePath, long nResult);
        [OperationContract]
        long GEMReqPPEx(string sPpid, string sRecipePath);
        [OperationContract]
        long GEMRspPPEx(long nMsgId, string sPpid, string sRecipePath);
        [OperationContract]
        long GEMSetVariableEx(long nObjectID, long nVid);
        [OperationContract]
        long GEMReqLoopback(long nCount, long[] pnAbs);
        [OperationContract]
        long GEMSetEventEnable(long nCount, long[] pnCEIDs, long nEnable);
        [OperationContract]
        long GEMSetAlarmEnable(long nCount, long[] pnALIDs, long nEnable);
        [OperationContract]
        long GEMGetEventEnable(long nCount, long[] pnCEIDs, ref long[] nEnable);
        [OperationContract]
        long GEMGetAlarmEnable(long nCount, long[] pnALIDs, ref long[] nEnable);
        [OperationContract]
        long GEMGetAlarmInfo(long nCount, long[] pnALIDs, ref long[] pnALCDs, ref string[] psALTXs);
        [OperationContract]
        long GEMGetSVInfo(long nCount, long[] pnVIDs, ref string[] psMins, ref string[] psMaxs);
        [OperationContract]
        long GEMGetECVInfo(long nCount, long[] pnECIDs, ref string[] psNames, ref string[] psDefs, ref string[] psMins, ref string[] psMaxs, ref string[] psUnits);
        [OperationContract]
        long GEMRsqOffline(long nMsgId, long nAck);
        [OperationContract]
        long GEMRspOnline(long nMsgId, long nAck);
        [OperationContract]
        long GEMReqHostOffline();
        [OperationContract]
        long GEMReqStartPolling(string sName, long nScanTime);
        [OperationContract]
        long GEMReqStopPolling(string sName);
        [OperationContract]
        long GEMEnableLog(long bEnabled);
        [OperationContract]
        long GEMSetLogOption(string sDriectory, string sPrefix, string sExtension, long nKeepDay, long bMakeDailyLog, long bMakeSubDirectory);
        [OperationContract]
        long ClearAlarmOnly(int stageNum = 0);
        #endregion

        #region XGem300Pro Default Function
        [OperationContract]
        long GEMGetVariables(ref long pnObjectID, long nVid);
        [OperationContract]
        long GEMSetEventEx(long nEventID, long nCount, long[] pnVID, string[] psValue);
        [OperationContract]
        long GEMRspOffline(long nMsgId, long nAck);
        [OperationContract]
        long SendUserMessage(long nObjectID, string sCommand, long nTransID);
        [OperationContract]
        long GetCurrentItemInfo(long nObjectID, ref long pnItemType, ref long pnItemCount);
        [OperationContract]
        long CMSSetCarrierLocationInfo(string sLocID, string sCarrierID);
        [OperationContract]
        long CMSSetPresenceSensor(string sLocID, long nState);
        [OperationContract]
        long CMSSetCarrierOnOff(string sLocID, long nState);
        [OperationContract]
        long CMSSetPIOSignalState(string sLocID, long nSignal, long nState);
        [OperationContract]
        long CMSSetReadyToLoad(string sLocID);
        [OperationContract]
        long CMSSetReadyToUnload(string sLocID);
        [OperationContract]
        long CMSSetCarrierMovement(string sLocID, string sCarrierID);
        [OperationContract]
        long CMSSetCarrierAccessing(string sLocID, long nState, string sCarrierID);
        [OperationContract]
        long CMSSetCarrierID(string sLocID, string sCarrierID, long nResult);
        [OperationContract]
        long CMSSetSlotMap(string sLocID, string sSlotMap, string sCarrierID, long nResult);
        [OperationContract]
        long CMSReqBind(string sLocID, string sCarrierID, string sSlotMap);
        [OperationContract]
        long CMSReqCancelBind(string sLocID, string sCarrierID);
        [OperationContract]
        long CMSReqCancelCarrier(string sLocID, string sCarrierID);
        [OperationContract]
        long CMSRspCancelCarrier(long nMsgId, string sLocID, string sCarrierID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText);
        [OperationContract]
        long CMSRspCancelCarrierAtPort(long nMsgId, string sLocID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText);
        [OperationContract]
        long CMSReqCarrierIn(string sLocID, string sCarrierID);
        [OperationContract]
        long CMSReqProceedCarrier(string sLocID, string sCarrierID, string sSlotMap, long nCount, string[] psLotID, string[] psSubstrateID, string sUsage);
        long CMSReqChangeServiceStatus(string sLocID, long nState);
        [OperationContract]
        long CMSReqCarrierOut(string sLocID, string sCarrierID);
        [OperationContract]
        long CMSRspCarrierOut(long nMsgId, string sLocID, string sCarrierID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText);
        [OperationContract]
        long CMSReqChangeAccess(long nMode, string sLocID);
        [OperationContract]
        long CMSRspChangeAccess(long nMsgId, long nMode, long nResult, long nErrCount, string[] psLocID, long[] pnErrCode, string[] psErrText);
        [OperationContract]
        long CMSRspChangeServiceStatus(long nMsgId, string sLocID, long nState, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText);
        [OperationContract]
        long CMSGetAllCarrierInfo(ref long pnMsgId, ref long pnCount);
        [OperationContract]
        long CMSDelCarrierInfo(string sCarrierID);
        [OperationContract]
        long GetCarrierID(long nMsgId, long nIndex, ref string psCarrierID);
        [OperationContract]
        long GetCarrierLocID(long nMsgId, long nIndex, ref string psLocID);
        [OperationContract]
        long GetCarrierIDStatus(long nMsgId, long nIndex, ref long pnState);
        [OperationContract]
        long GetCarrierSlotMapStatus(long nMsgId, long nIndex, ref long pnState);
        [OperationContract]
        long GetCarrierAccessingStatus(long nMsgId, long nIndex, ref long pnState);
        [OperationContract]
        long GetCarrierContentsMapCount(long nMsgId, long nIndex, ref long pnCount);
        [OperationContract]
        long GetCarrierContentsMap(long nMsgId, long nIndex, long nCount, ref string[] psLotID, ref string[] psSubstrateID);
        [OperationContract]
        long GetCarrierSlotMap(long nMsgId, long nIndex, ref string psSlotMap);
        [OperationContract]
        long GetCarrierUsage(long nMsgId, long nIndex, ref string psUsage);
        [OperationContract]
        long GetCarrierClose(long nMsgId);
        [OperationContract]
        long CMSDelAllCarrierInfo();
        [OperationContract]
        long CMSSetCarrierInfo(string sCarrierID, string sLocID, long nIdStatus, long nSlotMapStatus, long nAccessingStatus, string sSlotMap, long nContentsMapCount, string[] psLotID, string[] psSubstrateID, string sUsage);
        [OperationContract]
        long CMSRspCarrierTagReadData(long nMsgId, string sLocID, string sCarrierID, string sData, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText);
        [OperationContract]
        long CMSRspCarrierTagWriteData(long nMsgId, string sLocID, string sCarrierID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText);
        [OperationContract]
        long CMSRspCarrierRelease(long nMsgId, string sLocID, string sCarrierID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText);
        [OperationContract]
        long CMSSetBufferCapacityChanged(string sPartID, string sPartType, long nAPPCapacity, long nPCapacity, long nUnPCapacity);
        [OperationContract]
        long CMSRspCarrierIn(long nMsgId, string sLocID, string sCarrierID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText);
        [OperationContract]
        long CMSReqCarrierReCreate(string sLocID, string sCarrierID);
        [OperationContract]
        long CMSSetMaterialArrived(string sMaterialID);
        [OperationContract]
        long CMSSetCarrierIDStatus(string sCarrierID, long nState);
        [OperationContract]
        long CMSSetSlotMapStatus(string sCarrierID, long nState);
        [OperationContract]
        long CMSSetLPInfo(string sLocID, long nTransferState, long nAccessMode, long nReservationState, long nAssociationState, string sCarrierID);
        [OperationContract]
        long CMSSetSubstrateCount(string sCarrierID, long nSubstCount);
        [OperationContract]
        long CMSSetUsage(string sCarrierID, string sUsage);
        [OperationContract]
        long CMSSetCarrierOutStart(string sLocID, string sCarrierID);
        [OperationContract]
        long CMSRspCancelCarrierOut(long nMsgId, string sCarrierID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText);
        [OperationContract]
        long CMSSetTransferReady(string sLocID, long nState);
        [OperationContract]
        long PJReqCommand(long nCommand, string sPJobID);
        [OperationContract]
        long PJReqGetJob(string sPJobID);
        [OperationContract]
        long PJReqGetAllJobID();
        [OperationContract]
        long PJReqCreate(string sPJobID, long nMtrlFormat, long nAutoStart, long nMtrlOrder, long nMtrlCount, string[] psMtrlID, string[] psSlotInfo, long nRcpMethod, string sRcpID, long nRcpParCount, string[] psRcpParName, string[] psRcpParValue);
        [OperationContract]
        long PJRspVerify(long nMsgID, long nPJobCount, string[] psPJobID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText);
        [OperationContract]
        long PJSetState(string sPJobID, long nState);
        [OperationContract]
        long PJSettingUpCompt(string sPJobID);
        [OperationContract]
        long PJGetAllJobInfo(ref long pnObjID, ref long pnPJobCount);
        [OperationContract]
        long GetPRJobID(long nObjID, long nIndex, ref string psPJobID);
        [OperationContract]
        long GetPRJobState(long nObjID, long nIndex, ref long pnState);
        [OperationContract]
        long GetPRJobAutoStart(long nObjID, long nIndex, ref long pnAutoStart);
        [OperationContract]
        long GetPRJobMtrlOrder(long nObjID, long nIndex, ref long pnMtrlOrder);
        [OperationContract]
        long GetPRJobMtrlFormat(long nObjID, long nIndex, ref long pnMtrlFormat);
        [OperationContract]
        long GetPRJobCarrierCount(long nObjID, long nIndex, ref long pnCount);
        [OperationContract]
        long GetPRJobCarrier(long nObjID, long nIndex, long nCount, ref string[] psCarrierID, ref string[] psSlotInfo);
        [OperationContract]
        long GetPRJobRcpID(long nObjID, long nIndex, ref string psRcpID);
        [OperationContract]
        long GetPRJobRcpParamCount(long nObjID, long nIndex, ref long pnCount);
        [OperationContract]
        long GetPRJobRcpParam(long nObjID, long nIndex, long nCount, ref string[] psRcpParName, ref string[] psRcpParValue);
        [OperationContract]
        long GetPRJobClose(long nObjID);
        [OperationContract]
        long PJDelJobInfo(string sPJobID);
        [OperationContract]
        long PJDelAllJobInfo();
        [OperationContract]
        long PJSetJobInfo(string sPJobID, long nMtrlFormat, long nAutoStart, long nMtrlOrder, long nMtrlCount, string[] psMtrlID, string[] psSlotInfo, long nRcpMethod, string sRcpID, long nRcpParCount, string[] psRcpParName, string[] psRcpParVal);
        [OperationContract]
        long PJSettingUpStart(string sPJobID);
        [OperationContract]
        long PJRspCommand(long nMsgID, long nCommand, string sPJobID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText);
        [OperationContract]
        long PJRspSetRcpVariable(long nMsgID, string sPJobID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText);
        [OperationContract]
        long PJRspSetStartMethod(long nMsgID, long nPJobCount, string[] psPJobID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText);
        [OperationContract]
        long PJRspSetMtrlOrder(long nMsgID, long nResult);
        [OperationContract]
        long PJReqCreateEx(string sPJobID, long nMtrlFormat, long nAutoStart, long nMtrlOrder, long nMtrlCount, string[] psMtrlID, string[] psSlotInfo, long nRcpMethod, string sRcpID, long nRcpParCount, string[] psRcpParName, long[] pnRcpParValue);
        [OperationContract]
        long GetPRJobRcpParamEx(long nObjID, long nIndex, long nCount, ref string[] psRcpParName, ref long[] pnRcpParValue);
        [OperationContract]
        long PJSetJobInfoEx(string sPJobID, long nMtrlFormat, long nAutoStart, long nMtrlOrder, long nMtrlCount, string[] psMtrlID, string[] psSlotInfo, long nRcpMethod, string sRcpID, long nRcpParCount, string[] psRcpParName, long[] pnRcpParVal);
        [OperationContract]
        long CJReqCommand(string sCJobID, long nCommand, string sCPName, string sCPVal);
        [OperationContract]
        long CJReqGetJob(string sCJobID);
        [OperationContract]
        long CJReqGetAllJobID();
        [OperationContract]
        long CJReqCreate(string sCJobID, long nStartMethod, long nCountPRJob, string[] psPRJobID);
        [OperationContract]
        long CJReqSelect(string sCJobID);
        [OperationContract]
        long CJGetAllJobInfo(ref long pnObjID, ref long pnCount);
        [OperationContract]
        long GetCtrlJobID(long nObjID, long nIndex, ref string psCtrlJobID);
        [OperationContract]
        long GetCtrlJobState(long nObjID, long nIndex, ref long pnState);
        [OperationContract]
        long GetCtrlJobStartMethod(long nObjID, long nIndex, ref long pnAutoStart);
        [OperationContract]
        long GetCtrlJobPRJobCount(long nObjID, long nIndex, ref long pnCount);
        [OperationContract]
        long GetCtrlJobPRJobIDs(long nObjID, long nIndex, long nCount, ref string[] psPRJobIDs);
        [OperationContract]
        long GetCtrlJobClose(long nObjID);
        [OperationContract]
        long CJDelJobInfo(string sCJobID);
        [OperationContract]
        long CJDelAllJobInfo();
        [OperationContract]
        long CJSetJobInfo(string sCJobID, long nState, long nStartMethod, long nCountPRJob, string[] psPRJobID);
        [OperationContract]
        long CJReqHOQJob(string sCJobID);
        [OperationContract]
        long CJGetHOQJob();
        [OperationContract]
        long CJRspVerify(long nMsgId, string sCJobID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText);
        [OperationContract]
        long CJRspCommand(long nMsgId, string sCJobID, long nCommand, long nResult, long nErrCode, string sErrText);
        [OperationContract]
        long STSSetSubstLocationInfo(string sSubstLocID, string sSubstrateID);
        [OperationContract]
        long STSSetBatchLocationInfo(string sBatchLocID, string sSubstrateID);
        [OperationContract]
        long STSSetTransport(string sSubstLocID, string sSubstrateID, long nState);
        [OperationContract]
        long STSSetBatchTransport(long nCount, string[] psSubstLocID, string[] psSubstrateID, long nState);
        [OperationContract]
        long STSSetProcessing(string sSubstLocID, string sSubstrateID, long nState);
        [OperationContract]
        long STSSetBatchProcessing(long nCount, string[] psSubstLocID, string[] psSubstrateID, long nState);
        [OperationContract]
        long STSGetAllSubstrateInfo(ref long pnObjID, ref long pnCount);
        [OperationContract]
        long GetSubstrateID(long nObjID, long nIndex, ref string psSubstrateID);
        [OperationContract]
        long GetSubstrateState(long nObjID, long nIndex, ref long pnTransportState, ref long pnProcessingState, ref long pnReadingState);
        [OperationContract]
        long GetSubstrateLocID(long nObjID, long nIndex, ref string psSubstLocID);
        [OperationContract]
        long GetSubstrateClose(long nObjID);
        [OperationContract]
        long STSDelSubstrateInfo(string sSubstrateID);
        [OperationContract]
        long STSDelAllSubstrateInfo();
        [OperationContract]
        long STSSetSubstrateInfo(string sSubstLocID, string sSubstrateID, long nTransportState, long nProcessingState, long nReadingState);
        [OperationContract]
        long STSSetMaterialArrived(string sMaterialID);
        [OperationContract]
        long STSReqCreateSubstrate(string sSubstLocID, string sSubstrateID);
        [OperationContract]
        long STSRspCreateSubstrate(long nMsgID, string sSubstLocID, string sSubstrateID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText);
        [OperationContract]
        long STSReqCancelSubstrate(string sSubstLocID, string sSubstrateID);
        [OperationContract]
        long STSRspCancelSubstrate(long nMsgID, string sSubstLocID, string sSubstrateID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText);
        [OperationContract]
        long STSReqProceedSubstrate(string sSubstLocID, string sSubstrateID, string sReadSubstID);
        [OperationContract]
        long STSRspUpdateSubstrate(long nMsgID, string sSubstLocID, string sSubstrateID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText);
        [OperationContract]
        long STSReqDeleteSubstrate(string sSubstLocID, string sSubstrateID);
        [OperationContract]
        long STSRspDeleteSubstrate(long nMsgID, string sSubstLocID, string sSubstrateID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText);
        [OperationContract]
        long STSSetSubstrateID(string sSubstLocID, string sSubstrateID, string sSubstReadID, long nResult);
        [OperationContract]
        long OpenGEMObject(ref long pnMsgID, string sObjType, string sObjID);
        [OperationContract]
        long CloseGEMObject(long nMsgID);
        [OperationContract]
        long GetAttrData(ref long pnObjectID, long nMsgID, string sAttrName);

        //Add
        [OperationContract]
        string GetIP();
        [OperationContract]
        void SetIP(string sNewValue);
        [OperationContract]
        long GetPort();
        [OperationContract]
        void SetPort(long nNewValue);
        [OperationContract]
        bool GetActive();
        [OperationContract]
        void SetActive(bool bNewValue);
        [OperationContract]
        long Initialize(string sCfg);
        [OperationContract]
        long RangeCheck(long[] pnIds);
        [OperationContract]
        bool ReadXGemInfoFromRegistry();
        [OperationContract]
        int RunProcess(string sName, string sCfgPath, string sPassword);

        #endregion
    }

    public interface ISecsGemServiceHost : ISecsGemService
    {
        [OperationContract]
        void SetSecsGemDefineReport(SecsGemDefineReport gemDefineReport);

        [OperationContract]
        List<long> GetVidsFormCeid(long ceid);

        [OperationContract]
        bool CheckCommConnectivity(int stageIndex = -1);
        [OperationContract]
        bool StartCommanderService();
        [OperationContract]
        void OnRemoteCommandAction(int index, RemoteActReqData reqdata);
    }
    public interface ISecsGemMessageReceiver
    {
        ISecsGemServiceCallback callback { get; set; }
        bool SetXGem(object xgem);
        void XGEM_OnSECSMessageReceive(long nMsgId, long nStream, long nFunction, long nSysbyte);

        #region //Send Ack Message Function
        long MakeObject(ref long pnMsgId);
        long SetListItem(long nMsgId, long nItemCount);
        long SetBinaryItem(long nMsgId, byte nValue);
        long SetBoolItem(long nMsgId, bool nValue);
        long SendSECSMessage(long nMsgId, long nStream, long nFunction, long nSysbyte);
        #endregion
    }
    public interface ISecsGemServiceCallback
    {
        [OperationContract]
        bool CallBack_ConnectSuccess();

        [OperationContract]
        bool Are_You_There();

        [OperationContract(IsOneWay = true)]
        void CallBack_Close();

        [OperationContract(IsOneWay = true)]
        void OnSECSMessageReceive(long nObjectID, long nStream, long nFunction, long nSysbyte);

        [OperationContract(IsOneWay = true)]
        void RemoteActMsgReceive(RemoteActReqData msgData);

        [OperationContract(IsOneWay = true)]
        void OnCarrierActMsgRecive(CarrierActReqData msgData);
        [OperationContract(IsOneWay = true)]
        void ECVChangeMsgReceive(EquipmentReqData msgData);
        [OperationContract(IsOneWay = true)]
        void OnGEMControlStateChange(SecsControlStateEnum ControlState);
        [OperationContract(IsOneWay = true)]
        void OnGEMStateEvent(SecsGemStateEnum GemState);
        [OperationContract(IsOneWay = true)]
        void OnGEMCommStateChange(SecsCommStateEnum CommState);
        [OperationContract(IsOneWay = true)]
        void OnGEMRspGetDateTime(String sSystemTime);
        [OperationContract(IsOneWay = true)]
        void GEMRspRemoteCommand(long nMsgId, string sCmd, long nHCAck, long nCount, long[] pnResult);
        [OperationContract(IsOneWay = true)]
        void OnGEMTerminalMultiMessage(long nTid, long nCount, string[] psMsg);
        [OperationContract(IsOneWay = true)]
        void OnGEMTerminalMessage(long nTid, string sMsg);
        [OperationContract(IsOneWay = true)]
        void OnGEMReqDateTime(long nMsgId, string sSystemTime);
        [OperationContract(IsOneWay = true)]
        void OnGEMReqRemoteCommand(long nMsgId, EnumRemoteCommand Rcmd, long nCount, string[] psNames, string[] psVals);
        [OperationContract(IsOneWay = true)]
        void OnGEMReqPPList(long nMsgId);
        [OperationContract(IsOneWay = true)]
        void OnGEMECVChanged(long nCount, long[] pnEcids, string[] psVals);
        [OperationContract(IsOneWay = true)]
        void OnGEMReqChangeECV(long nMsgId, long nCount, long[] pnEcids, string[] psVals);
        [OperationContract(IsOneWay = true)]
        void OnGEMErrorEvent(string sErrorName, long nErrorCode);
        [OperationContract(IsOneWay = true)]
        void OnGEMReqPPDelete(long nMsgId, long nCount, string[] psPpid);
        [OperationContract(IsOneWay = true)]
        void OnGEMRspAllECInfo(long lCount, long[] plVid, string[] psName, string[] psValue, string[] psDefault, string[] psMin, string[] psMax, string[] psUnit);
        [OperationContract(IsOneWay = true)]
        void OnGEMReqPPSendEx(long nMsgId, string sPpid, string sRecipePath);
        [OperationContract(IsOneWay = true)]
        void OnGEMReqPPEx(long nMsgId, string sPpid, string sRecipePath);
        [OperationContract(IsOneWay = true)]
        void OnRemoteCommandAction(RemoteActReqData msgData);
        [OperationContract(IsOneWay = true)]
        void OnDefineReportRecive(SecsGemDefineReport report);
    }

    #region Event Delegate
    public delegate void OnSECSMessageReceived(long nObjectID, long nStream, long nFunction, long nSysbyte);
    public delegate void OnGEMCommStateChanged(long nState);
    public delegate void OnGEMControlStateChanged(long nState);
    public delegate void OnGEMReqChangeECV(long nMsgId, long nCount, long[] pnEcids, string[] psVals);
    public delegate void OnGEMECVChanged(long nCount, long[] pnEcids, string[] psVals);
    public delegate void OnGEMReqGetDateTime(long nMsgId);
    public delegate void OnGEMRspGetDateTime(string sSystemTime);
    public delegate void OnGEMReqDateTime(long nMsgId, string sSystemTime);
    public delegate void OnGEMErrorEvent(string sErrorName, long nErrorCode);
    public delegate void OnGEMReqRemoteCommand(long nMsgId, string sRcmd, long nCount, string[] psNames, string[] psVals);
    public delegate void OnGEMReqPPLoadInquire(long nMsgId, string sPpid, long nLength);
    public delegate void OnGEMRspPPLoadInquire(string sPpid, long nResult);
    public delegate void OnGEMReqPPSend(long nMsgId, string sPPID, string sBody);
    public delegate void OnGEMRspPPSend(string sPpid, long nResult);
    public delegate void OnGEMReqPP(long nMsgId, string sPpid);
    public delegate void OnGEMRspPP(string sPpid, string sBody);
    public delegate void OnGEMReqPPDelete(long nMsgId, long nCount, string[] psPpid);
    public delegate void OnGEMReqPPList(long nMsgId);
    public delegate void OnGEMReqPPFmtSend(long nMsgId, string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames);
    public delegate void OnGEMReqPPFmtSend2(long nMsgId, string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames, string[] psParamValues);
    public delegate void OnGEMRspPPFmtSend(string sPpid, long nResult);
    public delegate void OnGEMReqPPFmt(long nMsgId, string sPpid);
    public delegate void OnGEMRspPPFmt(string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames);
    public delegate void OnGEMRspPPFmt2(string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames, string[] psParamValues);
    public delegate void OnGEMRspPPFmtVerification(string sPpid, long nResult);
    public delegate void OnGEMTerminalMessage(long nTid, string sMsg);
    public delegate void OnGEMTerminalMultiMessage(long nTid, long nCount, string[] psMsg);
    public delegate void OnGEMSpoolStateChanged(long nState, long nLoadState, long nUnloadState, string sFullTime, long nMaxTransmit, long nMsgNum, long nTotalNum, long nTransmitFail);
    public delegate void OnXGEMStateEvent(long nState);
    public delegate void OnGEMRspAllECInfo(long lCount, long[] plVid, string[] psName, string[] psValue, string[] psDefault, string[] psMin, string[] psMax, string[] psUnit);
    public delegate void OnGEMReqPPSendEx(long nMsgId, string sPpid, string sRecipePath);
    public delegate void OnGEMRspPPEx(string sPpid, string sRecipePath);
    public delegate void OnGEMReqPPEx(long nMsgId, string sPpid, string sRecipePath);
    public delegate void OnGEMRspPPSendEx(string sPpid, string sRecipePath, long nResult);
    public delegate void OnGEMReportedEvent(long nEventID);
    public delegate void OnGEMReportedEvent2(long nEventID, long nSysbyte);
    public delegate void OnGEMRspLoopback(long nCount, long[] pnAbs);
    public delegate void OnGEMReqOnline(long nMsgId, long nFromState, long nToState);
    public delegate void OnGEMReqOffline(long nMsgId, long nFromState, long nToState);
    public delegate void OnGEMNotifyPerformanceWarning(long nLevel);
    public delegate void OnGEMSecondaryMsgReceived(long nS, long nF, long nSysbyte, string sParam1, string sParam2, string sParam3);
    public delegate void OnGEMRecvUserMessage(long nObjectID, string sCommand, long nTransId);
    #endregion

    // 정의된 함수를 GEM 제품별로 관련 함수로 연결.
    public class XGEM : ISecsGemService
    {
        private XGemNet m_XGem = null;
        private XGem300ProNet m_XGem300Pro = null;

        public enum MODE
        {
            NONE,
            XGEM,
            XGEM300Pro
        }
        public XGEM.MODE XGemMode;
        public XGEM()
        {
            this.XGemMode = MODE.NONE;
        }
        public void Dispose() 
        {
            EventRemove();   
            m_XGem = null;
            m_XGem300Pro = null;
        }
        public XGEM(XGemNet xgem)
        {
            this.XGemMode = MODE.XGEM;
            m_XGem = xgem;
            EventAdd();
        }
        public XGEM(XGem300ProNet xgem300Pro)
        {
            this.XGemMode = MODE.XGEM300Pro;
            m_XGem300Pro = xgem300Pro;
            EventAdd();
        }
        bool IsRunXGem()
        {
            return XGemMode == XGEM.MODE.XGEM && m_XGem != null;
        }
        bool IsRunXGem300Pro()
        {
            return XGemMode == XGEM.MODE.XGEM300Pro && m_XGem300Pro != null;
        }

        #region Event
        public event OnSECSMessageReceived OnSECSMessageReceived;
        public event OnGEMCommStateChanged OnGEMCommStateChanged;
        public event OnGEMControlStateChanged OnGEMControlStateChanged;
        public event OnGEMReqChangeECV OnGEMReqChangeECV;
        public event OnGEMECVChanged OnGEMECVChanged;
        public event OnGEMReqGetDateTime OnGEMReqGetDateTime;
        public event OnGEMRspGetDateTime OnGEMRspGetDateTime;
        public event OnGEMReqDateTime OnGEMReqDateTime;
        public event OnGEMErrorEvent OnGEMErrorEvent;
        public event OnGEMReqRemoteCommand OnGEMReqRemoteCommand;
        public event OnGEMReqPPLoadInquire OnGEMReqPPLoadInquire;
        public event OnGEMRspPPLoadInquire OnGEMRspPPLoadInquire;
        public event OnGEMReqPPSend OnGEMReqPPSend;
        public event OnGEMRspPPSend OnGEMRspPPSend;
        public event OnGEMReqPP OnGEMReqPP;
        public event OnGEMRspPP OnGEMRspPP;
        public event OnGEMReqPPDelete OnGEMReqPPDelete;
        public event OnGEMReqPPList OnGEMReqPPList;
        public event OnGEMReqPPFmtSend OnGEMReqPPFmtSend;
        public event OnGEMReqPPFmtSend2 OnGEMReqPPFmtSend2;
        public event OnGEMRspPPFmtSend OnGEMRspPPFmtSend;
        public event OnGEMReqPPFmt OnGEMReqPPFmt;
        public event OnGEMRspPPFmt OnGEMRspPPFmt;
        public event OnGEMRspPPFmt2 OnGEMRspPPFmt2;
        public event OnGEMRspPPFmtVerification OnGEMRspPPFmtVerification;
        public event OnGEMTerminalMessage OnGEMTerminalMessage;
        public event OnGEMTerminalMultiMessage OnGEMTerminalMultiMessage;
        public event OnGEMSpoolStateChanged OnGEMSpoolStateChanged;
        public event OnXGEMStateEvent OnXGEMStateEvent;
        public event OnGEMRspAllECInfo OnGEMRspAllECInfo;
        public event OnGEMReqPPSendEx OnGEMReqPPSendEx;
        public event OnGEMRspPPEx OnGEMRspPPEx;
        public event OnGEMReqPPEx OnGEMReqPPEx;
        public event OnGEMRspPPSendEx OnGEMRspPPSendEx;
        public event OnGEMReportedEvent OnGEMReportedEvent;
        public event OnGEMReportedEvent2 OnGEMReportedEvent2;
        public event OnGEMRspLoopback OnGEMRspLoopback;
        public event OnGEMReqOnline OnGEMReqOnline;
        public event OnGEMReqOffline OnGEMReqOffline;
        public event OnGEMNotifyPerformanceWarning OnGEMNotifyPerformanceWarning;
        public event OnGEMSecondaryMsgReceived OnGEMSecondaryMsgReceived;
        public event OnGEMRecvUserMessage OnGEMRecvUserMessage;
        #endregion
        #region Event 중계
        private void OnSECSMessageReceivedEvent(long nObjectID, long nStream, long nFunction, long nSysbyte) => OnSECSMessageReceived?.Invoke(nObjectID, nStream, nFunction, nSysbyte);
        private void OnGEMCommStateChangedEvent(long nState) => OnGEMCommStateChanged?.Invoke(nState);
        private void OnGEMControlStateChangedEvent(long nState) => OnGEMControlStateChanged?.Invoke(nState);
        private void OnGEMReqChangeECVEvent(long nMsgId, long nCount, long[] pnEcids, string[] psVals) => OnGEMReqChangeECV?.Invoke(nMsgId, nCount, pnEcids, psVals);
        private void OnGEMECVChangedEvent(long nCount, long[] pnEcids, string[] psVals) => OnGEMECVChanged?.Invoke(nCount, pnEcids, psVals);
        private void OnGEMReqGetDateTimeEvent(long nMsgId) => OnGEMReqGetDateTime?.Invoke(nMsgId);
        private void OnGEMRspGetDateTimeEvent(string sSystemTime) => OnGEMRspGetDateTime?.Invoke(sSystemTime);
        private void OnGEMReqDateTimeEvent(long nMsgId, string sSystemTime) => OnGEMReqDateTime?.Invoke(nMsgId, sSystemTime);
        private void OnGEMErrorEventEvent(string sErrorName, long nErrorCode) => OnGEMErrorEvent?.Invoke(sErrorName, nErrorCode);
        private void OnGEMReqRemoteCommandEvent(long nMsgId, string sRcmd, long nCount, string[] psNames, string[] psVals) => OnGEMReqRemoteCommand?.Invoke(nMsgId, sRcmd, nCount, psNames, psVals);
        private void OnGEMReqPPLoadInquireEvent(long nMsgId, string sPpid, long nLength) => OnGEMReqPPLoadInquire?.Invoke(nMsgId, sPpid, nLength);
        private void OnGEMRspPPLoadInquireEvent(string sPpid, long nResult) => OnGEMRspPPLoadInquire?.Invoke(sPpid, nResult);
        private void OnGEMReqPPSendEvent(long nMsgId, string sPPID, string sBody) => OnGEMReqPPSend?.Invoke(nMsgId, sPPID, sBody);
        private void OnGEMRspPPSendEvent(string sPpid, long nResult) => OnGEMRspPPSend?.Invoke(sPpid, nResult);
        private void OnGEMReqPPEvent(long nMsgId, string sPpid) => OnGEMReqPP?.Invoke(nMsgId, sPpid);
        private void OnGEMRspPPEvent(string sPpid, string sBody) => OnGEMRspPP?.Invoke(sPpid, sBody);
        private void OnGEMReqPPDeleteEvent(long nMsgId, long nCount, string[] psPpid) => OnGEMReqPPDelete?.Invoke(nMsgId, nCount, psPpid);
        private void OnGEMReqPPListEvent(long nMsgId) => OnGEMReqPPList?.Invoke(nMsgId);
        private void OnGEMReqPPFmtSendEvent(long nMsgId, string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames) => OnGEMReqPPFmtSend?.Invoke(nMsgId, sPpid, sMdln, sSoftRev, nCount, psCCode, pnParamCount, psParamNames);
        private void OnGEMReqPPFmtSend2Event(long nMsgId, string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames, string[] psParamValues) => OnGEMReqPPFmtSend2?.Invoke(nMsgId, sPpid, sMdln, sSoftRev, nCount, psCCode, pnParamCount, psParamNames, psParamValues);
        private void OnGEMRspPPFmtSendEvent(string sPpid, long nResult) => OnGEMRspPPFmtSend?.Invoke(sPpid, nResult);
        private void OnGEMReqPPFmtEvent(long nMsgId, string sPpid) => OnGEMReqPPFmt?.Invoke(nMsgId, sPpid);
        private void OnGEMRspPPFmtEvent(string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames) => OnGEMRspPPFmt?.Invoke(sPpid, sMdln, sSoftRev, nCount, psCCode, pnParamCount, psParamNames);
        private void OnGEMRspPPFmt2Event(string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames, string[] psParamValues) => OnGEMRspPPFmt2?.Invoke(sPpid, sMdln, sSoftRev, nCount, psCCode, pnParamCount, psParamNames, psParamValues);
        private void OnGEMRspPPFmtVerificationEvent(string sPpid, long nResult) => OnGEMRspPPFmtVerification?.Invoke(sPpid, nResult);
        private void OnGEMTerminalMessageEvent(long nTid, string sMsg) => OnGEMTerminalMessage?.Invoke(nTid, sMsg);
        private void OnGEMTerminalMultiMessageEvent(long nTid, long nCount, string[] psMsg) => OnGEMTerminalMultiMessage?.Invoke(nTid, nCount, psMsg);
        private void OnGEMSpoolStateChangedEvent(long nState, long nLoadState, long nUnloadState, string sFullTime, long nMaxTransmit, long nMsgNum, long nTotalNum, long nTransmitFail) => OnGEMSpoolStateChanged?.Invoke(nState, nLoadState, nUnloadState, sFullTime, nMaxTransmit, nMsgNum, nTotalNum, nTransmitFail);
        private void OnXGEMStateEventEvent(long nState) => OnXGEMStateEvent?.Invoke(nState);
        private void OnGEMRspAllECInfoEvent(long lCount, long[] plVid, string[] psName, string[] psValue, string[] psDefault, string[] psMin, string[] psMax, string[] psUnit) => OnGEMRspAllECInfo?.Invoke(lCount, plVid, psName, psValue, psDefault, psMin, psMax, psUnit);
        private void OnGEMReqPPSendExEvent(long nMsgId, string sPpid, string sRecipePath) => OnGEMReqPPSendEx?.Invoke(nMsgId, sPpid, sRecipePath);
        private void OnGEMRspPPExEvent(string sPpid, string sRecipePath) => OnGEMRspPPEx?.Invoke(sPpid, sRecipePath);
        private void OnGEMReqPPExEvent(long nMsgId, string sPpid, string sRecipePath) => OnGEMReqPPEx?.Invoke(nMsgId, sPpid, sRecipePath);
        private void OnGEMRspPPSendExEvent(string sPpid, string sRecipePath, long nResult) => OnGEMRspPPSendEx?.Invoke(sPpid, sRecipePath, nResult);
        private void OnGEMReportedEventEvent(long nEventID) => OnGEMReportedEvent?.Invoke(nEventID);
        private void OnGEMReportedEvent2Event(long nEventID, long nSysbyte) => OnGEMReportedEvent2?.Invoke(nEventID, nSysbyte);
        private void OnGEMRspLoopbackEvent(long nCount, long[] pnAbs) => OnGEMRspLoopback?.Invoke(nCount, pnAbs);
        private void OnGEMReqOnlineEvent(long nMsgId, long nFromState, long nToState) => OnGEMReqOnline?.Invoke(nMsgId, nFromState, nToState);
        private void OnGEMReqOfflineEvent(long nMsgId, long nFromState, long nToState) => OnGEMReqOffline?.Invoke(nMsgId, nFromState, nToState);
        private void OnGEMNotifyPerformanceWarningEvent(long nLevel) => OnGEMNotifyPerformanceWarning?.Invoke(nLevel);
        private void OnGEMSecondaryMsgReceivedEvent(long nS, long nF, long nSysbyte, string sParam1, string sParam2, string sParam3) => OnGEMSecondaryMsgReceived?.Invoke(nS, nF, nSysbyte, sParam1, sParam2, sParam3);
        private void OnGEMRecvUserMessageEvent(long nObjectID, string sCommand, long nTransId) => OnGEMRecvUserMessage?.Invoke(nObjectID, sCommand, nTransId);
        #endregion
        #region Event GemEvent를 내부 함수에 연결
        private void EventAddXGem()
        {
            var gem = m_XGem;
            gem.OnSECSMessageReceived += OnSECSMessageReceivedEvent;
            gem.OnGEMCommStateChanged += OnGEMCommStateChangedEvent;
            gem.OnGEMControlStateChanged += OnGEMControlStateChangedEvent;
            gem.OnGEMReqChangeECV += OnGEMReqChangeECVEvent;
            gem.OnGEMECVChanged += OnGEMECVChangedEvent;
            gem.OnGEMReqGetDateTime += OnGEMReqGetDateTimeEvent;
            gem.OnGEMRspGetDateTime += OnGEMRspGetDateTimeEvent;
            gem.OnGEMReqDateTime += OnGEMReqDateTimeEvent;
            gem.OnGEMErrorEvent += OnGEMErrorEventEvent;
            gem.OnGEMReqRemoteCommand += OnGEMReqRemoteCommandEvent;
            gem.OnGEMReqPPLoadInquire += OnGEMReqPPLoadInquireEvent;
            gem.OnGEMRspPPLoadInquire += OnGEMRspPPLoadInquireEvent;
            gem.OnGEMReqPPSend += OnGEMReqPPSendEvent;
            gem.OnGEMRspPPSend += OnGEMRspPPSendEvent;
            gem.OnGEMReqPP += OnGEMReqPPEvent;
            gem.OnGEMRspPP += OnGEMRspPPEvent;
            gem.OnGEMReqPPDelete += OnGEMReqPPDeleteEvent;
            gem.OnGEMReqPPList += OnGEMReqPPListEvent;
            gem.OnGEMReqPPFmtSend += OnGEMReqPPFmtSendEvent;
            gem.OnGEMReqPPFmtSend2 += OnGEMReqPPFmtSend2Event;
            gem.OnGEMRspPPFmtSend += OnGEMRspPPFmtSendEvent;
            gem.OnGEMReqPPFmt += OnGEMReqPPFmtEvent;
            gem.OnGEMRspPPFmt += OnGEMRspPPFmtEvent;
            gem.OnGEMRspPPFmt2 += OnGEMRspPPFmt2Event;
            gem.OnGEMRspPPFmtVerification += OnGEMRspPPFmtVerificationEvent;
            gem.OnGEMTerminalMessage += OnGEMTerminalMessageEvent;
            gem.OnGEMTerminalMultiMessage += OnGEMTerminalMultiMessageEvent;
            gem.OnGEMSpoolStateChanged += OnGEMSpoolStateChangedEvent;
            gem.OnXGEMStateEvent += OnXGEMStateEventEvent;
            gem.OnGEMRspAllECInfo += OnGEMRspAllECInfoEvent;
            gem.OnGEMReqPPSendEx += OnGEMReqPPSendExEvent;
            gem.OnGEMRspPPEx += OnGEMRspPPExEvent;
            gem.OnGEMReqPPEx += OnGEMReqPPExEvent;
            gem.OnGEMRspPPSendEx += OnGEMRspPPSendExEvent;
            gem.OnGEMReportedEvent += OnGEMReportedEventEvent;
            gem.OnGEMReportedEvent2 += OnGEMReportedEvent2Event;
            gem.OnGEMRspLoopback += OnGEMRspLoopbackEvent;
            gem.OnGEMReqOnline += OnGEMReqOnlineEvent;
            gem.OnGEMReqOffline += OnGEMReqOfflineEvent;
            gem.OnGEMNotifyPerformanceWarning += OnGEMNotifyPerformanceWarningEvent;
            gem.OnGEMSecondaryMsgReceived += OnGEMSecondaryMsgReceivedEvent;
            gem.OnGEMRecvUserMessage += OnGEMRecvUserMessageEvent;

        }
        private void EventRemoveXGem()
        {
            var gem = m_XGem;
            gem.OnSECSMessageReceived -= OnSECSMessageReceivedEvent;
            gem.OnGEMCommStateChanged -= OnGEMCommStateChangedEvent;
            gem.OnGEMControlStateChanged -= OnGEMControlStateChangedEvent;
            gem.OnGEMReqChangeECV -= OnGEMReqChangeECVEvent;
            gem.OnGEMECVChanged -= OnGEMECVChangedEvent;
            gem.OnGEMReqGetDateTime -= OnGEMReqGetDateTimeEvent;
            gem.OnGEMRspGetDateTime -= OnGEMRspGetDateTimeEvent;
            gem.OnGEMReqDateTime -= OnGEMReqDateTimeEvent;
            gem.OnGEMErrorEvent -= OnGEMErrorEventEvent;
            gem.OnGEMReqRemoteCommand -= OnGEMReqRemoteCommandEvent;
            gem.OnGEMReqPPLoadInquire -= OnGEMReqPPLoadInquireEvent;
            gem.OnGEMRspPPLoadInquire -= OnGEMRspPPLoadInquireEvent;
            gem.OnGEMReqPPSend -= OnGEMReqPPSendEvent;
            gem.OnGEMRspPPSend -= OnGEMRspPPSendEvent;
            gem.OnGEMReqPP -= OnGEMReqPPEvent;
            gem.OnGEMRspPP -= OnGEMRspPPEvent;
            gem.OnGEMReqPPDelete -= OnGEMReqPPDeleteEvent;
            gem.OnGEMReqPPList -= OnGEMReqPPListEvent;
            gem.OnGEMReqPPFmtSend -= OnGEMReqPPFmtSendEvent;
            gem.OnGEMReqPPFmtSend2 -= OnGEMReqPPFmtSend2Event;
            gem.OnGEMRspPPFmtSend -= OnGEMRspPPFmtSendEvent;
            gem.OnGEMReqPPFmt -= OnGEMReqPPFmtEvent;
            gem.OnGEMRspPPFmt -= OnGEMRspPPFmtEvent;
            gem.OnGEMRspPPFmt2 -= OnGEMRspPPFmt2Event;
            gem.OnGEMRspPPFmtVerification -= OnGEMRspPPFmtVerificationEvent;
            gem.OnGEMTerminalMessage -= OnGEMTerminalMessageEvent;
            gem.OnGEMTerminalMultiMessage -= OnGEMTerminalMultiMessageEvent;
            gem.OnGEMSpoolStateChanged -= OnGEMSpoolStateChangedEvent;
            gem.OnXGEMStateEvent -= OnXGEMStateEventEvent;
            gem.OnGEMRspAllECInfo -= OnGEMRspAllECInfoEvent;
            gem.OnGEMReqPPSendEx -= OnGEMReqPPSendExEvent;
            gem.OnGEMRspPPEx -= OnGEMRspPPExEvent;
            gem.OnGEMReqPPEx -= OnGEMReqPPExEvent;
            gem.OnGEMRspPPSendEx -= OnGEMRspPPSendExEvent;
            gem.OnGEMReportedEvent -= OnGEMReportedEventEvent;
            gem.OnGEMReportedEvent2 -= OnGEMReportedEvent2Event;
            gem.OnGEMRspLoopback -= OnGEMRspLoopbackEvent;
            gem.OnGEMReqOnline -= OnGEMReqOnlineEvent;
            gem.OnGEMReqOffline -= OnGEMReqOfflineEvent;
            gem.OnGEMNotifyPerformanceWarning -= OnGEMNotifyPerformanceWarningEvent;
            gem.OnGEMSecondaryMsgReceived -= OnGEMSecondaryMsgReceivedEvent;
            gem.OnGEMRecvUserMessage -= OnGEMRecvUserMessageEvent;
        }
        private void EventAddXGem300Pro()
        {
            var gem = m_XGem300Pro;
            gem.OnSECSMessageReceived += OnSECSMessageReceivedEvent;
            gem.OnGEMCommStateChanged += OnGEMCommStateChangedEvent;
            gem.OnGEMControlStateChanged += OnGEMControlStateChangedEvent;
            gem.OnGEMReqChangeECV += OnGEMReqChangeECVEvent;
            gem.OnGEMECVChanged += OnGEMECVChangedEvent;
            gem.OnGEMReqGetDateTime += OnGEMReqGetDateTimeEvent;
            gem.OnGEMRspGetDateTime += OnGEMRspGetDateTimeEvent;
            gem.OnGEMReqDateTime += OnGEMReqDateTimeEvent;
            gem.OnGEMErrorEvent += OnGEMErrorEventEvent;
            gem.OnGEMReqRemoteCommand += OnGEMReqRemoteCommandEvent;
            gem.OnGEMReqPPLoadInquire += OnGEMReqPPLoadInquireEvent;
            gem.OnGEMRspPPLoadInquire += OnGEMRspPPLoadInquireEvent;
            //gem.OnGEMReqPPSend += OnGEMReqPPSendEvent;
            gem.OnGEMRspPPSend += OnGEMRspPPSendEvent;
            gem.OnGEMReqPP += OnGEMReqPPEvent;
            //gem.OnGEMRspPP += OnGEMRspPPEvent;
            gem.OnGEMReqPPDelete += OnGEMReqPPDeleteEvent;
            gem.OnGEMReqPPList += OnGEMReqPPListEvent;
            gem.OnGEMReqPPFmtSend += OnGEMReqPPFmtSendEvent;
            gem.OnGEMReqPPFmtSend2 += OnGEMReqPPFmtSend2Event;
            gem.OnGEMRspPPFmtSend += OnGEMRspPPFmtSendEvent;
            gem.OnGEMReqPPFmt += OnGEMReqPPFmtEvent;
            gem.OnGEMRspPPFmt += OnGEMRspPPFmtEvent;
            gem.OnGEMRspPPFmt2 += OnGEMRspPPFmt2Event;
            gem.OnGEMRspPPFmtVerification += OnGEMRspPPFmtVerificationEvent;
            gem.OnGEMTerminalMessage += OnGEMTerminalMessageEvent;
            gem.OnGEMTerminalMultiMessage += OnGEMTerminalMultiMessageEvent;
            gem.OnGEMSpoolStateChanged += OnGEMSpoolStateChangedEvent;
            gem.OnXGEMStateEvent += OnXGEMStateEventEvent;
            gem.OnGEMRspAllECInfo += OnGEMRspAllECInfoEvent;
            gem.OnGEMReqPPSendEx += OnGEMReqPPSendExEvent;
            gem.OnGEMRspPPEx += OnGEMRspPPExEvent;
            gem.OnGEMReqPPEx += OnGEMReqPPExEvent;
            gem.OnGEMRspPPSendEx += OnGEMRspPPSendExEvent;
            gem.OnGEMReportedEvent += OnGEMReportedEventEvent;
            gem.OnGEMReportedEvent2 += OnGEMReportedEvent2Event;
            gem.OnGEMRspLoopback += OnGEMRspLoopbackEvent;
            gem.OnGEMReqOnline += OnGEMReqOnlineEvent;
            gem.OnGEMReqOffline += OnGEMReqOfflineEvent;
            gem.OnGEMNotifyPerformanceWarning += OnGEMNotifyPerformanceWarningEvent;
            gem.OnGEMSecondaryMsgReceived += OnGEMSecondaryMsgReceivedEvent;
            gem.OnGEMRecvUserMessage += OnGEMRecvUserMessageEvent;

        }
        private void EventRemoveXGem300Pro()
        {
            var gem = m_XGem300Pro;
            gem.OnSECSMessageReceived -= OnSECSMessageReceivedEvent;
            gem.OnGEMCommStateChanged -= OnGEMCommStateChangedEvent;
            gem.OnGEMControlStateChanged -= OnGEMControlStateChangedEvent;
            gem.OnGEMReqChangeECV -= OnGEMReqChangeECVEvent;
            gem.OnGEMECVChanged -= OnGEMECVChangedEvent;
            gem.OnGEMReqGetDateTime -= OnGEMReqGetDateTimeEvent;
            gem.OnGEMRspGetDateTime -= OnGEMRspGetDateTimeEvent;
            gem.OnGEMReqDateTime -= OnGEMReqDateTimeEvent;
            gem.OnGEMErrorEvent -= OnGEMErrorEventEvent;
            gem.OnGEMReqRemoteCommand -= OnGEMReqRemoteCommandEvent;
            gem.OnGEMReqPPLoadInquire -= OnGEMReqPPLoadInquireEvent;
            gem.OnGEMRspPPLoadInquire -= OnGEMRspPPLoadInquireEvent;
            //gem.OnGEMReqPPSend -= OnGEMReqPPSendEvent;
            gem.OnGEMRspPPSend -= OnGEMRspPPSendEvent;
            gem.OnGEMReqPP -= OnGEMReqPPEvent;
            //gem.OnGEMRspPP -= OnGEMRspPPEvent;
            gem.OnGEMReqPPDelete -= OnGEMReqPPDeleteEvent;
            gem.OnGEMReqPPList -= OnGEMReqPPListEvent;
            gem.OnGEMReqPPFmtSend -= OnGEMReqPPFmtSendEvent;
            gem.OnGEMReqPPFmtSend2 -= OnGEMReqPPFmtSend2Event;
            gem.OnGEMRspPPFmtSend -= OnGEMRspPPFmtSendEvent;
            gem.OnGEMReqPPFmt -= OnGEMReqPPFmtEvent;
            gem.OnGEMRspPPFmt -= OnGEMRspPPFmtEvent;
            gem.OnGEMRspPPFmt2 -= OnGEMRspPPFmt2Event;
            gem.OnGEMRspPPFmtVerification -= OnGEMRspPPFmtVerificationEvent;
            gem.OnGEMTerminalMessage -= OnGEMTerminalMessageEvent;
            gem.OnGEMTerminalMultiMessage -= OnGEMTerminalMultiMessageEvent;
            gem.OnGEMSpoolStateChanged -= OnGEMSpoolStateChangedEvent;
            gem.OnXGEMStateEvent -= OnXGEMStateEventEvent;
            gem.OnGEMRspAllECInfo -= OnGEMRspAllECInfoEvent;
            gem.OnGEMReqPPSendEx -= OnGEMReqPPSendExEvent;
            gem.OnGEMRspPPEx -= OnGEMRspPPExEvent;
            gem.OnGEMReqPPEx -= OnGEMReqPPExEvent;
            gem.OnGEMRspPPSendEx -= OnGEMRspPPSendExEvent;
            gem.OnGEMReportedEvent -= OnGEMReportedEventEvent;
            gem.OnGEMReportedEvent2 -= OnGEMReportedEvent2Event;
            gem.OnGEMRspLoopback -= OnGEMRspLoopbackEvent;
            gem.OnGEMReqOnline -= OnGEMReqOnlineEvent;
            gem.OnGEMReqOffline -= OnGEMReqOfflineEvent;
            gem.OnGEMNotifyPerformanceWarning -= OnGEMNotifyPerformanceWarningEvent;
            gem.OnGEMSecondaryMsgReceived -= OnGEMSecondaryMsgReceivedEvent;
            gem.OnGEMRecvUserMessage -= OnGEMRecvUserMessageEvent;
        }
        #endregion

        /// <summary>
        /// Gem 핸들러가 신규 등록되면 해당 핸들러의 이벤트를 내부 함수에 연결하여 중계 할 수 있도록 설정.
        /// </summary>
        private void EventAdd()
        {
            if (IsRunXGem())
            {
                EventAddXGem();
            }
            else if (IsRunXGem300Pro())
            {
                EventAddXGem300Pro();
            }
            else
            {
                //NONE.
            }
        }
        /// <summary>
        /// 종료시 내부 이벤트 핸들러를 끊도록 설정
        /// </summary>
        private void EventRemove()
        {
            if (IsRunXGem())
            {
                EventRemoveXGem();
            }
            else if (IsRunXGem300Pro())
            {
                EventRemoveXGem300Pro();
            }
            else
            {
                //NONE.
            }
        }

        static byte[] ToBytes(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }
        static string ToString(byte[] bytes)
        {
            return Encoding.ASCII.GetString(bytes);
        }
        #region Implement XGem/XGem300Pro
        public long Init_SECSGEM(string Config)
        {
            if (IsRunXGem())
            {
                return m_XGem.Initialize(Config);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.Initialize(Config);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long Close_SECSGEM(int proberId = 0)
        {
            if (IsRunXGem())
            {
                return m_XGem.Close();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.Close();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public long ClearAlarmOnly(int stageNum = 0)
        {
            //do nothing
            return 0;
        }

        public void ServerConnect(int proberId = 0)
        {
        }

        public bool IsOpened()
        {
            bool retVal = false;
            if (IsRunXGem())
            {
                retVal = true;
            }
            else if (IsRunXGem300Pro())
            {
                retVal = true;
            }
            else
            {
                throw new NotImplementedException();
            }

            return retVal;
        }

        public long LoadMessageRecevieModule(string dllpath, string receivername)
        {
            long retVal = -1;
            return retVal;
        }
        public long GEMRspRemoteCommand(long nMsgId, string sCmd, long nHCAck, long nCount, long[] pnResult)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMRspRemoteCommand(nMsgId, sCmd, nHCAck, nCount, pnResult);
            }
            else if (IsRunXGem300Pro())
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public long GEMRspRemoteCommand2(long nMsgId, string sCmd, long nHCAck, long nCount, string[] psCpName, long[] pnCpAck)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMRspRemoteCommand2(nMsgId, sCmd, nHCAck, nCount, psCpName, pnCpAck);
            }
            else if (IsRunXGem300Pro())
            {

                return m_XGem300Pro.GEMRspRemoteCommand(nMsgId, sCmd, nHCAck, nCount, psCpName, pnCpAck);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMSetPPChanged(long nMode, string sPpid, long nSize, string sBody)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMSetPPChanged(nMode, sPpid, nSize, sBody); ;
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMSetPPChanged(nMode, sPpid, nSize, XGEM.ToBytes(sBody)); //? string to byte[]
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public long GEMRspPP(long nMsgId, string sPpid, string sBody)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMRspPP(nMsgId, sPpid, sBody);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMRspPP(nMsgId, sPpid, XGEM.ToBytes(sBody));//? string to byte[]
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public long GEMReqPPSend(string sPpid, string sBody)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMReqPPSend(sPpid, sBody);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMReqPPSend(sPpid, XGEM.ToBytes(sBody));//? string to byte[]
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public long GEMEQInitialized(long nInitType)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMEQInitialized(nInitType);
            }
            else if (IsRunXGem300Pro())
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public long GEMRsqOffline(long nMsgId, long nAck)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMRsqOffline(nMsgId, nAck);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMRspOffline(nMsgId, nAck); //? 오타 수정
            }
            else
            {
                throw new NotImplementedException();
            }
        }


        public long SetList(long nObjectID, long nItemCount)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetList(nObjectID, nItemCount);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetListItem(nObjectID, nItemCount);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetList(long nObjectID, ref long pnItemCount)
        {
            if (IsRunXGem())
            {
                return m_XGem.GetList(nObjectID, ref pnItemCount);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetListItem(nObjectID, ref pnItemCount);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public long SetBinary(long nObjectID, byte[] pnValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetBinary(nObjectID, pnValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetBinaryItem(nObjectID, pnValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetBinary(long nObjectID, byte nValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetBinary(nObjectID, nValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetBinaryItem(nObjectID, nValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetBoolItems(long nObjectID, bool[] pnValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetBool(nObjectID, pnValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetBoolItem(nObjectID, pnValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetBool(long nObjectID, bool nValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetBool(nObjectID, nValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetBoolItem(nObjectID, nValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetU1(long nObjectID, byte[] pnValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetU1(nObjectID, pnValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetUint1Item(nObjectID, pnValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetU1(long nObjectID, byte nValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetU1(nObjectID, nValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetUint1Item(nObjectID, nValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetU2(long nObjectID, ushort[] pnValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetU2(nObjectID, pnValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetUint2Item(nObjectID, pnValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetU2(long nObjectID, ushort nValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetU2(nObjectID, nValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetUint2Item(nObjectID, nValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetU4(long nObjectID, uint[] prValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetU4(nObjectID, prValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetUint4Item(nObjectID, prValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetU4(long nObjectID, uint rValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetU4(nObjectID, rValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetUint4Item(nObjectID, rValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetU8(long nObjectID, uint[] prValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetU8(nObjectID, prValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetUint8Item(nObjectID, Array.ConvertAll(prValue, Convert.ToUInt64));
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetU8(long nObjectID, uint rValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetU8(nObjectID, rValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetUint8Item(nObjectID, rValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetI1(long nObjectID, sbyte[] pnValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetI1(nObjectID, pnValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetInt1Item(nObjectID, pnValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetI1(long nObjectID, sbyte nValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetI1(nObjectID, nValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetInt1Item(nObjectID, nValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetI2(long nObjectID, short[] pnValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetI2(nObjectID, pnValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetInt2Item(nObjectID, pnValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetI2(long nObjectID, short nValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetI2(nObjectID, nValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetInt2Item(nObjectID, nValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetI4(long nObjectID, int[] plValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetI4(nObjectID, plValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetInt4Item(nObjectID, plValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetI4(long nObjectID, int lValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetI4(nObjectID, lValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetInt4Item(nObjectID, lValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetI8(long nObjectID, int[] plValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetI8(nObjectID, plValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetInt8Item(nObjectID, Array.ConvertAll(plValue, Convert.ToInt64));
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetI8(long nObjectID, int lValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetI8(nObjectID, lValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetInt8Item(nObjectID, lValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetF4(long nObjectID, float[] prValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetF4(nObjectID, prValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetFloat4Item(nObjectID, prValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetF4(long nObjectID, float rValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetF4(nObjectID, rValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetFloat4Item(nObjectID, rValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetF8(long nObjectID, double[] prValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetF8(nObjectID, prValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetFloat8Item(nObjectID, prValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetF8(long nObjectID, double rValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetF8(nObjectID, rValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetFloat8Item(nObjectID, rValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetAscii(long nObjectID, string pszValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetAscii(nObjectID, pszValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetStringItem(nObjectID, pszValue); //? Ascii to string
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetBinary(long nObjectID, ref byte[] pnValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.GetBinary(nObjectID, ref pnValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetBinaryItem(nObjectID, ref pnValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetBool(long nObjectID, ref bool[] pnValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.GetBool(nObjectID, ref pnValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetBoolItem(nObjectID, ref pnValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetU1(long nObjectID, ref byte[] pnValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.GetU1(nObjectID, ref pnValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetUint1Item(nObjectID, ref pnValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetU2(long nObjectID, ref ushort[] pnValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.GetU2(nObjectID, ref pnValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetUint2Item(nObjectID, ref pnValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetU4(long nObjectID, ref uint[] prValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.GetU4(nObjectID, ref prValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetUint4Item(nObjectID, ref prValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetU8(long nObjectID, ref uint[] prValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.GetU8(nObjectID, ref prValue);
            }
            else if (IsRunXGem300Pro())
            {
                ulong[] temp = Array.ConvertAll(prValue, item => (ulong)item);
                long result = m_XGem300Pro.GetUint8Item(nObjectID, ref temp);
                prValue = Array.ConvertAll(temp, item => (uint)item);
                return result; //?
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetI1(long nObjectID, ref sbyte[] pnValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.GetI1(nObjectID, ref pnValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetInt1Item(nObjectID, ref pnValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetI2(long nObjectID, ref short[] pnValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.GetI2(nObjectID, ref pnValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetInt2Item(nObjectID, ref pnValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetI4(long nObjectID, ref int[] pnValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.GetI4(nObjectID, ref pnValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetInt4Item(nObjectID, ref pnValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetI8(long nObjectID, ref int[] pnValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.GetI8(nObjectID, ref pnValue);
            }
            else if (IsRunXGem300Pro())
            {
                long[] temp = Array.ConvertAll(pnValue, item => (long)item);
                long result = m_XGem300Pro.GetInt8Item(nObjectID, ref temp);
                pnValue = Array.ConvertAll(temp, item => (int)item);
                return result; //?                 
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetF4(long nObjectID, ref float[] prValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.GetF4(nObjectID, ref prValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetFloat4Item(nObjectID, ref prValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetF8(long nObjectID, ref double[] prValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.GetF8(nObjectID, ref prValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetFloat8Item(nObjectID, ref prValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetAscii(long nObjectID, ref string psValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.GetAscii(nObjectID, ref psValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetStringItem(nObjectID, ref psValue);//?Ascii to String
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public string GetIP()
        {
            if (IsRunXGem())
            {
                return m_XGem.GetIP();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetIP();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public void SetIP(string sNewValue)
        {
            if (IsRunXGem())
            {
                m_XGem.SetIP(sNewValue);
            }
            else if (IsRunXGem300Pro())
            {
                m_XGem300Pro.SetIP(sNewValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetPort()
        {
            if (IsRunXGem())
            {
                return m_XGem.GetPort();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetPort();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public void SetPort(long nNewValue)
        {
            if (IsRunXGem())
            {
                m_XGem.SetPort(nNewValue);
            }
            else if (IsRunXGem300Pro())
            {
                m_XGem300Pro.SetPort(nNewValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public bool GetActive()
        {
            if (IsRunXGem())
            {
                return m_XGem.GetActive();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetActive();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public void SetActive(bool bNewValue)
        {
            if (IsRunXGem())
            {
                m_XGem.SetActive(bNewValue);
            }
            else if (IsRunXGem300Pro())
            {
                m_XGem300Pro.SetActive(bNewValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMEnableLog(long bEnabled)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMEnableLog(bEnabled);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMEnableLog(bEnabled);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMSetLogOption(string sDriectory, string sPrefix, string sExtension, long nKeepDay, long bMakeDailyLog, long bMakeSubDirectory)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMSetLogOption(sDriectory, sPrefix, sExtension, nKeepDay, bMakeDailyLog, bMakeSubDirectory);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMSetLogOption(sDriectory, sPrefix, sExtension, nKeepDay, bMakeDailyLog, bMakeSubDirectory);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long Initialize(string sCfg)
        {
            if (IsRunXGem())
            {
                return m_XGem.Initialize(sCfg);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.Initialize(sCfg);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long Start()
        {
            if (IsRunXGem())
            {
                return m_XGem.Start();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.Start();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long Stop()
        {
            if (IsRunXGem())
            {
                return m_XGem.Stop();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.Stop();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long Close()
        {
            if (IsRunXGem())
            {
                return m_XGem.Close();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.Close();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long MakeObject(ref long nObjectID)
        {
            if (IsRunXGem())
            {
                return m_XGem.MakeObject(ref nObjectID);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.MakeObject(ref nObjectID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMSetParam(string sPName, string sPValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMSetParam(sPName, sPValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMSetParam(sPName, sPValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMGetParam(string sPName, ref string sPValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMGetParam(sPName, ref sPValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMGetParam(sPName, ref sPValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMSetEstablish(long bState)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMSetEstablish(bState);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMSetEstablish(bState);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMReqOffline()
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMReqOffline();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMReqOffline();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMReqLocal()
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMReqLocal();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMReqLocal();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMReqRemote()
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMReqRemote();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMReqRemote();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMSetAlarm(long nID, long nState, int stageNum = 0)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMSetAlarm(nID, nState);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMSetAlarm(nID, nState);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMRspRemoteCommand(long nMsgId, string sCmd, long nHCAck, long nCount, string[] psCpName, long[] pnCpAck)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMRspRemoteCommand2(nMsgId, sCmd, nHCAck, nCount, psCpName, pnCpAck);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMRspRemoteCommand(nMsgId, sCmd, nHCAck, nCount, psCpName, pnCpAck);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMRspChangeECV(long nMsgId, long nResult)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMRspChangeECV(nMsgId, nResult);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMRspChangeECV(nMsgId, nResult);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMSetPPChanged(long nMode, string sPpid, long nSize, byte[] baBody)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMSetPPChanged(nMode, sPpid, nSize, XGEM.ToString(baBody));//? byte to string Encoding ascii
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMSetPPChanged(nMode, sPpid, nSize, baBody);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMSetPPFmtChanged(long nMode, string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMSetPPFmtChanged(nMode, sPpid, sMdln, sSoftRev, nCount, psCCode, pnParamCount, psParamNames);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMSetPPFmtChanged(nMode, sPpid, sMdln, sSoftRev, nCount, psCCode, pnParamCount, psParamNames);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMSetECVChanged(long nCount, long[] pnEcIds, string[] psEcVals, int stageNum = -1)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMSetECVChanged(nCount, pnEcIds, psEcVals);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMSetECVChanged(nCount, pnEcIds, psEcVals);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long RangeCheck(long[] pnIds)
        {
            if (IsRunXGem())
            {
                return m_XGem.RangeCheck(pnIds);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.RangeCheck(pnIds);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMRspPPDelete(long nMsgId, long nCount, string[] psPpids, long nResult)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMRspPPDelete(nMsgId, nCount, psPpids, nResult);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMRspPPDelete(nMsgId, nCount, psPpids, nResult);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMRspPPList(long nMsgId, long nCount, string[] psPpids)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMRspPPList(nMsgId, nCount, psPpids);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMRspPPList(nMsgId, nCount, psPpids);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMRspPPFmt(long nMsgId, string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMRspPPFmt(nMsgId, sPpid, sMdln, sSoftRev, nCount, psCCode, pnParamCount, psParamNames);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMRspPPFmt(nMsgId, sPpid, sMdln, sSoftRev, nCount, psCCode, pnParamCount, psParamNames);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMReqPPLoadInquire(string sPpid, long nLength)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMReqPPLoadInquire(sPpid, nLength);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMReqPPLoadInquire(sPpid, nLength);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMReqPPFmtSend(string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMReqPPFmtSend(sPpid, sMdln, sSoftRev, nCount, psCCode, pnParamCount, psParamNames);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMReqPPFmtSend(sPpid, sMdln, sSoftRev, nCount, psCCode, pnParamCount, psParamNames);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMRspPPLoadInquire(long nMsgId, string sPpid, long nResult)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMRspPPLoadInquire(nMsgId, sPpid, nResult);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMRspPPLoadInquire(nMsgId, sPpid, nResult);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMRspPPFmtSend(long nMsgId, string sPpid, long nResult)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMRspPPFmtSend(nMsgId, sPpid, nResult);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMRspPPFmtSend(nMsgId, sPpid, nResult);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMReqPPFmt(string sPpid)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMReqPPFmt(sPpid);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMReqPPFmt(sPpid);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMReqPPFmtVerification(string sPpid, long nCount, long[] pnAck, string[] psSeqNumber, string[] psError)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMReqPPFmtVerification(sPpid, nCount, pnAck, psSeqNumber, psError);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMReqPPFmtVerification(sPpid, nCount, pnAck, psSeqNumber, psError);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMRspPP(long nMsgId, string sPpid, byte[] baBody)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMRspPP(nMsgId, sPpid, XGEM.ToString(baBody));//? byte to string Encoding ascii
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMRspPP(nMsgId, sPpid, baBody);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMReqPPSend(string sPpid, byte[] baBody)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMReqPPSend(sPpid, XGEM.ToString(baBody));//? byte to string Encoding ascii
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMReqPPSend(sPpid, baBody);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMReqPP(string sPpid)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMReqPP(sPpid);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMReqPP(sPpid);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMSetTerminalMessage(long nTid, string sMsg)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMSetTerminalMessage(nTid, sMsg);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMSetTerminalMessage(nTid, sMsg);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMRspDateTime(long nMsgId, long nResult)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMRspDateTime(nMsgId, nResult);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMRspDateTime(nMsgId, nResult);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMRspGetDateTime(long nMsgId, string sSystemTime)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMRspGetDateTime(nMsgId, sSystemTime);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMRspGetDateTime(nMsgId, sSystemTime);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMSetVariable(long nCount, long[] pnVid, string[] psValue, int stageNum = -1, bool immediatelyUpdate = false)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMSetVariable(nCount, pnVid, psValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMSetVariable(nCount, pnVid, psValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMSetVarName(long nCount, string[] psVidName, string[] psValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMSetVarName(nCount, psVidName, psValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMSetVarName(nCount, psVidName, psValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMGetVariable(long nCount, ref long[] pnVID, ref string[] psValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMGetVariable(nCount, ref pnVID, ref psValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMGetVariable(nCount, ref pnVID, ref psValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMGetVariables(ref long pnObjectID, long nVid)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
                //return m_XGem.GEMGetVariables(ref pnObjectID, nVid);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMGetVariables(ref pnObjectID, nVid);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMSetEventEx(long nEventID, long nCount, long[] pnVID, string[] psValue)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
                // return m_XGem.GEMSetEventEx(nEventID, nCount, pnVID, psValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMSetEventEx(nEventID, nCount, pnVID, psValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMGetVarName(long nCount, ref string[] psVIDName, ref string[] psValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMGetVarName(nCount, ref psVIDName, ref psValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMGetVarName(nCount, ref psVIDName, ref psValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetListItem(long nObjectID, long nItemCount)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetList(nObjectID, nItemCount);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetListItem(nObjectID, nItemCount);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMSetVariables(long nObjectID, long nVid, int stageNum = -1, bool immediatelyUpdate = false)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMSetVariables(nObjectID, nVid);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMSetVariables(nObjectID, nVid);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SendSECSMessage(long nObjectID, long nStream, long nFunction, long nSysbyte)
        {
            if (IsRunXGem())
            {
                return m_XGem.SendSECSMessage(nObjectID, nStream, nFunction, nSysbyte);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SendSECSMessage(nObjectID, nStream, nFunction, nSysbyte);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetListItem(long nObjectID, ref long pnItemCount)
        {
            if (IsRunXGem())
            {
                return m_XGem.GetList(nObjectID, ref pnItemCount);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetListItem(nObjectID, ref pnItemCount);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMRspPPSend(long nMsgId, string sPpid, long nResult)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMRspPPSend(nMsgId, sPpid, nResult);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMRspPPSend(nMsgId, sPpid, nResult);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMReqGetDateTime()
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMReqGetDateTime();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMReqGetDateTime();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMSetEvent(long nEventID, int stageNum = -1)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMSetEvent(nEventID);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMSetEvent(nEventID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMSetSpecificMessage(long nObjectID, string sMsgName)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMSetSpecificMessage(nObjectID, sMsgName);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMSetSpecificMessage(nObjectID, sMsgName);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMGetSpecificMessage(long nSObjectID, ref long pnRObjectID, string sMsgName)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMGetSpecificMessage(nSObjectID, ref pnRObjectID, sMsgName);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMGetSpecificMessage(nSObjectID, ref pnRObjectID, sMsgName);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetAllStringItem(long nObjectID, ref string psValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.GetAllStringItem(nObjectID, ref psValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetAllStringItem(nObjectID, ref psValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetAllStringItem(long nObjectID, string sValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetAllStringItem(nObjectID, sValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetAllStringItem(nObjectID, sValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CloseObject(long nObjectID)
        {
            if (IsRunXGem())
            {
                return m_XGem.CloseObject(nObjectID);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CloseObject(nObjectID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMReqAllECInfo()
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMReqAllECInfo();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMReqAllECInfo();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetBinaryItems(long nObjectID, byte[] pnValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetBinary(nObjectID, pnValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetBinaryItem(nObjectID, pnValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetBinaryItem(long nObjectID, byte nValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetBinary(nObjectID, nValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetBinaryItem(nObjectID, nValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetBoolItem(long nObjectID, bool nValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetBool(nObjectID, nValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetBoolItem(nObjectID, nValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetUint1Items(long nObjectID, byte[] pnValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetU1(nObjectID, pnValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetUint1Item(nObjectID, pnValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetUint1Item(long nObjectID, byte nValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetU1(nObjectID, nValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetUint1Item(nObjectID, nValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetUint2Items(long nObjectID, ushort[] pnValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetU2(nObjectID, pnValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetUint2Item(nObjectID, pnValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetUint2Item(long nObjectID, ushort nValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetU2(nObjectID, nValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetUint2Item(nObjectID, nValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetUint4Items(long nObjectID, uint[] prValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetU4(nObjectID, prValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetUint4Item(nObjectID, prValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetUint4Item(long nObjectID, uint rValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetU4(nObjectID, rValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetUint4Item(nObjectID, rValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetUint8Items(long nObjectID, ulong[] prValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetU8(nObjectID, Array.ConvertAll(prValue, Convert.ToUInt32));
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetUint8Item(nObjectID, prValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetUint8Item(long nObjectID, ulong rValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetU8(nObjectID, (uint)rValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetUint8Item(nObjectID, rValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetInt1Items(long nObjectID, sbyte[] pnValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetI1(nObjectID, pnValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetInt1Item(nObjectID, pnValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetInt1Item(long nObjectID, sbyte nValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetI1(nObjectID, nValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetInt1Item(nObjectID, nValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetInt2Items(long nObjectID, short[] pnValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetI2(nObjectID, pnValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetInt2Item(nObjectID, pnValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetInt2Item(long nObjectID, short nValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetI2(nObjectID, nValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetInt2Item(nObjectID, nValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetInt4Items(long nObjectID, int[] plValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetI4(nObjectID, plValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetInt4Item(nObjectID, plValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetInt4Item(long nObjectID, int lValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetI4(nObjectID, lValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetInt4Item(nObjectID, lValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetInt8Items(long nObjectID, long[] plValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetI8(nObjectID, Array.ConvertAll(plValue, Convert.ToInt32));
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetInt8Item(nObjectID, plValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetInt8Item(long nObjectID, long lValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetI8(nObjectID, (int)lValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetInt8Item(nObjectID, lValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetFloat4Items(long nObjectID, float[] prValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetF4(nObjectID, prValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetFloat4Item(nObjectID, prValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetFloat4Item(long nObjectID, float rValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetF4(nObjectID, rValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetFloat4Item(nObjectID, rValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetFloat8Items(long nObjectID, double[] prValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetF8(nObjectID, prValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetFloat8Item(nObjectID, prValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetFloat8Item(long nObjectID, double rValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetF8(nObjectID, rValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetFloat8Item(nObjectID, rValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SetStringItem(long nObjectID, string pszValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.SetAscii(nObjectID, pszValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SetStringItem(nObjectID, pszValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetBinaryItem(long nObjectID, ref byte[] pnValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.GetBinary(nObjectID, ref pnValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetBinaryItem(nObjectID, ref pnValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetBoolItem(long nObjectID, ref bool[] pnValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.GetBool(nObjectID, ref pnValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetBoolItem(nObjectID, ref pnValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetUint1Item(long nObjectID, ref byte[] pnValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.GetU1(nObjectID, ref pnValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetUint1Item(nObjectID, ref pnValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetUint2Item(long nObjectID, ref ushort[] pnValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.GetU2(nObjectID, ref pnValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetUint2Item(nObjectID, ref pnValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetUint4Item(long nObjectID, ref uint[] prValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.GetU4(nObjectID, ref prValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetUint4Item(nObjectID, ref prValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetUint8Item(long nObjectID, ref ulong[] prValue)
        {
            if (IsRunXGem())
            {
                uint[] temp = Array.ConvertAll(prValue, item => (uint)item);
                long result = m_XGem.GetU8(nObjectID, ref temp);
                prValue = Array.ConvertAll(temp, item => (ulong)item);
                return result;
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetUint8Item(nObjectID, ref prValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetInt1Item(long nObjectID, ref sbyte[] pnValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.GetI1(nObjectID, ref pnValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetInt1Item(nObjectID, ref pnValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetInt2Item(long nObjectID, ref short[] pnValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.GetI2(nObjectID, ref pnValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetInt2Item(nObjectID, ref pnValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetInt4Item(long nObjectID, ref int[] pnValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.GetI4(nObjectID, ref pnValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetInt4Item(nObjectID, ref pnValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetInt8Item(long nObjectID, ref long[] pnValue)
        {
            if (IsRunXGem())
            {
                int[] temp = Array.ConvertAll(pnValue, item => (int)item);
                long result = m_XGem.GetI8(nObjectID, ref temp);
                pnValue = Array.ConvertAll(temp, item => (long)item);
                return result;
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetInt8Item(nObjectID, ref pnValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetFloat4Item(long nObjectID, ref float[] prValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.GetF4(nObjectID, ref prValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetFloat4Item(nObjectID, ref prValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetFloat8Item(long nObjectID, ref double[] prValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.GetF8(nObjectID, ref prValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetFloat8Item(nObjectID, ref prValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetStringItem(long nObjectID, ref string psValue)
        {
            if (IsRunXGem())
            {
                return m_XGem.GetAscii(nObjectID, ref psValue);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetStringItem(nObjectID, ref psValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public bool ReadXGemInfoFromRegistry()
        {
            if (IsRunXGem())
            {
                return m_XGem.ReadXGemInfoFromRegistry();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.ReadXGemInfoFromRegistry();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public int RunProcess(string sName, string sCfgPath, string sPassword)
        {
            if (IsRunXGem())
            {
                return m_XGem.RunProcess(sName, sCfgPath, sPassword);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.RunProcess(sName, sCfgPath, sPassword);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMReqPPSendEx(string sPpid, string sRecipePath)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMReqPPSendEx(sPpid, sRecipePath);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMReqPPSendEx(sPpid, sRecipePath);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMRspPPEx(long nMsgId, string sPpid, string sRecipePath)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMRspPPEx(nMsgId, sPpid, sRecipePath);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMRspPPEx(nMsgId, sPpid, sRecipePath);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMRspPPSendEx(long nMsgId, string sPpid, string sRecipePath, long nResult)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMRspPPSendEx(nMsgId, sPpid, sRecipePath, nResult);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMRspPPSendEx(nMsgId, sPpid, sRecipePath, nResult);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMReqPPEx(string sPpid, string sRecipePath)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMReqPPEx(sPpid, sRecipePath);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMReqPPEx(sPpid, sRecipePath);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMSetVariableEx(long nObjectID, long nVid)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMSetVariableEx(nObjectID, nVid);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMSetVariableEx(nObjectID, nVid);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMReqLoopback(long nCount, long[] pnAbs)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMReqLoopback(nCount, pnAbs);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMReqLoopback(nCount, pnAbs);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMSetEventEnable(long nCount, long[] pnCEIDs, long nEnable)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMSetEventEnable(nCount, pnCEIDs, nEnable);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMSetEventEnable(nCount, pnCEIDs, nEnable);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMGetEventEnable(long nCount, long[] pnCEIDs, ref long[] nEnable)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMGetEventEnable(nCount, pnCEIDs, ref nEnable);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMGetEventEnable(nCount, pnCEIDs, ref nEnable);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMSetAlarmEnable(long nCount, long[] pnALIDs, long nEnable)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMSetAlarmEnable(nCount, pnALIDs, nEnable);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMSetAlarmEnable(nCount, pnALIDs, nEnable);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMGetAlarmEnable(long nCount, long[] pnALIDs, ref long[] nEnable)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMGetAlarmEnable(nCount, pnALIDs, ref nEnable);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMGetAlarmEnable(nCount, pnALIDs, ref nEnable);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMGetAlarmInfo(long nCount, long[] pnALIDs, ref long[] pnALCDs, ref string[] psALTXs)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMGetAlarmInfo(nCount, pnALIDs, ref pnALCDs, ref psALTXs);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMGetAlarmInfo(nCount, pnALIDs, ref pnALCDs, ref psALTXs);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMGetSVInfo(long nCount, long[] pnVIDs, ref string[] psMins, ref string[] psMaxs)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMGetSVInfo(nCount, pnVIDs, ref psMins, ref psMaxs);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMGetSVInfo(nCount, pnVIDs, ref psMins, ref psMaxs);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMGetECVInfo(long nCount, long[] pnECIDs, ref string[] psNames, ref string[] psDefs, ref string[] psMins, ref string[] psMaxs, ref string[] psUnits)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMGetECVInfo(nCount, pnECIDs, ref psNames, ref psDefs, ref psMins, ref psMaxs, ref psUnits);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMGetECVInfo(nCount, pnECIDs, ref psNames, ref psDefs, ref psMins, ref psMaxs, ref psUnits);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMRspOffline(long nMsgId, long nAck)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
                //return m_XGem.GEMRsqOffline(nMsgId, nAck);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMRspOffline(nMsgId, nAck);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMRspOnline(long nMsgId, long nAck)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMRspOnline(nMsgId, nAck);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMRspOnline(nMsgId, nAck);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMReqHostOffline()
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMReqHostOffline();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMReqHostOffline();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMReqStartPolling(string sName, long nScanTime)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMReqStartPolling(sName, nScanTime);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMReqStartPolling(sName, nScanTime);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMReqStopPolling(string sName)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMReqStopPolling(sName);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMReqStopPolling(sName);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMReqPPFmtSend2(string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames, string[] psParamValues)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMReqPPFmtSend2(sPpid, sMdln, sSoftRev, nCount, psCCode, pnParamCount, psParamNames, psParamValues);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMReqPPFmtSend2(sPpid, sMdln, sSoftRev, nCount, psCCode, pnParamCount, psParamNames, psParamValues);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GEMRspPPFmt2(long nMsgId, string sPpid, string sMdln, string sSoftRev, long nCount, string[] psCCode, long[] pnParamCount, string[] psParamNames, string[] psParamValues)
        {
            if (IsRunXGem())
            {
                return m_XGem.GEMRspPPFmt2(nMsgId, sPpid, sMdln, sSoftRev, nCount, psCCode, pnParamCount, psParamNames, psParamValues);
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GEMRspPPFmt2(nMsgId, sPpid, sMdln, sSoftRev, nCount, psCCode, pnParamCount, psParamNames, psParamValues);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long SendUserMessage(long nObjectID, string sCommand, long nTransID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.SendUserMessage(nObjectID, sCommand, nTransID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetCurrentItemInfo(long nObjectID, ref long pnItemType, ref long pnItemCount)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetCurrentItemInfo(nObjectID, ref pnItemType, ref pnItemCount);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public long CMSSetCarrierLocationInfo(string sLocID, string sCarrierID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSSetCarrierLocationInfo(sLocID, sCarrierID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSSetPresenceSensor(string sLocID, long nState)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException(); 
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSSetPresenceSensor(sLocID, nState);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSSetCarrierOnOff(string sLocID, long nState)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSSetCarrierOnOff(sLocID, nState);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSSetPIOSignalState(string sLocID, long nSignal, long nState)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSSetPIOSignalState(sLocID, nSignal, nState);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSSetReadyToLoad(string sLocID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSSetReadyToLoad(sLocID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSSetReadyToUnload(string sLocID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSSetReadyToUnload(sLocID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSSetCarrierMovement(string sLocID, string sCarrierID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSSetCarrierMovement(sLocID, sCarrierID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSSetCarrierAccessing(string sLocID, long nState, string sCarrierID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSSetCarrierAccessing(sLocID, nState, sCarrierID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSSetCarrierID(string sLocID, string sCarrierID, long nResult)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSSetCarrierID(sLocID, sCarrierID, nResult);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSSetSlotMap(string sLocID, string sSlotMap, string sCarrierID, long nResult)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException(); 
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSSetSlotMap(sLocID, sSlotMap, sCarrierID, nResult);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSReqBind(string sLocID, string sCarrierID, string sSlotMap)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException(); 
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSReqBind(sLocID, sCarrierID, sSlotMap);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSReqCancelBind(string sLocID, string sCarrierID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSReqCancelBind(sLocID, sCarrierID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSReqCancelCarrier(string sLocID, string sCarrierID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException(); 
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSReqCancelCarrier(sLocID, sCarrierID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSRspCancelCarrier(long nMsgId, string sLocID, string sCarrierID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSRspCancelCarrier(nMsgId, sLocID, sCarrierID, nResult, nErrCount, pnErrCode, psErrText);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSRspCancelCarrierAtPort(long nMsgId, string sLocID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSRspCancelCarrierAtPort(nMsgId, sLocID, nResult, nErrCount, pnErrCode, psErrText);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSReqCarrierIn(string sLocID, string sCarrierID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSReqCarrierIn(sLocID, sCarrierID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSReqProceedCarrier(string sLocID, string sCarrierID, string sSlotMap, long nCount, string[] psLotID, string[] psSubstrateID, string sUsage)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSReqProceedCarrier(sLocID, sCarrierID, sSlotMap, nCount, psLotID, psSubstrateID, sUsage);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSReqChangeServiceStatus(string sLocID, long nState)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSReqChangeServiceStatus(sLocID, nState);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSReqCarrierOut(string sLocID, string sCarrierID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSReqCarrierOut(sLocID, sCarrierID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSRspCarrierOut(long nMsgId, string sLocID, string sCarrierID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSRspCarrierOut(nMsgId, sLocID, sCarrierID, nResult, nErrCount, pnErrCode, psErrText);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSReqChangeAccess(long nMode, string sLocID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException(); 
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSReqChangeAccess(nMode, sLocID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSRspChangeAccess(long nMsgId, long nMode, long nResult, long nErrCount, string[] psLocID, long[] pnErrCode, string[] psErrText)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSRspChangeAccess(nMsgId, nMode, nResult, nErrCount, psLocID, pnErrCode, psErrText);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSRspChangeServiceStatus(long nMsgId, string sLocID, long nState, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSRspChangeServiceStatus(nMsgId, sLocID, nState, nResult, nErrCount, pnErrCode, psErrText);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSGetAllCarrierInfo(ref long pnMsgId, ref long pnCount)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSGetAllCarrierInfo(ref pnMsgId, ref pnCount);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSDelCarrierInfo(string sCarrierID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSDelCarrierInfo(sCarrierID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetCarrierID(long nMsgId, long nIndex, ref string psCarrierID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetCarrierID(nMsgId, nIndex, ref psCarrierID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetCarrierLocID(long nMsgId, long nIndex, ref string psLocID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetCarrierLocID(nMsgId, nIndex, ref psLocID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetCarrierIDStatus(long nMsgId, long nIndex, ref long pnState)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetCarrierIDStatus(nMsgId, nIndex, ref pnState);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetCarrierSlotMapStatus(long nMsgId, long nIndex, ref long pnState)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetCarrierSlotMapStatus(nMsgId, nIndex, ref pnState);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetCarrierAccessingStatus(long nMsgId, long nIndex, ref long pnState)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetCarrierAccessingStatus(nMsgId, nIndex, ref pnState);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetCarrierContentsMapCount(long nMsgId, long nIndex, ref long pnCount)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetCarrierContentsMapCount(nMsgId, nIndex, ref pnCount);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetCarrierContentsMap(long nMsgId, long nIndex, long nCount, ref string[] psLotID, ref string[] psSubstrateID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetCarrierContentsMap(nMsgId, nIndex, nCount, ref psLotID, ref psSubstrateID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetCarrierSlotMap(long nMsgId, long nIndex, ref string psSlotMap)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetCarrierSlotMap(nMsgId, nIndex, ref psSlotMap);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetCarrierUsage(long nMsgId, long nIndex, ref string psUsage)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetCarrierUsage(nMsgId, nIndex, ref psUsage);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetCarrierClose(long nMsgId)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetCarrierClose(nMsgId);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSDelAllCarrierInfo()
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSDelAllCarrierInfo();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSSetCarrierInfo(string sCarrierID, string sLocID, long nIdStatus, long nSlotMapStatus, long nAccessingStatus, string sSlotMap, long nContentsMapCount, string[] psLotID, string[] psSubstrateID, string sUsage)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSSetCarrierInfo(sCarrierID, sLocID, nIdStatus, nSlotMapStatus, nAccessingStatus, sSlotMap, nContentsMapCount, psLotID, psSubstrateID, sUsage);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSRspCarrierTagReadData(long nMsgId, string sLocID, string sCarrierID, string sData, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException(); 
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSRspCarrierTagReadData(nMsgId, sLocID, sCarrierID, sData, nResult, nErrCount, pnErrCode, psErrText);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSRspCarrierTagWriteData(long nMsgId, string sLocID, string sCarrierID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSRspCarrierTagWriteData(nMsgId, sLocID, sCarrierID, nResult, nErrCount, pnErrCode, psErrText);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSRspCarrierRelease(long nMsgId, string sLocID, string sCarrierID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSRspCarrierRelease(nMsgId, sLocID, sCarrierID, nResult, nErrCount, pnErrCode, psErrText);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSSetBufferCapacityChanged(string sPartID, string sPartType, long nAPPCapacity, long nPCapacity, long nUnPCapacity)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSSetBufferCapacityChanged(sPartID, sPartType, nAPPCapacity, nPCapacity, nUnPCapacity);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSRspCarrierIn(long nMsgId, string sLocID, string sCarrierID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSRspCarrierIn(nMsgId, sLocID, sCarrierID, nResult, nErrCount, pnErrCode, psErrText);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSReqCarrierReCreate(string sLocID, string sCarrierID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException(); 
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSReqCarrierReCreate(sLocID, sCarrierID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSSetMaterialArrived(string sMaterialID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSSetMaterialArrived(sMaterialID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSSetCarrierIDStatus(string sCarrierID, long nState)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSSetCarrierIDStatus(sCarrierID, nState);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSSetSlotMapStatus(string sCarrierID, long nState)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSSetSlotMapStatus(sCarrierID, nState);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSSetLPInfo(string sLocID, long nTransferState, long nAccessMode, long nReservationState, long nAssociationState, string sCarrierID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSSetLPInfo(sLocID, nTransferState, nAccessMode, nReservationState, nAssociationState, sCarrierID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSSetSubstrateCount(string sCarrierID, long nSubstCount)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSSetSubstrateCount(sCarrierID, nSubstCount);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSSetUsage(string sCarrierID, string sUsage)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSSetUsage(sCarrierID, sUsage);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSSetCarrierOutStart(string sLocID, string sCarrierID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSSetCarrierOutStart(sLocID, sCarrierID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSRspCancelCarrierOut(long nMsgId, string sCarrierID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSRspCancelCarrierOut(nMsgId, sCarrierID, nResult, nErrCount, pnErrCode, psErrText);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CMSSetTransferReady(string sLocID, long nState)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException(); 
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CMSSetTransferReady(sLocID, nState);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long PJReqCommand(long nCommand, string sPJobID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.PJReqCommand(nCommand, sPJobID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long PJReqGetJob(string sPJobID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException(); 
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.PJReqGetJob(sPJobID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long PJReqGetAllJobID()
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.PJReqGetAllJobID();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long PJReqCreate(string sPJobID, long nMtrlFormat, long nAutoStart, long nMtrlOrder, long nMtrlCount, string[] psMtrlID, string[] psSlotInfo, long nRcpMethod, string sRcpID, long nRcpParCount, string[] psRcpParName, string[] psRcpParValue)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.PJReqCreate(sPJobID, nMtrlFormat, nAutoStart, nMtrlOrder, nMtrlCount, psMtrlID, psSlotInfo, nRcpMethod, sRcpID, nRcpParCount, psRcpParName, psRcpParValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long PJRspVerify(long nMsgID, long nPJobCount, string[] psPJobID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.PJRspVerify(nMsgID, nPJobCount, psPJobID, nResult, nErrCount, pnErrCode, psErrText);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long PJSetState(string sPJobID, long nState)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.PJSetState(sPJobID, nState);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long PJSettingUpCompt(string sPJobID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException(); 
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.PJSettingUpCompt(sPJobID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long PJGetAllJobInfo(ref long pnObjID, ref long pnPJobCount)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.PJGetAllJobInfo(ref pnObjID, ref pnPJobCount);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetPRJobID(long nObjID, long nIndex, ref string psPJobID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetPRJobID(nObjID, nIndex, ref psPJobID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetPRJobState(long nObjID, long nIndex, ref long pnState)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetPRJobState(nObjID, nIndex, ref pnState);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetPRJobAutoStart(long nObjID, long nIndex, ref long pnAutoStart)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetPRJobAutoStart(nObjID, nIndex, ref pnAutoStart);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetPRJobMtrlOrder(long nObjID, long nIndex, ref long pnMtrlOrder)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException(); 
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetPRJobMtrlOrder(nObjID, nIndex, ref pnMtrlOrder);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetPRJobMtrlFormat(long nObjID, long nIndex, ref long pnMtrlFormat)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException(); 
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetPRJobMtrlFormat(nObjID, nIndex, ref pnMtrlFormat);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetPRJobCarrierCount(long nObjID, long nIndex, ref long pnCount)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException(); 
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetPRJobCarrierCount(nObjID, nIndex, ref pnCount);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetPRJobCarrier(long nObjID, long nIndex, long nCount, ref string[] psCarrierID, ref string[] psSlotInfo)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetPRJobCarrier(nObjID, nIndex, nCount, ref psCarrierID, ref psSlotInfo);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetPRJobRcpID(long nObjID, long nIndex, ref string psRcpID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetPRJobRcpID(nObjID, nIndex, ref psRcpID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetPRJobRcpParamCount(long nObjID, long nIndex, ref long pnCount)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException(); 
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetPRJobRcpParamCount(nObjID, nIndex, ref pnCount);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetPRJobRcpParam(long nObjID, long nIndex, long nCount, ref string[] psRcpParName, ref string[] psRcpParValue)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetPRJobRcpParam(nObjID, nIndex, nCount, ref psRcpParName, ref psRcpParValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetPRJobClose(long nObjID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetPRJobClose(nObjID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long PJDelJobInfo(string sPJobID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.PJDelJobInfo(sPJobID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long PJDelAllJobInfo()
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException(); 
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.PJDelAllJobInfo();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long PJSetJobInfo(string sPJobID, long nMtrlFormat, long nAutoStart, long nMtrlOrder, long nMtrlCount, string[] psMtrlID, string[] psSlotInfo, long nRcpMethod, string sRcpID, long nRcpParCount, string[] psRcpParName, string[] psRcpParVal)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.PJSetJobInfo(sPJobID, nMtrlFormat, nAutoStart, nMtrlOrder, nMtrlCount, psMtrlID, psSlotInfo, nRcpMethod, sRcpID, nRcpParCount, psRcpParName, psRcpParVal);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long PJSettingUpStart(string sPJobID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.PJSettingUpStart(sPJobID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long PJRspCommand(long nMsgID, long nCommand, string sPJobID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.PJRspCommand(nMsgID, nCommand, sPJobID, nResult, nErrCount, pnErrCode, psErrText);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long PJRspSetRcpVariable(long nMsgID, string sPJobID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.PJRspSetRcpVariable(nMsgID, sPJobID, nResult, nErrCount, pnErrCode, psErrText);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long PJRspSetStartMethod(long nMsgID, long nPJobCount, string[] psPJobID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.PJRspSetStartMethod(nMsgID, nPJobCount, psPJobID, nResult, nErrCount, pnErrCode, psErrText);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long PJRspSetMtrlOrder(long nMsgID, long nResult)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.PJRspSetMtrlOrder(nMsgID, nResult);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long PJReqCreateEx(string sPJobID, long nMtrlFormat, long nAutoStart, long nMtrlOrder, long nMtrlCount, string[] psMtrlID, string[] psSlotInfo, long nRcpMethod, string sRcpID, long nRcpParCount, string[] psRcpParName, long[] pnRcpParValue)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.PJReqCreateEx(sPJobID, nMtrlFormat, nAutoStart, nMtrlOrder, nMtrlCount, psMtrlID, psSlotInfo, nRcpMethod, sRcpID, nRcpParCount, psRcpParName, pnRcpParValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetPRJobRcpParamEx(long nObjID, long nIndex, long nCount, ref string[] psRcpParName, ref long[] pnRcpParValue)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetPRJobRcpParamEx(nObjID, nIndex, nCount, ref psRcpParName, ref pnRcpParValue);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long PJSetJobInfoEx(string sPJobID, long nMtrlFormat, long nAutoStart, long nMtrlOrder, long nMtrlCount, string[] psMtrlID, string[] psSlotInfo, long nRcpMethod, string sRcpID, long nRcpParCount, string[] psRcpParName, long[] pnRcpParVal)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();                
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.PJSetJobInfoEx(sPJobID, nMtrlFormat, nAutoStart, nMtrlOrder, nMtrlCount, psMtrlID, psSlotInfo, nRcpMethod, sRcpID, nRcpParCount, psRcpParName, pnRcpParVal);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CJReqCommand(string sCJobID, long nCommand, string sCPName, string sCPVal)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CJReqCommand(sCJobID, nCommand, sCPName, sCPVal);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CJReqGetJob(string sCJobID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CJReqGetJob(sCJobID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CJReqGetAllJobID()
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CJReqGetAllJobID();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CJReqCreate(string sCJobID, long nStartMethod, long nCountPRJob, string[] psPRJobID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CJReqCreate(sCJobID, nStartMethod, nCountPRJob, psPRJobID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CJReqSelect(string sCJobID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CJReqSelect(sCJobID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CJGetAllJobInfo(ref long pnObjID, ref long pnCount)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CJGetAllJobInfo(ref pnObjID, ref pnCount);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetCtrlJobID(long nObjID, long nIndex, ref string psCtrlJobID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetCtrlJobID(nObjID, nIndex, ref psCtrlJobID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetCtrlJobState(long nObjID, long nIndex, ref long pnState)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetCtrlJobState(nObjID, nIndex, ref pnState);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetCtrlJobStartMethod(long nObjID, long nIndex, ref long pnAutoStart)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetCtrlJobStartMethod(nObjID, nIndex, ref pnAutoStart);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetCtrlJobPRJobCount(long nObjID, long nIndex, ref long pnCount)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetCtrlJobPRJobCount(nObjID, nIndex, ref pnCount);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetCtrlJobPRJobIDs(long nObjID, long nIndex, long nCount, ref string[] psPRJobIDs)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetCtrlJobPRJobIDs(nObjID, nIndex, nCount, ref psPRJobIDs);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetCtrlJobClose(long nObjID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetCtrlJobClose(nObjID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CJDelJobInfo(string sCJobID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CJDelJobInfo(sCJobID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CJDelAllJobInfo()
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CJDelAllJobInfo();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CJSetJobInfo(string sCJobID, long nState, long nStartMethod, long nCountPRJob, string[] psPRJobID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CJSetJobInfo(sCJobID, nState, nStartMethod, nCountPRJob, psPRJobID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CJReqHOQJob(string sCJobID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CJReqHOQJob(sCJobID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CJGetHOQJob()
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CJGetHOQJob();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CJRspVerify(long nMsgId, string sCJobID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CJRspVerify(nMsgId, sCJobID, nResult, nErrCount, pnErrCode, psErrText);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CJRspCommand(long nMsgId, string sCJobID, long nCommand, long nResult, long nErrCode, string sErrText)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CJRspCommand(nMsgId, sCJobID, nCommand, nResult, nErrCode, sErrText);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long STSSetSubstLocationInfo(string sSubstLocID, string sSubstrateID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.STSSetSubstLocationInfo(sSubstLocID, sSubstrateID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long STSSetBatchLocationInfo(string sBatchLocID, string sSubstrateID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.STSSetBatchLocationInfo(sBatchLocID, sSubstrateID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long STSSetTransport(string sSubstLocID, string sSubstrateID, long nState)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();               
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.STSSetTransport(sSubstLocID, sSubstrateID, nState);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long STSSetBatchTransport(long nCount, string[] psSubstLocID, string[] psSubstrateID, long nState)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.STSSetBatchTransport(nCount, psSubstLocID, psSubstrateID, nState);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long STSSetProcessing(string sSubstLocID, string sSubstrateID, long nState)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.STSSetProcessing(sSubstLocID, sSubstrateID, nState);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long STSSetBatchProcessing(long nCount, string[] psSubstLocID, string[] psSubstrateID, long nState)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.STSSetBatchProcessing(nCount, psSubstLocID, psSubstrateID, nState);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long STSGetAllSubstrateInfo(ref long pnObjID, ref long pnCount)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.STSGetAllSubstrateInfo(ref pnObjID, ref pnCount);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetSubstrateID(long nObjID, long nIndex, ref string psSubstrateID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetSubstrateID(nObjID, nIndex, ref psSubstrateID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetSubstrateState(long nObjID, long nIndex, ref long pnTransportState, ref long pnProcessingState, ref long pnReadingState)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetSubstrateState(nObjID, nIndex, ref pnTransportState, ref pnProcessingState, ref pnReadingState);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetSubstrateLocID(long nObjID, long nIndex, ref string psSubstLocID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetSubstrateLocID(nObjID, nIndex, ref psSubstLocID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetSubstrateClose(long nObjID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetSubstrateClose(nObjID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long STSDelSubstrateInfo(string sSubstrateID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();                
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.STSDelSubstrateInfo(sSubstrateID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long STSDelAllSubstrateInfo()
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.STSDelAllSubstrateInfo();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long STSSetSubstrateInfo(string sSubstLocID, string sSubstrateID, long nTransportState, long nProcessingState, long nReadingState)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.STSSetSubstrateInfo(sSubstLocID, sSubstrateID, nTransportState, nProcessingState, nReadingState);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long STSSetMaterialArrived(string sMaterialID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.STSSetMaterialArrived(sMaterialID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long STSReqCreateSubstrate(string sSubstLocID, string sSubstrateID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.STSReqCreateSubstrate(sSubstLocID, sSubstrateID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long STSRspCreateSubstrate(long nMsgID, string sSubstLocID, string sSubstrateID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.STSRspCreateSubstrate(nMsgID, sSubstLocID, sSubstrateID, nResult, nErrCount, pnErrCode, psErrText);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long STSReqCancelSubstrate(string sSubstLocID, string sSubstrateID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.STSReqCancelSubstrate(sSubstLocID, sSubstrateID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long STSRspCancelSubstrate(long nMsgID, string sSubstLocID, string sSubstrateID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.STSRspCancelSubstrate(nMsgID, sSubstLocID, sSubstrateID, nResult, nErrCount, pnErrCode, psErrText);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long STSReqProceedSubstrate(string sSubstLocID, string sSubstrateID, string sReadSubstID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.STSReqProceedSubstrate(sSubstLocID, sSubstrateID, sReadSubstID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long STSRspUpdateSubstrate(long nMsgID, string sSubstLocID, string sSubstrateID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.STSRspUpdateSubstrate(nMsgID, sSubstLocID, sSubstrateID, nResult, nErrCount, pnErrCode, psErrText);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long STSReqDeleteSubstrate(string sSubstLocID, string sSubstrateID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.STSReqDeleteSubstrate(sSubstLocID, sSubstrateID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long STSRspDeleteSubstrate(long nMsgID, string sSubstLocID, string sSubstrateID, long nResult, long nErrCount, long[] pnErrCode, string[] psErrText)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.STSRspDeleteSubstrate(nMsgID, sSubstLocID, sSubstrateID, nResult, nErrCount, pnErrCode, psErrText);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long STSSetSubstrateID(string sSubstLocID, string sSubstrateID, string sSubstReadID, long nResult)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.STSSetSubstrateID(sSubstLocID, sSubstrateID, sSubstReadID, nResult);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long OpenGEMObject(ref long pnMsgID, string sObjType, string sObjID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.OpenGEMObject(ref pnMsgID, sObjType, sObjID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long CloseGEMObject(long nMsgID)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.CloseGEMObject(nMsgID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public long GetAttrData(ref long pnObjectID, long nMsgID, string sAttrName)
        {
            if (IsRunXGem())
            {
                throw new NotImplementedException();
            }
            else if (IsRunXGem300Pro())
            {
                return m_XGem300Pro.GetAttrData(ref pnObjectID, nMsgID, sAttrName);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        #endregion
    }
}
