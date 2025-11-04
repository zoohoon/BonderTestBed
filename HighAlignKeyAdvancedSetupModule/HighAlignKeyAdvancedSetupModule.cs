using LogModule;
using PnPControl;
using ProbeCardObject;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.PinAlign.ProbeCardData;
using ProberInterfaces.PnpSetup;
using ProberInterfaces.State;
using RelayCommandBase;
using SerializerUtil;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace HighAlignKeyAdvancedSetupModule
{
    public class HighAlignKeyAdvancedSetupModule : PNPSetupBase, ITemplateModule, INotifyPropertyChanged, ISetup, IPackagable
    {
        public override Guid ScreenGUID { get; } = new Guid("728d75f2-b66f-4fc1-90e2-dd174ff9452b");

        public override bool Initialized { get; set; } = false;

        private HighAlignKeyAdvancedSetupControlService ParameterSetupControl;

        public HighAlignKeyAdvancedSetupModule()
        {

        }

        public override EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    //Template을 아직 사용하지 않으므로 코드에서 적용.
                    SetEnableState(EnumEnableState.ENABLE);

                    Initialized = true;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public override void DeInitModule()
        {
        }

        public override Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //Pnp를 사용한다면 ListView에 띄워줄 이름을 기재해 주어야한다.
                Header = "Advanced Setup";

                //InitPnpModuleStage();
                //InitLightJog(this, EnumProberCam.WAFER_HIGH_CAM);

                PnpManager = this.PnPManager();

                SetupState = new NotCompletedState(this);

                SetNodeSetupState(EnumMoudleSetupState.NONE);

                #region //..PNP UI setup
                StepLabel = null;

                OneButton.IconSource = null;
                TwoButton.IconSource = null;
                ThreeButton.IconSource = null;
                FourButton.IconSource = null;
                FiveButton.IconSource = null;

                OneButton.Command = null;
                TwoButton.Command = null;
                ThreeButton.Command = null;
                FourButton.Command = null;
                FiveButton.Command = null;

                OneButton.IconSource = null;
                TwoButton.IconSource = null;
                ThreeButton.IconSource = null;
                FourButton.IconSource = null;
                FiveButton.IconSource = null;

                OneButton.IsEnabled = false;
                TwoButton.IsEnabled = false;
                ThreeButton.IsEnabled = false;
                FourButton.IsEnabled = false;
                FiveButton.IsEnabled = false;

                PadJogLeftUp.Caption = null;
                PadJogRightUp.Caption = null;
                PadJogLeftDown.Caption = null;
                PadJogRightDown.Caption = null;

                PadJogRightUp.Command = null;
                PadJogLeftUp.Command = null;
                PadJogLeftDown.Command = null;
                PadJogRightDown.Command = null;

                PadJogLeftUp.IsEnabled = false;
                PadJogRightUp.IsEnabled = false;
                PadJogLeftDown.IsEnabled = false;
                PadJogRightDown.IsEnabled = false;

                PadJogSelect.Caption = null;
                PadJogLeft.IconSource = null;
                PadJogRight.IconSource = null;
                PadJogUp.IconSource = null;
                PadJogDown.IconSource = null;

                PadJogSelect.Command = null;
                PadJogLeft.Command = null;
                PadJogRight.Command = null;
                PadJogUp.Command = null;
                PadJogDown.Command = null;

                PadJogSelect.IsEnabled = false;
                PadJogLeft.IsEnabled = false;
                PadJogRight.IsEnabled = false;
                PadJogUp.IsEnabled = false;
                PadJogDown.IsEnabled = false;
                #endregion

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.INITVIEWMODEL_EXCEPTION;

                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public void ApplyParams(List<byte[]> datas)
        {
            try
            {
                if (datas.Count == 2)
                {
                    for (int i = 0; i < datas.Count; i++)
                    {
                        object target;

                        if (i == 0)
                        {
                            SerializeManager.DeserializeFromByte(datas[i], out target, typeof(PinAlignDevParameters));

                            if (target != null)
                            {
                                this.PinAligner().PinAlignDevParam = (PinAlignDevParameters)target;
                            }
                        }
                        else if (i == 1)
                        {
                            SerializeManager.DeserializeFromByte(datas[i], out target, typeof(WrapperDutlist));

                            if (target != null)
                            {
                                WrapperDutlist wrapperdutlist = (WrapperDutlist)target;

                                this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList = wrapperdutlist.DutList;
                                this.StageSupervisor().SaveProberCard();
                                this.StageSupervisor().LoadProberCard();
                            }
                        }
                    }
                }
                else
                {
                    LoggerManager.Error("[HighAlignKeyAdvancedSetupModule] ApplyParams() : Input data is incorrect.");
                }

                this.PinAligner().SaveDevParameter();
                this.PinAligner().CollectElement();

                this.StageSupervisor().SaveProberCard();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void SetPackagableParams()
        {
            try
            {
                PackagableParams.Clear();

                PackagableParams.Add(SerializeManager.SerializeToByte(this.PinAligner().PinAlignDevParam));

                WrapperDutlist wrapperdutlist = new WrapperDutlist();

                List<IDut> tmpdutlist = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.ToList();

                wrapperdutlist.DutList = new AsyncObservableCollection<IDut>(tmpdutlist);

                PackagableParams.Add(SerializeManager.SerializeToByte(wrapperdutlist));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = await InitSetup();

                await ShowAdvanceSetupView();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.PAGE_SWITCHED_EXCEPTION;
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public Task<EventCodeEnum> InitSetup()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                UseUserControl = UserControlFucEnum.DEFAULT;
                retVal = InitPNPSetupUI();
                ParameterSetupControl = new HighAlignKeyAdvancedSetupControlService();
                AdvanceSetupView = ParameterSetupControl.DialogControl;
                AdvanceSetupViewModel = ParameterSetupControl;

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retVal);
        }

        private EventCodeEnum InitPNPSetupUI()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                PadJogSelect.IsEnabled = true;

                PadJogSelect.Caption = "Show";
                PadJogSelect.Command = new AsyncCommand(Show, false);

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        private async Task Show()
        {
            try
            {
                await ShowAdvanceSetupView();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = this.PinAligner().SaveDevParameter();
                retVal = this.StageSupervisor().SaveProberCard();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public override bool IsParameterChanged(bool issave = false)
        {
            bool retVal = false;

            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override void SetStepSetupState(string header = null)
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void UpdateLabel()
        {
            throw new NotImplementedException();
        }

    }
}
