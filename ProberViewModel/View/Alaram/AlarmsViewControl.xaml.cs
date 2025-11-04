using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace AlarmsViewControl
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class AlarmsViewControl : UserControl, IMainScreenView
    {
        public AlarmsViewControl()
        {
            InitializeComponent();
        }

        private readonly Guid _ViewGUID = new Guid("65ffcd05-b8f8-4e2b-81e5-25af0409fb75");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }
}
