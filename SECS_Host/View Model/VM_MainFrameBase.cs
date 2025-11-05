using SECS_Host.Help;
using SECS_Host.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml;
using System.Xml.Serialization;
using XComPro.Library;

namespace SECS_Host.View_Model
{
    public class VM_MainFrameBase : ViewModelBase
    {
        private SECS_HostOperator secs_HostOperator;
        public SECS_HostOperator Secs_HostOperator
        {
            get { return secs_HostOperator; }
            set { secs_HostOperator = value; }
        }

        private XComProW m_XComPro;

        private ObservableCollection<string> logCollection;
        public ObservableCollection<string> LogCollection
        {
            get { return logCollection; }
            set
            {
                logCollection = value;
                NotifyPropertyChange(nameof(LogCollection));
            }
        }

        private bool enableMenuItemConnInfo;
        public bool EnableMenuItemConnInfo
        {
            get { return enableMenuItemConnInfo; }
            set
            {
                enableMenuItemConnInfo = value;
                NotifyPropertyChange(nameof(EnableMenuItemConnInfo));
            }
        }

        private bool enablebtnStopHost;
        public bool EnablebtnStopHost
        {
            get { return enablebtnStopHost; }
            set
            {
                enablebtnStopHost = value;
                NotifyPropertyChange(nameof(EnablebtnStopHost));
            }
        }

