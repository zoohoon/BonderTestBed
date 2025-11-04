using LogModule;
using ProberInterfaces;
using ProberInterfaces.Enum;
using System;
using System.Threading.Tasks;
////using ProberInterfaces.ThreadSync;

namespace ChuckTiltHexagonJog
{
    public class ChuckTiltHexagonJogViewModel : IFactoryModule
    {
        private delegate void MoveDelegate();
        private IStageSupervisor _StageSupervisor { get; set; }

        private IMotionManager _MotionManager { get; set; }

        public double IndexMoveX { get; set; }//==> X 축 Index Move 거리
        public double IndexMoveY { get; set; }//==> Y 축 Index Move 거리
        public double StepMoveX { get; set; }//==> X 축 Step Move 거리
        public double StepMoveY { get; set; }//==> Y 축 Step Move 거리
        public double StepMoveZSmall { get; set; }//==> Z 축 Step Move 거리
        public double StepMoveZBig { get; set; }//==> Z 축 Step Move 거리

        private bool _IsScanMoveInOperation;//==> Scan Move 가 동작 상태 여부(true : Scan Move 가 계속 진행되고 있음, false : Scan Move가 동작하고 있지 않음)

        private ProbeAxisObject _XAxis;
        private ProbeAxisObject _YAxis;
        private ProbeAxisObject _ZAxis;

