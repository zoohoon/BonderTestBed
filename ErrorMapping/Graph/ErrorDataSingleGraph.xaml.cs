using MahApps.Metro.Controls;
using SciChart.Charting.Model.DataSeries;
//using SciChart.Examples.ExternalDependencies.Data;
using System;
using System.Collections.Generic;
using System.Windows;
using LogModule;
namespace ErrorMapping.Graph
{
    /// <summary>
    /// ErrorDataSingleGraph.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ErrorDataSingleGraph : MetroWindow
    {
        private int camIndex=-1;
        ErrorDataTable _ErrorDataX;
        ErrorDataTable _ErrorDataY;
        public ErrorDataSingleGraph(int idx, ErrorDataTable errorDataX, ErrorDataTable errorDataY)
        {
            try
            {
            InitializeComponent();
            camIndex = idx;
            _ErrorDataX = errorDataX;
            _ErrorDataY = errorDataY;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        List<double> HOR_PosX = new List<double>();
        List<double> VER_PosX = new List<double>();
        List<double> HOR_PosY = new List<double>();
        List<double> VER_PosY = new List<double>();

        List<double> HOR_ValueX = new List<double>();
        List<double> VER_ValueX = new List<double>();
        List<double> HOR_ValueY = new List<double>();
        List<double> VER_ValueY = new List<double>();

        private void SettingErrorData()
        {
            try
            {
            var errX_HOR = _ErrorDataX.ErrorData_HOR;
            var errX_VER = _ErrorDataX.ErrorData_VER;
            var errY_HOR =_ErrorDataY.ErrorData_HOR;
            var errY_VER = _ErrorDataY.ErrorData_VER;

            HOR_PosX = errX_HOR.GetPosXList();
            VER_PosX = errX_VER.GetPosXList();
            HOR_PosY = errY_HOR.GetPosYList();
            VER_PosY = errY_VER.GetPosYList();

            HOR_ValueX  = errX_HOR.GetOffsetXList();
            VER_ValueX  = errX_VER.GetOffsetXList();
            HOR_ValueY  = errY_HOR.GetOffsetYList();
            VER_ValueY  = errY_VER.GetOffsetYList();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        private void ErrorDataGraph_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
           

            SettingErrorData();
            var HOR_DataX = new XyDataSeries<double, double>() { SeriesName = "CAM_" + camIndex + "_HOR_X" };
            var VER_DataX = new XyDataSeries<double, double>() { SeriesName = "CAM_" + camIndex + "_VER_X" };
            var HOR_DataY = new XyDataSeries<double, double>() { SeriesName = "CAM_" + camIndex + "_HOR_Y" };
            var VER_DataY = new XyDataSeries<double, double>() { SeriesName = "CAM_" + camIndex + "_VER_Y" };
            HOR_SeriesX.DataSeries = HOR_DataX;
            VER_SeriesX.DataSeries = VER_DataX;

            HOR_SeriesY.DataSeries = HOR_DataY;
            VER_SeriesY.DataSeries = VER_DataY;

            //LinearSeries.
            //var data = DataManager.Instance.GetSinewave(1.0, 0.0, 100, 25);
        
            // Append data to series. SciChart automatically redraws
            HOR_DataX.Append(HOR_PosX, HOR_ValueX);
            VER_DataX.Append(VER_PosX, VER_ValueX);

            HOR_DataY.Append(HOR_PosY, HOR_ValueY);
            VER_DataY.Append(VER_PosY, VER_ValueY);

            sciChartX.ZoomExtents();
            sciChartY.ZoomExtents();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        private void IsVisibleChangedX(object sender, EventArgs e)
        {
            sciChartX.ZoomExtents();
        }
        private void IsVisibleChangedY(object sender, EventArgs e)
        {
            sciChartY.ZoomExtents();
        }
    }
}
