using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Xml;

namespace PreCheckerManager
{
    class FileProcess
    {
        public static void FuncFileProcess(string updateFilesDir, string bakDir)
        {
            var loaderDir = @"C:\ProberSystem\LoaderSystem\EMUL\Parameters\";
            var tmpDefaultDir = "";

            try
            {
                WriteDebugLog($"[START] Update Files");

                // 1. Install시, 삭제/붙여넣기 등 작업이 필요한 파일 정보가 담긴 json을 읽는다.
                var jsonFilePath = Path.Combine(updateFilesDir, "UpdateFilesInfo.json");
                if (!File.Exists(jsonFilePath))
                {
                    WriteDebugLog($"[END] not exists - {jsonFilePath}");
                    return;
                }

                var updatefileStr = File.ReadAllText(jsonFilePath);
                JObject jObj = JObject.Parse(updatefileStr);

                // 작업 directory를 생성할 때, 미리 backup directory도 담아 놓는다.
                Dictionary<string, string> cellBakDic = new Dictionary<string, string>();

                foreach (JToken action in jObj.Value<JToken>("Action"))
                {
                    var sysTypeName = "";
                    var paramDir = "";
                    var tmpBakDir = "";

                    var dir = action["dir"]?.ToString();
                    // Cell은 갯수만큼 반복 해야하기 때문에 공통으로 사용하기 위해 List로 처리함.
                    List<String> defaultDirList = new List<String>();

                    if (dir.Contains("Loader"))
                    {
                        // Loader Directory
                        sysTypeName = "Loader";
                        paramDir = dir.Replace("Loader\\", "");
                        tmpDefaultDir = Path.Combine(loaderDir, paramDir);
                        defaultDirList.Add(tmpDefaultDir);
                        tmpBakDir = Path.Combine(bakDir, sysTypeName);
                    }
                    else if (dir.Contains("Cell"))
                    {
                        sysTypeName = "Cell";
                        paramDir = dir.Replace("Cell\\", "");

                        // Cell Directory
                        for (var i = 1; i <= 12; i++)
                        {
                            var cellNum = $"C{i.ToString("00")}";
                            var cellDir = $@"C:\ProberSystem\{cellNum}\Parameters\";
                            if (!Directory.Exists(cellDir))
                            {
                                continue;
                            }
                            tmpDefaultDir = Path.Combine(cellDir, paramDir);
                            defaultDirList.Add(tmpDefaultDir);

                            if (!cellBakDic.ContainsKey(cellNum))
                            {
                                var cellBakDir = Path.Combine(bakDir, sysTypeName, cellNum);
                                cellBakDic.Add(cellNum, cellBakDir);
                            }
                        }
                    }
                    else
                    {
                        // dir에 full 경로가 있기 때문에 
                        defaultDirList.Add(dir);
                        tmpBakDir = Path.Combine(bakDir, "ETC");
                    }

                    // directory를 돌면서 각 작업을 수행한다.
                    foreach (var defaultDir in defaultDirList)
                    {
                        var cmd = action["cmd"]?.ToString();
                        if (cmd.Equals("rm")) // rm => rmove file (파일 삭제)
                        {
                            var rmFileName = action["name"]?.ToString();
                            var rmFile = Path.Combine(defaultDir, rmFileName);

                            if (!File.Exists(rmFile))
                            {
                                WriteDebugLog($"[File Delete] not exists - {rmFile}");
                                continue;
                            }
                            else
                            {
                                // 백업 할 cell 경로를 찾는다.
                                if (sysTypeName.Equals("Cell"))
                                {
                                    foreach (var cbDic in cellBakDic)
                                    {
                                        if (rmFile.Contains(cbDic.Key))
                                        {
                                            tmpBakDir = cbDic.Value?.ToString();
                                            break;
                                        }
                                    }
                                }

                                // 지워야 할 파일이 있으면 백업 폴더로 복사부터 한다.
                                var rmBakDir = Path.Combine(tmpBakDir, paramDir);
                                if (!Directory.Exists(rmBakDir))
                                {
                                    Directory.CreateDirectory(rmBakDir);
                                }
                                var rmBakFile = Path.Combine(rmBakDir, rmFileName);

                                File.Copy(rmFile, rmBakFile);
                            }

                            // 파일을 삭제한다.
                            File.Delete(rmFile);
                            WriteDebugLog($"[File Deleted] Target - {rmFile}");
                        }
                        else if (cmd.Equals("cpdir")) // cpdir => copy directory (폴더 복사)
                        {
                            var srcDirName = action["src"]?.ToString();
                            var dstDirName = action["dst"]?.ToString();
                            var dstDir = Path.Combine(defaultDir, dstDirName);
                            var srcDir = Path.Combine(updateFilesDir, sysTypeName, srcDirName);
                            var cpBakDir = "";

                            if (!Directory.Exists(srcDir) || !Directory.Exists(dstDir))
                            {
                                WriteDebugLog($"[Folder Copy] not exists - {srcDir} or {dstDir}");
                                continue;
                            }

                            // 백업 할 cell 경로를 찾는다.
                            if (sysTypeName.Equals("Cell"))
                            {
                                foreach (var cbDic in cellBakDic)
                                {
                                    if (dstDir.Contains(cbDic.Key))
                                    {
                                        tmpBakDir = cbDic.Value?.ToString();
                                        break;
                                    }
                                }
                            }

                            if (srcDirName.Contains("Utility"))
                            {
                                // zip파일을 풀 경로에 같은 폴더가 있는지 체크하고 있으면 백업 폴더로 기존 폴더를 복사한다.
                                if (Directory.Exists(dstDir))
                                {
                                    cpBakDir = Path.Combine(tmpBakDir, dstDirName);
                                    string cpBakTempDir = Directory.GetParent(cpBakDir).FullName;
                                    if (!Directory.Exists(cpBakTempDir))
                                    {
                                        Directory.CreateDirectory(cpBakTempDir);
                                    }
                                    Directory.Move(dstDir, cpBakDir);

                                    // zip파일을 만든다.
                                    var zipDir = srcDir + ".zip";
                                    if (Directory.Exists(zipDir))
                                    {
                                        Directory.Delete(zipDir);
                                    }
                                    ZipFile.CreateFromDirectory(srcDir, zipDir);

                                    // zip 파일 압축을 푼다.
                                    ZipFile.ExtractToDirectory(zipDir, dstDir);
                                    WriteDebugLog($"[Extract] Target - {dstDir}");

                                    // zip 파일을 삭제한다.
                                    File.Delete(zipDir);
                                    WriteDebugLog($"[File Deleted] Target - {zipDir}");
                                    WriteDebugLog($"[Folder Copied] Source - {srcDir} , Target - {dstDir}");
                                }
                            }
                            else
                            {
                                dstDir = Path.Combine(defaultDir, dstDirName);

                                // 폴더 하위에 있는 모든 파일을 복사한다.
                                string[] files = Directory.GetFiles(srcDir);
                                foreach (var file in files)
                                {
                                    var cpFileName = Path.GetFileName(file);
                                    var cpFile = Path.Combine(dstDir, cpFileName);
                                    // 파일이 있으면 백업 폴더로 복사부터 한다.
                                    if (File.Exists(cpFile))
                                    {
                                        cpBakDir = Path.Combine(tmpBakDir, paramDir, dstDirName);
                                        if (!Directory.Exists(cpBakDir))
                                        {
                                            Directory.CreateDirectory(cpBakDir);
                                        }
                                        var cpBakFile = Path.Combine(cpBakDir, cpFileName);

                                        File.Copy(cpFile, cpBakFile);
                                    }

                                    // 파일을 복사한다.
                                    File.Copy(file, cpFile, true);
                                    WriteDebugLog($"[File Copied] Source - {file} , Target - {cpFile}");
                                }
                            }
                        }
                        else if (cmd.Equals("vi")) // vi => 편집 (파일 내용 변경)
                        {
                            bool isFileChanged = false;
                            var viFileName = action["name"]?.ToString();
                            var viFile = Path.Combine(defaultDir, viFileName);

                            if (File.Exists(viFile))
                            {
                                // 백업 할 cell 경로를 찾는다.
                                if (sysTypeName.Equals("Cell"))
                                {
                                    foreach (var cbDic in cellBakDic)
                                    {
                                        if (viFile.Contains(cbDic.Key))
                                        {
                                            tmpBakDir = cbDic.Value?.ToString();
                                            break;
                                        }
                                    }
                                }

                                // 파일이 있으면 백업 폴더로 복사부터 한다.
                                var viBakDir = Path.Combine(tmpBakDir, paramDir);
                                if (!Directory.Exists(viBakDir))
                                {
                                    Directory.CreateDirectory(viBakDir);
                                }
                                var viBakFile = Path.Combine(viBakDir, viFileName);
                                File.Copy(viFile, viBakFile);


                                // 파일 내용을 변경한다.
                                if (viFileName.Equals("ModuleCount.json"))
                                {
                                    #region ModuleCount
                                    // C:\ProberSystem\SystemInfo에 ModuleCount.json을 변경한다. (현재는 M사에만 적용)
                                    XmlDocument xDoc = new XmlDocument();
                                    xDoc.Load(viFile);

                                    XmlNamespaceManager xNameSpaceManager = new XmlNamespaceManager(xDoc.NameTable);
                                    xNameSpaceManager.AddNamespace("sysInfo", "clr-namespace:ProberInterfaces;assembly=ProberInterfaces");

                                    var deviceName = xDoc.SelectSingleNode("sysInfo:ModuleCount/sysInfo:CardBufferCount", xNameSpaceManager);
                                    if (deviceName != null && !string.IsNullOrEmpty(deviceName.InnerText))
                                    {
                                        // CardBufferCount를 2로 변경한다.
                                        deviceName.InnerText = "2";
                                        isFileChanged = true;
                                    }

                                    if (isFileChanged)
                                    {
                                        // 변경한 값을 저장한다.
                                        xDoc.Save(viFile);
                                        WriteDebugLog($"[File Changed] Target - {viFile}");
                                    }
                                    #endregion
                                }
                                else if (viFileName.Equals("VisionProcParameter.json")) // 모든 Cell 폴더에 적용한다.
                                {
                                    #region VisionProcParameter
                                    var viFileStr = File.ReadAllText(viFile);
                                    JObject jObject = JObject.Parse(viFileStr);
                                    // MaxFocusFlatness 값을 70.0으로 변경한다.
                                    if (jObject.ContainsKey("MaxFocusFlatness"))
                                    {
                                        var orgMaxFocusFlatnessVal = jObject["MaxFocusFlatness"]["Value"]?.ToString();
                                        jObject["MaxFocusFlatness"]["Value"] = 70.0;
                                        WriteDebugLog($"MaxFocusFlatness - orgVal : {orgMaxFocusFlatnessVal}, newVal : {jObject["MaxFocusFlatness"]["Value"]}");
                                        isFileChanged = true;
                                    }
                                    else
                                    {
                                        WriteDebugLog($"[File Changed] not exist Property MaxFocusFlatness Target - {viFile}");
                                    }

                                    if (jObject.ContainsKey("FocusFlatnessTriggerValue"))
                                    {
                                        // FocusFlatnessTriggerValue 값을 1.5로 변경한다.
                                        var orgFocusFlatnessTriggerVal = jObject["FocusFlatnessTriggerValue"]["Value"]?.ToString();
                                        jObject["FocusFlatnessTriggerValue"]["Value"] = 1.5;
                                        WriteDebugLog($"FocusFlatnessTriggerValue - orgVal : {orgFocusFlatnessTriggerVal}, newVal : {jObject["FocusFlatnessTriggerValue"]["Value"]}");
                                        isFileChanged = true;
                                    }
                                    else
                                    {
                                        WriteDebugLog($"[File Changed] not exist Property FocusFlatnessTriggerValue Target - {viFile}");
                                    }

                                    if (isFileChanged)
                                    {
                                        // 변경한 값을 저장한다.
                                        File.WriteAllText(viFile, jObject.ToString());
                                        WriteDebugLog($"[File Changed] Target - {viFile}");
                                    }
                                    #endregion
                                }
                                else if (viFileName.Equals("MarkAlignerParameter.json"))
                                {
                                    #region MarkAlignerParameter
                                    var viFileStr = File.ReadAllText(viFile);
                                    JObject jObject = JObject.Parse(viFileStr);

                                    if (jObject.ContainsKey("MarkDiffTolerance_X"))
                                    {
                                        // MarkDiffTolerance_X Default 값을 100으로 변경한다.
                                        var orgMarkDiffToleranceXVal = jObject["MarkDiffTolerance_X"]["Value"]?.ToString();
                                        jObject["MarkDiffTolerance_X"]["Value"] = 100.0;
                                        WriteDebugLog($"MarkDiffTolerance_X - orgVal : {orgMarkDiffToleranceXVal}, newVal : {jObject["MarkDiffTolerance_X"]["Value"]}");
                                        isFileChanged = true;
                                    }
                                    else
                                    {
                                        WriteDebugLog($"[File Changed] not exist Property MarkDiffTolerance_X Target - {viFile}");
                                    }

                                    if (jObject.ContainsKey("MarkDiffTolerance_Y"))
                                    {
                                        // MarkDiffTolerance_Y Default 값을 100으로 변경한다.
                                        var orgMarkDiffToleranceYVal = jObject["MarkDiffTolerance_Y"]["Value"]?.ToString();
                                        jObject["MarkDiffTolerance_Y"]["Value"] = 100.0;
                                        WriteDebugLog($"MarkDiffTolerance_Y - orgVal : {orgMarkDiffToleranceYVal}, newVal : {jObject["MarkDiffTolerance_Y"]["Value"]}");
                                        isFileChanged = true;
                                    }
                                    else
                                    {
                                        WriteDebugLog($"[File Changed] not exist Property MarkDiffTolerance_Y Target - {viFile}");
                                    }

                                    if (isFileChanged)
                                    {
                                        // 변경한 값을 저장한다.
                                        File.WriteAllText(viFile, jObject.ToString());
                                        WriteDebugLog($"[File Changed] Target - {viFile}");
                                    }
                                    #endregion
                                }
                                else if (viFileName.Equals("IOMapping.json"))
                                {
                                    var viFileStr = File.ReadAllText(viFile);
                                    JObject jObject = JObject.Parse(viFileStr);

                                    //Cell
                                    if (defaultDir.Contains("LoaderSystem"))
                                    {
                                        //Loader
                                        var jObj_RemoteOutputs = (JObject)jObject.SelectToken("RemoteOutputs");
                                        var jObj_RemoteInputs = (JObject)jObject.SelectToken("RemoteInputs");

                                        #region DOCCArmVac_Break 
                                        if (!jObj_RemoteOutputs.ContainsKey("DOCCArmVac_Break"))
                                        {
                                            var jObj_ArmVac_Break = new JObject();
                                            var jObj_ArmVac_Break_TimeSub = new JObject();
                                            jObj_ArmVac_Break_TimeSub.Add("Value", 0);
                                            jObj_ArmVac_Break_TimeSub.Add("SetupState", "DEFAULT");
                                            jObj_ArmVac_Break.Add("TimeOut", jObj_ArmVac_Break_TimeSub);
                                            jObj_ArmVac_Break.Add("MaintainTime", jObj_ArmVac_Break_TimeSub);
                                            var jObj_ArmVac_Break_IOType = new JObject();
                                            jObj_ArmVac_Break_IOType.Add("Value", "OUTPUT");
                                            jObj_ArmVac_Break_IOType.Add("SetupState", "DEFAULT");
                                            jObj_ArmVac_Break.Add("IOType", jObj_ArmVac_Break_IOType);
                                            var jObj_ArmVac_Break_IOOveride = new JObject();
                                            jObj_ArmVac_Break_IOOveride.Add("Value", "NONE");
                                            jObj_ArmVac_Break_IOOveride.Add("SetupState", "DEFAULT");
                                            jObj_ArmVac_Break.Add("IOOveride", jObj_ArmVac_Break_IOOveride);
                                            var jObj_ArmVac_Break_ChannelIdx = new JObject();
                                            jObj_ArmVac_Break_ChannelIdx.Add("Value", 0);
                                            jObj_ArmVac_Break_ChannelIdx.Add("SetupState", "DEFAULT");
                                            jObj_ArmVac_Break.Add("ChannelIndex", jObj_ArmVac_Break_ChannelIdx);
                                            var jObj_ArmVac_Break_PortIdx = new JObject();
                                            jObj_ArmVac_Break_PortIdx.Add("Value", 0);
                                            jObj_ArmVac_Break_PortIdx.Add("SetupState", "DEFAULT");
                                            jObj_ArmVac_Break.Add("PortIndex", jObj_ArmVac_Break_PortIdx);
                                            var jObj_ArmVac_Break_Reverse = new JObject();
                                            jObj_ArmVac_Break_Reverse.Add("Value", false);
                                            jObj_ArmVac_Break_Reverse.Add("SetupState", "DEFAULT");
                                            jObj_ArmVac_Break.Add("Reverse", jObj_ArmVac_Break_Reverse);
                                            var jObj_ArmVac_Break_KeyVal = new JObject();
                                            jObj_ArmVac_Break_KeyVal.Add("Value", "DOCCArmVac_Break");
                                            jObj_ArmVac_Break_KeyVal.Add("SetupState", "DEFAULT");
                                            jObj_ArmVac_Break.Add("Key", jObj_ArmVac_Break_KeyVal);
                                            var jObj_ArmVac_Break_Description = new JObject();
                                            jObj_ArmVac_Break_Description.Add("Value", null);
                                            jObj_ArmVac_Break_Description.Add("SetupState", "DEFAULT");
                                            jObj_ArmVac_Break.Add("Description", jObj_ArmVac_Break_Description);

                                            jObj_RemoteOutputs.Add("DOCCArmVac_Break", jObj_ArmVac_Break);
                                            isFileChanged = true;

                                            WriteDebugLog($"[File Changed] Created json object DOCCArmVac_Break - {viFile}");
                                        }                                        
                                        else
                                        {
                                            //NONE
                                        }
                                        #endregion

                                        #region DILD_PCW_LEAK_STATE                                        
                                        if (!jObj_RemoteInputs.ContainsKey("DILD_PCW_LEAK_STATE"))
                                        {
                                            var jObj_DILD_PCW_LEAK_STATE = new JObject();
                                            var jObj_DILD_PCW_LEAK_STATE_Timeout = new JObject();
                                            jObj_DILD_PCW_LEAK_STATE_Timeout.Add("Value", 60000);
                                            jObj_DILD_PCW_LEAK_STATE.Add("TimeOut", jObj_DILD_PCW_LEAK_STATE_Timeout);
                                            var jObj_DILD_PCW_LEAK_STATE_MaintaintTime = new JObject();
                                            jObj_DILD_PCW_LEAK_STATE_MaintaintTime.Add("Value", 10);
                                            jObj_DILD_PCW_LEAK_STATE.Add("MaintainTime", jObj_DILD_PCW_LEAK_STATE_MaintaintTime);
                                            var jObj_DILD_PCW_LEAK_STATE_IOType = new JObject();
                                            jObj_DILD_PCW_LEAK_STATE_IOType.Add("Value", "INPUT");
                                            jObj_DILD_PCW_LEAK_STATE.Add("IOType", jObj_DILD_PCW_LEAK_STATE_IOType);
                                            var jObj_ArmVac_Break_IOOveride = new JObject();
                                            jObj_ArmVac_Break_IOOveride.Add("Value", "EMUL"); //기본 값을 EMUL이고, 실제 사용하는 사이트의 경우 추후 NONE으로 변경해줘야 함.
                                            jObj_DILD_PCW_LEAK_STATE.Add("IOOveride", jObj_ArmVac_Break_IOOveride);
                                            var jObj_DILD_PCW_LEAK_STATE_ChannelIdx = new JObject();
                                            jObj_DILD_PCW_LEAK_STATE_ChannelIdx.Add("Value", 4);
                                            jObj_DILD_PCW_LEAK_STATE.Add("ChannelIndex", jObj_DILD_PCW_LEAK_STATE_ChannelIdx);
                                            var jObj_DILD_PCW_LEAK_STATE_PortIdx = new JObject();
                                            jObj_DILD_PCW_LEAK_STATE_PortIdx.Add("Value", 0);
                                            jObj_DILD_PCW_LEAK_STATE.Add("PortIndex", jObj_DILD_PCW_LEAK_STATE_PortIdx);
                                            var jObj_DILD_PCW_LEAK_STATE_Reverse = new JObject();
                                            jObj_DILD_PCW_LEAK_STATE_Reverse.Add("Value", false);
                                            jObj_DILD_PCW_LEAK_STATE.Add("Reverse", jObj_DILD_PCW_LEAK_STATE_Reverse);
                                            var jObj_DILD_PCW_LEAK_STATE_KeyVal = new JObject();
                                            jObj_DILD_PCW_LEAK_STATE_KeyVal.Add("Value", "DILD_PCW_LEAK_STATE");
                                            jObj_DILD_PCW_LEAK_STATE.Add("Key", jObj_DILD_PCW_LEAK_STATE_KeyVal);
                                            var jObj_DILD_PCW_LEAK_STATE_Description = new JObject();
                                            jObj_DILD_PCW_LEAK_STATE_Description.Add("Value", "DILD_PCW_LEAK_STATE");
                                            jObj_DILD_PCW_LEAK_STATE.Add("Description", jObj_DILD_PCW_LEAK_STATE_Description);

                                            jObj_RemoteInputs.Add("DILD_PCW_LEAK_STATE", jObj_DILD_PCW_LEAK_STATE);
                                            isFileChanged = true;

                                            WriteDebugLog($"[File Changed] Created json object DILD_PCW_LEAK_STATE - {viFile}");
                                        }
                                        else
                                        { 
                                            //NONE
                                        }
                                        #endregion

                                        if (isFileChanged)
                                        {
                                            // 변경한 값을 저장한다.
                                            File.WriteAllText(viFile, jObject.ToString());
                                            WriteDebugLog($"[File Changed] Target - {viFile}");
                                        }
                                    }
                                    else
                                    {
                                        //Cell
                                        #region SystemType에 따른 IOOveride Value 설정
                                        XmlDocument xDoc = new XmlDocument();
                                        xDoc.Load(@"C:\ProberSystem\SystemInfo\SystemMode.json");
                                        XmlNamespaceManager xNameSpaceManager = new XmlNamespaceManager(xDoc.NameTable);
                                        xNameSpaceManager.AddNamespace("sysMode", "clr-namespace:ProberInterfaces;assembly=ProberInterfaces");
                                        var SystemType = xDoc.SelectSingleNode("sysMode:SystemMode/sysMode:SystemType", xNameSpaceManager)?.InnerText;

                                        var IOOveride_Val = "EMUL"; //Opera -> EMUL , DRAX -> NONE
                                        if (SystemType.Equals("DRAX"))
                                        {
                                            IOOveride_Val = "NONE";
                                        }
                                        else
                                        {
                                            //NONE
                                        }
                                        #endregion

                                        if (!jObject.ContainsKey("Outputs"))
                                        {
                                            WriteDebugLog($"[File Changed] Key does not exist. - Key : Outputs, File : {viFile}");
                                        }
                                        else
                                        {
                                            var jObj_Outputs = (JObject)jObject.SelectToken("Outputs");
                                            var jObj_Inputs = (JObject)jObject.SelectToken("Inputs");

                                            #region DOPOGOCARD_VACU_RELEASE_SUB
                                            if (!jObj_Outputs.ContainsKey("DOPOGOCARD_VACU_RELEASE_SUB"))
                                            {
                                                var jObj_Vacu_Release = new JObject();
                                                var jObj_Vacu_Release_TimeVal = new JObject();
                                                jObj_Vacu_Release_TimeVal.Add("Value", 0);
                                                jObj_Vacu_Release.Add("TimeOut", jObj_Vacu_Release_TimeVal);
                                                jObj_Vacu_Release.Add("MaintainTime", jObj_Vacu_Release_TimeVal);
                                                var jObj_Vacu_Release_IOType = new JObject();
                                                jObj_Vacu_Release_IOType.Add("Value", "OUTPUT");
                                                jObj_Vacu_Release.Add("IOType", jObj_Vacu_Release_IOType);
                                                var jObj_Vacu_Release_IOOveride = new JObject();
                                                jObj_Vacu_Release_IOOveride.Add("Value", IOOveride_Val);
                                                jObj_Vacu_Release.Add("IOOveride", jObj_Vacu_Release_IOOveride);
                                                var jObj_Vacu_Release_ChannelIdx = new JObject();
                                                jObj_Vacu_Release_ChannelIdx.Add("Value", 1);
                                                jObj_Vacu_Release.Add("ChannelIndex", jObj_Vacu_Release_ChannelIdx);
                                                var jObj_Vacu_Release_PortIdx = new JObject();
                                                jObj_Vacu_Release_PortIdx.Add("Value", 3);
                                                jObj_Vacu_Release.Add("PortIndex", jObj_Vacu_Release_PortIdx);
                                                var jObj_Vacu_Release_Reverse = new JObject();
                                                jObj_Vacu_Release_Reverse.Add("Value", false);
                                                jObj_Vacu_Release.Add("Reverse", jObj_Vacu_Release_Reverse);
                                                var jObj_Vacu_Release_KeyVal = new JObject();
                                                jObj_Vacu_Release_KeyVal.Add("Value", "DOPOGOCARD_VACU_RELEASE_SUB");
                                                jObj_Vacu_Release.Add("Key", jObj_Vacu_Release_KeyVal);
                                                jObj_Vacu_Release.Add("Description", jObj_Vacu_Release_KeyVal);
                                                jObj_Outputs.Add("DOPOGOCARD_VACU_RELEASE_SUB", jObj_Vacu_Release);
                                                isFileChanged = true;

                                                WriteDebugLog($"[File Changed] Created json object DOPOGOCARD_VACU_RELEASE_SUB - {viFile}");
                                            }
                                            else
                                            {
                                                var orgValue = jObj_Outputs["DOPOGOCARD_VACU_RELEASE_SUB"]["ChannelIndex"]["Value"]?.ToString();
                                                if (!orgValue.Equals("1"))
                                                {
                                                    jObj_Outputs["DOPOGOCARD_VACU_RELEASE_SUB"]["ChannelIndex"]["Value"] = 1;
                                                    WriteDebugLog($"[DOPOGOCARD_VACU_RELEASE_SUB] ChannelIndex - orgVal : {orgValue}, newVal : {jObj_Outputs["DOPOGOCARD_VACU_RELEASE_SUB"]["ChannelIndex"]["Value"]}");
                                                    isFileChanged = true;
                                                }
                                            }
                                            #endregion

                                            #region DOPOGOCARD_VACU_SUB
                                            if (!jObj_Outputs.ContainsKey("DOPOGOCARD_VACU_SUB"))
                                            {
                                                var jObj_Vacu = new JObject();
                                                var jObj_Vacu_TimeVal = new JObject();
                                                jObj_Vacu_TimeVal.Add("Value", 0);
                                                jObj_Vacu.Add("TimeOut", jObj_Vacu_TimeVal);
                                                jObj_Vacu.Add("MaintainTime", jObj_Vacu_TimeVal);
                                                var jObj_Vacu_IOType = new JObject();
                                                jObj_Vacu_IOType.Add("Value", "OUTPUT");
                                                jObj_Vacu.Add("IOType", jObj_Vacu_IOType);
                                                var jObj_Vacu_IOOveride = new JObject();
                                                jObj_Vacu_IOOveride.Add("Value", IOOveride_Val);
                                                jObj_Vacu.Add("IOOveride", jObj_Vacu_IOOveride);
                                                var jObj_Vacu_ChannelIdx = new JObject();
                                                jObj_Vacu_ChannelIdx.Add("Value", 1);
                                                jObj_Vacu.Add("ChannelIndex", jObj_Vacu_ChannelIdx);
                                                var jObj_Vacu_PortIdx = new JObject();
                                                jObj_Vacu_PortIdx.Add("Value", 2);
                                                jObj_Vacu.Add("PortIndex", jObj_Vacu_PortIdx);
                                                var jObj_Vacu_Reverse = new JObject();
                                                jObj_Vacu_Reverse.Add("Value", false);
                                                jObj_Vacu.Add("Reverse", jObj_Vacu_Reverse);
                                                var jObj_Vacu_KeyVal = new JObject();
                                                jObj_Vacu_KeyVal.Add("Value", "DOPOGOCARD_VACU_SUB");
                                                jObj_Vacu.Add("Key", jObj_Vacu_KeyVal);
                                                jObj_Vacu.Add("Description", jObj_Vacu_KeyVal);
                                                jObj_Outputs.Add("DOPOGOCARD_VACU_SUB", jObj_Vacu);
                                                isFileChanged = true;

                                                WriteDebugLog($"[File Changed] Created json object DOPOGOCARD_VACU_SUB - {viFile}");
                                            }
                                            else
                                            {
                                                var orgValue = jObj_Outputs["DOPOGOCARD_VACU_SUB"]["ChannelIndex"]["Value"]?.ToString();
                                                if (!orgValue.Equals("1"))
                                                {
                                                    jObj_Outputs["DOPOGOCARD_VACU_SUB"]["ChannelIndex"]["Value"] = 1;
                                                    WriteDebugLog($"[DOPOGOCARD_VACU_SUB] ChannelIndex - orgVal : {orgValue}, newVal : {jObj_Outputs["DOPOGOCARD_VACU_SUB"]["ChannelIndex"]["Value"]}");
                                                    isFileChanged = true;
                                                }
                                            }
                                            #endregion

                                            #region IOOveride value를 NLO -> EMUL로 변경
                                            if (jObj_Inputs.ContainsKey("DITH_LOCK"))
                                            {
                                                var orgValue = jObj_Inputs["DITH_LOCK"]["IOOveride"]["Value"]?.ToString();
                                                if (orgValue.Equals("NLO"))
                                                {
                                                    jObj_Inputs["DITH_LOCK"]["IOOveride"]["Value"] = "EMUL";
                                                    WriteDebugLog($"[DITH_LOCK] IOOveride Value - org: {orgValue}, new: {jObj_Inputs["DITH_LOCK"]["IOOveride"]["Value"]}");
                                                    isFileChanged = true;
                                                }
                                            }
                                            else
                                            {
                                                WriteDebugLog($"[File Changed] Key does not exist. - Key : DITH_LOCK, File : {viFile}");
                                            }

                                            if (jObj_Inputs.ContainsKey("DITH_MBLOCK"))
                                            {
                                                var orgValue = jObj_Inputs["DITH_MBLOCK"]["IOOveride"]["Value"]?.ToString();
                                                if (orgValue.Equals("NLO"))
                                                {
                                                    jObj_Inputs["DITH_MBLOCK"]["IOOveride"]["Value"] = "EMUL";
                                                    WriteDebugLog($"[DITH_MBLOCK] IOOveride Value - org: {orgValue}, new: {jObj_Inputs["DITH_MBLOCK"]["IOOveride"]["Value"]}");
                                                    isFileChanged = true;
                                                }
                                            }
                                            else
                                            {
                                                WriteDebugLog($"[File Changed] Key does not exist. - Key : DITH_MBLOCK, File : {viFile}");
                                            }

                                            if (jObj_Inputs.ContainsKey("DITH_PBLOCK"))
                                            {
                                                var orgValue = jObj_Inputs["DITH_PBLOCK"]["IOOveride"]["Value"]?.ToString();
                                                if (orgValue.Equals("NLO"))
                                                {
                                                    jObj_Inputs["DITH_PBLOCK"]["IOOveride"]["Value"] = "EMUL";
                                                    WriteDebugLog($"[DITH_PBLOCK] IOOveride Value - org: {orgValue}, new: {jObj_Inputs["DITH_PBLOCK"]["IOOveride"]["Value"]}");
                                                    isFileChanged = true;
                                                }
                                            }
                                            else
                                            {
                                                WriteDebugLog($"[File Changed] Key does not exist. - Key : DITH_PBLOCK, File : {viFile}");
                                            }

                                            if (jObj_Inputs.ContainsKey("DICLP_LOCK"))
                                            {
                                                var orgValue = jObj_Inputs["DICLP_LOCK"]["IOOveride"]["Value"]?.ToString();
                                                if (orgValue.Equals("NLO"))
                                                {
                                                    jObj_Inputs["DICLP_LOCK"]["IOOveride"]["Value"] = "EMUL";
                                                    WriteDebugLog($"[DICLP_LOCK] IOOveride Value - org: {orgValue}, new: {jObj_Inputs["DICLP_LOCK"]["IOOveride"]["Value"]}");
                                                    isFileChanged = true;
                                                }
                                            }
                                            else
                                            {
                                                WriteDebugLog($"[File Changed] Key does not exist. - Key : DICLP_LOCK, File : {viFile}");
                                            }

                                            if (jObj_Inputs.ContainsKey("DINO_CLAMP"))
                                            {
                                                var orgValue = jObj_Inputs["DINO_CLAMP"]["IOOveride"]["Value"]?.ToString();
                                                if (orgValue.Equals("NLO"))
                                                {
                                                    jObj_Inputs["DINO_CLAMP"]["IOOveride"]["Value"] = "EMUL";
                                                    WriteDebugLog($"[DINO_CLAMP] IOOveride Value - org: {orgValue}, new: {jObj_Inputs["DINO_CLAMP"]["IOOveride"]["Value"]}");
                                                    isFileChanged = true;
                                                }
                                            }
                                            else
                                            {
                                                WriteDebugLog($"[File Changed] Key does not exist. - Key : DINO_CLAMP, File : {viFile}");
                                            }

                                            #endregion

                                            if (isFileChanged)
                                            {
                                                // 변경한 값을 저장한다.
                                                File.WriteAllText(viFile, jObject.ToString());
                                                WriteDebugLog($"[File Changed] Target - {viFile}");
                                            }
                                        }
                                    }
                                }
                                else if (viFileName.Equals("LoaderSystem.json"))
                                {
                                    if (defaultDir.Contains("LoaderSystem"))
                                    {
                                        var viFileStr = File.ReadAllText(viFile);
                                        JObject jObject = JObject.Parse(viFileStr);

                                        #region CardARMModules 안에 DOAIROFF 추가
                                        if (jObject["CardARMModules"] is JArray jArray)
                                        {
                                            foreach (JObject obj in jArray)
                                            {
                                                if (!obj.ContainsKey("DOAIROFF"))
                                                {
                                                    var jObj_CarArmBreak_Val = new JObject();
                                                    jObj_CarArmBreak_Val.Add("Value", "DOCCArmVac_Break");
                                                    obj.Add("DOAIROFF", jObj_CarArmBreak_Val);
                                                }
                                                else
                                                {
                                                    var orgValue = obj["DOAIROFF"]["Value"]?.ToString();
                                                    if (!orgValue.Equals("DOCCArmVac_Break"))
                                                    {
                                                        obj["DOAIROFF"]["Value"] = "DOCCArmVac_Break";
                                                        WriteDebugLog($"[DOAIROFF] Value - org: {orgValue}, new: {obj["DOAIROFF"]["Value"]}");
                                                        isFileChanged = true;
                                                    }
                                                }
                                            }

                                            isFileChanged = true;
                                        }
                                        #endregion

                                        #region CognexOCRModules 안에 SubchuckMotionParams 추가
                                        if (jObject["CognexOCRModules"] is JArray OCRArray)
                                        {
                                            foreach (JObject obj in OCRArray)
                                            {
                                                // Check if "SubchuckMotionParams" exists and has values
                                                if (!obj.TryGetValue("SubchuckMotionParams", out JToken subchuckMotionParamsToken) || subchuckMotionParamsToken == null || !subchuckMotionParamsToken.HasValues)
                                                {
                                                    // "SubchuckMotionParams" does not exist or is empty, so fill it
                                                    JArray subchuckMotionParams = new JArray();

                                                    JObject subchuckMotionParam1 = new JObject();
                                                    subchuckMotionParam1["SubstrateType"] = new JObject(new JProperty("Value", "Wafer"));
                                                    subchuckMotionParam1["SubstrateSize"] = new JObject(new JProperty("Value", "INCH6"));
                                                    subchuckMotionParam1["SubchuckXCoord"] = new JObject(new JProperty("Value", 0.0));
                                                    subchuckMotionParam1["SubchuckYCoord"] = new JObject(new JProperty("Value", 30000.0));
                                                    subchuckMotionParam1["SubchuckAngle_Offset"] = new JObject(new JProperty("Value", 0.0));
                                                    subchuckMotionParams.Add(subchuckMotionParam1);

                                                    JObject subchuckMotionParam2 = new JObject();
                                                    subchuckMotionParam2["SubstrateType"] = new JObject(new JProperty("Value", "Wafer"));
                                                    subchuckMotionParam2["SubstrateSize"] = new JObject(new JProperty("Value", "INCH8"));
                                                    subchuckMotionParam2["SubchuckXCoord"] = new JObject(new JProperty("Value", 0.0));
                                                    subchuckMotionParam2["SubchuckYCoord"] = new JObject(new JProperty("Value", -20000.0));
                                                    subchuckMotionParam2["SubchuckAngle_Offset"] = new JObject(new JProperty("Value", 0.0));
                                                    subchuckMotionParams.Add(subchuckMotionParam2);

                                                    JObject subchuckMotionParam3 = new JObject();
                                                    subchuckMotionParam3["SubstrateType"] = new JObject(new JProperty("Value", "Wafer"));
                                                    subchuckMotionParam3["SubstrateSize"] = new JObject(new JProperty("Value", "INCH12"));
                                                    subchuckMotionParam3["SubchuckXCoord"] = new JObject(new JProperty("Value", 0.0));
                                                    subchuckMotionParam3["SubchuckYCoord"] = new JObject(new JProperty("Value", 0.0));
                                                    subchuckMotionParam3["SubchuckAngle_Offset"] = new JObject(new JProperty("Value", 0.0));
                                                    subchuckMotionParams.Add(subchuckMotionParam3);

                                                    obj["SubchuckMotionParams"] = subchuckMotionParams;
                                                    isFileChanged = true;
                                                }
                                            }
                                        }

                                        #endregion

                                        if (isFileChanged)
                                        {
                                            // 변경한 값을 저장한다.
                                            File.WriteAllText(viFile, jObject.ToString());
                                            WriteDebugLog($"[File Changed] Target - {viFile}");
                                        }
                                    }
                                    else
                                    {
                                        //Cell에서 작업 필요하면 추가
                                        //NONE.
                                    }
                                }
                                else if (viFileName.Equals("E84SysParam.json"))
                                {
                                    var viFileStr = File.ReadAllText(viFile);
                                    JObject jObject = JObject.Parse(viFileStr);

                                    #region E84Moduls안에 TD0, TD1 값을 변경한다.
                                    double td0Val = 0.0;
                                    int td1Val = 0;

                                    if (jObject["E84Moduls"] is JArray jArray)
                                    {
                                        foreach (JObject obj in jArray)
                                        {
                                            if (obj.ContainsKey("TD0"))
                                            {
                                                JObject td0 = (JObject)obj["TD0"];
                                                var orgValue = td0["Value"];
                                                if (!orgValue.Equals(td0Val))
                                                {
                                                    td0["Value"] = td0Val;
                                                    WriteDebugLog($"[E84Moduls] TD0 Value - org: {orgValue}, new: {td0["Value"]}");
                                                    isFileChanged = true;
                                                }
                                            }

                                            if (obj.ContainsKey("TD1"))
                                            {
                                                JObject td1 = (JObject)obj["TD1"];
                                                var orgValue = td1["Value"];
                                                if (!orgValue.Equals(td1Val))
                                                {
                                                    td1["Value"] = td1Val;
                                                    WriteDebugLog($"[E84Moduls] TD0 Value - org: {orgValue}, new: {td1["Value"]}");
                                                    isFileChanged = true;
                                                }
                                            }
                                        }
                                    }
                                    #endregion

                                    if (isFileChanged)
                                    {
                                        // 변경한 값을 저장한다.
                                        File.WriteAllText(viFile, jObject.ToString());
                                        WriteDebugLog($"[File Changed] Target - {viFile}");
                                    }

                                }
                                else if (viFileName.Equals("CardChangeParams.Json"))
                                {
                                    var viFileStr = File.ReadAllText(viFile);
                                    JObject jObject = JObject.Parse(viFileStr);

                                    if (jObject.ContainsKey("ProbeCardID"))
                                    {
                                        var orgProbeCardID = jObject["ProbeCardID"]["Value"]?.ToString();
                                        if (orgProbeCardID.ToUpper() == "HOLDER")
                                        {
                                            jObject["ProbeCardID"]["Value"] = "";
                                        }
                                        WriteDebugLog($"ProbeCardID - orgVal : {orgProbeCardID}, newVal : {jObject["ProbeCardID"]["Value"]}");
                                        isFileChanged = true;
                                    }

                                    if (isFileChanged)
                                    {
                                        // 변경한 값을 저장한다.
                                        File.WriteAllText(viFile, jObject.ToString());
                                        WriteDebugLog($"[File Changed] Target - {viFile}");
                                    }
                                }
                                else if (viFileName.Equals("FoupIOMapParam.json"))
                                {
                                    if (defaultDir.Contains("LoaderSystem"))
                                    {
                                        XmlDocument xDoc = new XmlDocument();
                                        xDoc.Load(@"C:\ProberSystem\SystemInfo\SystemMode.json");
                                        XmlNamespaceManager xNameSpaceManager = new XmlNamespaceManager(xDoc.NameTable);
                                        xNameSpaceManager.AddNamespace("sysMode", "clr-namespace:ProberInterfaces;assembly=ProberInterfaces");
                                        var SystemType = xDoc.SelectSingleNode("sysMode:SystemMode/sysMode:SystemType", xNameSpaceManager)?.InnerText;

                                        var viFileStr = File.ReadAllText(viFile);
                                        JObject jObject = JObject.Parse(viFileStr);

                                        var jObj_Inputs = (JObject)jObject.SelectToken("Inputs");


                                        if (SystemType.Equals("DRAX"))
                                        {
                                            if (jObj_Inputs.ContainsKey("DI_COVER_LOCKs") && jObj_Inputs.ContainsKey("DI_COVER_UNLOCKs"))
                                            {
                                                //Loader
                                                if (jObj_Inputs["DI_COVER_LOCKs"] is JArray jArray)
                                                {
                                                    for (int i = 0; i < jArray.Count; i++)
                                                    {
                                                        (jArray[i] as JObject)["PortIndex"]["Value"] = 24;
                                                        (jArray[i] as JObject)["ChannelIndex"]["Value"] = 5 + i;
                                                    }
                                                    isFileChanged = true;
                                                }
                                                if (jObj_Inputs["DI_COVER_UNLOCKs"] is JArray jArray2)
                                                {
                                                    for (int i = 0; i < jArray2.Count; i++)
                                                    {
                                                        (jArray2[i] as JObject)["PortIndex"]["Value"] = 25;
                                                        (jArray2[i] as JObject)["ChannelIndex"]["Value"] = 5 + i;
                                                    }
                                                    isFileChanged = true;
                                                }
                                                     
                                                if (isFileChanged)
                                                {
                                                    // 변경한 값을 저장한다.
                                                    File.WriteAllText(viFile, jObject.ToString());
                                                    WriteDebugLog($"[File Changed] Target - {viFile}");
                                                }
                                            }
                                            else
                                            {
                                                WriteDebugLog($"[File Changed Fail] Target - {viFile}");
                                            }
                                        }
                                        else
                                        {
                                            //NONE
                                        }
                                    }
                                }
                                else if (viFileName.Equals("DewpointSysParameter.json"))
                                {
                                    #region DewpointSysParameter
                                    var viFileStr = File.ReadAllText(viFile);
                                    JObject jObject = JObject.Parse(viFileStr);

                                    if (jObject.ContainsKey("MinAvaDewPoint"))
                                    {
                                        // MarkDiffTolerance_X Default 값을 100으로 변경한다.
                                        var orgMarkDiffToleranceXVal = jObject["MinAvaDewPoint"]["Value"]?.ToString();
                                        jObject["MinAvaDewPoint"]["Value"] = -100.0;
                                        WriteDebugLog($"MinAvaDewPoint - orgVal : {orgMarkDiffToleranceXVal}, newVal : {jObject["MinAvaDewPoint"]["Value"]}");
                                        isFileChanged = true;
                                    }
                                    else
                                    {
                                        WriteDebugLog($"[File Changed] not exist Property MarkDiffTolerance_X Target - {viFile}");
                                    }

                                    if (isFileChanged)
                                    {
                                        // 변경한 값을 저장한다.
                                        File.WriteAllText(viFile, jObject.ToString());
                                        WriteDebugLog($"[File Changed] Target - {viFile}");
                                    }
                                    #endregion
                                }
                                else
                                {
                                    //None.
                                }
                            }
                            else
                            {
                                WriteDebugLog($"[File Change] not exists - {viFile}");
                                continue;
                            }
                        }
                    }
                }

                // C:\ProberSystem\UpdateFiles 폴더를 백업 폴더로 옮긴다.
                if (!Directory.Exists(bakDir))
                {
                    Directory.CreateDirectory(bakDir);
                }
                var updateFilesBakDir = Path.Combine(bakDir, "UpdateFiles");
                Directory.Move(updateFilesDir, updateFilesBakDir);

                WriteDebugLog($"[END] Update Files");
            }
            catch (Exception err)
            {
                WriteExceptionLog(err);
            }
        }

        public static void WriteExceptionLog(Exception err, string msg = null)
        {
            var logDir = @"C:\Logs\PreChecker\Exception\";
            var logFileDir = logDir + $"PreChecker_{DateTime.Today.ToString("yyyy-MM-dd")}.log";
            var logStr = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} | MSG : {err.Message}, Target : {err.TargetSite}, Source : {err.Source}, {Environment.NewLine} {err.StackTrace}{msg}";

            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            using (StreamWriter sw = new StreamWriter(logFileDir, append: true))
            {
                sw.WriteLine(logStr);
            }
        }

        public static void WriteDebugLog(string msg = null)
        {
            var logDir = @"C:\Logs\PreChecker\";
            var logFileDir = logDir + $"PreChecker_{DateTime.Today.ToString("yyyy-MM-dd")}.log";
            var logStr = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} | {msg}";

            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            using (StreamWriter sw = new StreamWriter(logFileDir, append: true))
            {
                sw.WriteLine(logStr);
            }
        }
    }
}