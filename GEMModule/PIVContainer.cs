namespace GEMModule
{
    using LogModule;
    using NotifyEventModule;
    using ProberErrorCode;
    using ProberInterfaces;    
    using ProberInterfaces.Event;
    using ProberInterfaces.Param;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class PIVContainer : IPIVContainer, INotifyPropertyChanged, IHasComParameterizable, IParamNode, IParam, IFactoryModule
    {
        #region <remarks> PropertyChanged </remarks>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region <remarks> IParam Property </remarks>
        public string FilePath { get; set; }

        public string FileName { get; set; }

        public bool IsParamChanged { get; set; }

        #endregion

        #region <remarks> IParamNode Property </remarks>
        public string Genealogy { get; set; }
        public object Owner { get; set; }
        public List<object> Nodes { get; set; }
        #endregion

        #region <remarks> IStagePIV Property  </remarks> 
       

        public object GetPIVDataLockObject()
        {
            return this.GEMModule().GemCommManager.GetProcessorLockObj();
        }

        private IList<Element<int>> _StageStates = new List<Element<int>>() { };
        public IList<Element<int>> StageStates
        {
            get { return _StageStates; }
            set
            {
                if (value != _StageStates)
                {
                    _StageStates = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IList<Element<int>> _PreHeatStates = new List<Element<int>>();
        public IList<Element<int>> PreHeatStates
        {
            get { return _PreHeatStates; }
            set
            {
                if (value != _PreHeatStates)
                {
                    _PreHeatStates = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IList<Element<string>> _LoaderLotIDs = new List<Element<string>>();
        public IList<Element<string>> LoaderLotIDs
        {
            get { return _LoaderLotIDs; }
            set
            {
                if (value != _LoaderLotIDs)
                {
                    _LoaderLotIDs = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IList<Element<string>> _StageProberIDs = new List<Element<string>>();
        public IList<Element<string>> StageProberIDs
        {
            get { return _StageProberIDs; }
            set
            {
                if (value != _StageProberIDs)
                {
                    _StageProberIDs = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IList<Element<string>> _StageProberCardIDs = new List<Element<string>>();
        public IList<Element<string>> StageProberCardIDs
        {
            get { return _StageProberCardIDs; }
            set
            {
                if (value != _StageProberCardIDs)
                {
                    _StageProberCardIDs = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IList<Element<string>> _StageRecipeNames = new List<Element<string>>();
        public IList<Element<string>> StageRecipeNames
        {
            get { return _StageRecipeNames; }
            set
            {
                if (value != _StageRecipeNames)
                {
                    _StageRecipeNames = value;
                    RaisePropertyChanged();
                }
            }
        }


        private IList<Element<string>> _StageSVTemp = new List<Element<string>>();
        public IList<Element<string>> StageSVTemp
        {
            get { return _StageSVTemp; }
            set
            {
                if (value != _StageSVTemp)
                {
                    _StageSVTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IList<Element<string>> _StagePVTemp = new List<Element<string>>();
        public IList<Element<string>> StagePVTemp
        {
            get { return _StagePVTemp; }
            set
            {
                if (value != _StagePVTemp)
                {
                    _StagePVTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _StageNumber = new Element<int>() { RaisePropertyChangedFalg = true };
        public Element<int> StageNumber
        {
            get { return _StageNumber; }
            set
            {
                if (value != _StageNumber)
                {
                    _StageNumber = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _StageState = new Element<int>() { RaisePropertyChangedFalg = true };
        public Element<int> StageState
        {
            get { return _StageState; }
            set
            {
                if (value != _StageState)
                {
                    _StageState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _PreHeatState = new Element<int>() { RaisePropertyChangedFalg = true };
        public Element<int> PreHeatState
        {
            get { return _PreHeatState; }
            set
            {
                if (value != _PreHeatState)
                {
                    _PreHeatState = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<string> _WaferID = new Element<string>() { Value = "", RaisePropertyChangedFalg = true };
        public Element<string> WaferID
        {
            get { return _WaferID; }
            set
            {
                if (value != _WaferID)
                {
                    _WaferID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _UnloadingWaferID = new Element<string>() { Value = "", RaisePropertyChangedFalg = true };
        public Element<string> UnloadingWaferID
        {
            get { return _UnloadingWaferID; }
            set
            {
                if (value != _UnloadingWaferID)
                {
                    _UnloadingWaferID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _UnloadingSlotNum = new Element<int>() { RaisePropertyChangedFalg = true };
        public Element<int> UnloadingSlotNum
        {
            get { return _UnloadingSlotNum; }
            set
            {
                if (value != _UnloadingSlotNum)
                {
                    _UnloadingSlotNum = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<string> _UnloadedFormChuckWaferID = new Element<string>() { Value = "", RaisePropertyChangedFalg = true };
        public Element<string> UnloadedFormChuckWaferID
        {
            get { return _UnloadedFormChuckWaferID; }
            set
            {
                if (value != _UnloadedFormChuckWaferID)
                {
                    _UnloadedFormChuckWaferID = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// 1 = no card state
        /// 2 = no probe card
        /// 3 = probe card loaded
        /// </summary>
        private Element<int> _ProbeCardState = new Element<int>() { Value = 0, RaisePropertyChangedFalg = true };
        public Element<int> ProbeCardState
        {
            get { return _ProbeCardState; }
            set
            {
                if (value != _ProbeCardState)
                {
                    _ProbeCardState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _ProbeCardID = new Element<string>() { Value = "", RaisePropertyChangedFalg = true };
        public Element<string> ProbeCardID
        {
            get { return _ProbeCardID; }
            set
            {
                if (value != _ProbeCardID)
                {
                    _ProbeCardID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _RecipeID = new Element<string>() { Value = "", RaisePropertyChangedFalg = true };
        public Element<string> RecipeID
        {
            get { return _RecipeID; }
            set
            {
                if (value != _RecipeID)
                {
                    _RecipeID = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<int> _SlotNumber = new Element<int>() { RaisePropertyChangedFalg = true };
        public Element<int> SlotNumber
        {
            get { return _SlotNumber; }
            set
            {
                if (value != _SlotNumber)
                {
                    _SlotNumber = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private Element<int> _LoadFoupNumber = new Element<int>() { RaisePropertyChangedFalg = true };
        //public Element<int> LoadFoupNumber
        //{
        //    get { return _LoadFoupNumber; }
        //    set
        //    {
        //        if (value != _LoadFoupNumber)
        //        {
        //            _LoadFoupNumber = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private Dictionary<int, string> _LoadFoupAndLotID = new Dictionary<int, string>();

        public Dictionary<int, string> LoadFoupAndLotID
        {
            get { return _LoadFoupAndLotID; }
            set { _LoadFoupAndLotID = value; }
        }

        public Dictionary<int, string> GetLoadFoupAndLotID()
        {
            return LoadFoupAndLotID;
        }

        private Element<double> _CurTemperature = new Element<double>();
        public Element<double> CurTemperature
        {
            get { return _CurTemperature; }
            set
            {
                if (value != _CurTemperature)
                {
                    _CurTemperature = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _SetTemperature = new Element<double>() { RaisePropertyChangedFalg = true };
        public Element<double> SetTemperature
        {
            get { return _SetTemperature; }
            set
            {
                if (value != _SetTemperature)
                {
                    _SetTemperature = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<long> _XCoordinate = new Element<long>() { RaisePropertyChangedFalg = true };
        public Element<long> XCoordinate
        {
            get { return _XCoordinate; }
            set
            {
                if (value != _XCoordinate)
                {
                    _XCoordinate = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<long> _YCoordinate = new Element<long>() { RaisePropertyChangedFalg = true };
        public Element<long> YCoordinate
        {
            get { return _YCoordinate; }
            set
            {
                if (value != _YCoordinate)
                {
                    _YCoordinate = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _Overdrive = new Element<double>() { RaisePropertyChangedFalg = true };
        public Element<double> Overdrive
        {
            get { return _Overdrive; }
            set
            {
                if (value != _Overdrive)
                {
                    _Overdrive = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _CleaningTouchDownCount = new Element<int>() { RaisePropertyChangedFalg = true };
        public Element<int> CleaningTouchDownCount
        {
            get { return _CleaningTouchDownCount; }
            set
            {
                if (value != _CleaningTouchDownCount)
                {
                    _CleaningTouchDownCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// 1 : succesed, 0 : failed.
        /// </summary>
        private Element<int> _DevDownResult = new Element<int>() { Value = 1 };
        public Element<int> DevDownResult
        {
            get { return _DevDownResult; }
            set
            {
                if (value != _DevDownResult)
                {
                    _DevDownResult = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _ProberType = new Element<string>() { Value = "", RaisePropertyChangedFalg = true };
        public Element<string> ProberType
        {
            get { return _ProberType; }
            set
            {
                if (value != _ProberType)
                {
                    _ProberType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _FullSite = new Element<string>() { Value = "", RaisePropertyChangedFalg = true };
        public Element<string> FullSite
        {
            get { return _FullSite; }
            set
            {
                if (value != _FullSite)
                {
                    _FullSite = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _TmpFullSite = "";
        public string TmpFullSite
        {
            get { return _TmpFullSite; }
            set
            {
                if (value != _TmpFullSite)
                {
                    _TmpFullSite = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _CardSiteLocation = new Element<string>() { Value = "", RaisePropertyChangedFalg = true };
        public Element<string> CardSiteLocation
        {
            get { return _CardSiteLocation; }
            set
            {
                if (value != _CardSiteLocation)
                {
                    _CardSiteLocation = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _SystemClock = new Element<string>() { Value = "", RaisePropertyChangedFalg = true };
        public Element<string> SystemClock
        {
            get { return _SystemClock; }
            set
            {
                if (value != _SystemClock)
                {
                    _SystemClock = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _WaferStartTime = new Element<string>() { Value = "", RaisePropertyChangedFalg = true };
        public Element<string> WaferStartTime
        {
            get { return _WaferStartTime; }
            set
            {
                if (value != _WaferStartTime)
                {
                    _WaferStartTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// WaferEndTime
        /// 0 : 
        /// 1 : 
        /// </summary>
        private Element<string> _WaferEndTime = new Element<string>() { Value = "", RaisePropertyChangedFalg = true };
        public Element<string> WaferEndTime
        {
            get { return _WaferEndTime; }
            set
            {
                if (value != _WaferEndTime)
                {
                    _WaferEndTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<long> _PCardContactCount = new Element<long>() { RaisePropertyChangedFalg = true };
        public Element<long> PCardContactCount
        {
            get { return _PCardContactCount; }
            set
            {
                if (value != _PCardContactCount)
                {
                    _PCardContactCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsInsertOCommand;
        public bool IsInsertOCommand
        {
            get { return _IsInsertOCommand; }
            set
            {
                if (value != _IsInsertOCommand)
                {
                    _IsInsertOCommand = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _NotchAngle = new Element<int>() { RaisePropertyChangedFalg = true };
        public Element<int> NotchAngle
        {
            get { return _NotchAngle; }
            set
            {
                if (value != _NotchAngle)
                {
                    _NotchAngle = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// 0: normal 
        /// 1: rejected or abnormally unload
        /// </summary>
        private Element<int> _WaferEndResult = new Element<int>() { RaisePropertyChangedFalg = true, Value = 1 };
        public Element<int> WaferEndResult
        {
            get { return _WaferEndResult; }
            set
            {
                if (value != _WaferEndResult)
                {
                    _WaferEndResult = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<long> _TotalDieCount = new Element<long>() { RaisePropertyChangedFalg = true };
        public Element<long> TotalDieCount
        {
            get { return _TotalDieCount; }
            set
            {
                if (value != _TotalDieCount)
                {
                    _TotalDieCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<long> _PassDieCount = new Element<long>() { RaisePropertyChangedFalg = true };
        public Element<long> PassDieCount
        {
            get { return _PassDieCount; }
            set
            {
                if (value != _PassDieCount)
                {
                    _PassDieCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<long> _FailDieCount = new Element<long>() { RaisePropertyChangedFalg = true };
        public Element<long> FailDieCount
        {
            get { return _FailDieCount; }
            set
            {
                if (value != _FailDieCount)
                {
                    _FailDieCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _YieldOfBadDie = new Element<double>() { RaisePropertyChangedFalg = true };
        public Element<double> YieldOfBadDie
        {
            get { return _YieldOfBadDie; }
            set
            {
                if (value != _YieldOfBadDie)
                {
                    _YieldOfBadDie = value;
                    RaisePropertyChanged();
                }
            }
        }



        private Element<long> _FirstDieX = new Element<long>() { RaisePropertyChangedFalg = true };
        public Element<long> FirstDieX
        {
            get { return _FirstDieX; }
            set
            {
                if (value != _FirstDieX)
                {
                    _FirstDieX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<long> _FirstDieY = new Element<long>() { RaisePropertyChangedFalg = true };
        public Element<long> FirstDieY
        {
            get { return _FirstDieY; }
            set
            {
                if (value != _FirstDieY)
                {
                    _FirstDieY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _SoakingTimeSec = new Element<double>() { RaisePropertyChangedFalg = true };
        public Element<double> SoakingTimeSec
        {
            get { return _SoakingTimeSec; }
            set
            {
                if (value != _SoakingTimeSec)
                {
                    _SoakingTimeSec = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CatCoordinates _MarkChangeValue = new CatCoordinates();
        public CatCoordinates MarkChangeValue
        {
            get { return _MarkChangeValue; }
            set
            {
                if (value != _MarkChangeValue)
                {
                    _MarkChangeValue = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Dictionary<int, bool> _LoadPortDevLoadResults
             = new Dictionary<int, bool>();
        public Dictionary<int, bool> LoadPortDevLoadResults
        {
            get { return _LoadPortDevLoadResults; }
            set { _LoadPortDevLoadResults = value; }
        }

        private Dictionary<int, bool> _LoadPortDevDownResults
            = new Dictionary<int, bool>();
        public Dictionary<int, bool> LoadPortDevDownResults
        {
            get { return _LoadPortDevDownResults; }
            set { _LoadPortDevDownResults = value; }
        }


        private Element<double> _PinAlignPlanarity = new Element<double>();
        public Element<double> PinAlignPlanarity
        {
            get { return _PinAlignPlanarity; }
            set
            {
                if (value != _PinAlignPlanarity)
                {
                    _PinAlignPlanarity = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<List<List<double>>> _PinAlignResults = new Element<List<List<double>>>();
        public Element<List<List<double>>> PinAlignResults
        {
            get { return _PinAlignResults; }
            //set
            //{
            //    if (value != _PinAlignResults)
            //    {
            //        _PinAlignResults = value;
            //        RaisePropertyChanged();
            //    }
            //}
        }

        private Element<double> _PinAlignAngle = new Element<double>();
        public Element<double> PinAlignAngle
        {
            get { return _PinAlignAngle; }
            set
            {
                if (value != _PinAlignAngle)
                {
                    _PinAlignAngle = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _PinAlignCardCenterX = new Element<double>();
        public Element<double> PinAlignCardCenterX
        {
            get { return _PinAlignCardCenterX; }
            set
            {
                if (value != _PinAlignCardCenterX)
                {
                    _PinAlignCardCenterX = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<double> _PinAlignCardCenterY = new Element<double>();
        public Element<double> PinAlignCardCenterY
        {
            get { return _PinAlignCardCenterY; }
            set
            {
                if (value != _PinAlignCardCenterY)
                {
                    _PinAlignCardCenterY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _PinAlignCardCenterZ = new Element<double>();
        public Element<double> PinAlignCardCenterZ
        {
            get { return _PinAlignCardCenterZ; }
            set
            {
                if (value != _PinAlignCardCenterZ)
                {
                    _PinAlignCardCenterZ = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion



        #region <remarks> ILoaderPIV Property  </remarks> 


        private Element<string> _LoaderID = new Element<string>() { RaisePropertyChangedFalg = true };
        public Element<string> LoaderID
        {
            get { return _LoaderID; }
            set
            {
                if (value != _LoaderID)
                {
                    _LoaderID = value;
                    RaisePropertyChanged();
                }
            }
        }


        private List<FoupLotInfo> _FoupInfos = new List<FoupLotInfo>();

        public List<FoupLotInfo> FoupInfos
        {
            get { return _FoupInfos; }
            set { _FoupInfos = value; }
        }



        private Element<int> _FoupNumber = new Element<int>() { RaisePropertyChangedFalg = true };
        public Element<int> FoupNumber
        {
            get { return _FoupNumber; }
            set
            {
                if (value != _FoupNumber)
                {
                    _FoupNumber = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<int> _FoupState = new Element<int>() { RaisePropertyChangedFalg = true };
        public Element<int> FoupState
        {
            get { return _FoupState; }
            set
            {
                if (value != _FoupState)
                {
                    _FoupState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _ListOfStages = new Element<string>() { Value = "", RaisePropertyChangedFalg = true };
        public Element<string> ListOfStages
        {
            get { return _ListOfStages; }
            set
            {
                if (value != _ListOfStages)
                {
                    _ListOfStages = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<CardBufferInfo> _CardBufferInfos = new List<CardBufferInfo>();

        public List<CardBufferInfo> CardBufferInfos
        {
            get { return _CardBufferInfos; }
            set { _CardBufferInfos = value; }
        }

        private Element<int> _CardBufferIndex = new Element<int>() { RaisePropertyChangedFalg = true };
        public Element<int> CardBufferIndex
        {
            get { return _CardBufferIndex; }
            set
            {
                if (value != _CardBufferIndex)
                {
                    _CardBufferIndex = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<int> _CardLPState = new Element<int>() { RaisePropertyChangedFalg = true };
        public Element<int> CardLPState
        {
            get { return _CardLPState; }
            set
            {
                if (value != _CardLPState)
                {
                    _CardLPState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _CardLPAccessMode = new Element<int>() { Value = -1, RaisePropertyChangedFalg = true };
        public Element<int> CardLPAccessMode
        {
            get { return _CardLPAccessMode; }
            set
            {
                if (value != _CardLPAccessMode)
                {
                    _CardLPAccessMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _ListOfSlot = new Element<string>() { Value = "", RaisePropertyChangedFalg = true };
        public Element<string> ListOfSlot
        {
            get { return _ListOfSlot; }
            set
            {
                if (value != _ListOfSlot)
                {
                    _ListOfSlot = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _WaferChange_Location1_LoadPortId = new Element<string>() { Value = "", RaisePropertyChangedFalg = true };
        public Element<string> WaferChange_Location1_LoadPortId
        {
            get { return _WaferChange_Location1_LoadPortId; }
            set
            {
                if (value != _WaferChange_Location1_LoadPortId)
                {
                    _WaferChange_Location1_LoadPortId = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _WaferChange_Location1_AtomId = new Element<string>() { Value = "", RaisePropertyChangedFalg = true };
        public Element<string> WaferChange_Location1_AtomId
        {
            get { return _WaferChange_Location1_AtomId; }
            set
            {
                if (value != _WaferChange_Location1_AtomId)
                {
                    _WaferChange_Location1_AtomId = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _WaferChange_Location2_LoadPortId = new Element<string>() { Value = "", RaisePropertyChangedFalg = true };
        public Element<string> WaferChange_Location2_LoadPortId
        {
            get { return _WaferChange_Location2_LoadPortId; }
            set
            {
                if (value != _WaferChange_Location2_LoadPortId)
                {
                    _WaferChange_Location2_LoadPortId = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _WaferChange_Location2_AtomId = new Element<string>() { Value = "", RaisePropertyChangedFalg = true };
        public Element<string> WaferChange_Location2_AtomId
        {
            get { return _WaferChange_Location2_AtomId; }
            set
            {
                if (value != _WaferChange_Location2_AtomId)
                {
                    _WaferChange_Location2_AtomId = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _WaferChange_Location1_WaferId = new Element<string>() { Value = "", RaisePropertyChangedFalg = true };
        public Element<string> WaferChange_Location1_WaferId
        {
            get { return _WaferChange_Location1_WaferId; }
            set
            {
                if (value != _WaferChange_Location1_WaferId)
                {
                    _WaferChange_Location1_WaferId = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<string> _WaferChange_Location2_WaferId = new Element<string>() { Value = "", RaisePropertyChangedFalg = true };
        public Element<string> WaferChange_Location2_WaferId
        {
            get { return _WaferChange_Location2_WaferId; }
            set
            {
                if (value != _WaferChange_Location2_WaferId)
                {
                    _WaferChange_Location2_WaferId = value;
                    RaisePropertyChanged();
                }
            }
        }



        private List<Element<string>> _LoaderSelectedSlotLists = new List<Element<string>>();
        public List<Element<string>> LoaderSelectedSlotLists
        {
            get { return _LoaderSelectedSlotLists; }
            set
            {
                if (value != _LoaderSelectedSlotLists)
                {
                    _LoaderSelectedSlotLists = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _FoupShiftMode = new Element<int>() { RaisePropertyChangedFalg = true, UpperLimit = 1, LowerLimit = 0 };
        public Element<int> FoupShiftMode
        {
            get { return _FoupShiftMode; }
            set
            {
                if (value != _FoupShiftMode)
                {
                    _FoupShiftMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        //SVID로 사용
        private List<Element<string>> _LoaderCarrierIDs = new List<Element<string>>();
        public List<Element<string>> LoaderCarrierIDs
        {
            get { return _LoaderCarrierIDs; }
            set
            {
                if (value != _LoaderCarrierIDs)
                {
                    _LoaderCarrierIDs = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<Element<int>> _FoupStates = new List<Element<int>>();
        public List<Element<int>> FoupStates
        {
            get { return _FoupStates; }
            set
            {
                if (value != _FoupStates)
                {
                    _FoupStates = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<Element<int>> _FoupAccessModes = new List<Element<int>>();
        public List<Element<int>> FoupAccessModes
        {
            get { return _FoupAccessModes; }
            set
            {
                if (value != _FoupAccessModes)
                {
                    _FoupAccessModes = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<Element<string>> _CardBufferCardIds = new List<Element<string>>();
        public List<Element<string>> CardBufferCardIds
        {
            get { return _CardBufferCardIds; }
            set
            {
                if (value != _CardBufferCardIds)
                {
                    _CardBufferCardIds = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<Element<int>> _CardBufferStates = new List<Element<int>>();
        public List<Element<int>> CardBufferStates
        {
            get { return _CardBufferStates; }
            set
            {
                if (value != _CardBufferStates)
                {
                    _CardBufferStates = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<Element<int>> _CardBufferAccessModes = new List<Element<int>>();
        public List<Element<int>> CardBufferAccessModes
        {
            get { return _CardBufferAccessModes; }
            set
            {
                if (value != _CardBufferAccessModes)
                {
                    _CardBufferAccessModes = value;
                    RaisePropertyChanged();
                }
            }
        }


        private List<Element<string>> _ExchangerProberCardIDs = new List<Element<string>>();
        public List<Element<string>> ExchangerProberCardIDs
        {
            get { return _ExchangerProberCardIDs; }
            set
            {
                if (value != _ExchangerProberCardIDs)
                {
                    _ExchangerProberCardIDs = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _PreAlignNumber = new Element<int>();
        public Element<int> PreAlignNumber
        {
            get { return _PreAlignNumber; }
            set
            {
                if (value != _PreAlignNumber)
                {
                    _PreAlignNumber = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _PreLoadingWaferId = new Element<string>() { RaisePropertyChangedFalg = true };
        public Element<string> PreLoadingWaferId
        {
            get { return _PreLoadingWaferId; }
            set
            {
                if (value != _PreLoadingWaferId)
                {
                    _PreLoadingWaferId = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<int> _PreLoadingSlotNum = new Element<int>() { RaisePropertyChangedFalg = true };
        public Element<int> PreLoadingSlotNum
        {
            get { return _PreLoadingSlotNum; }
            set
            {
                if (value != _PreLoadingSlotNum)
                {
                    _PreLoadingSlotNum = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _ArmNumber = new Element<int>();
        public Element<int> ArmNumber
        {
            get { return _ArmNumber; }
            set
            {
                if (value != _ArmNumber)
                {
                    _ArmNumber = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _CarrierID = new Element<string>() { Value = "", RaisePropertyChangedFalg = true };
        public Element<string> CarrierID
        {
            get { return _CarrierID; }
            set
            {
                if (value != _CarrierID)
                {
                    _CarrierID = value;
                    RaisePropertyChanged();
                }
            }
        }
        private List<Element<int>> _LightState = new List<Element<int>>();
        public List<Element<int>> LightState
        {
            get { return _LightState; }
            set
            {
                if (value != _LightState)
                {
                    _LightState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _AccessMode = new Element<int>() { Value = -1, RaisePropertyChangedFalg = true };
        public Element<int> AccessMode
        {
            get { return _AccessMode; }
            set
            {
                if (value != _AccessMode)
                {
                    _AccessMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _VerifyParamResultMap = new Element<string>();
        public Element<string> VerifyParamResultMap
        {
            get { return _VerifyParamResultMap; }
            set
            {
                if (value != _VerifyParamResultMap)
                {
                    _VerifyParamResultMap = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<Element<bool>> _DryAirStates = new List<Element<bool>>();
        public List<Element<bool>> DryAirStates
        {
            get { return _DryAirStates; }
            set
            {
                if (value != _DryAirStates)
                {
                    _DryAirStates = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _WaferAutofeedResult = new Element<int>();
        public Element<int> WaferAutofeedResult
        {
            get { return _WaferAutofeedResult; }
            set
            {
                if (value != _WaferAutofeedResult)
                {
                    _WaferAutofeedResult = value;
                    RaisePropertyChanged();
                }
            }
        }

        //SVID로 사용
        /// <summary>
        /// smoke sensors current temperature
        /// </summary>
        private IList<Element<double>> _SmokeSensorTemp = new List<Element<double>>();
        public IList<Element<double>> SmokeSensorTemp
        {
            get { return _SmokeSensorTemp; }
            set
            {
                if (value != _SmokeSensorTemp)
                {
                    _SmokeSensorTemp = value;
                    RaisePropertyChanged();
                }
            }
        }
        //SVID로 사용
        /// <summary>
        /// smoke sensors current status
        /// GEMStateDefineParam.json에 정의해서 사용할 것.
        /// 
        /// 
        /// 
        /// </summary>
        private IList<Element<int>> _SmokeSensorStatus = new List<Element<int>>();
        public IList<Element<int>> SmokeSensorStatus
        {
            get { return _SmokeSensorStatus; }
            set
            {
                if (value != _SmokeSensorStatus)
                {
                    _SmokeSensorStatus = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<int> _PolishWaferIDReadResult = new Element<int>();
        public Element<int> PolishWaferIDReadResult
        {
            get { return _PolishWaferIDReadResult; }
            set
            {
                if (value != _PolishWaferIDReadResult)
                {
                    _PolishWaferIDReadResult = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<Element<string>> _FixedTrayPolishWaferIds = new List<Element<string>>();
        public List<Element<string>> FixedTrayPolishWaferIds
        {
            get { return _FixedTrayPolishWaferIds; }
            set
            {
                if (value != _FixedTrayPolishWaferIds)
                {
                    _FixedTrayPolishWaferIds = value;
                    RaisePropertyChanged();
                }
            }
        }
        private List<Element<double>> _FixedTrayPolishWaferTouchCounts = new List<Element<double>>();
        public List<Element<double>> FixedTrayPolishWaferTouchCounts
        {
            get { return _FixedTrayPolishWaferTouchCounts; }
            set
            {
                if (value != _FixedTrayPolishWaferTouchCounts)
                {
                    _FixedTrayPolishWaferTouchCounts = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region <remarks> Comm Property <remarks>

        private Element<string> _LotID = new Element<string>() { Value = "", RaisePropertyChangedFalg = true };
        public Element<string> LotID
        {
            get { return _LotID; }
            set
            {
                if (value != _LotID)
                {
                    _LotID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _CassetteHashCode = "";
        public string CassetteHashCode
        {
            get { return _CassetteHashCode; }
            set
            {
                if (value != _CassetteHashCode)
                {
                    _CassetteHashCode = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region <remarks> IStagePIV Method  </remarks> 

        public void SetPinAlignResults(List<List<double>> rst)
        {
            try
            {
                IElement element = this.ParamManager().GetElement(_PinAlignResults.ElementID);
                if (element != null)
                {
                    if (element.RaisePropertyChangedFalg == false)
                    {
                        element.RaisePropertyChangedFalg = true;
                    }
                    //if (element.GEMImmediatelyUpdate == false)// 확인할것.
                    //{
                    //    element.GEMImmediatelyUpdate = true;
                    //}
                }

                _PinAlignResults.Value = rst;
                RaisePropertyChanged();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public void SetStageState(GEMStageStateEnum stateenum)
        {
            try
            {
                LoggerManager.Debug($"Stage #{StageNumber.Value} SetStageState: [{stateenum}]");
                if (StageNumber.Value > 0)
                {
                    int stageState = this.GEMModule().GetStageStateEnumValue(stateenum);
                    if (stageState != -1)
                    {
                        StageState.Value = stageState;
                        LoggerManager.Debug($"Stage #{StageNumber.Value} set stage state to [{stateenum} : {StageState.Value}]");
                        if (!StageStates[StageNumber.Value - 1].RaisePropertyChangedFalg)
                            StageStates[StageNumber.Value - 1].RaisePropertyChangedFalg = true;
                        if (!StageStates[StageNumber.Value - 1].GEMImmediatelyUpdate)
                            StageStates[StageNumber.Value - 1].GEMImmediatelyUpdate = true;

                        IElement element = this.ParamManager().GetElement(StageStates[StageNumber.Value - 1].ElementID);
                        LoggerManager.Debug($"Stage #{StageNumber.Value} set stage state to [ElementID : {StageStates[StageNumber.Value - 1].ElementID}]");
                        if (element != null)
                        {
                            if (element.RaisePropertyChangedFalg == false)
                            {
                                element.RaisePropertyChangedFalg = true;
                            }
                            if (element.GEMImmediatelyUpdate == false)
                            {
                                element.GEMImmediatelyUpdate = true;
                            }
                        }

                        StageStates[StageNumber.Value - 1].Value = stageState;
                        UpdateStageLotInfo(FoupNumber.Value);
                    }
                }
                LoggerManager.Debug($"Stage #{StageNumber.Value} set stage state to [{stateenum} : {StageState.Value}]");

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetPreHeatState(GEMPreHeatStateEnum stateenum)
        {
            try
            {
                if (StageNumber.Value > 0)
                {
                    int preheatState = this.GEMModule().GetPreHeatStateEnumValue(stateenum);

                    if (preheatState != -1)
                    {
                        PreHeatState.Value = preheatState;

                        if (!PreHeatStates[StageNumber.Value - 1].RaisePropertyChangedFalg)
                        {
                            PreHeatStates[StageNumber.Value - 1].RaisePropertyChangedFalg = true;
                        }

                        if (!PreHeatStates[StageNumber.Value - 1].GEMImmediatelyUpdate)
                        {
                            PreHeatStates[StageNumber.Value - 1].GEMImmediatelyUpdate = true;
                        }

                        PreHeatStates[StageNumber.Value - 1].Value = (int)stateenum;


                    }
                }
                LoggerManager.Debug($"Stage #{StageNumber.Value} set preheat state to [{stateenum} : {PreHeatState.Value}]");

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetWaferID(string waferid)
        {
            try
            {

                WaferID.Value = waferid;
                LoggerManager.Debug($"[PIVContainer] SetWaferID() WaferID : {WaferID.Value}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            //if(LoadFoupNumber.Value !=0)
            //{
            //    SetStageLotInfo(LoadFoupNumber.Value, waferid: WaferID.Value);
            //}
        }

        public void SetOverDrive(double overdrive)
        {
            try
            {
                Overdrive.Value = overdrive;
                LoggerManager.Debug($"[PIVContainer] SetOverDrive() WaferID : {Overdrive.Value}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetUnloadedFormChuckWaferID(string waferid)
        {
            try
            {
                UnloadedFormChuckWaferID.Value = waferid;
                LoggerManager.Debug($"[PIVContainer] SetUnloadedFormChuckWaferID() WaferID : {UnloadedFormChuckWaferID.Value}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetProberCardID(string probercardid)
        {
            if (probercardid == null)
                probercardid = "";
            ProbeCardID.Value = "";
            ProbeCardID.Value = probercardid;

            if (StageNumber.Value > 0)
            {
                if (!StageProberCardIDs[StageNumber.Value - 1].RaisePropertyChangedFalg)
                    StageProberCardIDs[StageNumber.Value - 1].RaisePropertyChangedFalg = true;
                if (!StageProberCardIDs[StageNumber.Value - 1].GEMImmediatelyUpdate)
                    StageProberCardIDs[StageNumber.Value - 1].GEMImmediatelyUpdate = true;

                IElement element = this.ParamManager().GetElement(StageProberCardIDs[StageNumber.Value - 1].ElementID);
                if (element != null)
                {
                    if (element.RaisePropertyChangedFalg == false)
                    {
                        element.RaisePropertyChangedFalg = true;
                    }
                    if (element.GEMImmediatelyUpdate == false)
                    {
                        element.GEMImmediatelyUpdate = true;
                    }
                }

                StageProberCardIDs[StageNumber.Value - 1].Value = probercardid;

                if (ProbeCardID.Value != "")
                    ProbeCardState.Value = 3;
                else
                    ProbeCardState.Value = 2;
            }



            LoggerManager.Debug($"Stage #{StageNumber.Value} set probecard state to [{ProbeCardState.Value}]");
        }


        public Element<string> GetStageSVTemp(int stgindex)
        {
            Element<string> retVal = new Element<string>();
            try
            {
                lock (GetPIVDataLockObject())
                {
                    if (StageSVTemp.Count() >= stgindex)
                    {
                        retVal = StageSVTemp[stgindex - 1];
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        public void SetSVTemp(double temp)
        {
            try
            {
                if (StageNumber.Value > 0)
                {
                    if (!StageSVTemp[StageNumber.Value - 1].RaisePropertyChangedFalg)
                        StageSVTemp[StageNumber.Value - 1].RaisePropertyChangedFalg = true;
                    if (!StageSVTemp[StageNumber.Value - 1].GEMImmediatelyUpdate)
                        StageSVTemp[StageNumber.Value - 1].GEMImmediatelyUpdate = true;
                    IElement element = this.ParamManager().GetElement(StageSVTemp[StageNumber.Value - 1].ElementID);
                    if (element != null)
                    {
                        if (element.RaisePropertyChangedFalg == false)
                        {
                            element.RaisePropertyChangedFalg = true;
                        }
                        if (element.GEMImmediatelyUpdate == false)
                        {
                            element.GEMImmediatelyUpdate = true;
                        }
                    }
                    StageSVTemp[StageNumber.Value - 1].Value = temp.ToString();
                    SetTemperature.Value = temp;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            //LoggerManager.Debug($"Stage #{StageNumber.Value} set Cur Temp to [{temp}]");
        }

        public void SetPVTemp(double temp)
        {
            try
            {
                if (StageNumber.Value > 0)
                {
                    if (!StagePVTemp[StageNumber.Value - 1].RaisePropertyChangedFalg)
                        StagePVTemp[StageNumber.Value - 1].RaisePropertyChangedFalg = true;
                    if (!StagePVTemp[StageNumber.Value - 1].GEMImmediatelyUpdate)
                        StagePVTemp[StageNumber.Value - 1].GEMImmediatelyUpdate = true;
                    IElement element = this.ParamManager().GetElement(StagePVTemp[StageNumber.Value - 1].ElementID);
                    if (element != null)
                    {
                        if (element.RaisePropertyChangedFalg == false)
                        {
                            element.RaisePropertyChangedFalg = true;
                        }
                        if (element.GEMImmediatelyUpdate == false)
                        {
                            element.GEMImmediatelyUpdate = true;
                        }
                    }
                    StagePVTemp[StageNumber.Value - 1].Value = temp.ToString();
                    CurTemperature.Value = temp;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetListOfSlot(int foupnumber, string slotlist)
        {
            try
            {
                if (!LoaderSelectedSlotLists[foupnumber - 1].RaisePropertyChangedFalg)
                    LoaderSelectedSlotLists[foupnumber - 1].RaisePropertyChangedFalg = true;
                if (!LoaderSelectedSlotLists[foupnumber - 1].GEMImmediatelyUpdate)
                    LoaderSelectedSlotLists[foupnumber - 1].GEMImmediatelyUpdate = true;
                IElement element = this.ParamManager().GetElement(LoaderSelectedSlotLists[foupnumber - 1].ElementID);
                if (element != null)
                {
                    if (element.RaisePropertyChangedFalg == false)
                    {
                        element.RaisePropertyChangedFalg = true;
                    }
                    if (element.GEMImmediatelyUpdate == false)
                    {
                        element.GEMImmediatelyUpdate = true;
                    }
                }
                SetFoupInfo(foupnumber: foupnumber, slotlist: slotlist);
                LoaderSelectedSlotLists[foupnumber - 1].Value = slotlist;
                //StageSetTemperatures[StageNumber.Value - 1].Value =  바뀌어야함. 
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            //LoggerManager.Debug($"Stage #{StageNumber.Value} set Cur Temp to [{temp}]");
        }

        public void SetDeviceName(string devicename)
        {
            try
            {
                if (!StageRecipeNames[StageNumber.Value - 1].RaisePropertyChangedFalg)
                    StageRecipeNames[StageNumber.Value - 1].RaisePropertyChangedFalg = true;
                if (!StageRecipeNames[StageNumber.Value - 1].GEMImmediatelyUpdate)
                    StageRecipeNames[StageNumber.Value - 1].GEMImmediatelyUpdate = true;
                IElement element = this.ParamManager().GetElement(StageRecipeNames[StageNumber.Value - 1].ElementID);
                if (element != null)
                {
                    if (element.RaisePropertyChangedFalg == false)
                    {
                        element.RaisePropertyChangedFalg = true;
                    }
                    if (element.GEMImmediatelyUpdate == false)
                    {
                        element.GEMImmediatelyUpdate = true;
                    }
                }
                StageRecipeNames[StageNumber.Value - 1].Value = devicename;
                RecipeID.Value = devicename;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            //LoggerManager.Debug($"Stage #{StageNumber.Value} set Cur Temp to [{temp}]");
        }

        public void SetDevDownResult(bool result)
        {
            try
            {
                if (result)
                {
                    DevDownResult.Value = 1;
                }
                else
                {
                    DevDownResult.Value = 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// 1: reject
        /// 0: normal
        /// </summary>
        /// <param name="waferendrst"></param>
        public void SetStageWaferResult(int waferendrst = 1)
        {
            try
            {
                WaferEndResult.Value = waferendrst;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public void ResetWaferID(string waferid)
        {
            try
            {
                WaferID.Value = waferid;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// Foupnumber, LOT ID, CST HashCode 를 이용해서 LOT DATA 업데이트
        /// </summary>
        public void UpdateStageLotInfo(int foupnumber = 0, string lotid ="")
        {
            lock (GetPIVDataLockObject())
            {
                var stageLotInfo = this.LotOPModule().LotInfo.GetLotInfos();
                string cassetteHashCode = this.LotOPModule().LotInfo.CSTHashCode;
                StageLotInfo info;
                if (string.IsNullOrEmpty(lotid))
                    lotid = this.LotOPModule().LotInfo.LotName.Value;

                DynamicModeEnum dynamicMode = this.LotOPModule().LotInfo.DynamicMode.Value;

                if (dynamicMode == DynamicModeEnum.DYNAMIC)
                {
                    // ficed 에서 시작한 pw 같은 경우엔 CassetteHashCode 가 null
                    if (cassetteHashCode != null)
                    {
                        if (cassetteHashCode.Equals(""))
                        {
                            info = stageLotInfo.Find(stageinfo => stageinfo.FoupIndex == foupnumber
                                                                && stageinfo.LotID.Equals(lotid));
                        }
                        else
                        {
                            info = stageLotInfo.Find(stageinfo => stageinfo.FoupIndex == foupnumber
                                                                && stageinfo.LotID.Equals(lotid)
                                                                && stageinfo.CassetteHashCode.Equals(cassetteHashCode));
                        }


                        if (info != null)
                        {
                            FoupNumber.Value = foupnumber;
                            RecipeID.Value = info.RecipeID;
                            LotID.Value = info.LotID;
                            CarrierID.Value = info.CarrierId;
                            LoggerManager.Debug($"Update StageLotInfo foupnumber : {foupnumber}, lot id : {LotID.Value}, recipe id : {info.RecipeID}");
                        }
                        else
                        {
                            FoupNumber.Value = foupnumber;
                            RecipeID.Value = this.FileManager().GetDeviceName();
                            LotID.Value = this.LotOPModule().LotInfo.LotName.Value;
                            LoggerManager.Debug($"Update StageLotInfo is null, DYMode : {dynamicMode},  foupnumber: {foupnumber}, lot id : {lotid}, recipe id : {this.FileManager().GetDeviceName()}, cst hashcode : {cassetteHashCode}");
                        }
                    }
                    else
                    {
                        FoupNumber.Value = foupnumber;
                        RecipeID.Value = this.FileManager().GetDeviceName();
                        LotID.Value = this.LotOPModule().LotInfo.LotName.Value;
                        LoggerManager.Debug($"Update CassetteHashCode is null DYMode : {dynamicMode}, foupnumber: {foupnumber}, lot id : {lotid}, recipe id : {this.FileManager().GetDeviceName()}, cst hashcode : {cassetteHashCode}");
                    }
                }
                else
                {
                    info = info = stageLotInfo.Find(stageinfo => stageinfo.FoupIndex == foupnumber);
                    if(info != null)
                    {
                        FoupNumber.Value = foupnumber;
                        RecipeID.Value = info.RecipeID;
                        LotID.Value = info.LotID;
                        CarrierID.Value = info.CarrierId;
                        LoggerManager.Debug($"Update StageLotInfo DYMode : {dynamicMode}, foupnumber : {foupnumber}, lot id : {LotID.Value}, recipe id : {info.RecipeID}, carrier id: {info.CarrierId}");
                    }
                    else
                    {
                        FoupNumber.Value = foupnumber;
                        RecipeID.Value = this.FileManager().GetDeviceName();
                        LotID.Value = this.LotOPModule().LotInfo.LotName.Value;
                        LoggerManager.Debug($"Update StageLotInfo is null DYMode : {dynamicMode}, foupnumber: {foupnumber}, lot id : {lotid}, recipe id : {this.FileManager().GetDeviceName()}, cst hashcode : {cassetteHashCode}");
                    }
                }
            }
        }


        public void SetMarkChangeValue(double xoffset, double yoffset, double zoffset)
        {
            try
            {
                if (MarkChangeValue != null)
                {
                    MarkChangeValue.X.Value = xoffset;
                    MarkChangeValue.Y.Value = yoffset;
                    MarkChangeValue.Z.Value = zoffset;

                    LoggerManager.Debug($"[PIVContainer] SetMarkChangeValue(). X : {xoffset}, Y : {yoffset}, Z : {zoffset}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        #endregion

        #region <remarks> ILoaderPIV Method  </remarks> 
        public void SetFoupState(int foupindex, GEMFoupStateEnum stateenum)
        {
            try
            {
                lock (GetPIVDataLockObject())
                {
                    int foupState = this.GEMModule().GetFoupStateEnumValue(stateenum);

                    if (foupState != -1)
                    {
                        FoupNumber.Value = foupindex;
                        FoupState.Value = foupState;

                        if (FoupStates.Count > 0)
                        {
                            if (!FoupStates[foupindex - 1].RaisePropertyChangedFalg)
                                FoupStates[foupindex - 1].RaisePropertyChangedFalg = true;
                            if (!FoupStates[foupindex - 1].GEMImmediatelyUpdate)
                                FoupStates[foupindex - 1].GEMImmediatelyUpdate = true;

                            IElement element = this.ParamManager().GetElement(FoupStates[foupindex - 1].ElementID);
                            if (element != null)
                            {
                                if (element.RaisePropertyChangedFalg == false)
                                {
                                    element.RaisePropertyChangedFalg = true;
                                }
                                if (element.GEMImmediatelyUpdate == false)
                                {
                                    element.GEMImmediatelyUpdate = true;
                                }
                            }

                            FoupStates[foupindex - 1].Value = (int)foupState;
                            SetFoupInfo(foupindex, foupstate: (int)foupState);
                        }
                        else
                        {
                            LoggerManager.Debug($"[PIVContainer] SetFoupState() : FoupStates count is 0.");
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public GEMFoupStateEnum GetFoupState(int foupnumber)
        {
            GEMFoupStateEnum state = GEMFoupStateEnum.UNDIFIND;

            try
            {
                lock (GetPIVDataLockObject())
                {
                    if (FoupInfos.Count > 0 && (FoupInfos.Count >= foupnumber))
                    {
                        int number = FoupStates[foupnumber - 1].Value;
                        state = this.GEMModule().GetfoupStatEnumType(number);
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return state;
        }

        public void SetCardBufferState(int index, GEMFoupStateEnum stateenum)
        {
            try
            {
                int state = 0;
                lock (GetPIVDataLockObject())
                {
                    state =  this.GEMModule().GetCardLPStateEnumValue(stateenum);
                    if (state != -1)
                    {
                        CardBufferIndex.Value = index;
                        CardLPState.Value = state;

                        if (!CardBufferStates[index - 1].RaisePropertyChangedFalg)
                            CardBufferStates[index - 1].RaisePropertyChangedFalg = true;
                        if (!CardBufferStates[index - 1].GEMImmediatelyUpdate)
                            CardBufferStates[index - 1].GEMImmediatelyUpdate = true;

                        IElement element = this.ParamManager().GetElement(CardBufferStates[index - 1].ElementID);
                        if (element != null)
                        {
                            if (element.RaisePropertyChangedFalg == false)
                            {
                                element.RaisePropertyChangedFalg = true;
                            }
                            if (element.GEMImmediatelyUpdate == false)
                            {
                                element.GEMImmediatelyUpdate = true;
                            }
                        }

                        CardBufferStates[index - 1].Value = (int)state;                        
                    }
                }
                SetCardBufferInfo(index, lpstate: (int)state);
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public GEMFoupStateEnum GetCardBufferState(int index)
        {
            GEMFoupStateEnum state = GEMFoupStateEnum.UNDIFIND;

            lock (GetPIVDataLockObject())
            {
                if (CardBufferInfos.Count > 0 && (CardBufferInfos.Count >= index | CardBufferInfos.Count <= index))
                {
                    int number = CardBufferStates[index - 1].Value;
                    state = this.GEMModule().GetfoupStatEnumType(number);
                }

            }
            return state;
        }


        public Element<int> GetFoupAccessMode(int foupindex)
        {
            Element<int> retVal = new Element<int>();
            try
            {
                lock (GetPIVDataLockObject())
                {
                    if (FoupAccessModes.Count() >= foupindex)
                    {
                        retVal = FoupAccessModes[foupindex - 1];
                    }
                }                               
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void SetFoupAccessMode(int foupindex, bool isAutoMode)
        {
            try
            {
                lock (GetPIVDataLockObject())
                {
                    // 1 : AUTO
                    // 0 : MANUAL
                    int mode = isAutoMode ? 1 : 0;
                    if (mode != -1)
                    {
                        if (!FoupAccessModes[foupindex - 1].RaisePropertyChangedFalg)
                            FoupAccessModes[foupindex - 1].RaisePropertyChangedFalg = true;
                        if (!FoupAccessModes[foupindex - 1].GEMImmediatelyUpdate)
                            FoupAccessModes[foupindex - 1].GEMImmediatelyUpdate = true;

                        IElement element = this.ParamManager().GetElement(FoupAccessModes[foupindex - 1].ElementID);
                        if (element != null)
                        {
                            if (element.RaisePropertyChangedFalg == false)
                            {
                                element.RaisePropertyChangedFalg = true;
                            }
                            if (element.GEMImmediatelyUpdate == false)
                            {
                                element.GEMImmediatelyUpdate = true;
                            }
                        }

                        FoupAccessModes[foupindex - 1].Value = (int)mode;
                        SetFoupInfo(foupindex, accessmode: mode);
                        UpdateFoupInfo(foupindex);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public Element<int> GetCardBufferAccessMode(int index)
        {
            Element<int> retVal = new Element<int>();
            try
            {
                lock (GetPIVDataLockObject())
                {
                    if (CardBufferAccessModes.Count() >=  index)
                    {
                        retVal = CardBufferAccessModes[index - 1];
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void SetCardBufferAccessMode(int index, bool isAutoMode)
        {
            try
            {
                // 1 : AUTO
                // 0 : MANUAL
                int mode = isAutoMode ? 1 : 0;

                lock (GetPIVDataLockObject())
                {

                    if (mode != -1)
                    {
                        if (!CardBufferAccessModes[index - 1].RaisePropertyChangedFalg)
                            CardBufferAccessModes[index - 1].RaisePropertyChangedFalg = true;
                        if (!CardBufferAccessModes[index - 1].GEMImmediatelyUpdate)
                            CardBufferAccessModes[index - 1].GEMImmediatelyUpdate = true;

                        IElement element = this.ParamManager().GetElement(CardBufferAccessModes[index - 1].ElementID);
                        if (element != null)
                        {
                            if (element.RaisePropertyChangedFalg == false)
                            {
                                element.RaisePropertyChangedFalg = true;
                            }
                            if (element.GEMImmediatelyUpdate == false)
                            {
                                element.GEMImmediatelyUpdate = true;
                            }
                        }

                        CardBufferAccessModes[index - 1].Value = (int)mode;
                        

                    }
                }
                SetCardBufferInfo(index, accessmode: mode);
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetLoaderLotIds(int foupnumber, string lotid = "")
        {
            try
            {
                lock (GetPIVDataLockObject())
                {
                    if (LoaderLotIDs?.Count() > 0 && foupnumber > 0)
                    {
                        if (!LoaderLotIDs[foupnumber - 1].RaisePropertyChangedFalg)
                            LoaderLotIDs[foupnumber - 1].RaisePropertyChangedFalg = true;
                        if (!LoaderLotIDs[foupnumber - 1].GEMImmediatelyUpdate)
                            LoaderLotIDs[foupnumber - 1].GEMImmediatelyUpdate = true;

                        IElement element = this.ParamManager().GetElement(LoaderLotIDs[foupnumber - 1].ElementID);
                        if (element != null)
                        {
                            if (element.RaisePropertyChangedFalg == false)
                            {
                                element.RaisePropertyChangedFalg = true;
                            }
                            if (element.GEMImmediatelyUpdate == false)
                            {
                                element.GEMImmediatelyUpdate = true;
                            }
                        }


                        LoaderLotIDs[foupnumber - 1].Value = lotid;
                        SetFoupInfo(foupnumber, lotid: lotid);
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetCarrierId(int foupindex, string id)
        {
            try
            {
                lock (GetPIVDataLockObject())
                {
                    if (FoupInfos.Count > 0 && foupindex > 0 && (FoupInfos.Count >= foupindex))
                    {
                        LoaderCarrierIDs[foupindex - 1].Value = id;
                        var foup = FoupInfos[foupindex - 1];
                        foup.CarrierId = id;
                        //foup.StageList = "";
                        //foup.SlotList = "";
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetCardBufferCardId(int index, string id)
        {
            try
            {
                lock (GetPIVDataLockObject())
                {
                    if (CardBufferInfos.Count > 0 && (CardBufferInfos.Count >= index | CardBufferInfos.Count <= index))
                    {
                        CardBufferCardIds[index - 1].Value = id;                       
                    }                    
                }
                SetCardBufferInfo(index, pcardid: id);
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetFoupInfo(int foupnumber, string lotid = null, int foupstate = -1, string stagelist = null, string slotlist = null, string devicename = null, string carrierid = null, int accessmode = -1, double processingtemp = -999)
        {
            try
            {
                lock (GetPIVDataLockObject())
                {
                    string setinfo = "";
                    if (FoupInfos.Count > 0 && foupnumber > 0 && FoupInfos.Count >= foupnumber)
                    {
                        var foup = FoupInfos[foupnumber - 1];
                        foup.FoupNumber = foupnumber;
                        if (lotid != null)
                        {
                            foup.LotID = lotid;
                            setinfo += $" LotID: {lotid}";
                        }

                        if (carrierid != null)
                        {
                            foup.CarrierId = carrierid;
                            setinfo += $" CarrierId: {carrierid}";
                        }

                        if (foupstate != -1)
                        {
                            foup.FoupState = foupstate;
                            setinfo += $" FoupState: {foupstate}";
                        }

                        if (stagelist != null)
                        {
                            foup.StageList = stagelist;
                            setinfo += $" StageList: {stagelist}";
                        }

                        if (slotlist != null)
                        {
                            foup.SlotList = slotlist;
                            setinfo += $" SlotList: {slotlist}";
                        }

                        if (devicename != null)
                        {
                            foup.DeviceName = devicename;
                            setinfo += $" DeviceName: {devicename}";
                        }

                        if (accessmode != -1)
                        {
                            foup.AccessMode = accessmode;
                            setinfo += $" AccessMode: {accessmode}";
                        }

                        if (processingtemp != -999)
                        {
                            foup.ProcessingTemp = processingtemp;
                            setinfo += $" ProcessingTemp: {processingtemp}";
                        }

                        LoggerManager.Debug($"SetFoupInfo():{setinfo}");

                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void UpdateFoupInfo(int foupnumber)
        {
            try
            {
                lock (GetPIVDataLockObject())
                {
                    if (FoupInfos.Count > 0 && foupnumber > 0 && FoupInfos.Count >= foupnumber)
                    {
                        var foup = FoupInfos[foupnumber - 1];
                        if (foup.FoupNumber == 0)
                        {
                            FoupNumber.Value = foupnumber;
                        }
                        else
                        {
                            FoupNumber.Value = foup.FoupNumber;
                        }

                        LotID.Value = foup.LotID;
                        FoupState.Value = foup.FoupState;
                        ListOfStages.Value = foup.StageList;
                        ListOfSlot.Value = foup.SlotList;
                        CarrierID.Value = foup.CarrierId;
                        RecipeID.Value = foup.DeviceName;
                        AccessMode.Value = foup.AccessMode;
                        SetTemperature.Value = foup.ProcessingTemp;
                        LoggerManager.Debug($"Update UpdateFoupInfo  foupnumber : {foupnumber}, lot id : {LotID.Value}, carrier id : {foup.CarrierId}");
                    }
                    else
                    {
                        FoupNumber.Value = foupnumber;
                        LoggerManager.Debug($"Update UpdateFoupInfo  foupnumber : {foupnumber}, lot id : {LotID.Value}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetCardBufferInfo(int index, int lpstate = -1, string pcardid = null, int accessmode = -1)
        {
            try
            {
                lock (GetPIVDataLockObject())
                {
                    if (CardBufferInfos.Count > 0 && (CardBufferInfos.Count >= index | CardBufferInfos.Count <= index))
                    {
                        var cardbuffer = CardBufferInfos[index - 1];
                        cardbuffer.Index = index;

                        if (lpstate != -1)
                        {
                            cardbuffer.LoadPortState = lpstate;
                        }
                        if (pcardid != null)
                        {
                            cardbuffer.PCardId = pcardid;
                        }
                        if (accessmode != -1)
                        {
                            cardbuffer.AccessMode = accessmode;
                        }
                        LoggerManager.Debug($"SetCardBufferInfo(): index:{index}, lpstate:{lpstate}, pcardid:{pcardid}, accessmode:{accessmode}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetFixedTrayInfo(int index, string polishwaferid, double touchcount)
        {
            try
            {
                lock (GetPIVDataLockObject())
                {
                    FixedTrayPolishWaferIds[index - 1].Value = polishwaferid;
                    FixedTrayPolishWaferTouchCounts[index - 1].Value = touchcount;
                    LoggerManager.Debug($"SetFixedTrayInfo(): index:{index}, polishwaferid:{polishwaferid}, touchcount:{touchcount}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void UpdateCardBufferInfo(int index)
        {
            try
            {
                lock (GetPIVDataLockObject())
                {
                    if (CardBufferInfos.Count > 0 && (CardBufferInfos.Count >= index | CardBufferInfos.Count <= index))
                    {
                        var cardbuffer = CardBufferInfos[index - 1];
                        if (cardbuffer.Index == 0)
                        {
                            CardBufferIndex.Value = index;
                        }
                        else
                        {
                            CardBufferIndex.Value = cardbuffer.Index;
                        }

                        CardLPState.Value = cardbuffer.LoadPortState;
                        ProbeCardID.Value = cardbuffer.PCardId;
                        CardLPAccessMode.Value = cardbuffer.AccessMode;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public string GetLotIDAtFoupInfos(int foupindex, string carrierId)
        {
            string lotid = "";
            try
            {
                if (FoupInfos.Count > 0 && (FoupInfos.Count >= foupindex | FoupInfos.Count <= foupindex))
                {
                    var foup = FoupInfos[foupindex - 1];
                    if (foup.CarrierId.Equals(carrierId))
                    {
                        lotid = foup.LotID;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return lotid;
        }

        public void SetExchangerProberCardID(int exindex, string probercardid)
        {
            ExchangerProberCardIDs[exindex - 1].Value = probercardid;
        }

        public void SetLightState(LightStateEnum light1, LightStateEnum light2, LightStateEnum light3)
        {
            try
            {
                if (LightState.Count == 3)
                {
                    bool isChanged = (LightState[0].Value != (int)light1) ||
                        (LightState[1].Value != (int)light2) || (LightState[2].Value != (int)light3);
                    if (isChanged)
                    {
                        LightState[0].Value = (int)light1;
                        LightState[1].Value = (int)light2;
                        LightState[2].Value = (int)light3;
                        //this.GEMModule().SetEvent(this.GEMModule().GetEventNumberFormEventName(
                        //    typeof(TowerLightEvent).FullName));

                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(TowerLightEvent).FullName, new ProbeEventArgs(this, semaphore));
                        semaphore.Wait();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        /// <summary>
        /// Gem Control State 변경 시, Data 를 Update 해주기 위한 함수. 
        /// </summary>
        public void ChangededControlStateEvent()
        {
            try
            {
                lock (GetPIVDataLockObject())
                {
                    if (FoupStates != null)
                    {
                        for (int index = 0; index < FoupStates.Count; index++)
                        {
                            this.GEMModule().SetValue(FoupStates[index]);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetDryAirValveState(int valveindex, bool state)
        {
            try
            {
                int stageIndex = -1;
                var valveParam = this.EnvControlManager().GetValveParam();
                if (valveParam != null && valveParam.DryAirMappingParam != null)
                {
                    stageIndex = valveParam.DryAirMappingParam.Find(param => param.ValveIndex == valveindex)?.StageIndex ?? -1;
                }

                if (stageIndex != -1)
                {
                    if (!DryAirStates[stageIndex - 1].RaisePropertyChangedFalg)
                        DryAirStates[stageIndex - 1].RaisePropertyChangedFalg = true;
                    if (!DryAirStates[stageIndex - 1].GEMImmediatelyUpdate)
                        DryAirStates[stageIndex - 1].GEMImmediatelyUpdate = true;

                    IElement element = this.ParamManager().GetElement(DryAirStates[stageIndex - 1].ElementID);
                    if (element != null)
                    {
                        if (element.RaisePropertyChangedFalg == false)
                        {
                            element.RaisePropertyChangedFalg = true;
                        }
                        if (element.GEMImmediatelyUpdate == false)
                        {
                            element.GEMImmediatelyUpdate = true;
                        }
                    }

                    DryAirStates[stageIndex - 1].Value = state;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetSmokeSensorTempState(int sensorindex, double temp)
        {
            try
            {
                lock (lockobject)
                {                    
                    if (sensorindex >= 0)
                    {
                        if (!SmokeSensorTemp[sensorindex].RaisePropertyChangedFalg)
                            SmokeSensorTemp[sensorindex].RaisePropertyChangedFalg = true;
                        if (!SmokeSensorTemp[sensorindex].GEMImmediatelyUpdate)
                            SmokeSensorTemp[sensorindex].GEMImmediatelyUpdate = true;                        

                        IElement element = this.ParamManager().GetElement(SmokeSensorTemp[sensorindex].PropertyPath);
                        if (element != null)
                        {
                            if (element.RaisePropertyChangedFalg == false)
                            {
                                element.RaisePropertyChangedFalg = true;
                            }
                            if (element.GEMImmediatelyUpdate == false)
                            {
                                element.GEMImmediatelyUpdate = true;
                            }
                        }

                        SmokeSensorTemp[sensorindex].Value = temp;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        object lockobject = new object();
        public void SetSmokeSensorStatusState(int sensorindex, GEMSensorStatusEnum status)
        {
            try
            {
                lock (lockobject)
                {
                    int sensorState = this.GEMModule().GetSmokeSensorStateEnumValue(status);

                    if (sensorState != -1)                    
                    {
                        if (!SmokeSensorStatus[sensorindex].RaisePropertyChangedFalg)
                            SmokeSensorStatus[sensorindex].RaisePropertyChangedFalg = true;
                        if (!SmokeSensorStatus[sensorindex].GEMImmediatelyUpdate)
                            SmokeSensorStatus[sensorindex].GEMImmediatelyUpdate = true;

                        IElement element = this.ParamManager().GetElement(SmokeSensorStatus[sensorindex].PropertyPath);
                        if (element != null)
                        {
                            if (element.RaisePropertyChangedFalg == false)
                            {
                                element.RaisePropertyChangedFalg = true;
                            }
                            if (element.GEMImmediatelyUpdate == false)
                            {
                                element.GEMImmediatelyUpdate = true;
                            }
                        }

                        SmokeSensorStatus[sensorindex].Value = ((int)sensorState);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region <remarks> Comm Method <remarks>
        public EventCodeEnum InitData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober)
                {
                    int cellnum = this.LoaderController().GetChuckIndex();
                    if (this.GEMModule().GetPIVContainer().StagePVTemp.Count > 0 &&
                        this.GEMModule().GetPIVContainer().StagePVTemp.Count >= cellnum - 1)
                    {
                        if (this.GEMModule().GetPIVContainer().StagePVTemp[cellnum - 1].GEMEnable)
                        {
                            this.TempController().TempInfo.CurTemp.PropertyChanged -= UpdateCurTempVID;
                            this.TempController().TempInfo.CurTemp.PropertyChanged += UpdateCurTempVID;
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
        public void SetLotID(string lotid)
        {
            try
            {
                LoggerManager.Debug($"SetLotID Before: {LotID.Value}");
                LotID.Value = lotid;
                LoggerManager.Debug($"SetLotID After: {LotID.Value}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public string GetLotID()
        {
            return LotID.Value;
        }

        public void SetCassetteHashCode(string csthashcode)
        {
            try
            {
                LoggerManager.Debug($"SetCassetteHashCode Before: {CassetteHashCode}");
                CassetteHashCode = csthashcode;
                LoggerManager.Debug($"SetCassetteHashCode After: {CassetteHashCode}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public string GetCassetteHashCode()
        {
            return CassetteHashCode;
        }

        #endregion

        #region <remarks> IParam Method </remarks>

        public void SetElementMetaData()
        {
            try
            {
                //SetElementMetaDataMICRON();
                //SetElementMetaDataYMTC();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return;
        }

        public void SetElementMetaDataMICRON()
        {
            #region <remarks> Micron </remarks>
            int defaultElementID = 30000000;
            StageStates.Clear();
            PreHeatStates.Clear();
            StageProberCardIDs.Clear();
            for (int index = 1; index <= 12; index++)
            {
                StageStates.Add(new Element<int>() { VID = 200 + index, ElementID = 30002070 + index, Value = 1 });
                PreHeatStates.Add(new Element<int>() { VID = 250 + index, ElementID = defaultElementID + 250 + index });
                StageProberCardIDs.Add(new Element<string>() { VID = 300 + index, ElementID = 30002010 + index });
            }

            CarrierID.ElementID = 30005002;
            CarrierID.VID = 5002;
            StageNumber.ElementID = 30002002;
            StageNumber.VID = 1090;
            StageState.ElementID = 30002001;
            StageState.VID = 1091;
            PreHeatState.ElementID = 30001241;
            PreHeatState.VID = 1241;
            LotID.ElementID = 30001120;
            LotID.VID = 1520;
            WaferID.ElementID = 30001600;
            WaferID.VID = 1503;
            ProbeCardID.ElementID = 30002004;
            ProbeCardID.VID = 1105;
            ProbeCardState.ElementID = 30001106;
            ProbeCardState.VID = 1106;
            RecipeID.ElementID = 30002005;
            RecipeID.VID = 30098;
            SlotNumber.ElementID = 30001508;
            SlotNumber.VID = 1508;
            //LoadFoupNumber.ElementID = 30005003;
            //LoadFoupNumber.VID = 1035;
            CurTemperature.ElementID = 30030851;
            CurTemperature.VID = 30851;
            SetTemperature.ElementID = 30001025;
            SetTemperature.VID = 1025;
            XCoordinate.ElementID = 30001606;
            XCoordinate.VID = 1595;
            YCoordinate.ElementID = 30001607;
            YCoordinate.VID = 1596;
            CleaningTouchDownCount.ElementID = 30001063;
            CleaningTouchDownCount.VID = 1063;
            DevDownResult.ElementID = 30001037;
            DevDownResult.VID = 1037;

            FoupNumber.ElementName = "FoupNumber";
            FoupNumber.ElementID = 30005001;
            FoupNumber.VID = 1035;
            FoupState.ElementName = "FoupState";
            FoupState.ElementID = 30001036;
            FoupState.VID = 1036;

            ListOfStages.ElementID = 30001140;
            ListOfStages.VID = 1140;

            ListOfSlot.ElementID = 30005004;
            ListOfSlot.VID = 1141;

            FoupStates = new List<Element<int>>();
            for (int index = 1; index <= SystemModuleCount.ModuleCnt.FoupCount; index++)
            {
                FoupStates.Add(new Element<int>() { VID = 350 + index, ElementID = 30001006 + index, Value = 1 });
            }
            ExchangerProberCardIDs = new List<Element<string>>();
            for (int index = 1; index <= 9; index++)
            {
                ExchangerProberCardIDs.Add(new Element<string>() { VID = 330 + index, ElementID = 30000330 + index, Value = "" });
            }
            LightState = new List<Element<int>>();
            for (int index = 1; index <= 3; index++)
            {
                LightState.Add(new Element<int>() { VID = 1037 + index, ElementID = 30001037 + index, Value = 0 });
            }
            FoupAccessModes = new List<Element<int>>();
            for (int index = 1; index <= SystemModuleCount.ModuleCnt.FoupCount; index++)
            {
                FoupAccessModes.Add(new Element<int>() { VID = 1020 + index, ElementID = 30001020 + index, Value = 0 });
            }
            #endregion
        }
        public void SetElementMetaDataYMTC()
        {
            #region <remarks> Main </ramarks>

            int stageCount = 12;

            int defaultElementID = 30000000;
            ArmNumber.VID = 10022;
            ArmNumber.ElementID = defaultElementID + ArmNumber.VID;
            CarrierID.VID = 5002;
            CarrierID.ElementID = defaultElementID + CarrierID.VID;
            FoupNumber.VID = 5001;
            FoupNumber.ElementID = defaultElementID + FoupNumber.VID;
            FullSite.VID = 1605;
            FullSite.ElementID = defaultElementID + FullSite.VID;
            ListOfSlot.VID = 5004;
            ListOfSlot.ElementID = defaultElementID + ListOfSlot.VID;
            //LoadFoupNumber.VID = 5001;
            //LoadFoupNumber.ElementID = defaultElementID + LoadFoupNumber.VID;
            LotID.VID = 1120;
            LotID.ElementID = defaultElementID + LotID.VID;
            PreAlignNumber.VID = 10021;
            PreAlignNumber.ElementID = defaultElementID + PreAlignNumber.VID;
            ProbeCardID.VID = 2004;
            ProbeCardID.ElementID = defaultElementID + ProbeCardID.VID;
            ProberType.VID = 2003;
            ProberType.ElementID = defaultElementID + ProberType.VID;
            RecipeID.VID = 2005;
            RecipeID.ElementID = defaultElementID + RecipeID.VID;
            SoakingTimeSec.VID = 1701;
            SoakingTimeSec.ElementID = defaultElementID + SoakingTimeSec.VID;
            StageNumber.VID = 2002;
            StageNumber.ElementID = defaultElementID + StageNumber.VID;
            WaferID.VID = 2006;
            WaferID.ElementID = defaultElementID + WaferID.VID;
            XCoordinate.VID = 1606;
            XCoordinate.ElementID = defaultElementID + XCoordinate.VID;
            YCoordinate.VID = 1607;
            YCoordinate.ElementID = defaultElementID + YCoordinate.VID;
            VerifyParamResultMap.VID = 2006;
            YCoordinate.ElementID = defaultElementID + VerifyParamResultMap.VID;

            int InitVID = 1007;
            for (int index = 0; index < 3; index++)
            {
                if (StageProberCardIDs.SingleOrDefault(element => element.VID == InitVID + index) == null)
                {
                    FoupStates.Add(new Element<int>() { VID = InitVID + index, ElementID = defaultElementID + InitVID + index });
                }
            }

            InitVID = 2011;
            for (int index = 0; index < 12; index++)
            {
                if (StageProberCardIDs.SingleOrDefault(element => element.VID == InitVID + index) == null)
                {
                    StageProberCardIDs.Add(new Element<string>()
                    { VID = InitVID + index, ElementID = defaultElementID + InitVID + index, Value = "" });
                }
            }

            InitVID = 2071;
            for (int index = 0; index < 12; index++)
            {
                if (StageStates.SingleOrDefault(element => element.VID == InitVID + index) == null)
                {
                    StageStates.Add(new Element<int>()
                    { VID = InitVID + index, ElementID = defaultElementID + InitVID + index });
                }
            }

            InitVID = 2031;
            for (int index = 0; index < 12; index++)
            {
                if (StageRecipeNames.SingleOrDefault(element => element.VID == InitVID + index) == null)
                {
                    StageRecipeNames.Add(new Element<string>()
                    { VID = InitVID + index, ElementID = defaultElementID + InitVID + index, Value = "" });
                }
            }

            InitVID = 2051;
            for (int index = 0; index < 12; index++)
            {
                if (StageSVTemp.SingleOrDefault(element => element.VID == InitVID + index) == null)
                {
                    StageSVTemp.Add(new Element<string>()
                    { VID = InitVID + index, ElementID = defaultElementID + InitVID + index, Value = "" });
                }
            }

            InitVID = 2101;
            for (int index = 0; index < 12; index++)
            {
                if (StagePVTemp.SingleOrDefault(element => element.VID == InitVID + index) == null)
                {
                    StagePVTemp.Add(new Element<string>()
                    { VID = InitVID + index, ElementID = defaultElementID + InitVID + index, Value = "" });
                }
            }

            InitVID = 1021;
            FoupAccessModes = new List<Element<int>>();
            for (int index = 0; index < 3; index++)
            {
                if (FoupAccessModes.SingleOrDefault(element => element.VID == InitVID + index) == null)
                {
                    FoupAccessModes.Add(new Element<int>()
                    { VID = InitVID + index, ElementID = defaultElementID + InitVID + index, Value = 0 });
                }
            }

            LightState = new List<Element<int>>();
            for (int index = 0; index < 3; index++)
            {
                if (LightState.SingleOrDefault(element => element.VID == InitVID + index) == null)
                {
                    LightState.Add(new Element<int>() { VID = 1038 + index, Value = 0 });
                }
            }

            DryAirStates = new List<Element<bool>>();
            for (int index = 0; index < stageCount; index++)
            {
                if (DryAirStates.SingleOrDefault(element => element.VID == InitVID + index) == null)
                {
                    DryAirStates.Add(new Element<bool>() { VID = 2151 + index, Value = false });
                }
            }

            #endregion
        }

        //public Element<string> SetTempElement 
        //    = new Element<string>() { RaisePropertyChangedFalg = true,
        //                              GEMImmediatelyUpdate = true};


        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    if (System.AppDomain.CurrentDomain.FriendlyName == "ProberSystem.exe")
                    {

                        if (this.LoaderController() != null)
                        {
                            StageNumber.Value = this.LoaderController().GetChuckIndex();
                        }

                        ProberType.Value = this.FileManager().GetProberID();

                        int gpStageCount = SystemModuleCount.ModuleCnt.StageCount;

                        if (StageStates.Count != gpStageCount | PreHeatStates.Count != gpStageCount | StageProberCardIDs.Count != gpStageCount)
                        {
                            StageStates = new List<Element<int>>();
                            PreHeatStates = new List<Element<int>>();
                            StageProberCardIDs = new List<Element<string>>();
                            StageRecipeNames = new List<Element<string>>();
                            StageSVTemp = new List<Element<string>>();
                            StagePVTemp = new List<Element<string>>();
                            for (int index = 1; index <= gpStageCount; index++)
                            {
                                StageStates.Add(new Element<int>() { RaisePropertyChangedFalg = true, GEMImmediatelyUpdate = true });
                                PreHeatStates.Add(new Element<int>() { RaisePropertyChangedFalg = true, GEMImmediatelyUpdate = true });
                                StageProberCardIDs.Add(new Element<string>() { RaisePropertyChangedFalg = true, GEMImmediatelyUpdate = true });
                                StageRecipeNames.Add(new Element<string>() { RaisePropertyChangedFalg = true, GEMImmediatelyUpdate = true });
                                StageSVTemp.Add(new Element<string>() { RaisePropertyChangedFalg = true, GEMImmediatelyUpdate = true });
                                StagePVTemp.Add(new Element<string>() { RaisePropertyChangedFalg = true, GEMImmediatelyUpdate = true });

                                StageProberIDs.Add(new Element<string>() { RaisePropertyChangedFalg = true, GEMImmediatelyUpdate = true });
                            }
                            for (int index = 0; index < gpStageCount; index++)
                            {
                                StageStates[index].RaisePropertyChangedFalg = true;
                                StageStates[index].GEMImmediatelyUpdate = true;
                                PreHeatStates[index].RaisePropertyChangedFalg = true;
                                PreHeatStates[index].GEMImmediatelyUpdate = true;
                                StageProberCardIDs[index].RaisePropertyChangedFalg = true;
                                StageProberCardIDs[index].GEMImmediatelyUpdate = true;
                                StageRecipeNames[index].RaisePropertyChangedFalg = true;
                                StageRecipeNames[index].GEMImmediatelyUpdate = true;
                                StageSVTemp[index].RaisePropertyChangedFalg = true;
                                StageSVTemp[index].GEMImmediatelyUpdate = true;
                                StagePVTemp[index].RaisePropertyChangedFalg = true;
                                StagePVTemp[index].GEMImmediatelyUpdate = true;
                                StageProberIDs[index].RaisePropertyChangedFalg = true;
                                StageProberIDs[index].GEMImmediatelyUpdate = true;
                            }
                            StageProberIDs[StageNumber.Value - 1].Value = this.FileManager().GetProberID();
                        }
                    }
                    if (System.AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")
                    {
                        int foupCount = SystemModuleCount.ModuleCnt.FoupCount;
                        int stageCount = SystemModuleCount.ModuleCnt.StageCount;
                        LoaderID.Value = this.FileManager().GetProberID();
                        for (int index = 1; index <= foupCount; index++)
                        {
                            FoupStates.Add(new Element<int>() { RaisePropertyChangedFalg = true });
                            FoupStates[index - 1].RaisePropertyChangedFalg = true;
                            FoupInfos.Add(new FoupLotInfo());
                            FoupAccessModes.Add(new Element<int>() { RaisePropertyChangedFalg = true });
                            FoupAccessModes[index - 1].RaisePropertyChangedFalg = true;
                            LoaderCarrierIDs.Add(new Element<string>() { RaisePropertyChangedFalg = true });
                            LoaderCarrierIDs[index - 1].RaisePropertyChangedFalg = true;
                            LoaderSelectedSlotLists.Add(new Element<string>() { RaisePropertyChangedFalg = true });
                            LoaderSelectedSlotLists[index - 1].RaisePropertyChangedFalg = true;
                            LoaderLotIDs.Add(new Element<string>() { RaisePropertyChangedFalg = true });
                            LoaderLotIDs[index - 1].RaisePropertyChangedFalg = true;
                        }
                        for (int index = 1; index <= 9; index++)
                        {
                            ExchangerProberCardIDs.Add(new Element<string>() { RaisePropertyChangedFalg = true });
                            ExchangerProberCardIDs[index - 1].RaisePropertyChangedFalg = true;
                        }

                        for (int index = 1; index <= 3; index++)
                        {
                            LightState.Add(new Element<int>() { RaisePropertyChangedFalg = true, Value = 999 });
                            LightState[index - 1].RaisePropertyChangedFalg = true;
                        }

                        for (int index = 1; index <= SystemModuleCount.ModuleCnt.StageCount; index++)
                        {
                            DryAirStates.Add(new Element<bool>() { RaisePropertyChangedFalg = true, Value = false });
                            DryAirStates[index - 1].RaisePropertyChangedFalg = true;
                        }

                        for (int index = 1; index <= SystemModuleCount.ModuleCnt.CardBufferCount; index++)
                        {
                            CardBufferInfos.Add(new CardBufferInfo());
                            CardBufferStates.Add(new Element<int>() { RaisePropertyChangedFalg = true });
                            CardBufferStates[index - 1].RaisePropertyChangedFalg = true;
                            CardBufferCardIds.Add(new Element<string>() { RaisePropertyChangedFalg = true });
                            CardBufferCardIds[index - 1].RaisePropertyChangedFalg = true;
                            CardBufferAccessModes.Add(new Element<int>() { RaisePropertyChangedFalg = true });
                            CardBufferAccessModes[index - 1].RaisePropertyChangedFalg = true;
                        }

                        for (int index = 1; index <= SystemModuleCount.ModuleCnt.FixedTrayCount; index++)
                        {
                            FixedTrayPolishWaferIds.Add(new Element<string>() { RaisePropertyChangedFalg = true });
                            FixedTrayPolishWaferIds[index - 1].RaisePropertyChangedFalg = true;
                            FixedTrayPolishWaferTouchCounts.Add(new Element<double>() { RaisePropertyChangedFalg = true });
                            FixedTrayPolishWaferTouchCounts[index - 1].RaisePropertyChangedFalg = true;
                        }
                    }

                    int count = this.EnvMonitoringManager().GetSensorMaxCount();
                    for (int index = 1; index <= count; index++)
                    {
                        SmokeSensorTemp.Add(new Element<double>() { RaisePropertyChangedFalg = true, Value = -1 });
                        SmokeSensorTemp[index - 1].RaisePropertyChangedFalg = true;
                        SmokeSensorStatus.Add(new Element<int>() { RaisePropertyChangedFalg = true, Value = 999 });
                        SmokeSensorStatus[index - 1].RaisePropertyChangedFalg = true;
                    }
                }
                else if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    ProberType.Value = this.FileManager().GetProberID();
                    if (StageStates.Count == 0)
                    {
                        StageStates.Add(new Element<int>() { RaisePropertyChangedFalg = true, GEMImmediatelyUpdate = true });
                    }
                    if (PreHeatStates.Count == 0)
                    {
                        PreHeatStates.Add(new Element<int>() { RaisePropertyChangedFalg = true, GEMImmediatelyUpdate = true });
                    }
                    if (StageProberCardIDs.Count == 0)
                    {
                        StageProberCardIDs.Add(new Element<string>() { RaisePropertyChangedFalg = true, GEMImmediatelyUpdate = true });
                    }
                    if (StageRecipeNames.Count == 0)
                    {
                        StageRecipeNames.Add(new Element<string>() { RaisePropertyChangedFalg = true, GEMImmediatelyUpdate = true });
                    }
                    if (StageSVTemp.Count == 0)
                    {
                        StageSVTemp.Add(new Element<string>() { RaisePropertyChangedFalg = true, GEMImmediatelyUpdate = true });
                    }
                    if (StagePVTemp.Count == 0)
                    {
                        StagePVTemp.Add(new Element<string>() { RaisePropertyChangedFalg = true, GEMImmediatelyUpdate = true });
                    }
                    if (StageProberIDs.Count == 0)
                    {
                        StageProberIDs.Add(new Element<string>() { RaisePropertyChangedFalg = true, GEMImmediatelyUpdate = true });
                    }
                    StageProberIDs[0].Value = this.FileManager().GetProberID();
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum SetEmulParam()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetDefaultParam()
        {
            return EventCodeEnum.NONE;
        }

        #endregion

        #region <remarks> Methof </remarks>
        private void UpdateCurTempVID(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                var element = sender as IElement;
                if (element != null)
                {
                    SetPVTemp(Convert.ToDouble(element.GetValue()));
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion
    }

    /// <summary>
    /// 내부변수로 사용, Update를  자주할 필요가 없거나 Event 내보내는 타이밍에 이전에 이미 값을가지고 있는 파라미터들을 정의함.
    /// DVID로 보내줘야되는 파라미터 세트를 상시로 가지고 있다고 보면됨.
    /// </summary>
    public class CardBufferInfo
    {
        private int _Index;

        public int Index
        {
            get { return _Index; }
            set { _Index = value; }
        }

        private int _LoadPortState;

        public int LoadPortState
        {
            get { return _LoadPortState; }
            set { _LoadPortState = value; }
        }

        private string _PCardId = "";

        public string PCardId
        {
            get { return _PCardId; }
            set { _PCardId = value; }
        }

        private int _AccessMode = 0;

        public int AccessMode
        {
            get { return _AccessMode; }
            set { _AccessMode = value; }
        }

    }
}
