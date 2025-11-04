using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HighMagAdvanceSetup.ViewModel
{
    using Focusing;
    using LogModule;
    using ProberInterfaces;
    using ProberInterfaces.Param;
    using ProberInterfaces.Vision;
    using ProberInterfaces.WaferAlignEX;
    using RelayCommandBase;
    using SerializerUtil;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Input;
    using VirtualKeyboardControl;
    using WA_HighMagParameter_Standard;
    public class HighMagStandardAdvanceSetupViewModel : IFactoryModule, INotifyPropertyChanged, IDataErrorInfo, IPnpAdvanceSetupViewModel
    {
        #region //=>RaisePropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region //..IDataErrorInfo
        public string Error { get { return string.Empty; } }
        private bool ParamValidationFlag = true;

        public string this[string columnName]
        {
            get
            {
                if (columnName == "PMAcceptance" && (this.PMAcceptance < 50 || this.PMAcceptance > 100))
                {
                    ParamValidationFlag = false;
                    return Properties.Resources.PMValueSettingLimitError;

                }
                else if (columnName == "PMAcceptance" && this.PMAcceptance > this.PMCertainty)
                {
                    ParamValidationFlag = false;
                    return Properties.Resources.PMAcceptanceLimitError;
                }
                else
                    ParamValidationFlag = true;

                if (ManualChecked)
                {
                    if (columnName == "Left1Index" & this.Left1Index == 0)
                    {
                        ParamValidationFlag = false;
                        return "Can not be 0.";
                    }
                    else if (columnName == "Left2Index" & this.Left2Index == 0)
                    {
                        ParamValidationFlag = false;
                        return "Can not be 0.";
                    }
                    else if (columnName == "Right1Index" & this.Right1Index == 0)
                    {
                        ParamValidationFlag = false;
                        return "Can not be 0.";
                    }
                    else if (columnName == "Right2Index" & this.Right2Index == 0)
                    {
                        ParamValidationFlag = false;
                        return "Can not be 0.";
                    }
                    else if (columnName == "UpperIndex" & this.UpperIndex == 0)
                    {
                        ParamValidationFlag = false;
                        return "Can not be 0.";
                    }
                    else if (columnName == "BottomIndex" & this.BottomIndex == 0)
                    {
                        ParamValidationFlag = false;
                        return "Can not be 0.";
                    }
                }


                if (columnName == "Left1Index" && this.Left1Index > this.Left2Index)
                {
                    ParamValidationFlag = false;
                    return "LEFT1 < LEFT2";
                }
                else
                    ParamValidationFlag = true;

                if (columnName == "Left2Index" && this.Left2Index < this.Left1Index && this.Left2Index > (this.StageSupervisor().WaferObject.GetPhysInfo().MapCountX.Value / 2))
                {
                    ParamValidationFlag = false;
                    return "LEFT1 < LEFT2";
                }
                else
                    ParamValidationFlag = true;

                if (columnName == "Right1Index" && this.Right1Index > this.Right2Index)
                {
                    ParamValidationFlag = false;
                    //return "RIGHT1 must be less than RIGHT2.";
                    return "RIGHT1 < RIGHT2";
                }
                else
                    ParamValidationFlag = true;

                if (columnName == "Right2Index" && this.Right2Index < this.Right1Index && this.Right2Index > (this.StageSupervisor().WaferObject.GetPhysInfo().MapCountX.Value / 2))
                {
                    ParamValidationFlag = false;
                    return "RIGHT1 < RIGHT2";
                }
                else
                    ParamValidationFlag = true;

                return null;
            }
        }

        #endregion

        #region //..Property


        private WA_HighStandard_JumpIndex_ManualParam _JumpIndexParam;
        public WA_HighStandard_JumpIndex_ManualParam JumpIndexParam
        {
            get { return _JumpIndexParam; }
            set
            {
                if (value != _JumpIndexParam)
                {
                    _JumpIndexParam = value;
                    RaisePropertyChanged("JumpIndexParam");
                }
            }
        }

        private PMParameter _PMParam;
        public PMParameter PMParam
        {
            get { return _PMParam; }
            set
            {
                if (value != _PMParam)
                {
                    _PMParam = value;
                    RaisePropertyChanged("PMParam");
                }
            }
        }

        private WA_HighStandard_FocusingROIWithPatternSizeParam _LowAcceptanceRetryPMParam;
        public WA_HighStandard_FocusingROIWithPatternSizeParam LowAcceptanceRetryPMParam
        {
            get { return _LowAcceptanceRetryPMParam; }
            set
            {
                if (value != _LowAcceptanceRetryPMParam)
                {
                    _LowAcceptanceRetryPMParam = value;
                    RaisePropertyChanged("LowAcceptanceRetryPMParam");
                }
            }
        }

        //..JumpIndexParam
        private long _Left1Index;
        public long Left1Index
        {
            get { return _Left1Index; }
            set
            {
                if (value != _Left1Index)
                {
                    _Left1Index = value;
                }
                RaisePropertyChanged("Left1Index");
            }
        }

        private long _Left2Index;
        public long Left2Index
        {
            get { return _Left2Index; }
            set
            {
                if (value != _Left2Index)
                {
                    _Left2Index = value;

                    long preleft1 = Left1Index;
                    Left1Index = 0;
                    Left1Index = preleft1;
                }
                RaisePropertyChanged("Left2Index");
            }
        }

        private long _Right1Index;
        public long Right1Index
        {
            get { return _Right1Index; }
            set
            {
                if (value != _Right1Index)
                {
                    _Right1Index = value;
                }
                RaisePropertyChanged("Right1Index");
            }
        }



        private long _Right2Index;
        public long Right2Index
        {
            get { return _Right2Index; }
            set
            {
                if (value != _Right2Index)
                {
                    _Right2Index = value;
                    long preright1 = Right1Index;
                    Right1Index = 0;
                    Right1Index = preright1;
                }
                RaisePropertyChanged("Right2Index");
            }
        }

        private long _UpperIndex;
        public long UpperIndex
        {
            get { return _UpperIndex; }
            set
            {
                if (value != _UpperIndex)
                {
                    _UpperIndex = value;
                }
                RaisePropertyChanged("UpperIndex");
            }
        }


        private long _BottomIndex;
        public long BottomIndex
        {
            get { return _BottomIndex; }
            set
            {
                if (value != _BottomIndex)
                {
                    _BottomIndex = value;
                }
                RaisePropertyChanged("BottomIndex");
            }
        }

        private double _RetryFocusingROIMargin_X;
        public double RetryFocusingROIMargin_X
        {
            get { return _RetryFocusingROIMargin_X; }
            set
            {
                if (value != _RetryFocusingROIMargin_X)
                {
                    _RetryFocusingROIMargin_X = value;
                    RaisePropertyChanged("RetryFocusingROIMargin_X");
                }
            }
        }

        private double _RetryFocusingROIMargin_Y;
        public double RetryFocusingROIMargin_Y
        {
            get { return _RetryFocusingROIMargin_Y; }
            set
            {
                if (value != _RetryFocusingROIMargin_Y)
                {
                    _RetryFocusingROIMargin_Y = value;
                    RaisePropertyChanged("RetryFocusingROIMargin_Y");
                }
            }
        }


        //..PMParam

        private int _PMAcceptance;
        public int PMAcceptance
        {
            get { return _PMAcceptance; }
            set
            {
                if (value != _PMAcceptance)
                {
                    _PMAcceptance = value;
                    RaisePropertyChanged("PMAcceptance");
                }
            }
        }

        private int _PMCertainty;
        public int PMCertainty
        {
            get { return _PMCertainty; }
            set
            {
                if (value != _PMCertainty)
                {
                    _PMCertainty = value;
                    RaisePropertyChanged("PMCertainty");
                }
            }
        }


        //..HeightParam
        private HeightPointEnum _HeightPoint;
        public HeightPointEnum HeightPoint
        {
            get { return _HeightPoint; }
            set
            {
                if (value != _HeightPoint)
                {
                    _HeightPoint = value;
                    RaisePropertyChanged("HeightPoint");
                }
            }
        }

        private High_ProcessingPointEnum _High_ProcessingPoint;
        public High_ProcessingPointEnum High_ProcessingPoint
        {
            get { return _High_ProcessingPoint; }
            set
            {
                if (value != _High_ProcessingPoint)
                {
                    _High_ProcessingPoint = value;
                    RaisePropertyChanged("High_ProcessingPoint");
                }
            }
        }

        private double _VerifyXLimit;
        public double VerifyXLimit
        {
            get { return _VerifyXLimit; }
            set
            {
                if (value != _VerifyXLimit)
                {
                    _VerifyXLimit = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _VerifyYLimit;
        public double VerifyYLimit
        {
            get { return _VerifyYLimit; }
            set
            {
                if (value != _VerifyYLimit)
                {
                    _VerifyYLimit = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _VerfiyAngleLimit;
        public double VerfiyAngleLimit
        {
            get { return _VerfiyAngleLimit; }
            set
            {
                if (value != _VerfiyAngleLimit)
                {
                    _VerfiyAngleLimit = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _VerifyCenterXLimit;
        public double VerifyCenterXLimit
        {
            get { return _VerifyCenterXLimit; }
            set
            {
                if (value != _VerifyCenterXLimit)
                {
                    _VerifyCenterXLimit = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _VerifyCenterYLimit;
        public double VerifyCenterYLimit
        {
            get { return _VerifyCenterYLimit; }
            set
            {
                if (value != _VerifyCenterYLimit)
                {
                    _VerifyCenterYLimit = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _JumpIndexSetupManualType;
        public bool JumpIndexSetupManualType
        {
            get { return _JumpIndexSetupManualType; }
            set
            {
                if (value != _JumpIndexSetupManualType)
                {
                    _JumpIndexSetupManualType = value;

                }
                if (_JumpIndexSetupManualType)
                {
                    try
                    {
                        ManualChecked = true;
                        AutoChecked = false;
                        EnableJumpIndexTextBox(true);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                        throw;
                    }
                }
                else
                {
                    ManualChecked = false;
                    AutoChecked = true;
                    EnableJumpIndexTextBox(false);
                }
                RaisePropertyChanged("JumpIndexSetupManualType");
                InitValue();
            }
        }

        private bool _ManualChecked;
        public bool ManualChecked
        {
            get { return _ManualChecked; }
            set
            {
                if (value != _ManualChecked)
                {
                    _ManualChecked = value;
                    if (_ManualChecked)
                        JumpIndexSetupManualType = true;
                    else
                        JumpIndexSetupManualType = false;
                    RaisePropertyChanged("ManualChecked");
                }
            }
        }

        private bool _AutoChecked;
        public bool AutoChecked
        {
            get { return _AutoChecked; }
            set
            {
                if (value != _AutoChecked)
                {
                    _AutoChecked = value;
                    if (_AutoChecked)
                        JumpIndexSetupManualType = false;
                    else
                        JumpIndexSetupManualType = true;
                    RaisePropertyChanged("AutoChecked");
                }
            }
        }


        private bool _TestBoxInEnabled;
        public bool TestBoxInEnabled
        {
            get { return _TestBoxInEnabled; }
            set
            {
                if (value != _TestBoxInEnabled)
                {
                    _TestBoxInEnabled = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<HeightPointEnum> _EnumHeightPoints
             = new ObservableCollection<HeightPointEnum>();
        public ObservableCollection<HeightPointEnum> EnumHeightPoints
        {
            get { return _EnumHeightPoints; }
            set
            {
                if (value != _EnumHeightPoints)
                {
                    _EnumHeightPoints = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<High_ProcessingPointEnum> _EnumProcessingPoints
             = new ObservableCollection<High_ProcessingPointEnum>();
        public ObservableCollection<High_ProcessingPointEnum> EnumProcessingPoints
        {
            get { return _EnumProcessingPoints; }
            set
            {
                if (value != _EnumProcessingPoints)
                {
                    _EnumProcessingPoints = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _EnablePostJumpindex;
        public bool EnablePostJumpindex
        {
            get { return _EnablePostJumpindex; }
            set
            {
                if (value != _EnablePostJumpindex)
                {
                    _EnablePostJumpindex = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _WaferPlanarityLimit;
        public double WaferPlanarityLimit
        {
            get { return _WaferPlanarityLimit; }
            set
            {
                if (value != _WaferPlanarityLimit)
                {
                    _WaferPlanarityLimit = value;
                    RaisePropertyChanged();
                }
            }
        }


        private FocusParameter _FocusParam = new NormalFocusParameter();
        public FocusParameter FocusParam
        {
            get { return _FocusParam; }
            set
            {
                if (value != _FocusParam)
                {
                    _FocusParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _hiddenTabVisibility = Visibility.Collapsed;

        public Visibility HiddenTabVisibility
        {
            get { return _hiddenTabVisibility; }
            set
            {
                if (_hiddenTabVisibility != value)
                {
                    _hiddenTabVisibility = value;
                    RaisePropertyChanged("HiddenTabVisibility");
                }
            }
        }

        private void InitValue()
        {
            Left1Index = 0;
            Left2Index = 0;
            Right1Index = 0;
            Right2Index = 0;
            UpperIndex = 0;
            BottomIndex = 0;
        }


        #endregion

        #region //..Command & Method
        private WA_HighMagParam_Standard HighStandardParam;
        public HighMagStandardAdvanceSetupViewModel()
        {
            try
            {

                foreach (HeightPointEnum hpoint in Enum.GetValues(typeof(HeightPointEnum)))
                {
                    if (hpoint != HeightPointEnum.UNDEFINED & hpoint != HeightPointEnum.INVALID)
                    {
                        //combo_HeightPoint.Items.Add(hpoint);
                        EnumHeightPoints.Add(hpoint);
                    }
                }

                foreach (High_ProcessingPointEnum processing_point in Enum.GetValues(typeof(High_ProcessingPointEnum)))
                {
                    EnumProcessingPoints.Add(processing_point);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public HighMagStandardAdvanceSetupViewModel(WA_HighMagParam_Standard highstandardparam)
        {
            try
            {
                foreach (HeightPointEnum hpoint in Enum.GetValues(typeof(HeightPointEnum)))
                {
                    if (hpoint != HeightPointEnum.UNDEFINED & hpoint != HeightPointEnum.INVALID)
                    {
                        //combo_HeightPoint.Items.Add(hpoint);
                        EnumHeightPoints.Add(hpoint);
                    }
                }
                foreach (High_ProcessingPointEnum processing_point in Enum.GetValues(typeof(High_ProcessingPointEnum)))
                {
                    EnumProcessingPoints.Add(processing_point);
                }

                HighStandardParam = highstandardparam;
                JumpIndexParam = new WA_HighStandard_JumpIndex_ManualParam();
                PMParam = new PMParameter();
                HighStandardParam.JumpIndexManualInputParam.CopyTo(JumpIndexParam);
                Left1Index = JumpIndexParam.Left1Index;
                Left2Index = JumpIndexParam.Left2Index;
                Right1Index = JumpIndexParam.Right1Index;
                Right2Index = JumpIndexParam.Right2Index;
                UpperIndex = JumpIndexParam.UpperIndex;
                BottomIndex = JumpIndexParam.BottomIndex;

                HighStandardParam.DefaultPMParam.CopyTo(PMParam);
                PMCertainty = PMParam.PMCertainty.Value;
                PMAcceptance = PMParam.PMAcceptance.Value;

                HeightPoint = HighStandardParam.HeightProfilingPointType.Value;
                High_ProcessingPoint = HighStandardParam.High_ProcessingPoint.Value;
                VerifyXLimit = HighStandardParam.LimitValueX.Value;
                VerifyYLimit = HighStandardParam.LimitValueY.Value;
                VerfiyAngleLimit = HighStandardParam.VerifyLimitAngle.Value;

                var verifyWaferXYLimit = this.WaferAligner().GetVerifyCenterLimitXYValue();
                VerifyCenterXLimit = verifyWaferXYLimit.Item1;
                VerifyCenterYLimit = verifyWaferXYLimit.Item2;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void SettingData(WA_HighMagParam_Standard highstandardparam)
        {
            try
            {
                this.HiddenTabVisibility = Visibility.Collapsed;

                HighStandardParam = highstandardparam;
                JumpIndexParam = new WA_HighStandard_JumpIndex_ManualParam();
                PMParam = new PMParameter();
                LowAcceptanceRetryPMParam = new WA_HighStandard_FocusingROIWithPatternSizeParam();
                HighStandardParam.JumpIndexManualInputParam.CopyTo(JumpIndexParam);
                HighStandardParam.FocusingROIWithPatternSize.CopyTo(LowAcceptanceRetryPMParam);

                if (ManualJumpIndexConfirm() != -1)
                {
                    JumpIndexSetupManualType = true;
                }
                else
                    JumpIndexSetupManualType = false;

                if (_JumpIndexSetupManualType)
                {
                    ManualChecked = true;
                    AutoChecked = false;
                    EnableJumpIndexTextBox(true);
                }
                else
                {
                    ManualChecked = false;
                    AutoChecked = true;
                    EnableJumpIndexTextBox(false);
                }

                Left1Index = JumpIndexParam.Left1Index;
                Left2Index = JumpIndexParam.Left2Index;
                Right1Index = JumpIndexParam.Right1Index;
                Right2Index = JumpIndexParam.Right2Index;
                UpperIndex = JumpIndexParam.UpperIndex;
                BottomIndex = JumpIndexParam.BottomIndex;

                RetryFocusingROIMargin_X = LowAcceptanceRetryPMParam.RetryFocusingROIMargin_X.Value;
                RetryFocusingROIMargin_Y = LowAcceptanceRetryPMParam.RetryFocusingROIMargin_Y.Value;

                HighStandardParam.DefaultPMParam.CopyTo(PMParam);
                PMCertainty = PMParam.PMCertainty.Value;
                PMAcceptance = PMParam.PMAcceptance.Value;

                HeightPoint = HighStandardParam.HeightProfilingPointType.Value;
                High_ProcessingPoint = HighStandardParam.High_ProcessingPoint.Value;
                VerifyXLimit = HighStandardParam.LimitValueX.Value;
                VerifyYLimit = HighStandardParam.LimitValueY.Value;
                VerfiyAngleLimit = HighStandardParam.VerifyLimitAngle.Value;

                var verifyWaferXYLimit = this.WaferAligner().GetVerifyCenterLimitXYValue();

                VerifyCenterXLimit = verifyWaferXYLimit.Item1;
                VerifyCenterYLimit = verifyWaferXYLimit.Item2;

                EnablePostJumpindex = HighStandardParam.EnablePostJumpindex;
                WaferPlanarityLimit = HighStandardParam.WaferPlanarityLimit.Value;

                FocusParameter focusParameter = new NormalFocusParameter();

                HighStandardParam.FocusParam.CopyTo(focusParameter);

                FocusParam = focusParameter;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private int ManualJumpIndexConfirm()
        {
            int retVal = -1;
            try
            {
                if (HighStandardParam.JumpIndexManualInputParam.Left1Index != 0 && HighStandardParam.JumpIndexManualInputParam.Left2Index != 0
                    && HighStandardParam.JumpIndexManualInputParam.Right1Index != 0 && HighStandardParam.JumpIndexManualInputParam.Right2Index != 0
                     & HighStandardParam.JumpIndexManualInputParam.UpperIndex != 0 && HighStandardParam.JumpIndexManualInputParam.BottomIndex != 0)
                {
                    retVal = 1;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private void EnableJumpIndexTextBox(bool flag)
        {
            try
            {
                TestBoxInEnabled = flag;
                if (!flag)
                {
                    UpperIndex = 0;
                    Left1Index = 0;
                    Left2Index = 0;
                    Right1Index = 0;
                    Right2Index = 0;
                    BottomIndex = 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private AsyncCommand _CmdOKClick;
        public ICommand CmdOKClick
        {
            get
            {
                if (null == _CmdOKClick) _CmdOKClick = new AsyncCommand(CmdOKClickFunc);
                return _CmdOKClick;
            }
        }
        private async Task CmdOKClickFunc()
        {
            try
            {
                if (!ParamValidationFlag)
                    return;

                HighStandardParam.JumpIndexManualInputParam.Left1Index = Left1Index;
                HighStandardParam.JumpIndexManualInputParam.Left2Index = Left2Index;
                HighStandardParam.JumpIndexManualInputParam.Right1Index = Right1Index;
                HighStandardParam.JumpIndexManualInputParam.Right2Index = Right2Index;
                HighStandardParam.JumpIndexManualInputParam.UpperIndex = UpperIndex;
                HighStandardParam.JumpIndexManualInputParam.BottomIndex = BottomIndex;

                HighStandardParam.DefaultPMParam.PMAcceptance.Value = PMAcceptance;
                HighStandardParam.DefaultPMParam.PMCertainty.Value = PMCertainty;


                HighStandardParam.LimitValueX.Value = VerifyXLimit;
                HighStandardParam.LimitValueY.Value = VerifyYLimit;
                HighStandardParam.VerifyLimitAngle.Value = VerfiyAngleLimit;

                this.WaferAligner().SetVerifyCenterLimitXYValue(VerifyCenterXLimit, VerifyCenterYLimit);

                HighStandardParam.EnablePostJumpindex = EnablePostJumpindex;
                HighStandardParam.WaferPlanarityLimit.Value = WaferPlanarityLimit;
                FocusParam.CopyTo(HighStandardParam.FocusParam);
                //await this.PnPManager().ClosePnpAdavanceSetupWindow().ConfigureAwait(false);

                if (HighStandardParam.HeightProfilingPointType.Value != HeightPoint)
                {
                    HighStandardParam.HeightProfilingPointType.Value = HeightPoint;

                    // #Hynix_Merge: 검토 필요, 이부분 
                    foreach (var pattern in HighStandardParam.Patterns.Value)
                    {
                        if (HeightPoint == HeightPointEnum.POINT1)
                        {
                            foreach (var item in pattern.JumpIndexs)
                            {
                                item.AcceptFocusing.Value = false;
                            }
                        }
                        else if (HeightPoint == HeightPointEnum.POINT5 || HeightPoint == HeightPointEnum.POINT9)
                        {
                            foreach (var item in pattern.JumpIndexs)
                            {
                                item.AcceptFocusing.Value = true;
                            }
                        }


                        if (HeightPoint == HeightPointEnum.POINT1 || HeightPoint == HeightPointEnum.POINT5)
                        {
                            HighStandardParam.HeightPosParams.Clear();
                            foreach (var item in pattern.PostJumpIndex)
                            {
                                if (pattern.PostJumpIndex.IndexOf(item) == 0)
                                {
                                    item.AcceptFocusing.Value = true;
                                }
                                else
                                {
                                    item.AcceptFocusing.Value = false;
                                }                                
                            }
                        }
                        else if(HeightPoint == HeightPointEnum.POINT9)
                        {
                            foreach (var item in pattern.PostJumpIndex)
                            {
                                item.AcceptFocusing.Value = true;
                            }
                        }

                    }
                    
                }

                HighStandardParam.High_ProcessingPoint.Value = High_ProcessingPoint;

                HighStandardParam.FocusingROIWithPatternSize.RetryFocusingROIMargin_X.Value = RetryFocusingROIMargin_X;
                HighStandardParam.FocusingROIWithPatternSize.RetryFocusingROIMargin_Y.Value = RetryFocusingROIMargin_Y;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                // #Hynix_Merge: 검토 필요, V20은 오른쪽 코드 였음. await this.PnPManager().ClosePnpAdavanceSetupWindow();
                await this.PnPManager().ClosePnpAdavanceSetupWindow().ConfigureAwait(false);
            }

        }


        private AsyncCommand _CmdCancelClick;
        public ICommand CmdCancelClick
        {
            get
            {
                if (null == _CmdCancelClick) _CmdCancelClick = new AsyncCommand(CmdCancelClickFunc);
                return _CmdCancelClick;
            }
        }
        private async Task CmdCancelClickFunc()
        {
            try
            {
                await this.PnPManager().ClosePnpAdavanceSetupWindow();

                HighStandardParam.JumpIndexManualInputParam.CopyTo(JumpIndexParam);
                Left1Index = JumpIndexParam.Left1Index;
                Left2Index = JumpIndexParam.Left2Index;
                Right1Index = JumpIndexParam.Right1Index;
                Right2Index = JumpIndexParam.Right2Index;
                UpperIndex = JumpIndexParam.UpperIndex;
                BottomIndex = JumpIndexParam.BottomIndex;

                HighStandardParam.DefaultPMParam.CopyTo(PMParam);
                PMAcceptance = PMParam.PMAcceptance.Value;
                PMCertainty = PMParam.PMCertainty.Value;

                HeightPoint = HighStandardParam.HeightProfilingPointType.Value;
                VerifyXLimit = HighStandardParam.LimitValueX.Value;
                VerifyYLimit = HighStandardParam.LimitValueY.Value;
                VerifyCenterXLimit = HighStandardParam.LimitValueX.Value;
                VerifyCenterYLimit = HighStandardParam.LimitValueY.Value;
                EnablePostJumpindex = HighStandardParam.EnablePostJumpindex;

                WaferPlanarityLimit = HighStandardParam.WaferPlanarityLimit.Value;

                HighStandardParam.FocusParam.CopyTo(FocusParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private RelayCommand<Object> _TextBoxClickCommand;
        public ICommand TextBoxClickCommand
        {
            get
            {
                if (null == _TextBoxClickCommand) _TextBoxClickCommand = new RelayCommand<Object>(TextBoxClickCommandFunc);
                return _TextBoxClickCommand;
            }
        }


        private void TextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 100);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private RelayCommand<Object> _DecimalTextBoxClickCommand;
        public ICommand DecimalTextBoxClickCommand
        {
            get
            {
                if (null == _DecimalTextBoxClickCommand) _DecimalTextBoxClickCommand = new RelayCommand<Object>(DecimalTextBoxClickCommandFunc);
                return _DecimalTextBoxClickCommand;
            }
        }


        private void DecimalTextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL | KB_TYPE.FLOAT, 0, 10);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        #endregion

        #region //.. IPnpAdvanceSetupViewModel Method

        public void SetParameters(List<byte[]> datas)
        {
            try
            {
                if (datas != null)
                {
                    foreach (var param in datas)
                    {
                        object target;
                        SerializeManager.DeserializeFromByte(param, out target, typeof(WA_HighMagParam_Standard));
                        if (target != null)
                        {
                            SettingData(target as WA_HighMagParam_Standard);
                            break;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public List<byte[]> GetParameters()
        {
            List<byte[]> parameters = new List<byte[]>();
            try
            {
                if (HighStandardParam != null)
                    parameters.Add(SerializeManager.SerializeToByte(HighStandardParam));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return parameters;
        }

        #endregion

        public void Init()
        {
            return;
        }
    }
}
