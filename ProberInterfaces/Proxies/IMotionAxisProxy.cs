namespace ProberInterfaces
{
    using ProberErrorCode;
    using ProberInterfaces.Proxies;
    using System.Collections.ObjectModel;
    using System.ServiceModel;

    public interface IMotionAxisProxy : IProberProxy
    {
        bool IsOpened();
        CommunicationState GetCommunicationState();
        ObservableCollection<ProbeAxisObject> Axes { get; set; }
        void InitAxes();
        ProbeAxisObject GetAxis(EnumAxisConstants axis);
        EventCodeEnum RelMove(EnumAxisConstants axis, double pos);
        EventCodeEnum VMove(EnumAxisConstants axis,double vel, EnumTrjType trjtype);
        EventCodeEnum WaitForAxisMotionDone(EnumAxisConstants axis, long timeout = 0);
        EventCodeEnum IsThreeLegUp(EnumAxisConstants axis, ref bool isthreelegup);
        EventCodeEnum IsThreeLegDown(EnumAxisConstants axis, ref bool isthreelegdn);
        EventCodeEnum IsRls(EnumAxisConstants axis, ref bool isrls);
        EventCodeEnum IsFls(EnumAxisConstants axis, ref bool isfls);
    }
}
