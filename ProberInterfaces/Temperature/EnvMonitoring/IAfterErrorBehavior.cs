using ProberErrorCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProberInterfaces.Temperature.EnvMonitoring
{
    public interface IAfterErrorBehavior : IFactoryModule
    {        
        EventCodeEnum InitModule();
        //Task<EventCodeEnum> ErrorOccurred();                
        Element<bool> IsEnableBehavior { get; set; }        // behavior 동작을 할지 말지 파라미터
        Element<bool> IsEnableSystemError { get; set; }          // system error 발생할지 말지 파라미터
        Element<string> BehaviorClassName { get; set; }
    }
}
