using Autofac;
using LoaderBase;
using LoaderController.GPController;
using LogModule;
using Newtonsoft.Json;
using NotifyEventModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Event;
using ProberInterfaces.Event.EventProcess;
using ProberInterfaces.Foup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalTowerModule
{
    // 해당 이벤트가 발생했을 때 봐야하는 조건이 있다면 여기서 본다.
    // 본인이 리스트에 추가될 조건에 부합하면, 
    // signaltower manager -> controller 접근             
    [Serializable]
    public class StageEventProcessor : EventProcessBase
    {
        public override void EventNotify(object sender, ProbeEventArgs e)
        {
            try
            {
                int idx = 0;
                SignalTowerEventParam eventParam = new SignalTowerEventParam();
                ISignalTowerEventParam param = Parameter as ISignalTowerEventParam;

                if (eventParam != null)
                {
                    if (e != null)
                    {
                        if (e.Parameter != null)
                        {
                            if (e.Parameter is int)
                            {
                                idx = Convert.ToInt32(e.Parameter);
                                eventParam.CellIdx = idx;
                            }
                            else if (e.Parameter is PIVInfo)
                            {
                                PIVInfo pivInfo = null;
                                pivInfo = e.Parameter as PIVInfo;
                                eventParam.CellIdx = pivInfo.StageNumber;
                            }
                            eventParam.Guid = param.Guid;
                            eventParam.SignalDefineInformations = param.SignalDefineInformations;
                            eventParam.EventName = sender.ToString();
                            LoggerManager.Debug($"[TEST_ST] SignalTower Event Process : {sender.ToString()}");
                            this.SignalTowerManager().UpdateEventObjList(eventParam, Enable);
                        }
                    }
                }
                else
                {
                    LoggerManager.Debug($"SignalTower Event Process : eventParam is null ");
                    if (Parameter == null)
                    {
                        LoggerManager.Debug($"SignalTower Event Process : Parameter is null ");
                    }
                    else
                    {
                        LoggerManager.Debug($"SignalTower Event Process : Parameter= " + Parameter.ToString());
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    [Serializable]
    public class FoupEventProcessor : EventProcessBase
    {
        public override void EventNotify(object sender, ProbeEventArgs e)
        {
            try
            {
                int idx = 0;
                SignalTowerEventParam eventParam = new SignalTowerEventParam();
                ISignalTowerEventParam param = Parameter as ISignalTowerEventParam;

                if (eventParam != null)
                {
                    if (e != null)
                    {
                        if (e.Parameter != null)
                        {
                            if (e.Parameter is int)
                            {
                                idx = Convert.ToInt32(e.Parameter);
                                eventParam.FoupIdx = idx;
                            }
                            else if (e.Parameter is PIVInfo)
                            {
                                PIVInfo pivInfo = null;
                                pivInfo = e.Parameter as PIVInfo;
                                eventParam.FoupIdx = pivInfo.FoupNumber;
                            }
                            eventParam.Guid = param.Guid;
                            eventParam.SignalDefineInformations = param.SignalDefineInformations;
                            eventParam.EventName = sender.ToString();
                            LoggerManager.Debug($"[TEST_ST] SignalTower Event Process : {sender.ToString()} {eventParam.FoupIdx}");
                            this.SignalTowerManager().UpdateEventObjList(eventParam, Enable);
                        }
                    }
                }
                else
                {
                    LoggerManager.Debug($"SignalTower Event Process : eventParam is null ");
                    if (Parameter == null)
                    {
                        LoggerManager.Debug($"SignalTower Event Process : Parameter is null ");
                    }
                    else
                    {
                        LoggerManager.Debug($"SignalTower Event Process : Parameter= " + Parameter.ToString());
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    [Serializable]
    public class LoaderEventProcessor : EventProcessBase
    {
        public override void EventNotify(object sender, ProbeEventArgs e)
        {
            try
            {
                SignalTowerEventParam eventParam = new SignalTowerEventParam();
                ISignalTowerEventParam param = Parameter as ISignalTowerEventParam;

                if (eventParam != null)
                {
                    if (e != null)
                    {
                        eventParam.Guid = param.Guid;
                        eventParam.SignalDefineInformations = param.SignalDefineInformations;
                        eventParam.EventName = sender.ToString();
                        LoggerManager.Debug($"[TEST_ST] SignalTower Event Process : {sender.ToString()}");
                        this.SignalTowerManager().UpdateEventObjList(eventParam, Enable);
                    }
                }
                else
                {
                    LoggerManager.Debug($"SignalTower Event Process : eventParam is null ");
                    if (Parameter == null)
                    {
                        LoggerManager.Debug($"SignalTower Event Process : Parameter is null ");
                    }
                    else
                    {
                        LoggerManager.Debug($"SignalTower Event Process : Parameter= " + Parameter.ToString());
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }


    [Serializable]
    public class E84EventProcessor : EventProcessBase
    {
        public override void EventNotify(object sender, ProbeEventArgs e)
        {
            try
            {
                SignalTowerEventParam eventParam = new SignalTowerEventParam();
                ISignalTowerEventParam param = Parameter as ISignalTowerEventParam;

                if (eventParam != null)
                {
                    if (e != null)
                    {
                        eventParam.Guid = param.Guid;
                        eventParam.SignalDefineInformations = param.SignalDefineInformations;
                        eventParam.EventName = sender.ToString();
                        LoggerManager.Debug($"[TEST_ST] SignalTower Event Process : {sender.ToString()}");
                        this.SignalTowerManager().UpdateEventObjList(eventParam, Enable);
                    }
                }
                else
                {
                    LoggerManager.Debug($"SignalTower Event Process : eventParam is null ");
                    if (Parameter == null)
                    {
                        LoggerManager.Debug($"SignalTower Event Process : Parameter is null ");
                    }
                    else
                    {
                        LoggerManager.Debug($"SignalTower Event Process : Parameter= " + Parameter.ToString());
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}


