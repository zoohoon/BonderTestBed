using ProberErrorCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProberInterfaces.Proxies
{
    public interface IDataGatewayProxy 
    {
        bool IsServiceAvailable();
        EventCodeEnum NotifyStageAlarm(EventCodeParam noticeCodeInfo);
    }
}
