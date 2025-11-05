using LogModule;
using PostSharp.Aspects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProberInterfaces
{
    //[Serializable]
    //public class TimeThisAttribute : OnMethodBoundaryAspect, IFactoryModule
    //{
    //    [NonSerialized]
    //    private Stopwatch stopwatch;

    //    private int HashCode { get; set; }
    //    public override void OnEntry(MethodExecutionArgs args)
    //    {
    //        stopwatch = Stopwatch.StartNew();
    //        HashCode = GetHashCode();
    //        LoggerManager.Debug($"[{this.LotOPModule().ModuleState}], HashCode = {HashCode}, {args.Method.Name} : START");
    //    }

    //    public override void OnExit(MethodExecutionArgs args)
    //    {
    //        stopwatch.Stop();
    //        LoggerManager.Debug($"[{this.LotOPModule().ModuleState}], HashCode = {HashCode}, {args.Method.Name} : DONE, {stopwatch.Elapsed.TotalMilliseconds} ms");
    //    }
    //}
}
