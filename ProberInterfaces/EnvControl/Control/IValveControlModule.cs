
namespace ProberInterfaces
{
    using ProberErrorCode;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public interface IValveManager : IValveCore
    {
        EventCodeEnum SetEMGSTOP();
        ValveStateOfStage GetValveStateOfStage(int stageIndex);
    }

    public interface IValveController : IValveCore
    {
        void SetModbusCommDelayTime();
    }

    public interface IValveCore : IFactoryModule, IModule
    {
        bool GetValveState(EnumValveType valveType, int stageIndex = -1);
        EventCodeEnum SetValveState(bool state, EnumValveType valveType, int stageIndex = -1);
    }

    public class ValveStateOfStage : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public PropertyChangedEventHandler propertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged
        {
            add { this.propertyChanged += value; }
            remove { this.propertyChanged -= value; }
        }
        #endregion

        private int _StageIndex;

        public int StageIndex
        {
            get { return _StageIndex; }
            set { _StageIndex = value; }
        }

        private bool _CoolantValveState;

        public bool CoolantValveState
        {
            get { return _CoolantValveState; }
            set
            {
                if (value != _CoolantValveState)
                {
                    _CoolantValveState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _ChuckPurgeValveState;

        public bool ChuckPurgeValveState
        {
            get { return _ChuckPurgeValveState; }
            set
            {
                if (value != _ChuckPurgeValveState)
                {
                    _ChuckPurgeValveState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _DrainValveState;

        public bool DrainValveState
        {
            get { return _DrainValveState; }
            set
            {
                if (value != _DrainValveState)
                {
                    _DrainValveState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _PurgeValveState;

        public bool PurgeValveState
        {
            get { return _PurgeValveState; }
            set
            {
                if (value != _PurgeValveState)
                {
                    _PurgeValveState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _DryAirValveState;
        public bool DryAirValveState
        {
            get { return _DryAirValveState; }
            set
            {
                if (value != _DryAirValveState)
                {
                    _DryAirValveState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _LeakValveState;
        public bool LeakValveState
        {
            get { return _LeakValveState; }
            set
            {
                if (value != _LeakValveState)
                {
                    _LeakValveState = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ValveStateOfStage(int stageindex)
        {
            StageIndex = stageindex;
        }
    }
}
