using System;
using System.Diagnostics;

namespace TesterDriverModule
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows;
    using System.Windows.Media;

    public class StateObject
    {
        // Client socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 1024 * 4;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();

        //public string LastSendMessage { get; set; }
        //public string LastReceiveMessage { get; set; }
    }

    public class TCPIPDriver : ITesterComDriver, IFactoryModule, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        // For Emulator

        //private DummyParam DummyParam { get; set; }

        // The port number for the remote device.  
        private int SendPort;
        private int ReceivePort;

        private IPAddress iPAddress;

        public static Socket sendSocket;
        public static Socket receiveSocket;

        public Socket ClientSock;

        public delegate void SocketConnect(Socket sock);

        // ManualResetEvent instances signal completion.  
        //private static ManualResetEvent connectDone = new ManualResetEvent(false);
        //private static ManualResetEvent sendDone = new ManualResetEvent(false);
        //private static ManualResetEvent receiveDone = new ManualResetEvent(false);

        private const string Terminator = "\r\n";

        private int SendDelayTime = 0;

        // The response from the remote device.  
        //private static String response = String.Empty;

        private static List<string> responselist = new List<string>();

        public static EnumTesterDriverState State = EnumTesterDriverState.UNDEFINED;

        private Dictionary<int, int> SocketAndPort { get; set; }

        //private void ValidationTerminator()
        //{
        //    string tempterminator = string.Empty;

        //    try
        //    {
        //        tempterminator = Terminator.Replace("\\r", "\r").Replace("\\n", "\n");

        //        LoggerManager.Debug($"[TCPIPDriver], ValidationTerminator() : Old = {Terminator} new = {tempterminator}");

        //        Terminator = tempterminator;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        //private ObservableCollection<string> _CommandHistories = new ObservableCollection<string>();
        //public ObservableCollection<string> CommandHistories
        //{
        //    get { return _CommandHistories; }
        //    set
        //    {
        //        if (value != _CommandHistories)
        //        {
        //            _CommandHistories = value;
        //        }
        //    }
        //}

        #region For Deubbging

        private ObservableCollection<CollectionComponent> _CommandCollection;
        public ObservableCollection<CollectionComponent> CommandCollection
        {
            get { return _CommandCollection; }
            set
            {
                if (value != _CommandCollection)
                {
                    _CommandCollection = value;
                    RaisePropertyChanged();
                }
            }
        }

        private SolidColorBrush InterruputPortBrush = new SolidColorBrush(Colors.Purple);
        private SolidColorBrush CommandPortBrush = new SolidColorBrush(Colors.Blue);
        private SolidColorBrush DefaultBrush = new SolidColorBrush(Colors.Black);

        private static object HistoriesLockObject = new object();

        #endregion


        public TCPIPDriver()
        {
            if (CommandCollection == null)
            {
                CommandCollection = new ObservableCollection<CollectionComponent>();
            }
        }
        private void CommandCollectionAdd(string message, SolidColorBrush color = null)
        {
            try
            {
                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    lock (HistoriesLockObject)
                    {
                        CollectionComponent collectionComponent = new CollectionComponent();

                        string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);

                        collectionComponent.Message = timestamp + " " + message;

                        if (color == null)
                        {
                            collectionComponent.Color = new SolidColorBrush(Colors.Black);
                        }
                        else
                        {
                            collectionComponent.Color = color;
                        }

                        CommandCollection.Add(collectionComponent);
                    }
                }));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum Connect(object connectparam)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                ITCPIPSysParam param = connectparam as ITCPIPSysParam;

                if (CommandCollection == null)
                {
                    CommandCollection = new ObservableCollection<CollectionComponent>();
                }

                if (CommandCollection != null)
                {
                    CommandCollection.Clear();
                }

                if (SocketAndPort == null)
                {
                    SocketAndPort = new Dictionary<int, int>();
                }
                else
                {
                    SocketAndPort.Clear();
                }

                responselist.Clear();

                //Terminator = param.Terminator.Value;

                //ValidationTerminator();

                if (param != null)
                {
                    SendDelayTime = param.SendDelayTime.Value;
                    iPAddress = IPAddress.Parse(param.IP.Value.IP);
                    SendPort = param.SendPort.Value;
                    ReceivePort = param.ReceivePort.Value;

                    if (AppDomain.CurrentDomain.FriendlyName == "ProberEmulator.exe")
                    {
                        string msg = $"Connect start : IP = { param.IP.Value.IP}, Send port = { SendPort }, Receive port = { ReceivePort}, SendDelayTime = { SendDelayTime}";

                        CommandCollectionAdd(msg);

                        //Application.Current.Dispatcher.Invoke((Action)delegate
                        //{
                        //    lock (HistoriesLockObject)
                        //    {
                        //        string msg = timestamp + " " + $"Connect start : IP = { param.IP.Value.IP}, Send port = { SendPort }, Receive port = { ReceivePort}, SendDelayTime = { SendDelayTime}";

                        //        CommandCollectionAdd(msg);
                        //    }
                        //});
                    }

                    LoggerManager.Debug($"[TCPIPDriver], Connect() : IP = {param.IP.Value.IP}, Send port = {SendPort}, Receive port = {ReceivePort}, SendDelayTime = {SendDelayTime}");

                    // Establish the remote endpoint for the socket.
                    // The name of the   
                    // remote device is "host.contoso.com".
                    IPEndPoint sendEP = new IPEndPoint(iPAddress, SendPort);
                    //IPEndPoint receiveEP = new IPEndPoint(iPAddress, port2);

                    //// Create a TCP/IP socket.  
                    sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    //receiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    SocketAndPort.Add(sendSocket.GetHashCode(), SendPort);

                    sendSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
                    //receiveSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);

                    //sendSocket.Bind(sendEP);
                    //ClientSock = sendSocket;
                    // Connect to the remote endpoint.
                    //client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
                    //connectDone.WaitOne();
                    //receiveSocket.Connect(receiveEP);
                    sendSocket.Connect(sendEP);

                    if (sendSocket.Connected == true)
                    {
                        if (AppDomain.CurrentDomain.FriendlyName == "ProberEmulator.exe")
                        {
                            string msg = $"Connect succeed (Interrupt port)";

                            CommandCollectionAdd(msg);

                            //string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);

                            //Application.Current.Dispatcher.Invoke((Action)delegate
                            //{
                            //    lock (HistoriesLockObject)
                            //    {
                            //        CommandCollectionAdd(timestamp + " " + $"Connect succeed (Interrupt port)");
                            //    }
                            //});
                        }

                        // TODO : Command Port listen

                        IPEndPoint localendpoint = sendSocket.LocalEndPoint as IPEndPoint;

                        var recevieiPAddress = IPAddress.Parse(localendpoint.Address.ToString());

                        //IPEndPoint receiveEP = new IPEndPoint(IPAddress.Any, ReceivePort);
                        IPEndPoint receiveEP = new IPEndPoint(recevieiPAddress, ReceivePort);

                        receiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                        receiveSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);

                        receiveSocket.Bind(receiveEP);

                        LoggerManager.Debug($"[TCPIPDriver] Receive socket binded. EndPoint = {receiveEP}");

                        receiveSocket.Listen(100);

                        receiveSocket.BeginAccept(new AsyncCallback(AcceptCallback), receiveSocket);

                        LoggerManager.Debug($"[TCPIPDriver] Receive socket listen start.");

                        retval = EventCodeEnum.NONE;

                        SetState(EnumTesterDriverState.CONNECTED);

                        Receive(sendSocket);
                    }
                    else
                    {
                        if (AppDomain.CurrentDomain.FriendlyName == "ProberEmulator.exe")
                        {
                            string msg = $"Connect failed (Interrupt port)";

                            CommandCollectionAdd(msg);

                            //string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);

                            //Application.Current.Dispatcher.Invoke((Action)delegate
                            //{
                            //    lock (HistoriesLockObject)
                            //    {
                            //        CommandCollectionAdd(timestamp + " " + $"Connect failed (Interrupt port)");
                            //    }
                            //});
                        }

                        retval = EventCodeEnum.UNDEFINED;
                    }
                    //// Receive the response from the remote device.  
                    //Receive(client);

                    // Write the response to the console.  
                    //LoggerManager.Debug($"Response received : {response}");
                }
                else
                {
                    retval = EventCodeEnum.UNDEFINED;

                    LoggerManager.Debug($"[TCPIPDriver], Connect() : Parameter is null.");
                }
            }
            catch (ArgumentNullException er)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, er);
                retval = EventCodeEnum.UNDEFINED;
            }
            catch (SocketException err)
            {
                if (AppDomain.CurrentDomain.FriendlyName == "ProberEmulator.exe")
                {
                    string msg = $"{err}";

                    CommandCollectionAdd(msg);

                    //string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);

                    //Application.Current.Dispatcher.Invoke((Action)delegate
                    //{
                    //    lock (HistoriesLockObject)
                    //    {
                    //        CommandCollectionAdd(timestamp + " " + $"{err}");
                    //    }
                    //});
                }

                //receiveSocket.Close();

                if (sendSocket != null)
                {
                    sendSocket.Close();
                }
                else
                {
                    LoggerManager.Debug("[TCPIPDriver] Connect() : sendSocket is null.");
                }

                SocketExceptionOccurred(err);

                retval = EventCodeEnum.UNDEFINED;
            }
            catch (ObjectDisposedException er)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, er);
                retval = EventCodeEnum.UNDEFINED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                LoggerManager.Debug("[TCPIPDriver] AcceptCallback() : receiveSocket is connected.");

                if (AppDomain.CurrentDomain.FriendlyName == "ProberEmulator.exe")
                {
                    string msg = $"Connect succeed (Command port)";

                    CommandCollectionAdd(msg);

                    //string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);

                    //Application.Current.Dispatcher.Invoke((Action)delegate
                    //{
                    //    lock (HistoriesLockObject)
                    //    {
                    //        CommandCollectionAdd(timestamp + " " + $"Connect succeed (Command port)");
                    //    }
                    //});
                }

                receiveSocket = (Socket)ar.AsyncState;
                Socket handler = receiveSocket.EndAccept(ar);
                receiveSocket = handler;

                StateObject state = new StateObject();
                state.workSocket = handler;

                if (SocketAndPort != null)
                {
                    SocketAndPort.Add(handler.GetHashCode(), ReceivePort);
                }

                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch (SocketException err)
            {
                if (receiveSocket != null)
                {
                    receiveSocket.Close();
                }
                else
                {
                    LoggerManager.Debug("[TCPIPDriver] AcceptCallback() : receiveSocket is null.");
                }

                if (sendSocket != null)
                {
                    sendSocket.Close();
                }
                else
                {
                    LoggerManager.Debug("[TCPIPDriver] AcceptCallback() : sendSocket is null.");
                }

                SocketExceptionOccurred(err);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum DisConnect()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (sendSocket != null)
                {
                    sendSocket.Close();
                    //sendSocket.Dispose();

                    sendSocket = null;
                }
                else
                {
                    LoggerManager.Debug("[TCPIPDriver] DisConnect() : sendSocket is null.");
                }

                if (receiveSocket != null)
                {
                    receiveSocket.Close();
                    //receiveSocket.Dispose();

                    receiveSocket = null;
                }
                else
                {
                    LoggerManager.Debug("[TCPIPDriver] DisConnect() : receiveSocket is null.");
                }

                retval = EventCodeEnum.NONE;
            }
            catch (SocketException err)
            {
                SocketExceptionOccurred(err);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }

            return retval;
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                sendSocket = (Socket)ar.AsyncState;

                // Complete the connection.  
                sendSocket.EndConnect(ar);

                //Console.WriteLine("Socket connected to {0}", client.RemoteEndPoint.ToString());
                LoggerManager.Debug($"[TCPIPDriver] ConnectCallback() : Socket connected to {sendSocket.RemoteEndPoint.ToString()}");

                // Signal that the connection has been made.  
                //connectDone.Set();

                //Receive(client);
            }
            catch (Exception err)
            {
                if (sendSocket != null)
                {
                    sendSocket.Close();
                }
                else
                {
                    LoggerManager.Debug("[TCPIPDriver] ConnectCallback() : sendSocket is null.");
                }

                LoggerManager.Exception(err);
            }
        }

        private void Receive(Socket client)
        {
            try
            {
                // Create the state object.  
                StateObject state = new StateObject();
                state.workSocket = client;

                //if(State != EnumTesterDriverState.BEGINRECEIVE)
                //{
                LoggerManager.Debug("[TCPIPDriver] Start BeginRecevie()");

                // Begin receiving the data from the remote device.
                //receiveDone.Reset();
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);

                //SetState(EnumTesterDriverState.BEGINRECEIVE);
                //}

                //receiveDone.WaitOne();
            }
            catch (SocketException err)
            {
                if (sendSocket != null)
                {
                    sendSocket.Close();
                }
                else
                {
                    LoggerManager.Debug("[TCPIPDriver] Receive() : sendSocket is null.");
                }

                SocketExceptionOccurred(err);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                LoggerManager.Debug("[TCPIPDriver] Called ReceiveCallback()");
                //LoggerManager.Debug("[TCPIPDriver] Called ReceiveCallback()");

                // Retrieve the state object and the client socket   
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                //sendSocket = state.workSocket;
                //receiveDone.Set();

                // Read data from the remote device.  
                int bytesRead = state.workSocket.EndReceive(ar);

                string response = string.Empty;

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.  

                    //Console.WriteLine(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    //string receiveMessage = Encoding.ASCII.GetString(state.buffer, 0, bytesRead);

                    response = Encoding.ASCII.GetString(state.buffer, 0, bytesRead);
                    //response = RemoveTerminator(response);

                    var messages = SeperateMultipleMessages(response);

                    foreach (var message in messages)
                    {
                        responselist.Add(message);
                    }

                    if (AppDomain.CurrentDomain.FriendlyName == "ProberEmulator.exe")
                    {
                        SolidColorBrush solidColorBrush = null;
                        bool CanUsePort = false;
                        int port = 0;
                        CanUsePort = SocketAndPort.TryGetValue(state.workSocket.GetHashCode(), out port);

                        string msg = string.Empty;
                        string porttype = string.Empty;

                        if (CanUsePort == true)
                        {
                            if (port == SendPort)
                            {
                                solidColorBrush = InterruputPortBrush;
                                porttype = "INTERRUPT";
                            }
                            else if (port == ReceivePort)
                            {
                                solidColorBrush = CommandPortBrush;
                                porttype = "COMMAND";
                            }

                            msg = $"[{porttype}] RECEIVE({port}) : " + response;
                            //msg = $"[RECEIVE ({port})] " + response;
                        }
                        else
                        {
                            msg = $"[UNKNOWN] RECEIVE(UNKNOWN) : " + response;
                            //msg = $"[RECEIVE (Unknown Port)] " + response;
                        }

                        CommandCollectionAdd(msg, solidColorBrush);

                        //string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);

                        //Application.Current.Dispatcher.Invoke((Action)delegate
                        //{


                        //    int port = 0;
                        //    bool CanUsePort = false;

                        //    CanUsePort = SocketAndPort.TryGetValue(state.workSocket.GetHashCode(), out port);

                        //    lock (HistoriesLockObject)
                        //    {
                        //        if (CanUsePort == true)
                        //        {
                        //            CommandCollectionAdd(timestamp + " " + $"[RECEIVE ({port})] " + response);
                        //        }
                        //        else
                        //        {
                        //            CommandCollectionAdd(timestamp + " " + $"[RECEIVE (Unknown Port)] " + response);
                        //        }
                        //    }
                        //});

                        //Dispatcher.CurrentDispatcher.Invoke((Action)(() =>
                        //{

                        //}));
                    }

                    LoggerManager.Debug($"[TCPIPDriver] ReceiveCallback() : Receive message = {response}. Count = {responselist.Count}");

                    SetState(EnumTesterDriverState.RECEIVED);

                    Receive(state.workSocket);
                }
                else
                {
                    // All the data has arrived; put it in response.  
                    if (state.sb.Length > 1)
                    {
                        response = state.sb.ToString();

                        responselist.Add(response);

                        LoggerManager.Debug($"[TCPIPDriver] ReceiveCallback() : Receive message = {response}. Count = {responselist.Count}");
                    }
                    // Signal that all bytes have been received.  
                    //receiveDone.Set();
                }
            }
            catch (SocketException err)
            {
                if (sendSocket != null)
                {
                    sendSocket.Close();
                }
                else
                {
                    LoggerManager.Debug("[TCPIPDriver] ReceiveCallback() : sendSocket is null.");
                }

                SocketExceptionOccurred(err);
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

                //byte[] byteData = Encoding.ASCII.GetBytes(data);

                LoggerManager.Debug("[TCPIPDriver] Start SendCallback()");

                // Begin sending the data to the remote device.
                //sendDone.Reset();

                //LoggerManager.Debug($"[TCPIPDriver], Send() : Handler = {handler.GetHashCode()}");

                if (SendDelayTime > 0)
                {
                    Thread.Sleep(SendDelayTime);
                }

                if (AppDomain.CurrentDomain.FriendlyName == "ProberEmulator.exe")
                {
                    SolidColorBrush solidColorBrush = null;
                    bool CanUsePort = false;
                    int port = 0;
                    CanUsePort = SocketAndPort.TryGetValue(handler.GetHashCode(), out port);

                    string msg = string.Empty;
                    string porttype = string.Empty;

                    if (CanUsePort == true)
                    {
                        if (port == SendPort)
                        {
                            solidColorBrush = InterruputPortBrush;
                            porttype = "INTERRUPT";
                        }
                        else if (port == ReceivePort)
                        {
                            solidColorBrush = CommandPortBrush;
                            porttype = "COMMAND";
                        }

                        msg = $"[{porttype}] SEND({port}) : " + dataWithTerminator;
                    }
                    else
                    {
                        msg = $"[UNKNOWN] SEND(UNKNOWN) : " + dataWithTerminator;
                        //msg = $"[SEND (Unknown Port)] " + dataWithTerminator;
                    }

                    CommandCollectionAdd(msg, solidColorBrush);

                    //string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);

                    //Application.Current.Dispatcher.Invoke((Action)delegate
                    //{
                    //    int port = 0;
                    //    bool CanUsePort = false;

                    //    CanUsePort = SocketAndPort.TryGetValue(handler.GetHashCode(), out port);

                    //    lock (HistoriesLockObject)
                    //    {
                    //        if (CanUsePort == true)
                    //        {
                    //            CommandCollectionAdd(timestamp + " " + $"[SEND ({port})] " + dataWithTerminator);
                    //        }
                    //        else
                    //        {
                    //            CommandCollectionAdd(timestamp + " " + $"[SEND (Unknown Port)] " + dataWithTerminator);
                    //        }
                    //    }
                    //});
                }

                handler.BeginSend(sendByte, 0, sendByte.Length, 0, new AsyncCallback(SendCallback), handler);

                //sendDone.WaitOne();
                //SetState(EnumTesterDriverState.BEGINSEND);
            }
            catch (SocketException err)
            {
                if (sendSocket != null)
                {
                    sendSocket.Close();
                }
                else
                {
                    LoggerManager.Debug("[TCPIPDriver] Send() : sendSocket is null.");
                }

                SocketExceptionOccurred(err);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                LoggerManager.Debug($"[TCPIPDriver] Called SendCallback() : Handler = {sendSocket.GetHashCode()}");

                Socket handler = (Socket)ar.AsyncState;

                // Retrieve the socket from the state object.  
                //sendSocket = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                //int bytesSent = sendSocket.EndSend(ar);
                int bytesSent = handler.EndSend(ar);

                if (AppDomain.CurrentDomain.FriendlyName == "ProberEmulator.exe")
                {
                    SolidColorBrush solidColorBrush = null;

                    bool CanUsePort = false;
                    int port = 0;
                    CanUsePort = SocketAndPort.TryGetValue(handler.GetHashCode(), out port);

                    string msg = string.Empty;
                    string porttype = string.Empty;

                    if (CanUsePort == true)
                    {
                        if (port == SendPort)
                        {
                            solidColorBrush = InterruputPortBrush;
                            porttype = "INTERRUPT";
                        }
                        else if (port == ReceivePort)
                        {
                            solidColorBrush = CommandPortBrush;
                            porttype = "COMMAND";
                        }

                        msg = $"[{porttype}] SENDED({port}) : " + $"Sent {bytesSent} bytes.";
                        //msg = $"[SENDED ({port})] " + $"Sent {bytesSent} bytes.";
                    }
                    else
                    {
                        msg = $"[UNKNOWN] SENDED(UNKNOWN) : " + $"Sent {bytesSent} bytes.";
                        //msg = $"[SENDED (Unknown Port)] " + $"Sent {bytesSent} bytes.";
                    }

                    CommandCollectionAdd(msg, solidColorBrush);

                    //string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);

                    //Application.Current.Dispatcher.Invoke((Action)delegate
                    //{
                    //    int port = 0;
                    //    bool CanUsePort = false;

                    //    CanUsePort = SocketAndPort.TryGetValue(handler.GetHashCode(), out port);

                    //    lock (HistoriesLockObject)
                    //    {
                    //        if (CanUsePort == true)
                    //        {
                    //            CommandCollectionAdd(timestamp + " " + $"[SENDED ({port})] " + $"Sent {bytesSent} bytes.");
                    //        }
                    //        else
                    //        {
                    //            CommandCollectionAdd(timestamp + " " + $"[SENDED (Unknown Port)] " + $"Sent {bytesSent} bytes.");
                    //        }
                    //    }
                    //});
                }

                //Console.WriteLine("Sent {0} bytes to server.", bytesSent);
                LoggerManager.Debug($"[TCPIPDriver] SendCallback() : Sent {bytesSent} bytes to server.");

                // Signal that all bytes have been sent.  
                //sendDone.Set();
                //SetState(EnumTesterDriverState.SENDED);
            }
            catch (SocketException err)
            {
                if (sendSocket != null)
                {
                    sendSocket.Close();
                }
                else
                {
                    LoggerManager.Debug("[TCPIPDriver] SendCallback() : sendSocket is null.");
                }

                SocketExceptionOccurred(err);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void Reset()
        {

        }

        public void SetState(EnumTesterDriverState state)
        {
            try
            {
                EnumTesterDriverState oldstate = State;

                State = state;

                LoggerManager.Debug($"[TCPIPDriver] SetState() : Old State = {oldstate} -> New State  = {State}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public string Read()
        {
            string response = string.Empty;

            try
            {
                if (responselist.Count > 0)
                {
                    response = responselist[0];
                    responselist.RemoveAt(0);
                }

                //Receive(receiveSocket);

                if (responselist.Count == 0)
                {
                    SetState(EnumTesterDriverState.SENDED);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return response;
        }

        public void WriteSTB(object command)
        {
            string STB = null;

            try
            {
                STB = (string)command;

                Send(sendSocket, STB);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void WriteString(string query_command)
        {
            try
            {
                Send(receiveSocket, query_command);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public object GetState()
        {
            return State;
        }

        private void SocketExceptionOccurred(SocketException socketerr)
        {
            try
            {
                string prevFuncName = new StackFrame(1, true).GetMethod().Name;

                LoggerManager.Error($"[TCPIPDriver] {prevFuncName}() : Socekt Error Code : {socketerr.SocketErrorCode}");
                LoggerManager.Exception(socketerr);

                SetState(EnumTesterDriverState.ERROR);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            //StackTrace stackTrace = new StackTrace();           
            //StackFrame[] stackFrames = stackTrace.GetFrames();  

            //foreach (StackFrame stackFrame in stackFrames)
            //{
            //    Console.WriteLine(stackFrame.GetMethod().Name);
            //}

            //System.Reflection.MethodBase.GetCurrentMethod();
        }

        //public EventCodeEnum StartReceive()
        //{
        //    EventCodeEnum retval = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        //Receive(sendSocket);

        //        retval = EventCodeEnum.NONE;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        private List<string> SeperateMultipleMessages(string recv_data)
        {
            List<string> retval = new List<string>();

            try
            {
                // \r\n\r\n to \r\n
                string removeDoubleTerminatorstr = recv_data.Replace(Terminator + Terminator, Terminator);

                string[] seperatemessages = Regex.Split(removeDoubleTerminatorstr, Terminator);

                foreach (var message in seperatemessages)
                {
                    if (string.IsNullOrEmpty(message) == false)
                    {
                        retval.Add(RemoveTerminator(message));
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private string RemoveTerminator(string recv_data)
        {
            string terminator = Terminator;
            string retStr = string.Empty;

            try
            {
                if (terminator != null && terminator != string.Empty)
                {
                    retStr = recv_data.Replace(terminator, "");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retStr;
        }
    }
}
