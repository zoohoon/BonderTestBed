using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SorterPickTester.Net
{
    public class TcpClient
    {
        public delegate void ReceivedCallbackHandler(Socket sock, byte[] data, int len);
        public event ReceivedCallbackHandler ReceiveCallback;

        public delegate void ConnectCallbackHandler(Socket sock);
        public event ConnectCallbackHandler ConnectCallback;

        public delegate void DisconnectCallbackHandler();
        public event DisconnectCallbackHandler DisconnectCallback;


        byte[] recv_buffer = new byte[4096];
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Socket socket_conn = null;
        public bool bConnected = false;

        public bool Connect(string host, int port)
        {
            if (!bConnected)
            {
                if (this.socket == null)
                {
                    this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }

                this.socket.BeginConnect(host, port, new AsyncCallback(OnConnected), socket);
                bConnected = true;
            }

            return true;
        }

        public void Disconnect()
        {
            if (this.socket != null)
            {
                this.socket.Disconnect(false);
                this.socket.Dispose();
            }

            this.socket = null;
            this.socket_conn = null;
            bConnected = false;
        }

        public void Send(byte[] data)
        {
            try
            {
                if (this.socket == null)
                    return;

                if (this.socket.Connected)
                {
                    this.socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(OnSend), this.socket);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        protected void OnConnected(IAsyncResult ar)
        {
            try
            {
                Socket sock = (Socket)ar.AsyncState;
                IPEndPoint ep = (IPEndPoint)sock.RemoteEndPoint;

                Console.WriteLine($"server connected : ip ({ep.Address}), port ({ep.Port})");

                socket_conn = sock;
                sock.EndConnect(ar);

                ConnectCallback?.Invoke(socket_conn);
                sock.BeginReceive(this.recv_buffer, 0, 4096, SocketFlags.None, new AsyncCallback(OnReceived), sock);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected void OnSend(IAsyncResult ar)
        {
            Socket sock = (Socket)ar.AsyncState;
            int len = sock.EndSend(ar);
            //Console.WriteLine($"Client Sending  data length : {len}");
        }

        protected void OnReceived(IAsyncResult ar)
        {
            try
            {
                Socket sock = (Socket)ar.AsyncState;
                if (!sock.Connected)
                {
                    DisconnectCallback?.Invoke();
                    return;
                }

                IPEndPoint ep = (IPEndPoint)sock.RemoteEndPoint;

                int recvlen = sock.EndReceive(ar);
                if (recvlen < 0)
                {
                    DisconnectCallback?.Invoke();
                }
                else if (recvlen > 0)
                {
                    //Console.WriteLine($"Client Received data length : {recvlen}");

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
