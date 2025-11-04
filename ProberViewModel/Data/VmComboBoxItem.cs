using System;

namespace CognexOCRMainPageViewModel
{
    public class VmComboBoxItem
    {
        public String Name { get; set; }
        public Object CommandArg { get; set; }
        public VmComboBoxItem(String name, Object commandArg)
        {
            Name = name;
            CommandArg = commandArg;
        }
        public VmComboBoxItem(String name, String commandArg)
        {
            Name = name;
            CommandArg = commandArg;
        }
    }
}
