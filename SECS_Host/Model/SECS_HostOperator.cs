using SECS_Host.Help;
using SECS_Host.SecsMsgClass;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using XComPro.Library;

namespace SECS_Host.Model
{
    public class SECS_HostOperator : INotifyPropertyChanged
    {
        // Const Value
        private readonly double SECS_SENDING_TIMEOUT = 5000;

        // Interface Realization
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChange(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        private XComProW m_XComPro;
        private SECSData _ReceivedECV;
        private bool isCompleteSend;

        public AddLogDelegate AddLog { get; set; }
        public SecsMsgBase secsMsg;

        public SECSData ReceivedData { get; set; }

        private bool isConnected;
        public bool IsConnected
        {
            get { return isConnected; }
            set
            {
                isConnected = value;
                NotifyPropertyChange(nameof(IsConnected));
            }
        }

        public SECS_HostOperator(XComProW xComPro, string config)
        {
            int initRet = 0;

            this.m_XComPro = xComPro;
            m_XComPro.OnSecsEvent += new ON_SECSEVENT(m_XComPro_OnSecsEvent);
            m_XComPro.OnSecsMsg += new ON_SECSMSG(m_XComPro_OnSecsMsg);

            initRet = m_XComPro.Initialize(config);
            if (initRet < 0)
            {
                AddLog?.Invoke("XComPro initialization failed: XComPro error={0}", initRet);
                return;
            }
        }

        public void SetSecsMsg(SecsMsgBase _secsMsg)
        {
            secsMsg = _secsMsg;
            secsMsg.m_XComPro = m_XComPro;
            secsMsg.AddLog = AddLog;
        }

        #region => Functions for Commands

        #region ==> Functions that help Command
        private bool DoneSendDyMsg(double timeoutTime)
        {
            bool bRetVal = true;
            DateTime startTime = DateTime.Now;

            while (!isCompleteSend)
            {
                double duringTime = (DateTime.Now - startTime).TotalMilliseconds;
                if (timeoutTime < duringTime)
                {
                    isCompleteSend = true;
                    bRetVal = false;
                }
            }

            isCompleteSend = false;

            return bRetVal;
        }
        #endregion

        #region ==> Start/Stop Host
        public bool StartHost(object obj)
        {
            bool bRetVal = false;
            int ret = m_XComPro.Start();
            if (ret < 0)
            {
                AddLog("XComPro start failed: error={0}", ret);
                return bRetVal;
            }

            if (m_XComPro.GetParam("HSMS") == "true")
                AddLog("XComPro started in HSMS mode");
            else
                AddLog("XComPro started in SECS-I mode");

            bRetVal = true;
            return bRetVal;
        }

        public bool StopHost(object obj)
        {
            bool bRetVal = false;
            int ret = m_XComPro.Stop();
            if (ret < 0)
            {
                AddLog("XComPro stop failed: error={0}", ret);
                return bRetVal;
            }

            AddLog("XComPro stopped");
            bRetVal = true;
            return bRetVal;
        }

        public String StartStopHost(object obj)
        {
            String retString = null;

            if ((bool)obj)
            {
                if (StartHost(null))
                    retString = "Connected";
                else
                    retString = null;
            }
            else
            {
                if (StopHost(null))
                    retString = "UnConnected";
                else
                    retString = null;
            }
            
            return retString;
        }
        #endregion

        #region ==> Set On/Offline Mode
        public void SetOnlineMode(object obj)
        {
            SetSecsMsg(new SecsMsg_S1F17());
            secsMsg.Excute();
        }

        public void SetOfflineMode(object obj)
        {
            SetSecsMsg(new SecsMsg_S1F15());
            secsMsg.Excute();
        }
        #endregion

        #region ==> Send Dynamic Report
        public void SendDynamicReport(object obj, string Path, params String[] separator)
        {
            SECS_GetStructFromFile(Path, separator);
        }

        private async void SECS_GetStructFromFile(String path, params String[] separator)
        {
            string strline = null;
            int nReturn = 0;
            int lSMsgId = 0;

            System.IO.StreamReader file = new System.IO.StreamReader(path);

            try
            {
                while ((strline = file.ReadLine()) != null)
                {
                    foreach (var v in separator)
                    {
                        if (strline.Contains(v))
                        {
                            String[] splitStr = null;
                            DynamicMSG.SECS_DynamicReport tempDynamicReport = new DynamicMSG.SECS_DynamicReport();
                            short nStrm = 0;
                            short nFunc = 0;

                            tempDynamicReport.DyanmicReportName = v;
                            tempDynamicReport.GetStructFromStream(file); //Get Data

                            if (tempDynamicReport.DynamicValue != null)
                            {
                                splitStr = tempDynamicReport.DyanmicReportName.Split('S');
                                splitStr = splitStr[1].Split(',');
                                nStrm = short.Parse(splitStr[0]);
                                splitStr = splitStr[1].Split('F');
                                nFunc = short.Parse(splitStr[1]);

                                nReturn = m_XComPro.MakeSecsMsg(ref lSMsgId, m_XComPro.DeviceID, nStrm, nFunc, 0);

                                //if (tempDynamicReport.DynamicValue.Value != null)
                                SecsMsgBase.SetSECSItems(m_XComPro, lSMsgId, tempDynamicReport.DynamicValue);

                                isCompleteSend = false;
                                if ((nReturn = m_XComPro.Send(lSMsgId)) < 0)
                                {
                                    AddLog("Failed to reply S{0}F{1}: error={2}", nStrm, nFunc, nReturn);
                                    throw new Exception();
                                }
                                else
                                {
                                    AddLog("Reply S{0}F{1} was sent successfully", nStrm, nFunc);
                                    Task<bool> task = Task<bool>.Run(() => DoneSendDyMsg(SECS_SENDING_TIMEOUT));
                                    bool retVal = await task;

                                    if (!retVal)
                                        throw new TimeoutException();
                                }
                                tempDynamicReport.DisposeList();
                                tempDynamicReport = null;
                            }
                        }
                    }
                }
            }
            catch (Exception e)//send error
            {
                AddLog("Exception : SECS_GetStructFromFile {0}", e.Message);
                isCompleteSend = true;
            }
            finally
            {
                file.Close();
            }
        }
        #endregion

        #region ==> Get Status Name List
        public void GetStatusNamelist(object obj)
        {
            SetSecsMsg(new SecsMsg_S1F11());
            secsMsg.Excute();
        }
        #endregion

        #region ==> Are You There
        public void AreYouThere(object obj)
        {
            SetSecsMsg(new SecsMsg_S1F1ToEQ());
            secsMsg.Excute();
        }
        #endregion

        #region ==> Get ECData
        public async Task<SECSData> GetECValue()
        {
            List<SECSData> ecdList = new List<SECSData>();//
            try
            {
                (_ReceivedECV as SECSGenericData<List<SECSData>>)?.Value.Clear();

                //////////////////////////////////////////////////////////////////////
                isCompleteSend = false;
                SetSecsMsg(new SecsMsg_S2F29());

                if (!secsMsg.Excute()) { throw new Exception(); }
                else
                {
                    Task<bool> task = Task<bool>.Run(() => DoneSendDyMsg(SECS_SENDING_TIMEOUT));
                    if (!await task)
                    {
                        throw new TimeoutException();
                    }
                }

                if (_ReceivedECV == null)
                    throw new Exception("OpenChangeECDDialog() : Have not EC Data");


                foreach (var v in ((SECSGenericData<List<SECSData>>)_ReceivedECV)?.Value)
                {
                    SECSGenericData<List<SECSData>> tempData = (SECSGenericData<List<SECSData>>)v;
                    List<SECSData> tempList = (List<SECSData>)tempData.Value;

                    ecdList.Add(tempList[0]);
                }

                //////////////////////////////////////////////////////////////////////
                isCompleteSend = false;
                SetSecsMsg(new SecsMsg_S2F13());

                secsMsg.SetSendData(new SECSGenericData<List<SECSData>>() { Value = ecdList, MsgType = SECS_DataTypeCategory.LIST });

                if (!secsMsg.Excute()) { throw new Exception(); }
                else
                {
                    Task<bool> task = Task<bool>.Run(() => DoneSendDyMsg(SECS_SENDING_TIMEOUT));
                    if (!await task)
                    {
                        throw new TimeoutException();
                    }
                }
                secsMsg.SetSendData(null); 
            }
            catch (Exception e)
            {
                AddLog("Exception : SECS_GetStructFromFile {0}", e.Message);

                isCompleteSend = true;
                //send error
            }
            finally
            {
            }

            return _ReceivedECV;
        }
        #endregion

        #region ==> Request Date Time
        public void RequestDateTime()
        {
            SetSecsMsg(new SecsMsg_S2F17ToEQ());
            secsMsg.Excute();
        }
        #endregion

        #region ==> Set Date and Time
        public async Task<bool> SetDateTime(String m_Time)
        {
            bool retVal = false;
            uint timeFormatID = 3512;
            bool timeFormat = false;
            string SendData = null;

            try
            {
                #region if don't know ID of TimeFormat
                /*
                if (_ReceivedECV != null) _ReceivedECV.Clear();

                //////////////////////////////////////////////////////////////////////
                isCompleteSend = false;
                SetSecsMsg(new SecsMsg_S2F29());

                if (!secsMsg.Excute()) { throw new Exception(); }
                else
                {
                    Task<bool> task = Task<bool>.Run(() => DoneSendDyMsg(SECS_SENDING_TIMEOUT));
                    if (!await task)
                    {
                        throw new TimeoutException();
                    }
                }

                if (_ReceivedECV == null)
                    throw new Exception("OpenChangeECDDialog() : Have not EC Data");


                foreach(var v in _ReceivedECV)
                {
                    SECSDataReference<List<SECSData>> tempECVList = ((SECSDataReference<List<SECSData>>)v);
                    SECSData dateECID = tempECVList.Value[(int)View_Model.ECVStatus.ECDataValueName.ECNAME];

                    if(dateECID.MsgType == SECS_MsgCategory.ASCII)
                    {
                        if(((SECSDataReference<string>)dateECID).Value.ToLower() == "TimeFormat".ToLower())
                        {
                            if (uint.TryParse(tempECVList.Value[(int)View_Model.ECVStatus.ECDataValueName.ECID].ToString(), out timeFormatID))
                            {
                                break;
                            }
                            else
                                throw new Exception("SetDateTime() : Wrong TimeFormat Data");
                        }
                    }
                }
                */
                //////////////////////////////////////////////////////////////////////
                #endregion

                isCompleteSend = false;

                SECSGenericData<List<SECSData>> tempSendData
                    = new SECSGenericData<List<SECSData>>() { Value = new List<SECSData>(), MsgType = SECS_DataTypeCategory.LIST };
                //tempSendData.Value = new List<SECSData>();
                tempSendData.Value.Add(new SECSGenericData<uint>() { Value = timeFormatID, MsgType = SECS_DataTypeCategory.UINT4 });

                SetSecsMsg(new SecsMsg_S2F13());
                secsMsg.SetSendData(tempSendData);
                
                if (!secsMsg.Excute())
                {
                }
                else
                {
                    Task<bool> task = Task<bool>.Run(() => DoneSendDyMsg(SECS_SENDING_TIMEOUT));
                    if (!await task)
                        throw new TimeoutException();
                }

                //////////////////////////////////////////////////////////////////////
                if (_ReceivedECV != null)
                {
                    SECSData tempecv = (_ReceivedECV as SECSGenericData<List<SECSData>>)?.Value[(int)View_Model.ECVStatus.ECDataValueName.ECV];
                    timeFormat = ((SECSGenericData<bool>)tempecv).Value;
                }
                else
                    throw new Exception();

                (_ReceivedECV as SECSGenericData<List<SECSData>>)?.Value.Clear();

                _ReceivedECV = null;

                if (m_Time != null && m_Time != "")
                {
                    SendData = m_Time;
                }
                else
                {
                    if (!timeFormat)
                        SendData = DateTime.Now.ToString("yyMMddHHmmssff");
                    else
                        SendData = DateTime.Now.ToString("yyyyMMddHHmmssff");
                }

                SetSecsMsg(new SecsMsg_S2F31());
                secsMsg.SetSendData(new SECSGenericData<string>() { Value = SendData, MsgType = SECS_DataTypeCategory.ASCII });
                secsMsg.Excute();

                secsMsg.SetSendData(null);
            }
            catch (Exception e)
            {
                AddLog("Exception : SECS_GetStructFromFile {0}", e.Message);

                isCompleteSend = true;
                //send error
            }
            finally
            {
            }

            return retVal;
        }
        #endregion

        #region ==> Get Enable Alarm List
        public async Task<SECSData> GetEnableAlarmList()
        {
            //List<SECSData> ecdList = new List<SECSData>();//
            try
            {
               // (_ReceivedECV as SECSGenericData<List<SECSData>>)?.Value.Clear();

                //////////////////////////////////////////////////////////////////////
                isCompleteSend = false;
                SetSecsMsg(new SecsMsg_S5F7());
                secsMsg.SetParameter(0, 0, 5, 7, 0, 0);

                if (!secsMsg.Excute()) { throw new Exception(); }
                else
                {
                    Task<bool> task = Task<bool>.Run(() => DoneSendDyMsg(SECS_SENDING_TIMEOUT));
                    if (!await task)
                    {
                        throw new TimeoutException();
                    }
                }

                if (_ReceivedECV == null)
                    throw new Exception("GetAlarmList() : Have not Alarm Data");
            }
            catch (Exception e)
            {
                AddLog("Exception : GetAlarmList() {0}", e.Message);

                isCompleteSend = true;
                //send error
            }
            finally
            {
            }

            return _ReceivedECV;
        }
        #endregion

        #region ==> Get ECData
        public async Task<SECSData> GetAlarmList()
        {
            try
            {
                (_ReceivedECV as SECSGenericData<List<SECSData>>)?.Value.Clear();

                //////////////////////////////////////////////////////////////////////
                isCompleteSend = false;
                SetSecsMsg(new SecsMsg_S5F5());
                secsMsg.SetParameter(0, 0, 5, 5, 0, 0);

                if (!secsMsg.Excute()) { throw new Exception(); }
                else
                {
                    Task<bool> task = Task<bool>.Run(() => DoneSendDyMsg(SECS_SENDING_TIMEOUT));
                    if (!await task)
                    {
                        throw new TimeoutException();
                    }
                }

                if (_ReceivedECV == null)
                    throw new Exception("GetAlarmValue() : Have not alarm Data");

                secsMsg.SetSendData(null);
            }
            catch (Exception e)
            {
                AddLog("Exception : SECS_GetStructFromFile {0}", e.Message);

                isCompleteSend = true;
                //send error
            }
            finally
            {
            }

            return _ReceivedECV;
        }
        #endregion

        #region ==> Send Remote CMD
        public void SendRemoteCMD(string cmd)
        {
            SetSecsMsg(new SecsMsg_S2F21());
            secsMsg.SetSendData(new SECSGenericData<string>(){ Value = cmd, MsgType = SECS_DataTypeCategory.ASCII });
            secsMsg.Excute();
        }
        #endregion

        #region ==> Send Remote CMD
        public void SendCMD()
        {
            View.Dialog_SendCMD dialog_sendCMD = new View.Dialog_SendCMD(m_XComPro, AddLog);

            dialog_sendCMD.ShowDialog();
        }
        #endregion
        #endregion

        private void m_XComPro_OnSecsEvent(short nEventId, int lParam)
        {
            switch (nEventId)
            {
                case 101:
                    IsConnected = false;
                    AddLog("[EVENT] HSMS NOT CONNECTED");
                    break;
                case 102:
                    IsConnected = true;
                    AddLog("[EVENT] HSMS NOT SELECTED");
                    break;
                case 103:
                    AddLog("[EVENT] HSMS SELECTED");
                    break;
                default:
                    AddLog("[EVENT] Other event: eventId = {0}", nEventId);
                    break;
            }
        }

        private void m_XComPro_OnSecsMsg()
        {
            int lMsgId = 0, lSysbyte = 0;
            short nStream = 0, nFunc = 0, nDeviceID = 0, nWbit = 0;
            //isCompleteSend = true;

            while (m_XComPro.LoadSecsMsg(ref lMsgId, ref nDeviceID, ref nStream, ref nFunc, ref lSysbyte, ref nWbit) >= 0)
            {
                AddLog("Received S{0}F{1}, Sysbyte={2:X8}", nStream, nFunc, lSysbyte);

                if ((nStream == 1) && (nFunc == 1))
                {
                    SetSecsMsg(new SecsMsg_S1F1FromEQ());
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, nFunc, nWbit, lSysbyte);
                    secsMsg.Excute();
                }
                else if ((nStream == 1) && (nFunc == 2))
                {
                    SetSecsMsg(new SecsMsg_S1F2());
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, nFunc, nWbit, lSysbyte);
                    secsMsg.Excute();
                }
                else if ((nStream == 1) && (nFunc == 4))
                {
                    SetSecsMsg(new SecsMsg_S1F4());
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, nFunc, nWbit, lSysbyte);
                    secsMsg.Excute();
                }
                else if ((nStream == 1) && (nFunc == 12))
                {
                    SetSecsMsg(new SecsMsg_S1F12());
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, nFunc, nWbit, lSysbyte);
                    secsMsg.Excute();
                    if (ReceivedData != null)
                    {
                        if(ReceivedData.MsgType == SECS_DataTypeCategory.LIST)
                        {
                            (ReceivedData as SECSGenericData<List<SECSData>>)?.Value.Clear();
                        }
                    }
                    ReceivedData = secsMsg.m_RecvMSGDataList;
                }
                else if ((nStream == 1) && (nFunc == 13))
                {
                    SetSecsMsg(new SecsMsg_S1F13FromEQ());
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, nFunc, nWbit, lSysbyte);
                    secsMsg.Excute();
                }
                else if ((nStream == 1) && (nFunc == 14))
                {
                    SetSecsMsg(new SecsMsg_S1F14());
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, nFunc, nWbit, lSysbyte);
                    secsMsg.Excute();
                }
                else if ((nStream == 1) && (nFunc == 16))//Request OFF-Line
                {
                    SetSecsMsg(new SecsMsg_S1F16());
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, nFunc, nWbit, lSysbyte);
                    secsMsg.Excute();
                }
                else if ((nStream == 1) && (nFunc == 18))//Request ON-Line
                {
                    SetSecsMsg(new SecsMsg_S1F18());
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, nFunc, nWbit, lSysbyte);
                    secsMsg.Excute();
                }

                else if ((nStream == 2) && (nFunc == 14))
                {
                    isCompleteSend = true;
                    SetSecsMsg(new SecsMsg_S2F14());
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, nFunc, nWbit, lSysbyte);
                    secsMsg.Excute();

                    if (_ReceivedECV != null && _ReceivedECV.Count != 0)
                    {
                        for (int i = 0; i < secsMsg.m_RecvMSGDataList.Count; i++)
                        {
                            SECSData tempData = (_ReceivedECV as SECSGenericData<List<SECSData>>)?.Value[i];
                            List<SECSData> inputDestList = (tempData as SECSGenericData<List<SECSData>>)?.Value;

                            if (i <= secsMsg.m_RecvMSGDataList.Count)
                            {
                                inputDestList.Add((secsMsg.m_RecvMSGDataList as SECSGenericData<List<SECSData>>)?.Value[i]);
                            }
                        }
                    }
                    else
                    {
                        SECSGenericData<List<SECSData>> tempList = new SECSGenericData<List<SECSData>>();
                        tempList.Value = new List<SECSData>();

                        for (int i = 0; i < (int)View_Model.ECVStatus.ECDataValueName.ECV; i++)
                            tempList.Value.Add(new SECSGenericData<int>() { Value = 0 });
                        tempList.Value.Add((secsMsg.m_RecvMSGDataList as SECSGenericData<List<SECSData>>)?.Value[0]);

                        _ReceivedECV = tempList;
                    }
                }
                else if ((nStream == 2) && (nFunc == 16))
                {
                    SetSecsMsg(new SecsMsg_S2F16());
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, nFunc, nWbit, lSysbyte);
                    secsMsg.Excute();
                }
                else if ((nStream == 2) && (nFunc == 17))
                {
                    SetSecsMsg(new SecsMsg_S2F17FromEQ());
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, nFunc, nWbit, lSysbyte);
                    secsMsg.Excute();

                    SetSecsMsg(new SecsMsg_S2F18ToEQ());
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, nFunc, nWbit, lSysbyte);
                    secsMsg.Excute();
                }

                else if ((nStream == 2) && (nFunc == 18))
                {
                    SetSecsMsg(new SecsMsg_S2F18FromEQ());
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, nFunc, nWbit, lSysbyte);
                    secsMsg.Excute();
                }
                else if ((nStream == 2) && (nFunc == 22))
                {
                    SetSecsMsg(new SecsMsg_S2F22());
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, nFunc, nWbit, lSysbyte);
                    secsMsg.Excute();
                }
                else if ((nStream == 2) && (nFunc == 24))
                {
                    SetSecsMsg(new SecsMsg_S2F24());
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, nFunc, nWbit, lSysbyte);
                    secsMsg.Excute();
                }
                else if ((nStream == 2) && (nFunc == 30))
                {
                    SetSecsMsg(new SecsMsg_S2F30());
                    isCompleteSend = true;
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, nFunc, nWbit, lSysbyte);
                    secsMsg.Excute();
                    if (_ReceivedECV != null)
                        _ReceivedECV = null;
                    _ReceivedECV = secsMsg.m_RecvMSGDataList;
                }
                else if ((nStream == 2) && (nFunc == 32))
                {
                    SetSecsMsg(new SecsMsg_S2F32());
                    secsMsg.Excute();
                }
                else if ((nStream == 2) && (nFunc == 33))
                {
                    SetSecsMsg(new SecsMsg_S2F33());
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, nFunc, nWbit, lSysbyte);
                    secsMsg.Excute();
                }
                else if ((nStream == 2) && (nFunc == 34))
                {
                    SetSecsMsg(new SecsMsg_S2F34());
                    isCompleteSend = true;
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, nFunc, nWbit, lSysbyte);
                    secsMsg.Excute();
                }
                else if ((nStream == 2) && (nFunc == 36))
                {
                    SetSecsMsg(new SecsMsg_S2F36());
                    isCompleteSend = true;
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, nFunc, nWbit, lSysbyte);
                    secsMsg.Excute();
                }
                else if ((nStream == 2) && (nFunc == 38))
                {
                    SetSecsMsg(new SecsMsg_S2F38());
                    isCompleteSend = true;
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, nFunc, nWbit, lSysbyte);
                    secsMsg.Excute();
                }
                else if ((nStream == 2) && (nFunc == 42))
                {
                    SetSecsMsg(new SecsMsg_S2F42());
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, nFunc, nWbit, lSysbyte);
                    secsMsg.Excute();
                }
                else if ((nStream == 2) && (nFunc == 50))
                {
                    SetSecsMsg(new SecsMsg_S2F50());
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, nFunc, nWbit, lSysbyte);
                    secsMsg.Excute();
                }
                else if ((nStream == 2) && (nFunc == 17))
                {
                    SetSecsMsg(new SecsMsg_S2F17FromEQ());
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, nFunc, nWbit, lSysbyte);
                    secsMsg.Excute();

                    SetSecsMsg(new SecsMsg_S2F18ToEQ());
                    secsMsg.Excute();
                }
                else if ((nStream == 2) && (nFunc == 18))
                {
                    SetSecsMsg(new SecsMsg_S2F18FromEQ());
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, nFunc, nWbit, lSysbyte);
                    secsMsg.Excute();
                }
                else if ((nStream == 5) && (nFunc == 1))
                {
                    SetSecsMsg(new SecsMsg_S5F1());
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, nFunc, nWbit, lSysbyte);
                    bool bretVal = secsMsg.Excute();

                    SetSecsMsg(new SecsMsg_S5F2());
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, (byte)(nFunc+1), nWbit, lSysbyte);

                    if (bretVal == true)
                        secsMsg.SetSendData(new SECSGenericData<byte> { Value = 0, MsgType = SECS_DataTypeCategory.BINARY });
                    else
                        secsMsg.SetSendData(new SECSGenericData<byte> { Value = 1, MsgType = SECS_DataTypeCategory.BINARY });

                    secsMsg.Excute();
                }
                else if ((nStream == 5) && (nFunc == 4))
                {
                    SetSecsMsg(new SecsMsg_S5F4());
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, nFunc, nWbit, lSysbyte);
                    secsMsg.Excute();
                }
                else if ((nStream == 5) && (nFunc == 6))
                {
                    isCompleteSend = true;

                    SetSecsMsg(new SecsMsg_S5F6());
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, nFunc, nWbit, lSysbyte);
                    secsMsg.Excute();

                    if (_ReceivedECV != null)
                        _ReceivedECV = null;
                    _ReceivedECV = secsMsg.m_RecvMSGDataList;
                }
                else if ((nStream == 5) && (nFunc == 8))
                {
                    isCompleteSend = true;

                    SetSecsMsg(new SecsMsg_S5F8());
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, nFunc, nWbit, lSysbyte);
                    secsMsg.Excute();

                    if (_ReceivedECV != null)
                        _ReceivedECV = null;
                    _ReceivedECV = secsMsg.m_RecvMSGDataList;
                }
                else if ((nStream == 5) && (nFunc == 23))
                {
                    SetSecsMsg(new SecsMsg_S5F23());
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, nFunc, nWbit, lSysbyte);
                    secsMsg.Excute();
                }
                else if ((nStream == 5) && (nFunc == 24))
                {
                    SetSecsMsg(new SecsMsg_S5F24());
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, nFunc, nWbit, lSysbyte);
                    secsMsg.Excute();
                }
                else if ((nStream == 6) && (nFunc == 1))
                {
                    SetSecsMsg(new SecsMsg_S6F1());
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, nFunc, nWbit, lSysbyte);
                    secsMsg.Excute();

                    SetSecsMsg(new SecsMsg_S6F2());
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, (short)(nFunc+1), nWbit, lSysbyte);
                    secsMsg.Excute();

                }
                else if ((nStream == 6) && (nFunc == 11))
                {
                    SetSecsMsg(new SecsMsg_S6F11());
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, nFunc, nWbit, lSysbyte);
                    secsMsg.Excute();
                }
                else if ((nStream == 10) && (nFunc == 1))
                {
                    SetSecsMsg(new SecsMsg_S10F1());
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, nFunc, nWbit, lSysbyte);
                    secsMsg.Excute();

                    SetSecsMsg(new SecsMsg_S10F2());
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, (short)(nFunc + 1), nWbit, lSysbyte);
                    secsMsg.Excute();
                }
                else if ((nStream == 10) && (nFunc == 4))
                {
                    SetSecsMsg(new SecsMsg_S10F4());
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, nFunc, nWbit, lSysbyte);
                    secsMsg.Excute();
                }
                else if ((nStream == 10) && (nFunc == 6))
                {
                    SetSecsMsg(new SecsMsg_S10F6());
                    secsMsg.SetParameter(lMsgId, nDeviceID, nStream, nFunc, nWbit, lSysbyte);
                    secsMsg.Excute();
                }
                else
                {
                    AddLog("Undefined message received (S{0}F{1})", nStream, nFunc);
                    m_XComPro.CloseSecsMsg(lMsgId);
                    return;
                }
            }
        }

        public void EstablishRequest()
        {
            SetSecsMsg(new SecsMsg_S1F13ToEQ());
            secsMsg.Excute();
        }
    }
}
