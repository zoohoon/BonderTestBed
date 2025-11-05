using LogModule;
using NeedleCleanHeightProfilingParamObject;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.NeedleClean;
using ProberInterfaces.Param;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media.Media3D;

namespace NeedleCleanHeightProfilingModule
{
    public class NeedleCleanHeightProfiling : INotifyPropertyChanged, IFactoryModule, IModule, IHasSysParameterizable, INeedleCleanHeightProfiling
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;



        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        public void DeInitModule()
        {

        }

        public bool GetEnable()
        {
            return this.Param.Enable.Value;
        }

        public enum ZDirection
        {
            UP = 0,
            DOWN
        }

        //private PZErrorParameter GetTable(double ZValue, ZDirection dir)
        //{
        //    PZErrorParameter retval = null;

        //    try
        //    {
        //        if (dir == ZDirection.UP)
        //        {
        //            List<PZErrorParameter> tmptable = Param.ErrorTable.FindAll(x => x.ZHeightOfPlane.Value > ZValue);

        //            if (tmptable != null)
        //            {
        //                retval = tmptable.OrderByDescending(x => x.ZHeightOfPlane.Value).First();
        //            }
        //        }
        //        else if (dir == ZDirection.DOWN)
        //        {
        //            List<PZErrorParameter> tmptable = Param.ErrorTable.FindAll(x => x.ZHeightOfPlane.Value < ZValue);

