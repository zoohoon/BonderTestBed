using System;
using System.Windows.Controls;

namespace WizardEditorView
{
    using ProberInterfaces;
    /// <summary>
    /// Interaction logic for WizardEditor.xaml
    /// </summary>
    public partial class WizardEditor : UserControl, IMainScreenView
    {
        public WizardEditor()
        {
            InitializeComponent();
        }

        readonly Guid _ViewGUID = new Guid("770BEFDB-6D8F-054C-DFBD-BF3E77591032");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }
}
