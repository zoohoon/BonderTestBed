using System;
using System.Collections.Generic;

//==> Layout
//https://github.com/NLog/NLog/wiki/Stack-Trace-Layout-Renderer
//https://github.com/NLog/NLog/wiki/Layout-Renderers
//https://nlog-project.org/config/?tab=layout-renderers

namespace LogModule
{
    using LogModule.LoggerController;
    using LogModule.LoggerParam;
    using LogModule.LoggerRule;
    using NLog;
    using NLog.Targets;
    using ProberErrorCode;
    using SerializerUtil;
    using System.Collections.Concurrent;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using System.Management;

    public delegate void SendMessageToLoaderDelegate(string Message);

    public delegate void SendLotLogToLoaderDelegate(string Message, int idx, ModuleLogType ModuleType, StateLogType State);

    public delegate void SendParamLogToLoaderDelegate(string Message, int idx);

    public class ModuleStateInfo
    {
        public string Name { get; set; }
        public string State { get; set; }

        public ModuleStateInfo(string name, string state)
        {
            this.Name = name;
            this.State = state;
        }
    }

    #region loader throughput object

    public class LoaderMapHolder
    {
        public LoaderMapHolder(string name, string key, string origin, DateTime createTime, string participant = "")
        {
            HolderName = name;
            StartTime = createTime;
            HolderKey = key;
            Origin = origin;

            if (string.IsNullOrEmpty(participant))
            {
                Participant = HolderName;
            }
            else
            {
                Participant = participant;
            }
        }
        public bool ErrorSequence = false; //seq diagram상 inend가 없거나, 마지막 holder가 아니면서 outend가 없는 경우(manul로 뺌), sub sequece 수행시 exception 등
        public bool IsFinalHolder = false;
        public bool IsSubSequence = false;
        public bool IsManual = false;
        public DateTime StartTime { get; set; } = default; //이 holder로 이동하는 map이 만들어진 시간 or subsequence start 시간        
        public string HolderName = string.Empty; //Holder Name (slot1 or arm1 or chuck1 ...)
        public string Origin = string.Empty;
        public string HolderKey = string.Empty; //holder key(a to b)
        public string HolderKeyForHtml = string.Empty; //a -) b
        public string Participant = string.Empty;
        public List<LoaderMapHolder> SubSequence = new List<LoaderMapHolder>();

        public bool duplicateCheck = false;
        public bool AbnormalSequence = false; //lot 수행중 계획된 wafer가 아닌 경우
        public bool IsError = false; //holder state에서 에러가 났는지 여부
        public string ErrorMsg = string.Empty;
        public int recreateCnt = 0; //map slicing 에러로 인해 이전에 생성된 map을 지운 개수

        public int SubstratebasedCreateNo = 0; //한 wafer sequence 기준으로 이 map이 생성된 count
        public int SubstratebasedSequence = 0; //first hop 제외
        public int LoaderbasedCreateNo = 0; //loader lot 기준으로 이 map이 생성된 count
        public int LoaderbasedSequence = 0; //first hop 제외

        public string SubstrateKey = string.Empty; // holder에 있는 substrate key
        public string SubstrateLotID = "";
        public string SubstrateWaferID = "";
        public LoaderMapHolder preNode = null;

        public DateTime _WaferInStartTime = default;
        public DateTime WaferInStartTime
        {
            get
            {
                return _WaferInStartTime;
            }
            set
            {
                _WaferInStartTime = value;
                if (preNode != null)
                {
                    preNode.WaferOutStartTime = value;
                }
            }
        }

        public DateTime _WaferInEndTime = default;
        //이 holder로 실제 object로 들어온 시간
        public DateTime WaferInEndTime
        {
            get
            {
                return _WaferInEndTime;
            }
            set
            {
                _WaferInEndTime = value;
                if (value != default && WaferInStartTime != default)
                {
                    TimeSpan elapsedSpan = new TimeSpan(value.Ticks - WaferInStartTime.Ticks);
                    Induration = Math.Round(elapsedSpan.TotalSeconds, 2);
                }

                if (preNode != null)
                {
                    preNode.WaferOutEndTime = value;
                }

                if (EndTime == default) //PA 동작인 경우 EndTime이 wafer in 과정(subsequence) 중 별도로 설정된다.
                {
                    EndTime = value;
                }
            }
        }

        public DateTime _WaferOutStartTime = default;
        public DateTime WaferOutStartTime
        { 
            get
            {
                return _WaferOutStartTime;
            }
            set
            {
                _WaferOutStartTime = value;
                if(value != default)
                {
                    long inEnd = WaferInEndTime.Ticks;
                    if (EndTime != default) //PA
                    {
                        inEnd = EndTime.Ticks;
                    }
                    TimeSpan elapsedSpan = new TimeSpan(value.Ticks - inEnd);
                    InToOutduration = Math.Round(elapsedSpan.TotalSeconds, 2);

                    elapsedSpan = new TimeSpan(value.Ticks - StartTime.Ticks);
                    PathCreateToOutduration = Math.Round(elapsedSpan.TotalSeconds, 2);
                }
            }
        }
        //이 holder에서 실제 object가 나간 시간
        public DateTime _WaferOutEndTime = default;
        public DateTime WaferOutEndTime
        {
            get
            {
                return _WaferOutEndTime;
            }
            set
            {
                _WaferOutEndTime = value;
                if (value != default && WaferOutStartTime != default)
                {
                    TimeSpan elapsedSpan = new TimeSpan(value.Ticks - WaferOutStartTime.Ticks);
                    Outduration = Math.Round(elapsedSpan.TotalSeconds, 2);
                }
            }
        }

        public double PathCreateToOutduration = 0.0; //create map ~ wafer out start
        public double InToOutduration = 0.0;    //wafer in end ~ wafer out start
        public double Induration = 0.0; //wafer in start ~ wafer in end
        public double Outduration = 0.0;    //wafer out start ~ wafer out end
        public double RealInduration = 0.0; //wafer in start ~ real wafer in end(PA)

        public DateTime FakeEndTime = default;

        public DateTime _EndTime = default;
        public DateTime EndTime //subsequence 동작이 완료된 시간, subsequence가 아닌 경우 wafer in 동작이 완료된 시간
        {
            get
            {
                return _EndTime;
            }
            set
            {
                _EndTime = value;
                FakeEndTime = value;
                if (value != default && StartTime != default)
                {
                    TimeSpan elapsedSpan = new TimeSpan(value.Ticks - StartTime.Ticks);
                    Duration = Math.Round(elapsedSpan.TotalSeconds, 2);
                }
                if (value != default && WaferInStartTime != default)
                {
                    TimeSpan elapsedSpan = new TimeSpan(value.Ticks - WaferInStartTime.Ticks);
                    RealInduration = Math.Round(elapsedSpan.TotalSeconds, 2);
                }
            }
        }
        public double Duration { get; set; } = 0.0; //start ~ end tick
    }

    public class LoaderThroughputInfo
    {
        public LoaderThroughputInfo(DateTime loaderLotStartTime, string key)
        {
            LoaderLotStartTime = loaderLotStartTime;
            LoaderLotKey = key;
        }
        public string LoaderLotKey = string.Empty;
        public DateTime LoaderLotStartTime { get; set; }
        public DateTime LoaderLotEndTime { get; set; }
        //lot start ~ lot all end 까지 수행된 lot 수
        public int LotCount = 0;
        public int TotalCreateNo = 0;
        public int TotalSequence = 0;
        public int TotalTrashMapCount = 0;
        public int TotalAbnormalCount = 0;
        public int TotalErrorCount = 0;
        public int TotalSubSequence = 0;
        public string LotIDs = string.Empty;

        public ConcurrentDictionary<string, List<LoaderMapHolder>> CompleteMapHolders = new ConcurrentDictionary<string, List<LoaderMapHolder>>(); //최종 origin으로 돌아온 map 정보 저장소, substrate별 unique값이 key임
        public ConcurrentDictionary<string, List<LoaderMapHolder>> LoaderMapHolders = new ConcurrentDictionary<string, List<LoaderMapHolder>>(); //진행중인 loadermap 정보, origin 위치를 key값으로 함
        public ConcurrentDictionary<string, List<LoaderMapHolder>> StageSubSequences = new ConcurrentDictionary<string, List<LoaderMapHolder>>(); //cell 별 subsequence map

        public void Dispose()
        {
            try
            {
                foreach (var item in LoaderMapHolders)
                {
                    item.Value.Clear();
                }
                LoaderMapHolders.Clear();
                LoaderMapHolders = null;

                foreach (var item in CompleteMapHolders)
                {
                    item.Value.Clear();
                }
                CompleteMapHolders.Clear();
                CompleteMapHolders = null;

                foreach (var item in StageSubSequences)
                {
                    item.Value.Clear();
                }
                StageSubSequences.Clear();
                StageSubSequences = null;
            }
            catch (Exception err)
            {
                LoggerManager.LoaderMapException(err);
            }
        }

