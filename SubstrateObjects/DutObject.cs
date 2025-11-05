using LogModule;
using System;
using System.Collections.Generic;

namespace SubstrateObjects
{
    using ProberInterfaces;
    [Serializable]
    public class DutObject : IDutObject
    {
        private List<MachineIndex> _Duts;

        public List<MachineIndex> Duts
        {
            get { return _Duts; }
            set { _Duts = value; }
        }

        private List<UserIndex> _Duts1;

        public List<UserIndex> Duts1
        {
            get { return _Duts1; }
            set { _Duts1 = value; }
        }

        private string _ID;

        public string ID
        {
            get { return _ID; }
            set { _ID = value; }
        }

        private string _Loc;

        public string Loc
        {
            get { return _Loc; }
            set { _Loc = value; }
        }

        private string _MultiChParam;

        public string MultiChParam
        {
            get { return _MultiChParam; }
            set { _MultiChParam = value; }
        }

        private int _NumX;

        public int NumX
        {
            get { return _NumX; }
            set { _NumX = value; }
        }
        private int _NumY;

        public int NumY
        {
            get { return _NumY; }
            set { _NumY = value; }
        }

        private int _Site;

        public int Site
        {
            get { return _Site; }
            set { _Site = value; }
        }
        private List<DutStatistics> _DutStats = new List<DutStatistics>();

        public List<DutStatistics> DutStats
        {
            get { return _DutStats; }
            set { _DutStats = value; }
        }

        public MachineIndex GetRefOffset(int siteindex)
        {
            MachineIndex indexOffset = new MachineIndex();
            try
            {
            indexOffset.XIndex = Duts[siteindex].XIndex - Duts[0].XIndex;
            indexOffset.YIndex = Duts[siteindex].YIndex - Duts[0].YIndex;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return indexOffset;
        }
    }
}
