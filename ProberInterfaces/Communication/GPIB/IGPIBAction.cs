using System;
using Autofac;

namespace ProberInterfaces
{

    public interface IGPIBAction
    {
        IContainer Container { get; }
        void SetContainer(IContainer container);
        GPIBResponse Response { get; set; }
    }



    [Serializable]
    public class GPIBResponse
    {
        public GPIBResponse()
        {
        }

        private ENUMSTB _ACK;
        public ENUMSTB ACK
        {
            get { return _ACK; }
            set { _ACK = value; }
        }

        private ENUMSTB _NACK;
        public ENUMSTB NACK
        {
            get { return _NACK; }
            set { _NACK = value; }
        }
    }
}
