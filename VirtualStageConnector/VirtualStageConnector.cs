using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using ProberInterfaces.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace VirtualStageConnector
{
    public interface IVirtualStageConnector
    {
        int ChuckIndex { get; set; }
        Task<EventCodeEnum> InitModule();
        EventCodeEnum Start();
        bool Disconnect();
        void MoveAbsoluteWrapper(EnumAxisConstants axistype);
        void SetFocusingStartPos(double pos);
        void SetWaferType(EnumWaferType type);
        void SetOverdrive(double overdrive);
        void SetProbingOffset(ProbingOffset probingOffset);
        
        void SetLight(EnumProberCam camType, EnumLightType lightType, int grayLevel);
        void SetPZHome(double zum);
        Bitmap GetImage(ImageType imageType = ImageType.SNAP);
        //void SendTCPCommand(TCPCommand tcpcmd, object value = null, int xumstep = 0, int yumstep = 0, int zumstep = 0, double theta = 0, int grayLevel = 0);
        void SendTCPCommand(TCPCommand tcpcmd, object value = null, double xumstep = 0, double yumstep = 0, double zumstep = 0, double theta = 0, int grayLevel = 0);
    }

    public class DefaultVirtualStageConnector : IFactoryModule, INotifyPropertyChanged, IVirtualStageConnector
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        public int ChuckIndex { get; set; }

        public bool Disconnect()
        {
            return false;
        }

        public Bitmap GetImage(ImageType imageType = ImageType.SNAP)
        {
            return null;
        }

        public Task<EventCodeEnum> InitModule()
        {
            return Task.FromResult(EventCodeEnum.NONE);
        }

        public void MoveAbsoluteWrapper(EnumAxisConstants axistype)
        {
            return;
        }

        public void SendTCPCommand(TCPCommand tcpcmd, object value = null, int xumstep = 0, int yumstep = 0, int zumstep = 0, double theta = 0, int grayLevel = 0)
        {
            return;
        }

        public void SetFocusingStartPos(double pos)
        {
            return;
        }

        public void SetLight(EnumProberCam camType, EnumLightType lightType, int grayLevel)
        {
            return;
        }

        public void SetPZHome(double zum)
        {
            return;
        }

        public void SetWaferType(EnumWaferType type)
        {
            return;
        }

        public EventCodeEnum Start()
        {
            return EventCodeEnum.NONE;
        }

        public void SetOverdrive(double overdrive)
        {
            return;
        }

        public void SetProbingOffset(ProbingOffset probingOffset)
        {
            return;
        }

        public void SendTCPCommand(TCPCommand tcpcmd, object value = null, double xumstep = 0, double yumstep = 0, double zumstep = 0, double theta = 0, int grayLevel = 0)
        {
            return;
        }
    }

    public class EmulatedVirtualStageConnector : IFactoryModule, INotifyPropertyChanged, IVirtualStageConnector
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private static readonly object lockobj = new object();

        public bool Initialized { get; set; } = false;
        public bool IsDebug { get; set; } = false;

        TCPCommand m_lastCmd = TCPCommand.GRAB_SNAP;

        public int iGrabSizeX;
        public int iGrabSizeY;

        const int _IMAGE_WIDTH = 960;
        const int _IMAGE_HEIGHT = 960;
        const int _MAX_IMAGE_SIZE = _IMAGE_WIDTH * _IMAGE_HEIGHT * 3 + 1000;

        byte[] m_data;
        byte[] m_dataSizeFromClient;

        TcpClient m_client;
        NetworkStream m_networkStream;
        MemoryStream m_memoryStream;
        Bitmap m_bitmap;

        private string _m_serverIP = string.Empty;
        public string m_serverIP
        {
            get { return _m_serverIP; }
            set
            {
                if (value != _m_serverIP)
                {
                    _m_serverIP = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _m_serverPort;
        public int m_serverPort
        {
            get { return _m_serverPort; }
            set
            {
                if (value != _m_serverPort)
                {
                    _m_serverPort = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsVirtualProbing = false;
        public bool IsVirtualProbing
        {
            get { return _IsVirtualProbing; }
            set
            {
                if (value != _IsVirtualProbing)
                {
                    _IsVirtualProbing = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _bErrorStatus = false;
        public bool bErrorStatus
        {
            get { return _bErrorStatus; }
            set
            {
                if (value != _bErrorStatus)
                {
                    _bErrorStatus = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EnumProberCam _CurCamType = EnumProberCam.UNDEFINED;
        public EnumProberCam CurCamType
        {
            get { return _CurCamType; }
            set
            {
                if (value != _CurCamType)
                {
                    _CurCamType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _ChuckIndex;
        public int ChuckIndex
        {
            get { return _ChuckIndex; }
            set
            {
                if (value != _ChuckIndex)
                {
                    _ChuckIndex = value;
                }
            }
        }

        public async Task<EventCodeEnum> InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (!Initialized)
                {
                    Initialized = true;

                    SetDefaultServerIP();
                    SetDefaultServerPort();

                    m_data = new byte[_MAX_IMAGE_SIZE];
                    m_dataSizeFromClient = new byte[sizeof(int)];

                    await StartAsync();

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    // retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private void SetDefaultServerIP()
        {
            try
            {
                string hostName = Dns.GetHostName();
                var m_ipHostEntry = Dns.GetHostEntry(hostName);

                foreach (IPAddress address in m_ipHostEntry.AddressList)
                {
                    if (address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        m_serverIP = address.ToString();
                        break;
                    }
                }

                LoggerManager.Debug($"[{this.GetType().Name}], SetDefaultServerIP() : m_serverIP = {m_serverIP}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void SetDefaultServerPort()
        {
            try
            {
                int p = 7000;

                var stageIdx = this.ChuckIndex;

                m_serverPort = p + stageIdx;

                LoggerManager.Debug($"[{this.GetType().Name}], SetDefaultServerPort() : m_serverPort = {m_serverPort}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task<EventCodeEnum> StartAsync()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (m_client == null)
                {
                    IPEndPoint localAddress = new IPEndPoint(IPAddress.Parse(m_serverIP), m_serverPort);

                    var m_listener = new TcpListener(localAddress);
                    m_listener.Start();
                    await Task.Delay(1000); // Wait for 1 second to give the listener time to start

                    m_client = await m_listener.AcceptTcpClientAsync();

                    CurCamType = EnumProberCam.UNDEFINED;

                    ServicePointManager.Expect100Continue = false;
                    ServicePointManager.MaxServicePointIdleTime = 15000;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                    LoggerManager.Debug($"[{this.GetType().Name}], StartAsync() : Connected");

                    retval = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum Start()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (m_client == null)
                {
                    IPEndPoint localAddress = new IPEndPoint(IPAddress.Parse(m_serverIP), m_serverPort);

                    var m_listener = new TcpListener(localAddress);
                    m_listener.Start();

                    LoggerManager.Debug($"[{this.GetType().Name}], Start() : Before AcceptTcpClient");

                    m_client = m_listener.AcceptTcpClient();
                }

                CurCamType = EnumProberCam.UNDEFINED;

                ServicePointManager.Expect100Continue = false;
                ServicePointManager.MaxServicePointIdleTime = 15000;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                // 연결이 끊긴 경우
                if (m_client.Connected == false || m_networkStream?.DataAvailable == false)
                {
                    m_client.Dispose();
                    m_client = null;

                    IPEndPoint localAddress = new IPEndPoint(IPAddress.Parse(m_serverIP), m_serverPort);

                    var m_listener = new TcpListener(localAddress);
                    m_listener.Start();

                    m_client = m_listener.AcceptTcpClient();
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool Disconnect()
        {
            bool retval = false;

            try
            {
                if (m_client != null)
                {
                    m_client.Close();
                    m_client = null;
                }

                LoggerManager.Error($"[{this.GetType().Name}], Disconnect()");

                retval = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }


        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public Bitmap GetImage(ImageType imageType = ImageType.SNAP)
        {
            Bitmap retval = null;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                lock (lockobj)
                {
                    if (m_client != null && m_client.Connected)
                    {
                        m_networkStream = m_client.GetStream();

                        if (m_networkStream.CanRead)
                        {
                            TCPCommand cmd = TCPCommand.GRAB_SNAP;

                            if (imageType == ImageType.MARK)
                            {
                                cmd = TCPCommand.GRAB_MARK;
                            }
                            else if (imageType == ImageType.PA)
                            {
                                cmd = TCPCommand.GRAB_PA;
                            }
                            else if(imageType == ImageType.WAFER_EDGE)
                            {
                                cmd = TCPCommand.GRAB_WAFER_EDGE;
                            }

                            // Set a N millisecond timeout for reading.
                            m_networkStream.ReadTimeout = 10000;

                            Action ReadImageAct = () =>
                            {
                                bErrorStatus = false;

                                int m_expectedDataSize = 0;

                                if (bErrorStatus == false)
                                {
                                    int numberOfBytesRead = 0;

                                    // Check to see if this NetworkStream is readable.
                                    if (m_networkStream.CanRead)
                                    {
                                        // Incoming message may be larger than the buffer size.
                                        do
                                        {
                                            numberOfBytesRead += m_networkStream.Read(m_data, 0, m_data.Length);
                                        }
                                        while (m_networkStream.DataAvailable);
                                    }


                                    if (numberOfBytesRead == 0)
                                    {
                                        bErrorStatus = true;
                                    }

                                    if (bErrorStatus == false)
                                    {
                                        byte[] imagedata;

                                        // Get Size
                                        Array.Copy(m_data, 0, m_dataSizeFromClient, 0, m_dataSizeFromClient.Length);

                                        //m_expectedDataSize = BitConverter.ToInt32(m_dataSizeFromClient, 0) - 32;
                                        m_expectedDataSize = BitConverter.ToInt32(m_dataSizeFromClient, 0);

                                        imagedata = new byte[m_expectedDataSize];

                                        // Get Data
                                        Array.Copy(m_data, m_dataSizeFromClient.Length, imagedata, 0, m_expectedDataSize);

                                        if (m_lastCmd == TCPCommand.GRAB_SNAP ||
                                            m_lastCmd == TCPCommand.GRAB_MARK ||
                                            m_lastCmd == TCPCommand.GRAB_PA ||
                                            m_lastCmd == TCPCommand.GRAB_WAFER_EDGE)
                                        {
                                            m_memoryStream = new MemoryStream(imagedata, 0, imagedata.Length);
                                            m_bitmap = new Bitmap(m_memoryStream);

                                            retval = m_bitmap;

                                            m_memoryStream.Close();
                                        }
                                        else
                                        {
                                            LoggerManager.Error($"[{this.GetType().Name}], GetImage() : Error, m_lastCmd = {m_lastCmd}");
                                        }

                                        if (m_bitmap != null)
                                        {
                                            m_bitmap = null;
                                        }
                                    }
                                }
                            };

                            EventCodeEnum ec = WriteTCPCommand(cmd, ReadImageAct);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                sw.Stop();
            }

            return retval;
        }

        public void MoveAbsolute(double xum, double yum, double zum, double tdegree)
        {
            try
            {
                SendTCPCommand(TCPCommand.MOVEXY_ABS, xumstep: xum, yumstep: yum, zumstep: 0, theta: 0);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void MoveAbsoluteT(double tdegree)
        {
            SendTCPCommand(TCPCommand.MOVET_ABS, xumstep: 0, yumstep: 0, zumstep: 0, theta: tdegree);
        }

        public void MoveAbsoluteZ(double zum)
        {
            SendTCPCommand(TCPCommand.MOVEZ_ABS, xumstep: 0, yumstep: 0, zumstep: zum, theta: 0);
        }

        public void MoveAbsolutePZ(double zum)
        {
            // 원점 기준 이동 
            SendTCPCommand(TCPCommand.MOVEPZ_ABS, xumstep: 0, yumstep: 0, zumstep: zum, theta: 0);
        }

        public void SetZHome(double zum)
        {
            // Wafer
            SendTCPCommand(TCPCommand.MOVEZ_SETHOME, xumstep: 0, yumstep: 0, zumstep: zum, theta: 0);
        }

        public void SetPZHome(double zum)
        {
            // Mark, Pin
            SendTCPCommand(TCPCommand.MOVEPZ_SETPZHOME, xumstep: 0, yumstep: 0, zumstep: zum, theta: 0);
        }

        public void SetFocusingStartPos(double pos)
        {
            SendTCPCommand(TCPCommand.SET_FOCUSING_START_POS, xumstep: 0, yumstep: 0, zumstep: pos, theta: 0);
        }

        public void SetWaferType(EnumWaferType type)
        {
            SendTCPCommand(TCPCommand.SET_WAFER_TYPE, type);
        }

        public void SetOverdrive(double overdrive)
        {
            SendTCPCommand(TCPCommand.SET_OVERDRIVE, overdrive);
        }

        public void SetProbingOffset(ProbingOffset probingOffset)
        {
            SendTCPCommand(TCPCommand.SET_PROBING_OFFSET, probingOffset);
        }

        private EnumProberCam GetCamType(TCPCommand cmd)
        {
            EnumProberCam retval = EnumProberCam.INVALID;

            try
            {
                if (cmd == TCPCommand.WAFER_LOWMAG)
                {
                    retval = EnumProberCam.WAFER_LOW_CAM;
                }
                else if (cmd == TCPCommand.WAFER_HIGHMAG)
                {
                    retval = EnumProberCam.WAFER_HIGH_CAM;
                }
                else if (cmd == TCPCommand.PIN_LOWMAG)
                {
                    retval = EnumProberCam.PIN_LOW_CAM;
                }
                else if (cmd == TCPCommand.PIN_HIGHMAG)
                {
                    retval = EnumProberCam.PIN_HIGH_CAM;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void SetLight(EnumProberCam camType, EnumLightType lightType, int grayLevel)
        {
            try
            {
                if (CanWriteToStream())
                {
                    bool isNotSupported = false;

                    if (camType == EnumProberCam.WAFER_LOW_CAM)
                    {
                        if (lightType == EnumLightType.COAXIAL)
                        {
                            SendTCPCommand(TCPCommand.LIGHT_WAFER_LOW_COAXIAL, grayLevel: grayLevel);
                        }
                        else if (lightType == EnumLightType.OBLIQUE)
                        {
                            SendTCPCommand(TCPCommand.LIGHT_WAFER_LOW_OBLIQUE, grayLevel: grayLevel);
                        }
                        else
                        {
                            isNotSupported = true;
                        }
                    }
                    else if (camType == EnumProberCam.WAFER_HIGH_CAM)
                    {
                        if (lightType == EnumLightType.COAXIAL)
                        {
                            SendTCPCommand(TCPCommand.LIGHT_WAFER_HIGH_COAXIAL, grayLevel: grayLevel);
                        }
                        else if (lightType == EnumLightType.OBLIQUE)
                        {
                            SendTCPCommand(TCPCommand.LIGHT_WAFER_HIGH_OBLIQUE, grayLevel: grayLevel);
                        }
                        else
                        {
                            isNotSupported = true;
                        }
                    }
                    else if (camType == EnumProberCam.PIN_LOW_CAM)
                    {
                        if (lightType == EnumLightType.COAXIAL)
                        {
                            SendTCPCommand(TCPCommand.LIGHT_WAFER_LOW_COAXIAL, grayLevel: grayLevel);
                        }
                        else if (lightType == EnumLightType.OBLIQUE)
                        {
                            SendTCPCommand(TCPCommand.LIGHT_WAFER_LOW_OBLIQUE, grayLevel: grayLevel);
                        }
                        else
                        {
                            isNotSupported = true;
                        }
                    }
                    else if (camType == EnumProberCam.PIN_HIGH_CAM)
                    {
                        if (lightType == EnumLightType.COAXIAL)
                        {
                            SendTCPCommand(TCPCommand.LIGHT_PIN_HIGH_COAXIAL, grayLevel: grayLevel);
                        }
                        else if (lightType == EnumLightType.AUX)
                        {
                            SendTCPCommand(TCPCommand.LIGHT_PIN_HIGH_AUX, grayLevel: grayLevel);
                        }
                        else
                        {
                            isNotSupported = true;
                        }
                    }
                    else
                    {
                        isNotSupported = true;
                    }

                    if (isNotSupported)
                    {
                        if (IsDebug)
                        {
                            LoggerManager.Debug($"[{this.GetType().Name}], SetLight() : CurCamType = {CurCamType}. lightType = {lightType}, Not supported yet.");
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void MoveAbsoluteWrapper(EnumAxisConstants axistype)
        {
            try
            {
                CatCoordinates retval = null;

                if (CurCamType == EnumProberCam.WAFER_LOW_CAM)
                {
                    retval = this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert();
                }
                else if (CurCamType == EnumProberCam.WAFER_HIGH_CAM)
                {
                    retval = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                }
                else if (CurCamType == EnumProberCam.PIN_LOW_CAM)
                {
                    retval = this.CoordinateManager().PinLowPinConvert.CurrentPosConvert();
                }
                else if (CurCamType == EnumProberCam.PIN_HIGH_CAM)
                {
                    retval = this.CoordinateManager().PinHighPinConvert.CurrentPosConvert();
                }

                if (retval != null)
                {
                    if (axistype == EnumAxisConstants.Z)
                    {
                        MoveAbsoluteZ(retval.Z.Value);
                    }
                    else if (axistype == EnumAxisConstants.PZ)
                    {
                        var axispz = this.MotionManager().GetAxis(EnumAxisConstants.PZ);

                        MoveAbsolutePZ(axispz.Status.Position.Ref);
                    }
                    else if (axistype == EnumAxisConstants.C)
                    {
                        var axisc = this.MotionManager().GetAxis(EnumAxisConstants.C);

                        // TODO : 1/10000을 반영해야 하나?

                        if (axisc.Status.Position.Ref != 0)
                        {
                            var tdegree = axisc.Status.Position.Ref / 10000d;

                            MoveAbsoluteT(tdegree);
                        }
                        else
                        {
                            MoveAbsoluteT(axisc.Status.Position.Ref);
                        }
                    }
                    else
                    {
                        MoveAbsolute(retval.X.Value, retval.Y.Value, retval.Z.Value, retval.T.Value);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private bool CanWriteToStream()
        {
            bool retval = false;

            try
            {
                if (m_client != null && m_client.Connected)
                {
                    m_networkStream = m_client.GetStream();

                    if (m_networkStream != null && m_networkStream.CanWrite)
                    {
                        retval = true;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private EventCodeEnum WriteTCPCommand(TCPCommand command, Action action = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (CanWriteToStream())
                {
                    m_lastCmd = command;

                    byte[] strcmd = new byte[4];
                    strcmd = BitConverter.GetBytes((int)m_lastCmd);
                    m_networkStream.Write(strcmd, 0, strcmd.Length);

                    if (action != null)
                    {
                        action.Invoke();
                    }

                    retval = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (IsDebug)
                {
                    LoggerManager.Debug($"[{this.GetType().Name}], WriteTCPCommand() : {command}, END.");
                }
            }

            return retval;
        }

        public void SendTCPCommand(TCPCommand tcpcmd, object value = null, double xumstep = 0, double yumstep = 0, double zumstep = 0, double theta = 0, int grayLevel = 0)
        {
            try
            {
                if (!CanWriteToStream())
                {
                    return;
                }

                // int
                byte[] strdata4 = new byte[4];
                
                // double
                byte[] strdata8 = new byte[8];

                Action doAction = null;

                switch (tcpcmd)
                {
                    case TCPCommand.GRAB_SNAP:
                        break;
                    case TCPCommand.GRAB_LIVEON:
                        break;
                    case TCPCommand.GRAB_LIVEOFF:
                        break;
                    case TCPCommand.GRAB_MARK:
                        break;
                    case TCPCommand.MOVEXY:
                    case TCPCommand.MOVEXY_ABS:
                        doAction = () =>
                        {
                            strdata8 = BitConverter.GetBytes(xumstep);
                            m_networkStream.Write(strdata8, 0, strdata8.Length);

                            strdata8 = BitConverter.GetBytes(yumstep);
                            m_networkStream.Write(strdata8, 0, strdata8.Length);
                        };
                        break;
                    case TCPCommand.MOVEXY_ORGIN:
                        break;
                    case TCPCommand.ROTATE:

                    case TCPCommand.MOVET_ABS:
                        doAction = () =>
                        {
                            strdata8 = BitConverter.GetBytes(theta);
                            m_networkStream.Write(strdata8, 0, strdata8.Length);
                        };
                        break;
                    case TCPCommand.ROTATE_ORIGIN:
                        break;
                    case TCPCommand.MOVEXY_PINORIGIN:
                        break;
                    case TCPCommand.CALIBRATED_ORIGIN:
                        doAction = () =>
                        {
                            strdata8 = BitConverter.GetBytes(xumstep);
                            m_networkStream.Write(strdata8, 0, strdata8.Length);

                            strdata8 = BitConverter.GetBytes(yumstep);
                            m_networkStream.Write(strdata8, 0, strdata8.Length);

                            strdata8 = BitConverter.GetBytes(zumstep);
                            m_networkStream.Write(strdata8, 0, strdata8.Length);

                            strdata8 = BitConverter.GetBytes(theta);
                            m_networkStream.Write(strdata8, 0, strdata8.Length);
                        };
                        break;
                    case TCPCommand.VIRTUAL_PROBING_ON:
                        IsVirtualProbing = true;
                        break;
                    case TCPCommand.VIRTUAL_PROBING_OFF:
                        IsVirtualProbing = false;
                        break;
                    case TCPCommand.WAFER_HIGHMAG:
                    case TCPCommand.WAFER_LOWMAG:
                    case TCPCommand.PIN_HIGHMAG:
                    case TCPCommand.PIN_LOWMAG:

                        var camtype = GetCamType(tcpcmd);

                        if (CurCamType == camtype)
                        {
                            // SKIP
                            return;
                        }
                        else
                        {
                            CurCamType = camtype;
                        }

                        doAction = () =>
                        {
                            if (CurCamType == EnumProberCam.WAFER_LOW_CAM || CurCamType == EnumProberCam.WAFER_HIGH_CAM)
                            {
                                WriteTCPCommand(TCPCommand.MOVEZ_SETHOME);

                                double ActualThickness = this.StageSupervisor().WaferObject.GetSubsInfo().ActualThickness;
                                
                                strdata8 = BitConverter.GetBytes(ActualThickness);
                                m_networkStream.Write(strdata8, 0, strdata8.Length);
                            }
                            else if (CurCamType == EnumProberCam.PIN_LOW_CAM || CurCamType == EnumProberCam.PIN_HIGH_CAM)
                            {
                                WriteTCPCommand(TCPCommand.MOVEPZ_SETPZHOME);

                                double PinHeight = this.StageSupervisor().ProbeCardInfo.CalcLowestPin();

                                PinCoordinate pincoord = new PinCoordinate();
                                MachineCoordinate machine = new MachineCoordinate();

                                pincoord.X.Value = 0;
                                pincoord.Y.Value = 0;
                                pincoord.Z.Value = PinHeight;

                                if (CurCamType == EnumProberCam.PIN_LOW_CAM)
                                {
                                    machine = this.CoordinateManager().PinLowPinConvert.ConvertBack(pincoord);
                                }
                                else if (CurCamType == EnumProberCam.PIN_HIGH_CAM)
                                {
                                    machine = this.CoordinateManager().PinHighPinConvert.ConvertBack(pincoord);
                                }

                                strdata8 = BitConverter.GetBytes(machine.Z.Value);
                                m_networkStream.Write(strdata8, 0, strdata8.Length);
                            }

                            MoveAbsoluteWrapper(EnumAxisConstants.Undefined);
                        };
                        break;
                    case TCPCommand.MOVEZ_SETHOME:
                        doAction = () =>
                        {
                            strdata8 = BitConverter.GetBytes(zumstep);
                            m_networkStream.Write(strdata8, 0, strdata8.Length);
                        };
                        break;
                    case TCPCommand.MOVEPZ_SETPZHOME:
                        doAction = () =>
                        {
                            strdata8 = BitConverter.GetBytes(zumstep);
                            m_networkStream.Write(strdata8, 0, strdata8.Length);
                        };
                        break;
                    case TCPCommand.MOVEZ:
                    case TCPCommand.MOVEZ_ABS:
                    case TCPCommand.MOVEPZ:
                    case TCPCommand.MOVEPZ_ABS:
                        doAction = () =>
                        {
                            strdata8 = BitConverter.GetBytes(zumstep);
                            m_networkStream.Write(strdata8, 0, strdata8.Length);
                        };
                        break;
                    case TCPCommand.MOVEZ_ORIGIN:
                        break;
                    case TCPCommand.LIGHT_WAFER_LOW_COAXIAL:
                    case TCPCommand.LIGHT_WAFER_LOW_OBLIQUE:
                    case TCPCommand.LIGHT_WAFER_HIGH_COAXIAL:
                    case TCPCommand.LIGHT_WAFER_HIGH_OBLIQUE:
                    case TCPCommand.LIGHT_PIN_LOW_COAXIAL:
                    case TCPCommand.LIGHT_PIN_LOW_OBLIQUE:
                    case TCPCommand.LIGHT_PIN_HIGH_COAXIAL:
                    case TCPCommand.LIGHT_PIN_HIGH_AUX:
                        doAction = () =>
                        {
                            strdata4 = BitConverter.GetBytes(grayLevel);
                            m_networkStream.Write(strdata4, 0, strdata4.Length);
                        };
                        break;
                    case TCPCommand.SET_FOCUSING_START_POS:
                        doAction = () =>
                        {
                            strdata8 = BitConverter.GetBytes(zumstep);
                            m_networkStream.Write(strdata8, 0, strdata8.Length);
                        };
                        break;
                    case TCPCommand.SET_WAFER_TYPE:

                        if (value is EnumWaferType waferType)
                        {
                            doAction = () =>
                            {
                                byte[] data = BitConverter.GetBytes((int)waferType);
                                m_networkStream.Write(data, 0, data.Length);
                            };
                        }
                        else
                        {
                            // Invalid value passed for SET_WAFER_TYPE, you can handle the error here.
                            LoggerManager.Error($"Invalid value for SET_WAFER_TYPE command: {value}");
                        }
                        break;
                    case TCPCommand.SET_OVERDRIVE:

                        if (value is double overdrive)
                        {
                            doAction = () =>
                            {
                                strdata8 = BitConverter.GetBytes(overdrive);
                                m_networkStream.Write(strdata8, 0, strdata8.Length);
                            };
                        }
                        break;
                    case TCPCommand.SET_PROBING_OFFSET:
                        doAction = () =>
                        {
                            if (value != null && value is ProbingOffset probingOffset)
                            {
                                string jsonData = JsonConvert.SerializeObject(probingOffset);
                                byte[] objectData = Encoding.UTF8.GetBytes(jsonData);
                                byte[] objectSize = BitConverter.GetBytes(objectData.Length);

                                // Write the size of the serialized object
                                m_networkStream.Write(objectSize, 0, objectSize.Length);

                                // Write the actual serialized object
                                m_networkStream.Write(objectData, 0, objectData.Length);
                            }
                        };
                        break;
                    default:
                        break;
                }

                // PIN CAM이라면
                if (CurCamType == EnumProberCam.PIN_LOW_CAM || CurCamType == EnumProberCam.PIN_HIGH_CAM)
                {
                    xumstep = -xumstep;
                    yumstep = -yumstep;
                }

                // WAFER CAM
                if (CurCamType == EnumProberCam.WAFER_LOW_CAM || CurCamType == EnumProberCam.WAFER_HIGH_CAM)
                {
                    yumstep = -yumstep;
                }

                EventCodeEnum retval = EventCodeEnum.UNDEFINED;

                lock (lockobj)
                {
                    retval = WriteTCPCommand(tcpcmd, doAction);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class VirtualStageConnector
    {
        private static readonly object padlock = new object();
        private static IVirtualStageConnector instance = null;

        public static IVirtualStageConnector Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                        {
                            instance = new EmulatedVirtualStageConnector();
                        }
                        else
                        {
                            instance = new DefaultVirtualStageConnector();
                        }
                    }

                    return instance;
                }
            }
        }
    }
}
