namespace ServiceInterfaces
{
    using System.ServiceModel;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.ServiceHost;
    using Vision.GraphicsContext;

    [ServiceKnownType(typeof(DrawRectangleModule))]
    [ServiceKnownType(typeof(DrawEllipseModule))]
    [ServiceKnownType(typeof(DrawLineModule))]
    [ServiceKnownType(typeof(DrawTextModule))]
    [ServiceKnownType(typeof(DrawSVGPathModule))]
    
    [ServiceContract(CallbackContract = typeof(IImageDispHostCallback), SessionMode = SessionMode.Required)]
    public interface IImageDispHost
    {
        [OperationContract(IsOneWay = false)]
        void UpdateImage(ImageBuffer img);
        [OperationContract]
        void InitService(int chuckId);
        [OperationContract]
        bool IsServiceAvailable();
        [OperationContract]
        CommunicationState GetState();
        [OperationContract]
        EventCodeEnum Disconnect(int index = -1);
        IImageDispHostCallback GetDispHostClient(int index = -1);
    }


}
