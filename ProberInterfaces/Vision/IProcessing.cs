using LogModule;
using ProberInterfaces.Param;

namespace ProberInterfaces
{
    using ProberErrorCode;
    using ProberInterfaces.PMI;
    using ProberInterfaces.Vision;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows;

    public enum ProcessFunction
    {
        Blob = 0,
        PatternMatching,
        RegistPmModel
    }

    public enum ARITHMETIC_OPERATION_TYPE
    {
        M_AND = 23,
    }

    public enum ConvolveType
    {
        M_EDGE_DETECT_PREWITT_FAST = 9437192,
        M_EDGE_DETECT_SOBEL_FAST = 9437191,
        M_HORIZONTAL_EDGE_PREWITT = 9437201,
        M_HORIZONTAL_EDGE_SOBEL = 9437199,
        M_LAPLACIAN_4 = 9437185,
        M_LAPLACIAN_8 = 9437186,
        M_SHARPEN_4 = 9437188,
        M_SHARPEN_8 = 9437187,
        M_SMOOTH = 9437184,
        M_VERTICAL_EDGE_PREWITT = 9437202,
        M_VERTICAL_EDGE_SOBEL = 9437200
    }
    public interface IVisionAlgorithmes
    {
        EventCodeEnum Init(int milID, Autofac.IContainer container, long Attributes, long cl_iAttributes);
        ImageBuffer DrawingShape(ImageBuffer CurrentButter, List<Point> pt, ColorDept colordept, bool? fill = false, bool? overlap = false);
        ImageBuffer CropImage(ImageBuffer image, int offset_X, int offset_Y, int width, int height);
        ImageBuffer CopyClipImage(ImageBuffer SrcBuf, ImageBuffer CilpBuf, int offset_X, int offset_Y);
        ImageBuffer MaskingBuffer(ImageBuffer buffer, List<Point> pt);
        ImageBuffer DrawingPadEdge(ImageBuffer buffer, List<Point> pt, bool overlap = true);
        long GetThreshold_ForPadDetection(ImageBuffer SrcBuf);

        ImageBuffer Binarization(ImageBuffer Image, double threshold);
        double GetThreshold_ProbeMark(ImageBuffer CropImage, ImageBuffer maskImage, int PadColor);

        ImageBuffer Arithmethic(ImageBuffer Src1Buf, ImageBuffer Src2Buf, ARITHMETIC_OPERATION_TYPE Operation);
        ImageBuffer ConvolveImage(ImageBuffer image, ConvolveType type);

        ImageBuffer MilDefaultBinarize(ImageBuffer SrcBuf);

        double GetPixelSum(ImageBuffer Image);
        Rect GetRectInFirstBlob(ImageBuffer SrcBuf,
                                                BlobParameter blobparam = null,
                                                ROIParameter roiparam = null,
                                                bool UseAllFeatures = false,
                                                bool foregroundIsZero = false);
        List<Point> GetChainListInFirstBlob(ImageBuffer SrcBuf,
                                                BlobParameter blobparam = null,
                                                ROIParameter roiparam = null,
                                                bool UseAllFeatures = false,
                                                bool foregroundIsZero = false);
        ImageBuffer GetEdgeImageInFirstBlob(ImageBuffer SrcBuf,
                                                BlobParameter blobparam = null,
                                                ROIParameter roiparam = null,
                                                bool UseAllFeatures = false,
                                                bool foregroundIsZero = false);
        EventCodeEnum FindPMIPads(List<PMIPadObject> PadList,
                                            Point MarginPixel,
                                            ImageBuffer SrcBuf,
                                            ImageBuffer SrcBuf_Original,
                                            BlobParameter blobparam = null,
                                            ROIParameter roiparam = null,
                                            bool UseAllFeatures = false,
                                            bool foregroundIsZero = false);
        EventCodeEnum FindMarks(List<PMIPadObject> PadList,
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
                                            bool IsDrawingMarkForOverlay = false);

        BlobResult FindBlobObject(ImageBuffer SrcBuf,
                                    BlobParameter blobparam = null,
                                    ROIParameter roiparam = null,
                                    bool UseAllFeatures = false,
                                    bool foregroundIsZero = false,
                                    bool drawing = false);

