using LogModule;
using MapConverterModule.E142.Component;
using MapConverterModule.E142.Script;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.ResultMap;
using ProberInterfaces.ResultMap.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MapConverterModule.E142.Converter
{
    using MapConverterModule.E142.Attribute;
    using MapConverterModule.E142.Format;
    using ProberInterfaces.Utility;
    using RequestCore.Query;

    [MapConverterAttribute(ResultMapConvertType.E142, typeof(MapDataType))]
    public class E142Converter : MapConverterBase, INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private const string NullBinCode = "FF";

        private E142Scripter E142MapScripter
        {
            get => this.Scripter as E142Scripter;
        }

        private MapDataType E142Map
        {
            get => this.ResultMap as MapDataType;
        }

        private E142Script E142Script
        {
            get => this.Scripter.Script as E142Script;
        }

        public override ResultMapConvertType ConverterType { get => ResultMapConvertType.E142; }
        private List<E142MapComponent> e142MapComponents { get; set; }

        public override EventCodeEnum ConvertMapDataFromBaseMap(MapHeaderObject _header, ResultMapData _resultMap, ref object _mapfile)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                HeaderObj = _header;
                ResultMapObj = _resultMap;

                retval = E142MapScripter.MakeScript();

                if (retval == EventCodeEnum.NONE)
                {
                    retval = MakeMapData();
                }

                if (retval == EventCodeEnum.NONE)
                {
                    _mapfile = ResultMap;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum MakeMapData()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                ResultMap = new MapDataType();

                retval = AssignLayouts();

                retval = AssignSubstrates();

                retval = AssignSubstrateMaps();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private E142MapComponent GetComponent(string Identifier, string key)
        {
            E142MapComponent retval = null;

            try
            {
                retval = e142MapComponents.FirstOrDefault(x => x.Identifier == Identifier && x.Key == key);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private EventCodeEnum ExecuteComponent(object targetobj, string Identifier, string key)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                E142MapComponent component = null;

                component = GetComponent(Identifier, key);

                if (component != null)
                {
                    var val = component.Exeucte(HeaderObj);

                    retval = PropertyExtension.SetChildPropertyValue(targetobj, key, val);
                }
                else
                {
                    LoggerManager.Error($"[E142Converter], ExecuteComponent() : Can't find component. Identifier = {Identifier}, Key = {key}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum FieldsSet(object targetobj, string Identifier, IE142Field e142Field)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                string key = string.Empty;
                bool IsChildTurn = false;


                List<ChildInfo> childinfos = null;
                ChildInfo CurrentChildInfo = null;

                if (e142Field is IHasChild)
                {
                    childinfos = (e142Field as IHasChild).childInfos;
                }

                IEnumerator enumerator = e142Field.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    IsChildTurn = false;

                    Enum e = enumerator.Current as Enum;

                    if (e != null)
                    {
                        key = EnumExtensions.GetFullName(e);

                        var attribute = EnumExtensions.GetAttribute<E142FieldAttribute>(e) as E142FieldAttribute;

                        if (attribute != null && attribute.IsChild == true)
                        {
                            CurrentChildInfo = childinfos.FirstOrDefault(x => x.ChildEnum.Equals(e));

                            if (CurrentChildInfo != null)
                            {
                                IsChildTurn = true;
                            }
                            else
                            {
                                // TODO : 
                            }
                        }

                        if (IsChildTurn == true)
                        {
                            if (attribute.NeedAction == false)
                            {
                                key = string.Empty;
                            }

                            var newarray = MakeArrayFields(CurrentChildInfo.TargetArrayType, CurrentChildInfo.Fields, Identifier, key);

                            Type targettype = targetobj.GetType();
                            PropertyInfo[] props = targettype.GetProperties();

                            foreach (var prop in props)
                            {
                                if (prop.PropertyType == CurrentChildInfo.TargetArrayType)
                                {
                                    prop.SetValue(targetobj, newarray, null);

                                    break;
                                }
                            }
                        }
                        else
                        {
                            retval = ExecuteComponent(targetobj, Identifier, key);

                            if (retval != EventCodeEnum.NONE)
                            {
                                // TODO : ERROR
                                break;
                            }
                        }
                    }
                    else
                    {
                        // ???
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object MakeArrayFields(Type targettype, List<IE142Field> fields, string identifier, string key = null)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            object retval = null;

            try
            {
                Array values = null;

                if (string.IsNullOrEmpty(key) == true)
                {
                    int index = 0;

                    bool IsArray = typeof(Array).IsAssignableFrom(targettype);

                    if (IsArray == true)
                    {
                        Type elementtype = targettype.GetElementType();

                        values = Array.CreateInstance(elementtype, fields.Count);

                        foreach (var field in fields)
                        {
                            var newitem = Activator.CreateInstance(fields[index].AssignType);

                            identifier = field.Identifier;

                            LoggerManager.Debug($"Create Type = {fields[index].AssignType}");

                            ret = FieldsSet(newitem, identifier, field);

                            values.SetValue(newitem, index);

                            index++;
                        }
                    }
                    else
                    {
                        /// ???
                    }
                }
                else
                {
                    var component = GetComponent(identifier, key);

                    if (component != null)
                    {
                        values = component.Exeucte(HeaderObj) as Array;
                    }
                }

                //if (values != null && values.Length > 0)
                //{
                //    Type genericListType = typeof(List<>);
                //    Type concreteListType = genericListType.MakeGenericType(targettype);

                //    retval = Activator.CreateInstance(concreteListType, new object[] { values });
                //}

                if (values != null && values.Length > 0)
                {
                    retval = values;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private object EncodeBinDefinitions()
        {
            //Type targettype, List< IE142Field > fields, string Identifier

            BinDefinitionType[] retval = null;

            try
            {
                // TEST CODE : Duumy 데이터 변경 필요
                BinDefinitions DummyBindDefinitions = new BinDefinitions();

                DummyBindDefinitions.MakeDummyDataSet();

                int BinDefinitionCount = DummyBindDefinitions.binDefinitions.Count;

                retval = new BinDefinitionType[BinDefinitionCount];

                BinDefinitionType tmpBinDefinition = null;

                for (int i = 0; i < BinDefinitionCount; i++)
                {
                    tmpBinDefinition = new BinDefinitionType();

                    tmpBinDefinition.BinCode = DummyBindDefinitions.binDefinitions[i].binCodeField;
                    tmpBinDefinition.BinCount = DummyBindDefinitions.binDefinitions[i].binCountField;
                    tmpBinDefinition.BinQuality = DummyBindDefinitions.binDefinitions[i].binQualityField;
                    tmpBinDefinition.BinDescription = DummyBindDefinitions.binDefinitions[i].binDescriptionField;

                    retval[i] = tmpBinDefinition;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        /// <summary>
        /// Binning is the process of assigning a bin code to each device on the wafer. 
        /// Devices may be optically inspected and/or electrically tested. 
        /// Some are completely rejected. 
        /// Those that pass may be assigned grades based on the electrical test.
        /// </summary>
        /// <returns></returns>
        /// 
        private object Binning()
        {
            #region Description & Example
            /*
             * Blue: Null Bin | Cyan: Edge Bin (or Skip)| magenta: good/pass | yellow:fail
             * 
             * bin code - the character or number assigned. Some maps use a single ASCII character; others may use a HEX value (i.e. FF) and others may use a single digit or 3 digit bin label.
             * null bin - since the array is rectangular and the wafer is round, in order to keep track of the proper location null or empty positions are assigned using the null bin code. This is often the dot character; for SINF the null code is two consecutive underscores __. There are certain types of map encoding that do not require a null bincode.
             * reference bin - a reference die (also known as an alignment die) is one that the machine processing the wafer uses to properly align with the wafer map. It may or may not be a "good" die. Often it is visually distinct (and hence not a "good" die).
             * mirror bin - a mirror die is one that is highly reflective -- the equipment is able to find the mirror die from a full wafer view. Mirror die are often placed adjacent to a reference die.
             * ugly bin - an ugly die is typically one which may not be complete because it was either damaged during processing or is located in a region where it has been truncated.
             * edge bin - an edge die is one near the periphery of the wafer and is rejected because processing near the edge is not consistent.
             * fail bin - any die binned as fail will not be "picked" for further processing.
             * pass bin - a die which has been tested and found to be in spec - it will be picked.
             * first die - some maps identify the "first" die - i.e. the one that was probed first - whether it is good or bad.
             * skip die - for one reason or another, the probe machine has been instructed to skip testing of this die. Some maps may assign SKIP die the same bin code as NULL die.
             */

            // Example
            // <Dimension X="26" Y="28"/>
            /*
             * <BinCode>FFFFFFFFFFFFFFFFFFFEFEFEFEFEFEFEFFFFFFFFFFFFFFFFFFFF</BinCode> => Col (52)
               <BinCode>FFFFFFFFFFFFFEFE010900010101010101FEFEFFFFFFFFFFFFFF</BinCode>
               <BinCode>FFFFFFFFFFFEFE010101010101010101010000FEFFFFFFFFFFFF</BinCode>
               <BinCode>FFFFFFFFFE010101010101010101000001000000FEFFFFFFFFFF</BinCode>
               <BinCode>FFFFFFFE0101010101010101010100000001000000FEFFFFFFFF</BinCode>
               <BinCode>FFFFFE01010101010106010101000001000000000000FEFFFFFF</BinCode>
               <BinCode>FFFEFE0101010101010101010100000001000000000001FEFFFF</BinCode>
               <BinCode>FFFE010106010101010101000000000000000001000100FEFFFF</BinCode>
               <BinCode>FF0101010101010101010100000001000000000001000100FEFF</BinCode>
               <BinCode>FE1E01010101010101010100001000000000001000100001FEFF</BinCode>
               <BinCode>FE010101010101010101010000000100010000000D013B01FEFF</BinCode>
               <BinCode>0139010101010101102C000000000100010001000101010100FF</BinCode>
               <BinCode>091001010101010101010000000000010001000108013A0100FE</BinCode>
               <BinCode>3A0101010101010101010000000001000100010001012C0100FC</BinCode>
               <BinCode>01010101010101010101000000000001000101010101010100FC</BinCode>
               <BinCode>01010101010101080101000000000100000010000101010100FE</BinCode>
               <BinCode>01010101010101010101000000000001000001010106350100FF</BinCode>
               <BinCode>FE0101010101010101010000000000010000000100100101FEFF</BinCode>
               <BinCode>FE0101010114010101010100000000010100000101010101FEFF</BinCode>
               <BinCode>FFFE010601010101010100000000000001000000010001FEFEFF</BinCode>
               <BinCode>FFFE010101010101010600010000070001010001010109FEFFFF</BinCode>
               <BinCode>FFFFFE0101010101010101000000000100010000000100FEFFFF</BinCode>
               <BinCode>FFFFFE01010101010101010000000001010101000100FEFFFFFF</BinCode>
               <BinCode>FFFFFFFE0101010101010101000013001400010100FEFFFFFFFF</BinCode>
               <BinCode>FFFFFFFFFE010101010101010000000001010101FEFFFFFFFFFF</BinCode>
               <BinCode>FFFFFFFFFFFEFE010101010101010100000000FEFFFFFFFFFFFF</BinCode>
               <BinCode>FFFFFFFFFFFFFFFEFE010101010109000EFEFEFFFFFFFFFFFFFF</BinCode>
               <BinCode>FFFFFFFFFFFFFFFFFFFEFEFEFEFEFEFEFFFFFFFFFFFFFFFFFFFF</BinCode>
               => lines (28)
             */

            #endregion

            BinCodeType[] retval = null;

            try
            {
                int widthcount = ResultMapObj.mMapCoordX.Max() + 1;
                int heightcount = ResultMapObj.mMapCoordY.Max() + 1;

                retval = new BinCodeType[heightcount];

                int WidthStep = 0;
                BinCodeType tmpBinCode = null;

                for (int j = 0; j < heightcount; j++)
                {
                    string OneLine = string.Empty;

                    WidthStep = j * widthcount;
                    tmpBinCode = new BinCodeType();

                    string hexValue = string.Empty;

                    for (int i = 0; i < widthcount; i++)
                    {
                        int currentindex = WidthStep + i;

                        hexValue = ResultMapObj.mBINMap[currentindex].ToString("X2");

                        OneLine += hexValue;
                    }

                    tmpBinCode.Value = OneLine;
                    retval[j] = tmpBinCode;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private object DeviceIds()
        {
            DeviceIdType[] retval = null;

            try
            {
                long TestDieCount = HeaderObj.TestResultData.TestDieCnt;

                retval = new DeviceIdType[TestDieCount];

                if (TestDieCount > 0)
                {
                    int WidthStep = 0;
                    DeviceIdType deviceIdType = null;

                    int widthcount = ResultMapObj.mMapCoordX.Max() + 1;
                    int heightcount = ResultMapObj.mMapCoordY.Max() + 1;

                    int testdieindex = 0;

                    for (int j = 0; j < heightcount; j++)
                    {
                        string OneLine = string.Empty;

                        WidthStep = j * widthcount;

                        for (int i = 0; i < widthcount; i++)
                        {
                            int currentindex = WidthStep + i;

                            if (ResultMapObj.mStatusMap[currentindex] == 1 ||
                                ResultMapObj.mStatusMap[currentindex] == 4 ||
                                ResultMapObj.mStatusMap[currentindex] == 5
                               )
                            {
                                deviceIdType = new DeviceIdType();

                                deviceIdType.X = ResultMapObj.mMapCoordX[currentindex].ToString();
                                deviceIdType.Y = ResultMapObj.mMapCoordY[currentindex].ToString();

                                if (ResultMapObj.mStatusMap[currentindex] == 4)
                                {
                                    deviceIdType.Value = "pass";
                                }
                                else if (ResultMapObj.mStatusMap[currentindex] == 5)
                                {
                                    deviceIdType.Value = "fail";
                                }
                                else
                                {
                                    // TODO : 테스트가 진행되지 않은 테스트 다이
                                }

                                // TODO : 추후, sme:Die 데이터 형태로 전환 시.
                                // 아래의 데이터를 이용할 것.

                                // ResultMapObj.mInked[currentindex]
                                // ResultMapObj.mXWaferCenterDistance[currentindex]
                                // ResultMapObj.mYWaferCenterDistance[currentindex]
                                // ResultMapObj.mOverdrive[currentindex]
                                // ResultMapObj.mTestStartTime[currentindex]
                                // ResultMapObj.mFailMarkInspection[currentindex]
                                // ResultMapObj.mNeedleMarkInspection[currentindex]
                                // ResultMapObj.mNeedleCleaning[currentindex]
                                // ResultMapObj.mNeedleAlign[currentindex]

                                retval[testdieindex] = deviceIdType;

                                testdieindex++;
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }



        private EventCodeEnum AssignLayouts()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                List<LayoutType> layouts = new List<LayoutType>();

                foreach (var layoutfield in E142Script.mapDataField.layouts)
                {
                    LayoutType layoutType = new LayoutType();
                    string Identifier = layoutfield.Identifier;

                    retval = FieldsSet(layoutType, Identifier, layoutfield);

                    if (retval != EventCodeEnum.NONE)
                    {
                        // TODO : ERROR
                        break;
                    }
                    else
                    {
                        layouts.Add(layoutType);
                    }
                }

                E142Map.Layouts = layouts.ToArray();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private EventCodeEnum AssignSubstrates()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                List<SubstrateType> Substrates = new List<SubstrateType>();

                foreach (var substratefield in E142Script.mapDataField.substrates)
                {
                    SubstrateType substrateType = new SubstrateType();
                    string Identifier = substratefield.Identifier;

                    retval = FieldsSet(substrateType, Identifier, substratefield);

                    if (retval != EventCodeEnum.NONE)
                    {
                        // TODO : ERROR
                        break;
                    }
                    else
                    {
                        Substrates.Add(substrateType);
                    }
                }

                E142Map.Substrates = Substrates.ToArray();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private EventCodeEnum AssignSubstrateMaps()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                List<SubstrateMapType> SubstrateMaps = new List<SubstrateMapType>();

                foreach (var substratemapfield in E142Script.mapDataField.substrateMaps)
                {
                    SubstrateMapType substratemapType = new SubstrateMapType();
                    string Identifier = substratemapfield.Identifier;

                    retval = FieldsSet(substratemapType, Identifier, substratemapfield);

                    if (retval != EventCodeEnum.NONE)
                    {
                        // TODO : ERROR
                        break;
                    }
                    else
                    {
                        SubstrateMaps.Add(substratemapType);
                    }
                }

                E142Map.SubstrateMaps = SubstrateMaps.ToArray();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        //private string[,] DecodeMap()
        //{
        //    string[,] retval = null;

        //    try
        //    {
                
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        public override EventCodeEnum ConvertMapDataToBaseMap(object _mapfile, ref MapHeaderObject _header, ref ResultMapData _resultMap)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (_header == null)
                {
                    _header = new MapHeaderObject();
                }

                if (_resultMap == null)
                {
                    _resultMap = new ResultMapData();
                }

                // TODO : 
                MapDataType mapDataType = _mapfile as MapDataType;

                LayoutType layout = mapDataType.Layouts.ToList().FirstOrDefault(x => x.LayoutId == "WaferLayout");

                layout = mapDataType.Layouts.ToList().FirstOrDefault(x => x.LayoutId == "Devices");

                _header.MapConfigData.MapSizeX = Convert.ToInt32(layout.Dimension.X);
                _header.MapConfigData.MapSizeX = Convert.ToInt32(layout.Dimension.Y);

                _header.MapConfigData.RefDieCoordX = Convert.ToInt32(layout.LowerLeft.X);
                _header.MapConfigData.RefDieCoordY = Convert.ToInt32(layout.LowerLeft.Y);

                //if (_mapfile is List<char>)
                //{
                //    charsSet = _mapfile as List<char>;
                //}
                //else if (_mapfile is string)
                //{
                //    charsSet = StringToChars((string)_mapfile);
                //}

                //if (charsSet != null)
                //{
                //    bool IsVerifyChecksum = ChecksumVerification(charsSet);

                //    if (IsVerifyChecksum == true)
                //    {
                //        int startpos = 0;
                //        string line = string.Empty;
                //        List<string> basedata = new List<string>();

                //        // Line data 획득
                //        for (int i = 0; i <= charsSet.Count; i++)
                //        {
                //            if (i + 1 <= charsSet.Count)
                //            {
                //                char tmpc1 = charsSet[i];
                //                char tmpc2 = charsSet[i + 1];

                //                if (tmpc1 == 13 && tmpc2 == 10)
                //                {
                //                    int linelength = i - 1 - startpos;

                //                    for (int j = 0; j <= linelength; j++)
                //                    {
                //                        line += charsSet[startpos + j];
                //                    }

                //                    startpos = i + 2;
                //                    i = startpos - 1;

                //                    if (!string.IsNullOrEmpty(line))
                //                    {
                //                        basedata.Add(line);

                //                        line = string.Empty;
                //                    }
                //                }
                //            }
                //        }

                //        double version = GetVersionInLine(basedata[0]);

                //        // Make Script
                //        retval = MakeScript(version);

                //        if (retval == EventCodeEnum.NONE)
                //        {
                //            STIFScript script = STIFMapScripter.Script as STIFScript;

                //            foreach (var Componenttype in script.STIFComponentOrder)
                //            {
                //                STIFCOMPONENTTYPE? UsedType = null;
                //                List<MapScriptElement> param = null;

                //                switch (Componenttype)
                //                {
                //                    case STIFCOMPONENTTYPE.SIGNATURE:
                //                        break;
                //                    case STIFCOMPONENTTYPE.HEADER:
                //                        UsedType = STIFCOMPONENTTYPE.HEADER;
                //                        param = script.HeaderParameters;
                //                        break;
                //                    case STIFCOMPONENTTYPE.MAP:
                //                        UsedType = STIFCOMPONENTTYPE.MAP;
                //                        param = script.MapParameters;
                //                        break;
                //                    case STIFCOMPONENTTYPE.FOOTER:
                //                        UsedType = STIFCOMPONENTTYPE.FOOTER;
                //                        param = script.FooterParameters;
                //                        break;
                //                    default:
                //                        break;
                //                }

                //                if (UsedType != null && param != null)
                //                {
                //                    DecodeSTIF((STIFCOMPONENTTYPE)UsedType, param, basedata);
                //                }
                //            }
                //        }
                //        else
                //        {
                //            LoggerManager.Error($"[STIFMapConverter], MakeSTIFMap() : Script is not created.");
                //        }

                //        // HEADER

                //        foreach (KeyValuePair<string, string> entry in keyValuePairs)
                //        {
                //            // do something with entry.Value or entry.Key

                //            STIFMapComponent component = _STIFParameters.Find(x => x.Key == entry.Key);

                //            if (component != null)
                //            {
                //                ComponentNameAndValueBase tmp = null;
                //                string convertedval = entry.Value;
                //                object targetobj = null;
                //                object convertbackobj = null;

                //                if (component.Connector.BaseData != null)
                //                {
                //                    tmp = component.Connector.BaseData;

                //                    convertbackobj = component.Connector.Converter.ConvertBack(component.Connector, entry.Value);
                //                    convertedval = convertbackobj.ToString();

                //                    switch (tmp.ReferenceType)
                //                    {
                //                        case ENUMMAPDATAREFERENCE.UNDEFINED:
                //                            break;
                //                        case ENUMMAPDATAREFERENCE.BASIC:
                //                            targetobj = _header.BasicDataObj;
                //                            break;
                //                        case ENUMMAPDATAREFERENCE.CASSETTE:
                //                            targetobj = _header.CassetteDataObj;
                //                            break;
                //                        case ENUMMAPDATAREFERENCE.LOT:
                //                            targetobj = _header.LotDataObj;
                //                            break;
                //                        case ENUMMAPDATAREFERENCE.WAFER:
                //                            targetobj = _header.WaferDataObj;
                //                            break;
                //                        case ENUMMAPDATAREFERENCE.WAFERALIGN:
                //                            targetobj = _header.WaferAlignDataObj;
                //                            break;
                //                        case ENUMMAPDATAREFERENCE.MAPCONFIG:
                //                            targetobj = _header.MapConfigDataObj;
                //                            break;
                //                        case ENUMMAPDATAREFERENCE.TESTRESULT:
                //                            targetobj = _header.TestResultDataObj;
                //                            break;
                //                        case ENUMMAPDATAREFERENCE.TIME:
                //                            targetobj = _header.TimeDataObj;
                //                            break;
                //                        default:
                //                            break;
                //                    }

                //                    retval = PropertyExtension.SetPropertyValue(targetobj, tmp.PropertyName, convertedval);
                //                }
                //                else
                //                {
                //                    LoggerManager.Debug($"[STIFMapConverter], ForcedConvertMapDataToBaseMap() : Not used. entry name = {entry.Key}");
                //                }
                //            }
                //        }

                //        // MAP

                //        char[,] mapbase = DecodeMap(basedata);

                //        int width, height, widthstep;

                //        width = _header.MapConfigDataObj.MapSizeX;
                //        height = _header.MapConfigDataObj.MapSizeY;

                //        int STIFBin, Bin = 0;

                //        int FailExtensionValue = 32;
                //        int PassExtensionValue = 32 + 128;
                //        int NotUsasbleExtensionValue = 32;

                //        var dies = this.GetParam_Wafer().GetSubsInfo().DIEs;

                //        for (int j = 0; j < height; j++)
                //        {
                //            string OneLine = string.Empty;

                //            //widthstep = (j * width);

                //            for (int i = 0; i < width; i++)
                //            {
                //                IDeviceObject dev = dies[i, (height - 1) - j];

                //                STIFBin = mapbase[i, (height - 1) - j];

                //                Bin = 0;

                //                // NULBC
                //                if (STIFBin == NullBinCode)
                //                {
                //                    // TODO : 
                //                    Bin = NullBinCode - NotUsasbleExtensionValue;
                //                }
                //                // Ugly and non inkless
                //                else if (STIFBin == 122 || STIFBin == 124 || STIFBin == 125)
                //                {
                //                    if (dev.DieType.Value == DieTypeEnum.TEST_DIE)
                //                    {
                //                        Bin = 0;
                //                    }
                //                    else if (dev.DieType.Value == DieTypeEnum.SKIP_DIE)
                //                    {
                //                        Bin = STIFBin;
                //                    }
                //                    else
                //                    {
                //                        Bin = STIFBin - NotUsasbleExtensionValue;
                //                    }
                //                }
                //                // ???
                //                else if (STIFBin == 32)
                //                {
                //                    Bin = 0;
                //                }
                //                // Bad Dice
                //                else if (STIFBin > 32 && STIFBin <= 121)
                //                {
                //                    Bin = STIFBin - FailExtensionValue;
                //                }
                //                else if (STIFBin == 160)
                //                {
                //                    Bin = STIFBin - PassExtensionValue;
                //                }
                //                // Good Dice
                //                else if (STIFBin >= 161 && STIFBin <= 232)
                //                {
                //                    Bin = STIFBin - PassExtensionValue;
                //                }
                //                else
                //                {
                //                    // 122 <= STIFBin <= 159
                //                    // STIFBin > 232

                //                    Bin = 0;
                //                }

                //                _resultMap.mBINMap.Add((long)Bin);
                //                _resultMap.mMapCoordX.Add((int)dev.DieIndex.XIndex);
                //                _resultMap.mMapCoordY.Add((int)dev.DieIndex.YIndex);

                //                //mRePrbMap
                //                // 0: Not Re-Probed, 1: Passed at re-probing, 
                //                // 2: Failed at re-probing, 3: Perform fail (for special user)
                //                _resultMap.mRePrbMap.Add(0);

                //                // TODO : STIF맵에 포함되어 있지 않음.
                //                _resultMap.mDUTMap.Add(0);

                //                _resultMap.mTestCntMap.Add(0);
                //            }
                //        }
                //    }
                //    else
                //    {
                //        LoggerManager.Debug($"[STIFMapConverter], ConvertMapDataToBaseMap() : Checksum do not match.");
                //    }
                //}

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override char[,] GetASCIIMap()
        {
            throw new NotImplementedException();
        }

        public override object GetResultMap()
        {
            return ResultMap;
        }

        public override EventCodeEnum InitConverter(MapConverterParamBase param)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                this.ConverterParam = param;

                if (Scripter == null)
                {
                    Scripter = new E142Scripter();
                }

                retval = MakeComponent();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private EventCodeEnum MakeComponent()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (this.e142MapComponents == null)
                {
                    this.e142MapComponents = new List<E142MapComponent>();
                }

                E142MapComponent param = null;
                MultipleConnectMethod multicomponent = null;

                string Identifier = string.Empty;

                #region Layout - WaferLayout

                Identifier = "WaferLayout";

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(LayoutFieldType.LAYOUTID);
                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new ConstantConnectMethod(value: "WaferLayout");
                this.e142MapComponents.Add(param);

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(LayoutFieldType.DEFAULTUNITS);
                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new ConstantConnectMethod(value: "mm");
                this.e142MapComponents.Add(param);

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(LayoutFieldType.TOPLEVEL);
                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new ConstantConnectMethod(value: "true");
                this.e142MapComponents.Add(param);

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(LayoutFieldType.DIMENSION);
                param.Connector = new MapPropertyConnector();

                multicomponent = new MultipleConnectMethod();
                multicomponent.Customtype = typeof(LogicalCoordinatesType);
                multicomponent.SpecificPropertyConnectMethods.Add(new SpecificPropertyConnectMethod("X", new PropertyConnectMethod(EnumProberMapProperty.MAPSIZEX)));
                multicomponent.SpecificPropertyConnectMethods.Add(new SpecificPropertyConnectMethod("Y", new PropertyConnectMethod(EnumProberMapProperty.MAPSIZEY)));
                param.Connector.ConenectMethod = multicomponent;
                this.e142MapComponents.Add(param);

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(LayoutFieldType.DEVICESIZE);
                param.Connector = new MapPropertyConnector();

                // TODO : WAFERDATA & "WaferSize"
                multicomponent = new MultipleConnectMethod();
                multicomponent.Customtype = typeof(PhysicalCoordinatesType);
                multicomponent.SpecificPropertyConnectMethods.Add(new SpecificPropertyConnectMethod("X", new ConstantConnectMethod(value: "1")));
                multicomponent.SpecificPropertyConnectMethods.Add(new SpecificPropertyConnectMethod("Y", new ConstantConnectMethod(value: "1")));
                param.Connector.ConenectMethod = multicomponent;
                this.e142MapComponents.Add(param);

                #region ChildLayout - ChildLayout

                Identifier = "ChildLayout";

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(ChildLayoutFieldType.LAYOUTID);
                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new ConstantConnectMethod(value: "Devices");
                this.e142MapComponents.Add(param);

                #endregion

                #endregion

                #region Layout - Devices

                Identifier = "Devices";

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(LayoutFieldType.LAYOUTID);
                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new ConstantConnectMethod(value: "Devices");
                this.e142MapComponents.Add(param);

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(LayoutFieldType.DEFAULTUNITS);
                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new ConstantConnectMethod(value: "micron");
                this.e142MapComponents.Add(param);

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(LayoutFieldType.DIMENSION);
                param.Connector = new MapPropertyConnector();

                multicomponent = new MultipleConnectMethod();
                multicomponent.Customtype = typeof(LogicalCoordinatesType);
                multicomponent.SpecificPropertyConnectMethods.Add(new SpecificPropertyConnectMethod("X", new PropertyConnectMethod(EnumProberMapProperty.MAPSIZEX)));
                multicomponent.SpecificPropertyConnectMethods.Add(new SpecificPropertyConnectMethod("Y", new PropertyConnectMethod(EnumProberMapProperty.MAPSIZEY)));
                param.Connector.ConenectMethod = multicomponent;
                this.e142MapComponents.Add(param);

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(LayoutFieldType.LOWERLEFT);
                param.Connector = new MapPropertyConnector();

                multicomponent = new MultipleConnectMethod();
                multicomponent.Customtype = typeof(PhysicalCoordinatesType);
                multicomponent.SpecificPropertyConnectMethods.Add(new SpecificPropertyConnectMethod("X", new PropertyConnectMethod(EnumProberMapProperty.REFDIEX)));
                multicomponent.SpecificPropertyConnectMethods.Add(new SpecificPropertyConnectMethod("Y", new PropertyConnectMethod(EnumProberMapProperty.REFDIEY)));
                param.Connector.ConenectMethod = multicomponent;
                this.e142MapComponents.Add(param);

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(LayoutFieldType.DEVICESIZE);
                param.Connector = new MapPropertyConnector();
                
                multicomponent = new MultipleConnectMethod();
                multicomponent.Customtype = typeof(PhysicalCoordinatesType);
                multicomponent.SpecificPropertyConnectMethods.Add(new SpecificPropertyConnectMethod("X", new PropertyConnectMethod(EnumProberMapProperty.ALIGNEDINDEXSIZEX)));
                multicomponent.SpecificPropertyConnectMethods.Add(new SpecificPropertyConnectMethod("Y", new PropertyConnectMethod(EnumProberMapProperty.ALIGNEDINDEXSIZEY)));
                param.Connector.ConenectMethod = multicomponent;
                this.e142MapComponents.Add(param);

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(LayoutFieldType.STEPSIZE);
                param.Connector = new MapPropertyConnector();

                multicomponent = new MultipleConnectMethod();
                multicomponent.Customtype = typeof(PhysicalCoordinatesType);
                multicomponent.SpecificPropertyConnectMethods.Add(new SpecificPropertyConnectMethod("X", new PropertyConnectMethod(EnumProberMapProperty.STEPSIZEX)));
                multicomponent.SpecificPropertyConnectMethods.Add(new SpecificPropertyConnectMethod("Y", new PropertyConnectMethod(EnumProberMapProperty.STEPSIZEY)));
                param.Connector.ConenectMethod = multicomponent;
                this.e142MapComponents.Add(param);

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(LayoutFieldType.PRODUCTID);
                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.DEVICENAME);
                this.e142MapComponents.Add(param);

                #endregion

                #region Substrate - Wafer

                Identifier = SubstrateTypeEnum.Wafer.ToString();

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(SubstrateFieldType.SUBSTRATETYPE1);
                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new ConstantConnectMethod(value: SubstrateTypeEnum.Wafer);
                this.e142MapComponents.Add(param);

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(SubstrateFieldType.SUBSTRATEID);
                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.WAFERID);
                this.e142MapComponents.Add(param);

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(SubstrateFieldType.LOTID);
                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.LOTID);
                this.e142MapComponents.Add(param);

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(SubstrateFieldType.CARRIERTYPE);
                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new ConstantConnectMethod(value: "Cassette");
                this.e142MapComponents.Add(param);

                // TODO : 데이터 할당 확인
                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(SubstrateFieldType.CARRIERID);
                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.CASSETTENO);
                this.e142MapComponents.Add(param);

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(SubstrateFieldType.SLOTNUMBER);
                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.SLOTNO);
                this.e142MapComponents.Add(param);

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(SubstrateFieldType.SUBSTRATENUMBER);
                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.SLOTNO);
                this.e142MapComponents.Add(param);

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(SubstrateFieldType.GOODDEVICES);
                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.GOODDEVICESINWAFER);
                this.e142MapComponents.Add(param);

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(SubstrateFieldType.SUPPLIERNAME);
                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new ConstantConnectMethod(value: "STM");
                this.e142MapComponents.Add(param);

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(SubstrateFieldType.STATUS);
                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new ConstantConnectMethod(value: "generated");
                this.e142MapComponents.Add(param);

                #endregion

                #region SubstrateMaps - Wafer

                Identifier = SubstrateTypeEnum.Wafer.ToString();

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(SubstrateMapFieldType.SUBSTRATETYPE);
                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new ConstantConnectMethod(value: SubstrateTypeEnum.Wafer);
                this.e142MapComponents.Add(param);

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(SubstrateMapFieldType.SUBSTRATEID);
                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.WAFERID);
                this.e142MapComponents.Add(param);

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(SubstrateMapFieldType.LAYOUTSPECIFIER);
                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new ConstantConnectMethod(value: "WaferLayout/Devices");
                this.e142MapComponents.Add(param);

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(SubstrateMapFieldType.SUBSTRATESIDE);
                param.Connector = new MapPropertyConnector();

                // TODO : 변환기 제작 필, Branch 이용
                // SubstrateSideEnum.TopSide
                // SubstrateSideEnum.BottomSide
                // 기준값 확인 필요.

                param.Connector.ConenectMethod = new ConstantConnectMethod(value: SubstrateSideEnum.TopSide);
                this.e142MapComponents.Add(param);

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(SubstrateMapFieldType.ORIENTATION);
                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.NOTCHDIR);

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
                this.e142MapComponents.Add(param);

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(SubstrateMapFieldType.ORIGINLOCATION);
                param.Connector = new MapPropertyConnector();

                // TODO : 계산이 필요할 듯?
                // LowerLeft에 해당하는 값이 웨이퍼의 센터를 기준으로 하는 4분면의 몇 사분면에 해당하는지.
                // 디폴트 값을 할당해 놓음. (3사분면)
                param.Connector.ConenectMethod = new ConstantConnectMethod(value: OriginLocationEnum.LowerLeft);
                this.e142MapComponents.Add(param);

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(SubstrateMapFieldType.AXISDIRECTION);
                param.Connector = new MapPropertyConnector();

                param.Connector.ConenectMethod = new PropertyConnectMethod(EnumProberMapProperty.AXISDIR);

                param.Connector.InverseFormatter = new RequestCore.Controller.Branch()
                {
                    Condition = new EqualByArg(AxisDirectionEnum.UpLeft),
                    Positive = new RequestCore.QueryPack.FixData() { ResultData = AxisDirectionEnum.UpLeft.ToString() },
                    Negative = new RequestCore.Controller.Branch()
                    {
                        Condition = new EqualByArg(AxisDirectionEnum.UpRight),
                        Positive = new RequestCore.QueryPack.FixData() { ResultData = AxisDirectionEnum.UpRight.ToString() },
                        Negative = new RequestCore.Controller.Branch()
                        {
                            Condition = new EqualByArg(AxisDirectionEnum.DownRight),
                            Positive = new RequestCore.QueryPack.FixData() { ResultData = AxisDirectionEnum.DownRight.ToString() },
                            Negative = new RequestCore.Controller.Branch()
                            {
                                Condition = new EqualByArg(AxisDirectionEnum.DownLeft),
                                Positive = new RequestCore.QueryPack.FixData() { ResultData = AxisDirectionEnum.DownLeft.ToString() },
                                Negative = new RequestCore.QueryPack.FixData() { ResultData = "0" }
                            }
                        }
                    }
                };
                this.e142MapComponents.Add(param);

                #region Overlay

                Identifier = "Overlay";

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(OverlayFieldType.MAPNAME);
                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new ConstantConnectMethod(value: "HARD BIN MAP");
                this.e142MapComponents.Add(param);

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(OverlayFieldType.MAPVERSION);
                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new ConstantConnectMethod(value: "EWST1");
                this.e142MapComponents.Add(param);

                #region REFERENCEDEVICE - OriginLocation

                Identifier = "OriginLocation";

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(ReferenceDeviceFieldType.NAME);
                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new ConstantConnectMethod(value: "OriginLocation");
                this.e142MapComponents.Add(param);

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(ReferenceDeviceFieldType.COORDINATES);
                param.Connector = new MapPropertyConnector();

                // TODO : 실 데이터 할당.
                multicomponent = new MultipleConnectMethod();
                multicomponent.Customtype = typeof(LogicalCoordinatesType);
                multicomponent.SpecificPropertyConnectMethods.Add(new SpecificPropertyConnectMethod("X", new ConstantConnectMethod(value: "1")));
                multicomponent.SpecificPropertyConnectMethods.Add(new SpecificPropertyConnectMethod("Y", new ConstantConnectMethod(value: "1")));
                param.Connector.ConenectMethod = multicomponent;
                this.e142MapComponents.Add(param);

                #endregion

                #region REFERENCEDEVICE - ReferenceDie

                Identifier = "ReferenceDie";

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(ReferenceDeviceFieldType.NAME);
                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new ConstantConnectMethod(value: "ReferenceDie");
                this.e142MapComponents.Add(param);

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(ReferenceDeviceFieldType.COORDINATES);
                param.Connector = new MapPropertyConnector();

                // TODO : 실 데이터 할당.
                multicomponent = new MultipleConnectMethod();
                multicomponent.Customtype = typeof(LogicalCoordinatesType);
                multicomponent.SpecificPropertyConnectMethods.Add(new SpecificPropertyConnectMethod("X", new ConstantConnectMethod(value: "1")));
                multicomponent.SpecificPropertyConnectMethods.Add(new SpecificPropertyConnectMethod("Y", new ConstantConnectMethod(value: "1")));
                param.Connector.ConenectMethod = multicomponent;
                this.e142MapComponents.Add(param);

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(ReferenceDeviceFieldType.POSITION);
                param.Connector = new MapPropertyConnector();

                // TODO : 실 데이터 할당.
                multicomponent = new MultipleConnectMethod();
                multicomponent.Customtype = typeof(PhysicalCoordinatesType);
                multicomponent.SpecificPropertyConnectMethods.Add(new SpecificPropertyConnectMethod("X", new ConstantConnectMethod(value: "1")));
                multicomponent.SpecificPropertyConnectMethods.Add(new SpecificPropertyConnectMethod("Y", new ConstantConnectMethod(value: "1")));
                multicomponent.SpecificPropertyConnectMethods.Add(new SpecificPropertyConnectMethod("Units", new ConstantConnectMethod(value: "micron")));
                param.Connector.ConenectMethod = multicomponent;
                this.e142MapComponents.Add(param);

                #endregion

                #region REFERENCEDEVICE - CenterDie

                Identifier = "CenterDie";

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(ReferenceDeviceFieldType.NAME);
                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new ConstantConnectMethod(value: "CenterDie");
                this.e142MapComponents.Add(param);

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(ReferenceDeviceFieldType.COORDINATES);
                param.Connector = new MapPropertyConnector();

                // TODO : 실 데이터 할당.
                multicomponent = new MultipleConnectMethod();
                multicomponent.Customtype = typeof(LogicalCoordinatesType);
                multicomponent.SpecificPropertyConnectMethods.Add(new SpecificPropertyConnectMethod("X", new ConstantConnectMethod(value: "1")));
                multicomponent.SpecificPropertyConnectMethods.Add(new SpecificPropertyConnectMethod("Y", new ConstantConnectMethod(value: "1")));
                param.Connector.ConenectMethod = multicomponent;
                this.e142MapComponents.Add(param);

                #endregion

                #region BinCodeMap

                Identifier = "BinCodeMap";

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(BinCodeMapFieldType.BINTYPE);
                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new ConstantConnectMethod(value: BinTypeEnum.HexaDecimal);
                this.e142MapComponents.Add(param);

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(BinCodeMapFieldType.NULLBIN);
                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new ConstantConnectMethod(value: NullBinCode);
                this.e142MapComponents.Add(param);

                #region BinDefinitions

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(BinCodeMapFieldType.BINDEFINITIONS);
                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new FunctionConnectMethod(EncodeBinDefinitions);
                this.e142MapComponents.Add(param);

                #endregion

                #region BinCode

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(BinCodeMapFieldType.BINCODE);
                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new FunctionConnectMethod(Binning);
                this.e142MapComponents.Add(param);

                #endregion

                #endregion

                #region DeviceMap

                Identifier = "DeviceMap";

                param = new E142MapComponent();
                param.Identifier = Identifier;
                param.Key = EnumExtensions.GetFullName(DeviceIdMapFieldType.DEVICEID);
                param.Connector = new MapPropertyConnector();
                param.Connector.ConenectMethod = new FunctionConnectMethod(DeviceIds);
                this.e142MapComponents.Add(param);

                #endregion

                #endregion

                #endregion

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    public enum DummyBinQuailty
    {
        [Description("Fail")]
        FAIL,
        [Description("Pass")]
        PASS,
        [Description("Ugly")]
        UGLY,
        [Description("Null")]
        NULL
    }

    public enum DummyBinDescriptiopn
    {
        [Description("SKIP")]
        SKIP,
        [Description("GOOD")]
        GOOD,
        [Description("BAD")]
        BAD,
        [Description("SKIP GOOD")]
        SKIPGOOD,
        [Description("MARKING WINDOW")]
        MARKINGWINDOW,
        [Description("BACK END TARGET DIE")]
        BACKENDTARGETDIE,
        [Description("UGLY")]
        UGLY,
        [Description("OUT OF WAFER")]
        OUTOFWAFER
    }

    public class BinDefinitions
    {
        public List<BinDefinition> binDefinitions { get; set; }

        public BinDefinitions()
        {
            this.binDefinitions = new List<BinDefinition>();
        }

        private BinDefinition MakeDefinition(string BinCode, string BinCount, string BinQuality, string BinDescription)
        {
            BinDefinition retval = null;

            try
            {
                retval = new BinDefinition();

                retval.binCodeField = BinCode;
                retval.binCountField = BinCount;
                retval.binQualityField = BinQuality;
                retval.binDescriptionField = BinDescription;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void MakeDummyDataSet()
        {
            try
            {
                if (this.binDefinitions == null)
                {
                    this.binDefinitions = new List<BinDefinition>();
                }

                //for (int i = 0; i < count; i++)
                //{
                //    BinDefinition tmpBinDefinition = new BinDefinition();

                //    tmpBinDefinition.binCodeField = "00";
                //    tmpBinDefinition.binCountField = "0";
                //    tmpBinDefinition.binQualityField = EnumExtensions.GetDescription(DummyBinQuailty.FAIL);
                //    tmpBinDefinition.binDescriptionField = EnumExtensions.GetDescription(DummyBinDescriptiopn.BACKENDTARGETDIE);

                //    this.binDefinitions.Add(tmpBinDefinition);
                //}

                #region TEST SET

                this.binDefinitions.Add(MakeDefinition(BinCode: "00", BinCount: "0", BinQuality: "Skip", BinDescription: "SKIP"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "01", BinCount: "456", BinQuality: "Pass", BinDescription: "GOOD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "02", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "03", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "04", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "05", BinCount: "203", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "06", BinCount: "6", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "07", BinCount: "27", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "08", BinCount: "3", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "09", BinCount: "6", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "0A", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "0B", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "0C", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "0D", BinCount: "1", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "0E", BinCount: "1", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "0F", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "10", BinCount: "7", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "11", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "12", BinCount: "1", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "13", BinCount: "4", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "14", BinCount: "8", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "15", BinCount: "1", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "16", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "17", BinCount: "1", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "18", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "19", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "1A", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "1B", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "1C", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "1D", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "1E", BinCount: "1", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "1F", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "20", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "21", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "22", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "23", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "24", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "25", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "26", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "27", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "28", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "29", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "2A", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "2B", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "2C", BinCount: "3", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "2D", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "2E", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "2F", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "30", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "31", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "32", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "33", BinCount: "1", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "34", BinCount: "1", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "35", BinCount: "1", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "36", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "37", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "38", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "39", BinCount: "2", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "3A", BinCount: "5", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "3B", BinCount: "1", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "3C", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "3D", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "3E", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "3F", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "40", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "41", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "42", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "43", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "44", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "45", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "46", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "47", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "48", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "49", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "4A", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "4B", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "4C", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "4D", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "4E", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "4F", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "50", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "51", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "52", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "53", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "54", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "55", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "56", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "57", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "58", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "59", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "5A", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "5B", BinCount: "0", BinQuality: "Pass", BinDescription: "SKIP GOOD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "5C", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "5D", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "5E", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "5F", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "60", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "61", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "62", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "63", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "64", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "65", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "66", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "67", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "68", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "69", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "6A", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "6B", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "6C", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "6D", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "6E", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "6F", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "70", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "71", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "72", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "73", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "74", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "75", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "76", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "77", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "78", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "79", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "7A", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "7B", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "7C", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "7D", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "7E", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "7F", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "80", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "81", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "82", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "83", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "84", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "85", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "86", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "87", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "88", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "89", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "8A", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "8B", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "8C", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "8D", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "8E", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "8F", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "90", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "91", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "92", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "93", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "94", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "95", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "96", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "97", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "98", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "99", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "9A", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "9B", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "9C", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "9D", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "9E", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "9F", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "A0", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "A1", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "A2", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "A3", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "A4", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "A5", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "A6", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "A7", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "A8", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "A9", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "AA", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "AB", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "AC", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "AD", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "AE", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "AF", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "B0", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "B1", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "B2", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "B3", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "B4", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "B5", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "B6", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "B7", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "B8", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "B9", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "BA", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "BB", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "BC", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "BD", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "BE", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "BF", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "C0", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "C1", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "C2", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "C3", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "C4", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "C5", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "C6", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "C7", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "C8", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "C9", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "CA", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "CB", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "CC", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "CD", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "CE", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "CF", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "D0", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "D1", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "D2", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "D3", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "D4", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "D5", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "D6", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "D7", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "D8", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "D9", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "DA", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "DB", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "DC", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "DD", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "DE", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "DF", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "E0", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "E1", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "E2", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "E3", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "E4", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "E5", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "E6", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "E7", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "E8", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "E9", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "EA", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "EB", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "EC", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "ED", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "EE", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "EF", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "F0", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "F1", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "F2", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "F3", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "F4", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "F5", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "F6", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "F7", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "F8", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "F9", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "FA", BinCount: "0", BinQuality: "Fail", BinDescription: "BAD"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "FB", BinCount: "0", BinQuality: "Ugly", BinDescription: "SKIP UGLY"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "FC", BinCount: "2", BinQuality: "Ugly", BinDescription: "MARKING WINDOW"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "FD", BinCount: "0", BinQuality: "Ugly", BinDescription: "BACKEND TARGET DIE"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "FE", BinCount: "63", BinQuality: "Ugly", BinDescription: "UGLY"));
                this.binDefinitions.Add(MakeDefinition(BinCode: "FF", BinCount: "152", BinQuality: "Null", BinDescription: "OUT OF WAFER"));

                #endregion

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
    }

    public class BinDefinition
    {
        private string _binCodeField;
        public string binCodeField
        {
            get { return _binCodeField; }
            set
            {
                if (value != _binCodeField)
                {
                    _binCodeField = value;
                }
            }
        }

        private string _binCountField;
        public string binCountField
        {
            get { return _binCountField; }
            set
            {
                if (value != _binCountField)
                {
                    _binCountField = value;
                }
            }
        }

        private string _binQualityField;
        public string binQualityField
        {
            get { return _binQualityField; }
            set
            {
                if (value != _binQualityField)
                {
                    _binQualityField = value;
                }
            }
        }

        private string _binDescriptionField;
        public string binDescriptionField
        {
            get { return _binDescriptionField; }
            set
            {
                if (value != _binDescriptionField)
                {
                    _binDescriptionField = value;
                }
            }
        }
    }
}

