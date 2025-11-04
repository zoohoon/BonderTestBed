using System;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using System.Threading;
using System.Globalization;
using System.Windows.Markup;


namespace ProberSystem.Theme
{
    using CUIServices;
    using LogModule;
    //==========
    //==Using MessageBox
    using ProberInterfaces;

    //==========
    /// <summary>
    /// Interaction logic for AccentStyleWindow.xaml
    /// </summary>
    public partial class AccentStyleWindow : MetroWindow
    {
        //public static readonly DependencyProperty ColorsProperty
        //    = DependencyProperty.Register("Colors",
        //                                  typeof(List<KeyValuePair<string, Color>>),
        //                                  typeof(AccentStyleWindow),
        //                                  new PropertyMetadata(default(List<KeyValuePair<string, Color>>)));

        //public List<KeyValuePair<string, Color>> Colors
        //{
        //    get { return (List<KeyValuePair<string, Color>>)GetValue(ColorsProperty); }
        //    set { SetValue(ColorsProperty, value); }
        //}

        //public static readonly DependencyProperty CurLanguageProperty
        //    = DependencyProperty.Register("CurLanguage",
        //                                  typeof(List<EnumLangs>),
        //                                  typeof(AccentStyleWindow),
        //                                  new PropertyMetadata(default(List<EnumLangs>)));

        //public List<EnumLangs> CurLanguage
        //{
        //    get { return (List<EnumLangs>)GetValue(CurLanguageProperty); }
        //    set { SetValue(CurLanguageProperty, value); }
        //}


        //==========
        //==Using MessageBox
        private IStageSupervisor StageSupervisor;
        //==========

        public AccentStyleWindow(IStageSupervisor supervisor)
        {
            try
            {
                InitializeComponent();
                StageSupervisor = supervisor;

                //this.DataContext = this;

                //this.Colors = typeof(Colors)
                //    .GetProperties()
                //    .Where(prop => typeof(Color).IsAssignableFrom(prop.PropertyType))
                //    .Select(prop => new KeyValuePair<String, Color>(prop.Name, (Color)prop.GetValue(null)))
                //    .ToList();

                //var theme = ThemeManager.DetectAppStyle(Application.Current);
                //ThemeManager.ChangeAppStyle(this, theme.Item2, theme.Item1);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        //private void ChangeWindowThemeButtonClick(object sender, RoutedEventArgs e)
        //{
        //    var theme = ThemeManager.DetectAppStyle(this);
        //    ThemeManager.ChangeAppStyle(this, theme.Item2, ThemeManager.GetAppTheme("Base" + ((Button)sender).Content));
        //}

        //private void ChangeWindowAccentButtonClick(object sender, RoutedEventArgs e)
        //{
        //    var theme = ThemeManager.DetectAppStyle(this);
        //    ThemeManager.ChangeAppStyle(this, ThemeManager.GetAccent(((Button)sender).Content.ToString()), theme.Item1);
        //}

        //private void ChangeAppThemeButtonClick(object sender, RoutedEventArgs e)
        //{
        //    var theme = ThemeManager.DetectAppStyle(Application.Current);
        //    ThemeManager.ChangeAppStyle(Application.Current, theme.Item2, ThemeManager.GetAppTheme("Base" + ((Button)sender).Content));

        //}

        //private void ChangeAppAccentButtonClick(object sender, RoutedEventArgs e)
        //{
        //    var theme = ThemeManager.DetectAppStyle(Application.Current);
        //    ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(((Button)sender).Content.ToString()), theme.Item1);
        //}

        //private void DarkThemeAppButtonClick(object sender, RoutedEventArgs e)
        //{
        //    var theme = ThemeManager.DetectAppStyle(Application.Current);
        //    ThemeManager.ChangeAppStyle(Application.Current, theme.Item2, ThemeManager.GetAppTheme("DarkTheme"));
        //}


        //private void AccentSelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    var selectedAccent = AccentSelector.SelectedItem as Accent;
        //    if (selectedAccent != null)
        //    {
        //        var theme = ThemeManager.DetectAppStyle(Application.Current);
        //        ThemeManager.ChangeAppStyle(Application.Current, selectedAccent, theme.Item1);
        //        Application.Current.MainWindow.Activate();
        //    }
        //}

        //private void ColorsSelectorOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    var selectedColor = this.ColorsSelector.SelectedItem as KeyValuePair<string, Color>?;
        //    if (selectedColor.HasValue)
        //    {
        //        var theme = ThemeManager.DetectAppStyle(Application.Current);
        //        ThemeManagerHelper.CreateAppStyleBy(selectedColor.Value.Value, true);
        //        Application.Current.MainWindow.Activate();
        //    }
        //}

        private void LanguageSelectorOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                int selectedLanguage = LanguageSelector.SelectedIndex; //this.LanguageSelector.SelectedItem as EnumLangs?;

                int lcid;

                if (selectedLanguage == 0)
                {
                    lcid = 1033;
                }
                else if (selectedLanguage == 1)
                {
                    lcid = 1028;
                }
                else if (selectedLanguage == 2)
                {
                    lcid = 2052;
                }
                else if (selectedLanguage == 3)
                {
                    lcid = 1042;
                }
                else if (selectedLanguage == 4)
                {
                    lcid = 1041;
                }
                else if (selectedLanguage == 5)
                {
                    lcid = 1036;
                }
                else
                {
                    lcid = 1033;
                }

                //int lcid = (int)selectedLanguage;
                //Properties.Settings.Default.Language = lcid;
                //Properties.Settings.Default.Save();

                CUIService.param.lcid = lcid;
                CUIService.SaveCUIParam();

                //Thread.CurrentThread.CurrentUICulture = new CultureInfo((int)Properties.Settings.Default["Language"]);
                //Thread.CurrentThread.CurrentCulture = new CultureInfo((int)Properties.Settings.Default["Language"]);

                Thread.CurrentThread.CurrentUICulture = new CultureInfo(CUIService.param.lcid);
                Thread.CurrentThread.CurrentCulture = new CultureInfo(CUIService.param.lcid);

                this.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
                Application.Current.MainWindow.Activate();

                //StageSupervisor.ShowMessageBox("", ProberSystem.Properties.Resources.ChangeLanguageCheckMessage);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        
    }
}
