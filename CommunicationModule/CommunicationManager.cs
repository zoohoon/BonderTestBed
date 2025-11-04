using System;

namespace CommunicationModule
{
    using LogModule;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Threading;

    public static class CommunicationManager
    {
        static object pingLockObj = new object();
        private static bool _CommunicationFalg = false;

        public static bool CheckAvailabilityCommunication(string ip, int port)
        {
            bool retVal = false;
            try
            {
                lock (pingLockObj)
                {
                    
                    if (ip != null)
                    {
                        ConnectTest(ip, port);
                        retVal = _CommunicationFalg;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
            return retVal;
        }

        private static void ConnectTest(string ip, int port)
        {
            bool result = false;
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            try
            {
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, false);
              


                IAsyncResult ret = socket.BeginConnect(ip, port, null, null);
                Thread.Sleep(500);
                result = ret.AsyncWaitHandle.WaitOne(100, true);

                result = socket.Connected;

                if(result == true)
                {
                   socket.EndConnect(ret);
                }
                
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
            }
            finally
            {
                if (socket != null)
                {
                    
                    socket.Close();
                }
                _CommunicationFalg = result;
            }

        }

    }
}
