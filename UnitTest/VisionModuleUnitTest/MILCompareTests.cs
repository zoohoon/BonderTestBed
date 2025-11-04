using Microsoft.VisualStudio.TestTools.UnitTesting;
using Matrox.MatroxImagingLibrary;
using VisionModuleUnitTests;
using System.Drawing;
using System;
using ProberInterfaces;
using System.IO;
using System.Linq;

namespace ProcessingModule.Tests
{
    

    [TestClass()]
    public class MILCompareTests
    {
        /// <summary>
        /// MIL  라이선스 (동글 확인)
        /// </summary>
        [TestMethod]
        public void MILLicenseCheck()
        {   
            using (var app = new MilApp())
            {  
            }
        }

        /// <summary>
        /// MIL 단독 이진화 기능 테스트
        /// </summary>
        [TestMethod()]
        public void ThresholdFuncTest()
        {
            int width = 256;
            int height = 256;

            long Attributes = MIL.M_IMAGE + MIL.M_DISP + MIL.M_PROC;
            using (var app = new MilApp())
            using (var sys = new MilSys())
            {            
                using (var srcImageBuf = new Milbuf2d(sys, width, height, 8, Attributes))
                using (var dstImageBuf = new Milbuf2d(sys, width, height, 8, Attributes))
                {
                    MIL.MbufClear(srcImageBuf, 0);  // 버퍼를 0색상으로 지워준다.
                    MIL.MbufClear(dstImageBuf, 0);  // 버퍼를 0색상으로 지워준다.

                    var sample = Sample.CreateSampeImage_VerticalGradient(width, height);

                    MIL.MbufPut(srcImageBuf, sample);

                    int targetThreshold = 100;

                    MIL.MimBinarize(srcImageBuf, dstImageBuf, MIL.M_GREATER_OR_EQUAL, targetThreshold, MIL.M_NULL);
                    byte[] resultBuffer = new byte[width * height];

                    MIL.MbufGet(dstImageBuf, resultBuffer);


                    /// Compare Target
                    byte[] compare = new byte[sample.Length];

                    System.Threading.Tasks.Parallel.For(0, compare.Length, (i) =>
                    {
                        compare[i] = sample[i] >= targetThreshold ? (byte)255 : (byte)0;
                    });
                    ///Result            
                    CollectionAssert.AreEqual(compare, resultBuffer);
                }
            }
        }

