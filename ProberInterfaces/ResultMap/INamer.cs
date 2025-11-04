using System.Collections.Generic;

namespace ProberInterfaces.ResultMap
{
    public interface INamer
    {
        Dictionary<EnumProberMapProperty, object> ProberMapDictionary { get; }
    }
}
