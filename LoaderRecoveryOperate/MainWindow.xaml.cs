using Autofac;
using LoaderBase;
using LoaderCore;
using LogModule;
using ProberInterfaces.Foup;
using System;
using System.Windows;

namespace LoaderRecoveryOperate
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public bool IsCheck = false;
        LoaderRecoveryOperateVM vm = null;
        public MainWindow()
        {
            InitializeComponent();
            vm = new LoaderRecoveryOperateVM();
            this.DataContext = vm;
        }
        public ILoaderModule loaderModule = null;

        public ILoaderSupervisor LoaderMaster = null;

        private static Autofac.IContainer LoaderContainer = null;
        public MainWindow(Autofac.IContainer container) : this()
        {
            try
            {
                LoaderContainer = container;
                //     lbModuleName.Content = ModuleName;
                loaderModule = LoaderContainer.Resolve<ILoaderModule>();
                LoaderMaster = LoaderContainer.Resolve<ILoaderSupervisor>();
                loaderModule.ResetUnknownModule();
                Loaded += ToolWindow_Loaded;
                WindowStartupLocation = WindowStartupLocation.Manual;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        void ToolWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Code to remove close box from window
                var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
