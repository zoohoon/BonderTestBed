using System;
using System.Collections.Generic;
using System.Linq;

namespace GemActBehavior
{
    using Autofac;
    using LogModule;
    using ProberInterfaces;
    using ProberErrorCode;
    using SecsGemServiceInterface;
    using LoaderBase;
    using LoaderParameters;
    using ProberInterfaces.Foup;
    using LoaderBase.Communication;
    using System.Threading;
    using NotifyEventModule;
    using ProberInterfaces.Event;
    using XGEMWrapper;
    using ProberInterfaces.GEM;

    public class ProcessedWithCarrier : GemActBehaviorBase
    {
        public ProcessedWithCarrier()
        {
        }

        public override EventCodeEnum ExcuteCarrierCommander(CarrierActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                byte HAACK = 0x02;
                ILoaderSupervisor LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();

                ProceedWithCarrierReqData reqData = actReqData as ProceedWithCarrierReqData;
                if (reqData != null)
                {
                    HAACK = 0x04;
                    this.GEMModule().S3F17SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HAACK, actReqData.Count);

                    if (reqData.DataID == 0)
                    {
                        ///Load the cassette onto the docking port and proceed to the scan cassette.
                        ILoaderModule LoaderModule = LoaderMaster.Loader;
                        ActiveProcessActReqData activeData = new ActiveProcessActReqData();
                        activeData.FoupNumber = reqData.PTN;
                        // [STM_CATANIA] CarrierID가 없어서 LotID를 사용
                        activeData.LotID = reqData.LotID;

                        retVal = LoaderMaster.ActiveProcess(activeData);

                        var Cassette = LoaderMaster.Loader.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, actReqData.PTN);
                        Cassette.SetReservedScanState();

                        retVal = this.FoupOpModule().FoupControllers[actReqData.PTN - 1].Execute(new FoupLoadCommand());

                        if (retVal == EventCodeEnum.NONE)
                        {
                            //Cassette.SetNoReadScanState();
                            //bool scanWaitFlag = false;

                            if (Cassette.ScanState == CassetteScanStateEnum.ILLEGAL || Cassette.ScanState == CassetteScanStateEnum.NONE
                                || Cassette.ScanState == CassetteScanStateEnum.RESERVED)
                            {
                                LoaderMaster.Loader.DoScanJob(actReqData.PTN);
                            }

                            if(LoaderMaster.GetIsAlwaysCloseFoupCover())
                            {
                                // foup cover close / open 판단하는 함수
                                LoaderModule.CloseFoupCoverFunc(Cassette, false);
                            }
                            
                        }
                        else
                        {
                            Cassette.SetNoReadScanState();
                        }
                    }
                    else if (reqData.DataID == 1)
                    {
                        AssignWaferIDMap waferIds = new AssignWaferIDMap();
                        waferIds.FoupNumber = actReqData.PTN;

                        for (int i = 0; i < reqData.SlotMap.Length; i++)
                        {
                            waferIds.WaferIDs.Add(reqData.SlotMap[i]);

                        }
                        LoaderMaster.SetWaferIDs(waferIds);

                    }
                }
                
