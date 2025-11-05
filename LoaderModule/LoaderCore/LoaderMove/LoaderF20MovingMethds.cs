using System;
using System.Linq;

using Autofac;
using LoaderBase;
using ProberErrorCode;
using LoaderParameters;
using ProberInterfaces;
using ProberInterfaces.Foup;
using LogModule;
using LoaderBase.AttachModules.ModuleInterfaces;

namespace LoaderCore
{
    public class LoaderF20MovingMethds : ILoaderMovingMethods
    {
        public Autofac.IContainer Container { get; set; }

        public IUExtensionObject UExtension { get; set; }

        public ILoaderModule Loader => Container.Resolve<ILoaderModule>();


        public EventCodeEnum Init(Autofac.IContainer container, UExtensionBase extension)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.Container = container;

                var extensionDef = Loader.SystemParameter.UExtension;
                if (extensionDef.UExtensionType.Value == UExtensionTypeEnum.NONE)
                {
                    UExtension = new UExtensionNoneObject();
                }
                else if (extensionDef.UExtensionType.Value == UExtensionTypeEnum.MOTOR)
                {
                    UExtension = new UExtensionMotorObject();
                }
                else if (extensionDef.UExtensionType.Value == UExtensionTypeEnum.CYLINDER)
                {
                    UExtension = new UExtensionCylinderObject();
                }

                retVal = UExtension.Init(container, extensionDef);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }


