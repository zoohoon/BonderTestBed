using System.Collections.Generic;

using LoaderParameters;
using ProberErrorCode;
using ProberInterfaces;

namespace LoaderBase
{
    public interface ILoaderSequencer : ILoaderFactoryModule
    {
        ILoaderMapAnalyzer LoaderMapAnalyzer { get; }

        bool HasProcessor();

        LoaderProcStateEnum GetProcState();

        ReasonOfSuspendedEnum GetSuspendedInfo();

        ResponseResult SetRequest(LoaderMap dstMap);

        EventCodeEnum DoSchedule();

        LoaderProcStateEnum DoProcess();

        void AwakeProcessModule();

        void Clear();

        void SelfRecovery();
    }

    public interface ILoaderMapAnalyzer
    {
        List<ILoaderJob> Build(LoaderMap dstMap);
    }
    public interface ILoaderMapSlicer
    {
        List<LoaderMap> Slicing(LoaderMap loaderMap);
    }
}
