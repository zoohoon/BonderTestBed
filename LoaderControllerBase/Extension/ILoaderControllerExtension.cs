using LoaderParameters;
using ProberErrorCode;
using LoaderServiceBase;
using ProberInterfaces;

namespace LoaderControllerBase
{
    public interface ILoaderControllerExtension
    {
        LoaderControllerParam ControllerParam { get; }

        LoaderSystemParameter LoaderSystemParam { get; }

        LoaderDeviceParameter LoaderDeviceParam { get; }

        ModuleID ChuckID { get; }
        
        LoaderInfo LoaderInfo { get; }

        LoaderMapEditor GetLoaderMapEditor();

        ILoaderService LoaderService { get; }

        IWaferTransferScheduler WaferTransferScheduler { get; }

        EventCodeEnum UpdateSystemParam(LoaderSystemParameter systemParam);

        EventCodeEnum UpdateDeviceParam(LoaderDeviceParameter deviceParam);
        
        EventCodeEnum OCRUaxisRelMove(double value);

        EventCodeEnum OCRWaxisRelMove(double value);

    }
}
