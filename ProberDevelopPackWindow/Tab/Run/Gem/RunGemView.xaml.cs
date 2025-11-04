namespace ProberDevelopPackWindow.Tab
{
    using ICSharpCode.AvalonEdit;
    using ProberDevelopPackWindow.Tab.Run.Gem;
    using System.Windows;
    using System;
    using System.Windows.Controls;
    using System.Windows.Data;
    using ICSharpCode.AvalonEdit.Document;
    using System.Globalization;
    using System.ComponentModel;
    using MaterialDesignThemes.Wpf;
    using System.Windows.Input;
    using System.IO;
    using LogModule;
    using System.Windows.Media;

    /// <summary>
    /// RunGemView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RunGemView : UserControl
    {
        RunGemViewModel _ViewModel { get; set; }
        public RunGemView()
        {
            InitializeComponent();

            _ViewModel = new RunGemViewModel();

            this.DataContext = _ViewModel;

            // ViewModel의 PropertyChanged 이벤트에 대한 구독 설정
            if (DataContext is INotifyPropertyChanged vm)
            {
                vm.PropertyChanged += ViewModel_PropertyChanged;
            }

            if (multiLineTextBox.Document == null)
            {
                multiLineTextBox.Document = new TextDocument();
            }

            // Document의 변경을 감지하는 이벤트 핸들러 추가
            multiLineTextBox.Document.TextChanged += Document_TextChanged;

            _ViewModel.InitViewModel();

            // 키 이벤트를 처리하기 위해 포커스를 받을 수 있도록 설정
            this.Focusable = true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RunGemViewModel.SelectedSecsMessageText))
            {
                // 변경 이벤트 구독 해제
                multiLineTextBox.Document.TextChanged -= Document_TextChanged;

                // ViewModel의 SelectedSecsMessageText가 변경되면 TextEditor의 Document.Text를 업데이트
                if (DataContext is RunGemViewModel viewModel && multiLineTextBox.Document.Text != viewModel.SelectedSecsMessageText)
                {
                    multiLineTextBox.Document.Text = viewModel.SelectedSecsMessageText;
                }

                // 변경 이벤트 다시 구독
                multiLineTextBox.Document.TextChanged += Document_TextChanged;
            }
        }

        private void Document_TextChanged(object sender, EventArgs e)
        {
            if (DataContext is RunGemViewModel viewModel)
            {
                // 현재 Document의 텍스트와 ViewModel의 SelectedSecsMessageText가 다를 때만 업데이트
                var currentText = multiLineTextBox.Document.Text;
                if (viewModel.SelectedSecsMessageText != currentText)
                {
                    viewModel.UpdateSelectedSecsMessageText(currentText);
                }
            }
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                if (e.NewValue is FolderItemViewModel selectedItem)
                {
                    var viewModel = DataContext as RunGemViewModel;
                    if (selectedItem.IsFolder)
                    {
                        // 폴더 선택시 처리 (선택된 파일 경로를 설정하지 않음)
                    }
                    else
                    {
                        // 파일 선택시 처리
                        viewModel.SelectedFilePath = selectedItem.Path;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void TreeViewItem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (Keyboard.Modifiers != ModifierKeys.Control) // Ctrl 키가 눌려있지 않으면 아무 동작도 하지 않음
                    return;

                if (e.LeftButton == MouseButtonState.Pressed && sender is TreeViewItem)
                {
                    var draggedItem = (TreeViewItem)sender;
                    if (draggedItem.DataContext is FolderItemViewModel item)
                    {
                        if (!item.IsFolder)
                        {
                            DragDrop.DoDragDrop(draggedItem, item, DragDropEffects.Copy);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void TreeViewItem_DragEnter(object sender, DragEventArgs e)
        {
            var targetItem = sender as TreeViewItem;
            if (targetItem == null || !e.Data.GetDataPresent(typeof(FolderItemViewModel)))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            var targetFolder = targetItem.DataContext as FolderItemViewModel;
            // 폴더 대상으로 드래그 이벤트가 진입했는지 확인
            if (targetFolder != null && targetFolder.IsFolder)
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        private void TreeViewItem_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(typeof(FolderItemViewModel)) && sender is TreeViewItem targetItem)
                {
                    var targetFolder = (FolderItemViewModel)targetItem.DataContext;
                    var draggedItem = (FolderItemViewModel)e.Data.GetData(typeof(FolderItemViewModel));

                    if (!draggedItem.IsFolder && targetFolder.IsFolder)
                    {
                        string originalPath = draggedItem.Path;
                        string fileName = Path.GetFileName(originalPath);
                        string targetPath = Path.Combine(targetFolder.Path, fileName);

                        // 파일 이름 충돌을 피하기 위한 로직
                        targetPath = GenerateUniqueFilePath(targetFolder.Path, fileName);

                        File.Copy(originalPath, targetPath, false);

                        // UI 업데이트
                        targetFolder.AddChild(targetPath, false);

                        // 이벤트 처리를 완료했음을 나타내고 이벤트 버블링을 방지
                        e.Handled = true;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        // 파일 이름이 고유하도록 조정하는 메서드
        private string GenerateUniqueFilePath(string targetFolderPath, string fileName)
        {
            string path = string.Empty;

            try
            {
                path = Path.Combine(targetFolderPath, fileName);

                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                string extension = Path.GetExtension(fileName);
                int index = 1;

                while (File.Exists(path))
                {
                    string tempFileName = $"{fileNameWithoutExtension} - 복사본 ({index++}){extension}";
                    path = Path.Combine(targetFolderPath, tempFileName);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


            return path;
        }

        private void TreeView_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                var treeView = sender as TreeView;
                var selecteditem = treeView?.SelectedItem as FolderItemViewModel;

                if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    CopyToClipboard(selecteditem);
                }
                else if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    PasteFromClipboard(selecteditem);
                }
                else if(e.Key == Key.Delete)
                {
                    if (selecteditem != null)
                    {
                        if(selecteditem.IsFolder)
                        {
                            _ViewModel.DeleteFolderCommand.Execute(selecteditem);
                        }
                        else
                        {
                            _ViewModel.DeleteFileCommand.Execute(selecteditem);
                        }
                    }
                }
                else if(e.Key == Key.F2)
                {
                    if(selecteditem != null)
                    {
                        _ViewModel.RenameCommand.Execute(selecteditem);
                    }
                }
                else if(e.Key == Key.F5)
                {
                    // Refresh
                    _ViewModel.LoadRootFolder();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        // 선택된 항목의 Path를 클립보드에 저장합니다.
        private void CopyToClipboard(FolderItemViewModel item)
        {
            if (item != null)
            {
                Clipboard.SetText(item.Path);
            }
        }

        // 클립보드에서 Path를 가져와 해당 위치로 파일을 복사합니다.
        private void PasteFromClipboard(FolderItemViewModel targetFolder)
        {
            string sourcePath = Clipboard.GetText();

            if (!string.IsNullOrEmpty(sourcePath) && targetFolder != null && targetFolder.IsFolder)
            {
                string fileName = Path.GetFileName(sourcePath);
                string destPath = Path.Combine(targetFolder.Path, fileName);
                try
                {
                    File.Copy(sourcePath, destPath);
                    // 붙여넣은 파일을 UI에 반영하기 위해 targetFolder의 Children에 추가합니다.
                    targetFolder.AddChild(destPath, false);
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Error copying file: {ex.Message}");
                }
            }
        }

        private void TreeViewItem_DragLeave(object sender, DragEventArgs e)
        {
            var treeViewItem = sender as TreeViewItem;
            if (treeViewItem != null)
            {
                // 원래 스타일로 복구. 사용한 스타일에 따라 적절히 조정이 필요할 수 있습니다.
                treeViewItem.Background = Brushes.Transparent;
            }
        }

        private void TreeViewItem_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is TreeViewItem item)
            {
                item.Background = Brushes.LightBlue;
                // 이벤트 버블링을 방지
                e.Handled = true;
            }
        }

        private void TreeViewItem_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is TreeViewItem item)
            {
                item.Background = Brushes.Transparent;
                // 이벤트 버블링을 방지
                e.Handled = true;
            }
        }
    }

    public class StringToTextDocumentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                return new TextDocument(text);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TextDocument document)
            {
                return document.Text;
            }
            return value;
        }
    }
    public class FolderFileIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isFolder = (bool)value;
            return isFolder ? PackIconKind.Folder : PackIconKind.FileDocument;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class FolderFileIconColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFolder)
            {
                return isFolder ? "#FFE78F" : "#75D032";
            }

            return Binding.DoNothing; // 변환할 수 없는 경우 바인딩에 영향을 주지 않음
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); // 역변환은 구현하지 않음
        }
    }
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Brushes.Green : Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToForegrounColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Brushes.White : Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && (bool)value)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed; // 또는 Visibility.Hidden, 요구 사항에 따라 선택
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility && (Visibility)value == Visibility.Visible)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
