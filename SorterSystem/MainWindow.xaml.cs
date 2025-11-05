using LogModule;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SorterSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public SorterModuleVM ViewModel
        {
            get;
            private set;
        }

        public MainWindow()
        {
            ViewModel = new SorterModuleVM();
            _ = ViewModel.SetContainer();
            LoggerManager.Init();

            this.DataContext = ViewModel;

            InitializeComponent();

            //_ = viewModel.InitModule();
            ViewModel.InitCommand.Execute(null);
            CameraViewer.DataContext = ViewModel;

            this.Closed += new EventHandler(WindowClosed);
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            ViewModel.DeInitModule();
            LoggerManager.EventLogMg.DeInit();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int _selected = 0;
        private int selected
        {
            get
            {
                return _selected;
            }

            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    NotifyPropertyChanged(nameof(TabShow));
                    NotifyPropertyChanged(nameof(BtnColor));
                }
            }
        }
        private Visibility[] showTab = new Visibility[10];

        public Visibility[] TabShow
        {
            get
            {
                for (int i = 0; i < showTab.Length; i++) showTab[i] = i == selected ? Visibility.Visible : Visibility.Collapsed;
                return showTab;
            }
        }

        private Brush BtnBrush = new SolidColorBrush(Color.FromRgb(0x00, 0x88, 0xFF));
        private Brush[] _btnColor = new Brush[10];
        public Brush[] BtnColor
        {
            get
            {
                for (int i = 0; i < _btnColor.Length; i++) _btnColor[i] = i == selected ? BtnBrush : SystemColors.ControlBrush;
                return _btnColor;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs args)
        {
            Button btn = sender as Button;
            switch (btn.Name)
            {
                case "tab00": selected = 0; break;
                case "tab01": selected = 1; break;
                case "tab02": selected = 2; break;
                case "tab03": selected = 3; break;
                case "tab04": selected = 4; break;
                case "tab05": selected = 5; break;
                case "tab06": selected = 6; break;
                case "tab07": selected = 7; break;
                case "tab08": selected = 8; break;
                case "tab09": selected = 9; break;
            }
            Debug.WriteLine($"{btn?.Name} Clicked.");
        }
    }
}
