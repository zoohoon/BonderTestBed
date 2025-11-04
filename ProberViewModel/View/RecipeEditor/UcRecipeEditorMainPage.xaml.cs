using System;
using System.Windows.Controls;

namespace ProberViewModel
{
    using ProberInterfaces;
    /// <summary>
    /// UcRecipeEditorMainPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcRecipeEditorMainPage : UserControl, IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("E9A65DCF-90D7-421C-8174-6808D4466BAE");
        public Guid ScreenGUID { get { return _ViewGUID; } }
        public UcRecipeEditorMainPage()
        {
            InitializeComponent();
        }
    }
}
