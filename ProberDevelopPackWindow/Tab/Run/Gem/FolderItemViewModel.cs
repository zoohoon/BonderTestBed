using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ProberDevelopPackWindow.Tab.Run.Gem
{
    public class FolderItemViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _path;
        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                RaisePropertyChanged(); // 속성 변경 알림
            }
        }

        private string _Name;
        public string Name
        {
            get => _Name;
            set
            {
                _Name = value;
                RaisePropertyChanged(); // 속성 변경 알림
            }
        }

        public bool IsFolder { get; private set; } // 폴더인지 파일인지 구분

        private ObservableCollection<FolderItemViewModel> _children;
        public ObservableCollection<FolderItemViewModel> Children
        {
            get { return _children; }
            set
            {
                _children = value;
                RaisePropertyChanged();
            }
        }

        public FolderItemViewModel(string path, bool isFolder)
        {
            Path = path;
            IsFolder = isFolder;
            _Name = System.IO.Path.GetFileName(path) ?? path; // 초기 이름 설정
            Children = new ObservableCollection<FolderItemViewModel>();

            if (IsFolder)
            {
                LoadChildren();
            }
        }

        private void LoadChildren()
        {
            // 서브폴더 로드
            foreach (var directoryPath in Directory.GetDirectories(Path))
            {
                Children.Add(new FolderItemViewModel(directoryPath, true)); // 폴더
            }

            // 현재 폴더의 .json 파일 로드
            foreach (var filePath in Directory.GetFiles(Path).Where(f => f.EndsWith(".json")))
            {
                Children.Add(new FolderItemViewModel(filePath, false)); // 파일
            }
        }

        public void AddChild(string path, bool isFolder)
        {
            var newItem = new FolderItemViewModel(path, isFolder);
            Children.Add(newItem);
            RaisePropertyChanged(nameof(Children));
        }

        // JSON 파일이 있는지 확인
        public bool HasJsonFiles()
        {
            return Children.Any(child => !child.IsFolder && child.Path.EndsWith(".json"));
        }
    }
}
