using System.Reflection;
using System.Threading;
using System.Windows;

namespace MultiExecuter
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        Mutex mutex;
        protected override void OnStartup(StartupEventArgs e)
        {
            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

            base.OnStartup(e);

            string mutexName = "MultiExecuter";
            bool createNew;

            mutex = new Mutex(true, mutexName, out createNew);

            if (!createNew)
            {
                MessageBox.Show($"The program is already running. [{assemblyName}]", "Warning");
                Application.Current.Shutdown();
                System.Environment.Exit(0);
            }
        }
    }
    
}
