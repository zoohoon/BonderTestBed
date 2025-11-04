using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SecsGemServiceInterface
{

    #region //..CarrierActReqData

    [DataContract]
    [KnownType(typeof(ProceedWithCarrierReqData))]
    [KnownType(typeof(ProceedWithSlotReqData))]
    [KnownType(typeof(CarrieActReqData))]
    [KnownType(typeof(ProceedWithCellSlotActReqData))]
    [KnownType(typeof(CarrierAccesModeReqData))]
    [KnownType(typeof(EquipmentReqData))]

    public class CarrierActReqData 
    {
        [DataMember]
        public EnumCarrierAction ActionType { get; set; }
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

        [DataMember]
        public uint DataID { get; set; }
        [DataMember]
        public string CarrierAction { get; set; }
        [DataMember]
        public string CarrierID { get; set; }
        [DataMember]
        public int PTN { get; set; }
    }

    #region <remarks> ProceedWithCarrierReqData </remarks>

    [DataContract]
    public class ProceedWithCarrierReqData : CarrierActReqData
    {
        [DataMember]
        public string LotID { get; set; }
        [DataMember]
        public string[] CattrID { get; set; }
        [DataMember]
        public string[] CattrData { get; set; }
        [DataMember]
         public string[] SlotMap { get; set; }// slot ocr array
    }

    #endregion

    #region <remarks> ProceedWithSlotReqData </remarks>

    [DataContract]
    public class ProceedWithSlotReqData : CarrierActReqData
    {
        [DataMember]
        public string[] SlotMap { get; set; }
        [DataMember]
        public string Usage { get; set; }
        [DataMember]
        public string[] OcrMap { get; set; }
    }

    #endregion

    #region <remarks> CarrieActReqData </remarks>

    [DataContract]
    public class CarrieActReqData : CarrierActReqData
    {
        [DataMember]
        public string CarrierData { get; set; }
        [DataMember]
        public string LOTID { get; set; }
    }

    #endregion

    #region <remarks> ProceedWithCellSlotActReqData </remarks>

    [DataContract]
    public class ProceedWithCellSlotActReqData : CarrierActReqData
    {
        [DataMember]
        public long SystemByte { get; set; }
        [DataMember]
        public new uint DataID { get; set; }
        [DataMember]
        public new string CarrierAction { get; set; }
        [DataMember]
        public new string CarrierID { get; set; }
        [DataMember]
        public new int PTN { get; set; }
        [DataMember]
        public string LOTID { get; set; }
        [DataMember]
        public string SlotMap { get; set; }
        [DataMember]
        public string CellMap { get; set; }
    }

    #endregion

    #region <remark> CarrierAccesModeReqDara </remarks>
    [DataContract]
    public class CarrierAccesModeReqData : CarrierActReqData
    {
        [DataMember]
        public int AccessMode { get; set; }
        [DataMember]
        public List<int> LoadPortList { get; set; }
    }
    #endregion

    //TODO: 위치 이상하면 옮길 것.
    #region <remark> EquipmentReqData </remarks>
    [DataContract]
    public class EquipmentReqData : CarrierActReqData
    {
        [DataMember]
        public uint[] ECID { get; set; }
        [DataMember]
        public string ECV { get; set; }    
    }
    #endregion

    #endregion
}