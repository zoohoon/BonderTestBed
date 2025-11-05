namespace ProberInterfaces.Retest
{


    /// <summary>
    /// ALL의 경우, 모든 FAIL을 진행하다고 생각하면 됨.
    /// </summary>
    public enum ReTestMode
    {
        ALL = 0,
        BYBIN
    }

    public enum RetestCategoryEnum
    {
        UNDEFINED = 0,
        CP1,
        CP2,
        ONLINERETEST
    }

    public interface IRetestDeviceParam : IDeviceParameterizable, IParamNode
    {
    }
}
