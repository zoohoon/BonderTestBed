using System;

namespace ProberInterfaces
{
    using ProberInterfaces.Event;

    public enum EnumServerPathType
    {
        Undefined,
        Upload,
        Download,
    }

    public enum LogTag
    {
        EVENT_NUMBER,
        LOT_NAME,
        DEVICE_NAME,
        OVERDRIVE_VALUE,
    }
    public enum ImageLog
    {
        OCR,
        SCAN,
        PAD,
        WAFER_ALIGN,
        PIN_ALIGN,
    }

    public interface ILoggerController
    {
        void Debug(String msg);
        void Info(String msg);
        void Info(INotifyEvent evt, String msg);
        void Error(String msg);
    }
}
