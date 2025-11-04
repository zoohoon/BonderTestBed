using System;
using System.Windows.Controls;

namespace UcWaferRecipeSettingView
{
    using ProberInterfaces;

    public partial class WaferRecipeSettingView : UserControl, IMainScreenView
    {
        public WaferRecipeSettingView()
        {
            InitializeComponent();
        }

        private readonly Guid _ViewGUID = new Guid("A0005054-423A-F131-4B41-29B0091C34C0");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }


    }
}
