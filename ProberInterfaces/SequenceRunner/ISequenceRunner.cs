using ProberErrorCode;
using ProberInterfaces.CardChange;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ProberInterfaces.SequenceRunner
{
    public interface ISequenceRunner : IStateModule
    {
        EventCodeEnum AlternateManualMode();

        EventCodeEnum RunCardChange(bool IsTempSetBeforeOperation);
        EventCodeEnum RunTestHeadDockUndock(THDockType type, bool IsTempSetBeforeOperation);
        EventCodeEnum RunNcPadChage(bool IsTempSetBeforeOperation);
        EventCodeEnum RunRetry();
        EventCodeEnum RunReverse();
        EventCodeEnum RunManualRetry();
        EventCodeEnum RunManualReverse();

        SequenceRunnerStateEnum GetSequenceRunnerStateEnum();

        object GetSelectedSequence();

        IList<IOPortDescripter<bool>> GetInputPorts(IList<ISequenceBehaviorGroupItem> behaviorGroupItems);
        bool InitStepGroupCollection(IList<ISequenceBehaviorGroupItem> behaviorGroupItems);
    }

    public interface IBehaviorResult
    {
        EventCodeEnum ErrorCode { get; set; }
        IBehaviorResult InnerError { get;  }
        string Title { get; set; }
        string Reason { get; set; }
        bool PosNegBranch { get; set; }

        void SetInnerError(IBehaviorResult innererror);
        EventCodeEnum GetRootCause();
    }

    public interface ISequenceBehavior : INotifyPropertyChanged, IFactoryModule
    {
        ISequenceBehavior ReverseBehavior { get; set; }
        BehaviorFlag Flag { get; set; }
        List<IOPortDescripter<bool>> OutputPorts { get; set; }
        List<IOPortDescripter<bool>> InputPorts { get; set; }

        long WaitingTimeRemain { get; set; }

        string description { get; set; }
        string BehaviorID { get; set; }
        string NextID_Positive { get; set; }
        string NextID_Negative { get; set; }
        string SequenceDescription { get; set; }

        int InitModule();

        void SetReverseBehavior();

        Task<IBehaviorResult> Run();
        EventCodeEnum InitRun();

        string ToString();
    }

    public interface IEndBehavior : ISequenceBehavior
    {

    }

    public interface ISequenceBehaviorGroupItem : IFactoryModule
    {
        ObservableCollection<ISequenceBehaviorRun> IPreSafetyList { get; }
        ObservableCollection<ISequenceBehaviorRun> IPostSafetyList { get; }
        ISequenceBehavior IBehavior { get; }
        ISequenceBehaviorGroupItem IReverseBehaviorGroupItem { get; }
        ISequenceBehaviorState IBehaviorState { get; }

        String BehaviorID { get; }
        String NextID_Positive { get; }
        String NextID_Negative { get; }
        string SequenceDescription { get;}
        SequenceBehaviorStateEnum StateEnum { get; set; }

        void SequenceBehaviorStateTransition(ISequenceBehaviorState _BehaviorState);
        EventCodeEnum InitModule();
        void InitState();

        #region << RUN >>
        Task<IBehaviorResult> PreSafetyRun();
        Task<IBehaviorResult> BehaviorRun();
        Task<IBehaviorResult> PostSafetyRun();
        #endregion
    }

    public interface ISequenceBehaviorStruct : ISystemParameterizable
    {
        ObservableCollection<ISequenceBehaviorGroupItem> ICollectionBaseBehavior { get; }
        ObservableCollection<ISequenceBehavior> ICollectionLoadBehaviorOrder { get; }
    }
}
