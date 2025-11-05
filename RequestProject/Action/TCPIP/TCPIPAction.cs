using System;
using LogModule;
using NotifyEventModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.BinData;
using ProberInterfaces.Command.Internal;
using RequestCore.Query.TCPIP;
using RequestInterface;

namespace RequestCore.ActionPack.TCPIP
{
    [Serializable]
    public abstract class TCPIPAction : Action
    {
        public TCPIPAction() { }
    }

    [Serializable]
    public class TCPIPException : Action
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                LoggerManager.Exception(new Exception(), Argument.ToString());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class ResponseActionCommand : TCPIPAction
    {
        /// <summary>
        /// 커맨드 포트로부터 받은, 커맨드의 응답을 수행
        /// </summary>
        /// <returns></returns>
        /// 
        public Acknowledgement ACK = new Acknowledgement();
        public Acknowledgement NACK = new Acknowledgement();

        public RequestBase Validation { get; set; }

        public EventCodeEnum MakeResponseResultAndWriteString()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            string response = string.Empty;

            try
            {
                Acknowledgement acknowledgement = null;

                if (IsValid() == EventCodeEnum.NONE)
                {
                    acknowledgement = ACK;
                }
                else
                {
                    acknowledgement = NACK;
                }

                response = $"{acknowledgement.prefix}" + $"{Argument.ToString()}" + $"{acknowledgement.postfix}";

                retval = this.TCPIPModule().WriteString(response);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                retVal = MakeResponseResultAndWriteString();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum IsValid()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Validation == null)
                {
                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    Validation.Argument = this.Argument;
                    retval = Validation.Run();
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
    public class GetCalcualtePassFailYield : Action
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                ITCPIP tcpip = this.TCPIPModule();

                bool commandResult = false;
                string binCode = string.Empty;
                this.Result = null;

                binCode = this.Argument.ToString();
                BinAnalysisDataArray binAnalysisData = tcpip.AnalyzeBin(binCode);

                if (binAnalysisData.Valid == true)
                {
                    commandResult = this.CommandManager().SetCommand<ISetBinAnalysisData>(this, new BinCommandParam() { Param = binAnalysisData });
                }
                else
                {
                    LoggerManager.Debug($"[GetCalcualtePassFailYield], Run() : AnalyzeBin() return wrong data. Bin code = {binCode}");
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
    public class SetDataUsingDWID : Action
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                // DR000000-0
                // Minimum Count = 10
                EventCodeEnum isValid = EventCodeEnum.UNDEFINED;

                DWDataBase dataIDValue = this.TCPIPModule().GetDWDataBase(this.Argument.ToString());

                if (dataIDValue?.IsValid == true)
                {
                    IElement elment = null;
                    int? convertedid = null;

                    // CONVERT ID
                    DRDWConnectorBase dRDWConnectorBase = this.TCPIPModule().GetDRDWConnector(dataIDValue.ID);
                    convertedid = dRDWConnectorBase?.ID;

                    // ID로 Get Element
                    if (convertedid != null)
                    {
                        elment = this.ParamManager().GetElement(convertedid.GetValueOrDefault());
                    }
                    else
                    {
                        elment = this.ParamManager().GetElement(dataIDValue.ID);
                    }

                    if (elment != null)
                    {
                        // CHECK RANGE

                        bool InRange = false;

                        double dblval = Convert.ToDouble(dataIDValue.value);

                        if (dblval >= elment.LowerLimit && dblval <= elment.UpperLimit)
                        {
                            InRange = true;
                        }
                        else
                        {
                            InRange = false;
                            isValid = EventCodeEnum.PARAM_SET_INVALID_OUT_OF_RANGE;
                        }

                        if (InRange == true)
                        {
                            if (dRDWConnectorBase.WriteValueConverter != null)
                            {
                                // CONVERT FORMAT
                                dRDWConnectorBase.WriteValueConverter.Argument = dataIDValue.value;

                                retVal = dRDWConnectorBase.WriteValueConverter.Run();

                                if (retVal == EventCodeEnum.NONE)
                                {
                                    object setvalue = dRDWConnectorBase.WriteValueConverter.Result;

                                    //isValid = elment.SetValue(setvalue, isNeedValidation: true, source: this.TCIPIP());
                                    isValid = elment.SetValue(setvalue);//TODO: 위 코드를 안넣는게 나은지 검토 필요.
                                }
                            }
                            else
                            {
                                //isValid = elment.SetValue(setvalue, isNeedValidation: true, source: this.TCIPIP());
                                isValid = elment.SetValue(dataIDValue.value);//TODO: 위 코드를 안넣는게 나은지 검토 필요.
                            }
                        }
                    }
                }

                if (isValid == EventCodeEnum.NONE || isValid == EventCodeEnum.PARAM_SET_EQUAL_VALUE)
                {
                    this.EventManager().RaisingEvent(typeof(PassDWCommandEvent).FullName);
                }
                else
                {
                    this.EventManager().RaisingEvent(typeof(FailDWCommandEvent).FullName);
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
    public class DRDWValueValidation : Action
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                // TODO : Validation
                // (1). 입력받은 값이 Element의 Limit 조건에 부합 되는가
                // (2) 입력받은 ID에 해당하는 Element에 Write가 가능한가

                ITCPIP tcpip = this.TCPIPModule();

                DWDataBase data = tcpip.GetDWDataBase(this.Argument.ToString());

                if (data?.IsValid == true)
                {
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Debug($"[DRDWValueValidation], Run() : invalid case.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    //[Serializable]
    //public class SetHardwareAsseblyVerify : Action
    //{
    //    private const string Identifier = "HWAV";

    //    public override EventCodeEnum Run()
    //    {
    //        EventCodeEnum retVal = EventCodeEnum.NONE;

    //        try
    //        {
    //            string inputcmd = this.Argument.ToString();

    //            // Seperate : Command & Value

    //            string seperatestr = inputcmd.Replace(Identifier, string.Empty);

    //            var isNumeric = int.TryParse(seperatestr, out int n);

    //            ILotOPModule lotopmodule = this.LotOPModule();

    //            if (isNumeric == true)
    //            {
    //                if (n == 0)
    //                {
    //                    lotopmodule.LotInfo.HardwareAsseblyVerifed.Value = false;
    //                }
    //                else if (n == 1)
    //                {
    //                    lotopmodule.LotInfo.HardwareAsseblyVerifed.Value = true;
    //                }
    //                else
    //                {
    //                    lotopmodule.LotInfo.HardwareAsseblyVerifed.Value = false;
    //                }
    //            }
    //            else
    //            {
    //                // TODO : 
    //                lotopmodule.LotInfo.HardwareAsseblyVerifed.Value = false;
    //            }
    //        }
    //        catch (Exception err)
    //        {
    //            LoggerManager.Exception(err);
    //        }

    //        return retVal;
    //    }
    //}

    [Serializable]
    public class SetLotProcessingVerify : Action
    {
        private const string Identifier = "LPV";
        private const string Identifier_P = "P";

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                string inputcmd = this.Argument.ToString();

                // Seperate : Command & Value

                // TODO : #

                bool isValid = false;

                int n = -1;
                int portNum = -1;

                if (inputcmd.Contains("#"))
                {
                    var words = inputcmd.Split('#');

                    if (words.Length == 2)
                    {
                        string seperatestr = words[0].Replace(Identifier, string.Empty);

                        isValid = int.TryParse(seperatestr, out n);

                        if (isValid)
                        {
                            seperatestr = words[1].Replace(Identifier_P, string.Empty);
                            isValid = int.TryParse(seperatestr, out portNum);
                        }
                    }
                    else
                    {
                        // ERROR
                        LoggerManager.Error($"[{this.GetType().Name}], Run() : Argument = {Argument}, Length = {words.Length}");
                    }
                }
                else
                {
                    string seperatestr = inputcmd.Replace(Identifier, string.Empty);
                    isValid = int.TryParse(seperatestr, out n);
                    portNum = 1;
                }

                if (isValid == true)
                {
                    if (n == 1)
                    {
                        SendData(portNum, true);
                    }
                    else
                    {
                        SendData(portNum, false);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        private EventCodeEnum SendData(int portnumber, bool val)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                this.LoaderController()?.UpdateLotVerifyInfo(portnumber, val);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

    }

    [Serializable]
    public class SetProbeCardinfoVerify : Action
    {
        private const string Identifier = "ProbeCardLoadStart";
        private const string Identifier_Stage = "Stage";

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                string inputcmd = this.Argument.ToString();
                // Seperate : Command & Value
                // TODO : #
                bool isValid = false;

                int n = -1;
                int stageindex = -1;
                string cardid = string.Empty;

                if (inputcmd.Contains("#"))
                {
                    var words = inputcmd.Split('#');
                    if (words.Length == 3)
                    {
                        string seperatestr = words[0].Replace(Identifier, string.Empty);

                        isValid = int.TryParse(seperatestr, out n);
                        if (isValid)
                        {
                            seperatestr = words[1].Replace(Identifier_Stage, string.Empty);

                            isValid = int.TryParse(seperatestr, out stageindex);
                            if (isValid == true)
                            {
                                isValid = stageindex == this.LoaderController().GetChuckIndex();
                                cardid = words[2];
                            }
                        }
                    }
                }

                if (isValid == true)
                {
                    bool result = n == 1 ? true : false;
                    this.TCPIPModule().CardchangeTempInfo = new CardchangeTempInfo(result, cardid);
                }
                else
                {
                    LoggerManager.Debug($"[SetProbeCardinfoVerify], Run() : This command is unformatted. [{inputcmd}]");
                    retVal = EventCodeEnum.COMMAND_ERROR;
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
