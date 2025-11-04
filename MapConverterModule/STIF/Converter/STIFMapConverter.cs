using LogModule;
using MapConverterModule.STIF;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.ResultMap;
using ProberInterfaces.ResultMap.Attributes;
using ProberInterfaces.ResultMap.Script;
using ProbingDataInterface;
using RequestCore.Formatter;
using RequestCore.Query;
using ResultMapParamObject.STIF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace MapConverterModule
{

    [MapConverterAttribute(ResultMapConvertType.STIF, null)]
    public class STIFMapConverter : MapConverterBase, INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private const int FailExtensionValue = 32;
        private const int PassExtensionValue = 32 + 128;
        private const int NotUsasbleExtensionValue = 32;
        private const int NullBinCode = 126;
        private const int RecommandedTargetDieBinCode = 125;
        
        private int TargetDieBinCode;

        private const char Separator_Tab = '\t';
        private const char Separator_Equal = '=';

        private const string units = "UNITS";

        private Encoding encoding = default(Encoding);

        public double Version { get; set; }

        private const string SignatureFormat = "WM - Vx.y - STMicroelectronics Wafer Map File";

        private STIFMapScripter STIFMapScripter
        {
            get => this.Scripter as STIFMapScripter;
        }

        private STIFMapStructure STIFMap
        {
            get => this.ResultMap as STIFMapStructure;
        }

        private List<STIFMapComponent> _STIFParameters;
        public List<STIFMapComponent> STIFParameters
        {
            get { return _STIFParameters; }
            set
            {
                if (value != _STIFParameters)
                {
                    _STIFParameters = value;
                    RaisePropertyChanged();
                }
            }
        }

        public override ResultMapConvertType ConverterType { get => ResultMapConvertType.STIF; }

        public override EventCodeEnum InitConverter(MapConverterParamBase param)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                encoding = Encoding.GetEncoding(1252);

                this.ConverterParam = param;
                this.Version = (ConverterParam as STIFMapParameter).Version.Value;

                MakeDefaultParameters();

                if (Scripter == null)
                {
                    Scripter = new STIFMapScripter();
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object GetFormattedResultMap()
        {
            object retval = null;

            try
            {
                if (STIFMap != null && STIFMap.FullMap != null)
                {
                    foreach (var line in STIFMap.FullMap)
                    {
                        retval += line.Text + Environment.NewLine;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override EventCodeEnum ConvertMapDataFromBaseMap(MapHeaderObject _header, ResultMapData _resultMap, ref object _mapfile)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                HeaderObj = _header;
                ResultMapObj = _resultMap;

                retval = this.MakeSTIFMap(Version, ref _mapfile);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public double GetVersionInLine(string data)
        {
            double retval = 0;

            int startindex = data.IndexOf('V');
            string v = data.Substring(startindex + 1, 3);

            retval = Convert.ToDouble(v);

            return retval;
        }


        public Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

        public List<char> StringToChars(string input)
        {
            List<char> retval = new List<char>();

            try
            {
                foreach (var c in input)
                {
                    retval.Add(c);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public override EventCodeEnum ConvertMapDataToBaseMap(object _mapfile, ref MapHeaderObject _header, ref ResultMapData _resultMap)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {

                List<char> charsSet = null;

                if (_mapfile is List<char>)
                {
                    charsSet = _mapfile as List<char>;
                }
                else if (_mapfile is string)
                {
                    charsSet = StringToChars((string)_mapfile);
                }

                if (charsSet != null)
                {
                    bool IsVerifyChecksum = ChecksumVerification(charsSet);

                    if (IsVerifyChecksum == true)
                    {
                        string line = string.Empty;
                        List<string> basedata = new List<string>();

                        // Line data 획득
                        var myString = new string(charsSet.ToArray());
                        if (!myString.Contains('\r'))
                        {
                            myString = myString.ToString().Replace("\n", "\r\n");
                        }
                        string[] lines = myString.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                        basedata = lines.ToList();

                        double version = GetVersionInLine(basedata[0]);

                        // Make Script
                        retval = MakeScript(version);

                        if (retval == EventCodeEnum.NONE)
                        {
                            STIFScript script = STIFMapScripter.Script as STIFScript;

                            foreach (var Componenttype in script.STIFComponentOrder)
                            {
                                STIFCOMPONENTTYPE? UsedType = null;
                                List<MapScriptElement> param = null;

                                switch (Componenttype)
                                {
                                    case STIFCOMPONENTTYPE.SIGNATURE:
                                        break;
                                    case STIFCOMPONENTTYPE.HEADER:
                                        UsedType = STIFCOMPONENTTYPE.HEADER;
                                        param = script.HeaderParameters;
                                        break;
                                    case STIFCOMPONENTTYPE.MAP:
                                        UsedType = STIFCOMPONENTTYPE.MAP;
                                        param = script.MapParameters;
                                        break;
                                    case STIFCOMPONENTTYPE.FOOTER:
                                        UsedType = STIFCOMPONENTTYPE.FOOTER;
                                        param = script.FooterParameters;
                                        break;
                                    default:
                                        break;
                                }

                                if (UsedType != null && param != null)
                                {
                                    DecodeSTIF((STIFCOMPONENTTYPE)UsedType, param, basedata);
                                }
                            }

                            // TODO : 
                            ResultMap = new STIFMapStructure();

                            if (STIFMap.FullMap == null)
                            {
                                STIFMap.FullMap = new List<MapScriptElement>();
                            }
                            else
                            {
                                STIFMap.FullMap.Clear();
                            }

                            foreach (var data in basedata)
                            {
                                MapScriptElement tmp = new MapScriptElement();
                                tmp.Text = data;

                                STIFMap.FullMap.Add(tmp);
                            }
                        }
                        else
                        {
                            LoggerManager.Error($"[STIFMapConverter], MakeSTIFMap() : Script is not created.");
                        }

                        // HEADER

                        foreach (KeyValuePair<string, string> entry in keyValuePairs)
                        {
                            // do something with entry.Value or entry.Key

                            STIFMapComponent component = _STIFParameters.Find(x => x.Key == entry.Key);

                            if (component != null)
                            {
                                string convertedval = entry.Value;
                                //object targetobj = null;
                                object convertbackobj = null;

                                if (component.Connector.ConenectMethod != null)
                                {
                                    convertbackobj = component.Connector.Converter.ConvertBack(component.Connector, entry.Value);

                                    if(convertbackobj != null)
                                    {
                                        convertedval = convertbackobj.ToString();
                                    }
                                    else
                                    {

                                    }

                                    EnumProberMapProperty key = component.Connector.ConenectMethod.PropertyName;

                                    if(key != EnumProberMapProperty.UNDEFINED)
                                    {
                                        retval = _header.SetProperty(key, convertedval);

                                        if (retval != EventCodeEnum.NONE)
                                        {
                                            LoggerManager.Error($"[STIFMapConverter], ConvertMapDataToBaseMap() : SetProperty() is faild.");
                                        }

                                        //bool IsValid = _header.PropertyDictionary.TryGetValue(key, out value);
                                        //bool IsValid = _header.PropertyDictionary.ContainsKey(key);

                                        //if (IsValid == true)
                                        //{

                                        //}
                                        //else
                                        //{
                                        //    // TODO :
                                        //}
                                    }

                                    //switch (connectmethod.ReferenceType)
                                    //{
                                    //    case EnumProberMapPropertyType.UNDEFINED:
                                    //        break;
                                    //    case EnumProberMapPropertyType.BASIC:
                                    //        targetobj = _header.BasicDataObj;
                                    //        break;
                                    //    case EnumProberMapPropertyType.CASSETTE:
                                    //        targetobj = _header.CassetteDataObj;
                                    //        break;
                                    //    case EnumProberMapPropertyType.LOT:
                                    //        targetobj = _header.LotDataObj;
                                    //        break;
                                    //    case EnumProberMapPropertyType.WAFER:
                                    //        targetobj = _header.WaferDataObj;
                                    //        break;
                                    //    case EnumProberMapPropertyType.WAFERALIGN:
                                    //        targetobj = _header.WaferAlignDataObj;
                                    //        break;
                                    //    case EnumProberMapPropertyType.MAPCONFIG:
                                    //        targetobj = _header.MapConfigDataObj;
                                    //        break;
                                    //    case EnumProberMapPropertyType.TESTRESULT:
                                    //        targetobj = _header.TestResultDataObj;
                                    //        break;
                                    //    case EnumProberMapPropertyType.TIME:
                                    //        targetobj = _header.TimeDataObj;
                                    //        break;
                                    //    default:
                                    //        break;
                                    //}

                                    //object proberpropertyobj = null;
                                    //bool IsValid = HeaderObj.ProberMapPropertiesDictionary.TryGetValue(connectmethod.PropertyName, out proberpropertyobj);

                                    //if(IsValid == true)
                                    //{
                                    //    retval = PropertyExtension.SetPropertyValue(targetobj, proberpropertyobj.ToString(), convertedval);
                                    //}

                                    //retval = PropertyExtension.SetPropertyValue(targetobj, tmp.PropertyName, convertedval);

                                }
                                else
                                {
                                    LoggerManager.Debug($"[STIFMapConverter], ForcedConvertMapDataToBaseMap() : Not used. entry name = {entry.Key}");
                                }
                            }
                        }

                        // MAP

                        char[,] mapbase = DecodeMap(basedata);
                        ASCIIMap = mapbase;
                        
                        int width, height;

                        width = _header.MapConfigData.MapSizeX;
                        height = _header.MapConfigData.MapSizeY;

                        int STIFBin, Bin = 0;

                        int FailExtensionValue = 32;
                        int PassExtensionValue = 32 + 128;
                        int NotUsasbleExtensionValue = 32;

                        var dies = this.GetParam_Wafer().GetSubsInfo().DIEs;

                        bool IsPass = false;
                        bool Unknown = false;

                        for (int j = 0; j < height; j++)
                        {
                            string OneLine = string.Empty;
                            //widthstep = (j * width);

                            for (int i = 0; i < width; i++)
                            {
                                IsPass = false;
                                Unknown = false;

                                IDeviceObject dev = dies[i, (height - 1) - j];

                                STIFBin = mapbase[i, (height - 1) - j];

                                Bin = 0;

                                // NULBC
                                if (STIFBin == NullBinCode)
                                {
                                    // TODO : 
                                    Bin = NullBinCode - NotUsasbleExtensionValue;
                                    Unknown = true;
                                }
                                // Ugly and non inkless
                                else if (STIFBin == 122 || STIFBin == 124 || STIFBin == 125)
                                {
                                    if (dev.DieType.Value == DieTypeEnum.TEST_DIE)
                                    {
                                        Bin = 0;
                                        Unknown = true;
                                    }
                                    else if (dev.DieType.Value == DieTypeEnum.SKIP_DIE)
                                    {
                                        Bin = STIFBin;
                                        Unknown = true;
                                    }
                                    else 
                                    {
                                        Bin = STIFBin - NotUsasbleExtensionValue;
                                        Unknown = true;
                                    }
                                }
                                // ???
                                else if (STIFBin == 32)
                                {
                                    Bin = 0;
                                    Unknown = true;
                                }
                                // Bad Dice
                                else if (STIFBin > 32 && STIFBin <= 121)
                                {
                                    Bin = STIFBin - FailExtensionValue;
                                }
                                else if (STIFBin == 160)
                                {
                                    Bin = STIFBin - PassExtensionValue;
                                    IsPass = true;
                                }
                                // Good Dice
                                else if (STIFBin >= 161 && STIFBin <= 232)
                                {
                                    Bin = STIFBin - PassExtensionValue;
                                    IsPass = true;
                                }
                                else
                                {
                                    // 122 <= STIFBin <= 159
                                    // STIFBin > 232

                                    Bin = 0;
                                    Unknown = true;
                                }

                                _resultMap.mBINMap.Add((long)Bin);
                                _resultMap.mMapCoordX.Add((int)dev.DieIndex.XIndex);
                                _resultMap.mMapCoordY.Add((int)dev.DieIndex.YIndex);

                                DieTypeEnum dieType = dev.DieType.Value;

                                if (dieType == DieTypeEnum.MARK_DIE)
                                {
                                    _resultMap.mStatusMap.Add((byte)TestState.MAP_STS_MARK);
                                }
                                else if (dieType == DieTypeEnum.TEST_DIE)
                                {
                                    if (Unknown == true)
                                    {
                                        IsPass = false;
                                    }

                                    byte mapResult = 0;

                                    if (Unknown == true)
                                    {
                                        // TODO : ??
                                        mapResult = (byte)TestState.MAP_STS_TEST;
                                    }
                                    else if (IsPass == true)
                                    {
                                        // PASS
                                        mapResult = (byte)TestState.MAP_STS_PASS;
                                    }
                                    else
                                    {
                                        // FAIL
                                        mapResult = (byte)TestState.MAP_STS_FAIL;
                                    }

                                    _resultMap.mStatusMap.Add(mapResult);
                                }
                                else if (dieType == DieTypeEnum.SKIP_DIE)
                                {
                                    _resultMap.mStatusMap.Add((byte)TestState.MAP_STS_SKIP);
                                }
                                else
                                {
                                    _resultMap.mStatusMap.Add((byte)TestState.MAP_STS_NOT_EXIST);
                                }

                                //mRePrbMap
                                // 0: Not Re-Probed, 1: Passed at re-probing, 
                                // 2: Failed at re-probing, 3: Perform fail (for special user)
                                _resultMap.mRePrbMap.Add(0);
                                
                                // TODO : STIF맵에 포함되어 있지 않음.
                                _resultMap.mDUTMap.Add(0);

                                _resultMap.mTestCntMap.Add(0);

                                //if(dev.TestHistory == null)
                                //{
                                //    dev.TestHistory = new List<TestHistory>();
                                //}

                                //dev.TestHistory.Add(newTestHistory);
                            }
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"[STIFMapConverter], ConvertMapDataToBaseMap() : Checksum do not match.");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public char[,] DecodeMap(List<string> basedata)
        {
            char[,] retval = null;

            try
            {
                string widthstr = string.Empty;
                string heightstr = string.Empty;

                int width = 0;
                int height = 0;

                bool isExist = keyValuePairs.TryGetValue("WMXDIM", out widthstr);

                if (isExist == true)
                {
                    width = Convert.ToInt32(widthstr);
                }

                isExist = keyValuePairs.TryGetValue("WMYDIM", out heightstr);

                if (isExist == true)
                {
                    height = Convert.ToInt32(heightstr);
                }

                List<string> mapdatas = new List<string>();

                int contcount = 0;

                for (int i = 0; i < basedata.Count; i++)
                {
                    if (basedata[i].Length == width)
                    {
                        mapdatas.Add(basedata[i]);

                        contcount++;

                        if (contcount == height)
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (mapdatas.Count > 0)
                        {
                            mapdatas.Clear();
                        }

                        contcount = 0;
                    }
                }

                retval = new char[width, height];

                var decoder = encoding.GetDecoder();
                var nextChar = new char[1];

                for (int j = 0; j < height; j++)
                {
                    //byte[] encodedbytes = encoding.GetBytes(mapdatas[j]);

                    //int i = 0;

                    //foreach (var item in encodedbytes)
                    //{
                    //    var charCount = decoder.GetChars(new[] { (byte)item }, 0, 1, nextChar, 0);

                    //    if (charCount == 0) continue;

                    //    retval[i, j] = nextChar[0];
                    //    i++;
                    //}

                    for (int i = 0; i < width; i++)
                    {
                        retval[i, height - j - 1] = mapdatas[j][i];
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void DecodeSTIF(STIFCOMPONENTTYPE type, List<MapScriptElement> scriptelementset, List<string> targetdata)
        {
            try
            {
                string key = string.Empty;
                string value = string.Empty;

                char UsedSeperator = default(char);

                switch (type)
                {
                    case STIFCOMPONENTTYPE.SIGNATURE:
                        break;
                    case STIFCOMPONENTTYPE.HEADER:
                        UsedSeperator = Separator_Tab;
                        break;
                    case STIFCOMPONENTTYPE.MAP:
                        UsedSeperator = Separator_Equal;
                        break;
                    case STIFCOMPONENTTYPE.FOOTER:
                        UsedSeperator = Separator_Tab;
                        break;
                    default:
                        break;
                }

                foreach (var element in scriptelementset)
                {
                    MakeKeyAndValueInfo(element.Text, targetdata, UsedSeperator);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void MakeKeyAndValueInfo(string comparekey, List<string> targetdata, char seperator)
        {
            try
            {
                string key = string.Empty;
                string value = string.Empty;

                string comparetxt = comparekey + seperator;
                List<string> foundlines = targetdata.FindAll(x => x.Contains(comparetxt));

                string reallines = string.Empty;

                if (foundlines.Count == 1)
                {
                    reallines = foundlines[0];
                }
                else
                {
                    bool isMatched = true;
                    int charindex = 0;

                    foreach (var candidateline in foundlines)
                    {
                        isMatched = true;
                        charindex = 0;

                        foreach (var comparechar in comparetxt)
                        {
                            if (candidateline[charindex] != comparechar)
                            {
                                isMatched = false;
                                break;
                            }

                            charindex++;
                        }

                        if (isMatched == true)
                        {
                            reallines = candidateline;
                            break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(reallines))
                {
                    string[] splitdata = reallines.Split(seperator);

                    if (splitdata.Length >= 2)
                    {
                        key = splitdata[0];
                        value = splitdata[1];

                        if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                        {
                            keyValuePairs.Add(key, value);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private List<MapScriptElement> MakeSignature(double version)
        {
            List<MapScriptElement> retval = null;

            try
            {
                retval = new List<MapScriptElement>();

                retval.Add(new MapScriptElement(SignatureFormat.Replace("x.y", version.ToString())));
                retval.Add(new MapScriptElement());

                //retval.Add(SignatureFormat.Replace("x.y", version.ToString()));
                //retval.Add("");

                LoggerManager.Debug("[STIFMapConverter], MakeSignature() : Success");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private List<MapScriptElement> EncodeHeader(List<MapScriptElement> headerinfo)
        {
            List<MapScriptElement> retval = null;

            try
            {
                retval = new List<MapScriptElement>();

                foreach (var item in headerinfo)
                {
                     string lineStr = MakeLineInfo(item.Text, Separator_Tab);

                    retval.Add(new MapScriptElement(lineStr, item.Background));
                }

                retval.Add(new MapScriptElement());

                LoggerManager.Debug("[STIFMapConverter], MakeHeader() : Success");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private void SetTargetDieBinCode()
        {
            try
            {
                if(STIFParameters != null)
                {
                    if(STIFParameters.Count > 0)
                    {
                        STIFMapComponent component = STIFParameters.Find(x => x.Key == "TARGBC");

                        if (component != null)
                        {
                            var val = component.Exeucte(HeaderObj);

                            TargetDieBinCode = Convert.ToInt32(val);
                        }
                        else
                        {
                            TargetDieBinCode = (char)RecommandedTargetDieBinCode;
                        }
                    }
                    else
                    {
                        TargetDieBinCode = (char)RecommandedTargetDieBinCode;
                    }
                }
                else
                {
                    TargetDieBinCode = (char)RecommandedTargetDieBinCode;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private bool IsTargetDie(int mx, int my)
        {
            bool retval = false;

            try
            {
                UserIndex ui = this.CoordinateManager().WMIndexConvertWUIndex(mx, my);

                var targetdie = this.GetParam_Wafer().GetPhysInfo().TargetUs.FirstOrDefault(x => x.XIndex.Value == ui.XIndex && x.YIndex.Value == ui.YIndex);

                if(targetdie != null)
                {
                    retval = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private void EncodeMap(List<MapScriptElement> target)
        {
            try
            {
                int widthcount = ResultMapObj.mMapCoordX.Max() + 1;
                int heightcount = ResultMapObj.mMapCoordY.Max() + 1;

                int WidthStep = 0;

                bool IsTestDie = false;

                ASCIIMap = new char[widthcount, heightcount];

                SetTargetDieBinCode();

                // HBIN 1 ==> 4   are for GOOD DICE
                // HBIN 5 ==> 89 are for FAIL DICE
                // HBIN 90 = EDGE DICE(z)
                // HBIN 92 = LASERMARK DICE(|)
                // HBIN 93 = back end DICE( } ) (only in case of STIF 1.3)
                // HBIN 94 = DUMMY DICE(~)

                for (int j = 0; j < heightcount; j++)
                {
                    string OneLine = string.Empty;

                    WidthStep = j * widthcount;

                    for (int i = 0; i < widthcount; i++)
                    {
                        IsTestDie = false;

                        int currentindex = WidthStep + i;

                        char asciiChar = default(char);

                        if (IsTargetDie(i, heightcount - j - 1) == true)
                        {
                            asciiChar = (char)TargetDieBinCode;
                        }
                        else
                        {
                            // Check Test die (TEST or PASS or FAIL)
                            if (ResultMapObj.mStatusMap[currentindex] == 1 ||
                                ResultMapObj.mStatusMap[currentindex] == 4 ||
                                ResultMapObj.mStatusMap[currentindex] == 5
                               )
                            {
                                IsTestDie = true;
                            }

                            if (IsTestDie == true)
                            {
                                //  | OLI BIN CODE | STIF ASCII code | Test Result |
                                //  |    0 - 89    |    32 - 121     | Bad Dice    |
                                //  |    1 - 72    |    161 - 232    | Good Dice   |

                                //  MAP_STS_NOT_EXIST = 0,    
                                //  MAP_STS_TEST = 1,         
                                //  MAP_STS_SKIP = 2,         
                                //  MAP_STS_MARK = 3,         
                                //  MAP_STS_PASS = 4,         
                                //  MAP_STS_FAIL = 5,         
                                //  MAP_STS_CUR_DIE = 6,      
                                //  MAP_STS_TEACH = 7

                                if (ResultMapObj.mStatusMap[currentindex] == 1)
                                {
                                    // +32
                                    asciiChar = (char)(ResultMapObj.mBINMap[currentindex] + NotUsasbleExtensionValue);

                                    //LoggerManager.Debug($"{i}, {j}, {(int)asciiChar}");
                                }
                                else if (ResultMapObj.mStatusMap[currentindex] == 4)
                                {
                                    // +160
                                    asciiChar = (char)(ResultMapObj.mBINMap[currentindex] + PassExtensionValue);

                                    //LoggerManager.Debug($"{i}, {j}, {(int)asciiChar}");
                                }
                                else if (ResultMapObj.mStatusMap[currentindex] == 5)
                                {
                                    // +32
                                    asciiChar = (char)(ResultMapObj.mBINMap[currentindex] + FailExtensionValue);

                                    //LoggerManager.Debug($"{i}, {j}, {(int)asciiChar}");
                                }
                            }
                            else
                            {
                                // +32
                                asciiChar = (char)(ResultMapObj.mBINMap[currentindex] + NotUsasbleExtensionValue);
                            }
                        }

                        OneLine += asciiChar;

                        ASCIIMap[i, heightcount - j - 1] = asciiChar;
                    }

                    target.Add(new MapScriptElement(OneLine));
                }

                //target.Add(new MapScriptElement()); // NO NEED
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private List<MapScriptElement> MakeMapData(List<MapScriptElement> MapInfo)
        {
            List<MapScriptElement> retval = null;

            try
            {
                retval = new List<MapScriptElement>();

                foreach (var item in MapInfo)
                {
                    string lineStr = MakeLineInfo(item.Text, Separator_Equal);

                    retval.Add(new MapScriptElement(lineStr, item.Background));
                }

                retval.Add(new MapScriptElement()); 

                EncodeMap(retval);

                LoggerManager.Debug("[STIFMapConverter], MakeMap() : Success");

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
       
        private List<MapScriptElement> EncodeFooter(List<MapScriptElement> footerinfo)
        {
            List<MapScriptElement> retval = null;

            try
            {
                retval = new List<MapScriptElement>();

                foreach (var item in footerinfo)
                {
                    string lineStr = MakeLineInfo(item.Text, Separator_Tab);

                    retval.Add(new MapScriptElement(lineStr, item.Background));
                }

                //retval.Add(new ScriptElement());

                LoggerManager.Debug("[STIFMapConverter], MakeFooter() : Success");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private string MakeLineInfo(string key, char separator)
        {
            string retval = string.Empty;

            try
            {
                STIFMapComponent component = STIFParameters.Find(x => x.Key == key);

                if (component != null)
                {
                    var val = component.Exeucte(HeaderObj);

                    retval = component.Key + separator + val;

                    if (component.UseUnit == true)
                    {
                        retval += separator + units;
                        retval += separator + component.Unitvalue + component.Unit;
                    }
                }
                else
                {
                    LoggerManager.Error($"[STIFMapConverter], GetLineData() : Component could not be configured. Key = {key}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum MakeSTIFData(ref object _mapfile)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                ResultMap = new STIFMapStructure();

                if (STIFMapScripter.Script != null)
                {
                    STIFScript script = STIFMapScripter.Script as STIFScript;

                    foreach (var order in script.STIFComponentOrder)
                    {
                        LoggerManager.Debug($"[STIFMapConverter], MakeSTIFData() : START => {order}");

                        switch (order)
                        {
                            case STIFCOMPONENTTYPE.SIGNATURE:
                                STIFMap.Signatures = MakeSignature(STIFMapScripter.GetVersion());
                                break;
                            case STIFCOMPONENTTYPE.HEADER:
                                STIFMap.Headers = EncodeHeader(script.HeaderParameters);
                                break;
                            case STIFCOMPONENTTYPE.MAP:
                                STIFMap.Maps = MakeMapData(script.MapParameters);
                                break;
                            case STIFCOMPONENTTYPE.FOOTER:
                                STIFMap.Footers = EncodeFooter(script.FooterParameters);
                                break;
                            default:
                                break;
                        }

                        LoggerManager.Debug($"[STIFMapConverter], MakeSTIFData() : {order} <= END");
                    }

                    STIFMap.MakeFullMap(script.STIFComponentOrder);

                    // CHECKSUM
                    string checksum = MakeCheckSum(STIFMap.FullMap);
                    STIFMap.AddChecksum(checksum);

                    // Format 변경
                    _mapfile = GetFormattedResultMap();

                    // TEST 
                    //bool IsVerifyChecksum = ChecksumVerification(STIFMap.FullMap);

                    retval = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum MakeScript(double version)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Scripter == null)
                {
                    Scripter = new STIFMapScripter();
                }

                STIFMapScripter.SetVersion(version);
                retval = STIFMapScripter.MakeScript();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public EventCodeEnum MakeSTIFMap(double version, ref object _mapfile)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = MakeScript(version);

                if (retval == EventCodeEnum.NONE)
                {
                    retval = MakeSTIFData(ref _mapfile);
                }
                else
                {
                    LoggerManager.Error($"[STIFMapConverter], MakeSTIFMap() : Script is not created.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        //private void WriteTestFunc()
        //{
        //    try
        //    {
        //        foreach (var item in STIFMap.FullMap)
        //        {
        //            LoggerManager.Debug($"{item}");
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //}


        public STIFMapConverter()
        {
        }

        private void MakeDefaultParameters()
        {
            try
            {
                MakeDefaultCompulsoryParameters();
                MakeDefaultOptinalParameters();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void MakeDefaultCompulsoryParameters()
        {
            try
            {
                if (_STIFParameters == null)
                {
                    _STIFParameters = new List<STIFMapComponent>();
                }

                STIFMapComponent param = null;

                param = new STIFMapComponent();
                param.Key = "LOT";
                param.Description = "Lot number (on 7 or 8 digits)";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.LOTID);
                param.Connector.InverseFormatter = new DigitFormatter(@"^[a-z][A-Z][0-9]$", 0, 15);

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);


                // SLOTNO 가 아닌 wafer id 에 있는 웨이퍼 id에 존재하는 num 이라고함...

                param = new STIFMapComponent();
                param.Key = "WAFER";
                param.Description = "Wafer number (on 2 digits)";

                //param.Connector = new MapPropertyConnector();
                //param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.SLOTNO);
                //param.Connector.InverseFormatter = new NumericFormatter("00");

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.WAFERID);
                param.Connector.InverseFormatter = new DigitFormatter("00",8,2);


                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                param = new STIFMapComponent();
                param.Key = "PRODUCT";
                param.Description = "Product name";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.DEVICENAME);

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                param = new STIFMapComponent();
                param.Key = "READER";
                param.Description = "Number read by the OCR as written on the wafer";

                param.Connector = new MapPropertyConnector();

                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.WAFERID);

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                param = new STIFMapComponent();
                param.Key = "XSTEP";
                param.Description = "X die stepping followed by the unit used";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.ALIGNEDINDEXSIZEX);

                // 1 um = 0.0393700786 mil, 1 mil = 25.4um
                // 1 um to 0.1 mil => / 2.54

                param.Connector.InverseFormatter = new UnitConverterByArg(25.4 * 0.1, "/");
                (param.Connector.InverseFormatter as UnitConverterByArg).roundenum = ROUNDENUM.TRUNCATE;

                param.Connector.ReverseFormatter = new UnitConverterByArg(25.4 * 0.1, "*");
                (param.Connector.ReverseFormatter as UnitConverterByArg).roundenum = ROUNDENUM.TRUNCATE;

                param.UseUnit = true;
                param.Unitvalue = "(0.1)";
                param.Unit = "MIL";

                _STIFParameters.Add(param);

                param = new STIFMapComponent();
                param.Key = "YSTEP";
                param.Description = "Y die stepping followed by the unit used";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.ALIGNEDINDEXSIZEY);

                // 1 um = 0.0393700786 mil, 1 mil = 25.4um
                // 1 um to 0.1 mil => / 2.54

                param.Connector.InverseFormatter = new UnitConverterByArg(25.4 * 0.1, "/");
                (param.Connector.InverseFormatter as UnitConverterByArg).roundenum = ROUNDENUM.TRUNCATE;

                param.Connector.ReverseFormatter = new UnitConverterByArg(25.4 * 0.1, "*");
                (param.Connector.ReverseFormatter as UnitConverterByArg).roundenum = ROUNDENUM.TRUNCATE;

                param.UseUnit = true;
                param.Unitvalue = "(0.1)";
                param.Unit = "MIL";

                _STIFParameters.Add(param);

                param = new STIFMapComponent();
                param.Key = "FLAT";
                param.Description = "Flat or notch orientation in degrees (0, 90, 180, 270), clockwise";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.NOTCHDIR);

                //param.Coupler.BaseDatas.Add(new ComponentNameAndValue(MAPHEADERTYPE.WAFER, "NotchAngleOffset");
                //param.Coupler.Expression = "1+2";

                param.Connector.InverseFormatter = new RequestCore.Controller.Branch()
                {
                    Condition = new EqualByArg(0),
                    Positive = new RequestCore.QueryPack.FixData() { ResultData = "270" },
                    Negative = new RequestCore.Controller.Branch()
                    {
                        Condition = new EqualByArg(90),
                        Positive = new RequestCore.QueryPack.FixData() { ResultData = "180" },
                        Negative = new RequestCore.Controller.Branch()
                        {
                            Condition = new EqualByArg(180),
                            Positive = new RequestCore.QueryPack.FixData() { ResultData = "90" },
                            Negative = new RequestCore.Controller.Branch()
                            {
                                Condition = new EqualByArg(270),
                                Positive = new RequestCore.QueryPack.FixData() { ResultData = "0" },
                                Negative = new RequestCore.QueryPack.FixData() { ResultData = null }
                            }
                        }
                    }
                };

                //param.Coupler.HeaderType = MAPHEADERTYPE.UNDEFINED;
                //param.Coupler.PropertyName = "";
                //param.Coupler.Function = GetFLATValue;

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                param = new STIFMapComponent();
                param.Key = "XREF";
                param.Description = "X distance from the wafer centre to the reference die centre, followed by the unit used";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.DISTANCEWAFERCENTERTOREFDIECENTERX);

                param.Connector.InverseFormatter = new UnitConverterByArg(25.4 * 0.1, "/");
                (param.Connector.InverseFormatter as UnitConverterByArg).roundenum = ROUNDENUM.TRUNCATE;

                param.Connector.ReverseFormatter = new UnitConverterByArg(25.4 * 0.1, "*");
                (param.Connector.ReverseFormatter as UnitConverterByArg).roundenum = ROUNDENUM.TRUNCATE;

                param.UseUnit = true;
                param.Unitvalue = "(0.1)";
                param.Unit = "MIL";

                _STIFParameters.Add(param);

                param = new STIFMapComponent();
                param.Key = "YREF";
                param.Description = "Y distance from the wafer centre to the reference die centre, followed by the unit used";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.DISTANCEWAFERCENTERTOREFDIECENTERY);

                param.Connector.InverseFormatter = new UnitConverterByArg(25.4 * 0.1, "/");
                (param.Connector.InverseFormatter as UnitConverterByArg).roundenum = ROUNDENUM.TRUNCATE;

                param.Connector.ReverseFormatter = new UnitConverterByArg(25.4 * 0.1, "*");
                (param.Connector.ReverseFormatter as UnitConverterByArg).roundenum = ROUNDENUM.TRUNCATE;

                //param.Coupler.HeaderType = MAPHEADERTYPE.MAPCONFIG;
                //param.Coupler.PropertyName = "WaferCenterToRefDieCenterDistanceY";

                param.UseUnit = true;
                param.Unitvalue = "(0.1)";
                param.Unit = "MIL";

                _STIFParameters.Add(param);

                param = new STIFMapComponent();
                param.Key = "XFRST";
                param.Description = "X index location of the reference die";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.REFDIEX);

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                param = new STIFMapComponent();
                param.Key = "YFRST";
                param.Description = "Y index location of the reference die";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.REFDIEY);

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                param = new STIFMapComponent();
                param.Key = "XSTRP";
                param.Description = "X index location of the first location of die in the wafer map (location in ther upper left)";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.FIRSTDIEX);

                
                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                param = new STIFMapComponent();
                param.Key = "YSTRP";
                param.Description = "Y index location of the first location of die in the wafer map (location in ther upper left)";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.FIRSTDIEY);

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                param = new STIFMapComponent();
                param.Key = "PRQUAD";
                param.Description = "Probing quadrant used (1,2,3,4)";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.AXISDIR);

                param.Connector.InverseFormatter = new RequestCore.Controller.Branch()
                {
                    Condition = new EqualByArg(AxisDirectionEnum.UpLeft),
                    Positive = new RequestCore.QueryPack.FixData() { ResultData = "4" },
                    Negative = new RequestCore.Controller.Branch()
                    {
                        Condition = new EqualByArg(AxisDirectionEnum.UpRight),
                        Positive = new RequestCore.QueryPack.FixData() { ResultData = "3" },
                        Negative = new RequestCore.Controller.Branch()
                        {
                            Condition = new EqualByArg(AxisDirectionEnum.DownRight),
                            Positive = new RequestCore.QueryPack.FixData() { ResultData = "2" },
                            Negative = new RequestCore.Controller.Branch()
                            {
                                Condition = new EqualByArg(AxisDirectionEnum.DownLeft),
                                Positive = new RequestCore.QueryPack.FixData() { ResultData = "1" },
                                Negative = new RequestCore.QueryPack.FixData() { ResultData = "0" }
                            }
                        }
                    }
                };

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                param = new STIFMapComponent();
                param.Key = "COQUAD";
                param.Description = "Coordinate quadrant used (1,2,3,4)";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.AXISDIR);

                param.Connector.InverseFormatter = new RequestCore.Controller.Branch()
                {
                    Condition = new EqualByArg(AxisDirectionEnum.UpLeft),
                    Positive = new RequestCore.QueryPack.FixData() { ResultData = "4" },
                    Negative = new RequestCore.Controller.Branch()
                    {
                        Condition = new EqualByArg(AxisDirectionEnum.UpRight),
                        Positive = new RequestCore.QueryPack.FixData() { ResultData = "3" },
                        Negative = new RequestCore.Controller.Branch()
                        {
                            Condition = new EqualByArg(AxisDirectionEnum.DownRight),
                            Positive = new RequestCore.QueryPack.FixData() { ResultData = "2" },
                            Negative = new RequestCore.Controller.Branch()
                            {
                                Condition = new EqualByArg(AxisDirectionEnum.DownLeft),
                                Positive = new RequestCore.QueryPack.FixData() { ResultData = "1" },
                                Negative = new RequestCore.QueryPack.FixData() { ResultData = "0" }
                            }
                        }
                    }
                };

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                param = new STIFMapComponent();
                param.Key = "DIAM";
                param.Description = "Wafer size in mils";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.WAFERSIZE);

                // 1 um = 0.0393700786 mil
                param.Connector.InverseFormatter = new UnitConverterByArg(0.0393700786, "*");
                (param.Connector.InverseFormatter as UnitConverterByArg).roundenum = ROUNDENUM.TRUNCATE;

                param.Connector.ReverseFormatter = new UnitConverterByArg(0.0393700786, "/");
                (param.Connector.ReverseFormatter as UnitConverterByArg).roundenum = ROUNDENUM.TRUNCATE;

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                param = new STIFMapComponent();
                param.Key = "NULBC";
                param.Description = "ASCII code number of the null bin code. (value is fixed to 126)";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new ConstantConnectMethod(value: "126");

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                param = new STIFMapComponent();
                param.Key = "GOODS";
                param.Description = "Quantity of good dice, computed from wafer map";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.GOODDEVICESINWAFER);

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                param = new STIFMapComponent();
                param.Key = "SETUP FILE";
                param.Description = "Access path and file name of the set up file used by prober";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.DEVICENAME);
                //param.Connector.ConenectMethod = new ConstantConnectMethod(value: "EMPTY");

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                param = new STIFMapComponent();
                param.Key = "RPSEL";
                param.Description = "Number of picking alignment reference dice";

                // TODO : NEED CHECK
                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new ConstantConnectMethod(value: "0");

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                param = new STIFMapComponent();
                param.Key = "DATE";
                param.Description = "Start probing date (YYYY-MM-DD)";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.PROBINGSTARTTIME);
                param.Connector.InverseFormatter = new DateTimeFormatter("yyMMddHHmmss", "yyyy-MM-dd");
                param.Connector.ReverseFormatter = new DateTimeFormatter("yyyy-MM-dd", "yyMMddHHmmss");

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                param = new STIFMapComponent();
                param.Key = "TIME";
                param.Description = "Start probing time (HH:mm:ss)";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.PROBINGSTARTTIME);
                param.Connector.InverseFormatter = new DateTimeFormatter("yyMMddHHmmss", "HH:mm:ss");
                param.Connector.ReverseFormatter = new DateTimeFormatter("HH:mm:ss", "yyMMddHHmmss");

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                param = new STIFMapComponent();
                param.Key = "WMXDIM";
                param.Description = "Width of the map in dice";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.MAPSIZEX);

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                param = new STIFMapComponent();
                param.Key = "WMYDIM";
                param.Description = "Height of the map in dice";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.MAPSIZEY);

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                param = new STIFMapComponent();
                param.Key = "EDATE";
                param.Description = "End probing date (YYYY-MM-DD)";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.PROBINGENDTIME);
                param.Connector.InverseFormatter = new DateTimeFormatter("yyMMddHHmmss", "yyyy-MM-dd");
                param.Connector.ReverseFormatter = new DateTimeFormatter("yyyy-MM-dd", "yyMMddHHmmss");

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                param = new STIFMapComponent();
                param.Key = "ETIME";
                param.Description = "End probing time (HH:mm:ss)";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.PROBINGENDTIME);
                param.Connector.InverseFormatter = new DateTimeFormatter("yyMMddHHmmss", "HH:mm:ss");
                param.Connector.ReverseFormatter = new DateTimeFormatter("HH:mm:ss", "yyMMddHHmmss");

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                param = new STIFMapComponent();
                param.Key = "CHECKSUM";
                param.Description = "Checksum of the whole STIF file, including this line";

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void MakeDefaultOptinalParameters()
        {
            try
            {
                if (_STIFParameters == null)
                {
                    _STIFParameters = new List<STIFMapComponent>();
                }

                STIFMapComponent param = null;

                param = new STIFMapComponent();
                param.Key = "OPERATOR";
                param.Description = "Operator identification";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.OPERATORNAME);

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                param = new STIFMapComponent();
                param.Key = "TEST SYSTEM";
                param.Description = "Tester identification";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new ConstantConnectMethod(value: "EMPTY");

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                param = new STIFMapComponent();
                param.Key = "TEST PROG";
                param.Description = "Tester program name";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new ConstantConnectMethod(value: "EMPTY");

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                param = new STIFMapComponent();
                param.Key = "PROBE CARD";
                param.Description = "Prober card used";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.CARDNAME);

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                param = new STIFMapComponent();
                param.Key = "PROBER";
                param.Description = "Prober name";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.PROBERID);

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                param = new STIFMapComponent();
                param.Key = "XFDI";
                param.Description = "X distance from the reference die centre to the FDI target center, followed by the unit used";

                param.Connector = new MapPropertyConnector();
                //param.Connector.BaseData = new ComponentSingleValue(EnumProberMapPropertyType.MAPCONFIG, "RefDieCenterToFDITargetCenterDistanceX");

                param.UseUnit = true;
                param.Unitvalue = string.Empty;
                param.Unit = "MICRONS";

                _STIFParameters.Add(param);

                param = new STIFMapComponent();
                param.Key = "YFDI";
                param.Description = "Y distance from the reference die centre to the FDI target center, followed by the unit used";

                param.Connector = new MapPropertyConnector();
                //param.Connector.BaseData = new ComponentSingleValue(EnumProberMapPropertyType.MAPCONFIG, "RefDieCenterToFDITargetCenterDistanceY");

                param.UseUnit = true;
                param.Unitvalue = string.Empty;
                param.Unit = "MICRONS";

                _STIFParameters.Add(param);

                // TODO : 리스트 필요? 안쓰이는 데이터?
                param = new STIFMapComponent();
                param.Key = "XREFPn";
                param.Description = "X index location of the reference point n";

                param.Connector = null;

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                // TODO : 리스트 필요? 안쓰이는 데이터?
                param = new STIFMapComponent();
                param.Key = "YREFPn";
                param.Description = "Y index location of the reference point n";
                param.Connector = null;
                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                // TODO : 리스트 필요?, 일단 2개 생성
                param = new STIFMapComponent();
                param.Key = "XBE TARG1";
                param.Description = "X index location of the back-end target die n";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new ConstantConnectMethod(value: "0");

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                // TODO : 리스트 필요?, 일단 2개 생성
                param = new STIFMapComponent();
                param.Key = "YBE TARG1";
                param.Description = "Y index location of the back-end target die n";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new ConstantConnectMethod(value: "0");

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                // TODO : 리스트 필요?, 일단 2개 생성
                param = new STIFMapComponent();
                param.Key = "XBE TARG2";
                param.Description = "X index location of the back-end target die n";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new ConstantConnectMethod(value: "0");

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                // TODO : 리스트 필요?, 일단 2개 생성
                param = new STIFMapComponent();
                param.Key = "YBE TARG2";
                param.Description = "Y index location of the back-end target die n";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new ConstantConnectMethod(value: "0");

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                param = new STIFMapComponent();
                param.Key = "TARGBC";
                param.Description = "ASCII code for the target dices (recommended value is 125)";

                // TODO : NEED CHECK
                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new ConstantConnectMethod(value: "125");

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                // 안쓰이는 데이터?
                param = new STIFMapComponent();
                param.Key = "EWS ID";
                param.Description = "EWS Flow id, indicated by EWS1, EWS2 etc. (only EWS internal)";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new ConstantConnectMethod(value: "EMPTY");

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                // 안쓰이는 데이터?
                param = new STIFMapComponent();
                param.Key = "UNSUREBC";
                param.Description = "ASCII code number of dice that are not systematically reliable in therm of alignment";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new ConstantConnectMethod(value: "EMPTY");

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                // NEED CHECK
                param = new STIFMapComponent();
                param.Key = "OLIPATH";
                param.Description = "Network path for the OLI server";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new ConstantConnectMethod(value: "EMPTY");

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                // NEED CHECK
                param = new STIFMapComponent();
                param.Key = "OLIFORMAT";
                param.Description = "Format of the OLI maps";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new ConstantConnectMethod(value: "EMPTY");

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                param = new STIFMapComponent();
                param.Key = "MERGEDATE";
                param.Description = "Data of the merge operation on the wafer map file";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new FunctionConnectMethod(NowDateTime);
                param.Connector.InverseFormatter = new DateTimeFormatter("yyMMddhhmmss", "yyyy-MM-dd");
                param.Connector.ReverseFormatter = new DateTimeFormatter("yyyy-MM-dd", "yyMMddhhmmss");

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                param = new STIFMapComponent();
                param.Key = "MERGETIME";
                param.Description = "Time of the merge operation on the wafer map file";

                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new FunctionConnectMethod(NowDateTime);
                param.Connector.InverseFormatter = new DateTimeFormatter("yyMMddhhmmss", "HH:mm:ss");
                param.Connector.ReverseFormatter = new DateTimeFormatter("HH:mm:ss", "yyMMddhhmmss");

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);

                param = new STIFMapComponent();
                param.Key = "PROD ID";
                param.Description = "For multiproduct, \"1\" for the first product tester, \"2\" for thje second product tested etc.";

                // 안쓰이는 데이터?
                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new ConstantConnectMethod(value: "EMPTY");

                param.UseUnit = false;
                param.Unitvalue = string.Empty;
                param.Unit = string.Empty;

                _STIFParameters.Add(param);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override object GetResultMap()
        {
            object retval = null;

            try
            {
                if (STIFMap != null)
                {
                    retval = STIFMap.FullMap;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override char[,] GetASCIIMap()
        {
            return ASCIIMap;
        }



        //string GetHeaderValueUsingConstantValue(MapHeaderObject header, ComponentCoupler coupler)
        //{
        //    string retval = string.Empty;

        //    try
        //    {
        //        retval = coupler.ConstantValue;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        

        
        
        bool ChecksumVerification(List<char> Datas)
        {
            bool retval = false;

            try
            {
                int CheckSum = 0;
                int count = 0;

                foreach (var d in Datas)
                {
                    if (d != 13 && d != 10)
                    {
                        CheckSum = CheckSum + d;
                        CheckSum = CheckSum * 16;
                        CheckSum = CheckSum % 251;
                    }

                    count++;
                }

                if (CheckSum == 0)
                {
                    retval = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        bool ChecksumVerification(List<MapScriptElement> fullmap)
        {
            bool retval = false;

            // Checksum code verification

            //  BEGIN
            //      CheckSum = 0;
            //      //STIF File reading
            //      FOR all lines DO
            //          FOR each character of the line without CR &LF DO
            //              CheckSum = CheckSum + ORD(Character);
            //              CheckSum = CheckSum * 16;
            //              CheckSum = CheckSum MOD 251;
            //          DONE
            //      DONE

            //      IF CheckSum different from 0 THEN
            //          Notify Checksum error
            //      END
            //  END

            try
            {
                int CheckSum = 0;
                int count = 0;

                foreach (var item in fullmap)
                {
                    byte[] encodedbytes = encoding.GetBytes(item.Text);

                    foreach (var by in encodedbytes)
                    {
                        if (by != 13 && by != 10)
                        {
                            CheckSum = CheckSum + by;
                            CheckSum = CheckSum * 16;
                            CheckSum = CheckSum % 251;
                        }

                        count++;
                    }
                }

                if (CheckSum == 0)
                {
                    retval = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        string MakeCheckSum(List<MapScriptElement> fullmap)
        {
            string retval = string.Empty;

            int CheckSum = 0;

            try
            {
                // Checksum code generator
                // ORD is a function giving the ordinal number of a character(in ASCII code);
                // MOD is a function giving the entire remainder of a division(Ex: 10 Mod 3 = 1)
                // DIV is a function giving the entire division(Ex: 10 div 3 = 3)
                // CHAR is a function giving a character from its ASCII code

                // BEGIN

                //      CheckSum = 0; //CheckSUM initialisation
                //      //STIF File reading without the last line (checksum line)
                //      FOR all lines without the checksum line DO
                //          FOR each character of the line without CR &LF DO
                //              CheckSum = CheckSum + ORD(Character);
                //              CheckSum = CheckSum * 16;
                //              CheckSum = CheckSum MOD 251;
                //          DONE
                //      DONE

                //      FOR each character of “CHECKSUM<TAB> AA” DO
                //          CheckSum = CheckSum + ORD(Character);
                //          IF Not(Last Character) THEN // Not for the last “A” character
                //              CheckSum = CheckSum * 16;
                //              CheckSum = CheckSum MOD 251;
                //      DONE

                //      // Checksum Character definition
                //      IF CheckSum different from 0 THEN
                //          CheckSum = 251 - CheckSum
                //      END

                //      CSChar1 = CHAR(65 + CheckSum DIV 16); // 1st character of the checksum
                //      CSChar2 = CHAR(65 + CheckSum MOD 16); // 2nd character of the checksum
                // END

                // TODO : CHECKSUM<TAB> AA... CHECK => Length = 11

                string ChecksumConst = "CHECKSUM" + '\t' + "AA";

                char[] CSChar = new char[2];

                byte[] encodedbytes = null;

                foreach (var item in fullmap)
                {
                    encodedbytes = encoding.GetBytes(item.Text);

                    foreach (var by in encodedbytes)
                    {
                        if (by != 13 && by != 10)
                        {
                            CheckSum = CheckSum + by;
                            CheckSum = CheckSum * 16;
                            CheckSum = CheckSum % 251;
                        }
                    }
                }

                encodedbytes = encoding.GetBytes(ChecksumConst);

                for (int i = 0; i < encodedbytes.Length; i++)
                {
                    CheckSum = CheckSum + encodedbytes[i];

                    if (i != encodedbytes.Length - 1)
                    {
                        CheckSum = CheckSum * 16;
                    }

                    CheckSum = CheckSum % 251;
                }

                if (CheckSum != 0)
                {
                    CheckSum = 251 - CheckSum;
                }

                CSChar[0] = (char)(65 + CheckSum / 16);
                CSChar[1] = (char)(65 + CheckSum % 16);

                retval = new string(CSChar);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        #region FUNCTION

        string NowDateTime()
        {
            string retval = string.Empty;

            try
            {
                string timeFormat = "yyMMddhhmmss";

                retval = DateTime.Now.ToLocalTime().ToString(timeFormat);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        //string ConvertWaferSizeData(MapHeaderObject header, ComponentCoupler coupler)
        //{
        //    string retval = string.Empty;

        //    try
        //    {
        //        // um to mils
        //        // 1 um = 0.0393700786 mil

        //        double umtomil = 0.0393700786;

        //        retval = Math.Truncate(HeaderObj.WaferDataObj.WaferSize * umtomil).ToString();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        //string ConvertWaferSlotNum(MapHeaderObject header, ComponentCoupler coupler)
        //{
        //    string retval = string.Empty;

        //    try
        //    {
        //        string convertedSlotNum = header.WaferDataObj.WaferSlotNum.ToString("00");

        //        retval = convertedSlotNum;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}


        //string ConvertPRQUADData(MapHeaderObject header, ComponentCoupler coupler)
        //{
        //    string retval = string.Empty;

        //    try
        //    {
        //        // 1 => 4
        //        // 2 => 3
        //        // 3 => 2
        //        // 4 => 1

        //        byte? convertedDir = null;

        //        switch (HeaderObj.MapConfigDataObj.MapDir)
        //        {
        //            case 1:
        //                convertedDir = 4;
        //                break;
        //            case 2:
        //                convertedDir = 3;
        //                break;
        //            case 3:
        //                convertedDir = 2;
        //                break;
        //            case 4:
        //                convertedDir = 1;
        //                break;
        //            default:
        //                break;
        //        }

        //        if (convertedDir == null)
        //        {
        //            LoggerManager.Error($"[STIFMapConverter], ConvertPRQUADData() : Direction value is wrong. Input value = {HeaderObj.MapConfigDataObj.MapDir}");
        //        }

        //        retval = convertedDir.ToString();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        //string ConvertDateTimeData(MapHeaderObject header, ComponentCoupler coupler)
        //{
        //    string retval = string.Empty;

        //    try
        //    {
        //        if (!string.IsNullOrEmpty(coupler.DataFormat))
        //        {
        //            if (coupler.PropValue != null)
        //            {
        //                DateTime dtDate = DateTime.ParseExact(coupler.PropValue.ToString(), "yyMMddhhmmFF", null);
        //                retval = dtDate.ToString(coupler.DataFormat);
        //            }
        //            else
        //            {
        //                LoggerManager.Error($"[STIFMapConverter], ConvertDateTimeData() : value is not defined.");
        //            }
        //        }
        //        else
        //        {
        //            retval = coupler.PropValue.ToString();
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        //string GetFLATValue(MapHeaderObject header, ComponentCoupler coupler)
        //{
        //    string retval = string.Empty;

        //    try
        //    {
        //        // 0이 FRONT
        //        // 0   => 270
        //        // 90  => 180
        //        // 180 => 90
        //        // 270 => 0

        //        // TODO : Offset으로 처리하는 것이 맞는가?

        //        int FLATDir = header.WaferDataObj.FlatNotchDir + header.WaferDataObj.NotchAngleOffset;
        //        int? ConvertedDir = null;

        //        switch (FLATDir)
        //        {
        //            case 0:
        //                ConvertedDir = 270;
        //                break;
        //            case 90:
        //                ConvertedDir = 180;
        //                break;
        //            case 180:
        //                ConvertedDir = 90;
        //                break;
        //            case 270:
        //                ConvertedDir = 0;
        //                break;
        //            default:
        //                break;
        //        }

        //        if (ConvertedDir != null)
        //        {
        //            retval = ConvertedDir.ToString();
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        #endregion
    }
}

