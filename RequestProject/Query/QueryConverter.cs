using System;
using System.Collections.Generic;
using System.Linq;
using ProberErrorCode;
using LogModule;
using RequestCore.OperationPack;
using RequestInterface;
using RequestCore.Interface;

namespace RequestCore.QueryPack
{
    [Serializable]
    public abstract class QueryConverter : Query
    {
        public abstract override EventCodeEnum Run();
    }

    [Serializable]
    public class QueryOperation : QueryConverter
    {
        public Operation Operation { get; set; }

        public override EventCodeEnum Run()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                QueryData QueryData = Operation.GetResult();

                Result = QueryData.GetRequestResult();

                retval = EventCodeEnum.NONE;
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    [Serializable]
    public class QueryFill : QueryConverter
    {
        public RequestBase Data { get; set; }
        public RequestBase PaddingData { get; set; }
        public RequestBase RightPaddingData { get; set; }
        public RequestBase Length { get; set; }
        public bool Vector { get; set; }

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                string tmpBuf = "";
                string tmpRBuf = "";

                string sData = "";
                char paddingChar = '\0';
                char paddingRChar = '\0';

                int totalWidth = 0;
                bool bParseResult = false;

                var data = Data?.GetRequestResult();
                if (data != null)
                {
                    sData = data.ToString();
                }

                tmpBuf = PaddingData?.GetRequestResult().ToString();
                tmpRBuf = RightPaddingData?.GetRequestResult().ToString();

                if (!string.IsNullOrEmpty(tmpBuf))
                {
                    paddingChar = tmpBuf.ElementAt(0); //#Hynix_Merge: 검토 필요, 뭐지 왜 두번해주지..?
                }

                if (!string.IsNullOrEmpty(tmpRBuf))
                {
                    paddingRChar = tmpRBuf.ElementAt(0);
                }

                bParseResult = int.TryParse(Length?.GetRequestResult().ToString(), out totalWidth);
                if (Vector)
                {
                    int dInt;
                    if(int.TryParse(sData, out dInt))
                    {
                        if(dInt < 0)
                        {
                            totalWidth -= 1;
                        }
                    }
                    
                }


                if (tmpBuf != null && tmpBuf != paddingChar.ToString())
                {
                    sData += tmpBuf;

                }
                else if (tmpRBuf != null && tmpRBuf != paddingRChar.ToString())
                {
                    sData += tmpRBuf;
                }
                else
                {
                    if (paddingChar != '\0' && totalWidth != 0)
                    {
                        sData = sData.PadLeft(totalWidth, paddingChar);
                    }
                    else if (paddingRChar != '\0' && totalWidth != 0)
                    {
                        sData = sData.PadRight(totalWidth, paddingRChar);
                    }
                }

                if (Length != null)
                {
                    int len = int.Parse(Length?.GetRequestResult().ToString());
                    if (sData.Length > len)
                    {
                        sData = sData.Substring(0, len);
                        LoggerManager.Debug($"sData.Length is over than {sData.Length}");
                        LoggerManager.Debug($"Return data is {sData}");
                    }
                }

                this.Result = sData;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;

        }
    }

   [Serializable]
    public class QueryHasAffix : QueryConverter
    {
        public string prefix { get; set; }
        public RequestBase Data { get; set; }
        public string InvaildData { get; set; }
        public string DataFormat { get; set; }
        public string postfix { get; set; }

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                string sData = "";

