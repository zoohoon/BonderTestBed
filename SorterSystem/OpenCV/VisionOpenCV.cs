using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SorterSystem.OpenCV
{
    public enum eObjDir
    {
        eObjDir_LeftTop,
        eObjDir_LeftBot,
        eObjDir_RightTop,
        eObjDir_RightBot,
    }

    public struct stObject
    {
        public OpenCvSharp.Point2d posCenter;
        public OpenCvSharp.Point2d posVertex;
        public eObjDir eDiePos;

        public void reset()
        {
            eDiePos = eObjDir.eObjDir_LeftTop;
            posCenter.X = posCenter.Y = 0;
            posVertex.X = posVertex.Y = 0;
        }
    };

    public struct stMachineResult
    {
        public OpenCvSharp.Point2d posCenter;
        public OpenCvSharp.Rect posRect;
        public double dScore;
        public double dAngle;

        public void reset()
        {
            posCenter.X = posCenter.Y = 0;
            posRect.X = posRect.Y = posRect.Width = posRect.Height = 0;
            dScore = dAngle = 0;
        }
    };

    public partial class VisionOpenCV
    {
        public static void OpenCVFindEdgeThres(
            Mat matOrg,
            Mat matDbg,
            Parameters param,
            out Dictionary<string, Mat> debugMats,
            out List<stObject> _outResult)
        {
            Dictionary<string, OpenCvSharp.Rect> resRectHs = new Dictionary<string, OpenCvSharp.Rect>();
            Dictionary<string, OpenCvSharp.Rect> resRectVs = new Dictionary<string, OpenCvSharp.Rect>();
            stObject stObj = new stObject();
            stObj.reset();

            debugMats = new Dictionary<string, Mat>();
            _outResult = new List<stObject>();

            Mat matThres = new Mat();
            if (matOrg.Type() != MatType.CV_8U)
                Cv2.CvtColor(matOrg, matOrg, ColorConversionCodes.BGR2GRAY);
            Mat matRansac = matDbg.Clone();
            Mat matOrigin = matDbg.Clone();
            debugMats.Add("[0] Origin", matOrigin);

            Mat matTmp = new Mat();
            if (param.ImageInvert)
            {
                Mat matInv = new Mat();
                matInv = matOrg.Clone();
                Cv2.Subtract(255, matInv, matTmp);
            }
            else
                matTmp = matOrg.Clone();

            Cv2.Threshold(matTmp, matThres, param.ThresholdMin, param.ThresholdMax, /*ThresholdTypes.Otsu*/ThresholdTypes.Binary);
            debugMats.Add("[1] Threshold", matThres);

            matTmp.Release();

            Mat matOpen = new Mat();
            Mat elementOpen = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(param.MorphologyOpen, param.MorphologyOpen));
            Cv2.MorphologyEx(matThres, matOpen, MorphTypes.Open, elementOpen, iterations: 1);
            debugMats.Add("[2] Open", matOpen);

            Mat matClose = new Mat();
            Mat elementClose = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(param.MorphologyClose, param.MorphologyClose));
            Cv2.MorphologyEx(matOpen, matClose, MorphTypes.Close, elementClose, iterations: 1);
            debugMats.Add("[3] Close", matClose);

            Mat dst = new Mat();
            Cv2.CvtColor(matOrg, dst, ColorConversionCodes.GRAY2BGR);
            OpenCvSharp.Point[][] contours;
            HierarchyIndex[] hierarchy;

            Mat matCloseInv = new Mat();
            Cv2.Subtract(255, matClose, matCloseInv);

            Mat matLabelImg = new Mat();
            Mat matStats = new Mat();
            Mat matCentroids = new Mat();
            Mat matBlob = Mat.Zeros(matCloseInv.Rows, matCloseInv.Cols, matCloseInv.Type());
            Mat matBlobs = new Mat(matCloseInv.Size(), matCloseInv.Type());

            int nNumOfLabels = Cv2.ConnectedComponentsWithStats(matCloseInv, matLabelImg, matStats, matCentroids);

            if (0 < nNumOfLabels)
            {
                int nContourCnt = nNumOfLabels;
                bool bContourOk = false;
                if (nContourCnt >= 2)
                    bContourOk = true;

                double[] refAH = new double[nContourCnt];
                double[] refBH = new double[nContourCnt];
                double[] refAV = new double[nContourCnt];
                double[] refBV = new double[nContourCnt];

                for (int i = 1; i < (int)nNumOfLabels; ++i)
                {
                    int nStatusTop = matStats.At<int>(i, (int)ConnectedComponentsTypes.Top);
                    int nStatusLeft = matStats.At<int>(i, (int)ConnectedComponentsTypes.Left);
                    int nStatusArea = matStats.At<int>(i, (int)ConnectedComponentsTypes.Area);
                    int nStatusWidth = matStats.At<int>(i, (int)ConnectedComponentsTypes.Width);
                    int nStatusHeight = matStats.At<int>(i, (int)ConnectedComponentsTypes.Height);

                    if (nStatusArea > param.BlobMinArea)
                    {
                        Mat matLabel = new Mat();
                        Cv2.InRange(matLabelImg, i, i, matLabel);
                        Cv2.BitwiseOr(matLabel, matBlob, matBlob);
                        matLabel.Release();
                    }
                    else
                        continue;

                    if (matBlob != null)
                    {
                        Cv2.BitwiseOr(matBlob, matBlobs, matBlobs);

                        Mat matBlobDraw = matBlob.Clone();
                        Mat matBlobFind = matBlob.Clone();
                        Cv2.CvtColor(matBlobDraw, matBlobDraw, ColorConversionCodes.GRAY2BGR);

                        OpenCvSharp.Point posCenter = new OpenCvSharp.Point();
                        posCenter.X = nStatusLeft + nStatusWidth / 2;
                        posCenter.Y = nStatusTop + nStatusHeight / 2;
                        Cv2.Line(matBlobDraw, new OpenCvSharp.Point(posCenter.X - 10, posCenter.Y), new OpenCvSharp.Point(posCenter.X + 10, posCenter.Y), new Scalar(0, 0, 255), 2);
                        Cv2.Line(matBlobDraw, new OpenCvSharp.Point(posCenter.X, posCenter.Y - 10), new OpenCvSharp.Point(posCenter.X, posCenter.Y + 10), new Scalar(0, 0, 255), 2);
                        stObj.posCenter.X = posCenter.X;
                        stObj.posCenter.Y = posCenter.Y;

                        Cv2.FindContours(matBlobFind, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                        //Blob의 위치 정보를 업데이트 한다.
                        if (posCenter.X <= matBlob.Cols / 2 && posCenter.Y <= matBlob.Rows / 2)
                            stObj.eDiePos = eObjDir.eObjDir_LeftTop;
                        else if (posCenter.X <= matBlob.Cols / 2 && posCenter.Y > matBlob.Rows / 2)
                            stObj.eDiePos = eObjDir.eObjDir_LeftBot;
                        else if (posCenter.X > matBlob.Cols / 2 && posCenter.Y <= matBlob.Rows / 2)
                            stObj.eDiePos = eObjDir.eObjDir_RightTop;
                        else if (posCenter.X > matBlob.Cols / 2 && posCenter.Y > matBlob.Rows / 2)
                            stObj.eDiePos = eObjDir.eObjDir_RightBot;

                        if (param.LineDetection)
                        {
                            OpenCvSharp.Rect rcOutLineRoi = new OpenCvSharp.Rect(nStatusLeft, nStatusTop, nStatusWidth, nStatusHeight);
                            List<stRoiData> _roiDatas;
                            //UpdateRoi(matBlobFind, matBlobDraw, rcOutLineRoi, stObj.eDiePos, out _roiDatas);
                            UpdateRoiEx(matBlobFind, matBlobDraw, rcOutLineRoi, stObj.eDiePos, out _roiDatas);
                            if (_roiDatas.Count() != 2)
                                return;

                            OpenCvSharp.Rect rcRoiH = new OpenCvSharp.Rect();
                            OpenCvSharp.Rect rcRoiV = new OpenCvSharp.Rect();
                            rcRoiV = _roiDatas[0].rcRoiPos;
                            rcRoiH = _roiDatas[1].rcRoiPos;

                            {
                                List<OpenCvSharp.Point> posSL = new List<OpenCvSharp.Point>();
                                posSL.Clear();

                                foreach (OpenCvSharp.Point[] p in contours)
                                {
                                    double length = Cv2.ArcLength(p, true);

                                    if (length > 100)
                                    {
                                        for (int nCur = 0; nCur < p.Count(); nCur++)
                                        {
                                            if (p[nCur].X > rcRoiH.Left && p[nCur].X < rcRoiH.Right &&
                                                p[nCur].Y > rcRoiH.Top && p[nCur].Y < rcRoiH.Bottom)
                                            {
                                                posSL.Add(p[nCur]);
                                                posCenter.X = p[nCur].X;
                                                posCenter.Y = p[nCur].Y;

                                                Cv2.Line(matBlobDraw, new OpenCvSharp.Point(posCenter.X - 2, posCenter.Y), new OpenCvSharp.Point(posCenter.X + 2, posCenter.Y), new Scalar(255, 0, 255), 2);
                                                Cv2.Line(matBlobDraw, new OpenCvSharp.Point(posCenter.X, posCenter.Y - 2), new OpenCvSharp.Point(posCenter.X, posCenter.Y + 2), new Scalar(255, 0, 255), 2);
                                            }
                                        }

                                        int nMax = posSL.Count();
                                        OpenCvSharp.Point[] posS = new OpenCvSharp.Point[nMax];
                                        if (nMax > 0)
                                            posSL.CopyTo(posS);

                                        if (bContourOk && nMax > 0)
                                            Test_Ransac_Hori(rcRoiH, posS, nMax, ref refAH[i], ref refBH[i], ref matRansac);
                                    }
                                }
                            }

                            {
                                List<OpenCvSharp.Point> posSL = new List<OpenCvSharp.Point>();
                                posSL.Clear();

                                foreach (OpenCvSharp.Point[] p in contours)
                                {
                                    double length = Cv2.ArcLength(p, true);
                                    if (length > 100)
                                    {
                                        for (int nCur = 0; nCur < p.Count(); nCur++)
                                        {
                                            if (p[nCur].X > rcRoiV.Left && p[nCur].X < rcRoiV.Right &&
                                                p[nCur].Y > rcRoiV.Top && p[nCur].Y < rcRoiV.Bottom)
                                            {
                                                posSL.Add(p[nCur]);
                                                posCenter.X = p[nCur].X;
                                                posCenter.Y = p[nCur].Y;

                                                Cv2.Line(matBlobDraw, new OpenCvSharp.Point(posCenter.X - 2, posCenter.Y), new OpenCvSharp.Point(posCenter.X + 2, posCenter.Y), new Scalar(255, 0, 255), 2);
                                                Cv2.Line(matBlobDraw, new OpenCvSharp.Point(posCenter.X, posCenter.Y - 2), new OpenCvSharp.Point(posCenter.X, posCenter.Y + 2), new Scalar(255, 0, 255), 2);
                                            }
                                        }

                                        int nMax = posSL.Count();
                                        OpenCvSharp.Point[] posS = new OpenCvSharp.Point[nMax];
                                        if (nMax > 0)
                                            posSL.CopyTo(posS);

                                        if (bContourOk && nMax > 0)
                                            Test_Ransac_Vert(rcRoiV, posS, nMax, ref refAV[i], ref refBV[i], ref matRansac);
                                    }
                                }
                            }
                            string strBlob = "[F] matBlob " + i;
                            debugMats.Add(strBlob, matBlobDraw);

                            resRectHs.Add(strBlob, rcRoiH);
                            resRectVs.Add(strBlob, rcRoiV);

                            matBlob = Mat.Zeros(matCloseInv.Rows, matCloseInv.Cols, matCloseInv.Type());

                            //꼭지점을 구하자...
                            double _a = refAV[i];
                            double _b = refBV[i];
                            double _A = refAH[i];
                            double _B = refBH[i];

                            stObj.posVertex.X = (_B - _b) / (_a - _A);
                            stObj.posVertex.Y = _a * (_B - _b) / (_a - _A) + _b;

                            Cv2.Line(matRansac, new OpenCvSharp.Point(stObj.posVertex.X - 10, stObj.posVertex.Y), new OpenCvSharp.Point(stObj.posVertex.X + 10, stObj.posVertex.Y), new Scalar(0, 0, 255), 5);
                            Cv2.Line(matRansac, new OpenCvSharp.Point(stObj.posVertex.X, stObj.posVertex.Y - 10), new OpenCvSharp.Point(stObj.posVertex.X, stObj.posVertex.Y + 10), new Scalar(0, 0, 255), 5);
                        }
                        else
                        {
                            List<OpenCvSharp.Point> posS;
                            OpenCvSharp.Point posVertex;
                            FindVertex(matBlobFind, matBlobDraw, stObj.eDiePos, out posS, out posVertex);

                            if (posS.Count() == 3)
                            {
                                string strBlob = "[F] matBlob " + i;
                                debugMats.Add(strBlob, matBlobDraw);

                                stObj.posVertex = posVertex;

                                if (matRansac.Type() == MatType.CV_8U)
                                    Cv2.CvtColor(matRansac, matRansac, ColorConversionCodes.GRAY2BGR);

                                Cv2.Line(matRansac, new OpenCvSharp.Point(stObj.posVertex.X - 10, stObj.posVertex.Y), new OpenCvSharp.Point(stObj.posVertex.X + 10, stObj.posVertex.Y), new Scalar(0, 0, 255), 5);
                                Cv2.Line(matRansac, new OpenCvSharp.Point(stObj.posVertex.X, stObj.posVertex.Y - 10), new OpenCvSharp.Point(stObj.posVertex.X, stObj.posVertex.Y + 10), new Scalar(0, 0, 255), 5);
                            }
                            else
                            {
                                Console.WriteLine("FindVertex Errror!!! ==> posS::Count=" + posS.Count + "  posVertex:" + posVertex);
                            }

                            matBlob = Mat.Zeros(matCloseInv.Rows, matCloseInv.Cols, matCloseInv.Type());
                        }

                        _outResult.Add(stObj);
                    }
                }

                Mat matResiult = matRansac.Clone();
                string strRansac = "[F] matResult";
                debugMats.Add(strRansac, matResiult);
            }
            matLabelImg.Release();
            dst.Release();
            elementOpen.Release();
            elementClose.Release();
        }

        public static void OpenCVFindEdgeThresEx(Mat _matOrg, Mat _matDbg, Parameters param, out Dictionary<string, Mat> resultMat, out List<stObject> _outResult)
        {
            Dictionary<string, OpenCvSharp.Rect> resRectHs = new Dictionary<string, OpenCvSharp.Rect>();
            Dictionary<string, OpenCvSharp.Rect> resRectVs = new Dictionary<string, OpenCvSharp.Rect>();
            stObject stObj = new stObject();
            stObj.reset();
            resultMat = new Dictionary<string, Mat>();

            _outResult = new List<stObject>();

            Mat matThres = new Mat();
            if (_matOrg.Type() != MatType.CV_8U)
                Cv2.CvtColor(_matOrg, _matOrg, ColorConversionCodes.BGR2GRAY);
            Mat matRansac = _matDbg.Clone();
            Mat matOrigin = _matDbg.Clone();
            resultMat.Add("[0] Origin", matOrigin);

            Mat matTmp = new Mat();
            if (param.ImageInvert)
            {
                Mat matInv = new Mat();
                matInv = _matOrg.Clone();
                Cv2.Subtract(255, matInv, matTmp);
            }
            else
                matTmp = _matOrg.Clone();

            Cv2.Threshold(matTmp, matThres, param.ThresholdMin, param.ThresholdMax, /*ThresholdTypes.Otsu*/ThresholdTypes.Binary);
            //OpenCVDbgViewer(matThres, "[1] Threshold", 0.5);
            resultMat.Add("[1] Threshold", matThres);

            matTmp.Release();

            Mat matOpen = new Mat();
            Mat elementOpen = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(param.MorphologyOpen, param.MorphologyOpen));
            Cv2.MorphologyEx(matThres, matOpen, MorphTypes.Open, elementOpen, iterations: 1);
            //OpenCVDbgViewer(matOpen, "[2] Open", 0.5);
            resultMat.Add("[2] Open", matOpen);

            Mat matClose = new Mat();
            Mat elementClose = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(param.MorphologyClose, param.MorphologyClose));
            Cv2.MorphologyEx(matOpen, matClose, MorphTypes.Close, elementClose, iterations: 1);
            //OpenCVDbgViewer(matClose, "[3] Close", 0.5);
            resultMat.Add("[3] Close", matClose);

            Mat dst = new Mat();
            Cv2.CvtColor(_matOrg, dst, ColorConversionCodes.GRAY2BGR);
            OpenCvSharp.Point[][] contours;
            HierarchyIndex[] hierarchy;

            Mat matCloseInv = new Mat();
            Cv2.Subtract(255, matClose, matCloseInv);

            Mat matLabelImg = new Mat();
            Mat matStats = new Mat();
            Mat matCentroids = new Mat();
            Mat matBlob = Mat.Zeros(matCloseInv.Rows, matCloseInv.Cols, matCloseInv.Type());
            Mat matBlobs = new Mat(matCloseInv.Size(), matCloseInv.Type());

            int nNumOfLabels = Cv2.ConnectedComponentsWithStats(matCloseInv, matLabelImg, matStats, matCentroids);

            if (0 < nNumOfLabels)
            {
                int nContourCnt = nNumOfLabels;
                bool bContourOk = false;
                if (nContourCnt >= 2)
                    bContourOk = true;

                double[] refAH = new double[nContourCnt];
                double[] refBH = new double[nContourCnt];
                double[] refAV = new double[nContourCnt];
                double[] refBV = new double[nContourCnt];

                for (int i = 1; i < (int)nNumOfLabels; ++i)
                {
                    int nStatusTop = matStats.At<int>(i, (int)ConnectedComponentsTypes.Top);
                    int nStatusLeft = matStats.At<int>(i, (int)ConnectedComponentsTypes.Left);
                    int nStatusArea = matStats.At<int>(i, (int)ConnectedComponentsTypes.Area);
                    int nStatusWidth = matStats.At<int>(i, (int)ConnectedComponentsTypes.Width);
                    int nStatusHeight = matStats.At<int>(i, (int)ConnectedComponentsTypes.Height);

                    if (nStatusArea > param.BlobMinArea)
                    {
                        Mat matLabel = new Mat();
                        Cv2.InRange(matLabelImg, i, i, matLabel);
                        Cv2.BitwiseOr(matLabel, matBlob, matBlob);
                        matLabel.Release();
                    }
                    else
                        continue;

                    if (matBlob != null)
                    {
                        Cv2.BitwiseOr(matBlob, matBlobs, matBlobs);

                        Mat matBlobDraw = matBlob.Clone();
                        Mat matBlobFind = matBlob.Clone();
                        Cv2.CvtColor(matBlobDraw, matBlobDraw, ColorConversionCodes.GRAY2BGR);

                        //검출된 Blob 외각 기준 정보를 이용한다.
                        OpenCvSharp.Point posCenter = new OpenCvSharp.Point();
                        posCenter.X = nStatusLeft + nStatusWidth / 2;
                        posCenter.Y = nStatusTop + nStatusHeight / 2;
                        Cv2.Line(matBlobDraw, new OpenCvSharp.Point(posCenter.X - 10, posCenter.Y), new OpenCvSharp.Point(posCenter.X + 10, posCenter.Y), new Scalar(0, 0, 255), 2);
                        Cv2.Line(matBlobDraw, new OpenCvSharp.Point(posCenter.X, posCenter.Y - 10), new OpenCvSharp.Point(posCenter.X, posCenter.Y + 10), new Scalar(0, 0, 255), 2);
                        stObj.posCenter.X = posCenter.X;
                        stObj.posCenter.Y = posCenter.Y;

                        //Blob의 위치 정보를 업데이트 한다.
                        if (posCenter.X <= matBlob.Cols / 2 && posCenter.Y <= matBlob.Rows / 2)
                            stObj.eDiePos = eObjDir.eObjDir_LeftTop;
                        else if (posCenter.X <= matBlob.Cols / 2 && posCenter.Y > matBlob.Rows / 2)
                            stObj.eDiePos = eObjDir.eObjDir_LeftBot;
                        else if (posCenter.X > matBlob.Cols / 2 && posCenter.Y <= matBlob.Rows / 2)
                            stObj.eDiePos = eObjDir.eObjDir_RightTop;
                        else if (posCenter.X > matBlob.Cols / 2 && posCenter.Y > matBlob.Rows / 2)
                            stObj.eDiePos = eObjDir.eObjDir_RightBot;

                        if (param.LineDetection)
                        {
                            OpenCvSharp.Rect rcOutLineRoi = new OpenCvSharp.Rect(nStatusLeft, nStatusTop, nStatusWidth, nStatusHeight);
                            List<stRoiData> _roiDatas;
                            //UpdateRoi(matBlobFind, matBlobDraw, rcOutLineRoi, stObj.eDiePos, out _roiDatas);
                            UpdateRoiEx(matBlobFind, matBlobDraw, rcOutLineRoi, stObj.eDiePos, out _roiDatas);
                            if (_roiDatas.Count() != 2)
                                return;

                            OpenCvSharp.Rect rcRoiH = new OpenCvSharp.Rect();
                            OpenCvSharp.Rect rcRoiV = new OpenCvSharp.Rect();
                            rcRoiV = _roiDatas[0].rcRoiPos;
                            rcRoiH = _roiDatas[1].rcRoiPos;

                            //1     : 수평방향 Roi를 이용하여 검출한다.
                            //1-1   : Roi 이미지를 Crop 하여 평균 이미지를 만든다.
                            //
                            int nRefCnt = 0;
                            List<OpenCvSharp.Point> posSL = new List<OpenCvSharp.Point>();
                            int nGapStep = (int)param.ProfileStep;
                            for (int nY = rcRoiH.Top; nY < rcRoiH.Bottom - nGapStep; nY += nGapStep)
                            {
                                int nSumPixelPrev = -1;
                                OpenCvSharp.Rect rcCrop = new OpenCvSharp.Rect(rcRoiH.X, nY, rcRoiH.Width, nGapStep);
                                if (rcCrop.Left > 0 && rcCrop.Top > 0 && rcCrop.Width > 0 && rcCrop.Height > 0 && rcCrop.Right < matBlob.Cols && rcCrop.Bottom < matBlob.Rows)
                                {
                                    Mat matCropH = new Mat(matBlob, rcCrop).Clone();

                                    //1-2   : 평균이미지를 이용하여 에지 포인트를 구하여, 포인트 어레이에 넣는다.
                                    bool bFound = false;
                                    for (int nXStep = 0; nXStep < rcCrop.Width; nXStep++)
                                    {
                                        if (bFound)
                                            continue;
                                        int nSumPixel = 0;
                                        for (int nYStep = 0; nYStep < nGapStep; nYStep++)
                                        {
                                            nSumPixel += matCropH.At<byte>(nYStep, nXStep);
                                        }
                                        nSumPixel /= nGapStep;

                                        if (nSumPixel > 127)
                                            nSumPixel = 255;
                                        else
                                            nSumPixel = 0;

                                        if (nSumPixelPrev != -1)
                                        {
                                            if (Math.Abs(nSumPixelPrev - nSumPixel) > 100)
                                            {
                                                Point pos = new Point();
                                                pos.X = rcRoiH.Left + nXStep;
                                                pos.Y = nY + rcCrop.Height / 2;
                                                posSL.Add(pos);

                                                Cv2.Line(matBlobDraw, new OpenCvSharp.Point(pos.X - 2, pos.Y), new OpenCvSharp.Point(pos.X + 2, pos.Y), new Scalar(255, 0, 255), 5);
                                                Cv2.Line(matBlobDraw, new OpenCvSharp.Point(pos.X, pos.Y - 2), new OpenCvSharp.Point(pos.X, pos.Y + 2), new Scalar(255, 0, 255), 5);

                                                bFound = true;
                                            }
                                            nSumPixelPrev = nSumPixel;
                                        }
                                        else
                                            nSumPixelPrev = nSumPixel;
                                    }
                                    matCropH.Release();
                                }
                            }

                            //1-3   : Ransac 알고리즘을 돌려서, 직선방정식을 구한다.
                            if (bContourOk)
                            {
                                if (posSL.Count() > 0)
                                {
                                    nRefCnt = posSL.Count();
                                    OpenCvSharp.Point[] posS = new OpenCvSharp.Point[nRefCnt];
                                    posSL.CopyTo(posS);
                                    Test_Ransac_Hori(rcRoiH, posS, nRefCnt, ref refAH[i], ref refBH[i], ref matRansac);
                                }
                            }

                            //2.    : 수직방향 Roi를 이용하여 검출한다.
                            //2-1   : Roi 이미지를 Crop 하여 평균 이미지를 만든다.
                            //
                            nRefCnt = 0;
                            posSL.Clear();
                            for (int nX = rcRoiV.Left; nX < rcRoiV.Right - nGapStep; nX += nGapStep)
                            {
                                int nSumPixelPrev = -1;
                                OpenCvSharp.Rect rcCrop = new OpenCvSharp.Rect(nX, rcRoiV.Y, nGapStep, rcRoiV.Height);

                                if (rcCrop.Left > 0 && rcCrop.Top > 0 && rcCrop.Width > 0 && rcCrop.Height > 0 && rcCrop.Right < matBlob.Cols && rcCrop.Bottom < matBlob.Rows)
                                {
                                    Mat matCropV = new Mat(matBlob, rcCrop).Clone();

                                    //1-2   : 평균이미지를 이용하여 에지 포인트를 구하여, 포인트 어레이에 넣는다.
                                    bool bFound = false;
                                    for (int nYStep = 0; nYStep < rcCrop.Height; nYStep++)
                                    {
                                        if (bFound)
                                            continue;
                                        int nSumPixel = 0;
                                        for (int nXStep = 0; nXStep < nGapStep; nXStep++)
                                        {
                                            nSumPixel += matCropV.At<byte>(nYStep, nXStep);
                                        }
                                        nSumPixel /= nGapStep;

                                        if (nSumPixel > 127)
                                            nSumPixel = 255;
                                        else
                                            nSumPixel = 0;

                                        if (nSumPixelPrev != -1)
                                        {
                                            if (Math.Abs(nSumPixelPrev - nSumPixel) > 100)
                                            {
                                                Point pos = new Point();
                                                pos.X = nX + rcCrop.Width / 2;
                                                pos.Y = rcRoiV.Top + nYStep;
                                                posSL.Add(pos);

                                                Cv2.Line(matBlobDraw, new OpenCvSharp.Point(pos.X - 2, pos.Y), new OpenCvSharp.Point(pos.X + 2, pos.Y), new Scalar(255, 0, 255), 5);
                                                Cv2.Line(matBlobDraw, new OpenCvSharp.Point(pos.X, pos.Y - 2), new OpenCvSharp.Point(pos.X, pos.Y + 2), new Scalar(255, 0, 255), 5);

                                                bFound = true;
                                            }
                                            nSumPixelPrev = nSumPixel;
                                        }
                                        else
                                            nSumPixelPrev = nSumPixel;
                                    }
                                    matCropV.Release();
                                }
                            }

                            //1-3   : Ransac 알고리즘을 돌려서, 직선방정식을 구한다.
                            if (bContourOk)
                            {
                                if (posSL.Count() > 0)
                                {
                                    nRefCnt = posSL.Count();
                                    OpenCvSharp.Point[] posS = new OpenCvSharp.Point[nRefCnt];
                                    posSL.CopyTo(posS);
                                    Test_Ransac_Vert(rcRoiV, posS, nRefCnt, ref refAV[i], ref refBV[i], ref matRansac);
                                }
                            }

                            string strBlob = "[F] matBlob " + i;
                            resultMat.Add(strBlob, matBlobDraw);

                            resRectHs.Add(strBlob, rcRoiH);
                            resRectVs.Add(strBlob, rcRoiV);

                            matBlob = Mat.Zeros(matCloseInv.Rows, matCloseInv.Cols, matCloseInv.Type());

                            //꼭지점을 구하자...
                            double _a = refAV[i];
                            double _b = refBV[i];
                            double _A = refAH[i];
                            double _B = refBH[i];

                            if (_A != 0 && _a != 0 && _B != 0 && _b != 0)
                            {
                                stObj.posVertex.X = (_B - _b) / (_a - _A);
                                stObj.posVertex.Y = _a * (_B - _b) / (_a - _A) + _b;

                                Cv2.Line(matRansac, new OpenCvSharp.Point(stObj.posVertex.X - 10, stObj.posVertex.Y), new OpenCvSharp.Point(stObj.posVertex.X + 10, stObj.posVertex.Y), new Scalar(0, 0, 255), 5);
                                Cv2.Line(matRansac, new OpenCvSharp.Point(stObj.posVertex.X, stObj.posVertex.Y - 10), new OpenCvSharp.Point(stObj.posVertex.X, stObj.posVertex.Y + 10), new Scalar(0, 0, 255), 5);
                            }

                            _outResult.Add(stObj);
                        }
                        else
                        {
                            List<OpenCvSharp.Point> posS;
                            OpenCvSharp.Point posVertex;
                            FindVertex(matBlobFind, matBlobDraw, stObj.eDiePos, out posS, out posVertex);

                            if (posS.Count() == 3)
                            {
                                string strBlob = "[F] matBlob " + i;
                                resultMat.Add(strBlob, matBlobDraw);

                                stObj.posVertex = posVertex;

                                if (matRansac.Type() == MatType.CV_8U)
                                    Cv2.CvtColor(matRansac, matRansac, ColorConversionCodes.GRAY2BGR);

                                Cv2.Line(matRansac, new OpenCvSharp.Point(stObj.posVertex.X - 10, stObj.posVertex.Y), new OpenCvSharp.Point(stObj.posVertex.X + 10, stObj.posVertex.Y), new Scalar(0, 0, 255), 5);
                                Cv2.Line(matRansac, new OpenCvSharp.Point(stObj.posVertex.X, stObj.posVertex.Y - 10), new OpenCvSharp.Point(stObj.posVertex.X, stObj.posVertex.Y + 10), new Scalar(0, 0, 255), 5);
                            }
                            else
                            {
                                Console.WriteLine("FindVertex Errror!!! ==> posS::Count=" + posS.Count + "  posVertex:" + posVertex);
                            }

                            matBlob = Mat.Zeros(matCloseInv.Rows, matCloseInv.Cols, matCloseInv.Type());
                        }
                    }
                }

                Mat matResult = matRansac.Clone();
                string strRansac = "[F] matResult";
                resultMat.Add(strRansac, matResult);
            }
            matLabelImg.Release();
            dst.Release();
            elementOpen.Release();
            elementClose.Release();
        }

        public static void OpenCVFindEdgeDeri(
            Mat matOrg,
            Mat matDbg,
            Parameters param,
            out Dictionary<string, Mat> debugMats,
            out List<stObject> _outResult)
        {
            debugMats = new Dictionary<string, Mat>();
            Mat blur = new Mat();
            //Mat sobelx = new Mat();
            //Mat sobely = new Mat();
            //Mat sobelxy = new Mat();
            //
            //Mat scharrx = new Mat();
            //Mat scharry = new Mat();
            //Mat scharrxy = new Mat();
            //
            //Mat laplacianx = new Mat();
            //Mat laplaciany = new Mat();
            //Mat laplacianxy = new Mat();

            Mat canny = new Mat();
            Mat cannyAddOrg = new Mat();

            OpenCvSharp.BorderTypes eBorderTypes = BorderTypes.Default;

            Cv2.GaussianBlur(matOrg, blur, new OpenCvSharp.Size(5, 5), 0, 0, BorderTypes.Default);
            //blur = matOrg.Clone();

            //Cv2.Sobel(blur, sobelx, MatType.CV_32F, 1, 0, param.SobelKSize, param.SobelScale, delta: 0, eBorderTypes);
            //sobelx.ConvertTo(sobelx, MatType.CV_8U);
            //debugMats.Add("sobelx", sobelx);
            //
            //Cv2.Sobel(blur, sobely, MatType.CV_32F, 0, 1, param.SobelKSize, param.SobelScale, delta: 0, eBorderTypes);
            //sobely.ConvertTo(sobely, MatType.CV_8U);
            //debugMats.Add("sobely", sobely);
            //
            //Cv2.Add(sobelx, sobely, sobelxy);
            //
            //debugMats.Add("sobelxy", sobelxy);
            //Mat sobelxyInv = new Mat();
            //Cv2.Subtract(255, sobelxy, sobelxyInv);
            //
            //{
            //    Cv2.Scharr(blur, scharrx, MatType.CV_32F, 1, 0, param.SobelScale, delta: 0, eBorderTypes);
            //    scharrx.ConvertTo(scharrx, MatType.CV_8U);
            //    debugMats.Add("scharrx", scharrx);
            //
            //    Cv2.Scharr(blur, scharry, MatType.CV_32F, 0, 1, param.SobelScale, delta: 0, eBorderTypes);
            //    sobely.ConvertTo(scharry, MatType.CV_8U);
            //    debugMats.Add("scharry", scharry);
            //
            //    Cv2.Add(scharrx, scharry, scharrxy);
            //
            //    debugMats.Add("scharrxy", scharrxy);
            //    Mat scharrxyInv = new Mat();
            //    Cv2.Subtract(255, scharrxy, scharrxyInv);
            //}
            //
            //{
            //    Cv2.Laplacian(blur, laplacianx, MatType.CV_32F, param.SobelKSize, param.SobelScale, delta: 0, eBorderTypes);
            //    scharrx.ConvertTo(laplacianx, MatType.CV_8U);
            //    debugMats.Add("laplacianx", laplacianx);
            //
            //    Cv2.Laplacian(blur, laplaciany, MatType.CV_32F, param.SobelKSize, param.SobelScale, delta: 0, eBorderTypes);
            //    sobely.ConvertTo(laplaciany, MatType.CV_8U);
            //    debugMats.Add("laplaciany", laplaciany);
            //
            //    Cv2.Add(laplacianx, laplaciany, laplacianxy);
            //
            //    debugMats.Add("laplacianxy", laplacianxy);
            //    Mat laplacianInv = new Mat();
            //    Cv2.Subtract(255, laplacianxy, laplacianInv);
            //}

            Cv2.Canny(blur, canny, 100, 200, 5, true);
            debugMats.Add("Canny", canny);
            Cv2.Add(blur, canny, cannyAddOrg);
            debugMats.Add("Canny+Origin", cannyAddOrg);

            OpenCVFindEdgeThres(cannyAddOrg, matDbg, param, out Dictionary<string, Mat> mats, out _outResult);

            foreach (var m in mats)
            {
                debugMats.Add(m.Key, m.Value);
            }

            blur.Release();
        }

        public static void OpenCVFindEdgeDeriEx(
            Mat matOrg,
            Mat matDbg,
            Parameters param,
            out Dictionary<string, Mat> debugMats,
            out List<stObject> _outResult)
        {
            debugMats = new Dictionary<string, Mat>();
            Mat blur = new Mat();
            //Mat sobelx = new Mat();
            //Mat sobely = new Mat();
            //Mat sobelxy = new Mat();
            //
            //Mat scharrx = new Mat();
            //Mat scharry = new Mat();
            //Mat scharrxy = new Mat();
            //
            //Mat laplacianx = new Mat();
            //Mat laplaciany = new Mat();
            //Mat laplacianxy = new Mat();

            Mat canny = new Mat();
            Mat cannyAddOrg = new Mat();

            OpenCvSharp.BorderTypes eBorderTypes = BorderTypes.Default;

            Cv2.GaussianBlur(matOrg, blur, new OpenCvSharp.Size(5, 5), 0, 0, BorderTypes.Default);
            //blur = matOrg.Clone();

            //Cv2.Sobel(blur, sobelx, MatType.CV_32F, 1, 0, param.SobelKSize, param.SobelScale, delta: 0, eBorderTypes);
            //sobelx.ConvertTo(sobelx, MatType.CV_8U);
            //debugMats.Add("sobelx", sobelx);
            //
            //Cv2.Sobel(blur, sobely, MatType.CV_32F, 0, 1, param.SobelKSize, param.SobelScale, delta: 0, eBorderTypes);
            //sobely.ConvertTo(sobely, MatType.CV_8U);
            //debugMats.Add("sobely", sobely);
            //
            //Cv2.Add(sobelx, sobely, sobelxy);
            //
            //debugMats.Add("sobelxy", sobelxy);
            //Mat sobelxyInv = new Mat();
            //Cv2.Subtract(255, sobelxy, sobelxyInv);
            //
            //{
            //    Cv2.Scharr(blur, scharrx, MatType.CV_32F, 1, 0, param.SobelScale, delta: 0, eBorderTypes);
            //    scharrx.ConvertTo(scharrx, MatType.CV_8U);
            //    debugMats.Add("scharrx", scharrx);
            //
            //    Cv2.Scharr(blur, scharry, MatType.CV_32F, 0, 1, param.SobelScale, delta: 0, eBorderTypes);
            //    sobely.ConvertTo(scharry, MatType.CV_8U);
            //    debugMats.Add("scharry", scharry);
            //
            //    Cv2.Add(scharrx, scharry, scharrxy);
            //
            //    debugMats.Add("scharrxy", scharrxy);
            //    Mat scharrxyInv = new Mat();
            //    Cv2.Subtract(255, scharrxy, scharrxyInv);
            //}
            //
            //{
            //    Cv2.Laplacian(blur, laplacianx, MatType.CV_32F, param.SobelKSize, param.SobelScale, delta: 0, eBorderTypes);
            //    scharrx.ConvertTo(laplacianx, MatType.CV_8U);
            //    debugMats.Add("laplacianx", laplacianx);
            //
            //    Cv2.Laplacian(blur, laplaciany, MatType.CV_32F, param.SobelKSize, param.SobelScale, delta: 0, eBorderTypes);
            //    sobely.ConvertTo(laplaciany, MatType.CV_8U);
            //    debugMats.Add("laplaciany", laplaciany);
            //
            //    Cv2.Add(laplacianx, laplaciany, laplacianxy);
            //
            //    debugMats.Add("laplacianxy", laplacianxy);
            //    Mat laplacianInv = new Mat();
            //    Cv2.Subtract(255, laplacianxy, laplacianInv);
            //}

            Cv2.Canny(blur, canny, 100, 200, 5, true);
            debugMats.Add("Canny", canny);
            Cv2.Add(blur, canny, cannyAddOrg);
            debugMats.Add("Canny+Origin", cannyAddOrg);

            OpenCVFindEdgeThresEx(cannyAddOrg, matDbg, param, out Dictionary<string, Mat> mats, out _outResult);

            foreach (var m in mats)
            {
                debugMats.Add(m.Key, m.Value);
            }

            blur.Release();
        }


        public static void Test_Ransac()
        {
            // 랜덤한 2D 점들을 생성합니다.
            Random random = new Random(0);
            int numPoints = 100;
            double[] x = Enumerable.Range(0, numPoints).Select(i => (double)i).ToArray();
            double[] y = x.Select(xi => 0.5 * xi + 20 + random.NextDouble() * 20 - 10).ToArray(); // 약간의 노이즈를 추가합니다.

            // 몇 개의 이상치를 추가합니다.
            int numOutliers = 10;
            double[] xOutliers = Enumerable.Range(0, numOutliers).Select(i => random.NextDouble() * 100).ToArray();
            double[] yOutliers = Enumerable.Range(0, numOutliers).Select(i => random.NextDouble() * 100).ToArray();

            x = x.Concat(xOutliers).ToArray();
            y = y.Concat(yOutliers).ToArray();

            // 점들을 OpenCV 형식으로 변환합니다.
            var points = new Point2f[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                points[i] = new Point2f((float)x[i], (float)y[i]);
            }

            // RANSAC을 사용하여 선형 피팅을 수행합니다.
            OpenCvSharp.Line2D line = Cv2.FitLine(points, DistanceTypes.L2, 0, 0.99, 0.01);
            double vx = line.Vx;
            double vy = line.Vy;
            double x0 = line.X1;
            double y0 = line.Y1;

            // x, y 좌표를 사용하여 직선의 시작과 끝 점을 계산합니다.
            double minX = x.Min(), maxX = x.Max();
            double minY = y0 + (minX - x0) * (vy / vx);
            double maxY = y0 + (maxX - x0) * (vy / vx);

            // 결과를 출력합니다.
            Console.WriteLine("Line equation: y = {0} * x + {1}", vy / vx, y0 - (vy / vx) * x0);

            // 결과를 시각화합니다.
            using (var window = new Window("RANSAC Line Fitting"))
            {
                Mat image = new Mat(new Size(400, 400), MatType.CV_8UC3, Scalar.White);
                for (int i = 0; i < x.Length; i++)
                {
                    Point2d point = new Point2d(x[i] * 4, 400 - y[i] * 4); // 시각화를 위해 크기 조정
                    Cv2.Circle(image, point.ToPoint(), 3, Scalar.Blue, -1);
                }

                for (int i = 0; i < numOutliers; i++)
                {
                    Point2d outlierPoint = new Point2d(xOutliers[i] * 4, 400 - yOutliers[i] * 4);
                    Cv2.Circle(image, outlierPoint.ToPoint(), 3, Scalar.Orange, -1);
                }

                Point2d pt1 = new Point2d(minX * 4, 400 - minY * 4);
                Point2d pt2 = new Point2d(maxX * 4, 400 - maxY * 4);
                Cv2.Line(image, pt1.ToPoint(), pt2.ToPoint(), Scalar.Red, 2);

                window.ShowImage(image);
                Cv2.WaitKey(0);
            }
        }

        public static void Test_Ransac_Hori(OpenCvSharp.Rect _refRectRoi, OpenCvSharp.Point[] _refEdgePos, int _refCount,
            ref double _resA, ref double _resB, ref Mat _matTestDraw)
        {
            int nRefCnt = 0;
            OpenCvSharp.Point[] posRefParsing;
            if (_refCount <= 0)
            {
                _resA = _resB = 0;
                return;
            }

            posRefParsing = new OpenCvSharp.Point[_refCount];

            if (_refEdgePos.Count() != _refCount)
            {
                Console.WriteLine(_refEdgePos.Count() + " " + _refCount);
            }

            for (int nCnt = 0; nCnt < _refEdgePos.Count(); nCnt++)
            {
                if (_refEdgePos[nCnt].X >= _refRectRoi.Left && _refEdgePos[nCnt].X <= _refRectRoi.Right &&
                    _refEdgePos[nCnt].Y >= _refRectRoi.Top && _refEdgePos[nCnt].Y <= _refRectRoi.Bottom)
                {
                    posRefParsing[nRefCnt] = _refEdgePos[nCnt];
                    nRefCnt++;
                }
            }

            // RANSAC을 사용하여 선형 피팅을 수행합니다.
            OpenCvSharp.Line2D line = Cv2.FitLine(posRefParsing, DistanceTypes.L2, 0, 0.99, 0.01);
            double vx = line.Vx;
            double vy = line.Vy;
            double x0 = line.X1;
            double y0 = line.Y1;

            _resA = vy / vx;
            _resB = y0 - (vy / vx) * x0;

            // 결과를 출력합니다.
            Console.WriteLine("Line equation: y = {0} * x + {1}", _resA, _resB);
            Console.WriteLine("Line equation: x = (y - {1}) / {0}", _resA, _resB);

            OpenCvSharp.Point pos1 = new Point((_refRectRoi.Top - _resB) / _resA, _refRectRoi.Top);
            OpenCvSharp.Point pos2 = new Point((_refRectRoi.Bottom - _resB) / _resA, _refRectRoi.Bottom);

            if (_matTestDraw.Type() == MatType.CV_8U)
                Cv2.CvtColor(_matTestDraw, _matTestDraw, ColorConversionCodes.GRAY2BGR);
            Cv2.Line(_matTestDraw, pos1, pos2, new Scalar(255, 0, 0), 3, LineTypes.Link8);
        }

        public static void Test_Ransac_Vert(OpenCvSharp.Rect _refRectRoi, OpenCvSharp.Point[] _refEdgePos, int _refCount,
            ref double _resA, ref double _resB, ref Mat _matTestDraw)
        {
            int nRefCnt = 0;
            OpenCvSharp.Point[] posRefParsing;
            if (_refCount <= 0)
            {
                _resA = _resB = 0;
                return;
            }

            posRefParsing = new OpenCvSharp.Point[_refCount];

            if (_refEdgePos.Count() != _refCount)
            {
                Console.WriteLine(_refEdgePos.Count() + " " + _refCount);
            }

            for (int nCnt = 0; nCnt < _refEdgePos.Count(); nCnt++)
            {
                if (_refEdgePos[nCnt].X > _refRectRoi.Left && _refEdgePos[nCnt].X < _refRectRoi.Right &&
                    _refEdgePos[nCnt].Y > _refRectRoi.Top && _refEdgePos[nCnt].Y < _refRectRoi.Bottom)
                {
                    posRefParsing[nRefCnt] = _refEdgePos[nCnt];
                    nRefCnt++;
                }
            }

            // RANSAC을 사용하여 선형 피팅을 수행합니다.
            OpenCvSharp.Line2D line = Cv2.FitLine(posRefParsing, DistanceTypes.L2, 0, 0.99, 0.01);
            double vx = line.Vx;
            double vy = line.Vy;
            double x0 = line.X1;
            double y0 = line.Y1;

            _resA = vy / vx;
            _resB = y0 - (vy / vx) * x0;

            // 결과를 출력합니다.
            Console.WriteLine("Line equation: y = {0} * x + {1}", _resA, _resB);
            Console.WriteLine("Line equation: x = (y - {1}) / {0}", _resA, _resB);

            OpenCvSharp.Point pos1 = new Point(_refRectRoi.Left, _resA * _refRectRoi.Left + _resB);
            OpenCvSharp.Point pos2 = new Point(_refRectRoi.Right, _resA * _refRectRoi.Right + _resB);

            if (_matTestDraw.Type() == MatType.CV_8U)
                Cv2.CvtColor(_matTestDraw, _matTestDraw, ColorConversionCodes.GRAY2BGR);
            Cv2.Line(_matTestDraw, pos1, pos2, new Scalar(255, 0, 0), 3, LineTypes.Link8);
        }

        public struct stRoiData
        {
            public enum eRoiType
            {
                eRoiType_Vert,
                eRoiType_Hori,
            }

            public eRoiType eRoiDir;
            public OpenCvSharp.Rect rcRoiPos;

            public void reset()
            {
                eRoiDir = eRoiType.eRoiType_Hori;
                rcRoiPos.X = rcRoiPos.Y = rcRoiPos.Width = rcRoiPos.Height = 0;
            }
        }

        public static void UpdateRoi(Mat _matOrg, Mat _matDraw, OpenCvSharp.Rect _rcOutLineRoi, eObjDir _eDiePos, out List<stRoiData> _roiDatas)
        {
            stRoiData stRoiV = new stRoiData();
            stRoiData stRoiH = new stRoiData();
            stRoiV.reset();
            stRoiH.reset();
            _roiDatas = new List<stRoiData>();

            int nDirH = 1;
            if (_eDiePos == eObjDir.eObjDir_LeftTop || _eDiePos == eObjDir.eObjDir_LeftBot)
                nDirH = 1;
            else
                nDirH = -3;

            int nDirV = 1;
            if (_eDiePos == eObjDir.eObjDir_LeftTop || _eDiePos == eObjDir.eObjDir_RightTop)
                nDirV = 1;
            else
                nDirV = -3;

            //if (nDirH > 0 && nDirV > 0)
            //    stObj.eDiePos = stObject.eObjDir.eObjDir_LeftTop;
            //else if (nDirH > 0 && nDirV < 0)
            //    stObj.eDiePos = stObject.eObjDir.eObjDir_LeftBot;
            //else if (nDirH < 0 && nDirV > 0)
            //    stObj.eDiePos = stObject.eObjDir.eObjDir_RightTop;
            //else if (nDirH < 0 && nDirV < 0)
            //    stObj.eDiePos = stObject.eObjDir.eObjDir_RightBot;

            OpenCvSharp.Point posCenter = new OpenCvSharp.Point();
            posCenter.X = _rcOutLineRoi.Left + _rcOutLineRoi.Width / 2;
            posCenter.Y = _rcOutLineRoi.Top + _rcOutLineRoi.Height / 2;

            int nMarginX = _rcOutLineRoi.Width / 4;
            int nMarginY = _rcOutLineRoi.Height / 4;

            stRoiH.rcRoiPos.Left = posCenter.X + nMarginX * nDirH;
            stRoiH.rcRoiPos.Width = nMarginX * 2;
            stRoiH.rcRoiPos.Top = posCenter.Y - nMarginY;
            stRoiH.rcRoiPos.Height = nMarginY * 2;
            if (stRoiH.rcRoiPos.Left <= 0)
            {
                stRoiH.rcRoiPos.Width += (stRoiH.rcRoiPos.Left - 10);
                stRoiH.rcRoiPos.Left = 10;
            }
            if (stRoiH.rcRoiPos.Left + stRoiH.rcRoiPos.Width >= _matOrg.Cols)
            {
                stRoiH.rcRoiPos.Width -= (stRoiH.rcRoiPos.Left + stRoiH.rcRoiPos.Width - _matOrg.Cols + 10);
            }
            Cv2.Rectangle(_matDraw, stRoiH.rcRoiPos, new Scalar(0, 128, 255), 2);

            stRoiV.rcRoiPos.Left = posCenter.X - nMarginX;
            stRoiV.rcRoiPos.Width = nMarginX * 2;
            stRoiV.rcRoiPos.Top = posCenter.Y + nMarginY * nDirV;
            stRoiV.rcRoiPos.Height = nMarginY * 2;
            if (stRoiV.rcRoiPos.Top <= 0)
            {
                stRoiV.rcRoiPos.Height -= (stRoiV.rcRoiPos.Top + 10);
                stRoiV.rcRoiPos.Top = 10;
            }
            if (stRoiV.rcRoiPos.Top + stRoiV.rcRoiPos.Height >= _matOrg.Rows)
            {
                stRoiV.rcRoiPos.Height -= (stRoiV.rcRoiPos.Top + stRoiV.rcRoiPos.Height - _matOrg.Rows + 10);
            }
            Cv2.Rectangle(_matDraw, stRoiV.rcRoiPos, new Scalar(0, 128, 255), 2);

            Console.WriteLine("RoiH Left={0} Top={1} Width={2} Height={3}", stRoiH.rcRoiPos.Left, stRoiH.rcRoiPos.Width, stRoiH.rcRoiPos.Top, stRoiH.rcRoiPos.Height);
            Console.WriteLine("RoiV Left={0} Top={1} Width={2} Height={3}", stRoiV.rcRoiPos.Left, stRoiV.rcRoiPos.Width, stRoiV.rcRoiPos.Top, stRoiV.rcRoiPos.Height);

            _roiDatas.Add(stRoiV);
            _roiDatas.Add(stRoiH);
        }

        public static void UpdateRoiEx(Mat _matOrg, Mat _matDraw, OpenCvSharp.Rect _rcOutLineRoi, eObjDir _eDiePos, out List<stRoiData> _roiDatas)
        {
            stRoiData stRoiV = new stRoiData();
            stRoiData stRoiH = new stRoiData();
            stRoiV.reset();
            stRoiH.reset();
            _roiDatas = new List<stRoiData>();

            //
            //Contours 값을 확인한다.
            //
            OpenCvSharp.Point[][] contours;
            HierarchyIndex[] hierarchy;

            Cv2.FindContours(_matOrg, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            OpenCvSharp.Point posCentriod;
            List<OpenCvSharp.Point> posSL = new List<OpenCvSharp.Point>();
            posSL.Clear();

            foreach (OpenCvSharp.Point[] p in contours)
            {
                double length = Cv2.ArcLength(p, true);

                Moments moments = Cv2.Moments(p, false);
                posCentriod.X = (int)(moments.M10 / moments.M00);
                posCentriod.Y = (int)(moments.M01 / moments.M00);

                if (length > 100)
                {

                    Cv2.Line(_matDraw, new OpenCvSharp.Point(posCentriod.X - 10, posCentriod.Y), new OpenCvSharp.Point(posCentriod.X + 10, posCentriod.Y), new Scalar(0, 255, 0), 2);
                    Cv2.Line(_matDraw, new OpenCvSharp.Point(posCentriod.X, posCentriod.Y - 10), new OpenCvSharp.Point(posCentriod.X, posCentriod.Y + 10), new Scalar(0, 255, 0), 2);
                    Cv2.Circle(_matDraw, posCentriod, 5, new Scalar(0, 255, 0), 2);

                    int nValueMaxX = -1;
                    int nValueMaxY = -1;
                    double dValueMaxA = -1.0;
                    OpenCvSharp.Point pos1 = new OpenCvSharp.Point();
                    OpenCvSharp.Point pos2 = new OpenCvSharp.Point();
                    OpenCvSharp.Point pos3 = new OpenCvSharp.Point();

                    //Blob의 위치 정보를 업데이트 한다.
                    if (posCentriod.X <= _matOrg.Cols / 2 && posCentriod.Y <= _matOrg.Rows / 2)
                        _eDiePos = eObjDir.eObjDir_LeftTop;
                    else if (posCentriod.X <= _matOrg.Cols / 2 && posCentriod.Y > _matOrg.Rows / 2)
                        _eDiePos = eObjDir.eObjDir_LeftBot;
                    else if (posCentriod.X > _matOrg.Cols / 2 && posCentriod.Y <= _matOrg.Rows / 2)
                        _eDiePos = eObjDir.eObjDir_RightTop;
                    else if (posCentriod.X > _matOrg.Cols / 2 && posCentriod.Y > _matOrg.Rows / 2)
                        _eDiePos = eObjDir.eObjDir_RightBot;

                    double dValueMax = -1;

                    pos1.X = pos1.Y = -1;
                    pos2.X = pos2.Y = -1;
                    pos3.X = pos3.Y = -1;
                    nValueMaxX = -1;
                    nValueMaxY = -1;
                    dValueMaxA = -1.0;

                    //Y좌표가 0에서, X값이 최대인 경우 -> pos1
                    //X좌표가 cols에서, Y값이 최대인 경우 -> pos2
                    //contours 중, pos1<->pos + pos2<->pos 합 거리가 제일 긴 경우, pos3

                    for (int i = 0; i < p.Count(); i++)
                    {
                        if (_eDiePos == eObjDir.eObjDir_LeftTop)
                        {
                            if ((nValueMaxY == -1 || nValueMaxY < p[i].Y) && (p[i].X == 0))
                            {
                                nValueMaxY = p[i].Y;
                                pos1 = p[i];
                            }
                        }
                        else if (_eDiePos == eObjDir.eObjDir_LeftBot)
                        {
                            if ((nValueMaxY == -1 || nValueMaxY > p[i].Y) && (p[i].X == 0))
                            {
                                nValueMaxY = p[i].Y;
                                pos1 = p[i];
                            }
                        }
                        else if (_eDiePos == eObjDir.eObjDir_RightTop)
                        {
                            if ((nValueMaxY == -1 || nValueMaxY < p[i].Y) && (p[i].X == _matOrg.Cols - 1))
                            {
                                nValueMaxY = p[i].Y;
                                pos1 = p[i];
                            }
                        }
                        else if (_eDiePos == eObjDir.eObjDir_RightBot)
                        {
                            if ((nValueMaxY == -1 || nValueMaxY > p[i].Y) && (p[i].X == _matOrg.Cols - 1))
                            {
                                nValueMaxX = p[i].Y;
                                pos1 = p[i];
                            }
                        }
                    }

                    if ((pos1.X == 0 && pos1.Y == _matOrg.Rows - 1) || (pos1.X == _matOrg.Cols - 1 && pos1.Y == _matOrg.Rows - 1) ||
                        (pos1.X == _matOrg.Cols - 1 && pos1.Y == 0) || (pos1.X == 0 && pos1.Y == 0))
                    {
                        nValueMaxX = -1;
                        for (int i = 0; i < p.Count(); i++)
                        {
                            if (_eDiePos == eObjDir.eObjDir_LeftTop)
                            {
                                if ((nValueMaxX == -1 || nValueMaxX < p[i].X) && (p[i].Y == _matOrg.Rows - 1))
                                {
                                    nValueMaxX = p[i].X;
                                    pos1 = p[i];
                                }
                            }
                            else if (_eDiePos == eObjDir.eObjDir_LeftBot)
                            {
                                if ((nValueMaxX == -1 || nValueMaxX < p[i].X) && (p[i].Y == _matOrg.Rows - 1))
                                {
                                    nValueMaxX = p[i].X;
                                    pos1 = p[i];
                                }
                            }
                            else if (_eDiePos == eObjDir.eObjDir_RightTop)
                            {
                                if ((nValueMaxX == -1 || nValueMaxX > p[i].X) && (p[i].Y == _matOrg.Rows - 1))
                                {
                                    nValueMaxX = p[i].X;
                                    pos1 = p[i];
                                }
                            }
                            else if (_eDiePos == eObjDir.eObjDir_RightBot)
                            {
                                if ((nValueMaxX == -1 || nValueMaxX > p[i].X) && (p[i].Y == _matOrg.Rows - 1))
                                {
                                    nValueMaxX = p[i].X;
                                    pos1 = p[i];
                                }
                            }
                        }
                    }

                    dValueMax = -1;
                    if ((pos1.X > 0 && pos1.Y >= 0) || (pos1.X >= 0 && pos1.Y > 0))
                    {
                        for (int i = 0; i < p.Count(); i++)
                        {
                            if (!(p[i].X == 0 && p[i].Y == 0) && !(p[i].X == _matOrg.Cols - 1 && p[i].Y == 0) && !(p[i].X == 0 && p[i].Y == _matOrg.Rows - 1) && !(p[i].X == _matOrg.Cols - 1 && p[i].Y == _matOrg.Rows - 1))
                            {
                                double dDist = Math.Sqrt((p[i].X - pos1.X) * (p[i].X - pos1.X) + (p[i].Y - pos1.Y) * (p[i].Y - pos1.Y));
                                if ((dValueMax == -1 || dValueMax < dDist))
                                {
                                    dValueMax = dDist;
                                    pos2 = p[i];
                                }
                            }
                        }
                    }

                    dValueMax = -1;
                    for (int i = 0; i < p.Count(); i++)
                    {
                        if (p[i].X != 0 && p[i].Y != 0 && p[i].X != _matOrg.Cols - 1 && p[i].Y != _matOrg.Rows - 1)
                        {
                            double dDistA = Math.Sqrt((p[i].X - pos1.X) * (p[i].X - pos1.X) + (p[i].Y - pos1.Y) * (p[i].Y - pos1.Y));
                            double dDistB = Math.Sqrt((p[i].X - pos2.X) * (p[i].X - pos2.X) + (p[i].Y - pos2.Y) * (p[i].Y - pos2.Y));
                            if (((dValueMax == -1.0) || (dValueMax < dDistA + dDistB)))
                            {
                                dValueMax = dDistA + dDistB;
                                pos3 = p[i];
                            }
                        }
                    }

                    if (_eDiePos == eObjDir.eObjDir_LeftTop)
                    {
                        if (((pos1.Y > 0 && pos1.Y < _matOrg.Rows - 1) && (pos1.X == 0 || pos1.X == _matOrg.Rows - 1)) &&
                            ((pos2.Y > 0 && pos2.Y < _matOrg.Rows - 1) && (pos2.X == 0 || pos2.X == _matOrg.Rows - 1)))
                        {
                            stRoiH.eRoiDir = stRoiData.eRoiType.eRoiType_Vert;
                            stRoiH.rcRoiPos.Width = pos1.X > pos2.X ? Math.Abs(pos1.X - pos3.X) * 1 / 3 : Math.Abs(pos2.X - pos3.X) * 1 / 3;
                            stRoiH.rcRoiPos.Left = pos1.X > pos2.X ? pos3.X + Math.Abs(pos1.X - pos3.X) * 1 / 15 : pos3.X + Math.Abs(pos2.X - pos3.X) * 1 / 15;
                            stRoiH.rcRoiPos.Height = pos1.X > pos2.X ? Math.Abs(pos3.Y - posCentriod.Y) : Math.Abs(pos3.Y - posCentriod.Y);
                            stRoiH.rcRoiPos.Top = posCentriod.Y;

                            stRoiV.eRoiDir = stRoiData.eRoiType.eRoiType_Vert;
                            stRoiV.rcRoiPos.Width = pos1.X > pos2.X ? Math.Abs(pos2.X - pos3.X) * 1 / 3 : Math.Abs(pos1.X - pos3.X) * 1 / 3;
                            stRoiV.rcRoiPos.Left = pos1.X > pos2.X ? pos3.X - Math.Abs(pos2.X - pos3.X) * 1 / 15 : pos3.X - Math.Abs(pos1.X - pos3.X) * 1 / 15;
                            stRoiV.rcRoiPos.Left = stRoiV.rcRoiPos.Left - stRoiV.rcRoiPos.Width;
                            stRoiV.rcRoiPos.Height = pos1.X > pos2.X ? Math.Abs(pos3.Y - posCentriod.Y) : Math.Abs(pos3.Y - posCentriod.Y);
                            stRoiV.rcRoiPos.Top = posCentriod.Y;
                        }
                        else if (!(pos1.Y == 0 && pos2.Y == _matOrg.Rows - 1) && !(pos2.Y == 0 && pos1.Y == _matOrg.Rows - 1))
                        {
                            stRoiH.eRoiDir = stRoiData.eRoiType.eRoiType_Hori;
                            stRoiH.rcRoiPos.Left = posCentriod.X;
                            stRoiH.rcRoiPos.Width = pos3.X > pos1.X ? Math.Abs(pos3.X - posCentriod.X) * 4 / 3 : Math.Abs(pos1.X - posCentriod.X) * 4 / 3;
                            stRoiH.rcRoiPos.Top = posCentriod.Y;
                            stRoiH.rcRoiPos.Height = pos3.Y > pos2.Y ? Math.Abs(pos2.Y - posCentriod.Y) * 2 / 3 : Math.Abs(pos3.Y - posCentriod.Y) * 2 / 3;

                            stRoiV.eRoiDir = stRoiData.eRoiType.eRoiType_Vert;
                            stRoiV.rcRoiPos.Left = posCentriod.X;
                            stRoiV.rcRoiPos.Width = pos3.X > pos1.X ? Math.Abs(pos3.X - posCentriod.X) * 2 / 3 : Math.Abs(pos2.X - posCentriod.X) * 2 / 3;
                            stRoiV.rcRoiPos.Top = posCentriod.Y;
                            stRoiV.rcRoiPos.Height = pos3.Y > pos2.Y ? Math.Abs(posCentriod.Y - pos2.Y) * 4 / 3 : Math.Abs(posCentriod.Y - pos3.Y) * 4 / 3;
                        }
                        else
                        {
                            stRoiH.eRoiDir = stRoiData.eRoiType.eRoiType_Hori;
                            stRoiH.rcRoiPos.Left = pos1.X;
                            stRoiH.rcRoiPos.Width = pos3.X - pos1.X;
                            stRoiH.rcRoiPos.Top = pos3.Y - Math.Abs(pos3.Y - pos1.Y) * 2 / 5;
                            stRoiH.rcRoiPos.Height = Math.Abs(pos3.Y - pos1.Y) * 1 / 3;

                            stRoiV.eRoiDir = stRoiData.eRoiType.eRoiType_Hori;
                            stRoiV.rcRoiPos.Left = pos2.X;
                            stRoiV.rcRoiPos.Width = pos3.X - pos2.X;
                            stRoiV.rcRoiPos.Top = pos3.Y + Math.Abs(pos3.Y - pos2.Y) * 1 / 15;
                            stRoiV.rcRoiPos.Height = Math.Abs(pos3.Y - pos2.Y) * 1 / 3;
                        }
                    }
                    else if (_eDiePos == eObjDir.eObjDir_LeftBot)
                    {
                        if (((pos1.Y > 0 && pos1.Y < _matOrg.Rows - 1) && (pos1.X == 0 || pos1.X == _matOrg.Rows - 1)) &&
                            ((pos2.Y > 0 && pos2.Y < _matOrg.Rows - 1) && (pos2.X == 0 || pos2.X == _matOrg.Rows - 1)))
                        {
                            stRoiH.eRoiDir = stRoiData.eRoiType.eRoiType_Vert;
                            stRoiH.rcRoiPos.Width = pos1.X > pos2.X ? Math.Abs(pos1.X - pos3.X) * 1 / 3 : Math.Abs(pos2.X - pos3.X) * 1 / 3;
                            stRoiH.rcRoiPos.Left = pos1.X > pos2.X ? pos3.X + Math.Abs(pos1.X - pos3.X) * 1 / 15 : pos3.X + Math.Abs(pos2.X - pos3.X) * 1 / 15;
                            stRoiH.rcRoiPos.Height = pos1.X > pos2.X ? Math.Abs(pos3.Y - posCentriod.Y) : Math.Abs(pos3.Y - posCentriod.Y);
                            stRoiH.rcRoiPos.Top = posCentriod.Y - stRoiH.rcRoiPos.Height;

                            stRoiV.eRoiDir = stRoiData.eRoiType.eRoiType_Vert;
                            stRoiV.rcRoiPos.Width = pos1.X > pos2.X ? Math.Abs(pos2.X - pos3.X) * 1 / 3 : Math.Abs(pos1.X - pos3.X) * 1 / 3;
                            stRoiV.rcRoiPos.Left = pos1.X > pos2.X ? pos3.X - Math.Abs(pos2.X - pos3.X) * 1 / 15 : pos3.X - Math.Abs(pos1.X - pos3.X) * 1 / 15;
                            stRoiV.rcRoiPos.Left = stRoiV.rcRoiPos.Left - stRoiV.rcRoiPos.Width;
                            stRoiV.rcRoiPos.Height = pos1.X > pos2.X ? Math.Abs(pos3.Y - posCentriod.Y) : Math.Abs(pos3.Y - posCentriod.Y);
                            stRoiV.rcRoiPos.Top = posCentriod.Y - stRoiH.rcRoiPos.Height;
                        }
                        else if (!(pos1.Y == 0 && pos2.Y == _matOrg.Rows - 1) && !(pos2.Y == 0 && pos1.Y == _matOrg.Rows - 1))
                        {
                            stRoiH.eRoiDir = stRoiData.eRoiType.eRoiType_Hori;
                            stRoiH.rcRoiPos.Left = posCentriod.X;
                            stRoiH.rcRoiPos.Width = pos3.X > pos1.X ? Math.Abs(pos3.X - posCentriod.X) * 4 / 3 : Math.Abs(pos1.X - posCentriod.X) * 4 / 3;
                            stRoiH.rcRoiPos.Height = pos3.Y > pos2.Y ? Math.Abs(pos2.Y - posCentriod.Y) * 2 / 3 : Math.Abs(pos3.Y - posCentriod.Y) * 2 / 3;
                            stRoiH.rcRoiPos.Top = posCentriod.Y - stRoiH.rcRoiPos.Height;

                            stRoiV.eRoiDir = stRoiData.eRoiType.eRoiType_Vert;
                            stRoiV.rcRoiPos.Left = posCentriod.X;
                            stRoiV.rcRoiPos.Width = pos3.X > pos1.X ? Math.Abs(pos3.X - posCentriod.X) * 2 / 3 : Math.Abs(pos2.X - posCentriod.X) * 2 / 3;
                            stRoiV.rcRoiPos.Height = pos3.Y > pos2.Y ? Math.Abs(posCentriod.Y - pos2.Y) * 4 / 3 : Math.Abs(posCentriod.Y - pos3.Y) * 4 / 3;
                            stRoiV.rcRoiPos.Top = posCentriod.Y - stRoiV.rcRoiPos.Height;
                        }
                        else
                        {
                            stRoiH.eRoiDir = stRoiData.eRoiType.eRoiType_Hori;
                            stRoiH.rcRoiPos.Left = pos1.X;
                            stRoiH.rcRoiPos.Width = pos3.X - pos1.X;
                            stRoiH.rcRoiPos.Top = pos3.Y - Math.Abs(pos3.Y - pos1.Y) * 2 / 5;
                            stRoiH.rcRoiPos.Height = Math.Abs(pos3.Y - pos1.Y) * 1 / 3;

                            stRoiV.eRoiDir = stRoiData.eRoiType.eRoiType_Hori;
                            stRoiV.rcRoiPos.Left = pos2.X;
                            stRoiV.rcRoiPos.Width = pos3.X - pos2.X;
                            stRoiV.rcRoiPos.Top = pos3.Y + Math.Abs(pos3.Y - pos2.Y) * 1 / 15;
                            stRoiV.rcRoiPos.Height = Math.Abs(pos3.Y - pos2.Y) * 1 / 3;
                        }
                    }
                    else if (_eDiePos == eObjDir.eObjDir_RightTop)
                    {
                        if (((pos1.Y > 0 && pos1.Y < _matOrg.Rows - 1) && (pos1.X == 0 || pos1.X == _matOrg.Rows - 1)) &&
                            ((pos2.Y > 0 && pos2.Y < _matOrg.Rows - 1) && (pos2.X == 0 || pos2.X == _matOrg.Rows - 1)))
                        {
                            stRoiH.eRoiDir = stRoiData.eRoiType.eRoiType_Vert;
                            stRoiH.rcRoiPos.Width = pos1.X > pos2.X ? Math.Abs(pos1.X - pos3.X) * 1 / 3 : Math.Abs(pos2.X - pos3.X) * 1 / 3;
                            stRoiH.rcRoiPos.Left = pos1.X > pos2.X ? pos3.X + Math.Abs(pos1.X - pos3.X) * 1 / 15 : pos3.X + Math.Abs(pos2.X - pos3.X) * 1 / 15;
                            stRoiH.rcRoiPos.Height = pos1.X > pos2.X ? Math.Abs(pos3.Y - posCentriod.Y) : Math.Abs(pos3.Y - posCentriod.Y);
                            stRoiH.rcRoiPos.Top = posCentriod.Y;

                            stRoiV.eRoiDir = stRoiData.eRoiType.eRoiType_Vert;
                            stRoiV.rcRoiPos.Width = pos1.X > pos2.X ? Math.Abs(pos2.X - pos3.X) * 1 / 3 : Math.Abs(pos1.X - pos3.X) * 1 / 3;
                            stRoiV.rcRoiPos.Left = pos1.X > pos2.X ? pos3.X - Math.Abs(pos2.X - pos3.X) * 1 / 15 : pos3.X - Math.Abs(pos1.X - pos3.X) * 1 / 15;
                            stRoiV.rcRoiPos.Left = stRoiV.rcRoiPos.Left - stRoiV.rcRoiPos.Width;
                            stRoiV.rcRoiPos.Height = pos1.X > pos2.X ? Math.Abs(pos3.Y - posCentriod.Y) : Math.Abs(pos3.Y - posCentriod.Y);
                            stRoiV.rcRoiPos.Top = posCentriod.Y;
                        }
                        else if (!(pos1.Y == 0 && pos2.Y == _matOrg.Rows - 1) && !(pos2.Y == 0 && pos1.Y == _matOrg.Rows - 1))
                        {
                            stRoiH.eRoiDir = stRoiData.eRoiType.eRoiType_Hori;
                            stRoiH.rcRoiPos.Width = pos3.X > pos1.X ? Math.Abs(pos3.X - posCentriod.X) * 4 / 3 : Math.Abs(pos1.X - posCentriod.X) * 4 / 3;
                            stRoiH.rcRoiPos.Left = posCentriod.X - stRoiH.rcRoiPos.Width;
                            stRoiH.rcRoiPos.Top = posCentriod.Y;
                            stRoiH.rcRoiPos.Height = pos3.Y > pos2.Y ? Math.Abs(pos2.Y - posCentriod.Y) * 2 / 3 : Math.Abs(pos3.Y - posCentriod.Y) * 2 / 3;

                            stRoiV.eRoiDir = stRoiData.eRoiType.eRoiType_Vert;
                            stRoiV.rcRoiPos.Width = pos3.X > pos1.X ? Math.Abs(pos3.X - posCentriod.X) * 2 / 3 : Math.Abs(pos2.X - posCentriod.X) * 2 / 3;
                            stRoiV.rcRoiPos.Left = posCentriod.X - stRoiV.rcRoiPos.Width;
                            stRoiV.rcRoiPos.Top = posCentriod.Y;
                            stRoiV.rcRoiPos.Height = pos3.Y > pos2.Y ? Math.Abs(posCentriod.Y - pos2.Y) * 4 / 3 : Math.Abs(posCentriod.Y - pos3.Y) * 4 / 3;
                        }
                        else
                        {
                            stRoiH.eRoiDir = stRoiData.eRoiType.eRoiType_Hori;
                            stRoiH.rcRoiPos.Width = pos3.X - pos1.X;
                            stRoiH.rcRoiPos.Left = pos1.X - stRoiH.rcRoiPos.Width;
                            stRoiH.rcRoiPos.Top = pos3.Y - Math.Abs(pos3.Y - pos1.Y) * 2 / 5;
                            stRoiH.rcRoiPos.Height = Math.Abs(pos3.Y - pos1.Y) * 1 / 3;

                            stRoiV.eRoiDir = stRoiData.eRoiType.eRoiType_Hori;
                            stRoiV.rcRoiPos.Width = pos3.X - pos2.X;
                            stRoiV.rcRoiPos.Left = pos2.X - stRoiV.rcRoiPos.Width;
                            stRoiV.rcRoiPos.Top = pos3.Y + Math.Abs(pos3.Y - pos2.Y) * 1 / 15;
                            stRoiV.rcRoiPos.Height = Math.Abs(pos3.Y - pos2.Y) * 1 / 3;
                        }
                    }
                    else if (_eDiePos == eObjDir.eObjDir_RightBot)
                    {
                        if (((pos1.Y > 0 && pos1.Y < _matOrg.Rows - 1) && (pos1.X == 0 || pos1.X == _matOrg.Rows - 1)) &&
                            ((pos2.Y > 0 && pos2.Y < _matOrg.Rows - 1) && (pos2.X == 0 || pos2.X == _matOrg.Rows - 1)))
                        {
                            stRoiH.eRoiDir = stRoiData.eRoiType.eRoiType_Vert;
                            stRoiH.rcRoiPos.Width = pos1.X > pos2.X ? Math.Abs(pos1.X - pos3.X) * 1 / 3 : Math.Abs(pos2.X - pos3.X) * 1 / 3;
                            stRoiH.rcRoiPos.Left = pos1.X > pos2.X ? pos3.X + Math.Abs(pos1.X - pos3.X) * 1 / 15 : pos3.X + Math.Abs(pos2.X - pos3.X) * 1 / 15;
                            stRoiH.rcRoiPos.Height = pos1.X > pos2.X ? Math.Abs(pos3.Y - posCentriod.Y) : Math.Abs(pos3.Y - posCentriod.Y);
                            stRoiH.rcRoiPos.Top = posCentriod.Y - stRoiH.rcRoiPos.Height;

                            stRoiV.eRoiDir = stRoiData.eRoiType.eRoiType_Vert;
                            stRoiV.rcRoiPos.Width = pos1.X > pos2.X ? Math.Abs(pos2.X - pos3.X) * 1 / 3 : Math.Abs(pos1.X - pos3.X) * 1 / 3;
                            stRoiV.rcRoiPos.Left = pos1.X > pos2.X ? pos3.X - Math.Abs(pos2.X - pos3.X) * 1 / 15 : pos3.X - Math.Abs(pos1.X - pos3.X) * 1 / 15;
                            stRoiV.rcRoiPos.Left = stRoiV.rcRoiPos.Left - stRoiV.rcRoiPos.Width;
                            stRoiV.rcRoiPos.Height = pos1.X > pos2.X ? Math.Abs(pos3.Y - posCentriod.Y) : Math.Abs(pos3.Y - posCentriod.Y);
                            stRoiV.rcRoiPos.Top = posCentriod.Y - stRoiH.rcRoiPos.Height;
                        }
                        else if (!(pos1.Y == 0 && pos2.Y == _matOrg.Rows - 1) && !(pos2.Y == 0 && pos1.Y == _matOrg.Rows - 1))
                        {
                            stRoiH.eRoiDir = stRoiData.eRoiType.eRoiType_Hori;
                            stRoiH.rcRoiPos.Width = pos3.X > pos1.X ? Math.Abs(pos3.X - posCentriod.X) * 4 / 3 : Math.Abs(pos1.X - posCentriod.X) * 4 / 3;
                            stRoiH.rcRoiPos.Left = posCentriod.X - stRoiH.rcRoiPos.Width;
                            stRoiH.rcRoiPos.Height = pos3.Y > pos2.Y ? Math.Abs(pos2.Y - posCentriod.Y) * 2 / 3 : Math.Abs(pos3.Y - posCentriod.Y) * 2 / 3;
                            stRoiH.rcRoiPos.Top = posCentriod.Y - stRoiH.rcRoiPos.Height;

                            stRoiV.eRoiDir = stRoiData.eRoiType.eRoiType_Vert;
                            stRoiV.rcRoiPos.Width = pos3.X > pos1.X ? Math.Abs(pos3.X - posCentriod.X) * 2 / 3 : Math.Abs(pos2.X - posCentriod.X) * 2 / 3;
                            stRoiV.rcRoiPos.Left = posCentriod.X - stRoiV.rcRoiPos.Width;
                            stRoiV.rcRoiPos.Height = pos3.Y > pos2.Y ? Math.Abs(posCentriod.Y - pos2.Y) * 4 / 3 : Math.Abs(posCentriod.Y - pos3.Y) * 4 / 3;
                            stRoiV.rcRoiPos.Top = posCentriod.Y - stRoiV.rcRoiPos.Height;
                        }
                        else
                        {
                            stRoiH.eRoiDir = stRoiData.eRoiType.eRoiType_Hori;
                            stRoiH.rcRoiPos.Width = pos2.X - pos3.X;
                            stRoiH.rcRoiPos.Left = pos3.X;
                            stRoiH.rcRoiPos.Top = pos3.Y - Math.Abs(pos3.Y - pos1.Y) * 2 / 5;
                            stRoiH.rcRoiPos.Height = Math.Abs(pos3.Y - pos1.Y) * 1 / 3;

                            stRoiV.eRoiDir = stRoiData.eRoiType.eRoiType_Hori;
                            stRoiV.rcRoiPos.Width = pos1.X - pos3.X;
                            stRoiV.rcRoiPos.Left = pos3.X;
                            stRoiV.rcRoiPos.Top = pos3.Y + Math.Abs(pos3.Y - pos2.Y) * 1 / 15;
                            stRoiV.rcRoiPos.Height = Math.Abs(pos3.Y - pos2.Y) * 1 / 3;
                        }
                    }
                    else
                    {
                        return;
                    }

                    Cv2.Rectangle(_matDraw, stRoiH.rcRoiPos, new Scalar(0, 128, 255), 2);
                    Cv2.Rectangle(_matDraw, stRoiV.rcRoiPos, new Scalar(0, 128, 255), 2);

                    Console.WriteLine("RoiH Left={0} Top={1} Width={2} Height={3}", stRoiH.rcRoiPos.Left, stRoiH.rcRoiPos.Width, stRoiH.rcRoiPos.Top, stRoiH.rcRoiPos.Height);
                    Console.WriteLine("RoiV Left={0} Top={1} Width={2} Height={3}", stRoiV.rcRoiPos.Left, stRoiV.rcRoiPos.Width, stRoiV.rcRoiPos.Top, stRoiV.rcRoiPos.Height);

                    OpenCvSharp.Point posDraw = pos1;
                    Cv2.Line(_matDraw, new OpenCvSharp.Point(posDraw.X - 5, posDraw.Y), new OpenCvSharp.Point(posDraw.X + 5, posDraw.Y), new Scalar(0, 255, 0), 2);
                    Cv2.Line(_matDraw, new OpenCvSharp.Point(posDraw.X, posDraw.Y - 5), new OpenCvSharp.Point(posDraw.X, posDraw.Y + 5), new Scalar(0, 255, 0), 2);
                    Cv2.Circle(_matDraw, posDraw, 2, new Scalar(0, 255, 0), 2);

                    posDraw = pos2;
                    Cv2.Line(_matDraw, new OpenCvSharp.Point(posDraw.X - 5, posDraw.Y), new OpenCvSharp.Point(posDraw.X + 5, posDraw.Y), new Scalar(0, 255, 0), 2);
                    Cv2.Line(_matDraw, new OpenCvSharp.Point(posDraw.X, posDraw.Y - 5), new OpenCvSharp.Point(posDraw.X, posDraw.Y + 5), new Scalar(0, 255, 0), 2);
                    Cv2.Circle(_matDraw, posDraw, 2, new Scalar(0, 255, 0), 2);

                    posDraw = pos3;
                    Cv2.Line(_matDraw, new OpenCvSharp.Point(posDraw.X - 5, posDraw.Y), new OpenCvSharp.Point(posDraw.X + 5, posDraw.Y), new Scalar(0, 255, 0), 2);
                    Cv2.Line(_matDraw, new OpenCvSharp.Point(posDraw.X, posDraw.Y - 5), new OpenCvSharp.Point(posDraw.X, posDraw.Y + 5), new Scalar(0, 255, 0), 2);
                    Cv2.Circle(_matDraw, posDraw, 2, new Scalar(0, 255, 0), 2);

                    _roiDatas.Add(stRoiV);
                    _roiDatas.Add(stRoiH);
                }
                else
                {
                    Cv2.Line(_matDraw, new OpenCvSharp.Point(posCentriod.X - 10, posCentriod.Y), new OpenCvSharp.Point(posCentriod.X + 10, posCentriod.Y), new Scalar(0, 255, 125), 2);
                    Cv2.Line(_matDraw, new OpenCvSharp.Point(posCentriod.X, posCentriod.Y - 10), new OpenCvSharp.Point(posCentriod.X, posCentriod.Y + 10), new Scalar(0, 255, 125), 2);
                    Cv2.Circle(_matDraw, posCentriod, 5, new Scalar(0, 255, 125), 2);
                }
            }
        }

        public static void FindVertex(Mat _matOrg, Mat _matDraw, eObjDir _eDiePos, out List<OpenCvSharp.Point> _PosS, out OpenCvSharp.Point _PosVertex)
        {
            _PosS = new List<OpenCvSharp.Point>();
            _PosVertex = new OpenCvSharp.Point();

            //
            //Contours 값을 확인한다.
            //
            OpenCvSharp.Point[][] contours;
            HierarchyIndex[] hierarchy;

            Cv2.FindContours(_matOrg, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            OpenCvSharp.Point posCentriod;
            List<OpenCvSharp.Point> posSL = new List<OpenCvSharp.Point>();
            posSL.Clear();

            foreach (OpenCvSharp.Point[] p in contours)
            {
                double length = Cv2.ArcLength(p, true);

                Moments moments = Cv2.Moments(p, false);
                posCentriod.X = (int)(moments.M10 / moments.M00);
                posCentriod.Y = (int)(moments.M01 / moments.M00);

                if (length > 100)
                {

                    Cv2.Line(_matDraw, new OpenCvSharp.Point(posCentriod.X - 10, posCentriod.Y), new OpenCvSharp.Point(posCentriod.X + 10, posCentriod.Y), new Scalar(0, 255, 0), 2);
                    Cv2.Line(_matDraw, new OpenCvSharp.Point(posCentriod.X, posCentriod.Y - 10), new OpenCvSharp.Point(posCentriod.X, posCentriod.Y + 10), new Scalar(0, 255, 0), 2);
                    Cv2.Circle(_matDraw, posCentriod, 5, new Scalar(0, 255, 0), 2);

                    int nValueMaxX = -1;
                    int nValueMaxY = -1;
                    OpenCvSharp.Point pos1 = new OpenCvSharp.Point();
                    OpenCvSharp.Point pos2 = new OpenCvSharp.Point();
                    OpenCvSharp.Point pos3 = new OpenCvSharp.Point();

                    //Blob의 위치 정보를 업데이트 한다.
                    if (posCentriod.X <= _matOrg.Cols / 2 && posCentriod.Y <= _matOrg.Rows / 2)
                        _eDiePos = eObjDir.eObjDir_LeftTop;
                    else if (posCentriod.X <= _matOrg.Cols / 2 && posCentriod.Y > _matOrg.Rows / 2)
                        _eDiePos = eObjDir.eObjDir_LeftBot;
                    else if (posCentriod.X > _matOrg.Cols / 2 && posCentriod.Y <= _matOrg.Rows / 2)
                        _eDiePos = eObjDir.eObjDir_RightTop;
                    else if (posCentriod.X > _matOrg.Cols / 2 && posCentriod.Y > _matOrg.Rows / 2)
                        _eDiePos = eObjDir.eObjDir_RightBot;

                    double dValueMax = -1;

                    pos1.X = pos1.Y = -1;
                    pos2.X = pos2.Y = -1;
                    pos3.X = pos3.Y = -1;
                    nValueMaxX = -1;
                    nValueMaxY = -1;

                    //Y좌표가 0에서, X값이 최대인 경우 -> pos1
                    //X좌표가 cols에서, Y값이 최대인 경우 -> pos2
                    //contours 중, pos1<->pos + pos2<->pos 합 거리가 제일 긴 경우, pos3

                    for (int i = 0; i < p.Count(); i++)
                    {
                        if (_eDiePos == eObjDir.eObjDir_LeftTop)
                        {
                            if ((nValueMaxY == -1 || nValueMaxY < p[i].Y) && (p[i].X == 0))
                            {
                                nValueMaxY = p[i].Y;
                                pos1 = p[i];
                            }
                        }
                        else if (_eDiePos == eObjDir.eObjDir_LeftBot)
                        {
                            if ((nValueMaxY == -1 || nValueMaxY > p[i].Y) && (p[i].X == 0))
                            {
                                nValueMaxY = p[i].Y;
                                pos1 = p[i];
                            }
                        }
                        else if (_eDiePos == eObjDir.eObjDir_RightTop)
                        {
                            if ((nValueMaxY == -1 || nValueMaxY < p[i].Y) && (p[i].X == _matOrg.Cols - 1))
                            {
                                nValueMaxY = p[i].Y;
                                pos1 = p[i];
                            }
                        }
                        else if (_eDiePos == eObjDir.eObjDir_RightBot)
                        {
                            if ((nValueMaxY == -1 || nValueMaxY > p[i].Y) && (p[i].X == _matOrg.Cols - 1))
                            {
                                nValueMaxX = p[i].Y;
                                pos1 = p[i];
                            }
                        }
                    }

                    if ((pos1.X == 0 && pos1.Y == _matOrg.Rows - 1) || (pos1.X == _matOrg.Cols - 1 && pos1.Y == _matOrg.Rows - 1) ||
                        (pos1.X == _matOrg.Cols - 1 && pos1.Y == 0) || (pos1.X == 0 && pos1.Y == 0))
                    {
                        nValueMaxX = -1;
                        for (int i = 0; i < p.Count(); i++)
                        {
                            if (_eDiePos == eObjDir.eObjDir_LeftTop)
                            {
                                if ((nValueMaxX == -1 || nValueMaxX < p[i].X) && (p[i].Y == _matOrg.Rows - 1))
                                {
                                    nValueMaxX = p[i].X;
                                    pos1 = p[i];
                                }
                            }
                            else if (_eDiePos == eObjDir.eObjDir_LeftBot)
                            {
                                if ((nValueMaxX == -1 || nValueMaxX < p[i].X) && (p[i].Y == _matOrg.Rows - 1))
                                {
                                    nValueMaxX = p[i].X;
                                    pos1 = p[i];
                                }
                            }
                            else if (_eDiePos == eObjDir.eObjDir_RightTop)
                            {
                                if ((nValueMaxX == -1 || nValueMaxX > p[i].X) && (p[i].Y == _matOrg.Rows - 1))
                                {
                                    nValueMaxX = p[i].X;
                                    pos1 = p[i];
                                }
                            }
                            else if (_eDiePos == eObjDir.eObjDir_RightBot)
                            {
                                if ((nValueMaxX == -1 || nValueMaxX > p[i].X) && (p[i].Y == _matOrg.Rows - 1))
                                {
                                    nValueMaxX = p[i].X;
                                    pos1 = p[i];
                                }
                            }
                        }
                    }

                    dValueMax = -1;
                    if ((pos1.X > 0 && pos1.Y >= 0) || (pos1.X >= 0 && pos1.Y > 0))
                    {
                        for (int i = 0; i < p.Count(); i++)
                        {
                            if (!(p[i].X == 0 && p[i].Y == 0) && !(p[i].X == _matOrg.Cols - 1 && p[i].Y == 0) && !(p[i].X == 0 && p[i].Y == _matOrg.Rows - 1) && !(p[i].X == _matOrg.Cols - 1 && p[i].Y == _matOrg.Rows - 1))
                            {
                                double dDist = Math.Sqrt((p[i].X - pos1.X) * (p[i].X - pos1.X) + (p[i].Y - pos1.Y) * (p[i].Y - pos1.Y));
                                if ((dValueMax == -1 || dValueMax < dDist))
                                {
                                    dValueMax = dDist;
                                    pos2 = p[i];
                                }
                            }
                        }
                    }

                    dValueMax = -1;
                    for (int i = 0; i < p.Count(); i++)
                    {
                        if (p[i].X != 0 && p[i].Y != 0 && p[i].X != _matOrg.Cols - 1 && p[i].Y != _matOrg.Rows - 1)
                        {
                            double dDistA = Math.Sqrt((p[i].X - pos1.X) * (p[i].X - pos1.X) + (p[i].Y - pos1.Y) * (p[i].Y - pos1.Y));
                            double dDistB = Math.Sqrt((p[i].X - pos2.X) * (p[i].X - pos2.X) + (p[i].Y - pos2.Y) * (p[i].Y - pos2.Y));
                            if (((dValueMax == -1.0) || (dValueMax < dDistA + dDistB)))
                            {
                                dValueMax = dDistA + dDistB;
                                pos3 = p[i];
                            }
                        }
                    }

                    _PosS.Add(pos1);
                    _PosS.Add(pos2);
                    _PosS.Add(pos3);

                    _PosVertex.X = pos3.X;
                    _PosVertex.Y = pos3.Y;

                    OpenCvSharp.Point posDraw = pos1;
                    Cv2.Line(_matDraw, new OpenCvSharp.Point(posDraw.X - 5, posDraw.Y), new OpenCvSharp.Point(posDraw.X + 5, posDraw.Y), new Scalar(0, 255, 0), 2);
                    Cv2.Line(_matDraw, new OpenCvSharp.Point(posDraw.X, posDraw.Y - 5), new OpenCvSharp.Point(posDraw.X, posDraw.Y + 5), new Scalar(0, 255, 0), 2);
                    Cv2.Circle(_matDraw, posDraw, 2, new Scalar(0, 255, 0), 2);

                    posDraw = pos2;
                    Cv2.Line(_matDraw, new OpenCvSharp.Point(posDraw.X - 5, posDraw.Y), new OpenCvSharp.Point(posDraw.X + 5, posDraw.Y), new Scalar(0, 255, 0), 2);
                    Cv2.Line(_matDraw, new OpenCvSharp.Point(posDraw.X, posDraw.Y - 5), new OpenCvSharp.Point(posDraw.X, posDraw.Y + 5), new Scalar(0, 255, 0), 2);
                    Cv2.Circle(_matDraw, posDraw, 2, new Scalar(0, 255, 0), 2);

                    posDraw = pos3;
                    Cv2.Line(_matDraw, new OpenCvSharp.Point(posDraw.X - 5, posDraw.Y), new OpenCvSharp.Point(posDraw.X + 5, posDraw.Y), new Scalar(0, 255, 0), 2);
                    Cv2.Line(_matDraw, new OpenCvSharp.Point(posDraw.X, posDraw.Y - 5), new OpenCvSharp.Point(posDraw.X, posDraw.Y + 5), new Scalar(0, 255, 0), 2);
                    Cv2.Circle(_matDraw, posDraw, 2, new Scalar(0, 255, 0), 2);
                }
            }
        }

        public static void OpenCVMatching1(
                Mat matOrg,
                Mat matDbg,
                Mat matRef,
                ParamsMatching param,
                out Dictionary<string, Mat> debugMats,
                out stMachineResult _outResult)
        {
            debugMats = new Dictionary<string, Mat>();
            _outResult = new stMachineResult();

            Mat matOrigin = matOrg.Clone();
            Mat matReference = matRef.Clone();
            Mat matDebug = matDbg.Clone();
            debugMats.Add("[0] Origin", matOrigin);
            debugMats.Add("[1] Reference", matReference);

            using (Mat res = matOrigin.MatchTemplate(matReference, TemplateMatchModes.CCoeffNormed))
            {
                if (matDebug.Type() == MatType.CV_8U)
                    Cv2.CvtColor(matDebug, matDebug, ColorConversionCodes.GRAY2BGR);

                double minval, maxval = 0;
                //찾은 이미지의 위치를 담을 포인트형을 선업합니다.
                OpenCvSharp.Point minloc, maxloc;
                //찾은 이미지의 유사도 및 위치 값을 받습니다. 
                Cv2.MinMaxLoc(res, out minval, out maxval, out minloc, out maxloc);

                _outResult.reset();
                if (maxval > param.MinScore / 100.0)
                {
                    OpenCvSharp.Rect rect = new OpenCvSharp.Rect(maxloc.X, maxloc.Y, matReference.Width, matReference.Height);
                    Cv2.Rectangle(matDebug, rect, new OpenCvSharp.Scalar(0, 0, 255), 2);
                    _outResult.dScore = maxval;
                    _outResult.dAngle = 0;
                    _outResult.posCenter.X = maxloc.X + matReference.Width / 2;
                    _outResult.posCenter.Y = maxloc.Y + matReference.Height / 2;
                    _outResult.posRect = rect;

                    OpenCvSharp.Point posCenterN;
                    posCenterN.X = (Int32)_outResult.posCenter.X;
                    posCenterN.Y = (Int32)_outResult.posCenter.Y;
                    OpenCvSharp.Point posTmp;
                    posTmp.X = (Int32)10;
                    posTmp.Y = (Int32)(matDebug.Rows - 100);
                    string strPos = posCenterN.ToString();
                    Cv2.PutText(matDebug, strPos, posTmp, HersheyFonts.HersheyComplex, 1, Scalar.Blue, 2, LineTypes.AntiAlias);
                    posTmp.X = (Int32)10;
                    posTmp.Y = (Int32)(matDebug.Rows - 50);
                    strPos = "Score=" + _outResult.dScore.ToString() + ",  Angle=" + _outResult.dAngle.ToString();
                    Cv2.PutText(matDebug, strPos, posTmp, HersheyFonts.HersheyComplex, 1, Scalar.Blue, 2, LineTypes.AntiAlias);


                    Cv2.Line(matDebug, new OpenCvSharp.Point(posCenterN.X - 100, posCenterN.Y), new OpenCvSharp.Point(posCenterN.X + 100, posCenterN.Y), new Scalar(0, 0, 255), 5);
                    Cv2.Line(matDebug, new OpenCvSharp.Point(posCenterN.X, posCenterN.Y - 100), new OpenCvSharp.Point(posCenterN.X, posCenterN.Y + 100), new Scalar(0, 0, 255), 5);

                    debugMats.Add("[2] Result", matDebug);
                }
            }

        }
    }
}
