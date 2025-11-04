namespace ProberViewModel.View.Summary
{
    using System;
    using System.Globalization;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media;
    using LogModule;
    using ProberInterfaces;

    /// <summary>
    /// ActiveStageInformationUC.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ActiveStageInformationUC : UserControl
    {
        public ActiveStageInformationUC()
        {
            InitializeComponent();
        }
    }

    public class ActiveStateColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush colorBrush = new SolidColorBrush(Colors.Gray);
            try
            {
                if(value is StageAssignStateEnum)
                {
                    StageAssignStateEnum stageAssignState = (StageAssignStateEnum)value;
                    if(stageAssignState == StageAssignStateEnum.ASSIGN)
                    {
                        colorBrush = new SolidColorBrush(Colors.Green);
                    }
                    else if (stageAssignState == StageAssignStateEnum.UNASSIGN)
                    {
                        colorBrush = new SolidColorBrush(Colors.Red);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return colorBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
