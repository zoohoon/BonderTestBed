using ProberInterfaces.ResultMap;

namespace MapConverterModule.STIF
{
    public class STIFMapComponent : MapComponentBase
    {
        public STIFCOMPONENTTYPE ComponentType { get; set; }
        public bool UseUnit { get; set; }
        public string Unitvalue { get; set; }
        public string Unit { get; set; }
    }
}
