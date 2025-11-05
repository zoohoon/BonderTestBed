using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileUtil
{
    public static class FileUtility
    {
        public static void CreateDirectory(string path)
        {
            string[] dirs = path.Split('\\');

            string createPath = dirs[0];
            for (var idx = 1; idx < dirs.Length; idx++)
            {
                createPath += '\\' + dirs[idx];

                try
                {
                    if (!Directory.Exists(createPath))
                    {
                        Directory.CreateDirectory(createPath);
                    }
                }
                catch (Exception)
                {
                }
            }
        }
        public static void DeleteDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            catch (Exception)
            {
            }
        }
        public static void FileCopy(string src, string dst)
        {
            try
            {
                if (File.Exists(dst))
                {
                    File.Delete(dst);
                }

                File.Copy(src, dst);
            }
            catch (Exception)
            {
            }
        }
        public static void DirectoryCopy(string src, string dst, bool recursive)
        {
            try
            {
                // Get information about the source directory
                var dir = new DirectoryInfo(src);

                // Check if the source directory exists
                if (!dir.Exists)
                    throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

                // Cache directories before we start copying
                DirectoryInfo[] dirs = dir.GetDirectories();

                // Create the destination directory
                Directory.CreateDirectory(dst);

                // Get the files in the source directory and copy to the destination directory
                foreach (FileInfo file in dir.GetFiles())
                {
                    string targetFilePath = Path.Combine(dst, file.Name);
                    file.CopyTo(targetFilePath);
                }

                // If recursive and copying subdirectories, recursively call this method
                if (recursive)
                {
                    foreach (DirectoryInfo subDir in dirs)
                    {
                        string newDestinationDir = Path.Combine(dst, subDir.Name);
                        DirectoryCopy(subDir.FullName, newDestinationDir, true);
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        public static List<string> GetFilteredFilesByCreationTime(DateTime startDTM, DateTime endDTM, string path)
        {
            List<string> retval = new List<string>();

            try
            {
                string[] dirs = Directory.GetDirectories(path);

                IEnumerable<string> files = null;

                files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Where(s => s.ToLower().EndsWith(".log") ||
                                                                   s.ToLower().EndsWith(".bmp") ||
                                                                   s.ToLower().EndsWith(".jpg") ||
                                                                   s.ToLower().EndsWith(".jpeg") ||
                                                                   s.ToLower().EndsWith(".png"));

                foreach (string f in files)
                {
                    try
                    {
                        FileInfo fInfo = new FileInfo(f);

                        if (fInfo.CreationTime >= startDTM && fInfo.CreationTime <= endDTM)
                        {
                            retval.Add(fInfo.FullName);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (Exception)
            {

            }

            return retval;
        }
        private static readonly List<string> lstSearchFilePeriod = new List<string>();
        public static List<string> DirFileSearchPeriod(List<string> unSelectedCell, DateTime startDTM, DateTime endDTM, string path, bool includeImageLog, bool firstSearch = false)
        {
            if (firstSearch)
            {
                lstSearchFilePeriod.Clear();
            }

            string[] dirs = Directory.GetDirectories(path);
            IEnumerable<string> files = null;
            if (includeImageLog)
            {
                files = Directory.GetFiles(path, "*.*").Where(s => s.ToLower().EndsWith(".log") || s.ToLower().EndsWith(".bmp"));
            }
            else
            {
                files = Directory.GetFiles(path, "*.log");
            }

            foreach (string f in files)
            {
                try
                {
                    FileInfo fInfo = new FileInfo(f);
                    if (fInfo.CreationTime >= startDTM && fInfo.CreationTime <= endDTM)
                    {
                        lstSearchFilePeriod.Add(fInfo.FullName);
                    }
                }
                catch (Exception)
                {
                }
            }

            if (dirs.Length > 0)
            {
                foreach (var dir in dirs)
                {
                    bool bExist = false;
                    foreach (var cell in unSelectedCell)
                    {

                        if (dir.Contains(cell))
                        {
                            bExist = true;
                            break;
                        }
                    }

                    if (bExist)
                    {
                        continue;
                    }

                    DirFileSearchPeriod(unSelectedCell, startDTM, endDTM, dir, includeImageLog);
                }
            }

            return new List<string>(lstSearchFilePeriod);
        }
        private static readonly List<string> lstFilterFiles = new List<string>();
        public static List<string> DirFileFilter(List<string> unSelectedCell, List<string> lstIncludeExt, List<string> lstExcludeDirName, string path, bool firstSearch = false)
        {
            if (firstSearch)
            {
                lstFilterFiles.Clear();
            }

            foreach (var excludeDir in lstExcludeDirName)
            {
                if (path.Contains(excludeDir))
                {
                    return null;
                }
            }

            if (!Directory.Exists(path))
            {
                return null;
            }

            string[] dirs = Directory.GetDirectories(path);
            string[] files = Directory.GetFiles(path);
            foreach (string f in files)
            {
                try
                {
                    FileInfo fInfo = new FileInfo(f);

                    foreach (var includeExt in lstIncludeExt)
                    {
                        if (fInfo.Extension.ToLower().Equals(includeExt.ToLower()))
                        {
                            lstFilterFiles.Add(fInfo.FullName);
                            break;
                        }
                    }
                }
                catch (Exception)
                {
                }
            }

            if (dirs.Length > 0)
            {
                foreach (string dir in dirs)
                {
                    bool bExist = false;
                    foreach (var cell in unSelectedCell)
                    {
                        if (dir.Contains(cell))
                        {
                            bExist = true;
                            break;
                        }
                    }

                    if (bExist)
                    {
                        continue;
                    }

                    DirFileFilter(unSelectedCell, lstIncludeExt, lstExcludeDirName, dir);
                }
            }

            return lstFilterFiles;
        }
        public static bool ZipDirectory(string srcDir, string outputZipPath)
        {
            try
            {
                if (File.Exists(outputZipPath))
                {
                    File.Delete(outputZipPath);
                }

                ZipFile.CreateFromDirectory(srcDir, outputZipPath);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static long GetDirectorySize(string directoryPath)
        {
            long size = 0;

            try
            {
                DirectoryInfo directory = new DirectoryInfo(directoryPath);

                foreach (FileInfo file in directory.GetFiles("*.*", SearchOption.AllDirectories))
                {
                    size += file.Length;
                }
            }
            catch (Exception)
            {
                // Handle any exceptions that may occur
            }

            return size;
        }

        /// <summary>
        /// Get Disk Free Size
        /// </summary>
        /// <param name="driveName">C or D or ...</param>
        /// <returns>Unit : MB</returns>
        public static int GetDiskFreeSize(string driveName)
        {
            int freeSize = 0;
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType != DriveType.Fixed)
                {
                    continue;
                }

                if (drive.Name.Contains(driveName))
                {
                    //string totalSize = Convert.ToInt32(drive.TotalSize / 1024 / 1024 / 1024).ToString(); //전체 사이즈
                    freeSize = Convert.ToInt32(drive.AvailableFreeSpace / 1024 / 1024); //남은 사이즈
                    break;
                }
            }

            return freeSize;
        }
    }
}
