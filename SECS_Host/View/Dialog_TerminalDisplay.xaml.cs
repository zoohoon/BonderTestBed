using SECS_Host.Help;
using SECS_Host.View_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using XComPro.Library;

namespace SECS_Host.View
{
    public partial class Dialog_TerminalDisplay : Window
    {
        public VM_DialogTerminalDisplay vm_DialogTerminalDisplay;
        private AddLogDelegate _AddLog;
        public AddLogDelegate AddLog
        {
            get { return _AddLog; }
            set
            {
                _AddLog = value;
                vm_DialogTerminalDisplay.AddLog = value;
            }
        }

        public Dialog_TerminalDisplay(XComProW rXComPro, AddLogDelegate addlog)
        {
            InitializeComponent();

            vm_DialogTerminalDisplay = new VM_DialogTerminalDisplay(rXComPro);
            vm_DialogTerminalDisplay.CloseWindow = vm_CloseWindowEvent;
            this.AddLog = addlog;
            this.DataContext = vm_DialogTerminalDisplay;
        }

        void vm_CloseWindowEvent(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            vm_DialogTerminalDisplay.Dispose();
        }
    }
}
