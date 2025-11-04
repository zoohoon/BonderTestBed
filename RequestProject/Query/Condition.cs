using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.PMI;
using ProberInterfaces.SequenceRunner;
using ProberInterfaces.WaferTransfer;
using RequestCore.QueryPack;
using RequestInterface;
using System;

namespace RequestCore.Query
{

    [Serializable]
    public class InRange : QueryData
    {
        public RequestBase Start { get; set; }
        public RequestBase End { get; set; }

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (Start != null && End != null)
                {
                    double inputval = Convert.ToDouble(this.Argument);

                    double startval = 0;
                    double endval = 0;

                    bool parseResult = false;

                    parseResult = double.TryParse(Start.GetRequestResult().ToString(), out startval);
                    parseResult = double.TryParse(End.GetRequestResult().ToString(), out endval) | parseResult;

                    if (parseResult == true)
                    {
                        if (inputval >= startval && inputval <= endval)
                        {
                            Result = true;

                            retVal = EventCodeEnum.NONE;
                        }
                        else
                        {
                            LoggerManager.Debug($"[{nameof(RequestCore)} - {nameof(InRange)}] [Error = Range is wrong. Start({startval}) <= Input({inputval}) <= End({endval})]");

                            Result = false;

                            retVal = EventCodeEnum.PARAM_SET_INVALID_OUT_OF_RANGE;
                        }
                    }
                    else
                    {
                        Result = false;
                    }
                }
                else
                {
                    LoggerManager.Debug($"[{nameof(RequestCore)} - {nameof(InRange)}] [Error = Input Empty Data Value.]");

                    Result = false;
                }
            }
            catch (Exception err)
            {
                Result = false;

                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }



    [Serializable]
    public class IsLotIdleState : QueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ILotOPModule LotOPModule = this.LotOPModule();

