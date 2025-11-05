namespace ProberInterfaces.Loader
{
    using ProberErrorCode;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface ITransferManager
    {
        EventCodeEnum ValidationTransferToTray(object sourceObj, object targetobj, ref string failReason);
    }
}
