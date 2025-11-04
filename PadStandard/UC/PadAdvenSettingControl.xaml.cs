using System;
using System.Windows;
using System.Windows.Input;

namespace WAPadStandardModule.UC
{
    using LogModule;
    using MahApps.Metro.Controls.Dialogs;
    using ProberInterfaces;
    using RelayCommandBase;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using VirtualKeyboardControl;

    /// <summary>
    /// Interaction logic for PadAdvenSettingControl.xaml
    /// </summary>
    public partial class PadAdvenSettingControl : CustomDialog, INotifyPropertyChanged, IDataErrorInfo
    {

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Error { get { return string.Empty; } }

        public string this[string columnName]
        {
            get
            {
                if(columnName == "BumpPadHeight")
                {
                    if(PadType == EnumPadType.NEGATIVE_BUMP)
                    {
                        if (this.BumpPadHeight > 0 || this.BumpPadHeight < -1000)
                            return "Minimum: -1000, Maximum: 0.";
                    }
                    else if (PadType == EnumPadType.POSITIVE_BUMP)
                    {
                        if (this.BumpPadHeight > 1000 || this.BumpPadHeight < 0)
                            return "Minimum: 0, Maximum: 1000.";
                    }
                }
                return "";
            }
        }


        public PadAdvenSettingControl()
        {
            InitializeComponent();
        }

        private PadStandard _PadModule;

        private EnumPadType _PadType;
        public EnumPadType PadType
        {
            get { return _PadType; }
            set
            {
                if (value != _PadType)
                {
                    _PadType = value;
                    if(_PadType == EnumPadType.NEGATIVE_BUMP
                         || _PadType == EnumPadType.POSITIVE_BUMP)
                    {
                        TBBumpPadHeightEnable = true;
                    }
                    else
                    {
                        TBBumpPadHeightEnable = false;
                    }
                    RaisePropertyChanged();
                }
            }
        }

        private double _BumpPadHeight;
        public double BumpPadHeight
        {
            get { return _BumpPadHeight; }
            set
            {
                if (value != _BumpPadHeight)
                {
                    _BumpPadHeight = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _TBBumpPadHeightEnable;
        public bool TBBumpPadHeightEnable
        {
            get { return _TBBumpPadHeightEnable; }
            set
            {
                if (value != _TBBumpPadHeightEnable)
                {
                    _TBBumpPadHeightEnable = value;
                    RaisePropertyChanged();
                }
            }
        }



        public PadAdvenSettingControl(PadStandard padModule)
        {
            try
            {
            this.DataContext = this;
            InitializeComponent();

            foreach (EnumPadType hpoint in Enum.GetValues(typeof(EnumPadType)))
            {
                if (hpoint != EnumPadType.UNDEFINED & hpoint != EnumPadType.INVALID)
                {
                    combo_padtype.Items.Add(hpoint);
                }
            }

            _PadModule = padModule;
            PadType = _PadModule.WaferObject.GetPhysInfo().PadType.Value;
            BumpPadHeight = _PadModule.WaferObject.GetPhysInfo().BumpPadHeight.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }


        private async void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(PadType != _PadModule.WaferObject.GetPhysInfo().PadType.Value)
                {
                    _PadModule.WaferObject.GetPhysInfo().PadType.Value = PadType;
                    _PadModule.WaferObject.GetPhysInfo().BumpPadHeight.Value = BumpPadHeight;
                    switch (PadType)
                    {
                        case EnumPadType.NORMAL:
                            _PadModule.PadStandardParam_Clone.FocusingModuleDllInfo = FocusingDLLInfo.GetNomalFocusingDllInfo();
                            break;
                        case EnumPadType.NEGATIVE_BUMP:
                            break;
                        case EnumPadType.POSITIVE_BUMP:
                            break;
                    }

                    _PadModule.IsParamChanged = true;
                }

                await (Application.Current.MainWindow as MahApps.Metro.Controls.MetroWindow).HideMetroDialogAsync(this);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }

        }

        private async void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            await (Application.Current.MainWindow as MahApps.Metro.Controls.MetroWindow).HideMetroDialogAsync(this);
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
            if(PadType == EnumPadType.NEGATIVE_BUMP)
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, -1000, 0);
            else if(PadType == EnumPadType.POSITIVE_BUMP)
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, -1000);
            tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

    }
}
