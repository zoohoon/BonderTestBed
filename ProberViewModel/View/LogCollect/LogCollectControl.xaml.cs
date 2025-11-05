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

namespace ProberViewModel.View.LogCollect
{
    /// <summary>
    /// LogCollectControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LogCollectControl : UserControl, IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("da1adafa-0d52-401b-937a-6b1ba5cae886");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        public LogCollectControl()
        {
            InitializeComponent();
        }
    }
}
