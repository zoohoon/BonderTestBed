using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Vision.GrabberModule
{
    using System.Threading;
    using ProberInterfaces;
    using ProberInterfaces.Vision;
    using System.IO;
    using System.Reflection;
    using System.ComponentModel;
    using ProberErrorCode;
    using VisionParams.Camera;
    using System.Diagnostics;
    using LogModule;
    using System.Drawing.Imaging;
    using System.Windows.Media.Imaging;
    using System.Drawing;
    using System.Windows.Media;
    using VirtualStageConnector;



    public class VisionGrabberSimul : IGrabber, INotifyPropertyChanged, IGrabberSimul
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        int ColorDept = 0;

        public int iGrabSizeX;
        public int iGrabSizeY;
        private int iGrabBand;
        private int iGrabDataType;

        private int UserGrabSizeX = 0;
        private int UserGrabSizeY = 0;

        public CameraParameter CamParam;
        public ImageBuffer.ImageReadyDelegate ImageGrabbed { get; set; }
        public ImageBuffer CurImageBuffer { get; set; }

        public byte[] ArrImageBuff { get; set; }

        public bool bContinousGrab { get; set; }

        private object lockobj = new object();

        Thread UpdateThread;
        private static int GrabbingInterValInms = 200;
        //private bool LoadImageFilesFlag;

        private List<ImageBuffer> Images { get; set; } = new List<ImageBuffer>();
        private ImageBuffer CopyImage = new ImageBuffer();
        private ImageBuffer Image = new ImageBuffer();

        private object Assembly { get; set; }
        private Type DeclaringType { get; set; }

        bool disposed = false;

        private EnumGrabberMode _GrabMode = EnumGrabberMode.DEFAULT;
        public EnumGrabberMode GrabMode
        {
            get { return _GrabMode; }
            set
            {
                if (value != _GrabMode)
                {
                    if (value == _GrabMode)
                        return;
                    _GrabMode = value;
                }
            }
        }

        public IVirtualStageConnector VirtualConnector
        {
            get => VirtualStageConnector.Instance;
        }

        public delegate object MyDelegate();

        public VisionGrabberSimul()
        {
        }
        ~VisionGrabberSimul()
        {
        }
        public async void InitGrabber(int milSystem, int digitizer, int camchn)
        {
            try
            {
                await VirtualConnector.InitModule();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SettingGrabber(EnumGrabberRaft grabberType, ICameraParameter camparam)
        {
            try
            {
                CamParam = camparam as CameraParameter;

                if (CamParam != null)
                {
                    if (ArrImageBuff != null)
                    {
                        ArrImageBuff = null;
                    }

                    if (ColorDept != CamParam.ColorDept.Value)
                    {
                        ColorDept = CamParam.ColorDept.Value;
                    }

                    UserGrabSizeX = CamParam.GrabSizeX.Value;
                    UserGrabSizeY = CamParam.GrabSizeY.Value;

                    iGrabSizeX = UserGrabSizeX;
                    iGrabSizeY = UserGrabSizeY;
                    iGrabBand = CamParam.Band.Value;
                    iGrabDataType = CamParam.ColorDept.Value / iGrabBand;

                    ArrImageBuff = new byte[UserGrabSizeX * UserGrabSizeX];

                    Image.SizeX = iGrabSizeX;
                    Image.SizeY = iGrabSizeY;
                    Image.ColorDept = CamParam.ColorDept.Value;
                    Image.Band = CamParam.Band.Value;
                    Image.CamType = CamParam.ChannelType.Value;

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        
        public void DigitizerHalt()
        {
            try
            {
                bContinousGrab = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ContinuousGrab()
        {
            try
            {
                lock (lockobj)
                {
                    bContinousGrab = true;
                    UpdateThread = new Thread(new ThreadStart(ImageGrabbedThread));
                    UpdateThread.Start();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. VisionGrabberEmul - ContinousGrab () : Error occurred.");
            }
        }

        public void StopContinuousGrab()
        {
            try
            {
                bContinousGrab = false;
                UpdateThread?.Join();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void Dispose()
        {
            try
            {
                Dispose(true);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (disposed)
                {
                    return;
                }

                if (disposing)
                {
                    try
                    {
                        // Code to dispose the managed resources of the class
                        if (bContinousGrab | ImageGrabbed != null)
                        {
                            StopContinuousGrab();
                        }

                        disposed = true;
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public ImageBuffer GetCurGrabImage()
        {
            return CopyImage;
        }

        public void SetCaller(object assembly)
        {
            try
            {
                this.Assembly = assembly;

                if(Assembly == null)
                {

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
      
        private ImageType GetImageType()
        {
            ImageType retval = ImageType.SNAP;

            try
            {
                if (Assembly != null)
                {
                    string name = Assembly.GetType().Name;

                    if (name == "MKOpusVProcess")
                    {
                        retval = ImageType.MARK;
                    }
                    else if (name == "CenteringState")
                    {
                        // TODO : PA 이름 확인 필요
                        retval = ImageType.PA;
                    }
                    else if (name == "PolishWaferFocusing_Standard")
                    {
                        retval = ImageType.PW;
                    }
                    else if(name == "EdgeStandard")
                    {
                        retval = ImageType.WAFER_EDGE;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private void ImageGrabbedThread()
        {
            try
            {
                while (bContinousGrab)
                {
                    try
                    {
                        if (Image == null)
                        {
                            Image = new ImageBuffer();
                        }

                        if (Image.Buffer == null)
                        {
                            Image.Buffer = new byte[iGrabSizeY * iGrabSizeY];
                        }

                        var img = VirtualConnector.GetImage(GetImageType());

                        if (img != null)
                        {
                            Image = BitmapToImageBuffer(img);
                        }

                        ImageGrabbed(Image);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }

                    Thread.Sleep(GrabbingInterValInms);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public ImageBuffer SingleGrab()
        {
            ImageBuffer retval = null;

            try
            {
                if (!bContinousGrab)
                {
                    Bitmap img = null;

                    img = VirtualConnector.GetImage(GetImageType());

                    if (img != null)
                    {
                        retval = BitmapToImageBuffer(img);
                        retval.CapturedTime = DateTime.Now;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private ImageBuffer BitmapToImageBuffer(Bitmap bitmap)
        {
            BitmapSource bitmapSource = null;

            try
            {
                bitmapSource = BitmapSourceConvert(bitmap);

                int stride = (int)bitmapSource.PixelWidth * ((bitmapSource.Format.BitsPerPixel + 7) / 8);

                if (ArrImageBuff == null)
                {
                    ArrImageBuff = new byte[(int)bitmapSource.PixelHeight * stride];
                }
                else
                {
                    if (ArrImageBuff.Length != bitmapSource.PixelHeight * stride)
                    {
                        ArrImageBuff = new byte[(int)bitmapSource.PixelHeight * stride];
                    }
                }

                bitmapSource.CopyPixels(ArrImageBuff, stride, 0);

                if (CurImageBuffer != null && CurImageBuffer.SizeX == iGrabSizeX & CurImageBuffer.SizeY == iGrabSizeY & CurImageBuffer.Band == iGrabBand)
                {
                    ArrImageBuff.CopyTo(CurImageBuffer.Buffer, 0);
                }
                else
                {
                    CurImageBuffer = new ImageBuffer(ArrImageBuff, iGrabSizeX, iGrabSizeY, iGrabBand, iGrabBand * iGrabDataType);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                bitmapSource = null;
            }

            return CurImageBuffer;
        }
        private static System.Windows.Media.PixelFormat ConvertPixelFormat(System.Drawing.Imaging.PixelFormat sourceFormat)
        {
            switch (sourceFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                    return PixelFormats.Gray8;

                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    return PixelFormats.Bgr24;

                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    return PixelFormats.Bgra32;

                case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                    return PixelFormats.Bgr32;

                    // .. as many as you need...
            }
            return new System.Windows.Media.PixelFormat();
        }

        private BitmapSource BitmapSourceConvert(System.Drawing.Bitmap bitmap)
        {
            BitmapData bitmapData;
            BitmapSource bitmapSource = null;
            Rectangle rect;

            try
            {
                rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);
                bitmapSource = BitmapSource.Create(bitmapData.Width, bitmapData.Height, 96, 96, ConvertPixelFormat(bitmap.PixelFormat), null, bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

                bitmap.UnlockBits(bitmapData);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return bitmapSource;
        }

        public EventCodeEnum LoadUserImageFiles(List<ImageBuffer> imgs)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Images.Clear();

                foreach (var img in imgs)
                {
                    img.Band = 1;
                }

                Images = imgs;

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. VisionGrabberEmul - LoadUserImageFiles () : Error occurred.");
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum clearUserimageFiles()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Images.Clear();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public Task<ImageBuffer> WaitSingleShot()
        {
            try
            {
                StackTrace stackTrace = new StackTrace(true);

                StringBuilder sb = new StringBuilder();
                List<string> sbs = new List<string>();

                foreach (StackFrame frame in stackTrace.GetFrames())

                {
                    string str = " Method Name: " + frame.GetMethod().Name + " || File Name:" + frame.GetMethod().Module.Name + " Line No: " + frame.GetFileLineNumber();
                    sb.AppendLine(str);
                    sbs.Add(str);
                }

                string dirname = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;

                ImageBuffer img = new ImageBuffer();
                Image.CopyTo(img);
                img.Buffer = null;
                this.VisionManager().WirteTextToBuffer(sbs, img);
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. VisionGrabberEmul - WaitSingleShot () : Error occurred.");
                LoggerManager.Exception(err);
            }

            return Task.FromResult<ImageBuffer>(Images[0]);
        }

        public void StopWaitGrab()
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SwitchCamera()
        {
            if (CamParam != null)
            {
                switch (CamParam.ChannelType.Value)
                {
                    case EnumProberCam.INVALID:
                        break;
                    case EnumProberCam.UNDEFINED:
                        break;
                    case EnumProberCam.WAFER_HIGH_CAM:
                        VirtualConnector.SendTCPCommand(TCPCommand.WAFER_HIGHMAG);
                        break;
                    case EnumProberCam.WAFER_LOW_CAM:
                        VirtualConnector.SendTCPCommand(TCPCommand.WAFER_LOWMAG);
                        break;
                    case EnumProberCam.PIN_HIGH_CAM:
                        VirtualConnector.SendTCPCommand(TCPCommand.PIN_HIGHMAG);
                        break;
                    case EnumProberCam.PIN_LOW_CAM:
                        VirtualConnector.SendTCPCommand(TCPCommand.PIN_LOWMAG);
                        break;
                    case EnumProberCam.PACL6_CAM:
                        break;
                    case EnumProberCam.PACL8_CAM:
                        break;
                    case EnumProberCam.PACL12_CAM:
                        break;
                    case EnumProberCam.ARM_6_CAM:
                        break;
                    case EnumProberCam.ARM_8_12_CAM:
                        break;
                    case EnumProberCam.OCR1_CAM:
                        break;
                    case EnumProberCam.OCR2_CAM:
                        break;
                    case EnumProberCam.MAP_1_CAM:
                        break;
                    case EnumProberCam.MAP_2_CAM:
                        break;
                    case EnumProberCam.MAP_3_CAM:
                        break;
                    case EnumProberCam.MAP_4_CAM:
                        break;
                    case EnumProberCam.MAP_5_CAM:
                        break;
                    case EnumProberCam.MAP_6_CAM:
                        break;
                    case EnumProberCam.MAP_7_CAM:
                        break;
                    case EnumProberCam.MAP_8_CAM:
                        break;
                    case EnumProberCam.MAP_REF_CAM:
                        break;
                    case EnumProberCam.CAM_LAST:
                        break;
                    case EnumProberCam.GIGE_VM0:
                        break;
                    default:
                        break;
                }
            }
        }

        public bool WaitOne(int millisecondsTimeout)
        {
            return true;
        }

        public void SetParameter(string modulename, string path)
        {
            throw new NotImplementedException();
        }

        public void SetParameters(string modulename, string[] paths)
        {
            throw new NotImplementedException();
        }
    }
}
