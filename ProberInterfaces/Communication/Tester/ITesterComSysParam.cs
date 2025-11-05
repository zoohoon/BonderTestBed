using ProberErrorCode;
using ProberInterfaces.Communication.Scenario;

namespace ProberInterfaces.Communication.Tester
{
    public enum EnumTesterComType
    {
        UNDEFINED = 0,
        GPIB,
        TCPIP
    }

    public interface ITesterComSysParam
    {

    }

    public interface IUseTesterDriver
    {
        ITesterComDriver TesterComDriver { get; set; }
        //ITesterComDriver GetTesterDriver();

        EventCodeEnum CreateTesterComDriver();

        bool GetTesterAvailable();
    }

    public interface ITesterCommunicationManager
    {
        EventCodeEnum CreateTesterComInstance(EnumTesterComType comType);
        bool GetTesterAvailable();

        ITesterScenarioManager ScenarioManager { get; set; }
    }
}
