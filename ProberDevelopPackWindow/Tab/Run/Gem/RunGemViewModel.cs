namespace ProberDevelopPackWindow.Tab.Run.Gem
{
    using LogModule;
    using ProberInterfaces;
    using RelayCommandBase;
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq.Expressions;
    using Newtonsoft.Json;
    using System.Runtime.Remoting.Messaging;
    using SecsGemServiceInterface;
    using WizardCategoryView.Converter;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Forms;
    using Microsoft.VisualBasic;
    using System.Runtime.InteropServices.ComTypes;

    public class RunGemViewModel : INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private string rootFolderPath = @"C:\ProberSystem\RunGEM";
        private string DefaultfileName = "DefaultFileName.json";

        private ObservableCollection<FolderItemViewModel> _folders;
        public ObservableCollection<FolderItemViewModel> Folders
        {
            get { return _folders; }
            set
            {
                _folders = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<SecsMessage> Messages { get; set; } = new ObservableCollection<SecsMessage>();

        private SecsMessage _SelectedSecsMessage;
        public SecsMessage SelectedSecsMessage
        {
            get
            {
                return _SelectedSecsMessage;
            }
            set
            {
                if (_SelectedSecsMessage != value)
                {
                    _SelectedSecsMessage = value;
                    RaisePropertyChanged();

                    // Update SelectedSecsMessageText with the string representation of the selected message
                    SelectedSecsMessageText = value != null ? SecsMessage.ToSecsMessageString(value) : string.Empty;
                }
            }
        }

        private string _SelectedSecsMessageText;
        public string SelectedSecsMessageText
        {
            get
            {
                return _SelectedSecsMessageText;
            }
            set
            {
                _SelectedSecsMessageText = value;
                RaisePropertyChanged();
            }
        }

        private string _SelectedFilePath;
        public string SelectedFilePath
        {
            get
            {
                return _SelectedFilePath;
            }
            set
            {
                _SelectedFilePath = value;

                LoadParametersFromJsonCommandFunc(value);

                RaisePropertyChanged();
            }
        }


        private ObservableCollection<EnumRemoteActionItem> _GemActlist;
        public ObservableCollection<EnumRemoteActionItem> GemActlist
        {
            get
            {
                return _GemActlist;
            }
            set
            {
                _GemActlist = value;
                RaisePropertyChanged();
            }
        }

        private EnumRemoteActionItem _SelectedGemAct;
        public EnumRemoteActionItem SelectedGemAct
        {
            get
            {
                return _SelectedGemAct;
            }
            set
            {
                _SelectedGemAct = value;
                RaisePropertyChanged();
            }
        }

        private bool _GemActlistVisibility = false;
        public bool GemActlistVisibility
        {
            get
            {
                return _GemActlistVisibility;
            }
            set
            {
                _GemActlistVisibility = value;
                RaisePropertyChanged();
            }
        }

        private bool _UseEditedMessage = false;
        public bool UseEditedMessage
        {
            get
            {
                return _UseEditedMessage;
            }
            set
            {
                _UseEditedMessage = value;
                RaisePropertyChanged();
            }
        }


        public RunGemViewModel()
        {

        }

        public void UpdateSelectedSecsMessageText(string newText)
        {
            _SelectedSecsMessageText = newText;
            RaisePropertyChanged(nameof(SelectedSecsMessageText));
        }

        public void InitViewModel()
        {
            try
            {
                // 초기 파일 경로 로드
                LoadRootFolder();

                GemActlist = GetCombinedEnumList();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void LoadRootFolder()
        {
            if (!Directory.Exists(rootFolderPath))
            {
                Directory.CreateDirectory(rootFolderPath);
            }

            var rootFolder = new FolderItemViewModel(rootFolderPath, true); // 루트 폴더로 시작
            Folders = new ObservableCollection<FolderItemViewModel> { rootFolder };

            // JSON 파일 존재 여부 확인
            if (!rootFolder.HasJsonFiles())
            {
                // JSON 파일이 없음. 여기에 처리 로직 추가 (예: 메시지 표시)
                MakeDefaultCommandSet();

                string newFilePath = Path.Combine(rootFolderPath, DefaultfileName);

                rootFolder.AddChild(newFilePath, false);
            }
        }

        private void MakeDefaultCommandSet()
        {
            try
            {
                if (Messages.Count > 0)
                {
                    Messages.Clear();
                }

                Messages.Add(MakeDefaultMessage(EnumCarrierAction.PROCEEDWITHCARRIER));
                Messages.Add(MakeDefaultMessage(EnumCarrierAction.PROCEEDWITHSLOT));
                Messages.Add(MakeDefaultMessage(EnumCarrierAction.CANCELCARRIER));

                Messages.Add(MakeDefaultMessage(EnumRemoteCommand.PSTART));
                Messages.Add(MakeDefaultMessage(EnumRemoteCommand.TC_START));
                Messages.Add(MakeDefaultMessage(EnumRemoteCommand.STAGE_SLOT));
                Messages.Add(MakeDefaultMessage(EnumRemoteCommand.WFIDCONFPROC));
                Messages.Add(MakeDefaultMessage(EnumRemoteCommand.DLRECIPE));
                Messages.Add(MakeDefaultMessage(EnumRemoteCommand.RESUME));
                Messages.Add(MakeDefaultMessage(EnumRemoteCommand.UNDOCK));
                Messages.Add(MakeDefaultMessage(EnumRemoteCommand.ZUP));
                Messages.Add(MakeDefaultMessage(EnumRemoteCommand.TESTEND));
                Messages.Add(MakeDefaultMessage(EnumRemoteCommand.PABORT));
                Messages.Add(MakeDefaultMessage(EnumRemoteCommand.START_CARD_CHANGE));
                Messages.Add(MakeDefaultMessage(EnumRemoteCommand.MOVEIN_CARD_CLOSE_COVER));
                Messages.Add(MakeDefaultMessage(EnumRemoteCommand.TC_END));
                Messages.Add(MakeDefaultMessage(EnumRemoteCommand.PROCEED_CARD_CHANGE));
                Messages.Add(MakeDefaultMessage(EnumRemoteCommand.SKIP_CARD_ATTACH));
                Messages.Add(MakeDefaultMessage(EnumRemoteCommand.WAFER_CHANGE));

                SaveParametersToJsonCommandFunc(DefaultfileName);

                Messages.Clear();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private SecsMessage MakeDefaultMessage(EnumRemoteCommand command)
        {
            SecsMessage retval = null;

            try
            {
                string inputData = RunGemRemoteMessage.GetDefaultMessage(command);

                retval = SecsMessage.Parse(inputData);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private SecsMessage MakeDefaultMessage(EnumCarrierAction command)
        {
            SecsMessage retval = null;

            try
            {
                string inputData = RunGemRemoteMessage.GetDefaultMessage(command);

                retval = SecsMessage.Parse(inputData);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public ObservableCollection<EnumRemoteActionItem> GetCombinedEnumList()
        {
            ObservableCollection<EnumRemoteActionItem> retval = new ObservableCollection<EnumRemoteActionItem>();
            List<EnumRemoteActionItem> tempList = new List<EnumRemoteActionItem>();

            try
            {
                // EnumCarrierAction 항목 추가
                foreach (var action in Enum.GetValues(typeof(EnumCarrierAction)).Cast<EnumCarrierAction>())
                {
                    tempList.Add(new EnumRemoteActionItem { Name = action.ToString(), IsIncluded = false });
                }

                // EnumRemoteCommand 항목 추가
                foreach (var command in Enum.GetValues(typeof(EnumRemoteCommand)).Cast<EnumRemoteCommand>())
                {
                    tempList.Add(new EnumRemoteActionItem { Name = command.ToString(), IsIncluded = false });
                }

                // IsIncluded 상태 업데이트
                foreach (var item in tempList)
                {
                    item.IsIncluded = this.GEMModule().CanExecuteRemoteAction(item.Name);
                }

                // IsIncluded가 true인 항목을 상위로, 그렇지 않은 항목은 하위로 정렬
                tempList.Sort((x, y) => y.IsIncluded.CompareTo(x.IsIncluded));

                // 정렬된 리스트를 ObservableCollection에 추가
                foreach (var item in tempList)
                {
                    retval.Add(item);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private RelayCommand<object> _EditDescriptionCommand;
        public ICommand EditDescriptionCommand
        {
            get
            {
                if (null == _EditDescriptionCommand) _EditDescriptionCommand = new RelayCommand<object>(EditDescription);
                return _EditDescriptionCommand;
            }
        }

        private void EditDescription(object obj)
        {
            try
            {
                // Assuming you have a method to show the popup and return the new description
                var newDescription = ShowEditDescriptionPopup(SelectedSecsMessage.Description);

                if (newDescription != null)
                {
                    SelectedSecsMessage.Description = newDescription;
                    RaisePropertyChanged(nameof(Messages)); // Refresh the list if necessary

                    SaveParametersToJsonCommandFunc(SelectedFilePath);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private string ShowEditDescriptionPopup(string currentDescription)
        {
            string retval = null;

            try
            {
                var editWindow = new EditDescriptionWindow(currentDescription);
                editWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;

                var result = editWindow.ShowDialog();

                if (result == true)
                {
                    retval = editWindow.Description;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }


        private RelayCommand<object> _CreateFolderCommand;
        public ICommand CreateFolderCommand
        {
            get
            {
                if (null == _CreateFolderCommand) _CreateFolderCommand = new RelayCommand<object>(CreateFolder);
                return _CreateFolderCommand;
            }
        }

        private void CreateFolder(object obj)
        {
            string basePath = ((FolderItemViewModel)obj).Path;
            string newFolderBaseName = "New Folder";
            string newFolderPath = Path.Combine(basePath, newFolderBaseName);
            int folderIndex = 1;

            // 동일한 이름이 있으면 새로운 이름 생성
            while (Directory.Exists(newFolderPath))
            {
                newFolderPath = Path.Combine(basePath, $"{newFolderBaseName} ({folderIndex++})");
            }

            Directory.CreateDirectory(newFolderPath);
            ((FolderItemViewModel)obj).Children.Add(new FolderItemViewModel(newFolderPath, true));
        }

        private RelayCommand<object> _DeleteFolderCommand;
        public ICommand DeleteFolderCommand
        {
            get
            {
                if (null == _DeleteFolderCommand) _DeleteFolderCommand = new RelayCommand<object>(DeleteFolder);
                return _DeleteFolderCommand;
            }
        }

        private void DeleteFolder(object obj)
        {
            // Implementation for deleting a folder
            var folderPath = ((FolderItemViewModel)obj).Path;

            // TODO : RootPath의 경우 삭제되면 안됨.

            if (folderPath != rootFolderPath)
            {
                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath, true);
                    var parent = Folders.FirstOrDefault(f => f.Children.Contains((FolderItemViewModel)obj));
                    parent?.Children.Remove((FolderItemViewModel)obj);
                }
            }
        }

        private RelayCommand<object> _CreateFileCommand;
        public ICommand CreateFileCommand
        {
            get
            {
                if (null == _CreateFileCommand) _CreateFileCommand = new RelayCommand<object>(CreateFile);
                return _CreateFileCommand;
            }
        }

        private void CreateFile(object obj)
        {
            try
            {
                if (obj is FolderItemViewModel item)
                {
                    if (item.IsFolder == false)
                    {
                        string basePath = ((FolderItemViewModel)obj).Path;
                        string newFileBaseName = "NewFile.json";
                        string newFilePath = Path.Combine(basePath, newFileBaseName);
                        int fileIndex = 1;

                        // 파일 이름이 이미 존재하면 고유한 이름 생성
                        while (File.Exists(newFilePath))
                        {
                            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(newFileBaseName);
                            string extension = Path.GetExtension(newFileBaseName);
                            newFilePath = Path.Combine(basePath, $"{fileNameWithoutExtension} ({fileIndex++}){extension}");
                        }

                        using (var fs = File.Create(newFilePath)) { } ((FolderItemViewModel)obj).Children.Add(new FolderItemViewModel(newFilePath, false));
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _DeleteFileCommand;
        public ICommand DeleteFileCommand
        {
            get
            {
                if (null == _DeleteFileCommand) _DeleteFileCommand = new RelayCommand<object>(DeleteFile);
                return _DeleteFileCommand;
            }
        }

        private void DeleteFile(object obj)
        {
            var item = (FolderItemViewModel)obj;
            var filePath = item.Path;
            if (File.Exists(filePath))
            {
                string messageBoxText = "Do you really want to delete this file?";
                string caption = "File Deletion Confirmation";
                MessageBoxButton button = MessageBoxButton.YesNo;
                MessageBoxImage icon = MessageBoxImage.Warning;
                // Display message box
                MessageBoxResult result = System.Windows.MessageBox.Show(messageBoxText, caption, button, icon);

                // Process message box results
                if (result == MessageBoxResult.Yes)
                {
                    File.Delete(filePath);

                    var parent = FindParent(Folders, item);

                    if (parent != null)
                    {
                        parent.Children.Remove(item);
                    }
                    else
                    {
                        // 처리할 수 없는 상황 또는 로그 남기기
                    }
                }
            }
        }

        private FolderItemViewModel FindParent(ObservableCollection<FolderItemViewModel> folders, FolderItemViewModel child)
        {
            foreach (var folder in folders)
            {
                if (folder.Children.Contains(child))
                {
                    return folder;
                }
                else
                {
                    var found = FindParent(folder.Children, child);
                    if (found != null) return found;
                }
            }
            return null; // 부모를 찾지 못한 경우
        }

        private RelayCommand<object> _RenameCommand;
        public ICommand RenameCommand
        {
            get
            {
                if (null == _RenameCommand) _RenameCommand = new RelayCommand<object>(RenameItem);
                return _RenameCommand;
            }
        }


        private void RenameItem(object obj)
        {
            if (obj is FolderItemViewModel item)
            {
                string currentName = item.Name;
                string message = "Enter a new name:";
                string caption = "Rename";
                string defaultValue = currentName; // 기본값은 현재 이름으로 설정합니다.
                string userInput = Interaction.InputBox(message, caption, defaultValue);

                // 사용자 입력이 유효하고 현재 이름과 다른 경우에만 처리합니다.
                if (!string.IsNullOrWhiteSpace(userInput) && userInput != currentName)
                {
                    string parentPath = Path.GetDirectoryName(item.Path);
                    // 파일인 경우 확장자가 없으면 자동으로 .json 추가
                    string newName = userInput;
                    if (!item.IsFolder && !Path.HasExtension(newName))
                    {
                        newName += ".json"; // 확장자가 없는 경우 .json 추가
                    }
                    string newPath = Path.Combine(parentPath, newName);

                    try
                    {
                        if (item.IsFolder)
                        {
                            // 새 폴더 이름이 이미 존재하지 않는지 확인
                            if (!Directory.Exists(newPath))
                            {
                                Directory.Move(item.Path, newPath);
                                item.Path = newPath; // Path 속성을 새 이름으로 업데이트
                            }
                            else
                            {
                                System.Windows.MessageBox.Show("A folder with that name already exists.", "Error");
                            }
                        }
                        else
                        {
                            // 새 파일 이름이 이미 존재하지 않는지 확인
                            if (!File.Exists(newPath))
                            {
                                File.Move(item.Path, newPath);
                                item.Path = newPath; // Path 속성을 새 이름으로 업데이트
                            }
                            else
                            {
                                System.Windows.MessageBox.Show("A file with that name already exists.", "Error");
                            }
                        }

                        item.Name = Path.GetFileName(newPath); // Name 속성도 업데이트
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Failed to rename the item. Error: {ex.Message}", "Error");
                    }

                    // UI 업데이트를 위해 알림
                    item.RaisePropertyChanged(nameof(item.Path));
                    item.RaisePropertyChanged(nameof(item.Name));
                }
            }
        }


        private RelayCommand<object> _AddMessageCommand;
        public ICommand AddMessageCommand
        {
            get
            {
                if (null == _AddMessageCommand) _AddMessageCommand = new RelayCommand<object>(AddMessageCommandFunc);
                return _AddMessageCommand;
            }
        }
        private void AddMessageCommandFunc(object obj)
        {
            try
            {
                //// Commnad 선택
                //var commandSelectWindow = new CommandSelectWindow();
                //string commandName = string.Empty;

                //commandSelectWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;

                //var dialogResult = commandSelectWindow.ShowDialog();
                //if (dialogResult.HasValue && dialogResult.Value)
                //{
                //    commandName = commandSelectWindow.SelectedCommand;
                //}

                if (string.IsNullOrEmpty(SelectedFilePath) == false)
                {
                    if (GemActlistVisibility == true && SelectedGemAct != null && string.IsNullOrEmpty(SelectedGemAct.Name) == false)
                    {
                        string msg = string.Empty;

                        msg = RunGemRemoteMessage.GetDefaultMessage(SelectedGemAct.Name);

                        if (string.IsNullOrEmpty(msg) == false)
                        {
                            var secsmsg = SecsMessage.Parse(msg);

                            if (Messages != null && secsmsg != null)
                            {
                                Messages.Add(secsmsg);

                                SaveParametersToJsonCommandFunc(SelectedFilePath);

                                LoggerManager.Debug($"[{this.GetType().Name}] AddMessageCommandFunc() : Command name = {SelectedGemAct.Name}");

                                SelectedSecsMessage = secsmsg;
                            }
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"[{this.GetType().Name}] AddMessageCommandFunc() : enumRemoteCommand is null.");
                    }
                }
                else
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] AddMessageCommandFunc() : SelectedFilePath is empty.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _DeleteMessageCommand;
        public ICommand DeleteMessageCommand
        {
            get
            {
                if (null == _DeleteMessageCommand) _DeleteMessageCommand = new RelayCommand<object>(DeleteMessageCommandFunc);
                return _DeleteMessageCommand;
            }
        }
        private void DeleteMessageCommandFunc(object obj)
        {
            try
            {
                // Null 체크 추가
                if (SelectedSecsMessage == null || Messages == null)
                {
                    return; // Null이면 함수 종료
                }

                if (Messages.Contains(SelectedSecsMessage))
                {
                    // Confirm before deleting
                    MessageBoxResult confirmation = System.Windows.MessageBox.Show("Are you sure you want to delete the selected message?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (confirmation == MessageBoxResult.Yes)
                    {
                        int currentIndex = Messages.IndexOf(SelectedSecsMessage);
                        Messages.Remove(SelectedSecsMessage);

                        // SAVE
                        SaveParametersToJsonCommandFunc(SelectedFilePath);

                        // After removal, adjust the selection
                        if (Messages.Count > 0) // Check if there are still items in the collection
                        {
                            if (currentIndex >= Messages.Count) // 현재 인덱스가 컬렉션의 크기와 같거나 크면 마지막 요소를 선택
                            {
                                SelectedSecsMessage = Messages.LastOrDefault();
                            }
                            else if (currentIndex > 0) // If the removed item was not the first, select the previous item
                            {
                                SelectedSecsMessage = Messages[currentIndex - 1];
                            }
                            else // If the removed item was the first, select the new first item
                            {
                                SelectedSecsMessage = Messages.FirstOrDefault();
                            }
                        }
                        else // If no items are left, clear the selection
                        {
                            SelectedSecsMessage = null;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err); // 이곳에서도 null이 아닌지 확인해야 할 수 있습니다.
            }
        }

        private RelayCommand<object> _SaveMessageCommand;
        public ICommand SaveMessageCommand
        {
            get
            {
                if (null == _SaveMessageCommand) _SaveMessageCommand = new RelayCommand<object>(SaveMessageCommandFunc);
                return _SaveMessageCommand;
            }
        }
        private void SaveMessageCommandFunc(object obj)
        {
            try
            {
                if (string.IsNullOrEmpty(SelectedSecsMessageText) == false)
                {
                    var newMessage = SecsMessage.Parse(SelectedSecsMessageText);

                    if (newMessage != null)
                    {
                        SelectedSecsMessage.Copy(newMessage);

                        SaveParametersToJsonCommandFunc(SelectedFilePath);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _ExecuteMessageCommand;
        public ICommand ExecuteMessageCommand
        {
            get
            {
                if (null == _ExecuteMessageCommand) _ExecuteMessageCommand = new RelayCommand<object>(ExecuteMessageCommandFunc);
                return _ExecuteMessageCommand;
            }
        }

        private readonly object _lockObject = new object();
        private void ExecuteMessageCommandFunc(object obj)
        {
            lock (_lockObject)
            {
                try
                {
                    if (SelectedSecsMessage != null && string.IsNullOrEmpty(SelectedSecsMessageText) == false)
                    {
                        SecsMessage secsMessage = null;

                        if (UseEditedMessage)
                        {
                            var newMessage = SecsMessage.Parse(SelectedSecsMessageText);

                            if (newMessage != null)
                            {
                                secsMessage = new SecsMessage();
                                secsMessage.Copy(newMessage);

                                if (string.IsNullOrEmpty(secsMessage.CommandName) == true)
                                {
                                    secsMessage.CommandName = SelectedSecsMessage.CommandName;
                                }
                            }
                        }
                        else
                        {
                            secsMessage = SelectedSecsMessage;
                        }

                        long nStream = secsMessage.Stream;
                        long nFunction = secsMessage.Function;

                        if (nStream == 2 && nFunction == 15)
                        {
                            var equipReqData = SecsMessageToEquipmentReqData(secsMessage);
                        }
                        else if (nStream == 3 && nFunction == 17)
                        {
                            var actReqData = SecsMessageToCarrierActReqData(secsMessage);

                            if (actReqData != null)
                            {
                                this.GEMModule().GemCommManager.OnCarrierActMsgRecive(actReqData);
                            }
                        }
                        else if ((nStream == 2 && nFunction == 49) || (nStream == 2 && nFunction == 41))
                        {
                            var actReqData = SecsMessageToRemoteActReqData(secsMessage);

                            if (actReqData != null)
                            {
                                this.GEMModule().GemCommManager.OnRemoteCommandAction(actReqData);
                            }
                        }
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
        }

        private EquipmentReqData SecsMessageToEquipmentReqData(SecsMessage secsMessage)
        {
            EquipmentReqData equipmentReqData = null;

            try
            {
                var Gem = new SecsMessageHelper(secsMessage.Data);

                long nMsgId = 0;
                long nSysbyte = 0;
                long nStream = secsMessage.Stream;
                long nFunction = secsMessage.Function;

                long mainListCount = 0;
                long subListCount = 0;

                uint[] ecid = new uint[1];
                string val = null;

                Gem.GetList(nMsgId, ref mainListCount);

                uint[] pnEcids = new uint[mainListCount];
                string[] psVals = new string[mainListCount];

                for (int i = 0; i < mainListCount; i++)
                {
                    // TODO : 
                    Gem.GetList(nMsgId, ref subListCount);

                    if(subListCount == 2)
                    {
                        Gem.GetU2(nMsgId, ref ecid);
                        Gem.GetAscii(nMsgId, ref val);

                        pnEcids[i] = ecid[0];
                        psVals[i] = val;
                    }
                }

                //Gem.CloseObject(nMsgId);

                if (equipmentReqData != null)
                {
                    equipmentReqData.Stream = nStream;
                    equipmentReqData.Function = nFunction;
                    equipmentReqData.Sysbyte = nSysbyte;
                }

                long[] pnEcidsLong = new long[mainListCount];
                // uint 배열에서 long 배열로 값 복사
                for (int i = 0; i < mainListCount; i++)
                {
                    pnEcidsLong[i] = pnEcids[i];

                }
                this.GEMModule().GemCommManager.ECVChangeMsgReceive(nMsgId, mainListCount, pnEcidsLong, psVals);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return equipmentReqData;
        }
        private RemoteActReqData SecsMessageToRemoteActReqData(SecsMessage secsMessage)
        {
            RemoteActReqData remoteActReqData = null;

            try
            {
                var Gem = new SecsMessageHelper(secsMessage.Data);

                long nMsgId = 0;
                long nSysbyte = 0;
                long nStream = secsMessage.Stream;
                long nFunction = secsMessage.Function;

                long mainListCount = 0;
                long subListCount = 0;
                long subItemListCount = 0;
                List<long> RetValues = new List<long>();

                uint[] USER_UINT4 = new uint[1];
                string REMOTE_COMMAND_ACTION = null;
                string REMOTE_COMMAND_ID = null;

                if (nFunction == 41)
                {
                    Gem.GetList(nMsgId, ref mainListCount);
                    Gem.GetAscii(nMsgId, ref REMOTE_COMMAND_ACTION);
                }
                else if (nFunction == 49)
                {
                    Gem.GetList(nMsgId, ref mainListCount);
                    Gem.GetU4(nMsgId, ref USER_UINT4);
                    Gem.GetAscii(nMsgId, ref REMOTE_COMMAND_ID);
                    Gem.GetAscii(nMsgId, ref REMOTE_COMMAND_ACTION);
                }

                /* 
                // [NOT DEFINED]
                // SELECT_SLOTS
                // SELECT_SLOTS_STAGES
                // DOCK_FOUP
                // WAFERUNLOAD
                // END_TEST_LP
                */

                if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.PSTART.ToString() ||
                    REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.TC_START.ToString())
                {
                    var actreqdata = new StartLotActReqData();
                    actreqdata.ObjectID = nMsgId;
                    actreqdata.Stream = nStream;
                    actreqdata.Function = nFunction;
                    actreqdata.Sysbyte = nSysbyte;

                    string CPNAME = null;
                    string lotId = null;
                    uint[] foupNumber = new uint[1];
                    string carrierId = null;

                    Gem.GetList(nMsgId, ref subListCount); // L, 2

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] LOT_ID
                    Gem.GetAscii(nMsgId, ref lotId);// [A] LOT_ID
                    RetValues.Add(0);
                    actreqdata.LotID = lotId;

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] PORT_ID
                    Gem.GetU4(nMsgId, ref foupNumber);// [U4] PORT_ID
                    RetValues.Add(0);
                    actreqdata.FoupNumber = (int)foupNumber[0];

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] CARRIER_ID
                    Gem.GetAscii(nMsgId, ref carrierId);// [A] CARRIER_ID
                    RetValues.Add(0);
                    actreqdata.CarrierID = carrierId;

                    if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.PSTART.ToString())
                        actreqdata.ActionType = EnumRemoteCommand.PSTART;
                    else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.TC_START.ToString())
                        actreqdata.ActionType = EnumRemoteCommand.TC_START;

                    remoteActReqData = actreqdata;
                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.STAGE_SLOT.ToString())
                {
                    var actreqdata = new SelectStageSlotActReqData(); // ProceedWithCellSlotActReqData

                    string CPNAME = null;
                    uint[] foupNumber = new uint[1];
                    string carrierId = null;
                    string stageNumber = null;
                    string selectedSlot = null;

                    Gem.GetList(nMsgId, ref subListCount); // L, 4                                        

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2                    
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] PORT_ID                    
                    Gem.GetU4(nMsgId, ref foupNumber);// [A] PORT_ID
                    RetValues.Add(0);
                    actreqdata.PTN = (int)foupNumber[0];

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] CARRIER_ID
                    Gem.GetAscii(nMsgId, ref carrierId);// [A] CARRIER_ID
                    RetValues.Add(0);
                    actreqdata.CarrierId = carrierId;

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] STAGE
                    Gem.GetAscii(nMsgId, ref stageNumber);// [A] STAGE
                    RetValues.Add(0);
                    actreqdata.CellMap = stageNumber;

                    Gem.GetList(nMsgId, ref subItemListCount); // [L] ListOfSlotToUse List
                    Gem.GetAscii(nMsgId, ref CPNAME); //[A] Title ListOfSlotToUse
                    Gem.GetAscii(nMsgId, ref selectedSlot); //[A]  ListOfSlotToUse
                    RetValues.Add(0);
                    actreqdata.SlotMap = selectedSlot;
                    remoteActReqData = actreqdata;

                    remoteActReqData.ActionType = EnumRemoteCommand.STAGE_SLOT;
                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.SELECT_SLOTS.ToString())
                {
                    var actreqdata = new SelectSlotsActReqData(); // ProceedWithCellSlotActReqData

                    string CPNAME = null;
                    uint[] foupNumber = new uint[1];
                    string lotid = null;
                    string selectedSlot = null;

                    Gem.GetList(nMsgId, ref subListCount); // L, 4                                        

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2                    
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] PORT_ID                    
                    Gem.GetU4(nMsgId, ref foupNumber);// [A] PORT_ID
                    RetValues.Add(0);
                    actreqdata.FoupNumber = (int)foupNumber[0];

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] lotid
                    Gem.GetAscii(nMsgId, ref lotid);// [A] lotid
                    RetValues.Add(0);
                    actreqdata.LotID = lotid;

                    Gem.GetList(nMsgId, ref subItemListCount); // [L] ListOfSlotToUse List
                    Gem.GetAscii(nMsgId, ref CPNAME); //[A] Title ListOfSlotToUse
                    Gem.GetAscii(nMsgId, ref selectedSlot); //[A]  ListOfSlotToUse
                    RetValues.Add(0);
                    actreqdata.UseSlotNumbers_str = selectedSlot;
                    remoteActReqData = actreqdata;

                    remoteActReqData.ActionType = EnumRemoteCommand.SELECT_SLOTS;
                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.SELECT_SLOTS_STAGES.ToString())
                {
                    var actreqdata = new SelectStageSlotsActReqData();

                    string CPNAME = null;
                    uint[] foupNumber = new uint[1];
                    string lotid = null;
                    string stagesToUse = null;
                    string selectedstageSlot = null;

                    Gem.GetList(nMsgId, ref subListCount); // L, 4                                        

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2                    
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] PORT_ID                    
                    Gem.GetU4(nMsgId, ref foupNumber);// [A] PORT_ID
                    RetValues.Add(0);
                    actreqdata.PTN = (int)foupNumber[0];

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] lotid
                    Gem.GetAscii(nMsgId, ref lotid);// [A] lotid
                    RetValues.Add(0);
                    actreqdata.LotID = lotid;

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] stagesToUse
                    Gem.GetAscii(nMsgId, ref stagesToUse);// [A] stagesToUse
                    RetValues.Add(0);
                    actreqdata.CellMap = stagesToUse;// like "101000000000"

                    Gem.GetList(nMsgId, ref subItemListCount); // [L] ListOfSlotToUse List
                    Gem.GetAscii(nMsgId, ref CPNAME); //[A] Title UseSlotStageNumbers_str
                    Gem.GetAscii(nMsgId, ref selectedstageSlot); //[A]  UseSlotStageNumbers_str
                    RetValues.Add(0);
                    actreqdata.UseSlotStageNumbers_str = selectedstageSlot;// like "113300000000"
                    remoteActReqData = actreqdata;

                    remoteActReqData.ActionType = EnumRemoteCommand.SELECT_SLOTS_STAGES;
                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.WFIDCONFPROC.ToString())
                {
                    var actreqdata = new WaferConfirmActReqData();

                    string CPNAME = null;
                    string LOTID = null;
                    uint[] foupNumber = new uint[1];
                    uint[] slotNumber = new uint[1];
                    string waferId = null;
                    string ocrRead = null;

                    Gem.GetList(nMsgId, ref subListCount); // L, 4                    

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] LOTID
                    Gem.GetAscii(nMsgId, ref LOTID);// [A] LOTID
                    RetValues.Add(0);
                    actreqdata.LotID = LOTID;

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [U4] PORT_ID
                    Gem.GetU4(nMsgId, ref foupNumber);// [U4] PORT_ID
                    RetValues.Add(0);
                    actreqdata.PTN = (int)foupNumber[0];

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [U4] SLOT_NUM
                    Gem.GetU4(nMsgId, ref slotNumber);// [U4] SLOT_NUM
                    RetValues.Add(0);
                    actreqdata.SlotNum = (int)slotNumber[0];

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] WAFER_ID                    
                    Gem.GetAscii(nMsgId, ref waferId);// [A] WAFER_ID
                    RetValues.Add(0);
                    actreqdata.WaferId = waferId;

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] OCRREAD
                    Gem.GetAscii(nMsgId, ref ocrRead);// [A] OCRREAD
                    RetValues.Add(0);
                    actreqdata.OCRReadFalg = int.Parse(ocrRead);

                    remoteActReqData = actreqdata;
                    remoteActReqData.ActionType = EnumRemoteCommand.WFIDCONFPROC;
                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.DLRECIPE.ToString())
                {
                    var actreqdata = new DownloadStageRecipeActReqData();

                    string CPNAME = null;
                    string recipe = null;
                    string StageNum = null;
                    int stgnum = 0;

                    Gem.GetList(nMsgId, ref subListCount); // L, 2

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] RECIPE
                    Gem.GetAscii(nMsgId, ref recipe);// [A] RECIPE
                    RetValues.Add(0);


                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [U1] StageNumber
                    Gem.GetAscii(nMsgId, ref StageNum);// [U4] StageNumber
                    int.TryParse(StageNum, out stgnum);
                    RetValues.Add(0);

                    actreqdata.RecipeDic.Add(stgnum, recipe); //Data : [U] StageNumber , [A] Recipe Id

                    remoteActReqData = actreqdata;
                    remoteActReqData.ActionType = EnumRemoteCommand.DLRECIPE;
                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.RESUME.ToString())
                {
                    var actreqdata = new StartStage();

                    string CPNAME = null;
                    string StageId = null;

                    Gem.GetList(nMsgId, ref subListCount); // L, 1

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] STAGE_ID
                    Gem.GetAscii(nMsgId, ref StageId);// [A] STAGE_ID
                    RetValues.Add(0);

                    if (StageId != null)
                    {
                        actreqdata.StageNumber = int.Parse(StageId);
                    }

                    remoteActReqData = actreqdata;
                    remoteActReqData.ActionType = EnumRemoteCommand.RESUME;
                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.UNDOCK.ToString() ||
                        REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.DOCK_FOUP.ToString())
                {
                    var actreqdata = new CarrierIdentityData();
                    actreqdata.ObjectID = nMsgId;
                    actreqdata.Stream = nStream;
                    actreqdata.Function = nFunction;
                    actreqdata.Sysbyte = nSysbyte;

                    string CPNAME = null;
                    uint[] foupNumber = new uint[1];
                    string carrierId = null;

                    Gem.GetList(nMsgId, ref subListCount); // L, 2

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] PORT_ID
                    Gem.GetU4(nMsgId, ref foupNumber);// [A] PORT_ID
                    RetValues.Add(0);
                    actreqdata.PTN = (int)foupNumber[0];

                    Gem.GetList(nMsgId, ref subItemListCount); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] CARRIER_ID
                    Gem.GetAscii(nMsgId, ref carrierId);// [A] CARRIER_ID
                    RetValues.Add(0);
                    actreqdata.CarrierId = carrierId;

                    remoteActReqData = actreqdata;
                    if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.DOCK_FOUP.ToString())
                    {
                        remoteActReqData.ActionType = EnumRemoteCommand.DOCK_FOUP;
                    }
                    else
                    {
                        remoteActReqData.ActionType = EnumRemoteCommand.UNDOCK;
                    }

                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.ZUP.ToString()
                    || REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.TESTEND.ToString()
                    || REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.WAFERUNLOAD.ToString()
                    || REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.PABORT.ToString()
                    || REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.START_CARD_CHANGE.ToString()
                    || REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.MOVEIN_CARD_CLOSE_COVER.ToString()
                    || REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.TC_END.ToString()
                    )
                {
                    byte HAACK = 0x04;
                    long objid = 0;

                    // TODO : 
                    Gem.MakeObject(ref objid);
                    Gem.SetListItem(objid, 2);
                    Gem.SetBinaryItem(objid, HAACK);
                    Gem.SetListItem(objid, 0);

                    var actreqdata = new StageActReqData();
                    actreqdata.ObjectID = objid;
                    actreqdata.Stream = nStream;
                    actreqdata.Function = nFunction;
                    actreqdata.Sysbyte = nSysbyte;

                    string CPNAME = null;
                    string stagenumber = null;
                    Gem.GetList(nMsgId, ref subListCount);

                    Gem.GetList(nMsgId, ref subItemListCount);// [L] StageNumber
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] StageNumber
                    Gem.GetAscii(nMsgId, ref stagenumber);// [A] StageNumber

                    actreqdata.StageNumber = Convert.ToInt32(stagenumber); //Data [U] StageNumber

                    remoteActReqData = actreqdata;

                    if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.ZUP.ToString())
                        remoteActReqData.ActionType = EnumRemoteCommand.ZUP;
                    else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.TESTEND.ToString())
                        remoteActReqData.ActionType = EnumRemoteCommand.TESTEND;
                    else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.WAFERUNLOAD.ToString())
                        remoteActReqData.ActionType = EnumRemoteCommand.WAFERUNLOAD;
                    else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.PABORT.ToString())
                        remoteActReqData.ActionType = EnumRemoteCommand.PABORT;
                    else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.START_CARD_CHANGE.ToString())
                        remoteActReqData.ActionType = EnumRemoteCommand.START_CARD_CHANGE;
                    else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.MOVEIN_CARD_CLOSE_COVER.ToString())
                        remoteActReqData.ActionType = EnumRemoteCommand.MOVEIN_CARD_CLOSE_COVER;
                    else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.TC_END.ToString())
                        remoteActReqData.ActionType = EnumRemoteCommand.TC_END;

                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.PROCEED_CARD_CHANGE.ToString() ||
                        REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.SKIP_CARD_ATTACH.ToString()
                        )
                {
                    byte HAACK = 0x04;
                    long objid = 0;

                    // TODO : 
                    Gem.MakeObject(ref objid);
                    Gem.SetListItem(objid, 2);
                    Gem.SetBinaryItem(objid, HAACK);
                    Gem.SetListItem(objid, 0);

                    var actreqdata = new StageSeqActReqData();
                    actreqdata.ObjectID = objid;
                    actreqdata.Stream = nStream;
                    actreqdata.Function = nFunction;
                    actreqdata.Sysbyte = nSysbyte;

                    string CPNAME = null;
                    string stagenumber = null;
                    byte[] endseq = new byte[1];
                    Gem.GetList(nMsgId, ref subListCount);

                    Gem.GetList(nMsgId, ref subItemListCount);// [L] StageNumber
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] StageNumber
                    Gem.GetAscii(nMsgId, ref stagenumber);// [A] StageNumber

                    Gem.GetList(nMsgId, ref subItemListCount);// [L] endseq
                    Gem.GetAscii(nMsgId, ref CPNAME);// [U1] endseq
                    Gem.GetU1(nMsgId, ref endseq);// [U1] endseq


                    actreqdata.StageNumber = Convert.ToInt32(stagenumber); //Data [U] StageNumber
                    actreqdata.EndSeq = Convert.ToBoolean(endseq[0]);

                    remoteActReqData = actreqdata;

                    if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.PROCEED_CARD_CHANGE.ToString())
                        remoteActReqData.ActionType = EnumRemoteCommand.PROCEED_CARD_CHANGE;
                    if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.SKIP_CARD_ATTACH.ToString())
                        remoteActReqData.ActionType = EnumRemoteCommand.SKIP_CARD_ATTACH;
                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.END_TEST_LP.ToString())
                {
                    byte HAACK = 0x04;
                    long objid = 0;

                    // TODO : 
                    Gem.MakeObject(ref objid);
                    Gem.SetListItem(objid, 2);
                    Gem.SetBinaryItem(objid, HAACK);
                    Gem.SetListItem(objid, 0);

                    var actreqdata = new EndTestReqLPDate();
                    actreqdata.ObjectID = objid;
                    actreqdata.Stream = nStream;
                    actreqdata.Function = nFunction;
                    actreqdata.Sysbyte = nSysbyte;

                    //long dataListCount = 0;
                    string CPNAME = null;
                    string stagenumber = null;
                    string UnloadFoupNumber = null;
                    Gem.GetList(nMsgId, ref subListCount);

                    Gem.GetList(nMsgId, ref subItemListCount);// [L] StageNumber
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] StageNumber
                    Gem.GetAscii(nMsgId, ref stagenumber);// [A] StageNumber
                    actreqdata.StageNumber = Convert.ToInt32(stagenumber); //Data [U] StageNumber

                    Gem.GetList(nMsgId, ref subItemListCount);// [L] UnloadFoupNumber
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] UnloadFoupNumber
                    Gem.GetAscii(nMsgId, ref UnloadFoupNumber);// [A] UnloadFoupNumber
                    actreqdata.FoupNumber = Convert.ToInt32(UnloadFoupNumber); //Data [U] StageNumber

                    remoteActReqData = actreqdata;
                    remoteActReqData.ActionType = EnumRemoteCommand.END_TEST_LP;

                }
                else if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.WAFER_CHANGE.ToString())
                {
                    var actreqdata = new WaferChangeData();
                    actreqdata.ObjectID = nMsgId;
                    actreqdata.Stream = nStream;
                    actreqdata.Function = nFunction;
                    actreqdata.Sysbyte = nSysbyte;

                    long dflist1 = 0;
                    long dflist2 = 0;
                    long dflist3 = 0;
                    long dflist4 = 0;

                    string CPNAME = null;
                    uint[] ocrRead = new uint[1];
                    string waferID = null;
                    string location1_LP = null;
                    string location1_Atom_Idx = null;
                    string location2_LP = null;
                    string location2_Atom_Idx = null;

                    Gem.GetList(nMsgId, ref dflist1); // L, 2
                    Gem.GetList(nMsgId, ref dflist1); // L, 2
                    Gem.GetAscii(nMsgId, ref CPNAME);// [A] OCR Read
                    Gem.GetU4(nMsgId, ref ocrRead);// [U4] Result 0 = Pass, 1 = Fail, 2 = Auto
                    actreqdata.OCRRead = (int)ocrRead[0];
                    Gem.GetList(nMsgId, ref dflist2); // L, n

                    long listcount = dflist2;
                    actreqdata.WaferID = new string[listcount];
                    actreqdata.LOC1_LP = new string[listcount];
                    actreqdata.LOC1_Atom_Idx = new string[listcount];
                    actreqdata.LOC2_LP = new string[listcount];
                    actreqdata.LOC2_Atom_Idx = new string[listcount];

                    for (int index = 0; index < listcount; index++)
                    {
                        Gem.GetList(nMsgId, ref dflist3); // L, 5

                        Gem.GetList(nMsgId, ref dflist4); // L, 2
                        Gem.GetAscii(nMsgId, ref CPNAME);// [A] "WAFERID"  
                        Gem.GetAscii(nMsgId, ref waferID);// [A] 'WaferID"
                        actreqdata.WaferID[index] = waferID;

                        Gem.GetList(nMsgId, ref dflist4); // L, 2
                        Gem.GetAscii(nMsgId, ref CPNAME);// [A] "LOC1_LP"  
                        Gem.GetAscii(nMsgId, ref location1_LP);// [A] 'LP#No.'
                        actreqdata.LOC1_LP[index] = location1_LP;

                        Gem.GetList(nMsgId, ref dflist4); // L, 2
                        Gem.GetAscii(nMsgId, ref CPNAME);// [A] "LOC1_ATOM_IDX"  
                        Gem.GetAscii(nMsgId, ref location1_Atom_Idx);// [A] 'S#No.' or 'F#No.'..
                        actreqdata.LOC1_Atom_Idx[index] = location1_Atom_Idx;

                        Gem.GetList(nMsgId, ref dflist4); // L, 2
                        Gem.GetAscii(nMsgId, ref CPNAME);// [A] "LOC2_LP"  
                        Gem.GetAscii(nMsgId, ref location2_LP);// [A] 'LP#No.'
                        actreqdata.LOC2_LP[index] = location2_LP;

                        Gem.GetList(nMsgId, ref dflist4); // L, 2
                        Gem.GetAscii(nMsgId, ref CPNAME);// [A] "LOC2_ATOM_IDX"  
                        Gem.GetAscii(nMsgId, ref location2_Atom_Idx);// [A] 'S#No.' or 'F#No.'..
                        actreqdata.LOC2_Atom_Idx[index] = location2_Atom_Idx;
                    }
                    remoteActReqData = actreqdata;
                    remoteActReqData.ActionType = EnumRemoteCommand.WAFER_CHANGE;
                }
                else
                {
                    //long objid = 0;
                    //long pnMsgId = 0;
                    //byte HAACK = 0x01;
                    //MakeObject(ref pnMsgId);
                    //SetListItem(pnMsgId, 2);
                    //SetBinaryItem(pnMsgId, HAACK);
                    //SetListItem(pnMsgId, 0);

                    //OnlyReqData actreqdata = new OnlyReqData();
                    //actreqdata.ObjectID = objid;
                    //actreqdata.Stream = nStream;
                    //actreqdata.Function = nFunction;
                    //actreqdata.Sysbyte = nSysbyte;


                    //remoteActReqData = actreqdata;
                    //if (REMOTE_COMMAND_ACTION.ToUpper() == EnumRemoteCommand.CARD_SEQ_ABORT.ToString())
                    //    remoteActReqData.ActionType = EnumRemoteCommand.CARD_SEQ_ABORT;
                }

                remoteActReqData.Stream = nStream;
                remoteActReqData.ObjectID = 0;
                remoteActReqData.Function = nFunction;
                remoteActReqData.Sysbyte = nSysbyte;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return remoteActReqData;
        }
        private CarrierActReqData SecsMessageToCarrierActReqData(SecsMessage secsMessage)
        {
            CarrierActReqData carrierActReqData = null;

            try
            {
                var Gem = new SecsMessageHelper(secsMessage.Data);

                long nMsgId = 0;
                long nSysbyte = 0;
                long nStream = secsMessage.Stream;
                long nFunction = secsMessage.Function;

                long mainListCount = 0;
                long subListCount = 0;
                long dataListCount = 0;

                uint[] USER_UINT4 = new uint[1];
                string CARRIERACTION = null;
                string CARRIERID = null;
                byte[] PTN = new byte[1];

                string CATTRID = null;
                string CATTRDATA = null;

                int total_cnt = 0;

                Gem.GetList(nMsgId, ref mainListCount);
                Gem.GetU4(nMsgId, ref USER_UINT4);
                Gem.GetAscii(nMsgId, ref CARRIERACTION);
                Gem.GetAscii(nMsgId, ref CARRIERID);
                Gem.GetU1(nMsgId, ref PTN);
                Gem.GetList(nMsgId, ref subListCount);

                if (secsMessage.CommandName == EnumCarrierAction.PROCEEDWITHCARRIER.ToString())
                {
                    var actCarrierData = new ProceedWithCarrierReqData();

                    actCarrierData.ActionType = EnumCarrierAction.PROCEEDWITHCARRIER;
                    actCarrierData.Sysbyte = nSysbyte;
                    actCarrierData.DataID = USER_UINT4[0];
                    actCarrierData.CarrierAction = CARRIERACTION;
                    actCarrierData.CarrierID = CARRIERID;
                    actCarrierData.PTN = PTN[0];
                    actCarrierData.Count = subListCount;
                    actCarrierData.CattrID = new string[subListCount];
                    actCarrierData.SlotMap = new string[subListCount * 25];

                    for (int i = 0; i < subListCount; i++)
                    {
                        Gem.GetList(nMsgId, ref dataListCount);

                        if (actCarrierData.DataID == 0)
                        {
                            //LOT ID
                            Gem.GetAscii(nMsgId, ref CATTRID);
                            actCarrierData.CattrID[i] = CATTRID;
                            Gem.GetAscii(nMsgId, ref CATTRDATA);
                            actCarrierData.LotID = CATTRDATA;
                        }
                        else if (actCarrierData.DataID == 1)
                        {
                            long waferIdListCount = 0;
                            //WAFER ID
                            Gem.GetAscii(nMsgId, ref CATTRID);
                            actCarrierData.CattrID[i] = CATTRID;
                            Gem.GetList(nMsgId, ref waferIdListCount);

                            for (int s = 0; s < 25; s++)
                            {
                                string slotmap = null;
                                Gem.GetAscii(nMsgId, ref slotmap);
                                actCarrierData.SlotMap[total_cnt++] = slotmap;
                            }
                        }
                    }

                    carrierActReqData = actCarrierData;
                }
                else if (secsMessage.CommandName == EnumCarrierAction.PROCEEDWITHSLOT.ToString())
                {
                    var actCarrierData = new ProceedWithSlotReqData();
                    actCarrierData.ActionType = EnumCarrierAction.PROCEEDWITHSLOT;

                    actCarrierData.CarrierID = CARRIERID;
                    actCarrierData.PTN = PTN[0];
                    actCarrierData.Count = subListCount;
                    actCarrierData.SlotMap = new string[25];
                    actCarrierData.OcrMap = new string[25];
                    long slotListCount = 0;

                    Gem.GetList(nMsgId, ref dataListCount);
                    Gem.GetAscii(nMsgId, ref CATTRID);
                    Gem.GetList(nMsgId, ref slotListCount);

                    for (int s = 0; s < 25; s++)
                    {
                        string slotmap = null;
                        Gem.GetAscii(nMsgId, ref slotmap);
                        actCarrierData.SlotMap[s] = slotmap;
                    }

                    string temp = null;
                    string cattrdata = null;
                    Gem.GetList(nMsgId, ref dataListCount);
                    Gem.GetAscii(nMsgId, ref temp);
                    Gem.GetAscii(nMsgId, ref cattrdata);
                    actCarrierData.Usage = cattrdata;

                    long item_cnt = 0;
                    string cattrid = null;
                    long slotmap_cnt = 0;
                    Gem.GetList(nMsgId, ref item_cnt);
                    Gem.GetAscii(nMsgId, ref cattrid);
                    Gem.GetList(nMsgId, ref slotmap_cnt);

                    for (int s = 0; s < 25; s++)
                    {
                        string ocrmap = null;
                        Gem.GetAscii(nMsgId, ref ocrmap);
                        actCarrierData.OcrMap[s] = ocrmap;
                    }

                    carrierActReqData = actCarrierData;
                }
                else if (secsMessage.CommandName == EnumCarrierAction.CANCELCARRIER.ToString())
                {
                    var actCarrierData = new CarrieActReqData();
                    actCarrierData.ActionType = EnumCarrierAction.CANCELCARRIER;
                    actCarrierData.PTN = PTN[0];

                    for (int i = 0; i < subListCount; i++)
                    {
                        Gem.GetList(nMsgId, ref dataListCount);

                        //CARRIER DATA
                        Gem.GetAscii(nMsgId, ref CATTRID);
                        Gem.GetAscii(nMsgId, ref CATTRDATA);
                        actCarrierData.CarrierData = CATTRDATA;
                    }

                    carrierActReqData = actCarrierData;
                }
                else
                {
                    //long pnMsgId = 0;
                    //byte CAACK = 0x01;

                    //Gem.MakeObject(ref pnMsgId);

                    //Gem.SetList(pnMsgId, 2);
                    //Gem.SetU1(pnMsgId, CAACK);
                    //Gem.SetList(pnMsgId, subListCount);

                    //for (int i = 0; i < subListCount; i++)
                    //{
                    //    Gem.SetList(pnMsgId, 2);
                    //    Gem.SetU2(pnMsgId, 0);
                    //    Gem.SetAscii(pnMsgId, "");
                    //}

                    //carrierActReqData = null;
                }

                carrierActReqData.Stream = nStream;
                carrierActReqData.Function = nFunction;
                carrierActReqData.Sysbyte = nSysbyte;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return carrierActReqData;
        }

        private void LoadParametersFromJsonCommandFunc(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return;

            try
            {
                string jsonString = File.ReadAllText(filename);
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new SecsItemConverter()); // Ensure this is your custom converter for SecsItem
                var commandsFromJson = JsonConvert.DeserializeObject<ObservableCollection<SecsMessage>>(jsonString, settings);
                Messages = commandsFromJson ?? new ObservableCollection<SecsMessage>();
                RaisePropertyChanged(nameof(Messages)); // Notify collection change

                if (Messages.Count > 0)
                {
                    SelectedSecsMessage = Messages.FirstOrDefault();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void SaveParametersToJsonCommandFunc(string filename)
        {
            string jsonFilePath = Path.Combine(rootFolderPath, filename);

            try
            {
                var options = new JsonSerializerSettings { Formatting = Formatting.Indented };
                string jsonString = JsonConvert.SerializeObject(Messages, options);
                File.WriteAllText(jsonFilePath, jsonString);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
