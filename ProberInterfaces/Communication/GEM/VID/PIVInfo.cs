using System.Collections.Generic;
namespace ProberInterfaces
{
    public class PIVInfo
    {
        #region <remarks> STAGE </remarks>
        public int StageNumber { get; set; }
        public GEMStageStateEnum StageState { get; set; }
        public GEMPreHeatStateEnum PreHeatState { get; set; }
        public string LotID { get; set; }
        public string WaferID { get; set; }
        public string ProbeCardID { get; set; }
        public string RecipeID { get; set; }
        public int SlotNumber { get; set; }
        //public int LoadFoupNumber { get; set; }
        public double CurTemperature { get; set; }
        public double SetTemperature { get; set; }
        public double Overdrive { get; set; }
        public long XCoordinate { get; set; }
        public long YCoordinate { get; set; }

        public string CardSiteLocation { get; set; }
        public string SystemClock { get; set; }
        public string WaferStartTime { get; set; }
        public string WaferEndTime { get; set; }
        public long PCardContactCount { get; set; }
        public int NotchAngle { get; set; }
        public int WaferEndResult { get; set; }
        public long TotalDieCount { get; set; }
        public long PassDieCount { get; set; }
        public long FailDieCount { get; set; }
        public long YieldOfBadDie { get; set; }
        public string UnloadingWaferID { get; set; }
        public int UnloadingSlotNum { get; set; }
        public int CleaningTouchDownCount { get; set; }
        public int DevDownResult { get; set; }
        /// <summary>
        /// 만약 FoupNumber가 다른 DL 레시피가 들어온다면 기존에 프로빙하고 있던 웨이퍼의 이벤트는 FoupNumber를 기존 값으로 보내줘야하기 때문.
        /// </summary>
        public int OriginLoadFoupNumber { get; set; } // Cascading 시 현재 동작중인 FoupNumber 로 복귀하기 위해 존재.
        public int PreAlignNumber { get; set; }
        public double PinAlignPlanarity { get; set; }        
        public double PinAlignAngle { get; set; }
        public double PinAlignCardCenterX { get; set; }
        public double PinAlignCardCenterY { get; set; }
        public double PinAlignCardCenterZ { get; set; }

        public List<List<double>> PinAlignResults { get; set; }
        #endregion

        #region <remarks> LOADER </remarks>
        public string LoaderID { get; set; }
        public int FoupNumber { get; set; }
        public int CardBufferIndex { get; set; }// 1 ~
        public GEMFoupStateEnum FoupState { get; set; }
        public int FoupShiftMode { get; set; }
        public int PreLoadingSlotNum { get; set; }
        public string PreLoadingWaferId { get; set; }
        public string ListOfStages { get; set; }
        public string ListOfSlot { get; set; }
        public string CarrierID { get; set; }
        public string ProberID { get; set; }
        public string VerifyParamResultMap { get; set; }
        public int WaferAutofeedResult { get; set; }
        public int PolishWaferIDReadResult { get; set; }
        public string FullSite { get; set; }
        public string WaferChange_Location1_LoadPortId { get; set; }
        public string WaferChange_Location1_AtomId { get; set; }
        public string WaferChange_Location2_LoadPortId { get; set; }
        public string WaferChange_Location2_AtomId { get; set; }
        public string WaferChange_Location1_WaferId { get; set; }
        public string WaferChange_Location2_WaferId { get; set; }
        #endregion


        public PIVInfo()
        {

        }

