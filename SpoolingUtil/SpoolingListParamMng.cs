using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Security.Cryptography;

namespace SpoolingUtil
{
    using ProberErrorCode;
    using ProberInterfaces;
    using LogModule;

    /// <summary>
    /// Spooling list param을 관리하는 class    
    /// </summary>
    public class SpoolingListParamMng : IFactoryModule
    {
        private readonly string key_add_string = "semics";
        private readonly string spoolingUnprocedFileName = "Spooling_Unprocessed.json";
        private readonly string spoolingProcessingFileName = "Spooling_Processing.json";
        private SpoolingListParameter UnprocessdSpoolingListParam = new SpoolingListParameter();
        private SpoolingListParameter ProcessingSpoolingListParam = new SpoolingListParameter();
        public string unprocessedListPath { get; set; } = "";
        public string processingListPath { get; set; } = "";
        private object lockObject = new object();

        public SpoolingListParameter Processing_SpoolingListParam
        { 
            get 
            {
                return ProcessingSpoolingListParam;
            }
            set
            {
                ProcessingSpoolingListParam = value;
            }
        }

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="strSpoolingListBasePath"> spooling item 정보가 생성되는 base path</param>
        public SpoolingListParamMng(string strSpoolingListBasePath)
        {
            unprocessedListPath = Path.Combine(strSpoolingListBasePath, spoolingUnprocedFileName);
            processingListPath = Path.Combine(strSpoolingListBasePath, spoolingProcessingFileName);
            LoadSpoolingListParam(unprocessedListPath, ref UnprocessdSpoolingListParam); // 처리될 item load
            LoadSpoolingListParam(processingListPath, ref ProcessingSpoolingListParam);  // 처리중이었던 item load
        }

