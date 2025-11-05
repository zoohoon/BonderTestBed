using System;
using System.Collections.Generic;
using System.Linq;

namespace ProberDevelopPackWindow.Tab
{
    using ControlModules;
    using LogModule;
    using ProberInterfaces;
    using RelayCommandBase;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    public class ChillerErrorMessageSettingViewModel : INotifyPropertyChanged, IFactoryModule
    {
        #region < PropertyChanged >
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region < Property >

        private ChillerManager _ChillerManager { get; set; }

        private ObservableCollection<ChillerErrorMessageInfo> _ErrorMessageInfos
             = new ObservableCollection<ChillerErrorMessageInfo>();
        public ObservableCollection<ChillerErrorMessageInfo> ErrorMessageInfos
        {
            get { return _ErrorMessageInfos; }
            set
            {
                if (value != _ErrorMessageInfos)
                {
                    _ErrorMessageInfos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _ErrorInfoDataGridSelectedIndex;
        public int ErrorInfoDataGridSelectedIndex
        {
            get { return _ErrorInfoDataGridSelectedIndex; }
            set
            {
                if (value != _ErrorInfoDataGridSelectedIndex)
                {
                    _ErrorInfoDataGridSelectedIndex = value;
                    RaisePropertyChanged();
                }
            }
        }



        #endregion

        #region < Creator & Init >

        public ChillerErrorMessageSettingViewModel()
        {
            InitViewModel();
        }
        public void InitViewModel()
        {
            try
            {
                if(_ChillerManager == null)
                {
                    _ChillerManager = (ChillerManager)this.EnvControlManager().ChillerManager;
                }

                if(_ChillerManager != null)
                {
                    ErrorMessageInfos.Clear();
                    var infos = _ChillerManager.ChillerErrorParam;
                    foreach (var info in infos.ChillerErrorMessageDic)
                    {
                        ErrorMessageInfos.Add(new ChillerErrorMessageInfo(info.Key, info.Value));
                    }
                }

                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region < Command >

        #region !--< ErrorInfoAddCommand >--!

        private RelayCommand _ErrorInfoAddCommand;
        public ICommand ErrorInfoAddCommand
        {
            get
            {
                if (null == _ErrorInfoAddCommand) _ErrorInfoAddCommand = new RelayCommand(ErrorInfoAddCommandFunc);
                return _ErrorInfoAddCommand;
            }
        }
        private void ErrorInfoAddCommandFunc()
        {
            try
            {
                if (ErrorMessageInfos.ToList<ChillerErrorMessageInfo>().Find(info => info.Number == 99999) != null)
                {
                    //await ChillerModule.MetroDialogManager().ShowMessageDialog("", "",MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                }
                else
                {

                    ErrorMessageInfos.Add(new ChillerErrorMessageInfo(99999, ""));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region !--< ErrorInfoDeleteCommand >--!

        private RelayCommand _ErrorInfoDeleteCommand;
        public ICommand ErrorInfoDeleteCommand
        {
            get
            {
                if (null == _ErrorInfoDeleteCommand) _ErrorInfoDeleteCommand = new RelayCommand(ErrorInfoDeleteCommandFunc);
                return _ErrorInfoDeleteCommand;
            }
        }
        private void ErrorInfoDeleteCommandFunc()
        {
            try
            {
                ErrorMessageInfos.RemoveAt(ErrorInfoDataGridSelectedIndex);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region !--< LoadParameterCommand >--!

        private RelayCommand _LoadParameterCommand;
        public ICommand LoadParameterCommand
        {
            get
            {
                if (null == _LoadParameterCommand) _LoadParameterCommand = new RelayCommand(LoadParameterCommandFunc);
                return _LoadParameterCommand;
            }
        }
        private void LoadParameterCommandFunc()
        {
            try
            {
                if (_ChillerManager != null)
                {
                    ErrorMessageInfos.Clear();
                    var infos = _ChillerManager.ChillerErrorParam;
                    foreach (var info in infos.ChillerErrorMessageDic)
                    {
                        ErrorMessageInfos.Add(new ChillerErrorMessageInfo(info.Key, info.Value));
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region !--< SaveParameterCommand >--!

        private RelayCommand _SaveParameterCommand;
        public ICommand SaveParameterCommand
        {
            get
            {
                if (null == _SaveParameterCommand) _SaveParameterCommand = new RelayCommand(SaveParameterCommandFunc);
                return _SaveParameterCommand;
            }
        }
        private void SaveParameterCommandFunc()
        {
            try
            {
                Dictionary<double, string> newDic = new Dictionary<double, string>();
                foreach (var info in ErrorMessageInfos)
                {
                    newDic.Add(info.Number, info.Message);
                }

                _ChillerManager.ChillerErrorParam.ChillerErrorMessageDic = newDic;
                _ChillerManager.SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region !--< RowEditEndingEventCommand >--!

        private RelayCommand _RowEditEndingEventCommand;
        public ICommand RowEditEndingEventCommand
        {
            get
            {
                if (null == _RowEditEndingEventCommand) _RowEditEndingEventCommand = new RelayCommand(RowEditEndingEventCommandFunc);
                return _RowEditEndingEventCommand;
            }
        }
        private void RowEditEndingEventCommandFunc()
        {
            try
            {
                //ErrorMessageInfos = new ObservableCollection<ChillerErrorMessageInfo>(ErrorMessageInfos.OrderBy(infos => infos.Number));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #endregion
    }

    public class ChillerErrorMessageInfo : INotifyPropertyChanged
    {
        #region < PropertyChanged >
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private double _Number;
        public double Number
        {
            get { return _Number; }
            set
            {
                if (value != _Number)
                {
                    _Number = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Message;
        public string Message
        {
            get { return _Message; }
            set
            {
                if (value != _Message)
                {
                    _Message = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ChillerErrorMessageInfo(double number, string message)
        {
            this.Number = number;
            this.Message = message;
        }
    }
}
