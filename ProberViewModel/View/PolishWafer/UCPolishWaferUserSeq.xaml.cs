using ProberInterfaces;
using System;
using System.Windows.Controls;

namespace UCPolishWaferUserSeq
{
    /// <summary>
    /// UCPolishWaferUserSeq.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UCPolishWaferUserSeq : UserControl, IMainScreenView
    {
        public UCPolishWaferUserSeq()
        {
            InitializeComponent();
        }
        readonly Guid _ViewGUID = new Guid("914A446B-2FBA-4D5B-8174-2F336A1D26AC");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }
}

