
using System;
using System.Collections.Generic;

namespace PinAlign
{
    using LogModule;
    using ProberInterfaces;
    using ProberInterfaces.Param;
    using ProberInterfaces.PinAlign.ProbeCardData;
    public class PinAlignAdditionalFunctionClass : IFactoryModule
    {
        //public EventCodeEnum PinAutoGrouping()
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    int groupnum = 0;
        //    int pinnum = 0;
        //    List<double> PinArrayMax = new List<double>();
        //    List<int> MajorPinIndex = new List<int>();
        //    List<double> pin_dbDist = new List<double>();
        //    List<double> pin_angle = new List<double>();
        //    List<double> ideg = new List<double>();
        //    List<double> iPinState = new List<double>();
        //    List<int> iPinAlignNuminGroup = new List<int>();
        //    List<int> iTempPinGroup = new List<int>();
        //    List<int> iDutCnt = new List<int>();
        //    //PinData refpin = null;
        //    List<IPinData> pins = new List<IPinData>();
        //    double StartAngle = 0;
        //    double PlusAngle = 0;
        //    GroupData groupData = null;

        //    LoggerManager.Debug($"[PinAlignAdditionalFunctionClass] PinAutoGrouping() : Start Pin Auto Grouping");

        //    try
        //    {
        //        // 일단 그룹은 하나만 쓴다.
        //        // 이 함수는 여기에서 불리면 안 됨. 디바이스 변경 시 초기화 되어야 하므로 위치 바꾸어야 함.
        //        int TotalPinNum = 0;
        //        IPinData tmpPinData;

        //        TotalPinNum = this.StageSupervisor().ProbeCardInfo.GetPinCount();

        //        this.StageSupervisor().ProbeCardInfo.PinGroupList.Clear();

        //        groupData = new GroupData();
        //        for (int i = 0; i <= TotalPinNum - 1; i++)
        //        {
        //            tmpPinData = this.StageSupervisor().ProbeCardInfo.GetPin(i);
        //            if (tmpPinData != null)
        //            {
        //                groupData.PinNumList.Add(i);
        //                groupData.GroupResult = PINGROUPALIGNRESULT.CONTINUE;
        //            }
        //        }

        //        this.StageSupervisor().ProbeCardInfo.PinGroupList.Add(groupData);                
        //        retVal = EventCodeEnum.NONE;
        //        return retVal;









        //        pinnum = CountPinNumber();
        //        if(pinnum < 1)
        //        {
        //            return EventCodeEnum.NODATA;
        //        }
        //        if (pinnum > 4)
        //        {
        //            groupnum = 4;
        //        }
        //        else
        //        {
        //            groupnum = 1;
        //        }

        //        if (pinnum < groupnum)
        //        {
        //            LoggerManager.Debug($"[PinAlignAdditionalFunctionClass] PinAutoGrouping() : Number of pin is lower than group num. pin = " + pinnum.ToString());
        //            groupnum = pinnum;
        //        }
        //        else if (groupnum == 0)
        //        {
        //            LoggerManager.Debug($"[PinAlignAdditionalFunctionClass] PinAutoGrouping() : Pin Data is not exist.");
        //            return EventCodeEnum.NODATA;
        //        }

        //        /*여기는 테스트 코드입니다.*/
        //        //Dut dut = new Dut();
        //        //dut.MacIndex = new MachineIndex(0, 0);
        //        //refpin = new PinData();
        //        //refpin.AbsPos = new PinCoordinate(-7000, 7000, -9000);
        //        //refpin.PinNum.SetValue("0");

        //        //PinData temppin = new PinData();

        //        //temppin.DutInfo = new Dut(dut);

        //        //temppin.AbsPos = new PinCoordinate(-7000, 7000, -9000);
        //        //temppin.PinNum.SetValue("0");
        //        //pins.Add(new PinData(temppin));

        //        //temppin.AbsPos = new PinCoordinate(-6900, 7000, -9000);
        //        //temppin.PinNum.SetValue("1");
        //        //pins.Add(new PinData(temppin));

        //        //temppin.AbsPos = new PinCoordinate(-7000, 6900, -9000);
        //        //temppin.PinNum.SetValue("2");
        //        //pins.Add(new PinData(temppin));

