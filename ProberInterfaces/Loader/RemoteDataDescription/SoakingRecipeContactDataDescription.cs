namespace ProberInterfaces
{
    using System.Runtime.Serialization;
    [DataContract]
    public class SoakingRecipeDataDescription
    {
        private string _SelectedItem;
        [DataMember]
        public string SelectedItem
        {
            get { return _SelectedItem; }
            set { _SelectedItem = value; }
        }

        //private double _ProbingModuleFirstContactHeight;
        //[DataMember]
        //public double ProbingModuleFirstContactHeight
        //{
        //    get { return _ProbingModuleFirstContactHeight; }
        //    set { _ProbingModuleFirstContactHeight = value; }
        //}

        //private double _ProbingModuleAllContactHeight;
        //[DataMember]
        //public double ProbingModuleAllContactHeight
        //{
        //    get { return _ProbingModuleAllContactHeight; }
        //    set { _ProbingModuleAllContactHeight = value; }
        //}

        //private double _ManualContactModuleOverDrive;
        //[DataMember]
        //public double ManualContactModuleOverDrive
        //{
        //    get { return _ManualContactModuleOverDrive; }
        //    set { _ManualContactModuleOverDrive = value; }
        //}

        //private bool _ManualCotactModuleIsZUpState;
        //[DataMember]
        //public bool ManualCotactModuleIsZUpState
        //{
        //    get { return _ManualCotactModuleIsZUpState; }
        //    set { _ManualCotactModuleIsZUpState = value; }
        //}


        //private double _WantToMoveZInterval;
        //[DataMember]
        //public double WantToMoveZInterval
        //{
        //    get { return _WantToMoveZInterval; }
        //    set { _WantToMoveZInterval = value; }
        //}

        //private bool _CanUsingManualContactControl;
        //[DataMember]
        //public bool CanUsingManualContactControl
        //{
        //    get { return _CanUsingManualContactControl; }
        //    set { _CanUsingManualContactControl = value; }
        //}

        //private bool _IsVisiblePanel;
        //[DataMember]
        //public bool IsVisiblePanel
        //{
        //    get { return _IsVisiblePanel; }
        //    set { _IsVisiblePanel = value; }
        //}
        //private List<IDeviceObject> _UnderDuts;

        //public List<IDeviceObject> UnderDuts
        //{
        //    get { return _UnderDuts; }
        //    set { _UnderDuts = value; }
        //}

        //private MachinePosition _ManualContactModuleMachinePosition;
        //[DataMember]
        //public MachinePosition ManualContactModuleMachinePosition
        //{
        //    get { return _ManualContactModuleMachinePosition; }
        //    set { _ManualContactModuleMachinePosition = value; }
        //}

        //private Point _ManualContactModuleXYIndex;
        //[DataMember]
        //public Point ManualContactModuleXYIndex
        //{
        //    get { return _ManualContactModuleXYIndex; }
        //    set { _ManualContactModuleXYIndex = value; }
        //}
    }
}
