using CustomMoveViewModel;
using LogModule;
using MahApps.Metro.Controls;
using ProberInterfaces;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace CustomMoveView
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CustomMove : MetroWindow, IMainScreenView, IDisposable
    {
        private static CustomMove custommoveView;
        private CustomMoveVM _custommoveVM;

        public CustomMoveVM custommoveVM
        {
            get { return _custommoveVM; }
            set { _custommoveVM = value; }
        }
        public CustomMove()
        {
            custommoveVM = new CustomMoveVM();
            this.DataContext = custommoveVM;
            InitializeComponent();
        }
        public static CustomMove GetInstance()
        {
            try
            {
                if (custommoveView == null)
                {
                    custommoveView = new CustomMove();
                }
                return custommoveView;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                e.Cancel = true;
                this.Visibility = Visibility.Hidden;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        readonly string _ViewModelType = "CustomMoveViewModel";
        public string ViewModelType { get { return _ViewModelType; } }

        readonly Guid _ViewGUID = new Guid("8D12A656-93CB-408F-9FBD-B61D1CF83236");
        public Guid ScreenGUID { get { return _ViewGUID; } }

    }

}
