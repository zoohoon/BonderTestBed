using System.Windows;

namespace SecsGemServiceHostApp
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);


            if (e.Args.Length > 0)
            {
                var StartupGemType = e.Args[0];

                MainWindow mainWindow = new MainWindow();
                mainWindow.StartupGemType = StartupGemType;
                mainWindow.Title = StartupGemType;
                mainWindow.Show();
            }
            else
            {
                //None.
            }
        }
    }
}
