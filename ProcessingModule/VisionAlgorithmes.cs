using Autofac;
using LogModule;
using Matrox.MatroxImagingLibrary;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using ProberInterfaces.PMI;
using ProberInterfaces.Vision;
using ProberInterfaces.WaferAlign;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using SystemExceptions.VisionException;

namespace ProcessingModule
{
    public class VisionAlgorithmes : IVisionAlgorithmes
    {
        private int MilSystem = 0;
        private IVisionProcessing VisionProcessing;

        private const int HIST_NUM_INTENSITIES = 256;

        private long Attributes;
        private long cl_iAttributes;

        IVisionManager vm;
        ICamera cam;

        public VisionAlgorithmes()
        {

        }
        public VisionAlgorithmes(IVisionProcessing processing)
        {
            VisionProcessing = processing;
        }
        public EventCodeEnum Init(int milID, Autofac.IContainer container, long Attributes, long cl_iAttributes)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                MilSystem = milID;
                container.TryResolve<IVisionManager>(out vm);
                container.TryResolve<ICamera>(out cam);
                this.Attributes = Attributes;
                this.cl_iAttributes = cl_iAttributes;

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private void BufAllocColorUsingImageBuffer(ImageBuffer SrcImgBuf, ref MIL_ID milbuffer, bool? overlap = false)
        {
            MIL_ID milImage = MIL.M_NULL;

            try
            {
                MIL.MbufAllocColor(MilSystem, 3, SrcImgBuf.SizeX, SrcImgBuf.SizeY, 8 + MIL.M_UNSIGNED, cl_iAttributes, ref milbuffer);

                if (overlap == true)
                {
                    MIL.MbufAlloc2d(MilSystem, SrcImgBuf.SizeX, SrcImgBuf.SizeY, SrcImgBuf.Band * SrcImgBuf.ColorDept, Attributes, ref milImage);
                    MIL.MbufPut2d(milImage, 0, 0, SrcImgBuf.SizeX, SrcImgBuf.SizeY, SrcImgBuf.Buffer);

                    MIL.MimConvert(milImage, milbuffer, MIL.M_L_TO_RGB);

                }
                else
                {
                    MIL.MbufClear(milbuffer, MIL.M_COLOR_BLACK);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (milImage != MIL.M_NULL)
                {
                    MIL.MbufFree(milImage);
                }
            }
        }
        private void BufAlloc2DUsingImageBuffer(ImageBuffer SrcImgBuf, ref MIL_ID milbuffer, bool? overlap = false)
        {
            try
            {
                MIL.MbufAlloc2d(MilSystem, SrcImgBuf.SizeX, SrcImgBuf.SizeY, SrcImgBuf.Band * SrcImgBuf.ColorDept, Attributes, ref milbuffer);

                if (overlap == true)
                {
                    MIL.MbufPut2d(milbuffer, 0, 0, SrcImgBuf.SizeX, SrcImgBuf.SizeY, SrcImgBuf.Buffer);
                }
                else
                {
                    MIL.MbufClear(milbuffer, MIL.M_COLOR_BLACK);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private bool ImageBufferValidation(ImageBuffer image, int? minX = null, int? maxX = null, int? minY = null, int? maxY = null)
        {
            bool retval = false;

            try
            {
                if ((image.Band != 0) &&
                    (image.SizeX > 0) && (image.SizeY > 0) &&
                    (minX == null || image.SizeX > minX) &&
                    (maxX == null || image.SizeX < maxX) &&
                    (minY == null || image.SizeY > minY) &&
                    (maxY == null || image.SizeY < maxY)
                    )
                {
                    retval = true;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public ImageBuffer ConvolveImage(ImageBuffer image, ConvolveType type)
        {
            ImageBuffer returnBuffer = null;

            MIL_ID SrcID = MIL.M_NULL;
            MIL_ID ConvID = MIL.M_NULL;

            try
            {

                BufAlloc2DUsingImageBuffer(image, ref SrcID, true);
                BufAlloc2DUsingImageBuffer(image, ref ConvID, false);

                //string saveDirectory = @"C:\Logs\Images";
                //string curDate = string.Format($@"{saveDirectory}\OrgImg_{DateTime.Now.ToString("yyMMddHHmmss")}.bmp");
                //if (!Directory.Exists(saveDirectory))
                //{
                //    Directory.CreateDirectory(saveDirectory);
                //}

                //MIL.MbufExport(curDate, SrcID, MIL.M_BMP);

                MIL.MimConvolve(SrcID, ConvID, (int)type);

                byte[] RetArray = new byte[image.SizeX * image.SizeY];

                MIL.MbufGet(ConvID, RetArray);

                returnBuffer = new ImageBuffer(RetArray, image.SizeX, image.SizeY, image.Band, (int)ColorDept.BlackAndWhite);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (SrcID != MIL.M_NULL)
                {
                    MIL.MbufFree(SrcID);
                }

                if (ConvID != MIL.M_NULL)
                {
                    MIL.MbufFree(ConvID);
                }
            }

            return returnBuffer;
        }

        public ImageBuffer CropImage(ImageBuffer image, int offset_X, int offset_Y, int width, int height)
        {
            ImageBuffer returnBuffer = null;
            MIL_ID milbuffer = MIL.M_NULL;

            try
            {
                if ((offset_X >= 0) && (offset_X < image.SizeX) &&
                    (offset_Y >= 0) && (offset_Y < image.SizeY) &&
                    (width > 0) && (height > 0) &&
                    ImageBufferValidation(image, (offset_X + width), null, (offset_Y + height), null)
                    )
                {

                    byte[] cropArray = new byte[width * height];

                    int WidthStep;

                    int count = 0;

                    for (int j = offset_Y; j < offset_Y + height; j++)
                    {
                        WidthStep = j * image.SizeX;

                        for (int i = offset_X; i < offset_X + width; i++)
                        {
                            cropArray[count] = image.Buffer[WidthStep + i];
                            count++;
                        }
                    }

                    MIL.MbufAlloc2d(MilSystem, width, height, image.Band * image.ColorDept, Attributes, ref milbuffer);
                    MIL.MbufPut(milbuffer, cropArray);

                    returnBuffer = new ImageBuffer(cropArray, width, height, image.Band, (int)ColorDept.BlackAndWhite);
                }
                else
                {
                    LoggerManager.Error($"[VisionAlgorithmes] Method : CropImage, Failed");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (milbuffer != MIL.M_NULL)
                {
                    MIL.MbufFree(milbuffer);
                }
            }

            return returnBuffer;
        }

        public ImageBuffer CopyClipImage(ImageBuffer SrcBuf, ImageBuffer CilpBuf, int offset_X, int offset_Y)
        {
            ImageBuffer returnBuffer = null;
            MIL_ID SrcID = MIL.M_NULL;
            MIL_ID CiplID = MIL.M_NULL;

            try
            {
                if ((offset_X >= 0) &&
                   (offset_Y >= 0) &&
                   ImageBufferValidation(SrcBuf, (offset_X + CilpBuf.SizeX), null, (offset_Y + CilpBuf.SizeY), null) &&
                   ImageBufferValidation(CilpBuf, null, null, null, null)
                   )
                {
                    byte[] CilpArray = new byte[SrcBuf.SizeX * SrcBuf.SizeY];

                    BufAlloc2DUsingImageBuffer(SrcBuf, ref SrcID, true);
                    BufAlloc2DUsingImageBuffer(CilpBuf, ref CiplID, true);

                    MIL.MbufCopyClip(CiplID, SrcID, offset_X, offset_Y);
                    MIL.MbufGet(SrcID, CilpArray);

                    returnBuffer = new ImageBuffer(CilpArray, SrcBuf.SizeX, SrcBuf.SizeY, SrcBuf.Band, (int)ColorDept.BlackAndWhite);
                }
                else
                {
                    LoggerManager.Error($"[VisionAlgorithmes] Method : CopyClipImage, Failed");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (SrcID != MIL.M_NULL)
                {
                    MIL.MbufFree(SrcID);
                }

                if (CiplID != MIL.M_NULL)
                {
                    MIL.MbufFree(CiplID);
                }
            }

            return returnBuffer;
        }

        public ImageBuffer Binarization(ImageBuffer Image, double threshold)
        {
            ImageBuffer returnBuffer = null;

            try
            {
                byte[] byteArray = new byte[Image.SizeX * Image.SizeY];

                int WidthStep;

                for (int j = 0; j < Image.SizeY; j++)
                {
                    WidthStep = j * Image.SizeX;

                    for (int i = 0; i < Image.SizeX; i++)
                    {
                        if (Image.Buffer[WidthStep + i] > threshold)
                        {
                            byteArray[WidthStep + i] = 255;
                        }
                        else
                        {
                            byteArray[WidthStep + i] = 0;
                        }
                    }
                }

                returnBuffer = new ImageBuffer(byteArray, Image.SizeX, Image.SizeY, Image.Band, (int)ColorDept.BlackAndWhite);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return returnBuffer;
        }

        public MIL_ID Binarization(MIL_ID Image, double threshold)
        {
            MIL_ID returnBuffer = MIL.M_NULL;

            try
            {
                MIL_INT width = MIL.MbufInquire(Image, MIL.M_SIZE_X, MIL.M_NULL);
                MIL_INT height = MIL.MbufInquire(Image, MIL.M_SIZE_Y, MIL.M_NULL);

                MIL.MbufAlloc2d(MilSystem, width, height, 8, Attributes, ref returnBuffer);
                MIL.MbufClear(returnBuffer, 0);

                MIL.MimBinarize(Image, returnBuffer, MIL.M_FIXED + MIL.M_GREATER, threshold, MIL.M_NULL);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return returnBuffer;
        }

        public ImageBuffer DrawingPadEdge(ImageBuffer buffer, List<Point> pt, bool overlap = true)
        {
            ImageBuffer returnBuffer = null;
            MIL_ID milProcBuffer = MIL.M_NULL;

            MIL_ID milProcResultBuffer = MIL.M_NULL;

            try
            {
                byte[] MaskArray = new byte[buffer.SizeX * buffer.SizeY];

                BufAlloc2DUsingImageBuffer(buffer, ref milProcBuffer, overlap);

                double XStart;
                double YStart;

                double XEnd;
                double YEnd;

                MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_WHITE);

                for (int i = 0; i < pt.Count - 1; i++)
                {
                    if (pt[i].X >= buffer.SizeX)
                    {
                        XStart = buffer.SizeX - 1;
                    }
                    else
                    {
                        XStart = pt[i].X;
                    }

                    if (pt[i + 1].X >= buffer.SizeX)
                    {
                        XEnd = buffer.SizeX - 1;
                    }
                    else
                    {
                        XEnd = pt[i + 1].X;
                    }

                    if (pt[i].Y >= buffer.SizeY)
                    {
                        YStart = buffer.SizeY - 1;
                    }
                    else
                    {
                        YStart = pt[i].Y;
                    }

                    if (pt[i + 1].Y >= buffer.SizeY)
                    {
                        YEnd = buffer.SizeY - 1;
                    }
                    else
                    {
                        YEnd = pt[i + 1].Y;
                    }
                    
                    MIL.MgraLine(MIL.M_DEFAULT, milProcBuffer, XStart, YStart, XEnd, YEnd);
                }

                MIL.MbufGet(milProcBuffer, MaskArray);

                returnBuffer = new ImageBuffer(MaskArray, buffer.SizeX, buffer.SizeY, buffer.Band, (int)ColorDept.BlackAndWhite);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (milProcBuffer != MIL.M_NULL)
                {
                    MIL.MbufFree(milProcBuffer);
                }
            }

            return returnBuffer;
        }

        public ImageBuffer MaskingBuffer(ImageBuffer buffer, List<Point> pt)
        {
            ImageBuffer returnBuffer = null;
            MIL_ID milProcBuffer = MIL.M_NULL;

            try
            {
                byte[] MaskArray = new byte[buffer.SizeX * buffer.SizeY];

                BufAlloc2DUsingImageBuffer(buffer, ref milProcBuffer);

                MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_WHITE);

                for (int i = 0; i < pt.Count; i++)
                {
                    if (i == pt.Count - 1)
                    {
                        MIL.MgraLine(MIL.M_DEFAULT, milProcBuffer, pt[i].X, pt[i].Y, pt[0].X, pt[0].Y);
                    }
                    else
                    {
                        MIL.MgraLine(MIL.M_DEFAULT, milProcBuffer, pt[i].X, pt[i].Y, pt[i + 1].X, pt[i + 1].Y);
                    }
                }

                double x, y;

                x = pt.Average(v => v.X);
                y = pt.Average(v => v.Y);

                MIL.MgraFill(MIL.M_DEFAULT, milProcBuffer, x, y);

                MIL.MbufGet(milProcBuffer, MaskArray);

                returnBuffer = new ImageBuffer(MaskArray, buffer.SizeX, buffer.SizeY, buffer.Band, (int)ColorDept.BlackAndWhite);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (milProcBuffer != MIL.M_NULL)
                {
                    MIL.MbufFree(milProcBuffer);
                }
            }

            return returnBuffer;
        }

        public ImageBuffer DrawingShape(ImageBuffer CurrentButter, List<Point> pt, ColorDept colordept, bool? fill = false, bool? overlap = false)
        {
            byte[] resultBuffer;
            ImageBuffer returnBuffer = null;
            MIL_ID milProcBuffer = MIL.M_NULL;

            try
            {
                if (colordept == ColorDept.BlackAndWhite)
                {
                    resultBuffer = new byte[CurrentButter.SizeX * CurrentButter.SizeY * 1];
                }
                else
                {
                    resultBuffer = new byte[CurrentButter.SizeX * CurrentButter.SizeY * 3];
                }

                if (pt.Count > 0)
                {
                    //returnBuffer.Buffer = new byte[CurrentButter.SizeX * CurrentButter.SizeY * 3];
                    //returnBuffer.ColorDept = (int)colordept;

                    if (colordept == ColorDept.BlackAndWhite)
                    {
                        BufAlloc2DUsingImageBuffer(CurrentButter, ref milProcBuffer, overlap);
                    }
                    else if (colordept == ColorDept.Color24)
                    {
                        BufAllocColorUsingImageBuffer(CurrentButter, ref milProcBuffer, overlap);
                    }


                    // Drawing

                    if (colordept == ColorDept.BlackAndWhite)
                    {
                        MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_WHITE);
                    }
                    else
                    {
                        MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_YELLOW);
                    }


                    for (int i = 0; i < pt.Count; i++)
                    {
                        if (i == pt.Count - 1)
                        {
                            MIL.MgraLine(MIL.M_DEFAULT, milProcBuffer, pt[i].X, pt[i].Y, pt[0].X, pt[0].Y);
                        }
                        else
                        {
                            MIL.MgraLine(MIL.M_DEFAULT, milProcBuffer, pt[i].X, pt[i].Y, pt[i + 1].X, pt[i + 1].Y);
                        }
                    }

                    if (fill == true)
                    {
                        double x, y;

                        x = pt.Average(v => v.X);
                        y = pt.Average(v => v.Y);

                        MIL.MgraFill(MIL.M_DEFAULT, milProcBuffer, x, y);
                    }
                }

                if (colordept == ColorDept.BlackAndWhite)
                {
                    MIL.MbufGet(milProcBuffer, resultBuffer);
                }
                else
                {
                    MIL.MbufGetColor(milProcBuffer, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BAND, resultBuffer);
                }

                returnBuffer = new ImageBuffer(resultBuffer, CurrentButter.SizeX, CurrentButter.SizeY, CurrentButter.Band, (int)colordept);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (milProcBuffer != MIL.M_NULL)
                {
                    MIL.MbufFree(milProcBuffer);
                }
            }

            return returnBuffer;
        }

        public double GetPixelSum(ImageBuffer Image)
        {
            double retval = 0;

            MIL_ID MimStatResult = MIL.M_NULL;
            MIL_ID milProcBuffer = MIL.M_NULL;

            try
            {
                BufAlloc2DUsingImageBuffer(Image, ref milProcBuffer, true);

                MIL.MimAllocResult(MilSystem, MIL.M_DEFAULT, MIL.M_STAT_LIST, ref MimStatResult);
                MIL.MimStat(milProcBuffer, MimStatResult, MIL.M_SUM, MIL.M_NULL, MIL.M_NULL, MIL.M_NULL);
                MIL.MimGetResult(MimStatResult, MIL.M_SUM, ref retval);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (MimStatResult != MIL.M_NULL)
                {
                    MIL.MimFree(MimStatResult);
                }

                if (milProcBuffer != MIL.M_NULL)
                {
                    MIL.MbufFree(milProcBuffer);
                }
            }

            return retval;
        }
        public TRIVALUE GET_TRIANGLE(double[] Hist, double TotalSize, int StartPoint, int EndPoint, bool Flag_Calc_Point = true)
        {
            TRIVALUE retval = new TRIVALUE();

            try
            {
                float LineA, LineB;

                float MinX, MaxX;
                float MinY, MaxY;

                float MaxDistance = 0;
                float TempDistance;

                float tX, tY;

                double tmpHvalue = 0;

                double tmpHeight;

                if (Flag_Calc_Point == true)
                {
                    for (int i = StartPoint; i < EndPoint; i++)
                    {
                        if ((retval.SPoint == -1) && (Hist[i] > 0))
                        {
                            retval.SPoint = i;
                        }

                        if (retval.SPoint != -1)
                        {
                            if (Hist[i] > tmpHvalue)
                            {
                                tmpHvalue = Hist[i];
                                retval.MPoint = i;
                            }
                        }
                    }
                }
                else
                {
                    retval.SPoint = StartPoint;
                    retval.MPoint = EndPoint;
                }

                MinX = retval.SPoint;
                MaxX = retval.MPoint;

                MinY = (float)Hist[retval.SPoint];
                MaxY = (float)Hist[retval.MPoint];

                LineA = (MaxY - MinY) / (MaxX - MinX);
                LineB = MinY - (LineA * MinX);

                for (int i = (int)MinX; i < (int)MaxX; i++)
                {
                    tX = i;
                    tY = (float)Hist[i];

                    tmpHeight = LineA * tX + LineB;

                    if ((tY > 0) && (tY < tmpHeight))
                    {
                        TempDistance = Math.Abs((LineA * tX) + (-1 * tY) + LineB) / (float)Math.Pow((LineA * LineA) + 1, 2);

                        if (TempDistance > MaxDistance)
                        {
                            MaxDistance = TempDistance;
                            retval.TPoint = i;
                        }
                    }
                }

                if ((retval.TPoint != -1) && (retval.TPoint > retval.SPoint))
                {
                    retval.Status = (int)MaxDistance;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public long GetThreshold_ForPadDetection(ImageBuffer SrcBuf)
        {
            long thre = 0;

            MIL_ID HistResultImID = MIL.M_NULL;
            MIL_ID SrcImageBufID = MIL.M_NULL;

            try
            {
                MIL_INT[] histbuf = new MIL_INT[HIST_NUM_INTENSITIES];
                double mean1 = 0, mean2 = 0;
                long cnt1 = 0, cnt2 = 0;
                double tmpThre = 0;

                int width = SrcBuf.SizeX;
                int height = SrcBuf.SizeY;

                BufAlloc2DUsingImageBuffer(SrcBuf, ref SrcImageBufID, true);

                MIL.MimAllocResult(MilSystem, HIST_NUM_INTENSITIES, MIL.M_HIST_LIST, ref HistResultImID);
                MIL.MimHistogram(SrcImageBufID, HistResultImID);
                MIL.MimGetResult(HistResultImID, MIL.M_VALUE, histbuf);

                for (int i = 0; i < HIST_NUM_INTENSITIES; i++)
                {
                    if (histbuf[i] > 10000000)
                    {
                        mean1 = mean1 + histbuf[i];
                    }
                    else
                    {
                        mean1 = mean1 + histbuf[i] * i;
                    }
                }

                if (mean1 == 0)
                {
                    mean1 = 1;
                }

                if (width <= 0)
                {
                    width = 1;
                }

                if (height <= 0)
                {
                    height = 1;
                }

                tmpThre = mean1 / (width * height);

                for (int i = 0; i <= 2; i++)
                {
                    mean1 = 0;
                    mean2 = 0;
                    cnt1 = 0;
                    cnt2 = 0;

                    for (int l = 0; l < HIST_NUM_INTENSITIES; l++)
                    {
                        if (l < tmpThre)
                        {
                            mean1 = mean1 + histbuf[l] * l;
                            cnt1 = cnt1 + histbuf[l];
                        }
                        else
                        {
                            mean2 = mean2 + histbuf[l] * l;
                            cnt2 = cnt2 + histbuf[l];
                        }
                    }

                    if (cnt1 == 0)
                    {
                        cnt1 = 1;
                    }

                    if (cnt2 == 0)
                    {
                        cnt2 = 1;
                    }

                    tmpThre = (mean1 / cnt1 + mean2 / cnt2) / 2;
                }

                thre = (long)tmpThre;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (HistResultImID != MIL.M_NULL)
                {
                    MIL.MimFree(HistResultImID);
                }

                if (SrcImageBufID != MIL.M_NULL)
                {
                    MIL.MbufFree(SrcImageBufID);
                }
            }

            return thre;
        }

        public long GetThreshold_ForPadDetection(MIL_ID SrcImageBufID, bool excludedZeroValue = false, int maxIterations = 3)
        {
            long thre = 0;
            MIL_ID HistResultImID = MIL.M_NULL;
            const int HIST_NUM_INTENSITIES = 256;

            try
            {
                MIL_INT[] histbuf = new MIL_INT[HIST_NUM_INTENSITIES];
                double backgroundMean = 0, foregroundMean = 0;
                long backgroundCount = 0, foregroundCount = 0;
                double tmpThre = 0;

                // 히스토그램 결과를 위한 버퍼 할당
                MIL.MimAllocResult(MilSystem, HIST_NUM_INTENSITIES, MIL.M_HIST_LIST, ref HistResultImID);
                MIL.MimHistogram(SrcImageBufID, HistResultImID);
                MIL.MimGetResult(HistResultImID, MIL.M_VALUE, histbuf);

                if (excludedZeroValue)
                {
                    histbuf[0] = 0;
                }

                // 전체 평균을 계산하여 초기 임계값 설정
                double totalSum = 0;
                long totalCount = 0;
                for (int i = 0; i < HIST_NUM_INTENSITIES; i++)
                {
                    totalSum += histbuf[i] * i;
                    totalCount += histbuf[i];
                }

                if (totalCount == 0) totalCount = 1; // 나누기 0 방지
                tmpThre = totalSum / totalCount;

                // 반복을 통한 임계값 갱신
                for (int iteration = 0; iteration < maxIterations; iteration++)
                {
                    backgroundMean = 0;
                    foregroundMean = 0;
                    backgroundCount = 0;
                    foregroundCount = 0;

                    for (int l = 0; l < HIST_NUM_INTENSITIES; l++)
                    {
                        if (l < tmpThre)
                        {
                            backgroundMean += histbuf[l] * l;
                            backgroundCount += histbuf[l];
                        }
                        else
                        {
                            foregroundMean += histbuf[l] * l;
                            foregroundCount += histbuf[l];
                        }
                    }

                    if (backgroundCount == 0) backgroundCount = 1;
                    if (foregroundCount == 0) foregroundCount = 1;

                    tmpThre = (backgroundMean / backgroundCount + foregroundMean / foregroundCount) / 2;
                }

                thre = (long)tmpThre;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (HistResultImID != MIL.M_NULL)
                {
                    MIL.MimFree(HistResultImID);
                }
            }

            return thre;
        }
        public ImageBuffer GetEdgeImage(ImageBuffer targetImage)
        {
            MIL_ID MilEdgeContext = MIL.M_NULL;
            MIL_ID MilEdgeResult = MIL.M_NULL;
            MIL_ID milProcBuffer = MIL.M_NULL;
            MIL_ID milResultBuffer = MIL.M_NULL;

            ImageBuffer resultImageBuffer = new ImageBuffer();

            try
            {
                byte[] resultarr = new byte[targetImage.SizeX * targetImage.SizeY * 3];
                targetImage.CopyTo(resultImageBuffer);
                BufAlloc2DUsingImageBuffer(targetImage, ref milProcBuffer, true);
                BufAlloc2DUsingImageBuffer(targetImage, ref milResultBuffer, false);

                //string curDate = string.Format("C:\\Logs\\Images\\EdgeResult_{0}.bmp", DateTime.Now.ToString("yyMMddHHmmss"));

                //MIL.MbufExport(curDate, milProcBuffer, MIL.M_BMP);

                MIL.MedgeAlloc(MilSystem, MIL.M_CONTOUR, MIL.M_DEFAULT, ref MilEdgeContext);
                MIL.MedgeAllocResult(MilSystem, MIL.M_DEFAULT, ref MilEdgeResult);

                // Calculate edges and features.
                MIL.MedgeCalculate(MilEdgeContext, milProcBuffer, MIL.M_NULL, MIL.M_NULL, MIL.M_NULL, MilEdgeResult, MIL.M_DEFAULT);

                MIL_INT NumEdgeFound = 0;
                MIL.MedgeGetResult(MilEdgeResult, MIL.M_DEFAULT, MIL.M_NUMBER_OF_CHAINS + MIL.M_TYPE_MIL_INT, ref NumEdgeFound);

                MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_WHITE);
                MIL.MedgeDraw(MIL.M_DEFAULT, MilEdgeResult, milResultBuffer, MIL.M_DRAW_EDGES, MIL.M_DEFAULT,
                    MIL.M_DEFAULT);

                MIL.MbufGetColor(milResultBuffer, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BAND, resultarr);

                //delay.DelayFor(100);
                //curDate = string.Format("C:\\Logs\\Images\\EdgeResult_{0}.bmp", DateTime.Now.ToString("yyMMddHHmmss"));

                //MIL.MbufExport(curDate, milResultBuffer, MIL.M_BMP);

                MIL.MbufGet2d(milResultBuffer, 0, 0, targetImage.SizeX, targetImage.SizeY, resultImageBuffer.Buffer);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

                if (MilEdgeContext != MIL.M_NULL)
                {
                    MIL.MedgeFree(MilEdgeContext);
                }

                if (MilEdgeResult != MIL.M_NULL)
                {
                    MIL.MedgeFree(MilEdgeResult);
                }

                if (milProcBuffer != MIL.M_NULL)
                {
                    MIL.MbufFree(milProcBuffer);
                }

                if (milResultBuffer != MIL.M_NULL)
                {
                    MIL.MbufFree(milResultBuffer);
                }
            }
            return resultImageBuffer;
        }

        public int GetOtsuThreshold(ImageBuffer curImg)
        {
            byte t = 0;
            float[] vet = new float[256];
            int[] hist = new int[256];
            try
            {
                vet.Initialize();
                hist.Initialize();

                float p1, p2, p12;
                int k;

                for (int i = 0; i < curImg.SizeX; i++)
                {
                    for (int j = 0; j < curImg.SizeY; j++)
                    {
                        hist[curImg.Buffer[(curImg.SizeX * j) + i]]++;
                    }
                }

                int minindex = 0, maxindex = 0;

                for (int i = 0; i < 256; i++)
                {
                    if (hist[i] != 0)
                    {
                        minindex = i;
                        break;
                    }
                }

                for (int i = 255; i >= 0; i--)
                {
                    if (hist[i] != 0)
                    {
                        maxindex = i;
                        break;
                    }
                }

                for (k = minindex + 1; k != maxindex; k++)
                {
                    if (k > 255) break;
                    p1 = Px(minindex, k, hist);             // 제일 어두운 색부터 제일 밝은 색까지 모두 더한 값
                    p2 = Px(k + 1, maxindex, hist);
                    p12 = p1 * p2;
                    if (p12 == 0)
                        p12 = 1;
                    float diff = (Mx(0, k, hist) * p2) - (Mx(k + 1, 255, hist) * p1);
                    vet[k] = (float)diff * diff / p12;
                }

                t = (byte)findMax(vet, 256);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err, "getOtsuThreshold() : Error occured.");
            }
            return t;
        }

        private float Px(int init, int end, int[] hist)
        {
            int sum = 0;
            int i;
            try
            {
                for (i = init; i <= end; i++)
                    sum += hist[i];
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err, "getOtsuThreshold() : Error occured.");
            }
            return (float)sum;
        }

        // function is used to compute the mean values in the equation (mu)
        private float Mx(int init, int end, int[] hist)
        {
            int sum = 0;
            int i;
            try
            {
                for (i = init; i <= end; i++)
                    sum += i * hist[i];
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err, "getOtsuThreshold() : Error occured.");
            }
            return (float)sum;
        }

        // finds the maximum element in a vector
        public int findMax(float[] vec, int n)
        {
            float maxVec = 0;
            int idx = 0;
            int i;

            try
            {
                for (i = 1; i < n - 1; i++)
                {
                    if (vec[i] > maxVec)
                    {
                        maxVec = vec[i];
                        idx = i;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err, "getOtsuThreshold() : Error occured.");
            }
            return idx;
        }



        public double GetThreshold_ProbeMark(ImageBuffer CropImage, ImageBuffer maskImage, int PadColor)
        {
            double retval = 0;

            Stopwatch stw = new Stopwatch();
            List<KeyValuePair<string, long>> timeStamp;

            timeStamp = new List<KeyValuePair<string, long>>();

            stw.Start();

            try
            {
                timeStamp.Add(new KeyValuePair<string, long>("[START] FUNCTION", stw.ElapsedMilliseconds));

                int MaxPixelValue = 255;

                int Hist_Size = 256;
                int ImageWidth;
                int ImageHeight;

                double[] Histogram_data = new double[Hist_Size];
                double[] Histogram_Sum_data = new double[Hist_Size];

                int BufferLength;
                byte[] ReverseBuffer;

                TRIVALUE FirstTValue = new TRIVALUE();
                TRIVALUE Variance_TValue_X = new TRIVALUE();
                TRIVALUE Variance_TValue_Y = new TRIVALUE();

                double MaskBufferPixelCount = 0;

                double RAverage = 0;
                int FirstOffsetPoint = -1;

                int Arr_Count;

                double Variance_Threshold_Margin;

                if ((CropImage.Buffer.Count() != maskImage.Buffer.Count()) ||
                    (CropImage.SizeX != maskImage.SizeX) ||
                    (CropImage.SizeY != maskImage.SizeY)
                    )
                {
                    // ERROR
                    retval = -1;
                }
                else
                {
                    BufferLength = CropImage.Buffer.Count();
                    ImageWidth = CropImage.SizeX;
                    ImageHeight = CropImage.SizeY;

                    ReverseBuffer = new byte[BufferLength];

                    for (int i = 0; i < BufferLength; i++)
                    {
                        if (maskImage.Buffer[i] == MaxPixelValue)
                        {
                            if (PadColor == 2)
                            {
                                ReverseBuffer[i] = CropImage.Buffer[i];
                            }
                            else
                            {
                                ReverseBuffer[i] = (byte)(MaxPixelValue - CropImage.Buffer[i]);
                            }

                            Histogram_data[ReverseBuffer[i]]++;
                            MaskBufferPixelCount++;
                        }
                    }

                    // Get Histogram Sum Value
                    Histogram_Sum_data[0] = Histogram_data[0];

                    for (int i = 1; i < Hist_Size; i++)
                    {
                        Histogram_Sum_data[i] += Histogram_Sum_data[i - 1] + Histogram_data[i];
                    }

                    FirstTValue = GET_TRIANGLE(Histogram_data, MaskBufferPixelCount, 0, Hist_Size - 1);

                    if (FirstTValue.Status == -1)
                    {
                        // ERROR
                        retval = -1;
                    }
                    else
                    {
                        RAverage = Histogram_Sum_data[FirstTValue.TPoint - 1] / (FirstTValue.TPoint - FirstTValue.SPoint);

                        // Get First Offset Point

                        for (int i = FirstTValue.TPoint - 1; i >= FirstTValue.SPoint; i--)
                        {
                            if (Histogram_data[i] < RAverage)
                            {
                                if (FirstOffsetPoint == -1)
                                {
                                    FirstOffsetPoint = i;

                                    break;
                                }
                            }
                        }

                        Arr_Count = (FirstTValue.TPoint - FirstTValue.SPoint);

                        double sum_X;
                        double sum_Y;

                        //double[] sum_X_Arr = new double[Arr_Count];
                        //double[] sum_Y_Arr = new double[Arr_Count];

                        //double WeightedPoint_X;
                        //double WeightedPoint_Y;

                        double[] WeightedPoint_X_Arr = new double[Arr_Count];
                        double[] WeightedPoint_Y_Arr = new double[Arr_Count];

                        double WeightedPoint_Count = 0;

                        int tmpThresholdValue = FirstTValue.SPoint;

                        double[] variance_X_Arr = new double[Arr_Count];
                        double[] variance_Y_Arr = new double[Arr_Count];

                        timeStamp.Add(new KeyValuePair<string, long>("111111", stw.ElapsedMilliseconds));

                        bool[] ComparedFLAG = new bool[BufferLength];

                        int[] PreCalcualtePosX = new int[BufferLength];
                        int[] PreCalcualtePosY = new int[BufferLength];

                        double m, n;

                        for (int i = 0; i < BufferLength; i++)
                        {
                            PreCalcualtePosX[i] = (i / ImageWidth) - 1;
                            PreCalcualtePosY[i] = (i % ImageWidth) - 1;
                        }

                        timeStamp.Add(new KeyValuePair<string, long>("222222", stw.ElapsedMilliseconds));

                        for (int k = 0; k < Arr_Count; k++)
                        {
                            sum_X = 0;
                            sum_Y = 0;

                            //WeightedPoint_X = 0;
                            //WeightedPoint_Y = 0;
                            WeightedPoint_Count = 0;

                            //Parallel.For(0, BufferLength, i =>
                            //{
                            //    if ((ReverseBuffer[i] <= tmpThresholdValue) && maskImage.Buffer[i] == MaxPixelValue)
                            //    {
                            //        CurrentYPos = (i / ImageWidth) - 1;
                            //        CurrentXPos = (i % ImageWidth) - 1;

                            //        PreCalcualtePosX[i] = CurrentXPos;
                            //        PreCalcualtePosY[i] = CurrentYPos;

                            //        WeightedPoint_X = WeightedPoint_X + CurrentXPos;
                            //        WeightedPoint_Y = WeightedPoint_Y + CurrentYPos;

                            //        WeightedPoint_Count++;

                            //        ComparedFLAG[i] = true;
                            //    }
                            //});

                            //PreCalcualtePosX.Select(x => ReverseBuffer[x] <= tmpThresholdValue).Sum();

                            //double average = someDoubles.Average();
                            //double sumOfSquaresOfDifferences = someDoubles.Select(val => (val - average) * (val - average)).Sum();
                            //double sd = Math.Sqrt(sumOfSquaresOfDifferences / someDoubles.Length);

                            for (int i = 0; i < BufferLength; i++)
                            {
                                if ((ReverseBuffer[i] <= tmpThresholdValue) && maskImage.Buffer[i] == MaxPixelValue)
                                {
                                    WeightedPoint_X_Arr[k] = WeightedPoint_X_Arr[k] + PreCalcualtePosX[i];
                                    WeightedPoint_Y_Arr[k] = WeightedPoint_Y_Arr[k] + PreCalcualtePosY[i];

                                    WeightedPoint_Count++;

                                    ComparedFLAG[i] = true;
                                }
                            }

                            if (WeightedPoint_Count == 0)
                            {
                                WeightedPoint_X_Arr[k] = -1;
                                WeightedPoint_Y_Arr[k] = -1;

                                variance_X_Arr[k] = 0;
                                variance_Y_Arr[k] = 0;
                            }
                            else
                            {
                                WeightedPoint_X_Arr[k] = (WeightedPoint_X_Arr[k] / WeightedPoint_Count);
                                WeightedPoint_Y_Arr[k] = (WeightedPoint_Y_Arr[k] / WeightedPoint_Count);

                                for (int i = 0; i < BufferLength; i++)
                                {
                                    if (ComparedFLAG[i] == true)
                                    {
                                        m = (WeightedPoint_X_Arr[k] - PreCalcualtePosX[i]);
                                        n = (WeightedPoint_Y_Arr[k] - PreCalcualtePosY[i]);

                                        sum_X = sum_X + (m * m);
                                        sum_Y = sum_Y + (n * n);
                                    }
                                }

                                variance_X_Arr[k] = sum_X / (WeightedPoint_Count);
                                variance_Y_Arr[k] = sum_Y / (WeightedPoint_Count);
                            }

                            tmpThresholdValue++;
                        }

                        timeStamp.Add(new KeyValuePair<string, long>("333333", stw.ElapsedMilliseconds));

                        Variance_TValue_X = GET_TRIANGLE(variance_X_Arr, Arr_Count, 0, Arr_Count - 1, false);
                        Variance_TValue_Y = GET_TRIANGLE(variance_Y_Arr, Arr_Count, 0, Arr_Count - 1, false);

                        Variance_Threshold_Margin = FirstTValue.TPoint - (FirstTValue.MPoint - FirstTValue.SPoint + 1) * 0.2;

                        if (((Variance_TValue_X.TPoint + FirstTValue.SPoint) < Variance_Threshold_Margin) &&
                            ((Variance_TValue_Y.TPoint + FirstTValue.SPoint) < Variance_Threshold_Margin)
                            )
                        {
                            retval = FirstOffsetPoint;
                        }
                        else
                        {
                            if ((Variance_TValue_X.Status >= 0) && (Variance_TValue_Y.Status >= 0))
                            {
                                if (Variance_TValue_X.TPoint < Variance_TValue_Y.TPoint)
                                {
                                    retval = Variance_TValue_Y.TPoint + FirstTValue.SPoint;
                                }
                                else
                                {
                                    retval = Variance_TValue_X.TPoint + FirstTValue.SPoint;
                                }
                            }
                            else if (Variance_TValue_X.Status >= 0)
                            {
                                retval = Variance_TValue_X.TPoint + FirstTValue.SPoint;
                            }
                            else if (Variance_TValue_Y.Status >= 0)
                            {
                                retval = Variance_TValue_Y.TPoint + FirstTValue.SPoint;
                            }
                            else
                            {
                                retval = FirstOffsetPoint;
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                // ERROR
                retval = -1;

                LoggerManager.Exception(err);
            }

            timeStamp.Add(new KeyValuePair<string, long>("[END] FUNCTION", stw.ElapsedMilliseconds));

            return retval;
        }

        private MIL_ID GetBlobResult(ImageBuffer SrcBuf,
                                     BlobParameter blobparam = null,
                                     ROIParameter roiparam = null,
                                     bool UseAllFeatures = false,
                                     bool foregroundIsZero = false)
        {

            MIL_ID milBlobResult = MIL.M_NULL;

            MIL_ID milProcBuffer = MIL.M_NULL;
            MIL_ID milBlobFeatureList = MIL.M_NULL;

            try
            {
                MIL.MbufAlloc2d(MilSystem, SrcBuf.SizeX, SrcBuf.SizeY, SrcBuf.ColorDept, Attributes, ref milProcBuffer);
                MIL.MbufClear(milProcBuffer, 0);

                MIL.MbufPut(milProcBuffer, SrcBuf.Buffer);

                // Allocate a Blob Control
                if (milBlobResult == MIL.M_NULL)
                {
                    MIL.MblobAllocResult(MilSystem, ref milBlobResult);
                }

                //Allocate a feature list
                if (milBlobFeatureList == MIL.M_NULL)
                {
                    MIL.MblobAllocFeatureList(MilSystem, ref milBlobFeatureList);
                }

                if (foregroundIsZero == false)
                {
                    MIL.MblobControl(milBlobResult, MIL.M_FOREGROUND_VALUE, MIL.M_NONZERO);
                }
                else
                {
                    MIL.MblobControl(milBlobResult, MIL.M_FOREGROUND_VALUE, MIL.M_ZERO);
                }

                if (blobparam != null)
                {
                    if (blobparam.BlobMinRadius.Value > 0)
                    {
                        // PreProcessing : opening , closing => For Remove Noise 
                        // Remove small particles and then remove small holes

                        MIL.MimOpen(milProcBuffer, milProcBuffer, 1, MIL.M_BINARY);
                        MIL.MimClose(milProcBuffer, milProcBuffer, 1, MIL.M_BINARY);
                    }
                }

                if (UseAllFeatures == true)
                {
                    MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_ALL_FEATURES);
                }
                else
                {
                    MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_FERET_X);
                    MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_FERET_Y);

                    MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_AREA);
                    MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_CENTER_OF_GRAVITY);
                    MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_AXIS_PRINCIPAL_ANGLE);

                    MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_BOX_X_MAX);
                    MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_BOX_X_MIN);
                    MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_BOX_Y_MAX);
                    MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_BOX_Y_MIN);
                }

                // Sort Option
                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_AREA + MIL.M_SORT1_DOWN);

                //if (fillholes == true)
                //{
                //    MIL.MblobReconstruct(milProcBuffer, MIL.M_NULL, milProcBuffer,
                //                                        MIL.M_FILL_HOLES, MIL.M_BINARY + MIL.M_8_CONNECTED);
                //}

                MIL.MblobCalculate(milProcBuffer, MIL.M_NULL, milBlobFeatureList, milBlobResult);

                // Apply Blob Filter
                if (blobparam != null)
                {
                    if (blobparam.MinBlobArea.Value != 0)
                    {
                        MIL.MblobSelect(milBlobResult, MIL.M_EXCLUDE, MIL.M_AREA, MIL.M_LESS, blobparam.MinBlobArea.Value, MIL.M_NULL);
                    }

                    if (blobparam.MaxBlobArea.Value != 0)
                    {
                        MIL.MblobSelect(milBlobResult, MIL.M_EXCLUDE, MIL.M_AREA, MIL.M_GREATER, blobparam.MaxBlobArea.Value, MIL.M_NULL);
                    }

                    if (blobparam.MIN_FERET_X.Value != 0)
                    {
                        MIL.MblobSelect(milBlobResult, MIL.M_EXCLUDE, MIL.M_FERET_X, MIL.M_LESS, blobparam.MIN_FERET_X.Value, MIL.M_NULL);
                    }

                    if (blobparam.MAX_FERET_X.Value != 0)
                    {
                        MIL.MblobSelect(milBlobResult, MIL.M_EXCLUDE, MIL.M_FERET_X, MIL.M_GREATER, blobparam.MAX_FERET_X.Value, MIL.M_NULL);
                    }

                    if (blobparam.MIN_FERET_Y.Value != 0)
                    {
                        MIL.MblobSelect(milBlobResult, MIL.M_EXCLUDE, MIL.M_FERET_Y, MIL.M_LESS, blobparam.MIN_FERET_Y.Value, MIL.M_NULL);
                    }

                    if (blobparam.MAX_FERET_Y.Value != 0)
                    {
                        MIL.MblobSelect(milBlobResult, MIL.M_EXCLUDE, MIL.M_FERET_Y, MIL.M_GREATER, blobparam.MAX_FERET_Y.Value, MIL.M_NULL);
                    }
                }
            }
            catch (MILException err)
            {
                throw new VisionException(err.Message, err, EventCodeEnum.VISION_BLOB_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }
            finally
            {
                if (milProcBuffer != MIL.M_NULL)
                {
                    MIL.MbufFree(milProcBuffer);
                }

                if (milBlobFeatureList != MIL.M_NULL)
                {
                    MIL.MblobFree(milBlobFeatureList);
                }
            }

            return milBlobResult;
        }

        private MIL_ID GetBlobResult(MIL_ID milProcBuffer,
                                     BlobParameter blobparam = null,
                                     ROIParameter roiparam = null,
                                     bool UseAllFeatures = false,
                                     bool foregroundIsZero = false)
        {

            MIL_ID milBlobResult = MIL.M_NULL;
            MIL_ID milBlobFeatureList = MIL.M_NULL;

            try
            {
                MIL_INT width = MIL.MbufInquire(milProcBuffer, MIL.M_SIZE_X, MIL.M_NULL);
                MIL_INT height = MIL.MbufInquire(milProcBuffer, MIL.M_SIZE_Y, MIL.M_NULL);

                // Allocate a Blob Control
                if (milBlobResult == MIL.M_NULL)
                {
                    MIL.MblobAllocResult(MilSystem, ref milBlobResult);
                }

                //Allocate a feature list
                if (milBlobFeatureList == MIL.M_NULL)
                {
                    MIL.MblobAllocFeatureList(MilSystem, ref milBlobFeatureList);
                }

                if (foregroundIsZero == false)
                {
                    MIL.MblobControl(milBlobResult, MIL.M_FOREGROUND_VALUE, MIL.M_NONZERO);
                }
                else
                {
                    MIL.MblobControl(milBlobResult, MIL.M_FOREGROUND_VALUE, MIL.M_ZERO);
                }

                if (blobparam != null)
                {
                    if (blobparam.BlobMinRadius.Value > 0)
                    {
                        // PreProcessing : opening , closing => For Remove Noise 
                        // Remove small particles and then remove small holes

                        MIL.MimOpen(milProcBuffer, milProcBuffer, 1, MIL.M_BINARY);
                        MIL.MimClose(milProcBuffer, milProcBuffer, 1, MIL.M_BINARY);
                    }
                }

                if (UseAllFeatures == true)
                {
                    MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_ALL_FEATURES);
                }
                else
                {
                    MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_FERET_X);
                    MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_FERET_Y);

                    MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_AREA);
                    MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_CENTER_OF_GRAVITY);
                    MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_AXIS_PRINCIPAL_ANGLE);

                    MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_BOX_X_MAX);
                    MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_BOX_X_MIN);
                    MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_BOX_Y_MAX);
                    MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_BOX_Y_MIN);
                }

                // Sort Option
                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_AREA + MIL.M_SORT1_DOWN);

                //if (fillholes == true)
                //{
                //    MIL.MblobReconstruct(milProcBuffer, MIL.M_NULL, milProcBuffer,
                //                                        MIL.M_FILL_HOLES, MIL.M_BINARY + MIL.M_8_CONNECTED);
                //}

                MIL.MblobCalculate(milProcBuffer, MIL.M_NULL, milBlobFeatureList, milBlobResult);

                // Apply Blob Filter
                if (blobparam != null)
                {
                    if (blobparam.MinBlobArea.Value != 0)
                    {
                        MIL.MblobSelect(milBlobResult, MIL.M_EXCLUDE, MIL.M_AREA, MIL.M_LESS, blobparam.MinBlobArea.Value, MIL.M_NULL);
                    }

                    if (blobparam.MaxBlobArea.Value != 0)
                    {
                        MIL.MblobSelect(milBlobResult, MIL.M_EXCLUDE, MIL.M_AREA, MIL.M_GREATER, blobparam.MaxBlobArea.Value, MIL.M_NULL);
                    }

                    if (blobparam.MIN_FERET_X.Value != 0)
                    {
                        MIL.MblobSelect(milBlobResult, MIL.M_EXCLUDE, MIL.M_FERET_X, MIL.M_LESS, blobparam.MIN_FERET_X.Value, MIL.M_NULL);
                    }

                    if (blobparam.MAX_FERET_X.Value != 0)
                    {
                        MIL.MblobSelect(milBlobResult, MIL.M_EXCLUDE, MIL.M_FERET_X, MIL.M_GREATER, blobparam.MAX_FERET_X.Value, MIL.M_NULL);
                    }

                    if (blobparam.MIN_FERET_Y.Value != 0)
                    {
                        MIL.MblobSelect(milBlobResult, MIL.M_EXCLUDE, MIL.M_FERET_Y, MIL.M_LESS, blobparam.MIN_FERET_Y.Value, MIL.M_NULL);
                    }

                    if (blobparam.MAX_FERET_Y.Value != 0)
                    {
                        MIL.MblobSelect(milBlobResult, MIL.M_EXCLUDE, MIL.M_FERET_Y, MIL.M_GREATER, blobparam.MAX_FERET_Y.Value, MIL.M_NULL);
                    }
                }
            }
            catch (MILException err)
            {
                throw new VisionException(err.Message, err, EventCodeEnum.VISION_BLOB_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }
            finally
            {
                if (milBlobFeatureList != MIL.M_NULL)
                {
                    MIL.MblobFree(milBlobFeatureList);
                }
            }

            return milBlobResult;
        }

        public ImageBuffer GetPMIResultImage(List<PMIPadObject> PadList, ImageBuffer SrcBuf, ref bool IsPass)
        {
            ImageBuffer retval = null;

            MIL_ID milProcResultBuffer = MIL.M_NULL;
            int drawingColor = MIL.M_COLOR_RED;

            try
            {
                if (milProcResultBuffer == MIL.M_NULL)
                {
                    BufAllocColorUsingImageBuffer(SrcBuf, ref milProcResultBuffer, true);
                }

                // PadList에는 몇 개의 패드가 속할지 모름.
                // 단 하나의 패드라도 Pass 또는 Fail이 존재하면 각 파라미터에 맞게 Triggered
                foreach (var pad in PadList)
                {
                    PMIPadResult lastresult = pad.PMIResults.Last();

                    if (lastresult != null)
                    {
                        // 개수가 1개이고, PASS를 갖고 있는 경우 => PASS
                        // 레퍼런스 이미지가 필요한 조건이였음에도 PASS. 인 경우 => PASS 
                        if ((lastresult.PadStatus.Count == 1 && lastresult.PadStatus[0] == PadStatusCodeEnum.PASS) ||
                            (lastresult.PadStatus.Count == 2 && lastresult.PadStatus[0] == PadStatusCodeEnum.NEED_REFERENCE_IMAGE && lastresult.PadStatus[1] == PadStatusCodeEnum.PASS))
                        {
                            drawingColor = MIL.M_COLOR_GREEN;
                            IsPass = true;
                        }
                        else
                        {
                            drawingColor = MIL.M_COLOR_RED;
                            IsPass = false;
                        }

                        // Drawing Pad 
                        MIL.MgraColor(MIL.M_DEFAULT, drawingColor);

                        double BOX_X_MIN;
                        double BOX_X_MAX;
                        double BOX_Y_MIN;
                        double BOX_Y_MAX;

                        if (lastresult.IsSuccessDetectedPad == true)
                        {
                            BOX_X_MIN = lastresult.PadPosition.Left;
                            BOX_X_MAX = lastresult.PadPosition.Right;

                            BOX_Y_MIN = lastresult.PadPosition.Top;
                            BOX_Y_MAX = lastresult.PadPosition.Bottom;
                        }
                        else
                        {
                            BOX_X_MIN = pad.BoundingBox.Left;
                            BOX_X_MAX = pad.BoundingBox.Right;

                            BOX_Y_MIN = pad.BoundingBox.Top;
                            BOX_Y_MAX = pad.BoundingBox.Bottom;
                        }

                        MIL.MgraRect(MIL.M_DEFAULT, milProcResultBuffer, BOX_X_MIN, BOX_Y_MIN, BOX_X_MAX, BOX_Y_MAX);

                        // Drawing Marks
                        if (lastresult.MarkResults.Count > 0)
                        {
                            bool isFaild = false;

                            foreach (var mark in lastresult.MarkResults)
                            {
                                isFaild = mark.Status.ToList().Any(x => x != MarkStatusCodeEnum.PASS);

                                if (isFaild == true)
                                {
                                    drawingColor = MIL.M_COLOR_RED;
                                }
                                else
                                {
                                    drawingColor = MIL.M_COLOR_GREEN;
                                }

                                MIL.MgraColor(MIL.M_DEFAULT, drawingColor);

                                double XStart = 0;
                                double YStart = 0;
                                double XEnd = 0;
                                double YEnd = 0;

                                if (lastresult.IsSuccessDetectedPad == true)
                                {
                                    XStart = lastresult.PadPosition.Left + mark.MarkPosPixel.Left;
                                    YStart = lastresult.PadPosition.Top + mark.MarkPosPixel.Top;
                                    XEnd = XStart + mark.MarkPosPixel.Width;
                                    YEnd = YStart + mark.MarkPosPixel.Height;
                                }
                                else
                                {
                                    XStart = pad.BoundingBox.Left + mark.MarkPosPixel.Left;
                                    YStart = pad.BoundingBox.Top + mark.MarkPosPixel.Top;
                                    XEnd = XStart + mark.MarkPosPixel.Width;
                                    YEnd = YStart + mark.MarkPosPixel.Height;
                                }

                                MIL.MgraRect(MIL.M_DEFAULT, milProcResultBuffer, XStart, YStart, XEnd, YEnd);
                            }
                        }
                    }
                }

                byte[] resultBuffer;
                resultBuffer = new byte[SrcBuf.SizeX * SrcBuf.SizeY * 3];
                MIL.MbufGetColor(milProcResultBuffer, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BAND, resultBuffer);

                retval = new ImageBuffer(resultBuffer, SrcBuf.SizeX, SrcBuf.SizeY, SrcBuf.Band, (int)ColorDept.Color24);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (milProcResultBuffer != null)
                {
                    MIL.MbufFree(milProcResultBuffer);
                    milProcResultBuffer = MIL.M_NULL;
                }
            }

            return retval;
        }

        public EventCodeEnum FindMarks(List<PMIPadObject> PadList,
                                            ImageBuffer SrcBuf,
                                            ImageBuffer OrginalBuf,
                                            ref ImageBuffer ResultBuf,
                                            PMIImageCombineMode CombindMdoe,
                                            MARK_AREA_CALCULATE_MODE MarkAreaCalculateMode,
                                            double OverlapPercenterToleranceX = 0,
                                            double OverlapPercenterToleranceY = 0,
                                            BlobParameter blobparam = null,
                                            ROIParameter roiparam = null,
                                            bool UseAllFeatures = false,
                                            bool foregroundIsZero = false,
                                            bool IsDrawingPadForOverlay = false,
                                            bool IsDrawingMarkForOverlay = false)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            MIL_ID milBlobResult = MIL.M_NULL;
            MIL_ID milProcResultBuffer = MIL.M_NULL;

            ResultBuf = null;

            try
            {
                milBlobResult = GetBlobResult(SrcBuf, blobparam, roiparam, UseAllFeatures, foregroundIsZero);

                MIL_INT mIntTotalBlobs = 0;

                double FERET_X_MAX = 0;
                double FERET_Y_MAX = 0;

                double FERET_X_MIN = 0;
                double FERET_Y_MIN = 0;

                double MIN_AREA = 0;

                FERET_X_MAX = PadList.Max(x => x.BoundingBox.Width);
                FERET_Y_MAX = PadList.Max(y => y.BoundingBox.Height);

                if (blobparam != null)
                {
                    if (blobparam.MIN_FERET_X.Value != 0)
                    {
                        FERET_X_MIN = blobparam.MIN_FERET_X.Value;
                    }

                    if (blobparam.MIN_FERET_Y.Value != 0)
                    {
                        FERET_Y_MIN = blobparam.MIN_FERET_Y.Value;
                    }

                    if (blobparam.MinBlobArea.Value != 0)
                    {
                        MIN_AREA = blobparam.MinBlobArea.Value;
                    }
                }

                List<int> AllowIndexList = new List<int>();

                foreach (var pad in PadList)
                {
                    MIL.MblobSelect(milBlobResult, MIL.M_INCLUDE, MIL.M_ALL_BLOBS, MIL.M_NULL, MIL.M_NULL, MIL.M_NULL);

                    // Pad Specific Filter

                    double BOX_X_MIN;
                    double BOX_X_MAX;
                    double BOX_Y_MIN;
                    double BOX_Y_MAX;

                    var PadCurrentResult = pad.PMIResults[pad.PMIResults.Count - 1];

                    if (PadCurrentResult.IsSuccessDetectedPad == true)
                    {
                        BOX_X_MIN = PadCurrentResult.PadPosition.Left;
                        BOX_X_MAX = PadCurrentResult.PadPosition.Right;

                        BOX_Y_MIN = PadCurrentResult.PadPosition.Top;
                        BOX_Y_MAX = PadCurrentResult.PadPosition.Bottom;
                    }
                    else
                    {
                        BOX_X_MIN = pad.BoundingBox.Left;
                        BOX_X_MAX = pad.BoundingBox.Right;

                        BOX_Y_MIN = pad.BoundingBox.Top;
                        BOX_Y_MAX = pad.BoundingBox.Bottom;
                    }

                    // Size (Big)
                    MIL.MblobSelect(milBlobResult, MIL.M_DELETE, MIL.M_FERET_X, MIL.M_GREATER, FERET_X_MAX * 0.9, MIL.M_NULL);
                    MIL.MblobSelect(milBlobResult, MIL.M_DELETE, MIL.M_FERET_Y, MIL.M_GREATER, FERET_Y_MAX * 0.9, MIL.M_NULL);

                    // Size (Small)
                    MIL.MblobSelect(milBlobResult, MIL.M_DELETE, MIL.M_FERET_X, MIL.M_LESS_OR_EQUAL, FERET_X_MIN, MIL.M_NULL);
                    MIL.MblobSelect(milBlobResult, MIL.M_DELETE, MIL.M_FERET_Y, MIL.M_LESS_OR_EQUAL, FERET_Y_MIN, MIL.M_NULL);

                    // Area (Small)
                    MIL.MblobSelect(milBlobResult, MIL.M_DELETE, MIL.M_AREA, MIL.M_LESS, MIN_AREA, MIL.M_NULL);

                    MIL.MblobSelect(milBlobResult, MIL.M_EXCLUDE, MIL.M_BOX_X_MIN, MIL.M_LESS, BOX_X_MIN, MIL.M_NULL);
                    MIL.MblobSelect(milBlobResult, MIL.M_EXCLUDE, MIL.M_BOX_X_MAX, MIL.M_GREATER, BOX_X_MAX, MIL.M_NULL);

                    MIL.MblobSelect(milBlobResult, MIL.M_EXCLUDE, MIL.M_BOX_Y_MIN, MIL.M_LESS, BOX_Y_MIN, MIL.M_NULL);
                    MIL.MblobSelect(milBlobResult, MIL.M_EXCLUDE, MIL.M_BOX_Y_MAX, MIL.M_GREATER, BOX_Y_MAX, MIL.M_NULL);

                    MIL.MblobGetNumber(milBlobResult, ref mIntTotalBlobs);

                    if (mIntTotalBlobs > 0)
                    {
                        double[] dblCOGX = new double[mIntTotalBlobs];
                        double[] dblCOGY = new double[mIntTotalBlobs];

                        double[] offset = new double[2] { 0, 0 };

                        double[] dblBlobArea = new double[mIntTotalBlobs];
                        double[] dblAreainum = new double[mIntTotalBlobs];

                        double[] dblSizeX = new double[mIntTotalBlobs];
                        double[] dblSizeY = new double[mIntTotalBlobs];

                        double[] dblBox_X_Max = new double[mIntTotalBlobs];
                        double[] dblBox_X_Min = new double[mIntTotalBlobs];
                        double[] dblBox_Y_Max = new double[mIntTotalBlobs];
                        double[] dblBox_Y_Min = new double[mIntTotalBlobs];

                        double[] dbl_feret_max_diameter = new double[mIntTotalBlobs];
                        double[] dbl_feret_mean_diameter = new double[mIntTotalBlobs];
                        double[] dbl_feret_min_diameter = new double[mIntTotalBlobs];

                        double[] dblCovexHull_Area = new double[mIntTotalBlobs];
                        double[] dblCovexHull_CogX = new double[mIntTotalBlobs];
                        double[] dblCovexHull_CogY = new double[mIntTotalBlobs];
                        double[] dblCovexHull_Fill_Ratio = new double[mIntTotalBlobs];
                        double[] dblCovexHull_Perimeter = new double[mIntTotalBlobs];

                        double[] dblLabel_Index = new double[mIntTotalBlobs];

                        MIL.MblobGetResult(milBlobResult, MIL.M_CONVEX_HULL_AREA, dblCovexHull_Area);
                        MIL.MblobGetResult(milBlobResult, MIL.M_CONVEX_HULL_COG_X, dblCovexHull_CogX);
                        MIL.MblobGetResult(milBlobResult, MIL.M_CONVEX_HULL_COG_Y, dblCovexHull_CogY);
                        MIL.MblobGetResult(milBlobResult, MIL.M_CONVEX_HULL_FILL_RATIO, dblCovexHull_Fill_Ratio);
                        MIL.MblobGetResult(milBlobResult, MIL.M_CONVEX_PERIMETER, dblCovexHull_Perimeter);

                        MIL.MblobGetResult(milBlobResult, MIL.M_FERET_MAX_DIAMETER, dbl_feret_max_diameter);
                        MIL.MblobGetResult(milBlobResult, MIL.M_FERET_MEAN_DIAMETER, dbl_feret_mean_diameter);
                        MIL.MblobGetResult(milBlobResult, MIL.M_FERET_MIN_DIAMETER, dbl_feret_min_diameter);

                        MIL.MblobGetResult(milBlobResult, MIL.M_FERET_X, dblSizeX);
                        MIL.MblobGetResult(milBlobResult, MIL.M_FERET_Y, dblSizeY);

                        MIL.MblobGetResult(milBlobResult, MIL.M_BOX_X_MAX, dblBox_X_Max);
                        MIL.MblobGetResult(milBlobResult, MIL.M_BOX_X_MIN, dblBox_X_Min);
                        MIL.MblobGetResult(milBlobResult, MIL.M_BOX_Y_MAX, dblBox_Y_Max);
                        MIL.MblobGetResult(milBlobResult, MIL.M_BOX_Y_MIN, dblBox_Y_Min);

                        MIL.MblobGetResult(milBlobResult, MIL.M_CENTER_OF_GRAVITY_X, dblCOGX);
                        MIL.MblobGetResult(milBlobResult, MIL.M_CENTER_OF_GRAVITY_Y, dblCOGY);

                        MIL.MblobGetResult(milBlobResult, MIL.M_AREA, dblBlobArea);

                        MIL.MblobGetResult(milBlobResult, MIL.M_LABEL_VALUE, dblLabel_Index);

                        double PadArea = pad.BoundingBox.Width * pad.BoundingBox.Height;

                        bool IsAllow = false;

                        // 길이가 10배 이상 차이나는 경우, 제외하기 위해 사용되는 변수
                        int SizeAllowMaxRatio = 10;

                        for (int index = 0; index < mIntTotalBlobs; index++)
                        {
                            IsAllow = true;

                            PMIMarkResult mark = new PMIMarkResult();

                            Point LeftTop = new Point(dblBox_X_Min[index] - BOX_X_MIN, dblBox_Y_Min[index] - BOX_Y_MIN);
                            Point RightBottom = new Point(dblBox_X_Max[index] - BOX_X_MIN, dblBox_Y_Max[index] - BOX_Y_MIN);

                            mark.Width = dblSizeX[index];
                            mark.Height = dblSizeY[index];

                            mark.MarkPosPixel = new Rect(LeftTop, RightBottom);

                            //Blob의 면적
                            if (MarkAreaCalculateMode == MARK_AREA_CALCULATE_MODE.Convex)
                            { //완만한 테두리 형태로 설정한 면적
                                mark.ScrubAreaPx = dblCovexHull_Area[index];
                            }
                            else
                            { //필터 적용없이 실제 면적.
                                mark.ScrubAreaPx = dblBlobArea[index];
                            }
                            if (dblSizeX[index] > dblSizeY[index])
                            {
                                if (dblSizeX[index] > (dblSizeY[index] * SizeAllowMaxRatio))
                                {
                                    IsAllow = false;
                                }
                            }
                            else if (dblSizeY[index] > dblSizeX[index])
                            {
                                if (dblSizeY[index] > (dblSizeX[index] * SizeAllowMaxRatio))
                                {
                                    IsAllow = false;
                                }
                            }

                            if (IsAllow == true)
                            {
                                int label_index = Convert.ToInt32(dblLabel_Index[index]);

                                AllowIndexList.Add(label_index);

                                // 이후, Overlap에서 배제될 때, 사용하기 위해 필요.
                                mark.Index = label_index;

                                PadCurrentResult.MarkResults.Add(mark);

                                if (blobparam != null)
                                {
                                    if (blobparam.BlobLimitedCount.Value > 0)
                                    {
                                        if (AllowIndexList.Count >= blobparam.BlobLimitedCount.Value)
                                        {
                                            // 이후의 Mark는 사용하지 않음.
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        // PadCurrentResult.MarkResults 중, Overlap 조건을 통해, 배제할 마크 추가 확인

                        // 파라미터가 설정되어 있는 경우
                        if (OverlapPercenterToleranceX > 0 || OverlapPercenterToleranceY > 0)
                        {
                            int IsAllowedMarkCount = PadCurrentResult.MarkResults.Count;

                            // 마크가 2개 이상 검출되었을 때
                            if (IsAllowedMarkCount >= 2)
                            {
                                // Area 오름차순으로 정렬
                                List<PMIMarkResult> AllowedMarkOrderedList = PadCurrentResult.MarkResults.OrderBy(o => o.MarkPosPixel.Width * o.MarkPosPixel.Height).ToList();

                                for (int s = 0; s < IsAllowedMarkCount; s++)
                                {
                                    var SourceMark = AllowedMarkOrderedList[s].MarkPosPixel;

                                    for (int t = s + 1; t < IsAllowedMarkCount; t++)
                                    {
                                        var TargetMark = AllowedMarkOrderedList[t].MarkPosPixel;

                                        // Is Intersect?
                                        if (SourceMark.IntersectsWith(TargetMark))
                                        {
                                            Rect intersectArea = Rect.Intersect(SourceMark, TargetMark);

                                            double OverlapWidthPercent = intersectArea.Width * 100 / SourceMark.Width;
                                            double OverlapHeightPercent = intersectArea.Height * 100 / SourceMark.Height;

                                            // TODO; 파라미터로 변경.
                                            if (OverlapWidthPercent > OverlapPercenterToleranceX ||
                                                OverlapHeightPercent > OverlapPercenterToleranceY)
                                            {
                                                // EXCLUDE

                                                int label_index = AllowedMarkOrderedList[s].Index;

                                                // TODO : 원하는 오브젝트 Remove 되는지 확인 필요.
                                                PadCurrentResult.MarkResults.Remove(AllowedMarkOrderedList[s]);
                                                AllowIndexList.Remove(label_index);

                                                LoggerManager.Debug($"[VisionAlgorithmes], FindMarks() : Removed Mark by overlap parameter. OverlapWidthPercent X = {OverlapWidthPercent}, OverlapWidthPercent Y = {OverlapHeightPercent}, Tolerance X = {OverlapPercenterToleranceX}, Y = {OverlapPercenterToleranceY}");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (CombindMdoe == PMIImageCombineMode.BINARY)
                {
                    BufAllocColorUsingImageBuffer(SrcBuf, ref milProcResultBuffer, true);
                }
                else
                {
                    BufAllocColorUsingImageBuffer(OrginalBuf, ref milProcResultBuffer, true);
                }

                MIL.MblobSelect(milBlobResult, MIL.M_INCLUDE, MIL.M_ALL_BLOBS, MIL.M_NULL, MIL.M_NULL, MIL.M_NULL);
                MIL.MblobGetNumber(milBlobResult, ref mIntTotalBlobs);

                // Drawing Pad 

                if (IsDrawingPadForOverlay == true)
                {
                    MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_RED);

                    foreach (var pad in PadList)
                    {
                        double BOX_X_MIN;
                        double BOX_X_MAX;
                        double BOX_Y_MIN;
                        double BOX_Y_MAX;

                        var PadCurrentResult = pad.PMIResults[pad.PMIResults.Count - 1];

                        if (PadCurrentResult.IsSuccessDetectedPad == true)
                        {
                            BOX_X_MIN = PadCurrentResult.PadPosition.Left;
                            BOX_X_MAX = PadCurrentResult.PadPosition.Right;

                            BOX_Y_MIN = PadCurrentResult.PadPosition.Top;
                            BOX_Y_MAX = PadCurrentResult.PadPosition.Bottom;
                        }
                        else
                        {
                            BOX_X_MIN = pad.BoundingBox.Left;
                            BOX_X_MAX = pad.BoundingBox.Right;

                            BOX_Y_MIN = pad.BoundingBox.Top;
                            BOX_Y_MAX = pad.BoundingBox.Bottom;
                        }

                        MIL.MgraRect(MIL.M_DEFAULT, milProcResultBuffer, BOX_X_MIN, BOX_Y_MIN, BOX_X_MAX, BOX_Y_MAX);
                    }
                }

                if (IsDrawingMarkForOverlay == true)
                {
                    if (mIntTotalBlobs > 0)
                    {
                        // Allow된 Blob 데이터만 그릴 수 있도록 

                        MIL.MblobSelect(milBlobResult, MIL.M_EXCLUDE, MIL.M_ALL_BLOBS, MIL.M_NULL, MIL.M_NULL, MIL.M_NULL);

                        foreach (var blob in AllowIndexList)
                        {
                            MIL.MblobSelect(milBlobResult, MIL.M_INCLUDE, MIL.M_LABEL_VALUE, MIL.M_EQUAL, blob, blob);
                        }

                        MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_GREEN);
                        MIL.MblobDraw(MIL.M_DEFAULT, milBlobResult, milProcResultBuffer, MIL.M_DRAW_BLOBS_CONTOUR, MIL.M_INCLUDED_BLOBS, MIL.M_DEFAULT);

                        //bool IsTest = true;

                        //if (IsTest)
                        //{
                        //    int bcount = 0;

                        //    uint[] colors = new uint[19];

                        //    colors[0] = MIL.M_COLOR_RED;
                        //    colors[1] = MIL.M_COLOR_GREEN;
                        //    colors[2] = MIL.M_COLOR_BLUE;
                        //    colors[3] = MIL.M_COLOR_MAGENTA;
                        //    colors[4] = MIL.M_COLOR_CYAN;
                        //    colors[5] = MIL.M_COLOR_YELLOW;
                        //    colors[6] = MIL.M_COLOR_GRAY;
                        //    colors[7] = MIL.M_COLOR_DARK_RED;
                        //    colors[8] = MIL.M_COLOR_DARK_GREEN;
                        //    colors[9] = MIL.M_COLOR_DARK_BLUE;
                        //    colors[10] = MIL.M_COLOR_DARK_CYAN;
                        //    colors[11] = MIL.M_COLOR_DARK_MAGENTA;
                        //    colors[12] = MIL.M_COLOR_DARK_YELLOW;
                        //    colors[13] = MIL.M_COLOR_LIGHT_WHITE;
                        //    colors[14] = MIL.M_COLOR_WHITE;
                        //    colors[15] = MIL.M_COLOR_LIGHT_GRAY;
                        //    colors[16] = MIL.M_COLOR_BRIGHT_GRAY;
                        //    colors[17] = MIL.M_COLOR_LIGHT_GREEN;
                        //    colors[18] = MIL.M_COLOR_LIGHT_BLUE;

                        //    foreach (var blob in AllowIndexList)
                        //    {
                        //        double compactness = 0;
                        //        double area = 0;
                        //        double convex_hull_area = 0;
                        //        double sizex = 0;
                        //        double sizey = 0;

                        //        double ChainNumber = 0;

                        //        MIL.MblobGetResultSingle(milBlobResult, blob, MIL.M_COMPACTNESS, ref compactness);
                        //        MIL.MblobGetResultSingle(milBlobResult, blob, MIL.M_AREA, ref area);
                        //        MIL.MblobGetResultSingle(milBlobResult, blob, MIL.M_CONVEX_HULL_AREA, ref convex_hull_area);
                        //        MIL.MblobGetResultSingle(milBlobResult, blob, MIL.M_FERET_X, ref sizex);
                        //        MIL.MblobGetResultSingle(milBlobResult, blob, MIL.M_FERET_Y, ref sizey);
                        //        MIL.MblobGetResultSingle(milBlobResult, blob, MIL.M_NUMBER_OF_CHAINED_PIXELS, ref ChainNumber);

                        //        MIL.MgraColor(MIL.M_DEFAULT, colors[bcount]);

                        //        MIL.MblobDraw(MIL.M_DEFAULT, milBlobResult, milProcResultBuffer, MIL.M_DRAW_BOX, blob, MIL.M_DEFAULT);
                        //        MIL.MblobDraw(MIL.M_DEFAULT, milBlobResult, milProcResultBuffer, MIL.M_DRAW_BLOBS_CONTOUR, blob, MIL.M_DEFAULT);

                        //        string txt = $"Compactness : {compactness:f1}, Area : {area:f1}, Convex Area : {convex_hull_area:f1}, X : {sizex:f1}, Y : {sizey:f1}, ChainNumber : {ChainNumber:f1}, Ratio : {area * 100 / (sizex * sizey):f1}";

                        //        // 텍스트를 그릴 위치 계산
                        //        int textYPosition = 20 + (15 * bcount);  // 20은 상단 여백, 15는 텍스트 라인 높이

                        //        // 텍스트 출력
                        //        MIL.MgraText(MIL.M_DEFAULT, milProcResultBuffer, 5, textYPosition, txt);

                        //        bcount++;
                        //    }
                        //}
                        //else
                        //{
                        //    MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_GREEN);
                        //    MIL.MblobDraw(MIL.M_DEFAULT, milBlobResult, milProcResultBuffer, MIL.M_DRAW_BLOBS_CONTOUR, MIL.M_INCLUDED_BLOBS, MIL.M_DEFAULT);
                        //}
                    }
                }

                //var curDate = DateTime.Now.ToString("yyMMddHHmmss");
                //MIL.MbufExport($@"C:\ProberSystem\PMITest\DetetedMarkImage.bmp", MIL.M_BMP, milProcResultBuffer);

                byte[] resultBuffer;

                resultBuffer = new byte[SrcBuf.SizeX * SrcBuf.SizeY * 3];

                MIL.MbufGetColor(milProcResultBuffer, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BAND, resultBuffer);

                ResultBuf = new ImageBuffer(resultBuffer, SrcBuf.SizeX, SrcBuf.SizeY, SrcBuf.Band, (int)ColorDept.Color24);
            }
            catch (Exception err)
            {
                retval = EventCodeEnum.UNDEFINED;

                LoggerManager.Exception(err);
            }
            finally
            {
                if (milBlobResult != null)
                {
                    MIL.MblobFree(milBlobResult);
                    milBlobResult = MIL.M_NULL;
                }

                if (milProcResultBuffer != null)
                {
                    MIL.MbufFree(milProcResultBuffer);
                    milProcResultBuffer = MIL.M_NULL;
                }

                retval = EventCodeEnum.NONE;
            }

            return retval;
        }

        /// <summary>
        /// SrcBuf를 통해 전달되는 이미지에 설정된 크기의 패드영역이 있는지 확인. 
        /// </summary>
        /// <param name="PadList"></param>
        /// <param name="MarginPixel"></param>
        /// <param name="SrcBuf"></param>
        /// <param name="blobparam"></param>
        /// <param name="roiparam"></param>
        /// <param name="UseAllFeatures"></param>
        /// <param name="foregroundIsZero"></param>
        /// <param name="drawing"></param>
        /// <returns>NONE = SUCCESS, others = FAIL </returns>
        public EventCodeEnum FindPMIPads(List<PMIPadObject> PadList,
                                            Point MarginPixel,
                                            ImageBuffer SrcBuf,
                                            ImageBuffer SrcBuf_Original,
                                            BlobParameter blobparam = null,
                                            ROIParameter roiparam = null,
                                            bool UseAllFeatures = false,
                                            bool foregroundIsZero = false)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            MIL_ID milBlobResult = MIL.M_NULL;

            try
            {
                MIL_INT mIntTotalBlobs = 0;
                MIL_INT mIntTotalBlobs2 = 0;

                milBlobResult = GetBlobResult(SrcBuf, blobparam, roiparam, UseAllFeatures, foregroundIsZero);

                MIL.MblobGetNumber(milBlobResult, ref mIntTotalBlobs);

                if (mIntTotalBlobs > 0)
                {
                    foreach (var pad in PadList)
                    {
                        double BOX_X_MIN_POS;
                        double BOX_X_MAX_POS;

                        double BOX_Y_MIN_POS;
                        double BOX_Y_MAX_POS;

                        BOX_X_MIN_POS = (pad.BoundingBox.Left - MarginPixel.X);
                        BOX_X_MAX_POS = (pad.BoundingBox.Right + MarginPixel.X);

                        BOX_Y_MIN_POS = (pad.BoundingBox.Top - MarginPixel.Y);
                        BOX_Y_MAX_POS = (pad.BoundingBox.Bottom + MarginPixel.Y);

                        double padCenOrgX = (pad.BoundingBox.Left + pad.BoundingBox.Right) / 2.0;
                        double padCenOrgY = (pad.BoundingBox.Top + pad.BoundingBox.Bottom) / 2.0;

                        MIL.MblobSelect(milBlobResult, MIL.M_INCLUDE, MIL.M_ALL_BLOBS, MIL.M_NULL, MIL.M_NULL, MIL.M_NULL);

                        MIL.MblobSelect(milBlobResult, MIL.M_EXCLUDE, MIL.M_BOX_X_MIN, MIL.M_LESS, BOX_X_MIN_POS, MIL.M_NULL);
                        MIL.MblobSelect(milBlobResult, MIL.M_EXCLUDE, MIL.M_BOX_X_MAX, MIL.M_GREATER, BOX_X_MAX_POS, MIL.M_NULL);
                        MIL.MblobSelect(milBlobResult, MIL.M_EXCLUDE, MIL.M_BOX_Y_MIN, MIL.M_LESS, BOX_Y_MIN_POS, MIL.M_NULL);
                        MIL.MblobSelect(milBlobResult, MIL.M_EXCLUDE, MIL.M_BOX_Y_MAX, MIL.M_GREATER, BOX_Y_MAX_POS, MIL.M_NULL);

                        var PadCurrentResult = pad.PMIResults[pad.PMIResults.Count - 1];

                        MIL.MblobGetNumber(milBlobResult, ref mIntTotalBlobs2);

                        if (mIntTotalBlobs2 > 0)
                        {
                            double Found_X_Min = 0;
                            double Found_X_Max = 0;

                            double Found_Y_Min = 0;
                            double Found_Y_Max = 0;

                            double[] dblCOGX = new double[mIntTotalBlobs2];
                            double[] dblCOGY = new double[mIntTotalBlobs2];

                            double[] offset = new double[2] { 0, 0 };

                            double[] dblBlobArea = new double[mIntTotalBlobs2];
                            double[] dblAreainum = new double[mIntTotalBlobs2];

                            double[] dblSizeX = new double[mIntTotalBlobs2];
                            double[] dblSizeY = new double[mIntTotalBlobs2];

                            double[] dblBox_X_Max = new double[mIntTotalBlobs2];
                            double[] dblBox_X_Min = new double[mIntTotalBlobs2];
                            double[] dblBox_Y_Max = new double[mIntTotalBlobs2];
                            double[] dblBox_Y_Min = new double[mIntTotalBlobs2];

                            double[] dblLabel_Index = new double[mIntTotalBlobs2];

                            MIL.MblobGetResult(milBlobResult, MIL.M_FERET_X, dblSizeX);
                            MIL.MblobGetResult(milBlobResult, MIL.M_FERET_Y, dblSizeY);

                            MIL.MblobGetResult(milBlobResult, MIL.M_BOX_X_MAX, dblBox_X_Max);
                            MIL.MblobGetResult(milBlobResult, MIL.M_BOX_X_MIN, dblBox_X_Min);
                            MIL.MblobGetResult(milBlobResult, MIL.M_BOX_Y_MAX, dblBox_Y_Max);
                            MIL.MblobGetResult(milBlobResult, MIL.M_BOX_Y_MIN, dblBox_Y_Min);

                            MIL.MblobGetResult(milBlobResult, MIL.M_CENTER_OF_GRAVITY_X, dblCOGX);
                            MIL.MblobGetResult(milBlobResult, MIL.M_CENTER_OF_GRAVITY_Y, dblCOGY);

                            MIL.MblobGetResult(milBlobResult, MIL.M_AREA, dblBlobArea);

                            MIL.MblobGetResult(milBlobResult, MIL.M_LABEL_VALUE, dblLabel_Index);

                            double PadArea = pad.BoundingBox.Width * pad.BoundingBox.Height;

                            bool isFoundPad = false;

                            for (int index = 0; index < mIntTotalBlobs2; index++)
                            {
                                // (1). Area 비교 : // 알고 있는 패드 사이즈의 70%에 해당하는 데이터의 경우, 높은 확률로 찾고자 하는 패드에 해당하는 데이터일 수 있다.
                                if (dblBlobArea[index] >= PadArea * 0.7)
                                {
                                    // (2). Size 비교 : 
                                    if ((Math.Abs(dblSizeX[index] - pad.BoundingBox.Width) < 10) &&
                                       (Math.Abs(dblSizeY[index] - pad.BoundingBox.Height) < 10))
                                    {
                                        Found_X_Min = dblBox_X_Min[index];
                                        Found_X_Max = dblBox_X_Max[index];

                                        Found_Y_Min = dblBox_Y_Min[index];
                                        Found_Y_Max = dblBox_Y_Max[index];

                                        isFoundPad = true;
                                    }
                                    else
                                    {
                                        // 패드에 해당하는 영역 외, 패드 주변의 픽셀들이 함께 포함되어 데이터가 생겼을 수 있다. 
                                        // 해당 블랍의 데이터를 기반으로 Threshold를 재계산하여 패드를 찾을 수 있도록 한다.

                                        MIL_ID milTestBuffer = MIL.M_NULL;
                                        MIL_ID milTestBuffer2 = MIL.M_NULL;
                                        MIL_ID milTestBuffer3 = MIL.M_NULL;
                                        MIL_ID binaryImg = MIL.M_NULL;

                                        MIL_ID milBlobResult2 = MIL.M_NULL;

                                        MIL_INT mIntTotalBlobs3 = 0;

                                        try
                                        {
                                            BufAlloc2DUsingImageBuffer(SrcBuf_Original, ref milTestBuffer, true);
                                            BufAlloc2DUsingImageBuffer(SrcBuf_Original, ref milTestBuffer2, false);
                                            BufAlloc2DUsingImageBuffer(SrcBuf_Original, ref milTestBuffer3, false);

                                            MIL_INT labelindex = (MIL_INT)dblLabel_Index[index];

                                            MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_WHITE);
                                            MIL.MblobDraw(MIL.M_DEFAULT, milBlobResult, milTestBuffer2, MIL.M_DRAW_BLOBS, labelindex, MIL.M_DEFAULT);

                                            MIL.MimArith(milTestBuffer, milTestBuffer2, milTestBuffer3, MIL.M_AND);

                                            var thre = GetThreshold_ForPadDetection(milTestBuffer3, true);

                                            binaryImg = Binarization(milTestBuffer3, thre);

                                            milBlobResult2 = GetBlobResult(binaryImg);

                                            MIL.MblobGetNumber(milBlobResult2, ref mIntTotalBlobs3);

                                            if (mIntTotalBlobs3 == 1)
                                            {
                                                double[] dblBlobArea2 = new double[mIntTotalBlobs3];

                                                double[] dblSizeX2 = new double[mIntTotalBlobs3];
                                                double[] dblSizeY2 = new double[mIntTotalBlobs3];

                                                double[] dblBox_X_Max2 = new double[mIntTotalBlobs3];
                                                double[] dblBox_X_Min2 = new double[mIntTotalBlobs3];
                                                double[] dblBox_Y_Max2 = new double[mIntTotalBlobs3];
                                                double[] dblBox_Y_Min2 = new double[mIntTotalBlobs3];

                                                MIL.MblobGetResult(milBlobResult2, MIL.M_FERET_X, dblSizeX2);
                                                MIL.MblobGetResult(milBlobResult2, MIL.M_FERET_Y, dblSizeY2);

                                                MIL.MblobGetResult(milBlobResult2, MIL.M_BOX_X_MAX, dblBox_X_Max2);
                                                MIL.MblobGetResult(milBlobResult2, MIL.M_BOX_X_MIN, dblBox_X_Min2);
                                                MIL.MblobGetResult(milBlobResult2, MIL.M_BOX_Y_MAX, dblBox_Y_Max2);
                                                MIL.MblobGetResult(milBlobResult2, MIL.M_BOX_Y_MIN, dblBox_Y_Min2);

                                                MIL.MblobGetResult(milBlobResult2, MIL.M_AREA, dblBlobArea2);

                                                if ((Math.Abs(dblSizeX2[0] - pad.BoundingBox.Width) < 10) &&
                                                    (Math.Abs(dblSizeY2[0] - pad.BoundingBox.Height) < 10))
                                                {
                                                    Found_X_Min = dblBox_X_Min2[index];
                                                    Found_X_Max = dblBox_X_Max2[index];

                                                    Found_Y_Min = dblBox_Y_Min2[index];
                                                    Found_Y_Max = dblBox_Y_Max2[index];

                                                    isFoundPad = true;
                                                }
                                            }
                                            else
                                            {
                                                // Unknown
                                                isFoundPad = false;
                                            }

                                        }
                                        catch (Exception err)
                                        {
                                            retval = EventCodeEnum.EXCEPTION;
                                            LoggerManager.Exception(err);
                                        }
                                        finally
                                        {
                                            if (milBlobResult2 != null)
                                            {
                                                MIL.MblobFree(milBlobResult2);
                                                milBlobResult2 = MIL.M_NULL;
                                            }

                                            if (milTestBuffer != null)
                                            {
                                                MIL.MbufFree(milTestBuffer);
                                                milTestBuffer = MIL.M_NULL;
                                            }

                                            if (milTestBuffer2 != null)
                                            {
                                                MIL.MbufFree(milTestBuffer2);
                                                milTestBuffer2 = MIL.M_NULL;
                                            }

                                            if (milTestBuffer3 != null)
                                            {
                                                MIL.MbufFree(milTestBuffer3);
                                                milTestBuffer3 = MIL.M_NULL;
                                            }

                                            if (binaryImg != null)
                                            {
                                                MIL.MbufFree(binaryImg);
                                                binaryImg = MIL.M_NULL;
                                            }
                                        }
                                    }
                                }

                                if (isFoundPad)
                                {
                                    Point LeftTop = new Point(Found_X_Min, Found_Y_Min);
                                    Point RightBottom = new Point(Found_X_Max, Found_Y_Max);

                                    PadCurrentResult.PadPosition = new Rect(LeftTop, RightBottom);

                                    double padCenFoundX = (LeftTop.X + RightBottom.X) / 2.0;
                                    double padCenFoundY = (LeftTop.Y + RightBottom.Y) / 2.0;

                                    PadCurrentResult.PadOffsetX = padCenFoundX - padCenOrgX;
                                    PadCurrentResult.PadOffsetY = padCenFoundY - padCenOrgY;

                                    PadCurrentResult.IsSuccessDetectedPad = true;

                                    break;
                                }
                            }

                            if (PadCurrentResult.IsSuccessDetectedPad != true)
                            {
                                retval = EventCodeEnum.PMI_NOT_FOUND_PAD;
                            }
                        }
                        else
                        {
                            PadCurrentResult.IsSuccessDetectedPad = false;

                            retval = EventCodeEnum.PMI_NOT_FOUND_PAD;
                        }
                    }
                }
                else
                {
                    foreach (var pad in PadList)
                    {
                        var PadCurrentResult = pad.PMIResults[pad.PMIResults.Count - 1];

                        PadCurrentResult.IsSuccessDetectedPad = false;

                        retval = EventCodeEnum.PMI_NOT_FOUND_PAD;
                    }
                }

                if (retval != EventCodeEnum.PMI_NOT_FOUND_PAD)
                {
                    retval = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                retval = EventCodeEnum.EXCEPTION;
                LoggerManager.Exception(err);
            }
            finally
            {
                if (milBlobResult != null)
                {
                    MIL.MblobFree(milBlobResult);
                    milBlobResult = MIL.M_NULL;
                }
            }

            return retval;
        }

        public ImageBuffer Arithmethic(ImageBuffer Src1Buf, ImageBuffer Src2Buf, ARITHMETIC_OPERATION_TYPE Operation)
        {
            MIL_ID Src1ID = MIL.M_NULL;
            MIL_ID Src2ID = MIL.M_NULL;
            MIL_ID DestID = MIL.M_NULL;

            int width = Src1Buf.SizeX;
            int height = Src1Buf.SizeY;

            byte[] DestArray = new byte[width * height];

            try
            {
                BufAlloc2DUsingImageBuffer(Src1Buf, ref Src1ID, true);
                BufAlloc2DUsingImageBuffer(Src2Buf, ref Src2ID, true);

                MIL.MbufAlloc2d(MilSystem, Src1Buf.SizeX, Src1Buf.SizeY, Src1Buf.Band * Src1Buf.ColorDept, Attributes, ref DestID);

                MIL.MimArith(Src1ID, Src2ID, DestID, MIL.M_AND);

                MIL.MbufGet(DestID, DestArray);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (Src1ID != MIL.M_NULL)
                {
                    MIL.MbufFree(Src1ID);
                }

                if (Src2ID != MIL.M_NULL)
                {
                    MIL.MbufFree(Src2ID);
                }

                if (DestID != MIL.M_NULL)
                {
                    MIL.MbufFree(DestID);
                }
            }

            return new ImageBuffer(DestArray, width, height, Src1Buf.Band, (int)ColorDept.BlackAndWhite);
        }

        public Rect GetRectInFirstBlob(ImageBuffer SrcBuf,
                                                BlobParameter blobparam = null,
                                                ROIParameter roiparam = null,
                                                bool UseAllFeatures = false,
                                                bool foregroundIsZero = false)
        {

            Rect retval = new Rect();
            MIL_ID milBlobResult = MIL.M_NULL;

            Point MinPt = new Point();
            Point MaxPt = new Point();

            try
            {
                byte[] resultBuffer = new byte[SrcBuf.SizeX * SrcBuf.SizeY * 3];

                ObservableCollection<GrabDevPosition> devicePositions = new ObservableCollection<GrabDevPosition>();

                milBlobResult = GetBlobResult(SrcBuf, blobparam, roiparam, UseAllFeatures, foregroundIsZero);

                MIL_INT mIntTotalBlobs = 0;

                MIL.MblobGetNumber(milBlobResult, ref mIntTotalBlobs);

                if (mIntTotalBlobs > 0)
                {
                    MIL_INT[] pBlobLabels = new MIL_INT[mIntTotalBlobs];

                    MIL.MblobGetResult(milBlobResult, MIL.M_LABEL_VALUE + MIL.M_TYPE_MIL_INT, pBlobLabels);

                    //double dblBox_X_Min = 0;
                    //double dblBox_Y_Min = 0;

                    //double dbl_X_Feret = 0;
                    //double dbl_Y_Feret = 0;

                    double[] dblBox_X_Min = new double[mIntTotalBlobs];
                    double[] dblBox_Y_Min = new double[mIntTotalBlobs];

                    double[] dblBox_X_Max = new double[mIntTotalBlobs];
                    double[] dblBox_Y_Max = new double[mIntTotalBlobs];

                    double[] dbl_X_Feret = new double[mIntTotalBlobs];
                    double[] dbl_Y_Feret = new double[mIntTotalBlobs];

                    //double dblBox_X_Max = 0;
                    //double dblBox_Y_Max = 0;

                    // GetBlobResult 함수에서 M_AREA를 기준으로 정렬을 해놓았기 때문에
                    // 첫 번째 Blob의 Area가 가장 크다.

                    MIL.MblobGetResult(milBlobResult, MIL.M_BOX_X_MIN, dblBox_X_Min);
                    MIL.MblobGetResult(milBlobResult, MIL.M_BOX_Y_MIN, dblBox_Y_Min);

                    MIL.MblobGetResult(milBlobResult, MIL.M_BOX_X_MAX, dblBox_X_Max);
                    MIL.MblobGetResult(milBlobResult, MIL.M_BOX_Y_MAX, dblBox_Y_Max);

                    MIL.MblobGetResult(milBlobResult, MIL.M_FERET_X, dbl_X_Feret);
                    MIL.MblobGetResult(milBlobResult, MIL.M_FERET_Y, dbl_Y_Feret);

                    //bool NeedAdjustX = false;
                    //bool NeedAdjustY = false;

                    //double CompareValue = 5;

                    for (int i = 0; i < mIntTotalBlobs; i++)
                    {
                        if (i == 0)
                        {
                            MinPt.X = dblBox_X_Min[i];
                            MinPt.Y = dblBox_Y_Min[i];

                            MaxPt.X = dblBox_X_Max[i];
                            MaxPt.Y = dblBox_Y_Max[i];

                            retval.X = MinPt.X;
                            retval.Y = MinPt.Y;

                            retval.Width = MaxPt.X - MinPt.X;
                            retval.Height = MaxPt.Y - MinPt.Y;
                        }
                        else
                        {
                            if (dblBox_X_Min[i] < MinPt.X)
                            {
                                if ((MinPt.X - dblBox_X_Min[i]) <= SrcBuf.SizeX)
                                {
                                    MinPt.X = dblBox_X_Min[i];

                                    retval.X = MinPt.X;
                                    retval.Width = MaxPt.X - MinPt.X;
                                }
                            }

                            if (dblBox_X_Max[i] > MaxPt.X)
                            {
                                if ((dblBox_X_Max[i] - MaxPt.X) <= SrcBuf.SizeX)
                                {
                                    MaxPt.X = dblBox_X_Max[i];
                                    retval.Width = MaxPt.X - MinPt.X;
                                }
                            }

                            if (dblBox_Y_Min[i] < MinPt.Y)
                            {
                                if ((MaxPt.Y - dblBox_Y_Min[i]) <= SrcBuf.SizeY)
                                {
                                    MinPt.Y = dblBox_Y_Min[i];

                                    retval.Y = MinPt.Y;
                                    retval.Height = MaxPt.Y - MinPt.Y;
                                }
                            }

                            if (dblBox_Y_Max[i] > MaxPt.Y)
                            {
                                if ((dblBox_Y_Max[i] - MaxPt.Y) <= SrcBuf.SizeY)
                                {
                                    MaxPt.Y = dblBox_Y_Max[i];
                                    retval.Height = MaxPt.Y - MinPt.Y;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (milBlobResult != null)
                {
                    MIL.MblobFree(milBlobResult);
                    milBlobResult = MIL.M_NULL;
                }
            }

            return retval;
        }

        public List<Point> GetChainListInFirstBlob(ImageBuffer SrcBuf,
                                                BlobParameter blobparam = null,
                                                ROIParameter roiparam = null,
                                                bool UseAllFeatures = false,
                                                bool foregroundIsZero = false)
        {
            List<Point> retval = new List<Point>();
            MIL_ID milBlobResult = MIL.M_NULL;

            try
            {
                byte[] resultBuffer = new byte[SrcBuf.SizeX * SrcBuf.SizeY * 3];

                ObservableCollection<GrabDevPosition> devicePositions = new ObservableCollection<GrabDevPosition>();

                milBlobResult = GetBlobResult(SrcBuf, blobparam, roiparam, UseAllFeatures, foregroundIsZero);

                MIL_INT mIntTotalBlobs = 0;

                MIL.MblobGetNumber(milBlobResult, ref mIntTotalBlobs);

                double ChainNumber = 0;

                if (mIntTotalBlobs > 0)
                {
                    MIL_INT[] pBlobLabels = new MIL_INT[mIntTotalBlobs];

                    MIL.MblobGetResult(milBlobResult, MIL.M_LABEL_VALUE + MIL.M_TYPE_MIL_INT, pBlobLabels);

                    MIL.MblobGetResultSingle(milBlobResult, pBlobLabels[0], MIL.M_NUMBER_OF_CHAINED_PIXELS, ref ChainNumber);

                    if (ChainNumber > 0)
                    {
                        double[] ChainXPos = new double[(int)ChainNumber];
                        double[] ChainYPos = new double[(int)ChainNumber];

                        MIL.MblobGetResultSingle(milBlobResult, pBlobLabels[0], MIL.M_CHAIN_X, ChainXPos);
                        MIL.MblobGetResultSingle(milBlobResult, pBlobLabels[0], MIL.M_CHAIN_Y, ChainYPos);

                        for (int i = 0; i < ChainNumber; i++)
                        {
                            retval.Add(new Point(ChainXPos[i], ChainYPos[i]));
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (milBlobResult != null)
                {
                    MIL.MblobFree(milBlobResult);
                }
            }

            return retval;
        }

        public ImageBuffer GetEdgeImageInFirstBlob(ImageBuffer SrcBuf,
                                                BlobParameter blobparam = null,
                                                ROIParameter roiparam = null,
                                                bool UseAllFeatures = false,
                                                bool foregroundIsZero = false)
        {
            ImageBuffer returnBuffer = null;

            MIL_ID milBlobResult = MIL.M_NULL;
            MIL_ID milProcBuffer = MIL.M_NULL;

            try
            {
                byte[] resultBuffer = new byte[SrcBuf.SizeX * SrcBuf.SizeY];

                BufAlloc2DUsingImageBuffer(SrcBuf, ref milProcBuffer, false);

                milBlobResult = GetBlobResult(SrcBuf, blobparam, roiparam, UseAllFeatures, foregroundIsZero);

                MIL_INT mIntTotalBlobs = 0;

                MIL.MblobGetNumber(milBlobResult, ref mIntTotalBlobs);

                if (mIntTotalBlobs > 0)
                {
                    MIL_INT[] pBlobLabels = new MIL_INT[mIntTotalBlobs];

                    MIL.MblobGetResult(milBlobResult, MIL.M_LABEL_VALUE + MIL.M_TYPE_MIL_INT, pBlobLabels);

                    MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_WHITE);
                    MIL.MblobDraw(MIL.M_DEFAULT, milBlobResult, milProcBuffer, MIL.M_DRAW_BLOBS_CONTOUR, pBlobLabels[0], MIL.M_DEFAULT);

                    MIL.MbufGet(milProcBuffer, resultBuffer);
                    returnBuffer = new ImageBuffer(resultBuffer, SrcBuf.SizeX, SrcBuf.SizeY, SrcBuf.Band, (int)ColorDept.BlackAndWhite);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (milBlobResult != null)
                {
                    MIL.MblobFree(milBlobResult);
                }

                if (milProcBuffer != null)
                {
                    MIL.MbufFree(milProcBuffer);
                }
            }

            return returnBuffer;
        }

        public BlobResult FindBlobObject(ImageBuffer SrcBuf,
                                            BlobParameter blobparam = null,
                                            ROIParameter roiparam = null,
                                            bool UseAllFeatures = false,
                                            bool foregroundIsZero = false,
                                            bool drawing = false)
        {
            MIL_ID milBlobResult = MIL.M_NULL;
            byte[] resultBuffer = new byte[SrcBuf.SizeX * SrcBuf.SizeY * 3];
            ObservableCollection<GrabDevPosition> devicePositions = new ObservableCollection<GrabDevPosition>();

            try
            {
                int nTotalBlobs = 0;

                milBlobResult = GetBlobResult(SrcBuf, blobparam, roiparam, UseAllFeatures, foregroundIsZero);

                // Get the total number of blobs.
                MIL_INT mIntTotalBlobs = 0;
                MIL.MblobGetNumber(milBlobResult, ref mIntTotalBlobs);

                nTotalBlobs = (int)mIntTotalBlobs;

                if (nTotalBlobs > 0)
                {
                    GrabDevPosition mChipPosition;

                    double[] dblCOGX = new double[nTotalBlobs];
                    double[] dblCOGY = new double[nTotalBlobs];

                    double[] offset = new double[2] { 0, 0 };

                    double[] dblBlobArea = new double[nTotalBlobs];
                    double[] dblAreainum = new double[nTotalBlobs];

                    double[] dblSizex = new double[nTotalBlobs];
                    double[] dblSizey = new double[nTotalBlobs];

                    double[] dblBox_X_Max = new double[nTotalBlobs];
                    double[] dblBox_X_Min = new double[nTotalBlobs];
                    double[] dblBox_Y_Max = new double[nTotalBlobs];
                    double[] dblBox_Y_Min = new double[nTotalBlobs];

                    MIL.MblobGetResult(milBlobResult, MIL.M_FERET_X, dblSizex);
                    MIL.MblobGetResult(milBlobResult, MIL.M_FERET_Y, dblSizey);

                    MIL.MblobGetResult(milBlobResult, MIL.M_BOX_X_MAX, dblBox_X_Max);
                    MIL.MblobGetResult(milBlobResult, MIL.M_BOX_X_MIN, dblBox_X_Min);
                    MIL.MblobGetResult(milBlobResult, MIL.M_BOX_Y_MAX, dblBox_Y_Max);
                    MIL.MblobGetResult(milBlobResult, MIL.M_BOX_Y_MIN, dblBox_Y_Min);

                    MIL.MblobGetResult(milBlobResult, MIL.M_CENTER_OF_GRAVITY_X, dblCOGX);
                    MIL.MblobGetResult(milBlobResult, MIL.M_CENTER_OF_GRAVITY_Y, dblCOGY);

                    MIL.MblobGetResult(milBlobResult, MIL.M_AREA, dblBlobArea);

                    for (int index = 0; index < nTotalBlobs; index++)
                    {
                        mChipPosition = new GrabDevPosition(0, 0, 0, 0, 0, 0);

                        mChipPosition.DevIndex = index;
                        mChipPosition.Area = dblAreainum[index];

                        mChipPosition.PosX = Math.Round(dblCOGX[index], 3);
                        mChipPosition.PosY = Math.Round(dblCOGY[index], 3);
                        mChipPosition.SizeX = dblSizex[index];
                        mChipPosition.SizeY = dblSizey[index];

                        devicePositions.Add(mChipPosition);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (milBlobResult != null)
                {
                    MIL.MblobFree(milBlobResult);
                }
            }

            return new BlobResult(new ImageBuffer(resultBuffer, SrcBuf.SizeX, SrcBuf.SizeY, SrcBuf.Band, SrcBuf.ColorDept), devicePositions);
        }

        public ImageBuffer MilDefaultBinarize(ImageBuffer SrcBuf)
        {
            MIL_ID milProcBuffer = MIL.M_NULL;
            byte[] resultBuffer = new byte[SrcBuf.SizeX * SrcBuf.SizeY * 1];

            try
            {
                MIL.MbufAlloc2d(MilSystem, SrcBuf.SizeX, SrcBuf.SizeY, SrcBuf.ColorDept, Attributes, ref milProcBuffer);

                MIL.MimBinarize(milProcBuffer, milProcBuffer, MIL.M_BIMODAL + MIL.M_GREATER, MIL.M_NULL, MIL.M_NULL);

                MIL.MbufGet(milProcBuffer, resultBuffer);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (milProcBuffer != null)
                {
                    MIL.MbufFree(milProcBuffer);
                }
            }

            return new ImageBuffer(resultBuffer, SrcBuf.SizeX, SrcBuf.SizeY, SrcBuf.Band, SrcBuf.ColorDept);
        }

        public ImageBuffer GetHistogramEqualizationImage(ImageBuffer InputImage, int Mode)
        {
            ImageBuffer retval = null;

            MIL_ID source = MIL.M_NULL;
            MIL_ID dest = MIL.M_NULL;

            try
            {
                BufAlloc2DUsingImageBuffer(InputImage, ref source, true);
                BufAlloc2DUsingImageBuffer(InputImage, ref dest, true);

                if (Mode == 0)
                {
                    double alpha = 0.5;

                    MIL.MimHistogramEqualize(source, dest, MIL.M_EXPONENTIAL, alpha, 0, 255);

                }
                else if (Mode == 1)
                {
                    MIL.MimHistogramEqualize(source, dest, MIL.M_HYPER_CUBE_ROOT, MIL.M_NULL, 0, 255);

                }
                else if (Mode == 2)
                {
                    MIL.MimHistogramEqualize(source, dest, MIL.M_HYPER_LOG, MIL.M_NULL, 0, 255);

                }
                else if (Mode == 3)
                {
                    double alpha = 0.5;

                    MIL.MimHistogramEqualize(source, dest, MIL.M_RAYLEIGH, alpha, 0, 255);

                }
                else if (Mode == 4)
                {
                    MIL.MimHistogramEqualize(source, dest, MIL.M_UNIFORM, MIL.M_NULL, 0, 255);
                }

                byte[] ResultBuffer = new byte[InputImage.SizeX * InputImage.SizeY];
                MIL.MbufGet(dest, ResultBuffer);

                retval = new ImageBuffer(ResultBuffer, (int)InputImage.SizeX, (int)InputImage.SizeY, (int)Attributes, (int)InputImage.ColorDept);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                MIL.MbufFree(source);
                source = MIL.M_NULL;

                MIL.MbufFree(dest);
                dest = MIL.M_NULL;
            }

            return retval;
        }

        public ImageBuffer GetSynthesisImage(ImageBuffer[] InputImages)
        {
            ImageBuffer retval = null;

            try
            {
                int width = 0;
                int height = 0;
                int band = 0;

                if (InputImages != null)
                {
                    int ImageNum = InputImages.Length;

                    if (ImageNum > 0)
                    {
                        width = InputImages[0].SizeX;
                        height = InputImages[0].SizeY;
                        band = InputImages[0].Band;

                        int[] SynthesisintArray = new int[width * height];
                        byte[] SynthesisByteArray = new byte[width * height];

                        int WidthStep;

                        foreach (var img in InputImages)
                        {
                            for (int j = 0; j < height; j++)
                            {
                                WidthStep = j * width;

                                for (int i = 0; i < width; i++)
                                {
                                    SynthesisintArray[WidthStep + i] += img.Buffer[WidthStep + i];
                                }
                            }
                        }

                        for (int j = 0; j < height; j++)
                        {
                            WidthStep = j * width;

                            for (int i = 0; i < width; i++)
                            {
                                SynthesisByteArray[WidthStep + i] = (byte)(SynthesisintArray[WidthStep + i] / ImageNum);
                            }
                        }

                        retval = new ImageBuffer(SynthesisByteArray, width, height, band, (int)ColorDept.BlackAndWhite);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }

            return retval;
        }

        /// <summary>
        /// 라플라시안 위상변환을 이용하여 이미지의 블롭을 강조하고 , 가우시안 범위내에 영역으로 확장합니다. 
        /// </summary>
        /// <param name="Image">이미지 데이터</param>
        /// <param name="fSigma">가우시안 분포 범위</param>
        /// <param name="fScaleFactor"></param>
        /// <returns></returns>
        public ImageBuffer EnhancedImage(ImageBuffer Image, float fSigma, float fScaleFactor)
        {
            ImageBuffer returnBuffer = null;
            MIL_ID milProcResultBuffer = MIL.M_NULL;

            try
            {
                byte[] inputImageBuffer = null;

                if (Image.ColorDept == (int)ColorDept.BlackAndWhite)
                {
                    // Gray scale
                    inputImageBuffer = Image.Buffer;
                }

                else
                {
                    throw new ArgumentException($"ERROR EnhancedImage Image.ColorDepth={Image.ColorDept}, expected value=1 ");
                }

                var resultBuffer = ImageProcessCLR.ImageProcess.EnhanceImageFilter(inputImageBuffer, Image.SizeX, Image.SizeY, 0, fSigma, fScaleFactor);
                returnBuffer = new ImageBuffer(resultBuffer, Image.SizeX, Image.SizeY, Image.Band, (int)ColorDept.BlackAndWhite);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (milProcResultBuffer != MIL.M_NULL)
                {
                    MIL.MbufFree(milProcResultBuffer);
                    milProcResultBuffer = MIL.M_NULL;
                }
            }

            return returnBuffer;
        }

        public ImageBuffer EnhancedOtsuImage(ImageBuffer Image, bool bWhite, int nThreshHoldLimit)
        {
            ImageBuffer returnBuffer = null;
            MIL_ID milProcResultBuffer = MIL.M_NULL;

            try
            {
                byte[] inputImageBuffer = null;

                if (Image.ColorDept == (int)ColorDept.BlackAndWhite)
                {
                    // Gray scale
                    inputImageBuffer = Image.Buffer;
                }
                else
                {
                    throw new ArgumentException($"ERROR EnhancedOtsuImage Image.ColorDepth={Image.ColorDept}, expected value=1 ");
                }

                var resultBuffer = ImageProcessCLR.ImageProcess.Enhance_Threshold_OtsuFilter(inputImageBuffer, Image.SizeX, Image.SizeY, bWhite, nThreshHoldLimit);
                returnBuffer = new ImageBuffer(resultBuffer, Image.SizeX, Image.SizeY, Image.Band, (int)ColorDept.BlackAndWhite);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (milProcResultBuffer != MIL.M_NULL)
                {
                    MIL.MbufFree(milProcResultBuffer);
                    milProcResultBuffer = MIL.M_NULL;
                }
            }

            return returnBuffer;
        }

    }
}

