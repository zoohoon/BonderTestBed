namespace ProberInterfaces
{
    public interface IMarkObject : IAlignModule, IHasComParameterizable, IFactoryModule
    {
        bool GetDoMarkAlign();
    }
}
