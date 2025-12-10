using Autofac;
using Configurator;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
////using ProberInterfaces.ThreadSync;

namespace ENETIO
{
    public class ENETIOProvider : IIOBase, ILightDeviceControl, ICameraChannelControl, INotifyPropertyChanged, IHasSysParameterizable
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private static int ByteCountPerModule = 1; // 하나의 IO Module당 몇 Byte의 데이터를 가지고 있는지를 의미하는 상수
        private int _netIONodesCount = 0;
        private int DIGITAL_OUTPUT = 8;
        private int _ConnHndl;
        private bool bStopUpdateThread;
        private bool _UseMemoryUpdate;

        //Board-Port-Bit
        //Node-Channel-Port

        //Dictionary<string, NetIOSocketControl> _netIONodes = new Dictionary<string, NetIOSocketControl>();
        List<NetIOSocketHandler> _netIONodes = new List<NetIOSocketHandler>();
        ENETIODescripter EnetIODesc;

        Thread UpdateThread;

        private short _DeviceNumber;
        public short DeviceNumber
        {
            get { return _DeviceNumber; }
            set
            {
                if (value != _DeviceNumber)
                {
                    _DeviceNumber = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ObservableCollection<Channel> _Channels;
        public ObservableCollection<Channel> Channels
        {
            get { return _Channels; }
            set
            {
                if (value == _Channels) return;
                _Channels = value;
                RaisePropertyChanged();
            }
        }

        //ushort[] nNodeList;

        private bool _DevConnected;
        public bool DevConnected
        {
            get { return _DevConnected; }
            set { _DevConnected = value; }
        }

        public ENETIOProvider()
        {
            try
            {
                _UseMemoryUpdate = false;

                //#region opus net io설정파일 읽어서 적용하기
                //string netIOSettingPath = "c:\\PROBERFILES\\Parameter\\IOMap.ini";
                //string setting = System.IO.File.ReadAllText(netIOSettingPath);
                //var settingIni = ReadIniFile(setting);

                //if (settingIni.ContainsKey("Address"))
                //{
                //    foreach (var pair in settingIni["Address"])
                //    {
                //        if (pair.Key.Contains("Num"))
                //            continue;

                //        _netIONodes.Add(pair.Key, new NetIOSocketControl(pair.Key, pair.Value));
                //    }
                //}
                //#endregion

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        /// <summary>
        /// readini 
        /// </summary>
        /// <param name="fileString"></param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, string>> ReadIniFile(string fileString)
        {
            Dictionary<string, Dictionary<string, string>> iniData = new Dictionary<string, Dictionary<string, string>>();
            string[] lines = fileString.Split('\n');

            string currentSection = null;

            foreach (var line in lines)
            {
                string trimmedLine = line.Trim();

                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith(";") || trimmedLine.StartsWith("#"))
                {
                    // 빈 줄 또는 주석은 무시
                    continue;
                }

                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    // 섹션 변경
                    currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
                    iniData[currentSection] = new Dictionary<string, string>();
                }
                else
                {
                    // 키-값 쌍 추가
                    int equalsIndex = trimmedLine.IndexOf('=');
                    if (equalsIndex != -1)
                    {
                        string key = trimmedLine.Substring(0, equalsIndex).Trim();
                        string value = trimmedLine.Substring(equalsIndex + 1).Trim();
                        iniData[currentSection][key] = value;
                    }
                }
            }

            return iniData;
        }


        ~ENETIOProvider()
        {
            DeInitIO();
        }

        public int DeInitIO()
        {
            int retVal = -1;
            try
            {
                foreach (var board in _netIONodes)
                {
                    //board.Value.CloseDevice();
                    board.CloseDevice();
                }

                bStopUpdateThread = true;
                UpdateThread?.Join();

                DevConnected = false;

                retVal = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"NETIOProvider DeInitIO() Function error: " + err.Message);
            }
            return retVal;
        }
        public int InitIO(int devNum, ObservableCollection<Channel> channels)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            int initVal = 0;

            try
            {
                _Channels = channels;

                retVal = LoadSysParameter();

                // ENETNodeDefinition에서 읽어온 개수만큼 _netIONodes 생성
                foreach (var nodeDefinition in EnetIODesc.ENETIODescripterParams.ENETNodeDefinitions)
                {
                    var netIOSocketHandler = new NetIOSocketHandler(nodeDefinition.IP);
                    _netIONodes.Add(netIOSocketHandler);
                }

                foreach (var board in _netIONodes)
                {
                    if (board.OpenDevice())
                    {
                        DevConnected = true;
                        LoggerManager.Error($"NETIOProvider InitIO({board.IPAddr}) Open Succeed");
                    }
                    else
                    {
                        LoggerManager.Error($"NETIOProvider InitIO({board.IPAddr}) Open ERROR");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                initVal = -1;
            }

            return initVal;
        }
        public int InitializeController()
        {
            int nRetVal = 0;
            try
            {

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                nRetVal = -1;
                LoggerManager.Error($"ECATIO InitializeController() ERROR");
            }
            return nRetVal;
        }
        private void PrevOutputUpdate()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private byte GetReadPIVar(ushort index, uint state)
        {
            try
            {
                int totalIndex = DIGITAL_OUTPUT + index;
                if (((state >> totalIndex) >> index & 0x01) == 0x01)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private void UpdateIOProc()
        {

            try
            {


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }
        public IORet ReadBit(int channel, int port, out bool value, bool reverse = false, bool isForced = false, bool ForcedValue = false)
        {

            //_netIONodes[0].readBit(channel, port);




            //return retCode;

            // TODO : HMNAM : Need Implementation 2020-03-17
            value = true;
            return IORet.NO_ERR;
        }
        /// <summary>
        ///  boardidx * 6 + channel
        /// </summary>
        /// <param name="channel"> boardidx * 6 + channel </param>
        /// <param name="port">bit in channel</param>
        /// <param name="value"></param>
        /// <returns></returns>
        public IORet WriteBit(int channel, int port, bool value)
        {
            //int boardidx = channel / 6;
            //int boardch = channel % 6;
            //string deviceName = $"DEV{boardidx+1}";
            //_netIONodes[deviceName].Write_Data_IO(boardch, port, value);

            IORet retCode = IORet.ERROR;
            bool ret = false;
            int board_idx = channel / 6;
            int board_ch = channel % 6;

            ret = _netIONodes[board_idx].Write_Data_IO(board_ch, port, value);

            if (ret) retCode = IORet.NO_ERR;
            return retCode;
        }
        public IORet ReadValue(int channel, int port, out long value, bool reverse = false)
        {
            //PI_VAR_UNION inputSt = new PI_VAR_UNION();

            IORet retCode = IORet.ERROR;
            value = 0;
            try
            {
                //Crevis
                if (Channels[channel].IOType == EnumIOType.AI)
                {

                }
                else if (Channels[channel].IOType == EnumIOType.AO)
                {

                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($string.Format("ReadBit(channel#{0}): Function error occurred. Error = {1}",channel, err.Message));
                LoggerManager.Exception(err);

                value = 0;
                retCode = IORet.ERROR;
            }
            return retCode;
        }
        public IORet WriteValue(int channel, int port, long value)
        {
            //PI_VAR_UNION outputSt = new PI_VAR_UNION();

            IORet retCode = IORet.NO_ERR;
            try
            {


                if (Channels[channel].IOType == EnumIOType.AO)
                {

                }
                else
                {
                    LoggerManager.Debug($"WriteValue(): Not proper type for write value. IO type is {Channels[channel].IOType}");
                    retCode = IORet.ErrorSignatureNotMatch;
                }

            }
            catch (Exception err)
            {
                //LoggerManager.Error($string.Format("WriteBit(channel#{0}, port#{1}, {2}): Function error occurred. Error = {3}", channel, port, value, err.Message));
                LoggerManager.Exception(err);

                retCode = IORet.ERROR;
                return retCode;
            }
            return (IORet)retCode;
        }
        public int WaitForIO(int channel, int port, bool level, long timeout = 0, bool isForced = false, bool forcedValue = false)
        {
            int retVal = -1;
            Stopwatch elapsedStopWatch = new Stopwatch();

            elapsedStopWatch.Reset();
            elapsedStopWatch.Start();
            int cnt = 0;
            List<KeyValuePair<string, long>> timeStamp;
            timeStamp = new List<KeyValuePair<string, long>>();
            IORet ioReturn;
            try
            {
                bool runFlag = true;
                bool value;
                if (timeout == 0)
                {
                    timeout = 2000;
                }
                timeStamp.Add(new KeyValuePair<string, long>($"Entering DoWhile Loop", elapsedStopWatch.ElapsedMilliseconds));
                do
                {
                    //timeStamp.Add(new KeyValuePair<string, long>($"ReadBit Start.", elapsedStopWatch.ElapsedMilliseconds));
                    ioReturn = ReadBit(channel, port, out value, isForced: isForced, ForcedValue: forcedValue);
                    //timeStamp.Add(new KeyValuePair<string, long>($"ReadBit Done.", elapsedStopWatch.ElapsedMilliseconds));

                    cnt++;
                    if (ioReturn == IORet.NO_ERR)
                    {
                        if (timeout != 0)
                        {
                            if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                            {
                                LoggerManager.Error($"WaitForIO({channel}, {port}) : wait io(Target level = {level}) timeout error occurred. Timeout = {timeout}ms");

                                runFlag = false;
                                retVal = -2;
                                //throw new IOException(
                                //    string.Format("WaitForIO({0}, {1}) : wait io timeout error occurred. Timeout = {2}ms",
                                //    channel, port, timeout));
                            }
                            else
                            {
                                if (value == level)
                                {
                                    runFlag = false;
                                    retVal = 0;
                                    LoggerManager.Debug($"WaitForIO({channel}, {port}) : IO value matched with {value}, Elapsed time = {elapsedStopWatch.ElapsedMilliseconds}ms");
                                }
                                else runFlag = true;
                            }
                        }
                        else
                        {
                            if (value == level)
                            {
                                runFlag = false;
                                retVal = 0;
                                LoggerManager.Debug($"WaitForIO({channel}, {port}) : IO value matched with {value}, Elapsed time = {elapsedStopWatch.ElapsedMilliseconds}ms");
                            }
                            else runFlag = true;
                        }
                    }
                    else
                    {
                        runFlag = false;

                        retVal = -1;
                        LoggerManager.Error($"WaitForIO({channel}, {port}) : wait io error occurred. Timeout = {timeout}ms");
                    }
                } while (runFlag == true);
            }
            catch (Exception err)
            {
                //LoggerManager.Error($string.Format("WaitForIO({0}, {1}) : wait io error occurred. Timeout = {2}ms, Err = {3}", channel, port, timeout, err.Message));
                LoggerManager.Exception(err);

            }
            finally
            {
                elapsedStopWatch?.Stop();

            }
            return retVal;
        }
        /// <summary>
        /// Analog IO의 값을 기다리는 메서드
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="port"></param>
        /// <param name="level"></param>
        /// <param name="timeout"></param>
        /// <param name="isForced"></param>
        /// <param name="forcedValue"></param>
        /// <returns></returns>
        public int WaitForIO(int channel, int port, long level, long timeout = 0, bool isForced = false, bool forcedValue = false)
        {
            int retVal = -1;
            Stopwatch elapsedStopWatch = new Stopwatch();

            elapsedStopWatch.Reset();
            elapsedStopWatch.Start();
            int cnt = 0;
            List<KeyValuePair<string, long>> timeStamp;
            timeStamp = new List<KeyValuePair<string, long>>();
            IORet ioReturn;
            try
            {
                bool runFlag = true;
                long value;
                if (timeout == 0)
                {
                    timeout = 2000;
                }
                timeStamp.Add(new KeyValuePair<string, long>($"Entering DoWhile Loop", elapsedStopWatch.ElapsedMilliseconds));
                do
                {
                    ioReturn = ReadValue(channel, port, out value);

                    cnt++;
                    if (ioReturn == IORet.NO_ERR)
                    {
                        if (timeout != 0)
                        {
                            if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                            {
                                LoggerManager.Error($"WaitForIO({channel}, {port}) : wait io(Target level = {level}) timeout error occurred. Timeout = {timeout}ms");

                                runFlag = false;
                                retVal = -2;
                            }
                            else
                            {
                                if (value == level)
                                {
                                    runFlag = false;
                                    retVal = 0;
                                    LoggerManager.Debug($"WaitForIO({channel}, {port}) : IO value matched with {value}, Elapsed time = {elapsedStopWatch.ElapsedMilliseconds}ms");
                                }
                                else runFlag = true;
                            }
                        }
                        else
                        {
                            if (value == level)
                            {
                                runFlag = false;
                                retVal = 0;
                                LoggerManager.Debug($"WaitForIO({channel}, {port}) : IO value matched with {value}, Elapsed time = {elapsedStopWatch.ElapsedMilliseconds}ms");
                            }
                            else runFlag = true;
                        }
                    }
                    else
                    {
                        runFlag = false;

                        retVal = -1;
                        LoggerManager.Error($"WaitForIO({channel}, {port}) : wait io error occurred. Timeout = {timeout}ms");
                    }
                } while (runFlag == true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                elapsedStopWatch?.Stop();
            }
            return retVal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="channel">light board channel , CAUTION is NOT IO-BOARD Channel</param>
        /// <param name="lightPower"></param>
        public void SetLight(int node, int channel, int lightPower)
        {// 23,21
            //try 0~5, 6~11, 12~17, 18~23 
            //{
            //    WriteValue(node, channel, lightPower);
            //}
            //catch (Exception err)
            //{
            //    LoggerManager.Exception(err);
            //    throw;
            //}
            //TODO, current test device.
            //_netIONodes["DEV2"].Write_Data_Light(channel, (ushort)lightPower);

        }
        public void WriteCameraPort(int chan, int port, bool isSet)
        {
            try
            {
                WriteBit(chan, port, isSet);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public int MonitorForIO(int channel, int port, bool level, long sustain = 0, long timeout = 10000, bool reverse = false, bool isForced = false, bool forcedValue = false, bool writelog = true, string ioKey = "")
        {
            if (timeout == 0)
                timeout = 10000;
            //#endif

            //=> Return Values
            int NO_ERROR = 0;
            int NET_IO_ERROR = -1;
            int TIME_OUT_ERROR = -2;

            int retVal = -1;
            bool matched = false;
            Stopwatch elapsedStopWatch = new Stopwatch();
            elapsedStopWatch.Reset();
            elapsedStopWatch.Start();
            int cnt = 0;
            Stopwatch sustainStopWatch = new Stopwatch();
            sustainStopWatch.Reset();

            IORet ioReturn;
            List<KeyValuePair<string, long>> timeStamp;
            timeStamp = new List<KeyValuePair<string, long>>();

            try
            {
                bool runFlag = true;
                bool value;
                timeStamp.Add(new KeyValuePair<string, long>($"Entering DoWhile Loop", elapsedStopWatch.ElapsedMilliseconds));
                do
                {
                    //timeStamp.Add(new KeyValuePair<string, long>($"ReadBit Start.", elapsedStopWatch.ElapsedMilliseconds));
                    ioReturn = ReadBit(channel, port, out value, reverse, isForced: isForced, ForcedValue: forcedValue);
                    //timeStamp.Add(new KeyValuePair<string, long>($"ReadBit Done.", elapsedStopWatch.ElapsedMilliseconds));

                    cnt++;
                    if (ioReturn == IORet.NO_ERR)
                    {
                        try
                        {
                            if (timeout != 0)
                            {
                                if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                                {
                                    if (writelog == true)
                                    {
                                        LoggerManager.Error($"MonitorForIO({channel}, {port}) : wait io(Target Level = {level}) timeout error occurred. Timeout = {timeout}ms, io:{ioKey}");
                                    }

                                    runFlag = false;
                                    retVal = TIME_OUT_ERROR;
                                    //throw new InOutException(
                                    //    string.Format("WaitForIO({0}, {1}) : wait io timeout error occurred. Timeout = {2}ms",
                                    //    channel, port, timeout));
                                }
                                else
                                {
                                    if (value == level)
                                    {
                                        if (matched == false)
                                        {
                                            sustainStopWatch.Start();
                                            matched = true;
                                            timeStamp.Add(new KeyValuePair<string, long>($"Value matched.", elapsedStopWatch.ElapsedMilliseconds));

                                        }
                                        else
                                        {
                                            if (sustainStopWatch.ElapsedMilliseconds > sustain)
                                            {
                                                runFlag = false;
                                                retVal = NO_ERROR;
                                                if (writelog == true)
                                                {
                                                    LoggerManager.Debug($"MonitorForIO({channel}, {port}) : IO value matched with {value}, Elapsed time = {elapsedStopWatch.ElapsedMilliseconds}ms, io:{ioKey}");
                                                }
                                                sustainStopWatch.Stop();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        sustainStopWatch.Stop();
                                        sustainStopWatch.Reset();

                                        matched = false;
                                        runFlag = true;
                                    }
                                }
                            }
                            else
                            {
                                if (value == level)
                                {
                                    runFlag = false;
                                    retVal = NO_ERROR;
                                    if (writelog == true)
                                    {
                                        LoggerManager.Debug($"MonitorForIO({channel}, {port}) : IO value matched with {value}, Elapsed time = {elapsedStopWatch.ElapsedMilliseconds}ms, io:{ioKey}");
                                    }
                                }
                                else
                                {
                                    runFlag = true;
                                }
                            }
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                            throw;
                        }
                    }
                    else
                    {
                        retVal = NET_IO_ERROR;
                        runFlag = false;
                        timeStamp.Add(new KeyValuePair<string, long>($"IO Error, Return code = {retVal}", elapsedStopWatch.ElapsedMilliseconds));

                        if (writelog == true)
                        {
                            LoggerManager.Error($"MonitorForIO({channel}, {port}) : wait io error occurred. Timeout = {timeout}ms, io:{ioKey}");
                        }
                        //throw new InOutException(
                        //    string.Format("WaitForIO({0}, {1}) : wait io error occurred. Timeout = {2}ms",
                        //    channel, port, timeout));
                    }

                    Thread.Sleep(4);

                } while (runFlag == true);
            }
            catch (Exception err)
            {
                retVal = NET_IO_ERROR;
                //LoggerManager.Error($string.Format("MonitorForIO({0}, {1}) : wait io error occurred. Timeout = {2}ms, Err = {3}", channel, port, timeout, err.Message));
                LoggerManager.Exception(err);

            }
            finally
            {
                elapsedStopWatch?.Stop();

            }

            return retVal;
        }
        /// <summary>
        /// Analog IO의 값을 모니터링하는 메서드
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="port"></param>
        /// <param name="level"></param>
        /// <param name="sustain"></param>
        /// <param name="timeout"></param>
        /// <param name="reverse"></param>
        /// <param name="isForced"></param>
        /// <param name="ForcedValue"></param>
        /// <param name="writelog"></param>
        /// <param name="ioKey"></param>
        /// <returns></returns>
        public int MonitorForIO(int channel, int port, long level, long sustain = 0, long timeout = 10000, bool reverse = false, bool isForced = false, bool ForcedValue = false, bool writelog = true, string ioKey = "")
        {
            if (timeout == 0)
                timeout = 10000;

            int NO_ERROR = 0;
            int NET_IO_ERROR = -1;
            int TIME_OUT_ERROR = -2;

            int retVal = -1;
            bool matched = false;
            Stopwatch elapsedStopWatch = new Stopwatch();
            elapsedStopWatch.Reset();
            elapsedStopWatch.Start();
            int cnt = 0;
            Stopwatch sustainStopWatch = new Stopwatch();
            sustainStopWatch.Reset();

            IORet ioReturn;
            List<KeyValuePair<string, long>> timeStamp;
            timeStamp = new List<KeyValuePair<string, long>>();

            try
            {
                bool runFlag = true;
                long value;
                timeStamp.Add(new KeyValuePair<string, long>($"Entering DoWhile Loop", elapsedStopWatch.ElapsedMilliseconds));
                do
                {
                    ioReturn = ReadValue(channel, port, out value);
                    cnt++;
                    if (ioReturn == IORet.NO_ERR)
                    {
                        try
                        {
                            if (timeout != 0)
                            {
                                if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                                {
                                    if (writelog == true)
                                    {
                                        LoggerManager.Error($"MonitorForIO({channel}, {port}) : wait io(Target Level = {level}) timeout error occurred. Timeout = {timeout}ms, io:{ioKey}");
                                    }

                                    runFlag = false;
                                    retVal = TIME_OUT_ERROR;
                                }
                                else
                                {
                                    if (value == level)
                                    {
                                        if (matched == false)
                                        {
                                            sustainStopWatch.Start();
                                            matched = true;
                                            timeStamp.Add(new KeyValuePair<string, long>($"Value matched.", elapsedStopWatch.ElapsedMilliseconds));

                                        }
                                        else
                                        {
                                            if (sustainStopWatch.ElapsedMilliseconds > sustain)
                                            {
                                                runFlag = false;
                                                retVal = NO_ERROR;
                                                if (writelog == true)
                                                {
                                                    LoggerManager.Debug($"MonitorForIO({channel}, {port}) : IO value matched with {value}, Elapsed time = {elapsedStopWatch.ElapsedMilliseconds}ms, io:{ioKey}");
                                                }
                                                sustainStopWatch.Stop();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        sustainStopWatch.Stop();
                                        sustainStopWatch.Reset();

                                        matched = false;
                                        runFlag = true;
                                    }
                                }
                            }
                            else
                            {
                                if (value == level)
                                {
                                    runFlag = false;
                                    retVal = NO_ERROR;
                                    if (writelog == true)
                                    {
                                        LoggerManager.Debug($"MonitorForIO({channel}, {port}) : IO value matched with {value}, Elapsed time = {elapsedStopWatch.ElapsedMilliseconds}ms, io:{ioKey}");
                                    }
                                }
                                else
                                {
                                    runFlag = true;
                                }
                            }
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                            throw;
                        }
                    }
                    else
                    {
                        retVal = NET_IO_ERROR;
                        runFlag = false;
                        timeStamp.Add(new KeyValuePair<string, long>($"IO Error, Return code = {retVal}", elapsedStopWatch.ElapsedMilliseconds));

                        if (writelog == true)
                        {
                            LoggerManager.Error($"MonitorForIO({channel}, {port}) : wait io error occurred. Timeout = {timeout}ms, io:{ioKey}");
                        }
                    }

                    Thread.Sleep(4);

                } while (runFlag == true);
            }
            catch (Exception err)
            {
                retVal = NET_IO_ERROR;
                LoggerManager.Exception(err);
            }
            finally
            {
                elapsedStopWatch?.Stop();
            }
            return retVal;
        }

        public EventCodeEnum LoadNETIODescripterParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            //EcatIODesc = new ECATIODescripter();
            EnetIODesc = new ENETIODescripter();
            try
            {
                IParam tmpParam = null;
                //RetVal = this.LoadParameter(ref tmpParam, typeof(ECATIODescripter.ECATIODescripterParam));
                RetVal = this.LoadParameter(ref tmpParam, typeof(ENETIODescripter.ENETIODescripterParam));
                if (RetVal == EventCodeEnum.NONE)
                {
                    //EcatIODesc.ECATIODescripterParams = tmpParam as ECATIODescripter.ECATIODescripterParam;
                    EnetIODesc.ENETIODescripterParams = tmpParam as ENETIODescripter.ENETIODescripterParam;
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($String.Format("[ECATIOProvider] LoadSysParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);

            }

            return RetVal;
        }
        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = LoadNETIODescripterParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            //this.SysParam = new IParamEmpty();

            return RetVal;
        }
        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        public int WaitForIO(int channel, int port, bool level, long sustain = 0, long timeout = 10000)
        {
#if DEBUG
            timeout = 60000;
#endif
            int retVal = -1;
            bool matched = false;
            Stopwatch elapsedStopWatch = new Stopwatch();
            Stopwatch sustainStopWatch = new Stopwatch();
            elapsedStopWatch.Reset();
            elapsedStopWatch.Start();

            sustainStopWatch.Reset();

            IORet ioReturn;
            try
            {
                bool runFlag = true;
                bool value;

                do
                {
                    Thread.Sleep(4);

                    ioReturn = ReadBit(channel, port, out value);
                    if (ioReturn == IORet.NO_ERR)
                    {
                        if (timeout != 0)
                        {
                            if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                            {
                                LoggerManager.Error($"WaitForIO({channel}, {port}) : wait io(Target Level = {level}) timeout error occurred. Timeout = {timeout}ms");

                                runFlag = false;
                                retVal = -2;
                                throw new IOException(
                                    string.Format("WaitForIO({0}, {1}) : wait io(Target Level = {level}) timeout error occurred. Timeout = {2}ms",
                                    channel, port, timeout));
                            }
                            else
                            {
                                if (value == level)
                                {
                                    if (matched == false)
                                    {
                                        sustainStopWatch.Start();
                                        matched = true;
                                    }
                                    else
                                    {
                                        if (sustainStopWatch.ElapsedMilliseconds > sustain)
                                        {
                                            runFlag = false;
                                            retVal = 0;
                                            LoggerManager.Debug($"WaitForIO({channel}, {port}) : IO value matched with {value}, Elapsed time = {elapsedStopWatch.ElapsedMilliseconds}ms");
                                            sustainStopWatch.Stop();
                                        }
                                    }
                                }
                                else
                                {
                                    sustainStopWatch.Stop();
                                    sustainStopWatch.Reset();
                                    matched = false;
                                    runFlag = true;
                                }
                            }
                        }
                        else
                        {
                            if (value == level)
                            {
                                runFlag = false;
                                retVal = 0;
                                LoggerManager.Debug($"WaitForIO({channel}, {port}) : IO value matched with {value}, Elapsed time = {elapsedStopWatch.ElapsedMilliseconds}ms");
                            }
                            else runFlag = true;
                        }
                    }
                    else
                    {
                        runFlag = false;

                        retVal = -1;
                        LoggerManager.Error($"WaitForIO({channel}, {port}) : wait io error occurred. Timeout = {timeout}ms");

                        throw new IOException(
                            string.Format("WaitForIO({0}, {1}) : wait io error occurred. Timeout = {2}ms",
                            channel, port, timeout));
                    }
                } while (runFlag == true);
            }
            catch (Exception err)
            {
                //LoggerManager.Error($string.Format("WaitForIO({0}, {1}) : wait io error occurred. Timeout = {2}ms, Err = {3}", channel, port, timeout, err.Message));
                LoggerManager.Exception(err);

            }
            finally
            {
                elapsedStopWatch?.Stop();

            }
            return retVal;
        }
    }
}

