using LogModule;
using ProberInterfaces;
using RelayCommandBase;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Input;

namespace TestResultDialogVM
{
    public class TestResultDialogViewModel : INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        #region ==> Property

        private string _LotID;
        public string LotID
        {
            get { return _LotID; }
            set
            {
                if (_LotID != value)
                {
                    _LotID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _WaferID;
        public string WaferID
        {
            get { return _WaferID; }
            set
            {
                if (_WaferID != value)
                {
                    _WaferID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _UploadPath;
        public string UploadPath
        {
            get { return _UploadPath; }
            set
            {
                if (_UploadPath != value)
                {
                    _UploadPath = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> Costructor
        public TestResultDialogViewModel()
        {
            UploadPath = $@"C:\mapdownload\";
            LotID = "LOT";
            WaferID = "U848A71-07C2";
        }
        #endregion

        #region ==> Command
        private ICommand _OpenPathDialogCommand;
        public ICommand OpenPathDialogCommand
        {
            get
            {
                if (_OpenPathDialogCommand == null)
                    _OpenPathDialogCommand = new RelayCommand(OpenPathDialogFunc);
                return _OpenPathDialogCommand;
            }
        }

        private void OpenPathDialogFunc()
        {
            FolderBrowserDialog folderBroserDialog = new FolderBrowserDialog();

            var result = folderBroserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                var path = folderBroserDialog.SelectedPath;
                this.UploadPath = path;
            }
        }

        private ICommand _DatToProberDataCommand;
        public ICommand DatToProberDataCommand
        {
            get
            {
                if (_DatToProberDataCommand == null)
                    _DatToProberDataCommand = new RelayCommand(DatToProberDataFunc);
                return _DatToProberDataCommand;
            }
        }

        private void DatToProberDataFunc()
        {
            try
            {
                //this.ResultMapModule().DownloadResultMap();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private ICommand _ProberDataToDatCommand;
        public ICommand ProberDataToDatCommand
        {
            get
            {
                if (_ProberDataToDatCommand == null)
                    _ProberDataToDatCommand = new RelayCommand(ProberDataToDatFunc);
                return _ProberDataToDatCommand;
            }
        }

        private void ProberDataToDatFunc()
        {
            try
            {
                this.ResultMapManager().Upload();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private ICommand _SaveRealTimeMapCommand;
        public ICommand SaveRealTimeMapCommand
        {
            get
            {
                if (_SaveRealTimeMapCommand == null) _SaveRealTimeMapCommand = new RelayCommand(SaveRealTimeMapCommandFunc);
                return _TestCommand;
            }
        }

        private void SaveRealTimeMapCommandFunc()
        {
            try
            {
                this.ResultMapManager().SaveRealTimeProbingData();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private ICommand _LoadRealTimeMapCommand;
        public ICommand LoadRealTimeMapCommand
        {
            get
            {
                if (_LoadRealTimeMapCommand == null) _LoadRealTimeMapCommand = new RelayCommand(LoadRealTimeMapCommandFunc);
                return _TestCommand;
            }
        }

        private void LoadRealTimeMapCommandFunc()
        {
            try
            {
                this.ResultMapManager().LoadRealTimeProbingData();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private ICommand _TestCommand;
        public ICommand TestCommand
        {
            get
            {
                if (_TestCommand == null)
                    _TestCommand = new RelayCommand(TestFunc);
                return _TestCommand;
            }
        }

        private void TestFunc()
        {
            try
            {
                // TODO : STIF TEST

                object rm = null;

                this.ResultMapManager().MakeResultMap(ref rm);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        
        #endregion
    }
}
