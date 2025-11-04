using LogModule;
using System;

namespace ProberInterfaces
{
    public static class FocusingDLLInfo
    {
        public static ModuleDllInfo GetNomalFocusingDllInfo()
        {
            try
            {
                return new ModuleDllInfo(
                             @"Focusing.dll",
                             "Focusing",
                             @"NormalFocusing",
                             1000,
                             true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
    }
}
