//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//using ProberInterfaces.Error;

//namespace ProberInterfaces.FoupController
//{
//    public enum FoupScheduleTypeEnum
//    {
//        NONE,
//        SEQUENTIAL,
//    }

//    public interface IFoupScheduleProvider
//    {
//        IFoupScheduleService Service { get; }

//        void Init(Autofac.IContainer container);

//        void Deinit();
//    }

//    public interface IFoupScheduleService
//    {
//        void Init(Autofac.IContainer container);

//        void Deinit();

//        EventCodeEnum Run();
//    }
//}
