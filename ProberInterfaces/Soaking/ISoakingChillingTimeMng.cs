namespace ProberInterfaces.Soaking
{
    using ProberErrorCode;

    public interface ISoakingChillingTimeMng
    {
        void InitChillingTimeMng();
        bool ChillingTimeInit();
        EventCodeEnum GetCurrentChilling_N_TimeToSoaking(ref long accumulated_chillingTime, ref int SoakingTimeMil, ref bool InChillingTimeTable, bool UseAutomaticCalculatedChilingTime = true);
        EventCodeEnum ActualProcessedSoakingTime(long ProcessedSoakingTimeMil, long received_chillingTimeMil, bool completeSoaking);
        EventCodeEnum GetChillingTimeAccordingToSoakingTime(long ProcessedSoakingTimeMil, out long chillingTime);
        void CalculateChillingTime(long time_MilliSec);
        long GetChillingTime();
        bool GetCardDockingFlag();
        bool IsChuckPositionIncreaseChillingTime(bool debugLog, ref string PositionInfo);
        bool IsShowDebugString();
    }
}
