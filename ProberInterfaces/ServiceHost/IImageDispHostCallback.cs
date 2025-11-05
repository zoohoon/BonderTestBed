namespace ProberInterfaces.ServiceHost
{
    using System.ServiceModel;
    public interface IImageDispHostCallback
    {
        [OperationContract(IsOneWay = true)]
        void DisConnect();
    }
}