                if(Data != null)
                {
                    Data.Argument = this.Argument;

                    retVal = Data.Run();

                    if(retVal == EventCodeEnum.NONE)
                    {
                        //object requestresult = Data.GetRequestResult();
                        object requestresult = Data.Result;

                        if (requestresult != null)
                        {
                            if (string.IsNullOrEmpty(DataFormat) == false)
                            {
                                int n;
                                var isNumeric = int.TryParse(requestresult.ToString(), out n);

                                if (isNumeric == true)
                                {
                                    sData = String.Format(DataFormat, n);
                                }
                                else
                                {
                                    sData = String.Format(DataFormat, requestresult);
                                }
                            }
                            else
                            {
                                sData = requestresult.ToString();
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(InvaildData) == false)
                            {
                                sData = InvaildData;
                            }
                        }

                        this.Result = prefix + sData + postfix;
                    }
                    else
                    {
                        this.Result = default(string);
                    }
                }
                else
                {
                    LoggerManager.Error($"[QueryHasAffix], Run() : Data is null.");
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
    public class QueryMerge : QueryConverter
    {
        public List<RequestBase> Querys { get; set; } = new List<RequestBase>();

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                Result = "";

                foreach (var Query in Querys)
                {
                    Query.Argument = this.Argument;
                    Result += Query?.GetRequestResult()?.ToString();
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
    public class QueryIntToBool : QueryConverter
    {
        public RequestBase Data
        {
            get;
            set;
        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                bool parseResult = false;
                int intOperand = 0;

                parseResult = int.TryParse(Data?.GetRequestResult().ToString(), out intOperand);

                if (parseResult == true)
                {
                    if (intOperand == 0)
                    {
                        Result = bool.FalseString;
                    }
                    else
                    {
                        Result = bool.TrueString;
                    }
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
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
    public class QueryBoolToInt : QueryConverter
    {
        public RequestBase Data
        {
            get;
            set;
        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                bool parseResult = false;
                bool boolOperand = false;

                parseResult = bool.TryParse(Data?.GetRequestResult().ToString(), out boolOperand);

                if (parseResult == true)
                {
                    if (boolOperand == true)
                    {
                        Result = "1";
                    }
                    else
                    {
                        Result = "0";
                    }
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
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
    public class QueryIntToHex : QueryConverter
    {
        public RequestBase Data
        {
            get;
            set;
        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                bool parseResult = false;
                int intOperand = 0;

                parseResult = int.TryParse(Data?.GetRequestResult().ToString(), out intOperand);

                if (parseResult == true)
                {
                    intOperand = Math.Abs(intOperand);
                    Result = intOperand.ToString("x");
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
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
    //public class QueryListToCount : QueryConverter
    //{
    //    public RequestBase Data
    //    {
    //        get;
    //        set;
    //    }
    //    public override EventCodeEnum Run()
    //    {
    //        EventCodeEnum retVal = EventCodeEnum.NONE;
    //        try
    //        {
    //            List<object> operand = null;
    //            int intOperand = -1;

    //            operand = (List<object>)Data?.GetRequestResult();

    //            if (operand != null)
    //            {
    //                intOperand = operand.Count;
    //                Result = intOperand.ToString();
    //                retVal = EventCodeEnum.NONE;
    //            }
    //            else
    //            {
    //                Result = intOperand;
    //                retVal = EventCodeEnum.UNDEFINED;
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
    public class QueryFloor : QueryConverter
    {
        public RequestBase Data
        {
            get;
            set;
        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                bool parseResult = false;
                double dblOperand = 0;

                parseResult = double.TryParse(Data?.GetRequestResult().ToString(), out dblOperand);

                if (parseResult == true)
                {
                    dblOperand = Math.Floor(dblOperand);
                    Result = (int)dblOperand;
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
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
    public class QueryNumToAscii : QueryConverter
    {
        public RequestBase Data { get; set; }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                string parseResult = string.Empty;
                string retString = string.Empty;

                parseResult = Data?.GetRequestResult().ToString();
                parseResult = ((int)(float.Parse(parseResult))).ToString(); // 편의상 일단 정수부만 사용 (반올림 X, 내림..)...

                if (parseResult != null)
                {                    
                    foreach (char c in parseResult)
                    {
                        retString += c;
                    }
                    Result = retString;
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
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
    public class QueryHexToInt : QueryConverter
    {
        public RequestBase Data
        {
            get;
            set;
        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                int intOperand = 0;

                intOperand = Convert.ToInt32(Data.GetRequestResult().ToString(), 16);

                Result = intOperand.ToString();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class QuerySignal : QueryConverter
    {
        public SignalType SignalType
        {
            get;
            set;
        }

        public RequestBase Data
        {
            get;
            set;
        }

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                bool parseResult = false;
                double dblOperand = 0;

                parseResult = double.TryParse(Data?.GetRequestResult().ToString(), out dblOperand);

                if (parseResult == true)
                {
                    /////////////////////////구현 必







                    /////////////////////////
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
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
    public class QueryDownUnit : QueryConverter
    {
        public RequestBase Data
        {
            get;
            set;
        }

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                bool parseResult = false;
                double dblOperand = 0;

                parseResult = double.TryParse(Data?.GetRequestResult().ToString(), out dblOperand);

                if (parseResult == true)
                {
                    dblOperand = dblOperand / (double)Argument;
                    Result = dblOperand.ToString();
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
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
    public class QueryAbs : QueryConverter
    {
        public RequestBase Data
        {
            get;
            set;
        }

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                bool parseResult = false;
                double dblOperand = 0;

                parseResult = double.TryParse(Data?.GetRequestResult().ToString(), out dblOperand);

                if (parseResult == true)
                {
                    dblOperand = Math.Abs(dblOperand);
                    Result = dblOperand.ToString();
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
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
    public class QueryHasStringFormattor : QueryConverter
    {     
        public List<RequestBase> Datas { get; set; }
        public string Fomatter = "";


        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                var args = new object[Datas.Count()];
                for (int i = 0; i < args.Length; i++)
                {
                    Datas[i].Run();
                    args[i] = Datas[i].Result;
                }

                this.Result = String.Format(Fomatter, args);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

}
