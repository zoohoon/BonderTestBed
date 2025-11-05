using NLog;

namespace LogService
{
    public interface ILogViewer
    {
        void Target_EventReceived(LogEventInfo log);
    }
}
