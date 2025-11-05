using LogModule;
using System;

namespace CameraChannelManager
{
    public class MuxPortDescriptor
    {
        private int _Channel;
        public int Channel
        {
            get { return _Channel; }
            set { _Channel = value; }
        }

        private int _Port;
        public int Port
        {
            get { return _Port; }
            set { _Port = value; }
        }

        public MuxPortDescriptor(int chn, int port)
        {
            try
            {
                _Channel = chn;
                _Port = port;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

}
