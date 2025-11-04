using BarcodeReader;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.CassetteIDReader;
using ProberInterfaces.Communication;
using ProberInterfaces.Communication.BarcodeReader;
using ProberInterfaces.Communication.RFID;
using ProberInterfaces.Enum;
using ProberInterfaces.RFID;
using RFID;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CassetteIDReader
{
    public class CassetteIDReaderModule : ICassetteIDReaderModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public CassetteIDReaderModule(int foupIndex)
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

        private CassetteIDReaderParameters _CassetteIDReaderSysParam;
        public CassetteIDReaderParameters CassetteIDReaderSysParam
        {
            get { return _CassetteIDReaderSysParam; }
            set
            {
                if (value != _CassetteIDReaderSysParam)
                {
                    _CassetteIDReaderSysParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ICSTIDReader _CSTIDReader;
        public ICSTIDReader CSTIDReader
        {
            get { return _CSTIDReader; }
            set
            {
                if (value != _CSTIDReader)
                {
                    _CSTIDReader = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IParam _CSTIDReaderParam;
        public IParam CSTIDReaderParam
        {
            get { return _CSTIDReaderParam; }
            set
            {
                if (value != _CSTIDReaderParam)
                {
                    _CSTIDReaderParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if(Initialized == false)
                {
                    if (CassetteIDReaderSysParam.CassetteIDReaderType.Value == EnumCSTIDReaderType.RFID)
                    {
                        CSTIDReader = new RFIDModule(FoupIndex);
                    }
                    else if(CassetteIDReaderSysParam.CassetteIDReaderType.Value == EnumCSTIDReaderType.BARCODE)
                    {
                        CSTIDReader = new BarcodeReaderModule(FoupIndex);
                    }

                    retval = CSTIDReader.LoadSysParameter();
                    retval = CSTIDReader.InitModule();

                    if (CassetteIDReaderSysParam.CassetteIDReaderType.Value == EnumCSTIDReaderType.RFID)
                    {
                        CSTIDReaderParam = (CSTIDReader as IRFIDModule).RFIDSysParam;
                    }
                    else if (CassetteIDReaderSysParam.CassetteIDReaderType.Value == EnumCSTIDReaderType.BARCODE)
                    {
                        CSTIDReaderParam = (CSTIDReader as IBarcodeReaderModule).BarcodeReaderSysParam;
                    }

                    Initialized = true;

                    retval = EventCodeEnum.NONE;
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
                tmpParam = new CassetteIDReaderParameters();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(CassetteIDReaderParameters));

                if (RetVal == EventCodeEnum.NONE)
                {
                    CassetteIDReaderSysParam = tmpParam as CassetteIDReaderParameters;
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
                RetVal = this.SaveParameter(CassetteIDReaderSysParam);
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
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public Task CheckConnectState()
        {
            EnumCommunicationState isConnected = EnumCommunicationState.EMUL;
            string moduleType = "";

            try
            {
                IRFIDAdapter adapter = (CSTIDReader as IRFIDModule)?.GetRFIDAdapter();

                if (CSTIDReader.ModuleAttached == true &&
                    CSTIDReader.ModuleCommType != EnumCommmunicationType.EMUL)                  
                {
                    if(adapter != null) // adapter가 null이 아니라는 것은 RIFD이면서 Multiple인 경우 
                    {
                        if (adapter.CommModule.CurState == EnumCommunicationState.CONNECTED)
                        {
                            isConnected = EnumCommunicationState.CONNECTED;
                        }
                        else
                        {
                            isConnected = EnumCommunicationState.DISCONNECT;
                        }
                    }
                    else
                    {
                        if (CSTIDReader.CommModule.CurState == EnumCommunicationState.CONNECTED)
                        {
                            isConnected = EnumCommunicationState.CONNECTED;
                        }
                        else
                        {
                            isConnected = EnumCommunicationState.DISCONNECT;
                        }
                    }
                }

                if (isConnected == EnumCommunicationState.DISCONNECT)
                {
                    if (CSTIDReader is IRFIDModule)
                    {
                        moduleType = "RFID Module";
                    }
                    else if (CSTIDReader is IBarcodeReaderModule)
                    {
                        moduleType = "Barcode Module";
                    }

                    while (!this.MetroDialogManager().MetroWindowLoaded ||
                        this.MetroDialogManager().GetMetroWindow(true) == null)
                    {
                        Thread.Sleep(100);
                    }

                    this.MetroDialogManager().ShowMessageDialog("Connection Failure", $"Cassette ID Reader Module#{CSTIDReader.ModuleIndex + 1} ({moduleType}) connection failed", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }
    }
}
