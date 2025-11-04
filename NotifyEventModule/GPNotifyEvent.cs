using ProberErrorCode;
using System;

namespace NotifyEventModule
{
    using LogModule;
    using ProberInterfaces;

    #region //..RemoteCommandEvent

    /// <summary>
    /// Host requests equipment to download stage specific recipes for the instructed lot
    /// </summary>
    public class DownloadStageRecipeEvent : NotifyEvent
    {
        public DownloadStageRecipeEvent()
        {

        }
    }

    /// <summary>
    /// Host activates a lot process sequence for the instructed lot on the load port
    /// </summary>
    public class ActivateProcessEvent : NotifyEvent
    {
        public ActivateProcessEvent()
        {

        }
    }

    /// <summary>
    /// Host specifies which wafers need to be probed by indicating slots of the carrier
    /// </summary>
    public class SelectSlotsEvent : NotifyEvent
    {
        public SelectSlotsEvent()
        {

        }
    }

    public class ScanCassetteStart : NotifyEvent
    {

    }

    public class ScanCassetteEnd : NotifyEvent
    {

    }
    /// <summary>
    /// Host requests equipment to set parameter values for the instructed recipe
    /// </summary>
    public class SetParameetersEvent : NotifyEvent
    {
        public SetParameetersEvent()
        {

        }
    }
    #endregion



    /// <summary>
    /// Loadport set to an active state
    /// </summary>
    public class LoadportActivatedEvent : NotifyEvent, IGemCommand
    {
        public LoadportActivatedEvent()
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
    /// Tool succeeded in downloading the requested recipe file
    /// </summary>
    public class RecipeDownloadStartEvent : NotifyEvent, IGemCommand
    {
        public RecipeDownloadStartEvent()
        {

        }

        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    IPIVContainer pIVContainer = this.GEMModule().GetPIVContainer();
                    pIVContainer.RecipeID.Value = pivInfo.RecipeID;
                    if (pivInfo.FoupNumber > 0)
                    {
                        pIVContainer.RecipeID.Value = pivInfo.RecipeID;
                    }
                    else
                    {                        
                        pIVContainer.UpdateStageLotInfo(pivInfo.FoupNumber, pivInfo.LotID);
                    }
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
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.GEMModule().GetPIVContainer().FoupNumber.Value = pivInfo.OriginLoadFoupNumber;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    /// <summary>
    /// Tool succeeded in downloading the requested recipe file
    /// </summary>
    public class RecipeDownloadSucceededEvent : NotifyEvent, IGemCommand
    {
        public RecipeDownloadSucceededEvent()
        {

        }

        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    IPIVContainer pIVContainer = this.GEMModule().GetPIVContainer();
                    pIVContainer.UpdateStageLotInfo(pivInfo.FoupNumber, pivInfo.LotID);
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
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.GEMModule().GetPIVContainer().FoupNumber.Value = pivInfo.OriginLoadFoupNumber;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    /// <summary>
    /// Tool failed to download the requested recipe file
    /// </summary>
    public class RecipeDownloadFailedEvent : NotifyEvent, IGemCommand
    {
        public RecipeDownloadFailedEvent()
        {

        }

        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    IPIVContainer pIVContainer = this.GEMModule().GetPIVContainer();
                    pIVContainer.UpdateStageLotInfo(pivInfo.FoupNumber, pivInfo.LotID);
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
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.GEMModule().GetPIVContainer().FoupNumber.Value = pivInfo.OriginLoadFoupNumber;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }


