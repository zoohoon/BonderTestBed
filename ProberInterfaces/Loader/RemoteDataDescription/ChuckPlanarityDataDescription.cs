using System.Collections.Generic;

namespace ProberInterfaces.Loader.RemoteDataDescription
{
    using ProberInterfaces.ControlClass.ViewModel;
    using System.Runtime.Serialization;
    [DataContract]
    public class ChuckPlanarityDataDescription
    {
        private double _MinHeight;
        [DataMember]
        public double MinHeight
        {
            get { return _MinHeight; }
            set { _MinHeight = value; }
        }

        private double _MaxHeight;
        [DataMember]
        public double MaxHeight
        {
            get { return _MaxHeight; }
            set { _MaxHeight = value; }
        }

        private double _DiffHeight;
        [DataMember]
        public double DiffHeight
        {
            get { return _DiffHeight; }
            set { _DiffHeight = value; }
        }

        private double _AvgHeight;
        [DataMember]
        public double AvgHeight
        {
            get { return _AvgHeight; }
            set { _AvgHeight = value; }
        }

        private double _ChuckEndPointMargin;
        [DataMember]
        public double ChuckEndPointMargin
        {
            get { return _ChuckEndPointMargin; }
            set { _ChuckEndPointMargin = value; }
        }

        private double _ChuckFocusingRange;
        [DataMember]
        public double ChuckFocusingRange
        {
            get { return _ChuckFocusingRange; }
            set { _ChuckFocusingRange = value; }
        }
        //private double _SpecHeight;
        //[DataMember]
        //public double SpecHeight
        //{
        //    get { return _SpecHeight; }
        //    set { _SpecHeight = value; }
        //}

        //private EnumChuckPosition _CurrentChuckPosEnum;
        //[DataMember]
        //public EnumChuckPosition CurrentChuckPosEnum
        //{
        //    get { return _CurrentChuckPosEnum; }
        //    set { _CurrentChuckPosEnum = value; }
        //}

        //private ObservableCollection<ChuckPos> _ChuckPosList;
        //[DataMember]
        //public ObservableCollection<ChuckPos> ChuckPosList
        //{
        //    get { return _ChuckPosList; }
        //    set { _ChuckPosList = value; }
        //}

        private List<ChuckPos> _ChuckPosList;
        [DataMember]
        public List<ChuckPos> ChuckPosList
        {
            get { return _ChuckPosList; }
            set { _ChuckPosList = value; }
        }

        //private List<BaseThing> _Items;
        //[DataMember]
        //public List<BaseThing> Items
        //{
        //    get { return _Items; }
        //    set
        //    {
        //        _Items = value;
        //    }
        //}

    }
}
