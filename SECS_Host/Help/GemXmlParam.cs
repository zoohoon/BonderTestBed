using System;
using System.Xml.Serialization;

namespace SECS_Host.Help
{
    [XmlType("GemXmlParam")]
    public class GemXmlParam
    {
        private String _CfgPath;
        [XmlElement(ElementName = "Gem_CfgPath")]
        public String CfgPath
        {
            get { return _CfgPath; }
            set
            {
                if(_CfgPath != value)
                {
                    _CfgPath = value;
                }
            }
        }

        private String _DynamicReportPath;
        [XmlElement(ElementName ="Gem_DynamicReportPath")]
        public String DynamicReportPath
        {
            get { return _DynamicReportPath; }
            set
            {
                if (_DynamicReportPath != value)
                {
                    _DynamicReportPath = value;
                }
            }
        }
    }
}