using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProberInterfaces.Enum
{
    public enum EnumPadColorType
    {
        INVALID = -1,
        UNDEFINED = 0,
        BLACK = UNDEFINED + 1,
        WHITE
    }

    public enum EnumPadShapeType
    {
        INVALID = -1,
        UNDEFINED = 0,
        SQUARE = UNDEFINED + 1,
        CIRCLE,
        POLYGON
    }
}
