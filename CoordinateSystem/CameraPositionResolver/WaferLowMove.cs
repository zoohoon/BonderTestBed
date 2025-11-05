using System;

namespace CoordinateSystem.CameraPositionResolver
{
    using ProberInterfaces;
    using System.Threading.Tasks;
    using ProberInterfaces.Param;
    using ProberErrorCode;
    using LogModule;

    public class WaferLowMove : CameraMoveBase
    {
        public override bool Initialized { get; set; } = false;

        public WaferLowMove()
        {
        }

        public override EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    ZeroPosition.X.Value += this.CoordinateManager().StageCoord.WHOffset.X.Value +
                        this.CoordinateManager().StageCoord.WLCAMFromWH.X.Value;
                    ZeroPosition.Y.Value += this.CoordinateManager().StageCoord.WHOffset.Y.Value +
                                            this.CoordinateManager().StageCoord.WLCAMFromWH.Y.Value;
                    ZeroPosition.Z.Value += this.CoordinateManager().StageCoord.WHOffset.Z.Value +
                                            this.CoordinateManager().StageCoord.WLCAMFromWH.Z.Value;

                    Initialized = true;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override Task<int> MoveAsync(
            double xpos = 0, double ypos = 0, double zpos = 0, double tpos = 0)
        {
            var axisX = this.MotionManager().GetAxis(EnumAxisConstants.X);
            var axisY = this.MotionManager().GetAxis(EnumAxisConstants.Y);
            var axisZ = this.MotionManager().GetAxis(EnumAxisConstants.Z);
            double zclearence = -10000;
            try
            {
                double zActualpos = 0.0;

                WaferCoordinate waferlow = new WaferCoordinate();
                MachineCoordinate machine = new MachineCoordinate();

                waferlow.X.Value = xpos;
                waferlow.Y.Value = ypos;
                waferlow.Z.Value = zpos * -1;

                //zpos = zpos * -1d;
                //zpos += this.CoordinateManager().StageCoord.WHOffset.Z.Value
                //     + this.CoordinateManager().StageCoord.WLCAMFromWH.Z.Value;
                //xpos += this.CoordinateManager().StageCoord.WHOffset.X.Value
                //     + this.CoordinateManager().StageCoord.WLCAMFromWH.X.Value;
                //ypos += this.CoordinateManager().StageCoord.WHOffset.Y.Value
                //     + this.CoordinateManager().StageCoord.WLCAMFromWH.Y.Value;

                //WaferAligner.HeightSearchIndex(xpos, ypos);
                //zpos = WaferAligner.GetHeightValue(xpos, ypos);

                machine = this.CoordinateManager().WaferLowChuckConvert.ConvertBack(waferlow);


                this.MotionManager().GetActualPos(EnumAxisConstants.Z, ref zActualpos);
                if (zclearence < zActualpos)
                {
                    var taskzclearance = this.MotionManager().AbsMove(axisZ,
                     zclearence,
                     axisZ.Param.Speed.Value,
                     axisZ.Param.Acceleration.Value);
                }

                var rel = this.MotionManager().AbsMove(axisZ,
                    machine.Z.Value,
                    axisZ.Param.Speed.Value,
                    axisZ.Param.Acceleration.Value);


                this.MotionManager().StageMove(machine.X.Value, machine.Y.Value);
                //var taskx =  this.MotionManager().AbsMoveAsync(axisX,
                //     machine.X.Value,
                //     axisX.Param.Speed.Value,
                //     axisX.Param.Acceleration.Value);

                // var tasky =  this.MotionManager().AbsMoveAsync(axisY,
                //     machine.Y.Value,
                //     axisY.Param.Speed.Value,
                //     axisY.Param.Acceleration.Value);

                //var relarr = Task.WhenAll(taskx, tasky).Result;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<int>(0);
        }

        public override Task<int> IndexMove(double indexsizex, double indexsizey, int movex,
           int movey, double squarness = 0, double zpos = 0)
        {
            var axisX = this.MotionManager().GetAxis(EnumAxisConstants.X);
            var axisY = this.MotionManager().GetAxis(EnumAxisConstants.Y);
            var axisZ = this.MotionManager().GetAxis(EnumAxisConstants.Z);

            double zclearence = -10000;
            try
            {
                double zActualpos = 0.0;

                zpos = zpos * -1d;
                double xpos = indexsizex * movex;
                double ypos = indexsizey * movey;

                double curxpos = 0.0;
                this.MotionManager().GetActualPos(EnumAxisConstants.X, ref curxpos);
                double curypos = 0.0;
                this.MotionManager().GetActualPos(EnumAxisConstants.Y, ref curypos);

                squarness = this.WaferAligner().GetReviseSquarness(curxpos + xpos, curypos + ypos);

                this.MotionManager().GetActualPos(EnumAxisConstants.Z, ref zActualpos);
                if (zclearence < zActualpos)
                {
                    this.MotionManager().AbsMove(axisZ,
                    zclearence,
                    axisZ.Param.Speed.Value,
                    axisZ.Param.Acceleration.Value);
                }

                if (xpos != 0)
                {
                    this.MotionManager().RelMoveAsync(axisX,
                        (xpos),
                        axisX.Param.Speed.Value,
                        axisX.Param.Acceleration.Value);
                }

                if (ypos != 0)
                {
                    this.MotionManager().RelMoveAsync(axisY,
                      (ypos),
                      axisY.Param.Speed.Value,
                      axisY.Param.Acceleration.Value);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<int>(0);
        }

        public override Task<int> RelMove(double diesizex, double diesizey, int movex,
            int movey, double squarness = 0, double zpos = 0)
        {
            var axisX = this.MotionManager().GetAxis(EnumAxisConstants.X);
            var axisY = this.MotionManager().GetAxis(EnumAxisConstants.Y);
            var axisZ = this.MotionManager().GetAxis(EnumAxisConstants.Z);

            double zclearence = -10000;
            try
            {
                double zActualpos = 0.0;

                zpos = zpos * -1d;
                double xpos = diesizex * movex;
                double ypos = diesizey * movey;

                double curxpos = 0.0;
                this.MotionManager().GetActualPos(EnumAxisConstants.X, ref curxpos);
                double curypos = 0.0;
                this.MotionManager().GetActualPos(EnumAxisConstants.Y, ref curypos);

                squarness = this.WaferAligner().GetReviseSquarness(curxpos + xpos, curypos + ypos);

                this.MotionManager().GetActualPos(EnumAxisConstants.Z, ref zActualpos);
                if (zclearence < zActualpos)
                {
                    this.MotionManager().AbsMove(axisZ,
                    zclearence,
                    axisZ.Param.Speed.Value,
                    axisZ.Param.Acceleration.Value);
                }

                if (xpos != 0)
                {
                    this.MotionManager().RelMove(axisX,
                        (diesizex),
                        axisX.Param.Speed.Value,
                        axisX.Param.Acceleration.Value);
                }

                if (ypos != 0)
                {
                    this.MotionManager().RelMove(axisY,
                      (diesizey),
                      axisY.Param.Speed.Value,
                      axisY.Param.Acceleration.Value);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<int>(0);
        }


    }
}
