

namespace ProberInterfaces
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Collections.ObjectModel;
    using System.Windows;
    using Param;
    using ProberInterfaces.Vision;
    using System.Windows.Input;
    using ProberErrorCode;
    using System;
    using System.Runtime.Serialization;
    using LogModule;
    using System.Runtime.CompilerServices;
    using Newtonsoft.Json;
    using System.Drawing.Imaging;
    using System.Threading.Tasks;

    public enum IMAGE_SAVE_TYPE
    {
        BMP,
        JPEG,
        PNG
    }

    public enum IMAGE_LOG_TYPE
    { 
        NORMAL,
        PASS,
        FAIL,
    }

    public enum IMAGE_PROCESSING_TYPE
    {
        FOCUSING,
        PATTERNMATCHING,
    }

    public enum ResizeRatioEnum
    {
        OriginalSize = 1,
        OneHalf = 2,
        OneFourth = 4
    }

    public enum ImageSaveFilter
    {
        [EnumMember]
        OnlyFail,
        [EnumMember]
        OnlyPass,
        [EnumMember]
        All,
        [EnumMember]
        NotUse,
    }

    public interface IVisionManager : IFactoryModule, INotifyPropertyChanged, IModule, IHasSysParameterizable
    {
        //==
        ObservableCollection<IDigitizer> DigitizerService { get; set; }
        System.Windows.Media.Imaging.WriteableBitmap WrbDispImage { get; set; }
        ICameraDescriptor CameraDescriptor { get; }
        //IVisionProcessingParameter VisionProcPrameter { get; }
        //IVisionDigiParameters DigiParameter { get; }
        IMil Mil { get; }
        int MilSystemNumber { get; }
        IVisionProcessing VisionProcessing { get; }
        /// <summary>
        /// VisionFramework 를 사용하는 라이브러리
        /// </summary>
        VisionFramework.IVisionLib VisionLib { get; }
        PatternRegisterStates PRState { get; set; }
        ICamera CurCamera { get; }
        ICommand DragDropVisionFilesCommand { get; }
        ICommand DragOverVisionFilesCommand { get; }
        EnumVisionProcRaft GetVisionProcRaft();
        void SetDisplayChannel(ICamera cam, IDisplayPort port);
        void SettingGrab(EnumProberCam camtype);
        void SetCaller(EnumProberCam cam, object caller);
        EventCodeEnum StartGrab(EnumProberCam type, object caller);
        EventCodeEnum StopGrab(EnumProberCam type);
        EventCodeEnum ImageGrabbed(EnumProberCam camtype, ImageBuffer img);
        EventCodeEnum SwitchCamera(ICameraParameter camparam, object caller, Type declaringtype = null);
        ObservableCollection<GrabDevPosition> Detect_Pad(ImageBuffer _grabedImage, double UserPadSizeX, double UserPadSizeY, ROIParameter roiparam, bool display = true);
        BlobResult Blob(EnumProberCam camtype, BlobParameter blobpatam, ROIParameter roiparam = null,bool fillholes = true, bool invert = false, bool runsort = true);
        ImageBuffer SingleGrab(EnumProberCam type, object assembly);
        void Dispose();
        PMResult PatternMatching(PatternInfomation ptinfo, object callassembly, bool angleretry = false, ImageBuffer img = null, bool display = true,
            int offsetx = 0, int offsety = 0, int sizex = 0, int sizey = 0, bool retryautolight = false, bool retrySuccessedLight = false);
        PMResult PatternMatchingRetry(PatternInfomation ptinfo, bool angleretry = false, int offsetx = 0, int offsety = 0, int sizex = 0, int sizey = 0);
        PMResult SearchPatternMatching(PatternInfomation ptinfo, bool findone = true, bool display = true);
        EventCodeEnum SavePattern(RegisteImageBufferParam prparam);
        EventCodeEnum SavePattern(PatternInfomation prparam, string filePathPrefix = "");
        EventCodeEnum SaveImageBuffer(ImageBuffer image, string path, IMAGE_LOG_TYPE logtype, EventCodeEnum eventcode, int offsetx = 0, int offsety = 0, int width = 0, int height = 0, double rotangle = 0.0);
        EventCodeEnum SaveImageBufferWithRectnagle(ImageBuffer image, string path, IMAGE_LOG_TYPE logtype, EventCodeEnum eventcode, Rect focusROI, int offsetx = 0, int offsety = 0, int width = 0, int height = 0, double rotangle = 0.0);
        EventCodeEnum AddEdgePosBuffer(ImageBuffer image, double x, double y, int width = 0, int height = 0, double rotangle = 0.0);
        ImageBuffer DrawCrosshair(ImageBuffer grabbedImage, Point pt, int length, int thickness = 1);
        ImageBuffer CombineImages(ImageBuffer[] images, int width, int height, int rows, int columns);

        List<ICamera> GetCameras();
        ICamera GetCam(EnumProberCam type);
        DispFlipEnum GetDispHorFlip();
        DispFlipEnum GetDispVerFlip();
        void SetDispHorFlip(DispFlipEnum value);
        void SetDispVerFlip(DispFlipEnum value);
        int GetFocusValue(ImageBuffer img, Rect roi = new Rect());
        EventCodeEnum GetGrayValue(ref ImageBuffer img);
        ImageBuffer LoadImageFile(string filepath);
        ImageBuffer ResizeImageBuffer(ImageBuffer ib, int ScaleFactorX, int ScaleFactorY);
        ImageBuffer ReduceImageSize(ImageBuffer ib, int offsetx = 0, int offsety = 0, int sizex = 0, int sizey = 0);
        EdgeProcResult FindEdgeProcessor(EnumProberCam camtype, bool saveDump = false);
        EdgeProcResult FindPreAlignCenteringEdge(ImageBuffer ib, bool saveDump = false);
        CassetteScanSlotResult CassetteScanProcessing(ImageBuffer ib, CassetteScanSlotParam slotParam, bool saveDump = false);
        ReadOCRResult ReadOCRProcessing(ImageBuffer ocrImage, ReadOCRProcessingParam ocrParams, string font_get_path, bool saveDump = false);
        ReadOCRResult OcrCalibrateFontProcessing(ImageBuffer ocrImage, ReadOCRProcessingParam ocrParams, string font_output_path, bool saveDump = false);
        ImageBuffer Line_Equalization(ImageBuffer img, int Cpos);
        EventCodeEnum GetPatternSize(string path, out Size size);
        void ChangeGrabMode(EnumGrabberMode grabmode);
        EventCodeEnum DisplayProcessing(EnumProberCam camtype, ImageBuffer img);
        ImageBuffer WirteTextToBuffer(List<string> sb, ImageBuffer ib);
        void StopWaitGrab(ICamera cam);
        BlobResult FindBlob(EnumProberCam camtype,
                            ref double ImgPosX,
                            ref double ImgPosY,
                            ref int Foundgraylevel,
                            int Threshold,
                            int BlobAreaLow,
                            int BlobAreaHigh,
                            int OffsetX,
                            int OffsetY,
                            int SizeX,
                            int SizeY,
                            bool isDilation = false,
                            double BlobSizeX = 0,
                            double BlobSizeY = 0,
                            double BlobSizeXMinimumMargin = 0.5,
                            double BlobSizeXMaximumMargin = 0.5,
                            double BlobSizeYMinimumMargin = 0.5,
                            double BlobSizeYMaximumMargin = 0.5,
                            bool AutolightFlag = false);
        BlobResult FindBlobWithRectangularity(EnumProberCam camtype,
                            ref double ImgPosX,
                            ref double ImgPosY,
                            ref int Foundgraylevel,
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
                            bool AutolightFlag = false,
                            double minRectangularity = 0.0);
        EventCodeEnum Crop(ImageBuffer SourceImg, ref ImageBuffer ResultImg, int OffsetX = 0, int OffsetY = 0, int SizeX = 480, int SizeY = 480);
        EventCodeEnum Binarize(EnumProberCam camtype, ref ImageBuffer RelsultImg, int Threshold, int OffsetX, int OffsetY, int SizeX, int SizeY);
        double EdgeFind_IndexAlign(ImageBuffer SrcBuf, int Cpos, int RWidth, int Threshold);
        void LoadImageFromFloder(string folderpath, EnumProberCam camtype);
        void LoadImageFromFileToGrabber(string filepath, EnumProberCam camtype);
        bool ConfirmDigitizerEmulMode(EnumProberCam camtype);
        bool ConfirmContinusGrab(EnumProberCam camtype);
        bool ClearGrabberUserImage(EnumProberCam camtype);
        void SetDisplayChannelStageCameras(IDisplayPort port);
        void SetDisplyChannelLoaderCameras(IDisplayPort port);
        void AllStageCameraStopGrab();
        ImageBuffer GetPatternImageInfo(PatternInfomation ptinfo);
        EventCodeEnum AllocateCamera(EnumProberCam camtype);
        EventCodeEnum DeAllocateCamera(EnumProberCam camtype);
        double GetMaxFocusFlatnessValue();
        double GetFocusFlatnessTriggerValue();
        int GetWaferLowPMDownAcceptance();
        void SetWaferLowPMDownAcceptance(int value);

        void SetParameter(EnumProberCam camtype, string modulename, string path);
        void SetParameters(EnumProberCam camtype, string modulename, string[] paths);

        #region ProberImageController
        ImageSaveFilter imageSaveFilter { get; set; }
        void StartImageCollection(EnumProberModule module, EventCodeEnum targetCode);
        void EndImageCollection();
        (EnumProberModule? currentModuleType, string lastModuleStartTime, string lastHashCode) GetImageDataSetIdentifiers();
        EventCodeEnum GetConnectedNotifyCode();
        ImageDataSet GetImageDataSet(EnumProberModule moduletype, string moduleStartTime, string hashCode);
        void SetImage(ImageBuffer imageBuffer, IMAGE_LOG_TYPE iMAGE_LOG_TYPE, IMAGE_SAVE_TYPE iMAGE_SAVE_TYPE, IMAGE_PROCESSING_TYPE iMAGE_PROCESSING_TYPE, EventCodeEnum eventCodeEnum);
        void SetImages(List<ImageBuffer> imageBuffers, IMAGE_LOG_TYPE iMAGE_LOG_TYPE, IMAGE_SAVE_TYPE iMAGE_SAVE_TYPE, IMAGE_PROCESSING_TYPE iMAGE_PROCESSING_TYPE, EventCodeEnum eventCodeEnum);
        Task SaveModuleImagesAsync(EnumProberModule? enumProberModule);
        #endregion

        bool EnableImageBufferToTextFile { get; set; }
        void ImageBufferFromTextFiles(string inputFolder, string outputFolder = "");
    }

    public abstract class PatternRegisterStates
    {
        public IVisionManager VisionManager;
        public abstract void Run(PatternRegiserParameter prparam);
        public abstract void Pause();
        public abstract void Abort();
    }

    // 각 모듈의 이미지 데이터를 관리하는 클래스
    public class ModuleImageCollection : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private EnumProberModule _ModuleType;
        public EnumProberModule ModuleType
        {
            get { return _ModuleType; }
            set
            {
                if (value != _ModuleType)
                {
                    _ModuleType = value;
                    RaisePropertyChanged();
                }
            }
        }
        
        private List<ImageDataSet> _ImageDataSetList;
        public List<ImageDataSet> ImageDataSetList
        {
            get { return _ImageDataSetList; }
            set
            {
                if (value != _ImageDataSetList)
                {
                    _ImageDataSetList = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ModuleImageCollection(EnumProberModule enumProberModule)
        {
            this.ModuleType = enumProberModule;

            if (ImageDataSetList == null)
            {
                ImageDataSetList = new List<ImageDataSet>();
            }
        }
    }

    // 1회 동작 단위의 데이터가 기록
    [Serializable()]
    [DataContract]
    public class ImageDataSet : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private ObservableCollection<ImageData> _imageDataCollection;
        [DataMember, JsonIgnore]
        public ObservableCollection<ImageData> ImageDataCollection
        {
            get { return _imageDataCollection; }
            set
            {
                if (value != _imageDataCollection)
                {
                    _imageDataCollection = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _imageDatasHashCode = string.Empty;
        [DataMember, JsonIgnore]
        public string ImageDatasHashCode
        {
            get { return _imageDatasHashCode; }
            set
            {
                if (value != _imageDatasHashCode)
                {
                    _imageDatasHashCode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _ModuleStartTime = string.Empty;
        [DataMember, JsonIgnore]
        public string ModuleStartTime
        {
            get { return _ModuleStartTime; }
            set
            {
                if (value != _ModuleStartTime)
                {
                    _ModuleStartTime = value;
                    RaisePropertyChanged();
                }
            }
        }


        public ImageDataSet()
        {
            try
            {
                _imageDataCollection = new ObservableCollection<ImageData>();
                _imageDatasHashCode = SecuritySystem.SecurityUtil.GetHashCode_SHA256(DateTime.Now.Ticks + this.GetHashCode().ToString());
                _ModuleStartTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    [Serializable()]
    [DataContract]
    public class ImageData : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private string _IMAGE_PROCESSING_TYPE;
        [DataMember, JsonIgnore]
        public string IMAGE_PROCESSING_TYPE
        {
            get { return _IMAGE_PROCESSING_TYPE; }
            set
            {
                if (value != _IMAGE_PROCESSING_TYPE)
                {
                    _IMAGE_PROCESSING_TYPE = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<ImageBufferData> _ImageBufferDataCollection;
        [DataMember, JsonIgnore]
        public ObservableCollection<ImageBufferData> ImageBufferDataCollection
        {
            get { return _ImageBufferDataCollection; }
            set
            {
                if (value != _ImageBufferDataCollection)
                {
                    _ImageBufferDataCollection = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ImageData(IMAGE_PROCESSING_TYPE iMAGE_PROCESSING_TYPE)
        {
            this.IMAGE_PROCESSING_TYPE = iMAGE_PROCESSING_TYPE.ToString();
            this.ImageBufferDataCollection = new ObservableCollection<ImageBufferData>();
        }
    }

    [Serializable()]
    [DataContract]
    public class ImageBufferData : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private ImageBuffer _ImageBuffer;
        public ImageBuffer ImageBuffer
        {
            get { return _ImageBuffer; }
            set { _ImageBuffer = value; }
        }

        private byte[] _Buffer;
        [DataMember]
        public byte[] Buffer
        {
            get { return _Buffer; }
            set { _Buffer = value; }
        }

        private DateTime _CapturedTime;
        [DataMember]
        public DateTime CapturedTime
        {
            get { return _CapturedTime; }
            set
            {
                if (value != _CapturedTime)
                {
                    _CapturedTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IMAGE_LOG_TYPE _iMAGE_LOG_TYPE;
        [DataMember, JsonIgnore]
        public IMAGE_LOG_TYPE iMAGE_LOG_TYPE
        {
            get { return _iMAGE_LOG_TYPE; }
            set
            {
                if (value != _iMAGE_LOG_TYPE)
                {
                    _iMAGE_LOG_TYPE = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IMAGE_SAVE_TYPE _iMAGE_SAVE_TYPE;
        [DataMember, JsonIgnore]
        public IMAGE_SAVE_TYPE iMAGE_SAVE_TYPE
        {
            get { return _iMAGE_SAVE_TYPE; }
            set
            {
                if (value != _iMAGE_SAVE_TYPE)
                {
                    _iMAGE_SAVE_TYPE = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EventCodeEnum _EventCode;
        [DataMember, JsonIgnore]
        public EventCodeEnum EventCode
        {
            get { return _EventCode; }
            set
            {
                if (value != _EventCode)
                {
                    _EventCode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _FileName;
        [DataMember, JsonIgnore]
        public string FileName
        {
            get { return _FileName; }
            set
            {
                if (value != _FileName)
                {
                    _FileName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _SavePath;
        [DataMember, JsonIgnore]
        public string SavePath
        {
            get { return _SavePath; }
            set
            {
                if (value != _SavePath)
                {
                    _SavePath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PixelFormat _PixelFormat;
        [DataMember, JsonIgnore]
        public PixelFormat PixelFormat
        {
            get { return _PixelFormat; }
            set
            {
                if (value != _PixelFormat)
                {
                    _PixelFormat = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ImageBufferData(ImageBuffer imageBuffer, IMAGE_LOG_TYPE iMAGE_LOG_TYPE, IMAGE_SAVE_TYPE iMAGE_SAVE_TYPE, EventCodeEnum eventCodeEnum)
        {
            this.ImageBuffer = imageBuffer;

            this.Buffer = imageBuffer.Buffer;
            this.CapturedTime = imageBuffer.CapturedTime;

            this.iMAGE_LOG_TYPE = iMAGE_LOG_TYPE;
            this.iMAGE_SAVE_TYPE = iMAGE_SAVE_TYPE;

            this.EventCode = eventCodeEnum;
            this.PixelFormat = PixelFormat.Format8bppIndexed;
        }
    }
}
