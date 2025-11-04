using System;

namespace CUICommands
{
    using CUI;
    using LogModule;
    using ProberInterfaces.Command;
    using ProberInterfaces.Command.Cui;

    public class CUITreeViewCommand : ProbeCommand, ICUITreeViewCommand
    {
        public override bool Execute()
        {
            try
            {
                CUITreeViewCommandParam param = Parameter as CUITreeViewCommandParam;
                if (param == null)
                    return false;

                TreeView treeView = null;
                CUIManager.TreeViewList.TryGetValue(param.UIGUID, out treeView);
                if (treeView == null)
                    return false;

                treeView.OnSelectedItemChangedEventRaise(param.Text);
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
