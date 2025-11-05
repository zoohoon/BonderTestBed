using System;
using System.Collections.Generic;
using System.Linq;

namespace PolishWaferCenteringModule
{
    using LogModule;
    using PolishWaferParameters;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Param;
    using ProberInterfaces.PolishWafer;
    using System.Windows;

    public class PolishWaferCentering_Standard : IPolishWaferCentering
    {
        public PolishWaferCentering_Standard()
        {

        }

        #region //..Edge Detection
        private List<WaferProcResult> procresults = new List<WaferProcResult>();
        private List<WaferCoordinate> _EdgePos = new List<WaferCoordinate>();
        public List<WaferCoordinate> EdgePos
        {
            get { return _EdgePos; }
            set { _EdgePos = value; }
        }
        private PolishWaferParameter PWParam
        {
            get
            {
                return (PolishWaferParameter)this.PolishWaferModule().PolishWaferParameter;
            }
        }

        public EventCodeEnum DoCentering(IPolishWaferCleaningParameter param)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            WaferCoordinate wafercoord = null;
            //ProbeAxisObject axist = this.MotionManager().GetAxis(EnumAxisConstants.C);

            //LoggerManager.Funclog(typeof(PolishWaferCentering_Standard), EnumFuncCallingTime.START);
            LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Polishwafer_Centering_Start);

