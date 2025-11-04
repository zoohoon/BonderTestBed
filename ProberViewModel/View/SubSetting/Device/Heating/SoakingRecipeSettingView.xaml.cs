using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace SoakingRecipeSettingView
{
    /// <summary>
    /// SoakingRecipeSettingView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SoakingRecipeSettingView : UserControl, IMainScreenView
    {
        public SoakingRecipeSettingView()
        {
            InitializeComponent();
        }
        private readonly Guid _ViewGUID = new Guid("00425A3B-902C-42BB-9501-9AA58952E57B");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }
}
