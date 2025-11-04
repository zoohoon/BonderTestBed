using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProberInterfaces.AlignEX
{
    using ProberInterfaces.Enum;
    using ProberInterfaces.Error;
    using Autofac;
    using System.Xml.Serialization;
    using ProberErrorCode;

    public interface IAlignRecoveryModule : IFactoryModule
    {
        ErrorCodeEnum Run();
    }

    //[Serializable]
    //public abstract class AlignRecoveryModuleBase : IAlignRecoveryModule
    //{
    //    [XmlIgnore]
    //    public int InitPriority { get; set; }

    //    public abstract ErrorCodeEnum InitModule(IContainer container);
    //}

}
