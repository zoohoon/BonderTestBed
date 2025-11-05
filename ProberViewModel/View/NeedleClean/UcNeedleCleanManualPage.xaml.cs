using System;
using System.Windows.Controls;

namespace NeedleCleanManualPageView
{
    using ProberInterfaces;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// UcNeedleCleanManualPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcNeedleCleanManualPage : UserControl, IMainScreenView, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion  

        public UcNeedleCleanManualPage()
        {            
            InitializeComponent();
        } 

        readonly Guid _ViewGUID = new Guid("4e9f3ab5-d0b8-47c0-8ce6-cbcb56803e98");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }
}
