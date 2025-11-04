using System;

namespace HexagonJogControl
{
    using Autofac;
    using LogModule;
    using MarkAlignParamObject;
    using ProberInterfaces;
    using ProberInterfaces.Enum;
    using ProberInterfaces.Param;
    public class JogDistanceParam
    {
        public double HighXYDistance { get; set; }
        public double HighZDistance { get; set; }
        public double LowXYDistance { get; set; }
        public double LowZDistance { get; set; }
        public JogDistanceParam()
        {
            HighXYDistance = 1;
            HighZDistance = 1;
            LowXYDistance = 10;
            LowZDistance = 1;
        }
    }
    public class HexagonJogViewModel : IHexagonJogViewModel, IFactoryModule
    {
        public EnumAxisConstants AxisForMapping { get; set; }
        public bool SetMoveZOffsetEnable { get; set; }

        private ProbeAxisObject _XAxis;
        private ProbeAxisObject _YAxis;
        private ProbeAxisObject _ZAxis;
        private JogDistanceParam _JogDistanceParam;

        public HexagonJogViewModel()
        {
            if (this.MotionManager() != null)
            {
                _XAxis = this.MotionManager().GetAxis(EnumAxisConstants.X);
                _YAxis = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                _ZAxis = this.MotionManager().GetAxis(this.StageSupervisor().StageModuleState.PinViewAxis);
            }

            _JogDistanceParam = new JogDistanceParam();
        }

