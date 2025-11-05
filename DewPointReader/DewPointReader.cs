using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProberInterfaces;
using ProberInterfaces.Temperature.DewPoint;
using ProberErrorCode;
using LogModule;
using System.Runtime.CompilerServices;
using ProberInterfaces.Enum;

namespace Temperature.Temp.DewPoint
{
    public class DewPointReader : IDewPointModule, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public bool Initialized { get; set; } = false;

        private readonly char vbCR = '\r';
        //private readonly double mScaleF_Conv = 14.1667; //상수
        //private readonly double gDewPointMax = 200;     //최대 이슬점 값.(20도)
        //private readonly double gDewPointADMin = 4000;  //DewPoint Sensor 최소 전류.(4mA) 
        //private readonly double gDewPointADMax = 20000; //DewPoint Sensor 최소 전류.(20mA) 

        private SerialPort mSerialPort = null;
        private String mDPRD_Com_Data_Format = null;
        private Task mReadTask = null;
        bool bStopUpdateThread;
        private bool _bIsUpdatting = false;
        private bool readError = false;

        Thread UpdateThread;
        bool bStopUpdateStateThread;
        private IParam _IParam_SysParam;
        public IParam IParam_SysParam
        {
            get { return _IParam_SysParam; }
            set
            {
                if (value != _IParam_SysParam)
                {
                    _IParam_SysParam = value;
                    NotifyPropertyChanged("IParam_SysParam");
                }
            }
        }


        //private DewpointSysParameter _DPSysParam;

        //public DewpointSysParameter DPSysParam
        //{
        //    get { return _DPSysParam; }
        //    set { _DPSysParam = value; }
        //}

        private bool _IsSensorValid;
        public bool IsSensorValid
        {
            get { return _IsSensorValid; }
            set
            {
                if (value != _IsSensorValid)
                {
                    _IsSensorValid = value;
                    NotifyPropertyChanged("IsSensorValid");
                }
            }
        }

        private EnumCommunicationState _DPCommState;

        private DewpointSysParameter _DewpointSysParam;
        public DewpointSysParameter DewpointSysParam
        {
            get { return _DewpointSysParam; }
            set
            {
                if (_DewpointSysParam != value)
                {
                    _DewpointSysParam = value;
                    NotifyPropertyChanged(nameof(DewpointSysParam));
                }
            }
        }

        private double _mCurDewPoint = 9999;
        public double CurDewPoint
        {
            get { return _mCurDewPoint; }
            set
            {
                if (_mCurDewPoint != value)
                {
                    _mCurDewPoint = value;
                    NotifyPropertyChanged(nameof(CurDewPoint));
                }
            }
        }

        /// <summary>
        /// DewPoint 를 읽은 값에 오차를 주기위해 사용됨 ( DewPoint 값 조절용 ) 
        /// dewpoint == 실제 읽은 dewpoint - dewpointoffset
        /// </summary>
        public double DewPointOffset
        {
            get { return this.DewpointSysParam?.DewPointOffset?.Value ?? 0; }
            set { this.DewpointSysParam.DewPointOffset.Value = value; }
        }

        /// <summary>
        /// DewPoint 확인시 ChillerInternalTemp 과 얼마나 차이가 나야 안정적인 온도로 볼지에 대한 차이 값 : CurDewPoint가 높아도 허용하겠다. 
        /// chiller internal temp > 실제 읽은 dewpoint - dewpointoffset - tolerance 
        /// 조건이 만족해야 valve 를 열어야 한다.
        /// </summary>
        public double Tolerence 
        {
            get { return this.DewpointSysParam?.Tolerence?.Value ?? 5.0; }
            set { this.DewpointSysParam.Tolerence.Value = value; }
        }

