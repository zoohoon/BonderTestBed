using System;
using SciChart.Charting.Visuals.Axes.LabelProviders;

namespace StatisticsViewModel
{
    public class CustomNumericLabelProvider : NumericLabelProvider
    {
        public CustomNumericLabelProvider(string[] xlabels)
        {
            _xLabels = xlabels;
        }

        private string[] _xLabels = { };

        //// Optional: called when the label provider is attached to the axis
        //public override void Init(IAxis parentAxis)
        //{
        //    // here you can keep a reference to the axis. We assume there is a 1:1 relation
        //    // between Axis and LabelProviders
        //    base.Init(parentAxis);
        //}

        // Optional: called when the Axis Begins each drawing pass
        public override void OnBeginAxisDraw()
        {
            try
            {
            // e.g. here you can reset any counters or any other custom logic at the start
            // of an axis render pass
            }
            catch (Exception err)
            {
                 throw;
            }
        }

        /// <summary>
        /// Formats a label for the axis from the specified data-value passed in
        /// </summary>
        /// <param name="dataValue">The data-value to format</param>
        /// <returns>
        /// The formatted label string
        /// </returns>
        public override string FormatLabel(IComparable dataValue)
        {
            double number = Convert.ToDouble(dataValue);

            var i = Math.Truncate(number) == number;

            if (i == true)
            {
                return _xLabels[(int)number - 1];
            }
            else
            {
                return "";
            }


            //if(i == 1)
            //{
            //    tmp = "SwingPlate";
            //}
            //else
            //{
            //    tmp = "";
            //}

            // Note: Implement as you wish, converting Data-Value to string
            //return dataValue.ToString();

            // NOTES:
            // dataValue is always a double.
            // For a NumericAxis this is the double-representation of the data
            // For a DateTimeAxis, the conversion to DateTime is new DateTime((long)dataValue)
            // For a TimeSpanAxis the conversion to TimeSpan is new TimeSpan((long)dataValue)
            // For a CategoryDateTimeAxis, dataValue is the index to the data-series
        }

        /// <summary>
        /// Formats a label for the cursor, from the specified data-value passed in
        /// </summary>
        /// <param name="dataValue">The data-value to format</param>
        /// <returns>
        /// The formatted cursor label string
        /// </returns>
        public override string FormatCursorLabel(IComparable dataValue)
        {
            // Note: Implement as you wish, converting Data-Value to string
            return dataValue.ToString();

            // NOTES:
            // dataValue is always a double.
            // For a NumericAxis this is the double-representation of the data
            // For a DateTimeAxis, the conversion to DateTime is new DateTime((long)dataValue)
            // For a TimeSpanAxis the conversion to TimeSpan is new TimeSpan((long)dataValue)
            // For a CategoryDateTimeAxis, dataValue is the index to the data-series
        }
    }
}
