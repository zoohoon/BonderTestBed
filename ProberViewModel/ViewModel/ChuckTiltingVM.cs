using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.LoaderController;
using ProberInterfaces.Param;
using RelayCommandBase;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChuckTiltingViewModel
{
    public class ChuckTiltingVM : IMainScreenViewModel
    {
        private Guid _ViewModelGUID = new Guid("dd94aa10-ff3b-4bac-a398-1d0736ab85ca");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        public Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if(this.MotionManager() != null)
                {
                    ZAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                    PZAxis = this.MotionManager().GetAxis(EnumAxisConstants.PZ);
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            Task<EventCodeEnum> task = null;

            try
            {
                task = Task<EventCodeEnum>.Run(() =>
                {
                    ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);

                    return this.MotionManager().StageMove(0, 0, -60000, 0);
                });

                task.Wait();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return task;
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private ZGroupAxes _ZGroupAxes;
        public ZGroupAxes ZGroupAxes
        {
            get { return _ZGroupAxes; }
            set
            {
                if (value != _ZGroupAxes)
                {
                    _ZGroupAxes = value;
                    RaisePropertyChanged();
                }
            }

        }

        private double _OffsetValue;
        public double OffsetValue
        {
            get { return _OffsetValue; }
            set
            {
                if (value != _OffsetValue)
                {
                    _OffsetValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ProbeAxisObject _ZAxis;
        public ProbeAxisObject ZAxis
        {
            get { return _ZAxis; }
            set
            {
                if (value != _ZAxis)
                {
                    _ZAxis = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ProbeAxisObject _PZAxis;
        public ProbeAxisObject PZAxis
        {
            get { return _PZAxis; }
            set
            {
                if (value != _PZAxis)
                {
                    _PZAxis = value;
                    RaisePropertyChanged();
                }
            }
        }


        //private ProbeAxisObject _Axis_Z0;
        //public ProbeAxisObject Axis_Z0
        //{
        //    get { return _Axis_Z0; }
        //    set
        //    {
        //        if (value != _Axis_Z0)
        //        {
        //            _Axis_Z0 = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private ProbeAxisObject _Axis_Z1;
        //public ProbeAxisObject Axis_Z1
        //{
        //    get { return _Axis_Z1; }
        //    set
        //    {
        //        if (value != _Axis_Z1)
        //        {
        //            _Axis_Z1 = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private ProbeAxisObject _Axis_Z2;
        //public ProbeAxisObject Axis_Z2
        //{
        //    get { return _Axis_Z2; }
        //    set
        //    {
        //        if (value != _Axis_Z2)
        //        {
        //            _Axis_Z2 = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}


        //private double _ThreePot_EncorderPos_LeftTop;
        //public double ThreePot_EncorderPos_LeftTop
        //{
        //    get { return _ThreePot_EncorderPos_LeftTop; }
        //    set
        //    {
        //        if (value != _ThreePot_EncorderPos_LeftTop)
        //        {
        //            _ThreePot_EncorderPos_LeftTop = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private double _ThreePot_EncorderPos_RightTop;
        //public double ThreePot_EncorderPos_RightTop
        //{
        //    get { return _ThreePot_EncorderPos_RightTop; }
        //    set
        //    {
        //        if (value != _ThreePot_EncorderPos_RightTop)
        //        {
        //            _ThreePot_EncorderPos_RightTop = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private double _ThreePot_EncorderPos_Bottom;
        //public double ThreePot_EncorderPos_Bottom
        //{
        //    get { return _ThreePot_EncorderPos_Bottom; }
        //    set
        //    {
        //        if (value != _ThreePot_EncorderPos_Bottom)
        //        {
        //            _ThreePot_EncorderPos_Bottom = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private double _OffsetValue_Pos12;
        public double OffsetValue_Pos12
        {
            get { return _OffsetValue_Pos12; }
            set
            {
                if (value != _OffsetValue_Pos12)
                {
                    _OffsetValue_Pos12 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _OffsetValue_Pos1;
        public double OffsetValue_Pos1
        {
            get { return _OffsetValue_Pos1; }
            set
            {
                if (value != _OffsetValue_Pos1)
                {
                    _OffsetValue_Pos1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _OffsetValue_Pos3;
        public double OffsetValue_Pos3
        {
            get { return _OffsetValue_Pos3; }
            set
            {
                if (value != _OffsetValue_Pos3)
                {
                    _OffsetValue_Pos3 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _OffsetValue_Pos5;
        public double OffsetValue_Pos5
        {
            get { return _OffsetValue_Pos5; }
            set
            {
                if (value != _OffsetValue_Pos5)
                {
                    _OffsetValue_Pos5 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _OffsetValue_Pos6;
        public double OffsetValue_Pos6
        {
            get { return _OffsetValue_Pos6; }
            set
            {
                if (value != _OffsetValue_Pos6)
                {
                    _OffsetValue_Pos6 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _OffsetValue_Pos7;
        public double OffsetValue_Pos7
        {
            get { return _OffsetValue_Pos7; }
            set
            {
                if (value != _OffsetValue_Pos7)
                {
                    _OffsetValue_Pos7 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _OffsetValue_Pos9;
        public double OffsetValue_Pos9
        {
            get { return _OffsetValue_Pos9; }
            set
            {
                if (value != _OffsetValue_Pos9)
                {
                    _OffsetValue_Pos9 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _OffsetValue_Pos11;
        public double OffsetValue_Pos11
        {
            get { return _OffsetValue_Pos11; }
            set
            {
                if (value != _OffsetValue_Pos11)
                {
                    _OffsetValue_Pos11 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _GroupIndex = 1;
        public int GroupIndex
        {
            get { return _GroupIndex; }
            set
            {
                if (value != _GroupIndex)
                {
                    _GroupIndex = value;

                    if (_GroupIndex == 1)
                    {
                        MultipleValue = 1;
                    }
                    else if (_GroupIndex == 2)
                    {
                        MultipleValue = 100;
                    }
                    else if (_GroupIndex == 3)
                    {
                        MultipleValue = 1000;
                    }
                    else
                    {
                        MultipleValue = 1;
                    }

                    RaisePropertyChanged();
                }
            }
        }

        private int _MultipleValue = 1;
        public int MultipleValue
        {
            get { return _MultipleValue; }
            set
            {
                if (value != _MultipleValue)
                {
                    _MultipleValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _RValue;
        public double RValue
        {
            get { return _RValue; }
            set
            {
                if (value != _RValue)
                {
                    _RValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _TValue;
        public double TValue
        {
            get { return _TValue; }
            set
            {
                if (value != _TValue)
                {
                    _TValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private RelayCommand<object> _TestCommand;
        public ICommand TestCommand
        {
            get
            {
                if (null == _TestCommand) _TestCommand = new RelayCommand<object>(TestCmd);
                return _TestCommand;
            }
        }

        #region ==> Z1
        private ProbeAxisObject _Z1;
        public ProbeAxisObject Z1
        {
            get { return _Z1; }
            set
            {
                if (value != _Z1)
                {
                    _Z1 = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> Z2
        private ProbeAxisObject _Z2;
        public ProbeAxisObject Z2
        {
            get { return _Z2; }
            set
            {
                if (value != _Z2)
                {
                    _Z2 = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> Z3
        private ProbeAxisObject _Z3;
        public ProbeAxisObject Z3
        {
            get { return _Z3; }
            set
            {
                if (value != _Z3)
                {
                    _Z3 = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> Z
        private ProbeAxisObject _Z;
        public ProbeAxisObject Z
        {
            get { return _Z; }
            set
            {
                if (value != _Z)
                {
                    _Z = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> PZ
        private ProbeAxisObject _PZ;
        public ProbeAxisObject PZ
        {
            get { return _PZ; }
            set
            {
                if (value != _PZ)
                {
                    _PZ = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        private void TestCmd(object obj)
        {
            try
            {

                Random r = new Random();
                int rInt = r.Next(-100, 100); //for ints
                int range = 100;
                double rDouble = r.NextDouble() * range; //for doubles

                OffsetValue_Pos12 = Math.Round(rDouble, 2, MidpointRounding.AwayFromZero);

                rDouble = r.NextDouble() * range; //for doubles

                OffsetValue_Pos1 = Math.Round(rDouble, 2, MidpointRounding.AwayFromZero);

                rDouble = r.NextDouble() * range; //for doubles

                OffsetValue_Pos3 = Math.Round(rDouble, 2, MidpointRounding.AwayFromZero);

                rDouble = r.NextDouble() * range; //for doubles
                OffsetValue_Pos5 = Math.Round(rDouble, 2, MidpointRounding.AwayFromZero);

                rDouble = r.NextDouble() * range; //for doubles
                OffsetValue_Pos6 = Math.Round(rDouble, 2, MidpointRounding.AwayFromZero);

                rDouble = r.NextDouble() * range; //for doubles
                OffsetValue_Pos7 = Math.Round(rDouble, 2, MidpointRounding.AwayFromZero);

                rDouble = r.NextDouble() * range; //for doubles
                OffsetValue_Pos9 = Math.Round(rDouble, 2, MidpointRounding.AwayFromZero);

                rDouble = r.NextDouble() * range; //for doubles

                OffsetValue_Pos11 = Math.Round(rDouble, 2, MidpointRounding.AwayFromZero);

                //r = new Random();
                //rInt = r.Next(-100, 100); //for ints
                //range = 100;
                //rDouble = r.NextDouble() * range; //for doubles

                //OffsetValue_Pos3 = rDouble.ToString();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private int _RPosDist;
        public int RPosDist
        {
            get { return _RPosDist; }
            set
            {
                if (value != _RPosDist)
                {
                    _RPosDist = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _TTPosDist;
        public double TTPosDist
        {
            get { return _TTPosDist; }
            set
            {
                if (value != _TTPosDist)
                {
                    _TTPosDist = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _TiltCommand;
        public bool TiltCommand
        {
            get { return _TiltCommand; }
            set
            {
                if (value != _TiltCommand)
                {
                    _TiltCommand = value;
                    RaisePropertyChanged();
                }
            }
        }

        public IWaferObject Wafer { get; private set; }
        public ILoaderController LoaderController { get; private set; }


        private AsyncCommand _TiltMoveCommand;
        public ICommand TiltMoveCommand
        {
            get
            {
                if (null == _TiltMoveCommand) _TiltMoveCommand = new AsyncCommand(ChuckTiltMove);
                return _TiltMoveCommand;
            }
        }
        private async Task ChuckTiltMove()
        {
            try
            {
                var axisz0 = this.MotionManager().GetAxis(EnumAxisConstants.Z0);
                var axisz1 = this.MotionManager().GetAxis(EnumAxisConstants.Z1);
                var axisz2 = this.MotionManager().GetAxis(EnumAxisConstants.Z2);
                double offsetz = 0d;
                await Task.Run(() =>
                {
                    //Z0Pos = axisz0.Status.RawPosition.Actual;
                    //Z1Pos = axisz1.Status.RawPosition.Actual;
                    //Z2Pos = axisz2.Status.RawPosition.Actual;

                    this.StageSupervisor().StageModuleState.ChuckTiltMove(RPosDist, TTPosDist);

                    //Z0Pos = axisz0.Status.RawPosition.Actual;
                    //Z1Pos = axisz1.Status.RawPosition.Actual;
                    //Z2Pos = axisz2.Status.RawPosition.Actual;

                    GetTiltOffset(ref offsetz, 45);
                    OffsetValue_Pos1 = offsetz;
                    GetTiltOffset(ref offsetz, 0);
                    OffsetValue_Pos3 = offsetz;
                    GetTiltOffset(ref offsetz, 315);
                    OffsetValue_Pos5 = offsetz;
                    GetTiltOffset(ref offsetz, 270);
                    OffsetValue_Pos6 = offsetz;
                    GetTiltOffset(ref offsetz, 225);
                    OffsetValue_Pos7 = offsetz;
                    GetTiltOffset(ref offsetz, 180);
                    OffsetValue_Pos9 = offsetz;
                    GetTiltOffset(ref offsetz, 135);
                    OffsetValue_Pos11 = offsetz;
                    GetTiltOffset(ref offsetz, 90);
                    OffsetValue_Pos12 = offsetz;
                    RValue = RPosDist;
                    TValue = TTPosDist;

                });

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _AutoTiltCommand;
        public ICommand AutoTiltCommand
        {
            get
            {
                if (null == _AutoTiltCommand) _AutoTiltCommand = new AsyncCommand(AutoTilt);
                return _AutoTiltCommand;
            }
        }
        private async Task AutoTilt()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ZGroupAxes tmpGroupAxes = new ZGroupAxes();

                var axisz0 = this.MotionManager().GetAxis(EnumAxisConstants.Z0);
                var axisz1 = this.MotionManager().GetAxis(EnumAxisConstants.Z1);
                var axisz2 = this.MotionManager().GetAxis(EnumAxisConstants.Z2);
                double offsetz = 0d;

                TiltCommand = true;
                int cnt = 0;
                int rposition = 0;
                double z0pos = 0;
                double z1pos = 0;
                double z2pos = 0;

                await Task.Run(() =>
                {
                    while (TiltCommand)
                    {
                        ZGroupAxes tmpGroupAxes2 = new ZGroupAxes();

                        if (cnt == 0)
                        {
                            rposition = 0;
                        }
                        else if (cnt == 1)
                        {
                            rposition = 45;
                        }
                        else if (cnt == 2)
                        {
                            rposition = 90;
                        }
                        else if (cnt == 3)
                        {
                            rposition = 135;
                        }
                        //Z0Pos = axisz0.Status.RawPosition.Actual;
                        //Z1Pos = axisz1.Status.RawPosition.Actual;
                        //Z2Pos = axisz2.Status.RawPosition.Actual;
                        retVal = this.StageSupervisor().StageModuleState.ChuckTiltMove(rposition, TTPosDist);
                        if (retVal == EventCodeEnum.STAGEMOVE_NOTIMPLEMENT_ERROR)
                        {
                            TiltCommand = false;
                            break;
                        }
                        RPosDist = rposition;
                        RValue = rposition;
                        TValue = TTPosDist;

                        this.MotionManager().GetActualPos(EnumAxisConstants.Z0, ref z0pos);
                        //Z0Pos = z0pos;
                        this.MotionManager().GetActualPos(EnumAxisConstants.Z1, ref z1pos);
                        //Z1Pos = z1pos;
                        this.MotionManager().GetActualPos(EnumAxisConstants.Z2, ref z2pos);
                        //Z2Pos = z2pos;

                        tmpGroupAxes2.Z0Pos = z0pos;
                        tmpGroupAxes2.Z1Pos = z1pos;
                        tmpGroupAxes2.Z2Pos = z2pos;

                        ZGroupAxes = tmpGroupAxes2;

                        Thread.Sleep(1000);

                        GetTiltOffset(ref offsetz, 45);
                        OffsetValue_Pos1 = offsetz;
                        GetTiltOffset(ref offsetz, 0);
                        OffsetValue_Pos3 = offsetz;
                        GetTiltOffset(ref offsetz, 315);
                        OffsetValue_Pos5 = offsetz;
                        GetTiltOffset(ref offsetz, 270);
                        OffsetValue_Pos6 = offsetz;
                        GetTiltOffset(ref offsetz, 225);
                        OffsetValue_Pos7 = offsetz;
                        GetTiltOffset(ref offsetz, 180);
                        OffsetValue_Pos9 = offsetz;
                        GetTiltOffset(ref offsetz, 135);
                        OffsetValue_Pos11 = offsetz;
                        GetTiltOffset(ref offsetz, 90);
                        OffsetValue_Pos12 = offsetz;

                        this.StageSupervisor().StageModuleState.ChuckTiltMove(rposition + 180, TTPosDist);
                        this.MotionManager().GetActualPos(EnumAxisConstants.Z0, ref z0pos);
                        //Z0Pos = z0pos;
                        this.MotionManager().GetActualPos(EnumAxisConstants.Z1, ref z1pos);
                        //Z1Pos = z1pos;
                        this.MotionManager().GetActualPos(EnumAxisConstants.Z2, ref z2pos);
                        //Z2Pos = z2pos;

                        tmpGroupAxes2 = new ZGroupAxes();

                        tmpGroupAxes2.Z0Pos = z0pos;
                        tmpGroupAxes2.Z1Pos = z1pos;
                        tmpGroupAxes2.Z2Pos = z2pos;

                        ZGroupAxes = tmpGroupAxes2;

                        //ZGroupAxes.Z0Pos = z0pos;
                        //ZGroupAxes.Z1Pos = z1pos;
                        //ZGroupAxes.Z2Pos = z2pos;

                        RPosDist = rposition + 180;
                        RValue = rposition + 180;
                        TValue = TTPosDist;
                        Thread.Sleep(1000);


                        GetTiltOffset(ref offsetz, 45);
                        OffsetValue_Pos1 = offsetz;
                        GetTiltOffset(ref offsetz, 0);
                        OffsetValue_Pos3 = offsetz;
                        GetTiltOffset(ref offsetz, 315);
                        OffsetValue_Pos5 = offsetz;
                        GetTiltOffset(ref offsetz, 270);
                        OffsetValue_Pos6 = offsetz;
                        GetTiltOffset(ref offsetz, 225);
                        OffsetValue_Pos7 = offsetz;
                        GetTiltOffset(ref offsetz, 180);
                        OffsetValue_Pos9 = offsetz;
                        GetTiltOffset(ref offsetz, 135);
                        OffsetValue_Pos11 = offsetz;
                        GetTiltOffset(ref offsetz, 90);
                        OffsetValue_Pos12 = offsetz;

                        cnt++;
                        if (cnt == 4)
                        {
                            cnt = 0;
                        }


                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _AutoTiltStopCommand;
        public ICommand AutoTiltStopCommand
        {
            get
            {
                if (null == _AutoTiltStopCommand) _AutoTiltStopCommand = new AsyncCommand(AutoTiltStop);
                return _AutoTiltStopCommand;
            }
        }
        private async Task AutoTiltStop()
        {
            try
            {
                await Task.Run(() =>
                {
                    TiltCommand = false;
                    RPosDist = 0;
                    TTPosDist = 0;
                    RValue = 0;
                    TValue = 0;
                    this.StageSupervisor().StageModuleState.ChuckTiltMove(RPosDist, TTPosDist);
                    RPosDist = 0;
                    TTPosDist = 0;
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private int GetTiltOffset(ref double offset, int offsetdegree)
        {
            try
            {
                int ret = -1;
                double a = 0d;
                double b = 0d;
                double c = 0d;
                double d = 0d;
                CatCoordinates tiltPoint = new CatCoordinates();
                CatCoordinates pivotPointCW = new CatCoordinates();
                CatCoordinates pivotPointCCW = new CatCoordinates();
                var axisTheta = this.MotionManager().GetAxis(EnumAxisConstants.C);

                tiltPoint.X.Value = (150000 * Math.Cos(Math.PI * RPosDist / 180.0));
                tiltPoint.Y.Value = (150000 * Math.Sin(Math.PI * RPosDist / 180.0));
                tiltPoint.Z.Value = TTPosDist;

                //각도로는 -방향
                pivotPointCW.X.Value = (150000 * Math.Cos(Math.PI * (RPosDist - 90.0) / 180.0));
                pivotPointCW.Y.Value = (150000 * Math.Sin(Math.PI * (RPosDist - 90.0) / 180.0));
                pivotPointCW.Z.Value = 0d;


                //각도로는 +방향
                pivotPointCCW.X.Value = (150000 * Math.Cos(Math.PI * (RPosDist + 90.0) / 180.0));
                pivotPointCCW.Y.Value = (150000 * Math.Sin(Math.PI * (RPosDist + 90.0) / 180.0));
                pivotPointCCW.Z.Value = 0d;

                a = tiltPoint.Y.Value * (pivotPointCW.Z.Value - pivotPointCCW.Z.Value) +
                       pivotPointCW.Y.Value * (pivotPointCCW.Z.Value - tiltPoint.Z.Value) +
                       pivotPointCCW.Y.Value * (tiltPoint.Z.Value - pivotPointCW.Z.Value);

                b = tiltPoint.Z.Value * (pivotPointCW.X.Value - pivotPointCCW.X.Value) +
                    pivotPointCW.Z.Value * (pivotPointCCW.X.Value - tiltPoint.X.Value) +
                    pivotPointCCW.Z.Value * (tiltPoint.X.Value - pivotPointCW.X.Value);

                c = tiltPoint.X.Value * (pivotPointCW.Y.Value - pivotPointCCW.Y.Value) +
                    pivotPointCW.X.Value * (pivotPointCCW.Y.Value - tiltPoint.Y.Value) +
                    pivotPointCCW.X.Value * (tiltPoint.Y.Value - pivotPointCW.Y.Value);

                d = -(tiltPoint.X.Value * ((pivotPointCW.Y.Value * pivotPointCCW.Z.Value) - (pivotPointCCW.Y.Value * pivotPointCW.Z.Value)) -
                     pivotPointCW.X.Value * ((pivotPointCCW.Y.Value * tiltPoint.Z.Value) - (tiltPoint.Y.Value * pivotPointCCW.Z.Value)) -
                     pivotPointCCW.X.Value * ((tiltPoint.Y.Value * pivotPointCW.Z.Value) - (pivotPointCW.Y.Value * tiltPoint.Z.Value)));

                double zRadius = 150000;
                //Z0 offset

                double pillar0X = 0d;

                double pillar0Y = 0d;

                pillar0X = zRadius * Math.Cos(Math.PI * (offsetdegree + (axisTheta.Status.Position.Ref / 10000)) / 180);
                pillar0Y = zRadius * Math.Sin(Math.PI * (offsetdegree + (axisTheta.Status.Position.Ref / 10000)) / 180);

                offset = (-a * pillar0X - b * pillar0Y - d) / c;
                offset = Math.Round(offset);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
           
            return 0;
        }
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    Initialized = true;

                    if(this.MotionManager() != null)
                    {
                        Z1 = this.MotionManager().GetAxis(EnumAxisConstants.Z0);
                        Z2 = this.MotionManager().GetAxis(EnumAxisConstants.Z1);
                        Z3 = this.MotionManager().GetAxis(EnumAxisConstants.Z2);
                        Z = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                        PZ = this.MotionManager().GetAxis(EnumAxisConstants.PZ);
                    }

                    ZGroupAxes = new ZGroupAxes();

                    OffsetValue = 0;

                    Wafer = this.StageSupervisor().WaferObject;
                    LoaderController = this.LoaderController();
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
                retval = EventCodeEnum.UNDEFINED;
                LoggerManager.Error($"[ChuckTiltingVM] InitModule() Error.");
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
}
