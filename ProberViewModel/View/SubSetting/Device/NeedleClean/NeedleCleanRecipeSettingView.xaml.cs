using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace UcNeedleCleanRecipeSettingView
{
    using LogModule;
    using ProberInterfaces;
    using System.Globalization;

    public class DoubleRangeRule : ValidationRule
    {
        public double Min { get; set; }

        public double Max { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            double parameter = 0;

            try
            {
                if (((string)value).Length > 0)
                {
                    parameter = Double.Parse((String)value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                return new ValidationResult(false, "Illegal characters or "+ err.Message);
            }

            if ((parameter < this.Min) || (parameter > this.Max))
            {
                return new ValidationResult(false,
                    "Please enter value in the range: "
                    + this.Min + " - " + this.Max + ".");
            }
            return new ValidationResult(true, null);
        }
    }

    /// <summary>
    /// NeedleCleanRecipeSettingView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class NeedleCleanRecipeSettingView : UserControl, IMainScreenView
    {
        public NeedleCleanRecipeSettingView()
        {
            InitializeComponent();
        }
        private readonly Guid _ViewGUID = new Guid("a4bd4bb5-030d-4644-9c3d-c21921c09eee");
        public Guid ScreenGUID
        {
            get { return _ViewGUID; }
        }
    }


    public class NCSheetDevCountToStrConverter : IValueConverter
    {
        object IValueConverter.Convert(object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            string countStr = string.Empty;
            bool isFirst = false;
            try
            {
                if( parameter?.ToString() == "1")
                {
                    isFirst = true;
                }

                if (value != null && value is int)
                {
                    int count = (int)value;

                    if(isFirst)
                    {
                        if (count == 0)
                        {
                            countStr = "NC 2";
                        }
                        else if (count == 1)
                        {
                            countStr = "NC 1";
                        }
                        else if (count == 2)
                        {
                            countStr = "NC 1";
                        }
                    }
                    else
                    {
                        if (count == 0)
                        {
                            countStr = "NC 3";
                        }
                        else if (count == 1)
                        {
                            countStr = "NC 3";
                        }
                        else if (count == 2)
                        {
                            countStr = "NC 2";
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug("[ValueConverter - CountNumConverter] Can't convert");
                LoggerManager.Exception(err);
            }

            return countStr;
        }

        object IValueConverter.ConvertBack(object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
