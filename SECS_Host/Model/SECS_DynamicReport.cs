using SECS_Host.Help;
using System;
using System.Collections.Generic;

namespace SECS_Host.Model.DynamicMSG
{
    public class SECS_DynamicReport
    {
        public String DyanmicReportName;
        public SECSData DynamicValue;

        public void SECS_GetStructFromFile(String path, String separator)
        {
            string strline = null;

            System.IO.StreamReader file = new System.IO.StreamReader(path);
            while ((strline = file.ReadLine()) != null)
            {
                if (strline.Contains(separator))
                {
                    this.DyanmicReportName = separator;
                    GetStructFromStream(file);
                }
            }
            file.Close();
        }

        public void GetStructFromStream(System.IO.StreamReader file)
        {
            SECSData dynamicVal = null;
            String strline = null;
            int listItemCount = 0;
            strline = file.ReadLine();

            dynamicVal = SECS_StructFactory(strline, ref listItemCount);

            if (dynamicVal != null)
            {
                if (dynamicVal.MsgType == SECS_DataTypeCategory.LIST)
                {
                    dynamicVal = GetSECSMsgList_Recursion(file, listItemCount);
                }

                this.DynamicValue = dynamicVal;
            }
        }

        private SECSGenericData<List<SECSData>> GetSECSMsgList_Recursion(System.IO.StreamReader file, int size)
        {
            SECSGenericData<List<SECSData>> retSecsMsg = new SECSGenericData<List<SECSData>>();
            retSecsMsg.Value = new List<SECSData>();
            retSecsMsg.MsgType = SECS_DataTypeCategory.LIST;

            int listItemCount = 0;
            String strline = null;

            for (int i = 0; i < size; i++)
            {
                SECSData tempSECSMsg = null;
                strline = file.ReadLine();
                tempSECSMsg = SECS_StructFactory(strline, ref listItemCount);

                if (tempSECSMsg != null)
                {
                    if (tempSECSMsg.MsgType == SECS_DataTypeCategory.LIST)
                    {
                        tempSECSMsg = GetSECSMsgList_Recursion(file, listItemCount);
                    }

                    retSecsMsg.Value.Add(tempSECSMsg);
                }
            }

            return retSecsMsg;
        }

        private SECSData SECS_StructFactory(String stringLine, ref int listItemCount)
        {
            SECSData factory = null;
            String[] splitLine;
            String[] lastData = null;
            bool bretVal = false;

            stringLine = stringLine.Trim();
            splitLine = stringLine.Split(',');

            if (splitLine != null)
            {
                SECS_DataTypeCategory dataCategory = SECS_DataTypeCategory.NONE;
                string realDataValue = "";
                dataCategory = EnumConverter.ParseDataTypeCategory(splitLine[0].ToUpper());

                if(2 < splitLine.Length)
                {
                    lastData = splitLine[2].Split('[', ']', '<', '>');
                    realDataValue = lastData[3];
                }

                if (dataCategory != SECS_DataTypeCategory.NONE)
                    factory = SECSData.MakeSECSDataFromStr(realDataValue, dataCategory);
                else
                    throw new Exception("SECS_StructFactory() : Wrong Data Category");

                if(lastData != null)
                    factory.IDCategory = 4 < lastData.Length ? EnumConverter.ParseIdCategory(lastData[1]) : SECS_IDCategory.NONE;

                bretVal = int.TryParse(splitLine[1], out listItemCount);
            }
            else
            {
                //잘못된 값
            }

            return factory;
        }

        public void DisposeList()
        {
            SECSData.RemoveStruct_Recursion(DynamicValue);
        }
    }
}
