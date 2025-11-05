using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace TemperatureCalViewProject
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TemperatureCalView : UserControl, IMainScreenView
    {
        public TemperatureCalView()
        {
            InitializeComponent();
        }

        readonly Guid _ViewGUID = new Guid("e905653f-52ff-460a-b7bf-d82f8996c0d8");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        private void DataGrid_First_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SecondDG.SelectedItem = null;
        }

        private void DataGrid_Second_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FirstDG.SelectedItem = null;
        }
    }
}
