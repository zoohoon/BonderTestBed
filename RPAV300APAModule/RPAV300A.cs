using LogModule;
using Newtonsoft.Json;
using ProberInterfaces.PreAligner;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProberErrorCode;
using System.Runtime.CompilerServices;
using ProberInterfaces;
using System.Xml.Serialization;
using System.Threading;
using System.Diagnostics;
using Autofac;

namespace PAModule
{
    public class PAVPAModule: IDisposable, INotifyPropertyChanged, IPreAligner
    {

        #region //..PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        private SerialPort serialPort = null;
        private System.Timers.Timer monitoringTimer = null;
        private AutoResetEvent areUpdate = null;
        private static int MonitoringInterValInms = 22;

        public List<object> Nodes { get; set; }


        public string Genealogy { get; set; }
        private Object _Owner;
        [JsonIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }
        #region // Properties
        private IPAState _State;
        public IPAState State
        {
            get { return _State; }
            set
            {
                if (value != _State)
                {
                    _State = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EnumPACONNECTIONState _ConnectionState;
        public EnumPACONNECTIONState ConnectionState
        {
            get { return _ConnectionState; }
            set
            {
                if (value != _ConnectionState)
                {
                    _ConnectionState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EnumPAStatus _PAStatus;
        public EnumPAStatus PAStatus
        {
            get { return _PAStatus; }
            set
            {
                if (value != _PAStatus)
                {
                    _PAStatus = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _ErrorCode;
        public int ErrorCode
        {
            get { return _ErrorCode; }
            set
            {
                if (value != _ErrorCode)
                {
                    _ErrorCode = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _IsAttatched;
        public bool IsAttatched
        {
            get { return _IsAttatched; }
            set
            {
                if (value != _IsAttatched)
                {
                    _IsAttatched = value;
                    RaisePropertyChanged();
                }
            }
        }



        private SerCommInfo _CommParam;
        public SerCommInfo CommParam
        {
            get { return _CommParam; }
            set
            {
                if (value != _CommParam)
                {
                    _CommParam = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        //<0x0D><0x0A>
        private readonly int CR = 0x0D;
        private readonly int LF = 0x0A;
        public PAVPAModule()
        {
            areUpdate = new AutoResetEvent(false);
            monitoringTimer = new System.Timers.Timer(MonitoringInterValInms);
            monitoringTimer.Elapsed += MonitoringTimer_Elapsed; ;
            monitoringTimer.Start();
            State = new PAState();
        }

        private void MonitoringTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            areUpdate.Set();
        }
        ~PAVPAModule()
        {
            Dispose();
        }
        public void Dispose()
        {
            try
            {
                monitoringTimer?.Stop();
                monitoringTimer?.Dispose();
                Close();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool Open()
        {
            bool bRetVal = false;
            try
            {

                if (!((serialPort?.IsOpen) == true ? true : false))
                {
                    try
                    {
                        serialPort?.Open();

                        ConnectionState = EnumPACONNECTIONState.CONNECTED;
                        bRetVal = true;
                    }
                    catch(Exception err)
                    {
                        serialPort?.Close();
                        ConnectionState = EnumPACONNECTIONState.DISCONNECTED;
                        bRetVal = false;
                        LoggerManager.Error($"RPAV300A.Open(): Error occurred. Err = {err.Message}");
                    }
                }
                else
                {
                    serialPort?.Close();
                    ConnectionState = EnumPACONNECTIONState.DISCONNECTED;
                    bRetVal = false;
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
                serialPort?.Close();
                serialPort = null;
                ConnectionState = EnumPACONNECTIONState.DISCONNECTED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");

            //    mMonitoringTimer.Change(Timeout.Infinite, Timeout.Infinite);
            //    mMonitoringTimer.Dispose();
            //    mMonitoringTimer = null;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                //RetVal = this.LoadSysParameter(CommParam);
                
            }
            catch (Exception err)
            {
                LoggerManager.Error($"RPAV300A.LoadSysParameter(): Error occurred. Err = {err.Message}");
            }
            return RetVal;
        }
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                try
                {
                    if (Initialized == false)
                    {
                        if (CommParam?.IsAttached.Value == true)
                        {
                            retval = InitTartget(CommParam);
                            if(retval == EventCodeEnum.NONE)
                            {
                                Initialized = true;
                            }
                        }
                        else
                        {
                            retval = EventCodeEnum.NONE;
                            Initialized = true;
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"DUPLICATE_INVOCATION");
                        retval = EventCodeEnum.DUPLICATE_INVOCATION;
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }

        private EventCodeEnum InitTartget(SerCommInfo info)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            ConnectionState = EnumPACONNECTIONState.DISCONNECTED;
            serialPort = new SerialPort();
            serialPort.PortName = info.PortName.Value;
            serialPort.BaudRate = info.BaudRate.Value;
            serialPort.Parity = info.Parity.Value;
            serialPort.DataBits = info.DataBits.Value;
            serialPort.StopBits = info.StopBits.Value;

            serialPort.ReceivedBytesThreshold = 1;
            serialPort.DtrEnable = false;
            serialPort.RtsEnable = false;
            serialPort.Handshake = Handshake.None;
            serialPort.ReadBufferSize = 1024;

            var isOpened = Open();

            if(isOpened)
            {
                ConnectionState = EnumPACONNECTIONState.CONNECTED;
                ret = EventCodeEnum.NONE;
            }
            else
            {
                ConnectionState = EnumPACONNECTIONState.DISCONNECTED;
                ret = EventCodeEnum.IO_DEV_CONN_ERROR;
            }
            return ret;
        }

        public EventCodeEnum SaveSysParameter()
        {
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum SetParam(IParam param)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            if (param is SerCommInfo)
            {
                CommParam = (SerCommInfo)param;
                ret = EventCodeEnum.NONE;
            }
            else
            {
                ret = EventCodeEnum.IO_PARAM_ERROR;
            }
            return ret;
        }

        private string BuildCommand(EnumRPAVPACommands command, string param)
        {
            string cmd = "";
            switch (command)
            {
                case EnumRPAVPACommands.READ_VER:
                    cmd = "VER\r\n";
                    break;
                case EnumRPAVPACommands.ORG:
                    cmd = $"ORG\r\n";
                    //ORG<CR> < LF >
                    //전축이 원점복귀 합니다.
                    //원점 복귀는 얼라이너의 전원이 인가되고, 작업이 시작되기 전에
                    //반드시 실행되어야 합니다.
                    break;
                case EnumRPAVPACommands.RST:
                    cmd = "RST\r\n";
                    //RST<CR> < LF >
                    //X(가로)축과 Y(세로)축을 초기 위치로 이동합니다.
                    //200mm 웨이퍼와 300mm 웨이퍼는 RST 초기위치가 다릅니다.
                    //얼라이닝 작업이 끝나고, 로봇이 웨이퍼를 가져간 후, 새 웨이퍼가
                    //올려지기 전에, 얼라이너는 반드시 RST 위치로 이동해야 합니다.
                    break;
                case EnumRPAVPACommands.GETPOS:
                    cmd = "APS\r\n";
                    //APS<CR> < LF >
                    //[###] : 회전축 위치 (단위 : pulse, 1pulse = 0.0018°)
                    //[$$$] : 가로축 위치(단위 : mm)
                    //[%%%] : 세로축 위치(단위 : mm)
                    break;
                case EnumRPAVPACommands.VACON:
                    cmd = "VVN\r\n";
                    //VVN<CR> < LF >
                    break;
                case EnumRPAVPACommands.VACOFF:
                    cmd = "VVF\r\n";
                    //VVF<CR> < LF >
                    break;
                case EnumRPAVPACommands.WAF_CHECK_VAC:
                    cmd = "WCH VR1\r\n";
                    //WCH VR1<CR>< LF >
                    //[#] : 웨이퍼 유무여부 (1 : 웨이퍼 있음, 0 : 웨이퍼 없음)
                    break;
                case EnumRPAVPACommands.WAF_CHECK_CCD:
                    cmd = "WCHC VR1\r\n";
                    // 해당 커맨드 사용시 (6/8 inch 의 경우 OCR reading 위치에 있으면 wafer 감지가 안 될 수 있음 )
                    // ccd sensor로 wafer 있는지 없는지 확인하는 커맨드
                    //WCHC VR1<CR>< LF >
                    //[#] : 웨이퍼 유무여부 (1 : 웨이퍼 있음, 0 : 웨이퍼 없음)
                    break;
                case EnumRPAVPACommands.ALIGN:
                    cmd = "ALG\r\n";
                    break;
                case EnumRPAVPACommands.ALIGNANGLE:
                    cmd = $"ALS {param}\r\n";
                    //ALS #<CR><LF>
                    //[#] : 지정변수번호(1~10)
                    break;
                case EnumRPAVPACommands.TURN:
                    cmd = $"TRN {param}\r\n";
                    //TRN ###, $$$<CR><LF>
                    //[###] : 회전축 회전속도 (단위: %, 범위 : 1~100)
                    //[$$$] : 회전축 회전각도(단위: 0.1°)
                    break;
                case EnumRPAVPACommands.DATAWRITE:
                    cmd = $"DWL {param}\r\n";
                    //DWL ###, $$$, %%%, &&&< CR >< LF >
                    //[###] : 변수번호 (범위 : 1~10)
                    //[$$$] : 가로축 위치(단위 : mm)
                    //[%%%] : 세로축 위치(단위 : mm)
                    //[&&&] : 회전축 위치(단위 : 0.1°)
                    break;
                case EnumRPAVPACommands.DATAREAD:
                    cmd = $"UWL {param}\r\n";
                    //UWL ###,VR1,VR2,VR3<CR><LF>
                    //[[###] : 변수번호 (범위 : 1~10)
                    //[$$$] : 가로축 위치(단위 : mm)
                    //[%%%] : 세로축 위치(단위 : mm)
                    //[&&&] : 회전축 위치(단위 : 0.1°)
                    break;
                case EnumRPAVPACommands.READ_ERR:
                    cmd = "ERR\r\n";
                    //ERR<CR> < LF >
                    //E## : 통신 에러코드 (## : 에러번호, 표 4 참조)
                    //### : Pre-Aligner 에러코드 (### : 에러번호, 표 3 참조)
                    break;
                case EnumRPAVPACommands.READ_STATUS:
                    cmd = "STS VR1\r\n";
                    break;
                case EnumRPAVPACommands.READ_STAT:
                    cmd = "IDO 1\r\n";
                    //IDO 1 < CR >< LF >→ ←IDO ##<CR><LF>
                    //Remarks ## : 제어키 상태 (16 진수)
                    //Bit0: 동작중
                    //Bit1: 정지중
                    //Bit2: 원점 복귀 완료 여부
                    //Bit3: Over Run
                    //Bit4: 에러 상태
                    //Bit5:
                    //Bit6: 진공 상태
                    //Example: IDO 1 < CR >< LF >→ ←IDO 06 < CR >< LF > : 06(16 진수) = 00000110(2 진수) → 정지중 + 원점 복귀 완료
                    break;
                case EnumRPAVPACommands.SET_WAF_TYPE:
                    //cmd = "WFT\r\n";
                    //WFT ###
                    //### : 웨이퍼 타입 (1=노치형, 2=플랫형)
                    cmd = $"WFT {param}\r\n";
                    break;
                case EnumRPAVPACommands.SET_WAF_SIZE:
                    //cmd = "WFS\r\n";
                    //WFS ###<CR><LF>
                    //### : 웨이퍼 크기 (200 또는 300)
                    cmd = $"WFS {param}\r\n";
                    break;
                case EnumRPAVPACommands.ALIGN_POS:
                    cmd = $"ALX {param}\r\n";
                    //ALX $$$,%%%,&& &< CR >< LF >
                    //[$$$] : 가로축 위치(단위 : 0.01mm)
                    //[%%%] : 세로축 위치(단위 : 0.01mm)
                    //[&&&] : 회전축 위치(단위 : 0.01°)
                    break;
                case EnumRPAVPACommands.DEV_RESET:
                    cmd = "DRT\r\n";
                    break;
                case EnumRPAVPACommands.MOVEREL:
                    cmd = $"LMI {param}\r\n";
                    break;
                default:
                    break;
            }
            return cmd;
        }
        public void CleanReadDataBuf()
        {
            try
            {
                int eraseReadCount = serialPort.BytesToRead;
                byte[] buffer = new byte[eraseReadCount];
                if(eraseReadCount > 0)
                {
                    serialPort?.Read(buffer, 0, eraseReadCount);
                }
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private string SendCommand(string commandstring)
        {
            StringBuilder readData = new StringBuilder();
            Stopwatch stw = new Stopwatch();
            bool err = false;
            try
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    readData.Clear();
                    lock (serialPort)
                    {
                        this.CleanReadDataBuf(); //버퍼 비우기.

                        byte[] bytes = Encoding.ASCII.GetBytes(commandstring);
                        //this.serialPort?.Write(commandstring);
                        this.serialPort?.Write(bytes, 0, bytes.Length);
                        stw.Reset();
                        stw.Start();
                        do
                        {
                            string readed = serialPort?.ReadExisting();
                            readData.Append(readed);
                            areUpdate.WaitOne(100);
                            if (stw.ElapsedMilliseconds > 45000)
                            {
                                LoggerManager.Debug($"RPAV300A.SendCommand(): Timout occurred. Tartget: {serialPort.PortName}");
                                err = true;
                            }
                        } while (!readData.ToString().Contains((char)CR) & !readData.ToString().Contains((char)LF) & err == false);
                        string command = commandstring.Replace("\r\n", string.Empty);
                        string read = readData.ToString().Replace("\r\n", string.Empty);
                        LoggerManager.Debug($"RPAV300A.SendCommand(): command [{command}]. Response: {read}");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerManager.Exception(ex);
            }
            return readData.ToString();
        }
        #region // PA Commands
        /// <summary>
        /// Examine wafer exist
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum IsSubstrateExist(out bool isExist)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            isExist = false;
            try
            {
                //WAF_CHECK_VAC 사용시 자동으로 Vac 을 켰다 끔 ( 웨이퍼 있어도 끄기 때문에 응답 확인 후 HoldSubstrate or ReleaseSubstrate 로 Vac 정리 해야 함!!!!!!!! )
                EnumRPAVPACommands commands = EnumRPAVPACommands.WAF_CHECK_VAC;
                var cmd = BuildCommand(commands, "");
                var res = SendCommand(cmd);

                var cmdString = cmd.Trim(' ');
                if (res.Contains(cmdString.Remove(3)) == true)
                {
                    //WCH V01=#<CR><LF>
                    var resultCharPos = res.IndexOf("=") + 1;
                    if (res.Remove(0, resultCharPos).Contains("1"))
                    {
                        ret = HoldSubstrate();
                        isExist = true;
                    }
                    else
                    {
                        ret = ReleaseSubstrate();
                        isExist = false;
                    }

                    if (ret == EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"IsSubstrateExist(): Module on {CommParam.PortName.Value}, Wafer state = {isExist}");
                        SetPAStatus(EnumPAStatus.Idle);
                        ret = EventCodeEnum.NONE;
                    }
                }
                else
                {
                    SetPAStatus(EnumPAStatus.Error);
                    LoggerManager.Error($"IsSubstrateExist(): Module on {CommParam.PortName.Value}, response error. Response = [{res}]");
                    ret = EventCodeEnum.LOADER_PA_WAF_MISSED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"IsSubstrateExist(): Module on {CommParam.PortName.Value}, Error occurred. Err = {err.Message}");
                SetPAStatus(EnumPAStatus.Error);
                ret = EventCodeEnum.COMMAND_ERROR;
            }

            return ret;
        }
        /// <summary>
        /// Hold substrate by turn on chuck vacuum
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum HoldSubstrate()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            EnumRPAVPACommands commands = EnumRPAVPACommands.VACON;
            try
            {
                var cmd = BuildCommand(commands, "");
                var res = SendCommand(cmd);
                var cmdString = cmd.Substring(0, 3);
                if (res.Contains(cmdString) == true)
                {
                    LoggerManager.Debug($"HoldSubstrate(): Module on {CommParam.PortName.Value} Vac. ON.");
                    SetPAStatus(EnumPAStatus.Idle);
                    ret = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"HoldSubstrate(): Module on {CommParam.PortName.Value}, response error. Response = [{res}]");
                    SetPAStatus(EnumPAStatus.Error);
                    ret = EventCodeEnum.LOADER_PA_VAC_ERROR;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"HoldSubstrate(): Module on {CommParam.PortName.Value}, Error occurred. Err = {err.Message}");
                SetPAStatus(EnumPAStatus.Error);
                ret = EventCodeEnum.COMMAND_ERROR;
            }

            return ret;
        }
        /// <summary>
        /// Release substrate
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum ReleaseSubstrate()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                EnumRPAVPACommands commands = EnumRPAVPACommands.VACOFF;
                var cmd = BuildCommand(commands, "");
                var res = SendCommand(cmd);

                var cmdString = cmd.Trim(' ');
                if (res.Contains(cmdString) == true)
                {
                    LoggerManager.Debug($"ReleaseSubstrate(): Module on {CommParam.PortName.Value} released.");
                    SetPAStatus(EnumPAStatus.Idle);
                    ret = EventCodeEnum.NONE;
                }
                else
                {
                    if (res.Contains("VVF 003") == true)
                    {
                        commands = EnumRPAVPACommands.VACOFF;
                        cmd = BuildCommand(commands, "");
                        res = SendCommand(cmd);
                        if (res.Contains(cmdString) == true)
                        {
                            LoggerManager.Debug($"ReleaseSubstrate(): Module on {CommParam.PortName.Value} released.");
                            SetPAStatus(EnumPAStatus.Idle);
                            ret = EventCodeEnum.NONE;
                        }
                        else
                        {
                            LoggerManager.Error($"ReleaseSubstrate(): Module on {CommParam.PortName.Value}, response error. Response = [{res}]");
                            if (res.Contains("216") == true | res.Contains("215") == true)
                            {
                                LoggerManager.Debug($"ReleaseSubstrate(): Module on {CommParam.PortName.Value} released.");
                                SetPAStatus(EnumPAStatus.Idle);
                                ret = EventCodeEnum.NONE;
                            }
                            else
                            {
                                SetPAStatus(EnumPAStatus.Error);
                                ret = EventCodeEnum.LOADER_PA_VAC_ERROR;
                            }
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"ReleaseSubstrate(): Module on {CommParam.PortName.Value} return error. Res = {res}");
                        if(res.Contains("216") == true | res.Contains("215") == true)
                        {
                            // Send "DRT" to reset module error datas.
                            ret = ClearError();
                            if (ret != EventCodeEnum.NONE)
                            {
                                LoggerManager.Error($"ReleaseSubstrate(): Module on {CommParam.PortName.Value}, reset failed.");
                                return ret;
                            }

                            // Resend VVF to release wafer on module.
                            commands = EnumRPAVPACommands.VACOFF;
                            cmd = BuildCommand(commands, "");
                            res = SendCommand(cmd);
                            cmdString = cmd.Trim(' ');
                            if (res.Contains(cmdString) == true)
                            {
                                LoggerManager.Debug($"ReleaseSubstrate(): Module on {CommParam.PortName.Value} released.");
                                SetPAStatus(EnumPAStatus.Idle);
                                ret = EventCodeEnum.NONE;
                            }
                            else
                            {
                                LoggerManager.Debug($"ReleaseSubstrate(): Module on {CommParam.PortName.Value} failed with {res} response. Release anyway.");
                                SetPAStatus(EnumPAStatus.Idle);
                                ret = EventCodeEnum.NONE;
                            }
                        }
                        else
                        {
                            SetPAStatus(EnumPAStatus.Error);
                            ret = EventCodeEnum.LOADER_PA_VAC_ERROR;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"ReleaseSubstrate(): Module on {CommParam.PortName.Value}, Error occurred. Err = {err.Message}");
                ret = EventCodeEnum.COMMAND_ERROR;
            }

            return ret;
        }
        /// <summary>
        /// PA relative move. Positions are in um and pulse unit.
        /// </summary>
        /// <param name="posx"></param>
        /// <param name="posy"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public EventCodeEnum MoveTo(double posx, double posy, double angle)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            State.Busy = true;
            try
            {
                ModuleCurPos();

                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(" ");
                var param = $"{posx}";

                #region // LMI
                stringBuilder.Clear();
                stringBuilder.Append(" ");

                param = $"{angle * 10.0}";
                stringBuilder.Append(param);

                param = $",";
                stringBuilder.Append(param);

                param = $"{posy / 1000:0.00}";
                stringBuilder.Append(param);

                param = $",";
                stringBuilder.Append(param);

                param = $"{-1 * posx / 1000:0.00}";
                stringBuilder.Append(param);

                var commands = EnumRPAVPACommands.MOVEREL;
                var cmd = BuildCommand(commands, stringBuilder.ToString());
                var res = SendCommand(cmd);

                var cmdString = cmd.Trim(' ');
                if (res.Contains(cmdString.Remove(3)) == true)
                {
                    ModuleCurPos();
                    LoggerManager.Debug($"MoveTo(): Module on {CommParam.PortName.Value} Move(X={posx}, Y={posy}, Angle={angle}) done.");
                    SetPAStatus(EnumPAStatus.Idle);
                    ret = EventCodeEnum.NONE;
                }
                else if (res.Contains("E"))
                {
                    LoggerManager.Error($"MoveTo(): Module on {CommParam.PortName.Value}, CMD[{cmd}] response error. Response = [{res}]");
                    SetPAStatus(EnumPAStatus.Error);
                    ret = EventCodeEnum.LOADER_PA_MOVE_FAILED;
                }
                else
                {
                    LoggerManager.Error($"MoveTo(): Module on {CommParam.PortName.Value}, response error. Response = [{res}]");
                    SetPAStatus(EnumPAStatus.Error);
                    ret = EventCodeEnum.LOADER_PA_MOVE_FAILED;
                }
                #endregion
            }
            catch (Exception err)
            {
                LoggerManager.Error($"MoveTo(): Module on {CommParam.PortName.Value}, Error occurred. Err = {err.Message}");
                SetPAStatus(EnumPAStatus.Error);
                ret = EventCodeEnum.COMMAND_ERROR;
            }
            finally 
            {
                State.Busy = false;
            }
            return ret;
        }

        /// <summary>
        /// PA relative move. Positions are in um and pulse unit.
        /// </summary>
 
        /// <param name="angle"></param>
        /// <returns></returns>
        public EventCodeEnum MoveTo( double angle)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            State.Busy = true;
            State.AlignDone = false;
            State.PAAlignAbort = false;
            ret = ModuleReset();
            if (ret != EventCodeEnum.NONE)
            {
                State.PAAlignAbort = true;
                LoggerManager.Error($"MoveTo(): Module on {CommParam.PortName.Value}, Reset error.");
                return ret;
            }
            ret = HoldSubstrate();
            if (ret != EventCodeEnum.NONE)
            {
                State.PAAlignAbort = true;
                LoggerManager.Error($"MoveTo(): Module on {CommParam.PortName.Value}, Hold Substrate error.");
                return ret;
            }

            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Clear();
                stringBuilder.Append(" ");
                var param = $"2";
                stringBuilder.Append(param);
                param = $",";
                stringBuilder.Append(param);
                param = $"0";
                stringBuilder.Append(param);
                param = $",";
                stringBuilder.Append(param);
                param = $"0";
                stringBuilder.Append(param);
                param = $",";
                stringBuilder.Append(param);
                param = $"{angle * 10.0}";
                stringBuilder.Append(param);

                EnumRPAVPACommands commands = EnumRPAVPACommands.DATAWRITE;
                var cmd = BuildCommand(commands, stringBuilder.ToString());
                var res = SendCommand(cmd);
                var cmdString = cmd.Trim(' ');
                if (res.Contains(cmdString.Remove(3)) != true)
                {
                    State.PAAlignAbort = true;
                    LoggerManager.Error($"MoveTo(): Module on {CommParam.PortName.Value}, response error. Response = [{res}]");
                    SetPAStatus(EnumPAStatus.Error);
                    return ret = EventCodeEnum.LOADER_PA_DATAWRITE_FAILED;
                }

                stringBuilder.Clear();
                stringBuilder.Append(" ");
                param = $"2";
                stringBuilder.Append(param);

                commands = EnumRPAVPACommands.ALIGNANGLE;
                cmd = BuildCommand(commands, stringBuilder.ToString());
                res = SendCommand(cmd);

                cmdString = cmd.Trim(' ');
                if (res.Contains(cmdString.Remove(3)) == true)
                {
                    LoggerManager.Debug($"MoveTo(): Module on {CommParam.PortName.Value} Angle: {angle} done.");
                    State.AlignDone = true;
                    State.PAAlignAbort = false;
                    SetPAStatus(EnumPAStatus.Idle);
                    ret = EventCodeEnum.NONE;
                }
                else
                {
                    // align error 발생 시 무조건 DRT 필수
                    State.PAAlignAbort = true;
                    ret = ClearError();
                    if (ret != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"MoveTo(): Module on {CommParam.PortName.Value}, Clear Error.");
                    }
                    LoggerManager.Error($"MoveTo(): Module on {CommParam.PortName.Value} Angle: {angle} error. Response = [{res}]");
                    SetPAStatus(EnumPAStatus.Error);
                    ret = EventCodeEnum.LOADER_PA_ALGN_FAILED;
                }
            }
            catch (Exception err)
            {
                State.PAAlignAbort = true;
                LoggerManager.Error($"MoveTo(): Module on {CommParam.PortName.Value}, Error occurred. Err = {err.Message}");
                SetPAStatus(EnumPAStatus.Error);
                ret = EventCodeEnum.COMMAND_ERROR;
            }
            finally 
            {
                State.Busy = false;
            }
            return ret;
        }
        /// <summary>
        /// Pre-align process run
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public EventCodeEnum DoPreAlign(double angle, bool isBusy = false)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            IGPLoader GPLoader = this.GetLoaderContainer().Resolve<IGPLoader>();
            LoggerManager.Debug($"DoPreAlign(): Module on {CommParam.PortName.Value}, Start.");
            if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
            {
                SetPAStatus(EnumPAStatus.Idle);
                State.PAAlignAbort = false;
                State.AlignDone = true;
                return EventCodeEnum.NONE;
            }            

            State.Busy = true;
            State.AlignDone = false;
            State.PAAlignAbort = false;


            ret = ClearError();
            if (ret != EventCodeEnum.NONE)
            {
                State.PAAlignAbort = true;
                LoggerManager.Error($"DoPreAlign(): Module on {CommParam.PortName.Value}, ClearError error.");
                return ret;
            }


            ret = ModuleReset();
            if (ret != EventCodeEnum.NONE)
            {
                State.PAAlignAbort = true;
                LoggerManager.Error($"DoPreAlign(): Module on {CommParam.PortName.Value}, Reset error.");
                return ret;
            }

            ret = HoldSubstrate();
            if (ret != EventCodeEnum.NONE)
            {
                State.PAAlignAbort = true;
                LoggerManager.Error($"DoPreAlign(): Module on {CommParam.PortName.Value}, Hold Substrate error.");
                return ret;
            }

            try
            {
                if (GPLoader != null)
                {
                    if (GPLoader.preAlignerControlItems.Enable && GPLoader.preAlignerControlItems.ControlMode == ExecutionControlMode.NONEXECUTION)
                    {
                        LoggerManager.Debug($"DoPreAlign(): Module on {CommParam.PortName.Value}, Control Mode = {GPLoader.preAlignerControlItems.ControlMode}, selected ret = {GPLoader.preAlignerControlItems.SelectedCode}");
                        ret = GPLoader.preAlignerControlItems.SelectedCode;
                        return ret;
                    }
                }

                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Clear();
                stringBuilder.Append(" ");
                var param = $"1";
                stringBuilder.Append(param);
                param = $",";
                stringBuilder.Append(param);
                param = $"0";
                stringBuilder.Append(param);
                param = $",";
                stringBuilder.Append(param);
                param = $"0";
                stringBuilder.Append(param);
                param = $",";
                stringBuilder.Append(param);
                param = $"{angle * 10.0}";
                stringBuilder.Append(param);

                EnumRPAVPACommands commands = EnumRPAVPACommands.DATAWRITE;

                var cmd = BuildCommand(commands, stringBuilder.ToString());
                var res = SendCommand(cmd);
                var cmdString = cmd.Trim(' ');

                if (res.Contains(cmdString.Remove(3)) != true)
                {
                    State.PAAlignAbort = true;
                    LoggerManager.Error($"DoPreAlign(): Module on {CommParam.PortName.Value}, response error. Response = [{res}]");
                    SetPAStatus(EnumPAStatus.Error);
                    return ret = EventCodeEnum.LOADER_PA_DATAWRITE_FAILED;
                }

                stringBuilder.Clear();
                stringBuilder.Append(" ");
                param = $"1";
                stringBuilder.Append(param);

                commands = EnumRPAVPACommands.ALIGNANGLE;
                cmd = BuildCommand(commands, stringBuilder.ToString());
                res = SendCommand(cmd);

                cmdString = cmd.Trim(' ');

                if (res.Contains(cmdString.Remove(3)) == true)
                {
                    var resultString = res.Remove(0, 3);

                    int indexOfCR = resultString.IndexOf('\r');
                    if (indexOfCR >= 0) resultString = resultString.Remove(indexOfCR, resultString.Length - indexOfCR);

                    if (resultString.Length > 0)
                    {
                        State.PAAlignAbort = true;
                        LoggerManager.Debug($"DoPreAlign(): Module on {CommParam.PortName.Value} Align. Failed. Response = {resultString}");
                        ret = ClearError();
                        if (ret != EventCodeEnum.NONE)
                            if (res.Count() > 0)
                                LoggerManager.Error($"DoPreAlign(): Module on {CommParam.PortName.Value}, Clear Error.");
                        LoggerManager.Debug($"CCD Sample Dump.");
                        SetPAStatus(EnumPAStatus.Error);

                        ret = EventCodeEnum.LOADER_FIND_NOTCH_FAIL;
                    }
                    else
                    {
                        LoggerManager.Debug($"DoPreAlign(): Module on {CommParam.PortName.Value} Align. done.");
                        State.PAAlignAbort = false;
                        State.AlignDone = true;
                        SetPAStatus(EnumPAStatus.Idle);
                        ret = EventCodeEnum.NONE;
                    }
                }
                else
                {
                    State.PAAlignAbort = true;
                    LoggerManager.Error($"DoPreAlign(): Module on {CommParam.PortName.Value}, response error. Response = [{res}]");
                    ret = ClearError();
                    if (ret != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"DoPreAlign(): Module on {CommParam.PortName.Value}, Clear Error.");
                    }
                    SetPAStatus(EnumPAStatus.Error);
                    ret = EventCodeEnum.LOADER_FIND_NOTCH_FAIL;
                }

                ModuleCurPos();
                LoggerManager.Debug($"DoPreAlign(): Module on {CommParam.PortName.Value}, Target Angle = {angle:0.000}, Curr. Pos(Angle(pulse, 0.0018deg), X(mm), Y(mm) = {res}");
            }
            catch (Exception err)
            {
                State.PAAlignAbort = true;
                LoggerManager.Error($"DoPreAlign(): Module on {CommParam.PortName.Value}, Error occurred. Err = {err.Message}");
                SetPAStatus(EnumPAStatus.Error);
                ret = EventCodeEnum.COMMAND_ERROR;
            }
            finally
            {
                State.Busy = false;

                if (GPLoader != null)
                {
                    if (GPLoader.preAlignerControlItems.Enable && GPLoader.preAlignerControlItems.ControlMode == ExecutionControlMode.COMMANDEXECUTION)
                    {
                        LoggerManager.Debug($"DoPreAlign(): Module on {CommParam.PortName.Value}, Control Mode = {GPLoader.preAlignerControlItems.ControlMode}, Original ret = {ret}, selected ret = {GPLoader.preAlignerControlItems.SelectedCode}");
                        ret = GPLoader.preAlignerControlItems.SelectedCode;
                    }
                }

                if (ret != EventCodeEnum.NONE)//TODO: 상위에서 Notify하는게 맞지않을까? 
                {
                    if (ret == EventCodeEnum.LOADER_FIND_NOTCH_FAIL)
                    {
                        this.NotifyManager().Notify(EventCodeEnum.LOADER_FIND_NOTCH_FAIL);
                    }
                    else if (ret == EventCodeEnum.MONITORING_PREALIGN_VAC_ERROR)
                    {
                        this.NotifyManager().Notify(EventCodeEnum.LOADER_PA_VAC_ERROR);
                    }
                    this.NotifyManager().Notify(EventCodeEnum.LOADER_PA_FAILED);
                }
            }

            return ret;
        }
        /// <summary>
        /// Pre-align process run
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public Task<EventCodeEnum> DoPreAlignAsync(double angle)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            Task<EventCodeEnum> taskPA = new Task<EventCodeEnum>(() =>
            {
                State.Busy = true;
                State.AlignDone = false;
                State.PAAlignAbort = false;
                ret = ModuleReset();
                if (ret != EventCodeEnum.NONE)
                {
                    State.PAAlignAbort = true;
                    LoggerManager.Error($"DoPreAlignAsync(): Module on {CommParam.PortName.Value}, Reset error.");
                    return ret;
                }
                ret = HoldSubstrate();
                if (ret != EventCodeEnum.NONE)
                {
                    State.PAAlignAbort = true;
                    LoggerManager.Error($"DoPreAlignAsync(): Module on {CommParam.PortName.Value}, Hold Substrate error.");
                    return ret;
                }

                try
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Clear();
                    stringBuilder.Append(" ");
                    var param = $"1";
                    stringBuilder.Append(param);
                    param = $",";
                    stringBuilder.Append(param);
                    param = $"0";
                    stringBuilder.Append(param);
                    param = $",";
                    stringBuilder.Append(param);
                    param = $"0";
                    stringBuilder.Append(param);
                    param = $",";
                    stringBuilder.Append(param);
                    param = $"{angle * 10.0}";
                    stringBuilder.Append(param);

                    var commands = EnumRPAVPACommands.DATAWRITE;
                    var cmd = BuildCommand(commands, stringBuilder.ToString());
                    var res = SendCommand(cmd);
                    var cmdString = cmd.Trim(' ');
                    if (res.Contains(cmdString.Remove(3)) != true)
                    {
                        State.PAAlignAbort = true;
                        LoggerManager.Error($"DoPreAlignAsync(): Module on {CommParam.PortName.Value}, response error. Response = [{res}]");
                        SetPAStatus(EnumPAStatus.Error);
                        ret = EventCodeEnum.LOADER_PA_DATAWRITE_FAILED;
                        return ret;
                    }

                    stringBuilder.Clear();
                    stringBuilder.Append(" ");
                    param = $"1";
                    stringBuilder.Append(param);

                    commands = EnumRPAVPACommands.ALIGNANGLE;
                    cmd = BuildCommand(commands, stringBuilder.ToString());
                    res = SendCommand(cmd);

                    cmdString = cmd.Trim(' ');
                    if (res.Contains(cmdString.Remove(3)) == true)
                    {
                        LoggerManager.Debug($"DoPreAlignAsync(): Module on {CommParam.PortName.Value} Align. done.");
                        SetPAStatus(EnumPAStatus.Idle);
                        State.PAAlignAbort = false;
                        State.AlignDone = true;
                        ret = EventCodeEnum.NONE;
                    }
                    else
                    {
                        State.PAAlignAbort = true;
                        LoggerManager.Error($"DoPreAlignAsync(): Module on {CommParam.PortName.Value}, response error. Response = [{res}]");
                        // align error 발생 시 DRT 필수
                        ret = ClearError();
                        if (ret != EventCodeEnum.NONE)
                        {
                            LoggerManager.Error($"DoPreAlignAsync(): Module on {CommParam.PortName.Value}, Clear Error.");
                        }
                        SetPAStatus(EnumPAStatus.Error);
                        ret = EventCodeEnum.LOADER_PA_ALGN_FAILED;
                    }
                    ModuleCurPos();
                }
                catch (Exception err)
                {
                    State.PAAlignAbort = true;
                    LoggerManager.Error($"DoPreAlignAsync(): Module on {CommParam.PortName.Value}, Error occurred. Err = {err.Message}");
                    ret = EventCodeEnum.COMMAND_ERROR;
                }
                finally 
                {
                    State.Busy = false;
                }
                return ret;
            });

            taskPA.Start();
            return taskPA;
        }
        /// <summary>
        /// Module reset method
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum ModuleReset()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                EnumRPAVPACommands commands = EnumRPAVPACommands.RST;
                var cmd = BuildCommand(commands, "");
                var res = SendCommand(cmd);

                var cmdString = cmd.Trim(' ');
                var needDRT = res.Contains("E02");
                if(res.Contains("E02") | res.Contains("002"))
                {
                    needDRT = true;
                }
                if (needDRT == true)
                {
                    ret = ClearError();
                    if (ret != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"ModuleReset(): Module on {CommParam.PortName.Value} init. done(DRT).");
                    }
                    else 
                    {
                        //RST 응답이 E02 일 경우 DRT 를 해줘야 한다고 명시 되어있음 
                        //DRT 후 다시 RST 하기
                        LoggerManager.Debug($"ModuleReset(): Module on {CommParam.PortName.Value} init. done(DRT).");
                        commands = EnumRPAVPACommands.RST;
                        cmd = BuildCommand(commands, "");
                        res = SendCommand(cmd);
                    }
                }

                if (res.Contains(cmdString.Remove(3)) == true)
                {
                    LoggerManager.Debug($"ModuleReset(): Module on {CommParam.PortName.Value} init. done.");
                    SetPAStatus(EnumPAStatus.Idle);
                    ret = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"ModuleReset(): Module on {CommParam.PortName.Value}, response error. Response = [{res}]");
                    SetPAStatus(EnumPAStatus.Error);
                    ret = EventCodeEnum.LOADER_PA_RST_MOVE_FAILED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"ModuleReset(): Module on {CommParam.PortName.Value}, Error occurred. Err = {err.Message}");
                SetPAStatus(EnumPAStatus.Error);
                ret = EventCodeEnum.COMMAND_ERROR;
            }

            return ret;
        }

        public EventCodeEnum ModuleCurPos()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                var cmd = BuildCommand(EnumRPAVPACommands.GETPOS, "");
                var res = SendCommand(cmd);
                LoggerManager.Debug($"ModuleCurPos(): Module on {CommParam.PortName.Value}, Curr. Pos(Angle(pulse, 0.0018deg), X(mm), Y(mm) = {res}");
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"ModuleCurPos(): Module on {CommParam.PortName.Value}, Error occurred. Err = {err.Message}");
                ret = EventCodeEnum.COMMAND_ERROR;
            }

            return ret;
        }
        public int WaitForPA(int timeOut)
        {
            int retVal = 0;
            Stopwatch elapsedStopWatch = new Stopwatch();
            elapsedStopWatch.Reset();
            elapsedStopWatch.Start();
            try
            {
                bool runFlag = true;
                do
                {
                    System.Threading.Thread.Sleep(100);
                    //UpdateState();
                    if (State.Busy == false)
                    {
                        retVal = 0;
                        runFlag = false;
                    }
                    else
                    {
                        if (elapsedStopWatch.ElapsedMilliseconds > timeOut)
                        {
                            retVal = -2;
                            runFlag = false;
                        }
                        else
                        {
                            runFlag = true;
                        }
                    }
                    System.Threading.Thread.Sleep(10);
                } while (runFlag == true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {
                elapsedStopWatch?.Stop();

            }
            return retVal;

        }

     

        /// <summary>
        /// Update States
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum UpdateState()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            EnumRPAVPACommands commands = EnumRPAVPACommands.READ_STAT;
            try
            {
                var cmd = BuildCommand(commands, "");
                var res = SendCommand(cmd);

                var cmdString = cmd.Trim(' ');

                if (res.Contains(cmdString.Remove(3)) == true)
                {
                    //←IDO ##<CR><LF>
                    var strState = res.Remove(0, 3);
                    int state;
                    if(int.TryParse(strState, out state))
                    {
                        State.SetState(state);
                        SetPAStatus(EnumPAStatus.Idle);
                        ret = EventCodeEnum.NONE;
                    }
                    else
                    {
                        LoggerManager.Debug($"UpdateState(): Module on {CommParam.PortName.Value} returned data error. Response = [{res}]");
                        SetPAStatus(EnumPAStatus.Error);
                        ret = EventCodeEnum.PARAM_ERROR;
                    }
                }
                else
                {
                    LoggerManager.Error($"UpdateState(): Module on {CommParam.PortName.Value}, response error. Response = [{res}]");
                    SetPAStatus(EnumPAStatus.Error);
                    ret = EventCodeEnum.PARAM_ERROR;
                }


                if(ret != EventCodeEnum.NONE)
                {
                    ret = ClearError();
                    if(ret == EventCodeEnum.NONE)
                    {
                        res = SendCommand(cmd.Replace(" ",""));

                        cmdString = cmd.Trim(' ');

                        if (res.Contains(cmdString.Remove(3)) == true)
                        {
                            //←IDO ##<CR><LF>
                            var strState = res.Remove(0, 3);
                            int state;
                            if (int.TryParse(strState, out state))
                            {
                                State.SetState(state);
                                SetPAStatus(EnumPAStatus.Idle);
                                ret = EventCodeEnum.NONE;
                            }
                            else
                            {
                                LoggerManager.Debug($"UpdateState(): Module on {CommParam.PortName.Value} returned data error. Response = [{res}]");
                                SetPAStatus(EnumPAStatus.Error);
                                ret = EventCodeEnum.PARAM_ERROR;
                            }
                        }
                        else
                        {
                            LoggerManager.Error($"UpdateState(): Module on {CommParam.PortName.Value}, response error. Response = [{res}]");
                            SetPAStatus(EnumPAStatus.Error);
                            ret = EventCodeEnum.PARAM_ERROR;
                        }
                    }

                   
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"UpdateState(): Module on {CommParam.PortName.Value}, Error occurred. Err = {err.Message}");
                SetPAStatus(EnumPAStatus.Error);
                ret = EventCodeEnum.COMMAND_ERROR;
            }

            return ret;
        }
        public EventCodeEnum ClearError()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            EnumRPAVPACommands commands = EnumRPAVPACommands.DEV_RESET;
            try
            {
                var cmd = BuildCommand(commands, "");
                var res = SendCommand(cmd);

                var cmdString = cmd.Trim(' ');
                if (res.Contains(cmdString.Remove(3)) == true)
                {
                    LoggerManager.Debug($"ClearError(): Module on {CommParam.PortName.Value} reset done.");
                    SetPAStatus(EnumPAStatus.Idle);
                    ret = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"ClearError(): Module on {CommParam.PortName.Value}, response error. Response = [{res}]");
                    SetPAStatus(EnumPAStatus.Error);
                    ret = EventCodeEnum.LOADER_PA_ERROR_CLEAR_FAILED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"ClearError(): Module on {CommParam.PortName.Value}, Error occurred. Err = {err.Message}");
                ret = EventCodeEnum.COMMAND_ERROR;
            }

