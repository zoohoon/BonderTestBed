using LogModule;
using ProbeCardObject;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using ProberInterfaces.PinAlign.ProbeCardData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Vision.GraphicsContext;

namespace PinAlign
{
    public class PinSetupControl
    {
        // POSITION = 0,
        // SIZE,
        // SEARCHAREA,
        // THRESHOLD,
        // BLOBMIN,
        // BLOBMAX

        private bool _ShowPosition;
        public bool ShowPosition
        {
            get { return _ShowPosition; }
            set
            {
                if (value != _ShowPosition)
                {
                    _ShowPosition = value;
                }
            }
        }

        private bool _ShowSize;
        public bool ShowSize
        {
            get { return _ShowSize; }
            set
            {
                if (value != _ShowSize)
                {
                    _ShowSize = value;
                }
            }
        }

        private bool _ShowTipSearchArea;
        public bool ShowTipSearchArea
        {
            get { return _ShowTipSearchArea; }
            set
            {
                if (value != _ShowTipSearchArea)
                {
                    _ShowTipSearchArea = value;
                }
            }
        }

        private bool _ShowKeySearchArea;
        public bool ShowKeySearchArea
        {
            get { return _ShowKeySearchArea; }
            set
            {
                if (value != _ShowKeySearchArea)
                {
                    _ShowKeySearchArea = value;
                }
            }
        }

        private bool _ShowThreshold;
        public bool ShowThreshold
        {
            get { return _ShowThreshold; }
            set
            {
                if (value != _ShowThreshold)
                {
                    _ShowThreshold = value;
                }
            }
        }

        private bool _ShowKeyBlobMin;
        public bool ShowKeyBlobMin
        {
            get { return _ShowKeyBlobMin; }
            set
            {
                if (value != _ShowKeyBlobMin)
                {
                    _ShowKeyBlobMin = value;
                }
            }
        }

        private bool _ShowKeyBlobMax;
        public bool ShowKeyBlobMax
        {
            get { return _ShowKeyBlobMax; }
            set
            {
                if (value != _ShowKeyBlobMax)
                {
                    _ShowKeyBlobMax = value;
                }
            }
        }

        private bool _ShowTipBlobMin;
        public bool ShowTipBlobMin
        {
            get { return _ShowTipBlobMin; }
            set
            {
                if (value != _ShowTipBlobMin)
                {
                    _ShowTipBlobMin = value;
                }
            }
        }

        private bool _ShowTipBlobMax;
        public bool ShowTipBlobMax
        {
            get { return _ShowTipBlobMax; }
            set
            {
                if (value != _ShowTipBlobMax)
                {
                    _ShowTipBlobMax = value;
                }
            }
        }

        private bool _ShowBaseFocusingArea;
        public bool ShowBaseFocusingArea
        {
            get { return _ShowBaseFocusingArea; }
            set
            {
                if (value != _ShowBaseFocusingArea)
                {
                    _ShowBaseFocusingArea = value;
                }
            }
        }

        private bool _ShowKeyFocusingArea = false;

        public bool ShowKeyFocusingArea
        {
            get { return _ShowKeyFocusingArea; }
            set { _ShowKeyFocusingArea = value; }
        }
    }

    public class ProbeCardGraphicsModule : IFactoryModule
    {


        [System.Diagnostics.DebuggerBrowsable(
            System.Diagnostics.DebuggerBrowsableState.Never)]

        private IProbeCard PCInfo { get { return this.GetParam_ProbeCard(); } }

        private CatCoordinates LUcorner = new CatCoordinates();
        private CatCoordinates RDcorner = new CatCoordinates();

        DrawRectangleModule drawRectangle = new DrawRectangleModule();
        DrawEllipseModule drawEclipse = new DrawEllipseModule();
        DrawRectangleModule drawAlignKeyRectangle = new DrawRectangleModule();
        DrawRectangleModule drawFocusingAreaRectangle = new DrawRectangleModule();
        //DrawRectangleModule drawPadRectangle = new DrawRectangleModule();
        List<DrawLineModule> drawLines = new List<DrawLineModule>();
        DrawLineModule drawLine = new DrawLineModule();
        List<DrawTextModule> drawTexts = new List<DrawTextModule>();
        DrawTextModule drawText = new DrawTextModule();

        public bool OnOff = false;

        public PinSetupControl PinSetupControl { get; set; }

        public ProbeCardGraphicsModule()
        {
            if (PinSetupControl == null)
            {
                PinSetupControl = new PinSetupControl();
            }
        }