        ImageBuffer GetHistogramEqualizationImage(ImageBuffer InputImage, int Mode);

        ImageBuffer GetSynthesisImage(ImageBuffer[] InputImages);
        int GetOtsuThreshold(ImageBuffer curImg);
        ImageBuffer GetEdgeImage(ImageBuffer targetImage);

        ImageBuffer GetPMIResultImage(List<PMIPadObject> PadList, ImageBuffer SrcBuf, ref bool IsPass);

        ImageBuffer EnhancedImage(ImageBuffer Image, float fSigma, float fScaleFactor);
        ImageBuffer EnhancedOtsuImage(ImageBuffer Image, bool bWhite = true, int nThreshHoldLimit = 127);
    }

    public interface IVisionProcessing : IFactoryModule, IModule
    {
        IVisionAlgorithmes Algorithmes { get; }
        ImageBuffer.ImageReadyDelegate ImageProcessing { get; set; }
        ImageBuffer.ImageReadyDelegate ImagePattView { get; set; }
        void InitProcessing(int milSystem);
        void Dispose();
        BlobResult BlobObject(ImageBuffer grabedImage, BlobParameter blobparam, ROIParameter roiparam = null, bool fillholes = true, bool invert = false, bool runsort = true);
        BlobResult BlobColorObject(ImageBuffer _grabedImage, BlobParameter blobparam, bool fillholes = true, bool invert = false, bool runsort = true);
        void ViewResult(ImageBuffer procResultBuffer);
        void ViewResult_Patt(int colorDept, byte[] _PattImageBuff);
        bool IsValidModLicense();
        PMResult PatternMatching(ImageBuffer MilGrabBuffer, ImageBuffer patternImage, PMParameter pmparam, string pattpath, string maskpath, bool angleretry = false, int offsetx = 0, int offsety = 0, int sizex = 480, int sizey = 480);
        PMResult SearchPatternMatching(ImageBuffer img, PatternInfomation ptinfo, bool findone = true);
        void RegistModel(ImageBuffer MilGrabBuffer, int offsetx = 0, int offsety = 0, int PattWidth = 128, int PattHeight = 128, double rotangle = 0.0, string pattFullPath = "", bool isregistMask = true);
        EventCodeEnum SaveImageBuffer(ImageBuffer _grabedImage, string path, IMAGE_LOG_TYPE logtype, EventCodeEnum eventcode, int offsetx = 0, int offsety = 0, int width = 0, int height = 0, double rotangle = 0.0);
        EventCodeEnum SaveImageBufferWithRectnagle(ImageBuffer _grabedImage, string path, IMAGE_LOG_TYPE logtype, EventCodeEnum eventcode, Rect focusROI, int offsetx = 0, int offsety = 0, int width = 0, int height = 0, double rotangle = 0.0);
        EventCodeEnum AddEdgePosBuffer(ImageBuffer _grabedImage, double x = 0, double y = 0, int width = 0, int height = 0, double rotangle = 0.0);
        EventCodeEnum GetGrayLevel(ref ImageBuffer img);
        int GetFocusValue(ImageBuffer img, Rect roi = new Rect());
        int CutSizeConvertEdge(ref ImageBuffer ib);
        ObservableCollection<GrabDevPosition> Detect_Pad(ImageBuffer _grabedImage, double UserPadSizeX, double UserPadSizeY, ROIParameter roiparam, bool fillholes = true);
        EdgeResult EdgeProcessing(ImageBuffer ib);
        int GetIntegralImg(ImageBuffer Ori, ref IntegralImage Result);
        long GetIntegralSum(IntegralImage Input, int x, int y, int bx, int by);

        EdgeProcResult FindEdgeProcessing(ImageBuffer ib, bool saveDump = false);
        int CreatePACLMaskImage(ImageBuffer ib, string path);
        ImageBuffer LoadImageFile(string filepath);

        ImageBuffer ResizeImageBuffer(ImageBuffer ib, int ScaleFactorX, int ScaleFactorY);
        ImageBuffer ReduceImageSize(ImageBuffer ib, int offsetx = 0, int offsety = 0, int sizex = 0, int sizey = 0);

