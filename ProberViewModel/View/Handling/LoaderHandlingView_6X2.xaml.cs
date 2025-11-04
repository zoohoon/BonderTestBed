using System;
using System.Windows.Controls;

namespace ProberViewModel
{
    using ProberInterfaces;
    /// <summary>
    /// LoaderHandlingView_DRAX.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoaderHandlingView_6X2 : UserControl, IMainScreenView
    {

        readonly Guid _ViewGUID = new Guid("758ceac2-5962-4810-a8ab-7719d2b12f0c");
        public Guid ScreenGUID { get { return _ViewGUID; } }
        public LoaderHandlingView_6X2()
        {
            InitializeComponent();
        }

     
    }
}
