using LogModule;
using ProberErrorCode;
using ProberInterfaces.Communication;
using ProberInterfaces.Enum;
using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace Communication.TCPIPCommModule
{
    public partial class TCPIPCommModule : ICommModule, INotifyPropertyChanged
    {
        #region ==> NotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        public IPAddress iPAddress;
        public Socket ClientSock;
        public event setDataHandler SetDataChanged;        

        public TCPIPCommState TCPIPCommState = null;
        private bool IsDisposed = false;
        public bool Initialized { get; set; }
        private Ping ping = new Ping();
        private string _IP;
        public string IP
        {
            get { return _IP; }
            set { _IP = value; }
        }
        private int _Port;
        public int Port
        {
            get { return _Port; }
            set { _Port = value; }
        }
        private IPEndPoint _EP;
        public IPEndPoint EP
        {
            get { return _EP; }
            set { _EP = value; }
        }
        public TCPIPCommModule(string ip, int port)
        {
            IP = ip;
            Port = port;
        }

        private EnumCommunicationState _CurState;
        public EnumCommunicationState CurState
        {
            get { return _CurState; }
            set
            {
                if (value != _CurState)
                {
                    _CurState = value;
                    NotifyPropertyChanged();
                }
            }
        }

        SerialPort ICommModule.Port { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public EventCodeEnum Connect()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                iPAddress = IPAddress.Parse(IP);
                EP = new IPEndPoint(iPAddress, Port);
                ClientSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ClientSock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
                retVal = TCPIPCommState.Connect(ClientSock);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void DisConnect()
        {
            try
            {
                TCPIPCommState?.Disconnect();
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
                if (!IsDisposed)
                {
                    TCPIPCommState.Disconnect();
                    IsDisposed = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"CommModule.Dispose() Error occurred. Err = {err.Message}");
            }
        }

        public string GetReceivedData()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (!Initialized)
                {
                    TCPIPCommState?.Disconnect();
                    TCPIPCommState = new TCPIPDisconnectState(this);
                    Initialized = true;
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                retVal = EventCodeEnum.UNDEFINED;
            }

            return retVal;
        }
        public void DeInitModule()
        {
            Dispose();
        }
        public EnumCommunicationState GetCommState()
            => this.TCPIPCommState?.GetCommunicationState() ?? EnumCommunicationState.DISCONNECT;

        public void CommStateTransition(TCPIPCommState state)
        {
            try
            {
                if (TCPIPCommState?.GetType() != state?.GetType())
                {
                    LoggerManager.Debug($"[CommModule_TCPIP] CommStateTransition() Old = {TCPIPCommState.GetType()}, New = {state.GetType()}");
                    this.TCPIPCommState = state;
                    CurState = this.TCPIPCommState.GetCommunicationState();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public IPStatus PingTest(string address)
            => this.ping.Send(address)?.Status ?? IPStatus.Unknown;

        public void Read(Socket client)
            => this.TCPIPCommState?.Read(client);

        public void Send(string sendData)
            => this.TCPIPCommState?.Send(sendData, ClientSock);
        public void SetReceivedData(string receiveData)
        {
            try
            {
                SetDataChanged(receiveData);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum ReInitalize(bool attatch)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (TCPIPCommState.GetCommunicationState() == EnumCommunicationState.CONNECTED)
                {
                    DisConnect();
                }
                if (TCPIPCommState.GetCommunicationState() == EnumCommunicationState.DISCONNECT)
                {
                    if (attatch)
                    {
                        retVal = Connect();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void Send(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
    public abstract class TCPIPCommState
    {
        public TCPIPCommModule CommModule;
        public TCPIPCommState(TCPIPCommModule commModule)
        {
            this.CommModule = commModule;
        }
        public abstract void Disconnect();
        public abstract EnumCommunicationState GetCommunicationState();
        public abstract EventCodeEnum Connect(Socket clientSock);
        public abstract void Read(Socket client);
        public abstract void Send(string sendData, Socket clientSock);
        public abstract Socket ClientSock { get; set; }
    }
    public sealed class TCPIPDisconnectState : TCPIPCommState
    {
        public TCPIPDisconnectState(TCPIPCommModule commModule) : base(commModule)
        {
        }

        public override Socket ClientSock { get ; set; }

        public override EventCodeEnum Connect(Socket clientSocket)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ClientSock = clientSocket;

                ClientSock.Connect(this.CommModule.EP);

                if (ClientSock.Connected == true)
                {
                    LoggerManager.Debug($"[CommModule_TCPIP] Connect Success.");
                    this.CommModule.CommStateTransition(new TCPIPConnectState(this.CommModule));
                    this.CommModule.Read(ClientSock);
                    retVal = EventCodeEnum.NONE;
                    //string test = "01000000000C" + Environment.NewLine;
                    //byte[] test1 = Encoding.Default.GetBytes(test);
                    //ClientSock.Send(test1);
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override void Disconnect()
        {
            try
            {
                if (ClientSock != null)
                {
                    ClientSock?.Close();
                    ClientSock = null;
                    this.CommModule.CommStateTransition(new TCPIPDisconnectState(this.CommModule));
                    LoggerManager.Debug($"[CommModule_TCPIP] Disconnect()");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override EnumCommunicationState GetCommunicationState()
            => EnumCommunicationState.DISCONNECT;

        public override void Read(Socket client)
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void Send(string sendData, Socket clientSock)
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
    public sealed class TCPIPConnectState : TCPIPCommState
    {
        public TCPIPConnectState(TCPIPCommModule commModule) : base(commModule)
        {
        }
        public override Socket ClientSock { get; set; }
        public override EventCodeEnum Connect(Socket clientSocket)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

            }
            catch (Exception)
            {

            }
            return retVal;
        }

        public override void Disconnect()
        {
            try
            {
                if (ClientSock != null)
                {
                    ClientSock?.Close();
                    ClientSock = null;
                    this.CommModule.CommStateTransition(new TCPIPDisconnectState(this.CommModule));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override EnumCommunicationState GetCommunicationState()
            => EnumCommunicationState.CONNECTED;

        public override void Read(Socket client)
        {
            try
            {
                StateObject state = new StateObject();
                state.workSocket = client;
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                Disconnect();
            }
        }
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                //sendSocket = state.workSocket;
                //receiveDone.Set();

                // Read data from the remote device.  
                int bytesRead = state.workSocket.EndReceive(ar);

                string response = string.Empty;

                if (bytesRead > 0)
                {
                    response = Encoding.ASCII.GetString(state.buffer, 0, bytesRead);
                    LoggerManager.Debug($"[CommModule_TCPIP] Receive Data = {response}");
                    CommModule.SetReceivedData(response);
                    Read(state.workSocket);
                }
                else
                {
                    if (state.sb.Length > 1)
                    {
                        response = state.sb.ToString();
                    }
                }
            }
            catch (SocketException err)
            {
                LoggerManager.Exception(err);
                Disconnect();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public override void Send(string sendData, Socket clientSock)
        {
            try
            {
                LoggerManager.Debug($"[CommModule_TCPIP] Send Data = {sendData}");
                ClientSock = clientSock;
                byte[] sendDataByte = Encoding.Default.GetBytes(sendData);
                ClientSock.Send(sendDataByte);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                Disconnect();
            }
        }
    }
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
    }
}
