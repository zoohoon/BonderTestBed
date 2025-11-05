using System;

namespace ProberInterfaces.WaferAlignEX
{
    using ProberInterfaces.Param;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    [Serializable]
    public class WARecoveryParam
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        [NonSerialized]
        private ObservableCollection<WAStandardPTInfomation> _TemporaryHighPatternBuffer
            = new ObservableCollection<WAStandardPTInfomation>();
        public ObservableCollection<WAStandardPTInfomation> TemporaryHighPatternBuffer
        {
            get { return _TemporaryHighPatternBuffer; }
            set
            {
                if (value != _TemporaryHighPatternBuffer)
                {
                    _TemporaryHighPatternBuffer = value;
                    RaisePropertyChanged();
                }
            }
        }
        [NonSerialized]
        private ObservableCollection<WAStandardPTInfomation> _TemporaryLowPatternBuffer
            = new ObservableCollection<WAStandardPTInfomation>();
        public ObservableCollection<WAStandardPTInfomation> TemporaryLowPatternBuffer
        {
            get { return _TemporaryLowPatternBuffer; }
            set
            {
                if (value != _TemporaryLowPatternBuffer)
                {
                    _TemporaryLowPatternBuffer = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private bool _SetRefPadFalg = false;
        public bool SetRefPadFalg
        {
            get { return _SetRefPadFalg; }
            set
            {
                if (value != _SetRefPadFalg)
                {
                    _SetRefPadFalg = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private bool _SetBoundaryFalg = false;
        public bool SetBoundaryFalg
        {
            get { return _SetBoundaryFalg; }
            set
            {
                if (value != _SetBoundaryFalg)
                {
                    _SetBoundaryFalg = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Point _OrgLCPoint = new Point();

        public Point OrgLCPoint
        {
            get { return _OrgLCPoint; }
            set { _OrgLCPoint = value; }
        }

        private Point _OrgRefPadPoint = new Point();

        public Point OrgRefPadPoint
        {
            get { return _OrgRefPadPoint; }
            set { _OrgRefPadPoint = value; }
        }

        private double _LCPointOffsetX = 0.0;

        public double LCPointOffsetX
        {
            get { return _LCPointOffsetX; }
            set { _LCPointOffsetX = value; }
        }

        private double _LCPointOffsetY = 0.0;

        public double LCPointOffsetY
        {
            get { return _LCPointOffsetY; }
            set { _LCPointOffsetY = value; }
        }

        private double _RefPadOffsetX = 0.0;

        public double RefPadOffsetX
        {
            get { return _RefPadOffsetX; }
            set { _RefPadOffsetX = value; }
        }

        private double _RefPadOffsetY = 0.0;

        public double RefPadOffsetY
        {
            get { return _RefPadOffsetY; }
            set { _RefPadOffsetY = value; }
        }

        private List<DUTPadObject> _BackupDutPadInfos
            = new List<DUTPadObject>();
        public List<DUTPadObject> BackupDutPadInfos
        {
            get { return _BackupDutPadInfos; }
            set
            {
                if (value != _BackupDutPadInfos)
                {
                    _BackupDutPadInfos = value;
                    RaisePropertyChanged();
                }
            }
        }
        public WARecoveryParam()
        {

        }

    }
}
