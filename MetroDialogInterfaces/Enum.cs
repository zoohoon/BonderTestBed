namespace MetroDialogInterfaces
{
    public enum EnumMessageDialogResult
    {
        INVALID = -1,
        UNDEFIND = 0,
        AFFIRMATIVE = UNDEFIND + 1,
        NEGATIVE,
        CANCEL,
        NO,
        FirstAuxiliary,
        SecondAuxiliary
    }

    public enum EnumMessageStyle
    {
        //
        // 요약:
        //     Just "OK"
        Affirmative = 0,
        //
        // 요약:
        //     "OK" and "Cancel"
        AffirmativeAndNegative = 1,
        AffirmativeAndNegativeAndSingleAuxiliary = 2,
        AffirmativeAndNegativeAndDoubleAuxiliary = 3
    }
}
