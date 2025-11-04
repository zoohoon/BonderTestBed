using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ProberInterfaces
{
    public interface IErrorCompensationManager : IFactoryModule, IModule, IHasSysParameterizable
    {
        ICompensationModule CompensationModule { get; set; }
        ObservableCollection<ProbeAxisObject> AssociatedAxes { get; set; }
        List<EnumAxisConstants> AssociatedAxisTypeList { get; set; }
        CompensationValue GetErrorComp(CompensationPos CPos);
        int CalcErrorComp();

        IParam ErrorCompensationDescriptorParam_IParam { get;set; }
    }
}