        #region ==> STEP MOVE        
        //==> Step Move Btn Hander Property & Function
        public void StickStepMove(JogParam parameter)
        {
            JogParam jogParam = parameter as JogParam;
            if (jogParam == null)
                return;

            double xyDistance = 1;
            double zDistance = 1;

            switch (jogParam.CurCameraType)
            {
                case EnumProberCam.WAFER_HIGH_CAM:
                case EnumProberCam.PIN_HIGH_CAM:
                case EnumProberCam.MAP_REF_CAM:
                    xyDistance = _JogDistanceParam.HighXYDistance * jogParam.Distance;
                    zDistance = _JogDistanceParam.HighZDistance * jogParam.Distance;
                    break;
                case EnumProberCam.WAFER_LOW_CAM:
                    xyDistance = _JogDistanceParam.LowXYDistance * jogParam.Distance;
                    zDistance = _JogDistanceParam.LowZDistance * jogParam.Distance;

                    if (jogParam.Direction == EnumJogDirection.RightUp ||
                        jogParam.Direction == EnumJogDirection.RightDown ||
                        jogParam.Direction == EnumJogDirection.LeftUp ||
                        jogParam.Direction == EnumJogDirection.LeftDown)
                    {
                        // Ratio 5.32 고려하여, 1픽셀 단위로 이동할 수 있도록 최소 이동 단위를 5로 사용
                        if (xyDistance == 10)
                        {
                            xyDistance = xyDistance / 2.0;
                        }
                    }

                    break;
                case EnumProberCam.PIN_LOW_CAM:
                    xyDistance = _JogDistanceParam.LowXYDistance * jogParam.Distance;
                    zDistance = _JogDistanceParam.LowZDistance * jogParam.Distance;
                    break;
            }

            LoggerManager.Debug($"HEXJOG STEP => DIRECTION : {jogParam.Direction}, CAM : {jogParam.CurCameraType}, X : {xyDistance}, Y : {xyDistance}, Z : {zDistance}");

            StepMove(
                jogParam.Direction,
                xyDistance,
                zDistance,
                jogParam.CurCameraType);
        }
        private void StepMove(EnumJogDirection direction, double xyRelPos, double zDistance, EnumProberCam camtype = EnumProberCam.UNDEFINED)
        {
            bool xAxisActive = false;
            bool yAxisActive = false;
            bool zAxisActive = false;
            double xAxisRelPos = 0;
            double yAxisRelPos = 0;
            double zAxisRelPos = 0;
            int reverseXDir = 1;
            int reverseYDir = 1;

            switch (direction)
            {
                case EnumJogDirection.Up:
                    yAxisActive = true;
                    yAxisRelPos = xyRelPos;
                    break;
                case EnumJogDirection.RightUp:
                    xAxisActive = true;
                    yAxisActive = true;
                    xAxisRelPos = xyRelPos;
                    yAxisRelPos = xyRelPos;
                    break;
                case EnumJogDirection.Right:
                    xAxisActive = true;
                    xAxisRelPos = xyRelPos;
                    break;
                case EnumJogDirection.RightDown:
                    xAxisActive = true;
                    yAxisActive = true;
                    xAxisRelPos = xyRelPos;
                    yAxisRelPos = xyRelPos * -1;
                    break;
                case EnumJogDirection.Down:
                    yAxisActive = true;
                    yAxisRelPos = xyRelPos * -1;
                    break;
                case EnumJogDirection.LeftDown:
                    xAxisActive = true;
                    yAxisActive = true;
                    xAxisRelPos = xyRelPos * -1;
                    yAxisRelPos = xyRelPos * -1;
                    break;
                case EnumJogDirection.Left:
                    xAxisActive = true;
                    xAxisRelPos = xyRelPos * -1;
                    break;
                case EnumJogDirection.LeftUp:
                    xAxisActive = true;
                    yAxisActive = true;
                    xAxisRelPos = xyRelPos * -1;
                    yAxisRelPos = xyRelPos;
                    break;
                case EnumJogDirection.ZBigUp:
                case EnumJogDirection.ZSmallUp:
                    zAxisActive = true;
                    zAxisRelPos = zDistance;
                    break;
                case EnumJogDirection.ZBigDown:
                case EnumJogDirection.ZSmallDown:
                    zAxisActive = true;
                    zAxisRelPos = zDistance * -1;
                    break;
            }

            if (this.CoordinateManager().GetReverseManualMoveX() == true)
            {
                reverseXDir = -1;
            }

            if (this.CoordinateManager().GetReverseManualMoveY() == true)
            {
                reverseYDir = -1;
            }

            xAxisRelPos = xAxisRelPos * reverseXDir;
            yAxisRelPos = yAxisRelPos * reverseYDir;

            if (xAxisActive && yAxisActive)
            {
                if (camtype == EnumProberCam.WAFER_LOW_CAM || camtype == EnumProberCam.WAFER_HIGH_CAM)
                {
                    XYAxisMove(-(xAxisRelPos), -(yAxisRelPos));
                }
                else if (camtype == EnumProberCam.PIN_LOW_CAM || camtype == EnumProberCam.PIN_HIGH_CAM)
                {
                    XYAxisMove(xAxisRelPos, yAxisRelPos);
                }
                else if (camtype == EnumProberCam.MAP_REF_CAM)
                {
                    this.StageSupervisor().StageModuleState.StageRelMove(xAxisRelPos, yAxisRelPos);
                }
            }
            else if (xAxisActive)
            {
                if (camtype == EnumProberCam.WAFER_LOW_CAM || camtype == EnumProberCam.WAFER_HIGH_CAM)
                {
                    SingleAxisMove(EnumAxisConstants.X, _XAxis, -(xAxisRelPos));
                }
                else if (camtype == EnumProberCam.PIN_LOW_CAM || camtype == EnumProberCam.PIN_HIGH_CAM)
                {
                    SingleAxisMove(EnumAxisConstants.X, _XAxis, xAxisRelPos);
                }
                else
                {
                    this.StageSupervisor().StageModuleState.StageRelMove(_XAxis, xAxisRelPos);
                }
            }
            else if (yAxisActive)
            {
                if (camtype == EnumProberCam.WAFER_LOW_CAM || camtype == EnumProberCam.WAFER_HIGH_CAM)
                {
                    SingleAxisMove(EnumAxisConstants.Y, _YAxis, -(yAxisRelPos));
                }
                else if (camtype == EnumProberCam.PIN_LOW_CAM || camtype == EnumProberCam.PIN_HIGH_CAM)
                {
                    SingleAxisMove(EnumAxisConstants.Y, _YAxis, yAxisRelPos);
                }
                else
                {
                    this.StageSupervisor().StageModuleState.StageRelMove(_YAxis, yAxisRelPos);
                }
            }
            else if (zAxisActive)
            {
                if (camtype == EnumProberCam.WAFER_LOW_CAM)
                {

                    if (this.StageSupervisor().StageMoveState == StageStateEnum.NC_PADVIEW)
                    {
                        if (this.MotionManager().GetAxis(this.NeedleCleaner().NCAxis.AxisType.Value).Status.AxisBusy == false)
                        {
                            this.StageSupervisor().StageModuleState.StageRelMove(this.MotionManager().GetAxis(this.NeedleCleaner().NCAxis.AxisType.Value), -(zAxisRelPos));
                        }
                    }
                    else
                    {
                        if (this.MotionManager().GetAxis(EnumAxisConstants.Z).Status.AxisBusy == false)
                        {
                            if (this.MotionManager().CheckSWLimit(EnumAxisConstants.Z, this.MotionManager().GetAxis(EnumAxisConstants.Z).Status.Position.Ref - zAxisRelPos) != ProberErrorCode.EventCodeEnum.NONE) return;
                            this.StageSupervisor().StageModuleState.StageRelMove(this.MotionManager().GetAxis(EnumAxisConstants.Z), -(zAxisRelPos));
                        }
                    }
                }
                else if (camtype == EnumProberCam.WAFER_HIGH_CAM)
                {

                    if (this.StageSupervisor().StageMoveState == StageStateEnum.NC_PADVIEW)
                    {
                        if (this.MotionManager().GetAxis(this.NeedleCleaner().NCAxis.AxisType.Value).Status.AxisBusy == false)
                        {
                            this.StageSupervisor().StageModuleState.StageRelMove(this.MotionManager().GetAxis(this.NeedleCleaner().NCAxis.AxisType.Value), -(zAxisRelPos));
                        }
                    }
                    else if (this.StageSupervisor().StageMoveState == StageStateEnum.MARK)
                    {
                        if (this.MotionManager().GetAxis((this.MarkAligner().MarkAlignParam_IParam as MarkAlignParam).FocusParam.FocusingAxis.Value).Status.AxisBusy == false)
                        {
                            this.StageSupervisor().StageModuleState.StageRelMove(this.MotionManager().GetAxis((this.MarkAligner().MarkAlignParam_IParam as MarkAlignParam).FocusParam.FocusingAxis.Value), -(zAxisRelPos));
                        }
                    }
                    else
                    {
                        if (this.MotionManager().GetAxis(EnumAxisConstants.Z).Status.AxisBusy == false)
                        {
                            if (this.MotionManager().CheckSWLimit(EnumAxisConstants.Z, this.MotionManager().GetAxis(EnumAxisConstants.Z).Status.Position.Ref - zAxisRelPos) != ProberErrorCode.EventCodeEnum.NONE) return;
                            this.StageSupervisor().StageModuleState.StageRelMove(this.MotionManager().GetAxis(EnumAxisConstants.Z), -(zAxisRelPos));
                        }
                    }
                }
                else if (camtype == EnumProberCam.MAP_REF_CAM)
                {
                    if (this.MotionManager().GetAxis(EnumAxisConstants.Z).Status.AxisBusy == false
                        & this.MotionManager().GetAxis(EnumAxisConstants.PZ).Status.AxisBusy == false)
                    {
                        if (AxisForMapping == EnumAxisConstants.Z)
                            this.StageSupervisor().StageModuleState.StageRelMove(this.MotionManager().GetAxis(EnumAxisConstants.Z), zAxisRelPos);
                        else if (AxisForMapping == EnumAxisConstants.PZ)
                            this.StageSupervisor().StageModuleState.StageRelMove(this.MotionManager().GetAxis(EnumAxisConstants.PZ), zAxisRelPos);

                    }
                }
                else
                {
                    if (this.MotionManager().GetAxis(EnumAxisConstants.Z).Status.AxisBusy == false)
                    {
                        this.StageSupervisor().StageModuleState.StageRelMove(this.MotionManager().GetAxis(this.StageSupervisor().StageModuleState.PinViewAxis), zAxisRelPos);
                    }
                }
            }
        }
        private void XYAxisMove(double xRelPos, double yRelPos)
        {
            if (this.MotionManager().CheckSWLimit(EnumAxisConstants.X, this.MotionManager().GetAxis(EnumAxisConstants.X).Status.Position.Ref + xRelPos) != ProberErrorCode.EventCodeEnum.NONE)
            {
                return;
            }
            if (this.MotionManager().CheckSWLimit(EnumAxisConstants.Y, this.MotionManager().GetAxis(EnumAxisConstants.Y).Status.Position.Ref + yRelPos) != ProberErrorCode.EventCodeEnum.NONE)
            {
                return;
            }
            this.StageSupervisor().StageModuleState.StageRelMove(xRelPos, yRelPos);
        }
        private void SingleAxisMove(EnumAxisConstants axisType, ProbeAxisObject axis, double relPos)
        {
            if (this.MotionManager().CheckSWLimit(axisType, this.MotionManager().GetAxis(axisType).Status.Position.Ref + relPos) != ProberErrorCode.EventCodeEnum.NONE)
            {
                return;
            }
            this.StageSupervisor().StageModuleState.StageRelMove(axis, relPos);
        }
        #endregion

