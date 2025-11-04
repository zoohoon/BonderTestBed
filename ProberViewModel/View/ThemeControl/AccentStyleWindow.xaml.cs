using System;
using System.Windows.Controls;
using LogModule;

namespace ThemeControl
{
    using MahApps.Metro.Controls;
    using ProberInterfaces;

    /// <summary>
    /// Interaction logic for AccentStyleWindow.xaml
    /// </summary>
    public partial class AccentStyleWindow : MetroWindow
    {
        private IStageSupervisor StageSupervisor;

        public AccentStyleWindow()
        {
            InitializeComponent();
        }
        public AccentStyleWindow(IStageSupervisor supervisor)
        {
            try
            {
                InitializeComponent();
                StageSupervisor = supervisor;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
        private void LanguageSelectorOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                /*
                //brett// 다국어 language 사용시 주석 해제 필요
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
                
                //Properties.Settings.Default.Language = lcid;
                //Properties.Settings.Default.Save();

                //Thread.CurrentThread.CurrentUICulture = new CultureInfo((int)Properties.Settings.Default["Language"]);
                //Thread.CurrentThread.CurrentCulture = new CultureInfo((int)Properties.Settings.Default["Language"]);
                //this.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
                //Application.Current.MainWindow.Activate();
                */
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }
}
