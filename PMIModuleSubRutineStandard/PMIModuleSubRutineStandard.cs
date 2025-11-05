using Focusing;
using Genericalgorithm;
using GeometryHelp;
using LogModule;
using PMIModuleParameter;
using PnPControl;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Enum;
using ProberInterfaces.LightJog;
using ProberInterfaces.Param;
using ProberInterfaces.PMI;
using ProberInterfaces.PnpSetup;
using ProberInterfaces.Vision;
using ProberInterfaces.VisionFramework;
using SharpDXRender;
using SharpDXRender.RenderObjectPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;


namespace PMIModuleSubRutineStandard
{
    using WinSize = System.Windows.Size;

    public class PMIModuleSubRutineStandard : IPMIModuleSubRutine, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ==> Common Declaration
        public PMIModuleSubRutineStandard()
        {
        }

        #region RenderObject

        private AsyncObservableCollection<RenderObject> RenderObject_Template = new AsyncObservableCollection<RenderObject>();
        private AsyncObservableCollection<RenderObject> RenderObject_JudgingWindow = new AsyncObservableCollection<RenderObject>();
        private AsyncObservableCollection<RenderObject> RenderObject_MarkMin = new AsyncObservableCollection<RenderObject>();
        private AsyncObservableCollection<RenderObject> RenderObject_MarkMax = new AsyncObservableCollection<RenderObject>();
        private AsyncObservableCollection<RenderObject> RenderObject_RegisteredPad = new AsyncObservableCollection<RenderObject>();
        private AsyncObservableCollection<RenderObject> RenderObject_RegisteredPadIndex = new AsyncObservableCollection<RenderObject>();
        private AsyncObservableCollection<RenderObject> RenderObject_DetectedMarks = new AsyncObservableCollection<RenderObject>();

        private AsyncObservableCollection<RenderObject> RenderObject_Proximityline = new AsyncObservableCollection<RenderObject>();

        #endregion

        //private bool MakeTestMarkDatas = false;

        //private bool MakeTestMarkUsingRealDatas = false;

        //private bool MakeTestSearchPadDatas = false;
        //private bool MakeTestProximitylineDatas = false;

        //private bool MakeTestOverlap = false;
        //private bool MakeTestSaveImage = false;

        public IPMIInfo PMIInfo
        {
            get { return this.StageSupervisor().WaferObject.GetSubsInfo().GetPMIInfo(); }
        }

        public PadGroup PadInfos
        {
            get { return this.StageSupervisor().WaferObject.GetSubsInfo().Pads; }
        }

        public PMIModuleDevParam PMIDevParam
        {
            get { return this.PMIModule().PMIModuleDevParam_IParam as PMIModuleDevParam; }
        }

        public PMIModuleSysParam PMISysParam
        {
            get { return this.PMIModule().PMIModuleSysParam_IParam as PMIModuleSysParam; }
        }

        private object SubModule { get; set; }

        //private double TemplateSizeScalingUnit { get; set; }
        //private double JudgingWindowSizeScalingUnit { get; set; }
        private double ProbeMarkSizeScalingUnit { get; set; }
        //private double TemplateOffsetScalingUnit { get; set; }

        private Point RenderLayerRatio { get; set; }

        private WinSize LayerSize { get; set; }

