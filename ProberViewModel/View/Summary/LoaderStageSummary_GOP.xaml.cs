using LoaderBase.Communication;
using LogModule;
using ProberInterfaces;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using UcAnimationScrollViewer;

namespace LoaderStageSummaryViewModule
{
    /// <summary>
    /// LoaderStageSummary_MPT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoaderStageSummary_GOP : UserControl, IMainScreenView, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public LoaderStageSummary_GOP()
        {
            InitializeComponent();
        }

        readonly Guid _ViewGUID = new Guid("75878d72-d4b9-4b6d-9ee6-f83290712b2e");
        public Guid ScreenGUID
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

    public static class TextBlockEx
    {
        public static Inline GetFormattedText(DependencyObject obj)
        {
            return (Inline)obj.GetValue(FormattedTextProperty);
        }

        public static void SetFormattedText(DependencyObject obj, Inline value)
        {
            obj.SetValue(FormattedTextProperty, value);
        }

        public static readonly DependencyProperty FormattedTextProperty =
            DependencyProperty.RegisterAttached(
                "FormattedText",
                typeof(Inline),
                typeof(TextBlockEx),
                new PropertyMetadata(null, OnFormattedTextChanged));

        private static void OnFormattedTextChanged(
            DependencyObject o,
            DependencyPropertyChangedEventArgs e)
        {
            var textBlock = o as TextBlock;
            if (textBlock == null)
                return;

            var inline = (Inline)e.NewValue;
            textBlock.Inlines.Clear();
            if (inline != null)
            {
                textBlock.Inlines.Add(inline);
            }
        }
    }


    public class EnumExcludeUndefinedConverter : IValueConverter
    {


        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            LotModeEnum[] retval = null;

            try
            {
                if (value == null) return DependencyProperty.UnsetValue;

                retval = (LotModeEnum[])value;

                if (retval != null)
                {
                    retval = retval.Where(val => val.ToString() != "UNDEFINED" && val.ToString() != "CONTINUEPROBING").ToArray();
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
            return value;
        }
    }

    public class EnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return DependencyProperty.UnsetValue;

            return GetDescription((Enum)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        public static string GetDescription(Enum en)
        {
            Type type = en.GetType();
            MemberInfo[] memInfo = type.GetMember(en.ToString());
            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs != null && attrs.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }
            return en.ToString();
        }
    }

    public class StageLotDataToFormattedTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Span retval = null;

