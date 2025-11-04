using LogModule;
using ProberErrorCode;
using System;
using System.Runtime.Serialization;

namespace SystemExceptions.ProberSystemException
{
    public class ProberSystemException : Exception
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

        public ProberSystemException()
        {
        }

        public ProberSystemException(string message) : base(message)
        {
        }
        public ProberSystemException(string message, object classname) : base("Class: " + classname.GetType().ToString() + " " + message)
        {
        }
        public ProberSystemException(string message, Exception innerException) : base(message, innerException)
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
        public ProberSystemException(string message, EventCodeEnum errorcode) :
            base(message + " ErrorCode: " + errorcode.ToString())
        {
            
        }
        public ProberSystemException(EventCodeEnum errorcode) :
            base("ErrorCode: " + errorcode.ToString())
        {

        }
        public ProberSystemException(string message, Exception innerException, EventCodeEnum errorcode) :
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
        public ProberSystemException(string message, Exception innerException, EventCodeEnum errorcode, int returnvalue, object classname) :
            base(message + " ErrorCode: " + errorcode.ToString() + " ReturnValue: " + returnvalue.ToString() + " HashCode: " + innerException.GetHashCode().ToString(), innerException)
        {
            ReturnValue = returnvalue;
            ErrorCode = errorcode;
            LoggerManager.Error($"| Class: " + classname.GetType() + "| " + "| Function: " + innerException.TargetSite + " |" + "ReturnValue =  " + returnvalue + " |" + "| HashCode: " + innerException.GetHashCode().ToString() + " |");
        }
        public ProberSystemException(string message, Exception innerException, EventCodeEnum errorcode, object classname) :
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

        protected ProberSystemException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}


