using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LogModule;
using ProberInterfaces;
using RelayCommandBase;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using VirtualKeyboardControl;
using SerializerUtil;
using ProbeCardObject;
using ProberInterfaces.PinAlign.ProbeCardData;
using System.Linq;

namespace HighAlignKeyAdvancedSetupModule
{

    public class HighAlignKeyAdvancedSetupControlService : INotifyPropertyChanged, IFactoryModule, IPnpAdvanceSetupViewModel
    {
        #region == > PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public HighAlignKeyAdvancedSetupControl DialogControl { get; set; }
        private PinAlignDevParameters _PinAlignDevParam;
        public PinAlignDevParameters PinAlignDevParam
        {
            get { return _PinAlignDevParam; }
            set
            {
                if (value != _PinAlignDevParam)
                {
                    _PinAlignDevParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private WrapperDutlist _WrapperDutlist;
        public WrapperDutlist WrapperDutlist
        {
            get { return _WrapperDutlist; }
            set
            {
                if (value != _WrapperDutlist)
                {
                    _WrapperDutlist = value;
                    RaisePropertyChanged();
                }
            }
        }

        private FOCUSINGRAGE _KeyFocusingRange = FOCUSINGRAGE.RANGE_100;
        public FOCUSINGRAGE KeyFocusingRange
        {
            get { return _KeyFocusingRange; }
            set
            {
                if (value != _KeyFocusingRange)
                {
                    _KeyFocusingRange = value;
                    RaisePropertyChanged();
                }

                IsChangedKeyFocusingRangeProperty = true;
            }
        }

        private int _MinBlobSizeX;
        public int MinBlobSizeX
        {
            get { return _MinBlobSizeX; }
            set
            {
                if (value != _MinBlobSizeX)
                {
                    _MinBlobSizeX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _MinBlobSizeY;
        public int MinBlobSizeY
        {
            get { return _MinBlobSizeY; }
            set
            {
                if (value != _MinBlobSizeY)
                {
                    _MinBlobSizeY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _MaxBlobSizeX;
        public int MaxBlobSizeX
        {
            get { return _MaxBlobSizeX; }
            set
            {
                if (value != _MaxBlobSizeX)
                {
                    _MaxBlobSizeX = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _MaxBlobSizeY;
        public int MaxBlobSizeY
        {
            get { return _MaxBlobSizeY; }
            set
            {
                if (value != _MaxBlobSizeY)
                {
                    _MaxBlobSizeY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool IsChangedKeyFocusingRangeProperty = false;

        public HighAlignKeyAdvancedSetupControlService()
        {
            try
            {
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    DialogControl = new HighAlignKeyAdvancedSetupControl();
                    DialogControl.DataContext = this;
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand _CmdMaunalInputContolExitClick;
        public ICommand CmdMaunalInputContolExitClick
        {
            get
            {
                if (null == _CmdMaunalInputContolExitClick) _CmdMaunalInputContolExitClick = new AsyncCommand(MaunalInputContolExitClick);
                return _CmdMaunalInputContolExitClick;
            }
        }

        private async Task MaunalInputContolExitClick()
        {
            try
            {
                //await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Please wait");

                if (IsChangedKeyFocusingRangeProperty == true)
                {
                    if (WrapperDutlist != null && WrapperDutlist.DutList != null)
                    {
                        double ChangedRangeValue = (double)KeyFocusingRange;

                        foreach (var dut in WrapperDutlist.DutList)
                        {
                            foreach (var pin in dut.PinList)
                            {
                                foreach (var key in pin.PinSearchParam.AlignKeyHigh)
                                {
                                    key.FocusingRange.Value = ChangedRangeValue;
                                }
                            }
                        }
                    }
                }

                IsChangedKeyFocusingRangeProperty = false;

                string altPramCheckMsg = "";
                if (this.ParamManager().CheckExistAltParamInParameterObject(this.PinAligner().PinAlignDevParam, ref altPramCheckMsg))
                {
                    await this.MetroDialogManager().ShowMessageDialog("Information Message", $"The file you want to save include parameters contained in AltParam.{altPramCheckMsg}", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                }
                
                await this.PnPManager().ClosePnpAdavanceSetupWindow();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        private AsyncCommand<Object> _DecimalTextBoxClickCommand;
        public ICommand DecimalTextBoxClickCommand
        {
            get
            {
                if (null == _DecimalTextBoxClickCommand) _DecimalTextBoxClickCommand = new AsyncCommand<Object>(FuncDecimalTextBoxClickCommand);
                return _DecimalTextBoxClickCommand;
            }
        }

        private async Task FuncDecimalTextBoxClickCommand(object param)
        {
            try
            {
                await System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                    tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL);
                    tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                 }));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _FloatTextBoxClickCommand;
        public ICommand FloatTextBoxClickCommand
        {
            get
            {
                if (null == _FloatTextBoxClickCommand) _FloatTextBoxClickCommand = new RelayCommand<Object>(FloatTextBoxClickCommandFunc);
                return _FloatTextBoxClickCommand;
            }
        }

        private void FloatTextBoxClickCommandFunc(object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL | KB_TYPE.FLOAT);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<Object> _PinTipSizeValidationTextBoxClickCommand;
        public ICommand PinTipSizeValidationTextBoxClickCommand
        {
            get
            {
                if (null == _PinTipSizeValidationTextBoxClickCommand) _PinTipSizeValidationTextBoxClickCommand = new AsyncCommand<Object>(PinTipSizeValidationTextBoxClickCommandFunc);
                return _PinTipSizeValidationTextBoxClickCommand;
            }
        }

        private async Task PinTipSizeValidationTextBoxClickCommandFunc(object param)
        {
            try
            {
                await System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                    string newName = tb.Name;
                    double newValue = double.Parse(VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL | KB_TYPE.FLOAT));

                    switch (newName)
                    {
                        case "PinTipSizeValidation_Min_X":
                            if (newValue >= MinBlobSizeX && newValue <= PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Max_X.Value)
                            {
                                tb.Text = newValue.ToString();
                            }
                            break;
                        case "PinTipSizeValidation_Min_Y":
                            if (newValue >= MinBlobSizeY && newValue <= PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Max_Y.Value)
                            {
                                tb.Text = newValue.ToString();
                            }
                            break;
                        case "PinTipSizeValidation_Max_X":
                            if (newValue <= MaxBlobSizeX && newValue >= PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Min_X.Value)
                            {
                                tb.Text = newValue.ToString();
                            }
                            break;
                        case "PinTipSizeValidation_Max_Y":
                            if (newValue <= MaxBlobSizeY && newValue >= PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Min_Y.Value)
                            {
                                tb.Text = newValue.ToString();
                            }
                            break;
                    }

                    tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();

                    //System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                    //tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL);
                    //tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                }));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private List<IPinData>_pinList = new List<IPinData>();
        public List<IPinData> pinList 
        {
            get { return _pinList; }
            set
            {
                if (value != _pinList)
                {
                    _pinList = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region //.. IPnpAdvanceSetupViewModel Method
        public void SetParameters(List<byte[]> datas)
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
                                PinAlignDevParam = (PinAlignDevParameters)target;
                            }
                            else
                            {
                                LoggerManager.Error("[HighAlignKeyAdvancedSetupControlService] SetParameters() : PinAlignDevParam deserialized failed.");
                            }
                        }
                        else if (i == 1)
                        {
                            SerializeManager.DeserializeFromByte(datas[i], out target, typeof(WrapperDutlist));
                            
                            if (target != null)
                            {
                                WrapperDutlist = (WrapperDutlist)target;
                                
                                IEnumerable<List<IPinData>> pinListes = WrapperDutlist.DutList.Select(dut => dut.PinList);
                                foreach (List <IPinData> pines in pinListes)
                                {
                                    pinList.AddRange(pines);
                                }

                                if(pinList.Count > 0)
                                {
                                    MinBlobSizeX = pinList[0].PinSearchParam.MinBlobSizeX.Value;
                                    MinBlobSizeY = pinList[0].PinSearchParam.MinBlobSizeY.Value;

                                    MaxBlobSizeX = pinList[0].PinSearchParam.MaxBlobSizeX.Value;
                                    MaxBlobSizeY = pinList[0].PinSearchParam.MaxBlobSizeY.Value;

                                    if(PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Min_X.Value < MinBlobSizeX ||
                                        PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Min_X.Value > MaxBlobSizeX ||
                                        PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Min_X.Value > PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Max_X.Value)
                                    {
                                        LoggerManager.Debug($"[HighAlignKeyAdvancedSetupControlService] SetParameters() : MinBlobSizeX = {MinBlobSizeX}, PinTipSizeValidation_Min_X = {PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Min_X.Value}");
                                        
                                        PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Min_X.Value = MinBlobSizeX;
                                    }

                                    if (PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Min_Y.Value < MinBlobSizeY ||
                                        PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Min_Y.Value > MaxBlobSizeY ||
                                        PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Min_Y.Value > PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Max_Y.Value)
                                    {
                                        LoggerManager.Debug($"[HighAlignKeyAdvancedSetupControlService] SetParameters() : MinBlobSizeY = {MinBlobSizeY}, PinTipSizeValidation_Min_Y = {PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Min_Y.Value}");
                                        
                                        PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Min_Y.Value = MinBlobSizeY;
                                    }

                                    if (PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Max_X.Value > MaxBlobSizeX ||
                                        PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Max_X.Value < MinBlobSizeX ||
                                        PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Max_X.Value < PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Min_X.Value)
                                    {
                                        LoggerManager.Debug($"[HighAlignKeyAdvancedSetupControlService] SetParameters() : MaxBlobSizeX = {MaxBlobSizeX}, PinTipSizeValidation_Max_X = {PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Max_X.Value}");
                                        
                                        PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Max_X.Value = MaxBlobSizeX;
                                    }

                                    if (PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Max_Y.Value > MaxBlobSizeY ||
                                        PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Max_Y.Value < MinBlobSizeY ||
                                        PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Max_Y.Value < PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Min_Y.Value)
                                    {
                                        LoggerManager.Debug($"[HighAlignKeyAdvancedSetupControlService] SetParameters() : MaxBlobSizeY = {MaxBlobSizeY}, PinTipSizeValidation_Max_Y = {PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Max_Y.Value}");
                                        
                                        PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Max_Y.Value = MaxBlobSizeY;
                                    }
                                }
                            }
                            else
                            {
                                LoggerManager.Error("[HighAlignKeyAdvancedSetupControlService] SetParameters() : Dutlist deserialized failed.");
                            }
                        }
                    }
                }
                else
                {
                    LoggerManager.Error("[HighAlignKeyAdvancedSetupControlService] SetParameters() : Input data is incorrect.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _Add_TargetList_For_PinTipSizeValidation;
        public ICommand Add_TargetList_For_PinTipSizeValidation
        {
            get
            {
                if (null == _Add_TargetList_For_PinTipSizeValidation) _Add_TargetList_For_PinTipSizeValidation = new RelayCommand<object>(Add_TargetList_For_PinTipSizeValidationFunc);
                return _Add_TargetList_For_PinTipSizeValidation;
            }
        }
        private void Add_TargetList_For_PinTipSizeValidationFunc(object param)
        {
            try
            {
                if(param is int pinNum)
                {
                    if(PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Pin_List == null)
                    {
                        PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Pin_List = new System.Collections.ObjectModel.ObservableCollection<ValidationPinItem>();
                    }

                    PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Pin_List.Add(new ValidationPinItem(pinNum, true));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _Delete_TargetList_For_PinTipSizeValidation;
        public ICommand DeletePinTipSizeValidation_TargetList
        {
            get
            {
                if (null == _Delete_TargetList_For_PinTipSizeValidation) _Delete_TargetList_For_PinTipSizeValidation = new RelayCommand<object>(DeletePinTipSizeValidation_TargetListFunc);
                return _Delete_TargetList_For_PinTipSizeValidation;
            }
        }
        private void DeletePinTipSizeValidation_TargetListFunc(object param)
        {
            try
            {
                if (param is int pinNum)
                {
                    // 삭제할 항목을 찾기 위해 리스트를 반복합니다.
                    List<ValidationPinItem> itemsToRemove = new List<ValidationPinItem>();

                    foreach (var item in PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Pin_List)
                    {
                        if (item.PinNum.Value == pinNum)
                        {
                            itemsToRemove.Add(item);
                        }
                    }

                    // 찾은 항목을 리스트에서 제거합니다.
                    foreach (var item in itemsToRemove)
                    {
                        PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Pin_List.Remove(item);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        //private RelayCommand<object> _All_Check_TargetList_For_PinTipSizeValidation;
        //public ICommand All_Check_TargetList_For_PinTipSizeValidation
        //{
        //    get
        //    {
        //        if (null == _All_Check_TargetList_For_PinTipSizeValidation) _All_Check_TargetList_For_PinTipSizeValidation = new RelayCommand<object>(All_Check_TargetList_For_PinTipSizeValidationFunc);
        //        return _All_Check_TargetList_For_PinTipSizeValidation;
        //    }
        //}
        //private void All_Check_TargetList_For_PinTipSizeValidationFunc(object param)
        //{
        //    try
        //    {
        //        if(PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Pin_List.Value.Count != 0)
        //        {
        //            PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Pin_List.Value.Clear();
        //        }

        //        if(PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Pin_List.Value == null)
        //        {
        //            PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Pin_List.Value = new List<ValidationPinItem>();
        //        }

        //        foreach (var num in pinList)
        //        {
        //            PinAlignDevParam.PinHighAlignParam.PinTipSizeValidation_Pin_List.Value.Add(new ValidationPinItem(num.PinNum.Value, true));
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        public List<byte[]> GetParameters()
        {
            List<byte[]> parameters = new List<byte[]>();
            try
            {
                if (PinAlignDevParam != null)
                {
                    parameters.Add(SerializeManager.SerializeToByte(PinAlignDevParam));
                }

                if (WrapperDutlist != null)
                {
                    parameters.Add(SerializeManager.SerializeToByte(WrapperDutlist));
                }
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
}
