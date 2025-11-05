using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace ProberInterfaces.Foup
{
    [Serializable]
    [DataContract]
    public class FoupModuleInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private int _FoupNumber;
        [DataMember]
        public int FoupNumber
        {
            get { return _FoupNumber; }
            set { _FoupNumber = value; RaisePropertyChanged(); }
        }

        private bool _Enable = true;
        [DataMember]
        public bool Enable
        {
            get { return _Enable; }
            set { _Enable = value; RaisePropertyChanged(); }
        }

        private FoupStateEnum _State;
        [DataMember]
        public FoupStateEnum State
        {
            get { return _State; }
            set { _State = value; RaisePropertyChanged(); }
        }

        private FoupPermissionStateEnum _PermissionState;
        [DataMember]
        public FoupPermissionStateEnum PermissionState
        {
            get { return _PermissionState; }
            set { _PermissionState = value; RaisePropertyChanged(); }
        }

        private FoupCassetteOpenerStateEnum _OpenerState;
        [DataMember]
        public FoupCassetteOpenerStateEnum OpenerState
        {
            get { return _OpenerState; }
            set { _OpenerState = value; RaisePropertyChanged(); }
        }

        private DockingPortStateEnum _DockingPortState;
        [DataMember]
        public DockingPortStateEnum DockingPortState
        {
            get { return _DockingPortState; }
            set { _DockingPortState = value; RaisePropertyChanged(); }
        }
        private DockingPort40StateEnum _DockingPort40State;
        [DataMember]
        public DockingPort40StateEnum DockingPort40State
        {
            get { return _DockingPort40State; }
            set { _DockingPort40State = value; RaisePropertyChanged(); }
        }

        private DockingPortDoorStateEnum _DockingPortDoorState;
        [DataMember]
        public DockingPortDoorStateEnum DockingPortDoorState
        {
            get { return _DockingPortDoorState; }
            set { _DockingPortDoorState = value; RaisePropertyChanged(); }
        }

        private FoupCoverStateEnum _FoupCoverState;
        [DataMember]
        public FoupCoverStateEnum FoupCoverState
        {
            get { return _FoupCoverState; }
            set { _FoupCoverState = value; RaisePropertyChanged(); }
        }

        private DockingPlateStateEnum _DockingPlateState;
        [DataMember]
        public DockingPlateStateEnum DockingPlateState
        {
            get { return _DockingPlateState; }
            set { _DockingPlateState = value; RaisePropertyChanged(); }
        }

        private FoupTypeEnum _FoupTypeEnum;
        [DataMember]
        public FoupTypeEnum FoupTypeEnum
        {
            get { return _FoupTypeEnum; }
            set { _FoupTypeEnum = value; RaisePropertyChanged(); }
        }
        private TiltStateEnum _TiltState;
        [DataMember]
        public TiltStateEnum TiltState
        {
            get { return _TiltState; }
            set { _TiltState = value; RaisePropertyChanged(); }
        }
        private FoupPRESENCEStateEnum _FoupPRESENCEState;
        [DataMember]
        public FoupPRESENCEStateEnum FoupPRESENCEState
        {
            get { return _FoupPRESENCEState; }
            set { _FoupPRESENCEState = value; RaisePropertyChanged(); }
        }
        private Foup8IN_PRESENCEStateEnum _Foup8IN_PRESENCEState;
        [DataMember]
        public Foup8IN_PRESENCEStateEnum Foup8IN_PRESENCEState
        {
            get { return _Foup8IN_PRESENCEState; }
            set { _Foup8IN_PRESENCEState = value; RaisePropertyChanged(); }
        }
        private Foup6IN_PRESENCEStateEnum _Foup6IN_PRESENCEState;
        [DataMember]
        public Foup6IN_PRESENCEStateEnum Foup6IN_PRESENCEState
        {
            get { return _Foup6IN_PRESENCEState; }
            set { _Foup6IN_PRESENCEState = value; RaisePropertyChanged(); }
        }
        private Foup12IN_PRESENCEStateEnum _Foup12IN_PRESENCEState;
        [DataMember]
        public Foup12IN_PRESENCEStateEnum Foup12IN_PRESENCEState
        {
            get { return _Foup12IN_PRESENCEState; }
            set { _Foup12IN_PRESENCEState = value; RaisePropertyChanged(); }
        }
        private bool _PresenceLamp;
        [DataMember]
        public bool PresenceLamp
        {
            get { return _PresenceLamp; }
            set { _PresenceLamp = value; RaisePropertyChanged(); }
        }
        private bool _PlacementLamp;
        [DataMember]
        public bool PlacementLamp
        {
            get { return _PlacementLamp; }
            set { _PlacementLamp = value; RaisePropertyChanged(); }
        }
        private bool _LoadLamp;
        [DataMember]
        public bool LoadLamp
        {
            get { return _LoadLamp; }
            set { _LoadLamp = value; RaisePropertyChanged(); }
        }
        private bool _UnloadLamp;
        [DataMember]
        public bool UnloadLamp
        {
            get { return _UnloadLamp; }
            set { _UnloadLamp = value; RaisePropertyChanged(); }
        }
        private bool _BusyLamp;
        [DataMember]
        public bool BusyLamp
        {
            get { return _BusyLamp; }
            set { _BusyLamp = value; RaisePropertyChanged(); }
        }
        private bool _AutoLamp;
        [DataMember]
        public bool AutoLamp
        {
            get { return _AutoLamp; }
            set { _AutoLamp = value; RaisePropertyChanged(); }
        }
        private bool _AlarmLamp;
        [DataMember]
        public bool AlarmLamp
        {
            get { return _AlarmLamp; }
            set { _AlarmLamp = value; RaisePropertyChanged(); }
        }
        private bool _ReserveLamp;
        [DataMember]
        public bool ReserveLamp
        {
            get { return _ReserveLamp; }
            set { _ReserveLamp = value; RaisePropertyChanged(); }
        }
        private FoupWaferOutSensorStateEnum _WaferOutSensor;
        [DataMember]
        public FoupWaferOutSensorStateEnum WaferOutSensor
        {
            get { return _WaferOutSensor; }
            set { _WaferOutSensor = value; RaisePropertyChanged(); }
        }
        private FoupModeStatusEnum _FoupModeStatus;
        [DataMember]
        public FoupModeStatusEnum FoupModeStatus
        {
            get { return _FoupModeStatus; }
            set { _FoupModeStatus = value; RaisePropertyChanged(); }
        }

        private bool _IsCassetteAutoLock;
        [DataMember]
        public bool IsCassetteAutoLock
        {
            get { return _IsCassetteAutoLock; }
            set { _IsCassetteAutoLock = value; RaisePropertyChanged(); }
        }

        private bool _IsCassetteAutoLockLeftOHT;
        [DataMember]
        public bool IsCassetteAutoLockLeftOHT
        {
            get { return _IsCassetteAutoLockLeftOHT; }
            set { _IsCassetteAutoLockLeftOHT = value; RaisePropertyChanged(); }
        }

        private bool _IsChangedFoupMode;
        [DataMember]
        public bool IsChangedFoupMode
        {
            get { return _IsChangedFoupMode; }
            set { _IsChangedFoupMode = value; RaisePropertyChanged(); }
        }
    }


    public class FoupOptionInfomation
    {
        private bool _IsCassetteDetectEventAfterRFID;

        public bool IsCassetteDetectEventAfterRFID
        {
            get { return _IsCassetteDetectEventAfterRFID; }
            set { _IsCassetteDetectEventAfterRFID = value; }
        }

        public FoupOptionInfomation()
        {

        }
        public FoupOptionInfomation(bool isCassetteDetectEventAfterRFID)
        {
            IsCassetteDetectEventAfterRFID = isCassetteDetectEventAfterRFID;
        }
    }
}
