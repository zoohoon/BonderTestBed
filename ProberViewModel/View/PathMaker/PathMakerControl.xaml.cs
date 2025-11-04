using ProberInterfaces;
using System;
using System.Windows;
using System.Windows.Controls;

namespace PathMakerView
{


    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PathMakerControl : UserControl, IMainScreenView
    {
        public PathMakerControl()
        {
            try
            {
                InitializeComponent();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (this.VisionManager().CameraDescriptor != null)
                    {

                        foreach (var item in this.VisionManager().CameraDescriptor.Cams)
                        {
                            this.VisionManager().SetDisplayChannel(item, Display);
                        }

                    }
                });
            }
            catch (Exception err)
            {
                throw;
            }
        }

        readonly Guid _ViewGUID = new Guid("717bad3b-49a1-46d5-b0ec-dfabee758708");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }
}
