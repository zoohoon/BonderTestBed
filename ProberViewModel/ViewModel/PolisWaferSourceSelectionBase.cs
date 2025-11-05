using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using LoaderParameters;

namespace PolishWaferSourceSettingVM
{
    public class CassetteInfo : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private AsyncObservableCollection<ButtonDescriptor> _CassetteSlotBtn = new AsyncObservableCollection<ButtonDescriptor>();
        public AsyncObservableCollection<ButtonDescriptor> CassetteSlotBtn
        {
            get { return _CassetteSlotBtn; }
            set { _CassetteSlotBtn = value; }
        }
    }

    public class PolisWaferSourceSelectionBase : INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private ObservableCollection<CassetteInfo> _CassetteInfos = new ObservableCollection<CassetteInfo>();
        public ObservableCollection<CassetteInfo> CassetteInfos
        {
            get { return _CassetteInfos; }
            set
            {
                if (value != _CassetteInfos)
                {
                    _CassetteInfos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CassetteInfo _SelectedCassetteInfo;
        public CassetteInfo SelectedCassetteInfo
        {
            get { return _SelectedCassetteInfo; }
            set
            {
                if (value != _SelectedCassetteInfo)
                {
                    _SelectedCassetteInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AsyncObservableCollection<ButtonDescriptor> _FixedTrayInfos = new AsyncObservableCollection<ButtonDescriptor>();
        public AsyncObservableCollection<ButtonDescriptor> FixedTrayInfos
        {
            get { return _FixedTrayInfos; }
            set
            {
                if (value != _FixedTrayInfos)
                {
                    _FixedTrayInfos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AsyncObservableCollection<ButtonDescriptor> _InspectionTrayInfos = new AsyncObservableCollection<ButtonDescriptor>();
        public AsyncObservableCollection<ButtonDescriptor> InspectionTrayInfos
        {
            get { return _InspectionTrayInfos; }
            set
            {
                if (value != _InspectionTrayInfos)
                {
                    _InspectionTrayInfos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _SelectCassetteNumber;
        public int SelectCassetteNumber
        {
            get { return _SelectCassetteNumber; }
            set
            {
                if (value != _SelectCassetteNumber)
                {
                    _SelectCassetteNumber = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _CassetteCount;
        public int CassetteCount
        {
            get { return _CassetteCount; }
            set
            {
                if (value != _CassetteCount)
                {
                    _CassetteCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _FixedTrayCount;
        public int FixedTrayCount
        {
            get { return _FixedTrayCount; }
            set
            {
                if (value != _FixedTrayCount)
                {
                    _FixedTrayCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _InspectionTrayCount;
        public int InspectionTrayCount
        {
            get { return _InspectionTrayCount; }
            set
            {
                if (value != _InspectionTrayCount)
                {
                    _InspectionTrayCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        public PolisWaferSourceSelectionBase()
        {

        }

        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                var LoaderInfo = this.LoaderController().LoaderInfoObj as LoaderInfo;

                CassetteCount = LoaderInfo.StateMap.CassetteModules.Count();
                FixedTrayCount = LoaderInfo.StateMap.FixedTrayModules.Count();
                InspectionTrayCount = LoaderInfo.StateMap.InspectionTrayModules.Count();

                // Make Cassette Infos
                foreach (var cassette in LoaderInfo.StateMap.CassetteModules)
                {
                    CassetteInfos.Add(new CassetteInfo());

                    foreach (var slot in cassette.SlotModules)
                    {
                        this.CassetteInfos[CassetteInfos.Count - 1].CassetteSlotBtn.Add(new ButtonDescriptor());

                    }
                }

                SelectCassetteNumber = 0;

                SelectedCassetteInfo = this.CassetteInfos[SelectCassetteNumber];
                //

                // Make Fixed Tray Infos
                foreach (var fixedtray in LoaderInfo.StateMap.FixedTrayModules)
                {
                    FixedTrayInfos.Add(new ButtonDescriptor());
                }
                //

                // Make Inspection Tray Infos
                foreach (var inspectiontray in LoaderInfo.StateMap.InspectionTrayModules)
                {
                    InspectionTrayInfos.Add(new ButtonDescriptor());
                }
                //

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
}
