using Autofac;
using FileSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using ProberInterfaces;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;

namespace ProcessingModule.Tests
{

    [TestClass()]
    public class VisionFrameworkLibTests
    {
        #region TestData
        private string SampleThresholdRecipe(int targetThreshold)
        {
            return @"{""Name"":""recipe XX"",""Comment"":""2022 - 04 - 07 오후 4:08:06"",""Filters"":
[{""Name"":""SourceFilter"",""Key"":""e500b5dd66644fc4"",""Type"":""SourceFilter"",""Enable"":true,""IsEndPoint"":false,
""Params"":[{""Key"":""ImageID"",""Value"":""1""},{""Key"":""Path"",""Value"":""""}],
""Pins"":[{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]},
{""Name"":""ThresholdFilter"",""Key"":""b4b659812a1c7e0b"",""Type"":""ThresholdFilter"",""Enable"":true,""IsEndPoint"":true,
""Params"":[{""Key"":""Threshold"",""Value"":""" + $"{targetThreshold}" + @"""},{""Key"":""Invert"",""Value"":""false""}],
""Pins"":[{""Type"":""IN"",""Name"":""In"",""Link"":{""FilterKey"":""e500b5dd66644fc4"",""PinKey"":-1},""Key"":1},{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]}]}";
        }

        //샘플 이미지 데이터셋
        private (byte[], int, int) CreateSampleImageData()
        {
            return (Sample.CreateSampeImage_VerticalGradient(256, 256), 256, 256);
        }

        //세로 그라데이션

        #endregion

        /// <summary>
        /// Recipe를 사용하여 그라데이션 이미지를 넣고 이진화 결과가 기대값 기준으로 나뉘는지 확인
        /// </summary>
        [TestMethod()]
        public void RecipeRunTest()
        {
            /// Process
            ProberInterfaces.VisionFramework.IVisionLib visionLib = new VisionFrameworkLib();

            var sample = CreateSampleImageData();

            int targetThreshold = 127;

            (byte[], int, int, int)[] src = { (sample.Item1, sample.Item2, sample.Item3, 1) };

            var result = visionLib.RecipeExecute(
                src,
                recipe: SampleThresholdRecipe(targetThreshold),
                out string strResult,
                param: "");

            /// Compare Target
            byte[] compare = new byte[sample.Item1.Length];

            Parallel.For(0, compare.Length, (i) =>
            {
                compare[i] = sample.Item1[i] > targetThreshold ? (byte)255 : (byte)0;
            });
            ///Result            
            CollectionAssert.AreEqual(compare, result.Item1);
        }

        /// <summary>
        /// 이진화 필터만 사용하여 그라데이션 이미지를 넣고 이진화 결과가 기대값 기준으로 나뉘는지 확인
        /// </summary>
        [TestMethod()]
        public void FilterRunTest()
        {
            /// Process
            ProberInterfaces.VisionFramework.IVisionLib visionLib = new VisionFrameworkLib();
            var src = CreateSampleImageData();

            int targetThreshold = 127;
            var result = visionLib.FilterExecute(
                 src.Item1, src.Item2, src.Item3,
                filter: "ThresholdFilter", param: $@"{{""Params"":[{{""Key"":""Threshold"", ""Value"":""{targetThreshold}""}},{{""Key"":""Invert"", ""Value"":""False""}}]}}");


            /// Compare Target
            byte[] compare = new byte[src.Item1.Length];

            Parallel.For(0, compare.Length, (i) =>
            {
                compare[i] = src.Item1[i] > targetThreshold ? (byte)255 : (byte)0;
            });

            ///Result
            CollectionAssert.AreEqual(compare, result.Item1);

        }

        /// <summary>
        /// 이미지 변환 과정에서 출력물 크기가 변경 되는경우 테스트
        /// </summary>
        [DataTestMethod]
        [DataRow(0, 0)]
        [DataRow(10, 0)]
        [DataRow(5, 5)]
        public void RecipeExecuteSizeTest(int offsetX, int offsetY)
        {
            var recipeCrop0_0_60_60 = @"{""Name"":""UnitTest Recipe Crop"",""Comment"":""5,5,60,60 2022-04-28 오전 9:40:02"",""Filters"":
[{""Name"":""SourceFilter"",""Key"":""a7a7b77bfdddb9ce"",""Type"":""SourceFilter"",""Enable"":true,""IsEndPoint"":false,""Params"":[{""Key"":""ImageID"",""Value"":""1""},
{""Key"":""Path"",""Value"":""""}],""Pins"":[{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]},
{""Name"":""CropFilter"",""Key"":""206ef170ff7a3f21"",""Type"":""CropFilter"",""Enable"":true,""IsEndPoint"":true,""Params"":
[{""Key"":""x"",""Value"":""" +
offsetX +
@"""},{""Key"":""y"",""Value"":""" +
offsetY +
@"""},{""Key"":""width"",""Value"":""60""},{""Key"":""height"",""Value"":""60""}],""Pins"":[{""Type"":""IN"",""Name"":""In"",
""Link"":{""FilterKey"":""a7a7b77bfdddb9ce"",""PinKey"":-1},""Key"":1},{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]}]}";

            ProberInterfaces.VisionFramework.IVisionLib visionLib = new VisionFrameworkLib();


            int sampleWidth = 256;
            int sampleHeight = 256;
            byte[] sampleImage = Sample.CreateSampeImage_VerticalGradient(sampleWidth, sampleHeight);

            (byte[], int, int, int)[] src = { (sampleImage, sampleWidth, sampleHeight, 1) };
            var result =
                visionLib.RecipeExecute(
                    src
                    , recipe: recipeCrop0_0_60_60
                    , param: "");

            /// Compare Target
            byte[] compare = new byte[60 * 60];

            for (int y = 0, src_y = offsetY; y < 60; y++, src_y++)
            {
                for (int x = 0, src_x = offsetX; x < 60; x++, src_x++)
                {
                    compare[y * 60 + x] = sampleImage[src_y * sampleWidth + src_x];
                }
            }


            ///Result
            Assert.AreEqual(compare.Length, result.Item1.Length);

            CollectionAssert.AreEqual(compare, result.Item1);
        }


        /// <summary>
        /// 이미지 변환 과정에서 출력물 크기가 변경되어도 요청한 사이즈가 유지되는지 확인.
        /// </summary>
        [DataTestMethod]
        [DataRow(0, 0)]
        [DataRow(10, 0)]
        [DataRow(5, 5)]
        public void RecipeExecuteFixedSizeTest(int offsetX, int offsetY)
        {
            var recipeCrop0_0_60_60 = @"{""Name"":""UnitTest Recipe Crop"",""Comment"":""5,5,60,60 2022-04-28 오전 9:40:02"",""Filters"":
[{""Name"":""SourceFilter"",""Key"":""a7a7b77bfdddb9ce"",""Type"":""SourceFilter"",""Enable"":true,""IsEndPoint"":false,""Params"":[{""Key"":""ImageID"",""Value"":""1""},
{""Key"":""Path"",""Value"":""""}],""Pins"":[{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]},
{""Name"":""CropFilter"",""Key"":""206ef170ff7a3f21"",""Type"":""CropFilter"",""Enable"":true,""IsEndPoint"":true,""Params"":
[{""Key"":""x"",""Value"":""" +
offsetX +
@"""},{""Key"":""y"",""Value"":""" +
offsetY +
@"""},{""Key"":""width"",""Value"":""60""},{""Key"":""height"",""Value"":""60""}],""Pins"":[{""Type"":""IN"",""Name"":""In"",""Link"":{""FilterKey"":""a7a7b77bfdddb9ce"",""PinKey"":-1},""Key"":1},{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]}]}";

            ProberInterfaces.VisionFramework.IVisionLib visionLib = new VisionFrameworkLib();


            int sampleWidth = 256;
            int sampleHeight = 256;

            byte[] sampleImage = Sample.CreateSampeImage_VerticalGradient(sampleWidth, sampleHeight);
            (byte[], int, int, int)[] src = { (sampleImage, sampleWidth, sampleHeight, 1) };
            var result =
                visionLib.RecipeExecuteFixedSize(
                    src
                    , recipe: recipeCrop0_0_60_60
                    , param: "");

            /// Compare Target
            byte[] compare = new byte[sampleImage.Length];

            for (int y = 0, src_y = offsetY; y < 60; y++, src_y++)
            {
                for (int x = 0, src_x = offsetX; x < 60; x++, src_x++)
                {
                    compare[y * 60 + x] = sampleImage[src_y * sampleWidth + src_x];
                }
            }


            ///Result
            CollectionAssert.AreEqual(compare, result);


        }

        /// <summary>
        /// ReservedRecipe.SinglePinAlignTipBlob 로 이미지 변환 되는지 확인 + 크기유지
        /// </summary>
        [TestMethod()]
        public void RecipeExecuteFixedSizeNReservedRecipeSinglePinAlignTipBlobTest()
        {
            int sampleWidth = 256;
            int sampleHeight = 256;

            byte[] sampleImage = Sample.CreateSampeImage_VerticalGradient(sampleWidth, sampleHeight);
            (byte[], int, int, int)[] src = { (sampleImage, sampleWidth, sampleHeight, 1) };
            ProberInterfaces.VisionFramework.IVisionLib visionLib = new VisionFrameworkLib();

            /*
             * [{"Name":"SourceFilter","Key":"a7a7b77bfdddb9ce","Result":{"Value":"##EMPTY"}},{"Name":"EnhanceOtsuFilter","Key":"ed293cc697f4bb87","Result":{"threshold":131}},{"Name":"EnhanceFilter","Key":"d204f79a5839d5ac","Result":{"Value":"##EMPTY"}}]
             */

            int targetThreshold = 127;
            bool autoMode = false;

            string recipeParam = $"{{" +
                $" \"ThresholdFilter\" : {{\"Params\":[  {{\"Key\":\"Threshold\", \"Value\":\"{targetThreshold}\"}}]}}" +
                $", \"PinBlobModeSelector\" : {{\"Params\":[  {{\"Key\":\"AutoMode\", \"Value\":\"{autoMode}\"}}]}}" +                 
                $"}}";

            string strResult = "";
            var result =
                visionLib.RecipeExecuteFixedSize(
                    src
                    , visionLib.GetReservedRecipe(ProberInterfaces.VisionFramework.ReservedRecipe.SinglePinAlignTipBlob)
                    ,                    out  strResult
                    , param: recipeParam);
            Assert.AreEqual(sampleImage.Length, result.Length);

            string threshold = VisionFrameworkLibExtend.FindValue(strResult, "threshold");



            //크롭 동작 확인

            int OffsetX = 10;
            int OffsetY = 10;
            int RealSearchAreaX = 100;
            int RealSearchAreaY = 100;

            string recipeCropParam = $"{{" +
                $" \"ThresholdFilter\" : {{\"Params\":[  {{\"Key\":\"Threshold\", \"Value\":\"{targetThreshold}\"}}]}}" +
                $", \"PinBlobModeSelector\" : {{\"Params\":[  {{\"Key\":\"AutoMode\", \"Value\":\"{autoMode}\"}}]}}" +
                 $", \"CropFilter\" : {{\"Params\":[  {{\"Key\":\"x\", \"Value\":\"{OffsetX}\"}},{{\"Key\":\"y\", \"Value\":\"{OffsetY}\"}},{{\"Key\":\"width\", \"Value\":\"{RealSearchAreaX}\"}},{{\"Key\":\"height\", \"Value\":\"{RealSearchAreaY}\"}}]}}" +
                $"}}";

            var resultCrop =
                visionLib.RecipeExecute(
                    src
                    , visionLib.GetReservedRecipe(ProberInterfaces.VisionFramework.ReservedRecipe.SinglePinAlignTipBlob)
                    , out  strResult
                    , param: recipeCropParam);

            Assert.AreEqual(resultCrop.image.Length, RealSearchAreaX* RealSearchAreaY);
            threshold = VisionFrameworkLibExtend.FindValue(strResult, "threshold");
            
            

        }

        [DataTestMethod]
        [DataRow("Sample0.bmp")]
        [DataRow("Sample1.bmp")]
        public void MakeSameResultTestEnhancedOtsu(string testImage)
        {

            string sampleFile = System.IO.Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "TestData",
                testImage);

            Assert.AreEqual(true, System.IO.File.Exists(sampleFile), $"File Not exist : {sampleFile}");

            Image sampleImg = Image.FromFile(sampleFile);

            Assert.IsNotNull(sampleImg);

            int SizeX = sampleImg.Width;
            int SizeY = sampleImg.Height;


            float fSigma = 5.0F;
            float fScaleFactor = 1.0F;


            byte[] imageProcbuffer = ImageToGrayBytes(sampleImg, 0, 0, SizeX, SizeY);

            VisionAlgorithmes visionAlgorithmes = new VisionAlgorithmes();

            var resultEnhanced = visionAlgorithmes.EnhancedImage(new ImageBuffer(imageProcbuffer, SizeX, SizeY, 8), fSigma, fScaleFactor);
            var resultOtsu = visionAlgorithmes.EnhancedOtsuImage(resultEnhanced, true, 127);

            Assert.IsNotNull(resultOtsu.Buffer);
            ProberInterfaces.VisionFramework.IVisionLib visionLib = new VisionFrameworkLib();
            (byte[], int, int, int)[] src = { (imageProcbuffer, SizeX, SizeY, 1) };
            var recipeResult = visionLib.RecipeExecuteFixedSize(
                    src
                    , visionLib.GetReservedRecipe(ProberInterfaces.VisionFramework.ReservedRecipe.SinglePinAlignTipBlob));

            Assert.IsNotNull(recipeResult);

            #region 
            //For Debugger image viewer
            //Bitmap ori = RGBByteStreamtoImage(resultOtsu.Buffer, resultOtsu.Buffer, resultOtsu.Buffer, SizeX, SizeY);
            //Bitmap rcp = RGBByteStreamtoImage(recipeResult, recipeResult, recipeResult, SizeX, SizeY);
            #endregion


            CollectionAssert.AreEqual(resultOtsu.Buffer, recipeResult);
        }
        [TestMethod]
        public void Binarization()
        {

            /// Process
            ProberInterfaces.VisionFramework.IVisionLib visionLib = new VisionFrameworkLib();
            var sampleData = CreateSampleImageData();

            int targetThreshold = 137;
            string thresholdParam = $"{{ \"ThresholdFilter\" : {{\"Params\":[  {{\"Key\":\"Threshold\", \"Value\":\"{targetThreshold}\"}}]}}}}";
            (byte[], int, int, int)[] src = { (sampleData.Item1, sampleData.Item2, sampleData.Item3, 1) };
            var result = visionLib.RecipeExecute(
                src,
                recipe: visionLib.GetReservedRecipe(ProberInterfaces.VisionFramework.ReservedRecipe.PMI_Normal),
                out string strResult,
                param: thresholdParam);



            /// Compare Target
            byte[] compare = new byte[sampleData.Item1.Length];

            Parallel.For(0, compare.Length, (i) =>
            {
                compare[i] = sampleData.Item1[i] > targetThreshold ? (byte)255 : (byte)0;
            });

            ///Result
            CollectionAssert.AreEqual(compare, result.Item1);


            VisionAlgorithmes visionAlgorithmes = new VisionAlgorithmes();
            var vaResult = visionAlgorithmes.Binarization(new ImageBuffer(sampleData.Item1, sampleData.Item2, sampleData.Item3, 8), targetThreshold);

            CollectionAssert.AreEqual(compare, vaResult.Buffer);

        }
        [DataTestMethod]
        [DataRow("PMI_Sample1_F10.bmp","PMI_Sample1_REF.bmp")]
        public void PMI_PatternPad(string testImage, string referenceImage)
        {

            string sampleFile = System.IO.Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "TestData", testImage);
            string referenceFile = System.IO.Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "TestData", referenceImage);

            Assert.AreEqual(true, System.IO.File.Exists(sampleFile), $"File Not exist : {sampleFile}");
            Assert.AreEqual(true, System.IO.File.Exists(referenceFile), $"File Not exist : {referenceFile}");

            Image sampleImg = Image.FromFile(sampleFile);
            Image refImg = Image.FromFile(referenceFile);

            Assert.IsNotNull(sampleImg);
            Assert.IsNotNull(refImg);

            int SizeX = sampleImg.Width;
            int SizeY = sampleImg.Height;

            byte[] imageProcbuffer = ImageToGrayBytes(sampleImg, 0, 0, SizeX, SizeY);
            byte[] refImageBuffer = ImageToGrayBytes(refImg, 0, 0, refImg.Width, refImg.Height);
            
            
            ProberInterfaces.VisionFramework.IVisionLib visionLib = new VisionFrameworkLib();
            (byte[], int, int, int)[] src = { (imageProcbuffer, SizeX, SizeY, 1), (refImageBuffer, refImg.Width, refImg.Height,2) };

            var recipeResult = visionLib.RecipeExecuteFixedSize(
                    src
                    , visionLib.GetReservedRecipe(ProberInterfaces.VisionFramework.ReservedRecipe.PMI_PatternProcessing));

            Assert.IsNotNull(recipeResult);

            #region 
            //For Debugger image viewer
            Bitmap ori = RGBByteStreamtoImage(imageProcbuffer, imageProcbuffer, imageProcbuffer, SizeX, SizeY);

            Bitmap @ref = RGBByteStreamtoImage(refImageBuffer, refImageBuffer, refImageBuffer, refImg.Width, refImg.Height);

            Bitmap rcp = RGBByteStreamtoImage(recipeResult, recipeResult, recipeResult, SizeX, SizeY);

            #endregion

        }
        [DataTestMethod]
        [DataRow("PMI_Sample1_F10.bmp")]
        public void PMI_Processing(string testImage)
        {

            string sampleFile = System.IO.Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "TestData", testImage);
            
            Assert.AreEqual(true, System.IO.File.Exists(sampleFile), $"File Not exist : {sampleFile}");
            

            Image sampleImg = Image.FromFile(sampleFile);
            

            Assert.IsNotNull(sampleImg);
            

            int SizeX = sampleImg.Width;
            int SizeY = sampleImg.Height;

            byte[] imageProcbuffer = ImageToGrayBytes(sampleImg, 0, 0, SizeX, SizeY);
            


            ProberInterfaces.VisionFramework.IVisionLib visionLib = new VisionFrameworkLib();
            (byte[], int, int, int)[] src = { (imageProcbuffer, SizeX, SizeY, 1)};

            var recipeResult = visionLib.RecipeExecuteFixedSize(
                    src
                    , visionLib.GetReservedRecipe(ProberInterfaces.VisionFramework.ReservedRecipe.PMI_Processing));

            Assert.IsNotNull(recipeResult);

            #region 
            //For Debugger image viewer
            Bitmap ori = RGBByteStreamtoImage(imageProcbuffer, imageProcbuffer, imageProcbuffer, SizeX, SizeY);

            Bitmap rcp = RGBByteStreamtoImage(recipeResult, recipeResult, recipeResult, SizeX, SizeY);

            #endregion

        }
        [DataTestMethod]
        [DataRow("EdgeSet1_0.bmp", "EdgeSet1_1.bmp", "EdgeSet1_2.bmp", "EdgeSet1_3.bmp")]
        public void WAFER_Edge_Processing(string edgeImgFile0, string edgeImgFile1, string edgeImgFile2, string edgeImgFile3)
        {

            string edgeImgPath0 = System.IO.Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "TestData", edgeImgFile0);
            string edgeImgPath1 = System.IO.Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "TestData", edgeImgFile1);
            string edgeImgPath2 = System.IO.Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "TestData", edgeImgFile2);
            string edgeImgPath3 = System.IO.Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "TestData", edgeImgFile3);


            Assert.AreEqual(true, System.IO.File.Exists(edgeImgPath0), $"File Not exist : {edgeImgPath0}");
            Assert.AreEqual(true, System.IO.File.Exists(edgeImgPath1), $"File Not exist : {edgeImgPath1}");
            Assert.AreEqual(true, System.IO.File.Exists(edgeImgPath2), $"File Not exist : {edgeImgPath2}");
            Assert.AreEqual(true, System.IO.File.Exists(edgeImgPath3), $"File Not exist : {edgeImgPath3}");

            Image edgeImg0 = Image.FromFile(edgeImgPath0);
            Image edgeImg1 = Image.FromFile(edgeImgPath1);
            Image edgeImg2 = Image.FromFile(edgeImgPath2);
            Image edgeImg3 = Image.FromFile(edgeImgPath3);


            Assert.IsNotNull(edgeImg0);
            Assert.IsNotNull(edgeImg1);
            Assert.IsNotNull(edgeImg2);
            Assert.IsNotNull(edgeImg3);


            byte[] imageBuffer0 = ImageToGrayBytes(edgeImg0, 0, 0, edgeImg0.Width, edgeImg0.Height);
            byte[] imageBuffer1 = ImageToGrayBytes(edgeImg1, 0, 0, edgeImg1.Width, edgeImg1.Height);
            byte[] imageBuffer2 = ImageToGrayBytes(edgeImg2, 0, 0, edgeImg2.Width, edgeImg2.Height);
            byte[] imageBuffer3 = ImageToGrayBytes(edgeImg3, 0, 0, edgeImg3.Width, edgeImg3.Height);



            ProberInterfaces.VisionFramework.IVisionLib visionLib = new VisionFrameworkLib();
            (byte[], int, int, int)[] src = {
                (imageBuffer0, edgeImg0.Width, edgeImg0.Height, 0),
                (imageBuffer1, edgeImg1.Width, edgeImg1.Height, 1) ,
                (imageBuffer2, edgeImg2.Width, edgeImg2.Height, 2),
                (imageBuffer3, edgeImg3.Width, edgeImg3.Height, 3) };

            int startSkip = 10;
            int Mask = 51;
            int grayTolerance = 10;
            bool useDebug = true;

            string waferEdgeFilterParam = $"{{ \"WaferEdgeFilter\" : {{\"Params\":[{{\"Key\":\"StartSkip\",\"Value\":\"{startSkip}\"}},{{\"Key\":\"Mask\",\"Value\":\"{Mask}\"}},{{\"Key\":\"GrayTolerance\",\"Value\":\"{grayTolerance}\"}},{{\"Key\":\"DrawDebug\",\"Value\":\"{useDebug}\"}}]}}}}";

            var recipeResult = visionLib.RecipeExecuteMultiResult(
                    src
                    , visionLib.GetReservedRecipe(ProberInterfaces.VisionFramework.ReservedRecipe.WAFER_Edge), waferEdgeFilterParam);

            Assert.IsNotNull(recipeResult);


            if (recipeResult.Any())
            {
                var waferMergeImage = recipeResult.ElementAt(0);
                if (recipeResult.Count() >= 2)
                {
                    var debugImage1 = recipeResult.ElementAt(1);
                    var debugImage2 = recipeResult.ElementAt(2);
                }

                JObject jObject = JObject.Parse(waferMergeImage.result);

                if (jObject.ContainsKey("Filter"))
                {
                    if (jObject["Filter"] is JArray arr)
                    {
                        foreach (var obj in arr)
                        {
                            JObject filterValue = (JObject)obj;
                            if (string.Compare(filterValue["Name"].ToString(), "WaferEdgeFilter") == 0)
                            {
                                JObject rt = (JObject)filterValue["Result"]?["WaferEdge"]?["RT"];
                                int rtX = rt?["X"]?.Value<int>() ?? 0;
                                int rtY = rt?["Y"]?.Value<int>() ?? 0;

                                JObject lt = (JObject)filterValue["Result"]?["WaferEdge"]?["LT"];
                                int ltX = lt?["X"]?.Value<int>() ?? 0;
                                int ltY = lt?["Y"]?.Value<int>() ?? 0;

                                JObject rb = (JObject)filterValue["Result"]?["WaferEdge"]?["RB"];
                                int rbX = rb?["X"]?.Value<int>() ?? 0;
                                int rbY = rb?["Y"]?.Value<int>() ?? 0;

                                JObject lb = (JObject)filterValue["Result"]?["WaferEdge"]?["LB"];
                                int lbX = lb?["X"]?.Value<int>() ?? 0;
                                int lbY = lb?["Y"]?.Value<int>() ?? 0;


                                string find45 = filterValue["Result"]?["WaferEdge"]?["FIND_RT_LB"]?.Value<string>() ?? "FAIL";
                                string find135 = filterValue["Result"]?["WaferEdge"]?["FIND_LT_RB"]?.Value<string>() ?? "FAIL";
                            }
                        }
                    }
                }
                #region 
                //For Debugger image viewer
                int rcpidx = 0;
                foreach (var resultimage in recipeResult)
                {
                    Bitmap rcp = ByteArrayToBitmap(resultimage.image, resultimage.width, resultimage.height);
                    rcp.Save($"C:\\Logs\\UnitTest_WAFER_Edge_Processing{rcpidx++}.bmp", ImageFormat.Bmp);
                }

                #endregion
            }
        }
        #region Util

        /// <summary>
        /// 회색 변경 컬러 매트릭스
        /// https://web.archive.org/web/20130111215043/http://www.switchonthecode.com/tutorials/csharp-tutorial-convert-a-color-image-to-grayscale
        /// </summary>
        /// <returns></returns>
        public static ImageAttributes GrayscaleAttribute()
        {
            ColorMatrix colorMatrix = new ColorMatrix(
                new float[][]
                {
                    new float[] {.3f, .3f, .3f, 0, 0},
                    new float[] {.59f, .59f, .59f, 0, 0},
                    new float[] {.11f, .11f, .11f, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                });
            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(colorMatrix);
            return attributes;
        }
        /// <summary>
        /// 이미지를 흑백으로 변환한후 8비트 배열로 반환
        /// </summary>
        /// <param name="data"></param>
        /// <param name="roiLeft"></param>
        /// <param name="roiTop"></param>
        /// <param name="roiRight"></param>
        /// <param name="roiBottom"></param>
        /// <param name="graydata"></param>
        /// <returns></returns>
        public static byte[] ImageToGrayBytes(Image data, int roiLeft, int roiTop, int roiRight, int roiBottom)
        {
            int pixelByte = 0;
            int ImageStride = 0;
            byte[] bytedata = ImageToRgbByteStream(data, roiLeft, roiTop, roiRight, roiBottom, GrayscaleAttribute(), out ImageStride, out pixelByte);
            if (bytedata == null)
                return null;
            int numbytes = bytedata.Length;
            int Imageheight = numbytes / ImageStride;

            ///AND Upside down
            byte[] graydata = new byte[(numbytes / pixelByte)];
            int j = 0;
            for (int i = 0; i < bytedata.Length; i += pixelByte)
            {
                graydata[j++] = bytedata[i];
            }

            return graydata;
        }
        /// <summary>
        /// 이미지 데이터를 RGB 바이트 (1픽셀당 32비트) 배열로 변환
        /// </summary>
        /// <param name="image"></param>
        /// <param name="roiLeft"></param>
        /// <param name="roiTop"></param>
        /// <param name="roiRight"></param>
        /// <param name="roiBottom"></param>
        /// <param name="imageAttributes"></param>
        /// <param name="stride"></param>
        /// <param name="pixelByte"></param>
        /// <returns></returns>
        private static byte[] ImageToRgbByteStream(Image image, int roiLeft, int roiTop, int roiRight, int roiBottom, ImageAttributes imageAttributes, out int stride, out int pixelByte)
        {
            stride = 0;
            pixelByte = 0;

            int TextureWidth = roiRight - roiLeft;
            int TextureHeight = roiBottom - roiTop;

            if (TextureWidth < 1 || TextureHeight < 1)
                return null;

            Bitmap bmpRoi = new Bitmap(TextureWidth, TextureHeight, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bmpRoi))
            {
                if (imageAttributes == null)
                {
                    g.DrawImage(image, new Rectangle(0, 0, TextureWidth, TextureHeight), roiLeft, roiTop, TextureWidth, TextureHeight, GraphicsUnit.Pixel);
                }
                else
                {
                    g.DrawImage(image, new Rectangle(0, 0, TextureWidth, TextureHeight), roiLeft, roiTop, TextureWidth, TextureHeight, GraphicsUnit.Pixel, imageAttributes);
                }
                g.Flush();
            }
            return BitmapToByte(bmpRoi, out stride, out pixelByte);
        }
        /// <summary>
        /// 비트맵 형식에서 이미지 바이트 데이터 추출
        /// </summary>
        /// <param name="bmpImage"></param>
        /// <param name="stride"></param>
        /// <param name="pixelByte"></param>
        /// <returns></returns>
        public static byte[] BitmapToByte(Bitmap bmpImage, out int stride, out int pixelByte)
        {
            BitmapData bmpdata = bmpImage.LockBits(new Rectangle(0, 0, bmpImage.Width, bmpImage.Height), ImageLockMode.ReadOnly, bmpImage.PixelFormat);

            int numbytes = bmpdata.Stride * bmpImage.Height;
            byte[] bytedata = new byte[numbytes];
            System.Runtime.InteropServices.Marshal.Copy(bmpdata.Scan0, bytedata, 0, numbytes);
            bmpImage.UnlockBits(bmpdata);
            pixelByte = Image.GetPixelFormatSize(bmpImage.PixelFormat) / 8;


            byte[] retByte = new byte[bmpImage.Height * bmpImage.Width * 3];//bgr color

            for (int y = 0; y < bmpImage.Height; ++y)
            {
                int invHeight = y;
                for (int x = 0; x < bmpImage.Width; x++)
                {
                    retByte[y * bmpImage.Width * 3 + (x * 3)] = bytedata[invHeight * bmpdata.Stride + (x * pixelByte)];
                    retByte[y * bmpImage.Width * 3 + (x * 3) + 1] = bytedata[invHeight * bmpdata.Stride + (x * pixelByte) + 1];
                    retByte[y * bmpImage.Width * 3 + (x * 3) + 2] = bytedata[invHeight * bmpdata.Stride + (x * pixelByte) + 2];
                }
            }
            pixelByte = 3;

            stride = bmpImage.Width * 3;

            return retByte;
        }

        /// <summary>
        ///  이미지 바이트 스트림을 Bitmap로 변환.   
        /// </summary>
        /// <param name="rdata"></param>
        /// <param name="gdata"></param>
        /// <param name="bdata"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Bitmap RGBByteStreamtoImage(byte[] rdata, byte[] gdata, byte[] bdata, int width, int height)
        {
            if (width == 0 || height == 0)
                return null;
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            BitmapData bmpdata = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);

            int bytes = Math.Abs(bmpdata.Stride) * bmp.Height;
            byte[] rgbValues = new byte[bytes];

            System.Runtime.InteropServices.Marshal.Copy(bmpdata.Scan0, rgbValues, 0, bytes);

            int pixelByte = Image.GetPixelFormatSize(bmp.PixelFormat) / 8;

            for (int y = 0; y < height; y++)
            {
                int invHeight = y;
                for (int x = 0; x < width; x++)
                {
                    rgbValues[invHeight * bmpdata.Stride + (x * pixelByte)] = bdata[y * width + x];
                    rgbValues[invHeight * bmpdata.Stride + (x * pixelByte) + 1] = gdata[y * width + x];
                    rgbValues[invHeight * bmpdata.Stride + (x * pixelByte) + 2] = rdata[y * width + x];
                    rgbValues[invHeight * bmpdata.Stride + (x * pixelByte) + 3] = 255;
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, bmpdata.Scan0, bytes);
            bmp.UnlockBits(bmpdata);
            return bmp;
        }

        public static Bitmap ByteArrayToBitmap(byte[] pixelData, int width, int height)
        {   
            if (pixelData.Length != width * height * 3)
            {
                throw new ArgumentException("Invalid pixel data length.");
            }

            Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);


            var rect = new Rectangle(0, 0, width, height);
            var bitmapData = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, bitmap.PixelFormat);


            IntPtr ptr = bitmapData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(pixelData, 0, ptr, pixelData.Length);


            bitmap.UnlockBits(bitmapData);

            return bitmap;
        }


        #endregion

        [TestMethod()]
        public void FindValueTest()
        {
            string sample = @"{ ""A1"": ""1"", ""A2"": ""2"",""B"": ""4"",""binaryFilter"": ""BinaryF"",""Enhanced"": ""EnhancedF"", }";

            Assert.AreEqual("1", VisionFrameworkLibExtend.FindValue(sample, "A1")); //첫번째 항목
            Assert.AreEqual(null, VisionFrameworkLibExtend.FindValue(sample, "A")); // 없는 항목
            Assert.AreEqual("BinaryF", VisionFrameworkLibExtend.FindValue(sample, "binaryFilter")); //4번째 항목
            Assert.AreEqual("BinaryF", VisionFrameworkLibExtend.FindValue(sample, "BinaryFilter")); //4번째 항목 대문자.
        }

        /// <summary>
        /// 비전 레시피 파일이 대체 경로에 있는 레피시 파일의 존재에 따라 우선순위에 맞추어 컨텐츠를 읽는지 확인.
        /// </summary>
        [TestMethod()]
        public void RecipeFileReadTest()
        {
            string systemPath = System.IO.Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "System");
            string devicePath = System.IO.Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Device");

            if (System.IO.Directory.Exists(systemPath))
            {
                System.IO.Directory.Delete(systemPath, true);
            }

            if (System.IO.Directory.Exists(devicePath))
            {
                System.IO.Directory.Delete(devicePath, true);
            }

            //장비 대체 경로 설정
            var filemanagerparam = new FileManagerParam();
            filemanagerparam.SystemParamRootDirectory = systemPath;
            filemanagerparam.DeviceParamRootDirectory = devicePath;
            filemanagerparam.DeviceName = "DEVICEX";


            var builder = new ContainerBuilder();
            builder.RegisterType<FileManager>().As<IFileManager>().As<IFactoryModule>().InstancePerLifetimeScope().SingleInstance().OnActivated((e) => { });
            Autofac.IContainer container = builder.Build();
            var iFileManager = container.Resolve<IFileManager>();
            var fileManager = iFileManager as FileManager;
            fileManager.FileManagerSysParam_IParam = filemanagerparam;

            var visionFramework = new VisionFrameworkLib();
            visionFramework.SetFileManager(container);
            ProberInterfaces.VisionFramework.IVisionLib visionLib = visionFramework;

            //내장 레시피 가져오기
            string recipe = visionLib.GetReservedRecipe(ProberInterfaces.VisionFramework.ReservedRecipe.PMI_Normal);
            Assert.IsNotNull(recipe);
            Assert.IsTrue(recipe.Length > 100); //내장 레시피 컨텐츠 길이는 100자 이상.
            visionLib.RecipeInit();

            //공통 경로에 레시피 생성
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(systemPath, @"Vision\Recipe"));
            System.IO.File.WriteAllText(System.IO.Path.Combine(systemPath, @"Vision\Recipe", "PMI_Normal.json"), "default");

            //공통 경로 레시피 가져오기
            string defaultRecipe = visionLib.GetReservedRecipe(ProberInterfaces.VisionFramework.ReservedRecipe.PMI_Normal);
            Assert.AreEqual("default", defaultRecipe);

            //변경 사항 Init 테스트 
            //공통 경로에 레시피 생성2
            System.IO.File.WriteAllText(System.IO.Path.Combine(systemPath, @"Vision\Recipe", "PMI_Normal.json"), "default2");

            //공통 경로 레시피 가져오기 2
            defaultRecipe = visionLib.GetReservedRecipe(ProberInterfaces.VisionFramework.ReservedRecipe.PMI_Normal);
            Assert.AreEqual("default", defaultRecipe);//이전값이 읽혀야함
            visionLib.RecipeInit();
            defaultRecipe = visionLib.GetReservedRecipe(ProberInterfaces.VisionFramework.ReservedRecipe.PMI_Normal);
            Assert.AreEqual("default2", defaultRecipe);//새로운 값이 읽혀야함


            visionLib.RecipeInit();
            //장비경로에 레시피 생성
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(devicePath, @"DEVICEX\Vision\Recipe"));
            System.IO.File.WriteAllText(System.IO.Path.Combine(devicePath, @"DEVICEX\Vision\Recipe", "PMI_Normal.json"), "device");

            //장비 경로 레시피 가져오기
            string deviceRecipeRecipe = visionLib.GetReservedRecipe(ProberInterfaces.VisionFramework.ReservedRecipe.PMI_Normal);
            Assert.AreEqual("device", deviceRecipeRecipe);


            //변경 사항 Init 테스트 
            //장비경로에 레시피 생성2
            System.IO.File.WriteAllText(System.IO.Path.Combine(devicePath, @"DEVICEX\Vision\Recipe", "PMI_Normal.json"), "device2");

            //장비 경로 레시피 가져오기2
            deviceRecipeRecipe = visionLib.GetReservedRecipe(ProberInterfaces.VisionFramework.ReservedRecipe.PMI_Normal);
            Assert.AreEqual("device", deviceRecipeRecipe); //초기화 이전이므로 이전 데이터가 읽혀야함.
            visionLib.RecipeInit();
            deviceRecipeRecipe = visionLib.GetReservedRecipe(ProberInterfaces.VisionFramework.ReservedRecipe.PMI_Normal);
            Assert.AreEqual("device2", deviceRecipeRecipe); //초기화 했으므로 변경된 내용이 읽혀야함.


            //공통 경로 삭제. - 변화 없음. 장비 경로 레시피 가져와야됨. 
            visionLib.RecipeInit();
            System.IO.Directory.Delete(systemPath, true);            
            Assert.AreEqual(deviceRecipeRecipe, visionLib.GetReservedRecipe(ProberInterfaces.VisionFramework.ReservedRecipe.PMI_Normal));

            //장비 경로 삭제 - 내장 레시피 가져와야됨. 
            visionLib.RecipeInit();
            System.IO.Directory.Delete(devicePath, true);            
            Assert.AreEqual(recipe, visionLib.GetReservedRecipe(ProberInterfaces.VisionFramework.ReservedRecipe.PMI_Normal));
        }
    }
}