        /// <summary>
        /// spooling list file을 load한다.
        /// </summary>
        /// <param name="SpoolingListFilePath"> load 할 file path</param>
        /// <param name="param"> load한 data를 가지고 있는 param obj</param>
        private void LoadSpoolingListParam(string SpoolingListFilePath, ref SpoolingListParameter param)
        {
            try
            {
                if (false == File.Exists(SpoolingListFilePath))
                    return;

                EventCodeEnum retVal = EventCodeEnum.NONE;
                IParam tmpParam = null;
                retVal = this.LoadParameter(ref tmpParam, typeof(SpoolingListParameter), null, SpoolingListFilePath);
                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[Spooling] {MethodBase.GetCurrentMethod().Name} - Failed to load Spooling list param. (path ={SpoolingListFilePath})");
                }
                else
                {
                    param = tmpParam as SpoolingListParameter;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// 인자로 들어온 file과 addKeywork를 합쳐 MD5 Hash를 반환
        /// </summary>
        /// <param name="targetFilePath"> md5 hash를 생성할 file 경로</param>        
        /// <returns>md5 hash string</returns>
        public string GetSpoolingKeyInfo(string targetFilePath)
        {
            try
            {
                byte[] KeywordByte = Encoding.UTF8.GetBytes(key_add_string);
                using (var md5 = MD5.Create())
                {
                    using (FileStream fileStream = new FileStream(targetFilePath, FileMode.Open, FileAccess.Read))
                    {
                        int fileLength = (int)fileStream.Length;
                        if (fileLength == 0)
                        {
                            LoggerManager.Error($"[Spooling] {MethodBase.GetCurrentMethod().Name} - file length is zero. (path={targetFilePath})");
                            return "";
                        }

                        byte[] readData = new byte[fileLength + key_add_string.Length + 1];
                        if (fileStream.Read(readData, 0, (int)fileStream.Length) > 0)
                        {
                            Array.Copy(KeywordByte, 0, readData, fileLength, KeywordByte.Length);
                            var comHash = md5.ComputeHash(readData);
                            var hashValue = BitConverter.ToString(comHash).Replace("-", String.Empty).ToLower();
                            return hashValue;
                        }
                    }
                }
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
            }

            return "";
        }

        /// <summary>
        /// param data를 파일로 저장
        /// </summary>
        /// <param name="saveParam"> 저장할 param ojbect </param>
        /// <param name="savePath"> 저장할 path </param>
        private void SaveParam(ref SpoolingListParameter saveParam, string savePath)
        {
            try
            {
                EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
                RetVal = Extensions_IParam.SaveParameter(null, saveParam, null, savePath);
                if (EventCodeEnum.NONE != RetVal)
                    LoggerManager.Error($"[Spooling] {MethodBase.GetCurrentMethod().Name} - Failed to save 'Spooling list param'. (path ={savePath})");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// spooling item 추가(file로 바로 반영)
        /// </summary>
        /// <param name="targetFilePath"> upload할 file path</param>
        /// <param name="uploadSubPath"> upload 시 배치되어야 하는 sub path</param>
        /// <param name="cell_idx"> cell idx</param>
        /// <param name="useBinary"> 재 전송 시 binary 처리되어야 할지 여부(ftp 전송방식에서 사용)</param>
        /// <param name="unprocessedList"> 추가될 곳이 unprocessed인지 processing 쪽인지</param>
        /// note : thread unsafe하기 때문에 호출하는 곳쪽에 동기화 처리 할것
        public void AddSpoolingItem(string targetFilePath, string uploadSubPath, int cell_idx, bool useBinary, bool unprocessedList = true)
        {
            try
            {
                string savePath;
                SpoolingListParameter paramObj;
                if (unprocessedList)
                {
                    paramObj = UnprocessdSpoolingListParam;
                    savePath = unprocessedListPath;
                }
                else
                {
                    paramObj = ProcessingSpoolingListParam;
                    savePath = processingListPath;
                }

                string keyMD5 = GetSpoolingKeyInfo(targetFilePath);
                if (false == string.IsNullOrEmpty(keyMD5))
                {
                    lock (lockObject)
                    {
                        string insertTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        paramObj.SpoolingListParam.Add(new SpoolingListItem(targetFilePath, uploadSubPath, keyMD5, insertTime, cell_idx, useBinary));
                        SaveParam(ref paramObj, savePath);
                    }
                }
                else
                {
                    LoggerManager.Error($"[Spooling] {MethodBase.GetCurrentMethod().Name} - target file hash is empty. (path ={targetFilePath})");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// 인자로 들어온 Item에 해당하는것을 삭제 후 file flush
        /// </summary>
        /// <param name="key"> spooling param에 있는 key 항목</param>
        public void DeleteSpoolingItem(string key, bool processedList = true)
        {
            try
            {
                string savePath;
                SpoolingListParameter paramObj;
                if (processedList)
                {
                    paramObj = ProcessingSpoolingListParam;
                    savePath = processingListPath;
                }
                else
                {
                    paramObj = UnprocessdSpoolingListParam;
                    savePath = unprocessedListPath;
                }

                lock (lockObject)
                {
                    var itemFind = paramObj.SpoolingListParam.Where(x => x.Key == key).FirstOrDefault();
                    if (itemFind != null)
                    {
                        if (paramObj.SpoolingListParam.Remove(itemFind))
                        {
                            SaveParam(ref paramObj, savePath);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }                   
        }

        /// <summary>
        /// unporcessed에 있는 Item을 Processed로 이동한다.
        /// </summary>
        public void MoveUnprocessdToProdessd()
        {
            try
            {
                lock (lockObject)
                {
                    if (UnprocessdSpoolingListParam.SpoolingListParam.Count <= 0)
                        return;

                    foreach (var item in UnprocessdSpoolingListParam.SpoolingListParam)
                    {
                        ProcessingSpoolingListParam.SpoolingListParam.Add(item);
                    }

                    SaveParam(ref ProcessingSpoolingListParam, processingListPath);

                    UnprocessdSpoolingListParam.SpoolingListParam.Clear();
                    SaveParam(ref UnprocessdSpoolingListParam, unprocessedListPath);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// spooling prodessing item을 파일로 저장
        /// </summary>
        public void SaveProcessingListParam()
        {
            SaveParam(ref ProcessingSpoolingListParam, processingListPath);
        }
    }
}
