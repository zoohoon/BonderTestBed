using LogModule;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace VirtualKeyboardControl
{
    /// <summary>
    /// UcKey.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcKey : UserControl
    {
        #region ==> DEP KeyText
        public static readonly DependencyProperty KeyTextProperty = DependencyProperty.Register(nameof(KeyText), typeof(String), typeof(UcKey), new FrameworkPropertyMetadata(default(String)));
        public String KeyText
        {
            get { return (String)this.GetValue(KeyTextProperty); }
            set { this.SetValue(KeyTextProperty, value); }
        }
        #endregion

        #region ==> DEP CaptionSize
        //public static readonly DependencyProperty CaptionSizeProperty =
        //    DependencyProperty.Register(nameof(CaptionSize)
        //        , typeof(double),
        //        typeof(UcKey),
        //        new FrameworkPropertyMetadata(default(double)));
        public static readonly DependencyProperty CaptionSizeProperty = DependencyProperty.Register(nameof(CaptionSize), typeof(double), typeof(UcKey), new FrameworkPropertyMetadata(24D));
        public double CaptionSize
        {
            get { return (double)this.GetValue(CaptionSizeProperty); }
            set { this.SetValue(CaptionSizeProperty, value); }
        }
        #endregion

        public UcKey()
        {
            try
            {
                InitializeComponent();

                Binding bindKeyText = new Binding();
                bindKeyText.Path = new PropertyPath(nameof(KeyText));
                bindKeyText.Source = this;
                BindingOperations.SetBinding(keyTextBlock, TextBlock.TextProperty, bindKeyText);

                Binding bindCaptionSize = new Binding();
                bindCaptionSize.Path = new PropertyPath(nameof(CaptionSize));
                bindCaptionSize.Source = this;
                BindingOperations.SetBinding(keyTextBlock, TextBlock.FontSizeProperty, bindCaptionSize);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void keyBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {

                keyBorder.Opacity = 0.5;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void keyBorder_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                keyBorder.Opacity = 1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void keyBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                keyBorder.Opacity = 1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
