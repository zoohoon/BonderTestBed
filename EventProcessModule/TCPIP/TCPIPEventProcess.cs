using Command.TCPIP;
using LogModule;
using ProberInterfaces;
using ProberInterfaces.Event;
using ProberInterfaces.Event.EventProcess;
using System;

namespace EventProcessModule.TCPIP
{
    [Serializable]
    public abstract class TCPIPEventProcessBase : EventProcessBase
    {
    }

    [Serializable]
    public class TCPIPEventProc_CmdSetter : TCPIPEventProcessBase
    {
        public override void EventNotify(object sender, ProbeEventArgs e)
        {
            try
            {
                EnumTCPIPEnable IsEnable = EnumTCPIPEnable.DISABLE;

                if (this.TCPIPModule() != null)
                {
                    IsEnable = this.TCPIPModule().GetTCPIPOnOff();
                }

                if (IsEnable == EnumTCPIPEnable.ENABLE)
                {
                    LoggerManager.Debug("[TCPIPEventProc_CmdSetter] EventNotify() : START");

                    bool retval = false;

                    ITCPIPEventParam eventParam = Parameter as ITCPIPEventParam;

                    if (eventParam != null)
                    {
                        TCPIPResponseParam tCPIPResponseParam = new TCPIPResponseParam();
                        tCPIPResponseParam.response = eventParam.Response;
                        tCPIPResponseParam.response.Argument = e?.Parameter;

                        retval = this.CommandManager().SetCommand(this, eventParam.CommandName, tCPIPResponseParam);

                        LoggerManager.Debug($"[TCPIPEventProc_CmdSetter] EventNotify() : SetCommand= {retval}");
                    }
                    else
                    {
                        LoggerManager.Debug($"[TCPIPEventProc_CmdSetter] EventNotify() : eventParam is null ");

                        if (Parameter == null)
                        {
                            LoggerManager.Debug($"[TCPIPEventProc_CmdSetter] EventNotify() : Parameter is null ");
                        }
                        else
                        {
                            LoggerManager.Debug($"[TCPIPEventProc_CmdSetter] EventNotify() : Parameter= " + Parameter.ToString());
                        }

                        retval = false;
                    }
                    
                    LoggerManager.Debug("[TCPIPEventProc_CmdSetter] EventNotify() : END");
                }
                else
                {
                    LoggerManager.Error($"[TCPIPEventProc_StbCmdSetter], EventNotify() : EnumTCPIPEnable = {IsEnable}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
