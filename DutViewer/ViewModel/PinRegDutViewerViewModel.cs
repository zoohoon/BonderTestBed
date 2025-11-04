using LogModule;
using System;

namespace DutViewer.ViewModel
{
    using ProberInterfaces.PinAlign.ProbeCardData;
    using WinSize = System.Windows.Size;

    public class PinRegDutViewerViewModel : DutViewerViewModel
    {
        public PinRegDutViewerViewModel(WinSize scopeSize)
            : base(scopeSize)
        {
            try
            {
                UpdatePinColor();
                UpdateDutColor();
                MainRenderLayer.GroupSpliterRenderData.IsVisible = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void AddRegisterPin()
        {
            try
            {
                //MainScreenRenderData.SelectedPinData.IsRegisteredPin.Value = true;
                MainRenderLayer.SelectedPinRender.Color = "Red";
                MainRenderLayer.SelectedDutRender.Color = "Cyan";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void DelRegisterPin()
        {
            try
            {
                //MainScreenRenderData.SelectedPinData.IsRegisteredPin.Value = false;
                MainRenderLayer.SelectedPinRender.Color = "PinColor";
                MainRenderLayer.SelectedDutRender.Color = "DutColor";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public IPinData GetSelectedPinData()
        {
            return MainRenderLayer.SelectedPinData;
        }
    }
}
