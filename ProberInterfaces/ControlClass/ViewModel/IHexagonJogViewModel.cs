namespace ProberInterfaces
{
    using ProberInterfaces.Enum;
    public interface IHexagonJogViewModel
    {
        EnumAxisConstants AxisForMapping { get; set; }
        void StickIndexMove(JogParam parameter, bool setzoffsetenable);
        void StickStepMove(JogParam parameter);
        bool SetMoveZOffsetEnable { get; set; }
    }
}
