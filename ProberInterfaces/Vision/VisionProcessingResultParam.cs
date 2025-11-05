using LogModule;
using System;

namespace ProberInterfaces
{
    public class WAPMResult  
    {

        private PMResult _Result;

        public PMResult Result
        {
            get { return _Result; }
            set { _Result = value; }
        }

        private int _Direction;

        public int Direction
        {
            get { return _Direction; }
            set { _Direction = value; }
        }


        private long _MoveIndex;

        public long MoveIndex
        {
            get { return _MoveIndex; }
            set { _MoveIndex = value; }
        }

        public WAPMResult()
        {

        }


        public WAPMResult(PMResult pmresult , int direction , long moveindex)
        {
            try
            {
            this.Result = pmresult;
            this.Direction = direction;
            this.MoveIndex = moveindex;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

    }
}
