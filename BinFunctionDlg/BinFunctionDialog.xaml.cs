using BinParamObject;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.BinData;
using RelayCommandBase;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace BinFunctionDlg
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BinFunctionDialog : Window
    {
        BinFunctionDialogViewModel vm = null;

        public BinFunctionDialog()
        {
            InitializeComponent();

            vm = new BinFunctionDialogViewModel();
            this.DataContext = vm;
        }
    }

    public class BinFunctionDialogViewModel : INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        #region ==> Property

        private ObservableCollection<IBINInfo> _binInfos;
        public ObservableCollection<IBINInfo> BINInfos
        {
            get { return _binInfos; }
            set
            {
                if (value != _binInfos)
                {
                    _binInfos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IBINInfo _selectedBINInfo = null;
        public IBINInfo SelectedBINInfo
        {
            get { return _selectedBINInfo; }
            set
            {
                if (value != _selectedBINInfo)
                {
                    _selectedBINInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region ==> Costructor
        public BinFunctionDialogViewModel()
        {
            var bininfos = this.ProbingModule().GetBinInfos();

            if (bininfos != null)
            {
                BINInfos = new ObservableCollection<IBINInfo>(bininfos);
            }
            else
            {
                LoggerManager.Error($"[BinFunctionDialogViewModel], BinFunctionDialogViewModel() : BIN information is null.");
            }

            if (BINInfos != null && BINInfos.Count > 0)
            {
                SelectedBINInfo = BINInfos.First();
            }
        }
        #endregion

        #region ==> Command
        private ICommand _addBINInfoCommand;
        public ICommand AddBINInfoCommand
        {
            get
            {
                if (_addBINInfoCommand == null)
                {
                    _addBINInfoCommand = new RelayCommand(AddBINInfoCommandFunc);
                }

                return _addBINInfoCommand;
            }
        }

        private void AddBINInfoCommandFunc()
        {
            try
            {
                if (BINInfos != null)
                {
                    BINInfo tmpbINInfo = new BINInfo();

                    tmpbINInfo.PassFail.Value = 0;

                    var maxBinCode = GetMaximumBinCode();

                    if (maxBinCode != null && maxBinCode >= 0)
                    {
                        tmpbINInfo.BinCode.Value = (int)maxBinCode + 1;
                    }
                    else
                    {
                        tmpbINInfo.BinCode.Value = -1;
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        BINInfos.Add(tmpbINInfo);

                        SelectedBINInfo = BINInfos.Last();
                    });

                    LoggerManager.Debug($"[BinFunctionDialogViewModel], DeleteBINInfoCommandFunc() : Added {tmpbINInfo.BinCode}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private ICommand _deleteBINInfoCommand;
        public ICommand DeleteBINInfoCommand
        {
            get
            {
                if (_deleteBINInfoCommand == null)
                {
                    _deleteBINInfoCommand = new RelayCommand(DeleteBINInfoCommandFunc);
                }

                return _deleteBINInfoCommand;
            }
        }

        private void DeleteBINInfoCommandFunc()
        {
            try
            {
                if (BINInfos != null && BINInfos.Count > 0)
                {
                    if (SelectedBINInfo != null)
                    {
                        LoggerManager.Debug($"[BinFunctionDialogViewModel], DeleteBINInfoCommandFunc() : Removed {SelectedBINInfo.BinCode}");

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            var selectedindex = BINInfos.IndexOf(SelectedBINInfo);

                            BINInfos.Remove(SelectedBINInfo);

                            if(BINInfos.Count > 0)
                            {
                                if (selectedindex == 0)
                                {
                                    SelectedBINInfo = BINInfos[0];
                                }
                                else
                                {
                                    SelectedBINInfo = BINInfos[selectedindex - 1];
                                }
                            }
                        });
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private ICommand _deleteAllBINInfosCommand;
        public ICommand DeleteAllBINInfosCommand
        {
            get
            {
                if (_deleteAllBINInfosCommand == null)
                {
                    _deleteAllBINInfosCommand = new RelayCommand(DeleteAllBINInfosCommandFunc);
                }

                return _deleteAllBINInfosCommand;
            }
        }

        private void DeleteAllBINInfosCommandFunc()
        {
            try
            {
                if (BINInfos != null && BINInfos.Count > 0)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        BINInfos.Clear();
                    });

                    // TODO : 정말 지우시겠습니까?

                    //var retVal = await this.MetroDialogManager().ShowMessageDialog("[DELETE ALL]", $"Do you want to delete?", EnumMessageStyle.AffirmativeAndNegative);

                    //if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
                    //{
                    //    Application.Current.Dispatcher.Invoke(() =>
                    //    {
                    //        BINInfos.Clear();
                    //    });
                    //}
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private int? GetMaximumBinCode()
        {
            int? retval = null;

            try
            {
                if (BINInfos != null && BINInfos.Count > 0)
                {
                    retval = BINInfos.Max(x => x.BinCode.Value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private ICommand _saveBINInfosCommand;
        public ICommand SaveBINInfosCommand
        {
            get
            {
                if (_saveBINInfosCommand == null)
                {
                    _saveBINInfosCommand = new RelayCommand(SaveBINInfosCommandFunc);
                }

                return _saveBINInfosCommand;
            }
        }

        private void SaveBINInfosCommandFunc()
        {
            try
            {
                EventCodeEnum retval = EventCodeEnum.UNDEFINED;

                if(BINInfos != null)
                {
                    retval = this.ProbingModule().SetBinInfos(this.BINInfos.ToList());

                    if (retval == EventCodeEnum.NONE)
                    {
                        retval = this.ProbingModule().SaveBinDevParam();
                    }
                    else
                    {
                        if (retval != EventCodeEnum.NONE)
                        {
                            LoggerManager.Error($"[BinFunctionDialogViewModel], SaveBINInfosCommandFunc() : SetBinInfos Failed. retval = {retval}");
                        }
                    }

                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"[BinFunctionDialogViewModel], SaveBINInfosCommandFunc() : Failed. retval = {retval}");
                    }
                }
                else
                {
                    LoggerManager.Error($"[BinFunctionDialogViewModel], SaveBINInfosCommandFunc() : BINInfos is null.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        //private void OpenPathDialogFunc()
        //{
        //    FolderBrowserDialog folderBroserDialog = new FolderBrowserDialog();

        //    var result = folderBroserDialog.ShowDialog();
        //    if (result == DialogResult.OK)
        //    {
        //        var path = folderBroserDialog.SelectedPath;
        //        this.UploadPath = path;
        //    }
        //}
        #endregion
    }
}