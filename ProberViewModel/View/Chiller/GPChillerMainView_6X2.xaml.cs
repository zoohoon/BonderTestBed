using System;
using System.Windows.Controls;

namespace ProberViewModel.View.Chiller
{
    using ProberInterfaces;

    /// <summary>
    /// Interaction logic for GPChillerMainView.xaml
    /// </summary>
    public partial class GPChillerMainView_6X2 : UserControl, IMainScreenView
    {
        public GPChillerMainView_6X2()
        {
            InitializeComponent();
        }
        private Autofac.IContainer Container { get; set; }

        readonly Guid _ViewGUID = new Guid("3817ed23-c61a-47a1-8e97-6fc2c3a210c4");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }
}
