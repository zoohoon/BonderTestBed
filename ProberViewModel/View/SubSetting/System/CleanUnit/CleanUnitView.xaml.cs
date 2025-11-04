using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace UcCleanUnitView
{
    public partial class CleanUnitView : UserControl, IMainScreenView
    {
        public CleanUnitView()
        {
            InitializeComponent();
        }

        private readonly Guid _ViewGUID = new Guid("89829dc2-e884-4afb-a7d0-ca7576834a18");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }
}
