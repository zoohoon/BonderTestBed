
using System;
using System.Threading.Tasks;

namespace WaferAlign
{
    using Autofac;
    using ProberInterfaces;
    using ProberInterfaces.Param;
    using ProberErrorCode;
    using ProberInterfaces.Vision;
    using System.Windows;
    using LogModule;

    public class WaferAlignEctFunction : IFactoryModule, IModule
    {
        #region Property
        private WaferAligner WaferAligner;
        #endregion

        //private ICamera Cam;

        public bool Initialized { get; set; } = false;

        public WaferAlignEctFunction(WaferAligner waferaligner)
        {
            WaferAligner = waferaligner;
        }
        public EventCodeEnum InitModule()
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

            return retval;
        }

        public void DeInitModule()
        {

        }

        public EventCodeEnum InitModule(IContainer container, object param)
        {
            throw new NotImplementedException();
        }


        #region //..CalCamRatio
        enum Direction
        {
            LeftTOP = 0,
            LeftBottom,
            RigthTop,
            RightBottom
        }
        /// <summary>
        /// 1.저장한 패턴 확인 용 패턴매칭
        /// 2. xml에 계산된 ratio 만큼 이동.
        /// </summary>
        public Task<int> CalCamRatio()
        {
            try
            {
                int retVal = -1;
                bool resultflag = false;
                PMResult preresult;
                PMResult result;
                Point prepoint;
                Point point;
                PMParameter pmparam = new PMParameter();

                Size size;
                string pmpath = this.FileManager().FileManagerParam.DeviceParamRootDirectory + "\\Vision" + "\\PMImage\\";
                string pattpath = pmpath + "_RefModel_BAndW.mmo";
                //string pattpath = @"C:\ProberSystem\Parameters\Vision\PMImage\_RefModel_BAndW.mmo";


                double ratiox = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM).GetRatioX();
                double ratioy = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM).GetRatioY();

                double acCanvaswidth = 645.5;  //window 의 canvas 값을 어떻게 받을지 수정.
                double acCanvasHeigth = 510.886; //

                //Task<int> waitForMotionDoneTask;
                double posx = 0.0;
                double posy = 0.0;
                double preXpos = 0.0;
                double preYpos = 0.0;
                double[] tempXRatio = new double[4];
                double[] tempYRatio = new double[4];
                double resultratiox = 0.0;
                double resultratioy = 0.0;
                double positionx = 0.0;
                double positiony = 0.0;
                double destx = 0.0;
                double desty = 0.0;
                int index = (int)Direction.LeftTOP;


                this.MotionManager().GetActualPos(EnumAxisConstants.X, ref positionx);
                this.MotionManager().GetActualPos(EnumAxisConstants.Y, ref positiony);
                //Task<int> xMoveTask;
                //Task<int> yMoveTask;
                int iter = 0;

                this.VisionManager().GetPatternSize(pattpath, out size);

                posx = ((acCanvaswidth - size.Width) / 5) / ratiox;
                posy = ((acCanvasHeigth - size.Height) / 5) / ratioy;

                preresult = this.VisionManager().PatternMatching(new PatternInfomation(EnumProberCam.WAFER_HIGH_CAM, pmparam, pmpath), this);  //640,480 은 Window Image Control 값 . 수정 해야함.

