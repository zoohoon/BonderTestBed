using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProberInterfaces.Command.Internal
{
    #region => Bonder Sequence Command
    public interface IBonderStartCommand : IProbeCommand
    {
        // BonderStage임
    }
    public interface IBonderEndCommand : IProbeCommand
    {

    }
    #endregion

    #region => Pick Command
    public interface IPickCommand : IProbeCommand
    {

    }
    #endregion

    #region => Place Command
    public interface IPlaceCommand : IProbeCommand
    {

    }
    #endregion

    #region => Rotation Command
    public interface IRotationCommand : IProbeCommand
    {

    }
    #endregion

    #region => NanoAlgin Command
    public interface IDieAlign : IProbeCommand
    {

    }
    #endregion
}
