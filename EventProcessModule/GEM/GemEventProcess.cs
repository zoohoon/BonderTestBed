using ProberInterfaces;
using ProberInterfaces.Event;
using ProberInterfaces.Event.EventProcess;
using System;

namespace EventProcessModule.GEM
{
    using LogModule;
    using ProberErrorCode;

    public abstract class GemEventProcessBase : EventProcessBase
    {
    }

    public class GemEventProc_EventMessageSet : GemEventProcessBase
    {
        public override void EventNotify(object sender, ProbeEventArgs e)
        {
            try
            {
                if(this.GEMModule().GemEnable())
                {
                    if (Enable == false)
                    {
                        return;
                    }

                    if(ConditionChecker != null)
                    {
                        string checkFailReason = "";
                        bool conditionRetVal = ConditionChecker.DoCkeck(ref checkFailReason);
                        if(conditionRetVal == false)
                        {
                            LoggerManager.Debug($"[GetEventProcessBase] Event : {sender.ToString()}, ConditionChecker : {ConditionChecker.ToString()}, fail reason : {checkFailReason}");
                            return;
                        }
                    }

                    PIVInfo pivInfo = null;
                    if(e != null)
                    {
                        if (e.Parameter != null)
                        {
                            pivInfo = e.Parameter as PIVInfo;
                        }
                    }


                    LoggerManager.Debug($"[GetEventProcessBase][{OwnerModuleName} HANDLER] Start...");

                    long eventNum = 0;
                    bool typeCheck = Parameter is long;                    
                    long resultVal = -1;

                    if (typeCheck != false)
                    {
                        eventNum = Convert.ToInt64(Parameter);
                        LoggerManager.Debug($"[GetEventProcessBase][{OwnerModuleName} HANDLER] Start eventid:{eventNum} ");
                        EventCodeEnum retVal = EventCodeEnum.NONE;
                        //object lockobj = this.GEMModule().GemCommManager.GetLockObj();
                        object lockobj = this.GEMModule().GemCommManager.GetProcessorLockObj();

                        lock (lockobj)
                        {
                            if(sender is IFoupNotifyEvent)
                            {
                                retVal = (sender as IFoupNotifyEvent).CheckFoupMode(pivInfo);

                            }
                            if (retVal == EventCodeEnum.NONE)
                            {
                                if (sender is IGemCommand)
                                {
                                    retVal = (sender as IGemCommand).PreCheck();
                                    if (retVal == EventCodeEnum.NONE)
                                    {
                                        retVal = (sender as IGemCommand).SetPIV(pivInfo);
                                    }
                                }
                                else
                                {
                                    retVal = EventCodeEnum.NONE;
                                }

                                if (retVal == EventCodeEnum.NONE)
                                {
                                    long? tmpResult = this.GEMModule()?.SetEvent(eventNum);
                                    resultVal = tmpResult == null ? -1 : (long)tmpResult;

                                    if (sender is IGemCommand)
                                    {
                                        retVal = (sender as IGemCommand).AfterSetPIV(pivInfo);
                                    }
                                }
                                else
                                {
                                    LoggerManager.Debug($"[Gem Event Notify Fail] event : {sender.ToString()}, eventid:{eventNum} reason : {retVal}");
                                }
                            }
                            else
                            {
                                LoggerManager.Debug($"[Gem Event Notify Fail] event : {sender.ToString()}, eventid:{eventNum} reason : {retVal}");
                            }
                        }
                    }
                    else
                    {
                    }

                    if (0 < resultVal) // Sucess
                    {
                       //resultStr = "Sucess";
                    }
                    else
                    {
                        //resultStr = "Fail";
                    }
                    LoggerManager.Debug($"[GetEventProcessBase][{OwnerModuleName} HANDLER] eventid:{eventNum} End...");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
