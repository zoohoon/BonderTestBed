using System;
using System.Windows;
using System.Windows.Controls;

namespace DBTableEditor
{
    using System.Reflection;

    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainWindowViewModel();
        }

        //==> currentCsvDataGrid, updateCsvDataGrid 의 Horizontal Scroll 움직임을 동기화
        private void CurrentCsvDataGrid_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            Type t = currentCsvDataGrid.GetType();
            ScrollViewer sv = t.InvokeMember("InternalScrollHost", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty, null, updateCsvDataGrid, null) as ScrollViewer;
            sv.ScrollToHorizontalOffset(e.HorizontalOffset);
        }
        private void UpdateCsvDataGrid_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            Type t = updateCsvDataGrid.GetType();
            ScrollViewer sv = t.InvokeMember("InternalScrollHost", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty, null, currentCsvDataGrid, null) as ScrollViewer;
            sv?.ScrollToHorizontalOffset(e.HorizontalOffset);
        }
        //==> currentCsvDataGrid, updateCsvDataGrid 의 row 자동 선택
        private void CurrentCsvDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (updateCsvDataGrid.SelectedItem == null)
            {
                return;
            }

            updateCsvDataGrid.ScrollIntoView(updateCsvDataGrid.SelectedItem);
        }
        //private void UpdateCsvDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        //{
        //    e.Row.Background = Brushes.Aqua;

        //    e.Row.Cell

        //    var i = e.Row.Item;
        //}
        //private void UpdateCsvDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        //{
        //    if (currentCsvDataGrid.SelectedItem == null)
        //    {
        //        return;
        //    }

        //    currentCsvDataGrid.ScrollIntoView(currentCsvDataGrid.SelectedItem);
        //}
    }
}
