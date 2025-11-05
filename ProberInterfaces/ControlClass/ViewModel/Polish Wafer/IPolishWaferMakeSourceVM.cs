using RelayCommandBase;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProberInterfaces.ControlClass.ViewModel.Polish_Wafer
{
    public interface IPolishWaferMakeSourceVM : IMainScreenViewModel
    {
        IAsyncCommand AddSourceCommand { get; }
        IAsyncCommand RemoveSourceCommand { get; }
        IAsyncCommand AssignCommand { get; }
        IAsyncCommand RemoveCommand { get; }
        IAsyncCommand SelectedObjectCommand { get; }
        ICommand ChangedSizeCommand { get; }

        ICommand ChangedNotchTypeCommand { get; }
        void UpdateCleaningParameters(string sourcename);

        //void SetPolishWaferIParam(byte[] param);
        //void SetSelectedObjectCommand(byte[] info);

        #region Remote commands

        Task AddSourceRemoteCommand();
        Task RemoveSourceRemoteCommand();
        Task AssignRemoteCommand();
        Task RemoveRemoteCommand();
        Task SelectedObjectRemoteCommand(object param);
        #endregion
    }
}
