using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ProberViewModel
{
    using ProberInterfaces;
    using System.ComponentModel;
    using LogModule;
    using System.Windows.Media.Animation;
    using UcAnimationScrollViewer;
    using System.Globalization;
    using UcDisplayPort;
    using Autofac;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class InspectionControl : UserControl, IMainScreenView
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion
        public static readonly DependencyProperty AxisXPosProperty =
            DependencyProperty.Register("AxisXPos"
            , typeof(ProbeAxisObject),
            typeof(InspectionControl), null);
        public ProbeAxisObject AxisXPos
        {
            get { return (ProbeAxisObject)this.GetValue(AxisXPosProperty); }
            set { this.SetValue(AxisXPosProperty, value); }
        }
        public static readonly DependencyProperty AxisYPosProperty =
DependencyProperty.Register("AxisYPos"
, typeof(ProbeAxisObject),
typeof(InspectionControl), null);
        public ProbeAxisObject AxisYPos
        {
            get { return (ProbeAxisObject)this.GetValue(AxisYPosProperty); }
            set { this.SetValue(AxisYPosProperty, value); }
        }
        readonly Guid _ViewGUID = new Guid("f8396e3a-b8ce-4dcd-9a0d-643532a7d9d1");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        public InspectionControl()
        {
            InitializeComponent();
        }

        private void CategoryUpBtnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DoubleAnimation verticalAnimation = new DoubleAnimation();

                verticalAnimation.From = svViewer.VerticalOffset;
                verticalAnimation.To = svViewer.VerticalOffset - ((svViewer.ActualHeight / 3) * 2);
                verticalAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300));

                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(verticalAnimation);

                Storyboard.SetTarget(verticalAnimation, svViewer);
                Storyboard.SetTargetProperty(verticalAnimation, new PropertyPath(AnimationScrollViewer.CurrentVerticalOffsetProperty));

                storyboard.Begin();
            }
            catch (Exception err)
            {
                throw;
            }
        }

        private void CategoryDwBtnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DoubleAnimation verticalAnimation = new DoubleAnimation();

                verticalAnimation.From = svViewer.VerticalOffset;
                verticalAnimation.To = svViewer.VerticalOffset + ((svViewer.ActualHeight / 3) * 2);
                verticalAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300));

                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(verticalAnimation);

                Storyboard.SetTarget(verticalAnimation, svViewer);
                Storyboard.SetTargetProperty(verticalAnimation, new PropertyPath(AnimationScrollViewer.CurrentVerticalOffsetProperty));
                storyboard.Begin();
            }
            catch (Exception err)
            {
                throw;
            }
        }

        public class TemplateSelector : DataTemplateSelector
        {
            private DataTemplate _MapViewDataTemplate;

            public DataTemplate MapViewDataTemplate
            {
                get { return _MapViewDataTemplate; }
                set { _MapViewDataTemplate = value; }
            }

            private DataTemplate _DisplayViewDataTemplate;

            public DataTemplate DisplayViewDataTemplate
            {
                get { return _DisplayViewDataTemplate; }
                set { _DisplayViewDataTemplate = value; }
            }

            private DataTemplate _RecipeEditDataTemplate;
            public DataTemplate RecipeEditDataTemplate
            {
                get { return _RecipeEditDataTemplate; }
                set { _RecipeEditDataTemplate = value; }
            }

            private DataTemplate _DutViewDataTemplate;

            public DataTemplate DutViewDataTemplate
            {
                get { return _DutViewDataTemplate; }
                set { _DutViewDataTemplate = value; }
            }

            public override DataTemplate SelectTemplate(object item, DependencyObject container)
            {
               
                #region Require

                //if (item == null) throw new ArgumentNullException();
                if (container == null) throw new ArgumentNullException();

                #endregion
                
                // Result
                DataTemplate itemDataTemplate = null;
            try
            {

                // Base DataTemplate
                itemDataTemplate = base.SelectTemplate(item, container);
                if (itemDataTemplate != null) return itemDataTemplate;

                // Interface DataTemplate
                FrameworkElement itemContainer = container as FrameworkElement;
                if (itemContainer == null) return null;

                //foreach (Type itemInterface in item.GetType().GetInterfaces())
                //{
                //    itemDataTemplate = itemContainer.TryFindResource(new DataTemplateKey(itemInterface)) as DataTemplate;
                //    if (itemDataTemplate != null) break;
                //}

                if (item is IWaferObject)
                {
                    return MapViewDataTemplate;
                }
                else if (item is ICamera)
                {
                    return DisplayViewDataTemplate;
                }
                else if (item is IVmRecipeEditorMainPage)
                {
                    return RecipeEditDataTemplate;
                }

                // Return
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
                return itemDataTemplate;
            }

        }
    }

    public class BooleanToIsEnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanToVisiblityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && ((bool)value) == false)
            {
                return Visibility.Hidden;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanToReverseVisiblityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && ((bool)value) == false)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Hidden;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CaclulatedXShiftValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
              object parameter, CultureInfo culture)
        {
            double shift = 0;
            try
            {
                if ((values != null))
                {
                    double AxisXpos = (double)values[0];
                    double XSetFromCoord = (double)values[1];

                    var xShift = Math.Round(AxisXpos - XSetFromCoord, 1);

                    shift = XSetFromCoord + xShift;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return shift;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class CaclulatedYShiftValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
              object parameter, CultureInfo culture)
        {
            double shift = 0;
            try
            {
                if ((values != null))
                {
                    double AxisYpos = (double)values[0];
                    double YSetFromCoord = (double)values[1];

                    var yShift = Math.Round(AxisYpos - YSetFromCoord, 1);

                    shift = YSetFromCoord + yShift;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return shift;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class VirtualKeyBoardTextBoxNoElementConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
              object parameter, CultureInfo culture)
        {
            TextBox textBox = null;
            double obj = 0.0;
            try
            {
                if (values[0] != DependencyProperty.UnsetValue)
                {
                    textBox = (System.Windows.Controls.TextBox)values[0];
                }
                if (values[1] != DependencyProperty.UnsetValue)
                {
                    obj = (double)values[1];
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return (textBox, obj);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
