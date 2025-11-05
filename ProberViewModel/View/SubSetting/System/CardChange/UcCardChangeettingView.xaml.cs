using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProberViewModel.View
{
    /// <summary>
    /// UcCardChangeettingView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcCardChangeSettingView : UserControl, IMainScreenView, IFactoryModule
    {
        public UcCardChangeSettingView()
        {
            InitializeComponent();
        }

        readonly Guid _ViewGUID = new Guid("444b1280-Fc75-4932-9b63-D055df8c91c9");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }
}
