using LogModule;
using ProberInterfaces;
using ProberInterfaces.PinAlign.ProbeCardData;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProbeCardObject
{
    [Serializable]
    public class DutPinPair : IDutPinPair, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private MachineIndex _DutMachineIndex;
        public MachineIndex DutMachineIndex
        {
            get { return _DutMachineIndex; }
            set
            {
                if (value != _DutMachineIndex)
                {
                    _DutMachineIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _PinNum;
        public int PinNum
        {
            get { return _PinNum; }
            set
            {
                if (value != _PinNum)
                {
                    _PinNum = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _Priority;
        public int Priority
        {
            get { return _Priority; }
            set
            {
                if (value != _Priority)
                {
                    _Priority = value;
                    RaisePropertyChanged();
                }
            }
        }

        public DutPinPair()
        {
            try
            {
            _DutMachineIndex = new MachineIndex();
            _PinNum = 0;
            _Priority = 9999;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public DutPinPair(DutPinPair data)
        {
            try
            {
            _DutMachineIndex = data.DutMachineIndex;
            _PinNum = data.PinNum;
            _Priority = data.Priority;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public DutPinPair(MachineIndex machineIndex, int pinnum)
        {
            try
            {
            _DutMachineIndex = new MachineIndex(machineIndex);
            _PinNum = pinnum;
            _Priority = 9999;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
    }
}
