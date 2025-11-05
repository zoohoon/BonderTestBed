using MahApps.Metro.Controls.Dialogs;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PinHighAlignModule.UC
{
    using Autofac;
    using ProberInterfaces;
    using ProberInterfaces.Enum;
    using ProberInterfaces.Error;
    using ProberInterfaces.WaferAlignEX;
    using ProberInterfaces.AlignEX;
    using System.IO;
    using ParamHelper;
    using ProberInterfaces.Param;
    using System.Windows;
    using RelayCommandBase;
    using System.Windows.Input;
    using ProberInterfaces.WaferAlign;
    using System.Collections.ObjectModel;
    using ProberInterfaces.PnpSetup;
    using System.Threading;    
    using PnPControl;
    using ProberInterfaces.Align;
    using ProberErrorCode;    
    using PnPontrol.UserModelBases;    
    using ProberInterfaces.WaferAlignEX.Enum;
    using LogModule;
    using ProberInterfaces.State;
    using global::PinHighAlignModule.UC;
    using MahApps.Metro.Controls;
    using VirtualKeyboardControl;
    using Newtonsoft.Json;
    using System.Xml.Serialization;


    /// <summary>
    /// PinAlignSetting.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PinAlignSetting : CustomDialog, IFactoryModule, IParamNode, INotifyPropertyChanged
    {

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
        public List<object> Nodes { get; set; }
        = new List<object>();

        public PinAlignSetting()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        public void Init()
        {
            try
            {
            /*lbWI_PinCnt.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[0].PinCount.Value.ToString();
            lbWI_EachPinTolX.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[0].EachPinTolerenceX.Value.ToString();
            lbWI_EachPinTolY.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[0].EachPinTolerenceY.Value.ToString();
            lbWI_EachPinTolZ.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[0].EachPinTolerenceZ.Value.ToString();
            lbWI_CardTolX.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[0].CardCenterTolerenceX.Value.ToString();
            lbWI_CardTolY.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[0].CardCenterTolerenceY.Value.ToString();
            lbWI_CardTolZ.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[0].CardCenterTolerenceZ.Value.ToString();
            lbWI_MinMaxZ.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[0].MinMaxZDiffLimit.Value.ToString();
            lbWI_SamplePinCnt.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[0].SamplePinAlignmentPinCount.Value.ToString();
            lbWI_SamplePinTolX.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[0].SamplePinTolerenceX.Value.ToString();
            lbWI_SamplePinTolY.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[0].SamplePinTolerenceY.Value.ToString();
            lbWI_SamplePinTolZ.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[0].SamplePinTolerenceZ.Value.ToString();
            lbWI_FailPercent.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[0].FailureDetermineThreshold.Value.ToString();

            lbDI_PinCnt.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[1].PinCount.Value.ToString();
            lbDI_EachPinTolX.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[1].EachPinTolerenceX.Value.ToString();
            lbDI_EachPinTolY.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[1].EachPinTolerenceY.Value.ToString();
            lbDI_EachPinTolZ.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[1].EachPinTolerenceZ.Value.ToString();
            lbDI_CardTolX.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[1].CardCenterTolerenceX.Value.ToString();
            lbDI_CardTolY.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[1].CardCenterTolerenceY.Value.ToString();
            lbDI_CardTolZ.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[1].CardCenterTolerenceZ.Value.ToString();
            lbDI_MinMaxZ.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[1].MinMaxZDiffLimit.Value.ToString();
            lbDI_SamplePinCnt.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[1].SamplePinAlignmentPinCount.Value.ToString();
            lbDI_SamplePinTolX.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[1].SamplePinTolerenceX.Value.ToString();
            lbDI_SamplePinTolY.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[1].SamplePinTolerenceY.Value.ToString();
            lbDI_SamplePinTolZ.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[1].SamplePinTolerenceZ.Value.ToString();
            lbDI_FailPercent.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[1].FailureDetermineThreshold.Value.ToString();

            lbTI_PinCnt.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[2].PinCount.Value.ToString();
            lbTI_EachPinTolX.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[2].EachPinTolerenceX.Value.ToString();
            lbTI_EachPinTolY.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[2].EachPinTolerenceY.Value.ToString();
            lbTI_EachPinTolZ.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[2].EachPinTolerenceZ.Value.ToString();
            lbTI_CardTolX.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[2].CardCenterTolerenceX.Value.ToString();
            lbTI_CardTolY.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[2].CardCenterTolerenceY.Value.ToString();
            lbTI_CardTolZ.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[2].CardCenterTolerenceZ.Value.ToString();
            lbTI_MinMaxZ.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[2].MinMaxZDiffLimit.Value.ToString();
            lbTI_SamplePinCnt.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[2].SamplePinAlignmentPinCount.Value.ToString();
            lbTI_SamplePinTolX.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[2].SamplePinTolerenceX.Value.ToString();
            lbTI_SamplePinTolY.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[2].SamplePinTolerenceY.Value.ToString();
            lbTI_SamplePinTolZ.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[2].SamplePinTolerenceZ.Value.ToString();
            lbTI_FailPercent.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[2].FailureDetermineThreshold.Value.ToString();

            lbNC_PinCnt.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[3].PinCount.Value.ToString();
            lbNC_EachPinTolX.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[3].EachPinTolerenceX.Value.ToString();
            lbNC_EachPinTolY.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[3].EachPinTolerenceY.Value.ToString();
            lbNC_EachPinTolZ.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[3].EachPinTolerenceZ.Value.ToString();
            lbNC_CardTolX.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[3].CardCenterTolerenceX.Value.ToString();
            lbNC_CardTolY.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[3].CardCenterTolerenceY.Value.ToString();
            lbNC_CardTolZ.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[3].CardCenterTolerenceZ.Value.ToString();
            lbNC_MinMaxZ.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[3].MinMaxZDiffLimit.Value.ToString();
            lbNC_SamplePinCnt.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[3].SamplePinAlignmentPinCount.Value.ToString();
            lbNC_SamplePinTolX.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[3].SamplePinTolerenceX.Value.ToString();
            lbNC_SamplePinTolY.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[3].SamplePinTolerenceY.Value.ToString();
            lbNC_SamplePinTolZ.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[3].SamplePinTolerenceZ.Value.ToString();
            lbNC_FailPercent.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[3].FailureDetermineThreshold.Value.ToString();

            lbPW_PinCnt.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[4].PinCount.Value.ToString();
            lbPW_EachPinTolX.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[4].EachPinTolerenceX.Value.ToString();
            lbPW_EachPinTolY.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[4].EachPinTolerenceY.Value.ToString();
            lbPW_EachPinTolZ.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[4].EachPinTolerenceZ.Value.ToString();
            lbPW_CardTolX.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[4].CardCenterTolerenceX.Value.ToString();
            lbPW_CardTolY.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[4].CardCenterTolerenceY.Value.ToString();
            lbPW_CardTolZ.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[4].CardCenterTolerenceZ.Value.ToString();
            lbPW_MinMaxZ.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[4].MinMaxZDiffLimit.Value.ToString();
            lbPW_SamplePinCnt.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[4].SamplePinAlignmentPinCount.Value.ToString();
            lbPW_SamplePinTolX.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[4].SamplePinTolerenceX.Value.ToString();
            lbPW_SamplePinTolY.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[4].SamplePinTolerenceY.Value.ToString();
            lbPW_SamplePinTolZ.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[4].SamplePinTolerenceZ.Value.ToString();
            lbPW_FailPercent.Content = this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[4].FailureDetermineThreshold.Value.ToString();*/
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        private void bntSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                /*this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[0].PinCount.Value = Convert.ToInt32(lbWI_PinCnt.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[0].EachPinTolerenceX.Value = Convert.ToDouble(lbWI_EachPinTolX.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[0].EachPinTolerenceY.Value = Convert.ToDouble(lbWI_EachPinTolY.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[0].EachPinTolerenceZ.Value = Convert.ToDouble(lbWI_EachPinTolZ.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[0].CardCenterTolerenceX.Value = Convert.ToDouble(lbWI_CardTolX.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[0].CardCenterTolerenceY.Value = Convert.ToDouble(lbWI_CardTolY.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[0].CardCenterTolerenceZ.Value = Convert.ToDouble(lbWI_CardTolZ.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[0].MinMaxZDiffLimit.Value = Convert.ToInt32(lbWI_MinMaxZ.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[0].SamplePinAlignmentPinCount.Value = Convert.ToInt32(lbWI_SamplePinCnt.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[0].SamplePinTolerenceX.Value = Convert.ToDouble(lbWI_SamplePinTolX.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[0].SamplePinTolerenceY.Value = Convert.ToDouble(lbWI_SamplePinTolY.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[0].SamplePinTolerenceZ.Value = Convert.ToDouble(lbWI_SamplePinTolZ.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[0].FailureDetermineThreshold.Value = Convert.ToInt32(lbWI_FailPercent.Content);

                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[1].PinCount.Value = Convert.ToInt32(lbDI_PinCnt.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[1].EachPinTolerenceX.Value = Convert.ToDouble(lbDI_EachPinTolX.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[1].EachPinTolerenceY.Value = Convert.ToDouble(lbDI_EachPinTolY.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[1].EachPinTolerenceZ.Value = Convert.ToDouble(lbDI_EachPinTolZ.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[1].CardCenterTolerenceX.Value = Convert.ToDouble(lbDI_CardTolX.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[1].CardCenterTolerenceY.Value = Convert.ToDouble(lbDI_CardTolY.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[1].CardCenterTolerenceZ.Value = Convert.ToDouble(lbDI_CardTolZ.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[1].MinMaxZDiffLimit.Value = Convert.ToInt32(lbDI_MinMaxZ.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[1].SamplePinAlignmentPinCount.Value = Convert.ToInt32(lbDI_SamplePinCnt.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[1].SamplePinTolerenceX.Value = Convert.ToDouble(lbDI_SamplePinTolX.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[1].SamplePinTolerenceY.Value = Convert.ToDouble(lbDI_SamplePinTolY.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[1].SamplePinTolerenceZ.Value = Convert.ToDouble(lbDI_SamplePinTolZ.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[1].FailureDetermineThreshold.Value = Convert.ToInt32(lbDI_FailPercent.Content);

                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[2].PinCount.Value = Convert.ToInt32(lbTI_PinCnt.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[2].EachPinTolerenceX.Value = Convert.ToDouble(lbTI_EachPinTolX.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[2].EachPinTolerenceY.Value = Convert.ToDouble(lbTI_EachPinTolY.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[2].EachPinTolerenceZ.Value = Convert.ToDouble(lbTI_EachPinTolZ.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[2].CardCenterTolerenceX.Value = Convert.ToDouble(lbTI_CardTolX.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[2].CardCenterTolerenceY.Value = Convert.ToDouble(lbTI_CardTolY.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[2].CardCenterTolerenceZ.Value = Convert.ToDouble(lbTI_CardTolZ.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[2].MinMaxZDiffLimit.Value = Convert.ToInt32(lbTI_MinMaxZ.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[2].SamplePinAlignmentPinCount.Value = Convert.ToInt32(lbTI_SamplePinCnt.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[2].SamplePinTolerenceX.Value = Convert.ToDouble(lbTI_SamplePinTolX.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[2].SamplePinTolerenceY.Value = Convert.ToDouble(lbTI_SamplePinTolY.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[2].SamplePinTolerenceZ.Value = Convert.ToDouble(lbTI_SamplePinTolZ.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[2].FailureDetermineThreshold.Value = Convert.ToInt32(lbTI_FailPercent.Content);

                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[3].PinCount.Value = Convert.ToInt32(lbNC_PinCnt.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[3].EachPinTolerenceX.Value = Convert.ToDouble(lbNC_EachPinTolX.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[3].EachPinTolerenceY.Value = Convert.ToDouble(lbNC_EachPinTolY.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[3].EachPinTolerenceZ.Value = Convert.ToDouble(lbNC_EachPinTolZ.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[3].CardCenterTolerenceX.Value = Convert.ToDouble(lbNC_CardTolX.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[3].CardCenterTolerenceY.Value = Convert.ToDouble(lbNC_CardTolY.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[3].CardCenterTolerenceZ.Value = Convert.ToDouble(lbNC_CardTolZ.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[3].MinMaxZDiffLimit.Value = Convert.ToInt32(lbNC_MinMaxZ.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[3].SamplePinAlignmentPinCount.Value = Convert.ToInt32(lbNC_SamplePinCnt.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[3].SamplePinTolerenceX.Value = Convert.ToDouble(lbNC_SamplePinTolX.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[3].SamplePinTolerenceY.Value = Convert.ToDouble(lbNC_SamplePinTolY.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[3].SamplePinTolerenceZ.Value = Convert.ToDouble(lbNC_SamplePinTolZ.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[3].FailureDetermineThreshold.Value = Convert.ToInt32(lbNC_FailPercent.Content);

                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[4].PinCount.Value = Convert.ToInt32(lbPW_PinCnt.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[4].EachPinTolerenceX.Value = Convert.ToDouble(lbPW_EachPinTolX.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[4].EachPinTolerenceY.Value = Convert.ToDouble(lbPW_EachPinTolY.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[4].EachPinTolerenceZ.Value = Convert.ToDouble(lbPW_EachPinTolZ.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[4].CardCenterTolerenceX.Value = Convert.ToDouble(lbPW_CardTolX.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[4].CardCenterTolerenceY.Value = Convert.ToDouble(lbPW_CardTolY.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[4].CardCenterTolerenceZ.Value = Convert.ToDouble(lbPW_CardTolZ.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[4].MinMaxZDiffLimit.Value = Convert.ToInt32(lbPW_MinMaxZ.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[4].SamplePinAlignmentPinCount.Value = Convert.ToInt32(lbPW_SamplePinCnt.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[4].SamplePinTolerenceX.Value = Convert.ToDouble(lbPW_SamplePinTolX.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[4].SamplePinTolerenceY.Value = Convert.ToDouble(lbPW_SamplePinTolY.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[4].SamplePinTolerenceZ.Value = Convert.ToDouble(lbPW_SamplePinTolZ.Content);
                this.PinAligner().AlignInfo.PinAlignParam.PinAlignSettignParam[4].FailureDetermineThreshold.Value = Convert.ToInt32(lbPW_FailPercent.Content);

                if (this.PinAligner().SavePinAlignParam() == EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"bntSave_Click() : Save Ok.");
                }
                else
                {
                    LoggerManager.Debug($"bntSave_Click() : Save Fail.");
                }*/
            }
            catch (Exception err)
            {
                throw err;
            }
            
        }

        private async void bntExit_Click(object sender, RoutedEventArgs e)
        {
            await this.MetroDialogManager().CloseWindow(this);
        }

        private void lb_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
            System.Windows.Controls.Label lb = (System.Windows.Controls.Label)sender;
            lb.Content = VirtualKeyboard.Show(lb.Content.ToString(), KB_TYPE.DECIMAL, 0, 100);
            //lb.GetBindingExpression(System.Windows.Controls.Label.ContentProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
    }
}
