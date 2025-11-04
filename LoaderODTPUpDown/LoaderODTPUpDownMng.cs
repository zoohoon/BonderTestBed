using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProberInterfaces;
using LogModule;
using ProberInterfaces.ODTP;
using ProberErrorCode;
using Autofac;
using LoaderBase.Communication;
using LoaderBase.LoaderLog;
using SpoolingUtil;
using System.Text.RegularExpressions;
using SpoolingUtil.TransferMethod;
using System.Reflection;
using System.IO;
using NoticeDialog;
using System.Windows;

namespace LoaderODTPUpDown
{
    public class LoaderODTPUpDownMng : ILoaderODTPManager, IFactoryModule, IDisposable, ILoaderFactoryModule
    {
        public enum TransferMethodType
        {
            UsingFTP,
            UsingNetwork_LocalDisk,
            UsingNetwork_NetDrv,
            UsingUnknownType
        }

        public InitPriorityEnum InitPriority { get; set; }

        public bool Initialized { get; set; }


        SpoolingManager spoolingMng;
        NoticeDialog.NoticeWindow noticeDialog;
        NoticeDialogViewModel NoticeDlgVM;
        ODTPManualUploadDlg.MainWindow maualUploadDlg;
        ODTPManualUploadDlg.ODTPDialogVM maualUploadDlgVM;

        public ITransferMethods ITransferMethod { get; set; } ///외부 서버에 전달처리를 하는 Interface

        private ILoaderCommunicationManager LoaderCommunicationManager => this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
        private ILoaderLogManagerModule LoaderLogmanager => this.GetLoaderContainer().Resolve<ILoaderLogManagerModule>(); //ResultMap Upload/Down 옵션이 해당 클래스에서 관리되고 있다.


        private readonly string ftpPattern = @"^ftp://|FTP:// ";
        private readonly string networkPattern = @"^\\(\\(\w+\s*)+)";
        private readonly string localPattern = @"^[A-Z]:(\\(\w+\s*)+)";
        private readonly string engineerModeKeyword = "semics:";
        private readonly string Loaderrootpath = @"C:\ProberSystem\LoaderSystem\EMUL\Parameters\Resultmaps\ODTP\Upload\";
        
        
        public Dictionary<TransferMethodType, ITransferMethods> TransferMethodStorage { get; set;}
        
        
        public LoaderODTPUpDownMng() 
        {
            TransferMethodStorage = new Dictionary<TransferMethodType, ITransferMethods>();
            spoolingMng = new SpoolingManager(this.LoaderLogmanager.LoaderLogParam.SpoolingBasePath.Value, 3);
            spoolingMng.InitSpooling(this.LoaderLogmanager.LoaderLogParam.ODTPUpLoadPath.Value, this.LoaderLogmanager.LoaderLogParam.UserName.Value, this.LoaderLogmanager.LoaderLogParam.Password.Value, false);
            spoolingMng.CallBackFunc = FailedToUploadCallBack;
            
            noticeDialog = new NoticeDialog.NoticeWindow();
            noticeDialog.Topmost = true;
            NoticeDlgVM = noticeDialog.DataContext as NoticeDialogViewModel;
            if (null != NoticeDlgVM)
            {
                NoticeDlgVM.CustomCallBack = ShowManaualUploadDlg;
                NoticeDlgVM.Name = "ODTP";
            }


            maualUploadDlg = new ODTPManualUploadDlg.MainWindow(spoolingMng);
            maualUploadDlg.Topmost = true;
            maualUploadDlgVM = maualUploadDlg.DataContext as ODTPManualUploadDlg.ODTPDialogVM;
        }

