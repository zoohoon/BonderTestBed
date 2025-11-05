using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;



namespace SoakingSettingView
{
    /// <summary>
    /// UcSoakingTemplateSaveList.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcSoakingTemplateSaveList : UserControl
    {
        public object ItemsSource
        {
            get => GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                "ItemsSource",
                typeof(object),
                typeof(UcSoakingTemplateSaveList));


        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public static readonly DependencyProperty SelectedItemProperty =
           DependencyProperty.Register(
               "SelectedItem",
               typeof(object),
               typeof(UcSoakingTemplateSaveList));


        
        public static readonly DependencyProperty ItemRemoveProperty =
            DependencyProperty.Register(
                "ItemRemove",
                typeof(ICommand),
                typeof(UcSoakingTemplateSaveList),
                new UIPropertyMetadata(null));
        public ICommand ItemRemove
        {
            get { return (ICommand)GetValue(ItemRemoveProperty); }
            set { SetValue(ItemRemoveProperty, value); }
        }

        public static readonly DependencyProperty ItemCopyProperty =
            DependencyProperty.Register(
                "ItemCopy",
                typeof(ICommand),
                typeof(UcSoakingTemplateSaveList),
                new UIPropertyMetadata(null));

        public ICommand ItemCopy
        {
            get { return (ICommand)GetValue(ItemCopyProperty); }
            set { SetValue(ItemCopyProperty, value); }
        }


        public static readonly DependencyProperty ItemApplyProperty =
           DependencyProperty.Register(
               "ItemApply",
               typeof(ICommand),
               typeof(UcSoakingTemplateSaveList),
               new UIPropertyMetadata(null));

        public ICommand ItemApply
        {
            get { return (ICommand)GetValue(ItemApplyProperty); }
            set { SetValue(ItemApplyProperty, value); }
        }

        public UcSoakingTemplateSaveList() => InitializeComponent();



    }

    #region Value Converter
    public class ValueRatioConverter : MarkupExtension, IValueConverter
    {
        private static ValueRatioConverter _instance;

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter.ToString().EndsWith("%"))
            {
                double.TryParse(parameter.ToString().TrimEnd('%'), out double paramDouble);
                return System.Convert.ToDouble(value) * paramDouble/100;
            }

            return System.Convert.ToDouble(value) * System.Convert.ToDouble(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new ValueRatioConverter());
        }
    }
    public class IndexConverter : IValueConverter
    {
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            ListViewItem item = (ListViewItem)value;
            ListView listView = ItemsControl.ItemsControlFromItemContainer(item) as ListView;
            int index = listView.ItemContainerGenerator.IndexFromContainer(item);
            
            //마지막 아이템 구분용 -1 표시
            if (listView.Items.Count - 1 == index)
                return -1;

            //인덱스 1부터 시작으로 조정.
            return (index+1).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class TextLineConverter : MarkupExtension, IValueConverter
    {
        private static TextLineConverter _instance;

        #region IValueConverter Members

        const int LineHeight = 13;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int lineCount = 1; // 컨텐츠가 없어도 기본 높이 
            
            if (!string.IsNullOrWhiteSpace(value?.ToString()))
            {
                lineCount = value.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Length;
            }
            return (lineCount * LineHeight) + System.Convert.ToDouble(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new TextLineConverter());
        }
    }

    #endregion


}
