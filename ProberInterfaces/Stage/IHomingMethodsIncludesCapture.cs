using LogModule;
using System;
using System.Threading;
using SystemExceptions.MotionException;

namespace ProberInterfaces
{
    public class NHPIIncludeCapture : IHomingMethods
    {



        public int Homing(IMotionBase motionBase, AxisObject axis)
        {
            int retVal = -1;
            double vel = 0.0;
            long timeOut = 60000;

            try
            {
                retVal = motionBase.WaitForAxisMotionDone(axis, 6000);
                motionBase.SetLimitSWNegAct(axis, EnumEventActionType.ActionNONE);
                motionBase.SetLimitSWPosAct(axis, EnumEventActionType.ActionNONE);

                if (retVal != 0)
                {
                    throw new MotionException(
                        $"Homming for Axis {axis.Label}: Error occurred while wait for previous motion", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                }
                #region // Clear state and position

                retVal = motionBase.DisableAxis(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                Thread.Sleep(500);

                retVal = motionBase.AlarmReset(axis);

                motionBase.SetZeroPosition(axis);



                if (axis.VerticalWithoutBreak.Value == true)
                {

                    retVal = motionBase.WaitForHomeSensor(axis);

                    if (retVal != 0) throw new MotionException(
                        $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                    Thread.Sleep(1000);
                }

                retVal = motionBase.AlarmReset(axis);

                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.EnableAxis(axis);

                if (retVal != 0) throw new MotionException(
                                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));



                retVal = motionBase.SetSwitchAction(axis,
                    EnumDedicateInputs.DedicateInputHOME,
                    EnumEventActionType.ActionNONE,
                    axis.Param.HomeInvert.Value == true ? EnumInputLevel.Inverted : EnumInputLevel.Normal);

                //if (axis.Param.HomeInvert.Value == false)
                //{
                //    retVal = motionBase.SetSwitchAction(axis,
                //        EnumDedicateInputs.DedicateInputHOME, EnumEventActionType.ActionNONE, EnumInputLevel.Normal);
                //}
                //else
                //{
                //    retVal = motionBase.SetSwitchAction(axis,
                //       EnumDedicateInputs.DedicateInputHOME, EnumEventActionType.ActionNONE, EnumInputLevel.Inverted);
                //}
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.AlarmReset(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.SetPosition(axis, 0);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.AlarmReset(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.EnableAxis(axis);
                if (retVal != 0) throw new MotionException(
                                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                Thread.Sleep(1000);

                retVal = motionBase.SetPosition(axis, 0);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                #endregion



                #region // Searching for Home switch


                //////////////////////////////
                //$"Homming for Axis Test {axis.Label}: BeginSearching for Home switch", axis.Label);


                retVal = motionBase.SetSwitchAction(axis,
                   axis.Config.InputHome.Value,
                    EnumEventActionType.ActionSTOP,
                    axis.Param.HomeInvert.Value == true ? EnumInputLevel.Inverted : EnumInputLevel.Normal);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                vel = axis.Param.HommingSpeed.Value * -1;
                motionBase.VMove(axis, vel, EnumTrjType.Homming);

                retVal = motionBase.WaitForAxisEvent(axis, EnumAxisState.STOPPED, axis.Param.HomeDistLimit.Value);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                #endregion




                #region // Escape Home switch
                retVal = motionBase.SetSwitchAction(axis,
                     axis.Config.InputHome.Value,
                    EnumEventActionType.ActionNONE,
                    axis.Param.HomeInvert.Value == true ? EnumInputLevel.Inverted : EnumInputLevel.Normal);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.AlarmReset(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                retVal = motionBase.SetSwitchAction(axis,
                     axis.Config.InputHome.Value, EnumEventActionType.ActionSTOP,
                    axis.Param.HomeInvert.Value == false ? EnumInputLevel.Inverted : EnumInputLevel.Normal);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.AlarmReset(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                vel = axis.Param.HommingSpeed.Value * 1;
                motionBase.VMove(axis, vel, EnumTrjType.Homming);
                retVal = motionBase.WaitForAxisEvent(axis, EnumAxisState.STOPPED, axis.Param.HomeDistLimit.Value);
                if (retVal != 0) throw new MotionException(
                $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                /////////
                motionBase.ClearUserLimit(axis);
                retVal = motionBase.AlarmReset(axis);

                motionBase.SetZeroPosition(axis);


                #endregion



                #region // Homming shift
                if (axis.Param.HomeShift.Value != 0)
                {
                    motionBase.SetZeroPosition(axis);
                    retVal = motionBase.RelMove(axis, axis.Param.HomeShift.Value, EnumTrjType.Homming, 1);
                    if (retVal != 0) throw new MotionException(
                        $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                    /////
                    retVal = motionBase.WaitForAxisMotionDone(axis, timeOut);
                    if (retVal != 0) throw new MotionException(
                        $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                }
                #endregion



                motionBase.SetPosition(axis,
                    axis.Param.NegSWLimit.Value + axis.Param.HomeShift.Value);


                #region // Searching for Index

                ////
                //$"Homming for Axis Test {axis.Label}: BeginSearching for index", axis.Label);


                retVal = motionBase.SetSwitchAction(axis,
                     axis.Config.InputHome.Value,
                    EnumEventActionType.ActionNONE,
                    axis.Param.HomeInvert.Value == true ? EnumInputLevel.Inverted : EnumInputLevel.Normal);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                // motionBase.SetNoneLimit(axis.AxisType.Value );

                retVal = motionBase.AlarmReset(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.SetSwitchAction(axis,
                        axis.Config.InputIndex.Value, EnumEventActionType.ActionSTOP,
                        axis.Param.IndexInvert.Value == true ? EnumInputLevel.Normal : EnumInputLevel.Inverted);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.AlarmReset(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                //motionBase.SetNoneLimit(axis.AxisType.Value );

                retVal = motionBase.ConfigCapture(
                    axis, axis.Config.InputMotor.Value);

                vel = axis.Param.IndexSearchingSpeed.Value * 1;
                motionBase.VMove(axis, vel, EnumTrjType.Homming);
                retVal = motionBase.WaitForAxisEvent(axis, EnumAxisState.STOPPED, axis.Param.IndexDistLimit.Value);
                if (retVal != 0) throw new MotionException(
                $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                double latchedPulse = 0.0;
                retVal = motionBase.GetCaptureStatus(axis.AxisIndex.Value, axis.Status);

                if (axis.Status.MotionCaptureStatus.CaptureState == CaptureState.CaptureStateCAPTURED)
                {
                    latchedPulse = axis.Status.MotionCaptureStatus.LatchedValue;
                    LoggerManager.Debug($"Homming for Axis {axis.Label}: Latched pulse = {latchedPulse}.");
                }
                else
                {
                    if (retVal != 0)
                    {
                        throw new MotionException(
                             $"Homming for Axis {axis.Label}: Error occurred. Index not detected.", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                    }
                    else
                    {
                        // 혹시나 retVal의 값이 0인 경우
                        LoggerManager.Debug($"GetCaptureStatus() : State={axis.Status.MotionCaptureStatus.CaptureState}, retVal = {retVal}");

                        throw new MotionException(
                             $"Homming for Axis {axis.Label}: Error occurred. Index not detected.", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(-1));
                    }
                }
                ////
                //  SetNoneLimit(axis.AxisType.Value );

                #endregion


                #region // Disable event action
                retVal = motionBase.SetSwitchAction(axis,
                    axis.Config.InputHome.Value, EnumEventActionType.ActionNONE, axis.Param.HomeInvert.Value == true ? EnumInputLevel.Inverted : EnumInputLevel.Normal);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                retVal = motionBase.AlarmReset(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                #endregion

                #region // Set home offset
                Thread.Sleep(100);
                if (axis.Status.MotionCaptureStatus.CaptureState == CaptureState.CaptureStateCAPTURED)
                {
                    double latchedPos = axis.PtoD(latchedPulse);
                    LoggerManager.Debug($"Home to Index Distance = {latchedPos}");
                    Thread.Sleep(500);

                    retVal = motionBase.AbsMove(axis, latchedPos, axis.Param.FinalVelociy.Value, EnumTrjType.Homming);
                    if (retVal != 0) throw new MotionException(
                        $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                    /////
                    retVal = motionBase.WaitForAxisMotionDone(axis, timeOut);
                    if (retVal != 0) throw new MotionException(
                        $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                    Thread.Sleep(500);

                    retVal = motionBase.SetPosition(axis, axis.Param.HomeOffset.Value);
                    if (retVal != 0) throw new MotionException(
                        $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                }


                #endregion
            }
            catch (MotionException moterr)
            {
                //retVal = (int)EnumReturnCodes.ReturnCodeINVALID;
                retVal = (int)EnumMotionBaseReturnCode.FatalError;

                LoggerManager.Error($"Homming() MotionException error occurred: " + moterr.Message);
            }
            catch (Exception err)
            {
                //retVal = (int)EnumReturnCodes.ReturnCodeFATAL_ERROR;
                retVal = (int)EnumMotionBaseReturnCode.FatalError;

                LoggerManager.Error($"Homming() Function error: " + err.Message);

                throw new MotionException("Homming system exception occurred", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
            }
            finally
            {
                motionBase.SetLimitSWNegAct(axis, EnumEventActionType.ActionSTOP);
                motionBase.SetLimitSWPosAct(axis, EnumEventActionType.ActionSTOP);
                double pos = 0;
                motionBase.GetCommandPosition(axis, ref pos);
                axis.Status.RawPosition.Ref = pos;
            }
            return retVal;

        }

    }
    /// <summary>
    /// 플러스 방향으로 홈 탐색 후, 마이너스 방향으로 인덱스 탐색
    /// </summary>
    public class PHNIIncludeCapture : IHomingMethods
    {
        /// <summary>
        /// 호밍함수 입니다.
        /// </summary>
        /// <param name="motionBase"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public int Homing(IMotionBase motionBase, AxisObject axis)
        {
            int retVal = -1;
            double vel = 0.0;
            long timeOut = 60000;

            try
            {
                motionBase.SetLimitSWNegAct(axis, EnumEventActionType.ActionNONE);
                motionBase.SetLimitSWPosAct(axis, EnumEventActionType.ActionNONE);
                retVal = motionBase.WaitForAxisMotionDone(axis, 6000);
                if (retVal != 0)
                {
                    throw new MotionException(
                        $"Homming for Axis {axis.Label}: Error occurred while wait for previous motion", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                }
                #region // Clear state and position

                retVal = motionBase.DisableAxis(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                Thread.Sleep(500);

                retVal = motionBase.Stop(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                retVal = motionBase.SetSwitchAction(axis,
                    axis.Config.InputHome.Value,
                    EnumEventActionType.ActionNONE,
                    axis.Param.HomeInvert.Value == true ? EnumInputLevel.Inverted : EnumInputLevel.Normal);
                //if (axis.Param.HomeInvert.Value == false)
                //{
                //    retVal = motionBase.SetSwitchAction(axis,
                //        EnumDedicateInputs.DedicateInputHOME, EnumEventActionType.ActionNONE, EnumInputLevel.Normal);
                //}
                //else
                //{
                //    retVal = motionBase.SetSwitchAction(axis,
                //       EnumDedicateInputs.DedicateInputHOME, EnumEventActionType.ActionNONE, EnumInputLevel.Inverted);
                //}
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.AlarmReset(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.SetPosition(axis, 0);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.AlarmReset(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.EnableAxis(axis);
                if (retVal != 0) throw new MotionException(
                                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                Thread.Sleep(1000);

                retVal = motionBase.SetPosition(axis, 0);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                #endregion



                #region // Searching for Home switch

                //////////////////////
                //$"Homming for Axis Test {axis.Label}: BeginSearching for Home switch", axis.Label);


                retVal = motionBase.SetSwitchAction(axis,
                    axis.Config.InputHome.Value,
                    EnumEventActionType.ActionSTOP,
                    axis.Param.HomeInvert.Value == true ? EnumInputLevel.Inverted : EnumInputLevel.Normal);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                Thread.Sleep(1000);
                retVal = motionBase.SetPosition(axis, 0);

                vel = axis.Param.HommingSpeed.Value * 1;
                motionBase.VMove(axis, vel, EnumTrjType.Homming);
                retVal = motionBase.WaitForAxisEvent(axis, EnumAxisState.STOPPED, axis.Param.HomeDistLimit.Value);
                if (retVal != 0) throw new MotionException(
                $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                #endregion


                #region // Escape Home switch
                retVal = motionBase.SetSwitchAction(axis,
                    axis.Config.InputHome.Value,
                    EnumEventActionType.ActionNONE,
                    axis.Param.HomeInvert.Value == true ? EnumInputLevel.Inverted : EnumInputLevel.Normal);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.AlarmReset(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                retVal = motionBase.SetSwitchAction(axis,
                    axis.Config.InputHome.Value, EnumEventActionType.ActionSTOP,
                    axis.Param.HomeInvert.Value == false ? EnumInputLevel.Inverted : EnumInputLevel.Normal);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.AlarmReset(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                Thread.Sleep(200);
                vel = axis.Param.HommingSpeed.Value * -1;
                motionBase.VMove(axis, vel, EnumTrjType.Homming);
                retVal = motionBase.WaitForAxisEvent(axis, EnumAxisState.STOPPED, axis.Param.HomeDistLimit.Value);
                if (retVal != 0) throw new MotionException(
                $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                /////////
                motionBase.ClearUserLimit(axis);
                retVal = motionBase.AlarmReset(axis);
                motionBase.SetZeroPosition(axis);

                #endregion




                #region // Homming shift
                if (axis.Param.HomeShift.Value != 0)
                {
                    motionBase.SetZeroPosition(axis);

                    retVal = motionBase.RelMove(
                        axis, axis.Param.HomeShift.Value, axis.Param.HommingSpeed.Value,
                        axis.Param.HommingAcceleration.Value);
                    if (retVal != 0) throw new MotionException(
                        $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                    /////
                    retVal = motionBase.WaitForAxisMotionDone(axis, timeOut);
                    if (retVal != 0) throw new MotionException(
                        $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                }
                #endregion

                //motionBase.SetPosition(axis,
                //   axis.Param.PosSWLimit.Value - axis.Param.HomeShift.Value);
                motionBase.SetPosition(axis,
                   axis.Param.PosSWLimit.Value + axis.Param.HomeShift.Value);

                #region // Searching for Index

                //$"Homming for Axis Test {axis.Label}: BeginSearching for index", axis.Label);


                retVal = motionBase.SetSwitchAction(axis,
                    axis.Config.InputHome.Value,
                    EnumEventActionType.ActionNONE,
                    axis.Param.HomeInvert.Value == true ? EnumInputLevel.Inverted : EnumInputLevel.Normal);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.AlarmReset(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                retVal = motionBase.SetSwitchAction(axis,
                    axis.Config.InputIndex.Value, EnumEventActionType.ActionSTOP,
                    axis.Param.IndexInvert.Value == true ? EnumInputLevel.Normal : EnumInputLevel.Inverted);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.AlarmReset(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.ConfigCapture(axis, axis.Config.InputMotor.Value);

                vel = axis.Param.IndexSearchingSpeed.Value * -1;
                motionBase.VMove(axis, vel, EnumTrjType.Homming);
                retVal = motionBase.WaitForAxisEvent(axis, EnumAxisState.STOPPED, axis.Param.IndexDistLimit.Value);
                if (retVal != 0) throw new MotionException(
                $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                double latchedPulse = 0.0;
                retVal = motionBase.GetCaptureStatus(axis.AxisIndex.Value, axis.Status);

                if (axis.Status.MotionCaptureStatus.CaptureState == CaptureState.CaptureStateCAPTURED)
                {
                    latchedPulse = axis.Status.MotionCaptureStatus.LatchedValue;
                }
                else
                {
                    if (retVal != 0)
                    {
                        throw new MotionException(
                             $"Homming for Axis {axis.Label}: Error occurred. Index not detected.", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                    }
                    else
                    {
                        // 혹시나 retVal의 값이 0인 경우
                        LoggerManager.Debug($"GetCaptureStatus() : State={axis.Status.MotionCaptureStatus.CaptureState}, retVal = {retVal}");

                        throw new MotionException(
                             $"Homming for Axis {axis.Label}: Error occurred. Index not detected.", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(-1));
                    }
                }

                ////
                //  SetNoneLimit(axis.AxisType.Value );
                #endregion

                #region // Disable event action
                retVal = motionBase.SetSwitchAction(axis,
                    axis.Config.InputHome.Value, EnumEventActionType.ActionNONE, axis.Param.HomeInvert.Value == true ? EnumInputLevel.Inverted : EnumInputLevel.Normal);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                retVal = motionBase.AlarmReset(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                #endregion

                #region // Set home offset
                Thread.Sleep(100);

                if (axis.Status.MotionCaptureStatus.CaptureState == CaptureState.CaptureStateCAPTURED)
                {
                    double latchedPos = axis.PtoD(latchedPulse);
                    retVal = motionBase.AbsMove(axis, latchedPos, 0, EnumTrjType.Homming);
                    if (retVal != 0) throw new MotionException(
                        $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                    /////
                    retVal = motionBase.WaitForAxisMotionDone(axis, timeOut);
                    if (retVal != 0) throw new MotionException(
                        $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                    retVal = motionBase.SetPosition(axis, axis.Param.HomeOffset.Value);
                    if (retVal != 0) throw new MotionException(
                        $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                }

                #endregion
            }
            catch (MotionException moterr)
            {
                //retVal = (int)EnumReturnCodes.ReturnCodeINVALID;
                retVal = (int)EnumMotionBaseReturnCode.FatalError;

                LoggerManager.Exception(moterr);
                //LoggerManager.Error($"Homming() MotionException error occurred: " + moterr.Message);
            }
            catch (Exception err)
            {
                //retVal = (int)EnumReturnCodes.ReturnCodeFATAL_ERROR;
                retVal = (int)EnumMotionBaseReturnCode.FatalError;

                LoggerManager.Exception(err);
                //LoggerManager.Error($"Homming() Function error: " + err.Message);
                throw new MotionException("Homming system exception occurred", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
            }
            finally
            {
                motionBase.SetLimitSWNegAct(axis, EnumEventActionType.ActionSTOP);
                motionBase.SetLimitSWPosAct(axis, EnumEventActionType.ActionSTOP);
                double pos = 0;
                motionBase.GetCommandPosition(axis, ref pos);
                axis.Status.RawPosition.Ref = pos;
            }

            return retVal;
        }
    }

    public class NHIncludeCapture : IHomingMethods
    {
        public int Homing(IMotionBase motionBase, AxisObject axis)
        {
            int retVal = -1;
            double vel = 0.0;
            long timeOut = 60000;

            try
            {
                retVal = motionBase.WaitForAxisMotionDone(axis, 6000);
                if (retVal != 0)
                {
                    throw new MotionException(
                        $"Homming for Axis {axis.Label}: Error occurred while wait for previous motion", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                }
                #region // Clear state and position

                retVal = motionBase.DisableAxis(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                Thread.Sleep(500);

                retVal = motionBase.Stop(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                retVal = motionBase.SetSwitchAction(axis,
                    EnumDedicateInputs.DedicateInputHOME,
                    EnumEventActionType.ActionNONE,
                    axis.Param.HomeInvert.Value == true ? EnumInputLevel.Inverted : EnumInputLevel.Normal);
                //if (axis.Param.HomeInvert.Value == false)
                //{
                //    retVal = motionBase.SetSwitchAction(axis,
                //        EnumDedicateInputs.DedicateInputHOME, EnumEventActionType.ActionNONE, EnumInputLevel.Normal);
                //}
                //else
                //{
                //    retVal = motionBase.SetSwitchAction(axis,
                //       EnumDedicateInputs.DedicateInputHOME, EnumEventActionType.ActionNONE, EnumInputLevel.Inverted);
                //}
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.AlarmReset(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.SetPosition(axis, 0);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.AlarmReset(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.EnableAxis(axis);
                if (retVal != 0) throw new MotionException(
                                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                Thread.Sleep(1000);
                //motionBase.DisableCapture(axis);
                axis.Status.MotionCaptureStatus.LatchedValue = 0;
                retVal = motionBase.SetPosition(axis, 0);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                #endregion

                #region // Searching for Home switch
                motionBase.SetLimitSWNegAct(axis, EnumEventActionType.ActionNONE);
                motionBase.SetLimitSWPosAct(axis, EnumEventActionType.ActionNONE);

                retVal = motionBase.SetSwitchAction(axis,
                   axis.Config.InputHome.Value,
                    EnumEventActionType.ActionSTOP,
                    axis.Param.HomeInvert.Value == true ? EnumInputLevel.Inverted : EnumInputLevel.Normal);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                vel = axis.Param.HommingSpeed.Value * -1;
                motionBase.VMove(axis, vel, EnumTrjType.Homming);

                retVal = motionBase.WaitForAxisEvent(axis, EnumAxisState.STOPPED, axis.Param.HomeDistLimit.Value);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                #endregion

                #region HomeCapture

                retVal = motionBase.ConfigCapture(
                    axis, EnumMotorDedicatedIn.MotorDedicatedInHOME);

                #endregion


                #region // Escape Home switch
                retVal = motionBase.SetSwitchAction(axis,
                     axis.Config.InputHome.Value,
                    EnumEventActionType.ActionNONE,
                    axis.Param.HomeInvert.Value == true ? EnumInputLevel.Inverted : EnumInputLevel.Normal);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.AlarmReset(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                retVal = motionBase.SetSwitchAction(axis,
                     axis.Config.InputHome.Value, EnumEventActionType.ActionSTOP,
                    axis.Param.HomeInvert.Value == false ? EnumInputLevel.Inverted : EnumInputLevel.Normal);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.AlarmReset(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                vel = axis.Param.HommingSpeed.Value * 1;
                motionBase.VMove(axis, vel, EnumTrjType.Homming);
                retVal = motionBase.WaitForAxisEvent(axis, EnumAxisState.STOPPED, axis.Param.HomeDistLimit.Value);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                /////////
                motionBase.ClearUserLimit(axis.AxisIndex.Value);
                retVal = motionBase.AlarmReset(axis);

                double latchedPulse = 0.0;
                retVal = motionBase.GetCaptureStatus(axis.AxisIndex.Value, axis.Status);
                if (axis.Status.MotionCaptureStatus.CaptureState == CaptureState.CaptureStateCAPTURED)
                {
                    latchedPulse = axis.Status.MotionCaptureStatus.LatchedValue;
                }
                else
                {
                    if (retVal != 0)
                    {
                        throw new MotionException(
                                                 $"Homming for Axis {axis.Label}: Error occurred. Index not detected.", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                    }
                    else
                    {
                        // 혹시나 retVal의 값이 0인 경우
                        LoggerManager.Debug($"GetCaptureStatus() : State={axis.Status.MotionCaptureStatus.CaptureState}, retVal = {retVal}");

                        throw new MotionException(
                         $"Homming for Axis {axis.Label}: Error occurred. Index not detected.", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(-1));
                    }
                }




                #region // Disable event action
                retVal = motionBase.SetSwitchAction(axis,
                    axis.Config.InputHome.Value, EnumEventActionType.ActionNONE, axis.Param.HomeInvert.Value == true ? EnumInputLevel.Inverted : EnumInputLevel.Normal);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                retVal = motionBase.AlarmReset(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                #endregion

                #region // Set home offset
                Thread.Sleep(100);
                if (axis.Status.MotionCaptureStatus.CaptureState == CaptureState.CaptureStateCAPTURED)
                {
                    double latchedPos = axis.PtoD(latchedPulse);
                    retVal = motionBase.AbsMove(axis, latchedPos, axis.Param.FinalVelociy.Value, EnumTrjType.Homming);
                    if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                    ///
                    retVal = motionBase.WaitForAxisMotionDone(axis, timeOut);
                    if (retVal != 0) throw new MotionException(
                        $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                    retVal = motionBase.SetPosition(axis, axis.Param.HomeOffset.Value);
                    if (retVal != 0) throw new MotionException(
                        $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                }

                #endregion

                #endregion
            }
            catch (MotionException moterr)
            {
                //retVal = (int)EnumReturnCodes.ReturnCodeINVALID;
                retVal = (int)EnumMotionBaseReturnCode.FatalError;

                LoggerManager.Error($"Homming() MotionException error occurred: " + moterr.Message);
            }
            catch (Exception err)
            {
                //retVal = (int)EnumReturnCodes.ReturnCodeFATAL_ERROR;
                retVal = (int)EnumMotionBaseReturnCode.FatalError;

                LoggerManager.Error($"Homming() Function error: " + err.Message);
                throw new MotionException("Homming system exception occurred", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
            }
            finally
            {
                motionBase.SetLimitSWNegAct(axis, EnumEventActionType.ActionSTOP);
                motionBase.SetLimitSWPosAct(axis, EnumEventActionType.ActionSTOP);
                double pos = 0;
                motionBase.GetCommandPosition(axis, ref pos);
                axis.Status.RawPosition.Ref = pos;
            }
            return retVal;
        }
    }

    public class VHincludecapture : IHomingMethods
    {
        public int Homing(IMotionBase motionBase, AxisObject axis)
        {
            int retVal = -1;

            //double vel = 0.0;
            //long timeOut = 60000;

            try
            {
                retVal = motionBase.WaitForAxisMotionDone(axis, 6000);
                if (retVal != 0)
                {
                    throw new MotionException(
                        $"Homming for Axis {axis.Label}: Error occurred while wait for previous motion", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                }

                retVal = motionBase.DisableAxis(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                //Thread.Sleep500);
                System.Threading.Thread.Sleep(500);

                retVal = motionBase.AlarmReset(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.SetPosition(axis, 0);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.EnableAxis(axis);
                if (retVal != 0) throw new MotionException(
                                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

            }
            catch (MotionException moterr)
            {
                //retVal = (int)EnumReturnCodes.ReturnCodeINVALID;
                retVal = (int)EnumMotionBaseReturnCode.FatalError;

                LoggerManager.Error($"Homming() MotionException error occurred: " + moterr.Message);
            }
            catch (Exception err)
            {
                //retVal = (int)EnumReturnCodes.ReturnCodeFATAL_ERROR;
                retVal = (int)EnumMotionBaseReturnCode.FatalError;

                LoggerManager.Error($"Homming() Function error: " + err.Message);
                throw new MotionException("Homming system exception occurred", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
            }
            finally
            {
                //motionBase.SetLimitSWNegAct(axis, EnumEventActionType.ActionSTOP);
                //motionBase.SetLimitSWPosAct(axis, EnumEventActionType.ActionSTOP);
                double pos = 0;
                motionBase.GetCommandPosition(axis, ref pos);
                axis.Status.RawPosition.Ref = pos;
            }
            return retVal;
        }
    }
}
