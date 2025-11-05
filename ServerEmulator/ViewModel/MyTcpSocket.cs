using System;
using System.Net.Sockets;

namespace ServerEmulator.ViewModel
{
    public class MyTcpSocket : IDisposable
    {
        public Socket _socket;
        string _host;
        int _port;

        public const int BufferSize = 1024 * 4;
        public byte[] buffer = new byte[BufferSize];

        bool _IsConnected;
        public bool IsConnected
        {
            set { _IsConnected = value; }
            get { return _IsConnected; }
        }

        public int GetBufferSize()
        {
            return BufferSize;
        }

        public int SendTimeout
        {
            get { return (int)_socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout); }
            set { _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, value); }
        }

        public int ReceiveTimeout
        {
            get { return (int)_socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout); }
            set { _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, value); }
        }

        public MyTcpSocket()
        {

        }

        public MyTcpSocket(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public bool Connect(bool ReuseAddressFlag)
        {
            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.Connect(_host, _port);

                _IsConnected = true;

                OnConnected();
                return true;
            }
            catch
            {
                Close();
            }

            return false;
        }

        //public bool Connect(IPEndPoint endpoint, bool ReuseAddressFlag)
        //{
        //    try
        //    {
        //        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        //        if(ReuseAddressFlag == true)
        //        {
        //            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
        //        }

        //        _socket.Connect(endpoint);

        //        _connected = true;

        //        OnConnected();
        //        return true;
        //    }
        //    catch
        //    {
        //        Close();
        //    }

        //    return false;
        //}

        public void Close()
        {
            bool fireLostEvent = _IsConnected == true;

            _IsConnected = false;

            if (_socket != null)
            {
                try
                {
                    _socket.Close();
                }
                catch { }

                if (fireLostEvent == true)
                {
                    OnConnectionLost();
                }

                _socket = null;
            }
        }

        public event Action Connected;
        protected virtual void OnConnected()
        {
            //Debug.WriteLine("Connected.");
            if (Connected == null)
            {
                return;
            }

            Connected();
        }

        public event Action ConnectionLost;
        protected virtual void OnConnectionLost()
        {
            //Debug.WriteLine("Connection lost");
            if (ConnectionLost == null)
            {
                return;
            }

            ConnectionLost();
        }

        public int Write(byte[] buffer)
        {
            if (_IsConnected == false)
            {
                return 0;
            }

            int offset = 0;
            int size = buffer.Length;

            int totalSent = 0;

            while (true)
            {
                int sent = 0;

                try
                {
                    sent = _socket.Send(buffer, 0, size, SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.TimedOut)
                    {
                        break;
                    }

                    Close();
                    break;
                }

                totalSent += sent;

                if (totalSent == buffer.Length)
                {
                    break;
                }

                offset += sent;
                size -= sent;
            }

            return totalSent;
        }

        public int Read(byte[] readBuf)
        {
            return Read(readBuf, 0, readBuf.Length);
        }

        public int Read(byte[] buffer, int offset, int size)
        {
            if (_IsConnected == false)
            {
                return 0;
            }

            if (size == 0)
            {
                return 0;
            }

            int totalRead = 0;
            int readRemains = size;

            while (true)
            {
                int readLen = 0;

                try
                {
                    readLen = _socket.Receive(buffer, offset, readRemains, SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.TimedOut)
                    {
                        break;
                    }
                }

                if (readLen == 0)
                {
                    Close();
                    break;
                }

                totalRead += readLen;

                if (totalRead == size)
                {
                    break;
                }

                offset += readLen;
                readRemains -= readLen;
            }

            return totalRead;
        }

        void IDisposable.Dispose()
        {
            Close();
        }
    }
}

//MyTcpSocket socket = new MyTcpSocket(serverAddress, 5000);

//// ...[생략]...

//socket.ReceiveTimeout = 5000; // Send/Receive에 대해 5초 타임아웃 지정
//socket.SendTimeout = 5000;

//Console.WriteLine("ReceiveTimeout: " + socket.ReceiveTimeout);
//Console.WriteLine("SendTimeout: " + socket.SendTimeout);

//bool loop = true;

//while (loop)
//{
//    byte[] buf = new byte[4];
//    Console.WriteLine("Reading...");
//    socket.Read(buf); // Receive 호출 후 5초 이내에 데이터를 받지 못하면 Timeout SocketException 발생
//    Console.WriteLine(socket.IsConnected);
//}
