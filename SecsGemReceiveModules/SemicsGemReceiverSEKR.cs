using System;
using System.Linq;

namespace SecsGemReceiveModules
{
    using GEM_XGem;
    using SecsGemServiceInterface;
    using XGEMWrapper;

    public class SemicsGemReceiverSEKR : ISecsGemMessageReceiver
    {
        //private XGemNet m_XGem { get; set; }
        public ISecsGemServiceCallback callback { get; set; }
        private XGEM Gem { get; set; }
        public SemicsGemReceiverSEKR()
        {
        }
        public bool SetXGem(object xgem)
        {
            bool retVal = false;
            try
            {
                if (xgem != null)
                {
                    Gem = xgem as XGEM;
                    retVal = true;
                }
            }
            catch (Exception err)
            {

                throw err;
            }
            return retVal;
        }
        public void XGEM_OnSECSMessageReceive(long nMsgId, long nStream, long nFunction, long nSysbyte)
        {
            try
            {
                if (nStream == 2 && nFunction == 49)
                {
                    XGEM_OnSECSMessageReceive_S2F49(nMsgId, nStream, nFunction, nSysbyte);
                }
            }
            catch (Exception err)
            {
                throw err;
            }
        }

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

                if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.ACTIVATE_PROCESS.ToString())
                {
                    long pnObjectID = 0;
                    //byte HAACK = 0x04;
                    //MakeObject(ref pnObjectID);
                    //SetListItem(pnObjectID, 2);
                    //SetBinaryItem(pnObjectID, HAACK);
                    //SetListItem(pnObjectID, 0);
                    //SendSECSMessage(pnObjectID, nStream, nFunction + 1, nSysbyte);

                    var actreqdata = new ActiveProcessActReqData();
                    actreqdata.ObjectID = pnObjectID;
                    actreqdata.Stream = nStream;
                    actreqdata.Function = nFunction;
                    actreqdata.Sysbyte = nSysbyte;

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
                    byte HAACK = 0x04;
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
                    remoteActReqData.ActionType = EnumRemoteCommand.DOWNLOAD_STAGE_RECIPE;
                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.SET_PARAMETERS.ToString())
                {
                    long pnObjectID = 0;
                    byte HAACK = 0x04;
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
                    actreqdata.RecipeId = msg; // Data : [A] Lot ID 

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
                    string msg = null;
                    string waferid = null;

                    uint[] user_foupnumber_UINT4 = new uint[1];
                    Gem.GetList(nMsgId, ref dflist);
                    Gem.GetList(nMsgId, ref dflist);// [L] FoupNumber
                    Gem.GetAscii(nMsgId, ref msg);// [A] FoupNumber
                    Gem.GetU4(nMsgId, ref user_foupnumber_UINT4);// [U] FoupNumber
                    actreqdata.FoupNumber = (int)user_foupnumber_UINT4[0]; //Data [U] Foup Number

                    Gem.GetList(nMsgId, ref dflist); // [L] Lot ID List
                    Gem.GetAscii(nMsgId, ref msg); //[A] Lot ID 
                    Gem.GetAscii(nMsgId, ref msg); //[A] Lot ID 
                    actreqdata.LotID = msg; // Data : [A] Lot ID 

                    Gem.GetList(nMsgId, ref dflist); // [L] ListOfWaferIds
                    Gem.GetAscii(nMsgId, ref msg); //[A] ListOfWaferIds

                    Gem.GetList(nMsgId, ref dflist);
                    long listcount = dflist;
                    for (int index = 0; index < listcount; index++)
                    {
                        waferid = "";
                        Gem.GetAscii(nMsgId, ref waferid); //[A] waferid
                        actreqdata.WaferIDs.Add(waferid);
                    }

                    remoteActReqData = actreqdata;
                    remoteActReqData.ActionType = EnumRemoteCommand.WAFERID_LIST;
                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.DOCK_FOUP.ToString() ||
                    REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.UNDOCK_FOUP.ToString())
                {
                    //long pnObjectID = 0;
                    //byte HAACK = 0x04;
                    //MakeObject(ref pnObjectID);
                    //SetListItem(pnObjectID, 2);
                    //SetBinaryItem(pnObjectID, HAACK);
                    //SetListItem(pnObjectID, 0);
                    //SendSECSMessage(pnObjectID, nStream, nFunction + 1, nSysbyte);

                    var actreqdata = new DockFoupActReqData();

                    actreqdata.ObjectID = nMsgId;
                    actreqdata.Stream = nStream;
                    actreqdata.Function = nFunction;
                    actreqdata.Sysbyte = nSysbyte;

                    long dflist1 = 0;
                    string lpnumber = null;
                    uint[] user_foupnumber_UINT4 = new uint[1];
                    Gem.GetList(nMsgId, ref dflist1);

                    Gem.GetList(nMsgId, ref dflist1);// [L] FoupNumber
                    Gem.GetAscii(nMsgId, ref lpnumber);// [A] FoupNumber
                    Gem.GetU4(nMsgId, ref user_foupnumber_UINT4);// [U] FoupNumber

                    actreqdata.FoupNumber = (int)user_foupnumber_UINT4[0]; //Data [U] Foup Number



                    remoteActReqData = actreqdata;
                    if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.DOCK_FOUP.ToString())
                    {
                        remoteActReqData.ActionType = EnumRemoteCommand.DOCK_FOUP;
                    }
                    else
                    {
                        remoteActReqData.ActionType = EnumRemoteCommand.UNDOCK_FOUP;
                    }

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
                    remoteActReqData.ActionType = EnumRemoteCommand.SELECT_SLOTS;
                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.SELECT_SLOTS_STAGE.ToString())
                {
                    long pnObjectID = 0;
                    byte HAACK = 0x04;
                    MakeObject(ref pnObjectID);
                    SetListItem(pnObjectID, 2);
                    SetBinaryItem(pnObjectID, HAACK);
                    SetListItem(pnObjectID, 0);
                    SendSECSMessage(pnObjectID, nStream, nFunction + 1, nSysbyte);

                    var actreqdata = new SelectSlotsStagesActReqData();

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

                    Gem.GetList(nMsgId, ref dflist); // [L] ListOfStagesToTransfer List
                    Gem.GetAscii(nMsgId, ref msg); //[A] Title ListOfStagesToTransfer

                    Gem.GetList(nMsgId, ref dflist);
                    long listcount = dflist;
                    for (int index = 0; index < listcount; index++)
                    {
                        Gem.GetU4(nMsgId, ref user_stagenumber_UINT4); //[A] stage number
                        SlotCellInfo slotCellInfo = new SlotCellInfo(index + 1);
                        slotCellInfo.CellIndexs.Add((int)user_stagenumber_UINT4[0]);
                        actreqdata.SlotStageNumbers.Add(slotCellInfo);

                    }

                    remoteActReqData = actreqdata;
                    remoteActReqData.ActionType = EnumRemoteCommand.SELECT_SLOTS_STAGE;
                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.START_LOT.ToString())
                {

                    var actreqdata = new StartLotActReqData();

                    actreqdata.ObjectID = nMsgId;
                    actreqdata.Stream = nStream;
                    actreqdata.Function = nFunction;
                    actreqdata.Sysbyte = nSysbyte;

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
                    remoteActReqData.ActionType = EnumRemoteCommand.START_LOT;
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
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.END_TEST_LP.ToString())
                {
                    long pnObjectID = 0;
                    byte HAACK = 0x04;
                    MakeObject(ref pnObjectID);
                    SetListItem(pnObjectID, 2);
                    SetBinaryItem(pnObjectID, HAACK);
                    SetListItem(pnObjectID, 0);
                    SendSECSMessage(pnObjectID, nStream, nFunction + 1, nSysbyte);

                    var actreqdata = new EndTestReqLPDate();

                    long dflist = 0;
                    uint[] user_stagenumber_UINT4 = new uint[1];
                    uint[] user_pmiexecflag_UINT4 = new uint[1];
                    uint[] user_foupnumber_UINT4 = new uint[1];
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

                    Gem.GetList(nMsgId, ref dflist);// [L] FoupNumber
                    Gem.GetAscii(nMsgId, ref msg);// [A] FoupNumber
                    Gem.GetU4(nMsgId, ref user_foupnumber_UINT4);// [U] FoupNumber
                    actreqdata.FoupNumber = (int)user_foupnumber_UINT4[0]; //Data [U] Foup Number

                    remoteActReqData = actreqdata;
                    remoteActReqData.ActionType = EnumRemoteCommand.END_TEST_LP;
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
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.ERROR_END_LP.ToString())
                {
                    long pnObjectID = 0;
                    byte HAACK = 0x04;
                    MakeObject(ref pnObjectID);
                    SetListItem(pnObjectID, 2);
                    SetBinaryItem(pnObjectID, HAACK);
                    SetListItem(pnObjectID, 0);
                    SendSECSMessage(pnObjectID, nStream, nFunction + 1, nSysbyte);

                    var actreqdata = new ErrorEndLPData();

                    long dflist1 = 0;
                    string lpnumber = null;
                    uint[] user_stagenumber_UINT4 = new uint[1];
                    uint[] user_foupnumber_UINT4 = new uint[1];
                    Gem.GetList(nMsgId, ref dflist1);

                    Gem.GetList(nMsgId, ref dflist1);// [L] StageNumber
                    Gem.GetAscii(nMsgId, ref lpnumber);// [A] StageNumber
                    Gem.GetU4(nMsgId, ref user_stagenumber_UINT4);// [U] StageNumber
                    actreqdata.StageNumber = (int)user_stagenumber_UINT4[0]; //Data [U] StageNumber

                    Gem.GetList(nMsgId, ref dflist1);// [L] FoupNumber
                    Gem.GetAscii(nMsgId, ref lpnumber);// [A] FoupNumber
                    Gem.GetU4(nMsgId, ref user_foupnumber_UINT4);// [U] FoupNumber
                    actreqdata.FoupNumber = (int)user_foupnumber_UINT4[0]; //Data [U] Foup Number

                    remoteActReqData = actreqdata;
                    remoteActReqData.ActionType = EnumRemoteCommand.ERROR_END_LP;
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
                    string cpname = "";
                    uint[] lpnumber = new uint[1];
                    string stagenumber = null;
                    string lotid = null;
                    uint[] user_stagenumber_UINT4 = new uint[1];
                    uint[] user_foupnumber_UINT4 = new uint[1];
                    Gem.GetList(nMsgId, ref dflist1);

                    Gem.GetList(nMsgId, ref dflist1);// [L] FoupNumber
                    Gem.GetAscii(nMsgId, ref cpname);// [A] FoupNumber
                    Gem.GetU4(nMsgId, ref lpnumber);// [U] FoupNumber
                    actreqdata.FoupNumber = Convert.ToInt32(lpnumber[0]); //Data [U] FoupNumber

                    Gem.GetList(nMsgId, ref dflist1); // [L] Lot ID List
                    Gem.GetAscii(nMsgId, ref lotid); //[A] Title LOT ID  
                    Gem.GetAscii(nMsgId, ref lotid); //[A] Lot ID 
                    actreqdata.LotID = lotid; // Data : [A] Lot ID 


                    Gem.GetList(nMsgId, ref dflist1);// [L] StageNumber
                    Gem.GetAscii(nMsgId, ref stagenumber);// [A] StageNumber
                    Gem.GetU4(nMsgId, ref user_stagenumber_UINT4);// [U] StageNumber
                    actreqdata.StageNumber = Convert.ToInt32(user_stagenumber_UINT4[0]); //Data [U] StageNumber

                    remoteActReqData = actreqdata;
                    remoteActReqData.ActionType = EnumRemoteCommand.START_STAGE;
                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.CHANGE_LOADPORT_MODE.ToString())
                {
                    long pnObjectID = 0;
                    byte HAACK = 0x04;
                    MakeObject(ref pnObjectID);
                    SetListItem(pnObjectID, 2);
                    SetBinaryItem(pnObjectID, HAACK);
                    SetListItem(pnObjectID, 0);
                    SendSECSMessage(pnObjectID, nStream, nFunction + 1, nSysbyte);

                    var actreqdata = new ChangeLoadPortModeActReqData();

                    long dflist1 = 0;
                    string lpnumber = null;
                    uint[] user_stagenumber_UINT4 = new uint[1];
                    uint[] user_foupnumber_UINT4 = new uint[1];
                    Gem.GetList(nMsgId, ref dflist1);

                    Gem.GetList(nMsgId, ref dflist1);// [L] FoupNumber
                    Gem.GetAscii(nMsgId, ref lpnumber);// [A] FoupNumber
                    Gem.GetU4(nMsgId, ref user_foupnumber_UINT4);// [U] FoupNumber
                    actreqdata.FoupNumber = (int)user_foupnumber_UINT4[0]; //Data [U] Foup Number

                    Gem.GetList(nMsgId, ref dflist1);// [L] LPmode
                    Gem.GetAscii(nMsgId, ref lpnumber);// [A] LPmode
                    Gem.GetU4(nMsgId, ref user_stagenumber_UINT4);// [U] LPmode
                    actreqdata.FoupModeState = (int)user_stagenumber_UINT4[0]; //Data [U] LPmode

                    remoteActReqData = actreqdata;
                    remoteActReqData.ActionType = EnumRemoteCommand.CHANGE_LOADPORT_MODE;
                }

                callback.OnRemoteCommandAction(remoteActReqData);
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        #region //Send Ack Message Function
        public long MakeObject(ref long pnMsgId)
        {
            return Gem.MakeObject(ref pnMsgId);
        }

        public long SetListItem(long nMsgId, long nItemCount)
        {
            return Gem.SetList(nMsgId, nItemCount);
        }

        public long SetBinaryItem(long nMsgId, byte nValue)
        {
            return Gem.SetBinary(nMsgId, nValue);
        }

        public long SetBoolItem(long nMsgId, bool nValue)
        {
            return Gem.SetBool(nMsgId, nValue);
        }
        public long SendSECSMessage(long nMsgId, long nStream, long nFunction, long nSysbyte)
        {
            return Gem.SendSECSMessage(nMsgId, nStream, nFunction, nSysbyte);
        }
        #endregion
    }
}
