using LoaderBase;
using LogModule;
using MahApps.Metro.Controls;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MultiManualContactTransferDiaglog
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow, IFactoryModule
    {
        public MainWindow()
        {
            InitializeComponent();
            PreviewKeyDown += MainWindow_PreviewKeyDown;
        }

        public MainWindow(ILoaderModule loader)
        {
            InitializeComponent();
            PreviewKeyDown += MainWindow_PreviewKeyDown;
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                        Keyboard.IsKeyDown(Key.K) &&
                        Keyboard.IsKeyDown(Key.F))
                {
                    this.Close();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
