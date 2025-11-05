using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Command.Internal;
using ProberInterfaces.Event;
using ProberInterfaces.PinAlign.ProbeCardData;
using System;
using System.Collections.ObjectModel;
using System.Threading;

namespace RequestCore.ActionPack.Internal
{
    [Serializable]
    public class ZUp : Action
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            IProbingModule Probing = this.ProbingModule();

            try
            {
                bool cmdResult = false;

                if (Probing.ModuleState.State == ModuleStateEnum.RUNNING ||
                    Probing.ModuleState.State == ModuleStateEnum.SUSPENDED)
                {
                    cmdResult = this.CommandManager().SetCommand<IZUPRequest>(this);
                }
                else
                {
                    LoggerManager.Debug($"[ZUp], Run() : Probing ModuleState = {Probing.ModuleState.State}");
                }

                LoggerManager.Debug($"[Action command - {this.GetType().Name}] Command Result = {(cmdResult ? "Success" : "Fail")}.");

                if (cmdResult == true)
                {
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class LotPause : Action
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            ILotOPModule lotopmodule = this.LotOPModule();

            try
            {
                bool cmdResult = false;

                cmdResult = this.CommandManager().SetCommand<ILotOpPause>(this);

                LoggerManager.Debug($"[Action command - {this.GetType().Name}] Command Result = {(cmdResult ? "Success" : "Fail")}.");

                if (cmdResult == true)
                {
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class ZDown : Action
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            IProbingModule Probing = this.ProbingModule();

            try
            {
                bool cmdResult = false;

                if (Probing.ModuleState.State == ModuleStateEnum.IDLE ||
                    Probing.ModuleState.State == ModuleStateEnum.RUNNING ||
                    Probing.ModuleState.State == ModuleStateEnum.SUSPENDED)
                {
                    cmdResult = this.CommandManager().SetCommand<IZDownRequest>(this);
                }

                LoggerManager.Debug($"[Action command - {this.GetType().Name}] Command Result = {(cmdResult ? "Success" : "Fail")}.");

                if (cmdResult == true)
                {
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class UnloadWafer : Action
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                bool cmdResult = false;

                this.LotOPModule().UnloadFoupNumber = (this.GetParam_Wafer().GetSubsInfo().SlotIndex.Value - 1) / 25 + 1;//DY �ϰ�쿡�� ���
                cmdResult = this.CommandManager().SetCommand<IUnloadWafer>(this);
                LoggerManager.Debug($"[Action command - {this.GetType().Name}] Command Result = {(cmdResult ? "Success" : "Fail")}.");

                if (cmdResult == true)
                {
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class MoveToFirstDie : Action
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            IProbingModule Probing = this.ProbingModule();

            try
            {
                bool cmdRet = false;

                if (Probing.ModuleState.State == ModuleStateEnum.IDLE ||
                    Probing.ModuleState.State == ModuleStateEnum.RUNNING ||
                    Probing.ModuleState.State == ModuleStateEnum.SUSPENDED)
                {
                    cmdRet = this.CommandManager().SetCommand<IGoToStartDie>(this);
                }

                LoggerManager.Debug($"[Action command - {this.GetType().Name}] Command Result = {(cmdRet ? "Success" : "Fail")}.");

                if (cmdRet == false)
                {
                    retVal = EventCodeEnum.UNDEFINED;
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }
    [Serializable]
    public class MoveToNextPosition : Action
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            IProbingModule Probing = this.ProbingModule();

            try
            {
                bool cmdRet = false;

                if (Probing.ModuleState.State == ModuleStateEnum.IDLE ||
                    Probing.ModuleState.State == ModuleStateEnum.RUNNING ||
                    Probing.ModuleState.State == ModuleStateEnum.SUSPENDED)
                {
                    cmdRet = this.CommandManager().SetCommand<IMoveToNextDie>(this, new ProbingCommandParam() { ProbingStateWhenReciveMoveToNextDie = Probing.ProbingStateEnum });
                    LoggerManager.Debug($"[Action command - Command Parameter {this.ProbingModule().ProbingStateEnum}");
                }

                LoggerManager.Debug($"[Action command - {this.GetType().Name}] Command Result = {(cmdRet ? "Success" : "Fail")}.");

                if (cmdRet == false)
                {
                    retVal = EventCodeEnum.UNDEFINED;
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class UnloadAllWaferAction : Action
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            IProbingModule Probing = this.ProbingModule();

            try
            {
                bool cmdRet = false;

                if (Probing.ModuleState.State == ModuleStateEnum.IDLE ||
                    Probing.ModuleState.State == ModuleStateEnum.RUNNING ||
                    Probing.ModuleState.State == ModuleStateEnum.SUSPENDED)
                {
                    this.LotOPModule().LotInfo.ContinueLot = false;
                    this.CommandManager().SetCommand<IUnloadAllWafer>(this);
                }

                LoggerManager.Debug($"[Action command - {this.GetType().Name}] Command Result = {(cmdRet ? "Success" : "Fail")}.");

                if (cmdRet == false)
                {
                    retVal = EventCodeEnum.UNDEFINED;
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class MoveToDiePosition : Action
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            IProbingModule Probing = this.ProbingModule();

            try
            {
                bool cmdRet = false;
                bool IsTestDiePosition = false;
                PositionParam positionParam = null;

                if (this.Argument.ToString() != string.Empty)
                {
                    positionParam = GetPositionFromStringData();
                    IsTestDiePosition = CheckTestDiePosition(positionParam);

                    if (IsTestDiePosition == true)
                    {
                        if (Probing.ModuleState.State == ModuleStateEnum.IDLE ||
                            Probing.ModuleState.State == ModuleStateEnum.RUNNING ||
                            Probing.ModuleState.State == ModuleStateEnum.SUSPENDED)
                        {
                            cmdRet = this.CommandManager().SetCommand<IMoveToDiePosition>(this, positionParam);
                        }

                        LoggerManager.Debug($"[Action command - {this.GetType().Name}] Command Result = {(cmdRet ? "Success" : "Fail")}.");

                        if (cmdRet == false)
                        {
                            retVal = EventCodeEnum.UNDEFINED;
                        }
                        else
                        {
                            retVal = EventCodeEnum.NONE;
                        }
                    }
                    else
                    {
                        retVal = EventCodeEnum.UNDEFINED;
                    }
                }
                else
                {
                    if (Probing.ModuleState.State == ModuleStateEnum.IDLE ||
                        Probing.ModuleState.State == ModuleStateEnum.RUNNING ||
                        Probing.ModuleState.State == ModuleStateEnum.SUSPENDED)
                    {
                        cmdRet = this.CommandManager().SetCommand<IMoveToDiePosition>(this, new ProbingCommandParam() { ProbingStateWhenReciveMoveToNextDie = Probing.ProbingStateEnum });
                    }

                    LoggerManager.Debug($"[Action command - {this.GetType().Name}] Command Result = {(cmdRet ? "Success" : "Fail")}.");

                    if (cmdRet == false)
                    {
                        retVal = EventCodeEnum.UNDEFINED;
                    }
                    else
                    {
                        retVal = EventCodeEnum.NONE;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public PositionParam GetPositionFromStringData()
        {
            PositionParam positionParam = new NoHavePositionParam();
            ICoordinateManager CoordinateManager = this.CoordinateManager();

            try
            {
                long x = 0;
                long y = 0;
                bool parseResult = false;
                string[] splitPositionData = this.Argument.ToString().Split(new char[] { 'Y', 'X' });

                if (splitPositionData.Length == 3)
                {
                    parseResult = long.TryParse(splitPositionData[1], out y);
                    parseResult = long.TryParse(splitPositionData[2], out x) || parseResult;

                    if (parseResult == true)
                    {
                        MachineIndex mi = null;
                        positionParam = new PositionParam();
                        mi = CoordinateManager.UserIndexConvertToMachineIndex(new UserIndex(x, y));
                        positionParam.X = mi.XIndex;
                        positionParam.Y = mi.YIndex;
                        LoggerManager.Debug($"[Action command - {this.GetType().Name}] Data which Get from StringData is X({positionParam.X}), Y({positionParam.Y}).");
                    }
                    else
                    {
                        LoggerManager.Debug($"[Action command - {this.GetType().Name}] Fail to Parse {splitPositionData[1]}, {splitPositionData[2]}.");
                    }
                }
                else
                {
                    LoggerManager.Debug($"[Action command - {this.GetType().Name}] Fail which get Position from StringData({this.Argument?.ToString()}).");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return positionParam;
        }

        public bool CheckTestDiePosition(PositionParam positionParam)
        {
            bool retVal = false;

            try
            {

                if (!(positionParam is NoHavePositionParam))
                {
                    IStageSupervisor stageSupervisor = this.StageSupervisor();

                    int firDimensionSize = stageSupervisor.WaferObject.GetSubsInfo().DIEs.GetLength(0);
                    int secDimensionSize = stageSupervisor.WaferObject.GetSubsInfo().DIEs.GetLength(1);

                    ObservableCollection<IDut> dutList = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList;

                    long XLength = 0;
                    long YLength = 0;

                    for (int i = 0; i < dutList.Count; i++)
                    {
                        long tmpValue = 0;
                        tmpValue = positionParam.X + dutList[i].MacIndex.XIndex;
                        XLength = XLength < tmpValue ? tmpValue : XLength;

                        tmpValue = 0;
                        tmpValue = positionParam.Y + dutList[i].MacIndex.YIndex;
                        YLength = YLength < tmpValue ? tmpValue : YLength;
                    }

                    if (XLength < firDimensionSize && YLength < secDimensionSize)
                    {
                        //if(stageSupervisor.WaferObject.DIEs[positionParam.X, positionParam.Y].DieType.Value == DieTypeEnum.TEST_DIE)
                        //{
                        //}

                        retVal = true;
                    }
                    else
                    {
                        retVal = false;
                    }
                    LoggerManager.Debug($"[Action command - {this.GetType().Name}] Position Data got from StringData is {(retVal ? "" : "not ")}the Check Die Position.");
                }
                else
                {
                    retVal = false;
                    LoggerManager.Debug($"[Action command - {this.GetType().Name}] Position Data got from StringData is not the Check Die Position.");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }


    [Serializable]
    public class MoveToDiePositionbyZState : Action
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            var probing = this.ProbingModule();
            try
            {
                var movetodiePos = new MoveToDiePosition();
                movetodiePos.Argument = this.Argument;

                var positionParam = movetodiePos.GetPositionFromStringData();
                var isTestDiePosition = movetodiePos.CheckTestDiePosition(positionParam);

                if (isTestDiePosition)
                {
                    if (probing.ModuleState.State == ModuleStateEnum.IDLE ||
                            probing.ModuleState.State == ModuleStateEnum.RUNNING ||
                            probing.ModuleState.State == ModuleStateEnum.SUSPENDED)
                    {

                        bool cmdRet = false;
                        if (probing.PreProbingStateEnum == EnumProbingState.ZUP ||
                            probing.PreProbingStateEnum == EnumProbingState.ZUPPERFORM ||
                            probing.PreProbingStateEnum == EnumProbingState.ZUPDWELL)
                        {
                            cmdRet = probing.CommandManager().SetCommand<IMoveToDiePositionAndZUp>(this, positionParam);
                            LoggerManager.Debug($"[Action command - {this.GetType().Name}] IMoveToDiePositionAndZUp Command Result = {(cmdRet ? "Success" : "Fail")}.");
                        }
                        else if (probing.PreProbingStateEnum == EnumProbingState.PINPADMATCHPERFORM ||
                            probing.PreProbingStateEnum == EnumProbingState.IDLE ||
                            probing.PreProbingStateEnum == EnumProbingState.DONE)
                        {
                            cmdRet = this.CommandManager().SetCommand<IMoveToDiePosition>(this, positionParam);
                            LoggerManager.Debug($"[Action command - {this.GetType().Name}] IMoveToDiePosition Command Result = {(cmdRet ? "Success" : "Fail")}.");
                        }

                        if (!cmdRet)
                        {
                            retVal = EventCodeEnum.UNDEFINED;
                        }
                        else
                        {
                            retVal = EventCodeEnum.NONE;
                        }
                    }
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]

    public class EventRaising : Action
    {
        public string EventName = string.Empty;

        public EventRaising()
        {

        }

        public EventRaising(string eventname)
        {
            EventName = eventname;
        }

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (string.IsNullOrEmpty(EventName) == false)
                {
                    retVal = this.EventManager().RaisingEvent(EventName);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class CheckTestCompleteIncluded : Action
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            IProbingModule Probing = this.ProbingModule();

            try
            {
                bool cmdRet = false;

                if (Probing.ModuleState.State == ModuleStateEnum.RUNNING ||
                      Probing.ModuleState.State == ModuleStateEnum.SUSPENDED)
                {
                    if (this.GPIB().GPIBSysParam_IParam.IsTestCompleteIncluded.Value)
                    {
                        this.CommandManager().SetCommand<IMoveToNextDie>(this, new ProbingCommandParam() { ProbingStateWhenReciveMoveToNextDie = Probing.ProbingStateEnum });
                        LoggerManager.Debug($"[Action command - Command Parameter {this.ProbingModule().ProbingStateEnum}");
                    }
                    else
                    {
                        this.EventManager().RaisingEvent(typeof(NotifyEventModule.GPIBTestCompleteNotIncludedEvent).FullName);
                    }
                }

                LoggerManager.Debug($"[Action command - {this.GetType().Name}] Command Result = {(cmdRet ? "Success" : "Fail")}.");

                if (cmdRet == false)
                {
                    retVal = EventCodeEnum.UNDEFINED;
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    // Tester �������� Error End ��� �ǹ�
    public class TestAbort : Action
    {
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                LoggerManager.Debug($"[Action command - {this.GetType().Name}] Lot OP Module State = {this.LotOPModule().ModuleState.State}.");
                string ErrorMessage = $"The cell paused by tester(CELL{this.LoaderController().GetChuckIndex()} ABORT TEST).";
                retVal = this.LoaderController().ReserveErrorEnd(ErrorMessage);
                LoggerManager.Debug($"[Action command - {this.GetType().Name}] Command Result = {(retVal== EventCodeEnum.NONE ? "Success" : "Fail")}.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

}
