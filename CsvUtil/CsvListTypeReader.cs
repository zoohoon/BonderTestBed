using System;
using System.Collections.Generic;
using System.Linq;

namespace CsvUtil
{
    using LogModule;
    using System.IO;
    public class CsvListTypeReader
    {
        public List<String> Column { get; set; }
        public List<List<String>> Data { get; set; }
        public CsvListTypeReader()
        {
            Data = new List<List<String>>();
        }

        public bool Import(String filePath)
        {
            if (File.Exists(filePath) == false)
                return false;

            try
            {
                String[] lines = null;
                using (StreamReader sr = new StreamReader(filePath))
                {
                    String[] lineSep = new String[] { "\r\n" };
                    String text = sr.ReadToEnd();
                    lines = text.Split(lineSep, StringSplitOptions.None);
                }

                if (lines == null || lines.Length < 2)
                    return false;

                String columnLine = lines[0];
                String[] columnArr = columnLine.Split(';');
                int columnLength = columnArr.Length;

                Column = columnArr.ToList();

                for (int i = 1; i < lines.Length; i++)
                {
                    String[] filedArr = lines[i].Split(';');
                    if (filedArr.Length < columnLength)
                        continue;
                    Data.Add(filedArr.ToList());
                }
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return true;
        }
    }
}
