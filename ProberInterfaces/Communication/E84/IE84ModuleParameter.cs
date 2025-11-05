namespace ProberInterfaces.E84
{
    public interface IE84ModuleParameter
    {
        int FoupIndex { get; set; }
        E84OPModuleTypeEnum E84OPModuleType { get; set; }
        E84ConnTypeEnum E84ConnType { get; }

        Element<E84Mode> AccessMode { get; set; }
    }
}
