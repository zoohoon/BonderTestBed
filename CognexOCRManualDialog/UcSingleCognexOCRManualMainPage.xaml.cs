using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace CognexOCRManualDialog
{
    /// <summary>
    /// Interaction logic for UcSingleCognexOCRManualMainPage.xaml
    /// </summary>
    using Cognex.Controls;
    using LogModule;
    using ProberInterfaces;
    using System.Globalization;
    using System.Windows.Markup;

    /// <summary>
    /// UcCognexOCRManualMainPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcSingleCognexOCRManualMainPage : UserControl, IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("0e592af2-bd77-4de9-9ea8-958cf98553cc");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        public UcSingleCognexOCRManualMainPage()
        {
            InitializeComponent();

            if (SystemManager.SysteMode == SystemModeEnum.Multiple)
            {
                return;
            }

            //UserControl_Loaded(null, null);


            InSightDisplayApp.SetLoaderContainer(this.LoaderController().GetLoaderContainer());
            InSightDisplayApp.Get();
        }


        //==> Live 영상 출력해야 할 때 필요
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //if (InSightDisplayApp.Get().Parent == null)
            //    this.CognexDisplay.Children.Add(InSightDisplayApp.Get());
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            //if (this.CognexDisplay.Children.Contains(InSightDisplayApp.Get()))
            //    this.CognexDisplay.Children.Remove(InSightDisplayApp.Get());
        }

        private void exitBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            exitBtn.Opacity = 0.5;
        }

        private void exitBtn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            exitBtn.Opacity = 1;
        }

        private void exitBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            exitBtn.Opacity = 1;
        }
    }

    /// <summary>
	/// Converts a Boolean value into a Visibility enumeration (and back)
	/// </summary>
	[ValueConversion(typeof(bool), typeof(Visibility))]
    [MarkupExtensionReturnType(typeof(BoolToVisibilityConverter))]
    public class BoolToVisibilityConverter : MarkupExtension, IValueConverter
    {
        /// <summary>
        /// FalseEquivalent (default : Visibility.Collapsed => see Constructor)
        /// </summary>
        public Visibility FalseEquivalent { get; set; }
        /// <summary>
        /// Define whether the opposite boolean value is crucial (default : false)
        /// </summary>
        public bool OppositeBooleanValue { get; set; }

        /// <summary>
        /// Initialize the properties with standard values
        /// </summary>
        public BoolToVisibilityConverter()
        {
            try
            {
                this.FalseEquivalent = Visibility.Collapsed;
                this.OppositeBooleanValue = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        //+------------------------------------------------------------------------
        //+ Usage :
        //+ -------
        //+ 1. <conv:BoolToVisibilityConverter x:Key="BoolToVisibilityConverter" />
        //+ 2. {Binding ... Converter={StaticResource BoolToVisibilityConverter}
        //+------------------------------------------------------------------------
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is bool && targetType == typeof(Visibility))
                {
                    bool? booleanValue = (bool?)value;

                    if (this.OppositeBooleanValue == true)
                    {
                        booleanValue = !booleanValue;
                    }

                    return booleanValue.GetValueOrDefault() ? Visibility.Visible : FalseEquivalent;
                }

                return value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is Visibility)
                {
                    Visibility visibilityValue = (Visibility)value;

                    if (this.OppositeBooleanValue == true)
                    {
                        visibilityValue = visibilityValue == Visibility.Visible ? FalseEquivalent : Visibility.Visible;
                    }

                    return (visibilityValue == Visibility.Visible);
                }

                return value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        #endregion // IValueConverter Members

        //+-----------------------------------------------------------------------------------
        //+ Usage :	(wpfsl: => XML Namespace mapping to the "BoolToVisibilityConverter" class)
        //+ -------
        //+ Use as follows : {Binding ... Converter={wpfsl:BoolToVisibilityConverter}
        //+ NO StaticResource required
        //+-----------------------------------------------------------------------------------
        #region MarkupExtension "overrides"

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            try
            {
                return new BoolToVisibilityConverter
                {
                    FalseEquivalent = this.FalseEquivalent,
                    OppositeBooleanValue = this.OppositeBooleanValue
                };
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        #endregion
    }


}
