using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ProberInterfaces.SequenceRunner
{
    public interface ISequenceBehaviorSafety : IFactoryModule, ISequenceBehaviorRun
    {
        List<IOPortDescripter<bool>> InputPorts { get; set; }
        List<IOPortDescripter<bool>> OutputPorts { get; set; }

        int InitModule();
        
        //Task<IBehaviorResult> Run();
    }

    public interface ISequenceBehaviorRun : IFactoryModule
    {
        //List<IOPortDescripter<bool>> InputPorts { get; set; }
        //List<IOPortDescripter<bool>> OutputPorts { get; set; }

        //int InitModule();
        Task<IBehaviorResult> Run();
    }

    public interface ISequenceBehaviors : ISystemParameterizable
    {
        ObservableCollection<ISequenceBehaviorGroupItem> ISequenceBehaviorCollection { get; }       
    }
}
