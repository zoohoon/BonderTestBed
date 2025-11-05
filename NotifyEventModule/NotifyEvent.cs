using ProberErrorCode;
using ProberInterfaces.Event;
using System;

namespace NotifyEventModule
{
    using DBManagerModule;
    using LogModule;
    using ProberInterfaces;
    using ProberInterfaces.SignalTower;
    using System.Collections.Generic;

    public abstract class NotifyEvent : INotifyEvent
    {
        public event EventHandler ProbeEventSubscibers;
        public ProbeEventArgs EventArg { get; set; }
        public Queue<ProbeEventArgs> EventArgQueue { get; set; } = new Queue<ProbeEventArgs>();


        public int EventNumber { get; set; }

        public NotifyEvent()
        {
            try
            {
                if (DBManager.EventInfo != null)
                {
                    String eventNumberStr = DBManager.EventInfo.ReadField(this.GetType().Name, nameof(EventNumber));
                    int eventNumberInt = 0;
                    if (int.TryParse(eventNumberStr, out eventNumberInt))
                        EventNumber = eventNumberInt;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public EventCodeEnum DoEvent(ProbeEventArgs args = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (ProbeEventSubscibers != null)
                {
                    LoggerManager.Debug($"[{this.GetType().Name}], DoEvent() Start", isInfo: true);

                    ProbeEventSubscibers(this, args);

                    LoggerManager.Debug($"[{this.GetType().Name}], DoEvent() End", isInfo: true);
                }
                else
                {
                    LoggerManager.Debug($"[{this.GetType().Name}], ProbeEventSubscibers is null.", isInfo: false);
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    public abstract class FoupNotifyEvent : NotifyEvent, IFoupNotifyEvent
    {
        public EventCodeEnum CheckFoupMode(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.CHECK_FOUP_MODE_DISABLE;
            try
            {
                if (pivInfo != null)
                {
                    if (pivInfo.FoupNumber != 0)
                    {
                        bool IsEnable = this.FoupOpModule()?.GetFoupController(pivInfo.FoupNumber)?.FoupModuleInfo?.Enable ?? true;

                        if (IsEnable)
                        {
                            var mode = this.FoupOpModule()?.GetFoupController(pivInfo.FoupNumber)?
                                .FoupModuleInfo?.FoupModeStatus ?? ProberInterfaces.Foup.FoupModeStatusEnum.ONLINE;
                            if (mode == ProberInterfaces.Foup.FoupModeStatusEnum.ONLINE)
                            {
                                retVal = EventCodeEnum.NONE;
                            }
                        }
                    }
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    //Gem event 6452
    public class CassetteLoadDoneEvent : FoupNotifyEvent, IGemCommand
    {
        public CassetteLoadDoneEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;
                    this.GEMModule().GetPIVContainer().FoupNumber.Value = foupnumber;
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }
    //Gem event 6453
    public class CassetteLoadFailEvent : FoupNotifyEvent, IGemCommand
    {
        public CassetteLoadFailEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;
                    this.GEMModule().GetPIVContainer().FoupNumber.Value = foupnumber;
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class CassetteUnloadDoneEvent : FoupNotifyEvent, IGemCommand
    {
        public CassetteUnloadDoneEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;
                    this.GEMModule().GetPIVContainer().FoupNumber.Value = foupnumber;
                    this.GEMModule().GetPIVContainer().UpdateFoupInfo(foupnumber);
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class CassetteUnloadFailEvent : FoupNotifyEvent, IGemCommand
    {
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;
                    this.GEMModule().GetPIVContainer().FoupNumber.Value = foupnumber;
                    this.GEMModule().GetPIVContainer().UpdateFoupInfo(foupnumber);
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }

    }

    //Gem event 9003 (Carrier Detected)
    public class FoupDockEvent : NotifyEvent, IGemCommand
    {
        public FoupDockEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;
                    this.GEMModule().GetPIVContainer().FoupNumber.Value = foupnumber;
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class FoupOpenErrorEvent : FoupNotifyEvent, IGemCommand
    {
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;
                    this.GEMModule().GetPIVContainer().FoupNumber.Value = foupnumber;
                    this.GEMModule().GetPIVContainer().UpdateFoupInfo(foupnumber);
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }

    }

    public class FoupCloseErrorEvent : FoupNotifyEvent, IGemCommand
    {
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;
                    this.GEMModule().GetPIVContainer().FoupNumber.Value = foupnumber;
                    this.GEMModule().GetPIVContainer().UpdateFoupInfo(foupnumber);
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }

    }

    public class CarrierAccessModeOnlineEvent : NotifyEvent, IGemCommand
    {
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;
                    this.GEMModule().GetPIVContainer().SetFoupAccessMode(foupnumber, true);
                    this.GEMModule().GetPIVContainer().UpdateFoupInfo(foupnumber);
                    this.GEMModule().GetPIVContainer().StageNumber.Value = pivInfo.StageNumber;
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
        public CarrierAccessModeOnlineEvent()
        {
        }
    }

    public class CarrierAccessModeManualEvent : NotifyEvent, IGemCommand
    {
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;
                    this.GEMModule().GetPIVContainer().SetFoupAccessMode(foupnumber, false);
                    this.GEMModule().GetPIVContainer().UpdateFoupInfo(foupnumber);
                    this.GEMModule().GetPIVContainer().StageNumber.Value = pivInfo.StageNumber;
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
        public CarrierAccessModeManualEvent()
        {
        }
    }

    //Gem event 9014 (Carrier Clamped)
    public class ClampLockEvent : FoupNotifyEvent, IGemCommand
    {
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;
                    this.GEMModule().GetPIVContainer().FoupNumber.Value = foupnumber;
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
        public ClampLockEvent()
        {
        }
    }

    public class ClampLockFailEvent : FoupNotifyEvent, IGemCommand
    {
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;
                    this.GEMModule().GetPIVContainer().FoupNumber.Value = foupnumber;
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class ClampUnlockEvent : FoupNotifyEvent, IGemCommand
    {
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;
                    this.GEMModule().GetPIVContainer().FoupNumber.Value = foupnumber;
                    this.GEMModule().GetPIVContainer().UpdateFoupInfo(foupnumber);
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }

    }

    public class ClampUnlockFailEvent : FoupNotifyEvent, IGemCommand
    {
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;
                    this.GEMModule().GetPIVContainer().FoupNumber.Value = foupnumber;
                    this.GEMModule().GetPIVContainer().UpdateFoupInfo(foupnumber);
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    //Gem event 870501
    public class CassetteIDReadDoneEvent : FoupNotifyEvent, IGemCommand
    {
        public CassetteIDReadDoneEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;
                    this.GEMModule().GetPIVContainer().SetCarrierId(foupnumber, pivInfo.CarrierID);
                    this.GEMModule().GetPIVContainer().SetFoupInfo(foupnumber, carrierid: pivInfo.CarrierID);
                    this.GEMModule().GetPIVContainer().UpdateFoupInfo(foupnumber);
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    //Gem event 9009
    public class SlotVarifyDoneEvent : NotifyEvent, IGemCommand
    {
        public SlotVarifyDoneEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().UpdateFoupInfo(pivInfo.FoupNumber);
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }


    //Gem event 9010
    public class SlotVarifyFailEvent : NotifyEvent, IGemCommand
    {
        public SlotVarifyFailEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().UpdateFoupInfo(pivInfo.FoupNumber);
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }


    //Gem event 9101
    public class StageSlotSelectedEvent : NotifyEvent, IGemCommand
    {
        public StageSlotSelectedEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().SetFoupInfo(pivInfo.FoupNumber, lotid: pivInfo.LotID, processingTemp: pivInfo.SetTemperature);
                    this.GEMModule().GetPIVContainer().UpdateFoupInfo(pivInfo.FoupNumber);
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }

    }

    //Gem event 6061
    public class CassetteToArmEvent : NotifyEvent, IGemCommand
    {
        public CassetteToArmEvent()
        {

        }

        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().PreLoadingSlotNum.Value = pivInfo.PreLoadingSlotNum;
                    this.GEMModule().GetPIVContainer().SetFoupInfo(pivInfo.FoupNumber);
                    this.GEMModule().GetPIVContainer().UpdateFoupInfo(pivInfo.FoupNumber);
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }

    }

    //Gem event 6013
    public class CellPausedEvent : NotifyEvent, IGemCommand
    {
        public CellPausedEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    //this.GEMModule().GetPIVContainer().StageNumber.Value = pivInfo.StageNumber;
                    //this.GEMModule().GetPIVContainer().SetFoupInfo(pivInfo.FoupNumber);
                    //this.GEMModule().GetPIVContainer().UpdateFoupInfo(pivInfo.FoupNumber);
                    this.GEMModule().GetPIVContainer().UpdateStageLotInfo(pivInfo.FoupNumber);

                    //this.GEMModule().GetPIVContainer().WaferID.Value = pivInfo.WaferID;
                    //this.GEMModule().GetPIVContainer().SlotNumber.Value = pivInfo.SlotNumber;

                    //this.GEMModule().GetPIVContainer().Overdrive.Value = pivInfo.Overdrive;
                    this.GEMModule().GetPIVContainer().CurTemperature.Value = pivInfo.CurTemperature;
                    this.GEMModule().GetPIVContainer().SetTemperature.Value = pivInfo.SetTemperature;
                    this.GEMModule().GetPIVContainer().UpdateStageLotInfo(pivInfo.FoupNumber);
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }


    //Gem event 9102
    public class StageSlotSelectFailEvent : NotifyEvent, IGemCommand
    {
        public StageSlotSelectFailEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;
                    this.GEMModule().GetPIVContainer().FoupNumber.Value = foupnumber;
                    this.GEMModule().GetPIVContainer().UpdateFoupInfo(foupnumber);
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }

    }

    //Gem event 9006
    public class CassetteIDReadFailEvent : FoupNotifyEvent, IGemCommand
    {
        public CassetteIDReadFailEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;
                    this.GEMModule().GetPIVContainer().FoupNumber.Value = foupnumber;
                    this.GEMModule().GetPIVContainer().UpdateFoupInfo(foupnumber);
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    //Gem event 6462
    public class ScanFoupDoneEvent : FoupNotifyEvent, IGemCommand
    {
        public ScanFoupDoneEvent()
        {
        }

        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;
                    this.GEMModule().GetPIVContainer().SetFoupInfo(foupnumber, slotlist: pivInfo.ListOfSlot);
                    this.GEMModule().GetPIVContainer().SetFoupState(foupnumber, GEMFoupStateEnum.READ_CARRIER_MAP);
                    this.GEMModule().GetPIVContainer().UpdateFoupInfo(foupnumber);
                    //this.GEMModule().GetPIVContainer().LoaderID.Value = "LOADER";// TEST
                }


                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    //Gem event 6463
    public class ScanFoupFailEvent : FoupNotifyEvent, IGemCommand
    {
        public ScanFoupFailEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;
                    this.GEMModule().GetPIVContainer().FoupNumber.Value = foupnumber;
                    this.GEMModule().GetPIVContainer().UpdateFoupInfo(foupnumber);
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    //Gem event 9009
    public class SlotMapVarifyDoneEvent : NotifyEvent
    {
        public SlotMapVarifyDoneEvent()
        {
        }
    }

    /// <summary>
    /// Single, Cell (Group Prober) 기준 
    /// </summary>
    public class LotStartEvent : NotifyEvent, IGemCommand
    {
        public LotStartEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;
                    this.GEMModule().GetPIVContainer().SetFoupInfo(foupnumber, lotid: pivInfo.LotID);
                    this.GEMModule().GetPIVContainer().UpdateFoupInfo(foupnumber);
                    this.GEMModule().GetPIVContainer().SetLotID(pivInfo.LotID);
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    /// <summary>
    /// Single, Cell (Group Prober) 기준 
    /// </summary>
    public class LotEndEvent : NotifyEvent, IGemCommand
    {
        public LotEndEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;
                    this.GEMModule().GetPIVContainer().UpdateStageLotInfo(foupnumber);
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    /// <summary>
    /// Single, Cell (Group Prober) 기준 
    /// </summary>
    public class LotAbortEvent : NotifyEvent
    {
    }

    /// <summary>
    /// Single, Cell (Group Prober) 기준 
    /// </summary>
    public class LotSwitchedEvent : NotifyEvent
    {
        public LotSwitchedEvent()
        {
        }
    }

    /// <summary>
    /// Loader (Group Prober) 기준 
    /// </summary>
    public class LoaderLotStartEvent : NotifyEvent, IGemCommand
    {
        public LoaderLotStartEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;
                    this.GEMModule().GetPIVContainer().SetFoupInfo(foupnumber, lotid: pivInfo.LotID);
                    this.GEMModule().GetPIVContainer().UpdateFoupInfo(foupnumber);
                    this.GEMModule().GetPIVContainer().SetLotID(pivInfo.LotID);
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }

    }

    /// <summary>
    /// Loader (Group Prober) 기준 
    /// </summary>
    public class LoaderLotEndEvent : NotifyEvent, IGemCommand
    {
        public LoaderLotEndEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;
                    this.GEMModule().GetPIVContainer().UpdateFoupInfo(foupnumber);
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }

    }

    /// <summary>
    /// Loader (Group Prober) 기준 
    /// </summary>
    public class LoaderLotAbortEvent : NotifyEvent, IGemCommand
    {
        public LoaderLotAbortEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;
                    this.GEMModule().GetPIVContainer().UpdateFoupInfo(foupnumber);
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }

    }

    //CEID 9200
    public class CardChangeSeqAbortEvent : NotifyEvent, IGemCommand
    {
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().StageNumber.Value = pivInfo.StageNumber;
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class WaferChangeSeqAbortEvent : NotifyEvent, IGemCommand
    {
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().StageNumber.Value = pivInfo.StageNumber;
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }


    public class CellAbortFail : NotifyEvent
    {
        public CellAbortFail()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().StageNumber.Value = pivInfo.StageNumber;
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }


    public class LotEndDueToUnloadAllWaferEvent : NotifyEvent, IGemCommand
    {
        public LotEndDueToUnloadAllWaferEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().SetLotID(pivInfo.LotID);
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }    

    public class FoupLoadCassetteAttachEvent : NotifyEvent
    {
        public FoupLoadCassetteAttachEvent()
        {

        }
    }

    public class IncorrectDRNumberEvent : NotifyEvent
    {
        public IncorrectDRNumberEvent()
        {
        }
    }

    public class PassDWCommandEvent : NotifyEvent
    {
        public PassDWCommandEvent()
        {
        }
    }

    public class FailDWCommandEvent : NotifyEvent
    {
        public FailDWCommandEvent()
        {
        }
    }


    //Gem event 6061
    public class FoupToArmEvent : NotifyEvent
    {
        public FoupToArmEvent()
        {
        }
    }

    public class ArmErrorEvent : NotifyEvent
    {

    }

    //Gem event 250
    public class OcrReadDoneEvent : NotifyEvent, IGemCommand
    {
        public OcrReadDoneEvent()
        {
        }

        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().WaferID.Value = pivInfo.WaferID;
                    this.GEMModule().GetPIVContainer().PreLoadingWaferId.Value = pivInfo.WaferID;
                    this.GEMModule().GetPIVContainer().PreLoadingSlotNum.Value = pivInfo.PreLoadingSlotNum;

                    int foupnumber = pivInfo.FoupNumber;
                    this.GEMModule().GetPIVContainer().SetFoupInfo(foupnumber, devicename: pivInfo.RecipeID);
                    this.GEMModule().GetPIVContainer().UpdateFoupInfo(foupnumber);
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }

    }

    public class OcrConfirmFailEvent : NotifyEvent, IGemCommand
    {
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().FoupNumber.Value = pivInfo.FoupNumber;
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class OcrReadFailEvent : NotifyEvent, IGemCommand
    {
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().PreAlignNumber.Value = pivInfo.PreAlignNumber;
                    this.GEMModule().GetPIVContainer().SlotNumber.Value = pivInfo.SlotNumber;
                    this.GEMModule().GetPIVContainer().SetLotID(pivInfo.LotID);
                    this.GEMModule().GetPIVContainer().FoupNumber.Value = pivInfo.FoupNumber;

                    this.GEMModule().GetPIVContainer().UpdateFoupInfo(pivInfo.FoupNumber);
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }
    /// <summary>
    /// [H사] CEID: 9054  
    /// </summary>
    public class PolishWaferIDReadResultEvent : NotifyEvent, IGemCommand
    {
        public PolishWaferIDReadResultEvent()
        {
        }

        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().WaferID.Value = pivInfo.WaferID;
                    this.GEMModule().GetPIVContainer().PolishWaferIDReadResult.Value = pivInfo.PolishWaferIDReadResult;
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }
    /// <summary>
    /// [H사] CEID: 9055
    /// </summary>
    public class WaferAutofeedResultEvent : NotifyEvent, IGemCommand
    {
        public WaferAutofeedResultEvent()
        {
        }

        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().WaferAutofeedResult.Value = pivInfo.WaferAutofeedResult;
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    /// <summary>
    /// [H사] CEID: 9055
    /// </summary>
    public class SingleWaferAutofeedResultEvent : NotifyEvent, IGemCommand
    {
        public SingleWaferAutofeedResultEvent()
        {
        }

        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().WaferAutofeedResult.Value = pivInfo.WaferAutofeedResult;
                    this.GEMModule().GetPIVContainer().WaferChange_Location1_LoadPortId.Value = pivInfo.WaferChange_Location1_LoadPortId;
                    this.GEMModule().GetPIVContainer().WaferChange_Location1_AtomId.Value = pivInfo.WaferChange_Location1_AtomId;
                    this.GEMModule().GetPIVContainer().WaferChange_Location1_WaferId.Value = pivInfo.WaferChange_Location1_WaferId;
                    this.GEMModule().GetPIVContainer().WaferChange_Location2_LoadPortId.Value = pivInfo.WaferChange_Location2_LoadPortId;
                    this.GEMModule().GetPIVContainer().WaferChange_Location2_AtomId.Value = pivInfo.WaferChange_Location2_AtomId;
                    this.GEMModule().GetPIVContainer().WaferChange_Location2_WaferId.Value = pivInfo.WaferChange_Location2_WaferId;
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }
    //Gem event 6474
    public class TcwOcrReadDoneEvent : NotifyEvent, IGemCommand
    {
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().WaferID.Value = pivInfo.WaferID;
                    this.GEMModule().GetPIVContainer().PreLoadingWaferId.Value = pivInfo.WaferID;
                    this.GEMModule().GetPIVContainer().PreLoadingSlotNum.Value = pivInfo.PreLoadingSlotNum;

                    int foupnumber = pivInfo.FoupNumber;
                    this.GEMModule().GetPIVContainer().SetFoupInfo(foupnumber, devicename: pivInfo.RecipeID);
                    this.GEMModule().GetPIVContainer().UpdateFoupInfo(foupnumber);
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }

    }

    //Gem event 6475
    public class TcwOcrReadFailEvent : NotifyEvent, IGemCommand
    {
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().PreAlignNumber.Value = pivInfo.PreAlignNumber;
                    this.GEMModule().GetPIVContainer().UpdateFoupInfo(pivInfo.FoupNumber);
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    //Gem event 9017
    public class LastWaferMoveOutEvent : NotifyEvent
    {
        public LastWaferMoveOutEvent()
        {
        }
    }

    //Gem event 839
    public class ChuckLoadDoneEvent : NotifyEvent
    {
        public ChuckLoadDoneEvent()
        {
        }
    }

    //Gem event 6003
    public class LotPausedEvent : NotifyEvent
    {
        public LotPausedEvent()
        {
        }
    }

    //Gem event 9012
    public class WaferReadyForProbeEvent : NotifyEvent
    {
        public WaferReadyForProbeEvent()
        {
        }
    }

    public class ProbingStartPreEvent : NotifyEvent
    {
        public ProbingStartPreEvent()
        {
        }
    }

    //Gem event 9018
    public class ProbingStartEvent : NotifyEvent
    {
        public ProbingStartEvent()
        {
        }
    }

    public class ProbingEndEvent : NotifyEvent
    {
        public ProbingEndEvent()
        {
        }
    }

    //Gem event 6970
    public class CurrentDieTestStartEvent : NotifyEvent
    {
        public CurrentDieTestStartEvent()
        {
        }
    }

    //Gem event 9002
    public class FoupReadyToUnloadEvent : FoupNotifyEvent, IGemCommand
    {
        public FoupReadyToUnloadEvent()
        {

        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;
                    if (foupnumber != 0)
                    {
                        this.GEMModule().GetPIVContainer().FoupNumber.Value = foupnumber;
                        //this.GEMModule().GetPIVContainer().SetFoupState(pivInfo.FoupNumber, GEMFoupStateEnum.READY_TO_UNLOAD); //#Hynix_Merge: 검토 필요, 이줄 하이닉스 브랜치에는 있었는데 빠지는게 맞는것 같음. 
                        this.GEMModule().GetPIVContainer().UpdateFoupInfo(foupnumber);
                    }
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    /// <summary>
    /// Loadport set to a carrier placed state
    /// </summary>
    public class CarrierPlacedEvent : FoupNotifyEvent, IGemCommand
    {
        public CarrierPlacedEvent()
        {
        }

        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().SetLotID("");
                    this.GEMModule().GetPIVContainer().SetLoaderLotIds(pivInfo.FoupNumber, "");
                    this.GEMModule().GetPIVContainer().SetFoupInfo(pivInfo.FoupNumber, "", 0, "");
                    this.GEMModule().GetPIVContainer().FoupNumber.Value = pivInfo.FoupNumber;
                    this.GEMModule().GetPIVContainer().SetFoupState(pivInfo.FoupNumber, GEMFoupStateEnum.PLACED);
                    this.GEMModule().GetPIVContainer().UpdateFoupInfo(pivInfo.FoupNumber);
                }


                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    //Gem event 9004
    public class CarrierRemovedEvent : FoupNotifyEvent, IGemCommand
    {
        public CarrierRemovedEvent()
        {
        }

        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;
                    this.GEMModule().GetPIVContainer().SetLotID("");
                    this.GEMModule().GetPIVContainer().SetFoupInfo(foupnumber, lotid: "", stagelist: "", slotlist: "");
                    this.GEMModule().GetPIVContainer().SetFoupState(foupnumber, GEMFoupStateEnum.READY_TO_UNLOAD);
                    this.GEMModule().GetPIVContainer().UpdateFoupInfo(foupnumber);
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    /// <summary>
    /// Loadport set to unload state from and other state.
    /// </summary>
    public class CarrierCanceledEvent : NotifyEvent, IGemCommand
    {
        public CarrierCanceledEvent() { }

        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;

                    this.GEMModule().GetPIVContainer().SetFoupState(foupnumber, GEMFoupStateEnum.READY_TO_UNLOAD);
                    this.GEMModule().GetPIVContainer().SetFoupInfo(foupnumber, lotid: pivInfo.LotID, stagelist: "", slotlist: "");
                    this.GEMModule().GetPIVContainer().UpdateFoupInfo(foupnumber);
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }

    }

    /// <summary>
    /// 같은 디바이스, 같은 셀이 할당된 풉이 모두 종료되었을때 이벤트 호출.
    /// </summary>
    public class SameDeviceEndToSlot : NotifyEvent, IGemCommand
    {
        public SameDeviceEndToSlot() { }

        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;
                    this.GEMModule().GetPIVContainer().UpdateFoupInfo(foupnumber);
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }

    }

    public class CarrierCompleateEvent : NotifyEvent, IGemCommand
    {
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;
                    //this.GEMModule().GetPIVContainer().SetFoupAccessMode(foupnumber, true);
                    this.GEMModule().GetPIVContainer().UpdateFoupInfo(foupnumber);
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    //AllAssignedWaferSlotOutEvent ==> CanFoupUnloadEvent(9017) Hynix
    public class CanFoupUnloadEvent : NotifyEvent, IGemCommand
    {
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;
                    this.GEMModule().GetPIVContainer().UpdateFoupInfo(foupnumber);
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class CarrierIdVerifiedEvent : NotifyEvent
    {

    }
    //Gem event 9001
    public class FoupReadyToLoadEvent : FoupNotifyEvent, IGemCommand
    {
        public FoupReadyToLoadEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;
                    if (foupnumber != 0)
                    {
                        this.GEMModule().GetPIVContainer().FoupNumber.Value = foupnumber;
                        this.GEMModule().GetPIVContainer().UpdateFoupInfo(foupnumber);
                    }
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class FoupInServiceEvent : FoupNotifyEvent, IGemCommand
    {
        public FoupInServiceEvent()
        {

        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;
                    if (foupnumber != 0)
                    {
                        this.GEMModule().GetPIVContainer().FoupNumber.Value = foupnumber;
                        this.GEMModule().GetPIVContainer().UpdateFoupInfo(foupnumber);
                    }
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class FoupOutOfServiceEvent : FoupNotifyEvent, IGemCommand
    {
        public FoupOutOfServiceEvent()
        {

        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;
                    if (foupnumber != 0)
                    {
                        this.GEMModule().GetPIVContainer().FoupNumber.Value = foupnumber;
                        this.GEMModule().GetPIVContainer().UpdateFoupInfo(foupnumber);
                    }
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class FoupTransferBlockedEvent : FoupNotifyEvent, IGemCommand
    {
        public FoupTransferBlockedEvent()
        {

        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;
                    if (foupnumber != 0)
                    {
                        this.GEMModule().GetPIVContainer().FoupNumber.Value = foupnumber;
                        this.GEMModule().GetPIVContainer().UpdateFoupInfo(foupnumber);
                    }
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class FoupTransferReady : FoupNotifyEvent, IGemCommand
    {
        public FoupTransferReady()
        {

        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;
                    if (foupnumber != 0)
                    {
                        this.GEMModule().GetPIVContainer().FoupNumber.Value = foupnumber;
                        this.GEMModule().GetPIVContainer().UpdateFoupInfo(foupnumber);
                    }
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class FoupStateChangedToLoadAndUnloadEvent : NotifyEvent, IGemCommand
    {
        public FoupStateChangedToLoadAndUnloadEvent()
        {

        }

        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;
                    if (foupnumber != 0)
                    {
                        this.GEMModule().GetPIVContainer().FoupNumber.Value = foupnumber;
                        this.GEMModule().GetPIVContainer().UpdateFoupInfo(foupnumber);
                    }
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }

    }

    public class FoupStateChangedToUnloadEvent : NotifyEvent, IGemCommand
    {
        public FoupStateChangedToUnloadEvent()
        {

        }

        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    int foupnumber = pivInfo.FoupNumber;
                    if (foupnumber != 0)
                    {
                        this.GEMModule().GetPIVContainer().FoupNumber.Value = foupnumber;
                        this.GEMModule().GetPIVContainer().UpdateFoupInfo(foupnumber);
                    }
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }

    }

    public class LotModeChangeToDynamicEvent : NotifyEvent, IGemCommand
    {
        public LotModeChangeToDynamicEvent()
        {

        }

        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class LotModeChangeToNormalEvent : NotifyEvent, IGemCommand
    {
        public LotModeChangeToNormalEvent()
        {

        }

        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }

    }

    //Gem event 9013
    public class EndWaferReturnSlotEvent : NotifyEvent, IGemCommand
    {
        public EndWaferReturnSlotEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().SetFoupInfo(pivInfo.FoupNumber);
                    this.GEMModule().GetPIVContainer().UpdateFoupInfo(pivInfo.FoupNumber);

                    //this.GEMModule().GetPIVContainer().WaferID.Value = pivInfo.WaferID;
                    //this.GEMModule().GetPIVContainer().UnloadingWaferID.Value = pivInfo.UnloadingWaferID; 셀에 미리 정의된 값을 사용
                    //this.GEMModule().GetPIVContainer().UnloadingSlotNum.Value = pivInfo.UnloadingSlotNum; 셀에 미리 정의된 값을 사용
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.GEM_PIV_SET_EXCEPTION;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    //Gem event 2051
    public class WaferEndEvent : NotifyEvent, IGemCommand
    {
        public WaferEndEvent()
        {
        }


        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //if (pivInfo.FoupNumber != null)
                //{
                //    this.GEMModule().GetPIVContainer().UpdateFoupInfo(pivInfo.FoupNumber);
                //}


                this.GEMModule().GetPIVContainer().NotchAngle.Value = pivInfo.NotchAngle;

                this.GEMModule().GetPIVContainer().TotalDieCount.Value = pivInfo.TotalDieCount;
                this.GEMModule().GetPIVContainer().PassDieCount.Value = pivInfo.PassDieCount;
                this.GEMModule().GetPIVContainer().FailDieCount.Value = pivInfo.FailDieCount;
                this.GEMModule().GetPIVContainer().YieldOfBadDie.Value = pivInfo.YieldOfBadDie;
                this.GEMModule().GetPIVContainer().PCardContactCount.Value = pivInfo.PCardContactCount;

                this.GEMModule().GetPIVContainer().WaferStartTime.Value = pivInfo.WaferStartTime;
                this.GEMModule().GetPIVContainer().WaferEndTime.Value = pivInfo.WaferEndTime;
                //this.GEMModule().GetPIVContainer().WaferEndResult.Value = pivInfo.WaferEndResult; 다른위치에서 SetWaferEndResult해주고 있음. 

                //this.GEMModule().GetPIVContainer().XCoordinate.Value = -9999;
                //this.GEMModule().GetPIVContainer().YCoordinate.Value = -9999;
                this.GEMModule().GetPIVContainer().FullSite.Value = "";

                this.GEMModule().GetPIVContainer().CurTemperature.Value = pivInfo.CurTemperature;
                this.GEMModule().GetPIVContainer().SetTemperature.Value = pivInfo.SetTemperature;
                this.GEMModule().GetPIVContainer().Overdrive.Value = pivInfo.Overdrive;


                this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.UNLOADING);
                this.GEMModule().GetPIVContainer().UpdateStageLotInfo(pivInfo.FoupNumber);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.GEM_PIV_SET_EXCEPTION;
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                //if (this.LoaderController().IsCancel != true)
                //{
                //    retVal = EventCodeEnum.NONE;
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.GEM_PRE_CHECK_EXCEPTION;
            }
            return retVal;
        }
    }

    public class WaferTestEndEvent : NotifyEvent, IGemCommand
    {
        public WaferTestEndEvent()
        {
        }


        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //if (pivInfo.FoupNumber != null)
                //{
                //    this.GEMModule().GetPIVContainer().UpdateFoupInfo(pivInfo.FoupNumber);
                //}


                this.GEMModule().GetPIVContainer().NotchAngle.Value = pivInfo.NotchAngle;

                this.GEMModule().GetPIVContainer().TotalDieCount.Value = pivInfo.TotalDieCount;
                this.GEMModule().GetPIVContainer().PassDieCount.Value = pivInfo.PassDieCount;
                this.GEMModule().GetPIVContainer().FailDieCount.Value = pivInfo.FailDieCount;
                this.GEMModule().GetPIVContainer().YieldOfBadDie.Value = pivInfo.YieldOfBadDie;
                this.GEMModule().GetPIVContainer().PCardContactCount.Value = pivInfo.PCardContactCount;

                this.GEMModule().GetPIVContainer().WaferStartTime.Value = pivInfo.WaferStartTime;
                this.GEMModule().GetPIVContainer().WaferEndTime.Value = pivInfo.WaferEndTime;
                //this.GEMModule().GetPIVContainer().WaferEndResult.Value = pivInfo.WaferEndResult; 다른위치에서 SetWaferEndResult해주고 있음. 

                //this.GEMModule().GetPIVContainer().XCoordinate.Value = -9999;
                //this.GEMModule().GetPIVContainer().YCoordinate.Value = -9999;
                this.GEMModule().GetPIVContainer().FullSite.Value = "";

                this.GEMModule().GetPIVContainer().CurTemperature.Value = pivInfo.CurTemperature;
                this.GEMModule().GetPIVContainer().SetTemperature.Value = pivInfo.SetTemperature;
                this.GEMModule().GetPIVContainer().Overdrive.Value = pivInfo.Overdrive;


                this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.UNLOADING);
                this.GEMModule().GetPIVContainer().UpdateStageLotInfo(pivInfo.FoupNumber);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.GEM_PIV_SET_EXCEPTION;
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                //if (this.LoaderController().IsCancel != true)
                //{
                //    retVal = EventCodeEnum.NONE;
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.GEM_PRE_CHECK_EXCEPTION;
            }
            return retVal;
        }
    }

    public class WaferStartEvent : NotifyEvent, IGemCommand
    {
        public WaferStartEvent()
        {
        }

        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    //this.GEMModule().GetPIVContainer().SetFoupInfo(pivInfo.FoupNumber);
                    //this.GEMModule().GetPIVContainer().UpdateFoupInfo(pivInfo.FoupNumber);

                    //this.GEMModule().GetPIVContainer().StageNumber.Value = pivInfo.StageNumber;
                    //this.GEMModule().GetPIVContainer().WaferID.Value = pivInfo.WaferID;
                    this.GEMModule().GetPIVContainer().WaferStartTime.Value = pivInfo.WaferStartTime;
                    this.GEMModule().GetPIVContainer().CurTemperature.Value = pivInfo.CurTemperature;
                    this.GEMModule().GetPIVContainer().SetTemperature.Value = pivInfo.SetTemperature;
                    this.GEMModule().GetPIVContainer().UpdateStageLotInfo(pivInfo.FoupNumber);
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.GEM_PIV_SET_EXCEPTION;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.LoaderController().IsCancel != true)
                {
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.GEM_PRE_CHECK_EXCEPTION;
            }
            return retVal;
        }



    }

    public class SingleSequenceEvent : NotifyEvent
    {
        public SingleSequenceEvent()
        {
        }
    }

    public class CanProbingStartEvent : NotifyEvent
    {
        public CanProbingStartEvent()
        {
        }
    }


    public class GoToStartDieEvent : NotifyEvent, IGemCommand
    {
        public GoToStartDieEvent()
        {
        }

        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    //this.GEMModule().GetPIVContainer().UpdateFoupInfo(pivInfo.FoupNumber);
                    //this.GEMModule().GetPIVContainer().UpdateStageLotInfo(pivInfo.FoupNumber);

                    //this.GEMModule().GetPIVContainer().SlotNumber.Value = pivInfo.SlotNumber;
                    this.GEMModule().GetPIVContainer().PreLoadingWaferId.Value = pivInfo.PreLoadingWaferId;
                    this.GEMModule().GetPIVContainer().XCoordinate.Value = pivInfo.XCoordinate;
                    this.GEMModule().GetPIVContainer().YCoordinate.Value = pivInfo.YCoordinate;
                    this.GEMModule().GetPIVContainer().CurTemperature.Value = pivInfo.CurTemperature;
                    this.GEMModule().GetPIVContainer().SetTemperature.Value = pivInfo.SetTemperature;
                    this.GEMModule().GetPIVContainer().UpdateStageLotInfo(pivInfo.FoupNumber);



                    retVal = EventCodeEnum.NONE;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.GEM_PIV_SET_EXCEPTION;
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }

    }
    public class UpdateStageProbingDataEvent : NotifyEvent, IGemCommand
    {
        public UpdateStageProbingDataEvent()
        {
        }

        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().XCoordinate.Value = pivInfo.XCoordinate;
                    this.GEMModule().GetPIVContainer().YCoordinate.Value = pivInfo.YCoordinate;
                    this.GEMModule().GetPIVContainer().FullSite.Value = pivInfo.FullSite;

                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.GEM_PIV_SET_EXCEPTION;
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }

    }
    public class GoToCentertDieEvent : NotifyEvent
    {
        public GoToCentertDieEvent()
        {
        }
    }

    public class GPIBTestCompleteNotIncludedEvent : NotifyEvent
    {
        public GPIBTestCompleteNotIncludedEvent()
        {
        }
    }

    //public class MoveToPinPadMatchedPosEvent : NotifyEvent
    //{
    //    public MoveToPinPadMatchedPosEvent()
    //    {
    //    }
    //}



    public class ProbingZUpFirstProcessEvent : NotifyEvent, IGemCommand
    {
        public ProbingZUpFirstProcessEvent()
        {
        }

        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.READY_TO_TEST);
                this.GEMModule().GetPIVContainer().UpdateStageLotInfo(pivInfo.FoupNumber);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.GEM_PIV_SET_EXCEPTION;
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class ProbingZUpProcessEvent : NotifyEvent, IGemCommand
    {
        public ProbingZUpProcessEvent()
        {
        }

        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.READY_TO_TEST);
                this.GEMModule().GetPIVContainer().UpdateStageLotInfo(pivInfo.FoupNumber);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.GEM_PIV_SET_EXCEPTION;
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.ProbingModule().IsFirstZupSequence == true)
                {
                    LoggerManager.Debug($"ProbingZUpProcessEvent.PreCheck(): Multiple Contact Event");
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.GEM_PRE_CHECK_EXCEPTION;
            }
            return retVal;
        }

    }

    public class ProbingZDownFirstProcessEvent : NotifyEvent
    {
        public ProbingZDownFirstProcessEvent()
        {
        }
    }

    public class ProbingZDownProcessEvent : NotifyEvent, IGemCommand
    {
        public ProbingZDownProcessEvent()
        {
        }

        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //if (pivInfo.FoupNumber != null)
                //{
                //    this.GEMModule().GetPIVContainer().UpdateFoupInfo(pivInfo.FoupNumber);
                //}

                //this.GEMModule().GetPIVContainer().FullSite.Value = "";

                //this.GEMModule().GetPIVContainer().SystemClock.Value = pivInfo.SystemClock;
                //this.GEMModule().GetPIVContainer().NotchAngle.Value = pivInfo.NotchAngle;

                //this.GEMModule().GetPIVContainer().TotalDieCount.Value = pivInfo.TotalDieCount;
                //this.GEMModule().GetPIVContainer().PassDieCount.Value = pivInfo.PassDieCount;
                //this.GEMModule().GetPIVContainer().FailDieCount.Value = pivInfo.FailDieCount;
                //this.GEMModule().GetPIVContainer().YieldOfBadDie.Value = pivInfo.YieldOfBadDie;

                //this.GEMModule().GetPIVContainer().WaferStartTime.Value = pivInfo.WaferStartTime;
                //this.GEMModule().GetPIVContainer().WaferEndTime.Value = pivInfo.WaferEndTime;
                //this.GEMModule().GetPIVContainer().Overdrive.Value = pivInfo.Overdrive;

                //this.GEMModule().GetPIVContainer().ProbeCardID.Value = pivInfo.ProbeCardID;
                //this.GEMModule().GetPIVContainer().ProberType.Value = pivInfo.ProberID;

                this.GEMModule().GetPIVContainer().FullSite.Value = "";
                this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.Z_DOWN);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.GEM_PIV_SET_EXCEPTION;
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class CalculatePfNYieldEvent : NotifyEvent // Calculate Pass/Fail & Yield. Bin 분석후 발생하는 이벤트.
    {
        public CalculatePfNYieldEvent()
        {
        }
    }

    public class RemainSequenceEvent : NotifyEvent
    {
        public RemainSequenceEvent()
        {
        }
    }

    public class DontRemainSequenceEvent : NotifyEvent
    {
        public DontRemainSequenceEvent()
        {
        }
    }

    public class MoveToDiePositionEvent : NotifyEvent
    {
        public MoveToDiePositionEvent()
        {
        }
    }

    public class DeviceChangedEvent : NotifyEvent
    {
        public DeviceChangedEvent()
        {

        }
    }

    public class DeviceChangeFailEvent : NotifyEvent
    {
        public DeviceChangeFailEvent()
        {
        }
    }

    public class MachineInitEvent : NotifyEvent
    {
        public MachineInitEvent()
        {
        }
    }

    public class MachineInitFailEvent : NotifyEvent
    {

    }

    public class LotResumeEvent : NotifyEvent
    {
        public LotResumeEvent()
        {
        }
    }

    public class CassetteSensingEvent : FoupNotifyEvent
    {
        public CassetteSensingEvent()
        {
        }
    }

    public class ScanCassetteDoneEvent : FoupNotifyEvent
    {
        public ScanCassetteDoneEvent()
        {
        }
    }



    public class PreAlignDoneEvent : NotifyEvent
    {
        public PreAlignDoneEvent()
        {
        }
    }

    public class PreAlignFailEvent : NotifyEvent
    {

    }

    public class WaferLoadedEvent : NotifyEvent, IGemCommand
    {
        public WaferLoadedEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().FoupNumber.Value = pivInfo.FoupNumber;

                    this.GEMModule().GetPIVContainer().CurTemperature.Value = pivInfo.CurTemperature;
                    this.GEMModule().GetPIVContainer().SetTemperature.Value = pivInfo.SetTemperature;
                    //v22_merge//this.GEMModule().GetPIVContainer().SetStageLotInfo(pivInfo.FoupNumber, stageNumber: pivInfo.StageNumber, slotNumber: pivInfo.SlotNumber);
                    this.GEMModule().GetPIVContainer().StageNumber.Value = pivInfo.StageNumber;
                    this.GEMModule().GetPIVContainer().SlotNumber.Value = pivInfo.SlotNumber;
                    this.GEMModule().GetPIVContainer().UpdateStageLotInfo(pivInfo.FoupNumber);
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class WaferUnloading : NotifyEvent, IGemCommand
    {
        public WaferUnloading()
        {
        }


        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.GEMModule().GetPIVContainer().XCoordinate.Value = -9999;
                this.GEMModule().GetPIVContainer().YCoordinate.Value = -9999;
                this.GEMModule().GetPIVContainer().FullSite.Value = "";
                this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.UNLOADING);
                this.GEMModule().GetPIVContainer().UpdateStageLotInfo(pivInfo.FoupNumber);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.GEM_PIV_SET_EXCEPTION;
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //if (this.LoaderController().IsCancel != true)
                //{
                //    retVal = EventCodeEnum.NONE;
                //}
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.GEM_PRE_CHECK_EXCEPTION;
            }
            return retVal;
        }
    }

    public class WaferUnloadedEvent : NotifyEvent
    {
        public WaferUnloadedEvent()
        {
        }
    }

    public class WaferUnloadedToSlotEvent : NotifyEvent, IGemCommand
    {
        public WaferUnloadedToSlotEvent()
        {

        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if(pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().SetWaferID(pivInfo.WaferID);
                    this.GEMModule().GetPIVContainer().SlotNumber.Value = pivInfo.SlotNumber;
                    this.GEMModule().GetPIVContainer().UnloadingSlotNum.Value = pivInfo.UnloadingSlotNum;
                    this.GEMModule().GetPIVContainer().UnloadingWaferID.Value = pivInfo.UnloadingWaferID;

                    if(pivInfo.FoupNumber > 0)
                    {
                        this.GEMModule().GetPIVContainer().FoupNumber.Value = pivInfo.FoupNumber;
                        this.GEMModule().GetPIVContainer().SetLotID(pivInfo.LotID);
                        this.GEMModule().GetPIVContainer().UpdateFoupInfo(pivInfo.FoupNumber);
                    }
                    
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }
    public class WaferUnloadedFailToSlotEvent : NotifyEvent, IGemCommand
    {
        public WaferUnloadedFailToSlotEvent()
        {

        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().FoupNumber.Value = pivInfo.FoupNumber;
                    this.GEMModule().GetPIVContainer().SetLotID(pivInfo.LotID);
                    this.GEMModule().GetPIVContainer().SetWaferID(pivInfo.WaferID);
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class PolishWaferLoadedEvent : NotifyEvent
    {
        public PolishWaferLoadedEvent() { }
    }

    public class PolishWaferUnloadedEvent : NotifyEvent
    {
        public PolishWaferUnloadedEvent() { }
    }

    //CEID 3602
    public class CardLoadingEvent : NotifyEvent, IGemCommand
    {
        public CardLoadingEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().SetProberCardID(pivInfo.ProbeCardID);
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class CardUnloadedEvent : NotifyEvent, IGemCommand
    {
        public CardUnloadedEvent()
        {
        }

        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.GEMModule().GetPIVContainer().SetProberCardID("");
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }

    }

    //CEID 3605
    public class CardDockEvent : NotifyEvent, IGemCommand
    {
        public CardDockEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            //this.GEMModule().GetPIVContainer().ProbeCardID.Value = pivInfo.ProbeCardID;
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    //CEID 3611
    public class CardIdleEvent : NotifyEvent, IGemCommand
    {
        public CardIdleEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().StageNumber.Value = pivInfo.StageNumber;
                    this.GEMModule().GetPIVContainer().UpdateCardBufferInfo(pivInfo.CardBufferIndex);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    //CEID 9065
    public class CardIdReadDoneEvent : NotifyEvent, IGemCommand
    {
        public CardIdReadDoneEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            try
            {
                this.GEMModule().GetPIVContainer().ProbeCardID.Value = pivInfo.ProbeCardID;
                this.GEMModule().GetPIVContainer().StageNumber.Value = pivInfo.StageNumber;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    //CEID 9066
    public class CardIdReadFailEvent : NotifyEvent, IGemCommand
    {
        public CardIdReadFailEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            try
            {
                this.GEMModule().GetPIVContainer().ProbeCardID.Value = pivInfo.ProbeCardID;
                this.GEMModule().GetPIVContainer().StageNumber.Value = pivInfo.StageNumber;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    //CEID 9063
    public class CardBufferDetectedEvent : NotifyEvent, IGemCommand
    {
        public CardBufferDetectedEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().UpdateCardBufferInfo(pivInfo.CardBufferIndex);
                    this.GEMModule().GetPIVContainer().StageNumber.Value = pivInfo.StageNumber;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    //CEID 9064
    public class CardBufferRemovedEvent : NotifyEvent, IGemCommand
    {
        public CardBufferRemovedEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().UpdateCardBufferInfo(pivInfo.CardBufferIndex);
                    this.GEMModule().GetPIVContainer().StageNumber.Value = pivInfo.StageNumber;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    //CEID 9069
    public class CardBufferLoadDoneEvent : NotifyEvent, IGemCommand
    {
        public CardBufferLoadDoneEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().UpdateCardBufferInfo(pivInfo.CardBufferIndex);
                    this.GEMModule().GetPIVContainer().StageNumber.Value = pivInfo.StageNumber;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    //CEID 9070
    public class CardBufferLoadFailEvent : NotifyEvent, IGemCommand
    {
        public CardBufferLoadFailEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().UpdateCardBufferInfo(pivInfo.CardBufferIndex);
                    this.GEMModule().GetPIVContainer().StageNumber.Value = pivInfo.StageNumber;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    //CEID 9071
    public class CardBufferUnloadDoneEvent : NotifyEvent, IGemCommand
    {
        public CardBufferUnloadDoneEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().UpdateCardBufferInfo(pivInfo.CardBufferIndex);
                    this.GEMModule().GetPIVContainer().StageNumber.Value = pivInfo.StageNumber;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    //CEID 9072
    public class CardBufferUnloadFailEvent : NotifyEvent, IGemCommand
    {
        public CardBufferUnloadFailEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().UpdateCardBufferInfo(pivInfo.CardBufferIndex);
                    this.GEMModule().GetPIVContainer().StageNumber.Value = pivInfo.StageNumber;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    //CEID 8907
    public class CardLPOutOfServiceEvent : NotifyEvent, IGemCommand
    {
        public CardLPOutOfServiceEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().UpdateCardBufferInfo(pivInfo.CardBufferIndex);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    //CEID 8908
    public class CardLPInServiceEvent : NotifyEvent, IGemCommand
    {
        public CardLPInServiceEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().UpdateCardBufferInfo(pivInfo.CardBufferIndex);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    //CEID 8909
    public class CardLPTransferReady : NotifyEvent, IGemCommand
    {
        public CardLPTransferReady()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().UpdateCardBufferInfo(pivInfo.CardBufferIndex);
                    this.GEMModule().GetPIVContainer().StageNumber.Value = pivInfo.StageNumber;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    //CEID 8910
    public class CardLPTransferBlockedEvent : NotifyEvent, IGemCommand
    {
        public CardLPTransferBlockedEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().UpdateCardBufferInfo(pivInfo.CardBufferIndex);
                    this.GEMModule().GetPIVContainer().StageNumber.Value = pivInfo.StageNumber;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    //CEID 8920
    public class CardLPAccessModeOnlineEvent : NotifyEvent, IGemCommand
    {
        public CardLPAccessModeOnlineEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().SetCardBufferAccessMode(pivInfo.CardBufferIndex, true);
                    this.GEMModule().GetPIVContainer().UpdateCardBufferInfo(pivInfo.CardBufferIndex);
                    this.GEMModule().GetPIVContainer().StageNumber.Value = pivInfo.StageNumber;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    //CEID 8921
    public class CardLPAccessModeManualEvent : NotifyEvent, IGemCommand
    {
        public CardLPAccessModeManualEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().SetCardBufferAccessMode(pivInfo.CardBufferIndex, false);
                    this.GEMModule().GetPIVContainer().UpdateCardBufferInfo(pivInfo.CardBufferIndex);
                    this.GEMModule().GetPIVContainer().StageNumber.Value = pivInfo.StageNumber;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    //CEID 8911
    public class CardLPReadyToLoadEvent : NotifyEvent, IGemCommand
    {
        public CardLPReadyToLoadEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().UpdateCardBufferInfo(pivInfo.CardBufferIndex);
                    this.GEMModule().GetPIVContainer().StageNumber.Value = pivInfo.StageNumber;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    //CEID 8912
    public class CardLPReadyToUnloadEvent : NotifyEvent, IGemCommand
    {
        public CardLPReadyToUnloadEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().UpdateCardBufferInfo(pivInfo.CardBufferIndex);
                    this.GEMModule().GetPIVContainer().StageNumber.Value = pivInfo.StageNumber;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    //CEID 8913
    public class CardLPReadyToUnloadNewCardEvent : NotifyEvent, IGemCommand
    {
        public CardLPReadyToUnloadNewCardEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().UpdateCardBufferInfo(pivInfo.CardBufferIndex);
                    this.GEMModule().GetPIVContainer().StageNumber.Value = pivInfo.StageNumber;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    //CEID 3610
    public class CardBusyEvent : NotifyEvent, IGemCommand
    {
        public CardBusyEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().ProbeCardID.Value = pivInfo.ProbeCardID;
                    this.GEMModule().GetPIVContainer().StageNumber.Value = pivInfo.StageNumber;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class CardChangeVaildationStartEvent : NotifyEvent
    {
        public CardChangeVaildationStartEvent()
        {
        }
    }
    public class AlarmSVChangedEvent : NotifyEvent
    {
        public AlarmSVChangedEvent()
        {
        }
    }

    public class RetestStartPreEvent : NotifyEvent
    {
        public RetestStartPreEvent()
        {
        }
    }

    //CEID 3604
    public class CardChangedEvent : NotifyEvent
    {
        public CardChangedEvent()
        {
        }
    }

    public class CardChangeFailEvent : NotifyEvent
    {

    }
    public class TesterHeadDockingEvent : NotifyEvent
    {
        public TesterHeadDockingEvent()
        {
        }
    }

    public class TesterHeadUndockingEvent : NotifyEvent
    {
        public TesterHeadUndockingEvent()
        {
        }
    }

    public class CleanSheetChangeEvent : NotifyEvent
    {
        public CleanSheetChangeEvent()
        {
        }
    }

    public class SetTemporatureChangeEvent : NotifyEvent
    {
        public SetTemporatureChangeEvent()
        {
        }
    }

    public class FrontDoorOpenEvent : NotifyEvent
    {
        public FrontDoorOpenEvent()
        {
        }
    }

    public class StopFrontDoorTempEvent : NotifyEvent
    {
        public StopFrontDoorTempEvent()
        {
        }
    }

    public class SetFrontDoorTempEvent : NotifyEvent
    {
        public SetFrontDoorTempEvent()
        {
        }
    }

    public class LoaderSideCoverOpenEvent : NotifyEvent
    {
        public LoaderSideCoverOpenEvent()
        {
        }
    }

    public class InspectionTrayOpenEvent : NotifyEvent
    {
        public InspectionTrayOpenEvent()
        {
        }
    }

    public class MoveToNextDiePreEvent : NotifyEvent
    {
        public MoveToNextDiePreEvent()
        {
        }
    }

    public class FirstTimeAfterMachineStarted : NotifyEvent
    {
        public FirstTimeAfterMachineStarted()
        {
        }
    }

    public class ProberStatusEvent : NotifyEvent
    {
        public ProberStatusEvent()
        {

        }
    }

    public class TesterConnectedEvent : NotifyEvent
    {
        public TesterConnectedEvent()
        {
        }
    }
    public class LoaderConnectedEvent : NotifyEvent
    {
        public LoaderConnectedEvent()
        {
        }
    }

    public class FoupAllocatedEvent : NotifyEvent
    {
        public FoupAllocatedEvent()
        {
        }
    }


    public class TesterDisonnectedEvent : NotifyEvent
    {
        public TesterDisonnectedEvent()
        {
        }
    }

    public class ParameterVerifySuccessEvent : NotifyEvent, IGemCommand
    {
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.GEMModule().GetPIVContainer().SetFoupInfo(pivInfo.FoupNumber, lotid: pivInfo.LotID);
                this.GEMModule().GetPIVContainer().UpdateFoupInfo(pivInfo.FoupNumber);
                this.GEMModule().GetPIVContainer().VerifyParamResultMap.Value = pivInfo.VerifyParamResultMap;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            try
            {
                this.GEMModule().GetPIVContainer().VerifyParamResultMap.Value = "";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    public class ParameterVerifyFailEvent : NotifyEvent, IGemCommand
    {
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.GEMModule().GetPIVContainer().UpdateFoupInfo(pivInfo.FoupNumber);
                this.GEMModule().GetPIVContainer().VerifyParamResultMap.Value = pivInfo.VerifyParamResultMap;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            try
            {
                this.GEMModule().GetPIVContainer().VerifyParamResultMap.Value = "";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    #region TCPIP

    //public class TCPIPConnectionStartEvent : NotifyEvent
    //{
    //    public TCPIPConnectionStartEvent()
    //    {
    //    }
    //}

    #region Emulator를 위한 클래스

    public class Response_ZupDone : NotifyEvent
    {
        public Response_ZupDone()
        {
        }
    }

    public class Response_MoveToNextIndexDone : NotifyEvent
    {
        public Response_MoveToNextIndexDone()
        {
        }
    }

    public class Response_UnloadWaferDone : NotifyEvent
    {
        public Response_UnloadWaferDone()
        {
        }
    }

    public class Response_StopAndAlarmDone : NotifyEvent
    {
        public Response_StopAndAlarmDone()
        {
        }
    }

    public class Response_BINCodeDone : NotifyEvent
    {
        public Response_BINCodeDone()
        {
        }
    }

    public class Response_HardwareAssemblyVerifyDone : NotifyEvent
    {
        public Response_HardwareAssemblyVerifyDone()
        {
        }
    }

    public class Response_LotProcessingVerifyDone : NotifyEvent
    {
        public Response_LotProcessingVerifyDone()
        {
        }
    }

    //public class Response_DWDone : NotifyEvent
    //{
    //    public Response_DWDone()
    //    {
    //    }
    //}

    #endregion


    public class ProberErrorEvent : NotifyEvent
    {
        public ProberErrorEvent()
        {

        }
    }

    public class ManualProbingEvent : NotifyEvent
    {
        public ManualProbingEvent()
        {

        }
    }

    #endregion

    #region Socking
    public class DoPreHeatSoaking : NotifyEvent
    {
        public DoPreHeatSoaking()
        {

        }
    }
    #endregion

    #region Stage

    public class MachineInitCompletedEvent : NotifyEvent
    {
        public MachineInitCompletedEvent()
        {
        }
    }

    public class StageMachineInitCompletedEvent : NotifyEvent
    {
        public StageMachineInitCompletedEvent()
        {
        }
    }

    public class MachineInitOnEvent : NotifyEvent
    {
        public MachineInitOnEvent()
        {
        }
    }

    #endregion

    #region Mark Alignment

    public class MarkAlignmentStart : NotifyEvent
    {

    }

    public class MarkAlignmentDone : NotifyEvent
    {

    }

    public class MarkAlignmentDoneBeforePinAlignment : NotifyEvent, IGemCommand
    {
        public MarkAlignmentDoneBeforePinAlignment()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING
                    || this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSED)
                {
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = EventCodeEnum.LOT_IS_NOT_RUNNING;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class MarkAlignmentDoneBeforeWaferAlignment : NotifyEvent, IGemCommand
    {
        public MarkAlignmentDoneBeforeWaferAlignment()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING
                    || this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSED)
                {
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = EventCodeEnum.LOT_IS_NOT_RUNNING;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class MarkAlignmentDoneAfterWaferAlignment : NotifyEvent, IGemCommand
    {
        public MarkAlignmentDoneAfterWaferAlignment()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AfterSetPIV(PIVInfo pivInfo)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING
                    || this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSED)
                {
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = EventCodeEnum.LOT_IS_NOT_RUNNING;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    #endregion

    #region SignalTower Event

    #region Cell State Event
    public class CellRunningStateEvent : NotifyEvent, ISignalTowerNotify
    {
        public CellRunningStateEvent()
        {

        }
    }

    public class CellPausedStateEvent : NotifyEvent, ISignalTowerNotify
    {
        public CellPausedStateEvent()
        {

        }
    }

    public class CellIdleStateEvent : NotifyEvent, ISignalTowerNotify
    {
        public CellIdleStateEvent()
        {

        }
    }
    #endregion
    #region Loader State Event
    public class LoaderIdleStateEvent : NotifyEvent, ISignalTowerNotify
    {
        public LoaderIdleStateEvent()
        {

        }
    }
    public class LoaderPausedStateEvent : NotifyEvent, ISignalTowerNotify
    {
        public LoaderPausedStateEvent()
        {

        }
    }
    public class LoaderRunningStateEvent : NotifyEvent, ISignalTowerNotify
    {
        public LoaderRunningStateEvent()
        {

        }
    }
    public class BuzzerOffStateEvent : NotifyEvent, ISignalTowerNotify
    {
        public BuzzerOffStateEvent()
        {

        }
    }
    #endregion
    #region Foup State Event
    public class CassetteRequestEvent : NotifyEvent, ISignalTowerNotify
    {
        public CassetteRequestEvent()
        {

        }
    }

    public class CassetteRemoveRequestEvent : NotifyEvent, ISignalTowerNotify
    {
        public CassetteRemoveRequestEvent()
        {

        }
    }
    public class CassetteAttachEvent : NotifyEvent, ISignalTowerNotify
    {
        public CassetteAttachEvent()
        {

        }
    }

    public class CassetteDetachEvent : NotifyEvent, ISignalTowerNotify
    {
        public CassetteDetachEvent()
        {

        }
    }
    public class LoadPortAccessViolation : NotifyEvent, ISignalTowerNotify
    {
        public LoadPortAccessViolation()
        {

        }
    }
    #endregion
    #region Loader System Error Event    
    public class LoaderMainAirErrorEvent : NotifyEvent, ISignalTowerNotify
    {
        public LoaderMainAirErrorEvent()
        {

        }
    }
    public class LoaderMainAirSuccessEvent : NotifyEvent, ISignalTowerNotify
    {
        public LoaderMainAirSuccessEvent()
        {

        }
    }    
    public class StageMainAirErrorEvent : NotifyEvent, ISignalTowerNotify
    {
        public StageMainAirErrorEvent()
        {

        }
    }
    public class StageMainAirSuccessEvent : NotifyEvent, ISignalTowerNotify
    {
        public StageMainAirSuccessEvent()
        {

        }
    }
    public class LoaderMainVacErrorEvent : NotifyEvent, ISignalTowerNotify
    {
        public LoaderMainVacErrorEvent()
        {

        }
    }
    public class LoaderMainVacSuccessEvent : NotifyEvent, ISignalTowerNotify
    {
        public LoaderMainVacSuccessEvent()
        {

        }
    }
    public class StageMainVacErrorEvent : NotifyEvent, ISignalTowerNotify
    {
        public StageMainVacErrorEvent()
        {

        }
    }
    public class StageMainVacSuccessEvent : NotifyEvent, ISignalTowerNotify
    {
        public StageMainVacSuccessEvent()
        {

        }
    }
    public class LoaderEMOErrorEvent : NotifyEvent, ISignalTowerNotify
    {
        public LoaderEMOErrorEvent()
        {

        }
    }
    public class LoaderEMOSuccessEvent : NotifyEvent, ISignalTowerNotify
    {
        public LoaderEMOSuccessEvent()
        {

        }
    }
    public class LoaderErrorStateEvent : NotifyEvent, ISignalTowerNotify
    {
        public LoaderErrorStateEvent()
        {

        }
    }
    public class LoaderMachineInitCompletedEvent : NotifyEvent, ISignalTowerNotify
    {
        public LoaderMachineInitCompletedEvent()
        {

        }
    }
    #endregion    
    #endregion
}
