using LoaderBase;
using LoaderParameters;
using LogModule;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LoaderCore
{
    public class GPLoaderMapAnalyzer : ILoaderMapAnalyzer
    {
        public ILoaderModule Loader { get; set; }

        public GPLoaderMapAnalyzer(ILoaderModule loader)
        {
            try
            {
                this.Loader = loader;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public List<ILoaderJob> Build(LoaderMap dstMap)
        {
            List<ILoaderJob> jobList = new List<ILoaderJob>();

            try
            {
                //analyze scan req
                //foreach (var dstCstInfo in dstMap.CassetteModules)
                //{
                //    var currCst = Loader.ModuleManager.FindModule<ICassetteModule>(dstCstInfo.ID);

                //    if (dstCstInfo.ScanState == CassetteScanStateEnum.READING)
                //    {
                //        currCst.SetReadingScanState();
                //        LoggerManager.Debug($"[LOADER] UpdateCassetteScanState() : Set reading scan state.");

                //        ScanCassetteJob sch = new ScanCassetteJob();
                //        sch.Init(Loader, currCst);
                //        jobList.Add(sch);
                //    }
                //}

                // analyze close foupcover req
                foreach (var dstCstInfo in dstMap.CassetteModules)
                {
                    var currCst = Loader.ModuleManager.FindModule<ICassetteModule>(dstCstInfo.ID);
                    if (Loader.LoaderMaster.GetIsAlwaysCloseFoupCover() == true)
                    {
                        // 이미 close 였는지, open -> close 로 바뀐건지 어떻게 판단 ?
                        if (dstCstInfo.FoupCoverState == ProberInterfaces.Foup.FoupCoverStateEnum.CLOSE &&
                            dstCstInfo.ScanState == CassetteScanStateEnum.READ)
                        {
                            LoggerManager.Debug($"[LOADER] UpdateFoupCoverState() : Set closing state.");
                            CloseFoupCoverScheduler sch = new CloseFoupCoverScheduler();
                            sch.Init(Loader, currCst);
                            jobList.Add(sch);
                        }
                    }
                }

                //analyze trasnfer req
                bool hasScanJob = jobList.Count > 0;
                if (hasScanJob == false)
                {
                    var transferObjects = dstMap.GetTransferObjectAll().Where(i=>i.WaferType.Value!=EnumWaferType.CARD);
                    var cardObject = dstMap.GetTransferObjectAll().Where(i => i.WaferType.Value == EnumWaferType.CARD);
                    //CC
                    foreach (var dstSubObj in transferObjects)
                    {
                        TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(dstSubObj.ID.Value);
                        if (currSubObj == null)
                        {
                            continue;
                        }
                        ModuleID dstPos = dstSubObj.CurrHolder;
                        IWaferLocatable dstModule = Loader.ModuleManager.FindModule(dstPos) as IWaferLocatable;

                        bool hasTrasnferReq = false;

                        bool equalRef = currSubObj.CurrPos == dstSubObj.CurrPos;


                        if (currSubObj.CurrPos != dstPos)
                        {
                            hasTrasnferReq = true;
                        }
                        else
                        {
                            if (dstModule is IOCRReadable)
                            {
                                if (dstSubObj.OverrideOCRDeviceOption.IsEnable.Value ||
                                    dstSubObj.OverrideOCROption.IsEnable.Value)
                                {
                                    hasTrasnferReq = true;
                                }
                            }
                        }

                        if (hasTrasnferReq)
                        {
                            //. 웨이퍼 정보가 없을 때 목적지 정보로 갱신
                            //**** UI 에서 Wanning처리 하도록 !!!
                            DescriptorJob sch = new DescriptorJob();

                            if (currSubObj.Type.Value == SubstrateTypeEnum.UNDEFINED ||
                                currSubObj.Size.Value == SubstrateSizeEnum.UNDEFINED)
                            {
                                if (dstModule is IWaferSupplyModule)
                                {
                                    var wsm = dstModule as IWaferSupplyModule;

                                    var devInfo = wsm.GetSourceDeviceInfo();
                                    currSubObj.SetDeviceInfo(dstModule.ID, devInfo);
                                }
                            }

                            //. Override Load Notch Angle Options
                            if (currSubObj.OverrideLoadNotchAngleOption.IsEnable != dstSubObj.OverrideLoadNotchAngleOption.IsEnable)
                            {
                                currSubObj.OverrideLoadNotchAngleOption = dstSubObj.OverrideLoadNotchAngleOption.Clone() as LoadNotchAngleOption;

                                LoggerManager.Debug($"[LOADER] LoaderJobScheduleModue.UpdateSubstrateState() : Override loadNotchAngle has been updated. holder={currSubObj.CurrHolder}, Angle={currSubObj.OverrideLoadNotchAngleOption.Angle}");
                            }

                            //. Override OCR Option
                            if (dstSubObj.OverrideOCROption.IsEnable.Value)
                            {
                                currSubObj.OverrideOCROption = dstSubObj.OverrideOCROption.Clone() as OCRPerformOption;

                                if (currSubObj.OverrideOCROption.IsPerform.Value)
                                {
                                    currSubObj.CleanOCRState();
                                }

                                LoggerManager.Debug($"[LOADER] LoaderJobScheduleModue.UpdateSubstrateState() : Override ocr option has been updated. holder={currSubObj.CurrHolder}, IsPerformProcessing={currSubObj.OverrideOCROption.IsPerform}");
                            }

                            //. Override OCR Device Option
                            if (dstSubObj.OverrideOCRDeviceOption.IsEnable.Value)
                            {
                                currSubObj.OverrideOCRDeviceOption = dstSubObj.OverrideOCRDeviceOption.Clone() as OCRDeviceOption;

                                LoggerManager.Debug($"[LOADER] LoaderJobScheduleModue.UpdateSubstrateState() : Override OCRDevice option has been updated. holder={currSubObj.CurrHolder}");
                            }


                            sch.Init(Loader, currSubObj, dstModule as IWaferLocatable);

                          
                            jobList.Add(sch);
                        }
                    }

                    foreach (var dstSubObj in cardObject)
                    {
                        TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(dstSubObj.ID.Value);
                        if (currSubObj == null)
                        {
                            continue;
                        }
                        ModuleID dstPos = dstSubObj.CurrHolder;
                        ICardLocatable dstModule = Loader.ModuleManager.FindModule(dstPos) as ICardLocatable;

                        ICardARMModule CardArmModule = Loader.ModuleManager.FindModule(ModuleTypeEnum.CARDARM, 1) as ICardARMModule;
                        if (CardArmModule != null)
                        {
                            if (CardArmModule.Holder.Status == EnumSubsStatus.CARRIER && (dstPos.ModuleType==ModuleTypeEnum.CARDBUFFER|| dstPos.ModuleType == ModuleTypeEnum.CARDTRAY))
                            {
                                currSubObj = CardArmModule.Holder.TransferObject;
                            }
                        }

                        bool hasTrasnferReq = false;

                        bool equalRef = currSubObj.CurrPos == dstSubObj.CurrPos;


                        if (currSubObj.CurrPos != dstPos)
                        {
                            hasTrasnferReq = true;
                        }
                        else
                        {
                            if (dstModule is IOCRReadable)
                            {
                                if (dstSubObj.OverrideOCRDeviceOption.IsEnable.Value ||
                                    dstSubObj.OverrideOCROption.IsEnable.Value)
                                {
                                    hasTrasnferReq = true;
                                }
                            }
                        }

                        if (hasTrasnferReq)
                        {
                            //. 웨이퍼 정보가 없을 때 목적지 정보로 갱신
                            //**** UI 에서 Wanning처리 하도록 !!!
                            DescriptorJob sch = new DescriptorJob();
                           
                             sch.Init(Loader, currSubObj, dstModule as ICardLocatable);
                            jobList.Add(sch);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return jobList;
        }
    }
}
