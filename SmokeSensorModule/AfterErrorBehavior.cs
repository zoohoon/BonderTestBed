using LoaderController.GPController;
using LogModule;
using Newtonsoft.Json;
using NotifyEventModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Temperature.EnvMonitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvMonitoring
{
    [Serializable]
    public abstract class AfterErrorBehavior : IAfterErrorBehavior
    {
        public AfterErrorBehavior()
        {

        }

        public Element<bool> IsEnableSystemError { get; set; }

        public Element<bool> IsEnableBehavior { get; set; }
        
        public Element<string> BehaviorClassName { get; set; }

        public Task<EventCodeEnum> ErrorOccurred()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum InitModule()
        {
            throw new NotImplementedException();
        }
    }
    [Serializable]
    public class SaveResultMap : IAfterErrorBehavior
    {
        public SaveResultMap(bool enable) 
        {
            IsEnableBehavior.Value = enable;
        }
        private Element<string> _BehaviorClassName = new Element<string>() { Value = "SaveResultMap" };
        public Element<string> BehaviorClassName
        {
            get { return _BehaviorClassName; }
            set { _BehaviorClassName = value; }
        }

        private Element<bool> _IsEnableBehavior = new Element<bool>() { Value = false };
        [JsonIgnore]
        public Element<bool> IsEnableBehavior
        {
            get { return _IsEnableBehavior; }
            set { _IsEnableBehavior = value; }
        }

        private Element<bool> _IsEnableSystemError = new Element<bool>() { Value = false };
        [JsonIgnore]
        public Element<bool> IsEnableSystemError
        {
            get { return _IsEnableSystemError; }
            set { _IsEnableSystemError = value; }
        }
    
        public EventCodeEnum InitModule()
        {
            throw new NotImplementedException();
        }        
    }

    [Serializable]
    public class ZDown : IAfterErrorBehavior
    {
        public ZDown(bool enable) 
        {
            IsEnableBehavior.Value = enable;
        }
        private Element<string> _BehaviorClassName = new Element<string>() { Value = "ZDown" };
        public Element<string> BehaviorClassName
        {
            get { return _BehaviorClassName; }
            set { _BehaviorClassName = value; }
        }

        private Element<bool> _IsEnableBehavior = new Element<bool>() { Value = false };
        [JsonIgnore]
        public Element<bool> IsEnableBehavior
        {
            get { return _IsEnableBehavior; }
            set { _IsEnableBehavior = value; }
        }

        private Element<bool> _IsEnableSystemError = new Element<bool>() { Value = false };
        [JsonIgnore]
        public Element<bool> IsEnableSystemError
        {
            get { return _IsEnableSystemError; }
            set { _IsEnableSystemError = value; }
        }

        public EventCodeEnum InitModule()
        {
            throw new NotImplementedException();
        }        
    }

    [Serializable]
    public class SystemError : IAfterErrorBehavior
    {
        public SystemError(bool enable) 
        {
            IsEnableBehavior.Value = enable;
        }
        private Element<string> _BehaviorClassName = new Element<string>() { Value = "SystemError" };
        public Element<string> BehaviorClassName
        {
            get { return _BehaviorClassName; }
            set { _BehaviorClassName = value; }
        }

        private Element<bool> _IsEnableBehavior = new Element<bool>() { Value = false };
        public Element<bool> IsEnableBehavior
        {
            get { return _IsEnableBehavior; }
            set { _IsEnableBehavior = value; }
        }

        private Element<bool> _IsEnableSystemError = new Element<bool>() { Value = false };
        [JsonIgnore]
        public Element<bool> IsEnableSystemError
        {
            get { return _IsEnableSystemError; }
            set { _IsEnableSystemError = value; }
        }

        public EventCodeEnum InitModule()
        {
            throw new NotImplementedException();
        }        
    }
}
