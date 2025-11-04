using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace PadProcModule
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Pad;
    using ProberInterfaces.Param;
    using ProberInterfaces.Vision;
    using SubstrateObjects;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    [Serializable]
    public class PadSearchStandard : PadSearchBase, INotifyPropertyChanged
    {
        public override bool Initialized { get; set; } = false;

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private IStageSupervisor StageSupervisor;

        private PadGroup _Pads;
        public PadGroup Pads
        {
            get { return _Pads; }
            set
            {
                if (value != _Pads)
                {
                    _Pads = value;
                    RaisePropertyChanged();
                }
            }
        }

        private WaferObject Wafer { get { return (WaferObject)this.StageSupervisor().WaferObject; } }

        public override EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                StageSupervisor = this.StageSupervisor();
                
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        //Die 외각에 존재하는 Pad Search(Standard)
        public override Task<EventCodeEnum> Run()
        {
            //string ParamPath = string.Empty;
            //_Container.Resolve<IFileManager>().GetXMLFilePath(typeof(PadGroup), out ParamPath);

            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                ROIParameter roiparam = new ROIParameter();

                //this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM).StopGrab();

                double MoveXNum;
                double MoveYNum;

                //double curXpos = 0.0;
                //double curYpos = 0.0;

                double NextPosX = 0.0;
                double NextPosY = 0.0;

                double DefaultMovingPosX;
                double DefaultMovingPosY;

                // Pixel
                double EdgeMarginX = 5;
                double EdgeMarginY = 5;

                ICamera cam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);

                //현재는 패드 탐색 영역이 가변적으로 변하게 되어있지만, 480*480 영역에 패드 사이즈만큼을 더 주어 동일한 크기로 이동하도록 코드 수정해야함.(패드 사이즈를 알고있다는 가정하에)
                DefaultMovingPosX = (cam.GetGrabSizeWidth() * cam.GetRatioX()) - Pads.SetPadObject.PadSizeX.Value - (EdgeMarginX * cam.GetRatioX());
                DefaultMovingPosY = (cam.GetGrabSizeHeight() * cam.GetRatioY()) - Pads.SetPadObject.PadSizeY.Value - (EdgeMarginY * cam.GetRatioY());

                // Current Position
                WaferCoordinate padwafercoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();

                WaferCoordinate DieEndPos = new WaferCoordinate();
                ImageBuffer GetCurGrabImage = new ImageBuffer();

                //DieEndPos.X.Value = padwafercoord.X.Value - this.WaferAligner().WAInfoFile.DieSizeActualX;
                //DieEndPos.Y.Value = padwafercoord.Y.Value - this.WaferAligner().WAInfoFile.DieSizeActualY;

                //MoveXNum = Math.Round((this.WaferAligner().WAInfoFile.DieSizeActualX / (cam.GetGrabSizeWidth() * cam.GetRatioX())));
                //MoveYNum = Math.Round((this.WaferAligner().WAInfoFile.DieSizeActualY / (cam.GetGrabSizeHeight() * cam.GetRatioY())));


                DieEndPos.X.Value = padwafercoord.X.Value - Wafer.GetSubsInfo().ActualDieSize.Width.Value;
                DieEndPos.Y.Value = padwafercoord.Y.Value - Wafer.GetSubsInfo().ActualDieSize.Height.Value;

                MoveXNum = Math.Round((Wafer.GetSubsInfo().ActualDieSize.Width.Value / 
                    (cam.GetGrabSizeWidth() * cam.GetRatioX())));
                MoveYNum = Math.Round((Wafer.GetSubsInfo().ActualDieSize.Height.Value /
                    (cam.GetGrabSizeHeight() * cam.GetRatioY())));

                //int Xsign = -1;
                //int Ysign = -1;

                //padwafercoord.X.Value -= (cam.CameraParameters.GrabSizeX.Value * cam.CameraParameters.RatioX.Value) / 2;
                //padwafercoord.Y.Value -= (cam.CameraParameters.GrabSizeY.Value * cam.CameraParameters.RatioY.Value) / 2;

                roiparam.OffsetX.Value = 0;
                roiparam.OffsetY.Value = 0;

                roiparam.Width.Value = (int)cam.GetGrabSizeWidth();
                roiparam.Height.Value = (int)cam.GetGrabSizeHeight();

                //this.VisionManager().DigitizerService[cam.CameraParameters.DigiNumber].BlockGrabEvent();

                // Start
                LoggerManager.Debug($"Die Corner Start pos : {padwafercoord.X.Value}, {padwafercoord.Y.Value}");

                // End
                LoggerManager.Debug($"Die Corner End pos : {DieEndPos.X.Value}, {DieEndPos.Y.Value}");

                //WaferAlign이 안되서 임시방편으로 만듬
                WaferCoordinate CurDieLeftCorner = new WaferCoordinate();
                Point pt = this.WaferAligner().GetLeftCornerPosition(padwafercoord.X.Value, padwafercoord.Y.Value);

                CurDieLeftCorner.X.Value = pt.X;
                CurDieLeftCorner.Y.Value = pt.Y;


                double limitX = 0.0;
                double limitY = 0.0;
                // CurDieCenter.Y.Value = -5721;  

                //CurDieCenter = this.CoordinateManager().GetCurDieCenterPos();
                double movex = 0.0;
                double movey = 0.0;

                //오른쪽 방향
                //limitX = CurDieLeftCorner.X.Value + WaferAlinger.WAInfoFile.DieSizeActualX - WaferAlinger.WAInfoFile.DieXClearance;
                limitX = CurDieLeftCorner.X.Value + Wafer.GetSubsInfo().ActualDeviceSize.Width.Value;
                do
                {
                    padwafercoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                    GetCurGrabImage = this.VisionManager().SingleGrab(cam.GetChannelType(), this);
                    ObservableCollection<GrabDevPosition> position = new ObservableCollection<GrabDevPosition>();
                    position = this.VisionManager().Detect_Pad(GetCurGrabImage, Pads.SetPadObject.PadSizeX.Value, Pads.SetPadObject.PadSizeY.Value, roiparam);

                    if (position.Count == 0)
                    {
                        NextPosX = padwafercoord.X.Value + DefaultMovingPosX;
                        NextPosY = DefaultMovingPosY;

                        StageSupervisor.StageModuleState.WaferHighViewMove(NextPosX, padwafercoord.Y.Value, Wafer.GetPhysInfo().Thickness.Value);

                    }

                    else if (position.Count >= 1)
                    {
                        double offsetx = position[0].PosX - (cam.GetGrabSizeWidth() / 2);

                        if (position[0].PosX > (cam.GetGrabSizeWidth() / 2))
                        {
                            offsetx = position[0].PosX - (cam.GetGrabSizeWidth() / 2);
                        }
                        else
                        {
                            offsetx = (cam.GetGrabSizeWidth() / 2) - position[0].PosX;
                            offsetx *= -1;
                        }

                        movex = padwafercoord.X.Value + (offsetx * cam.GetRatioX()) +
                        ((cam.GetGrabSizeWidth() / 2) * cam.GetRatioX());

                        StageSupervisor.StageModuleState.WaferHighViewMove(movex, padwafercoord.Y.Value, Wafer.GetPhysInfo().Thickness.Value);
                    }

                    for (int count = 0; count < position.Count(); count++)
                    {
                        PadObject padparam = new PadObject();

                        padparam.PadSizeX.Value = position[count].SizeX;
                        padparam.PadSizeY.Value = position[count].SizeY;

                        double pixeloffsetx = 0.0;
                        double pixeloffsety = 0.0;

                        if (position[count].PosX > (cam.GetGrabSizeWidth() / 2))
                        {
                            pixeloffsetx = position[count].PosX - (cam.GetGrabSizeWidth() / 2);
                        }
                        else
                        {
                            pixeloffsetx = (cam.GetGrabSizeWidth() / 2) - position[count].PosX;
                            pixeloffsetx *= -1;
                        }

                        if (position[count].PosY < (cam.GetGrabSizeHeight() / 2))
                        {
                            pixeloffsety = position[count].PosY - (cam.GetGrabSizeHeight() / 2);
                            pixeloffsety *= -1;
                        }
                        else
                        {
                            pixeloffsety = (cam.GetGrabSizeHeight() / 2) - position[count].PosY;
                        }

                        double pixelpadx = padwafercoord.X.Value + (pixeloffsetx * cam.GetRatioX());
                        double pixelpady = padwafercoord.Y.Value + (pixeloffsety * cam.GetRatioY());

                        padparam.PadCenter.X.Value = pixelpadx - CurDieLeftCorner.X.Value;
                        padparam.PadCenter.Y.Value = pixelpady - CurDieLeftCorner.Y.Value;

                        //Pads.PadInfo.Add(new PadParameter(position[count].SizeX, position[count].SizeY, 
                        //    new PadCoordinate(pixelpadx - CurDieLeftCorner.X.Value, pixelpady - CurDieLeftCorner.Y.Value)));

                        //Pads.PadInfo.Add(padparam);

                        int CheckPad;
                        int CheckTolerance;
                        // Check Pad Pos
                        CheckPad = this.PadRegist().CheckPadPosition(padparam);
                        CheckTolerance = this.PadRegist().CheckPadTolerance(padparam);

                        if (CheckPad == 0 && CheckTolerance == 0)
                        {
                            Pads.DutPadInfos.Add((DUTPadObject)padparam);
                            //Pads.PadInfo.OrderBy(a => a.PadCenter.X.Value);
                            //RWDeviceFile();

                            GraphicsParam Test = new GraphicsParam();

                            Test.X = padparam.PadCenter.X.Value;
                            Test.Y = padparam.PadCenter.Y.Value;
                            Test.SizeX = padparam.PadSizeX.Value;
                            Test.SizeY = padparam.PadSizeY.Value;

                            Test.Shape = padparam.PadShape.Value;

                            this.PadRegist().UpdateDrawList(Test);
                        }
                    }
                    //this.PadRegist().SavePadParam(ParamPath);

                    if (this.PadRegist() is IHasDevParameterizable)
                    {
                        (this.PadRegist() as IHasDevParameterizable).SaveDevParameter();
                    }
                }
                while (padwafercoord.X.Value + (cam.GetGrabSizeWidth()) < limitX);

                //아래 방향
                limitY = CurDieLeftCorner.Y.Value + (cam.GetGrabSizeHeight() / 2);
                do
                {
                    padwafercoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                    
                    GetCurGrabImage = this.VisionManager().SingleGrab(cam.GetChannelType(), this);

                    ObservableCollection<GrabDevPosition> position = new ObservableCollection<GrabDevPosition>();
                    position = this.VisionManager().Detect_Pad(GetCurGrabImage, Pads.SetPadObject.PadSizeX.Value, Pads.SetPadObject.PadSizeY.Value, roiparam);

                    if (position.Count == 0)
                    {
                        NextPosX = DefaultMovingPosX;
                        NextPosY = padwafercoord.Y.Value - DefaultMovingPosY;

                        StageSupervisor.StageModuleState.WaferHighViewMove(padwafercoord.X.Value, NextPosY, Wafer.GetPhysInfo().Thickness.Value);
                    }

                    else if (position.Count >= 1)
                    {
                        double offsety = 0.0;

                        if (position[position.Count - 1].PosY > (cam.GetGrabSizeHeight() / 2))
                        {
                            offsety = position[position.Count - 1].PosY - (cam.GetGrabSizeHeight() / 2);
                        }
                        else
                        {
                            offsety = (cam.GetGrabSizeHeight() / 2) - position[position.Count - 1].PosY;
                            offsety *= -1;
                        }

                        movey = padwafercoord.Y.Value - (offsety * cam.GetRatioY()) -
                        ((cam.GetGrabSizeHeight() / 2) * cam.GetRatioY());

                        StageSupervisor.StageModuleState.WaferHighViewMove(padwafercoord.X.Value, movey, Wafer.GetPhysInfo().Thickness.Value);
                    }

                    for (int count = 0; count < position.Count(); count++)
                    {
                        PadObject padparam = new PadObject();

                        padparam.PadSizeX.Value = position[count].SizeX;
                        padparam.PadSizeY.Value = position[count].SizeY;

                        double pixeloffsetx = 0.0;
                        double pixeloffsety = 0.0;

                        if (position[count].PosX > (cam.GetGrabSizeWidth() / 2))
                        {
                            pixeloffsetx = position[count].PosX - (cam.GetGrabSizeWidth() / 2);
                        }
                        else
                        {
                            pixeloffsetx = (cam.GetGrabSizeWidth() / 2) - position[count].PosX;
                            pixeloffsetx *= -1;
                        }

                        if (position[count].PosY < (cam.GetGrabSizeHeight() / 2))
                        {
                            pixeloffsety = position[count].PosY - (cam.GetGrabSizeHeight() / 2);
                            pixeloffsety *= -1;
                        }
                        else
                        {
                            pixeloffsety = (cam.GetGrabSizeHeight() / 2) - position[count].PosY;
                        }

                        double pixelpadx = padwafercoord.X.Value + (pixeloffsetx * cam.GetRatioX());
                        double pixelpady = padwafercoord.Y.Value + (pixeloffsety * cam.GetRatioY());

                        padparam.PadCenter.X.Value = pixelpadx - CurDieLeftCorner.X.Value;
                        padparam.PadCenter.Y.Value = pixelpady - CurDieLeftCorner.Y.Value;

                        //Pads.PadInfo.Add(padparam);
                        //Pads.PadInfo.Add(new PadParameter(position[count].SizeX, position[count].SizeY, 
                        //    new PadCoordinate(pixelpadx - CurDieLeftCorner.X.Value, pixelpady - CurDieLeftCorner.Y.Value)));

                        int CheckPad;
                        int CheckTolerance;
                        // Check Pad Pos
                        CheckPad = this.PadRegist().CheckPadPosition(padparam);
                        CheckTolerance = this.PadRegist().CheckPadTolerance(padparam);

                        if (CheckPad == 0 && CheckTolerance == 0)
                        {
                            Pads.DutPadInfos.Add((DUTPadObject)padparam);
                            //Pads.PadInfo.OrderBy(a => a.PadCenter.Y.Value);
                            //RWDeviceFile();

                            GraphicsParam Test = new GraphicsParam();

                            Test.X = padparam.PadCenter.X.Value;
                            Test.Y = padparam.PadCenter.Y.Value;
                            Test.SizeX = padparam.PadSizeX.Value;
                            Test.SizeY = padparam.PadSizeY.Value;

                            Test.Shape = padparam.PadShape.Value;

                            this.PadRegist().UpdateDrawList(Test);
                        }
                    }
                    //this.PadRegist().SavePadParam(ParamPath);

                    if (this.PadRegist() is IHasDevParameterizable)
                    {
                        (this.PadRegist() as IHasDevParameterizable).SaveDevParameter();
                    }
                }
                while (padwafercoord.Y.Value - (cam.GetGrabSizeHeight()) > limitY);

                //왼쪽 방향
                limitX = CurDieLeftCorner.X.Value + (cam.GetGrabSizeHeight() / 2);
                do
                {
                    padwafercoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                    
                    GetCurGrabImage = this.VisionManager().SingleGrab(cam.GetChannelType(), this);

                    ObservableCollection<GrabDevPosition> position = new ObservableCollection<GrabDevPosition>();
                    position = this.VisionManager().Detect_Pad(GetCurGrabImage, Pads.SetPadObject.PadSizeX.Value, Pads.SetPadObject.PadSizeY.Value, roiparam);

                    if (position.Count == 0)
                    {
                        NextPosX = padwafercoord.X.Value - DefaultMovingPosX;
                        NextPosY = DefaultMovingPosY;

                        StageSupervisor.StageModuleState.WaferHighViewMove(NextPosX, padwafercoord.Y.Value, Wafer.GetPhysInfo().Thickness.Value);

                    }

                    else if (position.Count >= 1)
                    {
                        double offsetx = 0.0;

                        if (position[position.Count - 1].PosX < cam.GetGrabSizeWidth() / 2)
                        {
                            offsetx = (cam.GetGrabSizeWidth() / 2) - position[position.Count - 1].PosX;
                            offsetx *= -1;
                        }
                        else
                        {
                            offsetx = position[position.Count - 1].PosX - (cam.GetGrabSizeWidth() / 2);

                        }

                        movex = padwafercoord.X.Value + (offsetx * cam.GetRatioX()) -
                        ((cam.GetGrabSizeWidth() / 2) * cam.GetRatioX());

                        StageSupervisor.StageModuleState.WaferHighViewMove(movex, padwafercoord.Y.Value, Wafer.GetPhysInfo().Thickness.Value);
                    }

                    for (int count = 0; count < position.Count(); count++)
                    {
                        PadObject padparam = new PadObject();

                        padparam.PadSizeX.Value = position[count].SizeX;
                        padparam.PadSizeY.Value = position[count].SizeY;

                        double pixeloffsetx = 0.0;
                        double pixeloffsety = 0.0;

                        if (position[count].PosX > (cam.GetGrabSizeWidth() / 2))
                        {
                            pixeloffsetx = position[count].PosX - (cam.GetGrabSizeWidth() / 2);
                        }
                        else
                        {
                            pixeloffsetx = (cam.GetGrabSizeWidth() / 2) - position[count].PosX;
                            pixeloffsetx *= -1;
                        }

                        if (position[count].PosY < (cam.GetGrabSizeHeight() / 2))
                        {
                            pixeloffsety = position[count].PosY - (cam.GetGrabSizeHeight() / 2);
                            pixeloffsety *= -1;
                        }
                        else
                        {
                            pixeloffsety = (cam.GetGrabSizeHeight() / 2) - position[count].PosY;
                        }

                        double pixelpadx = padwafercoord.X.Value + (pixeloffsetx * cam.GetRatioX());
                        double pixelpady = padwafercoord.Y.Value + (pixeloffsety * cam.GetRatioY());

                        padparam.PadCenter.X.Value = pixelpadx - CurDieLeftCorner.X.Value;
                        padparam.PadCenter.Y.Value = pixelpady - CurDieLeftCorner.Y.Value;

                        //Pads.PadInfo.Add(padparam);
                        //Pads.PadInfo.Add(new PadParameter(position[count].SizeX, position[count].SizeY, 
                        //    new PadCoordinate(pixelpadx - CurDieLeftCorner.X.Value, pixelpady - CurDieLeftCorner.Y.Value)));

                        int CheckPad;
                        int CheckTolerance;
                        // Check Pad Pos
                        CheckPad = this.PadRegist().CheckPadPosition(padparam);
                        CheckTolerance = this.PadRegist().CheckPadTolerance(padparam);

                        if (CheckPad == 0 && CheckTolerance == 0)
                        {
                            Pads.DutPadInfos.Add((DUTPadObject)padparam);
                            //Pads.PadInfo.OrderByDescending(a => a.PadCenter.X.Value);
                            //RWDeviceFile();

                            GraphicsParam Test = new GraphicsParam();

                            Test.X = padparam.PadCenter.X.Value;
                            Test.Y = padparam.PadCenter.Y.Value;
                            Test.SizeX = padparam.PadSizeX.Value;
                            Test.SizeY = padparam.PadSizeY.Value;

                            Test.Shape = padparam.PadShape.Value;

                            this.PadRegist().UpdateDrawList(Test);
                        }
                    }
                    //this.PadRegist().SavePadParam(ParamPath);

                    if (this.PadRegist() is IHasDevParameterizable)
                    {
                        (this.PadRegist() as IHasDevParameterizable).SaveDevParameter();
                    }
                }
                while (padwafercoord.X.Value - (cam.GetGrabSizeWidth()) > limitX);

                //위 방향
                //limitY = CurDieLeftCorner.Y.Value + WaferAlinger.WAInfoFile.DieSizeActualY - WaferAlinger.WAInfoFile.DieYClearance;
                limitY = CurDieLeftCorner.Y.Value + Wafer.GetSubsInfo().ActualDeviceSize.Height.Value;

                do
                {
                    padwafercoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                    
                    GetCurGrabImage = this.VisionManager().SingleGrab(cam.GetChannelType(), this);

                    ObservableCollection<GrabDevPosition> position = new ObservableCollection<GrabDevPosition>();
                    position = this.VisionManager().Detect_Pad(GetCurGrabImage, Pads.SetPadObject.PadSizeX.Value, Pads.SetPadObject.PadSizeY.Value, roiparam);

                    if (position.Count == 0)
                    {
                        NextPosX = DefaultMovingPosX;
                        NextPosY = padwafercoord.Y.Value + DefaultMovingPosY;

                        StageSupervisor.StageModuleState.WaferHighViewMove(padwafercoord.X.Value, NextPosY, Wafer.GetPhysInfo().Thickness.Value);

                    }

                    else if (position.Count >= 1)
                    {
                        double offsety = 0.0;
                        if (position[0].PosY < (cam.GetGrabSizeHeight() / 2))
                        {
                            offsety = position[0].PosY - (cam.GetGrabSizeHeight() / 2);
                            offsety *= -1;

                        }
                        else
                        {
                            offsety = (cam.GetGrabSizeHeight() / 2) - position[0].PosY;
                        }

                        movey = padwafercoord.Y.Value + (offsety * cam.GetRatioY()) +
                        ((cam.GetGrabSizeHeight() / 2) * cam.GetRatioY());

                        StageSupervisor.StageModuleState.WaferHighViewMove(padwafercoord.X.Value, movey, Wafer.GetPhysInfo().Thickness.Value);
                    }

                    for (int count = 0; count < position.Count(); count++)
                    {
                        PadObject padparam = new PadObject();

                        padparam.PadSizeX.Value = position[count].SizeX;
                        padparam.PadSizeY.Value = position[count].SizeY;

                        double pixeloffsetx = 0.0;
                        double pixeloffsety = 0.0;

                        if (position[count].PosX > (cam.GetGrabSizeWidth() / 2))
                        {
                            pixeloffsetx = position[count].PosX - (cam.GetGrabSizeWidth() / 2);
                        }
                        else
                        {
                            pixeloffsetx = (cam.GetGrabSizeWidth() / 2) - position[count].PosX;
                            pixeloffsetx *= -1;
                        }

                        if (position[count].PosY < (cam.GetGrabSizeHeight() / 2))
                        {
                            pixeloffsety = position[count].PosY - (cam.GetGrabSizeHeight() / 2);
                            pixeloffsety *= -1;
                        }
                        else
                        {
                            pixeloffsety = (cam.GetGrabSizeHeight() / 2) - position[count].PosY;
                        }

                        double pixelpadx = padwafercoord.X.Value + (pixeloffsetx * cam.GetRatioX());
                        double pixelpady = padwafercoord.Y.Value + (pixeloffsety * cam.GetRatioY());

                        padparam.PadCenter.X.Value = pixelpadx - CurDieLeftCorner.X.Value;
                        padparam.PadCenter.Y.Value = pixelpady - CurDieLeftCorner.Y.Value;

                        //Pads.PadInfo.Add(padparam);

                        //Pads.PadInfo.Add(new PadParameter(position[count].SizeX, position[count].SizeY, 
                        //    new PadCoordinate(pixelpadx - CurDieLeftCorner.X.Value, pixelpady - CurDieLeftCorner.Y.Value)));

                        int CheckPad;
                        int CheckTolerance;
                        // Check Pad Pos
                        CheckPad = this.PadRegist().CheckPadPosition(padparam);
                        CheckTolerance = this.PadRegist().CheckPadTolerance(padparam);

                        if (CheckPad == 0 && CheckTolerance == 0)
                        {
                            Pads.DutPadInfos.Add((DUTPadObject)padparam);
                            //Pads.PadInfo.OrderByDescending(a => a.PadCenter.Y.Value);
                            //RWDeviceFile();

                            GraphicsParam Test = new GraphicsParam();

                            Test.X = padparam.PadCenter.X.Value;
                            Test.Y = padparam.PadCenter.Y.Value;
                            Test.SizeX = padparam.PadSizeX.Value;
                            Test.SizeY = padparam.PadSizeY.Value;

                            Test.Shape = padparam.PadShape.Value;

                            this.PadRegist().UpdateDrawList(Test);
                        }
                    }

                    if (this.PadRegist() is IHasDevParameterizable)
                    {
                        (this.PadRegist() as IHasDevParameterizable).SaveDevParameter();
                    }

                }
                while (padwafercoord.Y.Value + (cam.GetGrabSizeHeight()) < limitY);        
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public override void DeInitModule()
        {
        }
    }
}
