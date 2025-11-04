using LoaderParameters;
using System.Threading.Tasks;

namespace LoaderBase
{
    public interface IForcedLoaderMapConvert
    {
    }

    public interface ILoaderMapConvert
    {
        Task LoaderMapConvert(LoaderMap map);

        void UpdateChanged();
    }
}
