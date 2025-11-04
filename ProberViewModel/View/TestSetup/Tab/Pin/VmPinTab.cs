using System;

namespace TestSetupDialog.Tab.Pin
{
    using LogModule;
    using ProberInterfaces;
    using ProberInterfaces.Param;
    using ProberInterfaces.PinAlign.ProbeCardData;
    using ProberInterfaces.PnpSetup;
    using RelayCommandBase;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    public class VmPinTab : INotifyPropertyChanged, IFactoryModule, IDisposable
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ==> EnablePNPBtnCommand
        private RelayCommand _EnablePNPBtnCommand;
        public ICommand EnablePNPBtnCommand
        {
            get
            {
                if (null == _EnablePNPBtnCommand) _EnablePNPBtnCommand = new RelayCommand(EnablePNPBtnCommandFunc);
                return _EnablePNPBtnCommand;
            }
        }
        private void EnablePNPBtnCommandFunc()
        {
            try
            {
                IPnpSetup step = this.PnPManager().SeletedStep as IPnpSetup;
                if (step == null)
                    return;

                step.ThreeButton.Visibility = System.Windows.Visibility.Visible;
                step.FourButton.Visibility = System.Windows.Visibility.Visible;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #region ==> DisablePNPBtnCommand
        private RelayCommand _DisablePNPBtnCommand;
        public ICommand DisablePNPBtnCommand
        {
            get
            {
                if (null == _DisablePNPBtnCommand) _DisablePNPBtnCommand = new RelayCommand(DisablePNPBtnCommandFunc);
                return _DisablePNPBtnCommand;
            }
        }
        private void DisablePNPBtnCommandFunc()
        {
            try
            {
                IPnpSetup step = this.PnPManager().SeletedStep as IPnpSetup;
                if (step == null)
                    return;

                step.ThreeButton.Visibility = System.Windows.Visibility.Collapsed;
                step.FourButton.Visibility = System.Windows.Visibility.Collapsed;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #region ==> RefreshCommand
        private RelayCommand _RefreshCommand;
        public ICommand RefreshCommand
        {
            get
            {
                if (null == _RefreshCommand) _RefreshCommand = new RelayCommand(RefreshCommandFunc);
                return _RefreshCommand;
            }
        }
        private void RefreshCommandFunc()
        {
            try
            {
                PinItemList = new ObservableCollection<PinItemVM>();
                foreach (IDut dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                {
                    foreach (IPinData pin in dut.PinList)
                    {
                        PinItemList.Add(new PinItemVM(dut, pin));
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #region ==> PinList
        private ObservableCollection<PinItemVM> _PinItemList;
        public ObservableCollection<PinItemVM> PinItemList
        {
            get { return _PinItemList; }
            set
            {
                if (value != _PinItemList)
                {
                    _PinItemList = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        public VmPinTab()
        {
            try
            {
                this.PinAligner().SinglePinAligner.TestMock = new SinglePinAlignTestMock();
                RefreshCommandFunc();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void Dispose()
        {
            this.PinAligner().SinglePinAligner.TestMock = null;
        }
    }
    public class PinItemVM : INotifyPropertyChanged, IFactoryModule
    {

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ==> PinNum
        private int _PinNum;
        public int PinNum
        {
            get { return _PinNum; }
            set
            {
                if (value != _PinNum)
                {
                    _PinNum = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> DutNum
        private int _DutNum;
        public int DutNum
        {
            get { return _DutNum; }
            set
            {
                if (value != _DutNum)
                {
                    _DutNum = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> PinResult
        private EnumPinTest _PinResult;
        public EnumPinTest PinResult
        {
            get { return _PinResult; }
            set
            {
                if (value != _PinResult)
                {
                    _PinResult = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> XPosValue
        private double _XPosValue;
        public double XPosValue
        {
            get { return _XPosValue; }
            set
            {
                if (value != _XPosValue)
                {
                    _XPosValue = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> YPosValue
        private double _YPosValue;
        public double YPosValue
        {
            get { return _YPosValue; }
            set
            {
                if (value != _YPosValue)
                {
                    _YPosValue = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> ZPosValue
        private double _ZPosValue;
        public double ZPosValue
        {
            get { return _ZPosValue; }
            set
            {
                if (value != _ZPosValue)
                {
                    _ZPosValue = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> XRelPosStr
        private String _XRelPosStr;
        public String XRelPosStr
        {
            get { return _XRelPosStr; }
            set
            {
                if (value != _XRelPosStr)
                {
                    _XRelPosStr = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> YRelPosStr
        private String _YRelPosStr;
        public String YRelPosStr
        {
            get { return _YRelPosStr; }
            set
            {
                if (value != _YRelPosStr)
                {
                    _YRelPosStr = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> ZRelPosStr
        private String _ZRelPosStr;
        public String ZRelPosStr
        {
            get { return _ZRelPosStr; }
            set
            {
                if (value != _ZRelPosStr)
                {
                    _ZRelPosStr = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> SettingEnable
        private bool _SettingEnable;
        public bool SettingEnable
        {
            get { return _SettingEnable; }
            set
            {
                if (value != _SettingEnable)
                {
                    _SettingEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> XPosSetCommand
        private RelayCommand _XPosSetCommand;
        public ICommand XPosSetCommand
        {
            get
            {
                if (null == _XPosSetCommand) _XPosSetCommand = new RelayCommand(XPosSetCommandFunc);
                return _XPosSetCommand;
            }
        }
        private void XPosSetCommandFunc()
        {
            _PinData.AbsPos.X.Value = XPosValue;
        }
        #endregion

        #region ==> YPosSetCommand
        private RelayCommand _YPosSetCommand;
        public ICommand YPosSetCommand
        {
            get
            {
                if (null == _YPosSetCommand) _YPosSetCommand = new RelayCommand(YPosSetCommandFunc);
                return _YPosSetCommand;
            }
        }
        private void YPosSetCommandFunc()
        {
            _PinData.AbsPos.Y.Value = YPosValue;
        }
        #endregion

        #region ==> ZPosSetCommand
        private RelayCommand _ZPosSetCommand;
        public ICommand ZPosSetCommand
        {
            get
            {
                if (null == _ZPosSetCommand) _ZPosSetCommand = new RelayCommand(ZPosSetCommandFunc);
                return _ZPosSetCommand;
            }
        }
        private void ZPosSetCommandFunc()
        {
            _PinData.AbsPos.Z.Value = ZPosValue;
        }
        #endregion

        #region ==> XRelPosSetCommand
        private RelayCommand _XRelPosSetCommand;
        public ICommand XRelPosSetCommand
        {
            get
            {
                if (null == _XRelPosSetCommand) _XRelPosSetCommand = new RelayCommand(XRelPosSetCommandFunc);
                return _XRelPosSetCommand;
            }
        }
        private void XRelPosSetCommandFunc()
        {
            try
            {
                double dbl;
                if (double.TryParse(XRelPosStr, out dbl) == false)
                {
                    XRelPosStr = XRelValue.ToString();
                    return;
                }

                XRelValue = dbl;
                this.PinAligner().SinglePinAligner.TestMock.PinRelMapping[PinNum].X.Value = XRelValue;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #region ==> YRelPosSetCommand
        private RelayCommand _YRelPosSetCommand;
        public ICommand YRelPosSetCommand
        {
            get
            {
                if (null == _YRelPosSetCommand) _YRelPosSetCommand = new RelayCommand(YRelPosSetCommandFunc);
                return _YRelPosSetCommand;
            }
        }
        private void YRelPosSetCommandFunc()
        {
            try
            {
                double dbl;
                if (double.TryParse(YRelPosStr, out dbl) == false)
                {
                    YRelPosStr = YRelValue.ToString();
                    return;
                }

                YRelValue = dbl;
                this.PinAligner().SinglePinAligner.TestMock.PinRelMapping[PinNum].Y.Value = YRelValue;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #region ==> ZRelPosSetCommand
        private RelayCommand _ZRelPosSetCommand;
        public ICommand ZRelPosSetCommand
        {
            get
            {
                if (null == _ZRelPosSetCommand) _ZRelPosSetCommand = new RelayCommand(ZRelPosSetCommandFunc);
                return _ZRelPosSetCommand;
            }
        }
        private void ZRelPosSetCommandFunc()
        {
            try
            {
                double dbl;
                if (double.TryParse(ZRelPosStr, out dbl) == false)
                {
                    ZRelPosStr = ZRelValue.ToString();
                    return;
                }

                ZRelValue = dbl;
                this.PinAligner().SinglePinAligner.TestMock.PinRelMapping[PinNum].Z.Value = ZRelValue;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #region ==> PassCommand
        private RelayCommand _PassCommand;
        public ICommand PassCommand
        {
            get
            {
                if (null == _PassCommand) _PassCommand = new RelayCommand(PassCommandFunc);
                return _PassCommand;
            }
        }
        private void PassCommandFunc()
        {
            try
            {
                PinResult = EnumPinTest.PASS;
                SettingEnable = true;
                this.PinAligner().SinglePinAligner.TestMock.PinTestMapping[PinNum] = PinResult;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #region ==> FailCommand
        private RelayCommand _FailCommand;
        public ICommand FailCommand
        {
            get
            {
                if (null == _FailCommand) _FailCommand = new RelayCommand(FailCommandFunc);
                return _FailCommand;
            }
        }
        private void FailCommandFunc()
        {
            try
            {
                PinResult = EnumPinTest.FAIL;
                SettingEnable = false;
                this.PinAligner().SinglePinAligner.TestMock.PinTestMapping[PinNum] = PinResult;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #region ==> NoneCommand
        private RelayCommand _NoneCommand;
        public ICommand NoneCommand
        {
            get
            {
                if (null == _NoneCommand) _NoneCommand = new RelayCommand(NoneCommandFunc);
                return _NoneCommand;
            }
        }
        private void NoneCommandFunc()
        {
            try
            {
                PinResult = EnumPinTest.NONE;
                SettingEnable = false;
                this.PinAligner().SinglePinAligner.TestMock.PinTestMapping[PinNum] = PinResult;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        double XRelValue;
        double YRelValue;
        double ZRelValue;

        private IDut _DutData;
        private IPinData _PinData;
        public PinItemVM(IDut dutData, IPinData pinData)
        {
            try
            {
                _DutData = dutData;
                _PinData = pinData;

                DutNum = _DutData.DutNumber;
                PinNum = _PinData.PinNum.Value;

                PinResult = EnumPinTest.NONE;
                SettingEnable = false;

                XPosValue = _PinData.AbsPos.X.Value;
                YPosValue = _PinData.AbsPos.Y.Value;
                ZPosValue = _PinData.AbsPos.Z.Value;

                XRelPosStr = "0";
                YRelPosStr = "0";
                ZRelPosStr = "0";
                if (this.PinAligner().SinglePinAligner.TestMock.PinRelMapping.ContainsKey(PinNum) == false)
                    this.PinAligner().SinglePinAligner.TestMock.PinRelMapping.Add(PinNum, new PinCoordinate(0, 0, 0));

                if (this.PinAligner().SinglePinAligner.TestMock.PinTestMapping.ContainsKey(PinNum) == false)
                    this.PinAligner().SinglePinAligner.TestMock.PinTestMapping.Add(PinNum, PinResult);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
