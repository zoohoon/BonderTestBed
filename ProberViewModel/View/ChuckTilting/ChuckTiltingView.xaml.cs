using LogModule;
using ProberInterfaces;
using System;
using System.Globalization;
using System.Windows.Controls;

namespace ChuckTiltingViewControl
{
    public class DoubleRangeRule : ValidationRule
    {
        public double Min { get; set; }

        public double Max { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            double parameter = 0;

            try
            {
                try
                {
                    if (((string)value).Length > 1)
                    {
                        parameter = Double.Parse((String)value);
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);

                    return new ValidationResult(false, "Illegal characters or " + err.Message);
                }

                if ((parameter < this.Min) || (parameter > this.Max))
                {
                    return new ValidationResult(false,
                        "Please enter value in the range: "
                        + this.Min + " - " + this.Max + ".");
                }
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return new ValidationResult(true, null);
        }
    }

    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChuckTiltingView : UserControl, IMainScreenView
    {
        private Guid _ViewGUID = new Guid("209216bf-209e-45ea-b35e-6fdfe2748876");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        public ChuckTiltingView()
        {
            InitializeComponent();
        }
    }
}
