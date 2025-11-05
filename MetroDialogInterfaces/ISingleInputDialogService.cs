using System.Threading.Tasks;

namespace MetroDialogInterfaces
{
    public delegate Task<EnumMessageDialogResult> SingleInputDialogShowEventHandler(string Label = "Input", string posBtnLabel = "OK", string negBtnLabel = "Cancel", string subLabel = "");
    //public delegate Task<EventCodeEnum> SingleInputDialogCloseEventHandler(object callerassembly = null);

    public delegate string SingleInputGetInputDataEventHandler();

    public interface ISingleInputDialogService
    {
        //Task<EnumMessageDialogResult> ShowDialog(string Label = "Input", string posBtnLabel = "OK", string negBtnLabel = "Cancel");
        string GetInputData();

        //event SingleInputDialogShowEventHandler SingleInputDialogShow;
        //event SingleInputGetInputDataEventHandler SingleInputGetInputData;
    }
}
