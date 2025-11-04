using LogModule;
using ProberInterfaces;
using ProberInterfaces.Communication.Scenario;
using ProberInterfaces.Enum;
using RelayCommandBase;
using RequestCore.Query.TCPIP;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using TesterDriverModule.TCPIP;

namespace ServerEmulator.ViewModel
{
    public enum EnumCommandCollectionType
    {
        Normal,
        SEND,
        RECEIVE,
    }

    public class VMServerEmulator : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private SolidColorBrush InterruputPortBrush { get; set; }
        private SolidColorBrush CommandPortBrush { get; set; }

        private SolidColorBrush DefaultBrush { get; set; }

        //public ClientClass server_data;
        //public static ManualResetEvent allDone = new ManualResetEvent(false);
        //public static ManualResetEvent sendDone = new ManualResetEvent(false);
        //public static ManualResetEvent receiveDone = new ManualResetEvent(false);
        Thread Listenthread;
        //Socket ClientSocket = null;

        //Socket receiveSocket = null;
        //Socket sendSocket = null;

        MyTcpSocket receiveSocket = null;
        MyTcpSocket sendSocket = null;

        //public int userNo = 0;
        //private readonly static IPAddress iPAddress = IPAddress.Parse("127.0.0.1");
        //public int responseCnt = 1;

        public List<string> ReplaceDataForActionResponse = new List<string>(
        new string[] { "ACK", "NACK", "OK", "TimingError", "[00]", "[01]", "[02]" });

        private string Terminator = "\r\n";

        //public StateObject state;

        BinDataMaker.BinDataMaker BinDataMaker = new BinDataMaker.BinDataMaker();

        private int InterruptPortHashCode { get; set; }
        private int CommandPortHashCode { get; set; }

        private int _DutCount;
        public int DutCount
        {
            get { return _DutCount; }
            set
            {
                if (value != _DutCount)
                {
                    _DutCount = value;
                    NotifyPropertyChanged(nameof(DutCount));
                }
            }
        }

        private BinType _BinType;
        public BinType BinType
        {
            get { return _BinType; }
            set
            {
                if (value != _BinType)
                {
                    _BinType = value;
                    NotifyPropertyChanged(nameof(BinType));
                }
            }
        }

        private RelayCommand<object> _SendToInterruptPortCommand;
        public ICommand SendToInterruptPortCommand
        {
            get
            {
                if (null == _SendToInterruptPortCommand)
                    _SendToInterruptPortCommand = new RelayCommand<object>(SendToInterruptPortCommandFunc);
                return _SendToInterruptPortCommand;
            }
        }

