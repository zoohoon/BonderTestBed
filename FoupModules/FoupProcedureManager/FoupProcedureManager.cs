using ProberErrorCode;
using ProberInterfaces.Foup;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using LogModule;

namespace FoupProcedureManagerProject
{
    public class FoupProcedureManager : IFoupProcedureManager, INotifyPropertyChanged, IFoupProcRunManager
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public FoupProcedureStateMaps LoadProcedureStateMaps { get; set; } = new FoupProcedureStateMaps();
        public FoupProcedureStateMaps UnloadProcedureStateMaps { get; set; } = new FoupProcedureStateMaps();

        private IFoupProcedureStateMaps _SelectedProcedureStateMaps;
        public IFoupProcedureStateMaps SelectedProcedureStateMaps
        {
            get { return _SelectedProcedureStateMaps; }
            set
            {
                _SelectedProcedureStateMaps = value;
                RaisePropertyChanged();
            }
        }

        public LinkedListNode<FoupProcedureStateMap> SelectedProcedureStateMapNode { get; set; } = null;
        //private FoupProcedureStateMaps SelectedProcedureStateMapNode { get; set; } = null;

        public IFoupProcedure GetSelectedProcedureStateMapNode()
        {
            IFoupProcedure procedurestatemapnode = null;

            try
            {
                if(SelectedProcedureStateMapNode != null)
                {
                    procedurestatemapnode = SelectedProcedureStateMapNode.Value.Procedure;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return procedurestatemapnode;
        }
        public int GetSelectedProcedureIndex()
        {
            int index = 0;

            try
            {
                if (SelectedProcedureStateMapNode != null)
                {
                    for (var node= SelectedProcedureStateMapNode.List.First;node!=null;node=node.Next, index++)
                    {
                        if (SelectedProcedureStateMapNode.Value.Equals(node.Value))
                            return index;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return index;
        }

        public void InitProcedures()
        {
            try
            {
                foreach (var loadProcedureStateMap in LoadProcedureStateMaps)
                {
                    loadProcedureStateMap.ProcedureState = new FoupProcedureIdle();
                }

                foreach (var unloadProcedureStateMap in UnloadProcedureStateMaps)
                {
                    unloadProcedureStateMap.ProcedureState = new FoupProcedureIdle();
                }

                SelectedProcedureStateMapNode = null;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        //LOAD SEQUENCE인지, UNLOAD SEQUENCE 인지 찾는 Method
        public FoupStateEnum FindFoupState() 
        {
            FoupStateEnum state = FoupStateEnum.UNDEFIND;

            try
            {
                foreach (var load in LoadProcedureStateMaps)
                {
                    if(SelectedProcedureStateMapNode.Value.Procedure == load.Procedure)
                    {
                        return FoupStateEnum.LOAD;
                    }
                }

                foreach (var unload in UnloadProcedureStateMaps)
                {
                    if (SelectedProcedureStateMapNode.Value.Procedure == unload.Procedure)
                    {
                        return FoupStateEnum.UNLOAD;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return state;
        }

        public EventCodeEnum ReverseRun()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                retVal = SelectedProcedureStateMapNode.Value.Procedure.ReverseProcedure.PreSafetiesRun();

                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = SelectedProcedureStateMapNode.Value.Procedure.ReverseProcedure.BehaviorRun();
                }

                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = SelectedProcedureStateMapNode.Value.Procedure.ReverseProcedure.PostSafetiesRun();
                }
                //if (retVal == EventCodeEnum.NONE)
                //{
                //    SelectedProcedureStateMapNode = SelectedProcedureStateMapNode.Previous;
                //}
                //else
                //{
                //    // ???
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public EventCodeEnum PreviousRun()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                if (SelectedProcedureStateMapNode != null && SelectedProcedureStateMapNode.Previous != null)
                {
                    if(SelectedProcedureStateMapNode.Previous.Value.Procedure.Caption == "FOUP COVER DOWN")
                    {
                        return EventCodeEnum.FOUP_SEQUENCE_NULL;
                    }

                    SelectedProcedureStateMapNode = SelectedProcedureStateMapNode.Previous;
                    LoggerManager.RecoveryLog($"Pre Foup Sequence : {SelectedProcedureStateMapNode.Value.Procedure.Caption}, Clear Sequence state");

                    retVal = SelectedProcedureStateMapNode.Value.PreviousRun();
                    if(retVal == EventCodeEnum.NONE)
                    {
                        SelectedProcedureStateMapNode.Value.Procedure.Behavior.FoupModule.BroadcastFoupStateAsync();
                        return retVal;
                    }
                    else
                    {
                        List<string> presaftieserrorlist = new List<string>();
                        List<string> postsaftieserrorlist = new List<string>();

                        foreach (var safety in SelectedProcedureStateMapNode.Value.Procedure.PreSafeties_Recovery)
                        {
                            if (safety.State != FoupSafetyStateEnum.DONE)
                            {
                                int index = safety.ToString().IndexOf("I_");

                                if (index != -1)
                                {
                                    string parsedContent = safety.ToString().Substring(index + 2);
                                    presaftieserrorlist.Add(parsedContent);
                                }

                            }
                        }

                        foreach (var safety in SelectedProcedureStateMapNode.Value.Procedure.PostSafeties_Recovery)
                        {
                            if (safety.State != FoupSafetyStateEnum.DONE)
                            {
                                int index = safety.ToString().IndexOf("I_");

                                if (index != -1)
                                {
                                    string parsedContent = safety.ToString().Substring(index + 2);
                                    postsaftieserrorlist.Add(parsedContent);
                                }

                            }
                        }

                        if (presaftieserrorlist.Count > 0)
                        {
                            SelectedProcedureStateMapNode.Value.Procedure.Behavior.FoupModule.ShowSequenceErrorMessage("PreSafties Error", presaftieserrorlist);
                        }

                        if (postsaftieserrorlist.Count > 0)
                        {
                            SelectedProcedureStateMapNode.Value.Procedure.Behavior.FoupModule.ShowSequenceErrorMessage("PostSafties Error", postsaftieserrorlist);
                        }
                        retVal = EventCodeEnum.FOUP_ERROR;
                    }

                }
                else
                {
                    retVal = EventCodeEnum.FOUP_SEQUENCE_NULL;
                    LoggerManager.RecoveryLog($"Reason : {retVal}", true);

                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum LoadRun()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                InitProcedures();
                if (SelectedProcedureStateMapNode == null)
                {
                    SelectedProcedureStateMaps = LoadProcedureStateMaps;

                    SelectedProcedureStateMapNode = (SelectedProcedureStateMaps as FoupProcedureStateMaps).First;
                }

                retVal = ProceduresRun();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum UnloadRun()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                InitProcedures();
                if (SelectedProcedureStateMapNode == null)
                {
                    SelectedProcedureStateMaps = UnloadProcedureStateMaps;
                    SelectedProcedureStateMapNode = (SelectedProcedureStateMaps as FoupProcedureStateMaps).First;
                }

                retVal = ProceduresRun();

                //var task = AsyncProceduresRun();

                //retVal = task.Result;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
        private EventCodeEnum ProceduresRun()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                bool EndFlag = false;

                while (SelectedProcedureStateMapNode != null)
                {
                    retVal = SelectedProcedureStateMapNode.Value.Run();

                    SelectedProcedureStateMapNode.Value.Procedure.Behavior.FoupModule.BroadcastFoupStateAsync();

                    if (retVal != EventCodeEnum.NONE)
                    {
                        EndFlag = false;
                        break;
                    }
                    else
                    {
                        EndFlag = true;
                        SelectedProcedureStateMapNode = SelectedProcedureStateMapNode.Next;

                    }
                }

                if (EndFlag == true)
                {
                    SelectedProcedureStateMapNode = null;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
        public EventCodeEnum NextRun()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                if (SelectedProcedureStateMapNode != null)
                {
                    retVal = SelectedProcedureStateMapNode.Value.Run();

                    if (retVal == EventCodeEnum.NONE)
                    {
                        SelectedProcedureStateMapNode = SelectedProcedureStateMapNode.Next;
                        SelectedProcedureStateMapNode.Value.Procedure.Behavior.FoupModule.BroadcastFoupStateAsync();

                    }
                    else
                    {
                        // ???
                    }
                }
                else
                {
                    // ???
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
        public EventCodeEnum ContinueRun()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                if (SelectedProcedureStateMapNode != null)
                {
                    LoggerManager.RecoveryLog($"RUN Foup Sequence : {SelectedProcedureStateMapNode.Value.Procedure.Caption}");

                    retVal = SelectedProcedureStateMapNode.Value.RecoveryRun();

                    if (retVal == EventCodeEnum.NONE)
                    {
                        SelectedProcedureStateMapNode = SelectedProcedureStateMapNode.Next;

                        if (SelectedProcedureStateMapNode == null)
                        {
                            retVal = EventCodeEnum.FOUP_SEQUENCE_END;
                            LoggerManager.RecoveryLog($"{retVal}");
                        }
                        else
                        {
                            SelectedProcedureStateMapNode.Value.Procedure.Behavior.FoupModule.BroadcastFoupStateAsync();
                        }
                    }
                    else
                    {
                        //SelectedProcedureStateMapNode.Value.cedureStateEnum PreSafetyError,PostSafetyError,BehaviorError

                        List<string> presaftieserrorlist = new List<string>();
                        List<string> postsaftieserrorlist = new List<string>();

                        foreach (var safety in SelectedProcedureStateMapNode.Value.Procedure.PreSafeties_Recovery)
                        {
                            if (safety.State != FoupSafetyStateEnum.DONE)
                            {
                                int index = safety.ToString().IndexOf("I_");

                                if (index != -1)
                                {
                                    string parsedContent = safety.ToString().Substring(index + 2);
                                    presaftieserrorlist.Add(parsedContent);
                                }

                            }
                        }

                        foreach (var safety in SelectedProcedureStateMapNode.Value.Procedure.PostSafeties_Recovery)
                        {
                            if (safety.State != FoupSafetyStateEnum.DONE)
                            {
                                int index = safety.ToString().IndexOf("I_");

                                if (index != -1)
                                {
                                    string parsedContent = safety.ToString().Substring(index + 2);
                                    postsaftieserrorlist.Add(parsedContent);
                                }

                            }
                        }

                        if(presaftieserrorlist.Count > 0)
                        {
                            SelectedProcedureStateMapNode.Value.Procedure.Behavior.FoupModule.ShowSequenceErrorMessage("PreSafties Error", presaftieserrorlist);
                        }

                        if (postsaftieserrorlist.Count > 0)
                        {
                            SelectedProcedureStateMapNode.Value.Procedure.Behavior.FoupModule.ShowSequenceErrorMessage("PostSafties Error", postsaftieserrorlist);
                        }
                        retVal = EventCodeEnum.FOUP_ERROR;
                    }
                }
                else
                {
                    retVal = EventCodeEnum.FOUP_SEQUENCE_NULL;
                    LoggerManager.RecoveryLog($"Reason : {retVal}", true);

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            finally
            {
                if(retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.RecoveryLog($"Reason : {retVal}", true);
                }
            }

            return retVal;
        }
        public EventCodeEnum FastForwardRun()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                retVal = ProceduresRun();

                //retVal = ProceduresRun(ref isEndProcedure);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
        public EventCodeEnum FastBackwardRun()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                retVal = PreviousRun();

                //retVal = ProceduresRun(ref isEndProcedure);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public void SettingProcedure(List<IFoupProcedure> foupProcedures
                                    , List<string> LoadOrderList
                                    , List<string> UnloadOrderList
                                    , IFoupModule FoupModule
                                    , IFoupIOStates FoupIOManager)
        {
            try
            {
                this.LoadProcedureStateMaps.Clear();
                this.UnloadProcedureStateMaps.Clear();

                SetFoupModule(foupProcedures, FoupModule);
                SetFoupIOManager(foupProcedures, FoupIOManager);

                SetReverse(foupProcedures);

                SetLoadProcedureStateMaps(LoadProcedureStateMaps, foupProcedures, LoadOrderList);
                SetLoadProcedureStateMaps(UnloadProcedureStateMaps, foupProcedures, UnloadOrderList);

                foreach (var procedure in foupProcedures)
                {
                    procedure.Behavior.InitBehavior();
                }
                InitProcedures();

                //Procedures = SelectedProcedureStateMapNode;

                //LoadProcedure = LoadProcedureStateMaps;
                //UnloadProcedure = UnloadProcedureStateMaps;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void SetFoupModule(List<IFoupProcedure> procs, IFoupModule FoupModule)
        {
            try
            {
                foreach (var Procedure in procs)
                {
                    FoupProcedure proc = Procedure as FoupProcedure;

                    proc.Behavior.FoupModule = FoupModule;

                    foreach (var Item in proc.PreSafeties_Recovery)
                    {
                        Item.FoupModule = FoupModule;
                    }
                    foreach (var Item in proc.PostSafeties_Recovery)
                    {
                        Item.FoupModule = FoupModule;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void SetFoupIOManager(List<IFoupProcedure> procs, IFoupIOStates FoupIOManager)
        {
            try
            {
                foreach (var Procedure in procs)
                {
                    FoupProcedure proc = Procedure as FoupProcedure;

                    proc.Behavior.FoupIOManager = FoupIOManager;

                    foreach (var Item in proc.PreSafeties_Recovery)
                    {
                        Item.FoupIOManager = FoupIOManager;
                    }
                    foreach (var Item in proc.PostSafeties_Recovery)
                    {
                        Item.FoupIOManager = FoupIOManager;
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void SetReverse(List<IFoupProcedure> procs)
        {
            try
            {
                List<FoupProcedure> foupProcs = new List<FoupProcedure>();

                foreach (var item in procs)
                {
                    foupProcs.Add(item as FoupProcedure);
                }


                foreach (var procedure in foupProcs)
                {
                    FoupProcedure findProcedure =
                        foupProcs.Find(i => i.Behavior.GetType().Name == procedure.ReverseProcedureName);
                    if (findProcedure != null)
                    {
                        procedure.ReverseProcedure = findProcedure;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void SetLoadProcedureStateMaps(FoupProcedureStateMaps targetStateMaps
                                            , List<IFoupProcedure> foupProcedures
                                            , List<string> orderlist)
        {
            try
            {
                foreach (var procedure in orderlist)
                {
                    FoupProcedure findProcedure = (FoupProcedure)foupProcedures.Find(i => i.Behavior.GetType().Name == procedure);
                    if (findProcedure != null)
                    {
                        targetStateMaps.AddLast(new FoupProcedureStateMap() { Procedure = findProcedure });
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public IFoupProcedureStateMaps GetProcedures(FoupStateEnum foupstate)
        {
            IFoupProcedureStateMaps procedures = null;

            try
            {
                if(foupstate == FoupStateEnum.LOAD)
                {
                    procedures = (IFoupProcedureStateMaps)this.LoadProcedureStateMaps;
                }
                else if(foupstate == FoupStateEnum.UNLOAD)
                {
                    procedures = (IFoupProcedureStateMaps)this.UnloadProcedureStateMaps;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return procedures;
        }
        public void SetDockingPlateProcedure(DockingPlateStateEnum dockingplatestate)
        {
            try
            {
                if (dockingplatestate == DockingPlateStateEnum.LOCK)
                {
                    (SelectedProcedureStateMaps as FoupProcedureStateMaps).First.Value.ProcedureState = new FoupProcedureDone();
                    if (SelectedProcedureStateMapNode.Value.Procedure == (SelectedProcedureStateMaps as FoupProcedureStateMaps).First.Value.Procedure)
                    {
                        SelectedProcedureStateMapNode = SelectedProcedureStateMapNode.Next;
                    }
                }
                else
                {
                    (SelectedProcedureStateMaps as FoupProcedureStateMaps).First.Value.ProcedureState = new FoupProcedureIdle();
                    SelectedProcedureStateMapNode = (SelectedProcedureStateMaps as FoupProcedureStateMaps).First;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetCoverDownProcedure(FoupCoverStateEnum coverstate)
        {
            try
            {
                if(coverstate == FoupCoverStateEnum.CLOSE)
                {
                    (SelectedProcedureStateMaps as FoupProcedureStateMaps).First.Value.ProcedureState = new FoupProcedureDone();
                    if(SelectedProcedureStateMapNode.Value.Procedure == (SelectedProcedureStateMaps as FoupProcedureStateMaps).First.Value.Procedure)
                    {
                        SelectedProcedureStateMapNode = SelectedProcedureStateMapNode.Next;
                    }
                }
                else
                {
                    (SelectedProcedureStateMaps as FoupProcedureStateMaps).First.Value.ProcedureState = new FoupProcedureIdle();
                    SelectedProcedureStateMapNode = (SelectedProcedureStateMaps as FoupProcedureStateMaps).First;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum InitSelectedProcedureStateMapNode(FoupStateEnum setfoupstate)
        {
            EventCodeEnum retval = EventCodeEnum.FOUP_SEQUENCE_NULL;

            try
            {
                if (setfoupstate == FoupStateEnum.LOAD)
                {
                    foreach (var loadProcedureStateMap in LoadProcedureStateMaps)
                    {
                        loadProcedureStateMap.ProcedureState = new FoupProcedureIdle();
                    }

                    SelectedProcedureStateMaps = LoadProcedureStateMaps;
                }
                else if (setfoupstate == FoupStateEnum.UNLOAD)
                {

                    foreach (var unloadProcedureStateMap in UnloadProcedureStateMaps)
                    {
                        unloadProcedureStateMap.ProcedureState = new FoupProcedureIdle();
                    }

                    SelectedProcedureStateMaps = UnloadProcedureStateMaps;
                }

                SelectedProcedureStateMapNode = (SelectedProcedureStateMaps as FoupProcedureStateMaps).First;

                if (SelectedProcedureStateMapNode != null)
                {
                    retval = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retval;
        }

        public EventCodeEnum SetPrevSelectedProcedureStateMapNode()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (SelectedProcedureStateMapNode.Previous != null)
                {
                    SelectedProcedureStateMapNode = SelectedProcedureStateMapNode.Previous;
                    SelectedProcedureStateMapNode.Value.ProcedureState = new FoupProcedureIdle(); //idle state로 변경
                    LoggerManager.RecoveryLog($"Pre Foup Sequence : {SelectedProcedureStateMapNode.Value.Procedure.Caption}, Clear Sequence state");
                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    retval = EventCodeEnum.FOUP_SEQUENCE_NULL;
                    LoggerManager.RecoveryLog($"Reason : {retval}", true);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }
        public int SetSequenceTab(IFoupProcedure curprocedure)
        {
            int tabindex = -1;

            try
            {
                
                foreach (var obj in LoadProcedureStateMaps) //load tab enabled, unload tab disabled
                {
                    if (obj.Procedure.Equals(curprocedure))
                    {
                        return 0;
                    }
                }

                foreach (var obj in UnloadProcedureStateMaps) //unload tab enabled, load tab disabled
                {
                    if (obj.Procedure.Equals(curprocedure))
                    {
                        return 1;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return tabindex;
        }

        public EventCodeEnum SequencesRefresh()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            //bool errorflag = false;
            try
            {
                SelectedProcedureStateMapNode = (SelectedProcedureStateMaps as FoupProcedureStateMaps).First;

                foreach (var procedure in SelectedProcedureStateMaps as FoupProcedureStateMaps)
                {
                    //if (procedure.ProcedureStateEnum == FoupProcedureStateEnum.DONE)
                    //{
                    procedure.ProcedureState = new FoupProcedureIdle();

                    EventCodeEnum ioValid = procedure.Procedure.Behavior.CheckIOState();

                    if (ioValid == EventCodeEnum.NONE)
                    {
                        procedure.ProcedureState = new FoupProcedureDone();
                        LoggerManager.RecoveryLog($"Foup Procedure : {SelectedProcedureStateMapNode.Value.Procedure.Caption}, IO Validation : DONE");
                        SelectedProcedureStateMapNode = SelectedProcedureStateMapNode.Next;
                    }
                    else if (ioValid == EventCodeEnum.UNDEFINED)
                    {
                        //error
                        LoggerManager.RecoveryLog($"Foup Procedure : {SelectedProcedureStateMapNode.Value.Procedure.Caption}, IO Validation : IDLE");
                        procedure.ProcedureState = new FoupProcedureIdle();
                        break;
                    }
                    else if (ioValid == EventCodeEnum.IO_PORT_ERROR || ioValid == EventCodeEnum.IO_NOT_MATCHED)
                    {
                        LoggerManager.RecoveryLog($"Foup Procedure : {SelectedProcedureStateMapNode.Value.Procedure.Caption}, IO Validation : ERROR", true);
                        procedure.ProcedureState = new FoupProcedureBehaviorError();
                        break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }
    }
}
