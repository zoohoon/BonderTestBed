using System.Collections.Generic;

namespace SecsGemServiceInterface
{
    using System.Runtime.Serialization;

    [DataContract]
    [KnownType(typeof(ActiveProcessActReqData))]
    [KnownType(typeof(DownloadStageRecipeActReqData))]
    [KnownType(typeof(SetParameterActReqData))]
    [KnownType(typeof(SetParameterInfo))]
    [KnownType(typeof(StageSetParameterInfo))]
    [KnownType(typeof(DockFoupActReqData))]
    [KnownType(typeof(SelectSlotsActReqData))]
    [KnownType(typeof(SelectStageSlotActReqData))]
    [KnownType(typeof(SelectSlotsStagesActReqData))]
    [KnownType(typeof(SelectStageSlotsActReqData))]
    [KnownType(typeof(StartLotActReqData))]
    //[KnownType(typeof(StartLotSerpCarrierActReqData))]
    [KnownType(typeof(WaferConfirmActReqData))]
    [KnownType(typeof(ZUpActReqData))]
    [KnownType(typeof(EndTestReqDate))]
    [KnownType(typeof(EndTestReqLPDate))]
    [KnownType(typeof(CarrierCancleData))]
    [KnownType(typeof(CarrierIdentityData))]
    [KnownType(typeof(ErrorEndData))]
    [KnownType(typeof(ErrorEndLPData))]
    [KnownType(typeof(StartStage))]
    [KnownType(typeof(StageSeqActReqData))]
    [KnownType(typeof(PStartActReqData))]
    [KnownType(typeof(StageActReqData))]
    [KnownType(typeof(UnDockReqData))]
    [KnownType(typeof(AssignWaferIDMap))]
    [KnownType(typeof(OnlyReqData))]
    [KnownType(typeof(PPSelectActReqData))]
    [KnownType(typeof(TC_EndTestReqDate))]
    [KnownType(typeof(ONLINEPPSelectActReqData))]

    [KnownType(typeof(ChangeLoadPortModeActReqData))]
    [KnownType(typeof(WaferChangeData))]
    public abstract class RemoteActReqData
    {
        [DataMember]
        public EnumRemoteCommand ActionType { get; set; }
        [DataMember]
        public long Count { get; set; }
        [DataMember]
        public long ObjectID { get; set; }
        [DataMember]
        public long Stream { get; set; }
        [DataMember]
        public long Function { get; set; }
        [DataMember]
        public long Sysbyte { get; set; }
    }

    [DataContract]
    public class OnlyReqData : RemoteActReqData
    { 
    
    }


    #region<remarks> ActiveProcessActReqData </remarks>
        [DataContract]
    public class ActiveProcessActReqData : RemoteActReqData
    {
        [DataMember]
        public int FoupNumber { get; set; }
        [DataMember]
        public string LotID { get; set; }
        [DataMember]
        public string UseStageNumbers_str { get; set; }
        [DataMember]
        public List<int> UseStageNumbers { get; set; } = new List<int>();

        public ActiveProcessActReqData()
        {
            this.UseStageNumbers_str = string.Empty;
            this.UseStageNumbers = new List<int>();
        }
    }
    #endregion

    #region <remarks> DownloadStageRecipeActReqData </remarks>
    public class DownloadStageRecipeActReqData : RemoteActReqData
    {
        [DataMember]
        public int FoupNumber { get; set; }
        [DataMember]
        public string LotID { get; set; }
        [DataMember]
        public bool UseFTP { get; set; } = true;
        /// <summary>
        /// StageNumber, RecipeID
        /// </summary>
        [DataMember]
        public Dictionary<int, string> RecipeDic { get; set; } = new Dictionary<int, string>();

        public DownloadStageRecipeActReqData()
        {
            this.ActionType = EnumRemoteCommand.DOWNLOAD_STAGE_RECIPE;
        }
    }

    #endregion

    #region <remarks> SetParameterActReqData </remarks> 

    [DataContract]
    public class SetParameterActReqData : RemoteActReqData
    {
        [DataMember]
        public int FoupNumber { get; set; }
        [DataMember]
        public string LotID { get; set; }
        [DataMember]
        public string RecipeId { get; set; }

