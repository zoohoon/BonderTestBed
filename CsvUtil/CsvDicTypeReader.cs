using System;
using System.Collections.Generic;

namespace CsvUtil
{
    using System.IO;
    using System.Text;

    public class CsvDicTypeReader
    {
        public const String DEVPARAMETERDATA_FILENAME = "DevParameterData.csv";
        public const String SYSPARAMETERDATA_FILENAME = "SysParameterData.csv";
        public const String COMMONPARAMETERDATA_FILENAME = "CommonParameterData.csv";

        //==>  Dictionary<KEY, Dictionary<COLUMN NAME, DATA>> 
        public Dictionary<String, Dictionary<String, String>> Data { get; set; }
        public List<String[]> ArrayData { get; set; }
        public String KeyColumn { get; set; }
        public int KeyColumnIdx { get; set; }
        public int ColumnCount { get; set; }
        public String FilePath { get; set; }
        public CsvDicTypeReader(String filePath, String keyColumn)
        {
            Data = new Dictionary<String, Dictionary<String, String>>();
            ArrayData = new List<String[]>();
            FilePath = filePath;
            KeyColumn = keyColumn;
        }

        public bool Import()
        {
            if (File.Exists(FilePath) == false)
                return false;

            String[] lines = null;
            using (StreamReader sr = new StreamReader(FilePath, Encoding.Default))
            {
                String[] lineSep = new String[] { "\r\n" };
                String text = sr.ReadToEnd();
                lines = text.Split(lineSep, StringSplitOptions.None);
            }

            if (lines == null || lines.Length < 2)
                return false;

            String columLine = lines[0];
            String[] columnArr = columLine.Split(';');
            int columnLength = columnArr.Length;
            ColumnCount = columnLength;

            bool isFindKey = false;
            KeyColumnIdx = 0;
            for (int i = 0; i < columnLength; i++)
            {
                if (columnArr[i] == KeyColumn)
                {
                    KeyColumnIdx = i;
                    isFindKey = true;
                    break;
                }
            }

            if (isFindKey == false)
                return false;

            for (int i = 1; i < lines.Length; i++)
            {

                String[] fieldArr = lines[i].Split(';');

                if (fieldArr.Length != columnLength)
                {

                    continue;
                }

                Dictionary<String, String> recordData = new Dictionary<String, String>();
                for (int col = 0; col < columnLength; col++)
                {
                    recordData.Add(columnArr[col], fieldArr[col]);
                }

                if (Data.ContainsKey(fieldArr[KeyColumnIdx]))
                {

                    continue;
                }


                Data.Add(fieldArr[KeyColumnIdx], recordData);
                ArrayData.Add(fieldArr);
            }

            return true;
        }
        public CsvRecord RemoveData(String key)
        {
            //==> Remove1
            var dataItem = Data[key];
            Data.Remove(key);

            //==> Remove2
            int removeDataIndex = ArrayData.FindIndex(item => item[KeyColumnIdx] == key);
            var arrayDataItem = ArrayData[removeDataIndex];
            ArrayData.RemoveAt(removeDataIndex);

            return new CsvRecord(dataItem, arrayDataItem, key);
        }
        public bool InsertData(int index, CsvRecord csvRecord)
        {
            if(Data.ContainsKey(csvRecord.Key))
            {
                return false;
            }

            Data.Add(csvRecord.Key, csvRecord.DataItem);
            ArrayData.Insert(index, csvRecord.ArrayDataItem);

            return true;
        }
    }
    public class CsvRecord
    {
        public Dictionary<String, String> DataItem { get; set; }
        public String[] ArrayDataItem { get; set; }
        public String Key { get; set; }
        public CsvRecord(Dictionary<String, String> dataItem, String[] arrayDataItem, String key)
        {
            DataItem = dataItem;
            ArrayDataItem = arrayDataItem;
            Key = key;
        }
    }
}
