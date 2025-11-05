using System;
using System.Collections.Generic;

namespace GEMModule.Parameter
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using GemActBehavior;
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using SecsGemServiceInterface;

    public class GemRemoteActionRecipeParam : INotifyPropertyChanged, IParam, ISystemParameterizable
    {
        #region <remarks> PropertyChanged                           </remarks>
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion


        #region IParam Property

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [JsonIgnore, ParamIgnore]
        public List<object> Nodes { get; set; }
        [JsonIgnore, ParamIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [JsonIgnore, ParamIgnore]
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
        [JsonIgnore, ParamIgnore]
        public string FilePath { get; } = "GEM";
        [JsonIgnore, ParamIgnore]
        public string FileName { get; } = "GemRemoteActionRecipe.Json";

        #endregion
        private Dictionary<string, ClassInfo> _ModuleDllInfoDictionary
             = new Dictionary<string, ClassInfo>();
        public Dictionary<string, ClassInfo> ModuleDllInfoDictionary
        {
            get { return _ModuleDllInfoDictionary; }
            set
            {
                if (value != _ModuleDllInfoDictionary)
                {
                    _ModuleDllInfoDictionary = value;
                    RaisePropertyChanged();
                }
            }
        }


        public GemRemoteActionRecipeParam()
        {

        }

        #region IParam Method

        public EventCodeEnum Init()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                SetDefaultParam();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (ModuleDllInfoDictionary == null)
                    ModuleDllInfoDictionary = new Dictionary<string, ClassInfo>();

                string baseDllName = "GemActBehavior.dll";

                #region MAIN
                //ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.ACTIVATE_PROCESS), new ClassInfo(baseDllName, nameof(AssignLot)));

                //ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.WAFERID_LIST), new ClassInfo(baseDllName, nameof(AssignWaferID)));

                //ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.DOWNLOAD_STAGE_RECIPE), new ClassInfo(baseDllName, nameof(DownloadDeviceToStages)));

                //ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.SET_PARAMETERS), new ClassInfo(baseDllName, nameof(SetValueOfParameters)));

                //ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.DOCK_FOUP), new ClassInfo(baseDllName, nameof(DockFoup)));

                //ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.SELECT_SLOTS), new ClassInfo(baseDllName, nameof(AssignSlots)));

                //ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.START_LOT), new ClassInfo(baseDllName, nameof(LotStart)));

                // External Mode 동작 중, ZUP 명령을 Loader로부터 받기 싫다면 빼야 된다.

                // Y사 시나리오를 돌리기 위해서는 ZUP을 사용해야 됨.
                //ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.ZUP), new ClassInfo(baseDllName, nameof(Zup)));

                // Y사 시나리오를 돌리기 위해서는 TESTEND을 사용해야 됨.
                //ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.TESTEND), new ClassInfo(baseDllName, nameof(TestEnd)));

                // M사 시나리오를 돌리기 위해서는 Z_UP을 사용해야 됨.
                //ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.Z_UP), new ClassInfo(baseDllName, nameof(Zup)));

                // M사 시나리오를 돌리기 위해서는 END_TEST을 사용해야 됨.
                //ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.END_TEST), new ClassInfo(baseDllName, nameof(TestEndNoAfterCommand)));

                ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.CANCEL_CARRIER), new ClassInfo(baseDllName, nameof(CancelCarrier)));

                ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.ERROR_END), new ClassInfo(baseDllName, nameof(CellAbort)));

                ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.START_STAGE), new ClassInfo(baseDllName, nameof(CellStart)));

                ModuleDllInfoDictionary.Add(nameof(EnumCarrierAction.PROCEEDWITHCARRIER), new ClassInfo(baseDllName, nameof(ProcessedWithCarrier)));
                ModuleDllInfoDictionary.Add(nameof(EnumCarrierAction.RELEASECARRIER), new ClassInfo(baseDllName, nameof(ReleaseCarrier)));
                ModuleDllInfoDictionary.Add(nameof(EnumCarrierAction.PROCESSEDWITHCELLSLOT), new ClassInfo(baseDllName, nameof(ProcessedWithCell)));
                ModuleDllInfoDictionary.Add(nameof(EnumCarrierAction.PROCEEDWITHSLOT), new ClassInfo(baseDllName, nameof(ProceedWithSlot)));
                ModuleDllInfoDictionary.Add(nameof(EnumCarrierAction.CHANGEACCESSMODE), new ClassInfo(baseDllName, nameof(CarrierChangeAccessMode)));
                ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.PSTART), new ClassInfo(baseDllName, nameof(PStart)));
                ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.WAFERUNLOAD), new ClassInfo(baseDllName, nameof(TestEndNoAfterCommand)));
                ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.TESTEND), new ClassInfo(baseDllName, nameof(ZDownCommand)));
                ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.CANCELCARRIER), new ClassInfo(baseDllName, nameof(CancelCarrier)));
                ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.CARRIER_SUSPEND), new ClassInfo(baseDllName, nameof(SuspendCarrier)));
                ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.ZUP), new ClassInfo(baseDllName, nameof(Zup)));
                ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.TC_START), new ClassInfo(baseDllName, nameof(TC_Start)));
                ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.TC_END), new ClassInfo(baseDllName, nameof(TC_End)));
                #endregion


                // MICRON(baseDllName);



                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private void MICRON(string baseDllName)
        {
            try
            {
                ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.ACTIVATE_PROCESS), new ClassInfo(baseDllName, nameof(AssignLot)));
                ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.DOWNLOAD_STAGE_RECIPE), new ClassInfo(baseDllName, nameof(DownloadDeviceToStages)));
                ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.WAFERID_LIST), new ClassInfo(baseDllName, nameof(AssignWaferID)));
                ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.SELECT_SLOTS), new ClassInfo(baseDllName, nameof(AssignSlots)));
                ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.SET_PARAMETERS), new ClassInfo(baseDllName, nameof(SetValueOfParameters)));//싱가포르X, 미국O
                ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.START_LOT), new ClassInfo(baseDllName, nameof(LotStart)));

                ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.Z_UP), new ClassInfo(baseDllName, nameof(Zup)));
                ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.END_TEST), new ClassInfo(baseDllName, nameof(TestEndNoAfterCommand)));
                ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.ERROR_END), new ClassInfo(baseDllName, nameof(CellAbort)));
                ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.START_STAGE), new ClassInfo(baseDllName, nameof(CellStart)));

                ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.DOCK_FOUP), new ClassInfo(baseDllName, nameof(DockFoup)));
                ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.UNDOCK_FOUP), new ClassInfo(baseDllName, nameof(ReleaseCarrier)));
                ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.CANCEL_CARRIER), new ClassInfo(baseDllName, nameof(CancelCarrier)));
                ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.CARRIER_SUSPEND), new ClassInfo(baseDllName, nameof(SuspendCarrier)));


                // < DY 추가되어야할것 >
                ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.CHANGE_LOADPORT_MODE), new ClassInfo(baseDllName, nameof(ChangeFoupModeState)));
                ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.SELECT_SLOTS_STAGE), new ClassInfo(baseDllName, nameof(AssignSlotsStages)));
                ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.END_TEST_LP), new ClassInfo(baseDllName, nameof(TestEndNoAfterCommand)));
                ModuleDllInfoDictionary.Add(nameof(EnumRemoteCommand.ERROR_END_LP), new ClassInfo(baseDllName, nameof(CellAbort)));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        
        public void SetElementMetaData()
        {

        }
        #endregion

    }
}
