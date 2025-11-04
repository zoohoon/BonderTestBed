using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ProberInterfaces.GEM
{
    public enum EnumVidType
    {
        NONE,
        SVID,
        DVID,
        ECID
    }

    public enum EnumVidObjectType
    {
        NONE = 100,
        OBJECT = 200,
    }


    public enum VidPropertyTypeEnum
    {
        NONE,
        LIST, // 파라미터에있는 LIST 에서 처리
        CLIST // DB 에 있는 같은 Element 를 LIST 처리.
    }

    public enum VidUpdateTypeEnum
    {
        SINGLE,
        CELL,
        COMMANDER,
        BOTH
    }



    [Serializable]
    public class ProberGemIdDictionaryParam : ISystemParameterizable, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        #region ==> IParam Implement
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [JsonIgnore, ParamIgnore]
        public List<object> Nodes { get; set; }
        [JsonIgnore, ParamIgnore]
        public string Genealogy { get; set; }
        [JsonIgnore, ParamIgnore]
        public object Owner { get; set; }
        #endregion

        public string FilePath { get; } = "GEM";
        public string FileName { get; } = "ProberGemIdDictionaryParam.Json";

        private EnumVidType _VidType = EnumVidType.SVID;//test code//
        public EnumVidType VidType
        {
            get { return _VidType; }
            set
            {
                if (_VidType != value)
                {
                    _VidType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<ObservableProberGemIdDictionary<string, GemVidInfo>> _DicProberGemID
            = new Element<ObservableProberGemIdDictionary<string, GemVidInfo>>()
            { Value = new ObservableProberGemIdDictionary<string, GemVidInfo>() };
        public Element<ObservableProberGemIdDictionary<string, GemVidInfo>> DicProberGemID
        {
            get { return _DicProberGemID; }
            set
            {
                if (_DicProberGemID != value)
                {
                    _DicProberGemID = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum Init()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = SetDefaultHynix();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SetDefaultHynix()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ObservableProberGemIdDictionary<string, GemVidInfo> dicobj = new ObservableProberGemIdDictionary<string, GemVidInfo>();
                switch (VidType)
                {
                    case EnumVidType.NONE:
                        break;
                    case EnumVidType.SVID:
                        {
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.DevDownResult", new GemVidInfo(1037, VidUpdateTypeEnum.BOTH)));
                            // 1095 (현재 시간) 는 정의되어 있지 않아도, Gem 동글에서 알아서 제공 됨. - Format : YYYYMMDDhhmmsscc
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.SystemClock", new GemVidInfo(1095, VidUpdateTypeEnum.BOTH)));

                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.FoupStates[0]", new GemVidInfo(1007, VidUpdateTypeEnum.COMMANDER, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.FoupStates[1]", new GemVidInfo(1008, VidUpdateTypeEnum.COMMANDER, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.LoaderID", new GemVidInfo(1202, VidUpdateTypeEnum.COMMANDER)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.LoaderLotIDs[0]", new GemVidInfo(1120, VidUpdateTypeEnum.COMMANDER, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.LoaderLotIDs[1]", new GemVidInfo(1121, VidUpdateTypeEnum.COMMANDER, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.LoaderCarrierIDs[0]", new GemVidInfo(5002, VidUpdateTypeEnum.COMMANDER, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.LoaderCarrierIDs[1]", new GemVidInfo(5003, VidUpdateTypeEnum.COMMANDER, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.LoaderSelectedSlotLists[0]", new GemVidInfo(1150, VidUpdateTypeEnum.COMMANDER, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.LoaderSelectedSlotLists[1]", new GemVidInfo(1151, VidUpdateTypeEnum.COMMANDER, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.CardBufferCardIds[0]", new GemVidInfo(6020, VidUpdateTypeEnum.COMMANDER, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.CardBufferStates[0]", new GemVidInfo(6001, VidUpdateTypeEnum.COMMANDER, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageProberIDs[0]", new GemVidInfo(2003, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageProberIDs[1]", new GemVidInfo(2004, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageProberIDs[2]", new GemVidInfo(2005, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageProberCardIDs[0]", new GemVidInfo(2011, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageProberCardIDs[1]", new GemVidInfo(2012, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageProberCardIDs[2]", new GemVidInfo(2013, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StagePVTemp[0]", new GemVidInfo(2101, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StagePVTemp[1]", new GemVidInfo(2102, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StagePVTemp[2]", new GemVidInfo(2103, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageStates[0]", new GemVidInfo(2071, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageStates[1]", new GemVidInfo(2072, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageStates[2]", new GemVidInfo(2073, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));

                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.FixedTrayPolishWaferIds[0]", new GemVidInfo(6041, VidUpdateTypeEnum.BOTH, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.FixedTrayPolishWaferIds[1]", new GemVidInfo(6042, VidUpdateTypeEnum.BOTH, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.FixedTrayPolishWaferIds[2]", new GemVidInfo(6043, VidUpdateTypeEnum.BOTH, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.FixedTrayPolishWaferIds[3]", new GemVidInfo(6044, VidUpdateTypeEnum.BOTH, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.FixedTrayPolishWaferIds[4]", new GemVidInfo(6045, VidUpdateTypeEnum.BOTH, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.FixedTrayPolishWaferIds[5]", new GemVidInfo(6046, VidUpdateTypeEnum.BOTH, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.FixedTrayPolishWaferIds[6]", new GemVidInfo(6047, VidUpdateTypeEnum.BOTH, true)));

                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.FixedTrayPolishWaferTouchCounts[0]", new GemVidInfo(6051, VidUpdateTypeEnum.BOTH, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.FixedTrayPolishWaferTouchCounts[1]", new GemVidInfo(6052, VidUpdateTypeEnum.BOTH, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.FixedTrayPolishWaferTouchCounts[2]", new GemVidInfo(6053, VidUpdateTypeEnum.BOTH, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.FixedTrayPolishWaferTouchCounts[3]", new GemVidInfo(6054, VidUpdateTypeEnum.BOTH, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.FixedTrayPolishWaferTouchCounts[4]", new GemVidInfo(6055, VidUpdateTypeEnum.BOTH, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.FixedTrayPolishWaferTouchCounts[5]", new GemVidInfo(6056, VidUpdateTypeEnum.BOTH, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.FixedTrayPolishWaferTouchCounts[6]", new GemVidInfo(6057, VidUpdateTypeEnum.BOTH, true)));

                            int vid = 2201;
                            int count = this.EnvMonitoringManager().GetSensorMaxCount();
                            for (int i = 0; i < count; i++)
                            {                                
                                dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.SmokeSensorTemp[" + i + "]" , new GemVidInfo(vid + i, VidUpdateTypeEnum.COMMANDER, true)));
                                dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.SmokeSensorStatus[" + i + "]", new GemVidInfo(vid + i + 1, VidUpdateTypeEnum.COMMANDER, true)));
                                ++vid;
                            }                                                       
                        }
                        break;
                    case EnumVidType.DVID:
                        {
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.FoupNumber", new GemVidInfo(1149, VidUpdateTypeEnum.BOTH))); 
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.LotID", new GemVidInfo(51102, VidUpdateTypeEnum.BOTH))); 
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.CarrierID", new GemVidInfo(55002, VidUpdateTypeEnum.BOTH)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.ListOfSlot", new GemVidInfo(51150, VidUpdateTypeEnum.BOTH)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.AccessMode", new GemVidInfo(51021, VidUpdateTypeEnum.BOTH)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.ProberType", new GemVidInfo(51202, VidUpdateTypeEnum.BOTH)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.RecipeID", new GemVidInfo(52031, VidUpdateTypeEnum.BOTH)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.ProbeCardID", new GemVidInfo(52003, VidUpdateTypeEnum.BOTH)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.CurTemperature", new GemVidInfo(52101, VidUpdateTypeEnum.BOTH)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.SetTemperature", new GemVidInfo(52051, VidUpdateTypeEnum.BOTH)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.Overdrive", new GemVidInfo(52300, VidUpdateTypeEnum.BOTH)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.WaferID", new GemVidInfo(1600, VidUpdateTypeEnum.BOTH)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.StageNumber", new GemVidInfo(2002, VidUpdateTypeEnum.BOTH)));
                            //dicobj.Add(new KeyValuePair<string, GemVidInfo>("", new GemVidInfo(5001, VidUpdateTypeEnum.BOTH))); // DB와 Utility 에 5001 데이터 없음.
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.PreLoadingWaferId", new GemVidInfo(1507, VidUpdateTypeEnum.BOTH)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.PreLoadingSlotNum", new GemVidInfo(1506, VidUpdateTypeEnum.BOTH)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.UnloadingSlotNum", new GemVidInfo(20100, VidUpdateTypeEnum.BOTH)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.UnloadingWaferID", new GemVidInfo(20101, VidUpdateTypeEnum.BOTH)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.SlotNumber", new GemVidInfo(1508, VidUpdateTypeEnum.BOTH)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.NotchAngle", new GemVidInfo(1522, VidUpdateTypeEnum.BOTH)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.WaferEndResult", new GemVidInfo(1538, VidUpdateTypeEnum.BOTH)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.TotalDieCount", new GemVidInfo(30848, VidUpdateTypeEnum.BOTH)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.PassDieCount", new GemVidInfo(1601, VidUpdateTypeEnum.BOTH)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.FailDieCount", new GemVidInfo(1602, VidUpdateTypeEnum.BOTH)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.YieldOfBadDie", new GemVidInfo(1603, VidUpdateTypeEnum.BOTH)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.WaferStartTime", new GemVidInfo(1610, VidUpdateTypeEnum.BOTH)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.WaferEndTime", new GemVidInfo(1611, VidUpdateTypeEnum.BOTH)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.PCardContactCount", new GemVidInfo(30854, VidUpdateTypeEnum.BOTH)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.XCoordinate", new GemVidInfo(1595, VidUpdateTypeEnum.BOTH)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.YCoordinate", new GemVidInfo(1596, VidUpdateTypeEnum.BOTH)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.CardSiteLocation", new GemVidInfo(30285, VidUpdateTypeEnum.BOTH)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.CardBufferIndex", new GemVidInfo(8000, VidUpdateTypeEnum.COMMANDER)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.CardLPState", new GemVidInfo(56001, VidUpdateTypeEnum.COMMANDER)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.CardLPAccessMode", new GemVidInfo(56040, VidUpdateTypeEnum.COMMANDER)));

                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.WaferAutofeedResult", new GemVidInfo(30511, VidUpdateTypeEnum.BOTH)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.PolishWaferIDReadResult", new GemVidInfo(30512, VidUpdateTypeEnum.BOTH)));
                        }
                        break;
                    case EnumVidType.ECID:
                        {
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageRecipeNames[0]", new GemVidInfo(2031, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageRecipeNames[1]", new GemVidInfo(2032, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageRecipeNames[2]", new GemVidInfo(2033, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageSVTemp[0]", new GemVidInfo(2051, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageSVTemp[1]", new GemVidInfo(2052, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageSVTemp[2]", new GemVidInfo(2053, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("Probing.ProbingModuleDevParam.OverDrive", new GemVidInfo(2300, VidPropertyTypeEnum.CLIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.FoupShiftMode", new GemVidInfo(3800, VidUpdateTypeEnum.COMMANDER)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.FoupAccessModes[0]", new GemVidInfo(1021, VidUpdateTypeEnum.COMMANDER, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.FoupAccessModes[1]", new GemVidInfo(1022, VidUpdateTypeEnum.COMMANDER)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.CardBufferAccessModes[0]", new GemVidInfo(6040, VidUpdateTypeEnum.COMMANDER)));
                        }
                        break;
                    default:
                        break;
                }
                DicProberGemID.Value = dicobj;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SetDefaultYMTC()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ObservableProberGemIdDictionary<string, GemVidInfo> dicobj = new ObservableProberGemIdDictionary<string, GemVidInfo>();
                switch (VidType)
                {
                    case EnumVidType.NONE:
                        break;
                    case EnumVidType.SVID:
                        {
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.FoupStates[0]", new GemVidInfo(1007, VidUpdateTypeEnum.COMMANDER, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.FoupStates[1]", new GemVidInfo(1008, VidUpdateTypeEnum.COMMANDER, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.FoupStates[2]", new GemVidInfo(1009, VidUpdateTypeEnum.COMMANDER, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.FoupAccessModes[0]", new GemVidInfo(1021, VidUpdateTypeEnum.COMMANDER, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.FoupAccessModes[1]", new GemVidInfo(1022, VidUpdateTypeEnum.COMMANDER, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.FoupAccessModes[2]", new GemVidInfo(1023, VidUpdateTypeEnum.COMMANDER, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.LotID", new GemVidInfo(1120, VidUpdateTypeEnum.BOTH)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.StageNumber", new GemVidInfo(2002, VidUpdateTypeEnum.CELL)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.ProberType", new GemVidInfo(2003, VidUpdateTypeEnum.CELL)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.ProbeCardID", new GemVidInfo(2004, VidUpdateTypeEnum.CELL)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageProberCardIDs[0]", new GemVidInfo(2011, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageProberCardIDs[1]", new GemVidInfo(2012, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageProberCardIDs[2]", new GemVidInfo(2013, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageProberCardIDs[3]", new GemVidInfo(2014, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageProberCardIDs[4]", new GemVidInfo(2015, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageProberCardIDs[5]", new GemVidInfo(2016, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageProberCardIDs[6]", new GemVidInfo(2017, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageProberCardIDs[7]", new GemVidInfo(2018, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageProberCardIDs[8]", new GemVidInfo(2019, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageProberCardIDs[9]", new GemVidInfo(2020, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageProberCardIDs[10]", new GemVidInfo(2021, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageProberCardIDs[11]", new GemVidInfo(2022, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageStates[0]", new GemVidInfo(2071, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageStates[1]", new GemVidInfo(2072, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageStates[2]", new GemVidInfo(2073, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageStates[3]", new GemVidInfo(2074, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageStates[4]", new GemVidInfo(2075, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageStates[5]", new GemVidInfo(2076, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageStates[6]", new GemVidInfo(2077, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageStates[7]", new GemVidInfo(2078, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageStates[8]", new GemVidInfo(2079, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageStates[9]", new GemVidInfo(2080, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageStates[10]", new GemVidInfo(2081, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageStates[11]", new GemVidInfo(2082, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.LoadFoupNumber", new GemVidInfo(5001, VidUpdateTypeEnum.COMMANDER)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.FoupNumber", new GemVidInfo(5001, VidUpdateTypeEnum.COMMANDER)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.CarrierID", new GemVidInfo(5002, VidUpdateTypeEnum.COMMANDER)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.ListOfSlot", new GemVidInfo(5004, VidUpdateTypeEnum.COMMANDER)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.PreAlignNumber", new GemVidInfo(10021, VidUpdateTypeEnum.COMMANDER)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.ArmNumber", new GemVidInfo(10022, VidUpdateTypeEnum.COMMANDER)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StagePVTemp[0]", new GemVidInfo(2101, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StagePVTemp[1]", new GemVidInfo(2102, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StagePVTemp[2]", new GemVidInfo(2103, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StagePVTemp[3]", new GemVidInfo(2104, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StagePVTemp[4]", new GemVidInfo(2105, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StagePVTemp[5]", new GemVidInfo(2106, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StagePVTemp[6]", new GemVidInfo(2107, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StagePVTemp[7]", new GemVidInfo(2108, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StagePVTemp[8]", new GemVidInfo(2109, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StagePVTemp[9]", new GemVidInfo(2110, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StagePVTemp[10]", new GemVidInfo(2111, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StagePVTemp[11]", new GemVidInfo(2112, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.DryAirStates[0]", new GemVidInfo(2141, VidUpdateTypeEnum.COMMANDER, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.DryAirStates[1]", new GemVidInfo(2142, VidUpdateTypeEnum.COMMANDER, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.DryAirStates[2]", new GemVidInfo(2143, VidUpdateTypeEnum.COMMANDER, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.DryAirStates[3]", new GemVidInfo(2144, VidUpdateTypeEnum.COMMANDER, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.DryAirStates[4]", new GemVidInfo(2145, VidUpdateTypeEnum.COMMANDER, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.DryAirStates[5]", new GemVidInfo(2146, VidUpdateTypeEnum.COMMANDER, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.DryAirStates[6]", new GemVidInfo(2147, VidUpdateTypeEnum.COMMANDER, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.DryAirStates[7]", new GemVidInfo(2148, VidUpdateTypeEnum.COMMANDER, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.DryAirStates[8]", new GemVidInfo(2149, VidUpdateTypeEnum.COMMANDER, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.DryAirStates[9]", new GemVidInfo(2150, VidUpdateTypeEnum.COMMANDER, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.DryAirStates[10]", new GemVidInfo(2151, VidUpdateTypeEnum.COMMANDER, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.DryAirStates[11]", new GemVidInfo(2152, VidUpdateTypeEnum.COMMANDER, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("PinAligner.PinAlignDevParameters.PinPlaneAdjustParam.EnablePinPlaneCompensation", new GemVidInfo(2121, VidPropertyTypeEnum.CLIST, VidUpdateTypeEnum.CELL, true)));

                        }
                        break;
                    case EnumVidType.DVID:
                        {
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.WaferID", new GemVidInfo(1600, VidUpdateTypeEnum.BOTH)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.FullSite", new GemVidInfo(1605, VidUpdateTypeEnum.CELL)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.XCoordinate", new GemVidInfo(1606, VidUpdateTypeEnum.CELL)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.YCoordinate", new GemVidInfo(1607, VidUpdateTypeEnum.CELL)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.SoakingTimeSec", new GemVidInfo(1701, VidUpdateTypeEnum.CELL)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.RecipeID", new GemVidInfo(2005, VidUpdateTypeEnum.CELL)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEM.PIVContainer.VerifyParamResultMap", new GemVidInfo(2006, VidUpdateTypeEnum.COMMANDER)));
                        }
                        break;
                    case EnumVidType.ECID:
                        {
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageRecipeNames[0]", new GemVidInfo(2031, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageRecipeNames[1]", new GemVidInfo(2032, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageRecipeNames[2]", new GemVidInfo(2033, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageRecipeNames[3]", new GemVidInfo(2034, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageRecipeNames[4]", new GemVidInfo(2035, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageRecipeNames[5]", new GemVidInfo(2036, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageRecipeNames[6]", new GemVidInfo(2037, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageRecipeNames[7]", new GemVidInfo(2038, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageRecipeNames[8]", new GemVidInfo(2039, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageRecipeNames[9]", new GemVidInfo(2040, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageRecipeNames[10]", new GemVidInfo(2041, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageRecipeNames[11]", new GemVidInfo(2042, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageSVTemp[0]", new GemVidInfo(2051, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageSVTemp[1]", new GemVidInfo(2052, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageSVTemp[2]", new GemVidInfo(2053, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageSVTemp[3]", new GemVidInfo(2054, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageSVTemp[4]", new GemVidInfo(2055, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageSVTemp[5]", new GemVidInfo(2056, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageSVTemp[6]", new GemVidInfo(2057, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageSVTemp[7]", new GemVidInfo(2058, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageSVTemp[8]", new GemVidInfo(2059, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageSVTemp[9]", new GemVidInfo(2060, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageSVTemp[10]", new GemVidInfo(2061, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("GEMModule.PIVContainer.StageSVTemp[11]", new GemVidInfo(2062, VidPropertyTypeEnum.LIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("Probing.ProbingModuleDevParam.OverDrive", new GemVidInfo(2161, VidPropertyTypeEnum.CLIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("PMIModule.PMIModuleDevParam.NormalPMI", new GemVidInfo(2181, VidPropertyTypeEnum.CLIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("SoakingModule.SoakingDeviceFile.EveryWaferEventSoaking[6].Enable", new GemVidInfo(2201, VidPropertyTypeEnum.CLIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("SoakingModule.SoakingDeviceFile.EveryWaferEventSoaking[6].ZClearance", new GemVidInfo(2221, VidPropertyTypeEnum.CLIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("SoakingModule.SoakingDeviceFile.AutoSoakingParam.Enable", new GemVidInfo(2241, VidPropertyTypeEnum.CLIST, VidUpdateTypeEnum.CELL, true)));
                            dicobj.Add(new KeyValuePair<string, GemVidInfo>("SoakingModule.SoakingDeviceFile.AutoSoakingParam.ZClearance", new GemVidInfo(2261, VidPropertyTypeEnum.CLIST, VidUpdateTypeEnum.CELL, true)));
                        }
                        break;
                    default:
                        break;
                }
                DicProberGemID.Value = dicobj;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            SetDefaultParam();
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public void SetElementMetaData()
        {
        }
    }

    #region ==> ObservableKeyValuePair
    [Serializable]
    public class ObservableProberGemIDPair<TKey, TValue> : INotifyPropertyChanged
    {
        #region properties
        private TKey _ProberID;
        private TValue _GemID;

        // Key
        public TKey ProberID
        {
            get { return _ProberID; }
            set
            {
                _ProberID = value;
                OnPropertyChanged(nameof(ProberID));
            }
        }

        // Value
        public TValue GemID
        {
            get { return _GemID; }
            set
            {
                this._GemID = value;
                OnPropertyChanged(nameof(GemID));
            }
        }
        #endregion

        #region INotifyPropertyChanged Members
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }

    [Serializable]
    public class ObservableProberGemIdDictionary<TKey, TValue> : ObservableCollection<ObservableProberGemIDPair<TKey, TValue>>, IDictionary<TKey, TValue>
    {

        #region IDictionary<TKey,TValue> Members

        public void Add(TKey key, TValue value)
        {
            if (ContainsKey(key))
            {
                throw new ArgumentException("The dictionary already contains the key");
            }
            base.Add(new ObservableProberGemIDPair<TKey, TValue>() { ProberID = key, GemID = value });
        }

        public bool ContainsKey(TKey key)
        {
            //var m=base.FirstOrDefault((i) => i.Key == key);
            var r = ThisAsCollection().FirstOrDefault((i) => Equals(key, i.ProberID));

            return !Equals(default(ObservableProberGemIDPair<TKey, TValue>), r);
        }

        bool Equals(TKey a, TKey b)
        {
            return EqualityComparer<TKey>.Default.Equals(a, b);
        }

        private ObservableCollection<ObservableProberGemIDPair<TKey, TValue>> ThisAsCollection()
        {
            return this;
        }

        public ICollection<TKey> Keys
        {
            get { return (from i in ThisAsCollection() select i.ProberID).ToList(); }
        }

        public bool Remove(TKey key)
        {
            var remove = ThisAsCollection().Where(pair => Equals(key, pair.ProberID)).ToList();
            foreach (var pair in remove)
            {
                ThisAsCollection().Remove(pair);
            }
            return remove.Count > 0;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default(TValue);
            var r = GetKvpByTheKey(key);
            if (r == null || Equals(r, default(ObservableProberGemIDPair<TKey, TValue>)))
            {
                return false;
            }
            value = r.GemID;
            return true;
        }

        private ObservableProberGemIDPair<TKey, TValue> GetKvpByTheKey(TKey key)
        {
            return ThisAsCollection().FirstOrDefault((i) => i.ProberID.Equals(key));
        }

        public ICollection<TValue> Values
        {
            get { return (from i in ThisAsCollection() select i.GemID).ToList(); }
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue result;
                if (!TryGetValue(key, out result))
                {
                    throw new ArgumentException("Key not found");
                }
                return result;
            }
            set
            {
                if (ContainsKey(key))
                {
                    GetKvpByTheKey(key).GemID = value;
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            var r = GetKvpByTheKey(item.Key);
            if (Equals(r, default(ObservableProberGemIDPair<TKey, TValue>)))
            {
                return false;
            }
            return Equals(r.GemID, item.Value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            var r = GetKvpByTheKey(item.Key);
            if (Equals(r, default(ObservableProberGemIDPair<TKey, TValue>)))
            {
                return false;
            }
            if (!Equals(r.GemID, item.Value))
            {
                return false;
            }
            return ThisAsCollection().Remove(r);
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        public new IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return (from i in ThisAsCollection() select new KeyValuePair<TKey, TValue>(i.ProberID, i.GemID)).ToList().GetEnumerator();
        }

        #endregion
    }
    #endregion
}
