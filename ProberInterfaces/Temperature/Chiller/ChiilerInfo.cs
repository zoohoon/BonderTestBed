namespace ProberInterfaces.Temperature.Chiller
{
    using LogModule;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public enum CillerModeEnum
    {
        ONLINE,
        OFFLINE,
        MAINTANANCE,
    }

    public interface IChillerInfo
    {
        int Index { get; }
        CillerModeEnum ChillerMode { get; set; }
        string SerialNumber { get; }
        bool UseLimitChiller { get; set; }
        bool ActivdCoolantValve { get; set; }
        bool _PreActivdCoolantValve { get; }
        bool AbortChiller { get; set; }
        double TargetTemp { get; set; }
        bool CoolantActivate { get; set; }
        bool IsErrorState { get; }
        double ChillerInternalTemp { get; }
        double SetTemp { get;}
        double ChillerActiveableDewPointTolerance { get; set; }
        int ZeroTemp { get; }
        bool ChillerActiveStage { get; }
        bool SetOperationLockFlag { get; set; }
        bool IsOccupied { get; }
        int TargetTempSetStageIndex { get;  }
        double TargetTempOfStage { get;  }
        double ChillerTargetTempOfStage { get; }
        int ErrorReport { get; set; }
        ObservableCollection<int> WarningReport { get; set; }
        void SetActiveState(bool state);
    }
    public class ChillerInfo : IChillerInfo, INotifyPropertyChanged
    {
        #region ==> NotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        private int _Index;
        public int Index
        {
            get { return _Index; }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                    RaisePropertyChanged();
                }
            }
        }


        private CillerModeEnum _ChillerMode;
        /// <summary>
        /// ONLINE : Chiller 정상 동작 , 사용 가능.
        /// MAINTANANCE : (LOCK 상태) 어떠한 Chiller 커멘트도 받아 들이지 않는다.  Chiller 사용 불가능.
        /// </summary>
        public CillerModeEnum ChillerMode
        {
            get { return _ChillerMode; }
            set
            {
                if (value != _ChillerMode)
                {
                    _ChillerMode = value;
                    RaisePropertyChanged();
                }
            }
        }


        private string _SerialNumber;
        public string SerialNumber
        {
            get { return _SerialNumber; }
            set
            {
                if (_SerialNumber != value)
                {
                    _SerialNumber = value;
                    RaisePropertyChanged();
                }
            }
        }

        //false : can use chiller
        //true : can not use chiller.
        private bool _UseLimitChiller;
        public bool UseLimitChiller
        {
            get { return _UseLimitChiller; }
            set
            {
                if (value != _UseLimitChiller)
                {
                    _UseLimitChiller = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool _PreActivdCoolantValve { get; set; } = false;

        private bool _ActivdCoolantValve;
        //Coolant In , Out valve 가 열린것을 확인하기위해.
        public bool ActivdCoolantValve
        {
            get { return _ActivdCoolantValve; }
            set 
            {
                _PreActivdCoolantValve = _ActivdCoolantValve;
                _ActivdCoolantValve = value;
            }
        }

        private bool _AbortChiller = false;
        // 칠러에서 Stop 시켰을때 플래그 (이 플래그가 true 라면 스테이지에서 동작하면 안됨 또는 로더에서 스테이지를 판단)SetActiveState
        public bool AbortChiller
        {
            get { return _AbortChiller; }
            set { _AbortChiller = value; }
        }



        // 로직을 위한 타겟 온도 변수.
        private double _TargetTemp;
        public double TargetTemp
        {
            get { return _TargetTemp; }
            set
            {
                if (value != _TargetTemp)
                {
                    _TargetTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _CoolantActivate;
        public bool CoolantActivate
        {

            get { return _CoolantActivate; }
            set
            {
                if (_CoolantActivate != value)
                {
                    _CoolantActivate = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsErrorState = false;
        // Chiller 에서 Error 나 Warning Count 가 0이 아닌 경우 true 임.
        public bool IsErrorState
        {
            get { return _IsErrorState; }
            set
            {
                if (value != _IsErrorState)
                {
                    _IsErrorState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ChillerInternalTemp;
        public double ChillerInternalTemp
        {
            get { return _ChillerInternalTemp; }
            set
            {
                if (value != _ChillerInternalTemp)
                {
                    _ChillerInternalTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _SetTemp;
        public double SetTemp
        {
            get { return _SetTemp; }
            set
            {
                if (value != _SetTemp)
                {
                    _SetTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ChillerActiveableDewPointTolerance = 5;
        public double ChillerActiveableDewPointTolerance
        {
            get { return _ChillerActiveableDewPointTolerance; }
            set
            {
                if (value != _ChillerActiveableDewPointTolerance)
                {
                    _ChillerActiveableDewPointTolerance = value;
                    RaisePropertyChanged();
                }
            }
        }


        private int _ZeroTemp;
        public int ZeroTemp
        {
            get { return _ZeroTemp; }
            set
            {
                if (value != _ZeroTemp)
                {
                    _ZeroTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _ChillerActiveStage;

        public bool ChillerActiveStage
        {
            get { return _ChillerActiveStage; }
            private set { _ChillerActiveStage = value; }
        }


        private bool _SetOperationLockFlag;
        public bool SetOperationLockFlag
        {
            get { return _SetOperationLockFlag; }
            set
            {
                if (value != _SetOperationLockFlag)
                {
                    _SetOperationLockFlag = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool  _IsOccupied;
        public bool IsOccupied
        {
            get { return _IsOccupied; }
            private set
            {
                if (value != _IsOccupied)
                {
                    _IsOccupied = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _TargetTempSetStageIndex;
        // 칠러 온도를 설정한 셀의 Index 정보
        public int TargetTempSetStageIndex
        {
            get { return _TargetTempSetStageIndex; }
            private set
            {
                if (value != _TargetTempSetStageIndex)
                {
                    _TargetTempSetStageIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _TargetTempOfStage = -999;
        // 칠러 온도를 설정한 셀에서 설정 하고자 하는 온도 (셀의 SV)
        public double TargetTempOfStage
        {
            get { return _TargetTempOfStage; }
            private set
            {
                if (value != _TargetTempOfStage)
                {
                    _TargetTempOfStage = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ChillerTargetTempOfStage = -999;
        // 칠러 온도를 설정한 셀에서 설정 하고자 하는 온도 (칠러의 SV)
        public double ChillerTargetTempOfStage
        {
            get { return _ChillerTargetTempOfStage; }
            private set
            {
                if (value != _ChillerTargetTempOfStage)
                {
                    _ChillerTargetTempOfStage = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _ErrorReport;
        public int ErrorReport
        {
            get { return _ErrorReport; }
            set
            {
                if (value != _ErrorReport)
                {
                    _ErrorReport = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<int> _WarningReport
            = new ObservableCollection<int>();
        public ObservableCollection<int> WarningReport
        {
            get { return _WarningReport; }
            set
            {
                if (value != _WarningReport)
                {
                    _WarningReport = value;
                    RaisePropertyChanged();
                }
            }
        }

        public void SetTargetTempSetStageInfo(int index, double targetTemp, double chillerTargetTemp)
        {
            try
            {
                IsOccupied = true;

                TargetTempSetStageIndex = index;
                TargetTempOfStage = targetTemp;
                ChillerTargetTempOfStage = chillerTargetTemp;
                LoggerManager.Debug($"[Chiller #{Index}] SetTargetTempSetStageIndex set to {TargetTempSetStageIndex}, TargetTempOfStage set to {TargetTempOfStage}, ChillerTargetTempOfStage set to {ChillerTargetTempOfStage}.");
            }
            catch (System.Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ResetTargetTempSetStageInfo()
        {
            try
            {
                IsOccupied = false;

                TargetTempSetStageIndex = 0;
                TargetTempOfStage = -999;
                ChillerTargetTempOfStage = -999;
                LoggerManager.Debug($"[Chiller #{Index}] SetTargetTempSetStageIndex reset to {TargetTempSetStageIndex}, TargetTempOfStage set to {TargetTempOfStage}, ChillerTargetTempOfStage set to {ChillerTargetTempOfStage}.");
            }
            catch (System.Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ChangedSetTemp()
        {
            try
            {
                IsOccupied = false;
                LoggerManager.Debug($"[Chiller #{Index}] ChangedSetTemp() IsOccupied set to false.");
            }
            catch (System.Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetActiveState(bool state)
        {
            ChillerActiveStage = state;
        }
    }
}