        /// <summary>
        /// Descriprion , Value
        /// </summary>
        [DataMember]
        public Dictionary<string, string> ParameterDic { get; set; } = new Dictionary<string, string>();
        [DataMember]
        public List<SetParameterInfo> ParameterInfos { get; set; } = new List<SetParameterInfo>();
        [DataMember]
        public List<int> UseStages { get; set; }
    }

    [DataContract]
    public class SetParameterInfo
    {
        //[DataMember]
        //public int StageNumber { get; set; }
        [DataMember]
        public List<StageSetParameterInfo> StageParameterInfos { get; set; } = new List<StageSetParameterInfo>();
    }

    [DataContract]
    public class StageSetParameterInfo
    {
        /// <summary>
        ///SVID : 1
        ///DVID : 2
        ///ECID : 3
        /// </summary>
        [DataMember]
        public string VidType { get; set; }
        [DataMember]
        public string ParameterName { get; set; }
        [DataMember]
        public string Value { get; set; }
    }

    #endregion

    #region <remarks> DockFoupActReqData </remarks>
    [DataContract]
    public class DockFoupActReqData : RemoteActReqData
    {
        [DataMember]
        public int FoupNumber { get; set; }

        public DockFoupActReqData()
        {
            this.ActionType = EnumRemoteCommand.DOCK_FOUP;
        }
    }
    #endregion

    #region <remarks> SelectSlotsActReqData </remarks>

    [DataContract]
    public class SelectSlotsActReqData : RemoteActReqData
    {
        [DataMember]
        public int FoupNumber { get; set; }
        [DataMember]
        public string LotID { get; set; }
        [DataMember]
        public string UseSlotNumbers_str { get; set; }
        [DataMember]
        public List<int> UseSlotNumbers { get; set; } = new List<int>();

        public SelectSlotsActReqData()
        {
            this.ActionType = EnumRemoteCommand.SELECT_SLOTS;
        }
    }

    #endregion

    #region <remarks> SelectSlotsActReqData </remarks>

    [DataContract]
    public class SelectStageSlotsActReqData : RemoteActReqData
    {
        [DataMember]
        public int PTN { get; set; }
        [DataMember]
        public string LotID { get; set; }
        [DataMember]
        public string CellMap { get; set; }
        [DataMember]
        public string UseSlotStageNumbers_str { get; set; }
        [DataMember]
        public List<int> UseSlotNumbers { get; set; } = new List<int>();

        //public SelectStageSlotsActReqData()
        //{
        //    this.ActionType = EnumRemoteCommand.SELECT_SLOTS_STAGES;
        //}
    }

    #endregion

    #region <remarks> SelectStageSlotActReqData </remarks>

    [DataContract]
    public class SelectStageSlotActReqData : RemoteActReqData
    {

        [DataMember]
        public int PTN { get; set; }
        [DataMember]
        public string CarrierId { get; set; }
        [DataMember]
        public string SlotMap { get; set; }
        [DataMember]
        public string CellMap { get; set; }

    }
    #endregion

    #region </remarks> [Class] Select Slot Stage </remarks>

    [DataContract]
    public class SelectSlotsStagesActReqData : RemoteActReqData
    {
        [DataMember]
        public int FoupNumber { get; set; }
        [DataMember]
        public string LotID { get; set; }
        [DataMember]
        public string UseSlotNumbers_str { get; set; }
        [DataMember]
        public List<SlotCellInfo> SlotStageNumbers { get; set; } = new List<SlotCellInfo>();
    }



    [DataContract]
    public class SlotCellInfo
    {
        [DataMember]
        public int SlotIndex { get; set; }
        [DataMember]
        public List<int> CellIndexs { get; set; } = new List<int>();

        public SlotCellInfo(int slotindex)
        {
            SlotIndex = slotindex;
        }
    }



    #endregion
    #region <remarks> StartLotActReqData </remarks>
    [DataContract]
    public class StartLotActReqData : RemoteActReqData
    {
        [DataMember]
        public int FoupNumber { get; set; }
        [DataMember]
        public string LotID { get; set; }
        [DataMember]
        public string CarrierID { get; set; }
        [DataMember]
        public int OCRReadFalg { get; set; } = -1;

