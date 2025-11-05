using System;
using System.Linq;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using RequestCore.QueryPack;
using RequestInterface;

namespace RequestCore.OperationPack
{
    [Serializable]
    public class Calculator : RequestBase
    {
        public RequestBase x { get; set; }
        public RequestBase y { get; set; }
        public string logic { get; set; }

        public RequestBase convert { get; set; }

        public Calculator(string logic)
        {
            this.logic = logic;
        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                object result = null;
                //Type type = default(Type);

                int xint; 
                int yint;
                double xdouble, ydouble;

                
                if (int.TryParse(x.GetRequestResult().ToString(), out xint) && int.TryParse(y.GetRequestResult().ToString(), out yint))
                {
                    switch (logic)
                    {
                        case "+":
                            result = xint + yint;
                            break;
                        case "-":
                            result = xint - yint;
                            break;
                        case "*":
                            result = xint * yint;
                            break;
                        case "/":
                            result = xint / yint;
                            break;
                        default:
                            break;
                    }

                }
                else if (double.TryParse(x.GetRequestResult().ToString(), out xdouble) && double.TryParse(y.GetRequestResult().ToString(), out ydouble))
                {
                    switch (logic)
                    {
                        case "+":
                            result = xdouble + ydouble;
                            break;
                        case "-":
                            result = xdouble - ydouble;
                            break;
                        case "*":
                            result = xdouble * ydouble;
                            break;
                        case "/":
                            result = xdouble / ydouble;
                            break;                        
                        default:
                            break;
                    }

                }



                if (convert != null)
                {
                    convert.Argument = result;
                    this.Result = convert.GetRequestResult();
                }
                else
                {
                    this.Result = result;
                }

                
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    [Serializable]
    public class DivisionByArgument : RequestBase
    {
        public double OperandValue { get; set; }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                double dblArgument = 0;

                dblArgument = Convert.ToDouble(this.Argument);

                this.Result = dblArgument / OperandValue;

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    [Serializable]
    public class MultiplicationByArgument : RequestBase
    {
        public double OperandValue { get; set; }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                double dblArgument = 0;

                dblArgument = Convert.ToDouble(this.Argument);

                this.Result = dblArgument * OperandValue;

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    [Serializable]
    public class ToInt : RequestBase
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                bool brst;
                int irst;
                double drst;
                if (this.Argument != null)
                {
                    string argu;
                    if (this.Argument is RequestBase)
                    {
                        argu = (Argument as RequestBase).GetRequestResult().ToString();
                    }
                    else 
                    {
                        argu = Argument.ToString();
                    }

                    if (bool.TryParse(argu, out brst))
                    {
                        this.Result = brst ? 1 : 0;
                    }
                    else if (int.TryParse(argu, out irst))
                    {
                        this.Result = irst;
                    }
                    else if(double.TryParse(argu, out drst))
                    {
                        this.Result = (int)drst;
                    }                     
                    else
                    {
                        Result = 0;
                        LoggerManager.Debug($"ToInt Argument is invalid. value:{this.Argument}, type:{this.Argument.GetType()}");
                    }
                    retval = EventCodeEnum.NONE;
                }
                else
                {                    
                    Result = 0;
                    LoggerManager.Debug($"ToInt Argument is null.");
                }
            }
            catch (OverflowException err)
            {
                Result = 0;
                LoggerManager.Exception(err);
            }
            catch (FormatException err)
            {
                Result = 0;
                LoggerManager.Exception(err);
            }
            catch (Exception err)
            {
                Result = 0;
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    [Serializable]
    public class IntToEnum : RequestBase
    {
        public Type Type { get; set; }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                int inputval = Convert.ToInt32(this.Argument);

                bool success = Enum.IsDefined(Type, inputval);

                if(success == true)
                {
                    this.Result = Enum.ToObject(Type, inputval);

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Debug($"IntToEnum is fail.");
                }
            }
            catch (OverflowException err)
            {
                LoggerManager.Exception(err);
            }
            catch (FormatException err)
            {
                LoggerManager.Exception(err);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    [Serializable]
    public class EnumToInt : RequestBase
    {
        public Type Type { get; set; }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                int inputval = (int)this.Argument;

                this.Result = inputval;

                retval = EventCodeEnum.NONE;
            }
            catch (OverflowException err)
            {
                LoggerManager.Exception(err);
            }
            catch (FormatException err)
            {
                LoggerManager.Exception(err);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    [Serializable]
    public class FillArg : RequestBase
    {
        public RequestBase PaddingData { get; set; }
        public RequestBase RightPaddingData { get; set; }
        public RequestBase Length { get; set; }

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

                var data = Argument;
                if (data != null)
                {
                    sData = data.ToString();
                }

                tmpBuf = PaddingData?.GetRequestResult().ToString();
                tmpRBuf = RightPaddingData?.GetRequestResult().ToString();

                if (!string.IsNullOrEmpty(tmpBuf))
                {
                    paddingChar = tmpBuf.ElementAt(0);
                }

                if (!string.IsNullOrEmpty(tmpRBuf))
                {
                    paddingRChar = tmpRBuf.ElementAt(0);
                }

                bParseResult = int.TryParse(Length?.GetRequestResult().ToString(), out totalWidth);
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
    public class UserToTskMapDir : QueryData
    {
        public RequestBase WaferMapDir = null;

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                bool parseResult = false;
                object dblOperand = WaferMapDir.GetRequestResult();
                parseResult = dblOperand is MapVertDirectionEnum;

                if (parseResult == true)
                {
                    if (dblOperand != null)
                    {
                        switch (dblOperand)
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

                    retVal = EventCodeEnum.NONE;
                    return retVal;

                }

                parseResult = dblOperand is MapHorDirectionEnum;

                if (parseResult == true)
                {
                    switch (dblOperand)
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

                    retVal = EventCodeEnum.NONE;
                    return retVal;
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
    public class HasAffixArg : RequestBase
    {
        public string prefix { get; set; }        
        public string InvaildData { get; set; }
        public string DataFormat { get; set; }
        public string postfix { get; set; }

        public RequestBase OptimizeFormat { get; set; }

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                string sData = "";

                var data = Argument;
                if (data != null)
                {

                    object requestresult = data;

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

                    if(OptimizeFormat != null)
                    {
                        OptimizeFormat.Argument = sData;
                        sData = OptimizeFormat.GetRequestResult().ToString();
                    }

                    this.Result = prefix + sData + postfix;
                }
                else
                {
                    this.Result = default(string);
                }
                retVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }
}
