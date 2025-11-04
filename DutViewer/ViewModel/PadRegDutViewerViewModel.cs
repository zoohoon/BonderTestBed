using System;
using System.Linq;

namespace DutViewer.ViewModel
{
    using System.Windows;
    using LogModule;
    using ProberInterfaces;
    using SharpDXRender.RenderObjectPack;
    public class PadRegDutViewerViewModel : DutViewerViewModel
    {
        private Size _ScopeSize;
        public Size UcDisplayDutSize { get; set; } = new Size(1, 1);
        public Size WndDutSize { get; set; } = new Size(1, 1);
        public PadRegDutViewerViewModel(Size scopeSize)
            : base(scopeSize)
        {
            try
            {
                _ScopeSize = scopeSize;
                MinimapVisible = false;
                MainRenderLayer.ZoomSetting(10, 50, 1);
                MainRenderLayer.MinZoomLevel = 10;

                MainRenderLayer.MouseDownEventEnable = false;

                RenderObject dut = MainRenderLayer.DutRenderContainer.RenderObjectList.FirstOrDefault();
                if (dut == null)
                    return;

                WndDutSize = new Size(dut.Width, dut.Height);
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public override void MainScreen_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                //return;//==> For Debug




                IDisplayPort displayPort = sender as IDisplayPort;
                if (displayPort == null)
                    return;

                FrameworkElement displayPortView = displayPort.GetViewObject() as FrameworkElement;
                if (displayPortView == null)
                    return;

                double ucDisplayWndWidth = displayPortView.ActualWidth;
                double ucDisplayWndHeight = displayPortView.ActualHeight;
                double ucDisplayWndCenterX = ucDisplayWndWidth / 2;
                double ucDisplayWndCenterY = ucDisplayWndHeight / 2;

                double ucDisplayWndMouseDownX = e.GetPosition(displayPortView).X;
                double ucDisplayWndMouseDownY = e.GetPosition(displayPortView).Y;
                double ucDisplayCenterFromX = ucDisplayWndCenterX - ucDisplayWndMouseDownX;
                double ucDisplayCenterFromY = ucDisplayWndCenterY - ucDisplayWndMouseDownY;

                //double dutMapWndWidth = _ScopeSize.Width;
                //double dutMapWndHeight = _ScopeSize.Height;
                //double wndRatioX = dutMapWndWidth / ucDisplayWndWidth;
                //double wndRatioY = dutMapWndHeight / ucDisplayWndHeight;

                double ucDisplayMagX = (WndDutSize.Width) / UcDisplayDutSize.Width;
                double ucDisplayMagY = (WndDutSize.Height) / UcDisplayDutSize.Height;
                MainRenderLayer.MoveMatrixPivot(
                    ucDisplayCenterFromX * ucDisplayMagX,
                    ucDisplayCenterFromY * ucDisplayMagY);
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
