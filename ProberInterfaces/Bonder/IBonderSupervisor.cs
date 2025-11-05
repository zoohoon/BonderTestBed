using ProberErrorCode;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProberInterfaces
{
    // 251013 sebas
    public interface IBonderSupervisor : IFactoryModule, IHasDevParameterizable, IHasSysParameterizable, INotifyPropertyChanged
    {
        IBonderMove BonderModuleState { get; }
    }
    public interface IBonderMove : IModule, IFactoryModule
    {
        BonderStateEnum GetState();
        EventCodeEnum FDStage_VacuumOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0);
        EventCodeEnum MovePickPosition(double offsetvalue); // FD Stage XY move
        EventCodeEnum MoveEjPickPosition(double offsetvalue);   // Ejection XY move
        EventCodeEnum MovePickZMove(bool updown); // FD Stage Z move , true(Up) , false(Down)
        EventCodeEnum MoveEjPickZMove(bool updown); // FD Stage Z move , true(Up) , false(Down)
        EventCodeEnum MovePinZMove(bool updown);    // FD Stage Pin Z up , 1 rotation = 1 up & down
        EventCodeEnum EjPin_VacuumOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0);  // Ejection Pin
        EventCodeEnum Arm1_VacuumOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0);  // ARM1
        EventCodeEnum Arm2_VacuumOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0);  // ARM2
        EventCodeEnum Arm1_BlowOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0);  // ARM1
        EventCodeEnum Arm2_BlowOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0);  // ARM2
        EventCodeEnum MoveRotation();
        EventCodeEnum MovePlacePosition(double offsetvalue); // Wafer Stage XY move
        EventCodeEnum MovePlaceZMove(bool updown); // Wafer Stage Z move , true(Up) , false(Down)
        EventCodeEnum MagneticOnOff(bool val, long timeout = 0);
        EventCodeEnum MoveNanoZMove(bool updown); // Nano Stage Z move , true(Up) , false(Down)
        EventCodeEnum Arm1_AirOnOff(bool val, long timeout = 0);
        EventCodeEnum Arm2_AirOnOff(bool val, long timeout = 0);
    }
    public abstract class IBonderState
    {
        public abstract BonderStateEnum GetState();
        public abstract EventCodeEnum FDStage_VacuumOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0);
        public abstract EventCodeEnum MovePickPosition(double offsetvalue);
        public abstract EventCodeEnum MoveEjPickPosition(double offsetvalue);
        public abstract EventCodeEnum MovePickZMove(bool updown);
        public abstract EventCodeEnum MoveEjPickZMove(bool updown);
        public abstract EventCodeEnum MovePinZMove(bool updown);
        public abstract EventCodeEnum EjPin_VacuumOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0);
        public abstract EventCodeEnum Arm1_VacuumOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0);
        public abstract EventCodeEnum Arm2_VacuumOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0);
        public abstract EventCodeEnum Arm1_BlowOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0);
        public abstract EventCodeEnum Arm2_BlowOnOff(bool val, bool extraVacReady, bool extraVacOn = true, long timeout = 0);
        public abstract EventCodeEnum MoveRotation();
        public abstract EventCodeEnum MovePlacePosition(double offsetvalue);
        public abstract EventCodeEnum MovePlaceZMove(bool updown);
        public abstract EventCodeEnum MagneticOnOff(bool val, long timeout = 0);
        public abstract EventCodeEnum MoveNanoZMove(bool updown);
        public abstract EventCodeEnum Arm1_AirOnOff(bool val, long timeout = 0);
        public abstract EventCodeEnum Arm2_AirOnOff(bool val, long timeout = 0);
    }
    public enum BonderStateEnum
    {
        UNKNOWN = 0,
        ERROR,
        IDLE,
        MOVETOPICKPOS,
        PICKZUP,
        PICKZDOWN,
        PINZUP,
        PINZDOWN,
        MOVETOPLACEPOS,
        FDHIGHVIEW,
        FDLOWVIEW,
        PICKING,
        ROTATING,
        PLACING,
        MANUAL,
        LOCK
    }
}
