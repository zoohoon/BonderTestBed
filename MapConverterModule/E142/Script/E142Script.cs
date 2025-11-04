using ProberInterfaces.ResultMap.Script;
using System.Collections.Generic;

namespace MapConverterModule.E142.Script
{
    public class E142Script : IMapScript
    {
        public MapDataField mapDataField { get; set; }
    }

    public class MapDataField
    {
        public List<LayoutField> layouts { get; set; }

        public List<SubstrateField> substrates { get; set; }

        public List<SubstrateMapField> substrateMaps { get; set; }

        public MapDataField()
        {
            this.layouts = new List<LayoutField>();
            this.substrates = new List<SubstrateField>();
            this.substrateMaps = new List<SubstrateMapField>();
        }
    }
}
