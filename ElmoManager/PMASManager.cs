using Configurator;
using ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET;
using ProberInterfaces;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ProberErrorCode;
using LogModule;
using System.Reflection;

namespace ElmoManager
{
    public class PMASManager : IPMASManager, INotifyPropertyChanged
    {
        //public static List<ParameterKeyValuePair> ParameterPairs = new List<ParameterKeyValuePair>(new ParameterKeyValuePair[] {
        //                                            new ParameterKeyValuePair(typeof(ElmoDescripter), @"c:\probersystem\parameters\Elmo\ElmoSetting.xml"),
        //});

        public bool Initialized { get; set; } = false;

        //private IParam _SysParam;
        //public IParam SysParam
        //{
        //    get { return _SysParam; }
        //    set
        //    {
        //        if (value != _SysParam)
        //        {
        //            _SysParam = value;
        //        }
        //    }
        //}

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public const int COMMUNICATION_TCP_PORT = 4000;
        public const int COMMUNICATION_UDP_PORT = 5000;

        ElmoDescripterParam ElmoDescripterParams;
        private int _ConnHndl;
        public int ConnHndl
        {
            get { return _ConnHndl; }
            set { _ConnHndl = value; }
        }
        private bool _InitFlag;

        public bool InitFlag
        {
            get { return _InitFlag; }
            set { _InitFlag = value; }
        }


        public PMASManager()
        {
            _InitFlag = false;
        }

        public EventCodeEnum InitializeController()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (InitFlag == false)
                {

                    IPAddress rIP, lIP;
                    IPAddress.TryParse(ElmoDescripterParams.PIP, out rIP);  // P-MAS IP
                    IPAddress.TryParse(ElmoDescripterParams.LIP, out lIP);  // LOCAL IP

                    // CONNECTION PROCESS 
                    if (MMCConnection.ConnectRPC(rIP, COMMUNICATION_TCP_PORT, lIP, COMMUNICATION_UDP_PORT, null,
                        0xEFFFFFFF, out _ConnHndl) != 0)
                    {
                        // MessageBox.Show("CONNECTION FAILED. CHECK NETWORK CONFIGURATION.", "CONNECTION FAILED");
                        InitFlag = false;

                        LoggerManager.Debug($"PMASManager InitializeController() ConnectRPC faild");

                        RetVal = EventCodeEnum.UNDEFINED;

                        return RetVal;
                    }

                    InitFlag = true;

                    RetVal = EventCodeEnum.NONE;
                }
                else
                {

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                InitFlag = false;

                RetVal = EventCodeEnum.UNDEFINED;
            }

            return RetVal;
        }

