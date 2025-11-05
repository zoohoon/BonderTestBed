using System.IO;

namespace FoupModules
{
    public static class FoupGlobal
    {
        public static string ParamRootPath = @"C:\ProberSystem\Default";

        public static string SystemParameterPath = @"Parameters\Foup\FoupSystem.json";

        public static string DeviceParameterPath = @"Parameters\Foup\FoupDevice.json";

        //public static string SystemParameterPath = @"C:\ProberSystem\Default\Parameters\Loader\LoaderSystem.xml";

        //public static string DeviceParameterPath = @"C:\ProberSystem\Default\Parameters\Loader\LoaderDevice.xml";

        public static Autofac.IContainer Container { get; set; }

        public static string GetSystemParameterFullPath()
        {
            return Path.Combine(ParamRootPath, SystemParameterPath);
        }

        public static string GetDeviceParameterFullPath()
        {
            return Path.Combine(ParamRootPath, DeviceParameterPath);
        }
    }
}
