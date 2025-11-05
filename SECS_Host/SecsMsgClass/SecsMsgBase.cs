using SECS_Host.Help;
using SECS_Host.Model;
using System;
using System.Collections.Generic;
using XComPro.Library;

namespace SECS_Host.SecsMsgClass
{
    public abstract class SecsMsgBase
    {
        public abstract bool Excute();

        public XComProW m_XComPro;
        public AddLogDelegate AddLog { get; set; }
        public SECSData m_RecvMSGDataList;
        protected SECSData m_SendDataList;

        protected int lMsgId;
        protected short nDeviceID;
        protected short nStream;
        protected short nFunc;
        protected short nWbit;
        protected int lSysbyte;

        public bool isSendComplete;

        public SecsMsgBase()
        {
            //InitSecsMsgClass();
        }

        public void InitSecsMsgClass()
        {
            m_RecvMSGDataList = null;
        }

        public void SetParameter(int lMsgId, short nDeviceID, short nStream, short nFunc, short nWbit, int lSysbyte)
        {
            this.lMsgId = lMsgId;
            this.nDeviceID = nDeviceID;
            this.nStream = nStream;
            this.nFunc = nFunc;
            this.nWbit = nWbit;
            this.lSysbyte = lSysbyte;
        }

        public void SetSendData(SECSData sendDataList)
        {
            m_SendDataList = sendDataList;
        }

        protected string Dump<ItemType>(ItemType[] items)
        {
            string ret = null;

            if (items == null || items.Length < 1)
            {
                ret = "";
            }
            else
            {
                ret = items[0].ToString();
                for (int i = 1; i < items.Length; i++)
                {
                    ret = ret + " " + items[i].ToString();
                }
            }

            return ret;
        }

        /// <summary>
        /// Func that Get Data from SECS Message. SECS 메시지로부터 데이터를 가져오는 부분.
        /// </summary>
        /// <param name="tabCount"></param>
        /// <returns></returns>
        public SECSData GetDataFromSecsMsg(int tabCount = -1)
        {
            SECSData retSECSData = null;
            SECS_DataTypeCategory currentMsgType = SECS_DataTypeCategory.NONE;
            string tabSpace = "";

            if(Enum.TryParse(m_XComPro.GetCurrentItemType(lMsgId).ToString(), out currentMsgType))
            {
                ////////////////////////////////////////////////////////////////////////
                if (tabCount != -1)
                {
                    for (int j = 0; j < tabCount; j++)
                        tabSpace += "   ";
                    tabSpace += "";
                }
                ////////////////////////////////////////////////////////////////////////

                if (currentMsgType == (SECS_DataTypeCategory.ASCII))
                {
                    retSECSData = GetAsciiItem(tabSpace);
                }
                else if (currentMsgType == (SECS_DataTypeCategory.BINARY))
                {
                    retSECSData = GetBinaryItem(tabSpace);
                }
                else if (currentMsgType == (SECS_DataTypeCategory.BOOL))
                {
                    retSECSData = GetBOOLItem(tabSpace);
                }
                else if (currentMsgType == (SECS_DataTypeCategory.FLOAT4))
                {
                    retSECSData = GetF4Item(tabSpace);
                }
                else if (currentMsgType == (SECS_DataTypeCategory.FLOAT8))
                {
                    retSECSData = GetF8Item(tabSpace);
                }
                else if (currentMsgType == (SECS_DataTypeCategory.INT1))
                {
                    retSECSData = GetI1Item(tabSpace);
                }
                else if (currentMsgType == (SECS_DataTypeCategory.INT2))
                {
                    retSECSData = GetI2Item(tabSpace);
                }
                else if (currentMsgType == (SECS_DataTypeCategory.INT4))
                {
                    retSECSData = GetI4Item(tabSpace);
                }
                else if (currentMsgType == (SECS_DataTypeCategory.INT8))
                {
                    retSECSData = GetI8Item(tabSpace);
                }
                else if (currentMsgType == (SECS_DataTypeCategory.UINT1))
                {
                    retSECSData = GetU1Item(tabSpace);
                }
                else if (currentMsgType == (SECS_DataTypeCategory.UINT2))
                {
                    retSECSData = GetU2Item(tabSpace);
                }
                else if (currentMsgType == (SECS_DataTypeCategory.UINT4))
                {
                    retSECSData = GetU4Item(tabSpace);
                }
                else if (currentMsgType == (SECS_DataTypeCategory.UINT8))
                {
                    retSECSData = GetU8Item(tabSpace);
                }
                else if (currentMsgType == (SECS_DataTypeCategory.LIST))
                {
                    retSECSData = GetListItem(tabCount, tabSpace);
                }
                else
                {

                }
            }
            else
            {
                throw new InvalidCastException("MakeSecsMsgData(int) : Wrong Cast to SECS_MsgCategory");
            }

            return retSECSData;
        }

