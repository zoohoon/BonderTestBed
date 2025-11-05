using System;
using System.Threading;

namespace LogModule
{
    [Serializable]
    public class CancellationTokenSourcePack
    {
        private bool _ReNew = false;
        public bool ReNew
        {
            get { return _ReNew; }
            set
            {
                if (value != _ReNew)
                {
                    _ReNew = value;
                }
            }
        }

        private CancellationTokenSource _TokenSource = new CancellationTokenSource();
        public CancellationTokenSource TokenSource
        {
            get
            {
                return _TokenSource;
            }
            set
            {
                if (value != _TokenSource)
                {
                    _TokenSource = value;
                }
            }
        }
    }
}
