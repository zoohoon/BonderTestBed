using Autofac;
using Communication.EmulCommModule;
using Communication.SerialCommModule;
using Communication.TCPIPCommModule;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Communication;
using ProberInterfaces.Communication.RFID;
using ProberInterfaces.Foup;
using ProberInterfaces.RFID;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace RFID
{
    public class RFIDAdapter : IRFIDAdapter
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        public bool Initialized { get; set; } = false;

        private RFIDSysParameters _RFIDSysParam;
        public RFIDSysParameters RFIDSysParam
        {
            get { return _RFIDSysParam; }
            set
            {
                if (value != _RFIDSysParam)
                {
                    _RFIDSysParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        public RFIDAdapter()
        {
            var rfidModule = FoupOPModule.FoupControllers[0].Service.FoupModule.CassetteIDReaderModule.CSTIDReader as IRFIDModule;
            if(rfidModule != null)
            {
                RFIDSysParam = rfidModule.RFIDSysParam;

                if (RFIDSysParam.RFIDProtocolType == EnumRFIDProtocolType.MULTIPLE)
                {
                    if (RFIDSysParam.RFIDParams[0].ModuleCommType.Value == EnumCommmunicationType.EMUL)
                    {
                        CommModule = new EmulCommModule(0);
                    }
                    else if (RFIDSysParam.RFIDParams[0].ModuleCommType.Value == EnumCommmunicationType.SERIAL)
                    {
                        CommModule = new SerialCommModule(
                            RFIDSysParam.RFIDParams[0].SerialPort.Value,
                            RFIDSysParam.RFIDParams[0].BaudRate.Value,
                            RFIDSysParam.RFIDParams[0].DataBits.Value,
                            RFIDSysParam.RFIDParams[0].StopBitsEnum.Value);
                    }
                    else if (RFIDSysParam.RFIDParams[0].ModuleCommType.Value == EnumCommmunicationType.TCPIP)
                    {
                        CommModule = new TCPIPCommModule(
                            RFIDSysParam.RFIDParams[0].IP.Value,
                            RFIDSysParam.RFIDParams[0].Port.Value);
                    }
                }
            }
            else
            {
                LoggerManager.Debug($"RFIDModule is null");
            }
        }

        public void DeInitModule()
        {
        }

        private void DataReceived(string receiveData)
        {
            try
            {
                var rfidModule = FoupOPModule.FoupControllers[FoupIndex].Service.FoupModule.CassetteIDReaderModule.CSTIDReader as IRFIDModule;
                if (rfidModule != null)
                {
                    //TODO
                    string removeSOH = receiveData.Replace(SOH, "");
                    string ant_id = removeSOH.Substring(1,1);
                    string removeID = removeSOH.Substring(2);
                    string removeFCS = removeID.Substring(0, removeID.Length - 3) + "\r";

                    int foupIndex = Convert.ToInt32(ant_id) - 1;
                    //index = receiveData 받은 정보로 foup인덱스 구분
                    string parsingData = removeFCS;
                    //parsingData = receiveData 받은 string중 foup인덱스를 제외함
                
                    rfidModule.DataReceived(parsingData);
                
                    bRcvDone = true;
                    bRcvErrCode = false;
                }
                else
                {
                    LoggerManager.Debug($"RFIDModule is null");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private Autofac.IContainer _Container => this.GetLoaderContainer();
        private IFoupOpModule FoupOPModule => _Container.Resolve<IFoupOpModule>();

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Initialized == false)
                {
                    
                    if (CommModule != null)
                    {
                        retVal = CommModule.InitModule();
                    }
                    if (retVal == EventCodeEnum.NONE)
                    {
                        CommModule.SetDataChanged += new setDataHandler(DataReceived);
                        retVal = CommModule.Connect();
                    }

                    Initialized = true;
                    
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        //COMMAND CODE
        public string RFID_READ = "0100";
        public string RFID_WRITE = "0200";
        public string RFID_TEST = "10";
        public string RFID_NAK = "12";
        public string RFID_RESET = "7F";

        //RESPONSE CODE
        public string RFID_NORMALEND = "00";
        public string RFID_FORMATERR = "14";
        public string RFID_COMMERR = "70";
        public string RFID_VERIFERR = "71";
        public string RFID_NOTAGERR = "72";
        public string RFID_OUTSIDEWRTERR = "7B";
        public string RFID_IDSYSERR1 = "7E";
        public string RFID_IDSYSERR2 = "7F";

        //FCS
        public string ID_1_Page1_FCS = "04";
        public string ID_2_Page1_FCS = "07";
        public string ID_3_Page1_FCS = "06";
        public string ID_4_Page1_FCS = "01";
        public string ID_5_Page1_FCS = "00";
        public string ID_6_Page1_FCS = "03";
        public string ID_7_Page1_FCS = "02";
        public string ID_8_Page1_FCS = "0D";
        public string ID_1_Page1_Plus_Page2_FCS = "73";
        public string ID_2_Page1_Plus_Page2_FCS = "70";
        public string ID_3_Page1_Plus_Page2_FCS = "71";
        public string ID_4_Page1_Plus_Page2_FCS = "76";

        //Page
        public string PAGE_1 = "00000004";
        public string PAGE_2 = "00000008";
        public string PAGE_3 = "00000010";
        public string PAGE_4 = "00000020";
        public string PAGE_5 = "00000040";
        public string PAGE_6 = "00000080";
        public string PAGE_7 = "00000100";
        public string PAGE_8 = "00000200";
        public string PAGE_9 = "00000400";
        public string PAGE_10 = "00000800";
        public string PAGE_11 = "00001000";
        public string PAGE_12 = "00002000";
        public string PAGE_13 = "00004000";
        public string PAGE_14 = "00008000";
        public string PAGE_15 = "00010000";
        public string PAGE_16 = "00020000";
        public string PAGE_17 = "00030000";
        public string PAGE_1PLUS2 = "0000000C";


        /*OMRON 1:N프로토콜 설명(캔탑스 RFID)
        
        컨트롤러 1대의 최대 4개의 안테나로 1:N Read가 가능함.
        기존 1:1방식과는 다른 Command를 보내야 함.

                                                             순서대로     
        싱글(1:1) : 01000000000C + "\r\n"                      01 (01:Read, 02:Write)
                                                              0 (0:Hex, 1:ASCII)
                                                              0 (교신지정 0:싱글트리거, 1:싱글오토 ...)
                                                              0000000C (8자리는 페이지)
                                                              "\r\n" (CRLF)

        멀티(1:N) : "\u0001" + 0101000000000404 + "\r\n"       제어 문자(헤더의 시작) : "\u0001" (SOH)
                                                              안테나 번호           : 01 (01: ANT1, 02:ANT2 ...)
                                                              코드                 : 01 (01: Read, 02:Write)
                                                              응답Data             : 0 (0:Hex, 1:ASCII),
                                                              교신 지정             : 0 (교신지정 0:싱글트리거, 1:싱글오토 ...)  
                                                              Page                : 00000004 (1페이지:00000004, 2페이지:00000008, 3페이지:00000010 ...)
                                                              FCS                 :  04 (1페이지FCS:04, 2페이지FCS:08, 3페이지FCS: 01...)
                                                              제어 문자(끝)         : "\r\n" (CRLF)      

         */

        public Boolean RFID_RD_DATA(int foupIndex)
        {
            int wret;
            bool run = true;
            bool ret = false;
            try
            {
                FoupIndex = foupIndex;
                string zero = "0";
                string head_ID;
                head_ID = zero + (foupIndex + 1).ToString();

                int rtry_cnt = 0;
                while (run)
                {
                    wret = WrtComm(RFID_READ, "RFID_RD_DATA", head_ID);
                    if (wret == 1)
                    {
                        //알림창 VBTRACE "Time out in RFID_RD_DATA:"
                        if (rtry_cnt > 5)
                        {
                            ret = false;
                            run = false;
                        }
                    }
                    else if (wret == 2)
                    {
                        //알림창 VBTRACE "Comm Error! in RFID_RD_DATA:"
                        if (rtry_cnt > 5)
                        {
                            ret = false;
                            run = false;
                        }
                    }
                    else
                    {
                        ret = true;
                        run = false;
                    }
                    rtry_cnt = rtry_cnt + 1;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return ret;
        }

        private int _FoupIndex;
        public int FoupIndex
        {
            get { return _FoupIndex; }
            set
            {
                if (value != _FoupIndex)
                {
                    _FoupIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int WrtComm(String strbuf, String callerid, string head_ID)
        {
            try
            {
                //RFID command format
                // command code + Parameter + CR
                // XXXX           nnnn...nn + &h0D
                // response
                // response code + Parameter + CR
                // XX              nnnn...nn + &h0D
                //1:1 protocol type
                //RS232 setting : "9600,e,8,1"
                string strtime;
                string out_str_buf;
                string headID = head_ID;
                string FCS = null;
                if (head_ID == "01")
                {
                    FCS = ID_1_Page1_Plus_Page2_FCS;
                }
                else if (head_ID == "02")
                {
                    FCS = ID_2_Page1_Plus_Page2_FCS;
                }
                else if (head_ID == "03")
                {
                    FCS = ID_3_Page1_Plus_Page2_FCS;
                }
                else if (head_ID == "04")
                {
                    FCS = ID_4_Page1_Plus_Page2_FCS;
                }

                strtime = GetLocalTimeStr() + " ";
                gbComOccupied = true;

                bRcvDone = false;
                bCommErr = false;
                bRcvErrCode = false;

                out_str_buf = headID + strbuf + PAGE_1PLUS2 + FCS;   //1PAGE Data (1Page안에 RFID ID정보가 담겨 있음)
                CommModule.Send(SOH + out_str_buf + Environment.NewLine);

                Thread.Sleep(500);  //위에서 값을 보내고나서 receive받으면 bRcvDone이 변수를 true로 변경함. 이 sleep이 너무 짧을 경우 값을 한번 더 보내는 경우가 생김.
                Stopwatch ts = new Stopwatch(); //timer
                ts.Start();

                while (bRcvDone == false)
                {
                    Stopwatch ts2 = new Stopwatch();
                    ts2.Start();
                    if (ts.Elapsed.Seconds - ts2.Elapsed.Seconds > 1)
                    {
                        gbComOccupied = false;
                        return 1;

                        //알림창 "[E84] 1sec. Time-out"                                
                    }
                }
                if (bCommErr)
                {
                    gbComOccupied = false;
                    return 2;
                    //알림창 "[E84] Com. Error"
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return 0;
            //시스템 에러 ???

        }

        public Boolean gbComOccupied;

        /////mod RFIDmain////
        public Boolean bRcvDone;
        public Boolean bCommErr;
        public Boolean bRcvErrCode;

        public string SOH = Convert.ToString((char)1);

        public string GetLocalTimeStr()
        {
            DateTime datetime = new DateTime();

            string time = string.Format("hour : {0} ||" + "Minute: {1} || " + "Seconds: {2} ||" + "Milliseconds: {3} ",
                    datetime.Hour, datetime.Minute, datetime.Second, datetime.Millisecond);

            return time;
        }

        private ICommModule _CommModule;
        public ICommModule CommModule
        {
            get { return _CommModule; }
            set
            {
                if (value != _CommModule)
                {
                    _CommModule = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
