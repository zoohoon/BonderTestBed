using ProberInterfaces.Communication.Scenario;
using RequestCore.Query.TCPIP;
using System.Collections.Generic;

namespace TesterDriverModule.TCPIP
{
    public class TCPIPTesterInterruptCommand : TesterCommand
    {
        private const EnumTCPIPCommandType type = EnumTCPIPCommandType.INTERRUPT;
        public string ACK { get; set; }
        public string NACK { get; set; }
    }

    public class TCPIPTesterActionCommand : TesterCommand
    {
        private const EnumTCPIPCommandType type = EnumTCPIPCommandType.ACTION;

        public List<string> ACK { get; set; }
        public List<string> NACK { get; set; }

        public TCPIPTesterActionCommand()
        {
            this.ACK = new List<string>();
            this.NACK = new List<string>();
        }
    }

    public class TCPIPTesterQueryCommand : TesterCommand
    {
        private const EnumTCPIPCommandType type = EnumTCPIPCommandType.QUERY;

        public string ACK { get; set; }
        public string NACK { get; set; }
    }
}
