using System;
using System.Collections.Generic;
using System.Text;

namespace DBTableEditor
{
    using DBManagerModule.Table;
    using System.Windows.Media;

    public enum EnumDGColor { LightGray, LightGreen, OrangeRed, Orange, White, Transparent }
    public static class DGColor
    {
        private static Dictionary<String, String> _ColorBinding = new Dictionary<string, string>()
        {
            {PramElementColumn.ElementID , "1"},
            {PramElementColumn.ElementName , "2"},
            {PramElementColumn.ElementAdmin , "3"},
            {PramElementColumn.AssociateElementID , "4"},
            {PramElementColumn.CategoryID , "5"},
            {PramElementColumn.Unit , "6"},
            {PramElementColumn.LowerLimit , "7"},
            {PramElementColumn.UpperLimit , "8"},
            {PramElementColumn.ReadMaskingLevel , "9"},
            {PramElementColumn.WriteMaskingLevel , "10"},
            {PramElementColumn.Description , "11"},
            {PramElementColumn.VID , "12"},
        };

        public static String ColorColumnName { get; } = "ColorColumn";

        public static String BuildColorComboString(EnumDGColor enumColor, IEnumerable<String> colors)
        {
            if (colors == null)
            {
                return enumColor.ToString();
            }

            //==> String build process Example
            StringBuilder stb = new StringBuilder();

            // stb => White
            stb.Append(enumColor.ToString());

            // stb => White,
            stb.Append(",");

            // stb => White,023
            foreach (String color in colors)
            {
                stb.Append(_ColorBinding[color]);
                stb.Append(":");
            }

            return stb.ToString();
        }
        public static Tuple<SolidColorBrush, HashSet<String>> GetColorInfoOrNull(String colorComboString)
        {
            String[] split = colorComboString.Split(',');

            String colorString = split[0];
            EnumDGColor color = (EnumDGColor)Enum.Parse(typeof(EnumDGColor), colorString);
            SolidColorBrush solidColorBrush = null;
            switch (color)
            {
                case EnumDGColor.LightGray:
                    solidColorBrush = Brushes.LightGray;
                    break;
                case EnumDGColor.LightGreen:
                    solidColorBrush = Brushes.LightGreen;
                    break;
                case EnumDGColor.OrangeRed:
                    solidColorBrush = Brushes.OrangeRed;
                    break;
                case EnumDGColor.Orange:
                    solidColorBrush = Brushes.Orange;
                    break;
                case EnumDGColor.White:
                    solidColorBrush = Brushes.White;
                    break;
                case EnumDGColor.Transparent:
                    solidColorBrush = Brushes.Transparent;
                    break;
            }

            String columnes = null;
            HashSet<String> columnesHash = null;
            if (split.Length > 1)
            {
                columnes = split[1];
                columnesHash = new HashSet<String>();

                String[] splitColumn = columnes.Split(':');

                foreach(String column in splitColumn)
                {
                    columnesHash.Add(column);
                }
            }

            return new Tuple<SolidColorBrush, HashSet<String>>(solidColorBrush, columnesHash);
        }
    }
}
