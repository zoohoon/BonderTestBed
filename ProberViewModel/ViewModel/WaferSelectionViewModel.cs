using System;
using System.Collections.Generic;
using System.Linq;

namespace PolishWaferDevMainPageViewModel.WaferSelection
{
    using LogModule;
    using ProberInterfaces;
    using RelayCommandBase;
    using System.Collections.ObjectModel;
    using WaferSelectSetup;

    public class WaferSelectionViewModel : WaferSelectSetupBase, IFactoryModule
    {
        #region //..Property
        private AsyncObservableCollection<ButtonDescriptor> _InspectionTraySlots
       = new AsyncObservableCollection<ButtonDescriptor>();
        public AsyncObservableCollection<ButtonDescriptor> InspectionTraySlots
        {
            get { return _InspectionTraySlots; }
            set
            {
                if (value != _InspectionTraySlots)
                {
                    _InspectionTraySlots = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ButtonDescriptor _InspectionTraySlot
             = new ButtonDescriptor();
        public ButtonDescriptor InspectionTraySlot
        {
            get { return _InspectionTraySlot; }
            set
            {
                if (value != _InspectionTraySlot)
                {
                    _InspectionTraySlot = value;
                    RaisePropertyChanged();
                }
            }
        }


        private AsyncObservableCollection<ButtonDescriptor> _FixedTraySlots
             = new AsyncObservableCollection<ButtonDescriptor>();
        public AsyncObservableCollection<ButtonDescriptor> FixedTraySlots
        {
            get { return _FixedTraySlots; }
            set
            {
                if (value != _FixedTraySlots)
                {
                    _FixedTraySlots = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AsyncObservableCollection<ButtonDescriptor> _WaferSlots
             = new AsyncObservableCollection<ButtonDescriptor>();
        public AsyncObservableCollection<ButtonDescriptor> WaferSlots
        {
            get { return _WaferSlots; }
            set
            {
                if (value != _WaferSlots)
                {
                    _WaferSlots = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion


        public WaferSelectionViewModel()
        {
            //WaferSlots
            for (int i = 0; i < 25; i++)
            {
                WaferSlots.Add(new ButtonDescriptor());
            }
            WaferSelectBtn = new ObservableCollection<ButtonDescriptor>(WaferSlots);

            for (int i = 0; i < 25; i++)
            {
                WaferSelectBtn[i].Command = new RelayCommand<Object>(SelectWaferIndexCommand);
                WaferSelectBtn[i].CommandParameter = i;
                //if (PinAlignParam.PinAlignInterval != null)
                //    WaferSelectBtn[i].isChecked = PinAlignParam.PinAlignInterval.WaferInterval[i].Value;
            }

            //IspectionTraySlots
            InspectionTraySlots.Add(new ButtonDescriptor());
            InspectionTraySlot = new ButtonDescriptor();
            InspectionTraySlot.Command = new RelayCommand<Object>(SelectWaferIndexCommand);
            InspectionTraySlot.CommandParameter = 0;
            
            //FixedTraySlots
            for (int i =0; i < 5; i++)
            {
                FixedTraySlots.Add(new ButtonDescriptor());
                FixedTraySlots[FixedTraySlots.Count()-1].Command = new RelayCommand<Object>(SelectWaferIndexCommand);
                FixedTraySlots[FixedTraySlots.Count() - 1].CommandParameter = i;
            }

            SelectAllBtn.Command = new RelayCommand<Object>(SetAllWaferUsingCommand);
            SelectAllBtn.CommandParameter = true;

            ClearAllBtn.Command = new RelayCommand<Object>(SetAllWaferUsingCommand);
            ClearAllBtn.CommandParameter = false;

        }

        public void SelectWaferIndexCommand(Object index)
        {
            try
            {
                int count1 = GetFixedTraySlotSelectedIndexs().Count;
                int count2 = GetWaferSlotSelectedIndexs().Count;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[WaferPMIMapSetupControlService] [SelectWaferIndexCommand()] : {err}");
            }
        }

        public void SetAllWaferUsingCommand(Object Using)
        {
            try
            {
                bool flag = (bool)Using;

                for (int i = 0; i < 25; i++)
                {
                    //PinAlignParam.PinAlignInterval.WaferInterval[i].SetValue(flag);
                    WaferSelectBtn[i].isChecked = flag;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[WaferPMIMapSetupControlService] [SetAllWaferUsingCommand()] : {err}");
            }
        }


        public List<int> GetFixedTraySlotSelectedIndexs()
        {
            List<int> retVal = new List<int>();
            try
            {
                foreach (var fixedtray in FixedTraySlots)
                {
                    if (fixedtray.isChecked)
                        retVal.Add((int)fixedtray.CommandParameter);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public List<int> GetWaferSlotSelectedIndexs()
        {
            List<int> retVal = new List<int>();
            try
            {
                foreach (var wafer in WaferSlots)
                {
                    if (wafer.isChecked)
                        retVal.Add((int)wafer.CommandParameter);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public bool GetInspectionTarySelected()
        {
            return InspectionTraySlot.isChecked;
        }
    }
}
