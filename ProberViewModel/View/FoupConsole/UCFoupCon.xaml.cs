using System;
using System.Windows.Controls;
using ProberInterfaces;


namespace UcFoupConsole
{
    /// <summary>
    /// UCFoupCon.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UCFoupCon : UserControl,IMainScreenView
    {
        public UCFoupCon()
        {
            InitializeComponent();
        }
        public Guid ScreenGUID => throw new NotImplementedException();
    }
}
