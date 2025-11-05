namespace LoaderBase
{
    /// <summary>
    /// 카세트를 스캔하는 모듈임을 정의합니다.
    /// </summary>
    public interface ICassetteScanable : IAttachedModule
    {
        /// <summary>
        /// 카세트를 스캔할 수 있는지 여부를 가져옵니다.
        /// </summary>
        /// <param name="Cassette">카세트 모듈 인스턴스</param>
        /// <returns>스캔이 가능하면 true, 그렇지 않으면 false</returns>
        bool CanScan(ICassetteModule Cassette);
    }
}
