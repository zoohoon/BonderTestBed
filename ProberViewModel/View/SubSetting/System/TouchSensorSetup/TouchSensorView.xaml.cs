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

namespace UcTouchSensorView
{
    public partial class TouchSensorView : UserControl, IMainScreenView
    {
        public TouchSensorView()
        {
            InitializeComponent();
        }

        private readonly Guid _ViewGUID = new Guid("9FF1993E-2306-4894-A1A1-271317338E41");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }
}
