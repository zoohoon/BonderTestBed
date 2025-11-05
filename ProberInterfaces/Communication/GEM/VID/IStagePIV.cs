namespace ProberInterfaces
{
    using LogModule;
    using ProberErrorCode;
    using System;
    using System.Collections.Generic;

    public interface IPIVContainer : IStagePIV, ILoaderPIV
    {
        EventCodeEnum InitData();
        void SetLotID(string lotid);
        string GetLotID();
        new object GetPIVDataLockObject(); 
        void ChangededControlStateEvent();
        void SetCassetteHashCode(string csthashcode);
        string GetCassetteHashCode();
    }
    public interface IStagePIV
    {
        Element<int> StageNumber { get; set; }
        Element<int> StageState { get; set; }
        Element<int> PreHeatState { get; set; }
        Element<string> LotID { get; set; }
        Element<string> WaferID { get; set; }
        Element<string> UnloadedFormChuckWaferID { get; set; }
        Element<string> ProbeCardID { get; set; }
        Element<string> RecipeID { get; set; }        
        Element<int> SlotNumber { get; set; }
        Element<string> CarrierID { get; set; }
        //Element<int> LoadFoupNumber { get; set; } // FoupNumber와 중복되므로 삭제
        Element<double> CurTemperature { get; set; }
        Element<double> SetTemperature { get; set; }
        Element<int> NotchAngle { get; set; }
        Element<double> Overdrive { get; set; }
        Element<long> XCoordinate { get; set; }
        Element<long> YCoordinate { get; set; }
        Element<int> WaferEndResult { get; set; }
        Element<long> TotalDieCount { get; set; }
        Element<long> PassDieCount { get; set; }
        Element<long> FailDieCount { get; set; }
        Element<double> YieldOfBadDie { get; set; }
        Element<long> PCardContactCount { get; set; }
        Element<int> CleaningTouchDownCount { get; set; }
        Element<int> DevDownResult { get; set; }
        Element<string> ProberType { get; set; }
        Element<string> FullSite { get; set; }
        Element<string> CardSiteLocation { get; set; }
        Element<string> SystemClock { get; set; }
        Element<string> UnloadingWaferID { get; set; }
        Element<int> UnloadingSlotNum { get; set; }
        Element<string> WaferStartTime { get; set; }
        Element<string> WaferEndTime { get; set; }
        Element<double> SoakingTimeSec { get; set; }
        string TmpFullSite { get; set; }
        IList<Element<string>> StagePVTemp { get; set; }
        Element<double> PinAlignPlanarity { get; set; }
        Element<double> PinAlignAngle { get; set; }
        Element<double> PinAlignCardCenterX { get; set; }
        Element<double> PinAlignCardCenterY { get; set; }
        Element<double> PinAlignCardCenterZ { get; set; }

        Element<List<List<double>>> PinAlignResults { get; }
        void SetPinAlignResults(List<List<double>> rst);

        bool IsInsertOCommand { get; set; }
        //bool GetDevDownResult(int foupnumber);
        //void SetDevDownResult(bool result, int foupnumber);
        //void SetDevLoadResult(bool result, int foupnumber);

        Element<int> GetFoupAccessMode(int foupnumber);
        Element<int> GetCardBufferAccessMode(int index);

        void SetStageState(GEMStageStateEnum stateenum);
        void SetPreHeatState(GEMPreHeatStateEnum stateenum);
        void SetWaferID(string waferid);
        void SetOverDrive(double overdrive);
        void SetProberCardID(string probercardid);
        void SetSVTemp(double temp);//#Hynix_Merge: 검토 필요, 통일 필요
        Element<string> GetStageSVTemp(int stgindex);
        void SetPVTemp(double temp);
        //void SetLoadFoupAndLotID(int foupnumber, string lotid);
        Dictionary<int, string> GetLoadFoupAndLotID();
        void SetStageWaferResult(int waferendrst = 1);
        void UpdateStageLotInfo(int foupnumber = 0, string lotid = "");
        void ResetWaferID(string waferid);

        void SetDeviceName(string devicename);
        void SetListOfSlot(int foupnumber, string slotlist);
        void SetUnloadedFormChuckWaferID(string waferid);
        void SetMarkChangeValue(double xoffset, double yoffset, double zoffset);
        void SetDevDownResult(bool result);
    }
}
