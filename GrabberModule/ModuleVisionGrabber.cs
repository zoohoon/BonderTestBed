using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Threading;
using Matrox.MatroxImagingLibrary;

namespace Vision.GrabberModule
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Vision;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    ////using ProberInterfaces.ThreadSync;
    using VisionParams.Camera;
    using SystemExceptions.VisionException;

    public class UserDataObject
    {
        public MIL_INT NbGrabStart;
    }


    public class ModuleVisionGrabber : INotifyPropertyChanged, IGrabber, IDisposable
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


        public event ImageReadyEventHandler ImageReadyEvent;
        private int MilDigitizer;
        private int MilSystem;
        public int iGrabSizeX;
        public int iGrabSizeY;
        private int iGrabBand;
        private int iGrabDataType;

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
        long lAttributes = MIL.M_IMAGE + MIL.M_DISP + MIL.M_GRAB + MIL.M_PROC;
        long cl_iAttributes = MIL.M_IMAGE + MIL.M_GRAB + MIL.M_RGB24 + MIL.M_PACKED;
        int _Camchn = 0;
        int _ColorDept = 0;
        MIL_DIG_HOOK_FUNCTION_PTR MDH_DigiGrabFrameEndDelegate;

        UserDataObject userObject = new UserDataObject();
        GCHandle userObjectHandle;

        private MIL_INT MilIntGrabSizeX = 0;
        private MIL_INT MilIntGrabSizeY = 0;
        private MIL_INT MilIntGrabBand = 0;
        private MIL_INT MilIntGrabDataType = 0;

        MIL_ID milGrabBuffer = MIL.M_NULL;
        MIL_ID milGrabColorBuffer = MIL.M_NULL;
        MIL_ID milUserBuffer = MIL.M_NULL;

        private int UserGrabSizeX = 0;
        private int UserGrabSizeY = 0;
        private bool IsGrabSizeConvert = false;


        private ImageBuffer _CurImageBuffer;

        public ImageBuffer CurImageBuffer
        {
            get { return _CurImageBuffer; }
            set { _CurImageBuffer = value; }
        }



        //private LockKey ImageBuffLock = new LockKey("Module Vision Grabber buffer");
        private object ImageBuffLock = new object();
        private object lockobj = new object();
        private object singleGrabLockObj = new object();

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

        //private readonly LockKey lockobj = new LockKey("Module Vision Grabber");
        //private object lockobj;
        ~ModuleVisionGrabber()
        {
            Dispose(false);
        }
        /// <summary>
        /// MIL용 Grabber 초기화 메소드.
        /// MilSystem을 파라미터로 받아서 Grab buffer 초기화를 진행
        /// </summary>
        /// <param name="MilSystem"></param>

        public void InitGrabber(int milSystem, int digitizer, int camchn)
        {
            try
            {
                MIL_INT grabTimeout = 1000;//in ms
                if (digitizer == -1)
                    return;

                MilSystem = milSystem;

                if (MilSystem == MIL.M_NULL)
                    return;

                _Camchn = camchn;
                MilDigitizer = digitizer;

                MIL.MdigControl(MilDigitizer, MIL.M_GRAB_MODE, MIL.M_SYNCHRONOUS);
                MIL.MdigControl(MilDigitizer, MIL.M_GRAB_TIMEOUT, grabTimeout);
                MIL.MdigInquire(MilDigitizer, MIL.M_SIZE_X, ref MilIntGrabSizeX);
                MIL.MdigInquire(MilDigitizer, MIL.M_SIZE_Y, ref MilIntGrabSizeY);
                //MilIntGrabSizeX = 480;
                MIL.MdigInquire(MilDigitizer, MIL.M_SIZE_BAND, ref MilIntGrabBand);
                MIL.MdigInquire(MilDigitizer, MIL.M_TYPE, ref MilIntGrabDataType);


                // Clear the buffer.

                mROIByteImageArray.Add(new byte[128 * 128]);

                //// Hook a function to the start of each frame to print the current frame index.
                userObject.NbGrabStart = 0;

                userObjectHandle = GCHandle.Alloc(userObject);

                MDH_DigiGrabFrameEndDelegate = new MIL_DIG_HOOK_FUNCTION_PTR(Digitizer1_GrabEnd);
                //MIL.MdigHookFunction(MilDigitizer, MIL.M_GRAB_FRAME_END, MDH_DigiGrabFrameEndDelegate, MIL.M_NULL);
                MIL.MdigHookFunction(MilDigitizer, MIL.M_GRAB_FRAME_END, MDH_DigiGrabFrameEndDelegate, GCHandle.ToIntPtr(userObjectHandle));

                //lockobj = MilDigitizer;

            }
            catch (MILException err)
            {
                LoggerManager.Exception(err);
                throw new VisionException("", err, EventCodeEnum.VISION_EXCEPTION, this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw new Exception(err.Message, err);
            }
        }
        private bool GigEReserveY = false;
        private MIL_ID GigeReserveBuffer;
        public CameraParameter CamParam;
        public void SettingGrabber(EnumGrabberRaft grabberType, ICameraParameter camparam)
        {
            try
            {
                CamParam = camparam as CameraParameter;
                if (CamParam != null)
                {
                    if (CamParam.ColorDept.Value == (int)ColorDept.BlackAndWhite)
                    {
                        bool IsChangMilBuffer = false;

                        if (ArrImageBuff != null)
                            ArrImageBuff = null;

                        if (_ColorDept != CamParam.ColorDept.Value)
                        {
                            _ColorDept = CamParam.ColorDept.Value;
                            IsChangMilBuffer = true;
                        }

                        if (UserGrabSizeX != CamParam.GrabSizeX.Value
                            && UserGrabSizeY != CamParam.GrabSizeY.Value)
                        {
                            IsChangMilBuffer = true;
                        }

                        if (MilIntGrabSizeX > CamParam.GrabSizeX.Value)
                        {
                            UserGrabSizeX = CamParam.GrabSizeX.Value;
                            iGrabSizeX = (int)MilIntGrabSizeX;
                            IsGrabSizeConvert = true;
                        }
                        else
                        {
                            UserGrabSizeX = CamParam.GrabSizeX.Value;
                            iGrabSizeX = (int)MilIntGrabSizeX;
                        }

                        if (MilIntGrabSizeY > CamParam.GrabSizeY.Value)
                        {
                            UserGrabSizeY = (int)CamParam.GrabSizeY.Value;
                            iGrabSizeY = (int)MilIntGrabSizeY;
                            IsGrabSizeConvert = true;
                        }
                        else
                        {
                            iGrabSizeY = (int)MilIntGrabSizeY;
                            UserGrabSizeY = CamParam.GrabSizeY.Value;
                        }
                        iGrabBand = (int)MilIntGrabBand;
                        iGrabDataType = (int)MilIntGrabDataType;
                        //if (UserGrabSizeX != iGrabSizeX || UserGrabSizeY != iGrabSizeY)
                        //{
                        //    MIL.MbufAlloc2d(MilSystem, UserGrabSizeX, UserGrabSizeY, iGrabDataType, lAttributes, ref milUserBuffer);
                        //}

                        if (IsChangMilBuffer)
                        {
                            if (milGrabBuffer != MIL.M_NULL)
                                MIL.MbufFree(milGrabBuffer);
                            MIL.MbufAlloc2d(MilSystem, MilIntGrabSizeX, MilIntGrabSizeY, iGrabDataType, lAttributes, ref milGrabBuffer);
                            MIL.MbufClear(milGrabBuffer, 0);
                            CurImageBuffer = null;
                            CurImageBuffer = new ImageBuffer(UserGrabSizeX, UserGrabSizeY, iGrabBand, iGrabBand * iGrabDataType);
                        }

                        ArrImageBuff = new byte[UserGrabSizeX * UserGrabSizeX];

                        MIL.MdigControl(MilDigitizer, MIL.M_CORRUPTED_FRAME_ERROR, MIL.M_DISABLE);

                        if (grabberType == EnumGrabberRaft.MILMORPHIS)
                        {
                            if (CamParam.HorizontalFlip.Value != FlipEnum.NONE)
                            {
                                MIL.MdigControl(MilDigitizer, MIL.M_GRAB_DIRECTION_X, MIL.M_REVERSE);
                            }
                            else
                            {
                                MIL.MdigControl(MilDigitizer, MIL.M_GRAB_DIRECTION_X, MIL.M_FORWARD);

                            }
                            if (CamParam.VerticalFlip.Value != FlipEnum.NONE)
                            {
                                MIL.MdigControl(MilDigitizer, MIL.M_GRAB_DIRECTION_Y, MIL.M_REVERSE);
                            }
                            else
                            {
                                MIL.MdigControl(MilDigitizer, MIL.M_GRAB_DIRECTION_Y, MIL.M_FORWARD);

                            }
                        }
                        else if (grabberType == EnumGrabberRaft.MILGIGE)
                        {
                            //if (CamParam.VerticalFlip.Value != FlipEnum.NONE |
                            //    CamParam.HorizontalFlip.Value != FlipEnum.NONE |
                            //    CamParam.Rotate.Value != 0)
                            //{

                            if (CamParam.VerticalFlip.Value != FlipEnum.NONE)                               
                            {
                                GigEReserveY = true;
                                MIL.MbufAlloc2d(MilSystem, MilIntGrabSizeX, MilIntGrabSizeY, iGrabDataType, lAttributes, ref GigeReserveBuffer);

                            }
                            else
                            {
                                GigEReserveY = false;
                            }

                            if (CamParam.HorizontalFlip.Value != FlipEnum.NONE)
                            {
                                MIL.MdigControl(MilDigitizer, MIL.M_GRAB_DIRECTION_X, MIL.M_REVERSE);
                            }
                            else
                            {
                                MIL.MdigControl(MilDigitizer, MIL.M_GRAB_DIRECTION_X, MIL.M_FORWARD);
                            }
                            
                            //if(camparams.HorizontalFlip != FlipEnum.NONE)
                            //{
                            //    MIL.MdigControlFeature(MilDigitizer, MIL.M_GRAB_DIRECTION_X, "M_GRAB_DIRECTION_X", MIL.M_REVERSE, "M_REVERSE");
                            //}
                            //else
                            //{
                            //    MIL.MdigControlFeature(MilDigitizer, MIL.M_GRAB_DIRECTION_X, "M_GRAB_DIRECTION_X", MIL.M_FORWARD, "M_FORWARD");

                            //}
                            //if (camparams.VerticalFlip != FlipEnum.NONE)
                            //{
                            //    MIL.MdigControlFeature(MilDigitizer, MIL.M_GRAB_DIRECTION_Y, "M_GRAB_DIRECTION_Y", MIL.M_REVERSE, "M_REVERSE");
                            //}
                            //else
                            //{
                            //    MIL.MdigControlFeature(MilDigitizer, MIL.M_GRAB_DIRECTION_Y, "M_GRAB_DIRECTION_Y", MIL.M_FORWARD, "M_FORWARD");

                            //}
                        }
                    }
                    else if (CamParam.ColorDept.Value == (int)ColorDept.Color24)
                    {
                        _ColorDept = CamParam.ColorDept.Value;
                        if (MilIntGrabSizeX <= CamParam.GrabSizeX.Value)
                            iGrabSizeX = (int)MilIntGrabSizeX;
                        else
                            iGrabSizeX = CamParam.GrabSizeX.Value;

                        if (MilIntGrabSizeY <= CamParam.GrabSizeY.Value)
                            iGrabSizeY = (int)MilIntGrabSizeY;
                        else
                            iGrabSizeY = CamParam.GrabSizeY.Value;

                        iGrabBand = (int)MilIntGrabBand;
                        iGrabDataType = (int)MilIntGrabDataType;

                        ArrImageBuff = new byte[iGrabSizeX * iGrabSizeY * 3];

                        MIL.MdigControl(MilDigitizer, MIL.M_CORRUPTED_FRAME_ERROR, MIL.M_DISABLE);
                        MIL.MbufAllocColor(MilSystem, 3, iGrabSizeX, iGrabSizeY, 8 + MIL.M_UNSIGNED, cl_iAttributes, ref milGrabColorBuffer);
                        MIL.MbufClear(milGrabColorBuffer, 0);
                    }
                }

            }
            catch (MILException err)
            {
                LoggerManager.Exception(err);
                throw new VisionException("", err, EventCodeEnum.VISION_EXCEPTION, this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw new Exception(err.Message, err);
            }
        }
        public void SetCaller(object assembly)
        {
        }

        public void ContinuousGrab()
        {
            try
            {
                //using (Locker locker = new Locker(lockobj))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return;
                //    }
                lock (lockobj)
                {


                    bContinousGrab = true;
                    ContinusGrab();
                }

            }
            catch (MILException err)
            {
                LoggerManager.Exception(err);
                throw new VisionException("", err, EventCodeEnum.VISION_EXCEPTION, this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw new Exception(err.Message, err);
            }
        }
        public void StopContinuousGrab()
        {
            try
            {
                //using (Locker locker = new Locker(lockobj))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return;
                //    }
                lock (lockobj)
                {
                    HaltGrab();
                    bContinousGrab = false;
                }

            }
            catch (MILException err)
            {
                LoggerManager.Exception(err);
                throw new VisionException("", err, EventCodeEnum.VISION_EXCEPTION, this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw new Exception(err.Message, err);
            }
        }
        //=====Thread 쓰지않음.
        private void ContinusGrab()
        {
            try
            {
                if (_ColorDept == (int)ColorDept.BlackAndWhite)
                {
                    MIL.MdigGrabContinuous(MilDigitizer, milGrabBuffer);
                }
                else if (_ColorDept == (int)ColorDept.Color24)
                {
                    MIL.MdigGrabContinuous(MilDigitizer, milGrabColorBuffer);
                    //MIL.MbufGetColor(milGrabColorBuffer, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BAND, ArrImageBuff);
                }
                //   AreUpdateEvent.WaitOne();
            }
            catch (MILException err)
            {
                LoggerManager.Exception(err);
                throw new VisionException("", err, EventCodeEnum.VISION_EXCEPTION, this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw new Exception(err.Message, err);
            }
        }

        private void HaltGrab()
        {
            try
            {
                if (bContinousGrab)
                {
                    //camera에 문제(connect error 및 지속적인 grab 실패 등)가 발생한 경우, mdig halt는 계속 실패하는 상황이 발생함, 한번만 try 하도록 함
                    //bContinousGrab은 호출 func에서 false로 바꾸어 주고 있음
                    MIL.MdigHalt(MilDigitizer);
                }
            }
            catch (MILException err)
            {
                LoggerManager.Error($"GrabberModuel - HaltGrab Error : " + err.Message);
                LoggerManager.Exception(err);
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GrabberModuel - HaltGrab Error : " + err.Message);
                LoggerManager.Exception(err);
            }
        }

        private SemaphoreSlim singleGrabSemaphore = new SemaphoreSlim(1, 1);// readonly여야하는가?
        public ImageBuffer SingleGrab()
        {
            try
            {
                //using (Locker locker = new Locker(lockobj))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return null;
                //    }
                byte[] arrImageBuff = new byte[ArrImageBuff.Length];

                //lock (singleGrabLockObj)
                //{
                AreUpdateEvent.Reset();
                singleGrabSemaphore.Wait();
                if (bContinousGrab != true)
                {
                    if (_ColorDept == (int)ColorDept.BlackAndWhite)
                    {
                        //그랩 영역 가운데 정렬. 
                        int offsetx = (iGrabSizeX - UserGrabSizeX) / 2;
                        int offsety = (iGrabSizeY - UserGrabSizeY) / 2;

                        arrImageBuff = new byte[UserGrabSizeX * UserGrabSizeX];

                        MIL.MbufClear(milGrabBuffer, 0);
                        MIL.MdigGrab(MilDigitizer, milGrabBuffer);
                        //if(CamParam.DoubleGrabEnable.Value == true)
                        //{
                        //    MIL.MbufClear(milGrabBuffer, 0);
                        //}
                        //LoggerManager.Debug($"MdigGrab(): End");
                        //MIL.MbufSave(@"C:\Logs\Image\Focusing\Focus.bmp", milGrabBuffer);
                        lock (ImageBuffLock)
                        {
                            MIL.MbufGet2d(milGrabBuffer, offsetx, offsety, UserGrabSizeX, UserGrabSizeY, arrImageBuff);
                        }
                        //그랩 이미지데이터를 이용하여 그랩 오류 판단.
                        bool firstGrabSuccess = IsGrabSuccess(arrImageBuff);
                        if (!firstGrabSuccess)
                        {
                            MIL.MdigGrab(MilDigitizer, milGrabBuffer);
                            LoggerManager.Debug($"MdigGrab(): Zero Data Retry");
                        }

                        lock (ImageBuffLock)
                        {
                            if (!IsGrabSizeConvert)
                            {
                                if (!firstGrabSuccess) //1차 그랩 실패라면 재 그랩 데이터가 버퍼에 있으므로 가져오기
                                {
                                    MIL.MbufGet(milGrabBuffer, arrImageBuff);
                                }
                            }
                            else
                            {
                                if (_ColorDept == (int)ColorDept.BlackAndWhite)
                                {
                                    if (GigEReserveY)
                                    {
                                        if (CamParam.Rotate.Value != 0)
                                        {
                                            MIL.MimRotate(milGrabBuffer, GigeReserveBuffer, CamParam.Rotate.Value,
                                                MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT);
                                            MIL.MimFlip(GigeReserveBuffer, GigeReserveBuffer, MIL.M_FLIP_VERTICAL, MIL.M_DEFAULT);
                                        }
                                        else
                                        {
                                            MIL.MimFlip(milGrabBuffer, GigeReserveBuffer, MIL.M_FLIP_VERTICAL, MIL.M_DEFAULT);
                                        }
                                        //MIL.MimFlip(milGrabBuffer, GigeReserveBuffer, MIL.M_FLIP_VERTICAL, MIL.M_DEFAULT);
                                        MIL.MbufGet2d(GigeReserveBuffer, offsetx, offsety, UserGrabSizeX, UserGrabSizeY, arrImageBuff);
                                    }
                                    else
                                    {
                                        if (!firstGrabSuccess)
                                        {
                                            MIL.MbufGet2d(milGrabBuffer, offsetx, offsety, UserGrabSizeX, UserGrabSizeY, arrImageBuff);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (_ColorDept == (int)ColorDept.Color24)
                    {
                        arrImageBuff = new byte[UserGrabSizeX * UserGrabSizeX * 3];
                        MIL_INT resultPresent = MIL.MdigInquire(MilDigitizer, MIL.M_CAMERA_PRESENT);
                        MIL_ID milGrabColorBuffer = MIL.M_NULL;
                        MIL.MbufAllocColor(MilSystem, 3, iGrabSizeX, iGrabSizeY, 8 + MIL.M_UNSIGNED, cl_iAttributes, ref milGrabColorBuffer);
                        MIL.MbufClear(milGrabColorBuffer, 0);
                        MIL.MdigGrab(MilDigitizer, milGrabColorBuffer);
                        MIL.MbufGetColor(milGrabColorBuffer, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BAND, arrImageBuff);
                        MIL.MbufFree(milGrabColorBuffer);
                        milGrabColorBuffer = MIL.M_NULL;
                    }

                    if(ArrImageBuff.Length == arrImageBuff.Length)
                    {
                        arrImageBuff.CopyTo(ArrImageBuff, 0);
                    }

                    CurImageBuffer.Buffer = ArrImageBuff;

                    ImageBuffer imgBuff = new ImageBuffer();
                    CurImageBuffer.CopyTo(imgBuff);
                    imgBuff.CapturedTime = CurImageBuffer.CapturedTime = DateTime.Now;
                    imgBuff.Buffer = arrImageBuff;

                    AreUpdateEvent.Set();
                    singleGrabSemaphore.Release();
                    return imgBuff;
                }
                else
                {
                    singleGrabSemaphore.Release();
                    return null;
                }
            }
            catch (MILException err)
            {
                #region <region> Retry Camera Connect </region>
                int retryCount = 5;
                int retrydelaymsec = 30000;
                EventCodeEnum retryRet = EventCodeEnum.UNDEFINED;
                for (int count = 0; count < retryCount; count++)
                {

                    Thread.Sleep(retrydelaymsec);
                    retryRet = this.VisionManager().DeAllocateCamera(CamParam.ChannelType.Value);
                    retryRet = this.VisionManager().AllocateCamera(CamParam.ChannelType.Value);

                    LoggerManager.Debug($"The CAM#{ _Camchn } SigleGrab Retry Count : {count}");
                    if(retryRet == EventCodeEnum.NONE)
                    {
                        break;
                    }
                }

                #endregion

                if(retryRet == EventCodeEnum.NONE)
                {
                    AreUpdateEvent.Set();
                    singleGrabSemaphore.Release();

                    return SingleGrab();
                }
                else
                {
                    AreUpdateEvent.Set();
                    singleGrabSemaphore.Release();

                    Debug.WriteLine("The CAM#" + _Camchn + "is not responding to control requests.");
                    Debug.WriteLine(err.Message);
                    LoggerManager.Exception(err);
                    throw new VisionException("", err, EventCodeEnum.VISION_EXCEPTION, this);
                }
            }
            catch (Exception err)
            {
                AreUpdateEvent.Set();
                singleGrabSemaphore.Release();
                LoggerManager.Exception(err);
                throw new Exception(err.Message, err);
            }
            finally
            {
                // #Hynix_Merge: 검토 필요, V20 코드 선택, Release 왜 finally에서 안하지?
                //LoggerManager.Debug($"MdigGrab(): Post-process End");

                //MIL.MdigControl(MilDigitizer, MIL.M_GRAB_MODE, MIL.M_ASYNCHRONOUS_QUEUED);

                //if (milGrabBuffer != MIL.M_NULL) MIL.MbufFree(milGrabBuffer); milGrabBuffer = MIL.M_NULL;
                //if (milGrabColorBuffer != MIL.M_NULL) MIL.MbufFree(milGrabColorBuffer); milGrabColorBuffer = MIL.M_NULL;
            }

        }


        /// <summary>
        /// 그랩된 이미지 데이터를 이용하여 그랩 실패 여부 판단.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>전부 0 이면 false </returns>
        private bool IsGrabSuccess(byte[] data)
        {
            //0이 아닌값이 포함되어 있다면 성공. 
            return IsContainsNonZero(data);
        }


        /// <summary>
        /// data에 0이 아닌값이 존재하는지 확인
        /// </summary>
        /// <param name="data"></param>
        /// <returns>1개라도 0이 아닌값이 있다면 true, 전부 0 이면 false </returns>
        private bool IsContainsNonZero(byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != 0)
                {
                    return true;
                }
            }

            return false;
        }

        public ImageBuffer GetCurGrabImage()
        {
            try
            {
                //using (Locker locker = new Locker(lockobj))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return null;
                //    }
                lock (lockobj)
                {
                    return CurImageBuffer;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }



        /// <summary>
        /// 영상획득 이벤트 발생.
        /// </summary>
        protected void OnImage1ReadyEvent(byte[] image)
        {
            try
            {
                if (ImageReadyEvent != null)
                {
                    CurImageBuffer = new ImageBuffer(image, iGrabSizeX, iGrabSizeY, iGrabBand, iGrabBand * iGrabDataType);
                    ImageReadyEvent(CurImageBuffer);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw new Exception(err.Message, err);
            }
        }

        /// <summary>
        /// Grab이 완료되었을때 발생하는 함수. 
        /// GrabThreadProc()에서 MIL.MdigGrab의 실행이 완료되었을때 발생하는 함수.
        /// </summary>
        /// <param name="HookType"></param>
        /// <param name="EventId"></param>
        /// <param name="UserStructPtr"></param>
        /// <returns></returns>
        MIL_INT Digitizer1_GrabEnd(MIL_INT HookType, MIL_ID EventId, IntPtr UserStructPtr)
        {
            try
            {
                if (ImageGrabbed != null)
                {
                    if (bContinousGrab != false)
                    {
                        if (!IsGrabSizeConvert)
                        {
                            if (_ColorDept == (int)ColorDept.BlackAndWhite)
                            {
                                //using (Locker locker = new Locker(ImageBuffLock))
                                //{
                                //    if (locker.AcquiredLock == false)
                                //    {
                                //        System.Diagnostics.Debugger.Break();
                                //        return (-1);
                                //    }
                                lock(ImageBuffLock)
                                { 
                                    //if (GigEReserveY)
                                    //{
                                    //    MIL.MimFlip(milGrabBuffer, milGrabBuffer, MIL.M_FLIP_VERTICAL, MIL.M_DEFAULT);
                                    //}
                                    if(CamParam.Rotate.Value != 0)
                                    {
                                        MIL.MimRotate(milGrabBuffer, milGrabBuffer, CamParam.Rotate.Value,
                                            MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT);
                                    }
                                    MIL.MbufGet(milGrabBuffer, ArrImageBuff);
                                    PassImageToCamera();
                                }
                            }
                            else if (_ColorDept == (int)ColorDept.Color24)
                            {
                                //using (Locker locker = new Locker(ImageBuffLock))
                                //{
                                //    if (locker.AcquiredLock == false)
                                //    {
                                //        System.Diagnostics.Debugger.Break();
                                //        return (-1);
                                //    }
                                lock (ImageBuffLock)
                                {
                                    if (CamParam.Rotate.Value != 0)
                                    {
                                        MIL.MimRotate(milGrabBuffer, milGrabBuffer, CamParam.Rotate.Value,
                                            MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT);
                                    }
                                    MIL.MbufGetColor(milGrabColorBuffer, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BAND, ArrImageBuff);
                                    PassImageToCamera();
                                }
                            }
                        }
                        else
                        {
                            int offsetx = (iGrabSizeX - UserGrabSizeX) / 2;
                            int offsety = (iGrabSizeY - UserGrabSizeY) / 2;
                            //MIL.MbufCopyClip(milUserBuffer,milGrabBuffer, offsetx, offsety);
                            //MIL.MbufCopyColor2d(milUserBuffer, milGrabBuffer, MIL.M_ALL_BANDS,
                            //0, 0, MIL.M_ALL_BANDS, offsetx, offsety, UserGrabSizeX, UserGrabSizeY);

                            if (_ColorDept == (int)ColorDept.BlackAndWhite)
                            {
                                //using (Locker locker = new Locker(ImageBuffLock))
                                //{
                                //    if (locker.AcquiredLock == false)
                                //    {
                                //        System.Diagnostics.Debugger.Break();
                                //        return (-1);
                                //    }

                                //    if (GigEReserveY)
                                //    {
                                //        MIL.MimFlip(milGrabBuffer, GigeReserveBuffer, MIL.M_FLIP_VERTICAL, MIL.M_DEFAULT);
                                //        MIL.MbufGet2d(GigeReserveBuffer, offsetx, offsety, UserGrabSizeX, UserGrabSizeY, ArrImageBuff);
                                //    }
                                //    else
                                //    {
                                //        MIL.MbufGet2d(milGrabBuffer, offsetx, offsety, UserGrabSizeX, UserGrabSizeY, ArrImageBuff);
                                //    }
                                //    PassImageToCamera();
                                //}
                                lock (ImageBuffLock)
                                {
                                    if (GigEReserveY)
                                    {
                                        if (CamParam.Rotate.Value != 0)
                                        {
                                            MIL.MimRotate(milGrabBuffer, GigeReserveBuffer, CamParam.Rotate.Value,
                                                MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT);
                                            MIL.MimFlip(GigeReserveBuffer, GigeReserveBuffer, MIL.M_FLIP_VERTICAL, MIL.M_DEFAULT);
                                        }
                                        else
                                        {
                                            MIL.MimFlip(milGrabBuffer, GigeReserveBuffer, MIL.M_FLIP_VERTICAL, MIL.M_DEFAULT);
                                        }
                                        MIL.MbufGet2d(GigeReserveBuffer, offsetx, offsety, UserGrabSizeX, UserGrabSizeY, ArrImageBuff);
                                    }
                                    else
                                    {
                                        if (CamParam.Rotate.Value != 0)
                                        {
                                            MIL.MimRotate(milGrabBuffer, GigeReserveBuffer, CamParam.Rotate.Value,
                                                MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT);
                                            MIL.MbufGet2d(GigeReserveBuffer, offsetx, offsety, UserGrabSizeX, UserGrabSizeY, ArrImageBuff);
                                        }
                                        else
                                        {
                                            MIL.MbufGet2d(milGrabBuffer, offsetx, offsety, UserGrabSizeX, UserGrabSizeY, ArrImageBuff);
                                        }

                                    }
                                    PassImageToCamera();
                                }
                            }
                            else if (_ColorDept == (int)ColorDept.Color24)
                            {
                                //using (Locker locker = new Locker(ImageBuffLock))
                                //{
                                lock (ImageBuffLock)
                                {
                                    
                                    MIL.MbufGetColor(milGrabColorBuffer, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BAND, ArrImageBuff);
                                    PassImageToCamera();
                                }
                            }
                        }
                    }

                    //lock (ArrImageBuff)
                    //{
                    //    CurImageBuffer.Buffer = ArrImageBuff;
                    //    int digi = (int)MilDigitizer;
                    //    ImageGrabbed(CurImageBuffer);

                    //}

                }

                return (0);
            }
            catch (MILException err)
            {
                LoggerManager.Exception(err);
                return (-1);
                //throw new VisionException("", err, EventCodeEnum.VISION_EXCEPTION, this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw new Exception(err.Message, err);
            }
            finally
            {
                if (bContinousGrab != false)
                {

                }
            }
        }
        private void PassImageToCamera()
        {
            try
            {
                CurImageBuffer.Buffer = ArrImageBuff;
                int digi = (int)MilDigitizer;
                ImageGrabbed(CurImageBuffer);
                //AreUpdateEvent.Set();
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "Grabber PassImageToCamera Error");
                LoggerManager.Exception(err);
                throw err;
            }
        }

        MIL_INT Digitizer1_Color_GrabEnd(MIL_INT HookType, MIL_ID EventId, IntPtr UserStructPtr)
        {
            try
            {
                OnImage1ReadyEvent(ArrImageBuff);
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return (0);
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
                LoggerManager.Exception(err);
                throw err;
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
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        /// <summary>
        /// _MilGrabBuffer,_MilDigitizer 해제
        /// </summary>
        public void Dispose()
        {
            try
            {
                Dispose(true);

            }
            catch (Exception err)
            {
                throw err;
            }

        }
        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (disposed)
                    return;

                if (disposing)
                {
                    try
                    {
                        // Code to dispose the managed resources of the class
                    }
                    catch (Exception) { }
                }

                // Code to dispose the un-managed resources of the class

                _ColorDept = 0;
                UserGrabSizeX = 0;
                UserGrabSizeY = 0;
                MilIntGrabSizeX = 0;
                MilIntGrabSizeY = 0;


                if (bContinousGrab)
                    StopContinuousGrab();

                if (milGrabBuffer != MIL.M_NULL)
                {
                    MIL.MbufFree(milGrabBuffer);
                    milGrabBuffer = MIL.M_NULL;
                }

                if (milGrabColorBuffer != MIL.M_NULL)
                {
                    MIL.MbufFree(milGrabColorBuffer);
                    milGrabColorBuffer = MIL.M_NULL;
                }

                if (milUserBuffer != MIL.M_NULL)
                {
                    MIL.MbufFree(milUserBuffer);
                    milUserBuffer = MIL.M_NULL;
                }

                if (GigeReserveBuffer != MIL.M_NULL)
                {
                    MIL.MbufFree(GigeReserveBuffer);
                    GigeReserveBuffer = MIL.M_NULL;
                }

                // Unhook the function at the start of each frame.
                MIL.MdigHookFunction(MilDigitizer, MIL.M_GRAB_FRAME_END + MIL.M_UNHOOK, MDH_DigiGrabFrameEndDelegate, GCHandle.ToIntPtr(userObjectHandle));

                //// Free GCHandle to allow the garbage collector to reclaim the object.
                userObjectHandle.Free();

                disposed = true;
            }
            catch (MILException err)
            {
                LoggerManager.Exception(err);
                throw new VisionException("", err, EventCodeEnum.VISION_EXCEPTION, this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw new Exception(err.Message, err);
            }
        }

        /// <summary>
        /// 진행중인 Grab 을 멈추는 함수.
        /// </summary>
        public void DigitizerHalt()
        {
            try
            {

            }
            catch (MILException err)
            {
                LoggerManager.Exception(err);
                throw new VisionException("", err, EventCodeEnum.VISION_EXCEPTION, this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw new Exception(err.Message, err);
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

        private void CheckCommunication()
        {
            try
            {
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
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