                prepoint = new Point(preresult.ResultParam[0].XPoss, preresult.ResultParam[0].YPoss);
                //waitForMotionDoneTask = Task.Run(() =>
                while (!resultflag & iter < 5)
                {
                    switch (index)
                    {
                        case (int)Direction.LeftTOP:
                            destx = positionx - posx;
                            desty = positiony - posy;
                            break;
                        case (int)Direction.LeftBottom:
                            destx = positionx - posx;
                            desty = positiony + posy;
                            break;
                        case (int)Direction.RigthTop:
                            destx = positionx + posx;
                            desty = positiony - posy;
                            break;
                        case (int)Direction.RightBottom:
                            destx = positionx + posx;
                            desty = positiony + posy;
                            break;
                    }
                    var axisX = this.MotionManager().GetAxis(EnumAxisConstants.X);
                    var axisY = this.MotionManager().GetAxis(EnumAxisConstants.Y);

                    //xMoveTask = this.MotionManager().AbsMoveAsync(
                    //        axisX,
                    //        destx, 1500000, 15000000, 10000);

                    //yMoveTask = this.MotionManager().AbsMoveAsync(
                    //    axisY,
                    //    desty, 1500000, 15000000, 10000);

                    this.MotionManager().StageMove(destx, desty);

                    LoggerManager.Debug($"CalCamRatio(): Move to position ({posx:0.00}, {posy:0.00})");

                    this.MotionManager().GetActualPos(EnumAxisConstants.X, ref preXpos);
                    this.MotionManager().GetActualPos(EnumAxisConstants.Y, ref preYpos);

                    result = this.VisionManager().PatternMatching(new PatternInfomation(EnumProberCam.WAFER_HIGH_CAM, pmparam, pmpath), this);

                    if (result.ResultParam.Count > 0)
                    {
                        point = new Point(result.ResultParam[0].XPoss, result.ResultParam[0].YPoss);
                        tempXRatio[index] = Math.Abs(preXpos) - Math.Abs(positionx);
                        tempYRatio[index] = Math.Abs(preYpos) - Math.Abs(positiony);
                        tempXRatio[index] = Math.Abs(tempXRatio[index] / (Math.Abs(point.X)
                            - Math.Abs(prepoint.X)));
                        tempYRatio[index] = Math.Abs(tempYRatio[index] / (Math.Abs(point.Y)
                            - Math.Abs(prepoint.Y)));
                        index++;
                    }
                    else
                    {
                        posx = ((acCanvaswidth - size.Width) / 5) / (ratiox - 0.2);
                        posy = ((acCanvasHeigth - size.Height) / 5) / (ratioy - 0.2);
                        resultflag = false;
                    }
                    iter++;
                    if (index == 4) resultflag = true;
                }
                for (int count = 0; count < tempXRatio.Length; count++)
                {
                    resultratiox += tempXRatio[count];
                    resultratioy += tempYRatio[count];
                }
                resultratiox /= 4;
                resultratioy /= 4;

                double RatioX;
                double RatioY;

                RatioX = resultratiox;
                RatioY = resultratioy;

                ///오차 구하기.
                ///오차범위를 벗어나는 평균값이 나왔다면 ratio값을 조절해서 다시 매칭.
                if ((Math.Abs(resultratiox) - Math.Abs(resultratioy)) > 0.1)
                {

                }


                //);
                //await waitForMotionDoneTask;
                return Task.FromResult<int>(retVal);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }
        #endregion

        #region //..Calculation

        public void CalWaferTheta()
        {
            try
            {
                PMParameter pmparm = new PMParameter();
                PMResult preresult;
                PMResult result;
                Point prepoint;
                Point point;
                //1.패턴매칭
                string pmpath = this.FileManager().FileManagerParam.DeviceParamRootDirectory + "\\Vision" + "\\PMImage\\";
                //string pmpath = @"C:\ProberSystem\Parameters\Vision\PMImage\";
                
                PatternInfomation ptinfo = new PatternInfomation(EnumProberCam.WAFER_HIGH_CAM, pmparm, pmpath);
                preresult = this.VisionManager().PatternMatching(ptinfo, this);

                //2.모션 각도 조정 후 패턴 매칭
                var axisT = this.MotionManager().GetAxis(EnumAxisConstants.C);
                this.MotionManager().RelMove(axisT, -4394, axisT.Param.Speed.Value, axisT.Param.Acceleration.Value);

                result = this.VisionManager().PatternMatching(ptinfo, this);

                prepoint = new Point(preresult.ResultParam[0].XPoss, preresult.ResultParam[0].YPoss);
                point = new Point(result.ResultParam[0].XPoss, result.ResultParam[0].YPoss);

                if ((Math.Round(prepoint.X) != Math.Round(point.X)) ||
                    (Math.Round(prepoint.Y) != Math.Round(point.Y)))
                {
                    double offsetx = Math.Round(prepoint.X) - Math.Round(point.X);
                    double offsety = Math.Tan(10) * offsetx;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        //public async void CalDieSize()
        //{
        //    //await EctFunction.CalDieSize(this.VisionManager().CurCamera.Param.ChannelType);
        //}



        #endregion
    }
}