            try
            {
                if (values[0] is ObservableCollection<StageLotDataComponent> && values[1] is StageLotDataComponent)
                {
                    retval = new Span();

                    if (values[1] is StageLotDataComponent stagelotdata)
                    {
                        Run tmpRun = new Run();

                        tmpRun.Text = stagelotdata.value;

                        StageLotDataEnum type = stagelotdata.stageLotDataEnum;

                        string valuestr = stagelotdata.value;

                        if (type == StageLotDataEnum.WAFERALIGNSTATE ||
                            type == StageLotDataEnum.PINALIGNSTATE ||
                            type == StageLotDataEnum.MARKALIGNSTATE)
                        {
                            if (valuestr.ToUpper().Contains("DONE"))
                            {
                                tmpRun.Foreground = Brushes.LimeGreen;
                            }
                            else
                            {
                                tmpRun.Foreground = Brushes.White;
                            }
                        }

                        retval.Inlines.Add(tmpRun);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

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
                            IStagelotSetting CurrentStage = values[1] as IStagelotSetting;

                            int index = selectedlist.IndexOf(CurrentStage);

                            if (index != -1)
                            {
                                if (CurrentStage.IsSelected == true)
                                {
                                    retval = GreenBrush;
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
                IStagelotSetting CurrentStage = values[1] as IStagelotSetting;

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

                        if (CurrentStage.IsSelected == true)
                        {
                            if (CurrentStage.IsAssigned == true)
                            {
                                retval = AsignedSelectedBorderThickness;
                            }
                            else
                            {
                                retval = SelectedBorderThickness;
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

    public class StageModeToEnabledConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool retval = false;

            try
            {
                if (values[0] is int && values[1] is ObservableCollection<IStageObject>)
                {
                    int idx = (int)values[0];
                    ObservableCollection<IStageObject> stages = values[1] as ObservableCollection<IStageObject>;

                    var stage = stages.FirstOrDefault(x => x.Index == idx);

                    if (stage != null)
                    {
                        GPCellModeEnum mode = stage.StageMode;

                        switch (mode)
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
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //public class StageModeToEnabledConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        bool retval = false;

    //        try
    //        {
    //            if (value is GPCellModeEnum)
    //            {
    //                GPCellModeEnum inputval = (GPCellModeEnum)value;

    //                switch (inputval)
    //                {
    //                    case GPCellModeEnum.OFFLINE:
    //                        // TODO : ?
    //                        break;
    //                    case GPCellModeEnum.ONLINE:
    //                    case GPCellModeEnum.MAINTENANCE:
    //                        retval = true;
    //                        break;
    //                    case GPCellModeEnum.DISCONNECT:
    //                        break;
    //                    default:
    //                        break;
    //                }
    //            }
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

    public class StageMoveStateToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool retval = false;

            try
            {
                if (value is string)
                {
                    if ((string)value == "STAGELOCK")
                    {
                        return true;
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
            string retval = string.Empty;

            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    public class StageModeToEnabledLockConverter : IValueConverter
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
                        case GPCellModeEnum.ONLINE:
                        case GPCellModeEnum.MAINTENANCE:
                            retval = true;
                            break;
                        case GPCellModeEnum.DISCONNECT:
                        case GPCellModeEnum.OFFLINE:
                            retval = false;
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

                if (values.Length > 2)
                {
                    if (values[0] is LotStateEnum && values[1] is ObservableCollection<IStagelotSetting>)
                    {
                        LotStateEnum lotStateEnum = (LotStateEnum)values[0];
                        ObservableCollection<IStagelotSetting> lotsettings = values[1] as ObservableCollection<IStagelotSetting>;

                        if (lotStateEnum == LotStateEnum.Idle || lotStateEnum == LotStateEnum.Done)
                        {
                            if (lotsettings != null)
                            {
                                if (lotsettings.Count > 0)
                                {
                                    bool ExistAssignedCell = lotsettings.Any(x => x.IsAssigned == true);

                                    if (ExistAssignedCell == true)
                                    {
                                        foreach (var lotsetting in lotsettings)
                                        {
                                            if (lotsetting.IsAssigned == true)
                                            {
                                                if (lotsetting.IsVerified == true)
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
                if (values.Length == 5)
                {
                    if (values[3] is SecsEnum_ControlState ctrlStat &&
                        values[4] is string recvType)
                    {
                        // [STM_CATANIA] GEM Automation - Online Remote 상태인 경우 버튼 비활성화
                        var forcedDisable = recvType.Equals("SemicsGemReceiverSEKS") && ctrlStat.Equals(SecsEnum_ControlState.ONLINE_REMOTE);
                        if (forcedDisable)
                        {
                            retval = false;
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

    public class LotStatetoTeachPinBtnEnableConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            bool retval = false;

            try
            {
                // 1) 선택 된 FOUP의 lotState가 IDLE 또는 DONE 일 것.
                if (values[0] == null)
                { return retval; }
                string inputval = values[0].ToString();

                switch (inputval)
                {
                    case "ABORTED":
                        break;
                    case "DONE":
                        break;
                    case "ERROR":
                        break;
                    case "IDLE":
                        retval = true;
                        break;
                    case "PAUSED":
                        retval = true;
                        break;
                    case "RUNNING":
                        break;
                    default:
                        break;
                }


            }
            catch (Exception err)
            {
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

                    if (inputval == true)
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

                    if (inputval == true)
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
                if (value is string)
                {
                    string compareStr = "SLOT #";

                    retval = (string)value;

                    retval = retval.Replace("SLOT #", "");

                    if (retval.Length == 1)
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

                    if (string.IsNullOrEmpty(inputval) == false)
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
}