        //        //temppin.AbsPos = new PinCoordinate(-6900, 6900, -9000);
        //        //temppin.PinNum.SetValue("3");
        //        //pins.Add(new PinData(temppin));

        //        //temppin.AbsPos = new PinCoordinate(7000, 7000, -9000);
        //        //temppin.PinNum.SetValue("4");
        //        //pins.Add(new PinData(temppin));

        //        //temppin.AbsPos = new PinCoordinate(6900, 7000, -9000);
        //        //temppin.PinNum.SetValue("5");
        //        //pins.Add(new PinData(temppin));

        //        //temppin.AbsPos = new PinCoordinate(7000, 6900, -9000);
        //        //temppin.PinNum.SetValue("6");
        //        //pins.Add(new PinData(temppin));

        //        //temppin.AbsPos = new PinCoordinate(6900, 6900, -9000);
        //        //temppin.PinNum.SetValue("7");
        //        //pins.Add(new PinData(temppin));

        //        //temppin.AbsPos = new PinCoordinate(7000, -7000, -9000);
        //        //temppin.PinNum.SetValue("8");
        //        //pins.Add(new PinData(temppin));

        //        //temppin.AbsPos = new PinCoordinate(6900, -7000, -9000);
        //        //temppin.PinNum.SetValue("9");
        //        //pins.Add(new PinData(temppin));

        //        //temppin.AbsPos = new PinCoordinate(7000, -6900, -9000);
        //        //temppin.PinNum.SetValue("10");
        //        //pins.Add(new PinData(temppin));

        //        //temppin.AbsPos = new PinCoordinate(6900, -6900, -9000);
        //        //temppin.PinNum.SetValue("11");
        //        //pins.Add(new PinData(temppin));

        //        //temppin.AbsPos = new PinCoordinate(-7000, -7000, -9000);
        //        //temppin.PinNum.SetValue("12");
        //        //pins.Add(new PinData(temppin));

        //        //temppin.AbsPos = new PinCoordinate(-6900, -7000, -9000);
        //        //temppin.PinNum.SetValue("13");
        //        //pins.Add(new PinData(temppin));

        //        //temppin.AbsPos = new PinCoordinate(-7000, -6900, -9000);
        //        //temppin.PinNum.SetValue("14");
        //        //pins.Add(new PinData(temppin));

        //        //temppin.AbsPos = new PinCoordinate(-6900, -6900, -9000);
        //        //temppin.PinNum.SetValue("15");
        //        //pins.Add(new PinData(temppin));

        //        //temppin.AbsPos = new PinCoordinate(-50, 50, -9000);
        //        //temppin.PinNum.SetValue("16");
        //        //pins.Add(new PinData(temppin));

        //        //temppin.AbsPos = new PinCoordinate(50, 50, -9000);
        //        //temppin.PinNum.SetValue("17");
        //        //pins.Add(new PinData(temppin));

        //        //temppin.AbsPos = new PinCoordinate(-50, -50, -9000);
        //        //temppin.PinNum.SetValue("18");
        //        //pins.Add(new PinData(temppin));

        //        //temppin.AbsPos = new PinCoordinate(50, -50, -9000);
        //        //temppin.PinNum.SetValue("19");
        //        //pins.Add(new PinData(temppin));

        //        //temppin.AbsPos = new PinCoordinate(6900, 50, -9000);
        //        //temppin.PinNum.SetValue("20");
        //        //pins.Add(new PinData(temppin));

        //        //temppin.AbsPos = new PinCoordinate(7000, 50, -9000);
        //        //temppin.PinNum.SetValue("21");
        //        //pins.Add(new PinData(temppin));

        //        //temppin.AbsPos = new PinCoordinate(6900, -50, -9000);
        //        //temppin.PinNum.SetValue("22");
        //        //pins.Add(new PinData(temppin));

        //        //temppin.AbsPos = new PinCoordinate(7000, -50, -9000);
        //        //temppin.PinNum.SetValue("23");
        //        //pins.Add(new PinData(temppin));

