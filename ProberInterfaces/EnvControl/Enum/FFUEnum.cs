namespace ProberInterfaces
{
    //0xABCD에서 B는 FFU Type, D는 사용유무
    public enum EnumFFUModuleMode
    {
        NONE = 0x0000,
        EMUL = 0x0101,
        SERIAL = 0x0201,
        REMOTE = 0x0202,
        IO = 0x0203
    }
}
