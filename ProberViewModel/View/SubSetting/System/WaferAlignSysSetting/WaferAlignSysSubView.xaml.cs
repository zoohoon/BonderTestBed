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
    /// WaferAlignSysSubView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WaferAlignSysSubView : UserControl, IMainScreenView, IFactoryModule
    {
        public WaferAlignSysSubView()
        {
            InitializeComponent();
        }

        readonly Guid _ViewGUID = new Guid("DA742EF8-7AA4-41BE-BA65-81F53166213C");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }
}
