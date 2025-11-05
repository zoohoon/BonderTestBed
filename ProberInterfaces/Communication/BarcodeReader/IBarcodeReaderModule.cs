namespace ProberInterfaces.Communication.BarcodeReader
{
    using ProberInterfaces.CassetteIDReader;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IBarcodeReaderModule : IModule, IFactoryModule, IHasSysParameterizable, ICSTIDReader
    {
        BarcodeReaderSysParameters BarcodeReaderSysParam { get; set; }
    }
}
