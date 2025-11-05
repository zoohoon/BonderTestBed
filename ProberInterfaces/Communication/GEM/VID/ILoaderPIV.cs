using ProberInterfaces.EnvControl.Enum;
using System.Collections.Generic;

namespace ProberInterfaces
{
    public interface ILoaderPIV
    {
        Element<string> LoaderID { get; set; }
        Element<int> FoupNumber { get; set; }
        Element<int> FoupState { get; set; }
        Element<int> FoupShiftMode { get; set; }
        Element<string> ListOfStages { get; set; }
        Element<string> ListOfSlot { get; set; }
        Element<string> LotID { get; set; }
        Element<int> PreLoadingSlotNum { get; set; }
        Element<string> PreLoadingWaferId { get; set; }
        Element<int> PreAlignNumber { get; set; }
        Element<string> VerifyParamResultMap { get; set; }
        Element<int> CardBufferIndex { get; set; }
        Element<int> CardLPState { get; set; }
        Element<int> CardLPAccessMode { get; set; }
        Element<int> WaferAutofeedResult { get; set; }
        Element<int> PolishWaferIDReadResult { get; set; }
        Element<string> WaferChange_Location1_LoadPortId { get; set; }
        Element<string> WaferChange_Location1_AtomId { get; set; }
        Element<string> WaferChange_Location2_LoadPortId { get; set; }
        Element<string> WaferChange_Location2_AtomId { get; set; }
        Element<string> WaferChange_Location1_WaferId { get; set; }
        Element<string> WaferChange_Location2_WaferId { get; set; }


        //void SetFoupState(GEMFoupStateEnum stateenum);
        void SetFoupState(int foupindex, GEMFoupStateEnum stateenum);
        void SetCardBufferState(int index, GEMFoupStateEnum stateenum);
        GEMFoupStateEnum GetFoupState(int foupindex);
        GEMFoupStateEnum GetCardBufferState(int index);
        void SetFoupInfo(int foupindex, string lotid = null, int foupstate = -1, string stagelist = null, string slotlist = null, string devicename = null, string carrierid = null, int accessmode = -1, double processingTemp = -999);
        void SetCardBufferInfo(int index, int lpstate = -1, string pcardid = null, int accessmode = -1);
        void SetFixedTrayInfo(int index, string fixedid , double touchcount );

        void SetCarrierId(int foupindex, string id);
        void SetCardBufferCardId(int index, string id);
        //void SeLoaderId(string id);
        void SetLoaderLotIds(int foupnumber, string lotid = "");
        void UpdateFoupInfo(int foupindex);
        void UpdateCardBufferInfo(int foupindex);
        object GetPIVDataLockObject();
        void SetLightState(LightStateEnum light1, LightStateEnum light2, LightStateEnum light3);
        string GetLotIDAtFoupInfos(int foupindex, string carrierId);
        void SetFoupAccessMode(int foupindex, bool isAutoMode);
        void SetCardBufferAccessMode(int index, bool isAutoMode);
        void SetDryAirValveState(int valveindex, bool state);
        void SetSmokeSensorTempState(int sensorindex, double temp);
        void SetSmokeSensorStatusState(int sensorindex, GEMSensorStatusEnum status);

    }
    public enum LightStateEnum
    {
        OFF = 0,
        ON = 1,
        BLIKING = 2
       
    }

}
