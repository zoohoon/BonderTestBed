using System.Windows.Input;

namespace ProberInterfaces.Loader
{
    public interface ILoaderMainMenuVM
    {
        ICommand MenuCloseclickCommand { get; }
        //void CloseMenu();
    }
}
