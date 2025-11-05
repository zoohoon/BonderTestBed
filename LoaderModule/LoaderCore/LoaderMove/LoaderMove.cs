using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using LoaderBase;
using ProberInterfaces;
using ProberErrorCode;
using ProberInterfaces.Utility;

namespace LoaderCore
{
    using LoaderMoveStates;
    using LogModule;

    public class LoaderMove : ILoaderMove
    {
        public LoaderMove()
        {
            try
            {
                GenerateStates();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public ILoaderMovingMethods MovingMethods { get; set; }

        public InitPriorityEnum InitPriority => InitPriorityEnum.LEVEL1;

        public IContainer Container { get; set; }

        public LoaderMoveStateEnum State => CurrState.State;

        public AccessInfo AccessInfo { get; set; }

        #region State Management
        private Dictionary<LoaderMoveStateEnum, LoaderMoveStateBase> _StateDic;

        private LoaderMoveStateBase CurrState { get; set; }

        private void GenerateStates()
        {
            var assem = Assembly.GetCallingAssembly();

            _StateDic = new Dictionary<LoaderMoveStateEnum, LoaderMoveStateBase>();

            var stateInstances = ReflectionEx.GetAssignableInstances<LoaderMoveStateBase>(assem, this);
            foreach (var inst in stateInstances)
            {
                _StateDic.Add(inst.State, inst);
            }

            CurrState = _StateDic[LoaderMoveStateEnum.ERROR];
        }

        public void StateTransition(LoaderMoveStateEnum state, AccessInfo accessInfo)
        {
            try
            {
                bool equalsState = CurrState != null && CurrState.State == state;
                bool equalsAccessInfo = AccessInfo != null && AccessInfo.IsSamePosition(accessInfo);

                if (equalsState && equalsAccessInfo)
                    return;

                AccessInfo = accessInfo;
                CurrState = _StateDic[state];

                string accessorLabel = AccessInfo == null || AccessInfo.Accessor == null ? "" : AccessInfo.Accessor.ID.Label;
                string targetLabel = AccessInfo == null || AccessInfo.Target == null ? "" : AccessInfo.Target.ID.Label;

                LoggerManager.Debug($"LOADERMOVE.StateTransition() : State={State}, Accessor={accessorLabel}, Target={targetLabel}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        //private readonly LockKey MovingObject = new LockKey("Loader Move");
        private static object MovingObject = new object();


        public ILoaderModule Loader => Container.Resolve<ILoaderModule>();

        public EventCodeEnum InitModule(IContainer container)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.Container = container;

                var extensionDef = Loader.SystemParameter.UExtension;

                //EnumLoaderMovingMethodType.OPUSV_MINI Type이 기본임, 신규 장비 개발시 MovingMethod 추가 및 구분 로직 필요
                this.MovingMethods = new LoaderF20MovingMethds(); 

                this.MovingMethods.Init(container, extensionDef);

                StateTransition(LoaderMoveStateEnum.ERROR, null);

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void DeInitModule()
        {

        }

        public FoupAccessModeEnum GetFoupAccessMode(int foupNumber)
        {
            FoupAccessModeEnum foupAccessMode = FoupAccessModeEnum.UNKNOWN;

            lock (MovingObject)
            {

                try
                {
                    foupAccessMode = CurrState.GetFoupAccessMode(foupNumber);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return foupAccessMode;
        }

        public EventCodeEnum JogAbsMove(EnumAxisConstants axis, double val)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.JogAbsMove(axis, val);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }

            }
            return retVal;
        }

        public EventCodeEnum JogRelMove(EnumAxisConstants axis, double val)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {

                try
                {
                    retVal = CurrState.JogRelMove(axis, val);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }

            }
            return retVal;
        }

        public EventCodeEnum ScanCameraStartPosMove(IScanCameraModule ScanCamera, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {

                try
                {
                    retVal = CurrState.ScanCameraStartPosMove(ScanCamera, Cassette);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }

        public EventCodeEnum ScanCameraRelMove(EnumAxisConstants axis, double val)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.ScanCameraRelMove(axis, val);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }

        public EventCodeEnum ChuckDownMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.ChuckDownMove(ARM, Chuck);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }

        public EventCodeEnum ChuckUpMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.ChuckUpMove(ARM, Chuck);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }

        public EventCodeEnum RetractAll(LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.RetractAll(movingType);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }


        public EventCodeEnum SafePosW()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.SafePosW();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }

        public EventCodeEnum FixedTrayDownMove(IARMModule ARM, IFixedTrayModule FixedTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.FixedTrayDownMove(ARM, FixedTray);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }

        public EventCodeEnum FixedTrayUpMove(IARMModule ARM, IFixedTrayModule FixedTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.FixedTrayUpMove(ARM, FixedTray);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }

        public EventCodeEnum InspectionTrayDownMove(IARMModule ARM, IInspectionTrayModule InspectionTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.InspectionTrayDownMove(ARM, InspectionTray);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }

        public EventCodeEnum InspectionTrayUpMove(IARMModule ARM, IInspectionTrayModule InspectionTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.InspectionTrayUpMove(ARM, InspectionTray);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }

        public EventCodeEnum MotionInit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.MotionInit();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }

