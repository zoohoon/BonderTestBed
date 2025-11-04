namespace MapConverterModule.E142.Attribute
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class E142FieldAttribute : System.Attribute
    {
        public bool IsChild;
        public bool NeedAction{ get; set; }
        public E142FieldAttribute(bool ischild, bool needaction = false)
        {
            this.IsChild = ischild;
            this.NeedAction = needaction;
        }
    }
}
