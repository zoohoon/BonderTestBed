using System.Windows;
using System.Windows.Controls;

namespace TestSetupDialog.Tab.GPCC
{
    using ProberInterfaces;
    /// <summary>
    /// UcGPCardChangeObservation.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcGPCardChangeObservation : UserControl, IFactoryModule
    {
        public UcGPCardChangeObservation()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.VisionManager().SetDisplayChannelStageCameras(displayport);

            this.VisionManager().StopGrab(EnumProberCam.PIN_LOW_CAM);
            this.VisionManager().StartGrab(EnumProberCam.PIN_LOW_CAM, this);
        }
    }
}
