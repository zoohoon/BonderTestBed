using System;

namespace SecsGemReceiveModules
{
    using SecsGemServiceInterface;
    using XGEMWrapper;

    public class SemicsGemReceiverSEKS : ISecsGemMessageReceiver
    {
        //private XGemNet Gem { get; set; }
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
                if( nStream == 3 && nFunction == 17)
                {
                    XGEM_OnSECSMessageReceive_S3F17(nMsgId, nStream, nFunction, nSysbyte);
                }
                else if ((nStream == 2 && nFunction == 49) || (nStream == 2 && nFunction == 41))
                {
                    XGEM_OnSECSMessageReceive_S2F4X(nMsgId, nStream, nFunction, nSysbyte);
                }
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        private void XGEM_OnSECSMessageReceive_S3F17(long nMsgId, long nStream, long nFunction, long nSysbyte)
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

            CarrierActReqData carrierActReqData = null;

            int total_cnt = 0;

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
                        if (actCarrierData.CattrID[i].ToUpper() == "LOTID")
                        {
                            actCarrierData.LotID = actCarrierData.CattrData[i];
                        }
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
                actCarrierData.DataID = USER_UINT4[0];

                string lotidval = string.Empty;
                string cellinfoval = string.Empty;
                string slotInfoval = string.Empty;

                for (int i = 0; i < subListCount; i++)
                {
                    var keystr = string.Empty;
                    Gem.GetList(nMsgId, ref dataListCount);
                    Gem.GetAscii(nMsgId, ref keystr);

                    if (keystr.ToUpper() == "LOTID")
                    {
                        Gem.GetAscii(nMsgId, ref lotidval);
                        actCarrierData.LOTID = lotidval;
                    }
                    else if (keystr.ToUpper() == "CELLINFO")
                    {
                        Gem.GetAscii(nMsgId, ref cellinfoval);    // [CPVAL][A] CELL INFO
                        actCarrierData.CellMap = cellinfoval;
                    }
                    else if (keystr.ToUpper() == "SLOTINFO")
                    {
                        Gem.GetAscii(nMsgId, ref slotInfoval); // [CPVAL][A] SLOT INFO
                        actCarrierData.SlotMap = slotInfoval;
                    }
                }

