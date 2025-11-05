using System.Collections.Generic;

namespace ProberInterfaces.PinAlign.ProbeCardData
{
    public interface IGroupData
    {
        List<int> PinNumList { get; set; }
        PINGROUPALIGNRESULT GroupResult { get; set; }
        List<IPinData> GetPinList();
    }
}
