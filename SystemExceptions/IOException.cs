using LogModule;
using ProberErrorCode;
using System;
using System.Runtime.Serialization;

namespace SystemExceptions.InOutException
{
    public class InOutException : Exception
    {

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

        public InOutException()
        {
        }

        public InOutException(string message) : base(message)
        {
        }
        public InOutException(string message, object classname) : base("Class: " + classname.GetType().ToString() + " " + message)
        {
        }
        public InOutException(string message, Exception innerException) : base(message, innerException)
        {
            try
            {
            LoggerManager.Error($"| Function: " + innerException.TargetSite + " |" + "| HashCode: " + innerException.GetHashCode().ToString() + " |");

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public InOutException(string message, Exception innerException, EventCodeEnum errorcode) :
            base(message + " ErrorCode: " + errorcode.ToString() + " HashCode: " + innerException.GetHashCode().ToString(), innerException)
        {
            try
            {
            ErrorCode = errorcode;
            LoggerManager.Error($"| Function: " + innerException.TargetSite + " |" + "| HashCode: " + innerException.GetHashCode().ToString() + " |");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public InOutException(string message, Exception innerException, EventCodeEnum errorcode, int returnvalue, object classname) :
            base(message + " ErrorCode: " + errorcode.ToString() + " ReturnValue: " + returnvalue.ToString() + " HashCode: " + innerException.GetHashCode().ToString(), innerException)
        {
            ReturnValue = returnvalue;
            ErrorCode = errorcode;
            LoggerManager.Error($"| Class: " + classname.GetType() + "| " + "| Function: " + innerException.TargetSite + " |" + "ReturnValue =  " + returnvalue + " |" + "| HashCode: " + innerException.GetHashCode().ToString() + " |");
        }
        public InOutException(string message, Exception innerException, EventCodeEnum errorcode, object classname) :
            base("Class: " + classname.GetType().ToString() + message + " ErrorCode: " + errorcode.ToString() + " HashCode: " + innerException.GetHashCode().ToString(), innerException)
        {
            try
            {
            ErrorCode = errorcode;
            LoggerManager.Error($"| Class: " + classname.GetType() + "| " + "| Function: " + innerException.TargetSite + " |" + "| HashCode: " + innerException.GetHashCode().ToString() + " |");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        protected InOutException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
