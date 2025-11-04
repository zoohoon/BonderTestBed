using SorterPickTester.Net;
using SorterPickTester.Module;
using SorterPickTester.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SorterPickTester
{
    public class MainVm : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public MainVm()
        {

        }

        public HeadBlock HeadBlock { get; set; } = new HeadBlock();


        private string _ServerAddress = "";
        public string ServerAddress
        {
            get 
            {
                if (_ServerAddress != HeadBlock._cntrPPR.IpAddress)
                    _ServerAddress = HeadBlock._cntrPPR.IpAddress;
                return _ServerAddress; 
            }
            set
            {
                if (value != _ServerAddress)
                {
                    HeadBlock._cntrPPR.IpAddress = value;
                    _ServerAddress = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _ServerPort = 50000;
        public int ServerPort
        {
            get { return _ServerPort; }
            set
            {
                if (value != _ServerPort)
                {
                    _ServerPort = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _PacketDataString = "Packet Test";
        public string PacketDataString
        {
            get { return _PacketDataString; }
            set
            {
                if (value != _PacketDataString)
                {
                    _PacketDataString = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ICommand _CommandConnect;
        public ICommand CommandConnect
        {
            get
            {
                return (this._CommandConnect) ??
                    (this._CommandConnect = new DelegateCommand(OnCommandConnect));
            }
        }
        private void OnCommandConnect()
        {
            if (!HeadBlock._cntrPPR.IsConnected)
                this.HeadBlock._cntrPPR.OnConnected = true;
            else
                this.HeadBlock._cntrPPR.OnConnected = false;
        }

        private ICommand _CommandSend;
        public ICommand CommandSend
        {
            get
            {
                return (this._CommandSend) ??
                    (this._CommandSend = new DelegateCommand(OnCommandSend));
            }
        }
        private void OnCommandSend()
        {
            HeadBlock._cntrPPR.OnSend(Encoding.ASCII.GetBytes(PacketDataString));
        }

        private ICommand _CommandServoOnZ;
        public ICommand CommandServoOnZ
        {
            get
            {
                return (this._CommandServoOnZ) ??
                    (this._CommandServoOnZ = new DelegateCommand(OnCommandServoOnZ));
            }
        }
        private void OnCommandServoOnZ()
        {
            //byte[] packetCmd = { 0x55, 0xAA };
            //byte[] packetData = { 0x32, 0x00, 0x60, 0x00, 0x01, 0x00, 0x81, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xCC, 0x00, 0x00, 0x00,
            //    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xCC };
            //
            //byte[] packet;
            //List<byte[]> datas = new List<byte[]>();
            //datas.Add(packetCmd);
            //datas.Add(packetData);
            //cntrPPR.OnMakePacket(datas, packetCmd.Length + packetData.Length, out packet);
            //
            //Console.WriteLine("Sending  Datas :" + ByteArrayToString(packet));
            ////this.tcpClient.Send(packet);
        }

        private ICommand _CommandServoOffZ;
        public ICommand CommandServoOffZ
        {
            get
            {
                return (this._CommandServoOffZ) ??
                    (this._CommandServoOffZ = new DelegateCommand(OnCommandServoOffZ));
            }
        }
        private void OnCommandServoOffZ()
        {
            //byte[] packetCmd = { 0x55, 0xAA };
            //byte[] packetData = { 0x32, 0x00, 0x60, 0x00, 0x01, 0x00, 0x81, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xCC, 0x00, 0x00, 0x00,
            //    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xCC };
            //
            //byte[] packet;
            //List<byte[]> datas = new List<byte[]>();
            //datas.Add(packetCmd);
            //datas.Add(packetData);
            //cntrPPR.OnMakePacket(datas, packetCmd.Length + packetData.Length, out packet);
            //
            //Console.WriteLine("Sending  Datas :" + ByteArrayToString(packet));
            ////this.tcpClient.Send(packet);
        }

        private ICommand _CommandHomeZ;
        public ICommand CommandHomeZ
        {
            get
            {
                return (this._CommandHomeZ) ??
                    (this._CommandHomeZ = new DelegateCommand(OnCommandHomeZ));
            }
        }
        private void OnCommandHomeZ()
        {
            //byte[] packetCmd = { 0x55, 0xAA };
            //byte[] packetData = { 0x32, 0x00, 0x60, 0x00, 0x01, 0x00, 0x81, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xCC, 0x00, 0x00, 0x00,
            //    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xCC };
            //
            //byte[] packet;
            //List<byte[]> datas = new List<byte[]>();
            //datas.Add(packetCmd);
            //datas.Add(packetData);
            //cntrPPR.OnMakePacket(datas, packetCmd.Length + packetData.Length, out packet);
            //
            //Console.WriteLine("Sending  Datas :" + ByteArrayToString(packet));
            ////this.tcpClient.Send(packet);
        }

        private ICommand _CommandSingle;
        public ICommand CommandSingle
        {
            get
            {
                return (this._CommandSingle) ??
                    (this._CommandSingle = new DelegateCommand(OnCommandSingle));
            }
        }
        private void OnCommandSingle()
        {
            Console.WriteLine("==============> OnCommandSingle()");

            HeadBlock.Send_PPR_Command_Once();
        }

        private ICommand _CommandAlarmReset;
        public ICommand CommandAlarmReset
        {
            get
            {
                return (this._CommandAlarmReset) ??
                    (this._CommandAlarmReset = new DelegateCommand(OnCommandAlarmReset));
            }
        }
        private void OnCommandAlarmReset()
        {
            Console.WriteLine("==============> OnCommandAlarmReset()");

            HeadBlock.SetFuncAlarmReset();
        }

        private ICommand _CommandServoOn;
        public ICommand CommandServoOn
        {
            get
            {
                return (this._CommandServoOn) ??
                    (this._CommandServoOn = new DelegateCommand(OnCommandServoOn));
            }
        }
        private void OnCommandServoOn()
        {
            HeadBlock.SetSeqNoServoOn();
            HeadBlock.SetFuncSeqStart();
        }

        private ICommand _CommandServoOff;
        public ICommand CommandServoOff
        {
            get
            {
                return (this._CommandServoOff) ??
                    (this._CommandServoOff = new DelegateCommand(OnCommandServoOff));
            }
        }
        private void OnCommandServoOff()
        {
            HeadBlock.SetSeqNoServoOff();
            HeadBlock.SetFuncSeqStart();
        }

        private ICommand _CommandHomeReturn;
        public ICommand CommandHomeReturn
        {
            get
            {
                return (this._CommandHomeReturn) ??
                    (this._CommandHomeReturn = new DelegateCommand(OnCommandHomeReturn));
            }
        }
        private void OnCommandHomeReturn()
        {
            HeadBlock.SetSeqNoHomeReturn();
            HeadBlock.SetFuncSeqStart();
        }

        

        private ICommand _CommandAirVacuum;
        public ICommand CommandAirVacuum
        {
            get
            {
                return (this._CommandAirVacuum) ??
                    (this._CommandAirVacuum = new DelegateCommand(OnCommandAirVacuum));
            }
        }
        private void OnCommandAirVacuum()
        {
            HeadBlock.SetFuncAirNega();
        }

        private ICommand _CommandAirRelease;
        public ICommand CommandAirRelease
        {
            get
            {
                return (this._CommandAirRelease) ??
                    (this._CommandAirRelease = new DelegateCommand(OnCommandAirRelease));
            }
        }
        private void OnCommandAirRelease()
        {
            HeadBlock.SetFuncAirStop();
        }

        private ICommand _CommandSeqStop;
        public ICommand CommandSeqStop
        {
            get
            {
                return (this._CommandSeqStop) ??
                    (this._CommandSeqStop = new DelegateCommand(OnCommandSeqStop));
            }
        }
        private void OnCommandSeqStop()
        {
            HeadBlock.SetFuncSeqStop();
        }
    }
}
