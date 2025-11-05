namespace ProberInterfaces
{
    using LogModule;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading.Tasks;

    [Serializable]
    public class StageLotInfo
    {
        private int _StageState;
        [DataMember]
        public int StageState
        {
            get { return _StageState; }
            set { _StageState = value; }
        }

        private int _FoupIndex;
        [DataMember]
        public int FoupIndex
        {
            get { return _FoupIndex; }
            set { _FoupIndex = value; }
        }

        private string _CassetteHashCode = "";
        [DataMember]
        public string CassetteHashCode
        {
            get { return _CassetteHashCode; }
            set { _CassetteHashCode = value; }
        }

        private string _CarrierId;
        [DataMember]
        public string CarrierId
        {
            get { return _CarrierId; }
            set { _CarrierId = value; }
        }

        private string _LotID = "";
        [DataMember]
        public string LotID
        {
            get { return _LotID; }
            set { _LotID = value; }
        }

        private bool _IsLotStarted = false;
        [DataMember]
        public bool IsLotStarted
        {
            get { return _IsLotStarted; }
            set { _IsLotStarted = value; }
        }

        /// <summary>
        /// device change 시에. load시에만 업데이트함.
        /// </summary>
        private string _RecipeID;
        [DataMember]
        public string RecipeID
        {
            get { return _RecipeID; }
            set { _RecipeID = value; }
        }

        /// <summary>
        /// 웨이퍼 로드 시에 Set하고, unload시에 clear함.
        /// </summary>
        private string _WaferID;
        [DataMember]
        public string WaferID
        {
            get { return _WaferID; }
            set { _WaferID = value; }
        }

        private bool _ProcDevOP;
        //DevDownResult 와 DevLoadResult 에 대해서 변경 된 적이 있는지 체크.
        [DataMember]
        public bool ProcDevOP
        {
            get { return _ProcDevOP; }
            set { _ProcDevOP = value; }
        }


        private bool _DevDownResult = false;
        [DataMember]
        public bool DevDownResult
        {
            get { return _DevDownResult; }
            set { _DevDownResult = value; }
        }

        private bool _DevLoadResult = false;
        [DataMember]
        public bool DevLoadResult
        {
            get { return _DevLoadResult; }
            set { _DevLoadResult = value; }
        }

        /// <summary>
        /// load시에만 업데이트함.
        /// </summary>
        private int _SlotNum;
        [DataMember]
        public int SlotNum
        {
            get { return _SlotNum; }
            set { _SlotNum = value; }
        }

        /// <summary>
        /// Overdrive 바뀔때마다 이벤트로 Set호출함
        /// </summary>
        private double _Overdrive;
        [DataMember]
        public double Overdrive
        {
            get { return _Overdrive; }
            set { _Overdrive = value; }
        }

        /// <summary>
        /// init 시에만 Set함.
        /// </summary>
        private string _ProberID;
        [DataMember]
        public string ProberID
        {
            get { return _ProberID; }
            set { _ProberID = value; }
        }

        /// <summary>
        /// CardLoad시에 Update하고 Unload시에 Clear함
        /// </summary>
        private string _PCardID;
        [DataMember]
        public string PCardID
        {
            get { return _PCardID; }
            set { _PCardID = value; }
        }

        private LotAssignStateEnum _StageLotAssignState;
        [DataMember]
        public LotAssignStateEnum StageLotAssignState
        {
            get { return _StageLotAssignState; }
            set { _StageLotAssignState = value; }
        }


        public StageLotInfo()
        {

        }
        public StageLotInfo(int foupindex, string lotid, string recipeid, string csthashcode, string carrierId)
        {
            FoupIndex = foupindex;
            LotID = lotid;
            CarrierId = carrierId;
            RecipeID = recipeid;
            if (csthashcode == null)
            {
                CassetteHashCode = "";
            }
            else
            {
                CassetteHashCode = csthashcode;
            }
        }

        public void CopyTo(StageLotInfo target)
        {
            try
            {
                target.FoupIndex = this.FoupIndex;
                target.CarrierId = this.CarrierId;
                target.LotID = this.LotID;
                target.RecipeID = this.RecipeID;
                target.PCardID = this.PCardID;
                target.ProberID = this.ProberID;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }
    }

    public class FoupLotInfo
    {
        private int _FoupNumber;

        public int FoupNumber
        {
            get { return _FoupNumber; }
            set { _FoupNumber = value; }
        }

        private int _FoupState;

        public int FoupState
        {
            get { return _FoupState; }
            set { _FoupState = value; }
        }

        private string _LotID = "";

        public string LotID
        {
            get { return _LotID; }
            set { _LotID = value; }
        }

        private string _StageList = "";

        public string StageList
        {
            get { return _StageList; }
            set { _StageList = value; }
        }

        private string _SlotList = "";

        public string SlotList
        {
            get { return _SlotList; }
            set { _SlotList = value; }
        }

        private string _CarrierId = "";

        public string CarrierId
        {
            get { return _CarrierId; }
            set { _CarrierId = value; }
        }

        private string _DeviceName = "";

        public string DeviceName
        {
            get { return _DeviceName; }
            set { _DeviceName = value; }
        }

        private int _AccessMode = 0;

        public int AccessMode
        {
            get { return _AccessMode; }
            set { _AccessMode = value; }
        }

        private double _ProcessingTemp = -999;

        public double ProcessingTemp
        {
            get { return _ProcessingTemp; }
            set { _ProcessingTemp = value; }
        }
    }
}
