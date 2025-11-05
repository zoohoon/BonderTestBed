using LogModule;
using System;
using System.Collections.Generic;

namespace DutViewer.ViewModel
{
    using ProberInterfaces.PinAlign.ProbeCardData;
    using SharpDXRender.RenderObjectPack;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using WinSize = System.Windows.Size;

    public class DutViewerViewModel : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ==> MinimapVisible
        private bool _MinimapVisible;
        public bool MinimapVisible
        {
            get { return _MinimapVisible; }
            set
            {
                if (value != _MinimapVisible)
                {
                    _MinimapVisible = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        public DutRenderLayer MainRenderLayer { get; set; }
        public DutRenderLayer SideRenderLayer { get; set; }
        public DutViewerViewModel(WinSize scopeSize)
        {
            try
            {
                MainRenderLayer = new DutRenderLayer(scopeSize);
                MainRenderLayer.MouseDownHandler = MainScreen_MouseDown;
                MainRenderLayer.Init();
                MainRenderLayer.ZoomSetting(5, 20, 1);
                MainRenderLayer.GroupSpliterRenderData.MouseMoveAmount = MainRenderLayer.MouseMoveAmount;
                MainRenderLayer.GroupSpliterRenderData.ZoomSetting(5, 20, 1);

                SideRenderLayer = new DutRenderLayer(scopeSize);
                //==> 
                SideRenderLayer.Degree = MainRenderLayer.Degree;
                SideRenderLayer.DutRenderContainer = MainRenderLayer.DutRenderContainer;
                SideRenderLayer.PinRenderContainer = MainRenderLayer.PinRenderContainer;
                SideRenderLayer.PinDataDic = MainRenderLayer.PinDataDic;
                SideRenderLayer.PinContainerDic = MainRenderLayer.PinContainerDic;
                SideRenderLayer.MouseMoveAmount = MainRenderLayer.MouseMoveAmount;
                //==> 
                SideRenderLayer.RedDot = new RenderEllipse(
                    0, 0,
                    MainRenderLayer.RedDot.Width * 5,
                    MainRenderLayer.RedDot.Height * 5,
                    "Red");
                SideRenderLayer.RedDot.DrawBasePos = MainRenderLayer.LayerCenterPos;
                SideRenderLayer.ZoomSetting(5, 20, 10);
                SideRenderLayer.GroupSpliterRenderData.MouseMoveAmount = MainRenderLayer.MouseMoveAmount;
                SideRenderLayer.GroupSpliterRenderData.ZoomSetting(5, 20, 10);

                MinimapVisible = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public virtual void ZoomIn()
        {
            try
            {
                MainRenderLayer.ZoomIn();
                SideRenderLayer.ZoomIn();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public virtual void ZoomOut()
        {
            try
            {
                MainRenderLayer.ZoomOut();
                SideRenderLayer.ZoomOut();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public virtual void LeftRotate()
        {
            try
            {
                MainRenderLayer.LeftRotate(15);
                SideRenderLayer.LeftRotate(15);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public virtual void RightRotate()
        {
            try
            {
                MainRenderLayer.RightRotate(15);
                SideRenderLayer.RightRotate(15);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private void CheckMinimapVisible()
        {
            try
            {
                //==> 추후 사용 할 수 있음
                float minimapLimitZoom = ((SideRenderLayer.MaxZoomLevel - SideRenderLayer.MinZoomLevel) / 5) * 4;
                if (SideRenderLayer.ZoomLevel > minimapLimitZoom)
                    MinimapVisible = false;
                else
                    MinimapVisible = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        protected void UpdateDutColor()
        {
            try
            {
                foreach (var item in MainRenderLayer.PinContainerDic)
                {
                    RenderRectangle dutRenderObject = item.Key;
                    List<RenderEllipse> pinRenderObject = item.Value;

                    String color = "DutColor";
                    foreach (RenderEllipse elip in pinRenderObject)
                    {
                        IPinData pinData;
                        if (MainRenderLayer.PinDataDic.TryGetValue(elip, out pinData) == false)
                            continue;

                        if (pinData.IsRegisteredPin.Value)
                        {
                            color = "Cyan";
                            break;
                        }
                    }
                    dutRenderObject.Color = color;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        protected void UpdatePinColor()
        {
            try
            {
                foreach (var pin in MainRenderLayer.PinDataDic)
                {
                    IPinData pinData = pin.Value;
                    RenderObject pinRenderObject = pin.Key;
                    if (pinData.IsRegisteredPin.Value)
                        pinRenderObject.Color = "Red";
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public virtual void MainScreen_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                //MainScreenRenderData.SelectPin();
                //MinimapRenderData.SelectedPinData = MainScreenRenderData.SelectedPinData;
                //MinimapRenderData.SelectedPinRenderObject = MainScreenRenderData.SelectedPinRenderObject;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
