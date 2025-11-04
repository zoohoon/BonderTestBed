using LoaderBase.Communication;
using LoaderParameters;
using LogModule;
using MahApps.Metro.Controls;
using ProberInterfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UcAnimationScrollViewer;

namespace LoaderStageSummaryViewModule
{
    /// <summary>
    /// LoaderStageSummary_MPT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoaderStageSummary_CATANIA : UserControl, IMainScreenView, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public LoaderStageSummary_CATANIA()
        {
            InitializeComponent();
        }

        readonly Guid _ViewGUID = new Guid("75878d72-d4b9-4b6d-9ee6-f83290712b2e");
        public Guid ViewGUID
        {
            get { return _ViewGUID; }
        }

        private void CategoryUpBtnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DoubleAnimation verticalAnimation = new DoubleAnimation();

                verticalAnimation.From = svViewer.VerticalOffset;
                verticalAnimation.To = svViewer.VerticalOffset - ((svViewer.ActualHeight / 3) * 2);
                verticalAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300));

                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(verticalAnimation);

                Storyboard.SetTarget(verticalAnimation, svViewer);
                Storyboard.SetTargetProperty(verticalAnimation, new PropertyPath(AnimationScrollViewer.CurrentVerticalOffsetProperty));

                storyboard.Begin();
            }
            catch (Exception err)
            {
                throw;
            }
        }

        private void CategoryDwBtnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DoubleAnimation verticalAnimation = new DoubleAnimation();

                verticalAnimation.From = svViewer.VerticalOffset;
                verticalAnimation.To = svViewer.VerticalOffset + ((svViewer.ActualHeight / 3) * 2);
                verticalAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300));

                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(verticalAnimation);

                Storyboard.SetTarget(verticalAnimation, svViewer);
                Storyboard.SetTargetProperty(verticalAnimation, new PropertyPath(AnimationScrollViewer.CurrentVerticalOffsetProperty));
                storyboard.Begin();
            }
            catch (Exception err)
            {
                throw;
            }
        }
    }

    //public class SqueezeStackPanel : Panel
    //{
    //    protected override Size MeasureOverride(Size availableSize)
    //    {
    //        var desiredHeight = 0.0;
    //        foreach (UIElement child in InternalChildren)
    //        {
    //            child.Measure(availableSize);
    //            desiredHeight += child.DesiredSize.Height;
    //        }

    //        if (availableSize.Height < desiredHeight)
    //        {
    //            // we will never go out of bounds
    //            return availableSize;
    //        }
    //        return new Size(availableSize.Width, desiredHeight);
    //    }

    //    protected override Size ArrangeOverride(Size finalSize)
    //    {
    //        // measure desired heights of children in case of unconstrained height
    //        var size = MeasureOverride(new Size(finalSize.Width, double.PositiveInfinity));

    //        var startHeight = 0.0;
    //        var squeezeFactor = 1.0;
    //        // adjust the desired item height to the available height
    //        if (finalSize.Height < size.Height)
    //        {
    //            squeezeFactor = finalSize.Height / size.Height;
    //        }

    //        foreach (UIElement child in InternalChildren)
    //        {
    //            var allowedHeight = child.DesiredSize.Height * squeezeFactor;
    //            var area = new Rect(new Point(0, startHeight), new Size(finalSize.Width, allowedHeight));
    //            child.Arrange(area);
    //            startHeight += allowedHeight;
    //        }

    //        return new Size(finalSize.Width, startHeight);
    //    }
    //}


    public class CellSelectedToBrushConverter_LotSettings : IMultiValueConverter
    {
        //static SolidColorBrush WhiteBrush = new SolidColorBrush(Colors.Gray);
        static SolidColorBrush WhiteBrush = new SolidColorBrush(Colors.White);
        static SolidColorBrush GreenBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5DFC0A"));


        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            SolidColorBrush retval = WhiteBrush;

            try
            {
                IList selectedlist = values[0] as IList;

                if (values[2] is int)
                {
                    int selectedcount = (int)values[2];

                    if (selectedcount != 0)
                    {
                        if (selectedlist.Count > 0)
                        {
                            //IStageObject_forLot CurrentStage = values[1] as IStageObject_forLot;
                            IStageObject CurrentStage = values[1] as IStageObject;

                            int index = selectedlist.IndexOf(CurrentStage);

                            if (index != -1)
                            {
                                //if (CurrentStage.bIsSelected == false)
                                if (CurrentStage.LotSetting.IsSelected == true)
                                {
                                    retval = GreenBrush;
                                    //retval = WhiteBrush;
                                }
                                //else
                                //{
                                //retval = GreenBrush;
                                //}
                            }
                            //else
                            //{
                            //    retval = WhiteBrush;
                            //}
                        }
                        //else
                        //{
                        //    retval = WhiteBrush;
                        //}
                    }
                    //else
                    //{
                    //    retval = WhiteBrush;
                    //}
                }
                //else
                //{
                //    retval = WhiteBrush;
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
             System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class CellSelectedToBorderThicknessConverter_LotSettings : IMultiValueConverter
    {
        static Thickness SelectedBorderThickness = new Thickness(3);
        static Thickness NotSelectedBorderThickness = new Thickness(1);
        static Thickness AsignedSelectedBorderThickness = new Thickness(2);

        public object Convert(object[] values, Type targetType, object parameter,
                System.Globalization.CultureInfo culture)
        {
            Thickness? retval = NotSelectedBorderThickness;

            try
            {
                IList selectedlist = values[0] as IList;

                //IStageObject_forLot CurrentStage = values[1] as IStageObject_forLot;
                IStageObject CurrentStage = values[1] as IStageObject;

                int selectedcount = -1;

                if (values[2] is int)
                {
                    selectedcount = (int)values[2];
                }

                if (selectedcount > 0)
                {
                    if (selectedlist.Count > 0)
                    {
                        int index = selectedlist.IndexOf(CurrentStage);

                        //if (CurrentStage.bIsSelected == true)
                        if (CurrentStage.LotSetting.IsSelected == true)
                        {
                            if (CurrentStage.LotSetting.IsAssigned == true)
                            {
                                retval = AsignedSelectedBorderThickness;
                            }
                            else
                            {
                                retval = SelectedBorderThickness;
                            }
                        }
                        //else
                        //{
                        //    retval = NotSelectedBorderThickness;
                        //}
                    }
                    //else
                    //{
                    //    retval = NotSelectedBorderThickness;
                    //}
                }
                //else
                //{
                //    retval = NotSelectedBorderThickness;
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class StageModeToEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool retval = false;

            try
            {
                if (value is GPCellModeEnum)
                {
                    GPCellModeEnum inputval = (GPCellModeEnum)value;

                    switch (inputval)
                    {
                        case GPCellModeEnum.OFFLINE:
                            // TODO : ?
                            break;
                        case GPCellModeEnum.ONLINE:
                        case GPCellModeEnum.MAINTENANCE:
                            retval = true;
                            break;
                        case GPCellModeEnum.DISCONNECT:
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StageModeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility retval = Visibility.Collapsed;

            try
            {
                if (value is GPCellModeEnum)
                {
                    GPCellModeEnum inputval = (GPCellModeEnum)value;

                    switch (inputval)
                    {
                        case GPCellModeEnum.OFFLINE:
                            // TODO : ?
                            break;
                        case GPCellModeEnum.ONLINE:
                        case GPCellModeEnum.MAINTENANCE:
                            retval = Visibility.Visible;
                            break;
                        case GPCellModeEnum.DISCONNECT:
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }



    public class StageAssignedToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility retval = Visibility.Collapsed;

            try
            {
                if (value is true)
                {
                    retval = Visibility.Visible;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StageModeToForegroundConverter : IValueConverter
    {
        static SolidColorBrush WhiteBrush = new SolidColorBrush(Colors.White);
        static SolidColorBrush LimeBrush = new SolidColorBrush(Colors.Lime);
        static SolidColorBrush GrayBrush = new SolidColorBrush(Colors.Gray);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush retval = WhiteBrush;

            try
            {
                if (value is GPCellModeEnum)
                {
                    GPCellModeEnum inputval = (GPCellModeEnum)value;

                    switch (inputval)
                    {
                        case GPCellModeEnum.OFFLINE:
                        case GPCellModeEnum.DISCONNECT:
                            retval = GrayBrush;
                            break;
                        case GPCellModeEnum.ONLINE:
                        case GPCellModeEnum.MAINTENANCE:
                            retval = LimeBrush;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SelectedItemTransferConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool || value is bool?)
            {
                if ((bool?)value == false)
                {
                    return null;
                }
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType == typeof(bool) || targetType == typeof(bool?))
            {
                return value != null;
            }
            return Binding.DoNothing;
        }
    }

    public class LotLauncherStartBtnEnableConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            bool retval = false;

            try
            {
                // TODO : 
                // 1) 선택 된 FOUP의 lotState가 IDLE 또는 DONE 일 것.
                // 2) 할당 된(IsAssigned = true) 스테이지들의 Verified Flag의 값이 모두 Ture일 것.

                if (values.Length == 3)
                {
                    if (values[0] is LotStateEnum && values[1] is ObservableCollection<IStageObject>)
                    {
                        LotStateEnum lotStateEnum = (LotStateEnum)values[0];
                        ObservableCollection<IStageObject> stages = values[1] as ObservableCollection<IStageObject>;

                        if (lotStateEnum == LotStateEnum.Idle || lotStateEnum == LotStateEnum.Done)
                        {
                            if (stages != null)
                            {
                                if (stages.Count > 0)
                                {
                                    bool ExistAssignedCell = stages.Any(x => x.LotSetting.IsAssigned == true);

                                    if (ExistAssignedCell == true)
                                    {
                                        foreach (var stage in stages)
                                        {
                                            if (stage.LotSetting.IsAssigned == true)
                                            {
                                                if (stage.StageInfo.IsVerified == true)
                                                {
                                                    retval = true;
                                                }
                                                else
                                                {
                                                    retval = false;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
             System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class TesterGridAvaliableToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility retval = Visibility.Collapsed;

            try
            {
                if (value is bool)
                {
                    bool inputval = (bool)value;

                    if(inputval == true)
                    {
                        retval = Visibility.Visible;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TesterConnectedToBrushConverter : IValueConverter
    {
        static SolidColorBrush WhiteBrush = new SolidColorBrush(Colors.White);
        static SolidColorBrush LimeBrush = new SolidColorBrush(Colors.Lime);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush retval = WhiteBrush;

            try
            {
                if (value is bool)
                {
                    bool inputval = (bool)value;

                    if(inputval == true)
                    {
                        retval = LimeBrush;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SlotNameLengthAdjustConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string retval = string.Empty;

            try
            {
                if(value is string)
                {
                    string compareStr = "SLOT #";

                    retval = (string)value;

                    retval = retval.Replace("SLOT #", "");

                    if(retval.Length == 1)
                    {
                        retval = compareStr + "0" + retval;
                    }
                    else
                    {
                        retval = (string)value;
                    }

                    // TODO : Cell Index
                    // Index 분리 및 로직 필요.

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }

    public class OCRStringToVisibilityConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility retval = Visibility.Collapsed;

            try
            {
                if (value is string)
                {
                    string inputval = (string)value;

                    if(string.IsNullOrEmpty(inputval) == false)
                    {
                        retval = Visibility.Visible;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }
    
    public class LayoutModeToVisibilityConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility retval = Visibility.Collapsed;

            try
            {
                if (value is GPSummaryLayoutModeEnum)
                {
                    GPSummaryLayoutModeEnum inputval = (GPSummaryLayoutModeEnum)value;

                    if (inputval == GPSummaryLayoutModeEnum.BOTH)
                    {
                        retval = Visibility.Visible;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }

    public class PMIFlagToForegroundConverter : IValueConverter
    {
        static SolidColorBrush DefaultBrush = new SolidColorBrush(Colors.Gray);
        static SolidColorBrush LimeGreenBrush = new SolidColorBrush(Colors.LimeGreen);

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush retval = DefaultBrush;

            try
            {
                if (value is bool)
                {
                    bool inputval = (bool)value;

                    if (inputval == true)
                    {
                        retval = LimeGreenBrush;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }

    public class EnumDescriptionConverter : IValueConverter
    {
        private string GetEnumDescription(Enum enumObj)
        {
            string retval = string.Empty;

            try
            {
                if (enumObj != null)
                {
                    FieldInfo fieldInfo = enumObj.GetType().GetField(enumObj.ToString());

                    object[] attribArray = fieldInfo.GetCustomAttributes(false);

                    if (attribArray.Length == 0)
                    {
                        retval = enumObj.ToString();
                    }
                    else
                    {
                        DescriptionAttribute attrib = attribArray[0] as DescriptionAttribute;
                        retval = attrib.Description;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string description = string.Empty;

            try
            {
                Enum myEnum = (Enum)value;

                description = GetEnumDescription(myEnum);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            
            return description;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }

    //public class ScrollViewerMaxHeightConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        double retval = 0;

    //        try
    //        {
    //            retval = (double)value;
    //        }
    //        catch (Exception err)
    //        {
    //            LoggerManager.Exception(err);
    //        }

    //        return retval;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
