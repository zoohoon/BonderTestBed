namespace SECSGEM.XGEM
{
    //public abstract class GemRC_Base
    //{
    //    protected Autofac.IContainer Container { get; set; }

    //    protected List<string> nameList = new List<string>();
    //    protected List<string> valList = new List<string>();
    //    protected string[] CPNames = null;
    //    protected long[] CPVals = null;
    //    protected string controlState;

    //    public abstract void Execute();
    //    public abstract long GetHCAck();

    //    public void SetRemoteList(Autofac.IContainer container, string controlState,
    //        List<string> nameList, List<string> valList)
    //    {
    //        Container = container;
    //        this.nameList = nameList;
    //        this.valList = valList;
    //        this.controlState = controlState;
    //    }

    //    public void GetCP(ref string[] CPName, ref long[] CPVals)
    //    {
    //        CPName = this.CPNames;
    //        CPVals = this.CPVals;
    //    }
    //}

    //public class GemRC_ABORT : GemRC_Base
    //{
    //    public override void Execute()
    //    {
    //    }

    //    public override long GetHCAck()
    //    {
    //        long retVal = -1;

    //        if(controlState != "REMOTE")
    //        {
    //            retVal = (long)SECSGEM.XGEM.HCACK.CANNOT_PERFORM_NOW;
    //        }
    //        else
    //        {
    //            retVal = (long)SECSGEM.XGEM.HCACK.HAS_BEEN_PERFORMED;
    //        }

    //        return retVal;
    //    }
    //}

    //public class GemRC_CC_START : GemRC_Base
    //{
    //    public override void Execute()
    //    {
    //    }

    //    public override long GetHCAck()
    //    {
    //        long retVal = -1;

    //        if (controlState != "REMOTE")
    //        {
    //            retVal = (long)SECSGEM.XGEM.HCACK.CANNOT_PERFORM_NOW;
    //        }
    //        else
    //        {
    //            retVal = (long)SECSGEM.XGEM.HCACK.HAS_BEEN_PERFORMED;
    //        }

    //        return retVal;
    //    }
    //}

    //public class GemRC_DLRECIPE : GemRC_Base
    //{
    //    public override void Execute()
    //    {
    //    }

    //    public override long GetHCAck()
    //    {
    //        long retVal = -1;

    //        if (controlState != "REMOTE")
    //        {
    //            retVal = (long)SECSGEM.XGEM.HCACK.CANNOT_PERFORM_NOW;
    //        }
    //        else
    //        {
    //            retVal = (long)SECSGEM.XGEM.HCACK.HAS_BEEN_PERFORMED;
    //        }

    //        return retVal;
    //    }
    //}

    //public class GemRC_PAUSE : GemRC_Base
    //{
    //    public override void Execute()
    //    {
    //    }

    //    public override long GetHCAck()
    //    {
    //        long retVal = -1;

    //        if (controlState != "REMOTE")
    //        {
    //            retVal = (long)SECSGEM.XGEM.HCACK.CANNOT_PERFORM_NOW;
    //        }
    //        else
    //        {
    //            retVal = (long)SECSGEM.XGEM.HCACK.HAS_BEEN_PERFORMED;
    //        }

    //        return retVal;
    //    }
    //}

    //public class GemRC_PSTART : GemRC_Base
    //{
    //    public IProberStation ProberStation
    //    {
    //        get
    //        {
    //            return Container.Resolve<IProberStation>();
    //        }
    //    }

    //    public override async void Execute()
    //    {
    //        //Task<EventCodeEnum> lotTask = ProberStation.RunLot();

    //        //await lotTask;
    //    }

    //    public override long GetHCAck()
    //    {
    //        long retVal = -1;

    //        if (controlState != "REMOTE")
    //        {
    //            retVal = (long)SECSGEM.XGEM.HCACK.CANNOT_PERFORM_NOW;
    //        }
    //        else
    //        {
    //            retVal = (long)SECSGEM.XGEM.HCACK.HAS_BEEN_PERFORMED;
    //        }

    //        return retVal;
    //    }
    //}

    //public class GemRC_PW_REQUEST : GemRC_Base
    //{
    //    public override void Execute()
    //    {
    //    }

    //    public override long GetHCAck()
    //    {
    //        long retVal = -1;

    //        if (controlState != "REMOTE")
    //        {
    //            retVal = (long)SECSGEM.XGEM.HCACK.CANNOT_PERFORM_NOW;
    //        }
    //        else
    //        {
    //            retVal = (long)SECSGEM.XGEM.HCACK.HAS_BEEN_PERFORMED;
    //        }

    //        return retVal;
    //    }
    //}

    //public class GemRC_RESTART : GemRC_Base
    //{
    //    public override void Execute()
    //    {
    //    }

    //    public override long GetHCAck()
    //    {
    //        return (long)SECSGEM.XGEM.HCACK.HAS_BEEN_PERFORMED; long retVal = -1;

    //        if (controlState != "REMOTE")
    //        {
    //            retVal = (long)SECSGEM.XGEM.HCACK.CANNOT_PERFORM_NOW;
    //        }
    //        else
    //        {
    //            retVal = (long)SECSGEM.XGEM.HCACK.HAS_BEEN_PERFORMED;
    //        }

    //        return retVal;
    //    }
    //}

    //public class GemRC_RESUME : GemRC_Base
    //{
    //    public override void Execute()
    //    {
    //    }

    //    public override long GetHCAck()
    //    {
    //        long retVal = -1;

    //        if (controlState != "REMOTE")
    //        {
    //            retVal = (long)SECSGEM.XGEM.HCACK.CANNOT_PERFORM_NOW;
    //        }
    //        else
    //        {
    //            retVal = (long)SECSGEM.XGEM.HCACK.HAS_BEEN_PERFORMED;
    //        }

    //        return retVal;
    //    }
    //}

    //public class GemRC_START : GemRC_Base
    //{
    //    public override void Execute()
    //    {
    //    }

    //    public override long GetHCAck()
    //    {
    //        long retVal = -1;

    //        if (controlState != "REMOTE")
    //        {
    //            retVal = (long)SECSGEM.XGEM.HCACK.CANNOT_PERFORM_NOW;
    //        }
    //        else
    //        {
    //            retVal = (long)SECSGEM.XGEM.HCACK.HAS_BEEN_PERFORMED;
    //        }

    //        return retVal;
    //    }
    //}


    //public class GemRC_STOP : GemRC_Base
    //{
    //    public override void Execute()
    //    {
    //    }

    //    public override long GetHCAck()
    //    {
    //        long retVal = -1;

    //        if (controlState != "REMOTE")
    //        {
    //            retVal = (long)SECSGEM.XGEM.HCACK.CANNOT_PERFORM_NOW;
    //        }
    //        else
    //        {
    //            retVal = (long)SECSGEM.XGEM.HCACK.HAS_BEEN_PERFORMED;
    //        }

    //        return retVal;
    //    }
    //}

    //public class GemRC_UNDOCK : GemRC_Base
    //{
    //    public override void Execute()
    //    {
    //    }

    //    public override long GetHCAck()
    //    {
    //        long retVal = -1;

    //        if (controlState != "REMOTE")
    //        {
    //            retVal = (long)SECSGEM.XGEM.HCACK.CANNOT_PERFORM_NOW;
    //        }
    //        else
    //        {
    //            retVal = (long)SECSGEM.XGEM.HCACK.WILL_BE_PERFORMED;
    //        }

    //        return retVal;
    //    }
    //}

    //public class GemRC_WFCLN : GemRC_Base
    //{
    //    public override void Execute()
    //    {
    //    }

    //    public override long GetHCAck()
    //    {
    //        long retVal = -1;

    //        if (controlState != "REMOTE")
    //        {
    //            retVal = (long)SECSGEM.XGEM.HCACK.CANNOT_PERFORM_NOW;
    //        }
    //        else
    //        {
    //            retVal = (long)SECSGEM.XGEM.HCACK.HAS_BEEN_PERFORMED;
    //        }

    //        return retVal;
    //    }
    //}

    //public class GemRC_WFIDCONFPROC : GemRC_Base
    //{
    //    public override void Execute()
    //    {
    //    }

    //    public override long GetHCAck()
    //    {
    //        long retVal = -1;

    //        if (controlState != "REMOTE")
    //        {
    //            retVal = (long)SECSGEM.XGEM.HCACK.CANNOT_PERFORM_NOW;
    //        }
    //        else
    //        {
    //            retVal = (long)SECSGEM.XGEM.HCACK.HAS_BEEN_PERFORMED;
    //        }

    //        return retVal;
    //    }
    //}

    //public class GemRC_ZIF_REQUEST : GemRC_Base
    //{
    //    public override void Execute()
    //    {
    //    }

    //    public override long GetHCAck()
    //    {
    //        long retVal = -1;

    //        if (controlState != "REMOTE")
    //        {
    //            retVal = (long)SECSGEM.XGEM.HCACK.CANNOT_PERFORM_NOW;
    //        }
    //        else
    //        {
    //            retVal = (long)SECSGEM.XGEM.HCACK.HAS_BEEN_PERFORMED;
    //        }

    //        return retVal;
    //    }
    //}
}
