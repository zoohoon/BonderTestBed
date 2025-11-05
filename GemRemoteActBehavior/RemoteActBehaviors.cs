using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GemActBehavior
{
    using Autofac;
    using LogModule;
    using ProberInterfaces;
    using ProberErrorCode;
    using SecsGemServiceInterface;
    using LoaderBase;
    using NotifyEventModule;
    using LoaderParameters;
    using ProberInterfaces.Command.Internal;
    using ProberInterfaces.Enum;
    using System.Threading;
    using ProberInterfaces.Event;
    using System.Runtime.InteropServices;
    using System.IO;
    using System.Xml;
    using ProberInterfaces.Foup;
    using LoaderServiceBase;
    using LoaderParameters.Data;
    using ProberInterfaces.Device;
    using XGEMWrapper;
    using ProberInterfaces.GEM;
    using LoaderBase.Communication;
    using System.Text.RegularExpressions;
    using System.Runtime.Serialization;

    /// <summary>
    /// S2F41
    /// </summary>

    public class AssignLot : GemActBehaviorBase
    {
        public AssignLot()
        {
        }

        public override EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ILoaderSupervisor LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                ILoaderModule LoaderModule = LoaderMaster.Loader;
                var reqData = actReqData as ActiveProcessActReqData;
                retVal = LoaderMaster.ActiveProcess(reqData);
                if (retVal == EventCodeEnum.NONE)
                {
                    byte HAACK = 0x04;
                    this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HAACK, 0);

                    PIVInfo pivinfo = new PIVInfo(foupnumber: reqData.FoupNumber);
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(LoadportActivatedEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    semaphore.Wait();


                    //var lockobj = LoaderModule.LoaderMaster.GetLoaderPIV().GetPIVDataLockObject();
                    //lock (lockobj)
                    //{
                    //    LoaderMaster.GetLoaderPIV().UpdateFoupInfo(reqData.FoupNumber);
                    //    this.GEMModule().SetEvent(this.GEMModule().GetEventNumberFormEventName(typeof(LoadportActivatedEvent).FullName));
                    //}
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ExcuteExcuter(RemoteActReqData actReqData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }
    public class AssignWaferID : GemActBehaviorBase
    {
        public AssignWaferID()
        {
        }

        public override EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ILoaderSupervisor LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();

                var reqData = actReqData as AssignWaferIDMap;
                if (reqData != null)
                {
                    LoaderMaster.SetWaferIDs(reqData);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ExcuteExcuter(RemoteActReqData actReqData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class DownloadDeviceToStages : GemActBehaviorBase
    {
        public DownloadDeviceToStages()
        {

        }

        public override EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                byte HAACK = 0x04;
                this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HAACK, actReqData.Count);

                ILoaderSupervisor LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();

                var reqData = actReqData as DownloadStageRecipeActReqData;
                object lockObj = this.DeviceManager().GetLockObject();
                lock (lockObj)
                {
                    var ret = this.DeviceManager().SetRecipeToStage(reqData);
                    if (reqData.LotID == null && reqData.FoupNumber < 0)
                    {
                        //nothing
                    }
                    else
                    {
                        retVal = LoaderMaster.SetRecipeToDevice(reqData);
                    }

                }
                LoggerManager.Debug($"[GEM COMMANDER] {reqData?.ActionType} - foup number {reqData?.FoupNumber} : ");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ExcuteExcuter(RemoteActReqData actReqData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class SetValueOfParameters : GemActBehaviorBase
    {
        public SetValueOfParameters()
        {

        }

        public override EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ILoaderSupervisor LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();

                byte HAACK = 0x04;
                this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HAACK, actReqData.Count);

                var reqData = actReqData as SetParameterActReqData;
                LoggerManager.Debug($"[GEM COMMANDER] {actReqData?.ActionType}");

                if (reqData.ParameterDic != null)
                {
                    NeedChangeParameterInDevice needChangeParameter = new NeedChangeParameterInDevice();
                    List<ElementParameterInfomation> elementParameterInfomations = new List<ElementParameterInfomation>();

                    needChangeParameter.FoupNumber = reqData.FoupNumber;
                    needChangeParameter.LOTID = reqData.LotID;
                    needChangeParameter.DeviceName = reqData.RecipeId;
                    foreach (var parameter in reqData.ParameterDic)
                    {
                        LoggerManager.Debug($"[GEM Commander] OnRemoteCommandAction - SET_PARAMETERS: {parameter.Key}");
                        switch (parameter.Key)
                        {
                            case "SET_TEMPERATURE"://"HOTTEMPERATURE_HOTTEMPERATURE":
                                                   //EX) 90 도면 090 으로 들어옴.
                                if (parameter.Value != "")
                                {
                                    double temp = Convert.ToDouble(parameter.Value);
                                    elementParameterInfomations.Add(new ElementParameterInfomation(
                                   "TempController.TempControllerDevParam.SetTemp", temp));
                                }
                                break;
                            case "PREHEATING_TIME"://"REPRE5":
                                if (parameter.Value != "")
                                {
                                    int soakingtime = Convert.ToInt32(parameter.Value) * 60;
                                    elementParameterInfomations.Add(new ElementParameterInfomation(
                                    "SoakingModule.SoakingDeviceFile.LotStartEventSoaking[3].SoakingTimeInSeconds", soakingtime));
                                }
                                break;
                            case "STOP_AT_FIRSTDIE"://"STOP_AT_FIRSTDIE":
                                if (parameter.Value != "")
                                {
                                    int parameterVal = Convert.ToInt32(parameter.Value);

                                    bool stopBeforeProbingFlag = (parameterVal == 0) ? false : true;
                                    elementParameterInfomations.Add(new ElementParameterInfomation(
                                      "LotOPModule.LotDeviceParam.StopOption.StopBeforeProbing", stopBeforeProbingFlag));
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    if (elementParameterInfomations.Count > 0)
                    {
                        needChangeParameter.ElementParameters = elementParameterInfomations;
                        this.DeviceManager().SetParameterForDevice(needChangeParameter);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ExcuteExcuter(RemoteActReqData actReqData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }
    //public class SelectCellWithSlot : GemActBehaviorBase
    //{

    //    public SelectCellWithSlot()
    //    {

    //    }

    //    public override EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
    //    {
    //        EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
    //        try
    //        {
    //            byte HAACK = 0x04;
    //            this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HAACK, actReqData.Count);

    //            SelectStageSlotActReqData reqData = actReqData as SelectStageSlotActReqData;
    //            if (reqData != null)
    //            {
    //                ILoaderSupervisor LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
    //                ILoaderModule LoaderModule = LoaderMaster.Loader;
    //                ActiveProcessActReqData activeData = new ActiveProcessActReqData();

    //                activeData.FoupNumber = reqData.PTN;

    //                activeData.LotID = this.GEMModule().GetPIVContainer().GetLotIDAtFoupInfos(activeData.FoupNumber, reqData.CarrierId);

    //                //string cellData=reqData.CellMap.Replace("1", "0");
    //                string cellData = reqData.CellMap;
    //                //cellData = cellData.Replace("1", "0");

    //                activeData.UseStageNumbers_str = cellData;
    //                this.GEMModule().GetPIVContainer().SetFoupInfo(foupindex: reqData.PTN, stagelist: cellData);
    //                var array = cellData.ToArray();

    //                for (int index = 0; index < array.Length; index++)
    //                {
    //                    if (array[index] == '1')
    //                        activeData.UseStageNumbers.Add(index + 1);// Data : [A] ListOfStagesToUse
    //                }
    //                retVal = LoaderMaster.ActiveProcess(activeData);

    //                var commmanager = this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
    //                if (activeData.UseStageNumbers.Count != 0)
    //                {
    //                    string devname = commmanager.GetStageSupervisorClient(activeData.UseStageNumbers.First()).GetDeviceName();

    //                    LoaderMaster.DeviceManager().SetPMIDevice(activeData.FoupNumber, devname);
    //                }


    //                //string slotData = reqData.SlotMap.Replace("1", "0");
    //                //slotData = slotData.Replace("3", "1");
    //                string slotData = reqData.SlotMap;

    //                var slotArray = slotData.ToArray();
    //                List<int> slots = new List<int>();

    //                for (int index = 0; index < slotArray.Length; index++)
    //                {
    //                    if (slotArray[index] == '1')
    //                    {
    //                        slots.Add(index + 1);
    //                    }
    //                }

    //                LoaderMaster.SelectSlot(reqData.PTN, activeData.LotID, slots);

    //                PIVInfo pIVInfo = new PIVInfo(foupnumber: reqData.PTN);
    //                SemaphoreSlim semaphore = new SemaphoreSlim(0);
    //                this.EventManager().RaisingEvent(typeof(StageSlotSelectedEvent).FullName, new ProbeEventArgs(this, semaphore, pIVInfo));
    //                semaphore.Wait();
    //                retVal = EventCodeEnum.NONE;

    //            }

    //        }
    //        catch (Exception err)
    //        {
    //            SemaphoreSlim semaphore = new SemaphoreSlim(0);
    //            this.EventManager().RaisingEvent(typeof(StageSlotSelectFailEvent).FullName, new ProbeEventArgs(this, semaphore));
    //            semaphore.Wait();
    //            LoggerManager.Exception(err);
    //        }
    //        return retVal;
    //    }
    //}

    public class DockFoup : GemActBehaviorBase
    {
        public DockFoup()
        {

        }

        public override EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                int foupnum = 0;

                var reqData1 = actReqData as DockFoupActReqData;
                if (reqData1 != null)
                {
                    foupnum = reqData1.FoupNumber;
                }

                var reqData2 = actReqData as CarrierIdentityData;
                if (reqData2 != null)
                {
                    foupnum = reqData2.PTN;
                }

                // Foup command - Load command execution.

                EventCodeEnum ackVal = EventCodeEnum.UNDEFINED;
                if (actReqData is DockFoupActReqData)
                {
                    ackVal = SetAck(remoteActReqData: actReqData);
                }
                else if (actReqData is CarrierIdentityData)
                {
                    ackVal = EventCodeEnum.NONE;
                    byte HAACK = 0x04;
                    this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HAACK, actReqData.Count);
                }

                if (ackVal == EventCodeEnum.GEM_COMMAND_PERFORM)
                {
                    ILoaderSupervisor LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                    ILoaderModule LoaderModule = LoaderMaster.Loader;
                    var modules = LoaderModule.ModuleManager;
                    var Cassette = LoaderModule.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, foupnum);

                    /// will execute cassette scan.
                    Cassette.SetReservedScanState();

                    retVal = this.FoupOpModule().GetFoupController(foupnum)
                         .Execute(new ProberInterfaces.Foup.FoupLoadCommand());
                    if (retVal == EventCodeEnum.NONE)
                    {

                        LoaderModule.ScanCount = 25;

                        //Cassette.SetNoReadScanState();
                        bool scanWaitFlag = false;

                        if (Cassette.ScanState == CassetteScanStateEnum.ILLEGAL || Cassette.ScanState == CassetteScanStateEnum.NONE
                            || Cassette.ScanState == CassetteScanStateEnum.RESERVED)
                        {
                            var scanRetVal = LoaderModule.DoScanJob(foupnum);
                            if (scanRetVal.Result == EventCodeEnum.NONE)
                            {
                                scanWaitFlag = true;

                            }
                        }
                        while (scanWaitFlag)
                        {

                            if (Cassette.ScanState == CassetteScanStateEnum.ILLEGAL || Cassette.ScanState == CassetteScanStateEnum.READ)
                            {
                                if (Cassette.ScanState == CassetteScanStateEnum.READ)
                                {
                                    StringBuilder scanstr = new StringBuilder();
                                    var slots = LoaderModule.GetLoaderInfo().StateMap.CassetteModules[foupnum - 1].SlotModules.ToList();
                                    if (slots != null)
                                    {
                                        slots.Sort((slotTarget1, slotTarget2) => slotTarget1.ID.Index.CompareTo(slotTarget2.ID.Index));
                                        //foreach (var slot in this.LoaderModule.GetLoaderInfo().StateMap.CassetteModules[reqData.FoupNumber - 1].SlotModules)
                                        foreach (var slot in slots)
                                        {
                                            if (slot.WaferStatus == EnumSubsStatus.EXIST)
                                            {
                                                scanstr.Append('1');
                                            }
                                            else
                                            {
                                                scanstr.Append('0');
                                            }
                                        }
                                    }
                                    // selly
                                    // 13slot 인 경우, 111111111111100000 이런 형태로 받는다 . 내부적으로 2번째 자리가 3slot 임을 판단하는 로직이 필요                                    
                                    string result = string.Empty;
                                    if (this.FoupOpModule().GetFoupController(foupnum).GetCassetteType() == CassetteTypeEnum.FOUP_13)
                                    {                         
                                        string binaryString = scanstr.ToString();
                                        string oddPositionedData = "";
                                        string evenPositionedData = "";

                                        for (int i = 0; i < binaryString.Length; i++)
                                        {
                                            if ((i + 1) % 2 != 0)
                                            {
                                                // 홀수 위치
                                                oddPositionedData += binaryString[i];
                                            }
                                            else
                                            {
                                                // 짝수 위치
                                                // 짝수 위치에 무조건 0 이 들어와야한다.
                                                // 만약 짝수 위치에 0이 들어오지 않는다면, 앞뒤 홀수 slot 과 같이 333으로 변경 필요
                                                evenPositionedData += "0";
                                            }
                                        }

                                        result = oddPositionedData + evenPositionedData;
                                    }

                                    PIVInfo pivinfo = new PIVInfo();
                                    if (this.FoupOpModule().GetFoupController(foupnum).GetCassetteType() == CassetteTypeEnum.FOUP_13)
                                    {
                                        pivinfo = new PIVInfo(foupnumber: foupnum, listofslot: result);
                                    }
                                    else
                                    {
                                        pivinfo = new PIVInfo(foupnumber: foupnum, listofslot: scanstr.ToString());
                                    }


                                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                    this.EventManager().RaisingEvent(typeof(ScanFoupDoneEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                    semaphore.Wait();

                                }

                                break;
                            }
                            System.Threading.Thread.Sleep(10);
                        }

                        if (LoaderMaster.GetIsAlwaysCloseFoupCover())
                        {
                            // foup cover close / open 판단하는 함수
                            LoaderModule.CloseFoupCoverFunc(Cassette, false);
                        }
                    }
                    else
                    {
                        Cassette.SetNoReadScanState();
                        //Docking Error
                        PIVInfo pivinfo = new PIVInfo(foupnumber: foupnum);

                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(ScanFoupFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();
                    }
                }
                else
                {
                    retVal = ackVal;
                }
                LoggerManager.Debug($"[GEM COMMANDER] {actReqData?.ActionType} - foup number {foupnum} : ");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ExcuteExcuter(RemoteActReqData actReqData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
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
                        ILoaderSupervisor LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                        var lotInfo = LoaderMaster.ActiveLotInfos.Find(info => info.FoupNumber == actReqData.FoupNumber);
                        if (lotInfo.AssignState == LotAssignStateEnum.CANCEL || lotInfo.AssignState == LotAssignStateEnum.CANCELED)
                        {
                            this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, (byte)EnumGemHCACK.REJECTED, actReqData.Count);
                            retVal = EventCodeEnum.GEM_COMMAND_REJECT;
                        }
                        else
                        {
                            this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, (byte)EnumGemHCACK.WILL_BE_PERFORMED, actReqData.Count);
                            retVal = EventCodeEnum.GEM_COMMAND_PERFORM;
                        }
                        LoggerManager.Debug($"DockFoup SetAck() RetVal : {retVal}, AssignState: {lotInfo.AssignState}.");
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

    public class AssignSlots : GemActBehaviorBase
    {
        public AssignSlots()
        {

        }

        EventCodeEnum CheckValidation(SelectStageSlotActReqData actReqData, out string errormsg)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            string description = "";
            ILoaderSupervisor Master = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
            try
            {
                if (actReqData == null)
                {
                    retVal = EventCodeEnum.EXCEPTION;
                    description = $"actReqData is null";
                    errormsg = description;
                    return retVal;
                }


                string cellData = actReqData.CellMap;

                var array = cellData.ToArray();
                List<int> usingstgIdx = new List<int>();

                for (int index = 0; index < array.Length; index++)
                {
                    if (array[index] == '1')
                    {
                        var stgnum = index + 1;
                        usingstgIdx.Add(stgnum);

                        //Command 의 대상인 StageNumber 가 0보다 작거나 modulecount와 클 경우
                        if (stgnum <= 0 || stgnum > SystemModuleCount.ModuleCnt.StageCount)
                        {
                            retVal = EventCodeEnum.EXCEPTION;
                            description = $"Allocated StageNum is invalid. stgnum:{stgnum}";
                            errormsg = description;
                            return retVal;
                        }
                    }
                }

                //할당된 stage가 0개일 경우
                if (usingstgIdx.Count() <= 0)
                {
                    retVal = EventCodeEnum.EXCEPTION;
                    description = $"Allocated Stage Count is 0.";
                    errormsg = description;
                    return retVal;
                }

                List<int> stagenumber = new List<int>();
                foreach (var stagenum in usingstgIdx)
                {
                    try
                    {
                        ILoaderServiceCallback client = Master.GetClient(stagenum);
                        if (client == null)
                        {//할당된 stage중 로더와 연결이 끊겨져 있는 셀이 있을 경우
                            retVal = EventCodeEnum.EXCEPTION;
                            description = $"Allocated Stage{stagenum} is Disconnected.";
                            errormsg = description;
                            return retVal;
                        }
                        else
                        {
                            //할당된 stage가 Machininit Done 상태가 아닐 경우
                            if (client.GetMachineInitDoneState() == false)
                            {
                                retVal = EventCodeEnum.MONITORING_MACHINE_INIT_ERROR;
                                description = $"Allocated Stage{stagenum} is not machine done state.";
                                errormsg = description;
                                return retVal;
                            }

                            if (client.CanRunningLot() == false)
                            {
                                retVal = EventCodeEnum.LOT_NOT_READY;
                                stagenumber.Add(stagenum);
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }

                if (retVal == EventCodeEnum.LOT_NOT_READY)
                {
                    string stagestr = "";
                    if (stagenumber != null)
                    {
                        foreach (var num in stagenumber)
                        {
                            stagestr += $"#{num}";
                        }
                    }
                    description = $"Allocated Stage{stagestr} is not ready";
                    errormsg = description;
                    return retVal;
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            errormsg = description;
            return retVal;
        }


        public override EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            int foupnumber = 0;
            byte HCACK = 0x02;
            bool canExecute = false;
            bool isSlotExceeded = false;
            try
            {
                PIVInfo pivinfo;
                SemaphoreSlim semaphore;

                var reqData = actReqData as SelectSlotsActReqData;
                if (reqData != null)
                {
                    HCACK = 0x00;
                    this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HCACK, actReqData.Count);

                    ILoaderSupervisor LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                    foupnumber = reqData.FoupNumber;

                    // selly                                                           
                    // 13slot 인 경우, 123456789 이런 형태로 받아서, 135791113151719 이런 형태로 변환 필요                    
                    if (this.FoupOpModule().GetFoupController(foupnumber).GetCassetteType() == CassetteTypeEnum.FOUP_13)
                    {
                        //1. 14보다 값이 작은지 확인                        
                        if(reqData.UseSlotNumbers.Count < 14)
                        {
                            // 홀수 값으로 변환
                            for (int i = 0; i < reqData.UseSlotNumbers.Count; i++)
                            {
                                reqData.UseSlotNumbers[i] = reqData.UseSlotNumbers[i] * 2 - 1;
                            }
                        }
                        else
                        {
                            // 선택한 slot 중에 13번째를 넘어가는 slot 이 있어서 13slot 에 부적합하다.                            
                            LoggerManager.Debug($"[GEM COMMANDER] {reqData?.ActionType} - foup number {reqData?.FoupNumber} : " +
                                $"The slot information received from the host should not contain any slots beyond the 13th.");
                            isSlotExceeded = true;
                        }                      
                    }

                    if (!isSlotExceeded)
                    {
                        LoaderMaster.SelectSlot(foupnumber, reqData.LotID, reqData.UseSlotNumbers);

                        pivinfo = new PIVInfo(foupnumber: foupnumber);

                        semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(SlotsSelectedEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();

                        LoggerManager.Debug($"[GEM COMMANDER] {reqData?.ActionType} - foup number {reqData?.FoupNumber} : ");
                    }                    
                }


                string errorlog;
                SelectStageSlotActReqData reqData2 = actReqData as SelectStageSlotActReqData;
                if (this.CheckValidation(reqData2, out errorlog) == EventCodeEnum.NONE)
                {
                    canExecute = true;
                }

                if (canExecute)
                {
                    HCACK = 0x00;// 4 - do async
                }
                else
                {
                    HCACK = 0x02;// 2 - cannot do now                    
                }


                if (canExecute)
                {
                    ILoaderSupervisor LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                    //ILoaderModule LoaderModule = LoaderMaster.Loader;
                    ActiveProcessActReqData activeData = new ActiveProcessActReqData();
                    foupnumber = reqData2.PTN;
                    activeData.FoupNumber = foupnumber;

                    activeData.LotID = this.GEMModule().GetPIVContainer().GetLotIDAtFoupInfos(activeData.FoupNumber, reqData2.CarrierId);

                    //string cellData=reqData.CellMap.Replace("1", "0");
                    string cellData = reqData2.CellMap;
                    //cellData = cellData.Replace("1", "0");

                    activeData.UseStageNumbers_str = cellData;
                    this.GEMModule().GetPIVContainer().SetFoupInfo(foupindex: reqData2.PTN, stagelist: cellData);
                    var array = cellData.ToArray();

                    for (int index = 0; index < array.Length; index++)
                    {
                        if (array[index] == '1')
                            activeData.UseStageNumbers.Add(index + 1);// Data : [A] ListOfStagesToUse
                    }
                    LoaderMaster.DynamicMode = DynamicModeEnum.NORMAL;//TODO: 없애야함.                    
                    retVal = LoaderMaster.ActiveProcess(activeData);

                    string slotData = reqData2.SlotMap;

                    var slotArray = slotData.ToArray();
                    List<int> slots = new List<int>();
                    
                    for (int index = 0; index < slotArray.Length; index++)
                    {
                        if (slotArray[index] == '1')
                        {
                            slots.Add(index + 1);       
                        }
                    }                                  
                    
                    var firstcellindex = activeData.UseStageNumbers.FirstOrDefault();

                    this.DeviceManager().SetPMIDeviceUsingCellParam(reqData2.PTN, firstcellindex);

                    LoaderMaster.SelectSlot(reqData2.PTN, activeData.LotID, slots);                                            

                    this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HCACK, actReqData.Count);
                    LoggerManager.Debug($"[GEM COMMANDER] {actReqData?.ActionType} Replied {HCACK} ");

                    PIVInfo pIVInfo = new PIVInfo(foupnumber: reqData2.PTN,
                                                    lotid: activeData.LotID,
                                                    settemperature: LoaderMaster.GetClient(firstcellindex)?.GetSetTemp() ?? -999);
                    semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(StageSlotSelectedEvent).FullName, new ProbeEventArgs(this, semaphore, pIVInfo));
                    semaphore.Wait();
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Debug($"[GEM COMMANDER] {actReqData?.ActionType} rejected reason: {errorlog}");

                    this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HCACK, actReqData.Count);
                    LoggerManager.Debug($"[GEM COMMANDER] {actReqData?.ActionType} Replied {HCACK} ");
                    if (reqData2 != null)
                    {
                        pivinfo = new PIVInfo(foupnumber: reqData2.PTN);
                        semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(StageSlotSelectFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();
                        retVal = EventCodeEnum.EXCEPTION;
                        return retVal;
                    }
                    else if (isSlotExceeded)
                    {
                        pivinfo = new PIVInfo(foupnumber: foupnumber);
                        semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(StageSlotSelectFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();
                        retVal = EventCodeEnum.EXCEPTION;
                        return retVal;
                    }
                }

                LoggerManager.Debug($"[GEM COMMANDER] {reqData2?.ActionType} - foup number {reqData2?.PTN} : ");
            }
            catch (Exception err)
            {
                this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HCACK, actReqData.Count);
                LoggerManager.Debug($"[GEM COMMANDER] {actReqData?.ActionType} Replied {HCACK} ");

                var pivinfo = new PIVInfo(foupnumber: foupnumber);
                var semaphore = new SemaphoreSlim(0);
                this.EventManager().RaisingEvent(typeof(StageSlotSelectFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                semaphore.Wait();
                LoggerManager.Exception(err);
                return EventCodeEnum.EXCEPTION;
            }
            return retVal;
        }

        public override EventCodeEnum ExcuteExcuter(RemoteActReqData actReqData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class AssignSlotsStages : GemActBehaviorBase
    {
        public AssignSlotsStages()
        {

        }

        public override EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                byte HAACK = 0x04;
                this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HAACK, actReqData.Count);

                ILoaderSupervisor LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                var reqData = actReqData as SelectSlotsStagesActReqData;
                if (reqData != null)
                {
                    Dictionary<int, List<int>> usingStageBySlot = new Dictionary<int, List<int>>();
                    LoaderMaster.ActiveLotInfos[reqData.FoupNumber - 1].UsingSlotList.Clear();
                    LoaderMaster.ActiveLotInfos[reqData.FoupNumber - 1].NotDoneSlotList.Clear();
                    foreach (var data in reqData.SlotStageNumbers)
                    {
                        if (data.CellIndexs.FindAll(index => index != 0).Count != 0)
                        {
                            usingStageBySlot.Add(data.SlotIndex, data.CellIndexs);
                            if (data.CellIndexs.Count > 0)
                            {
                                LoaderMaster.ActiveLotInfos[reqData.FoupNumber - 1].UsingSlotList.Add(data.SlotIndex);
                                LoaderMaster.ActiveLotInfos[reqData.FoupNumber - 1].NotDoneSlotList.Add(data.SlotIndex);
                            }
                        }
                    }
                    retVal = LoaderMaster.Select_Slot_Stages(reqData.FoupNumber, reqData.LotID, usingStageBySlot, reqData.SlotStageNumbers);

                    PIVInfo pivinfo = new PIVInfo(foupnumber: reqData.FoupNumber);
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(SlotsSelectedEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    semaphore.Wait();
                }

                //v22_merge// reqData2 관련 아래 로직이 왜 필요하지? hynix
                var reqData2 = actReqData as SelectStageSlotsActReqData;
                if (reqData2 != null)
                {
                    #region ActiveLot
                    ActiveProcessActReqData activeData = new ActiveProcessActReqData();

                    activeData.FoupNumber = reqData2.PTN;
                    activeData.LotID = reqData2.LotID;
                    activeData.UseStageNumbers_str = reqData2.CellMap;
                    for (int index = 0; index < activeData.UseStageNumbers_str.Length; index++)
                    {
                        if (activeData.UseStageNumbers_str[index] == '1')
                            activeData.UseStageNumbers.Add(index + 1);// Data : [A] ListOfStagesToUse 123000000000000
                    }
                    retVal = LoaderMaster.ActiveProcess(activeData);
                    #endregion

                    #region Match slot to cell
                    Dictionary<int, List<int>> usingStageBySlot = new Dictionary<int, List<int>>();


                    LoaderMaster.ActiveLotInfos[reqData2.PTN - 1].UsingSlotList.Clear();


                    IEnumerable<int> slotwithstageinfo = reqData2.UseSlotStageNumbers_str.Select(c => int.Parse(c.ToString()));//like "113300000000"

                    int slotNum = 1;
                    foreach (var slotinfo in slotwithstageinfo)
                    {
                        if (slotinfo != 0)
                        {
                            usingStageBySlot.Add(slotNum, new List<int> { slotinfo });// (slotnum, stagenum)
                            LoaderMaster.ActiveLotInfos[reqData2.PTN - 1].UsingSlotList.Add(slotNum);
                        }
                        slotNum += 1;
                    }


                    retVal = LoaderMaster.Select_Slot_Stages(reqData2.PTN, reqData2.LotID, usingStageBySlot);

                    PIVInfo pIVInfo = new PIVInfo(foupnumber: reqData2.PTN);
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(StageSlotSelectedEvent).FullName, new ProbeEventArgs(this, semaphore, pIVInfo));
                    semaphore.Wait();
                    retVal = EventCodeEnum.NONE;

                    #endregion

                    LoggerManager.Debug($"[GEM COMMANDER] {reqData2?.ActionType} - foup number {reqData2?.PTN} : ");
                }



            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ExcuteExcuter(RemoteActReqData actReqData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }



    /// <summary>
    /// M사 Dynamic Foup Mode 일때 Foup 에 대해서 Load or Load & Unload 모드 변경
    /// </summary>
    public class ChangeFoupModeState : GemActBehaviorBase
    {
        public ChangeFoupModeState()
        {

        }

        public override EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var reqData = actReqData as ChangeLoadPortModeActReqData;
                ILoaderSupervisor LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                if (reqData != null)
                {
                    DynamicFoupStateEnum stateEnum = DynamicFoupStateEnum.LOAD_AND_UNLOAD;
                    if (reqData.FoupModeState == 0)
                    {
                        stateEnum = DynamicFoupStateEnum.LOAD_AND_UNLOAD;
                    }
                    else if (reqData.FoupModeState == 1)
                    {
                        stateEnum = DynamicFoupStateEnum.UNLOAD;
                    }
                    LoaderMaster.ActiveLotInfos[reqData.FoupNumber - 1].DynamicFoupState = stateEnum;
                    LoaderMaster.Loader.Foups[reqData.FoupNumber - 1].DynamicFoupState = stateEnum;

                    if (stateEnum == DynamicFoupStateEnum.LOAD_AND_UNLOAD)
                    {
                        PIVInfo pivinfo = new PIVInfo(foupnumber: reqData.FoupNumber);
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(FoupStateChangedToLoadAndUnloadEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();
                    }
                    else if (stateEnum == DynamicFoupStateEnum.UNLOAD)
                    {
                        PIVInfo pivinfo = new PIVInfo(foupnumber: reqData.FoupNumber);
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(FoupStateChangedToUnloadEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();
                    }

                    LoggerManager.Debug($"#{reqData.FoupNumber}Foup State Mode Changed to {stateEnum}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class LotStart : GemActBehaviorBase
    {
        public LotStart()
        {

        }

        public override EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                EventCodeEnum ackVal = SetAck(remoteActReqData: actReqData);
                if (ackVal == EventCodeEnum.GEM_COMMAND_PERFORM)
                {
                    var reqData = actReqData as StartLotActReqData;

                    ILoaderSupervisor LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();

                    //OCR Setting 추가.
                    if (reqData.OCRReadFalg == 0)
                    {
                        LoaderMaster.Loader.OCRConfig.Mode = OCR_OperateMode.NG_GO;
                    }

                    PIVInfo pivinfo = new PIVInfo(foupnumber: reqData.FoupNumber, lotid: reqData.LotID);
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(FoupProcessingStartEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    semaphore.Wait();

                    LoaderMaster.ExternalLotOPStart(reqData.FoupNumber, reqData.LotID);
                }
                else
                {
                    retVal = ackVal;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ExcuteExcuter(RemoteActReqData actReqData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
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
                    var actReqData = remoteActReqData as StartLotActReqData;
                    if (actReqData != null)
                    {
                        ILoaderSupervisor LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                        var lotInfo = LoaderMaster.ActiveLotInfos.Find(info => info.FoupNumber == actReqData.FoupNumber);
                        if (lotInfo.AssignState == LotAssignStateEnum.ASSIGNED || lotInfo.AssignState == LotAssignStateEnum.POSTPONED)
                        {
                            this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, (byte)EnumGemHCACK.WILL_BE_PERFORMED, actReqData.Count);
                            retVal = EventCodeEnum.GEM_COMMAND_PERFORM;
                        }
                        else
                        {
                            this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, (byte)EnumGemHCACK.CANNOT_PERFORM_NOW, actReqData.Count);
                            retVal = EventCodeEnum.GEM_COMMAND_REJECT;
                        }
                        LoggerManager.Debug($"DockFoup SetAck() RetVal : {retVal}, AssignState: {lotInfo.AssignState}.");
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

    public class PStart : GemActBehaviorBase
    {
        public PStart()
        {

        }

        public override EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var reqData = actReqData as StartLotActReqData;

                ILoaderSupervisor LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                byte HCACK = 0x04;

                if (this.GEMModule().GemSysParam.ReceiveMessageType.Equals("SemicsGemReceiverSEKS"))
                {
                    #region [STM_CATANIA] PTN, LOTID Error Check & Ack
                    List<(string cpName, byte cpAck)> subAckList = new List<(string cpName, byte cpAck)>();

                    // 1. PTN
                    (string cpName, byte cpAck) foupAck = ("PTN", 0x00);
                    var foupcnt = SystemModuleCount.ModuleCnt.FoupCount;
                    var ptnVal = reqData.FoupNumber;
                    var lotinfos = this.GetLoaderContainer().Resolve<LoaderBase.ILoaderSupervisor>().ActiveLotInfos;
                    lotinfos[ptnVal - 1].RCMDErrorCheckDic.TryGetValue("PTN", out string prevPtn);

                    if (ptnVal < 1 || ptnVal > foupcnt || !ptnVal.ToString().Equals(prevPtn))
                    {
                        foupAck.cpAck = 0x02; // validation error
                        //foupAck.cpName = "PTNERROR"
                        subAckList.Add(foupAck);
                    }

                    // 2. OCR MODE
                    (string cpName, byte cpAck) ocrAck = ("OCRREAD", 0x00);
                    if (reqData.OCRReadFalg < 0 || reqData.OCRReadFalg > 2)
                    {
                        ocrAck.cpAck = 0x02;
                        //ocrAck.cpName = "OCRREADERROR";
                        subAckList.Add(ocrAck);
                    }

                    // 3. LOTID
                    (string cpName, byte cpAck) lotidAck = ("LOTID", 0x00);
                    var lotid = string.Empty;
                    if (ptnVal > 0 && ptnVal < lotinfos.Count + 1)
                    {
                        lotid = lotinfos[ptnVal - 1].LotID;
                    }

                    if (!string.IsNullOrEmpty(lotid) && !lotid.Equals(reqData.LotID))
                    {
                        lotidAck.cpAck = 0x02;
                        //lotidAck.cpName = "LOTIDERROR";
                        subAckList.Add(lotidAck);
                    }

                    if (subAckList.Count > 0)
                    {
                        HCACK = 0x03; // Parameter Error     
                    }
                    else
                    {
                        #region [STM_CATANIA] CELL의 Maintenance Mode 확인
                        if (SystemManager.SystemType == SystemTypeEnum.GOP)
                        {
                            // 사용하는 cell
                            var isMaintenanceMode = false;
                            var isWaiting_tester_response = false;
                            var is_tester_reject = false;


                            var loaderCommunicationManager_ = this.GetLoaderContainer().Resolve<LoaderBase.Communication.ILoaderCommunicationManager>();
                            var usingStages = LoaderMaster.ActiveLotInfos[ptnVal - 1].UsingStageIdxList;
                            usingStages.ForEach(x =>
                            {
                                var stage = loaderCommunicationManager_.GetStage(x);
                                if (stage.StageMode == GPCellModeEnum.MAINTENANCE)
                                {
                                    isMaintenanceMode = true;
                                }


                            });

                            is_tester_reject = LoaderMaster.Loader.Foups[ptnVal - 1].LotSettings.ToList().Any(x => usingStages.Contains(x.Index) && x.IsAssigned == false);
                            isWaiting_tester_response = LoaderMaster.Loader.Foups[ptnVal - 1].LotSettings.ToList().Any(x => usingStages.Contains(x.Index) && x.IsAssigned == true && x.IsVerified == false);

                            if (isMaintenanceMode)
                            {
                                HCACK = 0x02; // Can't do now
                                (string cpName, byte cpAck) modeAck = ("INMAINTENANCEMODE", 0x02);
                                subAckList.Add(modeAck);
                            }
                            else if (is_tester_reject)
                            {
                                HCACK = 0x02; // Can't do now
                                (string cpName, byte cpAck) modeAck = ("TESTERREHECT", 0x02);
                                subAckList.Add(modeAck);
                            }
                            else if (isWaiting_tester_response)
                            {
                                HCACK = 0x02; // Can't do now
                                (string cpName, byte cpAck) modeAck = ("WAITINGTESTERRESPONSE", 0x02);
                                subAckList.Add(modeAck);
                            }
                        }
                        #endregion
                    }

                    var gemService = this.GetLoaderContainer().Resolve<IGEMModule>().GemCommManager.GetSecsGemServiceModule();
                    long pnObjectID = reqData.ObjectID;
                    gemService.MakeObject(ref pnObjectID);
                    gemService.SetListItem(pnObjectID, 2);
                    gemService.SetBinaryItem(pnObjectID, HCACK);
                    gemService.SetListItem(pnObjectID, subAckList.Count);
                    for (int i = 0; i < subAckList.Count; i++)
                    {
                        gemService.SetListItem(pnObjectID, 2);
                        gemService.SetStringItem(pnObjectID, subAckList[i].cpName);
                        gemService.SetBinaryItem(pnObjectID, subAckList[i].cpAck);
                    }
                    gemService.SendSECSMessage(pnObjectID, reqData.Stream, reqData.Function + 1, reqData.Sysbyte);

                    if (HCACK != 0x04) { return retVal; }
                    #endregion
                }
                else
                {
                    this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HCACK, actReqData.Count);
                }

                //이전 랏드와 셀이 모두 일치한다면 LOT START 할 수 없음 조건 추가.
                //bool isStart = false;
                //foreach (var idx in LoaderMaster.ActiveLotInfos[reqData.FoupNumber - 1].UsingStageIdxList)
                //{
                //    if(LoaderMaster.StageStates[idx-1]==ModuleStateEnum.IDLE)
                //    {
                //        isStart = true;
                //        break;
                //    }
                //}

                //if (isStart == false)
                //{
                //    LoggerManager.Debug("[PSTART] Can not start lot. because all stage is running another lot.");
                //    return retVal;
                //}

                //OCR Setting 추가.
                if (reqData.OCRReadFalg == 0)
                {
                    LoaderMaster.Loader.OCRConfig.Mode = OCR_OperateMode.DEBUG;
                }
                else if (reqData.OCRReadFalg == 1)
                {
                    LoaderMaster.Loader.OCRConfig.Mode = OCR_OperateMode.NG_GO;
                }
                else if (reqData.OCRReadFalg == 2)
                {
                    LoaderMaster.Loader.OCRConfig.Mode = OCR_OperateMode.NORMAL;
                }

                var activeLotInfo = LoaderMaster.ActiveLotInfos[reqData.FoupNumber - 1];
                if (activeLotInfo.LotID == "")
                {
                    activeLotInfo.LotID = reqData.LotID;                    
                }
                LoaderMaster.SetActiveLotInfotoStage(activeLotInfo);
                retVal = LoaderMaster.ExternalLotOPStart(reqData.FoupNumber, reqData.LotID);

                foreach (var stage in activeLotInfo.UsingStageIdxList)
                {
                    gemServiceHost.OnRemoteCommandAction(stage, reqData);
                }

                if (retVal == EventCodeEnum.NONE)
                {

                    PIVInfo pivinfo = new PIVInfo(foupnumber: reqData.FoupNumber, lotid: reqData.LotID);

                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(FoupProcessingStartEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                    semaphore.Wait();

                    LoggerManager.Debug($"[GEM COMMANDER] {reqData?.ActionType} - foup number {reqData?.FoupNumber} : ");
                    //ILoaderModule LoaderModule = LoaderMaster.Loader;
                    //var lockobj = LoaderModule.LoaderMaster.GetLoaderPIV().GetPIVDataLockObject();
                    //lock (lockobj)
                    //{
                    //    LoaderMaster.GetLoaderPIV().SetFoupState(reqData.FoupNumber, GEMFoupStateEnum.PROCESSING);
                    //    LoaderMaster.GetLoaderPIV().UpdateFoupInfo(reqData.FoupNumber);
                    //    this.GEMModule().SetEvent(this.GEMModule().GetEventNumberFormEventName(typeof(FoupProcessingStartEvent).FullName));
                    //}

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ExcuteExcuter(RemoteActReqData actReqData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.PMIModule().SetRemoteOperation(PMIRemoteOperationEnum.ITSELF);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class TC_Start : GemActBehaviorBase
    {
        public TC_Start()
        {

        }

        EventCodeEnum CheckValidation(StartLotActReqData actReqData, out string errormsg)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            string description = "";
            ILoaderSupervisor Master = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
            try
            {
                if (actReqData == null)
                {
                    retVal = EventCodeEnum.EXCEPTION;
                    description = $"actReqData is null";
                    errormsg = description;
                    return retVal;
                }

                var foupnum = actReqData.FoupNumber;
                if (foupnum <= 0 || foupnum > SystemModuleCount.ModuleCnt.FoupCount)
                {
                    retVal = EventCodeEnum.PARAM_INSUFFICIENT;
                    description = $"actReqData.FoupNumber is {foupnum}";
                    errormsg = description;
                    return retVal;
                }

                // foup이 Error 상태인 경우 
                if (this.FoupOpModule().GetFoupController(foupnum).FoupModuleInfo.State == FoupStateEnum.ERROR)
                {
                    retVal = EventCodeEnum.FOUP_ERROR;
                    description = $"Cannot Start TC_Start. Foup{foupnum} State is Error.";
                    errormsg = description;
                    return retVal;
                }


                // 다른 foup에 해당 셀이 이미 할당되어있을 경우 Standard Wafer를 우선적으로 테스트 하기 위해서 Tcw Lot를 Reject한다.
                foreach (var curlotinfo in Master.ActiveLotInfos.Where(l => l.FoupNumber != foupnum))
                {
                    foreach (var tcwAllocCell in curlotinfo.UsingStageIdxList)
                    {
                        if (this.FoupOpModule().GetFoupController(curlotinfo.FoupNumber).FoupModuleInfo.State == FoupStateEnum.LOAD &&
                            curlotinfo.UsingStageIdxList.Contains(tcwAllocCell))
                        {
                            retVal = EventCodeEnum.ALREADY_USING_STAGE;
                            description = $"Already Allocated Cell, Cannnot allocate Cell{tcwAllocCell}, Allocted Lot: Lot {curlotinfo.FoupNumber}. LotState:{curlotinfo.State}";
                            errormsg = description;
                            return retVal;
                        }

                    }
                }

                foreach (var allocStage in Master.ActiveLotInfos[foupnum - 1].UsingStageIdxList)
                {
                    var connStage = Master.GetClient(allocStage);
                    if (connStage.GetChuckWaferStatus() != EnumSubsStatus.NOT_EXIST)
                    {
                        if (connStage.GetChuckWaferStatus() == EnumSubsStatus.EXIST && connStage.GetWaferType() == EnumWaferType.POLISH)
                        {
                            LoggerManager.Debug($"[GEM COMMANDER] {actReqData?.ActionType} Wafer Alread Exist, WaferType: Polish.");
                            continue;
                        }
                        retVal = EventCodeEnum.EXCEPTION;
                        description = $"Wafer On Chuck, WaferStatus:{connStage.GetChuckWaferStatus()}, WaferId:{connStage.GetWaferID()}";
                        errormsg = description;
                        return retVal;
                    }
                }



                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            errormsg = description;
            return retVal;
        }

        public override EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            bool canExecute = false;
            byte HCACK = 0x02;
            try
            {
                var reqData = actReqData as StartLotActReqData;

                ILoaderSupervisor LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();

                string errorlog;
                if (this.CheckValidation(reqData, out errorlog) == EventCodeEnum.NONE)
                {
                    canExecute = true;
                }


                if (canExecute)
                {
                    HCACK = 0x04;// 4 - do async
                }
                else
                {
                    HCACK = 0x02;// 2 - cannot do now                    
                }

                this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HCACK, actReqData.Count);
                LoggerManager.Debug($"[GEM COMMANDER] {actReqData?.ActionType} Replied {HCACK} ");

                if (canExecute)
                {
                    if (reqData.OCRReadFalg == 0)
                    {
                        LoaderMaster.Loader.OCRConfig.Mode = OCR_OperateMode.DEBUG;
                    }
                    else if (reqData.OCRReadFalg == 1)
                    {
                        LoaderMaster.Loader.OCRConfig.Mode = OCR_OperateMode.NG_GO;
                    }


                    if (LoaderMaster.ActiveLotInfos[reqData.FoupNumber - 1].LotID == "")
                    {
                        LoaderMaster.ActiveLotInfos[reqData.FoupNumber - 1].LotID = reqData.LotID;
                    }

                    retVal = LoaderMaster.ExternalTCW_Start(reqData.FoupNumber, reqData.LotID);

                    foreach (var stage in LoaderMaster.ActiveLotInfos[reqData.FoupNumber - 1].UsingStageIdxList)
                    {
                        gemServiceHost.OnRemoteCommandAction(stage, reqData);
                    }

                    if (retVal == EventCodeEnum.NONE)
                    {

                        PIVInfo pivinfo = new PIVInfo(foupnumber: reqData.FoupNumber, lotid: reqData.LotID);

                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(FoupProcessingStartEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();

                        LoggerManager.Debug($"[GEM COMMANDER] {reqData?.ActionType} - foup number {reqData?.FoupNumber} : ");

                    }
                }
                else
                {
                    LoggerManager.Debug($"[GEM COMMANDER] {actReqData?.ActionType} rejected reason: {errorlog}");
                }


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ExcuteExcuter(RemoteActReqData actReqData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.PMIModule().SetRemoteOperation(PMIRemoteOperationEnum.ITSELF);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class Zup : GemActBehaviorBase
    {
        public Zup()
        {

        }

        public override EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                byte HAACK = 0x04;
                this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HAACK, actReqData.Count);

                if (actReqData is StageActReqData)
                {
                    var reqData = actReqData as StageActReqData;
                    gemServiceHost?.OnRemoteCommandAction(reqData.StageNumber, reqData);
                    LoggerManager.Debug($"[GEM COMMANDER] {reqData?.ActionType} - StageNumber {reqData?.StageNumber} : ");
                }
                else if (actReqData is ZUpActReqData)
                {
                    var reqData = actReqData as ZUpActReqData;
                    gemServiceHost?.OnRemoteCommandAction(reqData.StageNumber, reqData);
                    LoggerManager.Debug($"[GEM COMMANDER] {reqData?.ActionType} - StageNumber {reqData?.StageNumber} : ");
                }


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ExcuteExcuter(RemoteActReqData actReqData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                bool cmdResult = false;
                if (this.ProbingModule().ModuleState.State == ModuleStateEnum.RUNNING ||
                    this.ProbingModule().ModuleState.State == ModuleStateEnum.SUSPENDED)
                {
                    cmdResult = this.CommandManager().SetCommand<IZUPRequest>(this);

                    LoggerManager.Debug($"[GEM EXECUTOR] OnRemoteCommandAction - Z_UP: SetCommand<IZUPRequest> {cmdResult}");
                }
                else
                {
                    LoggerManager.Debug($"[GEM EXECUTOR] OnRemoteCommandAction - Z_UP: Don't send SetCommand because probingmodule state is {this.ProbingModule().ModuleState.State}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class TestEnd : GemActBehaviorBase
    {
        public TestEnd()
        {

        }

        public override EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                byte HAACK = 0x04;
                this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HAACK, actReqData.Count);

                if (actReqData is StageActReqData)
                {
                    var reqData = actReqData as StageActReqData;
                    gemServiceHost.OnRemoteCommandAction(reqData.StageNumber, reqData);
                    LoggerManager.Debug($"[GEM COMMANDER] {reqData?.ActionType} - StageNumber {reqData?.StageNumber} : ");
                }
                else if (actReqData is EndTestReqDate)
                {
                    var reqData = actReqData as EndTestReqDate;
                    switch (reqData.PMIExecFlag)
                    {
                        //RECIPE PMI
                        case 0:
                            break;
                        // SKIP PMI
                        case 1:
                            break;
                        // EXCUTE PMI
                        case 2:
                            break;
                    }

                    gemServiceHost.OnRemoteCommandAction(reqData.StageNumber, reqData);
                    LoggerManager.Debug($"[GEM COMMANDER] {reqData?.ActionType} - StageNumber {reqData?.StageNumber} : ");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ExcuteExcuter(RemoteActReqData actReqData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (actReqData is StageActReqData)
                {
                    var reqData = actReqData as StageActReqData;
                    this.CommandManager().SetCommand<IMoveToNextDie>(this, new ProbingCommandParam() { ProbingStateWhenReciveMoveToNextDie = this.ProbingModule().ProbingStateEnum });
                    LoggerManager.Debug($"[GEM EXECUTOR] - Command Parameter {this.ProbingModule().ProbingStateEnum}");
                }
                else if (actReqData is EndTestReqDate)
                {
                    var reqData = actReqData as EndTestReqDate;
                    LoggerManager.Debug($"[GEM EXECUTOR] OnRemoteCommandAction - END_TEST: PMIExecFlag {reqData.PMIExecFlag} 0:RECIPE PMI, 1: SKIP PMI, 2: EXCUTE PMI");

                    if (this.ProbingModule().ModuleState.GetState() != ModuleStateEnum.PAUSED)
                    {
                        PMIRemoteOperationEnum remotevalue = PMIRemoteOperationEnum.UNDEFIEND;

                        switch (reqData.PMIExecFlag)
                        {
                            //RECIPE PMI
                            case 0:
                                remotevalue = PMIRemoteOperationEnum.ITSELF;
                                break;
                            // SKIP PMI
                            case 1:
                                remotevalue = PMIRemoteOperationEnum.SKIP;
                                break;
                            // EXCUTE PMI
                            case 2:
                                remotevalue = PMIRemoteOperationEnum.FORCEDEXECUTE;
                                break;
                            default:
                                remotevalue = PMIRemoteOperationEnum.UNDEFIEND;
                                break;
                        }

                        this.PMIModule().SetRemoteOperation(remotevalue);

                        // TODO : 향후 고려
                        //this.CommandManager().SetCommand<IMoveToNextDie>(this);

                        //this.CommandManager().SetCommand<IUnloadWafer>(this);
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

    public class TC_End : GemActBehaviorBase
    {
        public TC_End()
        {

        }

        EventCodeEnum CheckValidation(StageActReqData actReqData, out string errormsg)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            string description = "";
            ILoaderSupervisor Master = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
            try
            {
                if (actReqData == null)
                {
                    retVal = EventCodeEnum.EXCEPTION;
                    description = $"actReqData is null";
                    errormsg = description;
                    return retVal;
                }

                var stgnum = actReqData.StageNumber;
                if (stgnum <= 0 || stgnum > SystemModuleCount.ModuleCnt.StageCount)
                {
                    retVal = EventCodeEnum.PARAM_INSUFFICIENT;
                    description = $"actReqData.StageNumber is {stgnum}";
                    errormsg = description;
                    return retVal;
                }

                var isinitdone = Master.GetClient(stgnum)?.GetMachineInitDoneState();
                if (isinitdone == false)
                {
                    retVal = EventCodeEnum.MONITORING_MACHINE_INIT_ERROR;
                    description = $"Chuck{stgnum} Machineinit State is invalid Cur:{isinitdone}";
                    errormsg = description;
                    return retVal;
                }

                var enumSubsStatus = Master.GetClient(stgnum)?.GetChuckWaferStatus();
                if (enumSubsStatus != EnumSubsStatus.EXIST)
                {
                    retVal = EventCodeEnum.EXCEPTION;
                    description = $"Chuck{stgnum} Wafer State is Not Exist. Cur:{enumSubsStatus}";
                    errormsg = description;
                    return retVal;
                }

                var waferType = Master.GetClient(stgnum)?.GetWaferType();
                if (waferType != EnumWaferType.TCW)
                {
                    retVal = EventCodeEnum.EXCEPTION;
                    description = $"Chuck{stgnum} WaferType is Not TCW. Cur:{waferType}";
                    errormsg = description;
                    return retVal;
                }


                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            errormsg = description;
            return retVal;
        }

        public override EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            bool canExecute = false;
            byte HCACK = 0x02;
            try
            {
                string errorlog;

                var reqData = actReqData as StageActReqData;
                if (this.CheckValidation(reqData, out errorlog) == EventCodeEnum.NONE)
                {
                    canExecute = true;
                }


                if (canExecute)
                {
                    HCACK = 0x04;// 4 - do async
                }
                else
                {
                    HCACK = 0x02;// 2 - cannot do now                    
                }

                this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HCACK, actReqData.Count);
                LoggerManager.Debug($"[GEM COMMANDER] {actReqData?.ActionType} Replied {HCACK} ");

                if (canExecute)
                {
                    if (actReqData is StageActReqData)
                    {
                        gemServiceHost.OnRemoteCommandAction(reqData.StageNumber, reqData);
                        LoggerManager.Debug($"[GEM COMMANDER] {reqData?.ActionType} - StageNumber {reqData?.StageNumber} : ");
                    }
                }
                else
                {
                    LoggerManager.Debug($"[GEM COMMANDER] {actReqData?.ActionType} rejected reason: {errorlog}");
                }

                //else if (actReqData is TC_EndTestReqDate)
                //{
                //    var reqData = actReqData as TC_EndTestReqDate;

                //    gemServiceHost.OnRemoteCommandAction(reqData.StageNumber, reqData);
                //    LoggerManager.Debug($"[GEM COMMANDER] {reqData?.ActionType} - StageNumber {reqData?.StageNumber} : ");
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ExcuteExcuter(RemoteActReqData actReqData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (actReqData is StageActReqData)
                {
                    var reqData = actReqData as StageActReqData;
                    this.CommandManager().SetCommand<IChuckUnloadCommand>(this.StageSupervisor().WaferTransferModule());
                }

                //else if (actReqData is TC_EndTestReqDate)
                //{
                //    var reqData = actReqData as TC_EndTestReqDate;
                //    this.CommandManager().SetCommand<IChuckUnloadCommand>(this.StageSupervisor().WaferTransferModule());
                //    LoggerManager.Debug($"[GEM EXECUTOR] OnRemoteCommandAction - TC_END_TEST:  0:RECIPE PMI, 1: SKIP PMI, 2: EXCUTE PMI");

                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class WaferIDConfirm : GemActBehaviorBase
    {
        public WaferIDConfirm()
        {

        }

        public override EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                byte HAACK = 0x04;
                this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HAACK, actReqData.Count);
                ILoaderSupervisor LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                if (LoaderMaster != null)
                {
                    var reqData = actReqData as WaferConfirmActReqData;
                    if (reqData.PTN > 0)
                    {

                        var wafers = LoaderMaster.Loader.ModuleManager.GetTransferObjectAll().Where(
                            item => item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                                    item.OriginHolder.Index == (reqData.SlotNum + (reqData.PTN - 1) * 25)
                            ).ToList();

                        if (wafers != null)
                        {
                            wafers.First().WFWaitFlag = false;
                            LoggerManager.Debug($"WaferIDConfirm: WFWaitFlag: {false}");
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"WaferIDConfirm: Error reqData.PTN:{reqData.PTN}");
                    }

                    LoggerManager.Debug($"WaferIDConfirm: LotId:{reqData.LotID}, PTN:{reqData.PTN}, WaferId:{reqData.WaferId}, OCRReadFalg:{reqData.OCRReadFalg}");
                    LoggerManager.Debug($"[GEM COMMANDER] {reqData?.ActionType} - StageNumber {reqData?.PTN} : ");

                }


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ExcuteExcuter(RemoteActReqData actReqData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class TestEndNoAfterCommand : GemActBehaviorBase
    {
        public TestEndNoAfterCommand()
        {

        }

        public override EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                byte HAACK = 0x04;
                this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HAACK, actReqData.Count);

                if (actReqData is StageActReqData)
                {
                    var reqData = actReqData as StageActReqData;
                    gemServiceHost.OnRemoteCommandAction(reqData.StageNumber, reqData);
                    LoggerManager.Debug($"[GEM COMMANDER] {reqData?.ActionType} - StageNumber {reqData?.StageNumber} : ");
                }
                else if (actReqData is EndTestReqDate)
                {
                    var reqData = actReqData as EndTestReqDate;
                    switch (reqData.PMIExecFlag)
                    {
                        //RECIPE PMI
                        case 0:
                            break;
                        // SKIP PMI
                        case 1:
                            break;
                        // EXCUTE PMI
                        case 2:
                            break;
                    }

                    gemServiceHost.OnRemoteCommandAction(reqData.StageNumber, reqData);
                    LoggerManager.Debug($"[GEM COMMANDER] {reqData?.ActionType} - StageNumber {reqData?.StageNumber} : ");
                }
                else if (actReqData is EndTestReqLPDate)
                {
                    var reqData = actReqData as EndTestReqLPDate;
                    switch (reqData.PMIExecFlag)
                    {
                        //RECIPE PMI
                        case 0:
                            break;
                        // SKIP PMI
                        case 1:
                            break;
                        // EXCUTE PMI
                        case 2:
                            break;
                    }

                    gemServiceHost.OnRemoteCommandAction(reqData.StageNumber, reqData);
                    LoggerManager.Debug($"[GEM COMMANDER] {reqData?.ActionType} - StageNumber {reqData?.StageNumber} : ");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ExcuteExcuter(RemoteActReqData actReqData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (actReqData is StageActReqData)
                {
                    var reqData = actReqData as StageActReqData;
                    LoggerManager.Debug($"[GEM EXECUTOR] OnRemoteCommandAction - {actReqData.ActionType}");
                    this.CommandManager().SetCommand<IUnloadWafer>(this);
                }
                else if (actReqData is EndTestReqDate)
                {
                    var reqData = actReqData as EndTestReqDate;
                    LoggerManager.Debug($"[GEM EXECUTOR] OnRemoteCommandAction - {actReqData.ActionType}: PMIExecFlag {reqData.PMIExecFlag} 0:RECIPE PMI, 1: SKIP PMI, 2: EXCUTE PMI");

                    if (this.ProbingModule().ModuleState.GetState() != ModuleStateEnum.PAUSED)
                    {
                        PMIRemoteOperationEnum remotevalue = PMIRemoteOperationEnum.UNDEFIEND;

                        switch (reqData.PMIExecFlag)
                        {
                            //RECIPE PMI
                            case 0:
                                remotevalue = PMIRemoteOperationEnum.ITSELF;
                                break;
                            // SKIP PMI
                            case 1:
                                remotevalue = PMIRemoteOperationEnum.SKIP;
                                break;
                            // EXCUTE PMI
                            case 2:
                                remotevalue = PMIRemoteOperationEnum.FORCEDEXECUTE;
                                break;
                            default:
                                remotevalue = PMIRemoteOperationEnum.UNDEFIEND;
                                break;
                        }

                        this.PMIModule().SetRemoteOperation(remotevalue);

                        this.CommandManager().SetCommand<IMoveToNextDie>(this, new ProbingCommandParam() { ForcedZdownAndUnload = true });
                    }
                }
                else if (actReqData is EndTestReqLPDate)
                {
                    var reqData = actReqData as EndTestReqLPDate;
                    this.LotOPModule().UnloadFoupNumber = reqData.FoupNumber;
                    LoggerManager.Debug($"[GEM EXECUTOR] OnRemoteCommandAction - END_TEST: PMIExecFlag {reqData.PMIExecFlag} 0:RECIPE PMI, 1: SKIP PMI, 2: EXCUTE PMI, " +
                        $"Request Unload FOUP : {reqData.FoupNumber}");

                    if (this.ProbingModule().ModuleState.GetState() != ModuleStateEnum.PAUSED)
                    {
                        PMIRemoteOperationEnum remotevalue = PMIRemoteOperationEnum.UNDEFIEND;

                        switch (reqData.PMIExecFlag)
                        {
                            //RECIPE PMI
                            case 0:
                                remotevalue = PMIRemoteOperationEnum.ITSELF;
                                break;
                            // SKIP PMI
                            case 1:
                                remotevalue = PMIRemoteOperationEnum.SKIP;
                                break;
                            // EXCUTE PMI
                            case 2:
                                remotevalue = PMIRemoteOperationEnum.FORCEDEXECUTE;
                                break;
                            default:
                                remotevalue = PMIRemoteOperationEnum.UNDEFIEND;
                                break;
                        }

                        this.PMIModule().SetRemoteOperation(remotevalue);

                        this.CommandManager().SetCommand<IMoveToNextDie>(this, new ProbingCommandParam() { ForcedZdownAndUnload = true });
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

    public class TestEndToFoupNoAfterCommand : GemActBehaviorBase
    {
        public TestEndToFoupNoAfterCommand()
        {
        }

        public override EventCodeEnum ExcuteCarrierCommander(CarrierActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override EventCodeEnum ExcuteExcuter(RemoteActReqData actReqData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class ZDownCommand : GemActBehaviorBase
    {
        public ZDownCommand()
        {

        }

        public override EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                byte HAACK = 0x04;
                this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HAACK, actReqData.Count);

                if (actReqData is StageActReqData)
                {
                    var reqData = actReqData as StageActReqData;
                    gemServiceHost.OnRemoteCommandAction(reqData.StageNumber, reqData);
                    LoggerManager.Debug($"[GEM COMMANDER] {reqData?.ActionType} - StageNumber {reqData?.StageNumber} : ");
                }
                else if (actReqData is EndTestReqDate)
                {
                    var reqData = actReqData as EndTestReqDate;
                    gemServiceHost.OnRemoteCommandAction(reqData.StageNumber, reqData);
                    LoggerManager.Debug($"[GEM COMMANDER] {reqData?.ActionType} - StageNumber {reqData?.StageNumber} : ");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ExcuteExcuter(RemoteActReqData actReqData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (actReqData is StageActReqData)
                {
                    var reqData = actReqData as StageActReqData;
                    LoggerManager.Debug($"[GEM EXECUTOR] OnRemoteCommandAction - {actReqData.ActionType}");
                    this.CommandManager().SetCommand<IZDownRequest>(this);
                }
                else if (actReqData is EndTestReqDate)
                {
                    var reqData = actReqData as EndTestReqDate;
                    LoggerManager.Debug($"[GEM EXECUTOR] OnRemoteCommandAction - {actReqData.ActionType}");

                    if (this.ProbingModule().ModuleState.GetState() != ModuleStateEnum.PAUSED)
                    {
                        PMIRemoteOperationEnum remotevalue = PMIRemoteOperationEnum.UNDEFIEND;

                        switch (reqData.PMIExecFlag)
                        {
                            //RECIPE PMI
                            case 0:
                                remotevalue = PMIRemoteOperationEnum.ITSELF;
                                break;
                            // SKIP PMI
                            case 1:
                                remotevalue = PMIRemoteOperationEnum.SKIP;
                                break;
                            // EXCUTE PMI
                            case 2:
                                remotevalue = PMIRemoteOperationEnum.FORCEDEXECUTE;
                                break;
                            default:
                                remotevalue = PMIRemoteOperationEnum.UNDEFIEND;
                                break;
                        }

                        this.PMIModule().SetRemoteOperation(remotevalue);

                        this.CommandManager().SetCommand<IZDownRequest>(this);
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

    public class CancelCarrier : GemActBehaviorBase
    {
        public CancelCarrier()
        {

        }

        public override EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                byte HAACK = 0x04;
                this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HAACK, actReqData.Count);

                var reqData = actReqData as CarrierCancleData;
                ILoaderSupervisor LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();

                LoaderMaster.CarrierCancel(reqData.FoupNumber);
                LoggerManager.Debug($"[GEM COMMANDER] {reqData?.ActionType} - foup number {reqData?.FoupNumber} : ");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ExcuteExcuter(RemoteActReqData actReqData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override EventCodeEnum ExcuteCarrierCommander(CarrierActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                CarrierActReqData reqData = actReqData;
                ILoaderSupervisor LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();

                byte HAACK = 0x04;
                this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HAACK, actReqData.Count);

                LoaderMaster.CarrierCancel(reqData.PTN);
                LoggerManager.Debug($"[GEM COMMANDER] {reqData?.ActionType} - foup number {reqData?.PTN} : ");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

    }


    public class SuspendCarrier : GemActBehaviorBase
    {
        public SuspendCarrier()
        {

        }

        public override EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var reqData = actReqData as CarrierCancleData;
                ILoaderSupervisor LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();

                LoaderMaster.LotSuspend(reqData.FoupNumber);
                LoggerManager.Debug($"[GEM COMMANDER] {reqData?.ActionType} - foup number {reqData?.FoupNumber} : ");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ExcuteExcuter(RemoteActReqData actReqData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override EventCodeEnum ExcuteCarrierCommander(CarrierActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                // 지금은 CarrierCancleData로 받고 있는것 같지만... 일단 코드만 넣어놓음..
                CarrierActReqData reqData = actReqData;
                ILoaderSupervisor LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();

                LoaderMaster.LotSuspend(reqData.PTN);
                LoggerManager.Debug($"[GEM COMMANDER] {reqData?.ActionType} - foup number {reqData?.PTN} : ");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

    }


    public class UndockCarrier : GemActBehaviorBase
    {
        public UndockCarrier()
        {
        }

        public override EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                CarrierIdentityData reqData = actReqData as CarrierIdentityData;

                // DynamicFoup아닐경우에 이걸로 처리하는게 나을까..?

                //if (reqData != null)
                //{
                //    retVal = this.FoupOpModule().FoupControllers[reqData.PTN - 1].Execute(new FoupUnloadCommand());
                //}


                ILoaderSupervisor LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                EventCodeEnum ret = LoaderMaster.ActiveLotInfos[reqData.PTN - 1].FoupUnLoad();

                byte HAACK = 0x04;
                if (ret != EventCodeEnum.NONE)
                {
                    HAACK = 0x02;//2 - cannot do now
                }

                this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HAACK, actReqData.Count);
                LoggerManager.Debug($"[GEM COMMANDER] {reqData?.ActionType} - FoupNumber {reqData?.PTN} : ");
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        public override EventCodeEnum ExcuteExcuter(RemoteActReqData actReqData)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            return retVal;
        }
    }

    public class PAbort : GemActBehaviorBase
    {
        public PAbort()
        {

        }

        public override EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                byte HAACK = 0x04;
                this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HAACK, actReqData.Count);

                //var reqData = actReqData as ErrorEndData;

                ILoaderSupervisor LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                ILoaderCommunicationManager LoaderComm = this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
                int stgnumber = 0;

                if (actReqData is StageActReqData)
                {
                    var reqData = actReqData as StageActReqData;
                    stgnumber = reqData.StageNumber;
                }


                var client = LoaderMaster.GetClient(stgnumber);
                if (client != null)
                {
                    client.SetLotOut(true);//SetAbort와 다른점.
                    LoaderMaster.LotOPPauseClient(client, isabort: false, LoaderComm.GetStage(index: stgnumber));
                    LoaderMaster.AddLotAbortStageInfos(stgnumber, isCanReassignLot: false, abortCurrentLot: false);
                    ILoaderModule loader = this.GetLoaderContainer().Resolve<ILoaderModule>();

                    loader.BroadcastLoaderInfo();
                    //List<ActiveLotInfo> TotalLotInfos = null;
                    //TotalLotInfos = LoaderMaster.ActiveLotInfos;
                    ////TotalLotInfos.AddRange(Prev_ActiveLotInfos);//#PAbort 아직 FoupShift고려안됨.
                    //foreach (var lotinfo in TotalLotInfos)
                    //{
                    //    if (lotinfo.State != LotStateEnum.Idle)
                    //    {
                    //        //lotinfo.LotOutStageIndexList.Add(stgnumber);
                    //        LoaderMaster.
                    //    }
                    //    else
                    //    {
                    //        LoggerManager.Debug($"LotOutStageIndexList Add Skip. StageNumber:{stgnumber}, Cst_Hash:{lotinfo.CST_HashCode}");
                    //    }

                    //}

                }
                else
                {
                    PIVInfo pIVInfo = new PIVInfo(stagenumber: stgnumber);
                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                    this.EventManager().RaisingEvent(typeof(CellAbortFail).FullName, new ProbeEventArgs(this, semaphore, pIVInfo));
                    semaphore.Wait();
                }


                LoggerManager.Debug($"[GEM COMMANDER] {actReqData.ActionType} - stage number :{stgnumber}");
                //gemServiceHost?.OnRemoteCommandAction(stgnumber, actReqData);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ExcuteExcuter(RemoteActReqData actReqData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                //LoggerManager.Debug($"[GEM EXECUTER] {actReqData?.ActionType} , IsLotOut:{this.LoaderController().IsLotOut} ");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class CellAbort : GemActBehaviorBase
    {
        public CellAbort()
        {

        }

        public override EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (actReqData is ErrorEndData)
                {
                    var reqData = actReqData as ErrorEndData;
                    ILoaderSupervisor LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();

                    LoaderMaster.ErrorEndCell(reqData.StageNumber);
                    LoggerManager.Debug($"[GEM COMMANDER] {reqData?.ActionType} - stage number {reqData?.StageNumber} : ");
                    gemServiceHost?.OnRemoteCommandAction(reqData.StageNumber, reqData);
                }
                else if (actReqData is ErrorEndLPData)
                {
                    var reqData = actReqData as ErrorEndLPData;
                    ILoaderSupervisor LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();

                    LoaderMaster.ErrorEndCell(reqData.StageNumber);
                    LoggerManager.Debug($"[GEM COMMANDER] {reqData?.ActionType} - stage number {reqData?.StageNumber} : ");
                    gemServiceHost?.OnRemoteCommandAction(reqData.StageNumber, reqData);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ExcuteExcuter(RemoteActReqData actReqData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.LoaderController().IsCancel = true;

                //var pivinfo = new PIVInfo() { FoupNumber = this.GEMModule().GetPIVContainer().FoupNumber.Value };
                //SemaphoreSlim semaphore = new SemaphoreSlim(0);
                //this.EventManager().RaisingEvent(typeof(WaferTestingAborted).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                //semaphore.Wait();

                if (actReqData is ErrorEndLPData)
                {
                    this.LotOPModule().UnloadFoupNumber = (actReqData as ErrorEndLPData).FoupNumber;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }


    public class CellStart : GemActBehaviorBase
    {
        public CellStart()
        {

        }

        public override EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                byte HAACK = 0x04;
                this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HAACK, actReqData.Count);


                var reqData = actReqData as StartStage;
                gemServiceHost.OnRemoteCommandAction(reqData.StageNumber, reqData);
                LoggerManager.Debug($"[GEM COMMANDER] {reqData?.ActionType} - stage number {reqData?.StageNumber} : ");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ExcuteExcuter(RemoteActReqData actReqData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var reqData = actReqData as StartStage;
                if (reqData != null)
                {
                    //var lockobj = this.StageSupervisor().GetStagePIV().GetPIVDataLockObject();
                    //lock (lockobj)
                    //{
                    //    //this.StageSupervisor().GetStagePIV().SetBackupStageLotInfo(reqData.FoupNumber, reqData.LotID);
                    //}
                }
                this.CommandManager().SetCommand<ILotOpResume>(this);
                LoggerManager.Debug($"[GEM EXECUTER] {actReqData?.ActionType} CellStart ");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class PPSelect : GemActBehaviorBase
    {
        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);

        #region [STM_CATANIA] xtrf read & parsing
        (string deviceName, string lotMode) GetProberInfo(PPSelectActReqData ppSelectReqData)
        {
            var deviceName = "";
            var lotMode = "";

            try
            {
                // 1.Project Path를 가져온다.(PPBODY 정보는 ProjectPath + \XWork\Recipe 폴더에 sPpid name으로 저장되기 때문)
                var configFile = @"C:\ProberSystem\Utility\GEM\Config\GEM_PROCESS.cfg";

                StringBuilder projectPath = new StringBuilder(64);
                GetPrivateProfileString("XGEM", "ProjectPath", "", projectPath, 64, configFile);

                // 2.PPBODY 정보를 읽는다.
                var xtrfPath = Path.GetDirectoryName(projectPath.ToString()) + @"\XWork\Recipe";

                var Ppid = ppSelectReqData.Ppid.Split('/');
                var xtrfFileName = Ppid.Last().Trim();

                var xtrfFile = Path.Combine(xtrfPath, xtrfFileName);

                if (File.Exists(xtrfFile))
                {
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.Load(xtrfFile);

                    XmlNamespaceManager xNameSpaceManager = new XmlNamespaceManager(xDoc.NameTable);
                    xNameSpaceManager.AddNamespace("recipe", "urn:st-com:xsd.XTRF.V0162.Generic");

                    deviceName = xDoc.SelectSingleNode("recipe:testerRecipe/recipe:testPrgmIdentifier/recipe:testPrgm/recipe:proberInfo/recipe:setupName", xNameSpaceManager)?.InnerText;
                    lotMode = xDoc.SelectSingleNode("recipe:testerRecipe/recipe:testPrgmIdentifier/recipe:testPrgm/recipe:proberInfo/recipe:other[@name='otherProberInfo']/recipe:parameter[@name='lotMode']", xNameSpaceManager)?.InnerText;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return (deviceName, lotMode);
        }
        #endregion

        public override EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            if (actReqData is PPSelectActReqData ppdata)
            {
                var proberInfo = GetProberInfo(ppdata);
                if (VerifyXtrfInfo(proberInfo, ppdata))
                {
                    if (VerifyActParameters(ppdata))
                    {
                        retVal = DeviceDownload(ppdata, proberInfo.deviceName);
                        if (!retVal.Equals(EventCodeEnum.NONE))
                        {
                            this.EventManager().RaisingEvent(typeof(RecipeDownloadFailedEvent).FullName);
                            return retVal;
                        }

                        retVal = DeviceChange(ppdata, proberInfo.deviceName);
                        if (!retVal.Equals(EventCodeEnum.NONE))
                        {
                            this.EventManager().RaisingEvent(typeof(DeviceChangeFailEvent).FullName);
                            return retVal;
                        }

                        retVal = LotModeChange(ppdata, proberInfo.lotMode);
                    }
                }
            }

            return retVal;
        }
        public override EventCodeEnum ExcuteExcuter(RemoteActReqData actReqData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            return retVal;
        }
        private EventCodeEnum DeviceDownload(PPSelectActReqData ppdata, string deviceName)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            var isGOP = (SystemManager.SystemType == SystemTypeEnum.GOP);

            if (isGOP)
            {
                this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Downloading Device");
            }

            var downloadReqData = new DownloadStageRecipeActReqData();

            ILoaderPIV pivContainer = this.GEMModule().GetPIVContainer();
            if (ppdata is ONLINEPPSelectActReqData onppdata)
            {
                downloadReqData.FoupNumber = onppdata.PTN;//pivContainer.FoupNumber.Value;
                downloadReqData.LotID = onppdata.LotID;//pivContainer.LotID.Value;
            }
            else
            {
                downloadReqData.FoupNumber = pivContainer.FoupNumber.Value;
                downloadReqData.LotID = pivContainer.LotID.Value;
            }
            ppdata.UseStageNumbers.ForEach(x =>
            {
                downloadReqData.RecipeDic.Add(x, deviceName);
            });

            try
            {
                ILoaderSupervisor LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                retVal = this.DeviceManager().SetRecipeToStage(downloadReqData);
                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = LoaderMaster.SetRecipeToDevice(downloadReqData);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (isGOP)
                {
                    this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
                }
            }

            return retVal;
        }

        private bool VerifyXtrfInfo((string deviceName, string lotMode) proberInfo, PPSelectActReqData ppdata)
        {
            var ret = !string.IsNullOrEmpty(proberInfo.deviceName) && !string.IsNullOrEmpty(proberInfo.lotMode);

            byte HCACK = 0x00;  // 0 - ok, completed
            if (!ret)
            {
                HCACK = 0x03;   // 3 - parameter error

                List<(string cpName, byte cpAck)> subAckList = new List<(string cpName, byte cpAck)>();
                (string cpName, byte cpAck) xtrfAck = ("XTRFERROR", 0x02);
                subAckList.Add(xtrfAck);

                var gemService = this.GetLoaderContainer().Resolve<IGEMModule>().GemCommManager.GetSecsGemServiceModule();
                long pnObjectID = ppdata.ObjectID;
                gemService.MakeObject(ref pnObjectID);
                gemService.SetListItem(pnObjectID, 2);
                gemService.SetBinaryItem(pnObjectID, HCACK);
                gemService.SetListItem(pnObjectID, subAckList.Count);
                for (int i = 0; i < subAckList.Count; i++)
                {
                    gemService.SetListItem(pnObjectID, 2);
                    gemService.SetStringItem(pnObjectID, subAckList[i].cpName);
                    gemService.SetBinaryItem(pnObjectID, subAckList[i].cpAck);
                }
                gemService.SendSECSMessage(pnObjectID, ppdata.Stream, ppdata.Function + 1, ppdata.Sysbyte);

                SendPPSelectACK(ppdata.ObjectID, ppdata.Stream, ppdata.Function, ppdata.Sysbyte, HCACK, subAckList);
            }
            else
            {
                // HCACK = 0x00 인 경우 CellInfo까지 확인하고 Ack를 전송함. 
            }

            return ret;
        }

        private bool VerifyActParameters(PPSelectActReqData ppdata)
        {
            var ret = true;

            List<(string cpName, byte cpAck)> subAckList = new List<(string cpName, byte cpAck)>();

            if (ppdata is ONLINEPPSelectActReqData onlineppdata)
            {
                #region Error Check - PTN, LOTID, PPID, CELLINFO
                // 1. PTN
                (string cpName, byte cpAck) foupAck = ("PTN", 0x00);
                var foupcnt = SystemModuleCount.ModuleCnt.FoupCount;
                var ptnVal = onlineppdata.PTN;

                if (ptnVal < 1 || ptnVal > foupcnt)
                {
                    foupAck.cpAck = 0x02; // illegal value
                    //foupAck.cpName = "PTNERROR";
                    subAckList.Add(foupAck);
                }
                // 2. LOTID
                // 3. PPID
                // 4. CELLINFO
                (string cpName, byte cpAck) cellInfoAck = ("CELLINFO", 0x00);

                var cellcnt = SystemModuleCount.ModuleCnt.StageCount;
                var cellmap = ppdata.UseStageNumbers_str.Replace(",", "");
                if (cellmap.Length == cellcnt)
                {
                    for (int i = 0; i < cellmap.Length; i++)
                    {
                        if (!(cellmap[i] == '1' || cellmap[i] == '0'))
                        {
                            cellInfoAck.cpAck = 0x02; // illegal value
                                                      //cellInfoAck.cpName = "CELLINFOERROR";
                            break;
                        }
                        if (cellmap[i] == '1')
                        {
                            if (!GetConnectState(Convert.ToInt32(i + 1)))
                            {
                                cellInfoAck.cpAck = 0x02; // illegal value
                                                          //cellInfoAck.cpName = "CELLNOTCONNECTED";
                                break;

                            }
                        }
                    }
                }
                else
                {
                    cellInfoAck.cpAck = 0x02; // illegal value
                                              //cellInfoAck.cpName = "CELLINFOERROR";
                }

                if (cellInfoAck.cpAck != 0x00) { subAckList.Add(cellInfoAck); }
                #endregion

                #region Send Ack
                byte HCACK = 0x00;  // 0 - ok, completed
                if (subAckList.Count > 0)
                {
                    HCACK = 0x03;   // 3 - parameter error
                    ret = false;
                    //var dialogManager = this.GetLoaderContainer().Resolve<MetroDialogInterfaces.IMetroDialogManager>();
                    //dialogManager.ShowMessageDialog("[ERROR]",
                    //                                $"Invalid Cell Info.",
                    //                                MetroDialogInterfaces.EnumMessageStyle.AffirmativeAndNegative);
                }
                else
                {
                    // [STM_CATANIA] ONLINE_PPSELECT 이후 GEM Command에서 받은 Info와 비교하기 위해
                    var lotinfos = this.GetLoaderContainer().Resolve<LoaderBase.ILoaderSupervisor>().ActiveLotInfos;
                    List<string> errorCheckKey = new List<string>() { "PTN", "LOTID", "CELLINFO" };

                    if (lotinfos[ptnVal - 1].RCMDErrorCheckDic == null)
                    {
                        lotinfos[ptnVal - 1].RCMDErrorCheckDic = new Dictionary<string, string>();
                    }

                    foreach (string key in errorCheckKey)
                    {
                        if (lotinfos[ptnVal - 1].RCMDErrorCheckDic.ContainsKey(key))
                        {
                            lotinfos[ptnVal - 1].RCMDErrorCheckDic.Remove(key);
                        }

                        switch (key)
                        {
                            case "PTN":
                                lotinfos[ptnVal - 1].RCMDErrorCheckDic.Add(key, ptnVal.ToString());
                                break;
                            case "LOTID":
                                lotinfos[ptnVal - 1].RCMDErrorCheckDic.Add(key, onlineppdata.LotID);
                                break;
                            case "CELLINFO":
                                lotinfos[ptnVal - 1].RCMDErrorCheckDic.Add(key, ppdata.UseStageNumbers_str);
                                break;
                        }
                    }
                }
                SendPPSelectACK(ppdata.ObjectID, ppdata.Stream, ppdata.Function, ppdata.Sysbyte, HCACK, subAckList);
                #endregion
            }
            return ret;
        }

        public bool GetConnectState(int index = 0)
        {
            bool retVal = false;
            try
            {
                var loaderCommunicationManager_ = this.GetLoaderContainer().Resolve<LoaderBase.Communication.ILoaderCommunicationManager>();
                var stage = loaderCommunicationManager_.GetStage(index);
                if (stage != null)
                {

                    if (stage.StageInfo.IsConnected)
                    {
                        retVal = true;
                    }
                    else
                    {
                        retVal = false;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        private void SendPPSelectACK(long objectID, long pStream, long pFunc, long pSysbyte, byte ack, List<(string cpName, byte cpAck)> subAckList)
        {
            var gemService = this.GetLoaderContainer().Resolve<IGEMModule>().GemCommManager.GetSecsGemServiceModule();
            long pnObjectID = objectID;
            gemService.MakeObject(ref pnObjectID);
            gemService.SetListItem(pnObjectID, 2);
            gemService.SetBinaryItem(pnObjectID, ack);
            gemService.SetListItem(pnObjectID, subAckList.Count);
            for (int i = 0; i < subAckList.Count; i++)
            {
                gemService.SetListItem(pnObjectID, 2);
                gemService.SetStringItem(pnObjectID, subAckList[i].cpName);
                gemService.SetBinaryItem(pnObjectID, subAckList[i].cpAck);
            }
            gemService.SendSECSMessage(pnObjectID, pStream, pFunc + 1, pSysbyte);

        }

        private EventCodeEnum DeviceChange(PPSelectActReqData ppdata, string deviceName)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            var deviceChangeReqData = new DeviceChangeActReqData();
            deviceChangeReqData.NewDeviceName = deviceName;
            deviceChangeReqData.UsingStageList = ppdata.UseStageNumbers;

            try
            {
                var Container = this.GetLoaderContainer();
                var loaderCommManager = this.GetLoaderContainer().Resolve<LoaderBase.Communication.ILoaderCommunicationManager>();

                foreach (var i in deviceChangeReqData.UsingStageList)
                {
                    var stage = loaderCommManager.GetStage(i);
                    if (stage != null)
                    {
                        if (stage.StageInfo.IsConnected)
                        {
                            var mediumProxy = loaderCommManager.GetProxy<IRemoteMediumProxy>(stage.Index);

                            //mediumProxy.DeviceChangeWithName_ChangeDeviceCommand(deviceChangeReqData.NewDeviceName).Wait();
                            retVal = loaderCommManager.DeviceReload(stage, true).Result;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private EventCodeEnum LotModeChange(PPSelectActReqData ppdata, string lotMode)
        {
            var lotModeChangeActReqData = new LotModeChangeActReqData();
            lotModeChangeActReqData.LotMode = lotMode;
            lotModeChangeActReqData.UsingStageList = ppdata.UseStageNumbers;
            if (ppdata is ONLINEPPSelectActReqData onlineppdata)
            {
                lotModeChangeActReqData.PTN = onlineppdata.PTN;
            }
            this.GEMModule().GemCommManager.OnRemoteCommandAction(lotModeChangeActReqData);
            return EventCodeEnum.NONE;
        }
    }

    public class DeviceChange : GemActBehaviorBase
    {
        public DeviceChange()
        {

        }

        public override EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var reqData = actReqData as DeviceChangeActReqData;

                var Container = this.GetLoaderContainer();
                var loaderCommManager = this.GetLoaderContainer().Resolve<LoaderBase.Communication.ILoaderCommunicationManager>();

                foreach (var i in reqData.UsingStageList)
                {
                    var stage = loaderCommManager.GetStage(i);
                    if (stage != null)
                    {
                        if (stage.StageInfo.IsConnected)
                        {
                            var mediumProxy = loaderCommManager.GetProxy<IRemoteMediumProxy>(stage.Index);

                            mediumProxy.DeviceChangeWithName_ChangeDeviceCommand(reqData.NewDeviceName).Wait();
                            loaderCommManager.DeviceReload(stage, true).Wait();
                        }
                    }

                }
                this.EventManager().RaisingEvent(typeof(DeviceChangedEvent).FullName);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ExcuteExcuter(RemoteActReqData actReqData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class LotModeChange : GemActBehaviorBase
    {
        public LotModeChange()
        {

        }

        public override EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var reqData = actReqData as LotModeChangeActReqData;

                var loaderCommManager = this.GetLoaderContainer().Resolve<LoaderBase.Communication.ILoaderCommunicationManager>();
                ILoaderSupervisor LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();

                foreach (var i in reqData.UsingStageList)
                {
                    var stage = loaderCommManager.GetStage(i);
                    if (stage != null)
                    {
                        if (stage.StageInfo.IsConnected)
                        {
                            switch (reqData.LotMode)
                            {
                                //Normal Probing
                                case "0":
                                    stage.StageInfo.LotData.LotMode = LotModeEnum.CP1;
                                    break;
                                // Multi Pass Probing
                                case "1":
                                    stage.StageInfo.LotData.LotMode = LotModeEnum.MPP;
                                    break;
                                default:
                                    stage.StageInfo.LotData.LotMode = LotModeEnum.UNDEFINED;
                                    break;
                            }

                            var proxy = loaderCommManager.GetProxy<IStageSupervisorProxy>(stage.Index);

                            if (proxy != null)
                            {
                                proxy.ChangeLotMode(stage.StageInfo.LotData.LotMode);
                            }

                            if (reqData.PTN > 0)
                            {
                                LoaderMaster.Loader.Foups[reqData.PTN - 1].LotSettings[i - 1].LotMode = stage.StageInfo.LotData.LotMode;

                            }
                        }
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ExcuteExcuter(RemoteActReqData actReqData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class Resume : GemActBehaviorBase
    {
        public Resume()
        {

        }

        public override EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                byte HAACK = 0x04;
                this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HAACK, actReqData.Count);

                StartStage reqData = actReqData as StartStage;
                if (reqData != null)
                {
                    if (reqData.StageNumber != 0)
                    {
                        //stage resume
                        gemServiceHost.OnRemoteCommandAction(((StartStage)reqData).StageNumber, reqData);
                        LoggerManager.Debug($"[GEM COMMANDER] {reqData?.ActionType} - stage number {reqData?.StageNumber} : ");
                    }
                    else
                    {
                        //Lot Resume
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ExcuteExcuter(RemoteActReqData actReqData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var reqData = actReqData as StartStage;

                if (reqData != null)
                {
                    this.CommandManager().SetCommand<IResumeProbing>(this.ProbingModule());
                    LoggerManager.Debug($"[GEM EXECUTER] {actReqData?.ActionType}  Resume");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class StartCardChange : GemActBehaviorBase
    {
        public StartCardChange()
        {

        }

        public override EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                bool canExecute = false;
                byte HCACK = 0x02;
                int stgnum = 0;
                int cardbuffernum = 0;
                // IdleState인지 체크 후 Reject이면 0x02로 리턴 

                ICardChangeSupervisor cardChangeSupervisor = this.GetLoaderContainer().Resolve<ICardChangeSupervisor>();
                ILoaderSupervisor master = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                ICardBufferModule cardloadport = null;
                ICCModule cardreqmodule = null;

                if (master?.Loader.ModuleManager.FindModules<ICardBufferModule>().Count() == 1)
                {
                    cardbuffernum = 1;
                }


                if (actReqData is StageActReqData)
                {
                    var reqData = actReqData as StageActReqData;
                    stgnum = reqData.StageNumber;

                    cardloadport = master?.Loader.ModuleManager.FindModule<ICardBufferModule>(ModuleTypeEnum.CARDBUFFER, cardbuffernum);
                    cardreqmodule = master?.Loader.ModuleManager.FindModule<ICCModule>(ModuleTypeEnum.CC, stgnum);

                    canExecute = cardChangeSupervisor.CanStartCardChange(cardloadport, cardreqmodule);
                    LoggerManager.Debug($"[GEM COMMANDER] {reqData?.ActionType} - StageNumber {reqData?.StageNumber} : ");
                }


                if (canExecute)
                {
                    string hash = DateTime.Now.GetHashCode().ToString();

                    EventCodeEnum allocRst = cardChangeSupervisor.AllocateActiveCCInfo(new ActiveCCInfo(hash, cardloadport, cardreqmodule));
                    if (allocRst != EventCodeEnum.NONE)
                    {
                        HCACK = 0x02;// 2 - cannot do now
                    }
                    else
                    {
                        var joninfo = new RequestCCJobInfo() { allocSeqId = hash };
                        this.GEMModule().CommandManager().SetCommand<IStartCardChangeSequence>(cardChangeSupervisor, joninfo);
                        HCACK = 0x04;//4 - initiated for asynchronous completion

                        //var client = master?.GetClient(stgnum);
                        //if(client != null && master.IsAliveClient(client))
                        //{
                        //    bool needToSetSV = client.NeedToSetCCActivatableTemp();
                        //    if (needToSetSV)
                        //    {
                        //        client.SetCCActiveTemp();
                        //    }                         
                        //}
                    }
                }
                else
                {
                    HCACK = 0x02;// 2 - cannot do now
                }

                this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HCACK, actReqData.Count);
                LoggerManager.Debug($"[GEM COMMANDER] {actReqData?.ActionType} Replied {HCACK} ");

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ExcuteExcuter(RemoteActReqData actReqData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class TransferCardLPToCardReqModule : GemActBehaviorBase
    {
        public TransferCardLPToCardReqModule()
        {

        }

        public override EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                bool canExecute = false;
                byte HCACK = 0x02;
                string errmsg = string.Empty;
                int stgnum = 0;
                ICardChangeSupervisor cardChangeSupervisor = this.GetLoaderContainer().Resolve<ICardChangeSupervisor>();

                if (actReqData is StageSeqActReqData)
                {
                    var reqData = actReqData as StageSeqActReqData;
                    //카드 버퍼가 presence detach 상태이고 값이 EXIST, UNPROCESSED여야함.
                    //CardHolder의 상태는 exist, unprocessed 여야함.
                    stgnum = reqData.StageNumber;
                    ILoaderSupervisor Master = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                    var cardbuffer = Master.Loader.ModuleManager.FindModule<ICardBufferModule>(ModuleTypeEnum.CARDBUFFER, cardChangeSupervisor.GetRunningCCInfo().cardlpIndex);


                    if (cardbuffer.CardPRESENCEState == ProberInterfaces.CardChange.CardPRESENCEStateEnum.CARD_DETACH)
                    {
                        if (stgnum > 0)
                        {
                            var ccModule = Master.Loader.ModuleManager.FindModule<ICCModule>(ModuleTypeEnum.CC, stgnum);
                            bool isvalid = Master.ValidateTransferCardObject(cardbuffer, ccModule);
                            if (isvalid)
                            {
                                this.GEMModule().CommandManager().SetCommand<ITransferObject>(cardChangeSupervisor, new TransferObjectHolderInfo() { source = cardbuffer, target = ccModule });
                                canExecute = true;
                            }
                            else
                            {
                                errmsg = $"cannot transfer. cur:(cardbuffer:{cardbuffer.Holder.Status}, ccModule:{ccModule.Holder.Status})";
                            }
                        }
                    }
                    else
                    {
                        errmsg = $"Cardbuffer presence state is not detach. cur:{cardbuffer.CardPRESENCEState}.";
                    }

                    LoggerManager.Debug($"[GEM COMMANDER] {reqData?.ActionType} - StageNumber {reqData?.StageNumber} : {errmsg}");
                }

                if (canExecute)
                {
                    HCACK = 0x04;//4 - initiated for asynchronous completion
                }
                else
                {
                    HCACK = 0x02;// 2 - cannot do now
                }

                this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HCACK, actReqData.Count);
                LoggerManager.Debug($"[GEM COMMANDER] {actReqData?.ActionType} Replied {HCACK} ");

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ExcuteExcuter(RemoteActReqData actReqData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class TransferCardLPToCardIdReader : GemActBehaviorBase
    {
        public TransferCardLPToCardIdReader()
        {

        }

        public override EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                bool canExecute = false;
                byte HCACK = 0x02;
                string errmsg = string.Empty;

                ICardChangeSupervisor cardChangeSupervisor = this.GetLoaderContainer().Resolve<ICardChangeSupervisor>();

                if (actReqData is StageActReqData)
                {
                    var reqData = actReqData as StageActReqData;
                    //카드 버퍼가 presence detach 상태이고 값이 EXIST, UNPROCESSED여야함.
                    //CardHolder의 상태는 exist, unprocessed 여야함.                    

                    ILoaderSupervisor Master = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();

                    var cardbuffer = Master.Loader.ModuleManager.FindModule<ICardBufferModule>(ModuleTypeEnum.CARDBUFFER, cardChangeSupervisor.GetRunningCCInfo().cardlpIndex);
                    var cardArm = Master.Loader.ModuleManager.FindModule<ICardARMModule>(ModuleTypeEnum.CARDARM, 1);

                    if (cardbuffer?.CardPRESENCEState == ProberInterfaces.CardChange.CardPRESENCEStateEnum.CARD_ATTACH)
                    {
                        bool isvalid = Master.ValidateTransferCardObject(cardbuffer, cardArm);
                        if (isvalid)
                        {
                            this.GEMModule().CommandManager().SetCommand<ITransferObject>(cardChangeSupervisor, new TransferObjectHolderInfo() { source = cardbuffer, target = cardArm });
                            canExecute = true;
                        }
                        else
                        {
                            errmsg = $"cannot transfer. cur:(cardbuffer:{cardbuffer.Holder.Status}, cardArm:{cardArm.Holder.Status})";
                        }
                    }
                    else
                    {
                        errmsg = $"Cardbuffer presence state is not attach. cur:{cardbuffer.CardPRESENCEState}.";
                    }

                    LoggerManager.Debug($"[GEM COMMANDER] {reqData?.ActionType} - StageNumber {reqData?.StageNumber} : {errmsg}");
                }

                if (canExecute)
                {
                    HCACK = 0x04;//4 - initiated for asynchronous completion
                }
                else
                {
                    HCACK = 0x02;// 2 - cannot do now
                }

                this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HCACK, actReqData.Count);
                LoggerManager.Debug($"[GEM COMMANDER] {actReqData?.ActionType} Replied {HCACK} ");

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ExcuteExcuter(RemoteActReqData actReqData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class TransferCardIdReaderToCardReqModule : GemActBehaviorBase
    {
        public TransferCardIdReaderToCardReqModule()
        {

        }

        public override EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                bool canExecute = false;
                byte HCACK = 0x02;
                string errmsg = string.Empty;
                int stgnum = 0;
                ICardChangeSupervisor cardChangeSupervisor = this.GetLoaderContainer().Resolve<ICardChangeSupervisor>();

                if (actReqData is StageSeqActReqData)
                {
                    var reqData = actReqData as StageSeqActReqData;
                    //카드 버퍼가 presence detach 상태이고 값이 EXIST, UNPROCESSED여야함.
                    //CardHolder의 상태는 exist, unprocessed 여야함.
                    stgnum = reqData.StageNumber;

                    ILoaderSupervisor Master = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();

                    var cardArm = Master.Loader.ModuleManager.FindModule<ICardARMModule>(ModuleTypeEnum.CARDARM, 1);
                    if (stgnum > 0)
                    {
                        var ccModule = Master.Loader.ModuleManager.FindModule<ICCModule>(ModuleTypeEnum.CC, stgnum);
                        bool isvalid = Master.ValidateTransferCardObject(cardArm, ccModule);
                        if (isvalid)
                        {
                            this.GEMModule().CommandManager().SetCommand<ITransferObject>(cardChangeSupervisor, new TransferObjectHolderInfo() { source = cardArm, target = ccModule });
                            canExecute = true;
                        }
                        else
                        {
                            errmsg = $"cannot transfer. cur:(cardArm:{cardArm.Holder.Status}, ccModule:{ccModule.Holder.Status})";
                        }
                    }
                    LoggerManager.Debug($"[GEM COMMANDER] {reqData?.ActionType} - StageNumber {reqData?.StageNumber} : {errmsg}");
                }

                if (canExecute)
                {
                    HCACK = 0x04;//4 - initiated for asynchronous completion
                }
                else
                {
                    HCACK = 0x02;// 2 - cannot do now
                }

                this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HCACK, actReqData.Count);
                LoggerManager.Debug($"[GEM COMMANDER] {actReqData?.ActionType} Replied {HCACK} ");

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ExcuteExcuter(RemoteActReqData actReqData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class CardChangeSeqAbort : GemActBehaviorBase
    {
        public CardChangeSeqAbort()
        {

        }

        public override EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                byte HCACK = 0x02;

                ICardChangeSupervisor cardChangeSupervisor = this.GetLoaderContainer().Resolve<ICardChangeSupervisor>();

                cardChangeSupervisor.CommandManager().SetCommand<IAbortCardChangeSequence>(cardChangeSupervisor);

                HCACK = 0x04;//4 - initiated for asynchronous completion

                this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HCACK, actReqData.Count);
                LoggerManager.Debug($"[GEM COMMANDER] {actReqData?.ActionType} Replied {HCACK} ");

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ExcuteExcuter(RemoteActReqData actReqData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }


    public class StartWaferChange : GemActBehaviorBase
    {
        public StartWaferChange()
        {

        }
        private EventCodeEnum LocationDataValidation(string[] load_port_info, string[] location_info, out string msg)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            msg = "";
            try
            {
                //Atom Index Data Validation
                if (location_info == null || location_info.Length <= 0)
                {
                    retVal = EventCodeEnum.EXCEPTION;
                    msg = $"The Location data is null.";
                }
                else
                {
                    for (int index = 0; index < location_info.Length; index++)
                    {
                        if (location_info[index] != null && location_info[index] != "" && location_info[index].Length != 0)
                        {
                            var strloc = location_info[index].ToArray();
                            if (strloc[0] == 'S' || strloc[0] == 's')
                            {
                                //Location이 Slot일 때 LoadPort Validation
                                if (load_port_info[index] == null || load_port_info[index].Length <= 0)
                                {
                                    retVal = EventCodeEnum.EXCEPTION;
                                    msg = $"The load port number value is null. Data Index: {index}, Input Value: {load_port_info[index]}";
                                }
                                else
                                {
                                    string onlynumber_lp = load_port_info[index].Remove(0, 2);
                                    if (int.TryParse(onlynumber_lp, out int loadport_number))
                                    {
                                        if (loadport_number <= 0 || loadport_number > SystemModuleCount.ModuleCnt.FoupCount)
                                        {
                                            retVal = EventCodeEnum.EXCEPTION;
                                            msg = $"The load port number value is invalid. Data Index: {index}, Input Value: {load_port_info[index]}, 0 < Load Port Number < {SystemModuleCount.ModuleCnt.FoupCount}";
                                        }
                                        else
                                        {
                                            string onlynumber_slot = location_info[index].Remove(0, 1);
                                            if (int.TryParse(onlynumber_slot, out int slot_number))
                                            {
                                                if (slot_number <= 0 || slot_number > SystemModuleCount.ModuleCnt.SlotCount)
                                                {
                                                    retVal = EventCodeEnum.EXCEPTION;
                                                    msg = $"The location number value is invalid. Data Index: {index}, Value: {location_info[index]}, 0 < Index < {SystemModuleCount.ModuleCnt.SlotCount}";
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        retVal = EventCodeEnum.EXCEPTION;
                                        msg = $"The load port number value is invalid. Data Index: {index}, Input Value: {load_port_info[index]}, 0 < Load Port Number < {SystemModuleCount.ModuleCnt.FoupCount}";
                                        return retVal;
                                    }
                                }
                            }
                            else if (strloc[0] == 'F' || strloc[0] == 'f')
                            {
                                if (load_port_info[index] == null || load_port_info[index].Length <= 0)
                                {
                                    string onlynumbers_fixed = location_info[index].Remove(0, 1);
                                    int fixedtray_number;
                                    if (int.TryParse(onlynumbers_fixed, out fixedtray_number))
                                    {
                                        if (fixedtray_number <= 0 || fixedtray_number > SystemModuleCount.ModuleCnt.FixedTrayCount)
                                        {
                                            retVal = EventCodeEnum.EXCEPTION;
                                            msg = $"The location number is invalid.  Data Index: {index}, Value: {location_info[index]},  0 < Index < {SystemModuleCount.ModuleCnt.FixedTrayCount}";
                                        }
                                    }
                                }
                                else
                                {
                                    retVal = EventCodeEnum.EXCEPTION;
                                    msg = $"The load port number value is invalid. Data Index: {index}, Value: {location_info[index]}";
                                }
                            }
                            else
                            {
                                retVal = EventCodeEnum.EXCEPTION;
                                msg = $"The location value is invalid. Data Index: {index}, Value: {location_info[index]}, Make sure the first character is an \'S\' or \'F\'. ";
                            }
                        }
                        else
                        {
                            retVal = EventCodeEnum.EXCEPTION;
                            msg = $"The LOC1_Atom_Idx data[{index}] value is null.";
                        }
                        if (retVal == EventCodeEnum.EXCEPTION)
                        {
                            break;
                        }
                    }
                }
                if (retVal == EventCodeEnum.UNDEFINED)
                {
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        private EventCodeEnum CheckValidation(WaferChangeData actReqData, out string errormsg)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            errormsg = "";
            try
            {
                if (actReqData == null)
                {
                    retVal = EventCodeEnum.EXCEPTION;
                    errormsg = $"actReqData is null";
                    return retVal;
                }

                //loader state가 error, abort 인 경우
                ILoaderSupervisor LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                ModuleStateEnum loadermodulestate = LoaderMaster.Loader.ModuleState; // Loader가 실제 움직이는지?에 대한 state
                ModuleStateEnum loadermastermodulestate = LoaderMaster.ModuleState.State; // Lot 관련한 Loader의 State

                if (loadermodulestate == ModuleStateEnum.ABORT || loadermodulestate == ModuleStateEnum.ERROR ||
                    loadermastermodulestate == ModuleStateEnum.ABORT || loadermastermodulestate == ModuleStateEnum.ERROR)
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] CheckValidation() : Loader Module State = {loadermodulestate}, Loader Master State  = {loadermastermodulestate}, ");
                    retVal = EventCodeEnum.LOADER_STATE_INVALID;
                    errormsg = "Loader State is Abort or Error";
                    return retVal;
                }

                // wafer change 이미 동작중인 경우

                IWaferChangeSupervisor waferchangesupervisor = this.GetLoaderContainer().Resolve<IWaferChangeSupervisor>();

                ModuleStateEnum waferChangeModuleState = waferchangesupervisor.ModuleState.GetState();

                bool isModuleBusy = waferChangeModuleState != ModuleStateEnum.IDLE ||
                                    (waferchangesupervisor.WaferChangeAutofeed.AutoFeedActions?.Count > 0);

                if (isModuleBusy)
                {
                    int activeModuleCount = waferchangesupervisor.WaferChangeAutofeed.AutoFeedActions?.Count ?? 0; 
                    LoggerManager.Debug($"[{this.GetType().Name}] CheckValidation() : ModuleState = {waferChangeModuleState}, ActiveModuleList = {activeModuleCount}");

                    retVal = EventCodeEnum.EXCEPTION;
                    errormsg = "The command is already running.";
                    return retVal;
                }

                //"OCR Read" Validation
                if (int.TryParse(actReqData.OCRRead.ToString(), out int ocrRead) == false)
                {
                    retVal = EventCodeEnum.EXCEPTION;
                    errormsg = $"OCRRead Value is invalid. OCRRead: {ocrRead}"; 
                    return retVal;
                }
                else
                {
                    if (ocrRead < 0 || ocrRead > 2)
                    {
                        retVal = EventCodeEnum.EXCEPTION;
                        errormsg = $"OCRRead Value is invalid. OCRRead: {ocrRead}";
                        return retVal;
                    }
                }


                EventCodeEnum loc_Val = EventCodeEnum.UNDEFINED;
                string msg_loc1 = "";
                loc_Val = this.LocationDataValidation(actReqData.LOC1_LP, actReqData.LOC1_Atom_Idx, out msg_loc1);
                if (loc_Val != EventCodeEnum.NONE)
                {
                    retVal = loc_Val;
                    errormsg = msg_loc1;
                    return retVal;
                }

                string msg_loc2 = "";
                loc_Val = this.LocationDataValidation(actReqData.LOC2_LP, actReqData.LOC2_Atom_Idx, out msg_loc2);
                if (loc_Val != EventCodeEnum.NONE)
                {
                    retVal = loc_Val;
                    errormsg = msg_loc2;
                    return retVal;
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.Error($"CheckValidation(): CheckValidation Error occurred. Err = {err.Message}, msg: = {errormsg}");
            }
            return retVal;
        }

        public override EventCodeEnum ExcuteCommander(RemoteActReqData actReqData, ISecsGemServiceHost gemServiceHost)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            byte HCACK = 0x02;
            try
            {
                IWaferChangeSupervisor waferchangesupervisor = this.GetLoaderContainer().Resolve<IWaferChangeSupervisor>();

                var reqData = actReqData as WaferChangeData;

                if (this.CheckValidation(reqData, out string errorlog) == EventCodeEnum.NONE)
                {
                    waferchangesupervisor.AllocateAutoFeedActions(reqData);

                    //canExecute = waferchangesupervisor.CanStartWaferChange();

                    this.GEMModule().CommandManager().SetCommand<IStartWaferChangeSequence>(waferchangesupervisor);

                    HCACK = 0x00; // HCACK = 0x04; 원하는걸로 선택하세요~
                    this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HCACK, actReqData.Count);
                    LoggerManager.Debug($"[GEM COMMANDER] {actReqData?.ActionType} Replied {HCACK} ");

                }
                else
                {
                    waferchangesupervisor.CSTAutoUnloadAfterWaferChange();

                    LoggerManager.Debug($"[GEM COMMANDER] {actReqData?.ActionType} rejected reason: {errorlog}");
                    HCACK = 0x02;
                    this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HCACK, actReqData.Count);
                    LoggerManager.Debug($"[GEM COMMANDER] {actReqData?.ActionType} Replied {HCACK} ");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[GEM COMMANDER] {actReqData?.ActionType} exception: {err}");
                HCACK = 0x02;
                this.GEMModule().SendAck(actReqData.ObjectID, actReqData.Stream, actReqData.Function, actReqData.Sysbyte, HCACK, actReqData.Count);
                LoggerManager.Exception(err);
                return EventCodeEnum.EXCEPTION;
            }

            return retVal;
        }

        public override EventCodeEnum ExcuteExcuter(RemoteActReqData actReqData)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }
}




