using Communication.EmulCommModule;
using Communication.TCPIPCommModule;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.CassetteIDReader;
using ProberInterfaces.Communication;
using ProberInterfaces.Communication.BarcodeReader;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BarcodeReader
{
    public class BarcodeReaderModule : IBarcodeReaderModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public BarcodeReaderModule(int foupIndex)
        {
            FoupIndex = foupIndex;
        }

        public bool Initialized { get; set; } = false;

        private int _FoupIndex;
        public int FoupIndex
        {
            get { return _FoupIndex; }
            set { _FoupIndex = value; }
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
        private int _ModuleIndex;
        public int ModuleIndex
        {
            get { return _ModuleIndex; }
            set { _ModuleIndex = value; }
        }

        private BarcodeReaderSysParameters _BarcodeReaderSysParam;
        public BarcodeReaderSysParameters BarcodeReaderSysParam
        {
            get { return _BarcodeReaderSysParam; }
            set
            {
                if (value != _BarcodeReaderSysParam)
                {
                    _BarcodeReaderSysParam = value;
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

        private string _BCD_Real_ID;
        public string BCD_Real_ID
        {
            get { return _BCD_Real_ID; }
            set
            {
                if (value != _BCD_Real_ID)
                {
                    _BCD_Real_ID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsBufferClear;
        public bool IsBufferClear
        {
            get { return _IsBufferClear; }
            set
            {
                if (value != _IsBufferClear)
                {
                    _IsBufferClear = value;
                    RaisePropertyChanged();
                }
            }
        }

        //COMMAND CODE
        public string CMD_TURNON = "LON";
        public string CMD_TURNOFF = "LOFF";
        public string CMD_BCLR = "%BCLR";
        public string CMD_RESET = "%RESET";
        public string CMD_SAVE = "%SAVE"; //Save settings*
        public string CMD_LOAD = "%LOAD"; //Load saved settings
        public string CMD_DFLT = "%DFLT"; //Initialize settings

        //CR LF
        public string CR = "\r";
        public string LF = "\n";

        //COMMAND RESPONSE
        public string CMD_RSP_BCLR = "OK,%BCLR";
        public string CMD_RSP_RESET = "OK,%RESET";
        public string CMD_RSP_SAVE = "OK,%SAVE";
        public string CMD_RSP_LOAD = "OK,%LOAD";
        public string CMD_RSP_DFLT = "OK,%DFLT";

        public bool bRcvDone = false;
        public bool bCommErr = false;
        public bool bRcvErrCode = false;

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

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                if(Initialized == false)
                {
                    ModuleAttached = BarcodeReaderSysParam.BarcodeReaderParams[FoupIndex].ModuleAttached.Value;
                    ModuleCommType = BarcodeReaderSysParam.BarcodeReaderParams[FoupIndex].ModuleCommType.Value;
                    ModuleIndex = FoupIndex + 1;

                    if (ModuleCommType == EnumCommmunicationType.EMUL)
                    {
                        CommModule = new EmulCommModule(FoupIndex + 1);
                    }
                    else if(ModuleCommType == EnumCommmunicationType.TCPIP)
                    {
                        CommModule = new TCPIPCommModule(
                            BarcodeReaderSysParam.BarcodeReaderParams[FoupIndex].IP.Value,
                            BarcodeReaderSysParam.BarcodeReaderParams[FoupIndex].Port.Value);
                    }
                    if(CommModule != null)
                    {
                        retval = CommModule.InitModule();
                    }
                    if(retval == EventCodeEnum.NONE)
                    {
                        if(ModuleAttached == true)
                        {
                            retval = CommModule.Connect();
                            if(retval == EventCodeEnum.NONE)
                            {
                                CommModule.SetDataChanged += new setDataHandler(DataReceived);
                                string id = ReadCassetteID();
                                LoggerManager.Debug($"BarcodeReaderModule.InitModule() Connect Success");
                                LoggerManager.Debug($"[BarcodeReaderModule] BCD_Real_ID : {id}, " +
                                    $"[BarcodeReader Number #{FoupIndex + 1}]");
                            }
                            else
                            {
                                LoggerManager.Debug($"BarcodeReaderModule.InitModule() Connect Fail");
                            }
                        }
                    }
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

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                tmpParam = new BarcodeReaderSysParameters();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(BarcodeReaderSysParameters));

                if (RetVal == EventCodeEnum.NONE)
                {
                    BarcodeReaderSysParam = tmpParam as BarcodeReaderSysParameters;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                retval = this.SaveParameter(BarcodeReaderSysParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public void DataReceived(string receiveData)
        {
            bRcvDone = true;
            LoggerManager.Debug($"BarcdodeReaderModule.DataReceived() receiveData = {receiveData}");

            if (!string.IsNullOrEmpty(receiveData))
            {
                if (receiveData.Equals(CMD_RSP_BCLR + CR))
                {
                    IsBufferClear = true;
                    LoggerManager.Debug($"BarcdodeReaderModule.DataReceived() Buffer Clear OK");
                }
                else if (!receiveData.Contains("OK,") && !receiveData.Contains("ERROR"))
                {
                    //LON 이후 received된 데이터 없이 LOFF하게 될 경우 ERROR 를 Recevie 받음.
                    // 실제 읽은 값. 이쪽으로 들어옴. (OK를 포함하고 있지 않음. -> 나머지 Command들은 응답으로 OK를 받음)
                    if (receiveData.Contains(CR))
                    {
                        receiveData = receiveData.Replace(CR, "");
                    }
                    BCD_Real_ID = receiveData;
                }
                else
                {
                    LoggerManager.Debug($"BarcdodeReaderModule.DataReceived() Read Fail");
                }
            }
        }

        public EventCodeEnum ReInitialize()
        {
            EventCodeEnum retval = EventCodeEnum.NONE;
            try
            {
                LoggerManager.Debug($"BarcdodeReaderModule.ReInitialize()");
                bool moduleAttached = BarcodeReaderSysParam.BarcodeReaderParams[FoupIndex].ModuleAttached.Value;
                retval = CommModule.ReInitalize(moduleAttached);

                if(retval == EventCodeEnum.NONE)
                {
                    string id = ReadCassetteID();
                    LoggerManager.Debug($"BarcodeReaderModule.ReInitialize() Connect Success");
                    LoggerManager.Debug($"[BarcodeReaderModule] BCD_Real_ID : {id}, " +
                        $"[BarcodeReader Number #{FoupIndex + 1}]");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public string ReadCassetteID()
        {
            string val = "";
            try
            {
                BCD_Real_ID = "";

                if (CommModule is EmulCommModule)
                {
                    BCD_Real_ID = $"EMUL__Barcode_{FoupIndex + 1}";
                    return BCD_Real_ID;
                }
                BarcodeBufferClear();   // 바코드리더 모듈 내 버퍼 클리어
                Thread.Sleep(500);

                if (IsBufferClear)
                {
                    bRcvDone = false;

                    BarcodeSensorTurnON();  // 바코드 리더 센서 켜기
                    for (int i = 0; i < 3; i++) // 3초 동안 읽음.
                    {
                        if (bRcvDone)
                        {
                            bCommErr = false;
                            break;
                        }
                        else
                        {
                            bCommErr = true;
                        }
                        Thread.Sleep(1000);
                    }

                    if (!string.IsNullOrEmpty(BCD_Real_ID))
                    {
                        val = BCD_Real_ID;
                    }
                    else
                    {
                        val = "";
                    }

                    BarcodeSensorTurnOFF(); // 바코드 리더 센서 끄기
                }
                else
                {
                    LoggerManager.Debug($"BarcodeReaderModule.ReadCassetteID() BufferClear Fail");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return val;
        }

        public void BarcodeSensorTurnON()
        {
            try
            {
                LoggerManager.Debug($"BarcodeReaderModule.BarcodeSensorTurnON()");
                bRcvDone = false;
                CommModule.Send(CMD_TURNON + CR);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void BarcodeSensorTurnOFF()
        {
            try
            {
                LoggerManager.Debug($"BarcodeReaderModule.BarcodeSensorTurnOFF()");
                bRcvDone = false;
                CommModule.Send(CMD_TURNOFF + CR);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void BarcodeBufferClear()
        {
            try
            {
                LoggerManager.Debug($"BarcodeReaderModule.BarcodeBufferClear()");
                IsBufferClear = false;
                bRcvDone = false;
                CommModule.Send(CMD_BCLR + CR);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}