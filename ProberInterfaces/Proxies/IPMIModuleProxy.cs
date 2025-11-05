namespace ProberInterfaces
{
    using ProberErrorCode;
    using ProberInterfaces.Proxies;
    using ProberInterfaces.PMI;

    public interface IPMIModuleProxy : IFactoryModule, IProberProxy
    {
        new void InitService();

        EventCodeEnum LoadDevParameter();

        EventCodeEnum LoadSysParameter();

        EventCodeEnum SaveDevParameter();
        EventCodeEnum SaveSysParameter();
        EventCodeEnum InitDevParameter();
        void AddPadTemplate(PadTemplate template);
        PMITriggerComponent GetTriggerComponent();
        bool GetPMIEnableParam();
    }
}
