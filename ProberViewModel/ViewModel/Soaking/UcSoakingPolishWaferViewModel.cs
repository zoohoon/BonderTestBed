using System;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using Autofac;
using LoaderBase;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using SoakingParameters;

namespace ProberViewModel
{
    public class SelectableObject<T>
    {
        public bool IsSelected { get; set; }
        public T ObjectData { get; set; }

        public SelectableObject(T objectData)
        {
            ObjectData = objectData;
        }

        public SelectableObject(T objectData, bool isSelected)
        {
            IsSelected = isSelected;
            ObjectData = objectData;
        }
    }

    public class UcSoakingPolishWaferViewModel : UcStatusSoakingViewModelBase, IMainScreenViewModel
    {
        private readonly Guid _ViewModelGUID = new Guid("9B1AEE36-1D62-4B9B-B4C7-DF5C7C4CF84C");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        private readonly Dispatcher dispatcher;

        public UcSoakingPolishWaferViewModel()
        {
            dispatcher = Dispatcher.CurrentDispatcher;
        }

        #region Property

        private bool _UsePolishWafer;
        public bool UsePolishWafer
        {
            get
            {
                return _UsePolishWafer;
            }
            set
            {
                if (value != _UsePolishWafer)
                {
                    _UsePolishWafer = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _EnableEdgeDetection;
        public bool EnableEdgeDetection
        {
            get
            {
                return _EnableEdgeDetection;
            }
            set
            {
                if (value != _EnableEdgeDetection)
                {
                    _EnableEdgeDetection = value;
                    RaisePropertyChanged();
                }
            }
        }
        
        private string _SelectedPolishWafer;
        public string SelectedPolishWafer
        {
            get
            {
                return _SelectedPolishWafer;
            }
            set
            {
                if (value != _SelectedPolishWafer)
                {
                    _SelectedPolishWafer = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ObservableCollection<SelectableObject<string>> SelectedPolishWaferList { get; } = new ObservableCollection<SelectableObject<string>>();
        #endregion

        #region Method
        public override EventCodeEnum ReadStatusSoakinConfigParameter(StatusSoakingConfig statusSoakingParam)
        {
            Action<StatusSoakingCommonParam> SetSelectedPolishWaferInfo = (StatusSoakingCommonParam statusSoakingConfig) =>
            {
                UsePolishWafer = statusSoakingConfig.UsePolishWafer.Value;
                EnableEdgeDetection = statusSoakingConfig.EnableEdgeDetection.Value;

                SelectedPolishWaferList.Clear();
                foreach (var polishWaferInfos in this.DeviceManager().GetPolishWaferSources())
                {
                    var matchedObj = statusSoakingConfig.SelectedPolishwafer.FirstOrDefault(item => item.PolishWaferName.Value == polishWaferInfos.DefineName.Value);
                    if (matchedObj == null)
                    {
                        SelectedPolishWaferList.Add(new SelectableObject<string>(polishWaferInfos.DefineName.Value, false));
                    }
                    else
                    {
                        SelectedPolishWaferList.Add(new SelectableObject<string>(polishWaferInfos.DefineName.Value, true));
                    }
                }

                StringBuilder sb = new StringBuilder();
                foreach (SelectableObject<string> cbObject in SelectedPolishWaferList)
                {
                    if (cbObject.IsSelected)
                        sb.AppendFormat("{0}, ", cbObject.ObjectData.ToString());
                }

                SelectedPolishWafer = sb.ToString().Trim().TrimEnd(',');
            };

            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Remote)
                {
                    var StateMap = this.GetLoaderContainer().Resolve<ILoaderModule>()?.GetLoaderInfo()?.StateMap;
                    if (StateMap == null)
                    {
                        return EventCodeEnum.UNDEFINED;
                    }
                }

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
                        SetSelectedPolishWaferInfo(commonParam);
                    }
                    else
                    {
                        dispatcher.Invoke(() =>
                        {
                            SetSelectedPolishWaferInfo(commonParam);
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
                if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Remote)
                {
                    var StateMap = this.GetLoaderContainer().Resolve<ILoaderModule>()?.GetLoaderInfo()?.StateMap;
                    if (StateMap == null)
                    {
                        return EventCodeEnum.UNDEFINED;
                    }
                }

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
                    commonParam.UsePolishWafer.Value = UsePolishWafer;
                    commonParam.EnableEdgeDetection.Value = EnableEdgeDetection;

                    commonParam.SelectedPolishwafer.Clear();
                    foreach (var item in SelectedPolishWaferList)
                    {
                        if (item.IsSelected)
                        {
                            commonParam.SelectedPolishwafer.Add(new Selectedpolishwafer(item.ObjectData));
                        }
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
        #endregion
    }
}