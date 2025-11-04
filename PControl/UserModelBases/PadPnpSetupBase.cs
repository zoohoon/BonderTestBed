using System;

namespace PnPontrol.UserModelBases
{
    using PnPControl;
    using ProberErrorCode;
    using RelayCommandBase;
    using System.Windows.Input;
    using SubstrateObjects;
    using ProberInterfaces;
    using LogModule;

    public abstract class PadPnpSetupBase : PNPSetupBase
    {
        private WaferObject Wafer { get { return (WaferObject)this.StageSupervisor().WaferObject; } }

        private RelayCommand _ShowUCWADeviceSettingCommand;
        public ICommand ShowUCWADeviceSettingCommand
        {
            get
            {
                if (null == _ShowUCWADeviceSettingCommand) _ShowUCWADeviceSettingCommand = new RelayCommand(
                    ShowUCWADeviceSetting);
                return _ShowUCWADeviceSettingCommand;
            }
        }

        private void ShowUCWADeviceSetting()
        {
            try
            {

                //TargetRectangleWidth = Wafer.GetSubsInfo().Pads.SetPadObject.PadSizeX.Value / CurCam.GetRatioX();
                //TargetRectangleHeight = Wafer.GetSubsInfo().Pads.SetPadObject.PadSizeY.Value / CurCam.GetRatioX();

            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "PadPnpSetupBase- ShowUCWADeviceSetting() : Error occurred.");
                LoggerManager.Exception(err);
            }

        }

        public EventCodeEnum InitCommonUI()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                PadJogRightDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/DeviceSetting.png");
                PadJogRightDown.Command = ShowUCWADeviceSettingCommand;
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "PadPnpSetupBase - InitCommonUI() :  Error occurred.");
                LoggerManager.Exception(err);
            }
            return retVal;
        }

    }
}
