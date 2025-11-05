using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Autofac;
using ProberInterfaces;
using ProberInterfaces.Vision;
using ProberInterfaces.Enum;
using RelayCommandBase;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using PMIModuleParameter;
using ProberInterfaces.DialogControl;
using LogModule;
using System.Runtime.CompilerServices;
using ProberErrorCode;
using ProberInterfaces.PMI;
using System.Windows.Media;
using System.Windows.Shapes;
using VirtualKeyboardControl;
using SubstrateObjects;
using SerializerUtil;
using System.Collections.ObjectModel;
using MetroDialogInterfaces;

namespace PMISetup.UC
{
    public class PadTemplateSelectionControlService : INotifyPropertyChanged, IFactoryModule, IPnpAdvanceSetupViewModel
    {
        #region == > PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public PadTemplateSelectionControl DialogControl;
        private bool bChanged = false;

        private ObservableCollection<PMITemplatePack> _TemplatePacklist;
        public ObservableCollection<PMITemplatePack> TemplatePacklist
        {
            get { return _TemplatePacklist; }
            set
            {
                if (value != _TemplatePacklist)
                {
                    _TemplatePacklist = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PMITemplatePack _CurSelectedTemplate;
        public PMITemplatePack CurSelectedTemplate
        {
            get { return _CurSelectedTemplate; }
            set
            {
                if (value != _CurSelectedTemplate)
                {
                    _CurSelectedTemplate = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PAD_COLOR _PadColor = PAD_COLOR.WHITE;
        public PAD_COLOR PadColor
        {
            get { return _PadColor; }
            set
            {
                if (value != _PadColor)
                {
                    _PadColor = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _OffsetValue = 0;
        public double OffsetValue
        {
            get { return _OffsetValue; }
            set
            {
                if (value != _OffsetValue)
                {
                    _OffsetValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _CornerRadiusValue = 0;
        public double CornerRadiusValue
        {
            get { return _CornerRadiusValue; }
            set
            {
                double roundedValue = Math.Round(value, 2); // 2 decimal places

                if (roundedValue != _CornerRadiusValue)
                {
                    _CornerRadiusValue = roundedValue;
                    RaisePropertyChanged(nameof(CornerRadiusValue));
                }
            }
        }

        private string _TemplateName = "NONAME";
        public string TemplateName
        {
            get { return _TemplateName; }
            set
            {
                if (value != _TemplateName)
                {
                    _TemplateName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PadTemplate _DummyTemplate = null;
        public PadTemplate DummyTemplate
        {
            get { return _DummyTemplate; }
            set
            {
                if (value != _DummyTemplate)
                {
                    _DummyTemplate = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region ==> Constructor

        public PadTemplateSelectionControlService()
        {
            CommonConstructor();
        }
        private void CommonConstructor()
        {
            try
            {
                DialogControl = new PadTemplateSelectionControl();
                DialogControl.DataContext = this;

                //PMIModule = this.PMIModule();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        #endregion

        #region ==> Command 
        private RelayCommand<Object> _SelectTemplateShapeCommand;
        public ICommand SelectTemplateShapeCommand
        {
            get
            {
                if (null == _SelectTemplateShapeCommand) _SelectTemplateShapeCommand = new RelayCommand<Object>(SelectTemplateFunc);
                return _SelectTemplateShapeCommand;
            }
        }

        private void SelectTemplateFunc(object param)
        {
            try
            {
                PMITemplatePack pack = (param as PMITemplatePack);

                if (pack != null)
                {
                    PMITemplatePack templatepack = TemplatePacklist.Where(x => x.PadShape == pack.PadShape).FirstOrDefault();

                    if (templatepack != null)
                    {
                        CurSelectedTemplate = templatepack;
                    }
                }

                if (CurSelectedTemplate != null)
                {
                    TemplateName = CurSelectedTemplate.TemplateName;

                    // DummyTemplate으로 바인딩
                    DummyTemplate = MakePadTemplate(CurSelectedTemplate.PadShape, TemplateName, PadColor, OffsetValue, CornerRadiusValue);

                    bChanged = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _CmdExitSelectTemplateShapeClick;
        public ICommand CmdExitSelectTemplateShapeClick
        {
            get
            {
                if (null == _CmdExitSelectTemplateShapeClick) _CmdExitSelectTemplateShapeClick = new AsyncCommand(ExitTemplateShapeSelection);
                return _CmdExitSelectTemplateShapeClick;
            }
        }

        private async Task ExitTemplateShapeSelection()
        {
            PAD_SHAPE? SelectedShape = null;

            try
            {
                if (bChanged == true && CurSelectedTemplate != null)
                {
                    SelectedShape = CurSelectedTemplate.PadShape;

                    EnumMessageDialogResult ret = await this.MetroDialogManager().ShowMessageDialog(
                                                "Information",
                                                $"{SelectedShape.ToString()} was selected. Do you want to add Template?"
                                                + Environment.NewLine + "Ok         : Add & Exit"
                                                + Environment.NewLine + "Cancel     : Cancel Exit",
                                                EnumMessageStyle.AffirmativeAndNegative).ConfigureAwait(false);

                    if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        AddTemplateCommand((PAD_SHAPE)SelectedShape, TemplateName, PadColor, OffsetValue, CornerRadiusValue);
                    }

                    System.Threading.Thread.Sleep(300);
                    await this.PnPManager().ClosePnpAdavanceSetupWindow();
                }
                else
                {
                    // Not Selected Show Dialog Msg
                    EnumMessageDialogResult ret = await this.MetroDialogManager().ShowMessageDialog(
                                                "Information",
                                                "No shape was selected. Do you want to add Template?"
                                                + Environment.NewLine + "Ok         : Exit Anyway"
                                                + Environment.NewLine + "Cancel     : Cancel Exit",
                                                EnumMessageStyle.AffirmativeAndNegative).ConfigureAwait(false);

                    if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        
                    }

                    System.Threading.Thread.Sleep(300);
                    await this.PnPManager().ClosePnpAdavanceSetupWindow();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public PadTemplate MakePadTemplate(PAD_SHAPE SelectedShape, string TemplateName, PAD_COLOR color, double offset, double cornerradius)
        {
            PadTemplate SelectedTemplate = null;

            try
            {
                string val1 = (offset).ToString();
                string val2 = (1 - offset).ToString();

                switch (SelectedShape)
                {
                    // Rectangle
                    case PAD_SHAPE.RECTANGLE:
                        SelectedTemplate = new PadTemplate(PAD_SHAPE.RECTANGLE,
                                                            TemplateName,
                                                            color,
                                                            PAD_JUDGING_WINDOW_MODE.TWOWAY,
                                                            PAD_EDGE_OFFSET_MODE.DISABLE,
                                                            PAD_CORNERRADIUS_MODE.DISABLE,
                                                            "M0,0 L0,1 1,1 1,0 z");
                        break;
                    // Circle
                    case PAD_SHAPE.CIRCLE:
                        SelectedTemplate = new PadTemplate(PAD_SHAPE.CIRCLE,
                                                            TemplateName,
                                                            color,
                                                            PAD_JUDGING_WINDOW_MODE.ONEWAY,
                                                            PAD_EDGE_OFFSET_MODE.DISABLE,
                                                            PAD_CORNERRADIUS_MODE.DISABLE,
                                                            "M49.5, 25 C49.5, 38.530976 38.530976, 49.5 25, 49.5 C11.469024, 49.5 0.5, 38.530976 0.5, 25 C0.5, 11.469024 11.469024, 0.5 25, 0.5 C38.530976, 0.5 49.5, 11.469024 49.5, 25 z");
                        break;
                    // Diamond
                    case PAD_SHAPE.DIAMOND:
                        SelectedTemplate = new PadTemplate(PAD_SHAPE.DIAMOND,
                                                            TemplateName,
                                                            color,
                                                            PAD_JUDGING_WINDOW_MODE.ONEWAY,
                                                            PAD_EDGE_OFFSET_MODE.DISABLE,
                                                            PAD_CORNERRADIUS_MODE.DISABLE,
                                                            "M0.5,0 L1,0.5 0.5,1 0,0.5 z");
                        break;
                    // Oval
                    case PAD_SHAPE.OVAL:
                        SelectedTemplate = new PadTemplate(PAD_SHAPE.OVAL,
                                                            TemplateName,
                                                            color,
                                                            PAD_JUDGING_WINDOW_MODE.ONEWAY,
                                                            PAD_EDGE_OFFSET_MODE.DISABLE,
                                                            PAD_CORNERRADIUS_MODE.DISABLE,
                                                            "M24.752525,0.5 C24.961811,0.49999976 25.170599,0.50265068 25.378851,0.50792044 L25.5,0.51251572 25.5,0.5 74.5,0.5 74.5,0.50319523 74.752525,0.5 C88.146828,0.49999976 99.5,11.358226 99.5,24.752525 99.5,38.146824 88.146828,49.5 74.752525,49.5 L74.5,49.496658 74.5,49.5 25.5,49.5 25.5,49.486919 25.378851,49.491718 C25.170599,49.497226 24.961811,49.5 24.752525,49.5 11.358226,49.5 0.5,38.146824 0.5,24.752525 0.5,11.358226 11.358226,0.49999976 24.752525,0.5 z");
                        break;
                    // Octagon
                    case PAD_SHAPE.OCTAGON:
                        SelectedTemplate = new PadTemplate(PAD_SHAPE.OCTAGON,
                                                            TemplateName,
                                                            color,
                                                            PAD_JUDGING_WINDOW_MODE.ONEWAY,
                                                            PAD_EDGE_OFFSET_MODE.ENABLE,
                                                            PAD_CORNERRADIUS_MODE.DISABLE,
                                                            "M0," + val1 + " L0," + val2 + " " + val1 + ",1 " + val2 + ",1 1," + val2 + " 1," + val1 + " " + val2 + ",0 " + val1 + ",0 z",
                                                            offset);
                        //new PadTemplate("Octagon", "M0,0.2 L0,0.8 0.2,1 0.8,1 1,0.8 1,0.2 0.8,0 0.2,0 z");
                        break;
                    // Half-Octagon
                    case PAD_SHAPE.HALF_OCTAGON:
                        SelectedTemplate = new PadTemplate(PAD_SHAPE.HALF_OCTAGON,
                                                            TemplateName,
                                                            color,
                                                            PAD_JUDGING_WINDOW_MODE.ONEWAY,
                                                            PAD_EDGE_OFFSET_MODE.ENABLE,
                                                            PAD_CORNERRADIUS_MODE.DISABLE,
                                                            "M0," + val1 + " L0,1 1,1 1," + val1 + " " + val2 + ",0 " + val1 + ",0 z",
                                                            offset);
                        //new PadTemplate("Half-Octagon", "M0,0.2 L0,1 1,1 1,0.2 0.8,0 0.2,0 z");
                        break;
                    case PAD_SHAPE.ROUNDED_RECTANGLE:

                        SelectedTemplate = new PadTemplate(PAD_SHAPE.ROUNDED_RECTANGLE,
                                    TemplateName,
                                    color,
                                    PAD_JUDGING_WINDOW_MODE.TWOWAY,
                                    PAD_EDGE_OFFSET_MODE.DISABLE,
                                    PAD_CORNERRADIUS_MODE.ENABLE,
                                    "M" + cornerradius + ",0" +
                                    " H" + (1 - cornerradius) + // 오른쪽으로 이동
                                    " Q1,0 1," + cornerradius + // 오른쪽 상단 모서리 둥글게
                                    " V" + (1 - cornerradius) + // 아래로 이동
                                    " Q1,1 " + (1 - cornerradius) + ",1" + // 오른쪽 하단 모서리 둥글게
                                    " H" + cornerradius + // 왼쪽으로 이동
                                    " Q0,1 0," + (1 - cornerradius) + // 왼쪽 하단 모서리 둥글게
                                    " V" + cornerradius + // 위로 이동
                                    " Q0,0 " + cornerradius + ",0" + // 왼쪽 상단 모서리 둥글게
                                    " Z", // 경로 닫기
                                    offset,
                                    cornerradius);

                        break;

                    default:
                        LoggerManager.Error($"Selected shape of pad is unknown. Please Check the logic.");
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return SelectedTemplate;
        }

        public void AddTemplateCommand(PAD_SHAPE SelectedShape, string TemplateName, PAD_COLOR color, double offset, double cornerradius)
        {
            try
            {
                PadTemplate SelectedTemplate = null;

                SelectedTemplate = MakePadTemplate(SelectedShape, TemplateName, color, offset, cornerradius);

                if (SelectedTemplate != null)
                {
                    this.PMIModule().AddPadTemplate(SelectedTemplate);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PMIModuleSubRutineStandard] [AddTemplateCommand()] : {err}");
                LoggerManager.Exception(err);
            }
        }


        public async Task ShowDialogControl()
        {
            try
            {
                CurSelectedTemplate = null;

                await this.MetroDialogManager().ShowWindow(DialogControl, true);

                LoggerManager.Debug($"PadTemplateSelectioinControlService ShowDialogControl();");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _RadioButtonClickCommand;

        public ICommand RadioButtonClickCommand
        {
            get
            {
                if (null == _RadioButtonClickCommand) _RadioButtonClickCommand = new RelayCommand<Object>(RadioButtonClickCommandFunc);
                return _RadioButtonClickCommand;
            }
        }

        private void RadioButtonClickCommandFunc(Object param)
        {
            try
            {
                if (param.Equals("WHITE"))
                {
                    PadColor = PAD_COLOR.WHITE;
                }
                else if (param.Equals("BLACK"))
                {
                    PadColor = PAD_COLOR.BLACK;
                }
                LoggerManager.Debug($"RadioButtonClickCommandFunc() : Clicked. Selected Color : {PadColor}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _TextBoxClickCommand;

        public ICommand TextBoxClickCommand
        {
            get
            {
                if (null == _TextBoxClickCommand) _TextBoxClickCommand = new RelayCommand<Object>(TextBoxClickCommandFunc);
                return _TextBoxClickCommand;
            }
        }

        private void TextBoxClickCommandFunc(Object param)
        {
            try
            {
                LoggerManager.Debug($"TextBoxClickCommandFunc() : Clicked.");
                Object[] paramArr = param as Object[];

                int iLowerLimit = 0;
                int iUpperLimit = 20;
                string tbValue = "";

                System.Type type = null;

                if (paramArr[1] == null)
                {
                    throw new System.ArgumentException("Parameter Cannot Be Null");
                }

                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)paramArr[0];
                tbValue = tb.Text;

                if (paramArr[1] is string)
                {
                    string eParam = paramArr[1] as string;

                    type = typeof(string);
                    tbValue = eParam.ToString();
                }

                if (type == null)
                {
                    throw new System.ArgumentException("Parameter Type Cannot Be Null");
                }
                else if (type == typeof(string))
                {
                    tb.Text = VirtualKeyboard.Show(tbValue, KB_TYPE.DECIMAL | KB_TYPE.FLOAT | KB_TYPE.ALPHABET | KB_TYPE.SPECIAL, iLowerLimit, iUpperLimit);
                }

                // Update source data
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
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

                        //SerializeManager.DeserializeFromByte(param, out target, typeof(List<PMITemplatePack>));
                        target = SerializeManager.ByteToObject(param);

                        if (target != null)
                        {
                            this.TemplatePacklist = target as ObservableCollection<PMITemplatePack>;
                            //(this.StageSupervisor().WaferObject.GetSubsInfo() as SubstrateInfo).PMIInfo = (PMIInfo)target;

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

            //this.PMIModule().LoadSysParameter();
            try
            {
                //if (PMIInfo != null)
                //{
                //    parameters.Add(SerializeManager.ObjectToByte(PMIInfo));
                //    PMIInfo.PMIInfoUpdatedToLoader();
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return parameters;
        }

        public void Init()
        {
            try
            {
                bChanged = false;
                TemplateName = "NONAME";
                CurSelectedTemplate = null;
                DummyTemplate = null;

                if (TemplatePacklist.Count > 0)
                {
                    CurSelectedTemplate = TemplatePacklist[0];
                    TemplateName = CurSelectedTemplate.TemplateName;

                    DummyTemplate = MakePadTemplate(CurSelectedTemplate.PadShape, TemplateName, PadColor, OffsetValue, CornerRadiusValue);
                }
                else
                {
                    TemplateName = "NONAME";
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        #endregion
    }
}
