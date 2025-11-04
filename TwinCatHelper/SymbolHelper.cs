using LogModule;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Serialization;
using TwinCAT.Ads;

namespace TwinCatHelper
{
    #region SymbolHelper
    public class SymbolHelper
    {
        private TcAdsClient tcClient;
        private SymbolMap SymBolMap;


        public Dictionary<ADSSymbol, int> MachineStatusSymbolDict = new Dictionary<ADSSymbol, int>();
        public Dictionary<ADSSymbol, int> InitializeSymbolDict = new Dictionary<ADSSymbol, int>();
        public Dictionary<ADSSymbol, int> MotionParamSymbolDict = new Dictionary<ADSSymbol, int>();
        public Dictionary<ADSSymbol, int> MoveCmdSymbolDict = new Dictionary<ADSSymbol, int>();
        public Dictionary<ADSSymbol, int> CmdPosSymbolDict = new Dictionary<ADSSymbol, int>();
        public Dictionary<ADSSymbol, int> ControlSymbolDict = new Dictionary<ADSSymbol, int>();

        public SymbolHelper(ADSRouter adsrouter, SymbolMap symbolmap)
        {
            try
            {
                tcClient = adsrouter.tcClient;
                SymBolMap = symbolmap;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void InitSymbolDictionary(SymbolMap symbolmap)
        {
            object symbolGroup;
            object propObj;
            int handle = 0;
            ADSSymbol symbol = new ADSSymbol();

            SymBolMap = symbolmap;

            try
            {
                symbolGroup = SymBolMap.MachineStatusSymbols;
                MachineStatusSymbolDict.Clear();
                foreach (PropertyInfo symprop in symbolGroup.GetType().GetProperties())
                {
                    propObj = symprop.GetValue(symbolGroup, null);
                    if (propObj is ADSSymbol)
                    {
                        symbol = (ADSSymbol)propObj;

                        handle = tcClient.CreateVariableHandle(symbol.SymbolName);
                        symbol.Handle = handle;
                        MachineStatusSymbolDict.Add(symbol, handle);
                    }
                }

                symbolGroup = SymBolMap.AxisSymbols;
                InitializeSymbolDict.Clear();
                foreach (PropertyInfo symprop in symbolGroup.GetType().GetProperties())
                {
                    propObj = symprop.GetValue(symbolGroup, null);
                    if (propObj is ADSSymbol)
                    {
                        symbol = (ADSSymbol)propObj;

                        handle = tcClient.CreateVariableHandle(symbol.SymbolName);
                        symbol.Handle = handle;
                        InitializeSymbolDict.Add(symbol, handle);
                    }
                }

                symbolGroup = SymBolMap.MotionParamSymbols;
                InitializeSymbolDict.Clear();
                foreach (PropertyInfo symprop in symbolGroup.GetType().GetProperties())
                {
                    propObj = symprop.GetValue(symbolGroup, null);
                    if (propObj is ADSSymbol)
                    {
                        symbol = (ADSSymbol)propObj;

                        handle = tcClient.CreateVariableHandle(symbol.SymbolName);
                        symbol.Handle = handle;
                        MotionParamSymbolDict.Add(symbol, handle);
                    }
                }

                symbolGroup = SymBolMap.MoveCommandSymbols;
                MoveCmdSymbolDict.Clear();
                foreach (PropertyInfo symprop in symbolGroup.GetType().GetProperties())
                {
                    propObj = symprop.GetValue(symbolGroup, null);
                    if (propObj is ADSSymbol)
                    {
                        symbol = (ADSSymbol)propObj;

                        handle = tcClient.CreateVariableHandle(symbol.SymbolName);
                        symbol.Handle = handle;
                        MoveCmdSymbolDict.Add(symbol, handle);
                    }
                }

                symbolGroup = SymBolMap.CommandPosSymbols;
                CmdPosSymbolDict.Clear();
                foreach (PropertyInfo symprop in symbolGroup.GetType().GetProperties())
                {
                    propObj = symprop.GetValue(symbolGroup, null);
                    if (propObj is ADSSymbol)
                    {
                        symbol = (ADSSymbol)propObj;

                        handle = tcClient.CreateVariableHandle(symbol.SymbolName);
                        symbol.Handle = handle;
                        CmdPosSymbolDict.Add(symbol, handle);
                    }
                }


                symbolGroup = SymBolMap.ControlSymbols;
                ControlSymbolDict.Clear();
                foreach (PropertyInfo symprop in symbolGroup.GetType().GetProperties())
                {
                    propObj = symprop.GetValue(symbolGroup, null);
                    if (propObj is ADSSymbol)
                    {
                        symbol = (ADSSymbol)propObj;

                        handle = tcClient.CreateVariableHandle(symbol.SymbolName);
                        symbol.Handle = handle;
                        ControlSymbolDict.Add(symbol, handle);
                    }
                }

            }
            catch (Exception err)
            {
                //LoggerManager.Error($string.Format("SymbolHelper.InitSymbolDictionary(): Err = {0}, Current symbol = {1}",err.Message, symbol.SymbolName));
                LoggerManager.Exception(err);

            }

        }

        public void DeleteHandles()
        {
            try
            {
                foreach (var pair in InitializeSymbolDict)
                {
                    tcClient.DeleteVariableHandle(pair.Value);
                }

                foreach (var pair in MotionParamSymbolDict)
                {
                    tcClient.DeleteVariableHandle(pair.Value);
                }

                foreach (var pair in MoveCmdSymbolDict)
                {
                    tcClient.DeleteVariableHandle(pair.Value);
                }

                foreach (var pair in CmdPosSymbolDict)
                {
                    tcClient.DeleteVariableHandle(pair.Value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public bool[] ConvByte2Bits(byte input)
        {
            int ibuff = 0;
            bool[] cnvtVal;

            try
            {
                cnvtVal = new bool[8];

                for (int bitIndex = 0; bitIndex < 8; bitIndex++)
                {
                    ibuff = (input >> bitIndex) & 0x01;

                    if (ibuff == 1)
                    {
                        cnvtVal[bitIndex] = true;
                    }
                    else
                    {
                        cnvtVal[bitIndex] = false;
                    }
                }
                return cnvtVal;
            }
            catch (Exception)
            {
                //throw;
                return null;
            }
        }

        public bool[] ConvInt2Bits(int input)
        {
            int ibuff = 0;
            bool[] cnvtVal;

            try
            {
                cnvtVal = new bool[16];

                for (int bitIndex = 0; bitIndex < 16; bitIndex++)
                {
                    ibuff = (input >> bitIndex) & 0x01;

                    if (ibuff == 1)
                    {
                        cnvtVal[bitIndex] = true;
                    }
                    else
                    {
                        cnvtVal[bitIndex] = false;
                    }
                }
                return cnvtVal;
            }
            catch (Exception)
            {
                //throw;
                return null;
            }
        }

        public bool[] ConvLong2Bits(long input)
        {
            long ibuff = 0;
            bool[] cnvtVal;

            try
            {
                cnvtVal = new bool[32];

                for (int bitIndex = 0; bitIndex < 32; bitIndex++)
                {
                    ibuff = (input >> bitIndex) & 0x01;

                    if (ibuff == 1)
                    {
                        cnvtVal[bitIndex] = true;
                    }
                    else
                    {
                        cnvtVal[bitIndex] = false;
                    }
                }
                return cnvtVal;
            }
            catch (Exception)
            {
                //throw;
                return null;
            }
        }

        public bool[] ConvUInt2Bits(uint input)
        {
            uint ibuff = 0;
            bool[] cnvtVal;

            try
            {
                cnvtVal = new bool[32];

                for (int bitIndex = 0; bitIndex < 32; bitIndex++)
                {
                    ibuff = (input >> bitIndex) & 0x01;

                    if (ibuff == 1)
                    {
                        cnvtVal[bitIndex] = true;
                    }
                    else
                    {
                        cnvtVal[bitIndex] = false;
                    }
                }
                return cnvtVal;
            }
            catch (Exception)
            {
                //throw;
                return null;
            }
        }

        public object ReadSymbol(ADSSymbol symbol, Type datatype, int datanum)
        {
            return tcClient.ReadAny(symbol.Handle, datatype, new int[] { datanum });
        }
        public object ReadSymbol(ADSSymbol symbol, Type datatype)
        {
            return tcClient.ReadAny(symbol.Handle, datatype);
        }

        public int WriteSymbol(int handle, object value)
        {
            int retVal = -1;

            try
            {

                if (tcClient.IsConnected == true)
                {
                    try
                    {
                        tcClient.WriteAny(handle, value);
                        retVal = 0;
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                        throw;
                    }
                }
                else
                {

                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($"WriteSymbol(): Error occurred.", err.Message);
                LoggerManager.Exception(err);
            }

            return retVal;
        }


        public int WriteSymbol(ADSSymbol symbol, bool[] value)
        {
            int retVal = -1;
            bool validValue = false;

            try
            {

                string valueType = GetSymbolDataType(symbol);

                if (symbol.DataType == EnumDataType.BOOL & symbol.VariableType == EnumVariableType.ARR)
                {
                    validValue = true;
                }
                else
                {
                    validValue = false;
                }

                if (tcClient.IsConnected == true & symbol.Handle != 0 & validValue == true)
                {
                    tcClient.WriteAny(symbol.Handle, value);
                    retVal = 0;
                }
                else
                {
                    LoggerManager.Debug($"SymbolHelper.WriteSymbol(): Skip writing. Symbol = {symbol.SymbolName}, Data type = {symbol.DataType.ToString()}, Value type = {valueType}");

                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($"WriteSymbol(): Error occurred.", err.Message);
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public int WriteSymbol(ADSSymbol symbol, object value)
        {
            int retVal = -1;
            bool validValue = false;
            string valueType = "Unknown";

            try
            {
                switch (symbol.DataType)
                {
                    case EnumDataType.Type_Undefined:
                        if (symbol.VariableType == EnumVariableType.STRUCTUREDARRAY)
                        {
                            validValue = true;
                        }
                        else if (symbol.VariableType == EnumVariableType.STRUCTURE)
                        {
                            validValue = true;
                        }
                        break;
                    case EnumDataType.BOOL:
                        valueType = "bool";
                        if (value is bool | value is bool[])
                        {
                            validValue = true;
                        }
                        break;
                    case EnumDataType.BYTE:
                        valueType = "byte";
                        if (value is byte | value is byte[])
                        {
                            validValue = true;
                        }
                        break;
                    case EnumDataType.WORD:
                        valueType = "ushort";
                        if (value is ushort | value is ushort[])
                        {
                            validValue = true;
                        }
                        break;
                    case EnumDataType.DWORD:
                        valueType = "uint";
                        if (value is uint | value is uint[])
                        {
                            validValue = true;
                        }
                        break;
                    case EnumDataType.SINT:
                        valueType = "short";
                        if (value is short | value is short[])
                        {
                            validValue = true;
                        }
                        break;
                    case EnumDataType.INT:
                        valueType = "short";
                        if (value is short | value is short[])
                        {
                            validValue = true;
                        }
                        break;
                    case EnumDataType.DINT:
                        valueType = "int";
                        if (value is int | value is int[])
                        {
                            validValue = true;
                        }
                        break;
                    case EnumDataType.USINT:
                        valueType = "byte";
                        if (value is byte | value is byte[])
                        {
                            validValue = true;
                        }
                        break;
                    case EnumDataType.UINT:
                        valueType = "ushort";
                        if (value is ushort | value is ushort[])
                        {
                            validValue = true;
                        }
                        break;
                    case EnumDataType.UDINT:
                        valueType = "uint";
                        if (value is uint | value is uint[])
                        {
                            validValue = true;
                        }
                        break;
                    case EnumDataType.REAL:
                        valueType = "float";
                        if (value is float | value is float[])
                        {
                            validValue = true;
                        }
                        break;
                    case EnumDataType.LREAL:
                        valueType = "double";
                        if (value is double | value is double[])
                        {
                            validValue = true;
                        }
                        break;
                    default:
                        break;
                }

                if (tcClient.IsConnected == true & symbol.Handle != 0 & validValue == true)
                {
                    tcClient.WriteAny(symbol.Handle, value);
                    retVal = 0;
                }
                else
                {
                    LoggerManager.Debug($"SymbolHelper.WriteSymbol(): Skip writing. Symbol = {symbol.SymbolName}, Data type = {symbol.DataType.ToString()}, Value type = {valueType}");
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($"WriteSymbol(): Error occurred.", err.Message);
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public string GetSymbolDataType(ADSSymbol symbol)
        {
            string dataType = "Unknown";
            try
            {
                switch (symbol.DataType)
                {
                    case EnumDataType.Type_Undefined:
                        break;
                    case EnumDataType.BOOL:
                        dataType = "bool";
                        break;
                    case EnumDataType.BYTE:
                        dataType = "byte";
                        break;
                    case EnumDataType.WORD:
                        dataType = "ushort";
                        break;
                    case EnumDataType.DWORD:
                        dataType = "uint";
                        break;
                    case EnumDataType.SINT:
                        dataType = "short";
                        break;
                    case EnumDataType.INT:
                        dataType = "ushort";
                        break;
                    case EnumDataType.DINT:
                        dataType = "int";
                        break;
                    case EnumDataType.USINT:
                        dataType = "byte";
                        break;
                    case EnumDataType.UINT:
                        dataType = "ushort";
                        break;
                    case EnumDataType.UDINT:
                        dataType = "uint";
                        break;
                    case EnumDataType.REAL:
                        dataType = "float";
                        break;
                    case EnumDataType.LREAL:
                        dataType = "double";
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return dataType;
        }
        public Type GetDataTypeOfSymbol(ADSSymbol symbol)
        {
            Type dataType = null;
            try
            {
                switch (symbol.DataType)
                {
                    case EnumDataType.Type_Undefined:
                        break;
                    case EnumDataType.BOOL:
                        dataType = typeof(bool);
                        break;
                    case EnumDataType.BYTE:
                        dataType = typeof(byte);
                        break;
                    case EnumDataType.WORD:
                        dataType = typeof(ushort);
                        break;
                    case EnumDataType.DWORD:
                        dataType = typeof(uint); ;
                        break;
                    case EnumDataType.SINT:
                        dataType = typeof(SByte);
                        break;
                    case EnumDataType.INT:
                        dataType = typeof(short);
                        break;
                    case EnumDataType.DINT:
                        dataType = typeof(int);
                        break;
                    case EnumDataType.USINT:
                        dataType = typeof(byte);
                        break;
                    case EnumDataType.UINT:
                        dataType = typeof(ushort); ;
                        break;
                    case EnumDataType.UDINT:
                        dataType = typeof(uint); ;
                        break;
                    case EnumDataType.REAL:
                        dataType = typeof(float); ;
                        break;
                    case EnumDataType.LREAL:
                        dataType = typeof(double); ;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return dataType;
        }
    }

    [Serializable]
    public abstract class ADSSymbolBase
    {
        private EnumDataType mDataType;
        public EnumDataType DataType
        {
            get { return mDataType; }
            set { mDataType = value; }
        }

        private EnumVariableType mVariableType;
        public EnumVariableType VariableType
        {
            get { return mVariableType; }
            set { mVariableType = value; }
        }


        private string mSymbolName;

        public string SymbolName
        {
            get { return mSymbolName; }
            set { mSymbolName = value; }
        }

        private int mDataNumber;

        public int DataNumber
        {
            get { return mDataNumber; }
            set { mDataNumber = value; }
        }

        private string mDesc;
        [XmlAttribute("Desc")]
        public string Desc
        {
            get { return mDesc; }
            set { mDesc = value; }
        }



        [XmlIgnoreAttribute, JsonIgnore]
        public object AssociatedParameter = null;
        [XmlIgnoreAttribute, JsonIgnore]
        public object SymbolValue = null;

        private int mHandle;
        [XmlIgnoreAttribute, JsonIgnore]
        public int Handle
        {
            get { return mHandle; }
            set { mHandle = value; }
        }

        public ADSSymbolBase()
        {
            try
            {
                mDataType = EnumDataType.BYTE;
                mVariableType = EnumVariableType.VAR;
                mSymbolName = "";
                mDesc = "Undefined";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public ADSSymbolBase(string _symbol,
                        EnumDataType _datatype,
                        EnumVariableType _vartype,
                        string _desc = "", 
                        int _datanum = 1)
        {
            try
            {
                mSymbolName = string.Copy(_symbol);
                mDataType = _datatype;
                mVariableType = _vartype;
                mDataNumber = _datanum;

                if (_desc == "")
                {
                    mDesc = string.Copy(mSymbolName);
                }
                else
                {
                    mDesc = string.Copy(_desc);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public ADSSymbolBase(string _symbol,
                        EnumDataType _datatype,
                        EnumVariableType _vartype,
                        int _datanum = 1)
        {
            try
            {
                mSymbolName = string.Copy(_symbol);
                mDataType = _datatype;
                mVariableType = _vartype;
                mDataNumber = _datanum;

                mDesc = string.Copy(mSymbolName);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public ADSSymbolBase(string _symbol,
                        EnumDataType _datatype,
                        EnumVariableType _vartype)
        {
            mSymbolName = string.Copy(_symbol);
            mDataType = _datatype;
            mVariableType = _vartype;
            mDataNumber = 1;
            mDesc = string.Copy(mSymbolName);
        }

    }

    public class ADSSymbol : ADSSymbolBase
    {
        public ADSSymbol():base()
        {

        }
        public ADSSymbol(string _symbol,
                EnumDataType _datatype,
                EnumVariableType _vartype) : base(_symbol, _datatype, _vartype)
        {
        }
        public ADSSymbol(string _symbol,
                        EnumDataType _datatype,
                        EnumVariableType _vartype,
                        int _datanum = 1) : base(_symbol, _datatype, _vartype, _datanum)
        {
        }
        public ADSSymbol(string _symbol,
                         EnumDataType _datatype,
                         EnumVariableType _vartype,
                         string _desc = "",
                         int _datanum = 1):base(_symbol, _datatype, _vartype, _desc, _datanum)
        {
        }
    }
    public class ADSStructSymbol : ADSSymbolBase
    {
        private int _StreamLength;

        public int StreamLength
        {
            get { return _StreamLength; }
            set { _StreamLength = value; }
        }
        public ADSStructSymbol(string _symbol,
                EnumVariableType _vartype,
                string _desc = "", int length = 1)
        {
            SymbolName = string.Copy(_symbol);
            DataType = EnumDataType.Type_Undefined;
            VariableType = _vartype;
            DataNumber = 0;
            StreamLength = length;
            if (_desc == "")
            {
                Desc = string.Copy(SymbolName);
            }
            else
            {
                Desc = string.Copy(_desc);
            }

        }
    }
    #endregion


    #region MotionParam and CDX




    #endregion
}
