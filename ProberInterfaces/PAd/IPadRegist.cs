using Autofac;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProberInterfaces.Pad
{
    using ProberErrorCode;
    using ProberInterfaces.Param;
    using ProberInterfaces.Vision;

    public interface IPadRegist : IFactoryModule, IGraphicsContext, IModule
    {
        //Pads Pads { get; set; }
        void AddPad(PadObject padparameter, WaferCoordinate curcoord, ROIParameter roiparameter);
        void DeletePad(PadObject padparameter, WaferCoordinate curcoord, ROIParameter roiparam);
        void AddPad(PadObject padparameter, double xpos, double ypos);
        void MoveToPad(PadObject padparam, ROIParameter roiparam);
        Task<EventCodeEnum> AllPadSearch();
        int CheckPadPosition(PadObject padparam);
        int CheckPadTolerance(PadObject padparam);
        void UpdateDrawList(GraphicsParam r);
        //int SavePadParam(string ParamPath);
        //EventCodeEnum LoadPadParam();
        //EventCodeEnum SavePadParam();
        void PrevPad();
        void NextPad();
        void FindPad();
        Task<EventCodeEnum> DieAllSearch();
        void SortPadPositionList(List<DUTPadObject> padinfo);
    }

    public interface IPadRegistProcessing : IFactoryModule, IModule
    {
        Task<EventCodeEnum> Run();
    }

    public abstract class PadSearchBase : IPadRegistProcessing
    {
        public abstract bool Initialized { get; set; }
        public abstract EventCodeEnum InitModule();
        public abstract void DeInitModule();
        public EventCodeEnum InitModule(IContainer container, object param)
        {
            throw new NotImplementedException();
        }

        public abstract Task<EventCodeEnum> Run();
    }
}
