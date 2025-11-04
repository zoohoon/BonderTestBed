using LogModule;
using System;
using System.Windows.Controls;

namespace PopupControlElementControl
{
    public partial class GPIBModuleInfoList : UserControl
    {
        protected Autofac.IContainer Container { get; set; }

        public GPIBModuleInfoList()
        {
            try
            {
                InitializeComponent();
                DataContext = this;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public virtual void SetContainer(Autofac.IContainer container)
        {
            this.Container = container;
        }
    }
}
