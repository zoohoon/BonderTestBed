using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace UcPolishWaferRecipeSettingView
{
    using LogModule;
    using MetroDialogInterfaces;
    using PolishWaferParameters;
    using ProberInterfaces;
    using ProberInterfaces.PolishWafer;
    using System.Globalization;
    using System.Windows.Interactivity;

    /// <summary>
    /// PolishWaferRecipeSettingView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PolishWaferRecipeSettingView : UserControl, IMainScreenView
    {
        private readonly Guid _ViewGUID = new Guid("9C13D639-847B-233D-2D69-69149A78F310");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }

        public PolishWaferRecipeSettingView()
        {
            InitializeComponent();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var comboBox = sender as ComboBox;

                if (comboBox != null)
                {
                    if(comboBox.Items.Count > 0)
                    {
                        var currentItem = (EnumCleaningTriggerMode)comboBox.SelectedItem;

                        if(treeview.Items.Count > 0 && e.RemovedItems.Count > 0)
                        {
                            var intervals = treeview.Items.Cast<PolishWaferIntervalParameter>().ToList();

                            var lotstartdatas = intervals.FindAll(x => x.CleaningTriggerMode.Value == EnumCleaningTriggerMode.LOT_START);

                            if (lotstartdatas != null && lotstartdatas.Count >= 2)
                            {
                                comboBox.SelectionChanged -= ComboBox_SelectionChanged;
                                comboBox.SelectedItem = e.RemovedItems[0];
                                comboBox.SelectionChanged += ComboBox_SelectionChanged;

                                e.Handled = true;

                                this.MetroDialogManager().ShowMessageDialog("[Invalid]", "Already LOT_START exist.", EnumMessageStyle.Affirmative);
                            }
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

    public class BindableSelectedItemBehavior : Behavior<TreeView>
    {
        #region SelectedItem Property

        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(BindableSelectedItemBehavior), new UIPropertyMetadata(null, OnSelectedItemChanged));

        private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var item = e.NewValue as TreeViewItem;
            if (item != null)
            {
                item.SetValue(TreeViewItem.IsSelectedProperty, true);
            }
        }

        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
            }
        }

        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            this.SelectedItem = e.NewValue;
        }
    }

    public class CommonMethod
    {
        public static int GetSourceIndexUsingTreeViewItem(object value)
        {
            int retval = -1;

            TreeViewItem item = value as TreeViewItem;

            if (item != null)
            {
                TreeView treeView = ItemsControl.ItemsControlFromItemContainer(item) as TreeView;
                retval = treeView.ItemContainerGenerator.IndexFromContainer(item);
            }

            return retval;
        }
    }

    public class VirtualKeyBoardTextBoxConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            TextBox textBox = null;
            IElement element = null;
            if (values[0] != DependencyProperty.UnsetValue)
                textBox = (System.Windows.Controls.TextBox)values[0];
            if (values[1] != DependencyProperty.UnsetValue)
                element = (IElement)values[1];

            return (textBox, element);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class NameAndIndexConverter : IValueConverter
    {
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            string retval = null;

            int index = (int)value;

            index = index + 1;

            if (parameter != null)
            {
                retval = parameter.ToString() + " " + index.ToString();
            }
            else
            {
                retval = "Unknown " + index.ToString();
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TreeViewIndexForUIConverter : IValueConverter
    {
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            int index;

            index = CommonMethod.GetSourceIndexUsingTreeViewItem(value);
            index = index + 1;

            string retval = null;

            if (parameter != null)
            {
                retval = parameter.ToString() + " " + index.ToString();
            }
            else
            {
                retval = "Unknown " + index.ToString();
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class TreeViewIndexConverter : IValueConverter
    {
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            int index;

            index = CommonMethod.GetSourceIndexUsingTreeViewItem(value);

            return index;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ListViewIndexConverter : IValueConverter
    {
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            ListBoxItem item = (ListBoxItem)value;
            ListBox listView = ItemsControl.ItemsControlFromItemContainer(item) as ListBox;
            int index = listView.ItemContainerGenerator.IndexFromContainer(item);
            return index.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ItemToIndexConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var itemCollection = value[0] as ItemCollection;
            var item = value[1] as PolishWaferIntervalParameter;

            return itemCollection.IndexOf(item);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CustomConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type Target_Type, object Parameter, CultureInfo culture)
        {
            var findCommandParameters = new PolishWaferDefineModel();

            findCommandParameters.DefineName = values[0] as string;
            findCommandParameters.IntervalIndex = (int)values[1];
            findCommandParameters.CleaningIndex = (int)values[2];

            return findCommandParameters;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CustomConverter2 : IMultiValueConverter
    {
        public object Convert(object[] values, Type Target_Type, object Parameter, CultureInfo culture)
        {
            var findCommandParameters = new PolishWaferRecipeSettingModel2();

            findCommandParameters.IntervalParamIndex = CommonMethod.GetSourceIndexUsingTreeViewItem(values[0]);
            findCommandParameters.TextBox = values[1];

            return findCommandParameters;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CustomConverter3 : IMultiValueConverter
    {
        public object Convert(object[] values, Type Target_Type, object Parameter, CultureInfo culture)
        {
            var findCommandParameters = new PolishWaferIndexModel();

            findCommandParameters.IntervalIndex = (int)values[0];
            findCommandParameters.CleaningIndex = (int)values[1];

            return findCommandParameters;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }



    public class IntervalModeToVisibilityConverter : IValueConverter
    {
        public EnumCleaningTriggerMode VisibilityMode { get; set; }

        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            Visibility retval = Visibility.Collapsed;

            try
            {
                EnumCleaningTriggerMode InputValue = (EnumCleaningTriggerMode)value;

                if (InputValue == VisibilityMode)
                {
                    retval = Visibility.Visible;
                }
                else
                {
                    retval = Visibility.Collapsed;
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

    public class EnumExcludeTriggerModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            EnumCleaningTriggerMode[] retval = null;

            try
            {
                if (value == null) return DependencyProperty.UnsetValue;

                retval = (EnumCleaningTriggerMode[])value;

                if (retval != null)
                {
                    List<EnumCleaningTriggerMode> listofTriggerMode = new List<EnumCleaningTriggerMode>(retval);

                    listofTriggerMode.Remove(EnumCleaningTriggerMode.UNDEFIEND);
                    listofTriggerMode.Remove(EnumCleaningTriggerMode.LOT_END);

                    retval = listofTriggerMode.ToArray();
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

}
