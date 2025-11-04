using ProberInterfaces.LightJog;
using ProberInterfaces.Loader.RemoteDataDescription;
using RelayCommandBase;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProberInterfaces.ControlClass.ViewModel
{
    public interface IMarkSettingVM : IUseLightJog, IMainScreenViewModel
    {
        IDisplayPort DisplayPort { get; set; }

        new ICamera CurCam { get; set; }

        double MinHeight { get; set; }
        double MaxHeight { get; set; }
        double DiffHeight { get; set; }
        //double SpecHeight { get; set; }
        double ChuckEndPointMargin { get; set; }

        //IAsyncCommand<EnumChuckPosition> ChuckMoveCommand { get; }

        //IAsyncCommand<EnumChuckPosition> ChuckMoveCommand { get; }
        IAsyncCommand ChuckMoveCommand { get; }

        IAsyncCommand MeasureOnePositionCommand { get; }
        IAsyncCommand MeasureAllPositionCommand { get; }
        ICommand FloatTextBoxClickCommand { get; }

        //void ChangeSpecHeightValue(double value);
        void ChangeMarginValue(double value);

        ChuckPlanarityDataDescription GetChuckPlanarityInfo();

        Task WrapperChuckMoveCommand(ChuckPos param);
    }

}