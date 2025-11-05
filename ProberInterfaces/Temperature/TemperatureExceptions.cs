using System;

namespace ProberInterfaces.Temperature
{
    public class TemperatureConnectException : Exception
    {
        public TemperatureConnectException() { }
        public TemperatureConnectException(string Message)
        {
            Message += " ";
        }
        public override string Message =>
            "There is a problem with the TempControl connection.";
    }
}
