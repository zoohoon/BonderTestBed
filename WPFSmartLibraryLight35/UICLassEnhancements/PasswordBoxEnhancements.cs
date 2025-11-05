//	--------------------------------------------------------------------
//		Member of the WPFSmartLibrary
//		For more information see : http://wpfsmartlibrary.codeplex.com/
//		(by DotNetMastermind)
//
//		filename		: PasswordBoxEnhancements.cs
//		namespace	: SoftArcs.WPFSmartLibrary.UIClassEnhancements
//		class(es)	: PasswordBoxEnhancements
//							
//	--------------------------------------------------------------------
using LogModule;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SoftArcs.WPFSmartLibrary.UIClassEnhancements
{
    class PasswordBoxEnhancements
    {
        public static void AddAutoSelectToAllPasswordBoxes()
        {
            try
            {
                EventManager.RegisterClassHandler(typeof(PasswordBox), UIElement.GotFocusEvent,
                                                              new RoutedEventHandler(PasswordBox_GotFocus));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private static void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                var passwordBox = sender as PasswordBox;

                if (passwordBox != null)
                {
                    passwordBox.SelectAll();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
