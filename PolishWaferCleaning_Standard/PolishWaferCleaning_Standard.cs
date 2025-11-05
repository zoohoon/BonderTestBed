using System;
using System.Collections.Generic;
using System.Linq;

namespace PolishWaferCleaningModule
{
    using LogModule;
    using NotifyEventModule;
    using PolishWaferParameters;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Event;
    using ProberInterfaces.Param;
    using ProberInterfaces.PolishWafer;
    using ProberInterfaces.WaferAlign;
    using SubstrateObjects;
    using System.Threading;

    public class PolishWaferCleaning_Standard : IPolishWaferCleaning
    {

        private PolishWaferParameter PWParam
        {
            get
            {
                return (PolishWaferParameter)this.PolishWaferModule().PolishWaferParameter;
            }
        }

        private WaferObject WaferObject
        {
            get
            {
                return (WaferObject)this.StageSupervisor().WaferObject;
            }
        }


        public PolishWaferCleaning_Standard()
        {

        }

        private void ConverterScrubingDirection(CleaningDirection direction, out int dirx, out int diry)
        {
            try
            {
                switch (direction)
                {
                    case CleaningDirection.Right:
                        dirx = 1;
                        diry = 0;
                        break;
                    case CleaningDirection.Left:
                        dirx = -1;
                        diry = 0;
                        break;
                    case CleaningDirection.Up:
                        dirx = 0;
                        diry = 1;
                        break;
                    case CleaningDirection.Down:
                        dirx = 0;
                        diry = -1;
                        break;
                    case CleaningDirection.Right_Up:
                        dirx = 1;
                        diry = 1;
                        break;
                    case CleaningDirection.Right_Down:
                        dirx = 1;
                        diry = -1;
                        break;
                    case CleaningDirection.Left_Up:
                        dirx = -1;
                        diry = 1;
                        break;
                    case CleaningDirection.Left_Down:
                        dirx = -1;
                        diry = -1;
                        break;
                    default:
                        dirx = 0;
                        diry = 0;
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        private double CalculateZClearenceUsingOD(double overDrive, double zClearence)
        {
            double retZClearence = zClearence;
            try
            {
                var clearence = Math.Abs(zClearence) * -1d;

                if (overDrive < 0 && overDrive < clearence)
                {
                    retZClearence = overDrive + clearence;
                }
                else
                {
                    retZClearence = clearence;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retZClearence;
        }

        private double Distance2D(double X1, double Y1, double X2, double Y2)
        {
            return Math.Sqrt((X2 - X1) * (X2 - X1) + (Y2 - Y1) * (Y2 - Y1));
        }

        private bool CheckHeightPosition(WaferHeightMapping heightmapping)
        {
            bool retVal = false;
            try
            {
                double totaly = 0.0;

                foreach (var position in heightmapping.PlanPoints)
                {
                    totaly += Math.Abs(position.GetY());
                }
                totaly = totaly / heightmapping.PlanPoints.Count;

                List<WaferCoordinate> waferCoordinates = heightmapping.PlanPoints.ToList<WaferCoordinate>();

                waferCoordinates.Sort(delegate (WaferCoordinate wc_ccord1, WaferCoordinate wc_coord2)
                {
                    if (wc_ccord1 != null & wc_coord2 != null)
                    {
                        if (Distance2D(WaferObject.GetSubsInfo().WaferCenter.GetX(), WaferObject.GetSubsInfo().WaferCenter.GetY(), wc_ccord1.X.Value, wc_ccord1.Y.Value)
                            > Distance2D(WaferObject.GetSubsInfo().WaferCenter.GetX(), WaferObject.GetSubsInfo().WaferCenter.GetY(), wc_coord2.X.Value, wc_coord2.Y.Value)) return 1;
                        if (Distance2D(WaferObject.GetSubsInfo().WaferCenter.GetX(), WaferObject.GetSubsInfo().WaferCenter.GetY(), wc_ccord1.X.Value, wc_ccord1.Y.Value)
                            < Distance2D(WaferObject.GetSubsInfo().WaferCenter.GetX(), WaferObject.GetSubsInfo().WaferCenter.GetY(), wc_coord2.X.Value, wc_coord2.Y.Value)) return -1;
                    }
                    return 0;
                });

                //if (totaly > (waferCoordinates[0].GetY() + (Wafer.GetSubsInfo().ActualDieSize.Height.Value * 2)))
                //    retVal = true;

                ////Test Code
                retVal = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public double GetHeightValue(double posX, double posY, double thickness, WaferHeightMapping heightmapping)
        {
            double retVal = thickness;

            try
            {
                if (heightmapping.PlanPoints != null && (posX != 0 || posY != 0))
                {
                    if (heightmapping.PlanPoints.Count != 0 && !(heightmapping.PlanPoints.Count < 5))
                    {
                        if (!CheckHeightPosition(heightmapping))
                        {
                            return thickness;
                        }

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

                        List<WaferCoordinate> waferCoordinates = heightmapping.PlanPoints.ToList<WaferCoordinate>();

                        // 리스트 내의 점들의 위치를 현재 위치에서 가까운 순으로 정렬한다.
                        waferCoordinates.Sort(delegate (WaferCoordinate wc_ccord1, WaferCoordinate wc_coord2)
                        {
                            if (Distance2D(posX, posY, wc_ccord1.X.Value, wc_ccord1.Y.Value) > Distance2D(posX, posY, wc_coord2.X.Value, wc_coord2.Y.Value)) return 1;
                            if (Distance2D(posX, posY, wc_ccord1.X.Value, wc_ccord1.Y.Value) < Distance2D(posX, posY, wc_coord2.X.Value, wc_coord2.Y.Value)) return -1;
                            return 0;
                        });

                        heightmapping.PlanPoints.Clear();

                        foreach (var point in waferCoordinates)
                        {
                            heightmapping.PlanPoints.Add(point);
                        }

                        // 1. 가장 가까운 점을 고른다
                        tmpPoint[0].X.Value = heightmapping.PlanPoints[0].X.Value;
                        tmpPoint[0].Y.Value = heightmapping.PlanPoints[0].Y.Value;
                        tmpPoint[0].Z.Value = heightmapping.PlanPoints[0].Z.Value;


                        // 2. 현재 위치와 첫 번째 점 사이의 각도를 구한다.
                        degree1 = ((Math.Atan2(tmpPoint[0].Y.Value - posY, tmpPoint[0].X.Value - posX)) * (180 / Math.PI));
                        if (degree1 < 0) degree1 = 360 + degree1;

                        // 사용 가능한 영역에 점이 존재하는 지 확인하기 위해 반대편 각도 영역을 설정한다.
                        degree1 = degree1 + 180;
                        if (degree1 >= 360) degree1 = degree1 - 360;

                        for (i = 1; i <= heightmapping.PlanPoints.Count - 1; i++)
                        {
                            if (bfound == true) break;

                            degree2 = (Math.Atan2(heightmapping.PlanPoints[i].Y.Value - posY, heightmapping.PlanPoints[i].X.Value - posX)) * (180 / Math.PI);
                            if (degree2 < 0) degree2 = 360 + degree2;
                            // 사용 가능한 영역에 점이 존재하는 지 확인하기 위해 반대편 각도 영역을 설정한다.
                            degree2 = degree2 + 180;
                            if (degree2 >= 360) degree2 = degree2 - 360;

                            // 3. 세 번째의 점을 골라 필요한 영역에 존재하는 지 확인한다.
                            for (j = i + 1; j <= heightmapping.PlanPoints.Count - 1; j++)
                            {
                                degree3 = (Math.Atan2(heightmapping.PlanPoints[j].Y.Value - posY, heightmapping.PlanPoints[j].X.Value - posX)) * (180 / Math.PI);
                                if (degree3 < 0) degree3 = 360 + degree3;

                                // 첫번째 고른 점과 두번째 고른 점의 각도 차이가 180도 이상 발생한다는 뜻은 세번째 점을 고를 때 360도를 넘어서 존재할 수 있다는 뜻이다. 따라서 조건식에 주의해야 한다.
                                if (Math.Abs(degree2 - degree1) < 180)
                                {
                                    if (degree2 > degree1)
                                    {
                                        if (degree3 > degree1 && degree3 < degree2)
                                        {
                                            tmpPoint[1].X.Value = heightmapping.PlanPoints[i].X.Value;
                                            tmpPoint[1].Y.Value = heightmapping.PlanPoints[i].Y.Value;
                                            tmpPoint[1].Z.Value = heightmapping.PlanPoints[i].Z.Value;

                                            tmpPoint[2].X.Value = heightmapping.PlanPoints[j].X.Value;
                                            tmpPoint[2].Y.Value = heightmapping.PlanPoints[j].Y.Value;
                                            tmpPoint[2].Z.Value = heightmapping.PlanPoints[j].Z.Value;
                                            bfound = true;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (degree3 > degree2 && degree3 < degree1)
                                        {
                                            tmpPoint[1].X.Value = heightmapping.PlanPoints[i].X.Value;
                                            tmpPoint[1].Y.Value = heightmapping.PlanPoints[i].Y.Value;
                                            tmpPoint[1].Z.Value = heightmapping.PlanPoints[i].Z.Value;

                                            tmpPoint[2].X.Value = heightmapping.PlanPoints[j].X.Value;
                                            tmpPoint[2].Y.Value = heightmapping.PlanPoints[j].Y.Value;
                                            tmpPoint[2].Z.Value = heightmapping.PlanPoints[j].Z.Value;
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
                                            tmpPoint[1].X.Value = heightmapping.PlanPoints[i].X.Value;
                                            tmpPoint[1].Y.Value = heightmapping.PlanPoints[i].Y.Value;
                                            tmpPoint[1].Z.Value = heightmapping.PlanPoints[i].Z.Value;

                                            tmpPoint[2].X.Value = heightmapping.PlanPoints[j].X.Value;
                                            tmpPoint[2].Y.Value = heightmapping.PlanPoints[j].Y.Value;
                                            tmpPoint[2].Z.Value = heightmapping.PlanPoints[j].Z.Value;
                                            bfound = true;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if ((degree3 > degree1 && degree3 < 360) || (degree3 < degree2))
                                        {
                                            tmpPoint[1].X.Value = heightmapping.PlanPoints[i].X.Value;
                                            tmpPoint[1].Y.Value = heightmapping.PlanPoints[i].Y.Value;
                                            tmpPoint[1].Z.Value = heightmapping.PlanPoints[i].Z.Value;

                                            tmpPoint[2].X.Value = heightmapping.PlanPoints[j].X.Value;
                                            tmpPoint[2].Y.Value = heightmapping.PlanPoints[j].Y.Value;
                                            tmpPoint[2].Z.Value = heightmapping.PlanPoints[j].Z.Value;
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
                            double ret = -(pointD[0] * posX + pointD[1] * posY + pointD[3]) / pointD[2];
                            if (ret > 1500)
                            {
                                ret = thickness;
                            }
                            //return -(pointD[0] * posX + pointD[1] * posY + pointD[3]) / pointD[2];
                            double tiltedZ = this.WaferAligner().CalcThreePodTiltedPlane(posX, posY, true);
                            LoggerManager.Debug($"WaferAligner() - GetHeightValue() : ProfilingPlane Z = {ret:0.00} + TiltedPlane Z = {tiltedZ:0.00} for (X,Y) = ({posX:0.00},{posY:0.00})", isInfo: true);
                            ret = ret + tiltedZ;
                            LoggerManager.Debug($"WaferAligner() - GetHeightValue() : Get Height Z = {ret:0.00} for (X,Y) = ({posX},{posY:0.00})", isInfo: true);
                            return ret;
                        }
                        else
                        {
                            //Exception
                            return thickness + 1000;
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
                LoggerManager.Debug($"{err.ToString()}. WaferAligner() - GetHeightValue() : Error occured.");
                return 0;
            }

            return retVal;
        }

        public EventCodeEnum DoCleaning(IPolishWaferCleaningParameter param)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            //LoggerManager.Funclog(typeof(PolishWaferCleaning_Standard), EnumFuncCallingTime.START);
            LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Polishwafer_Cleaning_Start);
            IPolishWaferSourceInformation pwinfo = null;

            try
            {
                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                this.EventManager().RaisingEvent(typeof(CleaningStartEvent_PolishWafer).FullName, new ProbeEventArgs(this, semaphore));
                semaphore.Wait();
                //this.GEMModule().SetEvent(this.GEMModule().GetEventNumberFormEventName(typeof(CleaningStartEvent_PolishWafer).FullName));

                PolishWaferCleaningParameter _param = param as PolishWaferCleaningParameter;

                WaferCoordinate Wafer = new WaferCoordinate();
                PinCoordinate pin = new PinCoordinate();
                double zc;

                pwinfo = this.StageSupervisor().WaferObject.GetPolishInfo();

                //var source = PWParam.SourceParameters.Where(x => x.DefineName.Value == param.WaferDefineType.Value).FirstOrDefault();

                double wSize = 0;
                double pwradius = 0;
                double marginradius = 0;

                double overdrive = _param.OverdriveValue.Value;
                double clearance = _param.Clearance.Value;
                if (this.PolishWaferModule().ForcedDone != EnumModuleForcedState.Normal)
                {
                    overdrive = -1000;
                    clearance = this.StageSupervisor().PinMinRegRange;
                }

                switch (pwinfo.Size.Value)
                {
                    case SubstrateSizeEnum.INVALID:
                    case SubstrateSizeEnum.UNDEFINED:
                        break;
                    case SubstrateSizeEnum.INCH6:
                        wSize = 150000;
                        break;
                    case SubstrateSizeEnum.INCH8:
                        wSize = 200000;
                        break;
                    case SubstrateSizeEnum.INCH12:
                        wSize = 300000;
                        break;
                    case SubstrateSizeEnum.CUSTOM:
                        // TODO : 
                        break;
                    default:
                        break;
                }

                if (wSize <= 0)
                {
                    retVal = EventCodeEnum.POLISHWAFER_CLEAING_ERROR;
                    this.NotifyManager().Notify(retVal);
                    LoggerManager.Error($"Polish wafer size is wrong.");

                    return retVal;
                }

                pwradius = wSize / 2d;

                marginradius = pwradius - pwinfo.Margin.Value;

                Wafer.X.Value = pwinfo.PolishWaferCenter.X.Value;
                Wafer.Y.Value = pwinfo.PolishWaferCenter.Y.Value;
                Wafer.Z.Value = 1d * GetHeightValue(Wafer.X.Value, Wafer.Y.Value, pwinfo.Thickness.Value, pwinfo.WaferHeightMapping);

                if (Wafer.Z.Value < pwinfo.Thickness.Value)
                {
                    LoggerManager.Debug($"[PolishWaferCleaning_Standard], DoCleaning(), Wafer Z value = {Wafer.Z.Value}, PolishWafer Thickness = {pwinfo.Thickness.Value}", isInfo:true);
                    Wafer.Z.Value = pwinfo.Thickness.Value;
                    LoggerManager.Debug($"[PolishWaferCleaning_Standard], DoCleaning(), Wafer Z value is changed");
                }

                LoggerManager.Debug($"[PolishWaferCleaning_Standard], DoCleaning(), Wafer Z value = {Wafer.Z.Value}, WaferDefineType = {_param.WaferDefineType.Value}", isInfo: true);

                zc = CalculateZClearenceUsingOD(overdrive, clearance);

                //pin.X.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenX;
                //pin.Y.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenY;
                //pin.Z.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinHeight;

                pin.X.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenX;
                pin.Y.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenY;
                pin.Z.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinHeight;

                retVal = this.StageSupervisor().StageModuleState.MovePadToPin(Wafer, pin, zc);

                if (retVal == EventCodeEnum.NONE)
                {
                    for (int count = 0; count < _param.ContactCount.Value; count++)
                    {
                        if (count != 0)
                        {
                            //첫번째 Contact 이 아니라면
                            if (_param.ContactLength.Value > 0)
                            {
                                //ContactLenth 설정했다면
                                int movedirX = 0;
                                int movedirY = 0;

                                switch (_param.CleaningDirection.Value)
                                {
                                    case CleaningDirection.Right:
                                        movedirX = 1;
                                        movedirY = 0;
                                        break;
                                    case CleaningDirection.Down:
                                        movedirX = 0;
                                        movedirY = -1;
                                        break;
                                    case CleaningDirection.Left:
                                        movedirX = -1;
                                        movedirY = 0;
                                        break;
                                    case CleaningDirection.Left_Down:
                                        movedirX = -1;
                                        movedirY = -1;
                                        break;
                                    case CleaningDirection.Up:
                                        movedirX = 0;
                                        movedirY = 1;
                                        break;
                                    case CleaningDirection.Right_Up:
                                        movedirX = 1;
                                        movedirY = 1;
                                        break;
                                    case CleaningDirection.Right_Down:
                                        movedirX = 1;
                                        movedirY = -1;
                                        break;
                                    case CleaningDirection.Left_Up:
                                        movedirX = -1;
                                        movedirY = 1;
                                        break;
                                    case CleaningDirection.UNDEFIEND:
                                        movedirX = 0;
                                        movedirY = 0;
                                        break;
                                    default:
                                        break;
                                }

                                double MovecontactValue = _param.ContactLength.Value / _param.ContactCount.Value;

                                // 현재 알고 있는 Center 좌표로부터 누적되는 이동 거리
                                Wafer.X.Value += (MovecontactValue * movedirX);
                                Wafer.Y.Value += (MovecontactValue * movedirY);
                                Wafer.Z.Value = 1d * GetHeightValue(Wafer.X.Value, Wafer.Y.Value, pwinfo.Thickness.Value, pwinfo.WaferHeightMapping);
                                if (Wafer.Z.Value < pwinfo.Thickness.Value)
                                {
                                    LoggerManager.Debug($"[PolishWaferCleaning_Standard], DoCleaning(), Wafer Z value = {Wafer.Z.Value}, PolishWafer Thickness = {pwinfo.Thickness.Value}", isInfo: true);
                                    Wafer.Z.Value = pwinfo.Thickness.Value;
                                    LoggerManager.Debug($"[PolishWaferCleaning_Standard], DoCleaning(), Wafer Z value is changed", isInfo: true);
                                }

                                LoggerManager.Debug($"[PolishWaferCleaning_Standard], DoCleaning(), Wafer Z value = {Wafer.Z.Value}", isInfo: true);

                                // Check Margin

                                double dist = Distance2D(pwinfo.PolishWaferCenter.X.Value, pwinfo.PolishWaferCenter.Y.Value,
                                    Wafer.X.Value,
                                    Wafer.Y.Value);

                                // POLISHWAFER_CLEANING_MARGIN_EXCEEDED for Testing
                                if ((retVal == EventCodeEnum.NONE || retVal == EventCodeEnum.UNDEFINED) &&
                                    this.PolishWaferModule().PolishWaferControlItems.POLISHWAFER_CLEANING_MARGIN_EXCEEDED == true)
                                {
                                    retVal = EventCodeEnum.POLISHWAFER_CLEANING_MARGIN_EXCEEDED;
                                }

                                if (retVal != EventCodeEnum.POLISHWAFER_CLEANING_MARGIN_EXCEEDED)
                                {
                                    if (dist < marginradius)
                                    {
                                        // *** IMPORTANT *** 
                                        retVal = this.StageSupervisor().StageModuleState.MovePadToPin(Wafer, pin, zc);
                                    }
                                    else
                                    {
                                        retVal = EventCodeEnum.POLISHWAFER_CLEANING_MARGIN_EXCEEDED;

                                        this.PolishWaferModule().ReasonOfError.AddEventCodeInfo(retVal, "Cleaning margin exceeded", this.GetType().Name);
                                        this.NotifyManager().Notify(retVal);
                                        return retVal;
                                    }
                                }
                                else
                                {
                                    this.PolishWaferModule().ReasonOfError.AddEventCodeInfo(retVal, "Cleaning margin exceeded", this.GetType().Name);
                                    this.NotifyManager().Notify(retVal);
                                    return retVal;
                                }
                            }
                        }

                        if (retVal == EventCodeEnum.NONE)
                        {
                            //ZUp
                            retVal = this.StageSupervisor().StageModuleState.ProbingZUP(Wafer, pin, overdrive);

                            if (retVal != EventCodeEnum.NONE)
                            {
                                //this.PolishWaferModule().ReasonOfError.Reason = "Cleaning Z UP Failed.";
                                this.PolishWaferModule().ReasonOfError.AddEventCodeInfo(retVal, "Cleaning Z UP Failed.", this.GetType().Name);

                                return retVal;
                            }
                            else
                            {
                                pwinfo.TouchCount.Value = pwinfo.TouchCount.Value + 1;
                            }

                            LoggerManager.Debug($"Polish Wafer Cleaning Z Up. Total Count = {_param.ContactCount.Value}, Current Count = {count + 1}, Touch Count = {pwinfo.TouchCount.Value}", isInfo: true);

                            switch (_param.CleaningScrubMode.Value)
                            {
                                // TODO : 로직, 메모리, PW Type 등을 고려하여 나중에 추가할 것.
                                case PWScrubMode.UP_DOWN:
                                    break;
                                case PWScrubMode.One_Direction:
                                    //OneDirectionScrub(dirX, dirY, _param.ScrubingLength.Value);
                                    break;
                                case PWScrubMode.Octagonal:
                                    //OctagonalScrub(dirX, dirY, _param.ScrubingLength.Value);
                                    break;
                                case PWScrubMode.Square:
                                    //SquareScrub(dirX, dirY, _param.ScrubingLength.Value);
                                    break;
                                default:
                                    break;
                            }

                            //ZDown
                            retVal = this.StageSupervisor().StageModuleState.ProbingZDOWN(Wafer, pin, overdrive, clearance);

                            if (retVal != EventCodeEnum.NONE)
                            {
                                //this.PolishWaferModule().ReasonOfError.Reason = "Cleaning Z Down Failed.";
                                this.PolishWaferModule().ReasonOfError.AddEventCodeInfo(retVal, "Cleaning Z Down Failed.", this.GetType().Name);

                                return retVal;
                            }

                            LoggerManager.Debug($"Polish Wafer Cleaning Z Down. Total Count = {param.ContactCount.Value}, Current Count = {count + 1}, Z Position = {this.MotionManager().GetAxis(EnumAxisConstants.Z).Status.Position.Actual}", isInfo: true);
                        }
                    }
                }

                //this.StageSupervisor().StageModuleState.ZCLEARED();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.POLISHWAFER_CLEAING_ERROR;
                LoggerManager.Exception(err);
            }
            finally
            {
                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                this.EventManager().RaisingEvent(typeof(CleaningEndEvent_PolishWafer).FullName, new ProbeEventArgs(this, semaphore));
                semaphore.Wait();

                // POLISHWAFER_CLEAING_ERROR for Testing
                if ((retVal == EventCodeEnum.NONE || retVal == EventCodeEnum.UNDEFINED) &&
                    this.PolishWaferModule().PolishWaferControlItems.POLISHWAFER_CLEAING_ERROR == true)
                {
                    retVal = EventCodeEnum.POLISHWAFER_CLEAING_ERROR;
                }

                if (retVal == EventCodeEnum.NONE)
                {
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Polishwafer_Cleaning_OK);
                }
                else
                {
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Polishwafer_Cleaning_Failure, EventCodeEnum.NONE, retVal.ToString());
                    this.NotifyManager().Notify(retVal);
                }
            }

            return retVal;
        }
        private EventCodeEnum CleaningProcessing(PolishWaferCleaningParameter _param)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        //전체 Dut 가 Polish Wafer 내에 속하는지 확인.
        //파라미터 center 는 dut의 center(wafer 좌표계).
        private bool IsInsideCleaningWafer(double centerx, double centery)
        {
            bool retVal = false;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        /// <summary>
        /// Left, Right, Upper Bottom 네 방향의 Limit 위치를 구해준다. (Wafer 좌표계)
        ///     |->0            |->10        : Left Limit 은 10
        /// Wafer   CleaningSheet
        /// </summary>
        /// <param name="range_left"></param>
        /// <param name="range_right"></param>
        /// <param name="range_upper"></param>
        /// <param name="range_bottom"></param>
        private bool CalcRangeLimit(double margin, out double range_left, out double range_right, out double range_upper, out double range_bottom)
        {
            bool retVal = false;
            range_left = 0.0;
            range_right = 0.0;
            range_upper = 0.0;
            range_bottom = 0.0;
            try
            {
                //Polish Wafer 의 Size 는 Device Wafer Size 와 동일하다.
                double radius = (this.GetParam_Wafer().GetPhysInfo().WaferSize_um.Value / 2) - margin;
                range_left = this.GetParam_Wafer().GetSubsInfo().WaferCenter.GetX() - radius;
                range_right = this.GetParam_Wafer().GetSubsInfo().WaferCenter.GetX() + radius;
                range_upper = this.GetParam_Wafer().GetSubsInfo().WaferCenter.GetY() + radius;
                range_bottom = this.GetParam_Wafer().GetSubsInfo().WaferCenter.GetY() - radius;

                //range 예외처리
                if (range_left == 0 | range_right == 0 | range_upper == 0 | range_bottom == 0)
                    retVal = false;
                else
                    retVal = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }


        private List<PolishWaferCleaningJobParam> MakeCleaningJob(PolishWaferCleaningParameter cleaningparam)
        {
            List<PolishWaferCleaningJobParam> jobparams = null;
            try
            {
                double range_left = 0.0;
                double range_right = 0.0;
                double range_upper = 0.0;
                double range_bottom = 0.0;

                IWaferObject waferobj = this.GetParam_Wafer();

                if (CalcRangeLimit((waferobj.GetPolishInfo().Margin.Value + waferobj.GetPolishInfo().DeadZone),
                    out range_left, out range_right, out range_upper, out range_bottom))
                {
                    IProbeCardDevObject probeCard = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef;
                    IWaferObject waferObject = this.GetParam_Wafer();
                    double sizeX = 0.0;
                    double sizeY = 0.0;

                    sizeX = probeCard.DutIndexSizeX * waferObject.GetPhysInfo().DieSizeX.Value;
                    sizeY = probeCard.DutIndexSizeY * waferObject.GetPhysInfo().DieSizeY.Value;

                    switch (cleaningparam.CleaningDirection.Value)
                    {
                        case CleaningDirection.Right:
                            //from upper left 
                            break;
                        case CleaningDirection.Down:
                            //from upper left 
                            break;
                        case CleaningDirection.Left:
                            //from upper right 
                            break;
                        case CleaningDirection.Left_Down:
                            //from upper right
                            break;
                        case CleaningDirection.Up:
                            //from lower left 
                            break;
                        case CleaningDirection.Right_Up:
                            //from lower left  
                            break;
                        case CleaningDirection.Right_Down:
                            //from upper left 
                            break;
                        case CleaningDirection.Left_Up:
                            //from lower right
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    //CalcRangeLimit Error .
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return jobparams;
        }


        #region //..Scrub

        private EventCodeEnum OneDirectionScrub(int DirX, int DirY, double ScrubLength)
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                double scrubX = 0, scrubY = 0, scrubA;

                scrubA = Math.Abs(ScrubLength);

                scrubX = scrubA * DirX / Math.Sqrt(2);
                scrubY = scrubA * DirY / Math.Sqrt(2);

                retVal = this.StageSupervisor().StageModuleState.StageRelMove(scrubX, scrubY);

                return retVal;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        private EventCodeEnum OctagonalScrub(int DirX, int DirY, double ScrubLength)
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                double div = 0;
                double div_for_diagoanl = 0;
                double scrubX = 0, scrubY = 0;

                div = Math.Abs(ScrubLength);
                div_for_diagoanl = ScrubLength / Math.Sqrt(2);
                //+-- Octagonal Scrub Method (Dir Depends On Input Arg.')
                //      0-7   --+  --+
                //     /   \    |    | -> Div = Scrub_Len (modifyed "/ 3" removed)
                //    1     6   |  --+
                //    |     |   |
                //    2     5   | -> Scrub_Len (Ex. 12um)
                //     \   /    |
                //      3-4   --+
                if (div < 1)
                {
                    //retVal = -1;
                    return retVal;
                }
                //move 0->1 
                scrubX = -div_for_diagoanl * DirX;
                scrubY = -div_for_diagoanl * DirY;

                retVal = this.StageSupervisor().StageModuleState.StageRelMove(scrubX, scrubY);
                //move 1->2 
                scrubX = 0;
                scrubY = -div * DirY;
                retVal = this.StageSupervisor().StageModuleState.StageRelMove(scrubX, scrubY);
                //move 2->3
                scrubX = div_for_diagoanl * DirX;
                scrubY = -div_for_diagoanl * DirY;
                retVal = this.StageSupervisor().StageModuleState.StageRelMove(scrubX, scrubY);
                //move 3->4
                scrubX = div * DirX;
                scrubY = 0;
                retVal = this.StageSupervisor().StageModuleState.StageRelMove(scrubX, scrubY);
                //move 4->5
                scrubX = div_for_diagoanl * DirX;
                scrubY = div_for_diagoanl * DirY;
                retVal = this.StageSupervisor().StageModuleState.StageRelMove(scrubX, scrubY);
                //move 5->6
                scrubX = 0;
                scrubY = div * DirY;
                retVal = this.StageSupervisor().StageModuleState.StageRelMove(scrubX, scrubY);
                //move 6->7
                scrubX = -div_for_diagoanl * DirX;
                scrubY = div_for_diagoanl * DirY;
                retVal = this.StageSupervisor().StageModuleState.StageRelMove(scrubX, scrubY);
                //move 7->0
                scrubX = -div * DirX;
                scrubY = 0;
                retVal = this.StageSupervisor().StageModuleState.StageRelMove(scrubX, scrubY);
                return retVal;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        private EventCodeEnum SquareScrub(int DirX, int DirY, double ScrubLength)
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                //     0 - 3
                //     |   |
                //     1 - 2
                // I. Scrubing Direction(Seq): 0->1->2->3
                double div = 0;
                double scrubX = 0, scrubY = 0;

                div = Math.Abs(ScrubLength);
                //move 0->1 
                scrubX = 0;
                scrubY = -div * DirY;
                retVal = this.StageSupervisor().StageModuleState.StageRelMove(scrubX, scrubY);
                //move 1->2 
                scrubX = div * DirX;
                scrubY = 0;
                retVal = this.StageSupervisor().StageModuleState.StageRelMove(scrubX, scrubY);
                //move 2->3
                scrubX = 0;
                scrubY = div * DirY;
                retVal = this.StageSupervisor().StageModuleState.StageRelMove(scrubX, scrubY);
                //move 3->0
                scrubX = -div * DirX;
                scrubY = 0;
                retVal = this.StageSupervisor().StageModuleState.StageRelMove(scrubX, scrubY);
                return retVal;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }
        #endregion
    }
}
