using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;

namespace SoakingParameters
{
    using ProberErrorCode;
    using ProberInterfaces;
    using LogModule;
    using ProberInterfaces.Soaking;


    /// <summary>
    /// Status Soaking Parameter
    /// </summary>
    [Serializable]

    public class StatusSoakingDeviceFile : IParam, INotifyPropertyChanged, IDeviceParameterizable, IStatusSoakingParam
    {
        #region ParameterDefaultValue

        private readonly string Maintain_NotExistWafer_Ratio_Title = "Maintain Soaking - Not Exist Wafer";
        private readonly string Maintain_UsePolicyWafer_Ratio_Title = "Maintain Soaking - Use Polish Wafer";
        private readonly string Maintain_UseStandardWafer_Ratio_Title = "Maintain Soaking - Use Standard Wafer";
        private readonly string Recovery_NotExistWafer_Ratio_Title = "Recovery Soaking - Not Exist Wafer";

        /// hhh to-do: ChillingTime Ratio 초기값 논의하여 셋팅필요
        private readonly float Maintain_NotExistWafer_Ratio = 0f;
        private readonly float Maintain_UsePolicyWafer_Ratio = -1f;
        private readonly float Maintain_UseStandardWafer_Ratio = -1f;
        private readonly float Recovery_NotExistWafer_Ratio = 0f;

        #endregion

        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public string FilePath { get; } = "";
        public string FileName { get; } = "StatusSoakingStateFile.Json";

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }
        [XmlIgnore, JsonIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
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

        public EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetDefaultParam()
        {
            StatusSoakingConfigParameter.PrepareStateConfig.SoakingStepTable.Add(new Soakingsteptable(1, int.MaxValue, StatusSoakingConfigParameter.AdvancedSetting.ODLimit.Value));
            StatusSoakingConfigParameter.RecoveryStateConfig.SoakingStepTable.Add(new Soakingsteptable(1, int.MaxValue, StatusSoakingConfigParameter.AdvancedSetting.ODLimit.Value));
            StatusSoakingConfigParameter.MaintainStateConfig.SoakingStepTable.Add(new Soakingsteptable(1, int.MaxValue, StatusSoakingConfigParameter.AdvancedSetting.ODLimit.Value));

            StatusSoakingConfigParameter.AdvancedSetting.ChillingRatioPolicy.Add(new Chillingratiopolicy((int)SoakingRatioPolicy_Index.Maintain_NotExistWafer, Maintain_NotExistWafer_Ratio_Title, Maintain_NotExistWafer_Ratio));
            StatusSoakingConfigParameter.AdvancedSetting.ChillingRatioPolicy.Add(new Chillingratiopolicy((int)SoakingRatioPolicy_Index.Maintain_UsePolicyWafer, Maintain_UsePolicyWafer_Ratio_Title, Maintain_UsePolicyWafer_Ratio));
            StatusSoakingConfigParameter.AdvancedSetting.ChillingRatioPolicy.Add(new Chillingratiopolicy((int)SoakingRatioPolicy_Index.Maintain_UseStandardWafer, Maintain_UseStandardWafer_Ratio_Title, Maintain_UseStandardWafer_Ratio));
            StatusSoakingConfigParameter.AdvancedSetting.ChillingRatioPolicy.Add(new Chillingratiopolicy((int)SoakingRatioPolicy_Index.Recovery_NotExistWafer, Recovery_NotExistWafer_Ratio_Title, Recovery_NotExistWafer_Ratio));

            // eventsokaing
            StatusSoakingConfigParameter.StatusSoakingEvents.Add(new StatusSoakingEvent(EventSoakType.EveryWaferSoak.ToString(), EventSoakType.EveryWaferSoak));
            StatusSoakingConfigParameter.StatusSoakingEvents.Add(new StatusSoakingEvent(EventSoakType.BeforeZUpSoak.ToString(), EventSoakType.BeforeZUpSoak));

            return EventCodeEnum.NONE;
        }

        public void SetElementMetaData()
        {

        }

        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }

        private StatusSoakingConfig _StatusSoakingConfigParameter = new StatusSoakingConfig();
        public StatusSoakingConfig StatusSoakingConfigParameter
        {
            get { return _StatusSoakingConfigParameter; }
            set
            {
                if (value != _StatusSoakingConfigParameter)
                {
                    _StatusSoakingConfigParameter = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region Export_ParameterInterface
        /// <summary>
        /// Prepare, Recovery, Maintain에서 공통으로 가지고 있는 Parameter반환
        /// </summary>
        /// <param name="StatusSoaking"> 가져올 Soaking Status</param>
        /// <returns></returns>
        private StatusSoakingCommonParam Get_StausSoakingCommonParam(SoakingStateEnum StatusSoaking)
        {
            if (null == StatusSoakingConfigParameter)
                return null;

            if (SoakingStateEnum.PREPARE == StatusSoaking)
                return StatusSoakingConfigParameter.GetPrepareStatusCommonParam();
            else if (SoakingStateEnum.RECOVERY == StatusSoaking)
                return StatusSoakingConfigParameter.GetRecoveryStatusCommonParam();
            else if (SoakingStateEnum.MAINTAIN == StatusSoaking)
                return StatusSoakingConfigParameter.GetMaintainStatusCommonParam();
            else if (SoakingStateEnum.MANUAL == StatusSoaking)
                return StatusSoakingConfigParameter.GetManualStatusCommonParam();
            else
                return null;
        }

        /// <summary>
        /// Status Soaking 사용 여부 반환
        /// </summary>
        /// <param name="enableFlag">사용여부 반환</param>
        /// <returns>true: 성공, false: 실패</returns>
        public bool IsEnableStausSoaking(ref bool enableFlag)
        {
            if (null == StatusSoakingConfigParameter)
            {
                LoggerManager.SoakingErrLog($"'StatusSoakingConfigParameter' is null.");
                return false;
            }

            enableFlag = StatusSoakingConfigParameter.UseStatusSoaking.Value;
            return true;
        }

        /// <summary>
        /// Last Soaking State 사용 여부 반환
        /// </summary>
        /// <param name="useLastSoakingState">사용여부 반환</param>
        /// <returns>true: 성공, false: 실패</returns>
        public bool IsUseLastStatusSoakingState(ref bool useLastSoakingState)
        {
            if (null == StatusSoakingConfigParameter)
            {
                LoggerManager.SoakingErrLog($"'StatusSoakingConfigParameter' is null.");
                return false;
            }

            useLastSoakingState = StatusSoakingConfigParameter.AdvancedSetting.UseLastSoakingState.Value;
            return true;
        }

        /// <summary>
        /// PinAlign 유효 시간으로 잦은 시간 Full PinAlign을 하지 않기 위해 마지막 PinAlign한 시간을 기준으로 몇초동안 해당 PinAlign정보를 유효하게 사용할 것인지를 결정하는 Param
        /// </summary>
        /// <param name="pinAlignValidTimeSec"> pinAlign 유효시간</param>
        /// <returns>true: 성공, false: 실패</returns>
        public bool Get_PinAlignValidTimeSec(ref int pinAlignValidTimeSec)
        {
            if (null == StatusSoakingConfigParameter)
            {
                LoggerManager.SoakingErrLog($"'StatusSoakingConfigParameter' is null.");
                return false;
            }

            pinAlignValidTimeSec = StatusSoakingConfigParameter.AdvancedSetting.PinAlignValidTimeSec.Value;
            return true;
        }

        /// <summary>
        /// Prepare soaking time 반환
        /// </summary>
        /// <param name="PrepareSoakingTime">prepare에서 soaking할 시간 반환(초)</param>
        /// <returns>true:성공, false:실패 </returns>
        public bool Get_PrepareStatusSoakingTimeSec(ref int PrepareSoakingTime, bool useTempDiff_SoakingTime = false)
        {
            if (null == StatusSoakingConfigParameter)
            {
                LoggerManager.SoakingErrLog($"'Prepare - StatusSoakingConfigParameter' is null.");
                return false;
            }

            if (false == useTempDiff_SoakingTime)
            {
                PrepareSoakingTime = StatusSoakingConfigParameter.PrepareStateConfig.SoakingTimeSec.Value;
            }
            else
            {
                PrepareSoakingTime = StatusSoakingConfigParameter.PrepareStateConfig.TempDiffSoakingTimeSec.Value;
            }

            return true;
        }

        /// <summary>
        /// Soaking시 Polish wafer사용여부 반환
        /// </summary>
        /// <param name="StatusSoaking">가져올 Soaking Status</param>
        /// <param name="usePolishWaferFlag">사용여부</param>
        /// <returns>true:성공, false:실패 </returns>
        public bool Get_UsePolishWaferFlag(SoakingStateEnum StatusSoaking, ref bool usePolishWaferFlag)
        {
            if (null == StatusSoakingConfigParameter)
            {
                LoggerManager.SoakingErrLog($"'StatusSoakingConfigParameter' is null.(SoakingStateEnum:{StatusSoaking.ToString()})");
                return false;
            }

            StatusSoakingCommonParam paramInfo = Get_StausSoakingCommonParam(StatusSoaking);
            if (null == paramInfo)
            {
                LoggerManager.SoakingErrLog($"'StatusSoakingCommonParam' is null.(SoakingStateEnum:{StatusSoaking.ToString()})");
                return false;
            }

            usePolishWaferFlag = paramInfo.UsePolishWafer.Value;
            return true;
        }

        /// <summary>
        /// Edge detection 여부 반환
        /// </summary>
        /// <param name="StatusSoaking">가져올 Soaking Status</param>
        /// <param name="enalbeEdgeDetection">사용여부</param>
        /// <returns>true:성공, false:실패 </returns>
        public bool Get_EnableEdgeDetectionFlag(SoakingStateEnum StatusSoaking, ref bool enalbeEdgeDetection)
        {
            if (null == StatusSoakingConfigParameter)
            {
                LoggerManager.SoakingErrLog($"'StatusSoakingConfigParameter' is null.(SoakingStateEnum:{StatusSoaking.ToString()})");
                return false;
            }

            StatusSoakingCommonParam paramInfo = Get_StausSoakingCommonParam(StatusSoaking);
            if (null == paramInfo)
            {
                LoggerManager.SoakingErrLog($"'StatusSoakingCommonParam' is null.(SoakingStateEnum:{StatusSoaking.ToString()})");
                return false;
            }

            enalbeEdgeDetection = paramInfo.EnableEdgeDetection.Value;
            return true;
        }

        /// <summary>
        /// Polish wafer name list 반환
        /// </summary>
        /// <param name="StatusSoaking">가져올 Soaking Status</param>
        /// <param name="polishwaferList">Polish wafer name list</param>
        /// <returns>true:성공, false:실패 </returns>
        public bool Get_SelectedPolishWaferName(SoakingStateEnum StatusSoaking, ref ObservableCollection<string> polishwaferList)
        {
            if (null == StatusSoakingConfigParameter)
            {
                LoggerManager.SoakingErrLog($"'StatusSoakingConfigParameter' is null.(SoakingStateEnum:{StatusSoaking.ToString()})");
                return false;
            }

            StatusSoakingCommonParam paramInfo = Get_StausSoakingCommonParam(StatusSoaking);
            if (null == paramInfo)
            {
                LoggerManager.SoakingErrLog($"'StatusSoakingCommonParam' is null.(SoakingStateEnum:{StatusSoaking.ToString()})");
                return false;
            }

            foreach (var Item in paramInfo.SelectedPolishwafer)
                polishwaferList.Add(Item.PolishWaferName.Value);

            return true;
        }

        /// <summary>
        /// Soaking Step list 반환
        /// </summary>
        /// <param name="StatusSoaking">가져올 Soaking Status</param>
        /// <param name="soakingStepList">Soaking step list</param>
        /// <returns>true:성공, false:실패 </returns>
        public bool Get_SoakingStepTableList(SoakingStateEnum StatusSoaking, ref List<SoakingStepListInfo> soakingStepList)
        {
            if (null == StatusSoakingConfigParameter)
            {
                LoggerManager.SoakingErrLog($"'StatusSoakingConfigParameter' is null.(SoakingStateEnum:{StatusSoaking.ToString()})");
                return false;
            }

            StatusSoakingCommonParam paramInfo = Get_StausSoakingCommonParam(StatusSoaking);
            if (null == paramInfo)
            {
                LoggerManager.SoakingErrLog($"'StatusSoakingCommonParam' is null.(SoakingStateEnum:{StatusSoaking.ToString()})");
                return false;
            }

            foreach (var Item in paramInfo.SoakingStepTable)
            {
                soakingStepList.Add(new SoakingStepListInfo(Item.StepIdx.Value, Item.TimeSec.Value, Item.OD_Value.Value));
            }

            return true;
        }

        /// <summary>
        /// Wafer object가 없는 경우 사용할 OD값 반환
        /// </summary>
        /// <param name="StatusSoaking"> 가져올 Soaking Status </param>
        /// <param name="Od_value">OD Valuse</param>
        /// <returns>true:성공, false:실패 </returns>
        public bool Get_NotExistWaferObj_OD(SoakingStateEnum StatusSoaking, ref double Od_value)
        {
            if (null == StatusSoakingConfigParameter)
            {
                LoggerManager.SoakingErrLog($"'StatusSoakingConfigParameter' is null.(SoakingStateEnum:{StatusSoaking.ToString()})");
                return false;
            }

            StatusSoakingCommonParam paramInfo = Get_StausSoakingCommonParam(StatusSoaking);
            if (null == paramInfo)
            {
                LoggerManager.SoakingErrLog($"'StatusSoakingCommonParam' is null.(SoakingStateEnum:{StatusSoaking.ToString()})");
                return false;
            }
            var advParam = StatusSoakingConfigParameter.AdvancedSetting;
            if (paramInfo.NotExistWaferObj_OD.Value > advParam.ODLimit.Value)
            {
                Od_value = advParam.ODLimit.Value;
            }
            else
            {
                Od_value = paramInfo.NotExistWaferObj_OD.Value;
            }

            return true;
        }

        /// <summary>
        /// Wafer object가 없는 경우 Pin align을 사용할 것인지 여부
        /// </summary>
        /// <param name="StatusSoaking">가져올 Soaking Status </param>
        /// <param name="usePinAlign">사용여부</param>
        /// <returns>true:성공, false:실패 </returns>
        public bool Get_EnableWaitingPinAlign(SoakingStateEnum StatusSoaking, ref bool usePinAlign)
        {
            if (null == StatusSoakingConfigParameter)
            {
                LoggerManager.SoakingErrLog($"'StatusSoakingConfigParameter' is null.(SoakingStateEnum:{StatusSoaking.ToString()})");
                return false;
            }

            StatusSoakingCommonParam paramInfo = Get_StausSoakingCommonParam(StatusSoaking);
            if (null == paramInfo)
            {
                LoggerManager.SoakingErrLog($"'StatusSoakingCommonParam' is null.(SoakingStateEnum:{StatusSoaking.ToString()})");
                return false;
            }

            usePinAlign = paramInfo.EnableWaitingPinAlign.Value;
            return true;
        }

        /// <summary>
        /// Wafer object가 없는 경우 Pin align 주기를 설정한다.
        /// </summary>
        /// <param name="StatusSoaking">가져올 Soaking Status </param>
        /// <param name="pinAlignPeriod">Pin Align 주기</param>
        /// <returns>true:성공, false:실패 </returns>
        public bool Get_WaitingPinAlignTime(SoakingStateEnum StatusSoaking, ref int pinAlignPeriod)
        {
            if (null == StatusSoakingConfigParameter)
            {
                LoggerManager.SoakingErrLog($"'StatusSoakingConfigParameter' is null.(SoakingStateEnum:{StatusSoaking.ToString()})");
                return false;
            }

            StatusSoakingCommonParam paramInfo = Get_StausSoakingCommonParam(StatusSoaking);
            if (null == paramInfo)
            {
                LoggerManager.SoakingErrLog($"'StatusSoakingCommonParam' is null.(SoakingStateEnum:{StatusSoaking.ToString()})");
                return false;
            }

            pinAlignPeriod = paramInfo.WaitingPinAlignPeriod.Value;
            return true;
        }

        /// <summary>
        /// Chilling Time table정보 반환
        /// </summary>
        /// <param name="ChillingTimeTableList">Chilling Time List 반환</param>
        /// <returns>true:성공, false:실패 </returns>
        public bool Get_ChillingTimeTableInfo(ref List<ChillingTimeTableInfo> ChillingTimeTableList)
        {
            if (null == StatusSoakingConfigParameter)
            {
                LoggerManager.SoakingErrLog($"Recovery - 'StatusSoakingConfigParameter' is null.");
                return false;
            }

            var ChillingTimeList = StatusSoakingConfigParameter.RecoveryStateConfig.SoakingChillingTimeTable;
            foreach (var Item in ChillingTimeList)
            {
                ChillingTimeTableList.Add(new ChillingTimeTableInfo(Item.StepIdx.Value, Item.ChillingTimeSec.Value, Item.SoakingTimeSec.Value));
            }
            return true;
        }

        public bool Get_OperationForElapsedTimeSec(ref int OperationForElapsedTimeSec)
        {
            if (null == StatusSoakingConfigParameter)
            {
                LoggerManager.SoakingErrLog($"Advanced Setting - 'StatusSoakingConfigParameter' is null.");
                return false;
            }

            OperationForElapsedTimeSec = StatusSoakingConfigParameter.AdvancedSetting.OperationForElapsedTimeSec.Value;
            return true;
        }

        public bool Get_SoakingElapsedTimeSecForPWLoading(ref int SoakingElapsedTimeSecForPWLoading)
        {
            if (null == StatusSoakingConfigParameter)
            {
                LoggerManager.SoakingErrLog($"Advanced Setting - 'StatusSoakingConfigParameter' is null.");
                return false;
            }

            SoakingElapsedTimeSecForPWLoading = StatusSoakingConfigParameter.AdvancedSetting.SoakingElapsedTimeSecForPWLoading.Value;
            return true;
        }

        /// <summary>
        /// Status별로 모두 공통적으로 설정되는 Setting값을 반환
        /// </summary>
        /// <param name="StatusSoaking"> 가져올 Soaking Status  </param>
        /// <param name="SoakingCommonParam"> Soaking의 공통정보</param>
        /// <returns>true :성공, false: 실패</returns>
        public bool Get_StatusCommonOption(SoakingStateEnum StatusSoaking, ref StatusSoakingCommonParameterInfo SoakingCommonParam)
        {
            if (null == StatusSoakingConfigParameter)
            {
                LoggerManager.SoakingErrLog($"'StatusSoakingConfigParameter' is null.(SoakingStateEnum:{StatusSoaking.ToString()})");
                return false;
            }

            if (SoakingStateEnum.STATUS_EVENT_SOAK == StatusSoaking)
            {
                return true; //event soaking param은 별도 CommonParam이 없음. 호출단에서 처리 해준다.
            }

            StatusSoakingCommonParam paramInfo = Get_StausSoakingCommonParam(StatusSoaking);
            if (null == paramInfo)
            {
                LoggerManager.SoakingErrLog($"'StatusSoakingCommonParam' is null.(SoakingStateEnum:{StatusSoaking.ToString()})");
                return false;
            }

            SoakingCommonParam.use_polishwafer = paramInfo.UsePolishWafer.Value;
            SoakingCommonParam.enalbe_edge_detection = paramInfo.EnableEdgeDetection.Value;
            foreach (var Item in paramInfo.SelectedPolishwafer)
                SoakingCommonParam.polishwaferList.Add(Item.PolishWaferName.Value);

            foreach (var Item in paramInfo.SoakingStepTable)
                SoakingCommonParam.soakingStepList.Add(new SoakingStepListInfo(Item.StepIdx.Value, Item.TimeSec.Value, Item.OD_Value.Value));

            SoakingCommonParam.notExistWaferObj_ODVal = paramInfo.NotExistWaferObj_OD.Value;
            SoakingCommonParam.enableWatingPinAlign = paramInfo.EnableWaitingPinAlign.Value;
            SoakingCommonParam.waitingPinAlignPeriodSec = paramInfo.WaitingPinAlignPeriod.Value;
            return true;
        }

        /// <summary>
        /// soaking step 마다 진행하는 pin align에서 마지막 step의 pin align의 수행여부를 판단하는 시간
        /// (해당 시간 보다 남은 soaking 시간이 작다면 pin align을 하지 않기 위함)
        /// </summary>
        /// <param name="AllowalbeTime"></param>
        /// <returns></returns>
        public bool Get_AllowableTimeForLastPinAign(ref int AllowalbeTime)
        {
            if (null == StatusSoakingConfigParameter)
            {
                LoggerManager.SoakingErrLog($"'StatusSoakingConfigParameter' is null.");
                return false;
            }

            AllowalbeTime = StatusSoakingConfigParameter.AdvancedSetting.AllowableTimeForLastPinAlignSec.Value;
            return true;
        }

        /// <summary>
        /// event에 해당하는 parameter 정보를 반환
        /// </summary>
        /// <param name="soakingType"> 가져올 event Soaking type  </param>
        /// <param name="SoakingCommonParam"> Soaking의 공통정보</param>
        /// <returns>true :성공, false: 실패</returns>
        public bool Get_EventSoakingParam(EventSoakType soakingType, ref EventSoakingParameterInfo EventSoakingParam)
        {
            if (null == StatusSoakingConfigParameter)
            {
                LoggerManager.SoakingErrLog($"'StatusSoakingConfigParameter' is null.(event:{soakingType.ToString()})");
                return false;
            }

            var TargetEventSoakingParam = StatusSoakingConfigParameter.StatusSoakingEvents.FirstOrDefault(x => x.SoakingTypeEnum.Value == soakingType);
            if (null != TargetEventSoakingParam)
            {
                EventSoakingParam.IsEnable = TargetEventSoakingParam.UseEventSoaking.Value;
                EventSoakingParam.Soaking_TimeSec = TargetEventSoakingParam.SoakingTimeSec.Value;
                EventSoakingParam.Soaking_Priority = TargetEventSoakingParam.SoakingPriority.Value;
                EventSoakingParam.OD_Value = TargetEventSoakingParam.OD_Value.Value;
                EventSoakingParam.Soaking_PinAlignMode = TargetEventSoakingParam.PinAlignMode.Value;
                return true;
            }
            else
            {
                LoggerManager.SoakingErrLog($"Failed to get event param info({soakingType.ToString()})");
                return false;
            }
        }

        public bool Get_ManualSoakingTime(ref int SoakingTime)
        {
            if (null == StatusSoakingConfigParameter)
            {
                LoggerManager.SoakingErrLog($"'Manual - StatusSoakingConfigParameter' is null.");
                return false;
            }

            SoakingTime = StatusSoakingConfigParameter.ManualSoakingConfig.SoakingTimeSec.Value;
            return true;
        }
        public bool Get_ShowStatusSoakingSettingPageToggleValue()
        {
            bool ToggleValue = false;
            if (null == StatusSoakingConfigParameter)
            {
                LoggerManager.SoakingErrLog($"'StatusSoakingConfigParameter' is null.");
                return ToggleValue;
            }

            ToggleValue = StatusSoakingConfigParameter.ShowStatusSoakingSettingPage.Value;
            return ToggleValue;
        }
        public void Set_ShowStatusSoakingSettingPageToggleValue(bool ToggleValue)
        {
            if (null != StatusSoakingConfigParameter)
            {
                StatusSoakingConfigParameter.ShowStatusSoakingSettingPage.Value = ToggleValue;
            }
            else
            {
                LoggerManager.SoakingErrLog($"'StatusSoakingConfigParameter' is null.");
            }
        }

        /// <summary>
        /// 인자로 들어온 Chilling time 정책 index에 맞는 Ratio 값을 반환한다.
        /// </summary>
        /// <param name="ChillingTimeRatio_index"> 가져올 Chilling Time Ratio index</param>
        /// <returns></returns>
        public float Get_ChillingTimeRatio(SoakingRatioPolicy_Index ChillingTimeRatio_index)
        {
            try
            {
                if (null != StatusSoakingConfigParameter)
                {
                    var FindPolicyItem = StatusSoakingConfigParameter.AdvancedSetting.ChillingRatioPolicy.Where(x => x.PolicyIndex.Value == (int)ChillingTimeRatio_index).FirstOrDefault();
                    if (null != FindPolicyItem)
                    {
                        return FindPolicyItem.Ratio.Value;
                    }
                }
                else
                {
                    LoggerManager.SoakingErrLog($"'StatusSoakingConfigParameter' is null.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return 0f;
        }
        #endregion
    }

    /// <summary>
    /// Param node와 INotifyPropertyChanged를 상속한 Class
    /// </summary>
    [Serializable]
    public class NotifyPropertyChangeAndParamNode : INotifyPropertyChanged, IParamNode
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }
        [XmlIgnore, JsonIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
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
    }

    /// <summary>
    /// 신규 Soaking에 관련된 Parameter 
    /// </summary>
    [Serializable]
    public class StatusSoakingConfig : NotifyPropertyChangeAndParamNode
    {
        private Element<bool> _ShowStatusSoakingSettingPage = new Element<bool> { Value = false };
        public Element<bool> ShowStatusSoakingSettingPage
        {
            get { return _ShowStatusSoakingSettingPage; }
            set
            {
                if (value != _ShowStatusSoakingSettingPage)
                {
                    _ShowStatusSoakingSettingPage = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _UseStatusSoaking = new Element<bool> { Value = false };
        public Element<bool> UseStatusSoaking
        {
            get { return _UseStatusSoaking; }
            set
            {
                if (value != _UseStatusSoaking)
                {
                    _UseStatusSoaking = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Advancedsetting _AdvancedSetting = new Advancedsetting(); //Advance setting parameter
        public Advancedsetting AdvancedSetting
        {
            get { return _AdvancedSetting; }
            set
            {
                if (value != _AdvancedSetting)
                {
                    _AdvancedSetting = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Preparestate _PrepareState = new Preparestate(); //Prepare setting parameter
        public Preparestate PrepareStateConfig
        {
            get { return _PrepareState; }
            set
            {
                if (value != _PrepareState)
                {
                    _PrepareState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Recoverystate _RecoveryState = new Recoverystate(); //Recovery setting parameter
        public Recoverystate RecoveryStateConfig
        {
            get { return _RecoveryState; }
            set
            {
                if (value != _RecoveryState)
                {
                    _RecoveryState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Maintainstate _MaintainState = new Maintainstate(); //Maintain setting parameter
        public Maintainstate MaintainStateConfig
        {
            get { return _MaintainState; }
            set
            {
                if (value != _MaintainState)
                {
                    _MaintainState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Manualsoaking _ManualSoakingParam = new Manualsoaking(); // manual soaking setting parameter

        public Manualsoaking ManualSoakingConfig
        {
            get { return _ManualSoakingParam; }
            set
            {
                if (value != _ManualSoakingParam)
                {
                    _ManualSoakingParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        // event soaking list
        private ObservableCollection<StatusSoakingEvent> _StatusSoakingEventList = new ObservableCollection<StatusSoakingEvent>();
        public ObservableCollection<StatusSoakingEvent> StatusSoakingEvents
        {
            get { return _StatusSoakingEventList; }
            set
            {
                if (value != _StatusSoakingEventList)
                {
                    _StatusSoakingEventList = value;
                    RaisePropertyChanged();
                }
            }
        }

        //Prepare soaking parameter 반환
        public StatusSoakingCommonParam GetPrepareStatusCommonParam()
        {
            return PrepareStateConfig;
        }

        //Recovery soaking parameter 반환
        public StatusSoakingCommonParam GetRecoveryStatusCommonParam()
        {
            return RecoveryStateConfig;
        }

        //Maintain soaking parameter 반환
        public StatusSoakingCommonParam GetMaintainStatusCommonParam()
        {
            return MaintainStateConfig;
        }

        //Manual soaking parameter 반환
        public StatusSoakingCommonParam GetManualStatusCommonParam()
        {
            return ManualSoakingConfig;
        }

        //Event soaking parameter List 반환
        public ObservableCollection<StatusSoakingEvent> GetEventStatusCommonParam()
        {
            return StatusSoakingEvents;
        }
    }

    /// <summary>
    /// Advance option parameter
    /// </summary>
    [Serializable]
    public class Advancedsetting : NotifyPropertyChangeAndParamNode
    {
        #region Section 1 Chuck away tolerance to chilling time & Operation Time
        private Element<double> _ChuckAwayTolForChillingTime_X = new Element<double> { Value = 100 };
        public Element<double> ChuckAwayTolForChillingTime_X
        {
            get { return _ChuckAwayTolForChillingTime_X; }
            set
            {
                if (value != _ChuckAwayTolForChillingTime_X)
                {
                    _ChuckAwayTolForChillingTime_X = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _ChuckAwayTolForChillingTime_Y = new Element<double> { Value = 100 };
        public Element<double> ChuckAwayTolForChillingTime_Y
        {
            get { return _ChuckAwayTolForChillingTime_Y; }
            set
            {
                if (value != _ChuckAwayTolForChillingTime_Y)
                {
                    _ChuckAwayTolForChillingTime_Y = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _ChuckAwayTolForChillingTime_Z = new Element<double> { Value = 100 };
        public Element<double> ChuckAwayTolForChillingTime_Z
        {
            get { return _ChuckAwayTolForChillingTime_Z; }
            set
            {
                if (value != _ChuckAwayTolForChillingTime_Z)
                {
                    _ChuckAwayTolForChillingTime_Z = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _OperationForElapsedTimeSec = new Element<int> { Value = 10 };
        public Element<int> OperationForElapsedTimeSec
        {
            get { return _OperationForElapsedTimeSec; }
            set
            {
                if (value != _OperationForElapsedTimeSec)
                {
                    _OperationForElapsedTimeSec = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _UnderDutDiesPercnetage = new Element<int> { Value = 95 };
        public Element<int> UnderDutDiesPercnetage
        {
            get { return _UnderDutDiesPercnetage; }
            set
            {
                if (value != _UnderDutDiesPercnetage)
                {
                    _UnderDutDiesPercnetage = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _SoakingElapsedTimeSecForPWLoading = new Element<int> { Value = 180 };
        public Element<int> SoakingElapsedTimeSecForPWLoading
        {
            get { return _SoakingElapsedTimeSecForPWLoading; }
            set
            {
                if (value != _SoakingElapsedTimeSecForPWLoading)
                {
                    _SoakingElapsedTimeSecForPWLoading = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region Section 2 About Pin Align

        private Element<int> _PinAlignValidTimeSec = new Element<int> { Value = 1800 };
        public Element<int> PinAlignValidTimeSec
        {
            get { return _PinAlignValidTimeSec; }
            set
            {
                if (value != _PinAlignValidTimeSec)
                {
                    _PinAlignValidTimeSec = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _AllowableTimeForLastPinAlignSec = new Element<int> { Value = 30 };
        public Element<int> AllowableTimeForLastPinAlignSec
        {
            get { return _AllowableTimeForLastPinAlignSec; }
            set
            {
                if (value != _AllowableTimeForLastPinAlignSec)
                {
                    _AllowableTimeForLastPinAlignSec = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Section 3 Chuck Focusing setting
        private Element<double> _ThicknessTolerance = new Element<double> { Value = 150 };
        public Element<double> ThicknessTolerance
        {
            get { return _ThicknessTolerance; }
            set
            {
                if (value != _ThicknessTolerance)
                {
                    _ThicknessTolerance = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<EnumLightType> _LightType = new Element<EnumLightType> { Value = EnumLightType.COAXIAL };
        public Element<EnumLightType> LightType
        {
            get { return _LightType; }
            set
            {
                if (value != _LightType)
                {
                    _LightType = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Element<ushort> _LightValue = new Element<ushort> { Value = 70 };
        public Element<ushort> LightValue
        {
            get { return _LightValue; }
            set
            {
                if (value != _LightValue)
                {
                    _LightValue = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region Section 4 Limit
        private Element<int> _ChillingTimeMin = new Element<int> { Value = 60 };
        public Element<int> ChillingTimeMin
        {
            get { return _ChillingTimeMin; }
            set
            {
                if (value != _ChillingTimeMin)
                {
                    _ChillingTimeMin = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _ChillingSoakingTimeMin = new Element<int> { Value = 60 };
        public Element<int> ChillingSoakingTimeMin
        {
            get { return _ChillingSoakingTimeMin; }
            set
            {
                if (value != _ChillingSoakingTimeMin)
                {
                    _ChillingSoakingTimeMin = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _SoakingStepTimeMin = new Element<int> { Value = 60 };
        public Element<int> SoakingStepTimeMin
        {
            get { return _SoakingStepTimeMin; }
            set
            {
                if (value != _SoakingStepTimeMin)
                {
                    _SoakingStepTimeMin = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _ODLimit = new Element<int> { Value = -80 };
        public Element<int> ODLimit
        {
            get { return _ODLimit; }
            set
            {
                if (value != _ODLimit)
                {
                    _ODLimit = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _SoakingTempTolerance = new Element<int> { Value = 5, LowerLimit = 0, UpperLimit = 10 };
        public Element<int> SoakingTempTolerance
        {
            get { return _SoakingTempTolerance; }
            set
            {
                if (value != _SoakingTempTolerance)
                {
                    _SoakingTempTolerance = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region Section 5 Continue to last soaking status 
        private Element<bool> _UseLastSoakingState = new Element<bool> { Value = false };
        public Element<bool> UseLastSoakingState
        {
            get { return _UseLastSoakingState; }
            set
            {
                if (value != _UseLastSoakingState)
                {
                    _UseLastSoakingState = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region Section 6 Config Chilling Ratio
        private ObservableCollection<Chillingratiopolicy> _ChillingRatioPolicy = new ObservableCollection<Chillingratiopolicy>();
        public ObservableCollection<Chillingratiopolicy> ChillingRatioPolicy
        {
            get { return _ChillingRatioPolicy; }
            set
            {
                if (value != _ChillingRatioPolicy)
                {
                    _ChillingRatioPolicy = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

    }

    /// <summary>
    /// Prepare soaking 설정 parameter
    /// </summary>
    [Serializable]
    public class Preparestate : StatusSoakingCommonParam
    {
        private Element<int> _SoakingTimeSec = new Element<int>() { Value = 3600 };
        public Element<int> SoakingTimeSec
        {
            get { return _SoakingTimeSec; }
            set
            {
                if (value != _SoakingTimeSec)
                {
                    _SoakingTimeSec = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _TriggerDegreeForNormal = new Element<int>() { Value = 1 };
        public Element<int> TriggerDegreeForNormal
        {
            get { return _TriggerDegreeForNormal; }
            set
            {
                if (value != _TriggerDegreeForNormal)
                {
                    _TriggerDegreeForNormal = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _TempDiffSoakingTimeSec = new Element<int>() { Value = 7200 };
        public Element<int> TempDiffSoakingTimeSec
        {
            get { return _TempDiffSoakingTimeSec; }
            set
            {
                if (value != _TempDiffSoakingTimeSec)
                {
                    _TempDiffSoakingTimeSec = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _TriggerDegreeForTempDiff = new Element<int>() { Value = 80 };
        public Element<int> TriggerDegreeForTempDiff
        {
            get { return _TriggerDegreeForTempDiff; }
            set
            {
                if (value != _TriggerDegreeForTempDiff)
                {
                    _TriggerDegreeForTempDiff = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    /// <summary>
    /// Recovery soaking 설정 parameter
    /// </summary>
    [Serializable]
    public class Recoverystate : StatusSoakingCommonParam
    {
        private ObservableCollection<Chillingtimetable> _SoakingChillingTimeTable = new ObservableCollection<Chillingtimetable>();
        public ObservableCollection<Chillingtimetable> SoakingChillingTimeTable
        {
            get { return _SoakingChillingTimeTable; }
            set
            {
                if (value != _SoakingChillingTimeTable)
                {
                    _SoakingChillingTimeTable = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    /// <summary>
    /// Maintain soaking 설정 parameter
    /// </summary>
    [Serializable]
    public class Maintainstate : StatusSoakingCommonParam
    {
    }

    [Serializable]
    public class Manualsoaking : StatusSoakingCommonParam
    {
        private Element<int> _SoakingTimeSec = new Element<int>();
        public Element<int> SoakingTimeSec
        {
            get { return _SoakingTimeSec; }
            set
            {
                if (value != _SoakingTimeSec)
                {
                    _SoakingTimeSec = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    /// <summary>
    /// StatusSoaking 공통 Parameter
    /// </summary>
    [Serializable]
    public class StatusSoakingCommonParam : NotifyPropertyChangeAndParamNode
    {
        private Element<bool> _UsePolishWafer = new Element<bool> { Value = false };
        public Element<bool> UsePolishWafer
        {
            get { return _UsePolishWafer; }
            set
            {
                if (value != _UsePolishWafer)
                {
                    _UsePolishWafer = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _EnableEdgeDetection = new Element<bool> { Value = false };
        public Element<bool> EnableEdgeDetection
        {
            get { return _EnableEdgeDetection; }
            set
            {
                if (value != _EnableEdgeDetection)
                {
                    _EnableEdgeDetection = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<Selectedpolishwafer> _SelectedPolishwafer = new ObservableCollection<Selectedpolishwafer>();
        public ObservableCollection<Selectedpolishwafer> SelectedPolishwafer
        {
            get { return _SelectedPolishwafer; }
            set
            {
                if (value != _SelectedPolishwafer)
                {
                    _SelectedPolishwafer = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<Soakingsteptable> _SoakingStepTable = new ObservableCollection<Soakingsteptable>();
        public ObservableCollection<Soakingsteptable> SoakingStepTable
        {
            get { return _SoakingStepTable; }
            set
            {
                if (value != _SoakingStepTable)
                {
                    _SoakingStepTable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _NotExistWaferObj_OD = new Element<double> { Value = -200 };
        public Element<double> NotExistWaferObj_OD
        {
            get { return _NotExistWaferObj_OD; }
            set
            {
                if (value != _NotExistWaferObj_OD)
                {
                    _NotExistWaferObj_OD = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _EnableWaitingPinAlign = new Element<bool> { Value = false };
        public Element<bool> EnableWaitingPinAlign
        {
            get { return _EnableWaitingPinAlign; }
            set
            {
                if (value != _EnableWaitingPinAlign)
                {
                    _EnableWaitingPinAlign = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _WaitingPinAlignPeriod = new Element<int> { Value = 300 };
        public Element<int> WaitingPinAlignPeriod
        {
            get { return _WaitingPinAlignPeriod; }
            set
            {
                if (value != _WaitingPinAlignPeriod)
                {
                    _WaitingPinAlignPeriod = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    /// <summary>
    /// Chilling Time Policy table parameter
    /// </summary>
    [Serializable]
    public class Chillingratiopolicy : NotifyPropertyChangeAndParamNode
    {
        public Chillingratiopolicy() // 인자를 받는 생성자가 있다면 default 생성자가 있어야함(json deserialize 위해)
        {

        }

        public Chillingratiopolicy(int policyIndex, string ratioPolicyName, float ratio_val)
        {
            this.PolicyIndex.Value = policyIndex;
            this.RatioPolicyName.Value = ratioPolicyName;
            this.Ratio.Value = ratio_val;
        }

        private Element<int> _PolicyIndex = new Element<int>();
        public Element<int> PolicyIndex
        {
            get { return _PolicyIndex; }
            set
            {
                if (value != _PolicyIndex)
                {
                    _PolicyIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _RatioPolicyName = new Element<string>();
        public Element<string> RatioPolicyName
        {
            get { return _RatioPolicyName; }
            set
            {
                if (value != _RatioPolicyName)
                {
                    _RatioPolicyName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<float> _Ratio = new Element<float>();
        public Element<float> Ratio
        {
            get { return _Ratio; }
            set
            {
                if (value != _Ratio)
                {
                    _Ratio = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    /// <summary>
    /// 사용할 PolicyWafer Parameter
    /// </summary>
    [Serializable]
    public class Selectedpolishwafer : NotifyPropertyChangeAndParamNode
    {
        public Selectedpolishwafer() { }

        public Selectedpolishwafer(string polishwaferName)
        {
            this.PolishWaferName.Value = polishwaferName;
        }

        private Element<string> _PolishWaferName = new Element<string>();
        public Element<string> PolishWaferName
        {
            get { return _PolishWaferName; }
            set
            {
                if (value != _PolishWaferName)
                {
                    _PolishWaferName = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    /// <summary>
    /// Soaking Step table Parameter
    /// </summary>
    [Serializable]
    public class Soakingsteptable : NotifyPropertyChangeAndParamNode
    {
        public Soakingsteptable() { }

        public Soakingsteptable(int StepNo, int SoakingTimeSec, double od_val)
        {
            this.StepIdx.Value = StepNo;
            this.TimeSec.Value = SoakingTimeSec;
            this.OD_Value.Value = od_val;
        }

        private Element<int> _StepIdx = new Element<int>();
        public Element<int> StepIdx
        {
            get { return _StepIdx; }
            set
            {
                if (value != _StepIdx)
                {
                    _StepIdx = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _TimeSec = new Element<int>();
        public Element<int> TimeSec
        {
            get { return _TimeSec; }
            set
            {
                if (value != _TimeSec)
                {
                    _TimeSec = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _OD_Value = new Element<double>();
        public Element<double> OD_Value
        {
            get { return _OD_Value; }
            set
            {
                if (value != _OD_Value)
                {
                    _OD_Value = value;
                    RaisePropertyChanged();
                }
            }
        }

    }

    /// <summary>
    /// chilling time table parameter
    /// </summary>
    [Serializable]
    public class Chillingtimetable : NotifyPropertyChangeAndParamNode
    {
        public Chillingtimetable() { }
        public Chillingtimetable(int StepNo, int ChillingTimeSec, int SoakingTimeSec)
        {
            this.StepIdx.Value = StepNo;
            this.ChillingTimeSec.Value = ChillingTimeSec;
            this.SoakingTimeSec.Value = SoakingTimeSec;
        }

        private Element<int> _StepIdx = new Element<int>();
        public Element<int> StepIdx
        {
            get { return _StepIdx; }
            set
            {
                if (value != _StepIdx)
                {
                    _StepIdx = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _ChillingTimeSec = new Element<int>();
        public Element<int> ChillingTimeSec
        {
            get { return _ChillingTimeSec; }
            set
            {
                if (value != _ChillingTimeSec)
                {
                    _ChillingTimeSec = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _SoakingTimeSec = new Element<int>();
        public Element<int> SoakingTimeSec
        {
            get { return _SoakingTimeSec; }
            set
            {
                if (value != _SoakingTimeSec)
                {
                    _SoakingTimeSec = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    /// <summary>
    /// Event soaking parameter
    /// </summary>
    [Serializable]
    public class StatusSoakingEvent : NotifyPropertyChangeAndParamNode
    {
        public StatusSoakingEvent() { }
        public StatusSoakingEvent(string EventSoakingName, EventSoakType emSoakingType)
        {
            this.StatusSoakingEventName.Value = EventSoakingName;
            this.SoakingTypeEnum.Value = emSoakingType;
        }

        private Element<string> _StatusSoakingEventName = new Element<string>();
        public Element<string> StatusSoakingEventName
        {
            get { return _StatusSoakingEventName; }
            set
            {
                if (value != _StatusSoakingEventName)
                {
                    _StatusSoakingEventName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<EventSoakType> _SoakingTypeEnum = new Element<EventSoakType> { Value = EventSoakType.None };
        public Element<EventSoakType> SoakingTypeEnum
        {
            get { return _SoakingTypeEnum; }
            set
            {
                if (value != _SoakingTypeEnum)
                {
                    _SoakingTypeEnum = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _UseEventSoaking = new Element<bool> { Value = false };
        public Element<bool> UseEventSoaking
        {
            get { return _UseEventSoaking; }
            set
            {
                if (value != _UseEventSoaking)
                {
                    _UseEventSoaking = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _SoakingPriority = new Element<int> { Value = 0 };
        public Element<int> SoakingPriority
        {
            get { return _SoakingPriority; }
            set
            {
                if (value != _SoakingPriority)
                {
                    _SoakingPriority = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _SoakingTimeSec = new Element<int> { Value = 60 };
        public Element<int> SoakingTimeSec
        {
            get { return _SoakingTimeSec; }
            set
            {
                if (value != _SoakingTimeSec)
                {
                    _SoakingTimeSec = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _UseEventSoakingSkip = new Element<bool> { Value = false };
        public Element<bool> UseEventSoakingSkip
        {
            get { return _UseEventSoakingSkip; }
            set
            {
                if (value != _UseEventSoakingSkip)
                {
                    _UseEventSoakingSkip = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _OD_Value = new Element<double> { Value = -200 };
        public Element<double> OD_Value
        {
            get { return _OD_Value; }
            set
            {
                if (value != _OD_Value)
                {
                    _OD_Value = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<PinAlignType> _PinAlignMode = new Element<PinAlignType> { Value = PinAlignType.Full_PinAlign };
        public Element<PinAlignType> PinAlignMode
        {
            get
            {
                return _PinAlignMode;
            }
            set
            {
                if (value != _PinAlignMode)
                {
                    _PinAlignMode = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    public enum PinAlignForStausSoaking
    {
        NEED_TO_PINALIGN = 0,
        REQUESTED_PINALIGN,
        PINALIGN_DONE
    }

    /// <summary>
    /// Soaking 진행 시 Soaking할 step 별 정보로 관리되는 Soaking Parameter 아님
    /// StepIdx는 1부터 시작될 수 있도록 한다.
    /// </summary>
    public class SoakingStepProcInfo
    {
        private readonly int StepFirstIdx = 1;
        public int FIRST_SOAKING_STEP_IDX
        {
            get
            {
                return StepFirstIdx;
            }
        }

        public SoakingStepProcInfo(int StepIdx, int SoakingTm, double ODVal)
        {
            StepIndex = StepIdx;
            SoakingTime = SoakingTm;
            OD_Value = ODVal;

        }
        public int StepIndex { get; set; }  // 1부터 시작될 수 있도록 한다.
        public int SoakingTime { get; set; }
        public double OD_Value { get; set; }
        public bool StepDone { get; set; } = false;
        public PinAlignForStausSoaking PinAlignForStatusSoakingEnum { get; set; } = PinAlignForStausSoaking.NEED_TO_PINALIGN;
    }

    /// <summary>
    /// Status soaking 진행 시 현재 Soaking의 Status와 Soaking 동작시 필요한 정보등을 가지고 있는 Class로
    /// Status soaking 진행에 필요한 정보를 담고 있기 위한 Class
    /// </summary>
    public class StatusSoakingCurrentSetData : StatusSoakingCommonParameterInfo
    {

        public SoakingStateEnum StatusSoaking_State { get; set; }                        // soaking state
        public long StatusSoaking_ElapasedTime { get; set; } = 0;                        //status soaking 진행 시 soaking을 한 시간(Millisecond)
        public long StatusSoaking_ElapasedTime_Org { get; set; } = 0;                    //status soaking 진행 시 soaking을 한 시간(Millisecond)으로 추가 Soaking을 위해 새로 셋팅하기전의 원래 Soaking한 시간
        public long StatusSoakingTime { get; set; } = 0;                                //soaking을 해야 하는 시간(Millisecond)
        public long RecoverySoakingTime_WithoutWafer { get; set; } = 0;                 //Wafer가 없는 soaking을 해야 하는 경우 현재 ChillingTime에서 Ratio를 반영하여 실제 Soaking해야 하는 시간(Millsecond)
        public long RecvAccumlatedChillingTime { get; set; } = 0;                       // ChillingTimeManager로 부터 받은 AccumulatedChillingTime
        public long SoakStart_AccumlatedChillingTime { get; set; } = 0;                 // manual soaking 또는 Event soaking 진행 시 pinalign등으로 발생한 ChillingTime을 계산하기 위해 Soaking 시작시점의 누적된 Chilling Time정보
        public long StatusSoaking_ElapasedTimeWithoutWaferObj { get; set; } = 0;        // wafer object없이 soaking한 시간                      
        public bool Call_PinAlignCMD { get; set; } = false;
        public bool IsCheckedFullPinAlignForSoaking { get; set; } = false;                       // pin/wafer align등이 완료되어 실질적인 soaking 처리가 진행됨(soaking 중간에 pin valid time이 지난 경우 full pin을 또 하지 않기 위함)  
        public bool SoakingAbort { get; set; } = false;
        public new PinAlignType PinAlignMode_AfterSoaking { get; set; } = PinAlignType.Full_PinAlign;
        public DateTime StatusSoaking_StartTime { get; set; } = default; //chuck이 soaking 위치에 배치되고 온도도 문제없어서 실제 soaking을 시작이 된 시        
        public float RecoveryNotExistWafer_Ratio { get; set; } = 0f;
        private SoakingInfo _beforeSendSoakingInfo = new SoakingInfo();
        public SoakingInfo beforeSendSoakingInfo
        {
            get
            {
                return _beforeSendSoakingInfo;
            }
            set
            {
                if (value != _beforeSendSoakingInfo)
                    _beforeSendSoakingInfo = value;
            }
        }
        public bool LastExtraSoakingTime { get; set; } = false;
        public double CurrentODValueForSoaking { get; set; } = -2000;

        public PinAlignForStausSoaking FinalPinAlignAfterSoaking { get; set; } = PinAlignForStausSoaking.NEED_TO_PINALIGN; // soaking이 완료되고 나서 진행하는 full pin align
        public PinAlignForStausSoaking NeedToSamplePinAlign_WithoutWaferObj { get; set; } = PinAlignForStausSoaking.PINALIGN_DONE;

        private ObservableCollection<SoakingStepProcInfo> _StatusSoakingStepProcList = new ObservableCollection<SoakingStepProcInfo>();
        public ObservableCollection<SoakingStepProcInfo> StatusSoakingStepProcList
        {
            get
            {
                return _StatusSoakingStepProcList;
            }
        }

        public bool request_PolishWafer { get; set; } = false;
        public bool NeedToCheck_SoakingStatus_ByChillingTime { get; set; } = false; //wafer 없이 chilling time이 감소되어 maintain으로 전환 가능한 상태 여부
        public bool NeedToPinAlign_For_WithoutWaferSoaking { get; set; } = true;   //wafer 없이 chilling time이 감소되어 maintain으로 전환 가능한 상황에서 pin align 필요 여부

        /// <summary>
        /// Stauts Soaking 진행 시 SoakingSubState에서 Step에 따른 동작을 처리하기 위한 정보 셋팅
        /// </summary>
        /// <param name="StateVal"> 동작될 Soaking status</param>
        /// <param name="AccumualtedChillingTime"> Soaking동작 시 누적된 ChillingTime</param>
        /// <param name="SoakingTime"> Soaking 할 시간 (millesecond) </param>
        /// <param name="CommonParam"> Soaking 공통 정보 </para        
        public void Set_StatusSoakingData(SoakingStateEnum StateVal, long AccumualtedChillingTime, int SoakingTime, ref StatusSoakingCommonParameterInfo CommonParam)
        {
            StatusSoaking_State = StateVal;
            StatusSoaking_ElapasedTime = 0; //soaking 진행한 시간 초기화
            RecoverySoakingTime_WithoutWafer = 0;
            StatusSoaking_ElapasedTime_Org = 0;
            StatusSoaking_ElapasedTimeWithoutWaferObj = 0;
            RecvAccumlatedChillingTime = AccumualtedChillingTime;
            StatusSoakingTime = SoakingTime;
            SoakingAbort = false;
            request_PolishWafer = false;
            StatusSoaking_StartTime = default;
            RecoveryNotExistWafer_Ratio = 0f;
            this.PinAlignMode_AfterSoaking = CommonParam.PinAlignMode_AfterSoaking;
            this.SoakStart_AccumlatedChillingTime = 0;
            this.use_polishwafer = CommonParam.use_polishwafer;
            this.enableWatingPinAlign = CommonParam.enableWatingPinAlign;
            this.waitingPinAlignPeriodSec = CommonParam.waitingPinAlignPeriodSec;
            this.notExistWaferObj_ODVal = CommonParam.notExistWaferObj_ODVal;
            this.enalbe_edge_detection = CommonParam.enalbe_edge_detection;
            this.SoakingEvtType = CommonParam.SoakingEvtType;
            NeedToSamplePinAlign_WithoutWaferObj = PinAlignForStausSoaking.PINALIGN_DONE; //wafer가 없이 진행 시에는 최초 full pin align이 수행되므로 최초에는 done으로 하고 그 다음 부터 pin align
            this.polishwaferList.Clear();
            StatusSoakingStepProcList.Clear();
            Call_PinAlignCMD = false;
            IsCheckedFullPinAlignForSoaking = false;
            FinalPinAlignAfterSoaking = PinAlignForStausSoaking.NEED_TO_PINALIGN;
            LastExtraSoakingTime = false;
            NeedToCheck_SoakingStatus_ByChillingTime = false;
            NeedToPinAlign_For_WithoutWaferSoaking = true;
            //Recovery State라면 Ratio 정책에 따라 현재 Chilling Time에 맞는 Wafer 없을때의 Soaking시간을 산출하여 보관한다.
            if (SoakingStateEnum.RECOVERY == StateVal)
            {
                // Ratio가 0보다 작아 실제 chiling time을 감소하는 경우, soaking 할 시간 정보 표기를 위해 Ratio 보관
                if (CommonParam.Recovery_NotExistWafer_Ratio < 0)
                {
                    RecoveryNotExistWafer_Ratio = CommonParam.Recovery_NotExistWafer_Ratio;
                }
            }

            foreach (var polishWaferItem in CommonParam.polishwaferList)
                this.polishwaferList.Add(polishWaferItem);

            foreach (var StepItem in CommonParam.soakingStepList)
                StatusSoakingStepProcList.Add(new SoakingStepProcInfo(StepItem.stepIndex, StepItem.SoakingTimeSec, StepItem.OD_Value));
        }
    }
}
