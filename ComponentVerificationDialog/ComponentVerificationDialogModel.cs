using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using CylType;
using SubstrateObjects;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System.Reflection;
using ProberInterfaces.MarkAlign;
using ProberInterfaces.PolishWafer;
using TouchSensorSystemParameter;
using PolishWaferFocusingBySensorModule;
using PolishWaferFocusingModule;
using PolishWaferParameters;

namespace ComponentVerificationDialog
{
    public enum EnumMoveType
    {
        Relative,
        Absolute
    }

    class ComponentVerificationDialogModel : INotifyPropertyChanged, IFactoryModule
    {
        public ComponentVerificationDialogModel()
        {
            CommandList = new ObservableCollection<VerificationCommandBase>();
            CommandList.Add(new AxisHomingCommand());
            CommandList.Add(new MarkAlignCommand());
            CommandList.Add(new MarkAlignNoRetractCommand());
            CommandList.Add(new MarkAlignProcWaferCamBridgeCommand());
            CommandList.Add(new MarkAlignProcMoveToMarkCommand());
            CommandList.Add(new MarkAlignProcFocusingCommand());
            CommandList.Add(new MarkAlignProcPatternMatchingCommand());
            CommandList.Add(new WaferAlignCommand());
            CommandList.Add(new PinAlignCommand()); // Pin Align 기능 추가
            CommandList.Add(new AxisMoveCommand());
            CommandList.Add(new MultiAxisMoveCommand());
            CommandList.Add(new ChuckTiltMoveCommand()); // Chuck Tilt 기능 추가
            CommandList.Add(new LightControlCommand());
            CommandList.Add(new DelayTimeCommand());
            CommandList.Add(new DefaultPosMoveCommand());
            CommandList.Add(new StageMoveCommand());
            CommandList.Add(new PolishWaferFocusingCommand());
            CommandList.Add(new TouchSensorOffsetCommand());

            ResultLogListOrg = new List<string>();
            ScenarioList = new ObservableCollection<VerificationCommandBase>();
            ResultLogList = new ObservableCollection<string>();
            LoggerManager.CompVerifyLogBuffer.CollectionChanged += CompVerifyLogBuffer_CollectionChanged;

            XAxis = this.MotionManager().GetAxis(EnumAxisConstants.X);
            YAxis = this.MotionManager().GetAxis(EnumAxisConstants.Y);
            ZAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
            CAxis = this.MotionManager().GetAxis(EnumAxisConstants.C);
            TriAxis = this.MotionManager().GetAxis(EnumAxisConstants.TRI);
            PzAxis = this.MotionManager().GetAxis(EnumAxisConstants.PZ);

            WaferHighCamLightInfo = new ObservableCollection<CameraLightInfo>();
            WaferLowCamLightInfo = new ObservableCollection<CameraLightInfo>();
            PinHighCamLightInfo = new ObservableCollection<CameraLightInfo>();
            PinLowCamLightInfo = new ObservableCollection<CameraLightInfo>();

            Func<EnumProberCam, bool> AddLightChannel = (camType) =>
            {
                var Camera = this.VisionManager().GetCam(camType);
                foreach (var lightChannelType in Camera.LightsChannels)
                {
                    var lightChannel = Camera.LightAdmin().GetLightChannel(lightChannelType.ChannelMapIdx.Value);

                    switch(camType)
                    {
                        case EnumProberCam.WAFER_HIGH_CAM:
                            WaferHighCamLightInfo.Add(new CameraLightInfo(lightChannelType.Type.Value, lightChannel));
                            break;
                        case EnumProberCam.WAFER_LOW_CAM:
                            WaferLowCamLightInfo.Add(new CameraLightInfo(lightChannelType.Type.Value, lightChannel));
                            break;
                        case EnumProberCam.PIN_HIGH_CAM:
                            PinHighCamLightInfo.Add(new CameraLightInfo(lightChannelType.Type.Value, lightChannel));
                            break;
                        case EnumProberCam.PIN_LOW_CAM:
                            PinLowCamLightInfo.Add(new CameraLightInfo(lightChannelType.Type.Value, lightChannel));
                            break;
                        default:
                            break;
                    }
                }
                return true;
            };

            AddLightChannel(EnumProberCam.WAFER_HIGH_CAM);
            AddLightChannel(EnumProberCam.WAFER_LOW_CAM);
            AddLightChannel(EnumProberCam.PIN_HIGH_CAM);
            AddLightChannel(EnumProberCam.PIN_LOW_CAM);

            IsWaferBridgeExtended = this.IOManager()?.IO.Outputs.DOWAFERMIDDLE;
        }

        private bool ScenarioRunFlag = false;
        private List<string> ResultLogListOrg;
        string[] ArrayLogFilterString;
        JsonConverter[] jsonConverters = { new VerificationCommandConverter() };

        private object ScenarioListLockObject = new object();
        private object ResultLogListLockObject = new object();
        private object ResultLogListOrgLockObject = new object();

