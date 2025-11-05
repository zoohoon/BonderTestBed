using LogModule;
using ProberErrorCode;
using RequestInterface;
using System;

namespace RequestCore.Formatter
{
    public enum ROUNDENUM
    {
        NOTUSE,
        CEILING,
        TRUNCATE,
        ROUND
    }

    [Serializable]
    public class UnitConverterByArg : RequestBase
    {
        public double unit { get; set; }
        public string operand { get; set; }
        public ROUNDENUM roundenum { get; set; }

        public UnitConverterByArg(double unit, string operand)
        {
            this.unit = unit;
            this.operand = operand;
            roundenum = ROUNDENUM.NOTUSE;
        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            Result = null;

            try
            {
                if (operand == "*")
                {
                    Result = Convert.ToDouble(this.Argument) * unit;
                }
                else if (operand == "/")
                {
                    Result = Convert.ToDouble(this.Argument) / unit;
                }
                else
                {
                    throw new Exception("Invalid operand.");
                }

                if(Result != null)
                {
                    switch (roundenum)
                    {
                        case ROUNDENUM.NOTUSE:
                            break;
                        case ROUNDENUM.CEILING:
                            Result = Math.Ceiling((double)Result);
                            break;
                        case ROUNDENUM.TRUNCATE:
                            Result = Math.Truncate((double)Result);
                            break;
                        case ROUNDENUM.ROUND:
                            // TODO : ??
                            Result = Math.Round((double)Result);
                            break;
                        default:
                            break;
                    }
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
    public class DateTimeFormatter : RequestBase
    {
        public string ParseExactFormat { get; set; }
        public string Format { get; set; }
        //public Type ArgType { get; set; }
        public DateTimeFormatter(string parseexactformat, string format)
        {
            this.ParseExactFormat = parseexactformat;
            this.Format = format;
        }

        public override EventCodeEnum Run()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                DateTime dtDate = default(DateTime);

                if (!string.IsNullOrEmpty(ParseExactFormat))
                {
                    dtDate = DateTime.ParseExact(this.Argument.ToString(), ParseExactFormat, null);
                }

                Result = dtDate.ToString(Format);

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
    public class NumericFormatter : RequestBase
    {
        public string Format { get; set; }
        //public Type ArgType { get; set; }
        public NumericFormatter(string format)
        {
            this.Format = format;
        }

        public override EventCodeEnum Run()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                double val;

                if(this.Argument != null)
                {
                    bool IsParse = double.TryParse(this.Argument.ToString(), out val);

                    if(IsParse == true)
                    {
                        Result = val.ToString(Format);

                        retval = EventCodeEnum.NONE;
                    }
                }

                if(retval != EventCodeEnum.NONE)
                {
                    Result = null;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    [Serializable]//GetDigitizerIndex
    public class DigitFormatter : RequestBase
    {
        public string Format { get; set; }

        public int Digit { get; set; }
        public int Length { get; set; }
        public DigitFormatter(string format, int digiti, int length)
        {
            this.Format = format;
            this.Digit = digiti;
            this.Length = length;
        }

        public override EventCodeEnum Run()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                double val;

                if (this.Argument != null)
                {
                    if (this.Digit >= 0 && this.Length >= 0)
                    {
                        Result = this.Argument.ToString().Substring(this.Digit, this.Length);
                        retval = EventCodeEnum.NONE;
                    }

                    bool IsParse = double.TryParse(Result.ToString(), out val);
                    if (IsParse == true)
                    {
                        Result = val.ToString(Format);
                        retval = EventCodeEnum.NONE;
                    }
                    else 
                    {
                    }
                }

                if (retval != EventCodeEnum.NONE)
                {
                    Result = null;
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
