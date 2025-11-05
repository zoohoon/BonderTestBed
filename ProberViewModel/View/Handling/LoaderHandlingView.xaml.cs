using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace ProberViewModel
{
    using LoaderParameters.Data;
    using ProberInterfaces;
    using ProberInterfaces.Foup;
    using System.Globalization;

    /// <summary>
    /// LoaderHandlingView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoaderHandlingView : UserControl, IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("156F45C2-472E-A15D-1B1E-793F7E22DCA4");
        public Guid ScreenGUID { get { return _ViewGUID; } }
        public LoaderHandlingView()
        {
            InitializeComponent();
        }
    }

    public class MouseBehaviour
    {
        #region MouseUp

        public static readonly DependencyProperty MouseUpCommandProperty =
            DependencyProperty.RegisterAttached("MouseUpCommand", typeof(ICommand), typeof(MouseBehaviour), new FrameworkPropertyMetadata(new PropertyChangedCallback(MouseUpCommandChanged)));

        private static void MouseUpCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)d;

            element.MouseUp += element_MouseUp;
        }

        static void element_MouseUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;

            ICommand command = GetMouseUpCommand(element);

            command.Execute(e);
        }



        public static void SetMouseUpCommand(UIElement element, ICommand value)
        {
            element.SetValue(MouseUpCommandProperty, value);
        }

        public static ICommand GetMouseUpCommand(UIElement element)
        {
            return (ICommand)element.GetValue(MouseUpCommandProperty);
        }

        #endregion

        #region MouseDown

        public static readonly DependencyProperty MouseDownCommandProperty =
            DependencyProperty.RegisterAttached("MouseDownCommand", typeof(ICommand), typeof(MouseBehaviour), new FrameworkPropertyMetadata(new PropertyChangedCallback(MouseDownCommandChanged)));

        private static void MouseDownCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)d;

            element.MouseDown += element_MouseDown;
        }

        static void element_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;

            ICommand command = GetMouseDownCommand(element);

            command.Execute(e);
        }

        public static void SetMouseDownCommand(UIElement element, ICommand value)
        {
            element.SetValue(MouseDownCommandProperty, value);
        }

        public static ICommand GetMouseDownCommand(UIElement element)
        {
            return (ICommand)element.GetValue(MouseDownCommandProperty);
        }

        #endregion

        #region MouseLeave

        public static readonly DependencyProperty MouseLeaveCommandProperty =
            DependencyProperty.RegisterAttached("MouseLeaveCommand", typeof(ICommand), typeof(MouseBehaviour), new FrameworkPropertyMetadata(new PropertyChangedCallback(MouseLeaveCommandChanged)));

        private static void MouseLeaveCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)d;

            element.MouseLeave += new MouseEventHandler(element_MouseLeave);
        }

        static void element_MouseLeave(object sender, MouseEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;

            ICommand command = GetMouseLeaveCommand(element);

            command.Execute(e);
        }

        public static void SetMouseLeaveCommand(UIElement element, ICommand value)
        {
            element.SetValue(MouseLeaveCommandProperty, value);
        }

        public static ICommand GetMouseLeaveCommand(UIElement element)
        {
            return (ICommand)element.GetValue(MouseLeaveCommandProperty);
        }
        #endregion

        #region MouseLeftButtonDown

        public static readonly DependencyProperty MouseLeftButtonDownCommandProperty =
            DependencyProperty.RegisterAttached("MouseLeftButtonDownCommand", typeof(ICommand), typeof(MouseBehaviour), new FrameworkPropertyMetadata(new PropertyChangedCallback(MouseLeftButtonDownCommandChanged)));

        private static void MouseLeftButtonDownCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)d;

            element.MouseLeftButtonDown += element_MouseLeftButtonDown;
        }

        static void element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;

            ICommand command = GetMouseLeftButtonDownCommand(element);

            command.Execute(e);
        }

        public static void SetMouseLeftButtonDownCommand(UIElement element, ICommand value)
        {
            element.SetValue(MouseLeftButtonDownCommandProperty, value);
        }

        public static ICommand GetMouseLeftButtonDownCommand(UIElement element)
        {
            return (ICommand)element.GetValue(MouseLeftButtonDownCommandProperty);
        }

        #endregion

        #region MouseLeftButtonUp

        public static readonly DependencyProperty MouseLeftButtonUpCommandProperty =
            DependencyProperty.RegisterAttached("MouseLeftButtonUpCommand", typeof(ICommand), typeof(MouseBehaviour), new FrameworkPropertyMetadata(new PropertyChangedCallback(MouseLeftButtonUpCommandChanged)));

        private static void MouseLeftButtonUpCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)d;

            element.MouseLeftButtonUp += element_MouseLeftButtonUp;
        }

        static void element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;

            ICommand command = GetMouseLeftButtonUpCommand(element);

            command.Execute(e);
        }

        public static void SetMouseLeftButtonUpCommand(UIElement element, ICommand value)
        {
            element.SetValue(MouseLeftButtonUpCommandProperty, value);
        }

        public static ICommand GetMouseLeftButtonUpCommand(UIElement element)
        {
            return (ICommand)element.GetValue(MouseLeftButtonUpCommandProperty);
        }

        #endregion

        #region MouseMove

        public static readonly DependencyProperty MouseMoveCommandProperty =
            DependencyProperty.RegisterAttached("MouseMoveCommand", typeof(ICommand), typeof(MouseBehaviour), new FrameworkPropertyMetadata(new PropertyChangedCallback(MouseMoveCommandChanged)));

        private static void MouseMoveCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)d;

            element.MouseMove += new MouseEventHandler(element_MouseMove);
        }

        static void element_MouseMove(object sender, MouseEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;

            ICommand command = GetMouseMoveCommand(element);

            command.Execute(e);
        }

        public static void SetMouseMoveCommand(UIElement element, ICommand value)
        {
            element.SetValue(MouseMoveCommandProperty, value);
        }

        public static ICommand GetMouseMoveCommand(UIElement element)
        {
            return (ICommand)element.GetValue(MouseMoveCommandProperty);
        }

        #endregion

        #region MouseRightButtonDown

        public static readonly DependencyProperty MouseRightButtonDownCommandProperty =
            DependencyProperty.RegisterAttached("MouseRightButtonDownCommand", typeof(ICommand), typeof(MouseBehaviour), new FrameworkPropertyMetadata(new PropertyChangedCallback(MouseRightButtonDownCommandChanged)));

        private static void MouseRightButtonDownCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)d;

            element.MouseRightButtonDown += element_MouseRightButtonDown;
        }

        static void element_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;

            ICommand command = GetMouseRightButtonDownCommand(element);

            command.Execute(e);
        }

        public static void SetMouseRightButtonDownCommand(UIElement element, ICommand value)
        {
            element.SetValue(MouseRightButtonDownCommandProperty, value);
        }

        public static ICommand GetMouseRightButtonDownCommand(UIElement element)
        {
            return (ICommand)element.GetValue(MouseRightButtonDownCommandProperty);
        }

        #endregion

        #region MouseRightButtonUp

        public static readonly DependencyProperty MouseRightButtonUpCommandProperty =
            DependencyProperty.RegisterAttached("MouseRightButtonUpCommand", typeof(ICommand), typeof(MouseBehaviour), new FrameworkPropertyMetadata(new PropertyChangedCallback(MouseRightButtonUpCommandChanged)));

        private static void MouseRightButtonUpCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)d;

            element.MouseRightButtonUp += element_MouseRightButtonUp;
        }

        static void element_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;

            ICommand command = GetMouseRightButtonUpCommand(element);

            command.Execute(e);
        }

        public static void SetMouseRightButtonUpCommand(UIElement element, ICommand value)
        {
            element.SetValue(MouseRightButtonUpCommandProperty, value);
        }

        public static ICommand GetMouseRightButtonUpCommand(UIElement element)
        {
            return (ICommand)element.GetValue(MouseRightButtonUpCommandProperty);
        }

        #endregion

        #region MouseWheel

        public static readonly DependencyProperty MouseWheelCommandProperty =
            DependencyProperty.RegisterAttached("MouseWheelCommand", typeof(ICommand), typeof(MouseBehaviour), new FrameworkPropertyMetadata(new PropertyChangedCallback(MouseWheelCommandChanged)));

        private static void MouseWheelCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)d;

            element.MouseWheel += new MouseWheelEventHandler(element_MouseWheel);
        }

        static void element_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;

            ICommand command = GetMouseWheelCommand(element);

            command.Execute(e);
        }

        public static void SetMouseWheelCommand(UIElement element, ICommand value)
        {
            element.SetValue(MouseWheelCommandProperty, value);
        }

        public static ICommand GetMouseWheelCommand(UIElement element)
        {
            return (ICommand)element.GetValue(MouseWheelCommandProperty);
        }

        #endregion
    }

    public class FoupLabelColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if(values[1] is bool)
            {
                bool enable = (bool)values[1];
                if(enable == false)
                {
                    return new SolidColorBrush(Colors.OrangeRed);
                }
            }
            if (values[0] is FoupModeStatusEnum)
            {
                FoupModeStatusEnum targetEnum = (FoupModeStatusEnum)values[0];
                if (targetEnum == FoupModeStatusEnum.ONLINE)
                    return new SolidColorBrush(Colors.White);
                else if (targetEnum == FoupModeStatusEnum.OFFLINE)
                    return new SolidColorBrush(Colors.Yellow);

            }
            return new SolidColorBrush(Colors.White);
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class FoupStateLabelTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string text = "";
            if (values[1] is bool)
            {
                bool enable = (bool)values[1];
                if (enable == false)
                {
                    text += "DISABLE";
                    return text;
                }
                else
                {
                    text += "ENABLE";
                }
            }
            if (values[0] is FoupModeStatusEnum)
            {
                FoupModeStatusEnum targetEnum = (FoupModeStatusEnum)values[0];
                if (targetEnum == FoupModeStatusEnum.ONLINE)
                {
                    text += "/ ON-LINE";
                }
                else if (targetEnum == FoupModeStatusEnum.OFFLINE)
                {
                    text += "/ OFF-LINE";
                }
            }
            return text;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    //public class ListViewConverter : IMultiValueConverter
    //{
    //    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return values.Clone();
    //    }

    //    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    //    {
    //        return null;
    //    }
    //}

    //public class ImageConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {

    //        if (value is StageObject)
    //        {
    //            return SetIconSoruceBitmap(Properties.Resources.Cell);
    //        }
    //        else if (value is ArmObject)
    //        {
    //            return SetIconSoruceBitmap(Properties.Resources.Arm);
    //        }
    //        else if (value is SlotObject)
    //        {
    //            return SetIconSoruceBitmap(Properties.Resources.FOUP);
    //        }
    //        else if (value is PAObject)
    //        {
    //            return SetIconSoruceBitmap(Properties.Resources.PA);
    //        }
    //        else if (value is BufferObject)
    //        {
    //            return SetIconSoruceBitmap(Properties.Resources.Buffer);
    //        }
    //        else if (value is CardBufferObject)
    //        {
    //            return SetIconSoruceBitmap(Properties.Resources.CardHand);
    //        }
    //        else if (value is CardTrayObject)
    //        {
    //            return SetIconSoruceBitmap(Properties.Resources.CardTray);
    //        }
    //        else if (value is CardArmObject)
    //        {
    //            return SetIconSoruceBitmap(Properties.Resources.CardHand);
    //        }
    //        else if(value is FixedTrayInfoObject)
    //        {
    //            return SetIconSoruceBitmap(Properties.Resources.Buffer);
    //        }
    //        else if (value is InspectionTrayInfoObject)
    //        {
    //            return SetIconSoruceBitmap(Properties.Resources.Buffer);
    //        }
    //        else if (value is bool)
    //        {
    //            return SetIconSoruceBitmap(Properties.Resources.selecteicon);
    //        }

    //        return null;
    //    }
    //    public BitmapImage SetIconSoruceBitmap(Bitmap bitmap)
    //    {
    //        try
    //        {
    //            BitmapImage image = new BitmapImage();
    //            Application.Current.Dispatcher.Invoke(delegate
    //            {
    //                MemoryStream ms = new MemoryStream();
    //                (bitmap).Save(ms, System.Drawing.Imaging.ImageFormat.Png);
    //                image.BeginInit();
    //                ms.Seek(0, SeekOrigin.Begin);
    //                image.StreamSource = ms;
    //                image.EndInit();
    //            });
    //            return image;
    //        }
    //        catch (Exception err)
    //        {
    //            throw err;
    //        }
    //    }
    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return null;
    //    }
    //}

    //public class TransferObjectLabelConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        if (value is StageObject)
    //        {
    //            return (value as StageObject).Name;
    //        }
    //        else if (value is ArmObject)
    //        {
    //            return (value as ArmObject).Name;
    //        }
    //        else if (value is SlotObject)
    //        {
    //            string str = "";
    //            str = $"Foup#{(value as SlotObject).FoupNumber + 1} - {(value as SlotObject).Name}";
    //            return str;
    //        }else if(value is FixedTrayInfoObject)
    //        {
    //            return (value as FixedTrayInfoObject).Name;
    //        }
    //        else if (value is PAObject)
    //        {
    //            return (value as PAObject).Name;
    //        }
    //        else if (value is BufferObject)
    //        {
    //            return (value as BufferObject).Name;
    //        }
    //        else if (value is CardBufferObject)
    //        {
    //            return (value as CardBufferObject).Name;
    //        }
    //        else if (value is CardTrayObject)
    //        {
    //            return (value as CardTrayObject).Name;
    //        }
    //        else if (value is CardArmObject)
    //        {
    //            return (value as CardArmObject).Name;
    //        }
    //        else if (value is InspectionTrayInfoObject)
    //        {
    //            return (value as InspectionTrayInfoObject).Name;
    //        }
    //        else if (value is bool)
    //        {
    //            return null;
    //        }

    //        return null;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return null;
    //    }
    //}

    public class SlotAutomationIDConverter : IValueConverter
    {
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            string retval = string.Empty;

            try
            {
                ListBoxItem item = (ListBoxItem)value;
                ListBox listView = ItemsControl.ItemsControlFromItemContainer(item) as ListBox;
                int index = listView.ItemContainerGenerator.IndexFromContainer(item);

                SlotObject slotobj = listView.ItemContainerGenerator.ItemFromContainer(item) as SlotObject;

                //index++;

                retval = $"lvFoup#{slotobj.FoupNumber + 1}{slotobj.Name}";

                return retval;
            }
            catch (Exception err)
            {
                throw;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
