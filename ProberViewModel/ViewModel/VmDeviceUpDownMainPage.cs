using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeviceUpDownMainPageViewModel
{
    using System.ComponentModel;
    using ProberErrorCode;
    using ProberInterfaces;
    using RelayCommandBase;
    using System.Windows.Input;
    using DeviceUpDownControl;
    using LogModule;
    using System.Runtime.CompilerServices;
    using System.Windows.Media.Imaging;
    using MetroDialogInterfaces;

    public class VmDeviceUpDownMainPage : IMainScreenViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ==> RefreshLocalCommand
        private RelayCommand _RefreshLocalCommand;
        public ICommand RefreshLocalCommand
        {
            get
            {
                if (null == _RefreshLocalCommand) _RefreshLocalCommand = new RelayCommand(RefreshLocalCommandFunc);
                return _RefreshLocalCommand;
            }
        }
        private void RefreshLocalCommandFunc()
        {
            try
            {
                List<String> deviceNameList = this.DeviceUpDownManager()?.GetLocalDeviceNameList();
                LocalDeviceExplorer.SetDeviceItemDataSource(deviceNameList);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> RefreshServerCommand
        private AsyncCommand _RefreshServerCommand;
        public ICommand RefreshServerCommand
        {
            get
            {
                if (null == _RefreshServerCommand) _RefreshServerCommand = new AsyncCommand(RefreshServerCommandFunc);
                return _RefreshServerCommand;
            }
        }
        private async Task RefreshServerCommandFunc()
        {
            try
            {
                if (CheckIPPortConnection() == false)
                {
                    return;
                }

                if (DownloadFolderEnable)
                {
                    this.DeviceUpDownManager().ChangeUploadDirectory();
                }
                else
                {
                    this.DeviceUpDownManager().ChangeDownloadDirectory();
                }

                List<String> deviceNameList;
                EnumDeviceUpDownErr result = this.DeviceUpDownManager().GetServerDeviceNameList(out deviceNameList);
                if (result != EnumDeviceUpDownErr.NONE)
                {

                    this.MetroDialogManager().ShowMessageDialog("Device Up/Down", "[FAIL] Getting file list from server ", EnumMessageStyle.Affirmative);
                }

                ServerDeviceExplorer.SetDeviceItemDataSource(deviceNameList);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> CheckConnectionCommand
        private AsyncCommand _CheckConnectionCommand;
        public ICommand CheckConnectionCommand
        {
            get
            {
                if (null == _CheckConnectionCommand) _CheckConnectionCommand = new AsyncCommand(CheckConnectionCommandFunc);
                return _CheckConnectionCommand;
            }
        }
        private async Task CheckConnectionCommandFunc()
        {
            try
            {
                bool bCheckResult = CheckIPPortConnection();
                String message = String.Empty;

                if (bCheckResult)
                {
                    message = "Connection is OK!!!";
                }
                else
                {
                    message = "IP/PORT Invalid, Check IP and Port";
                }

                this.MetroDialogManager().ShowMessageDialog("Server Connection Test", message, EnumMessageStyle.Affirmative);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> UploadFolderCommand
        private RelayCommand _UploadFolderCommand;
        public ICommand UploadFolderCommand
        {
            get
            {
                if (null == _UploadFolderCommand) _UploadFolderCommand = new RelayCommand(UploadFolderCommandFunc);
                return _UploadFolderCommand;
            }
        }
        private void UploadFolderCommandFunc()
        {
            try
            {
                UploadFolderEnable = false;
                DownloadFolderEnable = true;

                LoadButtonIcon = _UploadIconURI;

                this.DeviceUpDownManager()?.ChangeUploadDirectory();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> DownloadFolderCommand
        private RelayCommand _DownloadFolderCommand;
        public ICommand DownloadFolderCommand
        {
            get
            {
                if (null == _DownloadFolderCommand) _DownloadFolderCommand = new RelayCommand(DownloadFolderCommandFunc);
                return _DownloadFolderCommand;
            }
        }
        private void DownloadFolderCommandFunc()
        {
            try
            {
                UploadFolderEnable = true;
                DownloadFolderEnable = false;

                LoadButtonIcon = _DownloadIconURI;

                this.DeviceUpDownManager().ChangeDownloadDirectory();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> LoadCommand
        private AsyncCommand _LoadCommand;
        public ICommand LoadCommand
        {
            get
            {
                if (null == _LoadCommand) _LoadCommand = new AsyncCommand(LoadCommandFunc);
                return _LoadCommand;
            }
        }
        private async Task LoadCommandFunc()
        {
            try
            {
                if (DownloadFolderEnable)
                    await Upload();
                else
                    await Download();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private async Task Download()
        {
            try
            {
                do
                {
                    //==> 다운로드할 Device 선택 하였는지 확인
                    if (ServerDeviceExplorer.SelectedDeviceItemVM == null)
                    {

                        this.MetroDialogManager().ShowMessageDialog("Device Up/Down", "Download Device is not selected", EnumMessageStyle.Affirmative);

                        break;
                    }

                    String downloadDevName = ServerDeviceExplorer.SelectedDeviceItemVM.DeviceName;

                    if (CheckIPPortConnection() == false)
                    {
                        break;
                    }

                    //==> 다운로드 하려는 Device가 Server에 존재하는지 확인
                    bool isExists;
                    EnumDeviceUpDownErr existsCheckResult = this.DeviceUpDownManager().CheckDeviceExistInServer(downloadDevName, out isExists);
                    if (existsCheckResult != EnumDeviceUpDownErr.NONE)
                    {
                        this.MetroDialogManager().ShowMessageDialog("Device Up/Down", "Server reply error, check server", EnumMessageStyle.Affirmative);

                        break;
                    }
                    if (isExists == false)
                    {

                        this.MetroDialogManager().ShowMessageDialog("Device Up/Down", "Device is not exists, Reflesh server directory", EnumMessageStyle.Affirmative);

                        break;
                    }

                    //==> 이미 같은 이름의 Device가 Local에 존재하는지 확인
                    if (this.DeviceUpDownManager().CheckDeviceExistInLocal(downloadDevName))
                    {

                        EnumMessageDialogResult msgRes = await this.MetroDialogManager().ShowMessageDialog("Device Up/Down", "Same device is aready exists. Are you overwrite device?", EnumMessageStyle.Affirmative);

                        if (msgRes == EnumMessageDialogResult.NEGATIVE)
                            break;

                        //==> 같은 이름의 Device 삭제함.
                        this.DeviceUpDownManager().DeleteDeviceInLocal(downloadDevName);
                    }

                    //==> 다운로드
                    EnumDeviceUpDownErr result = EnumDeviceUpDownErr.NONE;
                    using (DataTransferProgress progress = new DataTransferProgress($"Download... {downloadDevName}", true))
                    {
                        result = this.DeviceUpDownManager().DownloadDevice(downloadDevName, progress.DataTransferEvent);
                    }

                    //this.WaitCancelDialogService().CloseDialog();
                    this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());

                    if (result == EnumDeviceUpDownErr.NONE)
                    {

                        this.MetroDialogManager().ShowMessageDialog("Device Up/Down", $"{downloadDevName} : Download Success", EnumMessageStyle.Affirmative);

                        LoggerManager.Debug($"{downloadDevName} : Download Success");
                    }
                    else
                    {

                        this.MetroDialogManager().ShowMessageDialog("Device Up/Down", $"{downloadDevName} : Download [FAIL], {result}", EnumMessageStyle.Affirmative);

                        LoggerManager.Debug($"{downloadDevName} : Download [FAIL], {result}");
                    }

                } while (false);

                RefreshLocalCommandFunc();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private async Task Upload()
        {
            try
            {
                do
                {
                    //==> 업로드할 Device 선택 하였는지 확인
                    if (LocalDeviceExplorer.SelectedDeviceItemVM == null)
                    {

                        this.MetroDialogManager().ShowMessageDialog("Device Up/Down", "Upload Device is not Selected", EnumMessageStyle.Affirmative);

                        break;
                    }

                    String uploadDevName = LocalDeviceExplorer.SelectedDeviceItemVM.DeviceName;

                    if (CheckIPPortConnection() == false)
                    {
                        break;
                    }

                    //==> 업로드 하려는 Device가 Local에 존재하는지 확인
                    if (this.DeviceUpDownManager().CheckDeviceExistInLocal(uploadDevName) == false)
                    {

                        this.MetroDialogManager().ShowMessageDialog("Device Up/Down", "Device is not exists, Reflesh Local directory", EnumMessageStyle.Affirmative);

                        break;
                    }

                    //==> 업로드 하려는 Device가 Server에 존재하는지 확인
                    bool isExists;
                    EnumDeviceUpDownErr existsCheckResult = this.DeviceUpDownManager().CheckDeviceExistInServer(uploadDevName, out isExists);
                    if (existsCheckResult != EnumDeviceUpDownErr.NONE)
                    {

                        this.MetroDialogManager().ShowMessageDialog("Device Up/Down", "Checking device exist from server is error", EnumMessageStyle.Affirmative);

                        break;
                    }
                    if (isExists)
                    {

                        EnumMessageDialogResult msgRes = await this.MetroDialogManager().ShowMessageDialog("Device Up/Down", "Same device is aready exists. Are you overwrite device?", EnumMessageStyle.Affirmative);

                        if (msgRes == EnumMessageDialogResult.NEGATIVE)
                            break;
                    }

                    //==> 업로드
                    EnumDeviceUpDownErr result = EnumDeviceUpDownErr.NONE;
                    using (DataTransferProgress progress = new DataTransferProgress($"Upload... {uploadDevName}", true))
                    {
                        result = this.DeviceUpDownManager().UploadDevice(uploadDevName, progress.DataTransferEvent);
                    }

                    if (result == EnumDeviceUpDownErr.NONE)
                    {

                        this.MetroDialogManager().ShowMessageDialog("Device Up/Down", $"{uploadDevName} : Upload Success", EnumMessageStyle.Affirmative);

                        LoggerManager.Debug($"{uploadDevName} : Upload Success");
                    }
                    else
                    {

                        this.MetroDialogManager().ShowMessageDialog("Device Up/Down", $"{uploadDevName} : Upload [FAIL], {result}", EnumMessageStyle.Affirmative);

                        LoggerManager.Debug($"{uploadDevName} : Upload [FAIL], {result}");
                    }

                } while (false);

                RefreshLocalCommandFunc();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        class DataTransferProgress : IFactoryModule, IDisposable
        {
            public DataTransferProgress(String message, bool isSetProgress)
            {
                try
                {
                    //this.WaitCancelDialogService().ShowDialog(message);
                    this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), message);

                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            public bool DataTransferEvent(long totalTransfer, long totalData)
            {
                try
                {
                    if (totalData < 1)
                        return false;

                    double progreeVal = ((double)totalTransfer / totalData) * 100;

                    if (progreeVal < 1)
                        return false;

                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }

                return true;
            }
            public void Dispose()
            {
                try
                {
                    //this.WaitCancelDialogService().CloseDialog().Wait();
                    this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString()).Wait();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
        }
        #endregion

        #region ==> UploadFolderEnable
        private bool _UploadFolderEnable;
        public bool UploadFolderEnable
        {
            get { return _UploadFolderEnable; }
            set
            {
                if (value != _UploadFolderEnable)
                {
                    _UploadFolderEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> DownloadFolderEnable
        private bool _DownloadFolderEnable;
        public bool DownloadFolderEnable
        {
            get { return _DownloadFolderEnable; }
            set
            {
                if (value != _DownloadFolderEnable)
                {
                    _DownloadFolderEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> LoadButtonContent
        private String _LoadButtonContent;
        public String LoadButtonContent
        {
            get { return _LoadButtonContent; }
            set
            {
                if (value != _LoadButtonContent)
                {
                    _LoadButtonContent = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> LoadButtonIcon
        private BitmapImage _LoadButtonIcon;
        public BitmapImage LoadButtonIcon
        {
            get { return _LoadButtonIcon; }
            set
            {
                if (value != _LoadButtonIcon)
                {
                    _LoadButtonIcon = value;
                    RaisePropertyChanged();
                }
            }
        }
        private readonly BitmapImage _UploadIconURI = new BitmapImage(new Uri("pack://application:,,,/ImageResourcePack;component/Images/chevron-right.png", UriKind.Absolute));
        private readonly BitmapImage _DownloadIconURI = new BitmapImage(new Uri("pack://application:,,,/ImageResourcePack;component/Images/chevron-left.png", UriKind.Absolute));
        #endregion

        readonly Guid _ViewModelGUID = new Guid("C8855847-E34E-4651-878F-0E4A3875A02E");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        public bool Initialized { get; set; } = false;

        //==> UcDeviceExplorer(User Contorl) 와 Binding 되어 있음
        public DeviceExplorerViewModel LocalDeviceExplorer { get; set; }
        public DeviceExplorerViewModel ServerDeviceExplorer { get; set; }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    LocalDeviceExplorer = new DeviceExplorerViewModel("pack://application:,,,/ImageResourcePack;component/Images/FolderIcon.png");
                    ServerDeviceExplorer = new DeviceExplorerViewModel("pack://application:,,,/ImageResourcePack;component/Images/ZipIcon.png");

                    LoadButtonIcon = _UploadIconURI;

                    RefreshLocalCommandFunc();
                    UploadFolderCommandFunc();

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
        public Task<EventCodeEnum> InitViewModel()
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"DeInitViewModel() in {GetType().Name}");

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }
        public void DeInitModule()
        {
        }
        private bool CheckIPPortConnection()
        {
            bool bCheckResult = false;

            try
            {

                using (DataTransferProgress progress = new DataTransferProgress($"Network IP/PORT Checking...", false))
                {
                    bCheckResult = this.DeviceUpDownManager().CheckConnection();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return bCheckResult;
        }
    }

}