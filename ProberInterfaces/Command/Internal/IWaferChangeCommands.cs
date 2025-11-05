namespace ProberInterfaces.Command.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class RequestWCJobInfo : ProbeCommandParameter
    {
        public string allocSeqId { get; set; }
    }

    public class WaferTransferObjectHolderInfo : ProbeCommandParameter
    {
        public object source { get; set; }
        public object target { get; set; }
    }

    public interface IWaferTransferObject : IProbeCommand
    {
    }


    public interface IStartWaferChangeSequence : IProbeCommand
    {
    }
    public interface IAbortWaferChangeSequence : IProbeCommand
    {
    }
}
