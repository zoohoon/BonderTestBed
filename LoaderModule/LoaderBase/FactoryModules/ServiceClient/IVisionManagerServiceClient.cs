using System.Threading.Tasks;

namespace LoaderBase.FactoryModules.ServiceClient
{
    using ProberInterfaces;
    public interface IVisionManagerServiceClient : IVisionManager
    {
        Task DispHostService_ImageUpdate(ICamera Camera, ImageBuffer image);
    }
}