        //            if (tmptable != null)
        //            {
        //                retval = tmptable.OrderBy(x => x.ZHeightOfPlane.Value).First();
        //            }
        //        }

        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        private PZErrorParameter GetTable(double ZValue)
        {
            PZErrorParameter retval = null;

            try
            {
                double minval = 0;
                int index = 0;

                if (Param.ErrorTable != null && Param.ErrorTable.Count > 0)
                {
                    try
                    {
                        for (int i = 0; i < Param.ErrorTable.Count; i++)
                        {
                            var Diff = Math.Abs((Param.ErrorTable[i].ZHeightOfPlane.Value - ZValue));

                            if (i == 0)
                            {
                                minval = Diff;
                            }
                            else
                            {
                                if (Diff < minval)
                                {
                                    minval = Diff;
                                    index = i;
                                }
                            }
                        }

                        retval = Param.ErrorTable[index];
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                        throw;
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public double? GetZValue(Tuple<Point3D, double> eq, double x, double y)
        {
            double? retval = null;

            try
            {
                retval = (eq.Item2 - eq.Item1.X * x - eq.Item1.Y * y) / eq.Item1.Z;
            }
            catch (Exception err)
            {
                retval = null;
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private Tuple<Point3D, double> GetEquation(Point3D p1, Point3D p2, Point3D p3)
        {
            Tuple<Point3D, double> retval = null;

            try
            {
                // Create 2 vectors by subtracting p3 from p1 and p2
                Point3D v1 = new Point3D(p1.X - p3.X, p1.Y - p3.Y, p1.Z - p3.Z);
                Point3D v2 = new Point3D(p2.X - p3.X, p2.Y - p3.Y, p2.Z - p3.Z);

                // Create cross product from the 2 vectors
                Point3D abc = new Point3D(v1.Y * v2.Z - v1.Z * v2.Y, v1.Z * v2.X - v1.X * v2.Z, v1.X * v2.Y - v1.Y * v2.X);

                // find d in the equation aX + bY + cZ = d
                double d = abc.X * p3.X + abc.Y * p3.Y + abc.Z * p3.Z;

                retval = new Tuple<Point3D, double>(abc, d);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private double GetDistance2D(double x1, double x2, double y1, double y2)
        {
            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }

        //private Tuple<Point3D, Point3D, Point3D> GetThreePointsNearTheInputPos(double x, double y, PZErrorParameter table)
        //{
        //    Tuple<Point3D, Point3D, Point3D> retval = null;

        //    try
        //    {
        //        var closestToOrigin = table.Positions.OrderBy(p => GetDistance2D(x, p.X.Value, y, p.Y.Value)).ToList();

        //        Point3D p1 = new Point3D(closestToOrigin[0].X.Value, closestToOrigin[0].Y.Value, closestToOrigin[0].Z.Value);
        //        Point3D p2 = new Point3D(closestToOrigin[1].X.Value, closestToOrigin[1].Y.Value, closestToOrigin[1].Z.Value);
        //        Point3D p3 = new Point3D(closestToOrigin[2].X.Value, closestToOrigin[2].Y.Value, closestToOrigin[2].Z.Value);

        //        retval = new Tuple<Point3D, Point3D, Point3D>(p1, p2, p3);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        private Tuple<Point3D, Point3D, Point3D, bool> GetThreePointsNearTheInputPos(double x, double y, PZErrorParameter table)
        {
            Tuple<Point3D, Point3D, Point3D, bool> retval = null;

            Point3D p1 = new Point3D();
            Point3D p2 = new Point3D();
            Point3D p3 = new Point3D();

            bool bfound = false;

            try
            {
                double[] pointD = new double[4];

                pointD[0] = 0;
                pointD[1] = 0;
                pointD[2] = 0;

                PinCoordinate[] tmpPoint = new PinCoordinate[3];

                tmpPoint[0] = new PinCoordinate();
                tmpPoint[1] = new PinCoordinate();
                tmpPoint[2] = new PinCoordinate();

                double degree1 = 0.0;
                double degree2 = 0.0;
                double degree3 = 0.0;

                int i, j;

                /*                
                HeightProfile 함수 사용시 주의사항 !!!

                현재 위치에서 구하고자 하는 높이가 있을 때 사용하는데 만약 현재 위치를 포함하는 3점을 구할 수 없는 경우 즉, 높이를 알고있는 영역 바깥의 높이를
                알고자 하는 경우에 어설프게 평면의 방정식을 사용해서 높이를 구했다가는 엄청 뻥튀기 된 값이 나올 수 있다.

                1) 리스트에 존재하는 높이 값들이 모두 가까운 한 점에 모여 있는 경우, 즉 평면의 넓이가 너무 작은 경우
                2) 평면에서부터 현재 위치까지의 거리가 너무 먼 경우
                3) 구해진 평면이 정삼각형에 가깝지 않고 매우 얇고 길게 직선에 가까운 모양일 경우 (즉 X나 Y 둘중 하나가 매우 작은 경우)

                높이가 저장되는 리스트의 각 항목의 X/Y 위치가 일정치 않다고 가정하면 위의 각 경우에 대해서 항상 적합한 적당한 리미트를 설정하는것이 매우 어렵기 때문에
                여기에서는 현재 위치를 포함하는 평면이 존재하지 않을 경우 평면의 방정식을 사용하지 않고 그냥 가까운 한 점의 높이를 구하여 그 값을 사용한다.

                따라서 올바르게 높이를 구하기 위해서는 반드시 '항상' 현재 위치를 포함하는 평면이 나와야 한다.
                그렇기 때문에 이 함수가 불리기 전에 반드시 '항상' 현재 위치를 포함하는 평면이 존재하도록 리스트가 초기화 되어 있어야 한다.

                값을 초기화 할 때에는 사용 가능한 실제 X/Y 영역보다 훨씬 큰 영역에 대해 널널하게 먼 영역을 지정해 주도록 한다.
                그래야 실제 포커싱 한 영역 바로 바깥 부분의 높이를 계산할 때 높이 값이 외곽부분에서 급격하게 변하는 것을 막을 수 있다.
                예를 들어 웨이퍼 얼라인 높이를 설정하는 경우, 실제 사용가능한 넓이(= 반지름 150,000인 원의 넓이)보다 2배 이상 큰 영역을 모두 포함하는 8개의 포인트 높이를
                지정하여 사용한다.
                
                +                        +                          + <-- 가운데 X의 높이를 알게 되는 순간 십자 위치에 각 높이 지정을 미리 해둔다


                                         
                                     ########  <-- 실제 사용 영역
                                    ##########
                                   ###### #####
                +                 ###### X #####                    +
                                   ###### #####
                                    ##########
                                     ########



                +                        +                          +


                */

                var closestToOrigin = table.Positions.OrderBy(p => GetDistance2D(x, p.X.Value, y, p.Y.Value)).ToList();

                // 1. 가장 가까운 점을 고른다

                tmpPoint[0].X.Value = closestToOrigin[0].X.Value;
                tmpPoint[0].Y.Value = closestToOrigin[0].Y.Value;
                tmpPoint[0].Z.Value = closestToOrigin[0].Z.Value;

                // 2. 현재 위치와 첫 번째 점 사이의 각도를 구한다.
                degree1 = ((Math.Atan2(tmpPoint[0].Y.Value - y, tmpPoint[0].X.Value - x)) * (180 / Math.PI));
                if (degree1 < 0) degree1 = 360 + degree1;

                // 사용 가능한 영역에 점이 존재하는 지 확인하기 위해 반대편 각도 영역을 설정한다.
                degree1 = degree1 + 180;
                if (degree1 >= 360) degree1 = degree1 - 360;

                for (i = 1; i <= closestToOrigin.Count - 1; i++)
                {
                    if (bfound == true) break;

                    degree2 = (Math.Atan2(closestToOrigin[i].Y.Value - y, closestToOrigin[i].X.Value - x)) * (180 / Math.PI);

                    if (degree2 < 0) degree2 = 360 + degree2;
                    // 사용 가능한 영역에 점이 존재하는 지 확인하기 위해 반대편 각도 영역을 설정한다.
                    degree2 = degree2 + 180;
                    if (degree2 >= 360) degree2 = degree2 - 360;

                    // 3. 세 번째의 점을 골라 필요한 영역에 존재하는 지 확인한다.
                    for (j = i + 1; j <= closestToOrigin.Count - 1; j++)
                    {
                        degree3 = (Math.Atan2(closestToOrigin[j].Y.Value - y, closestToOrigin[j].X.Value - x)) * (180 / Math.PI);
                        if (degree3 < 0) degree3 = 360 + degree3;

                        // 첫번째 고른 점과 두번째 고른 점의 각도 차이가 180도 이상 발생한다는 뜻은 세번째 점을 고를 때 360도를 넘어서 존재할 수 있다는 뜻이다. 따라서 조건식에 주의해야 한다.
                        if (Math.Abs(degree2 - degree1) < 180)
                        {
                            if (degree2 > degree1)
                            {
                                if (degree3 > degree1 && degree3 < degree2)
                                {
                                    tmpPoint[1].X.Value = closestToOrigin[i].X.Value;
                                    tmpPoint[1].Y.Value = closestToOrigin[i].Y.Value;
                                    tmpPoint[1].Z.Value = closestToOrigin[i].Z.Value;

                                    tmpPoint[2].X.Value = closestToOrigin[j].X.Value;
                                    tmpPoint[2].Y.Value = closestToOrigin[j].Y.Value;
                                    tmpPoint[2].Z.Value = closestToOrigin[j].Z.Value;
                                    bfound = true;
                                    break;
                                }
                            }
                            else
                            {
                                if (degree3 > degree2 && degree3 < degree1)
                                {
                                    tmpPoint[1].X.Value = closestToOrigin[i].X.Value;
                                    tmpPoint[1].Y.Value = closestToOrigin[i].Y.Value;
                                    tmpPoint[1].Z.Value = closestToOrigin[i].Z.Value;

                                    tmpPoint[2].X.Value = closestToOrigin[j].X.Value;
                                    tmpPoint[2].Y.Value = closestToOrigin[j].Y.Value;
                                    tmpPoint[2].Z.Value = closestToOrigin[j].Z.Value;
                                    bfound = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (degree2 > degree1)
                            {
                                if ((degree3 > degree2 && degree3 < 360) || (degree3 < degree1))
                                {
                                    tmpPoint[1].X.Value = closestToOrigin[i].X.Value;
                                    tmpPoint[1].Y.Value = closestToOrigin[i].Y.Value;
                                    tmpPoint[1].Z.Value = closestToOrigin[i].Z.Value;

                                    tmpPoint[2].X.Value = closestToOrigin[j].X.Value;
                                    tmpPoint[2].Y.Value = closestToOrigin[j].Y.Value;
                                    tmpPoint[2].Z.Value = closestToOrigin[j].Z.Value;
                                    bfound = true;
                                    break;
                                }
                            }
                            else
                            {
                                if ((degree3 > degree1 && degree3 < 360) || (degree3 < degree2))
                                {
                                    tmpPoint[1].X.Value = closestToOrigin[i].X.Value;
                                    tmpPoint[1].Y.Value = closestToOrigin[i].Y.Value;
                                    tmpPoint[1].Z.Value = closestToOrigin[i].Z.Value;

                                    tmpPoint[2].X.Value = closestToOrigin[j].X.Value;
                                    tmpPoint[2].Y.Value = closestToOrigin[j].Y.Value;
                                    tmpPoint[2].Z.Value = closestToOrigin[j].Z.Value;
                                    bfound = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (bfound == false)
                {
                    // 현재 포인트를 포함하는 삼각형이 존재하지 않는다. 그냥 가까운거 한 점의 높이를 리턴한다.
                    // (현재 위치를 포함하는 평면이 존재하지 않으므로 평면의 거리가 멀거나 매우 얇은 평면이거나 할 경우
                    // 높이값이 엄청 뻥튀기 될 위험이 있으므로 평면의 방정식을 돌리지 않고 그냥 가까운 점의 높이를 리턴하여 사용한다)

                    tmpPoint[1].X.Value = closestToOrigin[1].X.Value;
                    tmpPoint[1].Y.Value = closestToOrigin[1].Y.Value;
                    tmpPoint[1].Z.Value = closestToOrigin[1].Z.Value;

                    tmpPoint[2].X.Value = closestToOrigin[2].X.Value;
                    tmpPoint[2].Y.Value = closestToOrigin[2].Y.Value;
                    tmpPoint[2].Z.Value = closestToOrigin[2].Z.Value;
                }

                p1 = new Point3D(tmpPoint[0].X.Value, tmpPoint[0].Y.Value, tmpPoint[0].Z.Value);
                p2 = new Point3D(tmpPoint[1].X.Value, tmpPoint[1].Y.Value, tmpPoint[1].Z.Value);
                p3 = new Point3D(tmpPoint[2].X.Value, tmpPoint[2].Y.Value, tmpPoint[2].Z.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            retval = new Tuple<Point3D, Point3D, Point3D, bool>(p1, p2, p3, bfound);

            return retval;
        }

        private double GetErrorValue(double UpperZ, double LowerZ, double InputZ)
        {
            double retval = 0;

            try
            {
                double interval = UpperZ - LowerZ;

                double m = UpperZ - InputZ;

                retval = UpperZ - (interval * (m / interval));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public double GetPZErrorComp(double x, double y, double z)
        {
            double retval = 0;
            try
            {
                // 예외 처리 추가 해야 됨.

                // Case 1. 평면 획득 과정에서, 1개 또는 0개가 획득 된 경우
                // Case 2. 테이블에서 해당 점(X,Y)가 속하는 평면이 이루어지지 않는 경우 ( 존재 함?)
                // Case 3. 평면에 등록 된 점의 수가 적은 경우 (?)

                // (1) 입력받은 Z값을 기준으로 위, 아래 두 개의 평면(A, B) 획득
                // (2) 획득 된, 각 평면의 방정식 획득.
                // (3) 입력받은 X, Y를 대입하여 두 개의 Z값 획득 (A-Z value, B-Z value)
                // (4) (3)에서 획득 된, 두 개의 Z 값을 보간하여 보상값 획득 

                PZErrorParameter Plane = GetTable(z);

                Tuple<Point3D, Point3D, Point3D, bool> A = null;

                Tuple<Point3D, double> A_eq = null;

                double? offsetZ = null;

                // Normal Case
                if (Plane != null)
                {
                    if ((Plane.Positions.Count >= 3))
                    {
                        A = GetThreePointsNearTheInputPos(x, y, Plane);

                        if (A.Item4 == false)
                        {
                            retval = A.Item1.Z;
                        }
                        else
                        {
                            if (A != null)
                            {
                                A_eq = GetEquation(A.Item1, A.Item2, A.Item3);

                                if (A_eq != null)
                                {
                                    offsetZ = GetZValue(A_eq, x, y);

                                    var abslimit = Math.Abs(Param.MaxLimit.Value);

                                    if (Math.Abs((double)offsetZ) > abslimit)
                                    {
                                        if (offsetZ < 0)
                                        {
                                            offsetZ = -abslimit;
                                        }
                                        else
                                        {
                                            offsetZ = abslimit;
                                        }
                                    }

                                    retval = (double)offsetZ;
                                }
                            }
                        }
                    }
                    else
                    {
                        // 평면에 속한 포인트의 개수가 3개 미만

                        retval = 0;
                    }
                }
                else
                {
                    // 두 개의 평면 데이터가 존재하지 않음.
                    retval = 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        //public double GetPZErrorComp(double x, double y, double z)
        //{
        //    double retval = 0;

        //    try
        //    {
        //        // 예외 처리 추가 해야 됨.

        //        // Case 1. 평면 획득 과정에서, 1개 또는 0개가 획득 된 경우
        //        // Case 2. 테이블에서 해당 점(X,Y)가 속하는 평면이 이루어지지 않는 경우 ( 존재 함?)
        //        // Case 3. 평면에 등록 된 점의 수가 적은 경우 (?)

        //        // (1) 입력받은 Z값을 기준으로 위, 아래 두 개의 평면(A, B) 획득
        //        // (2) 획득 된, 각 평면의 방정식 획득.
        //        // (3) 입력받은 X, Y를 대입하여 두 개의 Z값 획득 (A-Z value, B-Z value)
        //        // (4) (3)에서 획득 된, 두 개의 Z 값을 보간하여 보상값 획득 

        //        PZErrorParameter UpperPlane = GetTable(z, ZDirection.UP);
        //        PZErrorParameter LowerPlane = GetTable(z, ZDirection.DOWN);

        //        Tuple<Point3D, Point3D, Point3D> A = null;
        //        Tuple<Point3D, Point3D, Point3D> B = null;

        //        Tuple<Point3D, double> A_eq = null;
        //        Tuple<Point3D, double> B_eq = null;

        //        double? UpperZValue = null;
        //        double? LowerZValue = null;

        //        // Normal Case
        //        if ((UpperPlane != null) && (LowerPlane != null))
        //        {
        //            if ((UpperPlane.Positions.Count >= 3) && (LowerPlane.Positions.Count >= 3))
        //            {
        //                A = GetThreePointsNearTheInputPos(x, y, UpperPlane);
        //                B = GetThreePointsNearTheInputPos(x, y, LowerPlane);

        //                if (A != null)
        //                {
        //                    A_eq = GetEquation(A.Item1, A.Item2, A.Item3);

        //                    if (A_eq != null)
        //                    {
        //                        UpperZValue = GetZValue(A_eq, x, y);
        //                    }
        //                }

        //                if (B != null)
        //                {
        //                    B_eq = GetEquation(B.Item1, B.Item2, B.Item3);

        //                    if (B_eq != null)
        //                    {
        //                        LowerZValue = GetZValue(B_eq, x, y);
        //                    }
        //                }

        //                if( (UpperZValue != null) && (UpperZValue != null) )
        //                {
        //                    retval = GetErrorValue((double)UpperZValue, (double)LowerZValue, z);
        //                }
        //            }
        //            else
        //            {
        //                // 평면에 속한 포인트의 개수가 3개 미만
        //            }
        //        }
        //        else
        //        {
        //            // 두 개의 평면 데이터가 존재하지 않음.
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
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

        private IParam _NeedleCleanHeightProfilingParameter;
        public IParam NeedleCleanHeightProfilingParameter
        {
            get { return _NeedleCleanHeightProfilingParameter; }
            set
            {
                if (value != _NeedleCleanHeightProfilingParameter)
                {
                    _NeedleCleanHeightProfilingParameter = value;
                    RaisePropertyChanged();
                }
            }
        }

        private NeedleCleanHeightProfilingParameter _Param;
        public NeedleCleanHeightProfilingParameter Param
        {
            get { return _Param; }
            set
            {
                if (value != _Param)
                {
                    _Param = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                IParam tmpParam = null;
                tmpParam = new NeedleCleanHeightProfilingParameter();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(NeedleCleanHeightProfilingParameter));

                if (RetVal == EventCodeEnum.NONE)
                {
                    NeedleCleanHeightProfilingParameter = tmpParam;
                    Param = NeedleCleanHeightProfilingParameter as NeedleCleanHeightProfilingParameter;
                }

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
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = this.SaveParameter(NeedleCleanHeightProfilingParameter);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
}
