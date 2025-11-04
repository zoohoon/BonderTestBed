using LogModule;
using ProberInterfaces.Enum;
using System;
using System.Reflection;

namespace TwinCatHelper
{
    #region SymbolMap
    public interface ISymbolGroup
    {

    }
    [Serializable]
    public class SymbolMap
    {


        private StatusSymbolGroup _MachineStatusSymbols;

        public StatusSymbolGroup MachineStatusSymbols
        {
            get { return _MachineStatusSymbols; }
            set { _MachineStatusSymbols = value; }
        }

        private AxisSymbolGroup _AxisSymbols;
        public AxisSymbolGroup AxisSymbols
        {
            get { return _AxisSymbols; }
            set { _AxisSymbols = value; }
        }

        private MotionParamSymbolGroup _MotionParamSymbols;

        public MotionParamSymbolGroup MotionParamSymbols
        {
            get { return _MotionParamSymbols; }
            set { _MotionParamSymbols = value; }
        }

        private MoveCommandSymbolGroup _MoveCommandSymbols;

        public MoveCommandSymbolGroup MoveCommandSymbols
        {
            get { return _MoveCommandSymbols; }
            set { _MoveCommandSymbols = value; }
        }
        private CommandPositionSymbolGroup _CommandPosSymbols;
        public CommandPositionSymbolGroup CommandPosSymbols
        {
            get { return _CommandPosSymbols; }
            set { _CommandPosSymbols = value; }
        }
        private ControlsSymbolGroups _ControlSymbols;

        public ControlsSymbolGroups ControlSymbols
        {
            get { return _ControlSymbols; }
            set { _ControlSymbols = value; }
        }

