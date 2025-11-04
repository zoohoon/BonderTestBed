using System;

using Autofac;
using LoaderBase;
using ProberErrorCode;
using LoaderParameters;
using ProberInterfaces;
using LogModule;

namespace LoaderCore
{
    public class UExtensionNoneObject : IUExtensionObject
    {
        public EventCodeEnum Init(IContainer container, UExtensionBase definition)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                if (definition is UExtensionNone)
                    retVal = EventCodeEnum.NONE;
                else
                    retVal = EventCodeEnum.UNDEFINED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public UExtensionStateEnum GetState()
        {
            return UExtensionStateEnum.RETRACTED;
        }

        public EventCodeEnum Homming()
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

        public EventCodeEnum Retract(LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
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

        public EventCodeEnum MoveTo(UExtensionMoveParam param, LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
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
    }

    public class UExtensionMotorObject : IUExtensionObject
    {
        public UExtensionMotor Definition { get; set; }

        public Autofac.IContainer Container { get; set; }

        public ILoaderModule Loader => Container.Resolve<ILoaderModule>();

        public EventCodeEnum Init(IContainer container, UExtensionBase definition)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.Container = container;

                if (definition is UExtensionMotor)
                {
                    Definition = definition as UExtensionMotor;
                    retVal = EventCodeEnum.NONE;
                }
                else
                    retVal = EventCodeEnum.UNDEFINED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public UExtensionStateEnum GetState()
        {
            try
            {
                var axisObj = Loader.MotionManager.GetAxis(Definition.AxisType.Value);
                bool isInHome = Loader.MotionManager.GetIOHome(axisObj);

                if (isInHome == true)
                    return UExtensionStateEnum.RETRACTED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return UExtensionStateEnum.EXTENDED;
        }

        public EventCodeEnum Homming()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = EventCodeEnum.NODATA;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            //NoWORKS : motion manager에서 처리된다.

            return retVal;
        }

        public EventCodeEnum Retract(LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var axisObj = Loader.MotionManager.GetAxis(Definition.AxisType.Value);
                bool isInHome = Loader.MotionManager.GetIOHome(axisObj);

                double homePos = axisObj.Param.ClearedPosition.Value;//0;
                retVal = Loader.MotionManager.AbsMove(Definition.AxisType.Value, homePos);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum MoveTo(UExtensionMoveParam param, LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                var motorParam = param as UExtensionMotorMoveParam;

                retVal = Loader.MotionManager.AbsMove(Definition.AxisType.Value, motorParam.Value.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }


    }

    public class UExtensionCylinderObject : IUExtensionObject
    {
        public UExtensionCylinder Definition { get; set; }

        public Autofac.IContainer Container { get; set; }

        public ILoaderModule Loader => Container.Resolve<ILoaderModule>();

        private IOPortDescripter<bool> DOARMINAIR;
        private IOPortDescripter<bool> DOARMOUTAIR;
        private IOPortDescripter<bool> DIARMIN;
        private IOPortDescripter<bool> DIARMOUT;

        public EventCodeEnum Init(IContainer container, UExtensionBase definition)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.Container = container;

                if (definition is UExtensionCylinder)
                {
                    Definition = definition as UExtensionCylinder;

                    DOARMINAIR = Loader.IOManager.GetIOPortDescripter(Definition.DOARMINAIR.Value);
                    DOARMOUTAIR = Loader.IOManager.GetIOPortDescripter(Definition.DOARMOUTAIR.Value);
                    DIARMIN = Loader.IOManager.GetIOPortDescripter(Definition.DIARMIN.Value);
                    DIARMOUT = Loader.IOManager.GetIOPortDescripter(Definition.DIARMOUT.Value);

                    retVal = EventCodeEnum.NONE;
                }
                else
                    retVal = EventCodeEnum.UNDEFINED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public UExtensionStateEnum GetState()
        {
            UExtensionStateEnum state = UExtensionStateEnum.UNKNOWN;
            try
            {
                EventCodeEnum retVal;
                bool insensor, outsensor = false;
                retVal = Loader.IOManager.ReadIO(DIARMIN, out insensor);
                retVal = Loader.IOManager.ReadIO(DIARMOUT, out outsensor);

                if (insensor == true && outsensor == false)
                {
                    state = UExtensionStateEnum.RETRACTED;
                }
                else if (insensor == false && outsensor == true)
                {
                    state = UExtensionStateEnum.EXTENDED;
                }
                else
                {
                    state = UExtensionStateEnum.UNKNOWN;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return state;
        }

        public EventCodeEnum Homming()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Definition.UExtensionType.Value == UExtensionTypeEnum.CYLINDER)
                {
                    retVal = Retract();
                }
                else if (Definition.UExtensionType.Value == UExtensionTypeEnum.MOTOR)
                {
                    retVal = EventCodeEnum.NONE;
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

        public EventCodeEnum Retract(LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                var UExtensionCylinderMoveParamVar = new UExtensionCylinderMoveParam();

                UExtensionCylinderMoveParamVar.Port.Value = false;

                retVal = MoveTo(UExtensionCylinderMoveParamVar);

                //retVal = MoveTo(new UExtensionCylinderMoveParam() { Port = false });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum MoveTo(UExtensionMoveParam param, LoaderMovingTypeEnum movingType = LoaderMovingTypeEnum.NORMAL)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                var cylinderParam = param as UExtensionCylinderMoveParam;

                bool value = cylinderParam.Port.Value;

                retVal = Loader.IOManager.WriteIO(DOARMINAIR, !value);
                if (retVal == EventCodeEnum.NONE)
                    retVal = Loader.IOManager.WriteIO(DOARMOUTAIR, value);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Loader.IOManager.WaitForIO(DIARMIN, !value, Definition.IOWaitTimeout.Value);

                if (retVal == EventCodeEnum.NONE)
                    retVal = Loader.IOManager.WaitForIO(DIARMOUT, value, Definition.IOWaitTimeout.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }
}
