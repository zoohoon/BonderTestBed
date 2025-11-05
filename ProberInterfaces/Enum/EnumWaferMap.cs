namespace ProberInterfaces
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    /// 
    public enum VIEW_TYPE
    {
        NONE,
        PMI,
        PROBING,
        PROBING_BIN,
        PROBING_DUT_COLOR
    };

    /// <summary> 
    /// 선택되어진 Die 의 이동방향 열거자 
    /// </summary>
    public enum MOVE_DIR
    {
        MOVE_UP,
        MOVE_DOWN,
        MOVE_LEFT,
        MOVE_RIGHT,
        MOVE_UPRIGHT,
        MOVE_UPLEFT,
        MOVE_DOWNRIGHT,
        MOVE_DOWNLEFT,
        MOVE_MAX,
    };
    /// <summary> 
    /// Die 의 상태 값 표시 열거자
    /// </summary>
    public enum DIE_STATUS
    {
        NOT_EXIST_DIE = 0,
        TEST_DIE,
        SKIP_DIE,
        MARK_DIE,
        PASS_DIE,
        FAIL_DIE,
        CURRENT_DIE,
        TEACH_DIE,
        SAMPLE_DIE,
        DIE_STATUS_MAX,
    };
    /// <summary> 
    /// 사용자 좌표계의 X,Y + 방향 열거자 
    /// </summary>
    public enum MAP_POSITIVE_DIRECTION
    {
        MAP_DIR_UP,
        MAP_DIR_DOWN,
        MAP_DIR_RIGHT,
        MAP_DIR_LEFT,
    };
    /// <summary>
    /// 로그 레벨 열거자
    /// </summary>
    public enum LOG_WAFER_LV
    {
        Off = 0,
        Fatal,
        Error,
        Warn,
        Info,
        Debug,
        Trace,
        All
    };
}
