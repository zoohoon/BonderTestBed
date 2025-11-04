using System;
using System.Windows;
using System.Windows.Media;

namespace VirtualKeyboardControl
{
    public enum WindowLocationType
    {
        UNDEFINED = 0,
        LEFTOP,
        TOP,
        RIGHTTOP,
        LEFTCENTER,
        CENTER,
        RIGHTCENTER,
        BOTTOMLEFT,
        BOTTOM,
        BOTTOMRIGHT
    }
    /*
     * Virtual Keyboard를 Pop Up 윈도우로 띄우기 위한 Show 함수들로 구성되어 졌다.
     */
    public static class VirtualKeyboard
    {
        public static String Show(String curValue = "", KB_TYPE kbType = KB_TYPE.DECIMAL | KB_TYPE.FLOAT | KB_TYPE.ALPHABET | KB_TYPE.SPECIAL, int minCharLen = 0, int maxCharLen = 15)
        {
            if (minCharLen > maxCharLen)
                return String.Empty;

            UcVirtualKeyboard ucVirtualKeyboard = new UcVirtualKeyboard(curValue, kbType, minCharLen, maxCharLen);
            Window window = new Window()
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.None,
                Background = Brushes.Black,
                Height = 400,
                Width = 647,
                Content = ucVirtualKeyboard,
                Topmost = true
            };
            ucVirtualKeyboard.SetWindow(window);
            window.ShowDialog();

            return ucVirtualKeyboard.TextData;
        }
        public static String STA_Show(String curValue = "", KB_TYPE kbType = KB_TYPE.DECIMAL | KB_TYPE.FLOAT | KB_TYPE.ALPHABET | KB_TYPE.SPECIAL, int minCharLen = 0, int maxCharLen = 15)
        {
            String retVal = "";
            if (minCharLen > maxCharLen)
                return String.Empty;
            System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                UcVirtualKeyboard ucVirtualKeyboard = new UcVirtualKeyboard(curValue, kbType, minCharLen, maxCharLen);
               
                Window window = new Window()
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.None,
                Background = Brushes.Black,
                Height = 400,
                Width = 647,
                Content = ucVirtualKeyboard,
                Topmost = true
            };
            ucVirtualKeyboard.SetWindow(window);
            window.ShowDialog();
                retVal = ucVirtualKeyboard.TextData;
            })).Wait();
            return retVal;
        }
       
        public static int Show(int curValue, int minValue, int maxValue)
        {
            if (minValue > maxValue)
                return curValue;

            int minCharLen = 1;// minValue.ToString().Length;
            int maxCharLen = 32;// maxValue.ToString().Length;

            UcVirtualKeyboard ucVirtualKeyboard = new UcVirtualKeyboard(curValue.ToString(), KB_TYPE.DECIMAL, minCharLen, maxCharLen);
            Window window = new Window()
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.None,
                Background = Brushes.Black,
                Height = 400,
                Width = 647,
                Content = ucVirtualKeyboard,
                Topmost = true
            };
            ucVirtualKeyboard.SetWindow(window);
            window.ShowDialog();

            int retIntValue = 0;
            if (int.TryParse(ucVirtualKeyboard.TextData, out retIntValue) == false)
                return curValue;

            if (retIntValue < minValue)
                return minValue;

            if (retIntValue > maxValue)
                return maxValue;

            return retIntValue;
        }

        public static double Show(double curValue, double minValue, double maxValue)
        {
            if (minValue > maxValue)
                return curValue;

            int minCharLen = 1;
            int maxCharLen = 32;

            UcVirtualKeyboard ucVirtualKeyboard = new UcVirtualKeyboard(curValue.ToString(), KB_TYPE.DECIMAL | KB_TYPE.FLOAT, minCharLen, maxCharLen);
            Window window = new Window()
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.None,
                Background = Brushes.Black,
                Height = 400,
                Width = 647,
                Content = ucVirtualKeyboard,
                Topmost = true
            };
            ucVirtualKeyboard.SetWindow(window);
            window.ShowDialog();

            double retDblValue = 0;
            if (double.TryParse(ucVirtualKeyboard.TextData, out retDblValue) == false)
                return curValue;

            if (retDblValue < minValue)
                return minValue;

            if (retDblValue > maxValue)
                return maxValue;

            return retDblValue;
        }

        public static String Show(WindowLocationType locationtype, String curValue = "", KB_TYPE kbType = KB_TYPE.DECIMAL | KB_TYPE.ALPHABET, int minCharLen = 0, int maxCharLen = 15)
        {
            Window owner = Application.Current.MainWindow;

            double left = 0;
            double top = 0;
            double height = 400;
            double width = 647;

            switch (locationtype)
            {
                case WindowLocationType.UNDEFINED:
                    break;
                case WindowLocationType.LEFTOP:
                    break;
                case WindowLocationType.TOP:
                    left = (owner.ActualWidth - width) / 2;
                    top = 0;
                    break;
                case WindowLocationType.RIGHTTOP:
                    left = (owner.ActualWidth - width) / 2 + width;
                    top = 0;
                    break;
                case WindowLocationType.LEFTCENTER:
                    left = 0;
                    top = (owner.ActualHeight - height) / 2;
                    break;
                case WindowLocationType.CENTER:
                    left = (owner.ActualWidth - width) / 2;
                    top = (owner.ActualHeight - height) / 2;
                    break;
                case WindowLocationType.RIGHTCENTER:
                    left = (owner.ActualWidth - width) / 2 + width;
                    top = (owner.ActualHeight - height) / 2;
                    break;
                case WindowLocationType.BOTTOMLEFT:
                    left = 0;
                    top = (owner.ActualHeight - height);
                    break;
                case WindowLocationType.BOTTOM:
                    left = (owner.ActualWidth - width) / 2;
                    top = (owner.ActualHeight - height);
                    break;
                case WindowLocationType.BOTTOMRIGHT:
                    left = (owner.ActualWidth - width) / 2 + width;
                    top = (owner.ActualHeight - height);
                    break;
                default:
                    break;
            }

            double windowLeft = owner.Left + left;
            double windowTop = owner.Top + top;

            UcVirtualKeyboard ucVirtualKeyboard = new UcVirtualKeyboard(curValue, kbType, minCharLen, maxCharLen);
            Window window = new Window()
            {
                WindowStartupLocation = WindowStartupLocation.Manual,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.None,
                Background = Brushes.Black,
                Height = height,
                Width = width,
                Left = windowLeft,
                Top = windowTop,
                Content = ucVirtualKeyboard,
                Topmost = true,
                
            };
            ucVirtualKeyboard.SetWindow(window);
            window.ShowDialog();

            return ucVirtualKeyboard.TextData;
        }
    }
}
