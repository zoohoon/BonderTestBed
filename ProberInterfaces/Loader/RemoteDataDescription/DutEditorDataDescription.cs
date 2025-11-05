namespace ProberInterfaces //Loader.RemoteDataDescription
{
    using System.Runtime.Serialization;
    [DataContract]
    public class DutEditorDataDescription
    {
        private double _ZoomLevel;
        [DataMember]
        public double ZoomLevel
        {
            get { return _ZoomLevel; }
            set { _ZoomLevel = value; }
        }

        private bool? _AddCheckBoxIsChecked;
        [DataMember]
        public bool? AddCheckBoxIsChecked
        {
            get { return _AddCheckBoxIsChecked; }
            set { _AddCheckBoxIsChecked = value; }
        }

        private bool? _ShowPad;
        [DataMember]
        public bool? ShowPad
        {
            get { return _ShowPad; }
            set { _ShowPad = value; }
        }

        private bool? _ShowPin;
        [DataMember]
        public bool? ShowPin
        {
            get { return _ShowPin; }
            set { _ShowPin = value; }
        }

        private bool? _EnableDragMap;
        [DataMember]
        public bool? EnableDragMap
        {
            get { return _EnableDragMap; }
            set { _EnableDragMap = value; }
        }

        private bool? _ShowSelectedDut;
        [DataMember]
        public bool? ShowSelectedDut
        {
            get { return _ShowSelectedDut; }
            set { _ShowSelectedDut = value; }
        }

        private bool? _ShowGrid;
        [DataMember]
        public bool? ShowGrid
        {
            get { return _ShowGrid; }
            set { _ShowGrid = value; }
        }

        private bool? _ShowCurrentPos;
        [DataMember]
        public bool? ShowCurrentPos
        {
            get { return _ShowCurrentPos; }
            set { _ShowCurrentPos = value; }
        }

        private MachineIndex _SelectedCoordM;
        [DataMember]
        public MachineIndex SelectedCoordM
        {
            get { return _SelectedCoordM; }
            set { _SelectedCoordM = value; }
        }

        private MachineIndex _FirstDutM;
        [DataMember]
        public MachineIndex FirstDutM
        {
            get { return _FirstDutM; }
            set { _FirstDutM = value; }
        }
    }
}
