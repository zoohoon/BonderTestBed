
using System;
using System.Collections.Generic;

namespace PinAlign
{
    using LogModule;
    using ProbeCardObject;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Param;
    using ProberInterfaces.PinAlign.ProbeCardData;
    public class PinAlignAdditionalFunctionClass : IFactoryModule
    {
        public EventCodeEnum PinAutoGrouping()
        {
            LoggerManager.Debug($"[PinAlignAdditionalFunctionClass] PinAutoGrouping() : PinAutoGrouping Start");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            int groupnum = 0;
            int pinnum = 0;
            List<double> PinArrayMax = new List<double>();
            List<double> pin_dbDist = new List<double>();
            List<double> pin_angle = new List<double>();
            List<double> iPinState = new List<double>();
            List<int> iPinAlignNuminGroup = new List<int>();
            List<int> iTempPinGroup = new List<int>();
            List<int> iDutCnt = new List<int>();

            List<IPinData> pins = new List<IPinData>();
            try
            {
                pinnum = CountPinNumber();
                if (pinnum < 1)
                {
                    return EventCodeEnum.NODATA;
                }
                if (pinnum > 4)
                {
                    groupnum = 4;
                }
                else
                {
                    groupnum = 1;
                }

                if (pinnum < groupnum || groupnum == 0)
                {
                    LoggerManager.Debug($"[PinAlignAdditionalFunctionClass] PinAutoGrouping() : Pin Data is not ready");
                    return EventCodeEnum.NODATA;
                }

                LoggerManager.Debug($"[PinAlignAdditionalFunctionClass] PinAutoGrouping() : Total Pin Num = " + pinnum.ToString() + " Total group Num = " + groupnum.ToString());

                foreach (IDut dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                {
                    foreach (PinData pin in dut.PinList)
                    {
                        pins.Add(new PinData(pin));
                    }
                }

                PinCoordinate pincen = null;
                CalcCOG(ref pincen, pins);

                for (int i = 0; i < pinnum; i++)
                {
                    PinArrayMax.Add(-1);
                    pin_dbDist.Add(-1);
                    pin_angle.Add(-1);
                    iTempPinGroup.Add(-1);
                }


                for (int i = 0; i < pinnum; i++)
                {
                    iPinState.Add(0);
                    pin_dbDist[i] = GetDistance2D(pincen, pins[i].AbsPos);
                    LoggerManager.Debug($"[PinAlignAdditionalFunctionClass] PinAutoGrouping() : Distance from Pin Center to # = " + pins[i].PinNum.Value + " Distance = " + pin_dbDist[i].ToString());
                }

                for (int i = 0; i < groupnum; i++)
                {
                    iPinAlignNuminGroup.Add(pinnum / groupnum);
                }

                int endcnt = pinnum % groupnum;

                for (int i = 0; i < endcnt; i++)
                {
                    iPinAlignNuminGroup[i]++;
                }

                for (int i = 0; i < groupnum; i++)
                {
                    LoggerManager.Debug($"[PinAlignAdditionalFunctionClass] PinAutoGrouping() : Each group's number of pin # = " + i.ToString() + " Distance = " + iPinAlignNuminGroup[i].ToString());
                }


                int curpinindex = -1;
                double maxdist = -99999999;
                double mindist = 99999999;
                double dist = 0;
                int MajorIndex = -1;
                for (int j = 0; j < groupnum; j++)
                {
                    maxdist = -99999999;
                    for (int i = 0; i < pinnum; i++)
                    {
                        if (iPinState[i] == 0)
                        {
                            if (pin_dbDist[i] > maxdist)
                            {
                                maxdist = pin_dbDist[i];
                                MajorIndex = i;
                            }
                        }
                    }
                    LoggerManager.Debug($"[PinAlignAdditionalFunctionClass] PinAutoGrouping() : Selected Major Pin # = " + pins[MajorIndex].PinNum.Value + "==========>");
                    for (int x = 0; x < iPinAlignNuminGroup[j]; x++)
                    {
                        mindist = 99999999;
                        for (int k = 0; k < pinnum; k++)
                        {
                            if (iPinState[k] == 0)
                            {
                                dist = GetDistance2D(pins[MajorIndex].AbsPos, pins[k].AbsPos);
                                if (dist < mindist)
                                {
                                    mindist = dist;
                                    curpinindex = k;
                                }
                            }
                        }
                        iPinState[curpinindex] = 1;
                        iTempPinGroup[curpinindex] = j;
                        LoggerManager.Debug($"[PinAlignAdditionalFunctionClass] PinAutoGrouping() : Selected Major Pin # = " + pins[MajorIndex].PinNum.Value + "'s Member Pin = #" + pins[curpinindex].PinNum.Value);
                    }
                    LoggerManager.Debug($"[PinAlignAdditionalFunctionClass] PinAutoGrouping() : Selected Major Pin # = " + pins[MajorIndex].PinNum.Value + " Grouping Done");
                }
                LoggerManager.Debug($"[PinAlignAdditionalFunctionClass] PinAutoGrouping() : Make Group Data");
                GroupData groupData = null;
                this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinGroupList.Clear();
                for (int j = 0; j < groupnum; j++)
                {
                    groupData = new GroupData();
                    for (int i = 0; i < pinnum; i++)
                    {
                        if (iTempPinGroup[i] == j)
                        {
                            groupData.PinNumList.Add(pins[i].PinNum.Value);
                            groupData.GroupResult = PINGROUPALIGNRESULT.CONTINUE;
                        }
                    }
                    this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinGroupList.Add(new GroupData(groupData));
                }
                LoggerManager.Debug($"[PinAlignAdditionalFunctionClass] PinAutoGrouping() : Make Group Data Done");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            LoggerManager.Debug($"[PinAlignAdditionalFunctionClass] PinAutoGrouping() : PinAutoGrouping Done");
            return retVal;
        }
        private int CountPinNumber()
        {
            int NumOfPin = 0;
            try
            {
            foreach (Dut dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
            {
                NumOfPin += dut.PinList.Count;
            }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return NumOfPin;
        }
        private void CalcCOG(ref PinCoordinate CenterPos, List<IPinData> Pins)
        {
            try
            {
            LoggerManager.Debug($"[PinAlignAdditionalFunctionClass] CalcCOG() : CalcCOG Start");
            PinCoordinate Sum = new PinCoordinate();
            try
            {
                foreach (PinData pin in Pins)
                {
                    Sum.X.Value += pin.AbsPos.X.Value;
                    Sum.Y.Value += pin.AbsPos.Y.Value;
                }
                CenterPos = new PinCoordinate((Sum.GetX() / Pins.Count), (Sum.GetY() / Pins.Count));
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "CalcCOG() : Error occured.");
                LoggerManager.Exception(err);
            }
            LoggerManager.Debug($"[PinAlignAdditionalFunctionClass] CalcCOG() : CalcCOG Done");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        private double GetDistance2D(CatCoordinates FirstPin, CatCoordinates SecondPin)
        {
            double Distance = -1;
            try
            {
            LoggerManager.Debug($"[PinAlignAdditionalFunctionClass] GetDistance2D() : GetDistance2D Start");
            try
            {
                Distance = Math.Sqrt(Math.Pow(FirstPin.GetX() - SecondPin.GetX(), 2) + Math.Pow(FirstPin.GetY() - SecondPin.GetY(), 2));
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "CalcCOG() : Error occured.");
                LoggerManager.Exception(err);
            }
            LoggerManager.Debug($"[PinAlignAdditionalFunctionClass] GetDistance2D() : GetDistance2D Done");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return Distance;
        }
        public double GetDegree(PinCoordinate pivot, PinCoordinate pointOld, PinCoordinate pointNew)
        {
            LoggerManager.Debug($"[PinAlignAdditionalFunctionClass] GetDegree() : GetDegree Start");
            double originDegree = 0;
            double updateDegree = 0;
            try
            {
                originDegree = Math.Atan2(
                 pointOld.Y.Value - pivot.Y.Value,
                 pointOld.X.Value - pivot.X.Value)
                 * 180 / Math.PI;

                updateDegree = Math.Atan2(
                     pointNew.Y.Value - pivot.Y.Value,
                     pointNew.X.Value - pivot.X.Value)
                     * 180 / Math.PI;
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "CalcCOG() : Error occured.");
                LoggerManager.Exception(err);
            }
            LoggerManager.Debug($"[PinAlignAdditionalFunctionClass] GetDistance2D() : GetDistance2D Done");
            //==> degree = atan((y2 - cy) / (x2-cx)) - atan((y1 - cy)/(x1-cx)) : 세점사이의 각도 구함


            //==> 프로버 카드가 틀어진 θ 각
            LoggerManager.Debug($"[PinAlignAdditionalFunctionClass] GetDegree() : GetDegree Done");
            return updateDegree - originDegree;
        }
        private double DegreeToRadian(double angle)
        {
            double Angle = 0;
            try
            {
            LoggerManager.Debug($"[PinAlignAdditionalFunctionClass] DegreeToRadian() : DegreeToRadian Start");
            try
            {
                Angle = angle * 57.2957795130823;
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "CalcCOG() : Error occured.");
                LoggerManager.Exception(err);
            }
            LoggerManager.Debug($"[PinAlignAdditionalFunctionClass] DegreeToRadian() : DegreeToRadian Done");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return Angle;
        }
        private double RadianToDgree(double angle)
        {
            double Angle = 0;
            try
            {
            LoggerManager.Debug($"[PinAlignAdditionalFunctionClass] RadianToDgree() : RadianToDgree Start");
            try
            {
                Angle = angle * 1.74532925199433E-02;
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "CalcCOG() : Error occured.");
                LoggerManager.Exception(err);
            }
            LoggerManager.Debug($"[PinAlignAdditionalFunctionClass] RadianToDgree() : RadianToDgree Done");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return Angle;
        }
        private void GetRotCoord(ref MachineCoordinate NewPos, MachineCoordinate OriPos, double angle)
        {
            LoggerManager.Debug($"[PinAlignAdditionalFunctionClass] GetRotCoord() : GetRotCoord Start");
            double newx = 0.0;
            double newy = 0.0;
            double th = DegreeToRadian(angle);

            try
            {
                newx = OriPos.X.Value;
                newy = OriPos.Y.Value;

                NewPos = new MachineCoordinate((newx * Math.Cos(th) - newy * Math.Sin(th)), (newx * Math.Sin(th) + newy * Math.Cos(th)));
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "GetRotCoord() : Error occured.");
                LoggerManager.Exception(err);
            }

            LoggerManager.Debug($"[PinAlignAdditionalFunctionClass] GetRotCoord() : GetRotCoord Done");

            //NewPos.X.Value = newx * Math.Cos(th) - newy * Math.Sin(th);
            //NewPos.Y.Value = newx * Math.Sin(th) + newy * Math.Cos(th);
        }
    }

}
