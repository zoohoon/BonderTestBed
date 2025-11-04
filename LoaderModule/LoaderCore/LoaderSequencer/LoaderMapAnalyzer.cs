using System;
using System.Collections.Generic;

using LoaderBase;
using LoaderParameters;
using LogModule;
using ProberInterfaces;

namespace LoaderCore
{
    public class LoaderMapAnalyzer : ILoaderMapAnalyzer
    {
        public ILoaderModule Loader { get; set; }

        public LoaderMapAnalyzer(ILoaderModule loader)
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
                foreach (var dstCstInfo in dstMap.CassetteModules)
                {
                    var currCst = Loader.ModuleManager.FindModule<ICassetteModule>(dstCstInfo.ID);

                    if (dstCstInfo.ScanState == CassetteScanStateEnum.READING)
                    {
                        currCst.SetReadingScanState();
                        LoggerManager.Debug($"[LOADER] UpdateCassetteScanState() : Set reading scan state.");

                        ScanCassetteJob sch = new ScanCassetteJob();
                        sch.Init(Loader, currCst);
                        jobList.Add(sch);
                    }
                }

                //analyze trasnfer req
                bool hasScanJob = jobList.Count > 0;
                if (hasScanJob == false)
                {
                    var transferObjects = dstMap.GetTransferObjectAll();

                    foreach (var dstSubObj in transferObjects)
                    {
                        TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(dstSubObj.ID.Value);
                        if(currSubObj==null)
                        {
                            continue;
                        }
                        ModuleID dstPos = dstSubObj.CurrPos;
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

                            if (dstModule is IWaferSupplyModule)
                            {
                                if (dstModule is IFixedTrayModule)
                                {
                                    var module = dstModule as IFixedTrayModule;
                                    if (module.Definition.Enable.Value)
                                    {
                                        UnloadingJob sch = new UnloadingJob();
                                        sch.Init(Loader, currSubObj, dstModule as IWaferSupplyModule);
                                        jobList.Add(sch);
                                    }
                                }
                                else
                                {
                                    UnloadingJob sch = new UnloadingJob();
                                    sch.Init(Loader, currSubObj, dstModule as IWaferSupplyModule);
                                    jobList.Add(sch);
                                }
                            }
                            else if (dstModule is IChuckModule)
                            {
                                LoadingJob sch = new LoadingJob();
                                sch.Init(Loader, currSubObj, dstModule as IChuckModule);
                                jobList.Add(sch);
                            }
                            else
                            {
                                ManualTransferJob sch = new ManualTransferJob();
                                sch.Init(Loader, currSubObj, dstModule as IWaferLocatable);
                                jobList.Add(sch);
                            }
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
