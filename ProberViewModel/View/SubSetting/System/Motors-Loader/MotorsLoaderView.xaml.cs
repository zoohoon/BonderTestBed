using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace UcMotorsLoaderView
{
    public partial class MotorsLoaderView : UserControl, IMainScreenView
    {
        public MotorsLoaderView()
        {
            InitializeComponent();
        }

        private readonly Guid _ViewGUID = new Guid("bccda8db-7848-4e6b-af7a-79d796c987ab");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }
}
