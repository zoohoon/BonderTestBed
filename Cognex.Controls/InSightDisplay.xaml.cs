using LogModule;
using System;
using System.Windows.Controls;

namespace Cognex.Controls
{
    /// <summary>
    /// Interaction logic for InSightDisplay.xaml
    /// </summary>
    public partial class InSightDisplay : UserControl
    {
        public CvsInSightDisplayHost CogDisplay { get; set; }
        public InSightDisplay()
        {
            try
            {
            InitializeComponent();

            //CogDisplay = new CvsInSightDisplayHost();
            //WindowsFormsHost winFormHost = new WindowsFormsHost();
            //winFormHost.Child = CogDisplay;
            //this.maingrid.Children.Add(winFormHost);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }       
    }
}