            return ret;
        }

        /// <summary>
        /// Module initializing
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum ModuleInit()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            ret = ClearError();
            if(ret != EventCodeEnum.NONE)
            {
                LoggerManager.Error($"ModuleInit(): Module on {CommParam.PortName.Value}, reset failed.");
                return ret;
            }

            try
            {
                EnumRPAVPACommands commands = EnumRPAVPACommands.ORG;
                var cmd = BuildCommand(commands, "");
                var res = SendCommand(cmd);

                var cmdString = cmd.Trim(' ');
                if (res.Contains(cmdString.Remove(3)) == true)
                {
                    LoggerManager.Debug($"ModuleInit(): Module on {CommParam.PortName.Value} init. done.");
                    SetPAStatus(EnumPAStatus.Idle);
                    ret = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"ModuleInit(): Module on {CommParam.PortName.Value}, response error. Response = [{res}]");
                    SetPAStatus(EnumPAStatus.Error);
                    ret = EventCodeEnum.LOADER_PA_ORG_FAILED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"ModuleInit(): Module on {CommParam.PortName.Value}, Error occurred. Err = {err.Message}");
                SetPAStatus(EnumPAStatus.Error);
                ret = EventCodeEnum.COMMAND_ERROR;
            }
            
            return ret;
        }

        /// <summary>
        /// Module initializing
        /// </summary>
        /// <returns></returns>
        public Task<EventCodeEnum> ModuleInitAsync()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            Task<EventCodeEnum> taskPA = new Task<EventCodeEnum>(()=>
            {
                ret = ClearError();
                if (ret != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"ModuleInitAsync(): Module on {CommParam.PortName.Value}, reset failed.");
                    return ret;
                }
                EnumRPAVPACommands commands = EnumRPAVPACommands.ORG;
                try
                {
                    var cmd = BuildCommand(commands, "");
                    var res = SendCommand(cmd);

                    var cmdString = cmd.Trim(' ');
                    if (res.Contains(cmdString.Remove(3)) == true)
                    {
                        LoggerManager.Debug($"ModuleInitAsync(): Module on {CommParam.PortName.Value} init. done.");
                        SetPAStatus(EnumPAStatus.Idle);
                        ret = EventCodeEnum.NONE;
                    }
                    else
                    {
                        LoggerManager.Error($"ModuleInitAsync(): Module on {CommParam.PortName.Value}, response error. Response = [{res}]");
                        SetPAStatus(EnumPAStatus.Error);
                        ret = EventCodeEnum.LOADER_PA_ORG_FAILED;
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"ModuleInitAsync(): Module on {CommParam.PortName.Value}, Error occurred. Err = {err.Message}");
                    SetPAStatus(EnumPAStatus.Error);
                    ret = EventCodeEnum.COMMAND_ERROR;
                }

                return ret;
            });
            return taskPA;
        }

        public EventCodeEnum Rotate(double angle)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            EnumRPAVPACommands commands = EnumRPAVPACommands.TURN;
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(" ");
                //TRN ###, $$$<CR><LF>
                //[###] : 회전축 회전속도 (단위: %, 범위 : 1~100)
                //[$$$] : 회전축 회전각도(단위: 0.1°)
                var param = $"100";
                stringBuilder.Append(param);
                param = $",";
                param = $"{angle / 10000.0 * 1000.0}";
                stringBuilder.Append(param);

                var cmd = BuildCommand(commands, stringBuilder.ToString());
                var res = SendCommand(cmd);

                var cmdString = cmd.Trim(' ');

                if (res.Contains(cmdString.Remove(3)) == true)
                {
                    LoggerManager.Debug($"Rotate(): Module on {CommParam.PortName.Value} init. done.");
                    SetPAStatus(EnumPAStatus.Idle);
                    ret = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"Rotate(): Module on {CommParam.PortName.Value}, response error. Response = [{res}]");
                    SetPAStatus(EnumPAStatus.Error);
                    ret = EventCodeEnum.LOADER_PA_ROTATE_FAILED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Rotate(): Module on {CommParam.PortName.Value}, Error occurred. Err = {err.Message}");
                SetPAStatus(EnumPAStatus.Error);
                ret = EventCodeEnum.COMMAND_ERROR;
            }

            return ret;
        }
        /// <summary>
        /// Set target wafer size. Call before excute align.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public EventCodeEnum SetDeviceSize(SubstrateSizeEnum size, WaferNotchTypeEnum notch)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            StringBuilder stringBuilder = new StringBuilder();
            try
            {
                if (size == SubstrateSizeEnum.INCH6)
                {
                    if (notch != WaferNotchTypeEnum.FLAT)
                    {
                        LoggerManager.Error($"SetDeviceSize(): Device set {notch} => FLAT");
                        notch = WaferNotchTypeEnum.FLAT;
                    }
                }
                else if (size == SubstrateSizeEnum.INVALID || size == SubstrateSizeEnum.UNDEFINED) 
                {
                    LoggerManager.Error($"SetDeviceSize(): Device set {notch} => FLAT");
                    return EventCodeEnum.MONITORING_WAFER_SIZE_ERROR;
                }

                ret = ClearError();
                if (ret != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"SetDeviceSize(): Module on {CommParam.PortName.Value}, Clear Error failed.");
                }

                stringBuilder.Append(" ");
                var param = $"200";
                switch (size)
                {
                    case SubstrateSizeEnum.INVALID:
                        param = "200";
                        break;
                    case SubstrateSizeEnum.UNDEFINED:
                        param = "200";
                        break;
                    case SubstrateSizeEnum.INCH6:
                        param = "150";
                        break;
                    case SubstrateSizeEnum.INCH8:
                        param = "200";
                        break;
                    case SubstrateSizeEnum.INCH12:
                        param = "300";
                        break;
                    case SubstrateSizeEnum.CUSTOM:
                        param = "200";
                        break;
                    default:
                        break;
                }
                stringBuilder.Clear();
                stringBuilder.Append(param);
                var commands = EnumRPAVPACommands.SET_WAF_SIZE;
                var cmd = BuildCommand(commands, stringBuilder.ToString());
                var res = SendCommand(cmd);

                var cmdString = cmd.Trim(' ');

                if (res.Contains(cmdString.Remove(3)) == true)
                {
                    if(res.Contains(param) == true | res.Length == 5)
                    {
                        LoggerManager.Debug($"SetDeviceSize(): Device set as {size} for module on {CommParam.PortName.Value}");
                        SetPAStatus(EnumPAStatus.Idle);
                        ret = EventCodeEnum.NONE;
                    }
                    else
                    {
                        LoggerManager.Error($"SetDeviceSize(): Target device {size}, Module on {CommParam.PortName.Value}, response error. Response = [{res.Remove(res.Length > 0? res.Length - 2: res.Length)}]");
                        SetPAStatus(EnumPAStatus.Error);
                        ret = EventCodeEnum.LOADER_PA_DATAWRITE_FAILED;
                    }
                }
                else
                {
                    LoggerManager.Error($"SetDeviceSize(): Target device {size}, Module on {CommParam.PortName.Value}, response error. Response = [{res.Length - 2}]");
                    SetPAStatus(EnumPAStatus.Error);
                    ret = EventCodeEnum.LOADER_PA_DATAWRITE_FAILED;
                }


                #region // Notch type

                param = $"1";
                switch (notch)
                {
                    case WaferNotchTypeEnum.UNKNOWN:
                        break;
                    case WaferNotchTypeEnum.FLAT:
                        param = $"2";
                        break;
                    case WaferNotchTypeEnum.NOTCH:
                        param = $"1";
                        break;
                    default:
                        break;
                }

                stringBuilder.Clear();
                stringBuilder.Append(param);
                commands = EnumRPAVPACommands.SET_WAF_TYPE;
                cmd = BuildCommand(commands, stringBuilder.ToString());
                res = SendCommand(cmd);

                cmdString = cmd.Trim(' ');

                if (res.Contains(cmdString.Remove(3)) == true)
                {
                    if (res.Contains(param) == true | res.Length == 5)
                    {
                        LoggerManager.Debug($"SetDeviceSize(): Device set as {notch} for module on {CommParam.PortName.Value}");
                        SetPAStatus(EnumPAStatus.Idle);
                        ret = EventCodeEnum.NONE;
                    }
                    else
                    {
                        LoggerManager.Error($"SetDeviceSize(): Device set as {notch}, Module on {CommParam.PortName.Value}, response error. Response = [{res.Remove(res.Length > 0 ? res.Length - 2 : res.Length)}]");
                        SetPAStatus(EnumPAStatus.Error);
                        ret = EventCodeEnum.LOADER_PA_DATAWRITE_FAILED;
                    }
                }
                else
                {
                    LoggerManager.Error($"SetDeviceSize(): Target device {notch}, Module on {CommParam.PortName.Value}, response error. Response = [{res}]");
                    SetPAStatus(EnumPAStatus.Error);
                    ret = EventCodeEnum.LOADER_PA_DATAWRITE_FAILED;
                }
                #endregion

            }
            catch (Exception err)
            {
                LoggerManager.Error($"SetDeviceSize(): Target device {size}, {notch}, Module on {CommParam.PortName.Value}, Error occurred. Err = {err.Message}");
                SetPAStatus(EnumPAStatus.Error);
                ret = EventCodeEnum.COMMAND_ERROR;
            }

            return ret;
        }

        // pa Status commands

        public EventCodeEnum SetPAStatus(EnumPAStatus enumPAStatus)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                this.PAStatus = enumPAStatus;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"SetPAStatus(): Module on {CommParam.PortName.Value}, Error occurred. Err = {err.Message}");
                ret = EventCodeEnum.COMMAND_ERROR;
            }

            return ret;
        }

        public EnumPAStatus GetPAStatus()
        {
            EnumPAStatus ret = EnumPAStatus.Error;
            try
            {
                ret = this.PAStatus;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"ModuleInit(): Module on {CommParam.PortName.Value}, Error occurred. Err = {err.Message}");
            }

            return ret;
        }

        #endregion
    }
    public class PAState : IPAState, INotifyPropertyChanged
    {

        #region //..PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        private bool _Busy;
        public bool Busy
        {
            get { return _Busy; }
            set
            {
                if (value != _Busy)
                {
                    _Busy = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _OriginDone;
        public bool OriginDone
        {
            get { return _OriginDone; }
            set
            {
                if (value != _OriginDone)
                {
                    _OriginDone = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _OverRun;
        public bool OverRun
        {
            get { return _OverRun; }
            set
            {
                if (value != _OverRun)
                {
                    _OverRun = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _Error;
        public bool Error
        {
            get { return _Error; }
            set
            {
                if (value != _Error)
                {
                    _Error = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _VacStatus;
        public bool VacStatus
        {
            get { return _VacStatus; }
            set
            {
                if (value != _VacStatus)
                {
                    _VacStatus = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _AlignDone;
        public bool AlignDone
        {
            get { return _AlignDone; }
            set
            {
                if (value != _AlignDone)
                {
                    _AlignDone = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _PAAlignAbort;
        public bool PAAlignAbort
        {
            get { return _PAAlignAbort; }
            set
            {
                if (value != _PAAlignAbort)
                {
                    _PAAlignAbort = value;
                    RaisePropertyChanged();
                }
            }
        }

        public void SetState(int state)
        {
            //Bit0: 동작중
            //Bit1 : 정지중
            //Bit2 : 원점 복귀 완료 여부
            //Bit3: Over Run
            //Bit4: 에러 상태
            //Bit5:
            //Bit6: 진공 상태
            Busy = ((state >> 0) & 0x01) == 0x01;

            OriginDone = ((state >> 0) & 0x02) == 0x01;
            OverRun = ((state >> 0) & 0x03) == 0x01;
            Error = ((state >> 0) & 0x04) == 0x01;
            VacStatus = ((state >> 0) & 0x06) == 0x01;
        }
    }
    [Serializable]
    public class SerCommInfo : INotifyPropertyChanged, ISystemParameterizable, IParamNode
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }


        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);


                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        private void PropertyChange(String propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        public void SetElementMetaData()
        {

        }
        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            IsAttached.Value = false;
            PortName.Value = "COM1";
            BaudRate.Value = 57600;
            DataBits.Value = 8;
            StopBits.Value = System.IO.Ports.StopBits.One;
            Parity.Value = System.IO.Ports.Parity.None;
            ModAddress.Value = 1;
            return EventCodeEnum.NONE;
        }

        private Element<bool> _IsAttached
             = new Element<bool>();
        public Element<bool> IsAttached
        {
            get
            {
                return _IsAttached;
            }
            set
            {
                _IsAttached = value;
                PropertyChange(nameof(IsAttached));
            }
        }

        private Element<String> _PortName
             = new Element<string>();
        public Element<String> PortName
        {
            get { return _PortName; }
            set
            {
                if (_PortName != value)
                {
                    _PortName = value;
                    PropertyChange(nameof(PortName));
                }
            }
        }
        private Element<int> _BaudRate
             = new Element<int>();
        public Element<int> BaudRate
        {
            get { return _BaudRate; }
            set
            {
                _BaudRate = value;
                PropertyChange(nameof(BaudRate));
            }
        }
        private Element<int> _DataBits
             = new Element<int>();
        public Element<int> DataBits
        {
            get { return _DataBits; }
            set
            {
                _DataBits = value;
                PropertyChange(nameof(DataBits));
            }
        }
        private Element<StopBits> _StopBits
             = new Element<StopBits>();
        public Element<StopBits> StopBits
        {
            get { return _StopBits; }
            set
            {
                _StopBits = value;
                PropertyChange(nameof(StopBits));
            }
        }
        private Element<Parity> _Parity
             = new Element<Parity>();
        public Element<Parity> Parity
        {
            get { return _Parity; }
            set
            {
                _Parity = value;
                PropertyChange(nameof(Parity));
            }
        }
        private Element<int> _ModAddress
             = new Element<int>();
        public Element<int> ModAddress
        {
            get { return _ModAddress; }
            set
            {
                _ModAddress = value;
                PropertyChange(nameof(ModAddress));
            }
        }

        [ParamIgnore]
        public string FilePath { get; } = "PreAligner";
        [ParamIgnore]
        public string FileName { get; } = "PACommParam.Json";
        [ParamIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }

    }
}
