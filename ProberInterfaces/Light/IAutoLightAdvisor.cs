using System.Threading.Tasks;

namespace ProberInterfaces
{
    public interface IAutoLightAdvisor : IFactoryModule
    {
        Task<int> SetGrayLevel(EnumProberCam camType, EnumLightType lightType, int grayLevel);
        bool SetGrayLevel(EnumProberCam camType, int grayLevel);
        int SetupGraylLevelLUT(EnumProberCam camType, EnumLightType lightType);
        int GetGrayLevel(EnumProberCam camType, object assembly = null);
    }
}
