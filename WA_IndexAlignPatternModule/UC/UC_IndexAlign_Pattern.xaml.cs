using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WAIndexAlignPatternModule.UC
{
    using System.ComponentModel;
    using System.Globalization;
    using MahApps.Metro.Controls;
    using MahApps.Metro.Controls.Dialogs;
    using ProberInterfaces.WaferAlignEX.Enum;
    using LogModule;
    using RelayCommandBase;
    using ProberInterfaces;
    using WAIndexAlignPatternModule;
    using VirtualKeyboardControl;

    /// <summary>
    /// Interaction logic for UC_IndexAlign_Pattern.xaml
    /// </summary>
    public partial class UC_IndexAlign_Pattern : CustomDialog, INotifyPropertyChanged, IFactoryModule, IDataErrorInfo
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event if needed.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private IndexAlignPattern _Module;

        public static EnumWASubModuleEnable SIndexAlingEnable;
        private EnumWASubModuleEnable _IndexAlingEnable;
        public EnumWASubModuleEnable IndexAlingEnable
        {
            get { return _IndexAlingEnable; }
            set
            {
                if (value != _IndexAlingEnable)
                {
                    _IndexAlingEnable = value;
                    SIndexAlingEnable = _IndexAlingEnable;
                    RaisePropertyChanged("IndexAlingEnable");
                }
            }
        }

        public string Error { get { return string.Empty; } }
        private bool ParamValidationFlag = true;
        public string this[string columnName]
        {
            get
            {
                if (columnName == "PMAcceptance" && (this.PMAcceptance < 0 || this.PMAcceptance > 100))
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

                if (columnName == "PMCertainty" && (this.PMCertainty < 0 || this.PMCertainty > 100))
                {
                    ParamValidationFlag = false;
                    return Properties.Resources.PMValueSettingLimitError;

                }
                else if (columnName == "PMCertainty" && this.PMCertainty < this.PMAcceptance)
                {
                    ParamValidationFlag = false;
                    return Properties.Resources.PMCertaintyLimitError;
                }
                else
                    ParamValidationFlag = true;


                return null;
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



        public UC_IndexAlign_Pattern()
        {
            try
            {
                IndexAlingEnable = EnumWASubModuleEnable.UNDEFIND;
                InitializeComponent();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public UC_IndexAlign_Pattern(IndexAlignPattern module)
        {
            try
            {

                _Module = module;

                IWaferObject waferobject = this.StageSupervisor().WaferObject;
                double diesizex = waferobject.GetSubsInfo().ActualDieSize.Width.Value;
                double diesizey = waferobject.GetSubsInfo().ActualDieSize.Height.Value;

                ICamera whcam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);
                double displaysizex = whcam.GetGrabSizeWidth() * whcam.GetRatioX();
                double displaysizey = whcam.GetGrabSizeHeight() * whcam.GetRatioY();
                IndexAlingEnable = EnumWASubModuleEnable.UNDEFIND;

                InitializeComponent();

                if ((diesizex < displaysizex) || (diesizey < displaysizey))
                {
                    rb_enalbe.IsEnabled = false;
                    tb_disable.IsEnabled = false;
                }
                else
                {
                    IndexAlingEnable = module.IndexAlignPatternParam_Clone.AlignEnable;

                    rb_enalbe.IsEnabled = true;
                    tb_disable.IsEnabled = true;

                }


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private AsyncCommand _CmdMaunalInputContolOKClick;
        public ICommand CmdMaunalInputContolOKClick
        {
            get
            {
                if (null == _CmdMaunalInputContolOKClick) _CmdMaunalInputContolOKClick
                        = new AsyncCommand(MaunalInputContolOKClick);
                return _CmdMaunalInputContolOKClick;
            }
        }

        private async Task MaunalInputContolOKClick()
        {
            try
            {
                if (!ParamValidationFlag)
                    return;
                await this.MetroDialogManager().CloseWindow(this);
                _Module.IndexAlignPatternParam_Clone.AlignEnable = IndexAlingEnable;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }



        private AsyncCommand _CmdMaunalInputContolCancelClick;
        public ICommand CmdMaunalInputContolCancelClick
        {
            get
            {
                if (null == _CmdMaunalInputContolCancelClick) _CmdMaunalInputContolCancelClick
                        = new AsyncCommand(MaunalInputContolCancelClick);
                return _CmdMaunalInputContolCancelClick;
            }
        }

        private async Task MaunalInputContolCancelClick()
        {
            try
            {
                await this.MetroDialogManager().CloseWindow(this);

                IndexAlingEnable = _Module.IndexAlignPatternParam_Clone.AlignEnable;
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


    }

    public class IndexAlignEnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                EnumWASubModuleEnable param = (EnumWASubModuleEnable)parameter;
                switch (UC_IndexAlign_Pattern.SIndexAlingEnable)
                {
                    case EnumWASubModuleEnable.ENABLE:
                        {
                            switch (param)
                            {
                                case EnumWASubModuleEnable.ENABLE:
                                    return true;
                                case EnumWASubModuleEnable.DISABLE:
                                    return false;
                            }
                        }
                        break;
                    case EnumWASubModuleEnable.DISABLE:
                        {
                            switch (param)
                            {
                                case EnumWASubModuleEnable.ENABLE:
                                    return false;
                                case EnumWASubModuleEnable.DISABLE:
                                    return true;
                            }
                        }
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(true) ? parameter : Binding.DoNothing;

        }
    }
}
