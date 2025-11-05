using System;
using LogModule;
using RequestCore.QueryPack;

namespace RequestCore.OperationPack
{
    [Serializable]
    public abstract class Compare : Operation
    {
        public CalculateData CalculateData;
        public abstract override QueryData GetResult();

        public Compare() { }
    }

    [Serializable]
    public class LeftSmallerThanRight : Compare
    {
        public override QueryData GetResult()
        {
            FixData retData = new FixData();

            try
            {
                bool result = false;
                double dblLeftOperand = 0;
                double dblRightOperand = 0;
                bool parseResult = false;

                if (CalculateData != null
                    && CalculateData.LeftOperand != null
                    && CalculateData.RightOperand != null)
                {
                    parseResult = double.TryParse(CalculateData.LeftOperand.GetRequestResult().ToString(), out dblLeftOperand);
                    parseResult = double.TryParse(CalculateData.RightOperand.GetRequestResult().ToString(), out dblRightOperand) | parseResult;

                    if (parseResult == true)
                    {
                        result = dblLeftOperand < dblRightOperand;
                    }
                    else
                    {
                        LoggerManager.Debug($"[{nameof(RequestCore)} - {nameof(LeftSmallerThanRight)}] [Error = Input Wrong Data Value.]");
                        result = false;
                    }
                }
                else
                {
                    LoggerManager.Debug($"[{nameof(RequestCore)} - {nameof(LeftSmallerThanRight)}] [Error = Input Empty Data Value.]");
                    result = false;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retData;
        }
    }

    [Serializable]
    public class LeftBiggerThanRight : Compare
    {
        public override QueryData GetResult()
        {
            FixData retData = new FixData();

            try
            {
                bool result = false;
                double dblLeftOperand = 0;
                double dblRightOperand = 0;
                bool parseResult = false;

                if (CalculateData != null
                    && CalculateData.LeftOperand != null
                    && CalculateData.RightOperand != null)
                {
                    parseResult = double.TryParse(CalculateData.LeftOperand.GetRequestResult().ToString(), out dblLeftOperand);
                    parseResult = double.TryParse(CalculateData.RightOperand.GetRequestResult().ToString(), out dblRightOperand) | parseResult;

                    if (parseResult == true)
                    {
                        result = dblLeftOperand > dblRightOperand;
                    }
                    else
                    {
                        LoggerManager.Debug($"[{nameof(RequestCore)} - {nameof(LeftBiggerThanRight)}] [Error = Input Wrong Data Value.]");
                        result = false;
                    }
                }
                else
                {
                    LoggerManager.Debug($"[{nameof(RequestCore)} - {nameof(LeftBiggerThanRight)}] [Error = Input Empty Data Value.]");
                    result = false;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retData;
        }
    }

    [Serializable]
    public class Equal : Compare
    {
        public override QueryData GetResult()
        {
            FixData retData = new FixData();

            try
            {
                bool result = false;
                double dblLeftOperand = 0;
                double dblRightOperand = 0;
                bool parseResult = false;

                if (CalculateData != null
                    && CalculateData.LeftOperand != null
                    && CalculateData.RightOperand != null)
                {
                    parseResult = double.TryParse(CalculateData.LeftOperand.GetRequestResult().ToString(), out dblLeftOperand);
                    parseResult = double.TryParse(CalculateData.RightOperand.GetRequestResult().ToString(), out dblRightOperand) | parseResult;

                    if (parseResult == true)
                    {
                        result = dblLeftOperand == dblRightOperand;
                    }
                    else
                    {
                        LoggerManager.Debug($"[{nameof(RequestCore)} - {this.GetType().Name}] [Error = Input Wrong Data Value.]");
                        result = false;
                    }
                }
                else
                {
                    LoggerManager.Debug($"[{nameof(RequestCore)} - {this.GetType().Name}] [Error = Input Empty Data Value.]");
                    result = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retData;
        }
    }
}
