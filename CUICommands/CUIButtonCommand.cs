using System;

namespace CUICommands
{
    using CUI;
    using LogModule;
    using ProberInterfaces.Command;
    using ProberInterfaces.Command.Cui;

    public class CUIButtonCommand : ProbeCommand, ICUIButtonCommand
    {
        public override bool Execute()
        {
            try
            {
                CUIButtonCommandParam param = Parameter as CUIButtonCommandParam;
                if (param == null)
                    return false;

                Button button = null;
                CUIManager.ButtonList.TryGetValue(param.UIGUID, out button);
                if (button == null)
                    return false;

                button.OnClickEventRaise();
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return true;
        }
    }
}
