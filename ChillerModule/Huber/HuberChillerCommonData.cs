namespace Temperature.Temp.Chiller
{
    public enum ModBusChillerEnum
    {
        //RW    /Setpoint
        SET_TEMP = 0x00,
        //R     /Internal temp.
        INTERNAL_TEMP = 0x01,
        //R     /Return temp
        RETURN_TEMP = 0x02,
        //R     /Pump pressure
        PUMP_PRESSURE = 0x03,
        //R     /Current Power
        CUR_POWER = 0x04,
        //R     /Error report
        ERROR = 0x05,
        //R     /Warning message
        WARNING = 0x06,
        //R     /Process Temp
        PROCESS_TEMP = 0x07,
        //RW    /Presetting, precess temp
        EXTMOVE = 0x09,
        //R     /Status of the thermostat
        STATUS1 = 0x0A,
        //RW    /PID Para
        AUTO_PID = 0x12,
        //RW    /Temp. control mode
        TMP_MODE = 0x13,
        //RW    /Temp. control activation
        TMP_ACTIVE = 0x14,
        //RW    /Stop or start thermostat circulation
        CIRC_ACTIVE = 0x16,
        //RW    /Touch panel lock
        OPERATING_LOCK = 0x17,
        //RW    /Process temp. actual value preset
        PROC_TEMP_PRESET = 0x19,
        //R     /serial nunber RL
        SERIAL_NUMBER_L = 0x1B,
        //R     /serial nunber RH
        SERIAL_NUMBER_H = 0x1C,
        //R     /pump speed
        PUMP_SPEED = 0x26,
        //RW    /Min. set point
        MIN_SET_TEMP = 0x30,
        //RW    /Max. set point
        MAX_SET_POINT = 0x31,
        //RW    /Set pump speed
        SET_TEMP_PUMP_SPEED = 0x48,

        //RW    /Upper alram internal limit.
        ALARAM_LIMIT_INTERNAL_UPPER = 0x51,
        //RW    /Lower alram external limit.
        ALARAM_LIMIT_INTERNAL_LOWER = 0x52,
        //RW    /Upper alram process limit.
        ALARAM_LIMIT_PROCESS_UPPER = 0x53,
        //RW    /Lower alram process limit.
        ALARAM_LIMIT_PROCESS_LOWER = 0x54
    }
}
