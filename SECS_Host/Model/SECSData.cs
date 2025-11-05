using SECS_Host.Help;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace SECS_Host.Model
{
    public abstract class SECSData
    {
        public SECS_DataTypeCategory MsgType;
        public SECS_IDCategory IDCategory;
        public int Count
        {
            get
            {
                int retCount = 0;
                if (this.MsgType == SECS_DataTypeCategory.LIST)
                    retCount = (this as SECSGenericData<List<SECSData>>).Value.Count;
                else
                    retCount = 1;

                return retCount;
            }
        }

        public void ChangeSECSDataFromStr(string secsData)
        {
            if (this.MsgType == SECS_DataTypeCategory.ASCII)
            {
                if (secsData is string)
                    ((SECSGenericData<string>)this).Value = secsData;
                else
                    throw new InvalidCastException();
            }
            else if (this.MsgType == SECS_DataTypeCategory.BOOL)
            {
                if (!bool.TryParse(secsData.ToLower(), out ((SECSGenericData<bool>)this).Value))
                    throw new InvalidCastException();
            }
            else if (this.MsgType == SECS_DataTypeCategory.UINT4)
            {
                if (!uint.TryParse(secsData, out ((SECSGenericData<uint>)this).Value))
                    throw new InvalidCastException();
            }
            else if (this.MsgType == SECS_DataTypeCategory.BINARY)
            {
                if (!byte.TryParse(secsData.ToLower(), out ((SECSGenericData<byte>)this).Value))
                    throw new InvalidCastException();
            }
            else if (this.MsgType == SECS_DataTypeCategory.JIS8)
            {
                if (secsData is string)
                    ((SECSGenericData<string>)this).Value = secsData;
                else
                    throw new InvalidCastException();
            }
            else if (this.MsgType == SECS_DataTypeCategory.INT8)
            {
                if (!long.TryParse(secsData.ToLower(), out ((SECSGenericData<long>)this).Value))
                    throw new InvalidCastException();
            }
            else if (this.MsgType == SECS_DataTypeCategory.INT1)
            {
                if (!sbyte.TryParse(secsData.ToLower(), out ((SECSGenericData<sbyte>)this).Value))
                    throw new InvalidCastException();
            }
            else if (this.MsgType == SECS_DataTypeCategory.INT2)
            {
                if (!short.TryParse(secsData.ToLower(), out ((SECSGenericData<short>)this).Value))
                    throw new InvalidCastException();
            }
            else if (this.MsgType == SECS_DataTypeCategory.INT4)
            {
                if (!int.TryParse(secsData.ToLower(), out ((SECSGenericData<int>)this).Value))
                    throw new InvalidCastException();
            }
            else if (this.MsgType == SECS_DataTypeCategory.FLOAT8)
            {
                if (!double.TryParse(secsData.ToLower(), out ((SECSGenericData<double>)this).Value))
                    throw new InvalidCastException();
            }
            else if (this.MsgType == SECS_DataTypeCategory.FLOAT4)
            {
                if (!float.TryParse(secsData.ToLower(), out ((SECSGenericData<float>)this).Value))
                    throw new InvalidCastException();
            }
            else if (this.MsgType == SECS_DataTypeCategory.UINT8)
            {
                if (!ulong.TryParse(secsData.ToLower(), out ((SECSGenericData<ulong>)this).Value))
                    throw new InvalidCastException();
            }
            else if (this.MsgType == SECS_DataTypeCategory.UINT1)
            {
                if (!byte.TryParse(secsData.ToLower(), out ((SECSGenericData<byte>)this).Value))
                    throw new InvalidCastException();
            }
            else if (this.MsgType == SECS_DataTypeCategory.UINT2)
            {
                if (!ushort.TryParse(secsData.ToLower(), out ((SECSGenericData<ushort>)this).Value))
                    throw new InvalidCastException();
            }
            else if (this.MsgType == SECS_DataTypeCategory.UINT4)
            {
                if (!uint.TryParse(secsData.ToLower(), out ((SECSGenericData<uint>)this).Value))
                    throw new InvalidCastException();
            }
            else if (this.MsgType == SECS_DataTypeCategory.LIST)
            {
                if (!int.TryParse(secsData.ToLower(), out ((SECSGenericData<int>)this).Value))
                    throw new InvalidCastException();
            }
        }

        public static void RemoveStruct_Recursion(SECSData RemoveList)
        {
            if (RemoveList != null)
            {
                for (int i = 0; i < RemoveList.Count; i++)
                {
                    if (RemoveList.MsgType == SECS_DataTypeCategory.LIST)
                    {
                        RemoveStruct_Recursion(((SECSGenericData<List<SECSData>>)RemoveList).Value[i]);
                    }
                }
                if (RemoveList.MsgType == SECS_DataTypeCategory.LIST)
                {
                    ((SECSGenericData<List<SECSData>>)RemoveList).Value.Clear();
                    ((SECSGenericData<List<SECSData>>)RemoveList).Value = null;
                }
            }
        }

        public static SECSData MakeSECSDataFromStr(String dataStr, SECS_DataTypeCategory category)
        {
            SECSData retSecsData = null;

            if(category == SECS_DataTypeCategory.BINARY)
            {
                retSecsData = MakeBinaryFromStr(dataStr);
            }
            else if (category == SECS_DataTypeCategory.BOOL)
            {
                retSecsData = MakeBoolFromStr(dataStr);
            }
            else if (category == SECS_DataTypeCategory.FLOAT4)
            {
                retSecsData = MakeF4FromStr(dataStr);
            }
            else if (category == SECS_DataTypeCategory.FLOAT8)
            {
                retSecsData = MakeF8FromStr(dataStr);
            }
            else if (category == SECS_DataTypeCategory.INT1)
            {
                retSecsData = MakeI1FromStr(dataStr);
            }
            else if (category == SECS_DataTypeCategory.INT2)
            {
                retSecsData = MakeI2FromStr(dataStr);
            }
            else if (category == SECS_DataTypeCategory.INT4)
            {
                retSecsData = MakeI4FromStr(dataStr);
            }
            else if (category == SECS_DataTypeCategory.INT8)
            {
                retSecsData = MakeI8FromStr(dataStr);
            }
            else if (category == SECS_DataTypeCategory.JIS8)
            {
                retSecsData = MakeJIS8FromStr(dataStr);
            }
            else if (category == SECS_DataTypeCategory.LIST)
            {
                retSecsData = MakeListFromStr();
            }
            else if (category == SECS_DataTypeCategory.UINT1)
            {
                retSecsData = MakeU1FromStr(dataStr);
            }
            else if (category == SECS_DataTypeCategory.UINT2)
            {
                retSecsData = MakeU2FromStr(dataStr);
            }
            else if (category == SECS_DataTypeCategory.UINT4)
            {
                retSecsData = MakeU4FromStr(dataStr);
            }
            else if (category == SECS_DataTypeCategory.UINT8)
            {
                retSecsData = MakeU8FromStr(dataStr);
            }
            else if (category == SECS_DataTypeCategory.ASCII)
            {
                retSecsData = MakeAsciiFromStr(dataStr);
            }

            return retSecsData;
        }

        #region ==> Make Data From Str
        public static SECSData MakeAsciiFromStr(string dataStr)
        {
            SECSGenericData<String> retData = new SECSGenericData<String>();
            retData.MsgType = SECS_DataTypeCategory.ASCII;
            retData.Value = dataStr;

            return retData;
        }

        public static SECSData MakeU8FromStr(string dataStr)
        {
            SECSGenericData<ulong> retData = new SECSGenericData<ulong>();
            retData.MsgType = SECS_DataTypeCategory.UINT8;
            retData.Value = ulong.Parse(dataStr);

            return retData;
        }

        public static SECSData MakeU4FromStr(string dataStr)
        {
            SECSGenericData<uint> retData = new SECSGenericData<uint>();
            retData.MsgType = SECS_DataTypeCategory.UINT4;
            retData.Value = uint.Parse(dataStr);

            return retData;
        }

        public static SECSData MakeU2FromStr(string dataStr)
        {
            SECSGenericData<ushort> retData = new SECSGenericData<ushort>();
            retData.MsgType = SECS_DataTypeCategory.UINT2;
            retData.Value = ushort.Parse(dataStr);

            return retData;
        }

        public static SECSData MakeU1FromStr(string dataStr)
        {
            SECSGenericData<byte> retData = new SECSGenericData<byte>();
            retData.MsgType = SECS_DataTypeCategory.UINT1;
            bool bRet = byte.TryParse(dataStr, out retData.Value);

            return retData;
        }

        public static SECSData MakeListFromStr()
        {
            SECSGenericData<List<SECSData>> retData = new SECSGenericData<List<SECSData>>();
            retData.MsgType = SECS_DataTypeCategory.LIST;
            retData.Value = new List<SECSData>();

            return retData;
        }

        public static SECSData MakeJIS8FromStr(string dataStr)
        {
            SECSGenericData<String> retData = new SECSGenericData<String>();
            retData.MsgType = SECS_DataTypeCategory.JIS8;
            retData.Value = dataStr;

            return retData;
        }

        public static SECSData MakeI8FromStr(string dataStr)
        {
            SECSGenericData<long> retData = new SECSGenericData<long>();
            retData.MsgType = SECS_DataTypeCategory.INT8;
            retData.Value = long.Parse(dataStr);

            return retData;
        }

        public static SECSData MakeI4FromStr(string dataStr)
        {
            SECSGenericData<int> retData = new SECSGenericData<int>();
            retData.MsgType = SECS_DataTypeCategory.INT4;
            retData.Value = int.Parse(dataStr);

            return retData;
        }

        public static SECSData MakeI2FromStr(string dataStr)
        {
            SECSGenericData<short> retData = new SECSGenericData<short>();
            retData.MsgType = SECS_DataTypeCategory.INT2;
            retData.Value = short.Parse(dataStr);

            return retData;
        }

        public static SECSData MakeI1FromStr(string dataStr)
        {
            SECSGenericData<sbyte> retData = new SECSGenericData<sbyte>();
            retData.MsgType = SECS_DataTypeCategory.INT1;
            retData.Value = sbyte.Parse(dataStr);

            return retData;
        }

        public static SECSData MakeF8FromStr(string dataStr)
        {
            SECSGenericData<double> retData = new SECSGenericData<double>();
            retData.MsgType = SECS_DataTypeCategory.FLOAT8;
            retData.Value = double.Parse(dataStr);

            return retData;
        }

        public static SECSData MakeF4FromStr(string dataStr)
        {
            SECSGenericData<float> retData = new SECSGenericData<float>();
            retData.MsgType = SECS_DataTypeCategory.FLOAT4;
            retData.Value = float.Parse(dataStr);

            return retData;
        }

        public static SECSData MakeBinaryFromStr(String dataStr)
        {
            SECSGenericData<byte> retData = new SECSGenericData<byte>();
            retData.MsgType = SECS_DataTypeCategory.BINARY;
            retData.Value = byte.Parse(dataStr);

            return retData;
        }

        public static SECSData MakeBoolFromStr(String dataStr)
        {
            SECSGenericData<bool> retData = new SECSGenericData<bool>();
            retData.MsgType = SECS_DataTypeCategory.BOOL;
            retData.Value = dataStr == "T" ? true : false;

            return retData;
        }


        #endregion
    }

    public class SECSGenericData<T> : SECSData
    {
        public T Value;
        public override string ToString()
        {
            return Value.ToString();
        }
    }
}