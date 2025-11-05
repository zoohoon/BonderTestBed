using System.ServiceModel;

namespace ProberInterfaces.Retest
{
    [ServiceContract]
    public interface IRetestModule : IHasDevParameterizable, IModule, IFactoryModule
    {
        IParam RetestModuleDevParam_IParam { get; set; }

        [OperationContract]
        bool IsServiceAvailable();


        [OperationContract]
        IParam GetRetestIParam();

        [OperationContract]
        byte[] GetRetestParam();

        [OperationContract]
        void SetRetestIParam(byte[] param);
    }

    public interface IRetestSettingViewModel : IMainScreenViewModel
    {
        void SetRetestIParam(byte[] param);
    }
}
