using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ProberInterfaces
{
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces.Command;
    using ProberInterfaces.State;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;

    public enum LotOPStateEnum
    {
        IDLE = 0,
        READYTORUNNING,
        READY_PAUSED_TO_RUNNING,
        RUNNING,
        PAUSING,
        PAUSED,
        ERROR,
        ABORTED,
        DONE,
    }
    public enum ErrorEndStateEnum
    {
        NONE,
        Reserve,
        Processing,
        PausedUnload,
        ReserveAbort,
        Unload,
        DONE
    }

    [ServiceContract]
    public interface ILotOPModule : IStateModule, INotifyPropertyChanged
    {
        IParam AppItems_IParam { get; set; }
        ILotInfo LotInfo { get; set; }
        ISystemInfo SystemInfo { get; set; }
        IDeviceInfo DeviceInfo { get; set; }
        ReasonOfStopOption ReasonOfStopOption { get; set; }
        ILotDeviceParam LotDeviceParam { get; }
        bool IsLastWafer { get; set; }

        bool IsNeedLotEnd { get; set; }
        bool ModuleStopFlag { get; set; }
        bool LotStartFlag { get; set; }
        bool LotEndFlag { get; set; }
        bool TransferReservationAboutPolishWafer { get; set; }
        int UnloadFoupNumber { get; set; }
        ErrorEndStateEnum ErrorEndState { get; set; }
        List<IStateModule> RunList { get; set; }
        //void LotOPStateTransition(LotOPState state);
        IInnerState InnerState { get; }
        LotOPStateEnum LotStateEnum { get; }
        object ViewTarget { get; set; }
        int PauseRequest(object caller);
        int ResumeRequest(object caller);
        int StartRequest(object caller);
        //void MakeResultMapData();
        void InitLotScreen();
        void VisionScreenToLotScreen();
        void MapScreenToLotScreen();
        void NCToLotScreen();
        void LoaderScreenToLotScreen();
        void ChangePreMainViewTarget();
        void HiddenLoaderScreenToLotScreen();
        void SetLotViewDisplayChannel();
        void ViewSwip();
        void ChangeMainViewUserTarget(object target);
        EventCodeEnum SaveAppItems();
        EventCodeEnum InitData();
        void ViewTargetUpdate();
        void UpdateWafer(IWaferObject waferObject);
        //void UpdateSlotState(List<int> slots);
        [OperationContract]
        bool IsServiceAvailable();

        [OperationContract]
        void SetDeviceName(string devicename);
        
        EventCodeInfo PauseSourceEvent { get; set; }
        [OperationContract]
        bool GetErrorEndFlag();
        [OperationContract]
        void SetErrorEndFalg(bool flag);
        [OperationContract]
        int GetLotPauseTimeoutAlarm();
        [OperationContract]
        void SetLotPauseTimeoutAlarm(int time);
        void SetErrorState();
        [OperationContract]
        bool IsLotAbortedByUser();
        void UpdateLotName(string lotname);

        void UpdateWaferID(string id);
        bool IsCanPerformLotStart();
        bool IsCanPerformLotEnd(int foupidx, string lotID, string cstHashCode, bool isCheckHashCode);
        void ValidateCancelLot(bool iscellend, int foupNumber, string lotID, string cstHashCode);
    }

    public abstract class LotOPState : IInnerState
    {
        public abstract bool CanExecute(IProbeCommandToken token);
        public abstract LotOPStateEnum GetState();

        public abstract ModuleStateEnum GetModuleState();
        public abstract EventCodeEnum Execute();
        public abstract EventCodeEnum Pause();

        public virtual EventCodeEnum End()
        {
            throw new NotImplementedException();
        }
        public virtual EventCodeEnum Abort()
        {
            return EventCodeEnum.NONE;
        }
        public abstract EventCodeEnum ClearState();

        public virtual EventCodeEnum Resume()
        {
            return EventCodeEnum.NONE;
        }
    }
    public interface IStage3DModel
    {
    }

    public interface ILotDeviceParam : IDeviceParameterizable
    {
        LotStopOption StopOption { get; set; }
        LotStopOption OperatorStopOption { get; set; }
    }

    [Serializable]
    public class LotStopOption : INotifyPropertyChanged, IParamNode
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ==> IParam Implement
        [JsonIgnore, ParamIgnore]
        public string Genealogy { get; set; } = "LotStopOption";
        [NonSerialized]
        private Object _Owner;
        [JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }
        [JsonIgnore]
        public List<object> Nodes { get; set; }
        #endregion

        public LotStopOption()
        {
        }

        private Element<bool> _StopAfterScanCassette = new Element<bool>();
        public Element<bool> StopAfterScanCassette
        {
            get { return _StopAfterScanCassette; }
            set
            {
                if (value != _StopAfterScanCassette)
                {
                    _StopAfterScanCassette = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _StopAfterWaferLoad = new Element<bool>();
        public Element<bool> StopAfterWaferLoad
        {
            get { return _StopAfterWaferLoad; }
            set
            {
                if (value != _StopAfterWaferLoad)
                {
                    _StopAfterWaferLoad = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<bool> _EveryStopBeforeProbing = new Element<bool>();
        public Element<bool> EveryStopBeforeProbing
        {
            get { return _EveryStopBeforeProbing; }
            set
            {
                if (value != _EveryStopBeforeProbing)
                {
                    _EveryStopBeforeProbing = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _EveryStopAfterProbing = new Element<bool>();
        public Element<bool> EveryStopAfterProbing
        {
            get { return _EveryStopAfterProbing; }
            set
            {
                if (value != _EveryStopAfterProbing)
                {
                    _EveryStopAfterProbing = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _OnceStopBeforeProbing = new Element<bool>();
        [JsonIgnore]
        public Element<bool> OnceStopBeforeProbing
        {
            get { return _OnceStopBeforeProbing; }
            set
            {
                if (value != _OnceStopBeforeProbing)
                {
                    _OnceStopBeforeProbing = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _StopBeforeProbing = new Element<bool>();
        [JsonIgnore]
        public Element<bool> StopBeforeProbing
        {
            get { return _StopBeforeProbing; }
            set
            {
                if (value != _StopBeforeProbing)
                {
                    _StopBeforeProbing = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _OnceStopAfterProbing = new Element<bool>();
        [JsonIgnore]
        public Element<bool> OnceStopAfterProbing
        {
            get { return _OnceStopAfterProbing; }
            set
            {
                if (value != _OnceStopAfterProbing)
                {
                    _OnceStopAfterProbing = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _StopAfterProbing = new Element<bool>();
        [JsonIgnore]
        public Element<bool> StopAfterProbing
        {
            get { return _StopAfterProbing; }
            set
            {
                if (value != _StopAfterProbing)
                {
                    _StopAfterProbing = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _StopBeforeRetest = new Element<bool>();
        public Element<bool> StopBeforeRetest
        {
            get { return _StopBeforeRetest; }
            set
            {
                if (value != _StopBeforeRetest)
                {
                    _StopBeforeRetest = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<ObservableCollection<bool>> _StopAfterProbingFlag = new Element<ObservableCollection<bool>>();
        public Element<ObservableCollection<bool>> StopAfterProbingFlag
        {
            get { return _StopAfterProbingFlag; }
            set
            {
                if (value != _StopAfterProbingFlag)
                {
                    _StopAfterProbingFlag = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<ObservableCollection<bool>> _StopBeforeProbingFlag = new Element<ObservableCollection<bool>>();
        public Element<ObservableCollection<bool>> StopBeforeProbingFlag
        {
            get { return _StopBeforeProbingFlag; }
            set
            {
                if (value != _StopBeforeProbingFlag)
                {
                    _StopBeforeProbingFlag = value;
                    RaisePropertyChanged();
                }
            }
        }

        public void CopyTo(LotStopOption lotStopOption)
        {
            this.StopAfterScanCassette.Value = lotStopOption.StopAfterScanCassette.Value;
            this.StopAfterWaferLoad.Value = lotStopOption.StopAfterWaferLoad.Value;


            this.EveryStopBeforeProbing.Value = lotStopOption.EveryStopBeforeProbing.Value;
            this.EveryStopAfterProbing.Value = lotStopOption.EveryStopAfterProbing.Value;


            this.StopBeforeProbing.Value = lotStopOption.StopBeforeProbing.Value;
            this.StopAfterProbing.Value = lotStopOption.StopAfterProbing.Value;

            this.StopBeforeRetest.Value = lotStopOption.StopBeforeRetest.Value;


            this.StopAfterProbingFlag.Value = new ObservableCollection<bool>();

            if (lotStopOption?.StopAfterProbingFlag != null)
            {
                foreach (var val in lotStopOption.StopAfterProbingFlag.Value)
                {
                    this.StopAfterProbingFlag.Value.Add(val);
                }
            }


            this.StopBeforeProbingFlag.Value = new ObservableCollection<bool>();
            if (lotStopOption?.StopBeforeProbingFlag != null)
            {
                foreach (var val in lotStopOption.StopBeforeProbingFlag.Value)
                {
                    this.StopBeforeProbingFlag.Value.Add(val);
                }
            }
        }
    }

}
