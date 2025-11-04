using System.Windows.Controls;
//using SciChart.Examples.ExternalDependencies.Data;

namespace TempGraphControl
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TempGraph : UserControl
    {
        public TempGraph()
        {
            InitializeComponent();
        }

      
        //private int dataIndex;

        //Color[] graphColors = new Color[] { Colors.Red, Colors.Orange, Colors.Yellow, Colors.Green, Colors.Blue, Colors.Violet };

        //public void UpdateData(XyDataSeries<double, double> dataset)
        //{
        //    try
        //    {
        //        int colorIndex = dataIndex % 6;

        //        dataIndex++;

        //        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        //        {

        //            FastLineRenderableSeries renderSeries = new FastLineRenderableSeries();

        //            renderSeries.Stroke = graphColors[colorIndex];
        //            renderSeries.StrokeThickness = 2;

        //            renderSeries.PointMarker = new SquarePointMarker();
        //            renderSeries.PointMarker.Fill = Colors.White;
        //            renderSeries.PointMarker.Stroke = graphColors[colorIndex];

        //            sciChart.RenderableSeries.Add(renderSeries);

        //            renderSeries.DataSeries = dataset;
        //        }));
        //    }
        //    catch (Exception err)
        //    {

        //    }
        //}

        //private void LineChartExampleView_OnLoaded(object sender, RoutedEventArgs e)
        //{

        //    // Create a DataSeries of type X=double, Y=double
        //    var dataSeries = new XyDataSeries<double, double>();

        //    lineRenderSeries.DataSeries = dataSeries;

        //    var data = DataManager.Instance.GetFourierSeries(1.0, 0.1);

        //    // Append data to series. SciChart automatically redraws
        //    dataSeries.Append(data.XData, data.YData);

        //    sciChart.ZoomExtents();
        //}
    }
}