        private DateTime GetLatestTime(LoaderMapHolder holder)
        {
            DateTime time = default;
            try
            {
                if (holder != null)
                {
                    DateTime[] times = new DateTime[]
                    {
                        holder.WaferOutStartTime,
                        holder.WaferInEndTime,
                        holder.WaferInStartTime,
                        holder.StartTime
                    };
                    time = times.Max();

                    if (time == default)
                    {
                        time = DateTime.Now;
                    }
                    else
                    {
                        time.AddMilliseconds(1);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.LoaderMapException(err);
            }
            return time;
        }

        private List<List<T>> ChunkBy<T>(List<T> source, int chunkSize)
        {
            try
            {
                return source
                .Select((value, index) => new { Index = index, Value = value })
                .GroupBy(x => x.Index / chunkSize)
                .Select(g => g.Select(x => x.Value).ToList())
                .ToList();
            }
            catch (Exception err)
            {
                LoggerManager.LoaderMapException(err);
            }
            return null;
        }

        private void GetLoaderMapeSequenceList(ref List<LoaderMapHolder> tempList, ref List<LoaderMapHolder> temphtmlList, bool bSnapShot)
        {
            try
            {
                if (CompleteMapHolders.Count > 0 || LoaderMapHolders.Count > 0 || StageSubSequences.Count > 0)
                {
                    if (bSnapShot == false)
                    {
                        foreach (var key in CompleteMapHolders)
                        {
                            foreach (var holder in key.Value)
                            {
                                holder.ErrorSequence = false;
                                tempList.Add(holder);
                                if (holder.WaferInEndTime == default)
                                {
                                    // 비정상 map(실제로 이동을 수행하지 않은 map)의 end 시간을 map이 만들어진 시간으로 지정해 준다.
                                    holder.EndTime = GetLatestTime(holder);
                                    holder.ErrorSequence = true;
                                }
                                temphtmlList.Add(holder);
                                foreach (var seq in holder.SubSequence)
                                {
                                    seq.ErrorSequence = false;
                                    if (seq.EndTime == default)
                                    {
                                        seq.EndTime = seq.StartTime;
                                        seq.ErrorSequence = true;
                                    }
                                    temphtmlList.Add(seq);
                                }
                            }
                        }
                    }

                    foreach (var key in LoaderMapHolders)
                    {
                        foreach (var holder in key.Value)
                        {
                            tempList.Add(holder);
                            holder.ErrorSequence = false;
                            if (holder.WaferInEndTime == default)
                            {
                                // 비정상 map(실제로 이동을 수행하지 않은 map)의 end 시간을 map이 만들어진 시간으로 지정해 준다.
                                if (bSnapShot)
                                {
                                    holder.ErrorSequence = true; //snapshot 시점에서 inend가 없는 것을 비정상적으로 표시하도록 한다.
                                    holder.FakeEndTime = GetLatestTime(holder);
                                }
                                else
                                {
                                    holder.ErrorSequence = true;
                                    holder.EndTime = GetLatestTime(holder);
                                }
                            }
                            temphtmlList.Add(holder);
                            foreach (var seq in holder.SubSequence)
                            {
                                seq.ErrorSequence = false;
                                if (seq.EndTime == default)
                                {
                                    seq.ErrorSequence = true;
                                    if (bSnapShot)
                                    {
                                        seq.FakeEndTime = seq.StartTime;
                                    }
                                    else
                                    {
                                        seq.EndTime = seq.StartTime;
                                    }
                                }
                                temphtmlList.Add(seq);
                            }
                        }
                    }

                    foreach (var key in StageSubSequences)
                    {
                        foreach (var seq in key.Value)
                        {
                            if (seq.EndTime == default)
                            {
                                if (bSnapShot)
                                {
                                    seq.FakeEndTime = seq.StartTime;
                                }
                                else
                                {
                                    seq.EndTime = seq.StartTime;
                                }
                            }
                            temphtmlList.Add(seq);
                        }
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.LoaderMapException(err);
            }
        }

        private void SaverLoaderThroughputDetail(List<KeyValuePair<string, List<LoaderMapHolder>>> sortedList, StreamWriter file, bool csv = true)
        {
            try
            {
                foreach (var key in sortedList)
                {
                    bool bAbnormal = false;
                    LoaderMapHolder map = key.Value.FirstOrDefault(x => (x.IsError == true || x.AbnormalSequence == true || x.recreateCnt > 0));
                    if (map != null)
                    {
                        bAbnormal = true;
                    }
                    double totalDuration = 0;
                    if (key.Value.Last().WaferInEndTime != default)
                    {
                        TimeSpan elapsedSpan = new TimeSpan(key.Value.Last().WaferInEndTime.Ticks - key.Value.First().StartTime.Ticks);
                        totalDuration = elapsedSpan.TotalSeconds;
                    }
                    string row = "";
                    if (csv)
                    {
                        row = $"{LoaderLotKey},{key.Value.First().SubstrateLotID},{key.Value.First().SubstrateKey},{key.Value.First().SubstrateWaferID},{key.Value.First().Origin},{bAbnormal},{key.Value.Count},{key.Value.First().StartTime:yyyy-MM-dd-HH:mm:ss}" +
                        $",{key.Value.Last().WaferInEndTime:yyyy-MM-dd-HH:mm:ss},{totalDuration},";
                        foreach (var holder in key.Value)
                        {
                            row += $"{holder.HolderName},{holder.LoaderbasedCreateNo},{holder.StartTime:yyyy-MM-dd-HH:mm:ss},{holder.WaferInEndTime:yyyy-MM-dd-HH:mm:ss}" +
                                $",{holder.WaferOutEndTime:yyyy-MM-dd-HH:mm:ss},{holder.PathCreateToOutduration},{holder.InToOutduration},{holder.Outduration},";
                        }
                    }
                    else
                    {
                        row = $"LotID:{key.Value.First().SubstrateLotID} Origin:{key.Value.First().Origin} WaferID:{key.Value.First().SubstrateWaferID} Totalseq:{key.Value.Count} Init:{key.Value.First().StartTime:yyyy-MM-dd-HH:mm:ss} " +
                        $"Complete:{key.Value.Last().WaferInEndTime:yyyy-MM-dd-HH:mm:ss} Duration:{totalDuration}";
                    }
                    file.WriteLine(row);
                }
                sortedList.Clear();
            }
            catch (Exception err)
            {
                LoggerManager.LoaderMapException(err);
            }
        }

        private void SaveLoaderThroughputInfo(string savePath)
        {
            try
            {
                if (string.IsNullOrEmpty(savePath) == false)
                {
                    using (StreamWriter file = new StreamWriter(savePath))
                    {
                        file.WriteLine($"Lot Count: {LotCount} ({LotIDs})");
                        file.WriteLine($"Loader Lot Start Time : {LoaderLotStartTime::yyyy-MM-dd-HH:mm:ss}");
                        file.WriteLine($"Loader Lot End Time : {LoaderLotEndTime::yyyy-MM-dd-HH:mm:ss}");
                        double totalDuration = 0;
                        if (LoaderLotEndTime != default)
                        {
                            TimeSpan elapsedSpan = new TimeSpan(LoaderLotEndTime.Ticks - LoaderLotStartTime.Ticks);
                            totalDuration = elapsedSpan.TotalSeconds;
                        }
                        file.WriteLine($"Loader Lot Duration : {totalDuration}");
                        file.WriteLine($"");

                        file.WriteLine($"// Total number of loader maps excluding origin holder (slot, tray)");
                        file.WriteLine($"Total_Sequence_Count : {TotalSequence}");
                        file.WriteLine($"");

                        file.WriteLine($"// Total number of SubSequence (PA Action, PIN/WAFER/Mark Align, Cleaning, Probing, Soaking)");
                        file.WriteLine($"Total_SubSequence_Count : {TotalSubSequence}");
                        file.WriteLine($"");

                        file.WriteLine($"// Total number of loader maps including origin holder (slot, tray)");
                        file.WriteLine($"Total_Holder_Count : {TotalCreateNo}");
                        file.WriteLine($"");

                        file.WriteLine($"// Number of loader maps (trash map) duplicated due to map slicing errors, etc.");
                        file.WriteLine($"// (Total_Sequence_Count - Total_TrashMap_Count) = Actual Sequence Count");
                        file.WriteLine($"Total_TrashMap_Count : {TotalTrashMapCount}");
                        file.WriteLine($"");

                        file.WriteLine($"// Number of loader maps created by abnormal situations. ");
                        file.WriteLine($"// ex) If the wafer is already in the holder before the lot, the origin is changed manually and moved to chuck or tray.");
                        file.WriteLine($"Abnormal_Sequence_Count : {TotalAbnormalCount}");
                        file.WriteLine($"");

                        file.WriteLine($"// Number of errors occurring in wafer transfer and each holder module");
                        file.WriteLine($"Total_Error_Count : {TotalErrorCount}");
                        file.WriteLine($"");

                        file.WriteLine($"// Detail Info");
                        var sortedList = CompleteMapHolders.ToList().OrderBy(kv => kv.Value.First().LoaderbasedCreateNo).ToList();
                        SaverLoaderThroughputDetail(sortedList, file, false);
                        sortedList = LoaderMapHolders.ToList().OrderBy(kv => kv.Value.First().LoaderbasedCreateNo).ToList();
                        SaverLoaderThroughputDetail(sortedList, file, false);
                        file.WriteLine($"");

                        LoggerManager.LoaderMapLog($"SaveLoaderThroughputInfo Created. path={savePath}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.LoaderMapException(err);
            }
        }

        private void SaveLoaderThroughputType1(string savePath)
        {
            try
            {
                if (string.IsNullOrEmpty(savePath) == false)
                {
                    using (StreamWriter file = new StreamWriter(savePath))
                    {
                        // value의 개수가 가장 많은 key 찾기
                        int headercnt = 0;
                        if (LoaderMapHolders.Count > 0)
                        {
                            headercnt = LoaderMapHolders.OrderByDescending(kv => kv.Value.Count).First().Value.Count;
                        }
                        if (CompleteMapHolders.Count > 0)
                        {
                            int tempcnt = CompleteMapHolders.OrderByDescending(kv => kv.Value.Count).First().Value.Count;
                            if (tempcnt > headercnt)
                            {
                                headercnt = tempcnt;
                            }
                        }
                        string header = "LoaderLotKey,LotID,SubstrateKey,WaferID,Origin,Abnormal,TotalSequence,InitTime,CompleteTime,TotalDuration,";
                        for (int i = 1; i <= headercnt; i++)
                        {
                            header += $"Holder{i},CreateNo,InitMap,WaferIn,WaferOut,MapToOut,InToOut,OutOperationTime,";
                        }
                        file.WriteLine(header);


                        var sortedList = CompleteMapHolders.ToList().OrderBy(kv => kv.Value.First().LoaderbasedCreateNo).ToList();
                        SaverLoaderThroughputDetail(sortedList, file);

                        //비정상 종료 상황을 고려해서 진행중 loadmap 정보가 있으면 추가 저장 한다.
                        sortedList = LoaderMapHolders.ToList().OrderBy(kv => kv.Value.First().LoaderbasedCreateNo).ToList();
                        SaverLoaderThroughputDetail(sortedList, file);

                        LoggerManager.LoaderMapLog($"SaveLoaderThroughpubCSV_Type1 Created. path={savePath}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.LoaderMapException(err);
            }
        }

        private void SaveLoaderThroughputType2(string savePath, List<LoaderMapHolder> orderedList)
        {
            try
            {
                if (string.IsNullOrEmpty(savePath) == false && orderedList?.Count > 0)
                {
                    using (StreamWriter file = new StreamWriter(savePath))
                    {
                        string header = $"LoaderLotKey,LotID,SubstrateKey,WaferID,HolderKey,Holder,Origin,CreateNo,InitMap,WaferInStart,WaferInEnd,InOperationTime,WaferOutStart,WaferOutEnd,OutOperationTime,MapToOut,InToOut,Abnormal,IsError,ReCreate";
                        file.WriteLine(header);
                        foreach (var holder in orderedList)
                        {
                            string row = $"{LoaderLotKey},{holder.SubstrateLotID},{holder.SubstrateKey},{holder.SubstrateWaferID},{holder.HolderKey},{holder.HolderName},{holder.Origin}" +
                                $",{holder.LoaderbasedCreateNo},{holder.StartTime:yyyy-MM-dd-HH:mm:ss},{holder.WaferInStartTime:yyyy-MM-dd-HH:mm:ss}" +
                                $",{holder.WaferInEndTime:yyyy-MM-dd-HH:mm:ss},{holder.Induration},{holder.WaferOutStartTime:yyyy-MM-dd-HH:mm:ss}" +
                                $",{holder.WaferOutEndTime:yyyy-MM-dd-HH:mm:ss},{holder.Outduration},{holder.PathCreateToOutduration},{holder.InToOutduration},{holder.AbnormalSequence},{holder.IsError},{holder.recreateCnt},";
                            file.WriteLine(row);
                        }
                        LoggerManager.LoaderMapLog($"SaveLoaderThroughpubCSV_Type2 Created. path={savePath}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.LoaderMapException(err);
            }
        }
        private void SaveLoaderThroughputType3(string path, List<LoaderMapHolder> orderedList)
        {
            try
            {
                if (string.IsNullOrEmpty(path) == false && orderedList?.Count > 0)
                {
                    Dictionary<string, int> customOrder = new Dictionary<string, int>
                    {
                        { "Cassette", 11 }, { "Cassette1", 12 }, { "Cassette2", 13 }, { "Cassette3", 14 }, {"Cassette4", 15 },
                        { "ARM1", 21 }, { "ARM2", 22 },
                        { "PA1", 31 }, { "PA2", 32 }, { "PA3", 33 }, { "PA4", 34 },
                        { "BUFFER1", 41 }, { "BUFFER2", 42 }, { "BUFFER3", 43 }, { "BUFFER4", 44 }, { "BUFFER5", 45 },
                        { "BUFFER6", 46 }, { "BUFFER7", 47 }, { "BUFFER8", 48 }, { "BUFFER9", 49 }, { "BUFFER10", 50 },
                        { "CHUCK1", 51 }, { "CHUCK2", 52 }, { "CHUCK3", 53 }, { "CHUCK4", 54 }, { "CHUCK5", 55 }, { "CHUCK6", 56 },
                        { "CHUCK7", 57 }, { "CHUCK8", 58 }, { "CHUCK9", 59 }, { "CHUCK10", 60 }, { "CHUCK11", 61 }, { "CHUCK12", 62 },
                        { "FIXEDTRAY1", 71 }, { "FIXEDTRAY2", 72 }, { "FIXEDTRAY3", 73 }, { "FIXEDTRAY4", 74 }, { "FIXEDTRAY5", 75 },
                        { "FIXEDTRAY6", 76 }, { "FIXEDTRAY7", 77 }, { "FIXEDTRAY8", 78 }, { "FIXEDTRAY9", 79 }, { "FIXEDTRAY10", 80 },
                        { "INSPECTIONTRAY1", 91 }, { "INSPECTIONTRAY2", 92 }, { "INSPECTIONTRAY3", 93 }, { "INSPECTIONTRAY4", 94 }
                    };

                    List<string> distinctValues = orderedList.Select(x => x.Participant).Distinct().ToList();
                    distinctValues.Sort((a, b) =>
                    {
                        int orderA = customOrder[a];
                        int orderB = customOrder[b];
                        return orderA.CompareTo(orderB);
                    });

                    List<List<LoaderMapHolder>> chunks = ChunkBy(orderedList, 500);
                    if (chunks == null)
                    {
                        LoggerManager.LoaderMapLog($"SaveLoaderThroughputType3 error LoaderMapHolder Chunk(500) list null.");
                        return;
                    }

                    int nListCount = chunks.Count;
                    int k = 0;
                    int i = 1;
                    foreach (var chunk in chunks)
                    {
                        string savePath = $"{path}({k}).html";
                        using (StreamWriter file = new StreamWriter(savePath))
                        {
                            string sCurPath = Environment.CurrentDirectory + @"\mermaid.js";
                            string Header = @"<!DOCTYPE html><html><head><script src=""" + sCurPath + @"""></script><script> mermaid.initialize({ startOnLoad: true})</script></head><body>";
                            file.WriteLine(Header);
                            file.WriteLine(@"<div id=""LoaderSequence"" class=""mermaid""> sequenceDiagram");

                            foreach (var item in distinctValues)
                            {
                                file.WriteLine($"participant {item}");
                            }

                            foreach (var holder in chunk)
                            {
                                if (string.IsNullOrEmpty(holder.HolderKeyForHtml) == false) //첫번째 holder를 제외하기 위함
                                {
                                    string sKey = holder.HolderKey.Replace($"-)", $" To ");
                                    string sKeyForHtml = holder.HolderKeyForHtml;
                                    string sNoteOverTarget = holder.HolderKeyForHtml.Replace("-)", ",");
                                    if (holder.ErrorSequence)
                                    {
                                        sKeyForHtml = holder.HolderKeyForHtml.Replace($"-)", $"--x");
                                    }
                                    else
                                    {
                                        //subsequence는 range 표시 안함, abnormalsequence는 end가 없을 수 있으므로 range 표시 안함 (sequence가 많은 경우 expand 표시 하지 않도록 함)
                                        if (nListCount == 1 && holder.IsSubSequence == false && (holder.IsManual || holder.AbnormalSequence == false))
                                        {
                                            if (holder.HolderName.Contains(("CHUCK")) || holder.HolderName.Contains(("PA")))
                                            {
                                                sKeyForHtml = holder.HolderKeyForHtml.Replace($"-)", $"-)+");
                                            }
                                            else if (holder.preNode.HolderName.Contains(("CHUCK")) || holder.preNode.HolderName.Contains(("PA")))
                                            {
                                                sKeyForHtml = holder.HolderKeyForHtml.Replace($"-)", $"-)-");
                                            }
                                        }
                                    }

                                    if (holder.IsSubSequence)
                                    {
                                        if (holder.HolderKey.Contains("_START"))
                                        {
                                            file.WriteLine($"{sKeyForHtml} : {holder.HolderKey} ({holder.StartTime:yyyy-MM-dd-HH:mm:ss})");
                                        }
                                        else
                                        {
                                            file.WriteLine($"{sKeyForHtml} : {holder.HolderKey} ({holder.Duration}s)");
                                        }
                                    }
                                    else
                                    {
                                        double induration = holder.Induration;
                                        if (holder.EndTime != default)
                                        {
                                            induration = holder.RealInduration;
                                        }
                                        file.WriteLine($"{sKeyForHtml} : [{i}] ({holder.preNode.InToOutduration}s) {sKey}, O:{holder.Origin} ({induration}s)");
                                        i++;
                                    }

                                    string msg = "";
                                    bool bAbnormal = (string.IsNullOrEmpty(holder.ErrorMsg) == false || holder.IsError || holder.AbnormalSequence || holder.recreateCnt > 0);
                                    if (string.IsNullOrEmpty(holder.ErrorMsg) == false)
                                    {
                                        msg += $"{holder.ErrorMsg}";
                                    }
                                    else
                                    {
                                        if (holder.IsError)
                                        {
                                            msg += "(holder state error)";
                                        }
                                        if (holder.AbnormalSequence)
                                        {
                                            msg += "(Abnormal sequence)";
                                        }
                                    }

                                    if (holder.recreateCnt > 0)
                                    {
                                        msg += $"(recreated map count={holder.recreateCnt})";
                                    }

                                    if (bAbnormal)
                                    {
                                        file.WriteLine($"Note over {sNoteOverTarget}: {msg}");
                                    }
                                }
                            }
                            file.WriteLine(@"</div></body></html>");
                            LoggerManager.LoaderMapLog($"SaveLoaderThroughpubCSV_Type3 Created. path={savePath}");
                        }
                        k++;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.LoaderMapException(err);
            }
        }

        private static object snapshotLock = new object();
        public void SaveLoaderSnapshot()
        {
            try
            {
                lock (snapshotLock)
                {
                    if (LoaderMapHolders.Count > 0 || StageSubSequences.Count > 0)
                    {
                        DateTime errorTime = DateTime.Now;
                        //date 폴더/loaderthroughput.csv, date 폴더/loaderthroughput.info 파일 생성 해야 함
                        string savePath = $"{LoggerManager.LoaderMapLoggerCtl.LoggerParam.LogDirPath}\\ErrorSnapshot";
                        DirectoryInfo di = new DirectoryInfo(savePath);
                        if (di.Exists == false)
                        {
                            di.Create();
                        }

                        //sequence로 ordering 고려(map 내의 전체리스트를 하나로 합친후 ordering 하면 됨)
                        List<LoaderMapHolder> tempList = new List<LoaderMapHolder>();
                        List<LoaderMapHolder> temphtmlList = new List<LoaderMapHolder>();
                        GetLoaderMapeSequenceList(ref tempList, ref temphtmlList, true);

                        List<LoaderMapHolder> orderedList = tempList.OrderBy(kv => kv.LoaderbasedCreateNo).ToList(); //loader map 생성 순서로 ordering
                        string saveCSV = $"{savePath}//{errorTime:yyyy-MM-dd-HH-mm-ss}_snapshot.csv";
                        SaveLoaderThroughputType2(saveCSV, orderedList);

                        string saveHTML = $"{savePath}//{errorTime:yyyy-MM-dd-HH-mm-ss}_snapshot_onlyloader";
                        SaveLoaderThroughputType3(saveHTML, orderedList);

                        orderedList = temphtmlList.OrderBy(kv => kv.FakeEndTime).ToList(); //cell까지 고려하여 action이 끝난 시점 기준으로 ordering, snapshot 시점에서는 정상상황이지만 아직 action이 끊나지 않은 경우가 있다.
                        saveHTML = $"{savePath}//{errorTime:yyyy-MM-dd-HH-mm-ss}_snapshot_withcell";
                        SaveLoaderThroughputType3(saveHTML, orderedList);

                        tempList.Clear();
                        temphtmlList.Clear();
                        orderedList.Clear();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.LoaderMapException(err);
            }
        }

        public void SaveLoaderThroughputCSV()
        {
            try
            {
                if (CompleteMapHolders.Count > 0 || LoaderMapHolders.Count > 0 || StageSubSequences.Count > 0)
                {
                    //date 폴더/loaderthroughput.csv, date 폴더/loaderthroughput.info 파일 생성 해야 함
                    string savePath = $"{LoggerManager.LoaderMapLoggerCtl.LoggerParam.LogDirPath}\\{LoaderLotEndTime:yyyy-MM-dd-HH-mm-ss}";
                    DirectoryInfo di = new DirectoryInfo(savePath);
                    if (di.Exists == false)
                    {
                        di.Create();
                    }

                    string saveThroughputType1Path = $"{savePath}//{LoaderLotEndTime:yyyy-MM-dd-HH-mm-ss}_throughput_type1.csv";
                    SaveLoaderThroughputType1(saveThroughputType1Path);

                    //sequence로 ordering 고려(map 내의 전체리스트를 하나로 합친후 ordering 하면 됨)
                    List<LoaderMapHolder> tempList = new List<LoaderMapHolder>();
                    List<LoaderMapHolder> temphtmlList = new List<LoaderMapHolder>();
                    GetLoaderMapeSequenceList(ref tempList, ref temphtmlList, false);

                    List<LoaderMapHolder> orderedList = tempList.OrderBy(kv => kv.LoaderbasedCreateNo).ToList(); //loader map 생성 순서로 ordering
                    string saveThroughputType2Path = $"{savePath}//{LoaderLotEndTime:yyyy-MM-dd-HH-mm-ss}_throughput_type2.csv";
                    SaveLoaderThroughputType2(saveThroughputType2Path, orderedList);

                    string saveThroughputType3Path = $"{savePath}//{LoaderLotEndTime:yyyy-MM-dd-HH-mm-ss}_throughput_type3_1";
                    SaveLoaderThroughputType3(saveThroughputType3Path, orderedList);

                    orderedList = temphtmlList.OrderBy(kv => kv.EndTime).ToList(); //cell까지 고려하여 action이 끝난 시점 기준으로 ordering
                    saveThroughputType3Path = $"{savePath}//{LoaderLotEndTime:yyyy-MM-dd-HH-mm-ss}_throughput_type3_2";
                    SaveLoaderThroughputType3(saveThroughputType3Path, orderedList);

                    TotalAbnormalCount = tempList.Where(kv => kv.AbnormalSequence == true && kv.HolderKey != kv.HolderName).ToList().Count;
                    TotalErrorCount = temphtmlList.Where(kv => kv.IsError == true && kv.HolderKey != kv.HolderName).ToList().Count; //에러는 subsequence까지 포함
                    SaveLoaderThroughputInfo($"{savePath}//{LoaderLotEndTime:yyyy-MM-dd-HH-mm-ss}_throughput_info.txt");

                    tempList.Clear();
                    temphtmlList.Clear();
                    orderedList.Clear();

                    if (LoaderLotEndTime != default)
                    {
                        TimeSpan elapsedSpan = new TimeSpan(DateTime.Now.Ticks - LoaderLotEndTime.Ticks);
                        LoggerManager.LoaderMapLog($"SaveLoaderThroughputCSV processing time={elapsedSpan.TotalSeconds}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.LoaderMapException(err);
            }
        }
    }

    public class LoaderMapFoupInfo
    {
        public LoaderMapFoupInfo(int slotCnt = 0)
        {
            SlotCount = slotCnt;
        }
        public int SlotCount = 0;
        public string LotID = string.Empty;
    }

    public class LoaderMapHolderEx
    {
        public LoaderMapHolderEx(LoaderMapHolder h = null)
        {
            holder = h;
            if (h != null)
            {
                AssignLotID = h.SubstrateLotID;
            }
        }
        public LoaderMapHolder holder = null;
        public string AssignLotID = "";
    }

    #endregion

    public static class LoggerManager
    {
        public static LoaderThroughputInfo ThroughputInfo = null;
        public static ConcurrentDictionary<string, LoaderMapHolderEx> ChucksWafer = null;
        public static ConcurrentDictionary<int, LoaderMapFoupInfo> FoupInfos = null;
        public static ConcurrentDictionary<string, List<LoaderMapHolder>> LoaderMapHolders = null; //진행중인 loadermap 정보, origin 위치를 key값으로 함

        public static bool isMapLog = false; //임시 코드 릴리즈 시에는 false로 바꿔 주어야 함(추후 파라미터화 고민)

        public static TraceSwitch GPTraceSwitch = new TraceSwitch("gpTraceSwitch", "Switch in config file");

        public static EventLogManager EventLogMg = new EventLogManager();

        public static string NotifiedPropertyName = "LogNotifed";
        public static string DescriptionPropertyName = "Description";
        public static string LogidentifierPropertyName = "LogIdentifier";
        public static string LogTypePropertyName = "LogType";
        public static string LogCodePropertyName = "LogCode";
        public static string LogTagPropertyName = "LogTag";

        private static string Alias_ProLog = "[PL]";
        private static string Alias_DebugLog = "[DI]";
        private static string Alias_DebugLogError = "[DE]";
        private static string Alias_DebugLogException = "[DX]";
        private static string Alias_EventLog = "[EV]";
        private static string Alias_GPIBLog = "[GP]";
        private static string Alias_TEMPLog = "[TE]";
        private static string Alias_TCPIPLog = "[TP]";
        private static string Alias_SoakingPLog = "[SK]";
        private static string Alias_CompVerifyLog = "[CV]";
        private static string Alias_RecoveryLog = "[R]";
        private static string Alias_MonitoringLog = "[MT]";
        private static string Alias_SmokeSensorLog = "[SM]";

        public static int LogMaximumCount = 50;
        private static int GPCellCount = 12;

        public static object ProLogBufferLockObject = new object();
        public static object EventLogBufferLockObject = new object();
        public static object CompVerifyLogBufferLockObject = new object();

        private static NLoggerControllerResourceFactory _NLogCtlResFac;
        private static NLoggerController ProLoggerCtl { get; set; }
        private static NLoggerController DebugLoggerCtl { get; set; }
        private static NLoggerController DebugDetailLoggerCtl { get; set; }
        private static NLoggerController GpibLoggerCtl { get; set; }
        private static NLoggerController EventLoggerCtl { get; set; }
        private static NLoggerController PinAlignLoggerCtl { get; set; }
        private static NLoggerController PMILoggerCtl { get; set; }
        private static NLoggerController TempLoggerCtl { get; set; }
        private static NLoggerController LOTLoggerCtl { get; set; }
        private static NLoggerController TCPIPLoggerCtl { get; set; }
        private static NLoggerController ParamLoggerCtl { get; set; }
        private static NLoggerController SoakingLoggerCtl { get; set; }
        private static NLoggerController MonitoringLoggerCtl { get; set; }
        private static NLoggerController SmokeSensorLoggerCtl { get; set; }

        private static NLoggerController CompVerifyLoggerCtl { get; set; }

        private static NLoggerController InfoLoggerCtl { get; set; }
        public static NLoggerController LoaderMapLoggerCtl { get; set; }

        private static List<NLoggerController> LOT_Cell_LoggerCtlList { get; set; }
        private static List<NLoggerController> Param_Cell_LoggerCtlList { get; set; }
        public static ImageLoggerController CognexLoggerCtl { get; set; }
        public static LoggerManagerParameter LoggerManagerParam { get; set; }
        public static ObservableCollection<LogEventInfo> ProLogBuffer { get; set; }
        public static ObservableCollection<LogEventInfo> EventLogBuffer { get; set; }
        public static ObservableCollection<string> CompVerifyLogBuffer { get; set; }
        public static ObservableCollection<string> RecoveryLogBuffer { get; set; }

        public static int PrologBufferIndex { get; set; }
        public static int EventLogBufferIndex { get; set; }
        public static string CurrentDebuglogPath { get; set; }

        public static SendMessageToLoaderDelegate SendMessageToLoaderDelegate { get; set; }
        public static SendLotLogToLoaderDelegate SendActionLogToLoaderDelegate { get; set; }
        public static SendParamLogToLoaderDelegate SendParamLogToLoaderDelegate { get; set; }

        public static LogTransfer logTransfer = new LogTransfer();

        private static Dictionary<string, string> stateStrMap;

        private static double? SV { get; set; }
        private static double? PV { get; set; }
        private static double? DP { get; set; }
        private static double? MV { get; set; }
        private static string tempStrCache = string.Empty;

        private static string LotState { get; set; } = string.Empty;

        private static string LoaderState { get; set; } = string.Empty;

        private static string StageMode { get; set; } = string.Empty;

        private static List<ModuleStateInfo> ModuleStates { get; set; }
        private static string ModuleState { get; set; }
        private static string LotName { get; set; } = string.Empty;
        private static string DeviceName { get; set; } = string.Empty;
        private static string WaferID { get; set; } = string.Empty;
        private static string ProbeCardID { get; set; } = string.Empty;
        public static MonitoringLogType SubAlias_MonitoringLog { get; set; }

        public static void print_memoryInfo()
        {
            try
            {
                // WMI 쿼리를 통해 시스템 메모리 정보를 가져옴
                ObjectQuery wql = new ObjectQuery("SELECT FreeVirtualMemory, FreePhysicalMemory," +
                    "TotalVisibleMemorySize, TotalVirtualMemorySize FROM Win32_OperatingSystem");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(wql);
                ManagementObjectCollection results = searcher.Get();

                string pm = "";
                string apm = "";
                string vm = "";
                string avm = "";
                foreach (ManagementObject result in results)
                {
                    // KB 단위로 반환됨
                    ulong totalVirtualMemoryMB = (ulong)result["TotalVirtualMemorySize"] / 1024;
                    ulong totalPhysicalMemoryMB = (ulong)result["TotalVisibleMemorySize"] / 1024;

                    ulong freeVirtualMemoryMB = (ulong)result["FreeVirtualMemory"] / 1024;
                    ulong freePhysicalMemoryMB = (ulong)result["FreePhysicalMemory"] / 1024;

                    pm = $"PM: {totalPhysicalMemoryMB} MB";
                    apm = $"APM: {freePhysicalMemoryMB} MB";
                    vm = $"VM: {totalVirtualMemoryMB} MB";
                    avm = $"AVM: {freeVirtualMemoryMB} MB";

                }

                string disk = "";
                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    if (drive.IsReady)
                    {
                        disk += $" {drive.Name}Free space: {drive.TotalFreeSpace / (1024 * 1024)} MB";
                    }
                }

                // 현재 프로세스의 커밋 사이즈 가져오기
                string cpm = "";
                string cvm = "";
                using (Process proc = Process.GetCurrentProcess())
                {
                    cpm = $"ProberSystem PM: {proc.PrivateMemorySize64 / (1024 * 1024)} MB";
                    cvm = $"ProberSystem VM: {proc.VirtualMemorySize64 / (1024 * 1024)} MB";
                }

                Debug($"{pm}, {apm}, {vm}, {avm},{disk}, {cpm}, {cvm}");
            }
            catch (Exception err)
            {
                Trace.WriteLine(err.Message);
            }
        }

        #region loader throughput static Func

        public static int CheckDuplicateMap(List<LoaderMapHolder> mapList, string origin, string curr, string target, string chkTarget, out int recreateCnt)
        {
            int ret = 0;
            recreateCnt = 0;
            try
            {
                if (mapList != null)
                {
                    LoaderMapHolder map = mapList.FirstOrDefault(x => (x.HolderKey == $"{curr}-){chkTarget}" && x.WaferOutStartTime == default && x.preNode.WaferInEndTime != default));
                    if (map != null) //sequence 이름이 같고 아직 이전 holder에는 wafer가 들어왔고 현 holder에 wafer가 나가지 않았는데 다시 map이 만들어진 경우
                    {
                        //mapslicing error 발생한 경우// 이하 List value는 지우고 error count를 증가킨후 다시 list에 넣는다.
                        int removeindex = mapList.IndexOf(map);
                        int removecnt = mapList.Count - removeindex;
                        if (removecnt > 0 && map.ErrorSequence == false)
                        {
                            ret = removecnt;
                            //AbnormalSequenceForHtml가 true라는건 wafer 이송중 에러가 발생해서 해당 map 부터 이후 map들이 수행되지 않은 것들이다. 이경우는 map slicing 에러로 볼수 없다.
                            //실제 이송중 에러가 발생한 map을 지울 경우 이후 lot end 등에서 history가 짤린 sequence가 표시되게 되므로 해당 map은 지우지 않도록 한다.
                            recreateCnt = map.recreateCnt + removecnt;
                            mapList.RemoveRange(removeindex, removecnt);
                            LoaderMapLog($"[AddLoaderMapHolder] already exist. so oldmap deleate({curr}-){chkTarget}) and re-create map curr:{curr} target:{target} origin:{origin}, remove count={removecnt}");
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoaderMapException(err);
            }
            return ret;
        }
        public static void SetSlotInfo(int foupNum, int slotCnt)
        {
            try
            {
                if (FoupInfos == null)
                {
                    LoaderMapLog($"[SetSlotInfo] FoupInfos is null");
                    return;
                }

                if (!FoupInfos.ContainsKey(foupNum))
                {
                    LoaderMapFoupInfo info = new LoaderMapFoupInfo(slotCnt);
                    FoupInfos.TryAdd(foupNum, info);
                    LoaderMapLog($"[SetSlotInfo] FoupNum={foupNum}, SlotCnt={slotCnt}");
                }
                else
                {
                    if (FoupInfos[foupNum].SlotCount == 0 || FoupInfos[foupNum].SlotCount != slotCnt)
                    {
                        FoupInfos[foupNum].SlotCount = slotCnt;
                        LoaderMapLog($"[SetSlotInfo] FoupNum={foupNum}, SlotCnt={slotCnt}");
                    }
                }
            }
            catch (Exception err)
            {
                LoaderMapException(err);
            }
        }
        public static void SetLotInfo(int foupNum, string lotid)
        {
            try
            {
                if (FoupInfos == null)
                {
                    LoaderMapLog($"[SetLotInfo] FoupInfos is null");
                    return;
                }

                if (foupNum > 0 && foupNum < 5)
                {
                    if (!FoupInfos.ContainsKey(foupNum))
                    {
                        LoaderMapFoupInfo info = new LoaderMapFoupInfo();
                        info.LotID = lotid;
                        FoupInfos.TryAdd(foupNum, info);
                    }
                    else
                    {
                        FoupInfos[foupNum].LotID = lotid;
                    }
                    LoaderMapLog($"[SetLotInfo] FoupNum={foupNum}, lotId={lotid}");

                    if (ThroughputInfo != null)
                    {
                        if (string.IsNullOrEmpty(ThroughputInfo.LotIDs) == false)
                        {
                            ThroughputInfo.LotIDs += $" ,";
                        }
                        ThroughputInfo.LotIDs += lotid;
                        ThroughputInfo.LotCount++;
                    }
                }
            }
            catch (Exception err)
            {
                LoaderMapException(err);
            }
        }
        public static string GetLotID(string origin)
        {
            string lotID = "";
            try
            {
                if (origin.Contains("SLOT"))
                {
                    if (FoupInfos == null)
                    {
                        LoaderMapLog($"[GetLotID] FoupInfos is null");
                        return lotID;
                    }

                    Regex regex = new Regex(@"\d+");
                    MatchCollection matches = regex.Matches(origin);
                    if (matches.Count == 1)
                    {
                        int matchnum;
                        int.TryParse(matches[0].ToString(), out matchnum);

                        int foup1num = (FoupInfos.ContainsKey(1)) ? FoupInfos[1].SlotCount : 0;
                        int foup2num = (FoupInfos.ContainsKey(2)) ? FoupInfos[2].SlotCount : 0;
                        int foup3num = (FoupInfos.ContainsKey(3)) ? FoupInfos[3].SlotCount : 0;
                        int foup4num = (FoupInfos.ContainsKey(4)) ? FoupInfos[4].SlotCount : 0;


                        if (matchnum > 0 && matchnum <= foup1num)
                        {
                            lotID = (FoupInfos.ContainsKey(1)) ? FoupInfos[1].LotID : lotID;
                        }
                        else if (matchnum > foup1num && matchnum <= foup1num + foup2num)
                        {
                            lotID = (FoupInfos.ContainsKey(2)) ? FoupInfos[2].LotID : lotID;
                        }
                        else if (matchnum > foup1num + foup2num && matchnum <= foup1num + foup2num + foup3num)
                        {
                            lotID = (FoupInfos.ContainsKey(3)) ? FoupInfos[3].LotID : lotID;
                        }
                        else if (matchnum > foup1num + foup2num + foup3num && matchnum <= foup1num + foup2num + foup3num + foup4num)
                        {
                            lotID = (FoupInfos.ContainsKey(4)) ? FoupInfos[4].LotID : lotID;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return lotID;
        }
        public static string GetParticipant(string holderName)
        {
            string sRet = holderName;

            try
            {
                if (holderName.Contains("SLOT"))
                {
                    if (FoupInfos == null)
                    {
                        LoaderMapLog($"[GetParticipant] FoupInfos is null");
                        return sRet;
                    }

                    Regex regex = new Regex(@"\d+");
                    MatchCollection matches = regex.Matches(holderName);
                    if (matches.Count == 1)
                    {
                        int matchnum;
                        int.TryParse(matches[0].ToString(), out matchnum);

                        int foup1num = (FoupInfos.ContainsKey(1)) ? FoupInfos[1].SlotCount : 0;
                        int foup2num = (FoupInfos.ContainsKey(2)) ? FoupInfos[2].SlotCount : 0;
                        int foup3num = (FoupInfos.ContainsKey(3)) ? FoupInfos[3].SlotCount : 0;
                        int foup4num = (FoupInfos.ContainsKey(4)) ? FoupInfos[4].SlotCount : 0;


                        if (matchnum > 0 && matchnum <= foup1num)
                        {
                            sRet = $"Cassette1";
                        }
                        else if (matchnum > foup1num && matchnum <= foup1num + foup2num)
                        {
                            sRet = $"Cassette2";
                        }
                        else if (matchnum > foup1num + foup2num && matchnum <= foup1num + foup2num + foup3num)
                        {
                            sRet = $"Cassette3";
                        }
                        else if (matchnum > foup1num + foup2num + foup3num && matchnum <= foup1num + foup2num + foup3num + foup4num)
                        {
                            sRet = $"Cassette4";
                        }
                        else
                        {
                            sRet = $"Cassette";
                        }
                    }
                    else
                    {
                        sRet = $"Cassette";
                    }
                }
            }
            catch (Exception)
            {
                sRet = $"Cassette";
            }
            return sRet;
        }
        private static bool ExtractLotId(string input, out string id)
        {
            try
            {
                string pattern = @"Lot ID:\s*(.*?),";
                Match match = Regex.Match(input, pattern);

                if (match.Success)
                {
                    id = match.Groups[1].Value.Trim();
                    return true;
                }
            }
            catch (Exception err)
            {
                LoaderMapException(err);
            }
            id = "";
            return false;
        }

        public static void InitCheckDuplicateMap()
        {
            try
            {
                ConcurrentDictionary<string, List<LoaderMapHolder>> substrateMaps = (isMapLog) ? ThroughputInfo?.LoaderMapHolders : LoaderMapHolders;
                if (substrateMaps != null)
                {
                    foreach (var key in substrateMaps.Keys)
                    {
                        LoaderMapHolder holder = substrateMaps[key].FirstOrDefault();
                        if (holder != null)
                        {
                            holder.duplicateCheck = true;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoaderMapException(err);
            }
        }

        private static int sequenceNum = 1;
        public static string GetSequenceTitle(LoaderMapHolder holder, string title, string origin)
        {
            string ret = string.Empty;
            try
            {
                if (holder != null)
                {
                    ret = $"{holder.HolderKey} ({DateTime.Now:yyyy-MM-dd-HH:mm:ss})";
                    string sKey = holder.HolderKey.Replace($"-)", $" To ");

                    if (holder.IsSubSequence)
                    {
                        if (holder.HolderKey.Contains("_START"))
                        {
                            ret = $"{holder.HolderKey} ({holder.StartTime:yyyy-MM-dd-HH:mm:ss})";
                        }
                        else
                        {
                            ret = $"{holder.HolderKey} ({holder.Duration}s)";
                        }
                    }
                    else
                    {
                        double induration = holder.Induration;
                        if (holder.EndTime != default)
                        {
                            induration = holder.RealInduration;
                        }
                        ret = $"[{sequenceNum}] ({holder.preNode.InToOutduration}s) {sKey}, O:{origin} ({induration}s)";
                        sequenceNum++;
                    }
                }
                else
                {
                    ret = $"{title} ({DateTime.Now:yyyy-MM-dd-HH:mm:ss})";
                }
            }
            catch (Exception err)
            {
                LoaderMapException(err);
            }
            return ret;
        }

        public static void AddLoaderMapHolder(string action, string curr, string target, string origin, bool bManual)
        {
            try
            {
                ConcurrentDictionary<string, List<LoaderMapHolder>> substrateMaps = (isMapLog) ? ThroughputInfo?.LoaderMapHolders : LoaderMapHolders;
                action = action.Replace("PA", "PREALIGN");
                action = action.Replace("CHUCK", "STAGE");
                action = action.Replace("FIXEDTRAY", "FIXED");
                action = action.Replace("INSPECTIONTRAY", "INSP");

                if (substrateMaps != null)
                {
                    DateTime curTime = DateTime.Now;
                    if (!substrateMaps.ContainsKey(origin))
                    {
                        substrateMaps.TryAdd(origin, new List<LoaderMapHolder>());

                        LoaderMapHolder firsthop = new LoaderMapHolder(curr, curr, origin, curTime, GetParticipant(curr));
                        firsthop.SubstrateKey = curTime.Ticks.GetHashCode().ToString();
                        firsthop.SubstrateLotID = GetLotID(origin);
                        if (ThroughputInfo != null)
                        {
                            firsthop.LoaderbasedCreateNo = ++ThroughputInfo.TotalCreateNo;
                        }
                        firsthop.SubstratebasedCreateNo = 1;
                        firsthop.WaferInStartTime = curTime;
                        firsthop.WaferInEndTime = curTime;
                        firsthop.Induration = 0;
                        substrateMaps[origin].Add(firsthop);

                        if (curr != origin)
                        {
                            //invalid case// 이미 holder에 wafer가 있는 상태로 lot가 시작된 경우
                            firsthop.AbnormalSequence = true;
                            LoaderMapLog($"[AddLoaderMapHolder] AbnormalSequence (not exist firsthoap) action{action} curr:{curr} target:{target} origin:{origin}");
                        }
                        else
                        {
                            //first holder
                        }

                        LoaderMapLog($"[AddLoaderMapHolder] Create Substrate Sequence origin:{origin}, HolderKey:{firsthop.HolderKey}, SubstrateKey:{firsthop.SubstrateKey}");
                    }

                    int recreateCnt = 0;
                    int removeCnt = 0;

                    if (substrateMaps[origin].FirstOrDefault().duplicateCheck)
                    {
                        //완료되지 않은 같은 map이 있는지 check
                        removeCnt = CheckDuplicateMap(substrateMaps[origin], origin, curr, target, target, out recreateCnt);
                        if (removeCnt == 0)
                        {
                            //두개 이상의 exchange 발생으로 인해  map slicing 에러가 발생했는지 여부 체크
                            string chkTartget = "ARM1";
                            if (target.Equals(chkTartget))
                            {
                                chkTartget = "ARM2";
                            }
                            removeCnt = CheckDuplicateMap(substrateMaps[origin], origin, curr, target, chkTartget, out recreateCnt);
                        }
                    }
                    substrateMaps[origin].FirstOrDefault().duplicateCheck = false;

                    LoaderMapHolder preHolder = substrateMaps[origin].FindLast(x => x.HolderName == curr && x.WaferOutStartTime == default); //curr holder를 찾는다. 단 wafer가 나간적이 없어야함
                    if (preHolder == null)
                    {
                        LoaderMapLog($"[AddLoaderMapHolder] pre-map value is not exist action:{action} curr:{curr} target:{target} origin:{origin}");
                    }
                    else
                    {
                        LoaderMapHolder holder = new LoaderMapHolder(target, $"{curr}-){target}", origin, curTime, GetParticipant(target));
                        if (ThroughputInfo != null)
                        {
                            ThroughputInfo.TotalTrashMapCount += removeCnt;
                            holder.LoaderbasedCreateNo = ++ThroughputInfo.TotalCreateNo;
                            holder.LoaderbasedSequence = ++ThroughputInfo.TotalSequence; //first hop들은 제외
                        }

                        holder.SubstratebasedCreateNo = substrateMaps.Count + 1;
                        holder.SubstratebasedSequence = substrateMaps.Count; //first hop은 제외 add하기전 count
                        holder.preNode = preHolder;
                        holder.recreateCnt = recreateCnt;
                        holder.AbnormalSequence = holder.preNode.AbnormalSequence;
                        if (bManual)
                        {
                            holder.IsManual = true;
                            holder.AbnormalSequence = true;
                            holder.ErrorMsg = $"(Manual Transfer)";
                        }
                        holder.SubstrateKey = holder.preNode.SubstrateKey;
                        holder.SubstrateLotID = holder.preNode.SubstrateLotID;
                        if (action.Equals(ModuleLogType.ARM_TO_STAGE.ToString()))
                        {
                            //PW인경우 LOT ID가 없을수 있다. 이경우 wafer load시 set된 cell의 lot id 정보를 할당해 준다.
                            if (string.IsNullOrEmpty(holder.SubstrateLotID) && ChucksWafer.ContainsKey(target) && ChucksWafer[target] != null)
                            {
                                //해당 wafer의 기존 loadermap의 lotid 값을 일괄적으로 update 해준다.
                                string assignLotID = ChucksWafer[target].AssignLotID;
                                holder.SubstrateLotID = assignLotID;
                                substrateMaps[origin].ForEach(x => x.SubstrateLotID = assignLotID);
                            }
                        }
                        holder.SubstrateWaferID = holder.preNode.SubstrateWaferID;
                        if (curr.Contains("SLOT"))
                        {
                            holder.HolderKeyForHtml = $"{holder.preNode.Participant}-){target}";
                        }
                        else if (target.Contains("SLOT"))
                        {
                            holder.HolderKeyForHtml = $"{curr}-){holder.Participant}";
                        }
                        else
                        {
                            holder.HolderKeyForHtml = holder.HolderKey;
                        }
                        substrateMaps[origin].Add(holder);

                        string desc = $"S:{holder.HolderKeyForHtml} | T:{holder.HolderKey} | O:{origin} | L:{holder.SubstrateLotID} | W:{holder.SubstrateWaferID} | K:{holder.SubstrateKey} | A:{holder.AbnormalSequence} | M:{holder.ErrorMsg} | isManual:{bManual}";
                        LoaderMapActionLog(desc, $"{action}", "CREATE", LoaderMapLogType.DF);
                    }
                }
                else
                {
                    if (FoupInfos == null) //lot중이 아님
                    {
                        string desc = $"S: | T:{curr}-){target} | O:{origin} | L: | W: | K: | A: | M: | isManual:{bManual}";
                        LoaderMapActionLog(desc, $"{action}", "CREATE", LoaderMapLogType.DN);
                    }
                    else
                    {
                        LoaderMapLog($"[AddLoaderMapHolder] substrateMaps is null, action:{action} curr:{curr} target:{target} origin:{origin}");
                    }
                }
            }
            catch (Exception err)
            {
                LoaderMapException(err);
            }
        }
        public static void UpdateLoaderMapHolder(ModuleLogType ModuleType, StateLogType state, string curr, string target, string origin, string old = "", string errMsg = "", string ocr = "")
        {
            try
            {
                bool bCheck = (state == StateLogType.START || state == StateLogType.DONE || state == StateLogType.ERROR);
                ConcurrentDictionary<string, List<LoaderMapHolder>> substrateMaps = null;
                if (bCheck)
                {
                    substrateMaps = (isMapLog) ? ThroughputInfo?.LoaderMapHolders : LoaderMapHolders;
                }

                if (substrateMaps != null)
                {
                    string findKey = (old != "") ? old : origin;
                    if (substrateMaps.ContainsKey(findKey))
                    {
                        DateTime curTime = DateTime.Now;

                        if (string.IsNullOrEmpty(ocr) == false)
                        {
                            //해당 wafer의 기존 loadermap의 ocr 값을 일괄적으로 update 해준다.
                            substrateMaps[findKey].ForEach(x => x.SubstrateWaferID = ocr);
                        }
                        LoaderMapHolder map = substrateMaps[findKey].LastOrDefault(x => (x.HolderKey == $"{curr}-){target}" && x.WaferInEndTime == default && x.IsError == false));
                        if (map == null)
                        {
                            LoaderMapLog($"[UpdateLoaderMapHolder] map value is not exist find retry {curr}-)TEMP state:{state} curr:{curr} target:{target} origin:{origin} oldorigin:{old}");
                            map = substrateMaps[findKey].LastOrDefault(x => (x.HolderKey == $"{curr}-)TEMP" && x.WaferInEndTime == default && x.IsError == false));
                            if (map == null)
                            {
                                LoaderMapLog($"[UpdateLoaderMapHolder] map value is not exist state:{state} curr:{curr} target:TEMP origin:{origin} oldorigin:{old}");
                                return;
                            }
                            else
                            {
                                map.HolderName = target;
                                map.Participant = GetParticipant(target);
                                map.HolderKey = $"{curr}-){target}";
                                map.HolderKeyForHtml = map.HolderKey;
                                int index = substrateMaps[findKey].FindIndex(x => x == map);
                                if (index < substrateMaps[findKey].Count - 1)
                                {
                                    LoaderMapHolder nextmap = substrateMaps[findKey][index + 1];
                                    nextmap.HolderKey = $"{target}-){nextmap.HolderName}";
                                    nextmap.HolderKeyForHtml = nextmap.HolderKey;
                                }
                            }
                        }

                        if (ModuleType.Equals(ModuleLogType.ARM_TO_STAGE))
                        {
                            //PW인경우 LOT ID가 없을수 있다. 이경우 wafer load시 set된 cell의 lot id 정보를 할당해 준다.
                            if (string.IsNullOrEmpty(map.SubstrateLotID) && ChucksWafer.ContainsKey(target) && ChucksWafer[target] != null)
                            {
                                //해당 wafer의 기존 loadermap의 lotid 값을 일괄적으로 update 해준다.
                                string assignLotID = ChucksWafer[target].AssignLotID;
                                substrateMaps[findKey].ForEach(x => x.SubstrateLotID = assignLotID);
                            }
                        }

                        if (state == StateLogType.START) //in, out start time update
                        {
                            map.WaferInStartTime = curTime;
                            string desc = $"S:{map.HolderKeyForHtml} | T:{map.HolderKey} | O:{findKey} | L:{map.SubstrateLotID} | W:{map.SubstrateWaferID} | K:{map.SubstrateKey} | A:{map.AbnormalSequence} | M:{map.ErrorMsg} | isManual:{map.IsManual}";
                            LoaderMapActionLog(desc, $"{ModuleType}", $"{state}", LoaderMapLogType.DF);
                        }
                        else if (state == StateLogType.DONE) //in,out end time update
                        {
                            map.WaferInEndTime = curTime;

                            if (target == origin)
                            {
                                map.IsFinalHolder = true;
                                //complete map으로 이동해야함, substrate 단위로 생성된 unique key(orgin에서 unique key로 변경)
                                if (old != "" && old != origin)
                                {
                                    //manual 로 origin을 바꾸어 최종 holder로 이동한 경우
                                    map.AbnormalSequence = true;
                                    map.ErrorMsg += $"(origin changed {old} to {origin})";
                                    LoaderMapLog($"[UpdateLoaderMapHolder] AbnormalSequence (changed origin), curr:{curr} target:{target} origin:{origin} oldorigin:{old}");
                                }

                                ThroughputInfo?.CompleteMapHolders.TryAdd(substrateMaps[findKey].First().SubstrateKey, substrateMaps[findKey]);

                                List<LoaderMapHolder> removeValue;
                                substrateMaps.TryRemove(findKey, out removeValue);
                            }

                            if (ModuleType.Equals(ModuleLogType.ARM_TO_STAGE))
                            {
                                LoaderMapHolderEx holderEx = new LoaderMapHolderEx(map);
                                if (!ChucksWafer.ContainsKey(target))
                                {
                                    ChucksWafer.TryAdd(target, holderEx);
                                }
                                else
                                {
                                    ChucksWafer[target] = holderEx;
                                }
                            }

                            string desc = $"S:{map.HolderKeyForHtml} | T:{GetSequenceTitle(map, "", findKey)} | O:{findKey} | L:{map.SubstrateLotID} | W:{map.SubstrateWaferID} | K:{map.SubstrateKey} | A:{map.AbnormalSequence} | M:{map.ErrorMsg} | isManual:{map.IsManual}";
                            LoaderMapActionLog(desc, $"{ModuleType}", $"{state}", LoaderMapLogType.DT);
                        }
                        else if (state == StateLogType.ERROR)
                        {
                            map.ErrorSequence = true;
                            map.IsError = true;
                            map.ErrorMsg += $"({errMsg})";
                            string desc = $"S:{map.HolderKeyForHtml} | T:{GetSequenceTitle(map, "", findKey)} | O:{findKey} | L:{map.SubstrateLotID} | W:{map.SubstrateWaferID} | K:{map.SubstrateKey} | A:{map.AbnormalSequence} | M:{map.ErrorMsg} | isManual:{map.IsManual}";
                            LoaderMapActionLog(desc, $"{ModuleType}", $"{state}", LoaderMapLogType.DT);

                            ThroughputInfo?.SaveLoaderSnapshot();
                        }
                        else
                        {
                            LoaderMapLog($"[UpdateLoaderMapHolder] unknown_state, state:{state} curr:{curr} target:{target} origin:{origin} oldorigin:{old}");
                        }
                    }
                    else
                    {
                        LoaderMapLog($"[UpdateLoaderMapHolder] map key is not exist state:{state} curr:{curr} target:{target} origin:{origin} oldorigin:{old}");
                    }
                }
                else
                {
                    if (FoupInfos == null) //lot중이 아님
                    {
                        string desc = $"S: | T:{curr}-){target} | O:{origin} | L: | W:{ocr} | K: | A: | M:{errMsg} | isManual:";
                        LoaderMapActionLog(desc, $"{ModuleType}", $"{state}", LoaderMapLogType.DN);
                    }
                    else
                    {
                        LoaderMapLog($"[UpdateLoaderMapHolder] substrateMaps is null, state:{state} curr:{curr} target:{target} origin:{origin} oldorigin:{old}");
                    }
                }
            }
            catch (Exception err)
            {
                LoaderMapException(err);
            }
        }
        public static void UpdateLoaderMapHolderSubSequence(StateLogType state, string curr, string target, string origin, string holder, SubSequenceType type, string errMsg = "")
        {
            try
            {
                ConcurrentDictionary<string, List<LoaderMapHolder>> substrateMaps = (isMapLog) ? ThroughputInfo?.LoaderMapHolders : LoaderMapHolders;

                if (substrateMaps != null)
                {
                    if (substrateMaps.ContainsKey(origin))
                    {
                        DateTime curTime = DateTime.Now;
                        LoaderMapHolder map = substrateMaps[origin].FirstOrDefault(x => x.HolderName == $"{target}" && x.WaferInStartTime != default && x.WaferOutEndTime == default);
                        if (map == null)
                        {
                            LoaderMapLog($"[UpdateLoaderMapHolderSubSequence] map value is not exist state:{state} curr:{curr} target:{target} origin:{origin} subseq:{type}");
                            return;
                        }

                        if (state == StateLogType.START)
                        {
                            LoaderMapHolder seq = new LoaderMapHolder(holder, type.ToString(), origin, curTime);
                            seq.IsSubSequence = true;

                            if (ThroughputInfo != null)
                            {
                                ThroughputInfo.TotalSubSequence += 1;
                            }

                            seq.HolderKeyForHtml = $"{holder}-){holder}";
                            map.SubSequence.Add(seq);
                            if (type == SubSequenceType.PA_PICK) //pa to arm 시점에서 실제로 wafer가 arm으로 가는 시점, WaferInStart Time을 바꿔준다.
                            {
                                map.WaferInStartTime = curTime;
                            }

                            string desc = $"S:{seq.HolderKeyForHtml} | T:{seq.HolderKey} | O:{origin} | L:{map.SubstrateLotID} | W:{map.SubstrateWaferID} | K:{map.SubstrateKey} | A:false | M: {errMsg} |";
                            LoaderMapActionLog(desc, $"{type}", $"{state}", LoaderMapLogType.DF);
                        }
                        else if (state == StateLogType.DONE || state == StateLogType.ERROR)
                        {
                            LoaderMapHolder seq = map.SubSequence.FirstOrDefault(x => (x.HolderKey == $"{type}" && x.EndTime == default));
                            if (seq == null)
                            {
                                LoaderMapLog($"[UpdateLoaderMapHolderSubSequence] subsequence is not exist state:{state} curr:{curr} target:{target} origin:{origin} subseq:{type}");
                                return;
                            }

                            if (type == SubSequenceType.PA_PUT) //arm to pa 시점에서 실제로 wafer가 pa로 가는 시점
                            {
                                map.EndTime = curTime; //이동관점에서 arm to pa의 완료 시간 이다.(이후 align등의 subsequence가 수행 된다.)
                                curTime = curTime.AddMilliseconds(1);
                            }
                            seq.EndTime = curTime;
                            if (state == StateLogType.ERROR)
                            {
                                seq.ErrorSequence = true;
                                seq.IsError = true;
                                seq.ErrorMsg += $"({errMsg})";

                                ThroughputInfo?.SaveLoaderSnapshot();
                            }

                            string desc = $"S:{seq.HolderKeyForHtml} | T:{GetSequenceTitle(seq, "", origin)} | O:{origin} | L:{map.SubstrateLotID} | W:{map.SubstrateWaferID} | K:{map.SubstrateKey} | A:false | M:{seq.ErrorMsg} |";
                            LoaderMapActionLog(desc, $"{type}", $"{state}", LoaderMapLogType.DT);
                        }
                        else
                        {
                            LoaderMapLog($"[UpdateLoaderMapHolderSubSequence] unknown_state, state:{state} curr:{curr} target:{target} origin:{origin} subseq:{type}");
                        }
                    }
                    else
                    {
                        LoaderMapLog($"[UpdateLoaderMapHolderSubSequence] map key is not exist, state:{state} curr:{curr} target:{target} origin:{origin} subseq:{type}");
                    }
                }
                else
                {
                    if (FoupInfos == null) //lot중이 아님
                    {
                        string desc = $"S: | T:{state} | O:{origin} | L: | W: | K: | A: | M:{errMsg} |";
                        LoaderMapActionLog(desc, $"{type}", $"{state}", LoaderMapLogType.DN);
                    }
                    else
                    {
                        LoaderMapLog($"[UpdateLoaderMapHolderSubSequence] substrateMaps is null, state:{state} curr:{curr} target:{target} origin:{origin} subseq:{type}");
                    }
                }
            }
            catch (Exception err)
            {
                LoaderMapException(err);
            }
        }
        public static void LoaderMapStageProc(string msg, int index = 0, ModuleLogType ModuleType = ModuleLogType.UNDEFIEND, StateLogType State = StateLogType.UNDEFINED)
        {
            try
            {
                if (index < 1)
                {
                    return;
                }

                bool bCheck = (ModuleType == ModuleLogType.PIN_ALIGN || ModuleType == ModuleLogType.WAFER_ALIGN || ModuleType == ModuleLogType.SOAKING
                            || ModuleType == ModuleLogType.MARK_ALIGN || ModuleType == ModuleLogType.CLEANING || ModuleType == ModuleLogType.PROBING
                            || ModuleType == ModuleLogType.WAFER_LOAD || ModuleType == ModuleLogType.WAFER_UNLOAD);

                string cell = $"CHUCK{index}";
                if (bCheck)
                {
                    int cellMsgIndex = msg.LastIndexOf("|");
                    if (cellMsgIndex != -1)
                    {
                        msg = msg.Substring(cellMsgIndex + 1).Trim();
                    }

                    ConcurrentDictionary<string, List<LoaderMapHolder>> substrateMaps = (isMapLog) ? ThroughputInfo?.LoaderMapHolders : LoaderMapHolders;
                    if (substrateMaps == null || ChucksWafer == null)
                    {
                        if (FoupInfos == null) //lot중이 아님
                        {
                            string desc = $"S: | T:{ModuleType}_{State} | O: | L: | W: | K: | A: | M: |{msg}";
                            LoaderMapActionLog(desc, $"{ModuleType}", $"{State}", LoaderMapLogType.DN, index);
                        }
                        return;
                    }

                    DateTime curTime = DateTime.Now;
                    if (ThroughputInfo != null && ThroughputInfo.StageSubSequences != null && !ThroughputInfo.StageSubSequences.ContainsKey(cell))
                    {
                        ThroughputInfo?.StageSubSequences.TryAdd(cell, new List<LoaderMapHolder>());
                    }

                    bool bExistHolder = ChucksWafer != null && ChucksWafer.ContainsKey(cell) && ChucksWafer[cell] != null && ChucksWafer[cell].holder != null;
                    LoaderMapHolder map = bExistHolder ? ChucksWafer[cell].holder : null;
                    string org = bExistHolder ? map.Origin : "";
                    string lotid = bExistHolder ? map.SubstrateLotID : "";
                    string ocr = bExistHolder ? map.SubstrateWaferID : "";
                    string key = bExistHolder ? map.SubstrateKey : "";
                    bool abnormal = bExistHolder && map.AbnormalSequence;
                    if (string.IsNullOrEmpty(lotid))
                    {
                        if (ChucksWafer != null && ChucksWafer.ContainsKey(cell) && ChucksWafer[cell] != null)
                        {
                            lotid = ChucksWafer[cell].AssignLotID;
                        }
                    }

                    if (string.IsNullOrEmpty(lotid) && string.IsNullOrEmpty(msg) == false)
                    {
                        if (ExtractLotId(msg, out lotid))
                        {
                            if (bExistHolder)
                            {
                                map.SubstrateLotID = lotid;
                            }

                            if (ChucksWafer != null)
                            {
                                if (!ChucksWafer.ContainsKey(cell))
                                {
                                    LoaderMapHolderEx holderEx = new LoaderMapHolderEx();
                                    holderEx.AssignLotID = lotid;
                                    ChucksWafer.TryAdd(cell, holderEx);
                                }
                                else
                                {
                                    ChucksWafer[cell].AssignLotID = lotid;
                                }
                            }
                        }
                    }

                    string title = $"{ModuleType}_{State}";
                    if (State == StateLogType.START)
                    {
                        if (ThroughputInfo != null)
                        {
                            LoaderMapHolder seq = new LoaderMapHolder(cell, ModuleType.ToString() + "_" + State.ToString(), "", curTime);
                            seq.IsSubSequence = true;
                            ThroughputInfo.TotalSubSequence += 1;
                            seq.HolderKeyForHtml = $"{cell}-){cell}";
                            ThroughputInfo?.StageSubSequences[cell].Add(seq);
                        }
                        string desc = $"S:{cell}-){cell} | T:{GetSequenceTitle(null, title, org)} | O:{org} | L:{lotid} | W:{ocr} | K:{key} | A:{abnormal} | M: |{msg}";
                        LoaderMapActionLog(desc, $"{ModuleType}", $"{State}", LoaderMapLogType.DT, index);
                    }
                    else if (State == StateLogType.DONE || State == StateLogType.ERROR || State == StateLogType.ABORT)
                    {
                        string desc = "";
                        LoaderMapHolder seq = null;
                        if (ThroughputInfo != null)
                        {
                            bool bLoop = false;
                            do
                            {
                                string preSequence = $"{ModuleType}_START";
                                LoaderMapHolder preSeq = ThroughputInfo?.StageSubSequences[cell].LastOrDefault(x => x.HolderKey == preSequence && x.EndTime == default);
                                if (preSeq == null)
                                {
                                    LoaderMapLog($"[UpdateStageSubSequence] subsequence is not exist cell:{cell} module:{ModuleType} state:{State} preseq:{preSequence}");
                                    break;
                                }
                                preSeq.EndTime = preSeq.StartTime; //start subsequece의 end time을 이때 지정한다. start 시간으로 함

                                seq = new LoaderMapHolder(cell, ModuleType.ToString() + "_" + State.ToString(), "", preSeq.StartTime);
                                seq.IsSubSequence = true;
                                seq.HolderKeyForHtml = $"{cell}-){cell}";
                                ThroughputInfo?.StageSubSequences[cell].Add(seq);
                                seq.EndTime = curTime;

                            } while (bLoop);
                        }

                        if (State == StateLogType.ERROR)
                        {
                            desc = $"{cell}-){cell} | T:{GetSequenceTitle(null, title, org)} | O:{org} | L:{lotid} | W:{ocr} | K:{key} | A:{abnormal} | M:{msg} |";
                            LoaderMapActionLog(desc, $"{ModuleType}", $"{State}", LoaderMapLogType.DT, index);

                            if (seq != null)
                            {
                                seq.ErrorSequence = true;
                                seq.IsError = true;
                                seq.ErrorMsg = $"({msg})";
                                ThroughputInfo?.SaveLoaderSnapshot();
                            }
                        }
                        else
                        {
                            desc = $"S:{cell}-){cell} | T:{GetSequenceTitle(null, title, org)} | O:{org} | L:{lotid} | W:{ocr} | K:{key} | A:{abnormal} | M: |{msg}";
                            LoaderMapActionLog(desc, $"{ModuleType}", $"{State}", LoaderMapLogType.DT, index);
                            //unload시 ChuckWafer 정보를 초기화 한다.
                            if (ModuleType == ModuleLogType.WAFER_UNLOAD)
                            {
                                if (ChucksWafer.ContainsKey(cell))
                                {
                                    ChucksWafer[cell].holder = null;
                                }
                            }
                        }
                    }
                    else
                    {
                        LoaderMapLog($"[UpdateStageSubSequence] unknown_state cell:{cell} module:{ModuleType} state:{State} {msg}");
                    }
                }
                else
                {
                    //cell 에서 보낸 로그를 그대로 출력한다.
                    LoaderMapLog(msg, directCall: false);
                }
            }
            catch (Exception err)
            {
                LoaderMapException(err);
            }
        }
        public static void LoaderMapProc(string msg, int index = 0, ModuleLogType ModuleType = ModuleLogType.UNDEFIEND, StateLogType State = StateLogType.UNDEFINED)
        {
            try
            {
                if (index == 0)
                {
                    LoaderMapLog(msg, directCall: false);
                }

                if (index == 0 && ModuleType == ModuleLogType.LOT) //loader의 lot start와 all end 시점만 처리하기위해 index를 본다.
                {
                    if (State == StateLogType.START)
                    {
                        if (FoupInfos == null)
                        {
                            FoupInfos = new ConcurrentDictionary<int, LoaderMapFoupInfo>();
                            LoaderMapLog($"Loader Sequence Start.");
                        }
                        if (ChucksWafer == null)
                        {
                            ChucksWafer = new ConcurrentDictionary<string, LoaderMapHolderEx>();
                        }

                        if (isMapLog)
                        {
                            if (ThroughputInfo == null) //첫번째 lot start 시점에 만든다.
                            {
                                DateTime curTime = DateTime.Now;
                                ThroughputInfo = new LoaderThroughputInfo(curTime, curTime.Ticks.GetHashCode().ToString());
                            }
                        }
                        else
                        {
                            if (LoaderMapHolders == null)
                            {
                                LoaderMapHolders = new ConcurrentDictionary<string, List<LoaderMapHolder>>();
                            }
                        }
                    }
                    else if (State == StateLogType.LOADERALLDONE) //모든 lot가 종료 되는 시점임
                    {
                        if (isMapLog)
                        {
                            if (ThroughputInfo != null)
                            {
                                ThroughputInfo.LoaderLotEndTime = DateTime.Now;
                                ThroughputInfo.SaveLoaderThroughputCSV();
                                ThroughputInfo.Dispose();
                                ThroughputInfo = null;
                            }
                        }
                        else
                        {
                            LoaderMapHolders?.Clear();
                            LoaderMapHolders = null;
                        }

                        ChucksWafer.Clear();
                        ChucksWafer = null;
                        FoupInfos.Clear();
                        FoupInfos = null;
                        sequenceNum = 1;
                        LoaderMapLog($"Loader Sequence End.");
                    }
                    else if (State == StateLogType.DONE)
                    {
                        // 추후 foup 기준 lot end시 어떻게 할지 추가 고려 하도록 함
                    }
                }
            }
            catch (Exception err)
            {
                LoaderMapException(err);
            }
        }
        public static void LoaderMapActionLog(string desc, string action, string state, LoaderMapLogType type, int index = 0)
        {
            try
            {
                string subject = "LOADER";
                if (index > 0)
                {
                    if (index > 0 && index < 10)
                    {
                        subject = $"CELL0" + index;
                    }
                    else
                    {
                        subject = $"CELL" + index;
                    }
                }

                string msg = $"{subject} | {LoaderState} | {action} | {state} | {type} | {desc}";
                LoaderMapLoggerCtl?.WriteLog(msg);
            }
            catch (Exception err)
            {
                LoaderMapException(err);
            }
        }
        public static void LoaderMapLog(string msg, bool directCall = true, bool basicLog = true)
        {
            try
            {
                bool bWriteDebug = false;
                if (isMapLog || basicLog)
                {
                    bWriteDebug = true;
                }

                if (bWriteDebug)
                {
                    if (directCall)
                    {
                        msg = $"LOADER{GetTempStr()} | {LoaderState} | {msg}";
                    }
                    else
                    {
                        int n = msg.IndexOf("|");
                        if (n >= 0)
                        {
                            msg = msg.Insert(n + 1, $" {LoaderState} |");
                        }
                        else
                        {
                            msg = $"LOADER{GetTempStr()} | {LoaderState} | {msg}";
                        }
                    }
                    LoaderMapLoggerCtl?.WriteLog(msg);
                }
            }
            catch (Exception err)
            {
                LoaderMapException(err);
            }
        }
        public static void LoaderMapException(Exception err, string msg = null)
        {
            try
            {
                string msgFormat = $"MSG : {err.Message}, Target : {err.TargetSite}, Source : {err.Source}, {Environment.NewLine} {err.StackTrace}{msg}";

                msgFormat = $"Exception{GetTempStr()} | {msgFormat}";

                LoaderMapLoggerCtl?.WriteLog(msgFormat);
            }
            catch (Exception error)
            {
                Exception(error);
            }
        }

        #endregion

        public static EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                ProLogBuffer = new ObservableCollection<LogEventInfo>();
                EventLogBuffer = new ObservableCollection<LogEventInfo>();
                CompVerifyLogBuffer = new ObservableCollection<string>();
                RecoveryLogBuffer = new ObservableCollection<string>();

                PrologBufferIndex = 0;
                EventLogBufferIndex = 0;

                retval = LoadParameter();

                if (retval != EventCodeEnum.NONE)
                {
                    return retval;
                }

                _NLogCtlResFac = new NLoggerControllerResourceFactory();

                _NLogCtlResFac.AddResource(EnumLoggerType.PROLOG, LoggerManagerParam.ProLoggerParam, new ProLoggerRuleConfig());
                _NLogCtlResFac.AddResource(EnumLoggerType.DEBUG, LoggerManagerParam.DebugLoggerParam, new DebugLoggerRuleConfig());
                _NLogCtlResFac.AddResource(EnumLoggerType.EXCEPTION, LoggerManagerParam.ExceptionLoggerParam, new DebugDetailLoggerRuleConfig());
                _NLogCtlResFac.AddResource(EnumLoggerType.GPIB, LoggerManagerParam.GpibLoggerParam, new GpibLoggerRuleConfig());
                _NLogCtlResFac.AddResource(EnumLoggerType.EVENT, LoggerManagerParam.EventLoggerParam, new EventLoggerRuleConfig());
                _NLogCtlResFac.AddResource(EnumLoggerType.PIN, LoggerManagerParam.PinLoggerParam, new PinLoggerRuleConfig());
                _NLogCtlResFac.AddResource(EnumLoggerType.PMI, LoggerManagerParam.PMILoggerParam, new PMILoggerRuleConfig());
                _NLogCtlResFac.AddResource(EnumLoggerType.TEMP, LoggerManagerParam.TempLoggerParam, new TempLoggerRuleConfig());
                _NLogCtlResFac.AddResource(EnumLoggerType.LOT, LoggerManagerParam.LOTLoggerParam, new LOTLoggerRuleConfig());
                _NLogCtlResFac.AddResource(EnumLoggerType.TCPIP, LoggerManagerParam.TCPIPLoggerParam, new TCPIPLoggerRuleConfig());
                _NLogCtlResFac.AddResource(EnumLoggerType.PARAMETER, LoggerManagerParam.ParamLoggerParam, new ParamLoggerRuleConfig());
                _NLogCtlResFac.AddResource(EnumLoggerType.SOAKING, LoggerManagerParam.SoakingLoggerParam, new SoakingLoggerRuleConfig());
                _NLogCtlResFac.AddResource(EnumLoggerType.COMPVERIFY, LoggerManagerParam.CompVerifyLoggerParam, new CompVerifyLoggerRuleConfig());
                _NLogCtlResFac.AddResource(EnumLoggerType.INFO, LoggerManagerParam.InfoLoggerParam, new InfoLoggerRuleConfig());
                _NLogCtlResFac.AddResource(EnumLoggerType.MONITORING, LoggerManagerParam.MonitoringLoggerParam, new MonitoringLoggerRuleConfig());
                _NLogCtlResFac.AddResource(EnumLoggerType.LOADERMAP, LoggerManagerParam.LoaderMapLoggerParam, new LoaderMapLoggerRuleConfig());


                //==> Add SmokeSensor
                _NLogCtlResFac.AddResource(EnumLoggerType.ENVMONITORING, LoggerManagerParam.EnvMonitoringLoggerParam,new EnvMonitoringLoggerRuleConfig());

                //==> Logger Rule 구성
                _NLogCtlResFac.ConfigLoggerRule();

                //==> Set Logger Control
                ProLoggerCtl = new NLoggerController(_NLogCtlResFac.NLoggerDic[EnumLoggerType.PROLOG], _NLogCtlResFac.NLoggerParamDic[EnumLoggerType.PROLOG], _NLogCtlResFac.NLoggerRuleDic[EnumLoggerType.PROLOG]);
                DebugLoggerCtl = new NLoggerController(_NLogCtlResFac.NLoggerDic[EnumLoggerType.DEBUG], _NLogCtlResFac.NLoggerParamDic[EnumLoggerType.DEBUG], _NLogCtlResFac.NLoggerRuleDic[EnumLoggerType.DEBUG]);
                DebugDetailLoggerCtl = new NLoggerController(_NLogCtlResFac.NLoggerDic[EnumLoggerType.EXCEPTION], _NLogCtlResFac.NLoggerParamDic[EnumLoggerType.EXCEPTION], _NLogCtlResFac.NLoggerRuleDic[EnumLoggerType.EXCEPTION]);
                GpibLoggerCtl = new NLoggerController(_NLogCtlResFac.NLoggerDic[EnumLoggerType.GPIB], _NLogCtlResFac.NLoggerParamDic[EnumLoggerType.GPIB], _NLogCtlResFac.NLoggerRuleDic[EnumLoggerType.GPIB]);
                EventLoggerCtl = new NLoggerController(_NLogCtlResFac.NLoggerDic[EnumLoggerType.EVENT], _NLogCtlResFac.NLoggerParamDic[EnumLoggerType.EVENT], _NLogCtlResFac.NLoggerRuleDic[EnumLoggerType.EVENT]);
                PinAlignLoggerCtl = new NLoggerController(_NLogCtlResFac.NLoggerDic[EnumLoggerType.PIN], _NLogCtlResFac.NLoggerParamDic[EnumLoggerType.PIN], _NLogCtlResFac.NLoggerRuleDic[EnumLoggerType.PIN]);
                PMILoggerCtl = new NLoggerController(_NLogCtlResFac.NLoggerDic[EnumLoggerType.PMI], _NLogCtlResFac.NLoggerParamDic[EnumLoggerType.PMI], _NLogCtlResFac.NLoggerRuleDic[EnumLoggerType.PMI]);
                TempLoggerCtl = new NLoggerController(_NLogCtlResFac.NLoggerDic[EnumLoggerType.TEMP], _NLogCtlResFac.NLoggerParamDic[EnumLoggerType.TEMP], _NLogCtlResFac.NLoggerRuleDic[EnumLoggerType.TEMP]);
                LOTLoggerCtl = new NLoggerController(_NLogCtlResFac.NLoggerDic[EnumLoggerType.LOT], _NLogCtlResFac.NLoggerParamDic[EnumLoggerType.LOT], _NLogCtlResFac.NLoggerRuleDic[EnumLoggerType.LOT]);
                TCPIPLoggerCtl = new NLoggerController(_NLogCtlResFac.NLoggerDic[EnumLoggerType.TCPIP], _NLogCtlResFac.NLoggerParamDic[EnumLoggerType.TCPIP], _NLogCtlResFac.NLoggerRuleDic[EnumLoggerType.TCPIP]);
                ParamLoggerCtl = new NLoggerController(_NLogCtlResFac.NLoggerDic[EnumLoggerType.PARAMETER], _NLogCtlResFac.NLoggerParamDic[EnumLoggerType.PARAMETER], _NLogCtlResFac.NLoggerRuleDic[EnumLoggerType.PARAMETER]);
                SoakingLoggerCtl = new NLoggerController(_NLogCtlResFac.NLoggerDic[EnumLoggerType.SOAKING], _NLogCtlResFac.NLoggerParamDic[EnumLoggerType.SOAKING], _NLogCtlResFac.NLoggerRuleDic[EnumLoggerType.SOAKING]);
                CompVerifyLoggerCtl = new NLoggerController(_NLogCtlResFac.NLoggerDic[EnumLoggerType.COMPVERIFY], _NLogCtlResFac.NLoggerParamDic[EnumLoggerType.COMPVERIFY], _NLogCtlResFac.NLoggerRuleDic[EnumLoggerType.COMPVERIFY]);
                MonitoringLoggerCtl = new NLoggerController(_NLogCtlResFac.NLoggerDic[EnumLoggerType.MONITORING], _NLogCtlResFac.NLoggerParamDic[EnumLoggerType.MONITORING], _NLogCtlResFac.NLoggerRuleDic[EnumLoggerType.MONITORING]);
                InfoLoggerCtl = new NLoggerController(_NLogCtlResFac.NLoggerDic[EnumLoggerType.INFO], _NLogCtlResFac.NLoggerParamDic[EnumLoggerType.INFO], _NLogCtlResFac.NLoggerRuleDic[EnumLoggerType.INFO]);
                SmokeSensorLoggerCtl = new NLoggerController(_NLogCtlResFac.NLoggerDic[EnumLoggerType.ENVMONITORING],_NLogCtlResFac.NLoggerParamDic[EnumLoggerType.ENVMONITORING],_NLogCtlResFac.NLoggerRuleDic[EnumLoggerType.ENVMONITORING]);
                LoaderMapLoggerCtl = new NLoggerController(_NLogCtlResFac.NLoggerDic[EnumLoggerType.LOADERMAP], _NLogCtlResFac.NLoggerParamDic[EnumLoggerType.LOADERMAP], _NLogCtlResFac.NLoggerRuleDic[EnumLoggerType.LOADERMAP]);

                CognexLoggerCtl = new ImageLoggerController(LoggerManagerParam.CognexLoggerParam);

                var fileTarget = (FileTarget)LogManager.Configuration.FindTargetByName("debugLogFile");

                var logEventInfo = new LogEventInfo { TimeStamp = DateTime.Now };

                if (fileTarget != null)
                {
                    string fileName = fileTarget.FileName.Render(logEventInfo);

                    if (fileName != string.Empty)
                    {
                        CurrentDebuglogPath = fileName;
                    }
                }

                //EventLogMg.WriteEventCodeEnumTable();
                EventLogMg.StartUpdateAlarm();

                if (stateStrMap == null)
                {
                    // Initialize the stateCharMap to map each ModuleStateEnum value to a character from A to M
                    stateStrMap = new Dictionary<string, string>
                    {
                        { "UNDEFINED", "UN" },
                        { "INIT", "IN" },
                        { "IDLE", "ID" },
                        { "RUNNING", "RN" },
                        { "PENDING", "PD" },
                        { "SUSPENDED", "SP" },
                        { "ABORT", "AT" },
                        { "DONE", "DN" },
                        { "ERROR", "ER" },
                        { "PAUSED", "PE" },
                        { "RECOVERY", "RC" },
                        { "RESUMMING", "RS" },
                        { "PAUSING", "PI" }
                    };
                }

                print_memoryInfo();

                if (retval != EventCodeEnum.NONE)
                {
                    return retval;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public static EventCodeEnum Init_GPLoader()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                ProLogBuffer = new ObservableCollection<LogEventInfo>();
                EventLogBuffer = new ObservableCollection<LogEventInfo>();

                PrologBufferIndex = 0;
                EventLogBufferIndex = 0;

                retval = LoadParameter();

                if (retval != EventCodeEnum.NONE)
                {
                    return retval;
                }

                _NLogCtlResFac = new NLoggerControllerResourceFactory();
                _NLogCtlResFac.AddResource(EnumLoggerType.PROLOG, LoggerManagerParam.ProLoggerParam, new ProLoggerRuleConfig());
                _NLogCtlResFac.AddResource(EnumLoggerType.DEBUG, LoggerManagerParam.DebugLoggerParam, new DebugLoggerRuleConfig());
                _NLogCtlResFac.AddResource(EnumLoggerType.EXCEPTION, LoggerManagerParam.ExceptionLoggerParam, new DebugDetailLoggerRuleConfig());
                _NLogCtlResFac.AddResource(EnumLoggerType.GPIB, LoggerManagerParam.GpibLoggerParam, new GpibLoggerRuleConfig());
                _NLogCtlResFac.AddResource(EnumLoggerType.EVENT, LoggerManagerParam.EventLoggerParam, new EventLoggerRuleConfig());
                _NLogCtlResFac.AddResource(EnumLoggerType.PIN, LoggerManagerParam.PinLoggerParam, new PinLoggerRuleConfig());
                _NLogCtlResFac.AddResource(EnumLoggerType.PMI, LoggerManagerParam.PMILoggerParam, new PMILoggerRuleConfig());
                _NLogCtlResFac.AddResource(EnumLoggerType.TEMP, LoggerManagerParam.TempLoggerParam, new TempLoggerRuleConfig());
                _NLogCtlResFac.AddResource(EnumLoggerType.LOT, LoggerManagerParam.LOTLoggerParam, new LOTLoggerRuleConfig());
                _NLogCtlResFac.AddResource(EnumLoggerType.TCPIP, LoggerManagerParam.TCPIPLoggerParam, new TCPIPLoggerRuleConfig());
                _NLogCtlResFac.AddResource(EnumLoggerType.PARAMETER, LoggerManagerParam.ParamLoggerParam, new ParamLoggerRuleConfig());
                _NLogCtlResFac.AddResource(EnumLoggerType.SOAKING, LoggerManagerParam.SoakingLoggerParam, new SoakingLoggerRuleConfig());
                _NLogCtlResFac.AddResource(EnumLoggerType.COMPVERIFY, LoggerManagerParam.CompVerifyLoggerParam, new CompVerifyLoggerRuleConfig());
                _NLogCtlResFac.AddResource(EnumLoggerType.MONITORING, LoggerManagerParam.MonitoringLoggerParam, new MonitoringLoggerRuleConfig());
                _NLogCtlResFac.AddResource(EnumLoggerType.INFO, LoggerManagerParam.InfoLoggerParam, new InfoLoggerRuleConfig());
                //==> Add SmokeSensor
                _NLogCtlResFac.AddResource(EnumLoggerType.ENVMONITORING, LoggerManagerParam.EnvMonitoringLoggerParam,new EnvMonitoringLoggerRuleConfig());
                _NLogCtlResFac.AddResource(EnumLoggerType.LOADERMAP, LoggerManagerParam.LoaderMapLoggerParam, new LoaderMapLoggerRuleConfig());

                //==> Logger Rule 구성
                _NLogCtlResFac.ConfigLoggerRule();

                //==> Set Logger Control
                ProLoggerCtl = new NLoggerController(_NLogCtlResFac.NLoggerDic[EnumLoggerType.PROLOG], _NLogCtlResFac.NLoggerParamDic[EnumLoggerType.PROLOG], _NLogCtlResFac.NLoggerRuleDic[EnumLoggerType.PROLOG]);
                DebugLoggerCtl = new NLoggerController(_NLogCtlResFac.NLoggerDic[EnumLoggerType.DEBUG], _NLogCtlResFac.NLoggerParamDic[EnumLoggerType.DEBUG], _NLogCtlResFac.NLoggerRuleDic[EnumLoggerType.DEBUG]);
                DebugDetailLoggerCtl = new NLoggerController(_NLogCtlResFac.NLoggerDic[EnumLoggerType.EXCEPTION], _NLogCtlResFac.NLoggerParamDic[EnumLoggerType.EXCEPTION], _NLogCtlResFac.NLoggerRuleDic[EnumLoggerType.EXCEPTION]);
                GpibLoggerCtl = new NLoggerController(_NLogCtlResFac.NLoggerDic[EnumLoggerType.GPIB], _NLogCtlResFac.NLoggerParamDic[EnumLoggerType.GPIB], _NLogCtlResFac.NLoggerRuleDic[EnumLoggerType.GPIB]);
                EventLoggerCtl = new NLoggerController(_NLogCtlResFac.NLoggerDic[EnumLoggerType.EVENT], _NLogCtlResFac.NLoggerParamDic[EnumLoggerType.EVENT], _NLogCtlResFac.NLoggerRuleDic[EnumLoggerType.EVENT]);
                PinAlignLoggerCtl = new NLoggerController(_NLogCtlResFac.NLoggerDic[EnumLoggerType.PIN], _NLogCtlResFac.NLoggerParamDic[EnumLoggerType.PIN], _NLogCtlResFac.NLoggerRuleDic[EnumLoggerType.PIN]);
                PMILoggerCtl = new NLoggerController(_NLogCtlResFac.NLoggerDic[EnumLoggerType.PMI], _NLogCtlResFac.NLoggerParamDic[EnumLoggerType.PMI], _NLogCtlResFac.NLoggerRuleDic[EnumLoggerType.PMI]);
                TempLoggerCtl = new NLoggerController(_NLogCtlResFac.NLoggerDic[EnumLoggerType.TEMP], _NLogCtlResFac.NLoggerParamDic[EnumLoggerType.TEMP], _NLogCtlResFac.NLoggerRuleDic[EnumLoggerType.TEMP]);
                LOTLoggerCtl = new NLoggerController(_NLogCtlResFac.NLoggerDic[EnumLoggerType.LOT], _NLogCtlResFac.NLoggerParamDic[EnumLoggerType.LOT], _NLogCtlResFac.NLoggerRuleDic[EnumLoggerType.LOT]);
                TCPIPLoggerCtl = new NLoggerController(_NLogCtlResFac.NLoggerDic[EnumLoggerType.TCPIP], _NLogCtlResFac.NLoggerParamDic[EnumLoggerType.TCPIP], _NLogCtlResFac.NLoggerRuleDic[EnumLoggerType.TCPIP]);
                ParamLoggerCtl = new NLoggerController(_NLogCtlResFac.NLoggerDic[EnumLoggerType.PARAMETER], _NLogCtlResFac.NLoggerParamDic[EnumLoggerType.PARAMETER], _NLogCtlResFac.NLoggerRuleDic[EnumLoggerType.PARAMETER]);
                SoakingLoggerCtl = new NLoggerController(_NLogCtlResFac.NLoggerDic[EnumLoggerType.SOAKING], _NLogCtlResFac.NLoggerParamDic[EnumLoggerType.SOAKING], _NLogCtlResFac.NLoggerRuleDic[EnumLoggerType.SOAKING]);
                CompVerifyLoggerCtl = new NLoggerController(_NLogCtlResFac.NLoggerDic[EnumLoggerType.COMPVERIFY], _NLogCtlResFac.NLoggerParamDic[EnumLoggerType.COMPVERIFY], _NLogCtlResFac.NLoggerRuleDic[EnumLoggerType.COMPVERIFY]);
                MonitoringLoggerCtl = new NLoggerController(_NLogCtlResFac.NLoggerDic[EnumLoggerType.MONITORING], _NLogCtlResFac.NLoggerParamDic[EnumLoggerType.MONITORING], _NLogCtlResFac.NLoggerRuleDic[EnumLoggerType.MONITORING]);
                InfoLoggerCtl = new NLoggerController(_NLogCtlResFac.NLoggerDic[EnumLoggerType.INFO], _NLogCtlResFac.NLoggerParamDic[EnumLoggerType.INFO], _NLogCtlResFac.NLoggerRuleDic[EnumLoggerType.INFO]);
                SmokeSensorLoggerCtl = new NLoggerController( _NLogCtlResFac.NLoggerDic[EnumLoggerType.ENVMONITORING],_NLogCtlResFac.NLoggerParamDic[EnumLoggerType.ENVMONITORING],_NLogCtlResFac.NLoggerRuleDic[EnumLoggerType.ENVMONITORING]);
                LoaderMapLoggerCtl = new NLoggerController(_NLogCtlResFac.NLoggerDic[EnumLoggerType.LOADERMAP], _NLogCtlResFac.NLoggerParamDic[EnumLoggerType.LOADERMAP], _NLogCtlResFac.NLoggerRuleDic[EnumLoggerType.LOADERMAP]);

                CognexLoggerCtl = new ImageLoggerController(LoggerManagerParam.CognexLoggerParam);

                LOT_Cell_LoggerCtlList = new List<NLoggerController>();
                Param_Cell_LoggerCtlList = new List<NLoggerController>();

                for (int i = 0; i <= GPCellCount; i++)
                {
                    var ruleConfig = new LOTLoggerRuleConfig(i);
                    var logParam = new NLoggerParam();

                    if (i == 0)
                    {
                        logParam.LogDirPath = Path.Combine(LoggerManagerParam.FilePath, LoggerManagerParam.DevFolder, EnumLoggerType.LOT.ToString(), "LOADER");
                    }
                    else
                    {
                        logParam.LogDirPath = Path.Combine(LoggerManagerParam.FilePath, LoggerManagerParam.DevFolder, EnumLoggerType.LOT.ToString(), $"CELL{i}");
                    }

                    logParam.UploadPath = "";
                    logParam.UploadEnable = false;
                    logParam.FileSizeLimit = 104857600;//==> Byte
                    logParam.UploadFileSizeInterval = 0;
                    logParam.UploadTimeInterval = 0;
                    logParam.DeleteLogByDay = 90;

                    if (!Directory.Exists(logParam.LogDirPath))
                    {
                        Directory.CreateDirectory(logParam.LogDirPath);
                    }

                    ruleConfig.Config("LOT_" + i.ToString(), _NLogCtlResFac._Config, logParam);
                }

                LogManager.Configuration = _NLogCtlResFac._Config;

                for (int i = 0; i <= GPCellCount; i++)
                {
                    LOT_Cell_LoggerCtlList.Add(new NLoggerController(LogManager.GetLogger("LOT_" + i.ToString()), null, null));
                }

                for (int i = 0; i <= GPCellCount; i++)
                {
                    var ruleConfig = new ParamLoggerRuleConfig(i);
                    var logParam = new NLoggerParam();

                    if (i == 0)
                    {
                        logParam.LogDirPath = Path.Combine(LoggerManagerParam.FilePath, LoggerManagerParam.DevFolder, EnumLoggerType.PARAMETER.ToString(), "LOADER");
                    }
                    else
                    {
                        logParam.LogDirPath = Path.Combine(LoggerManagerParam.FilePath, LoggerManagerParam.DevFolder, EnumLoggerType.PARAMETER.ToString(), $"CELL{i}");
                    }

                    logParam.UploadPath = "";
                    logParam.UploadEnable = false;
                    logParam.FileSizeLimit = 104857600;//==> Byte
                    logParam.UploadFileSizeInterval = 0;
                    logParam.UploadTimeInterval = 0;
                    logParam.DeleteLogByDay = 90;

                    if (!Directory.Exists(logParam.LogDirPath))
                    {
                        Directory.CreateDirectory(logParam.LogDirPath);
                    }

                    ruleConfig.Config("PARAM_" + i.ToString(), _NLogCtlResFac._Config, logParam);
                }

                LogManager.Configuration = _NLogCtlResFac._Config;

                for (int i = 0; i <= GPCellCount; i++)
                {
                    Param_Cell_LoggerCtlList.Add(new NLoggerController(LogManager.GetLogger("PARAM_" + i.ToString()), null, null));
                }

                var fileTarget = (FileTarget)LogManager.Configuration.FindTargetByName("debugLogFile");

                var logEventInfo = new LogEventInfo { TimeStamp = DateTime.Now };

                if (fileTarget != null)
                {
                    string fileName = fileTarget.FileName.Render(logEventInfo);

                    if (fileName != string.Empty)
                    {
                        CurrentDebuglogPath = fileName;
                    }
                }

                EventLogMg.StartUpdateAlarm();

                foreach (var loggerRule in _NLogCtlResFac.NLoggerRuleDic)
                {
                    EnumLoggerType loggerType = loggerRule.Key;
                    NLoggerRuleConfiger loggerRuleConfiger = loggerRule.Value;

                    String loggerName = loggerType.ToString();
                    NLoggerParam param;

                    if (_NLogCtlResFac.NLoggerParamDic.TryGetValue(loggerType, out param))
                    {
                        LoggerManager.Debug($"[LoggerManager], Init() : LoggerName = {loggerName}, LogDirPath = {param.LogDirPath}");
                    }
                }

                if (retval != EventCodeEnum.NONE)
                {
                    return retval;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public static EventCodeEnum LoadParameter()
        {
            return LoadSysParameter();
        }
        public static EventCodeEnum SaveParameter()
        {
            return SaveSysParameter();
        }
        public static EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            LoggerManagerParam = new LoggerManagerParameter();

            object deserializedObj;

            string filePath = LoggerManagerParam.GetFilePath();
            string fileName = LoggerManagerParam.FileName;
            string fullPath = Path.Combine(filePath, filePath);

            fullPath = Path.Combine(fullPath, fileName);

            try
            {
                bool serializeResult = false;

                if (Directory.Exists(Path.GetDirectoryName(fullPath)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                }

                if (File.Exists(fullPath) == false)
                {
                    LoggerManagerParam.SetDefaultParam();
                    serializeResult = SerializeManager.Serialize(fullPath, LoggerManagerParam);

                    if (RetVal == EventCodeEnum.PARAM_ERROR)
                    {
                        return RetVal;
                    }
                }

                serializeResult = SerializeManager.Deserialize(fullPath, out deserializedObj, typeof(LoggerManagerParameter));

                if (deserializedObj == null | serializeResult == false)
                {
                    RetVal = EventCodeEnum.PARAM_ERROR;

                    return RetVal;
                }

                LoggerManagerParam = deserializedObj as LoggerManagerParameter;

                LoggerManagerParam.Init();

                isMapLog = LoggerManagerParam.LoaderMapLog;

                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[LoggerManager] [Method = LoadSysParameter] [Error = {err}]");
                RetVal = EventCodeEnum.PARAM_ERROR;
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public static string[] LoadEventLog(string lFileName)
        {
            string[] lines;
            string filePath = LoggerManagerParam.EventLoggerParam.LogDirPath;
            string fileName = "\\" + lFileName + ".log";

            string fullPath;

            fullPath = filePath + fileName;

            try
            {
                if (Directory.Exists(Path.GetDirectoryName(fullPath)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                }

                if (File.Exists(fullPath) == false)
                {
                    var file = File.OpenWrite(fullPath);
                    file.Close();
                }

                if (File.Exists(fullPath) == true)
                {
                    return lines = File.ReadAllLines(fullPath);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[LoggerManager] [Method = LoadSysParameter] [Error = {err}]");
                LoggerManager.Exception(err);
            }

            return lines = null;
        }

        public static List<List<string>> UpdateLogFile()
        {
            List<List<string>> retval = new List<List<string>>();
            List<NLoggerParam> nLogParamList = new List<NLoggerParam>(_NLogCtlResFac.NLoggerParamDic.Values);

            retval = logTransfer.UpdateLogFile(nLogParamList);

            return retval;
        }
        public static byte[] OpenLogFile(string filePath)
        {
            byte[] retval = null;
            retval = logTransfer.OpenLogFile(filePath);

            return retval;
        }

        public static void SetParameter(Dictionary<string, string> loggerparams, out bool needInitFileManger)
        {
            needInitFileManger = false;

            try
            {
                if (loggerparams != null)
                {
                    bool ischanged = false;

                    foreach (var param in loggerparams)
                    {
                        switch (param.Key)
                        {
                            case "DEBUG":
                                if (LoggerManagerParam.DebugLoggerParam.LogDirPath.Equals(param.Value) == false)
                                {
                                    LoggerManagerParam.DebugLoggerParam.LogDirPath = param.Value;
                                    ischanged = true;
                                    LoggerManager.Debug($"DebugLoggerParam LogDirPath is changed to {LoggerManagerParam.DebugLoggerParam.LogDirPath}");
                                }
                                break;
                            case "PROLOG":
                                if (LoggerManagerParam.ProLoggerParam.LogDirPath.Equals(param.Value) == false)
                                {
                                    LoggerManagerParam.ProLoggerParam.LogDirPath = param.Value;
                                    ischanged = true;
                                    LoggerManager.Debug($"ProLoggerParam LogDirPath is changed to {LoggerManagerParam.ProLoggerParam.LogDirPath}");
                                }
                                break;
                            case "EVENT":
                                if (LoggerManagerParam.EventLoggerParam.LogDirPath.Equals(param.Value) == false)
                                {
                                    LoggerManagerParam.EventLoggerParam.LogDirPath = param.Value;
                                    ischanged = true;
                                    LoggerManager.Debug($"EventLoggerParam LogDirPath is changed to {LoggerManagerParam.EventLoggerParam.LogDirPath}");
                                }
                                break;
                            case "GPIB":
                                if (LoggerManagerParam.GpibLoggerParam.LogDirPath.Equals(param.Value) == false)
                                {
                                    LoggerManagerParam.GpibLoggerParam.LogDirPath = param.Value;
                                    ischanged = true;
                                    LoggerManager.Debug($"GpibLoggerParam LogDirPath is changed to {LoggerManagerParam.GpibLoggerParam.LogDirPath}");
                                }
                                break;
                            case "PIN":
                                if (LoggerManagerParam.PinLoggerParam.LogDirPath.Equals(param.Value) == false)
                                {
                                    LoggerManagerParam.PinLoggerParam.LogDirPath = param.Value;
                                    ischanged = true;
                                    LoggerManager.Debug($"PinLoggerParam LogDirPath is changed to {LoggerManagerParam.PinLoggerParam.LogDirPath}");
                                }
                                break;
                            case "PMI":
                                if (LoggerManagerParam.PMILoggerParam.LogDirPath.Equals(param.Value) == false)
                                {
                                    LoggerManagerParam.PMILoggerParam.LogDirPath = param.Value;
                                    ischanged = true;
                                    LoggerManager.Debug($"PMILoggerParam LogDirPath is changed to {LoggerManagerParam.PMILoggerParam.LogDirPath}");
                                }
                                break;
                            case "TEMP":
                                if (LoggerManagerParam.TempLoggerParam.LogDirPath.Equals(param.Value) == false)
                                {
                                    LoggerManagerParam.TempLoggerParam.LogDirPath = param.Value;
                                    ischanged = true;
                                    LoggerManager.Debug($"TempLoggerParam LogDirPath is changed to {LoggerManagerParam.TempLoggerParam.LogDirPath}");
                                }
                                break;
                            case "LOT":
                                if (LoggerManagerParam.LOTLoggerParam.LogDirPath.Equals(param.Value) == false)
                                {
                                    LoggerManagerParam.LOTLoggerParam.LogDirPath = param.Value;
                                    ischanged = true;
                                    LoggerManager.Debug($"LOTLoggerParam LogDirPath is changed to {LoggerManagerParam.LOTLoggerParam.LogDirPath}");
                                }
                                break;
                            case "TCPIP":
                                if (LoggerManagerParam.TCPIPLoggerParam.LogDirPath.Equals(param.Value) == false)
                                {
                                    LoggerManagerParam.TCPIPLoggerParam.LogDirPath = param.Value;
                                    ischanged = true;
                                    LoggerManager.Debug($"TCPIPLoggerParam LogDirPath is changed to {LoggerManagerParam.TCPIPLoggerParam.LogDirPath}");
                                }
                                break;
                            case "COGNEX":
                                if (LoggerManagerParam.CognexLoggerParam.LogDirPath.Equals(param.Value) == false)
                                {
                                    LoggerManagerParam.CognexLoggerParam.LogDirPath = param.Value;
                                    ischanged = true;
                                    LoggerManager.Debug($"CognexLoggerParam LogDirPath is changed to {LoggerManagerParam.CognexLoggerParam.LogDirPath}");
                                }
                                break;
                            case "EXCEPTION":
                                if (LoggerManagerParam.ExceptionLoggerParam.LogDirPath.Equals(param.Value) == false)
                                {
                                    LoggerManagerParam.ExceptionLoggerParam.LogDirPath = param.Value;
                                    ischanged = true;
                                    LoggerManager.Debug($"ExceptionLoggerParam LogDirPath is changed to {LoggerManagerParam.ExceptionLoggerParam.LogDirPath}");
                                }
                                break;
                            case "IMAGE":
                                if (LoggerManagerParam.ImageLoggerParam.LogDirPath.Equals(param.Value) == false)
                                {
                                    LoggerManagerParam.ImageLoggerParam.LogDirPath = param.Value;
                                    needInitFileManger = true;
                                    LoggerManager.Debug($"ImageLoggerParam LogDirPath is changed to {LoggerManagerParam.ImageLoggerParam.LogDirPath}");
                                }
                                break;
                            case "FilePath":
                                if (LoggerManagerParam.FilePath.Equals(param.Value) == false)
                                {
                                    LoggerManagerParam.FilePath = param.Value;
                                    ischanged = true;
                                    LoggerManager.Debug($"FilePath is changed to {LoggerManagerParam.FilePath}");
                                }
                                break;
                            case "PARAM":
                                if (LoggerManagerParam.ParamLoggerParam.LogDirPath.Equals(param.Value) == false)
                                {
                                    LoggerManagerParam.ParamLoggerParam.LogDirPath = param.Value;
                                    ischanged = true;
                                    LoggerManager.Debug($"ParamLoggerParam LogDirPath changed to {LoggerManagerParam.ParamLoggerParam.LogDirPath}");
                                }
                                break;
                            case "SOAKING":
                                if (LoggerManagerParam.SoakingLoggerParam.LogDirPath.Equals(param.Value) == false)
                                {
                                    LoggerManagerParam.SoakingLoggerParam.LogDirPath = param.Value;
                                    ischanged = true;
                                    LoggerManager.Debug($"SoakingLoggerParam LogDirPath is changed to {LoggerManagerParam.SoakingLoggerParam.LogDirPath}");
                                }
                                break;
                            case "MONITORING":
                                if (LoggerManagerParam.MonitoringLoggerParam.LogDirPath.Equals(param.Value) == false)
                                {
                                    LoggerManagerParam.MonitoringLoggerParam.LogDirPath = param.Value;
                                    ischanged = true;
                                    LoggerManager.Debug($"MonitoringLoggerParam LogDirPath is changed to {LoggerManagerParam.MonitoringLoggerParam.LogDirPath}");
                                }
                                break;

                            default:
                                break;
                        }
                    }

                    if (ischanged)
                    {
                        SaveSysParameter();

                        EventLogMg.DeInit();

                        Init();
                    }

                    if (needInitFileManger)
                    {
                        SaveSysParameter();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public static EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            bool serializeResult = false;

            string filePath = LoggerManagerParam.GetFilePath();
            string fileName = LoggerManagerParam.FileName;
            string fullPath = Path.Combine(filePath, filePath);

            fullPath = Path.Combine(fullPath, fileName);

            try
            {
                if (Directory.Exists(Path.GetDirectoryName(fullPath)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                }

                serializeResult = SerializeManager.Serialize(fullPath, LoggerManagerParam);

                if (serializeResult == false)
                {
                    RetVal = EventCodeEnum.PARAM_ERROR;

                    return RetVal;
                }
                else
                {
                    RetVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[LoggerManager] [Method = SaveSysParameter] [Error = {err}]");

                RetVal = EventCodeEnum.UNDEFINED;
            }

            return RetVal;
        }

        public static void PinLog(String msg, List<string> tag = null)
        {
            msg = $"{Alias_DebugLog}{GetTempStr()} | {msg}";

            DebugLoggerCtl?.WriteLog(msg);
            PinAlignLoggerCtl?.WriteLog(msg);

            WriteInfoLog(msg);
        }
        public static void PMILog(String msg, List<string> tag = null)
        {
            msg = $"{Alias_DebugLog}{GetTempStr()} | {msg}";

            DebugLoggerCtl?.WriteLog(msg);
            PMILoggerCtl?.WriteLog(msg);

            WriteInfoLog(msg);
        }
        public static void ActionLog(string msg, int index, ModuleLogType ModuleType, StateLogType State, List<string> tag = null)
        {
            /*cell로 부터 delegate로 전달된 lot log를 출력하기위한 함수 임 (이용도에 맞게끔만 사용해야 함)*/

            DebugLoggerCtl?.WriteLog(msg);
            LOTLoggerCtl?.WriteLog(msg);

            WriteInfoLog(msg);

            if (LOT_Cell_LoggerCtlList != null && LOT_Cell_LoggerCtlList.Count > 0 && LOT_Cell_LoggerCtlList.Count > index)
            {
                LOT_Cell_LoggerCtlList[index]?.WriteLog(msg);
            }

            if (index > 0)
            {
                LoaderMapStageProc(msg, index, ModuleType, State);
            }
        }

        public static void ActionLog(ModuleLogType ModuleType, StateLogType State, string desc, int index = 0, List<string> tag = null, bool isLoaderMap = false)
        {
            try
            {
                string subject = "";

                if (index == 0)
                {
                    subject = "LOADER";
                }
                else
                {
                    if (index > 0 && index < 10)
                    {
                        subject = $"CELL0" + index;
                    }
                    else if (index >= 10)
                    {
                        subject = $"CELL" + index;
                    }
                }

                string msg = null;
                msg = string.Format("{0,-6} | {1,-15} | {2,-6 } | {3,-40}", subject, ModuleType.ToString(), State.ToString(), desc);

                DebugLoggerCtl?.WriteLog(msg);
                LOTLoggerCtl?.WriteLog(msg);

                if (isLoaderMap)
                {
                    LoaderMapProc(msg, index, ModuleType, State);
                }

                WriteInfoLog(msg);

                if (LOT_Cell_LoggerCtlList != null && LOT_Cell_LoggerCtlList.Count > 0 && LOT_Cell_LoggerCtlList.Count > index)
                {
                    LOT_Cell_LoggerCtlList[index]?.WriteLog(msg);
                }

                if (SendActionLogToLoaderDelegate != null)
                {
                    SendActionLogToLoaderDelegate(msg, index, ModuleType, State);
                }
            }
            catch (Exception err)
            {
                Exception(err);
            }
        }

        public static void WriteInfoLog(string msg, bool temp = false)
        {
            try
            {
                if (!temp)
                {
                    // Check if the cached tempStr matches anywhere in msg
                    if (!string.IsNullOrEmpty(tempStrCache) && msg.Contains(tempStrCache))
                    {
                        msg = msg.Replace(tempStrCache, "").Trim();
                    }
                }

                if (AppDomain.CurrentDomain.FriendlyName == "ProberSystem.exe")
                {
                    int index = msg.IndexOf("|");

                    if (index >= 0)
                    {
                        msg = msg.Insert(index + 1, $" {StageMode} | {LotState} | {ModuleState} | {LotName} | {DeviceName} | {WaferID} | {ProbeCardID} |");
                    }
                    else
                    {
                        msg = $"| {StageMode} | {LotState} | {ModuleState} | {LotName} | {DeviceName} | {WaferID} | {ProbeCardID} | {msg}";
                    }
                }
                else if (AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")
                {
                    int index = msg.IndexOf("|");

                    if (index >= 0)
                    {
                        msg = msg.Insert(index + 1, $" {LoaderState} |");
                    }
                    else
                    {
                        msg = $"| {LoaderState} | {msg}";
                    }
                }

                InfoLoggerCtl?.WriteLog(msg);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public static void ParamLog(String msg, int index = 0, List<string> tag = null)
        {
            ParamLoggerCtl?.WriteLog(msg);
        }
        public static void ParamLog(String fileName, String paramName, String preValue, String curValue, int index = 0, List<string> tag = null)
        {
            try
            {
                string subject = "";

                if (index == 0)
                {
                    subject = "LOADER";
                }
                else
                {
                    if (index > 0 && index < 10)
                    {
                        subject = $"CELL0" + index;
                    }
                    else if (index >= 10)
                    {
                        subject = $"CELL" + index;
                    }
                }

                Assembly entryAssembly = Assembly.GetEntryAssembly();
                string SwVersion = string.Empty;

                if (entryAssembly != null)
                {
                    FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(entryAssembly.Location);
                    SwVersion = fvi.ProductVersion;
                }
                
                string msg = null;
                msg = String.Format("{0,-10} | {1,-6} | Pre Value: {2,-10 } --> Cur Value: {3,-10} | {4,-50} | {5,-50 } ", SwVersion, subject, preValue?.ToString(), curValue?.ToString(), paramName?.ToString(), fileName?.ToString());

                ParamLoggerCtl?.WriteLog(msg);

                if (Param_Cell_LoggerCtlList != null && Param_Cell_LoggerCtlList.Count > 0 && Param_Cell_LoggerCtlList.Count > index)
                {
                    Param_Cell_LoggerCtlList[index]?.WriteLog(msg);
                }

                if (SendParamLogToLoaderDelegate != null)
                {
                    SendParamLogToLoaderDelegate(msg, index);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public static void SetLotState(string state)
        {
            try
            {
                LotState = state;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public static void InitialModuleState(string module, string state)
        {
            try
            {
                if (ModuleStates == null)
                {
                    ModuleStates = new List<ModuleStateInfo>();
                }

                ModuleStates.Add(new ModuleStateInfo(module, state));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public static void UpdateModuleState(bool NeedStateCheck = false)
        {
            try
            {
                bool inValid = false;

                if (NeedStateCheck)
                {
                    foreach (var module in ModuleStates)
                    {
                        if (string.IsNullOrEmpty(module.State))
                        {
                            inValid = true;
                            break;
                        }
                    }
                }

                if(!inValid)
                {
                    ModuleState = string.Join("^", ModuleStates.Select(ms => stateStrMap.TryGetValue(ms.State, out var stateStr) ? stateStr : "UN").ToArray());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public static void SetModuleState(string module, string state)
        {
            try
            {
                if(ModuleStates != null)
                {
                    if(string.IsNullOrEmpty(state) == true)
                    {
                        state = "UNDEFINED";
                    }

                    var existingModuleState = ModuleStates.Find(x => x.Name == module);

                    if (existingModuleState != null)
                    {
                        existingModuleState.State = state;

                        UpdateModuleState();
                    }
                    else
                    {
                        // If the entry doesn't exist, nothing.
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public static void SetLoaderState(string state)
        {
            try
            {
                LoaderState = state;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public static void SetStageMode(string mode)
        {
            try
            {
                StageMode = mode;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public static void SetLotName(string name)
        {
            try
            {
                LotName = name;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public static void SetDeviceName(string name)
        {
            try
            {
                DeviceName = name;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public static void SetWaferID(string id)
        {
            try
            {
                WaferID = id;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public static void SetProbeCardID(string id)
        {
            try
            {
                ProbeCardID = id;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public static void SetTempInfo(double sv, double pv, double dp, double mv)
        {
            try
            {
                SV = sv;
                PV = pv;
                DP = dp;
                MV = mv;

                string msg = $"{Alias_DebugLog}{GetTempStr()}";
                WriteInfoLog(msg, true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public static void TempLog(String msg, List<string> tag = null)
        {
            msg = $"{Alias_TEMPLog} | {msg}";
            TempLoggerCtl?.WriteLog(msg);
        }

        public static void TCPIPLog(String msg)
        {
            msg = $"{Alias_TCPIPLog}{GetTempStr()} | {msg}";

            TCPIPLoggerCtl?.WriteLog(msg);
            DebugLoggerCtl?.WriteLog(msg);
        }

        public static void SmokeSensorLog(String msg)
        {
            msg = $"{Alias_SmokeSensorLog}{GetTempStr()} | {msg}";
            SmokeSensorLoggerCtl?.WriteLog(msg);            
        }

        public static void SoakingLog(String msg, bool error = false, [CallerMemberName] string callFuncNm = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int sourceLineNumber = 0)
        {
            string PrefixErrKeyword = "";

            if (error)
            {
                PrefixErrKeyword = "[Error] ";
            }

            string fileName = Path.GetFileName(filePath);
            msg = $"{Alias_SoakingPLog}{GetTempStr()} | {PrefixErrKeyword}{msg} [{fileName}({sourceLineNumber}) '{callFuncNm}']";

            SoakingLoggerCtl?.WriteLog(msg);
            DebugLoggerCtl?.WriteLog(msg);
        }

        public static void SoakingErrLog(String msg, [CallerMemberName] string callFuncNm = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int sourceLineNumber = 0)
        {
            SoakingLog(msg, true, callFuncNm, filePath, sourceLineNumber);
        }
        public static void MonitoringLog(String msg, bool error = false, [CallerMemberName] string callFuncNm = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int sourceLineNumber = 0)
        {
            try
            {
                string PrefixErrKeyword = "";
                if (error)
                    PrefixErrKeyword = "[Error] ";

                string fileName = System.IO.Path.GetFileName(filePath);
                msg = $"{Alias_MonitoringLog} | {SubAlias_MonitoringLog} | {PrefixErrKeyword}{msg}, [{fileName}({sourceLineNumber}) '{callFuncNm}']";

                MonitoringLoggerCtl?.WriteLog(msg);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public static void MonitoringErrLog(String msg, [CallerMemberName] string callFuncNm = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int sourceLineNumber = 0)
        {
            try
            {
                MonitoringLog(msg, true, callFuncNm, filePath, sourceLineNumber);
                DebugLoggerCtl?.WriteLog($"{Alias_MonitoringLog} | {SubAlias_MonitoringLog} | [Error] {msg}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public static void CompVerifyLog(String msg, bool error = false, [CallerMemberName] string callFuncNm = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int sourceLineNumber = 0)
        {
            string PrefixErrKeyword = "";

            if (error)
            {
                PrefixErrKeyword = "[Error] ";
            }

            lock (CompVerifyLogBufferLockObject)
            {
                if (CompVerifyLogBuffer.Count >= LogMaximumCount)
                {
                    int overcount = LogMaximumCount - CompVerifyLogBuffer.Count;
                    for (int i = 1; i <= overcount; i++)
                    {
                        CompVerifyLogBuffer.RemoveAt(0);
                    }
                }

                CompVerifyLogBuffer?.Add(msg);
            }

            string fileName = Path.GetFileName(filePath);
            msg = $"{Alias_CompVerifyLog}{GetTempStr()} | {PrefixErrKeyword}{msg} [{fileName}({sourceLineNumber}) '{callFuncNm}']";

            CompVerifyLoggerCtl?.WriteLog(msg);
        }
        public static void RecoveryLog(String msg, bool error = false)
        {
            string PrefixErrKeyword = "";

            if (error)
            {
                PrefixErrKeyword = "[RECOVERY ERROR]";
            }

            if (RecoveryLogBuffer == null)
            {
                RecoveryLogBuffer = new ObservableCollection<string>();
            }

            if (RecoveryLogBuffer.Count >= LogMaximumCount)
            {
                int overcount = LogMaximumCount - RecoveryLogBuffer.Count;
                for (int i = 1; i <= overcount; i++)
                {
                    RecoveryLogBuffer.RemoveAt(0);
                }
            }

            RecoveryLogBuffer?.Add(msg);

            msg = $"{Alias_RecoveryLog}{GetTempStr()} | {PrefixErrKeyword}{msg}]";

            DebugLoggerCtl?.WriteLog($"{Alias_RecoveryLog} | {msg}");
        }

        public static void GpibCommlog(long gpibFlags, int gpibComActType, string cmd, GPIBlogType logtype = GPIBlogType.GPIB)
        {
            string msg = null;

            try
            {
                msg = $"{Convert.ToString(gpibFlags, 2).PadLeft(16, '0')} | {gpibComActType.ToString().PadLeft(4)} | {cmd}";
            }

            catch (Exception err)
            {
                Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }

            GpibLoggerCtl?.WriteLog(msg);
            DebugLoggerCtl?.WriteLog($"{Alias_GPIBLog}{GetTempStr()} | {msg}");
        }
        public static void GpibErrorlog(String msg, GPIBlogType logtype = GPIBlogType.GPIB)
        {
            string eventcode = string.Empty;

            GpibLoggerCtl?.WriteLog(msg);
            DebugLoggerCtl?.WriteLog($"{Alias_GPIBLog}{GetTempStr()} | {msg}");
        }

        public static void Prolog(PrologType logtype, object EventCode, EventCodeEnum errorCodeList = EventCodeEnum.NONE, String description = null, List<string> tag = null, [CallerFilePath] string file = "", [CallerMemberName] string caller = "", [CallerLineNumber] int linenumber = 0)
        {
            /*
               ProLog event code number list (7 digit)

               Lot Process                          : 0000001 ~ 0001000
               Probing (+retest)                    : 0001001 ~ 0002000
               Wafer handling                       : 0002001 ~ 0003000
               User account                         : 0003001 ~ 0004000
               Result map saving                    : 0004001 ~ 0005000
               Result map converting                : 0005001 ~ 0006000
               Import result map                    : 0006001 ~ 0007000
               Networking(up/download/spool)        : 0007001 ~ 0008000
               PMI                                  : 0008001 ~ 0009000
               Recipe (load/save/change/delete)     : 0009001 ~ 0010000
               Cassette Load/Unload (TFC)           : 0010001 ~ 0011000
               Scan cassette                        : 0011001 ~ 0012000
               OCR                                  : 0012001 ~ 0013000
               Mark align                           : 0013001 ~ 0014000
               Wafer align                          : 0014001 ~ 0015000
               Pin align                            : 0015001 ~ 0016000
               Needle Cleaning                      : 0016001 ~ 0017000
               Soaking                              : 0017001 ~ 0018000
               Card change                          : 0018001 ~ 0019000
               Polish wafer                         : 0019001 ~ 0020000
               Needle Brush                         : 0020001 ~ 0021000
               Inking                               : 0021001 ~ 0022000
               Tilting (chuck/top plate)            : 0022001 ~ 0023000
               Temperature                          : 0023001 ~ 0024000
               Air blowing                          : 0024001 ~ 0025000
               GPIB                                 : 0025001 ~ 0026000
               GEM                                  : 0026001 ~ 0027000
               Chiller                              : 0027001 ~ 0028000

               System faults                        : 0050001 ~ 0051000

            + Log example :
                 2018-09-28 05:04:06.266 [P/Y/00016001] Register_Touch_Sensor_Position_OK       <== ProLog
                 2018-09-28 05:04:06.266 [D/Y/********] nc z up = (12,010, 20, -3024)           <== Debug Log

                    Time + [Log Type] + [Lamp color] + [event code] + log description + (#error code)

                    [Log Type]
                        G : GPIB log
                        D : Debug log
                        E : Error log
                        P : Pro log

                    [Lamp color] :
                        R : Red
                        G : Green
                        Y : Yellow

                    [First number in event code] :
                        0 : Information
                        1 : Operation alarm
                        2 : System fault

                    [event code] :
                        7 digits. It must be an unique number.

                    [Log Description]
                        - Use '_' for spacing
                        - Use upper case character for each first word
                        - Describe 'Start' for begining of process
                          (Wafer_Alignment_Start, New_Lot_Start...)
                        - Describe 'OK' and 'Finish' for normal end of process
                          (Wafer_Alignment_OK, OCR_Process_OK, Lot_Process_Finish)
                        - Describe 'Failure' and 'Abort' for abnormal end of process, have to include an error code
                          (Wafer_Alignment_Failure, OCR_Process_Failure, Lot_Process_Abort)

            */

            string msg;

            bool validEvendCode = false;
            string eventcode = string.Empty;
            string eventname = string.Empty;

            CallSiteInformation callsite = null;

            try
            {
                if (EventCode is Enum)
                {
                    int eventcodevalue = (int)EventCode;
                    eventcode = eventcodevalue.ToString("D7");
                    eventname = Enum.GetName(EventCode.GetType(), EventCode);
                }
                else
                {
                    eventcode = (string)EventCode;
                }

                if (eventcode == null || eventcode.Length != 7)
                {
                    validEvendCode = false;
                    logtype = PrologType.UNDEFINED;
                }
                else
                {
                    validEvendCode = true;
                }

                if (validEvendCode == true)
                {
                    if (errorCodeList == EventCodeEnum.NONE)
                    {
                        msg = eventname;
                    }
                    else
                    {
                        msg = eventname + "(#" + $"{errorCodeList}" + ")";
                    }
                }
                else
                {
                    msg = $"EventCode is invalid";
                    callsite = new CallSiteInformation(file, caller, linenumber);
                }

                LogEventInfo log = new LogEventInfo(LogLevel.Debug, "What is this?", msg);

                // Set Identifier
                log.Properties[LogidentifierPropertyName] = Alias_ProLog;

                // Set Type
                log.Properties[LogTypePropertyName] = logtype;

                // Set Description

                if (description == null)
                {
                    description = "Empty";
                }

                log.Properties[DescriptionPropertyName] = description;

                // Set code
                log.Properties[LogCodePropertyName] = eventcode;

                if (tag == null)
                {
                    tag = new List<string>();
                }

                // Set tag
                log.Properties[LogTagPropertyName] = tag;

                // Set EventMark
                log.Properties[NotifiedPropertyName] = false;

                if (callsite != null)
                {
                    log.SetCallerInfo("", callsite.callermembername, callsite.callerfile, callsite.callerlinenumber);
                }

                if (ProLogBuffer.Count == LogMaximumCount)
                {
                    lock (ProLogBufferLockObject)
                    {
                        ProLogBuffer.RemoveAt(0);
                    }
                }

                lock (ProLogBufferLockObject)
                {
                    ProLogBuffer.Add(log);
                }

                ProLoggerCtl?.WriteLog(msg);
                DebugLoggerCtl?.WriteLog($"{Alias_ProLog}{GetTempStr()} | {msg}");

                WriteInfoLog($"{Alias_ProLog}{GetTempStr()} | {msg}");

                if (SendMessageToLoaderDelegate != null)
                {
                    SendMessageToLoaderDelegate(msg);
                }
            }
            catch (Exception err)
            {
                Trace.WriteLineIf(GPTraceSwitch.TraceError, err);
                DebugLoggerCtl?.WriteLog($"Exception occurred while prolog message create.");
                Debugger.Break();
                throw;
            }
        }

        public static void Error(string msg, List<string> tag = null, bool isLoaderMap = false)
        {
            string eventcode = string.Empty;

            msg = $"{Alias_DebugLogError}{GetTempStr()} | {msg}";

            DebugLoggerCtl?.WriteLog(msg);

            WriteInfoLog(msg);

            if (isLoaderMap)
            {
                LoaderMapLog(msg, directCall: false);
            }
        }

        private static string LastDebugMessage = "";
        private static bool LastDebugMessageisInfo = false;
        private static bool LastDebugMessageisLoaderMap = false;
        private static int LastDebugMessageCount = 0;
        private static int MaxCount = 10;

        public static void Debug(string msg, List<string> tag = null, bool isInfo = false, bool isLoaderMap = false)
        {
            try
            {
                if (msg == LastDebugMessage)
                {
                    if (LastDebugMessageCount == int.MaxValue - 1000)
                    {
                        WriteLastDebugLog();
                        LastDebugMessageCount = 0;
                        MaxCount = 10;
                    }
                    else
                    {
                        LastDebugMessageCount++;
                        LastDebugMessageisInfo = isInfo;
                        LastDebugMessageisLoaderMap = isLoaderMap;
                    }
                }
                else
                {
                    string logMessage = "";
                    if (LastDebugMessageCount > 1)
                    {
                        WriteLastDebugLog();
                    }

                    logMessage = $"{Alias_DebugLog}{GetTempStr()} | {msg}";
                    DebugLoggerCtl?.WriteLog(logMessage);
                    if (isInfo)
                    {
                        WriteInfoLog(logMessage);
                    }

                    if (isLoaderMap)
                    {
                        LoaderMapLog(msg, directCall: false);
                    }

                    LastDebugMessage = msg;
                    LastDebugMessageCount = 1;
                    MaxCount = 10;
                }

                if (LastDebugMessageCount >= MaxCount)
                {
                    WriteLastDebugLog();
                    LastDebugMessageCount = 0;
                    if (MaxCount <= int.MaxValue / 10)
                    {
                        MaxCount *= 10;
                    }
                    else 
                    {
                        MaxCount = int.MaxValue - 999;
                    }
                }

                void WriteLastDebugLog()
                {
                    string logMessage = $"{Alias_DebugLog}{GetTempStr()} | {LastDebugMessage}";
                    if (LastDebugMessageCount >= int.MaxValue - 1000)
                    {
                        logMessage += $" (Limit reached for counting duplicate log entries)";
                    }
                    else
                    {
                        logMessage += $" (Counting duplicate log entries... {LastDebugMessageCount})";
                    }
                    DebugLoggerCtl?.WriteLog(logMessage);
                    if (LastDebugMessageisInfo)
                    {
                        WriteInfoLog(logMessage);
                    }
                    if (LastDebugMessageisLoaderMap)
                    {
                        LoaderMapLog(msg, directCall: false);
                    }
                }
            }
            catch (Exception ex)
            {
                DebugLoggerCtl?.WriteLog($"Error in Debug method: {ex.Message}");
            }
        }

        public static string GetTempStr()
        {
            string retval = string.Empty;

            try
            {
                if (SV != null && PV != null && DP != null)
                {
                    retval = $" | SV = {SV / 1.0,5:0.00}, PV = {PV / 1.0,5:0.00}, DP = {DP,5:0.00}, MV = {MV,5:0.00}";
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            // Update the cached tempStr
            tempStrCache = retval;

            return retval;
        }
        public static void Event(int occurEquipment, DateTime errorOccurTime, EventCodeEnum errorCode, string msg = null, bool write = false, bool setLog = false, EventlogType logtype = EventlogType.EVENT, List<string> tag = null, EnumProberModule? moduleType = null, string moduleStartTime = "", string imageDatasHashCode = "")
        {
            string eventcode = string.Empty;

            try
            {
                if (setLog)
                {
                    EventLogMg.SetOriginEventLogList(occurEquipment, errorOccurTime, errorCode, msg, moduleType, moduleStartTime, imageDatasHashCode);
                }

                if (write)
                {
                    msg = $"{occurEquipment} | {errorCode} | {Alias_EventLog}{GetTempStr()} | {msg}";
                    EventLoggerCtl?.WriteLog(msg);
                }
            }
            catch (Exception err)
            {
                DebugLoggerCtl?.WriteLog($"Exception occurred while Event message create. Message = {msg}");
                Debugger.Break();
                throw err;
            }
        }

        public static void Event(string msg, string description = null, EventlogType logtype = EventlogType.EVENT, List<string> tag = null)
        {
            string eventcode = string.Empty;

            try
            {
                LogEventInfo log = new LogEventInfo(LogLevel.Debug, "What is this?", msg);

                // Set Type
                log.Properties[LogTypePropertyName] = logtype;

                // Set EventMark
                log.Properties[NotifiedPropertyName] = false;

                if (EventLogBuffer.Count == LogMaximumCount)
                {
                    lock (EventLogBufferLockObject)
                    {
                        EventLogBuffer.RemoveAt(0);
                    }
                }

                lock (EventLogBufferLockObject)
                {
                    EventLogBuffer.Add(log);
                }

                msg = $"{Alias_EventLog}{GetTempStr()} | {msg}";

                EventLoggerCtl?.WriteLog(msg);
                DebugLoggerCtl?.WriteLog(msg);
            }
            catch (Exception err)
            {
                DebugLoggerCtl?.WriteLog($"Exception occurred while Event message create. Message = {msg}");
                Debugger.Break();
                throw err;
            }
        }

        public static void Exception(Exception err, string msg = null)
        {
            try
            {
                print_memoryInfo();
                string msgFormat = $"MSG : {err.Message}, Target : {err.TargetSite}, Source : {err.Source}, {Environment.NewLine} {err.StackTrace}{msg}";

                msgFormat = $"{Alias_DebugLogException}{GetTempStr()} | {msgFormat}";

                DebugDetailLoggerCtl?.WriteLog(msgFormat);
                DebugLoggerCtl?.WriteLog(msgFormat);

                WriteInfoLog(msgFormat);
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(err.Message);
                System.Diagnostics.Trace.WriteLine(e.Message);
            }
        }

        public static EventCodeEnum Deinit()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"DeinitModule() in LoggerManager");
                EventLogMg.DeInit();

                // Flush and close down internal threads and timers
                LogManager.Shutdown();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public static string GetLogDirPath(EnumLoggerType logtype)
        {
            string retval = string.Empty;

            try
            {
                switch (logtype)
                {
                    case EnumLoggerType.PROLOG:
                        retval = LoggerManagerParam.ProLoggerParam.LogDirPath;
                        break;
                    case EnumLoggerType.DEBUG:
                        retval = LoggerManagerParam.DebugLoggerParam.LogDirPath;
                        break;
                    case EnumLoggerType.EXCEPTION:
                        retval = LoggerManagerParam.ExceptionLoggerParam.LogDirPath;
                        break;
                    case EnumLoggerType.EVENT:
                        retval = LoggerManagerParam.EventLoggerParam.LogDirPath;
                        break;
                    case EnumLoggerType.PIN:
                        retval = LoggerManagerParam.PinLoggerParam.LogDirPath;
                        break;
                    case EnumLoggerType.PMI:
                        retval = LoggerManagerParam.PMILoggerParam.LogDirPath;
                        break;
                    case EnumLoggerType.SOAKING:
                        retval = LoggerManagerParam.SoakingLoggerParam.LogDirPath;
                        break;
                    case EnumLoggerType.GPIB:
                        retval = LoggerManagerParam.GpibLoggerParam.LogDirPath;
                        break;
                    case EnumLoggerType.TCPIP:
                        retval = LoggerManagerParam.TCPIPLoggerParam.LogDirPath;
                        break;
                    case EnumLoggerType.TEMP:
                        retval = LoggerManagerParam.TempLoggerParam.LogDirPath;
                        break;
                    case EnumLoggerType.LOT:
                        retval = LoggerManagerParam.LOTLoggerParam.LogDirPath;
                        break;
                    case EnumLoggerType.PARAMETER:
                        retval = LoggerManagerParam.ParamLoggerParam.LogDirPath;
                        break;
                    case EnumLoggerType.COMPVERIFY:
                        retval = LoggerManagerParam.CompVerifyLoggerParam.LogDirPath;
                        break;
                    case EnumLoggerType.MONITORING:
                        retval = LoggerManagerParam.MonitoringLoggerParam.LogDirPath;
                        break;
                    case EnumLoggerType.INFO:
                        retval = LoggerManagerParam.InfoLoggerParam.LogDirPath;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
}