        //        //temppin.AbsPos = new PinCoordinate(-7000, -6900, -9000);
        //        //temppin.PinNum.SetValue("24");
        //        //pins.Add(new PinData(temppin));

        //        //temppin.AbsPos = new PinCoordinate(-6900, -6900, -9000);
        //        //temppin.PinNum.SetValue("25");
        //        //pins.Add(new PinData(temppin));

        //        //temppin.AbsPos = new PinCoordinate(-7000, -7000, -9000);
        //        //temppin.PinNum.SetValue("26");
        //        //pins.Add(new PinData(temppin));

        //        //temppin.AbsPos = new PinCoordinate(-6900, -7000, -9000);
        //        //temppin.PinNum.SetValue("27");
        //        //pins.Add(new PinData(temppin));

        //        //temppin.AbsPos = new PinCoordinate(-50, -6900, -9000);
        //        //temppin.PinNum.SetValue("28");
        //        //pins.Add(new PinData(temppin));

        //        //temppin.AbsPos = new PinCoordinate(50, -6900, -9000);
        //        //temppin.PinNum.SetValue("29");
        //        //pins.Add(new PinData(temppin));

        //        //temppin.AbsPos = new PinCoordinate(-50, -7000, -9000);
        //        //temppin.PinNum.SetValue("30");
        //        //pins.Add(new PinData(temppin));

        //        //temppin.AbsPos = new PinCoordinate(50, -7000, -9000);
        //        //temppin.PinNum.SetValue("31");
        //        //pins.Add(new PinData(temppin));

        //        //temppin.AbsPos = new PinCoordinate(6900, -6900, -9000);
        //        //temppin.PinNum.SetValue("32");
        //        //pins.Add(new PinData(temppin));

        //        //temppin.AbsPos = new PinCoordinate(7000, -6900, -9000);
        //        //temppin.PinNum.SetValue("33");
        //        //pins.Add(new PinData(temppin));

        //        //temppin.AbsPos = new PinCoordinate(6900, -7000, -9000);
        //        //temppin.PinNum.SetValue("34");
        //        //pins.Add(new PinData(temppin));

        //        //temppin.AbsPos = new PinCoordinate(7000, -7000, -9000);
        //        //temppin.PinNum.SetValue("35");
        //        //pins.Add(new PinData(temppin));

        //        //Cluster(pins, 4);
        //        /*=========================*/


        //        foreach(IDut dut in this.StageSupervisor().ProbeCardInfo.DutList)
        //        {
        //            foreach(IPinData pin in dut.PinList)
        //            {
        //                pins.Add(new PinData(pin));
        //            }
        //        }

        //        PinCoordinate pincen = null;
        //        CalcCOG(ref pincen, pins);
        //        LoggerManager.Debug($"[PinAlignAdditionalFunctionClass] PinAutoGrouping() : Pin Center Position  X = " + pincen.GetX().ToString() + " Y = " + pincen.GetY().ToString() + " Z = " + pincen.GetZ().ToString());

        //        for (int i = 0; i < pinnum; i++)
        //        {
        //            PinArrayMax.Add(-1);
        //            pin_dbDist.Add(-1);
        //            pin_angle.Add(-1);
        //            iTempPinGroup.Add(-1);
        //        }
        //        if (groupnum != 0)
        //        {
        //            StartAngle = -1 + 1 / Convert.ToDouble(groupnum);
        //            PlusAngle = 2 / Convert.ToDouble(groupnum);
        //        }

        //        for (int i = 0; i < groupnum; i++)
        //        {
        //            ideg.Add(StartAngle + (PlusAngle * i));
        //            MajorPinIndex.Add(-1);
        //        }
        //        LoggerManager.Debug($"[PinAlignAdditionalFunctionClass] PinAutoGrouping() : Each pin distance from pin center");
        //        for (int i = 0; i < pinnum; i++)
        //        {
        //            iPinState.Add(0);
        //            pin_dbDist[i] = GetDistance2D(pincen, pins[i].AbsPos);
        //            LoggerManager.Debug($"[PinAlignAdditionalFunctionClass] PinAutoGrouping() : #" + pins[i].PinNum.Value + " = " +pin_dbDist[i].ToString());

