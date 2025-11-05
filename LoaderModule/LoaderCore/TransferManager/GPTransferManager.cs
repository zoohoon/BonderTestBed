namespace LoaderCore.TransferManager
{
    using Autofac;
    using LoaderBase;
    using LoaderBase.Communication;
    using LoaderMapView;
    using LoaderParameters;
    using LoaderParameters.Data;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Foup;
    using ProberInterfaces.Loader;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class GPTransferManager : ITransferManager, IFactoryModule, ILoaderFactoryModule
    {
        public InitPriorityEnum InitPriority { get; set; }
        private ILoaderModule loaderModule => this.GetLoaderContainer().Resolve<ILoaderModule>();

        #region <!-- ILoaderFactoryModule Method -->

        public EventCodeEnum InitModule(Autofac.IContainer container)
        {
            return EventCodeEnum.NONE;
        }

        public void DeInitModule()
        {
            return;
        }

        #endregion

        /// <summary>
        /// 같은 Origin Position 을 가진 Object 가 Map 에 존재한다면 Transfer 할 수 없음.
        /// 옮기는 Source 가 PW 이거나, Target 위치가 PW 로 설정되어 있는 경우 PW Source Type 이 일치 해야 함.
        /// </summary>
        public EventCodeEnum ValidationTransferToTray(object sourceObj, object targetobj, ref string failReason)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                object transfersourcemodule = null;
                object transfertargetmodule = null;
                ModuleID sourceCurHolderID = new ModuleID();
                ModuleID soruceId = new ModuleID();
                ModuleID targetId = new ModuleID();
                TransferObject sourceTransferObj = null;

                // Target 이 FixedTray 인 경우에만 해당함.
                if (targetobj is FixedTrayInfoObject || targetobj is InspectionTrayInfoObject)
                {
                    LoaderMap Map = loaderModule.GetLoaderInfo().StateMap;

                    SetTransferInfoToModule(sourceObj, ref transfersourcemodule, true);
                    if (transfersourcemodule != null)
                    {
                        string sourceidstr = (string)transfersourcemodule;
                        sourceTransferObj = Map.GetTransferObjectAll().Where(item => item.ID.Value == sourceidstr).FirstOrDefault();
                        soruceId = (ModuleID)sourceTransferObj.OriginHolder;

                        SetTransferInfoToModule(sourceObj, ref transfersourcemodule, true, true);
                        sourceCurHolderID = (ModuleID)transfersourcemodule;
                    }

                    SetTransferInfoToModule(targetobj, ref transfertargetmodule, false);
                    if (transfertargetmodule != null)
                    {
                        targetId = (ModuleID)transfertargetmodule;
                    }

                    if (transfersourcemodule != null && transfertargetmodule != null)
                    {
                        /// 같은 Origin Position 을 가진 Object 가 Map 에 존재한다면 Transfer 할 수 없음.
                        var transferObjs = Map.GetHolderModuleAll();
                        transferObjs = transferObjs.FindAll(obj => obj.ID.ModuleType == ModuleTypeEnum.CHUCK
                          || obj.ID.ModuleType == ModuleTypeEnum.BUFFER
                          || obj.ID.ModuleType == ModuleTypeEnum.PA
                          || obj.ID.ModuleType == ModuleTypeEnum.ARM
                          || obj.ID.ModuleType == ModuleTypeEnum.INSPECTIONTRAY
                          || obj.ID.ModuleType == ModuleTypeEnum.FIXEDTRAY);

                        var sameOriginTransferObjs = transferObjs.FindAll(obj => obj.Substrate != null && (obj.Substrate.OriginHolder.Label?.Equals(targetId.Label) ?? false));
                        /// Target 위치의 Object 가 Map 에 있으면 Count 가 0 이상임.
                        if (sameOriginTransferObjs.Count != 0)
                        {
                            /// 같은 Origin 은 하나만 있다는 가정으로 first 를 사용
                            var transferobj = sameOriginTransferObjs.First();

                            if ((soruceId.ModuleType != targetId.ModuleType) || (soruceId.Index != targetId.Index))
                            {
                                /// Origin 위치와 다른 위치로 이동하는 경우
                                if (transferobj.ID != targetId)
                                {
                                    retVal = EventCodeEnum.TRASNFER_TARGET_INVALID;
                                    failReason = $"ErrorCode : {retVal}. Cannot operate because an object with the same origin position exists. \n Position : {transferobj.ID.Label}";
                                }
                                else
                                {
                                    retVal = EventCodeEnum.NONE;
                                }
                            }
                            else
                            {
                                /// Origin 위치와 같은 위치로 이동하는 경우에는 Object 가 Source 와 같은 위치에 있지 않은 경우에만 에러 
                                if (transferobj.ID != sourceCurHolderID)
                                {
                                    retVal = EventCodeEnum.TRASNFER_TARGET_INVALID;
                                    failReason = $"ErrorCode : {retVal}. Cannot operate because an object with the same origin position exists. \n Position : {transferobj.ID.Label}";
                                }
                                else
                                {
                                    retVal = EventCodeEnum.NONE;
                                }
                            }
                        }
                        else
                        {
                            retVal = EventCodeEnum.NONE;
                        }

                        if (retVal == EventCodeEnum.NONE)
                        {
                            /// Polish Wafer Type 이 다를 경우 Transfer 할 수 없음.
                            var targetPWInfo = GetPolishWaferSourceInfo(targetId);
                            if (targetPWInfo != null)
                            {
                                if (soruceId != targetId)
                                {
                                    if (sourceTransferObj.PolishWaferInfo != null) 
                                    {
                                        string sourcePWName = sourceTransferObj.PolishWaferInfo.DefineName.Value;
                                        string targetPWName = targetPWInfo.DefineName.Value;

                                        if (string.IsNullOrEmpty(sourcePWName) == false && string.IsNullOrEmpty(targetPWName) == false)
                                        {
                                            if (sourcePWName.Equals(targetPWName) == false)
                                            {
                                                retVal = EventCodeEnum.TRASNFER_TARGET_INVALID;
                                                failReason = $"ErrorCode : {retVal}. Can't to transfer because the source of the polish wafer is different." +
                                                    $"\n[Source PW Name] : {sourcePWName}" +
                                                    $"\n[Target PW Name] : {targetPWName}";
                                            }
                                        }
                                    }

                                    SubstrateSizeEnum sourcePWSize = sourceTransferObj.Size.Value;
                                    SubstrateSizeEnum targetPWSize = targetPWInfo.Size.Value;

                                    if (sourcePWSize != targetPWSize)
                                    {
                                        retVal = EventCodeEnum.TRASNFER_TARGET_INVALID;
                                        failReason = $"ErrorCode : {retVal}. Can't to transfer because the source of the polish wafer is different." +
                                            $"\n[Source Wafer Size] : {sourcePWSize}" +
                                            $"\n[Target Wafer Size] : {targetPWSize}";
                                    }

                                    //WaferNotchTypeEnum sourcePWNotchType = sourceTransferObj.NotchType.Value;
                                    //WaferNotchTypeEnum targetPWNotchType = targetPWInfo.NotchType.Value;

                                    //if (sourcePWNotchType != targetPWNotchType)
                                    //{
                                    //    retVal = EventCodeEnum.TRASNFER_TARGET_INVALID;
                                    //    failReason = $"ErrorCode : {retVal}. Can't to transfer because the source of the polish wafer is different." +
                                    //        $"\n[Source PW NotchType] : {sourcePWNotchType}" +
                                    //        $"\n[Target PW NotchType] : {targetPWNotchType}";
                                    //}
                                }
                            }
                        }
                    }
                    else
                    {
                        retVal = EventCodeEnum.NONE;
                    }
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.TRASNFER_TARGET_INVALID;
            }
            return retVal;
        }

        private void SetTransferInfoToModule(object param, ref object Id, bool issource , bool issourceholder = false)
        {
            try
            {
                LoaderMap Map = loaderModule.GetLoaderInfo().StateMap;

                if (param is StageObject)
                {
                    int chuckindex = Convert.ToInt32(((param as StageObject).Name.Split('#'))[1]);
                    HolderModuleInfo chuckModule = null;
                    if (issource || issourceholder)
                        chuckModule = Map.ChuckModules.FirstOrDefault(i => i.Substrate != null && i.ID.Index == chuckindex);
                    else
                        chuckModule = Map.ChuckModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index == chuckindex);

                    if (chuckModule == null)
                    {
                        Id = null;
                        return;
                    }
                    if (issource && !issourceholder)
                        Id = chuckModule.Substrate.ID.Value;
                    else
                        Id = chuckModule.ID;

                }
                else if (param is SlotObject)
                {
                    int slotindex = Convert.ToInt32(((param as SlotObject).Name.Split('#'))[1]);
                    var waferIdx = (((param as SlotObject).FoupNumber) * 25) + slotindex;
                    if (Map.CassetteModules[(param as SlotObject).FoupNumber].FoupState == FoupStateEnum.LOAD && Map.CassetteModules[(param as SlotObject).FoupNumber].ScanState == CassetteScanStateEnum.READ)
                    {
                        var slotModule = Map.CassetteModules[(param as SlotObject).FoupNumber].SlotModules.FirstOrDefault(i => i.ID.Index == waferIdx);
                        if (slotModule == null)
                            return;
                        if (issource && !issourceholder)
                            Id = slotModule.Substrate.ID.Value;
                        else
                            Id = slotModule.ID;
                    }
                    else
                    {
                        Id = null;
                        return;
                    }

                }
                else if (param is ArmObject)
                {
                    int armindex = Convert.ToInt32(((param as ArmObject).Name.Split('#'))[1]);
                    HolderModuleInfo armmodule = null;
                    if (issource || issourceholder)
                        armmodule = Map.ARMModules.FirstOrDefault(i => i.Substrate != null && i.ID.Index == armindex);
                    else
                        armmodule = Map.ARMModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index == armindex);

                    if (armmodule == null)
                    {
                        Id = null;
                        return;
                    }
                    if (issource && !issourceholder)
                        Id = armmodule.Substrate.ID.Value;
                    else
                        Id = armmodule.ID;
                }
                else if (param is PAObject)
                {
                    int paindex = Convert.ToInt32(((param as PAObject).Name.Split('#'))[1]);
                    HolderModuleInfo pamodule = null;
                    if (issource || issourceholder)
                        pamodule = Map.PreAlignModules.FirstOrDefault(i => i.Substrate != null && i.ID.Index == paindex);
                    else
                        pamodule = Map.PreAlignModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index == paindex);

                    if (pamodule == null)
                    {
                        Id = null;
                        return;
                    }
                    if (issource && !issourceholder)
                        Id = pamodule.Substrate.ID.Value;
                    else
                        Id = pamodule.ID;
                }
                else if (param is BufferObject)
                {
                    int bufferindex = Convert.ToInt32(((param as BufferObject).Name.Split('#'))[1]);
                    HolderModuleInfo buffermodule = null;
                    if (issource || issourceholder)
                        buffermodule = Map.BufferModules.FirstOrDefault(i => i.Substrate != null && i.ID.Index == bufferindex);
                    else
                        buffermodule = Map.BufferModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index == bufferindex);

                    if (buffermodule == null)
                    {
                        Id = null;
                        return;
                    }
                    if (issource && !issourceholder)
                        Id = buffermodule.Substrate.ID.Value;
                    else
                        Id = buffermodule.ID;
                }
                else if (param is CardTrayObject)
                {
                    int cardtaryindex = Convert.ToInt32(((param as CardTrayObject).Name.Split('#'))[1]);
                    HolderModuleInfo cardtraymodule = null;
                    if (issource || issourceholder)
                        cardtraymodule = Map.CardTrayModules.FirstOrDefault(i => i.Substrate != null && i.ID.Index == cardtaryindex);
                    else
                    {
                        cardtraymodule = Map.CardTrayModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index == cardtaryindex);
                        if (cardtraymodule == null)
                        {
                            cardtraymodule = Map.CardTrayModules.FirstOrDefault(i => i.Substrate != null && i.WaferStatus == EnumSubsStatus.CARRIER && i.ID.Index == cardtaryindex);
                        }
                    }
                    if (cardtraymodule == null)
                    {
                        Id = null;
                        return;
                    }
                    if (issource && !issourceholder)
                        Id = cardtraymodule.Substrate.ID.Value;
                    else
                        Id = cardtraymodule.ID;
                }
                else if (param is CardBufferObject)
                {
                    int cardbufindex = Convert.ToInt32(((param as CardBufferObject).Name.Split('#'))[1]);
                    HolderModuleInfo cardtraymodule = null;
                    if (issource || issourceholder)
                        cardtraymodule = Map.CardBufferModules.FirstOrDefault(i => i.Substrate != null && i.ID.Index == cardbufindex);
                    else
                    {
                        cardtraymodule = Map.CardBufferModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index == cardbufindex);
                        if (cardtraymodule == null)
                        {
                            cardtraymodule = Map.CardBufferModules.FirstOrDefault(i => i.Substrate != null && i.WaferStatus == EnumSubsStatus.CARRIER && i.ID.Index == cardbufindex);
                        }
                    }

                    if (cardtraymodule == null)
                        return;
                    if (issource && !issourceholder)
                        Id = cardtraymodule.Substrate.ID.Value;
                    else
                        Id = cardtraymodule.ID;
                }
                else if (param is CardArmObject)
                {
                    int cardarmindex = Convert.ToInt32(((param as CardArmObject).Name.Split('#'))[1]);
                    HolderModuleInfo cardarmmodule = null;
                    //Change Map.CardBufferModules => CardAramModule 
                    if (issource || issourceholder)
                        cardarmmodule = Map.CardArmModule.FirstOrDefault(i => i.Substrate != null && i.ID.Index == cardarmindex);
                    else
                        cardarmmodule = Map.CardArmModule.FirstOrDefault(i => i.Substrate == null && i.ID.Index == cardarmindex);

                    if (cardarmmodule == null)
                    {
                        Id = null;
                        return;
                    }
                    if (issource && !issourceholder)
                        Id = cardarmmodule.Substrate.ID.Value;
                    else
                        Id = cardarmmodule.ID;
                }
                else if (param is FixedTrayInfoObject)
                {
                    int fixedtrayindex = Convert.ToInt32(((param as FixedTrayInfoObject).Name.Split('#'))[1]);
                    HolderModuleInfo fixedTrayModule = null;
                    //Change Map.CardBufferModules => CardAramModule 
                    if (issource || issourceholder)
                        fixedTrayModule = Map.FixedTrayModules.FirstOrDefault(i => i.Substrate != null && i.ID.Index == fixedtrayindex);
                    else
                        fixedTrayModule = Map.FixedTrayModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index == fixedtrayindex);

                    if (fixedTrayModule == null)
                    {
                        Id = null;
                        return;
                    }
                    if (issource && !issourceholder)
                        Id = fixedTrayModule.Substrate.ID.Value;
                    else
                        Id = fixedTrayModule.ID;
                }
                else if (param is InspectionTrayInfoObject)
                {
                    int iNSPtrayindex = Convert.ToInt32(((param as InspectionTrayInfoObject).Name.Split('#'))[1]);
                    HolderModuleInfo iNSPTrayModule = null;
                    //Change Map.CardBufferModules => CardAramModule 
                    if (issource || issourceholder)
                        iNSPTrayModule = Map.InspectionTrayModules.FirstOrDefault(i => i.Substrate != null && i.ID.Index == iNSPtrayindex);
                    else
                        iNSPTrayModule = Map.InspectionTrayModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index == iNSPtrayindex);

                    if (iNSPTrayModule == null)
                    {
                        Id = null;
                        return;
                    }
                    if (issource && !issourceholder)
                        Id = iNSPTrayModule.Substrate.ID.Value;
                    else
                        Id = iNSPTrayModule.ID;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public PolishWaferInformation GetPolishWaferSourceInfo(ModuleID targetId)
        {
            PolishWaferInformation pwInformation = null;
            try
            {
                DeviceManagerParameter DMParam = this.DeviceManager()?.DeviceManagerParamerer_IParam as DeviceManagerParameter;
                if (DMParam != null)
                {
                    var deviceInfo = DMParam.DeviceMappingInfos.SingleOrDefault(info => info.WaferSupplyInfo.ID == targetId);
                    if(deviceInfo != null)
                    {
                        pwInformation = deviceInfo.DeviceInfo.PolishWaferInfo;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return pwInformation;
        }

    }
}
