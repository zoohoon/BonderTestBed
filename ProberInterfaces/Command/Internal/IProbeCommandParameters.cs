namespace ProberInterfaces.Command.Internal
{
    public class PositionParam : IProbeCommandParameter
    {
        public string Command { get; set; }

        private long _X;
        public long X
        {
            get { return _X; }
            set { _X = value; }
        }
        private long _Y;
        public long Y
        {
            get { return _Y; }
            set { _Y = value; }
        }
    }

    public class NoHavePositionParam : PositionParam
    { }
}
