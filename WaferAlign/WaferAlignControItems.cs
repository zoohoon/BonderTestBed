namespace WaferAlign
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using ProberInterfaces;
    using ProberInterfaces.WaferAlignEX.Enum;

    public class WaferAlignControItems : IWaferAlignControItems, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool _IsManualRecoveryModifyMode;
        public bool IsManualRecoveryModifyMode
        {
            get { return _IsManualRecoveryModifyMode; }
            set
            {
                if (value != _IsManualRecoveryModifyMode)
                {
                    _IsManualRecoveryModifyMode = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _EdgeFail;
        public bool EdgeFail
        {
            get { return _EdgeFail; }
            set
            {
                if (value != _EdgeFail)
                {
                    _EdgeFail = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsDebugEdgeProcessing;
        public bool IsDebugEdgeProcessing
        {
            get { return _IsDebugEdgeProcessing; }
            set
            {
                if (value != _IsDebugEdgeProcessing)
                {
                    _IsDebugEdgeProcessing = value;
                    RaisePropertyChanged();
                }
            }
        }

        


        //private bool _EdgeFail = false;

        //public bool EdgeFail
        //{
        //    get { return _EdgeFail; }
        //    set { _EdgeFail = value; RaisePropertyChanged(); }
        //}

        private EnumLowStandardPosition _LowFailPos = EnumLowStandardPosition.UNDIFIND;

        public EnumLowStandardPosition LowFailPos
        {
            get { return _LowFailPos; }
            set { _LowFailPos = value; RaisePropertyChanged(); }
        }

        private EnumHighStandardPosition _HighFailPos = EnumHighStandardPosition.UNDIFIND;

        public EnumHighStandardPosition HighFailPos
        {
            get { return _HighFailPos; }
            set { _HighFailPos = value; RaisePropertyChanged(); }
        }

    }
}