        #region
        private SECSData GetAsciiItem(String tabSpace = "")
        {
            int nReturn = 0;
            int count = 0;
            SECSGenericData<string> data = new SECSGenericData<string>() { Value = "", MsgType = SECS_DataTypeCategory.ASCII };
            nReturn = m_XComPro.GetAsciiItem(lMsgId, ref data.Value, ref count); AddLog(tabSpace + "ASCII {0}", data.Value);

            return data;
        }
        private SECSData GetBinaryItem(String tabSpace = "")
        {
            int nReturn = 0;
            SECSGenericData<byte> data = new SECSGenericData<byte>() { Value = 0, MsgType = SECS_DataTypeCategory.BINARY };
            nReturn = m_XComPro.GetBinaryItem(lMsgId, ref data.Value); AddLog(tabSpace + "BINARY {0}", data.Value);

            return data;
        }
        private SECSData GetBOOLItem(String tabSpace = "")
        {
            int nReturn = 0;
            SECSGenericData<bool> data = new SECSGenericData<bool>() { Value = false, MsgType = SECS_DataTypeCategory.BOOL };
            nReturn = m_XComPro.GetBoolItem(lMsgId, ref data.Value); AddLog(tabSpace + "bool {0}", data.Value);

            return data;
        }
        private SECSData GetF4Item(String tabSpace = "")
        {
            int nReturn = 0;
            SECSGenericData<float> data = new SECSGenericData<float>() { Value = 0, MsgType = SECS_DataTypeCategory.FLOAT4 };
            nReturn = m_XComPro.GetF4Item(lMsgId, ref data.Value); AddLog(tabSpace + "FLOAT4 {0}", data.Value);

            return data;
        }
        private SECSData GetF8Item(String tabSpace = "")
        {
            int nReturn = 0;
            SECSGenericData<double> data = new SECSGenericData<double>() { Value = 0, MsgType = SECS_DataTypeCategory.FLOAT8 };
            nReturn = m_XComPro.GetF8Item(lMsgId, ref data.Value); AddLog(tabSpace + "FLOAT8 {0}", data.Value);

            return data;
        }
        private SECSData GetI1Item(String tabSpace = "")
        {
            int nReturn = 0;
            SECSGenericData<sbyte> data = new SECSGenericData<sbyte>() { Value = 0, MsgType = SECS_DataTypeCategory.INT1 };
            nReturn = m_XComPro.GetI1Item(lMsgId, ref data.Value); AddLog(tabSpace + "INT1 {0}", data.Value);

            return data;
        }
        private SECSData GetI2Item(String tabSpace = "")
        {
            int nReturn = 0;
            SECSGenericData<short> data = new SECSGenericData<short>() { Value = 0, MsgType = SECS_DataTypeCategory.INT2 };
            nReturn = m_XComPro.GetI2Item(lMsgId, ref data.Value); AddLog(tabSpace + "INT2 {0}", data.Value);

            return data;
        }
        private SECSData GetI4Item(String tabSpace = "")
        {
            int nReturn = 0;
            SECSGenericData<int> data = new SECSGenericData<int>() { Value = 0, MsgType = SECS_DataTypeCategory.INT4 };
            nReturn = m_XComPro.GetI4Item(lMsgId, ref data.Value); AddLog(tabSpace + "INT4 {0}", data.Value);

            return data;
        }
        private SECSData GetI8Item(String tabSpace = "")
        {
            int nReturn = 0;
            SECSGenericData<long> data = new SECSGenericData<long>() { Value = 0, MsgType = SECS_DataTypeCategory.INT8 };
            nReturn = m_XComPro.GetI8Item(lMsgId, ref data.Value); AddLog(tabSpace + "INT8 {0}", data.Value);

            return data;
        }
        private SECSData GetU1Item(String tabSpace = "")
        {
            int nReturn = 0;
            SECSGenericData<byte> data = new SECSGenericData<byte>() { Value = 0, MsgType = SECS_DataTypeCategory.UINT1 };
            nReturn = m_XComPro.GetU1Item(lMsgId, ref data.Value); AddLog(tabSpace + "UINT1 {0}", data.Value);
            return data;
        }
        private SECSData GetU2Item(String tabSpace = "")
        {
            int nReturn = 0;
            SECSGenericData<ushort> data = new SECSGenericData<ushort>() { Value = 0, MsgType = SECS_DataTypeCategory.UINT2 };
            nReturn = m_XComPro.GetU2Item(lMsgId, ref data.Value); AddLog(tabSpace + "UINT2 {0}", data.Value);

            return data;
        }
        private SECSData GetU4Item(String tabSpace = "")
        {
            int nReturn = 0;
            SECSGenericData<uint> data = new SECSGenericData<uint>() { Value = 0, MsgType = SECS_DataTypeCategory.UINT4 };
            nReturn = m_XComPro.GetU4Item(lMsgId, ref data.Value); AddLog(tabSpace + "UINT4 {0}", data.Value);

            return data;
        }
        private SECSData GetU8Item(String tabSpace = "")
        {
            int nReturn = 0;
            SECSGenericData<ulong> data = new SECSGenericData<ulong>() { Value = 0, MsgType = SECS_DataTypeCategory.UINT8 };
            nReturn = m_XComPro.GetU8Item(lMsgId, ref data.Value); AddLog(tabSpace + "UINT8 {0}", data.Value);

            return data;
        }
        private SECSData GetListItem(int tabCount, String tabSpace = "")
        {
            int nReturn = 0;
            int retVal = 0;
            nReturn = m_XComPro.GetListItem(lMsgId, ref retVal); AddLog(tabSpace + "LIST {0}", retVal);

            SECSGenericData<List<SECSData>> data = new SECSGenericData<List<SECSData>>();
            data.MsgType = SECS_DataTypeCategory.LIST;
            data.Value = new List<SECSData>();

            for (int i = 0; i < retVal; i++)
            {
                SECSData listElement = GetDataFromSecsMsg(tabCount == -1 ? tabCount : tabCount + 1);
                data.Value.Add(listElement);
            }

            //data.Count = data.Value.Count;
            return data;
        }
        #endregion

