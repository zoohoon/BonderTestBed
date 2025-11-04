using System.Threading.Tasks;

namespace ProberInterfaces.DialogControl
{
    public interface IDisplayPortDialog : IFactoryModule, IModule
    {
        bool IsShowing { get; }
        bool WaferAlignOnToggle { get; set;}
        bool PinAlignOnToggle { get; set; }
        string NameTag { get; set; }
        Task<bool> ShowDialog();
        Task CloseDialog();
        bool DisplayPortDialogDone { get; set; }
    }
}