        private void SendToInterruptPortCommandFunc(object obj)
        {
            try
            {
                if (receiveSocket._socket != null)
                {
                    Send(receiveSocket._socket, SendMsg);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _SendToCommandPortCommand;
        public ICommand SendToCommandPortCommand
        {
            get
            {
                if (null == _SendToCommandPortCommand)
                    _SendToCommandPortCommand = new RelayCommand<object>(SendToCommandPortCommandFunc);
                return _SendToCommandPortCommand;
            }
        }

        private void SendToCommandPortCommandFunc(object obj)
        {
            try
            {
                if (sendSocket != null)
                {
                    if (sendSocket._socket != null)
                    {
                        this.CurrentQueryData = new CommandAndReceived();
                        CurrentQueryData.Received = false;

                        Send(sendSocket._socket, SendMsg);
                    }
                }
                else
                {
                    LoggerManager.Debug($"[VMServerEmulator] sendSocket is null.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void CommandCollectionAdd(string message, SolidColorBrush color = null)
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    CollectionComponent collectionComponent = new CollectionComponent();

                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);

                    collectionComponent.Message = timestamp + " " + message;

                    if (color == null)
                    {
                        collectionComponent.Color = DefaultBrush;
                    }
                    else
                    {
                        collectionComponent.Color = color;
                    }

                    CommandCollection.Add(collectionComponent);
                }));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void Send(Socket handler, string data)
        {
            try
            {
                string dataWithTerminator = data;

                if (Terminator != null && Terminator != string.Empty)
                {
                    dataWithTerminator = data + Terminator;
                }

                byte[] sendByte = Encoding.ASCII.GetBytes(dataWithTerminator);

                #region CollectionADD

                SolidColorBrush brush = null;
                int portHashCode = handler.GetHashCode();
                string porttype = string.Empty;

                if (portHashCode == InterruptPortHashCode)
                {
                    brush = InterruputPortBrush;
                    porttype = "INTERRUPT";
                }
                else if (portHashCode == CommandPortHashCode)
                {
                    brush = CommandPortBrush;
                    porttype = "COMMAND";
                }
                else
                {
                    brush = null;
                }

                string message = $"[{porttype}] SEND : " + dataWithTerminator;

                CommandCollectionAdd(message, brush);

                #endregion  

                handler.BeginSend(sendByte, 0, sendByte.Length, 0, new AsyncCallback(SendCallback), handler);
            }
            catch (SocketException err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        Thread UpdateThread;
        private bool isStopThreadReq = false;


        private void MonitoringThread()
        {
            try
            {
                while (isStopThreadReq == false)
                {
                    Thread.Sleep(100);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        // Cell#1 : 
        //  SendPort : 7109
        //  ReceivePort : 7009
        //

        // Cell#2: 
        //  SendPort : 7110
        //  ReceivePort : 7010
        //

        private void Init()
        {
            receiveSocket = new MyTcpSocket(IP, ReceivePortNo);

            QuerySplitstr = '=';
            QueryCheckstr = "=";

            BinType = BinType.BIN_PASSFAIL;

            InterruputPortBrush = new SolidColorBrush(Colors.Purple);
            CommandPortBrush = new SolidColorBrush(Colors.Blue);
            DefaultBrush = new SolidColorBrush(Colors.Black);

            UpdateThread = new Thread(new ThreadStart(MonitoringThread));
            UpdateThread.Name = this.GetType().Name;
            UpdateThread.Start();

            CommandCollection.Clear();

            Scenario = new TCPIPTesterScenario();
            Scenario.InitModule();

            ConnectExecuteCommandFunc(null);
        }

        public VMServerEmulator()
        {
        }

        private ObservableCollection<CollectionComponent> _CommandCollection = new ObservableCollection<CollectionComponent>();
        public ObservableCollection<CollectionComponent> CommandCollection
        {
            get { return _CommandCollection; }
            set
            {
                if (value != _CommandCollection)
                {
                    _CommandCollection = value;
                    NotifyPropertyChanged(nameof(CommandCollection));
                }
            }
        }

        private string _SendMsg;
        public string SendMsg
        {
            get { return _SendMsg; }
            set
            {
                if (value != _SendMsg)
                {
                    _SendMsg = value;
                    NotifyPropertyChanged(nameof(SendMsg));
                }
            }
        }

        private string _ReceiveMsg;
        public string ReceiveMsg
        {
            get { return _ReceiveMsg; }
            set
            {
                if (value != _ReceiveMsg)
                {
                    _ReceiveMsg = value;
                    NotifyPropertyChanged(nameof(ReceiveMsg));
                }
            }
        }

        private bool _ExistSendPort;
        public bool ExistSendPort
        {
            get { return _ExistSendPort; }
            set
            {
                if (value != _ExistSendPort)
                {
                    _ExistSendPort = value;
                    NotifyPropertyChanged(nameof(ExistSendPort));
                }
            }
        }

        private string _ClientConnect = "Red";
        public string ClientConnect
        {
            get { return _ClientConnect; }
            set
            {
                if (value != _ClientConnect)
                {
                    _ClientConnect = value;
                    NotifyPropertyChanged(nameof(ClientConnect));
                }
            }
        }
        private int _SendPortNo = 7009;
        public int SendPortNo
        {
            get { return _SendPortNo; }
            set
            {
                if (value != _SendPortNo)
                {
                    _SendPortNo = value;
                    NotifyPropertyChanged(nameof(SendPortNo));
                }
            }
        }

        private int _ReceivePortNo = 7109;
        public int ReceivePortNo
        {
            get { return _ReceivePortNo; }
            set
            {
                if (value != _ReceivePortNo)
                {
                    _ReceivePortNo = value;
                    NotifyPropertyChanged(nameof(ReceivePortNo));
                }
            }
        }

        private string _IP = "127.0.0.1";
        public string IP
        {
            get { return _IP; }
            set
            {
                if (value != _IP)
                {
                    _IP = value;
                    NotifyPropertyChanged(nameof(IP));
                }
            }
        }

        //private IPAddress _iPAddress;
        //public IPAddress iPAddress
        //{
        //    get { return _iPAddress; }
        //    set
        //    {
        //        if (value != _iPAddress)
        //        {
        //            _iPAddress = value;
        //            NotifyPropertyChanged(nameof(iPAddress));
        //        }
        //    }
        //}

        private bool _ConnectFlag = true;
        public bool ConnectFlag
        {
            get { return _ConnectFlag; }
            set
            {
                if (value != _ConnectFlag)
                {
                    _ConnectFlag = value;
                    NotifyPropertyChanged(nameof(ConnectFlag));
                }
            }
        }

        private char QuerySplitstr;
        private string QueryCheckstr = string.Empty;

        private TCPIPTesterScenario Scenario;

        //public List<string> TestString = new List<string>();

        public string InterruptResponse = string.Empty;
        public List<CommandAndReceived> Querylist = new List<CommandAndReceived>();
        public CommandAndReceived CurrentQueryData = null;

        private String lastReceiveQueryMsg = string.Empty;

        public void SendCommandFunc(Socket socket)
        {
            try
            {
                string str = null;
                byte[] dataWrite = null;

                //Socket handler = server_data.get_sock();
                //sendSocket = handler;
                //StateObject state = new StateObject();
                //state.workSocket = socket;

                //StateObject state1 = new StateObject();
                //state1.workSocket = socket;

                //SendCallbackParam sendCallbackParam = new SendCallbackParam();

                //sendCallbackParam.stateObject = state;
                //sendCallbackParam.ControlFlag = true;

                if (InterruptResponse != string.Empty)
                {
                    str = InterruptResponse;
                    InterruptResponse = string.Empty;

                    str = str + Terminator;

                    dataWrite = Encoding.ASCII.GetBytes(str);

                    //sendCallbackParam.Command = null;

                    #region CollectionADD

                    SolidColorBrush brush = null;

                    //int portHashCode = state.workSocket.GetHashCode();
                    int portHashCode = socket.GetHashCode();

                    string porttype = string.Empty;

                    if (portHashCode == InterruptPortHashCode)
                    {
                        brush = InterruputPortBrush;
                        porttype = "INTERRUPT";
                    }
                    else if (portHashCode == CommandPortHashCode)
                    {
                        brush = CommandPortBrush;
                        porttype = "COMMAND";
                    }
                    else
                    {
                        brush = null;
                    }

                    string message = $"[{porttype}] SEND : " + str;

                    CommandCollectionAdd(message, brush);

                    #endregion

                    //sendDone.Reset();
                    //clientsocket.BeginSend(dataWrite, 0, dataWrite.Length, 0, new AsyncCallback(SendCallback), sendCallbackParam);
                    //state.workSocket.BeginSend(dataWrite, 0, dataWrite.Length, 0, new AsyncCallback(SendCallback), state.workSocket);
                    //sendDone.WaitOne();

                    socket.BeginSend(dataWrite, 0, dataWrite.Length, 0, new AsyncCallback(SendCallback), socket);
                }
                else
                {
                    //ReadCallbackParam reaparam = new ReadCallbackParam();
                    //reaparam.stateObject = state;
                    //reaparam.Command = null;

                    if (Querylist.Count == 0)
                    {
                        //receiveDone.Reset();
                        //handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), reaparam);
                        //state.workSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                        //receiveDone.WaitOne();

                        MyTcpSocket MyTcpSocket = new MyTcpSocket();
                        MyTcpSocket._socket = socket;

                        socket.BeginReceive(MyTcpSocket.buffer, 0, MyTcpSocket.GetBufferSize(), 0, new AsyncCallback(ReadCallback), MyTcpSocket);
                    }
                    else
                    {
                        if (Querylist.Count > 0)
                        {
                            if (Querylist[0].Sended == false)
                            {
                                CurrentQueryData = Querylist[0];

                                str = CurrentQueryData.Command;
                                CurrentQueryData.Sended = true;

                                str = str + Terminator;

                                dataWrite = Encoding.ASCII.GetBytes(str);

                                //sendCallbackParam.Command = Querylist[0];

                                #region CollectionAdd

                                SolidColorBrush brush = null;

                                //int portHashCode = state.workSocket.GetHashCode();
                                int portHashCode = socket.GetHashCode();

                                string porttype = string.Empty;

                                if (portHashCode == InterruptPortHashCode)
                                {
                                    brush = InterruputPortBrush;
                                    porttype = "INTERRUPT";
                                }
                                else if (portHashCode == CommandPortHashCode)
                                {
                                    brush = CommandPortBrush;
                                    porttype = "COMMAND";
                                }
                                else
                                {
                                    brush = null;
                                }

                                string message = $"[{porttype}] SEND : " + str;

                                CommandCollectionAdd(message, brush);

                                #endregion

                                //sendDone.Reset();
                                //state.workSocket.BeginSend(dataWrite, 0, dataWrite.Length, 0, new AsyncCallback(SendCallback), state.workSocket);
                                //sendDone.WaitOne();

                                socket.BeginSend(dataWrite, 0, dataWrite.Length, 0, new AsyncCallback(SendCallback), socket);
                            }
                        }
                    }
                }
            }
            catch (SocketException err)
            {
                if (receiveSocket != null)
                {
                    receiveSocket.Close();
                }
                else
                {
                    LoggerManager.Debug($"receiveSocket is null.");
                }

                if (sendSocket != null)
                {
                    sendSocket.Close();
                }
                else
                {
                    LoggerManager.Debug($"sendSocket is null.");
                }

                LoggerManager.Error($"Socekt Error Code : {err.SocketErrorCode}");
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                //LoggerManager.Exception(err);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                //Socket handler = (Socket)ar.AsyncState;
                //handler = receiveSocket;
                //SendCallbackParam Sendparam = (SendCallbackParam)ar.AsyncState;
                //Socket handler = Sendparam.stateObject.workSocket;

                // Complete sending the data to the remote device.  
                //int bytesSent = handler.EndSend(ar);

                //sendDone.Set();

                // Query가 쌓여 있을 때와 아닐 때를 구분하고 싶다.
                // Query가 쌓여 있지 않다 => 프로버로부터 다음 커맨드를 받아야 함.
                // Query가 쌓여 있다. => 현재 커맨드 응답 후, 내가 가진 쿼리에서 꺼내서 전달을 해야 된다.

                //StateObject state = new StateObject();
                //state.workSocket = receiveSocket;

                //state.workSocket = (Socket)ar.AsyncState;


                //ReadCallbackParam reaparam = new ReadCallbackParam();
                //reaparam.stateObject = state;
                //reaparam.Command = null;

                MyTcpSocket MyTcpSocket = new MyTcpSocket();
                Socket socket = (Socket)ar.AsyncState;
                MyTcpSocket._socket = socket;

                if (CurrentQueryData != null && CurrentQueryData.Received == false)
                {
                    //reaparam.Command = Sendparam.Command;

                    //receiveDone.Reset();
                    //receiveSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);

                    //state.workSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);

                    //receiveDone.WaitOne();

                    socket.BeginReceive(MyTcpSocket.buffer, 0, MyTcpSocket.GetBufferSize(), 0, new AsyncCallback(ReadCallback), MyTcpSocket);
                }
                else
                {
                    if (Querylist.Count > 0)
                    {
                        // 프로버로부터 받을게 존재하지 않음.

                        Thread.Sleep(1000);
                        SendCommandFunc(sendSocket._socket);
                    }

                    //state.workSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                    socket.BeginReceive(MyTcpSocket.buffer, 0, MyTcpSocket.GetBufferSize(), 0, new AsyncCallback(ReadCallback), MyTcpSocket);

                    //if (Querylist.Count > 0)
                    //{
                    //    // 프로버로부터 받을게 존재하지 않음.

                    //    Thread.Sleep(1000);
                    //    SendCommandFunc(sendSocket);
                    //}
                    //else
                    //{ 
                    //    //receiveDone.Reset();
                    //    //receiveSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                    //    state.workSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                    //    //receiveDone.WaitOne();
                    //}
                }
            }
            catch (SocketException err)
            {
                if (receiveSocket != null)
                {
                    receiveSocket.Close();
                }
                else
                {
                    LoggerManager.Debug($"receiveSocket is null.");
                }

                if (sendSocket != null)
                {
                    sendSocket.Close();
                }
                else
                {
                    LoggerManager.Debug($"sendSocket is null.");
                }

                LoggerManager.Error($"Socekt Error Code : {err.SocketErrorCode}");
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                //LoggerManager.Exception(err);
            }
        }

        private string RemoveStr(string recv_data, params string[] args)
        {
            string retStr = recv_data;
            try
            {
                if (args != null)
                {
                    foreach (var arg in args)
                    {
                        retStr = retStr.Replace(arg, "");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retStr;
        }

        public void ReadCallback(IAsyncResult ar)
        {
            try
            {
                //ReadCallbackParam readparam = (ReadCallbackParam)ar.AsyncState;

                // Retrieve the state object and the handler socket  
                // from the asynchronous state object.  
                //StateObject state = (StateObject)ar.AsyncState;
                //Socket handler = state.workSocket;
                //bool chkCommand = false;

                //StateObject state = readparam.stateObject;
                //Socket handler = state.workSocket;

                MyTcpSocket MyTcpSocket = (MyTcpSocket)ar.AsyncState;

                // Read data from the client socket.
                //int bytesRead = handler.EndReceive(ar);
                int bytesRead = MyTcpSocket._socket.EndReceive(ar);

                //receiveDone.Set();

                if (bytesRead > 0)
                {
                    //ReceiveMsg = Encoding.ASCII.GetString(state.buffer, 0, bytesRead);
                    ReceiveMsg = Encoding.ASCII.GetString(MyTcpSocket.buffer, 0, bytesRead);

                    ReceiveMsg = RemoveStr(ReceiveMsg, "\r", "\n");

                    #region ColloectionAdd

                    SolidColorBrush brush = null;

                    //int portHashCode = handler.GetHashCode();
                    int portHashCode = MyTcpSocket._socket.GetHashCode();

                    string porttype = string.Empty;

                    if (portHashCode == InterruptPortHashCode)
                    {
                        brush = InterruputPortBrush;
                        porttype = "INTERRUPT";
                    }
                    else if (portHashCode == CommandPortHashCode)
                    {
                        brush = CommandPortBrush;
                        porttype = "COMMAND";
                    }
                    else
                    {
                        brush = null;
                    }

                    string message = $"[{porttype}] RECEIVE : " + ReceiveMsg;

                    CommandCollectionAdd(message, brush);

                    #endregion

                    //LoggerManager.Debug($"[VMServerEmulator] Received = {ReceiveMsg}");

                    if (ReceiveMsg == "ProberStart")
                    {
                        if (ExistSendPort == false)
                        {
                            //IPEndPoint SendEndPoint = new IPEndPoint(iPAddress, SendPortNo);

                            //sendSocket._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            //sendSocket._socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
                            //sendSocket._socket.Connect(SendEndPoint);

                            sendSocket.Connect(true);

                            CommandPortHashCode = sendSocket._socket.GetHashCode();

                            ExistSendPort = true;
                        }
                    }

                    if (CurrentQueryData != null)
                    {
                        if (CurrentQueryData.Received == false)
                        {
                            CurrentQueryData.Received = true;
                            Querylist.Remove(CurrentQueryData);
                            CurrentQueryData = null;
                        }
                    }

                    string[] querysplit = null;

                    querysplit = ReceiveMsg.Split(QuerySplitstr);

                    string QueryCommandName = string.Empty;

                    //if (ReceiveMsg.Contains("?") || ReceiveMsg.Contains("="))
                    if (ReceiveMsg.Contains(QueryCheckstr))
                    {

                        QueryCommandName = querysplit[0];
                    }
                    else
                    {
                        QueryCommandName = ReceiveMsg;

                        foreach (var replace in ReplaceDataForActionResponse)
                        {
                            QueryCommandName = QueryCommandName.Replace(replace, "");
                        }

                        QueryCommandName = QueryCommandName.Trim();
                    }

                    lastReceiveQueryMsg = ReceiveMsg;

                    EnumTCPIPCommandType cmdType = EnumTCPIPCommandType.INTERRUPT;

                    // 메시지를 받은 후....
                    if (ReceiveMsg.Contains(QueryCheckstr) == true)
                    {
                        VerifyCommandOrder(QueryCommandName, EnumTCPIPCommandType.QUERY);
                    }
                    else
                    {
                        if (ReceiveMsg.Contains("[00]") == true)
                        {
                            VerifyCommandOrder(QueryCommandName, EnumTCPIPCommandType.ACTION);
                            cmdType = EnumTCPIPCommandType.ACTION;
                        }
                        else
                        {
                            VerifyCommandOrder(QueryCommandName, EnumTCPIPCommandType.INTERRUPT);
                        }
                    }

                    if (cmdType == EnumTCPIPCommandType.INTERRUPT)
                    {
                        InterruptResponse = "ACK " + QueryCommandName;
                    }

                    //SendCommandFunc(handler);
                    SendCommandFunc(MyTcpSocket._socket);
                }
                else
                {
                    DisconnectExecuteCommandFunc(null);
                    Listen();
                }
            }
            catch (SocketException err)
            {
                //if (receiveSocket != null)
                //{
                //    receiveSocket.Close();
                //}
                //else
                //{
                //    LoggerManager.Debug($"receiveSocket is null.");
                //}

                //if (sendSocket != null)
                //{
                //    sendSocket.Close();
                //}
                //else
                //{
                //    LoggerManager.Debug($"sendSocket is null.");
                //}
                DisconnectExecuteCommandFunc(null);
                Listen();
                LoggerManager.Error($"Socekt Error Code : {err.SocketErrorCode}");
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                //LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _MakeBinDataCommand;
        public ICommand MakeBinDataCommand
        {
            get
            {
                if (null == _MakeBinDataCommand)
                    _MakeBinDataCommand = new RelayCommand<object>(MakeBinDataCommandFunc);
                return _MakeBinDataCommand;
            }
        }

        private void MakeBinDataCommandFunc(object obj)
        {
            try
            {
                //BinDataMaker.DemoModeOnOff(true);
                BinDataMaker.SetDutCount(this.DutCount);
                BinDataMaker.SetBinType(this.BinType);

                BinDataMaker.SetPrefix("BN");

                SendMsg = BinDataMaker.MakeBinInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private RelayCommand<object> _ClearCommand;
        public ICommand ClearCommand
        {
            get
            {
                if (null == _ClearCommand)
                    _ClearCommand = new RelayCommand<object>(ClearCommandFunc);
                return _ClearCommand;
            }
        }

        public void ClearCommandFunc(object obj)
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    CommandCollection.Clear();
                }));
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                //LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _DisconnectExecuteCommand;
        public ICommand DisconnectExecuteCommand
        {
            get
            {
                if (null == _DisconnectExecuteCommand)
                    _DisconnectExecuteCommand = new RelayCommand<object>(DisconnectExecuteCommandFunc);
                return _DisconnectExecuteCommand;
            }
        }

        public void DisconnectExecuteCommandFunc(object obj)
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    CommandCollectionAdd("[Server Listen Stop]");
                    ClientConnect = "Red";
                    ConnectFlag = true;
                }));

                if (sendSocket != null)
                {
                    sendSocket.Close();

                    //sendSocket.Disconnect(false);

                    //sendSocket.Shutdown(SocketShutdown.Both);
                    //sendSocket.Close();

                    //sendSocket.Dispose();
                    //sendSocket = null;
                }

                if (receiveSocket != null)
                {
                    receiveSocket.Close();

                    //receiveSocket.Disconnect(false);

                    //receiveSocket.Shutdown(SocketShutdown.Both);
                    //receiveSocket.Close();

                    //receiveSocket.Dispose();
                    //receiveSocket = null;
                }

                CalledBeginAcceptFlag = false;
                //CurrentOrderCount = 0;
                CurrentQueryData = null;
                Querylist.Clear();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                //LoggerManager.Exception(err);
            }
            finally
            {

            }
        }

        private RelayCommand<object> _InitExecuteCommand;
        public ICommand InitExecuteCommand
        {
            get
            {
                if (null == _InitExecuteCommand)
                    _InitExecuteCommand = new RelayCommand<object>(InitExecuteCommandFunc);
                return _InitExecuteCommand;
            }
        }

        public void InitExecuteCommandFunc(object obj)
        {
            try
            {
                Init();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                //LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _ConnectExecuteCommand;
        public ICommand ConnectExecuteCommand
        {
            get
            {
                if (null == _ConnectExecuteCommand)
                    _ConnectExecuteCommand = new RelayCommand<object>(ConnectExecuteCommandFunc);
                return _ConnectExecuteCommand;
            }
        }

        public void ConnectExecuteCommandFunc(object obj)
        {
            try
            {
                Listenthread = new Thread(new ThreadStart(Listen));
                Listenthread.Name = this.GetType().Name;
                Listenthread.Start();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                //LoggerManager.Exception(err);
            }
        }

        public bool CalledBeginAcceptFlag = false;

        private void Listen()
        {
            //iPAddress = IPAddress.Parse(IP);
            //IPEndPoint SendEndPoint = new IPEndPoint(iPAddress, SendPortNo);
            IPEndPoint ReceiveEndPoint = new IPEndPoint(IPAddress.Any, ReceivePortNo);

            //sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            receiveSocket._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //sendSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            receiveSocket._socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);

            try
            {
                //sendSocket.Bind(SendEndPoint);
                //sendSocket.Listen(100);

                receiveSocket._socket.Bind(ReceiveEndPoint);
                receiveSocket._socket.Listen(100);

                System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    CommandCollectionAdd("[Server Listen Start]");
                }));


                while (true)
                {
                    //if (receiveSocket.Connected)
                    //{
                    //    break;
                    //}

                    if (receiveSocket.IsConnected)
                    {
                        break;
                    }

                    if (CalledBeginAcceptFlag == false)
                    {
                        receiveSocket._socket.BeginAccept(new AsyncCallback(AcceptCallback), receiveSocket);
                        CalledBeginAcceptFlag = true;
                    }

                    //allDone.Reset();
                    //receiveSocket.BeginAccept(new AsyncCallback(AcceptCallback), receiveSocket);
                    //sendSocket.BeginAccept(new AsyncCallback(AcceptCallback2), sendSocket);
                    //allDone.WaitOne();

                    Thread.Sleep(100);
                }
            }
            catch (SocketException err)
            {
                if (receiveSocket != null)
                {
                    receiveSocket.Close();
                }
                else
                {
                    LoggerManager.Debug($"receiveSocket is null.");
                }

                //sendSocket.Close();
                LoggerManager.Error($"Socekt Error Code : {err.SocketErrorCode}");
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                //LoggerManager.Exception(err);
            }
        }

        //public void AcceptCallback2(IAsyncResult ar)
        //{
        //    try
        //    {
        //        sendSocket = (Socket)ar.AsyncState;
        //        Socket handler = sendSocket.EndAccept(ar);
        //        sendSocket = handler;
        //    }
        //    catch (Exception)
        //    {
        //        if (receiveSocket != null)
        //        {
        //            receiveSocket.Close();
        //        }
        //        else
        //        {
        //            LoggerManager.Debug($"receiveSocket is null.");
        //        }

        //        if (sendSocket != null)
        //        {
        //            sendSocket.Close();
        //        }
        //        else
        //        {
        //            LoggerManager.Debug($"sendSocket is null.");
        //        }
        //    }
        //}

        public void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                // Signal the main thread to continue.  
                //allDone.Set();

                // Get the socket that handles the client request.  
                receiveSocket = (MyTcpSocket)ar.AsyncState;

                if (receiveSocket._socket != null)
                {
                    Socket handler = receiveSocket._socket.EndAccept(ar);
                    receiveSocket.IsConnected = true;

                    receiveSocket._socket = handler;

                    IPEndPoint remoteipendpoint = receiveSocket._socket.RemoteEndPoint as IPEndPoint;

                    sendSocket = new MyTcpSocket(remoteipendpoint.Address.ToString(), SendPortNo);

                    InterruptPortHashCode = handler.GetHashCode();

                    //server_data = new ClientClass(handler);

                    System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        CommandCollectionAdd("Client connect!!!");
                        ClientConnect = "Green";
                        ConnectFlag = false;
                    }));

                    // Create the state object.
                    //StateObject state = new StateObject();
                    //state.workSocket = handler;

                    //ReadCallbackParam readparam = new ReadCallbackParam();
                    //readparam.stateObject = state;
                    //readparam.Command = null;

                    //receiveDone.Reset();
                    //handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                    //receiveDone.WaitOne();

                    receiveSocket._socket.BeginReceive(receiveSocket.buffer, 0, receiveSocket.GetBufferSize(), 0, new AsyncCallback(ReadCallback), receiveSocket);
                }

            }
            catch (SocketException err)
            {
                if (receiveSocket != null)
                {
                    receiveSocket.Close();
                }
                else
                {
                    LoggerManager.Debug($"receiveSocket is null.");
                }

                if (sendSocket != null)
                {
                    sendSocket.Close();
                }
                else
                {
                    LoggerManager.Debug($"sendSocket is null.");
                }

                LoggerManager.Error($"Socekt Error Code : {err.SocketErrorCode}");
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                //LoggerManager.Exception(err);
            }
        }

        private string SplitCommandName(string input)
        {
            string retval = string.Empty;

            try
            {
                if (input.Contains("WaferStart") == true)
                {
                    retval = "WaferStart";
                }
                else if (input.Contains("ChipStart") == true)
                {
                    retval = "ChipStart";
                }
                else if (input.Contains("LotEnd") == true)
                {
                    retval = "LotEnd";
                }
                else if (input.Contains("LotStart") == true)
                {
                    retval = "LotStart";
                }
                else if (input.Contains("MoveEnd") == true)
                {
                    retval = "MoveEnd";
                }
                else
                {
                    retval = input;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        private void VerifyCommandOrder(string InputCommand, EnumTCPIPCommandType type)
        {
            try
            {
                string ContainText = SplitCommandName(InputCommand);

                if (InputCommand != null && InputCommand != string.Empty)
                {
                    //if (MultiOrder.Count > 0)
                    //{
                    //    if (ContainText == "ChipStart" || ContainText == "WaferStart")
                    //    {
                    //        CurrentOrderCount = MultiOrder[0];
                    //        MultiOrder.Clear();
                    //    }
                    //}

                    //if (Scenario.CommandsOrder[CurrentOrderCount].multiOrderList != null)
                    //{
                    //    foreach (var count in Scenario.CommandsOrder[CurrentOrderCount].multiOrderList)
                    //    {
                    //        MultiOrder.Add(count);
                    //    }
                    //}

                    //TCPIPTesterScenarioCommand scenariocommand = Scenario.CommandsOrder.ToList().Find(x => x.order == CurrentOrderCount);

                    ScenarioCommand scenariocommand = Scenario.SelectedScenario.Commands.ToList().FirstOrDefault(x => x.Name.Contains(ContainText));

                    //string CorrectCommandName = string.Empty;

                    if (scenariocommand != null)
                    {
                        // 시나리오에 정의되어 있는 데이터를 이용하여 그 다음을 구성.
                        if (scenariocommand.RequestSet != null && scenariocommand.RequestSet.Count > 0)
                        {
                            GetReqeustcommandSet(scenariocommand.RequestSet);
                        }
                    }

                    //if (scenariocommand.ScenarioCommands.Any(x => x.CommandName.Contains(ContainText)) == true)
                    //{
                    //    ScenarioCommand scenarioCommand = scenariocommand.ScenarioCommands.FirstOrDefault(x => x.CommandName.Contains(ContainText));

                    //    //CorrectCommandName = scenarioCommand.CommandName;

                    //    //CurrentOrderCount++;

                    //    // 시나리오에 정의되어 있는 데이터를 이용하여 커맨드 데이터 세트 구성
                    //    if (scenariocommand.RequestSet != null && scenariocommand.RequestSet.Count > 0)
                    //    {
                    //        GetReqeustcommandSet(scenariocommand.RequestSet);
                    //    }
                    //}
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
        }
        private void GetReqeustcommandSet(List<RequestSet> requestset)
        {
            try
            {
                TesterCommand tmp = null;

                ScenarioTree tree = null;

                string requestname = string.Empty;

                foreach (var request in requestset)
                {
                    // 분기에 따른 Request를 얻어와야하는 경우 처리
                    if (request.getRequestNameFunc != null)
                    {
                        tree = request.getRequestNameFunc(this.lastReceiveQueryMsg);

                        //if (tree.NeedChangeOrder == true)
                        //{
                        //    CurrentOrderCount = tree.ChangeOrderNo;
                        //}

                        if (tree.RequestSet != null)
                        {
                            GetReqeustcommandSet(tree.RequestSet);
                        }

                        //requestname = tree.RequestName;
                    }
                    else
                    {
                        requestname = request.RequestName;
                    }

                    if (requestname != null && requestname != string.Empty)
                    {
                        tmp = Scenario.CommandRecipe.Commands.FirstOrDefault(x => x.Name.Contains(requestname));

                        if (tmp != null)
                        {
                            string commandname = string.Empty;

                            bool IsBinCode = requestname.Contains("BN");

                            if (IsBinCode == true)
                            {
                                BinDataMaker.SetDutCount(this.DutCount);
                                BinDataMaker.SetBinType(this.BinType);

                                BinDataMaker.SetPrefix("BN");

                                commandname = BinDataMaker.MakeBinInfo();
                            }
                            else
                            {
                                commandname = tmp.Name;
                            }

                            CommandAndReceived tmp2 = new CommandAndReceived();
                            //tmp2.Command = tmp.CommandName;

                            tmp2.Command = commandname;

                            Querylist.Add(tmp2);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                //LoggerManager.Exception(err);
            }
        }
    }

    public class CommandAndReceived
    {
        public string Command { get; set; }
        public bool Received { get; set; }
        public bool Sended { get; set; }
    }

    public class StringToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush retval = null;

            try
            {
                CollectionComponent inputVal = value as CollectionComponent;

                if (inputVal != null)
                {
                    retval = inputVal.Color;
                }

                //if (value != null && value.ToString().Contains("Send"))
                //    return "Red";
                //else if (value != null && value.ToString().Contains("Receive"))
                //    return "Blue";
                //else
                //    return "Black";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
