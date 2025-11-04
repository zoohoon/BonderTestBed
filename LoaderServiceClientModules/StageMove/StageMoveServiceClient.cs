using System;
using System.Threading.Tasks;

namespace LoaderServiceClientModules
{
    using Autofac;
    using LoaderBase.Communication;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Param;
    using ProberInterfaces.Proxies;
    using ProberInterfaces.WaferTransfer;
    using StageStateEnum = ProberInterfaces.StageStateEnum;

    public class StageMoveServiceClient :  IFactoryModule , IStageMove
    {

        public bool Initialized { get; set; } = false;
        public EnumAxisConstants PinViewAxis { get => EnumAxisConstants.PZ; }
        public InitPriorityEnum InitPriority { get; set; }
        public Autofac.IContainer _Container
        {
            get { return this.GetContainer(); }
        }

        private ILoaderCommunicationManager _LoaderCommunicationManager;

        public ILoaderCommunicationManager LoaderCommunicationManager
        {
            get
            {
                if (_LoaderCommunicationManager == null)
                    LoaderCommunicationManager = _Container.Resolve<ILoaderCommunicationManager>();
                return _LoaderCommunicationManager;
            }
            set { _LoaderCommunicationManager = value; }
        }

        public IStageSupervisorProxy StageSuperviosr => LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>();
        public IStageMoveProxy StageMove => LoaderCommunicationManager.GetProxy<IStageMoveProxy>();
        public void InitService()
        {
            LoggerManager.Debug("InitService - StageMoveServiceClient");
        }
        public bool IsServiceAvailable()
        {
            return true;
        }
        public EventCodeEnum AirBlowAirOnOff(bool val)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.AirBlowAirOnOff(val);
                }
            }catch(Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.AirBlowAirOnOff() err={err}");
            }
            return retVal;
        }

        public EventCodeEnum AirBlowMove(double xpos, double ypos, double zpos)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.AirBlowMove(xpos, ypos, zpos);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.AirBlowMove() err={err}");
            }
            return retVal;
        }

        public EventCodeEnum AirBlowMove(EnumAxisConstants axis, double pos, double speed, double acc)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.AirBlowMove(axis, pos, speed, acc);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.AirBlowMove() err={err}");
            }
            return retVal;
        }

        public EventCodeEnum CardChageMoveToIDLE()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.CardChageMoveToIDLE();
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.CardChageMoveToIDLE() err={err}");
            }
            return retVal;
        }

        public EventCodeEnum CardChageMoveToIN()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.CardChageMoveToIN();
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.CardChageMoveToIN() err={err}");
            }
            return retVal;
        }

        public EventCodeEnum CardChageMoveToOUT()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.CardChageMoveToOUT();
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.CardChageMoveToOUT() err={err}");
            }
            return retVal;
        }

        public EventCodeEnum CardViewMove(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.CardViewMove(xpos, ypos, trjtype, ovrd);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.CardViewMove(xpos, ypos, trjtype, ovrd) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum CardViewMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.CardViewMove(axis, pos, trjtype, ovrd);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.CardViewMove(axis,pos,trjtype,ovrd) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum CC_AxisMoveToPos(ProbeAxisObject axis, double pos, double velScale, double accScale)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.CC_AxisMoveToPos(axis, pos, velScale, accScale);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.CC_AxisMoveToPos(axis,pos,velScale,accScale) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum ChuckTiltMove(double RPos, double TTPos)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.ChuckTiltMove(RPos, TTPos);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.ChuckTiltMove(RPos, TTPos) err={err}");
            }
            return retVal;
        }

        public void DeInitModule()
        {
            StageMove.DeInitModule();
            try
            {
                if (StageMove != null)
                {
                    StageMove.DeInitModule();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"StageMoveServiceClient.DeInitModule() err={err}");
            }
        }

        public EventCodeEnum FrontDoorLock()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.FrontDoorLock();
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.FrontDoorLock() err={err}");
            }
            return retVal;
        }

        public EventCodeEnum FrontDoorUnLock()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.FrontDoorUnLock();
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.FrontDoorUnLock() err={err}");
            }
            return retVal;
        }

        public ModuleStateEnum GetModuleState()
        {
            ModuleStateEnum retVal = ModuleStateEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.GetModuleState();
                }
            }
            catch (Exception err)
            {
                retVal = ModuleStateEnum.UNDEFINED;
                LoggerManager.Debug($"StageMoveServiceClient.GetModuleState() err={err}");
            }
            return retVal;
        }

        public StageStateEnum GetState()
        {
            return StageMove?.GetState() ?? StageStateEnum.UNKNOWN;
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.InitModule();
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.InitModule() err={err}");
            }
            return retVal;
        }
        
        public bool IsHandlerholdWafer()
        {
            bool retVal = false;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.IsHandlerholdWafer();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"StageMoveServiceClient.IsFrontDoorClose(ref isfrontdoorclose) err={err}");
            }
            return retVal;
        }
        
        public EventCodeEnum IsFrontDoorClose(ref bool isfrontdoorclose)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.IsFrontDoorClose(ref isfrontdoorclose);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.IsFrontDoorClose(ref isfrontdoorclose) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum IsFrontDoorLock(ref bool isfrontdoorlock)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.IsFrontDoorLock(ref isfrontdoorlock);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.IsFrontDoorLock(ref isfrontdoorlock) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum IsFrontDoorOpen(ref bool isfrontdooropen)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.IsFrontDoorOpen(ref isfrontdooropen);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.IsFrontDoorOpen(ref isfrontdooropen) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum IsFrontDoorUnLock(ref bool isfrontdoorunlock)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.IsFrontDoorUnLock(ref isfrontdoorunlock);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.IsFrontDoorUnLock(ref isfrontdoorunlock) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum IsLoaderDoorClose(ref bool isloaderdoorclose, bool writelog = true)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.IsLoaderDoorClose(ref isloaderdoorclose);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.IsLoaderDoorClose(ref isloaderdoorclose) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum IsLoaderDoorOpen(ref bool isloaderdooropen, bool writelog = true)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.IsLoaderDoorOpen(ref isloaderdooropen);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.IsLoaderDoorOpen(ref isloaderdooropen) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum IsCardDoorOpen(ref bool iscarddooropen, bool writelog = true)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.IsCardDoorOpen(ref iscarddooropen);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.IsCardDoorOpen(ref iscarddooropen) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum LoaderDoorClose()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.LoaderDoorClose();
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.LoaderDoorClose() err={err}");
            }
            return retVal;
        }
        public EventCodeEnum LoaderDoorCloseRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.LoaderDoorCloseRecovery();
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.LoaderDoorCloseRecovery() err={err}");
            }
            return retVal;
        }
        public EventCodeEnum LoaderDoorOpen()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.LoaderDoorOpen();
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.LoaderDoorOpen() err={err}");
            }
            return retVal;
        }
        public EventCodeEnum CardDoorOpen()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.CardDoorOpen();
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.CardDoorOpen() err={err}");
            }
            return retVal;
        }
        public EventCodeEnum CardDoorClose()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.CardDoorClose();
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.CardDoorClose() err={err}");
            }
            return retVal;
        }
        public Task<EventCodeEnum> LoaderHomeOffsetMove()
        {
            return StageMove.LoaderHomeOffsetMove();
        }

        public EventCodeEnum LockCCState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.LockCCState();
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.LockCCState() err={err}");
            }
            return retVal;
        }

        public EventCodeEnum ManualAbsMove(double posX, double posY, double posZ, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.ManualAbsMove(posX, posY, posZ, trjtype, ovrd);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.ManualAbsMove(posX, posY, posZ, trjtype, ovrd) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum ManualRelMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.ManualRelMove(axis, pos, trjtype, ovrd);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.ManualRelMove(axis, pos, trjtype, ovrd) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum ManualZDownMove()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.ManualZDownMove();
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.ManualZDownMove() err={err}");
            }
            return retVal;
        }

        public EventCodeEnum MonitorForVacuum(bool val, long sustain = 0, long timeout = 0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.MonitorForVacuum(val, sustain, timeout);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.MonitorForVacuum(val, sustain, timeout)err={err}");
            }
            return retVal;
        }

        public EventCodeEnum ChuckMainVacOff()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.ChuckMainVacOff();
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.ChuckMainVacOff err={err}");
            }
            return retVal;
        }

        public EventCodeEnum MoveLoadingPosition(double offsetvalue)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.MoveLoadingPosition(offsetvalue);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.MoveLoadingPosition(offsetvalue) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum MoveTCW_Position()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.MoveTCW_Position();
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.MoveTCW_Position() err={err}");
            }
            return retVal;
        }
        public EventCodeEnum MoveLoadingOffsetPosition(double x, double y, double z, double t)
        {
            return StageMove.MoveLoadingOffsetPosition(x,y,z,t);
        }
        public EventCodeEnum MovePadToPin(WaferCoordinate waferoffset, PinCoordinate pinoffset, double zclearance)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.MovePadToPin(waferoffset, pinoffset, zclearance);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.MovePadToPin(waferoffset, pinoffset, zclearance) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum MoveStageRepeatRelMove(double xpos, double ypos, double xvel, double xacc, double yvel, double yacc)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.MoveStageRepeatRelMove(xpos, ypos, xvel, xacc, yvel, yacc);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.MoveStageRepeatRelMove(xpos, ypos, xvel, xacc, yvel, yacc) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum MoveToCardHolderPositionAndCheck()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.MoveToCardHolderPositionAndCheck();
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.MoveToCardHolderPositionAndCheck() err={err}");
            }
            return retVal;
        }


        public EventCodeEnum MoveToBackPosition()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.MoveToBackPosition();
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.MoveToBackPosition() err={err}");
            }
            return retVal;
        }

        public EventCodeEnum MoveToFrontPosition()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.MoveToFrontPosition();
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.MoveToFrontPosition() err={err}");
            }
            return retVal;
        }

        public EventCodeEnum MoveToCenterPosition()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.MoveToCenterPosition();
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.MoveToCenterPosition() err={err}");
            }
            return retVal;
        }

        public EventCodeEnum MoveToMark()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.MoveToMark();
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.MoveToMark() err={err}");
            }
            return retVal;
        }

        public EventCodeEnum MoveToNcPadChangePosition()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.MoveToNcPadChangePosition();
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.MoveToNcPadChangePosition() err={err}");
            }
            return retVal;
        }

        public EventCodeEnum MOVETONEXTDIE()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.MOVETONEXTDIE();
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.MOVETONEXTDIE() err={err}");
            }
            return retVal;
        }

        public EventCodeEnum MoveToSoaking(WaferCoordinate waferoffset, PinCoordinate pinoffset, double zclearance)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.MoveToSoaking(waferoffset, pinoffset, zclearance);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.MoveToSoaking(waferoffset, pinoffset, zclearance) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum NCPadDown()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.NCPadDown();
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.NCPadDown() err={err}");
            }
            return retVal;
        }

        public EventCodeEnum NCPadMove(NCCoordinate nccoord, PinCoordinate pincoord)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.NCPadMove(nccoord, pincoord);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.NCPadMove(nccoord, pincoord) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum NCPadUp()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.NCPadUp();
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.NCPadUp() err={err}");
            }
            return retVal;
        }

        public EventCodeEnum PinHighViewMove(double xpos, double ypos, double zpos, double tpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.PinHighViewMove(xpos, ypos, zpos, tpos, trjtype, ovrd);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.PinHighViewMove(xpos, ypos, zpos, tpos, trjtype, ovrd) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum PinHighViewMove(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.PinHighViewMove(xpos, ypos, zpos, trjtype, ovrd);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.PinHighViewMove(xpos, ypos, zpos, trjtype, ovrd) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum PinHighViewMove(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.PinHighViewMove(xpos, ypos, trjtype, ovrd);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.PinHighViewMove(xpos, ypos, trjtype, ovrd) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum PinHighViewMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.PinHighViewMove(axis, pos, trjtype, ovrd);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.PinHighViewMove(axis, pos, trjtype, ovrd) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum PinLowViewMove(double xpos, double ypos, double zpos, double tpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.PinLowViewMove(xpos, ypos, zpos, tpos, trjtype, ovrd);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.PinLowViewMove(xpos, ypos, zpos, tpos, trjtype, ovrd) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum PinLowViewMove(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.PinLowViewMove(xpos, ypos, zpos, trjtype, ovrd);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.PinLowViewMove(xpos, ypos, zpos, trjtype, ovrd) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum PinLowViewMove(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.PinLowViewMove(xpos, ypos, trjtype, ovrd);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.PinLowViewMove(xpos, ypos, trjtype, ovrd) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum PinLowViewMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.PinLowViewMove(axis, pos, trjtype, ovrd);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.PinLowViewMove(axis, pos, trjtype, ovrd) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum PogoViewMove(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.PogoViewMove(xpos, ypos, zpos, trjtype, ovrd);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.PogoViewMove(xpos, ypos, zpos, trjtype, ovrd) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum ProbingCoordMoveNCPad(NCCoordinate nccoord, PinCoordinate pincoord, double offsetZ)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.ProbingCoordMoveNCPad(nccoord, pincoord, offsetZ);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.ProbingCoordMoveNCPad(nccoord, pincoord, offsetZ) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum ProbingCoordMoveNCPad(NCCoordinate nccoord, PinCoordinate pincoord, double offsetZ, double zspeed, double zacc)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.ProbingCoordMoveNCPad(nccoord, pincoord, offsetZ, zspeed, zacc);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.ProbingCoordMoveNCPad(nccoord, pincoord, offsetZ, zspeed, zacc) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum ProbingZDOWN(WaferCoordinate wafercoord, PinCoordinate pincoord, double overdrive, double zclearance)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.ProbingZDOWN(wafercoord, pincoord, overdrive, zclearance);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.ProbingZDOWN(wafercoord, pincoord, overdrive, zclearance) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum ProbingZDOWN(NCCoordinate nccoord, PinCoordinate pincoord, double overdrive, double zclearance)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.ProbingZDOWN(nccoord, pincoord, overdrive, zclearance);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.ProbingZDOWN(nccoord, pincoord, overdrive, zclearance) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum ProbingZDOWN(double overdrive, double zclearance)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.ProbingZDOWN(overdrive, zclearance);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.ProbingZDOWN(overdrive, zclearance) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum ProbingZUP(double overdrive)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.ProbingZUP(overdrive);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.ProbingZUP(overdrive) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum ProbingZUP(WaferCoordinate wafercoord, PinCoordinate pincoord, double overdrive)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.ProbingZUP(wafercoord, pincoord, overdrive);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.ProbingZUP(wafercoord, pincoord, overdrive) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum ProbingZUP(NCCoordinate nccoord, PinCoordinate pincoord, double overdrive)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.ProbingZUP(nccoord, pincoord, overdrive);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.ProbingZUP(nccoord, pincoord, overdrive) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum ReadVacuum(out bool val)
        {

            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            val = false;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.ReadVacuum(out val);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.ReadVacuum(out val) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum SetWaferCamBasePos(bool value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.SetWaferCamBasePos(value);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.SetWaferCamBasePos(value) err={err}");
            }
            return retVal;
        }

        public Task<EventCodeEnum> StageHomeOffsetMove()
        {
            return StageMove.StageHomeOffsetMove();

        }

        public EventCodeEnum StageMoveStop(ProbeAxisObject axis)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.StageMoveStop(axis);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.StageMoveStop(axis) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum StageRelMove(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.StageRelMove(xpos, ypos, trjtype, ovrd);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.StageRelMove(xpos, ypos, trjtype, ovrd) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum StageRelMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.StageRelMove(axis, pos, trjtype, ovrd);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.StageRelMove(axis, pos, trjtype, ovrd) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum StageSystemInit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.StageSystemInit();
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.StageSystemInit() err={err}");
            }
            return retVal;
        }

        public EventCodeEnum StageVMove(ProbeAxisObject axis, double vel, EnumTrjType trjtype)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.StageVMove(axis, vel, trjtype);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.StageVMove(axis, vel , trjtype) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum Handlerrelease(long timeout = 0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.Handlerrelease();
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.Handlerrelease() err={err}");
            }
            return retVal;
        }

        public EventCodeEnum Handlerhold(long timeout = 0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.Handlerhold();
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.Handlerhold() err={err}");
            }
            return retVal;
        }

        public EventCodeEnum CCRotLock(long timeout = 0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.CCRotLock(timeout);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.CCRotLock(timeout) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum CCRotUnLock(long timeout = 0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.CCRotUnLock(timeout);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.CCRotUnLock(timeout) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum TiltingMove(double tz1offset, double tz2offset, double tz3offset)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.TiltingMove(tz1offset, tz2offset, tz3offset);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.TiltingMove(tz1offset , tz2offset, tz3offset) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum TiltMove(ProbeAxisObject axis, double pos)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.TiltMove(axis, pos);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.TiltMove(axis, pos) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum TouchSensorHighViewMove(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.TouchSensorHighViewMove(xpos, ypos, zpos, trjtype, ovrd);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.TouchSensorHighViewMove(xpos, ypos, zpos, trjtype, ovrd) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum TouchSensorLowViewMove(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.TouchSensorLowViewMove(xpos, ypos, zpos, trjtype, ovrd);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.TouchSensorLowViewMove(xpos, ypos, zpos, trjtype, ovrd) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum TouchSensorSensingMoveNCPad(NCCoordinate nccoord, PinCoordinate pincoord, double offsetZ)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.TouchSensorSensingMoveNCPad(nccoord, pincoord, offsetZ);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.TouchSensorSensingMoveNCPad(nccoord, pincoord, offsetZ) err={err}");
            }
            return retVal;
        }
        public EventCodeEnum TouchSensorSensingMoveStage(WaferCoordinate wcoord, PinCoordinate pincoord, double zclearance)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.TouchSensorSensingMoveStage(wcoord, pincoord, zclearance);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.TouchSensorSensingMoveNCPad(nccoord, pincoord, offsetZ) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum UnLockCCState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.UnLockCCState();
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.UnLockCCState() err={err}");
            }
            return retVal;
        }

        public EventCodeEnum VacuumOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.VacuumOnOff(val, extraVacReady, extraVacOn, timeout);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.VacuumOnOff(val ,timeout) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum VMAbsMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.VMAbsMove(axis, pos, trjtype, ovrd);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.VMAbsMove(axis, pos, trjtype, ovrd) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum VMRelMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.VMRelMove(axis, pos, trjtype, ovrd);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.VMRelMove(axis, pos, trjtype, ovrd) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum VMViewMove(double xpos, double ypos, double zpos, double tpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.VMViewMove(xpos, ypos, zpos, tpos, trjtype, ovrd);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.VMViewMove(xpos, ypos, zpos, tpos, trjtype, ovrd) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum VMViewMove(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.VMViewMove(xpos, ypos, zpos, trjtype, ovrd);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.VMViewMove(xpos, ypos, zpos, trjtype, ovrd) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum VMWaferHighViewMove(double xpos, double ypos, double zpos, double tpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.VMWaferHighViewMove(xpos, ypos, zpos, tpos, trjtype, ovrd);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.VMWaferHighViewMove(xpos, ypos, zpos, tpos, trjtype, ovrd) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum VMWaferHighViewMove(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.VMWaferHighViewMove(xpos, ypos, zpos, trjtype, ovrd);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.VMWaferHighViewMove(xpos, ypos, zpos, trjtype, ovrd) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum VMWaferHighViewMove(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.VMWaferHighViewMove(xpos, ypos, trjtype, ovrd);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.VMWaferHighViewMove(xpos, ypos, trjtype, ovrd) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum WaferHighCamCoordMoveNCpad(NCCoordinate nccoord, double offsetZ)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.WaferHighCamCoordMoveNCpad(nccoord, offsetZ);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.WaferHighCamCoordMoveNCpad(nccoord, offsetZ) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum WaferHighViewIndexMove(long mach_x, long mach_y, double zpos = 0, bool NotUseHeightProfile = false)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.WaferHighViewIndexMove(mach_x, mach_y, zpos, NotUseHeightProfile);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.WaferHighViewIndexMove(mach_x, mach_y) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum WaferHighViewMove(double xpos, double ypos, double zpos, double tpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.WaferHighViewMove(xpos, ypos, zpos, tpos, trjtype, ovrd);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.WaferHighViewMove(xpos, ypos, zpos, tpos, trjtype, ovrd) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum WaferHighViewMove(double xpos, double ypos, double zpos, bool NotUseHeightProfile = false, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.WaferHighViewMove(xpos, ypos, zpos, NotUseHeightProfile, trjtype, ovrd);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.WaferHighViewMove(xpos, ypos, zpos, NotUseHeightProfile, trjtype, ovrd) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum WaferHighViewMove(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.WaferHighViewMove(xpos, ypos, trjtype, ovrd);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.WaferHighViewMove(xpos, ypos, trjtype, ovrd) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum WaferHighViewMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.WaferHighViewMove(axis, pos, trjtype, ovrd);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.WaferHighViewMove(axis, pos, trjtype, ovrd) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum WaferLowCamCoordMoveNCpad(NCCoordinate nccoord, double offsetZ)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.WaferLowCamCoordMoveNCpad(nccoord, offsetZ);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.WaferLowCamCoordMoveNCpad(nccoord, offsetZ) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum WaferLowViewIndexMove(long mach_x, long mach_y, double zpos = 0, bool NotUseHeightProfile = false)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.WaferLowViewIndexMove(mach_x, mach_y, zpos, NotUseHeightProfile);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.WaferLowViewIndexMove(mach_x, mach_y) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum WaferLowViewMove(double xpos, double ypos, double zpos, double tpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.WaferLowViewMove(xpos, ypos, zpos, tpos, trjtype, ovrd);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.WaferLowViewMove(xpos, ypos, zpos, tpos, trjtype, ovrd) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum WaferLowViewMove(double xpos, double ypos, double zpos, bool NotUseHeightProfile = false, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.WaferLowViewMove(xpos, ypos, zpos, NotUseHeightProfile, trjtype, ovrd);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.WaferLowViewMove(xpos, ypos, zpos, NotUseHeightProfile, trjtype, ovrd) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum WaferLowViewMove(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.WaferLowViewMove(xpos, ypos, trjtype, ovrd);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.WaferLowViewMove(xpos, ypos, trjtype, ovrd) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum WaferLowViewMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.WaferLowViewMove(axis, pos, trjtype, ovrd);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.WaferLowViewMove(axis, pos, trjtype, ovrd) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum WaitForVacuum(bool val, long timeout = 0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.WaitForVacuum(val, timeout);
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.WaitForVacuum(val, timeout) err={err}");
            }
            return retVal;
        }

        public EventCodeEnum ZCLEARED()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.ZCLEARED();
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.ZCLEARED() err={err}");
            }
            return retVal;
        }
        public EventCodeEnum CCZCLEARED()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.CCZCLEARED();
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.CCZCLEARED() err={err}");
            }
            return retVal;
        }

        public EventCodeEnum InitModule(IContainer container)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum CheckWaferStatus(bool isExist)
        {
            return EventCodeEnum.NONE;
        }

        public bool IsCardExist()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum StageMoveLockState()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum StageMoveUnLockState()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum StageLock()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum StageUnlock()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum ThreeLegUp(long timeout = 0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.ThreeLegUp();
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.ThreeLegUp() err={err}");
            }
            return retVal;
        }


        public EventCodeEnum ThreeLegDown(long timeout = 0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMove != null)
                {
                    retVal = StageMove.ThreeLegDown();
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveServiceClient.ThreeLegDown() err={err}");
            }
            return retVal;
        }

        // Component Verification 기능을 통한 Wafer Align 수행시 WaferCamBrige를 접지 않는 옵션에 대한 Flag를 설정하는 함수
        public EventCodeEnum SetNoRetractWaferCamBridgeWhenMarkAlignFlag(bool isFlagOn)
        {
            return EventCodeEnum.NONE;
        }
    }
}
