using System;
using System.Windows.Threading;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using SoakingParameters;

namespace ProberViewModel
{
    public class UcSoakingODViewModel : UcStatusSoakingViewModelBase, IMainScreenViewModel
    {
        private readonly Guid _ViewModelGUID = new Guid("BEA1C5E1-5CDD-4E39-BC80-EB5C935BD025");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        private readonly Dispatcher dispatcher;

        public event EventHandler<double> ODValueChanged;

        public UcSoakingODViewModel()
        {
            dispatcher = Dispatcher.CurrentDispatcher;
        }

        #region Property
        private double _NotExistWaferObj_OD = new double();
        public double NotExistWaferObj_OD
        {
            get { return _NotExistWaferObj_OD; }
            set
            {
                if (value != _NotExistWaferObj_OD)
                {
                    _NotExistWaferObj_OD = value;
                    RaisePropertyChanged();

                    base.SaveStatusSoakingParameter();
                    base.UpdateStatusSoakingConfig();
                    ODValueChanged?.Invoke(this, value);
                }
            }
        }

        private bool _EnableWaitingPinAlign = false;
        public bool EnableWaitingPinAlign
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

        private int _WaitingPinAlignPeriod = 1800;
        public int WaitingPinAlignPeriod
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

        private string _TooltipODMax;
        public string TooltipODMax
        {
            get { return _TooltipODMax; }
            set
            {
                if (value != _TooltipODMax)
                {
                    _TooltipODMax = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region Method
        public override EventCodeEnum ReadStatusSoakinConfigParameter(StatusSoakingConfig statusSoakingParam)
        {
            Action<StatusSoakingCommonParam> SetSoakingParam = (StatusSoakingCommonParam commonParam) =>
            {
                NotExistWaferObj_OD = commonParam.NotExistWaferObj_OD.Value;
                EnableWaitingPinAlign = commonParam.EnableWaitingPinAlign.Value;
                WaitingPinAlignPeriod = commonParam.WaitingPinAlignPeriod.Value;
            };

            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                if (statusSoakingParam == null)
                {
                    return ret;
                }

                UpdateTooltip();

                StatusSoakingCommonParam commonParam = null;
                switch (soakingState)
                {
                    case SoakingStateEnum.PREPARE:
                        commonParam = statusSoakingParam.PrepareStateConfig;
                        break;
                    case SoakingStateEnum.RECOVERY:
                        commonParam = statusSoakingParam.RecoveryStateConfig;
                        break;
                    case SoakingStateEnum.MAINTAIN:
                        commonParam = statusSoakingParam.MaintainStateConfig;
                        break;
                    case SoakingStateEnum.MANUAL:
                        commonParam = statusSoakingParam.ManualSoakingConfig;
                        break;
                }

                if (null != commonParam)
                {
                    if (dispatcher.CheckAccess())
                    {
                        SetSoakingParam(commonParam);
                    }
                    else
                    {
                        dispatcher.Invoke(() =>
                        {
                            SetSoakingParam(commonParam);
                        });
                    }

                    ret = EventCodeEnum.NONE;
                }
                else
                {
                    ret = EventCodeEnum.UNDEFINED;
                }
                
            }
            catch (Exception ex)
            {
                ret = EventCodeEnum.UNDEFINED;
                LoggerManager.SoakingErrLog($"{ex.Message}");
            }

            return ret;
        }

        public override EventCodeEnum SaveStatusSoakingConfigParameter(ref StatusSoakingConfig statusSoakingParam)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                if (statusSoakingParam == null)
                {
                    return ret;
                }

                UpdateTooltip();

                StatusSoakingCommonParam commonParam = null;
                switch (soakingState)
                {
                    case SoakingStateEnum.PREPARE:
                        commonParam = statusSoakingParam.PrepareStateConfig;
                        break;
                    case SoakingStateEnum.RECOVERY:
                        commonParam = statusSoakingParam.RecoveryStateConfig;
                        break;
                    case SoakingStateEnum.MAINTAIN:
                        commonParam = statusSoakingParam.MaintainStateConfig;
                        break;
                    case SoakingStateEnum.MANUAL:
                        commonParam = statusSoakingParam.ManualSoakingConfig;
                        break;
                }

                if (null != commonParam)
                {
                    commonParam.NotExistWaferObj_OD.Value = NotExistWaferObj_OD;
                    commonParam.EnableWaitingPinAlign.Value = EnableWaitingPinAlign;
                    commonParam.WaitingPinAlignPeriod.Value = WaitingPinAlignPeriod;

                    ret = EventCodeEnum.NONE;
                }
                else
                {
                    ret = EventCodeEnum.UNDEFINED;
                }

            }
            catch (Exception ex)
            {
                ret = EventCodeEnum.UNDEFINED;
                LoggerManager.SoakingErrLog($"{ex.Message}");
            }

            return ret;
        }

        public void UpdateTooltip()
        {
            if (StatusSoakingParam == null)
            {
                return;
            }

            TooltipODMax = string.Format($"Maximum Value : {StatusSoakingParam.AdvancedSetting.ODLimit.Value}");
        }
        #endregion
    }
}