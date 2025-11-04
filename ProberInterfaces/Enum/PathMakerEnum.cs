namespace ProberInterfaces
{
    public enum PathMarkupCommandEnum
    {
        Start = 0,
        Point,
        Line,
        Arc,
        End,
        Clear
    }

    public enum CursorEnum
    {
        TOPLEFT = 0,
        UP,
        TOPRIGHT,
        LEFT,
        RIGHT,
        BOTTOMLEFT,
        DOWN,
        BOTTOMRIGHT
    }
}
