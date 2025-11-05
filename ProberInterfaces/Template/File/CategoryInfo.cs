using LogModule;
using System;

namespace ProberInterfaces
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    [Serializable]
    public class CategoryInfo : CategoryModuleBase, INotifyPropertyChanged
    {
        public CategoryInfo()
        {

        }
        public CategoryInfo(string name)
        {
            Header = name;
        }

        public CategoryInfo(string name, ObservableCollection<CategoryModuleBase> categories)
        {
            try
            {
            Header = name;
            Categories = categories;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
    }
}
