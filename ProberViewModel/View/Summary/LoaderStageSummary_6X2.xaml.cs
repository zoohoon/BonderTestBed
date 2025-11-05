using ProberInterfaces;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace LoaderStageSummaryViewModule
{
    /// <summary>
    /// LoaderStageSummary_DRAX.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoaderStageSummary_6X2 : UserControl, IMainScreenView, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public LoaderStageSummary_6X2()
        {
            InitializeComponent();
        }

        readonly Guid _ViewGUID = new Guid("3f033346-7b8e-4862-81bc-8cbac4fb2090");
        public Guid ScreenGUID { get { return _ViewGUID; } }

     
    }
}
