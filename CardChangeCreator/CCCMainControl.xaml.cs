using LogModule;
using System;
using System.Windows;
using System.Windows.Controls;

namespace CardChangeCreator
{
    /// <summary>
    /// CCCMainControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CCCMainControl : UserControl
    {
        public CCCMainControl()
        {
            InitializeComponent();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (tabCardChange.SelectedIndex == 0)
                {
                    int retVal = machineControl.Next();

                    if (retVal == 0)
                    {
                        machineControl.SaveMachineBaseFile();
                        tabCardChange.SelectedIndex = 1;
                    }
                }
                else if (tabCardChange.SelectedIndex == 1)
                {
                    int retVal = TesterDockingControl.Next();

                    if (retVal == 0)
                    {
                        TesterDockingControl.SaveMachineBaseFile("Dock");
                        tabCardChange.SelectedIndex = 2;
                    }
                }
                else if (tabCardChange.SelectedIndex == 2)
                {
                    int retVal = TesterUndockingControl.Next();

                    if (retVal == 0)
                    {
                        TesterUndockingControl.SaveMachineBaseFile("Undock");
                        tabCardChange.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception err)
            {

                LoggerManager.Error($"Method(): Error occurred. Err = {err.Message}");
            }
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (tabCardChange.SelectedIndex == 0)
                {
                    int retVal = machineControl.Prev();

                    if (retVal == 2)
                        tabCardChange.SelectedIndex = 2;
                }
                else if (tabCardChange.SelectedIndex == 1)
                {
                    int retVal = TesterDockingControl.Prev();

                    if (retVal == 2)
                        tabCardChange.SelectedIndex = 0;
                }
                else if (tabCardChange.SelectedIndex == 2)
                {
                    int retVal = TesterUndockingControl.Prev();

                    if (retVal == 2)
                        tabCardChange.SelectedIndex = 1;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }
}
