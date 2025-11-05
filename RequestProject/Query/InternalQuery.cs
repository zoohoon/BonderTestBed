using LoaderParameters;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using LoaderController.GPController;

namespace RequestCore.QueryPack
{
    [Serializable]
    public class GetElement : QueryData
    {
        public string Fullpath = "";
        public GetElement(string propertypath)
        {
            Fullpath = propertypath;
        }
        
        public override EventCodeEnum Run()
        {
            this.Result = this.ParamManager().GetElement(propertyPath: Fullpath).GetValue();

            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class DeviceName : QueryData
    {
        public override EventCodeEnum Run()
        {
            this.Result = this.LotOPModule().LotInfo.DeviceName.Value;

            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class WaferSize : QueryData
    {
        public override EventCodeEnum Run()
        {
            try
            {
                double? size = this.StageSupervisor().WaferObject.GetPhysInfo().WaferSize_um.Value;
                if (size != null)
                {
                    this.Result = size;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            
            return EventCodeEnum.NONE;
        }
    }
    [Serializable]
    public class WaferRadius100um : QueryData
    {
        public override EventCodeEnum Run()
        {
            try
            {
                string size = (new WaferSize()).GetRequestResult().ToString();

                if (size != null)
                {
                    double dsize = double.Parse(size);
                    if (dsize/2 >= 9999)
                    {
                        this.Result = 9999;
                    }
                    else
                    {
                        this.Result = (int)(dsize / 2);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class WaferSizeInt: QueryData
    {
        public override EventCodeEnum Run()
        {
            try
            {
                switch (this.StageSupervisor().WaferObject.GetPhysInfo().WaferSizeEnum)
                {
                    case EnumWaferSize.INCH6:
                        this.Result = "6";
                        break;
                    case EnumWaferSize.INCH8:
                        this.Result = "8";
                        break;
                    case EnumWaferSize.INCH12:
                        this.Result = "12";
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class MultiChip : QueryData
    {
        public override EventCodeEnum Run()
        {
            try
            {
                if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.Count <= 8)
                {
                    this.Result = "0";
                }
                else
                {
                    this.Result = "G";
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
    }

   [Serializable]
    public class PMIEnable : QueryData
    {
        public override EventCodeEnum Run()
        {
            try
            {
                this.Result = this.PMIModule().GetPMIEnableParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
    }
    [Serializable]
    public class WaferSizeInch : QueryData
    {
        public override EventCodeEnum Run()
        {
            try
            {
                switch (this.StageSupervisor().WaferObject.GetPhysInfo().WaferSizeEnum)
                {
                    case EnumWaferSize.INCH6:
                        this.Result = '6';
                        break;
                    case EnumWaferSize.INCH8:
                        this.Result = '8';
                        break;
                    case EnumWaferSize.INCH12:
                        this.Result = 'C';
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return EventCodeEnum.NONE;
        }
    }

  
    //[Serializable]
    //public class WaferSizeInch : QueryData
    //{
    //    public override EventCodeEnum Run()
    //    {
    //        double dblwafersize = this.StageSupervisor().WaferObject.GetPhysInfo().WaferSize_um.Value;

    //        string strwafersize = string.Empty;

    //        if (dblwafersize == 150000)
    //        {
    //            strwafersize = "6";
    //        }
    //        else if (dblwafersize == 200000)
    //        {
    //            strwafersize = "8";
    //        }
    //        else if (dblwafersize == 300000)
    //        {
    //            strwafersize = "12";
    //        }
    //        else
    //        {
    //            strwafersize = "Undefined";
    //        }

    //        this.Result = strwafersize;

    //        return EventCodeEnum.NONE;
    //    }
    //}

    

    [Serializable]
    public class FlatOrientation : QueryData
    {
        public override EventCodeEnum Run()
        {
            try
            {
                //this.Result = this.StageSupervisor().WaferObject.GetPhysInfo().NotchAngle.Value.ToString();
                if (this.StageSupervisor().VisionManager().GetDispHorFlip() == DispFlipEnum.FLIP
                    && this.StageSupervisor().VisionManager().GetDispVerFlip() == DispFlipEnum.FLIP)
                {
                    int nonflip = (int)(this.StageSupervisor().WaferObject.GetPhysInfo().NotchAngle.Value + this.StageSupervisor().WaferObject.GetPhysInfo().NotchAngleOffset.Value);
                    
                    nonflip += 180;
                    nonflip = ((int)nonflip) % 360;
                    if (nonflip > 360)
                    {
                        nonflip = nonflip - 360;
                    }

                    this.Result = nonflip;
                }
                else
                {
                    this.Result = (this.StageSupervisor().WaferObject.GetPhysInfo().NotchAngle.Value + this.StageSupervisor().WaferObject.GetPhysInfo().NotchAngleOffset.Value).ToString();
                }

                return EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

           

            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class OverDrive : QueryData
    {
        public override EventCodeEnum Run()
        {
            this.Result = this.ProbingModule().OverDrive;

            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class EdgeCorrection : QueryData
    {
        public override EventCodeEnum Run()
        {
            var gPIBSysParam = this.StageSupervisor().GPIB().GPIBSysParam_IParam as IGPIBSysParam;
            if (gPIBSysParam != null)
            {
                this.Result = this.GPIB().GPIBSysParam_IParam.EdgeCorrection.Value;
            }            

            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class ConsecutiveFailMode : QueryData
    {
        public override EventCodeEnum Run()
        {
            var gPIBSysParam = this.StageSupervisor().GPIB().GPIBSysParam_IParam as IGPIBSysParam;
            if (gPIBSysParam != null)
            {
                this.Result = this.GPIB().GPIBSysParam_IParam.ConsecutiveFailMode.Value;
            }

            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class ConsecutiveFailSkipLine : QueryData
    {
        public override EventCodeEnum Run()
        {
            var gPIBSysParam = this.StageSupervisor().GPIB().GPIBSysParam_IParam as IGPIBSysParam;
            if (gPIBSysParam != null)
            {
                this.Result = this.GPIB().GPIBSysParam_IParam.ConsecutiveFailSkipLine.Value;
            }

            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class MultiTestLocFor2Chan : QueryData
    {
        public override EventCodeEnum Run()
        {
            var gPIBSysParam = this.StageSupervisor().GPIB().GPIBSysParam_IParam as IGPIBSysParam;
            if (gPIBSysParam != null)
            {
                this.Result = this.GPIB().GPIBSysParam_IParam.MultiTestLocFor2Chan.Value;
            }

            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class DeviceSizeWidth : QueryData
    {
        public override EventCodeEnum Run()
        {
            this.Result = this.StageSupervisor().WaferObject.GetPhysInfo().DieSizeX.Value.ToString();

            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class DeviceSizeHeight : QueryData
    {
        public override EventCodeEnum Run()
        {
            this.Result = this.StageSupervisor().WaferObject.GetPhysInfo().DieSizeY.Value.ToString();

            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class OrgUIndexX : QueryData
    {
        public override EventCodeEnum Run()
        {
            IProbingModule ProbingModule = this.ProbingModule();
            ICoordinateManager CoordinateManager = this.CoordinateManager();
            IStageSupervisor StageSupervisor = this.StageSupervisor();
            MachineIndex probingMI = null;
            UserIndex probingUI = null;
            probingMI = ProbingModule.ProbingLastMIndex;
            probingUI = CoordinateManager.MachineIndexConvertToUserIndex(probingMI);

            this.Result = probingUI.XIndex.ToString();

            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class OrgUIndexY : QueryData
    {
        public override EventCodeEnum Run()
        {
            IProbingModule ProbingModule = this.ProbingModule();
            ICoordinateManager CoordinateManager = this.CoordinateManager();
            IStageSupervisor StageSupervisor = this.StageSupervisor();
            MachineIndex probingMI = null;
            UserIndex probingUI = null;
            probingMI = ProbingModule.ProbingLastMIndex;
            probingUI = CoordinateManager.MachineIndexConvertToUserIndex(probingMI);

            this.Result = probingUI.YIndex.ToString();

            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class RefUIndexX : QueryData
    {
        public override EventCodeEnum Run()
        {
            this.Result = this.StageSupervisor().WaferObject.GetPhysInfo().CenU.XIndex.ToString();

            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class RefUIndexY : QueryData
    {
        public override EventCodeEnum Run()
        {
            this.Result = this.StageSupervisor().WaferObject.GetPhysInfo().CenU.YIndex.ToString();

            return EventCodeEnum.NONE;
        }
    }


    [Serializable]
    public class ProbingUserXIndex : QueryData
    {
        public override EventCodeEnum Run()
        {
            try
            {
                IProbingModule ProbingModule = this.ProbingModule();
                ICoordinateManager CoordinateManager = this.CoordinateManager();
                IStageSupervisor StageSupervisor = this.StageSupervisor();
                MachineIndex probingMI = null;
                UserIndex probingUI = null;
                probingMI = ProbingModule.ProbingLastMIndex;
                probingUI = CoordinateManager.MachineIndexConvertToUserIndex(probingMI);

                long XIndex = probingUI.XIndex;
                
                if (XIndex < -999) { XIndex = -999; }
               
                this.Result = XIndex;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class ProbingUserYIndex : QueryData
    {
        public override EventCodeEnum Run()
        {
            IProbingModule ProbingModule = this.ProbingModule();
            ICoordinateManager CoordinateManager = this.CoordinateManager();
            IStageSupervisor StageSupervisor = this.StageSupervisor();
            MachineIndex probingMI = null;
            UserIndex probingUI = null;
            probingMI = ProbingModule.ProbingLastMIndex;
            probingUI = CoordinateManager.MachineIndexConvertToUserIndex(probingMI);

            long YIndex = probingUI.YIndex;
          
            if (YIndex < -999) { YIndex = -999; }

            this.Result = YIndex;

            return EventCodeEnum.NONE;
        }
    }


    [Serializable]
    public class DutList : ArrayData
    {
        public override EventCodeEnum Run()
        {
            this.Result = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList;

            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class ProbingMIndexX : QueryData
    {
        public override EventCodeEnum Run()
        {
            this.Result = this.ProbingModule().ProbingLastMIndex.XIndex.ToString(); //프로빙하는 1번 dut의 index X

            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class ProbingMIndexY : QueryData
    {
        public override EventCodeEnum Run()
        {
            this.Result = this.ProbingModule().ProbingLastMIndex.YIndex.ToString(); //프로빙하는 1번 dut의 index X

            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class OcrID : ArrayData
    {
        public override EventCodeEnum Run()
        {
            Result = this.GetParam_Wafer().GetSubsInfo().WaferID.Value;

            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class WaferOriginID : ArrayData
    {
        public override EventCodeEnum Run()
        {
            int slotcount = 25;
            var wafer = this.GetParam_Wafer().GetSubsInfo();
            string waferid = wafer.WaferID.Value;

            if(waferid != string.Empty)
            {
                Result = waferid.Substring(0, 7) + "-" + (wafer.SlotIndex.Value % slotcount == 0? 25: wafer.SlotIndex.Value % slotcount).ToString("00");
            }
            
            return EventCodeEnum.NONE;
        }
    }



    [Serializable]
    public class WaferThickness : ArrayData
    {
        public override EventCodeEnum Run()
        {
            
            Result = this.GetParam_Wafer().GetPhysInfo().Thickness.Value;

            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class CardID : ArrayData
    {
        public override EventCodeEnum Run()
        {
            var proercardid = this.CardChangeModule().GetProbeCardID();

            //Result = this.GetParam_ProbeCard().ProbeCardDevObjectRef.ProbeCardID.Value;

            Result = proercardid;

            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class CurTemp : QueryData
    {
        public override EventCodeEnum Run()
        {
            Result = this.TempController().TempInfo.CurTemp.Value;

            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class SetTemp : QueryData
    {
        public override EventCodeEnum Run()
        {
            Result = this.TempController().TempInfo.SetTemp.Value;

            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class ProberID : QueryData
    {
        public override EventCodeEnum Run()
        {
            Result = this.FileManager().FileManagerParam.ProberID.Value;

            return EventCodeEnum.NONE;
        }
    }

    /// <summary>
    /// 안쓰는데 다른 사이트의 디바이스 파일에 이렇게 정의되어있어서 없앨 수 없음.
    /// </summary>
    [Serializable]
    public class ProbeCardID : QueryData
    {
        public override EventCodeEnum Run()
        {
            //minskim// holder만 docking 된경우 STM은 CARD ID(holder)를 전달하고 있음, 그외에는 NULL을 리턴하고 있음
            //V22 Release 시에는 NULL 리턴을 기본으로 한다.(STM 재 협의 진행예정) IsExistCard 첫번째 인자가 true일 경우 holder만 docking 된경우 true를 리턴함
            if (this.CardChangeModule().IsExistCard())
            {
                Result = this.CardChangeModule().GetProbeCardID();
            }
            else 
            {
                Result = String.Empty;
            }
            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class LotNmae : QueryData
    {
        public override EventCodeEnum Run()
        {
            Result = this.LotOPModule().LotInfo.LotName.Value; // emul 

            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class LotName : QueryData
    {
        public override EventCodeEnum Run()
        {
            Result = this.LotOPModule().LotInfo.LotName.Value; // emul 

            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class WaferID : QueryData
    {
        public override EventCodeEnum Run()
        {
            Result = this.GetParam_Wafer().GetSubsInfo().WaferID.Value;

            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class CurrentSlotNumber : QueryData
    {
        public override EventCodeEnum Run()
        {
            int? snum = this.StageSupervisor().WaferObject.GetSlotIndex();

            // TODO: TSK Mode 일때 origin 이 inspectiomn이나 다른곳이면 **으로 리턴해야함. 
            if (snum != null )
            {
                Result = snum.ToString();
            }
            else
            {
                Result = string.Empty;
            }

            return EventCodeEnum.NONE;
        }
    }
    [Serializable]
    public class StageNumber : QueryData
    {
        public override EventCodeEnum Run()
        {
            int? snum = this.LoaderController().GetChuckIndex();

            if (snum != null)
            {
                Result = snum.ToString();
            }
            else
            {
                Result = string.Empty;
            }

            return EventCodeEnum.NONE;
        }
    }
    [Serializable]
    public class CurrentXCoordinate : QueryData
    {
        public override EventCodeEnum Run()
        {
            IProbingModule ProbingModule = this.ProbingModule();
            ICoordinateManager CoordinateManager = this.CoordinateManager();

            MachineIndex probingMI = null;
            UserIndex probingUI = null;

            probingMI = ProbingModule.ProbingLastMIndex;
            probingUI = CoordinateManager.MachineIndexConvertToUserIndex(probingMI);

            long XIndex = probingUI.XIndex;
            //long YIndex = probingUI.YIndex;

            Result = XIndex.ToString();

            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class CurrentYCoordinate : QueryData
    {
        public override EventCodeEnum Run()
        {
            IProbingModule ProbingModule = this.ProbingModule();
            ICoordinateManager CoordinateManager = this.CoordinateManager();

            MachineIndex probingMI = null;
            UserIndex probingUI = null;

            probingMI = ProbingModule.ProbingLastMIndex;
            probingUI = CoordinateManager.MachineIndexConvertToUserIndex(probingMI);

            //long XIndex = probingUI.XIndex;
            long YIndex = probingUI.YIndex;

            Result = YIndex.ToString();

            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class PortNumber : QueryData
    {
        public override EventCodeEnum Run()
        {
            if (this.Argument != null)
            {
                Result = this.Argument.ToString();
            }
            else 
            {
                int? portno = this.LotOPModule().LotInfo.FoupNumber.Value;
                Result = portno.ToString();
            }
            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class CassetteNum : QueryData
    {
        public override EventCodeEnum Run()
        {
            string cstId = "";
            try
            {
                cstId = this.LoaderController().GetFoupNumberStr();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            Result = cstId;

            //TODO: if(this.StageSupervisor().WaferObject.GetSubsInfo().origin) TSK 버전 에서는 5나 6으로 리턴함.
          
            return EventCodeEnum.NONE;
        }
    }


    [Serializable]
    public class SoakingTimeInSeconds : QueryData
    {
        public override EventCodeEnum Run()
        {
            double seconds = 0;
            try
            {
                seconds = this.StageSupervisor().SoakingModule().GetSoakingTime();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            Result = seconds;

            //TODO: if(this.StageSupervisor().WaferObject.GetSubsInfo().origin) TSK 버전 에서는 5나 6으로 리턴함.

            return EventCodeEnum.NONE;
        }
    }


    [Serializable]
    public class LeftWaferCount : QueryData
    {
        public override EventCodeEnum Run()
        {
            try
            {
                if(SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    string cstId = this.LoaderController().GetFoupNumberStr();
                    if (cstId != string.Empty)
                    {
                        int cstIndex = int.Parse(cstId) - 1;

                        var lotOp = this.StageSupervisor().LotOPModule();
                        if (lotOp != null)
                        {
                            int processedCnt = lotOp.LotInfo.WaferSummarys.Where(wafer => wafer.WaferState == EnumWaferState.PROCESSED).Count();
                            int unProcessedCnt = lotOp.LotInfo.WaferSummarys.Where(wafer => wafer.WaferState == EnumWaferState.UNDEFINED || wafer.WaferState == EnumWaferState.UNPROCESSED).Count();
                            // Set을 안해서 그런지 모르겠지만 UNPROCESSED 되어있는 웨이퍼가 없음. 로드 전의 웨이퍼는 다 UNDEFINED 상태로 있음..
                            int slotCnt = lotOp.LotInfo.WaferSummarys.Count();
                            if ((processedCnt + unProcessedCnt) > 0)
                            {
                                this.Result = slotCnt - processedCnt;
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"LeftWaferCount:() this.StageSupervisor().LotOPModule() is null");
                        }
                    }
                }else if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    GP_LoaderController LoaderController = this.LoaderController() as GP_LoaderController;
                    var UnprocessedWafers = LoaderController.LoaderInfo?.StateMap?
                                        .GetTransferObjectAll().Where(w => w.WaferType.Value == EnumWaferType.STANDARD &&
                                                                           w.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                                                                          ((w.OriginHolder.Index - 1) / 25) + 1 == this.GetParam_Wafer().GetOriginFoupNumber() &&
                                                                          w.WaferState == EnumWaferState.UNPROCESSED &&
                                                                          w.ProcessingEnable == ProcessingEnableEnum.ENABLE &&
                                                                          w.CurrHolder.ModuleType == ModuleTypeEnum.SLOT
                                                                          ).Count();
                    this.Result = UnprocessedWafers;
                }


                  

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }



            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class GetCassetteInfoFromCstNum : QueryData
    {
        public int cstNum = 1;
        public int length = 25;
        string cassetteInfo = string.Empty;
        public override EventCodeEnum Run()
        {
            try
            {                

                LoaderController.LoaderController LoaderController = null;
                LoaderController = this.LoaderController() as LoaderController.LoaderController;

                if (cstNum != 0)
                {
                    if(length > 25)
                    {//single
                        if (this.FoupOpModule().GetFoupController(cstNum)?.FoupModuleInfo.State == ProberInterfaces.Foup.FoupStateEnum.LOAD)
                        {
                            var CassetteModuleInfo = GetCassetteModuleInfo(cstNum - 1, LoaderController);

                            if (CassetteModuleInfo?.ScanState == CassetteScanStateEnum.READ)
                            {
                                string tmpCassetteInfo = "1";

                                if (this.LotOPModule().LotInfo.WaferSummarys.Count(wafer =>
                                (wafer.WaferState == EnumWaferState.UNPROCESSED) || (wafer.WaferState == EnumWaferState.PROBING)) <= 0)
                                {
                                    tmpCassetteInfo = "3";
                                }
                                else if (1 <= CassetteModuleInfo.SlotModules.Count(slot => slot.ModuleType == ModuleTypeEnum.CHUCK))
                                {
                                    tmpCassetteInfo = "2";
                                }

                                cassetteInfo += tmpCassetteInfo;
                            }
                            else
                            {
                                cassetteInfo += "0";
                            }
                        }
                        else
                        {
                            cassetteInfo += "0";
                        }

                        cassetteInfo += GetCassetteWaferInfos(cstNum - 1);
                        if (cassetteInfo.Length < length)
                        {
                            cassetteInfo = cassetteInfo.PadRight(length, '0');
                        }
                    }
                    else
                    {//multi
                        cassetteInfo += GetCassetteWaferInfos(cstNum - 1);
                        if(cassetteInfo.Length < length)
                        {
                            cassetteInfo = cassetteInfo.PadRight(length, '0');
                        }
                    }                    
                }
                else
                {
                    cassetteInfo = "".PadRight(length, '0');
                }
                this.Result = cassetteInfo;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"GetCassetteInfoFromCstNum(): Clear Casette Status. prev:{cassetteInfo}");
                this.Result = "".PadRight(length, '0');// exception 발생 하더라도 기존 포맷 맞춰줘야함. 25
                LoggerManager.Exception(err);
            }
            return EventCodeEnum.NONE;

        }


        private CassetteModuleInfo GetCassetteModuleInfo(int cassetteNum, LoaderController.LoaderController LoaderController)
        {
            CassetteModuleInfo retCassetteModuleInfo = null;

            try
            {
                if(SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    var CassetteModules = LoaderController?.LoaderInfo.StateMap.CassetteModules;

                    if (CassetteModules != null &&
                        cassetteNum <= CassetteModules.Length)
                    {
                        retCassetteModuleInfo = CassetteModules[cassetteNum];
                    }
                }
                else if(SystemManager.SysteMode == SystemModeEnum.Multiple)
                {

                }

                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retCassetteModuleInfo;
        }

        private string GetCassetteWaferInfos(int cassetteNum)
        {
            string retCassetteInfo = string.Empty;            
            IProbingModule probingModule = this.ProbingModule();

            

            try
            {
                if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    var cassetteInfo = this.LotOPModule().LotInfo.WaferSummarys;
                    for (int i = 0; i < cassetteInfo.Count; i++)
                    {
                        if (cassetteInfo[i].WaferStatus == EnumSubsStatus.NOT_EXIST ||
                            cassetteInfo[i].WaferState == EnumWaferState.UNDEFINED ||
                            cassetteInfo[i].WaferState == EnumWaferState.SKIPPED)//not allocated, not exist: 0
                        {
                            retCassetteInfo += "0";
                        }
                        else if (cassetteInfo[i].WaferState == EnumWaferState.UNPROCESSED ||
                                cassetteInfo[i].WaferState == EnumWaferState.SKIPPED ||
                                cassetteInfo[i].WaferState == EnumWaferState.MISSED)
                        {
                            //if (cassetteInfo[i].WaferHolder.ToLower() == "CHUCK".ToLower() &&// ** 나중에 다시 검토할것.                            
                            //    probingModule.ModuleState.State == ModuleStateEnum.RUNNING)//unprocessed probing running(zup): processing 
                            if ((cassetteInfo[i].WaferHolder.ToLower() == "CHUCK".ToLower()) &&
                                (probingModule.ModuleState.State == ModuleStateEnum.SUSPENDED ||
                                probingModule.ModuleState.State == ModuleStateEnum.RUNNING))
                            {
                                retCassetteInfo += "2";
                            }
                            else//unprocessed on other holder //unprocessed && probing suspend(temp wait)** 나중에 다시 검토할것.
                            {
                                retCassetteInfo += "1";
                            }
                        }
                        else if (cassetteInfo[i].WaferState == EnumWaferState.PROCESSED || cassetteInfo[i].WaferState == EnumWaferState.TESTED)//processed: 3 
                        {
                            retCassetteInfo += "3";
                        }
                        else if (cassetteInfo[i].WaferState == EnumWaferState.PROBING)//processing: 2
                        {
                            retCassetteInfo += "2";
                        }
                    }
                }
                else if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    GP_LoaderController LoaderController = this.LoaderController() as GP_LoaderController;
                    var wafersFromCst = LoaderController.LoaderInfo?.StateMap?
                                        .GetTransferObjectAll().Where(w => w.WaferType.Value == EnumWaferType.STANDARD &&
                                                                           w.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                                                                          ((w.OriginHolder.Index - 1) / 25) + 1 == cassetteNum + 1
                                                                          ).OrderBy(w => w.OriginHolder.Index).ToList();
                    if (wafersFromCst == null)
                    {
                        retCassetteInfo = "".PadRight(length, '0');// foup2가 돌았을 것 같은 부분 
                    }


                    for (int i = 1; i <= 25; i++)//slot 개수만큼
                    {
                        var waferExist = wafersFromCst.Where(w => ((w.OriginHolder.Index % 25 == 0) ? 25 : w.OriginHolder.Index % 25) == i);
                        if (waferExist.Count() > 0)
                        {
                            var targetWafer = waferExist.FirstOrDefault();
                            retCassetteInfo += (new WaferStatus() { wafer = targetWafer }).GetRequestResult();
                        }
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retCassetteInfo;
        }
    }


    [Serializable]
    public class WaferStatus : QueryData
    {
        public TransferObject wafer { get; set; }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            string ret = "";
            try
            {
                IProbingModule probingModule = this.ProbingModule();
                if(wafer == null)
                {
                    // 전달 받은 매개변수가 없을 경우 현재 로드된 웨이퍼 기준으로 waferStatus를 반환함.
                    if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                    {
                        GP_LoaderController LoaderController = this.LoaderController() as GP_LoaderController;
                        var wafersFromCst = LoaderController.LoaderInfo?.StateMap?
                                            .GetTransferObjectAll().Where(w => w.CurrHolder.ModuleType == ModuleTypeEnum.CHUCK &&
                                                                               w.CurrHolder.Index == LoaderController.GetChuckIndex()
                                                                              ).FirstOrDefault();
                        wafer = wafersFromCst;
                        if (wafer == null)
                        {
                            ret += "0";
                            return retVal;
                        }
                    }
                }

                if ((wafer.ProcessingEnable == ProcessingEnableEnum.DISABLE) ||
                                 wafer.WaferState == EnumWaferState.UNDEFINED)//not allocated, not exist: 0
                {
                   ret += "0";
                }
                else if (wafer.WaferState == EnumWaferState.UNPROCESSED ||
                    wafer.WaferState == EnumWaferState.SKIPPED ||
                    wafer.WaferState == EnumWaferState.MISSED)
                {
                    //if (cassetteInfo[i].WaferHolder.ToLower() == "CHUCK".ToLower() &&// ** 나중에 다시 검토할것.                            
                    //    probingModule.ModuleState.State == ModuleStateEnum.RUNNING)//unprocessed probing running(zup): processing 
                    if ((wafer.CurrHolder.ModuleType == ModuleTypeEnum.CHUCK) &&
                        (probingModule.ModuleState.State == ModuleStateEnum.SUSPENDED ||
                        probingModule.ModuleState.State == ModuleStateEnum.RUNNING))
                    {
                        ret += "2";
                    }
                    else//unprocessed on other holder //unprocessed && probing suspend(temp wait)** 나중에 다시 검토할것.
                    {
                        ret += "1";
                    }
                }
                else if ((wafer.ProcessingEnable == ProcessingEnableEnum.DISABLE) ||
                          wafer.WaferState == EnumWaferState.UNDEFINED)//not allocated, not exist: 0
                {
                    ret += "0";
                }
                else if (wafer.WaferState == EnumWaferState.PROCESSED || wafer.WaferState == EnumWaferState.TESTED)//processed: 3
                {
                    ret += "3";
                }
                else if (wafer.WaferState == EnumWaferState.PROBING)//processing: 2
                {
                    ret += "2";
                }
            
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                ret = "0";
                LoggerManager.Exception(err);
            }
            finally
            {
                this.Result = ret;
            }
            return retVal;
        }

    }




    [Serializable]
    public class NCEnable : QueryData
    {
        public override EventCodeEnum Run()
        {
            try
            {
                if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    //Needle clean pad 종류 3개다 검사하기
                }
                else
                {
                    List<ProberInterfaces.PolishWafer.IPolishWaferIntervalParameter> PolishWaferInterval 
                        = this.StageSupervisor().PolishWaferModule().GetPolishWaferIntervalParameters();
                    
                    if(PolishWaferInterval != null)
                    {
                        if (PolishWaferInterval.Count <= 0)
                        {
                            this.Result = "N";
                            return EventCodeEnum.NONE;
                        }

                        this.Result = "Y";
                    }
                    else
                    {
                        this.Result = "N";
                        return EventCodeEnum.NONE;
                    }
                }                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }          
            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class NCContactCount : QueryData
    {
        public override EventCodeEnum Run()
        {
            try
            {
                this.Result = string.Empty;
                if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    //Needle clean pad 종류 3개다 검사하기
                }
                else
                {
                    List<ProberInterfaces.PolishWafer.IPolishWaferIntervalParameter> PolishWaferInterval
                        = this.StageSupervisor().PolishWaferModule().GetPolishWaferIntervalParameters();

                    if (PolishWaferInterval != null)
                    {
                        if (PolishWaferInterval.Count <= 0)
                        {
                            return EventCodeEnum.NONE;
                        }

                        //ProberInterfaces.PolishWafer.IPolishWaferIntervalParameter polish 
                        //    = PolishWaferInterval.FirstOrDefault(interval => interval.CleaningTriggerMode.Value == ProberInterfaces.PolishWafer.EnumCleaningTriggerMode.WAFER_INTERVAL);

                        this.Result = PolishWaferInterval[0].CleaningParameters[0].ContactCount.Value;                      
                    }

                    return EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return EventCodeEnum.NONE;
        }
    }


    [Serializable]
    public class NCOverdrive : QueryData
    {
        public override EventCodeEnum Run()
        {
            try
            {
                this.Result = string.Empty;
                if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    //Needle clean pad 종류 3개다 검사하기
                }
                else
                {
                    List<ProberInterfaces.PolishWafer.IPolishWaferIntervalParameter> PolishWaferInterval
                        = this.StageSupervisor().PolishWaferModule().GetPolishWaferIntervalParameters();

                    if (PolishWaferInterval != null)
                    {
                        if (PolishWaferInterval.Count <= 0)
                        {
                            return EventCodeEnum.NONE;
                        }

                        double od = PolishWaferInterval[0].CleaningParameters[0].OverdriveValue.Value;
                        if(od >= 999)
                        {
                            this.Result = 999;
                        }
                        else
                        {
                            this.Result = od;
                        }
                        
                        // TODO: 현재폴리쉬 웨이퍼의 

                    }

                    return EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class NCContactCounter: QueryData
    {
        
        public override EventCodeEnum Run()
        {
            try
            {
                int? count = (int)this.StageSupervisor().WaferObject.GetSubsInfo().ContactCount;
                if (count != null)
                {
                    if(count >= 9999)
                    {
                        this.Result = 9999;
                    }
                    else
                    {
                        this.Result = count;
                    }
                }
                else
                {
                    this.Result = 0;
                }
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return EventCodeEnum.NONE;
        }
    }


    [Serializable]
    public class NCwaferInterval : QueryData
    {
        public override EventCodeEnum Run()
        {
            try
            {
                this.Result = string.Empty;
                if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    //Needle clean pad 종류 3개다 검사하기
                }
                else
                {
                    List<ProberInterfaces.PolishWafer.IPolishWaferIntervalParameter> PolishWaferInterval
                        = this.StageSupervisor().PolishWaferModule().GetPolishWaferIntervalParameters();

                    if (PolishWaferInterval != null)
                    {
                        if (PolishWaferInterval.Count <= 0)
                        {
                            return EventCodeEnum.NONE;
                        }

                        ProberInterfaces.PolishWafer.IPolishWaferIntervalParameter polish
                            = PolishWaferInterval.FirstOrDefault(interval => interval.CleaningTriggerMode.Value == ProberInterfaces.PolishWafer.EnumCleaningTriggerMode.WAFER_INTERVAL);

                        if (polish != null)
                        {
                            if (polish.IntervalCount.Value >= 99)
                            {
                                this.Result = 99;
                            }
                            else
                            {
                                this.Result = polish.IntervalCount.Value;
                            }

                        }

                    }

                    return EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class NCDieInterval : QueryData
    {
        public override EventCodeEnum Run()
        {
            try
            {
                this.Result = string.Empty;
                if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    //Needle clean pad 종류 3개다 검사하기
                }
                else
                {
                    List<ProberInterfaces.PolishWafer.IPolishWaferIntervalParameter> PolishWaferInterval
                        = this.StageSupervisor().PolishWaferModule().GetPolishWaferIntervalParameters();

                    if (PolishWaferInterval != null)
                    {
                        if (PolishWaferInterval.Count <= 0)
                        {
                            return EventCodeEnum.NONE;
                        }

                        ProberInterfaces.PolishWafer.IPolishWaferIntervalParameter polish
                            = PolishWaferInterval.FirstOrDefault(interval => interval.CleaningTriggerMode.Value == ProberInterfaces.PolishWafer.EnumCleaningTriggerMode.WAFER_INTERVAL);

                        if (polish != null)
                        {
                            int count = polish.IntervalCount.Value * this.GetParam_ProbeCard().ProbeCardDevObjectRef.DutList.ToList().Count();
                            if (count >= 9999)
                            {
                                this.Result = 9999;
                            }
                            else
                            {
                                this.Result = count;
                            }
                        }

                    }

                    return EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class ElementLowerLimit : QueryData
    {
        //public double DefaultLowerlimit { get; set; }
        public override EventCodeEnum Run()
        {
            if (this.Argument != null)
            {
                IElement element = this.ParamManager().GetElement(Convert.ToInt32(this.Argument));

                if (element != null)
                {
                    Result = element.LowerLimit;
                }
            }

            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class ElementUpperLimit : QueryData
    {
        //public double DefaultUpperlimit { get; set; }
        public override EventCodeEnum Run()
        {
            if (this.Argument != null)
            {
                IElement element = this.ParamManager().GetElement(Convert.ToInt32(this.Argument));

                if (element != null)
                {
                    Result = element.UpperLimit;
                }
            }

            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class DummyElementLowerLimit : QueryData
    {
        public double DefaultLowerlimit { get; set; }
        public override EventCodeEnum Run()
        {
            Result = DefaultLowerlimit;

            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public class DummyElementUpperLimit : QueryData
    {
        public double DefaultUpperlimit { get; set; }
        public override EventCodeEnum Run()
        {
            Result = DefaultUpperlimit;

            return EventCodeEnum.NONE;
        }
    }
}
