using LogModule;
using ProberErrorCode;
using System;
using System.Runtime.Serialization;

namespace SystemExceptions.VisionException
{
    public class VisionException : Exception
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

        public VisionException()
        {
        }

        public VisionException(string message) : base(message)
        {
        }
        public VisionException(string message, object classname) : base("Class: " + classname.GetType().ToString() + " " + message)
        {
        }
        public VisionException(string message, Exception innerException) : base(message, innerException)
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
        public VisionException(string message, Exception innerException, EventCodeEnum errorcode) :
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
        public VisionException(string message, Exception innerException, EventCodeEnum errorcode, int returnvalue, object classname) :
            base(message + " ErrorCode: " + errorcode.ToString() + " ReturnValue: " + returnvalue.ToString() + " HashCode: " + innerException.GetHashCode().ToString(), innerException)
        {
            ErrorCode = errorcode;
            ReturnValue = returnvalue;
            LoggerManager.Error($"| Class: " + classname.GetType() + "| " + "| Function: " + innerException.TargetSite + " |" + "ReturnValue =  " + returnvalue + " |" + "| HashCode: " + innerException.GetHashCode().ToString() + " |");
        }
        public VisionException(string message, Exception innerException, EventCodeEnum errorcode, object classname) :
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

        protected VisionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
