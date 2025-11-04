using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using Autofac;
using LoaderBase;
using LoaderParameters;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;

namespace LoaderCore
{
    public class LoaderMapSlicer : ILoaderMapSlicer, IFactoryModule
    {        
        public IContainer Container => this.GetContainer();
        public ILoaderModule Loader => Container.Resolve<ILoaderModule>();
        public InitPriorityEnum InitPriority => InitPriorityEnum.LEVEL3;

        public void DeInitModule()
        {
        }

        public EventCodeEnum InitModule(IContainer container)
        {
            return EventCodeEnum.NONE;
        }

        public bool isLotPause { get; set; } = false;

        private List<LoaderJobViewData> loaderJobList = new List<LoaderJobViewData>();
        public List<LoaderMap> Slicing(LoaderMap loaderMap)
        {
            LoggerManager.InitCheckDuplicateMap();

            List<LoaderMap> maps = new List<LoaderMap>();
            LoaderMap prevMap = new LoaderMap();
            List<CassetteModuleInfo> scanTargets = new List<CassetteModuleInfo>();
            isLotPause = false;

            try
            {
                if (loaderMap == null)
                {
                    return maps;
                }

                loaderJobList.Clear();

                var transferObjects = loaderMap.GetTransferObjectAll();
                transferObjects = transferObjects.OrderBy(i => i.Priority).ThenBy(item => item.OriginHolder.Index).ToList();
                Dictionary<string, List<ModuleTypeEnum>> paths = new Dictionary<string, List<ModuleTypeEnum>>();

                var transferPaths = new Dictionary<string, Queue<ModuleTypeEnum>>();
                var transferObjectIDs = new List<string>();
                var bufferedObjectIDs = new List<string>();
                var historyBufferedObjectIDs = new List<string>();
                var beSwappedObjectIDs = new List<string>();
                var transferredObjectIDs = new List<string>();
                var beSwappedObjectID = "";
                bool unBufferedTransfer = false;

                int toCount = 0;
                var buffTransfer = false;

                string unloadchuck = string.Empty;
                TransferObject backupObject = null;
                int backuptoCount = 0;
                bool backupbufftransfer = false;
                ModuleTypeEnum LoadMapSrcType = ModuleTypeEnum.UNDEFINED;
                List<TransferObject> realTransfers = new List<TransferObject>();

                foreach (var dstSubObj in transferObjects)
                {
                    TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(dstSubObj.ID.Value);

                    if (currSubObj == null)
                    {
                        continue;
                    }

                    ModuleID dstPos = dstSubObj.CurrPos;
                    IWaferLocatable dstModule = Loader.ModuleManager.FindModule(dstPos) as IWaferLocatable;

                    bool equalRef = currSubObj.CurrPos == dstSubObj.CurrPos;

                    if (dstPos.ModuleType == ModuleTypeEnum.CHUCK)
                    {
                        string loadchuck = $"{dstSubObj.CurrHolder.ModuleType}{dstSubObj.CurrHolder.Index}";
                        if (unloadchuck.Equals(loadchuck))
                        {
                            if (LoadMapSrcType != ModuleTypeEnum.UNDEFINED) //두반째 exchange map 생성이 필요한 경우 stop 후 다음 tick에 처리하도록 한다.
                            {
                                LoggerManager.LoaderMapLog($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} : There are already other stage exchanges. re-generate map(only one exchange)");
                                //map하나 지워야함
                                realTransfers.Remove(backupObject);
                                toCount = backuptoCount;
                                buffTransfer = backupbufftransfer;
                                break;
                            }
                            LoadMapSrcType = currSubObj.CurrPos.ModuleType; //exchage src holder 임시 저장, 다른 holder type의 exchage map이 같이 생성되지 않기 위함
                        }
                    }
                    unloadchuck = string.Empty;
                    realTransfers.Add(dstSubObj);
                    backupObject = dstSubObj;
                    if (currSubObj.CurrHolder.ModuleType == ModuleTypeEnum.CHUCK) //unload
                    {
                        unloadchuck = $"{currSubObj.CurrHolder.ModuleType}{currSubObj.CurrHolder.Index}";
                    }

                    if (currSubObj.CurrPos != dstPos)
                    {
                        backuptoCount = toCount;
                        toCount++;

                        if (dstModule is IBufferModule)
                        {
                            backupbufftransfer = buffTransfer;
                            buffTransfer = true;
                        }
                    }
                    else
                    {
                        if (dstModule is IOCRReadable)
                        {
                            if (dstSubObj.OverrideOCRDeviceOption.IsEnable.Value ||
                                dstSubObj.OverrideOCROption.IsEnable.Value)
                            {
                                backuptoCount = toCount;
                                toCount++;
                            }
                        }
                    }
                }

                var currAvaArmCount = Loader.GetLoaderInfo().StateMap.GetHolderModuleAll().Count(
                    h => h.ID.ModuleType == ModuleTypeEnum.ARM & h.WaferStatus == EnumSubsStatus.NOT_EXIST);

                foreach (var transferObj in realTransfers)
                {
                    TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(transferObj.ID.Value);
                    var dstSubObj = (TransferObject)transferObj.Clone();

                    if (currSubObj == null)
                    {
                        continue;
                    }
                    ModuleID dstPos = dstSubObj.CurrPos;
                    IWaferLocatable dstModule = Loader.ModuleManager.FindModule(dstPos) as IWaferLocatable;

                    //bool hasTrasnferReq = false;

                    bool equalRef = currSubObj.CurrPos == dstSubObj.CurrPos;
                    if (currSubObj.CurrPos != dstPos)
                    {
                        //hasTrasnferReq = true;
                        IModulePathGenerator pathGenerator = null;

                        dstSubObj.NotchAngle.Value = dstSubObj.ChuckNotchAngle.Value;
                        if (dstPos.ModuleType == ModuleTypeEnum.SLOT)
                        {
                            dstSubObj.NotchAngle.Value = dstSubObj.SlotNotchAngle.Value;
                            dstSubObj.OCRReadState = ProberInterfaces.Enum.OCRReadStateEnum.DONE;
                        }

                        if (currSubObj.CurrPos.ModuleType == ModuleTypeEnum.ARM)
                        {
                            unBufferedTransfer = true;
                            pathGenerator = new ModuleUnBuffuredPathGenerator(currSubObj, Loader);
                        }
                        else if (currAvaArmCount >= toCount & buffTransfer == false)
                        {
                            unBufferedTransfer = true;
                            pathGenerator = new ModuleUnBuffuredPathGenerator(currSubObj, Loader);
                        }
                        else
                        {
                            var buffCnt = Loader.GetLoaderInfo().StateMap.BufferModules?.Count();
                            bool isEnableBuffer = false;
                            if (buffCnt > 0)
                            {
                                for (int i = 0; i < Loader.GetLoaderInfo().StateMap.BufferModules.Count(); i++)
                                {
                                    isEnableBuffer = Loader.GetLoaderInfo().StateMap.BufferModules[i].Enable;
                                    if (isEnableBuffer == true)
                                    {
                                        break;
                                    }
                                }
                            }
                            unBufferedTransfer = !isEnableBuffer;

                            if (buffCnt == 0 || unBufferedTransfer)
                            {
                                pathGenerator = new ModuleUnBuffuredPathGenerator(currSubObj, Loader);
                            }
                            else
                            {
                                pathGenerator = new ModulePathGenerator(currSubObj);
                            }
                        }

                        paths.Add(currSubObj.ID.Value, pathGenerator.GetPath(dstSubObj));                        
                    }
                    else
                    {
                        if (dstModule is IOCRReadable)
                        {
                            if (dstSubObj.OverrideOCRDeviceOption.IsEnable.Value ||
                                dstSubObj.OverrideOCROption.IsEnable.Value)
                            {
                                //hasTrasnferReq = true;
                            }
                        }
                    }
                }

                var buffers = Loader.ModuleManager.FindModules<IBufferModule>();
                var handlingArms = Loader.ModuleManager.FindModules<IARMModule>();
                var bufferQueue = new Queue<IBufferModule>();

                for (int i = 0; i < buffers.Count; i++)
                {
                    bufferQueue.Enqueue(buffers[i]);
                }



                // Create path queue.
                foreach (var path in paths)
                {
                    var pathQueue = new Queue<ModuleTypeEnum>();
                    if(path.Value==null)
                    {
                        return null;
                    }

                    foreach (var item in path.Value)
                    {
                        pathQueue.Enqueue(item);
                    }
                    transferPaths.Add(path.Key, pathQueue);
                    transferObjectIDs.Add(path.Key);
                }
                bool toRemain = true;
                var currLoaderMap = (LoaderMap)Loader.GetLoaderInfo().StateMap.Clone();
                var originalMap = (LoaderMap)Loader.GetLoaderInfo().StateMap.Clone();
                bool bufferFullLoaded = false;
                List<string> pendingPaths = new List<string>();
                int avaArmCount = 0;
                int avaPACount = 0;

                bool loadWaferFullBuffered = false;
                int loopCount = 0; //루프 카운트가 계속 증가하면 무한 루프로 빠지기 때문에 다시 새로운 잡을 받아야한다.
                while (toRemain)
                {
                    if (isLotPause)
                    {
                        LoggerManager.Debug($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} : LoaderJobViewList.Clear. isLotPause : {isLotPause}");
                        Loader.LoaderJobViewList.Clear();
                        break;
                    }
                    loadWaferFullBuffered = false;
                    var availableHolder = currLoaderMap.GetHolderModuleAll();
                    foreach (var item in availableHolder)
                    {
                        if (item.WaferStatus == EnumSubsStatus.UNDEFINED)
                        {
                            item.WaferStatus = EnumSubsStatus.NOT_EXIST;
                        }
                    }
                    Queue<ModuleTypeEnum> hopQueue;
                    hopQueue = new Queue<ModuleTypeEnum>();
                    if (transferObjectIDs.Count == 0)
                    {
                        bool isLoadingPathRemained = false;
                        if (pendingPaths.Count > 0)
                        {


                            foreach (var item in pendingPaths)
                            {
                                TransferObject destTO = loaderMap.GetSubstrateByGUID(item);
                                if (destTO.CurrHolder.ModuleType == ModuleTypeEnum.CHUCK)
                                {
                                    isLoadingPathRemained = true;
                                }
                            }
                            if (!isLoadingPathRemained && bufferedObjectIDs.Count > 0)
                            {
                                foreach (var item in bufferedObjectIDs)
                                {
                                    TransferObject destTO = loaderMap.GetSubstrateByGUID(item);
                                    if (destTO.CurrHolder.ModuleType == ModuleTypeEnum.CHUCK)
                                    {
                                        transferObjectIDs.Add(item);
                                        historyBufferedObjectIDs.Add(item);
                                        loadWaferFullBuffered = true;
                                    }

                                }
                                foreach (var item in transferObjectIDs)
                                {
                                    bufferedObjectIDs.Remove(item);
                                }
                                if (transferObjectIDs.Count == 0)
                                {
                                    foreach (var item in pendingPaths)
                                    {
                                        transferObjectIDs.Add(item);
                                    }

                                    foreach (var item in transferObjectIDs)
                                    {
                                        pendingPaths.Remove(item);
                                    }
                                }

                            }
                            else
                            {
                                foreach (var item in pendingPaths)
                                {
                                    // Recover pended objects for futher path
                                    TransferObject destTO = loaderMap.GetSubstrateByGUID(item);
                                    if (isLoadingPathRemained == true)
                                    {
                                        if (destTO.CurrHolder.ModuleType == ModuleTypeEnum.CHUCK)
                                        {
                                            transferObjectIDs.Add(item);
                                        }
                                    }
                                    else
                                    {
                                        transferObjectIDs.Add(item);
                                    }

                                }


                                foreach (var item in transferObjectIDs)
                                {
                                    pendingPaths.Remove(item);
                                }
                            }

                        }
                        else if (beSwappedObjectIDs.Count > 0)
                        {
                            foreach (var item in beSwappedObjectIDs)
                            {
                                // Recover buffered objects for futher path
                                transferObjectIDs.Add(item);
                            }
                            foreach (var item in transferObjectIDs)
                            {
                                beSwappedObjectIDs.Remove(item);
                            }
                        }
                        else if (bufferedObjectIDs.Count > 0)
                        {
                            foreach (var item in bufferedObjectIDs)
                            {
                                // Recover buffered objects for futher path
                                transferObjectIDs.Add(item);
                            }
                            //foreach (var item in transferObjectIDs)
                            //{
                            //    bufferedObjectIDs.Remove(item);
                            //}
                            bufferFullLoaded = true;
                        }
                        else
                        {
                            //LoggerManager.LoaderMapLog($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} : Completed. (transferObjectIDs Count zero)", basicLog: false);
                            toRemain = false;
                        }
                    }
                    // Search be swapped objects.
                    else
                    {
                        // Source & target compare method
                        var searchMap = (LoaderMap)currLoaderMap.Clone();
                        var demandMap = (LoaderMap)loaderMap.Clone();
                        foreach (var to in transferObjectIDs)
                        {
                            TransferObject currSubObj = demandMap.GetSubstrateByGUID(to);

                            var targetHolder = searchMap.GetHolderModuleAll().FirstOrDefault(h => h.ID == currSubObj.CurrHolder);
                            if (targetHolder.Substrate != null && targetHolder.ID.ModuleType == ModuleTypeEnum.CHUCK)
                            {
                                if (targetHolder.WaferStatus == EnumSubsStatus.EXIST & targetHolder.Substrate.ID.Value != currSubObj.ID.Value)
                                {
                                    var subsToBeSwapped = searchMap.GetHolderModuleAll().FirstOrDefault(h => h.ID == currSubObj.CurrHolder);
                                    if (beSwappedObjectIDs.Exists(o => o == subsToBeSwapped.Substrate.ID.Value) == false)
                                    {
                                        beSwappedObjectIDs.Add(subsToBeSwapped.Substrate.ID.Value);
                                    }
                                    LoggerManager.Debug($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} : Substrate on {targetHolder.ID.Label} will be swapped with {currSubObj.OriginHolder.Label} substrate.", isLoaderMap: true);
                                }
                            }
                        }

                        // Remove be swapped objects from transfer objects.
                        foreach (var item in beSwappedObjectIDs)
                        {
                            transferObjectIDs.Remove(item);
                        }
                    }

                    foreach (var to in transferObjectIDs)
                    {
                        var fullLoaded = true;
                        if (transferPaths.TryGetValue(to, out hopQueue))
                        {
                            ModuleTypeEnum currHop = ModuleTypeEnum.UNDEFINED;
                            if (hopQueue.Count > 0)
                            {
                                currHop = hopQueue.Peek();
                                if (unBufferedTransfer == true)
                                {
                                    if (currHop != ModuleTypeEnum.ARM)
                                    {
                                        if (currHop != ModuleTypeEnum.BUFFER)
                                        {
                                            fullLoaded = false;
                                        }
                                    }
                                }
                                else
                                {
                                    if (currHop != ModuleTypeEnum.BUFFER)
                                    {
                                        fullLoaded = false;
                                    }
                                }
                            }
                        }
                        bufferFullLoaded = fullLoaded;
                    }
                    foreach (var to in pendingPaths)
                    {
                        var fullLoaded = true;
                        if (transferPaths.TryGetValue(to, out hopQueue))
                        {
                            ModuleTypeEnum currHop = ModuleTypeEnum.UNDEFINED;
                            if (hopQueue.Count > 0)
                            {
                                currHop = hopQueue.Peek();

                                if (unBufferedTransfer == true)
                                {
                                    if (currHop != ModuleTypeEnum.ARM)
                                    {
                                        if (currHop != ModuleTypeEnum.BUFFER)
                                        {
                                            fullLoaded = false;
                                        }
                                    }
                                }
                                else
                                {
                                    if (currHop != ModuleTypeEnum.BUFFER)
                                    {
                                        fullLoaded = false;
                                    }
                                }
                            }
                        }
                        bufferFullLoaded = fullLoaded;
                    }

                    bool isLoadingPathRemain = false;
                    bool isUnLoadingPathRemain = false;



                    if (bufferFullLoaded == false)
                    {
                        var avaPA = currLoaderMap.PreAlignModules.FirstOrDefault(p => p.WaferStatus == EnumSubsStatus.NOT_EXIST & p.Enable == true);

                        foreach (var item in transferObjectIDs)
                        {
                            TransferObject destTO = loaderMap.GetSubstrateByGUID(item);
                            if (destTO.CurrHolder.ModuleType == ModuleTypeEnum.CHUCK)
                            {
                                isLoadingPathRemain = true;
                            }
                            else
                            {
                                isUnLoadingPathRemain = true;
                            }
                        }
                        foreach (var item in transferObjectIDs)
                        {
                            // Recover pended objects for futher path
                            TransferObject destTO = loaderMap.GetSubstrateByGUID(item);
                            if (isLoadingPathRemain == true && isUnLoadingPathRemain == true)
                            {
                                if (destTO.CurrHolder.ModuleType != ModuleTypeEnum.CHUCK)
                                {
                                    if(avaPA != null)
                                    {
                                        pendingPaths.Add(item);
                                    }
                                }
                                else if (destTO.CurrHolder.ModuleType == ModuleTypeEnum.CHUCK)
                                {
                                    if (avaPA == null)
                                    {
                                        pendingPaths.Add(item);
                                    }
                                }
                            }
                        }
                        foreach (var item in pendingPaths)
                        {
                            transferObjectIDs.Remove(item);
                        }
                    }

                    foreach (var to in transferObjectIDs)
                    {
                        TransferObject destTO = loaderMap.GetSubstrateByGUID(to);

                        if (transferPaths.TryGetValue(to, out hopQueue))
                        {
                            ModuleTypeEnum currHop = ModuleTypeEnum.UNDEFINED;
                            ModuleTypeEnum prevHop = ModuleTypeEnum.UNDEFINED;
                            if (hopQueue.Count > 1)
                            {
                                currHop = hopQueue.Peek();
                            }
                            else
                            {
                                if (hopQueue.Count <= 0)
                                {
                                    continue;
                                }
                                currHop = hopQueue.Dequeue();
                                transferredObjectIDs.Add(to);
                                if (hopQueue.Count <= 0)
                                {
                                    break;
                                }
                            }

                            if (currHop != ModuleTypeEnum.UNDEFINED)
                            {
                                avaArmCount = currLoaderMap.GetHolderModuleAll().Count(
                                    h => h.ID.ModuleType == ModuleTypeEnum.ARM & h.WaferStatus == EnumSubsStatus.NOT_EXIST);
                                if (avaArmCount == 0 & currHop != ModuleTypeEnum.ARM)
                                {
                                    break;
                                }
                                avaPACount = currLoaderMap.GetHolderModuleAll().Count(
                                h => h.ID.ModuleType == ModuleTypeEnum.PA & h.WaferStatus == EnumSubsStatus.NOT_EXIST & h.Enable == true&&h.ReservationInfo.ReservationState==EnumReservationState.NOT_RESERVE);
                                if (avaPACount == 0 & currHop != ModuleTypeEnum.PA)
                                {
                                    TransferObject currSubObj = currLoaderMap.GetSubstrateByGUID(to);
                                    if (currSubObj.PrevHolder.ModuleType != ModuleTypeEnum.PA)
                                    {
                                        continue;

                                    }


                                }
                                //bufferFullLoaded = true;
                                //foreach (var item in transferObjectIDs)
                                //{
                                //    if(transferPaths.TryGetValue(item, out hopQueue))
                                //    {
                                //        bufferFullLoaded &= hopQueue.Peek() == ModuleTypeEnum.BUFFER;
                                //    }
                                //}

                                currHop = hopQueue.Peek();

                                //if(currHop == ModuleTypeEnum.BUFFER&& !bufferedObjectIDs.Contains(to)&& !historyBufferedObjectIDs.Contains(to))
                                //{
                                //    bufferedObjectIDs.Add(to);
                                //}
                                //else 
                                if (currHop != ModuleTypeEnum.BUFFER | bufferFullLoaded == true | loadWaferFullBuffered == true)
                                {
                                    bufferedObjectIDs.Remove(to);
                                    //currHop = hopQueue.Dequeue();
                                    currHop = hopQueue.Peek();
                                    ModuleTypeEnum nextHop;
                                    if (hopQueue.Count > 1)
                                    {
                                        // Queue empty. Transfer done.
                                        //nextHop = hopQueue.Peek();
                                        nextHop = hopQueue.ElementAt(1);
                                    }
                                    else
                                    {
                                        nextHop = ModuleTypeEnum.UNDEFINED;
                                    }
                                    if (nextHop != ModuleTypeEnum.UNDEFINED)
                                    {
                                        var hopMap = (LoaderMap)currLoaderMap.Clone();
                                        HolderModuleInfo targetHolder = null;
                                        //var handlingArm = Loader.ModuleManager.FindModule<IARMModule>(ModuleTypeEnum.ARM, i + 1);
                                        TransferObject currSubObj = hopMap.GetSubstrateByGUID(to);
                                        if (nextHop == ModuleTypeEnum.SLOT)
                                        {
                                            var currHolderModule = hopMap.GetHolderModuleAll()
                                                .FirstOrDefault(h =>
                                                {
                                                    if (h.Substrate != null)
                                                    {
                                                        return h.Substrate.ID.Value == to;
                                                    }
                                                    else
                                                    {
                                                        return false;
                                                    }
                                                });
                                            targetHolder = hopMap.GetHolderModuleAll().FirstOrDefault(
                                                h => h.ID.ModuleType == nextHop & h.WaferStatus == EnumSubsStatus.NOT_EXIST & h.ID == destTO.CurrHolder);
                                        }
                                        else if (nextHop == ModuleTypeEnum.CHUCK)
                                        {
                                            targetHolder = hopMap.GetHolderModuleAll().FirstOrDefault(h => h.ID.Label == destTO.CurrHolder.Label);
                                        }
                                        else if (nextHop == ModuleTypeEnum.BUFFER)
                                        {
                                            if (destTO.CurrHolder.ModuleType == ModuleTypeEnum.BUFFER)
                                            {
                                                targetHolder = hopMap.GetHolderModuleAll().FirstOrDefault(h => h.ID.Label == destTO.CurrHolder.Label && h.Enable == true);
                                            }
                                            else
                                            {
                                                targetHolder = hopMap.GetHolderModuleAll().FirstOrDefault(h => h.ID.ModuleType == nextHop & h.WaferStatus == EnumSubsStatus.NOT_EXIST && h.Enable == true);
                                            }

                                        }
                                        else
                                        {
                                            if (destTO.CurrHolder.ModuleType == ModuleTypeEnum.FIXEDTRAY)
                                            {
                                                if (nextHop == ModuleTypeEnum.ARM)
                                                {
                                                    targetHolder = hopMap.GetHolderModuleAll().FirstOrDefault(h => h.ID.ModuleType == nextHop & h.WaferStatus == EnumSubsStatus.NOT_EXIST && h.Enable == true);
                                                }
                                                else if (nextHop == ModuleTypeEnum.PA)
                                                {
                                                    targetHolder = hopMap.GetHolderModuleAll().FirstOrDefault(h => h.ID.ModuleType == nextHop & h.WaferStatus == EnumSubsStatus.NOT_EXIST && h.Enable == true && h.ReservationInfo.ReservationState==EnumReservationState.NOT_RESERVE);
                                                }
                                                else
                                                {
                                                    targetHolder = hopMap.GetHolderModuleAll().FirstOrDefault(h => h.ID.Label == destTO.CurrHolder.Label && h.Enable == true);
                                                }
                                            }
                                            else
                                            {
                                                if (nextHop == ModuleTypeEnum.INSPECTIONTRAY && destTO.CurrHolder.ModuleType == ModuleTypeEnum.INSPECTIONTRAY)
                                                {
                                                    targetHolder = hopMap.GetHolderModuleAll().FirstOrDefault(h => h.ID.Label == destTO.CurrHolder.Label && h.WaferStatus == EnumSubsStatus.NOT_EXIST);
                                                    if (targetHolder == null)
                                                    {
                                                        targetHolder = hopMap.GetHolderModuleAll().FirstOrDefault(h => h.ID.ModuleType == nextHop & h.WaferStatus == EnumSubsStatus.NOT_EXIST);
                                                    }
                                                }
                                                else
                                                {
                                                    targetHolder = hopMap.GetHolderModuleAll().FirstOrDefault(h => h.ID.ModuleType == nextHop & h.WaferStatus == EnumSubsStatus.NOT_EXIST && h.Enable == true && h.ReservationInfo.ReservationState == EnumReservationState.NOT_RESERVE);
                                                }
                                            }
                                        }

                                        if (targetHolder != null)    // Check availability
                                        {
                                            if (targetHolder.WaferStatus == EnumSubsStatus.NOT_EXIST)
                                            {
                                                var currHolderModule = hopMap.GetHolderModuleAll()
                                                    .FirstOrDefault(h =>
                                                    {
                                                        if (h.Substrate != null)
                                                        {
                                                            return h.Substrate.ID.Value == to;
                                                        }
                                                        else
                                                        {
                                                            return false;
                                                        }
                                                    });
                                                if (currHolderModule != null)
                                                {
                                                    prevHop = hopQueue.Dequeue();
                                                    MoveTO(targetHolder, currHolderModule);
                                                    currHop = hopQueue.Peek();
                                                    //maps.Add(hopMap);
                                                    maps = AddMap(targetHolder, currHolderModule, ref hopMap, maps);
                                                    currLoaderMap = (LoaderMap)hopMap.Clone();
                                                }
                                                if (hopQueue.Count == 2)
                                                {
                                                    // Queue empty. Transfer done.
                                                    hopMap = (LoaderMap)currLoaderMap.Clone();
                                                    var destHop = hopQueue.ElementAt(1);
                                                    var destHolder = hopMap.GetHolderModuleAll().First(h => h.ID.Label == destTO.CurrHolder.Label);
                                                    avaArmCount = currLoaderMap.GetHolderModuleAll().Count(h => h.ID.ModuleType == ModuleTypeEnum.ARM & h.WaferStatus == EnumSubsStatus.NOT_EXIST);
                                                    if (avaArmCount == 0)
                                                    {
                                                        ;
                                                    }
                                                    if (destHolder.WaferStatus == EnumSubsStatus.EXIST && avaArmCount > 0 && destHolder.Substrate.CurrHolder.ModuleType != ModuleTypeEnum.ARM)      // Substrate already exist on destination holder
                                                    {
                                                        Queue<ModuleTypeEnum> swapQueue;
                                                        if (transferPaths.TryGetValue(destHolder.Substrate.ID.Value, out swapQueue))
                                                        {
                                                            var destSubsID = destHolder.Substrate.ID.Value;

                                                            var backupHop = currHop;
                                                            currHop = swapQueue.Dequeue();
                                                            if (hopQueue.Count > 0)
                                                            {
                                                                nextHop = swapQueue.Peek();
                                                                targetHolder = hopMap.GetHolderModuleAll().FirstOrDefault(h => h.ID.ModuleType == nextHop & h.WaferStatus == EnumSubsStatus.NOT_EXIST);
                                                                if (targetHolder != null & beSwappedObjectIDs.Exists(o => o == destSubsID))
                                                                {
                                                                    beSwappedObjectID = beSwappedObjectIDs.First(o => o == destSubsID);
                                                                    //prevHop = hopQueue.Dequeue();
                                                                    MoveTO(targetHolder, destHolder);
                                                                    //currHop = hopQueue.Peek();
                                                                    //maps.Add(hopMap);
                                                                    maps = AddMap(targetHolder, currHolderModule, ref hopMap, maps);
                                                                    currLoaderMap = (LoaderMap)hopMap.Clone();
                                                                    backupHop = ModuleTypeEnum.UNDEFINED;
                                                                }
                                                                else
                                                                {
                                                                    var Index1 = hopQueue.ElementAt(1);
                                                                    var index2 = hopQueue.ElementAt(0);
                                                                    if (Index1 == ModuleTypeEnum.CHUCK & index2 == ModuleTypeEnum.ARM)
                                                                    {
                                                                        targetHolder = hopMap.GetHolderModuleAll().FirstOrDefault(h => h.ID.ModuleType == nextHop & h.ID.Index == 1);
                                                                        if (targetHolder != null & beSwappedObjectIDs.Exists(o => o == destSubsID))
                                                                        {
                                                                            beSwappedObjectID = beSwappedObjectIDs.First(o => o == destSubsID);
                                                                            prevHop = hopQueue.Dequeue();
                                                                            MoveTO(targetHolder, destHolder);
                                                                            currHop = hopQueue.Peek();
                                                                            //maps.Add(hopMap);
                                                                            maps = AddMap(targetHolder, currHolderModule, ref hopMap, maps);
                                                                            currLoaderMap = (LoaderMap)hopMap.Clone();
                                                                            backupHop = ModuleTypeEnum.UNDEFINED;
                                                                        }
                                                                    }
                                                                }
                                                            }

                                                            if (backupHop != ModuleTypeEnum.UNDEFINED)
                                                            {
                                                                LoggerManager.Error($"Map slicing error. No available target for substrate(@{destHolder.ID.Label}). Target holder is {nextHop}, use backupHop{backupHop}", isLoaderMap: true);
                                                                currHop = backupHop;
                                                            }
                                                        }
                                                    }
                                                }
                                                if (currHop == ModuleTypeEnum.BUFFER && !bufferedObjectIDs.Contains(to) && !historyBufferedObjectIDs.Contains(to))
                                                {
                                                    bufferedObjectIDs.Add(to);
                                                }
                                            }
                                            // Swap scenario
                                            else if (targetHolder.WaferStatus == EnumSubsStatus.EXIST & targetHolder.Substrate != null & targetHolder.Substrate.CurrHolder.ModuleType != ModuleTypeEnum.ARM)
                                            {
                                                var swapTarget = hopMap.GetSubstrateByGUID(targetHolder.Substrate.ID.Value);
                                                var currHolderModule = hopMap.GetHolderModuleAll()
                                                        .FirstOrDefault(h =>
                                                        {
                                                            if (h.Substrate != null)
                                                            {
                                                                return h.Substrate.ID.Value == targetHolder.Substrate.ID.Value;
                                                            }
                                                            else
                                                            {
                                                                return false;
                                                            }
                                                        });
                                                Queue<ModuleTypeEnum> swapHopQueue;

                                                if (transferPaths.TryGetValue(targetHolder.Substrate.ID.Value, out swapHopQueue))
                                                {
                                                    if (swapHopQueue.Count > 0)
                                                    {
                                                        // Queue empty. Transfer done.
                                                        nextHop = swapHopQueue.Peek();
                                                        avaArmCount = currLoaderMap.GetHolderModuleAll().Count(h => h.ID.ModuleType == ModuleTypeEnum.ARM & h.WaferStatus == EnumSubsStatus.NOT_EXIST);
                                                        if (avaArmCount == 0 && nextHop == ModuleTypeEnum.CHUCK)
                                                        {
                                                            break;
                                                        }
                                                        if (nextHop == targetHolder.Substrate.CurrHolder.ModuleType)
                                                        {
                                                            LoggerManager.Debug($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} : Curr. Hop = {targetHolder.Substrate.CurrHolder.ModuleType}, Next hop = {nextHop}. Dequeuing current hop.");
                                                            nextHop = swapHopQueue.Dequeue();
                                                            nextHop = swapHopQueue.Peek();

                                                            LoggerManager.Debug($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} : Continue to next hop({nextHop}).");
                                                        }

                                                    }
                                                    else
                                                    {
                                                        nextHop = ModuleTypeEnum.UNDEFINED;
                                                    }
                                                }
                                                if (nextHop == ModuleTypeEnum.CHUCK)
                                                {
                                                    targetHolder = hopMap.GetHolderModuleAll().FirstOrDefault(
                                                   h => h.ID.ModuleType == ModuleTypeEnum.BUFFER & h.WaferStatus == EnumSubsStatus.NOT_EXIST);
                                                }
                                                else
                                                {
                                                    targetHolder = hopMap.GetHolderModuleAll().FirstOrDefault(
                                                  h => h.ID.ModuleType == nextHop & h.WaferStatus == EnumSubsStatus.NOT_EXIST);
                                                }
                                                var destSubsID = currHolderModule.Substrate.ID.Value;
                                                if (targetHolder != null & beSwappedObjectIDs.Exists(o => o == destSubsID))
                                                {
                                                    beSwappedObjectID = beSwappedObjectIDs.First(o => o == destSubsID);
                                                    MoveTO(targetHolder, currHolderModule);

                                                    //maps.Add(hopMap);
                                                    maps = AddMap(targetHolder, currHolderModule, ref hopMap, maps);
                                                    currLoaderMap = (LoaderMap)hopMap.Clone();
                                                }
                                            }
                                            else
                                            {
                                                targetHolder = hopMap.GetHolderModuleAll().FirstOrDefault(h => h.ID.ModuleType == nextHop & h.WaferStatus == EnumSubsStatus.EXIST);
                                                if (targetHolder != null)
                                                {
                                                    // Swap scenario
                                                    if (targetHolder.WaferStatus == EnumSubsStatus.EXIST & targetHolder.Substrate.CurrHolder.ModuleType != ModuleTypeEnum.ARM)
                                                    {
                                                        var swapTarget = hopMap.GetSubstrateByGUID(targetHolder.Substrate.ID.Value);
                                                        var currHolderModule = hopMap.GetHolderModuleAll()
                                                                .FirstOrDefault(h =>
                                                                {
                                                                    if (h.Substrate != null)
                                                                    {
                                                                        return h.Substrate.ID.Value == targetHolder.Substrate.ID.Value;
                                                                    }
                                                                    else
                                                                    {
                                                                        return false;
                                                                    }
                                                                });
                                                        Queue<ModuleTypeEnum> swapHopQueue;

                                                        if (transferPaths.TryGetValue(targetHolder.Substrate.ID.Value, out swapHopQueue))
                                                        {
                                                            if (swapHopQueue.Count > 0)
                                                            {
                                                                // Queue empty. Transfer done.

                                                                nextHop = swapHopQueue.Peek();
                                                            }
                                                            else
                                                            {
                                                                nextHop = ModuleTypeEnum.UNDEFINED;
                                                            }
                                                        }
                                                        targetHolder = hopMap.GetHolderModuleAll().FirstOrDefault(
                                                            h => h.ID.ModuleType == nextHop & h.WaferStatus == EnumSubsStatus.NOT_EXIST);
                                                        if (targetHolder != null)
                                                        {
                                                            MoveTO(targetHolder, currHolderModule);
                                                            //maps.Add(hopMap);
                                                            maps = AddMap(targetHolder, currHolderModule, ref hopMap, maps);
                                                            currLoaderMap = (LoaderMap)hopMap.Clone();
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LoggerManager.Error($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} : Map slicing error. No available target for substrate(ID:{to}). Target holder is {nextHop}", isLoaderMap: true);
                                            LoggerManager.Debug($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} : LoaderJobViewList.Clear.");
                                            Loader.LoaderJobViewList.Clear();
                                            return null;
                                        }
                                    }
                                    // No more hop
                                    else
                                    {
                                        if (hopQueue.Count > 0)
                                        {
                                            // Queue empty. Transfer done.
                                            nextHop = hopQueue.Dequeue();
                                        }
                                    }
                                    //bufferFullLoaded = false;
                                }
                                // On buffer state
                                else
                                {
                                    if (currHop == ModuleTypeEnum.BUFFER)
                                    {
                                        bufferedObjectIDs.Add(to);
                                    }

                                    //bufferFullLoaded = true;
                                }
                            }
                            // No more hop
                            else
                            {

                            }
                        }
                    }

                    // No available arm. Pending unloaded holders.
                    if (avaArmCount == 0)
                    {
                        if (beSwappedObjectIDs.Exists(o => o == beSwappedObjectID))
                        {
                            transferObjectIDs.Add(beSwappedObjectID);
                            beSwappedObjectIDs.Remove(beSwappedObjectID);
                        }

                        foreach (var item in transferObjectIDs)
                        {
                            if (transferPaths.TryGetValue(item, out hopQueue))
                            {
                                if (hopQueue.Count > 0 && hopQueue.Peek() != ModuleTypeEnum.ARM)
                                {
                                    pendingPaths.Add(item);
                                }
                            }
                        }
                        foreach (var item in pendingPaths)
                        {
                            transferObjectIDs.Remove(item);
                        }
                    }
                    foreach (var duplicatedItem in pendingPaths)
                    {
                        transferObjectIDs.Remove(duplicatedItem);
                    }
                    if (bufferedObjectIDs.Count > 1 && beSwappedObjectIDs.Count > 1)
                    {
                    }
                    else
                    {
                        foreach (var bufferedItem in bufferedObjectIDs)
                        {
                            transferObjectIDs.Remove(bufferedItem);
                        }
                    }
                    foreach (var transferredItem in transferredObjectIDs)
                    {
                        transferObjectIDs.Remove(transferredItem);
                    }

                    // All TO has been transferred.
                    if (transferredObjectIDs.Count == transferPaths.Count)
                    {
                        //LoggerManager.LoaderMapLog($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} : Completed.", basicLog: false);
                        toRemain = false;
                    }
                    loopCount++;
                    if (loopCount > 500)
                    {
                        LoggerManager.Error($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} : Map slicing error. No available target, loopCount:{loopCount}", isLoaderMap: true);
                        LoggerManager.Debug($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} : LoaderJobViewList.Clear.");
                        Loader.LoaderJobViewList.Clear();
                        return null;

                    }
                    if (loaderJobList.Count() > 0)
                    {
                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            this.Loader.LoaderJobViewList.Clear();
                            foreach (var job in loaderJobList)
                            {
                                this.Loader.LoaderJobViewList.Add(job);
                            }
                            Loader.LoaderJobSorting();

                        });
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} : Map slicing error. Err = {err.Message}", isLoaderMap: true);
                LoggerManager.Debug($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} : LoaderJobViewList.Clear.");
                Loader.LoaderJobViewList.Clear();
            }

            return maps;
        }



        public List<LoaderMap> ManualSlicing(LoaderMap loaderMap)
        {
            LoggerManager.InitCheckDuplicateMap();

            List<LoaderMap> maps = new List<LoaderMap>();
            LoaderMap prevMap = new LoaderMap();
            List<CassetteModuleInfo> scanTargets = new List<CassetteModuleInfo>();
            ModuleID DestinationModule = new ModuleID();
            try
            {
                var lockobj = new object();
                lock (lockobj)
                {
                    if (loaderMap == null)
                    {
                        return maps;
                    }

                    loaderJobList.Clear();

                    //analyze scan req
                    foreach (var dstCstInfo in loaderMap.CassetteModules)
                    {
                        var currCst = Loader.ModuleManager.FindModule<ICassetteModule>(dstCstInfo.ID);

                        if (dstCstInfo.ScanState == CassetteScanStateEnum.READING)
                        {
                            scanTargets.Add(dstCstInfo);
                        }
                    }
                    if (scanTargets.Count > 0)
                    {
                        LoaderMap scanMap = new LoaderMap();

                        foreach (var cst in scanTargets)
                        {
                            var cstToRead = scanMap.CassetteModules.First(c => c.ID.Index == cst.ID.Index);
                            cstToRead.ScanState = CassetteScanStateEnum.READING;
                            LoggerManager.Debug($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} : Cassette(#{cstToRead.ID.Index} has been added to scan cassette target list.");
                        }
                        maps.Add(scanMap);
                        LoggerManager.Debug($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} : Sliced map has been added. Map count = {maps.Count}");
                    }


                    var transferObjects = loaderMap.GetTransferObjectAll();
                    transferObjects = transferObjects.OrderBy(i => i.Priority).ToList();
                    Dictionary<string, List<ModuleTypeEnum>> paths = new Dictionary<string, List<ModuleTypeEnum>>();

                    var transferPaths = new Dictionary<string, Queue<ModuleTypeEnum>>();
                    var transferObjectIDs = new List<string>();
                    var bufferedObjectIDs = new List<string>();
                    var historyBufferedObjectIDs = new List<string>();
                    var beSwappedObjectIDs = new List<string>();
                    var transferredObjectIDs = new List<string>();
                    var beSwappedObjectID = "";
                    bool unBufferedTransfer = false;

                    foreach (var dstSubObj in transferObjects)
                    {
                        TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(dstSubObj.ID.Value);
                        if (currSubObj == null)
                        {
                            continue;
                        }
                        else if (currSubObj.WaferType.Value != EnumWaferType.CARD)
                        {
                            continue;
                        }
                        ModuleID dstPos = dstSubObj.CurrPos;
                        ICardLocatable dstModule = Loader.ModuleManager.FindModule(dstPos) as ICardLocatable;

                        bool equalRef = currSubObj.CurrPos == dstSubObj.CurrPos;


                        if (currSubObj.CurrPos != dstPos)
                        {
                            ModulePathGenerator pathGenerator = new ModulePathGenerator(currSubObj);
                            paths.Add(currSubObj.ID.Value, pathGenerator.GetPath(dstSubObj));
                            var map = (LoaderMap)Loader.GetLoaderInfo().StateMap.Clone();



                            ModuleID CurrPos = currSubObj.CurrPos;
                            var path = paths.Keys;
                            foreach (var types in paths.Values)
                            {
                                for (int i = 1; i < types.Count; i++)
                                {

                                    var hopMap = (LoaderMap)map.Clone();
                                    var currHolderModule = hopMap.GetHolderModuleAll().FirstOrDefault(h => h.ID == CurrPos);
                                    HolderModuleInfo targetHolder = null;

                                    if (types[i] == ModuleTypeEnum.CARDARM)
                                    {
                                        targetHolder = hopMap.GetHolderModuleAll().FirstOrDefault(h => h.ModuleType == types[i] && (h.WaferStatus == EnumSubsStatus.NOT_EXIST || h.WaferStatus == EnumSubsStatus.CARRIER));
                                    }
                                    else
                                    {
                                        targetHolder = hopMap.GetHolderModuleAll().FirstOrDefault(h => h.ModuleType == types[i] && (h.WaferStatus == EnumSubsStatus.NOT_EXIST || h.WaferStatus == EnumSubsStatus.CARRIER) && h.ID.Index == dstPos.Index);
                                    }

                                    MoveTO(targetHolder, currHolderModule, bManual: true);
                                    CurrPos = targetHolder.Substrate.CurrHolder;
                                    //maps.Add(hopMap);
                                    maps = AddMap(targetHolder, currHolderModule, ref hopMap, maps);
                                    map = (LoaderMap)hopMap.Clone();
                                }
                            }
                            if (loaderJobList.Count() > 0)
                            {
                                Application.Current.Dispatcher.Invoke(delegate
                                {
                                    this.Loader.LoaderJobViewList.Clear();
                                    foreach (var job in loaderJobList)
                                    {
                                        this.Loader.LoaderJobViewList.Add(job);
                                    }
                                    Loader.LoaderJobSorting();

                                });
                            }
                            return maps; //Card와 관련된 ManualSlicing 에서는 여기서 빠져나감...주의..
                        }
                        else
                        {

                        }
                    } //Card 


                    var currAvaArmCount = Loader.GetLoaderInfo().StateMap.GetHolderModuleAll().Count(
                        h => h.ID.ModuleType == ModuleTypeEnum.ARM & h.WaferStatus == EnumSubsStatus.NOT_EXIST);

                    foreach (var transObj in transferObjects)
                    {
                        TransferObject currSubObj = Loader.ModuleManager.FindTransferObject(transObj.ID.Value);

                        var dstSubObj = (TransferObject)transObj.Clone();
                        if (currSubObj == null)
                        {
                            continue;
                        }
                        ModuleID dstPos = dstSubObj.CurrPos;
                        IWaferLocatable dstModule = Loader.ModuleManager.FindModule(dstPos) as IWaferLocatable;

                        bool equalRef = currSubObj.CurrPos == dstSubObj.CurrPos;

                        if (currSubObj.CurrPos != dstPos)
                        {
                            DestinationModule = dstPos;
                            IModulePathGenerator pathGenerator = null;
                            unBufferedTransfer = true;
                            dstSubObj.NotchAngle.Value = dstSubObj.ChuckNotchAngle.Value;
                            if (dstPos.ModuleType == ModuleTypeEnum.SLOT)
                            {
                                dstSubObj.NotchAngle.Value = dstSubObj.SlotNotchAngle.Value;
                            }
                            currSubObj.PreAlignState = dstSubObj.PreAlignState;
                            pathGenerator = new ModuleManualPathGenerator(currSubObj);
                            paths.Add(currSubObj.ID.Value, pathGenerator.GetPath(dstSubObj));
                        }
                        else
                        {
                            if (dstModule is IOCRReadable)
                            {
                                if (dstSubObj.OverrideOCRDeviceOption.IsEnable.Value ||
                                    dstSubObj.OverrideOCROption.IsEnable.Value)
                                {

                                }
                            }
                        }
                    }


                    // Create path queue.
                    foreach (var path in paths)
                    {
                        var pathQueue = new Queue<ModuleTypeEnum>();
                        foreach (var item in path.Value)
                        {
                            pathQueue.Enqueue(item);
                        }
                        transferPaths.Add(path.Key, pathQueue);
                        transferObjectIDs.Add(path.Key);
                    }
                    bool toRemain = true;
                    var currLoaderMap = (LoaderMap)Loader.GetLoaderInfo().StateMap.Clone();
                    var originalMap = (LoaderMap)Loader.GetLoaderInfo().StateMap.Clone();
                    bool bufferFullLoaded = false;
                    List<string> pendingPaths = new List<string>();
                    int avaArmCount = 0;

                    bool loadWaferFullBuffered = false;
                    while (toRemain)
                    {
                        loadWaferFullBuffered = false;
                        var availableHolder = currLoaderMap.GetHolderModuleAll();

                        foreach (var item in availableHolder)
                        {
                            if (item.WaferStatus == EnumSubsStatus.UNDEFINED)
                            {
                                item.WaferStatus = EnumSubsStatus.NOT_EXIST;
                            }
                        }

                        Queue<ModuleTypeEnum> hopQueue;
                        hopQueue = new Queue<ModuleTypeEnum>();
                        if (transferObjectIDs.Count == 0)
                        {
                            bool isLoadingPathRemained = false;
                            if (pendingPaths.Count > 0)
                            {


                                foreach (var item in pendingPaths)
                                {
                                    TransferObject destTO = loaderMap.GetSubstrateByGUID(item);
                                    if (destTO.CurrHolder.ModuleType == ModuleTypeEnum.CHUCK)
                                    {
                                        isLoadingPathRemained = true;
                                    }
                                }
                                if (!isLoadingPathRemained && bufferedObjectIDs.Count > 0)
                                {
                                    foreach (var item in bufferedObjectIDs)
                                    {
                                        TransferObject destTO = loaderMap.GetSubstrateByGUID(item);
                                        if (destTO.CurrHolder.ModuleType == ModuleTypeEnum.CHUCK)
                                        {
                                            transferObjectIDs.Add(item);
                                            historyBufferedObjectIDs.Add(item);
                                            loadWaferFullBuffered = true;
                                        }

                                    }
                                    foreach (var item in transferObjectIDs)
                                    {
                                        bufferedObjectIDs.Remove(item);
                                    }
                                    if (transferObjectIDs.Count == 0)
                                    {
                                        foreach (var item in pendingPaths)
                                        {
                                            transferObjectIDs.Add(item);
                                        }

                                        foreach (var item in transferObjectIDs)
                                        {
                                            pendingPaths.Remove(item);
                                        }
                                    }

                                }
                                else
                                {
                                    foreach (var item in pendingPaths)
                                    {
                                        // Recover pended objects for futher path
                                        TransferObject destTO = loaderMap.GetSubstrateByGUID(item);
                                        if (isLoadingPathRemained == true)
                                        {
                                            if (destTO.CurrHolder.ModuleType == ModuleTypeEnum.CHUCK)
                                            {
                                                transferObjectIDs.Add(item);
                                            }
                                        }
                                        else
                                        {
                                            transferObjectIDs.Add(item);
                                        }

                                    }


                                    foreach (var item in transferObjectIDs)
                                    {
                                        pendingPaths.Remove(item);
                                    }
                                }

                            }
                            else if (bufferedObjectIDs.Count > 0)
                            {
                                foreach (var item in bufferedObjectIDs)
                                {
                                    // Recover buffered objects for futher path
                                    transferObjectIDs.Add(item);
                                }
                                //foreach (var item in transferObjectIDs)
                                //{
                                //    bufferedObjectIDs.Remove(item);
                                //}
                                bufferFullLoaded = true;
                            }
                            else if (beSwappedObjectIDs.Count > 0)
                            {
                                foreach (var item in beSwappedObjectIDs)
                                {
                                    // Recover buffered objects for futher path
                                    transferObjectIDs.Add(item);
                                }
                                foreach (var item in transferObjectIDs)
                                {
                                    beSwappedObjectIDs.Remove(item);
                                }
                            }
                            else
                            {
                                //LoggerManager.LoaderMapLog($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} : Completed. (transferObjectIDs Count zero)", basicLog: false);
                                toRemain = false;
                            }
                        }
                        // Search be swapped objects.
                        else
                        {
                            // Source & target compare method
                            var searchMap = (LoaderMap)currLoaderMap.Clone();
                            var demandMap = (LoaderMap)loaderMap.Clone();
                            foreach (var to in transferObjectIDs)
                            {
                                TransferObject currSubObj = demandMap.GetSubstrateByGUID(to);

                                var targetHolder = searchMap.GetHolderModuleAll().FirstOrDefault(h => h.ID == currSubObj.CurrHolder);
                                if (targetHolder.Substrate != null)
                                {
                                    if (targetHolder.WaferStatus == EnumSubsStatus.EXIST & targetHolder.Substrate.ID.Value != currSubObj.ID.Value)
                                    {
                                        var subsToBeSwapped = searchMap.GetHolderModuleAll().FirstOrDefault(h => h.ID == currSubObj.CurrHolder);
                                        if (beSwappedObjectIDs.Exists(o => o == subsToBeSwapped.Substrate.ID.Value) == false)
                                        {
                                            beSwappedObjectIDs.Add(subsToBeSwapped.Substrate.ID.Value);
                                        }
                                        LoggerManager.Debug($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} : Substrate on {targetHolder.ID.Label} will be swapped with {currSubObj.OriginHolder.Label} substrate.", isLoaderMap: true);
                                    }
                                }
                            }

                            // Remove be swapped objects from transfer objects.
                            foreach (var item in beSwappedObjectIDs)
                            {
                                transferObjectIDs.Remove(item);
                            }
                        }

                        foreach (var to in transferObjectIDs)
                        {
                            var fullLoaded = true;
                            if (transferPaths.TryGetValue(to, out hopQueue))
                            {
                                ModuleTypeEnum currHop = ModuleTypeEnum.UNDEFINED;
                                if (hopQueue.Count > 0)
                                {
                                    currHop = hopQueue.Peek();
                                    if (unBufferedTransfer == true)
                                    {
                                        if (currHop != ModuleTypeEnum.ARM)
                                        {
                                            if (currHop != ModuleTypeEnum.BUFFER)
                                            {
                                                fullLoaded = false;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (currHop != ModuleTypeEnum.BUFFER)
                                        {
                                            fullLoaded = false;
                                        }
                                    }
                                }
                            }
                            bufferFullLoaded = fullLoaded;
                        }
                        foreach (var to in pendingPaths)
                        {
                            var fullLoaded = true;
                            if (transferPaths.TryGetValue(to, out hopQueue))
                            {
                                ModuleTypeEnum currHop = ModuleTypeEnum.UNDEFINED;
                                if (hopQueue.Count > 0)
                                {
                                    currHop = hopQueue.Peek();

                                    if (unBufferedTransfer == true)
                                    {
                                        if (currHop != ModuleTypeEnum.ARM)
                                        {
                                            if (currHop != ModuleTypeEnum.BUFFER)
                                            {
                                                fullLoaded = false;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (currHop != ModuleTypeEnum.BUFFER)
                                        {
                                            fullLoaded = false;
                                        }
                                    }
                                }
                            }
                            bufferFullLoaded = fullLoaded;
                        }

                        bool isLoadingPathRemain = false;
                        bool isUnLoadingPathRemain = false;



                        if (bufferFullLoaded == false)
                        {
                            foreach (var item in transferObjectIDs)
                            {
                                TransferObject destTO = loaderMap.GetSubstrateByGUID(item);
                                if (destTO.CurrHolder.ModuleType == ModuleTypeEnum.CHUCK)
                                {
                                    isLoadingPathRemain = true;
                                }
                                else
                                {
                                    isUnLoadingPathRemain = true;
                                }
                            }
                            foreach (var item in transferObjectIDs)
                            {
                                // Recover pended objects for futher path
                                TransferObject destTO = loaderMap.GetSubstrateByGUID(item);
                                if (isLoadingPathRemain == true && isUnLoadingPathRemain == true)
                                {
                                    if (destTO.CurrHolder.ModuleType != ModuleTypeEnum.CHUCK)
                                    {
                                        pendingPaths.Add(item);
                                    }
                                }
                            }
                            foreach (var item in pendingPaths)
                            {
                                transferObjectIDs.Remove(item);
                            }
                        }

                        foreach (var to in transferObjectIDs)
                        {
                            TransferObject destTO = loaderMap.GetSubstrateByGUID(to);

                            if (transferPaths.TryGetValue(to, out hopQueue))
                            {
                                ModuleTypeEnum currHop = ModuleTypeEnum.UNDEFINED;
                                ModuleTypeEnum prevHop = ModuleTypeEnum.UNDEFINED;
                                if (hopQueue.Count > 1)
                                {
                                    currHop = hopQueue.Peek();
                                }
                                else
                                {
                                    currHop = hopQueue.Dequeue();
                                    transferredObjectIDs.Add(to);
                                    if (hopQueue.Count <= 0)
                                    {
                                        break;
                                    }
                                }

                                if (currHop != ModuleTypeEnum.UNDEFINED)
                                {
                                    avaArmCount = currLoaderMap.GetHolderModuleAll().Count(
                                        h => h.ID.ModuleType == ModuleTypeEnum.ARM & h.WaferStatus == EnumSubsStatus.NOT_EXIST);
                                    if (avaArmCount == 0 & currHop != ModuleTypeEnum.ARM)
                                    {
                                        break;
                                    }

                                    //bufferFullLoaded = true;
                                    //foreach (var item in transferObjectIDs)
                                    //{
                                    //    if(transferPaths.TryGetValue(item, out hopQueue))
                                    //    {
                                    //        bufferFullLoaded &= hopQueue.Peek() == ModuleTypeEnum.BUFFER;
                                    //    }
                                    //}

                                    currHop = hopQueue.Peek();

                                    //if(currHop == ModuleTypeEnum.BUFFER&& !bufferedObjectIDs.Contains(to)&& !historyBufferedObjectIDs.Contains(to))
                                    //{
                                    //    bufferedObjectIDs.Add(to);
                                    //}
                                    //else 
                                    if (currHop != ModuleTypeEnum.BUFFER | bufferFullLoaded == true | loadWaferFullBuffered == true)
                                    {
                                        bufferedObjectIDs.Remove(to);
                                        //currHop = hopQueue.Dequeue();
                                        currHop = hopQueue.Peek();
                                        ModuleTypeEnum nextHop;
                                        if (hopQueue.Count > 1)
                                        {
                                            // Queue empty. Transfer done.
                                            //nextHop = hopQueue.Peek();
                                            nextHop = hopQueue.ElementAt(1);
                                        }
                                        else
                                        {
                                            nextHop = ModuleTypeEnum.UNDEFINED;
                                        }
                                        if (nextHop != ModuleTypeEnum.UNDEFINED)
                                        {
                                            var hopMap = (LoaderMap)currLoaderMap.Clone();
                                            HolderModuleInfo targetHolder = null;
                                            //var handlingArm = Loader.ModuleManager.FindModule<IARMModule>(ModuleTypeEnum.ARM, i + 1);
                                            TransferObject currSubObj = hopMap.GetSubstrateByGUID(to);
                                            if (nextHop == ModuleTypeEnum.SLOT)
                                            {
                                                var currHolderModule = hopMap.GetHolderModuleAll()
                                                    .FirstOrDefault(h =>
                                                    {
                                                        if (h.Substrate != null)
                                                        {
                                                            return h.Substrate.ID.Value == to;
                                                        }
                                                        else
                                                        {
                                                            return false;
                                                        }
                                                    });
                                                targetHolder = hopMap.GetHolderModuleAll().FirstOrDefault(
                                                    h => h.ID.ModuleType == nextHop & h.WaferStatus == EnumSubsStatus.NOT_EXIST & h.ID == destTO.CurrHolder);
                                            }
                                            else if (nextHop == ModuleTypeEnum.CHUCK)
                                            {
                                                targetHolder = hopMap.GetHolderModuleAll().FirstOrDefault(h => h.ID.Label == destTO.CurrHolder.Label);
                                            }
                                            else if (nextHop == ModuleTypeEnum.BUFFER)
                                            {
                                                if (destTO.CurrHolder.ModuleType == ModuleTypeEnum.BUFFER)
                                                {
                                                    targetHolder = hopMap.GetHolderModuleAll().FirstOrDefault(h => h.ID.Label == destTO.CurrHolder.Label);
                                                }
                                                else
                                                {
                                                    targetHolder = hopMap.GetHolderModuleAll().FirstOrDefault(h => h.ID.ModuleType == nextHop & h.WaferStatus == EnumSubsStatus.NOT_EXIST);
                                                }

                                            }
                                            else
                                            {
                                                if (destTO.CurrHolder.ModuleType == ModuleTypeEnum.FIXEDTRAY)
                                                {
                                                    if (nextHop == ModuleTypeEnum.ARM)
                                                    {
                                                        targetHolder = hopMap.GetHolderModuleAll().FirstOrDefault(h => h.ID.ModuleType == nextHop & h.WaferStatus == EnumSubsStatus.NOT_EXIST);
                                                    }
                                                    else if (nextHop == ModuleTypeEnum.PA)
                                                    {
                                                        targetHolder = hopMap.GetHolderModuleAll().FirstOrDefault(h => h.ID.ModuleType == nextHop & h.WaferStatus == EnumSubsStatus.NOT_EXIST && h.Enable);
                                                    }
                                                    else
                                                    {
                                                        targetHolder = hopMap.GetHolderModuleAll().FirstOrDefault(h => h.ID.Label == destTO.CurrHolder.Label);
                                                    }
                                                }
                                                else
                                                {
                                                    if (nextHop == ModuleTypeEnum.PA && currSubObj.CurrPos.ModuleType == ModuleTypeEnum.PA)
                                                    {
                                                        targetHolder = hopMap.GetHolderModuleAll().FirstOrDefault(h => h.ID.ModuleType == nextHop & h.WaferStatus == EnumSubsStatus.NOT_EXIST & h.ID.Index == currSubObj.CurrPos.Index && h.Enable);
                                                    }
                                                    else
                                                    {
                                                        targetHolder = hopMap.GetHolderModuleAll().FirstOrDefault(h => h.ID.ModuleType == nextHop & h.WaferStatus == EnumSubsStatus.NOT_EXIST && h.Enable);
                                                    }
                                                    if (DestinationModule.ModuleType == nextHop)
                                                    {
                                                        targetHolder = hopMap.GetHolderModuleAll().FirstOrDefault(h => h.ID.ModuleType == nextHop & h.WaferStatus == EnumSubsStatus.NOT_EXIST
                                                        && h.ID.Index == DestinationModule.Index);
                                                    }
                                                }
                                            }

                                            if (targetHolder != null)    // Check availability
                                            {
                                                if (targetHolder.WaferStatus == EnumSubsStatus.NOT_EXIST)
                                                {
                                                    var currHolderModule = hopMap.GetHolderModuleAll()
                                                        .FirstOrDefault(h =>
                                                        {
                                                            if (h.Substrate != null)
                                                            {
                                                                return h.Substrate.ID.Value == to;
                                                            }
                                                            else
                                                            {
                                                                return false;
                                                            }
                                                        });
                                                    if (currHolderModule != null)
                                                    {
                                                        prevHop = hopQueue.Dequeue();
                                                        MoveTO(targetHolder, currHolderModule, bManual: true);
                                                        currHop = hopQueue.Peek();
                                                        //maps.Add(hopMap);
                                                        maps = AddMap(targetHolder, currHolderModule, ref hopMap, maps);
                                                        currLoaderMap = (LoaderMap)hopMap.Clone();
                                                    }
                                                    if (hopQueue.Count == 2)
                                                    {
                                                        // Queue empty. Transfer done.
                                                        hopMap = (LoaderMap)currLoaderMap.Clone();
                                                        var destHop = hopQueue.ElementAt(1);
                                                        var destHolder = hopMap.GetHolderModuleAll().First(h => h.ID.Label == destTO.CurrHolder.Label);
                                                        avaArmCount = currLoaderMap.GetHolderModuleAll().Count(h => h.ID.ModuleType == ModuleTypeEnum.ARM & h.WaferStatus == EnumSubsStatus.NOT_EXIST);
                                                        if (avaArmCount == 0)
                                                        {

                                                        }
                                                        if (destHolder.WaferStatus == EnumSubsStatus.EXIST & avaArmCount > 0)      // Substrate already exist on destination holder
                                                        {
                                                            Queue<ModuleTypeEnum> swapQueue;
                                                            if (transferPaths.TryGetValue(destHolder.Substrate.ID.Value, out swapQueue))
                                                            {
                                                                var destSubsID = destHolder.Substrate.ID.Value;

                                                                currHop = swapQueue.Dequeue();
                                                                if (hopQueue.Count > 0)
                                                                {
                                                                    nextHop = swapQueue.Peek();
                                                                    targetHolder = hopMap.GetHolderModuleAll().FirstOrDefault(h => h.ID.ModuleType == nextHop & h.WaferStatus == EnumSubsStatus.NOT_EXIST);
                                                                    if (targetHolder != null & beSwappedObjectIDs.Exists(o => o == destSubsID))
                                                                    {
                                                                        beSwappedObjectID = beSwappedObjectIDs.First(o => o == destSubsID);
                                                                        //prevHop = hopQueue.Dequeue();
                                                                        MoveTO(targetHolder, destHolder, bManual: true);
                                                                        //currHop = hopQueue.Peek();
                                                                        //maps.Add(hopMap);
                                                                        maps = AddMap(targetHolder, currHolderModule, ref hopMap, maps);
                                                                        currLoaderMap = (LoaderMap)hopMap.Clone();
                                                                    }
                                                                    else
                                                                    {
                                                                        var Index1 = hopQueue.ElementAt(1);
                                                                        var index2 = hopQueue.ElementAt(0);
                                                                        if (Index1 == ModuleTypeEnum.CHUCK & index2 == ModuleTypeEnum.ARM)
                                                                        {
                                                                            targetHolder = hopMap.GetHolderModuleAll().FirstOrDefault(h => h.ID.ModuleType == nextHop & h.ID.Index == 1);
                                                                            if (targetHolder != null & beSwappedObjectIDs.Exists(o => o == destSubsID))
                                                                            {
                                                                                beSwappedObjectID = beSwappedObjectIDs.First(o => o == destSubsID);
                                                                                prevHop = hopQueue.Dequeue();
                                                                                MoveTO(targetHolder, destHolder, bManual: true);
                                                                                currHop = hopQueue.Peek();
                                                                                //maps.Add(hopMap);
                                                                                maps = AddMap(targetHolder, currHolderModule, ref hopMap, maps);
                                                                                currLoaderMap = (LoaderMap)hopMap.Clone();
                                                                            }
                                                                        }
                                                                        //LoggerManager.Error($"Map slicing error. No available target for substrate(@{destHolder.ID.Label}). Target holder is {nextHop}");
                                                                        //return null;
                                                                    }
                                                                }

                                                            }
                                                        }
                                                    }
                                                    if (currHop == ModuleTypeEnum.BUFFER && !bufferedObjectIDs.Contains(to) && !historyBufferedObjectIDs.Contains(to))
                                                    {
                                                        bufferedObjectIDs.Add(to);
                                                    }
                                                }
                                                // Swap scenario
                                                else if (targetHolder.WaferStatus == EnumSubsStatus.EXIST & targetHolder.Substrate.CurrHolder.ModuleType != ModuleTypeEnum.ARM)
                                                {
                                                    var swapTarget = hopMap.GetSubstrateByGUID(targetHolder.Substrate.ID.Value);
                                                    var currHolderModule = hopMap.GetHolderModuleAll()
                                                            .FirstOrDefault(h =>
                                                            {
                                                                if (h.Substrate != null)
                                                                {
                                                                    return h.Substrate.ID.Value == targetHolder.Substrate.ID.Value;
                                                                }
                                                                else
                                                                {
                                                                    return false;
                                                                }
                                                            });
                                                    Queue<ModuleTypeEnum> swapHopQueue;

                                                    if (transferPaths.TryGetValue(targetHolder.Substrate.ID.Value, out swapHopQueue))
                                                    {
                                                        if (swapHopQueue.Count > 0)
                                                        {
                                                            // Queue empty. Transfer done.
                                                            nextHop = swapHopQueue.Peek();
                                                            avaArmCount = currLoaderMap.GetHolderModuleAll().Count(h => h.ID.ModuleType == ModuleTypeEnum.ARM & h.WaferStatus == EnumSubsStatus.NOT_EXIST);
                                                            if (avaArmCount == 0 && nextHop == ModuleTypeEnum.CHUCK)
                                                            {
                                                                break;
                                                            }
                                                            if (nextHop == targetHolder.Substrate.CurrHolder.ModuleType)
                                                            {
                                                                LoggerManager.Debug($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} : Curr. Hop = {targetHolder.Substrate.CurrHolder.ModuleType}, Next hop = {nextHop}. Dequeuing current hop.");
                                                                nextHop = swapHopQueue.Dequeue();
                                                                nextHop = swapHopQueue.Peek();

                                                                LoggerManager.Debug($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} : Continue to next hop({nextHop}).");
                                                            }

                                                        }
                                                        else
                                                        {
                                                            nextHop = ModuleTypeEnum.UNDEFINED;
                                                        }
                                                    }
                                                    targetHolder = hopMap.GetHolderModuleAll().FirstOrDefault(
                                                        h => h.ID.ModuleType == nextHop & h.WaferStatus == EnumSubsStatus.NOT_EXIST);
                                                    var destSubsID = currHolderModule.Substrate.ID.Value;
                                                    if (targetHolder != null & beSwappedObjectIDs.Exists(o => o == destSubsID))
                                                    {
                                                        beSwappedObjectID = beSwappedObjectIDs.First(o => o == destSubsID);
                                                        MoveTO(targetHolder, currHolderModule, bManual: true);

                                                        //maps.Add(hopMap);
                                                        maps = AddMap(targetHolder, currHolderModule, ref hopMap, maps);
                                                        currLoaderMap = (LoaderMap)hopMap.Clone();
                                                    }
                                                }
                                                else
                                                {
                                                    targetHolder = hopMap.GetHolderModuleAll().FirstOrDefault(h => h.ID.ModuleType == nextHop & h.WaferStatus == EnumSubsStatus.EXIST);
                                                    if (targetHolder != null)
                                                    {
                                                        // Swap scenario
                                                        if (targetHolder.WaferStatus == EnumSubsStatus.EXIST & targetHolder.Substrate.CurrHolder.ModuleType != ModuleTypeEnum.ARM)
                                                        {
                                                            var swapTarget = hopMap.GetSubstrateByGUID(targetHolder.Substrate.ID.Value);
                                                            var currHolderModule = hopMap.GetHolderModuleAll()
                                                                    .FirstOrDefault(h =>
                                                                    {
                                                                        if (h.Substrate != null)
                                                                        {
                                                                            return h.Substrate.ID.Value == targetHolder.Substrate.ID.Value;
                                                                        }
                                                                        else
                                                                        {
                                                                            return false;
                                                                        }
                                                                    });
                                                            Queue<ModuleTypeEnum> swapHopQueue;

                                                            if (transferPaths.TryGetValue(targetHolder.Substrate.ID.Value, out swapHopQueue))
                                                            {
                                                                if (swapHopQueue.Count > 0)
                                                                {
                                                                    // Queue empty. Transfer done.

                                                                    nextHop = swapHopQueue.Peek();
                                                                }
                                                                else
                                                                {
                                                                    nextHop = ModuleTypeEnum.UNDEFINED;
                                                                }
                                                            }
                                                            targetHolder = hopMap.GetHolderModuleAll().FirstOrDefault(
                                                                h => h.ID.ModuleType == nextHop & h.WaferStatus == EnumSubsStatus.NOT_EXIST);
                                                            if (targetHolder != null)
                                                            {
                                                                MoveTO(targetHolder, currHolderModule, bManual: true);
                                                                //maps.Add(hopMap);
                                                                maps = AddMap(targetHolder, currHolderModule, ref hopMap, maps);
                                                                currLoaderMap = (LoaderMap)hopMap.Clone();
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                LoggerManager.Error($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} : Map slicing error. No available target for substrate(ID:{to}). Target holder is {nextHop}", isLoaderMap: true);
                                                LoggerManager.Debug($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} : LoaderJobViewList.Clear.");
                                                Loader.LoaderJobViewList.Clear();
                                                return null;
                                            }
                                        }
                                        // No more hop
                                        else
                                        {
                                            if (hopQueue.Count > 0)
                                            {
                                                // Queue empty. Transfer done.
                                                nextHop = hopQueue.Dequeue();
                                            }
                                        }
                                        //bufferFullLoaded = false;
                                    }
                                    // On buffer state
                                    else
                                    {
                                        if (currHop == ModuleTypeEnum.BUFFER)
                                        {
                                            bufferedObjectIDs.Add(to);
                                        }

                                        //bufferFullLoaded = true;
                                    }
                                }
                                // No more hop
                                else
                                {

                                }
                            }
                        }

                        // No available arm. Pending unloaded holders.
                        if (avaArmCount == 0)
                        {
                            if (beSwappedObjectIDs.Exists(o => o == beSwappedObjectID))
                            {
                                transferObjectIDs.Add(beSwappedObjectID);
                                beSwappedObjectIDs.Remove(beSwappedObjectID);
                            }

                            foreach (var item in transferObjectIDs)
                            {
                                if (transferPaths.TryGetValue(item, out hopQueue))
                                {
                                    if (hopQueue.Count > 0 && hopQueue.Peek() != ModuleTypeEnum.ARM)
                                    {
                                        pendingPaths.Add(item);
                                    }
                                }
                            }
                            foreach (var item in pendingPaths)
                            {
                                transferObjectIDs.Remove(item);
                            }
                        }
                        foreach (var duplicatedItem in pendingPaths)
                        {
                            transferObjectIDs.Remove(duplicatedItem);
                        }
                        foreach (var bufferedItem in bufferedObjectIDs)
                        {
                            transferObjectIDs.Remove(bufferedItem);
                        }
                        foreach (var transferredItem in transferredObjectIDs)
                        {
                            transferObjectIDs.Remove(transferredItem);
                        }

                        // All TO has been transferred.
                        if (transferredObjectIDs.Count == transferPaths.Count)
                        {
                            //LoggerManager.LoaderMapLog($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} : Completed.", basicLog: false);
                            toRemain = false;
                        }

                    }


                    if (loaderJobList.Count() > 0)
                    {
                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            this.Loader.LoaderJobViewList.Clear();
                            foreach (var job in loaderJobList)
                            {
                                this.Loader.LoaderJobViewList.Add(job);
                            }
                            Loader.LoaderJobSorting();

                        });
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} : Map slicing error. Err = {err.Message}", isLoaderMap: true);
                LoggerManager.Debug($"{GetType().Name}.{MethodBase.GetCurrentMethod().Name} : LoaderJobViewList.Clear.");
                Loader.LoaderJobViewList.Clear();
            }

            return maps;
        }


        private List<LoaderMap> AddMap(HolderModuleInfo target, HolderModuleInfo curr, ref LoaderMap hopmap, List<LoaderMap> maps)
        {
            LoaderMap closeMap = new LoaderMap();
            CassetteModuleInfo closeTarget = new CassetteModuleInfo();
            bool addCloseMap = false;
            var TargetModuleID = target.ID;
            HolderModuleInfo info = new HolderModuleInfo();
            try
            {
                foreach (var item in hopmap.CassetteModules)
                {
                    // 이전 맵으로 인해 close 상태가 되어버린 경우 ignore 만들기
                    if (item.FoupCoverState == ProberInterfaces.Foup.FoupCoverStateEnum.CLOSE)
                    {
                        item.FoupCoverState = ProberInterfaces.Foup.FoupCoverStateEnum.IGNORE;
                        LoggerManager.Debug("FoupCoverState: IGNORE");
                    }
                }

                maps.Add(hopmap);
                closeMap = (LoaderMap)hopmap.Clone();

                if (Loader.LoaderMaster.GetIsAlwaysCloseFoupCover())
                {
                    // analyze close req
                    // arm to slot 뒤에 닫기 // slot to arm 뒤에 닫기
                    if(curr.ID.ModuleType == ModuleTypeEnum.SLOT || TargetModuleID.ModuleType == ModuleTypeEnum.SLOT)
                    {
                        // 이 slot 이 속한 cassette index 검색하기
                        foreach (var dstCstInfo in closeMap.CassetteModules)
                        {
                            foreach (var item in dstCstInfo.SlotModules)
                            {
                                if (item.ID == TargetModuleID || item.ID == curr.ID)
                                {
                                    closeTarget = dstCstInfo;
                                    addCloseMap = true;
                                }
                            }
                        }
                    }

                    if (addCloseMap)
                    {
                        // close cover maps 뒤에 추가.                    
                        //foreach (var cst in closeTargets)

                        var cstToClose = closeMap.CassetteModules.First(c => c.ID.Index == closeTarget.ID.Index);
                        // 1. Close 아닐때만 , close map 만들어주기
                        if (cstToClose.FoupCoverState != ProberInterfaces.Foup.FoupCoverStateEnum.CLOSE)
                        {
                            cstToClose.FoupCoverState = ProberInterfaces.Foup.FoupCoverStateEnum.CLOSE;
                            // 단, 이미 앞에서 close 맵을 추가했다면, 그맵은 삭제하고 현재 맵의 뒤로 추가.                                       
                            var isExistCloseMap = maps.Find(a => a.CassetteModules[closeTarget.ID.Index - 1].FoupCoverState == ProberInterfaces.Foup.FoupCoverStateEnum.CLOSE);
                            if (isExistCloseMap != null)
                            {
                                maps.Remove(isExistCloseMap);
                                this.loaderJobList.Remove(new LoaderJobViewData(closeTarget.ID, closeTarget.ID, TargetModuleID));
                            }
                        }

                        LoggerManager.Debug($"Cassette(#{cstToClose.ID.Index}) has been added to close cassette target list.");
                        this.loaderJobList.Add(new LoaderJobViewData(cstToClose.ID, cstToClose.ID, TargetModuleID));
                        maps.Add(closeMap);
                        // close map 동기화 시키는 코드 추가하기
                        hopmap = (LoaderMap)closeMap.Clone();

                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            LoggerManager.Debug($"Sliced map has been added. Map count = {maps.Count}");
            return maps;
        }
        private void MoveTO(HolderModuleInfo target, HolderModuleInfo curr, bool bManual = false)
        {
            var TargetModuleID = target.ID;
            if (TargetModuleID.ModuleType == ModuleTypeEnum.COGNEXOCR)
            {
                TargetModuleID = curr.ID;
            }

            if ((curr.ID.ModuleType == ModuleTypeEnum.ARM || TargetModuleID.ModuleType == ModuleTypeEnum.ARM) || (curr.ID.ModuleType == ModuleTypeEnum.CARDARM || TargetModuleID.ModuleType == ModuleTypeEnum.CARDARM))
            {
                this.loaderJobList.Add(new LoaderJobViewData(curr.ID, TargetModuleID, curr.Substrate.OriginHolder));

                if (curr.ID.ModuleType == ModuleTypeEnum.ARM || TargetModuleID.ModuleType == ModuleTypeEnum.ARM)
                {
                    LoggerManager.AddLoaderMapHolder($"{curr.ID.ModuleType}_TO_{target.ID.ModuleType}", curr.ID.Label, target.ID.Label, curr.Substrate.OriginHolder.Label, bManual);
                }

                LoggerManager.Debug($"real loaderJobViewList.Add( " +
                                  $"Source({curr.ID.ModuleType}.{curr.ID.Index}), " +
                                  $"Target({TargetModuleID.ModuleType}.{TargetModuleID.Index})" +
                                  $" )", isLoaderMap: true);
            }
            else
            {
                if (target.ID.ModuleType != ModuleTypeEnum.COGNEXOCR) //COGNEXOCR holder는 실제 이동은 하지 않는 경로이다. PA를 버퍼로 쓰기 위한 상태임
                {
                    LoggerManager.Debug($"LoaderMapSlicer JobviewList Add Error: Source({curr.ID.ModuleType}.{curr.ID.Index}), , Tagert:({TargetModuleID.ModuleType}.{TargetModuleID.Index})", isLoaderMap: true);
                    LoggerManager.AddLoaderMapHolder($"{curr.ID.ModuleType}_TO_{ModuleTypeEnum.ARM}", curr.ID.Label, "TEMP", curr.Substrate.OriginHolder.Label, bManual);
                    LoggerManager.AddLoaderMapHolder($"{ModuleTypeEnum.ARM}_TO_{target.ID.ModuleType}", "TEMP", target.ID.Label, curr.Substrate.OriginHolder.Label, bManual);
                }
            }

            target.Substrate = (TransferObject)curr.Substrate.Clone();
            target.Substrate.PrevHolder = curr.Substrate.CurrHolder;
            target.Substrate.PrevPos = curr.Substrate.CurrPos;
            target.Substrate.CurrHolder = target.ID;
            curr.Substrate = null;
            target.WaferStatus = EnumSubsStatus.EXIST;
            curr.WaferStatus = EnumSubsStatus.NOT_EXIST;
        }
        private List<TransferObject> GenerateTransferPath(TransferObject src, TransferObject dst, IARMModule assignedarm, IBufferModule assignedbuffer)
        {
            List<TransferObject> path = new List<TransferObject>();

            if (src.CurrPos != dst.CurrPos)
            {
                while (src.CurrPos == dst.CurrPos)
                {




                }
            }
            else
            {
                LoggerManager.Debug($"Source and destination transfer object is same. Src = {src.CurrPos}, Dst = {dst.CurrPos}");
            }

            return path;
        }
    }


}
