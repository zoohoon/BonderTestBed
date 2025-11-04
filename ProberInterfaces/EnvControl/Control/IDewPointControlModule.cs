namespace ProberInterfaces
{
    using ProberInterfaces.Temperature.DewPoint;
    public interface IDewPointManager : IFactoryModule, IModule
    {
        IDewPointModule DewPointModule { get;  }
        double GetDewPoint(int stageindex);
    }
}
