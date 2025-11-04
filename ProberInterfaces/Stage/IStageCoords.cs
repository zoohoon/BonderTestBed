using System.ComponentModel;
using ProberInterfaces.Param;

namespace ProberInterfaces
{
    public interface IStageCoords
    {
        CatCoordinates MarkEncPos { get; set; }
        CatCoordinates MarkPosInChuckCoord { get; set; }

        event PropertyChangedEventHandler PropertyChanged;
    }
}
