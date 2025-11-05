namespace SorterPickTester.Data
{
    internal class LockStatement
    {
        public bool bCycleStopFlag { set; get; }

        public LockStatement()
        {
            bCycleStopFlag = false;
        }
    }
}