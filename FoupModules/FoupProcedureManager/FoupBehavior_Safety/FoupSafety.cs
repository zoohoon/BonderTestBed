using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces.Foup;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using LogModule;
using ProberInterfaces;

namespace FoupProcedureManagerProject
{

    [Serializable]
    public class FoupSafeties : List<FoupSafety>
    {
        public FoupSafeties()
        {

        }
    }

    [Serializable]
    public abstract class FoupSafety : IFoupBehavior
    {
        public FoupSafety()
        {

        }
        [XmlIgnore, JsonIgnore]
        public IFoupModule FoupModule { get; set; }
        [XmlIgnore, JsonIgnore]
        public IFoupIOStates FoupIOManager { get; set; }
        [XmlIgnore, JsonIgnore]
        public FoupSafetyStateEnum State { get; set; }

        public abstract EventCodeEnum Run();

        public void StateTransition(EventCodeEnum result)
        {
            try
            {
                // 0 : 정상
                // -1 : Error
                // -2 : TimeOut

                if (result == EventCodeEnum.FOUP_CHECK_IO_DONE)
                {
                    State = FoupSafetyStateEnum.DONE;
                }
                else if (result == EventCodeEnum.FOUP_CHECK_IO_ERROR)
                {
                    State = FoupSafetyStateEnum.ERROR;
                }
                else if (result == EventCodeEnum.IO_TIMEOUT_ERROR)
                {
                    State = FoupSafetyStateEnum.TIMEOUT;
                }
                else
                {
                    State = FoupSafetyStateEnum.UNDEFINED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

    [Serializable]
    public class I_WAFER_OUT : FoupSafety
    {
        public I_WAFER_OUT()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                int Val = -1;

                if(Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    State = FoupSafetyStateEnum.DONE;
                    return EventCodeEnum.NONE;
                }

                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_WAFER_OUTs[FoupModule.FoupIndex], false, 100, 800);
                }
                else if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_WAFER_OUT, false, 100, 1000);
                }

