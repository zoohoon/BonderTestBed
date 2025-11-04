using ProberInterfaces.Command;

namespace Command.GEM
{
    public class GEMEventParam : IProbeCommandParameter
    {
        public string Command { get; set; }

        private long _EventID;

        public long EventID
        {
            get { return _EventID; }
            set { _EventID = value; }
        }
    }

    //S6F11은 Define Report로 해당 CEID가 정의/Enable 되어야 한다.
    //public class GEM_S6F11 : ProbeCommand, IGEM_S6F11
    //{
    //    public override bool Execute()
    //    {
    //        try
    //        {
    //            long eventID = ((GEMEventParam)Parameter).EventID;

    //            this.GEMModule().SetEvent(eventID);

    //            throw new NotImplementedException();
    //        }
    //        catch (Exception err)
    //        {
    //            LoggerManager.Exception(err);
    //            throw;
    //        }
    //    }
    //}

    //public class CassetteLoadStart : ProbeCommand, ICassetteLoadStart
    //{
    //    public override bool Execute()
    //    {
    //        IGEMModule GEMModule = Container.Resolve<IGEMModule>();

    //        GEMModule.SetEvent(6451); //6451임

    //        return true;
    //    }
    //}

    //public class CassetteLoadDone : ProbeCommand, ICassetteLoadDone
    //{
    //    public override bool Execute()
    //    {
    //        try
    //        {
    //            this.GEMModule().SetEvent(6452);
    //        }
    //        catch (Exception err)
    //        {
    //            LoggerManager.Exception(err);
    //            throw;
    //        }

    //        return true;
    //    }
    //}
}