        #region ==> INDEX MOVE
        //==> Index Move Btn Hander Property & Function
        public void StickIndexMove(JogParam parameter, bool setzoffsetenable)
        {
            try
            {
                JogParam jogParam = parameter as JogParam;

                LoggerManager.Debug($"HEXJOG INDEX : {jogParam.Direction}");

                IndexMove(jogParam.Direction, setzoffsetenable);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void IndexMove(EnumJogDirection direction, bool setzoffsetenable)
        {
            long xInc = 0;
            long yInc = 0;
            double dieSizeX = 0.0;
            double dieSizeY = 0.0;
            int reverseXDir = 1;
            int reverseYDir = 1;

            MachineIndex mcoord = new MachineIndex();
            PinCoordinate pinCoordinate = new PinCoordinate();
            ICamera cam;
            CatCoordinates catcoord = new CatCoordinates();

            switch (direction)
            {
                case EnumJogDirection.Up:
                    xInc = 0;
                    yInc = 1;
                    break;
                case EnumJogDirection.RightUp:
                    xInc = 1;
                    yInc = 1;
                    break;
                case EnumJogDirection.Right:
                    xInc = 1;
                    yInc = 0;
                    break;
                case EnumJogDirection.RightDown:
                    xInc = 1;
                    yInc = -1;
                    break;
                case EnumJogDirection.Down:
                    xInc = 0;
                    yInc = -1;
                    break;
                case EnumJogDirection.LeftDown:
                    xInc = -1;
                    yInc = -1;
                    break;
                case EnumJogDirection.Left:
                    xInc = -1;
                    yInc = 0;
                    break;
                case EnumJogDirection.LeftUp:
                    xInc = -1;
                    yInc = 1;
                    break;
            }


            if (this.CoordinateManager().GetReverseManualMoveX() == true)
            {
                reverseXDir = -1;
            }

            if (this.CoordinateManager().GetReverseManualMoveY() == true)
            {
                reverseYDir = -1;
            }


            xInc = long.Parse(xInc.ToString()) * reverseXDir;
            yInc = long.Parse(yInc.ToString()) * reverseYDir;

            dieSizeX = this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Width.Value;
            dieSizeY = this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Height.Value;

            if (xInc != 0 || yInc != 0)
            {
                if (this.StageSupervisor().StageModuleState.GetState() == StageStateEnum.WAFERHIGHVIEW)
                {
                    // 현재 좌표
                    cam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);

                    if (this.MotionManager().CheckSWLimit(EnumAxisConstants.X, this.MotionManager().GetAxis(EnumAxisConstants.X).Status.Position.Ref - (xInc * dieSizeX)) != ProberErrorCode.EventCodeEnum.NONE) return;
                    if (this.MotionManager().CheckSWLimit(EnumAxisConstants.Y, this.MotionManager().GetAxis(EnumAxisConstants.Y).Status.Position.Ref - (yInc * dieSizeY)) != ProberErrorCode.EventCodeEnum.NONE) return;

                    mcoord = cam.GetCurCoordMachineIndex();

                    if (setzoffsetenable || this.SetMoveZOffsetEnable == true)
                    {
                        this.CoordinateManager().CalculateOffsetFromCurrentZ(cam.GetChannelType());

                        //double OffsetX = -(xInc * dieSizeX);
                        //double OffsetY = -(yInc * dieSizeY);

                        //var curpos = cam.GetCurCoordPos();

                        //double get_zpos = this.WaferAligner().GetHeightValueAddZOffset(curpos.X.Value + OffsetX, curpos.Y.Value + OffsetY, zoffset, true);
                        double zoffset = this.StageSupervisor().WaferObject.GetSubsInfo().MoveZOffset;
                        double get_zpos = 0;
                        this.WaferAligner().GetHeightValueAddZOffsetFromDutIndex(cam.GetChannelType(), mcoord.XIndex + xInc, mcoord.YIndex + yInc, zoffset, out get_zpos);
                        this.StageSupervisor().StageModuleState.WaferHighViewIndexMove(mcoord.XIndex + xInc, mcoord.YIndex + yInc, get_zpos, true);
                    }
                    else
                    {
                        //inspection view인경우에 zoffset 반영하지 않을 것인가? 할 것인가?
                        this.StageSupervisor().StageModuleState.WaferHighViewIndexMove(mcoord.XIndex + xInc, mcoord.YIndex + yInc);
                    }
                }
                else if (this.StageSupervisor().StageModuleState.GetState() == StageStateEnum.WAFERLOWVIEW)
                {
                    // 현재 좌표
                    cam = this.VisionManager().GetCam(EnumProberCam.WAFER_LOW_CAM);

                    if (this.MotionManager().CheckSWLimit(EnumAxisConstants.X, this.MotionManager().GetAxis(EnumAxisConstants.X).Status.Position.Ref - (xInc * dieSizeX)) != ProberErrorCode.EventCodeEnum.NONE) return;
                    if (this.MotionManager().CheckSWLimit(EnumAxisConstants.Y, this.MotionManager().GetAxis(EnumAxisConstants.Y).Status.Position.Ref - (yInc * dieSizeY)) != ProberErrorCode.EventCodeEnum.NONE) return;

                    mcoord = cam.GetCurCoordMachineIndex();

                    if (setzoffsetenable || this.SetMoveZOffsetEnable == true)
                    {
                        this.CoordinateManager().CalculateOffsetFromCurrentZ(cam.GetChannelType());

                        //double OffsetX = -(xInc * dieSizeX);
                        //double OffsetY = -(yInc * dieSizeY);

                        //var curpos = cam.GetCurCoordPos();

                        //double get_zpos = this.WaferAligner().GetHeightValueAddZOffset(curpos.X.Value + OffsetX, curpos.Y.Value + OffsetY, zoffset, true);
                        double zoffset = this.StageSupervisor().WaferObject.GetSubsInfo().MoveZOffset;
                        double get_zpos = 0;
                        this.WaferAligner().GetHeightValueAddZOffsetFromDutIndex(cam.GetChannelType(), mcoord.XIndex + xInc, mcoord.YIndex + yInc, zoffset, out get_zpos);
                        this.StageSupervisor().StageModuleState.WaferLowViewIndexMove(mcoord.XIndex + xInc, mcoord.YIndex + yInc, get_zpos, true);
                    }
                    else
                    {
                        this.StageSupervisor().StageModuleState.WaferLowViewIndexMove(mcoord.XIndex + xInc, mcoord.YIndex + yInc);
                    }
                }
                else
                {
                    if (this.StageSupervisor().StageModuleState.GetState() == StageStateEnum.PINHIGHVIEW)
                    {
                        cam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);

                        if (this.MotionManager().CheckSWLimit(EnumAxisConstants.X, this.MotionManager().GetAxis(EnumAxisConstants.X).Status.Position.Ref + (xInc * dieSizeX)) != ProberErrorCode.EventCodeEnum.NONE) return;
                        if (this.MotionManager().CheckSWLimit(EnumAxisConstants.Y, this.MotionManager().GetAxis(EnumAxisConstants.Y).Status.Position.Ref + (yInc * dieSizeY)) != ProberErrorCode.EventCodeEnum.NONE) return;
                    }
                    else if (this.StageSupervisor().StageModuleState.GetState() == StageStateEnum.PINLOWVIEW)
                    {
                        cam = this.VisionManager().GetCam(EnumProberCam.PIN_LOW_CAM);

                        if (this.MotionManager().CheckSWLimit(EnumAxisConstants.X, this.MotionManager().GetAxis(EnumAxisConstants.X).Status.Position.Ref + (xInc * dieSizeX)) != ProberErrorCode.EventCodeEnum.NONE) return;
                        if (this.MotionManager().CheckSWLimit(EnumAxisConstants.Y, this.MotionManager().GetAxis(EnumAxisConstants.Y).Status.Position.Ref + (yInc * dieSizeY)) != ProberErrorCode.EventCodeEnum.NONE) return;
                    }
                    else
                    {
                        return;
                    }
                    // TODO: FLIP 검토 할것 
                    this.StageSupervisor().StageModuleState.StageRelMove(xInc * dieSizeX, yInc * dieSizeY);
                }
            }
        }
        #endregion