                carrierActReqData = actCarrierData;
            }
            else if (CARRIERACTION.ToUpper() == EnumCarrierAction.CANCELCARRIER.ToString())
            {
                var actCarrierData = new CarrieActReqData();

                actCarrierData.ActionType = EnumCarrierAction.CANCELCARRIER;

                actCarrierData.Sysbyte = nSysbyte;
                actCarrierData.DataID = USER_UINT4[0];
                actCarrierData.CarrierAction = CARRIERACTION;
                actCarrierData.CarrierID = CARRIERID;
                actCarrierData.PTN = PTN[0];
                actCarrierData.Count = subListCount;

                string lotidval = string.Empty;

                for (int i = 0; i < subListCount; i++)
                {
                    var keystr = string.Empty;
                    Gem.GetList(nMsgId, ref dataListCount);
                    Gem.GetAscii(nMsgId, ref keystr);

                    if (keystr.ToUpper() == "LOTID")
                    {
                        Gem.GetAscii(nMsgId, ref lotidval);
                        actCarrierData.LOTID = lotidval;
                    }
                    else 
                    {
                        //CARRIER DATA
                        Gem.GetAscii(nMsgId, ref CATTRDATA);
                        actCarrierData.CarrierData = CATTRDATA;
                    }

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

                Gem.SendSECSMessage(pnMsgId, nStream, nFunction + 1, nSysbyte);
                carrierActReqData = null;
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

        private void XGEM_OnSECSMessageReceive_S2F4X(long nMsgId, long nStream, long nFunction, long nSysbyte)
        {
            try
            {
                RemoteActReqData remoteActReqData = null;
                long listCount1 = 0;

                string REMOTE_COMMAND_ACTION = null;
                Gem.GetList(nMsgId, ref listCount1);
                Gem.GetAscii(nMsgId, ref REMOTE_COMMAND_ACTION);

                if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.PSTART.ToString())
                {
                    var actreqdata = new StartLotActReqData();
                    actreqdata.ObjectID = nMsgId;
                    actreqdata.Stream = nStream;
                    actreqdata.Function = nFunction;
                    actreqdata.Sysbyte = nSysbyte;

                    long dflist1 = 0;
                    string CPNAME = null;
                    string lotId = null;
                    string ocrRead = null;
                    byte[] foupnumber = new byte[1];

                    Gem.GetList(nMsgId, ref dflist1);

                    Gem.GetList(nMsgId, ref dflist1);// [L] LOT ID 
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] LOT ID 
                    Gem.GetAscii(nMsgId, ref lotId);// [A] LOT ID 
                    actreqdata.LotID = lotId;

                    Gem.GetList(nMsgId, ref dflist1);// [L] OCR READ 
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] OCR READ
                    Gem.GetAscii(nMsgId, ref ocrRead);// [U] OCR READ
                    actreqdata.OCRReadFalg = Convert.ToInt32(ocrRead);

                    Gem.GetList(nMsgId, ref dflist1);// [L] FoupNumber
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] PTN
                    Gem.GetU1(nMsgId, ref foupnumber);// [U] PTN
                    actreqdata.FoupNumber = Convert.ToInt32(foupnumber[0]);

                    remoteActReqData = actreqdata;
                    remoteActReqData.ActionType = EnumRemoteCommand.PSTART;
                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.PP_SELECT.ToString())
                {
                    var actreqdata = new PPSelectActReqData();
                    actreqdata.ObjectID = nMsgId;
                    actreqdata.Stream = nStream;
                    actreqdata.Function = nFunction;
                    actreqdata.Sysbyte = nSysbyte;

                    long listcount = 0;

                    Gem.GetList(nMsgId, ref listcount);

                    for (int i = 0; i < listcount; i++)
                    {
                        long sublistcount = 0;
                        Gem.GetList(nMsgId, ref sublistcount);
                        for (int j = 0; j < sublistcount; j++)
                        {
                            string key_ = string.Empty;
                            string val_ = string.Empty;
                            Gem.GetAscii(nMsgId, ref key_);       // [A] Key = 'PPID'
                            Gem.GetAscii(nMsgId, ref val_);       // [A] Value

                            if (key_.ToUpper().Equals("PPID"))
                                actreqdata.Ppid = val_;
                            else if (key_.ToUpper().Equals("CELLINFO"))
                                actreqdata.UseStageNumbers_str = val_;
                        }
                    }

                    remoteActReqData = actreqdata;
                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.ONLINEPP_SELECT.ToString())
                {
                    var actreqdata = new ONLINEPPSelectActReqData();
                    actreqdata.ObjectID = nMsgId;
                    actreqdata.Stream = nStream;
                    actreqdata.Function = nFunction;
                    actreqdata.Sysbyte = nSysbyte;

                    long listcount = 0;

                    Gem.GetList(nMsgId, ref listcount);

                    for (int i = 0; i < listcount; i++)
                    {
                        long sublistcount = 0;
                        Gem.GetList(nMsgId, ref sublistcount);
                        //for (int j = 0; j < sublistcount; j++)
                        {
                            string key_ = string.Empty;
                            string val_ = string.Empty;
                            byte[] ptn_ = new byte[1];
                            
                            Gem.GetAscii(nMsgId, ref key_);       // [A] Key = 'PPID'
                            if (key_.ToUpper().Equals("PTN"))
                            {
                                Gem.GetU1(nMsgId, ref ptn_);
                            }
                            else
                            {
                                Gem.GetAscii(nMsgId, ref val_);       // [A] Value
                            }
                            
                            if (key_.ToUpper().Equals("PTN"))
                                actreqdata.PTN = ptn_[0];
                            else if (key_.ToUpper().Equals("LOTID"))
                                actreqdata.LotID = val_;
                            else if (key_.ToUpper().Equals("PPID"))
                                actreqdata.Ppid = val_;
                            else if (key_.ToUpper().Equals("CELLINFO"))
                                actreqdata.UseStageNumbers_str = val_;
                        }
                    }
                    
                    remoteActReqData = actreqdata;
                }
                else
                {
                    long pnMsgId = 0;
                    byte HAACK = 0x01;
                    MakeObject(ref pnMsgId);
                    SetListItem(pnMsgId, 2);
                    SetBinaryItem(pnMsgId, HAACK);
                    SetListItem(pnMsgId, 0);
                    SendSECSMessage(pnMsgId, nStream, nFunction + 1, nSysbyte);
                    remoteActReqData = null;
                }

                Gem.CloseObject(nMsgId);
                if(remoteActReqData != null)
                {
                    callback?.OnRemoteCommandAction(remoteActReqData);
                }
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
