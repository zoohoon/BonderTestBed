using System;
using System.Windows.Controls;
using ProberInterfaces;

namespace WaferHandlingControl
{
    /// <summary>
    /// UcWaferHandling.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcWaferHandling : UserControl, IMainScreenView
    {
        readonly string _ViewModelType = "IWaferHandlingViewModel";
        public string ViewModelType { get { return _ViewModelType; } }

        readonly Guid _ViewGUID = new Guid("4F42078C-05FE-B4B7-70ED-0602C9DF269B");
        public Guid ScreenGUID { get { return _ViewGUID; } }
      

        public UcWaferHandling()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception err)
            {
                throw;
            }
        }
    }
}
