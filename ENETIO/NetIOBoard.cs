using System.Net.Sockets;
using System.Net;
using System;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;

namespace ENETIO
{
    public class NetIOSocketHandler
    {
        private const string localaddrprefix = "192.168.0.";
        private string value;
        private string key;

        private static Socket socket;
        private IPAddress ipaddr;
        public string IPAddr
        {
            get { return ipaddr.ToString(); }
        }
        private bool connected = false;
        public bool Connected
        {
            get { return connected; }
        }

        const int REMOTE_PORT = 2005;
        const int BUFSIZE = 2048;

        private readonly byte[] TX_BUF = {
            (byte)'E',(byte)'M',(byte)'0',(byte)'T',(byte)'I',(byte)'O',(byte)'N',(byte)'T',(byte)'E',(byte)'K', // Company ID : "EMOTIONTEX"
            72,0,								        // ModuleInfo : 0x0048 (fixed)
            0,0,								        // Source of Frame : 0x0000 (fixed)
            0,0,								        // Send WaitTime : 0x0000
            0,0,								        // Output TimeOUt : 0x0000
            1,0,								        // Invoke ID : 0x0001
            50,0,								        // Frame Total Length : 0x0032 (fixed)
            0,0,								        // CheckSum : 0x0000 : (must be calculation)
            0,0,								        // Command : 0x0000 (Read/Write), 0x0001(Read), 0x0002(Write)
            2,0,								        // Data Type (Word) : 0x0002 (fixed)
            4,0,								        // Data Size (4Word) : 0x0004 (fixed)
            0,0,								        // Reserved-0 : 0x0000
            0,0,								        // Reserved-1 : 0x0000
            (byte)'0',(byte)'0',(byte)'0',(byte)'0',	// Output Data[0] ; P1,P0
            (byte)'0',(byte)'0',(byte)'0',(byte)'0',	// Output Data[1] : P3,P2
            (byte)'0',(byte)'0',(byte)'0',(byte)'0',	// Output Data[2] : P5,P4
            (byte)'0',(byte)'0',(byte)'0',(byte)'0' 	// Output Data[3] : Non
        }; // 50 bytes
        private readonly byte[] TX_BUF_LIGHT = {
            (byte)'E',(byte)'M',(byte)'0',(byte)'T',(byte)'I',(byte)'O',(byte)'N',(byte)'T',(byte)'E',(byte)'K', // Company ID : "EMOTIONTEX"
            72,0,                                       // ModuleInfo : 0x0048 (fixed)
            0,0,                                        // Source of Frame : 0x0000 (fixed)
            0,0,                                        // Send WaitTime : 0x0000
            0,0,                                        // Output TimeOUt : 0x0000
            1,0,                                        // Invoke ID : 0x0001
            50,0,                                       // Frame Total Length : 0x0032 (fixed)
            0,0,                                        // CheckSum : 0x0000 : (must be calculation)
            0,0,                                        // Command : 0x0000 (Read/Write), 0x0001(Read), 0x0002(Write)
            2,0,                                        // Data Type (Word) : 0x0002 (fixed)
            4,0,                                        // Data Size (4Word) : 0x0004 (fixed)
            0,0,                                        // Reserved-0 : 0x0000
            0,0,                                        // Reserved-1 : 0x0000
            (byte)'0',(byte)'0',(byte)'0',(byte)'0',    // Output Data[0] ; P1,P0
            (byte)'0',(byte)'0',(byte)'0',(byte)'0',    // Output Data[1] : P3,P2
            (byte)'0',(byte)'0',(byte)'0',(byte)'0',    // Output Data[2] : P5,P4
            (byte)'0',(byte)'0',(byte)'0',(byte)'0',    // Output Data[3] : Non
            0,64,3,4,                                   // mod by Wayne 130827
            0,(byte)'0',(byte)'0',(byte)'0',(byte)'0',
            0,(byte)'0',(byte)'0',(byte)'0',(byte)'0',
            0,(byte)'0',(byte)'0',(byte)'0',(byte)'0'
        }; // 54 + {DataSize} * {DataLength} bytes