        //            //pin_angle[i] = Math.Atan2((pins[i].AbsPos.Y.Value - refpin.AbsPos.Y.Value), (pins[i].AbsPos.X.Value - refpin.AbsPos.X.Value));
        //        }

        //        for (int i = 0; i < groupnum; i++)
        //        {
        //            iPinAlignNuminGroup.Add(pinnum / groupnum);
        //        }

        //        int endcnt = pinnum % groupnum;

        //        for (int i = 0; i < endcnt; i++)
        //        {
        //            iPinAlignNuminGroup[i]++;
        //        }

        //        int curpinindex = -1;
        //        //int pinnumingroup = 0;
        //        double maxdist = -99999999;
        //        double mindist = 99999999;
        //        double dist = 0;
        //        int MajorIndex = -1;
        //        for (int j = 0; j < groupnum; j++)
        //        {
        //            maxdist = -99999999;
        //            for (int i = 0; i < pinnum; i++)
        //            {
        //                if (iPinState[i] == 0)
        //                {
        //                    if (pin_dbDist[i] > maxdist)
        //                    {
        //                        maxdist = pin_dbDist[i];
        //                        MajorPinIndex[j] = i;
        //                        MajorIndex = i;
        //                        LoggerManager.Debug($"[PinAlignAdditionalFunctionClass] PinAutoGrouping() : Selected Major Pin #" + pins[MajorIndex].PinNum.Value);
        //                    }
        //                }
        //            }
        //            for (int x = 0; x < iPinAlignNuminGroup[j]; x++)
        //            {
        //                mindist = 99999999;
        //                for (int k = 0; k < pinnum; k++)
        //                {
        //                    if (iPinState[k] == 0)
        //                    {
        //                        dist = GetDistance2D(pins[MajorIndex].AbsPos, pins[k].AbsPos);
        //                        if (dist < mindist)
        //                        {
        //                            mindist = dist;
        //                            curpinindex = k;
        //                        }
        //                    }
        //                }
        //                LoggerManager.Debug($"[PinAlignAdditionalFunctionClass] PinAutoGrouping() : Member od Selected Major Pin #" + pins[MajorIndex].PinNum.Value + " = " +  pins[curpinindex].PinNum.Value);
        //                iPinState[curpinindex] = 1;
        //                iTempPinGroup[curpinindex] = j;
        //            }
        //        }

        //        this.StageSupervisor().ProbeCardInfo.PinGroupList.Clear();
        //        for (int j = 0; j < groupnum; j++)
        //        {
        //            groupData = new GroupData();
        //            for (int i = 0; i < pinnum; i++)
        //            {
        //                if (iTempPinGroup[i] == j)
        //                {
        //                    groupData.PinNumList.Add(pins[i].PinNum.Value);
        //                    groupData.GroupResult = PINGROUPALIGNRESULT.CONTINUE;
        //                }
        //            }
        //            this.StageSupervisor().ProbeCardInfo.PinGroupList.Add(new GroupData(groupData));
        //        }
        //        LoggerManager.Debug($"[PinAlignAdditionalFunctionClass] PinAutoGrouping() : End Pin Auto Grouping");
        //        retVal = EventCodeEnum.NONE;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    return retVal;
        //}
        //private int CountPinNumber()
        //{
        //    int NumOfPin = 0;
        //    foreach (IDut dut in this.StageSupervisor().ProbeCardInfo.DutList)
        //    {
        //        NumOfPin += dut.PinList.Count;
        //    }
        //    return NumOfPin;
        //}
        //private void CalcCOG(ref PinCoordinate CenterPos, List<IPinData> Pins)
        //{
        //    PinCoordinate Sum = new PinCoordinate();
        //    try
        //    {
        //        foreach (IPinData pin in Pins)
        //        {
        //            Sum.X.Value += pin.AbsPos.X.Value;
        //            Sum.Y.Value += pin.AbsPos.Y.Value;
        //        }
        //        CenterPos = new PinCoordinate((Sum.GetX() / Pins.Count), (Sum.GetY() / Pins.Count));
        //    }
        //    catch (Exception err)
        //    {
        //        //LoggerManager.Error($err + "CalcCOG() : Error occured.");
        //        LoggerManager.Exception(err);
        //    }
        //}
        //private double GetDistance2D(CatCoordinates FirstPin, CatCoordinates SecondPin)
        //{
        //    double Distance = -1;

