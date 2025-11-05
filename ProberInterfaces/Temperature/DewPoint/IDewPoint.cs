namespace ProberInterfaces.Temperature.DewPoint
{
    public interface IDewPointModule : IFactoryModule, IModule, IHasSysParameterizable
    {
        void Start();
        double CurDewPoint { get; set; }
        bool IsAttached { get; }
        bool IsSensorValid { get; }
        double DewPointOffset { get; set; }
        double Tolerence { get; set; }
        double Hysteresis { get; set; }
        long WaitTimeout { get; set; }        
    }
}
