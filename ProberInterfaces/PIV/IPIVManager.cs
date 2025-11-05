namespace ProberInterfaces
{
    public interface IPIVManager : IFactoryModule, IHasSysParameterizable
    {
        int GetFoupState(GEMFoupStateEnum foupStateEnum);
        int GetStageState(GEMStageStateEnum stageStateEnum);
        int GetPreHeatState(GEMPreHeatStateEnum preHeatStateEnum);

    }
}
