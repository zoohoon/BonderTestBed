using RequestCore.QueryPack;
using RequestInterface;
using System;

namespace RequestCore.OperationPack
{
    [Serializable]
    public class CalculateData
    {
        public RequestBase LeftOperand
        {
            get;
            set;
        }
        public RequestBase RightOperand
        {
            get;
            set;
        }

        public CalculateData() { }
    }

    [Serializable]
    public abstract class Operation
    {
        public abstract QueryData GetResult();

        public Operation() { }
    }
}
