using System.Collections.Generic;
using ProberInterfaces;

namespace LoaderCore
{
    public interface IModulePathGenerator
    {
        PathStateBase CurrPathState { get; set; }
        TransferObject StagedTransferObject { get; set; }
        TransferObject SubsToTransfer { get; set; }

        List<ModuleTypeEnum> GetPath(TransferObject dst);
    }
}