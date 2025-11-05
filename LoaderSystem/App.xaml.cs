using SciChart.Charting.Visuals;
using Sentinel;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace LoaderSystem
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            //SciChartSurface.SetRuntimeLicenseKey(@"<LicenseContract>
            //      <Customer>Semics</Customer>
            //    		<OrderId>ABT170210-6470-10116</OrderId>
            //      <LicenseCount>1</LicenseCount>
            //      <IsTrialLicense>false</IsTrialLicense>
            //      <SupportExpires>02/14/2018 00:00:00</SupportExpires>
            //    		<ProductCode>SC-WPF-SDK-ENTERPRISE-SRC</ProductCode>
            //      <KeyCode>lwAAAAEAAADhBXfYVYbSAXIAQ3VzdG9tZXI9U2VtaWNzO09yZGVySWQ9QUJUMTcwMjEwLTY0NzAtMTAxMTY7U3Vic2NyaXB0aW9uVmFsaWRUbz0xNC1GZWItMjAxODtQcm9kdWN0Q29kZT1TQy1XUEYtU0RLLUVOVEVSUFJJU0UtU1JDBZgVJU5KmSVijO7q1jBhuS3EGgk466C35UnSYrE0Xg4qPwB30zjXelOpldzc1Naq</KeyCode>
            //  </LicenseContract>");
            SciChartSurface.SetRuntimeLicenseKey(@"<LicenseContract>
                <Customer>Semics</Customer>
                <OrderId>ABT190823-9438-44125</OrderId>
                <LicenseCount>1</LicenseCount>
                <IsTrialLicense>false</IsTrialLicense>
                <SupportExpires>08/22/2020 00:00:00</SupportExpires>
                <ProductCode>SC-WPF-2D-ENTERPRISE-SRC</ProductCode>
                <KeyCode>lwAAAQEAAAA4sZ8VwFnVAXEAQ3VzdG9tZXI9U2VtaWNzO09yZGVySWQ9QUJUMTkwODIzLTk0MzgtNDQxMjU7U3Vic2NyaXB0aW9uVmFsaWRUbz0yMi1BdWctMjAyMDtQcm9kdWN0Q29kZT1TQy1XUEYtMkQtRU5URVJQUklTRS1TUkMestJKxT7JacnNzG+RIIi8Gbc78LhqY0Jf/G1UbPjQ+2O9sxKq6fxli7uHDjo2J0E=</KeyCode>
                </LicenseContract>");

        }
        Mutex mutex;
        protected override void OnStartup(StartupEventArgs e)
        {
            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

            int VerifyResult = Sentinel.Sentinel.VerifyEncryption();
            if(VerifyResult < 1)
            {
                MessageBox.Show($"The program can't running. [{assemblyName}]\nIt failed to authenticate with Sentinel.", "Warning");
                Application.Current.Shutdown();
                System.Environment.Exit(0);
                return;
            }
            
            base.OnStartup(e);

            string mutexName = "Maestro";
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