        CassetteScanSlotResult CassetteScanProcessing(ImageBuffer ib, CassetteScanSlotParam slotParams, bool saveDump = false);

        ReadOCRResult ReadOCRProcessing(ImageBuffer ocrImage, ReadOCRProcessingParam ocrParams, string font_get_path, bool saveDump = false);
        ReadOCRResult OcrCalibrateFontProcessing(ImageBuffer ocrImage, ReadOCRProcessingParam ocrParams, string font_output_path, bool saveDump = false);

        ImageBuffer Line_Equalization_Processing(ImageBuffer img, int Cpos);
       
        EventCodeEnum GetPatternSize(string path, out Size size);
        byte[] ConvertToImageBuffer(object image, int sizex, int sizey);

        ImageBuffer WirteTextToBuffer(List<string> sb, ImageBuffer ib);

        BlobResult FindBlob(ImageBuffer _grabedImage,
                    ref double ImgPosX,
                    ref double ImgPosY,
                    int Threshold,
                    int BlobAreaLow,
                    int BlobAreaHigh,
                    int OffsetX,
                    int OffsetY,
                    int SizeX,
                    int SizeY,
                    double BlobSizeX,
                    double BlobSizeY,
                    bool isDilation,
                    double BlobSizeXMinimumMargin = 0.5,
                    double BlobSizeXMaximumMargin = 0.5,
                    double BlobSizeYMinimumMargin = 0.5,
                    double BlobSizeYMaximumMargin = 0.5);

        BlobResult FindBlobWithRectangularity(ImageBuffer _grabedImage,
                            ref double ImgPosX,
                            ref double ImgPosY,
                            int Threshold,
                            int BlobAreaLow,
                            int BlobAreaHigh,
                            int OffsetX,
                            int OffsetY,
                            int SizeX,
                            int SizeY,
                            double BlobSizeX,
                            double BlobSizeY,
                            bool isDilation = false,
                            double BlobSizeXMinimumMargin = 0.5,
                            double BlobSizeXMaximumMargin = 0.5,
                            double BlobSizeYMinimumMargin = 0.5,
                            double BlobSizeYMaximumMargin = 0.5,
                            double minRectangularity = 0.0);

        void Mil_Binarize(ImageBuffer SourceImg, ref ImageBuffer RelsultImg, int Threshold, int OffsetX = 0, int OffsetY = 0, int SizeX = 480, int SizeY = 480);

        void Mil_Crop(ImageBuffer SourceImg, ref ImageBuffer ResultImg, int OffsetX = 0, int OffsetY = 0, int SizeX = 480, int SizeY = 480);

        double EdgeFind_IndexAlign(ImageBuffer SrcBuf, int Cpos, int RWidth, int Threshold);

        List<ModelFinderResult> ModelFind(ImageBuffer targetimg, 
            EnumModelTargetType targettype, EnumForegroundType foreground, 
            Size size, int acceptance, 
            double posx = 0, double posy = 0, double roiwidth = 0, double roiheight = 0, 
            double scale_min = 0, double scale_max = 0, double horthick = 0, double verthick = 0, 
            double smoothness = 70,
            int number = 0);
        List<ModelFinderResult> ModelFind_For_Key(ImageBuffer targetimg,
            EnumModelTargetType targettype, EnumForegroundType foreground,
            Size size, int acceptance,
            double posx = 0, double posy = 0, double roiwidth = 0, double roiheight = 0,
            double scale_min = 0, double scale_max = 0, double horthick = 0, double verthick = 0,
            double smoothness = 70,
            int number = 0);

        ImageBuffer DrawCrosshair(ImageBuffer grabbedImage, Point pt, int length, int thickness = 1);
        ImageBuffer CombineImages(ImageBuffer[] images, int width, int height, int rows, int columns);
    }
    public enum EnumForegroundType
    {
        ANY,
        FOREGROUND_WHITE,
        FOREGROUND_BLACK,
    }
    public enum EnumModelTargetType
    {
        Undefined,
        Circle,
        Ellipse,
        Rectangle,
        Square,
        Cross,
        Ring,
        Triangle,
        Diamond,
        DXF,
    }
    public class ModelFinderResult
    {
        //        Model Index   Time(ms)   Pos X   Pos Y   Scale Angle(°)   Score(%)   Target Score(%)    Height Width   Fit Error   Context ID  NbModel TimeOut
        //              0	0	8.68	478.49	477.44	0.846	268.83	93.51	39.16	84.58	84.58	0.59	201	1	False
        private CatCoordinates _Position;
        public CatCoordinates Position
        {
            get { return _Position; }
            set
            {
                if (value != _Position)
                {
                    _Position = value;
                }
            }
        }

