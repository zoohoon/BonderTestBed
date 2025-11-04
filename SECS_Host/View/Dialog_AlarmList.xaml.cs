using SECS_Host.Help;
using SECS_Host.Model;
using System;
using System.Windows;
using XComPro.Library;

namespace SECS_Host.View
{
    public partial class Dialog_AlarmList : Window
    {
        View_Model.VM_DialogAlarmList vm_DialogAlarmList;

        private AddLogDelegate _AddLog;
        public AddLogDelegate AddLog
        {
            get { return _AddLog; }
            set
            {
                _AddLog = value;
                vm_DialogAlarmList.AddLog = value;
            }
        }

        public Dialog_AlarmList(XComProW rXComPro, SECSData AlarmStruct, AddLogDelegate addlog)
        {
            InitializeComponent();

            vm_DialogAlarmList = new View_Model.VM_DialogAlarmList(rXComPro);
            vm_DialogAlarmList.CloseWindow = vm_CloseWindowEvent;
            vm_DialogAlarmList.SetAlarmCollection(AlarmStruct);
            this.AddLog = addlog;

            DataContext = vm_DialogAlarmList;
        }

        void vm_CloseWindowEvent(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            vm_DialogAlarmList.Dispose();
        }
    }
}