        private int selectedIndex;
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                selectedIndex = value;
                NotifyPropertyChange(nameof(SelectedIndex));
            }
        }

        private String _StartHostButtonStr;
        public String StartHostButtonStr
        {
            get { return _StartHostButtonStr; }
            set
            {
                _StartHostButtonStr = value;
                NotifyPropertyChange(nameof(StartHostButtonStr));
            }
        }

        private String _SetTimeStr;
        public String SetTimeStr
        {
            get { return _SetTimeStr; }
            set
            {
                _SetTimeStr = value;
                NotifyPropertyChange(nameof(SetTimeStr));
            }
        }

        private String _RemoteCMDStr;
        public String RemoteCMDStr
        {
            get { return _RemoteCMDStr; }
            set
            {
                _RemoteCMDStr = value;
                NotifyPropertyChange(nameof(RemoteCMDStr));
            }
        }

        private readonly String _StartStopLbStr;
        public String StartStopLbStr
        {
            get { return _StartStopLbStr; }
        }

        private readonly String _SetOnlineLbStr;
        public String SetOnlineLbStr
        {
            get { return _SetOnlineLbStr; }
        }

        private readonly String _SetOfflineLbStr;
        public String SetOfflineLbStr
        {
            get { return _SetOfflineLbStr; }
        }

        private readonly String _AreYouThereLbStr;
        public String AreYouThereLbStr
        {
            get { return _AreYouThereLbStr; }
        }

        private readonly String _GetStatusNamelistLbStr;
        public String GetStatusNamelistLbStr
        {
            get { return _GetStatusNamelistLbStr; }
        }

        private readonly String _RequestEquipSVLbStr;
        public String RequestEquipSVLbStr
        {
            get { return _RequestEquipSVLbStr; }
        }

        private readonly String _ChangeECVLbStr;
        public String ChangeECVLbStr
        {
            get { return _ChangeECVLbStr; }
        }

        private readonly String _RequestDateTimeLBStr;
        public String RequestDateTimeLBStr
        {
            get { return _RequestDateTimeLBStr; }
        }

        private readonly String _SetTimeGBoxStr;
        public String SetTimeGBoxStr
        {
            get { return _SetTimeGBoxStr; }
        }

        private String _DynamicReportPath;
        public String DynamicReportPath
        {
            get { return _DynamicReportPath; }
            set
            {
                if(_DynamicReportPath != value)
                {
                    _DynamicReportPath = value;

                    XmlSerializer serializer = new XmlSerializer(typeof(GemXmlParam));
                    gemPathParam.DynamicReportPath = value;
                    StreamWriter writer = new StreamWriter(@"C:\ProberSystem\Parameters\GemPathParam.xml");
                    serializer.Serialize(writer, gemPathParam);
                    writer.Close();

                    NotifyPropertyChange(nameof(DynamicReportPath));
                }
            }
        }

        private GemXmlParam gemPathParam;

        public VM_MainFrameBase()
        {
            if (!Directory.Exists(@"C:\ProberSystem\Parameters\"))
            {
                System.IO.Directory.CreateDirectory(@"C:\ProberSystem\Parameters\");
            }

            if (!File.Exists(@"C:\ProberSystem\Parameters\GemPathParam.xml"))
            {
                GemXmlParam initGemParam = new GemXmlParam() { CfgPath = @"C:\ProberSystem\Utility\GEM\HOST.CFG", DynamicReportPath = ""};
                XmlSerializer serializer = new XmlSerializer(typeof(GemXmlParam));
                StreamWriter writer = new StreamWriter(@"C:\ProberSystem\Parameters\GemPathParam.xml");
                serializer.Serialize(writer, initGemParam);
                writer.Close();
            }

            XmlSerializer deSerializer = new XmlSerializer(typeof(GemXmlParam));
            FileStream fs = new FileStream(@"C:\ProberSystem\Parameters\GemPathParam.xml", FileMode.Open);
            gemPathParam = (GemXmlParam)deSerializer.Deserialize(fs);
            fs.Close();

                           m_XComPro    = new XComProW();
                   secs_HostOperator    = new SECS_HostOperator(m_XComPro, gemPathParam.CfgPath);
            secs_HostOperator.AddLog    = AddLog;
                       logCollection    = new ObservableCollection<string>();
              EnableMenuItemConnInfo    = true;
                  StartHostButtonStr    = "UnConnected";
                          SetTimeStr    = "2017111111111111";
                     _StartStopLbStr    = "";
                     _SetOnlineLbStr    = " ▷S1F17";
                    _SetOfflineLbStr    = " ▷S1F15";
                   _AreYouThereLbStr    = " ▷S1F1";

             _GetStatusNamelistLbStr    = " ▷S1F11";
                _RequestEquipSVLbStr    = " ▷S1F3, S2F23";
                     _ChangeECVLbStr    = " ▷S2F15";
               _RequestDateTimeLBStr    = " ▷S2F17";
                     _SetTimeGBoxStr    = " ▷S2F13, S2F31";
                   DynamicReportPath    = gemPathParam.DynamicReportPath;
        }

        public void Closing(object sender, CancelEventArgs e)
        {
            Dispose();
        }

        private void Dispose()
        {
            m_XComPro.Stop();
            m_XComPro.Close();
            m_XComPro.Release();
        }

        private void AddLog(string formatString, params object[] args)
        {
            string addString = string.Format(formatString, args);

            logCollection.Add(string.Format("{0,4}. ", logCollection.Count) + DateTime.Now.ToString("[yy/MM/dd HH:mm:ss] ") + addString);
            SelectedIndex++;
            //System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            //{
            //}));
        }

        #region MainFrame_Command

        #region ==> Clear Log Collection Command
        private ICommand _ClearLogCollectionCommand;
        public ICommand ClearLogCollectionCommand
        {
            get
            {
                if (_ClearLogCollectionCommand == null)
                    _ClearLogCollectionCommand = new RelayCommand(ClearLogCollection);
                return _ClearLogCollectionCommand;
            }
        }
        public void ClearLogCollection(object obj)
        {
            LogCollection.Clear();
        }
        #endregion

        #region ==> OpenConnInfo Command
        private ICommand _ConnInfoCommand;
        public ICommand ConnInfoCommand
        {
            get
            {
                if (_ConnInfoCommand == null)
                    _ConnInfoCommand = new RelayCommand(OpenConnInfo);
                return _ConnInfoCommand;
            }
        }
        private void OpenConnInfo(object obj)
        {
            View.Dialog_ChangeConnInfo changeConnInfoDialog = new View.Dialog_ChangeConnInfo(m_XComPro);
            
            if (changeConnInfoDialog.ShowDialog() == true)
            {

            }
        }
        #endregion

        #region ==> StartStopHost Command
        private ICommand _StartStopHostCommand;
        public ICommand StartStopHostCommand
        {
            get
            {
                if (_StartStopHostCommand == null)
                    _StartStopHostCommand = new RelayCommand(StartStopHost);
                return _StartStopHostCommand;
            }
        }
        private void StartStopHost(object obj)
        {

            String resultStr = secs_HostOperator.StartStopHost(obj);

            if(resultStr != null)
            {
                StartHostButtonStr = resultStr;

                if(resultStr.ToLower().Equals("connected"))
                {
                    EnableMenuItemConnInfo = false;
                }
                else
                {
                    EnableMenuItemConnInfo = true;
                }
            }
            else
            {

            }
        }
        #endregion

        #region ==> SetOnline Command
        private ICommand _SetOnlineCommand;
        public ICommand SetOnlineCommand
        {
            get
            {
                if (_SetOnlineCommand == null)
                    _SetOnlineCommand = new RelayCommand(SetOnlineMode);
                return _SetOnlineCommand;
            }
        }
        public void SetOnlineMode(object obj)
        {
            secs_HostOperator.SetOnlineMode(obj);
        }
        #endregion

        #region ==> SetOffline Command
        private ICommand _SetOfflineCommand;
        public ICommand SetOfflineCommand
        {
            get
            {
                if (_SetOfflineCommand == null)
                    _SetOfflineCommand = new RelayCommand(SetOfflineMode);
                return _SetOfflineCommand;
            }
        }
        public void SetOfflineMode(object obj)
        {
            secs_HostOperator.SetOfflineMode(obj);
        }
        #endregion

        #region ==> SendDynamicReport Command
        private ICommand _SendDynamicReportCommand;
        public ICommand SendDynamicReportCommand
        {
            get
            {
                if (_SendDynamicReportCommand == null)
                    _SendDynamicReportCommand = new RelayCommand(SendDynamicReport);
                return _SendDynamicReportCommand;
            }
        }
        public void SendDynamicReport(object obj)
        {
            try
            {
                if (DynamicReportPath != null && DynamicReportPath != "")
                    secs_HostOperator.SendDynamicReport(obj, DynamicReportPath, "S2, F33", "S2, F35", "S2, F37");
                else
                    throw new NullReferenceException("SendDynamicReport() : Wrong [DynamicReportPath] Value.");
            }
            catch(NullReferenceException nullException)
            {
                AddLog("{0}", nullException.Message);
            }
        }
        #endregion

        #region ==> Search DynamicReport Path Command
        private ICommand _SearchDynamicReportPathCommand;
        public ICommand SearchDynamicReportPathCommand
        {
            get
            {
                if (_SearchDynamicReportPathCommand == null)
                    _SearchDynamicReportPathCommand = new RelayCommand(SearchDynamicReportPath);
                return _SearchDynamicReportPathCommand;
            }
        }
        public void SearchDynamicReportPath(object obj)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DynamicReportPath = openFileDialog.FileName;
            }
        }
        #endregion

        #region ==> GetStatusNamelist Command
        private ICommand _GetStatusNamelistCommand;
        public ICommand GetStatusNamelistCommand
        {
            get
            {
                if (_GetStatusNamelistCommand == null)
                    _GetStatusNamelistCommand = new RelayCommand(GetStatusNamelist);
                return _GetStatusNamelistCommand;
            }
        }

        public void GetStatusNamelist(object obj)
        {
            secs_HostOperator.GetStatusNamelist(obj);
        }
        #endregion

        #region ==> Request Equip SV Command
        private ICommand _RequestEquipSVCommand;
        public ICommand RequestEquipSVCommand
        {
            get
            {
                if (_RequestEquipSVCommand == null)
                    _RequestEquipSVCommand = new RelayCommand(RequestEquipSV);
                return _RequestEquipSVCommand;
            }
        }

        public void RequestEquipSV(object obj)
        {
            View.Dialog_RequestEquipSV requestStatusDialog = new View.Dialog_RequestEquipSV(m_XComPro, secs_HostOperator.ReceivedData, AddLog);
            requestStatusDialog.AddLog = AddLog;

            requestStatusDialog.ShowDialog();
        }
        #endregion

        #region ==> Are You There Command
        private ICommand _AreYouThereCommand;
        public ICommand AreYouThereCommand
        {
            get
            {
                if (_AreYouThereCommand == null)
                    _AreYouThereCommand = new RelayCommand(AreYouThere);
                return _AreYouThereCommand;
            }
        }
        public void AreYouThere(object obj)
        {
            secs_HostOperator.AreYouThere(obj);
        }
        #endregion

        #region ==> Change ECV Command
        private ICommand _ChangeECVCommand;
        public ICommand ChangeECVCommand
        {
            get
            {
                if (_ChangeECVCommand == null)
                    _ChangeECVCommand = new RelayCommand(ChangeECV);
                return _ChangeECVCommand;
            }
        }

        public void ChangeECV(object obj)
        {
            OpenECVDialog();
        }

        public async void OpenECVDialog()
        {
            SECSData taskRet = null;

            taskRet = await secs_HostOperator.GetECValue();

            if(taskRet != null)
            {
                View.Dialog_ChangeECV dialog_changeECD = new View.Dialog_ChangeECV(m_XComPro, taskRet, AddLog);

                dialog_changeECD.ShowDialog();

            }
        }

        #endregion

        #region ==> Set Date and Time
        private ICommand _SetDateTimeCommand;
        public ICommand SetDateTimeCommand
        {
            get
            {
                if (_SetDateTimeCommand == null)
                    _SetDateTimeCommand = new RelayCommand(SetDateTime);
                return _SetDateTimeCommand;
            }
        }

        public void SetDateTime(object obj)
        {
            AsyncSetDateTime(this.SetTimeStr);
        }

        public async void AsyncSetDateTime(String m_Time)
        {
            bool retVal = await secs_HostOperator.SetDateTime(m_Time);
            //Task<bool> task = Task<bool>.Run(()=> secs_HostOperator.SetDateTime());
            //await task;
        }
        #endregion

        #region ==> Request Date and Time Command
        private ICommand _RequestDateTimeCommand;
        public ICommand RequestDateTimeCommand
        {
            get
            {
                if (_RequestDateTimeCommand == null)
                    _RequestDateTimeCommand = new RelayCommand(RequestDateTime);
                return _RequestDateTimeCommand;
            }
        }
        public void RequestDateTime(object obj)
        {
            secs_HostOperator.RequestDateTime();
        }
        #endregion

        #region ==> Terminal Display Command
        private ICommand _TerminalDisplayCommand;
        public ICommand TerminalDisplayCommand
        {
            get
            {
                if (_TerminalDisplayCommand == null)
                    _TerminalDisplayCommand = new RelayCommand(TerminalDisplay);
                return _TerminalDisplayCommand;
            }
        }
        public void TerminalDisplay(object obj)
        {
            View.Dialog_TerminalDisplay dialog_terminalDisplay = new View.Dialog_TerminalDisplay(m_XComPro, AddLog);

            dialog_terminalDisplay.ShowDialog();
        }

        #endregion

        #region ==> Alarm List Display Command
        private ICommand _AlarmListDisplayCommand;
        public ICommand AlarmListDisplayCommand
        {
            get
            {
                if (_AlarmListDisplayCommand == null)
                    _AlarmListDisplayCommand = new RelayCommand(AlarmListDisplay);
                return _AlarmListDisplayCommand;
            }
        }
        public void AlarmListDisplay(object obj)
        {
            OpenAlarmDialog();
        }

        public async void OpenAlarmDialog()
        {
            SECSData taskRet = null;
            SECSData enableAlarm = null;

            taskRet = await secs_HostOperator.GetAlarmList();
            enableAlarm = await secs_HostOperator.GetEnableAlarmList();

            foreach (var v in (enableAlarm as SECSGenericData<List<SECSData>>)?.Value)
            {
                uint enableID = ((v as SECSGenericData<List<SECSData>>)?.Value[1] as SECSGenericData<uint>).Value;

                foreach(var v2 in (taskRet as SECSGenericData<List<SECSData>>)?.Value)
                {
                    if(((v2 as SECSGenericData<List<SECSData>>)?.Value[1] as SECSGenericData<uint>).Value.Equals(enableID))
                    {
                        (v2 as SECSGenericData<List<SECSData>>)?.Value.Add(new SECSGenericData<bool>() { Value = true, MsgType = SECS_DataTypeCategory.BOOL });
                    }
                }
            }

            if (taskRet != null)
            {
                View.Dialog_AlarmList dialog_AlarmDisplay = new View.Dialog_AlarmList(m_XComPro, taskRet, AddLog);
                dialog_AlarmDisplay.ShowDialog();
            }
        }

        #endregion

        #region ==> Send Remote CMD Command
        private ICommand _SendRemoteCMDCommand;
        public ICommand SendRemoteCMDCommand
        {
            get
            {
                if (_SendRemoteCMDCommand == null)
                    _SendRemoteCMDCommand = new RelayCommand(SendRemoteCMD);
                return _SendRemoteCMDCommand;
            }
        }
        public void SendRemoteCMD(object obj)
        {
            secs_HostOperator.SendRemoteCMD(RemoteCMDStr);
        }

        #endregion

        #region ==> Send CMD Command
        private ICommand _SendCMDCommand;
        public ICommand SendCMDCommand
        {
            get
            {
                if (_SendCMDCommand == null)
                    _SendCMDCommand = new RelayCommand(SendCMD);
                return _SendCMDCommand;
            }
        }
        public void SendCMD(object obj)
        {
            secs_HostOperator.SendCMD();
        }

        #endregion

        #region ==> Send CMD Command
        private ICommand _EstablishRequestCommand;
        public ICommand EstablishRequestCommand
        {
            get
            {
                if (_EstablishRequestCommand == null)
                    _EstablishRequestCommand = new RelayCommand(EstablishRequest);
                return _EstablishRequestCommand;
            }
        }
        public void EstablishRequest(object obj)
        {
            secs_HostOperator.EstablishRequest();
        }

        #endregion

        

        #endregion
    }
}