using ProberErrorCode;

namespace SystemExceptions
{
    public class ExceptionReturnData
    {
        private string _Message;

        public string Message
        {
            get { return _Message; }
            set { _Message = value; }
        }

        private EventCodeEnum _ErrorCode;

        public EventCodeEnum ErrorCode
        {
            get { return _ErrorCode; }
            set { _ErrorCode = value; }
        }

        private int _ReturnValue;
        public int ReturnValue
        {
            get { return _ReturnValue; }
            set { _ReturnValue = value; }
        }

        public ExceptionReturnData(string message,EventCodeEnum errorcode, int returnvalue)
        {
            Message = message;
            ErrorCode = errorcode;
            ReturnValue = returnvalue;
        }
        public ExceptionReturnData(string message)
        {
            Message = message;
        }

    }
}
