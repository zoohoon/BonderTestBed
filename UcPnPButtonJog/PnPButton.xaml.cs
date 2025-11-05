using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace UcPnPButtonJog
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PnPButton : UserControl
    {
        public Guid GUID { get; set; }
        public bool Lockable { get; set; } = false;
        public bool InnerLockable { get; set; }
        public List<int> AvoidLockHashCodes { get; set; }
        public PnPButton()
        {
            InitializeComponent();
        }
    }
}
