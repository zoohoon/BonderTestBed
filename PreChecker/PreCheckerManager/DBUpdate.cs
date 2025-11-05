using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace PreCheckerManager
{
    class DBUpdate
    {
        public static void DiffHashFile()
        {
            try
            {
                string[] args = Environment.GetCommandLineArgs();

                string csvHashFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CsvHashFile.ini");
                string installFilePath = @"C:\Program Files (x86)\Maestro";
                bool bTryChange = false;

                if (!File.Exists(csvHashFile))
                {
                    bTryChange = true;
                }
                else
                {
                    // Install 경로에 존재하는 csv는 parameter 파일들만 존재하기 때문에 전체를 가져오도록 처리함.
                    string[] csvFiles = Directory.GetFiles(installFilePath, $"*ParameterData.csv");
                    foreach (string file in csvFiles)
                    {
                        using (MD5 md5 = MD5.Create())
                        {
                            using (FileStream fileStream = File.OpenRead(file))
                            {
                                // 대상 csv 파일 => CommonParameterData.csv, SysParameterData.csv, DevParameterData.csv
                                // Install 경로에 있는 각 csv 파일에 대한 hash를 뜬다.
                                byte[] comHash = md5.ComputeHash(fileStream);
                                string hashVal = BitConverter.ToString(comHash).Replace("-", String.Empty).ToLower();
                                string key = (Path.GetFileName(file).Replace(".csv", ""));
                                var iniHashVal = GetHashValue(csvHashFile, key);
                                // Inatall 경로(C:\Program Files (x86)\Maestro)에 있는 각 csv 파일 hash 값과 기존 hash 값(C:\ProberSystem\DB 경로에 csv파일에 대해 미리 떠놓은 hash 값)을 비교한다.
                                if (!hashVal.Equals(iniHashVal))
                                {
                                    FileProcess.WriteDebugLog($"[File Comparison] The hash value is different.");
                                    FileProcess.WriteDebugLog($"Hash Value - [{key}.csv] : {hashVal}, CsvHashFile.ini : {iniHashVal}");
                                    bTryChange = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (bTryChange)
                {
                    ChangeDBUpdateFlag();
                }
            }
            catch (Exception err)
            {
                FileProcess.WriteExceptionLog(err);
            }
        }

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        private static string GetHashValue(string path, string key)
        {
            StringBuilder sb = new StringBuilder(255);
            GetPrivateProfileString("Hash", key, "", sb, sb.Capacity, path);
            return sb.ToString();
        }

        private static void ChangeDBUpdateFlag()
        {
            FileProcess.WriteDebugLog("[START] Change the flag of DB");

            string jsonFilePath = @"C:\ProberSystem\DB\DBManagerParam.json";
            if (!File.Exists(jsonFilePath)) return;

            string jsonDBMamager = File.ReadAllText(jsonFilePath);
            JObject jObject = JObject.Parse(jsonDBMamager);

            jObject["EnableDBSourceServer"] = true;
            jObject["EnableUpdateDBFile"] = true;

            FileProcess.WriteDebugLog($"Flag - EnableDBSourceServer : {jObject["EnableDBSourceServer"]}, EnableUpdateDBFile : {jObject["EnableUpdateDBFile"]}");

            File.WriteAllText(jsonFilePath, jObject.ToString());

            FileProcess.WriteDebugLog("[END] Change the flag of DB");
        }
    }
}
