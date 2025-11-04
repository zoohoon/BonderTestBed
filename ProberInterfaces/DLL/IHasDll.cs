using ProberErrorCode;
using System.Collections.Generic;

namespace ProberInterfaces
{
    public interface IHasDll
    {
        Queue<ModuleDllInfo> DllInfos { get; set; }
        void InsertDllInfo(ModuleDllInfo DllInfo);

        bool IsDllLoad();
        EventCodeEnum LoadDll();
    }
}
