using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Controls;

namespace ProberInterfaces
{
    public interface IDllImporter
    {
        //T Assignable<T>(Assembly assemlist);
        //List<T> Assignable2<T>(Assembly assemlist);
        ObservableCollection<T> Assignable<T>(Assembly assemlist);
        List<string> GetAssignableInterfaceName(Assembly assembly);

        string GetUIFileName(string dllname);
        UserControl LoadUserControlDll();
        T LoadUserControlDll<T>(T type);

    }
}
