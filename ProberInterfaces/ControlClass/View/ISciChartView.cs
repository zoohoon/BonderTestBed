using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Visuals;
using System.Collections.ObjectModel;

namespace ProberInterfaces
{
    public interface ISciChartView
    {
        ObservableCollection<IRenderableSeriesViewModel> RenderableSeriesViewModels { get; set; }
        SciChartSurface ChartSurface { get; }
    }
}
