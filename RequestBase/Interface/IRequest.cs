using ProberErrorCode;

namespace RequestInterface
{
    public interface IRequestArgument
    {
        string RequestName { get; set; }
    }

    public interface IRequest
    {
        object Argument { get; set; }
        object Result { get; }
        EventCodeEnum Run();
        object GetRequestResult();
    }
}
