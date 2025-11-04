using System;

namespace SoftArcs.WPFSmartLibrary.SmartUserControls
{
    using LogModule;
    using System.Windows;
    using System.Windows.Controls;
    public static class TextBoxHelper
    {
        public static string GetSelectedText(DependencyObject obj)
        {
            return (string)obj.GetValue(SelectedTextProperty);
        }
        public static void SetSelectedText(DependencyObject obj, string value)
        {
            obj.SetValue(SelectedTextProperty, value);
        }
        public static readonly DependencyProperty SelectedTextProperty =
            DependencyProperty.RegisterAttached("SelectedText",
                typeof(string),
                typeof(TextBoxHelper),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SelectedTextChanged));
        private static void SelectedTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                TextBox tb = obj as TextBox;
                if (tb != null)
                {
                    if (e.OldValue == null && e.NewValue != null)
                    {
                        tb.SelectionChanged += tb_SelectionChanged;
                    }
                    else if (e.OldValue != null && e.NewValue == null)
                    {
                        tb.SelectionChanged -= tb_SelectionChanged;
                    }

                    string newValue = e.NewValue as string;

                    if (newValue != null && newValue != tb.SelectedText)
                    {
                        tb.SelectedText = newValue as string;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        static void tb_SelectionChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBox tb = sender as TextBox;
                if (tb != null)
                {
                    SetSelectedText(tb, tb.SelectedText);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
