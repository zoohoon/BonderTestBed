namespace FoupModules.Template.MINI
{
    //public class MINIFoupDockingPortDoor : DockingPortDoorBase
    //{
    //    private MINIDPDoorState _State;

    //    public MINIDPDoorState State
    //    {
    //        get { return _State; }
    //        set { _State = value; }
    //    }
    //    #region // ITemplateModule implementation.
    //    public bool Initialized { get; set; } = false;
    //    public void DeInitModule()
    //    {

    //    }
    //    public EventCodeEnum InitModule()
    //    {
    //        EventCodeEnum ret = EventCodeEnum.UNDEFINED;
    //        ret = StateInit();
    //        if (ret != EventCodeEnum.NONE)
    //        {
    //            LoggerManager.Debug($"GPFoupCover12Inch.InitModule(): Init. error. Ret = {ret}");
    //        }
    //        return EventCodeEnum.NONE;
    //    }
    //    public EventCodeEnum ParamValidation()
    //    {
    //        return EventCodeEnum.NONE;
    //    }
    //    public bool IsParameterChanged(bool issave = false)
    //    {
    //        bool retVal = false;
    //        try
    //        {
    //        }
    //        catch (Exception err)
    //        {
    //            LoggerManager.Exception(err);
    //        }
    //        return retVal;
    //    }

    //    public override DockingPortDoorStateEnum GetState()
    //    {
    //        return State.GetState();
    //    }

    //    public override EventCodeEnum StateInit()
    //    {
    //        EventCodeEnum retVal;
    //        try
    //        {
    //            bool value;

    //            int ret = FoupIOManager.ReadBit(FoupIOManager.IOMap.Inputs.DI_FO_OPEN, out value);

    //            if (ret != 0)
    //            {
    //                State = new DockingPortDoorError(this);
    //                return EventCodeEnum.FOUP_ERROR;
    //            }
    //            else
    //            {
    //                if (value == true)
    //                {
    //                    State = new DockingPortDoorOpen(this);
    //                    retVal = EventCodeEnum.NONE;
    //                }
    //                else
    //                {
    //                    State = new DockingPortDoorClose(this);
    //                    retVal = EventCodeEnum.NONE;
    //                }
    //            }
    //        }
    //        catch (Exception err)
    //        {
    //            LoggerManager.Exception(err);
    //            throw;
    //        }
    //        return retVal;
    //    }

    //    public override EventCodeEnum Open()
    //    {
    //        return State.Open();
    //    }

    //    public override EventCodeEnum Close()
    //    {
    //        return State.Close();
    //    }
    //    #endregion
    //    public MINIFoupDockingPortDoor()
    //    {

    //    }

    //}
    //public abstract class MINIDPDoorState
    //{
    //    public MINIDPDoorState(DockingPortDoorNomal _module)
    //    {
    //        Module = _module;
    //    }
    //    private DockingPortDoorNomal _Module;

    //    public DockingPortDoorNomal Module
    //    {
    //        get { return _Module; }
    //        set { _Module = value; }
    //    }

    //    public abstract DockingPortDoorStateEnum GetState();

    //    public abstract EventCodeEnum Open();
    //    public abstract EventCodeEnum Close();
    //}

}
