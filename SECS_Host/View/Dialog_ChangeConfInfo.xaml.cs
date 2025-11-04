using SECS_Host.View_Model;
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
using XComPro.Library;

namespace SECS_Host.View
{
    public partial class Dialog_ChangeConnInfo : Window
    {
        public VM_DialogChangeConnInfo vm_DialogConnInfo;
        
        public Dialog_ChangeConnInfo()
        {
            InitializeComponent();
        }

        public Dialog_ChangeConnInfo(XComProW rXComPro)
        {
            InitializeComponent();
            vm_DialogConnInfo = new VM_DialogChangeConnInfo(rXComPro);
            vm_DialogConnInfo.CloseWindow = vm_CloseWindowEvent;
            DataContext = vm_DialogConnInfo;
        }

        void vm_CloseWindowEvent(object sender, System.EventArgs e)
        {
            this.Close();
        }
    }
}
