using ProberInterfaces.Communication.Scenario;
using RequestCore.QueryPack.GPIB;
using System.Collections.Generic;

namespace TesterDriverModule.NI4882
{
    //public class GPIBTesterInterruptCommand : TesterCommand
    //{
    //    private const EnumGPIBCommandType type = EnumGPIBCommandType.INTERRUPT;
    //    public string ACK { get; set; }
    //    public string NACK { get; set; }
    //}

    public class NI4882TesterActionCommand : TesterCommand
    {
        private const EnumGPIBCommandType type = EnumGPIBCommandType.ACTION;

        public List<string> ACK { get; set; }
        public List<string> NACK { get; set; }

        public NI4882TesterActionCommand()
        {
            this.ACK = new List<string>();
            this.NACK = new List<string>();
        }
    }

    public class NI4882TesterQueryCommand : TesterCommand
    {
        private const EnumGPIBCommandType type = EnumGPIBCommandType.QUERY;

        public string ACK { get; set; }
        public string NACK { get; set; }
    }
}