        private byte[] ReadBySize(Socket socket, int size = 0)
        {
            var readEvent = new AutoResetEvent(false);
            if (size == 0)
            {
                size = 8;
            }
            var buffer = new byte[size]; //Receive buffer
            try
            {
                var totalRecieved = 0;
                do
                {
                    var recieveArgs = new SocketAsyncEventArgs()
                    {
                        UserToken = readEvent
                    };
                    recieveArgs.SetBuffer(buffer, totalRecieved, size - totalRecieved);//Receive bytes from x to total - x, x is the number of bytes already recieved
                    recieveArgs.Completed += recieveArgs_Completed;
                    socket.ReceiveAsync(recieveArgs);
                    readEvent.WaitOne();//Wait for recieve

                    if (recieveArgs.BytesTransferred == 0)//If now bytes are recieved then there is an error
                    {
                        if (recieveArgs.SocketError != SocketError.Success)
                        {
                            buffer = null;
                            return buffer;
                        }
                    }
                    for (int i = totalRecieved; i < totalRecieved + recieveArgs.BytesTransferred; i++)
                    {
                        if (buffer[i] == ';')
                        {
                            return buffer;
                        }
                    }
                    totalRecieved += recieveArgs.BytesTransferred;

                } while (totalRecieved != size);//Check if all bytes has been received
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return buffer;
        }
        private void recieveArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                var are = (AutoResetEvent)e.UserToken;
                are.Set();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private int readSize = 20;
        //private byte[] ReadBtyeArray(Socket socket)
        //{

        //    byte[] rbuffer = new byte[readSize];
        //    socket.Receive(rbuffer);
        //}
        private int ElmoReceiveMessage(int nodeNum, String obj, out int value)
        {
            int retVal = -1;
            value = 0;
            try
            {

                IPAddress rIP, nodeIP;
                IPAddress.TryParse(ElmoDescripterParams.PIP, out rIP);  // P-MAS IP

                byte[] ipArr = rIP.GetAddressBytes();
                ipArr[ipArr.Length - 1] += (byte)(nodeNum);
                string input = obj + ";\n\r";
                nodeIP = new IPAddress(ipArr);
                int port = 5001;
                //UDP Socket 생성
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.ReceiveTimeout = 1000;
                IPEndPoint endPoint = new IPEndPoint(nodeIP, port);

                socket.Connect(endPoint);
                byte[] sBuffer = Encoding.ASCII.GetBytes(input);
                byte[] rBuffer = new byte[readSize];
                retVal = socket.Send(sBuffer, 0, sBuffer.Length, SocketFlags.None);
                retVal = socket.Receive(rBuffer);
                string result = System.Text.Encoding.ASCII.GetString(rBuffer);
                string[] parsedStr = result.Split('\r');
                parsedStr = parsedStr[1].Split(';');

                int parsedValue = 0;
                Int32.TryParse(parsedStr[0], out parsedValue);
                value = parsedValue;
                retVal = 0;
                //int indexOfprefix =result.IndexOf('\r');
                //string prefixRemovedString = result.Remove(0, indexOfprefix + 1);
                //indexOfprefix = prefixRemovedString.IndexOf(';');
                //prefixRemovedString = prefixRemovedString.Remove(indexOfprefix);
            }
            catch (MMCException mcerror)
            {
                LoggerManager.Exception(mcerror);
            }
            catch (Exception err)
            {
                //LoggerManager.Error($String.Format("ReceiveMessage(): Error occurred Receive. Err = {0}", e.Message));
                LoggerManager.Exception(err);

                retVal = -1;
            }
            return retVal;
        }
        public int ReceiveMessage(int nodeNum, String obj, out int value)
        {
            int retVal = -1;
            value = 0;

            try
            {
                for (int retrycount = 0; retrycount <= 5; retrycount++)
                {
                    try
                    {
                        retVal = ElmoReceiveMessage(nodeNum, obj, out value);
                        if (retVal == 0)
                        {
                            break;
                        }
                        else
                        {
                            if (retrycount == 5)
                            {
                                retVal = -1;
                            }
                        }
                    }
                    catch (MMCException mcerror)
                    {
                        if (retrycount < 6)
                        {
                            continue;
                        }
                        else
                        {
                            retVal = -1;
                            LoggerManager.Exception(mcerror);
                        }
                    }
                    catch (Exception err)
                    {
                        if (retrycount < 6)
                        {
                            LoggerManager.Error($"ReceiveMessage Method: {MethodBase.GetCurrentMethod()}, NodeNum:{nodeNum} obj:{obj}");
                            continue;
                        }
                        else
                        {
                            retVal = -1;
                            LoggerManager.Error($"ReceiveMessage Method: {MethodBase.GetCurrentMethod()}, NodeNum:{nodeNum} obj:{obj}");
                            LoggerManager.Exception(err);
                        }
                    }
                    Thread.Sleep(500);
                }

            }
            catch (MMCException mcerror)
            {
                LoggerManager.Error($"ReceiveMessage Method: {MethodBase.GetCurrentMethod()}, NodeNum:{nodeNum} obj:{obj}");
                retVal = -1;
                LoggerManager.Exception(mcerror);
            }
            catch (Exception err)
            {
                LoggerManager.Error($"ReceiveMessage Method: {MethodBase.GetCurrentMethod()}, NodeNum:{nodeNum} obj:{obj}");
                retVal = -1;
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        private int ElmoSendMessage(int nodeNum, string obj, string param)
        {
            int retVal = -1;
            try
            {

                IPAddress rIP, nodeIP;
                IPAddress.TryParse(ElmoDescripterParams.PIP, out rIP);  // P-MAS IP

                byte[] ipArr = rIP.GetAddressBytes();
                ipArr[ipArr.Length - 1] += (byte)(nodeNum);
                string input = obj + "=" + param + ";\n\r";

                nodeIP = new IPAddress(ipArr);
                int port = 5001;
                //UDP Socket 생성
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPEndPoint endPoint = new IPEndPoint(nodeIP, port);

                //바인드
                socket.Connect(endPoint);
                //데이터 입력

                //인코딩(byte[])
                byte[] sBuffer = Encoding.ASCII.GetBytes(input);
                //byte[] rbuffer;

                //보내기
                retVal = socket.Send(sBuffer, 0, sBuffer.Length, SocketFlags.None);
                //sBuffer = Encoding.ASCII.GetBytes("OL[15]");
                // retVal = socket.Send(sBuffer, 0, sBuffer.Length, SocketFlags.None);
                // rbuffer = ReadBySize(socket);
                retVal = 0;
                socket.Close();
            }
            catch (MMCException mcerror)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, mcerror);
                LoggerManager.Error($"ElmoSendMessage Method: {MethodBase.GetCurrentMethod()}");
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                LoggerManager.Error($"ElmoSendMessage Method: {MethodBase.GetCurrentMethod()}");
                retVal = -1;
            }
            return retVal;
        }
        public int SendMessage(int nodeNum, String obj, String param)
        {
            int retVal = -1;

            try
            {
                for (int retrycount = 0; retrycount <= 5; retrycount++)
                {
                    try
                    {
                        retVal = ElmoSendMessage(nodeNum, obj, param);
                        if (retVal == 0)
                        {
                            retVal = ValidationSendMessage(nodeNum, obj, param);
                            if (retVal == 0)
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (retrycount == 5)
                            {
                                retVal = -1;
                            }
                        }
                    }
                    catch (MMCException mcerror)
                    {
                        LoggerManager.Error($"SendMessage Method: {MethodBase.GetCurrentMethod()}, NodeNum:{nodeNum}  RetryCount:{retrycount}");
                        if (retrycount < 6)
                        {
                            LoggerManager.Debug($"SendMessage Method: {MethodBase.GetCurrentMethod()}, NodeNum:{nodeNum}  RetryCount:{retrycount}");
                            continue;
                        }
                        else
                        {
                            LoggerManager.Debug($"SendMessage Method: {MethodBase.GetCurrentMethod()}, NodeNum:{nodeNum}  RetryCount:{retrycount}");
                            retVal = -1;
                            LoggerManager.Exception(mcerror);
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Error($"SendMessage Method: {MethodBase.GetCurrentMethod()}, NodeNum:{nodeNum} RetryCount:{retrycount}");
                        if (retrycount < 6)
                        {
                            LoggerManager.Debug($"SendMessage Method: {MethodBase.GetCurrentMethod()}, NodeNum:{nodeNum}  RetryCount:{retrycount}");
                            continue;
                        }
                        else
                        {
                            LoggerManager.Debug($"SendMessage Method: {MethodBase.GetCurrentMethod()}, NodeNum:{nodeNum}  RetryCount:{retrycount}");
                            retVal = -1;
                            LoggerManager.Exception(err);
                        }
                    }
                    Thread.Sleep(500);
                }

            }
            catch (MMCException mcerror)
            {
                LoggerManager.Exception(mcerror);
            }
            catch (Exception err)
            {
                //LoggerManager.Error($String.Format("ReceiveMessage(): Error occurred Receive. Err = {0}", e.Message));
                LoggerManager.Exception(err);
                retVal = -1;
            }

            return retVal;
        }
        private int ValidationSendMessage(int nodeNumber, string obj, string param)
        {
            int retVal = -1;
            int receivedata = -1;
            try
            {
                retVal = ReceiveMessage(nodeNumber, obj, out receivedata);
                if (retVal == 0 && receivedata.ToString() == param)
                {
                    retVal = 0;
                }
                else
                {
                    LoggerManager.Error($"ValidationSendMessage(): different of send data:{param} and receivedata:{receivedata}");
                    retVal = -1;
                }
            }
            catch (MMCException mcerror)
            {
                LoggerManager.Exception(mcerror);
                retVal = -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = -1;
            }
            return retVal;
        }


        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                if (Initialized == false)
                {
                    retval = LoadSysParameter();

                    InitFlag = false;

                    retval = InitializeController();

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
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
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

                IParam tmpParam = null;
                tmpParam = new ElmoDescripterParam();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(ElmoDescripterParam));

                if (RetVal == EventCodeEnum.NONE)
                {
                    ElmoDescripterParams = tmpParam as ElmoDescripterParam;
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
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                RetVal = this.SaveParameter(ElmoDescripterParams);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
    }
}
