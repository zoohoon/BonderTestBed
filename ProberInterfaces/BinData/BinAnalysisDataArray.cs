using LogModule;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ProberInterfaces.BinData
{
    public class BinAnalysisDataArray : IEnumerable<BinAnalysisData>
    {
        private BinAnalysisData[] AnalysisDataArray;

        public BinAnalysisData this[int i]
        {
            get
            {
                BinAnalysisData retVal = null;

                if (i < AnalysisDataArray.Length)
                {
                    retVal = AnalysisDataArray[i];
                }
                else
                {
                    try
                    {
                        throw new IndexOutOfRangeException();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                    retVal = null;
                }

                return retVal;
            }
            set
            {
                if (i < AnalysisDataArray.Length)
                {
                    AnalysisDataArray[i] = value;
                }
                else
                {
                    try
                    {
                        throw new IndexOutOfRangeException();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
        }

        public BinAnalysisDataArray(int idx, string bindata)
        {
            try
            {
                AnalysisDataArray = new BinAnalysisData[idx];

                for (int i = 0; i < idx; i++)
                {
                    AnalysisDataArray[i] = new BinAnalysisData();
                    AnalysisDataArray[i].bin_data = bindata;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public int Count
        {
            get
            {
                int retCount = -1;
                if (AnalysisDataArray != null)
                {
                    retCount = AnalysisDataArray.Length;
                }
                else
                {
                    retCount = -1;
                }
                return AnalysisDataArray.Length;
            }
        }

        private bool _Valid = false;
        public bool Valid
        {
            get { return _Valid; }
            set
            {
                if (value != _Valid)
                {
                    _Valid = value;
                }
            }
        }

        public IEnumerator<BinAnalysisData> GetEnumerator()
        {
            return ((IEnumerable<BinAnalysisData>)AnalysisDataArray).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)AnalysisDataArray).GetEnumerator();
        }
    }

    public class BinAnalysisData
    {
        public string bin_data { get; set; }
        public byte pf_Data { get; set; }     // PassFail Data

        private byte _cat_Data;               // Category Data
        public byte cat_Data
        {
            get
            {
                return _cat_Data;
            }
            set
            {
                _cat_Data = value;
                SetAnalysisData();
            }
        }

        private byte _cat_Data_Ex;              // Category Data Ex
        public byte cat_Data_Ex
        {
            get
            {
                return _cat_Data_Ex;
            }
            set
            {
                _cat_Data_Ex = value;
                SetAnalysisData();
            }
        }

        public int analysis_Data
        {
            get;
            set;
        }

        private void SetAnalysisData()
        {
            try
            {
                analysis_Data = cat_Data_Ex * (0x01 << 8);
                analysis_Data += cat_Data;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public BinAnalysisData()
        {
            this.bin_data = string.Empty;
        }
    }
}
