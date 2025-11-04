namespace ProberInterfaces //Loader.RemoteDataDescription
{
    using System.Runtime.Serialization;
    [DataContract]
    public class InspcetionDataDescription
    {
        private bool _SetFromToggle;
        [DataMember]
        public bool SetFromToggle
        {
            get { return _SetFromToggle; }
            set { _SetFromToggle = value; }
        }

        private int _InspectionViewPadIndex;
        [DataMember]
        public int InspectionViewPadIndex
        {
            get { return _InspectionViewPadIndex; }
            set { _InspectionViewPadIndex = value; }
        }

        private int _InspectionViewDutIndex;
        [DataMember]
        public int InspectionViewDutIndex
        {
            get { return _InspectionViewDutIndex; }
            set { _InspectionViewDutIndex = value; }
        }

        private int _inspectionManualSetIndexX;
        [DataMember]
        public int inspectionManualSetIndexX
        {
            get { return _inspectionManualSetIndexX; }
            set { _inspectionManualSetIndexX = value; }
        }

        private int _inspectionManualSetIndexY;
        [DataMember]
        public int inspectionManualSetIndexY
        {
            get { return _inspectionManualSetIndexY; }
            set { _inspectionManualSetIndexY = value; }
        }

        private double _SystemXShiftValue;
        [DataMember]
        public double SystemXShiftValue
        {
            get { return _SystemXShiftValue; }
            set { _SystemXShiftValue = value; }
        }

        private double _SystemYShiftValue;
        [DataMember]
        public double SystemYShiftValue
        {
            get { return _SystemYShiftValue; }
            set { _SystemYShiftValue = value; }
        }

        private double _UserXShiftValue;
        [DataMember]
        public double UserXShiftValue
        {
            get { return _UserXShiftValue; }
            set { _UserXShiftValue = value; }
        }

        private double _UserYShiftValue;
        [DataMember]
        public double UserYShiftValue
        {
            get { return _UserYShiftValue; }
            set { _UserYShiftValue = value; }
        }

        private int _InspcetionDutCount;
        [DataMember]
        public int InspcetionDutCount
        {
            get { return _InspcetionDutCount; }
            set { _InspcetionDutCount = value; }
        }

        private int _InspcetionPADCount;
        [DataMember]
        public int InspcetionPADCount
        {
            get { return _InspcetionPADCount; }
            set { _InspcetionPADCount = value; }
        }

        private int _InspcetoinXDutStartIndexPoint;
        [DataMember]
        public int InspcetoinXDutStartIndexPoint
        {
            get { return _InspcetoinXDutStartIndexPoint; }
            set { _InspcetoinXDutStartIndexPoint = value; }
        }

        private int _InspectionYDutStartIndexPoint;
        [DataMember]
        public int InspectionYDutStartIndexPoint
        {
            get { return _InspectionYDutStartIndexPoint; }
            set { _InspectionYDutStartIndexPoint = value; }
        }

        private bool _InspectionManualSetIndexToggle;
        [DataMember]
        public bool InspectionManualSetIndexToggle
        {
            get { return _InspectionManualSetIndexToggle; }
            set { _InspectionManualSetIndexToggle = value; }
        }

        private bool _ToggleSetIndex;
        [DataMember]
        public bool ToggleSetIndex
        {
            get { return _ToggleSetIndex; }
            set { _ToggleSetIndex = value; }
        }
        private long _MapIndexX;
        [DataMember]
        public long MapIndexX
        {
            get { return _MapIndexX; }
            set { _MapIndexX = value; }
        }

        private long _MapIndexY;
        [DataMember]
        public long MapIndexY
        {
            get { return _MapIndexY; }
            set { _MapIndexY = value; }
        }

        private double _XSetFromCoord;
        [DataMember]
        public double XSetFromCoord
        {
            get { return _XSetFromCoord; }
            set { _XSetFromCoord = value; }
        }

        private double _YSetFromCoord;
        [DataMember]
        public double YSetFromCoord
        {
            get { return _YSetFromCoord; }
            set { _YSetFromCoord = value; }
        }
    }
}
