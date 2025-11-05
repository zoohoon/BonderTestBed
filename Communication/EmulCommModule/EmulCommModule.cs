using LogModule;
using ProberErrorCode;
using ProberInterfaces.Communication;
using ProberInterfaces.Enum;
using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Runtime.CompilerServices;
namespace Communication.EmulCommModule
{
    public class EmulCommModule : ICommModule, INotifyPropertyChanged
    {
        private EnumCommunicationState _CurState = EnumCommunicationState.EMUL;
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
        private int _FoupIndex;

        public int FoupIndex
        {
            get { return _FoupIndex; }
            set { _FoupIndex = value; }
        }

        public SerialPort Port { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        #region ==> NotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        public event setDataHandler SetDataChanged;        

        public EmulCommModule(int foupindex)
        {
            FoupIndex = foupindex;
        }

        public EventCodeEnum Connect()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public void DisConnect()
        {
        }

        public void Dispose()
        {
        }

        public string GetReceivedData()
        {
            return "";
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public EnumCommunicationState GetCommState()
        {
            return EnumCommunicationState.EMUL;
        }

        public void SetReceivedData(string receiveData)
        {
            try
            {
                SetDataChanged($"00EMUL_RFID_{FoupIndex}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void Send(string sendData)
        {
            SetReceivedData("");
            return;
        }

        public EventCodeEnum ReInitalize(bool attatch)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public void Send(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}