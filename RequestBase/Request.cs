using ProberErrorCode;
using System;

namespace RequestInterface
{
    [Serializable]
    public abstract class RequestBase : IRequest
    {
        //public string Prefix { get; set; }
        public object Result { get; set; }
        public object Argument { get; set; }

        public abstract EventCodeEnum Run();

        public object GetRequestResult()
        {
            Run();

            return Result;
        }

        public EventCodeEnum DoRun()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
               
            retval = Run();

            return retval;
        }

        public RequestBase() { }
    }


    [Serializable]
    public class CommunicationRequestSet : System.ComponentModel.INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }

        private RequestBase _Request;
        public RequestBase Request
        {
            get { return _Request; }
            set
            {
                if (value != _Request)
                {
                    _Request = value;
                    RaisePropertyChanged(nameof(Request));
                }
            }
        }
    }
}