        private readonly byte[] SET_1_BITMASK = new byte[8] { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };
        private readonly byte[] SET_0_BITMASK = new byte[8] { 0xFE, 0xFD, 0xFB, 0xF7, 0xEF, 0xDF, 0xBF, 0x7F };

        private byte[] AxBuf_O; // Output Data Buffer
        private byte[] AxBuf_I; // Input Data Buffer

        public NetIOSocketHandler()
        {
            AxBuf_O = new byte[6]; // CH 0~5
            AxBuf_I = new byte[6]; // CH 0~5
        }

        public NetIOSocketHandler(string ip)
        {
            ipaddr = IPAddress.Parse(ip);

            AxBuf_O = new byte[6]; // CH 0~5
            AxBuf_I = new byte[6]; // CH 0~5
        }

        /// <summary>
        /// Ini 에 있는 보드 정보를 가지고 초기화.
        /// </summary>
        /// <param name="key">DEV1</param>
        /// <param name="value">201</param>
        public NetIOSocketHandler(string key, string value)
        {
            this.key = key;
            this.value = localaddrprefix + value;

            ipaddr = IPAddress.Parse(this.value);

            AxBuf_O = new byte[6]; // CH 0~5
            AxBuf_I = new byte[6]; // CH 0~5
        }

        #region Public Method
        public bool OpenDevice()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ipEndPoint = new IPEndPoint(ipaddr, REMOTE_PORT);
            socket.Connect(ipEndPoint);

            // 소켓 연결이 안되었을 때 예외처리
            if (socket.Connected == false)
            {
                Console.WriteLine("Socket Connection Error!");
                return false;
            }

            // Board에 전원이 켜져 있는지 확인하기 위해 간단한 패킷을 전송하고 응답을 확인
            byte[] txbuf = new byte[50];
            byte[] rxbuf = new byte[50];
            Array.Copy(TX_BUF, txbuf, 50);
            txbuf[24] = 1; // Read CMD

            if (SendPacket(txbuf, rxbuf))
            {
                connected = true;
                return true;
            }
            else
            {
                Console.WriteLine("Unable to communicate with the board.");
                socket.Close();
                connected = false;
                return false;
            }
        }

        public bool OpenDevice(IPAddress ipname)
        {
            ipaddr = ipname;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ipEndPoint = new IPEndPoint(ipaddr, REMOTE_PORT);
            socket.Connect(ipEndPoint);

            // 소켓 연결이 안되었을 때 예외처리
            if (socket.Connected == false)
            {
                Console.WriteLine("Socket Connection Error!");
                return false;
            }

            // Board에 전원이 켜져 있는지 확인하기 위해 간단한 패킷을 전송하고 응답을 확인
            byte[] txbuf = new byte[50];
            byte[] rxbuf = new byte[50];
            Array.Copy(TX_BUF, txbuf, 50);
            txbuf[24] = 1; // Read CMD

            if (SendPacket(txbuf, rxbuf))
            {
                connected = true;
                return true;
            }
            else
            {
                Console.WriteLine("Unable to communicate with the board.");
                socket.Close();
                connected = false;
                return false;
            }
        }

        public void CloseDevice()
        {
            if (socket.Connected == true)
            {
                socket.Close();
            }
            connected = false;
        }

        public bool Write_Data_IO(int ch, int port, bool val)
        {
            if (connected == true)
            {
                if (val == true)
                    AxBuf_O[ch] |= SET_1_BITMASK[port];
                else
                    AxBuf_O[ch] &= SET_0_BITMASK[port];

                return NetOutPort(ch, AxBuf_O[ch]) == 0 ? true : false;
            }
            else
            {
                return false;
            }
        }

