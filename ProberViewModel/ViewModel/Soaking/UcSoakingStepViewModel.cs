using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using RelayCommandBase;
using SoakingParameters;
using System.Windows.Media;

namespace ProberViewModel
{
    public class SoakingStep : INotifyPropertyChanged
    {
        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

        private string _StepIdx;
        public string StepIdx
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

        private string _SoakingTimeSec;
        public string SoakingTimeSec
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

        private bool _IsReadonlySoakingTimeSec;
        public bool IsReadonlySoakingTimeSec
        {
            get { return _IsReadonlySoakingTimeSec; }
            set
            {
                if (value != _IsReadonlySoakingTimeSec)
                {
                    _IsReadonlySoakingTimeSec = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Brush _SoakingTimeSecBackColor;
        public Brush SoakingTimeSecBackColor
        {
            get { return _SoakingTimeSecBackColor; }
            set
            {
                if (value != _SoakingTimeSecBackColor)
                {
                    _SoakingTimeSecBackColor = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Thickness _SoakingTimeSecBorderThickness = new Thickness();
        public Thickness SoakingTimeSecBorderThickness
        {
            get { return _SoakingTimeSecBorderThickness; }
            set
            {
                if (value != _SoakingTimeSecBorderThickness)
                {
                    _SoakingTimeSecBorderThickness = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _OD = 0;
        public double OD
        {
            get { return _OD; }
            set
            {
                if (value != _OD)
                {
                    _OD = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _IsSamplePinAlign;
        public Visibility IsSamplePinAlign
        {
            get { return _IsSamplePinAlign; }
            set
            {
                if (value != _IsSamplePinAlign)
                {
                    _IsSamplePinAlign = value;
                    RaisePropertyChanged();
                }
            }
        }

        public SoakingStep(string _StepIdx, string _SoakingTimeSec, double _OD, bool _IsReadonlySoakingTimeSec = false, bool _IsSamplePinAlign = true)
        {
            StepIdx = _StepIdx;
            SoakingTimeSec = _SoakingTimeSec;
            OD = _OD;
            IsReadonlySoakingTimeSec = _IsReadonlySoakingTimeSec;
            IsSamplePinAlign = _IsSamplePinAlign ? Visibility.Visible : Visibility.Hidden;

            if (!IsReadonlySoakingTimeSec)
            {
                SoakingTimeSec = _SoakingTimeSec;
                SoakingTimeSecBackColor = new SolidColorBrush(Colors.White);
                SoakingTimeSecBorderThickness = new Thickness(0, 0, 0, 1);
            }
            else
            {
                SoakingTimeSec = "Remain Time";
                SoakingTimeSecBackColor = new SolidColorBrush(Colors.Gray);
                SoakingTimeSecBorderThickness = new Thickness(0);
            }
        }
    }

    public class UcSoakingStepViewModel : UcStatusSoakingViewModelBase, IMainScreenViewModel
    {
        private readonly Guid _ViewModelGUID = new Guid("E4C8AA27-45F3-4DB1-8E2C-0D76833F2A60");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        private readonly Dispatcher dispatcher;
        private readonly string LastStep = "Last Step";
        public static string RemainTime { get; }= "Remain Time";

        public UcSoakingStepViewModel()
        {
            dispatcher = Dispatcher.CurrentDispatcher;
            InitTemplateViewModel();
        }

        //최초 표시 여부 
        private bool isFirstVisible = true;
        internal void VisibleChanged(bool value)
        {
            //처음 화면에 표시되는 상태면 0.1초후 진짜 데이터 표시하기
            if (value == true && isFirstVisible)
            {
                Task.Run(() =>
                {
                    System.Threading.Thread.Sleep(100);
                    isFirstVisible = false;
                    RaisePropertyChanged(nameof(SoakingStepsForBind));
                });
            }
        }

        //바인딩 전용 Property
        //VisibleChanged 를 이용하여 표시 지연을 하려는 목적의 바인딩 프로퍼티
        //Auto 속성의 화면 컨트롤 요소의 길이 정보가 정상적으로 지정되지 않는 경우 화면 표시 시점 1차 렌더링 완료 이후 다시 설정하는 방법으로 회피하려는 목적으로 준비됨. 
        // !!!! Auto 속성이 아닌경우 아래 SoakingSteps 을 바인딩 항목으로 지정해야함. 
        public ObservableCollection<SoakingStep> SoakingStepsForBind
        {
            get
            {
                if (!isFirstVisible)
                {
                    return _SoakingSteps;
                }
                else
                {
                    return null;
                }

            }
        }


        #region Property

        private ObservableCollection<SoakingStep> _SoakingSteps = new ObservableCollection<SoakingStep>();
        public ObservableCollection<SoakingStep> SoakingSteps
        {
            get { return _SoakingSteps; }
            set
            {
                if (value != _SoakingSteps)
                {
                    _SoakingSteps = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(SoakingStepsForBind));
                }
            }
        }

        private SoakingStep _SelectedSoakingStep = null;
        public SoakingStep SelectedSoakingStep
        {
            get { return _SelectedSoakingStep; }
            set
            {
                if (value != _SelectedSoakingStep)
                {
                    _SelectedSoakingStep = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _ShowLastFullPinAlign = Visibility.Visible;
        public Visibility ShowLastFullPinAlign
        {
            get
            {
                return _ShowLastFullPinAlign;
            }
            set
            {
                if (value != _ShowLastFullPinAlign)
                {
                    _ShowLastFullPinAlign = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _TooltipSoakingTimeMin;
        public string TooltipSoakingTimeMin
        {
            get { return _TooltipSoakingTimeMin; }
            set
            {
                if (value != _TooltipSoakingTimeMin)
                {
                    _TooltipSoakingTimeMin = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _TooltipODMax;
        public string TooltipODMax
        {
            get { return _TooltipODMax; }
            set
            {
                if (value != _TooltipODMax)
                {
                    _TooltipODMax = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ICommand
        private RelayCommand stepAddCommand;
        public ICommand StepAddCommand
        {
            get
            {
                if (null == stepAddCommand) stepAddCommand = new RelayCommand(StepAddCommandFunc);
                return stepAddCommand;
            }
        }

        public void StepAddCommandFunc()
        {
            Action AddSoakingStep = () =>
            {
                if (StatusSoakingParam == null)
                {
                    return;
                }

                // 마지막 Item에 Sample Pin Align 추가
                if (SoakingSteps.Count > 0)
                {
                    SoakingSteps[SoakingSteps.Count - 1].StepIdx = Convert.ToString(SoakingSteps.Count);
                    SoakingSteps[SoakingSteps.Count - 1].SoakingTimeSec = StatusSoakingParam.AdvancedSetting.SoakingStepTimeMin.Value.ToString();
                    SoakingSteps[SoakingSteps.Count - 1].IsReadonlySoakingTimeSec = false;
                    SoakingSteps[SoakingSteps.Count - 1].SoakingTimeSecBackColor = new SolidColorBrush(Colors.White);
                    SoakingSteps[SoakingSteps.Count - 1].SoakingTimeSecBorderThickness = new Thickness(0, 0, 0, 1);
                    SoakingSteps[SoakingSteps.Count - 1].IsSamplePinAlign = Visibility.Visible;
                }

                SoakingSteps.Add(new SoakingStep(LastStep, RemainTime, StatusSoakingParam.AdvancedSetting.ODLimit.Value, true, false));

                SelectedSoakingStep = SoakingSteps.ElementAt(SoakingSteps.Count - 1);
            };

            try
            {
                if (dispatcher.CheckAccess())
                {
                    AddSoakingStep();
                }
                else
                {
                    dispatcher.Invoke(() =>
                    {
                        AddSoakingStep();
                    });
                }
            }
            catch (Exception err)
            {
                LoggerManager.SoakingErrLog($"{err.Message}");
                throw;
            }
        }

        private RelayCommand stepDeleteCommand;
        public ICommand StepDeleteCommand
        {
            get
            {
                if (null == stepDeleteCommand) stepDeleteCommand = new RelayCommand(StepDeleteCommandFunc);
                return stepDeleteCommand;
            }
        }

        public void StepDeleteCommandFunc()
        {
            if ((SelectedSoakingStep == null) || (SoakingSteps.Count == 0))
            {
                return;
            }

            Action DeleteStep = () =>
            {
                var selectedIdx = SelectedSoakingStep.StepIdx == LastStep ? SoakingSteps.Count : int.Parse(SelectedSoakingStep.StepIdx);
                SoakingSteps.Remove(SelectedSoakingStep);

                // StepIdx ReNumbering
                int idx = 1;
                foreach (var item in SoakingSteps)
                {
                    item.StepIdx = Convert.ToString(idx);
                    idx++;
                }

                // 마지막 Item Pin Align Hidden
                if (SoakingSteps.Count > 0)
                {
                    SoakingSteps[SoakingSteps.Count - 1].StepIdx = LastStep;
                    SoakingSteps[SoakingSteps.Count - 1].SoakingTimeSec = RemainTime;
                    SoakingSteps[SoakingSteps.Count - 1].IsReadonlySoakingTimeSec = true;
                    SoakingSteps[SoakingSteps.Count - 1].SoakingTimeSecBackColor = new SolidColorBrush(Colors.Gray);
                    SoakingSteps[SoakingSteps.Count - 1].SoakingTimeSecBorderThickness = new Thickness(0);
                    SoakingSteps[SoakingSteps.Count - 1].IsSamplePinAlign = Visibility.Hidden;
                }

                if (SoakingSteps.Count == 0)
                {
                    // Do Nothing
                }
                else if (SoakingSteps.Count >= selectedIdx)
                {
                    SelectedSoakingStep = SoakingSteps.ElementAt(selectedIdx - 1);
                }
                else
                {
                    SelectedSoakingStep = SoakingSteps.ElementAt(selectedIdx - 2);
                }
            };

            try
            {
                if (dispatcher.CheckAccess())
                {
                    DeleteStep();
                }
                else
                {
                    dispatcher.Invoke(() =>
                    {
                        DeleteStep();
                    });
                }
            }
            catch (Exception err)
            {
                LoggerManager.SoakingErrLog($"{err.Message}");
            }
        }

        private RelayCommand stepDeleteAllCommand;
        public ICommand StepDeleteAllCommand
        {
            get
            {
                if (null == stepDeleteAllCommand) stepDeleteAllCommand = new RelayCommand(StepDeleteAllCommandFunc);
                return stepDeleteAllCommand;
            }
        }

        public void StepDeleteAllCommandFunc()
        {
            Action DeleteAllStep = () =>
            {
                SoakingSteps.Clear();
            };

            try
            {
                if (dispatcher.CheckAccess())
                {
                    DeleteAllStep();
                }
                else
                {
                    dispatcher.Invoke(() =>
                    {
                        DeleteAllStep();
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
        private SoakingSettingView.UcSoakingTemplateSoakingSetting templateEditView;
        private UcSoakingTemplateViewModel<Soakingsteptable> templateEditViewModel;
        private SoakingSettingView.UcSoakingTemplateSaveView templateSaveView;
        private UcSoakingTemplateViewModel<Soakingsteptable> templateSaveViewModel;


        private void InitTemplateViewModel()
        {
            templateSaveView = new SoakingSettingView.UcSoakingTemplateSaveView();
            templateSaveViewModel = new UcSoakingTemplateViewModel<Soakingsteptable>(templateSaveView, templateSaveView.GetType().Name);
            templateSaveViewModel.Title = "Soaking Step";
            templateSaveView.DataContext = templateSaveViewModel;

            templateEditView = new SoakingSettingView.UcSoakingTemplateSoakingSetting();
            templateEditViewModel = new UcSoakingTemplateViewModel<Soakingsteptable>(templateEditView, templateEditView.GetType().Name);
            templateEditViewModel.Title = "Soaking Step";
            templateEditView.DataContext = templateEditViewModel;

            templateEditViewModel.ApplyRequest += (s, arg) =>
            {
                if (arg is ObjectEventArgs<TemplateItem<Soakingsteptable>> soakArg)
                {
                    System.Diagnostics.Debug.WriteLine("Apply Soak : " + soakArg.Value.Name);
                    System.Diagnostics.Debug.WriteLine("Apply Soak : " + string.Join(",", soakArg.Value.Steps.Select(x => x.TimeSec)));

                    // 요청하기전에 사전검증 하도록 수정됨 2022-07-07 dwbae
                    var cloneStep = soakArg.Value.Steps.DeepClone();
                    VerifySoakingStepLimit(cloneStep, true);
                    return SetSoakStepTable(cloneStep);
                }
                return false;
            };

            templateEditViewModel.Validator = VerifySoakingStepLimit;
        }

        
        delegate bool SoakingsteptableInvoker(ObservableCollection<Soakingsteptable> newValue);
        private bool SetSoakStepTable(ObservableCollection<Soakingsteptable> arg)
        {
            if (!dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
            {
                return (bool)dispatcher.Invoke(new SoakingsteptableInvoker(SetSoakStepTable), arg);
                
            }

            SoakingSteps.Clear();
            foreach (var soakStep in arg)
            {
                if (soakStep == arg[arg.Count - 1])
                {
                    // Last Item
                    SoakingSteps.Add(new SoakingStep(LastStep,
                                                     Convert.ToString(soakStep.TimeSec.Value),
                                                     soakStep.OD_Value.Value,
                                                     true, false));
                }
                else
                {
                    SoakingSteps.Add(new SoakingStep(Convert.ToString(soakStep.StepIdx.Value),
                                                     Convert.ToString(soakStep.TimeSec.Value),
                                                     soakStep.OD_Value.Value));
                }
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

        /// <summary>
        /// 템플릿 저장 창 열기
        /// </summary>
        /// <returns></returns>
        public async Task TemplateSaveCommandFunc()
        {
            try
            {
                dispatcher.Invoke(() =>
                {
                    //현재 입력된 내용 준비.
                    ObservableCollection<Soakingsteptable> currentSteps = new ObservableCollection<Soakingsteptable>();
                    foreach (var soakingStep in SoakingSteps)
                    {
                        int stepIdx = soakingStep.StepIdx != LastStep ? Convert.ToInt32(soakingStep.StepIdx) : SoakingSteps.Count;
                        int soakingTime = soakingStep.SoakingTimeSec != RemainTime ? Convert.ToInt32(soakingStep.SoakingTimeSec) : int.MaxValue;

                        currentSteps.Add(new Soakingsteptable(stepIdx, soakingTime, soakingStep.OD));
                    }

                    //기존 내용 다시 읽기
                    templateSaveViewModel.Load();
                    
                    //새로운 임시 템플릿 설정.
                    templateSaveViewModel.NewTemplate = new TemplateItem<Soakingsteptable>()
                    {
                        Name = DateTime.Now.ToString(),
                        EditName = "", //이름이 없음.
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

        /// <summary>
        ///  저장된 템플릿 편집창 열기
        /// </summary>
        /// <returns></returns>
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

        #region UcStatusSoakingBase
        public override void SetStatusSoakingState(SoakingStateEnum state)
        {
            soakingState = state;

            if (soakingState == SoakingStateEnum.MAINTAIN)
            {
                ShowLastFullPinAlign = Visibility.Hidden;
            }
        }
        #endregion

        #region Method
        public override void TextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;

                if (tb.Name == "SoakingStepSoakingTime_tb")
                {
                    // Selected Item Change
                    SelectedSoakingStep = SoakingSteps.FirstOrDefault(x => x.SoakingTimeSec == tb.Text);
                }
                //else if (tb.Name == "SoakingStepSoakingOD_tb")
                //{
                //    // Selected Item Change
                //    SelectedSoakingStep = SoakingSteps.FirstOrDefault(x => x.OD.ToString() == tb.Text);
                //}

                base.TextBoxClickCommandFunc(param);
            }
            catch (Exception err)
            {
                LoggerManager.SoakingErrLog($"{err.Message}");
            }
        }

        public override EventCodeEnum ReadStatusSoakinConfigParameter(StatusSoakingConfig statusSoakingParam)
        {
            Action<ObservableCollection<Soakingsteptable>> AddSoakingStep = (ObservableCollection<Soakingsteptable> soakintStepTable) =>
            {
                var chuckAwayZ = statusSoakingParam.AdvancedSetting.ChuckAwayTolForChillingTime_Z.Value;
                var odMax = statusSoakingParam.AdvancedSetting.ODLimit.Value;
                var odMin = statusSoakingParam.MaintainStateConfig.NotExistWaferObj_OD.Value - chuckAwayZ;

                SoakingSteps.Clear();
                var newStep = new ObservableCollection<SoakingStep>();
                foreach (var soakStep in soakintStepTable)
                {
                    if(SoakingStateEnum.MANUAL == soakingState) //Manual soaking load시 범위 확인
                    {
                        if (soakStep.OD_Value.Value < odMin)
                            soakStep.OD_Value.Value = odMin;
                        else if (soakStep.OD_Value.Value > odMax)
                            soakStep.OD_Value.Value = odMax;
                    }

                    if (soakStep == soakintStepTable[soakintStepTable.Count - 1])
                    {
                        // Last Item
                        newStep.Add(new SoakingStep(LastStep,
                                                         Convert.ToString(soakStep.TimeSec.Value),
                                                         soakStep.OD_Value.Value,
                                                         true, false));
                    }
                    else
                    {
                        newStep.Add(new SoakingStep(Convert.ToString(soakStep.StepIdx.Value),
                                                         Convert.ToString(soakStep.TimeSec.Value),
                                                         soakStep.OD_Value.Value));
                    }
                }
                SoakingSteps = newStep;
            };

            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                if (statusSoakingParam == null)
                {
                    return ret;
                }

                UpdateTooltip();

                StatusSoakingCommonParam commonParam = null;
                switch (soakingState)
                {
                    case SoakingStateEnum.PREPARE:
                        commonParam = statusSoakingParam.PrepareStateConfig;
                        break;
                    case SoakingStateEnum.RECOVERY:
                        commonParam = statusSoakingParam.RecoveryStateConfig;
                        break;
                    case SoakingStateEnum.MAINTAIN:
                        commonParam = statusSoakingParam.MaintainStateConfig;
                        break;
                    case SoakingStateEnum.MANUAL:
                        commonParam = statusSoakingParam.ManualSoakingConfig;
                        break;
                }

                if (null != commonParam)
                {
                    if (dispatcher.CheckAccess())
                    {
                        AddSoakingStep(commonParam.SoakingStepTable);
                    }
                    else
                    {
                        dispatcher.Invoke(() =>
                        {
                            AddSoakingStep(commonParam.SoakingStepTable);
                        });
                    }

                    ret = EventCodeEnum.NONE;
                }
                else
                {
                    ret = EventCodeEnum.UNDEFINED;
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

                StatusSoakingCommonParam commonParam = null;
                switch (soakingState)
                {
                    case SoakingStateEnum.PREPARE:
                        commonParam = statusSoakingParam.PrepareStateConfig;
                        break;
                    case SoakingStateEnum.RECOVERY:
                        commonParam = statusSoakingParam.RecoveryStateConfig;
                        break;
                    case SoakingStateEnum.MAINTAIN:
                        commonParam = statusSoakingParam.MaintainStateConfig;
                        break;
                    case SoakingStateEnum.MANUAL:
                        commonParam = statusSoakingParam.ManualSoakingConfig;
                        break;
                }

                if (null != commonParam)
                {
                    commonParam.SoakingStepTable.Clear();
                    foreach (var soakingStep in SoakingSteps)
                    {
                        int stepIdx = soakingStep.StepIdx != LastStep ? Convert.ToInt32(soakingStep.StepIdx) : SoakingSteps.Count;
                        int soakingTime = soakingStep.SoakingTimeSec != RemainTime ? Convert.ToInt32(soakingStep.SoakingTimeSec) : int.MaxValue;

                        commonParam.SoakingStepTable.Add(new Soakingsteptable(stepIdx, soakingTime, soakingStep.OD));
                    }

                    ret = EventCodeEnum.NONE;
                }
                else
                {
                    ret = EventCodeEnum.UNDEFINED;
                }
            }
            catch (Exception err)
            {
                ret = EventCodeEnum.UNDEFINED;
                LoggerManager.SoakingErrLog($"{err.Message}");
            }

            return ret;
        }

        private bool VerifySoakingStepLimit(object soakingSteps, out string msg)
        {
            bool ret = false;
            msg = "---FAIL---";

            if(soakingSteps is ObservableCollection<Soakingsteptable> s)
            { 
                ret=  VerifySoakingStepLimit(s);
            }
            if(!ret)
            {
                var soakingStepTimeMin = StatusSoakingParam?.AdvancedSetting.SoakingStepTimeMin.Value;
                var odLimit = StatusSoakingParam?.AdvancedSetting.ODLimit.Value;

                msg = string.Format($"Some item values do not match the limit.\r\n" +
                                           $"Press \"Yes\" to change the value to the limit.\r\n\r\n" +
                                           $"Soaking Step Time(Min) : {soakingStepTimeMin ?? 60}\r\n" +
                                           $"OD / Z Clearance(Max) : {odLimit ?? 60}\r\n");
            }
            return ret;
        }

        private bool VerifySoakingStepLimit(ObservableCollection<Soakingsteptable> soakingSteps, bool bSetValue = false)
        {
            bool bVerify = true;
            if (soakingSteps == null)
            {
                return bVerify;
            }

            try
            {
                var soakingStepTimeMin = StatusSoakingParam?.AdvancedSetting.SoakingStepTimeMin.Value;
                var odLimit = StatusSoakingParam?.AdvancedSetting.ODLimit.Value;

                if (soakingStepTimeMin == null || odLimit == null)
                {
                    return bVerify;
                }

                foreach (var soakingStep in soakingSteps)
                {
                    // Soaking Time
                    if(soakingStep.TimeSec.Value== Int32.MaxValue)//마지막 예외대상 항목
                    {
                        //skip ;
                    }
                    else if (soakingStep.TimeSec.Value < soakingStepTimeMin)
                    {
                        bVerify = false;

                        if (bSetValue)
                        {
                            soakingStep.TimeSec.Value = (int)soakingStepTimeMin;
                        }
                    }

                    if (!bSetValue && !bVerify)
                    {
                        break;
                    }

                    // OD
                    if (soakingStep.OD_Value.Value > odLimit)
                    {
                        bVerify = false;

                        if (bSetValue)
                        {
                            soakingStep.OD_Value.Value = (double)odLimit;
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

        public (EventCodeEnum, string) VerifySoakingStepTable(string objectName)
        {
            EventCodeEnum ret = EventCodeEnum.NONE;
            string retMsg = string.Empty;

            if (SoakingSteps.Count == 0)
            {
                base.ReadStatusSoakingParameter();
            }

            if (SoakingSteps.Count == 0)
            {
                LoggerManager.SoakingLog($"[{objectName}] Soaking Step Table is Empty!");
                retMsg = $"[{objectName}][SoakingStep Table] Soaking Step Table is Empty!";
                ret = EventCodeEnum.NODATA;

                return (ret, retMsg);
            }

            return (ret, retMsg);
        }

        public void UpdateTooltip()
        {
            if (StatusSoakingParam == null)
            {
                return;
            }

            TooltipSoakingTimeMin = string.Format($"Minimum Value : {StatusSoakingParam.AdvancedSetting.SoakingStepTimeMin.Value}");

            var baseOD = (int)StatusSoakingParam.MaintainStateConfig.NotExistWaferObj_OD.Value;
            var chuckAwayZ = (int)StatusSoakingParam.AdvancedSetting.ChuckAwayTolForChillingTime_Z.Value;
            TooltipODMax = string.Format($"Range : {baseOD - chuckAwayZ} ~ {StatusSoakingParam.AdvancedSetting.ODLimit.Value}");
        }
        #endregion
    }
}
