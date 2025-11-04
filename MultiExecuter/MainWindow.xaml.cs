using System.Diagnostics;
using System.Windows;

namespace MultiExecuter
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        Process[] processList = Process.GetProcesses();
        public MainWindow()
        {
            int cnt = 0;
                InitializeComponent();
            foreach (Process process in processList)
            {
                if (process.ProcessName.Equals("MultiExecuter"))
                {
                
                        cnt++;
                    if (cnt > 1)
                    {
                        Window.GetWindow(this).Close();
                    }
                }
            }
        }
    }
}
