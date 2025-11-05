using LogModule;
using Newtonsoft.Json;
using PMIModuleParameter;
using PMISetup.UC;
using PnPControl;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.PMI;
using ProberInterfaces.PnpSetup;
using ProberInterfaces.State;
using SerializerUtil;
using SubstrateObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace PMISetup
{
    public class PMIParameterSettingModule : PNPSetupBase, ITemplateModule, INotifyPropertyChanged, ISetup, IPackagable
    {
        public override Guid ScreenGUID { get; } = new Guid("61EFD80F-0029-BEE1-125F-2F1112A42C4D");
        #region ==> PropertyChanged
        public new event PropertyChangedEventHandler PropertyChanged;

        protected new void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public override bool Initialized { get; set; } = false;

        public new string Genealogy { get; set; }

        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public new Object Owner
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


        public new List<object> Nodes { get; set; }

        public PMIParameterSettingModule()
        {
        }

        //public PMIModuleDevParam PMIDevParam;
        //public PMIModuleSysParam PMISysParam;

        private PMIParameterSetupControlService ParameterSetupControl;

        private IWaferObject Wafer
        {
            get { return this.StageSupervisor().WaferObject; }
        }

        private new IProbeCard ProbeCard { get { return this.GetParam_ProbeCard(); } }

        public EventCodeEnum DoRecovery() // Recovery때 하는 코드
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            return retVal;
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
        public new void DeInitModule()
        {
        }

        public override Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //Pnp를 사용한다면 ListView에 띄워줄 이름을 기재해 주어야한다.
                Header = "PMI Parameter Setup";

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

                MiniViewSwapVisibility = Visibility.Hidden;
                MiniViewZoomVisibility = Visibility.Hidden;
                LightJogVisibility = Visibility.Hidden;
                MainViewZoomVisibility = Visibility.Hidden;
                MotionJogVisibility = Visibility.Hidden;

                ParameterSetupControl = new PMIParameterSetupControlService();

                AdvanceSetupView = ParameterSetupControl.DialogControl;
                AdvanceSetupViewModel = ParameterSetupControl;

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
                bool IsSuccess = false;

                PackagableParams = datas;

                if (PackagableParams != null)
                {
                    if (PackagableParams.Count == 2)
                    {
                        for (int i = 0; i < PackagableParams.Count; i++)
                        {
                            if (i == 0)
                            {
                                object target;
                                SerializeManager.DeserializeFromByte(PackagableParams[i], out target, typeof(PMIModuleDevParam));
                                if (target != null)
                                {
                                    this.PMIModule().PMIModuleDevParam_IParam = (PMIModuleDevParam)target;
                                    (AdvanceSetupViewModel as PMIParameterSetupControlService).PMIDevParm = (PMIModuleDevParam)this.PMIModule().PMIModuleDevParam_IParam;
                                }
                            }
                            else if (i == 1)
                            {
                                object target;
                                SerializeManager.DeserializeFromByte(PackagableParams[i], out target, typeof(PMIInfo));

                                if (target != null)
                                {
                                    IPMIInfo pmiinfo = target as PMIInfo;

                                    if (pmiinfo != null)
                                    {
                                        this.StageSupervisor().WaferObject.GetSubsInfo().SetPMIInfo((IPMIInfo)target);
                                        (AdvanceSetupViewModel as PMIParameterSetupControlService).PMIInfo = (PMIInfo)this.StageSupervisor().WaferObject.GetSubsInfo().GetPMIInfo();

                                        IsSuccess = true;
                                    }
                                    else
                                    {
                                        LoggerManager.Error("[PMIParameterSettingModule] ApplyParams() : Target casting error. target is not PMIInfo.");
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        LoggerManager.Error("[PMIParameterSettingModule] ApplyParams() : PackagableParams count is wrong.");

                    }
                }
                else
                {
                    LoggerManager.Error("[PMIParameterSettingModule] ApplyParams() : PackagableParams is empty.");
                }

                //foreach (var param in datas)
                //{
                //    object target;
                //    SerializeManager.DeserializeFromByte(param, out target, typeof(PMIModuleDevParam));
                //    if (target != null)
                //    {
                //        this.PMIModule().PMIModuleDevParam_IParam = (PMIModuleDevParam)target;
                //        (AdvanceSetupViewModel as PMIParameterSetupControlService).PMIDevParm = (PMIModuleDevParam)this.PMIModule().PMIModuleDevParam_IParam;
                //        break;
                //    }
                //}

                if (IsSuccess == true)
                {
                    this.PMIModule().SaveDevParameter();
                    this.StageSupervisor().SaveWaferObject();
                }
                else
                {
                    LoggerManager.Error("[PMIParameterSettingModule] ApplyParams() : Save procedure is not work.");
                }
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

                PackagableParams.Add(SerializeManager.SerializeToByte(this.PMIModule().PMIModuleDevParam_IParam));
                PackagableParams.Add(SerializeManager.SerializeToByte(this.StageSupervisor().WaferObject.GetSubsInfo().GetPMIInfo()));
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

        /// <summary>
        /// Setup시에 설정할데이터 화면등을 정의.
        /// </summary>
        /// <returns></returns>
        public Task<EventCodeEnum> InitSetup()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                UseUserControl = UserControlFucEnum.DEFAULT;

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public override async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            //EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                this.PMIModule().SaveDevParameter();
                retVal = await base.Cleanup(parameter);

                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[PMIParameterSettingModule] [Cleanup()] : Error");
                }
                return retVal;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                return retVal;
            }
        }

        public override bool IsParameterChanged(bool issave = false)
        {
            bool retVal = false;

            try
            {
                //if (Extensions_IParam.ElementStateDefaultValidation(PMIDevParam) == EventCodeEnum.NONE)
                //{
                //    retVal = false;
                //}
                //else
                //{
                //    retVal = true;
                //}
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

        #region //..Command Method
        #endregion

    }
}
