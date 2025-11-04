using System;
using System.Collections.Generic;

namespace CUI
{
    using LogModule;
    using ProberErrorCode;
    using System.Windows.Controls;
    public static class CUIManager
    {
        public static Dictionary<String, Button> ButtonList { get; set; }
        public static Dictionary<String, TreeViewItem> TreeViewItemList { get; set; }
        public static Dictionary<String, TreeView> TreeViewList { get; set; }
        public static bool Initialized { get; set; } = false;

        public static EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    ButtonList = new Dictionary<String, Button>();
                    TreeViewList = new Dictionary<String, TreeView>();
                    TreeViewItemList = new Dictionary<String, TreeViewItem>();

                    Initialized = true;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN CUIManager");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
}
