using LoaderBase.Communication;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace LoaderServiceClientModules.VisionManager
{
    using System.Collections.ObjectModel;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Autofac;
    using LoaderBase.FactoryModules.ServiceClient;
    using LoaderBase.FactoryModules.ViewModelModule;
    using LogModule;
    using ProberInterfaces.Vision;
    using ProberInterfaces.VisionFramework;

    public class VisionManagerServiceClient : IVisionManagerServiceClient, INotifyPropertyChanged,IFactoryModule, ILoaderFactoryModule
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]String info = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        //private Autofac.IContainer _Container;
        private ILoaderCommunicationManager LoaderCommunicationManager
        {
            get
            {
                return this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
            }
        }
        public bool Initialized 
        {
            get
            {
                if (LoaderCommunicationManager.SelectedStage != null)
                {
                    if (LoaderCommunicationManager.SelectedStage.StageInfo.IsConnected == true)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            set { }
        }

        public ObservableCollection<IDigitizer> DigitizerService { get; set; }
        public WriteableBitmap WrbDispImage { get; set; }

        public ICameraDescriptor CameraDescriptor { get; set; }

        public IMil Mil { get; set; }

        public int MilSystemNumber { get; set; }

        public IVisionProcessing VisionProcessing { get; set; }

        public PatternRegisterStates PRState { get; set; }

        public ICamera CurCamera { get; set; }

        

        public ICommand DragDropVisionFilesCommand { get; set; }

        public ICommand DragOverVisionFilesCommand { get; set; }

        public InitPriorityEnum InitPriority { get; set; }

        private MachineCoordinate _GrabCoord = new MachineCoordinate();
        public MachineCoordinate GrabCoord
        {
            get { return _GrabCoord; }
            set
            {
                if (value != _GrabCoord)
                {
                    _PreGrabCoord = _GrabCoord;
                    _GrabCoord = value;
                }
            }
        }
        private MachineCoordinate _PreGrabCoord = new MachineCoordinate();
        public MachineCoordinate PreGrabCoord
        {
            get { return _PreGrabCoord; }
            set
            {
                if (value != _PreGrabCoord)
                {
                    _PreGrabCoord = value;
                    RaisePropertyChanged();
                }
            }
        }

        public IVisionLib VisionLib => throw new NotImplementedException();

        public bool EnableImageBufferToTextFile { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ImageSaveFilter imageSaveFilter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        ImageBuffer dispImage;
        ProberInterfaces.Param.CatCoordinates precoord = new ProberInterfaces.Param.CatCoordinates();
        private ProberInterfaces.Param.CatCoordinates precatcoord = new ProberInterfaces.Param.CatCoordinates();

        private object _DisplayLock = new object();
        public EnumVisionProcRaft GetVisionProcRaft()
        {
            return EnumVisionProcRaft.EMUL;
        }

        public void SetDisplayChannel(ICamera cam, IDisplayPort port)
        {
            try
            {
                cam = (this.ViewModelManager() as ILoaderViewModelManager).Camera;
                if(cam != null)
                {
                    lock(_DisplayLock)
                    {
                        cam.DisplayService.DispPorts.Clear();
                        cam.DisplayService.DispPorts.Add(port);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public Task DispHostService_ImageUpdate(ICamera Camera, ImageBuffer image)
        {
            try
            {
                if (dispImage == null)
                {
                    dispImage = new ImageBuffer();
                }

                #region //..Overlay
                //await System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                //{
                Application.Current.Dispatcher.Invoke(() =>
                {
                    lock (image)
                    {
                        if (image.CatCoordinates != null)
                        {
                            GrabCoord = new MachineCoordinate(
                               Math.Round(image.CatCoordinates.GetX(), 0),
                               Math.Round(image.CatCoordinates.GetY(), 0),
                               Math.Round(image.CatCoordinates.GetZ(), 0),
                               Math.Round(image.CatCoordinates.GetT(), 0));
                        }

                        if (GrabCoord.GetX() != PreGrabCoord.GetX() ||
                             GrabCoord.GetY() != PreGrabCoord.GetY() ||
                             GrabCoord.GetT() != PreGrabCoord.GetT() || image.UpdateOverlayFlag || Camera.DisplayService.needUpdateOverlayCanvasSize)
                        {
                            DrawOverlayCanvas(Camera, image);
                            if (Camera.DisplayService.needUpdateOverlayCanvasSize)
                                Camera.DisplayService.needUpdateOverlayCanvasSize = false;
                        }
                        else
                        {
                            //if (image.DrawOverlayContexts.Count != 0 & image.DrawOverlayContexts.Count != Camera.DisplayService.OverlayCanvas.Children.Count)
                            //{
                            //    DrawOverlayCanvas(image);
                            //}
                            //if(image.DrawOverlayContexts.Count != 0)
                            //{
                            //    Camera.DisplayService.OverlayCanvas.UpdateLayout();
                            //}
                        }
                    }
                });

                //}));

                #endregion

                image.CopyTo(dispImage);
                //System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                //{
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        lock (_DisplayLock)
                        {
                            foreach (var displayport in Camera.DisplayService.DispPorts)
                            {
                                if (displayport != null)
                                {
                                    displayport.SetImage(Camera, dispImage);
                                }
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                });
                    
                //}));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        private void DrawOverlayCanvas(ICamera Camera, ImageBuffer image)
        {
            try
            {
                //Camera.DisplayService.OverlayCanvas.Children.Clear();

                if (image.DrawOverlayContexts.Count != 0)
                {
                    //Camera.DisplayService.DrawOverlayContexts.Clear();

                    foreach (var drawable in image.DrawOverlayContexts)
                    {
                        if (drawable is IControlDrawable)
                        {
                            if ((drawable as IControlDrawable).StringTypeColor != null)
                                (drawable as IControlDrawable).Color = (Color)ColorConverter.ConvertFromString((drawable as IControlDrawable).StringTypeColor);
                        }
                        else if (drawable is ITextDrawable)
                        {
                            if ((drawable as ITextDrawable).StringTypeFontColor != null)
                                (drawable as ITextDrawable).Fontcolor = (Color)ColorConverter.ConvertFromString((drawable as ITextDrawable).StringTypeFontColor);
                            if ((drawable as ITextDrawable).StringTypeBackColor != null)
                                (drawable as ITextDrawable).BackColor = (Color)ColorConverter.ConvertFromString((drawable as ITextDrawable).StringTypeBackColor);
                        }
                    }

                    Camera.DisplayService.DrawOverlayContexts = image.DrawOverlayContexts;
                    Camera.DisplayService.Draw(image);
                    Camera.DisplayService.OverlayCanvas.UpdateLayout();
                }
                else
                {
                    Camera.DisplayService.OverlayCanvas.Children.Clear();
                    Camera.DisplayService.OverlayCanvas.UpdateLayout();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SettingGrab(EnumProberCam camtype)
        {
            return;
        }

        public void SetCaller(EnumProberCam cam, object assembly)
        {
            return;
        }

        public EventCodeEnum StartGrab(EnumProberCam type, object assembly = null)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum StopGrab(EnumProberCam type)
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

        public EventCodeEnum SaveImageBuffer(ImageBuffer image, string path, IMAGE_LOG_TYPE logtype, EventCodeEnum eventcode, int offsetx = 0, int offsety = 0, int width = 0, int height = 0, double rotangle = 0.0)
        {
            throw new NotImplementedException();
        }
        public EventCodeEnum AddEdgePosBuffer(ImageBuffer image, double x = 0, double y = 0, int width = 0, int height = 0, double rotangle = 0)
        {
            throw new NotImplementedException();
        }

        public List<ICamera> GetCameras()
        {
            return null;
        }

        public ICamera GetCam(EnumProberCam type)
        {
            return null;
        }

        public DispFlipEnum GetDispHorFlip()
        {
            DispFlipEnum ret = DispFlipEnum.NONE;
            try
            {
                if (LoaderCommunicationManager.SelectedStage != null)
                {
                    if (LoaderCommunicationManager.SelectedStage.StageInfo.IsConnected == true)
                    {
                        ret = LoaderCommunicationManager.SelectedStage.DispHorFlip;
                    }
                    else
                    {
                        ret = DispFlipEnum.NONE;
                    }
                }
                else
                {
                    ret = DispFlipEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public DispFlipEnum GetDispVerFlip()
        {
            DispFlipEnum ret = DispFlipEnum.NONE;
            try
            {
                if (LoaderCommunicationManager.SelectedStage != null)
                {
                    if (LoaderCommunicationManager.SelectedStage.StageInfo.IsConnected == true)
                    {
                        ret = LoaderCommunicationManager.SelectedStage.DispVerFlip;                        
                    }
                    else
                    {
                        ret = DispFlipEnum.NONE;
                    }
                }
                else
                {
                    ret = DispFlipEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public void SetDispHorFlip(DispFlipEnum value)
        {
            
        }

        public void SetDispVerFlip(DispFlipEnum value)
        {

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

        public EdgeProcResult FindPreAlignCenteringEdge(ImageBuffer ib, bool saveDump = false)
        {
            throw new NotImplementedException();
        }

        public CassetteScanSlotResult CassetteScanProcessing(ImageBuffer ib, CassetteScanSlotParam slotParam, bool saveDump = false)
        {
            throw new NotImplementedException();
        }

        public ReadOCRResult ReadOCRProcessing(ImageBuffer ocrImage, ReadOCRProcessingParam ocrParams, string font_get_path, bool saveDump = false)
        {
            throw new NotImplementedException();
        }

        public ReadOCRResult OcrCalibrateFontProcessing(ImageBuffer ocrImage, ReadOCRProcessingParam ocrParams, string font_output_path, bool saveDump = false)
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

        public void LoadImageFromFloder(string folderpath, EnumProberCam camtype)
        {
            throw new NotImplementedException();
        }

        public void LoadImageFromFileToGrabber(string filepath, EnumProberCam camtype)
        {
            throw new NotImplementedException();
        }

        public bool ConfirmDigitizerEmulMode(EnumProberCam camtype)
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
            SetDisplayChannel(null, port);
            //(this.ViewModelManager() as ILoaderViewModelManager).RegisteDisplayPort(port);
        }

        public void SetDisplyChannelLoaderCameras(IDisplayPort port)
        {
            throw new NotImplementedException();
        }

        public void AllStageCameraStopGrab()
        {
            return;
        }

        public void DeInitModule()
        {
            return;
        }

        public EventCodeEnum InitModule()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum LoadSysParameter()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SaveSysParameter()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum InitModule(IContainer container)
        {
            return EventCodeEnum.NONE;
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
            return 0;
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
