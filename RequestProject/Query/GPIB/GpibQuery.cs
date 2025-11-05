using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.PinAlign.ProbeCardData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
using LogModule;
using RequestInterface;
using RequestCore.OperationPack;
using NotifyEventModule;
using ProbeCardObject;
using LoaderController.GPController;

namespace RequestCore.QueryPack.GPIB
{
    [Serializable]
    public class Acknowledgement
    {
        public string prefix { get; set; }
        public string data { get; set; }
        public string postfix { get; set; }
    }

    public enum EnumGPIBCommandType
    {
        INTERRUPT = 0,
        ACTION,
        QUERY
    }

    /// <summary>
    /// 없어져도 됨. DeviceParam\GPIB\GpibRequestParam.json Deserialize 시 에러나서 남겨놓음. 
    /// </summary>
    [Serializable]
    public class URValue : QueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                bool IsFound = false;

                string recvData = this.Argument.ToString().ToUpper();

                // TODO : Validation 
                // ur + 0000, Length is 4

                if (recvData.Length == 4)
                {
                    string URId = recvData.Replace("ur", "").Replace("?", "");

                    int id = Convert.ToInt32(URId);

                    URUWConnectorBase dRDWConnectorBase = this.GPIB().GetURUWConnector(id);//이부분만 수정되면됨

                    IElement elment = null;

                    if (dRDWConnectorBase != null)
                    {
                        elment = this.ParamManager().GetElement(dRDWConnectorBase.ID);
                    }

                    if (elment != null)
                    {
                        IsFound = true;
                    }

                    if (IsFound == true)
                    {
                        if (dRDWConnectorBase.ReadValueConverter != null)
                        {
                            dRDWConnectorBase.ReadValueConverter.Argument = elment.GetValue();

                            retVal = dRDWConnectorBase.ReadValueConverter.Run();

                            if (retVal == EventCodeEnum.NONE)
                            {
                                object convertedvalue = dRDWConnectorBase.ReadValueConverter.Result;

                                this.Result = convertedvalue;
                            }
                        }
                        else
                        {
                            this.Result = elment.GetValue().ToString();
                        }

                        retVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        this.Result = string.Empty;
                    }
                }