        private Version CurVersion = Assembly.GetExecutingAssembly().GetName().Version;
        private enum EnumJsonObject
        {
            Version,
            Command
        } 
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        #region ==> Property
        private string _TestString;
        public string TestString
        {
            get { return _TestString; }
            set
            {
                if (_TestString != value)
                {
                    _TestString = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IDisplayPort _DisplayPort;
        public IDisplayPort DisplayPort
        {
            get { return _DisplayPort; }
            set
            {
                if (value != _DisplayPort)
                {
                    _DisplayPort = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ObservableCollection<VerificationCommandBase> CommandList { get; set; }

        private VerificationCommandBase _SelectedCommand;
        public VerificationCommandBase SelectedCommand
        {
            get { return _SelectedCommand; }
            set
            {
                if (value != _SelectedCommand)
                {
                    _SelectedCommand = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ObservableCollection<VerificationCommandBase> ScenarioList { get; set; }

        private VerificationCommandBase _SelectedScenario;
        public VerificationCommandBase SelectedScenario
        {
            get { return _SelectedScenario; }
            set
            {
                if (value != _SelectedScenario)
                {
                    _SelectedScenario = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _SelectedScenarioIndex;
        public int SelectedScenarioIndex
        {
            get { return _SelectedScenarioIndex; }
            set
            {
                if(value != _SelectedScenarioIndex)
                {
                    _SelectedScenarioIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsEnableScenarioEdit = true;
        public bool IsEnableScenarioEdit
        {
            get { return _IsEnableScenarioEdit; }
            set
            {
                if (value != _IsEnableScenarioEdit)
                {
                    _IsEnableScenarioEdit = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsEnableStopBtn = false;
        public bool IsEnableStopBtn
        {
            get { return _IsEnableStopBtn; }
            set
            {
                if (value != _IsEnableStopBtn)
                {
                    _IsEnableStopBtn = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _RunCount = "1";
        public string RunCount
        {
            get { return _RunCount; }
            set
            {
                if (value != _RunCount)
                {
                    if (CheckNumberTextInput(value))
                    {
                        _RunCount = value;
                        RaisePropertyChanged();
                    }
                }
            }
        }

        private bool _CheckedCount = true;
        public bool CheckedCount
        {
            get { return _CheckedCount; }
            set
            {
                if (value != _CheckedCount)
                {
                    _CheckedCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _CheckedRepeat = false;
        public bool CheckedRepeat
        {
            get { return _CheckedRepeat; }
            set
            {
                if (value != _CheckedRepeat)
                {
                    _CheckedRepeat = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ObservableCollection<string> ResultLogList { get; set; }

        private string _LogFilterString;
        public string LogFilterString
        {
            get { return _LogFilterString; }
            set
            {
                if (value != _LogFilterString)
                {
                    _LogFilterString = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IOPortDescripter<bool> _IsWaferBridgeExtended;
        public IOPortDescripter<bool> IsWaferBridgeExtended
        {
            get { return _IsWaferBridgeExtended; }
            set
            {
                if (value != _IsWaferBridgeExtended)
                {
                    _IsWaferBridgeExtended = value;
                    RaisePropertyChanged();
                }
            }
        }

        // Log로 인해 Memory가 Full 되는것을 막기 위해 MaxCount 설정.
        private int _ResultLogMaxCount = 10000;
        public int ResultLogMaxCount
        {
            get { return _ResultLogMaxCount; }
            set
            {
                if (value != _ResultLogMaxCount)
                {
                    if(CheckNumberTextInput(value.ToString()))
                    {
                        _ResultLogMaxCount = value;
                        RaisePropertyChanged();
                    }
                }
            }
        }
        #endregion

        #region ==> Axis
        private AxisObject _XAxis;

        public AxisObject XAxis
        {
            get { return _XAxis; }
            set
            {
                if (value != _XAxis)
                {
                    _XAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _YAxis;

        public AxisObject YAxis
        {
            get { return _YAxis; }
            set
            {
                if (value != _YAxis)
                {
                    _YAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _ZAxis;

        public AxisObject ZAxis
        {
            get { return _ZAxis; }
            set
            {
                if (value != _ZAxis)
                {
                    _ZAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _CAxis;

        public AxisObject CAxis
        {
            get { return _CAxis; }
            set
            {
                if (value != _CAxis)
                {
                    _CAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _TriAxis;

        public AxisObject TriAxis
        {
            get { return _TriAxis; }
            set
            {
                if (value != _TriAxis)
                {
                    _TriAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _PzAxis;

        public AxisObject PzAxis
        {
            get { return _PzAxis; }
            set
            {
                if (value != _PzAxis)
                {
                    _PzAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> Light
        //public ObservableCollection<ILightChannel> LightChannels { get; set; }

        public ObservableCollection<CameraLightInfo> WaferHighCamLightInfo { get; set; }
        public ObservableCollection<CameraLightInfo> WaferLowCamLightInfo { get; set; }
        public ObservableCollection<CameraLightInfo> PinHighCamLightInfo { get; set; }
        public ObservableCollection<CameraLightInfo> PinLowCamLightInfo { get; set; }
        #endregion

        #region ==> Functions
        public void AddCommand()
        {
            if (null == SelectedCommand)
                return;
            var AddCommand = SelectedCommand.Create();
            if (0 == ScenarioList.Count) // List가 없는 경우
            {
                ScenarioList.Add(AddCommand);
                SelectedScenarioIndex = 0;
            }
            else if(SelectedScenarioIndex + 1 == ScenarioList.Count) // 맨 마지막 항목을 선택한 경우
            {
                ScenarioList.Add(AddCommand);
                SelectedScenarioIndex++;
            }
            else // 중간 항목을 선택한 경우 해당 항목의 밑에 추가한다.
            {
                ScenarioList.Insert(SelectedScenarioIndex + 1, AddCommand);
                SelectedScenarioIndex++;
            }
            AddCommand.ParameterList.ToList().ForEach(x => x.UserEditable = true);
        }

        public void DeleteCommand()
        {
            if(0 == ScenarioList.Count || SelectedScenarioIndex < 0) // 선택된게 아무것도 없는 경우
            {
                return;
            }
            else if(1 == ScenarioList.Count) // 마지막 하나 남은 경우
            {
                ScenarioList.Remove(SelectedScenario);
            }
            else if (SelectedScenarioIndex + 1 == ScenarioList.Count) // 맨 마지막 항목을 선택하는 경우
            {
                var tmpSelectedIndex = SelectedScenarioIndex;
                ScenarioList.Remove(SelectedScenario);
                SelectedScenarioIndex = tmpSelectedIndex - 1;
            }
            else // 일반적인 경우
            {
                var tmpSelectedIndex = SelectedScenarioIndex;
                ScenarioList.Remove(SelectedScenario);
                SelectedScenarioIndex = tmpSelectedIndex;
            }
        }

        public void ScenarioUp()
        {
            if (0 == ScenarioList.Count || SelectedScenarioIndex <= 0) // 선택된게 아무것도 없거나 맨 위 선택한 경우
            {
                return;
            }
            else // 일반적인 경우
            {
                ScenarioList.Move(SelectedScenarioIndex, SelectedScenarioIndex - 1);
            }
        }

        public void ScenarioDown()
        {
            if (0 == ScenarioList.Count || SelectedScenarioIndex < 0 || SelectedScenarioIndex == ScenarioList.Count - 1) // 선택된게 아무것도 없거나 맨 아래 선택한 경우
            {
                return;
            }
            else // 일반적인 경우
            {
                ScenarioList.Move(SelectedScenarioIndex, SelectedScenarioIndex + 1);
            }
        }

        private bool CheckNumberTextInput(string input)
        {
            bool ret = true;

            try
            {
                Regex regex = new Regex("^[0-9]*$");
                ret = regex.IsMatch(input);

                if (ret)
                {
                    var num = Convert.ToInt32(input);

                    if (0 == num)
                    {
                        MessageBox.Show("Wrong Input");
                        ret = false;
                    }
                }
                else
                {
                    MessageBox.Show("Wrong Input");
                }
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }

        public void ScenarioRun()
        {
            ScenarioRunFlag = (ScenarioList.Count > 0 ? true : false);
            int CurrentCount = 0;
            int TotalCount = Convert.ToInt32(RunCount);
            string MsgBoxString = "";
            IsEnableStopBtn = true;

            try
            {
                IsEnableScenarioEdit = false;
                while (ScenarioRunFlag)
                {
                    CurrentCount++;
                    LoggerManager.CompVerifyLog($"[ScenarioRun] Progress : {CurrentCount}/{TotalCount}");
                    SelectedScenarioIndex = 0;
                    foreach (var iter in ScenarioList)
                    {
                        if (!ScenarioRunFlag) // stop 눌러 멈췄을 경우
                        {
                            LoggerManager.CompVerifyLog($"[ScenarioRun] Stop Scenario Running Before {iter.CommandName}");
                            break;
                        }
                        iter.CommandTextColor = "Thistle";
                        bool IsStopScenario = false;
                        iter.DoCommand(ref IsStopScenario, ref ScenarioRunFlag, ref MsgBoxString);
                        iter.CommandTextColor = "White";
                        if (IsStopScenario)
                        {
                            ScenarioRunFlag = false;
                            LoggerManager.CompVerifyLog($"[ScenarioRun] Stop Scenario Running");
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                MessageBox.Show(new Window { Topmost = true }, $"Stop Scenario Running due to a failure.\n{MsgBoxString}", "Error");
                            });
                            break;
                        }
                        SelectedScenarioIndex = SelectedScenarioIndex + 1 < ScenarioList.Count ? SelectedScenarioIndex + 1 : 0;
                    }

                    // Count Mode 인 경우 
                    if (CheckedCount)
                    {
                        int count = Convert.ToInt32(RunCount);
                        if (1 >= count)
                        {
                            break;
                        }
                        count--;
                        RunCount = count.ToString();
                    }
                }
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                IsEnableScenarioEdit = true;
                IsEnableStopBtn = false;
            }
        }

        public void ScenarioStop()
        {
            ScenarioRunFlag = false;
            IsEnableStopBtn = false;
        }

        public void ScenarioExport()
        {
            try
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.DefaultExt = ".cvss";
                dlg.Filter = "Component Verification Scenario Script (*.cvss)|*.cvss";
                dlg.OverwritePrompt = true;

                bool? result = dlg.ShowDialog();
                if (result == true)
                {
                    string ScriptPath = dlg.FileName;
                    JObject ExportJObject = new JObject();
                    ExportJObject[EnumJsonObject.Version.ToString()] = JToken.FromObject(CurVersion.ToString());
                    ExportJObject[EnumJsonObject.Command.ToString()] = JToken.FromObject(ScenarioList);
                    var JsonScenario = JsonConvert.SerializeObject(ExportJObject, Formatting.Indented);
                    File.WriteAllText(ScriptPath, JsonScenario);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ScenarioImport()
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OpenFileDialog dlg = new OpenFileDialog();
                    dlg.Filter = "Component Verification Scenario Script (*.cvss)|*.cvss";

                    bool? result = dlg.ShowDialog();
                    if (result == true)
                    {
                         var JsonRead = File.ReadAllText(dlg.FileName);
                        dynamic DeserializedObject = JsonConvert.DeserializeObject(JsonRead);
                        ObservableCollection<VerificationCommandBase> ImportedScenario;
                        if ((DeserializedObject.First?.ContainsKey(VerificationCommandBase.CommandNameString)) ?? false) // 버전 정보 포함되기 전
                        {
                            ImportedScenario = JsonConvert.DeserializeObject<ObservableCollection<VerificationCommandBase>>(JsonRead, new JsonSerializerSettings() { Converters = jsonConverters });
                        }
                        else
                        {
                            if (CurVersion.CompareTo(Version.Parse(DeserializedObject[EnumJsonObject.Version.ToString()]?.ToString())) < 0) // 상위버전 import 시
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    MessageBox.Show(new Window { Topmost = true }, "Cannot import file because it was created by a higher version.");
                                });
                                return;
                            }
                             ImportedScenario = JsonConvert.DeserializeObject<ObservableCollection<VerificationCommandBase>>(DeserializedObject[EnumJsonObject.Command.ToString()].ToString(), new JsonSerializerSettings() { Converters = jsonConverters });
                        }

                        lock (ScenarioListLockObject)
                        {
                            ScenarioList.Clear();
                            bool HasInvalidCommand = false;
                            foreach (var Command in ImportedScenario)
                            {
                                if (null != Command && IsValidCommand(Command))
                                    ScenarioList.Add(Command);
                                else if (!HasInvalidCommand)
                                    HasInvalidCommand = true;
                            }
                            if (HasInvalidCommand)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    MessageBox.Show(new Window { Topmost = true }, "Some commands are not supported in this scenario.\nThe commands are not displayed.", "Notify");
                                });
                            }
                        }
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(new Window { Topmost = true }, $"Cannot open because it is an invalid file.", "Error");
                });
            }
        }

        private bool IsValidCommand(VerificationCommandBase Command)
        {
            bool IsValidCommand = true;
            foreach (var Parameter in Command.ParameterList)
            {
                if (null == Parameter)
                {
                    IsValidCommand = false;
                    break;
                }
                if (Parameter.ParameterType == CommandParameterBase.ParameterType_Bool || Parameter.ParameterType == CommandParameterBase.ParameterType_ComboBox)
                {
                    if (Parameter.SelectItems.Count <= 0 || Parameter.SelectedIndex < 0 || Parameter.SelectedIndex >= Parameter.SelectItems.Count)
                    {
                        IsValidCommand = false;
                        break;
                    }
                }
                else if (Parameter.ParameterType == CommandParameterBase.ParameterType_TextBox)
                {
                    TextBoxCommandParameter textBoxCommandParameter = Parameter as TextBoxCommandParameter;
                    if (!textBoxCommandParameter?.DoTextBoxValidCheck(textBoxCommandParameter.InputValue, false) ?? false)
                    {
                        IsValidCommand = false;
                        break;
                    }
                }
                else
                {
                    IsValidCommand = false;
                    break;
                }

                if (!Command.ParameterNameList.Contains(Parameter.ParameterName))
                {
                    IsValidCommand = false;
                    break;
                }
                Parameter.UserEditable = true; // import 완료하였으니, 사용자가 Parameter 변경 가능한 상태
            }

            return IsValidCommand;
        }

        public void ScenarioClear()
        {
            try
            {
                lock (ScenarioListLockObject)
                {
                    if (MessageBox.Show($"Are you sure you want to clear all?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        ScenarioList.Clear();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void CompVerifyLogBuffer_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                // 한번 Log를 출력할 때 Add, Remove 이벤트가 둘다 발생하므로 Add 이벤트인 경우에만 Log를 출력한다.
                if(e.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    return;
                }

                lock (LoggerManager.CompVerifyLogBufferLockObject)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        string logStr;

                        lock (ResultLogListOrgLockObject)
                        {
                            if (ResultLogListOrg.Count == ResultLogMaxCount)
                            {
                                ResultLogListOrg.RemoveAt(0);
                            }

                            string NowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            logStr = $"{NowTime} | {LoggerManager.CompVerifyLogBuffer.Last()}";

                            ResultLogListOrg.Add(logStr);
                        }

                        lock (ResultLogListLockObject)
                        {
                            if (null == ArrayLogFilterString)
                            {
                                ResultLogList.Add(logStr);
                            }
                            else
                            {
                                foreach (var FilterString in ArrayLogFilterString)
                                {
                                    if (true == logStr.ToLower().Contains(FilterString.ToLower()))
                                    {
                                        ResultLogList.Add(logStr);
                                    }
                                }
                            }
                        }
                    });
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ParamItemSellectionChanged(string paramName)
        {
            if(SelectedScenario is LightControlCommand)
            {
                if (paramName == LightControlCommand.ParamName_CameraType)
                {
                    var lightControlCommand = SelectedScenario as LightControlCommand;
                    lightControlCommand.CameraTypeSelectionChanged();
                }
            }
        }

        public void ApplyLogFilter()
        {
            try
            {
                // 1. LogFilterString을 빈칸 Trim 하기
                LogFilterString.Trim();

                // 2. LogFilterString을 "," 문자 기준으로 분리하기
                ArrayLogFilterString = LogFilterString.Split(',');

                // 3. 현재 ResultLog를 업데이트하기
                ResultLogList.Clear();
                foreach (var Log in ResultLogListOrg)
                {
                    foreach (var FilterString in ArrayLogFilterString)
                    {
                        if (true == Log.ToLower().Contains(FilterString.ToLower()))
                        {
                            ResultLogList.Add(Log);
                        }
                    }
                }
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ExportFilteredLog()
        {
            try
            {
                SaveFileDialog dlg = new SaveFileDialog();
                string NowTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                dlg.FileName = $"CompVerify_FilteredLog_{NowTime}";
                dlg.DefaultExt = ".log";
                dlg.Filter = "Log File (*.log)|*.log|All Files (*.*)|*.*";
                dlg.OverwritePrompt = true;

                bool? result = dlg.ShowDialog();

                if (result == true)
                {
                    string LogPath = dlg.FileName;
                    File.WriteAllLines(LogPath, ResultLogList);
                }
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void LogClear()
        {
            try
            {
                lock (ResultLogListLockObject)
                {
                    LogFilterString = null;
                    ResultLogListOrg.Clear();
                    ResultLogList.Clear();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
    }

    public class CameraLightInfo
    {
        public CameraLightInfo(EnumLightType enumLightType, ILightChannel lightChannel)
        {
            LightType = enumLightType;
            LightChannel = lightChannel;
        }

        public EnumLightType LightType { get; set; }
        public ILightChannel LightChannel { get; set; }
    }

    public class VerificationCommandConverter : JsonConverter
    {
        JsonConverter[] jsonConverts = { new CommandParameterConverter() };
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(VerificationCommandBase);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            var jsonString = jObject.ToString();

            if (jObject[VerificationCommandBase.CommandNameString].Value<string>() == VerificationCommandBase.CommandName_AxisHoming)
            {
                var ConvertedCommand = JsonConvert.DeserializeObject<AxisHomingCommand>(jsonString, new JsonSerializerSettings() { Converters = jsonConverts, ObjectCreationHandling = ObjectCreationHandling.Replace });
                return ConvertedCommand;
            }
            else if (jObject[VerificationCommandBase.CommandNameString].Value<string>() == VerificationCommandBase.CommandName_MarkAlign)
            {
                var ConvertedCommand = JsonConvert.DeserializeObject<MarkAlignCommand>(jsonString, new JsonSerializerSettings() { Converters = jsonConverts, ObjectCreationHandling = ObjectCreationHandling.Replace });
                return ConvertedCommand;
            }
            else if (jObject[VerificationCommandBase.CommandNameString].Value<string>() == VerificationCommandBase.CommandName_MarkAlignNoRetract)
            {
                var ConvertedCommand = JsonConvert.DeserializeObject<MarkAlignNoRetractCommand>(jsonString, new JsonSerializerSettings() { Converters = jsonConverts, ObjectCreationHandling = ObjectCreationHandling.Replace });
                return ConvertedCommand;
            }
            else if (jObject[VerificationCommandBase.CommandNameString].Value<string>() == VerificationCommandBase.CommandName_MarkAlignProcWaferCamBridge)
            {
                var ConvertedCommand = JsonConvert.DeserializeObject<MarkAlignProcWaferCamBridgeCommand>(jsonString, new JsonSerializerSettings() { Converters = jsonConverts, ObjectCreationHandling = ObjectCreationHandling.Replace });
                return ConvertedCommand;
            }
            else if (jObject[VerificationCommandBase.CommandNameString].Value<string>() == VerificationCommandBase.CommandName_MarkAlignProcMoveToMark)
            {
                var ConvertedCommand = JsonConvert.DeserializeObject<MarkAlignProcMoveToMarkCommand>(jsonString, new JsonSerializerSettings() { Converters = jsonConverts, ObjectCreationHandling = ObjectCreationHandling.Replace });
                return ConvertedCommand;
            }
            else if (jObject[VerificationCommandBase.CommandNameString].Value<string>() == VerificationCommandBase.CommandName_MarkAlignProcFocusing)
            {
                var ConvertedCommand = JsonConvert.DeserializeObject<MarkAlignProcFocusingCommand>(jsonString, new JsonSerializerSettings() { Converters = jsonConverts, ObjectCreationHandling = ObjectCreationHandling.Replace });
                return ConvertedCommand;
            }
            else if (jObject[VerificationCommandBase.CommandNameString].Value<string>() == VerificationCommandBase.CommandName_MarkAlignProcPatternMatching)
            {
                var ConvertedCommand = JsonConvert.DeserializeObject<MarkAlignProcPatternMatchingCommand>(jsonString, new JsonSerializerSettings() { Converters = jsonConverts, ObjectCreationHandling = ObjectCreationHandling.Replace });
                return ConvertedCommand;
            }
            else if (jObject[VerificationCommandBase.CommandNameString].Value<string>() == VerificationCommandBase.CommandName_WaferAlign)
            {
                var ConvertedCommand = JsonConvert.DeserializeObject<WaferAlignCommand>(jsonString, new JsonSerializerSettings() { Converters = jsonConverts, ObjectCreationHandling = ObjectCreationHandling.Replace });
                return ConvertedCommand;
            }
            else if (jObject[VerificationCommandBase.CommandNameString].Value<string>() == VerificationCommandBase.CommandName_PinAlign) // Pin Align 기능 추가
            {
                var ConvertedCommand = JsonConvert.DeserializeObject<PinAlignCommand>(jsonString, new JsonSerializerSettings() { Converters = jsonConverts, ObjectCreationHandling = ObjectCreationHandling.Replace });
                return ConvertedCommand;
            }
            else if (jObject[VerificationCommandBase.CommandNameString].Value<string>() == VerificationCommandBase.CommandName_AxisMove)
            {
                var ConvertedCommand = JsonConvert.DeserializeObject<AxisMoveCommand>(jsonString, new JsonSerializerSettings() { Converters = jsonConverts, ObjectCreationHandling = ObjectCreationHandling.Replace });
                return ConvertedCommand;
            }
            else if (jObject[VerificationCommandBase.CommandNameString].Value<string>() == VerificationCommandBase.CommandName_MultiAxisMove)
            {
                var ConvertedCommand = JsonConvert.DeserializeObject<MultiAxisMoveCommand>(jsonString, new JsonSerializerSettings() { Converters = jsonConverts, ObjectCreationHandling = ObjectCreationHandling.Replace });
                return ConvertedCommand;
            }
            else if (jObject[VerificationCommandBase.CommandNameString].Value<string>() == VerificationCommandBase.CommandName_ChuckTiltMove) // Chuck Tilt 기능 추가
            {
                var ConvertedCommand = JsonConvert.DeserializeObject<ChuckTiltMoveCommand>(jsonString, new JsonSerializerSettings() { Converters = jsonConverts, ObjectCreationHandling = ObjectCreationHandling.Replace });
                return ConvertedCommand;
            }
            else if (jObject[VerificationCommandBase.CommandNameString].Value<string>() == VerificationCommandBase.CommandName_LightControl)
            {
                var ConvertedCommand = JsonConvert.DeserializeObject<LightControlCommand>(jsonString, new JsonSerializerSettings() { Converters = jsonConverts, ObjectCreationHandling = ObjectCreationHandling.Replace });
                return ConvertedCommand;
            }
            else if (jObject[VerificationCommandBase.CommandNameString].Value<string>() == VerificationCommandBase.CommandName_DelayTime)
            {
                var ConvertedCommand = JsonConvert.DeserializeObject<DelayTimeCommand>(jsonString, new JsonSerializerSettings() { Converters = jsonConverts, ObjectCreationHandling = ObjectCreationHandling.Replace });
                return ConvertedCommand;
            }
            else if (jObject[VerificationCommandBase.CommandNameString].Value<string>() == VerificationCommandBase.CommandName_DefaultPosMove)
            {
                var ConvertedCommand = JsonConvert.DeserializeObject<DefaultPosMoveCommand>(jsonString, new JsonSerializerSettings() { Converters = jsonConverts, ObjectCreationHandling = ObjectCreationHandling.Replace });
                return ConvertedCommand;
            }
            else if (jObject[VerificationCommandBase.CommandNameString].Value<string>() == VerificationCommandBase.CommandName_StageMove)
            {
                var ConvertedCommand = JsonConvert.DeserializeObject<StageMoveCommand>(jsonString, new JsonSerializerSettings() { Converters = jsonConverts, ObjectCreationHandling = ObjectCreationHandling.Replace });
                return ConvertedCommand;
            }
            else if (jObject[VerificationCommandBase.CommandNameString].Value<string>() == VerificationCommandBase.CommandName_PolishWaferFocusing)
            {
                var ConvertedCommand = JsonConvert.DeserializeObject<PolishWaferFocusingCommand>(jsonString, new JsonSerializerSettings() { Converters = jsonConverts, ObjectCreationHandling = ObjectCreationHandling.Replace });
                return ConvertedCommand;
            }
            else if (jObject[VerificationCommandBase.CommandNameString].Value<string>() == VerificationCommandBase.CommandName_TouchSensorOffset)
            {
                var ConvertedCommand = JsonConvert.DeserializeObject<TouchSensorOffsetCommand>(jsonString, new JsonSerializerSettings() { Converters = jsonConverts, ObjectCreationHandling = ObjectCreationHandling.Replace });
                return ConvertedCommand;
            }
            return null;
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    abstract public class VerificationCommandBase : INotifyPropertyChanged
    {
        public readonly static string CommandNameString = "CommandName";
        public readonly static string CommandName_AxisHoming = "Axis Homing";
        public readonly static string CommandName_MarkAlign = "Mark Align";
        public readonly static string CommandName_MarkAlignNoRetract = "Mark Align - No Retract";
        public readonly static string CommandName_MarkAlignProcWaferCamBridge = "Mark Align Proc - Wafer Cam Bridge";
        public readonly static string CommandName_MarkAlignProcMoveToMark = "Mark Align Proc - Move To Mark";
        public readonly static string CommandName_MarkAlignProcFocusing = "Mark Align Proc - Focusing";
        public readonly static string CommandName_MarkAlignProcPatternMatching = "Mark Align Proc - Pattern Matching";
        public readonly static string CommandName_WaferAlign = "Wafer Align";
        public readonly static string CommandName_PinAlign = "Pin Align"; // Pin Align 기능 추가
        public readonly static string CommandName_AxisMove = "Axis Move";
        public readonly static string CommandName_MultiAxisMove = "Multi Axis Move";
        public readonly static string CommandName_ChuckTiltMove = "Chuck Tilt Move"; // Chuck Tilt 기능 추가
        public readonly static string CommandName_LightControl = "Light Control";
        public readonly static string CommandName_DelayTime = "Delay Time";
        public readonly static string CommandName_DefaultPosMove = "Default Pos Move";
        public readonly static string CommandName_StageMove = "Stage Move";
        public readonly static string CommandName_PolishWaferFocusing = "Polish Wafer Focusing";
        public readonly static string CommandName_TouchSensorOffset = "Touch Sensor Sensing Offset";

        public readonly static string ParamName_RepeatCount = "Repeat Count";

        public string CommandName { get; set; }
        public ObservableCollection<CommandParameterBase> ParameterList { get; set; } = new ObservableCollection<CommandParameterBase>();
        public bool Checked { get; set; } = true;

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion
        private string _CommandTextColor = "White";
        [JsonIgnore]
        public string CommandTextColor
        {
            get { return _CommandTextColor; }
            set
            {
                _CommandTextColor = value;
                RaisePropertyChanged();
            }
        }

        public abstract bool DoCommand(ref bool IsStopScenario, ref bool IsScenarioRunFlag, ref string MsgBoxString);
        public abstract VerificationCommandBase Create();

        [JsonIgnore]
        public List<string> ParameterNameList { get; set; } = new List<string>();
        protected void AddParameterName()
        {
            foreach (var Parameter in ParameterList)
            {
                ParameterNameList.Add(Parameter.ParameterName);
            }
        }
    }

    public class AxisHomingCommand : VerificationCommandBase, IFactoryModule
    {
        public readonly static string ParamName_Axis = "Axis";
        private const string ParamName_ZClearFirst = "Z Clear First";

        public AxisHomingCommand()
        {
            CommandName = CommandName_AxisHoming;

            ObservableCollection<object> SelectItemsAxis = new ObservableCollection<object>();
            SelectItemsAxis.Add(EnumAxisConstants.X);
            SelectItemsAxis.Add(EnumAxisConstants.Y);
            SelectItemsAxis.Add(EnumAxisConstants.Z);
            SelectItemsAxis.Add(EnumAxisConstants.C);
            SelectItemsAxis.Add(EnumAxisConstants.TRI);
            SelectItemsAxis.Add(EnumAxisConstants.PZ);
            ParameterList.Add(new ComboBoxCommandParameter(ParamName_Axis, SelectItemsAxis));

            ParameterList.Add(new BoolTypeCommandParameter(ParamName_ZClearFirst, false));

            ParameterList.Add(new TextBoxCommandParameter(ParamName_RepeatCount, 1, false, 0, 0, "", true));
            AddParameterName();
        }

        public override bool DoCommand(ref bool IsStopScenario, ref bool IsScenarioRunFlag, ref string MsgBoxString)
        {
            try
            {
                if(!Checked)
                {
                    LoggerManager.CompVerifyLog($"[{CommandName}] Skip Command");
                    return true;
                }

                LoggerManager.CompVerifyLog($"[{CommandName}] Start Command");

                var AxisParam = ParameterList.Where(item => item.ParameterName == ParamName_Axis).FirstOrDefault() as ComboBoxCommandParameter;
                ProbeAxisObject HomingAxisObject = this.MotionManager().GetAxis((EnumAxisConstants)AxisParam.SelectItems.ElementAt(AxisParam.SelectedIndex));

                var ZClearFirstParam = ParameterList.Where(item => item.ParameterName == ParamName_ZClearFirst).FirstOrDefault() as BoolTypeCommandParameter;
                bool IsZClearFirst = (bool)ZClearFirstParam.SelectItems.ElementAt(ZClearFirstParam.SelectedIndex);

                var ParamRepaetCount = ParameterList.Where(item => item.ParameterName == ParamName_RepeatCount).FirstOrDefault() as TextBoxCommandParameter;
                var TotalRepeatCount = (int)ParamRepaetCount.InputValue;
                int CurrentRepeatCount = 1;

                while (CurrentRepeatCount <= TotalRepeatCount)
                {
                    if (!IsScenarioRunFlag)
                    {
                        LoggerManager.CompVerifyLog($"[{CommandName}] Stop Command (Repeat Count : {CurrentRepeatCount - 1}/{TotalRepeatCount} done)");
                        break;
                    }
                    if(IsZClearFirst)
                    {
                        LoggerManager.CompVerifyLog($"[{CommandName}] ZCLEARED Before Mark Align Process");
                        var retCode = this.StageSupervisor().StageModuleState.ZCLEARED();
                        if (EventCodeEnum.NONE != retCode)
                        {
                            // 안전사양으로 추가한 기능이므로 ZCLEARED를 실패하는 경우 전체 Scenario를 멈춘다.
                            MsgBoxString = $"[{CommandName}] Failed to ZCLEARED Before {HomingAxisObject.AxisType.Value} Axis Homing. (retCode : {retCode})";
                            LoggerManager.CompVerifyLog(MsgBoxString);
                            IsStopScenario = true;
                            return false;
                        }
                    }

                    LoggerManager.CompVerifyLog($"[{CommandName}] Axis Homing (Axis : {HomingAxisObject.AxisType.Value})");
                    this.MonitoringManager().IsMachinInitOn = true;
                    EventCodeEnum errCode = this.MotionManager().Homing(HomingAxisObject);
                    this.MonitoringManager().IsMachinInitOn = false;

                    if (errCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.CompVerifyLog($"[{CommandName}] Failed to Axis Homing (Axis : {HomingAxisObject.AxisType.Value}, ErrorCode : {errCode})");
                    }

                    CurrentRepeatCount++;
                }

                LoggerManager.CompVerifyLog($"[{CommandName}] Finish Command");
            }
            catch(Exception e)
            {
                LoggerManager.Exception(e);
                return false;
            }

            return true;
        }

        public override VerificationCommandBase Create()
        {
            return new AxisHomingCommand();
        }
    }

    public class MarkAlignCommand : VerificationCommandBase, IFactoryModule
    {
        private const string ParamName_SaveImage = "Save Image";

        public MarkAlignCommand()
        {
            CommandName = CommandName_MarkAlign;

            ParameterList.Add(new BoolTypeCommandParameter(ParamName_SaveImage, false));
            ParameterList.Add(new TextBoxCommandParameter(ParamName_RepeatCount, 1, false, 0, 0, "", true));
            AddParameterName();
        }

        public override bool DoCommand(ref bool IsStopScenario, ref bool IsScenarioRunFlag, ref string MsgBoxString)
        {
            try
            {
                if (!Checked)
                {
                    LoggerManager.CompVerifyLog($"[{CommandName}] Skip Command");
                    return true;
                }

                LoggerManager.CompVerifyLog($"[{CommandName}] Start Command");

                var SaveImageParam = ParameterList.Where(item => item.ParameterName == ParamName_SaveImage).FirstOrDefault() as BoolTypeCommandParameter;
                bool IsSaveImage = (bool)SaveImageParam.SelectItems.ElementAt(SaveImageParam.SelectedIndex);

                var ParamRepaetCount = ParameterList.Where(item => item.ParameterName == ParamName_RepeatCount).FirstOrDefault() as TextBoxCommandParameter;
                var TotalRepeatCount = (int)ParamRepaetCount.InputValue;
                int CurrentRepeatCount = 1;

                while (CurrentRepeatCount <= TotalRepeatCount)
                {
                    if (!IsScenarioRunFlag)
                    {
                        LoggerManager.CompVerifyLog($"[{CommandName}] Stop Command (Repeat Count : {CurrentRepeatCount - 1}/{TotalRepeatCount} done)");
                        break;
                    }
                    this.MarkAligner().IsSaveImageCompVerify = IsSaveImage;

                    this.StageSupervisor().StageModuleState.ZCLEARED();
                    // WaferCamBrige가 펴져있는 경우 접고 시작한다. 앞에서 Mark Align - No Retract 커맨드를 사용하는 경우 WaferCamBridge가 펴져있는 상태로 남아있기 때문!
                    if (StageCylinderType.MoveWaferCam.State == CylinderStateEnum.EXTEND)
                    {
                        int retCode = StageCylinderType.MoveWaferCam.Retract();
                        if(0 != retCode)
                        {
                            LoggerManager.CompVerifyLog($"[{CommandName}] Failed to Retract WaferCamBridge. Command Canceled.");
                            throw new Exception($"FunctionName: {MethodBase.GetCurrentMethod().Name} Returncode: {retCode} Error occurred");
                        }
                    }
                    this.StageSupervisor().StageModuleState.SetWaferCamBasePos(true);

                    EventCodeEnum errCode = this.MarkAligner().DoMarkAlign();
                    if (EventCodeEnum.NONE != errCode)
                    {
                        LoggerManager.CompVerifyLog($"[{CommandName}] Failed to Mark Align (ErrorCode : {errCode})");
                    }
                    this.StageSupervisor().StageModuleState.SetWaferCamBasePos(false);
                    this.StageSupervisor().StageModuleState.ZCLEARED();

                    this.MarkAligner().IsSaveImageCompVerify = false;

                    CurrentRepeatCount++;
                }

                LoggerManager.CompVerifyLog($"[{CommandName}] Finish Command");
            }
            catch (Exception e)
            {
                this.MarkAligner().IsSaveImageCompVerify = false;

                LoggerManager.Exception(e);
                return false;
            }

            return true;
        }

        public override VerificationCommandBase Create()
        {
            return new MarkAlignCommand();
        }
    }

    public class MarkAlignNoRetractCommand : VerificationCommandBase, IFactoryModule
    {
        private const string ParamName_SaveImage = "Save Image";

        public MarkAlignNoRetractCommand()
        {
            CommandName = CommandName_MarkAlignNoRetract;

            ParameterList.Add(new BoolTypeCommandParameter(ParamName_SaveImage, false));
            ParameterList.Add(new TextBoxCommandParameter(ParamName_RepeatCount, 1, false, 0, 0, "", true));
            AddParameterName();
        }

        public override bool DoCommand(ref bool IsStopScenario, ref bool IsScenarioRunFlag, ref string MsgBoxString)
        {
            try
            {
                if (!Checked)
                {
                    LoggerManager.CompVerifyLog($"[{CommandName}] Skip Command");
                    return true;
                }

                LoggerManager.CompVerifyLog($"[{CommandName}] Start Command");

                var SaveImageParam = ParameterList.Where(item => item.ParameterName == ParamName_SaveImage).FirstOrDefault() as BoolTypeCommandParameter;
                bool IsSaveImage = (bool)SaveImageParam.SelectItems.ElementAt(SaveImageParam.SelectedIndex);

                var ParamRepaetCount = ParameterList.Where(item => item.ParameterName == ParamName_RepeatCount).FirstOrDefault() as TextBoxCommandParameter;
                var TotalRepeatCount = (int)ParamRepaetCount.InputValue;
                int CurrentRepeatCount = 1;

                while (CurrentRepeatCount <= TotalRepeatCount)
                {
                    if (!IsScenarioRunFlag)
                    {
                        LoggerManager.CompVerifyLog($"[{CommandName}] Stop Command (Repeat Count : {CurrentRepeatCount - 1}/{TotalRepeatCount} done)");
                        break;
                    }
                    this.StageSupervisor().StageModuleState.SetNoRetractWaferCamBridgeWhenMarkAlignFlag(true);
                    this.MarkAligner().IsSaveImageCompVerify = IsSaveImage;

                    this.StageSupervisor().StageModuleState.ZCLEARED();
                    this.StageSupervisor().StageModuleState.SetWaferCamBasePos(true);

                    EventCodeEnum errCode = this.MarkAligner().DoMarkAlign();
                    if (EventCodeEnum.NONE != errCode)
                    {
                        LoggerManager.CompVerifyLog($"[{CommandName}] Failed to Mark Align (ErrorCode : {errCode})");
                    }
                    this.StageSupervisor().StageModuleState.SetWaferCamBasePos(false);
                    this.StageSupervisor().StageModuleState.ZCLEARED();

                    this.StageSupervisor().StageModuleState.SetNoRetractWaferCamBridgeWhenMarkAlignFlag(false);
                    this.MarkAligner().IsSaveImageCompVerify = false;

                    CurrentRepeatCount++;
                }

                LoggerManager.CompVerifyLog($"[{CommandName}] Finish Command");
            }
            catch (Exception e)
            {
                this.StageSupervisor().StageModuleState.SetNoRetractWaferCamBridgeWhenMarkAlignFlag(false);
                this.MarkAligner().IsSaveImageCompVerify = false;

                LoggerManager.Exception(e);
                return false;
            }

            return true;
        }

        public override VerificationCommandBase Create()
        {
            return new MarkAlignNoRetractCommand();
        }
    }

    public class MarkAlignProcWaferCamBridgeCommand : VerificationCommandBase, IFactoryModule
    {
        public readonly static string ParamName_BridgeMoveType = "Move Type";
        private const string ParamName_StopWhenCommandFail = "Stop When Command Fail";

        public MarkAlignProcWaferCamBridgeCommand()
        {
            CommandName = CommandName_MarkAlignProcWaferCamBridge;

            ObservableCollection<object> SelectItemsMoveType = new ObservableCollection<object>();
            SelectItemsMoveType.Add(CylinderStateEnum.EXTEND);
            SelectItemsMoveType.Add(CylinderStateEnum.RETRACT);

            ParameterList.Add(new ComboBoxCommandParameter(ParamName_BridgeMoveType, SelectItemsMoveType));
            ParameterList.Add(new BoolTypeCommandParameter(ParamName_StopWhenCommandFail, false));
            ParameterList.Add(new TextBoxCommandParameter(ParamName_RepeatCount, 1, false, 0, 0, "", true));
            AddParameterName();
        }

        public override bool DoCommand(ref bool IsStopScenario, ref bool IsScenarioRunFlag, ref string MsgBoxString)
        {
            try
            {
                if (!Checked)
                {
                    LoggerManager.CompVerifyLog($"[{CommandName}] Skip Command");
                    return true;
                }

                LoggerManager.CompVerifyLog($"[{CommandName}] Start Command");

                var ParamBridgeMoveType = ParameterList.Where(item => item.ParameterName == ParamName_BridgeMoveType).FirstOrDefault() as ComboBoxCommandParameter;
                var BridgeMoveType = (CylinderStateEnum)ParamBridgeMoveType.SelectItems.ElementAt(ParamBridgeMoveType.SelectedIndex);

                var ParamStopWhenCommandFail = ParameterList.Where(item => item.ParameterName == ParamName_StopWhenCommandFail).FirstOrDefault() as BoolTypeCommandParameter;
                var IsStopWhenCommandFail = (bool)ParamStopWhenCommandFail.SelectItems.ElementAt(ParamStopWhenCommandFail.SelectedIndex);

                var ParamRepaetCount = ParameterList.Where(item => item.ParameterName == ParamName_RepeatCount).FirstOrDefault() as TextBoxCommandParameter;
                var TotalRepeatCount = (int)ParamRepaetCount.InputValue;
                int CurrentRepeatCount = 1;

                while (CurrentRepeatCount <= TotalRepeatCount)
                {
                    if (!IsScenarioRunFlag)
                    {
                        LoggerManager.CompVerifyLog($"[{CommandName}] Stop Command (Repeat Count : {CurrentRepeatCount - 1}/{TotalRepeatCount} done)");
                        break;
                    }
                    int retCode = 0;

                    LoggerManager.CompVerifyLog($"[{CommandName}] Move Wafer Cam Bridge. (BridgeMoveType : {BridgeMoveType})");
                    switch (BridgeMoveType)
                    {
                        case CylinderStateEnum.EXTEND:
                            retCode = StageCylinderType.MoveWaferCam.Extend();
                            break;
                        case CylinderStateEnum.RETRACT:
                            retCode = StageCylinderType.MoveWaferCam.Retract();
                            break;
                        default:
                            break;
                    }

                    if (0 != retCode)
                    {
                        MsgBoxString = $"[{CommandName}] Failed to Move Wafer Cam Bridge. (MoveType : {BridgeMoveType}, ErrorCode : {retCode}, StopScenario : {IsStopWhenCommandFail})";
                        LoggerManager.CompVerifyLog(MsgBoxString);

                        if(IsStopWhenCommandFail)
                        {
                            IsStopScenario = true;
                        }

                        throw new Exception($"FunctionName: {MethodBase.GetCurrentMethod().Name} Returncode: {retCode} Error occurred");
                    }

                    CurrentRepeatCount++;
                }

                LoggerManager.CompVerifyLog($"[{CommandName}] Finish Command");
            }
            catch (Exception e)
            {
                LoggerManager.Exception(e);
                return false;
            }

            return true;
        }

        public override VerificationCommandBase Create()
        {
            return new MarkAlignProcWaferCamBridgeCommand();
        }
    }

    public class MarkAlignProcMoveToMarkCommand : VerificationCommandBase, IFactoryModule
    {
        private const string ParamName_ZClearFirst = "Z Clear First";

        public MarkAlignProcMoveToMarkCommand()
        {
            CommandName = CommandName_MarkAlignProcMoveToMark;

            ParameterList.Add(new BoolTypeCommandParameter(ParamName_ZClearFirst));
            ParameterList.Add(new TextBoxCommandParameter(ParamName_RepeatCount, 1, false, 0, 0, "", true));
            AddParameterName();
        }

        public override bool DoCommand(ref bool IsStopScenario, ref bool IsScenarioRunFlag, ref string MsgBoxString)
        {
            try
            {
                if (!Checked)
                {
                    LoggerManager.CompVerifyLog($"[{CommandName}] Skip Command");
                    return true;
                }

                LoggerManager.CompVerifyLog($"[{CommandName}] Start Command");

                var ParamZClearFirst = ParameterList.Where(item => item.ParameterName == ParamName_ZClearFirst).FirstOrDefault() as BoolTypeCommandParameter;
                var IsZClearFirst = (bool)ParamZClearFirst.SelectItems.ElementAt(ParamZClearFirst.SelectedIndex);

                var ParamRepaetCount = ParameterList.Where(item => item.ParameterName == ParamName_RepeatCount).FirstOrDefault() as TextBoxCommandParameter;
                var TotalRepeatCount = (int)ParamRepaetCount.InputValue;
                int CurrentRepeatCount = 1;

                while (CurrentRepeatCount <= TotalRepeatCount)
                {
                    if (!IsScenarioRunFlag)
                    {
                        LoggerManager.CompVerifyLog($"[{CommandName}] Stop Command (Repeat Count : {CurrentRepeatCount - 1}/{TotalRepeatCount} done)");
                        break;
                    }
                    if (IsZClearFirst)
                    {
                        LoggerManager.CompVerifyLog($"[{CommandName}] ZCLEARED Before Mark Align Process");
                        var retCode = this.StageSupervisor().StageModuleState.ZCLEARED();
                        if(EventCodeEnum.NONE != retCode)
                        {
                            // 안전사양으로 추가한 기능이므로 ZCLEARED를 실패하는 경우 전체 Scenario를 멈춘다.
                            MsgBoxString = $"[{CommandName}] Failed to ZCLEARED Before Mark Align Process. (retCode : {retCode})";
                            LoggerManager.CompVerifyLog(MsgBoxString);
                            IsStopScenario = true;
                            return false;
                        }
                    }

                    LoggerManager.CompVerifyLog($"[{CommandName}] Do Mark Align Process Only MoveToMark");
                    EventCodeEnum errCode = MoveToMark();
                    if (EventCodeEnum.NONE != errCode)
                    {
                        // MoveToMark가 실패하는 경우 축 이동에 문제가 생긴 경우이므로 안전을 위해 전체 Scenario를 멈춘다.
                        MsgBoxString = $"[{CommandName}] Failed to Mark Align Process Only MoveToMark. (ErrorCode : {errCode})";
                        LoggerManager.CompVerifyLog(MsgBoxString);
                        IsStopScenario = true;
                        return false;
                    }

                    CurrentRepeatCount++;
                }

                LoggerManager.CompVerifyLog($"[{CommandName}] Finish Command");
            }
            catch (Exception e)
            {
                LoggerManager.Exception(e);
                return false;
            }

            return true;
        }

        public override VerificationCommandBase Create()
        {
            return new MarkAlignProcMoveToMarkCommand();
        }

        private EventCodeEnum MoveToMark()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            var axisx = this.MotionManager().GetAxis(EnumAxisConstants.X);
            var axisy = this.MotionManager().GetAxis(EnumAxisConstants.Y);
            var axisz = this.MotionManager().GetAxis(EnumAxisConstants.Z);
            var axispz = this.MotionManager().GetAxis(EnumAxisConstants.PZ);
            try
            {

                double xpos = this.CoordinateManager().StageCoord.RefMarkPos.X.Value;
                double ypos = this.CoordinateManager().StageCoord.RefMarkPos.Y.Value;
                double zpos = axisz.Param.HomeOffset.Value;
                double refPz = this.CoordinateManager().StageCoord.RefMarkPos.Z.Value;
                double curZpos = 0.0;
                double curXpos = 0.0;
                double curYpos = 0.0;
                double curPZ = 0;

                curXpos = axisx.Status.Position.Ref;
                curYpos = axisy.Status.Position.Ref;
                curZpos = axisz.Status.Position.Ref;
                curPZ = axispz.Status.Position.Ref;

                if (curZpos > zpos)
                {
                    ret = this.MotionManager().StageMove(curXpos, curYpos, zpos);
                    ResultValidate(MethodBase.GetCurrentMethod(), ret);

                    ret = this.MotionManager().StageMove(xpos, ypos, zpos);
                    ResultValidate(MethodBase.GetCurrentMethod(), ret);

                    ret = this.MotionManager().AbsMove(EnumAxisConstants.PZ, refPz);
                    ResultValidate(MethodBase.GetCurrentMethod(), ret);
                }
                else
                {
                    ret = this.MotionManager().StageMove(xpos, ypos, curZpos);
                    ResultValidate(MethodBase.GetCurrentMethod(), ret);

                    ret = this.MotionManager().AbsMove(EnumAxisConstants.PZ, refPz);
                    ResultValidate(MethodBase.GetCurrentMethod(), ret);
                    LoggerManager.Debug($"[MoveToMarkPos] x:{xpos}, y:{ypos}, pz:{refPz}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error ocurred  while this function : {MethodBase.GetCurrentMethod().Name}, Err = {err.Message}");
            }

            return ret;
        }

        private EventCodeEnum ResultValidate(object funcname, EventCodeEnum retcode)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            ret = retcode;

            if (retcode != EventCodeEnum.NONE)
            {
                LoggerManager.Debug($"ResultValidate Fail :  Error code = {retcode.ToString()}, fucntion name = {funcname.ToString()}");
                throw new Exception($"FunctionName: {funcname.ToString()} Returncode: {retcode.ToString()} Error occurred");
            }

            return ret;
        }
    }

    public class MarkAlignProcFocusingCommand : VerificationCommandBase, IFactoryModule
    {
        public MarkAlignProcFocusingCommand()
        {
            CommandName = CommandName_MarkAlignProcFocusing;

            ParameterList.Add(new TextBoxCommandParameter(ParamName_RepeatCount, 1, false, 0, 0, "", true));
            AddParameterName();
        }

        public override bool DoCommand(ref bool IsStopScenario, ref bool IsScenarioRunFlag, ref string MsgBoxString)
        {
            try
            {
                if (!Checked)
                {
                    LoggerManager.CompVerifyLog($"[{CommandName}] Skip Command");
                    return true;
                }

                LoggerManager.CompVerifyLog($"[{CommandName}] Start Command");

                var ParamRepaetCount = ParameterList.Where(item => item.ParameterName == ParamName_RepeatCount).FirstOrDefault() as TextBoxCommandParameter;
                var TotalRepeatCount = (int)ParamRepaetCount.InputValue;
                int CurrentRepeatCount = 1;

                while (CurrentRepeatCount <= TotalRepeatCount)
                {
                    if (!IsScenarioRunFlag)
                    {
                        LoggerManager.CompVerifyLog($"[{CommandName}] Stop Command (Repeat Count : {CurrentRepeatCount - 1}/{TotalRepeatCount} done)");
                        break;
                    }
                    LoggerManager.CompVerifyLog($"[{CommandName}] Do Mark Align Process Only Focusing");
                    EventCodeEnum errCode = this.MarkAligner().DoMarkAlign(true, false, EnumMarkAlignProcMode.OnlyFocusing);
                    if (EventCodeEnum.NONE != errCode)
                    {
                        LoggerManager.CompVerifyLog($"[{CommandName}] Failed to Mark Align Process Only Focusing. (ErrorCode : {errCode})");
                    }

                    CurrentRepeatCount++;
                }

                LoggerManager.CompVerifyLog($"[{CommandName}] Finish Command");
            }
            catch (Exception e)
            {
                LoggerManager.Exception(e);
                return false;
            }

            return true;
        }

        public override VerificationCommandBase Create()
        {
            return new MarkAlignProcFocusingCommand();
        }
    }

    public class MarkAlignProcPatternMatchingCommand : VerificationCommandBase, IFactoryModule
    {
       public MarkAlignProcPatternMatchingCommand()
        {
            CommandName = CommandName_MarkAlignProcPatternMatching;

            ParameterList.Add(new TextBoxCommandParameter(ParamName_RepeatCount, 1, false, 0, 0, "", true));
            AddParameterName();
        }

        public override bool DoCommand(ref bool IsStopScenario, ref bool IsScenarioRunFlag, ref string MsgBoxString)
        {
            try
            {
                if (!Checked)
                {
                    LoggerManager.CompVerifyLog($"[{CommandName}] Skip Command");
                    return true;
                }

                LoggerManager.CompVerifyLog($"[{CommandName}] Start Command");

                var ParamRepaetCount = ParameterList.Where(item => item.ParameterName == ParamName_RepeatCount).FirstOrDefault() as TextBoxCommandParameter;
                var TotalRepeatCount = (int)ParamRepaetCount.InputValue;
                int CurrentRepeatCount = 1;

                while (CurrentRepeatCount <= TotalRepeatCount)
                {
                    if (!IsScenarioRunFlag)
                    {
                        LoggerManager.CompVerifyLog($"[{CommandName}] Stop Command (Repeat Count : {CurrentRepeatCount - 1}/{TotalRepeatCount} done)");
                        break;
                    }
                    LoggerManager.CompVerifyLog($"[{CommandName}] Do Mark Align Process Only Pattern Matching");
                    EventCodeEnum errCode = this.MarkAligner().DoMarkAlign(true, false, EnumMarkAlignProcMode.OnlyPatternMatching);
                    if (EventCodeEnum.NONE != errCode)
                    {
                        LoggerManager.CompVerifyLog($"[{CommandName}] Failed to Mark Align Process Only Pattern Matching. (ErrorCode : {errCode})");
                    }

                    CurrentRepeatCount++;
                }

                LoggerManager.CompVerifyLog($"[{CommandName}] Finish Command");
            }
            catch (Exception e)
            {
                LoggerManager.Exception(e);
                return false;
            }

            return true;
        }

        public override VerificationCommandBase Create()
        {
            return new MarkAlignProcPatternMatchingCommand();
        }
    }

    public class WaferAlignCommand : VerificationCommandBase, IFactoryModule
    {
        public WaferAlignCommand()
        {
            CommandName = CommandName_WaferAlign;

            ParameterList.Add(new TextBoxCommandParameter(ParamName_RepeatCount, 1, false, 0, 0, "", true));
            AddParameterName();
        }

        public override bool DoCommand(ref bool IsStopScenario, ref bool IsScenarioRunFlag, ref string MsgBoxString)
        {
            try
            {
                if (!Checked)
                {
                    LoggerManager.CompVerifyLog($"[{CommandName}] Skip Command");
                    return true;
                }

                LoggerManager.CompVerifyLog($"[{CommandName}] Start Command");

                var ParamRepaetCount = ParameterList.Where(item => item.ParameterName == ParamName_RepeatCount).FirstOrDefault() as TextBoxCommandParameter;
                var TotalRepeatCount = (int)ParamRepaetCount.InputValue;
                int CurrentRepeatCount = 1;

                while (CurrentRepeatCount <= TotalRepeatCount)
                {
                    if (!IsScenarioRunFlag)
                    {
                        LoggerManager.CompVerifyLog($"[{CommandName}] Stop Command (Repeat Count : {CurrentRepeatCount - 1}/{TotalRepeatCount} done)");
                        break;
                    }
                    EventCodeEnum errCode = EventCodeEnum.UNDEFINED;
                    bool isError = false;

                    errCode = this.WaferAligner().DoManualOperation();

                    if (errCode != EventCodeEnum.NONE)
                    {
                        isError = true;
                        LoggerManager.CompVerifyLog($"[{CommandName}] Failed to PinPadMatch Test. (errCode : {errCode})");
                        return false;
                    }

                    int retVal = StageCylinderType.MoveWaferCam.Retract();
                    if (retVal != 0)
                    {
                        isError = true;
                        LoggerManager.CompVerifyLog($"[{CommandName}] Failed to Retract Wafer Cam Bridge. (errCode : {errCode})");
                        return false;
                    }


                    var wafer_object = (WaferObject)this.StageSupervisor().WaferObject;
                    WaferCoordinate wafercoord = new WaferCoordinate();
                    PinCoordinate pincoord = new PinCoordinate();
                    //Wafer Center로 갈건지 아니면 PadCenter로 갈건지 계산해야됨.
                    wafercoord.X.Value = wafer_object.GetSubsInfo().WaferCenter.X.Value;
                    wafercoord.Y.Value = wafer_object.GetSubsInfo().WaferCenter.Y.Value;
                    MachineIndex MI = new MachineIndex();
                    try
                    {
                        errCode = this.ProbingModule().ProbingSequenceModule().GetFirstSequence(ref MI);

                        if (errCode == EventCodeEnum.NONE)
                        {
                            var Wafer = this.WaferAligner().MachineIndexConvertToProbingCoord((int)MI.XIndex, (int)MI.YIndex);
                            wafercoord.X.Value = Wafer.X.Value;
                            wafercoord.Y.Value = Wafer.Y.Value;
                            wafercoord.T.Value = Wafer.T.Value;
                            LoggerManager.Debug($"[ComponentVerification] Used GetFirstSequence Position");
                        }
                        else
                        {
                            wafercoord.X.Value = wafer_object.GetSubsInfo().WaferCenter.X.Value;
                            wafercoord.Y.Value = wafer_object.GetSubsInfo().WaferCenter.Y.Value;
                            LoggerManager.Debug($"[ComponentVerification] Used WaferCenter Position");
                        }
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, e);
                        LoggerManager.Debug($"[ComponentVerification] Probing GetFirstSequence() Error. Used WaferCenter Position");
                        wafercoord.X.Value = wafer_object.GetSubsInfo().WaferCenter.X.Value;
                        wafercoord.Y.Value = wafer_object.GetSubsInfo().WaferCenter.Y.Value;
                    }


                    wafercoord.Z.Value = this.StageSupervisor().WaferObject.GetSubsInfo().ActualThickness;

                    pincoord.X.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenX;
                    pincoord.Y.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenY;
                    pincoord.Z.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinHeight;
                    LoggerManager.Debug($"[ComponentVerification] PinPadPosition(), wafercoord(X, Y ,Z) = { wafercoord.X.Value}, {wafercoord.Y.Value}, {wafercoord.Z.Value}, ");
                    LoggerManager.Debug($"[ComponentVerification] PinPadPosition(), pincoord(X, Y ,Z) = { pincoord.X.Value}, {pincoord.Y.Value}, {pincoord.Z.Value}");
                    var zclearance = -10000;

                    LoggerManager.Debug($"[ComponentVerification] zclearance= { zclearance}");


                    errCode = this.StageSupervisor().StageModuleState.MoveToSoaking(wafercoord, pincoord, zclearance);
                    if (errCode != EventCodeEnum.NONE)
                    {
                        isError = true;
                        LoggerManager.CompVerifyLog($"[{CommandName}] Failed to MoveToSoaking. (errCode : {errCode})");
                        return false;
                    }


                    errCode = this.StageSupervisor().StageModuleState.ZCLEARED();
                    if (errCode != EventCodeEnum.NONE)
                    {
                        isError = true;
                        LoggerManager.CompVerifyLog($"[{CommandName}] Failed to ZCLEARED. (errCode : {errCode})");
                        return false;
                    }

                    if (!isError)
                    {
                        LoggerManager.CompVerifyLog($"[{CommandName}] Success WaferAlign");
                    }

                    CurrentRepeatCount++;
                }

                LoggerManager.CompVerifyLog($"[{CommandName}] Finish Command");
            }
            catch (Exception e)
            {
                LoggerManager.Exception(e);
                return false;
            }

            return true;
        }

        public override VerificationCommandBase Create()
        {
            return new WaferAlignCommand();
        }
    }
    
    /// Pin Align Command
    public class PinAlignCommand : VerificationCommandBase, IFactoryModule
    {
        public PinAlignCommand()
        {
            CommandName = CommandName_PinAlign;

            ParameterList.Add(new TextBoxCommandParameter(ParamName_RepeatCount, 1, false, 0, 0, "", true));
            AddParameterName();
        }

        public override bool DoCommand(ref bool IsStopScenario, ref bool IsScenarioRunFlag, ref string MsgBoxString)
        {
            try
            {
                if (!Checked)
                {
                    LoggerManager.CompVerifyLog($"[{CommandName}] Skip Command");
                    return true;
                }

                LoggerManager.CompVerifyLog($"[{CommandName}] Start Command");

                var ParamRepaetCount = ParameterList.Where(item => item.ParameterName == ParamName_RepeatCount).FirstOrDefault() as TextBoxCommandParameter;
                var TotalRepeatCount = (int)ParamRepaetCount.InputValue;
                int CurrentRepeatCount = 1;

                while (CurrentRepeatCount <= TotalRepeatCount)
                {
                    if (!IsScenarioRunFlag)
                    {
                        LoggerManager.CompVerifyLog($"[{CommandName}] Stop Command (Repeat Count : {CurrentRepeatCount - 1}/{TotalRepeatCount} done)");
                        break;
                    }
                    this.StageSupervisor().StageModuleState.ZCLEARED();

                    ICamera curcam = Extensions_IModule.VisionManager(null).GetCam(EnumProberCam.PIN_HIGH_CAM);
                    List<LightValueParam> lights = new List<LightValueParam>();
                    foreach (var light in curcam.LightsChannels)
                    {
                        lights.Add(new LightValueParam(light.Type.Value, (ushort)curcam.GetLight(light.Type.Value)));
                    }

                    this.PinAligner().ClearState();

                    var retCode = this.PinAligner().DoManualOperation();

                    if(EventCodeEnum.NONE != retCode)
                    {
                        LoggerManager.CompVerifyLog($"[{CommandName}] Failed to Pin Align. (ErrorCode : {retCode})");
                    }
                    else
                    {
                        LoggerManager.CompVerifyLog($"[{CommandName}] Success to Pin Align)");
                    }

                    this.VisionManager().StartGrab(curcam.GetChannelType(), this);

                    foreach (var light in lights)
                    {
                        curcam.SetLight(light.Type.Value, light.Value.Value);
                    }

                    this.StageSupervisor().StageModuleState.ZCLEARED();

                    CurrentRepeatCount++;
                }

                LoggerManager.CompVerifyLog($"[{CommandName}] Finish Command");
            }
            catch (Exception e)
            {
                LoggerManager.Exception(e);
                return false;
            }

            return true;
        }

        public override VerificationCommandBase Create()
        {
            return new PinAlignCommand();
        }
    }

    public class AxisMoveCommand : VerificationCommandBase, IFactoryModule
    {
        public readonly static string ParamName_Axis = "Axis";
        public readonly static string ParamName_MoveType = "Move Type";
        private const string ParamName_PositionValue = "Position Value";

        public AxisMoveCommand()
        {
            CommandName = CommandName_AxisMove;

            ObservableCollection<object> SelectItemsAxis = new ObservableCollection<object>();
            SelectItemsAxis.Add(EnumAxisConstants.X);
            SelectItemsAxis.Add(EnumAxisConstants.Y);
            SelectItemsAxis.Add(EnumAxisConstants.Z);
            SelectItemsAxis.Add(EnumAxisConstants.C);
            SelectItemsAxis.Add(EnumAxisConstants.TRI);
            SelectItemsAxis.Add(EnumAxisConstants.PZ);
            ParameterList.Add(new ComboBoxCommandParameter(ParamName_Axis, SelectItemsAxis));

            ObservableCollection<object> SelectItemsMoveType = new ObservableCollection<object>();
            SelectItemsMoveType.Add(EnumMoveType.Relative);
            SelectItemsMoveType.Add(EnumMoveType.Absolute);
            ParameterList.Add(new ComboBoxCommandParameter(ParamName_MoveType, SelectItemsMoveType));
            ParameterList.Add(new TextBoxCommandParameter(ParamName_PositionValue));
            AddParameterName();
        }

        public override bool DoCommand(ref bool IsStopScenario, ref bool IsScenarioRunFlag, ref string MsgBoxString)
        {
            try
            {
                if (!Checked)
                {
                    LoggerManager.CompVerifyLog($"[{CommandName}] Skip Command");
                    return true;
                }

                LoggerManager.CompVerifyLog($"[{CommandName}] Start Command");

                var AxisParam = ParameterList.Where(item => item.ParameterName == ParamName_Axis).FirstOrDefault() as ComboBoxCommandParameter;
                ProbeAxisObject MoveAxisObject = this.MotionManager().GetAxis((EnumAxisConstants)AxisParam.SelectItems.ElementAt(AxisParam.SelectedIndex));

                var MoveTypeParam = ParameterList.Where(item => item.ParameterName == ParamName_MoveType).FirstOrDefault() as ComboBoxCommandParameter;
                var SelectedMoveType = (EnumMoveType)MoveTypeParam.SelectItems.ElementAt(MoveTypeParam.SelectedIndex);

                var PositionValueParam = ParameterList.Where(item => item.ParameterName == ParamName_PositionValue).FirstOrDefault() as TextBoxCommandParameter;

                EventCodeEnum errCode = EventCodeEnum.UNDEFINED;

                LoggerManager.CompVerifyLog($"[{CommandName}] Axis Move (Axis : {MoveAxisObject.AxisType.Value}, MoveType : {SelectedMoveType}, PositionValue : {PositionValueParam.InputValue})");
                switch (SelectedMoveType)
                {
                    case EnumMoveType.Absolute:
                        {
                            errCode = this.MotionManager().AbsMove(MoveAxisObject, PositionValueParam.InputValue);
                            break;
                        }
                    case EnumMoveType.Relative:
                        {
                            errCode = this.MotionManager().RelMove(MoveAxisObject, PositionValueParam.InputValue);
                            break;
                        }
                    default:
                        break;
                }

                if(errCode != EventCodeEnum.NONE)
                {
                    LoggerManager.CompVerifyLog($"[{CommandName}] Failed to Axis Move (Axis : {MoveAxisObject.AxisType.Value}, EventCode : {errCode})");
                }

                LoggerManager.CompVerifyLog($"[{CommandName}] Finish Command");
            }
            catch (Exception e)
            {
                LoggerManager.Exception(e);
                return false;
            }

            return true;
        }

        public override VerificationCommandBase Create()
        {
            return new AxisMoveCommand();
        }
    }

    public class MultiAxisMoveCommand : VerificationCommandBase, IFactoryModule
    {
        private const string ParamName_ZClearFirst = "Z Clear First";
        public readonly static string ParamName_MoveType = "Move Type";
        private const string ParamName_XPos = "X Position";
        private const string ParamName_YPos = "Y Position";
        private const string ParamName_ZPos = "Z Position";
        private const string ParamName_PZPos = "PZ Position";

        public MultiAxisMoveCommand()
        {
            CommandName = CommandName_MultiAxisMove;

            ParameterList.Add(new BoolTypeCommandParameter(ParamName_ZClearFirst));

            ObservableCollection<object> SelectItemsMoveType = new ObservableCollection<object>();
            SelectItemsMoveType.Add(EnumMoveType.Relative);
            SelectItemsMoveType.Add(EnumMoveType.Absolute);
            ParameterList.Add(new ComboBoxCommandParameter(ParamName_MoveType, SelectItemsMoveType));

            ParameterList.Add(new TextBoxCommandParameter(ParamName_XPos));
            ParameterList.Add(new TextBoxCommandParameter(ParamName_YPos));
            ParameterList.Add(new TextBoxCommandParameter(ParamName_ZPos));
            ParameterList.Add(new TextBoxCommandParameter(ParamName_PZPos));
            AddParameterName();
        }

        public override bool DoCommand(ref bool IsStopScenario, ref bool IsScenarioRunFlag, ref string MsgBoxString)
        {
            try
            {
                if (!Checked)
                {
                    LoggerManager.CompVerifyLog($"[{CommandName}] Skip Command");
                    return true;
                }

                LoggerManager.CompVerifyLog($"[{CommandName}] Start Command");

                var ParamZClearFirst = ParameterList.Where(item => item.ParameterName == ParamName_ZClearFirst).FirstOrDefault() as BoolTypeCommandParameter;
                var IsZClearFirst = (bool)ParamZClearFirst.SelectItems.ElementAt(ParamZClearFirst.SelectedIndex);

                var MoveTypeParam = ParameterList.Where(item => item.ParameterName == ParamName_MoveType).FirstOrDefault() as ComboBoxCommandParameter;
                var SelectedMoveType = (EnumMoveType)MoveTypeParam.SelectItems.ElementAt(MoveTypeParam.SelectedIndex);

                var XPosParam = ParameterList.Where(item => item.ParameterName == ParamName_XPos).FirstOrDefault() as TextBoxCommandParameter;
                var YPosParam = ParameterList.Where(item => item.ParameterName == ParamName_YPos).FirstOrDefault() as TextBoxCommandParameter;
                var ZPosParam = ParameterList.Where(item => item.ParameterName == ParamName_ZPos).FirstOrDefault() as TextBoxCommandParameter;
                var PZPosParam = ParameterList.Where(item => item.ParameterName == ParamName_PZPos).FirstOrDefault() as TextBoxCommandParameter;

                if(IsZClearFirst)
                {
                    LoggerManager.CompVerifyLog($"[{CommandName}] ZCLEARED Before Mark Align Process");
                    var retCode = this.StageSupervisor().StageModuleState.ZCLEARED();
                    if (EventCodeEnum.NONE != retCode)
                    {
                        // 안전사양으로 추가한 기능이므로 ZCLEARED를 실패하는 경우 전체 Scenario를 멈춘다.
                        MsgBoxString = $"[{CommandName}] Failed to ZCLEARED Before Mark Align Process. (retCode : {retCode})";
                        LoggerManager.CompVerifyLog(MsgBoxString);
                        IsStopScenario = true;
                        return false;
                    }
                }

                ProbeAxisObject MoveAxisObjectPZ = this.MotionManager().GetAxis(EnumAxisConstants.PZ);

                LoggerManager.CompVerifyLog($"[{CommandName}] Multi Axis Move (MoveType : {SelectedMoveType}, X Pos : {XPosParam.InputValue}, Y Pos : {YPosParam.InputValue}, Z Pos : {ZPosParam.InputValue}, PZ Pos : {PZPosParam.InputValue})");
                int errCode = 0;
                EventCodeEnum eventCode = EventCodeEnum.NONE;
                switch (SelectedMoveType)
                {
                    case EnumMoveType.Absolute:
                        {
                            errCode = this.MotionManager().StageMoveAync(XPosParam.InputValue, YPosParam.InputValue, ZPosParam.InputValue);
                            if (0 != errCode)
                            {
                                LoggerManager.CompVerifyLog($"[{CommandName}] Failed to StageMoveAync. (errCode : {errCode})");
                                return false;
                            }
                            else
                            {
                                eventCode = this.MotionManager().AbsMove(MoveAxisObjectPZ, PZPosParam.InputValue);
                                if (EventCodeEnum.NONE != eventCode)
                                {
                                    LoggerManager.CompVerifyLog($"[{CommandName}] Failed to PZ Axis AbsMove. (errCode : {errCode})");
                                    return false;
                                }
                            }
                            break;
                        }
                    case EnumMoveType.Relative:
                        {
                            errCode = this.MotionManager().StageRelMoveAsync(XPosParam.InputValue, YPosParam.InputValue, ZPosParam.InputValue);
                            if (0 != errCode)
                            {
                                LoggerManager.CompVerifyLog($"[{CommandName}] Failed to StageRelMoveAsync. (errCode : {errCode})");
                                return false;
                            }
                            else
                            {
                                eventCode = this.MotionManager().RelMove(MoveAxisObjectPZ, PZPosParam.InputValue);
                                if(EventCodeEnum.NONE != eventCode)
                                {
                                    LoggerManager.CompVerifyLog($"[{CommandName}] Failed to PZ Axis RelMove. (errCode : {errCode})");
                                    return false;
                                }
                            }
                            break;
                        }
                    default:
                        break;
                }

                LoggerManager.CompVerifyLog($"[{CommandName}] Finish Command");
            }
            catch (Exception e)
            {
                LoggerManager.Exception(e);
                return false;
            }

            return true;
        }

        public override VerificationCommandBase Create()
        {
            return new MultiAxisMoveCommand();
        }
    }

    // Chuck Tilt Move Command 추가
    public class ChuckTiltMoveCommand : VerificationCommandBase, IFactoryModule
    {
        private const string ParamName_OffsetZ0 = "Offset Z0";
        private const string ParamName_OffsetZ1 = "Offset Z1";
        private const string ParamName_OffsetZ2 = "Offset Z2";

        public ChuckTiltMoveCommand()
        {
            CommandName = CommandName_ChuckTiltMove;

            ParameterList.Add(new TextBoxCommandParameter(ParamName_OffsetZ0));
            ParameterList.Add(new TextBoxCommandParameter(ParamName_OffsetZ1));
            ParameterList.Add(new TextBoxCommandParameter(ParamName_OffsetZ2));
            AddParameterName();
        }

        public override bool DoCommand(ref bool IsStopScenario, ref bool IsScenarioRunFlag, ref string MsgBoxString)
        {
            try
            {
                if (!Checked)
                {
                    LoggerManager.CompVerifyLog($"[{CommandName}] Skip Command");
                    return true;
                }

                LoggerManager.CompVerifyLog($"[{CommandName}] Start Command");

                var OffsetZ0Param = ParameterList.Where(item => item.ParameterName == ParamName_OffsetZ0).FirstOrDefault() as TextBoxCommandParameter;
                var OffsetZ1Param = ParameterList.Where(item => item.ParameterName == ParamName_OffsetZ1).FirstOrDefault() as TextBoxCommandParameter;
                var OffsetZ2Param = ParameterList.Where(item => item.ParameterName == ParamName_OffsetZ2).FirstOrDefault() as TextBoxCommandParameter;

                LoggerManager.CompVerifyLog($"[{CommandName}] ChuckTiltMove (Z0 : {OffsetZ0Param.InputValue}, Z1 : {OffsetZ1Param.InputValue}, Z2 : {OffsetZ2Param.InputValue})");
                var AxisZ = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                var errCode = this.MotionManager().GetMotionProvider().ChuckTiltMove(AxisZ, OffsetZ0Param.InputValue, OffsetZ1Param.InputValue, OffsetZ2Param.InputValue, AxisZ.Status.RawPosition.Ref, AxisZ.Param.Speed.Value * 0.5, AxisZ.Param.Acceleration.Value * 0.5);
                if (0 != errCode)
                {
                    LoggerManager.CompVerifyLog($"[{CommandName}] Failed to ChuckTiltMove. (errCode : {errCode})");
                    LoggerManager.CompVerifyLog($"[{CommandName}] Finish Command");
                    return false;
                }
                else
                {
                    errCode = this.MotionManager().WaitForAxisMotionDone(AxisZ);
                    if (0 != errCode)
                    {
                        LoggerManager.CompVerifyLog($"[{CommandName}] Failed to ChuckTiltMove - WaitForAxisMotionDone. (errCode : {errCode})");
                        LoggerManager.CompVerifyLog($"[{CommandName}] Finish Command");
                        return false;
                    }
                }

                LoggerManager.CompVerifyLog($"[{CommandName}] Finish Command");
            }
            catch (Exception e)
            {
                LoggerManager.Exception(e);
                return false;
            }

            return true;
        }

        public override VerificationCommandBase Create()
        {
            return new ChuckTiltMoveCommand();
        }
    }

    public class LightControlCommand : VerificationCommandBase, IFactoryModule
    {
        public readonly static string ParamName_CameraType = "Camera Type";
        public readonly static string ParamName_LightType = "Light Type";
        private const string ParamName_LightValue = "Light Value";

        //==> 카메라 타입들
        public ObservableCollection<object> SelectItemsCameraType { get; set; }

        //==> 현재 선택된 카메라 조명 타입들
        public ObservableCollection<object> SelectItemsLightType { get; set; }

        public LightControlCommand()
        {
            CommandName = CommandName_LightControl;

            SelectItemsCameraType = new ObservableCollection<object>();
            SelectItemsCameraType.Add(EnumProberCam.WAFER_HIGH_CAM);
            SelectItemsCameraType.Add(EnumProberCam.WAFER_LOW_CAM);
            SelectItemsCameraType.Add(EnumProberCam.PIN_HIGH_CAM);
            SelectItemsCameraType.Add(EnumProberCam.PIN_LOW_CAM);
            ParameterList.Add(new ComboBoxCommandParameter(ParamName_CameraType, SelectItemsCameraType));

            SelectItemsLightType = new ObservableCollection<object>();
            var SelectedCamera = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);
            var LightTypeList = SelectedCamera.LightsChannels.Select(light => light.Type.Value).ToList();
            foreach (var lightType in LightTypeList)
            {
                SelectItemsLightType.Add(lightType);
            }
            ParameterList.Add(new ComboBoxCommandParameter(ParamName_LightType, SelectItemsLightType));
            ParameterList.Add(new TextBoxCommandParameter(ParamName_LightValue, 0, true, 0, 255));
            AddParameterName();
        }

        public override bool DoCommand(ref bool IsStopScenario, ref bool IsScenarioRunFlag, ref string MsgBoxString)
        {
            try
            {
                if (!Checked)
                {
                    LoggerManager.CompVerifyLog($"[{CommandName}] Skip Command");
                    return true;
                }

                LoggerManager.CompVerifyLog($"[{CommandName}] Start Command");

                var ParamCameraType = ParameterList.Where(item => item.ParameterName == ParamName_CameraType).FirstOrDefault() as ComboBoxCommandParameter;
                var CameraType = (EnumProberCam)ParamCameraType.SelectItems.ElementAt(ParamCameraType.SelectedIndex);

                var ParamLightType = ParameterList.Where(item => item.ParameterName == ParamName_LightType).FirstOrDefault() as ComboBoxCommandParameter;
                var LigthType = (EnumLightType)ParamLightType.SelectItems.ElementAt(ParamLightType.SelectedIndex);

                var ParamLightValue = ParameterList.Where(item => item.ParameterName == ParamName_LightValue).FirstOrDefault() as TextBoxCommandParameter;
                var LightValue = (int)ParamLightValue.InputValue;

                LoggerManager.CompVerifyLog($"[{CommandName}] Set Light Value. (CameraType : {CameraType}, LigthType : {LigthType}, LightValue : {LightValue})");
                var SelectedCamera = this.VisionManager().GetCam(CameraType);
                SelectedCamera.SetLight(LigthType, (ushort)LightValue);

                LoggerManager.CompVerifyLog($"[{CommandName}] Finish Command");
            }
            catch (Exception e)
            {
                LoggerManager.Exception(e);
                return false;
            }

            return true;
        }

        public override VerificationCommandBase Create()
        {
            return new LightControlCommand();
        }

        public void CameraTypeSelectionChanged()
        {
            var ParamCameraType = ParameterList.Where(item => item.ParameterName == ParamName_CameraType).FirstOrDefault() as ComboBoxCommandParameter;
            var CameraType = (EnumProberCam)ParamCameraType.SelectItems.ElementAt(ParamCameraType.SelectedIndex);
            var SelectedCamera = this.VisionManager().GetCam(CameraType);
            var LightTypeList = SelectedCamera.LightsChannels.Select(light => light.Type.Value).ToList();

            SelectItemsLightType.Clear();
            foreach (var lightType in LightTypeList)
            {
                SelectItemsLightType.Add(lightType);
            }

            var ParamLightType = ParameterList.Where(item => item.ParameterName == ParamName_LightType).FirstOrDefault() as ComboBoxCommandParameter;
            ParamLightType.SelectedIndex = 0;
        }
    }

    public class DelayTimeCommand : VerificationCommandBase, IFactoryModule
    {
        private const string ParamName_DelayTime = "Time to Delay";

        public DelayTimeCommand()
        {
            CommandName = CommandName_DelayTime;

            ParameterList.Add(new TextBoxCommandParameter(ParamName_DelayTime, 1000, true, 1, int.MaxValue, "ms", true));
            AddParameterName();
        }

        public override bool DoCommand(ref bool IsStopScenario, ref bool IsScenarioRunFlag, ref string MsgBoxString)
        {
            try
            {
                if (!Checked)
                {
                    LoggerManager.CompVerifyLog($"[{CommandName}] Skip Command");
                    return true;
                }

                LoggerManager.CompVerifyLog($"[{CommandName}] Start Command");

                var ParamDelayTime = ParameterList.Where(item => item.ParameterName == ParamName_DelayTime).FirstOrDefault() as TextBoxCommandParameter;
                var DelayTime = (int)(ParamDelayTime?.InputValue ?? 0);

                LoggerManager.CompVerifyLog($"[{CommandName}] Wait to delay time. (Time : {DelayTime}ms)");
                Thread.Sleep(DelayTime);

                LoggerManager.CompVerifyLog($"[{CommandName}] Finish Command");
            }
            catch (Exception e)
            {
                LoggerManager.Exception(e);
                return false;
            }

            return true;
        }

        public override VerificationCommandBase Create()
        {
            return new DelayTimeCommand();
        }
    }

    public class DefaultPosMoveCommand : VerificationCommandBase, IFactoryModule
    {
        private const string ParamName_ZClearance = "Z Clearance";
        private const string ParamName_MovetoCenter = "Move to Center";
        private const string ParamName_WaferCamFold = "Wafer Cam Fold";

        public DefaultPosMoveCommand()
        {
            CommandName = CommandName_DefaultPosMove;

            ParameterList.Add(new BoolTypeCommandParameter(ParamName_ZClearance, true));
            ParameterList.Add(new BoolTypeCommandParameter(ParamName_MovetoCenter, true));
            ParameterList.Add(new BoolTypeCommandParameter(ParamName_WaferCamFold, true));
            AddParameterName();
        }

        public override bool DoCommand(ref bool IsStopScenario, ref bool IsScenarioRunFlag, ref string MsgBoxString)
        {
            try
            {
                if (!Checked)
                {
                    LoggerManager.CompVerifyLog($"[{CommandName}] Skip Command");
                    return true;
                }
                EventCodeEnum retCode = EventCodeEnum.UNDEFINED;

                LoggerManager.CompVerifyLog($"[{CommandName}] Start Command");

                var ZClearance = ParameterList.Where(item => item.ParameterName == ParamName_ZClearance).FirstOrDefault() as BoolTypeCommandParameter;
                bool IsZClearance = (bool)(ZClearance?.SelectItems.ElementAt(ZClearance.SelectedIndex) ?? false);
                var MovetoCenter = ParameterList?.Where(item => item.ParameterName == ParamName_MovetoCenter).FirstOrDefault() as BoolTypeCommandParameter;
                bool IsMovetoCenter = (bool)(MovetoCenter?.SelectItems.ElementAt(MovetoCenter.SelectedIndex) ?? false);
                var WaferCamFold = ParameterList.Where(item => item.ParameterName == ParamName_WaferCamFold).FirstOrDefault() as BoolTypeCommandParameter;
                bool IsWaferCamFold = (bool)(WaferCamFold?.SelectItems.ElementAt(WaferCamFold.SelectedIndex) ?? false);

                if (IsZClearance)
                {
                    LoggerManager.CompVerifyLog($"[{CommandName}] Z CLEARED");
                    retCode = this.StageSupervisor().StageModuleState.ZCLEARED();
                    if (EventCodeEnum.NONE != retCode)
                    {
                        MsgBoxString = $"[{CommandName}] Failed to Z CLEARED. (retCode : {retCode})";
                        goto FailScenario;
                    }
                }
                if (IsMovetoCenter)
                {
                    LoggerManager.CompVerifyLog($"[{CommandName}] Move to Center");
                    ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                    if (IsZClearance)
                    {
                        LoggerManager.CompVerifyLog($"[{CommandName}] Skip Move to Center - Z CLEARED.");
                    }
                    else
                    {
                        retCode = this.StageSupervisor().StageModuleState.ZCLEARED();
                        if (EventCodeEnum.NONE != retCode)
                        {
                            MsgBoxString = $"[{CommandName}] Failed to Move to Center - Z CLEARED. (retCode : {retCode})";
                            goto FailScenario;
                        }
                    }
                    retCode = this.MotionManager().StageMove(0, 0, zaxis.Param.ClearedPosition.Value, 0);
                    if (EventCodeEnum.NONE != retCode)
                    {
                        MsgBoxString = $"[{CommandName}] Failed to Move to Center - StageMove. (retCode : {retCode})";
                        goto FailScenario;
                    }
                }
                if (IsWaferCamFold)
                {
                    LoggerManager.CompVerifyLog($"[{CommandName}] Wafer Cam Fold");
                    if (StageCylinderType.MoveWaferCam.State == CylinderStateEnum.EXTEND)
                    {
                        var retVal = StageCylinderType.MoveWaferCam.Retract();
                        if (0 != retVal)
                        {
                            MsgBoxString = $"[{CommandName}] Failed to Retract WaferCamBridge. (retCode : {retVal})";
                            goto FailScenario;
                        }
                    }
                }

                LoggerManager.CompVerifyLog($"[{CommandName}] Finish Command");
            }
            catch (Exception e)
            {
                LoggerManager.Exception(e);
                return false;
            }

            return true;

        FailScenario:
            LoggerManager.CompVerifyLog(MsgBoxString);
            IsStopScenario = true;
            return false;
        }

        public override VerificationCommandBase Create()
        {
            return new DefaultPosMoveCommand();
        }
    }

    public class StageMoveCommand : VerificationCommandBase, IFactoryModule
    {
        private const string ParamName_StageMoveTo = "Move to";
        private enum EnumStageMoveTo
        {
            EnumMoveToLoadPos,
            EnumMoveToCenter,
            EnumMoveToMarkPos,
            EnumMoveToBack,
            EnumMoveToFront
        }
        private Dictionary<EnumStageMoveTo, string> StageMoveToDictionary = new Dictionary<EnumStageMoveTo, string>();
        private string StageMoveToSelected = "";

        public StageMoveCommand()
        {
            CommandName = CommandName_StageMove;

            StageMoveToDictionary.Add(EnumStageMoveTo.EnumMoveToLoadPos, "Load Pos");
            StageMoveToDictionary.Add(EnumStageMoveTo.EnumMoveToCenter, "Center");
            StageMoveToDictionary.Add(EnumStageMoveTo.EnumMoveToMarkPos, "Mark Pos");
            StageMoveToDictionary.Add(EnumStageMoveTo.EnumMoveToBack, "Back");
            StageMoveToDictionary.Add(EnumStageMoveTo.EnumMoveToFront, "Front");

            ObservableCollection<object> SelectItemsStageMoveTo = new ObservableCollection<object>();
            
            foreach (var moveTo in StageMoveToDictionary)
            {
                SelectItemsStageMoveTo.Add(moveTo.Value);
            }

            ParameterList.Add(new ComboBoxCommandParameter(ParamName_StageMoveTo, SelectItemsStageMoveTo));

            AddParameterName();
        }

        public override bool DoCommand(ref bool IsStopScenario, ref bool IsScenarioRunFlag, ref string MsgBoxString)
        {
            try
            {
                if (!Checked)
                {
                    LoggerManager.CompVerifyLog($"[{CommandName}] Skip Command");
                    return true;
                }

                LoggerManager.CompVerifyLog($"[{CommandName}] Start Command");

                var StageMoveToParam = ParameterList.Where(item => item.ParameterName == ParamName_StageMoveTo).FirstOrDefault() as ComboBoxCommandParameter;
                StageMoveToSelected = StageMoveToParam?.SelectItems.ElementAt(StageMoveToParam.SelectedIndex).ToString() ?? "";

                if (!StageMoveToDictionary.ContainsValue(StageMoveToSelected))
                {
                    MsgBoxString = $"[{CommandName}] Failed to Move to SelectedIndex({StageMoveToParam.SelectedIndex}).";
                    LoggerManager.CompVerifyLog(MsgBoxString);
                    IsStopScenario = true;
                    return false;
                }

                LoggerManager.CompVerifyLog($"[{CommandName}] Move to {StageMoveToSelected}");

                if (StageMoveToDictionary[EnumStageMoveTo.EnumMoveToLoadPos] == StageMoveToSelected)
                {
                    if (IsHasFailLogMsg(this.StageSupervisor().StageModuleState.MoveLoadingPosition(0.0), "", out MsgBoxString))
                    {
                        IsStopScenario = true;
                        return false;
                    }
                }
                else if (StageMoveToDictionary[EnumStageMoveTo.EnumMoveToCenter] == StageMoveToSelected)
                {
                    ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                    if (IsHasFailLogMsg(this.StageSupervisor().StageModuleState.ZCLEARED(), "- ZCLEARED", out MsgBoxString) ||
                        IsHasFailLogMsg(this.MotionManager().StageMove(0, 0, zaxis.Param.ClearedPosition.Value, 0), "- StageMove", out MsgBoxString))
                    {
                        IsStopScenario = true;
                        return false;
                    }
                }
                else if (StageMoveToDictionary[EnumStageMoveTo.EnumMoveToMarkPos] == StageMoveToSelected)
                {
                    if (IsHasFailLogMsg(this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM).SetLight(EnumLightType.AUX, 255), "- PIN_HIGH_CAM AUX", out MsgBoxString) ||
                        IsHasFailLogMsg(this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM).SetLight(EnumLightType.COAXIAL, 0), "- PIN_HIGH_CAM COAXIAL", out MsgBoxString) ||
                        IsHasFailLogMsg(this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM).SetLight(EnumLightType.COAXIAL, 0), "- WAFER_HIGH_CAM COAXIAL", out MsgBoxString) ||
                        IsHasFailLogMsg(this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM).SetLight(EnumLightType.OBLIQUE, 150), "- WAFER_HIGH_CAM OBLIQUE", out MsgBoxString) ||
                        IsHasFailLogMsg(this.StageSupervisor().StageModuleState.MoveToMark(), "- MoveToMark", out MsgBoxString))
                    {
                        IsStopScenario = true;
                        this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM).SetLight(EnumLightType.AUX, 0);
                        this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM).SetLight(EnumLightType.OBLIQUE, 0);
                        return false;
                    }
                }
                else if (StageMoveToDictionary[EnumStageMoveTo.EnumMoveToBack] == StageMoveToSelected || StageMoveToDictionary[EnumStageMoveTo.EnumMoveToFront] == StageMoveToSelected)
                {
                    ProbeAxisObject yaxis = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                    ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                    var yMove = (StageMoveToDictionary[EnumStageMoveTo.EnumMoveToBack] == StageMoveToSelected) ? yaxis.Param.PosSWLimit.Value - 1000 : yaxis.Param.NegSWLimit.Value + 1000;
                    if (IsHasFailLogMsg(this.StageSupervisor().StageModuleState.ZCLEARED(), "- ZCLEARED", out MsgBoxString) ||
                        IsHasFailLogMsg(this.MotionManager().StageMove(0, yMove, zaxis.Param.HomeOffset.Value), "- Move", out MsgBoxString))
                    {
                        IsStopScenario = true;
                        return false;
                    }
                }

                LoggerManager.CompVerifyLog($"[{CommandName}] Finish Command");
            }
            catch (Exception e)
            {
                LoggerManager.Exception(e);
                return false;
            }

            return true;
        }

        public override VerificationCommandBase Create()
        {
            return new StageMoveCommand();
        }

        private bool IsHasFailLogMsg(EventCodeEnum retCode, string LogToAdd, out string FailLogMsg)
        {
            if (EventCodeEnum.NONE == retCode) // 정상
            {
                FailLogMsg = "";
                return false;
            }

            FailLogMsg = $"[{CommandName}] Failed to Move to {StageMoveToSelected} {LogToAdd}. (retCode : {retCode})";
            LoggerManager.CompVerifyLog(FailLogMsg);
            
            return true;
        }
    }

    public class CommandParameterConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(CommandParameterBase);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                JObject jObject = JObject.Load(reader);
                if (jObject["ParameterType"].Value<string>() == CommandParameterBase.ParameterType_Bool)
                {
                    return jObject.ToObject<BoolTypeCommandParameter>(serializer);
                }
                else if (jObject["ParameterType"].Value<string>() == CommandParameterBase.ParameterType_ComboBox)
                {
                    var ComboBoxParam = jObject.ToObject<ComboBoxCommandParameter>(serializer);
                    if (jObject["ParameterName"].Value<string>() == AxisHomingCommand.ParamName_Axis)
                    {
                        ComboBoxParam.SelectItems.Clear();
                        ComboBoxParam.SelectItems.Add(EnumAxisConstants.X);
                        ComboBoxParam.SelectItems.Add(EnumAxisConstants.Y);
                        ComboBoxParam.SelectItems.Add(EnumAxisConstants.Z);
                        ComboBoxParam.SelectItems.Add(EnumAxisConstants.C);
                        ComboBoxParam.SelectItems.Add(EnumAxisConstants.TRI);
                        ComboBoxParam.SelectItems.Add(EnumAxisConstants.PZ);
                    }
                    else if (jObject["ParameterName"].Value<string>() == AxisMoveCommand.ParamName_MoveType)
                    {
                        ComboBoxParam.SelectItems.Clear();
                        ComboBoxParam.SelectItems.Add(EnumMoveType.Relative);
                        ComboBoxParam.SelectItems.Add(EnumMoveType.Absolute);
                    }
                    else if (jObject["ParameterName"].Value<string>() == MarkAlignProcWaferCamBridgeCommand.ParamName_BridgeMoveType)
                    {
                        ComboBoxParam.SelectItems.Clear();
                        ComboBoxParam.SelectItems.Add(CylinderStateEnum.EXTEND);
                        ComboBoxParam.SelectItems.Add(CylinderStateEnum.RETRACT);
                    }
                    else if (jObject["ParameterName"].Value<string>() == LightControlCommand.ParamName_CameraType)
                    {
                        ComboBoxParam.SelectItems.Clear();
                        ComboBoxParam.SelectItems.Add(EnumProberCam.WAFER_HIGH_CAM);
                        ComboBoxParam.SelectItems.Add(EnumProberCam.WAFER_LOW_CAM);
                        ComboBoxParam.SelectItems.Add(EnumProberCam.PIN_HIGH_CAM);
                        ComboBoxParam.SelectItems.Add(EnumProberCam.PIN_LOW_CAM);
                    }
                    else if (jObject["ParameterName"].Value<string>() == LightControlCommand.ParamName_LightType)
                    {
                        ComboBoxParam.SelectItems.Clear();
                        ComboBoxParam.SelectItems.Add(EnumLightType.COAXIAL);
                        ComboBoxParam.SelectItems.Add(EnumLightType.OBLIQUE);
                        ComboBoxParam.SelectItems.Add(EnumLightType.AUX);
                    }
                    else
                    {
                        // Nothing
                    }

                    return ComboBoxParam;
                }
                else if (jObject["ParameterType"].Value<string>() == CommandParameterBase.ParameterType_TextBox)
                {
                    var TextBoxParam = jObject.ToObject<TextBoxCommandParameter>(serializer);
                    if (jObject["ParameterName"].Value<string>() == VerificationCommandBase.ParamName_RepeatCount)
                    {
                        TextBoxParam.IsInteger = true;
                    }
                    return TextBoxParam;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return null;
            }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    abstract public class CommandParameterBase : INotifyPropertyChanged
    {
        public readonly static string ParameterType_Bool = "Bool";
        public readonly static string ParameterType_ComboBox = "ComboBox";
        public readonly static string ParameterType_AxisComboBox = "AxisComboBox";
        public readonly static string ParameterType_TextBox = "TextBox";

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public CommandParameterBase()
        {
            ParameterType = "";
            ParameterName = "";
            SelectItems = new ObservableCollection<object>();
            SelectedIndex = 0;
            InputValue = 0;
            VisibilityTextBox = Visibility.Visible;
            VisibilityComboBox = Visibility.Collapsed;
            UnitText = "";
            UserEditable = false;
        }

        public string ParameterType { get; set; }
        public string ParameterName { get; set; }
        public ObservableCollection<object> SelectItems { get; set; }
        
        public double InputValue { get; set; }
        public Visibility VisibilityTextBox { get; set; }
        public Visibility VisibilityComboBox { get; set; }
        public string UnitText { get; set; }

        public bool UserEditable { get; set; }

        private int _SelectedIndex;
        public int SelectedIndex 
        {
            get { return _SelectedIndex; }
            set
            {
                if (value != _SelectedIndex)
                {
                    _SelectedIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    public class BoolTypeCommandParameter : CommandParameterBase
    {
        public BoolTypeCommandParameter() : base()
        {
        }

        public BoolTypeCommandParameter(string paramName, bool defaultVal = true)
        {
            ParameterType = ParameterType_Bool;
            ParameterName = paramName;

            SelectItems = new ObservableCollection<object>();
            SelectItems.Add(true);
            SelectItems.Add(false);
            if (defaultVal)
            {
                SelectedIndex = 0;
            }
            else
            {
                SelectedIndex = 1;
            }
            VisibilityTextBox = Visibility.Collapsed;
            VisibilityComboBox = Visibility.Visible;
        }
    }

    public class ComboBoxCommandParameter : CommandParameterBase
    {
        public ComboBoxCommandParameter() : base()
        {
        }

        public ComboBoxCommandParameter(string paramName, ObservableCollection<object> selectItems)
        {
            ParameterType = ParameterType_ComboBox;
            ParameterName = paramName;
            SelectItems = selectItems;
            SelectedIndex = 0;
            VisibilityTextBox = Visibility.Collapsed;
            VisibilityComboBox = Visibility.Visible;
        }

        public void AddItem(string item)
        {
            SelectItems.Add(item);
        }
    }

    public class TextBoxCommandParameter : CommandParameterBase
    {
        public TextBoxCommandParameter() : base()
        {
        }

        public TextBoxCommandParameter(string paramName, double defaultValue = 0, bool rangeCheck = false, double minValue = 0, double maxValue = 0, string unitText = "", bool isInteger = false)
        {
            ParameterType = ParameterType_TextBox;
            ParameterName = paramName;
            InputValue = defaultValue;
            VisibilityTextBox = Visibility.Visible;
            VisibilityComboBox = Visibility.Collapsed;

            RangeCheck = rangeCheck;
            MinValue = minValue;
            MaxValue = maxValue;
            UnitText = unitText;
            IsInteger = isInteger;
        }

        public bool RangeCheck { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }

        private string _UnitText;
        public new string UnitText
        {
            get { return _UnitText; }
            set
            {
                _UnitText = value;
            }
        }

        private double _InputValue;
        public new double InputValue
        {
            get { return _InputValue; }
            set
            {
                if (value != _InputValue)
                {
                    if (!UserEditable || (UserEditable && DoTextBoxValidCheck(value))) 
                    {
                        // import 시에도 valid check를 하면 inputValue가 default로 변경되어서 파일 내 실제값을 알 수 없음 > IsValidCommand()에서 따로 확인
                        // 사용자가 변경 가능한 상태일 경우는 확인하여 set (Add 하였더거나 import 완료하여 Scenario에 정상 표시된 경우)
                        _InputValue = value;
                        RaisePropertyChanged();
                    }
                }
            }
        }

        public bool IsInteger { get; set; } = false;

        public bool DoTextBoxValidCheck(double value, bool ShowMsg = true)
        {
            if (!MinMaxRangeValidation(value, ShowMsg))
            {
                return false;
            }
            if (!IntegerValidation(value, ShowMsg))
            {
                return false;
            }
            return true;
        }

        private bool MinMaxRangeValidation(double value, bool ShowMsg)
        {
            if (RangeCheck)
            {
                if(value < MinValue || value > MaxValue)
                {
                    if (ShowMsg)
                        MessageBox.Show($"Wrong Input. (Range : {MinValue} ~ {MaxValue})");
                    return false;
                }
            }

            return true;
        }

        private bool IntegerValidation(double value, bool ShowMsg)
        {
            if (IsInteger && value % 1 != 0)
            {
                if (ShowMsg)
                    MessageBox.Show($"Wrong Input. Only integers are allowed.");
                return false;
            }

            return true;
        }
    }
    public class PolishWaferFocusingCommand : VerificationCommandBase, IFactoryModule
    {
        private const string ParamName_PWUsingTouchSensor = "Polish Wafer Using Touch Sensor";
        private const string ParamName_PWWaferIntervalParam = "Polish Wafer Interval Param";

        public ObservableCollection<object> SelectItemsPolishWaferParam { get; set; }

        public PolishWaferFocusingCommand()
        {
            CommandName = CommandName_PolishWaferFocusing;

            PolishWaferParameter PolishWaferParam = this.PolishWaferModule().GetPolishWaferIParam() as PolishWaferParameter;
            if (PolishWaferParam.PolishWaferIntervalParameters != null && (PolishWaferParam.PolishWaferIntervalParameters.Count > 0))
            {
                SelectItemsPolishWaferParam = new ObservableCollection<object>();
                foreach (var item in PolishWaferParam.PolishWaferIntervalParameters)
                {
                    SelectItemsPolishWaferParam.Add(item.CleaningParameters.First().WaferDefineType.Value);
                }
            }

            ParameterList.Add(new BoolTypeCommandParameter(ParamName_PWUsingTouchSensor, true));
            ParameterList.Add(new ComboBoxCommandParameter(ParamName_PWWaferIntervalParam, SelectItemsPolishWaferParam));
            ParameterList.Add(new TextBoxCommandParameter(ParamName_RepeatCount, 1));

            AddParameterName();
        }

        public override bool DoCommand(ref bool IsStopScenario, ref bool IsScenarioRunFlag, ref string MsgBoxString)
        {
            try
            {
                if (!Checked)
                {
                    LoggerManager.CompVerifyLog($"[{CommandName}] Skip Command");
                    return true;
                }

                LoggerManager.CompVerifyLog($"[{CommandName}] Start Command");

                var UsingTouchSensor = ParameterList.Where(item => item.ParameterName == ParamName_PWUsingTouchSensor).FirstOrDefault() as BoolTypeCommandParameter;
                bool IsUsingTouchSensor = (bool)UsingTouchSensor.SelectItems.ElementAt(UsingTouchSensor.SelectedIndex);

                var IntervalParam = ParameterList.Where(item => item.ParameterName == ParamName_PWWaferIntervalParam).FirstOrDefault() as ComboBoxCommandParameter;

                var ParamRepaetCount = ParameterList.Where(item => item.ParameterName == ParamName_RepeatCount).FirstOrDefault() as TextBoxCommandParameter;
                var TotalRepeatCount = (int)ParamRepaetCount.InputValue;
                int CurrentRepeatCount = 1;

                while (CurrentRepeatCount <= TotalRepeatCount)
                {
                    EventCodeEnum retval = EventCodeEnum.UNDEFINED;

                    //this.StageSupervisor().GetWaferStatus();
                    if ((this.StageSupervisor().GetWaferStatus() == EnumSubsStatus.EXIST))// && (this.StageSupervisor().GetWaferType() == EnumWaferType.POLISH))
                    {
                        PolishWaferParameter PolishWaferParam = this.PolishWaferModule().GetPolishWaferIParam() as PolishWaferParameter;
                        PolishWaferCleaningParameter cleaningparam = (PolishWaferCleaningParameter)PolishWaferParam.PolishWaferIntervalParameters[IntervalParam.SelectedIndex].CleaningParameters.First();

                        if (cleaningparam == null)
                        {
                            LoggerManager.CompVerifyLog($"[{CommandName}] curcleaningapram is null.");
                        }

                        if (IsUsingTouchSensor == true)
                        {
                            PolishWaferFocusingBySensor_Standard FocusingBySensorModule = new PolishWaferFocusingBySensor_Standard();
                            retval = FocusingBySensorModule.DoFocusing(cleaningparam, true);
                            LoggerManager.CompVerifyLog($"[FocusingBySensorModule] DoFucisng()");
                        }
                        else
                        {

                            PolishWaferFocusing_Standard FocusingModule = new PolishWaferFocusing_Standard();
                            retval = FocusingModule.DoFocusing(cleaningparam, true);
                            LoggerManager.CompVerifyLog($"[FocusingModule] DoFucisng()");
                        }
                    }

                    int retVal = StageCylinderType.MoveWaferCam.Retract();

                    if (retVal != 0)
                    {
                        LoggerManager.CompVerifyLog($"[{CommandName}] Failed to Retract Wafer Cam Bridge.");
                        return false;
                    }
                    

                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.CompVerifyLog($"[{CommandName}], Command Stop {retval.ToString()}");
                        break;
                    }

                    CurrentRepeatCount++;
                }

                LoggerManager.CompVerifyLog($"[{CommandName}] Finish Command");
            }
            catch (Exception e)
            {
                LoggerManager.Exception(e);
                return false;
            }

            return true;
        }

        public override VerificationCommandBase Create()
        {
            return new PolishWaferFocusingCommand();
        }
    }
    public class TouchSensorOffsetCommand : VerificationCommandBase, IFactoryModule
    {
        public TouchSensorOffsetCommand()
        {
            CommandName = CommandName_TouchSensorOffset;

            ParameterList.Add(new TextBoxCommandParameter(ParamName_RepeatCount, 1));

            AddParameterName();
        }

        public override bool DoCommand(ref bool IsStopScenario, ref bool IsScenarioRunFlag, ref string MsgBoxString)
        {
            try
            {
                if (!Checked)
                {
                    LoggerManager.CompVerifyLog($"[{CommandName}] Skip Command");
                    return true;
                }

                LoggerManager.CompVerifyLog($"[{CommandName}] Start Command");

                var ParamRepaetCount = ParameterList.Where(item => item.ParameterName == ParamName_RepeatCount).FirstOrDefault() as TextBoxCommandParameter;
                var TotalRepeatCount = (int)ParamRepaetCount.InputValue;
                int CurrentRepeatCount = 1;

                while (CurrentRepeatCount <= TotalRepeatCount)
                {
                    EventCodeEnum retval = EventCodeEnum.UNDEFINED;

                    retval = this.TouchSensorCalcOffsetModule().SetCalcOffsetCommand();

                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.CompVerifyLog($"[{CommandName}], Command Stop {retval.ToString()}");
                        break;
                    }

                    CurrentRepeatCount++;
                }

                int retVal = StageCylinderType.MoveWaferCam.Retract();
                if (retVal != 0)
                {
                    LoggerManager.CompVerifyLog($"[{CommandName}] Failed to Retract Wafer Cam Bridge.");
                    return false;
                }

                LoggerManager.CompVerifyLog($"[{CommandName}] Finish Command");
            }
            catch (Exception e)
            {
                LoggerManager.Exception(e);
                return false;
            }

            return true;
        }

        public override VerificationCommandBase Create()
        {
            return new TouchSensorOffsetCommand();
        }
    }
}
