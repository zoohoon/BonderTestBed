using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vision.GrabberModule
{
    using ProberInterfaces;
    using System.ComponentModel;
    using System.Threading;
    using ProberInterfaces.Vision;
    using AForge.Video.DirectShow;
    using AForge.Video;
    using System.Drawing;
    using System.Windows.Media.Imaging;
    using System.Windows.Media;
    using ProberErrorCode;
    using VisionParams.Camera;
    using LogModule;
    ////using ProberInterfaces.ThreadSync;

    public class USBVisionGrabber : INotifyPropertyChanged, IGrabber, IDisposable
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #region //..Property
        public ImageBuffer.ImageReadyDelegate ImageGrabbed { get; set; }


        public int iGrabSizeX;
        public int iGrabSizeY;
        private int iGrabBand;
        private int iGrabDataType;
        private FlipEnum isFlipX = FlipEnum.NONE;
        private FlipEnum isFlipY = FlipEnum.NONE;

        private bool bStopGrabThread = false;

        private AutoResetEvent _AreUpdateEvent = new AutoResetEvent(false);
        public AutoResetEvent AreUpdateEvent
        {
            get { return _AreUpdateEvent; }
            set
            {
                if (value != _AreUpdateEvent)
                {
                    _AreUpdateEvent = value;
                    NotifyPropertyChanged("AreUpdateEvent");
                }
            }
        }

        bool disposed = false;

        private VideoCaptureDevice UsbCamera;
        private FilterInfoCollection UsbCamerasCollection;
        private ImageBuffer _CurImageBuffer;
        public ImageBuffer CurImageBuffer
        {
            get { return _CurImageBuffer; }
            set { _CurImageBuffer = value; }
        }


        //private LockKey ImageBuffLock = new LockKey("USB Vision Grabber buffer");
        private object ImageBuffLock = new object();
        public byte[] _ArrImageBuff;
        public byte[] ArrImageBuff
        {
            get
            {
                return _ArrImageBuff;
            }
            set
            {
                _ArrImageBuff = value;
            }
        }

        private bool _bContinousGrab;
        public bool bContinousGrab
        {
            get
            {
                return _bContinousGrab;
            }
            set
            {
                _bContinousGrab = value;
            }
        }


        private List<byte[]> mROIByteImageArray = new List<byte[]>();

        public List<byte[]> ROIByteImageArray
        {
            get { return mROIByteImageArray; }
            set { mROIByteImageArray = value; }
        }

        public EnumGrabberMode GrabMode { get; set; }
        #endregion


        ~USBVisionGrabber()
        {
            try
            {
                Dispose(false);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void InitGrabber(int milSystem, int digitizer, int camchn)
        {
            try
            {
                if (digitizer == -1)
                    return;
                UsbCamerasCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                UsbCamera = new VideoCaptureDevice(UsbCamerasCollection[digitizer].MonikerString);
                UsbCamera.NewFrame += new NewFrameEventHandler(ColorGrabEvent);

                _bContinousGrab = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }
        public CameraParameter CamParam;
        public void SettingGrabber(EnumGrabberRaft grabberType, ICameraParameter camparam)
        {
            try
            {
                //_ColorDept = camparams.ColorDept.Value ;

                //iGrabSizeX, iGrabSizeY, iGrabBand, iGrabDataType, iGrabBand* iGrabDataType
                CamParam = camparam as CameraParameter;
                if (camparam != null)
                {
                    iGrabSizeX = camparam.GrabSizeX.Value;
                    iGrabSizeY = camparam.GrabSizeY.Value;
                    iGrabDataType = camparam.ColorDept.Value;
                    iGrabBand = camparam.Band.Value;

                    isFlipX = camparam.HorizontalFlip.Value;
                    isFlipY = camparam.VerticalFlip.Value;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"SettingGrab Error : " + err);
                throw err;
            }
        }

        public void SetCaller(object assembly)
        {
        }
        
        public void ContinuousGrab()
        {
            try
            {

                bContinousGrab = true;
                ContinusGrab();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }
        public void StopContinuousGrab()
        {
            try
            {

                UsbCamera?.Stop();
                bContinousGrab = false;
                bStopGrabThread = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        private void ContinusGrab()
        {
            try
            {
                _bContinousGrab = true;
                UsbCamera.Start();
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GrabberModuel - ContinusGrab Error : " + err.Message);
                throw err;
            }

        }

        //private LockKey syncobj = new LockKey("USB Vision Grabber");
        private object syncobj = new object();

        Bitmap img;
        Graphics g;

        private bool isSingleGrabStarted = false;
        private int updateSingleGrabFrameCount = 0;
        private void ColorGrabEvent(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                //using (Locker locker = new Locker(syncobj))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return;
                //    }
                lock (syncobj)
                {

                    //Bitmap src = (Bitmap)eventArgs.Frame.Clone();
                    var src = eventArgs.Frame;

                    int cropX = src.Width > iGrabSizeX ? (src.Width / 2 - iGrabSizeX / 2) : 0;
                    int cropY = src.Height > iGrabSizeY ? (src.Height / 2 - iGrabSizeY / 2) : 0;
                    int cropW = src.Width > iGrabSizeX ? iGrabSizeX : src.Width;
                    int cropH = src.Height > iGrabSizeY ? iGrabSizeY : src.Height;
                    Rectangle cropRect = new Rectangle(cropX, cropY, cropW, cropH);
                    if (img == null || img.Width != iGrabSizeX || img.Height != iGrabSizeY)
                    {
                        if (img != null)
                            img.Dispose();
                        img = new Bitmap(iGrabSizeX, iGrabSizeY, src.PixelFormat);

                        if (g != null)
                            g.Dispose();
                        g = Graphics.FromImage(img);
                    }

                    int destX = src.Width > iGrabSizeX ? 0 : (iGrabSizeX / 2 - src.Width / 2);
                    int destY = src.Height > iGrabSizeY ? 0 : (iGrabSizeY / 2 - src.Height / 2);
                    int destW = src.Width > iGrabSizeX ? iGrabSizeX : iGrabSizeX - destX * 2;
                    int destH = src.Height > iGrabSizeY ? iGrabSizeY : iGrabSizeY - destY * 2;
                    var destRect = new Rectangle(destX, destY, destW, destH);
                    g.Clear(System.Drawing.Color.Black);
                    g.DrawImage(src, destRect, cropRect, GraphicsUnit.Pixel);

                    if (isFlipX == FlipEnum.FLIP && isFlipY == FlipEnum.FLIP)
                    {
                        img.RotateFlip(RotateFlipType.RotateNoneFlipXY);
                    }
                    else if (isFlipX == FlipEnum.FLIP)
                    {
                        img.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    }
                    else if (isFlipY == FlipEnum.FLIP)
                    {
                        img.RotateFlip(RotateFlipType.RotateNoneFlipY);
                    }

                    //g.Dispose();
                    //using (MemoryStream ms = new MemoryStream())
                    //{
                    //    img.Save(ms, ImageFormat.Bmp);
                    //    ms.Seek(0, SeekOrigin.Begin);
                    //    BitmapImage bi = new BitmapImage();
                    //    bi.BeginInit();
                    //    bi.StreamSource = ms;
                    //    bi.EndInit();

                    //    bi.Freeze();
                    //    ArrImageBuff = ms.ToArray();
                    //}

                    BitmapSource bitmapSource = BitmapSourceColorConvert(img);
                    int stride = (int)bitmapSource.PixelWidth * ((bitmapSource.Format.BitsPerPixel + 7) / 8);
                    ArrImageBuff = new byte[(int)bitmapSource.PixelHeight * stride];
                    bitmapSource.CopyPixels(ArrImageBuff, stride, 0);

                    CurImageBuffer = new ImageBuffer(ArrImageBuff, iGrabSizeX, iGrabSizeY, iGrabBand, iGrabBand * iGrabDataType);

                    OnImage1ReadyEvent(ArrImageBuff);

                    if (isSingleGrabStarted)
                        updateSingleGrabFrameCount++;

                    AreUpdateEvent.Set();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                LoggerManager.Exception(ex);
                throw ex;
            }

        }
        private void BlackAndwriteGrabEvent(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {

                Bitmap img = (Bitmap)eventArgs.Frame.Clone();
                BitmapSource bitmapSrc = BitmapSourceBlackConvert(img);
                //ArrImageBuff = BitmapSourceToArray(bitmapSrc);
                OnImage1ReadyEvent(ArrImageBuff);

            }
            catch (Exception ex)
            {
                LoggerManager.Exception(ex);
                throw ex;
            }
        }
        private BitmapSource BitmapSourceColorConvert(System.Drawing.Bitmap bitmap)
        {
            try
            {
                var bitmapData = bitmap.LockBits(
                    new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

                var bitmapSource = BitmapSource.Create(
                    bitmapData.Width, bitmapData.Height, 96, 96, PixelFormats.Bgr24, null,
                    bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

                bitmap.UnlockBits(bitmapData);
                return bitmapSource;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }
        private BitmapSource BitmapSourceBlackConvert(System.Drawing.Bitmap bitmap)
        {
            try
            {
                var bitmapData = bitmap.LockBits(
                    new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

                var bitmapSource = BitmapSource.Create(
                    bitmapData.Width, bitmapData.Height, 96, 96, PixelFormats.Gray16, null,
                    bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

                bitmap.UnlockBits(bitmapData);
                return bitmapSource;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        public ImageBuffer SingleGrab()
        {
            try
            {
                if (bStopGrabThread)
                {
                    UsbCamera?.Stop();
                    UsbCamera.WaitForStop();
                    bStopGrabThread = false;
                }

                isSingleGrabStarted = true;
                updateSingleGrabFrameCount = 0;
                UsbCamera.Start();

                while (true)
                {
                    if (updateSingleGrabFrameCount > 0)
                    {
                        isSingleGrabStarted = false;
                        break;
                    }

                    System.Threading.Thread.Sleep(100);
                }

                //using (Locker locker = new Locker(ImageBuffLock))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return null;
                //    }
                lock (ImageBuffLock)
                {

                    CurImageBuffer.Buffer = ArrImageBuff;

                    // WriteableBitmapFromColoredArray(ArrImageBuff, this.iGrabSizeX, this.iGrabSizeY);

                    UsbCamera?.Stop();
                    UsbCamera.WaitForStop();

                    return CurImageBuffer;
                }


            }
            catch (Exception err)
            {
                //LoggerManager.Error($err.Message);
                LoggerManager.Exception(err);
                throw new Exception(err.Message, err);
            }
            finally
            {

            }
        }

        public ImageBuffer GetCurGrabImage()
        {
            return CurImageBuffer;
        }



        protected void OnImage1ReadyEvent(byte[] image)
        {
            try
            {
                if (ImageGrabbed != null)
                {
                    ImageGrabbed.BeginInvoke(CurImageBuffer, null, null);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        public EventCodeEnum LoadUserImageFiles(List<ImageBuffer> imgs)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "LoadUserImageFiles() : Error occured");
                LoggerManager.Exception(err);
                throw new Exception(err.Message, err);
            }
            return retVal;
        }
        public EventCodeEnum clearUserimageFiles()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "LoadUserImageFiles() : Error occured");
                LoggerManager.Exception(err);
                throw new Exception(err.Message, err);
            }
            return retVal;
        }

        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                try
                {
                    bStopGrabThread = true;
                }
                catch (Exception err) { throw err; }
            }
        }

        /// <summary>
        /// 진행중인 Grab 을 멈추는 함수.
        /// </summary>
        public void DigitizerHalt()
        {
            try
            {
                bStopGrabThread = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        public Task<ImageBuffer> WaitSingleShot()
        {
            return null;
        }
        public void StopWaitGrab()
        {
            return;
        }

        public bool WaitOne(int millisecondsTimeout)
        {
            bool retval = false;

            try
            {
                retval = this.AreUpdateEvent.WaitOne(millisecondsTimeout);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
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
