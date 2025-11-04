using System;
using System.Collections.Generic;

namespace ProcessingModule
{
    using System.Collections.ObjectModel;
    using System.Windows;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Param;
    using ProberInterfaces.Vision;
    using SystemExceptions.VisionException;
    using VisionParams;
    using LogModule;
    using System.IO;
    using Matrox.MatroxImagingLibrary;
    using System.Drawing.Imaging;
    using System.Drawing;
    using Point = System.Windows.Point;
    using Size = System.Windows.Size;

    public class VisionProcessingEmul : IVisionProcessing
    {
        private IVisionManager _VisionManager;
        public IVisionManager VisionManager
        {
            get { return _VisionManager; }
            set { _VisionManager = value; }
        }
        public bool Initialized { get; set; } = false;

        public ImageBuffer.ImageReadyDelegate ImageProcessing { get; set; }
        public ImageBuffer.ImageReadyDelegate ImagePattView { get; set; }
        public VisionProcessingParameter VisionProcParam;

        public IVisionAlgorithmes Algorithmes { get; set; }

        private int DefaultFocusingValue = 1000000;
        private double FocusingMultiplyRate = 1.2;
        private int FocusingResultValue = 0;
        private int UpdateFocuingCount = 20;
        private int DefaultGrayValue = 127;
        //private int MilSystem = 0;

