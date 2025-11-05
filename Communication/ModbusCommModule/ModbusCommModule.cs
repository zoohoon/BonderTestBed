using EasyModbus;
using LogModule;
using ProberErrorCode;
using ProberInterfaces.Communication;
using ProberInterfaces.Enum;
using ProberInterfaces.RFID;
using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace Communication.ModbusCommModule
{
    public partial class ModbusCommModule : ICommModule, INotifyPropertyChanged
    {
        #region ==> NotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        readonly private string RegisterNum_TAGID = "A000";
        readonly private string WordCount_TAGID = "0004";
        readonly private string RegisterNum_ASCII = "0004";
        readonly private string WordCount_ASCII = "0001";

        public event setDataHandler SetDataChanged;        

        public ModbusCommState modbusCommState = null;
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

        private string _RegisterNum;
        public string RegisterNum
        {
            get { return _RegisterNum; }
            set { _RegisterNum = value; }
        }

        private string _WordCount;
        public string WordCount
        {
            get { return _WordCount; }
            set { _WordCount = value; }
        }

        private EnumRFIDModbusReadDataType _RFIDReadDataType;
        public EnumRFIDModbusReadDataType RFIDReadDataType
        {
            get { return _RFIDReadDataType; }
            set { _RFIDReadDataType = value; }
        }


        public ModbusCommModule(string ip, int port, EnumRFIDModbusReadDataType rfidReadDataType, string registerNum, string wordCount)
        {
            IP = ip;
            Port = port;
            RFIDReadDataType = rfidReadDataType;
            RegisterNum = registerNum;
            WordCount = wordCount;
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
                retVal = modbusCommState.Connect(IP, Port);
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
                modbusCommState?.Disconnect();
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
                    modbusCommState.Disconnect();
                    IsDisposed = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"RFIDCommModule.Dispose() Error occurred. Err = {err.Message}");
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
                    modbusCommState?.Disconnect();
                    modbusCommState = new ModbusDisconnectState(this);
                    Initialized = true;
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.UNDEFINED;
            }

            return retVal;
        }
        public void DeInitModule()
        {
            Dispose();
        }
        public EnumCommunicationState GetCommState()
            => this.modbusCommState?.GetCommunicationState() ?? EnumCommunicationState.DISCONNECT;

        public void CommStateTransition(ModbusCommState state)
        {
            try
            {
                if (modbusCommState?.GetType() != state?.GetType())
                {
                    LoggerManager.Debug($"[RFIDCommModule_Modbus] CommStateTransition() Old = {modbusCommState.GetType()}, New = {state.GetType()}");
                    this.modbusCommState = state;
                    CurState = this.modbusCommState.GetCommunicationState();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public IPStatus PingTest(string address)
            => this.ping.Send(address)?.Status ?? IPStatus.Unknown;

        public void Send(string sendData)
        {
            try
            {
                switch (RFIDReadDataType)
                {
                    case EnumRFIDModbusReadDataType.TAGID:
                        modbusCommState?.SendQuery(RegisterNum_TAGID, WordCount_TAGID);
                        break;
                    case EnumRFIDModbusReadDataType.ASCII:
                        modbusCommState?.SendQuery(RegisterNum_ASCII, WordCount_ASCII, true);
                        break;
                    case EnumRFIDModbusReadDataType.CUSTOM:
                        modbusCommState?.SendQuery(RegisterNum, WordCount);
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

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

        public EventCodeEnum ReInitalize(bool attattch = true)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (modbusCommState.GetCommunicationState() == EnumCommunicationState.CONNECTED)
                {
                    DisConnect();
                }
                if (modbusCommState.GetCommunicationState() == EnumCommunicationState.DISCONNECT)
                {
                    retVal = Connect();
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

    public abstract class ModbusCommState
    {
        public ModbusCommModule CommModule;
        public ModbusCommState(ModbusCommModule commModule)
        {
            this.CommModule = commModule;
        }
        public abstract void Disconnect();
        public abstract EnumCommunicationState GetCommunicationState();
        public abstract EventCodeEnum Connect(string address, int port);
        public abstract ModbusClient modbusClient { get; set; }
        public abstract void SendQuery(string registerNum, string wordCount, bool convertASCII = false);
    }
    public sealed class ModbusDisconnectState : ModbusCommState
    {
        public ModbusDisconnectState(ModbusCommModule commModule) : base(commModule)
        {
        }

        public override ModbusClient modbusClient { get; set; }

        public override EventCodeEnum Connect(string address, int port)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                try
                {
                    var pingStatus = this.CommModule.PingTest(address);

                    if (pingStatus == IPStatus.Success)
                    {
                        ModbusConnect();
                    }
                    else
                    {
                        //Ping Retry
                        pingStatus = this.CommModule.PingTest(address);
                        if (pingStatus == IPStatus.Success)
                        {
                            ModbusConnect();
                        }
                        else
                        {
                            retVal = EventCodeEnum.UNDEFINED;
                        }
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    //Ping Retry
                    var pingStatus = this.CommModule.PingTest(address);
                    if (pingStatus == IPStatus.Success)
                    {
                        ModbusConnect();
                    }
                    else
                    {
                        retVal = EventCodeEnum.UNDEFINED;
                    }
                }

                void ModbusConnect()
                {
                    ModbusClient HBClient = new ModbusClient(address, port) { UnitIdentifier = 0xFF };
                    HBClient.Connect(address, port);

                    if (HBClient?.Connected == true)
                    {
                        this.CommModule.CommStateTransition(new ModbusConnectState(this.CommModule) { modbusClient = HBClient });
                        retVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        HBClient.Connect(address, port);
                        if (HBClient?.Connected == true)
                        {
                            this.CommModule.CommStateTransition(new ModbusConnectState(this.CommModule) { modbusClient = HBClient });
                            retVal = EventCodeEnum.NONE;
                        }
                        else
                        {
                            retVal = EventCodeEnum.UNDEFINED;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                Disconnect();
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.UNDEFINED;
            }

            return retVal;
        }

        public override void Disconnect()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override EnumCommunicationState GetCommunicationState()
            => EnumCommunicationState.DISCONNECT;

        public override void SendQuery(string registerNum, string wordCount, bool convertASCII = false)
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
    public sealed class ModbusConnectState : ModbusCommState
    {
        public ModbusConnectState(ModbusCommModule commModule) : base(commModule)
        {
        }

        private readonly int ConvertStringHex = 16;
        public override ModbusClient modbusClient { get; set; }
        private object lockObject = new object();

        public override EventCodeEnum Connect(string address, int port)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            return retVal;
        }

        public override void Disconnect()
        {
            try
            {
                modbusClient?.Disconnect();
                modbusClient = null;
                this.CommModule.CommStateTransition(new ModbusDisconnectState(this.CommModule));
                LoggerManager.Debug($"[RFIDCommModule_Modbus] Disconnect()");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override EnumCommunicationState GetCommunicationState()
            => EnumCommunicationState.CONNECTED;

        public override void SendQuery(string registerNum, string wordCount, bool convertASCII = false)
        {
            int[] receivedData;

            try
            {
                int sendRegisterNum = Convert.ToInt32(registerNum, ConvertStringHex);
                int sendWordCount = Convert.ToInt32(wordCount, ConvertStringHex);

                receivedData = ReadHoldingRegister(sendRegisterNum, sendWordCount);
                if (receivedData == null)
                {
                    LoggerManager.Error($"[RFIDCommModule_Modbus] ReadHoldingRegister fail. Received Data is null");
                    return;
                }

                string parsedReceivedData = "";
                foreach (var curData in receivedData)
                {
                    parsedReceivedData += curData.ToString("X4");
                }

                if (convertASCII)
                {
                    // 1. 맨 앞 2자리 숫자가 Data의 길이를 의미하므로 해당 길이만큼 Data를 다시 읽어온다.
                    string receivedDataLengthString = parsedReceivedData.Substring(0, 2); // Data의 길이를 의미하는 문자열 파싱
                    int sendFourBitCount = Convert.ToInt32(receivedDataLengthString) + 2; // 읽어야할 Data의 길이를 의미하는 변수 (단위 : 4bit)
                    sendWordCount = (int)Math.Ceiling((double)sendFourBitCount / 4); // 최종적으로 ReadHoldingRegister 함수를 통해 읽어야 할 Data의 길이 (단위 : 2byte)

                    receivedData = ReadHoldingRegister(sendRegisterNum, sendWordCount);
                    if (receivedData == null)
                    {
                        LoggerManager.Error($"[RFIDCommModule_Modbus] ReadHoldingRegister fail. Received Data is null");
                        return;
                    }

                    parsedReceivedData = "";
                    foreach (var curData in receivedData)
                    {
                        parsedReceivedData += curData.ToString("X4");
                    }

                    // 2. 다시 읽어온 Data에서 맨 마지막에 의미없는 데이터가 있는 경우가 있으므로 정확한 Data의 길이만큼 자른다.
                    string asciiHexString = parsedReceivedData.Substring(2, sendFourBitCount - 2);


                    // 3. 잘라낸 Data가 Hex String 형태이므로 ASCII 코드로 변환하여 해당 문자열로 만든다.
                    parsedReceivedData = "";
                    for (int index = 0; index < asciiHexString.Length; index += 2)
                    {
                        string singleChar = asciiHexString.Substring(index, 2);
                        int askiiValue = Convert.ToInt32(singleChar, 16);
                        parsedReceivedData += char.ConvertFromUtf32(askiiValue);
                    }
                }

                CommModule.SetReceivedData(parsedReceivedData);
            }
            catch (Exception err)
            {
                LoggerManager.Error($"[RFIDCommModule_Modbus] Error occurred. Err = {err.Message}");
            }
        }

        private int[] ReadHoldingRegister(int registerNum, int wordCount)
        {
            lock (lockObject)
            {
                try
                {
                    var pingTestStatus = this.CommModule.PingTest(modbusClient?.IPAddress ?? "0");
                    if (pingTestStatus == IPStatus.Success)
                    {
                        int[] result = modbusClient?.ReadHoldingRegisters(registerNum, wordCount) ?? null;
                        if (result == null)
                        {
                            //Retry
                            result = modbusClient?.ReadHoldingRegisters(registerNum, wordCount) ?? null;
                            if (result == null)
                                throw new Exception($"[RFIDCommModule_Modbus] ReadHoldingRegister() : Fail Get Data. (RegisterNum : {registerNum})");
                        }
                        return result;
                    }
                    else
                    {
                        throw new Exception($"[RFIDCommModule_Modbus] ReadHoldingRegister() : Fail Ping Test. Check connect the RFID Module.");
                    }
                }
                catch (Exception err)
                {
                    throw err;
                }
            }
        }
    }
}
