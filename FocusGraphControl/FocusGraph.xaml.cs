using SciChart.Charting.Model.DataSeries;
using System;
using System.Windows;
using System.Windows.Media;
using LogModule;

namespace FocusGraphControl
{
    using SciChart.Charting.Visuals.PointMarkers;
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class FocusGraph : Window
    {
        private int dataIndex;
 
        public FocusGraph()
        {
            InitializeComponent();
        }
        Color[] graphColors = new Color[] {Colors.Red, Colors.Orange, Colors.Yellow, Colors.Green, Colors.Blue, Colors.Violet};
        public void UpdateData(XyDataSeries<double, double> dataset)
        {
            try
            {
                int colorIndex = dataIndex % 6;
                dataIndex++;

                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {

                    SplineLineRenderableSeries renderSeries = new SplineLineRenderableSeries();
                    
                    renderSeries.Stroke = graphColors[colorIndex];
                    renderSeries.StrokeThickness = 2;
                    renderSeries.IsSplineEnabled = false;
                    renderSeries.UpSampleFactor = 0;
                    //var pointMarker = new PointMarker();
                    renderSeries.PointMarker = new SquarePointMarker();
                    renderSeries.PointMarker.Fill = Colors.White;
                    renderSeries.PointMarker.Stroke = graphColors[colorIndex];
                    sciChartX.RenderableSeries.Add(renderSeries);

                    renderSeries.DataSeries = dataset;
                }));
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
        }
        public void ClearData()
        {
            try
            {
            dataIndex = 0;
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                sciChartX.RenderableSeries.Clear();
            }));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        private void IsVisibleChangedX(object sender, EventArgs e)
        {
            try
            {
                sciChartX.ZoomExtents();
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
     


    }
}


