using LogModule;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ProberInterfaces
{
    public enum CylinderStateEnum
    {
        UNDEFINED = 0,
        RUNNING,
        JAM,
        OVERRUN,
        EXTEND,
        RETRACT,
        ERROR,
        ALARM,
    }

    public class CylinderStatus
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan DiffTime { get; set; }

        public CylinderStateEnum State { get; set; }

        public List<string> Extend_Input_keys { get; set; }
        public List<string> Retract_Input_keys { get; set; }
        public CylinderStatus()
        {
            try
            {
            StartTime = new DateTime();
            EndTime = new DateTime();
            DiffTime = new TimeSpan();

            Extend_Input_keys = new List<string>();
            Retract_Input_keys = new List<string>();

            State = CylinderStateEnum.UNDEFINED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        public void UpdateCylinderStatus(DateTime starttime, DateTime endtime, CylinderStateEnum state, List<string> ex_in_keys, List<string> re_in_keys)
        {
            try
            {
            this.StartTime = starttime;
            this.EndTime = endtime;

            this.DiffTime = endtime - starttime;

            this.State = state;
            this.Extend_Input_keys = ex_in_keys;
            this.Retract_Input_keys = re_in_keys;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
    }

    public interface ICylinderType : IStatisticsInfoProvider, INotifyPropertyChanged
    {
        int Extend();
        int Retract();

        IEnumerable<ICylinderType> Members { get; set; }
        //IEnumerable<IStatisticsElement> StatisticsMembers { get; }
        //CylinderIOParameter CylinderParam { get; set; }
        string GetDisplayName();

        void SetExtend_OutPut(IOPortDescripter<bool> io);
        void SetRetract_OutPut(IOPortDescripter<bool> io);
        void SetExtend_Input(List<IOPortDescripter<bool>> io);
        void SetRetract_Input(List<IOPortDescripter<bool>> io);
        void SetInterlock_Input(List<IOPortDescripter<bool>> io);
        void Init();

        string Name { get; set; }
        CylinderStateEnum State { get; set;}
        long CylinderLifeTime { get; set; }
        ProberInterfaces.PoVEnum PoV { get; set; }
        List<CylinderStatus> Cylinder_Extend_StatisticsInfo { get; set; }
        List<CylinderStatus> Cylinder_Retract_StatisticsInfo { get; set; }
        bool SetExtInterLock(Func<bool> extinterlock);

    }
    public interface IStageCylinderType : ICylinderType
    {
        
    }
    public interface ILoaderCylinderType : ICylinderType
    {

    }
    public interface IFoupCylinderType : ICylinderType
    {

    }
}
