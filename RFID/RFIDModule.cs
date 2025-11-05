using ProberInterfaces;
using System;
using System.ComponentModel;
namespace RFID
{
    using System.Diagnostics;
    using System.Text;
    using ProberInterfaces.RFID;
    using Microsoft.VisualBasic;
    using System.Threading;
    using System.Collections.ObjectModel;
    using ProberInterfaces.Command;
    using ProberErrorCode;
    using ProberInterfaces.State;
    using LogModule;
    using System.Runtime.CompilerServices;
    using Communication.EmulCommModule;
    using Communication.SerialCommModule;
    using Communication.TCPIPCommModule;
    using ProberInterfaces.Foup;
    using Autofac;
    using LoaderBase;
    using System.IO.Ports;
    using ProberInterfaces.Communication;
    using ProberInterfaces.Communication.RFID;
    using ProberInterfaces.CassetteIDReader;
    using Communication.ModbusCommModule;

    public class RFIDModule : IRFIDModule
    {

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        public RFIDModule(int foupIndex)
        {
            FoupIndex = foupIndex;
            RFIDModuleType = EnumRFIDModuleType.FOUP; // foupIndex를 넣어주는 경우는 ModuleType을 FOUP으로 설정
        }

        public RFIDModule(EnumRFIDModuleType RFIDModuleType)
        {
            FoupIndex = 0; // RFIDModuleType 이 FOUP이 아닌 타입으로 생성하는 경우 FoupIndex를 0으로 설정
            this.RFIDModuleType = RFIDModuleType;
        }
        public bool Initialized { get; set; } = false;

        private int _FoupIndex;
        public int FoupIndex
        {
            get { return _FoupIndex; }
            set { _FoupIndex = value; }
        }

