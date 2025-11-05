using System;
using System.Threading.Tasks;

namespace CoordinateSystem.CameraPositionResolver
{
    using LogModule;
    using ProbeCardObject;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Param;

    public class PinLowMove : CameraMoveBase
    {
        public override bool Initialized { get; set; } = false;

        public PinLowMove()
        {
        }

        public override EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
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

            //ZeroPosition.X.Value = this.CoordinateManager().StageCoord.MarkPosInChuckCoord.X.Value +
            //    this.CoordinateManager().StageCoord.PHOffset.X.Value +
            //    this.CoordinateManager().StageCoord.PLCAMFromPH.X.Value;
            //ZeroPosition.Y.Value = this.CoordinateManager().StageCoord.MarkPosInChuckCoord.Y.Value +
            //    this.CoordinateManager().StageCoord.PHOffset.Y.Value +
            //    this.CoordinateManager().StageCoord.PLCAMFromPH.Y.Value;
            //ZeroPosition.Z.Value = this.CoordinateManager().StageCoord.MarkPosInChuckCoord.Z.Value + //==> Chuck Center에서 Mark 까지 이동
            //    this.CoordinateManager().StageCoord.PHOffset.Z.Value +                                //==> Mark 에서 PH 까지 이동;
            //    this.CoordinateManager().StageCoord.PLCAMFromPH.Z.Value -                             //==> PH 에서 PL 까지 이동
            //    this.PinAligner()..AlignInfo.PinAlignParam.PinHeight;
            //ZeroPosition.X.Value *= -1;
            //ZeroPosition.Y.Value *= -1;
            //ZeroPosition.Z.Value *= -1;
            return retval;
        }

        public async override Task<int> MoveAsync(
            double xpos = 0, double ypos = 0, double zpos = 0, double tpos = 0)
        {
            var axisX = this.MotionManager().GetAxis(EnumAxisConstants.X);
            var axisY = this.MotionManager().GetAxis(EnumAxisConstants.Y);
            var axisZ = this.MotionManager().GetAxis(EnumAxisConstants.Z);
            double zclearence = -10000;
            try
            {
                double zActualpos = 0.0;

                PinAlignDevParameters PinAlignParam = this.PinAligner().PinAlignDevParam as PinAlignDevParameters;
                //PinAlignInfo AlignInfo = (PinAlignInfo)this.PinAligner().AlignInfo;

                if (this.CoordinateManager().StageCoord.PinReg.PinRegMin.Value > PinAlignParam.PinHeight.Value
                    || PinAlignParam.PinHeight.Value > this.CoordinateManager().StageCoord.PinReg.PinRegMax.Value)
                {
                    // Exception 처리 
                    return -1;
                }

                //double PHFromMarkPosZ = this.CoordinateManager().StageCoord.MarkPosInChuckCoord.Z.Value + //==> Chuck Center에서 Mark 까지 이동
                //    this.CoordinateManager().StageCoord.PHOffset.Z.Value +                                //==> Mark 에서 PH 까지 이동;
                //    this.CoordinateManager().StageCoord.PLCAMFromPH.Z.Value -                             //==> PH 에서 PL 까지 이동
                //    this.PinAligner()..AlignInfo.PinAlignParam.PinHeight;                                            //==> Pin과의 충돌을 방지하기 위해 Pin Height만큼 Chuck을 내림

                //double PHFromMarkPosX = this.CoordinateManager().StageCoord.MarkPosInChuckCoord.X.Value +
                //    this.CoordinateManager().StageCoord.PHOffset.X.Value +
                //    this.CoordinateManager().StageCoord.PLCAMFromPH.X.Value;

                //double PHFromMarkPosY = this.CoordinateManager().StageCoord.MarkPosInChuckCoord.Y.Value +
                //    this.CoordinateManager().StageCoord.PHOffset.Y.Value +
                //    this.CoordinateManager().StageCoord.PLCAMFromPH.Y.Value;

                //zpos -= PHFromMarkPosZ;
                //xpos -= PHFromMarkPosX;
                //ypos -= PHFromMarkPosY;
                MachineCoordinate machine = new MachineCoordinate();
                PinCoordinate pincoord = new PinCoordinate();
                pincoord.X.Value = xpos;
                pincoord.Y.Value = ypos;
                pincoord.Z.Value = zpos;

                machine = this.CoordinateManager().PinLowPinConvert.ConvertBack(pincoord);


                this.MotionManager().GetActualPos(EnumAxisConstants.Z, ref zActualpos);
                if (zclearence < zActualpos)
                {
                    this.MotionManager().AbsMove(axisZ,
                        zclearence,
                        axisZ.Param.Speed.Value,
                        axisZ.Param.Acceleration.Value);
                }

                await this.MotionManager().WaitForMotionDoneAsync();

                this.MotionManager().AbsMove(axisX,
                    machine.X.Value,
                    axisX.Param.Speed.Value,
                    axisX.Param.Acceleration.Value);

                this.MotionManager().AbsMove(axisY,
                    machine.Y.Value,
                    axisY.Param.Speed.Value,
                    axisY.Param.Acceleration.Value);

                await this.MotionManager().WaitForMotionDoneAsync();

                this.MotionManager().AbsMove(axisZ,
                    machine.Z.Value,
                    axisZ.Param.Speed.Value,
                    axisZ.Param.Acceleration.Value);

                await this.MotionManager().WaitForMotionDoneAsync();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return 0;

        }

        public void ConvertPosition(ref double x, ref double y, ref double z)
        {
            try
            {
                //PinAlignInfo AlignInfo = (PinAlignInfo)this.PinAligner().AlignInfo;

                PinAlignDevParameters PinAlignParam = this.PinAligner().PinAlignDevParam as PinAlignDevParameters;

                double PHFromMarkPosZ = this.CoordinateManager().StageCoord.MarkPosInChuckCoord.Z.Value + //==> Chuck Center에서 Mark 까지 이동
                    this.CoordinateManager().StageCoord.PHOffset.Z.Value +                                //==> Mark 에서 PH 까지 이동;
                    this.CoordinateManager().StageCoord.PLCAMFromPH.Z.Value -                             //==> PH 에서 PL 까지 이동
                    PinAlignParam.PinHeight.Value;                                            //==> Pin과의 충돌을 방지하기 위해 Pin Height만큼 Chuck을 내림

                double PHFromMarkPosX = this.CoordinateManager().StageCoord.MarkPosInChuckCoord.X.Value +
                    this.CoordinateManager().StageCoord.PHOffset.X.Value +
                    this.CoordinateManager().StageCoord.PLCAMFromPH.X.Value;

                double PHFromMarkPosY = this.CoordinateManager().StageCoord.MarkPosInChuckCoord.Y.Value +
                    this.CoordinateManager().StageCoord.PHOffset.Y.Value +
                    this.CoordinateManager().StageCoord.PLCAMFromPH.Y.Value;


                z += PHFromMarkPosZ;
                x += PHFromMarkPosX;
                y += PHFromMarkPosY;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override Task<int> IndexMove(double indexsizex, double indexsizey, int movex,
            int movey, double squarness = 0, double zpos = 0)
        {
            throw new NotImplementedException();
        }

        public override Task<int> RelMove(double diesizex, double diesizey, int movex, int movey, double squarness = 0, double zpos = 0)
        {
            throw new NotImplementedException();
        }

    }
}