        #region ==>  Init & DeInit Function
        public void DeInitModule()
        {
            try
            {
                Initialized = false;
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public EventCodeEnum InitModule(IContainer container)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    Initialized = true;
                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");
                    Initialized = false;
                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public EventCodeEnum InitModule()
        {
            return EventCodeEnum.NONE;
        }
        #endregion

        public void Dispose()
        {
            if (null != spoolingMng)
                spoolingMng.Dispose();

            return;
        }

        public void FailedToUploadCallBack(bool showNoticeInfo)
        {
            ShowFailedNoticeDlg(showNoticeInfo);
        }

        private void ShowFailedNoticeDlg(bool showNoticeInfo)
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (null != noticeDialog)
                    {
                        if ((showNoticeInfo && noticeDialog.Visibility == Visibility.Visible) ||
                               (false == showNoticeInfo && noticeDialog.Visibility == Visibility.Hidden) ||
                               (showNoticeInfo && maualUploadDlg.Visibility == Visibility.Visible)
                            )
                        {
                            return;
                        }

                        if (showNoticeInfo)
                        {
                            noticeDialog.Left = SystemParameters.WorkArea.Left + (SystemParameters.WorkArea.Width - noticeDialog.ActualWidth) / 2;
                            noticeDialog.Top = SystemParameters.WorkArea.Top + (SystemParameters.WorkArea.Height - noticeDialog.ActualHeight) / 2;
                            noticeDialog.Show();
                        }
                        else
                        {
                            noticeDialog.Hide();
                        }
                    }
                }));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private bool SetTransferMethod(string serverpath, out TransferMethodType transferMethod)
        {
            if (Regex.IsMatch(serverpath, ftpPattern)) // ftp 
            {
                if (TransferMethodStorage.ContainsKey(TransferMethodType.UsingFTP))
                    ITransferMethod = TransferMethodStorage[TransferMethodType.UsingFTP];
                else
                {
                    ITransferMethod = new TransferUsingFTP(this.LoaderLogmanager.LoaderLogParam.UserName.Value, this.LoaderLogmanager.LoaderLogParam.Password.Value, this.LoaderLogmanager.LoaderLogParam.FTPUsePassive.Value);
                    TransferMethodStorage.Add(TransferMethodType.UsingFTP, ITransferMethod);
                }

                transferMethod = TransferMethodType.UsingFTP;
                return true;
            }
            else if (Regex.IsMatch(serverpath, localPattern)) //local path
            {
                if (TransferMethodStorage.ContainsKey(TransferMethodType.UsingNetwork_LocalDisk))
                    ITransferMethod = TransferMethodStorage[TransferMethodType.UsingNetwork_LocalDisk];
                else
                {
                    ITransferMethod = new TransferUsingLocalDisk();
                    TransferMethodStorage.Add(TransferMethodType.UsingNetwork_LocalDisk, ITransferMethod);
                }

                transferMethod = TransferMethodType.UsingNetwork_LocalDisk;
                return true;
            }
            else if (Regex.IsMatch(serverpath, networkPattern))
            {
                // to-do ?
                LoggerManager.Debug($"[ODTPUpDown] {MethodBase.GetCurrentMethod().Name} : Upload path networkType(not supported). path ={serverpath}");
            }
            else
            {
                LoggerManager.Debug($"[ODTPUpDown] {MethodBase.GetCurrentMethod().Name} : Upload path is wrong. path ={serverpath}");

            }

            transferMethod = TransferMethodType.UsingUnknownType;
            return false;
        }
        private bool CheckEngineerMode(string accessPath)
        {
            if (accessPath.Length >= 7)
            {
                string checkStr = accessPath.Substring(0, 7);
                if (checkStr.ToUpper() == engineerModeKeyword.ToUpper())
                {
                    LoggerManager.Debug($"[ODTPUpDown] Path is Engineer mode");
                    return true;
                }
            }

            return false;
        }
        private (bool, string) MakeODTPPathForRecvCell(int stageindex, string filename)
        {
            string foldername = filename.Substring(0, 12);
            string localDirectory = Path.Combine(Loaderrootpath, foldername);
            string localpath = Path.Combine(Loaderrootpath, foldername, filename);

            bool success = true;
            if (false == Directory.Exists(localDirectory))
            {
                try
                {
                    Directory.CreateDirectory(localDirectory);
                }
                catch (Exception err)
                {
                    success = false;
                    LoggerManager.Error($"[ODTPUpDown] {MethodBase.GetCurrentMethod().Name} : Failed to create directory about ODTP. Err = {err.Message})");
                }
            }
            else
            {
                if (File.Exists(localpath))
                {
                    File.Delete(localpath);
                }
            }

            return (success, localpath);
        }
        private bool GetODTPFileData(int stageindex, string filename, string ODTPFilePath)
        {
            bool success = false;
            var cell = this.LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>(stageindex);
            if (null != cell)
            {
                try
                {
                    File.WriteAllBytes(ODTPFilePath, cell.GetODTPdataFromFileName(filename));
                    success = true;
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"[ODTPUpDown] {MethodBase.GetCurrentMethod().Name} : Failed to write Result Map file data. Err = {err.Message}");
                }
            }
            else
            {
                LoggerManager.Error($"[ODTPUpDown] {MethodBase.GetCurrentMethod().Name} : Failed to get stage proxy(stage idx:{stageindex})");
            }
            return success;
        }

        public EventCodeEnum ServerODTPPathCheck(string foldername)
        {
            try
            {
                string serverpath = this.LoaderLogmanager.LoaderLogParam.ODTPUpLoadPath.Value;
                if (serverpath == null || serverpath == string.Empty || CheckEngineerMode(serverpath))
                    return EventCodeEnum.NONE;

                TransferMethodType transferMethod;
                if (false == SetTransferMethod(serverpath, out transferMethod))
                {
                    LoggerManager.Error($"[ODTPUpDown] {MethodBase.GetCurrentMethod().Name} - can't select method( path ={serverpath})");
                    return EventCodeEnum.RESULTMAP_UNKNOW_TRANSFER_METHOD_FAIL;
                }

                /// 3. download file and send to cell
                if (null != ITransferMethod)
                {
                    string user_name = this.LoaderLogmanager.LoaderLogParam.UserName.Value;
                    string password = this.LoaderLogmanager.LoaderLogParam.Password.Value;

                    if (ITransferMethod.PathCheck(serverpath, foldername, user_name, password))
                    {
                        return EventCodeEnum.NONE;
                    }
                    else
                        return EventCodeEnum.RESULTMAP_FOLDER_NOT_EXIST;
                }
                else
                {
                    LoggerManager.Error($"[ODTPUpDown] {MethodBase.GetCurrentMethod().Name} - Transfer method is null");
                    return EventCodeEnum.UNKNOWN_EXCEPTION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return EventCodeEnum.UNKNOWN_EXCEPTION;
            }
        }

        public EventCodeEnum CellODTPUploadToServer(int stageindex, string filename)
        {
            try
            {
                var task = Task<EventCodeEnum>.Run(() =>
                {

                    LoggerManager.Debug($"[ODTPUpDown] {MethodBase.GetCurrentMethod().Name} : Get ODTP file from cell(stage idx:{stageindex}, fileName:{filename})");
                    /// 1. create dir and getting path
                    (bool success, string ODTPFilePath) = MakeODTPPathForRecvCell(stageindex, filename);
                    if (false == success)
                    {
                        LoggerManager.Debug($"[ODTPUpDown] {MethodBase.GetCurrentMethod().Name} : Failed to proc 'MakeODTPPathForRecvCell'(stage idx:{stageindex}, fileName:{filename})");
                        return EventCodeEnum.RESULTMAP_MAKE_PATH_FAIL;
                    }

                    /// 2. get result map file from cell 
                    success = GetODTPFileData(stageindex, filename, ODTPFilePath);
                    if (false == success)
                    {
                        LoggerManager.Debug($"[ODTPUpDown] {MethodBase.GetCurrentMethod().Name} : Failed to proc 'GetODTPFileData'(stage idx:{stageindex}, fileName:{filename})");
                        return EventCodeEnum.RESULTMAP_GET_MAPFILE_FAIL;
                    }


                    /// 3. Check the path(also check 'Engineer mode')
                    string serverpath = this.LoaderLogmanager.LoaderLogParam.ODTPUpLoadPath.Value;
                    string path = string.Empty;
                    if (serverpath == null || serverpath == string.Empty || CheckEngineerMode(serverpath))
                    {
                        LoggerManager.Debug($"[ODTPUpDown] {MethodBase.GetCurrentMethod().Name} : server path is empty or EngineerMode(path={serverpath})");
                        return EventCodeEnum.NONE;
                    }

                    /// 4. Select transfer method                     
                    TransferMethodType transferMethod;
                    if (false == SetTransferMethod(serverpath, out transferMethod))
                    {
                        LoggerManager.Debug($"[ODTPUpDown] {MethodBase.GetCurrentMethod().Name} : Faile to proc 'SetTransferMethod'(path={serverpath})");
                        return EventCodeEnum.RESULTMAP_UNKNOW_TRANSFER_METHOD_FAIL;
                    }

                    if (TransferMethodType.UsingFTP == transferMethod) //ftp 옵션 설정(Resultmap Upload 시 기존 설정)
                    {
                        TransferUsingFTP transferFtp = ITransferMethod as TransferUsingFTP;
                        if (null != transferFtp)
                        {
                            transferFtp.FtpFunctionObj.UseBinary = false;
                            transferFtp.FtpFunctionObj.Timeout = 30000;
                            transferFtp.FtpFunctionObj.ReadWriteStreamTimeout = 30000;
                        }
                    }

                    /// 5. upload file to server
                    if (null != ITransferMethod)
                    {
                        string user_name = this.LoaderLogmanager.LoaderLogParam.UserName.Value;
                        string password = this.LoaderLogmanager.LoaderLogParam.Password.Value;

                        if (ITransferMethod.UploadFileData(serverpath, ODTPFilePath, filename, user_name, password, true))
                        {
                            LoggerManager.Debug($"[ODTPUpDown] {MethodBase.GetCurrentMethod().Name} : Upload sucess(server path={serverpath}, targetFile={ODTPFilePath}, user={user_name}, pwd={password})");
                            return EventCodeEnum.NONE;
                        }
                        else
                        {
                            LoggerManager.Debug($"[ODTPUpDown] {MethodBase.GetCurrentMethod().Name} : Faile to Upload (server path={serverpath}, targetFile={ODTPFilePath})");

                            spoolingMng.AddToItemInSpooling(ODTPFilePath, filename, stageindex, false);
                            ShowFailedNoticeDlg(true);

                            return EventCodeEnum.RESULTMAP_UPLOAD_FAIL;
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"[ODTPUpDown] {MethodBase.GetCurrentMethod().Name} : Failed to set transfer mode(ITransferMethod mode is null)");
                        return EventCodeEnum.UNKNOWN_EXCEPTION;
                    }
                });
                task.Wait();
                return task.Result;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"{MethodBase.GetCurrentMethod().Name} - Error occurred. Err = {err.Message}");
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.UNKNOWN_EXCEPTION;
        }

        public void ShowManaualUploadDlg()
        {
            try 
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (null != maualUploadDlg)
                    {
                        if (maualUploadDlg.Visibility == Visibility.Visible)
                        {
                            return;
                        }
                        if (null != maualUploadDlgVM)
                        {
                            maualUploadDlg.Left = SystemParameters.WorkArea.Left + (SystemParameters.WorkArea.Width - maualUploadDlg.ActualWidth) / 2;
                            maualUploadDlg.Top = SystemParameters.WorkArea.Top + (SystemParameters.WorkArea.Height - maualUploadDlg.ActualHeight) / 2;
                            maualUploadDlgVM.ShowMaualUploadDlg();
                        }

                    }
                }));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SpoolingOptionUpdate(string upload_basePath, string username, string password,
                                        bool ftp_userpassive = false, int retry_count = 3, int reupload_delayTm =5)
        {
            if (null != spoolingMng)
                spoolingMng.UpdateSpoolingOption(upload_basePath, username, password, ftp_userpassive, retry_count, reupload_delayTm);
        }

    }
}
