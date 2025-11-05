using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using RelayCommandBase;
using SoakingParameters;
using VirtualKeyboardControl;

namespace ProberViewModel
{
    public class UcSoakingChillingTimeTableViewModel : UcStatusSoakingViewModelBase, IMainScreenViewModel
    {
        private readonly Guid _ViewModelGUID = new Guid("5AE3462B-E2A5-44C2-BD97-FDFA19B77703");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        private readonly Dispatcher dispatcher;

        public UcSoakingChillingTimeTableViewModel()
        {
            dispatcher = Dispatcher.CurrentDispatcher;
            InitTemplateViewModel();
        }

        #region Property
        private int _StepIdx = 0;
        public int StepIdx
        {
            get { return _StepIdx; }
            set
            {
                if (value != _StepIdx)
                {
                    _StepIdx = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _ChillingTimeSec = 0;
        public int ChillingTimeSec
        {
            get { return _ChillingTimeSec; }
            set
            {
                if (value != _ChillingTimeSec)
                {
                    _ChillingTimeSec = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _SoakingTimeSec = 0;
        public int SoakingTimeSec
        {
            get { return _SoakingTimeSec; }
            set
            {
                if (value != _SoakingTimeSec)
                {
                    _SoakingTimeSec = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<Chillingtimetable> _ChillingTimeTable = new ObservableCollection<Chillingtimetable>();
        public ObservableCollection<Chillingtimetable> ChillingTimeTable
        {
            get { return _ChillingTimeTable; }
            set
            {
                if (value != _ChillingTimeTable)
                {
                    _ChillingTimeTable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Chillingtimetable _SelectedChillingTime = null;
        public Chillingtimetable SelectedChillingTime
        {
            get { return _SelectedChillingTime; }
            set
            {
                if (value != _SelectedChillingTime)
                {
                    _SelectedChillingTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _TooltipChillingTime;
        public string TooltipChillingTime
        {
            get { return _TooltipChillingTime; }
            set
            {
                if (value != _TooltipChillingTime)
                {
                    _TooltipChillingTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _TooltipChillingSoakingTime;
        public string TooltipChillingSoakingTime
        {
            get { return _TooltipChillingSoakingTime; }
            set
            {
                if (value != _TooltipChillingSoakingTime)
                {
                    _TooltipChillingSoakingTime = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region Command
        private RelayCommand _ChillingTimeTableAddCommand;
        public ICommand ChillingTimeTableAddCommand
        {
            get
            {
                if (null == _ChillingTimeTableAddCommand) _ChillingTimeTableAddCommand = new RelayCommand(ChillingTimeTableAddCommandFunc);
                return _ChillingTimeTableAddCommand;
            }
        }
        public void ChillingTimeTableAddCommandFunc()
        {
            Action AddChillTimeTable = () =>
            {
                var addChillingTimeTable = new Chillingtimetable(ChillingTimeTable.Count + 1, 0, 0);

                if (ChillingTimeTable.Count == 0)
                {
                    if (StatusSoakingParam == null)
                    {
                        return;
                    }

                    addChillingTimeTable.ChillingTimeSec.Value = StatusSoakingParam.AdvancedSetting.ChillingTimeMin.Value;
                    addChillingTimeTable.SoakingTimeSec.Value = StatusSoakingParam.AdvancedSetting.ChillingSoakingTimeMin.Value;
                }
                else
                {
                    var prevChillintTimeTable = ChillingTimeTable.ElementAt(ChillingTimeTable.Count - 1);

                    addChillingTimeTable.ChillingTimeSec.Value = prevChillintTimeTable.ChillingTimeSec.Value + 1;
                    addChillingTimeTable.SoakingTimeSec.Value = prevChillintTimeTable.SoakingTimeSec.Value + 1;
                }

                ChillingTimeTable.Add(addChillingTimeTable);

                SelectedChillingTime = ChillingTimeTable.ElementAt(ChillingTimeTable.Count - 1);
            };

            try
            {
                if (dispatcher.CheckAccess())
                {
                    AddChillTimeTable();
                }
                else
                {
                    dispatcher.Invoke(() =>
                    {
                        AddChillTimeTable();
                    });
                }
            }
            catch (Exception err)
            {
                LoggerManager.SoakingErrLog($"{err.Message}");
            }
        }

        private RelayCommand _ChillingTimeTableDeleteCommand;
        public ICommand ChillingTimeTableDeleteCommand
        {
            get
            {
                if (null == _ChillingTimeTableDeleteCommand) _ChillingTimeTableDeleteCommand = new RelayCommand(ChillingTimeTableDeleteCommandFunc);
                return _ChillingTimeTableDeleteCommand;
            }
        }
        public void ChillingTimeTableDeleteCommandFunc()
        {
            if ((SelectedChillingTime == null) || (ChillingTimeTable.Count == 0))
            {
                return;
            }

            Action DeleteChillingTime = () =>
            {
                var selectedIdx = SelectedChillingTime.StepIdx.Value;
                ChillingTimeTable.Remove(SelectedChillingTime);

                // StepIdx ReNumbering
                int idx = 1;
                foreach (var item in ChillingTimeTable)
                {
                    item.StepIdx.Value = idx;
                    idx++;
                }

                if (ChillingTimeTable.Count == 0)
                {
                    // Do Nothing
                }
                else if (ChillingTimeTable.Count >= selectedIdx)
                {
                    SelectedChillingTime = ChillingTimeTable.ElementAt(selectedIdx - 1);
                }
                else
                {
                    SelectedChillingTime = ChillingTimeTable.ElementAt(selectedIdx - 2);
                }
            };

            try
            {
                if (dispatcher.CheckAccess())
                {
                    DeleteChillingTime();
                }
                else
                {
                    dispatcher.Invoke(() =>
                    {
                        DeleteChillingTime();
                    });
                }
            }
            catch (Exception err)
            {
                LoggerManager.SoakingErrLog($"{err.Message}");
            }
        }

        private RelayCommand _ChillingTimeTableDeleteAllCommand;
        public ICommand ChillingTimeTableDeleteAllCommand
        {
            get
            {
                if (null == _ChillingTimeTableDeleteAllCommand) _ChillingTimeTableDeleteAllCommand = new RelayCommand(ChillingTimeTableDeleteAllCommandFunc);
                return _ChillingTimeTableDeleteAllCommand;
            }
        }
        public void ChillingTimeTableDeleteAllCommandFunc()
        {
            Action DeleteAllChillingTime = () =>
            {
                ChillingTimeTable.Clear();
            };

            try
            {
                if (dispatcher.CheckAccess())
                {
                    DeleteAllChillingTime();
                }
                else
                {
                    dispatcher.Invoke(() =>
                    {
                        DeleteAllChillingTime();
                    });
                }
            }
            catch (Exception err)
            {
                LoggerManager.SoakingErrLog($"{err.Message}");
            }
        }
        #endregion

        #region Template Edit Command

        private SoakingSettingView.UcSoakingTemplateChillingSetting templateEditView;
        private UcSoakingTemplateViewModel<Chillingtimetable> templateEditViewModel;
        private SoakingSettingView.UcSoakingTemplateSaveView templateSaveView;
        private UcSoakingTemplateViewModel<Chillingtimetable> templateSaveViewModel;

        private void InitTemplateViewModel()
        {
            templateSaveView = new SoakingSettingView.UcSoakingTemplateSaveView();
            templateSaveViewModel = new UcSoakingTemplateViewModel<Chillingtimetable>(templateSaveView, templateSaveView.GetType().Name);
            templateSaveViewModel.Title = "Chilling Time Table";
            templateSaveView.DataContext = templateSaveViewModel;

            templateEditView = new SoakingSettingView.UcSoakingTemplateChillingSetting();
            templateEditViewModel = new UcSoakingTemplateViewModel<Chillingtimetable>(templateEditView, templateEditView.GetType().Name);
            templateEditViewModel.Title = "Chilling Time Table";
            templateEditView.DataContext = templateEditViewModel;

            templateEditViewModel.ApplyRequest += (s, arg) =>
            {
                if (arg is ObjectEventArgs<TemplateItem<Chillingtimetable>> soakArg)
                {
                    System.Diagnostics.Debug.WriteLine("Apply Soak : " + soakArg.Value.Name);
                    System.Diagnostics.Debug.WriteLine("Apply Soak : " + string.Join(",", soakArg.Value.Steps.Select(x => x.ChillingTimeSec)));

                    // 요청하기전에 사전검증 하도록 수정됨 2022-07-07 dwbae
                    var cloneStep = soakArg.Value.Steps.DeepClone();
                    VerifyChillingTimeLimit(cloneStep, true);
                    return SetChillingStepTable(cloneStep);
                    //if (!VerifyChillingTimeLimit(soakArg.Value.Steps))
                    //{
                    //    var chillingTimeMin = StatusSoakingParam?.AdvancedSetting.ChillingTimeMin.Value;
                    //    var chillingSoakingTimeMin = StatusSoakingParam?.AdvancedSetting.ChillingSoakingTimeMin.Value;

                    //    string msg = string.Format($"Some item values do not match the limit.\r\n" +
                    //                               $"Press \"Yes\" to change the value to the limit.\r\n\r\n" +
                    //                               $"Chilling Time(Min) : {chillingTimeMin}\r\n" +
                    //                               $"Soaking Time(Min) : {chillingSoakingTimeMin}\r\n");

                    //    if (MessageBoxResult.Yes == MessageBox.Show(msg, "Warning",
                    //                                                MessageBoxButton.YesNo,
                    //                                                MessageBoxImage.Warning,
                    //                                                MessageBoxResult.Yes,
                    //                                                MessageBoxOptions.DefaultDesktopOnly))
                    //    {                            
                    //        var cloneStep = soakArg.Value.Steps.DeepClone();
                    //        VerifyChillingTimeLimit(cloneStep, true);
                    //        SetChillingStepTable(cloneStep);
                    //    }
                    //}
                }
                return false;
            };

            templateEditViewModel.Validator = VerifyChillingTimeLimit;
        }

        delegate bool ChillingsteptableInvoker(ObservableCollection<Chillingtimetable> newValue);
        private bool SetChillingStepTable(ObservableCollection<Chillingtimetable> arg)
        {
            if (!dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
            {
                return (bool)dispatcher.Invoke(new ChillingsteptableInvoker(SetChillingStepTable), arg);
            }
            ChillingTimeTable.Clear();
            foreach (var step in arg)
            {
                ChillingTimeTable.Add(new Chillingtimetable(step.StepIdx.Value,
                    step.ChillingTimeSec.Value,
                    step.SoakingTimeSec.Value));
            }
            return true;
        }

        private AsyncCommand templateSaveCommand;
        public ICommand TemplateSaveCommand
        {
            get
            {
                if (null == templateSaveCommand) templateSaveCommand = new AsyncCommand(TemplateSaveCommandFunc);
                return templateSaveCommand;
            }
        }

        public async Task TemplateSaveCommandFunc()
        {
            try
            {
                dispatcher.Invoke(() =>
                {
                    ObservableCollection<Chillingtimetable> currentSteps = new ObservableCollection<Chillingtimetable>();
                    foreach (var chillingStep in ChillingTimeTable)
                    {

                        currentSteps.Add(new Chillingtimetable(chillingStep.StepIdx.Value, chillingStep.ChillingTimeSec.Value, chillingStep.SoakingTimeSec.Value));
                    }

                    templateSaveViewModel.Load();
                    templateSaveViewModel.NewTemplate = new TemplateItem<Chillingtimetable>()
                    {
                        Name = DateTime.Now.ToString(),
                        EditName = "",
                        Time = DateTime.Now,
                        Steps = currentSteps,
                        EditSteps = currentSteps,
                        ReferenceSteps = currentSteps
                    };
                    // 이전 이름 유지, 한번더 갱신해서 PropertyChanged 이벤트 활성화 유도. 
                    templateSaveViewModel.NewTemplateName = templateSaveViewModel.NewTemplateName;
                });

                await this.MetroDialogManager().ShowWindow(templateSaveView);

            }
            catch (Exception err)
            {
                LoggerManager.SoakingLog($"{err.Message}");
            }
        }

        private AsyncCommand templateEditCommand;
        public ICommand TemplateEditCommand
        {
            get
            {
                if (null == templateEditCommand) templateEditCommand = new AsyncCommand(TemplateEditCommandFunc);
                return templateEditCommand;
            }
        }
        public async Task TemplateEditCommandFunc()
        {
            try
            {
                dispatcher.Invoke(() =>
                {
                    templateEditViewModel.Load();
                });

                await this.MetroDialogManager().ShowWindow(templateEditView);
            }
            catch (Exception err)
            {
                LoggerManager.SoakingLog($"{err.Message}");
            }
        }
        #endregion

        #region Method
        

        public override void TextBoxClickCommandFunc(Object param)
        {            
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;

                if (tb.Name == "ChillingTableChillingTime_tb")
                {
                    // Selected Item Change
                    SelectedChillingTime = ChillingTimeTable.FirstOrDefault(x => x.ChillingTimeSec.Value.ToString() == tb.Text);

                    int oldValue = int.Parse(tb.Text);
                    if (oldValue < StatusSoakingParam.AdvancedSetting.ChillingTimeMin.Value)
                    {
                        oldValue = StatusSoakingParam.AdvancedSetting.ChillingTimeMin.Value;
                    }

                    tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 5);
                    
                    // 입력된 값이 최소값보다 작으면 OldValue로 리턴
                    if (int.Parse(tb.Text) < StatusSoakingParam.AdvancedSetting.ChillingTimeMin.Value)
                    {
                        tb.Text = oldValue.ToString();
                    }
                    else
                    {
                        tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();

                        // Sorting
                        ChillingTimeTable = new ObservableCollection<Chillingtimetable>(ChillingTimeTable.OrderBy(x => x.ChillingTimeSec.Value));

                        // 번호 재정렬
                        int index = 1;
                        foreach (var chillingTimeTable in ChillingTimeTable)
                        {
                            chillingTimeTable.StepIdx.Value = index;
                            index++;
                        }
                    }

                    // Selected Item Change
                    SelectedChillingTime = null;
                    SelectedChillingTime = ChillingTimeTable.FirstOrDefault(x => x.ChillingTimeSec.Value.ToString() == tb.Text);
                }
                else if (tb.Name == "ChillingTableSoakingTimeSec_tb")
                {
                    // Selected Item Change
                    SelectedChillingTime = ChillingTimeTable.FirstOrDefault(x => x.SoakingTimeSec.Value.ToString() == tb.Text);
                    base.TextBoxClickCommandFunc(param);
                }
            }
            catch (Exception err)
            {
                LoggerManager.SoakingErrLog($"{err.Message}");
            }
        }

        public override EventCodeEnum ReadStatusSoakinConfigParameter(StatusSoakingConfig statusSoakingParam)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                if (statusSoakingParam == null)
                {
                    return ret;
                }

                UpdateTooltip();

                Recoverystate recoverystateParam = null;
                recoverystateParam = statusSoakingParam.RecoveryStateConfig;

                if (null != recoverystateParam)
                {
                    ChillingTimeTable = recoverystateParam.SoakingChillingTimeTable;
                    ret = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.SoakingLog($"Failed to 'ByteToObject'.");
                }
            }
            catch (Exception err)
            {
                ret = EventCodeEnum.UNDEFINED;
                LoggerManager.SoakingErrLog($"{err.Message}");
            }
            return ret;
        }

        public override EventCodeEnum SaveStatusSoakingConfigParameter(ref StatusSoakingConfig statusSoakingParam)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (statusSoakingParam == null)
                {
                    return ret;
                }

                UpdateTooltip();

                statusSoakingParam.RecoveryStateConfig.SoakingChillingTimeTable = ChillingTimeTable;
            }
            catch (Exception err)
            {
                ret = EventCodeEnum.UNDEFINED;
                LoggerManager.SoakingErrLog($"{err.Message}");
            }
            return ret;
        }
        private bool VerifyChillingTimeLimit(object soakingSteps, out string msg)
        {
            bool ret = false;
            msg = "---FAIL---";

            if (soakingSteps is ObservableCollection<Chillingtimetable> s)
            {
                ret = VerifyChillingTimeLimit(s);
            }
            if (!ret)
            {
                var chillingTimeMin = StatusSoakingParam?.AdvancedSetting.ChillingTimeMin.Value;
                var chillingSoakingTimeMin = StatusSoakingParam?.AdvancedSetting.ChillingSoakingTimeMin.Value;

                msg = string.Format($"Some item values do not match the limit.\r\n" +
                                           $"Press \"Yes\" to change the value to the limit.\r\n\r\n" +
                                           $"Chilling Time(Min) : {chillingTimeMin}\r\n" +
                                           $"Soaking Time(Min) : {chillingSoakingTimeMin}\r\n");
            }
            return ret;
        }
        private bool VerifyChillingTimeLimit(ObservableCollection<Chillingtimetable> chillingTimeTables, bool bSetValue = false)
        {
            bool bVerify = true;
            if (chillingTimeTables == null)
            {
                return bVerify;
            }

            try
            {
                var chillingTimeMin = StatusSoakingParam?.AdvancedSetting.ChillingTimeMin.Value;
                var chillingSoakingTimeMin = StatusSoakingParam?.AdvancedSetting.ChillingSoakingTimeMin.Value;

                if (chillingTimeMin == null || chillingSoakingTimeMin == null)
                {
                    return bVerify;
                }

                foreach (var chillingTime in chillingTimeTables)
                {
                    // Chilling Time
                    if (chillingTime.ChillingTimeSec.Value < chillingTimeMin)
                    {
                        bVerify = false;

                        if (bSetValue)
                        {
                            chillingTime.ChillingTimeSec.Value = (int)chillingTimeMin;
                        }
                    }

                    if (!bSetValue && !bVerify)
                    {
                        break;
                    }

                    // Soaking Time
                    if (chillingTime.SoakingTimeSec.Value < chillingSoakingTimeMin)
                    {
                        bVerify = false;

                        if (bSetValue)
                        {
                            chillingTime.SoakingTimeSec.Value = (int)chillingSoakingTimeMin;
                        }
                    }

                    if (!bSetValue && !bVerify)
                    {
                        break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.SoakingErrLog($"{err.Message}");
            }

            return bVerify;
        }
        
        public (EventCodeEnum, string) VerifyChillingTimeTable(string objectName)
        {
            EventCodeEnum ret = EventCodeEnum.NONE;
            string retMsg = string.Empty;

            if (ChillingTimeTable.Count == 0)
            {
                base.ReadStatusSoakingParameter();
            }

            if (ChillingTimeTable.Count == 0)
            {
                LoggerManager.SoakingLog($"[{objectName}] Chilling Time Table is Empty!");
                retMsg = $"[{objectName}][Chilling Table] Chilling Time Table is Empty!";
                ret = EventCodeEnum.NODATA;

                return (ret, retMsg);
            }

            // Chilling Time Table의 첫 번째 항목은 Limit 값 보다 커야 한다.
            var chillingTimeMinValue = StatusSoakingParam.AdvancedSetting.ChillingTimeMin.Value;
            var chillingSoakingTimeMinValue = StatusSoakingParam.AdvancedSetting.ChillingSoakingTimeMin.Value;

            var item = ChillingTimeTable[0];
            if ((item.ChillingTimeSec.Value < chillingTimeMinValue) || (item.SoakingTimeSec.Value < chillingSoakingTimeMinValue))
            {
                LoggerManager.SoakingLog($"[{objectName}] Invalid Parameter Setting. The value is less than limit. [ChillingTime] {item.ChillingTimeSec} Limit ({chillingTimeMinValue}), [SoakingTime] {item.SoakingTimeSec} Limit ({chillingSoakingTimeMinValue})");
                retMsg = $"[{objectName}][Chilling Table] Invalid Parameter Setting. The value is less than limit. [ChillingTime] {item.ChillingTimeSec} Limit ({chillingTimeMinValue}), [SoakingTime] {item.SoakingTimeSec} Limit ({chillingSoakingTimeMinValue})";
                ret = EventCodeEnum.SOAKING_ERROR_PARAMETER_INVALID;

                return (ret, retMsg);
            }

            int prevRow = 0;
            int currRow = 1;
            if (ret == EventCodeEnum.NONE)
            {
                for (; currRow < ChillingTimeTable.Count; prevRow++, currRow++)
                {
                    var prevItem = ChillingTimeTable[prevRow];
                    var currItem = ChillingTimeTable[currRow];

                    if ((prevItem.ChillingTimeSec.Value >= currItem.ChillingTimeSec.Value) || (prevItem.SoakingTimeSec.Value >= currItem.SoakingTimeSec.Value))
                    {
                        // 이전 Row의 Chilling Time이 현재 Row의 Chilltime 보다 큰 값을 가질 수 없다.
                        LoggerManager.SoakingLog($"[{objectName}] Invalid Parameter Setting. The next value is less than the previous value.");
                        retMsg = $"[{objectName}][Chilling Table] Invalid Parameter Setting. The next value is less than the previous value.";
                        ret = EventCodeEnum.SOAKING_ERROR_PARAMETER_INVALID;
                        
                        break;
                    }
                }
            }

            return (ret, retMsg);
        }

        public void UpdateTooltip()
        {
            if (StatusSoakingParam == null)
            {
                return;
            }

            TooltipChillingTime = string.Format($"Minimum Value : {StatusSoakingParam.AdvancedSetting.ChillingTimeMin.Value}");
            TooltipChillingSoakingTime = string.Format($"Minimum Value : {StatusSoakingParam.AdvancedSetting.ChillingSoakingTimeMin.Value}");
        }
        #endregion
    }
}