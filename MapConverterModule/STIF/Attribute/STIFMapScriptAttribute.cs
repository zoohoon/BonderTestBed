namespace MapConverterModule
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class STIFMapScriptVersionAttribute : System.Attribute
    {
        public double Version;

        public STIFMapScriptVersionAttribute(double version)
        {
            this.Version = version;
        }
    }
}
