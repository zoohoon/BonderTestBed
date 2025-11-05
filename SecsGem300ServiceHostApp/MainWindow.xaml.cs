using SecsGemServiceInterface;
using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Windows;
using System.Windows.Forms;
using WcfSecsGemService_XGem300;
using XGEMWrapper;

namespace SecsGem300ServiceHostApp
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        protected ServiceHost host = null;
        public System.Windows.Forms.NotifyIcon notify;
        public MainWindow()
        {
            try
            {
                InitializeComponent();

                Process[] p = Process.GetProcessesByName("XGem");

                if (0 < p.GetLength(0))
                    p[0].Kill();

                p = Process.GetProcessesByName("SecsGem300ServiceHostApp");
                if (1 < p.Length)
                {
                    TimeSpan bigTimeSpan = new TimeSpan(0);
                    int bigTimeIdx = 0;
                    for (int i = 0; i < p.Length; i++)
                    {
                        if (bigTimeSpan < p[i].TotalProcessorTime)
                        {
                            bigTimeSpan = p[i].TotalProcessorTime;
                            bigTimeIdx = i;
                        }
                    }

                    p[bigTimeIdx].Kill();
                }
            }
            catch (Exception )
            {

            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string localURI = "net.pipe://localhost/secsgempipe";
                ServiceMetadataBehavior serviceMetadataBehavior = null;
                ServiceDebugBehavior debugBehavior = null;

                //Client와 맞아야함.
                var netNamedPipeBinding = new NetNamedPipeBinding()
                {
                    MaxBufferPoolSize = 2147483647,
                    MaxBufferSize = 2147483647,
                    MaxReceivedMessageSize = 2147483647,
                    SendTimeout = TimeSpan.MaxValue,
                    ReceiveTimeout = TimeSpan.MaxValue
                };

                this.host = new ServiceHost(typeof(SecsGem300Service));
                this.host.AddServiceEndpoint(typeof(ISecsGem300ProService), netNamedPipeBinding, localURI);

                debugBehavior = this.host.Description.Behaviors.Find<ServiceDebugBehavior>();
                if (debugBehavior != null)
                {
                    debugBehavior.IncludeExceptionDetailInFaults = true;
                }

                serviceMetadataBehavior = this.host.Description.Behaviors.Find<ServiceMetadataBehavior>();
                // If not, add one
                if (serviceMetadataBehavior == null)
                    serviceMetadataBehavior = new ServiceMetadataBehavior();

                serviceMetadataBehavior.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                this.host.Description.Behaviors.Add(serviceMetadataBehavior);

                this.host.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName,
                    MetadataExchangeBindings.CreateMexNamedPipeBinding(),
                    $"{localURI}/mex"
                    );

                this.host.Faulted += HostRestart;
                this.host.Open();

                #region =>> Notify Icon (Not Used)
                //notify = new System.Windows.Forms.NotifyIcon();
                //notify.Icon = SecsGemServiceHostApp.Properties.Resources.GEMIcon;
                //notify.Visible = true;

                //notify.BalloonTipText = "GEM WCF Service";

                //notify.MouseClick += notifyMouseClick;

                //notify.DoubleClick += delegate
                //{
                //    this.Show();
                //    this.WindowState = WindowState.Normal;
                //};
                #endregion

                this.Hide();
            }
            catch (Exception)
            {
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.host.Faulted -= HostRestart;
            this.host.Close();
            this.notify.Visible = false;
        }

        private void notifyMouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.notify.ShowBalloonTip(1);
            }
        }

        private void HostRestart(object sender, EventArgs e)
        {
            this.host.Close();
            this.host.Open();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            switch (this.WindowState)
            {
                case WindowState.Maximized:
                    break;
                case WindowState.Minimized:
                    this.Hide();
                    break;
                case WindowState.Normal:

                    break;
            }
        }
    }
}
