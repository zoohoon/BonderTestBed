using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HighAlignKeyProcAdvanceSetup
{
    using LogModule;
    using ProbeCardObject;
    using ProberInterfaces;
    using RelayCommandBase;
    using SerializerUtil;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    public class HighAlignKeyProcAdvanceSetupViewModel : IFactoryModule, INotifyPropertyChanged, IPnpAdvanceSetupViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //..Property
        private PinAlignDevParameters pinParam;

        public static EnumThresholdType ThreshType;

        private EnumThresholdType _ThreshTypeEnum;
        public EnumThresholdType ThreshTypeEnum
        {
            get { return _ThreshTypeEnum; }
            set
            {
                if (value != _ThreshTypeEnum)
                {
                    _ThreshTypeEnum = value;
                    ThreshType = _ThreshTypeEnum;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region //..Command & Method

        public void SettingData(PinAlignDevParameters pin_DevParam)
        {
            try
            {
                this.pinParam = pin_DevParam;
                ThreshTypeEnum = pin_DevParam?.EnableAutoThreshold.Value ?? EnumThresholdType.AUTO;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        //private AsyncCommand _CmdMaunalInputContolOKClick;
        //public ICommand CmdMaunalInputContolOKClick
        //{
        //    get
        //    {
        //        if (null == _CmdMaunalInputContolOKClick) _CmdMaunalInputContolOKClick
        //                = new AsyncCommand(MaunalInputContolOKClick);
        //        return _CmdMaunalInputContolOKClick;
        //    }
        //}

        //private async Task MaunalInputContolOKClick()
        //{
        //    try
        //    {
        //        this.pinParam.EnableAutoThreshold.Value = ThreshTypeEnum;
        //        await this.PnPManager().ClosePnpAdavanceSetupWindow().ConfigureAwait(false);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //}

        private AsyncCommand _CmdMaunalInputContolOKClick;
        public ICommand CmdMaunalInputContolOKClick
        {
            get
            {
                if (null == _CmdMaunalInputContolOKClick) _CmdMaunalInputContolOKClick = new AsyncCommand(MaunalInputContolOKClick);
                return _CmdMaunalInputContolOKClick;
            }
        }

        private async Task MaunalInputContolOKClick()
        {
            try
            {
                this.pinParam.EnableAutoThreshold.Value = ThreshTypeEnum;
                await this.PnPManager().ClosePnpAdavanceSetupWindow();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        //private AsyncCommand _CmdMaunalInputContolCancelClick;
        //public ICommand CmdMaunalInputContolCancelClick
        //{
        //    get
        //    {
        //        if (null == _CmdMaunalInputContolCancelClick) _CmdMaunalInputContolCancelClick
        //                = new AsyncCommand(MaunalInputContolCancelClick);
        //        return _CmdMaunalInputContolCancelClick;
        //    }
        //}

        //private async Task MaunalInputContolCancelClick()
        //{
        //    try
        //    {
        //        ThreshTypeEnum = this.pinParam?.EnableAutoThreshold.Value ?? EnumThresholdType.AUTO;
        //        await this.PnPManager().ClosePnpAdavanceSetupWindow().ConfigureAwait(false);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //}

        private AsyncCommand _CmdMaunalInputContolCancelClick;
        public ICommand CmdMaunalInputContolCancelClick
        {
            get
            {
                if (null == _CmdMaunalInputContolCancelClick) _CmdMaunalInputContolCancelClick = new AsyncCommand(MaunalInputContolCancelClick);
                return _CmdMaunalInputContolCancelClick;
            }
        }

        private async Task MaunalInputContolCancelClick()
        {
            try
            {
                ThreshTypeEnum = this.pinParam?.EnableAutoThreshold.Value ?? EnumThresholdType.AUTO;
                await this.PnPManager().ClosePnpAdavanceSetupWindow();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        #endregion

        #region //.. IPnpAdvanceSetupViewModel Method

        public void SetParameters(List<byte[]> datas)
        {
            try
            {
                if (datas != null)
                {
                    foreach (var param in datas)
                    {
                        object target;
                        SerializeManager.DeserializeFromByte(param, out target, typeof(PinAlignDevParameters));
                        if (target != null)
                        {
                            SettingData(target as PinAlignDevParameters);
                            break;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public List<byte[]> GetParameters()
        {
            List<byte[]> parameters = new List<byte[]>();
            try
            {
                if (pinParam != null)
                    parameters.Add(SerializeManager.SerializeToByte(pinParam));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return parameters;
        }

        public void Init()
        {
            return;
        }
        #endregion

    }
}