        public EventCodeEnum DrawDutOverlay(ImageBuffer img, ICamera cam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if ((cam.GetChannelType() == EnumProberCam.PIN_HIGH_CAM ||
                    cam.GetChannelType() == EnumProberCam.PIN_LOW_CAM) && OnOff == true)
                {
                    if (cam.DisplayService.DrawOverlayContexts.Count != 0)
                    {
                        cam.DisplayService.DrawOverlayContexts.Clear();
                    }


                    //await Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    //{
                    //lock (img)
                    //{

                    double pt1_x;
                    double pt1_y;
                    double pt2_x;
                    double pt2_y;

                    //double LineTop;
                    //double LineBottom;
                    //double LineLeft;
                    //double LineRight;

                    //PinCoordinate CenterDutLeftCorner = null;
                    //PinCoordinate NewPos = null;
                    //PinCoordinate RefPinPos = null;
                    //double DistCenDutFromCurDutIndexX = 0;
                    //double DistCenDutFromCurDutIndexY = 0;

                    //double milScreenLeft = img.CatCoordinates.X.Value - (img.SizeX / 2 * img.RatioX.Value);
                    //double milScreenTop = img.CatCoordinates.Y.Value + (img.SizeY / 2 * img.RatioY.Value);
                    //double milScreenRight = img.CatCoordinates.X.Value + (img.SizeX / 2 * img.RatioX.Value);
                    //double milScreenBottom = img.CatCoordinates.Y.Value - (img.SizeY / 2 * img.RatioY.Value);

                    double[,] DutCornerPos = new double[4, 2];
                    double[,] DisplayCornerPos = new double[4, 2];
                    double PosLL_X = 0;
                    double PosLL_Y = 0;
                    double firstDut_LLX = 0;
                    double firstDut_LLY = 0;
                    double a = 0;
                    double b = 0;
                    double x = 0;
                    double y = 0;
                    double x2 = 0;
                    double y2 = 0;
                    int i = 0;

                    PinCoordinate NewPos = new PinCoordinate();
                    PinCoordinate CenPos = new PinCoordinate();
                    double DutAngle = 0;

                    // 핀 Draw에서 가져옴
                    double[,] DieConrerPos = new double[2, 2];
                    double[,] DisplayConerePos = new double[2, 2];

                    double milScreenLeft = img.CatCoordinates.X.Value - (img.SizeX / 2 * img.RatioX.Value);
                    double milScreenTop = img.CatCoordinates.Y.Value + (img.SizeY / 2 * img.RatioY.Value);
                    double milScreenRight = img.CatCoordinates.X.Value + (img.SizeX / 2 * img.RatioX.Value);
                    double milScreenBottom = img.CatCoordinates.Y.Value - (img.SizeY / 2 * img.RatioY.Value);

                    double pinstartX = 0;
                    double pinstartY = 0;

                    double alignKeyStartX = 0;
                    double alignKeyStartY = 0;

                    double alignKeyWidth = 0;
                    double alignKeyHeight = 0;

                    double radius = 0;

                    double pincetnerx = 0;
                    double pincentery = 0;

                    double alignkeyceterx = 0;
                    double alignkeycetery = 0;

                    string str = "";

                    PinCoordinate ZeroAnglePos = new PinCoordinate();

                    // 현재 보이는 화면 네 귀퉁이의 좌표를 구한다.    
                    // 좌측 하단
                    DisplayCornerPos[0, 0] = img.CatCoordinates.X.Value - (img.RatioX.Value * (img.SizeX / 2));
                    DisplayCornerPos[0, 1] = img.CatCoordinates.Y.Value - (img.RatioY.Value * (img.SizeY / 2));
                    // 우측 하단
                    DisplayCornerPos[1, 0] = img.CatCoordinates.X.Value + (img.RatioX.Value * (img.SizeX / 2));
                    DisplayCornerPos[1, 1] = img.CatCoordinates.Y.Value - (img.RatioY.Value * (img.SizeY / 2));
                    // 좌측 상단
                    DisplayCornerPos[2, 0] = img.CatCoordinates.X.Value - (img.RatioX.Value * (img.SizeX / 2));
                    DisplayCornerPos[2, 1] = img.CatCoordinates.Y.Value + (img.RatioY.Value * (img.SizeY / 2));
                    // 우측 상단
                    DisplayCornerPos[3, 0] = img.CatCoordinates.X.Value + (img.RatioX.Value * (img.SizeX / 2));
                    DisplayCornerPos[3, 1] = img.CatCoordinates.Y.Value + (img.RatioY.Value * (img.SizeY / 2));

                    // 더트 오버레이를 표시한다.
                    DrawLineModule drawLine1 = new DrawLineModule();
                    DrawLineModule drawLine2 = new DrawLineModule();
                    DrawLineModule drawLine3 = new DrawLineModule();
                    DrawLineModule drawLine4 = new DrawLineModule();

                    // 핀 센터(Abs) + 패드 센터(LL 로부터 상대거리) = 핀 좌표계에서 1번 더트의 LL 위치
                    //firstDut_LLX = this.StageSupervisor().ProbeCardInfo.PinCenX - this.StageSupervisor().WaferObject.GetSubsInfo().Pads.PadCenX;
                    //firstDut_LLY = this.StageSupervisor().ProbeCardInfo.PinCenY - this.StageSupervisor().WaferObject.GetSubsInfo().Pads.PadCenY;

                    CenPos.X.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenX;
                    CenPos.Y.Value = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenY;
                    DutAngle = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutAngle;

                    var firstDut = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.FirstOrDefault();

                    // TODO: FLIP 검토 할것
                    //if (this.VisionManager().GetDispHorFlip() == DispFlipEnum.FLIP && this.VisionManager().GetDispVerFlip() == DispFlipEnum.FLIP)
                    //{
                    //    firstDut = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.FirstOrDefault(item =>
                    //                item.MacIndex.XIndex == this.StageSupervisor().WaferObject.GetPhysInfo().MapCountX.Value - 1 - firstDut.MacIndex.XIndex
                    //                && item.MacIndex.YIndex == this.StageSupervisor().WaferObject.GetPhysInfo().MapCountY.Value - 1 - firstDut.MacIndex.YIndex
                    //                );
                    //}                    

                    firstDut_LLX = CenPos.X.Value - ((double)((this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeX / 2.0)) * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Width.Value);
                    firstDut_LLX += firstDut.MacIndex.XIndex * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Width.Value;
                    firstDut_LLY = CenPos.Y.Value - ((double)((this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutIndexSizeY / 2.0)) * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Height.Value);
                    firstDut_LLY += firstDut.MacIndex.YIndex * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Height.Value;

                    int iDutNum = -1;
                    String curDutStr = String.Empty;

                    foreach (IDut dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                    {

                        // 이 더트의 좌하단 위치
                        PosLL_X = firstDut_LLX + (dut.MacIndex.XIndex - firstDut.MacIndex.XIndex) * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Width.Value;
                        PosLL_Y = firstDut_LLY + (dut.MacIndex.YIndex - firstDut.MacIndex.YIndex) * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Height.Value;



                        // 이 더트의 각 꼭지점 위치
                        // 좌하
                        DutCornerPos[0, 0] = PosLL_X;
                        DutCornerPos[0, 1] = PosLL_Y;
                        // 우하
                        DutCornerPos[1, 0] = PosLL_X + this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Width.Value;
                        DutCornerPos[1, 1] = PosLL_Y;
                        // 좌상
                        DutCornerPos[2, 0] = PosLL_X;
                        DutCornerPos[2, 1] = PosLL_Y + this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Height.Value;
                        // 우상
                        DutCornerPos[3, 0] = PosLL_X + this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Width.Value;
                        DutCornerPos[3, 1] = PosLL_Y + this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Height.Value;

                        // 현재 포인터가 놓인 위치의 더트 번호를 확인한다. (디스플레이 용)
                        if (iDutNum <= 0)
                        {
                            Rect displayScreenRect = new Rect(new Point(DutCornerPos[0, 0], DutCornerPos[0, 1]), new Point(DutCornerPos[3, 0], DutCornerPos[3, 1]));
                            //Point displayCenterPoint = new Point((img.RatioX.Value * (img.SizeX / 2)), (img.RatioY.Value * (img.SizeY / 2)));
                            // 더트 각도를 고려하기 위해서 일단 현재 보고 있는 지점에 더트 각도를 반대로 적용시켜 위치를 구하고, 이 위치값을 각도가 적용되기 전
                            // 더트에 대입하여 더트 내부에 현재 지점이 존재하는지 확인한다. (꼼수 얍~)
                            GetRotCoordEx(ref ZeroAnglePos, new PinCoordinate(img.CatCoordinates.X.Value, img.CatCoordinates.Y.Value), CenPos, -DutAngle);
                            Point displayCenterPoint = new Point(ZeroAnglePos.X.Value, ZeroAnglePos.Y.Value);

                            if (displayScreenRect.Contains(displayCenterPoint))
                            {
                                // 빙고. 이 더트가 니 더트임.
                                iDutNum = dut.DutNumber;
                            }
                        }

                        // 핀 각도 적용
                        GetRotCoordEx(ref NewPos, new PinCoordinate(DutCornerPos[0, 0], DutCornerPos[0, 1]), CenPos, DutAngle);
                        DutCornerPos[0, 0] = NewPos.GetX();
                        DutCornerPos[0, 1] = NewPos.GetY();

                        GetRotCoordEx(ref NewPos, new PinCoordinate(DutCornerPos[1, 0], DutCornerPos[1, 1]), CenPos, DutAngle);
                        DutCornerPos[1, 0] = NewPos.GetX();
                        DutCornerPos[1, 1] = NewPos.GetY();

                        GetRotCoordEx(ref NewPos, new PinCoordinate(DutCornerPos[2, 0], DutCornerPos[2, 1]), CenPos, DutAngle);
                        DutCornerPos[2, 0] = NewPos.GetX();
                        DutCornerPos[2, 1] = NewPos.GetY();

                        GetRotCoordEx(ref NewPos, new PinCoordinate(DutCornerPos[3, 0], DutCornerPos[3, 1]), CenPos, DutAngle);
                        DutCornerPos[3, 0] = NewPos.GetX();
                        DutCornerPos[3, 1] = NewPos.GetY();

                        // 화면에 보일 라인만 표시한다
                        // 더트의 꼭지점이 화면 안에 존재하지 않더라도, 두 꼭지점을 잇는 직선의 일부분이 화면 내에 존재하면 디스플레이 해야 한다.

                        for (i = 0; i <= 3; i++)
                        {
                            // y = ax + b
                            if (i == 0)
                            {
                                // i = 0 : 더트 하단 (좌하 + 우하 직선)
                                if (DutCornerPos[1, 1] - DutCornerPos[0, 1] != 0)
                                {
                                    // 기울기가 있는 경우
                                    a = (DutCornerPos[1, 1] - DutCornerPos[0, 1]) / (DutCornerPos[1, 0] - DutCornerPos[0, 0]);  // 더트 하단을 포함하는 직선의 기울기
                                    b = DutCornerPos[0, 1] - (a * DutCornerPos[0, 0]);

                                    y = (a * DisplayCornerPos[0, 0]) + b;
                                    y2 = (a * DisplayCornerPos[1, 0]) + b;
                                    if ((y >= DisplayCornerPos[0, 1] && y <= DisplayCornerPos[2, 1]) || y2 >= DisplayCornerPos[0, 1] && y2 <= DisplayCornerPos[2, 1])
                                    {
                                        pt1_x = (img.SizeX / 2) + ((DutCornerPos[0, 0] - img.CatCoordinates.X.Value) / img.RatioX.Value);
                                        pt1_y = (img.SizeY / 2) - ((DutCornerPos[0, 1] - img.CatCoordinates.Y.Value) / img.RatioY.Value);
                                        pt2_x = (img.SizeX / 2) + ((DutCornerPos[1, 0] - img.CatCoordinates.X.Value) / img.RatioX.Value);
                                        pt2_y = (img.SizeY / 2) - ((DutCornerPos[1, 1] - img.CatCoordinates.Y.Value) / img.RatioY.Value);

                                        drawLine2 = new DrawLineModule();
                                        drawLine2.SetParameter(pt1_x, pt1_y, pt2_x, pt2_y, Colors.BlueViolet, 2,
                                        verflip: this.VisionManager().GetDispVerFlip(), horflip: this.VisionManager().GetDispHorFlip(),
                                        left: 0, right: img.SizeX, top: 0, bottom: img.SizeY);
                                        cam.DisplayService.DrawOverlayContexts.Add(drawLine2);
                                    }
                                }
                                else
                                {
                                    // 기울기가 없는 경우
                                    a = 0;
                                    b = DutCornerPos[1, 1];

                                    y = DutCornerPos[0, 1];
                                    if ((y >= DisplayCornerPos[0, 1] && y <= DisplayCornerPos[2, 1]) && (DutCornerPos[0, 0] <= DisplayCornerPos[1, 0] && DutCornerPos[1, 0] >= DisplayCornerPos[0, 0]))
                                    {
                                        pt1_x = (img.SizeX / 2) + ((DutCornerPos[0, 0] - img.CatCoordinates.X.Value) / img.RatioX.Value);
                                        pt1_y = (img.SizeY / 2) - ((DutCornerPos[0, 1] - img.CatCoordinates.Y.Value) / img.RatioY.Value);
                                        pt2_x = (img.SizeX / 2) + ((DutCornerPos[1, 0] - img.CatCoordinates.X.Value) / img.RatioX.Value);
                                        pt2_y = (img.SizeY / 2) - ((DutCornerPos[1, 1] - img.CatCoordinates.Y.Value) / img.RatioY.Value);

                                        drawLine2 = new DrawLineModule();
                                        drawLine2.SetParameter(pt1_x, pt1_y, pt2_x, pt2_y, Colors.BlueViolet, 2,
                                        verflip: this.VisionManager().GetDispVerFlip(), horflip: this.VisionManager().GetDispHorFlip(),
                                        left: 0, right: img.SizeX, top: 0, bottom: img.SizeY);
                                        cam.DisplayService.DrawOverlayContexts.Add(drawLine2);
                                    }
                                }
                            }
                            else if (i == 1)
                            {
                                // i = 1 : 더트 상단 (좌상 + 우상 직선)
                                if (DutCornerPos[3, 1] - DutCornerPos[2, 1] != 0)
                                {
                                    a = (DutCornerPos[3, 1] - DutCornerPos[2, 1]) / (DutCornerPos[3, 0] - DutCornerPos[2, 0]);
                                    b = DutCornerPos[2, 1] - (a * DutCornerPos[2, 0]);

                                    y = (a * DisplayCornerPos[0, 0]) + b;
                                    y2 = (a * DisplayCornerPos[1, 0]) + b;
                                    if ((y >= DisplayCornerPos[0, 1] && y <= DisplayCornerPos[2, 1]) || y2 >= DisplayCornerPos[0, 1] && y2 <= DisplayCornerPos[2, 1])
                                    {
                                        pt1_x = (img.SizeX / 2) + ((DutCornerPos[3, 0] - img.CatCoordinates.X.Value) / img.RatioX.Value);
                                        pt1_y = (img.SizeY / 2) - ((DutCornerPos[3, 1] - img.CatCoordinates.Y.Value) / img.RatioY.Value);
                                        pt2_x = (img.SizeX / 2) + ((DutCornerPos[2, 0] - img.CatCoordinates.X.Value) / img.RatioX.Value);
                                        pt2_y = (img.SizeY / 2) - ((DutCornerPos[2, 1] - img.CatCoordinates.Y.Value) / img.RatioY.Value);

                                        drawLine1 = new DrawLineModule();
                                        drawLine1.SetParameter(pt1_x, pt1_y, pt2_x, pt2_y, Colors.BlueViolet, 2,
                                        verflip: this.VisionManager().GetDispVerFlip(), horflip: this.VisionManager().GetDispHorFlip(),
                                        left: 0, right: img.SizeX, top: 0, bottom: img.SizeY);
                                        cam.DisplayService.DrawOverlayContexts.Add(drawLine1);
                                    }
                                }
                                else
                                {
                                    a = 0;
                                    b = DutCornerPos[2, 1];

                                    y = DutCornerPos[2, 1];
                                    if ((y >= DisplayCornerPos[0, 1] && y <= DisplayCornerPos[2, 1]) && (DutCornerPos[0, 0] <= DisplayCornerPos[1, 0] && DutCornerPos[1, 0] >= DisplayCornerPos[0, 0]))
                                    {
                                        pt1_x = (img.SizeX / 2) + ((DutCornerPos[3, 0] - img.CatCoordinates.X.Value) / img.RatioX.Value);
                                        pt1_y = (img.SizeY / 2) - ((DutCornerPos[3, 1] - img.CatCoordinates.Y.Value) / img.RatioY.Value);
                                        pt2_x = (img.SizeX / 2) + ((DutCornerPos[2, 0] - img.CatCoordinates.X.Value) / img.RatioX.Value);
                                        pt2_y = (img.SizeY / 2) - ((DutCornerPos[2, 1] - img.CatCoordinates.Y.Value) / img.RatioY.Value);

                                        drawLine1 = new DrawLineModule();
                                        drawLine1.SetParameter(pt1_x, pt1_y, pt2_x, pt2_y, Colors.BlueViolet, 2,
                                        verflip: this.VisionManager().GetDispVerFlip(), horflip: this.VisionManager().GetDispHorFlip(),
                                        left: 0, right: img.SizeX, top: 0, bottom: img.SizeY);
                                        cam.DisplayService.DrawOverlayContexts.Add(drawLine1);
                                    }
                                }
                            }
                            else if (i == 2)
                            {
                                // x = ay + b
                                // i = 2 : 더트 좌측 (좌하 + 좌상 직선)
                                if (DutCornerPos[0, 0] - DutCornerPos[2, 0] != 0)
                                {
                                    a = (DutCornerPos[2, 0] - DutCornerPos[0, 0]) / (DutCornerPos[2, 1] - DutCornerPos[0, 1]);
                                    b = DutCornerPos[2, 0] - (a * DutCornerPos[2, 1]);

                                    x = (DisplayCornerPos[2, 1] * a) + b;
                                    x2 = (DisplayCornerPos[0, 1] * a) + b;

                                    if ((x >= DisplayCornerPos[0, 0] && x <= DisplayCornerPos[1, 0]) || x2 >= DisplayCornerPos[0, 0] && x2 <= DisplayCornerPos[1, 0])
                                    {
                                        pt1_x = (img.SizeX / 2) + ((DutCornerPos[2, 0] - img.CatCoordinates.X.Value) / img.RatioX.Value);
                                        pt1_y = (img.SizeY / 2) - ((DutCornerPos[2, 1] - img.CatCoordinates.Y.Value) / img.RatioY.Value);
                                        pt2_x = (img.SizeX / 2) + ((DutCornerPos[0, 0] - img.CatCoordinates.X.Value) / img.RatioX.Value);
                                        pt2_y = (img.SizeY / 2) - ((DutCornerPos[0, 1] - img.CatCoordinates.Y.Value) / img.RatioY.Value);

                                        drawLine3 = new DrawLineModule();
                                        drawLine3.SetParameter(pt1_x, pt1_y, pt2_x, pt2_y, Colors.BlueViolet, 2,
                                        verflip: this.VisionManager().GetDispVerFlip(), horflip: this.VisionManager().GetDispHorFlip(),
                                        left: 0, right: img.SizeX, top: 0, bottom: img.SizeY
                                            );
                                        cam.DisplayService.DrawOverlayContexts.Add(drawLine3);
                                    }
                                }
                                else
                                {
                                    a = 0;
                                    b = DutCornerPos[2, 0];

                                    // x가 화면 범위 내에 존재하고 y의 위쪽이 화면 아래쪽 끝면보다 위고 y의 아래쪽이 화면 위쪽보다 아래면 그린다.
                                    x = DutCornerPos[0, 0];
                                    if ((x >= DisplayCornerPos[0, 0] && x <= DisplayCornerPos[1, 0]) && (DutCornerPos[2, 1] >= DisplayCornerPos[0, 1] && DutCornerPos[0, 1] <= DisplayCornerPos[2, 1]))
                                    {
                                        pt1_x = (img.SizeX / 2) + ((DutCornerPos[2, 0] - img.CatCoordinates.X.Value) / img.RatioX.Value);
                                        pt1_y = (img.SizeY / 2) - ((DutCornerPos[2, 1] - img.CatCoordinates.Y.Value) / img.RatioY.Value);
                                        pt2_x = (img.SizeX / 2) + ((DutCornerPos[0, 0] - img.CatCoordinates.X.Value) / img.RatioX.Value);
                                        pt2_y = (img.SizeY / 2) - ((DutCornerPos[0, 1] - img.CatCoordinates.Y.Value) / img.RatioY.Value);

                                        drawLine3 = new DrawLineModule();
                                        drawLine3.SetParameter(pt1_x, pt1_y, pt2_x, pt2_y, Colors.BlueViolet, 2,
                                        verflip: this.VisionManager().GetDispVerFlip(), horflip: this.VisionManager().GetDispHorFlip(),
                                        left: 0, right: img.SizeX, top: 0, bottom: img.SizeY);
                                        cam.DisplayService.DrawOverlayContexts.Add(drawLine3);
                                    }
                                }
                            }
                            else
                            {
                                // i = 3 : 더트 우측 (우하 + 우상 직선)
                                if (DutCornerPos[1, 0] - DutCornerPos[3, 0] != 0)
                                {
                                    a = (DutCornerPos[3, 0] - DutCornerPos[1, 0]) / (DutCornerPos[3, 1] - DutCornerPos[1, 1]);
                                    b = DutCornerPos[3, 0] - (a * DutCornerPos[3, 1]);

                                    x = (DisplayCornerPos[2, 1] * a) + b;
                                    x2 = (DisplayCornerPos[0, 1] * a) + b;

                                    if ((x >= DisplayCornerPos[0, 0] && x <= DisplayCornerPos[1, 0]) || x2 >= DisplayCornerPos[0, 0] && x2 <= DisplayCornerPos[1, 0])
                                    {
                                        pt1_x = (img.SizeX / 2) + ((DutCornerPos[3, 0] - img.CatCoordinates.X.Value) / img.RatioX.Value);
                                        pt1_y = (img.SizeY / 2) - ((DutCornerPos[3, 1] - img.CatCoordinates.Y.Value) / img.RatioY.Value);
                                        pt2_x = (img.SizeX / 2) + ((DutCornerPos[1, 0] - img.CatCoordinates.X.Value) / img.RatioX.Value);
                                        pt2_y = (img.SizeY / 2) - ((DutCornerPos[1, 1] - img.CatCoordinates.Y.Value) / img.RatioY.Value);

                                        drawLine4 = new DrawLineModule();
                                        drawLine4.SetParameter(pt1_x, pt1_y, pt2_x, pt2_y, Colors.BlueViolet, 2,
                                        verflip: this.VisionManager().GetDispVerFlip(), horflip: this.VisionManager().GetDispHorFlip(),
                                        left: 0, right: img.SizeX, top: 0, bottom: img.SizeY);
                                        cam.DisplayService.DrawOverlayContexts.Add(drawLine4);
                                    }
                                }
                                else
                                {
                                    a = 0;
                                    b = DutCornerPos[3, 0];

                                    x = DutCornerPos[1, 0];
                                    if ((x >= DisplayCornerPos[0, 0] && x <= DisplayCornerPos[1, 0]) && (DutCornerPos[2, 1] >= DisplayCornerPos[0, 1] && DutCornerPos[0, 1] <= DisplayCornerPos[2, 1]))
                                    {
                                        pt1_x = (img.SizeX / 2) + ((DutCornerPos[3, 0] - img.CatCoordinates.X.Value) / img.RatioX.Value);
                                        pt1_y = (img.SizeY / 2) - ((DutCornerPos[3, 1] - img.CatCoordinates.Y.Value) / img.RatioY.Value);
                                        pt2_x = (img.SizeX / 2) + ((DutCornerPos[1, 0] - img.CatCoordinates.X.Value) / img.RatioX.Value);
                                        pt2_y = (img.SizeY / 2) - ((DutCornerPos[1, 1] - img.CatCoordinates.Y.Value) / img.RatioY.Value);

                                        drawLine4 = new DrawLineModule();
                                        drawLine4.SetParameter(pt1_x, pt1_y, pt2_x, pt2_y, Colors.BlueViolet, 2,
                                        verflip: this.VisionManager().GetDispVerFlip(), horflip: this.VisionManager().GetDispHorFlip(),
                                        left: 0, right: img.SizeX, top: 0, bottom: img.SizeY);
                                        cam.DisplayService.DrawOverlayContexts.Add(drawLine4);
                                    }
                                }
                            }
                        }

                        //cam.DisplayService.DrawOverlayContexts.Add(drawLine1);

                        #region OLD
                        //foreach (Dut dut in this.StageSupervisor().ProbeCardInfo.DutList)
                        //{
                        //    foreach (PinData pin in dut.PinList)
                        //    {
                        //        if (pin.PinNum.Value == this.StageSupervisor().ProbeCardInfo.RefPinNum.Value)
                        //        {
                        //            RefPinPos = new PinCoordinate(pin.AbsPos);
                        //            break;
                        //        }
                        //    }
                        //    if (RefPinPos != null)
                        //        break;
                        //    else
                        //    {
                        //        retVal = EventCodeEnum.PIN_NOT_ENOUGH;
                        //        return;
                        //    }
                        //}

                        //this.StageSupervisor().ProbeCardInfo.DutDiffX = -10;
                        //this.StageSupervisor().ProbeCardInfo.DutDiffY = -10;
                        //this.StageSupervisor().ProbeCardInfo.DutAngle = -1;

                        //foreach (Dut dut in this.StageSupervisor().ProbeCardInfo.DutList)
                        //{
                        //    DutCornerPos[2, 0] = dut.RefCorner.GetX() + this.StageSupervisor().ProbeCardInfo.DutDiffX;
                        //    DutCornerPos[2, 1] = dut.RefCorner.GetY() + this.StageSupervisor().ProbeCardInfo.DutDiffY;

                        //    DutCornerPos[0, 0] = dut.RefCorner.GetX();
                        //    DutCornerPos[0, 1] = dut.RefCorner.GetY() + this.StageSupervisor().ProbeCardInfo.DutSizeY.Value;

                        //    DutCornerPos[1, 0] = dut.RefCorner.GetX() + this.StageSupervisor().ProbeCardInfo.DutSizeX.Value;
                        //    DutCornerPos[1, 1] = dut.RefCorner.GetY() + this.StageSupervisor().ProbeCardInfo.DutSizeY.Value;

                        //    DutCornerPos[3, 0] = dut.RefCorner.GetX() + this.StageSupervisor().ProbeCardInfo.DutSizeX.Value;
                        //    DutCornerPos[3, 1] = dut.RefCorner.GetY();

                        //    //PinCoordinate CardCenter = new PinCoordinate(this.CoordinateManager().PinHighPinConvert.Convert(this.StageSupervisor().ProbeCardInfo.ProbeCardCenterPos));

                        //    GetRotCoordEx(ref NewPos, new PinCoordinate(DutCornerPos[0, 0], DutCornerPos[0, 1]), RefPinPos, this.StageSupervisor().ProbeCardInfo.DutAngle);
                        //    DutCornerPos[0, 0] = NewPos.GetX();
                        //    DutCornerPos[0, 1] = NewPos.GetY();

                        //    GetRotCoordEx(ref NewPos, new PinCoordinate(DutCornerPos[1, 0], DutCornerPos[1, 1]), RefPinPos, this.StageSupervisor().ProbeCardInfo.DutAngle);
                        //    DutCornerPos[1, 0] = NewPos.GetX();
                        //    DutCornerPos[1, 1] = NewPos.GetY();

                        //    GetRotCoordEx(ref NewPos, new PinCoordinate(DutCornerPos[2, 0], DutCornerPos[2, 1]), RefPinPos, this.StageSupervisor().ProbeCardInfo.DutAngle);
                        //    DutCornerPos[2, 0] = NewPos.GetX();
                        //    DutCornerPos[2, 1] = NewPos.GetY();

                        //    GetRotCoordEx(ref NewPos, new PinCoordinate(DutCornerPos[3, 0], DutCornerPos[3, 1]), RefPinPos, this.StageSupervisor().ProbeCardInfo.DutAngle);
                        //    DutCornerPos[3, 0] = NewPos.GetX();
                        //    DutCornerPos[3, 1] = NewPos.GetY();

                        //    DrawLineModule drawLine1 = new DrawLineModule();
                        //    pt1_x = (DutCornerPos[0, 0] - milScreenLeft) / img.RatioX.Value;
                        //    pt1_y = (milScreenTop - DutCornerPos[0, 1]) / img.RatioY.Value;
                        //    pt2_x = (DutCornerPos[1, 0] - milScreenLeft) / img.RatioX.Value;
                        //    pt2_y = (milScreenTop - (DutCornerPos[1, 1])) / img.RatioY.Value;

                        //    if (DutCornerPos[0, 0] > DutCornerPos[1, 0])
                        //    {
                        //        LineRight = DutCornerPos[0, 0];
                        //        LineLeft = DutCornerPos[1, 0];
                        //    }
                        //    else
                        //    {
                        //        LineRight = DutCornerPos[1, 0];
                        //        LineLeft = DutCornerPos[0, 0];
                        //    }

                        //    if (DutCornerPos[0, 1] > DutCornerPos[1, 1])
                        //    {
                        //        LineTop = DutCornerPos[0, 1];
                        //        LineBottom = DutCornerPos[1, 1];
                        //    }
                        //    else
                        //    {
                        //        LineTop = DutCornerPos[1, 1];
                        //        LineBottom = DutCornerPos[0, 1];
                        //    }

                        //    if (((milScreenTop >= LineBottom && milScreenTop <= LineTop) && (milScreenLeft >= LineLeft && milScreenLeft <= LineRight)) ||
                        //    ((milScreenTop >= LineBottom && milScreenTop <= LineTop) && (milScreenRight >= LineLeft && milScreenRight <= LineRight)) ||
                        //    ((milScreenBottom >= LineBottom && milScreenBottom <= LineTop) && (milScreenLeft >= LineLeft && milScreenLeft <= LineRight)) ||
                        //    ((milScreenBottom >= LineBottom && milScreenBottom <= LineTop) && (milScreenRight >= LineLeft && milScreenRight <= LineRight)) ||
                        //    ((LineTop < milScreenTop && LineBottom > milScreenBottom) && (milScreenLeft > LineLeft && milScreenLeft < LineRight)) ||
                        //    ((LineLeft > milScreenLeft && LineRight < milScreenRight) && (milScreenTop < LineTop && milScreenBottom > LineBottom)) ||
                        //    ((LineTop < milScreenTop && LineLeft > milScreenLeft) && (LineTop > milScreenBottom && LineLeft < milScreenRight)) ||
                        //    ((LineBottom < milScreenTop && LineRight > milScreenLeft) && (LineBottom > milScreenBottom && LineRight < milScreenRight)))
                        //    {
                        //        drawLine1.SetParameter(pt1_x, pt1_y, pt2_x, pt2_y, Colors.BlueViolet, 3);
                        //        cam.DisplayService.DrawOverlayContexts.Add(drawLine1);
                        //    }


                        //    DrawLineModule drawLine2 = new DrawLineModule();
                        //    pt1_x = (DutCornerPos[1, 0] - milScreenLeft) / img.RatioX.Value;
                        //    pt1_y = (milScreenTop - DutCornerPos[1, 1]) / img.RatioY.Value;
                        //    pt2_x = (DutCornerPos[3, 0] - milScreenLeft) / img.RatioX.Value;
                        //    pt2_y = (milScreenTop - (DutCornerPos[3, 1])) / img.RatioY.Value;
                        //    if (DutCornerPos[1, 0] > DutCornerPos[3, 0])
                        //    {
                        //        LineRight = DutCornerPos[1, 0];
                        //        LineLeft = DutCornerPos[3, 0];
                        //    }
                        //    else
                        //    {
                        //        LineRight = DutCornerPos[3, 0];
                        //        LineLeft = DutCornerPos[1, 0];
                        //    }

                        //    if (DutCornerPos[1, 1] > DutCornerPos[3, 1])
                        //    {
                        //        LineTop = DutCornerPos[1, 1];
                        //        LineBottom = DutCornerPos[3, 1];
                        //    }
                        //    else
                        //    {
                        //        LineTop = DutCornerPos[3, 1];
                        //        LineBottom = DutCornerPos[1, 1];
                        //    }

                        //    if (((milScreenTop >= LineBottom && milScreenTop <= LineTop) && (milScreenLeft >= LineLeft && milScreenLeft <= LineRight)) ||
                        //    ((milScreenTop >= LineBottom && milScreenTop <= LineTop) && (milScreenRight >= LineLeft && milScreenRight <= LineRight)) ||
                        //    ((milScreenBottom >= LineBottom && milScreenBottom <= LineTop) && (milScreenLeft >= LineLeft && milScreenLeft <= LineRight)) ||
                        //    ((milScreenBottom >= LineBottom && milScreenBottom <= LineTop) && (milScreenRight >= LineLeft && milScreenRight <= LineRight)) ||
                        //    ((LineTop < milScreenTop && LineBottom > milScreenBottom) && (milScreenLeft > LineLeft && milScreenLeft < LineRight)) ||
                        //    ((LineLeft > milScreenLeft && LineRight < milScreenRight) && (milScreenTop < LineTop && milScreenBottom > LineBottom)) ||
                        //    ((LineTop < milScreenTop && LineLeft > milScreenLeft) && (LineTop > milScreenBottom && LineLeft < milScreenRight)) ||
                        //    ((LineBottom < milScreenTop && LineRight > milScreenLeft) && (LineBottom > milScreenBottom && LineRight < milScreenRight)))
                        //    {
                        //        drawLine2.SetParameter(pt1_x, pt1_y, pt2_x, pt2_y, Colors.BlueViolet, 3);
                        //        cam.DisplayService.DrawOverlayContexts.Add(drawLine2);
                        //    }

                        //    DrawLineModule drawLine3 = new DrawLineModule();
                        //    pt1_x = (DutCornerPos[3, 0] - milScreenLeft) / img.RatioX.Value;
                        //    pt1_y = (milScreenTop - DutCornerPos[3, 1]) / img.RatioY.Value;
                        //    pt2_x = (DutCornerPos[2, 0] - milScreenLeft) / img.RatioX.Value;
                        //    pt2_y = (milScreenTop - (DutCornerPos[2, 1])) / img.RatioY.Value;
                        //    if (DutCornerPos[3, 0] > DutCornerPos[2, 0])
                        //    {
                        //        LineRight = DutCornerPos[3, 0];
                        //        LineLeft = DutCornerPos[2, 0];
                        //    }
                        //    else
                        //    {
                        //        LineRight = DutCornerPos[2, 0];
                        //        LineLeft = DutCornerPos[3, 0];
                        //    }

                        //    if (DutCornerPos[3, 1] > DutCornerPos[2, 1])
                        //    {
                        //        LineTop = DutCornerPos[3, 1];
                        //        LineBottom = DutCornerPos[2, 1];
                        //    }
                        //    else
                        //    {
                        //        LineTop = DutCornerPos[2, 1];
                        //        LineBottom = DutCornerPos[3, 1];
                        //    }

                        //    if (((milScreenTop >= LineBottom && milScreenTop <= LineTop) && (milScreenLeft >= LineLeft && milScreenLeft <= LineRight)) ||
                        //    ((milScreenTop >= LineBottom && milScreenTop <= LineTop) && (milScreenRight >= LineLeft && milScreenRight <= LineRight)) ||
                        //    ((milScreenBottom >= LineBottom && milScreenBottom <= LineTop) && (milScreenLeft >= LineLeft && milScreenLeft <= LineRight)) ||
                        //    ((milScreenBottom >= LineBottom && milScreenBottom <= LineTop) && (milScreenRight >= LineLeft && milScreenRight <= LineRight)) ||
                        //    ((LineTop < milScreenTop && LineBottom > milScreenBottom) && (milScreenLeft > LineLeft && milScreenLeft < LineRight)) ||
                        //    ((LineLeft > milScreenLeft && LineRight < milScreenRight) && (milScreenTop < LineTop && milScreenBottom > LineBottom)) ||
                        //    ((LineTop < milScreenTop && LineLeft > milScreenLeft) && (LineTop > milScreenBottom && LineLeft < milScreenRight)) ||
                        //    ((LineBottom < milScreenTop && LineRight > milScreenLeft) && (LineBottom > milScreenBottom && LineRight < milScreenRight)))
                        //    {
                        //        drawLine3.SetParameter(pt1_x, pt1_y, pt2_x, pt2_y, Colors.BlueViolet, 3);
                        //        cam.DisplayService.DrawOverlayContexts.Add(drawLine3);
                        //    }

                        //    DrawLineModule drawLine4 = new DrawLineModule();
                        //    pt1_x = (DutCornerPos[2, 0] - milScreenLeft) / img.RatioX.Value;
                        //    pt1_y = (milScreenTop - DutCornerPos[2, 1]) / img.RatioY.Value;
                        //    pt2_x = (DutCornerPos[0, 0] - milScreenLeft) / img.RatioX.Value;
                        //    pt2_y = (milScreenTop - (DutCornerPos[0, 1])) / img.RatioY.Value;
                        //    if (DutCornerPos[2, 0] > DutCornerPos[0, 0])
                        //    {
                        //        LineRight = DutCornerPos[2, 0];
                        //        LineLeft = DutCornerPos[0, 0];
                        //    }
                        //    else
                        //    {
                        //        LineRight = DutCornerPos[0, 0];
                        //        LineLeft = DutCornerPos[2, 0];
                        //    }

                        //    if (DutCornerPos[2, 1] > DutCornerPos[0, 1])
                        //    {
                        //        LineTop = DutCornerPos[2, 1];
                        //        LineBottom = DutCornerPos[0, 1];
                        //    }
                        //    else
                        //    {
                        //        LineTop = DutCornerPos[0, 1];
                        //        LineBottom = DutCornerPos[2, 1];
                        //    }

                        //    if (((milScreenTop >= LineBottom && milScreenTop <= LineTop) && (milScreenLeft >= LineLeft && milScreenLeft <= LineRight)) ||
                        //    ((milScreenTop >= LineBottom && milScreenTop <= LineTop) && (milScreenRight >= LineLeft && milScreenRight <= LineRight)) ||
                        //    ((milScreenBottom >= LineBottom && milScreenBottom <= LineTop) && (milScreenLeft >= LineLeft && milScreenLeft <= LineRight)) ||
                        //    ((milScreenBottom >= LineBottom && milScreenBottom <= LineTop) && (milScreenRight >= LineLeft && milScreenRight <= LineRight)) ||
                        //    ((LineTop < milScreenTop && LineBottom > milScreenBottom) && (milScreenLeft > LineLeft && milScreenLeft < LineRight)) ||
                        //    ((LineLeft > milScreenLeft && LineRight < milScreenRight) && (milScreenTop < LineTop && milScreenBottom > LineBottom)) ||
                        //    ((LineTop < milScreenTop && LineLeft > milScreenLeft) && (LineTop > milScreenBottom && LineLeft < milScreenRight)) ||
                        //    ((LineBottom < milScreenTop && LineRight > milScreenLeft) && (LineBottom > milScreenBottom && LineRight < milScreenRight)))
                        //    {
                        //        drawLine4.SetParameter(pt1_x, pt1_y, pt2_x, pt2_y, Colors.BlueViolet, 3);
                        //        cam.DisplayService.DrawOverlayContexts.Add(drawLine4);
                        //    }
                        #endregion
                    }

                    // 현재 위치한 Dut 표시
                    if (iDutNum > 0)
                    {
                        curDutStr = $"DUT : {iDutNum}";
                    }
                    else
                    {
                        curDutStr = $"Out of DUT";
                    }

                    DrawTextModule dutIndexText = new DrawTextModule();

                    // TODO : 가운데 정렬 기능 필요, 전체 String 길이를 고려해, XStart 위치를 쉬프트 해야 한다.
                    // 일단 대충 넣어놓자.

                    dutIndexText.Text = curDutStr;

                    double shiftxleft = (dutIndexText.Text.Length * 9);

                    dutIndexText.XStart = (img.SizeX / 2) - shiftxleft;
                    dutIndexText.YStart = (img.SizeY / 20);

                    dutIndexText.BackColor = Colors.WhiteSmoke;
                    dutIndexText.Fontcolor = Colors.Black;
                    dutIndexText.FontSize = 36;

                    cam.DisplayService.DrawOverlayContexts.Add(dutIndexText);

                    // 핀을 화면에 표시하는 부분  (원래 따로 함수로 있었는데 이사옴)
                    // 어씽크로 Draw 함수 두개를 돌리니까 화면에 텍스트 오버레이가 연달아 두 개씩 표시되는 문제가 발생함.
                    // 그래서 그리는 함수를 하나로 합치고 한방에 그리도록 변경

                    foreach (IDut dut in PCInfo.ProbeCardDevObjectRef.DutList)
                    {
                        foreach (IPinData pin in dut.PinList)
                        {
                            pinstartX = (DieConrerPos[0, 0] + (pin.AbsPos.X.Value));
                            pinstartY = (DieConrerPos[1, 1] + (pin.AbsPos.Y.Value));

                            // Draw Current Die Corner
                            DisplayConerePos[0, 0] = img.CatCoordinates.X.Value - (img.RatioX.Value * (img.SizeX / 2));
                            DisplayConerePos[0, 1] = img.CatCoordinates.Y.Value - (img.RatioY.Value * (img.SizeY / 2));
                            DisplayConerePos[1, 0] = img.CatCoordinates.X.Value + (img.RatioX.Value * (img.SizeX / 2));
                            DisplayConerePos[1, 1] = img.CatCoordinates.Y.Value + (img.RatioY.Value * (img.SizeY / 2));

                            if (pin.PinSearchParam.PinSize.Value.Width != 0)
                            {
                                radius = pin.PinSearchParam.PinSize.Value.Width / 2;
                            }
                            else if (pin.PinSearchParam.MinBlobSizeX.Value != 0 && pin.PinSearchParam.MaxBlobSizeX.Value != 0)
                            {
                                radius = ((pin.PinSearchParam.MinBlobSizeX.Value + pin.PinSearchParam.MaxBlobSizeX.Value) / 2) / 2;
                            }
                            else
                            {
                                radius = 200;
                            }

                            PROBECARD_TYPE cardtype = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.ProbeCardType.Value;

                            //radius = 50;
                            if (pinstartX > milScreenLeft &&
                                pinstartX < milScreenRight &&
                                pinstartY < milScreenTop &&
                                pinstartY > milScreenBottom)
                            {
                                pincetnerx = (pinstartX - milScreenLeft) / img.RatioX.Value;
                                pincentery = (milScreenTop - pinstartY) / img.RatioY.Value;

                                if (cam.CameraChannel.Type == EnumProberCam.PIN_LOW_CAM)
                                {
                                    radius = radius / 10;
                                }
                                drawEclipse =
                                new DrawEllipseModule(
                                    pincetnerx,
                                    pincentery,
                                    radius,
                                    verflip: this.VisionManager().GetDispVerFlip(),
                                    horflip: this.VisionManager().GetDispHorFlip(),
                                    left: 0, right: img.SizeX, top: 0, bottom: img.SizeY);


                                str = string.Empty;

                                if (pin.IsRefPin.Value)
                                {
                                    str = "#Ref Pin";
                                }
                                else
                                {
                                    str = "#" + pin.PinNum.Value;
                                }

                                if (this.PinAligner().ModuleState.GetState() == ModuleStateEnum.RECOVERY)
                                {
                                    str += "\n";
                                    str += "AlignResult = " + pin.Result.Value.ToString();
                                }

                                //drawText = new DrawTextModule((pincetnerx), (pincentery - radius), str);
                                drawText = new DrawTextModule();
                                drawText.XStart = pincetnerx + radius;
                                drawText.YStart = pincentery - radius * 3;
                                drawText.Text = str;

                                drawText.BackColor = Colors.WhiteSmoke;
                                drawText.Fontcolor = Colors.Black;
                                drawText.FontSize = 12;

                                drawEclipse.Color = Colors.Yellow;

                                cam.DisplayService.DrawOverlayContexts.Add(drawEclipse);
                                cam.DisplayService.DrawOverlayContexts.Add(drawText);

                                if (PinSetupControl != null)
                                {
                                    if (PinSetupControl.ShowTipBlobMin == true)
                                    {
                                        double BlobMinWidth = 0;
                                        double BlobMinHeight = 0;

                                        if ((pin.PinSearchParam.MinBlobSizeX != null) && (pin.PinSearchParam.MinBlobSizeY != null) &&
                                            (pin.PinSearchParam.MinBlobSizeX.Value > 0) && (pin.PinSearchParam.MinBlobSizeY.Value > 0))
                                        {
                                            if (cardtype != PROBECARD_TYPE.MEMS_Dual_AlignKey)
                                            {
                                                BlobMinWidth = pin.PinSearchParam.MinBlobSizeX.Value;
                                                BlobMinHeight = pin.PinSearchParam.MinBlobSizeY.Value;
                                            }
                                            else
                                            {
                                                BlobMinWidth = pin.PinSearchParam.MinBlobSizeX.Value / cam.GetRatioX();
                                                BlobMinHeight = pin.PinSearchParam.MinBlobSizeY.Value / cam.GetRatioY();
                                            }

                                            drawAlignKeyRectangle = new DrawRectangleModule(pincetnerx, pincentery, BlobMinWidth, BlobMinHeight, verflip: this.VisionManager().GetDispVerFlip(), horflip: this.VisionManager().GetDispHorFlip(), left: 0, right: img.SizeX, top: 0, bottom: img.SizeY);
                                            drawAlignKeyRectangle.Color = Colors.Blue;

                                            cam.DisplayService.DrawOverlayContexts.Add(drawAlignKeyRectangle);
                                        }
                                    }

                                    if (PinSetupControl.ShowTipBlobMax == true)
                                    {
                                        double BlobMaxWidth = 0;
                                        double BlobMaxHeight = 0;

                                        if ((pin.PinSearchParam.MaxBlobSizeX != null) && (pin.PinSearchParam.MaxBlobSizeY != null) &&
                                            (pin.PinSearchParam.MaxBlobSizeX.Value > 0) && (pin.PinSearchParam.MaxBlobSizeY.Value > 0)
                                            )
                                        {
                                            if (cardtype != PROBECARD_TYPE.MEMS_Dual_AlignKey)
                                            {
                                                BlobMaxWidth = pin.PinSearchParam.MaxBlobSizeX.Value;
                                                BlobMaxHeight = pin.PinSearchParam.MaxBlobSizeY.Value;
                                            }
                                            else
                                            {
                                                BlobMaxWidth = pin.PinSearchParam.MaxBlobSizeX.Value / cam.GetRatioX();
                                                BlobMaxHeight = pin.PinSearchParam.MaxBlobSizeY.Value / cam.GetRatioY();
                                            }

                                            drawAlignKeyRectangle = new DrawRectangleModule(pincetnerx, pincentery, BlobMaxWidth, BlobMaxHeight, verflip: this.VisionManager().GetDispVerFlip(), horflip: this.VisionManager().GetDispHorFlip(), left: 0, right: img.SizeX, top: 0, bottom: img.SizeY);
                                            drawAlignKeyRectangle.Color = Colors.Green;

                                            cam.DisplayService.DrawOverlayContexts.Add(drawAlignKeyRectangle);
                                        }
                                    }

                                    if (PinSetupControl.ShowTipSearchArea == true)
                                    {
                                        var searchArea = pin.PinSearchParam.SearchArea.Value;

                                        if (searchArea.Width > 0 && searchArea.Height > 0)
                                        {
                                            double SearchAreaWidth;
                                            double SearchAreaHeight;

                                            if (cardtype != PROBECARD_TYPE.MEMS_Dual_AlignKey)
                                            {
                                                SearchAreaWidth = searchArea.Width;
                                                SearchAreaHeight = searchArea.Height;
                                            }
                                            else
                                            {
                                                SearchAreaWidth = searchArea.Width / cam.GetRatioX();
                                                SearchAreaHeight = searchArea.Height / cam.GetRatioY();
                                            }

                                            drawAlignKeyRectangle = new DrawRectangleModule(pincetnerx, pincentery, SearchAreaWidth, SearchAreaHeight, verflip: this.VisionManager().GetDispVerFlip(), horflip: this.VisionManager().GetDispHorFlip(), left: 0, right: img.SizeX, top: 0, bottom: img.SizeY);
                                            drawAlignKeyRectangle.Color = Colors.Violet;

                                            cam.DisplayService.DrawOverlayContexts.Add(drawAlignKeyRectangle);
                                        }
                                    }
                                }
                            }

                            if (PinSetupControl != null)
                            {
                                if (PinSetupControl.ShowBaseFocusingArea == true)
                                {
                                    PinAlignDevParameters pindevparam = (this.PinAligner().PinAlignDevParam as PinAlignDevParameters);

                                    var focusingROI = pindevparam.PinHighAlignBaseFocusingParam.FocusingParam.FocusingROI.Value;

                                    if (focusingROI.Width > 0 && focusingROI.Height > 0)
                                    {
                                        double OffsetX = ((int)cam.GetGrabSizeWidth() / 2) - (int)(focusingROI.Width / 2);
                                        double OffsetY = ((int)cam.GetGrabSizeHeight() / 2) - (int)(focusingROI.Height / 2);

                                        double centerX = (OffsetX + (focusingROI.Width / 2));
                                        double centerY = (OffsetY + (focusingROI.Height / 2));

                                        drawFocusingAreaRectangle = new DrawRectangleModule(centerX, centerY, focusingROI.Width, focusingROI.Height);

                                        drawFocusingAreaRectangle.Color = Colors.Red;
                                        drawFocusingAreaRectangle.Thickness = 1;
                                        cam.DisplayService.DrawOverlayContexts.Add(drawFocusingAreaRectangle);
                                    }
                                }
                            }

                            if ((pin.PinSearchParam.AlignKeyHigh != null) && (pin.PinSearchParam.AlignKeyHigh.Count > 0) && cardtype == PROBECARD_TYPE.MEMS_Dual_AlignKey)//AlignKey 그리기
                            {
                                var keyIndex = pin.PinSearchParam.AlignKeyIndex.Value;
                                var alignKey = pin.PinSearchParam.AlignKeyHigh[keyIndex];

                                if (pin.PinSearchParam.AlignKeyHigh.Count > keyIndex & pin.PinSearchParam.AlignKeyHigh.Count > 0)
                                {
                                    alignKeyStartX = pinstartX + alignKey.AlignKeyPos.GetX();
                                    alignKeyStartY = pinstartY + alignKey.AlignKeyPos.GetY();

                                    if (alignKeyStartX > milScreenLeft &&
                                        alignKeyStartX < milScreenRight &&
                                        alignKeyStartY < milScreenTop &&
                                        alignKeyStartY > milScreenBottom)
                                    {

                                        if (alignKey.ImageProcType == IMAGE_PROC_TYPE.PROC_BLOB)
                                        {
                                            alignKeyWidth = alignKey.BlobSizeX.Value / cam.GetRatioX(); //0.365; // img.RatioX.Value;
                                            alignKeyHeight = alignKey.BlobSizeY.Value / cam.GetRatioY(); //0.365; //img.RatioY.Value;
                                        }
                                        else if (alignKey.ImageProcType == IMAGE_PROC_TYPE.PROC_PATTERN_MATCHING)
                                        {
                                            alignKeyWidth = alignKey.PatternIfo.PMParameter.PattWidth.Value / cam.GetRatioX(); //0.365;// img.RatioX.Value;
                                            alignKeyHeight = alignKey.PatternIfo.PMParameter.PattHeight.Value / cam.GetRatioY(); //0.365; // img.RatioY.Value;
                                        }

                                        alignkeyceterx = (alignKeyStartX - milScreenLeft) / img.RatioX.Value;
                                        alignkeycetery = (milScreenTop - alignKeyStartY) / img.RatioY.Value;
                                        //drawAlignKeyRectangle = new DrawRectangleModule(pincetnerx + pin.PinSearchParam.HighAlignkeyPosition.GetX(), pincentery + pin.PinSearchParam.HighAlignkeyPosition.GetY(),
                                        //    pin.PinSearchParam.HighAlignKeySize.Value.Width, pin.PinSearchParam.HighAlignKeySize.Value.Height);

                                        drawAlignKeyRectangle = new DrawRectangleModule(alignkeyceterx, alignkeycetery,
                                               alignKeyWidth, alignKeyHeight,
                                                verflip: this.VisionManager().GetDispVerFlip(), horflip: this.VisionManager().GetDispHorFlip(),
                                                left: 0, right: img.SizeX, top: 0, bottom: img.SizeY);
                                        drawAlignKeyRectangle.Color = Colors.SkyBlue;

                                        cam.DisplayService.DrawOverlayContexts.Add(drawAlignKeyRectangle);

                                        if (PinSetupControl.ShowKeySearchArea == true)
                                        {
                                            double SearchAreaWidth = 0;
                                            double SearchAreaHeight = 0;

                                            bool ExtensionBlob = false;

                                            PinAlignDevParameters pindevparam = (this.PinAligner().PinAlignDevParam as PinAlignDevParameters);

                                            if (pindevparam != null)
                                            {
                                                ExtensionBlob = pindevparam.PinHighAlignParam.HighAlignKeyParameter.KeyBlobExtension.Value;
                                            }

                                            if (ExtensionBlob == false)
                                            {
                                                if ((alignKey.BlobRoiSizeX != null) && (alignKey.BlobRoiSizeY != null) &&
                                                (alignKey.BlobRoiSizeX.Value > 0) && (alignKey.BlobRoiSizeY.Value > 0)
                                                )
                                                {
                                                    SearchAreaWidth = alignKey.BlobRoiSizeX.Value / cam.GetRatioX();
                                                    SearchAreaHeight = alignKey.BlobRoiSizeY.Value / cam.GetRatioY();

                                                    drawAlignKeyRectangle = new DrawRectangleModule(alignkeyceterx, alignkeycetery, SearchAreaWidth, SearchAreaHeight,
                                                                                                    verflip: this.VisionManager().GetDispVerFlip(), horflip: this.VisionManager().GetDispHorFlip(),
                                                                                                    left: 0, right: img.SizeX, top: 0, bottom: img.SizeY);
                                                    drawAlignKeyRectangle.Color = Colors.Green;

                                                    cam.DisplayService.DrawOverlayContexts.Add(drawAlignKeyRectangle);
                                                }
                                            }
                                            else
                                            {
                                                double ExtensionWidth = pindevparam.PinHighAlignParam.HighAlignKeyParameter.KeyBlobExtensionWidth.Value;
                                                double ExtensionHeight = pindevparam.PinHighAlignParam.HighAlignKeyParameter.KeyBlobExtensionHeight.Value;

                                                int RealSearchAreaX = 0;
                                                int RealSearchAreaY = 0;

                                                int ROISizeX = 0;
                                                int ROISizeY = 0;

                                                int OffsetX = 0;
                                                int OffsetY = 0;

                                                int ExtensionWidthPixel = 0;
                                                int ExtensionHeightPixel = 0;

                                                if (ExtensionWidth > 0)
                                                {
                                                    ExtensionWidthPixel = Convert.ToInt32(ExtensionWidth / cam.GetRatioX());
                                                }

                                                if (ExtensionHeight > 0)
                                                {
                                                    ExtensionHeightPixel = Convert.ToInt32(ExtensionHeight / cam.GetRatioY());
                                                }

                                                int SignX = alignKey.AlignKeyPos.X.Value < 0 ? -1 : 1;
                                                int SignY = alignKey.AlignKeyPos.Y.Value > 0 ? -1 : 1;

                                                ROISizeX = Convert.ToInt32(alignKey.BlobRoiSizeX.Value / cam.GetRatioX());
                                                ROISizeY = Convert.ToInt32(alignKey.BlobRoiSizeY.Value / cam.GetRatioY());

                                                OffsetX = (SignX * ExtensionWidthPixel);
                                                OffsetY = (SignY * ExtensionHeightPixel);

                                                //if (SignX == -1)
                                                //{
                                                //    OffsetX = ((int)cam.GetGrabSizeWidth() / 2) - (int)(ROISizeX / 2) + (SignX * ExtensionWidthPixel);
                                                //}
                                                //else
                                                //{
                                                //    OffsetX = ((int)cam.GetGrabSizeWidth() / 2) - (int)(ROISizeX / 2);
                                                //}

                                                //if (SignY == -1)
                                                //{
                                                //    OffsetY = ((int)cam.GetGrabSizeHeight() / 2) - (int)(ROISizeY / 2) + (SignY * ExtensionHeightPixel);
                                                //}
                                                //else
                                                //{
                                                //    OffsetY = ((int)cam.GetGrabSizeHeight() / 2) - (int)(ROISizeY / 2);
                                                //}

                                                RealSearchAreaX = ROISizeX + ExtensionWidthPixel;
                                                RealSearchAreaY = ROISizeY + ExtensionHeightPixel;

                                                if ((alignKey.BlobRoiSizeX != null) && (alignKey.BlobRoiSizeY != null) &&
                                                (alignKey.BlobRoiSizeX.Value > 0) && (alignKey.BlobRoiSizeY.Value > 0)
                                                )
                                                {
                                                    //SearchAreaWidth = alignKey.BlobRoiSizeX.Value / cam.GetRatioX();
                                                    //SearchAreaHeight = alignKey.BlobRoiSizeY.Value / cam.GetRatioY();

                                                    //drawAlignKeyRectangle = new DrawRectangleModule(alignkeyceterx, alignkeycetery, SearchAreaWidth, SearchAreaHeight);
                                                    //drawAlignKeyRectangle.Color = Colors.Green;

                                                    drawAlignKeyRectangle = new DrawRectangleModule(alignkeyceterx, alignkeycetery, RealSearchAreaX, RealSearchAreaY, OffsetX, OffsetY,
                                                                                                    verflip: this.VisionManager().GetDispVerFlip(), horflip: this.VisionManager().GetDispHorFlip(),
                                                                                                    left: 0, right: img.SizeX, top: 0, bottom: img.SizeY);
                                                    drawAlignKeyRectangle.Color = Colors.Green;

                                                    cam.DisplayService.DrawOverlayContexts.Add(drawAlignKeyRectangle);
                                                }
                                            }
                                        }

                                        if (PinSetupControl.ShowKeyBlobMin == true)
                                        {
                                            double BlobMinWidth = 0;
                                            double BlobMinHeight = 0;

                                            if ((alignKey.BlobSizeMinX != null) && (alignKey.BlobSizeMinY != null) &&
                                                (alignKey.BlobSizeMinX.Value > 0) && (alignKey.BlobSizeMinY.Value > 0)
                                                )
                                            {
                                                BlobMinWidth = alignKey.BlobSizeMinX.Value / cam.GetRatioX();
                                                BlobMinHeight = alignKey.BlobSizeMinY.Value / cam.GetRatioY();

                                                drawAlignKeyRectangle = new DrawRectangleModule(alignkeyceterx, alignkeycetery, BlobMinWidth, BlobMinHeight,
                                                    verflip: this.VisionManager().GetDispVerFlip(), horflip: this.VisionManager().GetDispHorFlip(),
                                                    left: 0, right: img.SizeX, top: 0, bottom: img.SizeY);
                                                drawAlignKeyRectangle.Color = Colors.Violet;

                                                cam.DisplayService.DrawOverlayContexts.Add(drawAlignKeyRectangle);
                                            }
                                        }

                                        if (PinSetupControl.ShowKeyBlobMax == true)
                                        {
                                            double BlobMaxWidth = 0;
                                            double BlobMaxHeight = 0;

                                            if ((alignKey.BlobSizeMaxX != null) && (alignKey.BlobSizeMaxY != null) &&
                                                (alignKey.BlobSizeMaxX.Value > 0) && (alignKey.BlobSizeMaxY.Value > 0)
                                                )
                                            {
                                                BlobMaxWidth = alignKey.BlobSizeMaxX.Value / cam.GetRatioX();
                                                BlobMaxHeight = alignKey.BlobSizeMaxY.Value / cam.GetRatioY();

                                                drawAlignKeyRectangle = new DrawRectangleModule(alignkeyceterx, alignkeycetery, BlobMaxWidth, BlobMaxHeight,
                                                    verflip: this.VisionManager().GetDispVerFlip(), horflip: this.VisionManager().GetDispHorFlip(),
                                                    left: 0, right: img.SizeX, top: 0, bottom: img.SizeY);
                                                drawAlignKeyRectangle.Color = Colors.Cyan;

                                                cam.DisplayService.DrawOverlayContexts.Add(drawAlignKeyRectangle);
                                            }
                                        }

                                        if (PinSetupControl.ShowKeyFocusingArea == true)
                                        {
                                            double focusingAreaX = 0;
                                            double focusingAreaY = 0;
                                            double OffsetX = 0;
                                            double OffsetY = 0;
                                            double ratioX = cam.GetRatioX();
                                            double ratioY = cam.GetRatioY();

                                            PinAlignDevParameters pindevparam = (this.PinAligner().PinAlignDevParam as PinAlignDevParameters);
                                            if (pindevparam.PinHighAlignParam.HighAlignKeyParameter.KeyFocusingExtension.Value)
                                            {
                                                double ExtensionWidth = pindevparam.PinHighAlignParam.HighAlignKeyParameter.KeyFocusingExtensionWidth.Value;
                                                double ExtensionHeight = pindevparam.PinHighAlignParam.HighAlignKeyParameter.KeyFocusingExtensionHeight.Value;

                                                int ExtensionWidthPixel = 0;
                                                int ExtensionHeightPixel = 0;

                                                if (ExtensionWidth > 0)
                                                {
                                                    ExtensionWidthPixel = Convert.ToInt32(ExtensionWidth / ratioX);
                                                }

                                                if (ExtensionHeight > 0)
                                                {
                                                    ExtensionHeightPixel = Convert.ToInt32(ExtensionHeight / ratioY);
                                                }

                                                int SignX = alignKey.AlignKeyPos.X.Value < 0 ? -1 : 1;
                                                int SignY = alignKey.AlignKeyPos.Y.Value > 0 ? -1 : 1;

                                                int ROISizeX = Convert.ToInt32(alignKey.BlobRoiSizeX.Value / ratioX);
                                                int ROISizeY = Convert.ToInt32(alignKey.BlobRoiSizeY.Value / ratioY);


                                                if (SignX == -1)
                                                {
                                                    OffsetX = ((int)cam.GetGrabSizeWidth() / 2) - (int)(ROISizeX / 2) + (SignX * ExtensionWidthPixel);
                                                }
                                                else
                                                {
                                                    OffsetX = ((int)cam.GetGrabSizeWidth() / 2) - (int)(ROISizeX / 2);
                                                }

                                                if (SignY == -1)
                                                {
                                                    OffsetY = ((int)cam.GetGrabSizeHeight() / 2) - (int)(ROISizeY / 2) + (SignY * ExtensionHeightPixel);
                                                }
                                                else
                                                {
                                                    OffsetY = ((int)cam.GetGrabSizeHeight() / 2) - (int)(ROISizeY / 2);
                                                }

                                                focusingAreaX = ROISizeX + ExtensionWidthPixel;
                                                focusingAreaY = ROISizeY + ExtensionHeightPixel;
                                            }
                                            else
                                            {
                                                int ROISizeX = Convert.ToInt32(alignKey.BlobRoiSizeX.Value / ratioX);
                                                int ROISizeY = Convert.ToInt32(alignKey.BlobRoiSizeY.Value / ratioY);

                                                OffsetX = ((int)cam.GetGrabSizeWidth() / 2) - (int)(ROISizeX / 2);
                                                OffsetY = ((int)cam.GetGrabSizeHeight() / 2) - (int)(ROISizeY / 2);

                                                focusingAreaX = ROISizeX;
                                                focusingAreaY = ROISizeY;
                                            }


                                            //OffsetX = (OffsetX) / img.RatioX.Value;
                                            //OffsetY = (OffsetY) / img.RatioY.Value;

                                            //focusingAreaX = focusingAreaX / img.RatioX.Value;
                                            //focusingAreaY = focusingAreaY / img.RatioY.Value;

                                            double centerX = (OffsetX + (focusingAreaX / 2));
                                            double centerY = (OffsetY + (focusingAreaY / 2));

                                            //double canvasWidth = cam.DisplayService.OverlayCanvas.ActualWidth;
                                            //if (canvasWidth == 0)
                                            //    canvasWidth = 890;
                                            //double canvasHeight = cam.DisplayService.OverlayCanvas.ActualHeight;
                                            //if (canvasHeight == 0)
                                            //    canvasHeight = 890;
                                            //double convertOffsetX = (canvasWidth * OffsetX) / img.SizeX;
                                            //double convertOffsetY = (canvasHeight * OffsetY) / img.SizeY;
                                            //double convertAreaX = (canvasWidth * focusingAreaX) / img.SizeX;
                                            //double convertArezY = (canvasHeight * focusingAreaY) / img.SizeY;

                                            //double centerX = (convertOffsetX + (convertAreaX / 2));
                                            //double centerY = (convertOffsetY + (convertArezY / 2));



                                            drawFocusingAreaRectangle = new DrawRectangleModule(centerX, centerY, focusingAreaX, focusingAreaY,
                                                verflip: this.VisionManager().GetDispVerFlip(), horflip: this.VisionManager().GetDispHorFlip(),
                                                    left: 0, right: img.SizeX, top: 0, bottom: img.SizeY);
                                            LoggerManager.Debug($"[Draw Pin Focusing ROI] OffsetX : {OffsetX}, OffsetY : {OffsetY}" +
                                                $", AreaX : {focusingAreaX}, AreaY : {focusingAreaX}, centerX : {centerX}, cetnerY : {centerY}");
                                            drawFocusingAreaRectangle.Color = Colors.Red;
                                            drawFocusingAreaRectangle.Thickness = 1;
                                            cam.DisplayService.DrawOverlayContexts.Add(drawFocusingAreaRectangle);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    //cam.DisplayService.Draw(img);
                    LoggerManager.Debug("ProbeCardDutOverlay");
                    //}
                    //}));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                //LoggerManager.Error($err, "OverlayDie() Error occurred.");
            }
            return retVal;
        }

        //public EventCodeEnum DrawPinOverlay(ImageBuffer img, ICamera cam)
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    try
        //    {
        //        return EventCodeEnum.NONE;

        //        if (cam.GetChannelType() == EnumProberCam.PIN_HIGH_CAM ||
        //            cam.GetChannelType() == EnumProberCam.PIN_LOW_CAM && OnOff == true)
        //        {
        //            drawText = new DrawTextModule();
        //            drawEclipse = new DrawEllipseModule();

        //            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
        //            {
        //                Display = cam.DisplayService;

        //                foreach (IDut dut in PCInfo.DutList)
        //                {
        //                    foreach (IPinData pin in dut.PinList)
        //                    {
        //                        double[,] DieConrerPos = new double[2, 2];
        //                        double[,] DisplayConerePos = new double[2, 2];

        //                            // Draw Current Die Corner

        //                            DisplayConerePos[0, 0] = img.CatCoordinates.X.Value - (img.RatioX.Value * (img.SizeX / 2));
        //                        DisplayConerePos[0, 1] = img.CatCoordinates.Y.Value - (img.RatioY.Value * (img.SizeY / 2));

        //                        DisplayConerePos[1, 0] = img.CatCoordinates.X.Value + (img.RatioX.Value * (img.SizeX / 2));
        //                        DisplayConerePos[1, 1] = img.CatCoordinates.Y.Value + (img.RatioY.Value * (img.SizeY / 2));

        //                        double pt1_x;
        //                        double pt1_y;
        //                        double pt2_x;
        //                        double pt2_y;

        //                        bool flag;

        //                        double milScreenLeft = img.CatCoordinates.X.Value - (img.SizeX / 2 * img.RatioX.Value);
        //                        double milScreenTop = img.CatCoordinates.Y.Value + (img.SizeY / 2 * img.RatioY.Value);
        //                        double milScreenRight = img.CatCoordinates.X.Value + (img.SizeX / 2 * img.RatioX.Value);
        //                        double milScreenBottom = img.CatCoordinates.Y.Value - (img.SizeY / 2 * img.RatioY.Value);


        //                        double pinstartX = (DieConrerPos[0, 0] + (pin.AbsPos.X.Value));

        //                        double pinstartY = (DieConrerPos[1, 1] + (pin.AbsPos.Y.Value));
        //                        double radius = 0;
        //                        if (pin.PinSearchParam.PinSize.Value.Width != 0)
        //                        {
        //                            radius = pin.PinSearchParam.PinSize.Value.Width / 2;
        //                        }
        //                        else if (pin.PinSearchParam.MinBlobSizeX.Value != 0 && pin.PinSearchParam.MaxBlobSizeX.Value != 0)
        //                        {
        //                            radius = ((pin.PinSearchParam.MinBlobSizeX.Value + pin.PinSearchParam.MaxBlobSizeX.Value) / 2) / 2;
        //                        }
        //                        else
        //                        {
        //                            radius = 200;
        //                        }
        //                        radius = 50;

        //                        if (pinstartX > milScreenLeft &&
        //                              pinstartX < milScreenRight &&
        //                              pinstartY < milScreenTop &&
        //                              pinstartY > milScreenBottom)
        //                        {
        //                            double pincetnerx = (pinstartX - milScreenLeft) / img.RatioX.Value;
        //                            double pincentery = (milScreenTop - pinstartY) / img.RatioY.Value;

        //                                //idx = ;

        //                                if (cam.CameraChannel.Type == EnumProberCam.PIN_LOW_CAM)
        //                            {
        //                                radius = radius / 10;
        //                            }
        //                            drawEclipse =
        //                            new DrawEllipseModule(
        //                                pincetnerx,
        //                                pincentery,
        //                                radius);

        //                            string str = string.Empty;
        //                            if (pin.IsRefPin.Value)
        //                            {
        //                                str = "#Ref Pin";
        //                            }
        //                            else
        //                            {
        //                                str = "#" + pin.PinNum.Value;
        //                            }

        //                            if (this.PinAligner().ModuleState.GetState() == ModuleStateEnum.RECOVERY)
        //                            {
        //                                str += "\n";
        //                                str += "AlignResult = " + pin.Result.Value.ToString();
        //                            }

        //                                //drawText = new DrawTextModule((pincetnerx), (pincentery - radius), str);
        //                                drawText.XStart = pincetnerx;
        //                            drawText.YStart = pincentery - radius;
        //                            drawText.Text = str;

        //                            drawText.BackColor = Colors.WhiteSmoke;
        //                            drawText.Fontcolor = Colors.Black;
        //                            drawText.FontSize = 12;

        //                            drawEclipse.Color = Colors.Yellow;

        //                            cam.DisplayService.DrawOverlayContexts.Add(drawEclipse);
        //                            cam.DisplayService.DrawOverlayContexts.Add(drawText);
        //                        }
        //                    }
        //                }

        //                cam.DisplayService.Draw(img);
        //            }));

        //        }
        //        retVal = EventCodeEnum.NONE;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);

        //        //LoggerManager.Error($err, "OverlayPin() Error occurred.");
        //    }
        //    return retVal;
        //}
        private void GetRotCoordEx(ref PinCoordinate NewPos, PinCoordinate OriPos, PinCoordinate RefPos, double angle)
        {
            double newx = 0.0;
            double newy = 0.0;
            double th = DegreeToRadian(angle);

            if (NewPos == null)
                NewPos = new PinCoordinate();
            newx = OriPos.X.Value - RefPos.X.Value;
            newy = OriPos.Y.Value - RefPos.Y.Value;

            NewPos.X.Value = newx * Math.Cos(th) - newy * Math.Sin(th) + RefPos.X.Value;
            NewPos.Y.Value = newx * Math.Sin(th) + newy * Math.Cos(th) + RefPos.Y.Value;
        }
        private double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }
    }
}
