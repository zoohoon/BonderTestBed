using System;
using System.Windows.Controls;

namespace SequenceMakerScreen
{
    using ProberInterfaces;

    /// <summary>
    /// Interaction logic for SequenceMaker.xaml
    /// </summary>
    public partial class SequenceMaker : UserControl, IMainScreenView
    {
        public SequenceMaker()
        {
            InitializeComponent();
        }
        
        readonly Guid _ViewGUID = new Guid("EC8FB998-222F-1E88-2C18-6DF6A742B3E9");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }
}
