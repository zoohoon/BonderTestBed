using System;
using LogModule;
using NotifyEventModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.BinData;
using ProberInterfaces.Command.Internal;

namespace RequestCore.ActionPack.GPIB
{
    [Serializable]
    public abstract class GpibAction : Action
    {
        public GpibAction() { }
    }

    [Serializable]
    public class GpibException : Action
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

                string recvData = this.Argument.ToString().ToUpper();

                if (recvData.Length >= 10)
                {
                    string DWId = recvData.Replace("DW", "").Substring(0, 6);

                    int SeparatorIndex = recvData.IndexOf("-");
                    string setvalue = string.Empty;
                    IElement elment = null;

                    if (SeparatorIndex != -1)
                    {
                        setvalue = recvData.Substring(SeparatorIndex + 1, recvData.Length);
                        int id = Convert.ToInt32(DWId);
                        elment = this.ParamManager().GetElement(id);

                        if (elment != null)
                        {
                            // TODO : Element의 타입과 Set하려고하는 값의 Type이 문제가 없는지 ?
                            // 내부적으로 처리하는지 확인 먼저

                            //isValid = elment.SetValue(setvalue, isNeedValidation: true, source: this.GPIB());
                            isValid = elment.SetValue(setvalue);//TODO: 위 코드를 안넣는게 나은지 검토 필요.
                        }
                    }
                }

                if (isValid == EventCodeEnum.NONE || isValid == EventCodeEnum.PARAM_SET_EQUAL_VALUE)
                {
                    // STB 98
                    this.EventManager().RaisingEvent(typeof(PassDWCommandEvent).FullName);
                }
                else
                {
                    // STB 99
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
    public class SetDataUsingUWID : Action
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                EventCodeEnum isValid = EventCodeEnum.UNDEFINED;
                string recvData = this.Argument.ToString().ToUpper();

                if (recvData.Length > 4)
                {
                    string UWId = recvData.Replace("UW", "").Substring(0, 4);
                    string setvalue = recvData.Substring(5, recvData.Length - 5);
                    int id = Convert.ToInt32(UWId);
                    IElement elment = this.ParamManager().GetElement(id);

                    if (elment != null)
                    {
                        //isValid = elment.SetValue(setvalue, isNeedValidation: true, source: this.GPIB());
                        isValid = elment.SetValue(setvalue);//TODO: 위 코드를 안넣는게 나은지 검토 필요.
                    }
                }

                if (isValid == EventCodeEnum.NONE || isValid == EventCodeEnum.PARAM_SET_EQUAL_VALUE)
                {
                    // STB 98
                    this.EventManager().RaisingEvent(typeof(PassDWCommandEvent).FullName);
                }
                else
                {
                    // STB 99
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
    public class GetCalcualtePassFailYield : Action
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                IGPIB gpib = this.GPIB();
                IGPIBSysParam GpibSysParam = gpib.GPIBSysParam_IParam as IGPIBSysParam;

                bool commandResult = false;
                string binCode = string.Empty;
                this.Result = null;

                if (GpibSysParam == null || this.Argument == null)
                {
                    throw new Exception("Not enough 'GpibSysParam' or 'Bin information'.");
                }

                binCode = this.Argument.ToString();
                BinAnalysisDataArray binAnalysisData = gpib.AnalyzeBin(binCode);

                //////////////////////////////
                commandResult = this.CommandManager().SetCommand<ISetBinAnalysisData>(this, new BinCommandParam() { Param = binAnalysisData });
                //////////////////////////////
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class SetDeviceName : Action
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
    public class SetOverDrive : Action
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
    public class TestStart : Action
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
    public class ZIncrement : Action
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
    public class ZDecrement : Action
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
    public class PosDieToDesiredPos : Action
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
    public class BuzzerOnAndStopProber : Action
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
    public class SetHotCuckTemp : Action
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
    public class NeedleCleaning : Action
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
    public class IdxControlForFailInput : Action
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
}
