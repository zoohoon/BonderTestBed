using EnvMonitoring;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.EnvControl.Enum;
using ProberInterfaces.Temperature.EnvMonitoring;
using ProberViewModel.View.EnvMonitoring;
using RelayCommandBase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VirtualKeyboardControl;

namespace ProberViewModel.ViewModel
{              
    public class EnvMonitoringViewModel : IMainScreenViewModel, INotifyPropertyChanged, IFactoryModule
    {
        #region // ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        #region <remarks> Property </remarks>
        readonly Guid _ViewModelGUID = new Guid("D2AFD915-8453-4D35-B267-0963DCA8768D");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        public bool Initialized { get; set; } = false;        

        private EnvMonitoringManager _EnvMonitoringManager;
        public EnvMonitoringManager EnvMonitoringManager
        {
            get { return _EnvMonitoringManager; }
            set
            {
                if(value != _EnvMonitoringManager)
                {
                    _EnvMonitoringManager = value;
                    RaisePropertyChanged();
                }
            }
        }


        
        private ObservableCollection<ISensorModule> _SensorModules
            = new ObservableCollection<ISensorModule>();

        public ObservableCollection<ISensorModule> SensorModules
        {
            get { return _SensorModules; }
            set
            {
                if (value != _SensorModules)
                {
                    _SensorModules = value;
                    RaisePropertyChanged();
                }
            }
        }
        object lockObj = new object();
        #endregion

        #region <remarks> Command </remarks>                
        private RelayCommand<object> _SensorEnableChangedCommand;
        public ICommand SensorEnableChangedCommand
        {
            get
            {
                if (null == _SensorEnableChangedCommand) _SensorEnableChangedCommand = new RelayCommand<object>(SensorEnableChangedCommandFunc);
                return _SensorEnableChangedCommand;
            }
        }

