using System;
using System.Linq;
using System.Drawing;

using ProberInterfaces;
using ProberInterfaces.VisionFramework;

using ImageProcessCLR;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("VisionModuleUnitTests")]
namespace ProcessingModule
{
    /// <summary>
    /// Vision Framework 에서 제공하는 Library를 사용하는 기능을 정의합니다.
    /// </summary>
    public class VisionFrameworkLib : IVisionLib, IFactoryModule, IDisposable
    {
        /// <summary>
        /// 이 클래스의 이름
        /// </summary>
        const string VisionFrameworkLibName = "VisionFrameworkLibrary";
        /// <summary>
        /// VisionFramework를 사용하는 이 클래스는 아래 버전을 기준으로 작성
        /// </summary>
        const string VisionFrameworkLibVersion = "1.0.0.11";

        #region Wrapper

        /// <summary>
        /// Recipe 타입. 
        /// </summary>
        enum RecipeType
        {
            /// <summary>
            ///필터명으로 지정하여 사용
            /// </summary>
            Filter,
            /// <summary>
            ///조합된 Recipe를 사용
            /// </summary>
            Recipe
        }

        /// <summary>
        /// 이미지 변환. <br/>
        /// 기본형 : CLR에서 정의한 데이터 타입으로 호출
        /// </summary>
        /// <param name="type">Recipe or Filter (필터 단독호출 여부 선택)</param>
        /// <param name="src">적용할 이미지</param>
        /// <param name="dst">결과 이미지</param>
        /// <param name="recipe">적용할 Recipe 또는 Filter 이름</param>
        /// <param name="param">적용할 파라미터</param>
        /// <returns>산출된 추가 결과</returns>
        private string ImageConvert(RecipeType @type, IEnumerable<_ImageData> src, ref _ImageData[] dst, string recipe, string param = "")
        {
            System.Diagnostics.Debug.WriteLine("Call ImageConvert");
            if (string.IsNullOrWhiteSpace(recipe))
            {
                //Assert Recipe null
                dst = src.ToArray();
                return "";
            }

            switch (@type)
            {
                case RecipeType.Filter:
                    return ImageProcess.FilterRun(src.ToArray(), ref dst, recipe, param);

                case RecipeType.Recipe:
                default:

                    return ImageProcess.RecipeRun(src.ToArray(), ref dst, recipe, param);

            }
        }

        /// <summary>
        /// 이미지 변환 <br/>
        /// Wrapper : 변환 결과 이미지 배열만 받음
        /// </summary>
        /// <param name="type">Recipe or Filter (필터 단독호출 여부 선택)</param>
        /// <param name="src">적용할 이미지</param>
        /// <param name="recipe">적용할 Recipe 또는 Filter 이름</param>
        /// <param name="param">적용할 파라미터</param>
        /// <returns>결과 이미지</returns>
        private (byte[] image, int width, int height) ImageConvert(RecipeType @type, IEnumerable<_ImageData> src, string recipe, out string result, string param = "")
        {
            _ImageData[] dst = new _ImageData[0];

            result = ImageConvert(@type, src, ref dst, recipe, param);

            if (dst.Any())
            {
                return (dst[0].Data, dst[0].width, dst[0].height);
            }

            return (null, 0, 0);
        }

        #endregion

        #region Data Convert
        /// <summary>
        /// 데이터 형식 변환 바이트배열 -> _ImageData
        /// </summary>
        /// <param name="image">이미지 데이터</param>
        /// <param name="width">이미지 가로</param>
        /// <param name="height">이미지 세로</param>
        /// <param name="imageID">이미지 아이디 (기본값 1)</param>
        /// <returns>이미지 데이터</returns>
        private _ImageData Get_ImageData(byte[] image, int width, int height, int imageID = 1)
        {
            ImageProcessCLR._ImageData src = new ImageProcessCLR._ImageData();
            Bitmap img = new System.Drawing.Bitmap(width, height);
            src.channel = 1;
            src.width = img.Width;
            src.height = img.Height;
            src.imgID = imageID;
            src.Data = image;

            return src;

        }
        /// <summary>
        /// 데이터 형식 변환 바이트 배열-> _ImageData[] 
        /// </summary>
        /// <param name="image">이미지 데이터</param>
        /// <param name="width">이미지 가로</param>
        /// <param name="height">이미지 세로</param>
        /// <param name="imageID">이미지 아이디 (기본값 1)</param>
        /// <returns>이미지 배열</returns>
        private _ImageData[] Get_ImageDatas(byte[] image, int width, int height, int imageID = 1)
        {
            return new _ImageData[] { Get_ImageData(image, width, height, imageID) };
        }