        public StartLotActReqData()
        {
            this.ActionType = EnumRemoteCommand.START_LOT;
        }
    }
    #endregion



    #region <remarks> WaferConfirmActReqData </remarks>
    [DataContract]
    public class WaferConfirmActReqData : RemoteActReqData
    {
        [DataMember]
        public string LotID { get; set; }
        [DataMember]
        public int PTN { get; set; }
        [DataMember]
        public int SlotNum { get; set; }
        [DataMember]
        public string WaferId { get; set; }
        [DataMember]
        public int OCRReadFalg { get; set; } = -1;

        public WaferConfirmActReqData()
        {
            this.ActionType = EnumRemoteCommand.WFIDCONFPROC;
        }
    }
    #endregion

    #region <remarks> ZUpActReqData </remarks>
    [DataContract]
    public class ZUpActReqData : RemoteActReqData
    {
        [DataMember]
        public int StageNumber { get; set; }
    }
    #endregion
   

    #region  <remarks> [Class] EndTest </remarks>
    [DataContract]
    public class EndTestReqDate : RemoteActReqData
    {
        [DataMember]
        public int StageNumber { get; set; }

        //PMIExecFlag (U4)
        //0: No effect from Host.Prober decides PMI execute timing by itself
        //1: Skip PMI
        //2: Execute PMI
        [DataMember]
        public int PMIExecFlag { get; set; }
    }
    #endregion
    #region  <remarks> [Class] EndTest </remarks>
    [DataContract]
    public class TC_EndTestReqDate : RemoteActReqData
    {
        [DataMember]
        public int StageNumber { get; set; }

    }
    #endregion

    #region <remarks> [Class] End Test LP </remarks>
    [DataContract]
    public class EndTestReqLPDate : RemoteActReqData
    {
        [DataMember]
        public int StageNumber { get; set; }

        //PMIExecFlag (U4)
        //0: No effect from Host.Prober decides PMI execute timing by itself
        //1: Skip PMI
        //2: Execute PMI
        [DataMember]
        public int PMIExecFlag { get; set; }

        // Unload wafer to Foup number
        [DataMember]
        public int FoupNumber { get; set; }
    }
    #endregion
    #region <remarks> CarrierCancleData </remarks>
    [DataContract]
    public class CarrierCancleData : RemoteActReqData
    {
        [DataMember]
        public int FoupNumber { get; set; }
    }
    #endregion

    #region <remarks> CarrierIdentityData </remarks>
    [DataContract]
    public class CarrierIdentityData : RemoteActReqData
    {
        [DataMember]
        public int PTN { get; set; }
        [DataMember]
        public string CarrierId { get; set; }
    }
    #endregion

    #region <remarks> SelectCellWithSlotData </remarks>
    [DataContract]
    public class SelectCellWithSlotData : RemoteActReqData
    {
        [DataMember]
        public int PTN { get; set; }
        [DataMember]
        public string CarrierId { get; set; }
    }
    #endregion

    #region <remarks> [Class] ErrorEndLP </remarks>
    [DataContract]
    public class ErrorEndLPData : RemoteActReqData
    {
        [DataMember]
        public int StageNumber { get; set; }
        [DataMember]
        public int FoupNumber { get; set; }
    }
    #endregion

    #region <remarks> ErrorEndData </remarks>
    [DataContract]
    public class ErrorEndData : RemoteActReqData
    {
        [DataMember]
        public int StageNumber { get; set; }
    }
    #endregion

    #region <remarks> StartStage </remarks>
    [DataContract]
    public class StartStage : RemoteActReqData
    {
        [DataMember]
        public int FoupNumber { get; set; }
        [DataMember]
        public string LotID { get; set; }
        [DataMember]
        public int StageNumber { get; set; }
    }
    #endregion

    #region <remarks> PStartActReqData </remarks>

    [DataContract]
    public class PStartActReqData : RemoteActReqData
    {
        [DataMember]
        public string LotID { get; set; }
        [DataMember]
        public int OCRReadFalg { get; set; }
    }
    #endregion

    #region <remarks> StageActReqData (property is only stage number)  </remarks>
    [DataContract]
    public class StageActReqData : RemoteActReqData
    {
        [DataMember]
        public int StageNumber { get; set; }
    }
    #endregion

