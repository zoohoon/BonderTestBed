namespace ProberInterfaces.Command.Internal
{
    #region Stage Command 
    public interface IRUNCARDCHANGECOMMAND : IProbeCommand
    {
    }
    public interface IStagecardChangeStart : IProbeCommand
    {
    }
    public interface IAbortcardChange : IProbeCommand
    {
    }


    #endregion

    #region Loader Command
    /// <summary>
    /// PGV에 사용할 CC 데이터(ActiveCCInfo)의 SequnceId
    /// ActiveCCInfo를 한꺼번에 가지고 있지 않고 id만 전달해서 ActiveCCInfoList에서 찾아서 사용하기 위함. 
    /// // TODO: 만약 Start이후에 세션이 열린상태에서 AllocateActiveCCInfo가 또 불린다고하면 TRANSFERED 다음에 TRANSFER_READY로 와야함. 
    /// </summary>
    public class RequestCCJobInfo : ProbeCommandParameter
    {
        public string allocSeqId { get; set; }
    }

    public class TransferObjectHolderInfo : ProbeCommandParameter
    {
        public object source { get; set; }
        public object target { get; set; }
    }

    public interface ITransferObject : IProbeCommand
    {
    }

    public interface IStartCardChangeSequence : IProbeCommand
    {
    }
    public interface IAbortCardChangeSequence : IProbeCommand
    {
    }
   

    #endregion
}
