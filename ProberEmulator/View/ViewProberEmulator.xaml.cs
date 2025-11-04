using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using LoaderParameters.Data;
using LogModule;
using ProberEmulator.ViewModel;
using ProberInterfaces;
using ProberInterfaces.Enum;
using ProberInterfaces.Network;

namespace ProberEmulator.View
{
    /// <summary>
    /// ViewServerEmulator.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ViewProberEmulator : UserControl
    {
        private VMProberEmulator VMProberEmulator;
        public ViewProberEmulator()
        {
            try
            {
                InitializeComponent();

                VMProberEmulator = new VMProberEmulator();
                VMProberEmulator.InitViewModel();

                this.DataContext = VMProberEmulator;
                
                FoupListView.MaxHeight = SystemParameters.WorkArea.Height * 0.8;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

        public new void Loaded()
        {
            VMProberEmulator.Loaded();
        }

        private void Viewbox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }

        //private void Viewbox_SizeChanged(object sender, SizeChangedEventArgs e)
        //{
        //    e.Handled = true;
        //}
    }

    //public class IntValidationRule : ValidationRule
    //{
    //    public int Max { get; set; }

    //    public int Min { get; set; }

    //    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    //    {
    //        try
    //        {

    //            int i = Convert.ToInt32(value);

    //            return (i < Min || i > Max) ?

    //                new ValidationResult(false, "int out of range") :

    //                new ValidationResult(true, null);

    //        }
    //        catch (FormatException fe)
    //        {
    //            return new ValidationResult(false, fe.Message);
    //        }
    //    }
    //}

    public class EnumExcludeSpecificConverter : IValueConverter
    {
        public string[] ExcludeStr { get; set; }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            BinType[] retval = null;

            try
            {
                if (value == null) return DependencyProperty.UnsetValue;

                retval = (BinType[])value;

                if (retval != null)
                {
                    List<BinType> list = new List<BinType>(retval);

                    foreach (var item in ExcludeStr)
                    {
                        object bintype = Enum.Parse(typeof(BinType), item);

                        list.Remove((BinType)bintype);
                    }

                    retval = list.ToArray();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    public class EnumLotStateExcludeSpecificConverter : IValueConverter
    {
        public string[] ExcludeStr { get; set; }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            LotOPStateEnum[] retval = null;

            try
            {
                if (value == null) return DependencyProperty.UnsetValue;

                retval = (LotOPStateEnum[])value;

                if (retval != null)
                {
                    List<LotOPStateEnum> list = new List<LotOPStateEnum>(retval);

                    foreach (var item in ExcludeStr)
                    {
                        object bintype = Enum.Parse(typeof(LotOPStateEnum), item);

                        list.Remove((LotOPStateEnum)bintype);
                    }

                    retval = list.ToArray();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    public class ListBoxBehavior
    {
        static readonly Dictionary<ListBox, Capture> Associations =
               new Dictionary<ListBox, Capture>();

        public static bool GetScrollOnNewItem(DependencyObject obj)
        {
            return (bool)obj.GetValue(ScrollOnNewItemProperty);
        }

        public static void SetScrollOnNewItem(DependencyObject obj, bool value)
        {
            obj.SetValue(ScrollOnNewItemProperty, value);
        }

        public static readonly DependencyProperty ScrollOnNewItemProperty =
            DependencyProperty.RegisterAttached(
                "ScrollOnNewItem",
                typeof(bool),
                typeof(ListBoxBehavior),
                new UIPropertyMetadata(false, OnScrollOnNewItemChanged));

        public static void OnScrollOnNewItemChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var listBox = d as ListBox;
            if (listBox == null) return;
            bool oldValue = (bool)e.OldValue, newValue = (bool)e.NewValue;
            if (newValue == oldValue) return;
            if (newValue)
            {
                listBox.Loaded += ListBox_Loaded;
                listBox.Unloaded += ListBox_Unloaded;
                var itemsSourcePropertyDescriptor = TypeDescriptor.GetProperties(listBox)["ItemsSource"];
                itemsSourcePropertyDescriptor.AddValueChanged(listBox, ListBox_ItemsSourceChanged);
            }
            else
            {
                listBox.Loaded -= ListBox_Loaded;
                listBox.Unloaded -= ListBox_Unloaded;
                if (Associations.ContainsKey(listBox))
                    Associations[listBox].Dispose();
                var itemsSourcePropertyDescriptor = TypeDescriptor.GetProperties(listBox)["ItemsSource"];
                itemsSourcePropertyDescriptor.RemoveValueChanged(listBox, ListBox_ItemsSourceChanged);
            }
        }

        private static void ListBox_ItemsSourceChanged(object sender, EventArgs e)
        {
            var listBox = (ListBox)sender;
            if (Associations.ContainsKey(listBox))
                Associations[listBox].Dispose();
            Associations[listBox] = new Capture(listBox);
        }

        static void ListBox_Unloaded(object sender, RoutedEventArgs e)
        {
            var listBox = (ListBox)sender;
            if (Associations.ContainsKey(listBox))
                Associations[listBox].Dispose();
            listBox.Unloaded -= ListBox_Unloaded;
        }

        static void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            var listBox = (ListBox)sender;
            var incc = listBox.Items as INotifyCollectionChanged;
            if (incc == null) return;
            listBox.Loaded -= ListBox_Loaded;
            Associations[listBox] = new Capture(listBox);
        }

        class Capture : IDisposable
        {
            private readonly ListBox listBox;
            private readonly INotifyCollectionChanged incc;

            public Capture(ListBox listBox)
            {
                this.listBox = listBox;
                incc = listBox.ItemsSource as INotifyCollectionChanged;
                if (incc != null)
                {
                    incc.CollectionChanged += incc_CollectionChanged;
                }
            }

            void incc_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                try
                {
                    if (e.Action == NotifyCollectionChangedAction.Add)
                    {
                        listBox.ScrollIntoView(e.NewItems[0]);
                        listBox.SelectedItem = e.NewItems[0];
                    }
                }
                catch (Exception err)
                {
                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                }
            }

            public void Dispose()
            {
                if (incc != null)
                    incc.CollectionChanged -= incc_CollectionChanged;
            }
        }
    }

    public static class TextBlockEx
    {
        public static Inline GetFormattedText(DependencyObject obj)
        {
            return (Inline)obj.GetValue(FormattedTextProperty);
        }

        public static void SetFormattedText(DependencyObject obj, Inline value)
        {
            obj.SetValue(FormattedTextProperty, value);
        }

        public static readonly DependencyProperty FormattedTextProperty =
            DependencyProperty.RegisterAttached(
                "FormattedText",
                typeof(Inline),
                typeof(TextBlockEx),
                new PropertyMetadata(null, OnFormattedTextChanged));

        private static void OnFormattedTextChanged(
            DependencyObject o,
            DependencyPropertyChangedEventArgs e)
        {
            var textBlock = o as TextBlock;
            if (textBlock == null) return;

            var inline = (Inline)e.NewValue;
            textBlock.Inlines.Clear();
            if (inline != null)
            {
                textBlock.Inlines.Add(inline);
            }
        }
    }

    public class SelectedSlotToFormattedTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Span retval = null;

            try
            {
                if (values[0] is ObservableCollection<SlotObject> && values[1] is EnumWaferState)
                {
                    try
                    {
                        ObservableCollection<SlotObject> inputvalue = values[0] as ObservableCollection<SlotObject>;

                        if (inputvalue == null)
                        {
                            return null;
                        }

                        retval = new Span();

                        SlotObject last = inputvalue.Last();

                        foreach (var item in inputvalue)
                        {
                            Run tmpRun = new Run();
                            tmpRun.Text = $"{item.Index + 1}";

                            if (item.WaferState == EnumWaferState.PROCESSED)
                            {
                                tmpRun.Background = Brushes.LimeGreen;
                            }
                            else
                            {
                                //tmpRun.Background = Brushes.LimeGreen;
                            }

                            retval.Inlines.Add(tmpRun);

                            if (inputvalue.Count >= 2)
                            {
                                if (item.Equals(last))
                                {
                                    // do something different with the last item
                                }
                                else
                                {
                                    retval.Inlines.Add(new Run(", "));
                                }
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //public class SelectedSlotToFormattedTextConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        Span retval = null;

    //        try
    //        {
    //            List<SlotObject> inputvalue = value as List<SlotObject>;

    //            if (inputvalue == null)
    //            {
    //                return null;
    //            }

    //            retval = new Span();

    //            SlotObject last = inputvalue.Last();

    //            foreach (var item in inputvalue)
    //            {
    //                Run tmpRun = new Run();
    //                tmpRun.Text = $"{item.Index + 1}";

    //                if (item.WaferState == EnumWaferState.PROCESSED)
    //                {
    //                    tmpRun.Background = Brushes.LimeGreen;
    //                }
    //                else
    //                {
    //                    //tmpRun.Background = Brushes.LimeGreen;
    //                }
                    
    //                retval.Inlines.Add(tmpRun);

    //                if (inputvalue.Count >= 2)
    //                {
    //                    if (inputvalue.Equals(last))
    //                    {
    //                        // do something different with the last item
    //                    }
    //                    else
    //                    {
    //                        retval.Inlines.Add(new Run(", "));
    //                    }
    //                }
    //            }
    //        }
    //        catch (Exception err)
    //        {
    //            LoggerManager.Exception(err);
    //        }

    //        // TODO : ,로 Split해서 데이터 제작해볼 것.

    //        //var span = new Span();
    //        //span.Inlines.Add(new Run("Question1: "));
    //        //span.Inlines.Add(new Run("Answer1") { FontWeight = FontWeights.Bold });
    //        //span.Inlines.Add(new Run(", "));

    //        //span.Inlines.Add(new Run("Question2: "));
    //        //span.Inlines.Add(new Run("Answer2") { FontWeight = FontWeights.Bold });
    //        //span.Inlines.Add(new Run(", "));

    //        //span.Inlines.Add(new Run("Question3: "));
    //        //span.Inlines.Add(new Run("Answer3") { FontWeight = FontWeights.Bold });

    //        return retval;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class SlotSelectedToColorConverter : IValueConverter
    {
        static SolidColorBrush LimeGreenbrush = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush DimGraybrush = new SolidColorBrush(Colors.DimGray);
        //static SolidColorBrush Yellowbrush = new SolidColorBrush(Colors.Gold);
        //static SolidColorBrush Redbrush = new SolidColorBrush(Colors.Red);
        //static SolidColorBrush DarkSlateGraybrush = new SolidColorBrush(Colors.DarkSlateGray);
        //static SolidColorBrush Cyanbrush = new SolidColorBrush(Colors.Cyan);

        public object Convert(object value, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush retval = null;

            try
            {
                bool IsSelected = (bool)value;

                if (IsSelected == false)
                {
                    retval = DimGraybrush;
                }
                else
                {
                    retval = LimeGreenbrush;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType,
                  object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class ComTypeToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool retval = false;

            try
            {
                ProberEmulCommunicationMode inputvalue = (ProberEmulCommunicationMode)value;

                if (inputvalue == ProberEmulCommunicationMode.MANUAL)
                {
                    retval = true;
                }
                else
                {
                    retval = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush retval = null;

            try
            {
                CollectionComponent inputVal = value as CollectionComponent;

                if (inputVal != null)
                {
                    retval = inputVal.Color;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //public class StringToForegroundConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        if (value != null && value.ToString().ToLower().Contains("send"))
    //            return "Red";
    //        else if (value != null && value.ToString().ToLower().Contains("receive"))
    //            return "Blue";
    //        else
    //            return "Black";
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class MachineIndexToCoordinateStringConverter : IValueConverter
    {
        public MachineIndexToCoordinateStringConverter()
        {

        }
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            string retval = string.Empty;

            try
            {
                MachineIndex inputval = value as MachineIndex;

                if (inputval != null)
                {
                    retval = $"X = {inputval.XIndex}, Y = {inputval.YIndex}";
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object retval = null;

            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }



    public class ZStateToBoolConverter : IValueConverter
    {
        public string MatchedTrueValue { get; set; }
        public ZStateToBoolConverter()
        {

        }
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            bool retval = false;

            try
            {
                string inputval = value as string;

                if (MatchedTrueValue == inputval)
                {
                    retval = true;
                }
                else
                {
                    retval = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object retval = null;

            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }


    public class ConnectedStateToBoolConverter : IValueConverter
    {
        public bool MatchedTrueValue { get; set; }
        public ConnectedStateToBoolConverter()
        {

        }
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            bool retval = false;

            try
            {
                bool inputval = (bool)value;

                if (MatchedTrueValue == inputval)
                {
                    retval = true;
                }
                else
                {
                    retval = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object retval = null;

            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    public class IPAddressToStringConverter : IValueConverter
    {
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            string retval = string.Empty;

            try
            {
                IPAddressVer4 InputVal = (IPAddressVer4)value;

                retval = InputVal.ToString();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IPAddressVer4 retval = new IPAddressVer4();

            try
            {
                //retval = value as string;

                IPAddressVer4 address = new IPAddressVer4();

                address.IP = value as string;

                //address.IP = retval;

                String[] octetSplit = address.IP.Split(new char[] { '.' });

                bool IsAvailable = true;

                if (octetSplit.Count() == 4)
                {
                    foreach (String octet in octetSplit)
                    {
                        int intOctet = 0;

                        if (int.TryParse(octet, out intOctet) == false)
                        {
                            IsAvailable = false;
                            break;
                        }

                        if (intOctet < 0 || intOctet > 255)
                        {
                            IsAvailable = false;
                            break;
                        }
                    }
                }
                else
                {
                    IsAvailable = false;
                }

                if (IsAvailable == true)
                {
                    retval = address;
                }
                else
                {
                    return value as string;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
}
