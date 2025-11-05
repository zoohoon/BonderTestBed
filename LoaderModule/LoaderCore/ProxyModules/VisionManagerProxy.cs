using System;
using System.Collections.Generic;
using System.Windows;
using Autofac;
using LoaderBase;
using ProberErrorCode;
using ProberInterfaces;
using LoaderParameters;
using LogModule;
using ProberInterfaces.Param;
using ProberInterfaces.Vision;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using ProberInterfaces.VisionFramework;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace LoaderCore
{
    public class VisionManagerProxy : IVisionManagerProxy
    {
        public Autofac.IContainer Container { get; set; }

        public ILoaderModule Loader => Container.Resolve<ILoaderModule>();

        public InitPriorityEnum InitPriority => InitPriorityEnum.LEVEL3;

        public IVisionManager VisionManager { get; set; }

        public bool Initialized { get; set; } = false;
        public ObservableCollection<IDigitizer> DigitizerService { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public System.Windows.Media.Imaging.WriteableBitmap WrbDispImage { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public EventCodeEnum InitModule(Autofac.IContainer container)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    this.Container = container;
                    if (Loader.ServiceType == LoaderServiceTypeEnum.DynamicLinking)
                    {
                        VisionManager = Loader.StageContainer.Resolve<IVisionManager>();

                        retval = EventCodeEnum.NONE;
                    }
                    else
                    {
                        //throw new NotImplementedException();
                    }

                    Initialized = true;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void DeInitModule()
        {
            try
            {
                if (Loader.ServiceType == LoaderServiceTypeEnum.DynamicLinking)
                {
                    //No Works.
                }
                else
                {
                    VisionManager.DeInitModule();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public CassetteScanSlotResult CassetteScanProcessing(ImageBuffer ib, CassetteScanSlotParam slotParams, bool saveDump = false)
        {
            CassetteScanSlotResult result = null;

            try
            {
                result = VisionManager.CassetteScanProcessing(ib, slotParams, saveDump);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return result;
        }

        public EdgeProcResult FindPreAlignCenteringEdge(ImageBuffer ib, bool saveDump = false)
        {
            EdgeProcResult result = null;
            try
            {
                result = VisionManager.FindPreAlignCenteringEdge(ib, saveDump);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return result;
        }

        public ReadOCRResult ReadOCRProcessing(ImageBuffer ocrImage, ReadOCRProcessingParam ocrParams, string font_output_path, bool saveDump = false)
        {
            ReadOCRResult result = null;

            try
            {
                result = VisionManager.ReadOCRProcessing(ocrImage, ocrParams, font_output_path, saveDump);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return result;
        }
        public EventCodeEnum SaveImageBuffer(ImageBuffer image, string path, IMAGE_LOG_TYPE logtype, EventCodeEnum eventcode, int offsetx = 0, int offsety = 0, int width = 0, int height = 0, double rotangle = 0.0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = VisionManager.SaveImageBuffer(image, path, logtype, eventcode);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AddEdgePosBuffer(ImageBuffer img, double x, double y)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = VisionManager.AddEdgePosBuffer(img, x, y);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public ReadOCRResult OcrCalibrateFontProcessing(ImageBuffer ocrImage, ReadOCRProcessingParam ocrParams, string font_output_path, bool saveDump = false)
        {
            ReadOCRResult retVal = null;

            try
            {
                retVal = VisionManager.OcrCalibrateFontProcessing(ocrImage, ocrParams, font_output_path, saveDump);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public System.Windows.Size GetGrabSize(EnumProberCam camType)
        {
            System.Windows.Size size = new System.Windows.Size();

            try
            {
                var cam = VisionManager.GetCam(camType);
                size.Width = cam.GetGrabSizeWidth();
                size.Height = cam.GetGrabSizeHeight();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return size;
        }

        public EventCodeEnum StartGrab(EnumProberCam camType)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = VisionManager.StartGrab(camType, this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum StopGrab(EnumProberCam camType)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = VisionManager.StopGrab(camType);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public ImageBuffer WaitGrab(EnumProberCam camType)
        {
            ImageBuffer imgBuf = null;
            try
            {
                var cam = VisionManager.GetCam(camType);

                bool signaled = VisionManager.DigitizerService[cam.GetDigitizerIndex()].GrabberService.WaitOne(1000);

                signaled = VisionManager.DigitizerService[cam.GetDigitizerIndex()].GrabberService.WaitOne(1000);

                ImageBuffer grabBuf;
                cam.GetCurImage(out grabBuf);

                imgBuf = new ImageBuffer();
                lock (grabBuf)
                {
                    grabBuf.ImageCopyTo(imgBuf);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


            return imgBuf;
        }

        public ImageBuffer SingleGrab(EnumProberCam camType)
        {
            ImageBuffer imgBuf = new ImageBuffer();

            try
            {
                var grabBuf = VisionManager.SingleGrab(camType, this);

                lock (grabBuf)
                {
                    grabBuf.ImageCopyTo(imgBuf);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return imgBuf;
        }

        public void LoadImageFromFileToGrabber(string filepath, EnumProberCam camtype)
        {
            try
            {
                VisionManager.LoadImageFromFileToGrabber(filepath, camtype);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void LoadImageFromFloder(string folderpath, EnumProberCam camtype)
        {
            try
            {
                VisionManager.LoadImageFromFloder(folderpath, camtype);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool ConfirmDigitizerEmulMode(EnumProberCam camtype)
        {
            bool retVal = false;
            try
            {
                retVal = VisionManager.ConfirmDigitizerEmulMode(camtype);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public ICamera GetCam(EnumProberCam camtype)
        {
            return VisionManager.GetCam(camtype);
        }

        public void SetDisplayChannel(ICamera cam, IDisplayPort port)
        {
            VisionManager.SetDisplayChannel(cam, port);
        }

        public List<ICamera> GetCameras()
        {
            return VisionManager.GetCameras();
        }

        public DispFlipEnum GetDispHorFlip()
        {
            return VisionManager.GetDispHorFlip();
        }

        public DispFlipEnum GetDispVerFlip()
        {
            return VisionManager.GetDispVerFlip();
        }

        public void SetDispHorFlip(DispFlipEnum value)
        {
            
        }

        public void SetDispVerFlip(DispFlipEnum value)
        {
            
        }

        public EventCodeEnum InitModule()
        {
            throw new NotImplementedException();
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
        public EventCodeEnum AddEdgePosBuffer(ImageBuffer image, double x = 0, double y = 0, int width = 0, int height = 0, double rotangle = 0)
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

        public string GetImageDataHashCode()
        {
            throw new NotImplementedException();
        }

        public void EndImageCollection()
        {
            throw new NotImplementedException();
        }

        public ImageDataSet GetImageDataSet(EnumProberModule moduletype, string hashCode, string moduleStartTime)
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
