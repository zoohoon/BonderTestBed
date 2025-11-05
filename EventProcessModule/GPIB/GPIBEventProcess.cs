using Command.GPIB;
using LogModule;
using ProberInterfaces;
using ProberInterfaces.Event;
using ProberInterfaces.Event.EventProcess;
using System;

namespace EventProcessModule.GPIB
{
    [Serializable]
    public abstract class GPIBEventProcessBase : EventProcessBase
    {
    }

    [Serializable]
    public class GPIBEventProc_StbCmdSetter : GPIBEventProcessBase
    {
        public override void EventNotify(object sender, ProbeEventArgs e)
        {
            try
            {
                if (this.GPIB().GetGPIBEnable() == EnumGpibEnable.ENABLE)
                {
                    LoggerManager.Debug("[BEGIN] @@@GPIB Event Process@@@");

                    bool resultVal = false;
                    string resultStr = null;
                    
                    
                    IGpibEventParam eventParam = Parameter as IGpibEventParam;

                    if (eventParam != null)
                    {
                        resultVal = this.CommandManager().SetCommand(this, eventParam.CommandName, new GpibSrqParam() { StbNumber = eventParam.StbNumber });

                        if (resultVal == false)
                        {
                            LoggerManager.Debug($"GPIB Event Process : SetCommand= {resultVal}");
                        }
                        else
                        {
                            LoggerManager.Debug($"GPIB Event Process : SetCommand= {resultVal}");
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"GPIB Event Process : eventParam is null ");

                        if (Parameter == null)
                        {
                            LoggerManager.Debug($"GPIB Event Process : Parameter is null ");
                        }
                        else
                        {
                            LoggerManager.Debug($"GPIB Event Process : Parameter= " + Parameter.ToString());
                        }
                        resultVal = false;
                    }
                    

                    if (resultVal == true) // Sucess
                    {
                        resultStr = "Sucess";
                    }
                    else
                    {
                        resultStr = "Fail";
                    }
                    
                    LoggerManager.Debug($"[ END ] @@@GPIB Event Process@@@ : {resultStr}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
