namespace ProbingDataInterface
{
    public enum TestState
    {
        MAP_STS_NOT_EXIST = 0,           //'color =  8 --> Gray
        MAP_STS_TEST = 1,          //'color = 10 --> Light Green
        MAP_STS_SKIP = 2,           //'color = 14 --> Light Yellow
        MAP_STS_MARK = 3,           //'color = 11 --> Light Cyan
        MAP_STS_PASS = 4,           //'color =  9 --> Light Blue
        MAP_STS_FAIL = 5,           //'color = 12 --> Light Red
        MAP_STS_CUR_DIE = 6,           //'color = 13 --> Light Magenta
        MAP_STS_TEACH = 7               //'color =  4 --> Red
    }
}
