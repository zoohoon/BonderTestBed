using SECS_Host.Help;
using SECS_Host.Model;
using System.Windows.Input;
using XComPro.Library;

namespace SECS_Host.View_Model
{
    public class VM_DialogChangeConnInfo : ViewModelBase
    {
        public delegate void CloseDelegate(object sender, System.EventArgs e);

        private ConnectInfo _cInfo;
        private XComProW m_XComPro;
        public CloseDelegate CloseWindow;

        public ConnectInfo ConnInfo
        {
            get { return _cInfo; }
            set
            {
                if (_cInfo != value)
                {
                    _cInfo = value;
                    NotifyPropertyChange("cInfo");
                }
            }
        }

        public VM_DialogChangeConnInfo(XComProW _XComPro)
        {
            ConnInfo = new ConnectInfo();
            m_XComPro = _XComPro;
            GetConnectInfo();
        }

        #region ==> ApplyConnInfoCommand
        private ICommand _ApplyConnInfoCommand;
        public ICommand ApplyConnInfoCommand
        {
            get
            {
                if (_ApplyConnInfoCommand == null)
                    _ApplyConnInfoCommand = new RelayCommand(ApplyConnInfo);
                return _ApplyConnInfoCommand;
            }
        }
        private void ApplyConnInfo(object obj)
        {
            int retVal;
            int i = 0;

            retVal = m_XComPro.SetParam("Device ID", ConnInfo.DeviceID);
            retVal = m_XComPro.SetParam("IP", ConnInfo.IP);
            retVal = m_XComPro.SetParam("Port", ConnInfo.Port.ToString());
            retVal = m_XComPro.SetParam("Retry Limit", ConnInfo.RetryLimit.ToString());
            retVal = m_XComPro.SetParam("Link Test Interval", ConnInfo.LinkTestInterval.ToString());
            retVal = m_XComPro.SetParam("Active", ConnInfo.ConnectMode.Equals("Active") ? "true" : "false");

            foreach (var v in ConnInfo.TimeoutTimeList)
                m_XComPro.SetParam("T" + ++i, v.ToString());

            CloseWindow(this, null);
        }
        #endregion

        #region ==> MakeDefaultCommand
        private ICommand _MakeDefaultCommand;
        public ICommand MakeDefaultCommand
        {
            get
            {
                if (_MakeDefaultCommand == null)
                    _MakeDefaultCommand = new RelayCommand(MakeDefault);
                return _MakeDefaultCommand;
            }
        }
        private void MakeDefault(object obj)
        {
            //미구현
        }
        #endregion

        public void GetConnectInfo()
        {
            ConnInfo.DeviceID = m_XComPro.GetParam("Device ID");
            ConnInfo.IP = m_XComPro.GetParam("IP");
            ConnInfo.Port = int.Parse(m_XComPro.GetParam("Port"));
            ConnInfo.RetryLimit = int.Parse(m_XComPro.GetParam("Retry Limit"));
            ConnInfo.LinkTestInterval = int.Parse(m_XComPro.GetParam("Link Test Interval"));
            ConnInfo.ConnectMode = m_XComPro.GetParam("Active").Equals("true") ? "Active" : "Passive";

            ConnInfo.TimeoutTimeList.Add(new TimeOut() { Name = "T1", TimeOutTime = int.Parse(m_XComPro.GetParam("T1")) });
            ConnInfo.TimeoutTimeList.Add(new TimeOut() { Name = "T2", TimeOutTime = int.Parse(m_XComPro.GetParam("T2")) });
            ConnInfo.TimeoutTimeList.Add(new TimeOut() { Name = "T3", TimeOutTime = int.Parse(m_XComPro.GetParam("T3")) });
            ConnInfo.TimeoutTimeList.Add(new TimeOut() { Name = "T4", TimeOutTime = int.Parse(m_XComPro.GetParam("T4")) });
            ConnInfo.TimeoutTimeList.Add(new TimeOut() { Name = "T5", TimeOutTime = int.Parse(m_XComPro.GetParam("T5")) });
            ConnInfo.TimeoutTimeList.Add(new TimeOut() { Name = "T6", TimeOutTime = int.Parse(m_XComPro.GetParam("T6")) });
            ConnInfo.TimeoutTimeList.Add(new TimeOut() { Name = "T7", TimeOutTime = int.Parse(m_XComPro.GetParam("T7")) });
            ConnInfo.TimeoutTimeList.Add(new TimeOut() { Name = "T8", TimeOutTime = int.Parse(m_XComPro.GetParam("T8")) });
        }
    }
}
