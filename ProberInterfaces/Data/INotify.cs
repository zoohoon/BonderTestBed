using ProberInterfaces.Event;
using System.Collections.Generic;

namespace ProberInterfaces
{
    public interface INotifyParameters
    {
        List<INotifyEvent> EventRecipeList_Notify { get; }
        List<EventComponent> EventList { get; }

        Dictionary<string, string> NotifyRecipe { get; }
    }

    public interface INotify : IFactoryModule, IModule
    {
    }
}