        public EventCodeEnum OCRMoveFromPreAlignUp(IARMModule UseARM, IOCRReadable OCR, IPreAlignModule PA)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.OCRMoveFromPreAlignUp(UseARM, OCR, PA);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }

        public EventCodeEnum PreAlignUpMoveFromOCR(IARMModule ARM, IPreAlignModule PA, IOCRReadable OCR)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.PreAlignUpMoveFromOCR(ARM, PA, OCR);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }
        public EventCodeEnum PreAlignUpMove(IARMModule ARM, IPreAlignModule PA, LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.PreAlignUpMove(ARM, PA, movingType);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }

        public EventCodeEnum PreAlignDownMove(IARMModule ARM, IPreAlignModule PA)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.PreAlignDownMove(ARM, PA);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }

        public EventCodeEnum FindNotchMove(IPreAlignModule PA, EnumMotorDedicatedIn input)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.PAFindNotchMove(PA, input);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }

        public EventCodeEnum PreAlignRelMove(IARMModule ARM, double val)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.PreAlignRelMove(ARM, val);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }
        public EventCodeEnum PreAlignRelMove(IPreAlignModule PA, double val)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.PreAlignRelMove(PA, val);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }

        public EventCodeEnum PreAlignZeroMove(IPreAlignModule PA)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.PreAlignZeroMove(PA);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }

        public EventCodeEnum PickUp(IARMModule ARM, IWaferOwnable Target, 
            LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.ACCESS)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.PickUp(ARM, Target, movingType);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }

        public EventCodeEnum PlaceDown(IARMModule ARM, IWaferOwnable Target,
            LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.ACCESS)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.PlaceDown(ARM, Target, movingType);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }

        public EventCodeEnum PreChuckUpMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.PreChuckUpMove(ARM, Chuck);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }

        public EventCodeEnum PreChuckDownMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.PreChuckDownMove(ARM, Chuck);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }

        public EventCodeEnum ScanSensorStartPosMove(IScanSensorModule SensorScan, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.ScanSensorStartPosMove(SensorScan, Cassette);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }

        public EventCodeEnum ExtendScanSensor(IScanSensorModule SensorScan)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.ExtendScanSensor(SensorScan);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }
        public EventCodeEnum ScanSensorUpMove(IScanSensorModule SensorScan, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.ScanSensorUpMove(SensorScan, Cassette);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }

        public EventCodeEnum ScanSensorDownMove(IScanSensorModule SensorScan, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.ScanSensorDownMove(SensorScan, Cassette);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }

        public EventCodeEnum SlotDownMove(IARMModule ARM, ISlotModule Slot)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.SlotDownMove(ARM, Slot);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }

        public EventCodeEnum SlotUpMove(IARMModule ARM, ISlotModule Slot)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.SlotUpMove(ARM, Slot);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }

        #region SetupMove
        public EventCodeEnum SetupToCstSlot1(IScanSensorModule ScanSensor, ICassetteModule Cassette, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool uaxisskip, int slotnum, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.SetupToCstSlot(ScanSensor, Cassette, subtype, subsize, uaxisskip, slotnum, index);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }

        public EventCodeEnum SetupToPAMove(IARMModule ARM, IPreAlignModule PA, LoaderMovingTypeEnum movingType, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool uaxisskip, int slotnum, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.SetupToPAMove(ARM, PA, movingType, subtype, subsize, uaxisskip, slotnum, index);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }

        public EventCodeEnum SetupToOCRMove(IARMModule ARM, IPreAlignModule PA, IOCRReadable OCR, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool uaxisskip, int slotnum, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.SetupToOCRMove(ARM, PA, OCR, subtype, subsize, uaxisskip, slotnum, index);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }

        public EventCodeEnum SetupToChuckMove(IARMModule ARM, IChuckModule Chuck, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool uaxisskip, int slotnum, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.SetupToChuckMove(ARM, Chuck, subtype, subsize, uaxisskip, slotnum, index);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }

        public EventCodeEnum SetupToFixedTrayMove(IARMModule ARM, IFixedTrayModule FixedTray, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool uaxisskip, int slotnum, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.SetupToFixedTrayMove(ARM, FixedTray, subtype, subsize, uaxisskip, slotnum, index);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }

        public EventCodeEnum SetupToInspectionTrayMove(IARMModule ARM, IInspectionTrayModule InspectionTray, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool uaxisskip, int slotnum, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.SetupToInspectionTrayMove(ARM, InspectionTray, subtype, subsize, uaxisskip, slotnum, index);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }

        public EventCodeEnum SetupToCstMove(IARMModule ARM, ISlotModule Slot, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool uaxisskip, int slotnum, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            lock (MovingObject)
            {
                try
                {
                    retVal = CurrState.SetupToCstMove(ARM, Slot, subtype, subsize, uaxisskip, slotnum, index);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return retVal;
        }
        #endregion
    }//end of class

    public class AccessInfo
    {
        public IAttachedModule Accessor { get; set; }

        public IAttachedModule Target { get; set; }

        public AccessInfo(IAttachedModule accessor, IAttachedModule target)
        {
            try
            {
                this.Accessor = accessor;
                this.Target = target;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool IsSamePosition(AccessInfo other)
        {
            bool retVal = false;

            try
            {
                if (other == null)
                    return false;
                retVal = Accessor == other.Accessor && Target == other.Target;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

}
