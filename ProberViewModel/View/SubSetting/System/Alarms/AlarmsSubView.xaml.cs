using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace AlarmsSubView
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class AlarmsSubView : UserControl, IMainScreenView
    {
        public AlarmsSubView()
        {
            InitializeComponent();
        }

        private readonly Guid _ViewGUID = new Guid("0445a899-5d0d-4881-9388-ff215e5b3baf");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }
}
