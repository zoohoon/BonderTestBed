using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EdgeIndexAlignAdvanceSetup.ViewModel
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Input;
    using LogModule;
    using ProberInterfaces;
    using ProberInterfaces.WaferAlignEX.Enum;
    using RelayCommandBase;
    using SerializerUtil;
    using VirtualKeyboardControl;
    using WA_IndexAlignParameter_Edge;

    public class EdgeIndexAlignAdvanceSetupViewModel : INotifyPropertyChanged, IFactoryModule, IDataErrorInfo, IPnpAdvanceSetupViewModel
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

        #region //..Property

        private WA_IndexAlignParam_Edge _Param { get; set; }
        private EnumWASubModuleEnable _IndexAlingEnable;
        public EnumWASubModuleEnable IndexAlingEnable
        {
            get { return _IndexAlingEnable; }
            set
            {
                //if (value != _IndexAlingEnable)
                //{
                _IndexAlingEnable = value;
                RaisePropertyChanged("IndexAlingEnable");
                //}
            }
        }

        private int _TabIndex = 0;

        public int TabIndex
        {
            get { return _TabIndex; }
            set
            {
                if (_TabIndex != value)
                {
                    _TabIndex = value;
                    RaisePropertyChanged("TabIndex");
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

        private int _AllowableRange;
        public int AllowableRange
        {
            get { return _AllowableRange; }
            set
            {
                if (value != _AllowableRange)
                {
                    _AllowableRange = value;
                    RaisePropertyChanged("AllowableRange");
                }
            }
        }

        private int _AlignThreshold;
        public int AlignThreshold
        {
            get { return _AlignThreshold; }
            set
            {
                if (value != _AlignThreshold)
                {
                    _AlignThreshold = value;
                    RaisePropertyChanged("AlignThreshold");
                }
            }
        }

        public int _PositionToleranceWCtoTargetdie;
        public int PositionToleranceWCtoTargetdie
        {
            get { return _PositionToleranceWCtoTargetdie; }
            set
            {
                _PositionToleranceWCtoTargetdie = value;
                RaisePropertyChanged("PositionToleranceWCtoTargetdie");
            }
        }
        public double _WaferCentertoTargetX;
        public double WaferCentertoTargetX
        {
            get { return _WaferCentertoTargetX; }
            set
            {
                _WaferCentertoTargetX = value;
                RaisePropertyChanged("WaferCentertoTargetX");
            }
        }
        public double _WaferCentertoTargetY;
        public double WaferCentertoTargetY
        {
            get { return _WaferCentertoTargetY; }
            set
            {
                _WaferCentertoTargetY = value;
                RaisePropertyChanged("WaferCentertoTargetY");
            }
        }
        #endregion

        #region //.. Command & Method

        public void SettingData(WA_IndexAlignParam_Edge param)
        {
            try
            {
                this.TabIndex = 0;
                this.HiddenTabVisibility = Visibility.Collapsed;

                this._Param = param;
                IndexAlingEnable = _Param.AlignEnable;
                AllowableRange = _Param.AllowableRange.Value;
                AlignThreshold = _Param.AlignThreshold.Value;
                PositionToleranceWCtoTargetdie = _Param.PositionToleranceWCtoTargetdie.Value;
                WaferCentertoTargetX = _Param.WaferCentertoTargetX.Value;
                WaferCentertoTargetY = _Param.WaferCentertoTargetY.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
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
                {
                    return;
                }

                _Param.PositionToleranceWCtoTargetdie.Value = PositionToleranceWCtoTargetdie;
                _Param.AlignEnable = IndexAlingEnable;
                
                _Param.AllowableRange.Value = AllowableRange;
                _Param.AlignThreshold.Value = AlignThreshold;
                
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

                PositionToleranceWCtoTargetdie = _Param.PositionToleranceWCtoTargetdie.Value;
                IndexAlingEnable = _Param.AlignEnable;

                AllowableRange = _Param.AllowableRange.Value;
                AlignThreshold = _Param.AlignThreshold.Value;
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

        private RelayCommand<Object> _AllowableRangeCommand;
        public ICommand AllowableRangeCommand
        {
            get
            {
                if (null == _AllowableRangeCommand) _AllowableRangeCommand = new RelayCommand<Object>(AllowableRangeCommandFunc);
                return _AllowableRangeCommand;
            }
        }


        private void AllowableRangeCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 3);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private RelayCommand<Object> _ThresholdClickCommand;
        public ICommand ThresholdClickCommand
        {
            get
            {
                if (null == _ThresholdClickCommand) _ThresholdClickCommand = new RelayCommand<Object>(ThresholdClickCommandFunc);
                return _ThresholdClickCommand;
            }
        }


        private void ThresholdClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 3);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void Init()
        {
            return;
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
                        SerializeManager.DeserializeFromByte(param, out target, typeof(WA_IndexAlignParam_Edge));
                        if (target != null)
                        {
                            SettingData(target as WA_IndexAlignParam_Edge);
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
                if (_Param != null)
                    parameters.Add(SerializeManager.SerializeToByte(_Param));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return parameters;
        }

        #endregion

        #region //..IDataErrorInfo
        public string Error { get { return string.Empty; } }
        private bool ParamValidationFlag = true;
        public string this[string columnName]
        {
            get
            {
                if (columnName == "PositionToleranceWCtoTargetdie")
                {
                    if (this.PositionToleranceWCtoTargetdie < 0 || this.PositionToleranceWCtoTargetdie > 100)
                    {
                        ParamValidationFlag = false;
                        return "It can be set from 0 to 100.";
                    }
                }
                else if(columnName == "AllowableRange")
                {
                    if (this.AllowableRange < 8 || this.AllowableRange > 80)
                    {
                        ParamValidationFlag = false;
                        return "It can be set from 8 to 80.";
                    }
                }
                else if (columnName == "AlignThreshold")
                {
                    if (this.AlignThreshold <= 0)
                    {
                        ParamValidationFlag = false;
                        return "The value must be set to greater than 0.";
                    }
                }

                ParamValidationFlag = true;
                return "";
            }
        }
        #endregion
    }
}
