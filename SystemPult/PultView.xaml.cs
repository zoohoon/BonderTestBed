using System.Windows;
using System.Windows.Controls;

namespace SystemPult
{
    /// <summary>
    /// Interaction logic for PultView.xaml
    /// </summary>
    public partial class PultView : UserControl
    {
        public PultView()
        {
            InitializeComponent();
        }

        //private void ItemsControl_Drop(object sender, DragEventArgs e)
        //{
        //    if (e.Data.GetDataPresent("IOPortDescripter"))
        //    {
        //        string key = e.Data.GetData("IOPortDescripter") as string;

        //    }
        //}

        private void ItemsControl_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("IOPortDescripter") || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }
    }
}