        private int _ModuleIndex;
        public int ModuleIndex
        {
            get { return _ModuleIndex; }
            set { _ModuleIndex = value; }
        }
        private bool _ModuleAttached;
        public bool ModuleAttached
        {
            get { return _ModuleAttached; }
            set
            {
                if (value != _ModuleAttached)
                {
                    _ModuleAttached = value;
                    RaisePropertyChanged();
                }
            }
        }
        private EnumCommmunicationType _ModuleCommType;
        public EnumCommmunicationType ModuleCommType
        {
            get { return _ModuleCommType; }
            set
            {
                if (value != _ModuleCommType)
                {
                    _ModuleCommType = value;
                    RaisePropertyChanged();
                }
            }
        }
        private StopBits _StopBits;
        public StopBits StopBitsEnum
        {
            get { return _StopBits; }
            set
            {
                if (value != _StopBits)
                {
                    _StopBits = value;
                    RaisePropertyChanged();
                }
            }
        }
        private string _SerialPort;
        public string SerialPort
        {
            get { return _SerialPort; }
            set
            {
                if (value != _SerialPort)
                {
                    _SerialPort = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EnumRFIDModuleType _RFIDModuleType;
        public EnumRFIDModuleType RFIDModuleType
        {
            get { return _RFIDModuleType; }
            set { _RFIDModuleType = value; }
        }

        Stopwatch stw = new Stopwatch();
        private CommandInformation _CommandInfo;
        public CommandInformation CommandInfo
        {
            get { return _CommandInfo; }
            set { _CommandInfo = value; }
        }

        private ReasonOfError _ReasonOfError = new ReasonOfError(ModuleEnum.RFID);
        public ReasonOfError ReasonOfError
        {
            get { return _ReasonOfError; }
            set
            {
                if (value != _ReasonOfError)
                {
                    _ReasonOfError = value;
                    RaisePropertyChanged();
                }
            }
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

        private IRFIDAdapter _RFIDAdapter;
        public IRFIDAdapter RFIDAdapter
        {
            get { return _RFIDAdapter; }
            set
            {
                if (value != _RFIDAdapter)
                {
                    _RFIDAdapter = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Autofac.IContainer _Container => this.GetLoaderContainer();
        private IFoupOpModule FoupOPModule => _Container?.Resolve<IFoupOpModule>();

        /////mod RFIDmain////
        public Boolean bRcvDone;
        public Boolean bCommErr;
        public Boolean bRcvErrCode;


        public double rData;
        public int gcmd_len;
        public string glast_cmd;

        public Boolean gbComOccupied;
        public int[] SETBITMASK = new int[8] { 0, 1, 2, 3, 4, 5, 6, 7 };
        public int[] RESETBITMASK = new int[8] { 0, 1, 2, 3, 4, 5, 6, 7 };


        public string RCV_STR_Buf;
        public string gRFID_Real_TAG_ID;
        public string GetLocalTimeStr()
        {
            DateTime datetime = new DateTime();

            string time = string.Format("hour : {0} ||" + "Minute: {1} || " + "Seconds: {2} ||" + "Milliseconds: {3} ",
                    datetime.Hour, datetime.Minute, datetime.Second, datetime.Millisecond);

            return time;
        }
        private CommandSlot _CommandSendSlot = new CommandSlot();
        public CommandSlot CommandSendSlot
        {
            get { return _CommandSendSlot; }
            set { _CommandSendSlot = value; }
        }
        
        //로그
        private StringBuilder Log_Strings;
        //로그 객체
        private String LogStrings
        {
            set
            {
                if (Log_Strings == null)
                    Log_Strings = new StringBuilder(1024);
                //로그 1024 제한
                if (Log_Strings.Length >= (1024 - value.Length))
                    Log_Strings.Clear();
                //로그 추가 및 화면 표시
                Log_Strings.AppendLine(value);

            }
        }
        
        public void DataReceived(string receiveData)
        {
            if(CommModule is ModbusCommModule)
            {
                RCV_STR_Buf = receiveData;
                bRcvDone = true;
                bRcvErrCode = false;
                return;
            }

            string rcstrbuf = receiveData;
            
            LogStrings = String.Format("[RIFD_RECV] {0}", rcstrbuf);
            string compstr;
            string tmprcvstr = null;
            if (Strings.InStr(1, rcstrbuf, " ") == 0)
            {
                tmprcvstr = rcstrbuf;
            }
            else
            {
                tmprcvstr = Strings.Mid(tmprcvstr, 1, 2);
            }
            compstr = Strings.Mid(tmprcvstr, 1, 2);

            if (compstr == RFID_NORMALEND)
            {
                RCV_STR_Buf = Strings.Mid(tmprcvstr, 3, Strings.Len(tmprcvstr) - 2);
                bRcvDone = true;
                bRcvErrCode = false;
                //RFID_TAG_ID = RFID_Decode();
                //LoggerManager.Debug($"[RFIDModule] RFID_TAG_ID : {RFID_TAG_ID}");
            }
            else
            {
                //알림창  VBTRACE "Abnormal response code : " + compstr
                bRcvDone = true;
                bRcvErrCode = true;
            }
        }

        private ModuleStateBase _ModuleState;
        public ModuleStateBase ModuleState
        {
            get { return _ModuleState; }
            set
            {
                if (value != _ModuleState)
                {
                    _ModuleState = value;
                    RaisePropertyChanged();
                }
            }
        }
        public IInnerState PreInnerState { get; set; }

        private ObservableCollection<TransitionInfo> _TransitionInfo = new ObservableCollection<TransitionInfo>();
        public ObservableCollection<TransitionInfo> TransitionInfo
        {
            get { return _TransitionInfo; }
            set
            {
                if (value != _TransitionInfo)
                {
                    _TransitionInfo = value;
                    RaisePropertyChanged();
                }
            }
        }


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

        private CommandSlot _CommandRecvSlot = new CommandSlot();
        public CommandSlot CommandRecvSlot
        {
            get { return _CommandRecvSlot; }
            set { _CommandRecvSlot = value; }
        }

        private CommandSlot _CommandProcSlot = new CommandSlot();
        public CommandSlot CommandRecvProcSlot
        {
            get { return _CommandProcSlot; }
            set { _CommandProcSlot = value; }
        }

        private CommandSlot _CommandRecvDoneSlot = new CommandSlot();
        public CommandSlot CommandRecvDoneSlot
        {
            get { return _CommandRecvDoneSlot; }
            set { _CommandRecvDoneSlot = value; }
        }


        private CommandTokenSet _RunTokenSet;

        public CommandTokenSet RunTokenSet
        {
            get { return _RunTokenSet; }
            set { _RunTokenSet = value; }
        }
        private IProbeCommandToken _RequestToken;
        public IProbeCommandToken RequestToken
        {
            get { return _RequestToken; }
            set { _RequestToken = value; }
        }

        private EnumModuleForcedState _ForcedDone = EnumModuleForcedState.Normal;
        public EnumModuleForcedState ForcedDone
        {
            get { return _ForcedDone; }
            set { _ForcedDone = value; }
        }

        public string RFID_cont_READID()
        {
            string val = "";
            try
            {
                ILoaderSupervisor loaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();

                if (RFIDModuleType == EnumRFIDModuleType.FOUP)
                {
                    if (RFIDSysParam.RFIDProtocolType == EnumRFIDProtocolType.SINGLE)
                    {
                        if (CommModule is EmulCommModule)
                        {
                            gRFID_Real_TAG_ID = $"EMUL__RFID_{FoupIndex + 1}";
                            val = gRFID_Real_TAG_ID;
                        }
                        else
                        {
                            if (RFID_RD_DATA() == true)
                            {
                                if (bRcvErrCode == false)
                                {
                                    val = RFID_Decode();

                                }
                                else
                                {
                                    val = "";
                                }
                            }
                            else
                            {
                                val = "";
                            }
                        }
                    }
                    else if (RFIDSysParam.RFIDProtocolType == EnumRFIDProtocolType.MULTIPLE)
                    {
                        RFIDAdapter = GetRFIDAdapter();
                        if (RFIDAdapter.CommModule is EmulCommModule)
                        {
                            gRFID_Real_TAG_ID = $"EMUL__RFID_{FoupIndex + 1}";
                            return gRFID_Real_TAG_ID;
                        }
                        if (RFIDAdapter.RFID_RD_DATA(FoupIndex) == true)
                        {
                            if (bRcvErrCode == false)
                            {
                                val = RFID_Decode();
                            }
                            else
                            {
                                val = "";
                            }
                        }
                        else
                        {
                            val = "";
                        }
                    }

                    loaderMaster.Loader.Foups[FoupIndex].CassetteID = val;
                }
                else if (RFIDModuleType == EnumRFIDModuleType.PROBECARD)
                {
                    if (CommModule is EmulCommModule)
                    {
                        gRFID_Real_TAG_ID = $"EMUL__RFID__Card";
                        val = gRFID_Real_TAG_ID;
                    }
                    else
                    {
                        if (RFID_RD_DATA() == true)
                        {
                            if (bRcvErrCode == false)
                            {
                                val = RFID_Decode();

                            }
                            else
                            {
                                val = "";
                            }
                        }
                        else
                        {
                            val = "";
                        }
                    }
                }
                else
                {
                    LoggerManager.Debug($"RFID_cont_READID() RFIDModuleType is invalid");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return val;
        }

        public IRFIDAdapter GetRFIDAdapter()
        {
            IRFIDAdapter retVal = null;
            try
            {
                retVal = FoupOPModule?.RFIDAdapter;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void ClearRFID()
        {
            try
            {
                ILoaderSupervisor loaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                loaderMaster.Loader.Foups[FoupIndex].CassetteID = "";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //private void Class_Terminate()
        //{
        //    try
        //    {
        //        if (gclsRFID_Online == true)
        //        {
        //            Comm_Disconnect();
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //}
        public void Create_Bit_Mask()
        {
            try
            {
                SETBITMASK[0] = 0x01;
                SETBITMASK[1] = 0x02;
                SETBITMASK[2] = 0x04;
                SETBITMASK[3] = 0x08;
                SETBITMASK[4] = 0x10;
                SETBITMASK[5] = 0x20;
                SETBITMASK[6] = 0x40;
                SETBITMASK[7] = 0x80;
                RESETBITMASK[0] = 0xFE;
                RESETBITMASK[1] = 0xFD;
                RESETBITMASK[2] = 0xFB;
                RESETBITMASK[3] = 0xF7;
                RESETBITMASK[4] = 0xEF;
                RESETBITMASK[5] = 0xDF;
                RESETBITMASK[6] = 0xBF;
                RESETBITMASK[7] = 0x7F;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public string TrimNull(string strRaw)
        {
            int iChr0;
            string strtmp;
            strtmp = strRaw;
            iChr0 = Strings.InStr(strtmp, "0");
            if (iChr0 == 0)
            {
                return Strings.Trim(Strings.Left(strtmp, iChr0 - 1));
            }
            else
            {
                return Strings.Trim(strtmp);
            }

        }
        public string Parse(string sString, int iReq, string sDelim, bool bNONE = false)
        {
            //string[] tempStr;
            string sSt;
            int iCnt;
            int iPos;
            if (sDelim.Length == 0)
            {
                sDelim = ",";
            }
            sSt = sString + sDelim;
            for (iCnt = 1; iCnt < iReq + 1; iCnt++)
            {
                iPos = Strings.InStr(sSt, sDelim);

                if (iPos == 0)
                {
                    if (iCnt == iReq)
                    {
                        return Strings.Trim(Strings.Left(sSt, iPos - 1));
                    }
                    if (iPos == sSt.Length)
                    {
                        if (bNONE == true)
                        {
                            return "NONE";
                        }
                        else
                        {
                            return "";
                        }
                    }
                    sSt = Strings.Mid(sSt, iPos + sDelim.Length);
                }
                else
                {
                    return Strings.Trim(sSt);
                }
            }
            return sSt;
        }

        /////clsRFIDFunctions//
        public bool gclsRFID_Online;
        private void Class_Initialize()
        {
            bRcvDone = false;
            bCommErr = false;
            gRFID_Real_TAG_ID = "";
            RCV_STR_Buf = "";
            gclsRFID_Online = false;


        }
        /////modRFIDCMDs//////

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

        public Boolean RFID_RD_DATA()
        {
            int wret;
            bool run = true;
            bool ret = false;
            try
            {
                int rtry_cnt = 0;
                while (run)
                {
                    wret = WrtComm(RFID_READ, "RFID_RD_DATA");

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
        public string RFID_Decode()
        {
            try
            {
                if (CommModule is ModbusCommModule)
                {
                    gRFID_Real_TAG_ID = RCV_STR_Buf;
                    return gRFID_Real_TAG_ID;
                }

                string tmpstr;
                int tmpstrlen;
                int tmpmaxbytelen = 0;
                tmpstr = RCV_STR_Buf;

                if (!string.IsNullOrEmpty(tmpstr))
                {
                    tmpstrlen = RCV_STR_Buf.Length;
                    tmpmaxbytelen = tmpstrlen / 2;

                    byte[] tmpbytearr = new byte[tmpmaxbytelen];
                    string tmpdecodedstr;
                    int i;

                    tmpdecodedstr = "";
                    for (i = 0; i < tmpmaxbytelen; i += 2)
                    {
                        tmpbytearr[i] = (Byte)Conversion.Val("&h" + Strings.Mid(tmpstr, i + 1, 2));
                        tmpdecodedstr = tmpdecodedstr + Strings.Chr(tmpbytearr[i]);
                    }
                    //gRFID_Real_TAG_ID = TrimNull(tmpdecodedstr);
                    gRFID_Real_TAG_ID = Strings.Trim(tmpdecodedstr);
                    if (CommModule is EmulCommModule)
                    {
                        gRFID_Real_TAG_ID = RCV_STR_Buf;
                    }
                }
                else
                {
                    LoggerManager.Debug($"[RFID_Multiple] Module #{FoupIndex + 1} RCV_STR_Buf is null.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return gRFID_Real_TAG_ID;
        }

        ///frmRFIDcom//////
        public int WrtComm(String strbuf, String callerid)
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

                strtime = GetLocalTimeStr() + " ";
                gbComOccupied = true;

                bRcvDone = false;
                bCommErr = false;
                bRcvErrCode = false;

                out_str_buf = strbuf + "0000000C";   //2PAGE DATA     ''//Hex(&H1) + "01" + "10" + "12345678" + "08" ;
                //Ports.Write(out_str_buf + Environment.NewLine); //MSComm1.Output = out_str_buf + vbCrLf
               
                // Modbus 타입인 경우 Data를 Read 하기 위한 Parameter를 Update 해줘야함
                if(CommModule is ModbusCommModule)
                {
                    var ModbusRFIDCommModule = CommModule as ModbusCommModule;

                    switch(RFIDModuleType)
                    {
                        case EnumRFIDModuleType.FOUP:
                            ModbusRFIDCommModule.RegisterNum = RFIDSysParam.RFIDParams[FoupIndex].ModbusRegisterNumber.Value;
                            ModbusRFIDCommModule.WordCount = RFIDSysParam.RFIDParams[FoupIndex].ModbusWordCount.Value;
                            break;
                        case EnumRFIDModuleType.PROBECARD:
                            ModbusRFIDCommModule.RegisterNum = RFIDSysParam.ProbeCardRFIDParams[0].ModbusRegisterNumber.Value;
                            ModbusRFIDCommModule.WordCount = RFIDSysParam.ProbeCardRFIDParams[0].ModbusWordCount.Value;
                            break;
                        default:
                            break;
                    }
                }

                CommModule.Send(out_str_buf + Environment.NewLine);

                //알림창 "[E84] Transfering cmd : " + out_str_buf
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

        public string Mid(string Text, int Startint, int Endint)
        {
            string ConvertText;
            try
            {
                if (Startint < Text.Length || Endint < Text.Length)
                {
                    ConvertText = Text.Substring(Startint, Endint);
                    return ConvertText;
                }
                else
                {
                    return Text;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    switch (RFIDModuleType)
                    {
                        case EnumRFIDModuleType.FOUP:
                            {
                                if (RFIDSysParam.RFIDParams.Count >= FoupIndex + 1)
                                {
                                    ModuleAttached = RFIDSysParam.RFIDParams[FoupIndex].ModuleAttached.Value;
                                    ModuleCommType = RFIDSysParam.RFIDParams[FoupIndex].ModuleCommType.Value;
                                    StopBitsEnum = RFIDSysParam.RFIDParams[FoupIndex].StopBitsEnum.Value;
                                    SerialPort = RFIDSysParam.RFIDParams[FoupIndex].SerialPort.Value;
                                    if (RFIDSysParam.RFIDProtocolType == EnumRFIDProtocolType.SINGLE)
                                    {
                                        if (ModuleAttached == true)
                                        {
                                            if (ModuleCommType == EnumCommmunicationType.EMUL)
                                            {
                                                CommModule = new EmulCommModule(FoupIndex + 1);
                                            }
                                            else if (ModuleCommType == EnumCommmunicationType.SERIAL)
                                            {
                                                CommModule = new SerialCommModule(
                                                    RFIDSysParam.RFIDParams[FoupIndex].SerialPort.Value,
                                                    RFIDSysParam.RFIDParams[FoupIndex].BaudRate.Value,
                                                    RFIDSysParam.RFIDParams[FoupIndex].DataBits.Value,
                                                    RFIDSysParam.RFIDParams[FoupIndex].StopBitsEnum.Value);
                                            }
                                            else if (ModuleCommType == EnumCommmunicationType.TCPIP)
                                            {
                                                CommModule = new TCPIPCommModule(
                                                    RFIDSysParam.RFIDParams[FoupIndex].IP.Value,
                                                    RFIDSysParam.RFIDParams[FoupIndex].Port.Value);
                                            }
                                            else if (ModuleCommType == EnumCommmunicationType.MODBUS) // Modbus 타입 추가
                                            {
                                                CommModule = new ModbusCommModule(
                                                    RFIDSysParam.RFIDParams[FoupIndex].IP.Value,
                                                    RFIDSysParam.RFIDParams[FoupIndex].Port.Value,
                                                    RFIDSysParam.RFIDParams[FoupIndex].ModbusReadDataType.Value,
                                                    RFIDSysParam.RFIDParams[FoupIndex].ModbusRegisterNumber.Value,
                                                    RFIDSysParam.RFIDParams[FoupIndex].ModbusWordCount.Value);
                                            }
                                            if (CommModule != null)
                                            {
                                                retval = CommModule.InitModule();
                                            }
                                            if (retval == EventCodeEnum.NONE)
                                            {
                                                CommModule.SetDataChanged += new setDataHandler(DataReceived);
                                                retval = CommModule.Connect();
                                            }
                                        }
                                        else
                                        {
                                            LoggerManager.Debug($"RFIDModule.InitModule ModuleAttached is False");
                                            retval = EventCodeEnum.NONE;
                                        }
                                    }
                                }
                                Initialized = true;
                                break;
                            }
                        case EnumRFIDModuleType.PROBECARD:
                            {
                                ModuleAttached = RFIDSysParam.ProbeCardRFIDParams[0].ModuleAttached.Value;
                                ModuleCommType = RFIDSysParam.ProbeCardRFIDParams[0].ModuleCommType.Value;
                                if (ModuleAttached == true)
                                {
                                    if (ModuleCommType == EnumCommmunicationType.EMUL)
                                    {
                                        CommModule = new EmulCommModule(1);
                                    }
                                    else if (ModuleCommType == EnumCommmunicationType.SERIAL)
                                    {
                                        CommModule = new SerialCommModule(
                                            RFIDSysParam.ProbeCardRFIDParams[0].SerialPort.Value,
                                            RFIDSysParam.ProbeCardRFIDParams[0].BaudRate.Value,
                                            RFIDSysParam.ProbeCardRFIDParams[0].DataBits.Value,
                                            RFIDSysParam.ProbeCardRFIDParams[0].StopBitsEnum.Value);
                                    }
                                    else if (ModuleCommType == EnumCommmunicationType.TCPIP)
                                    {
                                        CommModule = new TCPIPCommModule(
                                            RFIDSysParam.ProbeCardRFIDParams[0].IP.Value,
                                            RFIDSysParam.ProbeCardRFIDParams[0].Port.Value);
                                    }
                                    else if (ModuleCommType == EnumCommmunicationType.MODBUS) // Modbus 타입 추가
                                    {
                                        CommModule = new ModbusCommModule(
                                            RFIDSysParam.ProbeCardRFIDParams[0].IP.Value,
                                            RFIDSysParam.ProbeCardRFIDParams[0].Port.Value,
                                            RFIDSysParam.ProbeCardRFIDParams[0].ModbusReadDataType.Value,
                                            RFIDSysParam.ProbeCardRFIDParams[0].ModbusRegisterNumber.Value,
                                            RFIDSysParam.ProbeCardRFIDParams[0].ModbusWordCount.Value);
                                    }

                                    if (CommModule != null)
                                    {
                                        retval = CommModule.InitModule();
                                        if (retval == EventCodeEnum.NONE)
                                        {
                                            CommModule.SetDataChanged += new setDataHandler(DataReceived);
                                            retval = CommModule.Connect();
                                            if (retval == EventCodeEnum.NONE)
                                            {
                                                LoggerManager.Debug($"[RFIDModule] Success to connecting RFID Module.");
                                            }
                                            else
                                            {
                                                LoggerManager.Debug($"[RFIDModule] Failed to connecting RFID Module. EventCodeEnum : {retval}");
                                            }
                                        }
                                        else
                                        {
                                            LoggerManager.Debug($"[RFIDModule] Failed to InitModule. EventCodeEnum : {retval}");
                                        }
                                    }
                                    else
                                    {
                                        LoggerManager.Debug($"[RFIDModule] RFIDCommModule is null!");
                                    }
                                }
                                else
                                {
                                    LoggerManager.Debug($"RFIDModule.InitModule ModuleAttached is False");
                                    retval = EventCodeEnum.NONE;
                                }
                                Initialized = true;
                                break;
                            }
                        default:
                            {
                                Initialized = false;
                                break;
                            }
                    }
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
        public EventCodeEnum ReInitialize()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                LoggerManager.Debug($"[RFIDModule] ReInitialize()");
                if(RFIDModuleType == EnumRFIDModuleType.FOUP)
                {
                    if (RFIDSysParam.RFIDProtocolType == EnumRFIDProtocolType.SINGLE)
                    {
                        if (CommModule == null)
                        {
                            retval = CommModuleInitialize();
                            if (retval == EventCodeEnum.NONE)
                            {
                                retval = CommModule.ReInitalize(ModuleAttached);
                            }
                            else
                            {
                                LoggerManager.Debug($"[RFIDModule]RFIDModule is initialize fail. [RFIDModule Number #{ModuleIndex}], return value = {retval}");
                            }
                        }
                        else
                        {
                            retval = CommModule.ReInitalize(ModuleAttached);
                        }

                        if (retval == EventCodeEnum.NONE)
                        {
                            gRFID_Real_TAG_ID = ReadCassetteID();
                            LoggerManager.Debug($"[RFIDModule] RFID_TAG_ID : {gRFID_Real_TAG_ID}, [RFIDModule Number #{ModuleIndex}]");
                        }

                        RFIDSysParam.RFIDParams[FoupIndex].ModuleAttached.Value = ModuleAttached;
                        SaveSysParameter();
                    }
                    else if (RFIDSysParam.RFIDProtocolType == EnumRFIDProtocolType.MULTIPLE)
                    {
                        var RFIDAdapter = GetRFIDAdapter();

                        if (RFIDAdapter != null)
                        {
                            if (RFIDAdapter.CommModule != null)
                            {
                                retval = RFIDAdapter.CommModule.ReInitalize(ModuleAttached);
                                if (retval == EventCodeEnum.NONE)
                                {
                                    gRFID_Real_TAG_ID = ReadCassetteID();
                                    LoggerManager.Debug($"[RFIDModule] RFID_TAG_ID : {gRFID_Real_TAG_ID}, [RFIDModule Number #{ModuleIndex}]");
                                }
                                else
                                {
                                    ModuleAttached = false;
                                    LoggerManager.Debug($"[RFIDModule]RFIDModule is initialize fail. return value = {retval}");
                                }
                            }
                            else
                            {
                                ModuleAttached = false;
                                LoggerManager.Debug($"[RFIDModule]RFIDAdapter.RFIDCommModule is null. RFIDAdapter.RFIDCommModule = {RFIDAdapter.CommModule}");
                            }
                        }
                        RFIDSysParam.RFIDParams[FoupIndex].ModuleAttached.Value = ModuleAttached;
                        SaveSysParameter();
                    }
                }
                else if(RFIDModuleType == EnumRFIDModuleType.PROBECARD)
                {
                    if (CommModule == null)
                    {
                        retval = CommModuleInitialize();
                        if (retval == EventCodeEnum.NONE)
                        {
                            retval = CommModule.ReInitalize(ModuleAttached);
                        }
                        else
                        {
                            LoggerManager.Debug($"[RFIDModule]RFIDModule is initialize fail. [RFIDModule Number #{ModuleIndex}], return value = {retval}");
                        }
                    }
                    else
                    {
                        retval = CommModule.ReInitalize(ModuleAttached);
                    }

                    if (retval == EventCodeEnum.NONE)
                    {
                        gRFID_Real_TAG_ID = ReadCassetteID();
                        LoggerManager.Debug($"[RFIDModule] RFID_TAG_ID : {gRFID_Real_TAG_ID}, [RFIDModule Number #{ModuleIndex}]");
                    }

                    RFIDSysParam.RFIDParams[FoupIndex].ModuleAttached.Value = ModuleAttached;
                    SaveSysParameter();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        private void RFIDCommModule_SetDataChanged(string receiveData)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                tmpParam = new RFIDSysParameters();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(RFIDSysParameters));

                if (RetVal == EventCodeEnum.NONE)
                {
                    RFIDSysParam = tmpParam as RFIDSysParameters;
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
                RetVal = this.SaveParameter(RFIDSysParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return RetVal;
        }

        public void DeInitModule()
        {
            try
            {
                CommModule.Dispose();
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum CommModuleInitialize()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                ModuleIndex = FoupIndex + 1;
                if (RFIDSysParam.RFIDParams.Count >= FoupIndex + 1)
                {
                    if (RFIDSysParam.RFIDProtocolType == EnumRFIDProtocolType.SINGLE)
                    {
                        if (ModuleCommType == EnumCommmunicationType.EMUL)
                        {
                            CommModule = new EmulCommModule(FoupIndex + 1);
                        }
                        else if (ModuleCommType == EnumCommmunicationType.SERIAL)
                        {
                            CommModule = new SerialCommModule(
                                RFIDSysParam.RFIDParams[FoupIndex].SerialPort.Value,
                                RFIDSysParam.RFIDParams[FoupIndex].BaudRate.Value,
                                RFIDSysParam.RFIDParams[FoupIndex].DataBits.Value,
                                RFIDSysParam.RFIDParams[FoupIndex].StopBitsEnum.Value);
                        }
                        else if (ModuleCommType == EnumCommmunicationType.TCPIP)
                        {
                            CommModule = new TCPIPCommModule(
                                RFIDSysParam.RFIDParams[FoupIndex].IP.Value,
                                RFIDSysParam.RFIDParams[FoupIndex].Port.Value);
                        }
                        if (CommModule != null)
                        {
                            RetVal = CommModule.InitModule();
                        }
                        if (RetVal == EventCodeEnum.NONE)
                        {
                            CommModule.SetDataChanged += new setDataHandler(DataReceived);
                            if (ModuleAttached == true)
                            {
                                RetVal = CommModule.Connect();
                                ReadCassetteID();
                                LoggerManager.Debug($"[RFIDModule] RFID_TAG_ID : {gRFID_Real_TAG_ID}, [RFIDModule Number #{ModuleIndex}]");
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return RetVal;
        }

        public string ReadCassetteID()
        {
            return RFID_cont_READID();
        }
    }
}
