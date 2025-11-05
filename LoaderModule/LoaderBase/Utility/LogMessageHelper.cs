using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LoaderBase
{
    public static class LogMessageHelper
    {
        public static string Generate(string message, params object[] inputValues)
        {
            StackTrace stackTrace = new StackTrace();
            var method = stackTrace.GetFrame(1).GetMethod();
            
            ParameterInfo[] paramInfos = method.GetParameters();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{method.DeclaringType.FullName}.{method.Name}() : {message}. ");

            if (inputValues != null && inputValues.Length > 0 && paramInfos.Length > 0)
            {
                sb.AppendLine("------------------------------------------------");
                for (int i = 0; i < paramInfos.Length; i++)
                {
                    if (i < inputValues.Length)
                    {
                        sb.AppendLine($"{paramInfos[i].Name} = {inputValues[i]}");
                    }
                }
                sb.AppendLine("------------------------------------------------");
            }
            return sb.ToString();
        }
    }
}
