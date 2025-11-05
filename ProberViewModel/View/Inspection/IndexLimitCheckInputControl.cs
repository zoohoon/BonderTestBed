using System.ComponentModel;

namespace ProberViewModel
{
    public class IndexLimitCheckInputControl : IDataErrorInfo
    {
        public string Error { get { return string.Empty; } }
        public string this[string columnName]
        {
            get
            {
                string retVal = string.Empty;
                if (columnName == "XManualIndex")
                {
                    //if(0 <= index && index < 10)
                    //{
                    //    retVal = "Faaaaaaaaaaaaaail";
                    //}
                }
                return retVal;
            }
        }

    }
}