        /// <summary>
        /// Add list elements to SECS message item (SecsData를 형식에 맞게 SECS Message item으로 Set.)
        /// </summary>
        /// <param name="_m_XComPro"></param>
        /// <param name="lSMsgId"></param>
        /// <param name="msgItem"></param>
        public static void SetSECSItems(XComProW _m_XComPro, int lSMsgId, SECSData msgItem)
        {
            if (msgItem.MsgType == SECS_DataTypeCategory.LIST)
            {
                _m_XComPro.SetListItem(lSMsgId, msgItem.Count);
                for (int i = 0; i < msgItem.Count; i++)
                    SetSECSItems(_m_XComPro, lSMsgId, ((SECSGenericData<List<SECSData>>)msgItem).Value[i]);
            }
            else
            {
                if (msgItem.MsgType == SECS_DataTypeCategory.BOOL)
                    _m_XComPro.SetBoolItem(lSMsgId, ((SECSGenericData<bool>)msgItem).Value);
                else if (msgItem.MsgType == SECS_DataTypeCategory.UINT4)
                    _m_XComPro.SetU4Item(lSMsgId, ((SECSGenericData<uint>)msgItem).Value);
                else if (msgItem.MsgType == SECS_DataTypeCategory.BINARY)
                    _m_XComPro.SetBinaryItem(lSMsgId, ((SECSGenericData<byte>)msgItem).Value);
                else if (msgItem.MsgType == SECS_DataTypeCategory.ASCII)
                    _m_XComPro.SetAsciiItem(lSMsgId, ((SECSGenericData<string>)msgItem).Value);
                else if (msgItem.MsgType == SECS_DataTypeCategory.JIS8)
                    _m_XComPro.SetJis8Item(lSMsgId, ((SECSGenericData<string>)msgItem).Value);
                else if (msgItem.MsgType == SECS_DataTypeCategory.INT8)
                    _m_XComPro.SetI8Item(lSMsgId, ((SECSGenericData<long>)msgItem).Value);
                else if (msgItem.MsgType == SECS_DataTypeCategory.INT1)
                    _m_XComPro.SetI1Item(lSMsgId, ((SECSGenericData<sbyte>)msgItem).Value);
                else if (msgItem.MsgType == SECS_DataTypeCategory.INT2)
                    _m_XComPro.SetI2Item(lSMsgId, ((SECSGenericData<short>)msgItem).Value);
                else if (msgItem.MsgType == SECS_DataTypeCategory.INT4)
                    _m_XComPro.SetI4Item(lSMsgId, ((SECSGenericData<int>)msgItem).Value);
                else if (msgItem.MsgType == SECS_DataTypeCategory.FLOAT8)
                    _m_XComPro.SetF8Item(lSMsgId, ((SECSGenericData<double>)msgItem).Value);
                else if (msgItem.MsgType == SECS_DataTypeCategory.FLOAT4)
                    _m_XComPro.SetF4Item(lSMsgId, ((SECSGenericData<float>)msgItem).Value);
                else if (msgItem.MsgType == SECS_DataTypeCategory.UINT8)
                    _m_XComPro.SetU8Item(lSMsgId, ((SECSGenericData<ulong>)msgItem).Value);
                else if (msgItem.MsgType == SECS_DataTypeCategory.UINT1)
                    _m_XComPro.SetU1Item(lSMsgId, ((SECSGenericData<byte>)msgItem).Value);
                else if (msgItem.MsgType == SECS_DataTypeCategory.UINT2)
                    _m_XComPro.SetU2Item(lSMsgId, ((SECSGenericData<ushort>)msgItem).Value);
            }
        }

    }
}
