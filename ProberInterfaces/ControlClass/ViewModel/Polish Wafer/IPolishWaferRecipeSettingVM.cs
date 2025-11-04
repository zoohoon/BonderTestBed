using ProberInterfaces.PolishWafer;
using RelayCommandBase;
using System.Threading.Tasks;

namespace ProberInterfaces.ControlClass.ViewModel.Polish_Wafer
{
    public interface IPolishWaferRecipeSettingVM : IMainScreenViewModel
    {
        Task CleaningDeleteCommandWrapper(byte[] param);
        
        IAsyncCommand IntervalAddCommand { get; }
        //ICommand IntervalDeleteCommand { get; }
        //IAsyncCommand CleaningAddCommand { get; }

        void SetPolishWaferIParam(byte[] param);
        Task SetSelectedInfos(SelectionUIType selectiontype, byte[] cleaningparam, byte[] pwinfo, byte[] intervalparam, int intervalindex, int cleaningindex);

        #region Remote commands
        
        Task IntervalAddRemoteCommand();
        //Task IntervalDeleteRemoteCommand(object obj);
        //Task CleaningAddRemoteCommand(object obj);
        #endregion

        void IntervalDelete(int index);
        void CleaningAdd(int index);
        void CleaningDelete(PolishWaferIndexModel obj);
    }
}