        public ChuckTiltHexagonJogViewModel(
            double indexMoveX = 50,
            double indexMoveY = 50,
            double stepMoveX = 5,
            double stepMoveY = 5,
            double stepMoveBigZ = 10,
            double stepMoveSmallZ = 1)
        {
            try
            {
                _StageSupervisor = this.StageSupervisor();
                _MotionManager = this.MotionManager();

                IndexMoveX = indexMoveX;
                IndexMoveY = indexMoveY;
                StepMoveX = stepMoveX;
                StepMoveY = stepMoveY;
                StepMoveZBig = stepMoveBigZ;
                StepMoveZSmall = stepMoveSmallZ;

                _XAxis = _MotionManager.GetAxis(EnumAxisConstants.X);
                _YAxis = _MotionManager.GetAxis(EnumAxisConstants.Y);
                //_ZAxis = _MotionManager.GetAxis(EnumAxisConstants.Z);
                _ZAxis = _MotionManager.GetAxis(_StageSupervisor.StageModuleState.PinViewAxis);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }

        //==> Index Move Btn Hander Property & Function
        #region ==> StickJogIndexMoveCommand
        //private RelayCommand<object> _StickJogIndexMoveCommand;
        //public ICommand StickJogIndexMoveCommand
        //{
        //    get
        //    {
        //        if (null == _StickJogIndexMoveCommand) _StickJogIndexMoveCommand = new RelayCommand<object>(StickJogIndexMoveFunc);
        //        return _StickJogIndexMoveCommand;
        //    }
        //}
        public void StickJogIndexMoveFunc(object parameter)
        {
            try
            {
                if (_IsScanMoveInOperation)
                    return;
                JogParam jogParam = parameter as JogParam;

                //EnumJogDirection jogDirection = (EnumJogDirection)parameter;

                //using (Locker locker = new Locker(lockObject))
                //{
                lock (lockObject)
                {
                    JogIndexMove(jogParam.Direction);
                    //JogStepRelMove(jogParam.Direction, IndexMoveX, IndexMoveY, 0, 0, jogParam.CurCameraType);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        //==> Step Move Btn Hander Property & Function
        #region ==> StickJogStepMoveCommand
        //private RelayCommand<object> _StickJogStepMoveCommand;
        //public ICommand StickJogStepMoveCommand
        //{
        //    get
        //    {
        //        if (null == _StickJogStepMoveCommand) _StickJogStepMoveCommand = new RelayCommand<object>(StickJogStepMoveFunc);
        //        return _StickJogStepMoveCommand;
        //    }
        //}
        public void StickJogStepMoveFunc(object parameter)
        {
            try
            {
                if (_IsScanMoveInOperation)
                    return;

                JogParam jogParam = parameter as JogParam;
                if (jogParam == null)
                    return;

                double stageMoveX = 1;
                double stageMoveY = 1;
                double stageMoveZBig = StepMoveZBig;
                double stageMoveZSmall = StepMoveZSmall;
                if (jogParam.Distance == 1)
                {
                    switch (jogParam.CurCameraType)
                    {
                        case EnumProberCam.WAFER_HIGH_CAM:
                        case EnumProberCam.PIN_HIGH_CAM:
                        case EnumProberCam.MAP_REF_CAM:
                            stageMoveX = 1;
                            stageMoveY = 1;
                            break;
                        case EnumProberCam.WAFER_LOW_CAM:
                        case EnumProberCam.PIN_LOW_CAM:
                            stageMoveX = 1;
                            stageMoveY = 1;
                            break;
                    }
                }
                else
                {
                    double lowRatio = 1.5;
                    switch (jogParam.CurCameraType)
                    {
                        case EnumProberCam.WAFER_HIGH_CAM:
                        case EnumProberCam.PIN_HIGH_CAM:
                        case EnumProberCam.MAP_REF_CAM:
                            stageMoveX = StepMoveX * jogParam.Distance;
                            stageMoveY = StepMoveY * jogParam.Distance;
                            break;
                        case EnumProberCam.WAFER_LOW_CAM:
                        case EnumProberCam.PIN_LOW_CAM:
                            stageMoveX = StepMoveX * jogParam.Distance * lowRatio;
                            stageMoveY = StepMoveY * jogParam.Distance * lowRatio;
                            break;
                    }
                }

                //using (Locker locker = new Locker(lockObject))
                //{
                lock (lockObject)
                {
                    JogStepRelMove(
                        jogParam.Direction,
                        stageMoveX,
                        stageMoveY,
                        stageMoveZBig,
                        stageMoveZSmall,
                        jogParam.CurCameraType);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        private void JogIndexMove(EnumJogDirection direction)
        {
            try
            {
                if (this.StageSupervisor().CheckAxisBusy() == true || this.StageSupervisor().CheckAxisIdle() == false)
                    return;

                long xInc = 0;
                long yInc = 0;
                double dieSizeX = 0.0;
                double dieSizeY = 0.0;

                MachineIndex mcoord = new MachineIndex();

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


                if (xInc != 0 || yInc != 0)
                {
                    if (this.StageSupervisor().StageModuleState.GetState() == StageStateEnum.WAFERHIGHVIEW)
                    {
                        // 현재 좌표
                        ICamera cam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);
                        mcoord = cam.GetCurCoordMachineIndex();

                        this.StageSupervisor().StageModuleState.WaferHighViewIndexMove(mcoord.XIndex + xInc, mcoord.YIndex + yInc);
                    }
                    else if (this.StageSupervisor().StageModuleState.GetState() == StageStateEnum.WAFERLOWVIEW)
                    {
                        // 현재 좌표
                        ICamera cam = this.VisionManager().GetCam(EnumProberCam.WAFER_LOW_CAM);
                        mcoord = cam.GetCurCoordMachineIndex();

                        this.StageSupervisor().StageModuleState.WaferLowViewIndexMove(mcoord.XIndex + xInc, mcoord.YIndex + yInc);
                    }
                    else
                    {
                        dieSizeX = this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Width.Value;
                        dieSizeY = this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Height.Value;
                        _StageSupervisor.StageModuleState.StageRelMove(xInc * dieSizeX, yInc * dieSizeY);
                    }
                }
            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. HexagonJogViewModel - JogIndexMove() : Error occured.");
            }
        }

        private void JogStepRelMove(EnumJogDirection direction, double xRelPos, double yRelPos, double zRelPosBig, double zRelPosSmall, EnumProberCam camtype = EnumProberCam.UNDEFINED)
        {
            try
            {
                if (this.StageSupervisor().CheckAxisBusy() == true || this.StageSupervisor().CheckAxisIdle() == false)
                    return;
                //if (this.MotionManager().StageAxesBusy())
                //    return;

                bool xAxisActive = false;
                bool yAxisActive = false;
                bool zAxisActive = false;
                double xAxisRelPos = 0;
                double yAxisRelPos = 0;
                double zAxisRelPos = 0;
                switch (direction)
                {
                    case EnumJogDirection.Up:
                        yAxisActive = true;
                        yAxisRelPos = yRelPos;
                        break;
                    case EnumJogDirection.RightUp:
                        xAxisActive = true;
                        yAxisActive = true;
                        xAxisRelPos = xRelPos;
                        yAxisRelPos = yRelPos;
                        break;
                    case EnumJogDirection.Right:
                        xAxisActive = true;
                        xAxisRelPos = xRelPos;
                        break;
                    case EnumJogDirection.RightDown:
                        xAxisActive = true;
                        yAxisActive = true;
                        xAxisRelPos = xRelPos;
                        yAxisRelPos = -yRelPos;
                        break;
                    case EnumJogDirection.Down:
                        yAxisActive = true;
                        yAxisRelPos = -yRelPos;
                        break;
                    case EnumJogDirection.LeftDown:
                        xAxisActive = true;
                        yAxisActive = true;
                        xAxisRelPos = -xRelPos;
                        yAxisRelPos = -yRelPos;
                        break;
                    case EnumJogDirection.Left:
                        xAxisActive = true;
                        xAxisRelPos = -xRelPos;
                        break;
                    case EnumJogDirection.LeftUp:
                        xAxisActive = true;
                        yAxisActive = true;
                        xAxisRelPos = -xRelPos;
                        yAxisRelPos = yRelPos;
                        break;
                    case EnumJogDirection.ZBigUp:
                        zAxisActive = true;
                        zAxisRelPos = zRelPosBig;
                        break;
                    case EnumJogDirection.ZBigDown:
                        zAxisActive = true;
                        zAxisRelPos = zRelPosBig * -1;
                        break;
                    case EnumJogDirection.ZSmallUp:
                        zAxisActive = true;
                        zAxisRelPos = zRelPosSmall;
                        break;
                    case EnumJogDirection.ZSmallDown:
                        zAxisActive = true;
                        zAxisRelPos = zRelPosSmall * -1;
                        break;
                }
                //if (this.MotionManager().GetAxis(EnumAxisConstants.X).Status.AxisBusy == false
                //    & this.MotionManager().GetAxis(EnumAxisConstants.Y).Status.AxisBusy == false
                //    & this.MotionManager().GetAxis(EnumAxisConstants.C).Status.AxisBusy == false
                //    & this.MotionManager().GetAxis(EnumAxisConstants.PZ).Status.AxisBusy == false
                //    & this.MotionManager().GetAxis(EnumAxisConstants.Z).Status.AxisBusy == false)
                //{

                if (xAxisActive && yAxisActive)
                {
                    if (camtype == EnumProberCam.WAFER_HIGH_CAM || camtype == EnumProberCam.WAFER_LOW_CAM)
                    {
                        _StageSupervisor.StageModuleState.StageRelMove(-(xAxisRelPos), -(yAxisRelPos));
                    }
                    else if (camtype == EnumProberCam.PIN_HIGH_CAM || camtype == EnumProberCam.PIN_LOW_CAM)
                    {
                        _StageSupervisor.StageModuleState.StageRelMove(xAxisRelPos, yAxisRelPos);
                    }
                    else if (camtype == EnumProberCam.MAP_REF_CAM)
                    {
                        _StageSupervisor.StageModuleState.StageRelMove(xAxisRelPos, yAxisRelPos);
                    }
                }
                else if (xAxisActive)
                {
                    if (camtype == EnumProberCam.WAFER_HIGH_CAM || camtype == EnumProberCam.WAFER_LOW_CAM)
                    {
                        _StageSupervisor.StageModuleState.StageRelMove(_XAxis, -(xAxisRelPos));
                    }
                    else if (camtype == EnumProberCam.PIN_HIGH_CAM || camtype == EnumProberCam.PIN_LOW_CAM)
                    {
                        _StageSupervisor.StageModuleState.StageRelMove(_XAxis, xAxisRelPos);
                    }
                    else
                    {
                        _StageSupervisor.StageModuleState.StageRelMove(_XAxis, xAxisRelPos);
                    }
                }
                else if (yAxisActive)
                {
                    if (camtype == EnumProberCam.WAFER_HIGH_CAM || camtype == EnumProberCam.WAFER_LOW_CAM)
                    {
                        _StageSupervisor.StageModuleState.StageRelMove(_YAxis, -(yAxisRelPos));
                    }
                    else if (camtype == EnumProberCam.PIN_HIGH_CAM || camtype == EnumProberCam.PIN_LOW_CAM)
                    {
                        _StageSupervisor.StageModuleState.StageRelMove(_YAxis, yAxisRelPos);
                    }
                    else
                    {
                        _StageSupervisor.StageModuleState.StageRelMove(_YAxis, yAxisRelPos);
                    }
                }
                else if (zAxisActive)
                {
                    if (camtype == EnumProberCam.WAFER_HIGH_CAM || camtype == EnumProberCam.WAFER_LOW_CAM)
                    {

                        if (this.StageSupervisor().StageMoveState == StageStateEnum.NC_PADVIEW)
                        {
                            if (this.MotionManager().GetAxis(this.NeedleCleaner().NCAxis.AxisType.Value).Status.AxisBusy == false)
                            {
                                _StageSupervisor.StageModuleState.StageRelMove(this.MotionManager().GetAxis(this.NeedleCleaner().NCAxis.AxisType.Value), -(zAxisRelPos));
                            }
                        }
                        else
                        {
                            if (this.MotionManager().GetAxis(EnumAxisConstants.Z).Status.AxisBusy == false)
                            {
                                _StageSupervisor.StageModuleState.StageRelMove(this.MotionManager().GetAxis(EnumAxisConstants.Z), -(zAxisRelPos));
                            }
                        }
                    }
                    else
                    {
                        if (this.MotionManager().GetAxis(EnumAxisConstants.Z).Status.AxisBusy == false)
                        {
                            _StageSupervisor.StageModuleState.StageRelMove(this.MotionManager().GetAxis(_StageSupervisor.StageModuleState.PinViewAxis), zAxisRelPos);
                        }
                    }
                }
                //}
                //else
                //{

                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        //==> Scan Move Btn 'Move Event' Hander Property & Function
        #region ==> StickJogScanMoveChangeCommand
        //private LockKey lockObject = new LockKey("Chuck tilt jog");//==> Scan Move 동기화 처리
        private object lockObject = new object();
        private Task scanMoveTask;//==> scan move task 참조

        //private RelayCommand<object> _StickJogScanMoveChangeCommand;
        //public ICommand StickJogScanMoveChangeCommand
        //{
        //    get
        //    {
        //        if (null == _StickJogScanMoveChangeCommand) _StickJogScanMoveChangeCommand = new RelayCommand<object>(StickJogScanMoveChangeFunc);
        //        return _StickJogScanMoveChangeCommand;
        //    }
        //}
        public void StickJogScanMoveChangeFunc(object parameter)
        {
            try
            {
                if (scanMoveTask != null && scanMoveTask.IsCompleted == false)
                    return;

                JogParam jogParam = (JogParam)parameter;
                if (jogParam.Distance == 0)
                    return;

                double distanceRatio = 0;
                switch (jogParam.CurCameraType)
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

                _IsScanMoveInOperation = true;
                scanMoveTask = Task.Run(() =>
                {
                    //using (Locker locker = new Locker(lockObject))
                    //{
                    lock (lockObject)
                    {
                        if (_IsScanMoveInOperation)
                        {
                            JogRelScanMove(
                                jogParam.Direction,
                                (_XAxis.Param.Speed.Value / distanceRatio) * jogParam.Distance,
                                (_YAxis.Param.Speed.Value / distanceRatio) * jogParam.Distance,
                                jogParam.CurCameraType);
                        }
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        //==> Scan Move Btn 'End Event' Hander Property & Function
        #region ==> StickJogScanMoveEndCommand
        //private RelayCommand<object> _StickJogScanMoveEndCommand;
        //public ICommand StickJogScanMoveEndCommand
        //{
        //    get
        //    {
        //        if (null == _StickJogScanMoveEndCommand) _StickJogScanMoveEndCommand = new RelayCommand<object>(StickJogScanMoveEndFunc);
        //        return _StickJogScanMoveEndCommand;
        //    }
        //}
        public void StickJogScanMoveEndFunc(object parameter)
        {
            try
            {
                _IsScanMoveInOperation = false;

                Task.Run(() =>
                {
                    if (scanMoveTask != null)
                        scanMoveTask.Wait();
                    scanMoveTask = null;

                    JogParam jogParam = (JogParam)parameter;
                    _StageSupervisor.StageModuleState.StageMoveStop(_XAxis);
                    _StageSupervisor.StageModuleState.StageMoveStop(_YAxis);
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        private void JogRelScanMove(EnumJogDirection direction, double xVelStand, double yVelStand, EnumProberCam camtype = EnumProberCam.UNDEFINED)
        {
            try
            {
                if (this.StageSupervisor().CheckAxisBusy() == true || this.StageSupervisor().CheckAxisIdle() == false)
                    return;

                //if (this.MotionManager().StageAxesBusy())
                //    return;

                _StageSupervisor.StageModuleState.StageMoveStop(_XAxis);
                _StageSupervisor.StageModuleState.StageMoveStop(_YAxis);

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
                {
                    //if (camtype == EnumProberCam.WAFER_HIGH_CAM || camtype == EnumProberCam.WAFER_LOW_CAM)
                    //    _StageSupervisor.StageModuleState.StageVMove(_XAxis, -(xAxisVel), EnumTrjType.Normal);
                    //else if (camtype == EnumProberCam.PIN_HIGH_CAM || camtype == EnumProberCam.PIN_LOW_CAM)
                    //    _StageSupervisor.StageModuleState.StageVMove(_XAxis, xAxisVel, EnumTrjType.Normal);
                    //else
                    _StageSupervisor.StageModuleState.StageVMove(_XAxis, xAxisVel, EnumTrjType.Normal);
                }

                if (yAxisActive)
                {
                    //if (camtype == EnumProberCam.WAFER_HIGH_CAM || camtype == EnumProberCam.WAFER_LOW_CAM)
                    //    _StageSupervisor.StageModuleState.StageVMove(_YAxis, -(yAxisVel), EnumTrjType.Normal);
                    //else if (camtype == EnumProberCam.PIN_HIGH_CAM || camtype == EnumProberCam.PIN_LOW_CAM)
                    //    _StageSupervisor.StageModuleState.StageVMove(_YAxis, yAxisVel, EnumTrjType.Normal);
                    //else
                    _StageSupervisor.StageModuleState.StageVMove(_YAxis, yAxisVel, EnumTrjType.Normal);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
