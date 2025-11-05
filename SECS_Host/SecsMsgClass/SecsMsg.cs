using SECS_Host.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SECS_Host.SecsMsgClass
{
    #region ==> Secs S1
    public class SecsMsg_S1F1FromEQ : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            int nReturn = 0;
            int lSMsgId = 0;

            if (m_XComPro != null)
            {
                m_XComPro.CloseSecsMsg(lMsgId);

                nReturn = m_XComPro.MakeSecsMsg(ref lSMsgId, nDeviceID, nStream, (short)(nFunc + 1), lSysbyte);
                #region Return Values
                if (nReturn < 0)
                {
                    AddLog("MakeSecsMsg failed: error={0}", nReturn);
                    return bReturn;
                }
                #endregion
                nReturn = m_XComPro.SetListItem(lSMsgId, 2);
                nReturn = m_XComPro.SetAsciiItem(lSMsgId, "XComProV1");
                nReturn = m_XComPro.SetAsciiItem(lSMsgId, "1.0.0");

                if (nWbit != 0)
                {
                    if ((nReturn = m_XComPro.Send(lSMsgId)) < 0)
                    {
                        AddLog("Failed to reply S{0}F{1}: error={2}", nStream, nFunc + 1, nReturn);
                        bReturn = false;
                    }
                    else
                    {
                        AddLog("Reply S{0}F{1} was sent successfully", nStream, nFunc + 1);
                        bReturn = true;
                    }
                }
            }

            return bReturn;
        }
    }
    public class SecsMsg_S1F1ToEQ : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            int nReturn = 0;
            int lSMsgId = 0;
            short strm = 1;
            short func = 1;

            nReturn = m_XComPro.MakeSecsMsg(ref lSMsgId, m_XComPro.DeviceID, strm, func, 0);
            if (nReturn < 0)
            {
                AddLog("MakeSecsMsg failed: error={0}", nReturn);
            }

            if ((nReturn = m_XComPro.Send(lSMsgId)) < 0)
                AddLog("Failed to reply S{0}F{1}: error={2}", strm, func, nReturn);
            else
            {
                AddLog("Reply S{0}F{1} was sent successfully", strm, func);
                bReturn = true;
            }

            return bReturn;
        }
    }
    public class SecsMsg_S1F2 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;

            SECSData MsgList = GetDataFromSecsMsg(1);
            m_XComPro.CloseSecsMsg(lMsgId);

            bReturn = true;
            return bReturn;
        }
    }

    public class SecsMsg_S1F3 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            int nReturn = 0;
            int lSMsgId = 0;

            nReturn = m_XComPro.MakeSecsMsg(ref lSMsgId, m_XComPro.DeviceID, (short)(1), (short)(3), 0);

            if (nReturn < 0)
            {
                AddLog("MakeSecsMsg failed: error={0}", nReturn);
                return bReturn;
            }

            SetSECSItems(m_XComPro, lSMsgId, m_SendDataList);

            if ((nReturn = m_XComPro.Send(lSMsgId)) < 0)
                AddLog("Failed to reply S{0}F{1}: error={2}", 1, 3, nReturn);
            else
                AddLog("Reply S{0}F{1} was sent successfully", 1, 3);

            return bReturn;
        }
    }

    public class SecsMsg_S1F4 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            SECSData MsgList = GetDataFromSecsMsg(1);
            m_XComPro.CloseSecsMsg(lMsgId);

            bReturn = true;
            return bReturn;
        }
    }
    public class SecsMsg_S1F11 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            int nReturn = 0;
            int lSMsgId = 0;

            nReturn = m_XComPro.MakeSecsMsg(ref lSMsgId, m_XComPro.DeviceID, (short)(1), (short)(11), 0);
            if (nReturn < 0)
            {
                AddLog("MakeSecsMsg failed: error={0}", nReturn);
            }

            nReturn = m_XComPro.SetListItem(lSMsgId, 0);

            if ((nReturn = m_XComPro.Send(lSMsgId)) < 0)
                AddLog("Failed to reply S{0}F{1}: error={2}", 1, 11, nReturn);
            else
            {
                AddLog("Reply S{0}F{1} was sent successfully", 1, 11);
                bReturn = true;
            }
            return bReturn;
        }
    }
    public class SecsMsg_S1F12 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;

            SECSData MsgList = GetDataFromSecsMsg(1);
            m_XComPro.CloseSecsMsg(lMsgId);

            m_RecvMSGDataList = MsgList;
            bReturn = true;
            return bReturn;
        }
    }

    public class SecsMsg_S1F13ToEQ : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            int iListCount = 0;
            int nReturn = 0;
            int lSMsgId = 0;

            nReturn = m_XComPro.MakeSecsMsg(ref lSMsgId, nDeviceID, (short)1, (short)13, lSysbyte);
            #region Return Values
            if (nReturn < 0)
            {
                AddLog("MakeSecsMsg failed: error={0}", nReturn);
                return bReturn;
            }
            #endregion
            nReturn = m_XComPro.SetListItem(lSMsgId, 2);
            nReturn = m_XComPro.SetAsciiItem(lSMsgId, "XComProV1");
            nReturn = m_XComPro.SetAsciiItem(lSMsgId, "1.0.0");

            //if (nWbit != 0)
            {
                if ((nReturn = m_XComPro.Send(lSMsgId)) < 0)
                {
                    AddLog("Failed to reply S{0}F{1}: error={2}", (short)1, (short)13, nReturn);
                    bReturn = false;
                }
                else
                {
                    AddLog("Reply S{0}F{1} was sent successfully", (short)1, (short)13);
                    bReturn = true;
                }
            }

            return bReturn;
        }
    }



    public class SecsMsg_S1F13FromEQ : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            int iListCount = 0;
            int nReturn = 0;
            int lSMsgId = 0;

            nReturn = m_XComPro.GetListItem(lMsgId, ref iListCount); AddLog("LIST {0}: next={1}", iListCount, nReturn);
            if (iListCount == 0)
            {
                m_XComPro.CloseSecsMsg(lMsgId);

                // create response message
                nReturn = m_XComPro.MakeSecsMsg(ref lSMsgId, nDeviceID, nStream, (short)(nFunc + 1), lSysbyte);
                #region Return Values
                if (nReturn < 0)
                {
                    AddLog("MakeSecsMsg failed: error={0}", nReturn);
                    return bReturn;
                }
                #endregion
                nReturn = m_XComPro.SetListItem(lSMsgId, 2);
                nReturn = m_XComPro.SetBinaryItem(lSMsgId, 0);
                nReturn = m_XComPro.SetListItem(lSMsgId, 0);

                if (nWbit != 0)
                {
                    if ((nReturn = m_XComPro.Send(lSMsgId)) < 0)
                    {
                        AddLog("Failed to reply S{0}F{1}: error={2}", nStream, nFunc + 1, nReturn);
                        bReturn = false;
                    }
                    else
                    {
                        AddLog("Reply S{0}F{1} was sent successfully", nStream, nFunc + 1);
                        bReturn = true;
                    }
                }
            }
            else
            {
                string sBuff = "";
                int nLen = 0;

                #region Next Item Format
                /* Return Values : Next Item Format

                    LIST = 0
                    Binary = 8
                    Boolean = 9
                    ASCII = 16
                    JIS-8 = 17
                    1-byte Signed Integer = 25
                    2-byte Signed Integer = 26
                    4-byte Signed Integer = 28
                    8-byte Signed Integer = 24
                    1-byte Unsigned Integer = 41
                    2-byte Unsigned Integer = 42
                    4-byte Unsigned Integer = 44
                    8-byte Unsigned Integer = 40
                    4-byte Floating Point = 36
                    8-byte Floating Point = 32
                    No Item = 63
                */

                #endregion
                nReturn = m_XComPro.GetAsciiItem(lMsgId, ref sBuff, ref nLen); AddLog("     ASCII {0}: next={1}", sBuff, nReturn);
                nReturn = m_XComPro.GetAsciiItem(lMsgId, ref sBuff, ref nLen); AddLog("     ASCII {0}: next={1}", sBuff, nReturn);
                m_XComPro.CloseSecsMsg(lMsgId);

                nReturn = m_XComPro.MakeSecsMsg(ref lSMsgId, nDeviceID, nStream, (short)(nFunc + 1), lSysbyte);
                #region Return Values
                if (nReturn < 0)
                {
                    AddLog("MakeSecsMsg failed: error={0}", nReturn);
                    return bReturn;
                }
                #endregion
                nReturn = m_XComPro.SetListItem(lSMsgId, 2);
                nReturn = m_XComPro.SetBinaryItem(lSMsgId, 0);
                nReturn = m_XComPro.SetListItem(lSMsgId, 2);
                nReturn = m_XComPro.SetAsciiItem(lSMsgId, "XComProV1");
                nReturn = m_XComPro.SetAsciiItem(lSMsgId, "1.0.0");

                if (nWbit != 0)
                {
                    if ((nReturn = m_XComPro.Send(lSMsgId)) < 0)
                    {
                        AddLog("Failed to reply S{0}F{1}: error={2}", nStream, nFunc + 1, nReturn);
                        bReturn = false;
                    }
                    else
                    {
                        AddLog("Reply S{0}F{1} was sent successfully", nStream, nFunc + 1);
                        bReturn = true;
                    }
                }
            }

            return bReturn;
        }
    }
    public class SecsMsg_S1F14 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            m_XComPro.CloseSecsMsg(lMsgId);
            bReturn = true;
            return bReturn;
        }
    }
    public class SecsMsg_S1F15 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            int nReturn = 0;
            int lSMsgId = 0;

            nReturn = m_XComPro.MakeSecsMsg(ref lSMsgId, m_XComPro.DeviceID, (short)(1), (short)(15), 0);
            if (nReturn < 0)
            {
                AddLog("MakeSecsMsg failed: error={0}", nReturn);
            }
            else
            {
                if ((nReturn = m_XComPro.Send(lSMsgId)) < 0)
                    AddLog("Failed to reply S{0}F{1}: error={2}", 1, 15, nReturn);
                else
                {
                    AddLog("Reply S{0}F{1} was sent successfully", 1, 15);
                    bReturn = true;
                }
            }

            return bReturn;
        }
    }
    public class SecsMsg_S1F16 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            byte bData = 0;
            int nReturn = m_XComPro.GetBinaryItem(lMsgId, ref bData);
            AddLog("    Binary {0}({1}): next={2}",
                bData, bData == 0 ? "OFF-LINE Acknowledge" : "Reserved", nReturn);
            m_XComPro.CloseSecsMsg(lMsgId);

            bReturn = true;
            return bReturn;
        }
    }
    public class SecsMsg_S1F17 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            int nReturn = 0;
            int lSMsgId = 0;

            nReturn = m_XComPro.MakeSecsMsg(ref lSMsgId, m_XComPro.DeviceID, (short)(1), (short)(17), 0);
            if (nReturn < 0)
            {
                AddLog("MakeSecsMsg failed: error={0}", nReturn);
            }
            else
            {
                if ((nReturn = m_XComPro.Send(lSMsgId)) < 0)
                    AddLog("Failed to reply S{0}F{1}: error={2}", 1, 17, nReturn);
                else
                {
                    AddLog("Reply S{0}F{1} was sent successfully", 1, 17);
                    bReturn = true;
                }
            }

            return bReturn;
        }
    }
    public class SecsMsg_S1F18 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            byte bData = 0;
            int nReturn = m_XComPro.GetBinaryItem(lMsgId, ref bData);
            AddLog("    Binary {0}({1}): next={2}",
                bData, bData == 0 ? "ON-Line Accepted" : (bData == 1 ? "ON_LINE Not Allowed" : (bData == 2 ? "Equipment Already ON_LINE" : "Reserved")), nReturn);
            m_XComPro.CloseSecsMsg(lMsgId);

            bReturn = true;
            return bReturn;
        }
    }
    #endregion

    #region ==> Secs S2
    public class SecsMsg_S2F13 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            int nReturn = 0;
            int lSMsgId = 0;

            nReturn = m_XComPro.MakeSecsMsg(ref lSMsgId, m_XComPro.DeviceID, 2, 13, 0);

            SetSECSItems(m_XComPro, lSMsgId, m_SendDataList);

            if ((nReturn = m_XComPro.Send(lSMsgId)) < 0)
            {
                AddLog("Failed to reply S{0}F{1}: error={2}", 2, 13, nReturn);
                throw new Exception();
            }
            else
            {
                AddLog("Reply S{0}F{1} was sent successfully", 2, 13);
                bReturn = true;
            }

            return bReturn;
        }
    }
    public class SecsMsg_S2F14 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;

            SECSData MsgList = GetDataFromSecsMsg(1);
            m_XComPro.CloseSecsMsg(lMsgId);
            //if (m_RecvMSGDataList != null)
            //    m_RecvMSGDataList.Clear();
            m_RecvMSGDataList = MsgList;

            bReturn = true;
            return bReturn;
        }
    }

    public class SecsMsg_S2F15 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            int nReturn = 0;
            int lSMsgId = 0;

            nReturn = m_XComPro.MakeSecsMsg(ref lSMsgId, m_XComPro.DeviceID, (short)(2), (short)(15), 0);

            if (nReturn < 0)
            {
                AddLog("MakeSecsMsg failed: error={0}", nReturn);
                return bReturn;
            }

            SetSECSItems(m_XComPro, lSMsgId, m_SendDataList);

            if ((nReturn = m_XComPro.Send(lSMsgId)) < 0)
                AddLog("Failed to reply S{0}F{1}: error={2}", 2, 15, nReturn);
            else
                AddLog("Reply S{0}F{1} was sent successfully", 2, 15);

            return bReturn;
        }
    }

    public class SecsMsg_S2F16 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            int nReturn = 0;
            byte retVal = 0;

            nReturn = m_XComPro.GetBinaryItem(lMsgId, ref retVal);

            string logAdd = "";
            switch (retVal)
            {
                case 0:
                    logAdd = "Acknowledge";
                    break;
                case 1:
                    logAdd = "Denied. At least one constant does not exist";
                    break;
                case 2:
                    logAdd = "Denied. Busy";
                    break;
                case 3:
                    logAdd = "Denied. At least one constant out of range";
                    break;
                default:
                    logAdd = "Other equipment-specific error";
                    break;
            }
            AddLog("S2F16 RetVal = {0}", logAdd);
            m_XComPro.CloseSecsMsg(lMsgId);
            bReturn = true;
            return bReturn;
        }
    }

    public class SecsMsg_S2F17ToEQ : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            int nReturn = 0;
            int lSMsgId = 0;

            nReturn = m_XComPro.MakeSecsMsg(ref lSMsgId, m_XComPro.DeviceID, 2, 17, 0);

            if ((nReturn = m_XComPro.Send(lSMsgId)) < 0)
            {
                AddLog("Failed to reply S{0}F{1}: error={2}", 2, 17, nReturn);
                throw new Exception();
            }
            else
            {
                AddLog("Reply S{0}F{1} was sent successfully", 2, 17);
                bReturn = true;
            }

            return bReturn;
        }
    }
    public class SecsMsg_S2F17FromEQ : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;

            SECSData MsgList = GetDataFromSecsMsg(1);
            m_XComPro.CloseSecsMsg(lMsgId);

            m_RecvMSGDataList = MsgList;

            bReturn = true;
            return bReturn;
        }
    }

    public class SecsMsg_S2F18ToEQ : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            int nReturn = 0;
            int lSMsgId = 0;
            String DateValue = DateTime.Now.ToString("yyyyMMddHHmmssff");

            nReturn = m_XComPro.MakeSecsMsg(ref lSMsgId, m_XComPro.DeviceID, 2, 18, 0);
            nReturn = m_XComPro.SetAsciiItem(lSMsgId, DateValue);

            if ((nReturn = m_XComPro.Send(lSMsgId)) < 0)
            {
                AddLog("Failed to reply S{0}F{1}: error={2}", 2, 18, nReturn);
                throw new Exception();
            }
            else
            {
                AddLog("Reply {2}(S{0}F{1}) was sent successfully", 2, 18, DateValue);
                bReturn = true;
            }

            return bReturn;
        }
    }
    public class SecsMsg_S2F18FromEQ : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;

            Model.SECSData Msg = GetDataFromSecsMsg(1);
            m_XComPro.CloseSecsMsg(lMsgId);
            //m_RecvMSGDataList = MsgList;

            bReturn = true;
            return bReturn;
        }
    }

    public class SecsMsg_S2F21 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            int nReturn = 0;
            int lSMsgId = 0;

            nReturn = m_XComPro.MakeSecsMsg(ref lSMsgId, m_XComPro.DeviceID, 2, 21, 0);

            SetSECSItems(m_XComPro, lSMsgId, m_SendDataList);

            if ((nReturn = m_XComPro.Send(lSMsgId)) < 0)
            {
                AddLog("Failed to reply S{0}F{1}: error={2}", 2, 21, nReturn);
                throw new Exception();
            }
            else
            {
                AddLog("Reply S{0}F{1} was sent successfully : {2}", 2, 21, m_SendDataList.ToString());
                bReturn = true;
            }

            return bReturn;
        }
    }

    public class SecsMsg_S2F22 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            int nReturn = 0;
            byte retVal = 0;

            nReturn = m_XComPro.GetBinaryItem(lMsgId, ref retVal);

            string logAdd = "";
            switch (retVal)
            {
                case 0:
                    logAdd = "Completed or done";
                    break;
                case 1:
                    logAdd = "Command does not exist";
                    break;
                case 2:
                    logAdd = "Cannot perform now";
                    break;
                default:
                    logAdd = "Reserved";
                    break;
            }

            AddLog("-> S2F22 RetVal = {0}", logAdd);
            m_XComPro.CloseSecsMsg(lMsgId);
            bReturn = true;
            return bReturn;
        }
    }

    public class SecsMsg_S2F23 : SecsMsgBase
    {
        public override bool Excute()
        {
            int nReturn = 0;
            int lSMsgId = 0;
            bool bReturn = false;
            nReturn = m_XComPro.MakeSecsMsg(ref lSMsgId, m_XComPro.DeviceID, (short)(2), (short)(23), 0);

            if (nReturn < 0)
            {
                AddLog("MakeSecsMsg failed: error={0}", nReturn);
                return bReturn;
            }

            SetSECSItems(m_XComPro, lSMsgId, m_SendDataList);

            if ((nReturn = m_XComPro.Send(lSMsgId)) < 0)
                AddLog("Failed to reply S{0}F{1}: error={2}", 2, 23, nReturn);
            else
                AddLog("Reply S{0}F{1} was sent successfully", 2, 23);

            return bReturn;
        }
    }

    public class SecsMsg_S2F24 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            int nReturn = 0;
            byte retVal = 0;

            nReturn = m_XComPro.GetBinaryItem(lMsgId, ref retVal);

            string logAdd = "";
            switch (retVal)
            {
                case 0:
                    logAdd = "Everything correct";
                    break;
                case 1:
                    logAdd = "Too many SVIDs";
                    break;
                case 2:
                    logAdd = "No more traces allowed";
                    break;
                case 3:
                    logAdd = "Invalid period";
                    break;
                default:
                    logAdd = "Equipment-specified error";
                    break;
            }

            AddLog("-> S2F24 RetVal = {0}", logAdd);
            m_XComPro.CloseSecsMsg(lMsgId);
            bReturn = true;
            return bReturn;
        }
    }
    public class SecsMsg_S2F29 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            int nReturn = 0;
            int lSMsgId = 0;

            nReturn = m_XComPro.MakeSecsMsg(ref lSMsgId, m_XComPro.DeviceID, 2, 29, 0);
            if (nReturn < 0)
                throw new Exception();
            nReturn = m_XComPro.SetListItem(lSMsgId, 0);
            if ((nReturn = m_XComPro.Send(lSMsgId)) < 0)
            {
                AddLog("Failed to reply S{0}F{1}: error={2}", 2, 29, nReturn);
            }
            else
            {
                AddLog("Reply S{0}F{1} was sent successfully", 2, 29);
                bReturn = true;
            }

            return bReturn;
        }
    }


    public class SecsMsg_S2F30 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;

            SECSData MsgList = GetDataFromSecsMsg(1);
            m_XComPro.CloseSecsMsg(lMsgId);

            if (m_RecvMSGDataList != null)
                SECSData.RemoveStruct_Recursion(m_RecvMSGDataList);
            m_RecvMSGDataList = MsgList;

            bReturn = true;
            return bReturn;
        }
    }

    public class SecsMsg_S2F31 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            int nReturn = 0;
            int lSMsgId = 0;

            nReturn = m_XComPro.MakeSecsMsg(ref lSMsgId, m_XComPro.DeviceID, 2, 31, 0);

            SetSECSItems(m_XComPro, lSMsgId, m_SendDataList);

            if ((nReturn = m_XComPro.Send(lSMsgId)) < 0)
            {
                AddLog("Failed to Reply TimeFormat(S2, F31): error={0}", nReturn);
                throw new Exception();
            }
            else
            {
                AddLog("Reply TimeFormat({0}) was sent successfully(S2, F31)", m_SendDataList.ToString());
            }

            return bReturn;
        }
    }
    public class SecsMsg_S2F32 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            int nReturn = 0;
            byte retVal = 0;

            nReturn = m_XComPro.GetBinaryItem(lMsgId, ref retVal);

            string logAdd = "";
            switch (retVal)
            {
                case 0:
                    logAdd = "OK";
                    break;
                case 1:
                    logAdd = "Error, not done";
                    break;
                case 2:
                default:
                    logAdd = "Reserved";
                    break;
            }
            AddLog("S2F32 RetVal = {0}", logAdd);
            m_XComPro.CloseSecsMsg(lMsgId);
            bReturn = true;
            return bReturn;
        }
    }

    public class SecsMsg_S2F33 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            int nReturn = 0;
            int lSMsgId = 0;

            int iListCount = 0;
            byte bData = 0;
            sbyte I1Data = 0;
            short sData = 0;
            int iData = 0;
            long lData = 0;
            byte U1Data = 0;
            ushort U2Data = 0;
            uint U4Data = 0;
            ulong U8Data = 0;
            float fData = 0;
            double dData = 0;
            bool blData = false;
            string sBuff = "";
            int nLen = 0;

            nReturn = m_XComPro.GetListItem(lMsgId, ref iListCount); AddLog("LIST {0}: next={1}", iListCount, nReturn);

            nReturn = m_XComPro.GetListItem(lMsgId, ref iListCount); AddLog("    LIST {0}: next={1}", iListCount, nReturn);
            nReturn = m_XComPro.GetI1Item(lMsgId, ref I1Data); AddLog("        INT1 {0}: next={1}", I1Data, nReturn);
            nReturn = m_XComPro.GetI2Item(lMsgId, ref sData); AddLog("        INT2 {0}: next={1}", sData, nReturn);
            nReturn = m_XComPro.GetI4Item(lMsgId, ref iData); AddLog("        INT4 {0}: next={1}", iData, nReturn);
            nReturn = m_XComPro.GetI8Item(lMsgId, ref lData); AddLog("        INT8 {0}: next={1}", lData, nReturn);
            nReturn = m_XComPro.GetU1Item(lMsgId, ref U1Data); AddLog("        UINT1 {0}: next={1}", U1Data, nReturn);
            nReturn = m_XComPro.GetU2Item(lMsgId, ref U2Data); AddLog("        UINT2 {0}: next={1}", U2Data, nReturn);
            nReturn = m_XComPro.GetU4Item(lMsgId, ref U4Data); AddLog("        UINT4 {0}: next={1}", U4Data, nReturn);
            nReturn = m_XComPro.GetU8Item(lMsgId, ref U8Data); AddLog("        UINT8 {0}: next={1}", U8Data, nReturn);
            nReturn = m_XComPro.GetF4Item(lMsgId, ref fData); AddLog("        FLOAT4 {0}: next={1}", fData, nReturn);
            nReturn = m_XComPro.GetF8Item(lMsgId, ref dData); AddLog("        FLOAT8 {0}: next={1}", dData, nReturn);

            nReturn = m_XComPro.GetListItem(lMsgId, ref iListCount); AddLog("    LIST {0}: next={1}", iListCount, nReturn);
            nReturn = m_XComPro.GetAsciiItem(lMsgId, ref sBuff, ref nLen); AddLog("        ASCII {0}: next={1}", sBuff, nReturn);
            nReturn = m_XComPro.GetJis8Item(lMsgId, ref sBuff, ref nLen); AddLog("        JIS8 {0}: next={1}", sBuff, nReturn);
            nReturn = m_XComPro.GetBinaryItem(lMsgId, ref bData); AddLog("        BINARY {0}: next={1}", bData, nReturn);
            nReturn = m_XComPro.GetBoolItem(lMsgId, ref blData); AddLog("        BOOL {0}: next={1}", blData, nReturn);

            m_XComPro.CloseSecsMsg(lMsgId);

            nReturn = m_XComPro.MakeSecsMsg(ref lSMsgId, nDeviceID, nStream, (short)(nFunc + 1), lSysbyte);
            #region Return Values
            if (nReturn < 0)
            {
                AddLog("MakeSecsMsg failed: error={0}", nReturn);
                return bReturn;
            }
            #endregion
            nReturn = m_XComPro.SetBinaryItem(lSMsgId, 0);

            if (nWbit != 0)
            {
                if ((nReturn = m_XComPro.Send(lSMsgId)) < 0)
                {
                    AddLog("Failed to reply S{0}F{1}: error={2}", nStream, nFunc + 1, nReturn);
                    bReturn = false;
                }
                else
                {
                    AddLog("Reply S{0}F{1} was sent successfully", nStream, nFunc + 1);
                    bReturn = true;
                }
            }
            return bReturn;
        }
    }
    public class SecsMsg_S2F34 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            m_XComPro.CloseSecsMsg(lMsgId);
            bReturn = true;
            return bReturn;
        }
    }
    public class SecsMsg_S2F36 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            m_XComPro.CloseSecsMsg(lMsgId);
            bReturn = true;
            return bReturn;
        }
    }
    public class SecsMsg_S2F38 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            m_XComPro.CloseSecsMsg(lMsgId);
            bReturn = true;
            return bReturn;
        }
    }

    public class SecsMsg_S2F41 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            int nReturn = 0;
            int lSMsgId = 0;

            nReturn = m_XComPro.MakeSecsMsg(ref lSMsgId, m_XComPro.DeviceID, 2, 41, 0);

            SetSECSItems(m_XComPro, lSMsgId, m_SendDataList);

            if ((nReturn = m_XComPro.Send(lSMsgId)) < 0)
            {
                AddLog("Failed to reply S{0}F{1}: error={2}", 2, 41, nReturn);
                throw new Exception();
            }
            else
            {
                AddLog("Reply S{0}F{1} was sent successfully : {2}", 2, 41, m_SendDataList.ToString());
                bReturn = true;
            }

            return bReturn;
        }
    }
    public class SecsMsg_S2F42 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;

            SECSData MsgList = GetDataFromSecsMsg(1);
            m_XComPro.CloseSecsMsg(lMsgId);
            m_RecvMSGDataList = MsgList;

            SECSGenericData<List<SECSData>> tempList = MsgList as SECSGenericData<List<SECSData>>;
            if(tempList != null)
            {
                string hcack = tempList.Value[0].ToString();

                string logAdd = "";
                switch (hcack)
                {
                    case "0":
                        logAdd = "Acknowledge";
                        break;
                    case "1":
                        logAdd = "Command does not exist";
                        break;
                    case "2":
                        logAdd = "Cannot perform now";
                        break;
                    case "3":
                        logAdd = "At least one parameter is invalid";
                        break;
                    case "4":
                        logAdd = "Acknowledge, command will be performed with completion signaled later by an event";
                        break;
                    case "5":
                        logAdd = "Rejected, Already in Desired Condition";
                        break;
                    case "6":
                        logAdd = "No such object exists";
                        break;
                    default:
                        logAdd = "Reserved";
                        break;
                }
                AddLog("-> S2F42 RetVal = {0}", logAdd);
            }

            bReturn = true;
            return bReturn;
        }
    }

    public class SecsMsg_S2F49 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            int nReturn = 0;
            int lSMsgId = 0;

            nReturn = m_XComPro.MakeSecsMsg(ref lSMsgId, m_XComPro.DeviceID, 2, 49, 0);

            SetSECSItems(m_XComPro, lSMsgId, m_SendDataList);

            if ((nReturn = m_XComPro.Send(lSMsgId)) < 0)
            {
                AddLog("Failed to reply S{0}F{1}: error={2}", 2, 49, nReturn);
                throw new Exception();
            }
            else
            {
                AddLog("Reply S{0}F{1} was sent successfully : {2}", 2, 49, m_SendDataList.ToString());
                bReturn = true;
            }

            return bReturn;
        }
    }
    public class SecsMsg_S2F50 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;

            SECSData MsgList = GetDataFromSecsMsg(1);
            m_XComPro.CloseSecsMsg(lMsgId);
            m_RecvMSGDataList = MsgList;

            SECSGenericData<List<SECSData>> tempList = MsgList as SECSGenericData<List<SECSData>>;
            if (tempList != null)
            {
                string hcack = tempList.Value[0].ToString();

                string logAdd = "";
                switch (hcack)
                {
                    case "0":
                        logAdd = "Acknowledge";
                        break;
                    case "1":
                        logAdd = "Command does not exist";
                        break;
                    case "2":
                        logAdd = "Cannot perform now";
                        break;
                    case "3":
                        logAdd = "At least one parameter is invalid";
                        break;
                    case "4":
                        logAdd = "Acknowledge, command will be performed with completion signaled later by an event";
                        break;
                    case "5":
                        logAdd = "Rejected, Already in Desired Condition";
                        break;
                    case "6":
                        logAdd = "No such object exists";
                        break;
                    default:
                        logAdd = "Reserved";
                        break;
                }
                AddLog("-> S2F42 RetVal = {0}", logAdd);
            }

            bReturn = true;
            return bReturn;
        }
    }

    #endregion

    #region ==> Secs S5
    public class SecsMsg_S5F1 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;

            SECSData MsgList = GetDataFromSecsMsg(1);
            m_XComPro.CloseSecsMsg(lMsgId);

            if (m_RecvMSGDataList != null)
                SECSData.RemoveStruct_Recursion(m_RecvMSGDataList);
            m_RecvMSGDataList = MsgList;

            bReturn = true;
            return bReturn;
        }
    }

    public class SecsMsg_S5F2 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            int nReturn = 0;
            int lSMsgId = 0;

            nReturn = m_XComPro.MakeSecsMsg(ref lSMsgId, m_XComPro.DeviceID, 5, 2, lSysbyte);

            SetSECSItems(m_XComPro, lSMsgId, m_SendDataList);

            if ((nReturn = m_XComPro.Send(lSMsgId)) < 0)
            {
                AddLog("Failed to reply S{0}F{1}: error={2}", 5, 2, nReturn);
                throw new Exception();
            }
            else
            {
                AddLog("Reply S{0}F{1} was sent successfully", 5, 2);
                bReturn = true;
            }

            return bReturn;
        }
    }

    public class SecsMsg_S5F3 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            int nReturn = 0;
            int lSMsgId = 0;

            nReturn = m_XComPro.MakeSecsMsg(ref lSMsgId, m_XComPro.DeviceID, (short)(5), (short)(3), 0);

            if (nReturn < 0)
            {
                AddLog("MakeSecsMsg failed: error={0}", nReturn);
                return bReturn;
            }

            SetSECSItems(m_XComPro, lSMsgId, m_SendDataList);

            if ((nReturn = m_XComPro.Send(lSMsgId)) < 0)
                AddLog("Failed to reply S{0}F{1}: error={2}", 5, 3, nReturn);
            else
                AddLog("Reply S{0}F{1} was sent successfully", 5, 3);

            return bReturn;
        }
    }

    public class SecsMsg_S5F4 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            int nReturn = 0;
            byte retVal = 0;

            nReturn = m_XComPro.GetBinaryItem(lMsgId, ref retVal);
            m_XComPro.CloseSecsMsg(lMsgId);

            string logAdd = "";
            switch (retVal)
            {
                case 0:
                    logAdd = "Accepted";
                    break;
                case 1:
                    logAdd = "Error, not accepted";
                    break;
                case 2:
                default:
                    logAdd = "Reserved";
                    break;
            }
            AddLog("S5F2 RetVal = {0}", logAdd);
            bReturn = true;
            return bReturn;
        }
    }

    public class SecsMsg_S5F5 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            int nReturn = 0;
            int lSMsgId = 0;

            nReturn = m_XComPro.MakeSecsMsg(ref lSMsgId, m_XComPro.DeviceID, nStream, nFunc, 0);
            if (nReturn < 0)
                throw new Exception();

            nReturn = m_XComPro.SetListItem(lSMsgId, 0);
            if ((nReturn = m_XComPro.Send(lSMsgId)) < 0)
            {
                AddLog("Failed to reply S{0}F{1}: error={2}", nStream, nFunc, nReturn);
            }
            else
            {
                AddLog("Reply S{0}F{1} was sent successfully", nStream, nFunc);
                bReturn = true;
            }

            return bReturn;
        }
    }

    public class SecsMsg_S5F6 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;

            m_RecvMSGDataList = GetDataFromSecsMsg(1);
            m_XComPro.CloseSecsMsg(lMsgId);

            bReturn = true;
            return bReturn;
        }
    }

    public class SecsMsg_S5F7 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            int nReturn = 0;
            int lSMsgId = 0;

            nReturn = m_XComPro.MakeSecsMsg(ref lSMsgId, m_XComPro.DeviceID, nStream, nFunc, 0);
            if (nReturn < 0)
                throw new Exception();

            if ((nReturn = m_XComPro.Send(lSMsgId)) < 0)
            {
                AddLog("Failed to reply S{0}F{1}: error={2}", nStream, nFunc, nReturn);
            }
            else
            {
                AddLog("Reply S{0}F{1} was sent successfully", nStream, nFunc);
                bReturn = true;
            }

            return bReturn;
        }
    }

    public class SecsMsg_S5F8 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;

            m_RecvMSGDataList = GetDataFromSecsMsg(1);
            m_XComPro.CloseSecsMsg(lMsgId);

            bReturn = true;
            return bReturn;
        }
    }

    public class SecsMsg_S5F23 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            int nReturn = 0;
            int lSMsgId = 0;

            int iListCount = 0;
            byte[] bData = new byte[7];
            sbyte[] I1Data = new sbyte[7];
            short[] sData = new short[7];
            int[] iData = new int[7];

            ushort[] U2Data = new ushort[7];
            uint[] U4Data = new uint[7];
            ulong[] U8Data = new ulong[7];
            long[] lData = new long[7];
            float[] fData = new float[7];
            double[] dData = new double[7];
            bool[] blData = new bool[7];
            int nItemCount = 0;

            nReturn = m_XComPro.GetListItem(lMsgId, ref iListCount); AddLog("LIST {0}: next={1}", iListCount, nReturn);

            nReturn = m_XComPro.GetBoolItem(lMsgId, blData, ref nItemCount); AddLog("    BOOL {0}: next={1}", Dump<bool>(blData), nReturn);
            nReturn = m_XComPro.GetBinaryItem(lMsgId, bData, ref nItemCount); AddLog("    BINARY {0}: next={1}", Dump<byte>(bData), nReturn);
            nReturn = m_XComPro.GetU1Item(lMsgId, bData, ref nItemCount); AddLog("    UINT1 {0}: next={1}", Dump<byte>(bData), nReturn);
            nReturn = m_XComPro.GetU2Item(lMsgId, U2Data, ref nItemCount); AddLog("    UINT2 {0}: next={1}", Dump<ushort>(U2Data), nReturn);
            nReturn = m_XComPro.GetU4Item(lMsgId, U4Data, ref nItemCount); AddLog("    UINT4 {0}: next={1}", Dump<uint>(U4Data), nReturn);
            nReturn = m_XComPro.GetU8Item(lMsgId, U8Data, ref nItemCount); AddLog("    UINT8 {0}: next={1}", Dump<ulong>(U8Data), nReturn);
            nReturn = m_XComPro.GetI1Item(lMsgId, I1Data, ref nItemCount); AddLog("    INT1 {0}: next={1}", Dump<sbyte>(I1Data), nReturn);
            nReturn = m_XComPro.GetI2Item(lMsgId, sData, ref nItemCount); AddLog("    INT2 {0}: next={1}", Dump<short>(sData), nReturn);
            nReturn = m_XComPro.GetI4Item(lMsgId, iData, ref nItemCount); AddLog("    INT4 {0}: next={1}", Dump<int>(iData), nReturn);
            nReturn = m_XComPro.GetI8Item(lMsgId, lData, ref nItemCount); AddLog("    INT8 {0}: next={1}", Dump<long>(lData), nReturn);
            nReturn = m_XComPro.GetF4Item(lMsgId, fData, ref nItemCount); AddLog("    FLOAT4 {0}: next={1}", Dump<float>(fData), nReturn);
            nReturn = m_XComPro.GetF8Item(lMsgId, dData, ref nItemCount); AddLog("    FLOAT8 {0}: next={1}", Dump<double>(dData), nReturn);

            m_XComPro.CloseSecsMsg(lMsgId);

            nReturn = m_XComPro.MakeSecsMsg(ref lSMsgId, nDeviceID, nStream, (short)(nFunc + 1), lSysbyte);
            #region Return Values
            if (nReturn < 0)
            {
                AddLog("MakeSecsMsg failed: error={0}", nReturn);
                return bReturn;
            }
            #endregion
            nReturn = m_XComPro.SetU1Item(lSMsgId, 0);

            if (nWbit != 0)
            {
                if ((nReturn = m_XComPro.Send(lSMsgId)) < 0)
                {
                    AddLog("Failed to reply S{0}F{1}: error={2}", nStream, nFunc + 1, nReturn);
                }
                else
                {
                    AddLog("Reply S{0}F{1} was sent successfully", nStream, nFunc + 1);
                }
            }
            return bReturn;
        }
    }
    public class SecsMsg_S5F24 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            m_XComPro.CloseSecsMsg(lMsgId);
            bReturn = true;
            return bReturn;
        }
    }
    #endregion

    #region ==> Secs S6
    public class SecsMsg_S6F1 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;

            SECSData MsgList = GetDataFromSecsMsg(1);
            m_XComPro.CloseSecsMsg(lMsgId);

            bReturn = true;
            return bReturn;
        }
    }

    public class SecsMsg_S6F2 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            int nReturn = 0;
            int lSMsgId = 0;

            nReturn = m_XComPro.MakeSecsMsg(ref lSMsgId, m_XComPro.DeviceID, nStream, nFunc, lSysbyte);
            if (nReturn < 0)
                throw new Exception();

            nReturn = m_XComPro.SetBinaryItem(lSMsgId, 0);

            if ((nReturn = m_XComPro.Send(lSMsgId)) < 0)
            {
                AddLog("Failed to reply S{0}F{1}: error={2}", nStream, nFunc, nReturn);
            }
            else
            {
                AddLog("Reply S{0}F{1} was sent successfully", nStream, nFunc);
                bReturn = true;
            }

            return bReturn;
        }
    }

    public class SecsMsg_S6F11 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            int nReturn = 0;
            int lSMsgId = 0;

            SECSData MsgList = GetDataFromSecsMsg(1);
            m_XComPro.CloseSecsMsg(lMsgId);

            nReturn = m_XComPro.MakeSecsMsg(ref lSMsgId, nDeviceID, nStream, (short)(nFunc + 1), lSysbyte);
            #region Return Values
            if (nReturn < 0)
            {
                AddLog("MakeSecsMsg failed: error={0}", nReturn);
                return bReturn;
            }
            #endregion
            nReturn = m_XComPro.SetBinaryItem(lSMsgId, 0);

            if (nWbit != 0)
            {
                if ((nReturn = m_XComPro.Send(lSMsgId)) < 0)
                {
                    AddLog("Failed to reply S{0}F{1}: error={2}", nStream, nFunc + 1, nReturn);
                    bReturn = false;
                }
                else
                {
                    AddLog("Reply S{0}F{1} was sent successfully", nStream, nFunc + 1);
                    bReturn = true;
                }
            }
            return bReturn;
        }
    }
    #endregion

    #region ==> Secs S10
    public class SecsMsg_S10F1 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;

            SECSData MsgList = GetDataFromSecsMsg(1);
            m_XComPro.CloseSecsMsg(lMsgId);

            bReturn = true;
            return bReturn;
        }
    }
    public class SecsMsg_S10F2 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;
            int nReturn = 0;
            int lSMsgId = 0;

            nReturn = m_XComPro.MakeSecsMsg(ref lSMsgId, m_XComPro.DeviceID, nStream, nFunc, lSysbyte);

            if (nReturn < 0)
                throw new Exception();

            nReturn = m_XComPro.SetBinaryItem(lSMsgId, 0);

            if ((nReturn = m_XComPro.Send(lSMsgId)) < 0)
            {
                AddLog("Failed to reply S{0}F{1}: error={2}", nStream, nFunc, nReturn);
            }
            else
            {
                AddLog("Reply S{0}F{1} was sent successfully", nStream, nFunc);
                bReturn = true;
            }

            return bReturn;
        }
    }

    public class SecsMsg_S10F3 : SecsMsgBase
    {
        public override bool Excute()
        {
            int nReturn = 0;
            int lSMsgId = 0;
            bool bReturn = false;
            nReturn = m_XComPro.MakeSecsMsg(ref lSMsgId, m_XComPro.DeviceID, nStream, nFunc, 0);

            if (nReturn < 0)
            {
                AddLog("MakeSecsMsg failed: error={0}", nReturn);
                return bReturn;
            }

            SetSECSItems(m_XComPro, lSMsgId, m_SendDataList);

            if ((nReturn = m_XComPro.Send(lSMsgId)) < 0)
                AddLog("Failed to reply S{0}F{1}: error={2}", nStream, nFunc, nReturn);
            else
                AddLog("Reply S{0}F{1} was sent successfully", nStream, nFunc);

            return bReturn;
        }
    }

    public class SecsMsg_S10F4 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;

            SECSData MsgList = GetDataFromSecsMsg(1);
            m_XComPro.CloseSecsMsg(lMsgId);

            bReturn = true;
            return bReturn;
        }
    }

    public class SecsMsg_S10F5 : SecsMsgBase
    {
        public override bool Excute()
        {
            int nReturn = 0;
            int lSMsgId = 0;
            bool bReturn = false;
            nReturn = m_XComPro.MakeSecsMsg(ref lSMsgId, m_XComPro.DeviceID, nStream, nFunc, 0);

            if (nReturn < 0)
            {
                AddLog("MakeSecsMsg failed: error={0}", nReturn);
                return bReturn;
            }

            SetSECSItems(m_XComPro, lSMsgId, m_SendDataList);

            if ((nReturn = m_XComPro.Send(lSMsgId)) < 0)
                AddLog("Failed to reply S{0}F{1}: error={2}", nStream, nFunc, nReturn);
            else
                AddLog("Reply S{0}F{1} was sent successfully", nStream, nFunc);

            return bReturn;
        }
    }

    public class SecsMsg_S10F6 : SecsMsgBase
    {
        public override bool Excute()
        {
            bool bReturn = false;

            SECSData MsgList = GetDataFromSecsMsg(1);
            m_XComPro.CloseSecsMsg(lMsgId);

            bReturn = true;
            return bReturn;
        }
    }
    #endregion
}