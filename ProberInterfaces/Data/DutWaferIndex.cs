namespace ProberInterfaces
{
    public class DutWaferIndex
    {

        private MachineIndex _DutIndex = new MachineIndex();

        public MachineIndex DutIndex
        {
            get { return _DutIndex; }
            set { _DutIndex = value; }
        }

        private MachineIndex _WaferIndex = new MachineIndex();

        public MachineIndex WaferIndex
        {
            get { return _WaferIndex; }
            set { _WaferIndex = value; }
        }

        private long _DutNumber;

        public long DutNumber
        {
            get { return _DutNumber; }
            set { _DutNumber = value; }
        }

        public DutWaferIndex()
        {

        }
        public DutWaferIndex(MachineIndex dutindex, IndexCoord waferindex, long dutnumber)

        {
            DutIndex = dutindex;
            WaferIndex.XIndex  = waferindex.XIndex;
            WaferIndex.YIndex = waferindex.YIndex;
            DutNumber = dutnumber;
        }
    }
}
