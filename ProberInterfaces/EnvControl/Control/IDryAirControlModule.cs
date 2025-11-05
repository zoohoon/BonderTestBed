namespace ProberInterfaces
{
    using ProberErrorCode;
    using ProberInterfaces.Temperature.DryAir;
    using System.Runtime.Serialization;
    public interface IDryAirManager : IDryAirController, IModule, IFactoryModule
    {
        IDryAirModule DryAirModule { get; }
        EnumDryAirModuleMode GetMode(int stageindex = -1);
    }

    public enum EnumDryAirType
    {
        [EnumMember]
        INVALID = -1,
        [EnumMember]
        UNDEFINED = 0,
        [EnumMember]
        STG, // SINGLE STAGE UPPER
        [EnumMember]
        STGBOTTOM,
        [EnumMember]
        LOADER,
        [EnumMember]
        TESTER
    }

}