                if (Val == 0 || Val == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (Val == -1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }
                else if (Val == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }

                StateTransition(retVal);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class I_FO_UP : FoupSafety
    {
        public I_FO_UP()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                int Val = -1;

                Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_FO_UP, true, 10, 300);

                if (Val == 0 || Val == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (Val == -1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }
                else if (Val == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }

                StateTransition(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class I_FO_DOWN : FoupSafety
    {
        public I_FO_DOWN()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                int Val = -1;

                Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_FO_DOWN, true, 10, 300);

                if (Val == 0 || Val == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (Val == -1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }
                else if (Val == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }

                StateTransition(retVal);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class I_LOAD_SWITCH : FoupSafety
    {
        public I_LOAD_SWITCH()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                int Val = -1;

                Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_LOAD_SWITCH, true, 10, 300);

                if (Val == 0 || Val == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (Val == -1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }
                else if (Val == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }

                StateTransition(retVal);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class I_UNLOAD_SWITCH : FoupSafety
    {
        public I_UNLOAD_SWITCH()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                int Val = -1;

                Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_UNLOAD_SWITCH, true, 10, 300);

                if (Val == 0 || Val == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (Val == -1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }
                else if (Val == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }

                StateTransition(retVal);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class I_COVER_DOOR_OPEN : FoupSafety
    {
        public I_COVER_DOOR_OPEN()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                int Val = -1;

                Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_COVER_DOOR_OPEN, true, 10, 300);

                if (Val == 0 || Val == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (Val == -1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }
                else if (Val == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }

                StateTransition(retVal);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    [Serializable]
    public class I_COVER_DOOR_CLOSE : FoupSafety
    {
        public I_COVER_DOOR_CLOSE()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                int Val = -1;

                Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_COVER_DOOR_CLOSE, true, 10, 300);

                if (Val == 0 || Val == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (Val == -1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }
                else if (Val == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }

                StateTransition(retVal);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    [Serializable]
    public class I_FO_VAC_ON : FoupSafety
    {
        public I_FO_VAC_ON()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                int Val = -1;
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    State = FoupSafetyStateEnum.DONE;
                    return EventCodeEnum.NONE;
                }
                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_CST_CoverVacuums[FoupModule.FoupIndex], true, 100, 800);
                }
                else if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_FO_VAC, true, 10, 300);
                }

                if (Val == 0 || Val == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (Val == -1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }
                else if (Val == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }

                StateTransition(retVal);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    [Serializable]
    public class I_FO_VAC_OFF : FoupSafety
    {
        public I_FO_VAC_OFF()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                int Val = -1;
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    State = FoupSafetyStateEnum.DONE;
                    return EventCodeEnum.NONE;
                }
                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_CST_CoverVacuums[FoupModule.FoupIndex], false, 100, 800);
                }
                else if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_FO_VAC, false, 10, 300);
                }

                if (Val == 0 || Val == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (Val == -1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }
                else if (Val == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }

                StateTransition(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    [Serializable]
    public class I_C12IN_POSB : FoupSafety
    {
        public I_C12IN_POSB()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                int Val = -1;

                Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_C12IN_POSB, true, 10, 300);

                if (Val == 0 || Val == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (Val == -1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }
                else if (Val == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }

                StateTransition(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    [Serializable]
    public class I_CP_IN : FoupSafety
    {
        public I_CP_IN()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                int Val = -1;

                Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_CP_IN, true, 10, 300);

                if (Val == 0 || Val == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (Val == -1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }
                else if (Val == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }

                StateTransition(retVal);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    [Serializable]
    public class I_CP_OUT : FoupSafety
    {
        public I_CP_OUT()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                int Val = -1;

                Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_CP_OUT, true, 10, 300);

                if (Val == 0 || Val == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (Val == -1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }
                else if (Val == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }

                StateTransition(retVal);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    [Serializable]
    public class I_CP_40_IN : FoupSafety
    {
        public I_CP_40_IN()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                int Val = -1;

                Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_CP_40_IN, true, 10, 300);

                if (Val == 0 || Val == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (Val == -1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }
                else if (Val == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }

                StateTransition(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    [Serializable]
    public class I_CP_40_OUT : FoupSafety
    {
        public I_CP_40_OUT()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                int Val = -1;

                Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_CP_40_OUT, true, 10, 300);

                if (Val == 0 || Val == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (Val == -1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }
                else if (Val == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }

                StateTransition(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    [Serializable]
    public class I_FO_OPEN : FoupSafety
    {
        public I_FO_OPEN()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                int Val = -1;

                Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_FO_OPEN, true, 10, 300);

                if (Val == 0 || Val == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (Val == -1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }
                else if (Val == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }

                StateTransition(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    [Serializable]
    public class I_FO_CLOSE : FoupSafety
    {
        public I_FO_CLOSE()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                int Val = -1;

                Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_FO_CLOSE, true, 10, 300);

                if (Val == 0 || Val == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (Val == -1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }
                else if (Val == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }

                StateTransition(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    [Serializable]
    public class I_FO_COVER : FoupSafety
    {
        public I_FO_COVER()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                int Val = -1;

                Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_COVER, true, 10, 300);

                if (Val == 0 || Val == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (Val == -1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }
                else if (Val == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }

                StateTransition(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    [Serializable]
    public class I_6IN_8IN_PRESENCE01 : FoupSafety
    {
        public I_6IN_8IN_PRESENCE01()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                int Val = -1;
                if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE1, true, 10, 300);
                }
                else
                {
                }

                if (Val == 0 || Val == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (Val == -1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }
                else if (Val == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }

                StateTransition(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    [Serializable]
    public class I_C6IN_PLACEMENT : FoupSafety
    {
        public I_C6IN_PLACEMENT()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                int Val = -1;

                Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_C6IN_PLACEMENT, true, 10, 300);

                if (Val == 0 || Val == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (Val == -1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }
                else if (Val == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }

                StateTransition(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    [Serializable]
    public class I_C8IN_PLACEMENT : FoupSafety
    {
        public I_C8IN_PLACEMENT()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                int Val = -1;

                Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_C8IN_PLACEMENT, true, 10, 300);

                if (Val == 0 || Val == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (Val == -1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }
                else if (Val == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }
                StateTransition(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    [Serializable]
    public class I_C6IN_C8IN_NPLACEMENT : FoupSafety
    {
        public I_C6IN_C8IN_NPLACEMENT()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                int Val = -1;

                Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_C6IN_C8IN_NPLACEMENT, true, 10, 300);

                if (Val == 0 || Val == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (Val == -1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }
                else if (Val == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }
                StateTransition(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    [Serializable]
    public class I_C12IN_PRESENCE1 : FoupSafety
    {
        public I_C12IN_PRESENCE1()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                int Val = -1;
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    State = FoupSafetyStateEnum.DONE;
                    return EventCodeEnum.NONE;
                }
                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    //12Inch
                    Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_CST12_PRESs[FoupModule.FoupIndex], true, 100, 800);
                    if(Val == 1)
                    {
                        Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_CST12_PRES2s[FoupModule.FoupIndex], true, 100, 800);
                    }

                }
                else if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_C12IN_PRESENCE1, true, 300, 3000);
                    if (Val == 1 || Val == 0)
                    {
                        Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_C12IN_PRESENCE2, true, 300, 3000);
                    }
                }

                if (Val == 0 || Val == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (Val == -1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }
                else if (Val == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }

                StateTransition(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    [Serializable]
    public class I_C12IN_PLACEMENT : FoupSafety
    {
        public I_C12IN_PLACEMENT()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                int Val = -1;
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    State = FoupSafetyStateEnum.DONE;
                    return EventCodeEnum.NONE;
                }
                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    //Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_CST_LOCK12s[FoupModule.FoupIndex], true, 100, 800); 
                }
                else if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_C12IN_PLACEMENT, true, 10, 300);
                }

                if (Val == 0 || Val == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (Val == -1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }
                else if (Val == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }

                StateTransition(retVal);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    [Serializable]
    public class I_C12IN_NPLACEMENT : FoupSafety
    {
        public I_C12IN_NPLACEMENT()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                int Val = -1;

                Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_C12IN_NPLACEMENT, true, 10, 300);

                if (Val == 0 || Val == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (Val == -1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }
                else if (Val == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }

                StateTransition(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    [Serializable]
    public class I_C12IN_POSA : FoupSafety
    {
        public I_C12IN_POSA()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                int Val = -1;

                Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_C12IN_POSA, true, 10, 300);

                if (Val == 0 || Val == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (Val == -1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }
                else if (Val == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }
                StateTransition(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    [Serializable]
    public class I_FD12POS : FoupSafety
    {
        public I_FD12POS()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                int Val = -1;

                if (Val == 0 || Val == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (Val == -1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }
                else if (Val == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }
                StateTransition(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    [Serializable]
    public class I_FD12CASSETTEON : FoupSafety
    {
        public I_FD12CASSETTEON()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                int Val = -1;

                if (Val == 0 || Val == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (Val == -1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }
                else if (Val == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }

                StateTransition(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    [Serializable]
    public class I_FO_LOCK12 : FoupSafety
    {
        public I_FO_LOCK12()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                int openIO = -1;
                int closeIO = -1;
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    State = FoupSafetyStateEnum.DONE;
                    return EventCodeEnum.NONE;
                }
                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    openIO = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_CST_LOCK12s[FoupModule.FoupIndex], true, 100, 800);
                    closeIO = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_CST_UNLOCK12s[FoupModule.FoupIndex], false, 100, 800);
                }
                else if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                }

                if (openIO == 1 && closeIO == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (openIO == -2 || closeIO == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }
                else
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }

                StateTransition(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    [Serializable]
    public class I_FO_UNLOCK12 : FoupSafety
    {
        public I_FO_UNLOCK12()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                int openIO = -1;
                int closeIO = -1;
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    State = FoupSafetyStateEnum.DONE;
                    return EventCodeEnum.NONE;
                }
                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    openIO = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_CST_UNLOCK12s[FoupModule.FoupIndex], true, 100, 800);
                    closeIO = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_CST_LOCK12s[FoupModule.FoupIndex], false, 100, 800);
                }
                else if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                }

                if (openIO == 1 && closeIO == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (openIO == -2 || closeIO == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }
                else
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }

                StateTransition(retVal);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    [Serializable]
    public class I_DP_IN : FoupSafety
    {
        public I_DP_IN()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                int openIO = -1;
                int closeIO = -1;
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    State = FoupSafetyStateEnum.DONE;
                    return EventCodeEnum.NONE;
                }
                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    openIO = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_DP_INs[FoupModule.FoupIndex], true, 100, 800);
                    closeIO = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_DP_OUTs[FoupModule.FoupIndex], false, 100, 800);
                }
                else if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    //Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_CP_IN, false, 10, 300);
                }

                if (openIO == 1 && closeIO == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (openIO == -2 || closeIO == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }
                else
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }

                StateTransition(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    [Serializable]
    public class I_DP_OUT : FoupSafety
    {
        public I_DP_OUT()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                int openIO = -1;
                int closeIO = -1;

                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    State = FoupSafetyStateEnum.DONE;
                    return EventCodeEnum.NONE;
                }

                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    openIO = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_DP_OUTs[FoupModule.FoupIndex], true, 100, 800);
                    closeIO = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_DP_INs[FoupModule.FoupIndex], false, 100, 800);
                }
                else if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    //Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_CP_OUT, false, 10, 300);
                }

                if (openIO == 1 && closeIO == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (openIO == -2 || closeIO == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }
                else
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }

                StateTransition(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    [Serializable]
    public class I_COVER_LOCK : FoupSafety
    {
        public I_COVER_LOCK()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                int openIO = -1;
                int closeIO = -1;
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    State = FoupSafetyStateEnum.DONE;
                    return EventCodeEnum.NONE;
                }

                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    openIO = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_COVER_LOCKs[FoupModule.FoupIndex], true, 100, 800);
                    closeIO = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_COVER_UNLOCKs[FoupModule.FoupIndex], false, 100, 800);
                }
                else if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    openIO = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_FOUP_COVER_LOCK, true, 10, 300);
                    closeIO = 1;
                }

                if ((openIO == 1 || openIO == 0)&& closeIO == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (openIO == -2 || closeIO == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }
                else
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }

                StateTransition(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    [Serializable]
    public class I_COVER_UNLOCK : FoupSafety
    {
        public I_COVER_UNLOCK()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                int openIO = -1;
                int closeIO = -1;
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    State = FoupSafetyStateEnum.DONE;
                    return EventCodeEnum.NONE;
                }

                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    openIO = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_COVER_UNLOCKs[FoupModule.FoupIndex], true, 100, 800);
                    closeIO = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_COVER_LOCKs[FoupModule.FoupIndex], false, 100, 800);
                }
                else if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    openIO = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_FOUP_COVER_UNLOCK, true, 10, 300);
                    closeIO = 1;
                }

                if ((openIO == 1 || openIO == 0) && closeIO == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (openIO == -2 || closeIO == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }
                else
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }

                StateTransition(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    [Serializable]
    public class I_COVER_UP : FoupSafety
    {
        public I_COVER_UP()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                int openIO = -1;
                int closeIO = -1;
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    State = FoupSafetyStateEnum.DONE;
                    return EventCodeEnum.NONE;
                }

                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    openIO = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_COVER_UPs[FoupModule.FoupIndex], true, 100, 800);
                    closeIO = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_COVER_DOWNs[FoupModule.FoupIndex], false, 100, 800);
                }
                else if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    openIO = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_FO_UP, true, 10, 300);
                    closeIO = 1;
                }

                if ((openIO == 1 || openIO == 0) && closeIO == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (openIO == -2 || closeIO == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }
                else
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }

                StateTransition(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    [Serializable]
    public class I_COVER_DOWN : FoupSafety
    {
        public I_COVER_DOWN()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                int openIO = -1;
                int closeIO = -1;
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    State = FoupSafetyStateEnum.DONE;
                    return EventCodeEnum.NONE;
                }

                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    openIO = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_COVER_DOWNs[FoupModule.FoupIndex], true, 100, 800);
                    closeIO = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_COVER_UPs[FoupModule.FoupIndex], false, 100, 800);
                }
                else if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    openIO = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_FO_DOWN, true, 10, 300);
                    closeIO = 1;
                }

                if ((openIO == 1 || openIO == 0) && closeIO == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (openIO == -2 || closeIO == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }
                else
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }

                StateTransition(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    [Serializable]
    public class I_COVER_CLOSE : FoupSafety
    {
        public I_COVER_CLOSE()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                int openIO = -1;
                int closeIO = -1;
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    State = FoupSafetyStateEnum.DONE;
                    return EventCodeEnum.NONE;
                }

                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    openIO = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_COVER_CLOSEs[FoupModule.FoupIndex], true, 100, 800);
                    closeIO = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_COVER_UPs[FoupModule.FoupIndex], false, 100, 800);
                }
                else if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    openIO = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_FO_CLOSE, true, 10, 300);
                    closeIO = 1;
                }

                if ((openIO == 1 || openIO == 0) && closeIO == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (openIO == -2 || closeIO == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }
                else
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }

                StateTransition(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class I_FO_EXIST : FoupSafety
    {
        public I_FO_EXIST()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                int Val = -1;

                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    State = FoupSafetyStateEnum.DONE;
                    return EventCodeEnum.NONE;
                }

                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_CST_Exists[FoupModule.FoupIndex], true, 100, 800);
                }
                else if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    //signle exist sensor 정의 필요
                }

                if (Val == 0 || Val == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (Val == -1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }
                else if (Val == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }

                StateTransition(retVal);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    [Serializable]
    public class I_FO_Mapping_Out : FoupSafety
    {
        public I_FO_Mapping_Out()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                int Val = -1;

                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    State = FoupSafetyStateEnum.DONE;
                    return EventCodeEnum.NONE;
                }

                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    Val = FoupIOManager.MonitorForIO(FoupIOManager.IOMap.Inputs.DI_CST_MappingOuts[FoupModule.FoupIndex], true, 100, 800);
                }
                else if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    //signle 
                }

                if (Val == 0 || Val == 1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_DONE;
                }
                else if (Val == -1)
                {
                    retVal = EventCodeEnum.FOUP_CHECK_IO_ERROR;
                }
                else if(Val == -2)
                {
                    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                }
                StateTransition(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
}