    #region to clear cardchange sequence 
    [DataContract]
    public class StageSeqActReqData : RemoteActReqData
    {
        [DataMember]
        public int StageNumber { get; set; }
        [DataMember]
        public bool EndSeq { get; set; }
    }
    #endregion

    #region <remarks> UnDockReqData </remarks>
    [DataContract]
    public class UnDockReqData : RemoteActReqData
    {
        [DataMember]
        public int LoadPortNumber { get; set; }
    }
    #endregion

    #region  <remarks> [Class] Assign wafer id map </remarks>
    [DataContract]
    public class AssignWaferIDMap : RemoteActReqData
    {
        [DataMember]
        public int FoupNumber { get; set; }
        [DataMember]
        public string LotID { get; set; }
        [DataMember]
        public List<string> WaferIDs { get; set; } = new List<string>();
    }
    #endregion

    #region <remarks> PPSelectActReqData </remarks>
    [DataContract]
    public class PPSelectActReqData : RemoteActReqData
    {
        [DataMember]
        public string Ppid { get; set; }
        [DataMember]

        public List<int> UseStageNumbers
        {
            get; set;
        }
        [DataMember]
        private string _UseStageNumbers_str = "";
        [DataMember]
        public string UseStageNumbers_str
        {
            get
            {
                return _UseStageNumbers_str;
            }
            set
            {
                _UseStageNumbers_str = value;
                UseStageNumbers = GetStageList(_UseStageNumbers_str);

            }
        }
        public PPSelectActReqData()
        {
            this.ActionType = EnumRemoteCommand.PP_SELECT;
        }

        private List<int> GetStageList(string stagelist)
        {
            var ret = new List<int>();
            stagelist.Replace(",", "");
            for (int i = 0; i < stagelist.Length; i++)
            {
                if (stagelist[i] == '1' || stagelist[i].ToString().ToUpper() == "T")
                    ret.Add(i + 1);
            }
            return ret;
        }
    }
    #endregion

    #region <remarks> ONLINEPPSelectActReqData </remarks>
    [DataContract]
    public class ONLINEPPSelectActReqData : PPSelectActReqData
    {
        [DataMember]
        public int PTN { get; set; }
        [DataMember]
        public string LotID { get; set; }
       
        public ONLINEPPSelectActReqData()
        {
            this.ActionType = EnumRemoteCommand.ONLINEPP_SELECT;
        }
    }
    #endregion

    #region <remarks> DeviceChangeActReqData </remarks>
    [DataContract]
    public class DeviceChangeActReqData : RemoteActReqData
    {
        [DataMember]
        public string NewDeviceName { get; set; }
        public List<int> UsingStageList
        {
            get; set;
        }
        public DeviceChangeActReqData()
        {
            this.ActionType = EnumRemoteCommand.DEVICE_CHANGE;
        }
    }
    #endregion

    #region <remarks> LotModeChangeActReqData </remarks>
    [DataContract]
    public class LotModeChangeActReqData : RemoteActReqData
    {
        [DataMember]
        public string LotMode { get; set; }
        public List<int> UsingStageList { get; set; }
        public int PTN { get; set; }
        public LotModeChangeActReqData()
        {
            this.ActionType = EnumRemoteCommand.LOTMODE_CHANGE;
        }
    }
    #endregion

    #region <remarks> [Class] Change LoadPort Mode </remarks>
    [DataContract]
    public class ChangeLoadPortModeActReqData : RemoteActReqData
    {
        [DataMember]
        public int FoupNumber { get; set; }
        [DataMember]
        public int FoupModeState { get; set; }
    }
    #endregion

    #region <remarks> CarrierIdentityData </remarks>
    [DataContract]
    public class WaferChangeData : RemoteActReqData
    {
        [DataMember]
        public int OCRRead { get; set; }
        [DataMember]
        public string[] WaferID { get; set; }
        [DataMember]
        public string[] LOC1_LP { get; set; }
        [DataMember]
        public string[] LOC1_Atom_Idx { get; set; }
        [DataMember]
        public string[] LOC2_LP { get; set; }
        [DataMember]
        public string[] LOC2_Atom_Idx { get; set; }
    }
    #endregion
}