        private double _Angle;
        public double Angle
        {
            get { return _Angle; }
            set
            {
                if (value != _Angle)
                {
                    _Angle = value;
                }
            }
        }
        private double _Score;
        public double Score
        {
            get { return _Score; }
            set
            {
                if (value != _Score)
                {
                    _Score = value;
                }
            }
        }
        private double _Height;
        public double Height
        {
            get { return _Height; }
            set
            {
                if (value != _Height)
                {
                    _Height = value;
                }
            }
        }
        private double _Width;
        public double Width
        {
            get { return _Width; }
            set
            {
                if (value != _Width)
                {
                    _Width = value;
                }
            }
        }

        private double _Radius;
        public double Radius
        {
            get { return _Radius; }
            set
            {
                if (value != _Radius)
                {
                    _Radius = value;
                }
            }
        }

        private EnumModelTargetType _TargetType;
        public EnumModelTargetType TargetType
        {
            get { return _TargetType; }
            set
            {
                if (value != _TargetType)
                {
                    _TargetType = value;
                }
            }
        }

        private ImageBuffer _ResultBuffer;
        public ImageBuffer ResultBuffer
        {
            get { return _ResultBuffer; }
            set
            {
                if (value != _ResultBuffer)
                {
                    _ResultBuffer = value;
                }
            }
        }

        public double AvgPixelValue { get; set; }

        public ModelFinderResult()
        {

        }
        public ModelFinderResult(CatCoordinates pos, double angle, double score, double height, double width, double radius, EnumModelTargetType type)
        {
            Position = pos;
            Angle = angle;
            Score = score;
            Height = height;
            Width = width;
            Radius = radius;
            TargetType = type;
        }
        public ModelFinderResult(CatCoordinates pos, double angle, double score, EnumModelTargetType type)
        {
            Position = pos;
            Angle = angle;
            Score = score;
            TargetType = type;
        }
        public ModelFinderResult(CatCoordinates pos, double radius, double score)
        {
            Position = pos;
            Angle = 0d;
            Score = score;
            Height = radius;
            Width = radius;
            Radius = radius;
            TargetType = EnumModelTargetType.Circle;
        }
        public ModelFinderResult(CatCoordinates pos, double angle, double score, double height, double width)
        {
            Position = pos;
            Angle = angle;
            Score = score;
            Height = height;
            Width = width;
            Radius = -1d;
            TargetType = EnumModelTargetType.Rectangle;
        }
    }

    public class BlobResult
    {
        private ImageBuffer _OriginalBuffer = new ImageBuffer();
        public ImageBuffer OriginalBuffer
        {
            get { return _OriginalBuffer; }
            set { _OriginalBuffer = value; }
        }

        private ImageBuffer _ResultBuffer;

        public ImageBuffer ResultBuffer
        {
            get { return _ResultBuffer; }
            set { _ResultBuffer = value; }
        }

        private ObservableCollection<GrabDevPosition> _DevicePositions = new ObservableCollection<GrabDevPosition>();

        public ObservableCollection<GrabDevPosition> DevicePositions
        {
            get { return _DevicePositions; }
            set { _DevicePositions = value; }
        }

        private double[] _DblCOGX;

        public double[] DblCOGX
        {
            get { return _DblCOGX; }
            set { _DblCOGX = value; }
        }

        private double[] _DblCOGY;

        public double[] DblDOGY
        {
            get { return _DblCOGY; }
            set { _DblCOGY = value; }
        }

        private double[] _DblBox_X_Max;

        public double[] DblBox_X_Max
        {
            get { return _DblBox_X_Max; }
            set { _DblBox_X_Max = value; }
        }
        private double[] _DblBox_X_Min;

