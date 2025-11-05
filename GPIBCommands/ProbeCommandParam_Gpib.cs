using ProberInterfaces.Command;
using System;

namespace Command.GPIB
{
    [Serializable]
    public class GpibSrqParam : ProbeCommandParameter
    {
        public int StbNumber { get; set; }
    }
    
    public class GpibCommandInfoParam : ProbeCommandParameter
    {
        public bool     ExistPrefixInRetVal { get; set; }
        public string     GpibCommandName     { get; set; } // G, Z, A...
        public string   GpibCommandExInfo   { get; set; } // ex) bin info
    }
}
