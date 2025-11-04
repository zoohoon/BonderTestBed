using ProberInterfaces.Command;

namespace ProberInterfaces
{
    public interface IInternalParameters
    {
        CommandRecipe CommandRecipe { get; set; }
        CommandRecipe ConsoleRecipe { get; set; }
    }

    public interface IInternal : IFactoryModule, IModule
    {
        IInternalParameters InternalParameterObject { get; }
    }
}
