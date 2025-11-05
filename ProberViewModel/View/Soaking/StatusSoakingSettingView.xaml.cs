using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace StatusSoakingRecipeSettingView
{

    public partial class StatusSoakingSettingView : UserControl, IMainScreenView
    {
        public StatusSoakingSettingView()
        {
            InitializeComponent();
        }

        private Guid _ViewGUID = new Guid("DA7462FF-9C5B-4E8A-B8C3-DAD82523A9F6");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }
}
