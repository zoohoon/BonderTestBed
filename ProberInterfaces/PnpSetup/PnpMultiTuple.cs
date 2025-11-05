using LogModule;
using System;

namespace ProberInterfaces.PnpSetup
{
    public class PnpMultiTuple
    {
        private IPnpSetupScreen _PnpSetupScreen;

        public IPnpSetupScreen PnpSetupScreen
        {
            get { return _PnpSetupScreen; }
            set { _PnpSetupScreen = value; }
        }

        private IPnpSetup _PnpSetup;

        public IPnpSetup PnpSetup
        {
            get { return _PnpSetup; }
            set { _PnpSetup = value; }
        }

        public PnpMultiTuple()
        {

        }
        public PnpMultiTuple(IPnpSetupScreen screen, IPnpSetup setup)
        {
            try
            {
            PnpSetupScreen = screen;
            PnpSetup = setup;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

    }
}
