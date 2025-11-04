using System;
using SystemExceptions.MotionException;
using LogModule;
using System.Threading;

namespace ProberInterfaces
{

    public enum HomingMethodType
    {
        NONE = 0,
        RLSEDGEINDEX = 1,
        FLSEDGEINDEX = 2,
        PHNI = 3,
        NHPI = 5,
        NHNIFLS = 7,    // 251031 sebas add
        PHNIRLS = 11,   // 251031 sebas add
        RLSEDGE = 17,
        FLSEDGE = 18,
        PH = 19,
        NH = 21,
        NHFLSEDGE = 24, // 251031 sebas add
        NI = 33,
        PI = 34,
        VH = 35,
        SYNC_NHPI = 99

    }
    public interface IHomingMethods
    {
        int Homing(IMotionBase motionBase, AxisObject axis);
    }

    public class NHPI : IHomingMethods
    {
        public int Homing(IMotionBase motionBase, AxisObject axis)
        {
            int retVal = -1;
            double vel = 0.0;
            long timeOut = 60000;

            try
            {
                motionBase.SetLimitSWNegAct(axis, EnumEventActionType.ActionNONE);
                motionBase.SetLimitSWPosAct(axis, EnumEventActionType.ActionNONE);
                motionBase.ClearUserLimit(axis);
                retVal = motionBase.AmpFaultClear(axis);
                motionBase.SetFeedrate(axis, 1, 0);
                retVal = motionBase.WaitForAxisMotionDone(axis, 60000);

                if (retVal != 0)
                {
                    throw new MotionException($"Homming for Axis {axis.Label}: Error occurred while wait for previous motion", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                }
                #region // Clear state and position

                retVal = motionBase.DisableAxis(axis);

                if (retVal != 0)
                {
                    throw new MotionException($"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                }
                Thread.Sleep(500);
                retVal = motionBase.AmpFaultClear(axis);

                motionBase.SetZeroPosition(axis);



                if (axis.VerticalWithoutBreak.Value == true)
                {

                    retVal = motionBase.WaitForHomeSensor(axis);

                    if (retVal != 0)
                    { 
                        throw new MotionException($"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                    }

                    Thread.Sleep(1000);
                }

                retVal = motionBase.AmpFaultClear(axis);

                if (retVal != 0)
                {
                    throw new MotionException($"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                }

                retVal = motionBase.EnableAxis(axis);

                if (retVal != 0)
                {
                    throw new MotionException($"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                }

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
                if (retVal != 0)
                {
                    throw new MotionException($"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                }

                retVal = motionBase.AmpFaultClear(axis);

                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.SetPosition(axis, 0);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.AmpFaultClear(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {1}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.EnableAxis(axis);
                if (retVal != 0) throw new MotionException(
                                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                //delay.DelayFor(1000);
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

                retVal = motionBase.AmpFaultClear(axis);

                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                retVal = motionBase.SetSwitchAction(axis,
                     axis.Config.InputHome.Value, EnumEventActionType.ActionSTOP,
                    axis.Param.HomeInvert.Value == false ? EnumInputLevel.Inverted : EnumInputLevel.Normal);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.AmpFaultClear(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                vel = axis.Param.HommingSpeed.Value * 1;
                motionBase.VMove(axis, vel, EnumTrjType.Homming);
                retVal = motionBase.WaitForAxisEvent(axis, EnumAxisState.STOPPED, axis.Param.HomeDistLimit.Value);
                if (retVal != 0) throw new MotionException(
                $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                /////////
                motionBase.ClearUserLimit(axis.AxisIndex.Value);
                retVal = motionBase.AmpFaultClear(axis);

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
                // motionBase.SetNoneLimit(axis.AxisIndex);
                //delay.DelayFor(1000);
                Thread.Sleep(1000);
                retVal = motionBase.AmpFaultClear(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                Thread.Sleep(1000);

                retVal = motionBase.SetSwitchAction(axis,
                        axis.Config.InputIndex.Value, EnumEventActionType.ActionSTOP,
                        axis.Param.IndexInvert.Value == true ? EnumInputLevel.Normal : EnumInputLevel.Inverted);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                retVal = motionBase.AmpFaultClear(axis);

                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                //motionBase.SetNoneLimit(axis.AxisIndex);

                //retVal = motionBase.ConfigCapture(
                //    axis, axis.Config.InputMotor);

                vel = axis.Param.IndexSearchingSpeed.Value * 1;
                motionBase.VMove(axis, vel, EnumTrjType.Homming);
                retVal = motionBase.WaitForAxisEvent(axis, EnumAxisState.STOPPED, axis.Param.IndexDistLimit.Value);
                if (retVal != 0) throw new MotionException(
                $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                //double latchedPulse = 0.0;
                //retVal = motionBase.GetCaptureStatus(axis.AxisIndex, axis.Status);
                //if (axis.Status.MotionCaptureStatus.CaptureState == CaptureState.CaptureStateCAPTURED)
                //{
                //    latchedPulse = axis.Status.MotionCaptureStatus.LatchedValue;
                //    LoggerManager.Debug($
                //        $
                //            "Homming for Axis {axis.Label}: Latched pulse = {retVal}.",
                //            axis.Label, latchedPulse));
                //}
                //else
                //{
                //    throw new MotionException(
                //         $"Homming for Axis {axis.Label}: Error occurred. Index not detected.", axis.Label));
                //}
                ////
                //  SetNoneLimit(axis.AxisIndex);

                #endregion


                #region // Disable event action
                retVal = motionBase.SetSwitchAction(axis,
                    axis.Config.InputHome.Value, EnumEventActionType.ActionNONE, axis.Param.HomeInvert.Value == true ? EnumInputLevel.Inverted : EnumInputLevel.Normal);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                retVal = motionBase.AmpFaultClear(axis);

                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                #endregion

                #region // Set home offset
                //delay.DelayFor(100);
                //if (axis.Status.MotionCaptureStatus.CaptureState == CaptureState.CaptureStateCAPTURED)
                //{
                //    double latchedPos = axis.PtoD(latchedPulse);
                //    LoggerManager.Debug($$"Home to Index Distance = {axis.Label}", latchedPos));
                //    delay.DelayFor(500);

                //    retVal = motionBase.AbsMove(axis, latchedPos, axis.Param.FinalVelociy.Value, EnumTrjType.Homming);
                //    if (retVal != 0) throw new MotionException(
                //        $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                //    /////
                //    retVal = motionBase.WaitForAxisMotionDone(axis, timeOut);
                //    if (retVal != 0) throw new MotionException(
                //        $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                //    delay.DelayFor(500);
                //}


                #endregion
                retVal = motionBase.SetPosition(axis, axis.Param.HomeOffset.Value);
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
    public class PHNI : IHomingMethods
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
                motionBase.ClearUserLimit(axis);
                motionBase.SetFeedrate(axis, 1, 0);
                retVal = motionBase.AmpFaultClear(axis);
                retVal = motionBase.WaitForAxisMotionDone(axis, 60000);

                if (retVal != 0)
                {
                    throw new MotionException(
                        $"Homming for Axis {axis.Label}: Error occurred while wait for previous motion", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                }
                #region // Clear state and position

                retVal = motionBase.DisableAxis(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                //delay.DelayFor(500);
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

                retVal = motionBase.AmpFaultClear(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.SetPosition(axis, 0);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.AmpFaultClear(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.EnableAxis(axis);
                if (retVal != 0) throw new MotionException(
                                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                //delay.DelayFor(1000);
                Thread.Sleep(500);

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

                //delay.DelayFor(1000);
                Thread.Sleep(1000);
                //retVal = motionBase.SetPosition(axis, 0);

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

                retVal = motionBase.AmpFaultClear(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                retVal = motionBase.SetSwitchAction(axis,
                    axis.Config.InputHome.Value, EnumEventActionType.ActionSTOP,
                    axis.Param.HomeInvert.Value == false ? EnumInputLevel.Inverted : EnumInputLevel.Normal);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.AmpFaultClear(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                //delay.DelayFor(200);
                Thread.Sleep(200);

                vel = axis.Param.HommingSpeed.Value * -1;
                motionBase.VMove(axis, vel, EnumTrjType.Homming);
                retVal = motionBase.WaitForAxisEvent(axis, EnumAxisState.STOPPED, axis.Param.HomeDistLimit.Value);

                if (retVal != 0) throw new MotionException(
                $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                /////////
                motionBase.ClearUserLimit(axis.AxisIndex.Value);
                retVal = motionBase.AmpFaultClear(axis);
                motionBase.SetZeroPosition(axis);
                retVal = motionBase.WaitForAxisMotionDone(axis, 6000);

                #endregion

                #region // Homming shift
                if (axis.Param.HomeShift.Value != 0)
                {
                    retVal = motionBase.AmpFaultClear(axis);
                    motionBase.EnableAxis(axis);
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
                LoggerManager.Debug($"AlramReset Entry");
                //delay.DelayFor(1000);
                Thread.Sleep(500);

                retVal = motionBase.AmpFaultClear(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                Thread.Sleep(1000);

                retVal = motionBase.SetSwitchAction(axis,
                    axis.Config.InputIndex.Value, EnumEventActionType.ActionSTOP,
                    axis.Param.IndexInvert.Value == true ? EnumInputLevel.Normal : EnumInputLevel.Inverted);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                Thread.Sleep(1000);

                retVal = motionBase.AmpFaultClear(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                Thread.Sleep(1000);

                retVal = motionBase.EnableAxis(axis);
                if (retVal != 0) throw new MotionException(
                                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                //retVal = motionBase.ConfigCapture(axis, axis.Config.InputMotor);

                vel = axis.Param.IndexSearchingSpeed.Value * -1;
                motionBase.VMove(axis, vel, EnumTrjType.Homming);
                retVal = motionBase.WaitForAxisEvent(axis, EnumAxisState.STOPPED, axis.Param.IndexDistLimit.Value);
                if (retVal != 0) throw new MotionException(
                $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                //double latchedPulse = 0.0;
                //retVal = motionBase.GetCaptureStatus(axis.AxisIndex, axis.Status);
                //if (axis.Status.MotionCaptureStatus.CaptureState == CaptureState.CaptureStateCAPTURED)
                //{
                //    latchedPulse = axis.Status.MotionCaptureStatus.LatchedValue;


                //}
                //else
                //{
                //    throw new MotionException(
                //         $"Homming for Axis {axis.Label}: Error occurred. Index not detected.", axis.Label));
                //}

                //
                //SetNoneLimit(axis.AxisIndex);
                #endregion

                #region // Disable event action
                retVal = motionBase.SetSwitchAction(axis,
                    axis.Config.InputHome.Value, EnumEventActionType.ActionNONE, axis.Param.HomeInvert.Value == true ? EnumInputLevel.Inverted : EnumInputLevel.Normal);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                    retVal = motionBase.AmpFaultClear(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                #endregion

                #region // Set home offset
                //delay.DelayFor(100);
                Thread.Sleep(100);
                //if (axis.Status.MotionCaptureStatus.CaptureState == CaptureState.CaptureStateCAPTURED)
                //{
                //    double latchedPos = axis.PtoD(latchedPulse);

                //    retVal = motionBase.AbsMove(axis, latchedPos, axis.Param.HommingSpeed.Value, axis.Param.HommingAcceleration.Value, axis.Param.HommingDecceleration.Value);
                //    if (retVal != 0) throw new MotionException(
                //        $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                //    /////
                //    retVal = motionBase.WaitForAxisMotionDone(axis, timeOut);
                //    if (retVal != 0) throw new MotionException(
                //        $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                //}

                #endregion
                //motionBase.DisableCapture(axis);

                retVal = motionBase.SetPosition(axis, axis.Param.HomeOffset.Value);
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
                //motionBase.AlarmReset(axis);
                //motionBase.EnableAxis(axis);
                motionBase.SetLimitSWNegAct(axis, EnumEventActionType.ActionSTOP);
                motionBase.SetLimitSWPosAct(axis, EnumEventActionType.ActionSTOP);
                double pos = 0;
                motionBase.GetCommandPosition(axis, ref pos);
                axis.Status.RawPosition.Ref = pos;
            }

            return retVal;
        }
    }

    public class NH : IHomingMethods
    {
        public int Homing(IMotionBase motionBase, AxisObject axis)
        {
            int retVal;

            double vel = 0.0;
            //long timeOut = 60000;

            try
            {
                motionBase.SetFeedrate(axis, 1, 0);
                retVal = motionBase.AmpFaultClear(axis);
                retVal = motionBase.WaitForAxisMotionDone(axis, 60000);

                if (retVal != 0)
                {
                    throw new MotionException(
                        $"Homming for Axis {axis.Label}: Error occurred while wait for previous motion", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                }

                #region // Clear state and position
                motionBase.SetFeedrate(axis, 1, 0);

                retVal = motionBase.DisableAxis(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                //delay.DelayFor(500);
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

                    retVal = motionBase.AmpFaultClear(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.SetPosition(axis, 0);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                    retVal = motionBase.AmpFaultClear(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                retVal = motionBase.EnableAxis(axis);
                if (retVal != 0) throw new MotionException(
                                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                //delay.DelayFor(1000);
                Thread.Sleep(500);
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

                //retVal = motionBase.ConfigCapture(
                //    axis, EnumMotorDedicatedIn.MotorDedicatedInHOME);

                #endregion


                #region // Escape Home switch
                retVal = motionBase.SetSwitchAction(axis,
                     axis.Config.InputHome.Value,
                    EnumEventActionType.ActionNONE,
                    axis.Param.HomeInvert.Value == true ? EnumInputLevel.Inverted : EnumInputLevel.Normal);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                    retVal = motionBase.AmpFaultClear(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                retVal = motionBase.SetSwitchAction(axis,
                     axis.Config.InputHome.Value, EnumEventActionType.ActionSTOP,
                    axis.Param.HomeInvert.Value == false ? EnumInputLevel.Inverted : EnumInputLevel.Normal);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                    retVal = motionBase.AmpFaultClear(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                vel = axis.Param.HommingSpeed.Value * 1;
                motionBase.VMove(axis, vel, EnumTrjType.Homming);
                retVal = motionBase.WaitForAxisEvent(axis, EnumAxisState.STOPPED, axis.Param.HomeDistLimit.Value);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                /////////
                motionBase.ClearUserLimit(axis.AxisIndex.Value);
                retVal = motionBase.AmpFaultClear(axis);

                //double latchedPulse = 0.0;
                //retVal = motionBase.GetCaptureStatus(axis.AxisIndex, axis.Status);
                //if (axis.Status.MotionCaptureStatus.CaptureState == CaptureState.CaptureStateCAPTURED)
                //{
                //    latchedPulse = axis.Status.MotionCaptureStatus.LatchedValue;
                //}
                //else
                //{
                //    throw new MotionException(
                //         $"Homming for Axis {axis.Label}: Error occurred. Index not detected.", axis.Label));
                //}




                #region // Disable event action
                retVal = motionBase.SetSwitchAction(axis,
                    axis.Config.InputHome.Value, EnumEventActionType.ActionNONE, axis.Param.HomeInvert.Value == true ? EnumInputLevel.Inverted : EnumInputLevel.Normal);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                retVal = motionBase.AmpFaultClear(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                #endregion

                #region // Set home offset
                //delay.DelayFor(100);
                Thread.Sleep(100);

                //if (axis.Status.MotionCaptureStatus.CaptureState == CaptureState.CaptureStateCAPTURED)
                //{
                //    double latchedPos = axis.PtoD(latchedPulse);
                //    retVal = motionBase.AbsMove(axis, latchedPos, axis.Param.FinalVelociy.Value, EnumTrjType.Homming);
                //    if (retVal != 0) throw new MotionException(
                //    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                //    ///
                //    retVal = motionBase.WaitForAxisMotionDone(axis, timeOut);
                //    if (retVal != 0) throw new MotionException(
                //        $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));


                //}
                retVal = motionBase.SetPosition(axis, axis.Param.HomeOffset.Value);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                #endregion

                #endregion


                retVal = motionBase.GetAxisStatus(axis);
                
                if (axis.Status.State != EnumAxisState.IDLE)
                {
                    if(retVal != 0)
                    {
                        throw new MotionException(
                        $"GetAxisStatus() : Error occurred. Axis={axis.Label}, State={axis.Status.State}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                    }
                    else
                    {
                        // 혹시나 retVal의 값이 0인 경우
                        LoggerManager.Debug($"GetAxisStatus() : State={axis.Status.State}, retVal = {retVal}");
                        throw new MotionException(
                            $"GetAxisStatus() : Error occurred. Axis={axis.Label}, State={axis.Status.State}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(-1));
                    }
                }
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

    public class VH : IHomingMethods
    {
        public int Homing(IMotionBase motionBase, AxisObject axis)
        {
            int retVal = -1;

            //double vel = 0.0;
            //long timeOut = 60000;

            try
            {
                motionBase.SetFeedrate(axis, 1, 0);
                retVal = motionBase.AmpFaultClear(axis);
                retVal = motionBase.WaitForAxisMotionDone(axis, 60000);
                if (retVal != 0)
                {
                    throw new MotionException(
                        $"Homming for Axis {axis.Label}: Error occurred while wait for previous motion", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));
                }

                retVal = motionBase.DisableAxis(axis);
                if (retVal != 0) throw new MotionException(
                    $"Homming for Axis {axis.Label}: Error occurred. Return value = {retVal}", EnumReturnCodesConverter.EnumReturnCodeToEventCodeConvert(retVal));

                //delay.DelayFor(500);
                Thread.Sleep(500);

                retVal = motionBase.AmpFaultClear(axis);
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
