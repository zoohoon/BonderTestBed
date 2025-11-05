using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace UCNeedleClean
{
    using ProberInterfaces;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// UcNeedleCleanMainPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcNeedleCleanMainPage : UserControl, IMainScreenView, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion    

        public UcNeedleCleanMainPage()
        {
            InitializeComponent();
        }

        readonly Guid _ViewGUID = new Guid("119e4fda-380f-4bd3-8863-f0998c7fb50d");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        private void Canvas_MouseLeave(object sender, MouseEventArgs e)
        {

        }
    }
}
