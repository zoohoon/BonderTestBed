using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace ChuckPlanarityView
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChuckPlanarityView : UserControl, IMainScreenView
    {
        private Guid _ViewGUID = new Guid("d3b97c85-2bee-4fd8-834f-bb4c3401752a");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        //private Canvas ChuckPlanarityViewCanvas;

        public ChuckPlanarityView()
        {
            InitializeComponent();
        }

        //private void ChuckPlanarityCanvas_Loaded(object sender, RoutedEventArgs e)
        //{
        //    ChuckPlanarityViewCanvas = sender as Canvas;
        //}

        //public Canvas GetCanvas()
        //{
        //    return ChuckPlanarityViewCanvas;
        //}
    }
}