        /// <summary>
        /// 이미지 배열을 원하는 크기로 자르거나 확장. <br/>
        /// 1:1 비율로 채워지고 나머지는 영역은 검은색으로. (0x00)        
        /// </summary>
        /// <param name="image"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="targetWidth"></param>
        /// <param name="targetHeight"></param>
        /// <returns></returns>
        private byte[] ResizeImage(byte[] image, int width, int height, int targetWidth, int targetHeight)
        {
            if (width == targetWidth && height == targetHeight)
            {//동일한 크기는 변환 없음.
                return image;
            }

            byte[] buffer = new byte[targetWidth * targetHeight]; //C# default 0

            int copyWidth = Math.Min(width, targetWidth);
            int copyHeight = Math.Min(height, targetHeight);

            for (int y = 0; y < copyHeight; y++)
            {
                for (int x = 0; x < copyWidth; x++)
                {
                    buffer[y * copyWidth + x] = image[y * copyWidth + x];
                }
            }
            return buffer;
        }
        #endregion

        #region By Filter




        #endregion


        #region Recipe Script Cache

        /// <summary>
        /// 기본 사용 레시피
        /// string DefaultPath = this.FileManager().GetSystemParamFullPath(@"Vision\Recipe", "");
        /// </summary>
        FileWatcher DefaultRecipe = new FileWatcher();
        /// <summary>
        /// Device별 별도 적용되는 레시피
        /// string DevicePath = this.FileManager().GetDeviceParamFullPath(@"Vision\Recipe", "");
        /// 
        /// </summary>
        FileWatcher DeviceRecipe = new FileWatcher();


        /// <summary>
        /// 디바이스별 설정 파일 경로 지정. 유닛테스트용. internal로 제한 설정, 실제 사용하지 않음. 
        /// </summary>
        /// <param name="path"></param>
        internal void SetFileManager(Autofac.IContainer container)
        {
            this.SetContainer(container);
        }

        /// <summary>
        /// 감시 폴더 갱신
        /// </summary>
        /// <param name="path"></param>
        private void RecipeUpdate()
        {
            // ex)C:\ProberSystem\Emul_c01\Parameters\DeviceParam\B16A31TXM\Vision\Recipe         
            DefaultRecipe.WatchPath = this.FileManager()?.GetSystemParamFullPath(@"Vision\Recipe", "recipe.json");
            // ex)C:\ProberSystem\Emul_c01\Parameters\SystemParam\Vision\Recipe
            DeviceRecipe.WatchPath = this.FileManager()?.GetDeviceParamFullPath(@"Vision\Recipe", "recipe.json");
        }


        #endregion

        public VisionFrameworkLib()
        {

        }
        ~VisionFrameworkLib()
        {
            this.Dispose();
        }

        #region IDisposable
        public void Dispose()
        {
            DefaultRecipe.Dispose();
            DeviceRecipe.Dispose();
        }
        #endregion

        #region IVisionLib
        //public (byte[], int, int) RecipeExecute(byte[] image, int width, int height, string recipe, string param)
        //{
        //    return ImageConvert(RecipeType.Recipe, Get_ImageDatas(image, width, height), recipe, param);
        //}
        public (byte[] image, int width, int height) RecipeExecute(IEnumerable<(byte[] image, int width, int height, int imageId)> images, string recipe, string param = "")
        {
            return ImageConvert(RecipeType.Recipe, images.Select(x => Get_ImageData(x.image, x.width, x.height, x.imageId)), recipe, out string _, param);
        }
        public (byte[] image, int width, int height) RecipeExecute(IEnumerable<(byte[] image, int width, int height, int imageId)> images, string recipe, out string result, string param = "")
        {
            return ImageConvert(RecipeType.Recipe, images.Select(x => Get_ImageData(x.image, x.width, x.height, x.imageId)), recipe, out result, param);
        }


        public IEnumerable<(byte[] image, int width, int height, string result)> RecipeExecuteMultiResult(IEnumerable<(byte[] image, int width, int height, int imageId)> images, string recipe, string param = "")
        {
            _ImageData[] dst = new _ImageData[0];

            var src = images.Select(x => Get_ImageData(x.image, x.width, x.height, x.imageId));

            string resultMessage = ImageConvert(RecipeType.Recipe, src, ref dst, recipe, param);

            if (dst.Any())
            {
                return dst.Select(d => (d.Data, d.width, d.height, resultMessage));
                
                //return (dst[0].Data, dst[0].width, dst[0].height);
            }

            return Enumerable.Empty<(byte[] image, int width, int height, string result)>();
        }

