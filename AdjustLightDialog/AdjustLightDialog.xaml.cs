using ProberInterfaces;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace AdjustLightDialog
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window, IMainScreenView
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        readonly Guid _ViewGUID = new Guid("57E3B9EC-B7C6-4A17-89A0-7122936399C8");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        private AdjustLightVM _ViewModel = new AdjustLightVM();
        public AdjustLightVM ViewModel
        {
            get { return _ViewModel; }
            set
            {
                if (_ViewModel != value)
                {
                    _ViewModel = value;
                    RaisePropertyChanged();
                }
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new AdjustLightVM();
            this.DataContext = ViewModel;
            ViewModel.InitModule();
        }
    }
}
