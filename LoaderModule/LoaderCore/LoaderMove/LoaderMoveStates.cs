using System;
using System.Runtime.CompilerServices;

using Autofac;
using LoaderBase;
using ProberInterfaces;
using ProberErrorCode;
using LogModule;

namespace LoaderCore.LoaderMoveStates
{
    public abstract class LoaderMoveStateBase
    {
        public LoaderMove Module { get; set; }

        public LoaderMoveStateBase(LoaderMove module)
        {
            this.Module = module;
        }

        protected IMotionManagerProxy Motion => Module.Container.Resolve<IMotionManagerProxy>();

        protected ILoaderModule Loader => Module.Container.Resolve<ILoaderModule>();

        protected void StateTransitionIfNoErr(EventCodeEnum retVal, LoaderMoveStateEnum nextState, AccessInfo info, [CallerMemberName] string memberName = "")
        {
            try
            {
                if (retVal == EventCodeEnum.NONE)
                {
                    Module.StateTransition(nextState, info);
                }
                else if (retVal == EventCodeEnum.LOADER_STATE_INVALID || retVal == EventCodeEnum.FOUP_SCAN_WAFEROUT)
                {
                    //No State Transition.
                    LoggerManager.Error($"LoaderMove.{Module.State}.{memberName}() : Error occurred. retVal={retVal}");
                }
                else
                {
                    LoggerManager.Error($"LoaderMove.{Module.State}.{memberName}() : Error occurred. retVal={retVal}");

                    Module.StateTransition(LoaderMoveStateEnum.ERROR, null);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public abstract LoaderMoveStateEnum State { get; }

        public abstract FoupAccessModeEnum GetFoupAccessMode(int cassetteNum);

        protected EventCodeEnum RaiseInvalidState([CallerMemberName] string memberName = "")
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //Log.Warn($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");
                LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");

                retVal = EventCodeEnum.LOADER_STATE_INVALID;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        protected ICassetteModule GetColisionCassette(IInspectionTrayModule INSP)
        {
            ICassetteModule collisionCassette = null;
            try
            {
                if (INSP.Definition.IsInterferenceWithCassettePort.Value == true)
                {
                    collisionCassette = Loader.ModuleManager.FindModule(ModuleTypeEnum.CST, INSP.Definition.InterferenceCassettePortNum.Value) as ICassetteModule;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return collisionCassette;
        }

        public virtual EventCodeEnum MotionInit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.MovingMethods.MotionInit();

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.Home, null);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;

        }
        //public virtual EventCodeEnum MotionInit()
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    try
        //    {
        //        retVal = RaiseInvalidState();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    return retVal;
        //}

        #region Jog Move
        public virtual EventCodeEnum JogAbsMove(EnumAxisConstants axis, double val)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum JogRelMove(EnumAxisConstants axis, double val)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion

        public virtual EventCodeEnum RetractAll(LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum SafePosW()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        #region ScanCamera Move
        public virtual EventCodeEnum ScanCameraStartPosMove(IScanCameraModule ScanCamera, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum ScanCameraRelMove(EnumAxisConstants axis, double val)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion

        #region ScanSensor Move
        public virtual EventCodeEnum ScanSensorStartPosMove(IScanSensorModule ScanSensor, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum ExtendScanSensor(IScanSensorModule ScanSensor)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum ScanSensorUpMove(IScanSensorModule ScanSensor, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum ScanSensorDownMove(IScanSensorModule ScanSensor, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion

        #region Slot Move
        public virtual EventCodeEnum SlotUpMove(IARMModule ARM, ISlotModule Slot)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum SlotDownMove(IARMModule ARM, ISlotModule Slot)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion

        #region PA Move
        public virtual EventCodeEnum PreAlignUpMoveFromOCR(IARMModule ARM, IPreAlignModule PA, IOCRReadable OCR)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum PreAlignUpMove(IARMModule ARM, IPreAlignModule PA, LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum PreAlignDownMove(IARMModule ARM, IPreAlignModule AccessModule)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum PreAlignRelMove(IARMModule ARM, double vale)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum PreAlignRelMove(IPreAlignModule ARM, double vale)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum PreAlignZeroMove(IPreAlignModule ARM)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum PAFindNotchMove(IPreAlignModule PA, EnumMotorDedicatedIn input)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        #endregion

        #region OCR Move
        public virtual EventCodeEnum OCRMoveFromPreAlignUp(IARMModule ARM, IOCRReadable OCR, IPreAlignModule PA)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion

        #region InspectionTray Move
        public virtual EventCodeEnum InspectionTrayUpMove(IARMModule ARM, IInspectionTrayModule InspectionTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum InspectionTrayDownMove(IARMModule ARM, IInspectionTrayModule InspectionTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion

        #region FixedTray Move
        public virtual EventCodeEnum FixedTrayUpMove(IARMModule ARM, IFixedTrayModule FixedTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum FixedTrayDownMove(IARMModule ARM, IFixedTrayModule FixedTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion

        #region PreChuck Move
        public virtual EventCodeEnum PreChuckUpMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public virtual EventCodeEnum PreChuckDownMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion

        #region Chuck Move
        public virtual EventCodeEnum ChuckUpMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum ChuckDownMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        #endregion

        #region PickUp & PlaceDown Move
        public virtual EventCodeEnum PickUp(IARMModule ARM, IWaferOwnable Target,
            LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.ACCESS)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum PlaceDown(IARMModule ARM, IWaferOwnable Target,
            LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.ACCESS)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion

        #region SetupMove

        public virtual EventCodeEnum SetupToCstSlot(IScanSensorModule ScanSensor, ICassetteModule Cassette, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool uaxisskip, int slotnum, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public virtual EventCodeEnum SetupToPAMove(IARMModule ARM, IPreAlignModule PA, LoaderMovingTypeEnum movingType, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool uaxisskip, int slotnum, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public virtual EventCodeEnum SetupToOCRMove(IARMModule ARM, IPreAlignModule PA, IOCRReadable OCR, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool uaxisskip, int slotnum, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum SetupToChuckMove(IARMModule ARM, IChuckModule Chuck, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool uaxisskip, int slotnum, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum SetupToFixedTrayMove(IARMModule ARM, IFixedTrayModule FixedTray, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool uaxisskip, int slotnum, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public virtual EventCodeEnum SetupToInspectionTrayMove(IARMModule ARM, IInspectionTrayModule InspectionTray, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool uaxisskip, int slotnum, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public virtual EventCodeEnum SetupToCstMove(IARMModule ARM, ISlotModule Slot, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool uaxisskip, int slotnum, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion

    }

    public class ERROR : LoaderMoveStateBase
    {
        public ERROR(LoaderMove module) : base(module) { }

        public override LoaderMoveStateEnum State => LoaderMoveStateEnum.ERROR;

        public override FoupAccessModeEnum GetFoupAccessMode(int cassetteNum)
        {
            FoupAccessModeEnum foupAccessMode = FoupAccessModeEnum.UNKNOWN;

            try
            {
                //TODO : 현재 모션상태를 알 수 없으므로
                //1. 모든 ARM의 위치가 센서에 들어와있고
                bool isCollideARM = false;
                var ARMs = Loader.ModuleManager.FindModules<IARMModule>();
                foreach (var ARM in ARMs)
                {
                    var axisType = ARM.Definition.AxisType;
                    var axisObj = Loader.MotionManager.GetAxis(axisType.Value);

                    if (Loader.MotionManager.GetIOHome(axisObj) == false)
                    {
                        isCollideARM = true;
                        break;
                    }
                }

                //2. 센서 스캔모듈 상태가 IN이여야하고
                bool isCollideScanSensorObj = false;
                var ScanSensors = Loader.ModuleManager.FindModules<IScanSensorModule>();
                foreach (var ScanSensor in ScanSensors)
                {
                    if (ScanSensor.GetState() != ScanSensorStateEnum.RETRACTED)
                    {
                        isCollideScanSensorObj = true;
                        break;
                    }
                }

                //3. U Extension 상태도 IN이어야 한다.
                bool isCollideUextObj = Module.MovingMethods.UExtension.GetState() != UExtensionStateEnum.RETRACTED;

                if (isCollideARM == false && isCollideScanSensorObj == false && isCollideUextObj == false)
                    foupAccessMode = FoupAccessModeEnum.NO_ACCESSED;
                else
                    foupAccessMode = FoupAccessModeEnum.UNKNOWN;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return foupAccessMode;
        }

        //public override EventCodeEnum MotionInit()
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        retVal = Module.MovingMethods.MotionInit();

        //        StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.Home, null);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retVal;
        //}
    }

    public class Jog : LoaderMoveStateBase
    {
        public Jog(LoaderMove module) : base(module) { }

        public override LoaderMoveStateEnum State => LoaderMoveStateEnum.Jog;

        public override FoupAccessModeEnum GetFoupAccessMode(int cassetteNum)
        {
            FoupAccessModeEnum foupAccessMode = FoupAccessModeEnum.UNKNOWN;

            try
            {
                //TODO : 현재 모션상태를 알 수 없으므로
                //1. 모든 ARM의 위치가 센서에 들어와있고
                bool isCollideARM = false;
                var ARMs = Loader.ModuleManager.FindModules<IARMModule>();
                foreach (var ARM in ARMs)
                {
                    var axisType = ARM.Definition.AxisType;
                    var axisObj = Loader.MotionManager.GetAxis(axisType.Value);

                    if (Loader.MotionManager.GetIOHome(axisObj) == false)
                    {
                        isCollideARM = true;
                        break;
                    }
                }

                //2. 센서 스캔모듈 상태가 IN이여야하고
                bool isCollideScanSensorObj = false;
                var ScanSensors = Loader.ModuleManager.FindModules<IScanSensorModule>();
                foreach (var ScanSensor in ScanSensors)
                {
                    if (ScanSensor.GetState() != ScanSensorStateEnum.RETRACTED)
                    {
                        isCollideScanSensorObj = true;
                        break;
                    }
                }

                //3. U Extension 상태도 IN이어야 한다.
                bool isCollideUextObj = Module.MovingMethods.UExtension.GetState() != UExtensionStateEnum.RETRACTED;

                if (isCollideARM == false && isCollideScanSensorObj == false && isCollideUextObj == false)
                    foupAccessMode = FoupAccessModeEnum.NO_ACCESSED;
                else
                    foupAccessMode = FoupAccessModeEnum.UNKNOWN;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return foupAccessMode;
        }

        public override EventCodeEnum JogAbsMove(EnumAxisConstants axis, double val)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                retVal = Motion.AbsMove(axis, val);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.Jog, null);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum JogRelMove(EnumAxisConstants axis, double val)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                retVal = Motion.RelMove(axis, val);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.Jog, null);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class Home : LoaderMoveStateBase
    {
        public Home(LoaderMove module) : base(module) { }

        public override LoaderMoveStateEnum State => LoaderMoveStateEnum.Home;

        public override FoupAccessModeEnum GetFoupAccessMode(int cassetteNum)
        {
            return FoupAccessModeEnum.NO_ACCESSED;
        }

        public override EventCodeEnum JogAbsMove(EnumAxisConstants axis, double val)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Motion.AbsMove(axis, val);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.Jog, null);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum JogRelMove(EnumAxisConstants axis, double val)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Motion.RelMove(axis, val);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.Jog, null);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum RetractAll(LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.MovingMethods.RetractAll(movingType);



                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.Retracted, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum ScanCameraStartPosMove(IScanCameraModule ScanCamera, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;


            retVal = Module.MovingMethods.RetractAll();

            if (retVal == EventCodeEnum.NONE)
                retVal = Module.MovingMethods.FoupCoverDown(Cassette);

            if (retVal == EventCodeEnum.NONE)
                retVal = Module.MovingMethods.ScanCameraSlot1PosMove(ScanCamera, Cassette);

            StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.ScanCameraHome, new AccessInfo(ScanCamera, Cassette));

            return retVal;
        }

        public override EventCodeEnum ScanSensorStartPosMove(IScanSensorModule ScanSensor, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;


            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = Module.MovingMethods.FoupCoverDown(Cassette);
                }

                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = Module.MovingMethods.ScanSensorStartPosMove(ScanSensor, Cassette);
                }

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.ScanSensorHome, new AccessInfo(ScanSensor, Cassette));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum SlotUpMove(IARMModule ARM, ISlotModule Slot)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            retVal = Module.MovingMethods.RetractAll();

            if (retVal == EventCodeEnum.NONE)
                retVal = Module.MovingMethods.FoupCoverDown(Slot.Cassette);

            if (retVal == EventCodeEnum.NONE)
                retVal = Module.MovingMethods.SlotUpMove(ARM, Slot);

            StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.SlotUp, new AccessInfo(ARM, Slot));

            return retVal;
        }

        public override EventCodeEnum SlotDownMove(IARMModule ARM, ISlotModule Slot)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            retVal = Module.MovingMethods.RetractAll();

            if (retVal == EventCodeEnum.NONE)
                retVal = Module.MovingMethods.FoupCoverDown(Slot.Cassette);

            if (retVal == EventCodeEnum.NONE)
                retVal = Module.MovingMethods.SlotDownMove(ARM, Slot);

            StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.SlotDown, new AccessInfo(ARM, Slot));

            return retVal;
        }

        public override EventCodeEnum PreAlignUpMove(IARMModule ARM, IPreAlignModule PA, LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            retVal = Module.MovingMethods.RetractAll();

            if (retVal == EventCodeEnum.NONE)
                retVal = Module.MovingMethods.PreAlignUpMove(ARM, PA, movingType);

            StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreAlignUp, new AccessInfo(ARM, PA));

            return retVal;
        }

        public override EventCodeEnum PreAlignDownMove(IARMModule ARM, IPreAlignModule PA)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            retVal = Module.MovingMethods.RetractAll();

            if (retVal == EventCodeEnum.NONE)
                retVal = Module.MovingMethods.PreAlignDownMove(ARM, PA);

            StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreAlignDown, new AccessInfo(ARM, PA));

            return retVal;
        }

        public override EventCodeEnum InspectionTrayUpMove(IARMModule ARM, IInspectionTrayModule InspectionTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            retVal = Module.MovingMethods.RetractAll();

            if (retVal == EventCodeEnum.NONE)
            {
                var CollisionCassette = GetColisionCassette(InspectionTray);
                if (CollisionCassette != null)
                    retVal = Module.MovingMethods.FoupCoverDown(CollisionCassette);
            }

            if (retVal == EventCodeEnum.NONE)
                retVal = Module.MovingMethods.InspectionTrayUpMove(ARM, InspectionTray);

            StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.InspectionTrayUp, new AccessInfo(ARM, InspectionTray));

            return retVal;
        }

        public override EventCodeEnum InspectionTrayDownMove(IARMModule ARM, IInspectionTrayModule InspectionTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            retVal = Module.MovingMethods.RetractAll();

            if (retVal == EventCodeEnum.NONE)
            {
                var CollisionCassette = GetColisionCassette(InspectionTray);
                if (CollisionCassette != null)
                    retVal = Module.MovingMethods.FoupCoverDown(CollisionCassette);
            }

            if (retVal == EventCodeEnum.NONE)
                retVal = Module.MovingMethods.InspectionTrayDownMove(ARM, InspectionTray);

            StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.InspectionTrayDown, new AccessInfo(ARM, InspectionTray));

            return retVal;
        }

        public override EventCodeEnum FixedTrayUpMove(IARMModule ARM, IFixedTrayModule FixedTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            retVal = Module.MovingMethods.RetractAll();

            if (retVal == EventCodeEnum.NONE)
                retVal = Module.MovingMethods.FixedTrayUpMove(ARM, FixedTray);

            StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.FixedTrayUp, new AccessInfo(ARM, FixedTray));

            return retVal;
        }

        public override EventCodeEnum FixedTrayDownMove(IARMModule ARM, IFixedTrayModule FixedTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            retVal = Module.MovingMethods.RetractAll();

            if (retVal == EventCodeEnum.NONE)
                retVal = Module.MovingMethods.FixedTrayDownMove(ARM, FixedTray);

            StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.FixedTrayDown, new AccessInfo(ARM, FixedTray));

            return retVal;
        }

        public override EventCodeEnum PreChuckUpMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            retVal = Module.MovingMethods.RetractAll();

            if (retVal == EventCodeEnum.NONE)
                retVal = Module.MovingMethods.PreChuckUpMove(ARM, Chuck);

            StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreChuck, new AccessInfo(ARM, Chuck));

            return retVal;
        }

        public override EventCodeEnum PreChuckDownMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            retVal = Module.MovingMethods.RetractAll();

            if (retVal == EventCodeEnum.NONE)
                retVal = Module.MovingMethods.PreChuckDownMove(ARM, Chuck);

            StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreChuck, new AccessInfo(ARM, Chuck));

            return retVal;
        }
    }

    public class Retracted : LoaderMoveStateBase
    {
        public Retracted(LoaderMove module) : base(module) { }

        public override LoaderMoveStateEnum State => LoaderMoveStateEnum.Retracted;

        public override FoupAccessModeEnum GetFoupAccessMode(int cassetteNum)
        {
            return FoupAccessModeEnum.NO_ACCESSED;
        }

        public override EventCodeEnum PreAlignZeroMove(IPreAlignModule PA)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (PA is IPreAlignModule)
                {
                    var axisType = (PA as IPreAlignModule).Definition.AxisType;
                    retVal = Motion.AbsMove(axisType.Value, 0);
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
                }

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.Retracted, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum RetractAll(LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.MovingMethods.RetractAll(movingType);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.Retracted, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum ScanCameraStartPosMove(IScanCameraModule ScanCamera, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.ScanCameraSlot1PosMove(ScanCamera, Cassette);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.ScanCameraHome, new AccessInfo(ScanCamera, Cassette));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum ScanSensorStartPosMove(IScanSensorModule ScanSensor, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.ScanSensorStartPosMove(ScanSensor, Cassette);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.ScanSensorHome, new AccessInfo(ScanSensor, Cassette));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum SlotUpMove(IARMModule ARM, ISlotModule Slot)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Slot.Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.SlotUpMove(ARM, Slot);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.SlotUp, new AccessInfo(ARM, Slot));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum SlotDownMove(IARMModule ARM, ISlotModule Slot)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Slot.Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.SlotDownMove(ARM, Slot);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.SlotDown, new AccessInfo(ARM, Slot));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum PreAlignUpMove(IARMModule ARM, IPreAlignModule PA, LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.MovingMethods.RetractAll(movingType);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreAlignUpMove(ARM, PA, movingType);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreAlignUp, new AccessInfo(ARM, PA));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum PreAlignDownMove(IARMModule ARM, IPreAlignModule PA)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreAlignDownMove(ARM, PA);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreAlignDown, new AccessInfo(ARM, PA));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum InspectionTrayUpMove(IARMModule ARM, IInspectionTrayModule InspectionTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                {
                    var CollisionCassette = GetColisionCassette(InspectionTray);
                    if (CollisionCassette != null)
                        retVal = Module.MovingMethods.FoupCoverDown(CollisionCassette);
                }

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.InspectionTrayUpMove(ARM, InspectionTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.InspectionTrayUp, new AccessInfo(ARM, InspectionTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum InspectionTrayDownMove(IARMModule ARM, IInspectionTrayModule InspectionTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                {
                    var CollisionCassette = GetColisionCassette(InspectionTray);
                    if (CollisionCassette != null)
                        retVal = Module.MovingMethods.FoupCoverDown(CollisionCassette);
                }

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.InspectionTrayDownMove(ARM, InspectionTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.InspectionTrayDown, new AccessInfo(ARM, InspectionTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum FixedTrayUpMove(IARMModule ARM, IFixedTrayModule FixedTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FixedTrayUpMove(ARM, FixedTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.FixedTrayUp, new AccessInfo(ARM, FixedTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum FixedTrayDownMove(IARMModule ARM, IFixedTrayModule FixedTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FixedTrayDownMove(ARM, FixedTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.FixedTrayDown, new AccessInfo(ARM, FixedTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum PreChuckUpMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreChuckUpMove(ARM, Chuck);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreChuck, new AccessInfo(ARM, Chuck));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum PreChuckDownMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreChuckDownMove(ARM, Chuck);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreChuck, new AccessInfo(ARM, Chuck));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum SetupToChuckMove(IARMModule ARM, IChuckModule Chuck, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool uaxisskip, int slotnum, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.MovingMethods.RetractAll();
                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = Module.MovingMethods.SetupToChuckMoveMethod(ARM, Chuck, subtype, subsize, uaxisskip, slotnum, index);
                }

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.LoaderSetup, new AccessInfo(ARM, Chuck));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        // 1acces
        public override EventCodeEnum SetupToCstMove(IARMModule ARM, ISlotModule Slot, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool uaxisskip, int slotnum, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.MovingMethods.RetractAll();
                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = Module.MovingMethods.SetupToCstMoveMethod(ARM, Slot, subtype, subsize, uaxisskip, slotnum, index);
                }

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.LoaderSetup, new AccessInfo(ARM, Slot));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override EventCodeEnum SetupToCstSlot(IScanSensorModule ScanSensor, ICassetteModule Cassette, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool uaxisskip, int slotnum, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.MovingMethods.RetractAll();
                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = Module.MovingMethods.SetupToCstSlot1Method(ScanSensor, Cassette, subtype, subsize, uaxisskip, slotnum, 1);
                }

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.LoaderSetup, new AccessInfo(ScanSensor, Cassette));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum SetupToFixedTrayMove(IARMModule ARM, IFixedTrayModule FixedTray, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool uaxisskip, int slotnum, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.MovingMethods.RetractAll();
                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = Module.MovingMethods.SetupToFixedTrayMoveMethod(ARM, FixedTray, subtype, subsize, uaxisskip, slotnum, index);
                }

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.LoaderSetup, new AccessInfo(ARM, FixedTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override EventCodeEnum SetupToInspectionTrayMove(IARMModule ARM, IInspectionTrayModule InspectionTray, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool uaxisskip, int slotnum, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.MovingMethods.RetractAll();
                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = Module.MovingMethods.SetupToInspectionTrayMoveMethod(ARM, InspectionTray, subtype, subsize, uaxisskip, slotnum, index);
                }

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.LoaderSetup, new AccessInfo(ARM, InspectionTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum SetupToOCRMove(IARMModule ARM, IPreAlignModule PA, IOCRReadable OCR, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool uaxisskip, int slotnum, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.MovingMethods.RetractAll();
                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = Module.MovingMethods.SetupToOCRMoveMethod(ARM, PA, OCR, subtype, subsize, uaxisskip, slotnum, index);
                }

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.LoaderSetup, new AccessInfo(ARM, OCR));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override EventCodeEnum SetupToPAMove(IARMModule ARM, IPreAlignModule PA, LoaderMovingTypeEnum movingType, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool uaxisskip, int slotnum, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.MovingMethods.RetractAll();
                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = Module.MovingMethods.SetupToPAMoveMethod(ARM, PA, LoaderMovingTypeEnum.NORMAL, subtype, subsize, uaxisskip, slotnum, index);
                }

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.LoaderSetup, new AccessInfo(ARM, PA));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class ScanCamera : LoaderMoveStateBase
    {
        public ScanCamera(LoaderMove module) : base(module) { }

        public override LoaderMoveStateEnum State => LoaderMoveStateEnum.ScanCameraHome;

        public override FoupAccessModeEnum GetFoupAccessMode(int cassetteNum)
        {
            FoupAccessModeEnum retVal = FoupAccessModeEnum.UNKNOWN;
            try
            {
                retVal = FoupAccessModeEnum.NO_ACCESSED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ScanCameraRelMove(EnumAxisConstants axis, double val)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (axis == EnumAxisConstants.A || axis == EnumAxisConstants.W)
                {
                    retVal = Motion.RelMove(axis, val);
                }
                else
                {
                    retVal = RaiseInvalidState();
                }

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.ScanCameraHome, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum RetractAll(LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.MovingMethods.RetractAll(movingType);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.Retracted, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum ScanCameraStartPosMove(IScanCameraModule ScanCamera, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.ScanCameraSlot1PosMove(ScanCamera, Cassette);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.ScanCameraHome, new AccessInfo(ScanCamera, Cassette));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum ScanSensorStartPosMove(IScanSensorModule ScanSensor, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.ScanSensorStartPosMove(ScanSensor, Cassette);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.ScanSensorHome, new AccessInfo(ScanSensor, Cassette));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum SlotUpMove(IARMModule ARM, ISlotModule Slot)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Slot.Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.SlotUpMove(ARM, Slot);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.SlotUp, new AccessInfo(ARM, Slot));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum SlotDownMove(IARMModule ARM, ISlotModule Slot)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Slot.Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.SlotDownMove(ARM, Slot);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.SlotDown, new AccessInfo(ARM, Slot));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreAlignUpMove(IARMModule ARM, IPreAlignModule PA, LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreAlignUpMove(ARM, PA, movingType);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreAlignUp, new AccessInfo(ARM, PA));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreAlignDownMove(IARMModule ARM, IPreAlignModule PA)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreAlignDownMove(ARM, PA);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreAlignDown, new AccessInfo(ARM, PA));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum InspectionTrayUpMove(IARMModule ARM, IInspectionTrayModule InspectionTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                {
                    var CollisionCassette = GetColisionCassette(InspectionTray);
                    if (CollisionCassette != null)
                        retVal = Module.MovingMethods.FoupCoverUp(CollisionCassette);
                }

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.InspectionTrayUpMove(ARM, InspectionTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.InspectionTrayUp, new AccessInfo(ARM, InspectionTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum InspectionTrayDownMove(IARMModule ARM, IInspectionTrayModule InspectionTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                {
                    var CollisionCassette = GetColisionCassette(InspectionTray);
                    if (CollisionCassette != null)
                        retVal = Module.MovingMethods.FoupCoverUp(CollisionCassette);
                }

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.InspectionTrayDownMove(ARM, InspectionTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.InspectionTrayDown, new AccessInfo(ARM, InspectionTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum FixedTrayUpMove(IARMModule ARM, IFixedTrayModule FixedTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FixedTrayUpMove(ARM, FixedTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.FixedTrayUp, new AccessInfo(ARM, FixedTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum FixedTrayDownMove(IARMModule ARM, IFixedTrayModule FixedTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FixedTrayDownMove(ARM, FixedTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.FixedTrayDown, new AccessInfo(ARM, FixedTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreChuckUpMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreChuckUpMove(ARM, Chuck);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreChuck, new AccessInfo(ARM, Chuck));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreChuckDownMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreChuckDownMove(ARM, Chuck);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreChuck, new AccessInfo(ARM, Chuck));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class ScanSensorHome : LoaderMoveStateBase
    {
        public ScanSensorHome(LoaderMove module) : base(module) { }

        public override LoaderMoveStateEnum State => LoaderMoveStateEnum.ScanSensorHome;

        public override FoupAccessModeEnum GetFoupAccessMode(int cassetteNum)
        {
            return FoupAccessModeEnum.NO_ACCESSED;
        }

        public override EventCodeEnum ExtendScanSensor(IScanSensorModule ScanSensor)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (Module.AccessInfo.Accessor == ScanSensor)
                {
                    retVal = Module.MovingMethods.ScanSensorOut(ScanSensor);
                }
                else
                {
                    retVal = RaiseInvalidState();
                }

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.ScanSensorExtended, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum RetractAll(LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll(movingType);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.Retracted, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ScanCameraStartPosMove(IScanCameraModule ScanCamera, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.ScanCameraSlot1PosMove(ScanCamera, Cassette);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.ScanCameraHome, new AccessInfo(ScanCamera, Cassette));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ScanSensorStartPosMove(IScanSensorModule ScanSensor, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.ScanSensorStartPosMove(ScanSensor, Cassette);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.ScanSensorHome, new AccessInfo(ScanSensor, Cassette));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum SlotUpMove(IARMModule ARM, ISlotModule Slot)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Slot.Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.SlotUpMove(ARM, Slot);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.SlotUp, new AccessInfo(ARM, Slot));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum SlotDownMove(IARMModule ARM, ISlotModule Slot)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Slot.Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.SlotDownMove(ARM, Slot);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.SlotDown, new AccessInfo(ARM, Slot));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreAlignUpMove(IARMModule ARM, IPreAlignModule PA, LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreAlignUpMove(ARM, PA, movingType);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreAlignUp, new AccessInfo(ARM, PA));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreAlignDownMove(IARMModule ARM, IPreAlignModule PA)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreAlignDownMove(ARM, PA);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreAlignDown, new AccessInfo(ARM, PA));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum InspectionTrayUpMove(IARMModule ARM, IInspectionTrayModule InspectionTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                {
                    var CollisionCassette = GetColisionCassette(InspectionTray);
                    if (CollisionCassette != null)
                        retVal = Module.MovingMethods.FoupCoverDown(CollisionCassette);
                }

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.InspectionTrayUpMove(ARM, InspectionTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.InspectionTrayUp, new AccessInfo(ARM, InspectionTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum InspectionTrayDownMove(IARMModule ARM, IInspectionTrayModule InspectionTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                {
                    var CollisionCassette = GetColisionCassette(InspectionTray);
                    if (CollisionCassette != null)
                        retVal = Module.MovingMethods.FoupCoverDown(CollisionCassette);
                }

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.InspectionTrayDownMove(ARM, InspectionTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.InspectionTrayDown, new AccessInfo(ARM, InspectionTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum FixedTrayUpMove(IARMModule ARM, IFixedTrayModule FixedTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FixedTrayUpMove(ARM, FixedTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.FixedTrayUp, new AccessInfo(ARM, FixedTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum FixedTrayDownMove(IARMModule ARM, IFixedTrayModule FixedTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FixedTrayDownMove(ARM, FixedTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.FixedTrayDown, new AccessInfo(ARM, FixedTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreChuckUpMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreChuckUpMove(ARM, Chuck);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreChuck, new AccessInfo(ARM, Chuck));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreChuckDownMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreChuckDownMove(ARM, Chuck);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreChuck, new AccessInfo(ARM, Chuck));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class ScanSensorOut : LoaderMoveStateBase
    {
        public ScanSensorOut(LoaderMove module) : base(module) { }

        public override LoaderMoveStateEnum State => LoaderMoveStateEnum.ScanSensorExtended;

        public override FoupAccessModeEnum GetFoupAccessMode(int cassetteNum)
        {
            FoupAccessModeEnum foupAccessMode = FoupAccessModeEnum.UNKNOWN;
            try
            {
                var CST = Module.AccessInfo.Target as ICassetteModule;
                if (CST.IsInFoup(cassetteNum))
                    foupAccessMode = FoupAccessModeEnum.ACCESSED;
                else
                    foupAccessMode = FoupAccessModeEnum.NO_ACCESSED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return foupAccessMode;
        }

        public override EventCodeEnum ScanSensorUpMove(IScanSensorModule ScanSensor, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.AccessInfo.Accessor == ScanSensor &&
                    Module.AccessInfo.Target == Cassette)
                {
                    retVal = Module.MovingMethods.ScanSensorUpMove(ScanSensor, Cassette);
                }
                else
                {
                    retVal = RaiseInvalidState();
                }

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.ScanSensorExtended, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ScanSensorDownMove(IScanSensorModule ScanSensor, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.AccessInfo.Accessor == ScanSensor &&
                    Module.AccessInfo.Target == Cassette)
                {
                    retVal = Module.MovingMethods.ScanSensorDownMove(ScanSensor, Cassette);
                }
                else
                {
                    retVal = RaiseInvalidState();
                }

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.ScanSensorHome, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class SlotUp : LoaderMoveStateBase
    {
        public SlotUp(LoaderMove module) : base(module) { }

        public override LoaderMoveStateEnum State => LoaderMoveStateEnum.SlotUp;

        public override FoupAccessModeEnum GetFoupAccessMode(int cassetteNum)
        {
            FoupAccessModeEnum foupAccessMode = FoupAccessModeEnum.UNKNOWN;
            try
            {
                var SLOT = Module.AccessInfo.Target as ISlotModule;
                if (SLOT.Cassette.IsInFoup(cassetteNum))
                    foupAccessMode = FoupAccessModeEnum.ACCESSED;
                else
                    foupAccessMode = FoupAccessModeEnum.NO_ACCESSED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return foupAccessMode;
        }

        public override EventCodeEnum PlaceDown(IARMModule ARM, IWaferOwnable Target,
            LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.ACCESS)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.AccessInfo.Accessor == ARM &&
                    Module.AccessInfo.Target == Target)
                {
                    var Slot = Module.AccessInfo.Target as ISlotModule;

                    if (ARM.Holder.TransferObject != null)
                    {
                        var accparam = Slot.Cassette.GetSlot1AccessParam(ARM.Holder.TransferObject.Type.Value, ARM.Holder.TransferObject.Size.Value);

                        double value = accparam.PickupIncrement.Value * -1.0;

                        retVal = Module.MovingMethods.RelMove(EnumAxisConstants.A, value, movingType);
                    }
                }
                else
                {
                    retVal = RaiseInvalidState();
                }

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.SlotDown, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum RetractAll(LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll(movingType);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.Retracted, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ScanCameraStartPosMove(IScanCameraModule ScanCamera, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.ScanCameraSlot1PosMove(ScanCamera, Cassette);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.ScanCameraHome, new AccessInfo(ScanCamera, Cassette));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ScanSensorStartPosMove(IScanSensorModule ScanSensor, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.ScanSensorStartPosMove(ScanSensor, Cassette);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.ScanSensorHome, new AccessInfo(ScanSensor, Cassette));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum SlotUpMove(IARMModule ARM, ISlotModule Slot)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Slot.Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.SlotUpMove(ARM, Slot);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.SlotUp, new AccessInfo(ARM, Slot));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum SlotDownMove(IARMModule ARM, ISlotModule Slot)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Slot.Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.SlotDownMove(ARM, Slot);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.SlotDown, new AccessInfo(ARM, Slot));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreAlignUpMove(IARMModule ARM, IPreAlignModule PA, LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreAlignUpMove(ARM, PA, movingType);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreAlignUp, new AccessInfo(ARM, PA));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreAlignDownMove(IARMModule ARM, IPreAlignModule PA)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreAlignDownMove(ARM, PA);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreAlignDown, new AccessInfo(ARM, PA));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum InspectionTrayUpMove(IARMModule ARM, IInspectionTrayModule InspectionTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                {
                    var CollisionCassette = GetColisionCassette(InspectionTray);
                    if (CollisionCassette != null)
                        retVal = Module.MovingMethods.FoupCoverUp(CollisionCassette);
                }

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.InspectionTrayUpMove(ARM, InspectionTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.InspectionTrayUp, new AccessInfo(ARM, InspectionTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum InspectionTrayDownMove(IARMModule ARM, IInspectionTrayModule InspectionTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                {
                    var CollisionCassette = GetColisionCassette(InspectionTray);
                    if (CollisionCassette != null)
                        retVal = Module.MovingMethods.FoupCoverUp(CollisionCassette);
                }

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.InspectionTrayDownMove(ARM, InspectionTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.InspectionTrayDown, new AccessInfo(ARM, InspectionTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum FixedTrayUpMove(IARMModule ARM, IFixedTrayModule FixedTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FixedTrayUpMove(ARM, FixedTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.FixedTrayUp, new AccessInfo(ARM, FixedTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum FixedTrayDownMove(IARMModule ARM, IFixedTrayModule FixedTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FixedTrayDownMove(ARM, FixedTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.FixedTrayDown, new AccessInfo(ARM, FixedTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreChuckUpMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreChuckUpMove(ARM, Chuck);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreChuck, new AccessInfo(ARM, Chuck));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreChuckDownMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreChuckDownMove(ARM, Chuck);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreChuck, new AccessInfo(ARM, Chuck));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class SlotDown : LoaderMoveStateBase
    {
        public SlotDown(LoaderMove module) : base(module) { }

        public override LoaderMoveStateEnum State => LoaderMoveStateEnum.SlotDown;

        public override FoupAccessModeEnum GetFoupAccessMode(int cassetteNum)
        {
            FoupAccessModeEnum foupAccessMode = FoupAccessModeEnum.UNKNOWN;

            try
            {
                var SLOT = Module.AccessInfo.Target as ISlotModule;
                if (SLOT.Cassette.IsInFoup(cassetteNum))
                    foupAccessMode = FoupAccessModeEnum.ACCESSED;
                else
                    foupAccessMode = FoupAccessModeEnum.NO_ACCESSED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return foupAccessMode;
        }

        public override EventCodeEnum PickUp(IARMModule ARM, IWaferOwnable Target, LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.ACCESS)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.AccessInfo.Accessor == ARM &&
                    Module.AccessInfo.Target == Target)
                {
                    var Slot = Module.AccessInfo.Target as ISlotModule;

                    var accparam = Slot.Cassette.GetSlot1AccessParam(Slot.Holder.TransferObject.Type.Value, Slot.Holder.TransferObject.Size.Value);

                    double value = accparam.PickupIncrement.Value;

                    retVal = Module.MovingMethods.RelMove(EnumAxisConstants.A, value, movingType);
                }
                else
                {
                    retVal = RaiseInvalidState();
                }

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.SlotUp, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum RetractAll(LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll(movingType);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.Retracted, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ScanCameraStartPosMove(IScanCameraModule ScanCamera, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.ScanCameraSlot1PosMove(ScanCamera, Cassette);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.ScanCameraHome, new AccessInfo(ScanCamera, Cassette));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ScanSensorStartPosMove(IScanSensorModule ScanSensor, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.ScanSensorStartPosMove(ScanSensor, Cassette);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.ScanSensorHome, new AccessInfo(ScanSensor, Cassette));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum SlotUpMove(IARMModule ARM, ISlotModule Slot)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Slot.Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.SlotUpMove(ARM, Slot);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.SlotUp, new AccessInfo(ARM, Slot));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum SlotDownMove(IARMModule ARM, ISlotModule Slot)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Slot.Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.SlotDownMove(ARM, Slot);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.SlotDown, new AccessInfo(ARM, Slot));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreAlignUpMove(IARMModule ARM, IPreAlignModule PA, LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreAlignUpMove(ARM, PA, movingType);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreAlignUp, new AccessInfo(ARM, PA));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreAlignDownMove(IARMModule ARM, IPreAlignModule PA)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreAlignDownMove(ARM, PA);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreAlignDown, new AccessInfo(ARM, PA));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum InspectionTrayUpMove(IARMModule ARM, IInspectionTrayModule InspectionTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                {
                    var CollisionCassette = GetColisionCassette(InspectionTray);
                    if (CollisionCassette != null)
                        retVal = Module.MovingMethods.FoupCoverUp(CollisionCassette);
                }

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.InspectionTrayUpMove(ARM, InspectionTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.InspectionTrayUp, new AccessInfo(ARM, InspectionTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum InspectionTrayDownMove(IARMModule ARM, IInspectionTrayModule InspectionTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                {
                    var CollisionCassette = GetColisionCassette(InspectionTray);
                    if (CollisionCassette != null)
                        retVal = Module.MovingMethods.FoupCoverUp(CollisionCassette);
                }

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.InspectionTrayDownMove(ARM, InspectionTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.InspectionTrayDown, new AccessInfo(ARM, InspectionTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum FixedTrayUpMove(IARMModule ARM, IFixedTrayModule FixedTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FixedTrayUpMove(ARM, FixedTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.FixedTrayUp, new AccessInfo(ARM, FixedTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum FixedTrayDownMove(IARMModule ARM, IFixedTrayModule FixedTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FixedTrayDownMove(ARM, FixedTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.FixedTrayDown, new AccessInfo(ARM, FixedTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreChuckUpMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreChuckUpMove(ARM, Chuck);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreChuck, new AccessInfo(ARM, Chuck));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreChuckDownMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreChuckDownMove(ARM, Chuck);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreChuck, new AccessInfo(ARM, Chuck));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class PreAlignUp : LoaderMoveStateBase
    {
        public PreAlignUp(LoaderMove module) : base(module) { }

        public override LoaderMoveStateEnum State => LoaderMoveStateEnum.PreAlignUp;

        public override FoupAccessModeEnum GetFoupAccessMode(int cassetteNum)
        {
            return FoupAccessModeEnum.NO_ACCESSED;
        }

        public override EventCodeEnum OCRMoveFromPreAlignUp(IARMModule ARM, IOCRReadable OCR, IPreAlignModule PA)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.AccessInfo.Accessor == ARM &&
                    Module.AccessInfo.Target == PA)
                {
                    retVal = Module.MovingMethods.OCRMoveFromPreAlignUp(ARM, OCR, PA);
                }
                else
                {
                    retVal = RaiseInvalidState();
                }

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.OCR, new AccessInfo(ARM, OCR));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PlaceDown(IARMModule ARM, IWaferOwnable Target, LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.ACCESS)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.AccessInfo.Accessor == ARM &&
                    Module.AccessInfo.Target == Target)
                {
                    var PA = Module.AccessInfo.Target as IPreAlignModule;

                    var accparam = PA.GetAccessParam(ARM.Holder.TransferObject.Type.Value, ARM.Holder.TransferObject.Size.Value);

                    double value = accparam.PickupIncrement.Value * -1.0;

                    retVal = Module.MovingMethods.RelMove(EnumAxisConstants.A, value, movingType);
                }
                else
                {
                    retVal = RaiseInvalidState();
                }

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreAlignDown, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreAlignRelMove(IARMModule ARM, double val)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Module.AccessInfo.Accessor == ARM)
                {
                    retVal = Motion.RelMove(ARM.Definition.AxisType.Value, val);
                }
                else
                {
                    retVal = RaiseInvalidState();
                }

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreAlignUp, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreAlignRelMove(IPreAlignModule PA, double val)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (Module.AccessInfo.Target == PA &&
                PA is IPreAlignModule)
                {
                    var axisType = (PA as IPreAlignModule).Definition.AxisType;
                    retVal = Motion.RelMove(axisType.Value, val);
                }
                else
                {
                    retVal = RaiseInvalidState();
                }

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreAlignUp, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum RetractAll(LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll(movingType);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.Retracted, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ScanCameraStartPosMove(IScanCameraModule ScanCamera, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.ScanCameraSlot1PosMove(ScanCamera, Cassette);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.ScanCameraHome, new AccessInfo(ScanCamera, Cassette));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ScanSensorStartPosMove(IScanSensorModule ScanSensor, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.ScanSensorStartPosMove(ScanSensor, Cassette);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.ScanSensorHome, new AccessInfo(ScanSensor, Cassette));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum SlotUpMove(IARMModule ARM, ISlotModule Slot)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Slot.Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.SlotUpMove(ARM, Slot);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.SlotUp, new AccessInfo(ARM, Slot));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum SlotDownMove(IARMModule ARM, ISlotModule Slot)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Slot.Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.SlotDownMove(ARM, Slot);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.SlotDown, new AccessInfo(ARM, Slot));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreAlignUpMove(IARMModule ARM, IPreAlignModule PA, LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreAlignUpMove(ARM, PA, movingType);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreAlignUp, new AccessInfo(ARM, PA));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreAlignDownMove(IARMModule ARM, IPreAlignModule PA)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreAlignDownMove(ARM, PA);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreAlignDown, new AccessInfo(ARM, PA));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum InspectionTrayUpMove(IARMModule ARM, IInspectionTrayModule InspectionTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                {
                    var CollisionCassette = GetColisionCassette(InspectionTray);
                    if (CollisionCassette != null)
                        retVal = Module.MovingMethods.FoupCoverUp(CollisionCassette);
                }

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.InspectionTrayUpMove(ARM, InspectionTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.InspectionTrayUp, new AccessInfo(ARM, InspectionTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum InspectionTrayDownMove(IARMModule ARM, IInspectionTrayModule InspectionTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                {
                    var CollisionCassette = GetColisionCassette(InspectionTray);
                    if (CollisionCassette != null)
                        retVal = Module.MovingMethods.FoupCoverUp(CollisionCassette);
                }

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.InspectionTrayDownMove(ARM, InspectionTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.InspectionTrayDown, new AccessInfo(ARM, InspectionTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum FixedTrayUpMove(IARMModule ARM, IFixedTrayModule FixedTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FixedTrayUpMove(ARM, FixedTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.FixedTrayUp, new AccessInfo(ARM, FixedTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum FixedTrayDownMove(IARMModule ARM, IFixedTrayModule FixedTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FixedTrayDownMove(ARM, FixedTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.FixedTrayDown, new AccessInfo(ARM, FixedTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreChuckUpMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreChuckUpMove(ARM, Chuck);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreChuck, new AccessInfo(ARM, Chuck));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreChuckDownMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreChuckDownMove(ARM, Chuck);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreChuck, new AccessInfo(ARM, Chuck));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

    }

    public class PreAlignDown : LoaderMoveStateBase
    {
        public PreAlignDown(LoaderMove module) : base(module) { }

        public override LoaderMoveStateEnum State => LoaderMoveStateEnum.PreAlignDown;

        public override FoupAccessModeEnum GetFoupAccessMode(int cassetteNum)
        {
            return FoupAccessModeEnum.NO_ACCESSED;
        }

        public override EventCodeEnum PickUp(IARMModule ARM, IWaferOwnable Target, LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.ACCESS)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (Module.AccessInfo.Accessor == ARM &&
                    Module.AccessInfo.Target == Target)
                {
                    var PA = Module.AccessInfo.Target as IPreAlignModule;

                    var accparam = PA.GetAccessParam(Target.Holder.TransferObject.Type.Value, Target.Holder.TransferObject.Size.Value);

                    double value = accparam.PickupIncrement.Value;

                    retVal = Module.MovingMethods.RelMove(EnumAxisConstants.A, value, movingType);
                }
                else
                {
                    retVal = RaiseInvalidState();
                }


                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreAlignUp, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreAlignRelMove(IARMModule ARM, double val)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (Module.AccessInfo.Accessor == ARM)
                {
                    retVal = Motion.RelMove(ARM.Definition.AxisType.Value, val);
                }
                else
                {
                    retVal = RaiseInvalidState();
                }

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreAlignDown, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreAlignRelMove(IPreAlignModule PA, double val)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (Module.AccessInfo.Target == PA &&
                        PA is IPreAlignModule)
                {
                    var axisType = (PA as IPreAlignModule).Definition.AxisType;
                    retVal = Motion.RelMove(axisType.Value, val);
                }
                else
                {
                    retVal = RaiseInvalidState();
                }

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreAlignDown, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PAFindNotchMove(IPreAlignModule PA, EnumMotorDedicatedIn input)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (Module.AccessInfo.Target == PA &&
                        PA is IPreAlignModule)
                {
                    var axisType = (PA as IPreAlignModule).Definition.AxisType;
                    retVal = Motion.NotchFindMove(axisType.Value, input);
                }
                else
                {
                    retVal = RaiseInvalidState();
                }

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreAlignDown, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum RetractAll(LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll(movingType);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.Retracted, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ScanCameraStartPosMove(IScanCameraModule ScanCamera, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.ScanCameraSlot1PosMove(ScanCamera, Cassette);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.ScanCameraHome, new AccessInfo(ScanCamera, Cassette));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ScanSensorStartPosMove(IScanSensorModule ScanSensor, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.ScanSensorStartPosMove(ScanSensor, Cassette);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.ScanSensorHome, new AccessInfo(ScanSensor, Cassette));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum SlotUpMove(IARMModule ARM, ISlotModule Slot)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Slot.Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.SlotUpMove(ARM, Slot);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.SlotUp, new AccessInfo(ARM, Slot));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum SlotDownMove(IARMModule ARM, ISlotModule Slot)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Slot.Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.SlotDownMove(ARM, Slot);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.SlotDown, new AccessInfo(ARM, Slot));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreAlignUpMove(IARMModule ARM, IPreAlignModule PA, LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreAlignUpMove(ARM, PA, movingType);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreAlignUp, new AccessInfo(ARM, PA));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreAlignDownMove(IARMModule ARM, IPreAlignModule PA)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (Module.AccessInfo.Accessor == ARM &&
                        Module.AccessInfo.Target == PA)
                {
                    //No Safety Move
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = Module.MovingMethods.RetractAll();
                }

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreAlignDownMove(ARM, PA);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreAlignDown, new AccessInfo(ARM, PA));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum InspectionTrayUpMove(IARMModule ARM, IInspectionTrayModule InspectionTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                {
                    var CollisionCassette = GetColisionCassette(InspectionTray);
                    if (CollisionCassette != null)
                        retVal = Module.MovingMethods.FoupCoverUp(CollisionCassette);
                }

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.InspectionTrayUpMove(ARM, InspectionTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.InspectionTrayUp, new AccessInfo(ARM, InspectionTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum InspectionTrayDownMove(IARMModule ARM, IInspectionTrayModule InspectionTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                {
                    var CollisionCassette = GetColisionCassette(InspectionTray);
                    if (CollisionCassette != null)
                        retVal = Module.MovingMethods.FoupCoverUp(CollisionCassette);
                }

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.InspectionTrayDownMove(ARM, InspectionTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.InspectionTrayDown, new AccessInfo(ARM, InspectionTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum FixedTrayUpMove(IARMModule ARM, IFixedTrayModule FixedTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FixedTrayUpMove(ARM, FixedTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.FixedTrayUp, new AccessInfo(ARM, FixedTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum FixedTrayDownMove(IARMModule ARM, IFixedTrayModule FixedTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FixedTrayDownMove(ARM, FixedTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.FixedTrayDown, new AccessInfo(ARM, FixedTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreChuckUpMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreChuckUpMove(ARM, Chuck);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreChuck, new AccessInfo(ARM, Chuck));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreChuckDownMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreChuckDownMove(ARM, Chuck);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreChuck, new AccessInfo(ARM, Chuck));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class OCR : LoaderMoveStateBase
    {
        public OCR(LoaderMove module) : base(module) { }

        public override LoaderMoveStateEnum State => LoaderMoveStateEnum.OCR;

        public override FoupAccessModeEnum GetFoupAccessMode(int cassetteNum)
        {
            return FoupAccessModeEnum.NO_ACCESSED;
        }

        public override EventCodeEnum PreAlignUpMoveFromOCR(IARMModule ARM, IPreAlignModule PA, IOCRReadable OCR)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (Module.AccessInfo.Accessor == ARM &&
                    Module.AccessInfo.Target == OCR)
                {
                    retVal = Module.MovingMethods.PreAlignUpMoveFromOCR(ARM, PA, OCR);
                }
                else
                {
                    retVal = RaiseInvalidState();
                }

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreAlignUp, new AccessInfo(ARM, PA));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum JogRelMove(EnumAxisConstants axis, double value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                double U_AXIS_MAX = 5000;
                double W_AXIS_MAX = 5;

                if (Module.AccessInfo.Accessor is IARMModule &&
                    Module.AccessInfo.Target is IOCRReadable)
                {
                    var ARM = Module.AccessInfo.Accessor as IARMModule;

                    bool isPerform = false;
                    if (axis == ARM.Definition.AxisType.Value)
                    {
                        isPerform = Math.Abs(value) <= U_AXIS_MAX;
                    }
                    else if (axis == EnumAxisConstants.W)
                    {
                        isPerform = Math.Abs(value) <= W_AXIS_MAX;
                    }

                    if (isPerform)
                        retVal = Module.MovingMethods.RelMove(axis, value, LoaderMovingTypeEnum.NORMAL);
                    else
                        retVal = RaiseInvalidState();
                }
                else
                {
                    retVal = RaiseInvalidState();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

    }

    public class InspectionTrayUp : LoaderMoveStateBase
    {
        public InspectionTrayUp(LoaderMove module) : base(module) { }

        public override LoaderMoveStateEnum State => LoaderMoveStateEnum.InspectionTrayUp;

        public override FoupAccessModeEnum GetFoupAccessMode(int cassetteNum)
        {
            FoupAccessModeEnum foupAccessMode = FoupAccessModeEnum.UNKNOWN;
            try
            {

                var INSP = Module.AccessInfo.Target as IInspectionTrayModule;

                if (INSP.Definition.IsInterferenceWithCassettePort.Value == true)
                {
                    var collisionCassette = Loader.ModuleManager.FindModule(ModuleTypeEnum.CST, INSP.Definition.InterferenceCassettePortNum.Value) as ICassetteModule;

                    if (collisionCassette.IsInFoup(cassetteNum))
                        foupAccessMode = FoupAccessModeEnum.ACCESSED;
                    else
                        foupAccessMode = FoupAccessModeEnum.NO_ACCESSED;
                }
                else
                {
                    foupAccessMode = FoupAccessModeEnum.NO_ACCESSED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return foupAccessMode;
        }

        public override EventCodeEnum PlaceDown(IARMModule ARM, IWaferOwnable Target, LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.ACCESS)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (Module.AccessInfo.Accessor == ARM &&
                        Module.AccessInfo.Target == Target)
                {
                    var INSP = Module.AccessInfo.Target as IInspectionTrayModule;

                    var accparam = INSP.GetAccessParam(ARM.Holder.TransferObject.Type.Value, ARM.Holder.TransferObject.Size.Value);

                    double value = accparam.PickupIncrement.Value * -1.0;

                    Func<bool> stopFunc = () =>
                    {
                        bool isOpend;
                        INSP.ReadOpened(out isOpend);
                        return isOpend;
                    };

                    retVal = Module.MovingMethods.RelMove(EnumAxisConstants.A, value, movingType, stopFunc, false);
                }
                else
                {
                    retVal = RaiseInvalidState();
                }

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.InspectionTrayDown, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum RetractAll(LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll(movingType);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.Retracted, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ScanCameraStartPosMove(IScanCameraModule ScanCamera, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.ScanCameraSlot1PosMove(ScanCamera, Cassette);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.ScanCameraHome, new AccessInfo(ScanCamera, Cassette));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ScanSensorStartPosMove(IScanSensorModule ScanSensor, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.ScanSensorStartPosMove(ScanSensor, Cassette);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.ScanSensorHome, new AccessInfo(ScanSensor, Cassette));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum SlotUpMove(IARMModule ARM, ISlotModule Slot)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Slot.Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.SlotUpMove(ARM, Slot);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.SlotUp, new AccessInfo(ARM, Slot));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum SlotDownMove(IARMModule ARM, ISlotModule Slot)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Slot.Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.SlotDownMove(ARM, Slot);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.SlotDown, new AccessInfo(ARM, Slot));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreAlignUpMove(IARMModule ARM, IPreAlignModule PA, LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreAlignUpMove(ARM, PA, movingType);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreAlignUp, new AccessInfo(ARM, PA));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreAlignDownMove(IARMModule ARM, IPreAlignModule PA)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreAlignDownMove(ARM, PA);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreAlignDown, new AccessInfo(ARM, PA));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum InspectionTrayUpMove(IARMModule ARM, IInspectionTrayModule InspectionTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                {
                    var CollisionCassette = GetColisionCassette(InspectionTray);
                    if (CollisionCassette != null)
                        retVal = Module.MovingMethods.FoupCoverUp(CollisionCassette);
                }

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.InspectionTrayUpMove(ARM, InspectionTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.InspectionTrayUp, new AccessInfo(ARM, InspectionTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum InspectionTrayDownMove(IARMModule ARM, IInspectionTrayModule InspectionTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                {
                    var CollisionCassette = GetColisionCassette(InspectionTray);
                    if (CollisionCassette != null)
                        retVal = Module.MovingMethods.FoupCoverUp(CollisionCassette);
                }

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.InspectionTrayDownMove(ARM, InspectionTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.InspectionTrayDown, new AccessInfo(ARM, InspectionTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum FixedTrayUpMove(IARMModule ARM, IFixedTrayModule FixedTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FixedTrayUpMove(ARM, FixedTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.FixedTrayUp, new AccessInfo(ARM, FixedTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum FixedTrayDownMove(IARMModule ARM, IFixedTrayModule FixedTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FixedTrayDownMove(ARM, FixedTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.FixedTrayDown, new AccessInfo(ARM, FixedTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreChuckUpMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreChuckUpMove(ARM, Chuck);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreChuck, new AccessInfo(ARM, Chuck));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreChuckDownMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreChuckDownMove(ARM, Chuck);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreChuck, new AccessInfo(ARM, Chuck));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

    }

    public class InspectionTrayDown : LoaderMoveStateBase
    {
        public InspectionTrayDown(LoaderMove module) : base(module) { }

        public override LoaderMoveStateEnum State => LoaderMoveStateEnum.InspectionTrayDown;

        public override FoupAccessModeEnum GetFoupAccessMode(int cassetteNum)
        {
            FoupAccessModeEnum foupAccessMode = FoupAccessModeEnum.UNKNOWN;
            try
            {

                var INSP = Module.AccessInfo.Target as IInspectionTrayModule;

                if (INSP.Definition.IsInterferenceWithCassettePort.Value == true)
                {
                    var collisionCassette = Loader.ModuleManager.FindModule(ModuleTypeEnum.CST, INSP.Definition.InterferenceCassettePortNum.Value) as ICassetteModule;

                    if (collisionCassette.IsInFoup(cassetteNum))
                        foupAccessMode = FoupAccessModeEnum.ACCESSED;
                    else
                        foupAccessMode = FoupAccessModeEnum.NO_ACCESSED;
                }
                else
                {
                    foupAccessMode = FoupAccessModeEnum.NO_ACCESSED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return foupAccessMode;
        }

        public override EventCodeEnum PickUp(IARMModule ARM, IWaferOwnable Target, LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.ACCESS)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (Module.AccessInfo.Accessor == ARM &&
                        Module.AccessInfo.Target == Target)
                {
                    var INSP = Module.AccessInfo.Target as IInspectionTrayModule;

                    var accparam = INSP.GetAccessParam(Target.Holder.TransferObject.Type.Value, Target.Holder.TransferObject.Size.Value);

                    double value = accparam.PickupIncrement.Value;

                    Func<bool> stopFunc = () =>
                    {
                        bool isOpend;
                        INSP.ReadOpened(out isOpend);
                        return isOpend;
                    };

                    retVal = Module.MovingMethods.RelMove(EnumAxisConstants.A, value, movingType, stopFunc, false);
                }
                else
                {
                    retVal = RaiseInvalidState();
                }

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.InspectionTrayUp, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum RetractAll(LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll(movingType);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.Retracted, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ScanCameraStartPosMove(IScanCameraModule ScanCamera, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.ScanCameraSlot1PosMove(ScanCamera, Cassette);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.ScanCameraHome, new AccessInfo(ScanCamera, Cassette));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ScanSensorStartPosMove(IScanSensorModule ScanSensor, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.ScanSensorStartPosMove(ScanSensor, Cassette);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.ScanSensorHome, new AccessInfo(ScanSensor, Cassette));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum SlotUpMove(IARMModule ARM, ISlotModule Slot)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Slot.Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.SlotUpMove(ARM, Slot);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.SlotUp, new AccessInfo(ARM, Slot));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum SlotDownMove(IARMModule ARM, ISlotModule Slot)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Slot.Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.SlotDownMove(ARM, Slot);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.SlotDown, new AccessInfo(ARM, Slot));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreAlignUpMove(IARMModule ARM, IPreAlignModule PA, LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreAlignUpMove(ARM, PA, movingType);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreAlignUp, new AccessInfo(ARM, PA));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreAlignDownMove(IARMModule ARM, IPreAlignModule PA)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreAlignDownMove(ARM, PA);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreAlignDown, new AccessInfo(ARM, PA));

            return retVal;
        }

        public override EventCodeEnum InspectionTrayUpMove(IARMModule ARM, IInspectionTrayModule InspectionTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                {
                    var CollisionCassette = GetColisionCassette(InspectionTray);
                    if (CollisionCassette != null)
                        retVal = Module.MovingMethods.FoupCoverUp(CollisionCassette);
                }

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.InspectionTrayUpMove(ARM, InspectionTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.InspectionTrayUp, new AccessInfo(ARM, InspectionTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum InspectionTrayDownMove(IARMModule ARM, IInspectionTrayModule InspectionTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                {
                    var CollisionCassette = GetColisionCassette(InspectionTray);
                    if (CollisionCassette != null)
                        retVal = Module.MovingMethods.FoupCoverUp(CollisionCassette);
                }

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.InspectionTrayDownMove(ARM, InspectionTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.InspectionTrayDown, new AccessInfo(ARM, InspectionTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum FixedTrayUpMove(IARMModule ARM, IFixedTrayModule FixedTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FixedTrayUpMove(ARM, FixedTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.FixedTrayUp, new AccessInfo(ARM, FixedTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum FixedTrayDownMove(IARMModule ARM, IFixedTrayModule FixedTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FixedTrayDownMove(ARM, FixedTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.FixedTrayDown, new AccessInfo(ARM, FixedTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreChuckUpMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreChuckUpMove(ARM, Chuck);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreChuck, new AccessInfo(ARM, Chuck));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreChuckDownMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreChuckDownMove(ARM, Chuck);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreChuck, new AccessInfo(ARM, Chuck));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

    }

    public class FixedTrayUp : LoaderMoveStateBase
    {
        public FixedTrayUp(LoaderMove module) : base(module) { }

        public override LoaderMoveStateEnum State => LoaderMoveStateEnum.FixedTrayUp;

        public override FoupAccessModeEnum GetFoupAccessMode(int cassetteNum)
        {
            return FoupAccessModeEnum.NO_ACCESSED;
        }

        public override EventCodeEnum PlaceDown(IARMModule ARM, IWaferOwnable Target, LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.ACCESS)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (Module.AccessInfo.Accessor == ARM &&
                    Module.AccessInfo.Target == Target)
                {
                    var FIXED = Module.AccessInfo.Target as IFixedTrayModule;

                    var accparam = FIXED.GetAccessParam(ARM.Holder.TransferObject.Type.Value, ARM.Holder.TransferObject.Size.Value);

                    double value = accparam.PickupIncrement.Value * -1.0;

                    retVal = Module.MovingMethods.RelMove(EnumAxisConstants.A, value, movingType);

                }
                else
                {
                    retVal = RaiseInvalidState();
                }

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.FixedTrayDown, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum RetractAll(LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll(movingType);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.Retracted, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ScanCameraStartPosMove(IScanCameraModule ScanCamera, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.ScanCameraSlot1PosMove(ScanCamera, Cassette);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.ScanCameraHome, new AccessInfo(ScanCamera, Cassette));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ScanSensorStartPosMove(IScanSensorModule ScanSensor, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.ScanSensorStartPosMove(ScanSensor, Cassette);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.ScanSensorHome, new AccessInfo(ScanSensor, Cassette));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum SlotUpMove(IARMModule ARM, ISlotModule Slot)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Slot.Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.SlotUpMove(ARM, Slot);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.SlotUp, new AccessInfo(ARM, Slot));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum SlotDownMove(IARMModule ARM, ISlotModule Slot)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Slot.Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.SlotDownMove(ARM, Slot);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.SlotDown, new AccessInfo(ARM, Slot));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreAlignUpMove(IARMModule ARM, IPreAlignModule PA, LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreAlignUpMove(ARM, PA, movingType);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreAlignUp, new AccessInfo(ARM, PA));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreAlignDownMove(IARMModule ARM, IPreAlignModule PA)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreAlignDownMove(ARM, PA);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreAlignDown, new AccessInfo(ARM, PA));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum InspectionTrayUpMove(IARMModule ARM, IInspectionTrayModule InspectionTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                {
                    var CollisionCassette = GetColisionCassette(InspectionTray);
                    if (CollisionCassette != null)
                        retVal = Module.MovingMethods.FoupCoverUp(CollisionCassette);
                }

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.InspectionTrayUpMove(ARM, InspectionTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.InspectionTrayUp, new AccessInfo(ARM, InspectionTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum InspectionTrayDownMove(IARMModule ARM, IInspectionTrayModule InspectionTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                {
                    var CollisionCassette = GetColisionCassette(InspectionTray);
                    if (CollisionCassette != null)
                        retVal = Module.MovingMethods.FoupCoverUp(CollisionCassette);
                }

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.InspectionTrayDownMove(ARM, InspectionTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.InspectionTrayDown, new AccessInfo(ARM, InspectionTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum FixedTrayUpMove(IARMModule ARM, IFixedTrayModule FixedTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FixedTrayUpMove(ARM, FixedTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.FixedTrayUp, new AccessInfo(ARM, FixedTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum FixedTrayDownMove(IARMModule ARM, IFixedTrayModule FixedTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FixedTrayDownMove(ARM, FixedTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.FixedTrayDown, new AccessInfo(ARM, FixedTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreChuckUpMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreChuckUpMove(ARM, Chuck);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreChuck, new AccessInfo(ARM, Chuck));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreChuckDownMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreChuckDownMove(ARM, Chuck);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreChuck, new AccessInfo(ARM, Chuck));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class FixedTrayDown : LoaderMoveStateBase
    {
        public FixedTrayDown(LoaderMove module) : base(module) { }

        public override LoaderMoveStateEnum State => LoaderMoveStateEnum.FixedTrayDown;

        public override FoupAccessModeEnum GetFoupAccessMode(int cassetteNum)
        {
            return FoupAccessModeEnum.NO_ACCESSED;
        }

        public override EventCodeEnum PickUp(IARMModule ARM, IWaferOwnable Target, LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.ACCESS)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (Module.AccessInfo.Accessor == ARM &&
                    Module.AccessInfo.Target == Target)
                {
                    var FIXED = Module.AccessInfo.Target as IFixedTrayModule;

                    var accparam = FIXED.GetAccessParam(Target.Holder.TransferObject.Type.Value, Target.Holder.TransferObject.Size.Value);

                    double value = accparam.PickupIncrement.Value;

                    retVal = Module.MovingMethods.RelMove(EnumAxisConstants.A, value, movingType);
                }
                else
                {
                    retVal = RaiseInvalidState();
                }

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.FixedTrayUp, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum RetractAll(LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll(movingType);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.Retracted, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ScanCameraStartPosMove(IScanCameraModule ScanCamera, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.ScanCameraSlot1PosMove(ScanCamera, Cassette);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.ScanCameraHome, new AccessInfo(ScanCamera, Cassette));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ScanSensorStartPosMove(IScanSensorModule ScanSensor, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.ScanSensorStartPosMove(ScanSensor, Cassette);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.ScanSensorHome, new AccessInfo(ScanSensor, Cassette));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum SlotUpMove(IARMModule ARM, ISlotModule Slot)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Slot.Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.SlotUpMove(ARM, Slot);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.SlotUp, new AccessInfo(ARM, Slot));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum SlotDownMove(IARMModule ARM, ISlotModule Slot)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Slot.Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.SlotDownMove(ARM, Slot);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.SlotDown, new AccessInfo(ARM, Slot));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreAlignUpMove(IARMModule ARM, IPreAlignModule PA, LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreAlignUpMove(ARM, PA, movingType);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreAlignUp, new AccessInfo(ARM, PA));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreAlignDownMove(IARMModule ARM, IPreAlignModule PA)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreAlignDownMove(ARM, PA);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreAlignDown, new AccessInfo(ARM, PA));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum InspectionTrayUpMove(IARMModule ARM, IInspectionTrayModule InspectionTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                {
                    var CollisionCassette = GetColisionCassette(InspectionTray);
                    if (CollisionCassette != null)
                        retVal = Module.MovingMethods.FoupCoverUp(CollisionCassette);
                }

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.InspectionTrayUpMove(ARM, InspectionTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.InspectionTrayUp, new AccessInfo(ARM, InspectionTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum InspectionTrayDownMove(IARMModule ARM, IInspectionTrayModule InspectionTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                {
                    var CollisionCassette = GetColisionCassette(InspectionTray);
                    if (CollisionCassette != null)
                        retVal = Module.MovingMethods.FoupCoverUp(CollisionCassette);
                }

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.InspectionTrayDownMove(ARM, InspectionTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.InspectionTrayDown, new AccessInfo(ARM, InspectionTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum FixedTrayUpMove(IARMModule ARM, IFixedTrayModule FixedTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FixedTrayUpMove(ARM, FixedTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.FixedTrayUp, new AccessInfo(ARM, FixedTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum FixedTrayDownMove(IARMModule ARM, IFixedTrayModule FixedTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FixedTrayDownMove(ARM, FixedTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.FixedTrayDown, new AccessInfo(ARM, FixedTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreChuckUpMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreChuckUpMove(ARM, Chuck);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreChuck, new AccessInfo(ARM, Chuck));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreChuckDownMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreChuckDownMove(ARM, Chuck);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreChuck, new AccessInfo(ARM, Chuck));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

    }

    public class PreChuck : LoaderMoveStateBase
    {
        public PreChuck(LoaderMove module) : base(module) { }

        public override LoaderMoveStateEnum State => LoaderMoveStateEnum.PreChuck;

        public override FoupAccessModeEnum GetFoupAccessMode(int cassetteNum)
        {
            return FoupAccessModeEnum.NO_ACCESSED;
        }

        public override EventCodeEnum RetractAll(LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll(movingType);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.Retracted, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ScanCameraStartPosMove(IScanCameraModule ScanCamera, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.ScanCameraSlot1PosMove(ScanCamera, Cassette);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.ScanCameraHome, new AccessInfo(ScanCamera, Cassette));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ScanSensorStartPosMove(IScanSensorModule ScanSensor, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.ScanSensorStartPosMove(ScanSensor, Cassette);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.ScanSensorHome, new AccessInfo(ScanSensor, Cassette));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum SlotUpMove(IARMModule ARM, ISlotModule Slot)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Slot.Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.SlotUpMove(ARM, Slot);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.SlotUp, new AccessInfo(ARM, Slot));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum SlotDownMove(IARMModule ARM, ISlotModule Slot)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FoupCoverDown(Slot.Cassette);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.SlotDownMove(ARM, Slot);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.SlotDown, new AccessInfo(ARM, Slot));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreAlignUpMove(IARMModule ARM, IPreAlignModule PA, LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreAlignUpMove(ARM, PA, movingType);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreAlignUp, new AccessInfo(ARM, PA));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreAlignDownMove(IARMModule ARM, IPreAlignModule PA)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreAlignDownMove(ARM, PA);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreAlignDown, new AccessInfo(ARM, PA));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum InspectionTrayUpMove(IARMModule ARM, IInspectionTrayModule InspectionTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                {
                    var CollisionCassette = GetColisionCassette(InspectionTray);
                    if (CollisionCassette != null)
                        retVal = Module.MovingMethods.FoupCoverUp(CollisionCassette);
                }

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.InspectionTrayUpMove(ARM, InspectionTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.InspectionTrayUp, new AccessInfo(ARM, InspectionTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum InspectionTrayDownMove(IARMModule ARM, IInspectionTrayModule InspectionTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                {
                    var CollisionCassette = GetColisionCassette(InspectionTray);
                    if (CollisionCassette != null)
                        retVal = Module.MovingMethods.FoupCoverUp(CollisionCassette);
                }

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.InspectionTrayDownMove(ARM, InspectionTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.InspectionTrayDown, new AccessInfo(ARM, InspectionTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum FixedTrayUpMove(IARMModule ARM, IFixedTrayModule FixedTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FixedTrayUpMove(ARM, FixedTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.FixedTrayUp, new AccessInfo(ARM, FixedTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum FixedTrayDownMove(IARMModule ARM, IFixedTrayModule FixedTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.FixedTrayDownMove(ARM, FixedTray);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.FixedTrayDown, new AccessInfo(ARM, FixedTray));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreChuckUpMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (Module.AccessInfo.Target == Chuck)
                {
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = Module.MovingMethods.RetractAll();
                }

                //if (retVal == EventCodeEnum.NONE)
                //    retVal = Module.MovingMethods.PreChuckUpMove(ARM, Chuck);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreChuck, new AccessInfo(ARM, Chuck));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum PreChuckDownMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (Module.AccessInfo.Target == Chuck)
                {
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = Module.MovingMethods.RetractAll();
                }

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.PreChuckDownMove(ARM, Chuck);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreChuck, new AccessInfo(ARM, Chuck));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ChuckUpMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (Module.AccessInfo.Target == Chuck)
                {
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = Module.MovingMethods.RetractAll();
                }

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.ChuckUpMove(ARM, Chuck);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.ChuckUp, new AccessInfo(ARM, Chuck));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ChuckDownMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (Module.AccessInfo.Target == Chuck)
                {
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = Module.MovingMethods.RetractAll();
                }

                if (retVal == EventCodeEnum.NONE)
                    retVal = Module.MovingMethods.ChuckDownMove(ARM, Chuck);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.ChuckDown, new AccessInfo(ARM, Chuck));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum SafePosW()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll();
                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = Module.MovingMethods.SafePosW();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class ChuckUp : LoaderMoveStateBase
    {
        public ChuckUp(LoaderMove module) : base(module) { }

        public override LoaderMoveStateEnum State => LoaderMoveStateEnum.ChuckUp;

        public override FoupAccessModeEnum GetFoupAccessMode(int cassetteNum)
        {
            return FoupAccessModeEnum.NO_ACCESSED;
        }

        public override EventCodeEnum PlaceDown(IARMModule ARM, IWaferOwnable Target, LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.ACCESS)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (Module.AccessInfo.Accessor == ARM &&
                    Module.AccessInfo.Target == Target)
                {
                    var CHUCK = Module.AccessInfo.Target as IChuckModule;

                    var accparam = CHUCK.GetAccessParam(ARM.Holder.TransferObject.Type.Value, ARM.Holder.TransferObject.Size.Value);

                    double value = accparam.PickupIncrement.Value * -1.0;

                    retVal = Module.MovingMethods.RelMove(EnumAxisConstants.A, value, movingType);
                }
                else
                {
                    retVal = RaiseInvalidState();
                }

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.ChuckDown, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum RetractAll(LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll(movingType);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreChuck, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

    }

    public class ChuckDown : LoaderMoveStateBase
    {
        public ChuckDown(LoaderMove module) : base(module) { }

        public override LoaderMoveStateEnum State => LoaderMoveStateEnum.ChuckDown;

        public override FoupAccessModeEnum GetFoupAccessMode(int cassetteNum)
        {
            return FoupAccessModeEnum.NO_ACCESSED;
        }

        public override EventCodeEnum PickUp(IARMModule ARM, IWaferOwnable Target, LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.ACCESS)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (Module.AccessInfo.Accessor == ARM &&
                    Module.AccessInfo.Target == Target)
                {
                    var CHUCK = Module.AccessInfo.Target as IChuckModule;

                    var accparam = CHUCK.GetAccessParam(Target.Holder.TransferObject.Type.Value, Target.Holder.TransferObject.Size.Value);

                    double value = accparam.PickupIncrement.Value;

                    retVal = Module.MovingMethods.RelMove(EnumAxisConstants.A, value, movingType);
                }
                else
                {
                    retVal = RaiseInvalidState();
                }

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.ChuckUp, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum RetractAll(LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll(movingType);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.PreChuck, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

    }


    public class LoaderSetup : LoaderMoveStateBase
    {
        public LoaderSetup(LoaderMove module) : base(module) { }

        public override LoaderMoveStateEnum State => LoaderMoveStateEnum.LoaderSetup;

        public override FoupAccessModeEnum GetFoupAccessMode(int cassetteNum)
        {
            return FoupAccessModeEnum.NO_ACCESSED;
        }

        public override EventCodeEnum SetupToChuckMove(IARMModule ARM, IChuckModule Chuck, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool uaxisskip, int slotnum, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();
                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = Module.MovingMethods.SetupToChuckMoveMethod(ARM, Chuck, subtype, subsize, uaxisskip, slotnum, index);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        // 1acces
        public override EventCodeEnum SetupToCstMove(IARMModule ARM, ISlotModule Slot, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool uaxisskip, int slotnum, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();
                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = Module.MovingMethods.SetupToCstMoveMethod(ARM, Slot, subtype, subsize, uaxisskip, slotnum, index);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override EventCodeEnum SetupToCstSlot(IScanSensorModule ScanSensor, ICassetteModule Cassette, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool uaxisskip, int slotnum, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();
                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = Module.MovingMethods.SetupToCstSlot1Method(ScanSensor, Cassette, subtype, subsize, uaxisskip, slotnum, 1);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum SetupToFixedTrayMove(IARMModule ARM, IFixedTrayModule FixedTray, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool uaxisskip, int slotnum, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();
                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = Module.MovingMethods.SetupToFixedTrayMoveMethod(ARM, FixedTray, subtype, subsize, uaxisskip, slotnum, index);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override EventCodeEnum SetupToInspectionTrayMove(IARMModule ARM, IInspectionTrayModule InspectionTray, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool uaxisskip, int slotnum, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();
                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = Module.MovingMethods.SetupToInspectionTrayMoveMethod(ARM, InspectionTray, subtype, subsize, uaxisskip, slotnum, index);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum SetupToOCRMove(IARMModule ARM, IPreAlignModule PA, IOCRReadable OCR, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool uaxisskip, int slotnum, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();
                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = Module.MovingMethods.SetupToOCRMoveMethod(ARM, PA, OCR, subtype, subsize, uaxisskip, slotnum, index);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override EventCodeEnum SetupToPAMove(IARMModule ARM, IPreAlignModule PA, LoaderMovingTypeEnum movingType, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool uaxisskip, int slotnum, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = Module.MovingMethods.RetractAll();
                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = Module.MovingMethods.SetupToPAMoveMethod(ARM, PA, LoaderMovingTypeEnum.NORMAL, subtype, subsize, uaxisskip, slotnum, index);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum RetractAll(LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Module.MovingMethods.RetractAll(movingType);

                StateTransitionIfNoErr(retVal, LoaderMoveStateEnum.Retracted, Module.AccessInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

    }

}