    /// <summary>
    /// Tool reports download results by stage recipe file
    /// </summary>
    public class StageRecipeDownloadEvent : NotifyEvent, IGemCommand
    {
        public StageRecipeDownloadEvent()
        {

        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    IPIVContainer pIVContainer = this.GEMModule().GetPIVContainer();
                    pIVContainer.UpdateStageLotInfo(pivInfo.FoupNumber, pivInfo.LotID);
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
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.GEMModule().GetPIVContainer().FoupNumber.Value = pivInfo.OriginLoadFoupNumber;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    /// <summary>
    /// Tool succeeded in downloading the requested stage recipe file into the buffer
    /// </summary>
    public class StageRecipeDownloadSuccededEvent : NotifyEvent, IGemCommand
    {
        public StageRecipeDownloadSuccededEvent()
        {

        }

        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    IPIVContainer pIVContainer = this.GEMModule().GetPIVContainer();
                    pIVContainer.UpdateStageLotInfo(pivInfo.FoupNumber, pivInfo.LotID);
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
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.GEMModule().GetPIVContainer().FoupNumber.Value = pivInfo.OriginLoadFoupNumber;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    /// <summary>
    /// Tool failed to download the requested stage recipe file into the buffer
    /// </summary>
    public class StageRecipeDownloadFailedEvent : NotifyEvent, IGemCommand
    {
        public StageRecipeDownloadFailedEvent()
        {

        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    IPIVContainer pIVContainer = this.GEMModule().GetPIVContainer();
                    pIVContainer.UpdateStageLotInfo(pivInfo.FoupNumber, pivInfo.LotID);
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
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.GEMModule().GetPIVContainer().FoupNumber.Value = pivInfo.OriginLoadFoupNumber;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }


    public class StageRecipeReadStartEvent : NotifyEvent, IGemCommand
    {
        public StageRecipeReadStartEvent()
        {

        }

        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    IPIVContainer pIVContainer = this.GEMModule().GetPIVContainer();

                    if (pivInfo.FoupNumber == 0)
                    {
                        pIVContainer.RecipeID.Value = pivInfo.RecipeID;
                    }
                    else
                    {
                        pIVContainer.UpdateStageLotInfo(pivInfo.FoupNumber, pivInfo.LotID);
                    }
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

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }

    }

    /// <summary>
    /// Stage succeded in reading the requested stage recipe
    /// </summary>
    public class StageRecipeReadCompleteEvent : NotifyEvent, IGemCommand
    {
        public StageRecipeReadCompleteEvent()
        {

        }

        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    IPIVContainer pIVContainer = this.GEMModule().GetPIVContainer();
                    pIVContainer.RecipeID.Value = pivInfo.RecipeID;
                    if (pivInfo.FoupNumber > 0)
                    {
                        pIVContainer.UpdateStageLotInfo(pivInfo.FoupNumber, pivInfo.LotID);
                    }
                    
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
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.GEMModule().GetPIVContainer().FoupNumber.Value = pivInfo.OriginLoadFoupNumber;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }

    }

    /// <summary>
    /// Stage failed to read the requested stage recipe
    /// </summary>
    public class StageRecipeReadFailedEvent : NotifyEvent, IGemCommand
    {
        public StageRecipeReadFailedEvent()
        {

        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    IPIVContainer pIVContainer = this.GEMModule().GetPIVContainer();
                    pIVContainer.RecipeID.Value = pivInfo.RecipeID;
                    if (pivInfo.FoupNumber > 0)
                    {
                        pIVContainer.UpdateStageLotInfo(pivInfo.FoupNumber, pivInfo.LotID);
                    }
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
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.GEMModule().GetPIVContainer().FoupNumber.Value = pivInfo.OriginLoadFoupNumber;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    /// <summary>
    /// Loadport to select slots
    /// </summary>
    public class SlotsSelectedEvent : NotifyEvent, IGemCommand
    {
        public SlotsSelectedEvent()
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
                    this.GEMModule().GetPIVContainer().SetFoupState(foupnumber, GEMFoupStateEnum.SLOT_SELECTED);
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
    /// Loadport set to a processing state
    /// </summary>
    public class FoupProcessingStartEvent : NotifyEvent, IGemCommand
    {
        public FoupProcessingStartEvent()
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
                    this.GEMModule().GetPIVContainer().SetFoupState(foupnumber, GEMFoupStateEnum.PROCESSING);
                    this.GEMModule().GetPIVContainer().SetFoupInfo(foupnumber, lotid: pivInfo.LotID);
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
    /// Stage is allocated to a wafer processing and starts alignment, etc.
    /// </summary>
    public class StageAllocatedEvent : NotifyEvent, IGemCommand
    {
        public StageAllocatedEvent()
        {

        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.LotOPModule().LotInfo.ProcessedWaferCnt == 0)
                {
                    this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.PROCESSING);
                }
                else
                {
                    this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.NEXT_WAFER_PREPRCOESSING);
                }

                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().UpdateStageLotInfo(pivInfo.FoupNumber);
                }
                else
                {
                    // TODO :
                    LoggerManager.Error($"[{this.GetType().Name}], StageAllocatedEvent() : pivInfo is null.");
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
    public class WaferLoadedInCurrentLotEvent : NotifyEvent, IGemCommand
    {
        public WaferLoadedInCurrentLotEvent()
        {

        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.LotOPModule().LotInfo.ProcessedWaferCnt == 0)
                {
                    this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.PROCESSING);
                }
                else
                {
                    this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.NEXT_WAFER_PREPRCOESSING);
                }

                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().UpdateStageLotInfo(pivInfo.FoupNumber);
                }
                else
                {
                    // TODO :
                    LoggerManager.Error($"[{this.GetType().Name}], WaferLoadedInCurrentLotEvent() : pivInfo is null.");
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
    /// Stage receives next wafer of the current lot
    /// </summary>
    public class StageStartedPreprocessingEvent : NotifyEvent
    {

    }

    /// <summary>
    /// Stage had no more wafer need to process.
    /// </summary>
    public class StageDeallocatedEvent : NotifyEvent, IGemCommand
    {
        public StageDeallocatedEvent()
        {

        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //this.GEMModule().GetPIVContainer().SetStageLotInfo(this.GEMModule().GetPIVContainer().FoupNumber.Value,
                //    lotid: this.GEMModule().GetPIVContainer().GetLotID()); // #Hynix_Merge: 검토 필요, Hynix는 이 코드 처럼 되어있었음. YMTC, MICRON 모두 확인 필요.

                //YMTC
                //this.GEMModule().GetPIVContainer().SetStageLotInfo(this.GEMModule().GetPIVContainer().LoadFoupNumber.Value,
                //    lotid: this.GEMModule().GetPIVContainer().GetLotID());
                //MICRON
                

                this.GEMModule().GetPIVContainer().SetLotID("");
                this.LotOPModule().UpdateLotName(string.Empty);
                this.GEMModule().GetPIVContainer().XCoordinate.Value = -9999;
                this.GEMModule().GetPIVContainer().YCoordinate.Value = -9999;
                //this.GEMModule().GetPIVContainer().FoupNumber.Value = 0;
                this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.IDLE);
                this.LotOPModule().LotInfo.RemoveLotInfo((pivInfo == null) ? 0 : pivInfo.FoupNumber);
                this.LotOPModule().LotInfo.isNewLot = true;
                this.GEMModule().GetPIVContainer().FoupNumber.Value = 0;

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
            this.GEMModule().GetPIVContainer().SetLotID(" ");
            this.LoaderController().IsCancel = false;
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }

    }
    /// <summary>
    /// Stage set to a ready to Z up position
    /// </summary>
    public class MatchedToTestEvent : NotifyEvent, IGemCommand
    {
        public MatchedToTestEvent()
        {

        }

        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //this.GEMModule().GetPIVContainer().SetProberCardID(this.CardChangeModule().GetProbeCardID());
                this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.READY_Z_UP);
                this.GEMModule().GetPIVContainer().UpdateStageLotInfo(pivInfo.FoupNumber);

                //#Hynix_Merge: 위의 코드 외에 Hynix에서 업데이트 하고 있던 값들, 꼭 필요한지 확인 필요.
                this.GEMModule().GetPIVContainer().XCoordinate.Value = pivInfo.XCoordinate;
                this.GEMModule().GetPIVContainer().YCoordinate.Value = pivInfo.YCoordinate;
                this.GEMModule().GetPIVContainer().CurTemperature.Value = pivInfo.CurTemperature;
                this.GEMModule().GetPIVContainer().SetTemperature.Value = pivInfo.SetTemperature;


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
                    retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class MatchedToTestFirstProcessEvent : NotifyEvent, IGemCommand
    {
        public MatchedToTestFirstProcessEvent()
        {

        }

        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //this.GEMModule().GetPIVContainer().SetProberCardID(this.CardChangeModule().GetProbeCardID());
                this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.READY_Z_UP);
                this.GEMModule().GetPIVContainer().UpdateStageLotInfo(pivInfo.FoupNumber);

                //#Hynix_Merge: 위의 코드 외에 Hynix에서 업데이트 하고 있던 값들, 꼭 필요한지 확인 필요.
                this.GEMModule().GetPIVContainer().XCoordinate.Value = pivInfo.XCoordinate;
                this.GEMModule().GetPIVContainer().YCoordinate.Value = pivInfo.YCoordinate;
                this.GEMModule().GetPIVContainer().CurTemperature.Value = pivInfo.CurTemperature;
                this.GEMModule().GetPIVContainer().SetTemperature.Value = pivInfo.SetTemperature;


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

    /// <summary>
    /// Stage starts next wafer
    /// </summary>
    public class NextWaferPreprocessingEvent : NotifyEvent
    {
        public NextWaferPreprocessingEvent()
        {

        }
    }

    /// <summary>
    /// Operator changes machine to online.
    /// </summary>
    public class StageOnlineEvent : NotifyEvent, IGemCommand
    {
        public StageOnlineEvent()
        {

        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.ONLINE);
                this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.AVAILABLE);
                this.GEMModule().GetPIVContainer().SetProberCardID(this.CardChangeModule().GetProbeCardID());
                this.GEMModule().GetPIVContainer().SetDeviceName(this.StageSupervisor().GetDeviceName());
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

    //CEID 6990
    public class TcwMoveCompleteEvent : NotifyEvent, IGemCommand
    {
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            try
            {
                //v22_merge// setwaferid, setdevicename 추가// this.GEMModule().GetPIVContainer().SetStageLotInfo(pivInfo.FoupNumber, waferid: pivInfo.WaferID, recipeid: pivInfo.RecipeID);
                this.GEMModule().GetPIVContainer().SetWaferID(pivInfo.WaferID);
                this.GEMModule().GetPIVContainer().SetDeviceName(pivInfo.RecipeID);
                this.GEMModule().GetPIVContainer().UpdateStageLotInfo(pivInfo.FoupNumber);
                this.GEMModule().GetPIVContainer().CurTemperature.Value = pivInfo.CurTemperature;
                this.GEMModule().GetPIVContainer().SetTemperature.Value = pivInfo.SetTemperature;
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

    /// <summary>
    /// Stage chuck toucked down to next index.
    /// </summary>
    public class StageAddressedNextIndexEvnet : NotifyEvent
    {
        public StageAddressedNextIndexEvnet()
        {

        }
    }


    /// <summary>
    /// Stage canceled the wafer process due to alignment error, etc.
    /// </summary>
    public class WaferCancelledBeforeProbing : NotifyEvent
    {
        public WaferCancelledBeforeProbing()
        {

        }
    }

    /// <summary>
    /// Operator changes machine to offline.
    /// </summary>
    public class StageOffineEvent : NotifyEvent, IGemCommand
    {
        public StageOffineEvent()
        {

        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.OFFLINE);
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

    /// <summary>
    /// Stage is ready to be re-introduced to the lot process.
    /// </summary>
    public class StageReadyToEnterToProcessEvent : NotifyEvent, IGemCommand
    {
        public StageReadyToEnterToProcessEvent()
        {

        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
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

    /// <summary>
    /// Vacuum error or similar happens on the stage. Stage waits for and error end command from cell host.
    /// </summary>
    public class StageErrorEvent : NotifyEvent
    {
        public StageErrorEvent()
        {
        }
    }

    public class StageChuckVacuumErrorEvent : NotifyEvent
    {

    }

    /// <summary>
    /// Stage receives a error end or carrier end command from cell host.
    /// </summary>
    public class WaferTestingAborted : NotifyEvent, IGemCommand
    {
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.GEMModule().GetPIVContainer().XCoordinate.Value = -9999;
                this.GEMModule().GetPIVContainer().YCoordinate.Value = -9999;

                //#PAbort // 이부분 Dev_Integrated랑 다름.
                this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.UNLOADING);// MICRON
                this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.UNAVAILABLE);// MPT
                //#Hynix_Merge: 검토 필요, 이부분 YMNTC 에서 StageState값을 같이써서 0으로 만들어 버릴 수도 있음. YMTC에서는 ErrorEnd 개념이 없었나..?
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().UpdateStageLotInfo(pivInfo.FoupNumber);
                }
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
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (this.StageSupervisor().WaferObject.GetStatus() == EnumSubsStatus.EXIST)
                {
                    //this.StageSupervisor().WaferObject.SetSkipped(); //MICRON, Unload 안하고 Zdown 만 한 상태일 수도 있는데 무조건 Skipped 처리하는건 이상한것 같음. Cancel에서도 불리기때문엥 다른셀이 Cancel 불렀을수도 있으니강. 
                    //#PAbort // 이부분 Dev_Integrated랑 다름.

                    if (this.StageSupervisor().WaferObject.GetState() != EnumWaferState.PROBING &&
                       this.StageSupervisor().WaferObject.GetState() != EnumWaferState.TESTED &&
                       this.StageSupervisor().WaferObject.GetState() != EnumWaferState.PROCESSED)
                    {
                        this.StageSupervisor().WaferObject.SetWaferState(EnumWaferState.SKIPPED);
                    }
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.GEM_PIV_SET_EXCEPTION;
            }

            return retVal;

        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    /// <summary>
    /// Stage forced to pause when testing by a user
    /// </summary>
    public class WaferTestingAbortedByUser : NotifyEvent
    {

    }

    /// <summary>
    /// Stage forced to pause by a user
    /// </summary>
    public class CellLotAbortedByUser : NotifyEvent
    {

    }


  

    /// <summary>
    /// When there is a problem with the stage so stage want to lot end
    /// </summary>
    public class RequestLotEnd : NotifyEvent
    {

    }

    /// <summary>
    /// Stage received test end comand from cell host and moved to z down position for next index
    /// </summary>
    public class StageSeparatedForNextIndexEvent : NotifyEvent
    {
    }

    /// <summary>
    /// Stage canceled the wafer process by receiving error end or cancel carrier command.
    /// </summary>
    public class WaferTestingCanceledCanceledByHostEvent : NotifyEvent, IGemCommand
    {
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().XCoordinate.Value = -9999;
                    this.GEMModule().GetPIVContainer().YCoordinate.Value = -9999;
                    this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.UNLOADING);
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
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //#Hynix_Merge
                if (this.StageSupervisor().WaferObject.GetState() != EnumWaferState.PROBING &&
                    this.StageSupervisor().WaferObject.GetState() != EnumWaferState.TESTED &&
                   this.StageSupervisor().WaferObject.GetState() != EnumWaferState.PROCESSED)
                {
                    this.StageSupervisor().WaferObject.SetWaferState(EnumWaferState.SKIPPED);
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum PreCheck()
        {
            return EventCodeEnum.NONE;
        }
    }

    /// <summary>
    /// The operator changes the stage to set up from maintenance
    /// </summary>
    public class SetupStartEvent : NotifyEvent
    {
    }

    /// <summary>
    /// The operator changes the stage to usable
    /// </summary>
    public class SetupEndEvent : NotifyEvent, IGemCommand
    {
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

    /// <summary>
    /// events that occur when there is a temperature change above the change value set at stage temperature.
    /// ex) change value = 0.1℃ : event occurs whenever the temperature changes 0.1 degrees.
    /// </summary>
    public class TempMonitorChangeValueEvent : NotifyEvent, IGemCommand
    {
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().CurTemperature.Value = pivInfo.CurTemperature;
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

    /// <summary>
    /// Stage issues this event when measured chuck temperature moves out of the predefined delta value (device deviation)
    /// </summary>
    public class TempMoveOutofRangeEvent : NotifyEvent, IGemCommand
    {
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().CurTemperature.Value = pivInfo.CurTemperature;
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

    /// <summary>
    /// Stage issues this event when measured chuck temperature moves back into the range (device deviation)
    /// </summary>
    public class TempMoveIntoRangeEvent : NotifyEvent, IGemCommand
    {
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().CurTemperature.Value = pivInfo.CurTemperature;
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

    /// <summary>
    /// Stage issues this event when measured chuck temperature moves out of the predefined delta value (alarm deviation)
    /// alarm deviation 이 설정되어 있는 경우, pv 가 sv에 대해 설정되 deviaiton 이상 차이나면 이벤트 발생.
    /// </summary>
    public class TempMoveOutofDeviationRangeDuringProbingEvent : NotifyEvent, IGemCommand
    {
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().CurTemperature.Value = pivInfo.CurTemperature;
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

    /// <summary>
    /// Stage issues this event when measured chuck temperature moves back into the range (alarm deviation)
    /// alarm deviation 이 설정되어 있는 경우, pv 가 sv에 대해 설정되 deviaiton 이내 범위로 돌아오면 이벤트 발생
    /// </summary>
    public class TempMoveInofDeviationRangeDuringProbingEvent : NotifyEvent, IGemCommand
    {
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().CurTemperature.Value = pivInfo.CurTemperature;
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

    public class PinAlignStartEvent : NotifyEvent, IGemCommand
    {
        public PinAlignStartEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                var pivcontainer = this.GEMModule().GetPIVContainer();
                string lotname = this.LotOPModule().LotInfo.LotName.Value;
                pivcontainer.SetLotID(lotname);
                pivcontainer.UpdateStageLotInfo(this.LotOPModule().LotInfo.GetFoupNumbetAtStageLotInfos(lotname));// Lot할당 받을때 FoupNumber 사용

                var foupnum = this.LotOPModule().LotInfo.GetFoupNumbetAtStageLotInfos(lotname);
                if (foupnum > 0)
                {
                    //v22_merge// setoverdrive, set waferid 추가// pivcontainer.SetStageLotInfo(foupnum, lotid: lotname, overdrive: this.ProbingModule().OverDrive, waferid: this.GetParam_Wafer().GetSubsInfo().WaferID.Value);
                    pivcontainer.SetOverDrive(this.ProbingModule().OverDrive);
                    pivcontainer.SetWaferID(this.GetParam_Wafer().GetSubsInfo().WaferID.Value);
                    pivcontainer.UpdateStageLotInfo(foupnum);
                }
                else
                {
                    //pivcontainer.SetStageLotInfo(foupnum, lotid: lotname);
                    pivcontainer.SetOverDrive(this.ProbingModule().OverDrive);
                    pivcontainer.SetWaferID(this.GetParam_Wafer().GetSubsInfo().WaferID.Value);
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

    public class PinAlignEndEvent : NotifyEvent, IGemCommand
    {
        public PinAlignEndEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var pivcontainer = this.GEMModule().GetPIVContainer();
                string lotname = this.LotOPModule().LotInfo.LotName.Value;
                pivcontainer.SetLotID(lotname);

                pivcontainer.UpdateStageLotInfo(this.LotOPModule().LotInfo.GetFoupNumbetAtStageLotInfos(lotname));// Lot할당 받을때 FoupNumber 사용

                pivcontainer.PinAlignPlanarity.Value = pivInfo.PinAlignPlanarity;
                pivcontainer.PinAlignAngle.Value = pivInfo.PinAlignAngle;
                pivcontainer.PinAlignCardCenterX.Value = pivInfo.PinAlignCardCenterX;
                pivcontainer.PinAlignCardCenterY.Value = pivInfo.PinAlignCardCenterY;
                pivcontainer.PinAlignCardCenterZ.Value = pivInfo.PinAlignCardCenterZ;
                pivcontainer.SetPinAlignResults(pivInfo.PinAlignResults);
                var foupnum = this.LotOPModule().LotInfo.GetFoupNumbetAtStageLotInfos(lotname);
                if (foupnum > 0)
                {
                    pivcontainer.UpdateStageLotInfo(foupnum);
                }
                else
                {
                    pivcontainer.Overdrive.Value = this.ProbingModule().OverDrive;
                    pivcontainer.WaferID.Value = this.GetParam_Wafer().GetSubsInfo().WaferID.Value;                    
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

    public class PinAlignFailEvent : NotifyEvent, IGemCommand
    {
        public PinAlignFailEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var pivcontainer = this.GEMModule().GetPIVContainer();
                string lotname = this.LotOPModule().LotInfo.LotName.Value;
                pivcontainer.SetLotID(lotname);
                pivcontainer.UpdateStageLotInfo(this.LotOPModule().LotInfo.GetFoupNumbetAtStageLotInfos(lotname));// Lot할당 받을때 FoupNumber 사용

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

    public class WaferAlignStartEvent : NotifyEvent, IGemCommand
    {
        public WaferAlignStartEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.GEMModule().GetPIVContainer().UpdateStageLotInfo(pivInfo.FoupNumber);
                this.GEMModule().GetPIVContainer().Overdrive.Value = this.ProbingModule().OverDrive;
                this.GEMModule().GetPIVContainer().WaferID.Value = this.GetParam_Wafer().GetSubsInfo().WaferID.Value;

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
    public class WaferAlignEndEvent : NotifyEvent, IGemCommand
    {
        public WaferAlignEndEvent()
        {
        }
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.GEMModule().GetPIVContainer().UpdateStageLotInfo(pivInfo.FoupNumber);
                this.GEMModule().GetPIVContainer().Overdrive.Value = this.ProbingModule().OverDrive;
                this.GEMModule().GetPIVContainer().WaferID.Value = this.GetParam_Wafer().GetSubsInfo().WaferID.Value;

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

    public class WaferAlignFailEvent : NotifyEvent
    {

    }

    /// <summary>
    /// Stage starts PMI
    /// </summary>
    public class PMIStartEvent : NotifyEvent, IGemCommand
    {
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            var pivcontainer = this.GEMModule().GetPIVContainer();

            string lotname = this.LotOPModule().LotInfo.LotName.Value;
            var foupnum = this.LotOPModule().LotInfo.GetFoupNumbetAtStageLotInfos(lotname);

            if (foupnum > 0)
            {
                pivcontainer.UpdateStageLotInfo(foupnum);
            }
            else
            {
                pivcontainer.Overdrive.Value = this.ProbingModule().OverDrive;
                pivcontainer.WaferID.Value = this.GetParam_Wafer().GetSubsInfo().WaferID.Value;
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

    /// <summary>
    /// Stage ends PMI
    /// </summary>
    public class PMIEndEvent : NotifyEvent, IGemCommand
    {
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            var pivcontainer = this.GEMModule().GetPIVContainer();
            pivcontainer.UpdateStageLotInfo(pivcontainer.FoupNumber.Value);

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

    public class PMIFailEvent : NotifyEvent
    {

    }

    /// <summary>
    /// Stage starts cleaning
    /// </summary>
    public class CleaningStartEvent_PolishWafer : NotifyEvent, IGemCommand
    {
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

    /// <summary>
    /// Stage ends cleaning
    /// </summary>
    public class CleaningEndEvent_PolishWafer : NotifyEvent, IGemCommand
    {
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

    public class CleaningFailEvent : NotifyEvent
    {

    }

    /// <summary>
    /// Stage starts pre-heat
    /// </summary>
    public class PreHeatStartEvent : NotifyEvent, IGemCommand
    {
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            try
            {
                var pivcontainer = this.GEMModule().GetPIVContainer();

                string lotname = this.LotOPModule().LotInfo.LotName.Value;
                var foupnum = this.LotOPModule().LotInfo.GetFoupNumbetAtStageLotInfos(lotname);

                if (foupnum > 0)
                {
                    pivcontainer.UpdateStageLotInfo(foupnum);
                }
                else
                {
                    pivcontainer.Overdrive.Value = this.ProbingModule().OverDrive;
                    pivcontainer.WaferID.Value = this.GetParam_Wafer().GetSubsInfo().WaferID.Value;
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

    /// <summary>
    /// Stage ends pre-heat
    /// </summary>
    public class PreHeatEndEvent : NotifyEvent, IGemCommand
    {
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            try
            {
                var pivcontainer = this.GEMModule().GetPIVContainer();

                string lotname = this.LotOPModule().LotInfo.LotName.Value;
                var foupnum = this.LotOPModule().LotInfo.GetFoupNumbetAtStageLotInfos(lotname);

                if (foupnum > 0)
                {
                    pivcontainer.UpdateStageLotInfo(foupnum);
                }
                else
                {
                    pivcontainer.Overdrive.Value = this.ProbingModule().OverDrive;
                    pivcontainer.WaferID.Value = this.GetParam_Wafer().GetSubsInfo().WaferID.Value;
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


    public class PreHeatFailEvent : NotifyEvent
    {
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            var pivcontainer = this.GEMModule().GetPIVContainer();
            pivcontainer.UpdateStageLotInfo(pivcontainer.FoupNumber.Value);
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

    /// <summary>
    /// Stage changed to online then stage probecard state changed to card loaded or card not loaded from and unknown state
    /// </summary>
    public class StageProbecardInitalStateEvent : NotifyEvent
    {
    }

    /// Loadport set to an online state
    /// </summary>
    public class FoupOnlineEvent : NotifyEvent, IGemCommand
    {
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            try
            {
                if(pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().SetFoupState(pivInfo.FoupNumber, GEMFoupStateEnum.ONLINE);
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

    /// <summary>
    /// Loadport set to an offline state
    /// </summary>
    public class FoupOfflineEvent : NotifyEvent, IGemCommand
    {
        public EventCodeEnum SetPIV(PIVInfo pivInfo)
        {
            try
            {
                if (pivInfo != null)
                {
                    this.GEMModule().GetPIVContainer().SetFoupState(pivInfo.FoupNumber, GEMFoupStateEnum.OFFLINE);
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


    /// <summary>
    /// LIGHTSTATUS
    /// </summary>
    public class TowerLightEvent : NotifyEvent
    {
    }

    /// <summary>
    /// LOT 정상 종료되어 Foup Unload 까지 완료 된 후의 Event 
    /// </summary>
    public class FoupUnloadedInLotEndEvent : NotifyEvent, IGemCommand
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

    /// <summary>
    /// LOT 강제 종료되어 Foup Unload 까지 완료 된 후의 Event 
    /// </summary>
    public class FoupUnloadedInLotAbortEvent : NotifyEvent, IGemCommand
    {
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

}
