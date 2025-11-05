using System.Windows;
using System.Windows.Controls;
using SciChart.Charting.Model.ChartSeries;

namespace ProberViewModel
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CylinderOPGraph : UserControl
    {
        #region ExtandThreshold
        public double ExtandThreshold
        {
            get { return (double)this.GetValue(ExtandThresholdProperty); }
            set { this.SetValue(ExtandThresholdProperty, value); }
        }

        public static readonly DependencyProperty ExtandThresholdProperty =
                        DependencyProperty.Register("ExtandThreshold",
                                                    typeof(double),
                                                    typeof(CylinderOPGraph), new PropertyMetadata(default(double)));
        #endregion

        #region RetractThreshold
        public double RetractThreshold
        {
            get { return (double)this.GetValue(RetractThresholdProperty); }
            set { this.SetValue(RetractThresholdProperty, value); }
        }

        public static readonly DependencyProperty RetractThresholdProperty =
                        DependencyProperty.Register("RetractThreshold",
                                                    typeof(double),
                                                    typeof(CylinderOPGraph), new PropertyMetadata(default(double)));
        #endregion

        #region ExtandRenderableSeries
        public IRenderableSeriesViewModel ExtandChartRenderableSeries
        {
            get { return (IRenderableSeriesViewModel)this.GetValue(RetractChartProperty); }
            set { this.SetValue(RetractChartProperty, value); }
        }

        public static readonly DependencyProperty RetractChartProperty =
                        DependencyProperty.Register("ExtandChartRenderableSeries",
                                                    typeof(IRenderableSeriesViewModel),
                                                    typeof(CylinderOPGraph), new PropertyMetadata(default(IRenderableSeriesViewModel)));
        #endregion

        #region RetractRenderableSeries
        public IRenderableSeriesViewModel RetractChartRenderableSeries
        {
            get { return (IRenderableSeriesViewModel)this.GetValue(ExtandChartProperty); }
            set { this.SetValue(ExtandChartProperty, value); }
        }

        public static readonly DependencyProperty ExtandChartProperty =
                DependencyProperty.Register("RetractChartRenderableSeries",
                                            typeof(IRenderableSeriesViewModel),
                                            typeof(CylinderOPGraph), new PropertyMetadata(default(IRenderableSeriesViewModel)));
        #endregion


        public CylinderOPGraph()
        {
            InitializeComponent();
        }
    }
}
