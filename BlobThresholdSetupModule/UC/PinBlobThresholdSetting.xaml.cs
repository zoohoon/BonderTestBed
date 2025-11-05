using LogModule;
using MahApps.Metro.Controls.Dialogs;
using ProbeCardObject;
using ProberInterfaces;
using RelayCommandBase;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace BlobThresholdSetupModule.UC
{
    /// <summary>
    /// Interaction logic for PinBlobThresholdSetting.xaml
    /// </summary>
    public partial class PinBlobThresholdSetting : CustomDialog, IFactoryModule, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public PinBlobThresholdSetting()
        {
            try
            {
                DataContext = this;
                InitializeComponent();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public PinBlobThresholdSetting(PinAlignDevParameters pin_DevParam)
        {
            try
            {
                this.pinParam = pin_DevParam;
                DataContext = this;
                InitializeComponent();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void SettingData(PinAlignDevParameters pin_DevParam)
        {
            try
            {
                this.pinParam = pin_DevParam;
                ThreshTypeEnum = pin_DevParam?.EnableAutoThreshold.Value ?? EnumThresholdType.AUTO;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private PinAlignDevParameters pinParam;

        public static EnumThresholdType ThreshType;

        private EnumThresholdType _ThreshTypeEnum;
        public EnumThresholdType ThreshTypeEnum
        {
            get { return _ThreshTypeEnum; }
            set
            {
                if (value != _ThreshTypeEnum)
                {
                    _ThreshTypeEnum = value;
                    ThreshType = _ThreshTypeEnum;
                    NotifyPropertyChanged("ThreshTypeEnum");
                }
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
                this.pinParam.EnableAutoThreshold.Value = ThreshTypeEnum;
                await this.MetroDialogManager().CloseWindow(this);
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
                ThreshTypeEnum = this.pinParam?.EnableAutoThreshold.Value ?? EnumThresholdType.AUTO;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        
    }

    //UI Converter
    public class ThresholdTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {


                EnumThresholdType param = (EnumThresholdType)parameter;
                switch (PinBlobThresholdSetting.ThreshType)
                {
                    case EnumThresholdType.AUTO:
                        {
                            switch (param)
                            {
                                case EnumThresholdType.AUTO:
                                    return true;
                                case EnumThresholdType.MANUAL:
                                    return false;                                
                            }
                        }
                        break;
                    case EnumThresholdType.MANUAL:
                        {
                            switch (param)
                            {
                                case EnumThresholdType.AUTO:
                                    return false;
                                case EnumThresholdType.MANUAL:
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
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return value.Equals(true) ? parameter : Binding.DoNothing;                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }



}