        public byte[] RecipeExecuteFixedSize(IEnumerable<(byte[] image, int width, int height, int imageId)> images, string recipe, string param = "")
        {
            var ret = RecipeExecute(images, recipe, out string result, param);

            if (images.Count() <= 0)
                return ret.image;
            else
                return ResizeImage(ret.image, ret.width, ret.height, images.ElementAt(0).width, images.ElementAt(0).height);
        }

        public byte[] RecipeExecuteFixedSize(IEnumerable<(byte[] image, int width, int height, int imageId)> images, string recipe, out string result, string param = "")
        {
            var ret = RecipeExecute(images, recipe, out result, param);

            if (images.Count() <= 0)
                return ret.image;
            else
                return ResizeImage(ret.image, ret.width, ret.height, images.ElementAt(0).width, images.ElementAt(0).height);
        }

        public (byte[] image, int width, int height) FilterExecute(byte[] image, int width, int height, string filter, string param = "")
        {
            return ImageConvert(RecipeType.Filter, Get_ImageDatas(image, width, height), filter, out string _, param);
        }

        public byte[] FilterExecuteFixedSize(byte[] image, int width, int height, string filter, string param = "")
        {
            var ret = FilterExecute(image, width, height, filter, param);
            return ResizeImage(ret.Item1, ret.Item2, ret.Item3, width, height);
        }