            try
            {
                //#region //..[Trun Theta 0 ] 
                //double curtpos = 0.0;
                //this.MotionManager().GetActualPos(EnumAxisConstants.C, ref curtpos);

                //curtpos = Math.Abs(curtpos);

                //int converttpos = Convert.ToInt32(curtpos);

                //if (converttpos != 0)
                //{
                //    this.StageSupervisor().StageModuleState.WaferLowViewMove(axist, 0);
                //}
                //#endregion

                IPolishWaferSourceInformation pwinfo = this.StageSupervisor().WaferObject.GetPolishInfo();

                if (pwinfo == null)
                {
                    LoggerManager.Error($"DoCentering() : Parameter is null.");

                    retVal = EventCodeEnum.PARAM_ERROR;
                    return retVal;
                }

                if (pwinfo.PolishWaferCenter == null)
                {
                    pwinfo.PolishWaferCenter = new WaferCoordinate();
                }

                pwinfo.PolishWaferCenter.X.Value = 0;
                pwinfo.PolishWaferCenter.Y.Value = 0;

                bool enableedge = param.EdgeDetectionBeforeCleaning.Value;

                if (!enableedge)
                {
                    ///Edge 가 Disable 로 설정되어있다면 정상으로 종료.
                    retVal = EventCodeEnum.NONE;

                    return retVal;
                }

                PolishWaferCleaningParameter _param = param as PolishWaferCleaningParameter;

                //var source = PWParam.SourceParameters.Where(x => x.DefineName.Value == _param.WaferDefineType.Value).FirstOrDefault();

                procresults.Clear();

                ICamera CurCam = this.VisionManager().GetCam(EnumProberCam.WAFER_LOW_CAM);
                this.VisionManager().StartGrab(EnumProberCam.WAFER_LOW_CAM, this);

                if (_param.CenteringLightParams == null || _param.CenteringLightParams.Count <= 0)
                {
                    retVal = EventCodeEnum.PARAM_ERROR;
                    return retVal;
                }

                foreach (var light in _param.CenteringLightParams)
                {
                    CurCam.SetLight(light.Type.Value, light.Value.Value);
                }

                double wSize = 0;

                //pwinfo.Size.Value = SubstrateSizeEnum.INCH12;

                switch (pwinfo.Size.Value)
                {
                    case SubstrateSizeEnum.INVALID:
                        retVal = EventCodeEnum.PARAM_ERROR;
                        break;
                    case SubstrateSizeEnum.UNDEFINED:
                        retVal = EventCodeEnum.PARAM_ERROR;
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
                        retVal = EventCodeEnum.PARAM_ERROR;
                        break;
                    default:
                        break;
                }

                if (retVal == EventCodeEnum.PARAM_ERROR)
                {
                    LoggerManager.Error($"DoCentering() : Parameter is invalid. SubstrateSizeEnum = {pwinfo.Size.Value}");

                    return retVal;
                }

                double edgepos = 0.0;
                edgepos = ((wSize / 2) / Math.Sqrt(2));

                EdgePos.Clear();

                EdgePos.Add(new WaferCoordinate(edgepos, edgepos));
                EdgePos.Add(new WaferCoordinate(-edgepos, edgepos));
                EdgePos.Add(new WaferCoordinate(-edgepos, -edgepos));
                EdgePos.Add(new WaferCoordinate(edgepos, -edgepos));

                var axisX = this.MotionManager().GetAxis(EnumAxisConstants.X);
                var axisY = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                var axisT = this.MotionManager().GetAxis(EnumAxisConstants.C);

                this.MotionManager().SetSettlingTime(axisX, 0.001);
                this.MotionManager().SetSettlingTime(axisY, 0.001);

                ImageBuffer[] EdgeBuffer = new ImageBuffer[EdgePos.Count];
                ImageBuffer[] EdgeLineBuffer = new ImageBuffer[EdgePos.Count];

                this.VisionManager().StopGrab(CurCam.GetChannelType());

                for (int index = 0; index < EdgePos.Count; index++)
                {
                    if (pwinfo.Thickness.Value <= 0)
                    {
                        LoggerManager.Error($"DoCentering() : Parameter is invalid. Thickness = {pwinfo.Thickness.Value}");

                        return retVal;
                    }

                    this.StageSupervisor().StageModuleState.WaferLowViewMove(EdgePos[index].X.Value, EdgePos[index].Y.Value, pwinfo.Thickness.Value);

                    if (this.VisionManager().ConfirmDigitizerEmulMode(CurCam.GetChannelType()))
                    {
                        this.VisionManager().LoadImageFromFileToGrabber(@"C:\ProberSystem\EmulImages\PolishWafer\Edge\Edge" + index + ".bmp", CurCam.GetChannelType());
                    }

                    EdgeBuffer[index] = this.VisionManager().SingleGrab(CurCam.GetChannelType(), this);

                    if (!this.VisionManager().ConfirmDigitizerEmulMode(CurCam.GetChannelType()))
                    {
                        this.VisionManager().StartGrab(CurCam.GetChannelType(), this);
                    }

                    EdgeLineBuffer[index] = this.VisionManager().Line_Equalization(EdgeBuffer[index], index);

                }

                if (EdgeLineBuffer.ToList<ImageBuffer>().FindAll(img => img != null).Count() != 0)
                {
                    int Except_Pixel = 30;
                    int Acc_Pixel = 20;
                    int Cmp_Pixel = 10;

                    double Temp_Sum = 0;
                    double Temp_Avg = 0;

                    int Width = EdgeLineBuffer[0].SizeX;
                    int Heigh = EdgeLineBuffer[0].SizeY;

                    int RWidth = EdgeLineBuffer[0].SizeX - 1;
                    int RHeight = EdgeLineBuffer[0].SizeY - 1;

                    double[,] EdgeOVal = new double[EdgePos.Count, Width];

                    double TempOval = 0;
                    int TempOvalPos = 0;

                    double Threshold = 20;

                    int i, ii, j, k, kk, m;

                    int alphaS, BetaS;
                    int alphaE, BetaE;

                    int PMFLAG;

                    for (k = 0; k < EdgePos.Count; k++)
                    {
                        if ((k == 1) || (k == 2))
                        {
                            alphaS = 0;
                            alphaE = RWidth;
                        }
                        else
                        {
                            alphaS = RWidth;
                            alphaE = 0;
                        }

                        if ((k == 0) || (k == 1))
                        {
                            BetaS = 0;
                            BetaE = RHeight;

                            PMFLAG = 1;
                        }
                        else
                        {
                            BetaS = RHeight;
                            BetaE = 0;

                            PMFLAG = -1;
                        }

                        j = BetaS + (Except_Pixel * PMFLAG);

                        // k = 0, 3
                        if (alphaS > alphaE)
                        {
                            for (i = (alphaS - Except_Pixel); i >= (alphaE + 1); i--)
                            {
                                Temp_Sum = 0;

                                for (kk = 1; kk <= Acc_Pixel; kk++)
                                {
                                    Temp_Sum += EdgeLineBuffer[k].Buffer[(j - (kk * PMFLAG)) * Width + (i + kk)];
                                }

                                Temp_Avg = Temp_Sum / Acc_Pixel;

                                if ((i > Except_Pixel) && (i < Width - Except_Pixel))
                                {
                                    EdgeOVal[k, RWidth - i] = Temp_Avg - EdgeLineBuffer[k].Buffer[j * Width + i];
                                }

                                j = j + PMFLAG;
                            }
                        }

                        // k = 1, 2
                        else
                        {
                            for (i = (alphaS + Except_Pixel); i <= (alphaE - 1); i++)
                            {
                                Temp_Sum = 0;

                                for (kk = 1; kk <= Acc_Pixel; kk++)
                                {
                                    Temp_Sum += EdgeLineBuffer[k].Buffer[(j - (kk * PMFLAG)) * Width + (i - kk)];
                                }

                                Temp_Avg = Temp_Sum / Acc_Pixel;

                                if ((i > Except_Pixel) && (i < Width - Except_Pixel))
                                {
                                    EdgeOVal[k, i] = Temp_Avg - EdgeLineBuffer[k].Buffer[j * Width + i];
                                }

                                j = j + PMFLAG;
                            }
                        }
                    }

                    double P_RSum = 0;
                    double P_LSum = 0;
                    double P_RAvg = 0;
                    double P_LAvg = 0;

                    int sx, sy;

                    int MaxDiffValue = 0;

                    Point TempPos = new Point();

                    int ret_Edge0_count = 0;
                    int ret_Edge1_count = 0;
                    int ret_Edge2_count = 0;
                    int ret_Edge3_count = 0;

                    int rsize = 32;

                    int[] ret_PMEdge0 = new int[rsize];
                    int[] ret_PMEdge1 = new int[rsize];
                    int[] ret_PMEdge2 = new int[rsize];
                    int[] ret_PMEdge3 = new int[rsize];

                    Point[] ret_Edge0 = new Point[rsize];
                    Point[] ret_Edge1 = new Point[rsize];
                    Point[] ret_Edge2 = new Point[rsize];
                    Point[] ret_Edge3 = new Point[rsize];

                    sx = Width;
                    sy = Heigh;

                    for (k = 0; k < EdgePos.Count; k++)
                    {
                        for (i = Except_Pixel; i < RWidth - Except_Pixel; i++)
                        {
                            if (Math.Abs(EdgeOVal[k, i]) > Threshold)
                            {
                                TempOval = 0;

                                for (j = -Cmp_Pixel; j < Cmp_Pixel; j++)
                                {
                                    if (Math.Abs(EdgeOVal[k, i + j]) > TempOval)
                                    {
                                        TempOval = Math.Abs(EdgeOVal[k, i + j]);
                                        TempOvalPos = i + j;
                                    }
                                }

                                if (Math.Abs(EdgeOVal[k, TempOvalPos - 1]) > (Threshold * 0.8) &&
                                    Math.Abs(EdgeOVal[k, TempOvalPos + 1]) > (Threshold * 0.8))
                                {

                                    if (TempOvalPos == i)
                                    {
                                        if (k == 0)
                                        {
                                            P_RSum = 0;
                                            P_LSum = 0;

                                            for (kk = 1; kk <= Cmp_Pixel; kk++)
                                            {
                                                P_RSum += EdgeLineBuffer[k].Buffer[(i - kk) * Width + (RWidth - i + kk)];
                                            }

                                            for (kk = 1; kk <= Cmp_Pixel; kk++)
                                            {
                                                P_LSum += EdgeLineBuffer[k].Buffer[(i + kk) * Width + (RWidth - i - kk)];
                                            }

                                            P_RAvg = P_RSum / Cmp_Pixel;
                                            P_LAvg = P_LSum / Cmp_Pixel;
                                        }

                                        if (k == 1)
                                        {
                                            P_RSum = 0;
                                            P_LSum = 0;

                                            for (kk = 1; kk <= Cmp_Pixel; kk++)
                                            {
                                                P_RSum += EdgeLineBuffer[k].Buffer[(i + kk) * Width + (i + kk)];
                                            }

                                            for (kk = 1; kk <= Cmp_Pixel; kk++)
                                            {
                                                P_LSum += EdgeLineBuffer[k].Buffer[(i - kk) * Width + (i - kk)];
                                            }

                                            P_RAvg = P_RSum / Cmp_Pixel;
                                            P_LAvg = P_LSum / Cmp_Pixel;
                                        }

                                        if (k == 2)
                                        {
                                            P_RSum = 0;
                                            P_LSum = 0;

                                            for (kk = 1; kk <= Cmp_Pixel; kk++)
                                            {
                                                P_RSum += EdgeLineBuffer[k].Buffer[(RHeight - i - kk) * Width + (i + kk)];
                                            }

                                            for (kk = 1; kk <= Cmp_Pixel; kk++)
                                            {
                                                P_LSum += EdgeLineBuffer[k].Buffer[(RHeight - i + kk) * Width + (i - kk)];
                                            }

                                            P_RAvg = P_RSum / Cmp_Pixel;
                                            P_LAvg = P_LSum / Cmp_Pixel;
                                        }

                                        if (k == 3)
                                        {
                                            P_RSum = 0;
                                            P_LSum = 0;

                                            for (kk = 1; kk <= Cmp_Pixel; kk++)
                                            {
                                                P_RSum += EdgeLineBuffer[k].Buffer[(RHeight - i + kk) * Width + (RWidth - i + kk)];
                                            }

                                            for (kk = 1; kk <= Cmp_Pixel; kk++)
                                            {
                                                P_LSum += EdgeLineBuffer[k].Buffer[(RHeight - i - kk) * Width + (RWidth - i - kk)];
                                            }

                                            P_RAvg = P_RSum / Cmp_Pixel;
                                            P_LAvg = P_LSum / Cmp_Pixel;
                                        }

                                        if (Math.Abs(P_RAvg - P_LAvg) > 10)
                                        {
                                            if ((i > (Width / 2 - 1) - (sx / 2 - 1)) && (i < (Width / 2 - 1) - (sx / 2 - 1) + sx))
                                            {
                                                MaxDiffValue = 0;

                                                if (k == 0)
                                                {
                                                    for (m = -5; m < 5; m++)
                                                    {
                                                        if (Math.Abs(EdgeLineBuffer[k].Buffer[(i + m) * Width + ((RWidth) - (i + m))] - EdgeLineBuffer[k].Buffer[(i + m + 1) * Width + ((RWidth) - (i + m + 1))]) > MaxDiffValue)
                                                        {
                                                            MaxDiffValue = Math.Abs(EdgeLineBuffer[k].Buffer[(i + m) * Width + ((RWidth) - (i + m))] - EdgeLineBuffer[k].Buffer[(i + m + 1) * Width + ((RWidth) - (i + m + 1))]);

                                                            TempPos.X = (RWidth) - (i + m + 1);
                                                            TempPos.Y = (i + m + 1);
                                                        }
                                                    }

                                                    if (ret_Edge0_count == rsize)
                                                    {
                                                        rsize *= 2;

                                                        //ret_Edge0 = (CvPoint*)realloc(ret_Edge0, sizeof(CvPoint) * rsize);
                                                    }

                                                    ret_Edge0[ret_Edge0_count] = TempPos;
                                                    ret_PMEdge0[ret_Edge0_count] = (int)EdgeOVal[k, (int)(RWidth - TempPos.X)];
                                                    ret_Edge0_count++;
                                                }
                                                else if (k == 1)
                                                {
                                                    for (m = -5; m <= 5; m++)
                                                    {
                                                        if (Math.Abs(EdgeLineBuffer[k].Buffer[(i + m) * Width + (i + m)] - EdgeLineBuffer[k].Buffer[(i + m + 1) * Width + (i + m + 1)]) > MaxDiffValue)
                                                        {
                                                            MaxDiffValue = Math.Abs(EdgeLineBuffer[k].Buffer[(i + m) * Width + (i + m)] - EdgeLineBuffer[k].Buffer[(i + m + 1) * Width + (i + m + 1)]);

                                                            TempPos.X = (i + m + 1);
                                                            TempPos.Y = (i + m + 1);
                                                        }
                                                    }

                                                    if (ret_Edge1_count == rsize)
                                                    {
                                                        rsize *= 2;

                                                        //ret_Edge1 = (CvPoint*)realloc(ret_Edge1, sizeof(CvPoint) * rsize);
                                                    }

                                                    ret_Edge1[ret_Edge1_count] = TempPos;
                                                    ret_PMEdge1[ret_Edge1_count] = (int)EdgeOVal[k, (int)TempPos.X];
                                                    ret_Edge1_count++;
                                                }
                                                else if (k == 2)
                                                {
                                                    for (m = -5; m <= 5; m++)
                                                    {
                                                        if (Math.Abs(EdgeLineBuffer[k].Buffer[((RHeight) - (i + m)) * Width + (i + m)] - EdgeLineBuffer[k].Buffer[((RHeight) - (i + m + 1)) * Width + (i + m + 1)]) > MaxDiffValue)
                                                        {
                                                            MaxDiffValue = Math.Abs(EdgeLineBuffer[k].Buffer[((RHeight) - (i + m)) * Width + (i + m)] - EdgeLineBuffer[k].Buffer[((RHeight) - (i + m + 1)) * Width + (i + m + 1)]);

                                                            TempPos.X = (i + m + 1);
                                                            TempPos.Y = (RHeight) - (i + m + 1);
                                                        }

                                                    }

                                                    if (ret_Edge2_count == rsize)
                                                    {
                                                        rsize *= 2;

                                                        //ret_Edge2 = (CvPoint*)realloc(ret_Edge2, sizeof(CvPoint) * rsize);
                                                    }

                                                    ret_Edge2[ret_Edge2_count] = TempPos;
                                                    ret_PMEdge2[ret_Edge2_count] = (int)EdgeOVal[k, (int)TempPos.X];
                                                    ret_Edge2_count++;
                                                }
                                                else if (k == 3)
                                                {
                                                    for (m = -5; m <= 5; m++)
                                                    {
                                                        if (Math.Abs(EdgeLineBuffer[k].Buffer[((RHeight) - (i + m)) * Width + ((RWidth) - (i + m))] - EdgeLineBuffer[k].Buffer[((RHeight) - (i + m + 1)) * Width + ((RWidth) - (i + m + 1))]) > MaxDiffValue)
                                                        {
                                                            MaxDiffValue = Math.Abs(EdgeLineBuffer[k].Buffer[((RHeight) - (i + m)) * Width + ((RWidth) - (i + m))] - EdgeLineBuffer[k].Buffer[((RHeight) - (i + m + 1)) * Width + ((RWidth) - (i + m + 1))]);

                                                            TempPos.X = (RWidth) - (i + m + 1);
                                                            TempPos.Y = (RHeight) - (i + m + 1);
                                                        }
                                                    }

                                                    if (ret_Edge3_count == rsize)
                                                    {
                                                        rsize *= 3;

                                                        //ret_Edge3 = (CvPoint*)realloc(ret_Edge3, sizeof(CvPoint) * rsize);
                                                    }

                                                    ret_Edge3[ret_Edge3_count] = TempPos;
                                                    ret_PMEdge3[ret_Edge3_count] = (int)EdgeOVal[k, (int)(RWidth - TempPos.X)];
                                                    ret_Edge3_count++;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if ((ret_Edge0_count <= 0) || (ret_Edge1_count <= 0) || (ret_Edge2_count <= 0) || (ret_Edge3_count <= 0)
                        )
                    {
                        // ERROR
                        for (int index = 0; index < EdgeBuffer.Length; index++)
                        {
                            string path = $"C:\\Logs\\Image\\PolishWafer\\FailImage\\EdgeFailImage_{DateTime.Now.ToString("yyyyMMdd_hhmmss")}[{index}].bmp";
                            this.VisionManager().SaveImageBuffer(EdgeBuffer[index], path, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.NONE);
                        }

                        for (int index = 0; index < EdgeLineBuffer.Length; index++)
                        {
                            string path = $"C:\\Logs\\Image\\PolishWafer\\FailImage\\EdgeProcFailImage_{DateTime.Now.ToString("yyyyMMdd_hhmmss")}[{index}].bmp";
                            this.VisionManager().SaveImageBuffer(EdgeBuffer[index], path, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.NONE);
                        }
                    }

                    int p0, p1, p2, p3;

                    int p_0_2;
                    int p_1_3;

                    double Len1, Len2;

                    bool DIAGFLAG0to2 = false;
                    bool DIAGFLAG1to3 = false;

                    int Interval_DiagPoint = 80;

                    int All_Count;

                    All_Count = ret_Edge0_count * ret_Edge1_count * ret_Edge2_count * ret_Edge3_count;

                    Point[,] Real_Pos = new Point[EdgePos.Count, All_Count];

                    //Point[] Real_Pos0 = new Point[All_Count];
                    //Point[] Real_Pos1 = new Point[All_Count];
                    //Point[] Real_Pos2 = new Point[All_Count];
                    //Point[] Real_Pos3 = new Point[All_Count];

                    double[] Real_Score = new double[All_Count];

                    int Real_Count = 0;

                    double Symmetry_Score;

                    for (p0 = 0; p0 < ret_Edge0_count; p0++)
                    {
                        for (p1 = 0; p1 < ret_Edge1_count; p1++)
                        {
                            for (p2 = 0; p2 < ret_Edge2_count; p2++)
                            {
                                for (p3 = 0; p3 < ret_Edge3_count; p3++)
                                {
                                    if (((ret_PMEdge0[p0] > 0) && (ret_PMEdge1[p1] > 0) && (ret_PMEdge2[p2] > 0) && (ret_PMEdge3[p3] > 0)) ||
                                        ((ret_PMEdge0[p0] < 0) && (ret_PMEdge1[p1] < 0) && (ret_PMEdge2[p2] < 0) && (ret_PMEdge3[p3] < 0))
                                        )
                                    {
                                        Len1 = Math.Abs((ret_Edge0[p0].X + Width) - ret_Edge2[p2].X);
                                        Len2 = Math.Abs(ret_Edge1[p1].X - (ret_Edge3[p3].X + Width));

                                        if (Math.Abs(Len1 - Len2) < 50)
                                        {
                                            DIAGFLAG0to2 = false;
                                            DIAGFLAG1to3 = false;

                                            // Check  Diag Point (0) and (2), (1) and (3)

                                            for (p_0_2 = 0; p_0_2 < ret_Edge2_count; p_0_2++)
                                            {
                                                if ((((Width + ret_Edge0[p0].X) - (ret_Edge2[p_0_2].X)) < Width + Interval_DiagPoint))
                                                {
                                                    DIAGFLAG0to2 = true;
                                                }
                                            }

                                            for (p_1_3 = 0; p_1_3 < ret_Edge3_count; p_1_3++)
                                            {
                                                if ((((Width + ret_Edge3[p_1_3].X) - (ret_Edge1[p1].X)) < Width + Interval_DiagPoint))
                                                {
                                                    DIAGFLAG1to3 = true;
                                                }
                                            }

                                            if ((DIAGFLAG0to2 == true) && (DIAGFLAG1to3 == true))
                                            {
                                                Symmetry_Score = 0;

                                                for (int mm = 0; mm < 25; mm++)
                                                {
                                                    if ((ret_Edge0[p0].X > Except_Pixel) && (ret_Edge0[p0].X < Width - Except_Pixel) &&
                                                    (ret_Edge1[p1].X > Except_Pixel) && (ret_Edge1[p1].X < Width - Except_Pixel) &&
                                                    (ret_Edge2[p2].X > Except_Pixel) && (ret_Edge2[p2].X < Width - Except_Pixel) &&
                                                    (ret_Edge3[p3].X > Except_Pixel) && (ret_Edge3[p3].X < Width - Except_Pixel) &&
                                                    (ret_Edge0[p0].Y > Except_Pixel) && (ret_Edge0[p0].Y < Heigh - Except_Pixel) &&
                                                    (ret_Edge1[p1].Y > Except_Pixel) && (ret_Edge1[p1].Y < Heigh - Except_Pixel) &&
                                                    (ret_Edge2[p2].Y > Except_Pixel) && (ret_Edge2[p2].Y < Heigh - Except_Pixel) &&
                                                    (ret_Edge3[p3].Y > Except_Pixel) && (ret_Edge3[p3].Y < Heigh - Except_Pixel))
                                                    {
                                                        Symmetry_Score += Math.Abs(EdgeLineBuffer[0].Buffer[((int)ret_Edge0[p0].Y + mm) * Width + ((int)ret_Edge0[p0].X - mm)] - EdgeLineBuffer[1].Buffer[(int)(ret_Edge1[p1].Y + mm) * Width + ((int)ret_Edge1[p1].X + mm)]) +
                                                                          Math.Abs(EdgeLineBuffer[0].Buffer[(int)(ret_Edge0[p0].Y + mm) * Width + ((int)ret_Edge0[p0].X - mm)] - EdgeLineBuffer[2].Buffer[(int)(ret_Edge2[p2].Y + mm) * Width + ((int)ret_Edge1[p1].X + mm)]) +
                                                                          Math.Abs(EdgeLineBuffer[0].Buffer[(int)(ret_Edge0[p0].Y + mm) * Width + ((int)ret_Edge0[p0].X - mm)] - EdgeLineBuffer[3].Buffer[(int)(ret_Edge3[p3].Y + mm) * Width + ((int)ret_Edge1[p1].X + mm)]) +
                                                                          Math.Abs(EdgeLineBuffer[1].Buffer[(int)(ret_Edge1[p1].Y + mm) * Width + ((int)ret_Edge1[p1].X + mm)] - EdgeLineBuffer[2].Buffer[(int)(ret_Edge2[p2].Y - mm) * Width + ((int)ret_Edge2[p2].X + mm)]) +
                                                                          Math.Abs(EdgeLineBuffer[1].Buffer[(int)(ret_Edge1[p1].Y + mm) * Width + ((int)ret_Edge1[p1].X + mm)] - EdgeLineBuffer[3].Buffer[(int)(ret_Edge3[p3].Y - mm) * Width + ((int)ret_Edge2[p2].X + mm)]) +
                                                                          Math.Abs(EdgeLineBuffer[2].Buffer[(int)(ret_Edge2[p2].Y - mm) * Width + ((int)ret_Edge2[p2].X + mm)] - EdgeLineBuffer[3].Buffer[(int)(ret_Edge3[p3].Y - mm) * Width + ((int)ret_Edge3[p3].X - mm)]);
                                                    }
                                                    else
                                                    {
                                                        Symmetry_Score = 99999;
                                                    }
                                                }

                                                Real_Pos[0, Real_Count].X = ret_Edge0[p0].X;
                                                Real_Pos[0, Real_Count].Y = ret_Edge0[p0].Y;

                                                Real_Pos[1, Real_Count].X = ret_Edge1[p1].X;
                                                Real_Pos[1, Real_Count].Y = ret_Edge1[p1].Y;

                                                Real_Pos[2, Real_Count].X = ret_Edge2[p2].X;
                                                Real_Pos[2, Real_Count].Y = ret_Edge2[p2].Y;

                                                Real_Pos[3, Real_Count].X = ret_Edge3[p3].X;
                                                Real_Pos[3, Real_Count].Y = ret_Edge3[p3].Y;

                                                Real_Score[Real_Count] = Symmetry_Score;

                                                Real_Count++;

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    for (i = 0; i < Real_Count - 1; i++)
                    {
                        for (ii = 1; ii < Real_Count - i; ii++)
                        {
                            if (Real_Score[ii - 1] > Real_Score[ii])
                            {
                                SWAP(Real_Pos[0, ii - 1].X, Real_Pos[0, ii].X);
                                SWAP(Real_Pos[0, ii - 1].Y, Real_Pos[0, ii].Y);

                                SWAP(Real_Pos[1, ii - 1].X, Real_Pos[1, ii].X);
                                SWAP(Real_Pos[1, ii - 1].Y, Real_Pos[1, ii].Y);

                                SWAP(Real_Pos[2, ii - 1].X, Real_Pos[2, ii].X);
                                SWAP(Real_Pos[2, ii - 1].Y, Real_Pos[2, ii].Y);

                                SWAP(Real_Pos[3, ii - 1].X, Real_Pos[3, ii].X);
                                SWAP(Real_Pos[3, ii - 1].Y, Real_Pos[3, ii].Y);

                                SWAP(Real_Score[ii - 1], Real_Score[ii]);
                            }
                        }
                    }

                    if (Real_Count > 0)
                    {

                        for (kk = 0; kk < Real_Count; kk++)
                        {
                            for (k = 0; k < EdgePos.Count; k++)
                            {
                                double offsetx = 0;
                                double offsety = 0;

                                //offsetx = (CurCam.GetGrabSizeWidth() / 2) - Math.Abs(Real_Pos[k, kk].X);
                                //offsety = (CurCam.GetGrabSizeHeight() / 2) - Math.Abs(Real_Pos[k, kk].Y);
                                offsetx = Real_Pos[k, kk].X - (CurCam.GetGrabSizeWidth() / 2);
                                offsety = (CurCam.GetGrabSizeHeight() / 2) - Real_Pos[k, kk].Y;

                                if (Real_Pos[k, kk].X != 0 && Real_Pos[k, kk].Y != 0)
                                {
                                    LoggerManager.Debug(string.Format("Pixel X : {0} , Pixel Y :{1}",
                                     Real_Pos[k, kk].X, Real_Pos[k, kk].Y));
                                }

                                offsetx *= CurCam.GetRatioX();
                                offsety *= CurCam.GetRatioY();

                                this.MotionManager().SetSettlingTime(axisX, 0.00001);
                                this.MotionManager().SetSettlingTime(axisY, 0.00001);

                                wafercoord = new WaferCoordinate();

                                wafercoord.X.Value = (EdgePos[k].X.Value + offsetx);
                                wafercoord.Y.Value = (EdgePos[k].Y.Value + offsety);

                                retVal = EventCodeEnum.NONE;

                                procresults.Add(new WaferProcResult(wafercoord, retVal));
                            }

                            retVal = Calculation();

                            if (retVal == EventCodeEnum.NONE)
                            {
                                break;
                            }
                            else
                            {
                                for (int index = 0; index < EdgeBuffer.Length; index++)
                                {
                                    string path = $"C:\\Logs\\Image\\PolishWafer\\FailImage\\EdgeFailImage_{DateTime.Now.ToString("yyyyMMdd_hhmmss")}[{index}].bmp";
                                    this.VisionManager().SaveImageBuffer(EdgeBuffer[index], path, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.NONE);
                                }
                                for (int index = 0; index < EdgeLineBuffer.Length; index++)
                                {
                                    string path = $"C:\\Logs\\Image\\PolishWafer\\FailImage\\EdgeProcFailImage_{DateTime.Now.ToString("yyyyMMdd_hhmmss")}[{index}].bmp";
                                    this.VisionManager().SaveImageBuffer(EdgeLineBuffer[index], path, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.NONE);
                                }
                            }
                        }
                    }
                    else
                    {
                        retVal = EventCodeEnum.POLISHWAFER_CENTERING_ERROR;

                        procresults.Add(new WaferProcResult(wafercoord, retVal));
                    }
                }
                else
                {
                    for (int index = 0; index < EdgeBuffer.Length; index++)
                    {
                        //Save Fail Images
                        string edgepath = @"C:\Logs\Image\PolishWaferEdgeImage\PolishWaferEdge" + index + ".bmp";
                        this.VisionManager().SaveImageBuffer(EdgeBuffer[index], edgepath, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.NONE);
                    }

                    retVal = EventCodeEnum.POLISHWAFER_CENTERING_ERROR;
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.POLISHWAFER_CENTERING_ERROR;
                LoggerManager.Debug($"{err.ToString()}.CenteringStndard - Processing() : Error occured.");
            }
            finally
            {
                // POLISHWAFER_CENTERING_ERROR for Testing
                if ((retVal == EventCodeEnum.NONE || retVal == EventCodeEnum.UNDEFINED) &&
                    this.PolishWaferModule().PolishWaferControlItems.POLISHWAFER_CENTERING_ERROR == true)
                {
                    retVal = EventCodeEnum.POLISHWAFER_CENTERING_ERROR;
                }

                if (retVal == EventCodeEnum.NONE)
                {
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Polishwafer_Centering_OK);
                }
                else
                {
                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Polishwafer_Centering_Failure, EventCodeEnum.NONE, retVal.ToString());
                    this.NotifyManager().Notify(EventCodeEnum.POLISHWAFER_CENTERING_ERROR);
                }
            }

            //LoggerManager.Funclog(typeof(PolishWaferCentering_Standard), EnumFuncCallingTime.END);

            return retVal;
        }

        private EventCodeEnum Calculation()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                try
                {
                    double a = 0.0;
                    double b = 0.0;
                    double c = 0.0;
                    double d = 0.0;
                    double e = 0.0;
                    double f = 0.0;

                    double chuckzeroAveXpos = 0.0;
                    double chuckzeroAveYpos = 0.0;

                    Point[] tmpGCPWaferCen = new Point[4];

                    LoggerManager.Debug($"Q1 xpos:{procresults[0].ResultPos.X.Value} ypos{procresults[0].ResultPos.Y.Value}", isInfo: true);
                    LoggerManager.Debug($"Q2 xpos:{procresults[1].ResultPos.X.Value} ypos{procresults[1].ResultPos.Y.Value}", isInfo: true);
                    LoggerManager.Debug($"Q3 xpos:{procresults[2].ResultPos.X.Value} ypos{procresults[2].ResultPos.Y.Value}", isInfo: true);
                    LoggerManager.Debug($"Q4 xpos:{procresults[3].ResultPos.X.Value} ypos{procresults[3].ResultPos.Y.Value}", isInfo: true);

                    double distancex = 2 * (procresults[1].ResultPos.X.Value - procresults[0].ResultPos.X.Value);
                    double distancey = 2 * (procresults[1].ResultPos.Y.Value - procresults[0].ResultPos.Y.Value);

                    //case1

                    a = 2 * (procresults[1].ResultPos.X.Value - procresults[0].ResultPos.X.Value);
                    b = 2 * (procresults[1].ResultPos.Y.Value - procresults[0].ResultPos.Y.Value);
                    c = Math.Pow(procresults[1].ResultPos.X.Value, 2.0) - Math.Pow(procresults[0].ResultPos.X.Value, 2.0) +
                        Math.Pow(procresults[1].ResultPos.Y.Value, 2.0) - Math.Pow(procresults[0].ResultPos.Y.Value, 2.0);
                    d = 2 * (procresults[2].ResultPos.X.Value - procresults[0].ResultPos.X.Value);
                    e = 2 * (procresults[2].ResultPos.Y.Value - procresults[0].ResultPos.Y.Value);
                    f = Math.Pow(procresults[2].ResultPos.X.Value, 2.0) - Math.Pow(procresults[0].ResultPos.X.Value, 2.0) +
                        Math.Pow(procresults[2].ResultPos.Y.Value, 2.0) - Math.Pow(procresults[0].ResultPos.Y.Value, 2.0);

                    tmpGCPWaferCen[0].X = ((c * e - f * b) / (e * a - b * d));
                    tmpGCPWaferCen[0].Y = ((c * d - a * f) / (d * b - a * e));

                    //case2
                    a = 2 * (procresults[2].ResultPos.X.Value - procresults[1].ResultPos.X.Value);
                    b = 2 * (procresults[2].ResultPos.Y.Value - procresults[1].ResultPos.Y.Value);
                    c = Math.Pow(procresults[2].ResultPos.X.Value, 2.0) - Math.Pow(procresults[1].ResultPos.X.Value, 2.0) +
                        Math.Pow(procresults[2].ResultPos.X.Value, 2.0) - Math.Pow(procresults[1].ResultPos.Y.Value, 2.0);
                    d = 2 * (procresults[3].ResultPos.X.Value - procresults[1].ResultPos.X.Value);
                    e = 2 * (procresults[3].ResultPos.Y.Value - procresults[1].ResultPos.Y.Value);
                    f = Math.Pow(procresults[3].ResultPos.X.Value, 2.0) - Math.Pow(procresults[1].ResultPos.X.Value, 2.0) +
                        Math.Pow(procresults[3].ResultPos.Y.Value, 2.0) - Math.Pow(procresults[1].ResultPos.Y.Value, 2.0);

                    tmpGCPWaferCen[1].X = ((c * e - f * b) / (e * a - b * d));
                    tmpGCPWaferCen[1].Y = ((c * d - a * f) / (d * b - a * e));

                    //case3
                    a = 2 * (procresults[3].ResultPos.X.Value - procresults[2].ResultPos.X.Value);
                    b = 2 * (procresults[3].ResultPos.Y.Value - procresults[2].ResultPos.Y.Value);
                    c = Math.Pow(procresults[3].ResultPos.X.Value, 2.0) - Math.Pow(procresults[2].ResultPos.X.Value, 2.0) +
                        Math.Pow(procresults[3].ResultPos.Y.Value, 2.0) - Math.Pow(procresults[2].ResultPos.Y.Value, 2.0);
                    d = 2 * (procresults[0].ResultPos.X.Value - procresults[2].ResultPos.X.Value);
                    e = 2 * (procresults[0].ResultPos.Y.Value - procresults[2].ResultPos.Y.Value);
                    f = Math.Pow(procresults[0].ResultPos.X.Value, 2.0) - Math.Pow(procresults[2].ResultPos.X.Value, 2.0) +
                        Math.Pow(procresults[0].ResultPos.Y.Value, 2.0) - Math.Pow(procresults[2].ResultPos.Y.Value, 2.0);

                    tmpGCPWaferCen[2].X = ((c * e - f * b) / (e * a - b * d));
                    tmpGCPWaferCen[2].Y = ((c * d - a * f) / (d * b - a * e));

                    //case4
                    a = 2 * (procresults[0].ResultPos.X.Value - procresults[3].ResultPos.X.Value);
                    b = 2 * (procresults[0].ResultPos.Y.Value - procresults[3].ResultPos.Y.Value);
                    c = Math.Pow(procresults[0].ResultPos.X.Value, 2.0) - Math.Pow(procresults[3].ResultPos.X.Value, 2.0) +
                        Math.Pow(procresults[0].ResultPos.Y.Value, 2.0) - Math.Pow(procresults[3].ResultPos.Y.Value, 2.0);
                    d = 2 * (procresults[1].ResultPos.X.Value - procresults[3].ResultPos.X.Value);
                    e = 2 * (procresults[1].ResultPos.Y.Value - procresults[3].ResultPos.Y.Value);
                    f = Math.Pow(procresults[1].ResultPos.X.Value, 2.0) - Math.Pow(procresults[3].ResultPos.X.Value, 2.0) +
                        Math.Pow(procresults[1].ResultPos.Y.Value, 2.0) - Math.Pow(procresults[3].ResultPos.Y.Value, 2.0);

                    tmpGCPWaferCen[3].X = ((c * e - f * b) / (e * a - b * d));
                    tmpGCPWaferCen[3].Y = ((c * d - a * f) / (d * b - a * e));

                    bool[] CEN_CHECK_FLAG = new bool[2];

                    CEN_CHECK_FLAG[0] = true;
                    CEN_CHECK_FLAG[1] = true;


                    if ((CEN_CHECK_FLAG[0] == true) && (CEN_CHECK_FLAG[1] == true))
                    {
                        chuckzeroAveXpos = (tmpGCPWaferCen[0].X + tmpGCPWaferCen[1].X + tmpGCPWaferCen[2].X + tmpGCPWaferCen[3].X) / 4;
                        chuckzeroAveYpos = (tmpGCPWaferCen[0].Y + tmpGCPWaferCen[1].Y + tmpGCPWaferCen[2].Y + tmpGCPWaferCen[3].Y) / 4;

                        LoggerManager.Debug($"Polish Wafer Center xpos : {chuckzeroAveXpos}, ypos : {chuckzeroAveYpos}", isInfo:true);

                        double[] CLength = new double[procresults.Count()];
                        double[] CLengthRDiff = new double[procresults.Count()];
                        double lRadius;

                        lRadius = Math.Sqrt(Math.Pow((EdgePos[2].X.Value - EdgePos[0].X.Value), 2) + Math.Pow((EdgePos[2].Y.Value - EdgePos[0].Y.Value), 2)) / 2.0;

                        for (int i = 0; i < procresults.Count(); i++)
                        {
                            CLength[i] = Math.Sqrt(Math.Pow((procresults[i].ResultPos.X.Value - chuckzeroAveXpos), 2) + Math.Pow((procresults[i].ResultPos.Y.Value - chuckzeroAveYpos), 2));
                            CLengthRDiff[i] = Math.Abs(lRadius - CLength[i]);
                        }

                        LoggerManager.Debug($"Distance Q1 : {CLength[0]}", isInfo:true);
                        LoggerManager.Debug($"Distance Q2 : {CLength[1]}", isInfo:true);
                        LoggerManager.Debug($"Distance Q3 : {CLength[2]}", isInfo:true);
                        LoggerManager.Debug($"Distance Q4 : {CLength[3]}", isInfo:true);

                        //if ((CLengthRDiff[0] < EdgeStandardParam_Clone.gIntEdgeDetectProcToleranceRad.Value) &&
                        //    (CLengthRDiff[1] < EdgeStandardParam_Clone.gIntEdgeDetectProcToleranceRad.Value) &&
                        //    (CLengthRDiff[2] < EdgeStandardParam_Clone.gIntEdgeDetectProcToleranceRad.Value) &&
                        //    (CLengthRDiff[3] < EdgeStandardParam_Clone.gIntEdgeDetectProcToleranceRad.Value))
                        //{

                        IPolishWaferSourceInformation pwinfo = this.StageSupervisor().WaferObject.GetPolishInfo();

                        pwinfo.PolishWaferCenter.X.Value = chuckzeroAveXpos;
                        pwinfo.PolishWaferCenter.Y.Value = chuckzeroAveYpos;

                        WaferCoordinate coordinate = this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert();

                        retVal = EventCodeEnum.NONE;
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Debug($"{err.ToString()}. CenteringStndard - Calculation() : Error occured.");
                    throw err;
                }
                finally
                {
                    //ProcessDialog.CloseDialg(this);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        private void SWAP(double a, double b)
        {
            try
            {
                double temp = a;
                a = b;
                b = temp;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion
    }
}
