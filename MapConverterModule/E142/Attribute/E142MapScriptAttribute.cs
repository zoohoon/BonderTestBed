namespace MapConverterModule.E142.Attribute
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class E142MapScriptVersionAttribute : System.Attribute
    {
        public double Version;

        public E142MapScriptVersionAttribute(double version)
        {
            this.Version = version;
        }
    }
}
