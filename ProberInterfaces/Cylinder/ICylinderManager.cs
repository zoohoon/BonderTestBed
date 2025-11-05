using System.Collections.Generic;
using ProberErrorCode;

namespace ProberInterfaces
{
    public interface ICylinderManager : IFactoryModule, IModule
    {
        //void Extend(ICylinderType cylinder);
        //void Retract(ICylinderType cylinder);

        //int sinqCylinderMove(IOPortDescripter<bool> Output,
        //                            bool Olevel,
        //                            IOPortDescripter<bool> Output2,
        //                            bool O2level,
        //                            IOPortDescripter<bool> Input,
        //                            bool Ilevel,
        //                            IOPortDescripter<bool> Input2,
        //                            bool I2level,
        //                            long maintainTime = 400,
        //                            long timeout = 60000);

        //IStageCylinderType StageCylinders { get; }
        //ILoaderCylinderType LoaderCylinders { get; }
        //IFoupCylinderType FoupCylinders { get; }
        ICylinderType Cylinders { get; }
        EventCodeEnum LoadParameter(string FullPath, List<IOPortDescripter<bool>> IOMap, CylinderParams DefaultParam = null);
    }
}
