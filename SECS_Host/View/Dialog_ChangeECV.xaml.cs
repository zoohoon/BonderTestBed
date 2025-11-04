using SECS_Host.Help;
using SECS_Host.Model;
using System;
using System.Windows;
using XComPro.Library;

namespace SECS_Host.View
{
    public partial class Dialog_ChangeECV : Window
    {
        View_Model.VM_DialogChangeECV vm_DialogChangeECD;

        private AddLogDelegate _AddLog;
        public AddLogDelegate AddLog
        {
            get { return _AddLog; }
            set
            {
                _AddLog = value;
                vm_DialogChangeECD.AddLog = value;
            }
        }

        public Dialog_ChangeECV(XComProW rXComPro, SECSData ECDStatusStruct, AddLogDelegate addlog)
        {
            InitializeComponent();

            vm_DialogChangeECD = new View_Model.VM_DialogChangeECV(rXComPro);
            vm_DialogChangeECD.CloseWindow = vm_CloseWindowEvent;
            vm_DialogChangeECD.SetECDCollection(ECDStatusStruct);
            this.AddLog = addlog;

            DataContext = vm_DialogChangeECD;
        }

        void vm_CloseWindowEvent(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            vm_DialogChangeECD.Dispose();
        }
    }
}
