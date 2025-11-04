using MultiExecuter.ViewModel;
using System;
using System.Windows.Controls;

namespace MultiExecuter.View
{
    public partial class ViewMultiExecuter : UserControl
    {
      
        private VmMultiExecuter vmMultiExecuter;
        public ViewMultiExecuter()
        {
            try
            {               
                InitializeComponent();
                vmMultiExecuter = new VmMultiExecuter();
                this.DataContext = vmMultiExecuter;
            }
            catch (Exception)
            {
                 throw;
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