        //    Distance = Math.Sqrt(Math.Pow(FirstPin.GetX() - SecondPin.GetX(), 2) + Math.Pow(FirstPin.GetY() - SecondPin.GetY(), 2));

        //    return Distance;
        //}
        //public double GetDegree(PinCoordinate pivot, PinCoordinate pointOld, PinCoordinate pointNew)
        //{
        //    //==> degree = atan((y2 - cy) / (x2-cx)) - atan((y1 - cy)/(x1-cx)) : 세점사이의 각도 구함
        //    double originDegree = Math.Atan2(
        //         pointOld.Y.Value - pivot.Y.Value,
        //         pointOld.X.Value - pivot.X.Value)
        //         * 180 / Math.PI;

        //    double updateDegree = Math.Atan2(
        //         pointNew.Y.Value - pivot.Y.Value,
        //         pointNew.X.Value - pivot.X.Value)
        //         * 180 / Math.PI;

        //    //==> 프로버 카드가 틀어진 θ 각
        //    return updateDegree - originDegree;
        //}
        private double DegreeToRadian(double angle)
        {
            return angle * 57.2957795130823;
        }
        private double RadianToDgree(double angle)
        {
            return angle * 1.74532925199433E-02;
        }
        private void GetRotCoord(ref MachineCoordinate NewPos, MachineCoordinate OriPos, double angle)
        {
            try
            {
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


                //NewPos.X.Value = newx * Math.Cos(th) - newy * Math.Sin(th);
                //NewPos.Y.Value = newx * Math.Sin(th) + newy * Math.Cos(th);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private void GetRotCoord2(ref PinCoordinate NewPos, PinCoordinate OriPos, double angle)
        {
            try
            {
                double newx = 0.0;
                double newy = 0.0;
                double th = DegreeToRadian(angle);

                try
                {
                    newx = OriPos.X.Value;
                    newy = OriPos.Y.Value;

                    NewPos = new PinCoordinate((newx * Math.Cos(th) - newy * Math.Sin(th)), (newx * Math.Sin(th) + newy * Math.Cos(th)));
                }
                catch (Exception err)
                {
                    //LoggerManager.Error($err + "GetRotCoord() : Error occured.");
                    LoggerManager.Exception(err);
                }


                //NewPos.X.Value = newx * Math.Cos(th) - newy * Math.Sin(th);
                //NewPos.Y.Value = newx * Math.Sin(th) + newy * Math.Cos(th);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private void GetRotCoordEx(ref PinCoordinate NewPos, PinCoordinate OriPos, PinCoordinate RefPos, double angle)
        {
            try
            {
                double newx = 0.0;
                double newy = 0.0;
                double th = DegreeToRadian(angle);

                newx = OriPos.X.Value - RefPos.X.Value;
                newy = OriPos.Y.Value - RefPos.Y.Value;

                NewPos.X.Value = newx * Math.Cos(th) - newy * Math.Sin(th) + RefPos.X.Value;
                NewPos.Y.Value = newx * Math.Sin(th) + newy * Math.Cos(th) + RefPos.Y.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public static int[] Cluster(List<IPinData> rawData, int numClusters)
        {
            double[][] data = Normalized(rawData);
            bool changed = true; bool success = true;
            int[] clustering = InitClustering(data.Length, numClusters, 3);
            try
            {
                double[][] means = Allocate(numClusters, data[0].Length);
                int maxCount = data.Length * 10000;
                int ct = 0;
                while (changed == true && success == true && ct < maxCount)
                {
                    ++ct;
                    success = UpdateMeans(data, clustering, means);
                    changed = UpdateClustering(data, clustering, means);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return clustering;
        }

        private static double[][] Normalized(List<IPinData> rawData)
        {

            try
            {
                double[][] result = new double[rawData.Count][];
                for (int i = 0; i < rawData.Count; ++i)
                {
                    result[i] = new double[2];
                }

                foreach (IPinData pin in rawData)
                {
                    result[rawData.IndexOf(pin)][0] = pin.AbsPos.X.Value;
                    result[rawData.IndexOf(pin)][1] = pin.AbsPos.Y.Value;
                }
                /*
                for (int j = 0; j < result[0].Length; ++j)
                {
                    double colSum = 0.0;
                    for (int i = 0; i < result.Length; ++i)
                        colSum += result[i][j];
                    double mean = colSum / result.Length;
                    double sum = 0.0;
                    for (int i = 0; i < result.Length; ++i)
                        sum += (result[i][j] - mean) * (result[i][j] - mean);
                    double sd = sum / result.Length;
                    for (int i = 0; i < result.Length; ++i)
                        result[i][j] = (result[i][j] - mean) / sd;
                */
                return result;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private static int[] InitClustering(int numTuples, int numClusters, int seed)
        {
            try
            {
                Random random = new Random(seed);
                int[] clustering = new int[numTuples];
                for (int i = 0; i < numClusters; ++i)
                    clustering[i] = i;
                for (int i = numClusters; i < clustering.Length; ++i)
                    clustering[i] = random.Next(0, numClusters);
                return clustering;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private static bool UpdateMeans(double[][] data, int[] clustering, double[][] means)
        {
            try
            {
                int numClusters = means.Length;
                int[] clusterCounts = new int[numClusters];
                for (int i = 0; i < data.Length; ++i)
                {
                    int cluster = clustering[i];
                    ++clusterCounts[cluster];
                }

                //for (int k = 0; k < numClusters; ++k)
                //    if (clusterCounts[k] == 0)
                //        return false;

                for (int k = 0; k < means.Length; ++k)
                    for (int j = 0; j < means[k].Length; ++j)
                        means[k][j] = 0.0;

                for (int i = 0; i < data.Length; ++i)
                {
                    int cluster = clustering[i];
                    for (int j = 0; j < data[i].Length; ++j)
                        means[cluster][j] += data[i][j]; // accumulate sum
                }

                for (int k = 0; k < means.Length; ++k)
                    for (int j = 0; j < means[k].Length; ++j)
                        means[k][j] /= clusterCounts[k]; // danger of div by 0
                return true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private static double[][] Allocate(int numClusters, int numColumns)
        {
            try
            {
                double[][] result = new double[numClusters][];
                for (int k = 0; k < numClusters; ++k)
                    result[k] = new double[numColumns];
                return result;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private static bool UpdateClustering(double[][] data, int[] clustering, double[][] means)
        {
            try
            {
                int numClusters = means.Length;
                //bool changed = false;

                int[] newClustering = new int[clustering.Length];
                Array.Copy(clustering, newClustering, clustering.Length);

                double[] distances = new double[numClusters];

                for (int i = 0; i < data.Length; ++i)
                {
                    for (int k = 0; k < numClusters; ++k)
                        distances[k] = Distance(data[i], means[k]);

                    int newClusterID = MinIndex(distances);
                    if (newClusterID != newClustering[i])
                    {
                        //changed = true;
                        newClustering[i] = newClusterID;
                    }
                }

                //if (changed == false)
                //    return false;

                int[] clusterCounts = new int[numClusters];
                for (int i = 0; i < data.Length; ++i)
                {
                    int cluster = newClustering[i];
                    ++clusterCounts[cluster];
                }

                //for (int k = 0; k < numClusters; ++k)
                //    if (clusterCounts[k] == 0)
                //        return false;

                Array.Copy(newClustering, clustering, newClustering.Length);
                return true; // no zero-counts and at least one change
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private static double Distance(double[] tuple, double[] mean)
        {
            double sumSquaredDiffs = 0.0;
            for (int j = 0; j < tuple.Length; ++j)
                sumSquaredDiffs += Math.Pow((tuple[j] - mean[j]), 2);
            return Math.Sqrt(sumSquaredDiffs);
        }

        private static int MinIndex(double[] distances)
        {
            int indexOfMin = 0;
            try
            {
                double smallDist = distances[0];
                for (int k = 0; k < distances.Length; ++k)
                {
                    if (distances[k] < smallDist)
                    {
                        smallDist = distances[k];
                        indexOfMin = k;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return indexOfMin;
        }
    }

}
