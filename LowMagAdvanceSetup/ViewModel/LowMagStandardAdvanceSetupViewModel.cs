using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LowMagAdvanceSetup.ViewModel
{
    using LogModule;
    using ProberInterfaces;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using WA_LowMagParameter_Standard;
    using RelayCommandBase;
    using System.Windows.Input;
    using VirtualKeyboardControl;
    using ThetaAlignStandardModule;
    using SerializerUtil;
    using ProberInterfaces.Param;
    using Focusing;
    using ProberInterfaces.WaferAlignEX;
    using System.Collections.ObjectModel;

    public class LowMagStandardAdvanceSetupViewModel : IFactoryModule, INotifyPropertyChanged, IDataErrorInfo, IPnpAdvanceSetupViewModel
    {
        #region //=> RaisePropertyChanged
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
                    if (columnName == "LeftJumpIndex" & this.LeftJumpIndex == 0)
                    {
                        ParamValidationFlag = false;
                        return "Can not be 0.";
                    }
                    if (columnName == "RightJumpIndex" & this.RightJumpIndex == 0)
                    {
                        ParamValidationFlag = false;
                        return "Can not be 0.";
                    }
                }

                if (columnName == "LeftJumpIndex" | columnName == "RightJumpIndex")
                {
                    if (this.LeftJumpIndex + this.RightJumpIndex < 0)
                    {
                        ParamValidationFlag = false;
                        return "";
                    }
                }

                if (columnName == "LeftJumpIndex" && this.LeftJumpIndex < 0)
                {
                    ParamValidationFlag = false;
                    return Properties.Resources.JumpIndexMinusError;
                }
                else if (columnName == "LeftJumpIndex" && this.LeftJumpIndex > (this.StageSupervisor().WaferObject.GetPhysInfo().MapCountX.Value / 2))
                {
                    ParamValidationFlag = false;
                    return Properties.Resources.JumpIndexLimitSettingError;
                }
                else
                    ParamValidationFlag = true;

                if (columnName == "RightJumpIndex" && this.RightJumpIndex < 0)
                {
                    ParamValidationFlag = false;
                    return Properties.Resources.JumpIndexMinusError;
                }
                else if (columnName == "RightJumpIndex" && this.RightJumpIndex > (this.StageSupervisor().WaferObject.GetPhysInfo().MapCountX.Value / 2))
                {
                    ParamValidationFlag = false;
                    return Properties.Resources.JumpIndexLimitSettingError;
                }
                else
                    ParamValidationFlag = true;


                return "";
            }
        }
        #endregion

        #region //..Property

        private IWaferObject _WaferObject;

        public IWaferObject WaferObject
        {
            get { return this.StageSupervisor().WaferObject; }
            set { _WaferObject = value; }
        }

        private Low_ProcessingPointEnum _Low_ProcessingPoint;
        public Low_ProcessingPointEnum Low_ProcessingPoint
        {
            get { return _Low_ProcessingPoint; }
            set
            {
                if (value != _Low_ProcessingPoint)
                {
                    _Low_ProcessingPoint = value;
                    RaisePropertyChanged("Low_ProcessingPoint");
                }
            }
        }

        private long _LeftJumpIndex;
        public long LeftJumpIndex
        {
            get { return _LeftJumpIndex; }
            set
            {
                if (value != _LeftJumpIndex)
                {
                    _LeftJumpIndex = value;
                }
                RaisePropertyChanged("LeftJumpIndex");
            }
        }

        private long _RightJumpIndex;
        public long RightJumpIndex
        {
            get { return _RightJumpIndex; }
            set
            {
                if (value != _RightJumpIndex)
                {
                    _RightJumpIndex = value;
                }
                RaisePropertyChanged("RightJumpIndex");
            }
        }

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

        private bool _JumpIndexSetupManualType;
        public bool JumpIndexSetupManualType
        {
            get { return _JumpIndexSetupManualType; }
            set
            {
                if (value != _JumpIndexSetupManualType)
                {
                    _JumpIndexSetupManualType = value;
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

        private ObservableCollection<Low_ProcessingPointEnum> _EnumProcessingPoints
             = new ObservableCollection<Low_ProcessingPointEnum>();
        public ObservableCollection<Low_ProcessingPointEnum> EnumProcessingPoints
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
        private void InitValue()
        {
            LeftJumpIndex = 0;
            RightJumpIndex = 0;
        }

        private void EnableJumpIndexTextBox(bool flag)
        {
            try
            {
                TestBoxInEnabled = flag;
                if (!flag)
                {
                    LeftJumpIndex = 0;
                    RightJumpIndex = 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private WA_LowMagParam_Standard LowStandardParam { get; set; }

        #endregion

        #region //..Command & Method

        public LowMagStandardAdvanceSetupViewModel()
        {
            try
            {
                foreach (Low_ProcessingPointEnum processing_point in Enum.GetValues(typeof(Low_ProcessingPointEnum)))
                {
                    EnumProcessingPoints.Add(processing_point);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        public void SettingData(WA_LowMagParam_Standard lowparam)
        {
            try
            {
                LowStandardParam = lowparam;

                if (ConfirmExistManualJumpIndex.Confirm(LowStandardParam.JumpIndexManualInputParam, EnumWAProcDirection.HORIZONTAL) != -1)
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

                LeftJumpIndex = LowStandardParam.JumpIndexManualInputParam.LeftIndex;
                RightJumpIndex = LowStandardParam.JumpIndexManualInputParam.RightIndex;

                PMAcceptance = LowStandardParam.DefaultPMParam.PMAcceptance.Value;
                PMCertainty = LowStandardParam.DefaultPMParam.PMCertainty.Value;

                Low_ProcessingPoint = LowStandardParam.Low_ProcessingPoint.Value;
                FocusParameter focusParameter = new NormalFocusParameter();

                LowStandardParam.FocusParam.CopyTo(focusParameter);

                FocusParam = focusParameter;
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
                LowStandardParam.JumpIndexManualInputParam.LeftIndex = LeftJumpIndex;
                LowStandardParam.JumpIndexManualInputParam.RightIndex = RightJumpIndex;

                LowStandardParam.DefaultPMParam.PMAcceptance.Value = PMAcceptance;
                LowStandardParam.DefaultPMParam.PMCertainty.Value = PMCertainty;

                FocusParam.CopyTo(LowStandardParam.FocusParam);

                LowStandardParam.Low_ProcessingPoint.Value = Low_ProcessingPoint;

                await this.PnPManager().ClosePnpAdavanceSetupWindow();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
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

                LeftJumpIndex = LowStandardParam.JumpIndexManualInputParam.LeftIndex;
                RightJumpIndex = LowStandardParam.JumpIndexManualInputParam.RightIndex;

                PMAcceptance = LowStandardParam.DefaultPMParam.PMAcceptance.Value;
                PMCertainty = LowStandardParam.DefaultPMParam.PMCertainty.Value;

                LowStandardParam.FocusParam.CopyTo(FocusParam);
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
                throw err;
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
                        SerializeManager.DeserializeFromByte(param, out target, typeof(WA_LowMagParam_Standard));
                        if (target != null)
                        {
                            SettingData(target as WA_LowMagParam_Standard);
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
                if (LowStandardParam != null)
                    parameters.Add(SerializeManager.SerializeToByte(LowStandardParam));
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
