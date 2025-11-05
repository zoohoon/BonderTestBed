using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EdgeAdvanceSetup.ViewModel
{
    using LogModule;
    using ProberInterfaces;
    using ProberInterfaces.WaferAlignEX.Enum;
    using RelayCommandBase;
    using SerializerUtil;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using WA_EdgeParameter_Standard;

    public class EdgeStandardAdvanceSetupViewModel :  IFactoryModule, INotifyPropertyChanged, IPnpAdvanceSetupViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //..Property

        public IPnpAdvanceSetupView PnpAdvanceSetupView { get; set; }
        private WA_EdgeParam_Standard EdgeParam { get; set; }

        public static EnumWASubModuleEnable SEdgeMoementEnum { get; set; }

        private EnumWASubModuleEnable _EdgeMoementEnum;
        public EnumWASubModuleEnable EdgeMoementEnum
        {
            get { return _EdgeMoementEnum; }
            set
            {
                //if (value != _EdgeMoementEnum)
                //{
                _EdgeMoementEnum = value;
                RaisePropertyChanged();
                //}
            }
        }
        #endregion

        public EdgeStandardAdvanceSetupViewModel()
        {
            try
            {
                //if (edgeParam == null)
                //    return;
                //this.EdgeParam = edgeParam;
                //EdgeMoementEnum = edgeParam.EdgeMovement.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void SettingData(WA_EdgeParam_Standard edgeParam)
        {
            try
            {
                this.EdgeParam = edgeParam;
                EdgeMoementEnum = edgeParam.EdgeMovement.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        #region //.. Command 

        private AsyncCommand _CmdOKClick;
        public ICommand CmdOKClick
        {
            get
            {
                if (null == _CmdOKClick) _CmdOKClick = new AsyncCommand(CmdOKClickFunc);
                return _CmdOKClick;
            }
        }
        private async Task CmdOKClickFunc()
        {
            try
            {
                EdgeParam.EdgeMovement.Value = EdgeMoementEnum;
                await this.PnPManager().ClosePnpAdavanceSetupWindow();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }


        private AsyncCommand _CmdCancelClick;
        public ICommand CmdCancelClick
        {
            get
            {
                if (null == _CmdCancelClick) _CmdCancelClick = new AsyncCommand(CmdCancelClickFunc);
                return _CmdCancelClick;
            }
        }
        private async Task CmdCancelClickFunc()
        {
            try
            {
                await this.PnPManager().ClosePnpAdavanceSetupWindow();
                EdgeMoementEnum = EdgeParam.EdgeMovement.Value;
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
                        SerializeManager.DeserializeFromByte(param, out target, typeof(WA_EdgeParam_Standard));
                        if (target != null)
                        {
                            SettingData(target as WA_EdgeParam_Standard);
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
                if (EdgeParam != null)
                    parameters.Add(SerializeManager.SerializeToByte(EdgeParam));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return parameters;
        }

        #endregion

        public void Init()
        {
            return;
        }
    }

    public class EdgeStandardAdSettingInfo
    {
        
    }
}
