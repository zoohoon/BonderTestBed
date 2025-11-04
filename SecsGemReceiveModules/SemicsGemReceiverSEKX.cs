using System;
using System.Collections.Generic;

namespace SecsGemReceiveModules
{
    using GEM_XGem;
    using SecsGemServiceInterface;
    using XGEMWrapper;

    public class SemicsGemReceiverSEKX : ISecsGemMessageReceiver
    {
        //private XGemNet m_XGem { get; set; }
        public ISecsGemServiceCallback callback { get; set; }
        private XGEM Gem { get; set; }
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
                if (nStream == 2 && nFunction == 15)
                {
                    //S2F15	Foup Shift
                    XGEM_OnSECSMessageReceive_S2F15(nMsgId, nStream, nFunction, nSysbyte);
                }
                else if (nStream == 3 && nFunction == 17)
                {
                    //S3F17	ProceedWithCarrier
                    //S3F17	ProceedWithSlot	
                    //S3F17	CancelCarrier	


                    XGEM_OnSECSMessageReceive_S3F17(nMsgId, nStream, nFunction, nSysbyte);
                }
                else if ((nStream == 2 && nFunction == 49) || (nStream == 2 && nFunction == 41))
                {
                    //S2F41	DLRECEIPE
                    //S2F49	PSTART
                    //S2F49	STAGE_SLOT	

                    //S2F41	WFIDCONF
                    //S2F41	Resume
                    //S2F41	Return Carrier	??

                    XGEM_OnSECSMessageReceive_S2F4X(nMsgId, nStream, nFunction, nSysbyte);
                }
            }
            catch (Exception err)
            {
                throw err;
            }
        }
        private void XGEM_OnSECSMessageReceive_S2F15(long nMsgId, long nStream, long nFunction, long nSysbyte)
        {
            try
            {
                long mainListCount = 0;
                long subListCount = 0;

                uint[] ECID = new uint[1];
                string ECV = null;

                long[] pnEcids = new long[1];
                string[] psVals = new string[1];

                EquipmentReqData EquipmentReqData = null;

                Gem.GetList(nMsgId, ref mainListCount);
                Gem.GetList(nMsgId, ref subListCount);
                Gem.GetU4(nMsgId, ref ECID);
                EquipmentReqData.ECID = ECID;
                pnEcids[0] = ECID[0];

                Gem.GetAscii(nMsgId, ref ECV);
                EquipmentReqData.ECV = ECV;
                psVals[0] = ECV;

                Gem.CloseObject(nMsgId);
                if (EquipmentReqData != null)
                {
                    EquipmentReqData.Stream = nStream;
                    EquipmentReqData.Function = nFunction;
                    EquipmentReqData.Sysbyte = nSysbyte;
                    callback?.ECVChangeMsgReceive(EquipmentReqData);
                }
            }
            catch (Exception err)
            {
                throw err;
            }

        }


         private void XGEM_OnSECSMessageReceive_S3F17(long nMsgId, long nStream, long nFunction, long nSysbyte)
        {
            try
            {
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
                    //actCarrierData.CattrData = new string[subListCount];
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
                            actCarrierData.LotID= CATTRDATA;
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
                        //    Gem.GetAscii(nMsgId, ref CATTRID);
                        //    Carrier.CattrID[i] = CATTRID;
                        //    Gem.GetList(nMsgId, ref listCount4);

                        //    for (int s = 0; s < 25; s++)
                        //    {
                        //        string slotmap = null;
                        //        Gem.GetAscii(nMsgId, ref slotmap);
                        //        Carrier.SlotMap[total_cnt++] = slotmap;
                        //    }

                        //    string temp = null;
                        //    string cattrdata = null;
                        //    Gem.GetList(nMsgId, ref listCount4);
                        //    Gem.GetAscii(nMsgId, ref temp);
                        //    Gem.GetAscii(nMsgId, ref cattrdata);
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
                    //for (int i = 0; i < subListCount; i++)
                    //{
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
                    //}

                    carrierActReqData = actCarrierData;
                }
                else if (CARRIERACTION.ToUpper() == EnumCarrierAction.CANCELCARRIER.ToString())
                {
                    var actCarrierData = new CarrieActReqData();
                    actCarrierData.ActionType = EnumCarrierAction.CANCELCARRIER;
                    actCarrierData.PTN = PTN[0];

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
                else
                {
                    long pnMsgId = 0;
                    byte CAACK = 0x01;

                    Gem.MakeObject(ref pnMsgId);

                    Gem.SetList(pnMsgId, 2);
                    Gem.SetU1(pnMsgId, CAACK);
                    Gem.SetList(pnMsgId, subListCount);

                    for (int i = 0; i < subListCount; i++)
                    {
                        Gem.SetList(pnMsgId, 2);
                        Gem.SetU2(pnMsgId, 0);
                        Gem.SetAscii(pnMsgId, "");
                    }

                    //Gem.SendSECSMessage(pnMsgId, nStream, nFunction + 1, nSysbyte);
                    carrierActReqData = null;
                }

                Gem.CloseObject(nMsgId);
                if (carrierActReqData != null)
                {
                    carrierActReqData.Stream = nStream;
                    carrierActReqData.Function = nFunction;
                    carrierActReqData.Sysbyte = nSysbyte;
                    callback?.OnCarrierActMsgRecive(carrierActReqData);
                }
            }
            catch (Exception err)
            {
                throw err;
            }


        }

        private void XGEM_OnSECSMessageReceive_S2F4X(long nMsgId, long nStream, long nFunction, long nSysbyte)
        {
            try
            {
                RemoteActReqData remoteActReqData = null;
                
                long mainListCount = 0;
                long subListCount = 0;
                long subItemListCount = 0;
                List<long> RetValues = new List<long>();


                uint[] USER_UINT4 = new uint[1];
                string REMOTE_COMMAND_ACTION = null;
                string REMOTE_COMMAND_ID = null;

                //EnumRemoteCommand Rcmd = EnumRemoteCommand.ABORT;
                

                if(nFunction == 41)
                {
                    Gem.GetList(nMsgId, ref mainListCount);                    
                    Gem.GetAscii(nMsgId, ref REMOTE_COMMAND_ACTION);
                }
                else if(nFunction == 49)
                {

                    Gem.GetList(nMsgId, ref mainListCount);
                    Gem.GetU4(nMsgId, ref USER_UINT4);
                    Gem.GetAscii(nMsgId, ref REMOTE_COMMAND_ID);
                    Gem.GetAscii(nMsgId, ref REMOTE_COMMAND_ACTION);
                }


                if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.PSTART.ToString()||
                    REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.TC_START.ToString())
                {
                    var actreqdata = new StartLotActReqData();
                    actreqdata.ObjectID = nMsgId;
                    actreqdata.Stream = nStream;
                    actreqdata.Function = nFunction;
                    actreqdata.Sysbyte = nSysbyte;


                    //string msg = null;
                    //string CPNAME = null;
                    //uint[] foupNumber = new uint[1];
                    //string carrierId = null;

                    //Gem.GetList(nMsgId, ref subListCount); // L, 2



                    //Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    //Gem.GetAscii(nMsgId, ref CPNAME);// [A] PORT_ID
                    //Gem.GetU4(nMsgId, ref foupNumber);// [A] PORT_ID
                    //RetValues.Add(0);
                    //actreqdata.FoupNumber = (int)foupNumber[0];

                    //Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    //Gem.GetAscii(nMsgId, ref CPNAME);// [A] CARRIER_ID                    
                    //Gem.GetAscii(nMsgId, ref carrierId);// [A] CARRIER_ID
                    //RetValues.Add(0);
                    //actreqdata.CarrierId = carrierId;



                    //string msg = null;
                    string CPNAME = null;
                    string lotId = null;
                    uint[] foupNumber = new uint[1];
                    //string ocrread = null;
                    string carrierId = null;

                    Gem.GetList(nMsgId, ref subListCount); // L, 2



                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] LOT_ID
                    Gem.GetAscii(nMsgId, ref lotId);// [A] LOT_ID
                    RetValues.Add(0);
                    actreqdata.LotID = lotId;

                    //Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    //Gem.GetAscii(nMsgId, ref CPNAME);// [A] OCR_READ                    
                    //Gem.GetAscii(nMsgId, ref ocrread);// [A] OCR_READ
                    //RetValues.Add(0);
                    //actreqdata.OCRReadFalg = ocrread;

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] PORT_ID
                    Gem.GetU4(nMsgId, ref foupNumber);// [U4] PORT_ID
                    RetValues.Add(0);
                    actreqdata.FoupNumber = (int)foupNumber[0];

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] CARRIER_ID
                    Gem.GetAscii(nMsgId, ref carrierId);// [A] CARRIER_ID
                    RetValues.Add(0);
                    actreqdata.CarrierID = carrierId;



                    if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.PSTART.ToString())
                        actreqdata.ActionType = EnumRemoteCommand.PSTART;
                    else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.TC_START.ToString())
                        actreqdata.ActionType = EnumRemoteCommand.TC_START;

                    remoteActReqData = actreqdata;
                    
                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.STAGE_SLOT.ToString())
                {
                    var actreqdata = new SelectStageSlotActReqData(); // ProceedWithCellSlotActReqData

                    //uint[] USER_UINT4 = new uint[1];
                    //string msg = null;
                    string CPNAME = null;
                    uint[] foupNumber = new uint[1];
                    string carrierId = null;
                    string stageNumber = null;
                    string selectedSlot = null;

                    //Gem.GetU4(nMsgId, ref USER_UINT4); // U4 [1]
                    //Gem.GetAscii(nMsgId, ref msg); // 0
                    //Gem.GetAscii(nMsgId, ref REMOTE_COMMAND_ACTION); // STAGE_SLOT
                    //Rcmd = EnumRemoteCommand.STAGE_SLOT;
                    Gem.GetList(nMsgId, ref subListCount); // L, 4                                        

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2                    
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] PORT_ID                    
                    Gem.GetU4(nMsgId, ref foupNumber);// [A] PORT_ID
                    RetValues.Add(0);
                    actreqdata.PTN = (int)foupNumber[0];                    

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] CARRIER_ID
                    Gem.GetAscii(nMsgId, ref carrierId);// [A] CARRIER_ID
                    RetValues.Add(0);
                    actreqdata.CarrierId = carrierId;

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] STAGE
                    Gem.GetAscii(nMsgId, ref stageNumber);// [A] STAGE
                    RetValues.Add(0);
                    actreqdata.CellMap = stageNumber;

                    Gem.GetList(nMsgId, ref subItemListCount); // [L] ListOfSlotToUse List
                    Gem.GetAscii(nMsgId, ref CPNAME); //[A] Title ListOfSlotToUse
                    Gem.GetAscii(nMsgId, ref selectedSlot); //[A]  ListOfSlotToUse
                    RetValues.Add(0);
                    actreqdata.SlotMap = selectedSlot;                                      
                    remoteActReqData = actreqdata;

                    remoteActReqData.ActionType = EnumRemoteCommand.STAGE_SLOT;
                }
                else if(REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.SELECT_SLOTS.ToString())
                {
                    var actreqdata = new SelectSlotsActReqData(); // ProceedWithCellSlotActReqData

                    //uint[] USER_UINT4 = new uint[1];
                    //string msg = null;
                    string CPNAME = null;
                    uint[] foupNumber = new uint[1];
                    string lotid = null;
                    //string stageNumber = null;
                    string selectedSlot = null;

                    //Gem.GetU4(nMsgId, ref USER_UINT4); // U4 [1]
                    //Gem.GetAscii(nMsgId, ref msg); // 0
                    //Gem.GetAscii(nMsgId, ref REMOTE_COMMAND_ACTION); // STAGE_SLOT
                    //Rcmd = EnumRemoteCommand.STAGE_SLOT;
                    Gem.GetList(nMsgId, ref subListCount); // L, 4                                        

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2                    
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] PORT_ID                    
                    Gem.GetU4(nMsgId, ref foupNumber);// [A] PORT_ID
                    RetValues.Add(0);
                    actreqdata.FoupNumber = (int)foupNumber[0];

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] lotid
                    Gem.GetAscii(nMsgId, ref lotid);// [A] lotid
                    RetValues.Add(0);
                    actreqdata.LotID = lotid;


                    Gem.GetList(nMsgId, ref subItemListCount); // [L] ListOfSlotToUse List
                    Gem.GetAscii(nMsgId, ref CPNAME); //[A] Title ListOfSlotToUse
                    Gem.GetAscii(nMsgId, ref selectedSlot); //[A]  ListOfSlotToUse
                    RetValues.Add(0);
                    actreqdata.UseSlotNumbers_str = selectedSlot;
                    remoteActReqData = actreqdata;

                    remoteActReqData.ActionType = EnumRemoteCommand.SELECT_SLOTS;
                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.SELECT_SLOTS_STAGES.ToString())
                {
                    var actreqdata = new SelectStageSlotsActReqData(); 

                    //uint[] USER_UINT4 = new uint[1];
                    //string msg = null;
                    string CPNAME = null;
                    uint[] foupNumber = new uint[1];
                    string lotid = null;
                    string stagesToUse = null;
                    string selectedstageSlot = null;

                    //Gem.GetU4(nMsgId, ref USER_UINT4); // U4 [1]
                    //Gem.GetAscii(nMsgId, ref msg); // 0
                    //Gem.GetAscii(nMsgId, ref REMOTE_COMMAND_ACTION); // STAGE_SLOT
                    //Rcmd = EnumRemoteCommand.STAGE_SLOT;
                    Gem.GetList(nMsgId, ref subListCount); // L, 4                                        

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2                    
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] PORT_ID                    
                    Gem.GetU4(nMsgId, ref foupNumber);// [A] PORT_ID
                    RetValues.Add(0);
                    actreqdata.PTN = (int)foupNumber[0];

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] lotid
                    Gem.GetAscii(nMsgId, ref lotid);// [A] lotid
                    RetValues.Add(0);
                    actreqdata.LotID = lotid;

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] stagesToUse
                    Gem.GetAscii(nMsgId, ref stagesToUse);// [A] stagesToUse
                    RetValues.Add(0);
                    actreqdata.CellMap = stagesToUse;// like "101000000000"

                    Gem.GetList(nMsgId, ref subItemListCount); // [L] ListOfSlotToUse List
                    Gem.GetAscii(nMsgId, ref CPNAME); //[A] Title UseSlotStageNumbers_str
                    Gem.GetAscii(nMsgId, ref selectedstageSlot); //[A]  UseSlotStageNumbers_str
                    RetValues.Add(0);
                    actreqdata.UseSlotStageNumbers_str = selectedstageSlot;// like "113300000000"
                    remoteActReqData = actreqdata;

                    remoteActReqData.ActionType = EnumRemoteCommand.SELECT_SLOTS_STAGES;
                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.WFIDCONFPROC.ToString())
                {
                    var actreqdata = new WaferConfirmActReqData();

                    //uint[] USER_UINT4 = new uint[1];
                    //string msg = null;
                    string CPNAME = null;
                    string LOTID = null;
                    uint[] foupNumber = new uint[1];
                    uint[] slotNumber = new uint[1];
                    string waferId = null;
                    string ocrRead = null;

                    //Gem.GetAscii(nMsgId, ref REMOTE_COMMAND_ACTION); // WFIDCONFPROC
                    //Rcmd = EnumRemoteCommand.WFIDCONFPROC;
                    Gem.GetList(nMsgId, ref subListCount); // L, 4                    

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] LOTID
                    Gem.GetAscii(nMsgId, ref LOTID);// [A] LOTID
                    RetValues.Add(0);
                    actreqdata.LotID = LOTID;

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [U4] PORT_ID
                    Gem.GetU4(nMsgId, ref foupNumber);// [U4] PORT_ID
                    RetValues.Add(0);
                    actreqdata.PTN = (int)foupNumber[0];

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [U4] SLOT_NUM
                    Gem.GetU4(nMsgId, ref slotNumber);// [U4] SLOT_NUM
                    RetValues.Add(0);
                    actreqdata.SlotNum = (int)slotNumber[0];

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] WAFER_ID                    
                    Gem.GetAscii(nMsgId, ref waferId);// [A] WAFER_ID
                    RetValues.Add(0);
                    actreqdata.WaferId = waferId;

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] OCRREAD
                    Gem.GetAscii(nMsgId, ref ocrRead);// [A] OCRREAD
                    RetValues.Add(0);
                    actreqdata.OCRReadFalg = int.Parse(ocrRead);


                    remoteActReqData = actreqdata;
                    remoteActReqData.ActionType = EnumRemoteCommand.WFIDCONFPROC;
                }                
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.DLRECIPE.ToString())
                {
                    var actreqdata = new DownloadStageRecipeActReqData();

                    //string msg = null;
                    string CPNAME = null;
                    string recipe = null;
                    string StageNum = null;
                    int stgnum = 0;
                    

                    Gem.GetList(nMsgId, ref subListCount); // L, 2

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] RECIPE
                    Gem.GetAscii(nMsgId, ref recipe);// [A] RECIPE
                    RetValues.Add(0);


                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [U1] StageNumber
                    Gem.GetAscii(nMsgId, ref StageNum);// [U4] StageNumber
                    int.TryParse(StageNum, out stgnum);
                    RetValues.Add(0);

                    actreqdata.RecipeDic.Add(stgnum, recipe); //Data : [U] StageNumber , [A] Recipe Id

                    remoteActReqData = actreqdata;
                    remoteActReqData.ActionType = EnumRemoteCommand.DLRECIPE;
                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.RESUME.ToString())
                {
                    var actreqdata = new StartStage();

                    //string msg = null;
                    string CPNAME = null;
                    string StageId = null;

                    //Gem.GetAscii(nMsgId, ref REMOTE_COMMAND_ACTION); // RESUME
                    //Rcmd = EnumRemoteCommand.RESUME;
                    Gem.GetList(nMsgId, ref subListCount); // L, 1
                    
                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] STAGE_ID
                    Gem.GetAscii(nMsgId, ref StageId);// [A] STAGE_ID
                    RetValues.Add(0);
                    if(StageId != null)
                    {
                        actreqdata.StageNumber = int.Parse(StageId);
                    }                    

                    remoteActReqData = actreqdata;
                    remoteActReqData.ActionType = EnumRemoteCommand.RESUME;
                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.UNDOCK.ToString() ||
                        REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.DOCK_FOUP.ToString())
                {
                    var actreqdata = new CarrierIdentityData();
                    actreqdata.ObjectID = nMsgId;
                    actreqdata.Stream = nStream;
                    actreqdata.Function = nFunction;
                    actreqdata.Sysbyte = nSysbyte;
                    
                    //string msg = null;
                    string CPNAME = null;
                    uint[] foupNumber = new uint[1];
                    string carrierId = null;

                    //Gem.GetU4(nMsgId, ref USER_UINT4); // U4 [4] UNDOCK
                    //Rcmd = EnumRemoteCommand.UNDOCK;
                    Gem.GetList(nMsgId, ref subListCount); // L, 2
                    
                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] PORT_ID
                    Gem.GetU4(nMsgId, ref foupNumber);// [A] PORT_ID
                    RetValues.Add(0);
                    actreqdata.PTN = (int)foupNumber[0];

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] CARRIER_ID
                    Gem.GetAscii(nMsgId, ref carrierId);// [A] CARRIER_ID
                    RetValues.Add(0);
                    actreqdata.CarrierId = carrierId;

                    remoteActReqData = actreqdata;
                    if(REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.DOCK_FOUP.ToString())
                    {
                        remoteActReqData.ActionType = EnumRemoteCommand.DOCK_FOUP;
                    }
                    else
                    {
                        remoteActReqData.ActionType = EnumRemoteCommand.UNDOCK;
                    }
                    
                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.ZUP.ToString()
                    || REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.TESTEND.ToString()
                    || REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.WAFERUNLOAD.ToString()
                    || REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.PABORT.ToString()
                    || REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.START_CARD_CHANGE.ToString()
                    || REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.MOVEIN_CARD_CLOSE_COVER.ToString()
                    || REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.TC_END.ToString()
                    )
                {
                    //long pnObjectID = 0;
                    byte HAACK = 0x04;
                    long objid = 0;
                    MakeObject(ref objid);
                    SetListItem(objid, 2);
                    SetBinaryItem(objid, HAACK);
                    SetListItem(objid, 0);
                    //SendSECSMessage(objid, nStream, nFunction + 1, nSysbyte);

                    var actreqdata = new StageActReqData();
                    actreqdata.ObjectID = objid;
                    actreqdata.Stream = nStream;
                    actreqdata.Function = nFunction;
                    actreqdata.Sysbyte = nSysbyte;

                    //long dataListCount = 0;
                    string CPNAME = null;
                    string stagenumber = null;
                    Gem.GetList(nMsgId, ref subListCount);

                    Gem.GetList(nMsgId, ref subItemListCount);// [L] StageNumber
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] StageNumber
                    Gem.GetAscii(nMsgId, ref stagenumber);// [A] StageNumber

                    actreqdata.StageNumber = Convert.ToInt32(stagenumber); //Data [U] StageNumber

                    remoteActReqData = actreqdata;

                    if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.ZUP.ToString())
                        remoteActReqData.ActionType = EnumRemoteCommand.ZUP;
                    else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.TESTEND.ToString())
                        remoteActReqData.ActionType = EnumRemoteCommand.TESTEND;
                    else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.WAFERUNLOAD.ToString())
                        remoteActReqData.ActionType = EnumRemoteCommand.WAFERUNLOAD;
                    else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.PABORT.ToString())
                        remoteActReqData.ActionType = EnumRemoteCommand.PABORT;
                    else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.START_CARD_CHANGE.ToString())
                        remoteActReqData.ActionType = EnumRemoteCommand.START_CARD_CHANGE;
                    else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.MOVEIN_CARD_CLOSE_COVER.ToString())
                        remoteActReqData.ActionType = EnumRemoteCommand.MOVEIN_CARD_CLOSE_COVER;
                    else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.TC_END.ToString())
                        remoteActReqData.ActionType = EnumRemoteCommand.TC_END;

                }
                else if(REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.PROCEED_CARD_CHANGE.ToString() ||
                        REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.SKIP_CARD_ATTACH.ToString()
                        )
                {
                    //long pnObjectID = 0;
                    byte HAACK = 0x04;
                    long objid = 0;
                    MakeObject(ref objid);
                    SetListItem(objid, 2);
                    SetBinaryItem(objid, HAACK);
                    SetListItem(objid, 0);
                    //SendSECSMessage(objid, nStream, nFunction + 1, nSysbyte);

                    var actreqdata = new StageSeqActReqData();
                    actreqdata.ObjectID = objid;
                    actreqdata.Stream = nStream;
                    actreqdata.Function = nFunction;
                    actreqdata.Sysbyte = nSysbyte;

                    //long dataListCount = 0;
                    string CPNAME = null;
                    string stagenumber = null;
                    byte[] endseq = new byte[1];
                    Gem.GetList(nMsgId, ref subListCount);

                    Gem.GetList(nMsgId, ref subItemListCount);// [L] StageNumber
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] StageNumber
                    Gem.GetAscii(nMsgId, ref stagenumber);// [A] StageNumber

                    Gem.GetList(nMsgId, ref subItemListCount);// [L] endseq
                    Gem.GetAscii(nMsgId, ref CPNAME);// [U1] endseq
                    Gem.GetU1(nMsgId, ref endseq);// [U1] endseq


                    actreqdata.StageNumber = Convert.ToInt32(stagenumber); //Data [U] StageNumber
                    actreqdata.EndSeq = Convert.ToBoolean(endseq[0]);

                    remoteActReqData = actreqdata;

                    if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.PROCEED_CARD_CHANGE.ToString())
                        remoteActReqData.ActionType = EnumRemoteCommand.PROCEED_CARD_CHANGE;
                    if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.SKIP_CARD_ATTACH.ToString())
                        remoteActReqData.ActionType = EnumRemoteCommand.SKIP_CARD_ATTACH;                    
                }               
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.END_TEST_LP.ToString())
                {
                    //long pnObjectID = 0;
                    byte HAACK = 0x04;
                    long objid = 0;
                    MakeObject(ref objid);
                    SetListItem(objid, 2);
                    SetBinaryItem(objid, HAACK);
                    SetListItem(objid, 0);
                    //SendSECSMessage(objid, nStream, nFunction + 1, nSysbyte);

                    var actreqdata = new EndTestReqLPDate();
                    actreqdata.ObjectID = objid;
                    actreqdata.Stream = nStream;
                    actreqdata.Function = nFunction;
                    actreqdata.Sysbyte = nSysbyte;

                    //long dataListCount = 0;
                    string CPNAME = null;
                    string stagenumber = null;
                    string UnloadFoupNumber = null;
                    Gem.GetList(nMsgId, ref subListCount);

                    Gem.GetList(nMsgId, ref subItemListCount);// [L] StageNumber
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] StageNumber
                    Gem.GetAscii(nMsgId, ref stagenumber);// [A] StageNumber
                    actreqdata.StageNumber = Convert.ToInt32(stagenumber); //Data [U] StageNumber

                    Gem.GetList(nMsgId, ref subItemListCount);// [L] UnloadFoupNumber
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] UnloadFoupNumber
                    Gem.GetAscii(nMsgId, ref UnloadFoupNumber);// [A] UnloadFoupNumber
                    actreqdata.FoupNumber = Convert.ToInt32(UnloadFoupNumber); //Data [U] StageNumber

                    remoteActReqData = actreqdata;
                    remoteActReqData.ActionType = EnumRemoteCommand.END_TEST_LP;

                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.WAFER_CHANGE.ToString())
                {
                    var actreqdata = new WaferChangeData();
                    actreqdata.ObjectID = nMsgId;
                    actreqdata.Stream = nStream;
                    actreqdata.Function = nFunction;
                    actreqdata.Sysbyte = nSysbyte;

                    long dflist1 = 0;
                    long dflist2 = 0;
                    long dflist3 = 0;
                    long dflist4 = 0;

                    string CPNAME = null;
                    uint[] ocrRead = new uint[1];
                    string waferID = null;
                    string location1_LP = null;
                    string location1_Atom_Idx = null;
                    string location2_LP = null;
                    string location2_Atom_Idx = null;

                    Gem.GetList(nMsgId, ref dflist1); // L, 2
                    Gem.GetList(nMsgId, ref dflist1); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] OCR Read
                    Gem.GetU4(nMsgId, ref ocrRead);// [U4] Result 0 = Pass, 1 = Fail, 2 = Auto
                    actreqdata.OCRRead = (int)ocrRead[0];
                    Gem.GetList(nMsgId, ref dflist2); // L, n

                    long listcount = dflist2;
                    actreqdata.WaferID  = new string[listcount];
                    actreqdata.LOC1_LP = new string[listcount];
                    actreqdata.LOC1_Atom_Idx = new string[listcount];
                    actreqdata.LOC2_LP = new string[listcount];
                    actreqdata.LOC2_Atom_Idx = new string[listcount];

                    for (int index = 0; index < listcount; index++)
                    {
                        Gem.GetList(nMsgId, ref dflist3); // L, 5

                        Gem.GetList(nMsgId, ref dflist4); // L, 2
                        Gem.GetAscii(nMsgId, ref CPNAME);// [A] "WAFERID"  
                        Gem.GetAscii(nMsgId, ref waferID);// [A] 'WaferID"
                        actreqdata.WaferID[index] = waferID;

                        Gem.GetList(nMsgId, ref dflist4); // L, 2
                        Gem.GetAscii(nMsgId, ref CPNAME);// [A] "LOC1_LP"  
                        Gem.GetAscii(nMsgId, ref location1_LP);// [A] 'LP#No.'
                        actreqdata.LOC1_LP[index] = location1_LP;

                        Gem.GetList(nMsgId, ref dflist4); // L, 2
                        Gem.GetAscii(nMsgId, ref CPNAME);// [A] "LOC1_ATOM_IDX"  
                        Gem.GetAscii(nMsgId, ref location1_Atom_Idx);// [A] 'S#No.' or 'F#No.'..
                        actreqdata.LOC1_Atom_Idx[index] = location1_Atom_Idx;

                        Gem.GetList(nMsgId, ref dflist4); // L, 2
                        Gem.GetAscii(nMsgId, ref CPNAME);// [A] "LOC2_LP"  
                        Gem.GetAscii(nMsgId, ref location2_LP);// [A] 'LP#No.'
                        actreqdata.LOC2_LP[index] = location2_LP;

                        Gem.GetList(nMsgId, ref dflist4); // L, 2
                        Gem.GetAscii(nMsgId, ref CPNAME);// [A] "LOC2_ATOM_IDX"  
                        Gem.GetAscii(nMsgId, ref location2_Atom_Idx);// [A] 'S#No.' or 'F#No.'..
                        actreqdata.LOC2_Atom_Idx[index] = location2_Atom_Idx;
                    }
                    remoteActReqData = actreqdata;
                    remoteActReqData.ActionType = EnumRemoteCommand.WAFER_CHANGE;
                }
                else
                {
                    long objid = 0;
                    long pnMsgId = 0;
                    byte HAACK = 0x01;
                    MakeObject(ref pnMsgId);
                    SetListItem(pnMsgId, 2);
                    SetBinaryItem(pnMsgId, HAACK);
                    SetListItem(pnMsgId, 0);
                    //SendSECSMessage(pnMsgId, nStream, nFunction + 1, nSysbyte);

                    OnlyReqData actreqdata = new OnlyReqData();
                    actreqdata.ObjectID = objid;
                    actreqdata.Stream = nStream;
                    actreqdata.Function = nFunction;
                    actreqdata.Sysbyte = nSysbyte;


                    remoteActReqData = actreqdata;
                    if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.CARD_SEQ_ABORT.ToString())
                        remoteActReqData.ActionType = EnumRemoteCommand.CARD_SEQ_ABORT;
                }

                long pnObjectID = 0;

                remoteActReqData.Stream = nStream;
                remoteActReqData.ObjectID = pnObjectID;//
                remoteActReqData.Function = nFunction;
                remoteActReqData.Sysbyte = nSysbyte;

                Gem.CloseObject(nMsgId);
                if (remoteActReqData != null)
                {
                    callback?.OnRemoteCommandAction(remoteActReqData); // Act



                    // 이게 맞는거긴한데 지금은 RemoteAction에서 SendAck 해버려서 일단 주석함..

                    //if (nFunction == 1)
                    //{                      
                    //    long HCACK = 0x04;
                    //    callback?.GEMRspRemoteCommand(nMsgId, Rcmd.ToString().ToUpper(), HCACK, subListCount, RetValues.ToArray());  
                    //    // 작업이 성공적으로 수행하지 않았을 경우에는 음수를 Retvalues에 넣어줄것..                      
                    //}
                    //else if(nFunction == 9)
                    //{
                    //    callback?.RemoteActMsgReceive(remoteActReqData); // Send ACK
                    //}                                        
                }
            }
            catch (Exception err)
            {
                throw err;
                //LoggerManager.Error(err.Message, new List<string>() { err.StackTrace });
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
