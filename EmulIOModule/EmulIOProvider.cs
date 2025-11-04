using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using LogModule;
using System.Runtime.CompilerServices;

namespace EmulIOModule
{
    public class EmulIOProvider : IIOBase, ILightDeviceControl, ICameraChannelControl
    {
        public short DeviceNumber { get; set; }
        public ObservableCollection<Channel> Channels { get; set; }

        public bool DevConnected { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }


        public EmulIOProvider()
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
        public int DeInitIO()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return 1;
        }

        public int InitIO(int devNum, ObservableCollection<Channel> channels)
        {
            try
            {
                Channels = channels;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return 1;
        }


        public int InitIO(int devNum, ObservableCollection<Channel> channels, string loadFilePath)
        {
            throw new NotImplementedException();
        }

        public IORet ReadBit(int port, int bit, out bool value, bool reverse = false, bool isForced = false, bool ForcedValue = false)
        {
            try
            {
                value = false;
                if (!isForced)
                {
                    if (reverse == false)
                    {
                        value = Channels[port].Port[bit].PortVal;
                    }
                    else
                    {
                        value = !Channels[port].Port[bit].PortVal;
                    }
                }
                else
                {
                    value = ForcedValue;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return IORet.NO_ERR;
        }

        public IORet ReadValue(int channel, int port, out long value, bool reverse = false)
        {
            throw new NotImplementedException();
        }

        public int WaitForIO(int channel, int port, bool level, long timeout = 0, bool isForced = false, bool forcedValue = false)
        {
            //#if DEBUG
            //            timeout = 60000;
            //#endif
            if (!isForced)
            {
                return 0;
            }
            int retVal = -1;
            Stopwatch elapsedStopWatch = new Stopwatch();
            timeout = 1000;
            elapsedStopWatch.Reset();
            elapsedStopWatch.Start();

            IORet ioReturn;
            try
            {
                bool runFlag = true;
                bool value;
                //minskim// GC 호출 및 CPU 사용률 절감을 위해 기존 timer+resetevent로 thread 제어하던 로직을 제거 하고 sleep으로 대체함, sleep시간은 기존 timer interval 주기 값으로 설정함
                System.Threading.Thread.Sleep(12);
                do
                {
                    //minskim// GC 호출 및 CPU 사용률 절감을 위해 기존 timer+resetevent로 thread 제어하던 로직을 제거 하고 sleep으로 대체함, sleep시간은 기존 timer interval 주기 값으로 설정함
                    System.Threading.Thread.Sleep(12);
                    ioReturn = ReadBit(channel, port, out value, false, isForced, forcedValue);
                    if (ioReturn == IORet.NO_ERR)
                    {
                        if (timeout != 0)
                        {
                            if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                            {
                                LoggerManager.Error($"WaitForIO({channel}, {port}) : wait io timeout error occurred. Timeout = {timeout}ms");

                                runFlag = false;
                                retVal = -2;
                                //throw new InOutException(
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

                        //throw new InOutException(
                        //    string.Format("WaitForIO({0}, {1}) : wait io error occurred. Timeout = {2}ms",
                        //    channel, port, timeout));
                    }
                } while (runFlag == true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                //LoggerManager.Error($"WaitForIO({channel}, {port}) : wait io error occurred. Timeout = {timeout}ms, Err = {err.Message}");
            }
            finally
            {
                elapsedStopWatch?.Stop();

            }
            return retVal;
        }

        public int MonitorForIO(int channel, int port, bool level, long sustain = 0, long timeout = 10000, bool reverse = false, bool isForced = false, bool forcedValue = false, bool writelog = true, string ioKey = "")
        {
            if (timeout == 0)
                timeout = 10000;

            if (!isForced)
                return (int)IORet.NO_ERR;

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
                    timeStamp.Add(new KeyValuePair<string, long>($"ReadBit Start.", elapsedStopWatch.ElapsedMilliseconds));
                    ioReturn = ReadBit(channel, port, out value, reverse, isForced, forcedValue);
                    timeStamp.Add(new KeyValuePair<string, long>($"ReadBit Done.", elapsedStopWatch.ElapsedMilliseconds));

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
                                        LoggerManager.Error($"MonitorForIO({channel}, {port}) : wait io timeout error occurred. Timeout = {timeout}ms, io:{ioKey}");
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

                    //minskim// 기존 manualresetevent wait를 sleep으로 대체함, sleep 시간은 기존 manualreset event 대기 시간으로 동일화 함(set하는 부분이 없어 항상 100ms을 대기 하고 있었음) 
                    System.Threading.Thread.Sleep(100);


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

        public IORet WriteBit(int channel, int port, bool value)
        {
            IORet retVal;

            try
            {
                Channels[channel].Port[port].PortVal = value;
                retVal = IORet.NO_ERR;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }

        public IORet WriteValue(int channel, int port, long value)
        {
            throw new NotImplementedException();
        }

        int IIOBase.InitIO(int devNum, ObservableCollection<Channel> channels)
        {
            try
            {
                Channels = channels;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return 0;
            //throw new NotImplementedException();
        }

        //public int WaitForIO(int channel, int port, bool level, long sustain = 0, long timeout = 10000, bool isForced = false, bool forcedValue = false)
        //{
        //    //#if DEBUG
        //    //            timeout = 60000;
        //    //#endif
        //    if(!isForced)
        //    {
        //        return 0;
        //    }
        //    int retVal = -1;
        //    Stopwatch elapsedStopWatch = new Stopwatch();

        //    elapsedStopWatch.Reset();
        //    elapsedStopWatch.Start();

        //    IORet ioReturn;
        //    try
        //    {
        //        bool runFlag = true;
        //        bool value;
        //        areUpdateEvent.WaitOne(100);
        //        do
        //        {
        //            areUpdateEvent.WaitOne(100);
        //            ioReturn = ReadBit(channel, port, out value);
        //            if (ioReturn == IORet.NO_ERR)
        //            {
        //                if (timeout != 0)
        //                {
        //                    if (elapsedStopWatch.ElapsedMilliseconds > timeout)
        //                    {
        //                        LoggerManager.Error($"WaitForIO({channel}, {port}) : wait io timeout error occurred. Timeout = {timeout}ms");

        //                        runFlag = false;
        //                        retVal = -2;
        //                        throw new InOutException(
        //                            string.Format("WaitForIO({0}, {1}) : wait io timeout error occurred. Timeout = {2}ms",
        //                            channel, port, timeout));
        //                    }
        //                    else
        //                    {
        //                        if (value == level)
        //                        {
        //                            runFlag = false;
        //                            retVal = 0;
        //                            LoggerManager.Debug($"WaitForIO({channel}, {port}) : IO value matched with {value}, Elapsed time = {elapsedStopWatch.ElapsedMilliseconds}ms");
        //                        }
        //                        else runFlag = true;
        //                    }
        //                }
        //                else
        //                {
        //                    if (value == level)
        //                    {
        //                        runFlag = false;
        //                        retVal = 0;
        //                        LoggerManager.Debug($"WaitForIO({channel}, {port}) : IO value matched with {value}, Elapsed time = {elapsedStopWatch.ElapsedMilliseconds}ms");
        //                    }
        //                    else runFlag = true;
        //                }
        //            }
        //            else
        //            {
        //                runFlag = false;

        //                retVal = -1;

        //                LoggerManager.Error($"WaitForIO({channel}, {port}) : wait io error occurred. Timeout = {timeout}ms");

        //                throw new InOutException(
        //                    string.Format("WaitForIO({0}, {1}) : wait io error occurred. Timeout = {2}ms",
        //                    channel, port, timeout));
        //            }
        //        } while (runFlag == true);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);

        //        //LoggerManager.Error($"WaitForIO({channel}, {port}) : wait io error occurred. Timeout = {timeout}ms, Err = {err.Message}");
        //    }
        //    finally
        //    {
        //        elapsedStopWatch?.Stop();

        //    }
        //    return retVal;
        //}

        public void SetLight(int node, int channel, int lightPower)
        {
            return;
        }

        public void WriteCameraPort(int chan, int port, bool isSet)
        {

        }
    }
}