        public WinSize GetLayerSize()
        {
            return LayerSize;
        }
        public EventCodeEnum ParamValidation()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }
        #endregion

        public bool Initialized { get; set; } = false;

        #region ==> Modules
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    LayerSize = new WinSize(890, 890);

                    //TemplateSizeScalingUnit = 1;
                    //JudgingWindowSizeScalingUnit = 0.1;
                    //ProbeMarkSizeScalingUnit = 1;
                    //TemplateOffsetScalingUnit = 0.01;

                    //SelectedPMIPadIndex = 0;

                    //SelectedPadTemplate = null;
                    //SelectedPadTableTemplate = null;

                    // Table Setup에서만 ENABLE 가능
                    //IsTabelSetup = PMI_RENDER_TABLE_MODE.DISABLE;


                    //PMIDevParam = ((this.PMIModule() as IHasDevParameterizable).DevParam) as PMIModuleDevParam;

                    //PMIPadTemplateIndex = 0;

                    //Wafer = (this.StageSupervisor().WaferObject) as WaferObject;

                    // Init
                    //SelectedPadTemplateIndex = 0;
                    //PMIInfo.SelectedNormalPMIMapTemplateIndex = 0;



                    //int padtemplateindex = PMIInfo.GetSelectedPadTemplateIndex();
                    //int normalmaptemplateindex = PMIInfo.GetSelectedNormalPMIMapIndex();

                    //SelectedPadTemplate = PMIInfo.GetPadTemplate(padtemplateindex);
                    //CurrentPMIMapTemplate = PMIInfo.GetNormalPMIMapTemplate(normalmaptemplateindex);

                    //Layer = new PMIRenderLayer(this.PMIModule());
                    //Layer.BackgroundColor = new RawColor4(50 / 255f, 50 / 255f, 50 / 255f, 0);

                    //WinSize layerSize = new WinSize();

                    //layerSize.Width = 890;
                    //layerSize.Height = 890;

                    //Layer = new PMIRenderLayer(layerSize);

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
                LoggerManager.Debug($"[PMIModuleSubRutineStandard] [InitModule()] : {err}");
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void DeInitModule()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        #endregion

        public RenderLayer InitPMIRenderLayer(WinSize size, float r, float g, float b, float a)
        {
            RenderLayer layer = new PMIRenderLayer(size, r, g, b, a);
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return layer;
        }

        public Point GetRenderLayerRatio()
        {
            return RenderLayerRatio;
        }

        public void UpdateLabel()
        {
            try
            {
                if (SubModule != null)
                {
                    var pnpmodule = (SubModule as IPnpSetup);

                    if (pnpmodule != null)
                    {
                        pnpmodule.UpdateLabel();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        #region ==> Set Functions
        public void SetSubModule(object SubModule)
        {
            this.SubModule = SubModule;
        }
        #endregion

        #region ==> Get Functions

        public object GetSubModule()
        {
            return this.SubModule;
        }
        /// <summary>
        /// RenderLayer를 리턴하는 함수
        /// </summary>
        /// <returns></returns>
        //public RenderLayer GetPMIRenderLayer()
        //{
        //    return Layer;
        //}

        /// <summary>
        /// Geometry 데이터로부터 RenderObject를 생성하는 함수
        /// </summary>
        /// <param name="geo"></param>
        /// <param name="color"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        private RenderObject GetRenderGeometryObject(Geometry geo, string color, bool fill = false, Transform t = null)
        {
            RenderGeometry rg = null;

            try
            {
                List<Point> pts = GetPointsOnFlattenedPath(geo, t);

                if (pts != null)
                {
                    float centerX = Convert.ToSingle(((geo.Bounds.Left + geo.Bounds.Right) / 2));
                    float centerY = Convert.ToSingle(((geo.Bounds.Top + geo.Bounds.Bottom) / 2));
                    float width = Convert.ToSingle(geo.Bounds.Width);
                    float height = Convert.ToSingle(geo.Bounds.Height);

                    rg = new RenderGeometry(centerX, centerY, width, height, color);

                    rg.SetGeometryPoint(pts);
                    rg.SetFill(fill);
                }

                return rg;

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PMIModuleSubRutineStandard] [GetRenderGeometryObject()] : {err}");
                LoggerManager.Exception(err);

                return null;
            }
        }

        /// <summary>
        /// Path에서 교차하는 지점의 좌표를 얻는 함수
        /// </summary>
        /// <param name="geo"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        private List<Point> GetPointsOnFlattenedPath(Geometry geo, Transform t = null)
        {
            List<Point> pts = null;

            try
            {
                PathGeometry pg = geo.GetFlattenedPathGeometry();

                if (t != null)
                {
                    pg = Geometry.Combine(pg, pg, GeometryCombineMode.Intersect, t);
                }

                pts = GeometryHelper.GetPointsOnFlattenedPath(pg);
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PMIModuleSubRutineStandard] [GetPointsOnFlattenedPath()] : {err}");
                LoggerManager.Exception(err);

            }

            return pts;
        }

        /// <summary>
        /// 현재 화면에서 Pad의 위치를 가져오는 함수
        /// </summary>
        /// <param name="Center"></param>
        /// <param name="PadList"></param>
        /// <param name="RatioX"></param>
        /// <param name="RatioY"></param>
        /// <param name="SizeX"></param>
        /// <param name="SizeY"></param>
        private EventCodeEnum GetPadPosInScreen(WaferCoordinate Center, WaferCoordinate LLCorner, IList<PMIPadObject> PadList, double RatioX, double RatioY, double SizeX, double SizeY)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                // Group Center를 이용하여, 각 패드의  Bounding Box Data를 픽셀 단위로 계산한다.

                double HalfMahcineXAmount, HalfMahcineYAmount;

                //var pnpmodule = SubModule as PNPSetupBase;

                //CatCoordinates CurWaferCoord = pnpmodule.CurCam.CamSystemPos;
                //Point LLCorner = this.WaferAligner().PositionFromLeftBottomCorner(CurWaferCoord.GetX(), CurWaferCoord.GetY());

                HalfMahcineXAmount = (RatioX * (SizeX / 2));
                HalfMahcineYAmount = (RatioY * (SizeY / 2));

                WaferCoordinate WaferPadCen = new WaferCoordinate();

                foreach (var pad in PadList)
                {
                    WaferPadCen.SetCoordinates
                        (
                            (pad.PadCenter.X.Value + LLCorner.X.Value),
                            (pad.PadCenter.Y.Value + LLCorner.Y.Value),
                            0,
                            0
                        );

                    double[,] CornerPos = new double[2, 2];

                    CornerPos[0, 0] = Center.X.Value - HalfMahcineXAmount;
                    CornerPos[0, 1] = Center.Y.Value + HalfMahcineYAmount;

                    CornerPos[1, 0] = Center.X.Value + HalfMahcineXAmount;
                    CornerPos[1, 1] = Center.Y.Value - HalfMahcineYAmount;

                    double DiffPosX = WaferPadCen.X.Value - CornerPos[0, 0];
                    double DiffPosY = CornerPos[0, 1] - WaferPadCen.Y.Value;

                    Point PixelLeftTop = new Point();
                    Point PixelBottomRight = new Point();

                    PixelLeftTop.X = (int)((DiffPosX - (pad.PadInfos.SizeX.Value / 2)) / RatioX);
                    PixelLeftTop.Y = (int)((DiffPosY - (pad.PadInfos.SizeY.Value / 2)) / RatioY);

                    PixelBottomRight.X = (int)((DiffPosX + (pad.PadInfos.SizeX.Value / 2)) / RatioX);
                    PixelBottomRight.Y = (int)((DiffPosY + (pad.PadInfos.SizeY.Value / 2)) / RatioY);

                    if ((PixelLeftTop.X > 0) && (PixelLeftTop.X < SizeX) &&
                        (PixelLeftTop.Y > 0) && (PixelLeftTop.Y < SizeY) &&
                        (PixelBottomRight.Y > 0) && (PixelBottomRight.Y < SizeY) &&
                        (PixelBottomRight.Y > 0) && (PixelBottomRight.Y < SizeY)
                        )
                    {
                        pad.BoundingBox = new Rect(PixelLeftTop, PixelBottomRight);

                        retval = EventCodeEnum.NONE;
                    }
                    else
                    {
                        LoggerManager.Debug($"Pad position calculation failed.");

                        // TODO : TEST CODE
                        retval = EventCodeEnum.NONE;
                        break;
                    }
                }
            }
            catch (Exception err)
            {
                retval = EventCodeEnum.UNDEFINED;

                LoggerManager.Debug($"[PMIModuleSubRutineStandard] [GetPadPosInScreen()] : {err}");
                LoggerManager.Exception(err);
            }

            return retval;
        }

        ///// <summary>
        ///// 현재 화면에서 Pad의 위치를 가져오는 함수
        ///// </summary>
        ///// <param name="CurCam"></param>
        ///// <param name="PadList"></param>
        //private void GetPadPosInScreen(ICamera CurCam, List<PMIPadObject> PadList)
        //{
        //    try
        //    {
        //        ImageBuffer CurImg = null;

        //        CurCam.GetCurImage(out CurImg);

        //        double HalfMahcineXAmount, HalfMahcineYAmount;

        //        MachineCoordinate CurrentMachinePos = CurImg.MachineCoordinates;

        //        CurCam.GetCurImage(out CurImg);

        //        HalfMahcineXAmount = (CurImg.RatioX.Value * (CurImg.SizeX / 2));
        //        HalfMahcineYAmount = (CurImg.RatioY.Value * (CurImg.SizeY / 2));

        //        foreach (var pad in PadList)
        //        {

        //            WaferCoordinate WaferPadCen = new WaferCoordinate(
        //                (this.StageSupervisor().WaferObject.GetSubsInfo().WaferCenter.X.Value + this.StageSupervisor().WaferObject.GetPhysInfo().LowLeftCorner.X.Value) + (pad.PadCenter.X.Value),
        //                (this.StageSupervisor().WaferObject.GetSubsInfo().WaferCenter.Y.Value + this.StageSupervisor().WaferObject.GetPhysInfo().LowLeftCorner.Y.Value) + (pad.PadCenter.Y.Value));

        //            MachineCoordinate MachinePadCenPos = new MachineCoordinate();

        //            if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
        //            {
        //                MachinePadCenPos = this.CoordinateManager().WaferLowChuckConvert.ConvertBack(WaferPadCen);
        //            }
        //            else if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
        //            {
        //                MachinePadCenPos = this.CoordinateManager().WaferHighChuckConvert.ConvertBack(WaferPadCen);
        //            }

        //            double[,] MachineCornerPos = new double[2, 2];

        //            MachineCornerPos[0, 0] = CurrentMachinePos.X.Value + HalfMahcineXAmount;
        //            MachineCornerPos[0, 1] = CurrentMachinePos.Y.Value - HalfMahcineYAmount;

        //            MachineCornerPos[1, 0] = CurrentMachinePos.X.Value - HalfMahcineXAmount;
        //            MachineCornerPos[1, 1] = CurrentMachinePos.Y.Value + HalfMahcineYAmount;

        //            double DiffPosX = MachineCornerPos[0, 0] - MachinePadCenPos.X.Value;
        //            double DiffPosY = MachinePadCenPos.Y.Value - MachineCornerPos[0, 1];

        //            Point PixelLeftTop = new Point();
        //            Point PixelBottomRight = new Point();

        //            PixelLeftTop.X = (DiffPosX - (pad.PadSizeX.Value / 2)) / CurImg.RatioX.Value;
        //            PixelLeftTop.Y = (DiffPosY - (pad.PadSizeY.Value / 2)) / CurImg.RatioY.Value;

        //            PixelBottomRight.X = (DiffPosX + (pad.PadSizeX.Value / 2)) / CurImg.RatioX.Value;
        //            PixelBottomRight.Y = (DiffPosY + (pad.PadSizeX.Value / 2)) / CurImg.RatioX.Value;

        //            if ((PixelLeftTop.X > 0) && (PixelLeftTop.X < CurImg.SizeX) &&
        //                (PixelLeftTop.Y > 0) && (PixelLeftTop.Y < CurImg.SizeY) &&
        //                (PixelBottomRight.Y > 0) && (PixelBottomRight.Y < CurImg.SizeY) &&
        //                (PixelBottomRight.Y > 0) && (PixelBottomRight.Y < CurImg.SizeY)
        //                )
        //            {
        //                pad.PixelPosRect = new Rect(PixelLeftTop, PixelBottomRight);
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Debug($"[PMIModuleSubRutineStandard] [GetPadPosInScreen()] : {err}");
        //        LoggerManager.Exception(err);
        //    }
        //}

        #endregion

        #region ==> Overlay
        /// <summary>
        /// Overlay object들을 초기화 해주는 함수
        /// </summary>
        public void ClearRenderObjects()
        {
            try
            {
                RenderObject_Template.Clear();
                RenderObject_JudgingWindow.Clear();
                RenderObject_MarkMin.Clear();
                RenderObject_MarkMax.Clear();
                RenderObject_RegisteredPad.Clear();
                RenderObject_RegisteredPadIndex.Clear();
                RenderObject_DetectedMarks.Clear();

                //if (MakeTestProximitylineDatas == true)
                //{
                //    RenderObject_Proximityline.Clear();
                //}

                if (SubModule is IHasRenderLayer)
                {
                    var Layer = (SubModule as IHasRenderLayer).SharpDXLayer;

                    if (Layer is IPMIRenderLayer)
                    {
                        IPMIRenderLayer pmirenderlayer = (Layer as IPMIRenderLayer);

                        pmirenderlayer.UpdateTemplate(RenderObject_Template);
                        pmirenderlayer.UpdateJudgingWindow(RenderObject_JudgingWindow);
                        pmirenderlayer.UpdateMarkMinSize(RenderObject_MarkMin);
                        pmirenderlayer.UpdateMarkMaxSize(RenderObject_MarkMax);
                        pmirenderlayer.UpdateRegisterdPad(RenderObject_RegisteredPad);
                        pmirenderlayer.UpdateRegisterdPadIndex(RenderObject_RegisteredPadIndex);
                        pmirenderlayer.UpdateDetectedMarks(RenderObject_DetectedMarks);

                        //if (MakeTestProximitylineDatas == true)
                        //{
                        //    pmirenderlayer.UpdateProximityLine(RenderObject_Proximityline);
                        //}
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void UpdateCurrentPadIndex()
        {
            try
            {
                var module = SubModule as IUseLightJog;

                double dist;
                double MinDist = 0;
                int tmpIndex = 0;

                Point LLConer = new Point();
                Point padpos = new Point();

                if ((module.CurCam.CamSystemMI.XIndex >= 0) &&
                    (module.CurCam.CamSystemMI.YIndex >= 0)
                    )
                {
                    CatCoordinates CurrentPos = module.CurCam.CamSystemPos;

                    LLConer = this.WaferAligner().GetLeftCornerPosition(CurrentPos.GetX(), CurrentPos.GetY());

                    // 부호 주의, 현재 위치에서 가장 가까이 등록된 패드를 찾아 Index 업데이트

                    for (int i = 0; i < PadInfos.PMIPadInfos.Count; i++)
                    {
                        padpos.X = LLConer.X + PadInfos.PMIPadInfos[i].PadCenter.X.Value;
                        padpos.Y = LLConer.Y + PadInfos.PMIPadInfos[i].PadCenter.Y.Value;

                        dist = Math.Sqrt(Math.Pow((CurrentPos.X.Value - padpos.X), 2) + Math.Pow((CurrentPos.Y.Value - padpos.Y), 2));

                        if (i == 0)
                        {
                            MinDist = dist;
                            tmpIndex = i;
                        }
                        else
                        {
                            if (MinDist > dist)
                            {
                                MinDist = dist;
                                tmpIndex = i;
                            }
                        }
                    }

                    PMIDrawingGroup drawingGroup = (SubModule as IHasPMIDrawingGroup)?.DrawingGroup;

                    if (drawingGroup != null)
                    {
                        drawingGroup.SelectedPMIPadIndex = tmpIndex;
                    }

                    UpdateLabel();
                    //SelectedPMIPadIndex = tmpIndex;

                    //PadInfos.SelectedPMIPadIndex = tmpIndex;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }



        /// <summary>
        /// Renderlayer 객체를 업데이트 하는 함수
        /// </summary>
        /// <param name="Img"></param>
        /// <param name="param"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void UpdateRenderLayer()
        {
            try
            {
                //LoggerManager.Debug($"Called UpdateRenerLayer Func().");

                //if (SubModule is PNPSetupBase && SubModule is IHasPMIDrawingGroup)
                if (SubModule is IUseLightJog && SubModule is IHasPMIDrawingGroup)
                {
                    ClearRenderObjects();

                    //if (Layer is IPMIRenderLayer)
                    //{
                    //    (Layer as IPMIRenderLayer).ClearAllRenderContainer();
                    //}`

                    #region ==> Declaration
                    //var pnpmodule = SubModule as PNPSetupBase;

                    var Cam = (SubModule as IUseLightJog).CurCam;
                    /*
                    double scaleX = 0;
                    double scaleY = 0;
                    double scaleX2;
                    double scaleY2;
                    double offsetx;
                    double offsety;
                    double offsetx2;
                    double offsety2;
                    */
                    //double ratio_layerToRealX;
                    //double ratio_layerToRealY;

                    double GrabSizeX, GrabSizeY;
                    double RatioX, RatioY;
                    bool IntersectsWithFlag = false;
                    double M, N;
                    double M2, N2;

                    String infoText;
                    RenderText txt;

                    //List<RenderObject> objects = new List<RenderObject>();
                    //List<RenderObject> objects2 = new List<RenderObject>();

                    Geometry geo;
                    //Geometry transfomedGeo;

                    //ScaleTransform ScaleTransform = null;
                    //TranslateTransform TranslateTransForm = null;
                    //RotateTransform RotateTransform = null;


                    PMIDrawingGroup drawingGroup = (SubModule as IHasPMIDrawingGroup).DrawingGroup;

                    Point SourceLeftTop = new Point();
                    Point SourceBottomRight = new Point();
                    Point TargetLeftTop = new Point();
                    Point TargetBottomRight = new Point();
                    //Point LLCorner = new Point();

                    Rect Source = new Rect();
                    Rect Target = new Rect();

                    CatCoordinates CurWaferCoord;
                    //PadTemplate template;
                    WaferCoordinate PadCenWaferCoord;

                    #endregion

                    RenderLayer Layer = null;
                    IPMIRenderLayer PMILayer = null;

                    if (this.SubModule is IHasRenderLayer)
                    {
                        Layer = (this.SubModule as IHasRenderLayer).SharpDXLayer;
                    }

                    if ((Layer != null) && (Layer.LayerSize.Width != 0) && (Layer.LayerSize.Height != 0) && (drawingGroup != null) && (drawingGroup.IsOverlay))
                    {
                        PMILayer = Layer as IPMIRenderLayer;

                        GrabSizeX = Cam.GetGrabSizeWidth();
                        GrabSizeY = Cam.GetGrabSizeHeight();

                        RatioX = Cam.GetRatioX();
                        RatioY = Cam.GetRatioY();

                        RenderLayerRatio = new Point((Layer.LayerSize.Width / (GrabSizeX * RatioX)),
                                                    (Layer.LayerSize.Height / (GrabSizeY * RatioY)));

                        CurWaferCoord = Cam.CamSystemPos;

                        try
                        {
                            #region ==> Pad Templates
                            if (drawingGroup.Template == true)
                            {
                                if (drawingGroup.PadTemplate != null)
                                {
                                    geo = Geometry.Parse(drawingGroup.PadTemplate.PathData.Value);

                                    PMIGeometry templategeo = new PMIGeometry(new Point(drawingGroup.PadTemplate.SizeX.Value, drawingGroup.PadTemplate.SizeY.Value),
                                         drawingGroup.PadTemplate.Angle.Value,
                                         geo,
                                         RenderLayerRatio,
                                         new Point(Layer.LayerSize.Width, Layer.LayerSize.Height));

                                    templategeo.CalcScalePoint();
                                    templategeo.CalcOffsetPoint((Layer.LayerSize.Width / 2), (Layer.LayerSize.Height / 2));
                                    templategeo.CalcCenterPoint();
                                    templategeo.GetTransformedGeometry(templategeo.ScaleT, templategeo.OffsetT, templategeo.CenterT);

                                    var ro = GetRenderGeometryObject(templategeo.TransfomedGeo, "Green");

                                    if (ro != null)
                                    {
                                        RenderObject_Template.Add(ro);

                                        geo = Geometry.Parse("M 23,2 1,2 12,21");

                                        PMIGeometry arrowgeo = new PMIGeometry(new Point(15, 15),
                                            drawingGroup.PadTemplate.Angle.Value,
                                             geo,
                                             RenderLayerRatio,
                                             new Point(Layer.LayerSize.Width, Layer.LayerSize.Height));

                                        arrowgeo.CalcScalePoint();
                                        arrowgeo.CalcOffsetPoint((Layer.LayerSize.Width / 2), (Layer.LayerSize.Height / 2), 0, -RenderObject_Template[0].Height);
                                        arrowgeo.CalcCenterPoint();
                                        arrowgeo.GetTransformedGeometry(arrowgeo.ScaleT, arrowgeo.OffsetT, arrowgeo.CenterT);

                                        RenderObject_Template.Add(GetRenderGeometryObject(arrowgeo.TransfomedGeo, "Yellow", true));

                                        PMILayer.UpdateTemplate(RenderObject_Template);
                                    }
                                }
                                else
                                {

                                }
                            }
                            #endregion
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                            throw;
                        }

                        try
                        {
                            #region ==> Judging Window
                            if (drawingGroup.JudgingWindow == true)
                            {
                                if (drawingGroup.PadTemplate != null)
                                {
                                    geo = Geometry.Parse(drawingGroup.PadTemplate.PathData.Value);

                                    PMIGeometry judgingwindowgeo = new PMIGeometry(new Point((drawingGroup.PadTemplate.SizeX.Value - drawingGroup.PadTemplate.JudgingWindowSizeX.Value * 2), (drawingGroup.PadTemplate.SizeY.Value - drawingGroup.PadTemplate.JudgingWindowSizeY.Value * 2)),
                                        drawingGroup.PadTemplate.Angle.Value,
                                        geo,
                                         RenderLayerRatio,
                                         new Point(Layer.LayerSize.Width, Layer.LayerSize.Height));

                                    judgingwindowgeo.CalcScalePoint();
                                    judgingwindowgeo.CalcOffsetPoint((Layer.LayerSize.Width / 2), (Layer.LayerSize.Height / 2));
                                    judgingwindowgeo.CalcCenterPoint();
                                    judgingwindowgeo.GetTransformedGeometry(judgingwindowgeo.ScaleT, judgingwindowgeo.OffsetT, judgingwindowgeo.CenterT);

                                    RenderObject_JudgingWindow.Add(GetRenderGeometryObject(judgingwindowgeo.TransfomedGeo, "Red"));

                                    PMILayer.UpdateJudgingWindow(RenderObject_JudgingWindow);
                                }
                            }
                            #endregion
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                            throw;
                        }

                        try
                        {
                            #region ==> Mark Min
                            if (drawingGroup.MarkMin == true)
                            {
                                if (drawingGroup.PadTemplate != null)
                                {
                                    geo = Geometry.Parse("M0,0 L0,1 1,1 1,0 z");

                                    PMIGeometry markmingeo = new PMIGeometry(new Point(drawingGroup.PadTemplate.MarkWindowMinSizeX.Value, drawingGroup.PadTemplate.MarkWindowMinSizeY.Value),
                                        0,
                                        geo,
                                         RenderLayerRatio,
                                         new Point(Layer.LayerSize.Width, Layer.LayerSize.Height));

                                    markmingeo.CalcScalePoint();
                                    markmingeo.CalcOffsetPoint((Layer.LayerSize.Width / 2), (Layer.LayerSize.Height / 2));
                                    markmingeo.CalcCenterPoint();
                                    markmingeo.GetTransformedGeometry(markmingeo.ScaleT, translateT: markmingeo.OffsetT);

                                    RenderObject_MarkMin.Add(GetRenderGeometryObject(markmingeo.TransfomedGeo, "Violet"));

                                    PMILayer.UpdateMarkMinSize(RenderObject_MarkMin);
                                }
                            }
                            #endregion
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                            throw;
                        }



                        try
                        {
                            #region ==> Mark Max
                            if (drawingGroup.MarkMax == true)
                            {
                                if (drawingGroup.PadTemplate != null)
                                {
                                    geo = Geometry.Parse("M0,0 L0,1 1,1 1,0 z");

                                    PMIGeometry markmaxgeo = new PMIGeometry(new Point(drawingGroup.PadTemplate.MarkWindowMaxSizeX.Value, drawingGroup.PadTemplate.MarkWindowMaxSizeY.Value),
                                        0,
                                        geo,
                                         RenderLayerRatio,
                                         new Point(Layer.LayerSize.Width, Layer.LayerSize.Height));

                                    markmaxgeo.CalcScalePoint();
                                    markmaxgeo.CalcOffsetPoint((Layer.LayerSize.Width / 2), (Layer.LayerSize.Height / 2));
                                    markmaxgeo.CalcCenterPoint();
                                    markmaxgeo.GetTransformedGeometry(markmaxgeo.ScaleT, translateT: markmaxgeo.OffsetT);

                                    RenderObject_MarkMax.Add(GetRenderGeometryObject(markmaxgeo.TransfomedGeo, "Cyan"));

                                    PMILayer.UpdateMarkMaxSize(RenderObject_MarkMax);
                                }
                            }
                            #endregion
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                            throw;
                        }


                        try
                        {
                            #region ==> Registered Pad
                            if (drawingGroup.RegisterdPad == true)
                            {
                                foreach (var Die in this.StageSupervisor().WaferObject.GetSubsInfo().PMIDIEs.ToList())
                                {
                                    if ((Die.Pads != null) && (Die.Pads.PMIPadInfos.Count > 0))
                                    {
                                        foreach (var p in Die.Pads.PMIPadInfos.ToList().Select((value, i) => new { i, value }))
                                        {
                                            var pad = p.value;
                                            var index = p.i;

                                            //template = PMIInfo.GetPadTemplate(pad.PMIPadTemplateIndex.Value);

                                            //if (template != null)
                                            //{
                                            IntersectsWithFlag = false;

                                            geo = Geometry.Parse(pad.PadInfos.PathData.Value);

                                            //// Get Pad Center Wafer coordinate from LL
                                            //CurWaferCoord = Cam.CamSystemPos;

                                            WaferCoordinate DieLowLeftCornerPos = new WaferCoordinate();

                                            DieLowLeftCornerPos = this.WaferAligner().MachineIndexConvertToDieLeftCorner_NonCalcZ(
                                                Die.DieIndexM.XIndex, Die.DieIndexM.YIndex);

                                            //LLCorner = this.WaferAligner().PositionFromLeftBottomCorner(CurWaferCoord.GetX(), CurWaferCoord.GetY());

                                            // 
                                            double offsetX = 0;
                                            double offsetY = 0;
                                            double PadSizeX = 0;
                                            double PadSizeY = 0;

                                            if (pad.PMIResults.Count > 0)
                                            {
                                                PMIPadResult tmpresult = pad.PMIResults.Last();

                                                if (tmpresult.IsSuccessDetectedPad == true)
                                                {
                                                    offsetX = tmpresult.PadOffsetX * RatioX;
                                                    offsetY = tmpresult.PadOffsetY * RatioY;

                                                    PadSizeX = tmpresult.PadPosition.Width * RatioX;
                                                    PadSizeY = tmpresult.PadPosition.Height * RatioY;
                                                }
                                                else
                                                {
                                                    PadSizeX = pad.PadInfos.SizeX.Value;
                                                    PadSizeY = pad.PadInfos.SizeY.Value;
                                                }
                                            }
                                            else
                                            {
                                                PadSizeX = pad.PadInfos.SizeX.Value;
                                                PadSizeY = pad.PadInfos.SizeY.Value;
                                            }

                                            PadCenWaferCoord = new WaferCoordinate((pad.PadCenter.X.Value + offsetX + DieLowLeftCornerPos.X.Value),
                                                                                   (pad.PadCenter.Y.Value - offsetY + DieLowLeftCornerPos.Y.Value));

                                            // Display Layer
                                            SourceLeftTop = new Point(CurWaferCoord.X.Value - ((GrabSizeX / 2) * RatioX),
                                                                          CurWaferCoord.Y.Value + ((GrabSizeY / 2) * RatioY));
                                            SourceBottomRight = new Point(CurWaferCoord.X.Value + ((GrabSizeX / 2) * RatioX),
                                                                          CurWaferCoord.Y.Value - ((GrabSizeY / 2) * RatioY));
                                            // Pad

                                            TargetLeftTop = new Point(PadCenWaferCoord.GetX() - (PadSizeX / 2),
                                                                      PadCenWaferCoord.GetY() + (PadSizeY / 2));
                                            TargetBottomRight = new Point(PadCenWaferCoord.GetX() + (PadSizeX / 2),
                                                                          PadCenWaferCoord.GetY() - (PadSizeY / 2));


                                            //TargetLeftTop = new Point(pad.PadCenter.X.Value - (pad.PadSizeX.Value / 2), pad.PadCenter.Y.Value + (pad.PadSizeY.Value / 2));
                                            //TargetBottomRight = new Point(pad.PadCenter.X.Value + (pad.PadSizeX.Value / 2), pad.PadCenter.Y.Value - (pad.PadSizeY.Value / 2));

                                            Source = new Rect(SourceLeftTop, SourceBottomRight);
                                            Target = new Rect(TargetLeftTop, TargetBottomRight);

                                            IntersectsWithFlag = IsIntersectsWith(Source, Target);

                                            if (IntersectsWithFlag == true)
                                            {
                                                if (this.VisionManager().GetDispHorFlip() == DispFlipEnum.FLIP && this.VisionManager().GetDispVerFlip() == DispFlipEnum.FLIP)
                                                {
                                                    M = Layer.LayerSize.Width - ((Layer.LayerSize.Width / 2) - ((CurWaferCoord.X.Value - (PadCenWaferCoord.X.Value)) * RenderLayerRatio.X));
                                                    N = Layer.LayerSize.Height - ((Layer.LayerSize.Height / 2) + ((CurWaferCoord.Y.Value - (PadCenWaferCoord.Y.Value)) * RenderLayerRatio.Y));
                                                }
                                                else
                                                {
                                                    M = (Layer.LayerSize.Width / 2) - ((CurWaferCoord.X.Value - PadCenWaferCoord.X.Value) * RenderLayerRatio.X);
                                                    N = (Layer.LayerSize.Height / 2) + ((CurWaferCoord.Y.Value - PadCenWaferCoord.Y.Value) * RenderLayerRatio.Y);
                                                }



                                                PMIGeometry padgeo = new PMIGeometry(new Point(PadSizeX, PadSizeY),
                                                                                     pad.PadInfos.Angle.Value,
                                                                                     geo,
                                                                                     RenderLayerRatio,
                                                                                     new Point(Layer.LayerSize.Width, Layer.LayerSize.Height));

                                                padgeo.CalcScalePoint();
                                                padgeo.CalcOffsetPoint(M, N);
                                                padgeo.CalcCenterPoint();
                                                padgeo.GetTransformedGeometry(padgeo.ScaleT, padgeo.OffsetT, padgeo.CenterT);

                                                //scaleX = pad.PadSizeX.Value / geo.Bounds.Width * RenderLayerRatio.X;
                                                //scaleY = pad.PadSizeY.Value / geo.Bounds.Height * RenderLayerRatio.Y;

                                                //ScaleTransform = new ScaleTransform(scaleX, scaleY);

                                                //Point scale = GetScalePoint(pad.PadSizeX.Value, pad.PadSizeY.Value, geo);

                                                //ScaleTransform = CreateScaleTransform(scale);

                                                //M = (Layer.LayerSize.Width / 2) - ((CurWaferCoord.X.Value - PadCenWaferCoord.GetX()) * RenderLayerRatio.X);
                                                //N = (Layer.LayerSize.Height / 2) + ((CurWaferCoord.Y.Value - PadCenWaferCoord.GetY()) * RenderLayerRatio.Y);

                                                //offsetx = -(geo.Bounds.Left * scaleX) + M - (geo.Bounds.Width * scaleX / 2);
                                                //offsety = -(geo.Bounds.Top * scaleY) + N - (geo.Bounds.Height * scaleY / 2);

                                                //Point offset = GetOffsetPoint(CurWaferCoord, PadCenWaferCoord, geo, scale);

                                                //TranslateTransForm = CreateTranslateTransform(offset);

                                                //Point center = GetCenterPoint(scale, offset, geo);

                                                //RotateTransform = new RotateTransform(pad.PadAngle.Value, (offsetx + (geo.Bounds.Width * scaleX / 2)), (offsety + (geo.Bounds.Height * scaleY / 2)));
                                                //RotateTransform = CreateRotateTransform(pad.PadAngle.Value, center);

                                                //transfomedGeo = GetTransformedGeometry(geo, ScaleTransform, TranslateTransForm, RotateTransform);

                                                // 현재 선택된 패드 
                                                if (index == drawingGroup.SelectedPMIPadIndex)
                                                {
                                                    if (drawingGroup.SetupMode == PMI_SETUP_MODE.NONE)
                                                    {
                                                        if (drawingGroup.PadTableTemplate?.PadEnable[pad.Index.Value].Value == true)
                                                        {
                                                            if (pad.PMIResults.Count > 0)
                                                            {
                                                                bool isPass = false;

                                                                if (pad.PMIResults.Last().PadStatus != null)
                                                                {
                                                                    isPass = pad.PMIResults.Last().PadStatus.ToList().Any(x => x == PadStatusCodeEnum.PASS);

                                                                    if (isPass == true)
                                                                    {
                                                                        RenderObject_RegisteredPad.Add(GetRenderGeometryObject(padgeo.TransfomedGeo, "Green"));
                                                                    }
                                                                    else
                                                                    {
                                                                        RenderObject_RegisteredPad.Add(GetRenderGeometryObject(padgeo.TransfomedGeo, "Red"));
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    RenderObject_RegisteredPad.Add(GetRenderGeometryObject(padgeo.TransfomedGeo, "Blue"));
                                                                }
                                                            }
                                                            else
                                                            {
                                                                RenderObject_RegisteredPad.Add(GetRenderGeometryObject(padgeo.TransfomedGeo, "Blue"));
                                                            }
                                                        }
                                                        else
                                                        {
                                                            RenderObject_RegisteredPad.Add(GetRenderGeometryObject(padgeo.TransfomedGeo, "White"));
                                                        }
                                                    }
                                                    else
                                                    {
                                                        RenderObject_RegisteredPad.Add(GetRenderGeometryObject(padgeo.TransfomedGeo, "Yellow"));
                                                    }

                                                    //RenderObject_RegisteredPad.Add(GetRenderGeometryObject(padgeo.TransfomedGeo, "Yellow"));
                                                }
                                                else
                                                {
                                                    // Table Setup 중, 각 Table에서 사용하는 패드와 아닌 패드를 색상으로 구분하기 위함.
                                                    if (drawingGroup.SetupMode != PMI_SETUP_MODE.TABLE)
                                                    {
                                                        if (drawingGroup.SetupMode == PMI_SETUP_MODE.NONE)
                                                        {
                                                            if (drawingGroup.PadTableTemplate?.PadEnable[pad.Index.Value].Value == true)
                                                            {
                                                                if (pad.PMIResults.Count > 0)
                                                                {
                                                                    bool isPass = pad.PMIResults.Last().PadStatus.ToList().Any(x => x == PadStatusCodeEnum.PASS);

                                                                    if (isPass == true)
                                                                    {
                                                                        RenderObject_RegisteredPad.Add(GetRenderGeometryObject(padgeo.TransfomedGeo, "Green"));
                                                                    }
                                                                    else
                                                                    {
                                                                        RenderObject_RegisteredPad.Add(GetRenderGeometryObject(padgeo.TransfomedGeo, "Red"));
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    RenderObject_RegisteredPad.Add(GetRenderGeometryObject(padgeo.TransfomedGeo, "Blue"));
                                                                }
                                                            }
                                                            else
                                                            {
                                                                RenderObject_RegisteredPad.Add(GetRenderGeometryObject(padgeo.TransfomedGeo, "White"));
                                                            }
                                                        }
                                                        else
                                                        {
                                                            RenderObject_RegisteredPad.Add(GetRenderGeometryObject(padgeo.TransfomedGeo, "Blue"));
                                                        }
                                                    }
                                                    else
                                                    {
                                                        // 현재 선택된 TableTemplate 정보 필요
                                                        if (drawingGroup.PadTableTemplate?.PadEnable[pad.Index.Value].Value == true)
                                                        {
                                                            RenderObject_RegisteredPad.Add(GetRenderGeometryObject(padgeo.TransfomedGeo, "Green"));
                                                        }
                                                        else
                                                        {
                                                            RenderObject_RegisteredPad.Add(GetRenderGeometryObject(padgeo.TransfomedGeo, "White"));
                                                        }
                                                    }
                                                }

                                                infoText = $"{pad.Index.Value + 1}";

                                                if (this.VisionManager().GetDispHorFlip() == DispFlipEnum.FLIP && this.VisionManager().GetDispVerFlip() == DispFlipEnum.FLIP)
                                                {
                                                    M2 = Layer.LayerSize.Width - ((Layer.LayerSize.Width / 2) - ((CurWaferCoord.X.Value - TargetBottomRight.X + 5) * RenderLayerRatio.X));
                                                    N2 = Layer.LayerSize.Height - ((Layer.LayerSize.Height / 2) + ((CurWaferCoord.Y.Value - TargetLeftTop.Y) * RenderLayerRatio.Y));
                                                }
                                                else
                                                {
                                                    M2 = (Layer.LayerSize.Width / 2) - ((CurWaferCoord.X.Value - TargetBottomRight.X + 5) * RenderLayerRatio.X);
                                                    N2 = (Layer.LayerSize.Height / 2) + ((CurWaferCoord.Y.Value - TargetLeftTop.Y) * RenderLayerRatio.Y);
                                                }


                                                txt = new RenderText(infoText, (float)M2, (float)N2, 20, 20, "White", 12);

                                                RenderObject_RegisteredPadIndex.Add(txt);
                                            }
                                            //}
                                        }
                                    }

                                    PMILayer.UpdateRegisterdPad(RenderObject_RegisteredPad);
                                    PMILayer.UpdateRegisterdPadIndex(RenderObject_RegisteredPadIndex);
                                }
                            }
                            #endregion
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                            throw;
                        }

                        try
                        {
                            #region ==> Detected Marks
                            if (drawingGroup.DetectedMark == true)
                            {
                                if (this.PMIModule().DoPMIInfo.ProcessedPMIMIndex.Count > 0)
                                {
                                    foreach (var Die in this.StageSupervisor().WaferObject.GetSubsInfo().PMIDIEs.ToList())
                                    {
                                        // Die
                                        var list = this.PMIModule().DoPMIInfo.ProcessedPMIMIndex.ToList().LastOrDefault(x => (x.XIndex == Die.DieIndexM.XIndex) && (x.YIndex == Die.DieIndexM.YIndex));

                                        if ((Die.Pads != null) && (Die.Pads.PMIPadInfos.Count > 0) && list != null)
                                        {
                                            foreach (var p in Die.Pads.PMIPadInfos.ToList().Select((value, i) => new { i, value }))
                                            {
                                                var pad = p.value;
                                                var index = p.i;

                                                //CurWaferCoord = Cam.CamSystemPos;

                                                WaferCoordinate DieLowLeftCornerPos = new WaferCoordinate();
                                                WaferCoordinate PadLTWaferCoord;

                                                DieLowLeftCornerPos = this.WaferAligner().MachineIndexConvertToDieLeftCorner_NonCalcZ(
                                                    Die.DieIndexM.XIndex, Die.DieIndexM.YIndex);

                                                // (1) Pad 위치 확보

                                                if (pad.PMIResults.Count > 0)
                                                {
                                                    double offsetX = 0;
                                                    double offsetY = 0;
                                                    double PadSizeX = 0;
                                                    double PadSizeY = 0;

                                                    PMIPadResult tmpresult = pad.PMIResults.Last();

                                                    if (tmpresult != null)
                                                    {
                                                        if (tmpresult.IsSuccessDetectedPad == true)
                                                        {
                                                            offsetX = tmpresult.PadOffsetX * RatioX;
                                                            offsetY = tmpresult.PadOffsetY * RatioY;

                                                            PadSizeX = tmpresult.PadPosition.Width * RatioX;
                                                            PadSizeY = tmpresult.PadPosition.Height * RatioY;
                                                        }
                                                        else
                                                        {
                                                            PadSizeX = pad.PadInfos.SizeX.Value;
                                                            PadSizeY = pad.PadInfos.SizeY.Value;
                                                        }

                                                        PadCenWaferCoord = new WaferCoordinate((pad.PadCenter.X.Value + offsetX + DieLowLeftCornerPos.X.Value),
                                                                                           (pad.PadCenter.Y.Value - offsetY + DieLowLeftCornerPos.Y.Value));

                                                        //PadCenWaferCoord = new WaferCoordinate((pad.PadCenter.X.Value + DieLowLeftCornerPos.X.Value),
                                                        //                                           (pad.PadCenter.Y.Value + DieLowLeftCornerPos.Y.Value));

                                                        PadLTWaferCoord = new WaferCoordinate(
                                                            PadCenWaferCoord.GetX() - (PadSizeX / 2),
                                                            PadCenWaferCoord.GetY() + (PadSizeY / 2)
                                                            );

                                                        // Display Layer
                                                        SourceLeftTop = new Point(CurWaferCoord.X.Value - ((GrabSizeX / 2) * RatioX),
                                                                                      CurWaferCoord.Y.Value + ((GrabSizeY / 2) * RatioY));
                                                        SourceBottomRight = new Point(CurWaferCoord.X.Value + ((GrabSizeX / 2) * RatioX),
                                                                                      CurWaferCoord.Y.Value - ((GrabSizeY / 2) * RatioY));
                                                        // Pad
                                                        TargetLeftTop = new Point(PadCenWaferCoord.GetX() - (PadSizeX / 2),
                                                                                      PadCenWaferCoord.GetY() + (PadSizeY / 2));
                                                        TargetBottomRight = new Point(PadCenWaferCoord.GetX() + (PadSizeX / 2),
                                                                                      PadCenWaferCoord.GetY() - (PadSizeY / 2));

                                                        Source = new Rect(SourceLeftTop, SourceBottomRight);
                                                        Target = new Rect(TargetLeftTop, TargetBottomRight);

                                                        IntersectsWithFlag = IsIntersectsWith(Source, Target);

                                                        // 화면에 해당 패드가 걸쳐있는 경우
                                                        if (IntersectsWithFlag == true)
                                                        {
                                                            foreach (var mark in tmpresult.MarkResults)
                                                            {
                                                                // (2) Pad로부터 상대 거리

                                                                WaferCoordinate MarkCenWaferCoord = new WaferCoordinate
                                                                    (
                                                                        PadLTWaferCoord.GetX() + mark.MarkPosUmFromLT.Left + (mark.MarkPosUmFromLT.Width / 2),
                                                                        PadLTWaferCoord.GetY() - mark.MarkPosUmFromLT.Top + (mark.MarkPosUmFromLT.Height / 2)
                                                                    );


                                                                WaferCoordinate MarkLTWaferCoord = new WaferCoordinate
                                                                    (
                                                                        PadLTWaferCoord.GetX() + mark.MarkPosUmFromLT.Left,
                                                                        PadLTWaferCoord.GetY() - mark.MarkPosUmFromLT.Top
                                                                    );

                                                                // Mark
                                                                TargetLeftTop = new Point(MarkCenWaferCoord.GetX() - (mark.MarkPosUmFromLT.Width / 2),
                                                                                              MarkCenWaferCoord.GetY() + (mark.MarkPosUmFromLT.Height / 2));
                                                                TargetBottomRight = new Point(MarkCenWaferCoord.GetX() + (mark.MarkPosUmFromLT.Width / 2),
                                                                                              MarkCenWaferCoord.GetY() - (mark.MarkPosUmFromLT.Height / 2));

                                                                Source = new Rect(SourceLeftTop, SourceBottomRight);
                                                                Target = new Rect(TargetLeftTop, TargetBottomRight);

                                                                IntersectsWithFlag = IsIntersectsWith(Source, Target);

                                                                // 화면에 해당 마크가 걸쳐있는 경우
                                                                if (IntersectsWithFlag == true)
                                                                {
                                                                    float left = 0f;
                                                                    float top = 0f;

                                                                    if (this.VisionManager().GetDispHorFlip() == DispFlipEnum.FLIP && this.VisionManager().GetDispVerFlip() == DispFlipEnum.FLIP)
                                                                    {
                                                                        left = (float)(Layer.LayerSize.Width - ((Layer.LayerSize.Width / 2) - ((CurWaferCoord.X.Value - (MarkLTWaferCoord.GetX() + mark.MarkPosUmFromLT.Width)) * RenderLayerRatio.X)));
                                                                        top = (float)(Layer.LayerSize.Height - ((Layer.LayerSize.Height / 2) + ((CurWaferCoord.Y.Value - (MarkLTWaferCoord.GetY() - mark.MarkPosUmFromLT.Height)) * RenderLayerRatio.Y)));
                                                                    }
                                                                    else
                                                                    {
                                                                        left = (float)((Layer.LayerSize.Width / 2) - ((CurWaferCoord.X.Value - MarkLTWaferCoord.GetX()) * RenderLayerRatio.X));
                                                                        top = (float)((Layer.LayerSize.Height / 2) + ((CurWaferCoord.Y.Value - MarkLTWaferCoord.GetY()) * RenderLayerRatio.Y));
                                                                    }

                                                                    float width = (float)(mark.MarkPosUmFromLT.Width * RenderLayerRatio.X);
                                                                    float height = (float)(mark.MarkPosUmFromLT.Height * RenderLayerRatio.Y);

                                                                    // Pass는 다른 값과 같이 존재할 수 없음.
                                                                    bool isPass = mark.Status.ToList().Any(x => x == MarkStatusCodeEnum.PASS);

                                                                    RenderRectangle m = null;

                                                                    if ((isPass == true) && (mark.Status.Count == 1))
                                                                    {
                                                                        // PASS
                                                                        m = new RenderRectangle(left, top, width, height, "Transparent", 1, "Green");
                                                                    }
                                                                    else if (isPass == true)
                                                                    {
                                                                        // Unknown
                                                                        m = new RenderRectangle(left, top, width, height, "Transparent", 1, "White");
                                                                    }
                                                                    else
                                                                    {
                                                                        // FAIL
                                                                        m = new RenderRectangle(left, top, width, height, "Transparent", 1, "Red");
                                                                    }

                                                                    RenderObject_DetectedMarks.Add(m);

                                                                    //if (MakeTestProximitylineDatas == true)
                                                                    //{
                                                                    //    RenderLine RL_L = null;
                                                                    //    //RenderLine RL_R = null;
                                                                    //    //RenderLine RL_T = null;
                                                                    //    //RenderLine RL_B = null;

                                                                    //    Point Startpt = new Point();
                                                                    //    Point Endpt = new Point();

                                                                    //    // Left
                                                                    //    Startpt.X = (float)((Layer.LayerSize.Width / 2) - ((CurWaferCoord.X.Value - PadLTWaferCoord.GetX()) * RenderLayerRatio.X));
                                                                    //    Startpt.Y = top + height / 2.0;

                                                                    //    Endpt.X = left - 1;
                                                                    //    Endpt.Y = Startpt.Y;

                                                                    //    RL_L = new RenderLine(Startpt, Endpt, "OpaqueYellow", 1);

                                                                    //    RenderObject_Proximityline.Add(RL_L);

                                                                    //    //// Right

                                                                    //    //Startpt.X = left + width + 1;
                                                                    //    //Startpt.Y = top + height / 2.0;

                                                                    //    //Endpt.X = PadLTWaferCoord.GetX() + pad.PadInfos.SizeX.Value;
                                                                    //    //Endpt.Y = Startpt.Y;

                                                                    //    //RL_R = new RenderLine(Startpt, Endpt, "OpaqueYellow");

                                                                    //    //RenderObject_Proximityline.Add(RL_R);

                                                                    //    //// Top

                                                                    //    //Startpt.X = left + width / 2.0;
                                                                    //    //Startpt.Y = PadLTWaferCoord.GetY() + 1;

                                                                    //    //Endpt.X = Startpt.X;
                                                                    //    //Endpt.Y = top - 1;

                                                                    //    //RL_T = new RenderLine(Startpt, Endpt, "OpaqueYellow");

                                                                    //    //RenderObject_Proximityline.Add(RL_T);

                                                                    //    //// Bottom
                                                                    //    //Startpt.X = left + width / 2.0;
                                                                    //    //Startpt.Y = top + height + 1;

                                                                    //    //Endpt.X = Startpt.X;
                                                                    //    //Endpt.Y = PadLTWaferCoord.GetY() + pad.PadInfos.SizeY.Value - 1;

                                                                    //    //RL_B = new RenderLine(Startpt, Endpt, "OpaqueYellow");

                                                                    //    //RenderObject_Proximityline.Add(RL_B);
                                                                    //}
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                PMILayer.UpdateDetectedMarks(RenderObject_DetectedMarks);

                                //if (MakeTestProximitylineDatas == true)
                                //{
                                //    PMILayer.UpdateProximityLine(RenderObject_Proximityline);
                                //}
                            }
                            #endregion
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                            throw;
                        }
                    }
                }

                //PMIInfo.PMIInfoUpdatedToLoader();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PMIModuleSubRutineStandard] [UpdateRenderLayer()] : {err}");
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// 두개의 Rect 객체가 서로 겹치는지 확인하는 함수
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="Target"></param>
        /// <returns></returns>
        private bool IsIntersectsWith(Rect Source, Rect Target)
        {
            bool retval = false;

            try
            {
                retval = Source.IntersectsWith(Target);
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PMIModuleSubRutineStandard] [IsIntersectsWith()] : {err}");
                LoggerManager.Exception(err);
            }

            return retval;
        }

        /// <summary>
        /// Motion 이동했을때 Render object를 update 하는 함수
        /// </summary>
        /// <param name="Img"></param>
        public void MovedDelegate(ImageBuffer Img)
        {
            UpdateRenderLayer();
        }
        #endregion

        #region ==> Vision processing
        /// <summary>
        /// 등록된 Pad를 Grouping 하는 함수
        /// </summary>
        /// <param name="TableNumber"></param>
        /// <param name="PMIPadGroups"></param>
        /// <returns></returns>
        public EventCodeEnum PadGroupingMethod(int TableNumber)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (PMIDevParam.PadGroupingMethod.Value == GROUPING_METHOD.Single)
                {
                    retval = PadGroupingForSingle(TableNumber);
                }
                else if (PMIDevParam.PadGroupingMethod.Value == GROUPING_METHOD.Multi)
                {
                    retval = PadGroupingForMulti(TableNumber);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum PadGroupingForSingle(int TableNumber)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                #region //..local variable
                //ICamera CurCam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);

                ICamera CurCam = this.VisionManager().GetCam(PMIDevParam.ProcessingCamType.Value);

                List<PMIPadObject> PadList = PadInfos.PMIPadInfos.ToList();

                List<PMIPadObject> UsedPadList = new List<PMIPadObject>();

                Point LT;
                Point RB;

                Point SLT;
                Point SRB;

                double imgSizeX = 0.0;
                double imgSizeY = 0.0;

                int PadRegCnt;


                //int MAXARRAYCNT = 99;

                #endregion

                PMIGroupData group = new PMIGroupData();

                LoggerManager.Debug("[PMIModuleSubRutineStandard] PadGroupingForSingle(): PMI Pad Grouping Start...");

                #region ==> init params

                imgSizeX = (CurCam.GetGrabSizeWidth()) * (CurCam.GetRatioX());
                imgSizeY = (CurCam.GetGrabSizeHeight()) * (CurCam.GetRatioY());

                PadRegCnt = PadInfos.PMIPadInfos.Count();
                #endregion

                PMIInfo.PadTableTemplateInfo[TableNumber].Groups.Clear();

                if ((PadRegCnt < 1) || (PMIInfo.PadTableTemplateInfo[TableNumber].PadEnable.Count <= 0))
                {
                    LoggerManager.Error("[PMIModuleSubRutineStandard] PadGroupingForSingle(): PMI Pad Not Registered.");

                    retval = EventCodeEnum.PMI_GROUPING_ERROR;
                }
                else
                {
                    // Multi 
                    for (int padIndex = 0; padIndex < PadRegCnt; padIndex++)
                    {
                        // 해당 패드를 사용하는 경우.
                        if (PMIInfo.PadTableTemplateInfo[TableNumber].PadEnable[padIndex].Value == true)
                        {
                            PMIPadObject tmpPadObj = new PMIPadObject();

                            var pad = PadList[padIndex];

                            LT = new Point(pad.PadCenter.X.Value - (pad.PadInfos.SizeX.Value) / 2,
                                           pad.PadCenter.Y.Value - (pad.PadInfos.SizeY.Value) / 2);
                            RB = new Point(pad.PadCenter.X.Value + (pad.PadInfos.SizeX.Value) / 2,
                                           pad.PadCenter.Y.Value + (pad.PadInfos.SizeY.Value) / 2);

                            pad.BoundingBox = new Rect(LT, RB);

                            PadDataInGroup data = new PadDataInGroup();

                            data.PadWidth.Value = PadList[pad.Index.Value].PadSizeX.Value;
                            data.PadHeight.Value = PadList[pad.Index.Value].PadSizeY.Value;
                            data.PadRealIndex.Value = pad.Index.Value;

                            group = new PMIGroupData();
                            group.PadDataInGroup.Add(data);

                            foreach (var paddata in group.PadDataInGroup)
                            {
                                Point PadCenPos = new Point();

                                PadCenPos.X = PadList[paddata.PadRealIndex.Value].PadCenter.X.Value;
                                PadCenPos.Y = PadList[paddata.PadRealIndex.Value].PadCenter.Y.Value;

                                paddata.PadCenPosInGroup.Value = PadCenPos;
                            }

                            SLT = new Point(pad.PadCenter.X.Value - (imgSizeX) / 2, pad.PadCenter.Y.Value - (imgSizeY) / 2);
                            SRB = new Point(pad.PadCenter.X.Value + (imgSizeX) / 2, pad.PadCenter.Y.Value + (imgSizeY) / 2);

                            PMIInfo.PadTableTemplateInfo[TableNumber].Groups.Add(group);
                            PMIInfo.PadTableTemplateInfo[TableNumber].Groups[PMIInfo.PadTableTemplateInfo[TableNumber].Groups.Count - 1].GroupPosition.Value = new Rect(SLT, SRB);


                        }
                    }
                }

                LoggerManager.Debug("[PMIModuleSubRutineStandard] PadGroupingForSingle(): PMI Pad Grouping End.");

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PMIModuleSubRutineStandard] PadGroupingForSingle() : {err}");
                LoggerManager.Exception(err);

                retval = EventCodeEnum.PMI_GROUPING_ERROR;
            }

            return retval;
        }

        public EventCodeEnum PadGroupingForMulti(int TableNumber)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                #region //..local variable
                //ICamera CurCam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);

                ICamera CurCam = this.VisionManager().GetCam(PMIDevParam.ProcessingCamType.Value);

                List<PMIPadObject> PadList = PadInfos.PMIPadInfos.ToList();

                List<PMIPadObject> UsedPadList = new List<PMIPadObject>();

                Point LT;
                Point RB;

                double imgSizeX = 0.0;
                double imgSizeY = 0.0;
                double GroupingMargin = 0.0;

                double InitAreaLeft = 0;
                double InitAreaBottom = 0;
                double InitAreaRight = 0;
                double InitAreaTop = 0;

                double SearchAreaLeft;
                double SearchAreaBottom;
                double SearchAreaRight;
                double SearchAreaTop;

                double DUp = 0;
                double DDown = 0;
                double DLeft = 0;
                double DRight = 0;

                bool DFLAG = false;
                bool FirstInFlag = false;

                bool CutXFlag = false;
                bool CutYFlag = false;

                bool XLineFlag = false;

                int UsedPadCount = 0;
                int CheckedPadNum = 0;
                int GrouppedPadCnt = 0;

                int PadRegCnt;
                bool[] CheckedFlag;

                //int MAXARRAYCNT = 99;

                #endregion

                PMIGroupData group = new PMIGroupData();

                LoggerManager.Debug("PadGroupingMethod(): PMI Pad Grouping Start...");

                #region ==> init params

                if (PMIDevParam.SearchPadEnable.Value == true)
                {
                    var findmaxmargin = Math.Max(PMIDevParam.PadFindMarginX.Value, PMIDevParam.PadFindMarginY.Value);

                    if (findmaxmargin > PMIDevParam.PadGroupingMargin.Value)
                    {
                        GroupingMargin = findmaxmargin;
                    }
                    else
                    {
                        GroupingMargin = PMIDevParam.PadGroupingMargin.Value;
                    }
                }
                else
                {
                    GroupingMargin = PMIDevParam.PadGroupingMargin.Value;
                }

                imgSizeX = (CurCam.GetGrabSizeWidth()) * (CurCam.GetRatioX());
                imgSizeY = (CurCam.GetGrabSizeHeight()) * (CurCam.GetRatioY());

                PadRegCnt = PadInfos.PMIPadInfos.Count();
                #endregion

                PMIInfo.PadTableTemplateInfo[TableNumber].Groups.Clear();

                if ((PadRegCnt < 1) || (PMIInfo.PadTableTemplateInfo[TableNumber].PadEnable.Count <= 0))
                {
                    LoggerManager.Error($"PadGroupingMethod(): PMI Pad Not Registered.");

                    retval = EventCodeEnum.PMI_GROUPING_ERROR;
                }
                else
                {
                    CheckedFlag = new bool[PadRegCnt];

                    // Multi
                    #region ==> Init Search Area (have all pads..)
                    for (int padIndex = 0; padIndex < PadRegCnt; padIndex++)
                    {
                        if (PMIInfo.PadTableTemplateInfo[TableNumber].PadEnable[padIndex].Value == true)
                        {
                            PMIPadObject tmpPadObj = new PMIPadObject();

                            var pad = PadList[padIndex];

                            LT = new Point(pad.PadCenter.X.Value - (pad.PadInfos.SizeX.Value) / 2,
                                           pad.PadCenter.Y.Value - (pad.PadInfos.SizeY.Value) / 2);
                            RB = new Point(pad.PadCenter.X.Value + (pad.PadInfos.SizeX.Value) / 2,
                                           pad.PadCenter.Y.Value + (pad.PadInfos.SizeY.Value) / 2);

                            pad.BoundingBox = new Rect(LT, RB);

                            // 
                            tmpPadObj.BoundingBox = pad.BoundingBox;
                            tmpPadObj.Index.Value = padIndex;

                            UsedPadList.Add(tmpPadObj);

                            var UsedPadPos = UsedPadList[UsedPadList.Count - 1].BoundingBox;

                            if (UsedPadCount == 0)
                            {
                                InitAreaLeft = UsedPadPos.Left;
                                InitAreaRight = UsedPadPos.Right;
                                InitAreaBottom = UsedPadPos.Bottom;
                                InitAreaTop = UsedPadPos.Top;
                            }
                            else
                            {
                                if (UsedPadPos.Left < InitAreaLeft)
                                {
                                    InitAreaLeft = UsedPadPos.Left;
                                }
                                if (UsedPadPos.Right > InitAreaRight)
                                {
                                    InitAreaRight = UsedPadPos.Right;
                                }
                                if (UsedPadPos.Bottom > InitAreaBottom)
                                {
                                    InitAreaBottom = UsedPadPos.Bottom;
                                }
                                if (UsedPadPos.Top < InitAreaTop)
                                {
                                    InitAreaTop = UsedPadPos.Top;
                                }
                            }

                            UsedPadCount++;
                        }
                    }
                    #endregion

                    #region ==> Set Search Area Box Size (from left-top)
                    SearchAreaLeft = InitAreaLeft - GroupingMargin;
                    SearchAreaBottom = InitAreaBottom + GroupingMargin;

                    SearchAreaRight = SearchAreaLeft + imgSizeX;
                    SearchAreaTop = SearchAreaBottom - imgSizeY;
                    #endregion

                    while (CheckedPadNum < UsedPadCount)
                    {
                        DFLAG = false;
                        FirstInFlag = false;
                        GrouppedPadCnt = 0;

                        foreach (var pad in UsedPadList)
                        {
                            if (CheckedFlag[pad.Index.Value] == false)
                            {
                                if (((pad.BoundingBox.Left - GroupingMargin) >= SearchAreaLeft) &&
                                     ((pad.BoundingBox.Right + GroupingMargin) <= SearchAreaRight) &&
                                     ((pad.BoundingBox.Bottom + GroupingMargin) <= SearchAreaBottom) &&
                                     ((pad.BoundingBox.Top - GroupingMargin) >= SearchAreaTop))
                                {
                                    #region ==> CHECK : IN
                                    if (DFLAG == false)
                                    {
                                        DRight = pad.BoundingBox.Right;
                                        DDown = pad.BoundingBox.Top;

                                        DFLAG = true;
                                    }
                                    else
                                    {
                                        if (FirstInFlag == false)
                                        {
                                            DRight = pad.BoundingBox.Right;
                                            DDown = pad.BoundingBox.Top;
                                        }
                                        else
                                        {
                                            if (pad.BoundingBox.Right < DRight)
                                            {
                                                DRight = pad.BoundingBox.Right;
                                            }
                                            if (pad.BoundingBox.Top > DDown)
                                            {
                                                DDown = pad.BoundingBox.Top;
                                            }

                                            FirstInFlag = true;
                                        }
                                    }

                                    PadDataInGroup data = new PadDataInGroup();

                                    data.PadWidth.Value = PadList[pad.Index.Value].PadSizeX.Value;
                                    data.PadHeight.Value = PadList[pad.Index.Value].PadSizeY.Value;
                                    data.PadRealIndex.Value = pad.Index.Value;

                                    group.PadDataInGroup.Add(data);

                                    CheckedFlag[pad.Index.Value] = true;
                                    GrouppedPadCnt++;
                                    #endregion
                                }
                                else
                                {
                                    #region ==> CHECK : CUT
                                    if ((SearchAreaLeft <= pad.BoundingBox.Left - GroupingMargin) &&
                                        (SearchAreaRight > pad.BoundingBox.Left) &&
                                        (SearchAreaRight < pad.BoundingBox.Right + GroupingMargin))
                                    {
                                        #region ==> X CUT
                                        if (CutXFlag == false)
                                        {
                                            DLeft = pad.BoundingBox.Left;
                                            CutXFlag = true;
                                            DFLAG = true;
                                        }
                                        else
                                        {
                                            if (pad.BoundingBox.Left < DLeft)
                                            {
                                                DLeft = pad.BoundingBox.Left;
                                            }
                                        }
                                        #endregion
                                    }

                                    if ((SearchAreaBottom > pad.BoundingBox.Bottom + GroupingMargin) &&
                                        (SearchAreaTop < pad.BoundingBox.Bottom) &&
                                        (SearchAreaTop > pad.BoundingBox.Top - GroupingMargin))
                                    {
                                        #region //..Y CUT
                                        if (CutYFlag == false)
                                        {
                                            DUp = pad.BoundingBox.Bottom;
                                            CutYFlag = true;
                                            DFLAG = true;
                                        }
                                        else
                                        {
                                            if (pad.BoundingBox.Bottom > DUp)
                                            {
                                                DUp = pad.BoundingBox.Bottom;
                                            }
                                        }
                                        #endregion
                                    }
                                    #endregion
                                }
                            }
                        }

                        #region ==> Add pad to Group
                        if (GrouppedPadCnt > 0)
                        {
                            LT = new Point(SearchAreaLeft, SearchAreaTop);
                            RB = new Point(SearchAreaRight, SearchAreaBottom);

                            foreach (var data in group.PadDataInGroup)
                            {
                                Point PadCenPos = new Point();

                                PadCenPos.X = PadList[data.PadRealIndex.Value].PadCenter.X.Value;
                                PadCenPos.Y = PadList[data.PadRealIndex.Value].PadCenter.Y.Value;

                                data.PadCenPosInGroup.Value = PadCenPos;
                            }

                            //PMIPadGroups.Add(group);
                            //PMIPadGroups[PMIPadGroups.Count - 1].GroupPosition.Value = new Rect(LT, RB);

                            PMIInfo.PadTableTemplateInfo[TableNumber].Groups.Add(group);
                            PMIInfo.PadTableTemplateInfo[TableNumber].Groups[PMIInfo.PadTableTemplateInfo[TableNumber].Groups.Count - 1].GroupPosition.Value = new Rect(LT, RB);

                            //PMIPadGroups[PMIPadGroups.Count - 1].PadRegCount = GrouppedPadCnt;

                            CheckedPadNum += GrouppedPadCnt;

                            group = new PMIGroupData();

                            //GroupCnt++;
                        }
                        #endregion

                        #region ==> Get Next Area
                        if (SearchAreaRight < (InitAreaRight + GroupingMargin))
                        {
                            XLineFlag = true;
                        }
                        else
                        {
                            XLineFlag = false;
                        }

                        #region ==> Detected
                        if (DFLAG == true)
                        {
                            #region ==> Next X Area
                            if (XLineFlag == true)
                            {
                                if (CutXFlag == true)
                                {
                                    SearchAreaLeft = DLeft - GroupingMargin;
                                }
                                else
                                {
                                    SearchAreaLeft = SearchAreaRight - GroupingMargin;
                                }

                                SearchAreaRight = SearchAreaLeft + imgSizeX;

                                if (SearchAreaRight > (InitAreaRight + GroupingMargin))
                                {
                                    SearchAreaRight = InitAreaRight + GroupingMargin;
                                }
                            }
                            #endregion
                            #region ==> Next Y Area
                            else
                            {
                                if (SearchAreaTop < (InitAreaTop - GroupingMargin)) { }
                                else
                                {
                                    SearchAreaLeft = InitAreaLeft - GroupingMargin;
                                    SearchAreaRight = SearchAreaLeft + imgSizeX;

                                    if (CutYFlag == true)
                                    {
                                        SearchAreaBottom = DUp + GroupingMargin;
                                    }
                                    else
                                    {
                                        SearchAreaBottom = SearchAreaTop + GroupingMargin;
                                    }

                                    SearchAreaTop = SearchAreaBottom - imgSizeY;

                                    if (SearchAreaTop < (InitAreaTop - GroupingMargin))
                                    {
                                        SearchAreaTop = InitAreaTop - GroupingMargin;
                                    }

                                    CutYFlag = false;
                                }
                            }
                            #endregion
                        }
                        #endregion
                        #region ==> Not Detected
                        else
                        {
                            #region ==> Next X AREA
                            if (XLineFlag == true)
                            {
                                SearchAreaLeft = SearchAreaRight - GroupingMargin;
                                SearchAreaRight = SearchAreaLeft + imgSizeX;

                                if (SearchAreaRight > (InitAreaRight + GroupingMargin))
                                {
                                    SearchAreaRight = (InitAreaRight + GroupingMargin);
                                }
                            }
                            #endregion
                            #region ==> Next Y AREA
                            else
                            {
                                if (SearchAreaTop < (InitAreaTop - GroupingMargin)) { }
                                else
                                {
                                    SearchAreaLeft = InitAreaLeft - GroupingMargin;
                                    SearchAreaRight = SearchAreaLeft + imgSizeX;

                                    if (CutYFlag == true)
                                    {
                                        SearchAreaBottom = DUp + GroupingMargin;
                                    }
                                    else
                                    {
                                        SearchAreaBottom = SearchAreaTop + GroupingMargin;
                                    }

                                    SearchAreaTop = SearchAreaBottom - imgSizeY;

                                    if (SearchAreaTop < (InitAreaTop - GroupingMargin))
                                    {
                                        SearchAreaTop = InitAreaTop - GroupingMargin;
                                    }

                                    CutYFlag = false;
                                }
                            }
                            #endregion
                        }
                        #endregion
                        #endregion

                        CutXFlag = false;

                    } //..End While

                    //PadGroupTemplate GroupTemplate = new PadGroupTemplate();
                    //GroupTemplate.UsedTableIndex.Value = TableNumber;
                    //GroupTemplate.Groups = PMIPadGroups;

                    //var tmplist = PMIInfo.PadGroupInfo.Where(x => x.UsedTableIndex == GroupTemplate.UsedTableIndex).ToList();

                    //PMIInfo.PadTableTemplateInfo[TableNumber].Groups = PMIPadGroups;

                    //PMIInfo.PadGroupTemplateInfo.Clear();
                    //PMIInfo.PadGroupTemplateInfo.Add(GroupTemplate);

                    retval = EventCodeEnum.NONE;
                }

                LoggerManager.Debug($"[PMIModuleSubRutineStandard] [PadGroupingMethod()] : PMI Pad Grouping End.");
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PMIModuleSubRutineStandard] [PadGroupingMethod()] : {err}");
                LoggerManager.Exception(err);

                retval = EventCodeEnum.PMI_GROUPING_ERROR;
            }

            return retval;
        }


        /// <summary>
        /// Grouping 데이터의 순서를 생성하는 함수
        /// </summary>
        /// <param name="TableIndex"></param>
        /// <returns></returns>
        public EventCodeEnum MakeGroupSequence(int TableIndex)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                List<Rect> GroupPosList = new List<Rect>();

                //PadGroupTemplate GroupTemplate = null;

                if ((PMIInfo.PadTableTemplateInfo[TableIndex].Groups != null) &&
                     (PMIInfo.PadTableTemplateInfo[TableIndex].Groups.Count > 0))
                {
                    foreach (var group in PMIInfo.PadTableTemplateInfo[TableIndex].Groups)
                    {
                        GroupPosList.Add(group.GroupPosition.Value);
                    }

                    int[] seq = new int[GroupPosList.Count];

                    if (GroupPosList.Count >= 3)
                    {
                        GenericAlgorithm GA = new GenericAlgorithm();

                        GA.pGAToolStripMenuItem = false;

                        Stopwatch sw = new Stopwatch();

                        sw.Start();

                        seq = GA.Compute(GroupPosList);

                        sw.Stop();

                        Console.WriteLine("(1) Elapsed : {0}", sw.Elapsed);
                    }
                    else
                    {
                        for (int i = 0; i < GroupPosList.Count; i++)
                        {
                            seq[i] = i;
                        }
                    }

                    if (seq != null)
                    {
                        for (int i = 0; i < PMIInfo.PadTableTemplateInfo[TableIndex].Groups.Count; i++)
                        {
                            int GroupIndex = seq[i];

                            PMIInfo.PadTableTemplateInfo[TableIndex].Groups[GroupIndex].SeqNum.Value = i;
                        }
                    }
                }

                retval = EventCodeEnum.NONE;

                //if (PMIInfo.PadGroupTemplateInfo.ElementAtOrDefault(TableIndex) != null)
                //{
                //    GroupTemplate = PMIInfo.GetPadGroupTemplate(TableIndex);
                //}
                //else
                //{
                //    GroupTemplate = null;
                //    retval = EventCodeEnum.UNDEFINED;
                //}



                //GA.pGAToolStripMenuItem = true;
                //GA.threadParallelismToolStripMenuItem = true;
                //GA.taskParallelismToolStripMenuItem = false;
                //GA.parallelForToolStripMenuItem = false;

                //sw.Start();

                //seq = GA.Compute(GroupPosList);

                //sw.Stop();
                //Console.WriteLine("(2) Elapsed : {0}", sw.Elapsed);

                //GA.pGAToolStripMenuItem = true;
                //GA.threadParallelismToolStripMenuItem = false;
                //GA.taskParallelismToolStripMenuItem = true;
                //GA.parallelForToolStripMenuItem = false;

                //sw.Start();

                //seq = GA.Compute(GroupPosList);

                //sw.Stop();
                //Console.WriteLine("(3) Elapsed : {0}", sw.Elapsed);

                //GA.pGAToolStripMenuItem = true;
                //GA.threadParallelismToolStripMenuItem = false;
                //GA.taskParallelismToolStripMenuItem = false;
                //GA.parallelForToolStripMenuItem = true;

                //sw.Start();

                //seq = GA.Compute(GroupPosList);

                //sw.Stop();
                //Console.WriteLine("(4) Elapsed : {0}", sw.Elapsed);

                //if (this.StageSupervisor() is IHasDevParameterizable)
                //{
                //    (this.StageSupervisor() as IHasDevParameterizable).SaveDevParameter();
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PMIModuleSubRutineStandard] [MakeGroupSequence()] : {err}");
                LoggerManager.Exception(err);

                retval = EventCodeEnum.PMI_GROUPSEQUENCE_ERROR;
            }

            return retval;
        }


        private EventCodeEnum MakePadStatusUsingMarkResults(PMIPadResult padresult)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (padresult.PadStatus == null)
                {
                    padresult.PadStatus = new ObservableCollection<PadStatusCodeEnum>();
                }

                if (PMIDevParam.FailCodeInfo.FailCodeEnableNoProbeMark.Value == true)
                {
                    if (padresult.MarkResults.Count <= 0)
                    {
                        padresult.PadStatus.Add(PadStatusCodeEnum.NO_PROBE_MARK);
                    }
                }

                if (PMIDevParam.FailCodeInfo.FailCodeEnableMarkCntOver.Value == true)
                {
                    if (padresult.MarkResults.Count > PMIDevParam.MaximumMarkCnt.Value)
                    {
                        padresult.PadStatus.Add(PadStatusCodeEnum.TOO_MANY_PROBE_MARK);
                    }
                }

                if (padresult.MarkResults.Count > 0)
                {
                    // 모든 마크가 PASS가 아닌 경우

                    bool isFaild = false;

                    foreach (var mark in padresult.MarkResults)
                    {
                        isFaild = mark.Status.ToList().Any(x => x != MarkStatusCodeEnum.PASS);

                        if (isFaild == true)
                        {
                            break;
                        }
                    }

                    if (isFaild == true)
                    {
                        padresult.PadStatus.Add(PadStatusCodeEnum.FAIL);
                    }
                    else
                    {
                        padresult.PadStatus.Add(PadStatusCodeEnum.PASS);
                    }
                }
                else
                {
                    // 마크 데이터가 없지만, 없을 때, Fail을 내지 않는 조건으로, Pad는 Pass되어야 한다.
                    if (PMIDevParam.FailCodeInfo.FailCodeEnableNoProbeMark.Value == false)
                    {
                        padresult.PadStatus.Add(PadStatusCodeEnum.PASS);
                    }
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private EventCodeEnum InspectionDataAnalysis(List<PMIPadObject> pads)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                ICamera CurCam;

                CurCam = this.VisionManager().GetCam(PMIDevParam.ProcessingCamType.Value);

                double ratio_x = CurCam.GetRatioX();
                double ratio_y = CurCam.GetRatioY();

                foreach (var pad in pads)
                {
                    retval = MakeMarkDataForPad(pad, ratio_x, ratio_y);

                    PMIPadResult padresult = pad.PMIResults.Last();

                    retval = MakePadStatusUsingMarkResults(padresult);

                    // PASS Case : PASS 하나의 데이터만 갖고 있는 경우
                    if ((padresult.PadStatus.Count == 1 && padresult.PadStatus[0] == PadStatusCodeEnum.PASS) ||
                        (padresult.PadStatus.Count == 2 && padresult.PadStatus[0] == PadStatusCodeEnum.NEED_REFERENCE_IMAGE && padresult.PadStatus[1] == PadStatusCodeEnum.PASS)
                        )
                    {
                        this.PMIModule().DoPMIInfo.LastPMIDieResult.PassPadCount++;
                    }
                    else
                    {
                        this.PMIModule().DoPMIInfo.LastPMIDieResult.FailPadCount++;
                    }

                    //if (padresult.PadStatus.Count > 0)
                    //{
                    //    padresult.PadStatus.Add(PadStatusCodeEnum.FAIL);
                    //}
                    //else
                    //{
                    //    padresult.PadStatus.Add(PadStatusCodeEnum.PASS);
                    //}

                    //LoggerManager.PMILog($"Pad Size X : {pad.PadInfos.SizeX}, Pad Size Y : {pad.PadInfos.SizeY}, Detected Mark count : {padresult.MarkResults.Count }");

                    foreach (var m in padresult.MarkResults.Select((value, i) => new { i, value }))
                    {
                        var mark = m.value;
                        var index = m.i;

                        string MarkStatus = string.Empty;

                        bool isFaild = false;

                        isFaild = mark.Status.ToList().Any(x => x != MarkStatusCodeEnum.PASS);

                        if (isFaild == true)
                        {
                            MarkStatus = "FAIL";
                        }
                        else
                        {
                            MarkStatus = "PASS";
                        }

                        // TODO : Check Unit, Pixel to um
                        LoggerManager.PMILog($"Mark Index : {index + 1}, " +
                            $"Status : {MarkStatus}, " +
                            $"Scrub Size X : {mark.MarkPosUmFromLT.Width:F3}, " +
                            $"Scrub Size Y : {mark.MarkPosUmFromLT.Height:F3}, " +
                            $"Scrub Center X : {mark.ScrubCenter.X:F3}, " +
                            $"Scrub Center Y : {mark.ScrubCenter.Y:F3}, " +
                            $"Scrub Area : {mark.ScrubArea:F3}um^2, " +
                            $"Proximity : " +
                            $"(Top : {mark.MarkProximity.Top:F3}, " +
                            $"Bottom : {mark.MarkProximity.Bottom:F3}, " +
                            $"Left : {mark.MarkProximity.Left:F3}, " +
                            $"Right : {mark.MarkProximity.Right:F3})");

                        if (MarkStatus.ToUpper() == "FAIL")
                        {
                            StringBuilder sbMarkStatus = new StringBuilder();

                            int MarkStatusCount = mark.Status.Count;

                            for (int i = 0; i < MarkStatusCount; i++)
                            {
                                sbMarkStatus.Append($" | ");

                                sbMarkStatus.Append($"{mark.Status[i]}");
                            }

                            //LoggerManager.PMILog($"Fail reason ({MarkStatusCount}) :{sbMarkStatus}");
                            LoggerManager.PMILog($"Fail reason : {sbMarkStatus}");
                        }
                    }

                    double SizeXSum = padresult.MarkResults.Sum(x => x.MarkPosUmFromLT.Width);
                    double SizeYSum = padresult.MarkResults.Sum(x => x.MarkPosUmFromLT.Height);
                    double AreaSum = padresult.MarkResults.Sum(x => x.ScrubArea);

                    if (padresult.PadStatus.Count > 0)
                    {
                        //if (padresult.PadStatus.FirstOrDefault(x => x == PadStatusCodeEnum.NO_PROBE_MARK) != null)
                        if (padresult.PadStatus.Contains(PadStatusCodeEnum.NO_PROBE_MARK) == true)
                        {
                            LoggerManager.PMILog($"Mark Index : 0, " +
                            $"Status : FAIL, " +
                            $"Scrub Size X : , " +
                            $"Scrub Size Y : , " +
                            $"Scrub Center X : , " +
                            $"Scrub Center Y : , " +
                            $"Scrub Area : , " +
                            $"Proximity : , " +
                            $"(Top : , " +
                            $"Bottom : , " +
                            $"Left : , " +
                            $"Right : , )");
                        }
                    }

                    LoggerManager.PMILog($"Mark summation Info. : Size X = {SizeXSum:F3}, Size Y = {SizeYSum:F3}, Area = {AreaSum:F3}um^2");
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum MakeMarkDataForPad(PMIPadObject pad, double ratio_x, double ratio_y)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                // 동일한 패드
                var LastPMiResult = pad.PMIResults[pad.PMIResults.Count - 1];

                double RealPadWidth = 0;
                double RealPadHeight = 0;

                // Pad를 찾는 경우와 찾지 않는 경우, 크기가 달라지는 것을 고려해야 될 듯?
                if (LastPMiResult.IsSuccessDetectedPad == false)
                {
                    // 알고 있던 패드 사이즈를 그대로 사용하면 됨. (um)
                    RealPadWidth = pad.PadInfos.SizeX.Value;
                    RealPadHeight = pad.PadInfos.SizeY.Value;
                }
                else
                {
                    // 검출된 pixel을 이용, um으로 변환
                    RealPadWidth = LastPMiResult.PadPosition.Width * ratio_x;
                    RealPadHeight = LastPMiResult.PadPosition.Height * ratio_y;
                }

                LoggerManager.PMILog($"[Pad Index {pad.Index.Value + 1}], " +
                    $"Detected Mark Count : {LastPMiResult.MarkResults.Count}, " +
                    $"Pad Size X = {RealPadWidth:F3}, " +
                    $"Pad Size Y = {RealPadHeight:F3}");

                // PadTemplate 정보는 각 패드에 할당 된, PadTemplateIndex를 이용하여, 데이터를 사용해야 한다.

                PadTemplate tmpTemplate = this.GetParam_Wafer().PMIInfo.PadTemplateInfo[pad.PMIPadTemplateIndex.Value];
                var PMIDevParam = this.PMIModule().PMIModuleDevParam_IParam as PMIModuleDevParam;

                foreach (var m in LastPMiResult.MarkResults.Select((value, i) => new { i, value }))
                {
                    var mark = m.value;
                    var index = m.i;

                    // 패드 왼쪽 상단으로부터 마크까지 떨어진 거리 
                    //var Left = (mark.MarkPosPixel.Left - LastPMiResult.PadPosition.Left);
                    //var Top = (mark.MarkPosPixel.Top - LastPMiResult.PadPosition.Top);

                    //// FOR TEST
                    //// 에뮬모드에서는 VisionAlgorithmes의 FindMarks()에서 검출 된 패드 위치를 기준으로 계산이 되지 않기 때문에
                    //// 테스트 플래그가 켜져있는 경우, 해당 값을 반영해준 뒤, 계산하자.

                    //if (MakeTestSearchPadDatas == true)
                    //{
                    //    Point LT = new Point(mark.MarkPosPixel.Left + LastPMiResult.PadOffsetX,
                    //                         mark.MarkPosPixel.Top + LastPMiResult.PadOffsetY);

                    //    Point RB = new Point(mark.MarkPosPixel.Right + LastPMiResult.PadOffsetX,
                    //                         mark.MarkPosPixel.Bottom + LastPMiResult.PadOffsetY);

                    //    mark.MarkPosPixel = new Rect(LT, RB);
                    //}

                    var Left = mark.MarkPosPixel.Left;
                    var Top = mark.MarkPosPixel.Top;

                    var left_um = Left * ratio_x;
                    var top_um = Top * ratio_y;

                    var width_um = mark.MarkPosPixel.Width * ratio_x;
                    var height_um = mark.MarkPosPixel.Height * ratio_y;

                    mark.MarkPosUmFromLT = new Rect(left_um, top_um, width_um, height_um);

                    // 필요한 데이터

                    // 아래의 4개의 데이터 DistanceFromPadEdge
                    // 1. 패드의 위쪽 모서리로부터 마크의 Top까지 떨어진 거리        (pad.BoundingBox.Top - mark.MarkPosPixel.Top)
                    // 2. 패드의 오른쪽 모서리로부터 마크의 Right까지 떨어진 거리    (pad.BoundingBox.Right - mark.MarkPosPixel.Right)
                    // 3. 패드의 아래쪽 모서리로부터 마크의 Bottom까지 떨어진 거리   (pad.BoundingBox.Bottom - mark.MarkPosPixel.Bottom)
                    // 4. 패드의 왼쪽 모서리로부터 마크의 Left까지 떨어진 거리       (pad.BoundingBox.Left - mark.MarkPosPixel.Left)

                    // Pixel
                    double proximity_top = (mark.MarkPosPixel.Top);
                    double proximity_left = (mark.MarkPosPixel.Left);

                    //var proximity_right = (pad.PadInfos.SizeX.Value / ratio_x) - mark.MarkPosPixel.Right;
                    //var proximity_bottom = (pad.PadInfos.SizeY.Value / ratio_y) - mark.MarkPosPixel.Bottom;

                    double proximity_right = 0;
                    double proximity_bottom = 0;

                    if (LastPMiResult.IsSuccessDetectedPad == false)
                    {
                        //proximity_right = (pad.PadInfos.SizeX.Value / ratio_x) - mark.MarkPosPixel.Right;
                        //proximity_bottom = (pad.PadInfos.SizeY.Value / ratio_y) - mark.MarkPosPixel.Bottom;

                        proximity_right = (pad.PadInfos.SizeX.Value / ratio_x) - mark.MarkPosPixel.Right;
                        proximity_bottom = (pad.PadInfos.SizeY.Value / ratio_y) - mark.MarkPosPixel.Bottom;
                    }
                    else
                    {
                        proximity_right = LastPMiResult.PadPosition.Width - mark.MarkPosPixel.Right;
                        proximity_bottom = LastPMiResult.PadPosition.Height - mark.MarkPosPixel.Bottom;
                    }

                    // Um
                    mark.MarkProximity.Top = proximity_top * ratio_y;
                    mark.MarkProximity.Bottom = proximity_bottom * ratio_y;
                    mark.MarkProximity.Right = proximity_right * ratio_x;
                    mark.MarkProximity.Left = proximity_left * ratio_x;

                    // 5. 패드의 Center로부터 마크의 Center까지의 거리 X (Positive : Right, Negative : Left) 
                    // 6. 패드의 Center로부터 마크의 Center까지의 거리 Y (Positive : Top, Negative : Bottom)

                    // Um : Pad 왼쪽 상단을 기준으로 떨어진 거리
                    var markcenter_x = (mark.MarkPosUmFromLT.Left + mark.MarkPosUmFromLT.Right) / 2.0;
                    var markcenter_y = (mark.MarkPosUmFromLT.Top + mark.MarkPosUmFromLT.Bottom) / 2.0;

                    //double padcenter_x = 0;
                    //double padcenter_y = 0;

                    // Pad의 중심으로부터 떨어진 거리
                    mark.ScrubCenter = new Point(markcenter_x - (RealPadWidth / 2.0), (RealPadHeight / 2.0) - markcenter_y);
                    if (PMIDevParam.MarkAreaCalculateMode.Value == MARK_AREA_CALCULATE_MODE.Square)
                    {
                        mark.ScrubArea = width_um * height_um;
                    }
                    else
                    {
                        mark.ScrubArea = mark.ScrubAreaPx * ratio_x * ratio_y;
                    }
                    mark.ScrubAreaPercent = (mark.ScrubArea * 100) / (RealPadWidth * RealPadHeight);

                    //LoggerManager.Debug($"[Mark Index {index + 1}], Pixel L, T, W, H: {mark.MarkPosPixel.Left}, {mark.MarkPosPixel.Top}, {mark.MarkPosPixel.Width}, {mark.MarkPosPixel.Height}");
                    //LoggerManager.Debug($"[Mark Index {index + 1}], Pad Left Top to Mark(Pixel) L, T, W, H: {Left}, {Top}, {mark.MarkPosPixel.Width}, {mark.MarkPosPixel.Height}");
                    //LoggerManager.Debug($"[Mark Index {index + 1}], Pad Left Top to Mark(Um) L, T, W, H: {left_um}, {top_um}, {width_um}, {height_um}");

                    retval = MakeMarkStatus(tmpTemplate, mark);

                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"Make mark status is error. Mark Index {index + 1}");
                    }
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum MakeMarkStatus(PadTemplate PadInfo, PMIMarkResult Mark)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Mark.Status == null)
                {
                    Mark.Status = new ObservableCollection<MarkStatusCodeEnum>();
                }

                if (PMIDevParam.FailCodeInfo.FailCodeEnableBondEdge.Value == true)
                {
                    if (IsCloseTothePadEdge(Mark, PadInfo.JudgingWindowMode.Value, PadInfo.JudgingWindowSizeX.Value, PadInfo.JudgingWindowSizeY.Value) == true)
                    {
                        Mark.Status.Add(MarkStatusCodeEnum.TOO_CLOSE_TO_EDGE);

                        LoggerManager.Error($"[PMIModuleSubRutineStandard], MakeMarkStatus() TOO_CLOSE_TO_EDGE occurred. JudgingWindowMode = {PadInfo.JudgingWindowMode.Value}, JudgingWindowSizeX = {PadInfo.JudgingWindowSizeX.Value}, JudgingWindowSizeY = {PadInfo.JudgingWindowSizeY.Value}");
                    }
                }

                if (PMIDevParam.FailCodeInfo.FailCodeEnableBigMarkArea.Value == true &&
                    ((PMIDevParam.MarkCompareMode.Value == MARK_COMPARE_MODE.Area) || (PMIDevParam.MarkCompareMode.Value == MARK_COMPARE_MODE.AreaAndSize))
                    )
                {
                    if (IsBigAreaMark(Mark, PadInfo.MarkWindowMaxPercent.Value) == true)
                    {
                        Mark.Status.Add(MarkStatusCodeEnum.MARK_AREA_TOO_BIG);

                        LoggerManager.Error($"[PMIModuleSubRutineStandard], MakeMarkStatus() MARK_AREA_TOO_BIG occurred. MarkCompareMode = {PMIDevParam.MarkCompareMode.Value}, MarkWindowMaxPercent = {PadInfo.MarkWindowMaxPercent.Value}");
                    }
                }

                if (PMIDevParam.FailCodeInfo.FailCodeEnableBigMarkSize.Value == true &&
                    ((PMIDevParam.MarkCompareMode.Value == MARK_COMPARE_MODE.Size) || (PMIDevParam.MarkCompareMode.Value == MARK_COMPARE_MODE.AreaAndSize)))
                {
                    if (IsBigSizeMark(Mark, PadInfo.MarkWindowMaxSizeX.Value, PadInfo.MarkWindowMaxSizeY.Value) == true)
                    {
                        Mark.Status.Add(MarkStatusCodeEnum.MARK_SIZE_TOO_BIG);

                        LoggerManager.Error($"[PMIModuleSubRutineStandard], MakeMarkStatus() MARK_SIZE_TOO_BIG occurred. MarkCompareMode = {PMIDevParam.MarkCompareMode.Value}, MarkWindowMaxSizeX = {PadInfo.MarkWindowMaxSizeX.Value}, MarkWindowMaxSizeY = {PadInfo.MarkWindowMaxSizeY.Value}");
                    }
                }

                if (PMIDevParam.FailCodeInfo.FailCodeEnableSmallMarkArea.Value == true &&
                    ((PMIDevParam.MarkCompareMode.Value == MARK_COMPARE_MODE.Area) || (PMIDevParam.MarkCompareMode.Value == MARK_COMPARE_MODE.AreaAndSize))
                    )
                {
                    if (IsSmallAreaMark(Mark, PadInfo.MarkWindowMinPercent.Value) == true)
                    {
                        Mark.Status.Add(MarkStatusCodeEnum.MARK_AREA_TOO_SMALL);

                        LoggerManager.Error($"[PMIModuleSubRutineStandard], MakeMarkStatus() MARK_AREA_TOO_SMALL occurred. MarkCompareMode = {PMIDevParam.MarkCompareMode.Value}, MarkWindowMinPercent = {PadInfo.MarkWindowMinPercent.Value}");
                    }
                }

                if (PMIDevParam.FailCodeInfo.FailCodeEnableSmallMarkSize.Value == true &&
                    ((PMIDevParam.MarkCompareMode.Value == MARK_COMPARE_MODE.Size) || (PMIDevParam.MarkCompareMode.Value == MARK_COMPARE_MODE.AreaAndSize)))
                {
                    if (IsSmallSizeMark(Mark, PadInfo.MarkWindowMinSizeX.Value, PadInfo.MarkWindowMinSizeY.Value) == true)
                    {
                        Mark.Status.Add(MarkStatusCodeEnum.MARK_SIZE_TOO_SMALL);

                        LoggerManager.Error($"[PMIModuleSubRutineStandard], MakeMarkStatus() MARK_SIZE_TOO_SMALL occurred. MarkCompareMode = {PMIDevParam.MarkCompareMode.Value}, MarkWindowMaxSizeX = {PadInfo.MarkWindowMinSizeX.Value}, MarkWindowMinSizeY = {PadInfo.MarkWindowMinSizeY.Value}");
                    }
                }

                if (Mark.Status.Count == 0)
                {
                    Mark.Status.Add(MarkStatusCodeEnum.PASS);
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        #region 마크 면적 비교
        public bool IsBigAreaMark(PMIMarkResult markinfo, double MarkWindowMaxPercent)
        {
            bool retval = false;

            try
            {
                if (markinfo.ScrubAreaPercent > MarkWindowMaxPercent)
                {
                    retval = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool IsSmallAreaMark(PMIMarkResult markinfo, double MarkWindowMinPercent)
        {
            bool retval = false;

            try
            {
                if (markinfo.ScrubAreaPercent < MarkWindowMinPercent)
                {
                    retval = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        #endregion
        #region 마크의 가로 세로 길이 비교.
        public bool IsBigSizeMark(PMIMarkResult markinfo, double MarkWindowMaxSizeX, double MarkWindowMaxSizeY)
        {
            bool retval = false;

            try
            {
                if ((markinfo.Width > MarkWindowMaxSizeX) || (markinfo.Height > MarkWindowMaxSizeY))
                {
                    retval = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool IsSmallSizeMark(PMIMarkResult markinfo, double MarkWindowMinSizeX, double MarkWindowMinSizeY)
        {
            bool retval = false;

            try
            {
                if ((markinfo.Width < MarkWindowMinSizeX) || (markinfo.Height < MarkWindowMinSizeY))
                {
                    retval = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        #endregion


        public bool IsCloseTothePadEdge(PMIMarkResult markinfo, PAD_JUDGING_WINDOW_MODE mode, double JudgingX, double JudgingY)
        {
            bool retval = false;

            try
            {
                // TODO : Need ABS ?
                if ((markinfo.MarkProximity.Left < JudgingX) || (markinfo.MarkProximity.Right < JudgingX) ||
                    (markinfo.MarkProximity.Top < JudgingY) || (markinfo.MarkProximity.Bottom < JudgingY)
                    )
                {
                    retval = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private EventCodeEnum UpdatePMIPadsToDevice(List<PMIPadObject> padlist, MachineIndex MI)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                //foreach (var p in padlist)
                //{
                //    LoggerManager.Debug($"Index = {p.PadRealIndex}, Result HashCode = {p.PMIResults.GetHashCode()}");
                //}

                // TODO: Analysis Data & Set Data in WaferObject

                DeviceObject target = this.StageSupervisor().WaferObject.GetSubsInfo().DIEs[MI.XIndex, MI.YIndex] as DeviceObject;

                target.Pads.PMIPadInfos.Clear();

                foreach (var p in padlist)
                {
                    PMIPadObject pad = new PMIPadObject();

                    p.CopyTo(pad);
                    target.Pads.PMIPadInfos.Add(pad);
                }

                retval = EventCodeEnum.NONE;
                //return retval;

                //Parallel.For(0, padlist.Count, i =>
                //{
                //    PMIPadObject pad = new PMIPadObject();
                //    padlist[i].CopyTo(pad);
                //    target.Pads.PMIPadInfos.Add(pad);
                //});

                //retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private EventCodeEnum GetAutoLight(DoPMIData info, List<PMIPadObject> padlist)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                ICamera CurCam;

                ImageBuffer CurImg = null;

                ImageBuffer CropBuffer = null;
                ImageBuffer CropPadMaskingBuffer = null;

                ImageBuffer PadMaskingBuffer = new ImageBuffer();
                ImageBuffer EdgeBuffer = null;
                ImageBuffer ArithmethicBuffer = null;

                TransformGroup tg = new TransformGroup();
                ScaleTransform t1 = new ScaleTransform();
                TranslateTransform t2 = new TranslateTransform();

                Geometry transfomedGeo;
                PathGeometry pg;
                List<Point> pointsOnFlattenedPath;

                double CropOffsetX, CropOffsetY;
                double CropWidth, CropHeight;

                double EdgeSum = 0;
                double EdgeMaxSum = 0;

                int ErrorCase = 0;

                CurCam = this.VisionManager().GetCam(PMIDevParam.ProcessingCamType.Value);

                // Get Image
                CurCam.GetCurImage(out CurImg);

                CurImg.CopyTo(PadMaskingBuffer);
                PadMaskingBuffer.ClearBuffer();

                var VisionAlgorithmes = this.VisionManager().VisionProcessing.Algorithmes;

                foreach (var pad in padlist)
                {
                    CropOffsetX = pad.BoundingBox.Left;
                    CropOffsetY = pad.BoundingBox.Top;
                    CropWidth = pad.BoundingBox.Width;
                    CropHeight = pad.BoundingBox.Height;

                    CropBuffer = VisionAlgorithmes.CropImage(CurImg, (int)CropOffsetX, (int)CropOffsetY, (int)CropWidth, (int)CropHeight);

                    if (CropBuffer != null)
                    {
                        PadTemplate template = PMIInfo.GetPadTemplate(pad.PMIPadTemplateIndex.Value);

                        if (template != null)
                        {
                            Geometry geo = Geometry.Parse(template.PathData.Value);

                            double scaleX;
                            double scaleY;

                            double ratioX = (CropBuffer.SizeX / (CurCam.GetGrabSizeWidth() * CurCam.GetRatioX()));
                            double ratioY = (CropBuffer.SizeY / (CurCam.GetGrabSizeHeight() * CurCam.GetRatioY()));

                            tg.Children.Clear();

                            scaleX = (CropBuffer.SizeX - 1) / geo.Bounds.Width;
                            scaleY = (CropBuffer.SizeY - 1) / geo.Bounds.Height;

                            t1.ScaleX = scaleX;
                            t1.ScaleY = scaleY;

                            tg.Children.Add(t1);

                            transfomedGeo = geo.Clone();
                            transfomedGeo.Transform = tg;

                            pg = transfomedGeo.GetFlattenedPathGeometry();

                            pointsOnFlattenedPath = GeometryHelper.GetPointsOnFlattenedPath(pg);

                            CropPadMaskingBuffer = VisionAlgorithmes.MaskingBuffer(CropBuffer, pointsOnFlattenedPath);

                            // Get Copied Clip Image
                            PadMaskingBuffer = VisionAlgorithmes.CopyClipImage(PadMaskingBuffer,
                                                                        CropPadMaskingBuffer,
                                                                        (int)CropOffsetX,
                                                                        (int)CropOffsetY);
                            if (PadMaskingBuffer == null)
                            {
                                ErrorCase = 1;
                                retval = EventCodeEnum.PMI_AutoLight_Failure;
                                break;
                            }
                            else
                            {
                                retval = EventCodeEnum.NONE;
                            }
                        }
                        else
                        {
                            ErrorCase = 2;
                            retval = EventCodeEnum.PMI_AutoLight_Failure;
                            break;
                        }
                    }
                    else
                    {
                        ErrorCase = 3;
                        retval = EventCodeEnum.PMI_AutoLight_Failure;
                        break;
                    }
                }

                if (retval == EventCodeEnum.NONE)
                {
                    for (ushort i = PMIDevParam.AutoLightStart.Value; i < PMIDevParam.AutoLightEnd.Value; i += PMIDevParam.AutoLightInterval.Value)
                    {
                        CurCam.SetLight(EnumLightType.COAXIAL, i);

                        // Get Image
                        CurCam.GetCurImage(out CurImg);

                        this.VisionManager().SingleGrab(CurCam.GetChannelType(), this);

                        // MIL 8.0  : M_EDGE_DETECT =>
                        // MIL 10.0 : M_EDGE_DETECT_SOBEL_FAST

                        EdgeBuffer = VisionAlgorithmes.ConvolveImage(CurImg, ConvolveType.M_EDGE_DETECT_SOBEL_FAST);

                        ArithmethicBuffer = VisionAlgorithmes.Arithmethic(PadMaskingBuffer, EdgeBuffer, ARITHMETIC_OPERATION_TYPE.M_AND);

                        EdgeSum = VisionAlgorithmes.GetPixelSum(ArithmethicBuffer);

                        if ((EdgeMaxSum != 0) &&
                             (EdgeMaxSum > EdgeSum) &&
                             (EdgeMaxSum > (CurCam.GetGrabSizeWidth() * CurCam.GetGrabSizeHeight()))
                             )
                        {
                            break;
                        }

                        if (EdgeSum > EdgeMaxSum)
                        {
                            EdgeMaxSum = EdgeSum;
                            info.RememberAutoLightValue = i;
                        }

                    }
                }
                else
                {
                    LoggerManager.Error($"[PMIModuleSubRutineStandard], GetAutoLight() : retval = {retval}, Error case = {ErrorCase}");
                }

                //retval = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                retval = EventCodeEnum.PMI_AutoLight_Failure;
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private int GetTableIndexUsingCurrentWaferIndex(MachineIndex MI, out int WaferIndex)
        {
            int retval = -1;
            WaferIndex = -1;

            try
            {
                DoPMIData DoInfo = this.PMIModule().DoPMIInfo;

                int curWaferNum = this.PMIModule().LotOPModule().LotInfo.LoadedWaferCountUntilBeforeLotStart;
                //int curMapIndex = PMIInfo.WaferTemplateInfo[curWaferNum - 1].SelectedMapIndex.Value;

                // TODO : 포즈 상태로 호출됐을 때, 확인

                if (DoInfo.WorkMode == PMIWORKMODE.MANUAL)
                {
                    WaferIndex = DoInfo.WaferMapIndex;
                }
                else if (DoInfo.WorkMode == PMIWORKMODE.AUTO)
                {
                    WaferIndex = (DoInfo.WaferMapIndex - 1) % 25;
                }
                else
                {
                    //LoggerManager.Debug($"PMI Work Mode's Info is invalid.");
                }

                int WaferMapIndex = PMIInfo.WaferTemplateInfo[WaferIndex].SelectedMapIndex.Value;

                retval = PMIInfo.NormalPMIMapTemplateInfo[WaferMapIndex].GetTable((int)MI.XIndex, (int)MI.YIndex);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }


        private enum PadMarkProcessingMethod
        {
            /// <summary>
            /// 처리 안함 단순 이진화
            /// </summary>
            None,
            /// <summary>
            /// 처리함
            /// </summary>
            Processing,
            /// <summary>
            /// 레퍼런스 이미지 활용 하여 처리함
            /// </summary>
            PattenProcessing
        }

        //private void MakeTestData(List<PMIPadObject> PadTemplist, Point PadFindMargin, ICamera CurCam)
        //{
        //    try
        //    {
        //        // Test case
        //        if (MakeTestMarkDatas == true)
        //        {
        //            if (MakeTestMarkUsingRealDatas == false)
        //            {
        //                Random r = new Random();
        //                Random rm = new Random();

        //                int MarkMinCount = 1;
        //                int MarkMaxCount = 3;

        //                foreach (var pad in PadTemplist)
        //                {
        //                    var PadCurrentResult = pad.PMIResults[pad.PMIResults.Count - 1];

        //                    int MarkCount = rm.Next(MarkMinCount, MarkMaxCount + 1);

        //                    // PadOffset은 Pixel로 갖고 있다.

        //                    if (MakeTestSearchPadDatas == true)
        //                    {
        //                        PadCurrentResult.PadOffsetX = rm.Next((int)-PadFindMargin.X, (int)PadFindMargin.X);
        //                        PadCurrentResult.PadOffsetY = rm.Next((int)-PadFindMargin.Y, (int)PadFindMargin.Y);

        //                        PadCurrentResult.PadPosition = new Rect(pad.BoundingBox.X + PadCurrentResult.PadOffsetX, pad.BoundingBox.Y + PadCurrentResult.PadOffsetY, pad.BoundingBox.Width, pad.BoundingBox.Height);

        //                        PadCurrentResult.IsSuccessDetectedPad = true;
        //                    }
        //                    else
        //                    {
        //                        PadCurrentResult.PadOffsetX = 0;
        //                        PadCurrentResult.PadOffsetY = 0;
        //                    }

        //                    for (int j = 0; j < MarkCount; j++)
        //                    {
        //                        PMIMarkResult mark = new PMIMarkResult();

        //                        int RealPadBoundingBoxX = (int)(pad.BoundingBox.X + PadCurrentResult.PadOffsetX);
        //                        int RealPadBoundingBoxY = (int)(pad.BoundingBox.Y + PadCurrentResult.PadOffsetY);

        //                        double X_MIN = r.Next(RealPadBoundingBoxX, RealPadBoundingBoxX + (int)pad.BoundingBox.Width);
        //                        double Y_MIN = r.Next(RealPadBoundingBoxY, RealPadBoundingBoxY + (int)pad.BoundingBox.Height);

        //                        double X_MAX = r.Next((int)(X_MIN + 1), RealPadBoundingBoxX + (int)pad.BoundingBox.Width);
        //                        double Y_MAX = r.Next((int)(Y_MIN + 1), RealPadBoundingBoxY + (int)pad.BoundingBox.Height);

        //                        double SIZE_X = X_MAX - X_MIN + 1;
        //                        double SIZE_Y = Y_MAX - Y_MIN + 1;

        //                        Point LeftTop = new Point(X_MIN - RealPadBoundingBoxX,
        //                                                  Y_MIN - RealPadBoundingBoxY);

        //                        Point RightBottom = new Point(X_MAX - RealPadBoundingBoxX + 1,
        //                                                      Y_MAX - RealPadBoundingBoxY + 1);

        //                        mark.Width = SIZE_X;
        //                        mark.Height = SIZE_Y;
        //                        mark.MarkPosPixel = new Rect(LeftTop, RightBottom);

        //                        PadCurrentResult.MarkResults.Add(mark);
        //                    }

        //                }
        //            }
        //            else
        //            {
        //                // 실제 데이터를 이용하여, 마크 데이터를 생성.

        //                List<PMITestDataEachPad> PADTestDatas = new List<PMITestDataEachPad>();

        //                int UsedCaseNum = 4;

        //                // Case 1.

        //                //2020 - 05 - 27 21:25:06.502 | [DI] | SV = 90.00, PV = 90.10, DP = -3.12 | [PMIModuleSubRutineStandard], DoPMI() : Pad #1, X Offset = 3.55, Y Offset = 5.70, (Unit : um)
        //                //2020 - 05 - 27 21:25:06.509 | [DI] | SV = 90.00, PV = 90.10, DP = -3.12 | [Pad Index 1], Detected Mark Count: 2, Pad Size X = 54.600, Pad Size Y = 74.391
        //                //2020 - 05 - 27 21:25:06.530 | [DI] | SV = 90.00, PV = 90.10, DP = -3.12 | Mark Index: 1, Status: PASS, Scrub Size X: 12.558, Scrub Size Y: 17.376, Scrub Center X: -0.819, Scrub Center Y: -4.072, Scrub Area : 218.208um ^ 2, Proximity: (Top: 32.580, Bottom: 24.435, Left: 20.202, Right: 21.840)
        //                //2020 - 05 - 27 21:25:06.582 | [DI] | SV = 90.00, PV = 90.10, DP = -3.12 | Mark Index: 2, Status: PASS, Scrub Size X: 3.276, Scrub Size Y: 6.516, Scrub Center X: -2.730, Scrub Center Y: -10.588, Scrub Area : 21.346um ^ 2, Proximity: (Top: 44.526, Bottom: 23.349, Left: 22.932, Right: 28.392)
        //                //2020 - 05 - 27 21:25:06.601 | [DI] | SV = 90.00, PV = 90.10, DP = -3.12 | Mark summation Info. : Size X = 29.000, Size Y = 44.000, Area = 239.554um ^ 2

        //                // Case 2.
        //                //2020 - 05 - 27 21:25:20.701 | [DI] | SV = 90.00, PV = 90.10, DP = -3.10 | [PMIModuleSubRutineStandard], DoPMI() : Pad #1, X Offset = 8.19, Y Offset = 4.62, (Unit : um)
        //                //2020 - 05 - 27 21:25:20.709 | [DI] | SV = 90.00, PV = 90.10, DP = -3.10 | [Pad Index 1], Detected Mark Count: 2, Pad Size X = 55.146, Pad Size Y = 74.391
        //                //2020 - 05 - 27 21:25:20.725 | [DI] | SV = 90.00, PV = 90.10, DP = -3.10 | Mark Index: 1, Status: PASS, Scrub Size X: 12.558, Scrub Size Y: 16.290, Scrub Center X: 7.644, Scrub Center Y: -3.529, Scrub Area : 204.570um ^ 2, Proximity: (Top: 32.580, Bottom: 25.521, Left: 28.938, Right: 13.650)
        //                //2020 - 05 - 27 21:25:20.758 | [DI] | SV = 90.00, PV = 90.10, DP = -3.10 | Mark Index: 2, Status: PASS, Scrub Size X: 4.368, Scrub Size Y: 13.032, Scrub Center X: 7.371, Scrub Center Y: -8.416, Scrub Area : 56.924um ^ 2, Proximity: (Top: 39.096, Bottom: 22.263, Left: 32.760, Right: 18.018)

        //                if (UsedCaseNum == 1)
        //                {
        //                    PADTestDatas.Add(new PMITestDataEachPad(CurCam.GetRatioX(), CurCam.GetRatioY(), 54.6, 74.391, 3.55, 5.70));

        //                    PADTestDatas[PADTestDatas.Count - 1].Index = 0;

        //                    PADTestDatas[PADTestDatas.Count - 1].MarkDatas.Add(
        //                        new MarkTestData(CurCam.GetRatioX(), CurCam.GetRatioY(), 12.558, 17.376, -0.819, -4.072));

        //                    PADTestDatas[PADTestDatas.Count - 1].MarkDatas.Add(
        //                        new MarkTestData(CurCam.GetRatioX(), CurCam.GetRatioY(), 3.276, 6.516, -2.730, -10.588));
        //                }
        //                else if (UsedCaseNum == 2)
        //                {
        //                    PADTestDatas.Add(new PMITestDataEachPad(CurCam.GetRatioX(), CurCam.GetRatioY(), 55.146, 74.391, 8.19, 4.62));

        //                    PADTestDatas[PADTestDatas.Count - 1].Index = 0;

        //                    PADTestDatas[PADTestDatas.Count - 1].MarkDatas.Add(
        //                        new MarkTestData(CurCam.GetRatioX(), CurCam.GetRatioY(), 12.558, 16.290, 7.644, -3.529));

        //                    PADTestDatas[PADTestDatas.Count - 1].MarkDatas.Add(
        //                        new MarkTestData(CurCam.GetRatioX(), CurCam.GetRatioY(), 4.368, 13.032, 7.371, -8.416));
        //                }
        //                else if (UsedCaseNum == 3)
        //                {
        //                    PADTestDatas.Add(new PMITestDataEachPad(CurCam.GetRatioX(), CurCam.GetRatioY(), 52.520, 71.240, 0, -3.64));

        //                    PADTestDatas[PADTestDatas.Count - 1].Index = 0;

        //                    PADTestDatas[PADTestDatas.Count - 1].MarkDatas.Add(
        //                        new MarkTestData(CurCam.GetRatioX(), CurCam.GetRatioY(), 21.840, 24.960, -1.3, 4.420));
        //                }
        //                else if (UsedCaseNum == 4)
        //                {
        //                    PADTestDatas.Add(new PMITestDataEachPad(CurCam.GetRatioX(), CurCam.GetRatioY(), 52.520, 71.240, 0, -3.64));

        //                    PADTestDatas[PADTestDatas.Count - 1].Index = 0;

        //                    // EDGE
        //                    PADTestDatas[PADTestDatas.Count - 1].MarkDatas.Add(
        //                        new MarkTestData(CurCam.GetRatioX(), CurCam.GetRatioY(), 21.840, 24.960, -1.3, 4.420));

        //                    // MIN
        //                    PADTestDatas[PADTestDatas.Count - 1].MarkDatas.Add(
        //                        new MarkTestData(CurCam.GetRatioX(), CurCam.GetRatioY(), 1, 1, 3, -5));

        //                    // MAX
        //                    PADTestDatas[PADTestDatas.Count - 1].MarkDatas.Add(
        //                        new MarkTestData(CurCam.GetRatioX(), CurCam.GetRatioY(), 21.840, 24.960, -5, -5));
        //                }

        //                foreach (var pad in PadTemplist)
        //                {
        //                    PMITestDataEachPad padtestdata = PADTestDatas.FirstOrDefault(p => p.Index == pad.Index.Value);

        //                    var PadCurrentResult = pad.PMIResults[pad.PMIResults.Count - 1];

        //                    if (padtestdata != null)
        //                    {
        //                        // Pixel로 변환
        //                        PadCurrentResult.PadOffsetX = padtestdata.PadOffset_X_um / padtestdata.RatioX;
        //                        PadCurrentResult.PadOffsetY = padtestdata.PadOffset_Y_um / padtestdata.RatioY;

        //                        PadCurrentResult.PadPosition = new Rect((pad.BoundingBox.X + PadCurrentResult.PadOffsetX),
        //                                                                 (pad.BoundingBox.Y + PadCurrentResult.PadOffsetY),
        //                                                                 pad.BoundingBox.Width,
        //                                                                 pad.BoundingBox.Height
        //                                                                );

        //                        if (PadCurrentResult.PadOffsetX != 0 && PadCurrentResult.PadOffsetY != 0)
        //                        {
        //                            PadCurrentResult.IsSuccessDetectedPad = true;
        //                        }

        //                        foreach (var markinfo in padtestdata.MarkDatas)
        //                        {
        //                            PMIMarkResult mark = new PMIMarkResult();

        //                            int RealPadBoundingBoxX = (int)(pad.BoundingBox.X + PadCurrentResult.PadOffsetX);
        //                            int RealPadBoundingBoxY = (int)(pad.BoundingBox.Y + PadCurrentResult.PadOffsetY);

        //                            double mark_size_x = markinfo.SizeX_um / markinfo.RatioX;
        //                            double mark_size_y = markinfo.SizeY_um / markinfo.RatioY;

        //                            double RealPadWidth = padtestdata.PadSize_X_um;
        //                            double RealPadHeight = padtestdata.PadSize_Y_um;

        //                            mark.ScrubCenter = new Point(markinfo.Scrub_Center_X_um, markinfo.Scrub_Center_Y_um);

        //                            var markLeft = (RealPadBoundingBoxX + (pad.BoundingBox.Width / 2.0) - (-mark.ScrubCenter.X / markinfo.RatioX) - (mark_size_x / 2.0)) - RealPadBoundingBoxX;
        //                            var markTop = (RealPadBoundingBoxY + (pad.BoundingBox.Height / 2.0) - (mark.ScrubCenter.Y / markinfo.RatioY) - (mark_size_y / 2.0)) - RealPadBoundingBoxY;

        //                            var markcenter_x = mark.ScrubCenter.X + (RealPadWidth / 2.0);
        //                            var markcenter_y = (RealPadHeight / 2.0) - mark.ScrubCenter.Y;

        //                            //var markLeft = mark.ScrubCenter.X - (SIZE_X / 2.0) - RealPadWidth;
        //                            //var markTop = mark.ScrubCenter.Y - (SIZE_Y / 2.0) - RealPadHeight;

        //                            // Pixel
        //                            mark.Width = mark_size_x;
        //                            mark.Height = mark_size_y;

        //                            // 패드 왼쪽 상단으로부터 떨어진 Pixel
        //                            //mark.MarkPosPixel = new Rect(LeftTop, RightBottom);
        //                            mark.MarkPosPixel = new Rect(markLeft, markTop, mark.Width, mark.Height);

        //                            PadCurrentResult.MarkResults.Add(mark);
        //                        }

        //                        // TODO : 오버랩 마크 삭제 로직

        //                        // TODO : TEST CODE
        //                        if (MakeTestOverlap == false)
        //                        {
        //                            int TotalMarkCount = PadCurrentResult.MarkResults.Count;

        //                            if (TotalMarkCount >= 2)
        //                            {
        //                                List<PMIMarkResult> TestList = PadCurrentResult.MarkResults.OrderBy(o => o.MarkPosPixel.Width * o.MarkPosPixel.Height).ToList();

        //                                for (int s = 0; s < TotalMarkCount; s++)
        //                                {
        //                                    var SourceMark = TestList[s].MarkPosPixel;

        //                                    for (int t = s + 1; t < TotalMarkCount; t++)
        //                                    {
        //                                        var TargetMark = TestList[t].MarkPosPixel;

        //                                        // Is Intersect?
        //                                        if (SourceMark.IntersectsWith(TargetMark))
        //                                        {
        //                                            Rect intersectArea = Rect.Intersect(SourceMark, TargetMark);

        //                                            double OverlapWidthPercent = intersectArea.Width * 100 / SourceMark.Width;
        //                                            double OverlapHeightPercent = intersectArea.Height * 100 / SourceMark.Height;

        //                                            if (OverlapWidthPercent > PMIDevParam.MarkOverlapPercentOfToleranceX.Value ||
        //                                                OverlapHeightPercent > PMIDevParam.MarkOverlapPercentOfToleranceY.Value)
        //                                            {
        //                                                // EXCLUDE

        //                                                PadCurrentResult.MarkResults.Remove(TestList[s]);
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        /// <summary>
        /// 실재 PMI를 수행하는 함수, 하나의 Die를 진행
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum DoPMI(DoPMIData Info)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            ICamera CurCam = null;

            bool IsStop = false;

            try
            {
                LoggerManager.print_memoryInfo();
                // Mark Align
                if (Info.IsTurnOnMarkAlign == true)
                {
                    retval = this.MarkAligner().DoMarkAlign(true);

                    Info.IsTurnOnMarkAlign = false;

                    if (retval == EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[PMIModuleSubRutineStandard], DoPMI() : Mark Align Finished.");
                    }
                    else
                    {
                        LoggerManager.Debug($"[PMIModuleSubRutineStandard], DoPMI() : Mark Align Failed.");

                        return retval;
                    }
                }

                CurCam = this.VisionManager().GetCam(PMIDevParam.ProcessingCamType.Value);

                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.PMI_Start);
                Stopwatch stw = new Stopwatch();
                List<KeyValuePair<string, long>> timeStamp;

                timeStamp = new List<KeyValuePair<string, long>>();

                ImageBuffer CurImg = null;
                ImageBuffer CropImg = null;
                ImageBuffer ClippedImg = new ImageBuffer();
                ImageBuffer ClippedImg_Original = new ImageBuffer();
                ImageBuffer MaskingImg = null;
                ImageBuffer MaskingAndImg = null;
                ImageBuffer PadEdgeImage = null;
                ImageBuffer DrawingPadEdgeImg = null;
                ImageBuffer DrawingPadTemplateEdge = null;
                ImageBuffer MarkResultImg = null;

                List<SaveImageBufferData> SaveImageBufferData = null;
                string SaveImageBufferDataBaseTime = string.Empty;
                string SaveImageBufferDataBasePath = string.Empty;

                if (PMISysParam.DoPMIDebugImages)
                {
                    SaveImageBufferData = new List<SaveImageBufferData>();
                }

                long Threshold;

                string Save_Extension = ".bmp";

                double CropOffsetX, CropOffsetY;
                double CropWidth, CropHeight;

                List<PMIPadObject> PadTemplist = new List<PMIPadObject>();
                List<PMIPadObject> AllPadlist = new List<PMIPadObject>();
                Dictionary<string, ImageBuffer> PadReferenceImageDic = new Dictionary<string, ImageBuffer>(); // ISSD-5010
                int TableIndex = 0;
                int WaferMapIndex = -1;

                // Auto Light

                Point PadFindMargin = new Point();

                Point MarginAppliedLeftTop = new Point();
                Size MarginAppliedSize = new Size();

                TransformGroup tg = new TransformGroup();
                ScaleTransform t1 = new ScaleTransform();
                TranslateTransform t2 = new TranslateTransform();

                Geometry transfomedGeo;
                PathGeometry pg;
                List<Point> pointsOnFlattenedPath;

                List<Point> Chianlist = new List<Point>();
                Rect EdgeRect;

                PadFindMargin.X = PMIDevParam.PadFindMarginX.Value / CurCam.GetRatioX();
                PadFindMargin.Y = PMIDevParam.PadFindMarginY.Value / CurCam.GetRatioY();

                var Xlength = this.StageSupervisor().WaferObject.GetSubsInfo().DIEs.GetLength(0);
                var Ylength = this.StageSupervisor().WaferObject.GetSubsInfo().DIEs.GetLength(1);

                MachineIndex LastMI = Info.ProcessedPMIMIndex.Last();

                if ((LastMI.XIndex <= Xlength) && (LastMI.YIndex <= Ylength) &&
                    (LastMI.XIndex >= 0) && (LastMI.YIndex >= 0))
                {
                    TableIndex = GetTableIndexUsingCurrentWaferIndex(LastMI, out WaferMapIndex);
                }

                List<PMIGroupData> GroupInfo = null;

                if (TableIndex == 0)
                {
                    if (PMIInfo.PadTableTemplateInfo[TableIndex].GroupingDone.Value == false)
                    {
                        retval = this.PMIModule().PadGroupingMethod(TableIndex);

                        if (retval == EventCodeEnum.NONE)
                        {
                            retval = this.PMIModule().MakeGroupSequence(TableIndex);

                            if (retval == EventCodeEnum.NONE)
                            {
                                PMIInfo.PadTableTemplateInfo[TableIndex].GroupingDone.Value = true;
                            }
                            else
                            {
                                LoggerManager.Debug($"DoPMI(), MakeGroupSequence() failed.");
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"DoPMI(), PadGroupingMethod() failed.");
                        }
                    }

                    if (PMIInfo.PadTableTemplateInfo[TableIndex].GroupingDone.Value == true)
                    {
                        GroupInfo = PMIInfo.PadTableTemplateInfo[TableIndex].Groups.OrderBy(o => o.SeqNum.Value).ToList();
                    }
                }
                else
                {
                    if (PMIInfo.PadTableTemplateInfo[TableIndex - 1].GroupingDone.Value == false)
                    {
                        retval = this.PMIModule().PadGroupingMethod(TableIndex - 1);

                        if (retval == EventCodeEnum.NONE)
                        {
                            retval = this.PMIModule().MakeGroupSequence(TableIndex - 1);

                            if (retval == EventCodeEnum.NONE)
                            {
                                PMIInfo.PadTableTemplateInfo[TableIndex - 1].GroupingDone.Value = true;
                            }
                            else
                            {
                                LoggerManager.Debug($"DoPMI(), MakeGroupSequence() failed.");
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"DoPMI(), PadGroupingMethod() failed.");
                        }
                    }

                    if (PMIInfo.PadTableTemplateInfo[TableIndex - 1].GroupingDone.Value == true)
                    {
                        GroupInfo = PMIInfo.PadTableTemplateInfo[TableIndex - 1].Groups.OrderBy(o => o.SeqNum.Value).ToList();
                    }
                }

                if (GroupInfo == null || GroupInfo.Count == 0)
                {
                    LoggerManager.Debug($"DoPMI(), Grouping Data is abnormal.");

                    retval = EventCodeEnum.PMI_FAIL;

                    return retval;
                }

                var VisionAlgorithmes = this.VisionManager().VisionProcessing.Algorithmes;

                int GroupCount = 0;
                stw.Start();
                WaferCoordinate DieLowLeftCornerPos = new WaferCoordinate();

                DieLowLeftCornerPos = this.WaferAligner().MachineIndexConvertToDieLeftCorner(Info.ProcessedPMIMIndex.Last().XIndex, Info.ProcessedPMIMIndex.Last().YIndex);

                WaferCoordinate WaferGroupCen = new WaferCoordinate();

                string DutNo = string.Empty;

                if (Info.WorkMode == PMIWORKMODE.AUTO)
                {
                    IDeviceObject currentdevice = this.ProbingModule().ProbingProcessStatus.UnderDutDevs.FirstOrDefault
                        (x => x.DieIndexM.XIndex == Info.ProcessedPMIMIndex.Last().XIndex && x.DieIndexM.YIndex == Info.ProcessedPMIMIndex.Last().YIndex);

                    if (currentdevice != null)
                    {
                        DutNo = currentdevice.DutNumber.ToString();
                    }
                    else
                    {
                        DutNo = "Unknown";
                    }
                }
                else
                {
                    DutNo = "Unknown";
                }

                int TotalProcessingPadCount = 0;

                foreach (var g in GroupInfo)
                {
                    TotalProcessingPadCount += g.PadDataInGroup.Count;
                }

                LoggerManager.PMILog($"PMI START, LOT = {this.LotOPModule().LotInfo.LotName.Value}, Wafer = {this.GetParam_Wafer().GetSubsInfo().WaferID.Value}, " +
                    $"Mode = {Info.WorkMode}, GroupingMethod = {PMIDevParam.PadGroupingMethod.Value}, " +
                    $"Total Pad Count = {TotalProcessingPadCount}, Dut No. {DutNo}, " +
                    $"XY-Coordinate : X = {Info.LastPMIDieResult.UI.XIndex}, Y = {Info.LastPMIDieResult.UI.YIndex}, " +
                    $"Group Count = {GroupInfo.Count}");

                if (PMISysParam.DoPMIDebugImages)
                {
                    SaveImageBufferDataBaseTime = DateTime.Now.ToString("yyMMddHHmmss");
                }

                foreach (var g in GroupInfo.Select((value, i) => new { i, value }))
                {
                    LoggerManager.print_memoryInfo();

                    var group = g.value;
                    var index = g.i;

                    GroupCount++;

                    if (PMISysParam.DoPMIDebugImages)
                    {
                        SaveImageBufferDataBasePath = $@"C:\Logs\Test\PMI\{SaveImageBufferDataBaseTime}\Group{GroupCount}_";
                    }

                    LoggerManager.PMILog($"Group No. {index + 1}, Pad Count = {group.PadDataInGroup.Count}");

                    #region Move

                    WaferGroupCen.SetCoordinates
                        (
                            (group.GroupPosition.Value.Left + group.GroupPosition.Value.Right) / 2,
                            (group.GroupPosition.Value.Top + group.GroupPosition.Value.Bottom) / 2,
                            0,
                            0
                        );

                    WaferGroupCen.X.Value += DieLowLeftCornerPos.X.Value;
                    WaferGroupCen.Y.Value += DieLowLeftCornerPos.Y.Value;

                    double getzpos = this.WaferAligner().GetHeightValue(WaferGroupCen.X.Value, WaferGroupCen.Y.Value);
                    double zfocusingoffset = Info.RememberFocusingZValue * -1; //remeberfocusingzvalue machine coord 이기 때문에 부호 반대로 적용
                    double movezpos = getzpos + zfocusingoffset;
                    LoggerManager.Debug($"[PMIModule] DoPMI(): MoveZPos : {movezpos:0.00} =  GetHeightValue : {getzpos:0.00} + FocusingZOffset : {zfocusingoffset:0.00}");

                    if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                    {
                        this.StageSupervisor().StageModuleState.WaferLowViewMove(WaferGroupCen.X.Value, WaferGroupCen.Y.Value, movezpos, NotUseHeightProfile: true);

                    }
                    else if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                    {
                        if (index == 0)
                        {
                            this.StageSupervisor().StageModuleState.WaferHighViewIndexMove(Info.ProcessedPMIMIndex.Last().XIndex, Info.ProcessedPMIMIndex.Last().YIndex);

                            if (PMIDevParam.DelayInFirstGroup.Value != 0)
                            {
                                Thread.Sleep(PMIDevParam.DelayInFirstGroup.Value);
                                LoggerManager.Debug($"DelayInFirstGroup apply. time is {PMIDevParam.DelayInFirstGroup.Value} ms.");
                            }
                        }

                        this.StageSupervisor().StageModuleState.WaferHighViewMove(WaferGroupCen.X.Value, WaferGroupCen.Y.Value, movezpos, NotUseHeightProfile: true);

                        if (PMIDevParam.DelayAfterMoveToPad.Value != 0)
                        {
                            Thread.Sleep(PMIDevParam.DelayAfterMoveToPad.Value);
                            LoggerManager.Debug($"DelayAfterMoveToPad apply. time is {PMIDevParam.DelayAfterMoveToPad.Value} ms.");
                        }
                    }

                    #endregion

                    PadTemplist.Clear();

                    foreach (var item in group.PadDataInGroup)
                    {
                        PMIPadObject tmp = new PMIPadObject();

                        tmp.PMIResults.Add(new PMIPadResult());

                        PadInfos.PMIPadInfos[item.PadRealIndex.Value].CopyTo(tmp);

                        PadTemplist.Add(tmp);

                        PadTemplist[PadTemplist.Count - 1].MachineIndex = new MachineIndex(Info.ProcessedPMIMIndex.Last().XIndex, Info.ProcessedPMIMIndex.Last().YIndex);
                    }

                    LoggerManager.Debug($"Pad Count: {PadTemplist.Count}");

                    retval = GetPadPosInScreen(WaferGroupCen, DieLowLeftCornerPos, PadTemplist, CurCam.GetRatioX(), CurCam.GetRatioY(), CurCam.GetGrabSizeWidth(), CurCam.GetGrabSizeHeight());

                    if (retval == EventCodeEnum.NONE)
                    {
                        bool expansionROIflag = false;
                        // Check Focusing, Only first Group
                        FocusParameter focusparam = new NormalFocusParameter();
                        PMIDevParam.NormalFocusParam.CopyTo(focusparam);// NormalFocusParam 값을 바꾸지 않고 Focusing 동작에서 FocusParm을 넘겨 주기 위함.

                        double focusingroiX = focusparam.FocusingROI.Value.X;
                        double focusingroiY = focusparam.FocusingROI.Value.Y;
                        double focusingroiWidth = focusparam.FocusingROI.Value.Width;
                        double focusingroiHeight = focusparam.FocusingROI.Value.Height;

                        if (Info.IsTurnFocusing == true)
                        {

                            double curStartRefPos = 0;
                            // single mode에서만 focusing roi를 고려함. 
                            if (PMIDevParam.FocusingROIExpansionRatio.Value >= 1
                                && focusparam.FocusingROI.Value.Width == CurCam.GetGrabSizeWidth()
                                && focusparam.FocusingROI.Value.Height == CurCam.GetGrabSizeHeight())// 960, 960 이면 사용자가 설정하지 않았다고 판단. 
                            {
                                expansionROIflag = true;
                            }

                            if (expansionROIflag)
                            {
                                // PadTemplist.FirstOrDefault() 를 사용하는 것은 1개의 Die 내에서 패드의 높이는 거의 같다고 보기때문에 첫번째 Pad의 정보를 사용한다.
                                PMIPadObject firstPadObject = PadTemplist.FirstOrDefault();
                                focusingroiWidth = firstPadObject.BoundingBox.Width * PMIDevParam.FocusingROIExpansionRatio.Value;
                                focusingroiHeight = firstPadObject.BoundingBox.Height * PMIDevParam.FocusingROIExpansionRatio.Value;

                                double focusingCenterX = (firstPadObject.BoundingBox.Left + firstPadObject.BoundingBox.Right) / 2;
                                double focusingCenterY = (firstPadObject.BoundingBox.Top + firstPadObject.BoundingBox.Bottom) / 2;

                                focusingroiX = focusingCenterX - focusingroiWidth / 2;
                                focusingroiY = focusingCenterY - focusingroiHeight / 2;

                                if ((focusingCenterX - (focusingroiWidth / 2)) < 0 && (focusingCenterX + (focusingroiWidth / 2)) > CurCam.GetGrabSizeWidth())
                                {
                                    //의도한 영역이 아니라고 판단하여 ratio 를 사용하지 않음.
                                    LoggerManager.Debug($"[PMIModule] DoPMI(): calculated width of focusing roi is over grab size, " +
                                        $"expansion roi not use. expansionROIflag:{expansionROIflag}");
                                    expansionROIflag = false;
                                }

                                if ((focusingCenterY - (focusingroiHeight / 2)) < 0 && (focusingCenterY + (focusingroiHeight / 2)) > CurCam.GetGrabSizeHeight())
                                {
                                    //의도한 영역이 아니라고 판단하여 ratio 를 사용하지 않음.
                                    LoggerManager.Debug($"[PMIModule] DoPMI(): calculated height of focusing roi is over grab size, " +
                                        $"expansion roi not use. expansionROIflag:{expansionROIflag}");
                                    expansionROIflag = false;
                                }

                                if (expansionROIflag == false)
                                {
                                    focusingroiX = PMIDevParam.NormalFocusParam.FocusingROI.Value.X;
                                    focusingroiY = PMIDevParam.NormalFocusParam.FocusingROI.Value.Y;
                                    focusingroiWidth = PMIDevParam.NormalFocusParam.FocusingROI.Value.Width;
                                    focusingroiHeight = PMIDevParam.NormalFocusParam.FocusingROI.Value.Height;
                                }
                            }

                            focusparam.FocusingROI.Value = new System.Windows.Rect(x: (int)focusingroiX, y: (int)focusingroiY, width: (int)focusingroiWidth, height: (int)focusingroiHeight);

                            IFocusing focusmodule = this.PMIModule().GetFocuisngModule();

                            this.MotionManager().GetRefPos(focusparam.FocusingAxis.Value, ref curStartRefPos);

                            // 포커싱 수행 전, 디바이스에 설정되어 있는 조명값을 사용
                            if (PMIDevParam.LightValue != null && PMIDevParam.LightValue?.Value > 0)
                            {
                                CurCam.SetLight(EnumLightType.COAXIAL, PMIDevParam.LightValue.Value);
                            }
                            else
                            {
                                // 최대 조명의 40%로 설정하였음.
                                CurCam.SetLight(EnumLightType.COAXIAL, 100);
                            }

                            retval = focusmodule.Focusing_Retry(focusparam, false, false, false, this);

                            if (retval == EventCodeEnum.NONE)
                            {
                                // 마지막 포커싱 성공한 높이를 기억함.
                                // Dut Interval로 포커싱할 때, 포커싱하지 않은 위치의 다이를 이동 시 사용.
                                double curEndRefPos = 0;
                                this.MotionManager().GetRefPos(focusparam.FocusingAxis.Value, ref curEndRefPos);

                                Info.RememberFocusingZValue = curEndRefPos - curStartRefPos;
                            }
                            else
                            {
                                //1차 Focusing 실패
                                retval = EventCodeEnum.PMI_Focusing_Failure;

                                Info.RememberFocusingZValue = 0;

                                focusparam = PMIDevParam.NormalFocusParam;
                                focusingroiX = focusparam.FocusingROI.Value.X;
                                focusingroiY = focusparam.FocusingROI.Value.Y;
                                focusingroiWidth = focusparam.FocusingROI.Value.Width;
                                focusingroiHeight = focusparam.FocusingROI.Value.Height;

                                if (expansionROIflag)
                                {
                                    expansionROIflag = false;
                                    LoggerManager.Debug($"[PMIModule] DoPMI(): FocusingROIExpansionRatio Failed. " +
                                        $"Do Focusing Again use PMIDevParam.NormalFocusParam ROI:{focusparam.FocusingROI.Value}");

                                    this.MotionManager().GetRefPos(focusparam.FocusingAxis.Value, ref curStartRefPos);

                                    retval = focusmodule.Focusing_Retry(PMIDevParam.NormalFocusParam, false, false, false, this);

                                    if (retval == EventCodeEnum.NONE)
                                    {
                                        // 마지막 포커싱 성공한 높이를 기억함.
                                        // Dut Interval로 포커싱할 때, 포커싱하지 않은 위치의 다이를 이동 시 사용.
                                        double curEndRefPos = 0;
                                        this.MotionManager().GetRefPos(focusparam.FocusingAxis.Value, ref curEndRefPos);

                                        Info.RememberFocusingZValue = curEndRefPos - curStartRefPos;
                                    }
                                    else
                                    {
                                        // Retry 실패

                                        retval = EventCodeEnum.PMI_Focusing_Failure;

                                        Info.RememberFocusingZValue = 0;
                                    }
                                }
                            }

                            if (PMIDevParam.FocusingEachGroupEnable.Value != true)
                            {
                                Info.IsTurnFocusing = false;
                            }
                        }

                        if (retval == EventCodeEnum.NONE)
                        {
                            // Set Light, Only first Group
                            if (index == 0)
                            {
                                if ((PMIDevParam.AutoLightEnable.Value == true) && (Info.IsTurnAutoLight == true))
                                {
                                    Info.RememberAutoLightValue = 0;

                                    retval = GetAutoLight(Info, PadTemplist);

                                    if (retval == EventCodeEnum.NONE)
                                    {
                                        CurCam.SetLight(EnumLightType.COAXIAL, Info.RememberAutoLightValue);
                                    }
                                    else
                                    {
                                        CurCam.SetLight(EnumLightType.COAXIAL, PMIDevParam.LightValue.Value);
                                    }

                                    Info.IsTurnAutoLight = false;
                                }
                                else
                                {
                                    CurCam.SetLight(EnumLightType.COAXIAL, PMIDevParam.LightValue.Value);
                                }
                            }

                            #region Grab

                            this.VisionManager().SingleGrab(CurCam.GetChannelType(), this);

                            #endregion

                            // Get Image
                            CurCam.GetCurImage(out CurImg);

                            if (PMISysParam.DoPMIDebugImages)
                            {
                                SaveImageBufferData.Add(MakeSaveImageBufferData(CurImg, $@"{SaveImageBufferDataBasePath}01_Origianl{Save_Extension}", IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE));
                            }

                            CurImg.CopyTo(ClippedImg);
                            CurImg.CopyTo(ClippedImg_Original);

                            if (ClippedImg != null)
                            {
                                ClippedImg.ClearBuffer();
                            }

                            if (ClippedImg_Original != null)
                            {
                                ClippedImg_Original.ClearBuffer();
                            }

                            double curEndRefPos = 0;
                            this.MotionManager().GetRefPos(focusparam.FocusingAxis.Value, ref curEndRefPos);
                            var gethash = DateTime.Now.ToString().GetHashCode();
                            LoggerManager.Debug($"Origin Image Hash [{gethash}] pos:{curEndRefPos} ratio:{PMIDevParam.FocusingROIExpansionRatio.Value:0.0} expansion:{expansionROIflag}");

                            if (PMIDevParam.SkipProcessing.Value == false)
                            {
                                if (PMIDevParam.SearchPadEnable.Value == true)
                                {
                                    LoggerManager.Debug($"Start PMI Processing");
                                    // Get Clipped Image
                                    //영역별 이진화를 해서 그랩 이미지에 다시 합치기
                                    //이진화 영역이 겹치면?
                                    foreach (var pad in PadTemplist)
                                    {
                                        MarginAppliedLeftTop.X = pad.BoundingBox.Left - PadFindMargin.X;
                                        MarginAppliedLeftTop.Y = pad.BoundingBox.Top - PadFindMargin.Y;

                                        MarginAppliedSize.Width = pad.BoundingBox.Width + (PadFindMargin.X * 2);
                                        MarginAppliedSize.Height = pad.BoundingBox.Height + (PadFindMargin.Y * 2);

                                        // Get Crop Image
                                        CropImg = VisionAlgorithmes.CropImage(CurImg,
                                            (int)MarginAppliedLeftTop.X,
                                            (int)MarginAppliedLeftTop.Y,
                                            (int)MarginAppliedSize.Width,
                                            (int)MarginAppliedSize.Height);

                                        // Get Threshold value
                                        Threshold = VisionAlgorithmes.GetThreshold_ForPadDetection(CropImg);

                                        // Get Binarization Image
                                        ImageBuffer BinaryImg = VisionAlgorithmes.Binarization(CropImg, Threshold);

                                        // Get Copied Clip Image
                                        ClippedImg = VisionAlgorithmes.CopyClipImage(ClippedImg, BinaryImg, (int)MarginAppliedLeftTop.X, (int)MarginAppliedLeftTop.Y);
                                        ClippedImg_Original = VisionAlgorithmes.CopyClipImage(ClippedImg, CropImg, (int)MarginAppliedLeftTop.X, (int)MarginAppliedLeftTop.Y);
                                    }

                                    if (PMISysParam.DoPMIDebugImages)
                                    {
                                        SaveImageBufferData.Add(MakeSaveImageBufferData(ClippedImg, $@"{SaveImageBufferDataBasePath}02_PadClippedImg{Save_Extension}", IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE));
                                        SaveImageBufferData.Add(MakeSaveImageBufferData(ClippedImg_Original, $@"{SaveImageBufferDataBasePath}03_PadClipped_Original{Save_Extension}", IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE));
                                    }

                                    // Get Blob Object for Pad Detection
                                    retval = VisionAlgorithmes.FindPMIPads(PadTemplist, PadFindMargin, ClippedImg, ClippedImg_Original, null, null, true, false);

                                    if (retval == EventCodeEnum.NONE)
                                    {
                                        LoggerManager.Debug($"[PMIModuleSubRutineStandard], DoPMI() : All Pad Found.");
                                    }
                                    else if (retval == EventCodeEnum.PMI_NOT_FOUND_PAD)
                                    {
                                        LoggerManager.Debug($"[PMIModuleSubRutineStandard], DoPMI() : PMI_NOT_FOUND_PAD");
                                    }
                                    else
                                    {
                                        LoggerManager.Debug($"[PMIModuleSubRutineStandard], DoPMI() : FindPMIPads error.");
                                    }
                                }
                                else
                                {
                                    //PadCurrentResult.PadPosition = new Rect(LeftTop, RightBottom);
                                }

                                CurImg.CopyTo(ClippedImg);

                                if (ClippedImg != null)
                                {
                                    ClippedImg.ClearBuffer();
                                }

                                foreach (var p in PadTemplist.Select((value, i) => new { i, value }))
                                {
                                    var pad = p.value;
                                    var padindex = p.i;

                                    int padRealIndex = pad.Index.Value + 1;

                                    CropOffsetX = pad.BoundingBox.Left;
                                    CropOffsetY = pad.BoundingBox.Top;
                                    CropWidth = pad.BoundingBox.Width;
                                    CropHeight = pad.BoundingBox.Height;

                                    var PadCurrentResult = pad.PMIResults[pad.PMIResults.Count - 1];

                                    if (PMIDevParam.SearchPadEnable.Value == true)
                                    {
                                        if (PadCurrentResult.IsSuccessDetectedPad == true)
                                        {
                                            CropOffsetX = PadCurrentResult.PadPosition.Left;
                                            CropOffsetY = PadCurrentResult.PadPosition.Top;
                                            CropWidth = PadCurrentResult.PadPosition.Width;
                                            CropHeight = PadCurrentResult.PadPosition.Height;
                                        }
                                        else
                                        {
                                            // TODO : SKIP
                                        }
                                    }

                                    //int marginPx = 5;

                                    //CropImg = VisionAlgorithmes.CropImage(CurImg,
                                    //    (int)CropOffsetX - marginPx,
                                    //    (int)CropOffsetY - marginPx,
                                    //    (int)CropWidth + (marginPx * 2),
                                    //    (int)CropHeight + (marginPx * 2)
                                    //    );

                                    CropImg = VisionAlgorithmes.CropImage(CurImg,
                                        (int)CropOffsetX,
                                        (int)CropOffsetY,
                                        (int)CropWidth,
                                        (int)CropHeight);

                                    if (PMISysParam.DoPMIDebugImages)
                                    {
                                        SaveImageBufferData.Add(MakeSaveImageBufferData(CropImg, $@"{SaveImageBufferDataBasePath}04_CropImg_Pad#{padRealIndex}{Save_Extension}", IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE));
                                    }

                                    if (CropImg != null)
                                    {
                                        //Step1. Save Cropimage (Pattern_Pad)
                                        this.VisionManager().SaveImageBuffer(CropImg, this.FileManager().FileManagerParam.DeviceParamRootDirectory + "\\" + this.FileManager().FileManagerParam.DeviceName + "\\" + "PMIModule\\Mark_Image" + Save_Extension, IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE);

                                        // TODO : Template Data를 쓰는 것이 맞는가?, Pad마다 따로 저장된 데이터를 쓰는 것이 맞는가?
                                        // Tempalte Data는 등록과정에서 변경될 수 있기 때문에, 각 패드에 저장된 데이터를 활용하도록 하자.
                                        PadTemplate template = PMIInfo.GetPadTemplate(pad.PMIPadTemplateIndex.Value);

                                        if (template != null)
                                        {
                                            Geometry geo = Geometry.Parse(template.PathData.Value);

                                            double scaleX;
                                            double scaleY;

                                            double ratioX = CropImg.SizeX / (CurCam.GetGrabSizeWidth() * CurCam.GetRatioX());
                                            double ratioY = CropImg.SizeY / (CurCam.GetGrabSizeHeight() * CurCam.GetRatioY());

                                            double offsetx;
                                            double offsety;

                                            tg.Children.Clear();

                                            scaleX = (CropImg.SizeX - 1) / geo.Bounds.Width;
                                            scaleY = (CropImg.SizeY - 1) / geo.Bounds.Height;

                                            t1.ScaleX = scaleX;
                                            t1.ScaleY = scaleY;

                                            tg.Children.Add(t1);

                                            transfomedGeo = geo.Clone();
                                            transfomedGeo.Transform = tg;

                                            pg = transfomedGeo.GetFlattenedPathGeometry();

                                            pointsOnFlattenedPath = GeometryHelper.GetPointsOnFlattenedPath(pg);

                                            MaskingImg = VisionAlgorithmes.MaskingBuffer(CropImg, pointsOnFlattenedPath);

                                            if (PMISysParam.DoPMIDebugImages)
                                            {
                                                SaveImageBufferData.Add(MakeSaveImageBufferData(MaskingImg, $@"{SaveImageBufferDataBasePath}05_MaskingImg_Pad#{padRealIndex}{Save_Extension}", IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE));
                                            }

                                            if (pad.PadColor.Value == EnumPadColorType.UNDEFINED)
                                            {
                                                pad.PadColor.Value = EnumPadColorType.WHITE;
                                            }

                                            //TODO If Pattern Pad begin
                                            #region Pad Mark 검출을 위한 전처리 #T8R5X ISSD-5010
                                            //Step 1. Option Check.  
                                            ReservedRecipe padVPMethod = ReservedRecipe.PMI_Normal;
                                            List<(byte[], int, int, int)> inputImages = new List<(byte[], int, int, int)>() { (CropImg.Buffer, CropImg.SizeX, CropImg.SizeY, 1) };
                                            string extraParam = "";

                                            if ((PMIDevParam.PadMarkDifferentiation.Value == PAD_MARK_DIFFERENTIATION_MODE.None) ||
                                                 (PMIDevParam.PadMarkDifferentiation.Value == PAD_MARK_DIFFERENTIATION_MODE.SimpleThreshold))
                                            {//단순 이진화. 이진화 수치값 전달
                                                // default option
                                                Threshold = (long)VisionAlgorithmes.GetThreshold_ProbeMark(CropImg, MaskingImg, (int)pad.PadColor.Value);
                                                extraParam = $"{{ \"ThresholdFilter\" : {{\"Params\":[  {{\"Key\":\"Threshold\", \"Value\":\"{Threshold}\"}}]}}}}";
                                            }
                                            else if (PMIDevParam.PadMarkDifferentiation.Value == PAD_MARK_DIFFERENTIATION_MODE.PadNoiseAwareThreshold)
                                            {//레퍼런스 없이 이미지 단독으로 노이즈 제거.
                                                padVPMethod = ReservedRecipe.PMI_Processing;
                                            }
                                            else if (PMIDevParam.PadMarkDifferentiation.Value == PAD_MARK_DIFFERENTIATION_MODE.ReferenceImage)
                                            {//레퍼런스 적용해서 이미지 연산
                                                string refernceFilePath =
                                                PMIModuleKeyWordInfo.PadTemplateReferencePath(this.FileManager(), pad.PMIPadTemplateIndex.Value);

                                                if (!PadReferenceImageDic.ContainsKey(refernceFilePath))
                                                {
                                                    if (System.IO.File.Exists(refernceFilePath))
                                                    {
                                                        PadReferenceImageDic[refernceFilePath] = this.VisionManager().LoadImageFile(refernceFilePath);
                                                    }
                                                    else
                                                    {
                                                        PadReferenceImageDic[refernceFilePath] = null;
                                                        LoggerManager.Error($"[PMIModuleSubRutineStandard], DoPMI() : Ref. image not found {refernceFilePath}");// 매 패드 검사할때마다 기록하지 않음. PMI 1회수행때만 기록
                                                    }
                                                }
                                                ImageBuffer maskingAndImg = PadReferenceImageDic[refernceFilePath];
                                                if (maskingAndImg != null)
                                                {
                                                    inputImages.Add((maskingAndImg.Buffer, maskingAndImg.SizeX, maskingAndImg.SizeY, 2)); //레퍼런스 이미지 ID=2
                                                    padVPMethod = ReservedRecipe.PMI_PatternProcessing;
                                                }
                                                else
                                                {
                                                    //default option
                                                    Threshold = (long)VisionAlgorithmes.GetThreshold_ProbeMark(CropImg, MaskingImg, (int)pad.PadColor.Value);
                                                    extraParam = $"{{ \"ThresholdFilter\" : {{\"Params\":[  {{\"Key\":\"Threshold\", \"Value\":\"{Threshold}\"}}]}}}}";
                                                    if (PadCurrentResult.PadStatus == null)
                                                    {
                                                        PadCurrentResult.PadStatus = new ObservableCollection<PadStatusCodeEnum>();
                                                    }
                                                    PadCurrentResult.PadStatus.Add(PadStatusCodeEnum.NEED_REFERENCE_IMAGE);
                                                    //LoggerManager.Error($"[PMIModuleSubRutineStandard], DoPMI() : Ref. image not found {refernceFilePath}"); 매번 검사할때마다 기록하지 않음. PMI 1회수행때만 기록
                                                }
                                            }

                                            LoggerManager.Debug($"[PMIModuleSubRutineStandard], DoPMI() : Recipe execute by {padVPMethod}");
                                            //Step 3. Processing.
                                            (byte[] image, int width, int height) recipeExecuteResult;
                                            using (var section = new SectionLog(timeStamp, stw, padVPMethod.ToString()))
                                            {
                                                recipeExecuteResult = this.VisionManager().VisionLib.RecipeExecute(
                                                    inputImages
                                                    , this.VisionManager().VisionLib.GetReservedRecipe(padVPMethod)
                                                    , extraParam);
                                            }

                                            if (recipeExecuteResult.image == null)
                                            {
                                                //ERROR
                                                this.NotifyManager().Notify(EventCodeEnum.PMI_FAIL);
                                                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.PMI_FAIL);
                                                LoggerManager.Debug($"[PMIModuleSubRutineStandard], DoPMI(), {padVPMethod.ToString()} function failed.");

                                                retval = EventCodeEnum.PMI_FAIL;

                                                return retval;
                                            }
                                            LoggerManager.Debug($"[PMIModuleSubRutineStandard], DoPMI(), RecipeResult ={recipeExecuteResult.width}x{recipeExecuteResult.height} ");
                                            ImageBuffer BinaryImg = new ImageBuffer(recipeExecuteResult.image, recipeExecuteResult.width, recipeExecuteResult.height, (int)ColorDept.BlackAndWhite);

                                            if (PMISysParam.DoPMIDebugImages)
                                            {
                                                SaveImageBufferData.Add(MakeSaveImageBufferData(BinaryImg, $@"{SaveImageBufferDataBasePath}06_BinaryImg_Pad#{padRealIndex}{Save_Extension}", IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE));
                                            }

                                            //try memory release
                                            inputImages.Clear();

                                            #endregion
                                            MaskingAndImg = VisionAlgorithmes.Arithmethic(BinaryImg, MaskingImg, ARITHMETIC_OPERATION_TYPE.M_AND);

                                            if (PMISysParam.DoPMIDebugImages)
                                            {
                                                SaveImageBufferData.Add(MakeSaveImageBufferData(MaskingAndImg, $@"{SaveImageBufferDataBasePath}07_MaskingAndImg_Pad#{padRealIndex}{Save_Extension}", IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE));
                                            }

                                            EdgeRect = VisionAlgorithmes.GetRectInFirstBlob(MaskingAndImg, null, null, true, false);


                                            if (PMISysParam.DoPMIDebugImages)
                                            {
                                                PadEdgeImage = VisionAlgorithmes.GetEdgeImageInFirstBlob(MaskingAndImg, null, null, true, false);

                                                SaveImageBufferData.Add(MakeSaveImageBufferData(PadEdgeImage, $@"{SaveImageBufferDataBasePath}08_PadEdgeImage_Pad#{padRealIndex}{Save_Extension}", IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE));
                                            }

                                            var edgePoints = VisionAlgorithmes.GetChainListInFirstBlob(MaskingAndImg, null, null, true, false);

                                            double initialScaleX = EdgeRect.Width / geo.Bounds.Width;
                                            double initialScaleY = EdgeRect.Height / geo.Bounds.Height;

                                            double stepSize = 3.0;

                                            double minScaleX = initialScaleX - stepSize;
                                            double minScaleY = initialScaleY - stepSize;

                                            double scaleAdjustment = 1.0; // 초기 스케일 조정값
                                            double scaleInterval = 0.02;

                                            double minScaleAdjustmentX = minScaleX / initialScaleX;
                                            double minScaleAdjustmentY = minScaleY / initialScaleY;
                                            double minScaleAdjustment = Math.Min(minScaleAdjustmentX, minScaleAdjustmentY);

                                            minScaleAdjustment = minScaleAdjustment - scaleInterval;

                                            double bestScaleX = initialScaleX;
                                            double bestScaleY = initialScaleY;
                                            double maxMatchingCount = 0;
                                            List<Point> bestAlignedPoints = null;

                                            do
                                            {
                                                scaleX = initialScaleX * scaleAdjustment;
                                                scaleY = initialScaleY * scaleAdjustment;

                                                tg = new TransformGroup();
                                                t1 = new ScaleTransform(scaleX, scaleY);
                                                tg.Children.Add(t1);

                                                offsetx = ((EdgeRect.Left + EdgeRect.Right) / 2) - (geo.Bounds.Width * scaleX / 2);
                                                offsety = ((EdgeRect.Top + EdgeRect.Bottom) / 2) - (geo.Bounds.Height * scaleY / 2);

                                                t2 = new TranslateTransform(offsetx, offsety);
                                                tg.Children.Add(t2);

                                                transfomedGeo = geo.Clone();
                                                transfomedGeo.Transform = tg;

                                                pg = transfomedGeo.GetFlattenedPathGeometry();
                                                pointsOnFlattenedPath = GeometryHelper.GetPointsOnFlattenedPath(pg);

                                                DrawingPadTemplateEdge = VisionAlgorithmes.DrawingPadEdge(MaskingAndImg, pointsOnFlattenedPath, false);
                                                var drawingPadTemplateEdgePoints = VisionAlgorithmes.GetChainListInFirstBlob(DrawingPadTemplateEdge, null, null, true, false);

                                                if (edgePoints.Count > 0 && drawingPadTemplateEdgePoints.Count > 0)
                                                {
                                                    var alignmentResult = ContourAligner.RefineAlignment(edgePoints, drawingPadTemplateEdgePoints, MaskingAndImg.SizeX, MaskingAndImg.SizeY);

                                                    double currentScore = alignmentResult.BestScore;

                                                    if (currentScore > maxMatchingCount)
                                                    {
                                                        maxMatchingCount = currentScore;

                                                        bestScaleX = scaleX;
                                                        bestScaleY = scaleY;

                                                        bestAlignedPoints = alignmentResult.AlignedPoints;

                                                        if (PMISysParam.DoPMIDebugImages)
                                                        {
                                                            SaveImageBufferData.Add(MakeSaveImageBufferData(DrawingPadTemplateEdge, $@"{SaveImageBufferDataBasePath}09_DrawingPadTemplateEdge_Pad#{padRealIndex}_Scale_X{bestScaleX}_Y{bestScaleY}{Save_Extension}", IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE));
                                                        }
                                                    }
                                                }

                                                // 감소 조정 전에 검사
                                                if (scaleAdjustment - 0.02 >= minScaleAdjustment)
                                                {
                                                    scaleAdjustment -= 0.02;

                                                    if (scaleAdjustment <= 0)
                                                    {
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    break; // 루프 종료 조건
                                                }
                                            } while (true);

                                            DrawingPadEdgeImg = VisionAlgorithmes.DrawingPadEdge(MaskingAndImg, bestAlignedPoints);

                                            if (PMISysParam.DoPMIDebugImages)
                                            {
                                                SaveImageBufferData.Add(MakeSaveImageBufferData(DrawingPadEdgeImg, $@"{SaveImageBufferDataBasePath}10_DrawingPadEdgeImg_Pad#{padRealIndex}{Save_Extension}", IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE));
                                            }

                                            // Get Copied Clip Image
                                            ClippedImg = VisionAlgorithmes.CopyClipImage(ClippedImg,
                                                                                        DrawingPadEdgeImg,
                                                                                        (int)CropOffsetX,
                                                                                        (int)CropOffsetY);

                                            if (PMISysParam.DoPMIDebugImages)
                                            {
                                                SaveImageBufferData.Add(MakeSaveImageBufferData(ClippedImg, $@"{SaveImageBufferDataBasePath}11_ClippedImg_Fianl_Pad#{padRealIndex}{Save_Extension}", IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        LoggerManager.Error($"Crop Image Error");

                                        IsStop = true;
                                        break;
                                    }
                                }

                                if (IsStop == true)
                                {
                                    retval = EventCodeEnum.PMI_FAIL;

                                    this.NotifyManager().Notify(EventCodeEnum.PMI_FAIL);
                                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.PMI_FAIL);

                                    break;
                                }

                                BlobParameter MarkBlobParam = new BlobParameter();

                                // 디폴트 기댓값 : | 2um^2 / 0.539 / 0.541 = Math.Truncate(6.8587340834502175933387974581531) => 6 pixel
                                MarkBlobParam.MinBlobArea.Value = (int)Math.Truncate(PMIDevParam.MinMarkNoiseArea.Value / CurCam.GetRatioX() / CurCam.GetRatioY());

                                MarkBlobParam.BlobThreshHold.Value = 0;
                                MarkBlobParam.BlobMinRadius.Value = 0;

                                // 디폴트 기댓값 : | 2um^2 / 0.539 = Math.Round(3.7105751391465677179962894248609, 0) => 4 pixel
                                MarkBlobParam.MIN_FERET_X.Value = Math.Round(PMIDevParam.MinMarkNoiseSizeY.Value / CurCam.GetRatioX(), 0);

                                // 디폴트 기댓값 : | 2um^2 / 0.541 = Math.Round(3.6968576709796672828096118299445, 0) => 4 pixel
                                MarkBlobParam.MIN_FERET_Y.Value = Math.Round(PMIDevParam.MinMarkNoiseSizeY.Value / CurCam.GetRatioY(), 0);

                                MarkBlobParam.MAX_FERET_X.Value = 0;
                                MarkBlobParam.MAX_FERET_Y.Value = 0;

                                MarkBlobParam.BlobLimitedCount.Value = PMIDevParam.MaximumBlobMarkCnt.Value;

                                retval = VisionAlgorithmes.FindMarks(PadTemplist,
                                                                     ClippedImg,
                                                                     CurImg,
                                                                     ref MarkResultImg,
                                                                     PMIImageCombineMode.ORIGINAL,
                                                                     PMIDevParam.MarkAreaCalculateMode.Value,
                                                                     PMIDevParam.MarkOverlapPercentOfToleranceX.Value,
                                                                     PMIDevParam.MarkOverlapPercentOfToleranceY.Value,
                                                                     MarkBlobParam,
                                                                     null,
                                                                     true,
                                                                     true,
                                                                     PMIDevParam.DisplayPadDuringPMI.Value,
                                                                     PMIDevParam.DisplayMarkDuringPMI.Value);

                                if (MarkResultImg != null)
                                {
                                    if ((PMIDevParam.DisplayPadDuringPMI.Value == true) ||
                                        (PMIDevParam.DisplayMarkDuringPMI.Value == true)
                                       )
                                    {
                                        this.VisionManager().DisplayProcessing(CurCam.GetChannelType(), MarkResultImg);
                                    }
                                }
                                else
                                {
                                    LoggerManager.Error($"[PMIModuleSubRutineStandard], DoPMI() : FindMarks function's result Image is null.");
                                }
                            }
                        }
                        else
                        {
                            IsStop = true;

                            if (IsStop == true)
                            {
                                retval = EventCodeEnum.PMI_FAIL;

                                this.NotifyManager().Notify(EventCodeEnum.PMI_FAIL);
                                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.PMI_FAIL);

                                break;
                            }
                        }
                    }
                    else
                    {
                        IsStop = true;

                        if (IsStop == true)
                        {
                            retval = EventCodeEnum.PMI_FAIL;

                            this.NotifyManager().Notify(EventCodeEnum.PMI_FAIL);
                            LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.PMI_FAIL);

                            break;
                        }
                    }

                    // Make Test data
                    //MakeTestData(PadTemplist, PadFindMargin, CurCam);

                    foreach (var pad in PadTemplist)
                    {
                        if (pad.PMIResults != null)
                        {
                            PMIPadResult tmpPad = pad.PMIResults.Last();

                            if (tmpPad != null)
                            {
                                if (tmpPad.UsedWaferPosition == null)
                                {
                                    tmpPad.UsedWaferPosition = new WaferCoordinate();
                                }

                                tmpPad.UsedWaferPosition.X.Value = WaferGroupCen.X.Value;
                                tmpPad.UsedWaferPosition.Y.Value = WaferGroupCen.Y.Value;

                                tmpPad.UsedWaferPosition.Z.Value = CurCam.CamSystemPos.GetZ();

                                if (tmpPad.IsSuccessDetectedPad == true)
                                {
                                    // Pixel로 갖고 있음. 사용상에 주의를 기울일 것.
                                    double DetectedPadOffsetXum = tmpPad.PadOffsetX * CurCam.GetRatioX();
                                    double DetectedPadOffsetYum = tmpPad.PadOffsetY * CurCam.GetRatioY();

                                    LoggerManager.Debug($"[PMIModuleSubRutineStandard], DoPMI() : Pad #{pad.Index.Value + 1}, X Offset = {DetectedPadOffsetXum:F2}, Y Offset = {DetectedPadOffsetYum:F2}, (Unit : um)");

                                    if ((PMIDevParam.MarkAlignTriggerToleranceUsingPadOffsetX.Value <= 0) ||
                                        (PMIDevParam.MarkAlignTriggerToleranceUsingPadOffsetY.Value <= 0))
                                    {
                                        // TODO : 0이하인 경우, 사용 안하는 것으로 간주하자. Side effect 충분히 고려 필요.

                                        //LoggerManager.Debug($"[PMIModuleSubRutineStandard], DoPMI() : Parameter is wrong. Please check the MarkAlignTriggerToleranceUsingPadOffsetX = {PMIDevParam.MarkAlignTriggerToleranceUsingPadOffsetX.Value}, MarkAlignTriggerToleranceUsingPadOffsetY = {PMIDevParam.MarkAlignTriggerToleranceUsingPadOffsetY.Value}");
                                    }
                                    else
                                    {
                                        if ((Math.Abs(DetectedPadOffsetXum) > PMIDevParam.MarkAlignTriggerToleranceUsingPadOffsetX.Value) ||
                                        (Math.Abs(DetectedPadOffsetYum) > PMIDevParam.MarkAlignTriggerToleranceUsingPadOffsetY.Value))
                                        {
                                            // 다음 PMI 진행 시, 마크 얼라인을 수행하기 위해, 트리거 파라미터 True로 변경.
                                            Info.IsTurnOnMarkAlign = true;

                                            LoggerManager.Debug($"[PMIModuleSubRutineStandard], DoPMI() : Mark Align Triggeered. Tolerance X = {PMIDevParam.MarkAlignTriggerToleranceUsingPadOffsetX.Value:F0}, Y = {PMIDevParam.MarkAlignTriggerToleranceUsingPadOffsetY.Value:F0}");
                                        }
                                    }
                                }
                                else
                                {
                                    //Info.IsTurnOnMarkAlign = true;
                                }
                            }
                        }
                    }

                    if (PMIDevParam.SkipProcessing.Value == false)
                    {
                        retval = InspectionDataAnalysis(PadTemplist);
                    }

                    // TODO : Original Image를 저장하는 경로에 대한 컨셉이 정해지면 코드 수정 필요. 현재는 기존 Pass 또는 Fail 이미지를 저장하는 경로를 활용.

                    // 이미지 저장 파라미터가 켜져있는 경우
                    if ((PMIDevParam.LogInfo.PassImageSaveHDDEnable.Value == true) ||
                        (PMIDevParam.LogInfo.FailImageSaveHDDEnable.Value == true) ||
                        (PMIDevParam.LogInfo.OriginalImageSaveHDDEnable.Value == true)
                        )
                    {
                        string NowTime = DateTime.Now.ToString("yyMMddHHmmss");
                        string Day = DateTime.Now.ToString("MMdd");
                        string Time = DateTime.Now.ToString("HHmm");
                        string waferid = this.StageSupervisor().WaferObject.GetSubsInfo().WaferID.Value;
                        string ProberID = this.FileManager().GetProberID();
                        int FoupNum = this.StageSupervisor().WaferObject.GetOriginFoupNumber();

                        string foupID;
                        if (this.GEMModule().GetPIVContainer() != null)
                        {
                            this.GEMModule().GetPIVContainer().UpdateStageLotInfo(FoupNum);
                            foupID = this.GEMModule().GetPIVContainer().CarrierID.Value;
                            if (foupID == "" || foupID == null)
                            {
                                foupID = "FOUP" + FoupNum;
                            }
                        }
                        else
                        {
                            foupID = "FOUP" + FoupNum;
                        }



                        string groupingMode = PMIDevParam.PadGroupingMethod.Value.ToString();

                        if (waferid == null || waferid == string.Empty)
                        {
                            waferid = "Unknown";

                            LoggerManager.Debug($"[PMIModuleSubRutineStandard], DoPMI() : WaferID = {waferid}");
                        }

                        string SaveBasePath = this.FileManager().GetImageSavePath(EnumProberModule.PMI);

                        string SaveImagePath = $"{NowTime}_{waferid}_Dut#{DutNo}_X#{Info.LastPMIDieResult.UI.XIndex}_Y#{Info.LastPMIDieResult.UI.YIndex}_MAP#{WaferMapIndex}_TABLE#{TableIndex}_PMIGroup#{index}_Mode#{groupingMode}{Save_Extension}";
                        string SaveImageFullPath = SaveBasePath + "\\" + SaveImagePath;

                        if (SaveImageFullPath != null && SaveImageFullPath != string.Empty)
                        {
                            if (Directory.Exists(Path.GetDirectoryName(SaveImageFullPath)) == false)
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(SaveImageFullPath));
                            }

                            //if (MakeTestSaveImage == true)
                            //{
                            //    string ImagePath = $"{SaveBasePath}\\TEST\\200729025541_WAFERID1_Dut#747_X#6_Y#35_MAP#0_TABLE#0_PMIGroup#0_Mode#Single_PASS.bmp";

                            //    if ((ImagePath != "") && (ImagePath.Contains(".bmp") == true))
                            //    {
                            //        string RelplaceStr = string.Empty;

                            //        ObservableCollection<PadStatusCodeEnum> status = PadTemplist[0].PMIResults[0].PadStatus;

                            //        if (status.Count == 1)
                            //        {
                            //            if (status[0] == PadStatusCodeEnum.PASS)
                            //            {
                            //                RelplaceStr = "PASS";
                            //            }
                            //            else
                            //            {
                            //                RelplaceStr = "FAIL";
                            //            }
                            //        }
                            //        else
                            //        {
                            //            RelplaceStr = "FAIL";
                            //        }

                            //        SaveImageFullPath = SaveImageFullPath.Replace(".bmp", "_" + RelplaceStr + ".bmp");
                            //        File.Copy(ImagePath, SaveImageFullPath);
                            //    }
                            //}

                            // Save original image
                            if (PMIDevParam.LogInfo.OriginalImageSaveHDDEnable.Value)
                            {
                                string wid = string.Empty;
                                string xin = Info.LastPMIDieResult.UI.XIndex.ToString().PadLeft(3, '0');
                                string yin = Info.LastPMIDieResult.UI.YIndex.ToString().PadLeft(3, '0');

                                if (waferid == "Unknown")
                                {
                                    wid = "NoWaferID";
                                }
                                else
                                {
                                    wid = waferid;
                                }

                                string SaveOriginImagePath = $"{ProberID}_{Day}_{Time}_{foupID}_{wid}_X{xin}Y{yin}_Pad{index + 1}{Save_Extension}";
                                string SaveOrgImageFullPath = SaveBasePath + "\\" + SaveOriginImagePath;

                                ImageBuffer exportImg = new ImageBuffer();
                                //ISSD-3755
                                if (PMIDevParam.OrgImgCropEnable.Value)
                                {
                                    PMIPadObject padTemp = PadTemplist.Where(pad => pad.MachineIndex.XIndex == Info.ProcessedPMIMIndex.Last().XIndex && pad.MachineIndex.YIndex == Info.ProcessedPMIMIndex.Last().YIndex).FirstOrDefault();
                                    exportImg = CropPMIOriginImage(CurCam.GetGrabSizeWidth(), CurCam.GetGrabSizeHeight(), CurCam.GetRatioX(), CurCam.GetRatioY(), CurImg, padTemp);
                                }
                                else
                                {
                                    CurImg.CopyTo(exportImg);
                                }

                                if (this.VisionManager().GetDispHorFlip() == DispFlipEnum.FLIP && this.VisionManager().GetDispVerFlip() == DispFlipEnum.FLIP)
                                {
                                    if (exportImg != null || exportImg.SizeX != 0 || exportImg.SizeY != 0)
                                    {
                                        this.VisionManager().SaveImageBuffer(exportImg, SaveOrgImageFullPath, IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE, exportImg.SizeX / 2, exportImg.SizeY / 2, exportImg.SizeX, exportImg.SizeY, 180);
                                    }
                                    else
                                    {
                                        LoggerManager.Error($"[PMIModuleSubRutineStandard], DoPMI() : ImageBuffer SizeX : {exportImg.SizeX}, ImageBuffer SizeY : {exportImg.SizeY}");
                                    }
                                }
                                else
                                {
                                    this.VisionManager().SaveImageBuffer(exportImg, SaveOrgImageFullPath, IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE);
                                }
                            }

                            // Save Pass or Fail image
                            if (PMIDevParam.SkipProcessing.Value == false)
                            {
                                if (PMIDevParam.LogInfo.PassImageSaveHDDEnable.Value == true || PMIDevParam.LogInfo.FailImageSaveHDDEnable.Value == true)
                                {
                                    try
                                    {
                                        bool isPass = false;
                                        var resultImage = this.VisionManager().VisionProcessing.Algorithmes.GetPMIResultImage(PadTemplist, CurImg, ref isPass);
                                        string savePath = SaveImageFullPath;
                                        string imageResultStatus = isPass ? "PASS" : "FAIL";

                                        bool imageSaveEnable = isPass ? PMIDevParam.LogInfo.PassImageSaveHDDEnable.Value : PMIDevParam.LogInfo.FailImageSaveHDDEnable.Value;

                                        if (imageSaveEnable)
                                        {
                                            string changepath = string.Empty;

                                            if (!string.IsNullOrEmpty(savePath) && savePath.Contains(".bmp"))
                                            {
                                                changepath = savePath.Replace(".bmp", $"_{imageResultStatus}.bmp");
                                                var imageLogType = isPass ? IMAGE_LOG_TYPE.PASS : IMAGE_LOG_TYPE.FAIL;

                                                this.VisionManager().SaveImageBuffer(resultImage, changepath, imageLogType, EventCodeEnum.NONE);

                                                if (PMISysParam.DoPMIDebugImages)
                                                {
                                                    SaveImageBufferData.Add(MakeSaveImageBufferData(resultImage, $@"{SaveImageBufferDataBasePath}12_resultImage{Save_Extension}", IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE));
                                                }

                                                if (imageLogType == IMAGE_LOG_TYPE.FAIL)
                                                {
                                                    changepath = savePath.Replace(".bmp", "_Ori.jpeg");
                                                    this.VisionManager().SaveImageBuffer(CurImg, changepath, imageLogType, EventCodeEnum.NONE);
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception err)
                                    {
                                        retval = EventCodeEnum.PMI_IMAGE_SAVE_ERROR;
                                        LoggerManager.Exception(err);
                                    }
                                }
                            }
                        }
                        else
                        {
                            LoggerManager.Error($"[PMIModuleSubRutineStandard], DoPMI() : Save path is wrong.");
                        }

                        if (PMISysParam.DoPMIDebugImages)
                        {
                            if (SaveImageBufferData != null && SaveImageBufferData.Count > 0)
                            {
                                LoggerManager.Debug($"[PMIModuleSubRutineStandard], DoPMI() : DoPMIDebugImages count = {SaveImageBufferData.Count}");

                                foreach (var item in SaveImageBufferData)
                                {
                                    this.VisionManager().SaveImageBuffer(item.Image, item.Path, item.LogType, item.EventCode);
                                    Thread.Sleep(1);
                                }

                                SaveImageBufferData.Clear();

                            }
                        }
                    }

                    foreach (var p in PadTemplist)
                    {
                        PMIPadObject tmp = new PMIPadObject();

                        p.CopyTo(tmp);

                        AllPadlist.Add(tmp);
                    }

                    retval = EventCodeEnum.NONE;
                }

                if ((retval == EventCodeEnum.NONE) && (AllPadlist.Count > 0))
                {
                    retval = UpdatePMIPadsToDevice(AllPadlist, Info.ProcessedPMIMIndex.Last());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PMIModuleSubRutineStandard] [DoPMI()] : {err}");
                LoggerManager.Exception(err);

            }
            finally
            {

                if (retval != EventCodeEnum.NONE)
                {
                    this.NotifyManager().Notify(retval);
                }

                PMIDieResult tmpresult = Info.LastPMIDieResult;

                if (tmpresult != null)
                {
                    LoggerManager.Debug($"[PMIModuleSubRutineStandard], DoPMI() : Total Pad Count : {tmpresult.PassPadCount + tmpresult.FailPadCount}, Pass : {tmpresult.PassPadCount}, Fail : {tmpresult.FailPadCount}");
                }
                else
                {
                    LoggerManager.Error($"[PMIModuleSubRutineStandard], DoPMI() : Last PMI's result is null. Unknown error.");
                }
                LoggerManager.print_memoryInfo();

                LoggerManager.PMILog($"PMI END");
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.PMI_END);

                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);
            }

            //retval = this.VisionManager().StartGrab(CurCam.GetChannelType());

            return retval;
        }

        public SaveImageBufferData MakeSaveImageBufferData(ImageBuffer image, string path, IMAGE_LOG_TYPE logtype, EventCodeEnum eventcode)
        {
            SaveImageBufferData retval = new SaveImageBufferData();

            try
            {
                // TODO : image의 경우 Copy해서 넣기

                retval.Image = new ImageBuffer();

                if (image != null)
                {
                    image.CopyTo(retval.Image);
                }

                retval.Path = path;
                retval.LogType = logtype;
                retval.EventCode = eventcode;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        #endregion

        #region ==> PMI Condition check functions
        /// <summary>
        /// PMI Wafer Interval을 확인하는 함수
        /// </summary>
        /// <returns></returns>
        //public bool CheckWaferInterval()
        //{
        //    bool retVal = false;
        //    int MapIndex = -1;

        //    try
        //    {
        //        //MapIndex = PMIInfo.PMIMapIndexPerWafer[0];

        //        if (MapIndex < 0 || MapIndex > 0)
        //        {

        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Debug($"[PMIModuleSubRutineStandard] [CheckWaferInterval()] : {err}");
        //        LoggerManager.Exception(err);
        //    }

        //    return retVal;
        //}

        public bool CheckFocusingInterval(int DutNo)
        {
            bool retVal = false;

            try
            {
                if (DutNo == 0)
                {
                    retVal = true;
                }
                else
                {
                    var param = this.PMIModule().PMIModuleDevParam_IParam as PMIModuleDevParam;

                    if (param.FocusingDutInterval.Value > 0)
                    {
                        if ((DutNo % param.FocusingDutInterval.Value) == 0)
                        {
                            retVal = true;
                        }
                        else
                        {
                            retVal = false;
                        }
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PMIModuleSubRutineStandard] [CheckFocusingInterval()] : {err}");
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public bool CheckPMIPadExist()
        {
            bool retVal = false;

            try
            {
                if (this.StageSupervisor().WaferObject.GetSubsInfo().Pads.PMIPadInfos.Count > 0)
                {
                    retVal = true;
                }
                else
                {
                    retVal = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PMIModuleSubRutineStandard] CheckPMIPadExist() : {err}");
                LoggerManager.Exception(err);
            }

            return retVal;
        }



        ///// <summary>
        ///// 마지막 Probing 된 좌표가 포함된 Dut에서 PMI할 다이가 있는지 확인하는 함수
        ///// </summary>
        ///// <returns></returns>
        //public bool CheckPMIDieExist()
        //{
        //    bool retVal = false;

        //    try
        //    {
        //        IProbingModule ProbingModule = this.ProbingModule();
        //        ICoordinateManager CoordinateManager = this.CoordinateManager();
        //        ILotOPModule LotOPModule = this.LotOPModule();

        //        ObservableCollection<IDeviceObject> DutInfo = ProbingModule.ProbingProcessStatus.UnderDutDevs;

        //        //DutInfo.Add(this.GetParam_Wafer().GetSubsInfo().DIEs[35, 33]);

        //        MachineIndex probingMI = ProbingModule.ProbingLastMIndex;

        //        // 해당 다이를 이미 PMI 진행했다면, 또 해서는 안됨.
        //        // this.PMIModule().DoPMIInfo.ProcessedPMIMIndex에 동일한 인덱스 정보가 포함되어 있는지 확인

        //        bool IsAlreadyProcessed = false;

        //        IsAlreadyProcessed = this.PMIModule().DoPMIInfo.ProcessedPMIMIndex.Any(x => x.XIndex == probingMI.XIndex && x.YIndex == probingMI.YIndex);

        //        if (IsAlreadyProcessed == false)
        //        {
        //            //UserIndex probingUI = CoordinateManager.MachineIndexConvertToUserIndex(probingMI);

        //            int curWaferNum = LotOPModule.LotInfo.LoadedWaferCountUntilBeforeLotStart;
        //            //return false;
        //            double dem = curWaferNum / 25;
        //            int wafernumoffset = (int)Math.Ceiling(dem);
        //            int curMapIndex = PMIInfo.WaferTemplateInfo[(curWaferNum - (wafernumoffset * 25)) - 1].SelectedMapIndex.Value;
        //            var PMIMap = PMIInfo.NormalPMIMapTemplateInfo[curMapIndex];

        //            if (DutInfo != null)
        //            {
        //                foreach (var dut in DutInfo)
        //                {
        //                    if (dut.State.Value == DieStateEnum.TESTED)
        //                    {
        //                        if (PMIMap.GetEnable((int)dut.DieIndexM.XIndex, (int)dut.DieIndexM.YIndex) == 0x01)
        //                        {
        //                            retVal = true;
        //                            break;
        //                        }

        //                        //if (PMIMap[dut.DieIndexM.XIndex.Value, dut.DieIndexM.YIndex.Value].PMIEnable.Value == true)
        //                        //{
        //                        //    retVal = true;
        //                        //    break;
        //                        //}
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                if (this.PMIModule().ForcedDone == EnumModuleForcedState.Normal)
        //                    LoggerManager.Debug($"[PMIModuleSubRutineStandard] [CheckPMIDieExist()] : UnderDutInfo is not exist");
        //            }
        //        }
        //        else
        //        {
        //            retVal = false;
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Debug($"[PMIModuleSubRutineStandard] [CheckPMIDieExist()] : {err}");
        //        LoggerManager.Exception(err);
        //    }

        //    return retVal;
        //}

        /// <summary>
        /// 현재 Wafer의 PMI Enable 상태를 확인하는 함수
        /// </summary>
        public bool CheckCurWaferPMIEnable()
        {
            try
            {
                int curWaferNum = this.LotOPModule().LotInfo.LoadedWaferCountUntilBeforeLotStart;
                if (curWaferNum >= 0 & curWaferNum < PMIInfo.WaferTemplateInfo.Count)
                {
                    return PMIInfo.WaferTemplateInfo[curWaferNum - 1].PMIEnable.Value;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PMIModuleSubRutineStandard] [CheckCurWaferPMIEnable()] : {err}");
                return false;
            }
        }

        /// <summary>
        /// 현재 확인하는 함수
        /// </summary>
        public bool CheckCurTouchdownCount()
        {
            bool retval = false;

            try
            {
                long touchdowncount = this.LotOPModule().LotInfo.TouchDownCount;
                if (touchdowncount > 0)
                {
                    if (PMIDevParam.TriggerComponent.TouchdownCountInterval.Value > 0)
                    {
                        if (touchdowncount % PMIDevParam.TriggerComponent.TouchdownCountInterval.Value == 0)
                        {
                            retval = true;
                        }
                    }
                    else
                    {
                        retval = false;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PMIModuleSubRutineStandard] [CheckCurTouchdownCount()] : {err}");
                retval = false;
            }

            return retval;
        }

        /// <summary>
        /// 현재 Wafer의 Interval 확인하는 함수
        /// </summary>
        public bool CheckCurWaferInterval()
        {
            bool retval = false;

            try
            {
                int curWaferNum = this.LotOPModule().LotInfo.LoadedWaferCountUntilBeforeLotStart;

                // TODO : 로직 확인

                //// 나머지가 0이면 해야 됨. 외부에서 트리거 된 경우 해야 됨.
                //if ((PMIDevParam.WaferInterval.Value % curWaferNum) == 0 ||
                //     this.PMIModule().DoPMIInfo.PMITrigger == PMITriggerEnum.WAFER_INTERVAL)
                //{
                //    retval = true;
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PMIModuleSubRutineStandard] [CheckCurWaferPMIEnable()] : {err}");
                return false;
            }

            return retval;
        }
        #endregion

        /// <summary>
        /// 특정 index의 PMI pad로 이동하는 함수
        /// </summary>
        /// <param name="CurCam"></param>
        /// <param name="padIndex"></param>
        /// <returns></returns>
        public EventCodeEnum MoveToPad(ICamera CurCam, MachineIndex mi, int padIndex)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            double xPos, yPos, zPos;

            try
            {
                if ((PadInfos.PMIPadInfos.Count > padIndex) &&
                     (PadInfos.PMIPadInfos[padIndex] != null))
                {
                    DeviceObject curdie = this.StageSupervisor().WaferObject.GetSubsInfo().DIEs[mi.XIndex, mi.YIndex] as DeviceObject;

                    //LLConer = this.WaferAligner().GetLeftCornerPosition(CurCam.CamSystemPos.GetX(), CurCam.CamSystemPos.GetY());
                    WaferCoordinate DieLowLeftCornerPos = new WaferCoordinate();

                    DieLowLeftCornerPos = this.WaferAligner().MachineIndexConvertToDieLeftCorner_NonCalcZ(
                        curdie.DieIndexM.XIndex, curdie.DieIndexM.YIndex);

                    // 전체 패드의 등록 순서 인덱스 기반으로 움직임
                    xPos = PadInfos.PMIPadInfos[padIndex].PadCenter.X.Value + DieLowLeftCornerPos.X.Value;
                    yPos = PadInfos.PMIPadInfos[padIndex].PadCenter.Y.Value + DieLowLeftCornerPos.Y.Value;
                    zPos = this.WaferAligner().GetHeightValue(xPos, yPos);

                    // 만약, 결과 데이터가 존재하는 경우, 그 당시 사용됐던, Z값을 이용해서 움직이자.

                    // 1.현재 다이의 인덱스가 결과 데이터에 포함되어 있다
                    // 2. PMIPadInfos에서 현재 움직이려고 하는 패드의 인덱스가 존재하는지
                    // 3.패드의 결과 데이터가 존재하는지

                    PMIPadResult ExistLastResult = null;
                    bool CanUsedLastResult = false;

                    var resultMachineIndex = this.PMIModule().DoPMIInfo.ProcessedPMIMIndex.ToList().LastOrDefault(x => (x.XIndex == mi.XIndex) && (x.YIndex == mi.YIndex));

                    if (resultMachineIndex != null)
                    {
                        //DeviceObject pmidie = this.StageSupervisor().WaferObject.GetSubsInfo().PMIDIEs.FirstOrDefault(p => p.DieIndexM.XIndex == mi.XIndex && p.DieIndexM.YIndex == mi.YIndex);

                        if (curdie != null)
                        {
                            if (curdie.Pads != null && curdie.Pads.PMIPadInfos.Count > 0 && curdie.Pads.PMIPadInfos[padIndex] != null)
                            {
                                if (curdie.Pads.PMIPadInfos[padIndex].PMIResults != null && curdie.Pads.PMIPadInfos[padIndex].PMIResults.Count > 0)
                                {
                                    ExistLastResult = curdie.Pads.PMIPadInfos[padIndex].PMIResults.LastOrDefault();

                                    if (ExistLastResult != null)
                                    {
                                        if (ExistLastResult.UsedWaferPosition.Z.Value != 0)
                                        {
                                            CanUsedLastResult = true;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (ExistLastResult != null)
                    {
                        if (ExistLastResult.IsSuccessDetectedPad == true)
                        {
                            xPos = xPos + (ExistLastResult.PadOffsetX * CurCam.GetRatioX());
                            yPos = yPos - (ExistLastResult.PadOffsetY * CurCam.GetRatioY());
                        }
                    }

                    if (CanUsedLastResult == false)
                    {
                        zPos = this.WaferAligner().GetHeightValue(xPos, yPos);
                    }
                    else
                    {
                        zPos = ExistLastResult.UsedWaferPosition.GetZ();
                    }



                    if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                    {
                        this.StageSupervisor().StageModuleState.WaferHighViewMove(xPos, yPos, zPos, true);
                    }
                    else if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                    {
                        this.StageSupervisor().StageModuleState.WaferLowViewMove(xPos, yPos, zPos);
                    }
                    else
                    {
                        LoggerManager.Error($"[PMIModuleSubRutineStandard] [MoveToPad()] : Wrong Camera Channel.");
                    }

                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PMIModuleSubRutineStandard] [MoveToPad()] : {err}");
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum MoveToMark(ICamera CurCam, MachineIndex mi, int padIndex, int markIndex)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (this.PMIModule().DoPMIInfo.ProcessedPMIMIndex.Count > 0)
                {
                    DeviceObject curdie = this.StageSupervisor().WaferObject.GetSubsInfo().DIEs[mi.XIndex, mi.YIndex] as DeviceObject;

                    WaferCoordinate DieLowLeftCornerPos = new WaferCoordinate();

                    DieLowLeftCornerPos = this.WaferAligner().MachineIndexConvertToDieLeftCorner(
                        curdie.DieIndexM.XIndex, curdie.DieIndexM.YIndex);

                    WaferCoordinate PadLTWaferCoord;
                    WaferCoordinate PadCenWaferCoord;

                    PMIPadObject pad = curdie.Pads.PMIPadInfos[padIndex];

                    PadCenWaferCoord = new WaferCoordinate((pad.PadCenter.X.Value + DieLowLeftCornerPos.X.Value),
                                                               (pad.PadCenter.Y.Value + DieLowLeftCornerPos.Y.Value));

                    PadLTWaferCoord = new WaferCoordinate(
                        PadCenWaferCoord.GetX() - (pad.PadInfos.SizeX.Value / 2),
                        PadCenWaferCoord.GetY() + (pad.PadInfos.SizeY.Value / 2)
                        );

                    PMIPadResult results = pad.PMIResults.LastOrDefault();

                    PMIMarkResult mark = results.MarkResults[markIndex];

                    WaferCoordinate MarkCenWaferCoord = new WaferCoordinate
                        (
                            PadLTWaferCoord.GetX() + mark.MarkPosUmFromLT.Left + (mark.MarkPosUmFromLT.Width / 2),
                            PadLTWaferCoord.GetY() - mark.MarkPosUmFromLT.Top - (mark.MarkPosUmFromLT.Height / 2)
                        );

                    // 만약, 결과 데이터가 존재하는 경우, 그 당시 사용됐던, Z값을 이용해서 움직이자.

                    // 1.현재 다이의 인덱스가 결과 데이터에 포함되어 있다
                    // 2. PMIPadInfos에서 현재 움직이려고 하는 패드의 인덱스가 존재하는지
                    // 3.패드의 결과 데이터가 존재하는지

                    PMIPadResult ExistLastResult = null;
                    bool CanUsedLastResult = false;

                    var resultMachineIndex = this.PMIModule().DoPMIInfo.ProcessedPMIMIndex.ToList().LastOrDefault(x => (x.XIndex == mi.XIndex) && (x.YIndex == mi.YIndex));

                    if (resultMachineIndex != null)
                    {
                        //DeviceObject pmidie = this.StageSupervisor().WaferObject.GetSubsInfo().PMIDIEs.FirstOrDefault(p => p.DieIndexM.XIndex == mi.XIndex && p.DieIndexM.YIndex == mi.YIndex);

                        if (curdie != null)
                        {
                            if (curdie.Pads != null && curdie.Pads.PMIPadInfos.Count > 0 && curdie.Pads.PMIPadInfos[padIndex] != null)
                            {
                                if (curdie.Pads.PMIPadInfos[padIndex].PMIResults != null && curdie.Pads.PMIPadInfos[padIndex].PMIResults.Count > 0)
                                {
                                    ExistLastResult = curdie.Pads.PMIPadInfos[padIndex].PMIResults.LastOrDefault();

                                    if (ExistLastResult != null)
                                    {
                                        if (ExistLastResult.UsedWaferPosition.Z.Value != 0)
                                        {
                                            CanUsedLastResult = true;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (ExistLastResult != null)
                    {
                        if (ExistLastResult.IsSuccessDetectedPad == true)
                        {
                            //MarkCenWaferCoord.X.Value = MarkCenWaferCoord.X.Value + (ExistLastResult.PadOffsetX * CurCam.GetRatioX());
                            //MarkCenWaferCoord.Y.Value = MarkCenWaferCoord.Y.Value - (ExistLastResult.PadOffsetY * CurCam.GetRatioY());
                        }
                    }

                    if (CanUsedLastResult == false)
                    {
                        MarkCenWaferCoord.Z.Value = this.WaferAligner().GetHeightValue(MarkCenWaferCoord.GetX(), MarkCenWaferCoord.GetY());
                    }
                    else
                    {
                        //if (ExistLastResult.IsSuccessDetectedPad == true)
                        //{
                        //    MarkCenWaferCoord.X.Value = MarkCenWaferCoord.X.Value + (ExistLastResult.PadOffsetX * CurCam.GetRatioX());
                        //    MarkCenWaferCoord.Y.Value = MarkCenWaferCoord.Y.Value - (ExistLastResult.PadOffsetY * CurCam.GetRatioY());
                        //}

                        MarkCenWaferCoord.Z.Value = ExistLastResult.UsedWaferPosition.GetZ();
                    }

                    if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                    {
                        this.StageSupervisor().StageModuleState.WaferHighViewMove(MarkCenWaferCoord.X.Value, MarkCenWaferCoord.Y.Value, MarkCenWaferCoord.Z.Value, true);
                    }
                    else if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                    {
                        this.StageSupervisor().StageModuleState.WaferLowViewMove(MarkCenWaferCoord.X.Value, MarkCenWaferCoord.Y.Value, MarkCenWaferCoord.Z.Value);
                    }
                    else
                    {
                        LoggerManager.Error($"[PMIModuleSubRutineStandard] [MoveToMark()] : Wrong Camera Channel.");
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[MoveToMark] [MoveToMark()] : {err}");
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public ImageBuffer CropPMIOriginImage(double sizex, double sizey, double ratiox, double ratioy, ImageBuffer orgImg, PMIPadObject padTemp)
        {
            ImageBuffer CropImg = null;
            try
            {
                if (orgImg != null)
                {
                    LoggerManager.Debug($"CropPMIOriginImage() OrgImgCropMode is {PMIDevParam.OrgImgCropMode.Value}, OrgImgCropSize is {PMIDevParam.OrgImgCropSize.Value}");
                    if (PMIDevParam.OrgImgCropMode.Value == PMIImageCropMode.AUTO)
                    {
                        double ImgCenPosPixelX = (int)(sizex / 2) - 1;
                        double ImgCenPosPixelY = (int)(sizey / 2) - 1;

                        double offsetX;
                        double offsetY;

                        if (padTemp.PadInfos.SizeX.Value <= 0 || padTemp.PadInfos.SizeY.Value <= 0)
                        {
                            LoggerManager.Debug($"CropPMIOrignImage() PadInfos.SizeX = {padTemp.PadInfos.SizeX.Value}, PadInfos.SizeY = {padTemp.PadInfos.SizeY.Value}");
                            padTemp.PadInfos.SizeX.Value = 50;
                            padTemp.PadInfos.SizeY.Value = 50;
                        }

                        double PadSizePixelX = (padTemp.PadInfos.SizeX.Value / ratiox);
                        double PadSizePixelY = (padTemp.PadInfos.SizeY.Value / ratioy);

                        int CroppedImageWidth = (int)(PadSizePixelX * 3);
                        int CroppedImageHeight = (int)(PadSizePixelY * 3);

                        offsetX = ImgCenPosPixelX - (PadSizePixelX / 2) * 3;
                        offsetY = ImgCenPosPixelY - (PadSizePixelY / 2) * 3;

                        CropImg = this.VisionManager().VisionProcessing.Algorithmes.CropImage(orgImg, (int)offsetX, (int)offsetY, CroppedImageWidth, CroppedImageHeight);
                    }
                    else
                    {
                        double ImgCenPosPixelX = (int)(sizex / 2) - 1;
                        double ImgCenPosPixelY = (int)(sizey / 2) - 1;

                        double offsetX;
                        double offsetY;

                        int CroppedImageWidth = (int)PMIDevParam.OrgImgCropSize.Value;
                        int CroppedImageHeight = (int)PMIDevParam.OrgImgCropSize.Value;
                        if (CroppedImageWidth <= 0 || CroppedImageHeight <= 0)
                        {
                            CroppedImageWidth = 480;
                            CroppedImageHeight = 480;
                        }
                        else if (CroppedImageWidth > 960 || CroppedImageHeight > 960)
                        {
                            CroppedImageWidth = 480;
                            CroppedImageHeight = 480;
                        }

                        offsetX = ImgCenPosPixelX - (PMIDevParam.OrgImgCropSize.Value / 2);
                        offsetY = ImgCenPosPixelY - (PMIDevParam.OrgImgCropSize.Value / 2);

                        CropImg = this.VisionManager().VisionProcessing.Algorithmes.CropImage(orgImg, (int)offsetX, (int)offsetY, CroppedImageWidth, CroppedImageHeight);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return CropImg;
        }

        public EventCodeEnum FindPad(PadTemplate padtemplate)
        {
            EventCodeEnum Retval = EventCodeEnum.UNDEFINED;

            ImageBuffer CurImg = null;
            ImageBuffer CropImg = null;
            ImageBuffer BinaryImg = null;

            long Threshold;

            //this.ViewModelManager().Lock(this.GetHashCode(), "Lock", "Screen Locked");


            try
            {
                var submodule = (SubModule as PNPSetupBase);

                if (submodule != null)
                {
                    submodule.CurCam.GetCurImage(out CurImg);

                    if (CurImg != null)
                    {
                        double ImgCenPosPixelX = (int)(submodule.CurCam.GetGrabSizeWidth() / 2) - 1;
                        double ImgCenPosPixelY = (int)(submodule.CurCam.GetGrabSizeHeight() / 2) - 1;

                        double offsetX;
                        double offsetY;

                        double PadSizePixelX = (padtemplate.SizeX.Value / submodule.CurCam.GetRatioX());
                        double PadSizePixelY = (padtemplate.SizeY.Value / submodule.CurCam.GetRatioY());

                        int CroppedImageWidth = (int)(PadSizePixelX * 2);
                        int CroppedImageHeight = (int)(PadSizePixelY * 2);

                        offsetX = ImgCenPosPixelX - (CroppedImageWidth / 2);
                        offsetY = ImgCenPosPixelY - (CroppedImageHeight / 2);

                        //string TestSavePathFolder = @"C:\ProberSystem\Test\";
                        //string TestSaveCropImage = "CropImage.bmp";
                        //string TestSaveBinaryImage = "BianryImage.bmp";

                        CropImg = this.VisionManager().VisionProcessing.Algorithmes.CropImage(CurImg, (int)offsetX, (int)offsetY, CroppedImageWidth, CroppedImageHeight);

                        if (CropImg != null)
                        {
                            // Debug
                            //this.VisionManager().SaveImageBuffer(CropImg, TestSavePathFolder + TestSaveCropImage, IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE);

                            Threshold = this.VisionManager().VisionProcessing.Algorithmes.GetThreshold_ForPadDetection(CropImg);

                            if (Threshold > 0)
                            {
                                BinaryImg = this.VisionManager().VisionProcessing.Algorithmes.Binarization(CropImg, Threshold);

                                if (BinaryImg != null)
                                {
                                    // TODO : Removed Debugging code
                                    //this.VisionManager().SaveImageBuffer(BinaryImg, TestSavePathFolder + TestSaveBinaryImage, IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE);

                                    BlobParameter blobParameter = new BlobParameter();
                                    ROIParameter roiParameter = new ROIParameter();

                                    // TODO : Check Value
                                    double TolSizePercnet = 0.5;

                                    blobParameter.MinBlobArea.Value = (int)((PadSizePixelX * PadSizePixelY) * (1 - TolSizePercnet));
                                    blobParameter.MaxBlobArea.Value = (PadSizePixelX * PadSizePixelY) * (1 + TolSizePercnet);

                                    blobParameter.MIN_FERET_X.Value = PadSizePixelX * (1 - TolSizePercnet);
                                    blobParameter.MAX_FERET_X.Value = PadSizePixelX * (1 + TolSizePercnet);

                                    blobParameter.MIN_FERET_Y.Value = PadSizePixelY * (1 - TolSizePercnet);
                                    blobParameter.MAX_FERET_Y.Value = PadSizePixelY * (1 + TolSizePercnet);

                                    roiParameter.OffsetX.Value = 0;
                                    roiParameter.OffsetY.Value = 0;
                                    roiParameter.Width.Value = CroppedImageWidth;
                                    roiParameter.OffsetX.Value = CroppedImageHeight;

                                    BlobResult blobResult = this.VisionManager().VisionProcessing.Algorithmes.FindBlobObject(BinaryImg, blobParameter, roiParameter, false, false, true);

                                    if (blobResult != null && blobResult.DevicePositions != null && blobResult.DevicePositions.Count == 1)
                                    {
                                        double DetectedCenPosX = offsetX + blobResult.DevicePositions[0].PosX;
                                        double DetectedCenPosY = offsetY + blobResult.DevicePositions[0].PosY;

                                        double MovingPosPixelX = DetectedCenPosX - ImgCenPosPixelX;
                                        double MovingPosPixelY = ImgCenPosPixelY - DetectedCenPosY;

                                        double MovingPosUmX = MovingPosPixelX * submodule.CurCam.GetRatioX();
                                        double MovingPosUmY = MovingPosPixelY * submodule.CurCam.GetRatioY();

                                        //MachineCoordinate machinecoord = new MachineCoordinate
                                        //    (
                                        //        (CurImg.MachineCoordinates.X.Value + MovingPosUmX),
                                        //        (CurImg.MachineCoordinates.Y.Value + MovingPosUmY)
                                        //    );

                                        WaferCoordinate wafercoord = new WaferCoordinate();

                                        wafercoord.X.Value = submodule.CurCam.CamSystemPos.X.Value + MovingPosUmX;
                                        wafercoord.Y.Value = submodule.CurCam.CamSystemPos.Y.Value + MovingPosUmY;
                                        wafercoord.Z.Value = submodule.CurCam.CamSystemPos.Z.Value;

                                        if (submodule.CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                                        {
                                            //wafercoord = StageSupervisor.CoordinateManager().WaferLowChuckConvert.Convert(machinecoord);
                                            this.StageSupervisor().StageModuleState.WaferLowViewMove(wafercoord.X.Value, wafercoord.Y.Value);
                                        }
                                        else if (submodule.CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                                        {
                                            //wafercoord = StageSupervisor.CoordinateManager().WaferHighChuckConvert.Convert(machinecoord);
                                            this.StageSupervisor().StageModuleState.WaferHighViewMove(wafercoord.X.Value, wafercoord.Y.Value);
                                        }
                                        else
                                        {
                                            LoggerManager.Error($"[PMIModuleSubRutineStandard] Method : FindPad, Camera Type is unknown.");
                                        }

                                        padtemplate.SizeX.Value = blobResult.DevicePositions[0].SizeX * submodule.CurCam.GetRatioX();
                                        padtemplate.SizeY.Value = blobResult.DevicePositions[0].SizeY * submodule.CurCam.GetRatioY();

                                        padtemplate.UpdateArea();

                                        UpdateRenderLayer();
                                    }
                                    else
                                    {

                                    }
                                }
                                else
                                {
                                    LoggerManager.Error($"[PMIModuleSubRutineStandard] Method : FindPad, Binarization fail.");
                                }
                            }
                            else
                            {
                                LoggerManager.Debug($"[PMIModuleSubRutineStandard] Method : FindPad, Threshold Value is zero.");
                            }
                        }
                        else
                        {
                            LoggerManager.Error($"[PMIModuleSubRutineStandard] Method : FindPad, Crop is Failed.");
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            Retval = EventCodeEnum.NONE;



            return Retval;
        }

        public bool IsParameterChanged(bool issave = false)
        {
            return false;
        }

        /// <summary>
        /// 넘겨받은, MI에 해당하는 데이터 중, 첫 Fail이 발생 된 Pad의 인덱스를 얻는 함수.
        /// </summary>
        /// <param name="mi"></param>
        /// <returns></returns>
        private int GetFirstFailedPadIndex(MachineIndex mi)
        {
            int retval = -1;

            try
            {
                //MachineIndex LastMI = this.PMIModule().DoPMIInfo.ProcessedPMIMIndex.ToList().LastOrDefault(x => (x.XIndex == mi.XIndex) && (x.YIndex == mi.YIndex));

                if (mi != null)
                {
                    int WaferMapIndex = -1;

                    int UsedTableIndex = GetTableIndexUsingCurrentWaferIndex(mi, out WaferMapIndex);

                    if (UsedTableIndex > 0)
                    {
                        List<PMIGroupData> GroupInfo = null;

                        if (PMIInfo.PadTableTemplateInfo[UsedTableIndex - 1].GroupingDone.Value == true)
                        {
                            GroupInfo = PMIInfo.PadTableTemplateInfo[UsedTableIndex - 1].Groups.OrderBy(o => o.SeqNum.Value).ToList();

                            DeviceObject pmidie = this.StageSupervisor().WaferObject.GetSubsInfo().DIEs[mi.XIndex, mi.YIndex] as DeviceObject;

                            //DeviceObject pmidie = this.StageSupervisor().WaferObject.GetSubsInfo().PMIDIEs.FirstOrDefault(p => p.DieIndexM.XIndex == mi.XIndex && p.DieIndexM.YIndex == mi.YIndex);

                            if (pmidie != null && pmidie.Pads != null && pmidie.Pads.PMIPadInfos.Count > 0 && GroupInfo != null)
                            {
                                bool FoundFailedPad = false;

                                foreach (var g in GroupInfo.Select((value, i) => new { i, value }))
                                {
                                    var group = g.value;
                                    var index = g.i;

                                    foreach (var item in group.PadDataInGroup)
                                    {
                                        if (pmidie.Pads.PMIPadInfos[item.PadRealIndex.Value].PMIResults != null && pmidie.Pads.PMIPadInfos[item.PadRealIndex.Value].PMIResults.Count > 0)
                                        {
                                            PMIPadResult LastResult = pmidie.Pads.PMIPadInfos[item.PadRealIndex.Value].PMIResults.LastOrDefault();

                                            if (LastResult != null)
                                            {
                                                bool IsPass = LastResult.PadStatus.ToList().Any(x => x == PadStatusCodeEnum.PASS);

                                                if (IsPass == false)
                                                {
                                                    retval = item.PadRealIndex.Value;
                                                    FoundFailedPad = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    if (FoundFailedPad == true)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            LoggerManager.Error($"[PMIModuleSubRutineStandard], GetFirstFailedPadIndex() : Grouping Done flag is not true.");
                        }
                    }
                }
                else
                {
                    retval = -1;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum EnterMovePadPosition(ref PMIPadObject MovedPadInfo)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                var module = SubModule as IUseLightJog;

                if (module.CurCam == null)
                {
                    module.CurCam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);
                }

                ICamera CurCam = module.CurCam;

                this.StageSupervisor().SetWaferMapCam(CurCam.CameraChannel.Type);

                ushort defaultlightvalue = 85;

                for (int lightindex = 0; lightindex < CurCam.LightsChannels.Count; lightindex++)
                {
                    CurCam.SetLight(CurCam.LightsChannels[lightindex].Type.Value, defaultlightvalue);
                }

                double xpos = 0;
                double ypos = 0;
                double zpos = this.WaferAligner().GetHeightValue(xpos, ypos);

                // 등록 된 패드가 존재하는 경우
                if (PadInfos.PMIPadInfos.Count > 0)
                {
                    PMIDrawingGroup drawingGroup = (SubModule as IHasPMIDrawingGroup)?.DrawingGroup;

                    // Center Die로 이동
                    if (drawingGroup != null)
                    {
                        MachineIndex MoveMI = null;
                        int padindex = 0;

                        if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSED)
                        {
                            // 결과 데이터가 존재하는 경우, 현재, 포즈인 상태에서, 이전 처리 된 결과를 보러 들어온 경우.
                            if (this.PMIModule().DoPMIInfo.ProcessedPMIMIndex.Count > 0)
                            {
                                MoveMI = this.PMIModule().DoPMIInfo.ProcessedPMIMIndex.Last();

                                // Fail 패드가 존재하는경우
                                padindex = GetFirstFailedPadIndex(MoveMI);

                                if (padindex < 0)
                                {
                                    padindex = 0;
                                }
                            }
                            else
                            {
                                long cenmx = this.StageSupervisor().WaferObject.GetPhysInfo().CenM.XIndex.Value;
                                long cenmy = this.StageSupervisor().WaferObject.GetPhysInfo().CenM.YIndex.Value;

                                MoveMI = new MachineIndex(cenmx, cenmy);
                                padindex = drawingGroup.SelectedPMIPadIndex;
                            }
                        }
                        else
                        {
                            long cenmx = this.StageSupervisor().WaferObject.GetPhysInfo().CenM.XIndex.Value;
                            long cenmy = this.StageSupervisor().WaferObject.GetPhysInfo().CenM.YIndex.Value;

                            MoveMI = new MachineIndex(cenmx, cenmy);
                            padindex = drawingGroup.SelectedPMIPadIndex;

                            retval = this.MoveToPad(CurCam, MoveMI, drawingGroup.SelectedPMIPadIndex);
                        }

                        if (padindex >= 0)
                        {
                            retval = this.MoveToPad(CurCam, MoveMI, padindex);
                        }
                    }
                }
                else
                {
                    if (this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos != null && this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos.Count > 0)
                    {
                        DUTPadObject firstpadobj = this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos[0];

                        WaferCoordinate wcoord = this.WaferAligner().MachineIndexConvertToDieLeftCorner(firstpadobj.MachineIndex.XIndex, firstpadobj.MachineIndex.YIndex);

                        xpos = wcoord.GetX() + firstpadobj.PadCenter.X.Value;
                        ypos = wcoord.GetY() + firstpadobj.PadCenter.Y.Value;
                        zpos = this.WaferAligner().GetHeightValue(xpos, ypos);
                    }
                    else
                    {
                        if (this.StageSupervisor().WaferObject.GetAlignState() == AlignStateEnum.DONE)
                        {
                            xpos = this.StageSupervisor().WaferObject.GetSubsInfo().WaferCenter.X.Value;
                            ypos = this.StageSupervisor().WaferObject.GetSubsInfo().WaferCenter.Y.Value;
                            zpos = this.WaferAligner().GetHeightValue(xpos, ypos);
                        }
                    }

                    // MOVE
                    retval = this.StageSupervisor().StageModuleState.WaferHighViewMove(xpos, ypos, zpos);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        //public void ChangedPadTemplate(PadTemplate template)
        //{
        //    SelectedPadTemplate = template;
        //}
    }

    public class PMITestDataEachPad
    {
        public PMITestDataEachPad(double ratiox, double ratioy, double padsizex, double padsizey, double padoffsetx, double padoffsety)
        {
            this.RatioX = ratiox;
            this.RatioY = ratioy;

            this.PadSize_X_um = padsizex;
            this.PadSize_Y_um = padsizey;

            this.PadOffset_X_um = padoffsetx;
            this.PadOffset_Y_um = padoffsety;

            MarkDatas = new List<MarkTestData>();
        }

        private int _Index;
        public int Index
        {
            get { return _Index; }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                }
            }
        }

        private double _RatioX;
        public double RatioX
        {
            get { return _RatioX; }
            set
            {
                if (value != _RatioX)
                {
                    _RatioX = value;
                }
            }
        }

        private double _RatioY;
        public double RatioY
        {
            get { return _RatioY; }
            set
            {
                if (value != _RatioY)
                {
                    _RatioY = value;
                }
            }
        }

        private double _PadSize_X_um;
        public double PadSize_X_um
        {
            get { return _PadSize_X_um; }
            set
            {
                if (value != _PadSize_X_um)
                {
                    _PadSize_X_um = value;
                }
            }
        }

        private double _PadSize_Y_um;
        public double PadSize_Y_um
        {
            get { return _PadSize_Y_um; }
            set
            {
                if (value != _PadSize_Y_um)
                {
                    _PadSize_Y_um = value;
                }
            }
        }

        private double _PadOffset_X_um;
        public double PadOffset_X_um
        {
            get { return _PadOffset_X_um; }
            set
            {
                if (value != _PadOffset_X_um)
                {
                    _PadOffset_X_um = value;
                }
            }
        }

        private double _PadOffset_Y_um;
        public double PadOffset_Y_um
        {
            get { return _PadOffset_Y_um; }
            set
            {
                if (value != _PadOffset_Y_um)
                {
                    _PadOffset_Y_um = value;
                }
            }
        }

        private List<MarkTestData> _MarkDatas;
        public List<MarkTestData> MarkDatas
        {
            get { return _MarkDatas; }
            set
            {
                if (value != _MarkDatas)
                {
                    _MarkDatas = value;
                }
            }
        }
    }

    public class MarkTestData
    {
        private double _RatioX;
        public double RatioX
        {
            get { return _RatioX; }
            set
            {
                if (value != _RatioX)
                {
                    _RatioX = value;
                }
            }
        }

        private double _RatioY;
        public double RatioY
        {
            get { return _RatioY; }
            set
            {
                if (value != _RatioY)
                {
                    _RatioY = value;
                }
            }
        }

        public MarkTestData(double ratiox, double ratioy, double marksizex, double marksizey, double scrubcenterx, double scrubcentery)
        {
            this.RatioX = ratiox;
            this.RatioY = ratioy;

            this.SizeX_um = marksizex;
            this.SizeY_um = marksizey;

            this.Scrub_Center_X_um = scrubcenterx;
            this.Scrub_Center_Y_um = scrubcentery;
        }

        private double _SizeX_um;
        public double SizeX_um
        {
            get { return _SizeX_um; }
            set
            {
                if (value != _SizeX_um)
                {
                    _SizeX_um = value;
                }
            }
        }

        private double _SizeY_um;
        public double SizeY_um
        {
            get { return _SizeY_um; }
            set
            {
                if (value != _SizeY_um)
                {
                    _SizeY_um = value;
                }
            }
        }

        private double _Scrub_Center_X_um;
        public double Scrub_Center_X_um
        {
            get { return _Scrub_Center_X_um; }
            set
            {
                if (value != _Scrub_Center_X_um)
                {
                    _Scrub_Center_X_um = value;
                }
            }
        }

        private double _Scrub_Center_Y_um;
        public double Scrub_Center_Y_um
        {
            get { return _Scrub_Center_Y_um; }
            set
            {
                if (value != _Scrub_Center_Y_um)
                {
                    _Scrub_Center_Y_um = value;
                }
            }
        }


    }


    public class SectionLog : IDisposable
    {
        private List<KeyValuePair<string, long>> timeStamp;
        private Stopwatch stw;
        private string v;
        public SectionLog(List<KeyValuePair<string, long>> timeStamp, Stopwatch stw, string v)
        {
            this.timeStamp = timeStamp;
            this.stw = stw;
            this.v = v;
            timeStamp.Add(new KeyValuePair<string, long>($"{v} START", stw.ElapsedMilliseconds));
        }

        public void Dispose()
        {
            timeStamp.Add(new KeyValuePair<string, long>($"{v} END", stw.ElapsedMilliseconds));
        }
    }

    public class AlignmentResult
    {
        public double BestScore { get; set; }
        public double BestX { get; set; }
        public double BestY { get; set; }
        public List<Point> AlignedPoints { get; set; }
    }

    public class SaveImageBufferData
    {
        public ImageBuffer Image { get; set; }
        public string Path { get; set; }
        public IMAGE_LOG_TYPE LogType { get; set; }
        public EventCodeEnum EventCode { get; set; }

        public SaveImageBufferData()
        {
        }

    }

    public class ContourAligner
    {
        //public static List<Point> AlignContours(List<Point> edgePoints, List<Point> templatePoints)
        //{
        //    // Calculate the centroid of both sets of points
        //    Point centroidEdge = CalculateCentroid(edgePoints);
        //    Point centroidTemplate = CalculateCentroid(templatePoints);

        //    // Calculate translation needed
        //    double translateX = centroidEdge.X - centroidTemplate.X;
        //    double translateY = centroidEdge.Y - centroidTemplate.Y;

        //    // Translate template points to align with edge points
        //    //List<Point> alignedPoints = TranslatePoints(templatePoints, Math.Round(translateX), Math.Round(translateY));
        //    List<Point> alignedPoints = TranslatePoints(templatePoints, translateX, translateY);

        //    // Iterate to refine the alignment
        //    AlignmentResult alignmentResult = RefineAlignment(edgePoints, alignedPoints);

        //    return alignmentResult.AlignedPoints;
        //}

        public static AlignmentResult RefineAlignment(List<Point> edgePoints, List<Point> templatePoints, int width, int height)
        {
            double bestScore = CalculateOverlapScore(edgePoints, templatePoints);
            double bestX = 0, bestY = 0;
            List<Point> bestAlignedPoints = new List<Point>(templatePoints);

            // Try adjusting in small steps around the current position
            double stepSize = 3; // Adjust step size based on your accuracy requirements
            for (double dx = -stepSize; dx <= stepSize; dx += 1)
            {
                for (double dy = -stepSize; dy <= stepSize; dy += 1)
                {
                    List<Point> testPoints = TranslatePoints(templatePoints, dx, dy, width, height);

                    if (testPoints.Count == 0)
                    {
                        // Skip if the translated points are out of bounds
                        continue;
                    }

                    double score = CalculateOverlapScore(edgePoints, testPoints);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestX = dx;
                        bestY = dy;
                        bestAlignedPoints = new List<Point>(testPoints);
                    }
                }
            }

            return new AlignmentResult
            {
                BestScore = bestScore,
                BestX = bestX,
                BestY = bestY,
                AlignedPoints = bestAlignedPoints
            };
        }

        public static double CalculateOverlapScore(List<Point> edgePoints, List<Point> testPoints)
        {
            HashSet<Point> edgeSet = new HashSet<Point>(edgePoints);
            int overlapCount = 0;

            foreach (Point p in testPoints)
            {
                if (edgeSet.Contains(p))
                {
                    overlapCount++;
                }
            }

            return overlapCount; // You could normalize this by the number of points if needed
        }

        private static Point CalculateCentroid(List<Point> points)
        {
            if (points.Count == 0)
                return new Point(0, 0);

            double sumX = 0, sumY = 0;

            foreach (Point p in points)
            {
                sumX += p.X;
                sumY += p.Y;
            }

            return new Point(sumX / points.Count, sumY / points.Count);
        }

        private static List<Point> TranslatePoints(List<Point> points, double translateX, double translateY, int width, int height)
        {
            List<Point> translatedPoints = new List<Point>();

            foreach (Point p in points)
            {
                int newX = (int)(p.X + translateX);
                int newY = (int)(p.Y + translateY);

                // Check if the translated point is within the bounds of the image buffer
                if (newX < 0 || newX >= width || newY < 0 || newY >= height)
                {
                    // If any point is out of bounds, return an empty list
                    return new List<Point>();
                }

                translatedPoints.Add(new Point(newX, newY));
            }

            return translatedPoints;
        }

        private static List<Point> TranslatePoints(List<Point> points, double translateX, double translateY)
        {
            List<Point> translatedPoints = new List<Point>();

            foreach (Point p in points)
            {
                int newX = (int)(p.X + translateX);
                int newY = (int)(p.Y + translateY);

                translatedPoints.Add(new Point(newX, newY));
            }

            return translatedPoints;
        }
    }
}