        public void initSymbols()
        {
            try
            {
                MachineStatusSymbols = new StatusSymbolGroup();
                AxisSymbols = new AxisSymbolGroup();
                MotionParamSymbols = new MotionParamSymbolGroup();
                MoveCommandSymbols = new MoveCommandSymbolGroup();
                CommandPosSymbols = new CommandPositionSymbolGroup();
                ControlSymbols = new ControlsSymbolGroups();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void InitVariableValue()
        {
            object symbolGroup;
            ADSSymbol symbol;

            foreach (PropertyInfo mapprop in this.GetType().GetProperties())
            {
                try
                {
                    symbolGroup = mapprop.GetValue(this, null);

                    foreach (PropertyInfo symprop in symbolGroup.GetType().GetProperties())
                    {
                        try
                        {
                            object propObj;

                            propObj = symprop.GetValue(symbolGroup, null);

                            if (propObj is ADSSymbol)
                            {
                                symbol = new ADSSymbol();
                                symbol = (ADSSymbol)propObj;

                                switch (symbol.DataType)
                                {
                                    case EnumDataType.Type_Undefined:
                                        break;
                                    case EnumDataType.BOOL:
                                        if (symbol.VariableType == EnumVariableType.ARR)
                                        {
                                            symbol.SymbolValue = (object)(new bool[symbol.DataNumber]);
                                        }
                                        else
                                        {
                                            symbol.SymbolValue = (object)(new bool());
                                        }
                                        break;
                                    case EnumDataType.BYTE:
                                        if (symbol.VariableType == EnumVariableType.ARR)
                                        {
                                            symbol.SymbolValue = (object)(new byte[symbol.DataNumber]);
                                        }
                                        else
                                        {
                                            symbol.SymbolValue = (object)(new byte());
                                        }
                                        break;
                                    case EnumDataType.WORD:
                                        if (symbol.VariableType == EnumVariableType.ARR)
                                        {
                                            symbol.SymbolValue = (object)(new char[symbol.DataNumber]);
                                        }
                                        else
                                        {
                                            symbol.SymbolValue = (object)(new char());
                                        }
                                        break;
                                    case EnumDataType.DWORD:
                                        if (symbol.VariableType == EnumVariableType.ARR)
                                        {
                                            symbol.SymbolValue = (object)(new UInt32[symbol.DataNumber]);
                                        }
                                        else
                                        {
                                            symbol.SymbolValue = (object)(new UInt32());
                                        }
                                        break;
                                    case EnumDataType.SINT:
                                        if (symbol.VariableType == EnumVariableType.ARR)
                                        {
                                            symbol.SymbolValue = (object)(new sbyte[symbol.DataNumber]);
                                        }
                                        else
                                        {
                                            symbol.SymbolValue = (object)(new sbyte());
                                        }
                                        break;
                                    case EnumDataType.INT:
                                        if (symbol.VariableType == EnumVariableType.ARR)
                                        {
                                            symbol.SymbolValue = (object)(new Int16[symbol.DataNumber]);
                                        }
                                        else
                                        {
                                            symbol.SymbolValue = (object)(new Int16());
                                        }
                                        break;
                                    case EnumDataType.DINT:
                                        if (symbol.VariableType == EnumVariableType.ARR)
                                        {
                                            symbol.SymbolValue = (object)(new Int32[symbol.DataNumber]);
                                        }
                                        else
                                        {
                                            symbol.SymbolValue = (object)(new Int32());
                                        }
                                        break;
                                    case EnumDataType.USINT:
                                        if (symbol.VariableType == EnumVariableType.ARR)
                                        {
                                            symbol.SymbolValue = (object)(new byte[symbol.DataNumber]);
                                        }
                                        else
                                        {
                                            symbol.SymbolValue = (object)(new byte());
                                        }
                                        break;
                                    case EnumDataType.UINT:
                                        if (symbol.VariableType == EnumVariableType.ARR)
                                        {
                                            symbol.SymbolValue = (object)(new UInt16[symbol.DataNumber]);
                                        }
                                        else
                                        {
                                            symbol.SymbolValue = (object)(new UInt16());
                                        }
                                        break;
                                    case EnumDataType.UDINT:
                                        if (symbol.VariableType == EnumVariableType.ARR)
                                        {
                                            symbol.SymbolValue = (object)(new UInt32[symbol.DataNumber]);
                                        }
                                        else
                                        {
                                            symbol.SymbolValue = (object)(new UInt32());
                                        }
                                        break;
                                    case EnumDataType.REAL:
                                        if (symbol.VariableType == EnumVariableType.ARR)
                                        {
                                            symbol.SymbolValue = (object)(new Single[symbol.DataNumber]);
                                        }
                                        else
                                        {
                                            symbol.SymbolValue = (object)(new Single());
                                        }
                                        break;
                                    case EnumDataType.LREAL:
                                        if (symbol.VariableType == EnumVariableType.ARR)
                                        {
                                            symbol.SymbolValue = (object)(new double[symbol.DataNumber]);
                                        }
                                        else
                                        {
                                            symbol.SymbolValue = (object)(new double());
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        catch (Exception err)
                        {
                            //LoggerManager.Error($string.Format("SymbolMap.InitVariableValue(): Err = {0}", err.Message));
                            LoggerManager.Exception(err);

                        }

                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }
            }
        }

    }
    public class StatusSymbolGroup : ISymbolGroup
    {
        private ADSSymbol mMachineStatus = new ADSSymbol("GVL.gStMachineStatus",
                                        EnumDataType.Type_Undefined,
                                        EnumVariableType.STRUCTURE,
                                        "CDX(Cyclic Data eXchange) symbol : Machine status");
        public ADSSymbol MachineStatus
        {
            get { return mMachineStatus; }
            set { mMachineStatus = value; }
        }
    }

    public class AxisSymbolGroup : ISymbolGroup
    {

        private ADSSymbol _VirtualAxisObj = new ADSSymbol("GVL.gVirtualAxisObj",
            EnumDataType.Type_Undefined,
            EnumVariableType.STRUCTUREDARRAY,
            "VirtualAxisObj",
            (int)EnumThreePodAxis.THREEPOD_AXIS_LAST);
        public ADSSymbol VirtualAxisObj
        {
            get { return _VirtualAxisObj; }
            set { _VirtualAxisObj = value; }
        }

        //private ADSSymbol _SettlingTime = new ADSSymbol("GVL.gLrSettlingTime",
        //    EnumDataType.LREAL,
        //    EnumVariableType.ARR,
        //    "SettlingTime",
        //    (int)EnumThreePodAxis.THREEPOD_AXIS_LAST);
        //public ADSSymbol SettlingTime
        //{
        //    get { return _SettlingTime; }
        //    set { _SettlingTime = value; }
        //}
    }
    public class ControlsSymbolGroups : ISymbolGroup
    {
        private ADSSymbol _EnableAmps = new ADSSymbol("GVL.gbEnableAmps",
                                        EnumDataType.BOOL,
                                        EnumVariableType.ARR,
                                        "Axis enable",
                                        (int)EnumThreePodAxis.THREEPOD_AXIS_LAST);
        public ADSSymbol EnableAmps
        {
            get { return _EnableAmps; }
            set { _EnableAmps = value; }
        }

        private ADSSymbol _CMDOriginPosSet = new ADSSymbol("GVL.gboriginPos",
                                          EnumDataType.BOOL,
                                          EnumVariableType.ARR,
                                          "Cmd Origin Pos",
                                          (int)EnumThreePodAxis.THREEPOD_AXIS_LAST);
        public ADSSymbol CMDOriginPosSet
        {
            get { return _CMDOriginPosSet; }
            set { _CMDOriginPosSet = value; }
        }

        private ADSSymbol _AmpFaultClearCommand = new ADSSymbol("GVL.gbAxisStatusClear",
                                          EnumDataType.BOOL,
                                          EnumVariableType.ARR,
                                          "Cmd Amp Fault",
                                          (int)EnumThreePodAxis.THREEPOD_AXIS_LAST);
        public ADSSymbol AmpFaultClearCommand
        {
            get { return _AmpFaultClearCommand; }
            set { _AmpFaultClearCommand = value; }
        }

        private ADSSymbol _HomingCommand = new ADSSymbol("GVL.gbHoming",
                                          EnumDataType.BOOL,
                                          EnumVariableType.ARR,
                                          "CMD HOMING",
                                          (int)EnumThreePodAxis.THREEPOD_AXIS_LAST);
        public ADSSymbol HomingCommand
        {
            get { return _HomingCommand; }
            set { _HomingCommand = value; }
        }

        private ADSSymbol _HomingCommand2 = new ADSSymbol("GVL.gbHoming2",
                                          EnumDataType.BOOL,
                                          EnumVariableType.ARR,
                                          "CMD HOMING2",
                                          (int)EnumThreePodAxis.THREEPOD_AXIS_LAST);
        public ADSSymbol HomingCommand2
        {
            get { return _HomingCommand2; }
            set { _HomingCommand2 = value; }
        }

        private ADSSymbol _CmdSetZeroForce = new ADSSymbol("GVL.gbCMDSetzeroForce",
                                          EnumDataType.BOOL,
                                          EnumVariableType.VAR,
                                          "CMD SetZeroForce");
        public ADSSymbol CmdSetZeroForce
        {
            get { return _CmdSetZeroForce; }
            set { _CmdSetZeroForce = value; }
        }

        private ADSSymbol _CmdDualLoopEnable = new ADSSymbol("GVL.gbCMDDualLoopEnble",
                                          EnumDataType.BOOL,
                                          EnumVariableType.VAR,
                                          "CMD DualLoop");
        public ADSSymbol CmdDualLoopEnable
        {
            get { return _CmdDualLoopEnable; }
            set { _CmdDualLoopEnable = value; }
        }

        private ADSSymbol _ChuckBalanceOffset = new ADSSymbol("GVL.glrChuckBalOffsetval",
                                          EnumDataType.LREAL,
                                          EnumVariableType.ARR,
                                          "Chuck Balance OffsetValue",
                                          (int)EnumThreePodAxis.THREEPOD_AXIS_LAST);
        public ADSSymbol ChuckBalanceOffset
        {
            get { return _ChuckBalanceOffset; }
            set { _ChuckBalanceOffset = value; }

        }
    }
    public class MotionParamSymbolGroup : ISymbolGroup
    {
        //gTCAxisTrjParam              ARRAY
        private ADSSymbol _TrjParam = new ADSSymbol("GVL.gAxisTrjParam",
            EnumDataType.Type_Undefined,
            EnumVariableType.STRUCTUREDARRAY,
            "trajectory parameters",
            (int)EnumThreePodAxis.THREEPOD_AXIS_LAST);
        public ADSSymbol TrjParam
        {
            get { return _TrjParam; }
            set { _TrjParam = value; }
        }
    }
    public class MoveCommandSymbolGroup : ISymbolGroup
    {
        ////gbZAxisAbsMove:					BOOL;
        //private ADSSymbol _ZAxisAbsMove = new ADSSymbol("GVL.gbZAxisAbsMove",
        //                                EnumDataType.BOOL,
        //                                EnumVariableType.VAR,
        //                                "ZAxisMOVE");
        //public ADSSymbol ZAxisAbsMove
        //{
        //    get { return _ZAxisAbsMove; }
        //    set { _ZAxisAbsMove = value; }

        //}

        ////gbRAxisAbsMove:					BOOL;
        //private ADSSymbol _RAxisAbsMove = new ADSSymbol("GVL.gbRAxisAbsMove",
        //                                EnumDataType.BOOL,
        //                                EnumVariableType.VAR,
        //                                "RAxisMOVE");
        //public ADSSymbol RAxisAbsMove
        //{
        //    get { return _RAxisAbsMove; }
        //    set { _RAxisAbsMove = value; }

        //}

        ////gbRAxisAbsMove:					BOOL;
        //private ADSSymbol _TTAxisAbsMove = new ADSSymbol("GVL.gbTTAxisAbsMove",
        //                                EnumDataType.BOOL,
        //                                EnumVariableType.VAR,
        //                                "TTAxisMOVE");
        //public ADSSymbol TTAxisAbsMove
        //{
        //    get { return _TTAxisAbsMove; }
        //    set { _TTAxisAbsMove = value; }
        //}

        ////gbZAxisRelMove:					BOOL;
        //private ADSSymbol _ZAxisRelMove = new ADSSymbol("GVL.gbZAxisRelMove",
        //                                EnumDataType.BOOL,
        //                                EnumVariableType.VAR,
        //                                "ZAxisMOVE");
        //public ADSSymbol ZAxisRelMove
        //{
        //    get { return _ZAxisRelMove; }
        //    set { _ZAxisRelMove = value; }

        //}

        ////gbRAxisRelMove:					BOOL;
        //private ADSSymbol _RAxisRelMove = new ADSSymbol("GVL.gbRAxisRelMove",
        //                                EnumDataType.BOOL,
        //                                EnumVariableType.VAR,
        //                                "RAxisMOVE");
        //public ADSSymbol RAxisRelMove
        //{
        //    get { return _RAxisRelMove; }
        //    set { _RAxisRelMove = value; }

        //}

        ////gbRAxisRelMove:					BOOL;
        //private ADSSymbol _TTAxisRelMove = new ADSSymbol("GVL.gbTTAxisRelMove",
        //                                EnumDataType.BOOL,
        //                                EnumVariableType.VAR,
        //                                "TTAxisMOVE");
        //public ADSSymbol TTAxisRelMove
        //{
        //    get { return _TTAxisRelMove; }
        //    set { _TTAxisRelMove = value; }
        //}


        //gbAbsMoveRun:					BOOL;
        private ADSSymbol _AbsMoveRun = new ADSSymbol("GVL.gbAbsMoveRun",
                                        EnumDataType.BOOL,
                                        EnumVariableType.ARR,
                                        "AbsMoveRun",
                                        (int)EnumThreePodAxis.THREEPOD_AXIS_LAST);
        public ADSSymbol AbsMoveRun
        {
            get { return _AbsMoveRun; }
            set { _AbsMoveRun = value; }
        }

        //gbRelMoveRun:					BOOL;
        private ADSSymbol _RelMoveRun = new ADSSymbol("GVL.gbRelMoveRun",
                                        EnumDataType.BOOL,
                                        EnumVariableType.ARR,
                                        "RelMoveRun",
                                        (int)EnumThreePodAxis.THREEPOD_AXIS_LAST);
        public ADSSymbol RelMoveRun
        {
            get { return _RelMoveRun; }
            set { _RelMoveRun = value; }
        }

        //gbVMoveRun:					BOOL;
        private ADSSymbol _VMoveRun = new ADSSymbol("GVL.gbVMoveRun",
                                        EnumDataType.BOOL,
                                        EnumVariableType.ARR,
                                        "VMoveRun",
                                        (int)EnumThreePodAxis.THREEPOD_AXIS_LAST);
        public ADSSymbol VMoveRun
        {
            get { return _VMoveRun; }
            set { _VMoveRun = value; }
        }

        //gbMoveStop:					BOOL;
        private ADSSymbol _MoveStop = new ADSSymbol("GVL.gbMoveStop",
                                        EnumDataType.BOOL,
                                        EnumVariableType.ARR,
                                        "MoveStop",
                                        (int)EnumThreePodAxis.THREEPOD_AXIS_LAST);
        public ADSSymbol MoveStop
        {
            get { return _MoveStop; }
            set { _MoveStop = value; }
        }


    }
    public class CommandPositionSymbolGroup : ISymbolGroup
    {
        ////gLrPosition:					LREAR;
        //private ADSSymbol _ZPosition = new ADSSymbol("GVL.gLrZPosition",
        //                                EnumDataType.LREAL,
        //                                EnumVariableType.VAR,
        //                                "ZPosition");
        //public ADSSymbol ZPosition
        //{
        //    get { return _ZPosition; }
        //    set { _ZPosition = value; }
        //}

        ////gLrRPosition:					LREAR;
        //private ADSSymbol _RPosition = new ADSSymbol("GVL.gLrRPosition",
        //                                EnumDataType.LREAL,
        //                                EnumVariableType.VAR,
        //                                "RPosition");
        //public ADSSymbol RPosition
        //{
        //    get { return _RPosition; }
        //    set { _RPosition = value; }
        //}

        ////gLrTTPosition:					LREAR;
        //private ADSSymbol _TTPosition = new ADSSymbol("GVL.gLrTTPosition",
        //                                EnumDataType.LREAL,
        //                                EnumVariableType.VAR,
        //                                "TTPosition");
        //public ADSSymbol TTPosition
        //{
        //    get { return _TTPosition; }
        //    set { _TTPosition = value; }
        //}

        //GLrOriginPosition
        private ADSSymbol _OriginPos = new ADSSymbol("GVL.gLrOriginPos",
                                       EnumDataType.LREAL,
                                       EnumVariableType.VAR,
                                       "OriingPosition",
                                        (int)EnumThreePodAxis.THREEPOD_AXIS_LAST);
        public ADSSymbol OriingPos
        {
            get { return _OriginPos; }
            set { _OriginPos = value; }
        }

        //GLrSetPos
        private ADSSymbol _SetPosition = new ADSSymbol("GVL.gLrSetPos",
                                       EnumDataType.LREAL,
                                       EnumVariableType.VAR,
                                       "SetPosition",
                                        (int)EnumThreePodAxis.THREEPOD_AXIS_LAST);
        public ADSSymbol SetPosition
        {
            get { return _SetPosition; }
            set { _SetPosition = value; }
        }

        //gLrAbsPosition:					LREAR;
        private ADSSymbol _AbsPos = new ADSSymbol("GVL.gLrAbsPosition",
                                        EnumDataType.LREAL,
                                        EnumVariableType.VAR,
                                        "AbsPos",
                                        (int)EnumThreePodAxis.THREEPOD_AXIS_LAST);
        public ADSSymbol AbsPos
        {
            get { return _AbsPos; }
            set { _AbsPos = value; }
        }

        //gLrRelPosition:					LREAR;
        private ADSSymbol _RelPos = new ADSSymbol("GVL.gLrRelPosition",
                                        EnumDataType.LREAL,
                                        EnumVariableType.VAR,
                                        "RelPos",
                                        (int)EnumThreePodAxis.THREEPOD_AXIS_LAST);
        public ADSSymbol RelPos
        {
            get { return _RelPos; }
            set { _RelPos = value; }
        }

        //gLrVPosition:					LREAR;
        private ADSSymbol _VPos = new ADSSymbol("GVL.gLrVPosition",
                                        EnumDataType.LREAL,
                                        EnumVariableType.VAR,
                                        "VPos",
                                        (int)EnumThreePodAxis.THREEPOD_AXIS_LAST);
        public ADSSymbol VPos
        {
            get { return _VPos; }
            set { _VPos = value; }
        }

    }
    #endregion
}
