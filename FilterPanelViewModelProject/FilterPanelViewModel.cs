using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using ProberInterfaces;
using System.Windows.Controls;
using System.Windows;
using LogModule;
using System.Windows.Media;

namespace FilterPanelVM
{
    public class FilterPanelViewModel : IFilterPanelViewModel
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName]string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        private ObservableCollection<MaskingItem> _RectangleCollection
            = new ObservableCollection<MaskingItem>();
        public ObservableCollection<MaskingItem> RectangleCollection
        {
            get { return _RectangleCollection; }
            set
            {
                _RectangleCollection = value;
                NotifyPropertyChanged();
            }
        }

        private string _Background;
        public string Background
        {
            get { return _Background; }
            set
            {
                _Background = value;
                NotifyPropertyChanged();
            }
        }

        private double _Opacity;
        public double Opacity
        {
            get { return _Opacity; }
            set
            {
                _Opacity = value;
                NotifyPropertyChanged();
            }
        }

        private bool _IsEnable;
        public bool IsEnable
        {
            get { return _IsEnable; }
            set
            {
                _IsEnable = value;
                NotifyPropertyChanged();
            }
        }

        public FilterPanelViewModel()
        {
            try
            {
                IsEnable = false;
                Background = "Black";
                Opacity = 0.6;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void RequestEnableMode()
        {
            try
            {
                UserControl MainScreenView = this.ViewModelManager().MainScreenView as UserControl;
                UserControl Topbar = this.ViewModelManager().MainTopBarView as UserControl;
                UserControl menu = this.ViewModelManager().MainMenuView as UserControl;

                var MainWindow = Application.Current.MainWindow;
                RectangleCollection.Clear();

                try
                {
                    double topbarTotalLeftMargine = GetTotalDirectionMargine(Topbar, MarinDirection.LEFT);
                    double topbarTotalTopMargine = GetTotalDirectionMargine(Topbar, MarinDirection.TOP);
                    double mainScreenTotalLeftMargine = GetTotalDirectionMargine(MainScreenView, MarinDirection.LEFT);
                    double mainScreenTotalTopMargine = GetTotalDirectionMargine(MainScreenView, MarinDirection.TOP);
                    double menuTotalLeftMargine = 0.0;
                    double menuTotalTopMargine = 0.0;
                    if (menu != null)
                    {
                        menuTotalLeftMargine = GetTotalDirectionMargine(menu, MarinDirection.LEFT);
                        menuTotalTopMargine = GetTotalDirectionMargine(menu, MarinDirection.TOP);
                    }


                    double TopMarginForMainScreenView = topbarTotalTopMargine + mainScreenTotalTopMargine + menuTotalTopMargine + Topbar.ActualHeight;

                    AddingRectangleCollection(Topbar, topbarTotalLeftMargine, topbarTotalTopMargine);
                    
                    if (AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")//Loader에서는 Menu랑 Main화면 빼고 다 빨간박스 안보이게 함.
                    {
                        if (MainScreenView.GetType().Name == "DeviceChangeView")
                        {
                            AddingRectangleCollection(MainScreenView, mainScreenTotalLeftMargine + 320, TopMarginForMainScreenView + 40);
                        }
                        else 
                        {
                            AddingRectangleCollection(MainScreenView, mainScreenTotalLeftMargine, TopMarginForMainScreenView);
                        }
                    }
                    else 
                    {
                        AddingRectangleCollection(MainScreenView, mainScreenTotalLeftMargine, TopMarginForMainScreenView);
                    }

                    if (this.ViewModelManager().FlyoutIsOpen)
                    {
                        AddingRectangleCollection(menu, menuTotalLeftMargine, menuTotalTopMargine);
                    }

                    SetReleaseInRectInfoCollection();

                    IsEnable = true;
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void RequestDisableMode()
        {
            try
            {
                if (IsEnable == true)
                {
                    SetReleaseInRectInfoCollection();

                    RectangleCollection.Clear();
                    IsEnable = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void AddingRectangleCollection(UserControl control, double topMargin, double leftMargin)
        {
            try
            {
                IEnumerable<FrameworkElement> elements = VisualChildrenHelper.VisualChildrenHelper.FindVisualChildren<FrameworkElement>(control);
                foreach (var element in elements)
                {
                    if (element is ICUIControl)
                    {
                        ICUIControl cuiControl = element as ICUIControl;

                        ScaleTransform scale = null;
                        SkewTransform skew = null;
                        RotateTransform rotate = null;
                        TranslateTransform translate = null;

                        if (cuiControl is IDisplayPort)
                        {
                            if ((cuiControl as IDisplayPort)?.EnalbeClickToMove == false)
                                continue;
                        }

                        var transformCollection = (element?.RenderTransform?.GetValue(TransformGroup.ChildrenProperty) as TransformCollection);

                        if (transformCollection != null)
                        {

                            foreach (var transform in transformCollection)
                            {
                                if (transform is ScaleTransform)
                                {
                                    scale = transform as ScaleTransform;
                                }
                                else if (transform is SkewTransform)
                                {
                                    skew = transform as SkewTransform;
                                }
                                else if (transform is RotateTransform)
                                {
                                    rotate = transform as RotateTransform;
                                }
                                else if (transform is TranslateTransform)
                                {
                                    translate = transform as TranslateTransform;
                                }
                            }
                        }

                        Rect rectInfo = this.GetControlPosition(control, element);
                        MaskingItem rect = new MaskingItem
                        {
                            X = topMargin + rectInfo.X,
                            Y = leftMargin + rectInfo.Y,
                            Width = (rectInfo.BottomRight - rectInfo.TopLeft).X,
                            Height = (rectInfo.BottomRight - rectInfo.TopLeft).Y,
                            SourceUI = cuiControl
                        };
                        
                        if (AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")//Loader에서는 Menu랑 Main화면 빼고 다 빨간박스 안보이게 함.
                        {
                            if (control.GetType().Name != "LoaderStageSummary_GOP"
                            && control.GetType().Name != "LoaderStageSummaryView"
                            && control.GetType().Name != "LoaderMainMenuControl"
                            && control.GetType().Name != "DeviceChangeView"
                            && control.GetType().Name != "LoaderFileTransferView"
                            )
                            {
                                continue;
                            }
                            
                            if (control.GetType().Name == "DeviceChangeView"
                                || control.GetType().Name == "LoaderMainMenuControl"
                                || control.GetType().Name == "LoaderStageSummary_GOP")//컨트롤 Visible이 false이면 빨간박스 안보이게하기 위함
                            {
                                if (element.IsVisible == false)
                                {
                                    continue;
                                }
                            }
                        }
                        RectangleCollection.Add(rect);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private Rect GetControlPosition(FrameworkElement basePanel, 
                                        FrameworkElement control,
                                        ScaleTransform scale = null,
                                        SkewTransform skew = null,
                                        RotateTransform rotate = null,
                                        TranslateTransform translate = null)
        {
            Rect controlPos;

            Point leftTopPoint;
            Point rightBottomPoint;
            Point leftTop = new Point();
            Point rightBottom = new Point();
            var half_ActualWidth = (control?.ActualWidth ?? 0) / 2;
            var half_ActualHeight = (control?.ActualHeight ?? 0) / 2;
            double scaleX = scale?.ScaleX ?? 1;
            double scaleY = scale?.ScaleY ?? 1;
            double direct = (rotate?.Angle ?? 0) / 180;

            direct = Math.Pow(-1, direct);
            leftTop.X = half_ActualWidth * (1 - (scaleX * direct));
            leftTop.Y = half_ActualHeight * (1 - (scaleY * direct));
            rightBottom.X = half_ActualWidth * (1 + (scaleX * direct));
            rightBottom.Y = half_ActualHeight * (1 + (scaleY * direct));

            leftTopPoint = control.TransformToVisual(basePanel).Transform(leftTop);
            rightBottomPoint = control.TransformToVisual(basePanel).Transform(rightBottom);

            controlPos = new Rect(leftTopPoint, rightBottomPoint);

            return controlPos;
        }

        private double GetTotalDirectionMargine(FrameworkElement control, MarinDirection direction)
        {
            double allMargine = 0;
            try
            {

                allMargine = GetDirectionMargineFromControl(control, direction);

                if (control.Parent != null
                    && control.Parent is FrameworkElement)
                {
                    allMargine += GetTotalDirectionMargine((FrameworkElement)control.Parent, direction);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return allMargine;
        }

        private double GetDirectionMargineFromControl(FrameworkElement control, MarinDirection direction)
        {
            double margine = 0;
            try
            {

                switch (direction)
                {
                    case MarinDirection.TOP:
                        margine = control.Margin.Top;
                        break;
                    case MarinDirection.BOTTOM:
                        margine = control.Margin.Bottom;
                        break;
                    case MarinDirection.LEFT:
                        margine = control.Margin.Left;
                        break;
                    case MarinDirection.RIGHT:
                        margine = control.Margin.Right;
                        break;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return margine;
        }

        private void SetReleaseInRectInfoCollection()
        {
            try
            {
                foreach (var rectInfo in RectangleCollection)
                {
                    CUIServices.CUIService.ToggleMaskingReleaseMode(rectInfo.SourceUI);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        private enum MarinDirection
        {
            TOP,
            BOTTOM,
            LEFT,
            RIGHT
        }
    }

}
