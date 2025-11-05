using System;
using System.Threading.Tasks;
using Autofac;
using ProberInterfaces.Param;
using System.Xml.Serialization;
using ProberErrorCode;
using Newtonsoft.Json;

namespace ProberInterfaces
{
    public abstract class CameraMoveBase : IFactoryModule, IModule
    {
        protected CatCoordinates _ZeroPosition = new CatCoordinates();
        [XmlIgnore, JsonIgnore]
        public CatCoordinates ZeroPosition
        {
            get { return _ZeroPosition; }
            set { _ZeroPosition = value; }
        }

        public abstract bool Initialized { get; set; }
        public abstract EventCodeEnum InitModule();
        abstract public Task<int> MoveAsync(
            double xpos=0, double ypos=0,double zpos =0,double tpos=0);


        abstract public Task<int> RelMove(double diesizex, double diesizey, int movex,
            int movey, double squarness = 0, double zpos = 0);

        abstract public Task<int> IndexMove(double diesizex, double diesizey,
            int movex, int movey,double squarness =0,double zpos=0);

        public EventCodeEnum InitModule(IContainer container, object param)
        {
            throw new NotImplementedException();
        }
        public void DeInitModule()
        {

        }
    }
}
