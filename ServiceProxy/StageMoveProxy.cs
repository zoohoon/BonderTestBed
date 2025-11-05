using System;
using System.Threading.Tasks;

namespace ServiceProxy
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Param;
    using ProberInterfaces.Proxies;
    using System.ComponentModel;
    using System.ServiceModel;
    using System.ServiceModel.Description;


    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class StageMoveProxy : ClientBase<IStageMove>, IStageMoveProxy, IFactoryModule
    {
        public StageMoveProxy(int port, string ip = null) :
                base(new ServiceEndpoint(
                    ContractDescription.GetContract(typeof(IStageMove)),
                    new NetTcpBinding()
                    {
                        ReceiveTimeout = TimeSpan.MaxValue,
                        MaxBufferPoolSize = 524288,
                        MaxReceivedMessageSize = 50000000,
                        Security = new NetTcpSecurity() { Mode = SecurityMode.None },

                        ReliableSession = new OptionalReliableSession() { InactivityTimeout = TimeSpan.FromMinutes(1), Enabled = true}
                    }, 
                       new EndpointAddress($"net.tcp://{ip}:{port}/POS/{ServiceAddress.StageMoveService}")))

        {
            LoggerManager.Debug($"End point address: {this.Endpoint.Address.Uri.AbsoluteUri}");
        }


        public EnumAxisConstants PinViewAxis { get; set; }

        public bool Initialized { get; set; } = false;

        private object chnLockObj = new object();

        public bool IsServiceAvailable()
        {
            bool retVal = false;
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                    {
                        var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                        try
                        {
                            (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 0, 15);
                            retVal = Channel.IsServiceAvailable();
                        }
                        catch (Exception)
                        {
                            LoggerManager.Error($"StageMoveProxy IsServiceAvailable timeout error");
                        }
                        finally
                        {
                            (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"StageMove Service service error.");
                        retVal = false;
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                LoggerManager.Error($"StageMove Service service error.");
                retVal = false;
            }

            return retVal;
        }
        public bool IsOpened()
        {
            if (State == CommunicationState.Opened | State == CommunicationState.Created)
                return true;
            else
                return false;
        }

        public CommunicationState GetCommunicationState()
        {
            return this.State;
        }


        public void InitService()
        {
            Channel.InitService();
        }

        public void DeInitService()
        {
            //Dispose
        }

        public EventCodeEnum AirBlowAirOnOff(bool val)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.AirBlowAirOnOff(val);
            }
            return retVal;
        }

        public EventCodeEnum AirBlowMove(double xpos, double ypos, double zpos)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.AirBlowMove(xpos, ypos, zpos);
            }
            return retVal;
        }

        public EventCodeEnum AirBlowMove(EnumAxisConstants axis, double pos, double speed, double acc)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.AirBlowMove(axis, pos, speed, acc);
            }
            return retVal;
        }

        public EventCodeEnum CardChageMoveToIDLE()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.CardChageMoveToIDLE();
            }
            return retVal;
        }

        public EventCodeEnum CardChageMoveToIN()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.CardChageMoveToIN();
            }
            return retVal;
        }

        public EventCodeEnum CardChageMoveToOUT()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.CardChageMoveToOUT();
            }
            return retVal;
        }

        public EventCodeEnum CardViewMove(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.CardViewMove(xpos, ypos, trjtype, ovrd);
            }
            return retVal;
        }

        public EventCodeEnum CardViewMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.CardViewMove(axis, pos, trjtype, ovrd);
            }
            return retVal;
        }

        public EventCodeEnum CC_AxisMoveToPos(ProbeAxisObject axis, double pos, double velScale, double accScale)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.CC_AxisMoveToPos(axis, pos, velScale, accScale);
            }
            return retVal;
        }

        public EventCodeEnum ChuckTiltMove(double RPos, double TTPos)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.ChuckTiltMove(RPos, TTPos);
            }
            return retVal;
        }

        public void DeInitModule()
        {
            if (IsOpened())
            {
                Channel.DeInitModule();
            }
        }

        public EventCodeEnum FrontDoorLock()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.FrontDoorLock();
            }
            return retVal;
        }

        public EventCodeEnum FrontDoorUnLock()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.FrontDoorUnLock();
            }
            return retVal;
        }

        public ModuleStateEnum GetModuleState()
        {
            ModuleStateEnum retVal = ModuleStateEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.GetModuleState();
            }
            return retVal;
        }

        public StageStateEnum GetState()
        {
            StageStateEnum retVal = StageStateEnum.IDLE;
            if (IsOpened())
            {
                retVal = Channel.GetState();
            }
            return retVal;
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.InitModule();
            }
            return retVal;
        }

        public EventCodeEnum IsFrontDoorClose(ref bool isfrontdoorclose)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.IsFrontDoorClose(ref isfrontdoorclose);
            }
            return retVal;
        }
        public bool IsHandlerholdWafer()
        {
            bool retVal = false;
            if (IsOpened())
            {
                retVal = Channel.IsHandlerholdWafer();
            }
            return retVal;
        }
        

        public EventCodeEnum IsFrontDoorLock(ref bool isfrontdoorlock)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.IsFrontDoorLock(ref isfrontdoorlock);
            }
            return retVal;
        }

        public EventCodeEnum IsFrontDoorOpen(ref bool isfrontdooropen)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.IsFrontDoorOpen(ref isfrontdooropen);
            }
            return retVal;
        }

        public EventCodeEnum IsFrontDoorUnLock(ref bool isfrontdoorunlock)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.IsFrontDoorUnLock(ref isfrontdoorunlock);
            }
            return retVal;
        }

        public EventCodeEnum IsLoaderDoorClose(ref bool isloaderdoorclose, bool writelog = true)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.IsLoaderDoorClose(ref isloaderdoorclose);
            }
            return retVal;
        }

        public EventCodeEnum IsLoaderDoorOpen(ref bool isloaderdooropen, bool writelog = true)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.IsLoaderDoorOpen(ref isloaderdooropen);
            }
            return retVal;
        }

        public EventCodeEnum IsCardDoorOpen(ref bool iscarddooropen, bool writelog = true)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.IsCardDoorOpen(ref iscarddooropen);
            }
            return retVal;
        }
        public EventCodeEnum LoaderDoorClose()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.LoaderDoorClose();
            }
            return retVal;
        }
        public EventCodeEnum LoaderDoorCloseRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.LoaderDoorCloseRecovery();
            }
            return retVal;
        }
        public EventCodeEnum LoaderDoorOpen()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.LoaderDoorOpen();
            }
            return retVal;
        }
        public EventCodeEnum CardDoorOpen()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.CardDoorOpen();
            }
            return retVal;
        }
        public EventCodeEnum CardDoorClose()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.CardDoorClose();
            }
            return retVal;
        }
        public async Task<EventCodeEnum> LoaderHomeOffsetMove()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                return await Channel.LoaderHomeOffsetMove();
            }
            return retVal;
        }

        public EventCodeEnum LockCCState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.LockCCState();
            }
            return retVal;
        }

        public EventCodeEnum ManualAbsMove(double posX, double posY, double posZ, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.ManualAbsMove(posX, posY, posZ, trjtype, ovrd);
            }
            return retVal;
        }

        public EventCodeEnum ManualRelMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.ManualRelMove(axis, pos, trjtype, ovrd);
            }
            return retVal;
        }

        public EventCodeEnum ManualZDownMove()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.ManualZDownMove();
            }
            return retVal;
        }

        public EventCodeEnum MonitorForVacuum(bool val, long sustain = 0, long timeout = 0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.MonitorForVacuum(val, sustain, timeout);
            }
            return retVal;
        }

        public EventCodeEnum ChuckMainVacOff()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsOpened())
                {
                    retVal = Channel.ChuckMainVacOff();
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Debug($"StageMoveProxy.ChuckMainVacOff err={err}");
            }
            return retVal;
        }

        public EventCodeEnum MoveLoadingPosition(double offsetvalue)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.MoveLoadingPosition(offsetvalue);
            }
            return retVal;
        }

        public EventCodeEnum MoveTCW_Position()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.MoveTCW_Position();
            }
            return retVal;
        }
        public EventCodeEnum MoveLoadingOffsetPosition(double x, double y, double z, double t)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.MoveLoadingOffsetPosition(x,y,z,t);
            }
            return retVal;
        }


        public EventCodeEnum MovePadToPin(WaferCoordinate waferoffset, PinCoordinate pinoffset, double zclearance)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.MovePadToPin(waferoffset, pinoffset, zclearance);
            }
            return retVal;
        }

        public EventCodeEnum MoveStageRepeatRelMove(double xpos, double ypos, double xvel, double xacc, double yvel, double yacc)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.MoveStageRepeatRelMove(xpos, ypos, xvel, xacc, yvel, yacc);
            }
            return retVal;
        }

        public EventCodeEnum MoveToCardHolderPositionAndCheck()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.MoveToCardHolderPositionAndCheck();
            }
            return retVal;
        }

        public EventCodeEnum MoveToBackPosition()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.MoveToBackPosition();
            }
            return retVal;
        }
        public EventCodeEnum MoveToFrontPosition()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.MoveToFrontPosition();
            }
            return retVal;
        }
        public EventCodeEnum MoveToCenterPosition()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.MoveToCenterPosition();
            }
            return retVal;
        }

        public EventCodeEnum MoveToMark()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.MoveToMark();
            }
            return retVal;
        }

        public EventCodeEnum MoveToNcPadChangePosition()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.MoveToNcPadChangePosition();
            }
            return retVal;
        }

        public EventCodeEnum MOVETONEXTDIE()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.MOVETONEXTDIE();
            }
            return retVal;
        }

        public EventCodeEnum MoveToSoaking(WaferCoordinate waferoffset, PinCoordinate pinoffset, double zclearance)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.MoveToSoaking(waferoffset, pinoffset, zclearance);
            }
            return retVal;
        }

        public EventCodeEnum NCPadDown()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.NCPadDown();
            }
            return retVal;
        }

        public EventCodeEnum NCPadMove(NCCoordinate nccoord, PinCoordinate pincoord)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.NCPadMove(nccoord, pincoord);
            }
            return retVal;
        }

        public EventCodeEnum NCPadUp()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.NCPadUp();
            }
            return retVal;
        }

        public EventCodeEnum PinHighViewMove(double xpos, double ypos, double zpos, double tpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.PinHighViewMove(xpos, ypos, zpos, tpos, trjtype, ovrd);
            }
            return retVal;
        }

        public EventCodeEnum PinHighViewMove(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.PinHighViewMove(xpos, ypos, zpos, trjtype, ovrd);
            }
            return retVal;
        }

        public EventCodeEnum PinHighViewMove(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.PinHighViewMove(xpos, ypos, trjtype, ovrd);
            }
            return retVal;
        }

        public EventCodeEnum PinHighViewMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.PinHighViewMove(axis, pos, trjtype, ovrd);
            }
            return retVal;
        }

        public EventCodeEnum PinLowViewMove(double xpos, double ypos, double zpos, double tpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.PinLowViewMove(xpos, ypos, zpos, tpos, trjtype, ovrd);
            }
            return retVal;
        }

        public EventCodeEnum PinLowViewMove(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.PinLowViewMove(xpos, ypos, zpos, trjtype, ovrd);
            }
            return retVal;
        }

        public EventCodeEnum PinLowViewMove(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.PinLowViewMove(xpos, ypos, trjtype, ovrd);
            }
            return retVal;
        }

        public EventCodeEnum PinLowViewMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.PinLowViewMove(axis, pos, trjtype, ovrd);
            }
            return retVal;
        }

        public EventCodeEnum PogoViewMove(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.PogoViewMove(xpos, ypos, zpos, trjtype, ovrd);
            }
            return retVal;
        }

        public EventCodeEnum ProbingCoordMoveNCPad(NCCoordinate nccoord, PinCoordinate pincoord, double offsetZ)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.ProbingCoordMoveNCPad(nccoord, pincoord, offsetZ);
            }
            return retVal;
        }

        public EventCodeEnum ProbingCoordMoveNCPad(NCCoordinate nccoord, PinCoordinate pincoord, double offsetZ, double zspeed, double zacc)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.ProbingCoordMoveNCPad(nccoord, pincoord, offsetZ, zspeed, zacc);
            }
            return retVal;
        }

        public EventCodeEnum ProbingZDOWN(WaferCoordinate wafercoord, PinCoordinate pincoord, double overdrive, double zclearance)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.ProbingZDOWN(wafercoord, pincoord, overdrive, zclearance);
            }
            return retVal;
        }

        public EventCodeEnum ProbingZDOWN(NCCoordinate nccoord, PinCoordinate pincoord, double overdrive, double zclearance)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.ProbingZDOWN(nccoord, pincoord, overdrive, zclearance);
            }
            return retVal;
        }

        public EventCodeEnum ProbingZDOWN(double overdrive, double zclearance)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.ProbingZDOWN(overdrive, zclearance);
            }
            return retVal;
        }

        public EventCodeEnum ProbingZUP(double overdrive)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.ProbingZUP(overdrive);
            }
            return retVal;
        }

        public EventCodeEnum ProbingZUP(WaferCoordinate wafercoord, PinCoordinate pincoord, double overdrive)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.ProbingZUP(wafercoord, pincoord, overdrive);
            }
            return retVal;
        }

        public EventCodeEnum ProbingZUP(NCCoordinate nccoord, PinCoordinate pincoord, double overdrive)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.ProbingZUP(nccoord, pincoord, overdrive);
            }
            return retVal;
        }

        public EventCodeEnum ReadVacuum(out bool val)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.ReadVacuum(out val);
            }
            else
                val = false;
            return retVal;
        }

        public EventCodeEnum SetWaferCamBasePos(bool value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.SetWaferCamBasePos(value);
            }
            return retVal;
        }

        public async Task<EventCodeEnum> StageHomeOffsetMove()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                return await Channel.StageHomeOffsetMove();
            }
            return retVal;
        }

        public EventCodeEnum StageMoveStop(ProbeAxisObject axis)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.StageMoveStop(axis);
            }
            return retVal;
        }

        public EventCodeEnum StageRelMove(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.StageRelMove(xpos, ypos, trjtype, ovrd);
            }
            return retVal;
        }

        public EventCodeEnum StageRelMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.StageRelMove(axis, pos, trjtype, ovrd);
            }
            return retVal;
        }

        public EventCodeEnum StageSystemInit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.StageSystemInit();
            }
            return retVal;
        }

        public EventCodeEnum StageVMove(ProbeAxisObject axis, double vel, EnumTrjType trjtype)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.StageVMove(axis, vel, trjtype);
            }
            return retVal;
        }

        public EventCodeEnum Handlerrelease(long timeout = 0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.Handlerrelease();
            }
            return retVal;
        }

        public EventCodeEnum Handlerhold(long timeout = 0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.Handlerhold();
            }
            return retVal;
        }

        public EventCodeEnum CCRotLock(long timeout = 0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.CCRotLock(60000);
            }
            return retVal;
        }

        public EventCodeEnum CCRotUnLock(long timeout = 0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.CCRotUnLock(60000);
            }
            return retVal;
        }

        public EventCodeEnum TiltingMove(double tz1offset, double tz2offset, double tz3offset)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.TiltingMove(tz1offset, tz2offset, tz3offset);
            }
            return retVal;
        }

        public EventCodeEnum TiltMove(ProbeAxisObject axis, double pos)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.TiltMove(axis, pos);
            }
            return retVal;
        }

        public EventCodeEnum TouchSensorHighViewMove(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.TouchSensorHighViewMove(xpos, ypos, zpos, trjtype, ovrd);
            }
            return retVal;
        }

        public EventCodeEnum TouchSensorLowViewMove(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.TouchSensorLowViewMove(xpos, ypos, zpos, trjtype, ovrd);
            }
            return retVal;
        }

        public EventCodeEnum TouchSensorSensingMoveNCPad(NCCoordinate nccoord, PinCoordinate pincoord, double offsetZ)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.TouchSensorSensingMoveNCPad(nccoord, pincoord, offsetZ);
            }
            return retVal;
        }

        public EventCodeEnum TouchSensorSensingMoveStage(WaferCoordinate wcoord, PinCoordinate pincoord, double zclearance)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.TouchSensorSensingMoveStage(wcoord, pincoord, zclearance);
            }
            return retVal;
        }

        public EventCodeEnum UnLockCCState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.UnLockCCState();
            }
            return retVal;

        }

        public EventCodeEnum VacuumOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.VacuumOnOff(val, extraVacReady, extraVacOn, timeout);
            }
            return retVal;
        }

        public EventCodeEnum VMAbsMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.VMAbsMove(axis, pos, trjtype, ovrd);
            }
            return retVal;
        }

        public EventCodeEnum VMRelMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.VMRelMove(axis, pos, trjtype, ovrd);
            }
            return retVal;
        }

        public EventCodeEnum VMViewMove(double xpos, double ypos, double zpos, double tpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.VMViewMove(xpos, ypos, zpos, tpos, trjtype, ovrd);
            }
            return retVal;
        }

        public EventCodeEnum VMViewMove(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.VMViewMove(xpos, ypos, zpos, trjtype, ovrd);
            }
            return retVal;
        }

        public EventCodeEnum VMWaferHighViewMove(double xpos, double ypos, double zpos, double tpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.VMWaferHighViewMove(xpos, ypos, zpos, tpos, trjtype, ovrd);
            }
            return retVal;
        }

        public EventCodeEnum VMWaferHighViewMove(double xpos, double ypos, double zpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.VMWaferHighViewMove(xpos, ypos, zpos, trjtype, ovrd);
            }
            return retVal;
        }

        public EventCodeEnum VMWaferHighViewMove(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.VMWaferHighViewMove(xpos, ypos, trjtype, ovrd);
            }
            return retVal;
        }

        public EventCodeEnum WaferHighCamCoordMoveNCpad(NCCoordinate nccoord, double offsetZ)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.WaferHighCamCoordMoveNCpad(nccoord, offsetZ);
            }
            return retVal;
        }

        public EventCodeEnum WaferHighViewIndexMove(long mach_x, long mach_y, double zpos, bool NotUseHeightProfile = false)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.WaferHighViewIndexMove(mach_x, mach_y, zpos, NotUseHeightProfile);
            }
            return retVal;
        }

        public EventCodeEnum WaferHighViewMove(double xpos, double ypos, double zpos, double tpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.WaferHighViewMove(xpos, ypos, zpos, tpos, trjtype, ovrd);
            }
            return retVal;
        }

        public EventCodeEnum WaferHighViewMove(double xpos, double ypos, double zpos, bool NotUseHeightProfile = false, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.WaferHighViewMove(xpos, ypos, zpos, NotUseHeightProfile, trjtype, ovrd);
            }
            return retVal;
        }

        public EventCodeEnum WaferHighViewMove(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.WaferHighViewMove(xpos, ypos, trjtype, ovrd);
            }
            return retVal;
        }

        public EventCodeEnum WaferHighViewMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.WaferHighViewMove(axis, pos, trjtype, ovrd);
            }
            return retVal;
        }

        public EventCodeEnum WaferLowCamCoordMoveNCpad(NCCoordinate nccoord, double offsetZ)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.WaferLowCamCoordMoveNCpad(nccoord, offsetZ);
            }
            return retVal;
        }

        public EventCodeEnum WaferLowViewIndexMove(long mach_x, long mach_y, double zpos, bool NotUseHeightProfile = false)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.WaferLowViewIndexMove(mach_x, mach_y, zpos, NotUseHeightProfile);
            }
            return retVal;
        }

        public EventCodeEnum WaferLowViewMove(double xpos, double ypos, double zpos, double tpos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.WaferLowViewMove(xpos, ypos, zpos, tpos, trjtype, ovrd);
            }
            return retVal;
        }

        public EventCodeEnum WaferLowViewMove(double xpos, double ypos, double zpos, bool NotUseHeightProfile = false, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.WaferLowViewMove(xpos, ypos, zpos, NotUseHeightProfile, trjtype, ovrd);
            }
            return retVal;
        }

        public EventCodeEnum WaferLowViewMove(double xpos, double ypos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.WaferLowViewMove(xpos, ypos, trjtype, ovrd);
            }
            return retVal;
        }

        public EventCodeEnum WaferLowViewMove(ProbeAxisObject axis, double pos, EnumTrjType trjtype = EnumTrjType.Normal, double ovrd = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.WaferLowViewMove(axis, pos, trjtype, ovrd);
            }
            return retVal;
        }

        public EventCodeEnum WaitForVacuum(bool val, long timeout = 0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.WaitForVacuum(val, timeout);
            }
            return retVal;
        }

        public EventCodeEnum ZCLEARED()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.ZCLEARED();
            }
            return retVal;
        }
        public EventCodeEnum CCZCLEARED()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.CCZCLEARED();
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
            if (IsOpened())
            {
                retVal = Channel.ThreeLegUp();
            }
            return retVal;
        }


        public EventCodeEnum ThreeLegDown(long timeout = 0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (IsOpened())
            {
                retVal = Channel.ThreeLegDown();
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
