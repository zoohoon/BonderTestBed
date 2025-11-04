using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vision.GrabberModule
{
    using ProberInterfaces;
    using System.ComponentModel;
    using System.Threading;
    using ProberInterfaces.Vision;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Windows.Media.Imaging;
    using System.Windows.Media;
    using ProberErrorCode;
    using VisionParams.Camera;
    using LogModule;
    using System.Runtime.CompilerServices;
    ////using ProberInterfaces.ThreadSync;

    public class TISGrabber : INotifyPropertyChanged, IGrabber, IDisposable
    {

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        #region // Properties

        //private LockKey ImageGrabbedLock = new LockKey("TIS Grabbed Lock");
        private object ImageGrabbedLock = new object();

        public ImageBuffer.ImageReadyDelegate ImageGrabbed { get; set; }
        private ImageBuffer _CurImageBuffer;
        public ImageBuffer CurImageBuffer
        {
            get { return _CurImageBuffer; }
            set { _CurImageBuffer = value; }
        }

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
        private bool _bContinuousGrab;
        public bool bContinousGrab
        {
            get
            {
                return _bContinuousGrab;
            }
            set
            {
                _bContinuousGrab = value;
            }
        }

        private CameraParameter _CamParam;

        public CameraParameter CamParam
        {
            get { return _CamParam; }
            set { _CamParam = value; }
        }

        public EnumGrabberMode GrabMode { get; set; }
        #endregion

        bool disposed = false;

        private TIS.Imaging.ICImagingControl icImg;
        private int iGrabSizeX;
        private int iGrabSizeY;
        private int iGrabBand;
        private int iGrabDataType;
        private FlipEnum isFlipX = FlipEnum.NONE;
        private FlipEnum isFlipY = FlipEnum.NONE;
        ManualResetEvent imgReadyMRE = new ManualResetEvent(false);
        private static Rectangle cloneRect;

        public EventCodeEnum clearUserimageFiles()
        {
            return EventCodeEnum.UNDEFINED;
        }

        public void ContinuousGrab()
        {
            try
            {
                bContinousGrab = true;
                continuousGrab();
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void continuousGrab()
        {
            try
            {
                bContinousGrab = true;
                icImg.LiveCaptureContinuous = true;
                icImg.LiveStart();
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void DigitizerHalt()
        {
            try
            {
                bContinousGrab = false;
                icImg.LiveCaptureContinuous = false;
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        ~TISGrabber()
        {
            try
            {
                Dispose(false);
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public void Dispose()
        {
            try
            {
                Dispose(false);
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                try
                {
                    bContinousGrab = false;
                    icImg.LiveStop();
                    icImg.LiveCaptureContinuous = false;

                }
                catch (Exception err) { throw err; }
            }
        }

        public ImageBuffer GetCurGrabImage()
        {
            return CurImageBuffer;
        }

        public void InitGrabber(int milSystem, int digitizer, int camchn)
        {
            try
            {
                if (digitizer == -1)
                    return;

                bContinousGrab = false;
                icImg = new TIS.Imaging.ICImagingControl();
                //Digitizer ID: ex) 0x35314287, 0x21814661, 0x21814661
                var devices = icImg.Devices;

                if (devices.Count() > 0)
                {
                    string strDevSer;
                    int devSer;

                    for (int i = 0; i < devices.Count(); i++)
                    {
                        if (devices[i].GetSerialNumber(out strDevSer))
                        {
                            if (int.TryParse(strDevSer, out devSer))
                            {
                                if (devSer == digitizer)
                                {
                                    icImg.Device = devices[i];
                                    icImg.ImageAvailableExecutionMode = TIS.Imaging.EventExecutionMode.MultiThreaded;
                                    icImg.DeviceLost += IcImg_DeviceLost;
                                    icImg.ImageAvailable += IcImg_ImageAvailable;
                                    icImg.LiveStart();
                                }
                            }
                            else
                            {
                                LoggerManager.Debug($"Serial parsing error. Dev. ser = {strDevSer}");
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"Device Serial error.");
                        }
                    }
                }
                else
                {
                    LoggerManager.Error($"No suitable device is available.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void IcImg_ImageAvailable(object sender, TIS.Imaging.ICImagingControl.ImageAvailableEventArgs e)
        {
            try
            {
                Bitmap bitMapImage = null;
                //CurImageBuffer.Buffer = 
                if (e.ImageBuffer == null)
                {
                    e.ImageBuffer.Lock();
                    imgReadyMRE.Reset();
                    if (cloneRect == null)
                    {
                        cloneRect = new Rectangle(0, 0, e.ImageBuffer.Size.Width, e.ImageBuffer.Size.Height);
                    }
                    else
                    {
                        if (cloneRect.Width != e.ImageBuffer.Size.Width |
                            cloneRect.Height != e.ImageBuffer.Size.Height)
                        {
                            cloneRect = new Rectangle(0, 0, e.ImageBuffer.Size.Width, e.ImageBuffer.Size.Height);
                        }
                    }
                    System.Drawing.Imaging.PixelFormat format = e.ImageBuffer.Bitmap.PixelFormat;
                    bitMapImage = e.ImageBuffer.Bitmap.Clone(cloneRect, format);
                }
                else
                {
                    icImg.ImageActiveBuffer.Lock();
                    imgReadyMRE.Reset();
                    if (cloneRect == null)
                    {
                        cloneRect = new Rectangle(0, 0, icImg.ImageActiveBuffer.Size.Width, icImg.ImageActiveBuffer.Size.Height);
                    }
                    else
                    {
                        if (cloneRect.Width != e.ImageBuffer.Size.Width |
                            cloneRect.Height != e.ImageBuffer.Size.Height)
                        {
                            cloneRect = new Rectangle(0, 0, icImg.ImageActiveBuffer.Size.Width, icImg.ImageActiveBuffer.Size.Height);
                        }
                    }
                    System.Drawing.Imaging.PixelFormat format = icImg.ImageActiveBuffer.Bitmap.PixelFormat;
                    bitMapImage = icImg.ImageActiveBuffer.Bitmap.Clone(cloneRect, format);
                }

                if (isFlipX == FlipEnum.FLIP)
                {
                    bitMapImage.RotateFlip(RotateFlipType.RotateNoneFlipX);
                }
                if (isFlipY == FlipEnum.FLIP)
                {
                    bitMapImage.RotateFlip(RotateFlipType.RotateNoneFlipY);
                }

                CurImageBuffer = CurImageBuffer ??
                    new ImageBuffer(CamParam.GrabSizeX.Value, CamParam.GrabSizeY.Value,
                    CamParam.Band.Value, (CamParam.ColorDept.Value * CamParam.Band.Value));

                //var bytes = ImageToByte(bitMapImage);
                //bitMapImage.Save("d:\\myBitmap.bmp");
                //CurImageBuffer.Buffer = bytes;
                //Buffer.BlockCopy(bytes, 0, CurImageBuffer.Buffer, 0, bytes.Count());

                CurImageBuffer = BitmapToImageBuffer(bitMapImage);

                //ArrImageBuff = ArrImageBuff ?? new byte[e.ImageBuffer.Size.Width * e.ImageBuffer.Size.Height];
                if (bContinousGrab == true)
                {
                    OnImage1ReadyEvent(ArrImageBuff);
                }
                bitMapImage = null;

            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "Error occurred.");
                LoggerManager.Debug($"[TISGrabber] Image not available. {err}");

                //LoggerManager.Exception(err);

                // TODO : Check
                //throw err;
            }
            finally
            {
                if (e.ImageBuffer == null)
                {
                    icImg.ImageActiveBuffer.Unlock();

                }
                else
                {
                    e.ImageBuffer.Unlock();
                }

            }
        }

        private ImageBuffer BitmapToImageBuffer(Bitmap bitmap)
        {
            BitmapSource bitmapSource = null;

            try
            {
                bitmapSource = BitmapSourceColorConvert(bitmap);

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

                if (CurImageBuffer.SizeX == iGrabSizeX & CurImageBuffer.SizeY == iGrabSizeY & CurImageBuffer.Band == iGrabBand)
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
                LoggerManager.Error($"BitmapToImageBuffer(): Error occurred. PixelWidth = {bitmapSource.PixelWidth}, {err.Message}");
                LoggerManager.Exception(err);
                throw err;
            }
            finally
            {
                bitmapSource = null;
            }

            return CurImageBuffer;
        }

        private BitmapSource BitmapSourceColorConvert(System.Drawing.Bitmap bitmap)
        {
            BitmapData bitmapData;
            BitmapSource bitmapSource = null;
            Rectangle rect;
            try
            {
                rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
                bitmapData = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);
                bitmapSource = BitmapSource.Create(bitmapData.Width, bitmapData.Height, 96, 96, PixelFormats.Bgr24, null, bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

                bitmap.UnlockBits(bitmapData);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }

            return bitmapSource;
        }
        private BitmapSource BitmapSourceBlackConvert(System.Drawing.Bitmap bitmap)
        {
            try
            {

                Rectangle rectangle = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
                var bitmapData = bitmap.LockBits(rectangle, System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

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
        //private static byte[] ImageToByte(Image img)
        //{
        //    ImageConverter converter = new ImageConverter();
        //    return (byte[])converter.ConvertTo(img, typeof(byte[]));
        //}

        private void IcImg_DeviceLost(object sender, TIS.Imaging.ICImagingControl.DeviceLostEventArgs e)
        {
            try
            {
                StopContinuousGrab();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }

            //LoggerManager.DebugError($"No suitable device is available.");
        }

        public EventCodeEnum LoadUserImageFiles(List<ImageBuffer> imgs)
        {
            return EventCodeEnum.UNDEFINED;
        }

        public void SetCaller(object assembly)
        {
        }

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
                    iGrabBand = camparam.Band.Value;
                    iGrabDataType = camparam.ColorDept.Value / iGrabBand;


                    isFlipX = camparam.HorizontalFlip.Value;
                    isFlipY = camparam.VerticalFlip.Value;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"SettingGrab Error : " + err);
                LoggerManager.Exception(err);
                throw err;
            }
        }

        public ImageBuffer SingleGrab()
        {
            Bitmap bitMapImage = null;
            //CurImageBuffer.Buffer = 
            try
            {
                imgReadyMRE.Reset();

                icImg.LiveCaptureContinuous = false;
                icImg.MemorySnapImage(1000);

                ////System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                ////{
                ////    icImg.MemorySnapImage(1000);
                ////});
                ////icImg.MemorySnapImage();
                //for (int i = 0; i < icImg.ImageBuffers.Length; i++)
                //{
                //    if (icImg.ImageBuffers[i].GetIntPtr() == icImg.ImageActiveBuffer.GetIntPtr())
                //    {
                //        icImg.ImageBuffers[i].Lock();
                //    }
                //}
                if(icImg.ImageActiveBuffer != null)
                {
                    icImg.ImageActiveBuffer.Lock();
                }
                else
                {
                    //icImg.MemorySnapImage(1000);
                    //icImg.ImageActiveBuffer.Lock();

                    // Retry
                    for (int i = 0; i < 5; i++)
                    {
                        icImg.MemorySnapImage(1000);
                        //icImg.ImageActiveBuffer.Lock();

                        if (icImg.ImageActiveBuffer != null)
                        {
                            icImg.ImageActiveBuffer.Lock();

                            break;
                        }
                        else
                        {
                            LoggerManager.Debug($"[TISGrabber] SingleGrab Retry Count : {i + 1}");
                        }
                    }
                }

                //Rectangle cloneRect = new Rectangle(0, 0, 
                //    icImg.ImageActiveBuffer.Size.Width, 
                //    icImg.ImageActiveBuffer.Size.Height);

                if (cloneRect == null)
                {
                    cloneRect = new Rectangle(0, 0, icImg.ImageActiveBuffer.Size.Width, icImg.ImageActiveBuffer.Size.Height);
                }
                else
                {
                    if (cloneRect.Width != icImg.ImageActiveBuffer.Size.Width | cloneRect.Height != icImg.ImageActiveBuffer.Size.Height)
                    {
                        cloneRect = new Rectangle(0, 0, icImg.ImageActiveBuffer.Size.Width, icImg.ImageActiveBuffer.Size.Height);
                    }
                }

                bitMapImage = icImg.ImageActiveBuffer.Bitmap.Clone(cloneRect, icImg.ImageActiveBuffer.Bitmap.PixelFormat);
                if (isFlipX == FlipEnum.FLIP)
                {
                    bitMapImage.RotateFlip(RotateFlipType.RotateNoneFlipX);
                }
                if (isFlipY == FlipEnum.FLIP)
                {
                    bitMapImage.RotateFlip(RotateFlipType.RotateNoneFlipY);
                }


                CurImageBuffer = CurImageBuffer ?? new ImageBuffer(CamParam.GrabSizeX.Value, CamParam.GrabSizeY.Value, CamParam.Band.Value, CamParam.ColorDept.Value);
                ArrImageBuff = ArrImageBuff ?? new byte[bitMapImage.Size.Width * bitMapImage.Size.Height];
                CurImageBuffer = BitmapToImageBuffer(bitMapImage);

                OnImage1ReadyEvent(CurImageBuffer.Buffer);
                //if (imgReadyMRE.WaitOne(1000) == true)
                //{
                //    imgReadyMRE.Reset();
                //}
                //else
                //{
                //    LoggerManager.DebugError($"TIS.SingleGrab(): Grab time out.");
                //}
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "Error occurred.");
                LoggerManager.Exception(err);
                throw err;
            }
            finally
            {
                //for (int i = 0; i < icImg.ImageBuffers.Length; i++)
                //{
                //    icImg.ImageBuffers[i].Unlock();
                //}
                icImg.ImageActiveBuffer.Unlock();
                imgReadyMRE.Reset();
            }
            return CurImageBuffer;
        }


        protected void OnImage1ReadyEvent(byte[] image)
        {
            try
            {
                if (ImageGrabbed != null)
                {
                    //using (Locker locker = new Locker(ImageGrabbedLock))
                    //{
                    //    if (locker.AcquiredLock == false)
                    //    {
                    //        System.Diagnostics.Debugger.Break();
                    //        return;
                    //    }
                    lock(ImageGrabbedLock)
                    { 
                        ImageGrabbed.BeginInvoke(CurImageBuffer, null, null);
                    }
                }
                imgReadyMRE.Set();
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
                bContinousGrab = false;
                //icImg.LiveCaptureContinuous = false;
                //icImg.LiveStop();
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public Task<ImageBuffer> WaitSingleShot()
        {
            throw new NotImplementedException();
        }
        public void StopWaitGrab()
        {
            return;
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