        /// <summary>
        /// Hysterisis
        /// DewPoint 확인시 ChillerInternalTemp 과 얼마나 차이가 나야 안정적인 온도로 볼지에 대한 차이 값 : CurDewPoint가 더 낮아야만 허용하겠다. 
        /// WaitForDewPoint -> WaitForChiller  :  chiller internal temp - (실제 읽은 dewpoint - dewpointoffset) >= tolerance + Hysteresis 이면 상태 변환 한다.
        /// WaitForChiller  -> WaitForDewPoint : chiller internal temp - (실제 읽은 dewpoint - dewpointoffset) > tolerance 이면 상태 변환 한다.
        /// </summary>
        public double Hysteresis
        {
            get { return this.DewpointSysParam?.Hysteresis?.Value ?? 5.0; }
            set { this.DewpointSysParam.Hysteresis.Value = value; }
        }

        public long WaitTimeout
        {
            get { return this.DewpointSysParam?.WaitTimeout?.Value ?? 60000; }
            set { this.DewpointSysParam.WaitTimeout.Value = value; }
        }


        public bool IsAttached
        {
            get { return this.DewpointSysParam?.IsAttached?.Value ?? false; }
        }

        //Cold 테스트시 DewPoint 설정을 위한 프로퍼티
        private double _EmulDewPoint = -999;
        public double EmulDewPoint
        {
            get { return _EmulDewPoint; }
            set
            {
                if (value != _EmulDewPoint)
                {
                    _EmulDewPoint = value;
                    NotifyPropertyChanged(nameof(EmulDewPoint));
                }
            }
        }

