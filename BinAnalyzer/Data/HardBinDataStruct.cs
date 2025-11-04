using System.Collections.Generic;

namespace BinAnalyzer.Data
{
    public class HardBinData
    {
        private Dictionary<string, byte> _UserBinDictionary;
        public Dictionary<string, byte> UserBinDictionary
        {
            get { return _UserBinDictionary; }
            set
            {
                if (_UserBinDictionary != value)
                    _UserBinDictionary = value;
            }
        }

        private int _HardBinSize;
        public int HardBinSize
        {
            get { return _HardBinSize; }
            set
            {
                if (value != _HardBinSize)
                {
                    _HardBinSize = value;
                }
            }
        }
    }
}
