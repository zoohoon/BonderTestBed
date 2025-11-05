using System;
using System.Collections.Generic;
using LogModule;
using RequestCore.QueryPack;
using RequestInterface;

namespace RequestCore.OperationPack
{
    [Serializable]
    public abstract class Calculate : Operation
    {
        private List<RequestBase> _Operands = new List<RequestBase>();
        public List<RequestBase> Operands
        {
            get { return _Operands; }
            set
            {
                _Operands = value;
            }
        }

        public abstract override QueryData GetResult();

        public Calculate() { }
    }

    [Serializable]
    public class Addition : Calculate
    {
        public override QueryData GetResult()
        {
            FixData retData = new FixData();
            try
            {
                double result = 0;
                double dblOperand = 0;
                bool parseResult = false;

                foreach (var Operand in Operands)
                {
                    parseResult = double.TryParse(Operand.GetRequestResult().ToString(), out dblOperand);

                    if (parseResult == true)
                    {
                        result += dblOperand;
                    }
                    else
                    {
                        LoggerManager.Debug($"[{nameof(RequestCore)} - {this.GetType().Name}] [Error = Input Empty Data Value.]");
                        result = 0;
                        break;
                    }
                }

                retData.ResultData = result.ToString();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retData;
        }
    }

    [Serializable]
    public class Subtraction : Calculate
    {
        public override QueryData GetResult()
        {
            FixData retData = new FixData();

            try
            {
                double result = 0;
                double dblOperand = 0;
                bool parseResult = false;

                foreach (var Operand in Operands)
                {
                    parseResult = double.TryParse(Operand.GetRequestResult().ToString(), out dblOperand);

                    if (parseResult == true)
                    {
                        result -= dblOperand;
                    }
                    else
                    {
                        LoggerManager.Debug($"[{nameof(RequestCore)} - {this.GetType().Name}] [Error = Input Empty Data Value.]");
                        result = 0;
                        break;
                    }
                }

                retData.ResultData = result.ToString();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retData;
        }
    }

    [Serializable]
    public class Division : Calculate
    {
        public override QueryData GetResult()
        {
            FixData retData = new FixData();

            try
            {
                double result = 0;
                double dblOperand = 0;
                bool parseResult = false;

                foreach (var Operand in Operands)
                {
                    parseResult = double.TryParse(Operand.GetRequestResult().ToString(), out dblOperand);

                    if (parseResult == true)
                    {
                        result /= dblOperand;
                    }
                    else
                    {
                        LoggerManager.Debug($"[{nameof(RequestCore)} - {this.GetType().Name}] [Error = Input Empty Data Value.]");
                        result = 0;
                        break;
                    }
                }

                retData.ResultData = result.ToString();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retData;
        }
    }

    [Serializable]
    public class Multiplication : Calculate
    {
        public override QueryData GetResult()
        {
            FixData retData = new FixData();

            try
            {
                double result = 1;
                double dblOperand = 0;
                bool parseResult = false;

                foreach (var Operand in Operands)
                {
                    parseResult = double.TryParse(Operand.GetRequestResult().ToString(), out dblOperand);

                    if (parseResult == true)
                    {
                        result *= dblOperand;
                    }
                    else
                    {
                        LoggerManager.Debug($"[{nameof(RequestCore)} - {this.GetType().Name}] [Error = Input Empty Data Value.]");
                        result = 0;
                        break;
                    }
                }

                retData.ResultData = result.ToString();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retData;
        }
    }

    [Serializable]
    public class Modulo : Calculate
    {
        public override QueryData GetResult()
        {
            FixData retData = new FixData();

            try
            {
                double result = 0;
                double dblOperand = 0;
                bool parseResult = false;

                foreach (var Operand in Operands)
                {
                    parseResult = double.TryParse(Operand.GetRequestResult().ToString(), out dblOperand);

                    if (parseResult == true)
                    {
                        result %= dblOperand;
                    }
                    else
                    {
                        LoggerManager.Debug($"[{nameof(RequestCore)} - {this.GetType().Name}] [Error = Input Empty Data Value.]");
                        result = 0;
                        break;
                    }
                }

                retData.ResultData = result.ToString();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retData;
        }
    }
}
