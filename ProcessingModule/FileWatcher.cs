using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using LogModule;

namespace ProcessingModule
{
    /// <summary>
    /// 폴더 변경 감시하여 캐싱. 
    /// 폴더내 변경이 감지되면 캐시를 삭제하고 요청이 오면 그때 다시 캐시를 함.  
    /// > 사용되지 않는 파일의 불필요한 사전 캐시를 막기 위해 , 파일의 요청이 오면 캐시함.
    /// </summary>
    public class FileWatcher : IDisposable
    {
        private string _watchPath;
        /// <summary>
        /// 감시 대상 지정. Directory 또는 파일명으로 설정 
        /// 변경시 폴더 캐시 내용 전부 삭제. 
        /// 캐시 삭제 이후 읽혀지는 파일은 다시 캐시함. 
        /// </summary>
        public string WatchPath
        {
            get => _watchPath;
            set
            {
                if (string.Compare(value, _watchPath, true) != 0)
                {
                    LoggerManager.Debug($"[FileWatcher] WatchPathUpdated : {_watchPath} -> {value}");
                    Reset();
                    _watchPath = value;

                    if (System.IO.Path.GetExtension(_watchPath) == "")
                        _watchDirectory = _watchPath;
                    else
                        _watchDirectory = System.IO.Path.GetDirectoryName(_watchPath);
                }
            }
        }

        private string _watchDirectory ;
        /// <summary>
        /// 감시 대상 폴더
        /// </summary>
        public string WatchDirectory
        {
            get => _watchDirectory;
        }

        /// <summary>
        /// 파일 캐싱
        /// </summary>
        private Dictionary<string, string> fileCache = new Dictionary<string, string>();

        public IEnumerable<string> GetCasheItems() { return fileCache.Keys; }

        /// <summary>
        /// 파일 변경 감지하여 캐시
        /// </summary>
        /// <param name="filePath"></param>
        public FileWatcher()
        {
        }

        public void Dispose()
        {
            Reset();
        }

        /// <summary>
        /// 파일이 변경된 경우 캐시를 삭제
        /// </summary>
        /// <param name="filePath"></param>
        private void UpdateFileCache(string filePath)
        {
            if (fileCache.ContainsKey(filePath))
            {
                fileCache.Remove(filePath);
            }
        }
        /// <summary>
        /// 캐시되어 있는 파일의 내용 가져오기
        /// 캐시가 없는 경우 파일을 읽어 캐싱. 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public string ReadAllText(string filePath)
        {
            filePath = GetValidPath(filePath);

            if (string.IsNullOrWhiteSpace(filePath))
                return null;

            if (!fileCache.ContainsKey(filePath))
            {
                if (!System.IO.File.Exists(filePath))
                {
                    fileCache[filePath] = null;
                }
                else
                { 
                    fileCache[filePath] = System.IO.File.ReadAllText(filePath);
                }
                LoggerManager.Debug($"[FileWatcher] FileCache {filePath} : {fileCache[filePath]}");

            }

            return fileCache[filePath];
        }

        /// <summary>
        /// 요청된 경로를 감시 폴더와 합쳐서 정상적인 경로로 만들어줌.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string GetValidPath(string fileName)
        {
            if (string.IsNullOrWhiteSpace(WatchDirectory))
                return null;

            

            if (fileName.ElementAt(1) == ':') // 드라이브 경로가 포함된 경우. 
            {
                if (fileName.Length > WatchDirectory.Length)//감시 폴더와 동일한 폴더를 가진 경우에만 예외 처리해줌. 
                {
                    if (string.Compare(WatchDirectory, 0, fileName, 0, WatchDirectory.Length, true) == 0)
                    {
                        fileName = fileName.Substring(WatchDirectory.Length + 1);
                    }
                    else//throw 다른 경로 처리 안함. 
                    {
                        return null;
                    }
                }
                else//throw 다른 경로 처리 안함.
                {
                    return null;
                }
            }

            return System.IO.Path.Combine(WatchDirectory, fileName);
        } 
       

        public void Reset()
        {
            fileCache.Clear();
        }
    }
}
