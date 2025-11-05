using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace LotInfoRecipeControl
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LotInfoRecipeView : UserControl, IMainScreenView
    {
        public LotInfoRecipeView()
        {
            InitializeComponent();
        }

        readonly Guid _ViewGUID = new Guid("7c94444f-d655-407b-9a7e-0b938505eb99");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }
}
