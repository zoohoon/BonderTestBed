using LogModule;
using ProberInterfaces;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Visuals;
using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace CylinderGraphControl
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CylinderGraph : UserControl, ISciChartView
    {
        public ObservableCollection<IRenderableSeriesViewModel> RenderableSeriesViewModels
        {
            get;
            set;
        }
        public SciChartSurface ChartSurface { get => this.scichartsurface; }

        public CylinderGraph()
        {
            try
            {
                InitializeComponent();

                RenderableSeriesViewModels = new ObservableCollection<IRenderableSeriesViewModel>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

    }
}