                if (LotOPModule.ModuleState.State == ModuleStateEnum.IDLE)
                {
                    Result = true;
                }
                else
                {
                    Result = false;
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

    [Serializable]
    public class IsLotRunningState : QueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ILotOPModule LotOPModule = this.LotOPModule();

                if (LotOPModule.ModuleState.State == ModuleStateEnum.RUNNING ||
                    LotOPModule.ModuleState.State == ModuleStateEnum.SUSPENDED ||
                    LotOPModule.ModuleState.State == ModuleStateEnum.PENDING)
                {
                    Result = true;
                }
                else
                {
                    Result = false;
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

    [Serializable]
    public class IsLotPausedState : QueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ILotOPModule LotOPModule = this.LotOPModule();

                if (LotOPModule.ModuleState.State == ModuleStateEnum.PAUSED ||
                    LotOPModule.ModuleState.State == ModuleStateEnum.PAUSING)
                {
                    Result = true;
                }
                else
                {
                    Result = false;
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


    [Serializable]
    public class IsProbingModuleRunningState : QueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                IProbingModule ProbingModule = this.ProbingModule();
                if (ProbingModule.ModuleState.State == ModuleStateEnum.RUNNING ||
                    ProbingModule.ModuleState.State == ModuleStateEnum.SUSPENDED)
                {
                    retVal = EventCodeEnum.NONE;
                    Result = true;
                }
                else
                {
                    Result = false;
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
    public class IsProbingModuleSuspendedState : QueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                IProbingModule ProbingModule = this.ProbingModule();
                if (ProbingModule.ModuleState.State == ModuleStateEnum.SUSPENDED)
                {
                    retVal = EventCodeEnum.NONE;
                    Result = true;
                }
                else
                {
                    Result = false;
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
    public class IsLotErrorState : QueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ILotOPModule LotOPModule = this.LotOPModule();

                if (LotOPModule.ModuleState.State == ModuleStateEnum.ERROR)
                {
                    Result = true;
                }
                else
                {
                    Result = false;
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

    [Serializable]
    public class IsCardChangeGoingOn : QueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                WaferTransferProcStateEnum state = this.WaferTransferModule().GetProcModuleState();
                if (state != WaferTransferProcStateEnum.IDLE) 
                {
                    WaferTransferTypeEnum type = this.WaferTransferModule().GetProcModuleTransferType();
                    if (state == WaferTransferProcStateEnum.RUNNING && type == WaferTransferTypeEnum.CardLoading)
                    {
                        retVal = EventCodeEnum.NONE;
                        Result = true;
                    }
                    else
                    {
                        retVal = EventCodeEnum.NONE;
                        Result = false;
                    }
                }
                else
                {
                    Result = false;
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
    public class IsZUP : QueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                IProbingModule ProbingModule = this.ProbingModule();

                if (ProbingModule.ProbingStateEnum == EnumProbingState.ZUP || ProbingModule.ProbingStateEnum == EnumProbingState.ZUPDWELL)
                {
                    Result = true;
                }
                else
                {
                    Result = false;
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

    [Serializable]
    public class IsZDN : QueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                IProbingModule ProbingModule = this.ProbingModule();

                if (ProbingModule.ProbingStateEnum == EnumProbingState.ZDN || ProbingModule.ProbingStateEnum == EnumProbingState.ZDOWNDELL)
                {
                    Result = true;
                }
                else
                {
                    Result = false;
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

    [Serializable]
    public class IsRemainProbingSequecne : QueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                IProbingSequenceModule probingseqmodule = this.ProbingSequenceModule();

                if (probingseqmodule.GetProbingSequenceState() == ProbingSequenceStateEnum.NOSEQ)
                {
                    Result = false;
                }
                else
                {
                    // TODO : 다른 스테이트들...?

                    Result = true;
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

    [Serializable]
    public class IsWafer6Inch : QueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                double wafersize = this.StageSupervisor().WaferObject.GetPhysInfo().WaferSize_um.Value;

                if (wafersize == 150000)
                {
                    Result = true;
                }
                else
                {
                    Result = false;
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

    [Serializable]
    public class IsWafer8Inch : QueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                double wafersize = this.StageSupervisor().WaferObject.GetPhysInfo().WaferSize_um.Value;

                if (wafersize == 200000)
                {
                    Result = true;
                }
                else
                {
                    Result = false;
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

    [Serializable]
    public class IsWafer12Inch : QueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                double wafersize = this.StageSupervisor().WaferObject.GetPhysInfo().WaferSize_um.Value;

                if (wafersize == 300000)
                {
                    Result = true;
                }
                else
                {
                    Result = false;
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

    [Serializable]
    public class IsPMIPass : QueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                IPMIModule pMIModule = this.PMIModule();

                if(pMIModule != null)
                {
                    bool pmiret = pMIModule.GetPMIResult();

                    if (pmiret == true)
                    {
                        Result = true;
                    }
                    else
                    {
                        Result = false;
                    }

                    retVal = EventCodeEnum.NONE;
                }
                else
                {

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
    public class IsPMIFail : QueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                IPMIModule pMIModule = this.PMIModule();

                if (pMIModule != null)
                {
                    bool pmiret = pMIModule.GetPMIResult();

                    if (pmiret == false)
                    {
                        Result = true;
                    }
                    else
                    {
                        Result = false;
                    }

                    retVal = EventCodeEnum.NONE;
                }
                else
                {

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
    public class EqualByArg : QueryData
    {
        public object ComaparisonValue { get; set; }

        public EqualByArg(object comparisonvalue)
        {
            this.ComaparisonValue = comparisonvalue;
        }

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            Result = false;

            try
            {
                if (Argument != null && ComaparisonValue != null)
                {
                    double val1, val2;

                    bool IsParse1 = false;
                    bool IsParse2 = false;

                    string ArgStr = this.Argument.ToString();
                    string CompareStr = ComaparisonValue.ToString();

                    IsParse1 = double.TryParse(ArgStr, out val1);
                    IsParse2 = double.TryParse(CompareStr, out val2);

                    if (IsParse1 == true && IsParse2 == true)
                    {
                        if (val1 == val2)
                        {
                            Result = true;
                        }
                        else
                        {
                            Result = false;
                        }

                        retVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        // TODO : 로직 정리 

                        if (Argument.GetType().IsEnum && ComaparisonValue.GetType().IsEnum)
                        {
                            Enum Eval1, Eval2;

                            Eval1 = (Enum)Argument;
                            Eval2 = (Enum)ComaparisonValue;

                            if (Eval1.Equals(Eval2))
                            {
                                Result = true;
                            }
                            else
                            {
                                // TODO : string 동일 시, 허용(namespace가 다른 경우)
                                if (Eval1.ToString() == Eval2.ToString())
                                {
                                    Result = true;
                                }
                                else
                                {
                                    Result = false;
                                }
                            }
                        }
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
    public class HandleFailedRequest : QueryData
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = (EventCodeEnum)Result;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }
}