        #region ==> SCAN MOVE
        //==> Scan Move Btn 'Move Event' Hander Property & Function
        public void StickScanMove(object parameter)
        {
            JogParam scanJob = (JogParam)parameter;

            double distanceRatio = 0;
            switch (scanJob.CurCameraType)
            {
                case EnumProberCam.WAFER_LOW_CAM:
                    distanceRatio = -32;
                    break;
                case EnumProberCam.PIN_LOW_CAM:
                    distanceRatio = 32;
                    break;
                case EnumProberCam.WAFER_HIGH_CAM:
                    distanceRatio = -64;
                    break;
                case EnumProberCam.PIN_HIGH_CAM:
                    distanceRatio = 64;
                    break;
                case EnumProberCam.MAP_REF_CAM:
                    distanceRatio = 64;
                    break;
                default:
                    distanceRatio = 0;
                    break;
            }

            if (distanceRatio == 0)
                return;

            LoggerManager.Debug($"HEXJOG SCAN => DIRECTION : {scanJob.Direction}, distance : {scanJob.Distance}");

            ScanMove(
                scanJob.Direction,
                (_XAxis.Param.Speed.Value / distanceRatio) * scanJob.Distance,
                (_YAxis.Param.Speed.Value / distanceRatio) * scanJob.Distance);
        }
        private void ScanMove(EnumJogDirection direction, double xVelStand, double yVelStand)
        {
            bool xAxisActive = false;
            bool yAxisActive = false;
            double xAxisVel = 0;
            double yAxisVel = 0;
            switch (direction)
            {
                case EnumJogDirection.Up:
                    yAxisActive = true;
                    yAxisVel = yVelStand;
                    break;
                case EnumJogDirection.RightUp:
                    xAxisActive = true;
                    yAxisActive = true;
                    xAxisVel = xVelStand;
                    yAxisVel = yVelStand;
                    break;
                case EnumJogDirection.Right:
                    xAxisActive = true;
                    xAxisVel = xVelStand;
                    break;
                case EnumJogDirection.RightDown:
                    xAxisActive = true;
                    yAxisActive = true;
                    xAxisVel = xVelStand;
                    yAxisVel = -yVelStand;
                    break;
                case EnumJogDirection.Down:
                    yAxisActive = true;
                    yAxisVel = -yVelStand;
                    break;
                case EnumJogDirection.LeftDown:
                    xAxisActive = true;
                    yAxisActive = true;
                    xAxisVel = -xVelStand;
                    yAxisVel = -yVelStand;
                    break;
                case EnumJogDirection.Left:
                    xAxisActive = true;
                    xAxisVel = -xVelStand;
                    break;
                case EnumJogDirection.LeftUp:
                    xAxisActive = true;
                    yAxisActive = true;
                    xAxisVel = -xVelStand;
                    yAxisVel = yVelStand;
                    break;
            }

            if (xAxisActive)
                this.MotionManager().ScanJogMove(xAxisVel, yAxisVel, EnumTrjType.Normal);
            if (yAxisActive)
                this.MotionManager().ScanJogMove(xAxisVel, yAxisVel, EnumTrjType.Normal);
        }
        //==> Scan Move Btn 'End Event' Hander Property & Function
        public void StickScanMoveEnd()
        {
            LoggerManager.Debug($"HEXJOG SCAN STOP");
            this.MotionManager().ScanJogMove(0, 0, EnumTrjType.Normal);
        }
        #endregion

        public void SetContainer(IContainer container)
        {
            return;
        }
    }
}