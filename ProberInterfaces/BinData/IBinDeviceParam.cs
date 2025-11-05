namespace ProberInterfaces.BinData
{
    public interface IBINInfo
    {
        Element<int> BinCode { get; set; }
        Element<int> PassFail { get; set; }
        Element<string> Description { get; set; }
        Element<bool> RetestForCP1Enable { get; set; }
        Element<bool> RetestForCP2Enable { get; set; }
        Element<bool> RretestForNthEnable { get; set; }
        Element<bool> InstantRetestForCP1Enable { get; set; }
        Element<bool> InstantRetestForRetestEnable { get; set; }
        Element<int> NthRetestBINCnt { get; set; }
        Element<int> ContinuousFailCnt { get; set; }
        Element<int> AccumulateFailCnt { get; set; }
        Element<int> BINGroupNo { get; set; }
        Element<bool> Unusable { get; set; }
    }

    public enum RetestTypeEnum
    {
        RETESTFORCP1 = 0,
        RETESTFORCP2,
        RRETESTFORNTH,
        INSTANTRETESTFORCP1,
        INSTANTRETESTFORRETEST
    }

    //public enum EnableEnum
    //{
    //    Disable,
    //    Enable
    //}

    //public interface IHasOldParameters
    //{
    //    EventCodeEnum ConvertParam();
    //}

    public interface IBinDeviceParam : IDeviceParameterizable/*, IHasOldParameters*/
    {
    }
}
