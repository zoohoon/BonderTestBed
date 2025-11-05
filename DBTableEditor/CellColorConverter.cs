using System;
using System.Collections.Generic;

namespace DBTableEditor
{
    using System.Data;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media;
    public class CellColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            DataGridCell dgc = value as DataGridCell;
            if (dgc == null)
            {
                return DependencyProperty.UnsetValue;
            }

            DataRowView rowView = dgc.DataContext as DataRowView;
            if (rowView == null)
            {
                return DependencyProperty.UnsetValue;
            }


            //==> Color Column
            String cellColorCombo = rowView.Row.ItemArray[rowView.Row.ItemArray.Length - 1] as String;
            if (cellColorCombo == null)
            {
                return DependencyProperty.UnsetValue;
            }

            Tuple<SolidColorBrush, HashSet<String>> colorInfo = DGColor.GetColorInfoOrNull(cellColorCombo);
            if (colorInfo == null)
            {
                return DependencyProperty.UnsetValue;
            }


            SolidColorBrush solidColorBrush = colorInfo.Item1;
            HashSet<String> columnes = colorInfo.Item2;

            //==> column 전체 선택
            if (columnes == null)
            {
                return solidColorBrush;
            }

            //==> column 선택 안함.
            if (columnes.Count == 0)
            {
                return DependencyProperty.UnsetValue;
            }

            //==> column 부분 선택
            String currentCellIndex = dgc.Column.DisplayIndex.ToString();
            
            if (columnes.Contains(currentCellIndex))
            {
                return solidColorBrush;
            }

            return Brushes.Beige;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
