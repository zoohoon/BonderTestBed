using LoaderParameters.Data;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using RequestCore.QueryPack;
using SerializerUtil;
using System;
using System.Collections.Generic;
using System.Text;
using ProberInterfaces.Foup;
using ProberInterfaces.PinAlign.ProbeCardData;
using NotifyEventModule;

namespace RequestCore.Query.TCPIP
{
    public enum EnumAcknowledgementType
    {
        ACK = 0,
        NACK
    }

    public enum EnumTCPIPCommandType
    {
        INTERRUPT = 0,
        ACTION,
        QUERY
    }

    [Serializable]
    public class Acknowledgement
    {
        public string prefix { get; set; }
        public string data { get; set; }
        public string postfix { get; set; }
    }

    [Serializable]
    public abstract class TCPIPQueryData : QueryData
    {
        public EnumTCPIPCommandType CommandType { get; set; }

        public Acknowledgement ACK = new Acknowledgement();
        public Acknowledgement NACK = new Acknowledgement();

        public QueryMerge ACKExtensionQueries = new QueryMerge();

        public virtual EventCodeEnum Validation()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public string GetResponse()
        {
            string retval = string.Empty;

            try
            {
                Acknowledgement acknowledgement = null;

                if (Validation() == EventCodeEnum.NONE)
                {
                    acknowledgement = ACK;
                }
                else
                {
                    acknowledgement = NACK;
                }

                ACKExtensionQueries.Argument = this.Argument;

                string ACKExtensionText = ACKExtensionQueries.GetRequestResult().ToString();

                // TODO : QUERY와 INTERRUPT 로직 분리?
                // QUERY, 정상적인 경우, ACKExtensionText의 값이 존재해야 함.

                string data = string.Empty;

                if (CommandType == EnumTCPIPCommandType.QUERY)
                {
                    if (!string.IsNullOrEmpty(ACKExtensionText))
                    {
                        data = this.Argument.ToString().Replace("?", "");
                        retval = $"{acknowledgement.prefix}" + $"{data}" + $"{acknowledgement.postfix}" + $"{ACKExtensionText}";
                    }
                }
                else if (CommandType == EnumTCPIPCommandType.INTERRUPT)
                {
                    data = acknowledgement.data;
                }

                if (!string.IsNullOrEmpty(data))
                {
                    retval = $"{acknowledgement.prefix}" + $"{data}" + $"{acknowledgement.postfix}" + $"{ACKExtensionText}";
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    [Serializable]
    public class GetCurTemp : TCPIPQueryData
    {
        public GetCurTemp()
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
    public class GetProberID : TCPIPQueryData
    {
        public GetProberID()
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
    public class GetProbeCardID : TCPIPQueryData
    {
        public GetProbeCardID()
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
    public class GetLotName : TCPIPQueryData
    {
        public GetLotName()
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
    public class GetKindNmae : TCPIPQueryData
    {
        public GetKindNmae()
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
    public class GetWaferSize : TCPIPQueryData
    {
        public GetWaferSize()
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
    public class GetOrientationFlatAngle : TCPIPQueryData
    {
        public GetOrientationFlatAngle()
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
    public class GetCardID : TCPIPQueryData
    {
        public GetCardID()
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
    public class GetWaferID : TCPIPQueryData
    {
        public GetWaferID()
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
    public class GetCarrierStatus : TCPIPQueryData
    {
        public GetCarrierStatus()
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
    public class GetCurrentSlotNumber : TCPIPQueryData
    {
        public GetCurrentSlotNumber()
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
    public class GetMoveResult : TCPIPQueryData
    {
        public GetMoveResult()
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
    public class GetOverDrive : TCPIPQueryData
    {
        public GetOverDrive()
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
    public class GetXCoordinate : TCPIPQueryData
    {
        public GetXCoordinate()
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
    public class GetYCoordinate : TCPIPQueryData
    {
        public GetYCoordinate()
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
    public class GetProberStatus : TCPIPQueryData
    {
        public GetProberStatus()
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
    public class GetOnWaferInfo : TCPIPQueryData
    {
        public GetOnWaferInfo()
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
    public class GetDRInfo : TCPIPQueryData
    {
        public GetDRInfo()
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
    public class GetDutMapInformation : TCPIPQueryData
    {
        public GetDutMapInformation()
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
    public class OnWaferInformation : QueryData
    {
        private string MakeOnWaferInfo(long mX, long mY)
        {
            string retData = null;

            try
            {
                var dies = this.StageSupervisor().WaferObject.GetSubsInfo().DIEs;

                List<IDut> DutList = null;
                IEnumerable<IDut> tmpDutList = (new DutList()).GetRequestResult() as IEnumerable<IDut>;
                DutList = new List<IDut>(tmpDutList);

                int[] o_code_new = null;
                long dmx, dmy;

                int idxCount = DutList.Count;

                o_code_new = new int[idxCount];

                for (int i = 0; i < idxCount; i++)
                {
                    o_code_new[i] = 0x40;
                }

                int usedcount = 0;

                for (int i = 0; i < DutList.Count; i++)
                {
                    //dmx = mX + DutList[i].MacIndex.XIndex;
                    //dmy = mY + DutList[i].MacIndex.YIndex;

                    dmx = mX + DutList[i].UserIndex.XIndex;
                    dmy = mY + DutList[i].UserIndex.YIndex;

                    bool isDutIsInRange = this.ProbingModule().DutIsInRange(dmx, dmy);

                    if (isDutIsInRange)
                    {
                        if (dies[dmx, dmy].DieType.Value == DieTypeEnum.TEST_DIE)
                        {
                            if (this.ProbingModule().IsTestedDIE(dmx, dmy) == true)
                            {
                                int oCodeIdx = i / 4;
                                o_code_new[oCodeIdx] = o_code_new[oCodeIdx] | (1 << (i % 4));

                                usedcount++;
                            }
                        }
                    }
                }

                // OnWafer count
                double c = DutList.Count / 4;
                int reminder = DutList.Count % 4;
                double onWaferCount = Math.Truncate(c);

                if (reminder > 0)
                {
                    onWaferCount += 1;
                }

                for (int i = 0; i < (int)onWaferCount; i++)
                {
                    retData += Encoding.ASCII.GetString(new byte[] { (byte)o_code_new[i] });
                }

                LoggerManager.Debug($"[OnWaferInformation], MakeOnWaferInfo() : x = {mX}, y = {mY}, Used die's count = {usedcount}, onWaferCount = {onWaferCount}");
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
    public class DRValue : QueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                bool IsFound = false;

                string recvData = this.Argument.ToString().ToUpper();

                // TODO : Validation 
                // DR + 000000 + ?, Length is 9

                if (recvData.Length == 9)
                {
                    string DRId = recvData.Replace("DR", "").Replace("?", "");

                    int id = Convert.ToInt32(DRId);

                    DRDWConnectorBase dRDWConnectorBase = this.TCPIPModule().GetDRDWConnector(id);

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

    [Serializable]
    public class DutMapInformation : QueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                string recvData = string.Empty;

                // Sample)
                // DMI = #29#34#X1,X2,X3

                IProbeCardDevObject probecarddev = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef;

                int width = probecarddev.DutIndexSizeX;
                int height = probecarddev.DutIndexSizeY;

                string dutmapinfo = string.Empty;
                string seperator = ",";

                foreach (IDut dut in probecarddev.DutList)
                {
                    int index = ((int)dut.MacIndex.YIndex * width) + (int)dut.MacIndex.XIndex + 1;
                    dutmapinfo += index.ToString() + seperator;
                }

                dutmapinfo = dutmapinfo.Remove(dutmapinfo.Length - 1, 1);
                dutmapinfo = "#" + $"{width}" + "#" + $"{height}" + "#" + dutmapinfo;

                this.Result = dutmapinfo;

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                this.Result = string.Empty;

                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class CarrierStatus : QueryData
    {
        // TODO : 로직 분리 가능?

        public override EventCodeEnum Run()
        {
            //Store each carrier status in order of carrier number, and separate with a comma(",")

            //N：without carrier
            //U：with carrier(not tested yet)
            //T：with carrier(under testing) => LotState is 
            //D：with carrier(already tested) => LotState is End

            string FoupStatus = string.Empty;

            byte[] bytefoupinfos = this.LoaderController()?.GetBytesFoupObjects();

            if (bytefoupinfos != null)
            {
                // Deserialize
                object target;

                WrappedCasseteInfos foupinfos = null;

                SerializeManager.DeserializeFromByte(bytefoupinfos, out target, typeof(WrappedCasseteInfos));

                if (target != null)
                {
                    foupinfos = target as WrappedCasseteInfos;
                }

                if (foupinfos != null)
                {
                    // foupinfos[0].State => FoupStateEnum (ERROR, LOAD, UNLOAD, EMPTY_CASSETTE)
                    // foupinfos[0].LotState => LotStateEnum (Idle, Running, Pause, Error, Done, End, Cancel)

                    string lotstatealias = string.Empty;

                    foreach (var foup in foupinfos.Foups)
                    {
                        if (foup.State == FoupStateEnum.LOAD || foup.State == FoupStateEnum.UNLOAD)
                        {
                            switch (foup.LotState)
                            {
                                case LotStateEnum.Idle:
                                case LotStateEnum.Pause:
                                case LotStateEnum.Error:
                                case LotStateEnum.Done:
                                case LotStateEnum.Cancel:
                                    lotstatealias = "U";
                                    break;
                                case LotStateEnum.Running:
                                    lotstatealias = "T";
                                    break;
                                case LotStateEnum.End:
                                    lotstatealias = "D";
                                    break;
                                default:
                                    break;
                            }

                            if (FoupStatus == string.Empty)
                            {
                                FoupStatus = lotstatealias;
                            }
                            else
                            {
                                FoupStatus = FoupStatus + "," + lotstatealias;
                            }
                        }
                        else if (foup.State == FoupStateEnum.EMPTY_CASSETTE)
                        {
                            if (FoupStatus == string.Empty)
                            {
                                FoupStatus = "N";
                            }
                            else
                            {
                                FoupStatus = FoupStatus + "," + "N";
                            }
                        }
                        else
                        {
                            if (FoupStatus == string.Empty)
                            {
                                FoupStatus = "N";
                            }
                            else
                            {
                                FoupStatus = FoupStatus + "," + "N";
                            }
                        }

                        LoggerManager.Debug($"[GetCarrierStatus], Run() : Foup index = {foup.Index}, Foup state = {foup.State}, Lot state = {foup.LotState}");
                    }
                }
            }

            Result = FoupStatus;

            return EventCodeEnum.NONE;
        }
    }


    [Serializable]
    public class GetPortNumberByFoupAllocatedInfo : QueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (this.Argument != null && this.Argument is FoupAllocatedInfo)
                {
                    var allocatedinfo = this.Argument as FoupAllocatedInfo;

                    Result = allocatedinfo.FoupNumber.ToString();

                    retval = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    [Serializable]
    public class GetDeviceNameByFoupAllocatedInfo : QueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (this.Argument != null && this.Argument is FoupAllocatedInfo)
                {
                    var allocatedinfo = this.Argument as FoupAllocatedInfo;

                    Result = allocatedinfo.DeviceName;

                    retval = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    [Serializable]
    public class GetLotNameByFoupAllocatedInfo : QueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (this.Argument != null && this.Argument is FoupAllocatedInfo)
                {
                    var allocatedinfo = this.Argument as FoupAllocatedInfo;

                    Result = allocatedinfo.LotName;

                    retval = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
    [Serializable]
    public class GetCardIDByProbeCardInfo : QueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (this.Argument != null && this.Argument is string)
                {
                    Result = this.Argument.ToString();

                    retval = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
}

