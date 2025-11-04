using System;
using System.Collections.Generic;
using System.Linq;

namespace ColorPaletteControl
{
    using LogModule;
    using System.Windows.Media;
    public static class ColorPalette
    {
        private static Dictionary<int, Brush> InitColor()
        {
            Dictionary<int, Brush> colorBrushList = new Dictionary<int, Brush>();
            try
            {
                colorBrushList.Add(0, Brushes.Black);
                colorBrushList.Add(1, Brushes.DarkBlue);
                colorBrushList.Add(2, Brushes.DarkGreen);
                colorBrushList.Add(3, Brushes.DarkCyan);
                colorBrushList.Add(4, Brushes.DarkRed);
                colorBrushList.Add(5, Brushes.DarkMagenta);
                colorBrushList.Add(6, Brushes.LightYellow);
                colorBrushList.Add(7, Brushes.Gray);
                colorBrushList.Add(8, Brushes.DarkGray);
                colorBrushList.Add(9, Brushes.Blue);
                colorBrushList.Add(10, Brushes.Green);
                colorBrushList.Add(11, Brushes.Cyan);
                colorBrushList.Add(12, Brushes.Red);
                colorBrushList.Add(13, Brushes.Magenta);
                colorBrushList.Add(14, Brushes.Yellow);
                colorBrushList.Add(15, Brushes.White);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return colorBrushList;
        }

        public static Brush Show()
        {
            MainWindow cp = new MainWindow(InitColor());
            try
            {
                cp.ShowDialog();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return cp.SelectBrush;
        }


        public static Brush ConvertToBrush(int color)
        {
            try
            {
                Dictionary<int, Brush> colorBrushList = InitColor();
                if (colorBrushList.ContainsKey(color) == false)
                    return Brushes.Peru;

                return colorBrushList[color];
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public static int ConvertToInt(Brush brush)
        {
            try
            {
                Dictionary<int, Brush> colorBrushList = InitColor();
                KeyValuePair<int, Brush> brushItem = colorBrushList.Where(item => item.Value == brush).FirstOrDefault();

                if (brushItem.Equals(default(KeyValuePair<int, Brush>)))
                    return -1;

                return brushItem.Key;
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
