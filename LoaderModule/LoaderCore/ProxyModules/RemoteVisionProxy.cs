using Autofac;
using CameraModule;
using LoaderBase;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using ProberInterfaces.Vision;
using ProberInterfaces.VisionFramework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using VisionParams.Camera;

namespace LoaderCore.ProxyModules
{
    public class RemoteVisionProxy : IVisionManagerProxy, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public InitPriorityEnum InitPriority => InitPriorityEnum.LEVEL3;
        public Autofac.IContainer Container { get; set; }

        public bool Initialized { get; set; } = false;
        public ObservableCollection<IDigitizer> DigitizerService { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public WriteableBitmap WrbDispImage { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ICameraDescriptor CameraDescriptor => throw new NotImplementedException();

        public IMil Mil => throw new NotImplementedException();

        public int MilSystemNumber => throw new NotImplementedException();

        public IVisionProcessing VisionProcessing => throw new NotImplementedException();

        public PatternRegisterStates PRState { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ICamera CurCamera => throw new NotImplementedException();

        public ICommand DragDropVisionFilesCommand => throw new NotImplementedException();

        public ICommand DragOverVisionFilesCommand => throw new NotImplementedException();
        /// <summary>
        /// VisionFramework를 사용하기 위한 인터페이스
        /// </summary>
        public IVisionLib VisionLib => throw new NotImplementedException();
        public bool EnableImageBufferToTextFile { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ImageSaveFilter imageSaveFilter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public CassetteScanSlotResult CassetteScanProcessing(ImageBuffer ib, CassetteScanSlotParam slotParams, bool saveDump = false)
        {
            throw new NotImplementedException();
        }

        public bool ConfirmDigitizerEmulMode(EnumProberCam camtype)
        {
            throw new NotImplementedException();
        }

        public void DeInitModule()
        {
            Initialized = false;
        }

        public EdgeProcResult FindPreAlignCenteringEdge(ImageBuffer ib, bool saveDump = false)
        {
            throw new NotImplementedException();
        }

        public Size GetGrabSize(EnumProberCam camType)
        {
            throw new NotImplementedException();
        }
        //public DispFlipEnum GetDispHorFlip()
        //{
        //    DispFlipEnum ret = DispFlipEnum.NONE;
        //    var communication = Container.Resolve<LoaderBase.Communication.ILoaderCommunicationManager>();

        //    Task.Run(() =>
        //    {
        //        if (communication != null)
        //        {
        //            if (communication.GetRemoteMediumClient() != null)
        //            {
        //                ret = communication.GetRemoteMediumClient().VisionManager().GetDispHorFlip();
        //            }
        //            else
        //            {
        //                ret = DispFlipEnum.NONE;
        //            }
        //        }
        //        else
        //        {
        //            ret = DispFlipEnum.NONE;
        //        }
                
        //    });

        //    return ret;
        //}

        //public DispFlipEnum GetDispVerFlip()
        //{
        //    DispFlipEnum ret = DispFlipEnum.NONE;
        //    var communication = Container.Resolve<LoaderBase.Communication.ILoaderCommunicationManager>();

        //    Task.Run(() =>
        //    {
        //        if (communication != null)
        //        {
        //            if (communication.GetRemoteMediumClient() != null)
        //            {
        //                ret = communication.GetRemoteMediumClient().VisionManager().GetDispVerFlip();
        //            }
        //            else
        //            {
        //                ret = DispFlipEnum.NONE;
        //            }
        //        }
        //        else
        //        {
        //            ret = DispFlipEnum.NONE;
        //        }

        //    });
        //    return ret;
        //}

        public EventCodeEnum InitModule(Autofac.IContainer container)
        {
            Initialized = true;
            Container = container;
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum InitModule()
        {

            return EventCodeEnum.NONE;
        }

        public void LoadImageFromFileToGrabber(string filepath, EnumProberCam camtype)
        {
            throw new NotImplementedException();
        }

        public void LoadImageFromFloder(string folderpath, EnumProberCam camtype)
        {
            throw new NotImplementedException();
        }

        public ReadOCRResult OcrCalibrateFontProcessing(ImageBuffer ocrImage, ReadOCRProcessingParam ocrParams, string font_output_path, bool saveDump = false)
        {
            throw new NotImplementedException();
        }

        public ReadOCRResult ReadOCRProcessing(ImageBuffer ocrImage, ReadOCRProcessingParam ocrParams, string font_in_path, bool saveDump = false)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum SaveImageBuffer(ImageBuffer image, string path, IMAGE_LOG_TYPE logtype, EventCodeEnum eventcode, int offsetx = 0, int offsety = 0, int width = 0, int height = 0, double rotangle = 0.0)
        {
            throw new NotImplementedException();
        }
        public EventCodeEnum AddEdgePosBuffer(ImageBuffer img, double x = 0, double y = 0)
        {
            throw new NotImplementedException();
        }

        public ImageBuffer SingleGrab(EnumProberCam camType)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum StartGrab(EnumProberCam camType)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum StopGrab(EnumProberCam camType)
        {
            return EventCodeEnum.NONE;
        }

        public ImageBuffer WaitGrab(EnumProberCam camType)
        {
            throw new NotImplementedException();
        }
        public ICamera GetCam(EnumProberCam camtype)
        {
            var communication = Container.Resolve<LoaderBase.Communication.ILoaderCommunicationManager>();
            var viewmodelmanager = Container.Resolve<IViewModelManager>();
            Camera cam = new Camera();

            Task.Run(() =>
            {
                cam.Param = new CameraParameter((EnumProberCam)communication.GetProxy<IRemoteMediumProxy>()?.GetCamType());
            });

            return cam;
        }

        public void SetDisplayChannel(ICamera cam, IDisplayPort port)
        {
            return;
        }

        public List<ICamera> GetCameras()
        {
            return new List<ICamera>();
        }

        public EnumVisionProcRaft GetVisionProcRaft()
        {
            throw new NotImplementedException();
        }

        public void SettingGrab(EnumProberCam camtype)
        {
            throw new NotImplementedException();
        }

        public void SetCaller(EnumProberCam cam, object assembly)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum StartGrab(EnumProberCam type, object assembly = null)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum ImageGrabbed(EnumProberCam camtype, ImageBuffer img)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum SwitchCamera(ICameraParameter camparam, object assembly = null, Type declaringtype = null)
        {
            throw new NotImplementedException();
        }

        public ObservableCollection<GrabDevPosition> Detect_Pad(ImageBuffer _grabedImage, double UserPadSizeX, double UserPadSizeY, ROIParameter roiparam, bool display = true)
        {
            throw new NotImplementedException();
        }

        public BlobResult Blob(EnumProberCam camtype, BlobParameter blobpatam, ROIParameter roiparam = null, bool fillholes = true, bool invert = false, bool runsort = true)
        {
            throw new NotImplementedException();
        }

        public ImageBuffer SingleGrab(EnumProberCam type, object assembly = null)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public PMResult PatternMatching(PatternInfomation ptinfo, object callassembly = null, bool angleretry = false, ImageBuffer img = null, bool display = true, int offsetx = 0, int offsety = 0, int sizex = 0, int sizey = 0, bool retryautolight = false, bool retrySuccessedLight = false)
        {
            throw new NotImplementedException();
        }

        public PMResult PatternMatchingRetry(PatternInfomation ptinfo, bool angleretry = false, int offsetx = 0, int offsety = 0, int sizex = 0, int sizey = 0)
        {
            throw new NotImplementedException();
        }

        public PMResult SearchPatternMatching(PatternInfomation ptinfo, bool findone = true, bool display = true)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum SavePattern(RegisteImageBufferParam prparam)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum SaveImageBuffer(ImageBuffer image, string path, int offsetx = 0, int offsety = 0, int width = 0, int height = 0, double rotangle = 0)
        {
            throw new NotImplementedException();
        }
        public EventCodeEnum AddEdgePosBuffer(ImageBuffer img, double x = 0, double y = 0, int width = 0, int height = 0, double rotangle = 0)
        {
            throw new NotImplementedException();
        }

        public int GetFocusValue(ImageBuffer img, Rect roi = default)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum GetGrayValue(ref ImageBuffer img)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum SaveImageBuffer(List<ImageBuffer> imageBufferList)
        {
            throw new NotImplementedException();
        }

        public ImageBuffer LoadImageFile(string filepath)
        {
            throw new NotImplementedException();
        }

        public ImageBuffer ReduceImageSize(ImageBuffer ib, int offsetx = 0, int offsety = 0, int sizex = 0, int sizey = 0)
        {
            throw new NotImplementedException();
        }

        public EdgeProcResult FindEdgeProcessor(EnumProberCam camtype, bool saveDump = false)
        {
            throw new NotImplementedException();
        }

        public ImageBuffer Line_Equalization(ImageBuffer img, int Cpos)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum GetPatternSize(string path, out Size size)
        {
            throw new NotImplementedException();
        }

        public void ChangeGrabMode(EnumGrabberMode grabmode)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum DisplayProcessing(EnumProberCam camtype, ImageBuffer img)
        {
            throw new NotImplementedException();
        }

        public ImageBuffer WirteTextToBuffer(List<string> sb, ImageBuffer ib)
        {
            throw new NotImplementedException();
        }

        public void StopWaitGrab(ICamera cam)
        {
            throw new NotImplementedException();
        }

        public double EdgeFind_IndexAlign(ImageBuffer SrcBuf, int Cpos, int RWidth, int Threshold)
        {
            throw new NotImplementedException();
        }

        public bool ConfirmContinusGrab(EnumProberCam camtype)
        {
            throw new NotImplementedException();
        }

        public bool ClearGrabberUserImage(EnumProberCam camtype)
        {
            throw new NotImplementedException();
        }

        public void SetDisplayChannelStageCameras(IDisplayPort port)
        {
            throw new NotImplementedException();
        }

        public void SetDisplyChannelLoaderCameras(IDisplayPort port)
        {
            throw new NotImplementedException();
        }

        public void AllStageCameraStopGrab()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum LoadSysParameter()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum SaveSysParameter()
        {
            throw new NotImplementedException();
        }

        public ImageBuffer GetPatternImageInfo(PatternInfomation ptinfo)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum Binarize(EnumProberCam camtype, ref ImageBuffer RelsultImg, int Threshold, int OffsetX, int OffsetY, int SizeX, int SizeY)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum AllocateCamera(EnumProberCam camtype)
        {
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum DeAllocateCamera(EnumProberCam camtype)
        {
            return EventCodeEnum.NONE;
        }

        public double GetMaxFocusFlatnessValue()
        {
            throw new NotImplementedException();
        }

        public double GetFocusFlatnessTriggerValue()
        {
            throw new NotImplementedException();
        }

        public int GetWaferLowPMDownAcceptance()
        {
            return -1;
        }

        public void SetWaferLowPMDownAcceptance(int value)
        {
            return;
        }
       
        public DispFlipEnum GetDispHorFlip()
        {
            throw new NotImplementedException();
        }

        public DispFlipEnum GetDispVerFlip()
        {
            throw new NotImplementedException();
        }

        public void SetDispHorFlip(DispFlipEnum value)
        {
            throw new NotImplementedException();
        }

        public void SetDispVerFlip(DispFlipEnum value)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum Crop(ImageBuffer SourceImg, ref ImageBuffer ResultImg, int OffsetX = 0, int OffsetY = 0, int SizeX = 480, int SizeY = 480)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum SavePattern(PatternInfomation prparam, string filePathPrefix = "")
        {
            throw new NotImplementedException();
        }

        public ImageBuffer ResizeImageBuffer(ImageBuffer ib, int ScaleFactorX, int ScaleFactorY)
        {
            throw new NotImplementedException();
        }

        public BlobResult FindBlob(EnumProberCam camtype, ref double ImgPosX, ref double ImgPosY, ref int Foundgraylevel, int Threshold, int BlobAreaLow, int BlobAreaHigh, int OffsetX, int OffsetY, int SizeX, int SizeY, bool isDilation = false, double BlobSizeX = 0, double BlobSizeY = 0, double BlobSizeXMinimumMargin = 0.5, double BlobSizeXMaximumMargin = 0.5, double BlobSizeYMinimumMargin = 0.5, double BlobSizeYMaximumMargin = 0.5, bool AutolightFlag = false)
        {
            throw new NotImplementedException();
        }

        public BlobResult FindBlobWithRectangularity(EnumProberCam camtype, ref double ImgPosX, ref double ImgPosY, ref int Foundgraylevel, int Threshold, int BlobAreaLow, int BlobAreaHigh, int OffsetX, int OffsetY, int SizeX, int SizeY, double BlobSizeX, double BlobSizeY, bool isDilation = false, double BlobSizeXMinimumMargin = 0.5, double BlobSizeXMaximumMargin = 0.5, double BlobSizeYMinimumMargin = 0.5, double BlobSizeYMaximumMargin = 0.5, bool AutolightFlag = false, double minRectangularity = 0)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum SaveImageBufferWithRectnagle(ImageBuffer image, string path, IMAGE_LOG_TYPE logtype, EventCodeEnum eventcode, Rect focusROI, int offsetx = 0, int offsety = 0, int width = 0, int height = 0, double rotangle = 0)
        {
            throw new NotImplementedException();
        }

        public void SetParameter(EnumProberCam camtype, string modulename, string path)
        {
            throw new NotImplementedException();
        }

        public void SetParameters(EnumProberCam camtype, string modulename, string[] paths)
        {
            throw new NotImplementedException();
        }

        public void SaveModuleImages()
        {
            throw new NotImplementedException();
        }

        public void ModuleStart(EnumProberModule module)
        {
            throw new NotImplementedException();
        }

        public void SetImage(ImageBuffer imageBuffer, IMAGE_LOG_TYPE iMAGE_LOG_TYPE, IMAGE_SAVE_TYPE iMAGE_SAVE_TYPE, IMAGE_PROCESSING_TYPE iMAGE_PROCESSING_TYPE, EventCodeEnum eventCodeEnum)
        {
            throw new NotImplementedException();
        }

        public void SetImages(List<ImageBuffer> imageBuffers, IMAGE_LOG_TYPE iMAGE_LOG_TYPE, IMAGE_SAVE_TYPE iMAGE_SAVE_TYPE, IMAGE_PROCESSING_TYPE iMAGE_PROCESSING_TYPE, EventCodeEnum eventCodeEnum)
        {
            throw new NotImplementedException();
        }

        public ImageDataSet GetImageDataSet(EnumProberModule moduletype, string hashCode, string moduleStartTime)
        {
            throw new NotImplementedException();
        }

        public void EndImageCollection()
        {
            throw new NotImplementedException();
        }

        public void StartImageCollection(EnumProberModule module, EventCodeEnum targetCode)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum GetConnectedNotifyCode()
        {
            throw new NotImplementedException();
        }

        public (EnumProberModule? currentModuleType, string lastModuleStartTime, string lastHashCode) GetImageDataSetIdentifiers()
        {
            throw new NotImplementedException();
        }

        public Task SaveModuleImagesAsync()
        {
            throw new NotImplementedException();
        }

        public Task SaveModuleImagesAsync(EnumProberModule? enumProberModule)
        {
            throw new NotImplementedException();
        }

        public ImageBuffer DrawCrosshair(ImageBuffer grabbedImage, Point pt, int length, int thickness = 1)
        {
            throw new NotImplementedException();
        }

        public ImageBuffer CombineImages(ImageBuffer[] images, int width, int height, int rows, int columns)
        {
            throw new NotImplementedException();
        }

        public void ImageBufferFromTextFiles(string inputFolder, string outputFolder = "")
        {
            throw new NotImplementedException();
        }
    }
}
