using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using ProberInterfaces;
using System.Xml.Serialization;
using Newtonsoft.Json;
using LogModule;
using LoaderParameters.Data;
using ProberInterfaces.Foup;

namespace LoaderParameters
{
    /// <summary>
    /// Loader의 상태 정보를 정의합니다.
    /// </summary>
    [Serializable]
    [DataContract]
    public class LoaderMap : INotifyPropertyChanged, ICloneable, IParamNode
    {
        [XmlIgnore, JsonIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }

        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }

        public LoaderMap()
        {
            try
            {
                _CassetteModules = new CassetteModuleInfo[0];
                _ARMModules = new HolderModuleInfo[0];
                _PreAlignModules = new HolderModuleInfo[0];
                _FixedTrayModules = new FixedTrayModuleInfo[0];
                _InspectionTrayModules = new HolderModuleInfo[0];
                _ChuckModules = new HolderModuleInfo[0];
                _SemicsOCRModules = new OCRModuleInfo[0];
                _CognexOCRModules = new OCRModuleInfo[0];
                _BufferModules = new HolderModuleInfo[0];
                CCModules = new CardModuleInfo[0];
                CardTrayModules = new CardModuleInfo[0];
                CardBufferModules = new CardModuleInfo[0];
                CardArmModule = new CardModuleInfo[0];
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        /// <summary>
        /// 속성값이 변경되면 발생합니다.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 지정된 속성이 변경되었음을 발생시킵니다.
        /// </summary>
        /// <param name="propertyName">속성 이름</param>
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private CassetteModuleInfo[] _CassetteModules;
        /// <summary>
        /// CassetteModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public CassetteModuleInfo[] CassetteModules
        {
            get { return _CassetteModules; }
            set { _CassetteModules = value; RaisePropertyChanged(); }
        }

        private HolderModuleInfo[] _ARMModules;
        /// <summary>
        /// ARMModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public HolderModuleInfo[] ARMModules
        {
            get { return _ARMModules; }
            set { _ARMModules = value; RaisePropertyChanged(); }
        }

        private HolderModuleInfo[] _PreAlignModules;
        /// <summary>
        /// PreAlignModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public HolderModuleInfo[] PreAlignModules
        {
            get { return _PreAlignModules.Where(x => x.Enable == true).ToArray(); ; }
            set { _PreAlignModules = value; RaisePropertyChanged(); }
        }

        private FixedTrayModuleInfo[] _FixedTrayModules;
        /// <summary>
        /// FixedTrayModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public FixedTrayModuleInfo[] FixedTrayModules
        {
            get { return _FixedTrayModules.Where(x => x.Enable == true).ToArray(); ; }
            set { _FixedTrayModules = value; RaisePropertyChanged(); }
        }

        private HolderModuleInfo[] _InspectionTrayModules;
        /// <summary>
        /// InspectionTrayModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public HolderModuleInfo[] InspectionTrayModules
        {
            get { return _InspectionTrayModules; }
            set { _InspectionTrayModules = value; RaisePropertyChanged(); }
        }

        private HolderModuleInfo[] _ChuckModules;
        /// <summary>
        /// ChuckModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public HolderModuleInfo[] ChuckModules
        {
            get { return _ChuckModules; }
            set { _ChuckModules = value; RaisePropertyChanged(); }
        }

        private OCRModuleInfo[] _SemicsOCRModules;
        /// <summary>
        /// SemicsOCRModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public OCRModuleInfo[] SemicsOCRModules
        {
            get { return _SemicsOCRModules; }
            set { _SemicsOCRModules = value; RaisePropertyChanged(); }
        }

        private OCRModuleInfo[] _CognexOCRModules;
        /// <summary>
        /// CognexOCRModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public OCRModuleInfo[] CognexOCRModules
        {
            get { return _CognexOCRModules; }
            set { _CognexOCRModules = value; RaisePropertyChanged(); }
        }
        private HolderModuleInfo[] _BufferModules;
        /// <summary>
        /// BufferModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public HolderModuleInfo[] BufferModules
        {
            get { return _BufferModules.Where(x => x.Enable == true).ToArray(); ; }
            set { _BufferModules = value; RaisePropertyChanged(); }
        }
        private CardModuleInfo[] _CCModules;
        /// <summary>
        /// CCModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public CardModuleInfo[] CCModules
        {
            get { return _CCModules; }
            set { _CCModules = value; RaisePropertyChanged(); }
        }
        private CardModuleInfo[] _CardTrayModules;
        /// <summary>
        /// CardTrayModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public CardModuleInfo[] CardTrayModules
        {
            get { return _CardTrayModules; }
            set { _CardTrayModules = value; RaisePropertyChanged(); }
        }


        private CardModuleInfo[] _CardBufferModules;
        /// <summary>
        /// CardTrayModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public CardModuleInfo[] CardBufferModules
        {
            get { return _CardBufferModules; }
            set { _CardBufferModules = value; RaisePropertyChanged(); }
        }


        private CardModuleInfo[] _CardArmModule;
        /// <summary>
        /// CardTrayModules 을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public CardModuleInfo[] CardArmModule
        {
            get { return _CardArmModule; }
            set { _CardArmModule = value; RaisePropertyChanged(); }
        }

        private List<int> _ActiveFoupList;
        [DataMember]
        public List<int> ActiveFoupList
        {
            get { return _ActiveFoupList; }
            set { _ActiveFoupList = value; }
        }


        /// <summary>
        /// 모든 TransferObject를 가져옵니다.
        /// </summary>
        /// <returns>Trnasfer Object List</returns>
        public List<TransferObject> GetTransferObjectAll()
        {
            try
            {
                List<TransferObject> list = new List<TransferObject>();

                foreach (var cassette in CassetteModules)
                {
                    foreach (var slot in cassette.SlotModules)
                    {
                        AddSubstrate(slot);
                    }
                }

                foreach (var module in ARMModules)
                {
                    AddSubstrate(module);
                }

                foreach (var module in PreAlignModules)
                {
                    AddSubstrate(module);
                }

                foreach (var module in FixedTrayModules)
                {
                    AddSubstrate(module);
                }

                foreach (var module in InspectionTrayModules)
                {
                    AddSubstrate(module);
                }

                foreach (var module in ChuckModules)
                {
                    AddSubstrate(module);
                }
                foreach (var module in CognexOCRModules)
                {
                    AddSubstrate(module);
                }
                foreach (var module in SemicsOCRModules)
                {
                    AddSubstrate(module);
                }

                foreach (var module in BufferModules)
                {
                    AddSubstrate(module);
                }
                foreach (var module in CCModules)
                {
                    module.WaferType = EnumWaferType.CARD;
                    AddSubstrate(module);
                }
                foreach (var module in CardTrayModules)
                {
                    module.WaferType = EnumWaferType.CARD;
                    AddSubstrate(module);
                }
                foreach (var module in CardBufferModules)
                {
                    module.WaferType = EnumWaferType.CARD;
                    AddSubstrate(module);
                }

                foreach (var module in CardArmModule)
                {
                    module.WaferType = EnumWaferType.CARD;
                    AddSubstrate(module);
                }

                return list;

                void AddSubstrate(HolderModuleInfo moduleInfo)
                {
                    if (moduleInfo.WaferStatus == EnumSubsStatus.EXIST || moduleInfo.WaferStatus == EnumSubsStatus.CARRIER)
                    {
                        list.Add(moduleInfo.Substrate);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        /// <summary>
        /// 모든 TransferObject를 가져옵니다.
        /// </summary>
        /// <returns>Trnasfer Object List</returns>
        public List<TransferObject> GetUnknownTransferObjectAll()
        {
            try
            {
                List<TransferObject> list = new List<TransferObject>();

                foreach (var cassette in CassetteModules)
                {
                    foreach (var slot in cassette.SlotModules)
                    {
                        AddSubstrate(slot);
                    }
                }

                foreach (var module in ARMModules)
                {
                    AddSubstrate(module);
                }

                foreach (var module in PreAlignModules)
                {
                    AddSubstrate(module);
                }

                foreach (var module in FixedTrayModules)
                {
                    AddSubstrate(module);
                }

                foreach (var module in InspectionTrayModules)
                {
                    AddSubstrate(module);
                }

                foreach (var module in ChuckModules)
                {
                    AddSubstrate(module);
                }
                foreach (var module in CognexOCRModules)
                {
                    AddSubstrate(module);
                }
                foreach (var module in SemicsOCRModules)
                {
                    AddSubstrate(module);
                }

                foreach (var module in BufferModules)
                {
                    AddSubstrate(module);
                }
                foreach (var module in CCModules)
                {
                    module.WaferType = EnumWaferType.CARD;
                    AddSubstrate(module);
                }
                foreach (var module in CardTrayModules)
                {
                    module.WaferType = EnumWaferType.CARD;
                    AddSubstrate(module);
                }
                foreach (var module in CardBufferModules)
                {
                    module.WaferType = EnumWaferType.CARD;
                    AddSubstrate(module);
                }

                foreach (var module in CardArmModule)
                {
                    module.WaferType = EnumWaferType.CARD;
                    AddSubstrate(module);
                }

                return list;

                void AddSubstrate(HolderModuleInfo moduleInfo)
                {
                    if (moduleInfo.WaferStatus == EnumSubsStatus.UNKNOWN)
                    {
                        list.Add(moduleInfo.Substrate);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        /// <summary>
        /// 모든 Holder Module을 가져옵니다.
        /// </summary>
        /// <returns>Holder Module List</returns>
        public List<HolderModuleInfo> GetHolderModuleAll()
        {
            List<HolderModuleInfo> list = new List<HolderModuleInfo>();
            try
            {

                foreach (var cassette in CassetteModules)
                {
                    foreach (var slot in cassette.SlotModules)
                    {
                        list.Add(slot);
                    }
                }

                foreach (var module in ARMModules)
                {
                    list.Add(module);
                }

                foreach (var module in PreAlignModules)
                {
                    list.Add(module);
                }

                foreach (var module in FixedTrayModules)
                {
                    list.Add(module);
                }

                foreach (var module in InspectionTrayModules)
                {
                    list.Add(module);
                }

                foreach (var module in ChuckModules)
                {
                    list.Add(module);
                }

                foreach (var module in BufferModules)
                {
                    list.Add(module);
                }
                foreach (var module in CCModules)
                {
                    list.Add(module);
                }
                foreach (var module in CardTrayModules)
                {
                    list.Add(module);
                }
                foreach (var module in CardBufferModules)
                {
                    list.Add(module);
                }
                foreach (var module in CardArmModule)
                {
                    list.Add(module);
                }

                foreach(var module in CognexOCRModules)
                {
                    list.Add(module);
                }
                foreach (var module in SemicsOCRModules)
                {
                    list.Add(module);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return list;
        }

        /// <summary>
        /// Holder Module을 포함한 모든 Location Module을 가져옵니다.
        /// </summary>
        /// <returns>ModuleInfoBase List</returns>
        public List<ModuleInfoBase> GetLocationModules()
        {
            var list = new List<ModuleInfoBase>();
            try
            {

                list.AddRange(GetHolderModuleAll());

                //foreach (var module in CognexOCRModules)
                //{
                //    list.Add(module);
                //}

                //foreach (var module in SemicsOCRModules)
                //{
                //    list.Add(module);
                //}

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return list;
        }

        /// <summary>
        /// 입력된 id에 위치하는 Transfer Object를 가져옵니다.
        /// </summary>
        /// <param name="holderID">Holder Module ID</param>
        /// <returns>Transfer Object</returns>
        public TransferObject GetSubstrateByHolder(ModuleID holderID)
        {
            var subAll = GetTransferObjectAll();

            return subAll.Where(item => item.CurrHolder == holderID).FirstOrDefault();
        }


        /// <summary>
        /// 입력된 guid와 일치하는 Transfer Object를 가져옵니다.
        /// </summary>
        /// <param name="holderID">Holder Module ID</param>
        /// <returns>Transfer Object</returns>
        public TransferObject GetSubstrateByGUID(string guid)
        {
            var subAll = GetTransferObjectAll();

            return subAll.Where(item => item.ID.Value == guid).FirstOrDefault();
        }


        /// <summary>
        /// Polish Type의 Transfer Object를 가져옵니다.
        /// </summary>
        /// <param name="originType">Transfer Object의 할당 위치 타입</param>
        /// <param name="index">Transfer Object의 할당 위치 인덱스</param>
        /// <returns>Polish Transfer Object</returns>
        public TransferObject GetPolish(ModuleTypeEnum originType, int index)
        {
            try
            {
                var found = GetTransferObjectAll().Where(
                item =>
                item.WaferType.Value == EnumWaferType.POLISH &&
                item.OriginHolder.ModuleType == originType &&
                item.OriginHolder.Index == index).FirstOrDefault();
                return found;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        /// <summary>
        /// Wafer를 Unload 가능한 Cassette가 있는지 확인하기 위한 함수
        /// </summary>
        /// <param name="srcTransferObject"></param>
        /// <returns></returns>
        public (bool isExist, CassetteModuleInfo cst, int slotIdx, bool isInCstAlready) IsUnloadCassetteExist(TransferObject srcTransferObject)
        {
            bool isExist = false;
            CassetteModuleInfo cst = null;
            int slotIdx = 0;
            bool isInCstAlready = false;

            var waferInCstAlready = GetTransferObjectAll().Where(m => m.CST_HashCode == srcTransferObject.CST_HashCode &&
                                                                      m.CurrHolder.ModuleType == ModuleTypeEnum.SLOT &&
                                                                      m.WaferState != EnumWaferState.UNPROCESSED 
                                                                      //m.ProcessingEnable == ProcessingEnableEnum.ENABLE
                                                                      ).ToList();//점유한 웨이퍼는 unprocessed로 체크해야함. enable로 체크시 pairwafer 서로다른 foup으로 언로드됨.

            if (waferInCstAlready.Count > 0)// 이미 같은 cst_hashcode의 웨이퍼가 점유한 foup이 있음.
            {
                int foupnum = ((waferInCstAlready.FirstOrDefault().CurrHolder.Index - 1) / 25) + 1;
                var cstModule = CassetteModules[foupnum - 1];
                var holderModuleInfo = cstModule?.SlotModules.FirstOrDefault(m => m.WaferStatus == EnumSubsStatus.NOT_EXIST);

                if (holderModuleInfo != null)
                {
                    isExist = true;
                    cst = cstModule;
                    slotIdx = holderModuleInfo.ID.Index;
                    isInCstAlready = true;
                }
            }
            else
            {
                foreach (var acFoupList in ActiveFoupList)
                {
                    // var Cassette = LoaderMap.FindModule<ICassetteModule>(ModuleTypeEnum.CST, activeLotInfo.FoupNumber);
                    // ==> 아래처럼 바꿈
                    var cstModule = CassetteModules[acFoupList - 1];
                    if(cstModule == null)
                    {

                    }

                    // Cassette가 Load되어있고 Scan 되어있는 상태에서만 Wafer를 Unload 가능하다.
                    if ((cstModule.FoupState != FoupStateEnum.LOAD) || (cstModule.ScanState != CassetteScanStateEnum.READ))
                    {
                        continue;
                    }

                    // Slot 정보를 못 얻어오면 Continue;
                    var slotModule = CassetteModules[acFoupList - 1]?.SlotModules;
                    if (slotModule == null)
                    {
                        continue;
                    }

                    // 비어있는 Slot이 없으면 Continue;
                    var holderModuleInfo = slotModule.FirstOrDefault(m => m.WaferStatus == EnumSubsStatus.NOT_EXIST);
                    if (holderModuleInfo == null)
                    {
                        continue;
                    }

                    // 다른 Hash Code를 가진 Wafer가 CST에 있으면 Continue;
                    bool isOtherHashCodeExist = false;
                    foreach (var slot in slotModule)
                    {
                        if (slot.WaferStatus != EnumSubsStatus.EXIST)
                        {
                            continue;
                        }

                        if (srcTransferObject.CST_HashCode != slot.Substrate.CST_HashCode && slot.Substrate.WaferState != EnumWaferState.UNPROCESSED)
                        {
                            isOtherHashCodeExist = true;
                            break;
                        }
                    }

                    if (isOtherHashCodeExist)
                    {
                        continue;
                    }


                    isExist = true;
                    cst = cstModule;
                    slotIdx = holderModuleInfo.ID.Index;

                    break;
                }
            }



            //foreach (var activeFoupList in ActiveFoupList)
            

            return (isExist, cst, slotIdx, isInCstAlready);
        }

        /// <summary>
        /// Wafer를 Unload 가능한 Holder가 있는지 확인하기 위한 함수
        /// </summary>
        /// <param name="unloadSub"></param>
        /// <returns></returns>
        public (bool isExistUnloaderHolder, ModuleID unloadHolder) IsUnloaderHolderExist(TransferObject unloadSub)
        {
            bool isExistUnloadHolder = true;
            var unloadHolder = unloadSub.OriginHolder;

            if (CassetteModules.FirstOrDefault(x => x.CST_HashCode == unloadSub.CST_HashCode) == null)
            {
                var unloadCST = IsUnloadCassetteExist(unloadSub);
                if (unloadCST.isExist)
                {
                    unloadHolder = unloadCST.cst.SlotModules.FirstOrDefault(x => x.WaferStatus == EnumSubsStatus.NOT_EXIST).ID;
                }
                else
                {
                    var unloadFixedTray = FixedTrayModules.Where(x => x.CanUseBuffer == true && x.WaferStatus != EnumSubsStatus.EXIST).FirstOrDefault();
                    if (unloadFixedTray != null)
                    {
                        unloadHolder = unloadFixedTray.ID;
                    }
                    else
                    {
                        isExistUnloadHolder = false;
                    }
                }
            }

            return (isExistUnloadHolder, unloadHolder);
        }
        
        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public object Clone()
        {
            var shallowClone = MemberwiseClone() as LoaderMap;
            try
            {

                shallowClone.CassetteModules = CassetteModules.CloneFrom();
                shallowClone.ARMModules = ARMModules.CloneFrom();
                shallowClone.PreAlignModules = PreAlignModules.CloneFrom();
                shallowClone.FixedTrayModules = FixedTrayModules.CloneFrom();
                shallowClone.InspectionTrayModules = InspectionTrayModules.CloneFrom();
                shallowClone.ChuckModules = ChuckModules.CloneFrom();
                shallowClone.SemicsOCRModules = SemicsOCRModules.CloneFrom();
                shallowClone.CognexOCRModules = CognexOCRModules.CloneFrom();
                shallowClone.BufferModules = BufferModules.CloneFrom();
                shallowClone.CCModules = CCModules.CloneFrom();
                shallowClone.CardTrayModules = CardTrayModules.CloneFrom();
                shallowClone.CardBufferModules = CardBufferModules.CloneFrom();
                shallowClone.CardArmModule = CardArmModule.CloneFrom();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return shallowClone;
        }

    }

    /// <summary>
    /// Module 기본 정보를 정의합니다.
    /// </summary>
    [Serializable]
    [DataContract]
    public abstract class ModuleInfoBase : INotifyPropertyChanged, ICloneable, IParamNode
    {
        [XmlIgnore, JsonIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }

        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }

        /// <summary>
        /// 속성값이 변경되면 발생합니다.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 지정된 속성이 변경되었음을 발생시킵니다.
        /// </summary>
        /// <param name="propertyName">속성 이름</param>
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private ModuleID _ID = new ModuleID();
        /// <summary>
        /// ModuleID 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ModuleID ID
        {
            get { return _ID; }
            set { _ID = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 현재의 오브젝트를 나타내는 문자열을 가져옵니다.
        /// </summary>
        /// <returns>오브젝트를 나타내는 문자열</returns>
        public override string ToString()
        {
            return $"ID={ID}";
        }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public virtual object Clone()
        {
            try
            {
                var shallowClone = MemberwiseClone() as ModuleInfoBase;
                return shallowClone;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

    /// <summary>
    /// 카세트의 스캔 상태를 정의합니다.
    /// </summary>
    [Serializable]
    [DataContract]
    public enum CassetteScanStateEnum
    {
        /// <summary>
        /// 스캔되지 않은 상태
        /// </summary>
        [EnumMember]
        NONE,
        /// <summary>
        /// 스캔 명령이 요청된 상태
        /// </summary>
        [EnumMember]
        READING,
        /// <summary>
        /// 스캔이 완료된 상태
        /// </summary>
        [EnumMember]
        READ,
        /// <summary>
        /// 스캔정보가 유효하지 않은 상태
        /// </summary>
        [EnumMember]
        ILLEGAL,
        /// <summary>
        /// 스캔을 할 것이라는  상태 
        /// </summary>
        [EnumMember]
        RESERVED

    }

    /// <summary>
    /// 카세트 모듈의 정보를 정의합니다.
    /// </summary>
    [Serializable]
    [DataContract]
    public class CassetteModuleInfo : ModuleInfoBase, IParamNode
    {
        [XmlIgnore, JsonIgnore]
        public new string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public new Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }

        [XmlIgnore, JsonIgnore]
        public new List<object> Nodes { get; set; }

        private CassetteScanStateEnum _ScanState;
        /// <summary>
        /// ScanState 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public CassetteScanStateEnum ScanState
        {
            get { return _ScanState; }
            set { _ScanState = value; RaisePropertyChanged(); }
        }

        private HolderModuleInfo[] _SlotModules;
        /// <summary>
        /// SlotModules 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public HolderModuleInfo[] SlotModules
        {
            get { return _SlotModules; }
            set { _SlotModules = value; RaisePropertyChanged(); }
        }


        private ProberInterfaces.Foup.FoupStateEnum _FoupState;
        /// <summary>
        /// ScanState 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ProberInterfaces.Foup.FoupStateEnum FoupState
        {
            get { return _FoupState; }
            set { _FoupState = value; RaisePropertyChanged(); }
        }

        private ProberInterfaces.Foup.FoupCoverStateEnum _FoupCoverState;
        /// <summary>
        /// 바뀌어야할 FoupCoverState 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ProberInterfaces.Foup.FoupCoverStateEnum FoupCoverState
        {
            get { return _FoupCoverState; }
            set { _FoupCoverState = value; RaisePropertyChanged(); }
        }

        private string _CST_HashCode;
        /// <summary>
        /// ScanState 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public string CST_HashCode
        {
            get { return _CST_HashCode; }
            set { _CST_HashCode = value; RaisePropertyChanged(); }
        }


        private string _FoupID = "";
        [DataMember]
        public string FoupID
        {
            get { return _FoupID; }
            set { _FoupID = value; RaisePropertyChanged(); }
        }

        private LotModeEnum _LotMode = LotModeEnum.UNDEFINED;
        [DataMember]
        public LotModeEnum LotMode
        {
            get { return _LotMode; }
            set { _LotMode = value; RaisePropertyChanged(); }
        }

        private bool _Enable;
        [DataMember]
        public bool Enable
        {
            get { return _Enable; }
            set { _Enable = value; RaisePropertyChanged(); }
        }

        public CassetteModuleInfo()
        {
            _SlotModules = new HolderModuleInfo[0];
        }

        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public override object Clone()
        {
            try
            {
            var shallowClone = MemberwiseClone() as CassetteModuleInfo;
                shallowClone.SlotModules = SlotModules.CloneFrom();
            return shallowClone;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

    /// <summary>
    /// HolderModule의 정보를 정의합니다.
    /// </summary>
    [Serializable]
    [DataContract]
    public class HolderModuleInfo : ModuleInfoBase, IParamNode
    {
        [XmlIgnore, JsonIgnore]
        public new string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public new Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }

        [XmlIgnore, JsonIgnore]
        public new List<object> Nodes { get; set; }

        private EnumSubsStatus _WaferStatus;
        /// <summary>
        /// WaferStatus 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public EnumSubsStatus WaferStatus
        {
            get { return _WaferStatus; }
            set { _WaferStatus = value; RaisePropertyChanged(); }
        }

        private TransferObject _Substrate;
        /// <summary>
        /// TransferObject 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public TransferObject Substrate
        {
            get { return _Substrate; }
            set { _Substrate = value; RaisePropertyChanged(); }
        }
        private ModuleTypeEnum _ModuleType;
        /// <summary>
        /// WaferStatus 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ModuleTypeEnum ModuleType
        {
            get { return _ModuleType; }
            set { _ModuleType = value; RaisePropertyChanged(); }
        }

        private ReservationInfo _ReservationInfo;
       
        [DataMember]
        public ReservationInfo ReservationInfo
        {
            get { return _ReservationInfo; }
            set { _ReservationInfo = value; RaisePropertyChanged(); }
        }
        private bool _Enable=true;
        /// <summary>
        /// WaferStatus 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public bool Enable
        {
            get { return _Enable; }
            set { _Enable = value; RaisePropertyChanged(); }
        }
        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public override object Clone()
        {
            try
            {
            var shallowClone = MemberwiseClone() as HolderModuleInfo;
                shallowClone.Substrate = Substrate != null ? Substrate.Clone() as TransferObject : null;
            return shallowClone;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public static implicit operator EnumSubsStatus(HolderModuleInfo v)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// OCR 모듈의 정보를 정의합니다.
    /// </summary>
    [Serializable]
    [DataContract]
    public class OCRModuleInfo : HolderModuleInfo
    {
        /// <summary>
        /// 오브젝트의 복사본을 가져옵니다.
        /// </summary>
        /// <returns>복사본</returns>
        public override object Clone()
        {
            var shallowClone = MemberwiseClone() as OCRModuleInfo;
            return shallowClone;
        }
    }

    [Serializable]
    [DataContract]
    public class CardModuleInfo : HolderModuleInfo, IParamNode
    {
        [XmlIgnore, JsonIgnore]
        public new string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public new Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }

        [XmlIgnore, JsonIgnore]
        public new List<object> Nodes { get; set; }


        private EnumWaferType _WaferType;
        /// <summary>
        /// WaferStatus 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public EnumWaferType WaferType
        {
            get { return _WaferType; }
            set { _WaferType = value; RaisePropertyChanged(); }
        }
        private bool _Enable = true;
        /// <summary>
        /// WaferStatus 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public new bool Enable
        {
            get { return _Enable; }
            set { _Enable = value; RaisePropertyChanged(); }
        }
    }

    [Serializable]
    [DataContract]
    public class FixedTrayModuleInfo : HolderModuleInfo, IParamNode
    {
        [XmlIgnore, JsonIgnore]
        public new string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public new Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }

        [XmlIgnore, JsonIgnore]
        public new List<object> Nodes { get; set; }


        //[FOUP_SHIFT]
        private bool _CanUseBuffer;
        /// <summary>
        /// Buffer로 사용가능한지 여부를 설정합니다.
        /// </summary>
        [DataMember]
        public bool CanUseBuffer
        {
            get { return _CanUseBuffer; }
            set { _CanUseBuffer = value; RaisePropertyChanged(); }
        }

    }



    [Serializable]
    [DataContract]
    public class WaferSupplyModuleInfo : ModuleInfoBase, IParamNode
    {
        [XmlIgnore, JsonIgnore]
        public new string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public new Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }
        public WaferSupplyModuleInfo()
        {

        }
        public WaferSupplyModuleInfo(ModuleTypeEnum moduleType,int index)
        {
            _ModuleType = moduleType;
            ID = new ModuleID(moduleType, index, null);
        }
        [XmlIgnore, JsonIgnore]
        public new List<object> Nodes { get; set; }


        private ModuleTypeEnum _ModuleType;
        /// <summary>
        /// WaferStatus 를 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public ModuleTypeEnum ModuleType
        {
            get { return _ModuleType; }
            set { _ModuleType = value; RaisePropertyChanged(); }
        }

    }

}
