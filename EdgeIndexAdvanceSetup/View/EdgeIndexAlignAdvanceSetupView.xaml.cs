using System;
using System.Windows.Data;

namespace EdgeIndexAlignAdvanceSetup.View
{
    using EdgeIndexAlignAdvanceSetup.ViewModel;
    using LogModule;
    using MahApps.Metro.Controls.Dialogs;
    using ProberInterfaces.WaferAlignEX.Enum;
    using System.Globalization;
    using System.Windows.Input;

    /// <summary>
    /// EdgeIndexAlignAdvanceSetupView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class EdgeIndexAlignAdvanceSetupView : CustomDialog
    {
        public EdgeIndexAlignAdvanceSetupView()
        {
            InitializeComponent();
            this.KeyDown += EdgeIndexAlignAdvanceSetupView_KeyDown;
        }
        private void EdgeIndexAlignAdvanceSetupView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
            {
                if (e.KeyboardDevice.IsKeyDown(Key.H))
                {
                    var viewModel = this.DataContext as EdgeIndexAlignAdvanceSetupViewModel;

                    if (viewModel != null)
                    {
                        viewModel.HiddenTabVisibility = System.Windows.Visibility.Visible;
                    }
                }
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
                switch (value)
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
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }
}
