using Autofac;
using LogModule;
using ProberInterfaces.Temperature;
using System;
using System.Windows.Controls;

namespace PopupControlElementControl
{
    public partial class ChillerModuleInfoList : UserControl
    {
        protected Autofac.IContainer Container { get; set; }
        public ITempController TempController 
        {
            get 
            { 
                if(Container.IsRegistered<ITempController>() == true)
                {
                    return Container.Resolve<ITempController>();
                }
                else
                {
                    return null;
                }
            }
        }

        public ChillerModuleInfoList()
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
