using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SorterPickTester.Net
{
    public class TcpServer
    {
        public delegate void AcceptCallbackHandler(Socket sock);
        public event AcceptCallbackHandler AcceptCallback;

        public delegate void ReceivedCallbackHandler(Socket sock, byte[] data, int len);
        public event ReceivedCallbackHandler ReceiveCallback;

        public delegate void DisonnectCallbackHandler();
        public event DisonnectCallbackHandler DisonnectCallback;

        byte[] recv_buffer = new byte[4096];
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Socket socket_conn = null;

        public bool Start(int port)
        {
            if (this.socket == null)
            {
                this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }

            this.socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));
            this.socket.Listen(1);

            this.socket.BeginAccept(new AsyncCallback(OnAccept), this.socket);

            return true;
        }

        public void Stop()
        {
            if (this.socket != null)
            {
                this.socket.Close();
                this.socket.Dispose();
            }

            this.socket = null;
            this.socket_conn = null;
        }

        public void Send(byte[] data)
        {
            try
            {
                if (this.socket_conn == null)
                    return;

                if (this.socket_conn.Connected)
                {
                    this.socket_conn.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(OnSend), this.socket_conn);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        public void OnAccept(IAsyncResult ar)
        {
            Socket sock = (Socket)ar.AsyncState;
            sock = sock.EndAccept(ar);
            IPEndPoint ep = (IPEndPoint)sock.RemoteEndPoint;

            socket_conn = sock;
            Console.WriteLine($"client accept : ip ({ep.Address}), port ({ep.Port})");

            AcceptCallback?.Invoke(sock);
            sock.BeginReceive(this.recv_buffer, 0, 4096, SocketFlags.None, new AsyncCallback(OnReceived), sock);
        }

        protected void OnSend(IAsyncResult ar)
        {
            Socket sock = (Socket)ar.AsyncState;
            int len = sock.EndSend(ar);
            Console.WriteLine($"Server Sending  data length : {len}");
        }

        protected void OnReceived(IAsyncResult ar)
        {
            try
            {
                Socket sock = (Socket)ar.AsyncState;
                IPEndPoint ep = (IPEndPoint)sock.RemoteEndPoint;

                int recvlen = sock.EndReceive(ar);
                if (recvlen < 0)
                {
                    DisonnectCallback?.Invoke();
                }
                else if (recvlen > 0)
                {
                    //Console.WriteLine($"Server Received data length : {recvlen}");

                    byte[] data = new byte[recvlen];
                    Array.Copy(this.recv_buffer, data, recvlen);

                    ReceiveCallback?.Invoke(sock, data, recvlen);
                    socket_conn.BeginReceive(this.recv_buffer, 0, 4096, SocketFlags.None, new AsyncCallback(OnReceived), socket_conn);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
