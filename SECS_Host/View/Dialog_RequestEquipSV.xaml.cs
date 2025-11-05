using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using SECS_Host.Model.DynamicMSG;
using XComPro.Library;
using SECS_Host.Model;
using SECS_Host.Help;

namespace SECS_Host.View
{
    public partial class Dialog_RequestEquipSV : Window
    {
        View_Model.VM_DialogRequestEquipSV vm_DialogRequestEquipStatus;
        //private XComProW m_XComPro;
        //private SECS_MsgStructBase receivedData;

        private AddLogDelegate _AddLog;
        public AddLogDelegate AddLog
        {
            get { return _AddLog; }
            set
            {
                _AddLog = value;
                vm_DialogRequestEquipStatus.AddLog = value;
            }
        }

        public Dialog_RequestEquipSV(XComProW rXComPro, SECSData EquipStatusStruct, Action<string, object[]> addlog)
        {
            InitializeComponent();
            vm_DialogRequestEquipStatus = new View_Model.VM_DialogRequestEquipSV(rXComPro);
            vm_DialogRequestEquipStatus.CloseWindow = vm_CloseWindowEvent;
            vm_DialogRequestEquipStatus.SetEquipCollection(EquipStatusStruct);

            DataContext = vm_DialogRequestEquipStatus;
        }

        void vm_CloseWindowEvent(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            vm_DialogRequestEquipStatus.Dispose();
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach(char c in e.Text)
            {
                if(!char.IsDigit(c))
                {
                    e.Handled = true;
                    break;
                }
            }
        }
    }
}
