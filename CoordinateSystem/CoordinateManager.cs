using System;
using System.ComponentModel;
using ProberInterfaces;
using ProberInterfaces.Param;
using System.Windows;
using SubstrateObjects;
using ProberErrorCode;

using LogModule;
using System.Threading;
using System.Runtime.CompilerServices;
using System.ServiceModel;

namespace CoordinateSystem
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class CoordinateManager : ICoordinateManager, INotifyPropertyChanged, IModule
    { 
        public OverlayUpdate OverlayUpdateDelegate { get; set; }

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        public bool Initialized { get; set; } = false;


        //private IParam _SysParam;
        //[ParamIgnore]
        //public IParam SysParam
        //{
        //    get { return _SysParam; }
        //    set
        //    {
        //        if (value != _SysParam)
        //        {
        //            _SysParam = value;
        //            NotifyPropertyChanged("SysParam");
        //        }
        //    }
        //}
        private StageCoords _StageCoord;
        public StageCoords StageCoord
        {
            get { return _StageCoord; }
            set
            {
                if (value != _StageCoord)
                {
                    _StageCoord = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private DeviceParams _DeviceParam;
        //public DeviceParams DeviceParam
        //{
        //    get { return _DeviceParam; }
        //    set
        //    {
        //        if (value != _DeviceParam)
        //        {
        //            _DeviceParam = value;
        //            NotifyPropertyChanged("DeviceParam");
        //        }
        //    }
        //}

        //================================


        private double _ChuckCoordXPos;
        public double ChuckCoordXPos
        {
            get { return _ChuckCoordXPos; }
            set
            {
                if (value != _ChuckCoordXPos)
                {
                    _ChuckCoordXPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ChuckCoordYPos;
        public double ChuckCoordYPos
        {
            get { return _ChuckCoordYPos; }
            set
            {
                if (value != _ChuckCoordYPos)
                {
                    _ChuckCoordYPos = value;
                    RaisePropertyChanged();
                }
            }
        }


        private double _ChuckCoordZPos;
        public double ChuckCoordZPos
        {
            get { return _ChuckCoordZPos; }
            set
            {
                if (value != _ChuckCoordZPos)
                {
                    _ChuckCoordZPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ViewCoordXPos;
        public double ViewCoordXPos
        {
            get { return _ViewCoordXPos; }
            set
            {
                if (value != _ViewCoordXPos)
                {
                    _ViewCoordXPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ViewCoordYPos;
        public double ViewCoordYPos
        {
            get { return _ViewCoordYPos; }
            set
            {
                if (value != _ViewCoordYPos)
                {
                    _ViewCoordYPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private UserIndex _CurUserIndex;
        [ParamIgnore]
        public UserIndex CurUserIndex
        {
            get { return _CurUserIndex; }
            set
            {
                if (value != _CurUserIndex)
                {
                    _CurUserIndex = value;
                    RaisePropertyChanged();

                }
            }
        }

        private MachineIndex _MachineIdx;
        [ParamIgnore]
        public MachineIndex MachineIdx
        {
            get { return _MachineIdx; }
            set
            {
                if (value != _MachineIdx)
                {
                    _MachineIdx = value;
                    RaisePropertyChanged();
                }
            }
        }

        private static int MonitoringInterValInms = 33;
        bool bStopUpdateThread;
        Thread UpdateThread;


        WaferCoordinate waferhigh = new WaferCoordinate();
        PinCoordinate pinhigh = new PinCoordinate();
        WaferCoordinate waferlow = new WaferCoordinate();
        PinCoordinate pinlow = new PinCoordinate();



        private WaferHighChuckCoordConvert _WaferHighChuckConvert = new WaferHighChuckCoordConvert();

        public WaferHighChuckCoordConvert WaferHighChuckConvert
        {
            get { return _WaferHighChuckConvert; }
            set { _WaferHighChuckConvert = value; }
        }

        private WaferLowChuckCoordConvert _WaferLowChuckConvert = new WaferLowChuckCoordConvert();

        public WaferLowChuckCoordConvert WaferLowChuckConvert
        {
            get { return _WaferLowChuckConvert; }
            set { _WaferLowChuckConvert = value; }
        }
        private PinHighPinCoordConvert _PinHighPinConvert = new PinHighPinCoordConvert();

        public PinHighPinCoordConvert PinHighPinConvert
        {
            get { return _PinHighPinConvert; }
            set { _PinHighPinConvert = value; }
        }
        private PinLowPinCoordinateConvert _PinLowPinConvert = new PinLowPinCoordinateConvert();

        public PinLowPinCoordinateConvert PinLowPinConvert
        {
            get { return _PinLowPinConvert; }
            set { _PinLowPinConvert = value; }
        }
        private WaferHighNCPadCoordConvert _WaferHighNCPadConvert = new WaferHighNCPadCoordConvert();

        public WaferHighNCPadCoordConvert WaferHighNCPadConvert
        {
            get { return _WaferHighNCPadConvert; }
            set { _WaferHighNCPadConvert = value; }
        }
        private WaferLowNCPadCoordinate _WaferLowNCPadConvert = new ProberInterfaces.WaferLowNCPadCoordinate();

        public WaferLowNCPadCoordinate WaferLowNCPadConvert
        {
            get { return _WaferLowNCPadConvert; }
            set { _WaferLowNCPadConvert = value; }
        }

        //================================
        private Point _WfCenterLowCam;

        public Point WfCenterLowCam
        {
            get { return _WfCenterLowCam; }
            set { _WfCenterLowCam = value; }
        }

        private Point _WfCenterHighCam;

        public Point WfCenterHighCam
        {
            get { return _WfCenterHighCam; }
            set { _WfCenterHighCam = value; }
        }


        private EnumProberCam _CamType;
        public EnumProberCam CamType
        {
            get { return _CamType; }
            set
            {
                if (value != _CamType)
                {
                    _CamType = value;
                    RaisePropertyChanged();
                }
            }
        }


        private CatCoordinates _CurrentCoordinate = new CatCoordinates();
        [ParamIgnore]
        public CatCoordinates CurrentCoordinate
        {
            get { return _CurrentCoordinate; }
            set
            {
                if (value != _CurrentCoordinate)
                {
                    _CurrentCoordinate = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CatCoordinates _CurUserCoord = new CatCoordinates();

        //private IDieViewPoint _DieViewPoint;

        //private EnumAxisConstants ProberPinAxis;
        private EnumAxisConstants _ProberPinAxis;
        public EnumAxisConstants ProberPinAxis
        {
            get { return _ProberPinAxis; }
            private set { _ProberPinAxis = value; }
        }

        [System.Diagnostics.DebuggerBrowsable(
            System.Diagnostics.DebuggerBrowsableState.Never)]
        [ParamIgnore]
        private WaferObject Wafer
        {
            get
            {
                return this.StageSupervisor()?.WaferObject as WaferObject;
            }
        }

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                IParam tmpParam = null;
                tmpParam = new StageCoords();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                //RetVal = Extensions_IParam.LoadParameter(ref tmpParam, typeof(StageCoords), null, null, Extensions_IParam.FileType.BIN, owner: this);
                RetVal = this.LoadParameter(ref tmpParam, typeof(StageCoords));

                if (RetVal == EventCodeEnum.NONE)
                {
                    StageCoord = tmpParam as StageCoords;

                    StageCoord.PHOffset.X.ValueChangedEvent += PHOffset_X_ValueChangedEvent;
                    StageCoord.PHOffset.Y.ValueChangedEvent += PHOffset_Y_ValueChangedEvent;
                    StageCoord.PHOffset.Z.ValueChangedEvent += PHOffset_Z_ValueChangedEvent;
                    StageCoord.PHOffset.T.ValueChangedEvent += PHOffset_T_ValueChangedEvent;
                }
                if (StageCoord.SafePosPZAxis > StageCoord.MarkEncPos.Z.Value + StageCoord.MarkPosInChuckCoord.Z.Value - 1000)
                {
                    StageCoord.SafePosPZAxis = StageCoord.MarkEncPos.Z.Value + StageCoord.MarkPosInChuckCoord.Z.Value + 5000;
                    LoggerManager.Debug($"CoordinateManager.LoadSysParameter(): StageCoord.SafePosPZAxis = {StageCoord.SafePosPZAxis}, Apply default interlock parameter({StageCoord.SafePosPZAxis}).");
                }
                if (StageCoord.SafePosZAxis > StageCoord.MarkEncPos.Z.Value + StageCoord.MarkPosInChuckCoord.Z.Value + 1500)
                {
                    StageCoord.SafePosZAxis = StageCoord.MarkEncPos.Z.Value + StageCoord.MarkPosInChuckCoord.Z.Value + 1500;        // 1500: 웨이퍼 허용 최대 두께
                    LoggerManager.Debug($"CoordinateManager.LoadSysParameter(): StageCoord.SafePosZAxis = {StageCoord.SafePosZAxis}, Apply default interlock parameter({StageCoord.MarkEncPos.Z.Value + StageCoord.MarkPosInChuckCoord.Z.Value + 1500}).");
                }
                else
                {
                    LoggerManager.Debug($"CoordinateManager.LoadSysParameter(): StageCoord.SafePosZAxis = {StageCoord.SafePosZAxis}, Apply default interlock parameter({StageCoord.MarkEncPos.Z.Value + StageCoord.MarkPosInChuckCoord.Z.Value + 1500}).");
                }
                //SysParam = new IParamEmpty();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                RetVal = this.SaveParameter(StageCoord);

                this.StageSupervisor().PinMaxRegRange = StageCoord.PinReg.PinRegMax.Value;
                this.StageSupervisor().PinMinRegRange = StageCoord.PinReg.PinRegMin.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        private void PHOffset_X_ValueChangedEvent(object oldValue, object newValue, object valueChangedParam = null)
        {
            try
            {
                LoggerManager.Debug($"[{this.GetType().Name}], PHOffset_X_ValueChangedEvent() : old = {oldValue}, new = {newValue}");

                this.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void PHOffset_Y_ValueChangedEvent(object oldValue, object newValue, object valueChangedParam = null)
        {
            try
            {
                LoggerManager.Debug($"[{this.GetType().Name}], PHOffset_Y_ValueChangedEvent() : old = {oldValue}, new = {newValue}");

                this.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void PHOffset_Z_ValueChangedEvent(object oldValue, object newValue, object valueChangedParam = null)
        {
            try
            {
                LoggerManager.Debug($"[{this.GetType().Name}], PHOffset_Z_ValueChangedEvent() : old = {oldValue}, new = {newValue}");

                this.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void PHOffset_T_ValueChangedEvent(object oldValue, object newValue, object valueChangedParam = null)
        {
            try
            {
                LoggerManager.Debug($"[{this.GetType().Name}], PHOffset_T_ValueChangedEvent() : old = {oldValue}, new = {newValue}");

                this.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public CoordinateManager()
        {

        }
        public void InitCoordinateManager()
        {
            try
            {
                bStopUpdateThread = false;
                UpdateThread = new Thread(new ThreadStart(UpdatePosition));
                UpdateThread.Name = this.GetType().Name;
                UpdateThread.Start();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void UpdatePosition()
        {
            try
            {
                while (bStopUpdateThread == false)
                {
                    PositionUpdated();
                    //minskim// GC 호출 및 CPU 사용률 절감을 위해 기존 timer+resetevent로 thread 제어하던 로직을 제거 하고 sleep으로 대체함, sleep시간은 기존 timer interval 주기 값으로 설정함
                    System.Threading.Thread.Sleep(MonitoringInterValInms);

                    //this.GEMModule().SetEvent(100);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public bool IsServiceAvailable()
        {
            return true;
        }
        public void InitService()
        {
            LoggerManager.Debug($"Coordinate service initialized.");
        }
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    Autofac.IContainer ContainerBuffer = this.GetContainer();
                    
                    ProberPinAxis = EnumAxisConstants.PZ;

                    WaferHighChuckConvert.InitModule();
                    WaferLowChuckConvert.InitModule();
                    PinHighPinConvert.InitModule(ContainerBuffer);
                    PinHighPinConvert.ProberPinAxis = ProberPinAxis;
                    PinLowPinConvert.InitModule(ContainerBuffer);
                    PinLowPinConvert.ProberPinAxis = ProberPinAxis;
                    WaferHighNCPadConvert.InitModule(ContainerBuffer);
                    WaferLowNCPadConvert.InitModule(ContainerBuffer);

                    //InitCoordinateManager();

                    _CurUserIndex = new UserIndex(0, 0);

                    Initialized = true;

                    retval = EventCodeEnum.NONE;
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
        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");

                bStopUpdateThread = true;
                UpdateThread?.Join();
            }
            catch (Exception err)
            {
                LoggerManager.Error($"DeInitMotionService() Function error: " + err.Message);
            }
        }
        MachineCoordinate PreMCoordinate = new MachineCoordinate();
        MachineCoordinate CurMCoordinate = new MachineCoordinate();        
        private void PositionUpdated()
        {
            try
            {
                this.VisionManager()?.GetCam(EnumProberCam.WAFER_HIGH_CAM).GetCurCoordPos();
                this.VisionManager()?.GetCam(EnumProberCam.WAFER_LOW_CAM).GetCurCoordPos();
                this.VisionManager()?.GetCam(EnumProberCam.PIN_HIGH_CAM).GetCurCoordPos();
                this.VisionManager()?.GetCam(EnumProberCam.PIN_LOW_CAM).GetCurCoordPos();
                this.VisionManager()?.GetCam(EnumProberCam.WAFER_HIGH_CAM).GetCurCoordIndex();
                this.VisionManager()?.GetCam(EnumProberCam.WAFER_LOW_CAM).GetCurCoordIndex();
                this.VisionManager()?.GetCam(EnumProberCam.PIN_HIGH_CAM).GetCurCoordIndex();
                this.VisionManager()?.GetCam(EnumProberCam.PIN_LOW_CAM).GetCurCoordIndex();
                this.VisionManager()?.GetCam(EnumProberCam.WAFER_HIGH_CAM).GetCurCoordMachineIndex();
                this.VisionManager()?.GetCam(EnumProberCam.WAFER_LOW_CAM).GetCurCoordMachineIndex();
                this.VisionManager()?.GetCam(EnumProberCam.PIN_HIGH_CAM).GetCurCoordMachineIndex();
                this.VisionManager()?.GetCam(EnumProberCam.PIN_LOW_CAM).GetCurCoordMachineIndex();
                this.VisionManager()?.GetCam(EnumProberCam.WAFER_HIGH_CAM).GetCurCoorNCCoord();
                this.VisionManager()?.GetCam(EnumProberCam.WAFER_LOW_CAM).GetCurCoorNCCoord();

                double xpos = 0.0;
                double ypos = 0.0;
                double zpos = 0.0;
                double tpos = 0.0;

                this.MotionManager()?.GetActualPoss(ref xpos, ref ypos, ref zpos, ref tpos);

                CurMCoordinate.CopyTo(PreMCoordinate);
                CurMCoordinate = new MachineCoordinate(xpos, ypos);

                if (OverlayUpdateDelegate != null)
                {
                    if ((Math.Abs(CurMCoordinate.GetX() - PreMCoordinate.GetX()) >= 1)
                        || (Math.Abs(CurMCoordinate.GetY() - PreMCoordinate.GetY()) >= 1))
                    {
                        OverlayUpdateDelegate();

                        LoggerManager.Debug($"Cur X, Y ({CurMCoordinate.GetX()}, {CurMCoordinate.GetY()}) || Pre X, Y ({PreMCoordinate.GetX()}, {PreMCoordinate.GetY()}) ");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public MachineCoordinate RelPosToAbsPos(MachineCoordinate RelPos)
        {
            double x = 0, y = 0, z = 0, t = 0;
            this.MotionManager().GetActualPoss(ref x, ref y, ref z, ref t);
            return new MachineCoordinate(RelPos.X.Value + x, RelPos.Y.Value + y, RelPos.Z.Value + z, RelPos.T.Value + t);
        }

        public double[,] GetAnyDieCornerPos(UserIndex ui)
        {
            double[,] RetPos = new double[2, 2];
            try
            {
                CatCoordinates DieCenterPos = new CatCoordinates();

                WaferCoordinate CurPosition = new WaferCoordinate();
                WaferCoordinate CurDieLeftCorner = new WaferCoordinate();

                Point pt = this.WaferAligner().GetLeftCornerPosition(CurPosition.X.Value, CurPosition.Y.Value);

                MachineIndex MachinDieIndex = GetCurMachineIndex(CurPosition);
                WaferCoordinate CurDieCenter = this.WaferAligner().MachineIndexConvertToDieLeftCorner_NonCalcZ(Convert.ToInt32(MachinDieIndex.XIndex), Convert.ToInt32(MachinDieIndex.YIndex));

                CurDieLeftCorner.X.Value = pt.X;
                CurDieLeftCorner.Y.Value = pt.Y;

                //좌측 상단
                RetPos[0, 0] = CurDieLeftCorner.X.Value;
                RetPos[0, 1] = CurDieLeftCorner.Y.Value + Wafer.GetSubsInfo().ActualDieSize.Height.Value;

                //우측 하단
                RetPos[1, 0] = CurDieLeftCorner.X.Value + Wafer.GetSubsInfo().ActualDieSize.Width.Value;
                RetPos[1, 1] = CurDieLeftCorner.Y.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetPos;
        }

        public double[,] GetAnyDieCornerPos(UserIndex ui, EnumProberCam camtype)
        {
            double[,] RetPos = new double[2, 2];
            try
            {
                CatCoordinates DieCenterPos = new CatCoordinates();

                //DieCenterPos = GetAnyDieCenterPos(ui);            

                //RetPos[0, 0] = DieCenterPos.X.Value + (this.WaferAligner().WaferAlignInfo.DieSizeX / 2);
                //RetPos[0, 1] = DieCenterPos.Y.Value + (this.WaferAligner().WaferAlignInfo.DieSizeY / 2);

                //RetPos[1, 0] = DieCenterPos.X.Value - (this.WaferAligner().WaferAlignInfo.DieSizeX / 2);
                //RetPos[1, 1] = DieCenterPos.Y.Value - (this.WaferAligner().WaferAlignInfo.DieSizeY / 2);

                WaferCoordinate CurPosition = new WaferCoordinate();
                WaferCoordinate CurDieLeftCorner = new WaferCoordinate();

                switch (camtype)
                {
                    case EnumProberCam.WAFER_LOW_CAM:
                        CurPosition = WaferLowChuckConvert.CurrentPosConvert();
                        break;
                    case EnumProberCam.WAFER_HIGH_CAM:
                        CurPosition = WaferHighChuckConvert.CurrentPosConvert();
                        break;
                }
                Point pt = this.WaferAligner().GetLeftCornerPosition(CurPosition.X.Value, CurPosition.Y.Value);

                MachineIndex MachinDieIndex = GetCurMachineIndex(CurPosition);
                WaferCoordinate CurDieCenter = this.WaferAligner().MachineIndexConvertToDieLeftCorner_NonCalcZ(Convert.ToInt32(MachinDieIndex.XIndex), Convert.ToInt32(MachinDieIndex.YIndex));

                CurDieLeftCorner.X.Value = pt.X;
                CurDieLeftCorner.Y.Value = pt.Y;

                //좌측 상단
                RetPos[0, 0] = Math.Truncate(CurDieLeftCorner.X.Value);
                RetPos[0, 1] = Math.Truncate(CurDieLeftCorner.Y.Value + Wafer.GetSubsInfo().ActualDeviceSize.Height.Value);


                //우측 하단
                RetPos[1, 0] = Math.Truncate(CurDieLeftCorner.X.Value + Wafer.GetSubsInfo().ActualDeviceSize.Width.Value);
                RetPos[1, 1] = Math.Truncate(CurDieLeftCorner.Y.Value);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetPos;
        }

        private bool _IsCoordToChuckContinus = false;
        public bool IsCoordToChuckContinus
        {
            get { return _IsCoordToChuckContinus; }
            set { _IsCoordToChuckContinus = value; }
        }

        public void Dispose()
        {
            try
            {
                IsCoordToChuckContinus = false;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void StageCoordConvertToChuckCoord()
        {
            try
            {
                _IsCoordToChuckContinus = true;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void StopStageCoordConvertToChuckCoord()
        {
        }
        
        public CatCoordinates StageCoordConvertToUserCoord(EnumProberCam camtype)
        {
            CatCoordinates coordPos = new CatCoordinates();

            double xPos = 0d, yPos = 0d, zPos = 0d;
            
            try
            {
                switch (camtype)
                {
                    case EnumProberCam.WAFER_HIGH_CAM:
                        waferhigh = WaferHighChuckConvert.CurrentPosConvert();
                        xPos = (waferhigh.X.Value);
                        yPos = (waferhigh.Y.Value);
                        zPos = (waferhigh.Z.Value);
                        break;
                    case EnumProberCam.WAFER_LOW_CAM:
                        waferlow = WaferLowChuckConvert.CurrentPosConvert();
                        xPos = (waferlow.X.Value);
                        yPos = (waferlow.Y.Value);
                        zPos = (waferlow.Z.Value);
                        break;
                    case EnumProberCam.PIN_HIGH_CAM:
                        pinhigh = PinHighPinConvert.CurrentPosConvert();
                        xPos = pinhigh.X.Value;
                        yPos = pinhigh.Y.Value;
                        zPos = pinhigh.Z.Value;
                        break;
                    case EnumProberCam.PIN_LOW_CAM:
                        pinlow = PinLowPinConvert.CurrentPosConvert();
                        xPos = pinlow.X.Value;
                        yPos = pinlow.Y.Value;
                        zPos = pinlow.Z.Value;
                        break;
                }
                
                //var wafc = new WaferCoordinate(0, 0, 0);
                //var mac = WaferHighChuckConvert.ConvertBack(wafc);
                //var wafR = WaferHighChuckConvert.Convert(mac);

            }
            catch (Exception err)
            {
                LoggerManager.Error($"CoordinateManager - StageCoordConvertToUserCoord() Error occurred: " + err.Message);
                throw new Exception(string.Format("StageCoordConvertToUserCoord(): Error occurred."), err);
            }
            
            _CurUserCoord.X.Value = xPos;
            _CurUserCoord.Y.Value = yPos;
            _CurUserCoord.Z.Value = zPos;

            ChuckCoordXPos = xPos;
            ChuckCoordYPos = yPos;
            ChuckCoordZPos = zPos;
                          
            coordPos.X.Value = xPos;
            coordPos.Y.Value = yPos;
            coordPos.Z.Value = zPos;
            
            return coordPos;                       
        }

        public bool GetReverseManualMoveX()
        {
            return this.StageCoord.ReverseManualMoveX.Value;
        }

        public bool GetReverseManualMoveY()
        {
            return this.StageCoord.ReverseManualMoveY.Value;
        }
        public UserIndex GetCurUserIndex(CatCoordinates Pos)
        {
            try
            {
                MachineIndex mindex = GetCurMachineIndex(new WaferCoordinate(Pos.GetX(), Pos.GetY(), Pos.GetZ()));
                long indexX = 0;
                long indexY = 0;
                if (mindex != null)
                {
                    long offsetindexX = mindex.XIndex - Wafer.GetPhysInfo().OrgM.XIndex.Value;
                    long offsetindexY = mindex.YIndex - Wafer.GetPhysInfo().OrgM.YIndex.Value;

                    indexX = Wafer.GetPhysInfo().OrgU.XIndex.Value + (offsetindexX * (int)Wafer.GetPhysInfo().MapDirX.Value);
                    indexY = Wafer.GetPhysInfo().OrgU.YIndex.Value + (offsetindexY * (int)Wafer.GetPhysInfo().MapDirY.Value);
                }
                return new UserIndex(indexX, indexY);

            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "GetCurUserIndex() : Error occured");
                LoggerManager.Exception(err);

                return null;
            }
        }

        public MachineIndex GetCurMachineIndex(WaferCoordinate Pos)
        {
            try
            {

                Point pt = this.WaferAligner().GetLeftCornerPosition(Pos);
                Point refideleftcorner = this.WaferAligner().GetLeftCornerPosition(Wafer.GetSubsInfo().WaferCenter);

                double offsetrefdiex = (pt.X - refideleftcorner.X) / Wafer.GetSubsInfo().ActualDieSize.Width.Value;
                double offsetrefdiey = (pt.Y - refideleftcorner.Y) / Wafer.GetSubsInfo().ActualDieSize.Height.Value;

                long indexX = Convert.ToInt64(Wafer.GetPhysInfo().CenM.XIndex.Value + offsetrefdiex);
                long indexY = Convert.ToInt64(Wafer.GetPhysInfo().CenM.YIndex.Value + offsetrefdiey);
 
                return new MachineIndex(indexX, indexY);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                return null;
            }
        }
        public UserIndex MachineIndexConvertToUserIndex(MachineIndex MI)
        {
            long uindexx = 0;
            long uindexy = 0;

            try
            {
                uindexx = MI.XIndex - Wafer.GetPhysInfo().OrgM.XIndex.Value;
                uindexy = MI.YIndex - Wafer.GetPhysInfo().OrgM.YIndex.Value;

                uindexx *= (int)Wafer.GetPhysInfo().MapDirX.Value;
                uindexy *= (int)Wafer.GetPhysInfo().MapDirY.Value;

                uindexx = Wafer.GetPhysInfo().OrgU.XIndex.Value + uindexx;
                uindexy = Wafer.GetPhysInfo().OrgU.YIndex.Value + uindexy;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return new UserIndex(uindexx, uindexy);
        }
        public MachineIndex UserIndexConvertToMachineIndex(UserIndex UI)
        {
            long mindexx = 0;
            long mindexy = 0;

            try
            {
                mindexx = ((UI.XIndex - Wafer.GetPhysInfo().OrgU.XIndex.Value) * (long)Wafer.GetPhysInfo().MapDirX.Value) + Wafer.GetPhysInfo().OrgM.XIndex.Value;
                mindexy = ((UI.YIndex - Wafer.GetPhysInfo().OrgU.YIndex.Value) * (long)Wafer.GetPhysInfo().MapDirY.Value) + Wafer.GetPhysInfo().OrgM.YIndex.Value;

                //mindexx = Wafer.GetPhysInfo().CenM.XIndex.Value
                //     + (UI.XIndex *= (int)Wafer.GetPhysInfo().MapDirX.Value);
                //mindexy = Wafer.GetPhysInfo().CenM.YIndex.Value
                //    + (UI.YIndex *= (int)Wafer.GetPhysInfo().MapDirY.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return new MachineIndex(mindexx, mindexy);
        }

        public UserIndex WMIndexConvertWUIndex(long mindexX, long mindexY)
        {
            long uindexx = 0;
            long uindexy = 0;

            try
            {
                uindexx = mindexX - Wafer.GetPhysInfo().OrgM.XIndex.Value;
                uindexy = mindexY - Wafer.GetPhysInfo().OrgM.YIndex.Value;

                uindexx *= (int)Wafer.GetPhysInfo().MapDirX.Value;
                uindexy *= (int)Wafer.GetPhysInfo().MapDirY.Value;

                uindexx = Wafer.GetPhysInfo().OrgU.XIndex.Value + uindexx;
                uindexy = Wafer.GetPhysInfo().OrgU.YIndex.Value + uindexy;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return new UserIndex(uindexx, uindexy);
        }

        // 계산식 잘못되어 있지만 쓰이고 있는 곳 없는 상태. 쓰면 안됨.
        public MachineIndex WUIndexConvertWMIndex(long uindexX, long uindexY)
        {
            long mindexx = 0;
            long mindexy = 0;

            try
            {
                mindexx = Wafer.GetPhysInfo().CenM.XIndex.Value + (uindexX *= (int)Wafer.GetPhysInfo().MapDirX.Value);
                mindexy = Wafer.GetPhysInfo().CenM.YIndex.Value + (uindexY *= (int)Wafer.GetPhysInfo().MapDirY.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return new MachineIndex(mindexx, mindexy);
        }
     
        /// <summary>
        /// Calculate distance of two points
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        public double DistanceOfPoints(double x1, double y1, double x2, double y2)
        {
            double x = x1 - x2;
            double y = y1 - y2;
            return Math.Sqrt(x * x + y * y);
        }

        /// <summary>
        /// Calculate degree of two points.
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        public double CalcP2PAngle(double x1, double y1, double x2, double y2)
        {
            double degree = 0.0;
            try
            {
                double dx = x2 - x1;
                double dy = y2 - y1;

                double radian = (double)Math.Atan2(dy, dx);
                degree = (double)((radian * 180) / Math.PI);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return degree;
        }

        public MachineCoordinate GetRotatedPoint(MachineCoordinate point, MachineCoordinate pivotPoint, double degrees)
        {
            try
            {
                double radian = (double)(((degrees) / 180) * Math.PI);

                double cosq = Math.Cos(radian);
                double sinq = Math.Sin(radian);
                double sx = point.X.Value - pivotPoint.X.Value;
                double sy = point.Y.Value - pivotPoint.Y.Value;
                double rx = (sx * cosq - sy * sinq) + pivotPoint.X.Value; // 결과 좌표 x
                double ry = (sx * sinq + sy * cosq) + pivotPoint.Y.Value; // 결과 좌표 y

                return new MachineCoordinate((int)rx, (int)ry);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public CatCoordinates PmResultConverToUserCoord(PMResult pmresult)
        {
            CatCoordinates coord = null;
            try
            {

                if (pmresult.ResultBuffer.CamType == EnumProberCam.WAFER_HIGH_CAM)
                {
                    coord = (WaferCoordinate)WaferHighChuckConvert.CurrentPosConvert();
                }
                else if (pmresult.ResultBuffer.CamType == EnumProberCam.WAFER_LOW_CAM)
                {
                    coord = (WaferCoordinate)WaferLowChuckConvert.CurrentPosConvert();
                }

                double ptxpos = pmresult.ResultParam[0].XPoss;
                double ptypos = pmresult.ResultParam[0].YPoss;

                double offsetx = ptxpos - (pmresult.ResultBuffer.SizeX / 2);
                double offsety = ptypos - (pmresult.ResultBuffer.SizeY / 2);

                offsetx *= pmresult.ResultBuffer.RatioX.Value;
                offsety *= pmresult.ResultBuffer.RatioY.Value;

                coord.X.Value += offsetx;
                coord.Y.Value += offsety;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return coord;
        }

        public void SetPinAxisAs(EnumAxisConstants axis)
        {
            ProberPinAxis = axis;
            PinHighPinConvert.ProberPinAxis = ProberPinAxis;
            PinLowPinConvert.ProberPinAxis = ProberPinAxis;
            LoggerManager.Debug($"Use pin view axis as {axis}.");
        }

        public void UpdateCenM()
        {
            try
            {
                IPhysicalInfo physicalInfo = this.StageSupervisor().WaferObject.GetPhysInfo();

                var CenM = this.UserIndexConvertToMachineIndex(new UserIndex(physicalInfo.CenU.XIndex.Value, physicalInfo.CenU.YIndex.Value));

                physicalInfo.CenM.XIndex.Value = CenM.XIndex;
                physicalInfo.CenM.YIndex.Value = CenM.YIndex;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void CalculateOffsetFromCurrentZ(EnumProberCam camchannel)
        {
            try
            {
                double curxpos = 0.0;
                double curypos = 0.0;
                double expected_zpos = 0.0;
                double curzpos = 0.0;

                var waferobj = this.StageSupervisor().WaferObject.GetSubsInfo();

                CoordinateConvertBase<WaferCoordinate> convert = null;
                
                if (camchannel == EnumProberCam.WAFER_HIGH_CAM)
                {
                    convert = this.CoordinateManager().WaferHighChuckConvert;
                }
                else if (camchannel == EnumProberCam.WAFER_LOW_CAM)
                {
                    convert = this.CoordinateManager().WaferLowChuckConvert;
                }

                curxpos = convert.CurrentPosConvert().GetX();
                curypos = convert.CurrentPosConvert().GetY();
                curzpos = convert.CurrentPosConvert().GetZ();

                // focusing 하기 전 x,y 값으로 move 된 값
                expected_zpos = this.WaferAligner().GetHeightValue(curxpos, curypos, true);

                waferobj.MoveZOffset = curzpos - expected_zpos;
                
                //TODO : Pad 높이가 정해진다면, 그 값으로 tolereance 기준을 변경
                int tolereance_zoffset = 50;

                if (Math.Abs(waferobj.MoveZOffset) > tolereance_zoffset)
                {
                    if (waferobj.MoveZOffset > 0)
                    {
                        waferobj.MoveZOffset = tolereance_zoffset;
                    }
                    else if (waferobj.MoveZOffset < 0)
                    {
                        waferobj.MoveZOffset = -tolereance_zoffset;
                    }
                }

                LoggerManager.Debug($"[{this.GetType().Name}] CalculateOffsetFromCurrentZ(), CamChannel : {camchannel}, Move Z Offset : {waferobj.MoveZOffset:0.00} = Current Pos : {curzpos:0.00} - GetHeightValue : {expected_zpos:0.00} (X,Y) : ({curxpos:0.00},{curypos:0.00})");
            }
            catch (Exception err)
            {
                this.StageSupervisor().WaferObject.GetSubsInfo().MoveZOffset = 0.0;
                LoggerManager.Exception(err);
            }
        }
    }
}



