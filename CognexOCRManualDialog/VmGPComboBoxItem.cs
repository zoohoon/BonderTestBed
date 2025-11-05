using System;

namespace CognexOCRManualDialog
{
    using LogModule;
    public class VmGPComboBoxItem
    {
        public String Name { get; set; }
        public Object CommandArg { get; set; }
        public VmGPComboBoxItem(String name, Object commandArg)
        {
            try
            {
                Name = name;
                CommandArg = commandArg;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public VmGPComboBoxItem(String name, String commandArg)
        {
            try
            {
                Name = name;
                CommandArg = commandArg;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
