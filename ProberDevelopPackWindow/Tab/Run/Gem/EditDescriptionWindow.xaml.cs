using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProberDevelopPackWindow.Tab.Run.Gem
{
    /// <summary>
    /// EditDescriptionWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class EditDescriptionWindow : Window, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private string _Description;
        public string Description
        {
            get { return _Description; }
            set
            {
                _Description = value;
                RaisePropertyChanged();
            }
        }

        public EditDescriptionWindow(string currentDescription)
        {
            InitializeComponent();

            this.DataContext = this;
            Description = currentDescription;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Description = DescriptionTextBox.Text;
            this.DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DescriptionTextBox.Focus();
            DescriptionTextBox.SelectAll();
        }
    }
}