        public BlobResult BlobColorObject(ImageBuffer _grabedImage, BlobParameter blobparam, bool fillholes = true, bool invert = false, bool runsort = true)
        {
            try
            {
                return new BlobResult();
            }
            catch (Exception err)
            {
                throw new VisionException("BlobColorObject Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
        }

        public BlobResult BlobObject(ImageBuffer grabedImage, BlobParameter blobparam, ROIParameter roiparam = null, bool fillholes = true, bool invert = false, bool runsort = true)
        {
            try
            {
                return new BlobResult();
            }
            catch (Exception err)
            {
                throw new VisionException("BlobObject Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
        }

        public CassetteScanSlotResult CassetteScanProcessing(ImageBuffer ib, CassetteScanSlotParam slotParams, bool saveDump = false)
        {
            try
            {
                return new CassetteScanSlotResult();

            }
            catch (Exception err)
            {
                throw new VisionException("CassetteScanProcessing Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
        }

        public ReadOCRResult ReadOCRProcessing(ImageBuffer ocrImage, ReadOCRProcessingParam ocrParams, string font_output_path, bool saveDump = false)
        {
            //try
            //{
            //    return VisionManager.ReadOCRProcessing(ocrImage, ocrParams, saveDump);
            //}
            //catch (Exception err)
            //{
            //    throw err;
            //}

            try
            {
                return null;
            }
            catch (Exception err)
            {
                throw new VisionException("ReadOCRProcessing Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
        }

        public ReadOCRResult OcrCalibrateFontProcessing(ImageBuffer ocrImage, ReadOCRProcessingParam ocrParams, string font_output_path, bool saveDump = false)
        {
            //try
            //{
            //    return VisionManager.OcrCalibrateFontProcessing(ocrImage, ocrParams, saveDump);
            //}
            //catch (Exception err)
            //{

            //    throw err;
            //}

            try
            {
                return null;
            }
            catch (Exception err)
            {
                throw new VisionException("OcrCalibrateFontProcessing Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
        }

        public byte[] ConvertToImageBuffer(object image, int sizex, int sizey)
        {
            try
            {
                return new byte[sizex + sizey * 3];
            }
            catch (Exception err)
            {
                throw new VisionException("ConvertToImageBuffer Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
        }

        public int CreatePACLMaskImage(ImageBuffer ib, string path)
        {
            try
            {
                return -1;
            }
            catch (Exception err)
            {
                throw new VisionException("CreatePACLMaskImage Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
        }

        public int CutSizeConvertEdge(ref ImageBuffer ib)
        {
            try
            {
                return -1;
            }
            catch (Exception err)
            {
                throw new VisionException("CutSizeConvertEdge Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
        }

        public ObservableCollection<GrabDevPosition> Detect_Pad(ImageBuffer _grabedImage, double UserPadSizeX, double UserPadSizeY, ROIParameter roiparam, bool fillholes = true)
        {
            try
            {
                return new ObservableCollection<GrabDevPosition>();
            }
            catch (Exception err)
            {
                throw new VisionException("Detect_Pad Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
        }

        public void Dispose()
        {
            return;
        }

        public EdgeResult EdgeProcessing(ImageBuffer ib)
        {
            try
            {
                return new EdgeResult();
            }
            catch (Exception err)
            {
                throw new VisionException("EdgeProcessing Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
        }

        int prealing_curr_index = 0;
        List<Point> _prealign_points = new List<Point>()
        {
            new Point(240, 240),
            new Point(240, 240),
            new Point(240, 240),
            new Point(240, 240),
            new Point(240, 240),
            new Point(240, 240),
        };
        public EdgeProcResult FindEdgeProcessing(ImageBuffer ib, bool saveDump = false)
        {
            try
            {
                EdgeProcResult rel = new EdgeProcResult();

                rel.ImageSize = new Point(480, 480);
                if (prealing_curr_index >= _prealign_points.Count)
                {
                    prealing_curr_index = 0;
                }
                rel.Edges = new List<Point>();
                rel.Edges.Add(_prealign_points[prealing_curr_index++]);

                return rel;
            }
            catch (Exception err)
            {
                throw new VisionException("FindEdgeProcessing Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
        }

        public int GetFocusValue(ImageBuffer img, Rect roi = default(Rect))
        {
            try
            {
                FocusingResultValue = 0;
                FocusingResultValue = DefaultFocusingValue;
                if (UpdateFocuingCount == 20)
                {
                    UpdateFocuingCount = 0;
                    return Convert.ToInt32(FocusingResultValue * FocusingMultiplyRate);
                }
                else
                {
                    UpdateFocuingCount++;
                    return FocusingResultValue;
                }
            }
            catch (Exception err)
            {
                throw new VisionException("GetFocusValue Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }

        }
        public EventCodeEnum GetGrayLevel(ref ImageBuffer img)
        {
            try
            {
                img.GrayLevelValue = DefaultGrayValue;
                return EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw new VisionException("GetGrayLevel Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }

        }
        public int GetIntegralImg(ImageBuffer Ori, ref IntegralImage Result)
        {
            try
            {
                return -1;
            }
            catch (Exception err)
            {
                throw new VisionException("GetIntegralImg Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
        }

        public long GetIntegralSum(IntegralImage Input, int x, int y, int bx, int by)
        {
            try
            {
                return -1;
            }
            catch (Exception err)
            {
                throw new VisionException("GetIntegralSum Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
        }


        public EventCodeEnum GetPatternSize(string path, out Size size)
        {
            try
            {
                size = new Size();
                return EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw new VisionException("GetPatternSize Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    Algorithmes = new VisionAlgorithmesEmul(this);

                    Initialized = true;

                    retval = EventCodeEnum.NONE;
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
        
        public void InitProcessing(int milSystem)
        {
            //MilSystem = milSystem;
            return;
        }

        public ImageBuffer Line_Equalization_Processing(ImageBuffer img, int Cpos)
        {
            try
            {
                return new ImageBuffer();
            }
            catch (Exception err)
            {
                throw new VisionException("Line_Equalization_Processing Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }

        }

        public ImageBuffer LoadImageFile(string filepath)
        {
            try
            {
                if (File.Exists(filepath))
                {
                    using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(filepath))
                    {
                        byte[] imageData;
                        FileStream imageStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
                        BinaryReader binaryReader = new BinaryReader(imageStream);
                        imageData = binaryReader.ReadBytes((int)imageStream.Length);
                        imageStream.Close();
                        binaryReader.Close();
                        System.Drawing.Imaging.PixelFormat pformet = bitmap.PixelFormat;
                        if (pformet == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
                            return new ImageBuffer(imageData, bitmap.Width, bitmap.Height, 1, 8);
                        else
                            return new ImageBuffer(imageData, bitmap.Width, bitmap.Height, 1, 8);
                    }                        
                }
                return new ImageBuffer();
            }
            catch (Exception err)
            {
                throw new VisionException("LoadImageFile Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
        }

        public ImageBuffer ReduceImageSize(ImageBuffer ib, int offsetx = 0, int offsety = 0, int sizex = 0, int sizey = 0)
        {
            try
            {
                return ib;
                //return new ImageBuffer(new byte[1 * 1], 0, 0, 0, 0);
            }
            catch (Exception err)
            {
                throw new VisionException("ReduceImageSize Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
        }

        public void RegistModel(ImageBuffer MilGrabBuffer, int offsetx = 0, int offsety = 0, int PattWidth = 128, int PattHeight = 128, double rotangle = 0, string pattFullPath = "", bool isregistMask = true)
        {
            return;
        }

        public void ResetPMSearchRegion()
        {
            return;
        }

        public EventCodeEnum SaveImageBuffer(ImageBuffer _grabedImage, string path, IMAGE_LOG_TYPE logtype, EventCodeEnum eventcode, int offsetx = 0, int offsety = 0, int width = 0, int height = 0, double rotangle = 0.0)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum AddEdgePosBuffer(ImageBuffer _grabedImage, double x = 0, double y = 0,
                int width = 0, int height = 0, double rotangle = 0.0)
        {
            return EventCodeEnum.NONE;
        }

        public PMResult PatternMatching(ImageBuffer targetImage, ImageBuffer patternImage, PMParameter pmparam,
            string pattpath, string maskpath, bool angleretry = false,
            int offsetx = 0, int offsety = 0, int sizex = 480, int sizey = 480)
        {
            try
            {
                ICamera camera = this.VisionManager().GetCam(targetImage.CamType);
                PMResult ret = new PMResult();
                ret.ResultBuffer = targetImage;
                ret.ResultParam = new ObservableCollection<PMResultParameter>();
                ret.ResultParam.Add(new PMResultParameter(camera.GetGrabSizeWidth() / 2, camera.GetGrabSizeHeight() / 2, 0, 100));
                ret.RetValue = EventCodeEnum.NONE;
                return ret;
            }
            catch (Exception err)
            {
                throw new VisionException("SetPattModel Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }

        }

        public void SetPMSearchRegion(int offx, int offy, int sizex, int sizey)
        {
            return;
        }

        public PMResult SearchPatternMatching(ImageBuffer img, PatternInfomation ptinfo, bool findone = true)
        {
            try
            {
                PMResult ret = new PMResult();
                ret.ResultParam = new ObservableCollection<PMResultParameter>();
                ret.ResultParam.Add(new PMResultParameter(0, 0, 0, 0));
                ret.RetValue = EventCodeEnum.NONE;
                return ret;
            }
            catch (Exception err)
            {
                throw new VisionException("SearchPatternMatching Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
        }
        public void ViewResult(ImageBuffer procResultBuffer)
        {
            return;
        }

        public void ViewResult_Patt(int colorDept, byte[] _PattImageBuff)
        {
            return;
        }
        public void DeInitModule()
        {

        }

        public ImageBuffer WirteTextToBuffer(List<string> sb, ImageBuffer ib)
        {
            try
            {
                if (ib.Buffer == null)
                {
                    ib.Buffer = new byte[ib.SizeX * ib.SizeY];
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }

            return ib;
        }

        public void Mil_Binarize(ImageBuffer SourceImg, ref ImageBuffer RelsultImg, int Threshold, int OffsetX = 0, int OffsetY = 0, int SizeX = 480, int SizeY = 480)
        {
            try
            {
                RelsultImg = new ImageBuffer();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return;
        }

        
        public double EdgeFind_IndexAlign(ImageBuffer SrcBuf, int Cpos, int RWidth, int Threshold)
        {
            return 0;
        }

        public BlobResult FindBlob(ImageBuffer _grabedImage,
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
                            double BlobSizeYMaximumMargin = 0.5)
        {
            BlobResult retval = null;

            try
            {
                ImgPosX = 480;
                ImgPosY = 480;

                ObservableCollection<GrabDevPosition> devicePositions = new ObservableCollection<GrabDevPosition>();
                GrabDevPosition mChipPosition;
                mChipPosition = new GrabDevPosition(0, 0, 0, 0, 0, 0);
                devicePositions.Add(mChipPosition);
                var originalimagebuffer = new ImageBuffer(_grabedImage);
                var ResultBuffer = new ImageBuffer(_grabedImage.Buffer, _grabedImage.SizeX, _grabedImage.SizeY, _grabedImage.Band, _grabedImage.ColorDept);
                retval = new BlobResult(originalimagebuffer, ResultBuffer, devicePositions);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }

            return retval;

        }
        public BlobResult FindBlobWithRectangularity(ImageBuffer _grabedImage,
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
                            double minRectangularity = 0.0)
        {
            BlobResult retval = null;

            try
            {
                ImgPosX = 480;
                ImgPosY = 480;

                ObservableCollection<GrabDevPosition> devicePositions = new ObservableCollection<GrabDevPosition>();
                GrabDevPosition mChipPosition;
                mChipPosition = new GrabDevPosition(0, 0, 0, 0, 0, 0);
                devicePositions.Add(mChipPosition);
                var originalimagebuffer = new ImageBuffer(_grabedImage);
                var ResultBuffer = new ImageBuffer(_grabedImage.Buffer, _grabedImage.SizeX, _grabedImage.SizeY, _grabedImage.Band, _grabedImage.ColorDept);
                retval = new BlobResult(originalimagebuffer, ResultBuffer, devicePositions);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }

            return retval;
        }

        public List<ModelFinderResult> ModelFind(ImageBuffer targetimg, EnumModelTargetType targettype, EnumForegroundType foreground, Size size, int acceptance, double posx = 0, double posy = 0, double roiwidth = 0, double roiheight = 0, double scale_min = 0, double scale_max = 0, double horthick = 0, double verthick = 0, double smoothness = 70, int number = 0)
        {
            return new List<ModelFinderResult>();
        }
        public List<ModelFinderResult> ModelFind_For_Key(ImageBuffer targetimg, EnumModelTargetType targettype, EnumForegroundType foreground, Size size, int acceptance, double posx = 0, double posy = 0, double roiwidth = 0, double roiheight = 0, double scale_min = 0, double scale_max = 0, double horthick = 0, double verthick = 0, double smoothness = 70, int number = 0)
        {
            return new List<ModelFinderResult>();
        }

        public bool IsValidModLicense()
        {
            return false;
        }

        public void Mil_Crop(ImageBuffer SourceImg, ref ImageBuffer ResultImg, int OffsetX = 0, int OffsetY = 0, int SizeX = 480, int SizeY = 480)
        {
            throw new NotImplementedException();
        }

        public ImageBuffer ResizeImageBuffer(ImageBuffer ib, int ScaleFactorX, int ScaleFactorY)
        {
            try
            {
                return ib;
            }
            catch (Exception err)
            {
                throw new VisionException("ResizeImageBuffer Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
        }

        public EventCodeEnum SaveImageBufferWithRectnagle(ImageBuffer _grabedImage, string path, IMAGE_LOG_TYPE logtype, EventCodeEnum eventcode, Rect focusROI, int offsetx = 0, int offsety = 0, int width = 0, int height = 0, double rotangle = 0)
        {
            return EventCodeEnum.NONE;
        }
        public ImageBuffer DrawCrosshair(ImageBuffer grabbedImage, Point pt, int length, int thickness = 1)
        {
            throw new NotImplementedException();
        }

        public ImageBuffer CombineImages(ImageBuffer[] images, int width, int height, int rows, int columns)
        {
            throw new NotImplementedException();
        }
    }

}