        /// <summary>
        /// L 형태로 채워진 블롭이 채워진 면적으로 계산되는지 확인 Convex Hull 알고리즘 검증
        /// </summary>
        [TestMethod()]
        public void ConvexHullFuncTest()
        {
            int width = 256;
            int height = 256;

            long Attributes = MIL.M_IMAGE + MIL.M_DISP + MIL.M_PROC;
            using (var app = new MilApp())
            using (var sys = new MilSys())
            {
                using (var srcImageBuf = new Milbuf2d(sys, width, height, 8, Attributes))
                using (var dstImageBuf = new Milbuf2d(sys, width, height, 8, Attributes))
                {
                    MIL.MbufClear(srcImageBuf, 0);  // 버퍼를 0색상으로 지워준다.
                    MIL.MbufClear(dstImageBuf, 0);  // 버퍼를 0색상으로 지워준다.

                    var sample = Sample.CreateSampeImage_L(width, height);

                    MIL.MbufPut(srcImageBuf, sample);
                    
                    string curDate = "C:\\Logs\\Images\\ConvexHullFuncTest.bmp";
                    MIL.MbufExport(curDate, MIL.M_BMP, srcImageBuf);

                    MIL_ID milBlobResult = MIL.M_NULL;
                    MIL.MblobAllocResult(sys, ref milBlobResult);
                    
                    MIL_ID milBlobFeatureList = MIL.M_NULL;
                    MIL.MblobAllocFeatureList(sys, ref milBlobFeatureList);
                    
                    MIL.MblobControl(milBlobResult, MIL.M_FOREGROUND_VALUE, MIL.M_NONZERO);
                    MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_ALL_FEATURES);
                    MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_AREA + MIL.M_SORT1_DOWN);

                    MIL.MblobCalculate(srcImageBuf, MIL.M_NULL, milBlobFeatureList, milBlobResult);

                    MIL_INT mIntTotalBlobs = 0;
                    MIL.MblobGetNumber(milBlobResult, ref mIntTotalBlobs);
                    Assert.AreEqual(1, mIntTotalBlobs);

                    double[] dblCovexHull_Area = new double[mIntTotalBlobs];
                    MIL.MblobGetResult(milBlobResult, MIL.M_CONVEX_HULL_AREA, dblCovexHull_Area);

                    Assert.AreEqual(3612, dblCovexHull_Area[0]);
                    
                }
            }
        }

        [TestMethod()]
        public void ThresholdCrossCompare()
        {

            int width = 256;
            int height = 256;

            long Attributes = MIL.M_IMAGE + MIL.M_DISP + MIL.M_PROC;
            using (var app = new MilApp())
            using (var sys = new MilSys())
            {
                using (var srcImageBuf = new Milbuf2d(sys, width, height, 8, Attributes))
                using (var dstImageBuf = new Milbuf2d(sys, width, height, 8, Attributes))
                {
                    MIL.MbufClear(srcImageBuf, 0);  // 버퍼를 0색상으로 지워준다.
                    MIL.MbufClear(dstImageBuf, 0);  // 버퍼를 0색상으로 지워준다.

                    var sample = Sample.CreateSampeImage_VerticalGradient(width, height);

                    MIL.MbufPut(srcImageBuf, sample);

                    int targetThreshold = 100;

                    //MIL.MimBinarize(srcImageBuf, dstImageBuf, MIL.M_GREATER_OR_EQUAL, targetThreshold, MIL.M_NULL);
                    MIL.MimBinarize(srcImageBuf, dstImageBuf, MIL.M_GREATER, targetThreshold, MIL.M_NULL);
                    byte[] resultBuffer = new byte[width * height];

                    MIL.MbufGet(dstImageBuf, resultBuffer);

                    ProberInterfaces.VisionFramework.IVisionLib visionLib = new VisionFrameworkLib();

                    var result = visionLib.FilterExecute(
                       sample, width, height,
                      filter: "ThresholdFilter", param: $@"{{""Params"":[{{""Key"":""Threshold"", ""Value"":""{targetThreshold}""}},{{""Key"":""Invert"", ""Value"":""False""}}]}}");


                    /// Compare Target
                    byte[] compare = new byte[sample.Length];

                    System.Threading.Tasks.Parallel.For(0, compare.Length, (i) =>
                    {
                        compare[i] = sample[i] > targetThreshold ? (byte)255 : (byte)0;
                    });
                    ///Result            
                    CollectionAssert.AreEqual(compare, resultBuffer);

                    CollectionAssert.AreEqual(resultBuffer, result.Item1);

                }
            }
        }

        [TestMethod()]
        public void MILTESTXXX()
        {
            int FoundNum = 0;
            //int width = 256;
            //int height = 256;

            long Attributes = MIL.M_IMAGE + MIL.M_DISP + MIL.M_PROC;

            ProberInterfaces.VisionFramework.IVisionLib visionLib = new VisionFrameworkLib();

            using (var app = new MilApp())
            using (var sys = new MilSys())
            {
                try
                {
                    string filename= "";
                    //string sampleFile = System.IO.Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "TestData", "sample0.bmp");
                    string folderPath = @"C:\ProberSystem\EmulImages\PinImage\ProcessImage\"; // 폴더 경로 설정

                    // 폴더 내 모든 파일 가져오기
                    string[] files = Directory.GetFiles(folderPath);

                    foreach (string filePath in files)
                    {
                        filename = Path.GetFileNameWithoutExtension(filePath);

                        //Assert.AreEqual(true, System.IO.File.Exists(sampleFile), $"File Not exist : {sampleFile}");

                        Image sampleImg = Image.FromFile(filePath);

                        Assert.IsNotNull(sampleImg);

                        byte[] resultBuffer = new byte[sampleImg.Width * sampleImg.Height];

                        using (var srcImageBuf = new Milbuf2d(sys, sampleImg.Width, sampleImg.Height, 8, Attributes))
                        using (var dstImageBuf = new Milbuf2d(sys, sampleImg.Width, sampleImg.Height, 8, Attributes))
                        {
                            MIL.MbufClear(srcImageBuf, 0);  // 버퍼를 0색상으로 지워준다.
                            MIL.MbufClear(dstImageBuf, 0);  // 버퍼를 0색상으로 지워준다.

                            MIL_ID FeatureList = 0, BlobResult = 0, milProcBuffer = MIL.M_NULL;
                            MIL_INT TotalBlobs = 0;
                            int targetThreshold = 180;

                            byte[] imageProcbuffer = VisionFrameworkLibTests.ImageToGrayBytes(sampleImg, 0, 0, sampleImg.Width, sampleImg.Height);
                            MIL.MbufPut(srcImageBuf, imageProcbuffer);
                            MIL.MbufChild2d(srcImageBuf, 0, 0, sampleImg.Width, sampleImg.Height, ref milProcBuffer);
#pragma warning disable 0162
                            if (true)
                            {
                                (byte[], int, int, int)[] inputImages = { (imageProcbuffer, sampleImg.Width, sampleImg.Height, 1) };
                                string recipeParam = $"{{" +
                                           $" \"ThresholdFilter\" : {{\"Params\":[  {{\"Key\":\"Threshold\", \"Value\":\"{targetThreshold}\"}}]}}" +
                                           $", \"PinBlobModeSelector\" : {{\"Params\":[  {{\"Key\":\"AutoMode\", \"Value\":\"{true}\"}}]}}" +
                                           $", \"EnhanceOtsuFilter\" : {{\"Params\":[  {{\"Key\":\"W/B\", \"Value\":\"{false}\"}}]}}" +
                                           $"}}";

                                MIL.MbufPut(milProcBuffer,
                                    visionLib.RecipeExecuteFixedSize(inputImages
                                    , visionLib.GetReservedRecipe(ProberInterfaces.VisionFramework.ReservedRecipe.SinglePinAlignTipBlob)
                                        , out string result
                                        , recipeParam
                                        ));
                                //</2022-05-03-3C89>
                            }
                            else
                            {
                                MIL.MimBinarize(milProcBuffer, milProcBuffer, MIL.M_GREATER_OR_EQUAL, targetThreshold, MIL.M_NULL);
                                MIL.MimOpen(milProcBuffer, milProcBuffer, 1, MIL.M_BINARY);
                                MIL.MimClose(milProcBuffer, milProcBuffer, 1, MIL.M_BINARY);
                                MIL.MbufGet(milProcBuffer, resultBuffer);
                            }
#pragma warning restore 0162

                            MIL.MblobAllocFeatureList(sys, ref FeatureList);
                            MIL.MblobSelectFeature(FeatureList, MIL.M_AREA);
                            MIL.MblobSelectFeature(FeatureList, MIL.M_BOX_AREA);
                            MIL.MblobSelectFeature(FeatureList, MIL.M_CENTER_OF_GRAVITY);
                            MIL.MblobSelectFeature(FeatureList, MIL.M_ELONGATION);

                            MIL.MblobSelectFeature(FeatureList, MIL.M_BOX_X_MIN);
                            MIL.MblobSelectFeature(FeatureList, MIL.M_BOX_X_MAX);
                            MIL.MblobSelectFeature(FeatureList, MIL.M_BOX_Y_MIN);
                            MIL.MblobSelectFeature(FeatureList, MIL.M_BOX_Y_MAX);

                            MIL.MblobAllocResult(sys, ref BlobResult);

                            MIL.MblobCalculate(milProcBuffer, MIL.M_NULL, FeatureList, BlobResult);

                            MIL.MblobSelect(BlobResult, MIL.M_INCLUDE, MIL.M_ALL_BLOBS, MIL.M_NULL, MIL.M_NULL, MIL.M_NULL);
                            MIL.MblobGetNumber(BlobResult, ref TotalBlobs);

                            MIL.MblobSelect(BlobResult, MIL.M_EXCLUDE, MIL.M_BOX_X_MIN, MIL.M_LESS_OR_EQUAL, 0, MIL.M_NULL);
                            MIL.MblobSelect(BlobResult, MIL.M_EXCLUDE, MIL.M_BOX_X_MAX, MIL.M_GREATER_OR_EQUAL, sampleImg.Width - 1, MIL.M_NULL);
                            MIL.MblobSelect(BlobResult, MIL.M_EXCLUDE, MIL.M_BOX_Y_MIN, MIL.M_LESS_OR_EQUAL, 0, MIL.M_NULL);
                            MIL.MblobSelect(BlobResult, MIL.M_EXCLUDE, MIL.M_BOX_Y_MAX, MIL.M_GREATER_OR_EQUAL, sampleImg.Height - 1, MIL.M_NULL);

                            MIL.MblobGetNumber(BlobResult, ref TotalBlobs);


                            var ratio = 0.42;
                            int BlobMinSize = Convert.ToInt32(15 / ratio * 15 / ratio);
                            int BlobMaxSize = Convert.ToInt32(80/ ratio * 80/ ratio);
                            double AreaInnerMarginRatio = 0.1;
                            double AreaLowAddMarginValue = BlobMinSize * (1.0 - AreaInnerMarginRatio);
                            double AreaHighAddMarginValue = BlobMaxSize * (1.0 + AreaInnerMarginRatio);

                            if (TotalBlobs > 0)
                            {
                                double[] dblBlobArea = new double[TotalBlobs];

                                double[] dblSizeX = new double[TotalBlobs];
                                double[] dblSizeY = new double[TotalBlobs];

                                double[] dblBox_X_Max = new double[TotalBlobs];
                                double[] dblBox_X_Min = new double[TotalBlobs];
                                double[] dblBox_Y_Max = new double[TotalBlobs];
                                double[] dblBox_Y_Min = new double[TotalBlobs];

                                double[] dblGravity_X = new double[TotalBlobs];
                                double[] dblGravity_Y = new double[TotalBlobs];

                                double[] dblBlobIndex = new double[TotalBlobs];

                                bool[] IsFound = new bool[TotalBlobs];

                                MIL.MblobGetResult(BlobResult, MIL.M_AREA, dblBlobArea);

                                MIL.MblobGetResult(BlobResult, MIL.M_FERET_X, dblSizeX);
                                MIL.MblobGetResult(BlobResult, MIL.M_FERET_Y, dblSizeY);

                                MIL.MblobGetResult(BlobResult, MIL.M_BOX_X_MAX, dblBox_X_Max);
                                MIL.MblobGetResult(BlobResult, MIL.M_BOX_X_MIN, dblBox_X_Min);
                                MIL.MblobGetResult(BlobResult, MIL.M_BOX_Y_MAX, dblBox_Y_Max);
                                MIL.MblobGetResult(BlobResult, MIL.M_BOX_Y_MIN, dblBox_Y_Min);

                                MIL.MblobGetResult(BlobResult, MIL.M_CENTER_OF_GRAVITY_X, dblGravity_X);
                                MIL.MblobGetResult(BlobResult, MIL.M_CENTER_OF_GRAVITY_Y, dblGravity_Y);

                                MIL.MblobGetResult(BlobResult, MIL.M_LABEL_VALUE, dblBlobIndex);

                                for (int index = 0; index < TotalBlobs; index++)
                                {
                                    Console.WriteLine($"Mark Index = {index + 1} : SIZE X, Y = ({dblSizeX[index]}, {dblSizeY[index]}), AREA = {dblBlobArea[index]}");

                                    if ((dblBlobArea[index] >= AreaLowAddMarginValue) &&
                                         (dblBlobArea[index] <= AreaHighAddMarginValue)
                                         )
                                    {
                                        Console.WriteLine($"Deslect Mark Index = {index + 1} : SIZE X, Y = ({dblSizeX[index]}, {dblSizeY[index]}), AREA = {dblBlobArea[index]}");
                                    }
                                    else
                                    {
                                        IsFound[index] = false;

                                        MIL.MblobSelect(BlobResult, MIL.M_EXCLUDE, MIL.M_LABEL_VALUE, MIL.M_EQUAL, dblBlobIndex[index], MIL.M_NULL);
                                        //MIL.MblobGetNumber(BlobResult, ref TotalBlobs);
                                    }
                                }

                                FoundNum = IsFound.Count(x => x == true);
                            }

                            MIL_ID milDrawingOriginalBuffer = MIL.M_NULL;
                            MIL_ID milDrawingChildBuffer = MIL.M_NULL;
                            MIL_ID milImage = MIL.M_NULL;

                            long cl_iAttributes = MIL.M_IMAGE + MIL.M_PROC + MIL.M_RGB24 + MIL.M_PACKED;
                            Attributes = MIL.M_IMAGE + MIL.M_DISP + MIL.M_PROC;


                            MIL.MbufAllocColor(sys, 3, sampleImg.Width, sampleImg.Height, 8 + MIL.M_UNSIGNED, cl_iAttributes, ref milDrawingOriginalBuffer);

                            MIL.MbufAlloc2d(sys, sampleImg.Width, sampleImg.Height, 8, Attributes, ref milImage);
                            MIL.MbufPut2d(milImage, 0, 0, sampleImg.Width, sampleImg.Height, imageProcbuffer);

                            MIL.MimConvert(milImage, milDrawingOriginalBuffer, MIL.M_L_TO_RGB);

                            if (milImage != MIL.M_NULL)
                            {
                                MIL.MbufFree(milImage);
                                milImage = MIL.M_NULL;
                            }

                            MIL_INT BufSizeX = MIL.MbufInquire(milProcBuffer, MIL.M_SIZE_X, MIL.M_NULL);
                            MIL_INT BufSizeY = MIL.MbufInquire(milProcBuffer, MIL.M_SIZE_Y, MIL.M_NULL);
                            MIL_INT BufBand = MIL.MbufInquire(milProcBuffer, MIL.M_SIZE_BAND, MIL.M_NULL);
                            MIL_INT BufSizeBit = MIL.MbufInquire(milProcBuffer, MIL.M_SIZE_BIT, MIL.M_NULL);

                            MIL.MbufAllocColor(sys, 3, BufSizeX, BufSizeY, 8 + MIL.M_UNSIGNED, cl_iAttributes, ref milDrawingChildBuffer);

                            MIL.MbufAlloc2d(sys, BufSizeX, BufSizeY, BufBand * BufSizeBit, Attributes, ref milImage);

                            byte[] InputArr = new byte[BufSizeX * BufSizeY];
                            MIL.MbufGet(milProcBuffer, InputArr);

                            MIL.MbufPut2d(milImage, 0, 0, BufSizeX, BufSizeY, InputArr);

                            MIL.MimConvert(milImage, milDrawingChildBuffer, MIL.M_L_TO_RGB);
                            // Blob을 계산할 때, ChildBuffer(Search Area가 적용된)를 사용했기 때문에
                            // 해당 사이즈와 같은 컬러 버퍼를 할당하고 Drawing을 진행한다.
                            // 이 때, 저장할 이미지는 원본 영상의 크기로 저장하기 때문에, Childer Buffer의 Offset을 이용하여 MbufPutColor2d를 통해, 이미지를 합성시킨 후 저장한다.

                            MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_GREEN);
                            MIL.MblobDraw(MIL.M_DEFAULT, BlobResult, milDrawingChildBuffer, MIL.M_DRAW_CENTER_OF_GRAVITY + MIL.M_DRAW_BLOBS_CONTOUR, MIL.M_INCLUDED_BLOBS, MIL.M_DEFAULT);


                            BufSizeX = MIL.MbufInquire(milDrawingChildBuffer, MIL.M_SIZE_X, MIL.M_NULL);
                            BufSizeY = MIL.MbufInquire(milDrawingChildBuffer, MIL.M_SIZE_Y, MIL.M_NULL);
                            BufBand = MIL.MbufInquire(milDrawingChildBuffer, MIL.M_SIZE_BAND, MIL.M_NULL);
                            BufSizeBit = MIL.MbufInquire(milDrawingChildBuffer, MIL.M_SIZE_BIT, MIL.M_NULL);

                            byte[] DrawingChildBufferArr = new byte[BufSizeX * BufSizeY * BufBand];

                            MIL.MbufGetColor(milDrawingChildBuffer, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BAND, DrawingChildBufferArr);
                            MIL.MbufPutColor2d(milDrawingOriginalBuffer, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BANDS, 0, 0, sampleImg.Width, sampleImg.Height, DrawingChildBufferArr);

                            MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_BLUE);
                            MIL.MgraRect(MIL.M_DEFAULT, milDrawingOriginalBuffer, 0, 0, sampleImg.Width + 0 - 1, sampleImg.Height + 0 - 1);
                            string SaveProcessedImage = @"C:\ProberSystem\EmulImages\PinImage\ProcessedImage\" + filename + ".bmp";
                            MIL.MbufExport(SaveProcessedImage, MIL.M_BMP, milDrawingOriginalBuffer);


                            // First Child Buffer Free?
                            if (milProcBuffer != null)
                                MIL.MbufFree(milProcBuffer);

                            if (BlobResult != null)
                                MIL.MblobFree(BlobResult);

                            if (FeatureList != null)
                                MIL.MblobFree(FeatureList);

                            if (milDrawingOriginalBuffer != MIL.M_NULL)
                            {
                                MIL.MbufFree(milDrawingOriginalBuffer);
                                milDrawingOriginalBuffer = MIL.M_NULL;
                            }

                            if (milDrawingChildBuffer != MIL.M_NULL)
                            {
                                MIL.MbufFree(milDrawingChildBuffer);
                                milDrawingChildBuffer = MIL.M_NULL;
                            }

                            if (milImage != MIL.M_NULL)
                            {
                                MIL.MbufFree(milImage);
                                milImage = MIL.M_NULL;
                            }

                        }

                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                }
            }
        }


        [TestMethod()]
        public void MILTESTXXXX()
        {
            //int FoundNum = 0;
            //int width = 256;
            //int height = 256;
            long iAttributes = MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP;

            ProberInterfaces.VisionFramework.IVisionLib visionLib = new VisionFrameworkLib();

            using (var app = new MilApp())
            using (var sys = new MilSys())
            {
                try
                {
                    string filename = "";
                    string folderPath = @"C:\ProberSystem\EmulImages\WaferAlign\IndexAlignEdge\"; // 폴더 경로 설정
                    int Cpos = 0;
                    int RWidth = 10;
                    int Threshold = 20;
                    // 폴더 내 모든 파일 가져오기
                    string[] files = Directory.GetFiles(folderPath);
                    ImageBuffer SrcBuf;
                    foreach (string filePath in files)
                    {
                        filename = Path.GetFileNameWithoutExtension(filePath);

                        using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(filePath))
                        {
                            byte[] imageData;
                            FileStream imageStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                            BinaryReader binaryReader = new BinaryReader(imageStream);
                            imageData = binaryReader.ReadBytes((int)imageStream.Length);
                            imageStream.Close();
                            binaryReader.Close();
                            System.Drawing.Imaging.PixelFormat pformet = bitmap.PixelFormat;
                            if (pformet == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
                                SrcBuf = new ImageBuffer(imageData, bitmap.Width, bitmap.Height, 1, 8);
                            else
                                SrcBuf = new ImageBuffer(imageData, bitmap.Width, bitmap.Height, 1, 8);
                        }

                        {
                            double retval = -1;

                            try
                            {
                                ImageBuffer Line_Equalization_Buf;

                                int ImageWidth = SrcBuf.SizeX;
                                int ImageHeight = SrcBuf.SizeY;

                                int alphaS, BetaS;
                                int alphaE, BetaE;
                                int PMFLAG;

                                int Except_Pixel = 30;
                                int Acc_Pixel = 20;
                                int Cmp_Pixel = 10;

                                double Temp_Sum = 0;
                                double Temp_Avg = 0;

                                int j;

                                int XPos, YPos;


                                double[] EdgeOVal = new double[ImageWidth];
                                double[] EdgePLAvg = new double[ImageWidth];
                                double[] EdgePRAvg = new double[ImageWidth];
                                double[] EdgeMOVal = new double[ImageWidth];


                                double P_RSum;
                                double P_LSum;

                                bool OKFLAG = false;

                                int innerAreaMin = (ImageWidth / 2 - 1) - (RWidth / 2);
                                int innerAreaMax = (ImageWidth / 2 - 1) + (RWidth / 2);

                                int lastWidthPixelValue = ImageWidth - 1;
                                int lastHeightPixelValue = ImageHeight - 1;

                                Line_Equalization_Buf = Line_Equalization_Processing(SrcBuf, Cpos);

                                byte[,] Equal2DBufArray = ConvertByte1DTo2DArray(Line_Equalization_Buf);

                                if (Line_Equalization_Buf == null)
                                {
                                    retval = -1;
                                }
                                else
                                {
                                    if ((Cpos == 1) || (Cpos == 2))
                                    {
                                        alphaS = 0;
                                        alphaE = lastWidthPixelValue;
                                    }
                                    else
                                    {
                                        alphaS = lastWidthPixelValue;
                                        alphaE = 0;
                                    }

                                    if ((Cpos == 0) || (Cpos == 1))
                                    {
                                        BetaS = 0;
                                        BetaE = lastWidthPixelValue;

                                        PMFLAG = 1;
                                    }
                                    else
                                    {
                                        BetaS = lastWidthPixelValue;
                                        BetaE = 0;

                                        PMFLAG = -1;
                                    }

                                    j = BetaS + (Except_Pixel * PMFLAG);

                                    // 0
                                    if (Cpos == 0)
                                    {
                                        if (alphaS > alphaE)
                                        {
                                            for (int i = (alphaS - Except_Pixel); i >= (alphaE + 1); i--)
                                            {
                                                Temp_Sum = 0;

                                                for (int k = 1; k <= Acc_Pixel; k++)
                                                {
                                                    XPos = i + k;
                                                    YPos = (j - (k * PMFLAG));

                                                    Temp_Sum += Equal2DBufArray[YPos, XPos];
                                                }

                                                Temp_Avg = Temp_Sum / Acc_Pixel;

                                                if ((i > Except_Pixel) && (i < ImageWidth - Except_Pixel))
                                                {
                                                    EdgeOVal[lastWidthPixelValue - i] = Temp_Avg - Equal2DBufArray[j, i];
                                                }

                                                j = j + PMFLAG;
                                            }
                                        }

                                        for (int i = Except_Pixel; i < lastWidthPixelValue - Except_Pixel; i++)
                                        {
                                            P_RSum = 0;
                                            P_LSum = 0;

                                            for (int k = 1; k <= Cmp_Pixel; k++)
                                            {
                                                //P_RSum += Equal2DBufArray[(i - k) * ImageWidth, (ImageWidth - 1 - i + k)];
                                                P_RSum += Equal2DBufArray[(i - k), (lastWidthPixelValue - i + k)];
                                            }

                                            for (int k = 1; k <= Cmp_Pixel; k++)
                                            {
                                                //P_LSum += Equal2DBufArray[(i + k) * ImageWidth, (ImageWidth - 1 - i - k)];
                                                P_LSum += Equal2DBufArray[(i + k), (lastWidthPixelValue - i - k)];
                                            }

                                            EdgePRAvg[i] = P_RSum / Cmp_Pixel;
                                            EdgePLAvg[i] = P_LSum / Cmp_Pixel;
                                        }

                                        for (int i = 0; i < ImageWidth; i++)
                                        {
                                            EdgeMOVal[lastWidthPixelValue - i] = Math.Abs(EdgeOVal[lastWidthPixelValue - i]) + Math.Abs(EdgePLAvg[lastWidthPixelValue - i] - EdgePRAvg[lastWidthPixelValue - i]);

                                            if (EdgeMOVal[lastWidthPixelValue - i] > Threshold)
                                            {
                                                if ((i >= innerAreaMin) && (i <= innerAreaMax))
                                                {
                                                    OKFLAG = true;
                                                }
                                            }

                                            if ((i >= innerAreaMin) && (i <= innerAreaMax))
                                            {
                                                if (EdgeMOVal[lastWidthPixelValue - i] > retval)
                                                {
                                                    retval = EdgeMOVal[lastWidthPixelValue - i];
                                                }
                                            }
                                        }
                                    }

                                    // 1
                                    if (Cpos == 1)
                                    {
                                        for (int i = (alphaS + Except_Pixel); i <= (alphaE - 1); i++)
                                        {
                                            Temp_Sum = 0;

                                            for (int k = 1; k <= Acc_Pixel; k++)
                                            {
                                                XPos = i - k;
                                                YPos = (j - (k * PMFLAG));

                                                Temp_Sum += Equal2DBufArray[YPos, XPos];
                                            }

                                            Temp_Avg = Temp_Sum / Acc_Pixel;

                                            if ((i > Except_Pixel) && (i < ImageWidth - Except_Pixel))
                                            {
                                                EdgeOVal[i] = Temp_Avg - Equal2DBufArray[j, i];
                                            }

                                            j = j + PMFLAG;
                                        }

                                        for (int i = Except_Pixel; i < lastWidthPixelValue - Except_Pixel; i++)
                                        {
                                            P_RSum = 0;
                                            P_LSum = 0;

                                            for (int k = 1; k <= Cmp_Pixel; k++)
                                            {
                                                P_RSum += Equal2DBufArray[(i + k), (i + k)];
                                            }

                                            for (int k = 1; k <= Cmp_Pixel; k++)
                                            {
                                                P_LSum += Equal2DBufArray[(i - k), (i - k)];
                                            }

                                            EdgePRAvg[i] = P_RSum / Cmp_Pixel;
                                            EdgePLAvg[i] = P_LSum / Cmp_Pixel;
                                        }

                                        for (int i = 0; i < ImageWidth; i++)
                                        {
                                            EdgeMOVal[i] = Math.Abs(EdgeOVal[i]) + Math.Abs(EdgePLAvg[i] - EdgePRAvg[i]);
                                            if (EdgeMOVal[i] > Threshold)
                                            {
                                                if ((i >= innerAreaMin) && (i <= innerAreaMax))
                                                {
                                                    OKFLAG = true;
                                                }
                                            }

                                            if ((i >= innerAreaMin) && (i <= innerAreaMax))
                                            {
                                                if (EdgeMOVal[i] > retval)
                                                {
                                                    retval = EdgeMOVal[i];
                                                }
                                            }
                                        }
                                    }

                                    // 2
                                    if (Cpos == 2)
                                    {
                                        for (int i = (alphaS + Except_Pixel); i <= (alphaE - 1); i++)
                                        {
                                            Temp_Sum = 0;

                                            for (int k = 1; k <= Acc_Pixel; k++)
                                            {
                                                XPos = i - k;
                                                YPos = (j - (k * PMFLAG));

                                                Temp_Sum += Equal2DBufArray[YPos, XPos];
                                            }

                                            Temp_Avg = Temp_Sum / Acc_Pixel;

                                            if ((i > Except_Pixel) && (i < ImageWidth - Except_Pixel))
                                            {
                                                EdgeOVal[i] = Temp_Avg - Equal2DBufArray[j, i];
                                            }

                                            j = j + PMFLAG;
                                        }

                                        for (int i = Except_Pixel; i < lastWidthPixelValue - Except_Pixel; i++)
                                        {
                                            P_RSum = 0;
                                            P_LSum = 0;

                                            for (int k = 1; k <= Cmp_Pixel; k++)
                                            {
                                                P_RSum += Equal2DBufArray[(lastWidthPixelValue - i + k), (i - k)];
                                            }

                                            for (int k = 1; k <= Cmp_Pixel; k++)
                                            {
                                                //P_LSum += Equal2DBufArray[(ImageHeight - i + k), (i - k)];
                                                P_LSum += Equal2DBufArray[(lastWidthPixelValue - i - k), (i + k)];
                                            }

                                            EdgePRAvg[i] = P_RSum / Cmp_Pixel;
                                            EdgePLAvg[i] = P_LSum / Cmp_Pixel;
                                        }

                                        for (int i = 0; i < ImageWidth; i++)
                                        {
                                            EdgeMOVal[i] = Math.Abs(EdgeOVal[i]) + Math.Abs(EdgePLAvg[i] - EdgePRAvg[i]);

                                            if (EdgeMOVal[i] > Threshold)
                                            {
                                                if ((i >= innerAreaMin) && (i <= innerAreaMax))
                                                {
                                                    OKFLAG = true;
                                                }
                                            }

                                            if ((i >= innerAreaMin) && (i <= innerAreaMax))
                                            {
                                                if (EdgeMOVal[i] > retval)
                                                {
                                                    retval = EdgeMOVal[i];
                                                }
                                            }
                                        }
                                    }

                                    // 3
                                    if (Cpos == 3)
                                    {
                                        for (int i = (alphaS - Except_Pixel); i >= (alphaE + 1); i--)
                                        {
                                            Temp_Sum = 0;

                                            for (int k = 1; k <= Acc_Pixel; k++)
                                            {
                                                XPos = i + k;
                                                YPos = (j - (k * PMFLAG));

                                                Temp_Sum += Equal2DBufArray[YPos, XPos];
                                            }

                                            Temp_Avg = Temp_Sum / Acc_Pixel;

                                            if ((i > Except_Pixel) && (i < ImageWidth - Except_Pixel))
                                            {
                                                EdgeOVal[lastWidthPixelValue - i] = Temp_Avg - Equal2DBufArray[j, i];
                                            }

                                            j = j + PMFLAG;
                                        }

                                        for (int i = Except_Pixel; i < lastWidthPixelValue - Except_Pixel; i++)
                                        {
                                            P_RSum = 0;
                                            P_LSum = 0;

                                            for (int k = 1; k <= Cmp_Pixel; k++)
                                            {
                                                P_RSum += Equal2DBufArray[(lastWidthPixelValue - i - k), (lastWidthPixelValue - i - k)];
                                            }

                                            for (int k = 1; k <= Cmp_Pixel; k++)
                                            {
                                                //P_LSum += Equal2DBufArray[(ImageHeight - i - k), (ImageWidth - i - k)];
                                                P_LSum += Equal2DBufArray[(lastWidthPixelValue - i + k), (lastWidthPixelValue - i + k)];
                                            }

                                            EdgePRAvg[i] = P_RSum / Cmp_Pixel;
                                            EdgePLAvg[i] = P_LSum / Cmp_Pixel;
                                        }

                                        for (int i = 0; i < ImageWidth; i++)
                                        {
                                            EdgeMOVal[lastWidthPixelValue - i] = Math.Abs(EdgeOVal[lastWidthPixelValue - i]) + Math.Abs(EdgePLAvg[lastWidthPixelValue - i] - EdgePRAvg[lastWidthPixelValue - i]);

                                            if (EdgeMOVal[lastWidthPixelValue - i] > Threshold)
                                            {
                                                if ((i >= innerAreaMin) && (i <= innerAreaMax))
                                                {
                                                    OKFLAG = true;
                                                }
                                            }

                                            if ((i >= innerAreaMin) && (i <= innerAreaMax))
                                            {
                                                if (EdgeMOVal[lastWidthPixelValue - i] > retval)
                                                {
                                                    retval = EdgeMOVal[lastWidthPixelValue - i];
                                                }
                                            }
                                        }
                                    }

                                    if (OKFLAG == true)
                                    {
                                        retval = -1;
                                    }
                                }
                            }
                            catch (Exception)
                            {
                            }
                            
                        }
                        Cpos ++;
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                }

                byte[,] ConvertByte1DTo2DArray(ImageBuffer img)
                {
                    byte[,] RetByte2DArray = new byte[img.SizeY, img.SizeX];
                    try
                    {
                        int WidthStep;

                        for (int j = 0; j < img.SizeY; j++)
                        {
                            WidthStep = j * img.SizeX;

                            for (int i = 0; i < img.SizeX; i++)
                            {
                                RetByte2DArray[j, i] = img.Buffer[WidthStep + i];
                            }
                        }

                    }
                    catch (Exception err)
                    {
                        throw new Exception(err.Message, err);
                    }
                    return RetByte2DArray;
                }

                ImageBuffer Line_Equalization_Processing(ImageBuffer img, int Cpos)
                {
                    ImageBuffer RetImg = null;

                    int Line_Pixel_count = 0;

                    double Line_PSum = 0;
                    double Line_PAvg = 0;

                    int i, j, m, n;

                    MIL_ID Test_Img = MIL.M_NULL;
                    try
                    {

                        byte[,] ImgByte2DArray_INPUT = ConvertByte1DTo2DArray(img);
                        byte[,] ImgByte2DArray_OUTPUT = new byte[img.SizeY, img.SizeX];


                        if (img.SizeX != img.SizeY)
                        {
                            return RetImg;
                        }

                        MIL.MbufAlloc2d(sys, img.SizeX, img.SizeY, img.ColorDept, iAttributes, ref Test_Img);

                        MIL.MbufPut2d(Test_Img, 0, 0, img.SizeX, img.SizeY, ImgByte2DArray_INPUT);

                        RetImg = new ImageBuffer(img.SizeX, img.SizeY, img.Band, (img.ColorDept * img.Band));

                        int RWidth = img.SizeX - 1;
                        int RHeight = img.SizeY - 1;

                        if (Cpos == 0 || Cpos == 2)
                        {
                            for (i = RWidth; i >= 0; i--)
                            {
                                m = 0;
                                n = 0;

                                Line_PSum = 0;
                                Line_Pixel_count = 0;

                                while (true)
                                {
                                    Line_PSum += ImgByte2DArray_INPUT[m, (i + m)];

                                    Line_Pixel_count++;

                                    if ((i + m) == RWidth)
                                    {
                                        break;
                                    }

                                    m++;

                                }

                                Line_PAvg = Line_PSum / Line_Pixel_count;

                                while (true)
                                {
                                    ImgByte2DArray_OUTPUT[n, (i + n)] = (byte)Line_PAvg;

                                    if ((i + n) == RWidth)
                                    {
                                        break;
                                    }

                                    n++;

                                }

                            }

                            for (j = RHeight; j >= 0; j--)
                            {
                                m = 0;
                                n = 0;

                                Line_PSum = 0;
                                Line_Pixel_count = 0;

                                while (true)
                                {
                                    Line_PSum += ImgByte2DArray_INPUT[(j + m), m];

                                    Line_Pixel_count++;

                                    if ((j + m) == RHeight)
                                    {
                                        break;
                                    }

                                    m++;
                                }

                                Line_PAvg = Line_PSum / Line_Pixel_count;

                                while (true)
                                {
                                    ImgByte2DArray_OUTPUT[(j + n), n] = (byte)Line_PAvg;

                                    if ((j + n) == RHeight)
                                    {
                                        break;
                                    }

                                    n++;

                                }
                            }
                        }
                        else
                        {
                            for (i = 0; i <= RWidth; i++)
                            {
                                m = 0;
                                n = 0;

                                Line_PSum = 0;
                                Line_Pixel_count = 0;

                                while (true)
                                {
                                    Line_PSum += ImgByte2DArray_INPUT[m, (i - m)];

                                    Line_Pixel_count++;

                                    if ((i - m) == 0)
                                    {
                                        break;
                                    }

                                    m++;
                                }

                                Line_PAvg = Line_PSum / Line_Pixel_count;

                                while (true)
                                {
                                    ImgByte2DArray_OUTPUT[n, (i - n)] = (byte)Line_PAvg;

                                    if ((i - n) == 0)
                                    {
                                        break;
                                    }

                                    n++;
                                }

                            }

                            for (j = RHeight; j >= 0; j--)
                            {
                                m = RWidth;
                                n = RWidth;

                                Line_PSum = 0;
                                Line_Pixel_count = 0;

                                while (true)
                                {
                                    Line_PSum += ImgByte2DArray_INPUT[m, j + (RWidth - m)];

                                    Line_Pixel_count++;

                                    if (j + (RWidth - m) == RWidth)
                                    {
                                        break;
                                    }

                                    m--;
                                }

                                Line_PAvg = Line_PSum / Line_Pixel_count;

                                while (true)
                                {
                                    ImgByte2DArray_OUTPUT[n, j + (RWidth - n)] = (byte)Line_PAvg;

                                    if (j + (RWidth - n) == RWidth)
                                    {
                                        break;
                                    }

                                    n--;

                                }
                            }
                        }

                        MIL.MbufPut2d(Test_Img, 0, 0, img.SizeX, img.SizeY, ImgByte2DArray_OUTPUT);
                        MIL.MbufGet(Test_Img, RetImg.Buffer);
                    }
                    catch (Exception err)
                    {
                        throw new Exception(err.Message, err);
                    }
                    finally
                    {
                        if (Test_Img != MIL.M_NULL)
                        {
                            MIL.MbufFree(Test_Img); Test_Img = MIL.M_NULL;
                        }
                    }
                    return RetImg;
                }
            }
        }

       
    }
}
