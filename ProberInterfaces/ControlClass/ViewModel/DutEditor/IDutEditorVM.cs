using ProberErrorCode;
using ProberInterfaces.Enum;
using RelayCommandBase;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProberInterfaces.ControlClass.ViewModel.DutEditor
{
    public interface IDutEditorVM : IDutViewControlVM
    {
        ICommand CmdImportCardData { get; }
        IAsyncCommand InitializePalletCommand { get; }
        IAsyncCommand DutAddCommand { get; }
        IAsyncCommand DutDeleteCommand { get; }
        ICommand ZoomInCommand { get; }
        ICommand ZoomOutCommand { get; }
        //IAsyncCommand<EnumArrowDirection> DutEditerMoveCommand { get; }

        //ICommand DutEditerMoveCommand { get; }
        Task DutEditerMoveCommandFunc(EnumArrowDirection obj);
        DutEditorDataDescription GetDutEditorInfo();

        Task<EventCodeEnum> PageSwitched(object parameter = null);
        Task<EventCodeEnum> Cleanup(object parameter = null);
        void ChangedSelectedCoordM(MachineIndex param);
        void ChangedChangedFirstDutM(MachineIndex param);
        void ChangedAddCheckBoxIsChecked(bool? param);

        byte[] GetDutlist();

        Stream CSVFileStream { get; set; }
        string CSVFilePath { get; set; }

        Task<EventCodeEnum> ImportCardData();
        Task FuncInitializePalletCommand();
        new Task DutAddbyMouseDown();
        Task DutDelete();
        Task DutAdd();


    }
}
