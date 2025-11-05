namespace Cognex.Controls
{


    using System.Windows.Forms.Integration;
    public static class InSightDisplayApp
    {
        public static Autofac.IContainer Container { get; set; }
        private static WindowsFormsHost _WinFormHost;
        public static void SetLoaderContainer(Autofac.IContainer container)
        {
            Container = container;
        }
        public static WindowsFormsHost Get()
        {
            if (_WinFormHost != null)
            {
                return _WinFormHost;
            }

            _WinFormHost = new WindowsFormsHost();
            _WinFormHost.Child = new CvsInSightDisplayHost();

            return _WinFormHost;
        }
        public static WindowsFormsHost GP_Get()
        {
            if (_WinFormHost != null)
            {
                return _WinFormHost;
            }

            _WinFormHost = new WindowsFormsHost();
            _WinFormHost.Child = new CvsInSightDisplayHost(Container);

            return _WinFormHost;
        }
    }
}
