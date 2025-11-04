using System;
using LogModule;
using ProberErrorCode;
using RequestInterface;

namespace RequestCore.Controller
{
    [Serializable]
    public abstract class Controller : RequestBase
    {
        public abstract override EventCodeEnum Run();
        public Controller() { }
    }

    [Serializable]
    public class Branch : Controller
    {
        public RequestBase Condition { get; set; }

        public RequestBase Positive { get; set; }
        public RequestBase Negative { get; set; }

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                bool tmpParseResult = false;
                bool conditionResult = false;

                Condition.Argument = this.Argument;
                Condition.Run();

                if(Condition.Result != null)
                {
                    tmpParseResult = bool.TryParse(Condition.Result.ToString(), out conditionResult);

                    if (tmpParseResult == true)
                    {
                        if (conditionResult == true)
                        {
                            Positive.Argument = this.Argument;
                            retVal = Positive.Run();
                            this.Result = Positive.Result;
                        }
                        else
                        {
                            Negative.Argument = this.Argument;
                            retVal = Negative.Run();
                            this.Result = Negative.Result;
                        }
                    }
                    else
                    {
                        this.Result = "";
                        retVal = EventCodeEnum.UNDEFINED;
                    }
                }
                else
                {
                    this.Result = "";
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

    #region 주석
    //public enum LoopMode
    //{
    //    MERGE,
    //    CALCULATE
    //}

    //public class PolicyForLoop
    //{
    //    public int StartPoint  { get; set; } = 0;
    //    public int EndPoint    { get; set; } = 0;
    //    public int Term { get; set; } = 0;
    //}

    //public class Loop : Controller
    //{
    //    public LoopMode LoopMode { get; set; } = LoopMode.MERGE;

    //    public RequestBase LogicCondition { get; set; }
    //    public PolicyForLoop CountCondition { get; set; }

    //    public RequestBase Logic { get; set; }

    //    protected override EventCodeEnum Run()
    //    {
    //        EventCodeEnum retVal = EventCodeEnum.NONE;
    //        string mergeModeResult = "";
    //        double calculateModeResult = 0;

    //        bool conditionResult = false;

    //        bool.TryParse(LogicCondition.GetRequestResult(), out conditionResult);

    //        if(    conditionResult         != false
    //            || CountCondition != null
    //        )
    //        {
    //            if(conditionResult != false)
    //            {
    //                while (!conditionResult)
    //                {
    //                    if(LoopMode == LoopMode.MERGE)
    //                    {
    //                        mergeModeResult += Logic.GetRequestResult();
    //                    }
    //                    else
    //                    {
    //                        double LogicVal = 0;
    //                        bool LogicConvertResult = double.TryParse(Logic.GetRequestResult(), out LogicVal);

    //                        if(LogicConvertResult != false)
    //                        {
    //                            calculateModeResult += LogicVal;
    //                        }
    //                    }
    //                }
    //            }
    //            else if(CountCondition != null)
    //            {
    //                if (CountCondition.Term == 0)
    //                    CountCondition.Term = 1;

    //                for (int i = CountCondition.StartPoint; i < CountCondition.EndPoint; i+= CountCondition.Term)
    //                {
    //                    if (LoopMode == LoopMode.MERGE)
    //                    {
    //                        mergeModeResult += Logic.GetRequestResult();
    //                    }
    //                    else
    //                    {
    //                        double LogicVal = 0;

    //                        bool LogicConvertResult = double.TryParse(Logic.GetRequestResult(), out LogicVal);

    //                        if (LogicConvertResult != false)
    //                        {
    //                            calculateModeResult += LogicVal;
    //                        }
    //                    }
    //                }
    //            }

    //            if(LoopMode == LoopMode.MERGE)
    //            {
    //                Result = mergeModeResult;
    //            }
    //            else
    //            {
    //                Result = calculateModeResult.ToString();
    //            }

    //        }
    //        else
    //        {
    //            retVal = EventCodeEnum.UNDEFINE; 
    //        }

    //        return retVal;
    //    }
    //}
    #endregion
}