        public double[] DblBox_X_Min
        {
            get { return _DblBox_X_Min; }
            set { _DblBox_X_Min = value; }
        }

        private double[] _DblBox_Y_Max;

        public double[] DblBox_Y_Max
        {
            get { return _DblBox_Y_Max; }
            set { _DblBox_Y_Max = value; }
        }

        private double[] _DblBox_Y_Min;

        public double[] DblBox_Y_Min
        {
            get { return _DblBox_Y_Min; }
            set { _DblBox_Y_Min = value; }
        }
        private double[] _DblBlobArea;

        public double[] DblBlobArea
        {
            get { return _DblBlobArea; }
            set { _DblBlobArea = value; }
        }

        public BlobResult(ImageBuffer originalbuffer, ImageBuffer resultbuffer, ObservableCollection<GrabDevPosition> devicePositions)
        {
            try
            {
                OriginalBuffer = originalbuffer;
                ResultBuffer = resultbuffer;
                DevicePositions = devicePositions;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public BlobResult(ImageBuffer resultbuffer, ObservableCollection<GrabDevPosition> devicePositions)
        {
            try
            {
                ResultBuffer = resultbuffer;
                DevicePositions = devicePositions;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public BlobResult()
        {
        }
    }

    public class PMResult
    {

        private EventCodeEnum _RetValue;

        public EventCodeEnum RetValue
        {
            get { return _RetValue; }
            set { _RetValue = value; }
        }

        private ImageBuffer _ResultBuffer;

        public ImageBuffer ResultBuffer
        {
            get { return _ResultBuffer; }
            set { _ResultBuffer = value; }
        }
        private byte[] _PattImage;

        public byte[] PattImage
        {
            get { return _PattImage; }
            set { _PattImage = value; }
        }

        private ObservableCollection<PMResultParameter> _ResultParam;

        public ObservableCollection<PMResultParameter> ResultParam
        {
            get { return _ResultParam; }
            set { _ResultParam = value; }
        }

        private ImageBuffer _FailOriginImageBuffer = new ImageBuffer();
        public ImageBuffer FailOriginImageBuffer
        {
            get { return _FailOriginImageBuffer; }
            set { _FailOriginImageBuffer = value; }
        }

        private ImageBuffer _FailPatternImageBuffer;
        public ImageBuffer FailPatternImageBuffer
        {
            get { return _FailPatternImageBuffer; }
            set { _FailPatternImageBuffer = value; }
        }

        public PMResult()
        {

        }
        public PMResult(ImageBuffer resultbuffer, byte[] pattImage)
        {
            try
            {
                ResultBuffer = resultbuffer;
                PattImage = pattImage;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public PMResult(ImageBuffer resultbuffer, byte[] pattImage,
            ObservableCollection<PMResultParameter> resultparam)
        {
            try
            {
                ResultBuffer = resultbuffer;
                PattImage = pattImage;
                ResultParam = resultparam;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public PMResult(EventCodeEnum retval, ImageBuffer resultbuffer, byte[] pattImage,
           ObservableCollection<PMResultParameter> resultparam)
        {
            try
            {
                RetValue = retval;
                ResultBuffer = resultbuffer;
                PattImage = pattImage;
                ResultParam = resultparam;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public PMResult(EventCodeEnum retval)
        {
            RetValue = retval;
        }

        public void CopyTo(PMResult target)
        {
            try
            {
                target.RetValue = this.RetValue;
                ResultBuffer.CopyTo(target.ResultBuffer);
                if (PattImage != null)
                    Array.Copy(this.PattImage, target.PattImage, PattImage.Length);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

    public class EdgeProcResult
    {
        public Point ImageSize { get; set; }
        public List<Point> Edges { get; set; }
    }

    public class EdgeResult
    {
        public Point EdgePoint { get; set; }
        public EventCodeEnum Result { get; set; }

    }

    public class CassetteScanSlotParam
    {
        public string DumpPrefix { get; set; }

        public string SlotLabel { get; set; }

        public Rect ROI { get; set; }
    }

    public class CassetteScanSlotResult
    {
        public string SlotLabel { get; set; }

        public bool HasEdges { get; set; }

        public Rect ROE { get; set; }
    }

    public class ReadOCRProcessingParam
    {
        public long OcrReadRegionPosX { get; set; }
        public long OcrReadRegionPosY { get; set; }
        public long OcrReadRegionWidth { get; set; }
        public long OcrReadRegionHeight { get; set; }
        public double OcrCharSizeX { get; set; }
        public double OcrCharSizeY { get; set; }
        public double OcrCharSpacing { get; set; }
        public int OcrMaxStringLength { get; set; }
        public double OcrCalibrateMinX { get; set; }
        public double OcrCalibrateMaxX { get; set; }
        public double OcrCalibrateStepX { get; set; }
        public double OcrCalibrateMinY { get; set; }
        public double OcrCalibrateMaxY { get; set; }
        public double OcrCalibrateStepY { get; set; }
        public string OcrSampleString { get; set; }
        public string OcrConstraint { get; set; }
        public double OcrStrAcceptance { get; set; }
        public double OcrCharAcceptance { get; set; }
        public int UserOcrLightType { get; set; }
        public ushort UserOcrLight1_Offset { get; set; }
        public ushort UserOcrLight2_Offset { get; set; }
        public ushort UserOcrLight3_Offset { get; set; }
        public int OcrCalibrationType { get; set; }
        public int OcrMasterFilter { get; set; }
        public int OcrMasterFilterGain { get; set; }
        public int OcrSlaveFilter { get; set; }
        public int OcrSlaveFilterGain { get; set; }
    }

    public class OCRCharacterInfo
    {
        public OCRCharacterInfo(int length)
        {
            this.length = length;

            this.CharPositionX = new double[this.length];
            this.CharPositionY = new double[this.length];
            this.CharScore = new double[this.length];
            this.CharSizeX = new double[this.length];
            this.CharSizeY = new double[this.length];
            this.CharSpacing = new double[this.length];
            this.CharValidFlag = new int[this.length];
        }

        public int length { get; set; }
        public double[] CharPositionX { get; set; }
        public double[] CharPositionY { get; set; }
        public double[] CharScore { get; set; }
        public double[] CharSizeX { get; set; }
        public double[] CharSizeY { get; set; }
        public double[] CharSpacing { get; set; }
        public int[] CharValidFlag { get; set; }
    }


    public class ReadOCRResult
    {
        public bool OCRResultValid { get; set; }
        public string OCRResultStr { get; set; }
        public double OCRResultScore { get; set; }
        public OCRCharacterInfo OCRCharInfo { get; set; }
        public ImageBuffer OCRResultBuf { get; set; }
    }


    //namespace ProberInterfaces.VisionFramework
    /// <summary>
    /// Vision Framework를 사용하는 네임스페이스 정의
    /// </summary>
    namespace VisionFramework
    {
        /// <summary>
        /// 사전 정의된 레시피
        /// </summary>
        public enum ReservedRecipe
        {
            /// <summary>
            /// 단순 이진화
            /// </summary>
            PMI_Normal = 0,
            /// <summary>
            /// 이미지 처리가 추가된 이진화
            /// </summary>
            PMI_Processing,
            /// <summary>
            /// 참조 이미지를 활용한 이미지 처리가 추가된 이진화.
            /// </summary>
            PMI_PatternProcessing,
            SinglePinAlignTipBlob,
            PinTip,
            WAFER_Edge,

        }

        /// <summary>
        /// 사전 정의된 필터
        /// </summary>
        public enum ReservedFilter
        {
            Binary,
            Enhanced,
            EnhancedOtsu
        }

        /// <summary>
        /// Vision Framework lib 사용을 위한 인터페이스 정의
        /// </summary>
        public interface IVisionLib
        {
            /// <summary>
            /// 흑백 이미지를 VisionRecipe 를 사용하여 변환 
            /// </summary>
            /// <param name="image">이미지 데이터</param>
            /// <param name="width">이미지 가로 픽셀 수</param>
            /// <param name="height">이미지 세로 픽셀 수</param>
            /// <param name="recipe">적용할 VisionRecipe (JSON)</param>
            /// <param name="param">수정할 필터 파라미터 값</param>
            /// <returns>(결과이미지, 결과이미지 가로, 결과이미지 세로)</returns>
            (byte[] image, int width, int height) RecipeExecute(IEnumerable<(byte[] image, int width, int height, int imageId)> images, string recipe, string param = "");
            (byte[] image, int width, int height) RecipeExecute(IEnumerable<(byte[] image, int width, int height, int imageId)> images, string recipe, out string result, string param = "");
            IEnumerable<(byte[] image, int width, int height, string result)> RecipeExecuteMultiResult(IEnumerable<(byte[] image, int width, int height, int imageId)> images, string recipe, string param = "");
            /// <summary>
            /// 흑백 이미지를 VisionRecipe 를 사용하여 변환 <br/>
            /// 입력된 이미지 크기와 변환된 결과 이미지 크기는 동일하게 유지 <br/>
            /// 크기가 다른경우 왼쪽 상단 기준으로 확대 또는 축소 <br/>
            /// </summary>
            /// <param name="image">이미지 데이터</param>
            /// <param name="width">이미지 가로 픽셀 수</param>
            /// <param name="height">이미지 세로 픽셀 수</param>
            /// <param name="recipe">적용할 VisionRecipe (JSON)</param>
            /// <param name="param">수정할 필터 파라미터 값</param>
            /// <returns>(결과이미지, 결과이미지 가로, 결과이미지 세로)</returns>
            byte[] RecipeExecuteFixedSize(IEnumerable<(byte[] image, int width, int height, int imageId)> images, string recipe, string param = "");
            byte[] RecipeExecuteFixedSize(IEnumerable<(byte[] image, int width, int height, int imageId)> images, string recipe, out string result, string param = "");

            /// <summary>
            /// 흑백 이미지를 단일 필터에 적용하여 변환 
            /// </summary>
            /// <param name="image">이미지 데이터</param>
            /// <param name="width">이미지 가로 픽셀 수</param>
            /// <param name="height">이미지 세로 픽셀 수</param>
            /// <param name="filter">적용할 필터 이름</param>
            /// <param name="param">수정할 필터 파라미터 값</param>
            /// <returns>(결과이미지, 결과이미지 가로, 결과이미지 세로)</returns>
            (byte[] image, int width, int height) FilterExecute(byte[] image, int width, int height, string filter, string param = "");

            /// <summary>
            /// 흑백 이미지를 단일 필터에 적용하여 변환 
            /// 입력된 이미지 크기와 변환된 결과 이미지 크기는 동일하게 유지 <br/>
            /// 크기가 다른경우 왼쪽 상단 기준으로 확대 또는 축소 <br/>
            /// </summary>
            /// <param name="image">이미지 데이터</param>
            /// <param name="width">이미지 가로 픽셀 수</param>
            /// <param name="height">이미지 세로 픽셀 수</param>
            /// <param name="filter">적용할 필터 이름</param>
            /// <param name="param">수정할 필터 파라미터 값</param>
            /// <returns>(결과이미지, 결과이미지 가로, 결과이미지 세로)</returns>
            byte[] FilterExecuteFixedSize(byte[] image, int width, int height, string filter, string param = "");

            /// <summary>
            /// 미리 정의된 VisionRecipe.
            /// </summary>
            /// <param name="recipe"></param>
            /// <returns>사전 작성된 Recipe (JSON)</returns>
            string GetReservedRecipe(ReservedRecipe recipe);
            /// <summary>
            /// 미리 정의된 필터 이름
            /// </summary>
            /// <param name="recipe"></param>
            /// <returns> 지정된 필터 이름</returns>
            string GetReservdFilter(ReservedFilter filter);
            /// <summary>
            /// Json 파서를 이용하여 특정 키의 값을 읽음
            /// </summary>
            /// <param name="contents"></param>
            /// <param name="key"></param>
            /// <returns></returns>
            string FindValue(string contents, string key);
            /// <summary>
            /// 런타임중에 변경된 설정을 초기화 하여 변경사항이 반영 될수 있도록함.
            /// 외부 파일이 변경된 경우에 호출 되어야 변경사항이 반영됨.
            /// </summary>
            void RecipeInit();
        }
    }
}
