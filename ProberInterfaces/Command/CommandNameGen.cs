using System;

namespace ProberInterfaces.Command
{
    public class CommandNameGen
    {
        public static string Generate(Type type)
        {
            return type.FullName;
        }
    }
}
