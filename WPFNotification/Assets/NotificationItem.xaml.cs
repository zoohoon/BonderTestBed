using LogModule;
using System;
using System.Windows;
using System.Windows.Controls;

namespace WPFNotification.Assets
{
    /// <summary>
    /// Interaction logic for NotificationItem.xaml
    /// </summary>
    public partial class NotificationItem : UserControl
    {
        public NotificationItem()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Window parentWindow = Window.GetWindow(this);
                this.Visibility = Visibility.Hidden;
                parentWindow.Close();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
