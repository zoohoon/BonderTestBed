using System;
using System.Collections.Generic;

namespace NeedleCleanViewer
{
    using DXControlBase;
    using LogModule;
    using NeedleCleanerModuleParameter;
    using ProberInterfaces;
    using ProberInterfaces.PinAlign.ProbeCardData;
    using SharpDX.Direct2D1;
    using SharpDXRender;
    using SharpDXRender.RenderObjectPack;
    using SubstrateObjects;
    using System.Windows;
    using WinSize = System.Windows.Size;

    public class NeedleCleanRenderLayer : RenderLayer
    {
        public IProbeCard ProbeCard { get; set; }
        public NeedleCleanDeviceParameter NCDevParam { get; set; }
        public NeedleCleanSystemParameter NCSysParam { get; set; }
        public RenderContainer DutRenderContainer { get; set; }
        public NeedleCleanObject NC { get; set; }
        public WaferObject Wafer { get; set; }

        private double _RatioX;
        private double _RatioY;
        private RenderRectangle rectDut;
        private RenderRectangle rectSheet;
        private double _DutWidth;
        private double _DutHeight;
        //        private int ncNum = 0;
        //private RenderLine line1;
        //private RenderLine line2;
        private RenderText index;

        public NeedleCleanRenderLayer(
            WinSize layerSize,
            IProbeCard probeCard,
            NeedleCleanDeviceParameter ncDevParam,
            NeedleCleanSystemParameter ncSysParam,
            NeedleCleanObject nc,
            WaferObject wafer)
            : base(layerSize)
        {
            try
            {
                NCDevParam = ncDevParam;
                NCSysParam = ncSysParam;
                ProbeCard = probeCard;
                NC = nc;
                Wafer = wafer;
                DutRenderContainer = new RenderContainer(new Rect(0, 0, LayerSize.Width, LayerSize.Height));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void Init()
        {
            try
            {
                double sheetWidth = NC.NCSysParam.SheetDefs[NC.NCSheetVMDef.Index].Range.Value.X.Value * 2;
                double sheetHeight = NC.NCSysParam.SheetDefs[NC.NCSheetVMDef.Index].Range.Value.Y.Value * 2;

                double padWidth = NC.NCSysParam.NeedleCleanPadWidth.Value;
                double padHeight = NC.NCSysParam.NeedleCleanPadHeight.Value;

                //_RatioX = (float)LayerSize.Width / (padWidth + (padWidth * 1 / 10));
                //_RatioY = (float)LayerSize.Height / (padHeight + (padHeight * 1 / 10));

                _RatioX = (float)LayerSize.Width / padWidth;
                _RatioY = (float)LayerSize.Height / padHeight;

                _DutWidth = (float)(Wafer.GetPhysInfo().DieSizeX.Value * _RatioX);
                _DutHeight = (float)(Wafer.GetPhysInfo().DieSizeY.Value * _RatioY);

                rectDut = new RenderRectangle(0, 0, (float)(_DutWidth + LayerCenterPos.X), (float)(_DutHeight + LayerCenterPos.Y), "DutColor");
                rectSheet = new RenderRectangle(0, 0, (float)sheetWidth, (float)sheetHeight, "DutColor");
                //line1 = new RenderLine(new Point(0,0),new Point(0,0), "Black");
                //line2 = new RenderLine(new Point(0, 0), new Point(0, 0), "Black");
                index = new RenderText((NC.NCSheetVMDef.Index).ToString(), 0, 0, (float)(_DutWidth + LayerCenterPos.X), (float)(_DutHeight + LayerCenterPos.Y), "Black");

                MouseDownEventEnable = false;
                MouseWheelEventEnable = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void InitRender(RenderTarget target, ResourceCache resCache)
        {
            try
            {
                rectDut.Draw(target, resCache);
                rectSheet.Draw(target, resCache);
                index.Draw(target, resCache);
                //line1.Draw(target, resCache);
                //line2.Draw(target, resCache);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        protected override void RenderCore(RenderTarget target, ResourceCache resCache)
        {
            try
            {
                //_RatioX = (float)LayerSize.Width / (NC.NCSysParam.SheetDefs[ncNum].Range.Value.X.Value * 2);
                //_RatioY = (float)LayerSize.Height / (NC.NCSysParam.SheetDefs[ncNum].Range.Value.Y.Value * 2);            

                //NC = this.StageSupervisor().NCObject as NeedleCleanObject;

                //NC = (NeedleCleanObject)this.StageSupervisor().NCObject;
                //NCDevParam = (NeedleCleanDeviceParameter)this.StageSupervisor().NeedleCleaner().NeedleCleanDeviceParameter_IParam;
                //NCSysParam = (NeedleCleanSystemParameter)this.StageSupervisor().NeedleCleaner().NeedleCleanSysParam_IParam;
                //_DutWidth = (float)(this.StageSupervisor().WaferObject.GetPhysInfo().DieSizeX.Value * _RatioX);
                //_DutHeight = (float)(this.StageSupervisor().WaferObject.GetPhysInfo().DieSizeY.Value * _RatioY);

                float marginX = 0;
                float marginY = 0;
                //marginX = ((float)(LayerSize.Width / 2) - (float)(NC.NCSysParam.SheetDefs[NC.NCSheetVMDef.Index].Range.Value.X.Value * _RatioX)) / 2;
                //marginY = ((float)(LayerSize.Height / 2) - (float)(NC.NCSysParam.SheetDefs[NC.NCSheetVMDef.Index].Range.Value.Y.Value * _RatioY)) / 2;

                rectSheet = new RenderRectangle((float)(marginX + (LayerSize.Width / 2) - (double)(NC.NCSysParam.SheetDefs[NC.NCSheetVMDef.Index].Range.Value.X.Value * _RatioX)),
                    marginY + (float)((LayerSize.Height / 2) - (double)(NC.NCSysParam.SheetDefs[NC.NCSheetVMDef.Index].Range.Value.Y.Value * _RatioY)),
                    (float)(NC.NCSysParam.SheetDefs[NC.NCSheetVMDef.Index].Range.Value.X.Value * 2 * _RatioX),
                    (float)(NC.NCSysParam.SheetDefs[NC.NCSheetVMDef.Index].Range.Value.Y.Value * 2 * _RatioY), "Gray");
                rectSheet.Draw(target, resCache);
                index = new RenderText("Clean Pad " + (NC.NCSheetVMDef.Index + 1).ToString(),
                    (float)((LayerSize.Width / 2) - (double)(NC.NCSysParam.SheetDefs[NC.NCSheetVMDef.Index].Range.Value.X.Value * _RatioX))
                    + (float)(NC.NCSysParam.SheetDefs[NC.NCSheetVMDef.Index].Range.Value.X.Value * _RatioX) - 50,
                    50, 150, 50, "White", 20);

                //rectSheet = new RenderRectangle((float)((680 / 2) - (double)(NC.NCSysParam.SheetDefs[NC.NCSheetVMDef.Index].Range.Value.X.Value * _RatioX)),
                //    (float)((544 / 2) - (double)(NC.NCSysParam.SheetDefs[NC.NCSheetVMDef.Index].Range.Value.Y.Value * _RatioY)),
                //    (float)(NC.NCSysParam.SheetDefs[NC.NCSheetVMDef.Index].Range.Value.X.Value * 2 * _RatioX),
                //    (float)(NC.NCSysParam.SheetDefs[NC.NCSheetVMDef.Index].Range.Value.Y.Value * 2 * _RatioY), "Gray");
                //rectSheet.Draw(target, resCache);
                //index = new RenderText("Clean Pad " + (NC.NCSheetVMDef.Index + 1).ToString(),
                //    (float)((680 / 2) - (double)(NC.NCSysParam.SheetDefs[NC.NCSheetVMDef.Index].Range.Value.X.Value * _RatioX))
                //    + (float)(NC.NCSysParam.SheetDefs[NC.NCSheetVMDef.Index].Range.Value.X.Value * _RatioX) - 50,
                //    50, 150, 50, "White", 20);
                index.Draw(target, resCache);
                //ZoomSetting(1, 10, 1);

                //line1 = new RenderLine(new Point(0, LayerCenterPos.Y), new Point(LayerCenterPos.X * 2, LayerCenterPos.Y), "Black");
                //line1.Draw(target, resCache);
                //line2 = new RenderLine(new Point(LayerCenterPos.X, 0), new Point(LayerCenterPos.X, LayerCenterPos.Y * 2), "Black");
                //line2.Draw(target, resCache);

                if (NCDevParam.SheetDevs[NC.NCSheetVMDef.Index].CleaningCount.Value < 1)
                    throw new Exception("Cleaning count 0");

                if (NC.NCSheetVMDefs[NC.NCSheetVMDef.Index].CurCleaningLoc == null)
                    return;


                double ncPosX = NC.NCSheetVMDefs[NC.NCSheetVMDef.Index].CurCleaningLoc.X.Value;
                double ncPosY = NC.NCSheetVMDefs[NC.NCSheetVMDef.Index].CurCleaningLoc.Y.Value;
                //ncPosX = 700;
                //ncPosY = -700;
                ncPosX = ncPosX * _RatioX;
                ncPosY = ncPosY * _RatioY * -1;

                double xPos = ncPosX;
                double yPos = ncPosY;

                double dutDistanceX = 0; //NCDevParam.SheetDevs[ncNum].CleaningDistance.Value * _RatioX;
                double dutDistanceY = 0; //NCDevParam.SheetDevs[ncNum].CleaningDistance.Value * _RatioY;

                switch (NCDevParam.SheetDevs[NC.NCSheetVMDef.Index].CleaningDirection.Value)
                {
                    case NC_CleaningDirection.LEFT:
                        xPos = xPos - dutDistanceX;
                        break;
                    case NC_CleaningDirection.RIGHT:
                        xPos = xPos + dutDistanceX;
                        break;
                    case NC_CleaningDirection.TOP:
                        yPos = yPos - dutDistanceY;
                        break;
                    case NC_CleaningDirection.BOTTOM:
                        yPos = yPos + dutDistanceY;
                        break;
                    case NC_CleaningDirection.TOP_LEFT:
                        xPos = xPos - dutDistanceX;
                        yPos = yPos - dutDistanceY;
                        break;
                    case NC_CleaningDirection.TOP_RIGHT:
                        xPos = xPos + dutDistanceX;
                        yPos = yPos - dutDistanceY;
                        break;
                    case NC_CleaningDirection.BOTTOM_LEFT:
                        xPos = xPos - dutDistanceX;
                        yPos = yPos + dutDistanceY;
                        break;
                    case NC_CleaningDirection.BOTTOM_RIGHT:
                        xPos = xPos + dutDistanceX;
                        yPos = yPos + dutDistanceY;
                        break;
                    case NC_CleaningDirection.HOLD:
                        break;
                }
                // Test code
                //ProbeCard.XSize.Value = 4;
                //ProbeCard.YSize.Value = 4;

                int cenIndexX = (int)Math.Truncate((double)(ProbeCard.ProbeCardDevObjectRef.DutIndexSizeX / 2.0));
                int cenIndexY = (int)Math.Truncate((double)(ProbeCard.ProbeCardDevObjectRef.DutIndexSizeY / 2.0));
                float dutLeft = 0f;
                float dutTop = 0f;
                float shiftOffsetX = 0;
                float shiftOffsetY = 0;

                if (ProbeCard.ProbeCardDevObjectRef.DutIndexSizeX % 2 != 0)
                {
                    // Â¦¼ö
                    shiftOffsetX = (float)_DutWidth / 2;
                }
                if (ProbeCard.ProbeCardDevObjectRef.DutIndexSizeY % 2 != 0)
                {
                    // Â¦¼ö
                    shiftOffsetY = (float)_DutHeight / 2;
                }

                foreach (IDut displayDut in ProbeCard.ProbeCardDevObjectRef.DutList)
                {
                    dutLeft = (float)(((displayDut.MacIndex.XIndex - cenIndexX) * _DutWidth) + xPos) - shiftOffsetX;
                    dutTop = (float)(yPos - ((displayDut.MacIndex.YIndex - cenIndexY + 1) * _DutHeight)) + shiftOffsetY;

                    //if (ProbeCard.DutList.Count % 2 == 0)
                    //{
                    //    dutLeft = (float)(((displayDut.MacIndex.XIndex - cenIndexX) * _DutWidth) + xPos);
                    //    dutTop = (float)(yPos - ((displayDut.MacIndex.YIndex - cenIndexY + 1) * _DutHeight));
                    //}
                    //else
                    //{
                    //    dutLeft = (float)(((displayDut.MacIndex.XIndex - cenIndexX - 0.5) * _DutWidth) + xPos);
                    //    dutTop = (float)(yPos - ((displayDut.MacIndex.YIndex - cenIndexY + 0.5) * _DutHeight));
                    //}
                    rectDut = new RenderRectangle((float)(dutLeft + (LayerSize.Width / 2)), (float)(dutTop + (LayerSize.Height / 2)), (float)_DutWidth, (float)_DutHeight, "DutColor");
                    //rectDut = new RenderRectangle((float)(dutLeft), (float)(dutTop ), (float)_DutWidth, (float)_DutHeight, "DutColor");
                    rectDut.Draw(target, resCache, 0);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        protected override void RenderHudCore(RenderTarget target, ResourceCache resCache)
        {
        }

        public override List<RenderContainer> GetRenderContainers()
        {
            List<RenderContainer> containers = new List<RenderContainer>();
            try
            {
                containers.Add(DutRenderContainer);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return containers;
        }
    }
}
