using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Soaking;
using SoakingParameters;

namespace ProberViewModel
{
    public class UcSoakingEventSoakViewModel : UcStatusSoakingViewModelBase, IMainScreenViewModel
    {
        private readonly Guid _ViewModelGUID = new Guid("C3D542E5-F279-4B47-A18C-3F6C15922BF1");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        private readonly Dispatcher dispatcher;

        public UcSoakingEventSoakViewModel()
        {
            dispatcher = Dispatcher.CurrentDispatcher;

            foreach (var mode in Enum.GetValues(typeof(PinAlignType)))
            {
                PinAlignModeList.Add((PinAlignType)mode);
            }
        }

        #region Property
        private EventSoakType _EventSoakMode = EventSoakType.None;
        public EventSoakType EventSoakMode
        {
            get { return _EventSoakMode; }
            set
            {
                if (value != _EventSoakMode)
                {
                    _EventSoakMode = value;
                    if (_EventSoakMode == EventSoakType.BeforeZUpSoak)
                    {
                        VisiblePinAlignMode = Visibility.Hidden;
                        VisibleSoakingSkip = Visibility.Hidden;
                    }
                    else
                    {
                        VisiblePinAlignMode = Visibility.Visible;
                        VisibleSoakingSkip = Visibility.Visible;
                    }
                }
            }
        }
        
        private bool _IsEnableSoaking;
        public bool IsEnableSoaking
        {
            get
            {
                return _IsEnableSoaking;
            }
            set
            {
                if (value != _IsEnableSoaking)
                {
                    _IsEnableSoaking = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _SoakingTime;
        public int SoakingTime
        {
            get
            {
                return _SoakingTime;
            }
            set
            {
                if (value != _SoakingTime)
                {
                    _SoakingTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsEnableSoakingSkip = false;
        public bool IsEnableSoakingSkip
        {
            get
            {
                return _IsEnableSoakingSkip;
            }
            set
            {
                if (value != _IsEnableSoakingSkip)
                {
                    _IsEnableSoakingSkip = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Chilling Time이 설정 된 시간 보다 큰 경우 트리거 됨.
        /// Chilling Time이 설정 된 시간 보다 작은 경우 스킵 됨.
        /// </summary>
        private int _SoakingSkipTime;
        public int SoakingSkipTime
        {
            get
            {
                return _SoakingSkipTime;
            }
            set
            {
                if (value != _SoakingSkipTime)
                {
                    _SoakingSkipTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _OD;
        public double OD
        {
            get
            {
                return _OD;
            }
            set
            {
                if (value != _OD)
                {
                    _OD = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<PinAlignType> _PinAlignModeList = new ObservableCollection<PinAlignType>();
        public ObservableCollection<PinAlignType> PinAlignModeList
        {
            get
            {
                return _PinAlignModeList;
            }
            set
            {
                if (value != _PinAlignModeList)
                {
                    _PinAlignModeList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PinAlignType _SelectedPinAlignMode;
        public PinAlignType SelectedPinAlignMode
        {
            get
            {
                return _SelectedPinAlignMode;
            }
            set
            {
                if (value != _SelectedPinAlignMode)
                {
                    _SelectedPinAlignMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _VisibleSoakingSkip;
        public Visibility VisibleSoakingSkip
        {
            get
            {
                return _VisibleSoakingSkip;
            }
            set
            {
                if (value != _VisibleSoakingSkip)
                {
                    _VisibleSoakingSkip = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _VisiblePinAlignMode;
        public Visibility VisiblePinAlignMode
        {
            get
            {
                return _VisiblePinAlignMode;
            }
            set
            {
                if (value != _VisiblePinAlignMode)
                {
                    _VisiblePinAlignMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _TooltipSoakingTime;
        public string TooltipSoakingTime
        {
            get { return _TooltipSoakingTime; }
            set
            {
                if (value != _TooltipSoakingTime)
                {
                    _TooltipSoakingTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _TooltipIsEnableSoakingSkip;
        public string TooltipIsEnableSoakingSkip
        {
            get { return _TooltipIsEnableSoakingSkip; }
            set
            {
                if (value != _TooltipIsEnableSoakingSkip)
                {
                    _TooltipIsEnableSoakingSkip = value;
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
            Action<StatusSoakingEvent> SetEventSoakingParam = (StatusSoakingEvent statusSoakingEventParam) =>
            {
                IsEnableSoaking = statusSoakingEventParam.UseEventSoaking.Value;
                SoakingTime = statusSoakingEventParam.SoakingTimeSec.Value;

                IsEnableSoakingSkip = statusSoakingEventParam.UseEventSoakingSkip.Value;

                OD = statusSoakingEventParam.OD_Value.Value;
                SelectedPinAlignMode = statusSoakingEventParam.PinAlignMode.Value;
            };

            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                if (statusSoakingParam == null)
                {
                    return ret;
                }

                UpdateTooltip();

                var statusSoakingEventParamList = statusSoakingParam.GetEventStatusCommonParam();
                StatusSoakingEvent statusSoakingEventParam = null;
                foreach (var eventParam in statusSoakingEventParamList)
                {
                    if (EventSoakMode != (EventSoakType)Enum.Parse(typeof(EventSoakType), eventParam.StatusSoakingEventName.Value))
                    {
                        continue;
                    }

                    statusSoakingEventParam = eventParam;
                }

                if (null != statusSoakingEventParam)
                {
                    if (dispatcher.CheckAccess())
                    {
                        SetEventSoakingParam(statusSoakingEventParam);
                    }
                    else
                    {
                        dispatcher.Invoke(() =>
                        {
                            SetEventSoakingParam(statusSoakingEventParam);
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

                var statusSoakingEventParamList = statusSoakingParam.GetEventStatusCommonParam();
                StatusSoakingEvent statusSoakingEventParam = null;
                foreach (var eventParam in statusSoakingEventParamList)
                {
                    if (EventSoakMode != (EventSoakType)Enum.Parse(typeof(EventSoakType), eventParam.StatusSoakingEventName.Value))
                    {
                        continue;
                    }

                    statusSoakingEventParam = eventParam;
                }

                if (null != statusSoakingEventParam)
                {
                    statusSoakingEventParam.UseEventSoaking.Value = IsEnableSoaking;
                    statusSoakingEventParam.SoakingTimeSec.Value = SoakingTime;

                    statusSoakingEventParam.UseEventSoakingSkip.Value = IsEnableSoakingSkip;

                    statusSoakingEventParam.OD_Value.Value = OD;
                    statusSoakingEventParam.PinAlignMode.Value = SelectedPinAlignMode;

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

            TooltipSoakingTime = string.Format($"Minimum Value : 1");
            TooltipIsEnableSoakingSkip = string.Format($"(* After the wafer is loaded, if the soaking operation is completed, it is skipped.)");
            TooltipODMax = string.Format($"Maximum Value : {StatusSoakingParam.AdvancedSetting.ODLimit.Value}");
        }
        #endregion
    }
}