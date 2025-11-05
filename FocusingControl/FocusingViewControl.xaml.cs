using LogModule;
using ProberInterfaces.Param;
using System;
using System.Windows;
using System.Windows.Controls;

namespace FocusingControl
{
    /// <summary>
    /// FocusingViewControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FocusingViewControl : UserControl
    {
        private FocusingControlViewModel _ControlViewModel;

        public FocusingControlViewModel ControlViewModel
        {
            get { return _ControlViewModel; }
            set { _ControlViewModel = value; }
        }

        public FocusingViewControl()
        {
            ControlViewModel = new FocusingControlViewModel();
            this.DataContext = ControlViewModel;

            InitializeComponent();
        }

        #region ..//DependencyProperty

        private static void FocusParamPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                FocusingViewControl mv = null;
                if (sender is FocusingViewControl)
                {
                    mv = (FocusingViewControl)sender;
                }
                else
                    return;

                if (sender != null && e.NewValue != null)
                {
                    mv.FocusParam = (FocusParameter)e.NewValue;
                    mv.ControlViewModel.SetFocusParam(mv.FocusParam);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public static readonly DependencyProperty FocusParamProperty =
            DependencyProperty.Register(nameof(FocusParam), typeof(FocusParameter), typeof(FocusingViewControl),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(FocusParamPropertyChanged)));
        public FocusParameter FocusParam
        {
            get { return (FocusParameter)this.GetValue(FocusParamProperty); }
            set { this.SetValue(FocusParamProperty, value); }
        }

        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