                retVal = EventCodeEnum.NONE;

                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
    }

    public class ReleaseCarrier : GemActBehaviorBase
    {
        public ReleaseCarrier()
        {
        }

        public override EventCodeEnum ExcuteCarrierCommander(CarrierActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                CarrieActReqData reqData = actReqData as CarrieActReqData;
                if (reqData != null)
                {
                    byte HAACK = 0x02;
                    bool canExcute = false;


                    if(actReqData.PTN > 0 && actReqData.PTN <= SystemModuleCount.ModuleCnt.FoupCount)
                    {
                        ILoaderModule Loader = this.GetLoaderContainer().Resolve<ILoaderModule>();
                        if (Loader != null)
                        {
                            if (Loader.IsFoupUnload(actReqData.PTN))
                            {
                                var e84controller = Loader.LoaderMaster.E84Module().GetE84Controller(actReqData.PTN, E84OPModuleTypeEnum.FOUP);
                                if (e84controller != null)
                                {
                                    var e84mode = e84controller.CommModule.RunMode;
                                    if (e84mode == E84Mode.AUTO)
                                    {
                                        canExcute = true;
                                    }
                                    else
                                    {
                                        LoggerManager.Debug($"ReleaseCarrier Cannot Excute. e84mode is {e84mode}.");
                                    }
                                }
                                else
                                {
                                    //e84 사용 안할 시에는 slot동작 중인지만 확인.
                                    canExcute = true;
                                }
                            }
                            else
                            {
                                LoggerManager.Debug($"ReleaseCarrier Cannot Excute. Foup{actReqData.PTN} is Running.");
                            }

                        }
                        else
                        {
                            LoggerManager.Debug($"ReleaseCarrier Cannot Excute. Loader is null.");
                        }
                                            
                    }
                    else 
                    {
                        LoggerManager.Debug($"ReleaseCarrier Cannot Excute. foupnum:{actReqData.PTN}");
                    }

                    if (canExcute)
                    {
                        HAACK = 0x04;
                    }
                    else
                    {
                        HAACK = 0x02;
                    }

                    this.GEMModule().S3F17SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HAACK, actReqData.Count);

                    if (canExcute)
                    {
                        this.FoupOpModule().FoupControllers[actReqData.PTN - 1].SetLock(false);
                        retVal = this.FoupOpModule().FoupControllers[actReqData.PTN - 1].Execute(new FoupUnloadCommand());                        
                    }
                    else
                    {                        
                        LoggerManager.Debug($"ReleaseCarrier Rejected. Return:{HAACK}");
                    }

                    




                    //#Hynix_Merge: Dev_Integrated_Hynix는 아래와 같은 코드 였는데 뭐가 맞는지 확인 필요 @한송희
                    //if (reqData.DataID == 0)
                    //{
                    //    ///Load the cassette onto the docking port and proceed to the scan cassette.                      
                    //    retVal = this.FoupOpModule().FoupControllers[actReqData.PTN - 1].Execute(new FoupUnloadCommand());
                    //}
                    //else if (reqData.DataID == 1)
                    //{
                    //    ///Input the lot name from WaferID.
                    //}

                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        public override EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                UnDockReqData reqData = actReqData as UnDockReqData;
                if (reqData != null)
                {
                    this.FoupOpModule().FoupControllers[reqData.LoadPortNumber - 1].SetLock(false);
                    retVal = this.FoupOpModule().FoupControllers[reqData.LoadPortNumber - 1].Execute(new FoupUnloadCommand());

                }

                DockFoupActReqData reqData2 = actReqData as DockFoupActReqData;
                if (reqData2 != null)
                {
                    EventCodeEnum ackVal = SetAck(remoteActReqData: actReqData);
                    ILoaderSupervisor LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                    if(LoaderMaster != null)
                    {
                        if(LoaderMaster.DynamicMode == DynamicModeEnum.DYNAMIC)
                        {
                            if (LoaderMaster.ActiveLotInfos[reqData2.FoupNumber - 1].State == LotStateEnum.Running)
                            {
                                //LoaderMaster.ActiveLotInfos[reqData2.FoupNumber - 1].State = LotStateEnum.Abort;
                                LoaderMaster.ActiveLotInfos[reqData2.FoupNumber - 1].ResevationState = FoupReservationEnum.RESERVE;
                            }
                            else
                            {
                                LoaderMaster.ActiveLotInfos[reqData2.FoupNumber - 1].FoupUnLoad();
                            }
                        }
                    }
                    //this.FoupOpModule().FoupControllers[reqData2.FoupNumber - 1].SetLock(false);
                    //retVal = this.FoupOpModule().FoupControllers[reqData2.FoupNumber - 1].Execute(new FoupUnloadCommand());

                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        public override EventCodeEnum SetAck(CarrierActReqData carrierActReqData = null, RemoteActReqData remoteActReqData = null, ISecsGemServiceHost gemServiceHost = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (remoteActReqData != null)
                {
                    var actReqData = remoteActReqData as DockFoupActReqData;
                    if (actReqData != null)
                    {
                        this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, (byte)EnumGemHCACK.WILL_BE_PERFORMED, actReqData.Count);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class ProcessedWithCell : GemActBehaviorBase
    {
        public ProcessedWithCell()
        {

        }
        public override EventCodeEnum ExcuteCarrierCommander(CarrierActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            byte HAACK = 0x02;
            try
            {
                ProceedWithCellSlotActReqData reqData = actReqData as ProceedWithCellSlotActReqData;
                if (reqData != null)
                {
                    ILoaderSupervisor LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                    ILoaderModule LoaderModule = LoaderMaster.Loader;
                    ActiveProcessActReqData activeData = new ActiveProcessActReqData();

                    activeData.FoupNumber = reqData.PTN;

                    if (reqData.DataID == 1)
                    {
                        // STM_CATANIA에서는 현재 CarrirerID를 사용하지 않기 때문에 직접 CarrirerID 대신 LOTID를 받아서 처리함.
                        activeData.LotID = reqData.LOTID;
                    }
                    else
                    {
                        activeData.LotID = this.GEMModule().GetPIVContainer().GetLotIDAtFoupInfos(activeData.FoupNumber, reqData.CarrierID);
                    }
                    
                    //string cellData=reqData.CellMap.Replace("1", "0");
                    string cellData = reqData.CellMap;
                    //cellData = cellData.Replace("1", "0");

                    activeData.UseStageNumbers_str = cellData;
                    this.GEMModule().GetPIVContainer().SetFoupInfo(foupindex: reqData.PTN, stagelist: cellData);
                    var array = cellData.ToArray();

                    for (int index = 0; index < array.Length; index++)
                    {
                        if (array[index] == '1')
                            activeData.UseStageNumbers.Add(index + 1);// Data : [A] ListOfStagesToUse
                    }
                    retVal = LoaderMaster.ActiveProcess(activeData);
                    
                    string devname = null;
                    var commmanager = this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
                    if (activeData.UseStageNumbers.Count != 0)
                    {
                        if (commmanager.GetProxy<IStageSupervisorProxy>(activeData.UseStageNumbers.First()) != null)
                        {
                            devname = commmanager.GetProxy<IStageSupervisorProxy>(activeData.UseStageNumbers.First()).GetDeviceName();
                            LoaderMaster.DeviceManager().SetPMIDevice(activeData.FoupNumber, devname);
                        }
                    }


                    //string slotData = reqData.SlotMap.Replace("1", "0");
                    //slotData = slotData.Replace("3", "1");
                    string slotData = reqData.SlotMap;

                    var slotArray = slotData.ToArray();
                    List<int> slots = new List<int>();

                    for (int index = 0; index < slotArray.Length; index++)
                    {
                        if (slotArray[index] == '1')
                        {
                            slots.Add(index + 1);
                        }
                    }

                    LoaderMaster.SelectSlot(reqData.PTN, activeData.LotID, slots);
                    LoaderMaster.VerifyParam(reqData.PTN, activeData.LotID);

                    HAACK = 0x00;

                    this.GEMModule().S3F17SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HAACK, actReqData.Count);



                    PIVInfo pivinfo = new PIVInfo(listofstage: LoaderMaster.Loader.Foups[activeData.FoupNumber - 1].AllocatedCellInfo, 
                        foupnumber: activeData.FoupNumber, receipeid: devname, lotid: activeData.LotID);
                    System.Threading.SemaphoreSlim semaphore = new System.Threading.SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(FoupAllocatedEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    semaphore.Wait();

                    retVal = EventCodeEnum.NONE;

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

    }

    public class ProceedWithSlot : GemActBehaviorBase
    {
        public ProceedWithSlot()
        {
        }

        EventCodeEnum CheckValidation(CarrierActReqData actReqData, out string errormsg)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            string errmsg = "";
            try
            {
                var reqData = actReqData as ProceedWithSlotReqData;
                //scan data 가지고 오기 
                ILoaderModule Loader = this.GetLoaderContainer().Resolve<ILoaderModule>();
                var realslotdatas = Loader.GetLoaderInfo().StateMap.CassetteModules[reqData.PTN - 1].SlotModules.ToList();
             
               
                if(reqData.SlotMap.Count() != SystemModuleCount.ModuleCnt.SlotCount) 
                {
                    retVal = EventCodeEnum.SLOT_VARIFY_ERROR;
                    errmsg = $"slot num not same. reqData.SlotMap.Count:{reqData.SlotMap.Count()}, ModuleCnt.SlotCount:{SystemModuleCount.ModuleCnt.SlotCount}.";
                }
                else
                {
                    for (int i = 0; i < SystemModuleCount.ModuleCnt.SlotCount; i++)
                    {
                        // scan data는       wafer exist: 3, wafer not exist: 1
                        // proceedwithslot은 use: 3, not use: 1
                        if (reqData.SlotMap[i] == "3")
                        {
                            if (realslotdatas[SystemModuleCount.ModuleCnt.SlotCount - i - 1]?.Substrate?.WaferState == EnumWaferState.UNPROCESSED)
                            {
                                retVal = EventCodeEnum.NONE;
                            }
                            else
                            {
                                retVal = EventCodeEnum.SLOT_VARIFY_ERROR;
                                errmsg = $"slot(num:{i + 1}) not matched. scan data:{realslotdatas[SystemModuleCount.ModuleCnt.SlotCount - i - 1].WaferStatus}, reqData.SlotMap:{reqData.SlotMap[i]}";
                                break;
                            }

                        }
                        else
                        {
                            retVal = EventCodeEnum.NONE;
                        }

                    }
                }

               

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                errormsg = errmsg;
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"ProceedWithSlot Failed. Foup({actReqData.PTN}) errorcode:{retVal} errmsg:{errormsg}");
                }                               
            }
            return retVal;

        }

        public override EventCodeEnum ExcuteCarrierCommander(CarrierActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var reqData =  actReqData as ProceedWithSlotReqData;
                byte HAACK = 0x02;
                //bool canExcute = false;
                string errorlog;
                if (this.CheckValidation(reqData, out errorlog) == EventCodeEnum.NONE)
                {
                    AssignWaferIDMap waferIds = new AssignWaferIDMap();
                    waferIds.FoupNumber = reqData.PTN;

                    for (int i = 0; i < reqData.OcrMap.Length; i++)
                    {
                        waferIds.WaferIDs.Add(reqData.OcrMap[i]);
                    }

                    ILoaderSupervisor LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                    retVal = LoaderMaster.SetWaferIDs(waferIds);
                    if(retVal == EventCodeEnum.NONE)
                    {
                        HAACK = 0x00;
                    }
                    else
                    {
                        HAACK = 0x06;// command performed with errors
                    }

                }
                else
                {
                    HAACK = 0x03;//invalid data or argument
                }

             
                this.GEMModule().S3F17SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HAACK, actReqData.Count);

                PIVInfo pivinfo = new PIVInfo(foupnumber: reqData.PTN);
                SemaphoreSlim semaphore = new SemaphoreSlim(0);                                
                if (retVal == EventCodeEnum.NONE)
                {
                    this.EventManager().RaisingEvent(typeof(SlotVarifyDoneEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                }
                else
                {
                    this.EventManager().RaisingEvent(typeof(SlotVarifyFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                }
                semaphore.Wait();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class CarrierChangeAccessMode : GemActBehaviorBase
    {
        public CarrierChangeAccessMode()
        {
        }

        public override EventCodeEnum ExcuteCarrierCommander(CarrierActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                CarrierAccesModeReqData reqData = actReqData as CarrierAccesModeReqData;
                if (reqData != null)
                {
                    List<CarrierChangeAccessModeResult> result = new List<CarrierChangeAccessModeResult>();
                    if (reqData.LoadPortList.Count == 0)
                    {
                        //모든 foup 에 적용.
                        if (this.FoupOpModule().FoupControllers != null)
                        {
                            foreach (var foupcontroller in this.FoupOpModule().FoupControllers)
                            {
                                reqData.LoadPortList.Add(foupcontroller.FoupModuleInfo.FoupNumber);
                            }
                        }
                    }

                    for (int index = 1; index <= reqData.LoadPortList.Count; index++)
                    {
                        var retResult = SetLoadPortMode(reqData.LoadPortList[index - 1], reqData.AccessMode);
                        result.Add(retResult);
                    }
                    byte CACK = 0;
                    this.GEMModule().S3F27SendAck(reqData.ObjectID, reqData.Stream, reqData.Function, reqData.Sysbyte, CACK, reqData.Count, result);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private CarrierChangeAccessModeResult SetLoadPortMode(int loadportnumber, int mode)
        {
            CarrierChangeAccessModeResult retVal = new CarrierChangeAccessModeResult();
            try
            {
                var E84controller = this.E84Module().GetE84Controller(loadportnumber, E84OPModuleTypeEnum.FOUP);
                if(E84controller != null)
                {
                    var ret = E84controller.SetMode(mode);
                    if (ret == 0)
                    {
                        //Success
                        retVal.FoupNumber = loadportnumber;
                        retVal.ErrorCode = 0;

                    }
                    else
                    {
                        retVal.FoupNumber = loadportnumber;
                        retVal.ErrorCode = 50;
                        retVal.ErrText = "Load port can not change mode";
                    }
                }
                else
                {
                    retVal.FoupNumber = loadportnumber;
                    retVal.ErrorCode = 48;
                    retVal.ErrText = "Load port does not exist";
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }   
}