        /// <summary>
        /// 사전 정의된 레시피. <br/>
        /// 내장된 레시피는 시스템 기본 값. 
        /// </summary>
        /// <param name="recipe"></param>
        /// <returns></returns>
        public string GetReservedRecipe(ReservedRecipe recipe)
        {
            RecipeUpdate();
            switch (recipe)
            {
                case ReservedRecipe.PMI_Normal:
                    //단순 이진화
                    return GetRcipe("PMI_Normal.json", @"{""Name"":""Reserved Recipe PMI_Normal"",""Comment"":""2022-06-13 오후 3:33:26"",""Filters"":[{""Name"":""ThresholdFilter"",""Key"":""1f3fea95b563cdd4"",""Type"":""ThresholdFilter"",""Enable"":true,""IsEndPoint"":true,""Params"":[{""Key"":""Threshold"",""Value"":""127""},{""Key"":""Invert"",""Value"":""false""}],""Pins"":[{""Type"":""IN"",""Name"":""In"",""Link"":{""FilterKey"":""904ee7a79a76018c"",""PinKey"":-1},""Key"":1},{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]},{""Name"":""SourceFilter"",""Key"":""904ee7a79a76018c"",""Type"":""SourceFilter"",""Enable"":true,""IsEndPoint"":false,""Params"":[{""Key"":""ImageID"",""Value"":""1""},{""Key"":""Path"",""Value"":""""}],""Pins"":[{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]}]}");

                case ReservedRecipe.PMI_Processing:
                    //배경 노이즈 제거후 마크 영역에 대한 마스크 이미지, 패드 흰색, 마크 검은색 결과.
                    return GetRcipe("PMI_Processing.json", @"{""Name"":""Reserved Recipe PMI_Processing"",""Comment"":""2022-05-24 오후 5:22:28"",""Filters"":[{""Name"":""SourceFilter"",""Key"":""38915acf73b10e90"",""Type"":""SourceFilter"",""Enable"":true,""IsEndPoint"":false,""Params"":[{""Key"":""ImageID"",""Value"":""1""},{""Key"":""Path"",""Value"":""""}],""Pins"":[{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]},{""Name"":""BackgroundSubstractFilter"",""Key"":""b5d78729b0e121fd"",""Type"":""BackgroundSubstractFilter"",""Enable"":true,""IsEndPoint"":true,""Params"":[{""Key"":""BlurSize"",""Value"":""5""},{""Key"":""ThreshWeight"",""Value"":""3.0""},{""Key"":""CurvedThresh"",""Value"":""false""},{""Key"":""B/W"",""Value"":""false""},{""Key"":""ErodeCount"",""Value"":""0""},{""Key"":""EdgeDetect"",""Value"":""false""}],""Pins"":[{""Type"":""IN"",""Name"":""In"",""Link"":{""FilterKey"":""38915acf73b10e90"",""PinKey"":-1},""Key"":1},{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]}]}");

                case ReservedRecipe.PMI_PatternProcessing:
                    //배경 노이즈 제거후 마크 영역에 대한 마스크 이미지, 패드 흰색, 마크 검은색 결과.
                    return GetRcipe("PMI_PatternProcessing.json", @"{""Name"":""Reserved Recipe PMI_PatternProcessing"",""Comment"":""2022-06-16 오후 4:38:42"",""Filters"":[{""Name"":""SourceFilter"",""Key"":""c63b8db7c80e98af"",""Type"":""SourceFilter"",""Enable"":true,""IsEndPoint"":false,""Params"":[{""Key"":""ImageID"",""Value"":""1""},{""Key"":""Path"",""Value"":""""}],""Pins"":[{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]},{""Name"":""SourceFilter"",""Key"":""39fb566ab63b8457"",""Type"":""SourceFilter"",""Enable"":true,""IsEndPoint"":false,""Params"":[{""Key"":""ImageID"",""Value"":""2""},{""Key"":""Path"",""Value"":""""}],""Pins"":[{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]},{""Name"":""PatternInPadMaskingFilter"",""Key"":""a0263312b0c1af7c"",""Type"":""PatternInPadMaskingFilter"",""Enable"":true,""IsEndPoint"":true,""Params"":[],""Pins"":[{""Type"":""IN"",""Name"":""In"",""Link"":{""FilterKey"":""c63b8db7c80e98af"",""PinKey"":-1},""Key"":1},{""Type"":""IN"",""Name"":""Reference"",""Link"":{""FilterKey"":""39fb566ab63b8457"",""PinKey"":-1},""Key"":2},{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]}]}");
                case ReservedRecipe.SinglePinAlignTipBlob:
                    return GetRcipe("SinglePinAlignTipBlob.json"
                    //~2022-09-28 v1 EnhancedFilter+Otus 이진화.
                    //,@"{""Name"":""Reserved Recipe SinglePinAlignTipBlob"",""Comment"":""2022-04-27 오후 6:06:22"",""Filters"":[{""Name"":""EnhanceFilter"",""Key"":""d204f79a5839d5ac"",""Type"":""EnhanceFilter"",""Enable"":true,""IsEndPoint"":false,""Params"":[{""Key"":""Diretion"",""Value"":""0""},{""Key"":""Sigma"",""Value"":""5.0""},{""Key"":""Scale"",""Value"":""1.0""}],""Pins"":[{""Type"":""IN"",""Name"":""In"",""Link"":{""FilterKey"":""a7a7b77bfdddb9ce"",""PinKey"":-1},""Key"":1},{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]},{""Name"":""EnhanceOtsuFilter"",""Key"":""ed293cc697f4bb87"",""Type"":""EnhanceOtsuFilter"",""Enable"":true,""IsEndPoint"":true,""Params"":[{""Key"":""W/B"",""Value"":""true""},{""Key"":""ABSolute"",""Value"":""127""}],""Pins"":[{""Type"":""IN"",""Name"":""In"",""Link"":{""FilterKey"":""d204f79a5839d5ac"",""PinKey"":-1},""Key"":1},{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]},{""Name"":""SourceFilter"",""Key"":""a7a7b77bfdddb9ce"",""Type"":""SourceFilter"",""Enable"":true,""IsEndPoint"":false,""Params"":[{""Key"":""ImageID"",""Value"":""1""},{""Key"":""Path"",""Value"":""""}],""Pins"":[{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]}]}"
                    //2022-09-28~ v2 EnhancedFilter + Otsu Manual 선택 가능.
                    //, @"{""Name"":""Reserved Recipe SinglePinAlignTipBlob_v2"",""Comment"":""2022-09-28 오후 1:52:34"",""Filters"":[{""Name"":""EnhanceFilter"",""Key"":""d204f79a5839d5ac"",""Type"":""EnhanceFilter"",""Enable"":true,""IsEndPoint"":false,""Params"":[{""Key"":""Diretion"",""Value"":""0""},{""Key"":""Sigma"",""Value"":""5.0""},{""Key"":""Scale"",""Value"":""1.0""}],""Pins"":[{""Type"":""IN"",""Name"":""In"",""Link"":{""FilterKey"":""997b26b74d5dca56"",""PinKey"":-1},""Key"":1},{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]},{""Name"":""EnhanceOtsuFilter"",""Key"":""ed293cc697f4bb87"",""Type"":""EnhanceOtsuFilter"",""Enable"":true,""IsEndPoint"":false,""Params"":[{""Key"":""W/B"",""Value"":""true""},{""Key"":""ABSolute"",""Value"":""127""}],""Pins"":[{""Type"":""IN"",""Name"":""In"",""Link"":{""FilterKey"":""d204f79a5839d5ac"",""PinKey"":-1},""Key"":1},{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]},{""Name"":""SourceFilter"",""Key"":""a7a7b77bfdddb9ce"",""Type"":""SourceFilter"",""Enable"":true,""IsEndPoint"":false,""Params"":[{""Key"":""ImageID"",""Value"":""1""},{""Key"":""Path"",""Value"":""D:\\Image\\BLOB IMAGE\\SINGLEPINBLOB210530235754_PinNo#_1_Threshold_70_ORIGINAL.bmp""}],""Pins"":[{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]},{""Name"":""PinBlobModeSelector"",""Key"":""c753a89104837a81"",""Type"":""PinBlobModeSelector"",""Enable"":true,""IsEndPoint"":true,""Params"":[{""Key"":""AutoMode"",""Value"":""true""}],""Pins"":[{""Type"":""IN"",""Name"":""Auto"",""Link"":{""FilterKey"":""ed293cc697f4bb87"",""PinKey"":-1},""Key"":1},{""Type"":""IN"",""Name"":""Manual"",""Link"":{""FilterKey"":""e6eabe3610f50667"",""PinKey"":-1},""Key"":2},{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]},{""Name"":""ThresholdFilter"",""Key"":""e6eabe3610f50667"",""Type"":""ThresholdFilter"",""Enable"":true,""IsEndPoint"":false,""Params"":[{""Key"":""Threshold"",""Value"":""115""},{""Key"":""Invert"",""Value"":""false""}],""Pins"":[{""Type"":""IN"",""Name"":""In"",""Link"":{""FilterKey"":""d204f79a5839d5ac"",""PinKey"":-1},""Key"":1},{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]},{""Name"":""CropFilter"",""Key"":""997b26b74d5dca56"",""Type"":""CropFilter"",""Enable"":true,""IsEndPoint"":false,""Params"":[{""Key"":""x"",""Value"":""0""},{""Key"":""y"",""Value"":""0""},{""Key"":""width"",""Value"":""0""},{""Key"":""height"",""Value"":""0""}],""Pins"":[{""Type"":""IN"",""Name"":""In"",""Link"":{""FilterKey"":""a7a7b77bfdddb9ce"",""PinKey"":-1},""Key"":1},{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]}]}"
                    //2022-11-08~ v3 EnhancedFilter 기본 Sigma값 5->7
                    //, @"{""Name"":""Reserved Recipe SinglePinAlignTipBlob_v3"",""Comment"":""2022-09-28 오후 1:52:34"",""Filters"":[{""Name"":""EnhanceFilter"",""Key"":""d204f79a5839d5ac"",""Type"":""EnhanceFilter"",""Enable"":true,""IsEndPoint"":false,""Params"":[{""Key"":""Diretion"",""Value"":""0""},{""Key"":""Sigma"",""Value"":""7.0""},{""Key"":""Scale"",""Value"":""1.0""}],""Pins"":[{""Type"":""IN"",""Name"":""In"",""Link"":{""FilterKey"":""997b26b74d5dca56"",""PinKey"":-1},""Key"":1},{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]},{""Name"":""EnhanceOtsuFilter"",""Key"":""ed293cc697f4bb87"",""Type"":""EnhanceOtsuFilter"",""Enable"":true,""IsEndPoint"":false,""Params"":[{""Key"":""W/B"",""Value"":""true""},{""Key"":""ABSolute"",""Value"":""127""}],""Pins"":[{""Type"":""IN"",""Name"":""In"",""Link"":{""FilterKey"":""d204f79a5839d5ac"",""PinKey"":-1},""Key"":1},{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]},{""Name"":""SourceFilter"",""Key"":""a7a7b77bfdddb9ce"",""Type"":""SourceFilter"",""Enable"":true,""IsEndPoint"":false,""Params"":[{""Key"":""ImageID"",""Value"":""1""},{""Key"":""Path"",""Value"":""D:\\Image\\BLOB IMAGE\\SINGLEPINBLOB210530235754_PinNo#_1_Threshold_70_ORIGINAL.bmp""}],""Pins"":[{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]},{""Name"":""PinBlobModeSelector"",""Key"":""c753a89104837a81"",""Type"":""PinBlobModeSelector"",""Enable"":true,""IsEndPoint"":true,""Params"":[{""Key"":""AutoMode"",""Value"":""true""}],""Pins"":[{""Type"":""IN"",""Name"":""Auto"",""Link"":{""FilterKey"":""ed293cc697f4bb87"",""PinKey"":-1},""Key"":1},{""Type"":""IN"",""Name"":""Manual"",""Link"":{""FilterKey"":""e6eabe3610f50667"",""PinKey"":-1},""Key"":2},{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]},{""Name"":""ThresholdFilter"",""Key"":""e6eabe3610f50667"",""Type"":""ThresholdFilter"",""Enable"":true,""IsEndPoint"":false,""Params"":[{""Key"":""Threshold"",""Value"":""115""},{""Key"":""Invert"",""Value"":""false""}],""Pins"":[{""Type"":""IN"",""Name"":""In"",""Link"":{""FilterKey"":""d204f79a5839d5ac"",""PinKey"":-1},""Key"":1},{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]},{""Name"":""CropFilter"",""Key"":""997b26b74d5dca56"",""Type"":""CropFilter"",""Enable"":true,""IsEndPoint"":false,""Params"":[{""Key"":""x"",""Value"":""0""},{""Key"":""y"",""Value"":""0""},{""Key"":""width"",""Value"":""0""},{""Key"":""height"",""Value"":""0""}],""Pins"":[{""Type"":""IN"",""Name"":""In"",""Link"":{""FilterKey"":""a7a7b77bfdddb9ce"",""PinKey"":-1},""Key"":1},{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]}]}"
                    //2023-09-01~ v4 EnhancedFilter 기본 otus filter ABSolute value 127 -> 200
                    , @"{""Name"":""Reserved Recipe SinglePinAlignTipBlob_v3"",""Comment"":""2023-09-01 오후 1:52:34"",""Filters"":[{""Name"":""EnhanceFilter"",""Key"":""d204f79a5839d5ac"",""Type"":""EnhanceFilter"",""Enable"":true,""IsEndPoint"":false,""Params"":[{""Key"":""Diretion"",""Value"":""0""},{""Key"":""Sigma"",""Value"":""7.0""},{""Key"":""Scale"",""Value"":""1.0""}],""Pins"":[{""Type"":""IN"",""Name"":""In"",""Link"":{""FilterKey"":""997b26b74d5dca56"",""PinKey"":-1},""Key"":1},{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]},{""Name"":""EnhanceOtsuFilter"",""Key"":""ed293cc697f4bb87"",""Type"":""EnhanceOtsuFilter"",""Enable"":true,""IsEndPoint"":false,""Params"":[{""Key"":""W/B"",""Value"":""true""},{""Key"":""ABSolute"",""Value"":""200""}],""Pins"":[{""Type"":""IN"",""Name"":""In"",""Link"":{""FilterKey"":""d204f79a5839d5ac"",""PinKey"":-1},""Key"":1},{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]},{""Name"":""SourceFilter"",""Key"":""a7a7b77bfdddb9ce"",""Type"":""SourceFilter"",""Enable"":true,""IsEndPoint"":false,""Params"":[{""Key"":""ImageID"",""Value"":""1""},{""Key"":""Path"",""Value"":""D:\\Image\\BLOB IMAGE\\SINGLEPINBLOB210530235754_PinNo#_1_Threshold_70_ORIGINAL.bmp""}],""Pins"":[{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]},{""Name"":""PinBlobModeSelector"",""Key"":""c753a89104837a81"",""Type"":""PinBlobModeSelector"",""Enable"":true,""IsEndPoint"":true,""Params"":[{""Key"":""AutoMode"",""Value"":""true""}],""Pins"":[{""Type"":""IN"",""Name"":""Auto"",""Link"":{""FilterKey"":""ed293cc697f4bb87"",""PinKey"":-1},""Key"":1},{""Type"":""IN"",""Name"":""Manual"",""Link"":{""FilterKey"":""e6eabe3610f50667"",""PinKey"":-1},""Key"":2},{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]},{""Name"":""ThresholdFilter"",""Key"":""e6eabe3610f50667"",""Type"":""ThresholdFilter"",""Enable"":true,""IsEndPoint"":false,""Params"":[{""Key"":""Threshold"",""Value"":""115""},{""Key"":""Invert"",""Value"":""false""}],""Pins"":[{""Type"":""IN"",""Name"":""In"",""Link"":{""FilterKey"":""d204f79a5839d5ac"",""PinKey"":-1},""Key"":1},{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]},{""Name"":""CropFilter"",""Key"":""997b26b74d5dca56"",""Type"":""CropFilter"",""Enable"":true,""IsEndPoint"":false,""Params"":[{""Key"":""x"",""Value"":""0""},{""Key"":""y"",""Value"":""0""},{""Key"":""width"",""Value"":""0""},{""Key"":""height"",""Value"":""0""}],""Pins"":[{""Type"":""IN"",""Name"":""In"",""Link"":{""FilterKey"":""a7a7b77bfdddb9ce"",""PinKey"":-1},""Key"":1},{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]}]}"
                    );
                case ReservedRecipe.PinTip:
                    return GetRcipe("PinTip.json", @"{""Name"":""Reserved Recipe PinTip"",""Comment"":""2022-04-27 오후 6:06:22"",""Filters"":[{""Name"":""EnhanceFilter"",""Key"":""d204f79a5839d5ac"",""Type"":""EnhanceFilter"",""Enable"":true,""IsEndPoint"":false,""Params"":[{""Key"":""Diretion"",""Value"":""0""},{""Key"":""Sigma"",""Value"":""5.0""},{""Key"":""Scale"",""Value"":""1.0""}],""Pins"":[{""Type"":""IN"",""Name"":""In"",""Link"":{""FilterKey"":""a7a7b77bfdddb9ce"",""PinKey"":-1},""Key"":1},{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]},{""Name"":""EnhanceOtsuFilter"",""Key"":""ed293cc697f4bb87"",""Type"":""EnhanceOtsuFilter"",""Enable"":true,""IsEndPoint"":true,""Params"":[{""Key"":""W/B"",""Value"":""true""},{""Key"":""ABSolute"",""Value"":""127""}],""Pins"":[{""Type"":""IN"",""Name"":""In"",""Link"":{""FilterKey"":""d204f79a5839d5ac"",""PinKey"":-1},""Key"":1},{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]},{""Name"":""SourceFilter"",""Key"":""a7a7b77bfdddb9ce"",""Type"":""SourceFilter"",""Enable"":true,""IsEndPoint"":false,""Params"":[{""Key"":""ImageID"",""Value"":""1""},{""Key"":""Path"",""Value"":""""}],""Pins"":[{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]}]}");
                case ReservedRecipe.WAFER_Edge:
                    return GetRcipe("WAFER_Edge.json", @"{""Name"":""recipe Waferedge"",""Comment"":""2024-07-17 오전 11:36:33"",""Filters"":[{""Name"":""SourceFilter"",""Key"":""08dca39045ba3016"",""Type"":""SourceFilter"",""Enable"":true,""IsEndPoint"":false,""Params"":[{""Key"":""ImageID"",""Value"":""0""},{""Key"":""Path"",""Value"":""""}],""Pins"":[{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]},{""Name"":""SourceFilter"",""Key"":""eafe2687fcad133a"",""Type"":""SourceFilter"",""Enable"":true,""IsEndPoint"":false,""Params"":[{""Key"":""ImageID"",""Value"":""2""},{""Key"":""Path"",""Value"":""""}],""Pins"":[{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]},{""Name"":""SourceFilter"",""Key"":""6dcde08b1b5b04c2"",""Type"":""SourceFilter"",""Enable"":true,""IsEndPoint"":false,""Params"":[{""Key"":""ImageID"",""Value"":""3""},{""Key"":""Path"",""Value"":""""}],""Pins"":[{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]},{""Name"":""SourceFilter"",""Key"":""7950df3d3d938d86"",""Type"":""SourceFilter"",""Enable"":true,""IsEndPoint"":false,""Params"":[{""Key"":""ImageID"",""Value"":""1""},{""Key"":""Path"",""Value"":""""}],""Pins"":[{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]},{""Name"":""WaferEdgeFilter"",""Key"":""6ae1d79c9b3a8ff8"",""Type"":""WaferEdgeFilter"",""Enable"":true,""IsEndPoint"":true,""Params"":[],""Pins"":[{""Type"":""IN"",""Name"":""RT"",""Link"":{""FilterKey"":""08dca39045ba3016"",""PinKey"":-1},""Key"":1},{""Type"":""IN"",""Name"":""LT"",""Link"":{""FilterKey"":""7950df3d3d938d86"",""PinKey"":-1},""Key"":2},{""Type"":""IN"",""Name"":""LB"",""Link"":{""FilterKey"":""eafe2687fcad133a"",""PinKey"":-1},""Key"":3},{""Type"":""IN"",""Name"":""RB"",""Link"":{""FilterKey"":""6dcde08b1b5b04c2"",""PinKey"":-1},""Key"":4},{""Type"":""OUT"",""Name"":""Out"",""Key"":-1}]}]}");
                default: return string.Empty;
            }
        }
        /// <summary>
        /// 1. 디바이스 2. 기본값 폴더에서 지정된 JSON파일을 읽어옴. 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        private string GetRcipe(string filePath, string @default = "")
        {
            //1 Device 파일우선
            string ret = DeviceRecipe.ReadAllText(filePath);

            //2 Default 값 에서 읽어보기
            ret = ret ?? DefaultRecipe.ReadAllText(filePath);

            //3.없으면 기본값.
            ret = ret ?? @default;

            return ret;
        }

        /// <summary>
        /// 사전 정의된 필터
        ///  TODO: 단독으로 사용 정의된 필터 교체를 반영하기 위해 아래 항목을 파일에서 읽어서 처리
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public string GetReservdFilter(ReservedFilter filter)
        {
            RecipeUpdate();

            switch (filter)
            {
                case ReservedFilter.Binary:
                    return GetFilterName("ThresholdFilter", "ThresholdFilter");
                case ReservedFilter.Enhanced:
                    return GetFilterName("EnhanceFilter", "EnhanceFilter");
                case ReservedFilter.EnhancedOtsu:
                    return GetFilterName("EnhanceOtsuFilter", "EnhanceOtsuFilter");
                default: return "";
            }
        }
        /// <summary>
        /// 1. 디바이스 2. 기본값 설정 파일에서 순서로 지정된 키 저장파일을 읽어 지정된 필터 명을 가져옴
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="default">못찾으면 사용될 값</param>
        /// <returns></returns>
        private string GetFilterName(string filter, string @default = "")
        {
            const string FilterDefineFile = @"FilterDefine.json";

            //1 Device 파일우선
            string filterDefineByDevice = DeviceRecipe.ReadAllText(FilterDefineFile);
            string findValue = FindValue(filterDefineByDevice, filter);

            if (findValue == null)
            {//2 Default 에서
                string filterDefineByDefault = DeviceRecipe.ReadAllText(FilterDefineFile);
                findValue = FindValue(filterDefineByDefault, filter);
            }

            if (findValue == null)
            {//3 기본값
                findValue = @default;
            }

            return findValue;
        }

       

        /// <summary>
        /// Json Key:Value 에서 Key를 이용하여 Value를 찾음. <br/>
        /// 1차원적인 문자형에만 맞춰 설계. <br/>
        /// 첫번째 매칭되는 아이템 값만 가져옴,  JSON은 대소문자에 민감하지만, 민감하지 않게 설정함.
        /// </summary>
        /// <param name="contents">json 컨텐츠</param>
        /// <param name="key">찾을 키</param>
        /// <returns></returns>
        public string FindValue(string contents, string key)
        {         
            return VisionFrameworkLibExtend.FindValue(contents, key);
        }

        /// <summary>
        /// Recipe 설정을 초기화 하여 변경된 외부 설정을 다시 읽을수 있도록 함. 
        /// </summary>
        public void RecipeInit()
        {   
            DefaultRecipe.Reset();
            DeviceRecipe.Reset();
        }

        #endregion
    }

    /// <summary>
    /// 비전 프레임워크의 데이터를 변환하는 유틸리티성 정적 함수 집합
    /// </summary>
    public static class VisionFrameworkLibExtend
    {
        /// <summary>
        /// Json Key:Value 에서 Key를 이용하여 Value를 찾음. <br/>
        /// 1차원적인 문자형에만 맞춰 설계. <br/>
        /// 첫번째 매칭되는 아이템 값만 가져옴,  JSON은 대소문자에 민감하지만, 민감하지 않게 설정함.
        /// </summary>
        /// <param name="contents">json 컨텐츠</param>
        /// <param name="key">찾을 키</param>
        /// <returns></returns>
        public static string FindValue(string contents, string key)
        {
            if (string.IsNullOrWhiteSpace(contents))
                return null;

            if (string.IsNullOrWhiteSpace(key))
                return null;

            string findPattern = $"\"{key}\"\\s*:\\s*\"{{0,1}}([^\",}}]+)";
            RegexOptions options = RegexOptions.IgnoreCase;

            Match match = Regex.Match(contents, findPattern, options);

            if (match == null || string.IsNullOrWhiteSpace(match.Value))
                return null;

            return match.Groups[1].Value;
        }
    }

}