        public DewPointReader()
        {
            try
            {
                mReadTask = new Task(() => UpdateDP());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    if (_DewpointSysParam?.IsAttached.Value == true)
                    {
                        if (DewpointSysParam.ModuleType.Value == ProberInterfaces.Enum.DewPointTypeEnum.COMM_TYPE)
                        {
                            retval = Init();
                            Start();
                        }
                        else
                        {
                            UpdateDPByDirectIO();
                            retval = EventCodeEnum.NONE;
                        }
                    }
                    else
                    {
                        CurDewPoint = this.EnvControlManager().GetChillerModule()?.ChillerInfo?.SetTemp ?? 0;
                        UpdateEmulFormTemp();
                        retval = EventCodeEnum.NONE;
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
                bStopUpdateThread = true;
                bStopUpdateStateThread = true;
                UpdateThread?.Join();

                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
                this.Dispose();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void Start()
        {
            try
            {
                if (_DPCommState == EnumCommunicationState.CONNECTED)
                {
                    Close();
                }

                Open();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {

                //Get param from xml
                if (_DewpointSysParam.PortName != null && _DewpointSysParam.BaudRate.Value != 0)
                {
                    this._DPCommState = EnumCommunicationState.DISCONNECT;
                    this.mSerialPort = new SerialPort();
                    this.mSerialPort.PortName = _DewpointSysParam.PortName.Value;
                    this.mSerialPort.BaudRate = _DewpointSysParam.BaudRate.Value;
                    this.mSerialPort.Parity = _DewpointSysParam.Parity.Value;
                    this.mSerialPort.DataBits = _DewpointSysParam.DataBits.Value;
                    this.mSerialPort.StopBits = _DewpointSysParam.StopBits.Value;

                    this.mSerialPort.ReceivedBytesThreshold = 1;
                    this.mSerialPort.DtrEnable = false;
                    this.mSerialPort.RtsEnable = false;
                    this.mSerialPort.Handshake = Handshake.None;
                    this.mSerialPort.ReadBufferSize = 1024;

                    this._bIsUpdatting = false;

                    this.mDPRD_Com_Data_Format = $"#{_DewpointSysParam.ModAddress.Value:00}{vbCR}";

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    retval = EventCodeEnum.UNDEFINED;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }

        public bool Open()
        {
            bool bRetVal = false;
            try
            {
                bool isOpened = (mSerialPort?.IsOpen) ?? false;
                if (!isOpened)
                {
                    try
                    {
                        mSerialPort?.Open();
                        this._bIsUpdatting = true;
                        mReadTask.Start();
                        this._DPCommState = EnumCommunicationState.CONNECTED;
                        bRetVal = true;
                    }
                    catch (Exception err)
                    {
                        System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                        this._bIsUpdatting = false;
                        mSerialPort?.Close();
                        this._DPCommState = EnumCommunicationState.DISCONNECT;
                        bRetVal = false;
                    }
                }
                else
                {
                    bRetVal = true;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return bRetVal;
        }

        public void Close()
        {
            try
            {
                _bIsUpdatting = false;

                //while (mReadTask.Status != TaskStatus.Running) { }
                while (mReadTask.Status == TaskStatus.Running) { }
                mSerialPort?.Close();
                mSerialPort = null;
                this._DPCommState = EnumCommunicationState.DISCONNECT;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void Dispose()
        {
            try
            {
                Close();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void UpdateDPByDirectIO()
        {
            int defaultInterval = 100;
            int updateInterval = defaultInterval;

            try
            {
                UpdateThread = new Thread(new ThreadStart(UpdateProc));
                UpdateThread.Name = this.GetType().Name;
                bStopUpdateStateThread = false;
                UpdateThread.Start();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void UpdateProc()
        {
            int defaultInterval = 100;
            int updateInterval = defaultInterval;
            bool bErrDetected = false;

            double tmpDewPoint = 9999;
            int errDetectRetryCount = 5;
            int errDetectRetryCurCount = 0;

            while (bStopUpdateStateThread == false)
            {
                try
                {
                    if (this.IOManager().IOServ.AnalogInputs.Count() > 0)
                    {
                        tmpDewPoint = GetCurDewPoint(
                            (int)this.IOManager().IOServ.AnalogInputs[DewpointSysParam.DevIndex.Value].Values[DewpointSysParam.ChannelIndex.Value].PortVal);
                    }
                    else
                    {
                        CurDewPoint = DewpointSysParam.MinAvaDewPoint.Value;
                    }

                    if ((tmpDewPoint > DewpointSysParam.MinAvaDewPoint.Value) && (readError == false))
                    {
                        if (IsSensorValid == false)
                        {
                            updateInterval = DewpointSysParam.UpdateInterval.Value;
                            LoggerManager.Debug($"Dew point sensor recovered. Curr. Dew point = {tmpDewPoint}");
                        }
                        updateInterval = DewpointSysParam.UpdateInterval.Value;
                        IsSensorValid = true;
                        errDetectRetryCurCount = 0;

                        CurDewPoint = tmpDewPoint;
                    }
                    else
                    {
                        if (IsSensorValid == true)
                        {
                            LoggerManager.Debug($"Dew point sensor invliad. Curr. Dew point = {tmpDewPoint}");
                        }
                        IsSensorValid = false;
                        updateInterval = 5000;

                        if(readError == true)
                        {
                            if(errDetectRetryCurCount >= errDetectRetryCount)
                            {
                                CurDewPoint = 99;
                            }
                            else
                            {
                                errDetectRetryCurCount++;
                            }
                        }

                        if(bErrDetected == false && readError == true)
                        {
                            if (bErrDetected == false)
                            {
                                LoggerManager.Debug($"Dew point error state chaged to true.");
                            }
                            bErrDetected = true;
                        }
                    }
                    if (bErrDetected == true && readError == false)
                    {
                        LoggerManager.Debug($"Dew point update error recovered.");

                        bErrDetected = false;
                        updateInterval = DewpointSysParam.UpdateInterval.Value;
                    }
                    Thread.Sleep(updateInterval);
                }
                catch (Exception err)
                {
                    if (bErrDetected == false)
                    {
                        LoggerManager.Debug($"Dew point update error occurred. Err = {err.Message}");
                    }
                    bErrDetected = true;
                    updateInterval = 5000;
                    //System.Threading.Thread.Sleep(updateInterval).Wait();
                    Thread.Sleep(updateInterval);
                }
            }
        }

        public void UpdateDP()
        {
            try
            {
                StringBuilder readData = new StringBuilder();
                while (_bIsUpdatting)
                {
                    if (mSerialPort.IsOpen)
                    {
                        readData.Clear();
                        this.CleanReadDataBuf(); //버퍼 비우기.
                        this.mSerialPort.Write(mDPRD_Com_Data_Format);

                        do
                        {
                            string readed = mSerialPort?.ReadExisting();
                            readData.Append(readed);
                            System.Threading.Thread.Sleep(0);
                        } while (!readData.ToString().Contains(vbCR));

                        string readStr = TrimNull(readData.ToString());

                        if (0 < readStr.Length)
                        {
                            if (readStr[0] == '>')
                            {
                                double trimval = double.Parse((readStr.Split('>'))[1].Trim());
                                int gCurDewPointADval = (int)(trimval * 1000);
                                CurDewPoint = GetCurDewPoint(gCurDewPointADval) * 10;

                                if (EmulDewPoint != -999)
                                {
                                    CurDewPoint = EmulDewPoint;
                                }
                            }
                        }
                    }
                    else
                    {
                        Close();
                    }

                    System.Threading.Thread.Sleep(1000);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void UpdateEmulFormTemp()
        {
            try
            {
                int defaultInterval = 100;
                int updateInterval = defaultInterval;
                bool bErrDetected = false;

                double tmpDewPoint = 9999;
                int errDetectRetryCount = 5;
                int errDetectRetryCurCount = 0;

                if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober)
                {
                    Task.Run(() =>
                    {
                        while (bStopUpdateThread == false)
                        {
                            try
                            {
                               if ((this.EnvControlManager()?.GetChillerModule()?.ChillerParam?.ChillerModuleMode?.Value
                                      ?? EnumChillerModuleMode.NONE) != EnumChillerModuleMode.NONE)
                                {
                                    if (EmulDewPoint == -999)
                                    {
                                        if (CurDewPoint >= this.EnvControlManager().GetChillerModule().ChillerInfo.SetTemp)
                                        {
                                            tmpDewPoint = CurDewPoint - DewPointOffset - Tolerence - Hysteresis;//?? 이게 맞나

                                        }
                                        readError = false;
                                    }
                                    else
                                    {
                                        if(EmulDewPoint == -9999)
                                        {
                                            readError = true;
                                        }
                                        else
                                        {
                                            tmpDewPoint = EmulDewPoint;
                                            readError = false;
                                        }
                                       
                                    }
                                }
                                else
                                {
                                     CurDewPoint = this.TempController().TempInfo?.TargetTemp?.Value ?? 9999;
                                 }
                                 
                                if ((tmpDewPoint > DewpointSysParam.MinAvaDewPoint.Value) && (readError == false))
                                {
                                    if (IsSensorValid == false)
                                    {
                                        updateInterval = DewpointSysParam.UpdateInterval.Value;
                                        LoggerManager.Debug($"Dew point sensor recovered. Curr. Dew point = {tmpDewPoint}");
                                    }
                                    updateInterval = DewpointSysParam.UpdateInterval.Value;
                                    IsSensorValid = true;
                                    errDetectRetryCurCount = 0;

                                    CurDewPoint = tmpDewPoint;
                                }
                                else
                                {
                                    if (IsSensorValid == true)
                                    {
                                        LoggerManager.Debug($"Dew point sensor invliad. Curr. Dew point = {tmpDewPoint}");
                                    }
                                    IsSensorValid = false;
                                    updateInterval = 5000;

                                    if (readError == true)
                                    {
                                        if (errDetectRetryCurCount >= errDetectRetryCount)
                                        {
                                            CurDewPoint = 99;
                                        }
                                        else
                                        {
                                            errDetectRetryCurCount++;
                                        }
                                    }

                                    if (bErrDetected == false && readError == true)
                                    {
                                        if (bErrDetected == false)
                                        {
                                            LoggerManager.Debug($"Dew point error state chaged to true.");
                                        }
                                        bErrDetected = true;
                                    }
                                }
                                if (bErrDetected == true && readError == false)
                                {
                                    LoggerManager.Debug($"Dew point update error recovered.");

                                    bErrDetected = false;
                                    updateInterval = DewpointSysParam.UpdateInterval.Value;
                                }
                                Thread.Sleep(updateInterval);
                            }
                            catch (Exception err)
                            {
                                if (bErrDetected == false)
                                {
                                    LoggerManager.Debug($"Dew point update error occurred. Err = {err.Message}");
                                }
                                bErrDetected = true;
                                updateInterval = 5000;
                                //System.Threading.Thread.Sleep(updateInterval).Wait();
                                Thread.Sleep(updateInterval);
                            }
                        }
                    });
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public string TrimNull(string trimData)
        {
            string[] splitStr = trimData.Split(Convert.ToChar(0));
            string retStr = null;
            try
            {

                if (0 < splitStr.Length)
                    retStr = splitStr[0];

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retStr;
        }

        private double GetCurDewPoint(int gCurDewPointADval)
        {
            double curDP = 0;
            try
            {
                // 최소 전류 값 이하로 들어온 경우
                // DewPointADMax 값은 AnalogInput의 Value 값에 따라 다를 수 있음
                // Beckhoff : 32767 (2^16 - 1)
                // Crevis : 4905 : (2^12 - 1)
                if (DewpointSysParam.DewPointADMax.Value - DewpointSysParam.DewPointADMin.Value == 0)
                {
                    DewpointSysParam.DewPointADMax.Value = 32767;
                    DewpointSysParam.DewPointADMin.Value = DewpointSysParam.DewPointADMax.Value / 30;
                    LoggerManager.Debug($"GetCurDewPoint(): AD Min/Max value error! Restore to default({DewpointSysParam.DewPointADMin.Value }, {DewpointSysParam.DewPointADMax.Value})");
                }
                if (DewpointSysParam.DewPointADMin.Value == 0)
                {
                    DewpointSysParam.DewPointADMin.Value = DewpointSysParam.DewPointADMax.Value / 30;
                    LoggerManager.Debug($"GetCurDewPoint(): AD Min value error! Restore to default({DewpointSysParam.DewPointADMin.Value})");
                }
                if (gCurDewPointADval < DewpointSysParam.DewPointADMin.Value)
                {
                    if(readError == false)
                    {
                        LoggerManager.Debug($"GetCurDerPoint(): AD Value is out of range. AD Val. = {gCurDewPointADval}");
                    }
                    readError = true;
                }
                else
                {
                    curDP = DewpointSysParam.DewPointMaxValue.Value -
                            (double)(1 - (gCurDewPointADval / DewpointSysParam.DewPointADMax.Value)) *
                            (Math.Abs(DewpointSysParam.DewPointMaxValue.Value) +
                            Math.Abs(DewpointSysParam.DewPointMinValue.Value)) +
                            DewpointSysParam.DewPointOffset.Value;
                    readError = false;
                }
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return curDP;
        }

        public void CleanReadDataBuf()
        {
            try
            {
                int eraseReadCount = mSerialPort.BytesToRead;
                byte[] buffer = new byte[eraseReadCount];
                mSerialPort?.Read(buffer, 0, eraseReadCount);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.PARAM_ERROR;
            try
            {
                IParam tmpParam = null;
                RetVal = this.LoadParameter(ref tmpParam, typeof(DewpointSysParameter));

                if (RetVal == EventCodeEnum.NONE)
                {
                    DewpointSysParam = tmpParam as DewpointSysParameter;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.PARAM_ERROR;
            try
            {
                RetVal = this.SaveParameter(DewpointSysParam);
                if (RetVal != EventCodeEnum.NONE)
                {
                    throw new Exception($"[{this.GetType().Name} - SaveAutoTiltSysFile] Faile SaveParameter");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public void SetDewPointTolerance(double tolerance)
        {
            if (DewpointSysParam != null)
            {
                DewpointSysParam.Tolerence.Value = tolerance;
            }
        }

        public void SetDewPointOffset(double offset)
        {
            if (DewpointSysParam != null)
            {
                DewpointSysParam.DewPointOffset.Value = offset;
            }
        }

    }
}
