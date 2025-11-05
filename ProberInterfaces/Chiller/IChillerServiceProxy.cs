using ProberInterfaces.Temperature.Chiller;

namespace ProberInterfaces
{
    public interface IChillerServiceProxy : System.ServiceModel.ICommunicationObject , IChillerComm
    {
        IChillerService GetService();
        bool IsServiceAvailable();
    }
}
