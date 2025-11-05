using System;
using System.Xml.Serialization;
using ProberErrorCode;

namespace RequestCore.QueryPack
{
    [Serializable]
    public abstract class QueryData : Query
    {
        public abstract override EventCodeEnum Run();
    }

    [Serializable]
    public class FixData : QueryData
    {
        public string ResultData { set { this.Result = value; } }

        public override EventCodeEnum Run()
        {
            return EventCodeEnum.NONE;
        }
    }

    [Serializable]
    public abstract class ArrayData : QueryData
    {
        [XmlAttribute]
        public int Index { get; set; }

        public string ResultData { set { this.Result = value; } }

        public abstract override EventCodeEnum Run();
    }

    //public class ObservableArrayData : ArrayData
    //{
    //    int[] testInt = new int[] { 1, 2, 3 };

    //    protected override EventCodeEnum Run()
    //    {
    //        Result = testInt[Index].ToString();

    //        return EventCodeEnum.NONE;
    //    }
    //}
}