                if (IsFound == false)
                {
                    this.EventManager().RaisingEvent(typeof(ProberErrorEvent).FullName);
                }
            }
            catch (Exception err)
            {
                this.Result = string.Empty;

                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    /// <summary>
    /// 없어져도 됨. DeviceParam\GPIB\GpibRequestParam.json Deserialize 시 에러나서 남겨놓음. 
    /// </summary>
    [Serializable]
    public class GetURInfo : GpibQueryData
    {
        public GetURInfo()
        {
        }

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.Result = GetResponse();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }



    [Serializable]
    public abstract class GpibQueryData : QueryData
    {
        public EnumGPIBCommandType CommandType { get; set; }

        public Acknowledgement ACK = new Acknowledgement();
        public Acknowledgement NACK = new Acknowledgement();

        public QueryMerge ACKExtensionQueries = new QueryMerge();
        public string GetResponse()
        {
            string retval = string.Empty;

            try
            {
                //Acknowledgement acknowledgement = null;

                //if (Validation() == EventCodeEnum.NONE)
                //{
                //    acknowledgement = ACK;
                //}
                //else
                //{
                //    acknowledgement = NACK;
                //}

                ACKExtensionQueries.Argument = this.Argument;

                string ACKExtensionText = ACKExtensionQueries.GetRequestResult().ToString();

                // TODO : QUERY와 INTERRUPT 로직 분리?
                // QUERY, 정상적인 경우, ACKExtensionText의 값이 존재해야 함.

                string data = string.Empty;

                //if (CommandType == EnumTCPIPCommandType.QUERY)
                //{
                if (!string.IsNullOrEmpty(ACKExtensionText))
                {
                    data = this.Argument.ToString().Replace("?", "");
                    //retval = $"{acknowledgement.prefix}" + $"{ACKExtensionText}" + $"{acknowledgement.postfix}" + $"{ACKExtensionText}";
                    retval = $"{ACKExtensionText}";
                }
                //}
                //else if (CommandType == EnumTCPIPCommandType.INTERRUPT)
                //{
                //    data = acknowledgement.data;
                //}

                //if (!string.IsNullOrEmpty(data))
                //{
                //    retval = $"{acknowledgement.prefix}" + $"{data}" + $"{acknowledgement.postfix}" + $"{ACKExtensionText}";
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        protected string CheckLocation(List<IDut> DutList)
        {
            string retString = null;
            try
            {
                long dmx, dmy;
                retString = "00";

                dmx = DutList[DutList.Count - 1].UserIndex.XIndex;
                dmy = DutList[DutList.Count - 1].UserIndex.YIndex;

                if (DutList.Count == 2)
                {
                    string coordimxmy = dmx + ", " + dmy;

                    switch (coordimxmy)
                    {
                        case "1, -1":
                            retString = "01";
                            break;
                        case "0, -1":
                            retString = "02";
                            break;
                        case "-1, -1":
                            retString = "03";
                            break;
                        case "-1, 0":
                            retString = "04";
                            break;
                        case "-1, 1":
                            retString = "05";
                            break;
                        case "0, 1":
                            retString = "06";
                            break;
                        case "1, 1":
                            retString = "07";
                            break;
                        case "1, 0":
                            retString = "08";
                            break;

                        case "-2, -1":
                            retString = "11";
                            break;
                        case "-2, 1":
                            retString = "12";
                            break;
                        case "2, 1":
                            retString = "13";
                            break;
                        case "2, -1":
                            retString = "14";
                            break;
                        case "1, -2":
                            retString = "15";
                            break;
                        case "-1, -2":
                            retString = "16";
                            break;
                        case "-1, 2":
                            retString = "17";
                            break;
                        case "1, 2":
                            retString = "18";
                            break;
                        default:
                            retString = "00";
                            break;
                    }
                }
                else if (DutList.Count == 4)
                {
                    if (0 < dmx && dmy == 0)
                    {
                        retString = "09";
                    }
                    else if (dmx < 0 && dmy == 0)
                    {
                        retString = "18";
                    }
                    else if (dmx == 0 && 0 < dmy)
                    {
                        retString = "10";
                    }
                    else if (dmx == 0 && dmy < 0)
                    {
                        retString = "16";
                    }
                    else
                    {
                        retString = "00";
                    }
                }
                else if (DutList.Count == 8)
                {
                    if (0 < dmx && dmy == 0)
                    {
                        retString = "17";
                    }
                    else if (dmx < 0 && dmy == 0)
                    {
                        retString = "19";
                    }
                    else if (dmx == 0 && 0 < dmy)
                    {
                        retString = "15";
                    }
                    else if (dmx == 0 && dmy < 0)
                    {
                        retString = "20";
                    }
                    else
                    {
                        retString = "00";
                    }
                }
                else
                {
                    retString = "00";
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retString;
        }
    }

    [Serializable]
    public class GetTelXYCoordinate : GpibQueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                IGPIBSysParam GpibSysParam = this.GPIB().GPIBSysParam_IParam;

                if (GpibSysParam != null)
                {
                    IXYCoordinate coordinate = new TelXYCoordinate();
                    this.Result = coordinate.MakeXYCoordinate();
                }
                else
                {
                    this.Result = "";
                }
            }

            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }
    }


    [Serializable]
    public class GetTskXYCoordinate : GpibQueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                IGPIBSysParam GpibSysParam = this.GPIB().GPIBSysParam_IParam;

                if (GpibSysParam != null)
                {
                    IXYCoordinate coordinate = new TskXYCoordinate();
                    this.Result = coordinate.MakeXYCoordinate();
                }
                else
                {
                    this.Result = "";
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }
    }


    [Serializable]
    public class GetDataUsingDRID : GpibQueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                bool IsFound = false;

                string recvData = this.Argument.ToString().ToUpper();

                string DRId = recvData.Replace("DR", "");

                int id = Convert.ToInt32(DRId);

                IElement elment = null;

                elment = this.ParamManager().GetElement(id);

                if (elment != null)
                {
                    IsFound = true;
                }

                if (IsFound == true)
                {
                    this.Result = elment.GetValue().ToString();
                }
                else
                {
                    this.Result = string.Empty;

                    // STB 76
                    this.EventManager().RaisingEvent(typeof(IncorrectDRNumberEvent).FullName);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    [Serializable]
    public class GetCenterDieCoordinate : GpibQueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

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

    [Serializable]
    public class GetCenterDieOffset : GpibQueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
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

    [Serializable]
    public class GetDieSize : GpibQueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
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

    [Serializable]
    public class GetPassFailCount : GpibQueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

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

    [Serializable]
    public class GetMachineNumber : GpibQueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

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

    [Serializable]
    public class GetProberType : GpibQueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

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

    [Serializable]
    public class GetProberId : GpibQueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

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

    [Serializable]
    public class GetMultipleChannelParam : GpibQueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

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

    [Serializable]
    public class GetLayoutDescription : GpibQueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

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

    //public class GetRefDieCoordinates : GpibQueryData
    //{
    //    public override EventCodeEnum Run()
    //    {
    //        EventCodeEnum retVal = EventCodeEnum.NONE;

    //        RequestBase OrgUIndexX = new RefUIndexX();
    //        RequestBase OrgUIndexY = new RefUIndexY();

    //        QueryMerge queryMerge = new QueryMerge();
    //        queryMerge.Querys = new List<RequestBase>()
    //        {
    //            new FixData(){ResultData = "X"},
    //            OrgUIndexX,
    //            new FixData(){ResultData = "Y"},
    //            OrgUIndexY
    //        };

    //        this.Result = queryMerge.GetRequestResult();

    //        return retVal;
    //    }
    //}

    //public class GetOrgDieCoordinates : GpibQueryData
    //{
    //    public override EventCodeEnum Run()
    //    {
    //        EventCodeEnum retVal = EventCodeEnum.NONE;

    //        RequestBase OrgUIndexX = new OrgUIndexX();
    //        RequestBase OrgUIndexY = new OrgUIndexY();

    //        QueryMerge queryMerge = new QueryMerge();
    //        queryMerge.Querys = new List<RequestBase>()
    //        {
    //            new FixData(){ResultData = "X"},
    //            OrgUIndexX,
    //            new FixData(){ResultData = "Y"},
    //            OrgUIndexY
    //        };

    //        this.Result = queryMerge.GetRequestResult();

    //        return retVal;
    //    }
    //}

    [Serializable]
    public class GetCassetteStatus : GpibQueryData
    {
        public string linkchar = string.Empty;

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {   //TODO: cst 개수 동적으로 만들어야함..
                QueryMerge queryMerge = new QueryMerge();
                QueryHasAffix queryHasAffix = null;
                if (linkchar != string.Empty)
                {
                    queryHasAffix = new QueryHasAffix();
                    queryHasAffix.Data = new GetCassetteInfoFromCstNum { cstNum = 1, length = 26 };
                    queryHasAffix.postfix = linkchar;
                    queryMerge.Querys.Add(queryHasAffix);

                    queryHasAffix = null;
                    queryHasAffix = new QueryHasAffix();
                    queryHasAffix.Data = new GetCassetteInfoFromCstNum { cstNum = 0, length = 26 };
                    queryMerge.Querys.Add(queryHasAffix);

                }
                else
                {
                    if (SystemManager.SysteMode == SystemModeEnum.Single)
                    {
                        // 현재 자기한테 assign되어있는 cst의 상태를 첫번째 string으로 보냄
                        queryHasAffix = new QueryHasAffix();
                        queryHasAffix.Data = new GetCassetteInfoFromCstNum { cstNum = 1, length = 25 };
                        queryHasAffix.prefix = "1";
                        queryMerge.Querys.Add(queryHasAffix);

                        queryHasAffix = null;
                        queryHasAffix = new QueryHasAffix();
                        queryHasAffix.Data = new GetCassetteInfoFromCstNum { cstNum = 0, length = 25 };
                        queryHasAffix.prefix = "2";
                        queryMerge.Querys.Add(queryHasAffix);
                    }
                    else if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                    {
                        GP_LoaderController LoaderController = this.LoaderController() as GP_LoaderController;
                        var cstCount = LoaderController.LoaderInfo?.StateMap?.CassetteModules?.Count() ?? 0;
                        if(cstCount == 0)
                        {
                            LoggerManager.Debug($"GetCassetteStatus(): Clear Casette Status. cstCount is 0.");
                            this.Result = new string('0', 52);
                        }
                        else
                        {
                            for (int i = 1; i <= cstCount; i++)
                            {
                                queryHasAffix = new QueryHasAffix();
                                queryHasAffix.Data = new GetCassetteInfoFromCstNum { cstNum = i, length = 25 };
                                queryHasAffix.prefix = i.ToString();
                                queryMerge.Querys.Add(queryHasAffix);
                            }
                        }
                    }                    
                }

                this.Result = queryMerge.GetRequestResult();

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"GetCassetteStatus(): Clear Casette Status. prev:{this.Result}");
                this.Result = new string('0', 52);// exception 발생 하더라도 기존 포맷 맞춰줘야함. 1 + 25 + 1 + 25
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    [Serializable]
    public class GetTotalTestDieCount : GpibQueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

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

    [Serializable]
    public class GetCassetteMapInfo : GpibQueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

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

    [Serializable]
    public class GetCurrentChuckTemp : GpibQueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                this.Result = this.TempController().TempInfo.CurTemp.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class GetSettingChuckTemp : GpibQueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                this.Result = this.TempController().TempInfo.SetTemp.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class GetLotParameters : GpibQueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

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

    [Serializable]
    public class GetProbeCardName : GpibQueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

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

    [Serializable]
    public class GetErrorCode : GpibQueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

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

    [Serializable]
    public class GetWaferNumber : GpibQueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

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

    [Serializable]
    public class GetProbingOverdrive : GpibQueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

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

    [Serializable]
    public class GetWaferOrientation : GpibQueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

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

    [Serializable]
    public class GetUnitsOfWaferAndDieDimensions : GpibQueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

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

    [Serializable]
    public class GetCurrentWaferSize : GpibQueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

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

    [Serializable]
    public class GetPosDieToDesiredPos : GpibQueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

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

    [Serializable]
    public class GetConvOpusII2TelFlatOriQuery : GpibQueryData
    {
        public RequestBase FlatDir { get; set; }

        public override EventCodeEnum Run()
        {
            int FlatOri = 0;
            int tmpFlatDir = 0;

            int intFlatDir = int.Parse(FlatDir.GetRequestResult().ToString());

            tmpFlatDir = intFlatDir / 90;
            //    0
            //    |
            //7   +   3
            //    |
            //    5

            switch (tmpFlatDir)
            {
                case 0:
                    FlatOri = 3;
                    break;
                case 1:
                    FlatOri = 0;
                    break;
                case 2:
                    FlatOri = 7;
                    break;
                case 3:
                    FlatOri = 5;
                    break;
            }

            this.Result = FlatOri.ToString();

            return EventCodeEnum.NONE;
        }
    }



    [Serializable]
    public class GetLotInfomation : GpibQueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                IGPIBSysParam GpibSysParam = this.GPIB().GPIBSysParam_IParam;

                if (GpibSysParam == null)
                {
                    // ERROR
                }

                QueryMerge queryMerge = new QueryMerge();

                //DeviceName
                QueryFill queryFill = null;

                queryFill = new QueryFill();
                queryFill.Data = new DeviceName();
                queryFill.PaddingData = new FixData() { ResultData = " " };
                queryFill.Length = new FixData() { ResultData = "20" };
                queryMerge.Querys.Add(queryFill);

                //WaferSize
                queryFill = new QueryFill();
                queryFill.Data = new QueryIntToHex() { Data = new WaferSize() };
                queryFill.PaddingData = new FixData() { ResultData = " " };
                queryFill.Length = new FixData() { ResultData = "20" };
                queryMerge.Querys.Add(queryFill);

                queryFill = new QueryFill();
                queryFill.Data = new QueryIntToHex() { Data = new WaferSize() };
                queryFill.PaddingData = new FixData() { ResultData = " " };
                queryFill.Length = new FixData() { ResultData = "20" };
                queryMerge.Querys.Add(queryFill);

                //FlatOrientation                                           
                //1글자, 0 => 3, 9000 => 0, 18000 => 7, 27000 => 5
                queryFill = new QueryFill();
                queryFill.Data = new QueryOperation()
                {
                    Operation = new Multiplication()
                    {
                        Operands = new List<RequestBase>()
                    {
                        new GetConvOpusII2TelFlatOriQuery()
                        {
                            FlatDir = new QueryIntToHex()
                            {
                                Data = new FlatOrientation()
                            }
                        },
                        new FixData(){ ResultData = "100" }
                    }
                    }
                };

                queryFill.PaddingData = new FixData() { ResultData = " " };
                queryFill.Length = new FixData() { ResultData = "20" };
                queryMerge.Querys.Add(queryFill);

                //Die.SizeX
                queryFill = new QueryFill();
                queryFill.Data = new DeviceSizeWidth();
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "5" };
                queryMerge.Querys.Add(queryFill);


                //Die.SizeY
                queryFill = new QueryFill();
                queryFill.Data = new DeviceSizeHeight();
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "5" };
                queryMerge.Querys.Add(queryFill);


                //Die.OverDrive
                queryFill = new QueryFill();
                queryFill.Data = new OverDrive();
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "3" };
                queryMerge.Querys.Add(queryFill);

                //EdgeCorrection
                queryFill = new QueryFill();
                queryFill.Data = new EdgeCorrection();
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "2" };
                queryMerge.Querys.Add(queryFill);

                //ConsecutiveFailMode
                queryFill = new QueryFill();
                queryFill.Data = new ConsecutiveFailMode();
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "1" };
                queryMerge.Querys.Add(queryFill);

                //ConsecutiveFailCount
                //tmpData = string.Format("{0:D2}", ParaBinAnyCountLimit);   //2글자.
                queryFill = new QueryFill();
                queryFill.Data = new FixData() { ResultData = "5" };
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "2" };
                queryMerge.Querys.Add(queryFill);

                //ConsecutiveFailSkipLine
                queryFill = new QueryFill();
                queryFill.Data = new ConsecutiveFailSkipLine();//2글자. //ConsecutiveFailSkipLine parameter. default 0.
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "2" };
                queryMerge.Querys.Add(queryFill);


                queryFill = new QueryFill();
                queryFill.Data = new OrgUIndexX();//ConsecutiveFailSkipLine parameter. default 0.
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "3" };//3글자. //Die.OrgUX
                queryMerge.Querys.Add(queryFill);

                queryFill = new QueryFill();
                queryFill.Data = new OrgUIndexY();//ConsecutiveFailSkipLine parameter. default 0.
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "3" };//3글자. //Die.OrgUY
                queryMerge.Querys.Add(queryFill);

                //DummyZero15
                queryFill = new QueryFill();
                queryFill.Data = new FixData() { ResultData = "" };//15글자. //DummyZero15
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "15" };
                queryMerge.Querys.Add(queryFill);

                //MultiTestYesNo
                queryFill = new QueryFill();
                queryFill.Data = new FixData() { ResultData = "" };//15글자. //DummyZero15
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "15" };
                queryMerge.Querys.Add(queryFill);


                queryFill = new QueryFill();
                queryFill.Data = new GetMultiTestYesNo();
                queryFill.PaddingData = null;
                queryFill.Length = null;
                queryMerge.Querys.Add(queryFill);


                //MultiTestMode
                queryFill = new QueryFill();
                queryFill.Data = new GetMultiTestModeData();
                queryFill.PaddingData = null;
                queryFill.Length = null;
                queryMerge.Querys.Add(queryFill);

                //MultiTestLocData
                queryFill = new QueryFill();
                queryFill.Data = new GetMultiTestLocData();
                queryFill.PaddingData = null;
                queryFill.Length = null;
                queryMerge.Querys.Add(queryFill);

                //MultiTestLocFor2Chan
                queryFill = new QueryFill();
                queryFill.Data = new GetMultiTestLocFor2Chan(); //1글자. MultiTestLocFor2Chan parameter. default is 0.
                queryFill.PaddingData = null;
                queryFill.Length = null;
                queryMerge.Querys.Add(queryFill);

                //DummyZero131
                queryFill = new QueryFill();
                queryFill.Data = new FixData() { ResultData = "" }; //131
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "131" };
                queryMerge.Querys.Add(queryFill);

                this.Result = queryMerge.GetRequestResult();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }
    }

    [Serializable]
    public class GetMultiTestYesNo : GpibQueryData
    {
        public override EventCodeEnum Run()
        {
            //FuncExeResult_Data<string> retData = new FuncExeResult_Data<string>();
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                IGPIBSysParam GpibSysParam = this.GPIB().GPIBSysParam_IParam;

                if (GpibSysParam == null)
                {
                    // ERROR
                }

                // IFuncExeResult FuncExeResult_Data = null;
                List<IDut> DutList = null;
                IEnumerable<IDut> tmpDutList = (new DutList()).GetRequestResult() as IEnumerable<IDut>;
                DutList = new List<IDut>(tmpDutList);

                //MultiTestYesNo
                if (GpibSysParam.UseAutoLocationNum.Value == false) //bUseAutoLocationNum
                {
                    if (this.GPIB().GPIBSysParam_IParam.TelMultiTestYesNo.Value != "NONE" && this.GPIB().GPIBSysParam_IParam.TelMultiTestYesNo.Value.Length == 1)
                    {
                        Result = this.GPIB().GPIBSysParam_IParam.TelMultiTestYesNo.Value;           //1글자. //tmpMultiTestYesNo
                    }
                    else
                    {
                        if (8 < DutList.Count)
                        {
                            if (this.GPIB().GPIBSysParam_IParam.TelGCMDMultiTestYesNoFM.Value == false)
                            {
                                Result = "F";                              //1글자.
                            }
                            else
                            {
                                Result = "M";                              //1글자.
                            }
                        }
                        else if (DutList.Count == 1)
                        {
                            Result = "0";                                  //1글자.
                        }
                        else
                        {
                            if (this.GPIB().GPIBSysParam_IParam.SpecialGCMDMultiDutMode.Value == true)
                            {
                                if (this.GPIB().GPIBSysParam_IParam.TelGCMDMultiTestYesNoFM.Value == false)
                                {
                                    Result = "F";                          //1글자.
                                }
                                else
                                {
                                    Result = "M";                          //1글자.
                                }
                            }
                            else
                            {
                                Result = "F";                              //1글자.
                            }
                        }
                    }
                }
                else
                {
                    string strSTDLoc = null;
                    strSTDLoc = CheckLocation(DutList.ToList());

                    //MultiTestYesNo
                    if (DutList.Count == 1)
                    {
                        Result = "0";
                    }
                    else if (this.GPIB().GPIBSysParam_IParam.TelGCMDMultiTestYesNoFM.Value == false)
                    {
                        Result = "F";
                    }
                    else if (strSTDLoc != "00")
                    {
                        Result = "1";
                    }
                    else
                    {
                        Result = "M";
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


    [Serializable]
    public class GetMultiTestModeData : GpibQueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                IGPIBSysParam GpibSysParam = this.GPIB().GPIBSysParam_IParam;

                if (GpibSysParam == null)
                {
                    // ERROR
                }

                //IFuncExeResult FuncExeResult_Data = null;
                List<IDut> DutList = null;
                IEnumerable<IDut> tmpDutList = (new DutList()).GetRequestResult() as IEnumerable<IDut>;
                DutList = new List<IDut>(tmpDutList);

                if (this.GPIB().GPIBSysParam_IParam.UseAutoLocationNum.Value == false) //bUseAutoLocationNum
                {
                    if (this.GPIB().GPIBSysParam_IParam.TelGCMDForcedOver8MultiDutMode.Value == true)
                    {
                        Result = "0";
                    }
                    else
                    {
                    }
                }

                if (Result == null)
                {
                    if (
                           DutList.Count == 2
                        || DutList.Count == 3
                        || DutList.Count == 4
                        || DutList.Count == 8
                        )
                    {
                        Result = string.Format("{0:D1}", DutList.Count);
                    }
                    else
                    {
                        Result = "0";
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

    [Serializable]
    public class GetMultiTestLocData : GpibQueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                IGPIBSysParam GpibSysParam = this.GPIB().GPIBSysParam_IParam;

                if (GpibSysParam == null)
                {
                    // ERROR
                }

                //IFuncExeResult FuncExeResult_Data = null;
                List<IDut> DutList = null;
                IEnumerable<IDut> tmpDutList = (new DutList()).GetRequestResult() as IEnumerable<IDut>;
                DutList = new List<IDut>(tmpDutList);

                if (this.GPIB().GPIBSysParam_IParam.UseAutoLocationNum.Value == false)
                {
                    if (this.GPIB().GPIBSysParam_IParam.TelGCMDForcedOver8MultiDutMode.Value == true)
                    {
                        Result = "00";
                    }
                    else
                    {
                        if (
                               DutList.Count == 2
                            || DutList.Count == 3
                            || DutList.Count == 4
                            || DutList.Count == 8
                          )
                        {
                            Result = string.Format("{0:D2}", DutList.Count);
                        }
                        else
                        {
                            Result = "00";
                        }
                    }
                }
                else
                {
                    Result = CheckLocation(DutList.ToList());
                    Result = string.Format("{0:D2}", Result);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class GetMultiTestLocFor2Chan : GpibQueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                IGPIBSysParam GpibSysParam = this.GPIB().GPIBSysParam_IParam;

                if (GpibSysParam == null)
                {
                    // ERROR
                }

                // IFuncExeResult FuncExeResult_Data = null;
                List<IDut> DutList = null;
                IEnumerable<IDut> tmpDutList = (new DutList()).GetRequestResult() as IEnumerable<IDut>;
                DutList = new List<IDut>(tmpDutList);

                if (this.GPIB().GPIBSysParam_IParam.UseAutoLocationNum.Value == false)
                {
                    Result = string.Format("{0:D1}", this.GPIB().GPIBSysParam_IParam.MultiTestLocFor2Chan.Value);
                }
                else
                {
                    string strSTDLoc = null;
                    strSTDLoc = CheckLocation(DutList.ToList());

                    if (DutList.Count == 2 && 15 <= strSTDLoc.Length && int.Parse(strSTDLoc) <= 18)
                    {
                        Result = "1";
                    }
                    else
                    {
                        Result = "0";
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


    [Serializable]
    public class GetLoadedWaferId : GpibQueryData
    {
        private string GetOcrId(int CSNum, int sumCstNum)
        {
            string retData = null;

            try
            {
                //IGPIBSysParam GpibSysParam = this.GPIB().GPIBSysParam_IParam;

                //if (GpibSysParam == null)
                //{
                //    // ERROR
                //}

                //if (GpibSysParam.TelSendSlotNumberForSmallNCommand.Value == 0)
                //{
                //    retData = (new OcrID() { Index = sumCstNum + CSNum }).GetRequestResult().ToString();
                //    if (string.IsNullOrEmpty(retData))
                //    {
                //        retData = "OCR_NULL_ID";
                //    }

                //    if (20 < retData.Length)
                //    {
                //        retData = retData.PadRight(20, ' ');
                //    }
                //}
                //else
                //{
                //    retData = CSNum.ToString();
                //}

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retData;
        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                int sumCstNum = 0;
                int CSNum = 0;

                //CSNum = 어디서 가져와야함!!!!!!!!!!!!!!!!!!!!도움!!!!!!!!!!!!!;

                //sumCstNum = ReturnSumCstNum(); //default is 0

                QueryMerge queryMerge = new QueryMerge();
                if (this.LotOPModule().LotInfo.LotName.Value == "NONE")
                {
                    QueryFill queryFill = new QueryFill();
                    queryFill.Data = new WaferOriginID();
                    queryFill.RightPaddingData = new FixData() { ResultData = " " };
                    queryFill.Length = new FixData() { ResultData = "10" };
                    queryMerge.Querys.Add(queryFill);
                }
                else
                {
                    QueryFill queryFill = new QueryFill();
                    queryFill.Data = new OcrID();
                    queryFill.RightPaddingData = new FixData() { ResultData = " " };
                    queryFill.Length = new FixData() { ResultData = "20" };
                    queryMerge.Querys.Add(queryFill);
                }

                var OcrId_old = GetOcrId(CSNum, sumCstNum);
                var OcrId_new = queryMerge.GetRequestResult();

                System.Diagnostics.Debug.WriteLine($"##GetLoadedWaferId old: {OcrId_old}, new: {OcrId_new}");

                Result = queryMerge.GetRequestResult();//GetOcrId(CSNum, sumCstNum);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class GetOnWaferInformation : QueryData
    {
        private bool _ApplyFlipParam = true;

        public bool ApplyFlipParam
        {
            get { return _ApplyFlipParam; }
            set { _ApplyFlipParam = value; }
        }


        public string MakeOnWaferInfo(long mX, long mY)
        {
            string retData = null;
            bool FlipFlag = false;

            try
            {
                var dies = this.StageSupervisor().WaferObject.GetSubsInfo().DIEs;
                
                List<IDut> DutList = null;
                IEnumerable<IDut> tmpDutList = (new DutList()).GetRequestResult() as IEnumerable<IDut>;
                DutList = new List<IDut>(tmpDutList);

                int rowMaxIndex = this.StageSupervisor().WaferObject.GetPhysInfo().MapCountX.Value;
                int colMaxIndex = this.StageSupervisor().WaferObject.GetPhysInfo().MapCountY.Value;
             
                if(this.CoordinateManager().GetReverseManualMoveX() == true && this.CoordinateManager().GetReverseManualMoveY() == true)
                {
                    if (ApplyFlipParam)
                    {
                        FlipFlag = true;
                    }
                }

                int[] o_code_new = null;
                long dmx, dmy;

                int idxCount = DutList.Count / 4 + (DutList.Count % 4 == 0 ? 0 : 1);

                LoggerManager.Debug($"MakeOnWaferInfo(): dut count({DutList.Count}), result count({idxCount})");

                o_code_new = new int[idxCount];

                for (int i = 0; i < idxCount; i++)
                {
                    o_code_new[i] = 0x40;
                }

                int numx = dies.GetUpperBound(0);
                int numy = dies.GetUpperBound(1);
                
                for (int i = 0; i < DutList.Count; i++)
                {
                    // @ 40, 100 0000
                    // O 4F, 100 1111
                    // C 43, 100 0011

                    dmx = mX + DutList[i].UserIndex.XIndex;
                    dmy = mY + DutList[i].UserIndex.YIndex;

                    bool isDutIsInRange = this.ProbingModule().DutIsInRange(dmx, dmy);                                                                                                          
                                                                                                                                                                                                
                                                                                                                                                                                                
                    //if (numx >= dmx & numy >= dmy)
                    if (isDutIsInRange)
                    {
                        if (FlipFlag)
                        {
                            if (dies[rowMaxIndex - dmx - 1, colMaxIndex - dmy - 1].DieType.Value == DieTypeEnum.TEST_DIE)
                            {
                                int oCodeIdx = i / 4;
                                o_code_new[oCodeIdx] = o_code_new[oCodeIdx] | (1 << (i % 4));
                            }
                            else
                            {
                                LoggerManager.Debug($"MakeOnWaferInfo(): [{rowMaxIndex - dmx - 1},{colMaxIndex - dmy - 1}] :{dies[rowMaxIndex - dmx - 1, colMaxIndex - dmy - 1].DieType.Value}");
                            }
                        }
                        else
                        {
                            if (dies[dmx, dmy].DieType.Value == DieTypeEnum.TEST_DIE)
                            {
                                int oCodeIdx = i / 4;
                                o_code_new[oCodeIdx] = o_code_new[oCodeIdx] | (1 << (i % 4));
                            }
                            else
                            {
                                LoggerManager.Debug($"MakeOnWaferInfo(): [{dmx},{dmy}] :{dies[dmx, dmy].DieType.Value}");
                            }
                        }
                        
                    }
                    else
                    {

                    }
                }

                for (int i = 0; i < idxCount; i++)
                {
                    retData += Encoding.ASCII.GetString(new byte[] { (byte)o_code_new[i] });
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retData;
        }

        private int ReturnSumCstNum()
        {
            int sumCstNum = 0;
            try
            {

                //Todo : dualfoup이 있을때 해당.

                //if (CSTSourceNumForManual)
                //{
                //    sumCstNum = 25;
                //}
                //else
                //{
                //    sumCstNum = 0;
                //}

                //if ((dualfouptype == true && flag_exchange_dual == true))
                //{
                //    sumCstNum -= 25;
                //    sumCstNum = Math.Abs(sumCstNum);
                //}

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return sumCstNum;
        }

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                int mxValue = 0;
                int myValue = 0;

                mxValue = int.Parse((new ProbingMIndexX()).GetRequestResult().ToString());
                myValue = int.Parse((new ProbingMIndexY()).GetRequestResult().ToString());

                if (this.ProbingModule().ProbingStateEnum == EnumProbingState.DONE)
                {
                    //error 안돼!! 프로빙이 아니고 끝난다음에는 가져올 수 없어!!
                    Result = null;
                }
                else
                {
                    Result = MakeOnWaferInfo(mxValue, myValue);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class GetOnTestDutInformation : QueryData
    {
        public string MakeOnWaferDutInfo(long mX, long mY)
        {
            string retData = null;

            try
            {

                List<int> dutlist = getDutInfo(new MachineIndex(mX, mY));
                List<IDeviceObject> DieList = this.GetParam_Wafer().GetDevices();
                List<IDut> DutList = null;
                IEnumerable<IDut> tmpDutList = (new DutList()).GetRequestResult() as IEnumerable<IDut>;
                DutList = new List<IDut>(tmpDutList);
                int idxCount = DutList.Count; //2048
                int o_code_count = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(dutlist.Count) / 4));
                int[] o_code_new = new int[o_code_count];
                int codeCount = 0;
                int o_code_index = 0;
                for (int i = 0; i < o_code_count; i++)
                {
                    o_code_new[i] = 0x40;
                }

                for (int i = 0; i < DutList.Count; i++)
                {
                    if (codeCount == 4 || o_code_index > o_code_count)
                    {
                        codeCount = 0;
                        o_code_index += 1;
                    }

                    var dutObj = dutlist.Find(dut => dut == DutList[i].DutNumber);
                    if (dutObj == DutList[i].DutNumber)
                    {
                        var deviceObj = DieList.Find(die => die.DutNumber == dutObj);
                        if (deviceObj != null)
                        {
                            if (deviceObj.DieType.Value == DieTypeEnum.TEST_DIE)
                            {
                                int oCodeIdx = i / 4;
                                o_code_new[o_code_index] = o_code_new[oCodeIdx] | (1 << (i % 4));
                            }
                        }
                    }

                    codeCount += 1;
                }

                for (int i = 0; i < o_code_count; i++)
                {
                    retData += Encoding.ASCII.GetString(new byte[] { (byte)o_code_new[i] });
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retData;
        }

        private List<int> getDutInfo(MachineIndex machineIndex)
        {
            List<int> dutlist = new List<int>();
            try
            {
                var cardinfo = this.GetParam_ProbeCard();
                List<IDeviceObject> dev = new List<IDeviceObject>();
                if ((cardinfo != null) && (cardinfo.ProbeCardDevObjectRef.DutList.Count > 0))
                {
                    for (int dutIndex = 0; dutIndex < cardinfo.ProbeCardDevObjectRef.DutList.Count; dutIndex++)
                    {
                        IndexCoord retindex = machineIndex.Add(cardinfo.GetRefOffset(dutIndex));
                        IDeviceObject devobj = this.GetParam_Wafer().GetDevices().Find(
                            x => x.DieIndexM.Equals(retindex));
                        if (devobj != null)
                        {
                            dev.Add(devobj);
                            dev[dev.Count() - 1].DutNumber = cardinfo.ProbeCardDevObjectRef.DutList[dutIndex].DutNumber;
                        }
                        else
                        {
                            devobj = new DeviceObject();
                            devobj.DieIndexM.XIndex = retindex.XIndex;
                            devobj.DieIndexM.YIndex = retindex.YIndex;
                            dev.Add(devobj);
                            dev[dev.Count() - 1].DutNumber = cardinfo.ProbeCardDevObjectRef.DutList[dutIndex].DutNumber;

                        }
                    }

                    ObservableCollection<IDeviceObject> dutdevs = new ObservableCollection<IDeviceObject>();
                    if (dev.Count() > 0)
                    {

                        for (int devIndex = 0; devIndex < dev.Count; devIndex++)
                        {
                            if (dev[devIndex] != null)
                                dutlist.Add(dev[devIndex].DutNumber);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return dutlist;
        }

        private int ReturnSumCstNum()
        {
            int sumCstNum = 0;
            try
            {

                //Todo : dualfoup이 있을때 해당.

                //if (CSTSourceNumForManual)
                //{
                //    sumCstNum = 25;
                //}
                //else
                //{
                //    sumCstNum = 0;
                //}

                //if ((dualfouptype == true && flag_exchange_dual == true))
                //{
                //    sumCstNum -= 25;
                //    sumCstNum = Math.Abs(sumCstNum);
                //}

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return sumCstNum;
        }

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                int mxValue = 0;
                int myValue = 0;

                mxValue = int.Parse((new ProbingMIndexX()).GetRequestResult().ToString());
                myValue = int.Parse((new ProbingMIndexY()).GetRequestResult().ToString());

                if (this.ProbingModule().ProbingStateEnum == EnumProbingState.DONE)
                {
                    //error 안돼!! 프로빙이 아니고 끝난다음에는 가져올 수 없어!!
                    Result = null;
                }
                else
                {
                    Result = MakeOnWaferDutInfo(mxValue, myValue);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class GetTestSampleInform : QueryData
    {
        private static int? SampleInfoHashCode = null;
        private static string saveDirectory = @"C:\ProberSystem";
        //private static string saveFullPath = @"C:\ProberSystem\GpibTest.txt";

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                string recvData = this.Argument.ToString();
                int tmpHashCode = recvData.GetHashCode();

                Console.Write("Recive.");

                if (SampleInfoHashCode != null)
                {
                    if (SampleInfoHashCode != tmpHashCode)
                    {
                        if (!Directory.Exists(saveDirectory))
                            Directory.CreateDirectory(saveDirectory);

                        //using (StreamWriter writer = new StreamWriter(saveFullPath, true))
                        //{
                        //    string logStr = $"{DateTime.Now.ToString()} : preHashCode = {SampleInfoHashCode}, recvHashCode = {tmpHashCode}({recvData})";
                        //    Console.ForegroundColor = ConsoleColor.Red;
                        //    Console.WriteLine();
                        //    Console.WriteLine();
                        //    Console.WriteLine(logStr);
                        //    Console.WriteLine();
                        //    Console.WriteLine();
                        //    writer.WriteLine(logStr);
                        //    Console.ForegroundColor = ConsoleColor.White;
                        //}
                    }
                }

                SampleInfoHashCode = tmpHashCode;

                this.Result = recvData;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class GetLotId : GpibQueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            string lotid = this.LotOPModule().LotInfo.LotName.Value;

            if (string.IsNullOrEmpty(lotid)) { lotid = string.Empty; }

            while (lotid.Length < 16)
            {
                lotid += " ";
            }
            Result = lotid;

            return retVal;
        }
    }

    [Serializable]
    public class TSK_UploadDevice : QueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                QueryMerge queryMerge = new QueryMerge();

                //-- WaferName
                QueryFill queryFill = null;
                queryFill = new QueryFill();
                queryFill.Data = new WaferID();
                queryFill.PaddingData = new FixData() { ResultData = " " };
                queryFill.Length = new FixData() { ResultData = "12" };
                queryMerge.Querys.Add(queryFill);

                //-- WaferSizeInch 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.Data = new WaferSizeInch();
                queryFill.Length = new FixData() { ResultData = "1" };
                queryMerge.Querys.Add(queryFill);

                //-- FlatOrientation 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.Data = new FlatOrientation();
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "3" };
                queryMerge.Querys.Add(queryFill);

                //-- DeviceSizeWidth 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.Data = new DeviceSizeWidth();
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "5" };
                queryMerge.Querys.Add(queryFill);

                //-- DeviceSizeHeight 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.Data = new DeviceSizeHeight();
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "5" };
                queryMerge.Querys.Add(queryFill);

                //-- Fixed0
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.PaddingData = new FixData() { ResultData = "000000000000000000XN11" };
                queryFill.Length = new FixData() { ResultData = "22" };
                queryMerge.Querys.Add(queryFill);

                //-- WaferRadius 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.Data = new WaferRadius100um();
                queryFill.Length = new FixData() { ResultData = "4" };
                queryMerge.Querys.Add(queryFill);

                //-- TargetSense 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "1" };
                queryMerge.Querys.Add(queryFill);

                //-- Fixed1 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.PaddingData = new FixData() { ResultData = "000000000000" };
                queryFill.Length = new FixData() { ResultData = "12" };
                queryMerge.Querys.Add(queryFill);

                //-- StdChipX 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.PaddingData = new FixData() { ResultData = "+00" };
                queryFill.Length = new FixData() { ResultData = "3" };
                queryMerge.Querys.Add(queryFill);

                //-- StdChipY 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.PaddingData = new FixData() { ResultData = "+00" };
                queryFill.Length = new FixData() { ResultData = "3" };
                queryMerge.Querys.Add(queryFill);

                //-- Fixed2 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.PaddingData = new FixData() { ResultData = "N000000N00" };
                queryFill.Length = new FixData() { ResultData = "12" };
                queryMerge.Querys.Add(queryFill);

                //-- ProbeAreaSelect 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.PaddingData = new FixData() { ResultData = "Y" };
                queryFill.Length = new FixData() { ResultData = "1" };
                queryMerge.Querys.Add(queryFill);



                //.AlignAxis = "X"            'bytes:1
                //.AutoFocus = "1"            'bytes:1
                //.AlignSizeX = "9"           'bytes:1
                //.AlignSizeY = "9"           'bytes:1
                //.MonitorChipX = "9"         'bytes:1
                //.MonitorChipY = "9"         'bytes:1
                //.Fixed4 = "00000000"        'bytes:8  'fixed string




                //-- EdgeCorrect 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.PaddingData = new FixData() { ResultData = "100" };
                queryFill.Length = new FixData() { ResultData = "3" };
                queryMerge.Querys.Add(queryFill);


                //-- SampleProbe 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "1" };
                queryMerge.Querys.Add(queryFill);

                //-- SampleFixed 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.PaddingData = new FixData() { ResultData = "+00+00+00+00+00+00+00+00+00+00+00+00+00+00+00+00" };
                queryFill.Length = new FixData() { ResultData = "48" };
                queryMerge.Querys.Add(queryFill);

                //-- PassNet 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.PaddingData = new FixData() { ResultData = "65535" };
                queryFill.Length = new FixData() { ResultData = "5" };
                queryMerge.Querys.Add(queryFill);

                //-- Fixed3 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.PaddingData = new FixData() { ResultData = "000" };
                queryFill.Length = new FixData() { ResultData = "3" };
                queryMerge.Querys.Add(queryFill);

                //-- AlignAxis 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.PaddingData = new FixData() { ResultData = "X" };
                queryFill.Length = new FixData() { ResultData = "1" };
                queryMerge.Querys.Add(queryFill);

                //-- AutoFocus 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.PaddingData = new FixData() { ResultData = "1" };
                queryFill.Length = new FixData() { ResultData = "1" };
                queryMerge.Querys.Add(queryFill);

                //-- AlignSizeX 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.PaddingData = new FixData() { ResultData = "9" };
                queryFill.Length = new FixData() { ResultData = "1" };
                queryMerge.Querys.Add(queryFill);

                //-- AlignSizeY
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.PaddingData = new FixData() { ResultData = "9" };
                queryFill.Length = new FixData() { ResultData = "1" };
                queryMerge.Querys.Add(queryFill);

                //-- MonitorChipX 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.PaddingData = new FixData() { ResultData = "9" };
                queryFill.Length = new FixData() { ResultData = "1" };
                queryMerge.Querys.Add(queryFill);

                //-- MonitorChipY
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.PaddingData = new FixData() { ResultData = "9" };
                queryFill.Length = new FixData() { ResultData = "1" };
                queryMerge.Querys.Add(queryFill);

                //-- Fixed4 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.PaddingData = new FixData() { ResultData = "00000000" };
                queryFill.Length = new FixData() { ResultData = "8" };
                queryMerge.Querys.Add(queryFill);

                //-- MultiChip 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.Data = new MultiChip();
                queryFill.PaddingData = new FixData() { ResultData = "0" }; // 디폴트가 0이어도 될까..?
                queryFill.Length = new FixData() { ResultData = "1" };
                queryMerge.Querys.Add(queryFill);

                //-- MultiLocation 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.Data = new TSK_MultiSiteLocation();
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "2" };
                queryMerge.Querys.Add(queryFill);

                //-- UnloadStop 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.PaddingData = new FixData() { ResultData = "N" };
                queryFill.Length = new FixData() { ResultData = "1" };
                queryMerge.Querys.Add(queryFill);

                //-- StopCounter 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "2" };
                queryMerge.Querys.Add(queryFill);

                //-- TestRejectCst 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.PaddingData = new FixData() { ResultData = "0" }; //"0":LOAD CST, 1:REJECT CST....?  --- Fixed
                queryFill.Length = new FixData() { ResultData = "1" };
                queryMerge.Querys.Add(queryFill);

                //-- AlignRejectCst 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.PaddingData = new FixData() { ResultData = "0" }; //"0":LOAD CST, 1:REJECT CST....? --- Fixed
                queryFill.Length = new FixData() { ResultData = "1" };
                queryMerge.Querys.Add(queryFill);

                //-- ContFailProcess1 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "1" };
                queryMerge.Querys.Add(queryFill);

                //-- ContFailProcess2 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.PaddingData = new FixData() { ResultData = "0" }; //STOP
                queryFill.Length = new FixData() { ResultData = "1" };
                queryMerge.Querys.Add(queryFill);

                //-- ContFailLimit 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.PaddingData = new FixData() { ResultData = "002" };// TODO: 몇으로 가져올지,YMTC 구조 참고
                queryFill.Length = new FixData() { ResultData = "3" };
                queryMerge.Querys.Add(queryFill);

                //-- SkipChipRow 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "2" };
                queryMerge.Querys.Add(queryFill);

                //-- CheckBackCount 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "1" };
                queryMerge.Querys.Add(queryFill);

                //-- RejectWaferCount 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "1" };
                queryMerge.Querys.Add(queryFill);

                //-- PolishAfterChkBack 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.PaddingData = new FixData() { ResultData = "N" };
                queryFill.Length = new FixData() { ResultData = "1" };
                queryMerge.Querys.Add(queryFill);

                //-- NC_Enable 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.Data = new NCEnable(); // PolishWaferInterval.Count
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "1" };
                queryMerge.Querys.Add(queryFill);

                //-- NC_ZCount 
                queryFill = null;
                queryFill = new QueryFill();// (interval[0] CleaningParameters[0].ContactCount) 
                queryFill.Data = new NCContactCount();
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "2" };
                queryMerge.Querys.Add(queryFill);

                //-- NC_ChipCount 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.Data = new NCDieInterval(); // WAFER_INTERVAL (IntervalCount * DutList.Count)
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "4" };
                queryMerge.Querys.Add(queryFill);

                //-- NC_WaferCount 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.Data = new NCwaferInterval();// WAFER_INTERVAL (IntervalCount) 
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "2" };
                queryMerge.Querys.Add(queryFill);

                //-- NC_Overdrive 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.Data = new NCOverdrive();// 누구의...? interval[0], parameter[0]
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "3" };
                queryMerge.Querys.Add(queryFill);

                //-- NC_TotalCount 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.Data = new NCContactCounter();// 현재 몇번 NC conatct 했는지 
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "4" };
                queryMerge.Querys.Add(queryFill);

                //-- HotChuckTemp 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.Data = new QueryFloor { Data = new SetTemp() };
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "3" };
                queryMerge.Querys.Add(queryFill);

                //-- PresetAddrX 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "2" };
                queryMerge.Querys.Add(queryFill);

                //-- PresetAddrY 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "2" };
                queryMerge.Querys.Add(queryFill);

                this.Result = queryMerge.GetRequestResult();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class MultiSiteLocation : GpibQueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                List<IDut> DutList = null;
                IEnumerable<IDut> tmpDutList = (new DutList()).GetRequestResult() as IEnumerable<IDut>;
                DutList = new List<IDut>(tmpDutList);
                Result = CheckLocation(DutList.ToList());

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }


    [Serializable]
    public class TSK_MultiSiteLocation : GpibQueryData // 없어져야함.
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                List<IDut> DutList = null;
                IEnumerable<IDut> tmpDutList = (new DutList()).GetRequestResult() as IEnumerable<IDut>;
                DutList = new List<IDut>(tmpDutList);
                Result = CheckLocation(DutList.ToList());

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class TSK_ChuckTemperature : QueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                double curTemp = (double)(new CurTemp()).GetRequestResult() * 10;
                double setTemp = (double)(new SetTemp()).GetRequestResult() * 10;
                this.Result = curTemp.ToString("0000") + setTemp.ToString("0000");

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class TSK_SettingTemperature : QueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                double value = (double)(new SetTemp()).GetRequestResult();

                if (value > 0)
                {
                    //this.Result = string.Format("{000.0}", value);
                    this.Result = $"{value:000.0}";
                }
                else if (value < 0 && value > -100)
                {
                    //this.Result = string.Format("-000.0", value);
                    this.Result = $"{value:000.0}";
                }
                else if (value <= -100)  // Should be 6 bytes for negative temp.
                {
                    //this.Result = string.Format("###.0", value);
                    this.Result = $"{value:000.0}";
                }
                else
                {
                    //this.Result = string.Format("000.0", value);
                    this.Result = $"{value:000.0}";
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class TSK_ProberID : QueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                //ProberID
                QueryMerge queryMerge = new QueryMerge();

                //DeviceName
                QueryFill queryFill = null;

                queryFill = new QueryFill();
                queryFill.Data = (new ProberID());
                queryMerge.Querys.Add(queryFill);

                this.Result = queryMerge.GetRequestResult();


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class TSK_WaferParameter : QueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {

                QueryMerge queryMerge = new QueryMerge();
                QueryFill queryFill = null;

                // wafer name
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.Data = new GetLoadedWaferId();
                queryFill.Length = new FixData() { ResultData = "20" };
                queryFill.RightPaddingData = new FixData() { ResultData = " " };
                queryMerge.Querys.Add(queryFill);

                // wafer size
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.Data = new WaferSizeInt();
                queryFill.Length = new FixData() { ResultData = "1" };
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryMerge.Querys.Add(queryFill);

                // flat orientation
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.Data = new GetConvOpusII2TelFlatOriQuery() { FlatDir = new FlatOrientation() };
                queryFill.Length = new FixData() { ResultData = "1" };
                queryFill.PaddingData = new FixData() { ResultData = " " };
                queryMerge.Querys.Add(queryFill);


                // die size x 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.Data = new DeviceSizeWidth();
                queryFill.Length = new FixData() { ResultData = "5" };
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryMerge.Querys.Add(queryFill);

                // die size y
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.Data = new DeviceSizeHeight();
                queryFill.Length = new FixData() { ResultData = "5" };
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryMerge.Querys.Add(queryFill);

                // overdrive
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.Data = new OverDrive();
                queryFill.Length = new FixData() { ResultData = "3" };
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryMerge.Querys.Add(queryFill);

                // edge correction --> 무슨 값인지 모름
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.Data = new EdgeCorrection();
                queryFill.Length = new FixData() { ResultData = "2" };
                queryFill.PaddingData = new FixData() { ResultData = "50" };
                queryMerge.Querys.Add(queryFill);

                // ContFailMode
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.Length = new ConsecutiveFailMode();
                queryFill.PaddingData = new FixData() { ResultData = "1" };
                queryMerge.Querys.Add(queryFill);

                // ContFailCount
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.Length = new FixData() { ResultData = "2" };
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryMerge.Querys.Add(queryFill);


                // ContFailSkip
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.Data = new ConsecutiveFailSkipLine();
                queryFill.Length = new FixData() { ResultData = "1" };
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryMerge.Querys.Add(queryFill);


                // PresetAddressX
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.Data = new OrgUIndexX();
                queryFill.Length = new FixData() { ResultData = "3" };
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryMerge.Querys.Add(queryFill);

                // PresetAddressY
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.Data = new OrgUIndexY();
                queryFill.Length = new FixData() { ResultData = "3" };
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryMerge.Querys.Add(queryFill);

                // Dummy0
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.Length = new FixData() { ResultData = "15" };
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryMerge.Querys.Add(queryFill);

                // MultiTesting Enable
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.Data = new GetMultiTestYesNo();
                queryFill.Length = new FixData() { ResultData = "1" };
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryMerge.Querys.Add(queryFill);

                // MultiTesting Mode 
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.Data = new GetMultiTestModeData();
                queryFill.Length = new FixData() { ResultData = "1" };
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryMerge.Querys.Add(queryFill);

                // MultiTesting Location
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.Data = new GetMultiTestLocData();
                queryFill.Length = new FixData() { ResultData = "2" };
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryMerge.Querys.Add(queryFill);



                // MultiTesting Location for 2 channel MultipleProbing
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.Data = new GetMultiTestLocFor2Chan();
                queryFill.Length = new FixData() { ResultData = "1" };
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryMerge.Querys.Add(queryFill);


                // Dummy0
                queryFill = null;
                queryFill = new QueryFill();
                queryFill.Length = new FixData() { ResultData = "131" };
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryMerge.Querys.Add(queryFill);

                this.Result = queryMerge.GetRequestResult();


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class TSK_UR : QueryData
    {
        public override EventCodeEnum Run()
        {
            try
            {
                QueryFill queryFill = null;
                string arg = this.Argument.ToString().Trim();
                string command = arg.ToString().Substring(arg.Length - 4);
                switch (command)
                {
                    case "0033":
                        QueryHasAffix queryHasAffix = new QueryHasAffix();
                        queryHasAffix.Data = new SetTemp();
                        queryHasAffix.postfix = "deg";
                        this.Result = queryHasAffix.GetRequestResult().ToString();// 자릿수는 상관없다곤 하지만... 어떻게 나올지 모르겠군..
                        break;
                    case "0137":
                        var pinalignparam = this.PinAligner().GetPinAlignerIParam() as PinAlignDevParameters;
                        var sourceInfo = pinalignparam.PinAlignSettignParam.Where(setup => setup.SourceName.Value == PINALIGNSOURCE.WAFER_INTERVAL) as PinAlignSettignParameter;
                        if (sourceInfo != null)
                        {
                            this.Result = sourceInfo.SamplePinAlignmentPinCount.Value;
                        }
                        else
                        {
                            this.Result = pinalignparam.PinAlignSettignParam.Count;//default
                        }
                        break;
                    case "0204":
                        queryFill = new QueryFill();
                        queryFill.Data = new OverDrive();
                        queryFill.Length = new FixData() { ResultData = "3" };
                        queryFill.PaddingData = new FixData() { ResultData = "0" };
                        this.Result = queryFill.GetRequestResult().ToString() + "um";
                        break;
                    case "0281":
                        queryFill = new QueryFill();
                        queryFill.Data = new QueryBoolToInt { Data = new PMIEnable() };
                        queryFill.PaddingData = new FixData() { ResultData = "0" };
                        queryFill.Length = new FixData() { ResultData = "1" };
                        break;
                    case "1224":
                        MapHorDirectionEnum? hordir = this.StageSupervisor().WaferObject.GetPhysInfo().MapDirX.Value;
                        if (hordir != null)
                        {
                            switch (hordir)
                            {
                                case MapHorDirectionEnum.RIGHT:
                                    this.Result = 0;
                                    break;
                                case MapHorDirectionEnum.LEFT:
                                    this.Result = 1;
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case "1225":
                        MapVertDirectionEnum? verdir = this.StageSupervisor().WaferObject.GetPhysInfo().MapDirY.Value;
                        if (verdir != null)
                        {
                            switch (verdir)
                            {
                                case MapVertDirectionEnum.UP:
                                    this.Result = 0;
                                    break;
                                case MapVertDirectionEnum.DOWN:
                                    this.Result = 1;
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;

                    case "1227":
                        //QueryHasAffix queryHasAffix = new QueryHasAffix();
                        //queryHasAffix.Data = new QueryDownUnit() { Data = new SoakingTimeInSeconds(), Argument = 60 };
                        //queryHasAffix.DataFormat = "{0,2:00}";//int
                        //queryHasAffix.postfix = "min";
                        //this.Result = (queryHasAffix.GetRequestResult()).ToString();
                        this.Result = ((int)(this.StageSupervisor().SoakingModule().GetSoakingTime() / 60)).ToString() + "min";
                        break;
                    default:
                        LoggerManager.Debug($"TSK_UR: Cannot find ur{this.Argument} response.");
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
    public class TSK_WaferNumber : QueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {

                QueryMerge queryMerge = new QueryMerge();

                QueryFill queryFill = null;

                queryFill = new QueryFill();
                queryFill.Data = new CurrentSlotNumber();
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "2" };
                queryMerge.Querys.Add(queryFill);

                queryFill = new QueryFill();
                queryFill.Data = new CassetteNum();
                queryFill.PaddingData = new FixData() { ResultData = "6" };
                queryFill.Length = new FixData() { ResultData = "1" };
                queryMerge.Querys.Add(queryFill);


                //For a wafer loaded from the inspection tray or front side, no wafer number is assigned and so
                //the wafer number becomes "* *" and the cassette number is 5.
                //2) For a wafer loaded from the special tray, no wafer number is assigned and so the wafer number
                //becomes "* *" and the cassette number is 6.
                this.Result = queryMerge.GetRequestResult();

                LoggerManager.Debug($"TODO_X");

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class TSK_DeviceInfoReq : QueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                QueryMerge queryMerge = new QueryMerge();
                QueryFill queryFill = null;
                QueryHasAffix queryHasAffix = null;

                //command 연결해야함
                string command = this.Argument.ToString().Substring(this.Argument.ToString().Length - 2);
                switch (command)
                {
                    case "00":
                        queryFill = new QueryFill();
                        queryFill.Data = new WaferID();
                        queryFill.Length = new FixData() { ResultData = "19" };
                        queryFill.PaddingData = new FixData() { ResultData = " " };
                        queryMerge.Querys.Add(queryFill);
                        break;
                    case "01":
                        queryHasAffix = new QueryHasAffix();
                        queryHasAffix.Data = new QueryDownUnit() { Data = new WaferSize(), Argument = 1000 };
                        queryHasAffix.DataFormat = "{0,3:000}";
                        queryHasAffix.postfix = "m";
                        queryMerge.Querys.Add(queryHasAffix);
                        break;
                    case "20":
                        queryFill = new QueryFill();
                        queryFill.Data = new CardID();// ocr?
                        queryFill.PaddingData = new FixData() { ResultData = " " };
                        queryFill.Length = new FixData() { ResultData = "5" };
                        queryMerge.Querys.Add(queryFill);
                        break;
                    case "22":
                        queryHasAffix = new QueryHasAffix();
                        queryHasAffix.Data = new QueryNumToAscii() { Data = new WaferThickness() };
                        queryHasAffix.DataFormat = "{0,4:0000}";
                        queryMerge.Querys.Add(queryHasAffix);
                        break;
                    case "24":
                        queryHasAffix = new QueryHasAffix();
                        queryHasAffix.Data = new QueryNumToAscii() { Data = new OverDrive() };
                        queryHasAffix.DataFormat = "{0,5:00000}";
                        queryMerge.Querys.Add(queryHasAffix);
                        break;
                    default:
                        break;
                }

                this.Result = queryMerge.GetRequestResult();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class TSK_LotNumber : QueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {

                QueryMerge queryMerge = new QueryMerge();

                QueryFill queryFill = null;

                queryFill = new QueryFill();
                queryFill.Data = (new OcrID());
                queryFill.RightPaddingData = new FixData() { ResultData = " " };
                queryFill.Length = new FixData() { ResultData = "16" };
                queryMerge.Querys.Add(queryFill);

                this.Result = queryMerge.GetRequestResult();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class TSK_CassetStatus : QueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                QueryMerge queryMerge = new QueryMerge();

                QueryFill queryFill = null;

                queryFill = new QueryFill();
                queryFill.Data = (new CurrentSlotNumber());
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "2" };
                queryMerge.Querys.Add(queryFill);

                queryFill = new QueryFill();
                queryFill.Data = (new LeftWaferCount());
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "2" };
                queryMerge.Querys.Add(queryFill);

                queryFill = new QueryFill();
                queryFill.Data = (new CassetteNum());
                queryFill.PaddingData = new FixData() { ResultData = "0" };
                queryFill.Length = new FixData() { ResultData = "2" };
                queryMerge.Querys.Add(queryFill);


                this.Result = queryMerge.GetRequestResult();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }


}
