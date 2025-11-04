namespace ProberInterfaces.CassetteIDReader
{
    using ProberErrorCode;
    using ProberInterfaces.Communication;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface ICassetteIDReaderModule : IModule, IFactoryModule, IHasSysParameterizable
    {
        CassetteIDReaderParameters CassetteIDReaderSysParam { get; set; }
        ICSTIDReader CSTIDReader { get; }
        IParam CSTIDReaderParam { get; }
        Task CheckConnectState();
    }

    public interface ICSTIDReader : IHasSysParameterizable, IModule
    {
        EventCodeEnum ReInitialize();
        ICommModule CommModule { get; }
        string ReadCassetteID();
        bool ModuleAttached { get; set; }
        void DataReceived(string receiveData);
        EnumCommmunicationType ModuleCommType { get; set; }
        int ModuleIndex { get; set; }
    }
}
