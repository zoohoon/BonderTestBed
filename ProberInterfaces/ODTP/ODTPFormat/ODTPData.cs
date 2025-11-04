namespace ProberInterfaces.ODTP.ODTPFormat
{
    using System.Collections.Generic;


    // 참고: 생성된 코드에 .NET Framework 4.5 또는 .NET Core/Standard 2.0이(가) 필요할 수 있습니다.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:stm:xsd.ODTP.V6-0")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "urn:stm:xsd.ODTP.V6-0", IsNullable = false)]
    public partial class ODTP
    {
        public ODTPHeader Header { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("PCContact")]
        public List<ODTPPCContact> PCContact { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:stm:xsd.ODTP.V6-0")]
    public partial class ODTPHeader
    {
        public string DateTime { get; set; }
        public string Reader { get; set; }
        public string PCName { get; set; }
        public int PCParallelism { get; set; }
        public string SetupFile { get; set; }
        public int SetupTemp { get; set; }
        public int Flat { get; set; }
        public int CoQuad { get; set; }
        public int PrQuad { get; set; }
        public double XRef { get; set; }
        public double YRef { get; set; }
        public int XRefDie { get; set; }
        public int YRefDie { get; set; }
        public string ODUnit { get; set; }
        public int TouchNum { get; set; }
        public int XStrp { get; set; }
        public int YStrp { get; set; }
        public double XStep { get; set; }
        public double YStep { get; set; }
        public string Prober { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:stm:xsd.ODTP.V6-0")]
    public partial class ODTPPCContact
    {
        [System.Xml.Serialization.XmlElementAttribute("Die")]
        public List<ODTPPCContactDie> Die { get; set; }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int ContactNumber { get; set; }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DateTime { get; set; }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int OD { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:stm:xsd.ODTP.V6-0")]
    public partial class ODTPPCContactDie
    {
        public long XCoord { get; set; }
        public long YCoord { get; set; }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int DUTNumber { get; set; }
    }


}
