using LogModule.LoggerParam;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;

namespace LogModule
{
    public class LogTransfer
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private bool _IsChecked;
        public bool IsChecked
        {
            get { return _IsChecked; }
            set
            {
                if (value != _IsChecked)
                {
                    _IsChecked = value;
                    RaisePropertyChanged();
                }
            }
        }

        public LoggerManagerParameter LoggerManagerParam { get; private set; }
        List<string> FilePathList;
        public LogTransfer()
        {

        }
        public List<List<string>> UpdateLogFile(List<NLoggerParam> nLogparamList)
        {
            FilePathList = new List<string>();

            for (int i = 0; i < nLogparamList.Count; i++)
            {
                FilePathList.Add(nLogparamList[i].LogDirPath);
            }

            List<List<string>> retval = new List<List<string>>();
            List<List<string>> LogFileList = new List<List<string>>();

            try
            {
                for (int i = 0; i < FilePathList.Count; i++)
                {
                    if (Directory.Exists(FilePathList[i]) == false)
                    {
                        Directory.CreateDirectory(FilePathList[i]);
                    }

                    string[] result = FilePathList[i].Split(new string[] { "\\" }, StringSplitOptions.None);
                    LogFileList.Add(new List<string>() { result[result.Length - 1] });

                    foreach (string path in Directory.GetFiles(FilePathList[i]))
                    {
                        LogFileList[i].Add(path);
                    }
                }

                retval = LogFileList;

                return retval;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return retval;
            }
        }
        public byte[] OpenLogFile(string selectedFilePath)
        {
            string filePath;
            byte[] retval = null;
            filePath = selectedFilePath;

            retval = CompressFileToStream(filePath);

            return retval;
        }

        private byte[] CompressFileToStream(string filePath)
        {
            byte[] byteArray = null;
            byte[] retArray = null;
            
            try
            {
                if (filePath != null)
                {
                    if (File.Exists(filePath))
                    {
                        var bytes = File.ReadAllBytes(filePath);
                        using (MemoryStream stream = new MemoryStream())
                        {
                            stream.Write(bytes, 0, bytes.Length);
                            stream.Position = 0;

                            byte[] buffer = new byte[16 * 1024];
                            using (MemoryStream ms = new MemoryStream())
                            {
                                int read;
                                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    ms.Write(buffer, 0, read);
                                }
                                byteArray = ms.ToArray();
                            }
                            retArray = Compress(byteArray);
                        }                            
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retArray;
        }
        public byte[] Compress(byte[] data)
        {
            byte[] retVal = null;

            using(MemoryStream output = new MemoryStream())
            {
                using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal))
                {
                    dstream.Write(data, 0, data.Length);
                }
                retVal = output.ToArray();
            }

            return retVal;
        }
    }
}