        public PIVInfo(int stagenumber = 0, GEMStageStateEnum stagestate = GEMStageStateEnum.IDLE, GEMPreHeatStateEnum preheatstate = GEMPreHeatStateEnum.UNDIFIND,
            string loaderid = "", string lotid = "", string waferid = "", string probecardid = "", string receipeid = "", int slotnumber = 0,
            int originloadportnumber = 0, double od = -10000, int notchangle = 0, double curtemperature = 0, double settemperature = 0, long xcoord = 0, long ycoord = 0,
            string waferStartTime = "", string waferEndTime = "", long totalDieCount = 0, long passDieCount = 0, long faildiecount = 0, long yieldbaddie = 0, int waferendrst = 1, int cleaningtouchdowncount = 0,
            int unloadingslotnum = 0, string unloadingwid = "", long pcardcontactcount = 0, int foupshiftmode = 0, string proberid = "", string preloadingWaferId = "", int preloadingSlotIndex = 0,
            int devdownresult = 0, int foupnumber = 0, int cardbufferindex = 0, GEMFoupStateEnum foupstate = GEMFoupStateEnum.UNDIFIND, string listofstage = "", string listofslot = "",
            int prealignnumber = 0, int armalignnumber = 0, string carrierid = "", string verifyparammap = "", string systemclock = "", int waferAutofeedResult = 0, int PolishWaferIDReadResult = 0,
            double pinAlignPlanarity = 0, double pinAlignAngle = 0, double pinAlignCardCenterX = 0, double pinAlignCardCenterY = 0, double pinAlignCardCenterZ = 0, List<List<double>> pinalignresults = null, string FullSite = "",
            string waferchange_Loc1_LpId = "", string waferChange_Loc1_AtomId = "", string waferchange_Loc2_LpId = "", string WaferChange_Loc2_AtomId = "", string WaferChange_Location1_WaferId = "", string WaferChange_Location2_WaferId = "")
        {
            this.LoaderID = loaderid;
            this.StageNumber = stagenumber;
            this.StageState = stagestate;
            this.PreHeatState = preheatstate;
            this.LotID = lotid;
            this.FoupShiftMode = foupshiftmode;
            this.WaferID = waferid;
            this.ProbeCardID = probecardid;
            this.RecipeID = receipeid;
            this.SlotNumber = slotnumber;
            //this.LoadFoupNumber = loadfoupnumber;
            this.Overdrive = od;
            this.NotchAngle = notchangle;
            this.CurTemperature = curtemperature;
            this.SetTemperature = settemperature;
            this.XCoordinate = xcoord;
            this.YCoordinate = ycoord;            
            this.WaferStartTime = waferStartTime;
            this.WaferEndTime = waferEndTime;
            this.TotalDieCount = totalDieCount;
            this.PassDieCount = passDieCount;
            this.FailDieCount = faildiecount;
            this.YieldOfBadDie = yieldbaddie;
            this.WaferEndResult = waferendrst;
            this.PreLoadingSlotNum = preloadingSlotIndex;
            this.PreLoadingWaferId = preloadingWaferId;
            this.UnloadingSlotNum = unloadingslotnum;
            this.UnloadingWaferID = unloadingwid;
            this.CleaningTouchDownCount = cleaningtouchdowncount;
            this.PCardContactCount = pcardcontactcount;
            this.DevDownResult = devdownresult;
            this.FoupNumber = foupnumber;
            this.CardBufferIndex = cardbufferindex;
            this.FoupState = foupstate;
            this.ListOfStages = listofstage;
            this.ListOfSlot = listofslot;
            this.PreAlignNumber = prealignnumber;
            this.CarrierID = carrierid;
            this.SystemClock = systemclock;
            this.ProberID = proberid;
            this.VerifyParamResultMap = verifyparammap;
            this.PinAlignPlanarity = pinAlignPlanarity;
            this.PinAlignAngle = pinAlignAngle;
            this.PinAlignCardCenterX = pinAlignCardCenterX;
            this.PinAlignCardCenterY = pinAlignCardCenterY;
            this.PinAlignCardCenterZ = pinAlignCardCenterZ;
            this.PinAlignResults = pinalignresults;
            this.OriginLoadFoupNumber = originloadportnumber;
            this.WaferAutofeedResult = waferAutofeedResult;
            this.PolishWaferIDReadResult = PolishWaferIDReadResult;

            this.FullSite = FullSite;

            this.WaferChange_Location1_LoadPortId = waferchange_Loc1_LpId;
            this.WaferChange_Location1_AtomId = waferChange_Loc1_AtomId;
            this.WaferChange_Location2_LoadPortId = waferchange_Loc2_LpId;
            this.WaferChange_Location2_AtomId = WaferChange_Loc2_AtomId;
            this.WaferChange_Location1_WaferId = WaferChange_Location1_WaferId;
            this.WaferChange_Location2_WaferId = WaferChange_Location2_WaferId;

        }

    }
}
