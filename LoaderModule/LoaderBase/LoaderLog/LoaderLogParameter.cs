using System.Collections.Generic;

namespace LoaderBase.LoaderLog
{
    using ProberErrorCode;
    using ProberInterfaces;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Newtonsoft.Json;
    using System.IO;
    using LogModule;
    using System;
    using System.Text.RegularExpressions;

    public class LoaderLogParameter : INotifyPropertyChanged, ISystemParameterizable, IParam
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region //.. IParam
        public string FilePath { get; set; } = "";

        public string FileName { get; set; } = "LoaderLogParameter.Json";

        public bool IsParamChanged { get; set; }
        public string Genealogy { get; set; }
        public object Owner { get; set; }
        public List<object> Nodes { get; set; }

        [JsonIgnore]
        public readonly string spoolingBasePath = @"C:\ProberSystem\LoaderSystem\EMUL\Parameters\Resultmaps\Spooling";
        [JsonIgnore]
        public readonly int minSpoolingDelayTimeSec = 3;
        [JsonIgnore]
        public readonly int defaultSpoolingRetryCnt = 3;
        [JsonIgnore]
        public readonly int defaultSpoolingDelayTimeSec = 5;

        

        public EventCodeEnum Init()
        {
            try
            {
                string OpParameterPath = Path.Combine(this.FileManager().GetRootParamPath(), @"Parameters\Loader\LoaderLogParameter.json");


                if(StageLogParams == null)
                {
                    StageLogParams = new List<StageLogParameter>();
                }

                if (StageLogParams.Count == 0)
                {
                    string OriginStagePath_Debug = "";
                    string OriginStagePath_PMI = "";
                    string OriginStagePath_Pin = "";
                    string OriginStagePath_LOT = "";
                    string OriginStagePath_Temp = "";
                    string OriginStagePath_PMIImage = "";
                    string OriginStagePath_PinTipValidationImage = "";

                    for (int i = 0; i < SystemModuleCount.ModuleCnt.StageCount; i++)
                    {
                        // 셀 로그 서버에 업로드시 기존에는 Path 끝에 Cell번호로 폴더가 생성된 후 저장이 되었던 것을 호환시키기 위해
                        // 셀별 경로들 마지막에 Cell번호를 붙여 버전 업데이트 이후에도 기존 동작에 지장이 없도록 함.

                        if(StageSystemUpDownLoadPath == null || StageSystemUpDownLoadPath.Value == "" || StageSystemUpDownLoadPath.Value == null)
                        {
                            OriginStagePath_Debug = "";
                        }
                        else
                        {
                            if (StageSystemUpDownLoadPath.Value != null &&
                                StageSystemUpDownLoadPath.Value[StageSystemUpDownLoadPath.Value.Length - 1] == '/')
                            {
                                OriginStagePath_Debug = StageSystemUpDownLoadPath.Value + $"Cell{(i + 1).ToString().PadLeft(2, '0')}" + '/';
                            }
                            else
                            {
                                OriginStagePath_Debug = StageSystemUpDownLoadPath.Value + '/' + $"Cell{(i + 1).ToString().PadLeft(2, '0')}" + '/';
                            }
                        }

                        if (StageLOTUpDownLoadPath == null || StageLOTUpDownLoadPath.Value == "" || StageLOTUpDownLoadPath.Value == null)
                        {
                            OriginStagePath_LOT = "";
                        }
                        else
                        {
                            if (StageLOTUpDownLoadPath.Value != null &&
                                StageLOTUpDownLoadPath.Value[StageLOTUpDownLoadPath.Value.Length - 1] == '/')
                            {
                                OriginStagePath_LOT = StageLOTUpDownLoadPath.Value + $"Cell{(i + 1).ToString().PadLeft(2, '0')}" + '/';
                            }
                            else
                            {
                                OriginStagePath_LOT = StageLOTUpDownLoadPath.Value + '/' + $"Cell{(i + 1).ToString().PadLeft(2, '0')}" + '/';
                            }
                        }



                        if (StagePMIUpDownLoadPath == null || StagePMIUpDownLoadPath.Value == "" || StagePMIUpDownLoadPath.Value == null)
                        {
                            OriginStagePath_PMI = "";
                        }
                        else
                        {
                            if (StagePMIUpDownLoadPath.Value != null &&
                            StagePMIUpDownLoadPath.Value[StagePMIUpDownLoadPath.Value.Length - 1] == '/')
                            {
                                OriginStagePath_PMI = StagePMIUpDownLoadPath.Value + $"Cell{(i + 1).ToString().PadLeft(2, '0')}" + '/';
                            }
                            else
                            {
                                OriginStagePath_PMI = StagePMIUpDownLoadPath.Value + '/' + $"Cell{(i + 1).ToString().PadLeft(2, '0')}" + '/';
                            }
                        }

                        if (StageTempUpDownLoadPath == null || StageTempUpDownLoadPath.Value == "" || StageTempUpDownLoadPath.Value == null)
                        {
                            OriginStagePath_Temp = "";
                        }
                        else
                        {
                            if (StageTempUpDownLoadPath.Value != null &&
                            StageTempUpDownLoadPath.Value[StageTempUpDownLoadPath.Value.Length - 1] == '/')
                            {
                                OriginStagePath_Temp = StageTempUpDownLoadPath.Value + $"Cell{(i + 1).ToString().PadLeft(2, '0')}" + '/';
                            }
                            else
                            {
                                OriginStagePath_Temp = StageTempUpDownLoadPath.Value + '/' + $"Cell{(i + 1).ToString().PadLeft(2, '0')}" + '/';
                            }
                        }

                        if(StagePinUpDownLoadPath == null|| StagePinUpDownLoadPath.Value == "" || StagePinUpDownLoadPath.Value == null)
                        {
                            OriginStagePath_Pin = "";
                        }
                        else
                        {
                            if (StagePinUpDownLoadPath.Value != null &&
                           StagePinUpDownLoadPath.Value[StagePinUpDownLoadPath.Value.Length - 1] == '/')
                            {
                                OriginStagePath_Pin = StagePinUpDownLoadPath.Value + $"Cell{(i + 1).ToString().PadLeft(2, '0')}" + '/';
                            }
                            else
                            {
                                OriginStagePath_Pin = StagePinUpDownLoadPath.Value + '/' + $"Cell{(i + 1).ToString().PadLeft(2, '0')}" + '/';
                            }
                        }


                        if (StagePMIImageUploadPath == null || StagePMIImageUploadPath.Value == "" || StagePMIImageUploadPath.Value == null)
                        {
                            OriginStagePath_PMIImage = "";
                        }
                        else
                        {
                            if (StagePMIImageUploadPath.Value != null &&
                           StagePMIImageUploadPath.Value[StagePMIImageUploadPath.Value.Length - 1] == '/')
                            {
                                OriginStagePath_PMIImage = StagePMIImageUploadPath.Value + $"Cell{(i + 1).ToString().PadLeft(2, '0')}" + '/';
                            }
                            else
                            {
                                OriginStagePath_PMIImage = StagePMIImageUploadPath.Value + '/' + $"Cell{(i + 1).ToString().PadLeft(2, '0')}" + '/';
                            }
                        }

                        if (StagePinTipValidationResultUploadPath == null || StagePinTipValidationResultUploadPath.Value == "" || StagePinTipValidationResultUploadPath.Value == null)
                        {
                            OriginStagePath_PinTipValidationImage = "";
                        }
                        else
                        {
                            if (StagePinTipValidationResultUploadPath.Value != null &&
                            StagePinTipValidationResultUploadPath.Value[StagePinTipValidationResultUploadPath.Value.Length - 1] == '/')
                            {
                                OriginStagePath_PinTipValidationImage = StagePinTipValidationResultUploadPath.Value + $"Cell{(i + 1).ToString().PadLeft(2, '0')}" + '/';
                            }
                            else
                            {
                                OriginStagePath_PinTipValidationImage = StagePinTipValidationResultUploadPath.Value + '/' + $"Cell{(i + 1).ToString().PadLeft(2, '0')}" + '/';
                            }
                        }


                        StageLogParameter stageLogs = new StageLogParameter();
                        stageLogs.StageIndex.Value = i + 1;
                        stageLogs.StageSystemUpDownLoadPath.Value = OriginStagePath_Debug;
                        stageLogs.StageLOTUpDownLoadPath.Value = OriginStagePath_LOT;
                        stageLogs.StagePMIUpDownLoadPath.Value = OriginStagePath_PMI;
                        stageLogs.StageTempUpDownLoadPath.Value = OriginStagePath_Temp;
                        stageLogs.StagePinUpDownLoadPath.Value = OriginStagePath_Pin;
                        stageLogs.StagePMIImageUploadPath.Value = OriginStagePath_PMIImage;
                        stageLogs.StagePinTipValidationResultUploadPath.Value = OriginStagePath_PinTipValidationImage;
                        StageLogParams.Add(stageLogs);
                    }
                }
                this.SaveParameter(this, null, OpParameterPath);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetEmulParam()
        {
            SetDefaultParam();
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetDefaultParam()
        {
            UploadEnable.Value = false;
            UpdateTime.Value = 22;
            AutoLogUploadIntervalMinutes.Value = 0;
            UserName.Value = "winntdom\\semics";
            Password.Value = "aim2Serv";
            FTPUsePassive.Value = false;
            ServerPath.Value = "\\\\137.201.5.154\\semics";

            StageSystemUpDownLoadPath.Value = "\\\\137.201.5.154\\semics\\axcel\\SystemLog";
            StagePMIUpDownLoadPath.Value = "\\\\137.201.5.154\\semics\\axcel\\PMI";
            StageTempUpDownLoadPath.Value = "\\\\137.201.5.154\\semics\\axcel\\TEMP";
            StagePinUpDownLoadPath.Value = "\\\\137.201.5.154\\semics\\axcel\\PIN";

            LoaderSystemUpDownLoadPath.Value = "\\\\137.201.5.154\\semics\\axcel\\LoaderSystem";
            LoaderOCRUpDownLoadPath.Value = "\\\\137.201.5.154\\semics\\axcel\\LoaderOCR";

            DeviceDownLoadPath.Value = "\\\\137.201.5.154\\semics\\axcel\\DownLoadDeviceFile";
            DeviceUpLoadPath.Value = "\\\\137.201.5.154\\semics\\axcel\\UpLoadDevice File";

            ResultMapDownLoadPath.Value = "";
            ResultMapUpLoadPath.Value = "";
            ResultMapUploadRetryCount.Value = defaultSpoolingRetryCnt;
            SpoolingBasePath.Value = spoolingBasePath;
            ResultMapUploadDelayTime.Value = defaultSpoolingDelayTimeSec;
            ODTPUpLoadPath.Value = "";
            
            for (int i = 1; i <= SystemModuleCount.ModuleCnt.StageCount; i++)
            {
                StageLogParameter stageLogs = new StageLogParameter();
                stageLogs.StageIndex.Value = i;
                stageLogs.StageSystemUpDownLoadPath.Value = "ftp://127.0.0.1/FTP/STAGE_SYSTEM_UPDOWN";
                stageLogs.StageLOTUpDownLoadPath.Value = "ftp://127.0.0.1/FTP/STAGE_LOT_LOG";
                stageLogs.StagePMIUpDownLoadPath.Value = "ftp://127.0.0.1/FTP/STAGE_PMI_LOG";
                stageLogs.StageTempUpDownLoadPath.Value = "ftp://127.0.0.1/FTP/STAGE_TEMP_LOG";
                stageLogs.StagePinUpDownLoadPath.Value = "ftp://127.0.0.1/FTP/STAGE_PIN_LOG";
                stageLogs.StagePMIImageUploadPath.Value = "ftp://127.0.0.1/FTP/STAGE_PMI_IMAGE";
                stageLogs.StagePinTipValidationResultUploadPath.Value = "ftp://127.0.0.1/FTP/PINIMAGE";
                StageLogParams.Add(stageLogs);
            }

            CanUseStageLogParam.Value = false;

            return EventCodeEnum.NONE;
        }

        public void SetElementMetaData()
        {
            return;
        }

        #endregion

        
        private Element<bool> _UploadEnable = new Element<bool>();
        public Element<bool> UploadEnable
        {
            get { return _UploadEnable; }
            set
            {
                if (value != _UploadEnable)
                {
                    _UploadEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _UpdateTime = new Element<int>();
        public Element<int> UpdateTime
        {
            get { return _UpdateTime; }
            set
            {
                if (value != _UpdateTime)
                {
                    _UpdateTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _AutoLogUploadIntervalMinutes = new Element<int>();
        public Element<int> AutoLogUploadIntervalMinutes
        {
            get { return _AutoLogUploadIntervalMinutes; }
            set
            {
                if (value != _AutoLogUploadIntervalMinutes)
                {
                    _AutoLogUploadIntervalMinutes = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _UserName = new Element<string>();
        public Element<string> UserName
        {
            get { return _UserName; }
            set
            {
                if (value != _UserName)
                {
                    _UserName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _Password = new Element<string>();
        public Element<string> Password
        {
            get { return _Password; }
            set
            {
                if (value != _Password)
                {
                    _Password = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _FTPUsePassive = new Element<bool>();
        public Element<bool> FTPUsePassive
        {
            get { return _FTPUsePassive; }
            set
            {
                if (value != _FTPUsePassive)
                {
                    _FTPUsePassive = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _ServerPath = new Element<string>();
        public Element<string> ServerPath
        {
            get { return _ServerPath; }
            set
            {
                if (value != _ServerPath)
                {
                    _ServerPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _DeviceUpLoadPath = new Element<string>();
        public Element<string> DeviceUpLoadPath
        {
            get { return _DeviceUpLoadPath; }
            set
            {
                if (value != _DeviceUpLoadPath)
                {
                    _DeviceUpLoadPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _DeviceDownLoadPath = new Element<string>();
        public Element<string> DeviceDownLoadPath
        {
            get { return _DeviceDownLoadPath; }
            set
            {
                if (value != _DeviceDownLoadPath)
                {
                    _DeviceDownLoadPath = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<string> _ResultMapUpLoadPath = new Element<string>();
        public Element<string> ResultMapUpLoadPath
        {
            get { return _ResultMapUpLoadPath; }
            set
            {
                if (value != _ResultMapUpLoadPath)
                {
                    _ResultMapUpLoadPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _ResultMapDownLoadPath = new Element<string>();
        public Element<string> ResultMapDownLoadPath
        {
            get { return _ResultMapDownLoadPath; }
            set
            {
                if (value != _ResultMapDownLoadPath)
                {
                    _ResultMapDownLoadPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _ODTPUpLoadPath = new Element<string>();
        public Element<string> ODTPUpLoadPath
        {
            get { return _ODTPUpLoadPath; }
            set
            {
                if (value != _ODTPUpLoadPath)
                {
                    _ODTPUpLoadPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _ResultMapUploadRetryCount = new Element<int>();
        public Element<int> ResultMapUploadRetryCount
        {
            get { return _ResultMapUploadRetryCount; }
            set
            {
                if (value != _ResultMapUploadRetryCount)
                {
                    _ResultMapUploadRetryCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _ResultMapUploadDelayTime = new Element<int>();
        public Element<int> ResultMapUploadDelayTime
        {
            get { return _ResultMapUploadDelayTime; }
            set
            {
                if (value != _ResultMapUploadDelayTime)
                {
                    _ResultMapUploadDelayTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _SpoolingBasePath = new Element<string>();
        public Element<string> SpoolingBasePath
        {
            get { return _SpoolingBasePath; }
            set
            {
                if (value != _SpoolingBasePath)
                {
                    _SpoolingBasePath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _StageSystemUpDownLoadPath = new Element<string>();
        public Element<string> StageSystemUpDownLoadPath
        {
            get { return _StageSystemUpDownLoadPath; }
            set
            {
                if (value != _StageSystemUpDownLoadPath)
                {
                    _StageSystemUpDownLoadPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _LoaderSystemUpDownLoadPath = new Element<string>();
        public Element<string> LoaderSystemUpDownLoadPath
        {
            get { return _LoaderSystemUpDownLoadPath; }
            set
            {
                if (value != _LoaderSystemUpDownLoadPath)
                {
                    _LoaderSystemUpDownLoadPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _LoaderOCRUpDownLoadPath = new Element<string>();
        public Element<string> LoaderOCRUpDownLoadPath
        {
            get { return _LoaderOCRUpDownLoadPath; }
            set
            {
                if (value != _LoaderOCRUpDownLoadPath)
                {
                    _LoaderOCRUpDownLoadPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _StagePinUpDownLoadPath = new Element<string>();
        public Element<string> StagePinUpDownLoadPath
        {
            get { return _StagePinUpDownLoadPath; }
            set
            {
                if (value != _StagePinUpDownLoadPath)
                {
                    _StagePinUpDownLoadPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _StageTempUpDownLoadPath = new Element<string>();
        public Element<string> StageTempUpDownLoadPath
        {
            get { return _StageTempUpDownLoadPath; }
            set
            {
                if (value != _StageTempUpDownLoadPath)
                {
                    _StageTempUpDownLoadPath = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<string> _StagePMIUpDownLoadPath = new Element<string>();
        public Element<string> StagePMIUpDownLoadPath
        {
            get { return _StagePMIUpDownLoadPath; }
            set
            {
                if (value != _StagePMIUpDownLoadPath)
                {
                    _StagePMIUpDownLoadPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _StageLOTUpDownLoadPath = new Element<string>();
        public Element<string> StageLOTUpDownLoadPath
        {
            get { return _StageLOTUpDownLoadPath; }
            set
            {
                if (value != _StageLOTUpDownLoadPath)
                {
                    _StageLOTUpDownLoadPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _StagePMIImageUploadPath = new Element<string>();
        public Element<string> StagePMIImageUploadPath
        {
            get { return _StagePMIImageUploadPath; }
            set
            {
                if (value != _StagePMIImageUploadPath)
                {
                    _StagePMIImageUploadPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _StagePinTipValidationResultUploadPath = new Element<string>();
        public Element<string> StagePinTipValidationResultUploadPath
        {
            get { return _StagePinTipValidationResultUploadPath; }
            set
            {
                if (value != _StagePinTipValidationResultUploadPath)
                {
                    _StagePinTipValidationResultUploadPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<StageLogParameter> _StageLogParams = new List<StageLogParameter>();
        public List<StageLogParameter> StageLogParams
        {
            get { return _StageLogParams; }
            set
            {
                if (value != _StageLogParams)
                {
                    _StageLogParams = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _CanUseStageLogParam = new Element<bool>();
        public Element<bool> CanUseStageLogParam
        {
            get { return _CanUseStageLogParam; }
            set
            {
                if (value != _CanUseStageLogParam)
                {
                    _CanUseStageLogParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string GetConnectPath()
        {
            string connectPath = string.Empty;

            foreach (var prop in this.GetType().GetProperties())
            {
                if (!prop.Name.Contains("Path") || prop.Name.Equals("ServerPath"))
                {
                    continue;
                }

                string value = prop.GetValue(this, null).ToString();

                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                string ftpPattern = @"^ftp://|FTP:// ";
                string networkPattern = @"^\\(\\(\w+\s*)+)";

                if (Regex.IsMatch(value, ftpPattern))
                {
                    var u = new Uri(value);
                    connectPath = u.AbsoluteUri.Replace(u.AbsolutePath, "");
                }
                else if (Regex.IsMatch(value, networkPattern))
                {
                    var u = new Uri(value);
                    connectPath = u.AbsoluteUri.Replace(u.AbsolutePath, "");
                    connectPath = connectPath.Replace("file:", "/");
                }
                else
                {
                    // Not support protocol
                }

                break;
            }

            return connectPath;
        }

        private void removeServerPathForLoaderLogParam()
        {
            try
            {
                foreach (var prop in this.GetType().GetProperties())
                {
                    if (!prop.Name.Contains("Path") || prop.Name.Equals("ServerPath"))
                    {
                        continue;
                    }

                    string value = prop.GetValue(this, null).ToString();

                    if (string.IsNullOrEmpty(value))
                    {
                        continue;
                    }

                    string ftpPattern = @"^ftp://|FTP:// ";
                    string networkPattern = @"^\\(\\(\w+\s*)+)";

                    if (Regex.IsMatch(value, ftpPattern))
                    {
                        var u = new Uri(value);
                        string path = u.AbsolutePath;
                        IElement elem = prop.GetValue(this, null) as IElement;
                        elem.SetValue(path);
                    }
                    else if (Regex.IsMatch(value, networkPattern))
                    {
                        var u = new Uri(value);
                        string path = u.AbsolutePath;
                        IElement elem = prop.GetValue(this, null) as IElement;
                        elem.SetValue(path);
                    }
                    else
                    {
                        // Not support protocol
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class StageLogParameter : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
        private Element<int> _StageIndex = new Element<int>();
        public Element<int> StageIndex
        {
            get { return _StageIndex; }
            set
            {
                if (value != _StageIndex)
                {
                    _StageIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _StageSystemUpDownLoadPath = new Element<string>();
        public Element<string> StageSystemUpDownLoadPath
        {
            get { return _StageSystemUpDownLoadPath; }
            set
            {
                if (value != _StageSystemUpDownLoadPath)
                {
                    _StageSystemUpDownLoadPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _StagePinUpDownLoadPath = new Element<string>();
        public Element<string> StagePinUpDownLoadPath
        {
            get { return _StagePinUpDownLoadPath; }
            set
            {
                if (value != _StagePinUpDownLoadPath)
                {
                    _StagePinUpDownLoadPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _StageTempUpDownLoadPath = new Element<string>();
        public Element<string> StageTempUpDownLoadPath
        {
            get { return _StageTempUpDownLoadPath; }
            set
            {
                if (value != _StageTempUpDownLoadPath)
                {
                    _StageTempUpDownLoadPath = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<string> _StagePMIUpDownLoadPath = new Element<string>();
        public Element<string> StagePMIUpDownLoadPath
        {
            get { return _StagePMIUpDownLoadPath; }
            set
            {
                if (value != _StagePMIUpDownLoadPath)
                {
                    _StagePMIUpDownLoadPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _StageLOTUpDownLoadPath = new Element<string>();
        public Element<string> StageLOTUpDownLoadPath
        {
            get { return _StageLOTUpDownLoadPath; }
            set
            {
                if (value != _StageLOTUpDownLoadPath)
                {
                    _StageLOTUpDownLoadPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _StagePMIImageUploadPath = new Element<string>();
        public Element<string> StagePMIImageUploadPath
        {
            get { return _StagePMIImageUploadPath; }
            set
            {
                if (value != _StagePMIImageUploadPath)
                {
                    _StagePMIImageUploadPath = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<string> _StagePinTipValidationResultUploadPath = new Element<string>();
        public Element<string> StagePinTipValidationResultUploadPath
        {
            get { return _StagePinTipValidationResultUploadPath; }
            set
            {
                if (value != _StagePinTipValidationResultUploadPath)
                {
                    _StagePinTipValidationResultUploadPath = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
