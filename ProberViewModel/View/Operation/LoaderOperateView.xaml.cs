using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;
using System.Linq;
using System.Text;
using ProberInterfaces;


namespace ProberViewModel
{

    /// <summary>
    /// LoaderOperateView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoaderOperateView : UserControl, IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("4732F634-2292-6228-C7E5-24A18C888187");
        public Guid ScreenGUID { get { return _ViewGUID; } }
        public LoaderOperateView()
        {
            InitializeComponent();
        }
    }

}

