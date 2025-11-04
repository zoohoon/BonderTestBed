using Autofac;
using LogModule;
using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace PopupControlElementControl
{
    public partial class GEMModuleInfoList : UserControl
    {
        protected Autofac.IContainer Container { get; set; }
        public IGEMModule GEMModule
        {
            get 
            {
                if (Container.IsRegistered<IGEMModule>() == true)
                {
                    return Container.Resolve<IGEMModule>();
                }
                else
                {
                    return null;
                }
            }
        }

        public GEMModuleInfoList()
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
