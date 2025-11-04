namespace RequestInterface
{
    public abstract class RequestArgument : IRequestArgument
    {
        public string RequestName { get; set; }
    }

    public class RequestArgument<T> : RequestArgument
    {
        public T Argument { get; set; }
    }
}
