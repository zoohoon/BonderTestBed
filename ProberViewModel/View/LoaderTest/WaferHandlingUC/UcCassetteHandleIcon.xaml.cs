using LogModule;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace LoaderTestMainPageView.WaferHandlingUC
{
    /// <summary>
    /// UcCassetteHandleIcon.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcCassetteHandleIcon : UserControl
    {
        #region ==> DEP SlotNum
        public static readonly DependencyProperty SlotNumProperty =
            DependencyProperty.Register(nameof(SlotNum)
                , typeof(String),
                typeof(UcCassetteHandleIcon),
                new FrameworkPropertyMetadata(null));
        public String SlotNum
        {
            get { return (String)this.GetValue(SlotNumProperty); }
            set { this.SetValue(SlotNumProperty, value); }
        }
        #endregion

        public UcCassetteHandleIcon()
        {
            try
            {
            InitializeComponent();

            Binding bindBtnText = new Binding();
            bindBtnText.Path = new PropertyPath(nameof(SlotNum));
            bindBtnText.Source = this;
            bindBtnText.StringFormat = "#{0}";
            BindingOperations.SetBinding(slotNumTextBlock, TextBlock.TextProperty, bindBtnText);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        private void icon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            icon.Opacity = 0.5;
        }

        private void icon_MouseUp(object sender, MouseButtonEventArgs e)
        {
            icon.Opacity = 1;
        }

        private void icon_MouseLeave(object sender, MouseEventArgs e)
        {
            icon.Opacity = 1;
        }
    }
}
