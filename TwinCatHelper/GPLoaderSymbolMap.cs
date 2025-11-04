using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ProberInterfaces;
using ProberErrorCode;
using LogModule;

namespace TwinCatHelper
{
    [Serializable]
    public class GPLoaderSymbolMap : INotifyPropertyChanged, ISystemParameterizable, IParamNode, IParam
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private List<ADSSymbolBase> symbols;
        [JsonIgnore]
        public bool IsParamChanged { get; set; }
        public EventCodeEnum Init()
        {
            try
            {
                symbols = new List<ADSSymbolBase>();
                symbols.Clear();

                symbols.Add(InputSymbol.CDXIn);
                symbols.Add(OutputSymbol.CDXOut);
                symbols.Add(OutputSymbol.CardBufferParams);
                symbols.Add(OutputSymbol.CCAccParams);
                symbols.Add(OutputSymbol.ChuckAccParams);
                symbols.Add(OutputSymbol.CSTAccParams);
                symbols.Add(OutputSymbol.FixTrayParams);
                symbols.Add(OutputSymbol.PAAccParams);
                symbols.Add(OutputSymbol.Front_Signal_Buzzer);
                symbols.Add(OutputSymbol.Front_Signal_Green);
                symbols.Add(OutputSymbol.Front_Signal_Red);
                symbols.Add(OutputSymbol.Front_Signal_Yellow);

                symbols.Add(OutputSymbol.GPLParam);
                symbols.Add(OutputSymbol.Visu_Active);
                symbols.Add(OutputSymbol.LeftSaftyDoor_Lock);
                symbols.Add(OutputSymbol.Rear_Signal_Buzzer);
                symbols.Add(OutputSymbol.FreezeRobot);
                symbols.Add(OutputSymbol.Rear_Signal_Green);
                symbols.Add(OutputSymbol.Rear_Signal_Red);
                symbols.Add(OutputSymbol.Rear_Signal_Yellow);
                symbols.Add(OutputSymbol.RightSaftyDoor_Lock);
                symbols.Add(OutputSymbol.CardTrayVac);
                symbols.Add(OutputSymbol.CardTrayUnlock);
                symbols.Add(OutputSymbol.IsCardHolderOn);
                symbols.Add(InputSymbol.CSTCtrlStatus);
                symbols.Add(OutputSymbol.CSTCtrlCommand);
                symbols.Add(OutputSymbol.CSTJobCommand);
                symbols.Add(OutputSymbol.DRWAccParams);

                symbols.Add(OutputSymbol.CardIDAccParam);

                symbols.Add(OutputSymbol.CoolantInletCommand);
                symbols.Add(OutputSymbol.CoolantOutletCommand);
                symbols.Add(OutputSymbol.CoolantPurgeCommand);
                symbols.Add(OutputSymbol.CoolantDrainCommand);
                symbols.Add(OutputSymbol.DryAirCommand);

                symbols.Add(OutputSymbol.BufferedMove);
                symbols.Add(OutputSymbol.BufferedXPos);
                symbols.Add(OutputSymbol.BufferedZPos);
                symbols.Add(OutputSymbol.BufferedWPos);

                symbols.Add(OutputSymbol.OutputStates);
                symbols.Add(OutputSymbol.FOUPParamEx);
                symbols.Add(OutputSymbol.Foup_JogCmd);
                symbols.Add(OutputSymbol.Foup_JogPos);

                symbols.Add(OutputSymbol.PCWLeakStatus);

                symbols.Add(InputSymbol.CoolantLeakStatus);
                symbols.Add(InputSymbol.CoolantPressStatus);

                symbols.Add(InputSymbol.InputStates);


                symbols.Add(InputSymbol.WaitHandle);
                symbols.Add(InputSymbol.LockedCST);

                symbols.Add(InputSymbol.WaferInfos);
                symbols.Add(InputSymbol.CSTState);
                symbols.Add(InputSymbol.CSTWaferCount);
                symbols.Add(InputSymbol.CSTWaferState);

                symbols.Add(InputSymbol.FOUPDriveErrs);
                symbols.Add(InputSymbol.Foup_Poss);
                symbols.Add(InputSymbol.Foup_Velos);
                symbols.Add(InputSymbol.LastEvent);
                symbols.Add(InputSymbol.FoupLastEvent);

                symbols.Add(InputSymbol.EventLog_RingBuffer);
                symbols.Add(InputSymbol.EventLog_RingBuffer_Foup);
                symbols.Add(InputSymbol.EventLog_Index);
                symbols.Add(InputSymbol.EventLog_Index_Foup);

                symbols.Add(OutputSymbol.CoverOpen);
                symbols.Add(OutputSymbol.CoverClose);

                symbols.Add(OutputSymbol.LCC_Vac);
                symbols.Add(OutputSymbol.LCC_Vac_Break);
                symbols.Add(OutputSymbol.CoverLock);
                symbols.Add(OutputSymbol.CoverUnlock);
                symbols.Add(OutputSymbol.CST_Mapping);
                symbols.Add(OutputSymbol.CST_Load);
                symbols.Add(OutputSymbol.CST_Unload);
                symbols.Add(OutputSymbol.CST_Lock_12inch);
                symbols.Add(OutputSymbol.CST_Unlock_12inch);
                symbols.Add(OutputSymbol.CST_Lock_8inch);
                symbols.Add(OutputSymbol.CST_Unlock_8inch);
                symbols.Add(OutputSymbol.CST_Vacuum);

                symbols.Add(OutputSymbol.CST_IND_ALARM);
                symbols.Add(OutputSymbol.CST_IND_RESERVED);
                symbols.Add(OutputSymbol.CST_IND_AUTO);
                symbols.Add(OutputSymbol.CST_IND_BUSY);
                symbols.Add(OutputSymbol.CST_IND_LOAD);
                symbols.Add(OutputSymbol.CST_IND_UNLOAD);
                symbols.Add(OutputSymbol.CST_IND_PLACEMENT);
                symbols.Add(OutputSymbol.CST_IND_PRESENCE);

                symbols.Add(OutputSymbol.Buffer_Vac);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }

        public List<ADSSymbolBase> GetSymbols()
        {
            return symbols;
        }

        public EventCodeEnum SetEmulParam()
        {
            InputSymbol = new InputSymbols();
            OutputSymbol = new OutputSymbols();

            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetDefaultParam()
        {
            InputSymbol = new InputSymbols();
            OutputSymbol = new OutputSymbols();

            return EventCodeEnum.NONE;
        }

        public void SetElementMetaData()
        {
        }
        #endregion

        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [JsonIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }
        public List<object> Nodes { get; set; }

        public string FilePath { get; } = "";
        public string FileName { get; } = "GPLoaderSymbolMap.json";


        private InputSymbols _InputSymbol;

        public InputSymbols InputSymbol
        {
            get { return _InputSymbol; }
            set { _InputSymbol = value; }
        }

        private OutputSymbols _OutputSymbol;

        public OutputSymbols OutputSymbol
        {
            get { return _OutputSymbol; }
            set { _OutputSymbol = value; }
        }
    }


    public class InputSymbols : ISymbolGroup
    {
        public static int MaxFoupCnt = 3;

        private ADSStructSymbol _CDXIn = new ADSStructSymbol("GV_Interface.gst_Int_PLCToPC",
                                        EnumVariableType.STRUCTURE,
                                        "CDX Input data.", 236);
        public ADSStructSymbol CDXIn
        {
            get { return _CDXIn; }
            set { _CDXIn = value; }
        }

        private ADSStructSymbol _WaferInfos = new ADSStructSymbol("GV_Interface.gst_CSTWaferInfos",
                                        EnumVariableType.STRUCTURE,
                                        "Wafer Information Data.", 2*100*4);    // 2 Byte * MaxAvaWaferCount * FoupCount
        public ADSStructSymbol WaferInfos
        {
            get { return _WaferInfos; }
            set { _WaferInfos = value; }
        }


        private ADSSymbol _CSTState = new ADSSymbol("GV_Interface.gn_CSTState",
            EnumDataType.UINT, EnumVariableType.ARR, "Cassette State", SystemModuleCount.ModuleCnt.FoupCount);
        public ADSSymbol CSTState
        {
            get { return _CSTState; }
            set { _CSTState = value; }
        }


        private ADSSymbol _CSTWaferCount = new ADSSymbol("GV_Interface.gn_CSTWafer_Cnt",
            EnumDataType.UDINT, EnumVariableType.ARR, "Wafer Count in Cassette", SystemModuleCount.ModuleCnt.FoupCount);
        public ADSSymbol CSTWaferCount
        {
            get { return _CSTWaferCount; }
            set { _CSTWaferCount = value; }
        }

        private ADSSymbol _CSTWaferState = new ADSSymbol("GV_Interface.gn_CSTWafer_State",
            EnumDataType.UDINT, EnumVariableType.ARR, "Wafer State in Cassette", SystemModuleCount.ModuleCnt.FoupCount);
        public ADSSymbol CSTWaferState
        {
            get { return _CSTWaferState; }
            set { _CSTWaferState = value; }
        }

        private ADSSymbol _CSTCtrlStatus = new ADSSymbol("GV_Interface.gb_Int_CSTCtrlStatus",
            EnumDataType.INT, EnumVariableType.ARR, "Cassette control status", SystemModuleCount.ModuleCnt.FoupCount);
        public ADSSymbol CSTCtrlStatus
        {
            get { return _CSTCtrlStatus; }
            set { _CSTCtrlStatus = value; }
        }

        private ADSSymbol _CoolantPressStatus = new ADSSymbol("GV_Interface.gb_Int_CoolantPressStatus",
            EnumDataType.INT, EnumVariableType.ARR, "Coolant Pressure Status", GPLoaderDef.StageCount);
        public ADSSymbol CoolantPressStatus
        {
            get { return _CoolantPressStatus; }
            set { _CoolantPressStatus = value; }
        }
        private ADSSymbol _CoolantLeakStatus = new ADSSymbol("GV_Interface.gb_Bool_CoolantLeakStatus",
            EnumDataType.BOOL, EnumVariableType.ARR, "Coolant Pressure Status", GPLoaderDef.ColdUtilBoxCount);
        public ADSSymbol CoolantLeakStatus
        {
            get { return _CoolantLeakStatus; }
            set { _CoolantLeakStatus = value; }
        }

        private ADSSymbol _WaitHandle = new ADSSymbol("GV_Interface.gInt_WaitHandle",
                EnumDataType.INT, EnumVariableType.VAR, "Loader Action Wait Handle");
        public ADSSymbol WaitHandle
        {
            get { return _WaitHandle; }
            set { _WaitHandle = value; }
        }
        private ADSSymbol _InputStates = new ADSSymbol("GV_Interface.gUDI_InputStates",
            EnumDataType.UDINT, EnumVariableType.ARR, "Input States", GPLoaderDef.MaxInputModuleCount);
        public ADSSymbol InputStates
        {
            get { return _InputStates; }
            set { _InputStates = value; }
        }
        private ADSSymbol _LockedCST = new ADSSymbol("GV_Interface.gInt_LockedCST",
           EnumDataType.INT, EnumVariableType.ARR, "Locked CST Type. 1: 12Inch, 2: 6Inch, 3: 8Inch", SystemModuleCount.ModuleCnt.FoupCount);
        public ADSSymbol LockedCST
        {
            get { return _LockedCST; }
            set { _LockedCST = value; }
        }

        //gFoup_DriveErrCode		: ARRAY[gcn_MinCnt..gcn_MaxFoupCnt] OF UINT;
        private ADSSymbol _FOUPDriveErrs = new ADSSymbol("GV_Interface.gFoup_DriveErrCode",
                EnumDataType.UINT, EnumVariableType.ARR, "FOUPDriveErrs", SystemModuleCount.ModuleCnt.FoupCount);
        public ADSSymbol FOUPDriveErrs
        {
            get { return _FOUPDriveErrs; }
            set { _FOUPDriveErrs = value; }
        }
        //gnFoup3_Pos				: ARRAY[gcn_MinCnt..gcn_MaxFoupCnt] OF DINT;
        private ADSSymbol _Foup_Poss = new ADSSymbol("GV_Interface.gnFoup_Pos",
                        EnumDataType.DINT, EnumVariableType.ARR, "FOUPDriveErrs", SystemModuleCount.ModuleCnt.FoupCount);
        public ADSSymbol Foup_Poss
        {
            get { return _Foup_Poss; }
            set { _Foup_Poss = value; }
        }
        //gnFoup_Velo			: ARRAY[gcn_MinCnt..gcn_MaxFoupCnt] OF DINT;
        private ADSSymbol _Foup_Velos = new ADSSymbol("GV_Interface.gnFoup_Velo",
                        EnumDataType.DINT, EnumVariableType.ARR, "FOUPDriveErrs", SystemModuleCount.ModuleCnt.FoupCount);
        public ADSSymbol Foup_Velos
        {
            get { return _Foup_Velos; }
            set { _Foup_Velos = value; }
        }

        public ADSSymbol LastEvent { get; set; } = new ADSSymbol("GV_Interface.gInt_LastEvent",
           EnumDataType.DINT, EnumVariableType.VAR, "Recent Event Code");

        public ADSSymbol FoupLastEvent { get; set; } = new ADSSymbol("GV_Interface.gInt_FoupLastEvent",
           EnumDataType.DINT, EnumVariableType.ARR, "Recent Event Code", SystemModuleCount.ModuleCnt.FoupCount);

        //EventLog_RingBuffer
        public ADSStructSymbol _EventLog_RingBuffer { get; set; } = new ADSStructSymbol("GV_Interface.gEI_EventLogs",
        EnumVariableType.ARR, "Event Log RingBuffer", 1000);
        public ADSStructSymbol EventLog_RingBuffer
        {
            get { return _EventLog_RingBuffer; }
            set { _EventLog_RingBuffer = value; }
        }

        //EventLog_RingBuffer_Foup
        public ADSStructSymbol _EventLog_RingBuffer_Foup { get; set; } = new ADSStructSymbol("GV_Interface.gEI_FoupEventLogs",
        EnumVariableType.ARR, "Foup Event Log RingBuffer", SystemModuleCount.ModuleCnt.FoupCount);
        public ADSStructSymbol EventLog_RingBuffer_Foup
        {
            get { return _EventLog_RingBuffer_Foup; }
            set { _EventLog_RingBuffer_Foup = value; }
        }

        //EventLog_Index
        private ADSSymbol _EventLog_Index = new ADSSymbol("GV_Interface.gInt_EventIndex",
                        EnumDataType.INT, EnumVariableType.VAR, "Event Log Index");
        public ADSSymbol EventLog_Index
        {
            get { return _EventLog_Index; }
            set { _EventLog_Index = value; }
        }

        //EventLog_Index_Foup
        private ADSSymbol _EventLog_Index_Foup = new ADSSymbol("GV_Interface.gInt_FoupEventIndex",
                        EnumDataType.INT, EnumVariableType.ARR, "Event Log Index (Foup)");
        public ADSSymbol EventLog_Index_Foup
        {
            get { return _EventLog_Index_Foup; }
            set { _EventLog_Index_Foup = value; }
        }
    }
    public class OutputSymbols : ISymbolGroup
    {
        private ADSStructSymbol _CDXOut = new ADSStructSymbol("GV_Interface.gst_Int_PCToPLC",
                                        EnumVariableType.STRUCTURE,
                                        "CDX Output data.", 81);
        public ADSStructSymbol CDXOut
        {
            get { return _CDXOut; }
            set { _CDXOut = value; }
        }
        private ADSStructSymbol _GPLParam = new ADSStructSymbol("GV_Interface.gpst_Int_Params",
                                EnumVariableType.STRUCTURE,
                                "CDX Output data.", 53 * 4 + 2);
        public ADSStructSymbol GPLParam
        {
            get { return _GPLParam; }
            set { _GPLParam = value; }
        }


        //private ADSStructSymbol _GPL_RobotParam = new ADSStructSymbol("GV_Pro_SubSystem.gst_Pro_RobotParams",
        //                       EnumVariableType.STRUCTURE,
        //                       "RobotParams.", 53 * 4 + 2);
        //public ADSStructSymbol GPL_RobotParam
        //{
        //    get { return _GPL_RobotParam; }
        //    set { _GPL_RobotParam = value; }
        //}

        private ADSStructSymbol _FixTrayParams = new ADSStructSymbol("GV_Interface.gpst_Int_FixTray_Params", 
            EnumVariableType.STRUCTUREDARRAY, "Fixed Tray access parameters", 12);
        public ADSStructSymbol FixTrayParams
        {
            get { return _FixTrayParams; }
            set { _FixTrayParams = value; }
        }
        private ADSStructSymbol _CardBufferParams = new ADSStructSymbol("GV_Interface.gpst_Int_CardBuffer_Params",
            EnumVariableType.STRUCTUREDARRAY, "Fixed Tray access parameters", 4);
        public ADSStructSymbol CardBufferParams
        {
            get { return _CardBufferParams; }
            set { _CardBufferParams = value; }
        }

        private ADSStructSymbol _CardTrayParams = new ADSStructSymbol("GV_Interface.gpst_Int_CardBuffer_Params",
          EnumVariableType.STRUCTUREDARRAY, "Card Tray access parameters", 9);
        public ADSStructSymbol CardTrayParams
        {
            get { return _CardTrayParams; }
            set { _CardTrayParams = value; }
        }

        private ADSStructSymbol _CSTAccParams = new ADSStructSymbol("GV_Interface.gpst_CSTAcc_Params",
            EnumVariableType.STRUCTUREDARRAY, "Cassette Tray access parameters", 3);
        public ADSStructSymbol CSTAccParams
        {
            get { return _CSTAccParams; }
            set { _CSTAccParams = value; }
        }
        private ADSStructSymbol _DRWAccParams = new ADSStructSymbol("GV_Interface.gpst_CSTDWR_Params",
            EnumVariableType.STRUCTUREDARRAY, "Drawer access parameters", 3);
        public ADSStructSymbol DRWAccParams
        {
            get { return _DRWAccParams; }
            set { _DRWAccParams = value; }
        }
        private ADSStructSymbol _PAAccParams = new ADSStructSymbol("GV_Interface.gpst_PAAcc_Params",
            EnumVariableType.STRUCTUREDARRAY, "Fixed Tray access parameters", 3);
        public ADSStructSymbol PAAccParams
        {
            get { return _PAAccParams; }
            set { _PAAccParams = value; }
        }
        private ADSStructSymbol _ChuckAccParams = new ADSStructSymbol("GV_Interface.gpst_CHK_Acc_Params",
            EnumVariableType.STRUCTUREDARRAY, "Fixed Tray access parameters", 12);
        public ADSStructSymbol ChuckAccParams
        {
            get { return _ChuckAccParams; }
            set { _ChuckAccParams = value; }
        }

        private ADSStructSymbol _CCAccParams = new ADSStructSymbol("GV_Interface.gpst_CC_Acc_Params",
            EnumVariableType.STRUCTUREDARRAY, "CC access parameters", 12);
        public ADSStructSymbol CCAccParams
        {
            get { return _CCAccParams; }
            set { _CCAccParams = value; }
        }

        private ADSStructSymbol _CardIDAccParam = new ADSStructSymbol("GV_Interface.gpst_CARD_ID_Acc_Param",
          EnumVariableType.STRUCTURE, "CC ID access parameters",7);
        public ADSStructSymbol CardIDAccParam
        {
            get { return _CardIDAccParam; }
            set { _CardIDAccParam = value; }
        }

        private ADSSymbol _Visu_Active = new ADSSymbol("GV_Visulization.gb_Visu_Active",
            EnumDataType.BOOL, EnumVariableType.VAR, "Visu_Active");
        public ADSSymbol Visu_Active
        {
            get { return _Visu_Active; }
            set { _Visu_Active = value; }
        }


        private ADSSymbol _LeftSaftyDoor_Lock = new ADSSymbol("GV_Interface.gb_Int_LeftSaftyDoor_Lock",
            EnumDataType.BOOL,EnumVariableType.VAR, "LeftSaftyDoor Lock");
        public ADSSymbol LeftSaftyDoor_Lock
        {
            get { return _LeftSaftyDoor_Lock; }
            set { _LeftSaftyDoor_Lock = value; }
        }

        private ADSSymbol _RightSaftyDoor_Lock = new ADSSymbol("GV_Interface.gb_Int_RightSaftyDoor_Lock",
            EnumDataType.BOOL, EnumVariableType.VAR, "RightSaftyDoor Lock");
        public ADSSymbol RightSaftyDoor_Lock
        {
            get { return _RightSaftyDoor_Lock; }
            set { _RightSaftyDoor_Lock = value; }
        }

        private ADSSymbol _Front_Signal_Red = new ADSSymbol("GV_Interface.gb_Int_Front_Signal_Red", 
            EnumDataType.BOOL, EnumVariableType.VAR, "Front Signal Red");
        public ADSSymbol Front_Signal_Red
        {
            get { return _Front_Signal_Red; }
            set { _Front_Signal_Red = value; }
        }

        private ADSSymbol _Front_Signal_Yellow = new ADSSymbol("GV_Interface.gb_Int_Front_Signal_Yellow", 
            EnumDataType.BOOL, EnumVariableType.VAR, "Front Signal Yellow");
        public ADSSymbol Front_Signal_Yellow
        {
            get { return _Front_Signal_Yellow; }
            set { _Front_Signal_Yellow = value; }
        }

        private ADSSymbol _Front_Signal_Green = new ADSSymbol("GV_Interface.gb_Int_Front_Signal_Green",
           EnumDataType.BOOL, EnumVariableType.VAR, "Front Signal Green");
        public ADSSymbol Front_Signal_Green
        {
            get { return _Front_Signal_Green; }
            set { _Front_Signal_Green = value; }
        }

        private ADSSymbol _Front_Signal_Buzzer = new ADSSymbol("GV_Interface.gb_Int_Front_Signal_Buzzer",
            EnumDataType.BOOL, EnumVariableType.VAR, "Front Signal Buzzer");
        public ADSSymbol Front_Signal_Buzzer
        {
            get { return _Front_Signal_Buzzer; }
            set { _Front_Signal_Buzzer = value; }
        }

        private ADSSymbol _Rear_Signal_Red = new ADSSymbol("GV_Interface.gb_Int_Rear_Signal_Red",
           EnumDataType.BOOL, EnumVariableType.VAR, "Rear Signal Red");
        public ADSSymbol Rear_Signal_Red
        {
            get { return _Rear_Signal_Red; }
            set { _Rear_Signal_Red = value; }
        }

        private ADSSymbol _Rear_Signal_Yellow = new ADSSymbol("GV_Interface.gb_Int_Rear_Signal_Yellow",
            EnumDataType.BOOL, EnumVariableType.VAR, "Rear Signal Yellow");
        public ADSSymbol Rear_Signal_Yellow
        {
            get { return _Rear_Signal_Yellow; }
            set { _Rear_Signal_Yellow = value; }
        }
        private ADSSymbol _Rear_Signal_Green = new ADSSymbol("GV_Interface.gb_Int_Rear_Signal_Green",
           EnumDataType.BOOL, EnumVariableType.VAR, "Rear Signal Green");
        public ADSSymbol Rear_Signal_Green
        {
            get { return _Rear_Signal_Green; }
            set { _Rear_Signal_Green = value; }
        }

        private ADSSymbol _Rear_Signal_Buzzer = new ADSSymbol("GV_Interface.gb_Int_Rear_Signal_Buzzer",
            EnumDataType.BOOL, EnumVariableType.VAR, "Rear Signal Buzzer");
        public ADSSymbol Rear_Signal_Buzzer
        {
            get { return _Rear_Signal_Buzzer; }
            set { _Rear_Signal_Buzzer = value; }
        }

        //gb_FreezeRobot							: BOOL;
        private ADSSymbol _FreezeRobot = new ADSSymbol("GV_Interface.gb_FreezeRobot",
            EnumDataType.BOOL, EnumVariableType.VAR, "Freeze robot command");
        public ADSSymbol FreezeRobot
        {
            get { return _FreezeRobot; }
            set { _FreezeRobot = value; }
        }

        private ADSSymbol _CSTCtrlCommand = new ADSSymbol("GV_Interface.gb_Int_CSTCtrlCommand",
            EnumDataType.INT, EnumVariableType.ARR, "Cassette control commands", SystemModuleCount.ModuleCnt.FoupCount);
        public ADSSymbol CSTCtrlCommand
        {
            get { return _CSTCtrlCommand; }
            set { _CSTCtrlCommand = value; }
        }

        private ADSSymbol _CSTJobCommand = new ADSSymbol("GV_Interface.gInt_CSTJobCommand",
            EnumDataType.INT, EnumVariableType.ARR, "Cassette Job commands", SystemModuleCount.ModuleCnt.FoupCount);
        public ADSSymbol CSTJobCommand
        {
            get { return _CSTJobCommand; }
            set { _CSTJobCommand = value; }
        }
        private ADSSymbol _PCWLeakStatus = new ADSSymbol("GV_Interface.gb_Bool_PCWLeakStatus",
            EnumDataType.BOOL, EnumVariableType.ARR, "Cassette Job commands", SystemModuleCount.ModuleCnt.StageCount);
        public ADSSymbol PCWLeakStatus
        {
            get { return _PCWLeakStatus; }
            set { _PCWLeakStatus = value; }
        }

        private ADSSymbol _CardTrayVac = new ADSSymbol("GV_Interface.gb_CardTrayVac",
            EnumDataType.BOOL, EnumVariableType.VAR, "Card Tray Vac.");
        public ADSSymbol CardTrayVac
        {
            get { return _CardTrayVac; }
            set { _CardTrayVac = value; }
        }
        private ADSSymbol _IsCardHolderOn = new ADSSymbol("GV_Interface.gInt_IsCardHolderOn",
         EnumDataType.INT, EnumVariableType.VAR, "Is CardHolder On");
        public ADSSymbol IsCardHolderOn
        {
            get { return _IsCardHolderOn; }
            set { _IsCardHolderOn = value; }
        }
        private ADSSymbol _CardTrayUnlock = new ADSSymbol("GV_Post_HW.DO_CARD_DRW_UNLOCK",
            EnumDataType.BOOL, EnumVariableType.VAR, "Card Tray Unlock.");
        public ADSSymbol CardTrayUnlock
        {
            get { return _CardTrayUnlock; }
            set { _CardTrayUnlock = value; }
        }
        #region // Cold Utility Box Control Commands
        private ADSSymbol _CoolantInletCommand = new ADSSymbol("GV_Interface.gb_Bool_CoolantInletCommand",
            EnumDataType.BOOL, EnumVariableType.ARR, "Coolant Inlet Valve Control Commands", GPLoaderDef.StageCount);
        public ADSSymbol CoolantInletCommand
        {
            get { return _CoolantInletCommand; }
            set { _CoolantInletCommand = value; }
        }
        private ADSSymbol _CoolantOutletCommand = new ADSSymbol("GV_Interface.gb_Bool_CoolantOutletCommand",
            EnumDataType.BOOL, EnumVariableType.ARR, "Coolant Outlet Valve Control Commands", GPLoaderDef.StageCount);
        public ADSSymbol CoolantOutletCommand
        {
            get { return _CoolantOutletCommand; }
            set { _CoolantOutletCommand = value; }
        }
        private ADSSymbol _CoolantPurgeCommand = new ADSSymbol("GV_Interface.gb_Bool_CoolantPurgeCommand",
            EnumDataType.BOOL, EnumVariableType.ARR, "Coolant Purge Valve Control Commands", GPLoaderDef.StageCount);
        public ADSSymbol CoolantPurgeCommand
        {
            get { return _CoolantPurgeCommand; }
            set { _CoolantPurgeCommand = value; }
        }
        private ADSSymbol _CoolantDrainCommand = new ADSSymbol("GV_Interface.gb_Bool_CoolantDrainCommand",
            EnumDataType.BOOL, EnumVariableType.ARR, "Coolant Drain Valve Control Commands", GPLoaderDef.StageCount);
        public ADSSymbol CoolantDrainCommand
        {
            get { return _CoolantDrainCommand; }
            set { _CoolantDrainCommand = value; }
        }
        private ADSSymbol _DryAirCommand = new ADSSymbol("GV_Interface.gb_Bool_DryAirCommand",
            EnumDataType.BOOL, EnumVariableType.ARR, "Coolant Inlet Valve Control Commands", GPLoaderDef.StageCount);
        public ADSSymbol DryAirCommand
        {
            get { return _DryAirCommand; }
            set { _DryAirCommand = value; }
        }


        private ADSSymbol _OutputStates = new ADSSymbol("GV_Interface.gUDI_OutputStates",
           EnumDataType.UDINT, EnumVariableType.ARR, "Output States", GPLoaderDef.MaxOutputModuleCount);
        public ADSSymbol OutputStates
        {
            get { return _OutputStates; }
            set { _OutputStates = value; }
        }


        private ADSSymbol _Foup_JogCmd = new ADSSymbol("GV_Interface.gnFoup_JogCmd",
           EnumDataType.UINT, EnumVariableType.ARR, "Foup_JogCmd", SystemModuleCount.ModuleCnt.PACount);
        public ADSSymbol Foup_JogCmd
        {
            get { return _Foup_JogCmd; }
            set { _Foup_JogCmd = value; }
        }

        private ADSSymbol _Foup_JogPos = new ADSSymbol("GV_Interface.gnFoup_JogPos",
           EnumDataType.DINT, EnumVariableType.ARR, "Foup_JogPos", SystemModuleCount.ModuleCnt.PACount);
        public ADSSymbol Foup_JogPos
        {
            get { return _Foup_JogPos; }
            set { _Foup_JogPos = value; }
        }

        private ADSStructSymbol _FOUPParamEx = new ADSStructSymbol("GV_Interface.gpst_ParamsEx",
            EnumVariableType.STRUCTUREDARRAY, "Extended Parameters", 5 * 2 * SystemModuleCount.ModuleCnt.FoupCount);      // Five(5) of DINT(2), for four FOUPS(4) 
        public ADSStructSymbol FOUPParamEx
        {
            get { return _FOUPParamEx; }
            set { _FOUPParamEx = value; }
        }

        private ADSSymbol _BufferedMove = new ADSSymbol("GV_Interface.gbRequestBufferedMove",
                    EnumDataType.BOOL, EnumVariableType.VAR, "Buffered move request");
        public ADSSymbol BufferedMove
        {
            get { return _BufferedMove; }
            set { _BufferedMove = value; }
        }

        private ADSSymbol _BufferedXPos = new ADSSymbol("GV_Interface.gLr_BufferedXPos",
                    EnumDataType.DINT, EnumVariableType.VAR, "Buffered move taget X");
        public ADSSymbol BufferedXPos
        {
            get { return _BufferedXPos; }
            set { _BufferedXPos = value; }
        }
        private ADSSymbol _BufferedZPos = new ADSSymbol("GV_Interface.gLr_BufferedZPos",
                    EnumDataType.DINT, EnumVariableType.VAR, "Buffered move target Z");
        public ADSSymbol BufferedZPos
        {
            get { return _BufferedZPos; }
            set { _BufferedZPos = value; }
        }
        private ADSSymbol _BufferedWPos = new ADSSymbol("GV_Interface.gLr_BufferedWPos",
                            EnumDataType.DINT, EnumVariableType.VAR, "Buffered move target W");
        public ADSSymbol BufferedWPos
        {
            get { return _BufferedWPos; }
            set { _BufferedWPos = value; }
        }

        private ADSSymbol _LCC_Vac = new ADSSymbol("GV_Post_HW.DO_LCC_Vac",
          EnumDataType.BOOL, EnumVariableType.VAR, "LCC Vac");
        public ADSSymbol LCC_Vac
        {
            get { return _LCC_Vac; }
            set { _LCC_Vac = value; }
        }
        private ADSSymbol _LCC_Vac_Break = new ADSSymbol("GV_Post_HW.DO_LCC_Vac_Break",
          EnumDataType.BOOL, EnumVariableType.VAR, "LCC Vac Break");
        public ADSSymbol LCC_Vac_Break
        {
            get { return _LCC_Vac_Break; }
            set { _LCC_Vac_Break = value; }
        }
        private ADSSymbol _CoverOpen = new ADSSymbol("GV_Post_HW.DO_CoverOpen",
           EnumDataType.BOOL, EnumVariableType.ARR, "CST Cover Open", SystemModuleCount.ModuleCnt.FoupCount);
        public ADSSymbol CoverOpen
        {
            get { return _CoverOpen; }
            set { _CoverOpen = value; }
        }

        private ADSSymbol _CoverClose = new ADSSymbol("GV_Post_HW.DO_CoverClose",
        EnumDataType.BOOL, EnumVariableType.ARR, "CST Cover Close", SystemModuleCount.ModuleCnt.FoupCount);
        public ADSSymbol CoverClose
        {
            get { return _CoverClose; }
            set { _CoverClose = value; }
        }
        private ADSSymbol _CoverLock = new ADSSymbol("GV_Post_HW.DO_CoverLock",
         EnumDataType.BOOL, EnumVariableType.ARR, "CST Cover Lock", SystemModuleCount.ModuleCnt.FoupCount);
        public ADSSymbol CoverLock
        {
            get { return _CoverLock; }
            set { _CoverLock = value; }
        }

        private ADSSymbol _CoverUnlock = new ADSSymbol("GV_Post_HW.DO_CoverUnlock",
        EnumDataType.BOOL, EnumVariableType.ARR, "CST Cover Unlock", SystemModuleCount.ModuleCnt.FoupCount);
        public ADSSymbol CoverUnlock
        {
            get { return _CoverUnlock; }
            set { _CoverUnlock = value; }
        }

        private ADSSymbol _CST_Mapping = new ADSSymbol("GV_Post_HW.DO_CST_Mapping",
      EnumDataType.BOOL, EnumVariableType.ARR, "CST Mapping", SystemModuleCount.ModuleCnt.FoupCount);
        public ADSSymbol CST_Mapping
        {
            get { return _CST_Mapping; }
            set { _CST_Mapping = value; }
        }

        private ADSSymbol _CST_Load = new ADSSymbol("GV_Post_HW.DO_CST_Load",
    EnumDataType.BOOL, EnumVariableType.ARR, "CST Load", SystemModuleCount.ModuleCnt.FoupCount);
        public ADSSymbol CST_Load
        {
            get { return _CST_Load; }
            set { _CST_Load = value; }
        }

        private ADSSymbol _CST_Unload = new ADSSymbol("GV_Post_HW.DO_CST_Unload",
   EnumDataType.BOOL, EnumVariableType.ARR, "CST Unload", SystemModuleCount.ModuleCnt.FoupCount);
        public ADSSymbol CST_Unload
        {
            get { return _CST_Unload; }
            set { _CST_Unload = value; }
        }

        private ADSSymbol _CST_Lock_12inch = new ADSSymbol("GV_Post_HW.DO_CST_Lock_12inch",
 EnumDataType.BOOL, EnumVariableType.ARR, "CST Lock 12inch", SystemModuleCount.ModuleCnt.FoupCount);
        public ADSSymbol CST_Lock_12inch
        {
            get { return _CST_Lock_12inch; }
            set { _CST_Lock_12inch = value; }
        }
        private ADSSymbol _CST_Unlock_12inch = new ADSSymbol("GV_Post_HW.DO_CST_Unlock_12inch",
EnumDataType.BOOL, EnumVariableType.ARR, "CST Unlock 12inch", SystemModuleCount.ModuleCnt.FoupCount);
        public ADSSymbol CST_Unlock_12inch
        {
            get { return _CST_Unlock_12inch; }
            set { _CST_Unlock_12inch = value; }
        }
        private ADSSymbol _CST_Lock_8inch = new ADSSymbol("GV_Post_HW.DO_CST_Lock_8inch",
EnumDataType.BOOL, EnumVariableType.ARR, "CST Lock 8inch", SystemModuleCount.ModuleCnt.FoupCount);
        public ADSSymbol CST_Lock_8inch
        {
            get { return _CST_Lock_8inch; }
            set { _CST_Lock_8inch = value; }
        }
        private ADSSymbol _CST_Unlock_8inch = new ADSSymbol("GV_Post_HW.DO_CST_Unlock_8inch",
EnumDataType.BOOL, EnumVariableType.ARR, "CST Unlock 8inch", SystemModuleCount.ModuleCnt.FoupCount);
        public ADSSymbol CST_Unlock_8inch
        {
            get { return _CST_Unlock_8inch; }
            set { _CST_Unlock_8inch = value; }
        }
        private ADSSymbol _CST_Vacuum = new ADSSymbol("GV_Post_HW.DO_CST_Vacuum",
   EnumDataType.BOOL, EnumVariableType.ARR, "CST Vacuum", SystemModuleCount.ModuleCnt.FoupCount);
        public ADSSymbol CST_Vacuum
        {
            get { return _CST_Vacuum; }
            set { _CST_Vacuum = value; }
        }


        private ADSSymbol _CST_IND_ALARM = new ADSSymbol("GV_Post_HW.DO_CST_IND_ALARM",
  EnumDataType.BOOL, EnumVariableType.ARR, "CST_IND_ALARM", SystemModuleCount.ModuleCnt.FoupCount);
        public ADSSymbol CST_IND_ALARM
        {
            get { return _CST_IND_ALARM; }
            set { _CST_IND_ALARM = value; }
        }
        private ADSSymbol _CST_IND_BUSY = new ADSSymbol("GV_Post_HW.DO_CST_IND_BUSY",
  EnumDataType.BOOL, EnumVariableType.ARR, "CST_IND_BUSY", SystemModuleCount.ModuleCnt.FoupCount);
        public ADSSymbol CST_IND_BUSY
        {
            get { return _CST_IND_BUSY; }
            set { _CST_IND_BUSY = value; }
        }
        private ADSSymbol _CST_IND_RESERVED = new ADSSymbol("GV_Post_HW.DO_CST_IND_RESERVED",
 EnumDataType.BOOL, EnumVariableType.ARR, "CST_IND_RESERVED", SystemModuleCount.ModuleCnt.FoupCount);
        public ADSSymbol CST_IND_RESERVED
        {
            get { return _CST_IND_RESERVED; }
            set { _CST_IND_RESERVED = value; }
        }

        private ADSSymbol _CST_IND_AUTO = new ADSSymbol("GV_Post_HW.DO_CST_IND_AUTO",
  EnumDataType.BOOL, EnumVariableType.ARR, "CST_IND_AUTO", SystemModuleCount.ModuleCnt.FoupCount);
        public ADSSymbol CST_IND_AUTO
        {
            get { return _CST_IND_AUTO; }
            set { _CST_IND_AUTO = value; }
        }

        private ADSSymbol _CST_IND_LOAD = new ADSSymbol("GV_Post_HW.DO_CST_IND_LOAD",
 EnumDataType.BOOL, EnumVariableType.ARR, "CST_IND_LOAD", SystemModuleCount.ModuleCnt.FoupCount);
        public ADSSymbol CST_IND_LOAD
        {
            get { return _CST_IND_LOAD; }
            set { _CST_IND_LOAD = value; }
        }

        private ADSSymbol _CST_IND_UNLOAD = new ADSSymbol("GV_Post_HW.DO_CST_IND_UNLOAD",
 EnumDataType.BOOL, EnumVariableType.ARR, "CST_IND_UNLOAD", SystemModuleCount.ModuleCnt.FoupCount);
        public ADSSymbol CST_IND_UNLOAD
        {
            get { return _CST_IND_UNLOAD; }
            set { _CST_IND_UNLOAD = value; }
        }

        private ADSSymbol _CST_IND_PLACEMENT = new ADSSymbol("GV_Post_HW.DO_CST_IND_PLACEMENT",
 EnumDataType.BOOL, EnumVariableType.ARR, "CST_IND_PLACEMENT", SystemModuleCount.ModuleCnt.FoupCount);
        public ADSSymbol CST_IND_PLACEMENT
        {
            get { return _CST_IND_PLACEMENT; }
            set { _CST_IND_PLACEMENT = value; }
        }

        private ADSSymbol _CST_IND_PRESENCE = new ADSSymbol("GV_Post_HW.DO_CST_IND_PRESENCE",
EnumDataType.BOOL, EnumVariableType.ARR, "CST_IND_PRESENCE", SystemModuleCount.ModuleCnt.FoupCount);
        public ADSSymbol CST_IND_PRESENCE
        {
            get { return _CST_IND_PRESENCE; }
            set { _CST_IND_PRESENCE = value; }
        }



        #endregion
        private ADSSymbol _Buffer_Vac = new ADSSymbol("GV_Post_HW.DO_Buffer_Vac",
            EnumDataType.BOOL, EnumVariableType.ARR, "Wafer Buffer Vac", SystemModuleCount.ModuleCnt.BufferCount);
        public ADSSymbol Buffer_Vac
        {
            get { return _Buffer_Vac; }
            set { _Buffer_Vac = value; }
        }
    }
}
