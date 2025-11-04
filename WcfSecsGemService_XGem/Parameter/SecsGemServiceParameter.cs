namespace WcfSecsGemService_XGem
{
    public class SecsGemServiceParameter
    {
        private string _ReceiveMessageType;

        public string ReceiveMessageType
        {
            get { return _ReceiveMessageType; }
            set { _ReceiveMessageType = value; }
        }

        public SecsGemServiceParameter()
        {

        }

        public void SetDefault()
        {
            ReceiveMessageType = "";
        }
    }
}
