using System;
using System.Threading.Tasks;

namespace ProberInterfaces
{
    using System.Collections.ObjectModel;

    public interface IModuleUpdater : IFactoryModule, IHasSysParameterizable, IModule
    {
        Task RetrieveFromSources();
        Task ShowPanel();
        ObservableCollection<IModuleInformation> ModuleInfos { get; }
    }
    public interface IModuleInformation
    {
        AssemblyInfo AssemblyInformation { get; set; }
        string ModuleName { get; set; }
        string Description { get; set; }
        Version InstalledVersion { get; set; }
        Version RecentVersion { get; set; }
        bool IsNeedUpdate { get; set; }
    }
}
