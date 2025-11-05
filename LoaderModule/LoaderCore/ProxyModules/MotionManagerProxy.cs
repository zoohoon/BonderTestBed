using System;
using System.Linq;

using Autofac;
using LoaderBase;
using ProberErrorCode;
using ProberInterfaces;
using LoaderParameters;
using LogModule;

namespace LoaderCore
{
    public class MotionManagerProxy : IMotionManagerProxy
    {
        public bool Initialized { get; set; } = false;
        public InitPriorityEnum InitPriority => InitPriorityEnum.LEVEL3;

        public IContainer Container { get; set; }

        public ILoaderModule Loader => Container.Resolve<ILoaderModule>();

        public IMotionManager MotionManager { get; set; }

        public ProbeAxes LoaderAxes => MotionManager.LoaderAxes;

        public EventCodeEnum InitModule(IContainer container)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.Container = container;

                if (Loader.ServiceType == LoaderServiceTypeEnum.DynamicLinking)
                {
                    MotionManager = Loader.StageContainer.Resolve<IMotionManager>();

                    retVal = EventCodeEnum.NONE;
                }
                else
                {

                    //TODO : 
                    throw new NotImplementedException();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void DeInitModule()
        {
            try
            {
                if (Loader.ServiceType == LoaderServiceTypeEnum.DynamicLinking)
                { 
                    //No Works.
                }
                else
                {
                    MotionManager.DeInitModule();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public ProbeAxisObject GetAxis(EnumAxisConstants axis)
        {
            ProbeAxisObject retObj = null;

            try
            {
                retObj = MotionManager.GetAxis(axis);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retObj;
        }

        public bool GetIOHome(ProbeAxisObject axisObj)
        {
            bool retVal = false;

            try
            {
                retVal = MotionManager.GetIOHome(axisObj);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SaveLoaderAxesObject()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                string FullPath;

                string RootPath = Loader.RootParamPath;
                string FilePath = MotionManager.LoaderAxes.FilePath;
                string FileName = MotionManager.LoaderAxes.FileName;

                if (FilePath != "")
                {
                    FullPath = RootPath + "\\" + FilePath + "\\" + FileName;
                }
                else
                {
                    FullPath = RootPath + "\\" + FileName;
                }

                retVal = Extensions_IParam.SaveParameter(null, MotionManager.LoaderAxes, null, FullPath,Extensions_IParam.FileType.NONE,null,true);
                if (retVal != EventCodeEnum.NONE)
                {
                    throw new Exception($"[{this.GetType().Name} - SaveAutoTiltSysFile] Faile SaveParameter");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum NotchFindMove(EnumAxisConstants axis, EnumMotorDedicatedIn input)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = MotionManager.NotchFinding(GetAxis(axis), input);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    // 신규 장비 추가(EnumLoaderMovingMethodType 추가)시 구분해서 처리해 주어야 함
                    if (this.Loader.SystemParameter.LoaderMovingMethodType == EnumLoaderMovingMethodType.OPUSV_MINI)
                    {
                        retval = MotionManager.HomingTaskRun(EnumAxisConstants.U1, EnumAxisConstants.U2);
                        if (retval != EventCodeEnum.NONE)
                        {
                            return EventCodeEnum.MOTION_HOMING_ERROR;
                        }

                        retval = MotionManager.HomingTaskRun(EnumAxisConstants.W);
                        if (retval != EventCodeEnum.NONE)
                        {
                            return EventCodeEnum.MOTION_HOMING_ERROR;
                        }

                        retval = MotionManager.HomingTaskRun(EnumAxisConstants.A);
                        if (retval != EventCodeEnum.NONE)
                        {
                            return EventCodeEnum.MOTION_HOMING_ERROR;
                        }

                        retval = MotionManager.HomingTaskRun(EnumAxisConstants.SC);
                        if (retval != EventCodeEnum.NONE)
                        {
                            return EventCodeEnum.MOTION_HOMING_ERROR;
                        }

                        retval = MotionManager.HomingTaskRun(EnumAxisConstants.V);
                        if (retval != EventCodeEnum.NONE)
                        {
                            retval = EventCodeEnum.MOTION_HOMING_ERROR;
                        }
                    }
                    else
                    {
                        retval = MotionManager.HomingTaskRun(EnumAxisConstants.U1, EnumAxisConstants.U2);
                        if (retval != EventCodeEnum.NONE)
                        {
                            return EventCodeEnum.MOTION_HOMING_ERROR;
                        }
                        retval = MotionManager.HomingTaskRun(EnumAxisConstants.W, EnumAxisConstants.A);
                        if (retval != EventCodeEnum.NONE)
                        {
                            return EventCodeEnum.MOTION_HOMING_ERROR;
                        }
                        retval = MotionManager.HomingTaskRun(EnumAxisConstants.V);
                        if (retval != EventCodeEnum.NONE)
                        {
                            retval = EventCodeEnum.MOTION_HOMING_ERROR;
                        }
                    }
                    //retVal = MotionManager.LoaderSystemInit();
                    

                    Initialized = false;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum HomeMove()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //system init 순서대로 움직인다.
                foreach (HomingGroup hominggroup in LoaderAxes.HomingGroups)
                {
                    var groupAxes = LoaderAxes
                        .ProbeAxisProviders
                        .Where(axis => axis.AxisType.Value == hominggroup.Stage.Value.Find(eax => eax == axis.AxisType.Value));

                    //=> move group
                    foreach (var axisObj in groupAxes)
                    {
                        if (axisObj.AxisType.Value == EnumAxisConstants.V)
                            continue;

                        double homePos = axisObj.Param.HomeOffset.Value;
                        int retCode = MotionManager.AbsMoveAsync(axisObj, homePos);
                        if (retCode != 0)
                        {
                            retVal = EventCodeEnum.MOTION_MOVING_ERROR;
                            break;
                        }
                    }

                    if (retVal != EventCodeEnum.NONE)
                        break;

                    //=> wait done group
                    foreach (var axisObj in groupAxes)
                    {
                        if (axisObj.AxisType.Value == EnumAxisConstants.V)
                            continue;

                        int retCode = MotionManager.WaitForAxisMotionDone(axisObj);
                        if (retCode != 0)
                        {
                            retVal = EventCodeEnum.MOTION_MOTIONDONE_ERROR;
                            break;
                        }
                    }

                    if (retVal != EventCodeEnum.NONE)
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum AbsMove(EnumAxisConstants axis, double pos)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = MotionManager.AbsMove(axis, pos);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum AbsMove(EnumAxisConstants axis, double pos, double vel, double acc)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = MotionManager.AbsMove(GetAxis(axis), pos, vel, acc);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum AbsMoveAsync(EnumAxisConstants axis, double pos, double vel, double acc)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                int rel = MotionManager.AbsMoveAsync(GetAxis(axis), pos, vel, acc);
                retVal = rel == 0 ? EventCodeEnum.NONE : EventCodeEnum.MOTION_MOVING_ERROR;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum RelMove(EnumAxisConstants axis, double pos)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = MotionManager.RelMove(axis, pos);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum RelMoveAsync(EnumAxisConstants axis, double pos)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                int rel = MotionManager.RelMoveAsync(GetAxis(axis), pos);
                retVal = rel == 0 ? EventCodeEnum.NONE : EventCodeEnum.MOTION_MOVING_ERROR;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum RelMove(EnumAxisConstants axis, double pos, double vel, double acc)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = MotionManager.RelMove(GetAxis(axis), pos, vel, acc);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum RelMoveAsync(EnumAxisConstants axis, double pos, double vel, double acc)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                int rel = MotionManager.RelMoveAsync(GetAxis(axis), pos, vel, acc);
                retVal = rel == 0 ? EventCodeEnum.NONE : EventCodeEnum.MOTION_MOVING_ERROR;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum WaitForAxisMotionDone(EnumAxisConstants axis)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                int rel = MotionManager.WaitForAxisMotionDone(GetAxis(axis));
                retVal = rel == 0 ? EventCodeEnum.NONE : EventCodeEnum.MOTION_MOVING_ERROR;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum WaitForAxisMotionDone(EnumAxisConstants axis, Func<bool> GetSourceLevel, bool resumeLevel, long timeout = 0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (GetSourceLevel != null)
                {
                    int rel = MotionManager.WaitForAxisMotionDone(GetAxis(axis), GetSourceLevel, resumeLevel, timeout);
                    retVal = rel == 0 ? EventCodeEnum.NONE : EventCodeEnum.MOTION_MOVING_ERROR;
                }
                else
                {
                    retVal = WaitForAxisMotionDone(axis);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum StartScanPosCapt(EnumAxisConstants axis, EnumMotorDedicatedIn MotorDedicatedIn)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                int rel = MotionManager.StartScanPosCapt(GetAxis(axis), MotorDedicatedIn);
                retVal = rel == 0 ? EventCodeEnum.NONE : EventCodeEnum.MOTION_MOVING_ERROR;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum StopScanPosCapt(EnumAxisConstants axis)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                int rel = MotionManager.StopScanPosCapt(GetAxis(axis));
                retVal = rel == 0 ? EventCodeEnum.NONE : EventCodeEnum.MOTION_MOVING_ERROR;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }
}