        private void SensorEnableChangedCommandFunc(object param)
        {
            try
            {
                // 이미 포트에 연결되어 있는 상태에서 사용할지 말지 여부 결정
                if (param == null)
                    return;
                Object[] values = param as Object[];
                int idx = (int)values[0];
                bool enable = (bool)values[1];
                // disable -> enable 로 할때 index = 0 으로 들어옴. 예외처리 필요
                ISensorModule sensor = SensorModules.FirstOrDefault(item => item.SensorSysParameter.Index.Value == idx);
                if (sensor != null)
                {
                    IEnvMonitoringHub envMonitoringHub = EnvMonitoringManager.EnvMonitoringHubs.Find(hub => hub.CommunicationParam.Hub.Value == sensor.SensorSysParameter.Hub.Value);
                    {
                        if(envMonitoringHub != null)
                        {
                            sensor.SensorSysParameter.ModuleEnable.Value = !enable;

                            if (!sensor.SensorSysParameter.ModuleEnable.Value)
                            {
                                // disable                                                                                        
                                ClearData(idx);
                            }
                            else
                            {
                                // enable                                                        
                            }
                        }
                    }                    
                }
                UpdateChanged(idx);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        
        private AsyncCommand<object> _ReconnectCommand;
        public ICommand ReconnectCommand
        {
            get
            {
                if (null == _ReconnectCommand) _ReconnectCommand = new AsyncCommand<object>(ReconnectCommandFunc);
                return _ReconnectCommand;
            }
        }

        private async Task ReconnectCommandFunc(object param)
        {
            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Please wait");
                foreach (var item in EnvMonitoringManager.EnvMonitoringHubs)
                {
                    item.CommModule.ReInitalize(true);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        private RelayCommand<object> _AllAlarmResetCommand;
        public ICommand AllAlarmResetCommand
        {
            get
            {
                if (null == _AllAlarmResetCommand) _AllAlarmResetCommand = new RelayCommand<object>(AllAlarmResetCommandFunc);
                return _AllAlarmResetCommand;
            }
        }

        private void AllAlarmResetCommandFunc(object param)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                lock (lockObj)
                {                 
                    // enable = ture && attach = true 인경우 모든 센서에 알람 리셋 보내기
                    foreach (var sensor in EnvMonitoringManager.SensorModules)
                    {
                        IEnvMonitoringHub envMonitoringHub = EnvMonitoringManager.EnvMonitoringHubs.Find(hub => hub.CommunicationParam.Hub.Value == sensor.SensorSysParameter.Hub.Value);
                        if (envMonitoringHub != null)
                        {
                            if (sensor.SensorSysParameter.ModuleEnable.Value && sensor.SensorSysParameter.ModuleAttached.Value)
                            {
                                retVal = sensor.ProtocolModule.VerifyData(sensor.SensorSysParameter.Index.Value, sensor.ProtocolModule.AlarmResetbuff);
                                if(retVal == EventCodeEnum.NONE)
                                {
                                    envMonitoringHub.Send(sensor.ProtocolModule.AlarmResetbuff, 0, sensor.ProtocolModule.AlarmResetbuff.Length);                                    
                                }
                                if (sensor.bAlarmNotifyDone)
                                    sensor.bAlarmNotifyDone = false;
                            }
                        }                        
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _SensorAliasTextBoxClickCommand;
        public ICommand SensorAliasTextBoxClickCommand
        {
            get
            {
                if (null == _SensorAliasTextBoxClickCommand) _SensorAliasTextBoxClickCommand = new RelayCommand<Object>(SensorAliasTextBoxClickCommandFunc);
                return _SensorAliasTextBoxClickCommand;
            }
        }

        private void SensorAliasTextBoxClickCommandFunc(Object param)
        {
            try
            {
                Object[] values = param as Object[];
                int idx = (int)values[1];

                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)values[0];
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.ALPHABET, 0, 15);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();

                ISensorModule sensor = SensorModules.FirstOrDefault(item => item.SensorSysParameter.Index.Value == idx);
                if(sensor != null)
                {
                    sensor.SensorSysParameter.Sensor.SensorAlias.Value = tb.Text;                    
                }
                UpdateChanged(idx);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }        
        
        private RelayCommand<Object> _WarningTempTextBoxClickCommand;
        public ICommand WarningTempTextBoxClickCommand
        {
            get
            {
                if (null == _WarningTempTextBoxClickCommand) _WarningTempTextBoxClickCommand = new RelayCommand<Object>(WarningTempTextBoxClickCommandFunc);
                return _WarningTempTextBoxClickCommand;
            }
        }

        private void WarningTempTextBoxClickCommandFunc(Object param)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                lock (lockObj)
                {
                    Object[] values = param as Object[];
                    int idx = (int)values[1];

                    ISensorModule sensor = SensorModules.FirstOrDefault(item => item.SensorSysParameter.Index.Value == idx);
                    if (sensor != null)
                    {
                        IEnvMonitoringHub envMonitoringHub = EnvMonitoringManager.EnvMonitoringHubs.Find(hub => hub.CommunicationParam.Hub.Value == sensor.SensorSysParameter.Hub.Value);
                        if (envMonitoringHub != null)
                        {
                            if (sensor.SensorInfo is SmokeSensorInfo)
                            {
                                SmokeSensorInfo sensorinfo = sensor.SensorInfo as SmokeSensorInfo;

                                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)values[0];
                                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL | KB_TYPE.FLOAT, 0, 100);
                                if (double.Parse(tb.Text) > 70 || double.Parse(tb.Text) < -20)
                                {
                                    this.MetroDialogManager().ShowMessageDialog("[Smoke Sensor Warning]", $"The ambient temperature ranges from - 20 to 70 degrees.", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                                    tb.Text = sensorinfo.WarningTemp.ToString();
                                    return;
                                }
                                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                                sensorinfo.WarningTemp.Value = double.Parse(tb.Text);                                
                                string[] warn = DoubleToHex(sensorinfo.WarningTemp.Value);
                                string[] alarm = DoubleToHex(sensorinfo.AlarmTemp.Value);
                                SetTempBuffFunc(sensor.ProtocolModule.SetTempbuff, warn, alarm);
                                retVal = sensor.ProtocolModule.VerifyData(idx, sensor.ProtocolModule.SetTempbuff);
                                if (retVal == EventCodeEnum.NONE)
                                {
                                    envMonitoringHub.Send(sensor.ProtocolModule.SetTempbuff, 0, sensor.ProtocolModule.SetTempbuff.Length);
                                }
                                else
                                {
                                    LoggerManager.Debug("WarningTempTextBoxClickCommandFunc: VerifyData Failed");
                                }
                            }
                        }
                        UpdateChanged(idx);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _AlarmTempTextBoxClickCommand;
        public ICommand AlarmTempTextBoxClickCommand
        {
            get
            {
                if (null == _AlarmTempTextBoxClickCommand) _AlarmTempTextBoxClickCommand = new RelayCommand<Object>(AlarmTempTextBoxClickCommandFunc);
                return _AlarmTempTextBoxClickCommand;
            }
        }

        private void AlarmTempTextBoxClickCommandFunc(Object param)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                lock (lockObj)
                {
                    Object[] values = param as Object[];
                    int idx = (int)values[1];

                    ISensorModule sensor = SensorModules.FirstOrDefault(item => item.SensorSysParameter.Index.Value == idx);
                    if (sensor != null)
                    {
                        IEnvMonitoringHub envMonitoringHub = EnvMonitoringManager.EnvMonitoringHubs.Find(hub => hub.CommunicationParam.Hub.Value == sensor.SensorSysParameter.Hub.Value);
                        if (envMonitoringHub != null)
                        {                            
                            if (sensor.SensorInfo is SmokeSensorInfo)
                            {
                                SmokeSensorInfo sensorinfo = sensor.SensorInfo as SmokeSensorInfo;

                                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)values[0];
                                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL | KB_TYPE.FLOAT, 0, 100);
                                if (double.Parse(tb.Text) > 70 || double.Parse(tb.Text) < -20)
                                {
                                    this.MetroDialogManager().ShowMessageDialog("[Smoke Sensor Warning]", $"The ambient temperature ranges from - 20 to 70 degrees.", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                                    tb.Text = sensorinfo.AlarmTemp.ToString();
                                    return;
                                }

                                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                                sensorinfo.AlarmTemp.Value = double.Parse(tb.Text);                                
                                string[] warn = DoubleToHex(sensorinfo.WarningTemp.Value);
                                string[] alarm = DoubleToHex(sensorinfo.AlarmTemp.Value);
                                SetTempBuffFunc(sensor.ProtocolModule.SetTempbuff, warn, alarm);
                                retVal = sensor.ProtocolModule.VerifyData(idx, sensor.ProtocolModule.SetTempbuff);
                                if (retVal == EventCodeEnum.NONE)
                                {
                                    envMonitoringHub.Send(sensor.ProtocolModule.SetTempbuff, 0, sensor.ProtocolModule.SetTempbuff.Length);
                                }
                                else
                                {
                                    LoggerManager.Debug("AlarmTempTextBoxClickCommandFunc: VerifyData Failed");
                                }
                            }                            
                        }
                        UpdateChanged(idx);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        

        private RelayCommand<Object> _TempDeviationTextBoxClickCommand;
        public ICommand TempDeviationTextBoxClickCommand
        {
            get
            {
                if (null == _TempDeviationTextBoxClickCommand) _TempDeviationTextBoxClickCommand = new RelayCommand<Object>(TempDeviationTextBoxClickCommandFunc);
                return _TempDeviationTextBoxClickCommand;
            }
        }

        private void TempDeviationTextBoxClickCommandFunc(Object param)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                lock (lockObj)
                {
                    Object[] values = param as Object[];
                    int idx = (int)values[1];

                    ISensorModule sensor = SensorModules.FirstOrDefault(item => item.SensorSysParameter.Index.Value == idx);
                    if (sensor != null)
                    {
                        IEnvMonitoringHub envMonitoringHub = EnvMonitoringManager.EnvMonitoringHubs.Find(hub => hub.CommunicationParam.Hub.Value == sensor.SensorSysParameter.Hub.Value);
                        if (envMonitoringHub != null)
                        {
                            if (sensor.SensorInfo is SmokeSensorInfo)
                            {
                                SmokeSensorInfo sensorinfo = sensor.SensorInfo as SmokeSensorInfo;

                                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)values[0];
                                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL | KB_TYPE.FLOAT, 0, 100);
                                if (double.Parse(tb.Text) > 70 || double.Parse(tb.Text) < -20)
                                {
                                    this.MetroDialogManager().ShowMessageDialog("[Smoke Sensor Warning]", $"The ambient temperature ranges from - 20 to 70 degrees.", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                                    tb.Text = sensorinfo.TempDeviation.ToString();
                                    return;
                                }

                                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                                sensorinfo.TempDeviation.Value = double.Parse(tb.Text);
                                string[] temp = DoubleToHex(sensorinfo.TempDeviation.Value);
                                SetTempDeviationBuffFunc(sensor.ProtocolModule.SetTempDeviationbuff, temp);
                                retVal = sensor.ProtocolModule.VerifyData(idx, sensor.ProtocolModule.SetTempDeviationbuff);
                                if (retVal == EventCodeEnum.NONE)
                                {
                                    envMonitoringHub.Send(sensor.ProtocolModule.SetTempDeviationbuff, 0, sensor.ProtocolModule.SetTempDeviationbuff.Length);
                                }
                                else
                                {
                                    LoggerManager.Debug("TempDeviationTextBoxClickCommandFunc: VerifyData Failed");
                                }
                            }
                        }
                        UpdateChanged(idx);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region <remarks> IMainScreenViewModel Method </remarks>
        public Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            string[] strPortsList = SerialPort.GetPortNames();
            try
            {
                EnvMonitoringManager = (EnvMonitoringManager)this.EnvMonitoringManager();
                foreach (var moduleitem in EnvMonitoringManager.SensorModules)
                {
                    List<SensorSysParameter> param = EnvMonitoringManager.SensorParams.SensorSysParams.FindAll(a => a.Hub.Value == moduleitem.SensorSysParameter.Hub.Value);
                    if (param.Count != 0)
                    {
                        SensorModules.Add(moduleitem);                        
                    }                    
                }

                Initialized = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }
        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (EnvMonitoringManager != null)
                {
                    foreach (var moduleitem in EnvMonitoringManager.SensorModules)
                    {
                        UpdateChanged(moduleitem.SensorIndex);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }
        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                foreach (var item in EnvMonitoringManager.EnvMonitoringHubs)
                {
                    item.CommModule.DisConnect();
                }                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

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
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        #endregion

        #region <remarks> Function </remarks>
        public int HexToInt(string hex)
        {
            int value = 0;
            try
            {
                if (hex == null)
                    return 0;

                string str = hex.Substring(hex.Length - 2, 2);
                value = Convert.ToInt32(str, 16);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return value;

        }

        private string[] DoubleToHex(double temp)
        {
            int intval = 0;
            string[] hexval = new string[2];
            try
            {
                intval = (int)temp * 100;
                string hexValue = intval.ToString("X2");     // To hex
                if (hexValue.Length % 2 == 1)
                {
                    hexValue = hexValue.Insert(0, "0");
                }
                var temp_MSB = hexValue.Substring(hexValue.Length - 4, 2);
                var temp_LSB = hexValue.Substring(hexValue.Length - 2, 2);
                //LoggerManager.SmokeSensorLog($"DoubleToHex: {hexValue}");
                hexval[0] = String.Format("0x{0:X}", temp_MSB);
                hexval[1] = String.Format("0x{0:X}", temp_LSB);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return hexval;
        }

        /// <summary>
        /// 온도 경고, 알람 기준값 설정하는 함수
        /// </summary>        
        public void SetTempBuffFunc(byte[] buffer, string[] warn, string[] alarm)
        {
            //byte[] temp = new byte[2];
            try
            {
                if (buffer.Length == 0 || warn.Length == 0 || alarm.Length == 0)
                    return;
                // value 뒤에 2자리 잘라서 이어 붙이고 hex => int로 바꾸고 100으로 나누기
                // ex, value[0] = "0x0C"

                // Warning temp
                buffer[9] = (byte)HexToInt(warn[0]);
                buffer[10] = (byte)HexToInt(warn[1]);

                // Alarm temp
                buffer[11] = (byte)HexToInt(alarm[0]);
                buffer[12] = (byte)HexToInt(alarm[1]);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// 편차 기준값 설정하는 함수
        /// </summary> 
        public void SetTempDeviationBuffFunc(byte[] buffer, string[] value)
        {
            try
            {
                if (buffer.Length == 0 || value.Length == 0)
                    return;

                buffer[9] = (byte)HexToInt(value[0]);
                buffer[10] = (byte)HexToInt(value[1]);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                RetVal = this.SaveParameter(EnvMonitoringManager.SensorParams);
                if (RetVal == EventCodeEnum.PARAM_ERROR)
                {
                    LoggerManager.Error($"[EnvMonitoringViewModel] SaveSysParameter(): Serialize Error");
                    return RetVal;
                }
                else
                {
                    RetVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                LoggerManager.Exception(err);

                throw;
            }
            return RetVal;
        }
        
        private void UpdateChanged(int idx)
        {
            try
            {
                ISensorModule sensor = SensorModules.FirstOrDefault(item => item.SensorSysParameter.Index.Value == idx);
                if(sensor != null)
                {
                    if (sensor.SensorInfo is SmokeSensorInfo)
                    {
                        SmokeSensorInfo sensorinfo = sensor.SensorInfo as SmokeSensorInfo;
                        if (sensor.SensorSysParameter.Sensor is SmokeSensorSettingParam)
                        {
                            SmokeSensorSettingParam smokeSensorSettingParam = sensor.SensorSysParameter.Sensor as SmokeSensorSettingParam;
                            smokeSensorSettingParam.WarningTemp.Value = sensorinfo.WarningTemp.Value;
                            smokeSensorSettingParam.AlarmTemp.Value = sensorinfo.AlarmTemp.Value;
                            smokeSensorSettingParam.TempDeviation.Value = sensorinfo.TempDeviation.Value;
                            smokeSensorSettingParam.SensorAlias.Value = sensor.SensorAlias;
                        }

                    }
                }                
                SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        
        private void ClearData(int idx)
        {
            try
            {
                ISensorModule sensor = SensorModules.FirstOrDefault(item => item.SensorSysParameter.Index.Value == idx);
                if (sensor.SensorInfo is SmokeSensorInfo)
                {
                    SmokeSensorInfo sensorinfo = sensor.SensorInfo as SmokeSensorInfo;                    
                    sensorinfo.CurHumi.Value = 0;
                    sensorinfo.CurTemp.Value = 0;
                    sensorinfo.WarningTemp.Value = 0;
                    sensorinfo.AlarmTemp.Value = 0;
                    sensorinfo.TempDeviation.Value = 0;                    
                }
                sensor.SensorStatus = SensorStatusEnum.NOTUSED;
                SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
    
}
