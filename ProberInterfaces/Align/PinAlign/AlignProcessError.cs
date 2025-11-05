using ProberErrorCode;
using System;

namespace ProberInterfaces.PinAlign
{
    public class AlignProcessError : Exception
    {
        public AlignProcessError(EventCodeEnum errcode, string message) : base(message)
        {

        }

    }
}
