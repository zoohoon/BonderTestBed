using System;

namespace LoaderServiceClientModules.Inspection
{
    using ProberErrorCode;
    using ProberInterfaces;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;

    public class InspectionModuleServiceClient : IInspection, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //..Property
        public ObservableCollection<IStateModule> ModuleForInspectionCollection => throw new NotImplementedException();

        public Point MXYIndex { get; set; }
        private int _ManualSetIndexX;

        public int ManualSetIndexX
        {
            get { return _ManualSetIndexX; }
            set
            {
                if (value != _ManualSetIndexX)
                {
                    _ManualSetIndexX = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _ManualSetIndexY;

        public int ManualSetIndexY
        {
            get { return _ManualSetIndexY; }
            set
            {
                if (value != _ManualSetIndexY)
                {
                    _ManualSetIndexY = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _ManualSetIndexToggle;

        public bool ManualSetIndexToggle
        {
            get { return _ManualSetIndexToggle; }
            set
            {
                if (value != _ManualSetIndexToggle)
                {
                    _ManualSetIndexToggle = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _DUTCount;

        public int DUTCount
        {
            get { return _DUTCount; }
            set
            {
                if (value != _DUTCount)
                {
                    _DUTCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _PADCount;

        public int PADCount
        {
            get { return _PADCount; }
            set
            {
                if (value != _PADCount)
                {
                    _PADCount = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _CurPadIndex = 0;
        public int CurPadIndex
        {
            get { return _CurPadIndex; }
            set
            {
                if (value != _CurPadIndex)
                {
                    _CurPadIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _CurBeforePadIndex = 0;
        public int CurBeforePadIndex
        {
            get { return _CurBeforePadIndex; }
            set
            {
                if (value != _CurBeforePadIndex)
                {
                    _CurBeforePadIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _ViewPadIndex = 0;
        public int ViewPadIndex
        {
            get { return _ViewPadIndex; }
            set
            {
                if (value != _ViewPadIndex)
                {
                    _ViewPadIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _ViewDutIndex = 0;
        public int ViewDutIndex
        {
            get { return _ViewDutIndex; }
            set
            {
                if (value != _ViewDutIndex)
                {
                    _ViewDutIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _XDutStartIndexPoint = 0;
        public int XDutStartIndexPoint
        {
            get { return _XDutStartIndexPoint; }
            set
            {
                if (value != _XDutStartIndexPoint)
                {
                    _XDutStartIndexPoint = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _YDutStartIndexPoint = 0;
        public int YDutStartIndexPoint
        {
            get { return _YDutStartIndexPoint; }
            set
            {
                if (value != _YDutStartIndexPoint)
                {
                    _YDutStartIndexPoint = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _XSetFromCoord;
        public double XSetFromCoord
        {
            get
            {
                return _XSetFromCoord;
            }
            set
            {
                if (value != _XSetFromCoord && value != 0)
                {
                    _XSetFromCoord = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _YSetFromCoord;
        public double YSetFromCoord
        {
            get
            {
                return _YSetFromCoord;
            }
            set
            {
                if (value != _YSetFromCoord && value != 0)
                {
                    _YSetFromCoord = value;
                    RaisePropertyChanged();
                }
            }
        }
        public bool Initialized { get; set; }
        #endregion

        #region //..Method

        public void Apply()
        {
            return;
        }

        public void Clear()
        {
            return;
        }

        public void DecreaseX()
        {
            return;
        }

        public void DecreaseY()
        {
            return;
        }

        public void DeInitModule()
        {
            return;
        }

        public void DutStartPoint()
        {
            return;
        }

        public void IncreaseX()
        {
            return;
        }

        public void IncreaseY()
        {
            return;
        }

        public EventCodeEnum InitModule()
        {
            return EventCodeEnum.NONE;
        }

        public void MachinePositionUpdate()
        {
            return;
        }

        public void ManualSetIndex()
        {
            return;
        }

        public void NextDut(ICamera camera)
        {
            return;
        }

        public void NextPad(ICamera camera)
        {
            return;
        }

        public void PreDut(ICamera camera)
        {
            return;
        }

        public void PrePad(ICamera camera)
        {
            return;
        }

        public void SetFrom()
        {
            return;
        }

        public void ZoomIn()
        {
            return;
        }

        public void ZoomOut()
        {
            return;
        }

        public int SavePadImages(ICamera camera)
        {
            return -1;
        }
        public void CalculateOffsetFromCurrentZ(EnumProberCam camchannel)
        {
            return;
        }
        public EventCodeEnum CalculateOffsetToAutoFocusedPosition(ICamera curcam)
        {
            return EventCodeEnum.UNDEFINED;
        }
        #endregion
    }
}
