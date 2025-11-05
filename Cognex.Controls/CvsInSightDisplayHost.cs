using System;
using System.Windows.Forms;

namespace Cognex.Controls
{
    using ProberInterfaces;
    using LoaderBase.AttachModules.ModuleInterfaces;
    using Autofac;

    public partial class CvsInSightDisplayHost : UserControl, IFactoryModule
    {
        private Autofac.IContainer _Container;
        private ICognexProcessManager _CognexProcessManager;

        public CvsInSightDisplayHost()
        {
            InitializeComponent();

            _Container = this.LoaderController()?.GetLoaderContainer();
            _CognexProcessManager = _Container?.Resolve<ICognexProcessManager>();
            _CognexProcessManager?.InitWindow(this.Handle);
        }
        public CvsInSightDisplayHost(Autofac.IContainer container)
        {
            InitializeComponent();
            _Container = container;

            _CognexProcessManager = _Container?.Resolve<ICognexProcessManager>();
            _CognexProcessManager?.InitWindow(this.Handle);
        }
        protected override void WndProc(ref Message m)
        {
            _CognexProcessManager?.WndProc(m.Msg, m.LParam);
            base.WndProc(ref m);
        }
        protected override void OnSizeChanged(EventArgs e)
        {
            _CognexProcessManager?.OnSizeChanged(e, this.ClientSize.Width, this.ClientSize.Height);
            base.OnSizeChanged(e);
        }
    }
}