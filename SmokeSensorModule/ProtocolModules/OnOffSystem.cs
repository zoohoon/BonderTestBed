using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Communication;
using ProberInterfaces.Enum;
using ProberInterfaces.Temperature.EnvMonitoring;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EnvMonitoring.ProtocolModules
{
    public class OnOffSystem : IProtocolModule, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion  
        public byte[] Sendbuff { get; set; }
        public byte[] AlarmResetbuff { get; set; }
        public byte[] SetTempbuff { get; set; }        
        public byte[] SetTempDeviationbuff { get; set; }       
        public bool Initialized { get; set; } = false;

        private ISensorModule _SensorModule;
        public ISensorModule SensorModule
        {
            get { return _SensorModule; }
            set { _SensorModule = value; }
        }
        /* 
        *
        * STX : 0x53
        * Length : CMD ~ DATA 까지의 바이트 수
        * CMD : 통신 명령
        * Code : 명령 구분 
        * Count : 송신 Count (0~255 반복)
        * DATA : ID, 온도, 습도 등 데이터
        * Checksum : length ~ Data 까지의 1Byte 식 합한 값
        * ETX : 0x45        
        * */

        public OnOffSystem(SensorModule sensorModule)
        {
            SensorModule = sensorModule;
        }

        public static byte STX = 0x53;
        public static byte Length_MSB = 0x00;
        public static byte Length_LSB = 0x0C;
        public static byte CMD_MSB = 0x05;
        public static byte CMD_LSB = 0x01;
        public static byte Code = 0x01;
        public static byte Count = 0x00;
        public static byte DATA_ID = 0x01;
        public static byte ETX = 0x45;

        #region <remarks> Functions </remarks>

        public void BuffInit()
        {
            try
            {
                // default // submodule이 만들어야한다. 
                Sendbuff = new byte[]
                {
                    STX,
                    Length_MSB, Length_LSB,
                    CMD_MSB, CMD_LSB,
                    Code,
                    Count,
                    DATA_ID,
                    0x00, 0x00,
                    0x00, 0x00,
                    0x00, 0x00,
                    0x00,
                    0x00, 0x00,
                    ETX
                };
                // alarm reset
                AlarmResetbuff = new byte[]
                {
                    STX ,
                    Length_MSB, Length_LSB,
                    CMD_MSB , 0x03 ,
                    Code, Count,
                    DATA_ID, 0x00, 
                    0x01, 0x00, 
                    0x00, 0x00, 
                    0x00, 0x00,
                    0x00, 0x00,
                    ETX
                  };
                // Set temp (warning , alarm)
                SetTempbuff = new byte[]
                {
                    STX ,
                    Length_MSB, Length_LSB,
                    CMD_MSB, 0x03,
                    0x02, Count,
                    DATA_ID, 0x00,
                    0x00, 0x00,
                    0x00, 0x00,
                    0x00, 0x00,
                    0x00, 0x00,                    
                    ETX
                };
                // Temp deviation
                SetTempDeviationbuff = new byte[]
                {
                    STX ,
                    Length_MSB, Length_LSB,
                    CMD_MSB, 0x03,
                    0x06, Count,
                    DATA_ID, 0x00,
                    0x00, 0x00,
                    0x00, 0x00,
                    0x00, 0x00,
                    0x00, 0x00,                    
                    ETX
                };
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
           
        public void ParseData(byte[] receiveData, ISensorInfo sensorInfo)
        {
            try
            {                                
                if (sensorInfo is SmokeSensorInfo)
                {
                    SmokeSensorInfo sensor = new SmokeSensorInfo();                                                     
                    string msg = BitConverter.ToString(receiveData);
                    string[] hexValuesSplit = msg.Split('-');                    

                    // CR+LF 의 경우 END_CODE 가 USED 설정 시 추가로 송신됨
                    // 정상 명령 응답시, Length = 24 / END_CODE 가 USED 설정 시 CR+LF 포함 = 26
                    // 온도 경고,알람 기준값, 편차 기준값 설정 시, Length = 18  
                    if (receiveData.Length == 24 || receiveData.Length == 26)
                    {                                             
                        string hex_temp = hexValuesSplit[11] + hexValuesSplit[12];
                        sensor.CurTemp.Value = DataParsing(hex_temp);

                        string hex_humi = hexValuesSplit[13] + hexValuesSplit[14];
                        sensor.CurHumi.Value = DataParsing(hex_humi);

                        string hex_warning_temp = hexValuesSplit[15] + hexValuesSplit[16];
                        sensor.WarningTemp.Value = DataParsing(hex_warning_temp);

                        string hex_alarm_temp = hexValuesSplit[17] + hexValuesSplit[18];
                        sensor.AlarmTemp.Value = DataParsing(hex_alarm_temp);

                        string hex_temp_deviation = hexValuesSplit[19] + hexValuesSplit[20];
                        sensor.TempDeviation.Value = DataParsing(hex_temp_deviation);

                        string opState_MSB = hexValuesSplit[8];
                        string opState_MSB_binary = HexToBinary(opState_MSB);
                        string opState_LSB = hexValuesSplit[9]; // "A8" "2C"
                        string opState_LSB_binary = HexToBinary(opState_LSB); // 101100         

                        // Bit 1
                        var smoke_detect = opState_LSB_binary.Substring(opState_LSB_binary.Length - 2, 1);
                        // Bit 2
                        var temp_warning = opState_LSB_binary.Substring(opState_LSB_binary.Length - 3, 1);
                        // Bit 3
                        var temp_alarm = opState_LSB_binary.Substring(opState_LSB_binary.Length - 4, 1);
                        // Bit 7
                        var disconnect_sensor = opState_MSB_binary.Substring(opState_MSB_binary.Length - 8, 1);

                        // Priority : TEMP_ALARM > SMOKE_DETECTED > TEMP_WARN
                        if (temp_warning == "1")
                        {                                                    
                            sensor.bTemp_Warning.Value = true;                            
                        }
                        else
                        {
                            // error reason list에 삭제
                            // 1. 센서에서 reset 한 경우 2. UI에서 reset 한 경우                                                
                            sensor.bTemp_Warning.Value = false;
                        }

                        if (smoke_detect == "1")
                        {
                            sensor.bSmoke_Detect.Value = true;
                        }
                        else
                        {
                            sensor.bSmoke_Detect.Value = false;
                        }

                        if (temp_alarm == "1")
                        {
                            sensor.bTemp_Alarm.Value = true;                            
                        }
                        else
                        {
                            sensor.bTemp_Alarm.Value = false;
                        }

                        if (disconnect_sensor == "1")
                        {
                            sensor.bDisconnect_Sensor.Value = true;                                                               
                        }
                        else
                        {
                            sensor.bDisconnect_Sensor.Value = false;
                        }
                        SensorModule.UpdateSensorInfo(sensor);
                    }                     
                }                                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        /// <summary>
        /// sensor index set 해주는 함수
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="buffer"></param>
        private EventCodeEnum VerifyDataIdFunc(int idx, byte[] buffer)
        {
            byte temp = new byte();
            EventCodeEnum retVal = EventCodeEnum.INVALID_PARAMETER_FIND;
            try
            {
                temp = Convert.ToByte(idx);
                buffer[7] = temp;
                retVal = EventCodeEnum.NONE;                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);                
            }
            return retVal;
        }

        /// <summary>
        /// sensor index get 하는 함수
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public int GetDataIdFunc(byte[] buffer)
        {
            int idx = -1;            
            try
            {
                if (buffer.Length == 24 || buffer.Length == 26)
                {
                    idx = buffer[7];
                }                    
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return idx;
        }

        private string[] ByteToHex(byte[] bytes)
        {
            string[] hexValuesSplit = new string[bytes.Length];
            try
            {
                string hex = BitConverter.ToString(bytes);
                hexValuesSplit = hex.Split('-');                
            }
            catch (Exception err )
            {
                LoggerManager.Exception(err);                
            }
            return hexValuesSplit;
        }

        /// <summary>
        /// Data 받고 checksum 구하는 함수
        /// </summary>
        /// <param name="buffer"></param>
        private EventCodeEnum VerifyCheckSumFunc(byte[] buffer)
        {            
            int sum = 0;
            string[] str = new string[buffer.Length];
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                // Length ~ DATA Sum value
                str = ByteToHex(buffer);
                for (int i = 1; i <= 14; i++)
                {                    
                    sum += Convert.ToInt32(str[i].ToString(), 16);                                       
                }

                // sum : 453 => hex : 1C5 => 2 byte 로 나눠서 01 C5 넣어야한다.
                if (sum > 256)
                {
                    string hexValue = sum.ToString("X2");     // To hex
                    if (hexValue.Length % 2 == 1)
                    {
                        // 2byte 로 값을 나눠주기 위해 4자릿수를 맞춰주기 위함.   
                        hexValue = hexValue.Insert(0, "0");
                    }

                    var val_MSB = hexValue.Substring(hexValue.Length - 4, 2);
                    var val_LSB = hexValue.Substring(hexValue.Length - 2, 2);

                    int checksum_front = Convert.ToInt32(val_MSB.ToString(), 16);
                    int checksum_back = Convert.ToInt32(val_LSB.ToString(), 16);
                    buffer[15] = (byte)checksum_front;
                    buffer[16] = (byte)checksum_back;
                }
                else
                {
                    buffer[15] = 0x00;
                    buffer[16] = (byte)sum;
                }
                
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        /// <summary>
        /// CMD ~ DATA 까지의 바이트 수
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private EventCodeEnum VerifyLengthByteFunc(byte[] buffer)
        {
            int count = 0;
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                // CMD ~ DATA byte sum value
                for (int i = 3; i <= 14; i++)
                {
                    count++;
                }
                buffer[2] = (byte)count;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }               
        
        public EventCodeEnum VerifyData(int idx, byte[] sendBuff)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;            
            try
            {
                retVal = VerifyDataIdFunc(idx, sendBuff);
                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = VerifyCheckSumFunc(sendBuff);
                    if (retVal == EventCodeEnum.NONE)
                    {
                        retVal = VerifyLengthByteFunc(sendBuff);                        
                    }
                    else
                    {
                        LoggerManager.Debug($"[OnOffSystem] Verify CheckSum Fail.");
                    }
                }
                else
                {
                    LoggerManager.Debug($"[OnOffSystem] Verify DataId Fail.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }        

        #endregion

        #region <remarks> InitModule & DeInitModule </remarks>
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                if (Initialized == false)
                {
                    // serial 인지 판단하는 로직 추가할예정 TODO
                    BuffInit();                                    
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

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion        

        #region <remarks> Data Parsing Functions </remarks>
        private float DataParsing(string receiveData)
        {
            float floatVal = 0;
            try
            {
                int intVal = int.Parse(receiveData, System.Globalization.NumberStyles.HexNumber);
                floatVal = (float)intVal / 100;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return floatVal;
        }
        private string HexToBinary(string hexValue)
        {
            string binaryval = "";
            try
            {
                binaryval = Convert.ToString(Convert.ToInt32(hexValue, 16), 2).PadLeft(8, '0');
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return binaryval;
        }
        #endregion
    }
}