        public bool Read_Data_IO(int ch, int port, ref bool val)
        {
            if (connected == true)
            {
                if (NetInPortAll() == 0)
                {
                    val = (AxBuf_I[ch] & SET_1_BITMASK[port]) == 0 ? false : true;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool Read_Data_IO(int ch, ref byte port_byte)
        {
            if (connected == true)
            {
                if (NetInPortAll() == 0)
                {
                    port_byte = AxBuf_I[ch];
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool Read_Data_IO_All(ref byte[] port_bytes)
        {
            if (connected == true)
            {
                if (NetInPortAll() == 0)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        port_bytes[i] = AxBuf_I[i];
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool Write_Data_Light(int ch, ushort val)
        {
            if (connected == true)
            {
                // 팀장님 상의해봐야함.
                // DATA, CLK, LOAD, S0, S1, S2 정보가 필요함. 현재 정보 하드 코딩 되어있음.
                return NetLightControl(ch, val) == 0 ? true : false;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region Private Method
        private byte HexToAsc(ushort HEX)
        {
            if (HEX >= 10 && HEX <= 15)
                return (byte)(HEX + 55);
            else
                return (byte)(HEX + 48);
        }

        private byte AscToHex(byte data_H, byte data_L)
        {
            byte tmp = 0;

            if (data_H >= 'A' && data_H <= 'F')
                tmp = (byte)(ushort)((data_H - 55) * 16);
            else
                tmp = (byte)(ushort)((data_H - 48) * 16);

            if (data_L >= 'A' && data_L <= 'F')
                tmp = (byte)(tmp + (data_L - 55));
            else
                tmp = (byte)(tmp + (data_L - 48));

            return tmp;
        }

        private void CheckSumArg(byte[] csbuf, uint num)
        {
            uint sum = 0;
            csbuf[22] = csbuf[23] = 0;
            for (uint i = 0; i < num; i++)
                sum += csbuf[i];
            csbuf[22] = (byte)(sum & 0xFF);
            csbuf[23] = (byte)((sum >> 8) & 0xFF);
        }

        private byte Get_PortInfo(string iomap)
        {
            // 팀장님 상의해봐야함.
            // Light 제어하기 위해 Port 정보를 가지고 오는 함수. (Ex: 4P0B)
            byte[] data = new byte[4];
            byte[] temp = new byte[2];

            data = Encoding.ASCII.GetBytes(iomap);
            temp[0] = (byte)(data[0] - 48); // ASCII to HEX
            temp[1] = (byte)(data[2] - 48);

            // Port(3 bits) + Bit(2 bits) + Out/In(1 bit) + Set/Rst(1 bit)
            return (byte)((temp[0] << 5) + (temp[1] << 2));
        }

        private void ReadDummyData(IPEndPoint addr)
        {
            int nRecvLen;
            byte[] buffer = new byte[BUFSIZE];
            EndPoint _addr = (EndPoint)addr;

            for (int i = 0; i < 10; i++)
            {
                if (socket.Poll(100, SelectMode.SelectRead))
                {
                    nRecvLen = socket.ReceiveFrom(buffer, ref _addr);
                    if (nRecvLen == 0)
                        break;
                    else
                        Console.WriteLine("Read Dummy Count! (" + nRecvLen + "byte)");
                }
                else
                {
                    break;
                }
            }
        }

        private int NetOutPort(int ch, byte data)
        {
            byte[] txbuf = new byte[50];
            byte[] rxbuf = new byte[50];

            byte data_H = HexToAsc((ushort)((data >> 4) & 0x0F));
            byte data_L = HexToAsc((ushort)(data & 0x0F));

            Array.Copy(TX_BUF, txbuf, 50);
            txbuf[24] = 2; // Write CMD
            txbuf[32] = (byte)ch;

            switch (ch)
            {
                case 0: txbuf[36] = data_H; txbuf[37] = data_L; break;
                case 1: txbuf[34] = data_H; txbuf[35] = data_L; break;
                case 2: txbuf[40] = data_H; txbuf[41] = data_L; break;
                case 3: txbuf[38] = data_H; txbuf[39] = data_L; break;
                case 4: txbuf[44] = data_H; txbuf[45] = data_L; break;
                case 5: txbuf[42] = data_H; txbuf[43] = data_L; break;
            }

            return SendPacket(txbuf, rxbuf) == true ? 0 : 1;
        }

        private int NetInPortAll()
        {
            byte[] txbuf = new byte[50];
            byte[] rxbuf = new byte[50];

            Array.Copy(TX_BUF, txbuf, 50);
            txbuf[24] = 1; // Read CMD

            bool rc = SendPacket(txbuf, rxbuf);
            int result;

            if (rc == true)
            {
                AxBuf_I[0] = (byte)(AscToHex(rxbuf[36], rxbuf[37]) & 0xFF);
                AxBuf_I[1] = (byte)(AscToHex(rxbuf[34], rxbuf[35]) & 0xFF);
                AxBuf_I[2] = (byte)(AscToHex(rxbuf[40], rxbuf[41]) & 0xFF);
                AxBuf_I[3] = (byte)(AscToHex(rxbuf[38], rxbuf[39]) & 0xFF);
                AxBuf_I[4] = (byte)(AscToHex(rxbuf[44], rxbuf[45]) & 0xFF);
                AxBuf_I[5] = (byte)(AscToHex(rxbuf[42], rxbuf[43]) & 0xFF);
                result = 0;
            }
            else
            {
                result = 1;
            }

            return result;
        }

        private int NetLightControl(int ch, ushort data)
        {
            byte[] txbuf = new byte[1024];
            byte[] rxbuf = new byte[1024];

            int[] bit = new int[12];
            int[] tmpData = new int[3];
            int ibase;

            if (data > 0x0FFF) data = 0x0FFF;

            // Fix for CS0233 and CS0246
            Array.Copy(TX_BUF_LIGHT, txbuf, TX_BUF_LIGHT.Length);

            txbuf[24] = 4; // CMD: Port Control Mode
            txbuf[52] = 6; // Control Bit Number (MAX 48 bits)
            txbuf[53] = 4; // Data Length (MAX 8 bytes)
            txbuf[20] = (byte)(54 + (txbuf[52] * (txbuf[53] + 1))); // Total Data Length

            for (int i = 0; i < 12; i++)
            {
                bit[i] = (data >> i) & 0x01;
                switch (i / 4)
                {
                    case 0:
                        tmpData[0] = tmpData[0] | bit[i] << (2 * (i % 4));
                        tmpData[0] = tmpData[0] | bit[i] << (2 * (i % 4) + 1);
                        break;
                    case 1:
                        tmpData[1] = tmpData[1] | bit[i] << (2 * (i % 4));
                        tmpData[1] = tmpData[1] | bit[i] << (2 * (i % 4) + 1);
                        break;
                    case 2:
                        tmpData[2] = tmpData[2] | bit[i] << (2 * (i % 4));
                        tmpData[2] = tmpData[2] | bit[i] << (2 * (i % 4) + 1);
                        break;
                }
            }

            short[] tmp = new short[3];
            byte[] iomap = new byte[4];

            // Data Signal Set
            ibase = 54;
            txbuf[ibase] = Get_PortInfo("4P1B");
            txbuf[ibase + 1] = (byte)((tmpData[2] >> 4) & 0x0F);
            txbuf[ibase + 2] = (byte)(((tmpData[2] << 4) & 0xF0) | ((tmpData[1] >> 4) & 0x0F));
            txbuf[ibase + 3] = (byte)(((tmpData[1] << 4) & 0xF0) | ((tmpData[0] >> 4) & 0x0F));
            txbuf[ibase + 4] = (byte)((tmpData[0] << 4) & 0xF0);

            // Clk Signal Set
            ibase = ibase + txbuf[53] + 1;
            txbuf[ibase] = Get_PortInfo("4P0B");
            txbuf[ibase + 1] = 0x0A;
            txbuf[ibase + 2] = 0xAA;
            txbuf[ibase + 3] = 0xAA;
            txbuf[ibase + 4] = 0xA0;

            // Load Signal Set
            ibase = ibase + txbuf[53] + 1;
            txbuf[ibase] = Get_PortInfo("4P2B");
            txbuf[ibase + 1] = 0xFF;
            txbuf[ibase + 2] = 0xFF;
            txbuf[ibase + 3] = 0xFF;
            txbuf[ibase + 4] = 0xF7;

            // MUX Select signal 
            byte S0 = 0, S1 = 0, S2 = 0;
            switch (ch)
            {
                case 0: S0 = S1 = S2 = 0x00; break;
                case 1: S0 = 0xFF; S1 = S2 = 0x00; break;
                case 2: S0 = S2 = 0x00; S1 = 0xFF; break;
                case 3: S0 = S1 = 0xFF; S2 = 0x00; break;
                case 4: S2 = 0xFF; S0 = S1 = 0x00; break;
                case 5: S0 = S2 = 0xFF; S1 = 0x00; break;
                case 6: S1 = S2 = 0xFF; S0 = 0x00; break;
                case 7: S0 = S1 = S2 = 0xFF; break;
            }

            // S0
            ibase = ibase + txbuf[53] + 1;
            txbuf[ibase] = Get_PortInfo("4P5B");
            txbuf[ibase + 1] = txbuf[ibase + 2] = txbuf[ibase + 3] = txbuf[ibase + 4] = S0;

            // S1
            ibase = ibase + txbuf[53] + 1;
            txbuf[ibase] = Get_PortInfo("4P4B");
            txbuf[ibase + 1] = txbuf[ibase + 2] = txbuf[ibase + 3] = txbuf[ibase + 4] = S1;

            // S2
            ibase = ibase + txbuf[53] + 1;
            txbuf[ibase] = Get_PortInfo("4P3B");
            txbuf[ibase + 1] = txbuf[ibase + 2] = txbuf[ibase + 3] = txbuf[ibase + 4] = S2;

            //CheckSum_Light(txbuf);
            CheckSumArg(txbuf, txbuf[20]);

            return SendPacket_Light(txbuf, rxbuf) == true ? 0 : 1;
        }

        private bool SendPacket(byte[] txbuf, byte[] rxbuf)
        {
            IPEndPoint addr = new IPEndPoint(ipaddr, REMOTE_PORT);

            ReadDummyData(addr); // For fix packet error
            CheckSumArg(txbuf, 50);

            int SendSize = socket.SendTo(txbuf, addr);
            if (SendSize == (int)SocketError.SocketError)
            {
                int wRet = (int)SocketError.SocketError;
                Console.WriteLine("sendto error! SendPacket(), Error Code={0}", wRet);
                return false;
            }

            socket.ReceiveTimeout = 100; // Set receive timeout to 1000 milliseconds
            try
            {
                EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                int readbytes = socket.ReceiveFrom(rxbuf, ref remoteEP);
                if (readbytes != 50)
                {
                    int wRet = (int)SocketError.SocketError;
                    Console.WriteLine("Packet Error in SendPacket() readbytes = {0}, Error Code={1}", readbytes, wRet);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine("TimeOut in SendPacket() - Exception: {0}", ex.Message);
                return false;
            }
        }

        private bool SendPacket_Light(byte[] txbuf, byte[] rxbuf)
        {
            IPEndPoint addr = new IPEndPoint(ipaddr, REMOTE_PORT);
            ReadDummyData(addr); // For fix packet error

            int SendSize = socket.SendTo(txbuf, txbuf[20], SocketFlags.None, addr); // txbuf[20] = Frame Total Length

            if (SendSize == (int)SocketError.SocketError)
            {
                int wRet = (int)SocketError.SocketError;
                Console.WriteLine("sendto error! SendPacket_Light(), Error Code={0}", wRet);
                return false;
            }

            socket.ReceiveTimeout = 100; // Set receive timeout to 1000 milliseconds
            try
            {
                EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                int readbytes = socket.ReceiveFrom(rxbuf, ref remoteEP);
                if (readbytes != txbuf[20])
                {
                    int wRet = (int)SocketError.SocketError;
                    Console.WriteLine("Packet Error in SendPacket_Light() readbytes = {0}, Error Code={1}", readbytes, wRet);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine("TimeOut in SendPacket_Light() - Exception: {0}", ex.Message);
                return false;
            }
        }
        #endregion
    }
}