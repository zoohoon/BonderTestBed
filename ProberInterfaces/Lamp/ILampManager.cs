namespace ProberInterfaces
{
    using ProberInterfaces.Lamp;

    //[Flags]
    //public enum LampStatusEnum
    //{
    //    DONTCARE = 0,
    //    RedOn = 1 << 0,
    //    YellowOn = 1 << 1,
    //    BlueOn = 1 << 2,
    //    BuzzerOn = 1 << 3,
    //    RedOff = 1 << 4,
    //    YellowOff = 1 << 5,
    //    BlueOff = 1 << 6,
    //    BuzzerOff = 1 << 7,
    //    RedBlinkOn = 1 << 8,
    //    YellowBlinkOn = 1 << 9,
    //    BlueBlinkOn = 1 << 10,
    //}
    public enum LampStatusEnum
    {
        DONTCARE,
        On,
        Off,
        BlinkOn,
    }
    public enum AlarmPriority
    {
        Info = 0,
        Normal = 1,
        Warning = 2,
        Emergency = 3,
    }
    public enum EnumLampType
    {
        UNDEFINED,
        Red,
        Yellow,
        Blue,
    }
    public interface ILampManager : IFactoryModule, IModule
    {
        EnumLampType OnLampType { get; set; }
        void RequestSirenLamp();
        void RequestRedLamp();
        void RequestWarningLamp();
        void ClearRequestLamp();
        void SetRequestLampCombo(RequestCombination requestCombo);

        void WrappingSetBuzzerStatus(LampStatusEnum lampStatusEnum);
        RequestCombination GetCurrentLampCombo();
    }
}
