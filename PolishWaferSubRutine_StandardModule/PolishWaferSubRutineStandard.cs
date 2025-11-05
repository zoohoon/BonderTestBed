using System;
using System.Collections.Generic;
using System.Linq;

namespace PolishWaferSubRutine_StandardModule
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Param;
    using ProberInterfaces.PolishWafer;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;

    public class PolishWaferSubRutineStandard : IPolishWaferSubRutineStandard, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //..IModule
        public bool Initialized { get; set; }
        public void DeInitModule()
        {
            return;
        }
        public EventCodeEnum InitModule()
        {
            return EventCodeEnum.NONE;
        }
        #endregion

        #region //..ISubRutine
        public EventCodeEnum ParamValidation()
        {
            return EventCodeEnum.NONE;
        }

        public bool IsParameterChanged(bool issave = false)
        {
            return false;
        }
        #endregion

        public PolishWaferSubRutineStandard()
        {

        }


        //public EventCodeEnum SelectIntervalWafer()
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    try
        //    {

        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    return retVal;
        //}


        //public EventCodeEnum LoadPolishWafer(string definetype)
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    try
        //    {
        //        (this.PolishWaferModule().PolishWaferParameter as IPolishWaferParameter).NeedLoadWaferFlag = true;
        //        (this.PolishWaferModule().PolishWaferParameter as IPolishWaferParameter).LoadWaferType = definetype;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    return retVal;
        //}


        //public EventCodeEnum UnLoadPolishWafer()
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    try
        //    {
        //        // TODO : 올바른 정보를 넣어주고 사용 되어야 함. WaferObject가 갖고 있는 PolishWaferInformation

        //        if(this.StageSupervisor().WaferObject.GetState() != EnumWaferState.READY)
        //        {
        //            this.StageSupervisor().WaferObject.SetReady();
        //        }

        //        //if (this.StageSupervisor().WaferObject.GetPolishInfo().MaxLimitCount.Value >= this.StageSupervisor().WaferObject.GetPolishInfo().TouchCount.Value)
        //        //    this.StageSupervisor().WaferObject.SetProcessed();
        //        //else
        //        //    this.StageSupervisor().WaferObject.SetReady();

        //        retVal = EventCodeEnum.NONE;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retVal;
        //}

        #region //..Height Profiling

        private ObservableCollection<WaferCoordinate> _PlanPoints = new ObservableCollection<WaferCoordinate>();
        public ObservableCollection<WaferCoordinate> PlanPoints
        {
            get { return _PlanPoints; }
            set
            {
                if (value != _PlanPoints)
                {
                    _PlanPoints = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool CheckHeightPosition()
        {
            bool retVal = false;
            try
            {
                IWaferObject Wafer = this.StageSupervisor().WaferObject;
                double totaly = 0.0;

                foreach (var position in PlanPoints)
                {
                    totaly += Math.Abs(position.GetY());
                }
                totaly = totaly / PlanPoints.Count;

                List<WaferCoordinate> waferCoordinates = PlanPoints.ToList<WaferCoordinate>();

                waferCoordinates.Sort(delegate (WaferCoordinate wc_ccord1, WaferCoordinate wc_coord2)
                {
                    if (wc_ccord1 != null & wc_coord2 != null)
                    {
                        if (Distance2D(Wafer.GetSubsInfo().WaferCenter.GetX(), Wafer.GetSubsInfo().WaferCenter.GetY(), wc_ccord1.X.Value, wc_ccord1.Y.Value)
                            > Distance2D(Wafer.GetSubsInfo().WaferCenter.GetX(), Wafer.GetSubsInfo().WaferCenter.GetY(), wc_coord2.X.Value, wc_coord2.Y.Value)) return 1;
                        if (Distance2D(Wafer.GetSubsInfo().WaferCenter.GetX(), Wafer.GetSubsInfo().WaferCenter.GetY(), wc_ccord1.X.Value, wc_ccord1.Y.Value)
                            < Distance2D(Wafer.GetSubsInfo().WaferCenter.GetX(), Wafer.GetSubsInfo().WaferCenter.GetY(), wc_coord2.X.Value, wc_coord2.Y.Value)) return -1;
                    }
                    return 0;
                });

                if (totaly > (waferCoordinates[0].GetY() + (Wafer.GetSubsInfo().ActualDieSize.Height.Value * 2)))
                    retVal = true;
                //Test Code
                retVal = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private double Distance2D(double X1, double Y1, double X2, double Y2)
        {
            return Math.Sqrt((X2 - X1) * (X2 - X1) + (Y2 - Y1) * (Y2 - Y1));
        }

        public double GetHeightValue(double posX, double posY)
        {
            double retVal = -1;
            //double retVal = _CleaningParameter.CleaningParam.PhysicalParam.ActualThickness;

            try
            {
                if (PlanPoints != null && (posX != 0 || posY != 0))
                {
                    if (PlanPoints.Count != 0 && !(PlanPoints.Count < 5))
                    {
                        if (!CheckHeightPosition())
                            return retVal;

                        double[] pointD = new double[4];
                        pointD[0] = 0;
                        pointD[1] = 0;
                        pointD[2] = 0;

                        WaferCoordinate[] tmpPoint = new WaferCoordinate[3];

                        tmpPoint[0] = new WaferCoordinate();
                        tmpPoint[1] = new WaferCoordinate();
                        tmpPoint[2] = new WaferCoordinate();

                        double degree1 = 0.0;
                        double degree2 = 0.0;
                        double degree3 = 0.0;

                        int i, j;
                        bool bfound = false;

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

                        List<WaferCoordinate> waferCoordinates = PlanPoints.ToList<WaferCoordinate>();

                        // 리스트 내의 점들의 위치를 현재 위치에서 가까운 순으로 정렬한다.
                        waferCoordinates.Sort(delegate (WaferCoordinate wc_ccord1, WaferCoordinate wc_coord2)
                        {
                            if (Distance2D(posX, posY, wc_ccord1.X.Value, wc_ccord1.Y.Value) > Distance2D(posX, posY, wc_coord2.X.Value, wc_coord2.Y.Value)) return 1;
                            if (Distance2D(posX, posY, wc_ccord1.X.Value, wc_ccord1.Y.Value) < Distance2D(posX, posY, wc_coord2.X.Value, wc_coord2.Y.Value)) return -1;
                            return 0;
                        });

                        PlanPoints.Clear();
                        foreach (var point in waferCoordinates)
                        {
                            PlanPoints.Add(point);
                        }

                        // 1. 가장 가까운 점을 고른다
                        tmpPoint[0].X.Value = PlanPoints[0].X.Value;
                        tmpPoint[0].Y.Value = PlanPoints[0].Y.Value;
                        tmpPoint[0].Z.Value = PlanPoints[0].Z.Value;


                        // 2. 현재 위치와 첫 번째 점 사이의 각도를 구한다.
                        degree1 = ((Math.Atan2(tmpPoint[0].Y.Value - posY, tmpPoint[0].X.Value - posX)) * (180 / Math.PI));
                        if (degree1 < 0) degree1 = 360 + degree1;

                        // 사용 가능한 영역에 점이 존재하는 지 확인하기 위해 반대편 각도 영역을 설정한다.
                        degree1 = degree1 + 180;
                        if (degree1 >= 360) degree1 = degree1 - 360;

                        for (i = 1; i <= PlanPoints.Count - 1; i++)
                        {
                            if (bfound == true) break;

                            degree2 = (Math.Atan2(PlanPoints[i].Y.Value - posY, PlanPoints[i].X.Value - posX)) * (180 / Math.PI);
                            if (degree2 < 0) degree2 = 360 + degree2;
                            // 사용 가능한 영역에 점이 존재하는 지 확인하기 위해 반대편 각도 영역을 설정한다.
                            degree2 = degree2 + 180;
                            if (degree2 >= 360) degree2 = degree2 - 360;

                            // 3. 세 번째의 점을 골라 필요한 영역에 존재하는 지 확인한다.
                            for (j = i + 1; j <= PlanPoints.Count - 1; j++)
                            {
                                degree3 = (Math.Atan2(PlanPoints[j].Y.Value - posY, PlanPoints[j].X.Value - posX)) * (180 / Math.PI);
                                if (degree3 < 0) degree3 = 360 + degree3;

                                // 첫번째 고른 점과 두번째 고른 점의 각도 차이가 180도 이상 발생한다는 뜻은 세번째 점을 고를 때 360도를 넘어서 존재할 수 있다는 뜻이다. 따라서 조건식에 주의해야 한다.
                                if (Math.Abs(degree2 - degree1) < 180)
                                {
                                    if (degree2 > degree1)
                                    {
                                        if (degree3 > degree1 && degree3 < degree2)
                                        {
                                            tmpPoint[1].X.Value = PlanPoints[i].X.Value;
                                            tmpPoint[1].Y.Value = PlanPoints[i].Y.Value;
                                            tmpPoint[1].Z.Value = PlanPoints[i].Z.Value;

                                            tmpPoint[2].X.Value = PlanPoints[j].X.Value;
                                            tmpPoint[2].Y.Value = PlanPoints[j].Y.Value;
                                            tmpPoint[2].Z.Value = PlanPoints[j].Z.Value;
                                            bfound = true;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (degree3 > degree2 && degree3 < degree1)
                                        {
                                            tmpPoint[1].X.Value = PlanPoints[i].X.Value;
                                            tmpPoint[1].Y.Value = PlanPoints[i].Y.Value;
                                            tmpPoint[1].Z.Value = PlanPoints[i].Z.Value;

                                            tmpPoint[2].X.Value = PlanPoints[j].X.Value;
                                            tmpPoint[2].Y.Value = PlanPoints[j].Y.Value;
                                            tmpPoint[2].Z.Value = PlanPoints[j].Z.Value;
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
                                            tmpPoint[1].X.Value = PlanPoints[i].X.Value;
                                            tmpPoint[1].Y.Value = PlanPoints[i].Y.Value;
                                            tmpPoint[1].Z.Value = PlanPoints[i].Z.Value;

                                            tmpPoint[2].X.Value = PlanPoints[j].X.Value;
                                            tmpPoint[2].Y.Value = PlanPoints[j].Y.Value;
                                            tmpPoint[2].Z.Value = PlanPoints[j].Z.Value;
                                            bfound = true;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if ((degree3 > degree1 && degree3 < 360) || (degree3 < degree2))
                                        {
                                            tmpPoint[1].X.Value = PlanPoints[i].X.Value;
                                            tmpPoint[1].Y.Value = PlanPoints[i].Y.Value;
                                            tmpPoint[1].Z.Value = PlanPoints[i].Z.Value;

                                            tmpPoint[2].X.Value = PlanPoints[j].X.Value;
                                            tmpPoint[2].Y.Value = PlanPoints[j].Y.Value;
                                            tmpPoint[2].Z.Value = PlanPoints[j].Z.Value;
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
                            return tmpPoint[0].Z.Value;

                            //tmpPoint[1].X.Value = Wafer.Info.WaferHeightMapping.PlanPoints[1].X.Value;
                            //tmpPoint[1].Y.Value = Wafer.Info.WaferHeightMapping.PlanPoints[1].Y.Value;
                            //tmpPoint[1].Z.Value = Wafer.Info.WaferHeightMapping.PlanPoints[1].Z.Value;

                            //tmpPoint[2].X.Value = Wafer.Info.WaferHeightMapping.PlanPoints[2].X.Value;
                            //tmpPoint[2].Y.Value = Wafer.Info.WaferHeightMapping.PlanPoints[2].Y.Value;
                            //tmpPoint[2].Z.Value = Wafer.Info.WaferHeightMapping.PlanPoints[2].Z.Value;
                        }

                        pointD[0] = tmpPoint[0].Y.Value * (tmpPoint[1].Z.Value - tmpPoint[2].Z.Value) +
                                    tmpPoint[1].Y.Value * (tmpPoint[2].Z.Value - tmpPoint[0].Z.Value) +
                                    tmpPoint[2].Y.Value * (tmpPoint[0].Z.Value - tmpPoint[1].Z.Value);

                        pointD[1] = tmpPoint[0].Z.Value * (tmpPoint[1].X.Value - tmpPoint[2].X.Value) +
                                    tmpPoint[1].Z.Value * (tmpPoint[2].X.Value - tmpPoint[0].X.Value) +
                                    tmpPoint[2].Z.Value * (tmpPoint[0].X.Value - tmpPoint[1].X.Value);

                        pointD[2] = tmpPoint[0].X.Value * (tmpPoint[1].Y.Value - tmpPoint[2].Y.Value) +
                                    tmpPoint[1].X.Value * (tmpPoint[2].Y.Value - tmpPoint[0].Y.Value) +
                                    tmpPoint[2].X.Value * (tmpPoint[0].Y.Value - tmpPoint[1].Y.Value);

                        pointD[3] = -tmpPoint[0].X.Value * ((tmpPoint[1].Y.Value * tmpPoint[2].Z.Value) - (tmpPoint[2].Y.Value * tmpPoint[1].Z.Value)) -
                                     tmpPoint[1].X.Value * ((tmpPoint[2].Y.Value * tmpPoint[0].Z.Value) - (tmpPoint[0].Y.Value * tmpPoint[2].Z.Value)) -
                                     tmpPoint[2].X.Value * ((tmpPoint[0].Y.Value * tmpPoint[1].Z.Value) - (tmpPoint[1].Y.Value * tmpPoint[0].Z.Value));

                        if (pointD[2] != 0)
                        {
                            double ret = -(pointD[0] * posX + pointD[1] * posY + pointD[3]) / pointD[2] ;
                            if (ret > 1500)
                            {
                                //ret = _CleaningParameter.CleaningParam.PhysicalParam.ActualThickness;
                            }

                            double tiltedZ = this.WaferAligner().CalcThreePodTiltedPlane(posX, posY, true);
                            LoggerManager.Debug($"WaferAligner() - GetHeightValue() : ProfilingPlane Z = {ret:0.00} + TiltedPlane Z = {tiltedZ:0.00} for (X,Y) = ({posX:0.00},{posY:0.00})");
                            ret = ret + tiltedZ;
                            LoggerManager.Debug($"WaferAligner() - GetHeightValue() : Get Height Z = {ret:0.00} for (X,Y) = ({posX:0.00},{posY:0.00})");
                            return ret;
                        }
                        else
                        {
                            //Exception
                            //return _CleaningParameter.CleaningParam.PhysicalParam.ActualThickness + 1000;
                        }
                    }
                }
                else
                {
                    return retVal;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Exception(err);
                LoggerManager.Debug($"{err.ToString()}. PolishWaferModule() - GetHeightValue() : Error occured.");
                return 0;
            }

            return retVal;
        }

        public void AddHeighPlanePoint(WaferCoordinate param = null)
        { // 안쓰이는 부분인듯
            try
            {
                //double radius = 0.0;
                //double TiltedZ = 0.0;
                WaferCoordinate PlanePoint = new WaferCoordinate();
                if (param == null)
                {
                    PlanePoint = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                }
                else
                {
                    param.CopyTo(PlanePoint);
                }

                if (PlanPoints != null)
                {
                    for (int i = 0; i < PlanPoints.Count; i++)
                    {
                        double pointdist =
                            Point.Subtract(new Point(PlanPoints[i].X.Value,
                            PlanPoints[i].Y.Value),
                            new Point(PlanePoint.X.Value, PlanePoint.Y.Value)).Length;
                        //if (pointdist < radius)
                        //{
                        //    PlanPoints.RemoveAt(i);
                        //}
                    }
                }


                //TiltedZ = this.WaferAligner().CalcThreePodTiltedPlane(PlanePoint.GetX(), PlanePoint.GetY(), true);
                //LoggerManager.Debug($"WaferAligner() - AddHeighPlanePoint() : PlanePoint Z = {PlanePoint.GetZ():0.00} - TiltedPlane Z = {TiltedZ:0.00} for (X,Y) = ({PlanePoint.GetX():0.00},{PlanePoint.GetY():0.00})");
                //PlanePoint.Z.Value = PlanePoint.Z.Value - TiltedZ;

                PlanPoints.Add(PlanePoint);

                LoggerManager.Debug($"AddHeightPlanePoint to Cleaning. X : {PlanePoint.GetX()}, Y : {PlanePoint.GetY():0.00}, Z : {PlanePoint.GetZ():0.00}");
                LoggerManager.Debug($"TotalHeightPoint to Cleaning : {PlanPoints.Count}");
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. CleaningModule(Polish Wafer Module) - AddHeighPlanePoint() : Error occured. ");
            }
        }

        public void AddOutSideHeightPlanePoint(double zpos = -1.0)
        {
            try
            {
                WaferCoordinate PlanePoint = new WaferCoordinate();
                if (zpos == -1.0)
                    PlanePoint = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                else
                    PlanePoint = new WaferCoordinate(
                        this.StageSupervisor().WaferObject.GetSubsInfo().WaferCenter.GetX(),
                        this.StageSupervisor().WaferObject.GetSubsInfo().WaferCenter.GetY(),
                        this.StageSupervisor().WaferObject.GetSubsInfo().ActualThickness);

                PlanPoints.Add(PlanePoint);

                //_CleaningParameter.CleaningParam.PhysicalParam.ActualThickness = PlanePoint.GetX();
                //Outside
                double distance = 500000;
                double edgepos = 0.0;
                edgepos = ((distance / 2) / Math.Sqrt(2));

                //Left
                PlanPoints.Add(new WaferCoordinate(
                    PlanePoint.GetX() - (distance / 2), PlanePoint.GetY(), PlanePoint.GetZ()));
                //Right
                PlanPoints.Add(new WaferCoordinate(
                    PlanePoint.GetX() + (distance / 2), PlanePoint.GetY(), PlanePoint.GetZ()));
                //Upper
                PlanPoints.Add(new WaferCoordinate(
                    PlanePoint.GetX(), PlanePoint.GetY() + (distance / 2), PlanePoint.GetZ()));
                //Lower
                PlanPoints.Add(new WaferCoordinate(
                    PlanePoint.GetX(), PlanePoint.GetY() - (distance / 2), PlanePoint.GetZ()));
                //LeftUpper
                PlanPoints.Add(new WaferCoordinate(
                    PlanePoint.GetX() - edgepos, PlanePoint.GetY() + edgepos, PlanePoint.GetZ()));
                //RightUpper
                PlanPoints.Add(new WaferCoordinate(
                    PlanePoint.GetX() + edgepos, PlanePoint.GetY() + edgepos, PlanePoint.GetZ()));
                //LeftLower
                PlanPoints.Add(new WaferCoordinate(
                    PlanePoint.GetX() - edgepos, PlanePoint.GetY() - edgepos, PlanePoint.GetZ()));
                //RightLower
                PlanPoints.Add(new WaferCoordinate(
                    PlanePoint.GetX() + edgepos, PlanePoint.GetY() - edgepos, PlanePoint.GetZ()));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        #endregion

        //public bool IsAvailablePwArea(double DutSizeX, double DutSizeY)
        //{
        //    bool retVal = false;
        //    try
        //    {
        //        PolishWaferParameter PWParam = PolishWaferParams_Clone.SelectedPolishWaferParameter;
        //        double tmpdiameter = 0;

        //        if (PWParam != null)
        //        {
        //            tmpdiameter = PWParam.WaferSize.Value / 2 - PWParam.FocusingRangeOffset.Value;

        //            if (DutSizeX > tmpdiameter * 2 || DutSizeY > tmpdiameter * 2)
        //            {
        //                retVal = false;
        //            }
        //            else
        //            {
        //                retVal = true;
        //            }
        //            return retVal;
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    return false;
        //}

        //public EventCodeEnum CalcCleaningArea()
        //{
        //    EventCodeEnum retVal = EventCodeEnum.NONE;
        //    try
        //    {
        //        double tmpdiameter = 0;
        //        //PolishWaferParameter PWParam = PolishWaferParams_Clone.SelectedPolishWaferParameter;
        //        if (PWParam != null)
        //        {
        //            tmpdiameter = (PWParam.WaferSize.Value / 2) - PWParam.FocusingRangeOffset.Value;

        //            PWParam.MaxCleaningArea = tmpdiameter / Math.Sqrt(2);

        //            return retVal;

        //        }
        //        else
        //        {
        //            retVal = EventCodeEnum.NODATA;
        //            return retVal;
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        return EventCodeEnum.EXCEPTION;
        //    }
        //}
    }
}
