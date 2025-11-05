using System;
using System.Windows.Controls;

namespace WizardTopBarView
{
    using ProberInterfaces;
    /// <summary>
    /// Interaction logic for WizardTopBarControl.xaml
    /// </summary>
    //public partial class WizardTopBarControl : UserControl, IMainTopBarView
    public partial class WizardTopBarControl : UserControl, IMainScreenView
    {
        public WizardTopBarControl()
        {
            InitializeComponent();
        }
        readonly Guid _ViewGUID = new Guid("BBB84A5E-87DB-9A15-2D4D-72FDE8A584B2");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }
}