        public EventCodeEnum RetractAll(LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var scaxis = Loader.MotionManager.GetAxis(EnumAxisConstants.SC);
                retVal = RetractScanSensorAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = RetractARMs(movingType);

                if (retVal == EventCodeEnum.NONE)
                    retVal = UExtension.Retract(movingType);

                if (retVal == EventCodeEnum.NONE)
                    retVal = ScanAxisAbsMove(scaxis.Param.ClearedPosition.Value, LoaderMovingTypeEnum.NORMAL);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        private EventCodeEnum CheckWaferOutSensor()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.IOManager.MonitorForIO(Loader.IOManager.IOMappings.Inputs.DIWAFERSENSOR, true);
                if (retVal != EventCodeEnum.NONE)
                {
                    retVal = EventCodeEnum.MONITORING_LOADER_WAFEROUTSENSOR_ERROR;
                    if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                    {
                        retVal = EventCodeEnum.NONE;
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
        public EventCodeEnum MotionInit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //=> ARM status
                var ARMs = Loader.ModuleManager.FindModules<IARMModule>();
                int unknownARMCount = ARMs.Count(item =>
                item.Holder.Status == EnumSubsStatus.UNDEFINED ||
                item.Holder.Status == EnumSubsStatus.UNKNOWN);

                if (unknownARMCount > 0)
                    retVal = EventCodeEnum.UNDEFINED;
                else
                    retVal = EventCodeEnum.NONE;

                if (retVal == EventCodeEnum.NONE)
                    retVal = RetractScanSensorAll();

                if (retVal == EventCodeEnum.NONE)
                    retVal = UExtension.Homming();

                if (retVal == EventCodeEnum.NONE)
                    retVal = CheckWaferOutSensor();

                if (retVal == EventCodeEnum.NONE && SystemManager.SysteMode == SystemModeEnum.Single)
                    retVal = Loader.MotionManager.InitModule();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum RetractScanSensorAll()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                var modules = Loader.ModuleManager.FindModules<IScanSensorModule>();

                if (modules.Count() > 0)
                {
                    retVal = EventCodeEnum.UNDEFINED;
                    foreach (var module in modules)
                    {
                        var scanparam = module.Definition.ScanParams.FirstOrDefault();

                        if (scanparam.ScanAxis.Value == EnumAxisConstants.A)
                        {
                            retVal = module.Retract();
                        }
                        else if (scanparam.ScanAxis.Value == EnumAxisConstants.SC)
                        {
                            retVal = EventCodeEnum.NONE;
                        }
                        else
                        {
                            retVal = EventCodeEnum.PARAM_ERROR;
                        }

                        if (retVal != EventCodeEnum.NONE)
                            break;
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

        public EventCodeEnum RetractARMs(LoaderMovingTypeEnum movingType)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                var ARMs = Loader.ModuleManager.FindModules<IARMModule>();

                foreach (var ARM in ARMs)
                {
                    var UaxisObj = Loader.MotionManager.GetAxis(ARM.Definition.AxisType.Value);

                    double pos = UaxisObj.Param.ClearedPosition.Value;
                    double vel = GetSpeed(UaxisObj, movingType);
                    double acc = UaxisObj.Param.Acceleration.Value;

                    retVal = Loader.MotionManager.AbsMove(UaxisObj.AxisType.Value, pos, vel, acc);
                    if (retVal != EventCodeEnum.NONE)
                    {
                        break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum ScanSensorStartPosMove(IScanSensorModule ScanSensor, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.IOManager.MonitorForIO(Loader.IOManager.IOMappings.Inputs.DIWAFERSENSOR, true);

                if (retVal == EventCodeEnum.NONE)
                {
                    //=> move to sensor scan start position
                    var slot1Param = Cassette.GetSlot1AccessParam(Cassette.Device.AllocateDeviceInfo.Type.Value, Cassette.Device.AllocateDeviceInfo.Size.Value);
                    var scanParam = ScanSensor.GetScanParam(Cassette);

                    if(slot1Param != null && scanParam != null)
                    {
                        double Apos = slot1Param.Position.A.Value;
                        double SCpos =
                            slot1Param.Position.SC.Value +
                            scanParam.SensorOffset.Value +
                            Math.Abs(scanParam.DownOffset.Value) * -1.0;

                        double Wpos = slot1Param.Position.W.Value;

                        UExtensionMoveParam Epos = slot1Param.Position.E.Value;
                        RetractARMs(LoaderMovingTypeEnum.NORMAL);
                        // Move
                        //retVal = LoaderAbsMove(Apos, Wpos, Epos, LoaderMovingTypeEnum.NORMAL);

                        retVal = ScanAxisAbsMove(SCpos, LoaderMovingTypeEnum.NORMAL);
                    }
                    else
                    {
                        // TODO : 
                        retVal = EventCodeEnum.UNDEFINED;
                    }
                }
                else
                {
                    // Error

                    retVal = EventCodeEnum.FOUP_SCAN_WAFEROUT;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


            return retVal;
        }

        public EventCodeEnum ScanSensorUpMove(IScanSensorModule ScanSensor, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //            retVal = ScanSensor.Out();

                var scanParam = ScanSensor.GetScanParam(Cassette);

                var slot1Param = Cassette.GetSlot1AccessParam(Cassette.Device.AllocateDeviceInfo.Type.Value, Cassette.Device.AllocateDeviceInfo.Size.Value);

                var slotCount = Cassette.Device.SlotModules.Count();

                double sensor1slotPos =
                    slot1Param.Position.SC.Value +
                    scanParam.SensorOffset.Value;

                double fullSlotDist = Cassette.Device.SlotSize.Value * (slotCount - 1);

                double endPos = sensor1slotPos + fullSlotDist + Math.Abs(scanParam.UpOffset.Value);

                retVal = ScanAxisAbsMove(endPos, LoaderMovingTypeEnum.NORMAL);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum ScanSensorOut(IScanSensorModule ScanSensor)
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

        public EventCodeEnum ScanSensorDownMove(IScanSensorModule ScanSensor, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //retVal = ScanSensor.Out();

                var slot1Param = Cassette.GetSlot1AccessParam(Cassette.Device.AllocateDeviceInfo.Type.Value, Cassette.Device.AllocateDeviceInfo.Size.Value);

                var scanParam = ScanSensor.GetScanParam(Cassette);

                double startPos =
                    slot1Param.Position.SC.Value +
                    scanParam.SensorOffset.Value +
                    Math.Abs(scanParam.DownOffset.Value) * -1.0;

                retVal = ScanAxisAbsMove(startPos, LoaderMovingTypeEnum.NORMAL);
                if (retVal == EventCodeEnum.NONE)
                {
                    //var axis = Loader.MotionManager.GetAxis(EnumAxisConstants.SC);
                    //retVal = ScanAxisAbsMove(axis.Param.ClearedPosition.Value, LoaderMovingTypeEnum.NORMAL);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum ScanCameraSlot1PosMove(IScanCameraModule ScanCamera, ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                var scanParam = ScanCamera.GetScanCameraParam(Cassette);

                double Apos = scanParam.Slot1Position.A.Value;
                double Wpos = scanParam.Slot1Position.W.Value;
                UExtensionMoveParam Epos = scanParam.Slot1Position.E.Value;

                retVal = LoaderAbsMove(Apos, Wpos, Epos, LoaderMovingTypeEnum.NORMAL);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        /// <summary>
        /// 해당 슬롯 높이에서 1차 U축을 진입 그리고 다시한번 2차U축 이동 수행
        /// </summary>
        /// <param name="ARM"></param>
        /// <param name="Slot"></param>
        /// <returns></returns>
        public EventCodeEnum SlotUpMove(IARMModule ARM, ISlotModule Slot)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (ARM.Holder.Status == EnumSubsStatus.EXIST &&
                    Slot.Holder.Status == EnumSubsStatus.NOT_EXIST)
                {
                    var SubObj = ARM.Holder;

                    //Generate
                    var ARMAxisType = ARM.Definition.AxisType;
                    var Cassette = Slot.Cassette;
                    var localSlotNumber = Slot.LocalSlotNumber;
                    var slot1Param = Slot.Cassette.GetSlot1AccessParam(SubObj.TransferObject.Type.Value, SubObj.TransferObject.Size.Value);

                    double Apos =
                        slot1Param.Position.A.Value +
                        Cassette.Device.SlotSize.Value * (localSlotNumber - 1) +
                        ARM.Definition.UpOffset.Value * -1.0 +
                        slot1Param.PickupIncrement.Value;

                    double PrevUpos = slot1Param.Position.U.Value - Math.Abs(slot1Param.UStopPosOffset.Value);

                    double Wpos = slot1Param.Position.W.Value;

                    UExtensionMoveParam Epos = slot1Param.Position.E.Value;

                    //=> move to slot (step1)
                    retVal = LoaderAbsMove(Apos, PrevUpos, Wpos, Epos, ARM, LoaderMovingTypeEnum.NORMAL);

                    //=> Vacuum off if ARM has substrate.
                    if (retVal == EventCodeEnum.NONE)
                    {
                        if (ARM.Holder.Status == EnumSubsStatus.EXIST)
                        {
                            retVal = ARM.WriteVacuum(false);
                            retVal = ARM.WaitForVacuum(false);
                        }
                    }

                    //=> move to Upos (step1)
                    if (retVal == EventCodeEnum.NONE)
                    {
                        double Upos = slot1Param.Position.U.Value - ARM.Definition.EndOffset.Value;
                        retVal = Loader.MotionManager.AbsMove(ARMAxisType.Value, Upos,
                            GetSpeed(ARMAxisType.Value, LoaderMovingTypeEnum.ACCESS),
                            GetAcc(ARMAxisType.Value, LoaderMovingTypeEnum.ACCESS));
                    }
                }
                else
                {
                    retVal = EventCodeEnum.LOADER_STATE_INVALID;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum SlotDownMove(IARMModule ARM, ISlotModule Slot)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (ARM.Holder.Status == EnumSubsStatus.NOT_EXIST &&
                    Slot.Holder.Status == EnumSubsStatus.EXIST)
                {
                    var SubObj = Slot.Holder;

                    var Cassette = Slot.Cassette;
                    var localSlotNumber = Slot.LocalSlotNumber;
                    var accparam = Slot.Cassette.GetSlot1AccessParam(SubObj.TransferObject.Type.Value, SubObj.TransferObject.Size.Value);

                    double Apos = accparam.Position.A.Value +
                        Cassette.Device.SlotSize.Value * (localSlotNumber - 1) + ARM.Definition.UpOffset.Value * -1.0;

                    double Upos =
                        accparam.Position.U.Value +
                        ARM.Definition.EndOffset.Value * -1.0;

                    double Wpos = accparam.Position.W.Value;

                    UExtensionMoveParam Epos = accparam.Position.E.Value;


                    retVal = LoaderAbsMove(Apos, Upos, Wpos, Epos, ARM, LoaderMovingTypeEnum.NORMAL);
                }
                else
                {
                    retVal = EventCodeEnum.LOADER_STATE_INVALID;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum PreAlignUpMove(IARMModule ARM, IPreAlignModule PA, LoaderMovingTypeEnum movingType)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (ARM.Holder.Status == EnumSubsStatus.EXIST &&
                    PA.Holder.Status == EnumSubsStatus.NOT_EXIST)
                {
                    var SubObj = ARM.Holder;

                    //Generate
                    var ARMAxisType = ARM.Definition.AxisType;
                    var accparam = PA.GetAccessParam(SubObj.TransferObject.Type.Value, SubObj.TransferObject.Size.Value);

                    if (accparam != null)
                    {
                        double Apos = accparam.Position.A.Value +
                                      ARM.Definition.UpOffset.Value * -1.0 +
                                      accparam.PickupIncrement.Value;

                        double Upos = accparam.Position.U.Value + 
                                      ARM.Definition.EndOffset.Value * -1.0;

                        double Wpos = accparam.Position.W.Value;

                        UExtensionMoveParam Epos = accparam.Position.E.Value;

                        var WaxisObj = Loader.MotionManager.GetAxis(EnumAxisConstants.W);

                        if(WaxisObj != null)
                        {
                            var vel = GetSpeed(WaxisObj, movingType);
                            var acc = WaxisObj.Param.Acceleration.Value;

                            retVal = Loader.MotionManager.AbsMoveAsync(EnumAxisConstants.W, WaxisObj.Param.ClearedPosition.Value, vel, acc);

                            retVal = Loader.MotionManager.WaitForAxisMotionDone(EnumAxisConstants.W);

                            if (retVal == EventCodeEnum.NONE)
                            {
                                retVal = LoaderAbsMove(Apos, Upos, Wpos, Epos, ARM, movingType);
                            }
                        }
                        else
                        {
                            LoggerManager.Error($"[LoaderF20MovingMethds], PreAlignUpMove() : WaxisObj is null.");

                            retVal = EventCodeEnum.LOADER_STATE_INVALID;
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"[LoaderF20MovingMethds], PreAlignUpMove() : accparam is null.");

                        retVal = EventCodeEnum.LOADER_STATE_INVALID;
                    }
                }
                else
                {
                    retVal = EventCodeEnum.LOADER_STATE_INVALID;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum PreAlignDownMove(IARMModule ARM, IPreAlignModule PA)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                if (ARM.Holder.Status == EnumSubsStatus.NOT_EXIST &&
                    PA.Holder.Status == EnumSubsStatus.EXIST)
                {
                    var SubObj = PA.Holder;

                    //Generate
                    var ARMAxisType = ARM.Definition.AxisType;
                    var accparam = PA.GetAccessParam(SubObj.TransferObject.Type.Value, SubObj.TransferObject.Size.Value);

                    double Apos =
                        accparam.Position.A.Value +
                        ARM.Definition.UpOffset.Value * -1.0;

                    double Upos =
                        accparam.Position.U.Value +
                        ARM.Definition.EndOffset.Value * -1.0;

                    double Wpos = accparam.Position.W.Value;

                    UExtensionMoveParam Epos = accparam.Position.E.Value;

                    var WaxisObj = Loader.MotionManager.GetAxis(EnumAxisConstants.W);
                    var vel = GetSpeed(WaxisObj, LoaderMovingTypeEnum.NORMAL);
                    var acc = WaxisObj.Param.Acceleration.Value;

                    retVal = Loader.MotionManager.AbsMoveAsync(EnumAxisConstants.W, WaxisObj.Param.ClearedPosition.Value, vel, acc);
                    retVal = Loader.MotionManager.WaitForAxisMotionDone(EnumAxisConstants.W);
                    if (retVal == EventCodeEnum.NONE)
                    {
                        retVal = LoaderAbsMove(Apos, Upos, Wpos, Epos, ARM, LoaderMovingTypeEnum.NORMAL);
                    }
                }
                else
                {
                    retVal = EventCodeEnum.LOADER_STATE_INVALID;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum InspectionTrayUpMove(IARMModule ARM, IInspectionTrayModule InspectionTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (ARM.Holder.Status == EnumSubsStatus.EXIST &&
                    InspectionTray.Holder.Status == EnumSubsStatus.NOT_EXIST)
                {
                    var SubObj = ARM.Holder;

                    //Generate
                    var ARMAxisType = ARM.Definition.AxisType;
                    var accparam = InspectionTray.GetAccessParam(SubObj.TransferObject.Type.Value, SubObj.TransferObject.Size.Value);

                    double Apos =
                        accparam.Position.A.Value +
                        ARM.Definition.UpOffset.Value * -1.0 +
                        accparam.PickupIncrement.Value;

                    double Upos =
                        accparam.Position.U.Value +
                        ARM.Definition.EndOffset.Value * -1.0;

                    double Wpos = accparam.Position.W.Value;

                    UExtensionMoveParam Epos = accparam.Position.E.Value;

                    Func<bool> stopFunc = () =>
                    {
                        bool isOpend;
                        InspectionTray.ReadOpened(out isOpend);
                        return isOpend;
                    };
                    retVal = LoaderAbsMove(Apos, Upos, Wpos, Epos, ARM, LoaderMovingTypeEnum.NORMAL, stopFunc, false);


                }
                else
                {
                    retVal = EventCodeEnum.LOADER_STATE_INVALID;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum InspectionTrayDownMove(IARMModule ARM, IInspectionTrayModule InspectionTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (ARM.Holder.Status == EnumSubsStatus.NOT_EXIST &&
                    InspectionTray.Holder.Status == EnumSubsStatus.EXIST)
                {
                    var SubObj = InspectionTray.Holder;

                    //Generate
                    var ARMAxisType = ARM.Definition.AxisType;
                    var accparam = InspectionTray.GetAccessParam(SubObj.TransferObject.Type.Value, SubObj.TransferObject.Size.Value);

                    double Apos =
                        accparam.Position.A.Value +
                        ARM.Definition.UpOffset.Value * -1.0;

                    double Upos =
                        accparam.Position.U.Value +
                        ARM.Definition.EndOffset.Value * -1.0;

                    double Wpos = accparam.Position.W.Value;

                    UExtensionMoveParam Epos = accparam.Position.E.Value;

                    Func<bool> stopFunc = () =>
                    {
                        bool isOpend;
                        InspectionTray.ReadOpened(out isOpend);
                        return isOpend;
                    };
                    retVal = LoaderAbsMove(Apos, Upos, Wpos, Epos, ARM, LoaderMovingTypeEnum.NORMAL, stopFunc, false);
                }
                else
                {
                    retVal = EventCodeEnum.LOADER_STATE_INVALID;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum FixedTrayUpMove(IARMModule ARM, IFixedTrayModule FixedTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (ARM.Holder.Status == EnumSubsStatus.EXIST &&
                    FixedTray.Holder.Status == EnumSubsStatus.NOT_EXIST)
                {
                    var SubObj = ARM.Holder;

                    //Generate
                    var ARMAxisType = ARM.Definition.AxisType;
                    var accparam = FixedTray.GetAccessParam(SubObj.TransferObject.Type.Value, SubObj.TransferObject.Size.Value);

                    double Apos =
                        accparam.Position.A.Value +
                        ARM.Definition.UpOffset.Value * -1.0 +
                        accparam.PickupIncrement.Value;

                    double Upos =
                        accparam.Position.U.Value +
                        ARM.Definition.EndOffset.Value * -1.0;

                    double Wpos = accparam.Position.W.Value;

                    UExtensionMoveParam Epos = accparam.Position.E.Value;

                    retVal = LoaderAbsMove(Apos, Upos, Wpos, Epos, ARM, LoaderMovingTypeEnum.NORMAL);
                }
                else
                {
                    retVal = EventCodeEnum.LOADER_STATE_INVALID;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum FixedTrayDownMove(IARMModule ARM, IFixedTrayModule FixedTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (ARM.Holder.Status == EnumSubsStatus.NOT_EXIST &&
                    FixedTray.Holder.Status == EnumSubsStatus.EXIST)
                {
                    var SubObj = FixedTray.Holder;

                    //Generate
                    var ARMAxisType = ARM.Definition.AxisType;
                    var accparam = FixedTray.GetAccessParam(SubObj.TransferObject.Type.Value, SubObj.TransferObject.Size.Value);

                    double Apos =
                        accparam.Position.A.Value +
                        ARM.Definition.UpOffset.Value * -1.0;

                    double Upos =
                        accparam.Position.U.Value +
                        ARM.Definition.EndOffset.Value * -1.0;

                    double Wpos = accparam.Position.W.Value;

                    UExtensionMoveParam Epos = accparam.Position.E.Value;

                    retVal = LoaderAbsMove(Apos, Upos, Wpos, Epos, ARM, LoaderMovingTypeEnum.NORMAL);
                }
                else
                {
                    retVal = EventCodeEnum.LOADER_STATE_INVALID;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum ChuckUpMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (ARM.Holder.Status == EnumSubsStatus.EXIST &&
                    Chuck.Holder.Status == EnumSubsStatus.NOT_EXIST)
                {
                    var SubObj = ARM.Holder;

                    //Generate
                    var ARMAxisType = ARM.Definition.AxisType;
                    var accparam = Chuck.GetAccessParam(SubObj.TransferObject.Type.Value, SubObj.TransferObject.Size.Value);

                    double Apos =
                        accparam.Position.A.Value +
                        accparam.PickupIncrement.Value;
                    //Loader.SetArmUpOffset(ARM.Definition.UpOffset.Value);
                    double Upos =
                        accparam.Position.U.Value +
                        ARM.Definition.EndOffset.Value * -1.0;

                    double Wpos = accparam.Position.W.Value;

                    UExtensionMoveParam Epos = accparam.Position.E.Value;

                    retVal = LoaderAbsMove(Apos, Upos, Wpos, Epos, ARM, LoaderMovingTypeEnum.NORMAL);
                }
                else
                {
                    retVal = EventCodeEnum.LOADER_STATE_INVALID;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum SafePosW()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                var WaxisObj = Loader.MotionManager.GetAxis(EnumAxisConstants.W);
                retVal = Loader.MotionManager.AbsMove(EnumAxisConstants.W, WaxisObj.Param.ClearedPosition.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum ChuckDownMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (ARM.Holder.Status == EnumSubsStatus.NOT_EXIST &&
                   Chuck.Holder.Status == EnumSubsStatus.EXIST)
                {
                    var SubObj = Chuck.Holder;

                    //Generate
                    var ARMAxisType = ARM.Definition.AxisType;
                    var accparam = Chuck.GetAccessParam(SubObj.TransferObject.Type.Value, SubObj.TransferObject.Size.Value);

                    double Apos =
                        accparam.Position.A.Value;
                    //Loader.SetArmUpOffset(ARM.Definition.UpOffset.Value);
                    double Upos =
                        accparam.Position.U.Value +
                        ARM.Definition.EndOffset.Value * -1.0;

                    double Wpos = accparam.Position.W.Value;

                    UExtensionMoveParam Epos = accparam.Position.E.Value;

                    retVal = LoaderAbsMove(Apos, Upos, Wpos, Epos, ARM, LoaderMovingTypeEnum.NORMAL);
                }
                else
                {
                    retVal = EventCodeEnum.LOADER_STATE_INVALID;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum PreChuckUpMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (ARM.Holder.Status == EnumSubsStatus.EXIST &&
                    Chuck.Holder.Status == EnumSubsStatus.NOT_EXIST)
                {
                    var SubObj = ARM.Holder;

                    //Generate
                    var ARMAxisType = ARM.Definition.AxisType;
                    var accparam = Chuck.GetAccessParam(SubObj.TransferObject.Type.Value, SubObj.TransferObject.Size.Value);

                    if(accparam != null)
                    {
                        double Apos =
                        accparam.Position.A.Value +
                        accparam.PickupIncrement.Value;
                        //   this.Loader.SetArmUpOffset(ARM.Definition.UpOffset.Value);
                        double Wpos = accparam.Position.W.Value;
                        Wpos = 0.0;
                        retVal = LoaderAbsMove(Apos, Wpos, LoaderMovingTypeEnum.NORMAL);
                    }
                    else
                    {
                        LoggerManager.Error($"[LoaderF20MovingMethds], PreChuckUpMove() : accparam is null.");

                        retVal = EventCodeEnum.LOADER_STATE_INVALID;
                    }
                }
                else
                {
                    retVal = EventCodeEnum.LOADER_STATE_INVALID;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum PreChuckDownMove(IARMModule ARM, IChuckModule Chuck)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (ARM.Holder.Status == EnumSubsStatus.NOT_EXIST &&
                    Chuck.Holder.Status == EnumSubsStatus.EXIST)
                {
                    var SubObj = Chuck.Holder.TransferObject;

                    //Generate
                    var ARMAxisType = ARM.Definition.AxisType;
                    var accparam = Chuck.GetAccessParam(SubObj.Type.Value, SubObj.Size.Value);

                    double Apos =
                        accparam.Position.A.Value;
                    // this.Loader.SetArmUpOffset(ARM.Definition.UpOffset.Value);
                    double Wpos = accparam.Position.W.Value;

                    retVal = LoaderAbsMove(Apos, Wpos, LoaderMovingTypeEnum.NORMAL);
                }
                else
                {
                    retVal = EventCodeEnum.LOADER_STATE_INVALID;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum OCRMoveFromPreAlignUp(IARMModule ARM, IOCRReadable OCR, IPreAlignModule PA)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (ARM.Holder.Status == EnumSubsStatus.EXIST)
                {
                    var SubObj = ARM.Holder;

                    //Generate
                    var ARMAxisType = ARM.Definition.AxisType;
                    var accparam = OCR.GetAccessParam(SubObj.TransferObject.Type.Value, SubObj.TransferObject.Size.Value);

                    double offsetU, offsetW;

                    if (SubObj.TransferObject.OverrideOCRDeviceOption.IsEnable.Value)
                    {
                        offsetU = SubObj.TransferObject.OverrideOCRDeviceOption.OCRDeviceBase.OffsetU.Value;
                        offsetW = SubObj.TransferObject.OverrideOCRDeviceOption.OCRDeviceBase.OffsetW.Value;
                    }
                    else
                    {
                        var offset = OCR.GetOCROffset();
                        offsetU = offset.OffsetU;
                        offsetW = offset.OffsetW;
                    }


                    double Apos = accparam.Position.A.Value;
                    //double Upos = accparam.Position.U.Value + offsetU;
                    //double Wpos = accparam.Position.W.Value + offsetW;
                    double Upos = accparam.Position.U.Value + this.Loader.Container.Resolve<ICognexProcessManager>().CognexProcDevParam.ConfigList[PA.ID.Index - 1].UOffset;
                    double Wpos = accparam.Position.W.Value + this.Loader.Container.Resolve<ICognexProcessManager>().CognexProcDevParam.ConfigList[PA.ID.Index - 1].WOffset;

                    //1. Move U
                    retVal = Loader.MotionManager.AbsMove(ARM.Definition.AxisType.Value, Upos);

                    //2. Move W
                    if (retVal == EventCodeEnum.NONE)
                    {
                        retVal = Loader.MotionManager.AbsMove(EnumAxisConstants.W, Wpos);
                    }

                    if (retVal == EventCodeEnum.NONE)
                    {
                        retVal = Loader.MotionManager.AbsMove(EnumAxisConstants.A, Apos);
                    }

                }
                else
                {
                    retVal = EventCodeEnum.LOADER_STATE_INVALID;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum PreAlignUpMoveFromOCR(IARMModule ARM, IPreAlignModule PA, IOCRReadable OCR)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (ARM.Holder.Status == EnumSubsStatus.EXIST &&
                    PA.Holder.Status == EnumSubsStatus.NOT_EXIST)
                {
                    var SubObj = ARM.Holder;

                    //Generate
                    var accparam = PA.GetAccessParam(SubObj.TransferObject.Type.Value, SubObj.TransferObject.Size.Value);

                    var ARMAxisType = ARM.Definition.AxisType;

                    double Apos =
                        accparam.Position.A.Value +
                        ARM.Definition.UpOffset.Value * -1.0 +
                        accparam.PickupIncrement.Value;

                    double Upos = accparam.Position.U.Value;
                    double Wpos = accparam.Position.W.Value;

                    //0. Align V axis with tranfer object.
                    retVal = Loader.MotionManager.AbsMove(
                            EnumAxisConstants.V,
                            ARM.Holder.TransferObject.NotchAngle.Value * ConstantValues.V_DEGREE_TO_DIST);
                    //1. Move A
                    retVal = Loader.MotionManager.AbsMove(EnumAxisConstants.A, Apos);
                    //2. Move W
                    if (retVal == EventCodeEnum.NONE)
                    {
                        retVal = Loader.MotionManager.AbsMove(EnumAxisConstants.W, Wpos);
                    }
                    //3. Move U
                    if (retVal == EventCodeEnum.NONE)
                    {
                        retVal = Loader.MotionManager.AbsMove(ARM.Definition.AxisType.Value, Upos);
                    }

                }
                else
                {
                    retVal = EventCodeEnum.LOADER_STATE_INVALID;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum RelMove(EnumAxisConstants axis, double value, LoaderMovingTypeEnum movingType)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                var axisObj = Loader.MotionManager.GetAxis(axis);
                double vel = GetSpeed(axisObj, movingType);
                //double acc = axisObj.Param.Acceleration.Value;
                double acc = GetAcc(axisObj, movingType);


                retVal = Loader.MotionManager.RelMove(axis, value, vel, acc);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum RelMove(EnumAxisConstants axis, double value, LoaderMovingTypeEnum movingType, Func<bool> stopFunc, bool resumeVal)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                var axisObj = Loader.MotionManager.GetAxis(axis);
                double vel = GetSpeed(axisObj, movingType);
                double acc = GetAcc(axisObj, movingType);

                retVal = Loader.MotionManager.RelMoveAsync(axis, value, vel, acc);
                retVal = Loader.MotionManager.WaitForAxisMotionDone(axis, stopFunc, resumeVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum FoupCoverUp(ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                int cassetteNum = Cassette.ID.Index;
                var foupInfo = Loader.ServiceCallback.FOUP_GetFoupModuleInfo(cassetteNum);
                if (foupInfo.FoupCoverState != FoupCoverStateEnum.CLOSE)
                {
                    retVal = Loader.ServiceCallback.FOUP_FoupCoverUp(cassetteNum);
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

        public EventCodeEnum FoupCoverDown(ICassetteModule Cassette)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                int cassetteNum = Cassette.ID.Index;
                var foupInfo = Loader.ServiceCallback.FOUP_GetFoupModuleInfo(cassetteNum);
                if (foupInfo.FoupCoverState != FoupCoverStateEnum.OPEN)
                {
                    retVal = Loader.ServiceCallback.FOUP_FoupCoverDown(cassetteNum);
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


        //=> Private
        private EventCodeEnum LoaderAbsMove(double Apos, double Wpos, LoaderMovingTypeEnum movingType)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            double vel, acc;
            try
            {
                // retVal = RetractARMs(LoaderMovingTypeEnum.NORMAL);
                //if (retVal == EventCodeEnum.NONE)
                //{
                //=> Move W
                //move W
                var WaxisObj = Loader.MotionManager.GetAxis(EnumAxisConstants.W);
                vel = GetSpeed(WaxisObj, movingType);
                acc = GetAcc(WaxisObj, movingType);

                //retVal = Loader.MotionManager.AbsMoveAsync(EnumAxisConstants.W, WaxisObj.Param.ClearedPosition.Value, vel, acc);

                ////=> Wait for W
                //retVal = Loader.MotionManager.WaitForAxisMotionDone(EnumAxisConstants.W);

                //if (retVal == EventCodeEnum.NONE)
                //{
                //move A
                var AaxisObj = Loader.MotionManager.GetAxis(EnumAxisConstants.A);
                vel = GetSpeed(AaxisObj, movingType);
                acc = GetAcc(AaxisObj, movingType);

                retVal = Loader.MotionManager.AbsMoveAsync(EnumAxisConstants.A, Apos, vel, acc);

                retVal = Loader.MotionManager.WaitForAxisMotionDone(EnumAxisConstants.A);

                //if (retVal == EventCodeEnum.NONE)
                //{
                //    vel = GetSpeed(WaxisObj, movingType);
                //    acc = WaxisObj.Param.Acceleration.Value;

                //    retVal = Loader.MotionManager.AbsMoveAsync(EnumAxisConstants.W, Wpos, vel, acc);

                //    //=> Wait for W
                //    retVal = Loader.MotionManager.WaitForAxisMotionDone(EnumAxisConstants.W);

                //}
                //}
                //else
                //{
                //    //Error
                //    retVal = EventCodeEnum.MOTION_MOVING_ERROR;
                //}

                //  }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        //=> Private
        private EventCodeEnum LoaderAbsMove(double Apos, double Wpos, UExtensionMoveParam Epos, LoaderMovingTypeEnum movingType)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                double vel, acc;

                //retVal = RetractARMs(LoaderMovingTypeEnum.NORMAL);
                //if (retVal == EventCodeEnum.NONE)
                //{
                var axissc = Loader.MotionManager.GetAxis(EnumAxisConstants.SC);
                retVal = ScanAxisAbsMove(axissc.Param.ClearedPosition.Value, LoaderMovingTypeEnum.NORMAL);
                //=> Move W
                //move W
                var WaxisObj = Loader.MotionManager.GetAxis(EnumAxisConstants.W);
                vel = GetSpeed(WaxisObj, movingType);
                acc = GetAcc(WaxisObj, movingType);

                retVal = Loader.MotionManager.AbsMoveAsync(EnumAxisConstants.W, WaxisObj.Param.ClearedPosition.Value, vel, acc);

                //=> Wait for W
                retVal = Loader.MotionManager.WaitForAxisMotionDone(EnumAxisConstants.W);

                if (retVal == EventCodeEnum.NONE)
                {
                    //move A
                    var AaxisObj = Loader.MotionManager.GetAxis(EnumAxisConstants.A);
                    vel = GetSpeed(AaxisObj, movingType);
                    acc = GetAcc(AaxisObj, movingType);

                    retVal = Loader.MotionManager.AbsMoveAsync(EnumAxisConstants.A, Apos, vel, acc);
                    retVal = Loader.MotionManager.WaitForAxisMotionDone(EnumAxisConstants.A);

                    if (retVal == EventCodeEnum.NONE)
                    {
                        //move W
                        vel = GetSpeed(WaxisObj, movingType);
                        acc = GetAcc(WaxisObj, movingType);

                        retVal = Loader.MotionManager.AbsMoveAsync(EnumAxisConstants.W, Wpos, vel, acc);

                        //=> Wait for W
                        retVal = Loader.MotionManager.WaitForAxisMotionDone(EnumAxisConstants.W);
                        if (retVal == EventCodeEnum.NONE)
                        {
                            retVal = UExtension.MoveTo(Epos, movingType);
                        }
                        else
                        {
                            retVal = EventCodeEnum.MOTION_MOVING_ERROR;
                        }
                    }
                    //}
                    //else
                    //{
                    //    //Error
                    //    retVal = EventCodeEnum.MOTION_MOVING_ERROR;
                    //}

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }


        private EventCodeEnum ScanAxisAbsMove(double scpos, LoaderMovingTypeEnum movingType)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                double vel, acc;
                //=> Move 
                //move Scan
                {
                    //retVal = Loader.IOManager.WriteIO(Loader.IOManager.IOMappings.Outputs.DOFOUPSWING, false);
                    //retVal = Loader.IOManager.WaitForIO(Loader.IOManager.IOMappings.Inputs.DIFOUPSWINGSENSOR, true);
                    //if (retVal == EventCodeEnum.NONE)
                    //{
                    var scanaxisObj = Loader.MotionManager.GetAxis(EnumAxisConstants.SC);
                    vel = GetSpeed(scanaxisObj, movingType);
                    acc = GetAcc(scanaxisObj, movingType);

                    //retVal = Loader.MotionManager.AbsMoveAsync(EnumAxisConstants.SC, scpos, vel, acc);
                    retVal = Loader.MotionManager.AbsMove(EnumAxisConstants.SC, scpos, vel, acc);
                    System.Threading.Thread.Sleep(100);

                    //=> Wait for Scan
                    EventCodeEnum ScanaxisMotionDoneRetVal;
                    ScanaxisMotionDoneRetVal = Loader.MotionManager.WaitForAxisMotionDone(EnumAxisConstants.SC);
                    //}
                    //else
                    //{
                    //    retVal = EventCodeEnum.IO_TIMEOUT_ERROR;
                    //}

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        private EventCodeEnum LoaderAbsMove(double Apos, double Upos, double Wpos, UExtensionMoveParam Epos,
            IARMModule ARM,
            LoaderMovingTypeEnum movingType,
            Func<bool> stopFunc = null, bool resumeVal = false)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                double vel, acc;
                //=> Move 

                //retVal = RetractARMs(LoaderMovingTypeEnum.NORMAL);
                //if (retVal == EventCodeEnum.NONE)
                //{
                //move W Cleard
                var WaxisObj = Loader.MotionManager.GetAxis(EnumAxisConstants.W);
                //vel = GetSpeed(WaxisObj, movingType);
                //acc = WaxisObj.Param.Acceleration.Value;

                //retVal = Loader.MotionManager.AbsMoveAsync(EnumAxisConstants.W, WaxisObj.Param.ClearedPosition.Value, vel, acc);

                ////=> Wait for W
                //retVal = Loader.MotionManager.WaitForAxisMotionDone(EnumAxisConstants.W);
                //move A
                //if (retVal == EventCodeEnum.NONE)
                //{
                var AaxisObj = Loader.MotionManager.GetAxis(EnumAxisConstants.A);
                vel = GetSpeed(AaxisObj, movingType);
                acc = GetAcc(AaxisObj, movingType);

                retVal = Loader.MotionManager.AbsMoveAsync(EnumAxisConstants.A, Apos, vel, acc);

                retVal = Loader.MotionManager.WaitForAxisMotionDone(EnumAxisConstants.A);

                //WMove

                if (retVal == EventCodeEnum.NONE)
                {
                    vel = GetSpeed(WaxisObj, movingType);
                    acc = GetAcc(WaxisObj, movingType);

                    retVal = Loader.MotionManager.AbsMoveAsync(EnumAxisConstants.W, Wpos, vel, acc);

                    //=> Wait for W
                    retVal = Loader.MotionManager.WaitForAxisMotionDone(EnumAxisConstants.W);

                    //move E
                    if (retVal == EventCodeEnum.NONE)
                    {
                        retVal = UExtension.MoveTo(Epos, movingType);

                        //=> move U
                        if (retVal == EventCodeEnum.NONE)
                        {
                            var UaxisObj = Loader.MotionManager.GetAxis(ARM.Definition.AxisType.Value);
                            vel = GetSpeed(UaxisObj, movingType);
                            acc = GetAcc(UaxisObj, movingType);

                            retVal = Loader.MotionManager.AbsMoveAsync(UaxisObj.AxisType.Value, Upos, vel, acc);

                            retVal = Loader.MotionManager.WaitForAxisMotionDone(ARM.Definition.AxisType.Value, stopFunc, resumeVal);
                        }
                        else
                        {
                            retVal = EventCodeEnum.MOTION_MOVING_ERROR;
                        }
                    }
                    else
                    {
                        retVal = EventCodeEnum.MOTION_MOVING_ERROR;
                    }
                    //}
                    //else
                    //{
                    //    retVal = EventCodeEnum.MOTION_MOVING_ERROR;
                    //}
                    //}
                    //else
                    //{
                    //    retVal = EventCodeEnum.MOTION_MOVING_ERROR;
                    //}

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }



            return retVal;
        }

        private double GetAcc(EnumAxisConstants axis, LoaderMovingTypeEnum movingType)
        {
            double acc = 0;
            try
            {
                acc = Loader.MotionManager.GetAxis(axis).Param.Acceleration.Value;
                switch (movingType)
                {
                    case LoaderMovingTypeEnum.NORMAL:
                        break;
                    case LoaderMovingTypeEnum.RECOVERY:

                        break;
                    case LoaderMovingTypeEnum.ACCESS:
                        acc = acc * ConstantValues.ACCESS_SPEED_RATIO;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return acc;
        }
        private double GetAcc(ProbeAxisObject obj, LoaderMovingTypeEnum movingType)
        {
            double acc = 0;
            try
            {
                acc = obj.Param.Acceleration.Value;
                switch (movingType)
                {
                    case LoaderMovingTypeEnum.NORMAL:
                        break;
                    case LoaderMovingTypeEnum.RECOVERY:

                        break;
                    case LoaderMovingTypeEnum.ACCESS:
                        acc = acc * ConstantValues.ACCESS_SPEED_RATIO;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return acc;
        }
        private double GetDec(ProbeAxisObject obj, LoaderMovingTypeEnum movingType)
        {
            double dec = 0;
            try
            {
                dec = obj.Param.Decceleration.Value;
                switch (movingType)
                {
                    case LoaderMovingTypeEnum.NORMAL:
                        break;
                    case LoaderMovingTypeEnum.RECOVERY:

                        break;
                    case LoaderMovingTypeEnum.ACCESS:
                        dec = dec * ConstantValues.ACCESS_SPEED_RATIO;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return dec;
        }

        private double GetDec(EnumAxisConstants axis, LoaderMovingTypeEnum movingType)
        {
            double dec = 0;
            try
            {
                dec = Loader.MotionManager.GetAxis(axis).Param.Decceleration.Value;
                switch (movingType)
                {
                    case LoaderMovingTypeEnum.NORMAL:
                        break;
                    case LoaderMovingTypeEnum.RECOVERY:

                        break;
                    case LoaderMovingTypeEnum.ACCESS:
                        dec = dec * ConstantValues.ACCESS_SPEED_RATIO;
                        break;
                    default:
                        break;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return dec;
        }

        private double GetSpeed(ProbeAxisObject obj, LoaderMovingTypeEnum movingType)
        {
            double speed = 0;
            try
            {
                speed = obj.Param.Speed.Value;

                switch (movingType)
                {
                    case LoaderMovingTypeEnum.NORMAL:
                        break;
                    case LoaderMovingTypeEnum.RECOVERY:
                        speed = speed * ConstantValues.RECOVERY_SPEED_RATIO;
                        break;
                    case LoaderMovingTypeEnum.ACCESS:
                        speed = speed * ConstantValues.ACCESS_SPEED_RATIO;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return speed;
        }

        private double GetSpeed(EnumAxisConstants axis, LoaderMovingTypeEnum movingType)
        {
            double speed = 0;
            try
            {
                speed = GetSpeed(Loader.MotionManager.GetAxis(axis), movingType);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return speed;
        }

        private EventCodeEnum ValidateWaferExistARMs()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                var ARMs = Loader.ModuleManager.FindModules<IARMModule>();

                int invalidCount = 0;
                foreach (var ARM in ARMs)
                {
                    if (ARM.Holder.Status == EnumSubsStatus.EXIST)
                    {
                        ARM.ValidateWaferStatus();
                        if (ARM.Holder.Status != EnumSubsStatus.EXIST)
                        {
                            invalidCount++;
                        }
                    }
                }

                if (invalidCount > 0)
                {
                    retVal = EventCodeEnum.LOADER_WAFER_MISSED_ON_ARM;
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


        #region SetupMethod
        public EventCodeEnum SetupToCstSlot1Method(IScanSensorModule ScanSensor, ICassetteModule Cassette, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool skipuaxis, int slot, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Loader.IOManager.MonitorForIO(Loader.IOManager.IOMappings.Inputs.DIWAFERSENSOR, true);
                if (retVal == EventCodeEnum.NONE)
                {
                    //=> move to sensor scan start position
                    var slot1Param = Cassette.GetSlot1AccessParam(Cassette.Device.AllocateDeviceInfo.Type.Value, Cassette.Device.AllocateDeviceInfo.Size.Value);

                    var scanParam = ScanSensor.GetScanParam(Cassette);
                    double Apos = slot1Param.Position.A.Value;

                    double Wpos = slot1Param.Position.W.Value;

                    UExtensionMoveParam Epos = slot1Param.Position.E.Value;

                    retVal = LoaderAbsMove(Apos, Wpos, Epos, LoaderMovingTypeEnum.NORMAL);

                    if (retVal == EventCodeEnum.NONE)
                    {
                        if (skipuaxis == false)
                        {
                            retVal = Loader.MotionManager.AbsMove(EnumAxisConstants.U1, slot1Param.Position.U.Value);
                            retVal = Loader.MotionManager.WaitForAxisMotionDone(EnumAxisConstants.U1);
                        }
                        else
                        {

                        }
                    }
                }
                else
                {
                    // Error
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


            return retVal;
        }

        public EventCodeEnum SetupToPAMoveMethod(IARMModule ARM, IPreAlignModule PA, LoaderMovingTypeEnum movingType, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool skipuaxis, int slot, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                var paaccesparam = PA.GetAccessParam(subtype, subsize);

                UExtensionMoveParam Epos = paaccesparam.Position.E.Value;

                retVal = LoaderAbsMove(paaccesparam.Position.A.Value, paaccesparam.Position.W.Value, Epos, LoaderMovingTypeEnum.NORMAL);
                if (retVal == EventCodeEnum.NONE)
                {
                    if (skipuaxis == false)
                    {
                        retVal = Loader.MotionManager.AbsMove(EnumAxisConstants.U1, paaccesparam.Position.U.Value);
                        retVal = Loader.MotionManager.WaitForAxisMotionDone(EnumAxisConstants.U1);
                    }
                    else
                    {

                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;

        }

        public EventCodeEnum SetupToOCRMoveMethod(IARMModule ARM, IPreAlignModule PA, IOCRReadable OCR, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool skipuaxis, int slot, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                var ocrAccesparam = OCR.GetAccessParam(subtype, subsize);

                UExtensionMoveParam Epos = ocrAccesparam.Position.E.Value;
                // uSkip할수가 없음  유 무조건 함 
                retVal = LoaderAbsMove(ocrAccesparam.Position.A.Value, ocrAccesparam.Position.W.Value, Epos, LoaderMovingTypeEnum.NORMAL);
                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = Loader.MotionManager.AbsMove(EnumAxisConstants.U1, ocrAccesparam.Position.U.Value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SetupToChuckMoveMethod(IARMModule ARM, IChuckModule Chuck, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool skipuaxis, int slot, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                var chuckAccesparam = Chuck.GetAccessParam(subtype, subsize);

                UExtensionMoveParam Epos = chuckAccesparam.Position.E.Value;

                retVal = LoaderAbsMove(chuckAccesparam.Position.A.Value, chuckAccesparam.Position.W.Value, Epos, LoaderMovingTypeEnum.NORMAL);
                if (retVal == EventCodeEnum.NONE)
                {
                    if (skipuaxis == false)
                    {
                        retVal = Loader.MotionManager.AbsMove(EnumAxisConstants.U1, chuckAccesparam.Position.U.Value);
                        retVal = Loader.MotionManager.WaitForAxisMotionDone(EnumAxisConstants.U1);
                    }
                    else
                    {

                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


            return retVal;
        }

        public EventCodeEnum SetupToFixedTrayMoveMethod(IARMModule ARM, IFixedTrayModule FixedTray, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool skipuaxis, int slot, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                var fixedAccesparam = FixedTray.GetAccessParam(subtype, subsize);

                UExtensionMoveParam Epos = fixedAccesparam.Position.E.Value;

                retVal = LoaderAbsMove(fixedAccesparam.Position.A.Value, fixedAccesparam.Position.W.Value, Epos, LoaderMovingTypeEnum.NORMAL);
                if (retVal == EventCodeEnum.NONE)
                {
                    if (skipuaxis == false)
                    {
                        retVal = Loader.MotionManager.AbsMove(EnumAxisConstants.U1, fixedAccesparam.Position.U.Value);
                        retVal = Loader.MotionManager.WaitForAxisMotionDone(EnumAxisConstants.U1);
                    }
                    else
                    {

                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum SetupToInspectionTrayMoveMethod(IARMModule ARM, IInspectionTrayModule InspectionTray, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool skipuaxis, int slot, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                var inspectionaccesparam = InspectionTray.GetAccessParam(subtype, subsize);

                UExtensionMoveParam Epos = inspectionaccesparam.Position.E.Value;

                retVal = LoaderAbsMove(inspectionaccesparam.Position.A.Value, inspectionaccesparam.Position.W.Value, Epos, LoaderMovingTypeEnum.NORMAL);
                if (retVal == EventCodeEnum.NONE)
                {
                    if (skipuaxis == false)
                    {
                        retVal = Loader.MotionManager.AbsMove(EnumAxisConstants.U1, inspectionaccesparam.Position.U.Value);
                        retVal = Loader.MotionManager.WaitForAxisMotionDone(EnumAxisConstants.U1);
                    }
                    else
                    {

                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum SetupToCstMoveMethod(IARMModule ARM, ISlotModule Slot, SubstrateTypeEnum subtype, SubstrateSizeEnum subsize, bool skipuaxis, int slot, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                var SubObj = ARM.Holder;

                //Generate
                var ARMAxisType = ARM.Definition.AxisType;
                var Cassette = Slot.Cassette;
                var localSlotNumber = slot;
                var slot1Param = Slot.Cassette.GetSlot1AccessParam(subtype, subsize);

                double Apos =
                    slot1Param.Position.A.Value +
                    Cassette.Device.SlotSize.Value * (localSlotNumber - 1);

                double PrevUpos = slot1Param.Position.U.Value - Math.Abs(slot1Param.UStopPosOffset.Value);

                double Wpos = slot1Param.Position.W.Value;

                UExtensionMoveParam Epos = slot1Param.Position.E.Value;

                retVal = LoaderAbsMove(Apos, Wpos, Epos, LoaderMovingTypeEnum.NORMAL);
                if (retVal == EventCodeEnum.NONE)
                {
                    if (skipuaxis == false)
                    {
                        //=> move to slot (step1)
                        retVal = LoaderAbsMove(Apos, PrevUpos, Wpos, Epos, ARM, LoaderMovingTypeEnum.NORMAL);

                        //=> Vacuum off if ARM has substrate.
                        if (retVal == EventCodeEnum.NONE)
                        {
                            if (ARM.Holder.Status == EnumSubsStatus.EXIST)
                            {
                                retVal = ARM.WriteVacuum(false);
                                retVal = ARM.WaitForVacuum(false);
                            }
                        }

                        //=> move to Upos (step1)
                        if (retVal == EventCodeEnum.NONE)
                        {
                            double Upos = slot1Param.Position.U.Value - ARM.Definition.EndOffset.Value;
                            retVal = Loader.MotionManager.AbsMove(ARMAxisType.Value, Upos);
                        }
                        else
                        {
                            retVal = EventCodeEnum.LOADER_STATE_INVALID;
                        }
                    }
                    else
                    {

                    }

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        #endregion
    }
}
